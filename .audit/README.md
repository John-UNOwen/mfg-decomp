# .audit - what these sweeps prove, and what they refuse to claim

Nothing here restarts the core. Check what is actually up first:

```
pwsh C:\mfg\.audit\server_status.ps1
```

It prints the pid that owns each port (8083 integrated AOG, 22421 native, 8090 webfront,
8094 static lane, 8080 mock) or says plainly that no AsphyxiaCore is running. A report may
write "server left running (pid N)" only by quoting that output or a sweep's own
`server ... LISTENING pid=N` line. The earlier reports quoted pids - 496, 33224, 32796,
27208, "job pwsh-1346" - that no check backed: nothing held 8083 when the last review
started, so those live claims were already false. The sweeps now measure the listener at
both ends of a run and exit 2 with `NOT LISTENING` when there is nothing to probe.

## The sweeps

| command | what it covers |
| --- | --- |
| `pwsh C:\mfg\scripts\ryunet_stage.ps1 -Action status` | every copy of the plugin in the tree: canonical / identical / diverged, and whether a diverged copy can still boot a server (exit 1 if one can) |
| `pwsh C:\mfg\scripts\ryunet_stage.ps1 -Action lane [-Lane verify-dist] [-Port 8093]` | wipe and re-derive a verification lane from the canonical tree: current plugin, own empty savedata, its own ports, `LANE_STAMP.json` naming the build it holds |
| `python .audit/smoke_mfg_endpoints.py` | integrated lane, `http://127.0.0.1:8083/aog/` - the canonical tree's own port |
| `python .audit/smoke_mfg_endpoints_p7.py [http://127.0.0.1:8093/aog/] [REF]` | same sweep against a staged lane; it prints the build it measured and exits 2 when the lane's stamp no longer matches the canonical tree |
| `node .audit/probe_p7_verify.js [base] [db]` | P7 package: dojo session handling, friend_game served as saved, odekake borrow (default staged lane 8093) |
| `node .audit/probe_p7_roundtrip.js [base]` | posted player_game against what the read serves back |
| `node .audit/probe_p7.js [db] [space]` / `node .audit/probe_p7_lenders.js [db] [space]` | profile rows / lender candidacy read straight out of a savedata DB |
| `node tmp/p2repro/repro.mjs` (from `C:\mfg`) | session / state persistence integrity, reads the savedata DB itself |
| `python tmp/p8repro/verify_p8.py [REF]` | content sweep: event kinds, mission schedule, notices and acks, name/sticker screening, `chara_enabled`, `competition_entry`, server-owned state kinds |
| `node .audit/p8_content_vocab.js [utils.ts]` | the content vocabulary one build actually ships - run it on a snapshot to see the before |
| `node .audit/p8_content_unit_check.js` / `node .audit/p8_config_unit_check.js` | the shipped content helpers and their config overrides, loaded straight from `dist/.../handlers/utils.ts`: no server, no restart |
| `python .audit/probe_notice_p8.py [REF]` / `python .audit/probe_odekake_p8.py [LENDER] [BORROWER]` | notice authoring on an account that has never read them; the friend row and the borrow it backs |
| `smoke_common.py` | the part both sweeps share: liveness, the reserve/play contract, fixture rotation |

## The reports

- `.audit/parity-report-2026-11-01.md` - the feature-parity matrix: every audited area (sessions,
  routing, matches, gacha, economy, dojo/friends/borrow, content, client lane) with the verdict and
  the sweep that backs it, the deployment knob changes the audit made, and the accepted gaps.
- `.audit/p6-match-lifecycle-report.md` - match addressing, tids, archives, records, stamps.
- `.audit/p8-content-completeness-report.md` - the content package: what was implemented for its
  seven findings, the line each change lives on, and the checks that back it. Its sweep output is
  `p8_content_sweep.out.txt` (54 checks) and the failing re-run it replaced is
  `p8_content_sweep_exhausted.out.txt`.
- repo `docs/29-checkup-2026-11-02-findings.md` - the 2026-11-02 checkup over the shipped hosted lane: open defects with evidence. The live-edge probes it names (lobby suite, play-chain probe, banner probe) run from `C:\mfgtmp` and the gitignored `tmp/checkup-scratch/`, not from here.

A package that leaves code and probes but no report is not delivered: the probes are how a claim
gets re-checked, the report is how anyone finds them.

The shared parts used to be copied into each sweep. The copies drifted and both went wrong
in the same way, so they live in `smoke_common.py` now - do not paste them back into a sweep.

## One canonical copy; lanes are staged, not kept

`tmp/RyuNET-core/dist/plugins/mfg@asphyxia` is the canonical plugin tree: the only copy that is
edited, the only copy the core loads. As the fixes landed it is 16 files with `handlers/aog.ts`
`6dd99d10210d`, `handlers/utils.ts` `5f310fce40fe`, `index.ts` `c195eb805163` - the claim
`pwsh C:\mfg\scripts\ryunet_stage.ps1 -Action status` prints, and the same hashes a sweep prints in
its `build` line (`.audit/smoke_common.py`). Anything else that holds plugin code is derived from
it by `pwsh C:\mfg\scripts\ryunet_stage.ps1` and says so:

- `tmp/RyuNET-core/verify-dist` - the one verification lane (port 8093, native 22431, its own
  empty `savedata`), re-derived from the tree on every use and stamped with `LANE_STAMP.json`.
  A lane's `mfg_service_url` points at the lane, not at 8083: a lane that advertises 8083 sends
  its own clients to the other lane's build. A lane stops being evidence the moment the canonical
  tree moves on under it, and it is still a bootable server: close the round with
  `-Action retire -Lane verify-dist -Note '<which build this evidences>'`, which freezes the build
  it held and frees the name for a fresh `-Action lane`. `-Action status` is red until a superseded
  lane has been re-staged or retired.
- `tmp/asphyxia_plugins/mfg@asphyxia` - staging mirror, refreshed *from* the canonical tree
  (`-Action mirror`), byte-identical. The generators write into the canonical tree now; the
  mirror is not a place to edit.
- `tmp/RyuNET-core/.frozen-builds/<name>` - read-only build snapshots (see
  [FROZEN.md](../tmp/RyuNET-core/.frozen-builds/FROZEN.md)): plugin TypeScript and run outputs
  kept, core runtime, `config.ini` and `savedata` removed, so nothing boots or is probeable.
- a package's own repro directory may keep a before/after copy of the build (for example
  `tmp/p2midfix/pre/pkg`, transpiled and loaded in process by `tmp/p2midfix/verify.cjs`) as long
  as nothing there can start a server. That is the pattern to copy: a repro that loads the code
  it is testing, instead of a lane that answers requests from an old copy of it.

What it replaces: a lane used to be a whole copy of dist (core JS + `config.ini` + plugin) made
by hand per package and never re-staged, so it froze at whatever revision it was copied at and
remained bootable. `verify-dist` (01:06), `check-p7` (01:19) and `verify-p7` (01:54) existed at
once, each with its own port, and the reports named them as if they were live builds - P3 saying
a fix was staged into `verify-dist` while P4 said the same directory was pre-fix, P6 and P7
naming different pids on 8083, `smoke_mfg_endpoints_p7.py` defaulting to 8095, the port of a copy
that had stopped existing. `-Action status` is the check: it lists the copies and exits 1 while a
divergent one can still boot.

## Fixture accounts

Music gacha possession is server-owned and permanent:
`handle_music_gacha_play_reserve` (handlers/aog.ts:2450-2463) refuses as soon as the account
owns every OID in the series pool, and `recordMusicGain` (aog.ts:860-879) never forgets it.
One account reserves each of the 7 advertised series once in the life of a database, so a
single fixed name (the old `GACHAP4SMOKE`) was a probe that could pass exactly once and then
failed deterministically - its play landed on `request_id 0` and the server answered 930,
which is the correct answer, not the regression the sweep reported.

The sweeps log in as `GACHAP4SMOKE`, `GACHAP4SMOKE2..6` (and the `GACHAP7SMOKE` equivalents),
read the server's own `music_gacha` ledger, and keep the first account that still has an
unfinished series. Pin one on purpose with `SMOKE_REF=<account>` (P4) or the second argv (P7).
A sweep that lands on an exhausted account still passes and prints
`unfinished music series=none (all complete)`: that run asserts the completed branch only, and
it says so out loud.

Those names are probe residue in `dist/savedata/mfg@asphyxia.db` - one profile document each,
attributable by name (`GACHAP4SMOKE*`, `GACHAP7SMOKE*`, `P2REPRO*`, `P8VERIFY*`, `P8V*`,
`P8NOTICE*`, `P8LENDER*` / `P8BORROW*`, plus older `P2GHOST*` and `MID*` orphans from before the
session-integrity fix). The `MID*` ones are the three rows `MID00998877` / `MID88000123` /
`MID88000124`, written through the mid fallback by `probe_live.py` and the P2 probes; they are
quarantined now, and `.audit/p9-mid-phantom-report.md` is the record of what that repair did and
did not do.

Notices are the same trap in a different shape: an ack is permanent per profile
(`handle_notice_ack` writes the ids, `noticeGameJson` / `receiveOnlyJson` filter them out), so the
notice assertions in `tmp/p8repro/verify_p8.py` can pass on a given account exactly once. A pinned
account name turns a re-run into five deterministic `FAIL`s that look like a regression - that is
what `p8_content_sweep_exhausted.out.txt` records. The sweep's default account name now rotates;
pass one as `argv[1]` only when you mean to reuse that account.

## What `SMOKE OK` means for the music gacha now

- A play is only posted behind a reserve the server granted (`<is_success>1</is_success>`).
  The serv_st code stays 0 on a refusal, so reading the code was never enough:
  MusicGachaPlayReserve.cs:29-34 reads `is_success` and `request_id` out of the body.
- A refusal has to be the shape the client can recover from (`is_success 0`, `request_id`
  present, no error serv_st - GachaSelectFlow.cs:436-449 re-enables the buttons, an error
  code throws NetworkResponseException and ends in ERROR_STOP), and cashing the refused
  request_id has to be 930 (StatusCode.AlreadyFinishedTransaction, handlers/aog.ts:2497).
- Pulls stop exactly when the series is owned, no OID is granted twice, and every grant shows
  up in the server ledger afterwards.
- Once the ledger is full the authored `music_gacha` state says `IsComplete` true - the client
  branches on it for the banner and the MusicGachaComp missions.

## The P2 probe

Section (e) addresses the match with the tid `entry_game` handed out (EntryTakuInfo.cs:50-52
reads `<tid>`/`<pindex>`, GGet.cs:30 sends them back in the `must` chain). Match tids are
allocated from a DB-persisted high-water mark (utils.ts:585-614), so hardcoding `1001` made a
healthy build look like a `gget code=770` regression. The probe also asserts the other side: a
`gget` addressing a tid that seat does not hold must be refused with 770. Quote its
`EXPECTATIONS: all held` / `PROBLEMS: ...` line, not a remembered `code=` value.
