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
                    client.ReceiveTimeout = 30000;
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
                        string origin = "", acceptEnc = "", contentType = "";
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
                            }
                        }

                        byte[] body = Array.Empty<byte>();
                        if (contentLength > 0)
                        {
                            body = new byte[contentLength];
                            int read = 0;
                            while (read < contentLength)
                            {
                                int n = net.Read(body, read, contentLength - read);
                                if (n <= 0) break;
                                read += n;
                            }
                        }

                        string path = target.Split('?')[0];
                        try
                        {
                            if (method == "OPTIONS" && path.StartsWith("/aog/", StringComparison.Ordinal))
                            {
                                Respond(writer, origin, 204, "No Content", null, null, null);
                            }
                            else if (path.StartsWith("/aog/", StringComparison.Ordinal))
                            {
                                Forward(writer, origin, method, target, body, contentType);
                            }
                            else
                            {
                                ServeStatic(writer, origin, acceptEnc, path);
                            }
                        }
                        catch (Exception e)
                        {
                            try { Respond(writer, origin, 502, "Bad Gateway", "text/plain", ToLatin1(UTF8Fallback.GetBytes("front error: " + e.Message)), null); } catch { }
                        }
                    }
                }
            }
            catch { }
        }

        private static readonly Encoding UTF8Fallback = new UTF8Encoding(false);

        private static void Respond(StreamWriter writer, string origin, int status, string reason, string contentType, string bodyLatin1, string contentEncoding)
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
            writer.Write("Cache-Control: public, max-age=31536000, immutable\r\nContent-Length: ");
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
                Respond(writer, origin, 403, "Forbidden", "text/plain", "forbidden", null);
                return;
            }
            string enc = null;
            if (acceptEnc != null && acceptEnc.Contains("br") && File.Exists(file + ".br")) { file += ".br"; enc = "br"; }
            if (!File.Exists(file))
            {
                Respond(writer, origin, 404, "Not Found", "text/plain", "not found", null);
                return;
            }
            Respond(writer, origin, 200, "OK", Mime(file), ToLatin1(File.ReadAllBytes(file)), enc);
        }

        private static void Forward(StreamWriter writer, string origin, string method, string target, byte[] body, string contentType)
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
                resp = Up.SendAsync(mreq).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                string xml = "<root><serv_st><code>502</code><msg>" + WebUtility.HtmlEncode(e.Message) + "</msg></serv_st></root>";
                Respond(writer, origin, 502, "Bad Gateway", "text/xml; charset=utf-8", ToLatin1(UTF8Fallback.GetBytes(xml)), null);
                return;
            }
            using (resp)
            {
                byte[] payload = resp.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
                string ct = resp.Content.Headers.ContentType?.ToString() ?? "text/xml";
                Respond(writer, origin, (int)resp.StatusCode, resp.ReasonPhrase ?? "OK", ct, ToLatin1(payload), null);
            }
        }

        private static string Mime(string f)
        {
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
