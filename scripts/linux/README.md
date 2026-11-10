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
| `install_systemd.sh` | Substitutes real paths/user into the unit templates, stages the results plus a non-secret `lane.env` into a staging directory, and prints the exact `sudo` commands a human runs. It never invokes sudo itself. Before staging anything it proves on this box every path those units start from (the `ExecStart` binaries, `<core-dir>/dist/AsphyxiaCore.js`, `<webfront-dir>/bin/Release/net8.0/WebFront.dll`, `<webroot>/index.html`, `<sec-dir>/presenter.secret`) and refuses with every missing one named - the printed list ends in `systemctl enable --now`, and a half-populated host cannot start the units it would enable. |
| `lane_smoke.sh` | Read-only curl acceptance suite against a base URL: COOP/COEP headers, brotli sidecar rule, immutable-vs-TTL caching, `index.html` no-store, FoxNET login page, `/static` assets, anonymous session rejection, `/aog` reaching the core handler, play page `-AUTOSERVE`, and an optional login/logout section (`SMOKE_USER`/`SMOKE_PASS`). Prints PASS/FAIL/SKIP with evidence and a summary count; non-zero exit on any FAIL. |
| `transfer_build.sh` | Ships the WebGL build over ssh in chunked tars (no rsync), one chunk per top-level directory, resumable only in the "skip complete chunks by sha256" sense, with a manifest and a hard refusal to delete anything remote. Every remote observation runs under `bash -c` with `set -o pipefail` and must close with a per-run observation-proof trailer line: a far-side `find`/`sha256sum` failure, or a missing/misplaced/duplicated trailer, refuses the chunk - it is no longer hidden behind a successful `sort`/`sed` and never answers as a skip or send. The done line states its denominator - `… = sent + skipped = N of total chunk(s) enumerated` - and a run that looked at fewer chunks than the enumerated list is a nonzero exit with no done line and no manifest upload (a `1 skipped` truncated run can no longer be read as a converged rerun, the 2026-11-09 promotion-log defect); the done output also always states the surplus - remote files under the observed chunk paths that the manifest does not list, which never block a skip but are `verify_transfer.sh`'s `EXTRA`, so a converged rerun over a tree still holding a dropped name says so. `--dry-run` shows the plan + manifest, including the enumerated chunk total and the exact wrapped far-side command; that enumeration is complete - `MANIFEST_LIMIT` (`--help` documents it as a report cap for dry-run listings, default 20, 0 = show all) is declared by the script and read by nothing, so no dry-run listing is capped by it. Chunk tars in `STAGE_DIR` are deleted as soon as their send is confirmed, and a run that completes sweeps the `chunk__*.tar*` leftovers earlier runs left behind (a chunk that now skips, or a top-level name the build renamed or dropped) — a promotion no longer leaves a second full copy of the tree in `STAGE_DIR`, the peak is one chunk. A mid-transfer failure is not a no-op: extraction replaces each file it reaches as the stream arrives, so a run that dies mid-chunk leaves `REMOTE_DIR` a **mixed tree** (reached files hold this build's bytes — the previous bytes are destroyed, no delete needed; the member being written is truncated; everything not yet reached is still the old tree), still served if the path is live — the failure line says so and names the only two recoveries: complete a full transfer into the same `REMOTE_DIR`, or restore the operator's aside of the previous tree. The failed chunk's tar is kept for the documented rerun; `--keep-tars` / `KEEP_TARS=1` opts out of all deleting for forensics. `manifest.tsv` is never swept, and it is what `verify_transfer.sh` and `swap_webroot.sh` read — and the readers after the run (the KB delta procedure included) can never see a half-written one: the upload lands in `.lane_transfer/manifest.tsv.tmp`, is measured on the target (`wc -c` + `sha256sum` under `bash -c` with `set -o pipefail`), and is renamed onto `manifest.tsv` only when bytes and sha256 equal the local file — the manifest path is never truncated or streamed into, so a cut upload (transport exit 0 included) refuses with the previous manifest still published. `chunk_hashes.tsv` is never swept either but is read by nothing — it is the what-was-sent history, one line per confirmed send as `<run-tag>` TAB `<tar sha256>` TAB `<tar name>`: the line is appended only after the chunk's tar finished streaming **and** the remote ledger append of that same line succeeded, so a chunk whose stream died mid-tar is never recorded as sent, and the file is never truncated — runs stack up, each line tagged with its UTC run tag, so a rerun adds to the record instead of replacing it. A real run first passes a **capacity gate**: it reads the free space of the local filesystem holding `STAGE_DIR` and of the remote filesystem holding `REMOTE_DIR` (or, on a first transfer with no `REMOTE_DIR` yet, that path's nearest existing ancestor — same filesystem) via `df -B1 --output=avail`, and refuses **before the first byte moves**, both numbers in the line, when either filesystem holds less than the manifest total (tree bytes — an upper bound: skipped chunks and gzip make what actually streams smaller). The failure mode the gate exists to prevent is a tar write that fills the filesystem mid-chunk and leaves the destination a half-new, half-old mix (2026-11-10: 1.2 GB free hit mid-promotion of a 9 GB build on the filesystem the gate serves); an unreadable or not-a-number answer is a refusal for the same reason — guessing at capacity is what filled the disk. `--skip-space-check` / `SKIP_SPACE_CHECK=1` opts out for the operator who measured the fit themselves (e.g. a converged rerun that sends 0 bytes over a nearly-full target), and `--dry-run` prints the exact probe a real run will issue. Every read-only transport call — the `.lane_transfer` mkdir, the observations, the free-space and manifest-tmp probes, the manifest rename — runs with its stdin held shut, and only the three calls handed a payload keep it (the tar send, the sent-ledger and manifest pipes). That is what lets a `while read …; done < build-list` promotion wrapper run every line: a transport that behaves like ssh reads the stdin it inherits, so one read-only call left holding it ate the rest of the list under a clean exit 0 (2026-11-09 promotion, `9761a3b`). The destination also has to be the tree the operator meant: before its first write a real run asks the target whether `REMOTE_DIR` is a directory, and an absent one — or no answer at all from the transport — is a refusal. The bookkeeping `mkdir` is `mkdir -p`, so before that gate a plausible typo was silently *created* by the run that meant to fill the real tree, shipped into, and `verify_transfer.sh` agreed about the seconds-old result, with no line in the log saying the destination had been made. `--create` / `CREATE_REMOTE_DIR=1` opts in to creating a first-transfer destination, printing a loud `CREATION:` line that names the path before the `mkdir`; a rerun once the tree exists needs neither (and since `BOOKKEEPING_DIR` moved the bookkeeping out of the tree, the `mkdir` it runs for the bookkeeping can no longer create `REMOTE_DIR` at all - the destination gets its own `mkdir`, behind that `CREATION:` line). The manifest publish and the remote sent-ledger go to `BOOKKEEPING_DIR`, which defaults outside `REMOTE_DIR` as its leading-dot sibling and is refused whenever it normalises to the served tree or a path inside it (see the section above). |
| `verify_transfer.sh` | Compares the manifest against the remote tree: missing / extra / size-or-hash mismatched files plus file and byte totals, so a 5.47 GB transfer is confirmed, not assumed. Both remote listings use the same `bash -c` + `set -o pipefail` + trailer wrapping as `transfer_build.sh`: a failed or unproven observation is a refusal, never a `MISSING`/`HASH` verdict computed from a half-read tree. `REMOTE_DIR` is required, absolute, and guarded with the identical block `transfer_build.sh` applies: normalised lexically first (`//` collapsed, `./`/`../` resolved, trailing `/` removed — both values printed when they differ) and then refused before any listing runs when it normalises to `/`, to a system directory, to or inside one, or holds a single quote. Both scripts accept exactly the same `REMOTE_DIR` values, so a typo can never turn the verification into a `/proc` walk, a `cd` into the invisible ssh login cwd, or an `agree` about a tree `transfer_build.sh` would never have written. Both listings also run with their stdin held shut: the script reads nothing from stdin, and a transport that behaves like ssh forwards whatever it inherits — left open, the first listing ate the work list of a `while read` wrapper around this script and the wrapper ran one tree of three, under exit 0. Exit 0 is the only `agree` a caller may act on: a refused comparison and a compared-but-not-equal tree both exit 1 and both mean *do not serve this tree*, and the `MISSING`/`SIZE`/`HASH`/`EXTRA` verdict names print capped at `REPORT_MAX` (default 40 per class, `0` prints every name). `BOOKKEEPING_DIR` is taken with the same default (`REMOTE_DIR`'s leading-dot sibling) and the same refusals as `transfer_build.sh` - a value that lands inside the served tree is refused before any listing runs - and the listings still exclude an in-tree `.lane_transfer/` leftover so a pre-move tree does not read as `EXTRA`. |
| `swap_webroot.sh` | Promotes a staged webroot over the live one with two renames, but only after the whole staged tree has been counted against the manifest of the transfer that shipped it (file count + per-file size: `MISSING`, size-mismatch, `EXTRA`, both totals printed). `chown` runs while the gate is still up; `SERVICES` (default `mfg-gate`) are stopped only for the renames; the new root is then proven by polling `/build_stamp.txt` for the expected stamp, and any failure after the first rename — a signal included — puts the renames back and restarts the services. The previous root is kept as the undo handle and never deleted. `--dry-run` runs the verification and prints the plan. |

## Run order for a parallel install (Windows lane keeps serving)

Do all of this **on the Linux box**; the Windows lane is not involved at any step.

1. `transfer_build.sh --dry-run` -> review chunks/manifest; then real run (disk accounting: it refuses
   up front unless both its `STAGE_DIR` filesystem and `REMOTE_DIR`'s filesystem have the manifest
   total free — roughly the tree size, so a full-size promotion wants that much free beside the
   staged webroot, `--skip-space-check` when you have measured the fit; and a `REMOTE_DIR` that does
   not exist on the target is refused before anything is created — a first transfer to a fresh path
   takes `--create` / `CREATE_REMOTE_DIR=1`, which prints a loud `CREATION:` line naming the path
   it is about to make); then `verify_transfer.sh` until it reports agreement, and step 2 does not
   start before it has: agreement is the proof that closes a transfer, never an optional extra look -
   only exit 0 says it (zero missing, zero `EXTRA`, zero size/hash mismatches; verdict names print
   capped at `REPORT_MAX`, default 40 per class, `0` prints every name). `transfer_build.sh`'s done
   line states which chunks this run looked at, not that the tree is whole - nothing assembles
   (step 2), brings up (steps 3-6) or promotes (step 7) on a done line alone.
2. `assemble_webroot.sh` on the Linux box (assembles the lane webroot from the
   transferred build; needs the brotli sidecar tool + a .NET 8 runtime).
3. `lane_up.sh --dry-run` -> sanity-check paths/env/ports; then `lane_up.sh`.
4. `lane_smoke.sh` -> the acceptance suite. With credentials:
   `SMOKE_USER=... SMOKE_PASS=... lane_smoke.sh`.
5. `lane_down.sh` whenever the lane has to move.
6. Optional permanent form: `install_systemd.sh /path/to/repo --user <svcuser>`,
   review the staging directory, run the printed `sudo` commands yourself, then
   `systemctl status mfg-lane.service`. From then on use `systemctl start/stop` -
   do not run `lane_up.sh`/`lane_down.sh` for the same services. Run it **after** steps 1-2
   and after the secrets exist: before staging a single file the installer proves on this box
   each path the units start from - the `ExecStart` binaries, `<core-dir>/dist/AsphyxiaCore.js`,
   `<webfront-dir>/bin/Release/net8.0/WebFront.dll`, `<webroot>/index.html` and
   `<sec-dir>/presenter.secret` - and a host missing any of them is refused with every missing
   path named and nothing staged, because the list it prints ends in `systemctl enable --now`.
   It also reads `<core-dir>/dist/config.ini` before staging: a non-empty `[mfg@asphyxia]`
   `VFG_PRESENTER_SECRET` value beats the `core.env` the run stages (`handlers/utils.ts:1516-1522`),
   so a value proven different from `<sec-dir>/presenter.secret` is refused, both files named.
   A green run says which paths it proved, so the webroot in that line is the one to compare
   with what step 2 actually assembled. The units run as
   `<svcuser>`, and every staged path (webroot, `--sec-dir` and its `core.env`, the tunnel
   unit's `ReadWritePaths`) resolves under that user's home: with no `--home` the installer
   reads the home from `getent passwd <svcuser>`, and a user this box cannot resolve is
   refused with both candidate homes printed. Its printed `core.env` recipe runs
   `sudo -u <svcuser>` - the service must own what it reads at mode 600. Re-running into the
   same staging directory is the normal dev loop: every unit is rewritten from its template
   with that run's paths and user, and the run says so; a unit that run does not stage - an
   earlier `--tunnel` run's `mfg-tunnel.service` after a plain run - is removed from the
   staging directory rather than left there installable and unnamed.
7. Updating the webroot a live lane serves (a refresh, not a fresh install): stage the incoming
   tree beside the live one, ship the delta into it with `transfer_build.sh`
   (`REMOTE_DIR=<staged>`), `verify_transfer.sh` it to agreement (exit 0; `swap_webroot.sh` re-runs
   this count against the staged tree itself and refuses the swap on any `MISSING`, size mismatch or
   `EXTRA`), then `WEBROOT=... STAMP=<stamp> swap_webroot.sh` - see the next section. The live root is
   swapped through that script, never through hand-run renames.

## Bookkeeping of a transfer (`BOOKKEEPING_DIR`)

Every `transfer_build.sh` run publishes two files on the target: `manifest.tsv` (the whole asset
inventory the KB-delta procedure diffs against) and `chunk_hashes`, the append-only remote
sent-ledger. Their location is `BOOKKEEPING_DIR` - and it must not be inside `REMOTE_DIR`, because
`REMOTE_DIR` is the document root the front serves. The old fixed location was exactly that
(`$REMOTE_DIR/.lane_transfer/`): a GET of `/.lane_transfer/manifest.tsv` on the live lane answered
`200` with 13 MB - the full 58,809-path production asset inventory, hashes included - until the
directory was moved off the live tree by hand. `BOOKKEEPING_DIR` now defaults **outside** the
served tree, to `REMOTE_DIR`'s leading-dot sibling (`/srv/mfg/webroot` ->
`/srv/mfg/.webroot.lane_transfer`), and any value that normalises to `REMOTE_DIR` itself or to a
path inside it - the old location included - is refused before anything is staged or sent. The
dot-name hides the dir from plain listings; it is **not** the protection (`verify_transfer.sh`
takes the same name, the same default and the same refusals, and keeps excluding the in-tree
`.lane_transfer/` leftovers of trees shipped before the move from its listings, so a legacy dir
never reads as `EXTRA`). The dot-sibling also keeps each destination tree's bookkeeping apart:
staging a delta into `webroot.new-<stamp>` writes that tree's manifest beside *it*, not the live
tree's - the two never overwrite each other.

> **Follow-up for owner sign-off (not implemented in the kit):** webfront's `ServeStatic`
> (`webfront/Program.cs:1041`) confines every open to the root (`file.StartsWith(Root + separator)`
> or 403) but never rejects a request whose path segments start with `.`, so anything that does
> land inside a served root under a dot name is served like any other file - which is exactly how
> `/.lane_transfer/manifest.tsv` was reachable. A one-line gate rejecting dot-prefixed path
> segments would close that class server-side; it is a webfront change, not a kit change, and it
> waits on the owner's sign-off.

Note the same name means one thing more than once, by design: in `swap_webroot.sh`, `BOOKKEEPING_DIR`
is the **local** directory holding the manifest the staged tree is counted against (default:
`$HOME/mfgtmp/transfer_stage`, `transfer_build.sh`'s `STAGE_DIR` - outside every webroot on purpose,
so the first rename cannot carry the manifest away). On the box where both scripts run, the two
readings coincide: the transfer publishes its manifest at `$BOOKKEEPING_DIR/manifest.tsv` and the
swap reads its manifest from `$BOOKKEEPING_DIR/manifest.tsv`.

## Promoting a staged webroot (`swap_webroot.sh`)

The production webroot swap, productized out of the one-off script the 2026-11-10 promotion ran:
two renames, with the checks that script did not have. That one-off (`C:\mfgtmp\promote_swap.sh`) is
kept as a historical artifact of a swap that could print a matching stamp over a truncated play page
and exit 0 - it is not a recipe; live-root promotions run this script.

```
WEBROOT=/srv/mfg/webroot STAMP=D759A911DF0C scripts/linux/swap_webroot.sh [--dry-run]
```

Order, and why it is the order: verify the staged tree against the manifest -> `chown` the
incoming tree -> stop `SERVICES` -> rename the live root aside -> rename the staged tree in ->
start `SERVICES` -> ask the front for `/build_stamp.txt` until it answers `STAMP`. Everything
left of the first rename is reversible by construction; right of it an exit trap puts the renames
back and restarts what it stopped, and every line it prints says which path holds what.

**An interrupt is a failed swap.** Ctrl-C, `kill`, or a dropped ssh session (`SIGHUP`) inside the
down window goes through the same rollback: the renames go back, the units come up, and the output
names the signal that arrived (exit 130 / 143 / 129). Bash does not spring an `EXIT` trap for an
untrapped `SIGTERM` or `SIGHUP`, so the script traps them into the rollback instead of letting the
shell die mid-window - interrupted that way before the traps were wired, a swap left the unproven
tree in the live path, the previous root parked at an aside path no printed line named, and only
exit 143 to show for it. `SIGKILL` - an OOM kill - is still uncatchable: the last
`rename A -> B` line names the aside, and `systemctl start <unit>` says what to bring up.

| Name | Default | What it is |
|---|---|---|
| `WEBROOT` | required | the live root the front serves. Absolute; refused when it normalises to `/` or to/inside a system directory, because the first rename moves whatever stands there |
| `STAMP` | required | the stamp the staged tree must carry, and the answer the front must give afterwards |
| `STAGED` | `<WEBROOT>.new-<STAMP>` | the tree to promote; refused when it is or nests inside `WEBROOT` |
| `SERVICES` | `mfg-gate` | space-separated units to stop before the renames and start after them (`'mfg-gate mfg-lane'` when the core is a unit too). Empty = the front is stopped some other way, and the HTTP proof is then the only net |
| `CHOWN_TO` | `mfg:mfg` | `owner:group` the incoming tree is `chown -R`'d to **before the stop** - an `-R` over 58,809 files is work the gate does not need to be down for. Empty = skip |
| `BOOKKEEPING_DIR` | `$HOME/mfgtmp/transfer_stage` | where the bookkeeping of the transfer run that shipped this tree lives **on this box**: `transfer_build.sh`'s `STAGE_DIR`, holding the local `manifest.tsv`. Same name as the transfer scripts' remote bookkeeping dir (see the note above); this one is local and sits outside every webroot on purpose |
| `MANIFEST` | `$BOOKKEEPING_DIR/manifest.tsv` | the manifest the staged tree is counted against |
| `GATE_PORT` / `PROOF_HOST` | `8090` / `127.0.0.1` | where the proof asks `/build_stamp.txt` |
| `PROOF_TRIES` / `PROOF_SLEEP` | `60` / `0.5` | how long the proof polls; raise them when `SERVICES` names `mfg-lane` (the core needs ~20 s to bind) |
| `REPORT_MAX` | `20` | how many `MISSING` / size-mismatch / `EXTRA` names to print per class (`0` = all) |
| `STAGED_VERIFY_CMD` | empty | extra check, run as `bash -c "$cmd" swap_webroot <STAGED>` before the down window; a non-zero exit refuses the swap |

**The pre-check is the whole tree, not the stamp.** Every file the manifest lists must be in the
staged tree at the size it lists, and no file the manifest does not list may be there. Both
totals print - they are the denominator, and "the stamp matches" says nothing about 58,809 files.
`.lane_transfer/` is excluded, as in `verify_transfer.sh`: the only tree that can still hold one is
a tree shipped before `BOOKKEEPING_DIR` moved the bookkeeping out (see the note above).
A size mismatch is the half-shipped tree: the top-level-files chunk
is a path-ordered tar, so `build_stamp.txt` is unpacked **before** `index.html`, and a transport
that died inside it leaves a matching stamp over a truncated play page - the state the one-off
promotion accepted, served, and exited 0 on. `EXTRA` is refused too: the delta flow says to
delete the remote-only names from the staged tree before promoting, and a swap that promoted them
instead serves files no catalog points at, wearing this build's stamp. Size rather than content
because hashing 8.6 GB on the box is `verify_transfer.sh`'s job, and a file that changed at the
same size is its report, not this one's - `STAGED_VERIFY_CMD` is where that pass plugs in.

**`BOOKKEEPING_DIR` sits outside the webroot on purpose.** The manifest is what the swap verifies
against, and a bookkeeping dir inside the live root is carried off by the first rename: the path
a refusal prints would no longer hold the file that refusal was based on (and in a tree shipped
before the transfer scripts' `BOOKKEEPING_DIR` move, its `.lane_transfer/` - manifest +
`chunk_hashes` - travels with the old root too). A `MANIFEST` that
resolves inside `WEBROOT` is refused for that reason. `transfer_build.sh` tolerates leftover names
in the remote tree because it issues no deletions - it states the surplus count it observed in its
own done output (`surplus: N remote file(s) …`) and the names themselves go to
`verify_transfer.sh` as `EXTRA`; the swap refuses them in the tree it is about to promote.

**Proof, not a curl that prints nothing.** The answer is compared with `STAMP` over a bounded
poll, the same raw-socket probe `assemble_webroot.sh` uses (`build_stamp.txt` has no trailing
newline, so a line-oriented read never sees it). The one-off promotion printed
`stamp now: $(curl -sf ...)` under `set -e`, and a curl that could not connect still left that
line printed with an empty value - nothing in its output could tell "the new root answers" from
"the gate is down and nothing answered".

**Disk accounting.** The swap costs no space: two renames inside one directory on one filesystem,
which is why it renames instead of extracting - a 30 GB box with ~9 GB free cannot hold a second
untarred webroot, and `tar -xf` would re-write every member and destroy the `cp -al` hardlinks a
staged tree relies on. While the aside stands, though, the tree holds two roots. This script
never deletes it - it is the undo handle until the new root has passed smoke - and the success
line prints its path with the `rm -rf` to run once it has.

## What the box must have

- **node >= 22.5** - `tmp/RyuNET-core/package.json` `"engines": { "node": ">=22.5" }`.
  Under systemd the unit execs an absolute path (default `/usr/bin/node`, `--node-bin` for
  nvm/tarball installs), and `install_systemd.sh` refuses to stage if that path is not an
  executable file: Ubuntu 24.04's apt node is 18.x, so the node a lane box actually runs is
  usually not `/usr/bin/node`.
- **.NET 8 runtime** (`dotnet` on PATH) - `webfront/webfront.csproj` targets `net8.0`;
  the gate runs as `dotnet WebFront.dll` (the `WebFront.exe` apphost from
  `hosting_up.ps1:77` is Windows-only). The unit execs an absolute path too (default
  `/usr/bin/dotnet`, `--dotnet-bin`).
- **cloudflared** with the named tunnel already installed in the account
  (`cloudflared tunnel run <name>` reads `~/.cloudflared/config.yml`). Skip via
  `CLOUDFLARED=0`. Under systemd the unit execs `/usr/bin/cloudflared` unless `--cloudflared-bin`
  says otherwise, and that path is only checked on a run that stages the tunnel unit (`--tunnel`).
- GNU-ish coreutils for the transfer scripts: `tar`, GNU `find` (`-printf`,
  `-print0`), `xargs -0`, `sha256sum`, `awk`, `df` (`-B1 --output=avail`, read on both ends
  by `transfer_build.sh`'s capacity gate - the far side needs it too). `zstd` only if `COMPRESS=zstd`.
- Secrets under `~/.mfg-web` at mode **600**: `presenter.secret` (required;
  `hosting_up.ps1:19-21` - both `lane_up.sh:322-324` and `install_systemd.sh` refuse to start or
  stage without it, and the gate would otherwise read an absent `WEBFRONT_PRESENTER_SECRET_FILE`
  as no secret at all, `Program.cs:57`), `access.code` (optional, access-code lobby,
  `hosting_up.ps1:73-74`; the systemd installer notes it and carries on). For the systemd path also `~/.mfg-web/core.env`
  (mode 600, carries `VFG_PRESENTER_SECRET=` for the core unit) - under the
  *service user's* home and owned by them: the units `EnvironmentFile=` it as that user,
  and a caller-owned 600 file is skipped with no error. `core.env` only arms the presenter while
  `config.ini`'s `[mfg@asphyxia]` section carries no `VFG_PRESENTER_SECRET` value: a non-empty ini key
  beats the environment (`handlers/utils.ts:1516-1522`), so `install_systemd.sh` proves the key empty,
  absent, or equal to `presenter.secret` before it stages - a different value there is refused.

## Environment the two programs actually read

Core (`node AsphyxiaCore.js` + the `mfg@asphyxia` plugin):

- `VFG_PRESENTER_SECRET` - plugin `handlers/utils.ts:1623-1624` (`presenterSecret` -> `configString`,
  `utils.ts:1516-1522`): a **non-empty `config.ini` `[mfg@asphyxia]` key of the same name wins and the
  environment is only read when that key is absent or empty** (`U.GetConfig` serves the parsed ini,
  `ExternalPluginLoader.ts:170-176`). The key is empty in the current lane, so there the exported env
  var is what arms the presenter; on a host whose key was filled in once (the WebUI config page
  persists it via `SaveConfig`, `ArgConfig.ts:334-340`) the env export is silently ignored and the gate
  and core end up on different secrets. `install_systemd.sh` reads `<core-dir>/dist/config.ini` and
  refuses to stage a `core.env` that would be silently ignored by a different ini value.
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
  shim (no ssh at any point); re-run skipped all four chunks with 0 bytes sent - each skip an
  observation of the remote tree (the chunk's payload digest folded from a live remote listing of
  the files its manifest lists), never a reading of the `.lane_transfer` sent-ledger, which records
  what was sent and decides nothing (the skip read that ledger when this bullet was first written;
  the 2026-11-08 remote-observation rewrite retired it - the 2026-11-10 fixture bullets below are
  what the skip rests on today).
  `verify_transfer.sh`: agreement run exit 0; a deliberately corrupted remote tree
  (one delete, one same-size content change, one added file) reported MISSING + HASH
  + EXTRA with exit 1.
- Observation integrity (2026-11-10, fixture cases C54-C59 in `tmp/linux_fix/fixture/run_all.sh`,
  transport shims only): a far-side `find` error or a killed `sha256sum` inside a listing, a listing
  whose observation-proof trailer was stripped mid-stream, and one whose trailer was duplicated -
  each is a refusal with a named reason in both `transfer_build.sh` and `verify_transfer.sh`, no
  `send`/`skip`/`transfer done` line and no verdict is printed for an errored observation, while the
  same trees observed cleanly still skip all chunks and still report agreement.
- Promotion denominator (2026-11-10, fixture cases C60-C62, transport shims only): the done line states
  `sent + skipped = N of total chunk(s) enumerated` on the first run and on the converged rerun, and the
  `--dry-run` plan prints the same enumerated total; a run that passes a chunk through the loop without
  looking at it (a byte-for-byte copy of the script with one chunk bypassed) is refused with both counts
  named, no `transfer done` line and no manifest upload - the truncated-loop `0 sent, 1 skipped` shape can
  no longer be mistaken for a converged rerun; and a remote still holding `./Build/webgl.oldname.wasm`,
  a name the build dropped, still skips every chunk (surplus must not block the skip) while the done
  output states `surplus: 1 remote file(s)` - the same tree reads `EXTRA` + MISMATCH, exit 1, from
  `verify_transfer.sh`.
- Sent-ledger honesty (2026-11-10, fixture cases C75-C77, transport shims only): the local `chunk_hashes.tsv`
  line is written only after the chunk's tar finished streaming and the remote ledger append of that same line
  succeeded, so a transport that dies mid-tar leaves no local line for the dead chunk - the confirmed send has its
  line on both sides, and a remote append that is refused records nothing locally either while still saying the
  bytes landed. Nothing truncates either: an all-skip rerun leaves the file byte-identical, a rebuild appends its
  own line (that tar recorded once per run that shipped it), every line carries its run tag, and both ledgers hold
  the same sends in the same order. Against the pre-fix script 10 of these 15 assertions fail: the interrupted
  chunk was recorded as sent, and the rerun that shipped it wiped the record of the chunks that had.
- `verify_transfer.sh` REMOTE_DIR guard (fixture case C81, echo transport only): the values `transfer_build.sh`
  refused for years - `/` (which became `cd ''`, a bash no-op that lists the ssh login cwd and can print `agree`
  about a tree nobody serves), `//`, `/usr//`, `/bin///` (live system directories, procfs walk, `sha256sum`
  hanging on `/proc/kmsg`), relative paths, and single-quoted values - are now refused before any listing is
  dispatched (the echo transport logs zero commands), while a messy-but-legal value (`/srv/mfg//webroot/../webroot/`)
  is normalised with both values printed, dispatches `cd` with the normalised one, and a real tree verified through
  a trailing-slash `REMOTE_DIR` still reports agreement, exit 0.
- Read-only transport stdin (2026-11-10, fixture cases C82-C84, transport shims only): the transport shim behaves
  like ssh — it reads all of its stdin, because ssh cannot know whether the far side will consume it — and logs the
  class of every call with the number of stdin bytes it drained. Driven through a `while read` wrapper over a
  3-line work list, the kit as it stood before this fix (that one call reopened, every earlier closure kept) runs
  1 iteration of 3 and exits 0 — the drain log shows the `.lane_transfer` mkdir and the first listing eating the
  remaining 14 bytes — while the kit as tracked runs all three: three `transfer done` lines and three agreements,
  each exit 0. The same drain log shows the payload calls still
  handed their stdin — the chunk tars, the ledger lines and the manifest upload all landed, and the verifier agrees
  with the last build byte-for-byte — and every read-only call drained nothing. A census of both scripts counts
  every transport dispatch and holds each one either closed (`</dev/null`) or payload-fed: 9 call sites in
  `transfer_build.sh` (6 closed, 3 payload), 1 in `verify_transfer.sh` (closed), zero left inheriting the caller.
- Destination existence (2026-11-10, fixture cases C85-C86, transport shims only): the far-side form of the
  bookkeeping `mkdir` is `mkdir -p` — proven to create the destination tree itself, exit 0, the mechanism that
  turned a plausible typo into a seconds-old tree the verifier would later agree about. Against the tracked
  script, a run at an absent `REMOTE_DIR` refuses before the first write, names the path and `--create`, creates
  nothing (the typoed path is still absent after the whole run), and prints no send/skip/CREATION/done line;
  `--create` and `CREATE_REMOTE_DIR=1` each ship a first transfer to a fresh (flag) or fresh nested (env) path
  behind a loud `CREATION:` line naming the path and preceding the first send, the tree lands complete with its
  manifest, `verify_transfer.sh` agrees, and the convergence rerun needs no flag and prints no CREATION line;
  an existing tree without the flag still ships untouched, no over-refusal; and a transport that fails only the
  read-only existence test is its own refusal — the target is not guessed absent, no bookkeeping dir appears,
  nothing streams. The `--dry-run` plan prints the existence gate, the exact `if test -d …` probe, and `--create`
  as the opt-in; the `--help` usage line carries `[--create]`.
- Bookkeeping outside the served tree (fixture case C87, transport shims only): with `BOOKKEEPING_DIR`
  unset, a full transfer publishes `manifest.tsv` byte-for-byte (and appends its `chunk_hashes` ledger) into
  `REMOTE_DIR`'s leading-dot sibling, while `REMOTE_DIR` itself ends the run holding zero files named
  `manifest.tsv`/`chunk_hashes` and no `.lane_transfer` dir - the served tree never sees the bookkeeping -
  and `verify_transfer.sh`, deriving the same default from the same `REMOTE_DIR`, agrees (exit 0) through
  that layout while a planted legacy in-tree `.lane_transfer/` leftover still stays excluded. An explicit
  `BOOKKEEPING_DIR=` is honoured by both scripts; a value that equals `REMOTE_DIR` or normalises inside it
  (the old `$REMOTE_DIR/.lane_transfer` location included), a relative one and a single-quoted one are
  refused before a single transport command runs (the echo transport logs zero commands), in both scripts,
  with the default path named in the refusal. `--create` behind the move still ships the whole tree: the
  `CREATION:` line, its own `mkdir` for the destination, bookkeeping in the new tree's own sibling.

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
assembled by the Windows script. And `swap_webroot.sh` on the live box: the swap it replaces was
done by hand on 2026-11-10, and this script has only the fixture runs behind it - the first real
run is a `--dry-run` against the staged tree hours before the window.

