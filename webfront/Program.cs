using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace WebFront
{
    // Browser front for the WebGL rebuild (Gate 3/4 support piece).
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

        private static readonly HttpClient Up = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false }) { Timeout = TimeSpan.FromSeconds(60) };
        private static HttpListener Listener;

        private static int Main()
        {
            Directory.CreateDirectory(Root);
            Listener = new HttpListener();
            Listener.Prefixes.Add("http://127.0.0.1:" + Port.ToString(CultureInfo.InvariantCulture) + "/");
            try { Listener.Start(); }
            catch (HttpListenerException e) { Console.Error.WriteLine("bind failed: " + e.Message); return 1; }
            Console.WriteLine("webfront listening on http://127.0.0.1:" + Port + " root=" + Root + " upstream=" + Upstream);

            while (true)
            {
                HttpListenerContext ctx;
                try { ctx = Listener.GetContext(); }
                catch (Exception) { return 0; }
                ThreadPool.QueueUserWorkItem(_ => Handle(ctx));
            }
            return 0;
        }

        private static void Handle(HttpListenerContext ctx)
        {
            var req = ctx.Request; var res = ctx.Response;
            try
            {
                res.AddHeader("Cross-Origin-Opener-Policy", "same-origin");
                res.AddHeader("Cross-Origin-Embedder-Policy", "require-corp");
                res.AddHeader("Access-Control-Allow-Origin", req.Headers["Origin"] ?? "*");
                res.AddHeader("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
                res.AddHeader("Access-Control-Allow-Headers", "content-type");

                string path = req.Url.AbsolutePath;
                if (req.HttpMethod == "OPTIONS" && path.StartsWith("/aog/", StringComparison.Ordinal))
                {
                    res.StatusCode = 204; res.Close(); return;
                }
                if (path.StartsWith("/aog/", StringComparison.Ordinal))
                {
                    Forward(req, res, path).GetAwaiter().GetResult();
                    return;
                }
                ServeStatic(req, res, path);
            }
            catch (Exception e)
            {
                try
                {
                    byte[] eb = Encoding.UTF8.GetBytes("front error: " + e.Message);
                    res.StatusCode = 502; res.ContentType = "text/plain"; res.ContentLength64 = eb.Length;
                    res.OutputStream.Write(eb, 0, eb.Length); res.Close();
                }
                catch { }
            }
        }

        private static void ServeStatic(HttpListenerRequest req, HttpListenerResponse res, string path)
        {
            string rel = path == "/" ? "index.html" : path.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            string file = Path.GetFullPath(Path.Combine(Root, rel));
            if (!file.StartsWith(Root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(file, Root, StringComparison.OrdinalIgnoreCase))
            { res.StatusCode = 403; res.Close(); return; }

            string accept = req.Headers["Accept-Encoding"] ?? "";
            string enc = "";
            if (accept.Contains("br") && File.Exists(file + ".br")) { file += ".br"; enc = "br"; }
            if (!File.Exists(file)) { res.StatusCode = 404; res.Close(); return; }

            res.StatusCode = 200;
            res.ContentType = Mime(file);
            if (enc.Length > 0) res.AddHeader("Content-Encoding", "br");
            res.AddHeader("Cache-Control", "public, max-age=31536000, immutable");
            byte[] data = File.ReadAllBytes(file);
            res.ContentLength64 = data.Length;
            res.OutputStream.Write(data, 0, data.Length);
            res.Close();
        }

        private static System.Threading.Tasks.Task Forward(HttpListenerRequest req, HttpListenerResponse res, string path)
        {
            return ForwardAsync(req, res, path);
        }

        private static async System.Threading.Tasks.Task ForwardAsync(HttpListenerRequest req, HttpListenerResponse res, string path)
        {
            byte[] body = Array.Empty<byte>();
            if (req.HasEntityBody)
            {
                using var ms = new MemoryStream();
                req.InputStream.CopyTo(ms);
                body = ms.ToArray();
            }
            string target = Upstream.TrimEnd('/') + path + (req.Url.Query.Length > 0 ? req.Url.Query : "");
            using var mreq = new HttpRequestMessage(req.HttpMethod == "GET" ? HttpMethod.Get : HttpMethod.Post, target);
            if (body.Length > 0)
            {
                var content = new ByteArrayContent(body);
                if (req.ContentType != null) content.Headers.TryAddWithoutValidation("Content-Type", req.ContentType);
                mreq.Content = content;
            }
            mreq.Headers.TryAddWithoutValidation("User-Agent", RelayUA);

            HttpResponseMessage resp;
            try { resp = await Up.SendAsync(mreq).ConfigureAwait(false); }
            catch (Exception e)
            {
                byte[] eb = Encoding.UTF8.GetBytes("<root><serv_st><code>502</code><msg>" + WebUtility.HtmlEncode(e.Message) + "</msg></serv_st></root>");
                res.StatusCode = 502; res.ContentType = "text/xml"; res.ContentLength64 = eb.Length;
                res.OutputStream.Write(eb, 0, eb.Length); res.Close(); return;
            }
            byte[] payload = await resp.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
            res.StatusCode = (int)resp.StatusCode;
            res.ContentType = resp.Content.Headers.ContentType?.ToString() ?? "text/xml";
            res.AddHeader("X-WebFront-Upstream", Upstream);
            res.ContentLength64 = payload.Length;
            res.OutputStream.Write(payload, 0, payload.Length);
            res.Close();
        }

        private static string Mime(string f)
        {
            if (f.EndsWith(".html", StringComparison.OrdinalIgnoreCase)) return "text/html";
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
