# Linux lane kit (`scripts/linux/`)

Linux-side replacement for the Windows lane scripts. It never touches the Windows
lane: every script here either targets a Linux box or scratch paths you point it at,
nothing in the kit signals a process it did not record itself, and the core/gate
refuse to start on a port that already answers (so a running lane is never stepped
on). The Windows lane (core 8083, gate 8090, probe relay 8091, static front 8095)
stays untouched by design; the defaults below assume a **separate Linux box** with
the same port numbers.

Already verified earlier: `assemble_webroot.sh` (the bash port of
`assemble_webroot.ps1`). This kit adds the start/stop/smoke/transfer side.

## Files

| File | Purpose |
|---|---|
| `lane_up.sh` | Starts core + gate + cloudflared without systemd: port-probes first (refuses occupied ports, never reclaims by process name), exports exactly the env the two programs read, reads `~/.mfg-web` secrets at mode 600 without printing them, copies `config.ini` before rewriting `mfg_http_bind=127.0.0.1` (only while the core is not listening), records pid files, polls bounded (0.5 s steps) and fails loudly with the error-log tail. `--dry-run` prints every command + the full environment with secrets redacted. Replaces `hosting_up.ps1`. |
| `lane_down.sh` | Stops exactly the pids `lane_up.sh` recorded in `PID_DIR`, verifying each pid's `/proc/<pid>/cmdline` before signalling. Never kills by name/pattern/port, never pid 1 or itself, reports every refusal and why. Replaces `hosting_down.ps1`. |
| `mfg-lane.service`, `mfg-gate.service`, `mfg-tunnel.service` | systemd unit **templates** (`@PLACEHOLDER@` tokens) with `WorkingDirectory`, `EnvironmentFile`, `Restart`, `TimeoutStopSec`, `NoNewPrivileges`, `ProtectSystem=strict` and `ReadWritePaths` lines, each annotated where its path requirement comes from. |
| `install_systemd.sh` | Substitutes real paths/user into the unit templates, stages the results plus a non-secret `lane.env` into a staging directory, and prints the exact `sudo` commands a human runs. It never invokes sudo itself. |
| `lane_smoke.sh` | Read-only curl acceptance suite against a base URL: COOP/COEP headers, brotli sidecar rule, immutable-vs-TTL caching, `index.html` no-store, FoxNET login page, `/static` assets, anonymous session rejection, `/aog` reaching the core handler, play page `-AUTOSERVE`, and an optional login/logout section (`SMOKE_USER`/`SMOKE_PASS`). Prints PASS/FAIL/SKIP with evidence and a summary count; non-zero exit on any FAIL. |
| `transfer_build.sh` | Ships the WebGL build over ssh in chunked tars (no rsync), one chunk per top-level directory, resumable only in the "skip complete chunks by sha256" sense, with a manifest and a hard refusal to delete anything remote. `--dry-run` shows the plan + manifest. |
| `verify_transfer.sh` | Compares the manifest against the remote tree: missing / extra / size-or-hash mismatched files plus file and byte totals, so a 5.47 GB transfer is confirmed, not assumed. |

## Run order for a parallel install (Windows lane keeps serving)

Do all of this **on the Linux box**; the Windows lane is not involved at any step.

1. `transfer_build.sh --dry-run` -> review chunks/manifest; then real run; then
   `verify_transfer.sh` until it reports agreement.
2. `assemble_webroot.sh` on the Linux box (assembles the lane webroot from the
   transferred build; needs the brotli sidecar tool + a .NET 8 runtime).
3. `lane_up.sh --dry-run` -> sanity-check paths/env/ports; then `lane_up.sh`.
4. `lane_smoke.sh` -> the acceptance suite. With credentials:
   `SMOKE_USER=... SMOKE_PASS=... lane_smoke.sh`.
5. `lane_down.sh` whenever the lane has to move.
6. Optional permanent form: `install_systemd.sh /path/to/repo --user <svcuser>`,
   review the staging directory, run the printed `sudo` commands yourself, then
   `systemctl status mfg-lane.service`. From then on use `systemctl start/stop` -
   do not run `lane_up.sh`/`lane_down.sh` for the same services.

## What the box must have

- **node >= 22.5** - `tmp/RyuNET-core/package.json` `"engines": { "node": ">=22.5" }`.
- **.NET 8 runtime** (`dotnet` on PATH) - `webfront/webfront.csproj` targets `net8.0`;
  the gate runs as `dotnet WebFront.dll` (the `WebFront.exe` apphost from
  `hosting_up.ps1:77` is Windows-only).
- **cloudflared** with the named tunnel already installed in the account
  (`cloudflared tunnel run <name>` reads `~/.cloudflared/config.yml`). Skip via
  `CLOUDFLARED=0`.
- GNU-ish coreutils for the transfer scripts: `tar`, GNU `find` (`-printf`,
  `-print0`), `xargs -0`, `sha256sum`, `awk`. `zstd` only if `COMPRESS=zstd`.
- Secrets under `~/.mfg-web` at mode **600**: `presenter.secret` (required;
  `hosting_up.ps1:19-21`), `access.code` (optional, access-code lobby,
  `hosting_up.ps1:73-74`). For the systemd path also `~/.mfg-web/core.env`
  (mode 600, carries `VFG_PRESENTER_SECRET=` for the core unit).

## Environment the two programs actually read

Core (`node AsphyxiaCore.js` + the `mfg@asphyxia` plugin):

- `VFG_PRESENTER_SECRET` - plugin `handlers/utils.ts:1434-1435` (config first, env
  fallback `utils.ts:1338`); `config.ini` `[mfg@asphyxia]` key of the same name is
  empty in the current lane, so the exported env var is what arms the presenter.
- `mfg_http_bind` / `mfg_http_port` - env beats `config.ini`
  (`index.ts:296-304`: env, then `U.GetConfig`, then directKnob).
- `WEB_SECURE_COOKIE` - `src/webui/index.ts:65`, accepts `1` or `true`
  (case-insensitive); only meaningful when the lane really terminates https.
- `hosting_up.ps1:12-15` clears 16 inherited names before starting (the kit mirrors
  that, and keeps `WEBFRONT_PUBLIC_URL` like `:11` does). Note the plugin also reads
  other `VFG_*` names from env (`VFG_GACHA_ALL`, `VFG_GACHA_SAFE`,
  `VFG_GACHA_CHARAS`, `VFG_GACHA_EXCLUDE`, `VFG_HIDE_GACHA`, `VFG_EVENT_TAKU`,
  `VFG_CARDMNG_MODE`, `VFG_CARDMNG_INQUIRE_MODE`) that `hosting_up.ps1` does **not**
  clear - on a shared box, make sure they are unset or intentional.

Gate (`webfront/Program.cs`):

- `WEBFRONT_UPSTREAM` (:25, default `http://127.0.0.1:8083`), `WEBFRONT_ROOT` (:26,
  default `<app dir>/webroot`), `WEBFRONT_PORT` (:27, default 8090),
  `WEBFRONT_GATE` (:40, gate only when exactly `1`), `WEBFRONT_AUTH` (:41-42),
  `WEBFRONT_TRUST_PROXY` (:47, only value `cloudflare`),
  `WEBFRONT_PAGE_FRONTURL` (:50, default `/aog`), `WEBFRONT_PRESENTER_SECRET` or
  `WEBFRONT_PRESENTER_SECRET_FILE` (:52-59), `WEBFRONT_ACCESS_CODE` or
  `WEBFRONT_ACCESS_CODE_FILE` (:65-74), `WEBFRONT_BIND` (:343-344, `any` serves all
  interfaces, anything else is loopback).
- Hard refusals at startup: `WEBFRONT_GATE=1` without `WEBFRONT_AUTH` (:345-352);
  `WEBFRONT_GATE=1` without `WEBFRONT_UPSTREAM` (:353-360); gate-off relay with an
  armed presenter secret and `WEBFRONT_BIND=any` (:361-368). `lane_up.sh` always
  sets AUTH + UPSTREAM (gate mode is the hosted shape).
- **Leave `WEBFRONT_PAGE_FRONTURL` at its default `/aog` on a hosted lane.** The client
  appends its endpoint paths straight onto the `-FRONTURL` base, so an absolute base
  produces requests like `https://host/appli_info` with no `/aog` (404 at the gate) and a
  doubled host on `cardmng/inquire`: auto card entry never completes, the client sits on
  "Checking e-amusement pass", and the 60 s login timer ends the run at `Login ->
  ScopeClose`. `lane_up.sh` only exports it when the caller sets `WEBFRONT_PUBLIC_URL`, so
  leave that unset unless you know exactly why you need it. Measured on the VPS lane
  2026-11-05: with `WEBFRONT_PUBLIC_URL=http://127.0.0.1:18090/aog` the play page ran
  `Title -> ScopeClose`; with it unset the splice serves `-FRONTURL "/aog"` and the chain
  proceeds.

Core `dist/config.ini` keys the lane cares about (current values):
`port=8083`, `bind=127.0.0.1`, `matching_port=5700`, `allow_register=true`,
`maintenance_mode=false`, `require_pcbid_auth=false`, `server_name=MFG Local`,
`server_tag=LOCAL`, `ea_service_url=http://ea.ryu7w7.xyz/service/init`,
discord keys **all empty** (`discord_bot_token=`, `discord_client_id=`,
`discord_client_secret=`, `VFG_DISCORD_WEBHOOK=`), tachi keys empty, and under
`[mfg@asphyxia]`: `mfg_service_url=http://127.0.0.1:8083/aog`,
`mfg_http_port=22421`, `mfg_http_bind=127.0.0.1`, `VFG_ACCOUNT_AUTH=card`,
`VFG_CARD_AUTH_WINDOW=900`, `VFG_PRESENTER_SECRET=` (empty - the env export is what
sets it), plus the `VFG_*` toggles left blank/`false`.

## The four things that break if done wrong

1. **`dist` is not self-contained.** `dist/plugins/mfg@asphyxia` holds `.ts` files
   loaded through ts-node (`dist/eamuse/ExternalPluginLoader.js:59-74`), and its
   AOG handler fallback does `require('../../src/aog/AogRegistry')` /
   `require('../../../src/aog/AogRegistry')` (`index.ts:369-370`). The `src` tree
   must travel with `dist` and stay next to it, or AOG fallback routes die.
2. **Working-directory-relative paths.** `EXEC_PATH = resolve(process.cwd())`
   drives `PLUGIN_PATH` and `SAVE_PATH` (`EamuseIO.js:96-99`), `uploads` is
   `cwd/uploads` (`AsphyxiaCore.js:74`), `log.txt` lands in cwd
   (`utils/Logger.js:37`), and `ASSETS_PATH` is the cwd-relative
   `../build-env/assets` (`EamuseIO.js:98`) that serves `views` and `/static`
   (`AsphyxiaCore.js:67,73`). The core must start with cwd at `<core>/dist` -
   `lane_up.sh` does it with `cd`, the systemd unit with `WorkingDirectory`.
3. **Plugin attribution is a literal prefix test.** Stack-frame filenames are
   compared with `startsWith(PLUGIN_PATH)` (`ExternalPluginLoader.js:36`) where
   `PLUGIN_PATH` is cwd-derived (`EamuseIO.js:97`). A symlink or bind mount spelled
   differently from the cwd form resolves to another path in the stack frames, the
   prefix test fails, and every plugin-attributed call errors out
   (`WrapCall`, `utils.ts:1338`-style config lookups included). Keep the paths
   literal - no symlinking the plugins dir or bind-mounting it under a different
   spelling.
4. **Case-sensitive lookups.** On a case-sensitive filesystem the gate's static
   handler opens the exact name it assembled (`Program.cs:1043-1060`) and the core's
   `require`/asset paths are exact too. `AsphyxiaCore.js`, `WebFront.dll`,
   `build_stamp.txt`, the `aa/WebGL/<hash>.bundle` names - a case-mangled copy of
   the webroot or of `dist` 404s or fails to load. (The gate's own
   `StartsWith`/`Equals` routing comparisons are case-insensitive on purpose;
   the file opens are not.)

## Process identity: the cwd rule matters

`AsphyxiaCore` rewrites the node process title at startup, so a live core's
`/proc/<pid>/cmdline` reads `MFG Local v1.70b | P` and `comm` reads `MFG Local v1.70`.
A command-line rule that looks for `node` plus `AsphyxiaCore.js` therefore never matches a
running core: on the VPS lane on 2026-11-05 `lane_down.sh` refused to stop its own core and
deleted the pid file as stale, leaving an unmanaged server. Both scripts now accept a second
identity rule, the working directory `lane_up.sh` started them in: `<CORE_DIR>/dist` for the
core, `<WEBFRONT_DIR>/bin/Release/net8.0` for the gate, each side resolved with `readlink -f`
so the comparison only ever matches the directory itself. Name matching, `pgrep`, `pkill` and
port ownership are still never used.

## Verified: first on Windows scratch, then on the real Ubuntu box

Verified on the Windows dev box with Git bash (`C:\Program Files\Git\bin\bash.exe`)
against scratch trees under `C:\mfgtmp\lane_kit_test` (scratch stub core listens on
18083/18421; scratch gate is a self-built net8.0 stub dll on 18090 - the live lane
ports 8083/8090/8091/8095 were never targeted):

- `bash -n` on every shell file; zero CR bytes in every kit file.
- `lane_up.sh --dry-run`: full command + environment plan printed, secret contents
  never printed; variants `CLOUDFLARED=0` and `WEBFRONT_PUBLIC_URL` passthrough.
- `lane_up.sh` real run against the scratch stubs: port probes pass, `config.ini`
  timestamped copy kept the old `mfg_http_bind=0.0.0.0` while the live file shows
  `127.0.0.1`, pid files written, both children up and port-verified. Guard refusals
  proven: live pid in `core.pid` and an occupied core port both exit 1 with reasons.
- `lane_down.sh` negatives: empty pid dir (exit 0), malformed pid, pid 1, the parent
  shell's own pid, dead pid (stale file cleaned), a live pid with the wrong command
  line - all refused. Positives: a stub node core stopped, and both children recorded
  by the real `lane_up` run stopped via SIGTERM with pid files cleaned.
- `install_systemd.sh --staging`: three units + `lane.env` staged, every placeholder
  substituted, nothing written outside the staging dir, no sudo executed.
- `lane_smoke.sh`: `--help`; 16 PASS / 0 FAIL / exit 0 against a scratch mimic
  server (authenticated section included); 8 FAIL / 3 PASS / 1 SKIP / exit 1 against
  a bare anti-fixture server.
- `transfer_build.sh`: `--dry-run` plan + manifest; a full offline run via a transport
  shim (no ssh at any point); re-run skipped all four chunks via the remote ledger.
  `verify_transfer.sh`: agreement run exit 0; a deliberately corrupted remote tree
  (one delete, one same-size content change, one added file) reported MISSING + HASH
  + EXTRA with exit 1.

On the real box (Ubuntu 24.04 LTS, the owner's VPS lane, 2026-11-05): the shipped copies
matched the repo by sha256 with zero CR bytes and `bash -n` clean; `lane_up.sh` started core
and gate with port probes, the timestamped `config.ini` copy and both pid files, refusing a
second core while `127.0.0.1:8083` was still answering; `lane_down.sh` stopped both children
with the cwd rule; the batch transfer moved 9,026,296,654 bytes across 58,809 files in 24
batches with 0 missing, 0 extra, 0 size mismatch and 0 hash mismatch against the manifest;
`lane_smoke.sh` ran 15 PASS / 0 FAIL / 1 SKIP against the live gate and core, the single SKIP
being the anonymous play page, which gate mode answers with a 302 to `/login?next=%2F` by
design and which the splice test covers instead.

Still unverified: the systemd units have never been loaded by a real systemd
(`install_systemd.sh` only stages them and prints the commands), the `zstd` compression
branch, `lane_up.sh`'s cloudflared path (`CLOUDFLARED=0` on the VPS lane, no tunnel
configured there), and `assemble_webroot.sh` on Linux - the VPS webroot was shipped already
assembled by the Windows script.

