# Spike S1: mock front server to lobby (first light)

Date: 2026-10-24. Status: PASS. The current arcade install boots `-KONAMINET`, logs in as a guest against our own mock front server, and reaches the lobby (Home scene) with zero proprietary components involved.

# Wiring

- Hosts entries (sandbox container `C:\Windows\System32\drivers\etc\hosts`, writable in this container):
  - `127.0.0.1 proxy-jp100.konami` (the hardcoded KONAMINET proxy, `NetEnvironmentProxy.cs:25-30`)
  - `127.0.0.1 aog03-stg.pvt.mo.konami.net` (`NetEnvironmentConnection.cs:40-48`)
- Mock: `mock/` .NET 8 console app, `dotnet run --project mock` from repo root. Listens `127.0.0.1:8080`, answers `CONNECT aog03-stg.pvt.mo.konami.net:443` with a TLS tunnel (in-memory self-signed RSA cert) and serves `POST /aog/<endpoint>` canned XML from `mock/specs/`. Every request body is logged to `mock/mock.log.txt`.
- Cert trust is a non-issue: `HttpWebRequestor.cs:82-93` returns true for every `SslPolicyErrors` value and `RequestManagerBase.cs:113-114` installs it globally.
- Launch: `spice64.exe -w -windowborder 2 -KONAMINET -k doorstop.dll` from the install root. `-KONAMINET` is consumed by `ResidentFlow.ApplyCommandLineArgs` (`ResidentFlow.cs:961-962`); spice2x logs unknown options but passes them through untouched in the process argv.

# Response contract (corrected during the spike)

- `serv_st` is mandatory in the response body: `ParseResultStatusCode` calls `NeedNotElement("serv_st")` outside the inner try (`RequestApiBase.cs:162`), so a payload without `serv_st` aborts endpoint parsing before the payload is read.
- `code` is read from `serv_st` only, and absent code yields 630 (seed value), not OK. Every template carries `<serv_st><code>0</code></serv_st>`.
- No endpoint is GET-only; `Reconnect` posts an empty body. Templates: 48 files, one per parser class plus `_default.xml`, derived from `MFG.GameRequest/*` with per-endpoint required-node notes in `mock/FINDINGS.md`.

# Verified guest chain (mock access log, one run)

`/create_player`, `/appli_info`, `/login`, `/get_menudata`, `/mission_date`, `/gacha_info`, `/client_state_read`, `/player_record` fire within ~70 ms of tapping guest start; `/client_state_write` follows character selection. The client parses every canned response (`resultcode:0` lines in the Unity log; `PlayerGameData Load` shows our template JSON).

Flow path observed after login (Unity scene log): `Login` -> `StoryPlay` (mandatory story intro; dialogue is local, zero requests; skip-all + confirm) -> `FirstCharaSelect` -> `Home` (lobby, guest profile, menu buttons). Screenshot series: `out/s1/shot*.png` plus `out/s1/burst/` (untracked).

# Observability tooling built during the spike

- The game kills Unity logging at boot (`ResidentFlow.cs:219`), and `DebugFlag.IsActive` is stubbed `return false` in the shipping build (`DebugFlag.cs:360-363`), so `output_log.txt` is engine-only by default.
- BepInEx 5.4.23.6 (stable) injects via SpiceDoorstop: extract `doorstop.dll` + `doorstop_config.ini` next to `spice64.exe`, add `-k doorstop.dll` (spice2x Inject-DLL-Hooks option). Repo: https://github.com/ainlorn/SpiceDoorstop. The BepInEx release zip for Unity Mono ships only managed core plus a `winhttp.dll` proxy with no native chainloader, so the plain proxy does not inject under spice2x; the `-k` hook is the working path.
- `mods/S1Freeze` plugin (deployed to `BepInEx/plugins/`): re-enables `Debug.unityLogger.logEnabled` after boot, force-enables `DebugFlag.Flag.EnableRequestAPIResponseLog` (full response XML in the Unity log), and skips `CountDownTimer.Update` so the countdowns never expire.
- The 20 s forced `logout` seen before the mod is the global `ResidentFlow.GetTimer()` countdown (default `m_startTime = 20f`, `CountDownTimer.cs:33`), armed by windows all over the flow graph; the login chain outran the parse but the flow bailed at timer zero. Freezing it turns the same chain into a stable lobby arrival.
- Input injection without spiceapi: `SetCursorPos` + `mouse_event` clicks act as touch (`Input.multiTouchEnabled = false`, Unity reads mouse-as-touch); attract prompt `(960,1010)`, guest start `(1349,932)`, story skip-all confirm `(1765,921)` then `(764,569)`, char select decide `(1270,975)`.

# Non-fatal findings

- `対応するコンテンツ情報がありません: GAME_MODE:JANDRI` warning from `GameUtility.cs:743/756` (`ToGameContents` throws for JANDRI) while pricing the payment window; harmless with our placeholder menudata.
- CREDIT keeps incrementing during KONAMINET sessions (billing no-op path); guest mode shows the payment window with ノーマルプレー = 1 CREDIT.
- Boot-stage clicks can kill the process (observed once during `Bio:Recovery`); interact only after `LoadScene-->Title` appears in the log.

# Open items

- Match loop: `gget`/`gpost` need a scripted command engine (KYOKUSTART / TSUMOCHOICES / SUTECHOICES), not static templates, before any match can play.
- Template enrichment: gacha info ids must resolve against client masters; menudata playmode values are placeholders.
- S2 ( <provider> capture) now has an easier runbook: same launch line without hosts pins and without `-KONAMINET`, plus `-netdump`.
