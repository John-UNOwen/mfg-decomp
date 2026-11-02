using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;

namespace WebFront
{
    // Browser front for the WebGL rebuild (Gate 3/4 support piece).
    // Raw TcpListener (same bind model as the repo mock, no http.sys/urlacl dependencies).
    // Serves the Unity WebGL export + content trees (incl. addressables bundles) from one origin,
    // and proxies /aog/* to the provider, rewriting the User-Agent (the provider's AOG
    // middleware refuses browser UAs) and adding CORS headers for dev clients.
    // Same-origin design means the built game needs no CORS at runtime; COOP/COEP headers support
    // Unity's SharedArrayBuffer threading mode.
    internal static class Program
    {
        private static readonly string Upstream = Environment.GetEnvironmentVariable("WEBFRONT_UPSTREAM") ?? "http:// <provider-host>";
        private static readonly string Root = Path.GetFullPath(Environment.GetEnvironmentVariable("WEBFRONT_ROOT") ?? Path.Combine(AppContext.BaseDirectory, "webroot"));
        private static readonly int Port = int.TryParse(Environment.GetEnvironmentVariable("WEBFRONT_PORT"), out int p) ? p : 8090;
        // The provider only excludes UAs containing "Mozilla"; any neutral agent works.
        private static readonly string RelayUA = "MFG-WebFront/1.0 (+https://github.com/John-UNOwen/mfg-decomp)";

        // Hosted mode (docs/28 web identity & hosting plan). WEBFRONT_GATE=1 makes
        // this front the single authenticated edge: game pages and /aog traffic only
        // reach the origin behind a live web session (connect.sid checked at the
        // core's /api/me), every page gets the session's DataID spliced into
        // config.arguments, signup/login/logout are relayed to the core with the
        // session cookie carried both ways, and a relayed /aog login is preceded by
        // presenting that account's own DataID as its card credential (plugin
        // /vfgcard_present, WEBFRONT_PRESENTER_SECRET). WEBFRONT_GATE unset/0 is the
        // LAN dev relay: today's unauthenticated behaviour, unchanged.
        private static readonly bool Gate = Environment.GetEnvironmentVariable("WEBFRONT_GATE") == "1";
        private static readonly string AuthBaseEnv = Environment.GetEnvironmentVariable("WEBFRONT_AUTH");
        private static readonly string AuthBase = AuthBaseEnv ?? (Environment.GetEnvironmentVariable("WEBFRONT_UPSTREAM") ?? "http:// <provider-host>");
        // WEBFRONT_TRUST_PROXY=cloudflare makes CF-Connecting-IP the visitor identity
        // (behind the tunnel that is the only honest source ip). Without it the gate
        // keys visitors on the real socket peer - incoming XFF/CF headers from a direct
        // client are NOT trusted, and whatever reaches the core is the gate's own value.
        private static readonly bool TrustCloudflare = Environment.GetEnvironmentVariable("WEBFRONT_TRUST_PROXY") == "cloudflare";
        // The -FRONTURL the spliced page plays against; empty-host "/aog" keeps the
        // game same-origin with the page (behind the tunnel/https front).
        private static readonly string PageFrontUrl = Environment.GetEnvironmentVariable("WEBFRONT_PAGE_FRONTURL") ?? "/aog";
        private static readonly string PresenterSecret = ResolvePresenterSecret();
        private static string ResolvePresenterSecret()
        {
            var s = Environment.GetEnvironmentVariable("WEBFRONT_PRESENTER_SECRET");
            if (!string.IsNullOrEmpty(s)) return s;
            var f = Environment.GetEnvironmentVariable("WEBFRONT_PRESENTER_SECRET_FILE");
            try { if (!string.IsNullOrEmpty(f) && File.Exists(f)) return File.ReadAllText(f).Trim(); } catch { }
            return "";
        }

        // Access-code lobby (owner requirement, 2026-11-01): before the login/signup
        // feature is shown at all, visitors pass one shared passphrase - no username,
        // just the code to gain access to the feature. WEBFRONT_ACCESS_CODE (or
        // WEBFRONT_ACCESS_CODE_FILE); unset = lobby off, today's behavior unchanged.
        private static readonly string AccessCode = ResolveAccessCode();
        private static bool LobbyEnabled => Gate && !string.IsNullOrEmpty(AccessCode);
        private static string ResolveAccessCode()
        {
            var s = Environment.GetEnvironmentVariable("WEBFRONT_ACCESS_CODE");
            if (!string.IsNullOrEmpty(s)) return s.Trim();
            var f = Environment.GetEnvironmentVariable("WEBFRONT_ACCESS_CODE_FILE");
            try { if (!string.IsNullOrEmpty(f) && File.Exists(f)) return File.ReadAllText(f).Trim(); } catch { }
            return "";
        }

        // Session cache: session id -> who it answers to (keyed on the sid token, not
        // the whole Cookie header, so junk cookie pairs cannot mint cache entries).
        // The core is loopback, but the game posts keep_alive/gget every second, so
        // /api/me is not re-asked for every one of those; 30 s is short enough for a
        // logout to catch up. Bounded: 5000 entries, expired ones swept on insert.
        private enum SessionKind { Ok, Anonymous, UpstreamError }
        private sealed class SessionInfo { public string Username; public string Card; public bool Admin; }
        private sealed class SessionEntry { public SessionKind Kind; public SessionInfo Info; public long UntilMs; }
        private const int MaxSessions = 5000;
        private const long SessionTtlMs = 30000, NegativeTtlMs = 10000;
        private static readonly Dictionary<string, SessionEntry> Sessions = new Dictionary<string, SessionEntry>();
        private static readonly object SessionsLock = new object();

        // Per-visitor request window at the gate (docs/28 checkup item: the public
        // edge needs its own throttle in front of the origin's per-IP limiters).
        // 600/min: above the game's own steady-state traffic (keep_alive is 1/s),
        // well below what a junk-cookie flood would need.
        private const int VisitorWindowMs = 60000, VisitorMaxPerWindow = 600, MaxVisitorEntries = 20000;
        private static readonly Dictionary<string, (long WindowStart, int Count)> VisitorHits = new Dictionary<string, (long, int)>();
        private static readonly Dictionary<string, int> VisitorConns = new Dictionary<string, int>();
        private static readonly object VisitorLock = new object();
        private static bool VisitorAllowed(string ip, long now)
        {
            lock (VisitorLock)
            {
                if (VisitorHits.Count >= MaxVisitorEntries)
                {
                    foreach (string k in new List<string>(VisitorHits.Keys))
                        if (VisitorHits[k].WindowStart + VisitorWindowMs < now) VisitorHits.Remove(k);
                    if (VisitorHits.Count >= MaxVisitorEntries) VisitorHits.Clear(); // full of live keys - reset the window
                }
                if (!VisitorHits.TryGetValue(ip, out var e) || now - e.WindowStart >= VisitorWindowMs)
                {
                    VisitorHits[ip] = (now, 1);
                    return true;
                }
                if (e.Count >= VisitorMaxPerWindow) return false;
                VisitorHits[ip] = (e.WindowStart, e.Count + 1);
                return true;
            }
        }

        // Per-visitor CONCURRENT-connection cap (review round 2 critical #3):
        // Admission bounds the total, but without a per-source cap one visitor's
        // stalled sockets could occupy all 128 slots and starve every other
        // visitor out of the gate. A browser's own parallelism stays far below 32.
        private const int MaxVisitorConns = 32;
        private static bool VisitorConnAcquire(string ip)
        {
            lock (VisitorLock)
            {
                if (VisitorConns.TryGetValue(ip, out int n))
                {
                    if (n >= MaxVisitorConns) return false;
                    VisitorConns[ip] = n + 1;
                    return true;
                }
                if (VisitorConns.Count >= MaxVisitorEntries) VisitorConns.Clear();
                VisitorConns[ip] = 1;
                return true;
            }
        }
        private static void VisitorConnRelease(string ip)
        {
            if (string.IsNullOrEmpty(ip)) return;
            lock (VisitorLock)
            {
                if (VisitorConns.TryGetValue(ip, out int n))
                {
                    if (n <= 1) VisitorConns.Remove(ip);
                    else VisitorConns[ip] = n - 1;
                }
            }
        }

        // Lobby unlock tokens: the access-code check mints a random 128-bit token
        // (24 h) kept here; the browser holds it in an HttpOnly cookie. No plaintext
        // code ever reaches the client side after the one unlock POST.
        private const int MaxLobbyTokens = 5000;
        private const long LobbyTtlMs = 86400000;
        private static readonly Dictionary<string, long> LobbyTokens = new Dictionary<string, long>();
        private static readonly object LobbyLock = new object();
        // The unlock attempt itself is throttled hard: 10 codes/min per visitor.
        private const int LobbyMaxPerWindow = 10;
        private static readonly Dictionary<string, (long WindowStart, int Count)> LobbyHits = new Dictionary<string, (long, int)>();
        // A short shared code needs more than a per-minute window: consecutive wrong
        // codes stack a per-visitor timeout (5 wrong = 1 min out, 10 = 5 min, 15+ = 30 min).
        // Attempts during a lockout are refused with 429 + Retry-After and never extend it,
        // so a locked visitor cannot self-perpetuate the block; the correct code wipes the
        // failure history (and the window quota stays as the baseline limiter).
        private static readonly Dictionary<string, (int Fails, long Until)> LobbyLockouts = new Dictionary<string, (int, long)>();
        private static bool LobbyLockedOut(string ip, long now, out long retryAfterSec)
        {
            retryAfterSec = 0;
            lock (VisitorLock)
            {
                if (!LobbyLockouts.TryGetValue(ip, out var e)) return false;
                // Until == 0 means "failures counted, no penalty active yet": keep the history.
                if (e.Until > 0 && e.Until <= now) { LobbyLockouts.Remove(ip); return false; }
                if (e.Until <= now) return false;
                retryAfterSec = (e.Until - now + 999) / 1000;
                return true;
            }
        }
        private static long LobbyFailPenaltyMs(int fails)
        {
            if (fails >= 15) return 1800000;
            if (fails >= 10) return 300000;
            if (fails >= 5) return 60000;
            return 0;
        }
        private static void LobbyRecordFail(string ip, long now)
        {
            lock (VisitorLock)
            {
                if (LobbyLockouts.Count >= MaxVisitorEntries) LobbyLockouts.Clear();
                int fails = 1;
                long until = 0;
                if (LobbyLockouts.TryGetValue(ip, out var e)) { fails = e.Fails + 1; until = e.Until; }
                long penalty = LobbyFailPenaltyMs(fails);
                if (penalty > 0) until = Math.Max(until, now + penalty);
                LobbyLockouts[ip] = (fails, until);
                Console.WriteLine("webfront lobby: wrong code from " + ip + " fails=" + fails + (penalty > 0 ? " locked " + (until - now) / 1000 + "s" : ""));
            }
        }
        private static void LobbyClearFails(string ip)
        {
            lock (VisitorLock) { LobbyLockouts.Remove(ip); }
        }
        private static bool LobbyAttemptAllowed(string ip, long now)
        {
            lock (VisitorLock)
            {
                if (LobbyHits.Count >= MaxVisitorEntries)
                {
                    foreach (string k in new List<string>(LobbyHits.Keys))
                        if (LobbyHits[k].WindowStart + VisitorWindowMs < now) LobbyHits.Remove(k);
                    if (LobbyHits.Count >= MaxVisitorEntries) LobbyHits.Clear();
                }
                if (!LobbyHits.TryGetValue(ip, out var e) || now - e.WindowStart >= VisitorWindowMs)
                {
                    LobbyHits[ip] = (now, 1);
                    return true;
                }
                if (e.Count >= LobbyMaxPerWindow) return false;
                LobbyHits[ip] = (e.WindowStart, e.Count + 1);
                return true;
            }
        }
        private static string CookieValueOf(string cookieHeader, string name)
        {
            if (string.IsNullOrEmpty(cookieHeader)) return null;
            foreach (string part in cookieHeader.Split(';'))
            {
                string p = part.Trim();
                if (p.StartsWith(name + "=", StringComparison.Ordinal)) return p.Substring(name.Length + 1);
            }
            return null;
        }
        private static bool LobbyOk(string cookieHeader)
        {
            string tok = CookieValueOf(cookieHeader, "mfg_lobby");
            if (string.IsNullOrEmpty(tok) || tok.Length != 32) return false;
            lock (LobbyLock)
            {
                if (!LobbyTokens.TryGetValue(tok, out long until)) return false;
                if (until < Environment.TickCount64) { LobbyTokens.Remove(tok); return false; }
                return true;
            }
        }
        private static string NewLobbyToken()
        {
            lock (LobbyLock)
            {
                if (LobbyTokens.Count >= MaxLobbyTokens)
                {
                    foreach (string k in new List<string>(LobbyTokens.Keys))
                        if (LobbyTokens[k] < Environment.TickCount64) LobbyTokens.Remove(k);
                    if (LobbyTokens.Count >= MaxLobbyTokens) LobbyTokens.Clear();
                }
                string t = Convert.ToHexString(RandomNumberGenerator.GetBytes(16));
                LobbyTokens[t] = Environment.TickCount64 + LobbyTtlMs;
                return t;
            }
        }
        // Compare digests, not the strings: no length or byte timing channel.
        private static bool CodeMatches(string given)
        {
            byte[] a = SHA256.HashData(Encoding.UTF8.GetBytes(given));
            byte[] b = SHA256.HashData(Encoding.UTF8.GetBytes(AccessCode));
            return CryptographicOperations.FixedTimeEquals(a, b);
        }
        private static string SafeNext(string n)
        {
            if (string.IsNullOrEmpty(n)) return "/";
            if (n.Length > 200) return "/";
            if (!n.StartsWith("/") || n.StartsWith("//")) return "/";
            foreach (char c in n) if (c < 0x20 || c == '\\' || c == '@') return "/";
            return n;
        }
        private static string LobbyPage(string next, string error)
        {
            string err = error == null ? "" : "<p class=e>" + WebUtility.HtmlEncode(error) + "</p>";
            return "<!DOCTYPE html><html><head><meta charset=\"utf-8\"><meta name=\"viewport\" content=\"width=device-width,initial-scale=1\">" +
                "<title>Private hosting - access code required</title><style>" +
                "body{font-family:system-ui,sans-serif;background:#10151c;color:#e6ecf3;display:flex;align-items:center;justify-content:center;min-height:100vh;margin:0}" +
                "form{background:#1a2230;border:1px solid #2c3a4e;border-radius:10px;padding:28px 32px;width:min(92vw,380px)}" +
                "h1{font-size:18px;margin:0 0 6px}p.sub{color:#9fb0c3;font-size:13px;margin:0 0 18px}" +
                "input{width:100%;padding:10px 12px;border-radius:6px;border:1px solid #3a4a60;background:#0f1620;color:#e6ecf3;font-size:15px;box-sizing:border-box}" +
                "button{margin-top:14px;width:100%;padding:10px;border:0;border-radius:6px;background:#3b82f6;color:#fff;font-size:15px;cursor:pointer}" +
                ".e{color:#f87171;font-size:13px;margin:10px 0 0}</style></head><body>" +
                "<form method=\"post\" action=\"/welcome\"><h1>Private hosting</h1>" +
                "<p class=sub>Enter the access code to unlock sign-up and sign-in.</p>" + err +
                "<input type=\"password\" name=\"code\" autofocus autocomplete=\"current-password\" placeholder=\"Access code\">" +
                "<input type=\"hidden\" name=\"next\" value=\"" + WebUtility.HtmlEncode(next ?? "/") + "\">" +
                "<button type=\"submit\">Unlock</button></form></body></html>";
        }

        // Latin1 (28591): char 0..255 encodes to exactly one byte, so request and
        // response bodies are staged as latin1-mapped strings and go out byte-exact.
        private static readonly Encoding Latin1 = Encoding.GetEncoding(28591);
        // UseCookies=false on every client: the gate relays cookies explicitly per
        // visitor. A shared cookie jar would bleed the core's connect.sid between
        // visitors (checkup critical #1).
        private static readonly HttpClient Up = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false, UseCookies = false }) { Timeout = TimeSpan.FromSeconds(30) };
        private static readonly HttpClient Auth = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false, UseCookies = false }) { Timeout = TimeSpan.FromSeconds(8) };
        private const int MaxBodyBytes = 262144; // gate-side body cap (413 beyond it)

        // Core-side AogMiddleware treats ANY path whose last segment names a known
        // AOG endpoint as a game call (non-Mozilla UA). At the gate that must only
        // happen under /aog/, else POST /signup/keep_alive style paths would slip
        // past the webui classification (checkup major #10).
        private static readonly HashSet<string> KnownAogNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "appli_boot", "appli_info", "login", "logout", "create_player", "get_menudata", "keep_alive",
            "client_state_read", "client_state_write", "entry_game", "gget", "gpost", "end_game", "kiken_game", "end_show", "reconnect",
            "chk_tabooword", "dojo_get_status", "dojo_set_slot", "dojo_gain_soul", "gacha_info", "gacha_log", "req_draw_gacha", "get_gacha_result",
            "music_gacha_play", "music_gacha_play_reserve", "gchat", "gget_stamp_info", "player_record", "get_record", "get_haifu_list", "get_haifu_data",
            "get_jongstone_info", "get_mg", "mission_date", "present_done", "competition_entry", "item_gain_log", "item_consume_log", "notice_done", "important_notice_done", "set_favorite_character", "odekake_done", "coop_done", "eashop_done",
            "vfgcard_present"
        };

        private static int Main()
        {
            Directory.CreateDirectory(Root);
            // WEBFRONT_BIND=any serves on all interfaces (guest VM browser play); default loopback.
            var bindEnv = Environment.GetEnvironmentVariable("WEBFRONT_BIND");
            var bind = bindEnv == "any" ? IPAddress.Any : IPAddress.Loopback;
            if (Gate && string.IsNullOrEmpty(AuthBaseEnv))
            {
                // A gate started for hosting must know its own identity origin. Falling
                // back to a public default would mail every visitor's cookie + login
                // password to someone else's host.
                Console.Error.WriteLine("WEBFRONT_GATE=1 requires WEBFRONT_AUTH to be set explicitly (the core webui origin). Refusing to start.");
                return 1;
            }
            if (Gate && string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBFRONT_UPSTREAM")))
            {
                // Same refusal for the GAME origin: /aog relays carry card presentations
                // built from the presenter secret; the public default must never be used
                // on a hosted lane (review round 2 minor #14).
                Console.Error.WriteLine("WEBFRONT_GATE=1 requires WEBFRONT_UPSTREAM to be set explicitly (the game origin). Refusing to start.");
                return 1;
            }
            if (!Gate && !string.IsNullOrEmpty(PresenterSecret) && bindEnv == "any")
            {
                // The gate-off dev relay presents from the request body (no session to
                // check). On a non-loopback bind that turns the presenter into an open
                // card scanner for anyone on the network - refused.
                Console.Error.WriteLine("WEBFRONT_GATE=0 with an armed WEBFRONT_PRESENTER_SECRET must stay loopback-bound (WEBFRONT_BIND=any refused). See docs/28.");
                return 1;
            }
            var listener = new TcpListener(bind, Port);
            try { listener.Start(); }
            catch (SocketException e) { Console.Error.WriteLine("bind failed: " + e.Message); return 1; }
            Console.WriteLine("webfront listening on http://" + bind + ":" + Port + " root=" + Root + " upstream=" + Upstream);
            if (Gate)
            {
                Console.WriteLine("webfront GATE ON: pages, auth pages and /aog need a web session (identity from " + AuthBase + "/api/me)");
                if (LobbyEnabled)
                    Console.WriteLine("webfront LOBBY ON: /login and /signup stay hidden until the access code is entered at /welcome");
                if (string.IsNullOrEmpty(PresenterSecret))
                    Console.WriteLine("webfront WARNING: WEBFRONT_PRESENTER_SECRET is not set - relayed logins will NOT present card credentials");
            }
            else
            {
                Console.WriteLine("webfront GATE OFF (LAN dev relay): pages and /aog answer without a web session");
            }

            new Thread(BusyWorker) { IsBackground = true }.Start();
            while (true)
            {
                TcpClient client;
                try { client = listener.AcceptTcpClient(); }
                catch (Exception) { return 0; }
                // Admission gate at ACCEPT time (review round 2 major #2): stalled
                // sockets can pin at most this many worker threads; anything beyond
                // gets a fast inline 503 from the accept loop itself, so a stalled
                // crowd can never queue a legitimate visitor behind it.
                if (!Admission.Wait(0))
                {
                    // Over capacity: hand to the busy thread (answers 503, drains what
                    // the client already wrote, closes clean). The queue depth is an
                    // O(1) counter, and an unbounded backlog is simply dropped (review
                    // round 2 major #7).
                    if (Interlocked.Increment(ref BusyDepth) > 256)
                    {
                        Interlocked.Decrement(ref BusyDepth);
                        try { client.Close(); } catch { }
                        continue;
                    }
                    BusyQueue.Enqueue(client);
                    continue;
                }
                // Dedicated background thread per connection (capped by Admission):
                // request handling never competes with the shared thread pool, so a
                // stalled crowd cannot delay a legitimate visitor's start either.
                var worker = new Thread(() =>
                {
                    try { HandleClient(client); }
                    finally { Admission.Release(); }
                }) { IsBackground = true };
                worker.Start();
            }
        }

        // Byte-accurate request reader: headers as latin1 lines, body as raw bytes
        // (multi-byte UTF-8 bodies and chunked encoding relay byte-exact - a StreamReader
        // char model wedges on Content-Length != char count, checkup majors #15/#16).
        // Lines are capped (review round 2 major #10) and every read runs against an
        // absolute deadline, so a half-sent body ends in a status code, never a stall
        // (review round 2 critical #1).
        private sealed class RawReader
        {
            private readonly Stream s;
            private readonly byte[] buf = new byte[8192];
            private int pos, len;
            private long deadlineMs;
            public const int MaxLineBytes = 8192;
            public RawReader(Stream s) { this.s = s; }
            public void SetDeadline(int msFromNow) { deadlineMs = Environment.TickCount64 + msFromNow; }
            private int Fill()
            {
                if (deadlineMs > 0 && Environment.TickCount64 > deadlineMs)
                    throw new TimeoutException("request read deadline exceeded");
                if (pos < len) return len - pos;
                len = s.Read(buf, 0, buf.Length);
                pos = 0;
                return len;
            }
            private int NextByte() { return Fill() <= 0 ? -1 : buf[pos++]; }
            public string ReadLineLatin1()
            {
                int b = NextByte();
                if (b < 0) return null;
                var sb = new StringBuilder();
                while (b >= 0)
                {
                    if (b == '\n') break;
                    if (b != '\r') sb.Append((char)b);
                    if (sb.Length > MaxLineBytes) throw new IOException("request line exceeds " + MaxLineBytes + " bytes");
                    b = NextByte();
                }
                return sb.ToString();
            }
            public int ReadRaw(byte[] dst, int off, int count)
            {
                int done = 0;
                while (done < count)
                {
                    if (Fill() <= 0) break;
                    int n = Math.Min(count - done, len - pos);
                    Buffer.BlockCopy(buf, pos, dst, off + done, n);
                    pos += n;
                    done += n;
                }
                return done;
            }
        }

        // Bounded admission (review round 2 major #2): stalled sockets can pin at
        // most this many worker threads; over-capacity connections get a fast 503
        // from a dedicated busy thread instead of queueing behind them.
        private static readonly SemaphoreSlim Admission = new SemaphoreSlim(128, 128);
        private static readonly System.Collections.Concurrent.ConcurrentQueue<TcpClient> BusyQueue = new System.Collections.Concurrent.ConcurrentQueue<TcpClient>();
        private static int BusyDepth;

        // Dedicated busy thread for over-capacity connections: never shares the
        // worker pool, so stalled workers cannot delay the busy answer. Answering
        // first unblocks the client; a bounded non-blocking drain of whatever the
        // client already queued makes the close a clean FIN, not a RST (review
        // round 2 major #6/#7).
        private static void BusyWorker()
        {
            while (true)
            {
                TcpClient c;
                if (!BusyQueue.TryDequeue(out c)) { Thread.Sleep(20); continue; }
                Interlocked.Decrement(ref BusyDepth);
                try
                {
                    c.SendTimeout = 2000;
                    using (c)
                    {
                        var st = c.GetStream();
                        using (var w = new StreamWriter(st, Latin1, 1024, true))
                            Respond(w, "", 503, "Service Unavailable", "text/plain", "web front busy - try again", null, "no-store");
                        DrainPending(st);
                        try { c.Client.Shutdown(SocketShutdown.Send); } catch { }
                    }
                }
                catch { }
            }
        }

        // Consume what the client already has in flight (bounded), so disposing the
        // socket after a response never strands unread bytes into a RST that eats
        // the answer (review round 2 major #6).
        private static void DrainPending(NetworkStream net)
        {
            byte[] tmp = new byte[16384];
            long until = Environment.TickCount64 + 1000;
            try
            {
                while (net.DataAvailable && Environment.TickCount64 < until)
                    if (net.Read(tmp, 0, tmp.Length) <= 0) break;
            }
            catch { }
        }

        private static void HandleClient(TcpClient client)
        {
            string inflightKey = null;
            try
            {
                using (client)
                {
                    client.ReceiveTimeout = 15000;   // idle release; 120 s parking removed
                    client.SendTimeout = 30000;
                    var net = client.GetStream();
                    var remoteIp = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
                    var reader = new RawReader(net);
                    using var writer = new StreamWriter(net, Latin1, 8192, true);

                    string line;
                    while (true)
                    {
                        try { reader.SetDeadline(15000); line = reader.ReadLineLatin1(); }
                        catch (TimeoutException) { break; }
                        catch (IOException)
                        {
                            Respond(writer, "", 431, "Request Header Fields Too Large", "text/plain", "request headers too large", null, "no-store");
                            break;
                        }
                        if (line == null) break;
                        if (line.Length == 0) continue;
                        string origin = "", acceptEnc = "", contentType = "", expect = "", cookie = "", visitorIp = remoteIp, ua = "", host = "", cfIp = "", accept = "";
                        bool broken = false;
                        bool isHead = false; // hoisted: catch-path answers must suppress HEAD bodies too (review round 2 minor #10)
                        int phase = 0; // 0=request line, 1=headers, 2=body
                        try
                        {
                            string[] parts = line.Split(' ');
                            if (parts.Length < 2) break;
                            string method = parts[0], target = parts[1];
                            phase = 1;
                            if (method != "GET" && method != "POST" && method != "HEAD" && method != "OPTIONS"
                                && method != "PUT" && method != "DELETE" && method != "PATCH")
                            {
                                Respond(writer, origin, 405, "Method Not Allowed", "text/plain", "method not allowed", null, "no-store");
                                break;
                            }
                            isHead = method == "HEAD";

                            int contentLength = 0;
                            bool chunked = false;
                            bool badFraming = false, clSeen = false;
                            long clFirst = 0;
                            reader.SetDeadline(15000);
                            int headerCount = 0, headerBytes = 0;
                            while ((line = reader.ReadLineLatin1()) != null && line.Length > 0)
                            {
                                if (++headerCount > 200 || (headerBytes += line.Length) > 65536) { broken = true; break; }
                                int colon = line.IndexOf(':');
                                if (colon <= 0) continue;
                                string name = line.Substring(0, colon).Trim().ToLowerInvariant();
                                string value = line.Substring(colon + 1).Trim();
                                switch (name)
                                {
                                    case "content-length":
                                        // Strict framing (review round 2 major #9): a
                                        // non-numeric, negative or conflicting CL is a
                                        // request-splitting setup - refuse it, never guess.
                                        if (!long.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out long clv) || clv < 0) { badFraming = true; break; }
                                        if (clSeen && clv != clFirst) { badFraming = true; break; }
                                        clSeen = true; clFirst = clv;
                                        contentLength = clv > MaxBodyBytes ? MaxBodyBytes + 1 : (int)clv;
                                        break;
                                    case "transfer-encoding": if (value.Contains("chunked", StringComparison.OrdinalIgnoreCase)) chunked = true; break;
                                    case "origin": origin = value; break;
                                    case "accept": accept = value; break;
                                    case "accept-encoding": acceptEnc = value; break;
                                    case "content-type": contentType = value; break;
                                    case "expect": expect = value; break;
                                    case "cookie": cookie = value; break;
                                    case "host": host = value; break;
                                    // The core webui only answers browser-looking UAs (it
                                    // 404s curl/8); the AOG side demands the opposite. So
                                    // the auth relay carries the visitor's own UA through.
                                    case "user-agent": ua = value; break;
                                    case "cf-connecting-ip": cfIp = value; break;
                                    // A client-supplied X-Forwarded-For is never a source
                                    // of identity: the gate's own hop is the only one the
                                    // core ever sees (review round 2 minor #15).
                                    case "x-forwarded-for": break;
                                }
                            }
                            if (broken)
                            {
                                Respond(writer, origin, 431, "Request Header Fields Too Large", "text/plain", "too many request headers", null, "no-store");
                                break;
                            }
                            if (badFraming)
                            {
                                Respond(writer, origin, 400, "Bad Request", "text/plain", "bad content-length framing", null, "no-store", null, isHead);
                                break;
                            }
                            // CF-Connecting-IP counts only when it really parses as an
                            // ip (review round 2 major #4): garbage must not become a
                            // limiter identity key either.
                            if (TrustCloudflare && !string.IsNullOrEmpty(cfIp) && cfIp.Length <= 64 && System.Net.IPAddress.TryParse(cfIp, out _)) visitorIp = cfIp;

                            // Concurrent-connection cap per visitor identity (review
                            // round 2 critical #3): one source can never hold the gate.
                            if (!VisitorConnAcquire(visitorIp))
                            {
                                Console.WriteLine("webfront: visitor " + visitorIp + " holds too many concurrent connections");
                                Respond(writer, origin, 429, "Too Many Requests", "text/plain", "too many concurrent connections from this visitor", null, "no-store", null, isHead);
                                break;
                            }
                            inflightKey = visitorIp;

                            // Normalize the path BEFORE any routing decision: percent-decoded,
                            // dot segments collapsed, control chars refused. The core receives
                            // this same canonical form, so gate classification and core routing
                            // cannot disagree (checkup major #10).
                            string path = CanonicalPath(target);
                            string query = "";
                            int q = target.IndexOf('?');
                            if (q >= 0) query = target.Substring(q + 1);
                            if (path == null)
                            {
                                Respond(writer, origin, 400, "Bad Request", "text/plain", "bad request", null, "no-store", null, isHead);
                                break;
                            }
                            string relayTarget = RelayPath(path) + (query.Length > 0 ? "?" + query : "");

                            // Per-visitor limiting covers the identity-touching routes (gate
                            // pages, core webui pages, /aog relays - everything that costs
                            // the origin). Static file serving is cheap loopback file reads
                            // with legitimate burst shape (a WebGL boot pulls hundreds of
                            // bundles); bandwidth abuse there is Cloudflare's job, not the
                            // origin gate's.
                            string lastSeg = "";
                            int slash = path.LastIndexOf('/');
                            lastSeg = slash >= 0 ? path.Substring(slash + 1) : path;
                            bool isHtml = path.EndsWith(".html", StringComparison.OrdinalIgnoreCase) || path.EndsWith(".htm", StringComparison.OrdinalIgnoreCase);
                            bool aogPath = path.StartsWith("/aog/", StringComparison.Ordinal);
                            bool aogHijack = !aogPath && KnownAogNames.Contains(lastSeg);
                            bool identityRoute = Gate && (aogPath || IsCorePage(path, method) || path == "/" || path == "/index.html" || (aogPath && isHtml));

                            if (identityRoute && !VisitorAllowed(visitorIp, Environment.TickCount64))
                            {
                                Console.WriteLine("webfront rate-limited visitor " + visitorIp + " on " + method + " " + path);
                                if (aogPath)
                                    RespondXml(writer, origin, 429, "Too Many Requests", 429, "rate limited at the web front - slow down", isHead);
                                else
                                    Respond(writer, origin, 429, "Too Many Requests", "text/plain", "rate limited at the web front - slow down", null, "no-store", null, isHead);
                                break;
                            }

                            // node fetch and some HTTP clients hold the POST body until the
                            // server sends 100 Continue; answer it before reading, else deadlock.
                            if ((chunked || contentLength > 0) && contentLength <= MaxBodyBytes && expect.Contains("100", StringComparison.OrdinalIgnoreCase))
                            {
                                writer.Write("HTTP/1.1 100 Continue\r\n\r\n");
                                writer.Flush();
                            }

                            byte[] body = Array.Empty<byte>();
                            if (contentLength > MaxBodyBytes)
                            {
                                Respond(writer, origin, 413, "Payload Too Large", "text/plain", "request body too large", null, "no-store", null, isHead);
                                break;
                            }
                            phase = 2;
                            reader.SetDeadline(30000);
                            if (chunked)
                            {
                                // chunked framing relayed byte-exact up to the same cap.
                                var chunks = new List<byte[]>();
                                int chunkTotal = 0;
                                bool bodyFailed = false, bodyTooBig = false;
                                while (true)
                                {
                                    string sizeLine = reader.ReadLineLatin1();
                                    if (sizeLine == null) { bodyFailed = true; break; }
                                    int sc = sizeLine.IndexOf(';');
                                    if (sc >= 0) sizeLine = sizeLine.Substring(0, sc);
                                    if (!int.TryParse(sizeLine.Trim(), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int cs) || cs < 0) { bodyFailed = true; break; }
                                    if (cs == 0)
                                    {
                                        // trailer fields (RFC 7230) run until the empty line (review minor #11)
                                        string t;
                                        while ((t = reader.ReadLineLatin1()) != null && t.Length > 0) { }
                                        break;
                                    }
                                    if (chunkTotal + cs > MaxBodyBytes) { bodyTooBig = true; break; }
                                    byte[] one = new byte[cs];
                                    if (reader.ReadRaw(one, 0, cs) != cs) { bodyFailed = true; break; }
                                    chunks.Add(one);
                                    chunkTotal += cs;
                                    reader.ReadLineLatin1(); // CRLF after chunk data
                                }
                                if (bodyTooBig)
                                {
                                    Respond(writer, origin, 413, "Payload Too Large", "text/plain", "request body too large", null, "no-store", null, isHead);
                                    break;
                                }
                                if (bodyFailed)
                                {
                                    Respond(writer, origin, 400, "Bad Request", "text/plain", "bad chunked body", null, "no-store", null, isHead);
                                    break;
                                }
                                int total = 0;
                                foreach (byte[] c in chunks) total += c.Length;
                                body = new byte[total];
                                int at = 0;
                                foreach (byte[] c in chunks) { Buffer.BlockCopy(c, 0, body, at, c.Length); at += c.Length; }
                            }
                            else if (contentLength > 0)
                            {
                                body = new byte[contentLength];
                                int got = 0;
                                while (got < contentLength)
                                {
                                    int n = reader.ReadRaw(body, got, contentLength - got);
                                    if (n <= 0) break;
                                    got += n;
                                }
                                if (got < contentLength)
                                {
                                    Respond(writer, origin, 400, "Bad Request", "text/plain", "incomplete body", null, "no-store", null, isHead);
                                    break;
                                }
                            }

                            // Access-code lobby: the signup/login FEATURE is behind a
                            // shared passphrase (no username). Anonymous visitors meet a
                            // code form at /welcome; /login and /signup are answered by
                            // the gate itself until the unlock cookie is present, so the
                            // core's auth pages are never even shown.
                            bool lobbyHandled = false;
                            if (Gate && LobbyEnabled)
                            {
                                if (path.Equals("/welcome", StringComparison.OrdinalIgnoreCase))
                                {
                                    lobbyHandled = true;
                                    if (method == "POST")
                                    {
                                        long ra;
                                        if (LobbyLockedOut(visitorIp, Environment.TickCount64, out ra))
                                        {
                                            Console.WriteLine("webfront lobby: locked-out attempt from " + visitorIp + " (" + ra + "s left)");
                                            Respond(writer, origin, 429, "Too Many Requests", "text/plain", "locked after wrong codes - retry in " + ra + "s", null, "no-store", new string[] { "Retry-After: " + ra }, isHead);
                                        }
                                        else if (!LobbyAttemptAllowed(visitorIp, Environment.TickCount64))
                                        {
                                            Console.WriteLine("webfront lobby: rate-limited unlock attempts from " + visitorIp);
                                            Respond(writer, origin, 429, "Too Many Requests", "text/plain", "too many access attempts - wait a minute", null, "no-store", new string[] { "Retry-After: 60" }, isHead);
                                        }
                                        else
                                        {
                                            string formText = body != null && body.Length > 0 && body.Length < 16384 ? Latin1.GetString(body) : "";
                                            string given = FormValue(formText, "code");
                                            string nxt = FormValue(formText, "next");
                                            if (string.IsNullOrEmpty(nxt)) nxt = QueryOf(relayTarget, "next");
                                            if (!string.IsNullOrEmpty(given) && CodeMatches(given))
                                            {
                                                LobbyClearFails(visitorIp);
                                                string tok = NewLobbyToken();
                                                Console.WriteLine("webfront lobby: unlocked for " + visitorIp);
                                                Respond(writer, origin, 302, "Found", null, null, null, "no-store",
                                                    new string[] { "Set-Cookie: mfg_lobby=" + tok + "; path=/; HttpOnly; SameSite=Lax; Max-Age=86400", "Location: " + SafeNext(nxt) }, isHead);
                                            }
                                            else
                                            {
                                                LobbyRecordFail(visitorIp, Environment.TickCount64);
                                                Respond(writer, origin, 200, "OK", "text/html; charset=utf-8", LobbyPage(SafeNext(nxt), "Wrong access code."), null, "no-store", null, isHead);
                                            }
                                        }
                                    }
                                    else if (method == "GET" || method == "HEAD")
                                    {
                                        string nxt = QueryOf(relayTarget, "next");
                                        if (string.IsNullOrEmpty(nxt)) nxt = "/login";
                                        if (LobbyOk(cookie))
                                            Respond(writer, origin, 302, "Found", null, null, null, "no-store", new string[] { "Location: " + SafeNext(nxt) }, isHead);
                                        else
                                            Respond(writer, origin, 200, "OK", "text/html; charset=utf-8", LobbyPage(SafeNext(nxt), null), null, "no-store", null, isHead);
                                    }
                                    else
                                        Respond(writer, origin, 405, "Method Not Allowed", "text/plain", "method not allowed", null, "no-store", null, isHead);
                                }
                                else if (!LobbyOk(cookie) && (path.Equals("/login", StringComparison.OrdinalIgnoreCase) || path.Equals("/signup", StringComparison.OrdinalIgnoreCase)))
                                {
                                    lobbyHandled = true;
                                    if (method == "GET" || method == "HEAD")
                                        Respond(writer, origin, 200, "OK", "text/html; charset=utf-8", LobbyPage(SafeNext(relayTarget), null), null, "no-store", null, isHead);
                                    else
                                        Respond(writer, origin, 403, "Forbidden", "text/plain", "enter the access code at /welcome first", null, "no-store", null, isHead);
                                }
                            }

                            // login/signup/logout are AOG names too (the old native lane
                            // posted them bare) - the hijack guard must not block the core
                            // webui's own routes. It refuses an AOG endpoint name outside
                            // /aog/ unless the path is exactly one of those webui pages;
                            // webui pages do not take a game endpoint as a sub-path.
                            if (lobbyHandled) { /* the lobby already answered this request */ }
                            else if (aogHijack && Gate && !ExactCorePath(path))
                            {
                                Console.WriteLine("webfront refused " + method + " " + path + " (AOG endpoint name outside /aog/)");
                                Respond(writer, origin, 404, "Not Found", "text/plain", "not found", null, "no-store", null, isHead);
                            }
                            else if (method == "OPTIONS" && aogPath && !isHtml)
                            {
                                Respond(writer, origin, 204, "No Content", null, null, null, "no-store", null, isHead);
                            }
                            else if (aogPath && !isHtml)
                            {
                                if (Gate) HandleApi(writer, origin, method, relayTarget, body, contentType, cookie, visitorIp, isHead);
                                else DevRelay(writer, origin, method, relayTarget, path, body, contentType, visitorIp, isHead);
                            }
                            else if (aogPath && isHtml)
                            {
                                // A page under the /aog prefix (the shape the statics
                                // server uses too): gated or plain, never relayed as API.
                                if (Gate) HandlePage(writer, origin, cookie, path, visitorIp, isHead);
                                else ServeStatic(writer, net, origin, acceptEnc, path, isHead);
                            }
                            else if (path == "/index_play.html" && Gate)
                            {
                                // The LAN artifact's old name: bounce to the session page.
                                Respond(writer, origin, 302, "Found", null, null, null, "no-store",
                                    new string[] { "Location: /" }, isHead);
                            }
                            else if (Gate && (path == "/" || path == "/index.html"))
                            {
                                HandlePage(writer, origin, cookie, path, visitorIp, isHead);
                            }
                            else if (Gate && method == "GET" && path.Equals("/about", StringComparison.OrdinalIgnoreCase))
                            {
                                // Hosted lane: the core webui landing page is an operator
                                // page (contributor wall, admin nav). Signed-in visitors go
                                // straight to the game - the '/' page pins their own card -
                                // anonymous visitors go to the login form.
                                SessionEntry aboutSession = CheckSession(cookie, visitorIp);
                                Respond(writer, origin, 302, "Found", null, null, null, "no-store",
                                    new string[] { "Location: " + (aboutSession.Kind == SessionKind.Ok ? "/" : "/login") }, isHead);
                            }
                            else if (Gate && IsCorePage(path, method))
                            {
                                HandleAuth(writer, origin, method, relayTarget, path, body, contentType, cookie, visitorIp, ua, host, accept, isHead);
                            }
                            else if (Gate && isHtml)
                            {
                                // Gated mode never answers arbitrary .html names with the
                                // index page (checkup major #19).
                                Respond(writer, origin, 404, "Not Found", "text/plain", "not found", null, "no-store", null, isHead);
                            }
                            else
                            {
                                ServeStatic(writer, net, origin, acceptEnc, path, isHead);
                            }
                            // One request per connection: every answer declares
                            // Connection: close, so responding ends this request.
                            break;
                        }
                        catch (TimeoutException)
                        {
                            // body did not arrive in time: answer and drop the socket
                            // (review round 2 critical #1 - no more 120 s silence)
                            Console.WriteLine("webfront: request read deadline exceeded (" + phase + ")");
                            Respond(writer, origin, 408, "Request Timeout", "text/plain", "request body did not arrive in time", null, "no-store", null, isHead);
                            break;
                        }
                        catch (IOException io)
                        {
                            // socket idle-timeout / oversized line: phase tells which
                            if (phase >= 2)
                            {
                                Console.WriteLine("webfront: body read stalled (" + io.Message + ")");
                                Respond(writer, origin, 400, "Bad Request", "text/plain", "incomplete body", null, "no-store", null, isHead);
                            }
                            else
                                Respond(writer, origin, 431, "Request Header Fields Too Large", "text/plain", "request headers too large", null, "no-store", null, isHead);
                            break;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("webfront error: " + e);
                            Respond(writer, origin, 502, "Bad Gateway", "text/plain", "front upstream unavailable", null, "no-store", null, isHead);
                            break;
                        }
                    }
                    // One request per connection (every answer declares Connection:
                    // close). Drain what the client already queued so the close is a
                    // clean FIN carrying the answer, not a RST that eats it (review
                    // round 2 majors #2/#5/#6).
                    DrainPending(net);
                    try { client.Client.Shutdown(SocketShutdown.Send); } catch { }
                }
            }
            catch { }
            finally { if (inflightKey != null) VisitorConnRelease(inflightKey); }
        }

        private static readonly Encoding UTF8Fallback = new UTF8Encoding(false);

        private static void Respond(StreamWriter writer, string origin, int status, string reason, string contentType, string bodyLatin1, string contentEncoding, string cacheControl, string[] extraHeaders = null, bool head = false, long clOverride = -1)
        {
            WriteHead(writer, origin, status, reason, contentType,
                clOverride >= 0 ? clOverride : (bodyLatin1 == null ? 0 : bodyLatin1.Length),
                contentEncoding, cacheControl, extraHeaders);
            // HEAD may declare the entity length but must not carry body bytes (RFC 9112).
            if (!head && bodyLatin1 != null && bodyLatin1.Length > 0) writer.Write(bodyLatin1);
            writer.Flush();
        }

        private static void WriteHead(StreamWriter writer, string origin, int status, string reason, string contentType, long contentLength, string contentEncoding, string cacheControl, string[] extraHeaders)
        {
            writer.Write("HTTP/1.1 ");
            writer.Write(status.ToString(CultureInfo.InvariantCulture));
            writer.Write(' ');
            writer.Write(reason);
            writer.Write("\r\nCross-Origin-Opener-Policy: same-origin\r\nCross-Origin-Embedder-Policy: require-corp\r\nAccess-Control-Allow-Origin: ");
            writer.Write(string.IsNullOrEmpty(origin) ? "*" : origin);
            writer.Write("\r\nAccess-Control-Allow-Methods: GET, HEAD, POST, PUT, DELETE, OPTIONS\r\nAccess-Control-Allow-Headers: content-type\r\n");
            if (contentType != null) { writer.Write("Content-Type: "); writer.Write(contentType); writer.Write("\r\n"); }
            if (contentEncoding != null) { writer.Write("Content-Encoding: "); writer.Write(contentEncoding); writer.Write("\r\n"); }
            if (extraHeaders != null)
                foreach (string h in extraHeaders)
                    if (!string.IsNullOrEmpty(h)) { writer.Write(h); writer.Write("\r\n"); }
            writer.Write("Cache-Control: ");
            writer.Write(string.IsNullOrEmpty(cacheControl) ? "no-store" : cacheControl);
            writer.Write("\r\nContent-Length: ");
            writer.Write(contentLength.ToString(CultureInfo.InvariantCulture));
            writer.Write("\r\nConnection: close\r\n\r\n");
        }

        private static void RespondXml(StreamWriter writer, string origin, int status, string reason, int code, string msg, bool head = false)
        {
            string xml = "<root><serv_st><code>" + code + "</code><msg>" + WebUtility.HtmlEncode(msg) + "</msg></serv_st></root>";
            Respond(writer, origin, status, reason, "text/xml; charset=utf-8", ToLatin1(UTF8Fallback.GetBytes(xml)), null, "no-store", null, head);
        }

        private static string ToLatin1(byte[] b)
        {
            if (b == null || b.Length == 0) return null;
            char[] chars = new char[b.Length];
            for (int i = 0; i < b.Length; i++) chars[i] = (char)b[i];
            return new string(chars);
        }

        // Percent-decode (up to 3 rounds), collapse dot segments, refuse control chars.
        // Returns null for a path that cannot be made safe.
        private static string CanonicalPath(string target)
        {
            string p = target;
            int q = p.IndexOf('?');
            if (q >= 0) p = p.Substring(0, q);
            if (p.Length == 0 || p[0] != '/') p = "/" + p;
            for (int round = 0; round < 3; round++)
            {
                string u;
                try { u = Uri.UnescapeDataString(p); } catch { return null; }
                if (u == p) break;
                p = u;
            }
            var segs = new List<string>();
            foreach (string seg in p.Split('/'))
            {
                if (seg.Length == 0 || seg == ".") continue;
                if (seg == "..") { if (segs.Count > 0) segs.RemoveAt(segs.Count - 1); continue; }
                foreach (char c in seg)
                    if (c < 0x20 || c == '\\') return null;
                // Windows path normalization silently drops trailing dots/spaces -
                // "/index.html." was reaching index.html (checkup major #19).
                if (seg.EndsWith(".") || seg.EndsWith(" ")) return null;
                segs.Add(seg);
            }
            return "/" + string.Join("/", segs);
        }

        // Re-encode a canonical path for the wire: each segment escaped, non-ASCII safe.
        private static string RelayPath(string canonical)
        {
            var parts = canonical.Split('/');
            for (int i = 0; i < parts.Length; i++)
                if (parts[i].Length > 0) parts[i] = Uri.EscapeDataString(parts[i]);
            return string.Join("/", parts);
        }

        private static void ServeStatic(StreamWriter writer, NetworkStream net, string origin, string acceptEnc, string canonical, bool head = false)
        {
            string rel = canonical == "/" ? "index.html" : canonical.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            string file = Path.GetFullPath(Path.Combine(Root, rel));
            if (!file.StartsWith(Root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            {
                Respond(writer, origin, 403, "Forbidden", "text/plain", "forbidden", null, "no-store", null, head);
                return;
            }
            // Compression sidecars are only ever served through their base name
            // (which keeps gating intact): naming .br/.gz directly is a 404
            // (review round 2 major #4 - /index.html.br skipped the whole page gate).
            if (file.EndsWith(".br", StringComparison.OrdinalIgnoreCase) || file.EndsWith(".gz", StringComparison.OrdinalIgnoreCase))
            {
                Respond(writer, origin, 404, "Not Found", "text/plain", "not found", null, "no-store", null, head);
                return;
            }
            string enc = null;
            if (acceptEnc != null && acceptEnc.Contains("br") && File.Exists(file + ".br")) { file += ".br"; enc = "br"; }
            if (!File.Exists(file))
            {
                Console.WriteLine("webfront static 404: " + file);
                Respond(writer, origin, 404, "Not Found", "text/plain", "not found", null, "no-store", null, head);
                return;
            }
            // Streamed copy (review round 2 critical #1): the gate must never stage a
            // whole build artifact in memory - six concurrent 144 MB downloads used
            // to reserve ~5 GB of strings.
            long size = new FileInfo(file).Length;
            WriteHead(writer, origin, 200, "OK", Mime(file), size, enc, CachePolicyOf(file), null);
            writer.Flush();
            if (head) return;
            try
            {
                byte[] buf = new byte[65536];
                using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 65536, FileOptions.SequentialScan))
                {
                    int n;
                    while ((n = fs.Read(buf, 0, buf.Length)) > 0) net.Write(buf, 0, n);
                }
                net.Flush();
            }
            catch (IOException) { } // client hung up mid-send
        }

        // Content-hashed build artifacts are immutable; plain-named entry files
        // (loader/framework/data, .js/.css without a hash token) get a short TTL so
        // a rebuild reaches browsers within an hour instead of being pinned for a
        // year (review round 2 major #15). Play pages stay no-store.
        private static readonly Regex HashedTokenRe = new Regex("[0-9a-f]{16}", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static string CachePolicyOf(string file)
        {
            string name = Path.GetFileName(file);
            if (name.EndsWith(".html", StringComparison.OrdinalIgnoreCase) || name.EndsWith(".htm", StringComparison.OrdinalIgnoreCase)
                || name.EndsWith(".html.br", StringComparison.OrdinalIgnoreCase) || name.EndsWith(".html.gz", StringComparison.OrdinalIgnoreCase))
                return "no-store";
            return HashedTokenRe.IsMatch(name) ? "public, max-age=31536000, immutable" : "public, max-age=3600";
        }

        private static void Forward(StreamWriter writer, string origin, string method, string relayTarget, byte[] body, string contentType, string visitorIp, bool head = false)
        {
            string pathAndQuery = relayTarget.StartsWith("/") ? relayTarget : "/" + relayTarget;
            string url = Upstream.TrimEnd('/') + pathAndQuery;
            HttpResponseMessage resp;
            try
            {
                // Relay the true verb (review round 2 minor #16): rewriting every
                // non-GET to POST let a HEAD probe execute POST handlers at the origin.
                HttpMethod verb = method switch
                {
                    "GET" => HttpMethod.Get,
                    "HEAD" => HttpMethod.Head,
                    "POST" => HttpMethod.Post,
                    "PUT" => HttpMethod.Put,
                    "DELETE" => HttpMethod.Delete,
                    "PATCH" => HttpMethod.Patch,
                    _ => HttpMethod.Post,
                };
                using var mreq = new HttpRequestMessage(verb, url);
                if (body != null && body.Length > 0)
                {
                    var content = new ByteArrayContent(body);
                    if (!string.IsNullOrEmpty(contentType)) content.Headers.TryAddWithoutValidation("Content-Type", contentType);
                    mreq.Content = content;
                }
                mreq.Headers.TryAddWithoutValidation("User-Agent", RelayUA);
                // Single honest hop: the gate's own view of the visitor, never a
                // client-supplied chain (checkup major #21).
                mreq.Headers.TryAddWithoutValidation("X-Forwarded-For", visitorIp);
                resp = Up.SendAsync(mreq).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                Console.WriteLine("webfront forward error: " + e.Message);
                RespondXml(writer, origin, 502, "Bad Gateway", 502, "upstream unavailable", head);
                return;
            }
            using (resp)
            {
                byte[] payload = resp.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
                string ct = resp.Content.Headers.ContentType?.ToString() ?? "text/xml";
                // AOG answers are per-session game state - caching one would let a
                // superseded pcuid replay last game's bytes (docs/28 defect fix).
                // HEAD keeps the origin's declared entity length instead of 0
                // (review round 2 minor #11).
                long clOver = head ? (resp.Content.Headers.ContentLength ?? payload.Length) : -1;
                Respond(writer, origin, (int)resp.StatusCode, resp.ReasonPhrase ?? "OK", ct, ToLatin1(payload), null, "no-store", null, head, clOver);
            }
        }

        // --- gated (hosted) handlers: WEBFRONT_GATE=1 -------------------------

        // Paths the core webui owns; relayed with the session cookie in both
        // directions (Set-Cookie is how a login reaches the browser at all).
        // Segment-boundary match (no /loginish), exact-only relaying (the core
        // receives the canonical path the gate classified).
        private static readonly string[] CorePagePrefixes = { "/login", "/signup", "/logout", "/about", "/account", "/my-profile", "/profile", "/profiles", "/static", "/help", "/api/me", "/favicon.ico" };
        private static readonly string[] CoreExactPaths = { "/login", "/signup", "/logout", "/about", "/account", "/api/me", "/favicon.ico" };
        private static bool ExactCorePath(string path)
        {
            foreach (string p in CoreExactPaths)
                if (path.Equals(p, StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }
        private static bool IsCorePage(string path, string method)
        {
            foreach (string p in CorePagePrefixes)
                if (path == p || path.StartsWith(p + "/", StringComparison.OrdinalIgnoreCase))
                    return true;
            // Plugin static assets only, GET-only: the core's settings POST lives under
            // /plugin too and must never be reachable from the public edge (checkup
            // critical #2; the core additionally admin-gates those POSTs).
            if (method == "GET" && path.StartsWith("/plugin/", StringComparison.OrdinalIgnoreCase) && path.Contains("/static/"))
                return true;
            return false;
        }

        private static string SessionKeyOf(string cookie)
        {
            if (string.IsNullOrEmpty(cookie)) return null;
            int i = cookie.IndexOf("connect.sid=", StringComparison.Ordinal);
            if (i < 0) return null;
            i += "connect.sid=".Length;
            int end = cookie.IndexOf(';', i);
            string sid = end < 0 ? cookie.Substring(i) : cookie.Substring(i, end - i);
            return sid.Length == 0 || sid.Length > 512 ? null : sid;
        }

        private static SessionEntry CheckSession(string cookie, string visitorIp)
        {
            string key = SessionKeyOf(cookie);
            if (key == null) return new SessionEntry { Kind = SessionKind.Anonymous, UntilMs = 0 };
            long now = Environment.TickCount64;
            lock (SessionsLock)
            {
                if (Sessions.TryGetValue(key, out SessionEntry hit) && hit.UntilMs > now) return hit;
            }
            SessionEntry entry = null;
            try
            {
                using var mreq = new HttpRequestMessage(HttpMethod.Get, AuthBase.TrimEnd('/') + "/api/me");
                mreq.Headers.TryAddWithoutValidation("Cookie", "connect.sid=" + key);
                mreq.Headers.TryAddWithoutValidation("X-Forwarded-For", visitorIp);
                // Browser-looking UA: the core webui 404s non-browser agents.
                mreq.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (compatible; MFG-WebFront/1.0; session-gate)");
                using var resp = Auth.SendAsync(mreq).GetAwaiter().GetResult();
                if (resp.StatusCode == HttpStatusCode.OK)
                {
                    string json = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    string user = null, card = null;
                    bool admin = false, ok = false;
                    try
                    {
                        using var doc = JsonDocument.Parse(json);
                        var root = doc.RootElement;
                        ok = root.TryGetProperty("success", out var s) && s.GetBoolean();
                        if (ok && root.TryGetProperty("username", out var u) && u.ValueKind == JsonValueKind.String) user = u.GetString();
                        if (ok && root.TryGetProperty("cardNumber", out var c) && c.ValueKind == JsonValueKind.String) card = c.GetString();
                        if (ok && root.TryGetProperty("admin", out var a) && a.ValueKind == JsonValueKind.True) admin = true;
                    }
                    catch { }
                    if (ok && !string.IsNullOrEmpty(user))
                        entry = new SessionEntry { Kind = SessionKind.Ok, Info = new SessionInfo { Username = user, Card = card ?? "", Admin = admin }, UntilMs = now + SessionTtlMs };
                    else
                        entry = new SessionEntry { Kind = SessionKind.Anonymous, UntilMs = now + NegativeTtlMs };
                }
                else if ((int)resp.StatusCode < 500)
                    entry = new SessionEntry { Kind = SessionKind.Anonymous, UntilMs = now + NegativeTtlMs };
            }
            catch (Exception e)
            {
                // Upstream down/slow: report UpstreamError, keep any previous entry
                // (a live session must not die because the origin hiccuped).
                Console.WriteLine("webfront session check error: " + e.Message);
                return new SessionEntry { Kind = SessionKind.UpstreamError, UntilMs = 0 };
            }
            if (entry != null)
                lock (SessionsLock)
                {
                    if (Sessions.Count >= MaxSessions)
                    {
                        foreach (string k in new List<string>(Sessions.Keys))
                            if (Sessions[k].UntilMs <= now) Sessions.Remove(k);
                        if (Sessions.Count >= MaxSessions) Sessions.Clear();
                    }
                    Sessions[key] = entry;
                }
            return entry ?? new SessionEntry { Kind = SessionKind.UpstreamError, UntilMs = 0 };
        }

        // Present this session's own DataID as its card credential, just before
        // the /login we relay (utils.ts presentViaPresenter consumes exactly one
        // stamp per login - so every relayed login presents, and nothing else
        // can). The plugin answers 200 even for refusals (AOG convention), so
        // success is the <presented> marker in the body, not the status code.
        private static string FormValue(string formText, string key)
        {
            foreach (string pair in formText.Split('&'))
            {
                int eq = pair.IndexOf('=');
                if (eq <= 0) continue;
                if (pair.Substring(0, eq) == key)
                {
                    try { return Uri.UnescapeDataString(pair.Substring(eq + 1)); } catch { return pair.Substring(eq + 1); }
                }
            }
            return "";
        }

        private static string QueryOf(string target, string name)
        {
            int q = target.IndexOf('?');
            if (q < 0) return null;
            foreach (string pair in target.Substring(q + 1).Split('&'))
            {
                int eq = pair.IndexOf('=');
                if (eq <= 0) continue;
                if (pair.Substring(0, eq) == name)
                {
                    try { return Uri.UnescapeDataString(pair.Substring(eq + 1)); } catch { return null; }
                }
            }
            return null;
        }

        private static bool PresentCard(string card, string label)
        {
            if (string.IsNullOrEmpty(PresenterSecret) || string.IsNullOrEmpty(card)) return false;
            try
            {
                using var mreq = new HttpRequestMessage(HttpMethod.Post, Upstream.TrimEnd('/') + "/aog/vfgcard_present");
                byte[] form = UTF8Fallback.GetBytes("refid=" + Uri.EscapeDataString(card) + "&cardid=" + Uri.EscapeDataString(card) + "&presenter=" + Uri.EscapeDataString(PresenterSecret));
                var content = new ByteArrayContent(form);
                content.Headers.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded");
                mreq.Content = content;
                mreq.Headers.TryAddWithoutValidation("User-Agent", RelayUA);
                using var resp = Up.SendAsync(mreq).GetAwaiter().GetResult();
                string bodyText = "";
                try { bodyText = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult(); } catch { }
                bool ok = resp.StatusCode == HttpStatusCode.OK && bodyText.Contains("<presented>");
                if (!ok && (card.Length == 16 || label.StartsWith("user="))) // unissued non-hex fixtures are refused routinely on dev lanes; web-issued ids must be visible
                    Console.WriteLine("webfront presenter REFUSED " + label + " card=" + card + " status=" + (int)resp.StatusCode + " " + (bodyText.Length > 200 ? bodyText.Substring(0, 200) : bodyText));
                return ok;
            }
            catch (Exception e)
            {
                Console.WriteLine("webfront presenter error: " + e.Message);
                return false;
            }
        }

        private static void DevRelay(StreamWriter writer, string origin, string method, string relayTarget, string path, byte[] body, string contentType, string visitorIp, bool head)
        {
            // Dev relay with an armed presenter: claimed/web-issued accounts (a core
            // card document exists - the plugin checks that) log in on this lane too,
            // and once they are card-bound they answer /login with a presentation or
            // nothing. user_id comes from the relayed form body; ids the deployment
            // never issued are never bound, and this lane is loopback-only by the
            // startup guard above (docs/28 trust model).
            if (!string.IsNullOrEmpty(PresenterSecret) && (path == "/aog/login" || path == "/aog/create_player")
                && body != null && body.Length > 0 && body.Length < 16384)
            {
                string userId = FormValue(Latin1.GetString(body), "user_id");
                if (!string.IsNullOrEmpty(userId)) PresentCard(userId.Trim().ToUpperInvariant(), "dev-relay");
            }
            Forward(writer, origin, method, relayTarget, body, contentType, visitorIp, head);
        }

        private static void HandlePage(StreamWriter writer, string origin, string cookie, string path, string visitorIp, bool head)
        {
            SessionEntry e = CheckSession(cookie, visitorIp);
            if (e.Kind == SessionKind.UpstreamError)
            {
                Respond(writer, origin, 503, "Service Unavailable", "text/plain", "the identity origin is temporarily unavailable - try again", null, "no-store", null, head);
                return;
            }
            if (e.Kind != SessionKind.Ok)
            {
                // Lobby enabled and not unlocked: the entrance itself is the code
                // form, not the login page.
                string where = LobbyEnabled && !LobbyOk(cookie)
                    ? "/welcome?next=" + Uri.EscapeDataString(path)
                    : "/login?next=" + Uri.EscapeDataString(path);
                Respond(writer, origin, 302, "Found", null, null, null, "no-store",
                    new string[] { "Location: " + where }, head);
                return;
            }
            if (string.IsNullOrEmpty(e.Info.Card))
            {
                // A web account without a game DataID cannot play; send it where an
                // admin can claim one for it instead of looping / <-> /login.
                Respond(writer, origin, 302, "Found", null, null, null, "no-store",
                    new string[] { "Location: /account" }, head);
                return;
            }
            // Always splice from the canonical template: a stale index_play.html
            // left by the LAN scripts (with someone else's -DATAID) is never served
            // to a hosted session.
            string file = Path.GetFullPath(Path.Combine(Root, "index.html"));
            if (!File.Exists(file))
            {
                Respond(writer, origin, 404, "Not Found", "text/plain", "webroot index.html missing", null, "no-store", null, head);
                return;
            }
            string html = UTF8Fallback.GetString(File.ReadAllBytes(file));
            // Fail closed: the template must carry the splice anchor AND no leftover
            // pinned identity - otherwise someone else's -DATAID could ride through
            // verbatim (checkup minor #30).
            if (!html.Contains("\"-AUTOSERVE\"") || html.Contains("\"-DATAID="))
            {
                Console.WriteLine("webfront: webroot index.html is not a clean splice template (anchor missing or identity already pinned) - refusing to serve");
                Respond(writer, origin, 500, "Internal Server Error", "text/plain", "play page template not splicable", null, "no-store", null, head);
                return;
            }
            string pin = "\"-ENTERCARD\", \"-TRANSLATE\", \"-REGULAR\", \"-DATAID=" + e.Info.Card + "\"";
            string page = html.Replace(", \"-AUTOSERVE\"", ", " + pin).Replace("\"-AUTOSERVE\", ", pin + ", ");
            if (page.Contains("-AUTOSERVE"))
            {
                Console.WriteLine("webfront: webroot index.html has no -AUTOSERVE anchor to splice - refusing to serve");
                Respond(writer, origin, 500, "Internal Server Error", "text/plain", "play page splice failed", null, "no-store", null, head);
                return;
            }
            // Positional -FRONTURL rewrite: assemble scripts may write any literal.
            page = Regex.Replace(page, "-FRONTURL\\x22,\\s*\\x22[^\\x22]*\\x22", "-FRONTURL\", \"" + PageFrontUrl.Replace("$", "$$") + "\"");
            Respond(writer, origin, 200, "OK", "text/html; charset=utf-8", ToLatin1(UTF8Fallback.GetBytes(page)), null, "no-store", null, head);
        }

        private static void HandleApi(StreamWriter writer, string origin, string method, string relayTarget, byte[] body, string contentType, string cookie, string visitorIp, bool head)
        {
            SessionEntry e = CheckSession(cookie, visitorIp);
            if (e.Kind == SessionKind.UpstreamError)
            {
                RespondXml(writer, origin, 503, "Service Unavailable", 503, "the identity origin is temporarily unavailable", head);
                return;
            }
            if (e.Kind != SessionKind.Ok)
            {
                RespondXml(writer, origin, 403, "Forbidden", 770, LobbyEnabled ? "web session required: enter the access code at /welcome, then sign in at /login" : "web session required: sign in at /login first", head);
                return;
            }
            if (string.IsNullOrEmpty(e.Info.Card))
            {
                RespondXml(writer, origin, 403, "Forbidden", 770, "this web account has no game DataID yet - an admin must claim one for it", head);
                return;
            }
            string path = relayTarget.Split('?')[0];
            if (path == "/aog/login" || path == "/aog/create_player") PresentCard(e.Info.Card, "user=" + e.Info.Username);
            Forward(writer, origin, method, relayTarget, body, contentType, visitorIp, head);
        }

        private static void HandleAuth(StreamWriter writer, string origin, string method, string relayTarget, string path, byte[] body, string contentType, string cookie, string visitorIp, string ua, string host, string accept, bool head)
        {
            string pathAndQuery = relayTarget.StartsWith("/") ? relayTarget : "/" + relayTarget;
            string url = AuthBase.TrimEnd('/') + pathAndQuery;
            try
            {
                // Relay the visitor's real method - rewriting everything to POST made
                // nominally safe verbs execute POST routes at the origin (review #12).
                HttpMethod verb = method switch
                {
                    "GET" => HttpMethod.Get,
                    "POST" => HttpMethod.Post,
                    "HEAD" => HttpMethod.Head,
                    "PUT" => HttpMethod.Put,
                    "DELETE" => HttpMethod.Delete,
                    _ => new HttpMethod(method),
                };
                using var mreq = new HttpRequestMessage(verb, url);
                if (body != null && body.Length > 0)
                {
                    var content = new ByteArrayContent(body);
                    if (!string.IsNullOrEmpty(contentType)) content.Headers.TryAddWithoutValidation("Content-Type", contentType);
                    mreq.Content = content;
                }
                if (!string.IsNullOrEmpty(cookie)) mreq.Headers.TryAddWithoutValidation("Cookie", cookie);
                // Accept decides JSON-vs-redirect at the core (the account page's
                // token buttons depend on it - review round 2 minor #14).
                if (!string.IsNullOrEmpty(accept)) mreq.Headers.TryAddWithoutValidation("Accept", accept);
                mreq.Headers.TryAddWithoutValidation("X-Forwarded-For", visitorIp);
                if (!string.IsNullOrEmpty(host)) mreq.Headers.TryAddWithoutValidation("Host", host);
                mreq.Headers.TryAddWithoutValidation("X-Forwarded-Proto", TrustCloudflare ? "https" : "http");
                // The core webui 404s non-browser agents and its AOG middleware claims
                // any non-Mozilla request - so the auth relay always travels as a
                // browser agent. (Game traffic is the /aog relay with the neutral UA.)
                mreq.Headers.TryAddWithoutValidation("User-Agent", !string.IsNullOrEmpty(ua) && ua.Contains("Mozilla") ? ua : "Mozilla/5.0 (compatible; MFG-WebFront/1.0)");
                using var resp = Auth.SendAsync(mreq).GetAwaiter().GetResult();
                byte[] payload = resp.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
                string ct = resp.Content.Headers.ContentType?.ToString() ?? "text/html; charset=utf-8";
                var extras = new List<string>();
                if (resp.Headers.TryGetValues("Set-Cookie", out var cookies))
                    foreach (string v in cookies) extras.Add("Set-Cookie: " + v);
                if (resp.Headers.Location != null) extras.Add("Location: " + resp.Headers.Location.ToString());
                if (path.Equals("/logout", StringComparison.OrdinalIgnoreCase) && (int)resp.StatusCode < 400)
                {
                    // Evict the gate's view immediately and clear the browser cookie -
                    // only after the origin really ended the session (the core now
                    // answers POST /logout too, review round 2 major #3). A logged-out
                    // cookie must not keep the play page alive for the cache window
                    // (checkup major #14).
                    string key = SessionKeyOf(cookie);
                    if (key != null) lock (SessionsLock) Sessions.Remove(key);
                    extras.Add("Set-Cookie: connect.sid=; path=/; expires=Wed, 21 Oct 2015 07:28:00 GMT");
                }
                // HEAD keeps the origin's declared entity length (review round 2 minor #11).
                long clOver = head ? (resp.Content.Headers.ContentLength ?? payload.Length) : -1;
                Respond(writer, origin, (int)resp.StatusCode, resp.ReasonPhrase ?? "OK", ct, ToLatin1(payload), null, "no-store", extras.ToArray(), head, clOver);
            }
            catch (Exception e)
            {
                Console.WriteLine("webfront auth relay error: " + e);
                Respond(writer, origin, 502, "Bad Gateway", "text/plain", "the identity origin is temporarily unavailable", null, "no-store", null, head);
            }
        }

        private static string Mime(string f)
        {
            // Classify brotli sidecars by the underlying name: .br must keep the base type,
            // otherwise index.html.br arrives as octet-stream and the browser aborts the load.
            if (f.EndsWith(".br", StringComparison.OrdinalIgnoreCase)) f = f.Substring(0, f.Length - 3);
            if (f.EndsWith(".html", StringComparison.OrdinalIgnoreCase)) return "text/html; charset=utf-8";
            if (f.EndsWith(".js", StringComparison.OrdinalIgnoreCase)) return "text/javascript";
            if (f.EndsWith(".wasm", StringComparison.OrdinalIgnoreCase)) return "application/wasm";
            if (f.EndsWith(".json", StringComparison.OrdinalIgnoreCase)) return "application/json";
            if (f.EndsWith(".css", StringComparison.OrdinalIgnoreCase)) return "text/css";
            if (f.EndsWith(".bundle", StringComparison.OrdinalIgnoreCase)) return "application/octet-stream";
            if (f.EndsWith(".webm", StringComparison.OrdinalIgnoreCase)) return "video/webm";
            if (f.EndsWith(".ogg", StringComparison.OrdinalIgnoreCase)) return "audio/ogg";
            return "application/octet-stream";
        }
    }
}
