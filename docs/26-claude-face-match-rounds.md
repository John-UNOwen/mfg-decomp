# Face-match lane report (2026-10-30) - smile line, registration, outlines

## Summary
- The smile line now renders in the rebuild at rows ~307-315 (curve bottom col ~257), against a
  corrected target of rows ~312-313. It uses the original's additive atlas formula, with no fitted
  constants.
- **Defect B (the "+31 px mouth offset") does not exist.** It came from the registration tool:
  mfg_reg2.py's "iris" landmarks sit on the bangs in both images (original ~40 px, rebuild ~50 px
  above the real pupils). Its amber mask also matches red hair. Registered on the real pupils and
  the nose dot, the chin edge and neck strip line up within ~1-2 px. The "lower face 40 px low"
  finding, the unexplained x1.47 RT->screen scale (it becomes ~1.07 with the real pupils) and every
  region table built on mfg_reg2 are artifacts of this.
- The C1 mouth uv did not need restoring: the rebuild asset uv0 is the original channel
  (113 verts low island at v=0, 183 in the band 0.774-0.94; original 113 / 176 by 8-bin histogram).
  The only "glTF change" is the v-flip, which the importer undid. Only the shader formula was wrong.
- The 6 outline renderers are enabled. Rims now appear on the silhouette, but the mouth and eye
  outline copies render **black**: they sample mdl_face.png with their atlas-column uv0 (94% of
  samples land on black texels). This needs a decision (see Remaining).

## Files changed (backups in C:\mfgtmp\claude\backups\)
| file | change | sha256 now |
|---|---|---|
| Assets/Shaders/Character/Mouth.shader | `_MouthUVMode` property (default 0 = unchanged legacy). `MouthAtlasUV()` in both passes: mode 1 = `uv + (0.25c, -0.25row) + offset`, c = sign(i)(abs(i) mod 4), row = (i-c)/4 (asm08:289-304). Mode 1 samples art/mask with implicit mips (asm 2.3: plain `sample`); mode 0 keeps LOD0. | 7e63c1f9... |
| Character1/3D/Common/Mt_mouth.mat | `_MouthUVMode: 1` (shared by C1 costumes C001-C010, all band layout; C003/C005/C006 did not decode with my quick parser - unverified) | 2a73b301... |
| Character1/3D/C001/Prefabs/CharaModel.prefab | 6 `group outlien` SMRs m_Enabled 0->1 (exactly 6 lines) | fb0185fb... |
| Eye.shader | untouched (hash = backup) | fa965028... |
Character2/Character4 mouth meshes are remapped (v 0.263-0.672 / 0.384-0.847). Their materials do
not set the flag, so they keep the legacy mapping.

## Measurements
Registration (mfg_reg3.py: pupils as darkest blob + nose dot, least squares): scale 1.297,
rot -2.75 deg, residuals <=1.6 px; neck top 480.8 vs 482; chin edge 479.5 vs 481.
Original line: row 458 at midline (lum 192, (226,183,168)) -> rebuild target row 312-313, cols 238-269.

Smile line (diff vs baseline r0_blush_hold_01, 4 frames each):
| round | line rows | cols | depth | color |
|---|---|---|---|---|
| baseline | none | - | - | blown (255,250,254) |
| A (authored mapping, LOD0) | 310-315 | 246-268 | -45 lum | (226,200,203) |
| BC (+mips, +outlines) | 307-315 (curve) | 246-270 | -32..-37 | (238,205,208) |
The line is in place and has the right shape, but is ~2x too pale. That follows from the blown
mouth-card litBase: surrounding card 251-255. Original line contrast is ~30-40 lum on skin ~230.
Weak columns along the flat bottom are a ~1 px line split across two pixel rows (AA), not aliasing.

Region deltas, rebuild - original, lum (mfg_reg3 boxes):
| region | baseline | BC |
|---|---|---|
| mouth_line | +31 | +23 |
| chin | +26 | +27 |
| cheek_L / cheek_R | +31 / +7 | +22 / +7 |
| nose | +12 | +13 |
| eye_L / eye_R | -18 / -20 | -30 / -24 (outline rims darken the eye boxes) |
| neck | +9 | -2 |
| face | +6 (B +21) | +3 (B +19) |
The skin is consistently blue-heavy (B +20..+49) and too bright below the eyes. That is the mouth
card's litBase and face lighting (handoff D: the `LightTint(UNITY_LIGHTMODEL_AMBIENT*2)` hack +
lavender flat ambient).

Rims: the original's chin edge is (221,161,147)/(179,104,89). The rebuild now has an extra
(0,0,0) row at 331, then (179,104,85). Near-black pixels in the face box: 18 -> 102.

## Verified vs assumed
- Verified by pixels: line position/shape in 8 frames; old landmarks on hair; neck/chin/nose fit;
  rim black at the chin; served build fresh (webgl.data 00:31).
- Verified by data: mouth/eye/face bindposes identical (Head); all 12 SMRs share one bone list;
  face shell weights equal the original glb exactly; mouth uv0 = original channel.
- Assumed: the outline black comes from the mouth/eye outline copies (by uv sampling, not by
  per-renderer isolation); the original glb's 0.75 Hips / 0.25 Head mouth/eye weights are an
  exporter artifact (irrelevant at bind pose; the rebuild's 100% Head matches the eye placement).

## Remaining (ranked)
1. Outline uv for the face-part copies: the mouth/eye outlines need face-layout uv (their uv1)
   rather than the atlas uv0, or a separate material. Note disassembly_report 4.3's "UV0 disabled =
   channel3" is the COLOR channel; its UV1/UV2 labels are mesh uv0/uv1, and it lists them swapped
   relative to the rebuild asset (rect first, band second). Worth re-checking before relying on it.
2. Lighting (handoff D): mouth/eye ambient hack -> face toon formula; this is what makes the line
   pale and the skin blue. Then re-measure the region table.
3. Eyes: the rebuild iris art is visibly distorted (grey ring; pupil-grid deviation in report 4.4).
4. Propagate the authored mapping to Character2/4 (restore their band uv, set the flag).
5. Harness: mfg_reg2.py should be retired for mfg_reg3.py. Edge 156 detaches from its launcher, so
   puppeteer.launch fails; use blush_capture_connect.cjs (spawn + connect) via round_full.ps1.

## Artifacts (C:\mfgtmp\claude\)
mfg_reg3.py, reg3_*.log/json, frames_A/, frames_BC/, compare_A_BC.png, mouth_compare_A.png,
scripts/ (predict_ink.py, line_measure.py, outline_smrs.py, glb_dump.py, asset_mesh*.py),
blush_capture_connect.cjs, capture_only.ps1, round_full.ps1, round_*.log, backups/.

---
## Round DE (2026-10-30 16:17): outline uv fix + UTS2 lighting for C1 mouth/eyes

### Changes (backups: backups/*.orig, backups/Mouth.shader.roundBC)
| file | change |
|---|---|
| Assets/Shaders/sg_cha_outline.shader | `_OutlineUV` (0 = uv0 default, 1 = uv1) |
| Character1/3D/Common/Mt_outline_facepart.mat (+ .meta, new guid ab3ad06c...) | copy of Mt_outline_face with `_OutlineUV: 1` |
| C001 CharaModel.prefab | outline copies of model_face_mouth / eye_L / eye_R now use Mt_outline_facepart (3 lines) |
| Mouth.shader, Eye.shader | `_UtsLighting` (default 0 = legacy). Mode 1: light = UTS2 `max(SH ambient, _LightColor0)` (UCTS_DoubleShadeWithFeather.cginc:282,287) instead of the lavender ambient*2 tint; rim = UTS2 rim + antipodean rim (cginc:327-334), which honours `_RimLight_InsideMask` and subtracts `_Tweak_LightDirection_MaskLevel` (legacy added it, ignored the inside mask -> ~0.5 white over the whole card). Six UTS2 flags added as hidden properties (UTS2 defaults). |
| Mt_mouth.mat, Mt_eye_L.mat, Mt_eye_R.mat | `_UtsLighting: 1` |
Why uv1 for the outline: the face-part uv1 is the face-layout rect (mdl_face samples: mouth 0% black,
eyes 0% black); their uv0/uv2/uv3 are atlas strips (59-94% black).

### Results (4 frames, mfg_reg3)
| region (lum delta vs original) | baseline | BC | DE |
|---|---|---|---|
| mouth_line | +31 | +23 | **-8** |
| mouth_up | +5 | +5 | **-1** |
| chin | +26 | +27 | **-4** |
| cheek_L / cheek_R | +31 / +7 | +22 / +7 | **-3 / -9** |
| nose | +12 | +13 | -16 |
| between_eyes | 0 | -8 | **-33** |
| eye_L / eye_R | -18 / -20 | -30 / -24 | **-34 / -32** |
| neck | +9 | -2 | +4 |
The blue cast is gone: B deltas went from +20..+49 to -1..-12 below the eyes.
Smile line: continuous across cols 240-272 in all 4 frames (17/17 sampled cols), rows 306-321 along
the curve, depth 51-67 lum, colour ~(190,150,136). Original: depth ~38 lum (229 -> 190), colour
(226,183,168). So the rebuild line is now somewhat darker/heavier than the original.
Chin edge at col 257: BC (0,0,0) -> DE (153,87,73); original (179,104,89). Near-black px in the face
box 102 -> 43.

### Remaining after DE
1. Eye band too dark (-33): a tan rectangle across the eyes. Likely the eye card's litBase/shade ramp
   under the new light (the eye shader composites its own skin under the eye art), plus the
   distorted iris art (pupil grid, report 4.4).
2. Smile line ~1.5x too strong: candidates are the mouth `_RShadowColor` interior multiply and the
   implicit-mip LOD bias; tune after (1).
3. Character2/4 mouth propagation; C003/C005/C006 mesh check; docs/KB corrections (unchanged).
4. Ops note: the 8094 static front had died (machine sleep/restart); restarted with
   `node C:\mfgtmp\serve_aog.js` per KB front-server-plumbing.

---
## Rounds F-H (2026-10-30 17:06): eye band, eye overlay, scrambled iris fixed

### Face-clip correction (affects the handoff premise)
The handoff's "eye 12 / mouth 10, shy face" comes from the generic Face_Smile clip, used only by
Character20. C1's controller (CharaMotion01_Story) plays its own clip: eyes 0/1/3, mouth 7, base 0,
pupil offsets ~0 plus look-at <=0.03.

### Round F: eye band (tan rectangle) - Eye.shader `_EyeOverlayMode` (default 0 = legacy)
Mode 1 is the original composite (eye_asm08 299-336): layer = lerp(art, pupil, mask.g), alpha =
max(mask.g, mask.r), brow lerp by brow mask, face-shadow tint by shade, final = lerp(litBody, layer, alpha),
rim suppressed under the overlay. Pupil grid: raw TEXCOORD3 + (0.5c, 0.25(i-c)) + raw offsets.
Mt_eye_L/R: `_EyeOverlayMode: 1`. Tan band gone (between_eyes -33 -> -6, eye_R -32 -> -18).

### Round H: the "jumbled inside of the eye" = broken eye MESH, not the shader
- A debug render (pupil uv as colour, since reverted) showed the on-screen pupil uv FOLDS inside the
  iris (non-monotonic u and v), while an offline render from the original glb is clean.
- Cause: the Unity asset model_face_eye_L/R.asset (written 10-28 by the conversion pipeline) has uv1/uv2/uv3
  attached to the wrong vertices: 107 of 321 triangles have no exact twin in the glb (6 differ even in
  topology). Positions and uv0 are fine, which is why the eye outline/whites looked right and only
  the pupil-mapped interior was scrambled. (My earlier "mesh data identical" check compared ranges,
  not per-triangle attributes - it was wrong.)
- Fix: scripts/eye_mesh_rewrite.py rewrites the vertex streams and index buffer of both eye meshes
  from the glb (same 271 verts; x mirrored, v flipped, tangent.w flipped, winding flipped; bone
  stream kept - all verts on bone 5). Re-check: 0 of 321 triangles mismatched, both sides.
- The same check (scripts/mesh_tricheck.py) on C1 mouth / face / hair / body: 0 mismatches (they are
  only reordered). Scan of all 68 exported left-eye meshes: also broken are **Character2/C001 (17),
  Character5/C001 (160), Character8/C001 (28)** - not fixed (right eyes not scanned).

### Results (frames_H, 4 frames, all built clean: errShader=0, result=Succeeded, data 17:03)
| metric (original vs rebuild, iris core 18x18 around pupil, best shift +-5 px) | F | H |
|---|---|---|
| eye_L lum correlation | 0.72 | **0.92-0.94** |
| eye_R lum correlation | 0.22 | **0.93** |
Side-by-side: eyecmp_F.png (scrambled) vs eyecmp_H.png (intact iris, highlight, slit pupil, amber gradient).
Best alignment needs the rebuild ~1-3 px left / 2-3 px down: the pupil anchor (darkest-blob centroid)
moved up ~3 px now that the pupil has its real shape. That moves the eye boxes in the region table:
| region | F | H |
|---|---|---|
| eye_L / eye_R | 0 / -18 | -17 / -21 |
| between_eyes | -6 | -23 |
| cheek_L / cheek_R | +17 / -5 | +13 / 0 |
| mouth_line / chin / nose | -2 / -2 / +1 | +3 / -1 / +2 |
Independent checks improved (neck_top err 2.0 -> 3.2 px, crimson_top 10.2 -> 3.3 px), so the eye-box
swings are mostly the anchor shift, not darker rendering. **Assumed, not proven.**

### Files (backups in backups/)
| file | state |
|---|---|
| MFG/Characters/Character1/3D/C001/Fbxs/model_face_eye_L.asset | rewritten from glb (sha 61a18addf384...) |
| .../model_face_eye_R.asset | rewritten from glb (sha 947db17995ea...) |
| Shaders/Character/Eye.shader | = backups/Eye.shader.roundF (debug removed; sha 3659a367c40d...) |
| Common/Mt_eye_L.mat, Mt_eye_R.mat | = .roundF (`_UtsLighting: 1`, `_EyeOverlayMode: 1`; no debug) |

### Remaining
1. Smile line ~1.5x too strong; cheek_L +13.
2. (done, see below) Eye meshes of Character2/5/8 C001.
3. Registration: make mfg_reg3's anchor robust to iris art (e.g. eye-white centroid) so eye boxes stop moving.
4. Character2/4 mouth propagation; C003/C005/C006 mouth check; docs/KB corrections (mfg_reg2 retired,
   FINAL/registration_report claims, handoff face-clip premise). Nothing committed.

### Follow-up: Character2/5/8 C001 eye meshes (same defect)
Right eyes scanned too: the broken set is exactly C2/C001 (17 tris), C5/C001 (160, 12 also differ in
topology), C8/C001 (28), both sides. All 60+ other costume eye meshes are clean.
Rewritten with scripts/eye_mesh_rewrite.py (now takes `CharacterN/3D/C00X` args): vertex counts match
the glb (227 / 247 / 238), there are no blend shapes, and position sets agree (C2 exact; C5/C8 within 0.27 mm).
Re-check with mesh_tricheck.py: 0 mismatched triangles for C1, C2, C5, C8, L and R.
Backups: backups/C{2,5,8}_C001_model_face_eye_{L,R}.asset.orig.
**Not rendered:** the capture harness only shows C1 on character select, so C2/C5/C8 are verified by
mesh data only. Their materials do not use `_EyeOverlayMode`, so they still use the legacy eye path.

---
## Round I (2026-10-30 17:22): smile line strength
Cause: the rebuild darkened the mouth art twice beyond the original. It multiplied by `_RShadowColor`
(0.5,0.4,0.35) through the mask, then lit it with the skin shade ramp. The original (mouth asm08 305-312)
paints the RAW art over the lit skin by mask.r*art.a. Only `_FaceShadowColor` is applied, by the
1st-shade weight.
Change: Mouth.shader `_MouthOverlayMode` (default 0 = legacy):
`lighting = lerp(litBase, lerp(art, art*_FaceShadowColor, 1-firstStep), mouthWeight)`.
Mt_mouth.mat `_MouthOverlayMode: 1`. Backups: Mouth.shader.roundH, Mt_mouth.mat.roundH.

| smile line (registered into original frame, scripts/line_compare.py, cols 330-370) | original | H | I |
|---|---|---|---|
| depth below local skin, median (lum) | 29 | 46-56 | **31-35** |
| darkest-pixel mean rgb | (233,192,177) | (209,174,160) | (224,186,172) |
Region table I vs H: all within +-3 (mouth_line +3 -> +5, neck +11 -> +8, cheek_L +13 -> +14).
Remaining on the line: max depth 69-79 vs 50. A darker ~6 px dab at the bottom centre with a soft
shade under it (linecmp_I.png) is still there; the line is also ~1 px wider/wavier than the original. Not diagnosed.

---
## Round L (2026-10-30 17:5x): white cheek streaks + dark sliver at the right eye (owner-flagged)
Isolation:
- Debug tag round (Jdbg): eye-card skin = magenta, mouth-card skin = cyan. Every streak sits in eye-card skin.
- Rim-off round (Kdbg): streaks unchanged, so rim is not the cause.
Cause: two legacy Eye.shader stages still ran AFTER the overlay composite:
(a) the "unlit eye window" re-lerp of the legacy art by openWin;
(b) the "skin-frame rim blend", which pastes colours from `_EyeFrameRefTex`, a texture baked from an earlier
screen render.
The original has neither (eye_asm08 336 ends at the overlay). Fix: both are skipped when `_EyeOverlayMode > 0.5`
(legacy path untouched). Debug edits reverted; no TEMP lines left. Backups: Eye.shader.roundI (before), .roundL (after).
Results (4 frames each, I -> L):
- near-white px in the cheek band (rows 283-303, cols 205-295): ~270 -> 102. The rest is the white hair strand
  plus the small highlight dots, which now look like the original's.
- right-eye corner sliver: gone visually (fix_L.png). The dark-px count in its box went from ~76 to ~65;
  the box also holds lash and hair ink.
- region table: cheek_L +14 -> **+2**, cheek_R -1 -> -5, eye_L/R -18/-23 -> -13/-16,
  between_eyes -20 -> -31 (the frame blend had been brightening skin under the bangs), others within +-5.
