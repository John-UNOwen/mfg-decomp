# 36. Seat animation / springbone — handoff for the next agent

Date: 2026-11-09. Status: **all defect-2 project changes reverted** (unity repo `a70b969`), so the
project is byte-equal to the pre-experiment state (`3efbea58eb`). Read this before touching anything
animation- or physics-related. Ground-truth diffs: `C:\mfgtmp\defect2_diff\report.md` (full
shipped-vs-recon comparison, 274 lines, every claim anchored to file:line) and
docs/34 section E (original animation mechanism).

## 1. What the owner saw on staging (2026-11-09) — the new facts

The experimental build (stamp `0EC5FC16E761`: springs on `Time.deltaTime`, quality restored to
shipped) was tested by the owner on the staging edge and rejected:

1. **Leg swivel/reset: completely unchanged** by the experiment.
2. **New on that build:** physics-driven parts (hair, chest) glitch out and move wildly/spontaneously
   in bursts, **while the game runs low on system memory**; it subsides afterwards and repeats.
3. Hiyori's bowtie animation stays in sync with her body, **her chest does not**.

All three fit one explanation on a 60 Hz display, which is what matters for the owner's lane:
the owner's browser renders at ~60 fps, so the old baked `simulationFrameRate: 60` already equals
wall clock there — the experiment could not change the leg symptom (obs #1). And `timeStep =
Time.deltaTime` with a springbone `UpdateDynamics` that takes **one** step per rendered frame turns
any long frame (memory pressure → GC/decode stall → 1-3 s frame) into one enormous integration
step: every chain explodes, hits its angular limits, then recovers — bursty, spontaneous, wide
motion that subsides and repeats (obs #2). The chest chain (`stiffnessForce`-soft, longest pendulum)
is the most step-sensitive: body-driven parts (bowtie, parented to animated bones) stay in sync,
spring-driven parts (chest) desync (obs #3). The old fixed step is immune to spike frames (each
frame still advances exactly 1/60 s); its real cost is the 1.4-2x fast-forward, which only exists
at fps > 60 — measurable on the headless lanes (86-120 fps), invisible on a 60 Hz browser.

Reverting was correct. Do not re-apply the deltaTime change without a clamp: the prefab exposes
only the binary choice (fixed 1/N vs raw `Time.deltaTime`). Any frame-rate independence that is
also spike-safe needs a code-level cap (e.g. `min(Time.deltaTime, 1/30)`), which lives in
`Springbone.Runtime.dll` internals — a DLL patch decision for the owner, not a prefab edit.

## 2. Reverted changes (what they were)

| Commit (unity repo) | Change |
| --- | --- |
| `5d0cf207ff` (reverted) | `simulationFrameRate: 60 -> 0` on all 202 SpringManagers across 197 `Assets\MFG\Characters\*\3D\*\Prefabs\CharaModel.prefab` |
| `9d87ade731` (reverted) | `ProjectSettings\QualitySettings.asset` Medium: `pixelLightCount 4 -> 1`, `shadowDistance 0 -> 20.0` |

Note the quality drift is real and still stands in the project (`pixelLightCount 4`,
`shadowDistance 0` vs shipped `1` / `20.0`, per the shipped `globalgamemanagers` quality level read
in the diff report D5). It is orthogonal to springs — whether to restore it is an open owner call;
the KB eye-iris note already treats `pixelLightCount: 1` as the pinned analysis baseline.

## 3. Asset inventory (everything involved, both sides)

Shipped originals (ground truth, read-only):

| What | Where |
| --- | --- |
| CharaMotion01 controller | its own bundle under `\\Myappsandbox\c\Games\MFG\game\MFGClient_Data\StreamingAssets\aa\StandaloneWindows64\` |
| Chara03 / Chara04 controllers | `dba09bb09c23cc659e437e49af7ac762.bundle` / `6c346193ef639acf79422ef89050866a.bundle` (same dir) |
| Seated CharaModel prefab | `5005fb018f625f3651ca4ac49779b9fc.bundle` (same dir) |
| Model + avatar | `d7359c0b36888bc21890a4b155995999.bundle` (same dir) |
| MahjongPlay scene | `\\Myappsandbox\c\Games\MFG\game\MFGClient_Data\level18` (plain serialized file) |
| Quality/Time managers | `\\Myappsandbox\c\Games\MFG\game\MFGClient_Data\globalgamemanagers` |

Reconstruction (`C:\mfg\build\unity-project`):

- `Assets\MFG\Scenes\MahjongPlay\MahjongPlay.unity`
- `Assets\MFG\Characters\Character1\3D\C001\Prefabs\CharaModel.prefab` (template; all 197 follow the shape)
- `Assets\MFG\Characters\Character1\3D\Animations\Motion\CharaMotion01_Game.controller`, clips under `...\Motion\Default\*.anim`
- SpringManager knobs, per prefab: `simulationFrameRate` (:13029), `dynamicRatio` (:13030), `enableCollision` (:13035), thigh capsule colliders `m_Enabled` (:7749, :9603)
- `Assets\Plugins\Springbone.Runtime.dll` (E977199F605B3908) vs shipped `Springbone.Runtime.dll` (AE1F571E1E532F33) — same size, identical method surface, same code
- `ProjectSettings\QualitySettings.asset`, `ProjectSettings\TimeManager.asset` (TimeManager byte-identical to shipped: fixed 0.02, maxTimestep 0.33333334, timeScale 1)

Runtime identity chain proven (diff report "Inputs"): `ModelManager.cs:102-128` -> addressable group
`5005fb018f625f3651ca4ac49779b9fc.bundle.asset:21` -> the prefab above; the diffed assets are what the
table loads.

## 4. What is PROVEN equal (do not re-litigate)

Controller: 34 params / 2 layers / 103 states / 130 per-state + 53 AnyState transitions, all
conditions, exit-times, speeds, `AwaySpeed` bindings, clip bindings — zero deltas on CharaMotion01
at count level (`trans_diff.txt`); delta-class level for 03/04. Skinning: bone-for-bone identical.
Springbone: all 942 physics scalars present in the original payloads (`spring_param_verify.txt`).
Scene: 4 seat frames, `Players` scale, 23 cameras all fields (`seat_frames_and_cameras.txt`).
Write Defaults 103/103, no AvatarMasks either side, loop byte is import-derived (D1). The P0
do-not-patch list in the diff report §4 still holds in full.

## 5. Open questions the next agent should actually chase

1. **Is the side-seat swivel-and-snap present in the shipped game?** The reconstruction's leg curves
   max ~5.8 deg swing (interpolated +0.92 deg); the ±115 deg seats project it ~20x wider than the
   near seat (geometry, docs/34 E.3). Decisive single run: capture the **original** Windows build at
   the same table with the same ROI series, or run P1.1 (mute the `Thigh/Shin` rotation curves in
   `HiyoriWait.anim`, first at :902, or retarget the `Wait0` motion at controller :4072).
2. **Muscle-stream values** remain the one unread input (UnityPy 1.25.4 cannot decode
   `m_MuscleClip` binary; `HiyoriWait` = 37192 B, sampleRate 60). Writing a 2022.3 muscle-clip
   parser would close D6a and settle question 1 without runtime guessing.
3. **Low-memory spring bursts on the reverted build:** the owner should confirm the bursts are gone
   on the reverted staging build. If they survive the revert, they are a different defect (memory
   stalls interacting with Unity's own clamping), not timeStep — capture `Time.deltaTime` maxima via
   the harness beat line during a forced-stall run before theorizing.
4. `dynamicRatio` (0.5) is inert for seated chars: nothing calls `UpdateBoneIsAnimatedStates`
   (docs/34 E.4). Only becomes live if code starts calling it.

## 6. Measurement kit this round built (still valid)

- Lanes free-run at **86-120 fps** headless (`[beat] fps=` lines in `C:\mfgtmp\wr_pN.log.txt`) — any
  time-dependent physics comparison must state the lane fps.
- Table burst: harness bursts `mp_<lane>_NN.png` on MahjongPlay entry (+6 s, 30 frames) — kept set:
  `C:\mfgtmp\mpnew_*.png` (110 frames of the experimental build) and `mp_*.png` (old build A/B).
- `python C:\mfgtmp\mp_roi_diff.py 'C:/mfgtmp/mp_*.png' [x1 y1 x2 y2]` — motion localization / ROI
  sway series. Idle amplitude baselines from the experimental build: front seats 2-10, side 5-26
  (grayscale mean-abs per pair).
- CDP screencast attempt (`MP_CAST` env in `C:\mfg\tmp\webgl_proof.cjs`): captured 0 frames — both
  event names wired, unproven; do not trust it until proven.
- Batch launcher: scheduled task `mfg_wr_batch` (`schtasks /Run /TN mfg_wr_batch`); never kill lane
  nodes from inside a tool call.

## 7. What the owner must still do

Validate on the reverted staging build (`https://mfg-staging.0006767.xyz`, rebuilt from
`a70b969`): leg behavior back to baseline, physics bursts gone under memory pressure, bowtie/chest
sync state. Their answers decide whether questions 1-3 stay open or close.
