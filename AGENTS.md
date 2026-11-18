# mfg-decomp

## Temp-file housekeeping (every session)

Scratch for this project lives in `C:\mfgtmp` and it grows fast. Check free space on C: at the
start of a session and clean when it drops below ~20 GB.

Delete when superseded:

- `webroot.old-*` directories (8.6 GB each): keep only the newest, and only until the current
  webroot has passed smoke. Then delete it too.
- old batch evidence: `wr_archive/` keeps 3 sets by script. Older archive dirs and previous
  rounds' frame dumps can go.
- one-shot analysis dumps (`*.npy`, `catdump*.log`, `catalog_manifest*.tsv`, `key2bundle*.json`,
  `tracked_assets.txt`, old `*.log.txt`), test dirs (`assemble_test`, `deploy_<date>`, `parity`,
  `diag`, `checkup_*`, `fresh`, `cscheck`), and browser profiles (`edge_profile_*`, `probe_profile`,
  `probe_login`), all regenerable.
- per-task scripts no longer referenced (`patch_*.py`, `keep_*.py`, `restart_gate_oldroot.ps1` style).

Keep (cited evidence or live kit):

- `C:\mfgtmp\lane_frames`, `vps_lane_frames`, `faceprop`, cited as sources by OKF concepts.
- frame sets named in the newest docs report (for example `mpnew_*`, `p4_08_*`, `wr_match_4_*.png`).
- lane kit: `smoke_local.sh`, `wr_four*`, `wr_task.cmd(.tpl)`, `kill_lanes.ps1`, restart scripts,
  `gate_pid.txt` / lane pid records, the log the running core writes (`ryunet.out.txt`), and the
  gate's live stdout/stderr.
- Never touch anything a running lane process has open. Delete by explicit path, never by wildcard
  sweeps over the whole tree.

### End of every task (this workspace)

- Run a cleanup round before calling a task done, on top of the rules above.
- Sweep every process the task started: harness/probe browsers, build pollers, background jobs.
  Sweep leaked headless browsers by recorded PID (never by name) and check host commit + C: free
  space so a runaway run does not stall the machine (a leaked Edge tree is ~2.3 GB commit each.
  Dozens drove commit to exhaustion and a graphics-driver/OS stall once).
- Leave the lane in a known-good, warm state (gate + core answering) or say plainly what is down.
- Discard scratch probes/one-shot scripts no longer referenced. Keep cited evidence and live kit.
