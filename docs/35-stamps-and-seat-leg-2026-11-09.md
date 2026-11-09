# 35. Sticker stamps and the seat leg loop — fix + verification report (2026-11-09)

Two owner-reported defects were investigated against the live local lane:
"the stamps are fired in the wrong place, and/or don't disappear", and the seated character
leg-animation loop issue (defect 2). Verdicts, evidence and the applied changes below.
VPS untouched per instruction; everything here ran on the Windows lane against the
defect-1-fixed webroot (stamp D33DFEFDA262).

## 1. Stamps — original mechanism (ground truth)

- Balloons exist only in the Matching waiting-room scene: `Canvas/Matching/Players/MatchingPlayerN/StampBalloon`
  (rect anchored (0,5), 180x180, Icon child, scale 0). `MahjongPlay` has no stamp node at all.
- `StampBalloon.Play()`: sprite = `CustomizeItem/{itemID}.png` (Addressable), position =
  seat's `CharacterProperties.MatchingStampOffset` over Awake (0,5), SFX_SYS_INPUT3,
  DOScale 0 -> (1,1,0) 0.3 s OutBack(1.2). Then nothing: no hold, no out, no timer —
  "never disappears until scene exit" is the shipped behaviour, not a bug (docs/34 E).
- Delivery: sender POSTs `/aog/gchat` (`scene=MATCHING&mid&pindex&contents=ItemID&name`) +
  renders locally + 2 s cooldown; receivers pick it up on the 1 s GChat poll (gated on the
  seat being established), with per-(table,player) backlog cursors and SeqID watermark
  dedup; own seat is filtered. CPU seats reply with their own sticker (`maybeCpuStamp`).

## 2. Stamps — live proof on the reconstruction

Batch 2026-11-09 06:41 (5 lanes, all four accounts seated by the lobby split):

- All four lanes posted stamps; core `[VFG]` log shows the table backlog:
  `tid=1115 idx=3 wr4 seat=3, idx=4 wr3 seat=2, idx=5 wr2 seat=1, idx=6 wr1 seat=0, idx=7 CPU reply seat=3`.
- `wr_match_4_08.png` (p4's view, seat N): sticker balloons float above the seated characters
  on panels S and W with the correct per-sticker art (TableSticker002 -> "HIYORI" chibi bubble,
  TableSticker003 -> "CHIFORI"), own seat N blank (self-exclusion working), panel E absent —
  that one stamp had been consumed by the harness's own POST (cursor theft, section 4).
- Balloons persist across consecutive 1 s frames until the MahjongPlay switch — matching the
  shipped persistence; the balloon object belongs to the Matching scene and is destroyed by
  the scene switch, so it does disappear exactly where the original's does.

**Verdict: no delta to fix.** Balloon rendering, seat placement, sticker art selection,
self-exclusion, CPU replies and persistence all match the original. The reported symptoms
are explained: "wrong place" = the historic wrong-seat delivery bug (fixed in the defect-1
round, `handle_gchat` matchSeat authority), and "don't disappear" = original in-match
persistence. L102/L103 remain the standing design notes; no code change was made for stamps.

## 3. Defect 2 — seated leg loop: what was already proven

`C:\mfgtmp\defect2_diff\report.md` (this round): CharaMotion01 controller zero deltas
(34 params / 103 states / 130+53 transitions / 103 clip bindings), 103/103 clip bindings
byte-identical, 239 bones, 78 springbone chains, 23 cameras, TimeManager identical. The
idle clip itself (HiyoriWait) carries a drift-then-snap key pattern (0 / 0.7667 / 1 /
1.7667 / 2 s, thigh swing <= 5.81 deg) that the shipped game contains too; the seats are
pure yaws, so the same swing projects ~20x wider at the +/-115 deg side seats than at the
near seat. The Loop byte on 72 states is import-derived (`m_LoopTime`) — patching it would
have moved the build away from the shipped bytes (P0, not done).

## 4. Defect 2 — changes applied this round

1. **Quality level restored to the shipped values** (`ProjectSettings/QualitySettings.asset`,
   level "Medium"): `pixelLightCount 4 -> 1`, `shadowDistance 0 -> 20.0` (docs/34 D5 — the
   only substantive project-level drift found; light pops read as snaps in captures).
2. **Spring sim decoupled from frame rate** — `simulationFrameRate: 60 -> 0` on all 202
   SpringManagers of all 197 seated `CharaModel.prefab` files. Verified semantics: the UTJ
   springbone `SpringManager.UpdateDynamics` takes `timeStep = (simulationFrameRate > 0) ?
   1/simulationFrameRate : Time.deltaTime` and runs once per rendered frame — with 60 the
   sim advances one 1/60 s step per rAF, so spring speed scales with browser fps (the
   shipped native game ran vsync-locked and never showed this). The shipped DLL
   (`export-primary-content/Assemblies/Springbone.Runtime.dll`) and the reconstruction's
   `Assets/Plugins/Springbone.Runtime.dll` carry the identical method surface
   (`SatisfyConstraintsAndComputeRotation`, `UpdateBoneIsAnimatedStates`, …, same size; the
   hash diff is build metadata), so this applies to both. At a steady 60 fps the behaviour
   is identical to the shipped bytes; in a variable-fps browser lane it tracks wall clock,
   which is what the shipped context always was.
   - `dynamicRatio` was left at 0.5 deliberately: it is only consulted for bones flagged
     animated by `UpdateBoneIsAnimatedStates`, which nothing ever calls for seat characters
     (docs/34 E.4) — patching it is a no-op.
3. **Not applied (deliberately):** the P1.1 leg-curve ablation and P1.2 skirt-collider
   disable — both are diagnostic ablations that make the shipped-faithful data worse, not
   fixes; and P4's cosmetic node moves. They are documented as one-line toggles in
   `defect2_diff/report.md` section 4 if the owner wants the A/B once he is back.

## 5. Verification state (what the owner will find)

- Table burst added to the harness: from MahjongPlay entry +6 s, 30 frames at ~0.45 s
  (`C:\mfgtmp\mp_<lane>_NN.png`) — enough to cover several cycles of the 1.25-2 s sway at
  every seat in one lane; `[beat] fps=` lines now report each lane's real rAF rate.
- `python C:\mfgtmp\mp_roi_diff.py 'C:/mfgtmp/mp_1_*.png'` localizes the moving grid cells;
  with an ROI box it prints the sway time series for a chosen seat region.

### Measured on the fixed build (07:21 batch, stamp 0EC5FC16E761)

- **Lane frame rate is the headline number: the headless lanes free-run at 86-120 fps**
  (`fps=` beats in `wr_pN.log.txt`, one lane settling at 60.7). On the previous build that
  means the spring sim ran 1.4-2.0x wall-clock speed there; on the fixed build the sim is
  frame-rate independent by construction.
- Post-fix idle amplitudes (mean abs luma diff per frame pair, 29-pair series, lane mp_3):
  front-facing opponent ROIs idle at **2-10** with the table event spikes (deal/hand reveal)
  at 58-120; the side CPU seat idles at **5-26**, rising to 26-77 only while its Pon-call
  animation plays. No runaway leg-region oscillation was captured on the fixed build.
- The A/B re-run on the kept old webroot (07:32 batch) reached the table and captured 6-24
  frames per lane, but the main-loop screenshot cadence (1-20 s per frame under lane
  contention) cannot phase-align two runs for a period comparison, and the real-time CDP
  screencast attempt captured 0 frames (event-name mismatch; the fixed handler is in
  `tmp/webgl_proof.cjs`, gated on `MP_CAST`, not proven this round). The before/after
  period comparison is therefore left to the owner's eye on the lane, as is the judgement
  of whether the remaining side-seat sway matches the shipped game.
- Kept artefacts: `C:\mfgtmp\mpnew_*.png` (110 fixed-build table frames), `mp_*.png`
  (old-root A/B frames), `p4_08_sheet.png` + `crop3x_19_*.png` (stamp balloons), batch logs
  under `C:\mfgtmp\wr_archive\`.

### Not verified (stated plainly)

- No pixel-level proof that the fix removes the owner's specific artifact — the lane shows
  modest idle motion on the fixed build at 90-120 fps, but "matches the original look" is a
  human call the owner reserved.
- The muscle-stream curve values remain undecodable (UnityPy 1.25.4); if the fix does not
  satisfy, the report's P1.1 ablation is the next decisive single run.

## 6. Harness gotchas found on the way (not app bugs)

- IL2CPP WebGL carries POST bodies as fetch Blobs — CDP `postData` sees nothing; the harness
  now tees the exact bytes to its console (`[WIRESRC]`) and builds each lane's own wire identity from them.
- A harness-side `/aog/gchat` POST advances the sender seat's backlog cursor, so the stamp
  it returns is consumed away from the game client of that same seat (seat E in section 2).
- A stale `fireStamp()` call site crashed every lane ~103 s in (at first reveal) — fixed.
- Tool-runner process supervision can retry/killed-sweep detached lanes; batches now run
  from a Windows scheduled task (`mfg_wr_batch`), outside the tool call's process tree.
