# 24 - Face reconstruction asset inventory + handoff (2026-10-29)

Fresh-perspective brief for an agent picking up the Character1 (Yawaragi Hiyori) face/mouth match work.
Everything below was scripted and verified today; nothing here is assumption unless marked OPEN.
Paths: rebuild project `C:\mfg\build\unity-project`, original rip `~\Documents\GitHub\mfg-decomp` (REPO;
its `build\export-primary-content` = original-side dumps), install `\\MYAPPSANDBOX\c\Games\MFG` (read-only,
28,292 plain UnityFS + `game\MFGClient_Data\StreamingAssets\aa\catalog.json`).

## 0. Bottom line

Every asset needed to rebuild the face is present and slot-verified (meshes, textures, materials, code,
anims, scenes, original compiled shaders + property tables + disassembly recipe). Nothing blocks work.
The two re-decompile items are done (2026-10-30, §6): CharaSelectNum1.mat was only a name collision -
its content was already exported - and the Mouth/Eye DXBC is fully disassembled, recovering the authored
uv->cell transforms. The mouth rebuild mapping (`uv*0.25 + AtlasCellOrigin`, fitted -0.13) is a rescale
approximation, not the authored transform (authored = raw band-anchored uv + 0.25 index steps); the eye
rebuild formula already matches the original except the pupil-grid v-step. Three OPEN mechanism questions
are listed in §3/§7 - they are analysis, not missing data.

## 1. Extraction completeness (verified facts)

- docs/20: AssetRipper exported 29,404 of 29,409 catalog assets (99.98%). The 5: four built-in shaders
  (not face-related) + `CharaSelectNum1.mat` (select-screen number digit UI material; closed 2026-10-30,
  see §6 - content was already exported under a collision name).
- GUID integrity: 364,066/377,324 refs resolve; all others are built-in assets. No face-domain gaps.
- Today's audit (scripts `C:\mfgtmp\face_audit*.py`, reports `C:\mfgtmp\face_audit_report*.txt`): walked
  CharaModel.prefab for Character1/2/5/8 - every face/mouth/eye/brow/hair/body renderer resolves mesh +
  material + every texture slot to a real file. Zero missing.

## 2. Asset map (face domain, Character1 unless noted)

### 2.1 Meshes
| what | path | size |
|---|---|---|
| C001 face shell (original, per-costume) | REPO export `...\Character1\3D\C001\Fbxs\model_face.glb` | 454v/686t |
| C001 mouth mesh | same dir `model_face_mouth.glb` | 296v/486t |
| C001 eyes | same dir `model_face_eye_L/R.glb` | 271v/321t each |
| shared face shell | REPO export `Assets\Mesh\model_face.glb` | 489v/728t |
| full-head hulls (face-aperture skin surface candidates) | REPO export `Assets\Mesh\model_face_outline{,_0.._5}.glb`; outline_3 = Character19 hull, 991v/1756t | - |
| costume body/hair | `...\C001\Fbxs\body.glb, hair.glb, Hiyori_01.glb` | - |
| in-project .asset versions | `Assets\MFG\Characters\Character1\3D\...` + `Assets\Mesh\*` | - |
- The prefab's `model_face_front` renderer (Mt_face) is the rebuild's bone-parented skin sheet (from
  outline_3); it does NOT exist in the original export. The original mechanism for the face-aperture skin
  surface is OPEN (§7 task 1). Band-shell property (no face-aperture triangles) was verified pre-handoff
  on the shared mesh; re-verify on the C001 per-costume mesh too.

### 2.2 Textures (slot-verified against materials)
Folder `Assets\MFG\Characters\Character1\3D\Common\` (all present):
- `mdl_face.png` 213KB - skin + face-layout atlas (Mt_face/Mt_mouth/_MainTex aux, Mt_eye_* aux,
  Mt_outline_face._Texture)
- `mdl_face_pale.png` 774KB, `mdl_face_shy.png` 189KB - alternate face states (baseTextures swap)
- `mdl_mouth.png` + `mdl_mouth_msk.png` (1024x1024) - mouth art + mask (mask red = soft line profile,
  264 red texels rows 171-175 cols 91-170; cell bg flat 230,172,136)
- `mdl_eye.png` + `mdl_eye_msk.png`, `mdl_eyebrow.png` + `mdl_eyebrow_msk.png`, `mdl_pupil_L/R.png`
- `mdl_hair.png`, `mdl_hair_light.png`, wrist/necktie/hoodie variants, `msk_black/msk_green/msk_white`
- Select UI: `Assets\MFG\Characters\Character1\CharaSelect\Textures\BtCharaSelect01.png` etc (146 paths)

### 2.3 Materials (all resolve; serialized values are shipped ground truth)
`Mt_face.mat` (UTS2 guid eeb96954..., queue 2449), `Mt_mouth.mat`, `Mt_eye_L/R.mat`, `Mt_outline_face.mat`,
hair/ribbon/body-family mats in C001 (chain-resolved). Note mouth/eye mats serialize `_1st_ShadeColor_Step 0`
while face uses 0.55.

### 2.4 Shaders
- Face: stock UnityChanToonShader v2.0.9 `Toon_DoubleShadeWithFeather_TransClipping_StencilMask`, proven
  instruction-for-instruction vs shipped DXBC (`REPO\build\_tmp_audit_face\FINAL_REPORT.md`, 52 programs
  disassembled asm02..83 + *_ann/_ann_c annotated + cb_layout.txt; scripts `REPO\build\_tmp_audit\auditA..H3.py`).
  Shader source restored in project: `Assets\Toon\Shader\Toon_DoubleShadeWithFeather_TransClipping_StencilMask.shader`.
  FINAL_REPORT key facts: face props _GBlend/_Chroma/... are dead data; pale/warm question = scene light,
  original effective lightColor ~= (1, 0.95, 0.75) warm; lightmap path does not exist in the compiled variants.
- Eye/Mouth (`Bsac/Character/Eye|Mouth`, 202/193 props): source lost, hand-reconstructed in
  `Assets\Shaders\Character\Eye.shader` / `Mouth.shader`. Original compiled data present:
  REPO export `Assets\Shaders\Character\Mouth.json` (blob 23,080 b64 -> LZ4 block -> 191,280 B) and
  `Eye.json` (27,412 -> 205,752 B) with full property tables; FINAL_REPORT §1 documents the container
  format + the audit scripts are saved. Regenerating mouth/eye disassembly = optional re-decompile
  (D3DDisassemble/dxc on the SHDR sections; name uniforms from json m_NameIndices like auditF.py did).
  Rebuild shader current state: see §5. Pristine backup: `C:\mfgtmp\Mouth_shader_backup_pre_debug.shader`.

### 2.5 Code, anim, scene
- Controllers (decompiled, in project `Assets\Assembly-CSharp\MFG.MahjongPlay.Character.Face\`):
  FaceController, MouthController, EyeController; Standing3DManager attaches controllers includeInactive.
  MouthController pushes serialized indices every frame (select = 0/0 in both games; verified).
- `Character1\Property\Properties.asset` -> class CharacterProperties (Assets\Assembly-CSharp\MFG.Assets).
- 212 `.anim` files for Character1 incl full `3D\Animations\Motion\Face\Face_*.anim` set
  (Anger/Normal/etc Start/Loop/End). Curve attributes are serialized-field name hashes (map via Unity
  hash128 of the field path); originals reference driven fields like _leftEyeIndex (curves exist, verified
  in earlier spans across 4027 anims).
- Scenes in BOTH trees (rebuild + REPO export, identical): `Assets\MFG\Scenes\FirstCharaSelect\
  FirstCharaSelect.unity` (301KB, the parked capture scene) and `CharaSelect\CharaSelect.unity` (51MB).

## 3. Scene ground truth (parsed from the scene files today)

- RenderSettings (both scenes): ambientMode 3 (flat), ambient sky = (0.414, 0.388, 0.458) - this matches
  the ambient the tk rig harvested from the original game, so it IS the original select value.
- FirstCharaSelect: 1 Directional Light, color (1, 0.957, 0.839), intensity 1 - serialized DISABLED
  (m_Enabled: 0). 1 Main Camera. 13 Animator components; UI controllers resolve to
  AnimatorController/{CharaBt2, CharaMove, BtCharaSelect, CharaSelectAll1Ani, CharaVoiceFrame, ...}.controller.
  Scene GO names include: Character, StandingCharaManager, SceneFlow, Parent3D, CharaMove, SelectPanel.
- CharaSelect: 3 cameras (Main Camera, Camera, CutinHai_Camera), Directional Light ENABLED (same warm
  color) + 2 white Spot Lights (intensity 1.81 range 25; intensity 2.66 range 5).
- OPEN #1 (lighting) - RESOLVED 2026-10-30, see §3b: the original select render shows warm-lit skin (FINAL_REPORT inferred
  lightColor~=(1,0.95,0.75) = the directional color). Determine what enables the warm Directional Light
  for the screen being captured (code: check SceneFlow/StandingCharaManager in decompiled sources; or the
  compared screen is actually CharaSelect, not FirstCharaSelect; or an animator/Playable enables it). Then
  set the rebuild's select lighting to the identical configuration - this is the root-cause lead for the
  whole residual family (pale skin, eye/mouth ambient*2 hack, iris +20, body +32).
- OPEN #2 (camera): original ref capture is 1778x1026, rebuild capture 1280x720; measured iris IOD 101.9
  vs 67.6 = 73.3 after width conversion => ~8% face-size difference. Find the real camera params (Main
  Camera transform/FOV live in the scene's !u!4/!u!20 blocks - parse or open in editor) and reproduce the
  original framing/scale in the capture lane instead of rescaling in post.
- OPEN #3 (skin surface) - RESOLVED 2026-10-30, see §3b: how does the ORIGINAL render the face-aperture skin (band shell has no face
  triangles): inspect the FirstCharaSelect 'Character' prefab-instance overrides (m_Modifications - the
  scene may add the original face surface), Standing3DManager/CharacterProperties select-time renderer or
  material assignments, and the C001 Fbxs model_face geometry (uv coverage of mdl_face.png). Do not assume
  the rebuild's model_face_front sheet is how the original does it.

## 3b. Fresh-agent investigation results (2026-10-30, `C:\mfgtmp\fresh\FINAL.md`)

The follow-up investigation resolved the open items above:
- OPEN #1 resolved: no runtime mechanism enables the warm light (exhaustive decompiled search; shipped
  bundle matches the export). The original's warm-bright cheek/mouth comes from the SHY face map itself
  (BaseTextureIndex 2, texels ~(246,216,205)) under white light; keep select lighting as shipped.
- OPEN #3 resolved: the band shell IS the original face surface (492/728 tris on artwork; 56 aperture
  tris are edge geometry, eyes behind); the shipped prefab has NO model_face_front (311st-GO rebuild
  artifact - delete or ignore).
- NEW shipped fact: the original prefab has 12 SMRs ALL enabled, including 6 `group outlien` outline
  copies; the rebuild serialized those 6 disabled, so the original draws silhouette rims (~3 px, strip
  texels ~(132,84,78)) and the rebuild draws none. Re-enable to match.
- Mouth root causes measured: runtime state is `_mouthIndex` 12 + shy map (Face_Smile.anim, not 0/0);
  the live card samples cell-12 background rows 783-825 while ink sits at 931-962 (weight 0; sampling
  v-shift ~-0.13 recovers the authored mapping); plus a +31 px rebuild-live-only card projection offset
  (rows 293-336 vs serialized 242-305; cause open, suspect mouth-mesh neck weights; do not fix via
  `_MouthOffsetY`, the anim zeroes it).
- Registration: use `C:\mfgtmp\fresh\mfg_reg2.py` (two-iris similarity, scale ~1.54, hairline verify
  0.5-0.6 px). All new artifacts + logs live under `C:\mfgtmp\fresh\`.

Next actions (in order): 1) resolve the +31 px offset (runtime bone/bounds dump; check neck weights);
2) apply the sampling shift (or per-mesh band remap consistent with the eye convention) + re-enable the
6 outline SMRs, one verification capture - falsifiable: line at rebuild rows 299-302 +-4 (cols 210-280),
ribbon from 306, rims present; 3) optional: re-disassemble the shipped Mouth DXBC (FINAL_REPORT §1 recipe)
to replace the fitted -0.13 with the authored transform.

## 4. Measurement state (read before trusting ANY row/box number)

- Registration bug (owner-flagged, verified): the cmp harness registered rebuild->original by eye-center
  TRANSLATION ONLY across different frame sizes; its rebuild eye anchor sat ~12px below true iris centers.
  All 2026-10-29 below-eyes rows/claims are invalid: 'band rows 412-448', 'line at 460-499', and original
  target colors sampled there (the quoted (133,60,45) may be original costume-red pixels - re-derive).
- True landmarks (native, same capture set): rebuild 1280x720 irises y~227, x~212.6/280.4 (IOD 67.6);
  original ref 1778x1026 irises y~371, x~291/393 (IOD 101.9). Rebuild rows 332-380 = saturated bow red;
  true mouth zone ~296-320 = skin with mild warm tint, no line visible. So even after the depth fix, the
  current build shows no crisp line AT THE MOUTH - owner-confirmed observation, correct.
- What survives: depth/occlusion proof (nothing rendered at queue 2000; artwork appeared with ZTest
  Always) - that chain never used the cmp registration. Mouth mesh may project too low on the face (near
  chin/bow): re-verify footprint vs true landmarks FIRST (task 2).
- Vision desk is unreliable on fine detail (called the dark-red arc a 'tan neck', then 'bows'); trust
  numeric pixel profiles and owner eyes. Owner ground truths standing: 'it should be 1' (RealVNC layering),
  'both eyes garbled in inner half' (iris).

## 5. Mouth shader state (current tree)

`Assets\Shaders\Character\Mouth.shader`: Tags Queue Geometry+450 (face 2449 < mouth 2450 < eyes 2451),
ZWrite Off + ZTest LEqual in FORWARD, vertex bias o.pos.z -= 0.003*o.pos.w, weight = saturate(mmask.r *
art.a), mask+art tex2Dlod LOD0, RShadow lerp kept. Removed earlier gates: art-luminance key (slab) and
auto-LOD mips (bleed). The composite RULES came from texel measurement + the original asm composite
(a = mask*alpha, alpha-over) - those sources are alignment-free. Whether the mesh lands at the right
place on the face is UNVERIFIED (task 2).

## 6. Gaps closed (2026-10-30)

1. **CharaSelectNum1.mat** - extracted with UnityPy; it was a naming gap, not a data gap. The bundle
   holds the material under its in-bundle name `DFG-PopMix-W5 1`, which collides with an existing
   asset, so AssetRipper exported it as `Assets\Material\DFG-PopMix-W5 1.mat`; the addressable name
   `Assets/MFG/Fonts/DFG-PopMix-W5 CharaSelectNum1.mat` exists only in the bundle name table (byte
   2318 of `game\MFGClient_Data\StreamingAssets\StandaloneWindows64\3633e69d040cf55c4855557f2daec047.bundle`).
   Verified via `REPO\scripts\extract_charaselectnum1.py` (path_id 8204823393702252719): raw 1,844 B +
   attrs json under `REPO\build\unity-project-missing\Assets\MFG\Fonts\`, notes in
   `REPO\build\unity-project-missing\manifest.json`. Resolved slots (via rebuild-tree .meta files):
   shader `0ed61b859a0f9944eb6e50f136be9ecb` -> `Assets\Shader\TextMeshPro_Distance Field.shader`,
   main tex `a414bf4b0bfdfad49b158411989c33f6` -> `Assets\Texture2D\DFG-PopMix-W5 Atlas.png`; values
   identical to `Black.mat` except `_FaceColor` white / `_OutlineColor` black swapped. UnityPy 1.25.4
   lacks the typetree-dump helper, so recovery = raw bytes + attrs json (documented in the manifest).
   The other four docs/20 gaps are built-in shaders - not extractable, not needed.
   `extract_missing_unitypy.py` finds 0 gaps because its resolver assumes a catalog id format this
   build does not use; the new script supersedes it.

2. **Mouth + Eye DXBC disassembly** - regenerated in `REPO\build\_tmp_audit_moutheye\` (blobs 191,280 /
   205,752 B; 38 + 40 programs; `progs_{mouth,eye}.json` inventories - json `m_BlobIndex` == region
   index, stage-agreement 38/38 and 40/40; per-program `*_asmNN.txt`; forward VS+PS annotated with
   cbuffer slot->name and t->texture in `*_asm02/08_ann.txt` + `cb_layout_*.txt`). All findings are
   line-cited in `disassembly_report.md` there; headlines:
   - Authored mouth transform (Mouth FORWARD frag, mouth_asm08:289-304):
     `u = (uv.x + 0.25·smod4(_MouthIndex) + _MouthOffsetX)·ST.x + ST.z`,
     `v = (uv.y − 0.25·row(_MouthIndex) + _MouthOffsetY)·ST.y + ST.w` - mesh uv enters **raw**
     (band-anchored: the original mouth island lives inside the v∈[0.75,1] cell band), the index only
     adds 0.25 steps (col right, row down, index 0 = top-left), art and mask share the transformed
     coords, materials ship ST=(1,1,0,0) / offsets 0.
   - So the rebuild's `uv*0.25 + AtlasCellOrigin` (Mouth.shader 402-407, 471-473) is NOT the authored
     mapping. At index 12 the original is exactly `v = uv.y − 0.75` at slope 1; the fitted −0.13/−0.145
     (slope 0.25) matches the authored row/col placement only at isolated uv values - the fit confirms
     cell selection but is a rescale artifact of the glTF-round-tripped mesh, and authored needs no
     fitted constant.
   - Authored eye transform (eye_asm08:299-326): three independent raw uv bases, additive 0.25-cell
     steps for brow/eye art (same scheme, sign matches) - **the rebuild Eye.shader formula already
     matches the original**; pupil differs: original is +0.5·mod4 in u and +1.0·div4 in v (vertical
     no-op), rebuild applies a −0.5 downward v-step and `_IrisMap`-scaled `_PupliOffset*` - iris tuning
     should keep 0.5·index u-steps, drop the pupil v-step, and treat the pupil offsets as raw uv adds.
   - Mouth composite (mouth_asm08:305-312): `col = art.rgb` tinted toward `col·_FaceShadowColor` by the
     feathered 1st-shade mask, `a = _MouthMaskTex.r`, alpha-over the lit+rim body by `saturate(a)` -
     confirms the rebuild's composite rules. `_BlurLevelSGM` doubles as the SGM sample lod (asm08:209 /
     eye 219); the report §4.5 lists the numeric constants the FORWARD frags bake that the rebuild does
     not model 1:1 (sRGB 1.055 curve, 0.299/0.587/0.114 luma, 0.05 unlit scale, 0.95/0.001 SGM gates,
     0.9999 discard threshold, `_UseShadowOnly` alpha routing, LUT z math).
   - Flagged ambiguities (report §4.3/§5): which serialized mesh channel the game binds to the shader's
     TEXCOORD0 (bundle meshes have the UV0 slot disabled - formulas are stated in shader-input terms);
     sampler wrap mode makes the negative row-shift exact only under REPEAT; runtime `_MouthIndex`/
     offsets are set by MouthController, so material files show defaults (index 0).

3. Everything else (meshes, textures, masks, atlases, anims, controllers, scenes, lighting values,
   properties, UI) remains verified present - no further decompiling needed for data.

## 7. Task order (items 1-2 executed 2026-10-30; current next actions in §3b)

1. Rebuild registration: iris-centroid + scale matching per capture pair (landmarks in §4); regenerate
   cmp_face_regions/overlay with corrected alignment; re-derive ALL region stats (skin/blush/iris/nose/mouth)
   against the original ref. Fix mfg_cmp_face.py eye detection (it drifted 64.3 -> 69.7 IOD).
2. Mouth footprint: compute where model_face_mouth projects in capture pixels (project mesh vertices with
   the scene camera from §3, or ZTest-Always debug capture like the proven chain), compare to the true
   mouth zone; determine if the mesh/uv lands too low; if yes, that is the line's real root cause.
3. Lighting unification: resolve OPEN #1; replicate the original select light config in the rebuild; then
   re-evaluate the eye/mouth LightTint(ambient*2) hack against UTS2 (max(ambientSH,_LightColor0)) and
   re-tune iris/mouth winners after, not before.
4. Original mechanism check (OPEN #3) decides whether model_face_front stays or gets replaced by the true
   original surface - affects every downstream color measurement.
5. Only after 1-4: fine color/mouth composite tweaks.

## 8. Tooling and rules

- Capture lane runbook: KB concept
  `/projects/mfg/references/front-server-plumbing.md` (mock one-shot hold, park re-arm, 8094 static front,
  debug_round.ps1 flow, always-close windows rule - now automated and verified). Driver:
  `C:\mfg\tmp\blush_capture.cjs` + `C:\mfgtmp\debug_round.ps1` (build->assemble->re-arm->capture).
- Comparison tool: `C:\mfgtmp\mfg_cmp_face.py` (ref, frame, --out-dir, --reb-top 0 --reb-eyes ... --orig-eyes
  ... --orig-top 25; hints currently stale, see §4). Frames: `C:\mfgtmp\blush_hold_NN.png` (latest =
  mask-only LOD0 build 19:49). Original ref: `C:\mfgtmp\ref_original_charselect.png` (1778x1026).
- Full session notes with all corrections: `C:\mfgtmp\eye_fix_report.txt` (read the two 2026-10-29 closing
  sections). KB: reconstructed-face-shader-contract (mouth section incl registration gotcha),
  eye-iris-composite (iris winner values), face-closeup-fix-rounds (run history).
- Environment rules: English only; trust order owner > in-game pixels > tk math > desk prose; approvals
  off (no sandbox escalation); >5-line code in .ps1/.py files; never leave capture windows open.
