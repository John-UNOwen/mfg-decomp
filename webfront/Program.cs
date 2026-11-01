using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace WebFront
{
    // Browser front for the WebGL rebuild (Gate 3/4 support piece).
    // Raw TcpListener (same bind model as the repo mock, no http.sys/urlacl dependencies).
    // Serves the Unity WebGL export + content trees (incl. addressables bundles) from one origin,
    // and proxies /aog/* to the public provider, rewriting the User-Agent (the provider's AOG
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
        private static readonly string AuthBase = Environment.GetEnvironmentVariable("WEBFRONT_AUTH") ?? (Environment.GetEnvironmentVariable("WEBFRONT_UPSTREAM") ?? "http:// <provider-host>");
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

        // Session cache: cookie -> who it answers to. The core is loopback, but the
        // game posts keep_alive/gget every second, so /api/me is not re-asked for
        // every one of those; 30 s is short enough for a logout to catch up.
        private sealed class SessionInfo { public string Username; public string Card; public bool Admin; public long UntilMs; }
        private static readonly System.Collections.Generic.Dictionary<string, SessionInfo> Sessions = new System.Collections.Generic.Dictionary<string, SessionInfo>();
        private static readonly object SessionsLock = new object();

        // Latin1 (28591): char 0..255 encodes to exactly one byte, so bodies are staged as
        // latin1-mapped strings and go out byte-exact.
        private static readonly Encoding Latin1 = Encoding.GetEncoding(28591);
        private static readonly HttpClient Up = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false }) { Timeout = TimeSpan.FromSeconds(60) };

        private static int Main()
        {
            Directory.CreateDirectory(Root);
            // WEBFRONT_BIND=any serves on all interfaces (guest VM browser play); default loopback.
            var bind = Environment.GetEnvironmentVariable("WEBFRONT_BIND") == "any" ? IPAddress.Any : IPAddress.Loopback;
            var listener = new TcpListener(bind, Port);
            try { listener.Start(); }
            catch (SocketException e) { Console.Error.WriteLine("bind failed: " + e.Message); return 1; }
            Console.WriteLine("webfront listening on http://" + bind + ":" + Port + " root=" + Root + " upstream=" + Upstream);
            if (Gate)
            {
                Console.WriteLine("webfront GATE ON: pages, auth pages and /aog need a web session (identity from " + AuthBase + "/api/me)");
                if (string.IsNullOrEmpty(PresenterSecret))
                    Console.WriteLine("webfront WARNING: WEBFRONT_PRESENTER_SECRET is not set - relayed logins will NOT present card credentials");
            }
            else
            {
                Console.WriteLine("webfront GATE OFF (LAN dev relay): pages and /aog answer without a web session");
            }

            while (true)
            {
                TcpClient client;
                try { client = listener.AcceptTcpClient(); }
                catch (Exception) { return 0; }
                ThreadPool.QueueUserWorkItem(_ => HandleClient(client));
            }
        }

        private static void HandleClient(TcpClient client)
        {
            try
            {
                using (client)
                {
                    client.ReceiveTimeout = 120000;
                    client.SendTimeout = 30000;
                    var net = client.GetStream();
                    using var reader = new StreamReader(net, UTF8Fallback, false, 8192, true);
                    using var writer = new StreamWriter(net, Latin1, 8192, true);

                    string line;
                    while ((line = reader.ReadLine()) != null && line.Length > 0)
                    {
                        string[] parts = line.Split(' ');
                        if (parts.Length < 2) continue;
                        string method = parts[0], target = parts[1];

                        int contentLength = 0;
                        string origin = "", acceptEnc = "", contentType = "", expect = "", cookie = "", xff = "", ua = "";
                        while ((line = reader.ReadLine()) != null && line.Length > 0)
                        {
                            int colon = line.IndexOf(':');
                            if (colon <= 0) continue;
                            string name = line.Substring(0, colon).Trim().ToLowerInvariant();
                            string value = line.Substring(colon + 1).Trim();
                            switch (name)
                            {
                                case "content-length": int.TryParse(value, out contentLength); break;
                                case "origin": origin = value; break;
                                case "accept-encoding": acceptEnc = value; break;
                                case "content-type": contentType = value; break;
                                case "expect": expect = value; break;
                                case "cookie": cookie = value; break;
                                // The core webui only answers browser-looking UAs (it
                                // 404s curl/8); the AOG side demands the opposite. So
                                // the auth relay carries the visitor's own UA through.
                                case "user-agent": ua = value; break;
                                case "x-forwarded-for": xff = value; break;
                                // Behind a Cloudflare tunnel this is the only honest
                                // visitor ip; the core's auth rate limits need it.
                                case "cf-connecting-ip": xff = value; break;
                            }
                        }

                        // node fetch and some HTTP clients hold the POST body until the
                        // server sends 100 Continue; answer it before reading, else deadlock.
                        if (contentLength > 0 && expect.Contains("100", StringComparison.OrdinalIgnoreCase))
                        {
                            writer.Write("HTTP/1.1 100 Continue\r\n\r\n");
                            writer.Flush();
                        }

                        byte[] body = Array.Empty<byte>();
                        if (contentLength > 0)
                        {
                            // Read through the SAME buffered reader: the header loop already pulled
                            // body bytes into its internal buffer, so a raw net.Read here would find
                            // an empty socket and hang until ReceiveTimeout while Forward relays nothing.
                            // AOG bodies are urlencoded (ASCII), so char->byte is byte-exact.
                            char[] chars = new char[contentLength];
                            int got = 0;
                            while (got < contentLength)
                            {
                                int n = reader.ReadBlock(chars, got, contentLength - got);
                                if (n <= 0) break;
                                got += n;
                            }
                            body = new byte[got];
                            for (int i = 0; i < got; i++) body[i] = (byte)chars[i];
                        }

                        string path = target.Split('?')[0];
                        try
                        {
                            bool isHtml = path.EndsWith(".html", StringComparison.OrdinalIgnoreCase);
                            if (method == "OPTIONS" && path.StartsWith("/aog/", StringComparison.Ordinal) && !isHtml)
                            {
                                Respond(writer, origin, 204, "No Content", null, null, null, "no-store");
                            }
                            else if (path.StartsWith("/aog/", StringComparison.Ordinal) && isHtml)
                            {
                                // A page under the /aog prefix (the shape the statics
                                // server uses too): gated or plain, never relayed as API.
                                if (Gate) HandlePage(writer, origin, cookie, path);
                                else ServeStatic(writer, origin, acceptEnc, path);
                            }
                            else if (path.StartsWith("/aog/", StringComparison.Ordinal))
                            {
                                if (Gate) HandleApi(writer, origin, method, target, body, contentType, cookie, xff);
                                else
                                {
                                    // Dev relay with an armed presenter: claimed/web-issued
                                    // accounts (a core card document exists - the plugin
                                    // checks that) log in on this lane too, and once they
                                    // are card-bound they answer /login with a presentation
                                    // or nothing. user_id comes from the relayed form body;
                                    // ids the deployment never issued are never bound.
                                    if (!string.IsNullOrEmpty(PresenterSecret) && (path == "/aog/login" || path == "/aog/create_player")
                                        && body != null && body.Length > 0 && body.Length < 16384)
                                    {
                                        string userId = FormValue(Latin1.GetString(body), "user_id");
                                        if (!string.IsNullOrEmpty(userId)) PresentCard(userId.Trim().ToUpperInvariant(), "dev-relay");
                                    }
                                    Forward(writer, origin, method, target, body, contentType, xff);
                                }
                            }
                            else if (Gate && (path == "/" || isHtml))
                            {
                                HandlePage(writer, origin, cookie, path);
                            }
                            else if (Gate && IsCorePage(path))
                            {
                                HandleAuth(writer, origin, method, target, body, contentType, cookie, xff, ua);
                            }
                            else
                            {
                                ServeStatic(writer, origin, acceptEnc, path);
                            }
                        }
                        catch (Exception e)
                        {
                            try { Respond(writer, origin, 502, "Bad Gateway", "text/plain", ToLatin1(UTF8Fallback.GetBytes("front error: " + e.Message)), null, "no-store"); } catch { }
                        }
                    }
                }
            }
            catch { }
        }

        private static readonly Encoding UTF8Fallback = new UTF8Encoding(false);

        private static void Respond(StreamWriter writer, string origin, int status, string reason, string contentType, string bodyLatin1, string contentEncoding, string cacheControl, string[] extraHeaders = null)
        {
            writer.Write("HTTP/1.1 ");
            writer.Write(status.ToString(CultureInfo.InvariantCulture));
            writer.Write(' ');
            writer.Write(reason);
            writer.Write("\r\nCross-Origin-Opener-Policy: same-origin\r\nCross-Origin-Embedder-Policy: require-corp\r\nAccess-Control-Allow-Origin: ");
            writer.Write(string.IsNullOrEmpty(origin) ? "*" : origin);
            writer.Write("\r\nAccess-Control-Allow-Methods: GET, POST, OPTIONS\r\nAccess-Control-Allow-Headers: content-type\r\n");
            if (contentType != null) { writer.Write("Content-Type: "); writer.Write(contentType); writer.Write("\r\n"); }
            if (contentEncoding != null) { writer.Write("Content-Encoding: "); writer.Write(contentEncoding); writer.Write("\r\n"); }
            if (extraHeaders != null)
                foreach (string h in extraHeaders)
                    if (!string.IsNullOrEmpty(h)) { writer.Write(h); writer.Write("\r\n"); }
            writer.Write("Cache-Control: ");
            writer.Write(string.IsNullOrEmpty(cacheControl) ? "no-store" : cacheControl);
            writer.Write("\r\nContent-Length: ");
            writer.Write((bodyLatin1 == null ? 0 : bodyLatin1.Length).ToString(CultureInfo.InvariantCulture));
            writer.Write("\r\nConnection: close\r\n\r\n");
            if (bodyLatin1 != null && bodyLatin1.Length > 0) writer.Write(bodyLatin1);
            writer.Flush();
        }

        private static string ToLatin1(byte[] b)
        {
            if (b == null || b.Length == 0) return null;
            char[] chars = new char[b.Length];
            for (int i = 0; i < b.Length; i++) chars[i] = (char)b[i];
            return new string(chars);
        }

        private static void ServeStatic(StreamWriter writer, string origin, string acceptEnc, string path)
        {
            string rel = path == "/" ? "index.html" : Uri.UnescapeDataString(path.TrimStart('/')).Replace('/', Path.DirectorySeparatorChar);
            string file = Path.GetFullPath(Path.Combine(Root, rel));
            if (!file.StartsWith(Root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            {
                Respond(writer, origin, 403, "Forbidden", "text/plain", "forbidden", null, "no-store");
                return;
            }
            string enc = null;
            if (acceptEnc != null && acceptEnc.Contains("br") && File.Exists(file + ".br")) { file += ".br"; enc = "br"; }
            if (!File.Exists(file))
            {
                Respond(writer, origin, 404, "Not Found", "text/plain", "not found", null, "no-store");
                return;
            }
            // Content-hashed build trees stay immutable; a plain .html page name is
            // served fresh so a relaunched play page never reads from a stale cache.
            string cache = file.EndsWith(".html", StringComparison.OrdinalIgnoreCase) || file.EndsWith(".html.br", StringComparison.OrdinalIgnoreCase)
                ? "no-store" : "public, max-age=31536000, immutable";
            Respond(writer, origin, 200, "OK", Mime(file), ToLatin1(File.ReadAllBytes(file)), enc, cache);
        }

        private static void Forward(StreamWriter writer, string origin, string method, string target, byte[] body, string contentType, string xff)
        {
            string pathAndQuery = target.StartsWith("/") ? target : "/" + target;
            string url = Upstream.TrimEnd('/') + pathAndQuery;
            HttpResponseMessage resp;
            try
            {
                using var mreq = new HttpRequestMessage(method == "GET" ? HttpMethod.Get : HttpMethod.Post, url);
                if (body != null && body.Length > 0)
                {
                    var content = new ByteArrayContent(body);
                    if (!string.IsNullOrEmpty(contentType)) content.Headers.TryAddWithoutValidation("Content-Type", contentType);
                    mreq.Content = content;
                }
                mreq.Headers.TryAddWithoutValidation("User-Agent", RelayUA);
                if (!string.IsNullOrEmpty(xff)) mreq.Headers.TryAddWithoutValidation("X-Forwarded-For", xff);
                resp = Up.SendAsync(mreq).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                string xml = "<root><serv_st><code>502</code><msg>" + WebUtility.HtmlEncode(e.Message) + "</msg></serv_st></root>";
                Respond(writer, origin, 502, "Bad Gateway", "text/xml; charset=utf-8", ToLatin1(UTF8Fallback.GetBytes(xml)), null, "no-store");
                return;
            }
            using (resp)
            {
                byte[] payload = resp.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
                string ct = resp.Content.Headers.ContentType?.ToString() ?? "text/xml";
                // AOG answers are per-session game state - caching one would let a
                // superseded pcuid replay last game's bytes (docs/28 defect fix).
                Respond(writer, origin, (int)resp.StatusCode, resp.ReasonPhrase ?? "OK", ct, ToLatin1(payload), null, "no-store");
            }
        }

        // --- gated (hosted) handlers: WEBFRONT_GATE=1 -------------------------

        // Paths the core webui owns; relayed with the session cookie in both
        // directions (Set-Cookie is how a login reaches the browser at all).
        private static bool IsCorePage(string path)
        {
            return path.StartsWith("/login", StringComparison.OrdinalIgnoreCase)
                || path.StartsWith("/signup", StringComparison.OrdinalIgnoreCase)
                || path.StartsWith("/logout", StringComparison.OrdinalIgnoreCase)
                || path.StartsWith("/about", StringComparison.OrdinalIgnoreCase)
                || path.StartsWith("/account", StringComparison.OrdinalIgnoreCase)
                || path.StartsWith("/static", StringComparison.OrdinalIgnoreCase)
                || path.StartsWith("/help", StringComparison.OrdinalIgnoreCase)
                || path.StartsWith("/plugin", StringComparison.OrdinalIgnoreCase)
                || path.StartsWith("/api/me", StringComparison.OrdinalIgnoreCase)
                || path.StartsWith("/favicon.ico", StringComparison.OrdinalIgnoreCase);
        }

        private static SessionInfo CheckSession(string cookie)
        {
            if (string.IsNullOrEmpty(cookie) || !cookie.Contains("connect.sid")) return null;
            long now = Environment.TickCount64;
            lock (SessionsLock)
            {
                if (Sessions.TryGetValue(cookie, out SessionInfo hit) && hit.UntilMs > now) return hit;
            }
            SessionInfo info = null;
            try
            {
                using var mreq = new HttpRequestMessage(HttpMethod.Get, AuthBase.TrimEnd('/') + "/api/me");
                mreq.Headers.TryAddWithoutValidation("Cookie", cookie);
                // Browser-looking UA: the core webui 404s non-browser agents.
                mreq.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (compatible; MFG-WebFront/1.0; session-gate)");
                using var resp = Up.SendAsync(mreq).GetAwaiter().GetResult();
                if (resp.StatusCode == HttpStatusCode.OK)
                {
                    string json = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    string user = JsonField(json, "username");
                    string card = JsonField(json, "cardNumber");
                    if (!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(card))
                    {
                        info = new SessionInfo { Username = user, Card = card, Admin = json.Contains("\"admin\":true"), UntilMs = now + 30000 };
                    }
                }
            }
            catch { }
            lock (SessionsLock)
            {
                if (info != null) Sessions[cookie] = info;
                else Sessions.Remove(cookie);
            }
            return info;
        }

        private static string JsonField(string json, string field)
        {
            string key = "\"" + field + "\":\"";
            int i = json.IndexOf(key, StringComparison.Ordinal);
            if (i < 0) return "";
            i += key.Length;
            int end = json.IndexOf('"', i);
            if (end < 0) return "";
            return json.Substring(i, end - i);
        }

        // Present this session's own DataID as its card credential, just before
        // the /login we relay (utils.ts presentViaPresenter consumes exactly one
        // stamp per login - so every relayed login presents, and nothing else
        // can). The plugin refuses without the matching VFG_PRESENTER_SECRET.
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
                bool ok = resp.StatusCode == HttpStatusCode.OK;
                if (!ok && card.Length == 16) // unissued/fixture ids are routinely refused here; keep the log honest but quiet
                {
                    string why = "";
                    try { why = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult(); } catch { }
                    Console.WriteLine("webfront presenter REFUSED " + label + " card=" + card + " status=" + (int)resp.StatusCode + " " + (why.Length > 200 ? why.Substring(0, 200) : why));
                }
                return ok;
            }
            catch (Exception e)
            {
                Console.WriteLine("webfront presenter error: " + e.Message);
                return false;
            }
        }

        private static void HandlePage(StreamWriter writer, string origin, string cookie, string path)
        {
            SessionInfo s = CheckSession(cookie);
            if (s == null)
            {
                Respond(writer, origin, 302, "Found", null, null, null, "no-store",
                    new string[] { "Location: /login?next=" + Uri.EscapeDataString(path) });
                return;
            }
            // Always splice from the canonical template: a stale index_play.html
            // left by the LAN scripts (with someone else's -DATAID) is never served
            // to a hosted session.
            string file = Path.GetFullPath(Path.Combine(Root, "index.html"));
            if (!File.Exists(file))
            {
                Respond(writer, origin, 404, "Not Found", "text/plain", "webroot index.html missing", null, "no-store");
                return;
            }
            string html = UTF8Fallback.GetString(File.ReadAllBytes(file));
            string pin = "\"-ENTERCARD\", \"-TRANSLATE\", \"-REGULAR\", \"-DATAID=" + s.Card + "\"";
            string page = html.Replace(", \"-AUTOSERVE\"", ", " + pin).Replace("\"-AUTOSERVE\", ", pin + ", ");
            if (page.Contains("-AUTOSERVE"))
            {
                Console.WriteLine("webfront: webroot index.html has no -AUTOSERVE anchor to splice - refusing to serve");
                Respond(writer, origin, 500, "Internal Server Error", "text/plain", "play page splice failed", null, "no-store");
                return;
            }
            page = page.Replace("http://127.0.0.1:8090/aog", PageFrontUrl);
            Respond(writer, origin, 200, "OK", "text/html; charset=utf-8", ToLatin1(UTF8Fallback.GetBytes(page)), null, "no-store");
        }

        private static void HandleApi(StreamWriter writer, string origin, string method, string target, byte[] body, string contentType, string cookie, string xff)
        {
            SessionInfo s = CheckSession(cookie);
            if (s == null)
            {
                string xml = "<root><serv_st><code>770</code><msg>web session required: sign in at /login first</msg></serv_st></root>";
                Respond(writer, origin, 403, "Forbidden", "text/xml; charset=utf-8", ToLatin1(UTF8Fallback.GetBytes(xml)), null, "no-store");
                return;
            }
            string path = target.Split('?')[0];
            if (path == "/aog/login" || path == "/aog/create_player") PresentCard(s.Card, "user=" + s.Username);
            Forward(writer, origin, method, target, body, contentType, xff);
        }

        private static void HandleAuth(StreamWriter writer, string origin, string method, string target, byte[] body, string contentType, string cookie, string xff, string ua)
        {
            string path = target.Split('?')[0];
            if ((path.StartsWith("/settings", StringComparison.OrdinalIgnoreCase) || path.StartsWith("/admin", StringComparison.OrdinalIgnoreCase))
                && !(CheckSession(cookie)?.Admin == true))
            {
                Respond(writer, origin, 403, "Forbidden", "text/plain", "admin only", null, "no-store");
                return;
            }
            string pathAndQuery = target.StartsWith("/") ? target : "/" + target;
            string url = AuthBase.TrimEnd('/') + pathAndQuery;
            try
            {
                using var mreq = new HttpRequestMessage(method == "GET" ? HttpMethod.Get : HttpMethod.Post, url);
                if (body != null && body.Length > 0)
                {
                    var content = new ByteArrayContent(body);
                    if (!string.IsNullOrEmpty(contentType)) content.Headers.TryAddWithoutValidation("Content-Type", contentType);
                    mreq.Content = content;
                }
                if (!string.IsNullOrEmpty(cookie)) mreq.Headers.TryAddWithoutValidation("Cookie", cookie);
                if (!string.IsNullOrEmpty(xff)) mreq.Headers.TryAddWithoutValidation("X-Forwarded-For", xff);
                mreq.Headers.TryAddWithoutValidation("User-Agent", string.IsNullOrEmpty(ua) ? "Mozilla/5.0 (compatible; MFG-WebFront/1.0)" : ua);
                using var resp = Up.SendAsync(mreq).GetAwaiter().GetResult();
                byte[] payload = resp.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
                string ct = resp.Content.Headers.ContentType?.ToString() ?? "text/html; charset=utf-8";
                var extras = new System.Collections.Generic.List<string>();
                if (resp.Headers.TryGetValues("Set-Cookie", out var cookies))
                    foreach (string v in cookies) extras.Add("Set-Cookie: " + v);
                if (resp.Headers.Location != null) extras.Add("Location: " + resp.Headers.Location.ToString());
                Respond(writer, origin, (int)resp.StatusCode, resp.ReasonPhrase ?? "OK", ct, ToLatin1(payload), null, "no-store", extras.ToArray());
            }
            catch (Exception e)
            {
                Respond(writer, origin, 502, "Bad Gateway", "text/plain", ToLatin1(UTF8Fallback.GetBytes("auth relay error: " + e.Message)), null, "no-store");
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
