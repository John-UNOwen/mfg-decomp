# Handoff: carry the Character1 face fixes to the other characters

Status 2026-10-31. Written after the Character1 (C1) character-select face match. The full round log
is `C:\mfgtmp\claude\REPORT.md`; `docs/26-claude-face-match-rounds.md` is an early copy of it. Read
this file first.

## TL;DR

- **Most of the C1 procedure carries over.** All 21 characters use the same `Mouth.shader` and `Eye.shader`, so
  the C1 fixes are material switches plus, in two cases, mesh work. Nothing else needs a shader change.
- **The broken-mesh part is already done everywhere.** A triangle check of every face-part mesh (306 eye/mouth meshes, Characters 1-20) against
  the exported glb shows 0 broken eye meshes. The only mismatches are the 15 Character2/Character4 mouth meshes,
  which were remapped on purpose (see step 3). Character21, and some meshes of Characters 17-20, sit
  deeper than the scan reached; check them (step 1).
- **The rest is per-character switches.** Only C1 has the new settings on. Every other character still runs the
  legacy paths: `_MouthUVMode`, `_MouthOverlayMode`, `_EyeOverlayMode` and `_UtsLighting` are absent, so they default to 0.
- **Not portable / not solved:** the grey teeth and the moving grey oval inside the *open* mouth (see
  "Open problem"). Two stencil attempts broke the mouth and were reverted; don't repeat them as written.

## What was wrong on C1, and the fix for each

| # | Symptom | Cause | Fix (C1 state now) |
|---|---|---|---|
| 1 | Smile line missing / wrong place | Mouth atlas formula wrong in the rebuild shader | `Mt_mouth.mat` `_MouthUVMode: 1` (original additive atlas mapping, asm08:289-304) |
| 2 | Blue cast, pale line, white wash over mouth/eye cards | Legacy lavender `ambient*2` light + a rim that ignored the inside mask | `Mt_mouth.mat`, `Mt_eye_L.mat`, `Mt_eye_R.mat` `_UtsLighting: 1` (UTS2 light + rim) |
| 3 | Tan band across the eyes | Legacy eye composite | `Mt_eye_L/R.mat` `_EyeOverlayMode: 1` (original eye overlay, eye_asm08 299-336) |
| 4 | Jumbled iris interior | Eye **mesh** had uv1-3 attached to the wrong vertices (conversion bug) | Rewrote eye meshes from glb with `eye_mesh_rewrite.py` (C1, C2, C5, C8 - C001 only were broken) |
| 5 | Smile line ~1.5x too dark | Art darkened twice (`_RShadowColor` multiply + skin ramp) | `Mt_mouth.mat` `_MouthOverlayMode: 1` (raw art alpha-overs the lit skin, asm08 305-312) |
| 6 | White cheek streaks, dark sliver at eye | Two legacy eye blends (openWin re-lerp, baked frame rim) | Automatic once `_EyeOverlayMode: 1` (Eye.shader skips both in overlay mode) |
| 7 | Black mouth/eye outline copies | Outline copies sampled `mdl_face.png` with atlas uv0 | `Mt_outline_facepart.mat` (`_OutlineUV: 1`) on the mouth/eye outline copies |
| 8 | Grey-pink showing through the open mouth (partial) | Mouth outline copy wins depth when the mouth bends | Mouth outline renderer `m_Enabled: 0` in C1 `C001/Prefabs/CharaModel.prefab` |
| 9 | Hair strand outlines too heavy (owner's taste) | `_width 1.5` | `Common/Mt_outline_hair.mat` `_width: 0.5` |

## Character inventory (data checked 2026-10-31)

All use Mouth.shader (`b8b7e4c7...`) and Eye.shader (`26b1ec72...`). The face shader varies.

| Char | Face shader | Hair outline width | Notes |
|---|---|---|---|
| 1 | Toon TransClipping_StencilMask (`eeb96954`) | 0.5 | Done (reference) |
| 2 | StencilMask | (no hair outline mat in Common) | Mouth meshes remapped -> keep `_MouthUVMode 0` (step 3); C001 eye mesh fixed |
| 3 | StencilMask | - | |
| 4 | StencilMask | - | Mouth meshes remapped -> keep `_MouthUVMode 0` (step 3) |
| 5 | StencilMask | 1.5 | C001 eye mesh fixed |
| 6-9 | StencilMask | 1.5 | C8 C001 eye mesh fixed |
| 10-11 | StencilMask | 2 | |
| 12 | StencilMask | 1.5 | Files named `face_eye_L/R`, `face_mouth` (not `model_face_*`) |
| 13-15, 20-21 | TransClipping_StencilOut (`3523ffe3`) | varies | Different face shader: re-check anything depth/stencil-related |
| 16 | Face uses the Mouth shader (`b8b7e4c7`) | - | Odd; inspect before touching |
| 17-19 | unknown (`663a352b`) | varies | Inspect |

- **Disabled renderers in the C001 prefabs:** C2 23/46, C5 21/42, C8 16/32 skinned renderers are off; all the others have 0 off.
- Find out what those disabled renderers are (probably outline copies) before enabling or disabling anything (step 5).
- Textures for C14, C16 and C18 are not in `3D/Common` (mouth texture absent there); find their material bindings first.

## Procedure per character (CharacterN)

Rules that still apply:
- Snapshot every file to `C:\mfgtmp\claude\backups` (or your own backup dir) before editing.
- Never scan `build\unity-project` recursively; address files by exact path. Depth-limited walks under one
  `CharacterN/3D` are fine.
- Don't touch the installed game.
- Edit text assets with a tool that keeps their line endings. `sed -i` on Git Bash converted a CRLF prefab to
  LF once. Use Python binary read/replace, and assert the match count.
- `unity-project` is git-ignored. Write findings into this doc / REPORT.md, not commits.

### 1. Mesh check (data only, ~2 min for everything)
```bash
python C:/mfgtmp/claude/scripts/all_facecheck2.py > C:/mfgtmp/claude/all_facecheck2.txt
```
- What it does: for every `*eye_L|eye_R|mouth.asset` up to 3 levels under `CharacterN/3D`, it runs
  `mesh_tricheck.py ASSET GLB` against the same path in `build/export-primary-content`.
- A healthy line: `tris w/o exact glb twin 0; w/o position twin 0`.
- To reach Character21 and the deeper meshes of 17-20, raise the depth limit (line 10, `>2`).
- Last run: 306 meshes, 291 exact. The 15 failures are all Character2/4 mouths (expected, step 3).

If an eye mesh fails: `python C:/mfgtmp/claude/scripts/eye_mesh_rewrite.py CharacterN/3D/C00X` (dry run), then
add `--write`. It rewrites the vertex streams and index buffer from the glb (x mirror, v flip, tangent.w flip,
winding flip, 16-byte stream alignment) and keeps the bone stream. Re-run mesh_tricheck: it must show 0.
The script assumes `model_face_eye_*` names under `Fbxs/`; adapt the paths for the Character12-style names.

### 2. Eye materials (`CharacterN/3D/Common/Mt_eye_L.mat`, `Mt_eye_R.mat`)
Add under `m_Floats` (match the file's existing indentation and list style; files differ):
```
_UtsLighting: 1
_EyeOverlayMode: 1
```
This fixes the tan band, the cheek streaks and the eye sliver (C1 rows 2, 3, 6). Precondition: the eye mask follows
the C1 convention (r = eye white/frame, g = pupil area); open `mdl_eye_msk.png` and compare it with C1's.

### 3. Mouth material (`CharacterN/3D/Common/Mt_mouth.mat`)
```
_UtsLighting: 1
_MouthOverlayMode: 1
_MouthUVMode: 1      # ONLY if the mouth mesh passed step 1 exactly
```
- `_MouthUVMode 1` expects the original band-anchored uv0 (cells on a 4x4 atlas, `mdl_mouth.png` 1024²).
- Character2/4 mouth meshes were remapped by `scripts/remap_mesh_uv.ps1` (v 0.263-0.672 / 0.384-0.847). Leave them at
  mode 0, or first restore their uv0 from the glb (same idea as the eye rewrite) and then set mode 1.
- Check that the material's `_MouthIndex` / animation drives the expected cell. C1's own clip uses mouth 7, eyes 0/1/3. The
  generic Face_Smile clip ("eye 12 / mouth 10") is only used by Character20.

### 4. Outline copies of the face parts (prefab)
- In `CharacterN/3D/C00X/Prefabs/CharaModel.prefab`, find the duplicate `model_face_mouth` / `eye_L` / `eye_R` objects
  that use an outline material (`sg_cha_outline`, guid `c3d34d8d...`).
- If enabled, they must sample with uv1: point them at a copy of `Mt_outline_face` with `_OutlineUV: 1`. C1 made
  `Common/Mt_outline_facepart.mat` (guid `ab3ad06c...`); make one per character or share it.
- C1 also disabled the **mouth** outline copy (`m_Enabled: 1 -> 0` on its SkinnedMeshRenderer), because its depth shell
  showed through the open mouth.

### 5. Hair outline width (optional, owner preference)
`CharacterN/3D/Common/Mt_outline_hair.mat` `_width` -> 0.5. Owner reference: a thin dark line on the hair edge and
between strands, not none.

### 6. Build and hand over
```powershell
& 'C:\mfgtmp\rebuild_blush.ps1'
& 'C:\mfg\scripts\assemble_webroot.ps1'
```
Check `C:\mfgtmp\build_blush_status.txt` for `errCS=0 errShader=0`, and the last `webgl build result=Succeeded` in
`C:\mfgtmp\side.log.txt`. (`succ=False` in the status line and "BUILD NOT CLEAN" in round_full are false alarms
when the result line says Succeeded.)

**Verification is visual and by the owner.** The capture harness (`round_full.ps1`) only reaches C1 on character
select, so other characters can only be verified in the game build by eye. The owner asked that agents don't run
captures themselves: build, assemble, then ask them to look.

## Open problem: grey teeth + moving grey oval inside the open mouth

Known:
- Not the art: `mdl_mouth.png` teeth are ~(236,229,228) and the texture, mask and mouth mesh normals equal the export exactly.
- Not the mouth shader's shade term: with C1 settings `firstStep` is always 1 (`_1st_ShadeColor_Step 0`).
- Not a separate inner-mouth asset: the game exports none (only `model_face_mouth` + `mdl_mouth*.png`).
- The size changes with the animation, so it is depth-related. The mouth (queue Geometry+450, ZWrite Off, ZTest LEqual,
  clip bias `o.pos.z -= 0.003*o.pos.w`) loses the depth test against the face (2449, ZWrite On) where the
  bones push the mouth card behind the face surface.

Tried and reverted (backups `*.roundN`, `*.roundN2`, `*.roundO`):
- **N:** mouth drawn before the face (queue 2448), and the face skips mouth pixels via a stencil bit. Result: the whole mouth
  card turned **black** with only the art visible. So the card's own `litBase` (`_MainTex` at uv1 × shadeRamp) does not
  give lit skin in practice, though on paper it should. Find out why before relying on it.
- **O:** mouth after the face, art-only with alpha blending, ZTest Always, gated by a face-only stencil bit (`Mt_face`
  `_StencilNo 17`). Result: the card area became **transparent/black**.
  - Likely cause: the face pixels did not carry the bit as assumed. Possible reasons: the face's Fail/ZFail stencil ops, draw
    order inside queue 2449, or hair re-writing.
  - Unverified. Debug it with a single-colour stencil test before building on it.

Ideas not yet tried:
- A larger or adaptive depth bias only for the mouth (cheap; risks the mouth drawing through hair or hands).
- Measure how far the mouth card sinks behind the face per animation frame, from the bone animation, to size the bias.
- Pull the original game's mouth/face render state (ZTest/Stencil/queue) from the serialized shader. The DXBC in
  `C:\mfgtmp\mouth_dxbc` has no state, but the shader blob header might.

### Resolved 2026-11-05: the bias is deleted, the sink was never the problem

`build/unity-project/Assets/Editor/MouthDepthProbe.cs` (build commit 6b944aef8d) instantiates a CharaModel in a throwaway
scene, bakes every skinned mesh at sampled animation times and compares view depths at the MahjongPlay panel-camera settings
(near 0.3, far 15, FOV 10, 2 m, camera on the horizontal face axis taken from the eye cards). Only faces each pass actually
rasterizes are counted: `sg_cha_outline` is `Cull Front`, the rest `Cull Back`. Report `C:\mfgtmp\mouth_depth_pass4.txt`,
354 frame rows over Character1 / 9 / 10 C001 (bind, Shy, Think, Reach, Surprise, Result, Hand_Yose).

- The face shell blocks 0 of the card's ~440 visible triangles at bind pose and never more than 17 in a frame, always under
  1 cm. The card's front is already ~1.6 cm outside the shell's frontmost point: the shell has an aperture and the card fills it.
- A hand over the face sits 11-15 cm nearer the camera than the card and covers 34-161 of its triangles.
- Hair crossing the mouth band occludes it too (Character9 Shy: 190-360 triangles), which is the correct arcade behaviour.
- So `o.pos.z -= 0.003 * o.pos.w` (3.9 cm at 2 m) was about 40x the sink it was hiding, and it is what painted hands and hair.
  Deleted in build commit 7785e975d8; `ZTest LEqual` and the queue 450 placement are unchanged.
- `ZWrite` stays Off deliberately. The shipped Mouth pass writes depth, but our restored Eye forward pass tests LEqual where the
  shipped eye composite pass tests Always (`export-primary-content/.../Eye.json`, pass3 `m_ZTest 0`), and the bands overlap
  (mouth card y 1.384-1.466, eye card y 1.444-1.573): copying the write without the eye's test lets the card clip the eye card.
  Hands need the test, not the write; they write depth at 2449.
- The grey-oval defect of row 1 in docs/25 was measured while the vestigial `model_face_front` hull sheet still closed the
  aperture; that GameObject is now `m_IsActive: 0` in every prefab that has it, so the card rides in the hole as authored.
- Original mechanism, for the record: aperture + honest depth. `build/_tmp_audit_moutheye/mouth_asm02_ann.txt:54-62` builds the
  clip position as `unity_MatrixVP * (unity_ObjectToWorld * pos)` with no added term and never reads `_Offset_Z`; queue 2450 with
  `ZWrite 1` / `ZTest LEqual`. Nothing at runtime moves the card: `FaceController`/`MouthController` push only material floats
  (`_MouthIndex`, `_MouthOffsetX/Y`, `_GBlend`), the `Face_*.anim` clips animate the controller's own fields (`classID: 114`) with
  the offsets left at 0, and no MahjongPlay scene override references the mouth node.
- Open follow-up, untouched so far: the mouth's own silhouette copy (`Mt_outline_facepart` on a second `model_face_mouth`
  renderer sharing the mesh) is switched off in 14 C001 prefabs - Character1, 6, 7, 9, 10, 11, 12, 13, 14, 15, 16, 18, 20, 21 -
  a change our face round made between `1a37ae344b` and HEAD; the rip ships every renderer enabled in all 197 CharaModel prefabs.
  Probe `tmp/prefab_renderer_diff.py` (main repo scratch) reproduces the list.

## Tools (all in `C:\mfgtmp\claude\scripts\`)

| Script | Use |
|---|---|
| `mesh_tricheck.py ASSET GLB` | Per-triangle attribute check (ranges/histograms miss permutations) |
| `all_facecheck2.py` | Runs the check over every character |
| `eye_mesh_rewrite.py CharacterN/3D/C00X [--write]` | Rewrites eye mesh streams from the glb |
| `mouth_normcheck.py ASSET GLB` | Normal/position comparison |
| `asset_mesh2.py`, `glb_dump.py` | Unity asset / glb decoders used by the above |
| `eye_compare.py`, `line_compare.py`, `mfg_reg3.py` | C1 capture metrics (pupil-anchored registration; mfg_reg2 is retired) |

## Where C1's changed files live (for diffing)
- Shaders: `Assets/Shaders/Character/Mouth.shader`, `Eye.shader`, `Assets/Shaders/sg_cha_outline.shader`.
- C1 materials: `Assets/MFG/Characters/Character1/3D/Common/Mt_mouth.mat`, `Mt_eye_L.mat`, `Mt_eye_R.mat`,
  `Mt_outline_facepart.mat`, `Mt_outline_hair.mat`.
- C1 prefab: `Assets/MFG/Characters/Character1/3D/C001/Prefabs/CharaModel.prefab`.
- Eye meshes: `Character{1,2,5,8}/3D/C001/Fbxs/model_face_eye_{L,R}.asset`.
- Backups: `C:\mfgtmp\claude\backups\` (`.orig` = before any change; `.roundX` = before round X).
