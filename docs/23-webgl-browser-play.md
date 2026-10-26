# 23. WebGL browser play — pipeline, settings, and proof recipe

Browser proof as of this doc: headless Chromium (Edge) loads the WebGL build from webfront,
boots through ResidentFlow, logs in over the KONAMINET front path against the mock (fronted by
webfront `/aog`), skips the intro story, reaches Home, picks a mode, enters matching
(entry_game/gget/gchat), and starts a kyoku in MahjongPlay with the mock match engine. The
proof harness is `tmp/webgl_proof.cjs`; the oracle is `mock/mock.log.txt`.

## One-shot recipe

1. Build (editor closed is fine; automation applies the WebGL player settings itself):
   `& 'C:\Program Files\Unity 2022.3.13f1\Editor\Unity.exe' -batchmode -quit -projectPath 'C:\mfg\build\unity-project' -executeMethod MfgRebuild.Gate2Content.BuildWebGLPlayer -logFile 'C:\mfgtmp\webglbuildN.log'`
   Watch `C:\mfgtmp\side.log.txt` for `webgl build result=Succeeded` and the `webgl exceptions=` line
   (must be `FullWithStacktrace`). Exit 0 = success, 1 = compile error (`error CS` in log), 5 = build failed.
2. Assemble webroot: `.\scripts\assemble_webroot.ps1` — stops any WebFront, copies the build,
   patches `index.html` (config.arguments) and `framework.js` (URL->argv), then runs sidecar.exe
   to brotli every StreamingAssets file (~3.8GB -> 3.4GB, ~5 min).
3. Serve: start `webfront\bin\Release\net8.0\WebFront.exe` with
   `WEBFRONT_ROOT=C:\mfgtmp\webroot`, `WEBFRONT_UPSTREAM=http://127.0.0.1:8080` (mock),
   `WEBFRONT_BIND=any`; logs to `C:\mfgtmp\wf.*.txt`. Mock (`mock\bin`) must run on :8080.
4. Proof: `node tmp/webgl_proof.cjs 'http://127.0.0.1:8090/?-KONAMINET&-FRONTURL=http://127.0.0.1:8090/aog&-GAMELOG&-NOIO&-appBehavior=NETCAFE&-AUTOSERVE' 1500000`
   from the repo root. Live log appends to `tmp/webgl_proof.log.txt`; JSON summary on stdout.

## WebGL player settings that are mandatory (2022.3.13f1)

AssetRipper-exported ProjectSettings ships every WebGL knob as 0/stub, so the build automation
(`Gate2Content.ApplyWebGLPlayerSettings`) sets them via reflection on the nested static class
`PlayerSettings.WebGL` (enum-safe SetValue):

- `webGLInitialMemorySize=512`, `webGLMaximumMemorySize=3584`, growth mode 2 (geometric) with
  step 8 / cap 512. With initial 0, emcc dies: `INITIAL_MEMORY must be larger than TOTAL_STACK, was 0`.
- `webGLExceptionSupport=FullWithStacktrace` (serialized value 3). With `None` the linker emits
  `function ___cxa_allocate_exception(){abort()}` stubs and every C# `throw` kills the wasm —
  it looks like an OOM abort but is not. Enum member names in 2022.3: `None`,
  `ExplicitlyThrownExceptionsOnly`, `FullWithoutStacktrace`, `FullWithStacktrace` (singular
  `Stacktrace`; the plural spellings people copy around do not exist).
- Compression disabled (sidecar brotli on the server does the job), template Default.

## Code paths that needed a browser branch

- `HttpWebRequestor.Request`: `HttpWebRequest.BeginGetRequestStream` throws `DllNotFoundException`
  on WebGL (threads/sockets absent). WebGL branch uses `UnityWebRequest` (uploadHandler=UploadHandlerRaw
  for POST, DownloadHandlerBuffer, timeout 60, `completed` callback) and fills the same
  `IsDone`/`Responce*` fields the existing request coroutine polls.
- `RebuildJst.Zone()`: `TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time")` throws
  `TimeZoneNotFoundException` on WebGL (no BCL tz database); custom +9 zone cached instead.
- `EXECEXE` + `ResidentFlow.ExecExeResidientFileLoad`: kernel32/pinvoke paths are skipped
  (`#if UNITY_WEBGL return`), otherwise `WaitForSingleObject` throws during ResidentFlow init.
- `ResidentFlow.InitializeOnBoot` silences `UnityEngine.Debug` at startup; `-GAMELOG` re-enables it.
  Any early echo must run after that re-enable or it never reaches the browser console.

## argv: the 2022.3 loader ignores config.arguments

The 2022.3 loader has no URL->argv mapping and `var arguments_=[]` in the built
`webgl.framework.js` is frozen at script load, so `-KONAMINET` from `config.arguments` never
reaches wasm (symptom: fresh `ScopeClose` at Logo because `IsKonamiNetConnection` is false).
`assemble_webroot.ps1` patches framework.js: the first `var arguments_=[]` becomes an IIFE that
parses `location.search` into `['-FLAG','-KEY','VALUE',...]` (honoring a pre-set `Module.arguments`).
Verified by the `WEBGLARGS` echo in the browser console; keep it as the canary line in every proof.

## webfront specifics

Keep-alive HTTP/1.1 (one loopback socket serves the whole addressable fan-out; without it Windows
occasionally fails burst GETs with `net::ERR_NO_BUFFER_SPACE`), `100 Continue` before reading POST
bodies (node fetch and Chromium XHR hold them otherwise), `.br`-aware MIME (strip `.br` before
classifying, else `index.html.br` arrives as octet-stream and the document aborts), brotli
sidecars for every bundle, COOP/COEP + UA swap for the provider proxy (see 22-browser-front.md).

## AUTOSERVE steering without a cfg file

The browser has no `C:\mfgtmp`, so `AutoServeBehaviour` bakes the working rule set in
(`WebglDefaultRuleLines`, still merged with the cfg file on desktop): story/guide skip, Home:
`BtReturn` closes the fresh-save character-custom window, `ModeButton` opens the mode window,
exact-name `Bt` (ModeBunner tiles) picks the default taku, `BtLowRate` answers the rate window,
`BtStart` confirms. Chara select uses the reflection `decide` builtin. Matching self-completes
against the mock (poll 2 reports 3 CPUs, ready cue deals the kyoku).

## Proof-harness specifics

- Headless Edge (`msedge.exe`, `headless:'new'`) with
  `--disable-background-timer-throttling --disable-renderer-backgrounding --disable-backgrounding-occluded-windows --disable-hang-monitor`:
  without them the wasm main loop collapses to the once-a-minute intensive throttling beat
  (visible as gchat spacing jumping from 1s to 61s at kyoku start).
- The harness installs an XHR retry shim (`evaluateOnNewDocument`): a status-0 failure on a
  `/StreamingAssets/` GET is retried up to 4 times. Windows loopback occasionally answers one
  burst socket with `ERR_NO_BUFFER_SPACE`; Unity trusts a single XHR outcome, so without a retry
  one flake strands the game in Boot for the whole run.
- `SharedArrayBuffer` feature flag + COOP/COEP headers on webfront are required for threads-mode
  wasm init.
- Timings to expect: boot+front stack ~90-130s, story ~7 min (skip spam works but story pages
  advance one-per-click), Home decisions instant once rules fire, matching ~20s, kyoku minutes.
  Use a 1500s window; do not add frontHits-based early breaks (gchat spam exceeds any count).
- Console slice cap is 1200 chars per line; scene timeline comes from grepping
  `AUTOSERVE poll active='X'` out of the live log.

## Gotchas ledger

- Never kill node processes by name on this box (the owner runs several); kill session PIDs.
- `webgl_proof.cjs` must stay CommonJS (`.cjs`): `require()` fails in `.mjs`.
- Run harnesses from the repo root; puppeteer resolves from `tmp/unity-activate/...node_modules`.
- The proof harness closes its browser on exit; leaked headless trees are identifiable by
  `remote-debugging` cmdline + creation time — kill only those.
- Mock is stateful per session (entry_game resets the match); a fresh browser run re-logs in as
  a fresh profile and replays story (~7 min) — budget window time accordingly.
