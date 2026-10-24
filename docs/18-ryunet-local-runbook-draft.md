# 18 — RyuNET (Asphyxia fork + mfg@asphyxia) local runbook — DRAFT

Status: code-read draft (no install/run performed; nothing committed). Clones inspected:
`tmp/RyuNET-core` (core, TS/Node), `tmp/asphyxia_plugins` (plugins, `mfg@asphyxia`).
Line citations point into those clones; our-side citations point into this repo.

Update 2026-10-24 (after this draft ran end to end): per owner, v1 uses the PUBLICLY hosted provider
(http:// <provider-host>, AOG front on :80 /aog/, same stack) instead of self-hosting; the local boot
below was verified working (server started, AOG 200s, client booted to it). For the desktop game the
repo mock runs in forwarding mode (`MFG_UPSTREAM=http:// <provider-host>`, commit d8cb710): it keeps the
hosts-pin/proxy hop, fixes the Host header the vhost needs, logs every exchange, relays the rest.

## Verdict

**Usable as-is: YES, with three non-code adjustments** — (1) a TLS front (the client hardcodes
`https://`; RyuNET is plain HTTP), (2) the plugin folder staged into the core's `plugins/` dir
under the right cwd, (3) `config.ini` for our case. No TypeScript patch needed for the cardless
AOG guest loop. Node here is v24.20.0 (≥22.5 required, `node:sqlite` verified working
unflagged on this box). Do **not** use the old pkg `.exe` releases: CI still builds them with a
Node 16 runtime (RyuNET-core `.github/workflows/build.yml:19`), which has no `node:sqlite`.

## 1. Prerequisites

- Node ≥ 22.5 for `node:sqlite` (core `package.json` `engines`, README "Requirements").
  Checked on this box: `node --version` → v24.20.0, `npm --version` → 11.19.0. `--experimental-sqlite`
  is accepted-but-unneeded on 24.
- No DB server: SQLite via `node:sqlite` (`src/utils/SqliteStore.ts:21`). Fresh start = just create
  `savedata/` on first boot (path `SAVE_PATH = cwd/savedata`, `EamuseIO.ts:33`).
- npm deps include prebuilt natives only (`@napi-rs/canvas`, no node-gyp expected). Core clone has
  **no `node_modules` and no `dist` checked in** — install + build are mandatory.
- The core ships **TypeScript source only**; no esbuild. It compiles with `tsc` (`module: node16`,
  `outDir: dist`, `tsconfig.json`) and runs plugins through **ts-node** (transpileOnly by default)
  — `src/eamuse/ExternalPluginLoader.ts:104-115`. No bundling step for plugins.

## 2. Install + first run (PowerShell, from core clone root)

```powershell
cd '<repo>\tmp\RyuNET-core'
npm install
npm run build                     # tsc -> dist/

# Stage the runtime layout. CRITICAL: cwd must be dist/ — plugins, config.ini and savedata
# resolve against process.cwd() (ArgConfig.ts:9-10, EamuseIO.ts:28-33) and assets resolve
# to ../build-env/assets relative to cwd (EamuseIO.ts:31). Everything lives under dist/.
New-Item -ItemType Directory -Force dist\plugins | Out-Null
Copy-Item plugins\tsconfig.json,plugins\asphyxia-core.d.ts dist\plugins\
Copy-Item -Recurse ..\asphyxia_plugins\mfg@asphyxia dist\plugins\
# (plugin folder needs no package.json/node_modules: ts-node resolves index.ts via require
#  extensions; mfg imports are builtins + relative files only — index.ts, handlers/aog.ts)

# Write config.ini first (section below), else the server writes defaults into dist/config.ini
# on boot via SaveConfig() (AsphyxiaCore.ts:66, ArgConfig.ts:334) — comments get stripped.

cd dist
node AsphyxiaCore.js
```

First-boot console: Asphyxia banner, `Default admin account created (username: admin, password: admin)`
(`EamuseIO.ts:525-538`), `[VFG] Native AOG HTTP Server listening on 0.0.0.0:22421` (plugin
`index.ts:194-196`), `Listening - 127.0.0.1 at 8083`. Skip `--dev` on first run (it turns on
ts-node full typecheck → slow/type-noise; plugin log visibility comes from `VFG_VERBOSE`, not
`--dev`: handlers/utils.ts:94-108).

Smoke test without the game:

```powershell
Invoke-WebRequest -UseBasicParsing http://127.0.0.1:8083/ -UserAgent curl/8 | % Content   # "VFG local server ok via RyuNET" (AogMiddleware.ts:93-113)
Invoke-WebRequest -Method Post -UseBasicParsing http://127.0.0.1:8083/aog/appli_boot | % Content   # <root><serv_st><code>0 ... <boot_mes> XML
```

## 3. Routing / ports (what serves what)

- **Core port** (`port`, default 8083): express mounts `AogGetMiddleware`, `AogMiddleware` **before**
  the e-amusement wildcard (`AsphyxiaCore.ts:79-81`). AOG middleware treats a request as AOG if the
  URL starts with `/aog` **or** the last path segment is in a fixed `KNOWN_AOG` name set
  (`AogMiddleware.ts:20-38`); the last segment (minus `aog/` prefix) is the handler name
  (`gameApiName`, `:13-18`). So `POST /aog/gget` = `POST /gget` on 8083. **Gotcha:** names outside
  `KNOWN_AOG` (e.g. `cutin_gacha_play_draw`, in `AOG_HANDLER_MAP` aog.ts:1837-1838 but absent from
  the core's set) 404 via the eamuse catch-all when hit at the root — always keep the `/aog` path
  prefix on the core port (the plugin's own comment says exactly this: eamuse.ts:491-499).
- **Native AOG server, port 22421** (`mfg_http_port`): the plugin **always** starts a second plain
  `http.createServer` bound to `0.0.0.0:mfg_http_port` inside `register()` (`index.ts:114-203`),
  dispatching its own `AOG_HANDLER_MAP` by last path segment (root-style `/gget` **and**
  `/aog/gget` both fine, `index.ts:130-135`). EADDRINUSE only logs, server keeps running (198).
- `mfg_service_url` only shapes what **eAC3 `vfgac.service_list`** returns to card/emulator-route
  clients (`eamuse.ts:481-528`): empty → `http://<host>:22421/aog/`; set → that URL, with `/aog/`
  appended if given as a bare root. It does **not** move any listener; pick integrated
  (`http://<host>:8083/aog`) or native (default 22421) per deployment.
- `matching_port` (5700) is a PCB/eAC3 concept only: it surfaces as `portfw` in `facility.get`
  (`eamuse.ts:178-213`, core `Facility.ts:9`). The Unity client's VFG match flow never uses it —
  matching is pure HTTP `entry_game` + `gget` long-poll against the front/game URL
  (docs/15 §1.1, gserv_url used only by the never-called Reconnect, docs/15:744,809-810).

## 4. Config for our dev case

`dist/config.ini` — plugin keys land in a `[mfg@asphyxia]` section (`ArgConfig.ts:291-299`); both
flat core keys and plugin sections are editable live in the WebUI (admin/admin, `/settings`;
webui routes `src/webui/routes/settings.ts`, `plugin.ts`). CLI flags `-p/-b` override the file
(`ArgConfig.ts:342-354`).

```ini
port=8083                      ; keep off 8080 (our TLS mock front owns it; see §6)
bind=127.0.0.1                 ; 0.0.0.0 for LAN cabinets
ping_ip=127.0.0.1
matching_port=5700             ; unused by VFG; keep default
allow_register=true            ; gates WebUI signup only, NOT AOG guests
maintenance_mode=false
enable_paseli=false
webui_on_startup=false         ; otherwise a browser pops on every boot (win32 default true)
server_name=RyuNET local
server_tag=VFG-LOCAL
require_pcbid_auth=false       ; only checked for e-amusement XRPC bodies carrying a pcbid
                               ; (EamuseMiddleware.ts:273-281); pure AOG ignores it entirely

[mfg@asphyxia]
mfg_service_url=http://127.0.0.1:8083/aog   ; integrated; empty = native :22421
mfg_http_port=22421
VFG_VERBOSE=true               ; per-request/per-player VFG logs (default false)
VFG_EVENT_TAKU=min             ; min|off|all event-table sets (index.ts:21-27)
VFG_DISCORD_WEBHOOK=           ; lobby announcements; empty = off
VFG_GACHA_ALL=false
VFG_CARDMNG_MODE=compat        ; card id handling (PCB/card route only)
VFG_HIDE_GACHA=
VFG_GACHA_SAFE=false           ; + VFG_GACHA_CHARAS / VFG_GACHA_EXCLUDE: gacha freeze bisection knobs
```

All `VFG_*` keys are declared in `mfg@asphyxia/index.ts:21-109`; read via `U.GetConfig` which reads
the `[mfg@asphyxia]` section of config.ini only (ExternalPluginLoader.ts:170-176). Some are set at
load time (native server port), so port changes need a restart.

## 5. Cardless AOG-only identity (what login/create_player/entry actually require)

- `login` accepts any of `user_id|dataid|refid|ref_id` (uppercased → profile refid), falls back to
  a shared `GUEST` refid; mints a random 32-hex `session_id` which **is** the client's `pcuid`
  (aog.ts:361-403; docs/15 §1.1 row "pcuid == login session_id"). Unknown refid →
  `ensureProfile` auto-creates a DB profile with a derived `player_id` (utils.ts:318-354). No card,
  no PCBID, no password, **no admin gate, no version check anywhere in the AOG handlers**
  (grep "version" finds nothing in the plugin). `guest=1` only flags `is_guest`.
- `create_player` stores the chosen name onto the same profile (resolves via session when the
  client sends only pcuid+name, aog.ts:411-453). `get_menudata` returns real mid/name + all 23
  playmodes from the profile (aog.ts:456-473) — richer than our mock's placeholders.
- `entry_game`: 2-seat modes (gmode 4/Nima… see `GMODE_SEATS`) enter an **instant private table**
  seat 0 human + rest CPU (aog.ts:902-911). 3/4-seat public lobbies need every seat filled **or a
  60 s wall** elapsed while `gget` is being polled, then a table is built with humans + CPU for the
  rest (aog.ts:1087-1095, ensureSharedTable 207-216; `taikyoku.ts:170-173` — Table(human_seats)
  auto-CPU the others). Disconnected humans convert to CPU after 60 s (gget 1113-1118). No config
  knob for the 60 s. Passphrase match = private lobby keyed `gmode_<passphrase>` (861).
- `end_game` archives results + full cell stream (replayable via `get_haifu_*`), kiken is
  not archived (aog.ts:1217-1268).
- Daily JST login bonus present is appended at login and auto-applied client-side
  (aog.ts:376-399) — a small parity feature our mock lacks; expect one present on first `client_state_read`.

## 6. Pointing the game at it (hosts + the TLS reality)

The client hardcodes **https** in both connection profiles: `DefaultKonamiNet` =
`https://aog03-stg.pvt.mo.konami.net/aog` via CONNECT proxy `proxy-jp100.konami:8080`, and plain
`Default` = `https://rproxy0400.ea.konami.net/aog` direct (api/recovered Assembly-CSharp
`Konami.System.Net/NetEnvironmentConnection.cs:20-48`, `NetEnvironmentProxy.cs:25-30`).
RyuNET listens plain HTTP. So the **first hop always needs TLS termination** — the good news is the
client trusts any cert unconditionally (docs/12 "Cert trust is a non-issue").

**Route A — keep our existing mock front (recommended first run, preserves the proven -KONAMINET
guest chain).** Hosts entries unchanged (`127.0.0.1 proxy-jp100.konami`,
`127.0.0.1 aog03-stg.pvt.mo.konami.net`; docs/12 "Wiring"). Mock stays on `127.0.0.1:8080` doing
TLS/CONNECT, but flips from canned XML to **reverse-proxying every request to
`http://127.0.0.1:8083`** (integrated AOG on the core port; `mfg_service_url` irrelevant on this
route since FrontServerUrl stays the pinned https URL, docs/15 §1.1). RyuNET keeps 8083 so both
coexist. Verify: the real `appli_boot` embeds `moserv_url`/`boot_mes` (aog.ts:335-349) and
`entry_game` embeds `gserv_url=http://<request-host>:8083/aog/` (aog.ts:994-1018) — if the client
follows `moserv_url` off the https front, have the mock proxy rewrite/strip it (our canned
appli_boot already passed this gate, docs/12 "Verified guest chain"). `gserv_url` is inert for
this client (no call site; docs/15:744, 809-810).

**Route B — community shape (plain HTTP for game traffic, closer to  <provider> live).** Hosts:
`127.0.0.1 rproxy0400.ea.konami.net`. Run Caddy (or any TLS terminator) on `:443` reverse-proxying
`https://rproxy0400.ea.konami.net → http://127.0.0.1:8083`, launch **without** `-KONAMINET`. eAC3
`vfgac.service_list` then hands the client `http://rproxy0400.ea.konami.net:22421/aog/` (default)
or your `mfg_service_url`, and all gget/gpost traffic is plain HTTP to the native server —
exactly the live pattern captured in docs/15 §1.1 (`http:// <provider-host>/aog/`). Requires
`require_pcbid_auth=false` (the emulator route prefixes `system_id=<PCBID>&sic=…`, docs/15:33, and
registered-cabinet enforcement would 403 unknown PCBIDs; EamuseMiddleware.ts:275-279). Cardless
guest on this route: not yet observed locally — verify.

No e-amusement web-interface domains are required for AOG-only play; the only URLs the game uses
are the front/game URL plus `ping_ip` keepalive text probes (`services.get` keepalive item,
core `eamuse/index.ts:86-92`; GET `/core/keepalive` answered, AogMiddleware.ts:107-110).

## 7. Expected first boot for a fresh identity

1. `POST /aog/appli_boot` → reviewed_version false, boot_mes with moserv_url (aog.ts:335-349).
2. `POST /aog/login` (guest) → fresh 32-hex `session_id` → new profile row in
   `savedata/<plugin>/profile.db`, `name=GUEST` (Route A) or the client's persistent user id
   (Route B, when the body carries one) — aog.ts:361-403, utils.ts:318-354.
3. Same ~70 ms chain as docs/12 "Verified guest chain": `/create_player` `/appli_info`
   `/get_menudata` `/mission_date` `/gacha_info` `/client_state_read` `/player_record`, then
   `/client_state_write` on char select — all have real handlers (aog.ts:1812-1908).
4. Story intro + char select locally (no requests, docs/12); lobby = Home with real playmode list.
5. Match: entry 2P → instant CPU table, `gget` polls return `all_ready` once ready=1 and stream
   tai cells (aog.ts:1169-1181; contract verified against docs/15 must-slot layout: gget slots 2/5
   = tid/nextSeqNo aog.ts:1074-1076; gpost slots 3,6,9-13 = pindex/kind/pai/tepai/tepai2/reach/
   tsumogiri aog.ts:1184-1205).

## 8. Risks / open items (ranked)

1. **TLS bootstrap unavoidable** — both client profiles are https-only
   (NetEnvironmentConnection.cs:26,46) vs plain-HTTP RyuNET (AsphyxiaCore.ts:97). Hosts-pinning
   the core port directly cannot work without a TLS/CONNECT front (existing mock or Caddy).
2. **Bare-root 404 trap on the core port** — AOG names missing from the core's `KNOWN_AOG` set
   (e.g. `cutin_gacha_play_draw`) only resolve under `/aog/...` (AogMiddleware.ts:20-38 vs
   aog.ts:1812-1908; plugin comment eamuse.ts:491-499 records the live 404). Always include the
   `/aog` path in any core-port service URL. Native 22421 binds unconditionally and may clash
   silently (index.ts:114-203).
3. **Shared `GUEST` profile on Route A** — a login body with no refid collapses every anonymous
   client into refid `GUEST` (aog.ts:363-371): fine for a single test loop (persistence still
   works, sessions rotate), wrong for multi-client. Route B / real user ids avoid it.
4. **Solo CPU fill differs from our mock** — instant only in 2-seat modes; 3/4-seat tables need a
   full lobby or the hardcoded 60 s poll window (aog.ts:902-911, 1087-1095). Our loop script must
   either use 2P modes or wait/poll ≥60 s; no knob. (2P instant fill = README line "CPU opponents
   (instant solo tables in 2-player mode)".)
5. **Future WebGL client: blocked today** — no `Access-Control-*` headers and no OPTIONS/preflight
   handling anywhere (grep across core+plugin: zero hits; AogMiddleware.ts:28-90 sends via
   `res.send` only), and browser UAs are explicitly **excluded** from AOG handling
   (`agent.includes('Mozilla') → next()`, AogMiddleware.ts:32-33,100-102). Serve the WebGL build
   same-origin (e.g. under `build-env/assets/static/` → `/static/...`, AsphyxiaCore.ts:82) or add
   CORS middleware at the front; a cross-origin fetch from another port currently 404s.
6. Loader/runtime edges: ts-node@9 transpile path on Node 24 (untested at runtime here — verify on
   first boot; `--dev` switches to full typecheck and needs type packages in the plugins tree,
   ExternalPluginLoader.ts:104-115 + plugins/tsconfig.json); CI/pkg releases are Node 16 runtime
   (build.yml:19, engines:>=22.5) → exe artifacts unreliable, use `node dist/AsphyxiaCore.js`;
   `appli_info`/gacha pools load `data/gacha_pools.json` via require with fallbacks
   (utils.ts:509-523) — ship the folder intact; vfgac/vfglog are NOT advertised in eAC3
   `services.get` (no `ExtraModuleHandler` in mfg plugin; core `eamuse/index.ts:24-113` does
   extra modules only when a plugin registers them) — live server works without it, but note it if
   a client build starts relying on service discovery; config.ini is rewritten (comments stripped)
   on every boot (ArgConfig.ts:334-340).
7. Non-implemented parity vs our mock expectations: no friends/block list, no odekake lending
   (hard-disabled, aog.ts:1868-1873), no MG/jongstone economy, no pro opponents, match viewer
   limited to archived tables (plugin README:21). `get_menudata` playmode pricing may surface the
   same harmless JANDRI warning (docs/12 "Non-fatal findings").

## 9. Do / don't

- Do keep both clones pristine; stage plugins into `dist/plugins/` (copies).
- Do check `savedata/` grew `mfg@asphyxia/profile.db` after first guest login; WebUI at
  `http://127.0.0.1:8083` (admin/admin — change the password; core README "Change the default
  admin password").
- Don't run the core cwd at repo root expecting plugins to load (`PLUGIN_PATH = cwd/plugins`,
  EamuseIO.ts:28-30); don't expect the plugin to typecheck clean under `--dev` without staging
  `asphyxia-core.d.ts` + `tsconfig.json` in the plugins dir.
- Don't expose 22421 or the WebUI on LAN without changing the admin password (native server binds
  0.0.0.0, index.ts:194).
