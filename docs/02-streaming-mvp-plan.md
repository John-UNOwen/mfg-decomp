# 02 - Streaming MVP plan (interim product)

The browser plan we settled on while decomp research proceeds: run the real game server-side and stream it. Full background in the install's `browser-hosting-plan.md`; condensed here for the repo.

## Architecture

```
Browser -> Caddy (TLS + basic auth)
  /        -> panel: login, profile (server URL, card UID, PCB ID, PIN), Play/Stop, spiceapi buttons
  /stream  -> Moonlight Web (WebRTC relay) -> Sunshine -> spice64.exe session
```

- Host: Windows PC with GPU encode (NVENC/AMF/QSV). Sunshine app-mode launches `spice64.exe` with per-user args on a virtual display; [Moonlight Web](https://github.com/MrCreativ3001/moonlight-web-stream) streams to the tab; only port 443 is exposed.
- Per-user config maps 1:1 to spice2x options: `-url`, `-card0`, `-p`, `-pinmacro0` (+ autopin log trigger), `-mfgcabtype`, plus `-mfgargs` passthrough. Card/coin actions go through spiceapi (JSON over TCP 1337).
- Concurrency: 1 slot to start; scale with one Sunshine instance + virtual display per slot ([precedent](https://github.com/loki-47-6F-64/sunshine/issues/59)).
- Phases: 0 prove input+latency; 1 single-user MVP behind the domain; 2 accounts/profiles/spiceapi buttons/watchdog; 3 N slots; 4 tile-pad overlay UI.

## Why it coexists with this repo

Streaming ships in a weekend and keeps the domain useful while Phases 1-4 of [04-decompilation-plan](04-decompilation-plan.md) run. It also doubles as the test rig for the decomp (same server, same card, side-by-side comparison).
