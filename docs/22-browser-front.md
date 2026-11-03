# 22 - Browser front (webfront)

Small .NET 8 static+proxy server that hosts the future WebGL build and speaks to the public
provider on its behalf. It exists because the provider cannot take browser traffic directly
(AOG middleware excludes Mozilla UAs, sends no CORS headers) while hosting a WebGL build needs a
static server with COOP/COEP anyway. Game logic stays on the provider; this is plumbing only.

## Behavior

- One origin on `http://127.0.0.1:8090` (port via `WEBFRONT_PORT`). Raw TcpListener, no http.sys/urlacl dependency; HTTP/1.1 keep-alive so the addressable fan-out reuses a handful of loopback sockets (burst connection churn intermittently trips Windows with `ERR_NO_BUFFER_SPACE`), `100 Continue` answered before reading POST bodies.
- `/aog/*` proxied to `WEBFRONT_UPSTREAM` (default `http://<provider-host>`), path+body+content-type
  relayed untouched, `User-Agent` replaced with a neutral agent string (the provider only rejects
  UAs containing "Mozilla"), `Host` set by the outbound URI. OPTIONS on /aog/* answers 204 preflight.
- Everything else served from `WEBFRONT_ROOT` (default exe-relative `webroot`; placeholder index
  while the build does not exist). Serves `.br` variants with `Content-Encoding: br` when the
  client accepts brotli. Immutable cache headers (content is content-hashed by Unity builds).
- All responses carry `Cross-Origin-Opener-Policy: same-origin` + `Cross-Origin-Embedder-Policy:
  require-corp` (Unity WebGL SharedArrayBuffer threading), plus permissive CORS for dev tooling.
  Same-origin by design means the built game itself needs no CORS.

## WebGL phase usage (Gate 3/4)

- Drop the Unity WebGL export at webroot root (index.html + Build/*.framework.js/.wasm/.data).
- Copy StreamingAssets content (video trees + `aa/` catalog + the 28k bundles staged in
  docs/21) under webroot/StreamingAssets so addressables lazy-load from the same origin; COEP is
  satisfied without CORS gymnastics because everything is same-origin.
  Caveat: those staged bundles are the shipped StandaloneWindows64 builds. Correct for
  editor/standalone play mode (same platform), but NOT directly usable by a WebGL player: bundle
  content is target-specific (desktop-compiled shaders, BC textures). The WebGL build automation
  regenerates the whole content set for the WebGL target during BuildPlayer (docs/23), so the
  exported webgl tree under `out/webgl` carries its own `aa/WebGL` bundles; `assemble_webroot.ps1`
  copies that tree and brotli-compresses every file for serving.
- The game's front-server URL uses the rebuild's `-FRONTURL <url>` command-line option (added to
  `ResidentFlow.ApplyCommandLineArgs` on top of shipping `-KONAMINET`; calls the shipped but unused
  `NetEnvironment.SetEnv` with the proxy hop off). Desktop examples below. On WebGL the 2022.3
  loader ignores `config.arguments` and freezes argv at script load, so `assemble_webroot.ps1`
  patches `framework.js` to build argv from the page URL query string:
  `http://127.0.0.1:8090/?-KONAMINET&-FRONTURL=http://127.0.0.1:8090/aog&...` configures the
  browser client with no per-build code change (absolute URL keeps it same-origin).
## First-payload sizing (measured 2026-10-25)

- Boot scene files are tiny (< 1 MB); nothing heavy in Resources. The shipped startup cost is the
  44.8 MB addressables `catalog.json` (init parses the whole catalog). Brotli-precompressed it streams
  ~4-6 MB and webfront already serves `.br` variants, so brotli sidecar generation for the WebGL
  content tree (catalog + regenerated bundles) is a prebuild step, not a code problem.
- All 28k bundles (4.9 GB desktop equivalent) are lazy-loaded per screen; a browser first-play only
  downloads the boot + title + lobby groups.

- Real hosting can later be any server with the same three properties (static, COOP/COEP,
  UA-swapping proxy); webfront is the reference implementation and the local dev server.

## Smoke evidence (2026-10-25, against live provider)
- `GET /` with Chrome UA: 200 with coop=same-origin, coep=require-corp. Random 4 KB binary served byte-exact (latin1 staging).
- `POST /aog/appli_boot` with Chrome UA + Origin header: 200 provider XML (UA rewrite works),
  Access-Control-Allow-Origin echoes origin, X-WebFront-Upstream: http://<provider-host>.
- `OPTIONS /aog/gget` preflight: 204 + ACAO.

## Run

```
dotnet build webfront -c Release
set WEBFRONT_ROOT=<path to webroot>
webfront\bin\Release\net8.0\WebFront.exe
```

Desktop rebuild launches (once the editor is licensed):

```
# play mode against the offline mock engine (same harness that proved S3)
Unity.exe -projectPath build\unity-project -FRONTURL http://127.0.0.1:8080/aog
# play mode straight against the public provider (Unity's UA passes their middleware; no hop, no hosts pin)
Unity.exe -projectPath build\unity-project -FRONTURL http://<provider-host>/aog
```
