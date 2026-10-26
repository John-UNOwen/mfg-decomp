using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

namespace MockServer;

// Mock front server for the decompiled MFG game in KONAMINET mode.
// Speaks the two shapes the game's HttpWebRequestor can produce through an HTTP proxy:
//   CONNECT host:port HTTP/1.1  -> 200 Connection established -> TLS (self-signed, client accepts anything) -> HTTP/1.1
//   POST https://host/aog/<endpoint> HTTP/1.1                 -> served directly on the socket
// Bodies come from mock\specs\<endpoint>.xml, falling back to mock\specs\_default.xml.
// Always answers Content-Length + Connection: close. One thread per connection.
internal static class Program
{
    private const string DefaultHost = "aog03-stg.pvt.mo.konami.net";
    private const string PfxPassword = "mock";
    private const string LogFileName = "mock.log.txt";
    private const string FallbackSpec = "_default";

    private static readonly object LogGate = new object();
    private static readonly CertHolder Cert = new CertHolder();
    private static readonly MatchEngine Engine = new MatchEngine();
    // Live S3 verification hooks: MFG_TEST_TEPAI gives seat 0 a fixed 13-kind hand,
    // MFG_TEST_WIN forces the first kyoku-1 draw to that kind (deal stays conserved).
    internal static readonly int[] TestTePai = ParseKindList(Environment.GetEnvironmentVariable("MFG_TEST_TEPAI"));
    internal static readonly int TestWinKind = int.TryParse(Environment.GetEnvironmentVariable("MFG_TEST_WIN"), out int wk) ? wk : -1;
    internal static readonly int TestSeat = int.TryParse(Environment.GetEnvironmentVariable("MFG_TEST_SEAT"), out int ts) ? ts : 0;
    // Forwarding mode: with MFG_UPSTREAM=http://host[:port] set, every request is relayed verbatim
    // to that front server (e.g. the publicly hosted provider) and the response passed back.
    // The local engine and spec templates stay unused. The listener keeps doing what the client
    // needs locally: proxy-hop request shapes, CONNECT+TLS, logging, and Host header rewriting.
    private static readonly string Upstream = Environment.GetEnvironmentVariable("MFG_UPSTREAM") ?? "";
    private static readonly System.Net.Http.HttpClient UpstreamClient =
        new System.Net.Http.HttpClient { Timeout = TimeSpan.FromSeconds(30) };
    private static int[] ParseKindList(string s) =>
        string.IsNullOrEmpty(s) ? null : s.Split(',').Select(int.Parse).ToArray();
    private static long SeqCounter;
    private static string SpecDir = "";
    private static string LogPath = "";

    private static int Main(string[] args)
    {
        if (args.Length > 0 && string.Equals(args[0], "selftest", StringComparison.OrdinalIgnoreCase))
            return SelfTest.Run();

        int port = 8080;
        if (args.Length > 0 &&
            (!int.TryParse(args[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out port) || port < 1 || port > 65535))
        {
            Console.Error.WriteLine("usage: MockServer [port | selftest]   (default 8080)");
            return 2;
        }

        ResolvePaths();
        try
        {
            Cert.Get();
        }
        catch (Exception ex)
        {
            Log("CERT_ERR " + ex.GetType().Name + ": " + ex.Message);
            return 1;
        }

        // MFG_BIND=any serves on all interfaces (needed when a guest VM or another
        // machine plays against this front); default stays loopback-only.
        var bind = Environment.GetEnvironmentVariable("MFG_BIND") == "any" ? IPAddress.Any : IPAddress.Loopback;
        Log("START port=" + port + " bind=" + bind + " specs=" + SpecDir + " log=" + LogPath + " san=" + Cert.SanNames);
        var listener = new TcpListener(bind, port);
        try
        {
            listener.Start();
        }
        catch (SocketException ex)
        {
            Log("BIND_FAIL 127.0.0.1:" + port + " " + ex.Message + " (port busy? pass another port as the first argument)");
            return 1;
        }
        Log("LISTEN " + listener.LocalEndpoint);
        while (true)
        {
            TcpClient client;
            try
            {
                client = listener.AcceptTcpClient();
            }
            catch (SocketException ex)
            {
                Log("ACCEPT_ERR " + ex.Message);
                break;
            }
            var thread = new Thread(() => HandleConnection(client)) { IsBackground = true, Name = "mock-conn" };
            thread.Start();
        }
        listener.Stop();
        return 0;
    }

    // ---------------- connection handling ----------------

    private static void HandleConnection(TcpClient client)
    {
        string remote = "?";
        try
        {
            using (client)
            {
                client.NoDelay = true;
                if (client.Client.RemoteEndPoint != null) remote = client.Client.RemoteEndPoint.ToString();
                NetworkStream net = client.GetStream();
                net.ReadTimeout = 120000;
                var reader = new LineReader(net);
                string firstLine = reader.ReadLine();
                if (string.IsNullOrEmpty(firstLine)) return;

                string[] line = firstLine.Split(' ');
                if (line.Length < 2)
                {
                    Log("BAD_REQUEST_LINE remote=" + remote + " line=" + firstLine);
                    return;
                }

                if (string.Equals(line[0], "CONNECT", StringComparison.OrdinalIgnoreCase))
                {
                    string authority = line[1];
                    ReadHeaders(reader);
                    byte[] established = Encoding.ASCII.GetBytes("HTTP/1.1 200 Connection established\r\n\r\n");
                    net.Write(established, 0, established.Length);
                    net.Flush();
                    string tunnelHost = HostFromAuthority(authority);
                    Cert.Register(tunnelHost);
                    Log("CONNECT_OPEN remote=" + remote + " authority=" + authority);
                    ServeTunnel(net, remote, tunnelHost);
                    Log("CONNECT_CLOSE remote=" + remote + " authority=" + authority);
                }
                else
                {
                    // The request line was already consumed by the CONNECT probe; hand it to the server.
                    ServeRequest(net, reader, remote, HostFromAuthority(line[1]), firstLine);
                }
            }
        }
        catch (Exception ex)
        {
            Log("CONN_ERR remote=" + remote + " err=" + ex.GetType().Name + ": " + ex.Message);
        }
    }

    private static void ServeTunnel(NetworkStream net, string remote, string tunnelHost)
    {
        using var ssl = new SslStream(net, false);
        try
        {
            ssl.AuthenticateAsServer(Cert.Get(), false, SslProtocols.None, false);
        }
        catch (Exception ex)
        {
            Log("TLS_ERR remote=" + remote + " host=" + tunnelHost + " err=" + ex.GetType().Name + ": " + ex.Message);
            return;
        }
        Log("TLS_OK remote=" + remote + " host=" + tunnelHost + " protocol=" + ssl.SslProtocol + " cipher=" + ssl.CipherAlgorithm);
        ServeRequest(ssl, new LineReader(ssl), remote, tunnelHost);
        try { ssl.Close(); } catch { }
    }

    // Reads one request (headers + body), answers it, and leaves the socket to the caller for close.
    // firstLine is the request line when the caller already had to read it to classify the connection.
    private static void ServeRequest(Stream stream, LineReader reader, string remote, string fallbackHost, string firstLine = null)
    {
        string requestLine = firstLine ?? reader.ReadLine();
        if (string.IsNullOrEmpty(requestLine)) return;

        string[] line = requestLine.Split(' ');
        if (line.Length < 2)
        {
            Log("BAD_REQUEST_LINE remote=" + remote + " line=" + requestLine);
            return;
        }
        string method = line[0];
        string target = line[1];

        Dictionary<string, string> headers = ReadHeaders(reader);
        string host = headers.TryGetValue("Host", out string hostHeader) ? HostFromAuthority(hostHeader) : "";
        if (host.Length == 0) host = fallbackHost;
        Cert.Register(host);

        byte[] rawBody = ReadBody(stream, headers);
        string body = rawBody.Length > 0 ? Encoding.UTF8.GetString(rawBody) : "";

        string specName = Route(target);
        long seq = Interlocked.Increment(ref SeqCounter);
        if (Upstream.Length > 0)
        {
            ForwardUpstream(stream, method, target, headers, rawBody, seq, host);
            return;
        }
        // Match-aware endpoints override their spec file as long as the session has a live match;
        // everything else (and every request without a session) keeps the canned template path.
        string xml = Engine.Handle(specName, body, spec => FillTemplate(LoadSpec(spec), seq, body))
                     ?? FillTemplate(LoadSpec(specName), seq, body);
        byte[] payload = Encoding.UTF8.GetBytes(xml);

        var head = new StringBuilder();
        head.Append("HTTP/1.1 200 OK\r\n");
        head.Append("Content-Type: text/xml; charset=utf-8\r\n");
        head.Append("Content-Length: ").Append(payload.Length.ToString(CultureInfo.InvariantCulture)).Append("\r\n");
        head.Append("Connection: close\r\n");
        head.Append("\r\n");
        byte[] headBytes = Encoding.ASCII.GetBytes(head.ToString());
        stream.Write(headBytes, 0, headBytes.Length);
        stream.Write(payload, 0, payload.Length);
        stream.Flush();

        Log("REQUEST seq=" + seq + " " + method + " host=" + host + " target=" + target +
            " spec=" + specName + ".xml bytes=" + payload.Length + " body=" + body);
    }

    // ---------------- routing ----------------

    // Relay one request to MFG_UPSTREAM and pass the reply through. Host header comes from the
    // upstream URL (vhost-correct); the local mock hop stays invisible to the client.
    private static void ForwardUpstream(Stream stream, string method, string target, Dictionary<string, string> headers, byte[] body, long seq, string host)
    {
        int status = 502;
        byte[] payload = Array.Empty<byte>();
        string ctype = "text/plain; charset=utf-8";
        try
        {
            string path = target;
            int scheme = path.IndexOf("://", StringComparison.Ordinal);
            if (scheme >= 0)
            {
                int slash = path.IndexOf('/', scheme + 3);
                path = slash < 0 ? "/" : path.Substring(slash);
            }
            var req = new System.Net.Http.HttpRequestMessage(new System.Net.Http.HttpMethod(method), Upstream.TrimEnd('/') + path);
            if (body.Length > 0)
            {
                var content = new System.Net.Http.ByteArrayContent(body);
                if (headers.TryGetValue("Content-Type", out string ct)) content.Headers.TryAddWithoutValidation("Content-Type", ct);
                req.Content = content;
            }
            var resp = UpstreamClient.SendAsync(req).GetAwaiter().GetResult();
            status = (int)resp.StatusCode;
            if (resp.Content != null)
            {
                payload = resp.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
                if (resp.Content.Headers.ContentType != null) ctype = resp.Content.Headers.ContentType.ToString();
            }
        }
        catch (Exception e)
        {
            Log("UPSTREAM_ERR seq=" + seq + " target=" + target + " err=" + e.Message);
        }
        var head = new StringBuilder();
        head.Append("HTTP/1.1 ").Append(status.ToString(CultureInfo.InvariantCulture));
        head.Append(status == 200 ? " OK\r\n" : " Bad Gateway\r\n");
        head.Append("Content-Type: ").Append(ctype).Append("\r\n");
        head.Append("Content-Length: ").Append(payload.Length.ToString(CultureInfo.InvariantCulture)).Append("\r\n");
        head.Append("Connection: close\r\n\r\n");
        byte[] headBytes = Encoding.ASCII.GetBytes(head.ToString());
        stream.Write(headBytes, 0, headBytes.Length);
        stream.Write(payload, 0, payload.Length);
        stream.Flush();
        Log("UPSTREAM seq=" + seq + " " + method + " host=" + host + " target=" + target +
            " status=" + status + " bytes=" + payload.Length + " body=" + body);
    }

    // /aog/<endpoint> -> <endpoint>; also matches a bare /<endpoint>, a /<endpoint>/ form,
    // and endpoints hidden inside a path segment (reconnect posts .../aog<gserv_url>reconnect/...).
    private static string Route(string target)
    {
        string path = target;
        int scheme = path.IndexOf("://", StringComparison.Ordinal);
        if (scheme >= 0)
        {
            int slash = path.IndexOf('/', scheme + 3);
            path = slash < 0 ? "/" : path.Substring(slash);
        }
        int query = path.IndexOf('?');
        if (query >= 0) path = path.Substring(0, query);
        int fragment = path.IndexOf('#');
        if (fragment >= 0) path = path.Substring(0, fragment);
        if (path.StartsWith("/aog", StringComparison.OrdinalIgnoreCase)) path = path.Substring(4);
        path = path.Trim('/');

        if (path.Length > 0 && IsSpec(path)) return path;
        foreach (string segment in path.Split('/'))
            if (segment.Length > 0 && IsSpec(segment)) return segment;
        // Longest name first so gget_stamp_info is not stolen by gget.
        foreach (string name in SpecNames().OrderByDescending(n => n.Length))
            if (path.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0) return name;
        return FallbackSpec;
    }

    private static bool IsSpec(string name)
    {
        return !string.Equals(name, FallbackSpec, StringComparison.OrdinalIgnoreCase) &&
               File.Exists(Path.Combine(SpecDir, name + ".xml"));
    }

    private static List<string> SpecNames()
    {
        var names = new List<string>();
        try
        {
            foreach (string file in Directory.GetFiles(SpecDir, "*.xml"))
            {
                string name = Path.GetFileNameWithoutExtension(file);
                if (!string.Equals(name, FallbackSpec, StringComparison.OrdinalIgnoreCase)) names.Add(name);
            }
        }
        catch
        {
            // spec dir vanished mid run; the caller falls back to _default
        }
        return names;
    }

    private static string LoadSpec(string name)
    {
        foreach (string candidate in new[] { Path.Combine(SpecDir, name + ".xml"), Path.Combine(SpecDir, FallbackSpec + ".xml") })
        {
            try
            {
                if (File.Exists(candidate)) return File.ReadAllText(candidate, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Log("SPEC_ERR " + candidate + " " + ex.GetType().Name + ": " + ex.Message);
            }
        }
        return "<res><serv_st><code>0</code></serv_st><code>0</code></res>\n";
    }

    // {{seq}} = per-request counter (gget/gpost/gchat templates use it for fetch windows and stamp ids).
    // {{user_id}} = the posted user_id, echoed by the login template.
    // Substitution skips XML comments so the explanatory comments in a spec keep showing the token.
    private static string FillTemplate(string xml, long seq, string body)
    {
        string userId = FormValue(body, "user_id");
        var sb = new StringBuilder(xml.Length + 32);
        int i = 0;
        while (i < xml.Length)
        {
            int open = xml.IndexOf("<!--", i, StringComparison.Ordinal);
            if (open < 0)
            {
                sb.Append(Substitute(xml.Substring(i), seq, userId));
                break;
            }
            sb.Append(Substitute(xml.Substring(i, open - i), seq, userId));
            int close = xml.IndexOf("-->", open + 4, StringComparison.Ordinal);
            if (close < 0)
            {
                sb.Append(xml.Substring(open));
                break;
            }
            sb.Append(xml, open, close + 3 - open);
            i = close + 3;
        }
        return sb.ToString();
    }

    private static string Substitute(string text, long seq, string userId)
    {
        text = text.Replace("{{seq}}", seq.ToString(CultureInfo.InvariantCulture));
        if (text.IndexOf("{{user_id}}", StringComparison.Ordinal) >= 0)
            text = text.Replace("{{user_id}}", XmlEscape(userId));
        return text;
    }

    // Percent-encoding is left as-is on purpose: values are logged raw and ids are ASCII in this mode.
    private static string FormValue(string body, string key)
    {
        foreach (string pair in body.Split('&'))
        {
            int eq = pair.IndexOf('=');
            if (eq <= 0) continue;
            if (string.Equals(pair.Substring(0, eq), key, StringComparison.OrdinalIgnoreCase))
                return pair.Substring(eq + 1);
        }
        return "";
    }

    private static string XmlEscape(string value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        var sb = new StringBuilder(value.Length + 8);
        foreach (char c in value)
        {
            switch (c)
            {
                case '&': sb.Append("&amp;"); break;
                case '<': sb.Append("&lt;"); break;
                case '>': sb.Append("&gt;"); break;
                case '"': sb.Append("&quot;"); break;
                case '\'': sb.Append("&apos;"); break;
                default: sb.Append(c); break;
            }
        }
        return sb.ToString();
    }

    // ---------------- naive HTTP/1.1 plumbing ----------------

    private static Dictionary<string, string> ReadHeaders(LineReader reader)
    {
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        while (true)
        {
            string line = reader.ReadLine();
            if (line == null || line.Length == 0) break;
            int colon = line.IndexOf(':');
            if (colon <= 0) continue;
            headers[line.Substring(0, colon).Trim()] = line.Substring(colon + 1).Trim();
        }
        return headers;
    }

    private static byte[] ReadBody(Stream stream, Dictionary<string, string> headers)
    {
        if (headers.TryGetValue("Transfer-Encoding", out string transferEncoding) &&
            transferEncoding.IndexOf("chunked", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return ReadChunkedBody(stream);
        }
        int length = 0;
        if (headers.TryGetValue("Content-Length", out string contentLength))
            int.TryParse(contentLength, NumberStyles.Integer, CultureInfo.InvariantCulture, out length);
        if (length <= 0) return Array.Empty<byte>();

        var buffer = new byte[length];
        int got = 0;
        while (got < length)
        {
            int read = stream.Read(buffer, got, length - got);
            if (read <= 0) break;
            got += read;
        }
        if (got == length) return buffer;
        var trimmed = new byte[got];
        Array.Copy(buffer, trimmed, got);
        return trimmed;
    }

    private static byte[] ReadChunkedBody(Stream stream)
    {
        using var ms = new MemoryStream();
        var reader = new LineReader(stream);
        while (true)
        {
            string sizeLine = reader.ReadLine();
            if (sizeLine == null) break;
            int semicolon = sizeLine.IndexOf(';');
            if (semicolon >= 0) sizeLine = sizeLine.Substring(0, semicolon);
            if (!int.TryParse(sizeLine.Trim(), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int size) || size <= 0) break;
            int remaining = size;
            var chunk = new byte[size];
            while (remaining > 0)
            {
                int read = stream.Read(chunk, chunk.Length - remaining, remaining);
                if (read <= 0) break;
                remaining -= read;
            }
            ms.Write(chunk, 0, chunk.Length - remaining);
            reader.ReadLine();
        }
        return ms.ToArray();
    }

    // Byte-at-a-time line reader: no read-ahead buffering, so it can share a stream with Stream.Read.
    private sealed class LineReader
    {
        private readonly Stream stream;

        public LineReader(Stream stream)
        {
            this.stream = stream;
        }

        public string ReadLine()
        {
            var sb = new StringBuilder();
            bool gotByte = false;
            while (true)
            {
                int c = stream.ReadByte();
                if (c < 0) return gotByte ? sb.ToString() : null;
                gotByte = true;
                if (c == '\n') break;
                if (c == '\r') continue;
                sb.Append((char)c);
            }
            return sb.ToString();
        }
    }

    private static string HostFromAuthority(string value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        string host = value;
        int scheme = host.IndexOf("://", StringComparison.Ordinal);
        if (scheme >= 0)
        {
            int slash = host.IndexOf('/', scheme + 3);
            host = slash < 0 ? host.Substring(scheme + 3) : host.Substring(scheme + 3, slash - scheme - 3);
        }
        int at = host.LastIndexOf('@');
        if (at >= 0) host = host.Substring(at + 1);
        if (host.StartsWith("[", StringComparison.Ordinal))
        {
            int close = host.IndexOf(']');
            if (close > 0) return host.Substring(1, close - 1);
        }
        int colon = host.LastIndexOf(':');
        if (colon >= 0) host = host.Substring(0, colon);
        return host.Trim();
    }

    // ---------------- paths and logging ----------------

    private static void ResolvePaths()
    {
        string cwd = Directory.GetCurrentDirectory();
        var candidates = new List<string>
        {
            Path.Combine(cwd, "specs"),
            Path.Combine(cwd, "mock", "specs"),
        };
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir != null && candidates.Count < 16; dir = dir.Parent)
        {
            candidates.Add(Path.Combine(dir.FullName, "specs"));
            candidates.Add(Path.Combine(dir.FullName, "mock", "specs"));
        }
        foreach (string candidate in candidates)
        {
            if (Directory.Exists(candidate))
            {
                SpecDir = Path.GetFullPath(candidate);
                break;
            }
        }
        if (string.IsNullOrEmpty(SpecDir))
        {
            SpecDir = Path.GetFullPath(Path.Combine(cwd, "specs"));
            try { Directory.CreateDirectory(SpecDir); } catch { }
        }
        string parent = Directory.GetParent(SpecDir.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))?.FullName ?? SpecDir;
        LogPath = Path.Combine(parent, LogFileName);
    }

    internal static void Log(string message)
    {
        string line = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fff'Z'", CultureInfo.InvariantCulture) + " " + message;
        lock (LogGate)
        {
            Console.WriteLine(line);
            try
            {
                File.AppendAllText(LogPath, line + "\n", Encoding.UTF8);
            }
            catch
            {
                // logging must never take the mock down
            }
        }
    }

    // ---------------- in-memory self-signed certificate ----------------

    private sealed class CertHolder
    {
        private readonly object gate = new object();
        private readonly SortedSet<string> hosts = new SortedSet<string>(StringComparer.OrdinalIgnoreCase) { DefaultHost };
        private X509Certificate2 current;
        private string signature = "";

        public string SanNames
        {
            get { lock (gate) { return string.Join(",", hosts); } }
        }

        public void Register(string host)
        {
            if (string.IsNullOrEmpty(host)) return;
            lock (gate) { hosts.Add(host); }
        }

        // Rebuilt when a new host shows up (CONNECT authority or Host header). The game ignores
        // every SslPolicyError, so this is about tidiness, not about the handshake succeeding.
        public X509Certificate2 Get()
        {
            lock (gate)
            {
                string signature = string.Join(",", hosts);
                if (current == null || signature != this.signature)
                {
                    byte[] pfx = BuildCertificate(hosts.ToList());
                    current?.Dispose();
                    // No EphemeralKeySet here: Schannel rejects it for server auth ("platform does not
                    // support ephemeral keys"), and the game only needs a throwaway TLS endpoint.
                    current = new X509Certificate2(pfx, PfxPassword, X509KeyStorageFlags.Exportable);
                    this.signature = signature;
                }
                return current;
            }
        }

        private static byte[] BuildCertificate(List<string> dnsNames)
        {
            using RSA rsa = RSA.Create(2048);
            var san = new SubjectAlternativeNameBuilder();
            foreach (string name in dnsNames) san.AddDnsName(name);
            var request = new CertificateRequest(
                "CN=" + dnsNames[0],
                rsa,
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1);
            request.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, false));
            request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment, false));
            request.CertificateExtensions.Add(san.Build());
            using X509Certificate2 selfSigned = request.CreateSelfSigned(
                DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(10));
            return selfSigned.Export(X509ContentType.Pfx, PfxPassword);
        }
    }
}
