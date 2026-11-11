# 37. Round-robin face audit — every opponent vs Musashi (2026-11-11)

Purpose: after the mouth-card rim fix (docs/31 L38; rim zeroed on Character7/17 via
`MfgRebuild.Gate2Content.FixMouthRim`, unity `579ed0ceb4`), random CPU casts made it
impossible to tell whether any other character showed the bright chin band. This round
pinned every non-Musashi character as an opponent across eight smoke runs, Musashi at the
human seat, so each face is seen once in the kami seat (facing camera) plus a profile seat.

## Method

- Test account `lanchtest2` (LANCH2) flipped to Musashi by editing the stored profile:
  `SelectChara 4 -> 7` in the `player_game` state of doc `6A0F6406C5D9140B`,
  `tmp/RyuNET-core/dist/savedata/mfg@asphyxia.db` (helper `C:\mfgtmp\patch_selectchara.py`;
  reverted to 4 at the last run). The core must be down for the edit; it reads the doc on login.
- Opponent pins: `MFG_CPU_PIN="seat=chara,..."` (seat 1 = right, 2 = left, 3 = across),
  read by `matchingXml` in `tmp/RyuNET-core/dist/plugins/mfg@asphyxia/handlers/aog.ts`.
- Kit: `C:\mfg\tmp\webgl_proof.cjs` via loop script `C:\mfgtmp\robin_run.ps1`
  (status log `C:\mfgtmp\robin_status.txt`, per-run logs `C:\mfgtmp\robin_rN.log.txt`).
  Captures at `CAP_W=1920`, tag `mp_rN_NN.png`.
- Build under test: staging root `931E50A1B0E5` (rim fix + restored quality + 0.5/0 outlines).
- Cast identity taken from each run's `[verdict]` standing-load line, never from guesswork.

## Casts (seat: right / left / across — verified per run)

| Run | Verdict cast | Right | Left | Across | Frame shipped |
|---|---|---|---|---|---|
| r1 | 08 01 02 03 | Hyori | Sen | Tsumire | `mp_r1_06.png` |
| r2 | 08 04 05 06 | Mitsuba | Yao | Tenshi | `mp_r2_06.png` |
| r3 | 08 07 09 10 | Toitoi | Pine | Shiori | `mp_r3_06.png` |
| r4 | 08 11 12 13 | Chaos | Clear | G-14 Ie | `mp_r4_06.png` |
| r5 | 08 14 15 16 | Aloe | Koa | Dia | `mp_r5_06.png` |
| r6 | 08 17 18 19 | Dabriel | Ippatsu | Shindoe | `mp_r6_06.png` |
| r7 | 08 20 21 09 | Jun | Toka | Pine (seat 3 unpinned, pool drew it again) | `mp_r7_06.png` |
| r8 | 05 01 02 08 | Hyori | Sen | **Musashi** (review run, Yao main) | `mp_r8_06.png` |

## Findings

- All eight shipped frames were pre-checked in-play (no win splash/result).
- Automated screen (vision pass per run): no bright white mouth/chin patch on any face.
  Toitoi (r3 right) and Dabriel (r6 right) double as the fixed-character before/after.
- Owner judgment on the shipped frames is the standing verdict; nothing reported against
  any of the 21 characters since.
- Census ground truth this round builds on (see OKF `references/lower-face-grey-band-mechanism`):
  18 characters still run the white rim-on default live; visibility depends on card geometry.

## Artifacts (KEEP — cited by this report)

- Shipped frames: `C:\mfgtmp\mp_r{1..8}_06.png` (1920x1080).
- Full burst sets per run: `C:\mfgtmp\mp_r{1..8}_NN.png` (26-30 frames each) — keep until the
  owner's visual pass is signed off; then superseded runs' non-shipped frames may go.
- Logs: `C:\mfgtmp\robin_r{1..8}.log.txt`, `C:\mfgtmp\robin_status.txt`.
- Kit: `C:\mfgtmp\robin_run.ps1`, `C:\mfgtmp\patch_selectchara.py` (reusable; pins +
  per-run SelectChara patch documented above).

## Lane state after

Standard owner pins restored (`3=7,1=17,2=8`), account back to Yao, staging on `931E50A1B0E5`.
