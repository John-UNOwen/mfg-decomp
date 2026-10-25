# 22 - Browser front (webfront)

Small .NET 8 static+proxy server that hosts the future WebGL build and speaks to the public
provider on its behalf. It exists because the provider cannot take browser traffic directly
(AOG middleware excludes Mozilla UAs, sends no CORS headers) while hosting a WebGL build needs a
static server with COOP/COEP anyway. Game logic stays on the provider; this is plumbing only.

## Behavior

- One origin on `http://127.0.0.1:8090` (port via `WEBFRONT_PORT`).
- `/aog/*` proxied to `WEBFRONT_UPSTREAM` (default `http:// <provider-host>`), path+body+content-type
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
- The game's front-server URL points at same-origin `/aog/` (KONAMINET URL override in the
  rebuild, or hosts pin inside the browser is not possible - the WebGL rebuild must use a
  configurable URL; recorded in docs/21 follow-ups).
- Real hosting can later be any server with the same three properties (static, COOP/COEP,
  UA-swapping proxy); webfront is the reference implementation and the local dev server.

## Smoke evidence (2026-10-25, against live provider)

- `GET /` with Chrome UA: 200 with coop=same-origin, coep=require-corp.
- `POST /aog/appli_boot` with Chrome UA + Origin header: 200 provider XML (UA rewrite works),
  Access-Control-Allow-Origin echoes origin, X-WebFront-Upstream: http:// <provider-host>.
- `OPTIONS /aog/gget` preflight: 204 + ACAO.

## Run

```
dotnet build webfront -c Release
set WEBFRONT_ROOT=<path to webroot>
webfront\bin\Release\net8.0\WebFront.exe
```
