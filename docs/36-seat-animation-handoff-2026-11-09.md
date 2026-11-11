# 36. Seat animation / springbone — handoff for the next agent

Date: 2026-11-09. Status: **defect-2 root cause found and fixed 2026-11-10 — read section 8 first**
(`-Infinity` tangent tokens in the extracted `.anim` files; sanitized + verified). The pre-fix
history below (springs reverted, unity repo `a70b969`) stands as written and explains what was
tried before the data cause was pinned. Ground-truth diffs: `C:\mfgtmp\defect2_diff\report.md`
(full shipped-vs-recon comparison, 274 lines, every claim anchored to file:line) and
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

## 8. Root cause found (2026-11-10): `-Infinity` tangents in the extracted `.anim` files

**The snap is corrupted animation data, not game code and not springbone.** The extraction tool
that rebuilt the repo's `.anim` files wrote the literal YAML token `-Infinity` as the slope for
flat components (e.g. the constant-zero x/z of quaternion curves). Unity 2022.3.13f1 imports
`-Infinity` as a `-inf` tangent and the editor evaluates those curves **correctly** — every
editor-side check passes. But the player content packer compiles any quaternion curve carrying a
non-finite tangent into **stepped, one-key phase-shifted stream keys** (all tangents zeroed, each
key's value replaced by the next key's value). The runtime then faithfully evaluates that step
curve: the shin holds one pose and teleports — the observed ~1 s snap.

Why every earlier check missed it:

- The shipped desktop bundle holds the same clip with finite tangents and glides (decoded with
  UnityPy: `2abba23a…` group, shin y keys `t=0 → -0.097548 os=-0.037751`).
- The repo YAML parses smooth — hermite math on the key tables glides (the tangents *in the file*
  are fine; only two of four components carry the sentinel).
- `AnimationClip.SampleAnimation` on the live loaded clip reproduced the step in the browser
  (run3 log: shin pinned at -0.12557 from ct 2.0 to 2.6333) — proof the *loaded stream data* was
  bad, not the evaluation path.
- Fresh re-packs from the repo YAML reproduce the bad bytes deterministically (new content hash,
  same step) — so it is not a stale-build artifact; the packer itself produces it.
- Toitoi's clip has zero sentinel entries and packs smooth; in the broken Yao clip the thigh (all
  finite slopes) packs smooth next to the stepped shin — the discriminator is per-curve.

**Blast radius** (old staging webroot, UnityPy scan of 97 character Wait/Idle bundles): 17 bundles
carry at least one stepped quaternion curve — characters 1 (Hiyori), 2 (Sen), 5 (Yao ×3),
8 (Musashi ×2) fully sentinel-caused; 11 (Chaos), 14 (GAloe), 15 (Cocoa), 16 (Dia), 17 (Dubly)
carry stepped curves of the non-constant class (below). Toitoi, iyo and the rest are clean —
matching the owner's per-seat report.

**The fix** (`C:\mfgtmp\sanitize_inf.py`, applied 2026-11-10): replace the sentinel with `0`
**only where that component's value is constant across the curve entry** — provably correct for a
constant curve. Applied under `Assets/MFG/Characters`: 1,488 files, 11,759 tokens replaced, diff
contains only `inSlope/outSlope` lines. Re-bake → the 2abba group's new bundle (`bf285fa3…`)
decodes byte-equivalent to the shipped desktop tuple: `Shin_L y: t=0 -0.09755 os=-0.0378,
t=0.6333 -0.12557 os=+0.0893, …`. Nine of the 17 bundles are fully fixed (Yao both costumes +
swim, Musashi both, Hiyori both, Sen).

**Runtime verification** (probe build, run4 `C:\mfgtmp\legprobe_run4.log.txt`, webroot
`CC2F363962E8`): `SampleAnimation` sweep of the live YaoWait clip now slides through the old snap
window (SLy 2.0→-0.09755, 2.1→-0.10215, 2.3→-0.11893; run3 pinned all -0.12557). Per-frame shin
delta profile moved from 59 jumps >0.02 (max 0.1221) to 7 (max 0.0447) — inside the band of the
known-healthy Toitoi (9, max 0.0962). One 389-frame flat run remains with `n0` advancing
throughout: animator culling freezes bone writes while the state clock runs, the hold value is the
t=0 loop pose (smooth rest, no jump) — a cull artifact, not the defect.

**Follow-up class (open):** 33,669 sentinel tokens sit on **non-constant** components (the 8
bundles that remain partially stepped, incl. Dia/Chaos/Cocoa/Dubly/GAloe idles). Zeroing those
would flatten real motion. The ground truth for their tangents exists: the shipped bundles' stream
keys carry the true in/out slopes (e.g. Sen's shipped shin y slope -0.0378). A shipped-stream
reconstruction pass is the right repair; nobody has run it.

**Bake mechanics learned this pass** (unity repo, uncommitted `Gate2Content.BakeContentOnly` =
`AddressableAssetSettings.BuildPlayerContent`): the per-bundle skip is Unity's content
**BuildCache** (`Library\BuildCache`, keyed on imported artifact bytes) — editing the YAML text in
a way that doesn't change the imported object (a comment) will not re-pack; a real data change
will. Deleting `addressables_content_state.bin` alone does **not** force a re-pack. The batch-mode
content build **deletes** `Assets/AddressableAssetsData/link.xml` (tracked, commit `21fa555321`)
and never restores it — `BakeContentOnly` now snapshots/restores it around the build.

Staging note: the lane currently serves the **probe build with fixed content** (webroot
`CC2F363962E8`, undo handle `C:\mfgtmp\webroot.old-20261111001734-128140`). Promotion shape still
needs the probe-chain revert + rebuild (chain tip: drop-CurveCount fix, 5 commits over
`df0af9ed90` + uncommitted Gate2Content tooling).
