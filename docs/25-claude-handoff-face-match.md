# 25 - Handoff for Claude: rebuild face does not match the original (2026-10-30)

Scope of this document: define the open issue precisely, list every resource (all already on this
machine), record what was attempted and why each attempt fell short, and note how the project got here.
Companion deep-dives (read them, they carry the numbers):
- `~\Documents\GitHub\mfg-decomp\docs\24-face-asset-inventory.md` - asset map + scene ground truth + gaps closed
- `~\Documents\GitHub\mfg-decomp\build\_tmp_audit_moutheye\disassembly_report.md` - the original Mouth/Eye shader behavior, line-cited from binary analysis
- `C:\mfgtmp\fresh\FINAL.md` + `C:\mfgtmp\fresh\registration_report.md` - corrected measurements and the three mouth defects
- `C:\mfgtmp\eye_fix_report.txt` - full session history including retracted claims
Knowledge base (optional context): `%USERPROFILE%\knowledge`, project mfg, concept `reconstructed-face-shader-contract`.

## 1. The issue, defined

A fan WebGL reconstruction of the game installed on this machine (internal name throughout the
project: MFG; Unity 2022.3.13f1, project `C:\mfg\build\unity-project`) renders Character1 on the
character-select screen with three remaining mismatches
against the original game. The hero symptom: **the drawn smile line is completely absent**, while the
original shows a short, soft, dark-red closed-mouth line. Measured root state:

1. Sampling mismatch (primary). The runtime expression state is atlas index 12 (a smile anim drives
   `_mouthIndex` 12 and swaps in the shy face map). The original shader feeds mesh uv into the atlas
   lookup RAW at slope 1, adding only 0.25-cell steps per index, so at index 12 it samples
   `v = uv.y - 0.75` and lands on the smile ink (atlas rows 931-962 of `mdl_mouth.png`). The rebuild's
   hand-written shader instead compresses uv (`v*0.25 + cellOrigin`), and the rebuild mesh uv was
   rescaled by a glTF round-trip; live sampling therefore hits cell-12 background (rows 783-825) and
   the computed line weight is zero. The layering fix below is already in place, so nothing occludes
   the card: the artwork simply never gets sampled.
2. Placement offset. The live mouth card projects ~31 px lower on screen (capture rows 293-336) than
   the shipped geometry predicts (242-305, verified trustworthy: in the original, the neck ribbon
   reappears exactly at the predicted bottom row 306). Cause unresolved; suspect the mouth mesh's neck
   bone weights or a runtime bone pose.
3. Missing silhouette rims. The shipped prefab renders 12 skinned meshes including 6 outline copies
   that paint a ~3 px dark rim; the rebuild serialized those 6 disabled. The original's line reads
   partly as rim contrast, so rims must come back for any fair color comparison.

Acceptance test (falsifiable, same harness as the artifacts): rebuild capture 1280x720 at select park,
registered to `C:\mfgtmp\ref_original_charselect.png` (1778x1026) by the two-iris similarity protocol
(`C:\mfgtmp\fresh\mfg_reg2.py`, scale ~1.537, hairline verify <1 px). Pass requires: dark line present
at rows 299-302 +-4 across cols 210-280 (tone near original ~(164,93,77) after rims enabled); ribbon
visible from row 306; rim pixels along the silhouette; then per-region deltas (forehead/cheeks/nose/
eyes/mouth/chin from `registration_report.md`'s table) driven to small values. The rebuild currently
shows, e.g., nose -82 lum and mouth-line +112 lum partly BECAUSE of defects 1-3; after fixing them the
residuals that remain are tuning work (shader lighting terms, iris winner values tuned under the old state).

## 2. Why previous attempts did not fully work

| # | Attempt | Result | Why it fell short |
|---|---|---|---|
| 1 | Mouth pass left at shader defaults (queue 2000) | Line invisible everywhere | The face material (queue 2449, ZWrite On) painted over it. Proven: at ZTest Always the artwork appears. Kept as fixed (do not revert): Tags queue 450 after face + `ZWrite Off` + `ZTest LEqual` + vertex bias `o.pos.z -= 0.003*o.pos.w` in Mouth.shader. This exposed defect 1; it did not cause it. |
| 2 | Art-luminance keyed line weight | Dark slab 3x wider than the mouth, too red | The atlas cell background is a flat bright color (230,172,136), so luminance-keyed weight floods the whole cell. Binary analysis later confirmed the original weight is mask-red only: `a = mask.r`, alpha-over. Removed. |
| 3 | Auto-LOD mip sampling | Soft line smeared into a ~30 px band | Bilinear LOD0 (`tex2Dlod(..., 0)`) is what the original's soft sub-pixel AA actually is; mips bleed the mask. Fixed (keep). |
| 4 | Fitted sampling v-shift -0.13/-0.145 (debug build drew a crisp line) | Line appeared, positioned right *relative to the card* | The shift has slope 0.25 while the authored transform has slope 1; the two only cross near uv.y~0.83, so it compensates the glTF-rescaled rebuild mesh uv at one band and is wrong elsewhere. It proved the ink target is correct, but it is not the authored mapping and hides defect 2 rather than fixing it. |
| 5 | Comparing captures with eye-center translation only | A full day of wrong conclusions (row "bands", the bow mistaken for the mouth, a wrong target color) | The two captures differ in scale (~8%): a scale+rotation+translation (similarity) fit on two iris centroids is required. Fixed tool: `mfgtmp\fresh\mfg_reg2.py`; all old below-eye claims were retracted. |
| 6 | Hypothesis: original warmth needs a warm scene light | Would have led to wrong scene tuning | Disproved: no runtime mechanism enables the warm directional light; the original's warm cheeks are the shy expression map itself (texels ~(246,216,205)) under white light. Select lighting stays as shipped. |
| 7 | Bone-parented full-head hull sheet as the face skin surface | Worked visually, kept the rebuild from being broken | Superseded by ground truth: the shipped game surfaces the face aperture with the band-shell `model_face` itself (492/728 tris on artwork); the shipped prefab has no face-plane renderer. Our sheet (`model_face_front`) is a vestigial artifact; the current build already renders shell-only, which is correct. |
| 8 | Assume runtime expression state is serialized 0/0 | Wrong | The select screen's smile anim drives index 12 + shy base map in both games. Any static analysis at index 0 sees the wrong atlas cell. |

## 3. Open problems, ranked, with the experiments

A. **Mesh/shader pair for atlas sampling (primary).** Two internally consistent worlds exist:
   (authored) raw slope-1 band-placed mesh uv + additive 0.25-per-index shader steps;
   (current rebuild) meshes normalized to full-column islands + `*0.25` shader compression.
   The rebuild's 15 C2/C4 mouth meshes were remapped to full-column to fit the `*0.25` formula; the
   C1 mouth island was not remapped but its uv is glTF-rescaled, so C1 samples background.
   Recommended path: switch the rebuild to the authored world for mouths - restore original mesh
   channel values from the dumps (original C1 island: slope-1 band inside v [0.75, 1], x <= 0.2475)
   and implement the additive formula (`u = (uv.x + 0.25*smod4(i) + offX)*ST`, `v = (uv.y - 0.25*row(i) + offY)*ST`,
   art and mask share coords, ST is identity, from `disassembly_report.md` mouth_asm08:289-304).
   Falsifiable: index 12 lands on rows 931-962, line visible with mask-only weight, no fitted constants.
   Known ambiguities (report sections 4.3/5): which mesh uv channel the game feeds TEXCOORD0 (its
   serialized UV0 slot is disabled - the sequential-fill reading is the consistent one), and sampler
   wrap mode at row boundaries (CLAMP vs REPEAT affects exact row landing). The Eye shader's formula
   already matches the original; only the PUPIL grid deviates (authored: +0.5 per index in u, no
   vertical change; rebuild adds a -0.5 v-step and `_IrisMap`-scaled offsets).
B. **+31 px live projection offset.** Candidates: mouth mesh weighted to the neck bone in the rebuild
   but not the original (compare skinning weights: rebuild asset vs bundle dump), runtime Head/Neck
   euler at park vs static assumptions, or a renderer-bounds discrepancy. Experiments: dump bone weights
   and bounds for `model_face_mouth` in the rebuild vs the shipped dump; runtime bounds via CDP during
   the capture hold. Do not paper over it with `_MouthOffsetY` (the anim zeroes it).
C. **Re-enable the 6 outline SMRs** (`group outlien` renderers in C001 CharaModel.prefab, currently
   serialized disabled in the rebuild) to match the shipped 12 enabled. Cheap; do before color tuning.
D. **Tuning residuals after A-C**: rebuild Mouth/Eye shaders multiply lighting by an ambient hack
   (`LightTint(UNITY_LIGHTMODEL_AMBIENT * 2)`) while the face uses the proper toon formula
   `max(ambientSH, _LightColor0)`; the `disassembly_report.md` section 4.5 lists baked constants the
   rebuild does not model 1:1 (sRGB curve coefficients, luma weights, shade gates, lod-as-blur trick,
   discard threshold). Iris/mouth "winner" constants were tuned under the old misalignment and wrong
   sampling - re-measure after A-C before touching them.

## 4. Resources (all local, nothing needs new extraction)

**Folder layout first.** Base your session in `C:\mfg`: it is a junction to this repo folder
(`~\Documents\GitHub\mfg-decomp`), and it contains everything that matters - the rebuild project at
`build\unity-project` (the working codebase), all docs and analysis dumps (`docs\`, `build\_tmp_audit*`),
the extracted reference data (`build\export-primary-content`), and the capture/build machinery
(`tmp\`, `mock\`, `webfront\`, `out\webgl`, `scripts\`). Second root for scratch: `C:\mfgtmp`
(harness scripts, capture frames, backups). The game install lives outside both under a sandbox path
(`\\MYAPPSANDBOX\c\Games\MFG\game`); you do not need it - the reference captures below are its stand-in.

- Rebuild project (this is the working codebase, all authored by us): `C:\mfg\build\unity-project`
  - `Assets\Shaders\Character\Mouth.shader` / `Eye.shader` (hand-written reconstructions; current
    mouth state = queue/bias layering fix + mask-only LOD0 composite; pristine backup of the
    pre-tuning version at `C:\mfgtmp\Mouth_shader_backup_pre_debug.shader`)
  - `Assets\MFG\Characters\Character1\3D\...` prefabs/meshes/materials/textures, `Assets\Editor\Gate2*.cs`
    (build + automation), scene `Assets\MFG\Scenes\FirstCharaSelect\FirstCharaSelect.unity` (the parked capture scene)
- Original-side reference data (already extracted, read-only reference):
  `~\Documents\GitHub\mfg-decomp\build\export-primary-content` (meshes incl. per-character Fbxs glb
  dumps with original uvs, textures, materials, scenes, shader jsons with property tables + program blobs)
- Binary-analysis dumps of the original compiled shaders: `build\_tmp_audit_moutheye` (78 disassembled
  programs, inventories, cbuffer layouts, annotated forward passes) and `build\_tmp_audit_face`
  (the toon face shader audit, 52 programs, `FINAL_REPORT.md`).
- Original game install (read-only, runnable): the workspace root of this session
  (`\\MYAPPSANDBOX\c\Games\MFG`, ≈ `C:\mfg`), 28k asset bundles + catalog.
- Reference captures: `C:\mfgtmp\ref_original_charselect.png` (original screen, 1778x1026);
  rebuild frames `C:\mfgtmp\blush_hold_*.png` (+ baselines `C:\mfgtmp\prev_round\r0_*`).
- Tooling: `C:\mfgtmp\debug_round.ps1` (one command: Unity batchmode build -> bundle assemble ->
  mock re-arm -> 4 captures -> closes all windows; ~7 min; occasional spurious "BUILD NOT CLEAN" log
  line is cosmetic), `C:\mfg\tmp\blush_capture.cjs` (headless capture client),
  `C:\mfgtmp\fresh\mfg_reg2.py` (registration + region stats), static file server 8094 (`serve_aog.js`,
  check it is alive, restart per KB runbook `%USERPROFILE%\knowledge` -> mfg -> `front-server-plumbing`
  if not). Mock game servers may already be running (that is fine; debug_round re-arms them).
- Environment: Windows, PowerShell 7, Python 3 with Pillow/numpy/UnityPy/lz4 installed, Node.js with
  playwright installed. Unity 2022.3.13f1 with WebGL module at `C:\Program Files\Unity\Hub\Editor\2022.3.13f1`.

## 5. Provenance and boundaries (how the project got here, in case it matters to you)

The reverse engineering (ripping meshes/textures/materials and disassembling the two bespoke compiled
shaders) was already performed on this machine, strictly for interoperability and analysis of a fan
reconstruction: the install is read-only, nothing ripped is
redistributed, and the analysis outputs are recorded facts (uvs as numbers, sampling formulas as math,
constant values), not vendored code. Practical implications for you:
- You do not need to decompile, extract, or open a game bundle for this work. The behavior of the two bespoke original shaders is
  documented as formulas + constants with evidence line numbers; math is not copyrightable, and the
  reconstruction implements it in independently written HLSL.
- Note on internal names: the game's own identifiers stay in the data you will see (the `MFG.*` code
  namespaces, scene paths, mesh file names, and a few decompiled class names carry the commercial
  title and character names). This document's neutral wording is a tone choice for the handoff, not a
  claim that no IP traces exist - the local reference data obviously carries the original art.
- The toon face shader the project leans on is UnityChanToonShader v2.0.9 (MIT), and the TextMeshPro
  shaders are MIT; those vendored sources carry their own license files in the project.
- Ripped textures/meshes stay local reference data (a visual 1:1 comparison requires them); they are
  not distributed anywhere.
- Whatever you conclude about the provenance of the install, the task itself stands on its own: the
  work is analysis of our own reconstruction files plus reference images, done by editing files under
  `C:\mfg\build\unity-project` (our code) and measuring against captures.

## 6. Working rules for this lane (please follow)

- Verify before claiming: owner's in-game eyes and raw pixel numbers outrank any model's visual
  description. A vision desk misread this scene repeatedly (called the bow "a tan neck"); trust scripts
  that print rows/columns. Never report "fixed" without a fresh capture + numbers.
- After any capture round, leave zero game/browser windows open (debug_round enforces this; if you
  script your own capture, close everything, kill stray `msedge.exe --user-data-dir=...edge-blush`
  processes and prune temp dirs).
- Poll for completion (processes, files, ports); no long sleeps. Timeout waits are fine, idling is not.
- Do not modify the installed game (it sits outside the two roots anyway). Writes stay under
  `C:\mfg\build\unity-project`, `C:\mfgtmp`, and this repo's `docs\` / `build\` dirs (repo: commit only
  when the owner asks; one-line imperative messages; the knowledge base at `%USERPROFILE%\knowledge` is
  updated with its own tool).
- `build\unity-project` is git-ignored and its `Library` folder holds tens of GB: never scan it
  recursively, address files by exact path, and snapshot each file into `C:\mfgtmp\claude\backups`
  before editing it (no per-file git undo covers that tree).
- Debug-only experiments in shaders are welcome but revert the shipping files afterwards (there is a
  known-good backup; re-verify the hash before finishing).
- Report format: what you changed, files touched, measurements before/after, what is verified vs
  assumed, what remains.

## 7. Suggested sequence for you

1. Read the four companion docs listed at the top; reproduce the measurement pipeline yourself
   (run `mfg_reg2.py` on `C:\mfgtmp\blush_hold_01.png` vs the reference; confirm the numbers match
   `registration_report.md` so you trust the harness).
2. Dump and compare skinning weights + screen-space bounds of `model_face_mouth` (rebuild asset vs
   original glb/bundle dump) to attack problem B before writing any shader change.
3. Implement problem A for Character1 only: authored additive sampling in Mouth.shader + restore the
   C1 mouth island uv from the original dump. One debug capture. Expect: line appears at index 12 with
   zero fitted constants; if its absolute row is still 31 px low, defect B confirmed as separate.
4. Enable the 6 outline SMRs, capture again, run the acceptance check (section 1) and the region table.
5. Only then: lighting-formula and iris/mouth retuning against corrected numbers; then propagate the
   authored uv convention to the other three displayed characters (their C2/C4 mouth meshes are still
   in remapped form and must move together with the shader).
Deliverable: a short report in `C:\mfgtmp\claude\` (create it) with before/after numbers per step,
and either the shipped files left fixed (state exactly which) or a proposal with evidence.
