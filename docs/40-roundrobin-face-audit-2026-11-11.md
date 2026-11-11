# 40. Round-robin face audit - every opponent vs Musashi (2026-11-11)

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

## Casts (seat: right / left / across - verified per run)

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

## Artifacts (KEEP - cited by this report)

- Shipped frames: `C:\mfgtmp\mp_r{1..8}_06.png` (1920x1080).
- Full burst sets per run: `C:\mfgtmp\mp_r{1..8}_NN.png` (26-30 frames each) - keep until the
  owner's visual pass is signed off; then superseded runs' non-shipped frames may go.
- Logs: `C:\mfgtmp\robin_r{1..8}.log.txt`, `C:\mfgtmp\robin_status.txt`.
- Kit: `C:\mfgtmp\robin_run.ps1`, `C:\mfgtmp\patch_selectchara.py` (reusable; pins +
  per-run SelectChara patch documented above).

## Lane state after

Standard owner pins restored (`3=7,1=17,2=8`), account back to Yao, staging on `931E50A1B0E5`.

## Round 2 (2026-11-11 pm): all-character rim zeroing, same rotation

Owner pass over round 1 named nine characters with the band (3, 6, 10, 11, 13 hard; 8, 9, 19, 20
mid) and observed it angle-dependent: strong at side seats, fading when a character turns to face
the camera - textbook rim (Fresnel) behavior, consistent with the mechanism in docs/33.

Fix: `FixMouthRim` generalized to enumerate every `Mt_mouth` material (`Gate2Content.cs`, unity
`05cc9830a8`); the run zeroed 33 materials - the shared `Assets/Material/Mt_mouth*` family (the
antipodean-ON variant), every `Character1..21/3D/Common/Mt_mouth.mat` including Character10's
`NewCommon`, skipping 10 Standard-shader FBX mats without the property and Ch7/17 (already zero).
Materials committed unity `334f497af0`. Full content rebuild; `rim_verify2.py` proved it shipped:
exactly 33 bundles renamed vs the live root, every material inside reading `rim=0, ap=0`.
New staging stamp `884F48017067` (Baked-16 needle green).

Round 2 (`robin_run2.ps1`, tags `mp_s*_NN.png`, logs `robin2_s*.log.txt`): identical groupings to
round 1, all eight casts verdict-matched (s7 seat 3 drew Dia; unpinned seats pool-draw). Old-vs-new
same-seat pair (`mp_r4_06.png` vs `mp_s4_06.png`, G-14 Ie at the kami seat): band present on old,
absent on new. One s4 flag and one s7 flag from single-frame vision did not survive direct
comparison; the mechanical truth is rim=0 in all 33 bundles. Owner visual pass on the `s*` frames
is the standing verdict.
Artifacts KEEP: `C:\mfgtmp\mp_s{1..8}_06.png` (+ burst sets), `robin_run2.ps1`, `rim_verify2.py`,
`run_rimfix.ps1`, `rim_lane.log`, `robin_status2.txt`.

## Live promotion (2026-11-11 evening, owner-approved)

The rim-zero build (`884F48017067`) went live on the VPS (`mfg.0006767.xyz`, units
`mfg-lane`/`mfg-gate`/`mfg-tunnel`). Live had been `234BB22AA309` - the morning build (rim on Ch7/17 +
quality restore), promoted ~06:17Z, so the delta was rim-all-only in content terms plus the re-hashed
catalog and engine data: **4,231 files / 530 MB shipped, 4,224 dropped names deleted**, staged by
`cp -al` off live + delta tar, verified by re-hashing every shipped file against the new manifest and
spot-hashing 369 unchanged files against the live tree's published manifest (premise held).
`swap_webroot.sh` (first real run of the kit script; tally 0 MISSING/0 EXTRA) promoted with
`SERVICES="mfg-gate mfg-lane"`. `aog.ts` (only plugin drift: the `MFG_CPU_PIN` test hook, off by
default) shipped with a timestamped backup; savedata backed up untouched beforehand.

Post-swap proofs: stamp served at the public edge, a renamed bundle 200/immutable, its old name 404,
served catalog carries the renamed rim groups, `lane_smoke.sh` 17 PASS / 0 FAIL / 3 SKIP, kit boot
against live green (login auth OK, no Addressables key errors - unlike the morning baseline run on the
pre-promotion tree, which threw `InvalidKeyException` spam - and the test account parks at
`NewRegistration`, the documented name-entry step for an account with no live-side game data).
Undo handle kept at `webroot.old-20261111T191040Z-281779` (the `234BB` tree);
the previous generation's aside was freed after smoke passed (486 MB back).

Two lessons banked: live had 12 orphan bundles from an intermediate build (in live, in no manifest) -
they surfaced as EXTRA at staged-count time and were deleted before the swap, which is exactly what
`swap_webroot.sh`'s count gate is for; and pipefail + `head` aborts a list pipeline on SIGPIPE -
write the list to a file, then take the head.
