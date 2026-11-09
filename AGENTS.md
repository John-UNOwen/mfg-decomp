# mfg-decomp

## Temp-file housekeeping (every session)

Scratch for this project lives in `C:\mfgtmp` and it grows fast; check free space on C: at the
start of a session and clean when it drops below ~20 GB.

Delete when superseded:

- `webroot.old-*` directories (8.6 GB each): keep only the newest, and only until the current
  webroot has passed smoke; then delete it too.
- old batch evidence: `wr_archive/` keeps 3 sets by script; older archive dirs and previous
  rounds' frame dumps can go.
- one-shot analysis dumps (`*.npy`, `catdump*.log`, `catalog_manifest*.tsv`, `key2bundle*.json`,
  `tracked_assets.txt`, old `*.log.txt`), test dirs (`assemble_test`, `deploy_<date>`, `parity`,
  `diag`, `checkup_*`, `fresh`, `cscheck`), and browser profiles (`edge_profile_*`, `probe_profile`,
  `probe_login`) — all regenerable.
- per-task scripts no longer referenced (`patch_*.py`, `keep_*.py`, `restart_gate_oldroot.ps1` style).

Keep — these are cited evidence or live kit:

- `C:\mfgtmp\lane_frames`, `vps_lane_frames`, `faceprop` — cited as sources by OKF concepts.
- frame sets named in the newest docs report (e.g. `mpnew_*`, `p4_08_*`, `wr_match_4_*.png`).
- lane kit: `smoke_local.sh`, `wr_four*`, `wr_task.cmd(.tpl)`, `kill_lanes.ps1`, restart scripts,
  `gate_pid.txt` / lane pid records, the log the running core writes (`ryunet.out.txt`), and the
  gate's live stdout/stderr.
- never touch anything a running lane process has open; delete by explicit path, never by wildcard
  sweeps over the whole tree.
