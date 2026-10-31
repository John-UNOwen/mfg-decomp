# Mouth / Eye shader DXBC disassembly — re-extracted from AssetRipper PrimaryContent

Date: 2026-10-30. Companion to `docs/24-face-asset-inventory.md` §6.

## 0. Method, artifacts, verification

Sources (read-only): `build/export-primary-content/Assets/Shaders/Character/{Mouth,Eye}.json`
(player-platform program tables) + the same assets' compressed program blobs, re-decompressed
with the recipe from the face audit (`me0_extract.py`: lz4 block, skip 0,
`m_DecompressedLengths[0][0]`). Everything below lives in
`build/_tmp_audit_moutheye/`:

| artifact | content |
| --- | --- |
| `mouth_blob.bin` (191 280 B), `eye_blob.bin` (205 752 B) | decompressed SerializedProgram containers |
| `progs_mouth.json`, `progs_eye.json` | full program inventory: region index, progHash (sha1[:12] of SHDR), stage, pass id + per-variant keywords (json side) and keyword-prefix (blob side), `$Globals` size, DXBC/section offsets |
| `mouth_asmNN.txt` (38 files), `eye_asmNN.txt` (40 files) | D3DDisassemble text for every program (D3dCompiler 47 via ctypes) |
| `mouth_asm02_ann.txt`, `mouth_asm08_ann.txt`, `eye_asm02_ann.txt`, `eye_asm08_ann.txt` | forward VS/PS annotated with `cbN[slot] → uniform name` + `tN → texture name` (from `cb_layout_*.txt`, derived from json `m_NameIndices` + `m_CommonParameters`; t-slot names from `*_ST` mad-pairing) |
| `cb_layout_mouth.txt`, `cb_layout_eye.txt` | per-pass cbuffer slot→name tables |
| scripts `me0..me10*.py` | reproducible pipeline |

Container layout (same as face): u32 region count at 0; region entries `{u32 off, u32 size,
u32 flags}` from offset 4; program regions hold record header (28 B) + keyword-name stream
(`[u32 len][utf8]`…) then `DXBC` (16 B MD5, u32(1), u32 totalLen@dx+24, u32 nsec@dx+32; only
ISGN/OSGN/SHDR sections → no R5SO, so uniform names come from the json, not the asm).

Mapping json `m_BlobIndex → region index`: **identity**, re-verified here:
stage agreement **38/38 (Mouth), 40/40 (Eye)** (`me9_inventory.py`, field
`stage_agreement`); keyword-prefix vs json `m_KeywordIndices` matched exactly for 36/38 +
38/40 programs; the exceptions carry only the `_IS_CLIPPING_OFF6` token (AssetRipper keyword-
table quirk seen in the face audit). Program stage is read from the SHDR program-header dword
at SHDR+8 (bit 16 set = VS `0x10040`, clear = PS `0x40`, sm_4_0).

## 1. Pass ↔ program map

Both shaders: 4 (Mouth) / 5 (Eye) passes in `m_SubShaders[0]`; `m_Name` is empty in the json,
pass identity comes from the `_IS_*` keywords. Region = `m_BlobIndex`.

| pass | identity (keywords) | Mouth regions | Eye regions |
| --- | --- | --- | --- |
| 0 | `_IS_PASS_FWDBASE` — ForwardBase (2 VS+PS variants each: DIRECTIONAL, +SHADOWS_SCREEN) | VS 2,3,4,5 / PS 8,9 | VS 2,3,4,5 / PS 8,9 |
| 1 | `_IS_PASS_FWDDELTA` — ForwardDelta additive lights (DIRECTIONAL/POINT/SPOT ×shadow, 13 VS+PS variants) | VS 17–29 / PS 40–52 | VS 17–29 / PS 40–52 |
| 2 | `_IS_CLIPPING_OFF` — ShadowCaster (tiny VS, dummy PS) | VS 54,55 / PS 57,58 | VS 54,55 / PS 57,58 |
| 3 | no `_IS_*` — Meta | VS 60 / PS 61 | VS 60 / PS 62 |
| 4 | — | — | VS 63 / PS 64 |

ForwardBase first-variant = **region 2 (VS) + region 8 (PS)** in both shaders; those four
files are the annotated ones. `$Globals` = 1376 B in both (86 float4 slots), identical
layout for Mouth and Eye (they are two shaders sharing one authored parameter set —
`cb_layout_mouth.txt` ≡ `cb_layout_eye.txt` for slots 2–85).

Uniform facts that matter below (slot names from `cb_layout_*.txt`):
- Mouth: slot 11.w `_MouthIndex`, 12.w `_MouthOffsetX`, 13.w `_MouthOffsetY`, 14 `_MouthTex_ST`,
  15 `_MouthMaskTex_ST`, 46 `_MaterialMaskTex_ST`, 4 `_DissolveTex_ST`, 5.x/y
  `_DissolveMin/MaxThreashold`, 40 `_FaceShadowColor`, 48 `_DissolveColor`, 54 `_ShadingGradeMap_ST`,
  74 `_Emissive_Tex_ST`, 84.w `_UseShadowOnly`, 85.x `_GI_Intensity`.
- Eye: slot 11.x/y/z = `_PupilIndex`/`_EyeBrowIndex`/`_EyeIndex`, 12 = offsets X
  (`_PupliOffsetX`,`_EyeBrowOffsetX`,`_EyeOffsetX`), 13 = offsets Y, 6–10 = `_PupilTex_ST`,
  `_EyeBrowTex_ST`, `_EyeBrowMaskTex_ST`, `_EyeTex_ST`, `_EyeMaskTex_ST`.
- Materials (`mats_out.txt`, from rebuild `Assets/Material/Mt_mouth*`+`Mt_eye*`): every atlas
  `*_ST` = scale (1,1), offset (0,0); every index/offset float = 0; `_Height` = 0 with
  `_HeightTex` unbound; `_StencilMask` = 4. So authored defaults are identity STs.

## 2. Mouth FORWARD FRAG (`mouth_asm08.txt`, pass0 PS, DIRECTIONAL)

### 2.1 VS feeding it (`mouth_asm02.txt`)
- 77: `o1.xy = v4.xy` — TEXCOORD1 input passed raw (aux uv).
- 115: `o7.xy = TRANSFORM_TEX(v4, unity_LightmapST)`; 116: `o7.zw = v3.xxxy` — TEXCOORD0 input
  passed raw as `(x,y)` (no scaling anywhere in the VS).
- Mouth mesh (original bundle channels, UnityPy dump `dump_mesh_channels.py`): UV0 **disabled**,
  UV1 = `[0,0.7219]×[0.25,0.5]`-mode rect, UV2 = `[0,0.2475]×{[0,0.125]∪[0.75,1.0]}` bimodal
  (me8_uvhist: 113 verts in the v<0.125 island, 176 verts in the v∈[0.75,1] band). Which mesh
  channel lands on which shader input in-game is a Unity channel-binding question (see §4.3);
  the formulas below are stated in terms of shader inputs, exactly as the asm shows.

### 2.2 The authored uv→cell transform (asm08 lines 289–304)
```
289  and  r1.w, cb0[11].w, 0x80000000      ; sign bit of _MouthIndex (integer ops ⇒
290  imax r2.x, cb0[11].w, -cb0[11].w        original cbuffer holds it as INT despite the
291  and  r2.x, r2.x, 3                      material property being Float)
292  ineg r2.z, r2.x
293  movc r1.w, r1.w, r2.z, r2.x            ; c = ±(|index| mod 4)   (column, signed)
294  itof r1.w, r1.w
295  itof r2.x, cb0[11].w
296  add  r2.x, -r1.w, r2.x                 ; q = index − c = 4·trunc(index/4) (row part)
297  mad  r1.w, r1.w, 0.25, v7.z            ; u = v7.z + 0.25·c
298  mad  r2.x, -r2.x, 0.0625, v7.w         ; v = v7.w − 0.0625·q = v7.w − 0.25·q/4·4
                                              ; (q=4·row ⇒ v = v7.w − 0.25·row)
299  add  r3.x, r1.w, cb0[12].w             ; u += _MouthOffsetX
300  add  r3.y, r2.x, cb0[13].w             ; v += _MouthOffsetY
301  mad  r2.xz, r3.xy, cb0[14].xy, cb0[14].zw  ; TRANSFORM_TEX(uv, _MouthTex_ST)
302  sample r4, r2.xz, t5, s2               ; t5 = _MouthTex   (plain bilinear sample)
303  mad  r2.xz, r3.xy, cb0[15].xy, cb0[15].zw  ; TRANSFORM_TEX(uv, _MouthMaskTex_ST)
304  sample r3, r2.xz, t6, s3               ; t6 = _MouthMaskTex (same transformed coords)
```
i.e. authored formula (v7.zw = VS TEXCOORD15.zw = raw mesh TEXCOORD0 input):
```
u = (uv.x + 0.25·smod4(index) + _MouthOffsetX)·ST.x + ST.z
v = (uv.y − 0.25·(index − smod4(index))/4 + _MouthOffsetY)·ST.y + ST.w      ST = _MouthTex_ST
```
Key properties: **no 0.25 compression of the mesh uv, no origin lookup** — the mesh uv already
sits *inside* its cell (band-anchored), the index only adds integer 0.25-steps
(+x for columns, −y for rows: index 0 = top-left, rows run downward); the material ST scales
the shifted value (default ST=(1,1,0,0) ⇒ passthrough). Coordinate order = standard TRANSFORM_TEX
(x→u·ST.x+ST.z, y→v·ST.y+ST.w); both art and mask use the identical transformed coords.

### 2.3 Sampling set and lod
- t0 `_MaterialMaskTex`, t1 `_DissolveTex`, t2 `_MainTex`, t3 `_MaskTex`, t7 `_Emissive_Tex`:
  plain `sample` of `TRANSFORM_TEX(v1.xy=aux uv, …_ST)` at 125–126, 127–128, 136–137, 138–139,
  313–314.
- t4 `_ShadingGradeMap`: **`sample_l` with lod = cb0[55].y = `_BlurLevelSGM`** (208–209) — the
  only explicit-LOD texture op in the forward frag; art/mask/emissive samples carry no lod.
- t8 = color-grading LUT (texture3d), sampled only when `cb4[0].x==y==1` (65–78); z slice
  `max(viewY·0.25+0.75, gradeZ·0.5+0.75)`.
- Dissolve mask (129–135): `clamp((t1.r − _DissolveMinThreashold)/(_DissolveMaxThreashold −
  _DissolveMinThreashold))` with movc hard-camps → r2.w.

### 2.4 Composite math (weights, lerps)
- 140–190: `_MaskTex` rgb-zone replace chain (zones × `_R/G/BUseBaseColor`, `_R/G/BBlend`,
  lerp of `_R/G/BReplaceColor11/12` toward `_R/G/BShadowColor`, 190 = `_SkinType∈[0,5]`&
  coverage select) — byte-identical structure in the Eye frag (eye 150–195). Skin-type pair
  select 197–200 swaps shade step/feather pairs cb55.zw↔cb56.xy, cb55.zw↔cb56.zw.
- 206–207 half-Lambert `d·L·0.5+0.5`; 210–219 SGM-adjusted toon weight (SGM r + `_Tweak_ShadingGradeMapLevel`
  + occlusion `dp4_sat(occlLUT, unity_OcclusionMaskSelector)` 82 + `_Tweak_SystemShadowsLevel`),
  gated `0.001` (212,217 fallback 0.0001); 213–215 SGM<0.95 zone flag with `_Tweak_ShadingGradeMapLevel`.
- 220–224 / 229–233: feathered ramps to 1st/2nd shade masks `r2.y`, `r1.w` (step±feather from
  the skin-type-selected pair); 225–240 shade chain `_1st/_2nd_ShadeColor` × `_Is_LightColor_*`
  light-tint lerps (288 mixes rim in via `_RimLight`).
- Rim 241–287: `pow` via exp/log chains, `_RimLight_Power·(−3)` exponent scale (247),
  `_RimLight_InsideMask` feather ramp (252–258), `_Tweak_LightDirection_MaskLevel`/
  `_Add_Antipodean_RimLight` second lobe (265–277), `_Inverse_RimLightMask`,
  `_Emission_RimLight[_Power/_Color]` additive (284–286), `_RimLight` master (288).
- **Mouth art overlay** 305–312:
  `col = (art.rgb, mask.r)` (305–308 alpha from `_MouthMaskTex`.R);
  `col = lerp(col, col·_FaceShadowColor, firstShadeMask)` (309–310, mask r2.y from 220–224);
  `lit = sat-col'·col' + …` → 311–312: `result = lit_base + saturate(alpha)·(col − lit_base)`
  — art is painted over the lit+rim body by its mask alpha.
- 313–316 emissive: `t7·… + max(_Emissive_Color, _EffectEmissive_Color)` (material color
  default (0,0,0,1) ⇒ max picks (0,0,0,1)-based).
- 317–341: 9-term SH ambient (`cb3[9..11]` float4 SHAr/Ag/Ab + `cb2[39..45]` float3 SHAr/Ag/
  Ab/SHBr/Bg/Bb/C), gamma-ish `pow(1/2.4)` decode (93–108, 329–333: `log·0.416667·exp` +
  `1.055·x^… − 0.055`), luminance `0.299/0.587/0.114` (334) × `_GI_Intensity` (336),
  smoothstep `w²(3−2w)` blend (337–341).
- 341–343 output: `o = lerp(lit+ambient+emissive, _DissolveColor, dissolveMask)`
  (`o.xyz = r2.w·cb48 + graded`).
- 344–346 `discard` when dissolveMask > 0.9999; 347–348 `o.w = _UseShadowOnly ? firstShadeMask
  : _BaseColor.a` (shadow-only materials get lit-region-only alpha).

## 3. Eye FORWARD FRAG (`eye_asm08.txt`)

Same body as the mouth (aux samplings 135–136/146–149, dissolve 137–145 with `_DissolveTex`
t1 + min/max thresholds, `_MaskTex` zone chain 150–195, SGM `sample_l` lod=`_BlurLevelSGM`
218–219, shade ramps 220–250, rim 251–298, emissive 337–340, SH/grading 342–365, dissolve
out + discard 367–370, `_UseShadowOnly` alpha 371–372). The eye-specific part:

### 3.1 VS feeding it (`eye_asm02.txt`) — four mesh uvs
Inputs v3=TEXCOORD0, v4=TEXCOORD1, v5=TEXCOORD2, v6=TEXCOORD3 (all used). Outputs:
```
121  o7.xy = TRANSFORM_TEX(v4, unity_LightmapST)   ; o7.zw = v3.xxxy = uv0 (raw pair)
123  o8.xy = v6.xy      (TEXCOORD3 raw)
124  o8.zw = v5.xxxy    (TEXCOORD2 raw pair)
```
so the PS receives **three independent raw mesh uv bases** (v7.zw, v8.xy, v8.zw) — one per
atlas family — and does the index stepping itself.

### 3.2 Per-family index decomposition (asm08 lines 299–326)
Same integer idiom as the mouth (`sign bit`/`imax`/`and 3`/`ineg`/`movc`/`itof`), applied per
component of cb0[11] (reordered `.yxzy` = EyeBrow, Pupil, Eye):
```
299–306  c_x = ±(_EyeBrowIndex mod 4); q_x = _EyeBrowIndex − c_x (=4·row)  [same for Pupil, Eye]
307  r2.x = c_x·0.25 + v8.z ; r2.z = c_pupil·0.5 + v8.x
308  r1.w = −q_x·0.0625 + v8.w      → v_brow = v8.w − 0.25·row(_EyeBrowIndex)
309  r5.y = r1.w + cb0[13].y        (+ _EyeBrowOffsetY)
310  r1.w = q_pupil·0.25 + v8.y     → v_pupil = v8.y + 1.0·(PupilIndex div 4)   (0.25·q=1.0·row)
312  r5.w = r1.w + cb0[13].x        (+ _PupliOffsetY)
311  r5.xz = (v8.z + 0.25·c_brow + _EyeBrowOffsetX,  v8.x + 0.5·c_pupil + _PupliOffsetX)
313–316 eye art: u = v7.z + 0.25·(EyeIndex mod 4) + _EyeOffsetX ;
                 v = v7.w − 0.25·(EyeIndex div 4)   + _EyeOffsetY
317–326 TRANSFORM_TEX + sample:
  t5=_EyeBrowTex     uv=(r5.x,r5.y)·_EyeBrowTex_ST     t6=_EyeBrowMaskTex same uv ·_EyeBrowMaskTex_ST
  t7=_PupilTex       uv=(r5.z,r5.w)·_PupilTex_ST
  t8=_EyeTex         uv=(r3.x,r3.y)·_EyeTex_ST         t9=_EyeMaskTex    same uv ·_EyeMaskTex_ST
```
Grid facts implied: brow/eye-art = 0.25-cell 4×4 grid stepped downward (row 0 at high v);
pupil = **0.5-wide columns (mod4 → u+0..1.5) and a +1.0·row step** (≡ no vertical change under
wrap; effectively a 2-column-style variant strip, matching the rebuild's 2-column assumption in
u but *upward*-unit in v — see §4.4).
Composite 327–336: `m = 1−_EyeMaskTex.g` (327); `pupilMix = t7.xyz·t9.g + t7.xxx·(1−t9.g)`
(grayscale/pupil-channel mix, 328–329); `a = max(t9.g, t9.b)` (330);
`brow = lerp(pupilMix, _EyeBrowTex, _EyeBrowMaskTex.r)` (331–332); tint lerp toward
`×_FaceShadowColor` by toon weight r2.y (333–334); final overlay onto lit body by
sat(`lerp(_EyeBrowTex.a, a, _EyeBrowMaskTex.r)`) (335–336).

### 3.3 The eye mesh's UV0 channel
Original C001 eye meshes are exported in the same channel style as the mouth (uv0 slot
disabled in bundle meshes — `dump_mesh_channels.py` shows `model_face_mouth`; the eye mesh
follows the same authored scheme per its 4-input VS). The rebuild eye pass already samples
`v.texcoord0.xy + AtlasCellOrigin − (0,0.75)` — the same additive shape as the original.

## 4. Original vs the rebuild (`C:\mfg\build\unity-project\Assets\Shaders\Character\*`, read-only)

### 4.1 Mouth — **the original is NOT the rebuild's `uv*0.25 + AtlasCellOrigin`**
Rebuild (`Mouth.shader` 402–407, 471–473):
`mouthUV = (uv0.x, 0.25·uv0.y) + (col·0.25, 1−(row+1)·0.25) + (_MouthOffsetX, _MouthOffsetY)`.
Original: `(uv.x + col·0.25 + offX, uv.y − row·0.25 + offY)·ST + STzw` — mesh uv enters at
**slope 1.0** (band-anchored: mouth island lives in the v∈[0.75,1] band, x≤0.2475, already
cell-sized — §2.1/§4.3), row 0 at top, rows downward (row step sign matches the rebuild's
`1−(row+1)·0.25` family), offsets added pre-ST in uv units (matches rebuild placement).
The rebuild's scheme only reproduces the original if the mesh uv fed to it is the original
channel data: with original band values, `0.25·uv.y + 0` (index 12) maps v∈[0.75,1]→[0.1875,
0.25] (a 1/4-height sliver at the cell's top) while the original maps to exactly [0,0.25].
Conversely with the rebuild pipeline's glb-derived uv0 (island stretched to y∈[0.009,1.0]),
the original formula would stretch 4× beyond the cell. Neither uv*0.25+origin nor the fitted
shifts are the authored transform; they are rescale-approximations forced by the glTF round-trip.

### 4.2 What the original implies for the fitted −0.13 / −0.145 at index 12
For index 12 (row 3, col 0), original `v = uv.y − 0.75` exactly. The rebuild's fitted
`v = 0.25·uv.y − 0.13` (or −0.145) equals the original only at isolated uv (c=−0.13 crosses at
uv.y≈0.827 — right in the island band) with a 4× different slope. So: the fit confirms the
row-3/0.25-step reading of the authored scheme (both put the island near v∈[0,0.25]) but
**refutes uv*0.25+origin as the mapping** — authored is additive at slope 1 on band-anchored
original uv, needing **no** fitted constant (−0.75 falls out of the formula; the measured
−0.13/−0.145 is the compression-compensation artifact of the stretched-uv rebuild).

### 4.3 Honest ambiguity: which mesh channel is which shader input
The bundle meshes serialize **UV0 disabled** with data in UV1/UV2 (UnityPy table: channel3
dim0; UV1=stream1@0; UV2=stream1@8). The authored VS reads TEXCOORD0 (cell base) and TEXCOORD1
(aux). Static evidence pins the formula but not the game-side channel↔input binding (slot-index
binding vs sequential-fill remap; the game may also repack uvs when it assembles the combined
mesh at runtime). Consequences: under "first available uv → TEXCOORD0" the cell base = the
band island's own uv (clean additive story of §4.1–4.2, aux = UV1 rect); under strict slot
binding TEXCOORD0 would read zeros and the cell sample degenerates to a constant texel —
inconsistent with the visible game, so the band-island reading is the operative one. Flagging
this because any uv re-normalization in mesh import silently changes which formula is correct.

### 4.4 Eye — rebuild matches shape, differs in grid details
- Eye art: rebuild `v.texcoord0.xy + AtlasCellOrigin(_EyeIndex,4,0.25) − (0,0.75)` (Eye.shader
  571–572) = `uv + (col·0.25, −row·0.25)` — **exactly the authored transform** (§3.2) when fed
  original channel uv. Confirms the additive scheme is already correct in the rebuild eye.
- Brow: rebuild uses the same 4×0.25 shape (584–585) — matches.
- Pupil/iris: original steps are **+0.5·(PupilIndex mod 4) in u and +1.0·div4 in v** (307,310,
  312), on the independent v8.xy/v8.zw mesh-uv base per side. Rebuild (`606–616`) uses
  `AtlasCellOrigin(_PupilIndex, 2.0, 0.5)` (→ −0.5 v-step per row, downward) inside a rect-
  remapped space (`_IrisMap`), with `_PupliOffset*·_IrisMap.zw`. For PupilIndex<4 both agree in
  u (+0.5·index); the v component differs in sign/scale (+1.0 ≡ wrap no-op vs −0.5 downward) —
  an index ≥4 would diverge, and iris offsets in the original are **pre-ST uv units, not
  rect-scaled** (`_PupliOffsetY + _IrisOffsetY` mixing in the rebuild is a rebuild-only term).
  Iris-transform tuning should therefore (a) keep u-steps = 0.5·index, (b) drop any downward
  v-step for pupil rows unless an index≥4 variant is proven, and (c) treat `_PupliOffset*` as
  raw uv additions.

### 4.5 Numeric constants baked in the FORWARD frags (line cites; = things a rebuild must match)
Shared set (mouth 93–108/329–333, eye 104–118/354–357): gamma decode `log·(1/2.4=0.416667)·exp
+ ×1.055 − 0.055`; luma `0.299/0.587/0.114`; LUT slice `0.25/0.75` + `0.5·gradeZ+0.75`;
unlit scale `×0.05` (89/99); SGM gates `0.95` and `0.001`, weight fallback `0.0001`; rim exponent
scale `×3`; `max(...,1e-6)`-style sat-step idiom (`×100000` + `0.999999`); discard threshold
`0.9999`; `_BlurLevelSGM` doubles as the SGM sample lod; dissolve mask uses
`(_DissolveMaxThreashold − _DissolveMinThreashold)` divisor with no zero-guard (asm relies on
material defaults keeping it nonzero). Rebuild status (Mouth.shader/Eye.shader hand-written
ramps): half-Lambert/feathered shade + rim exist (`ToonStep`, `HalfLambertShade` 409–417) but
the list above (`0.95` SGM gate role, lod-as-blur trick, LUT z math, sRGB 1.055 curve, 0.05
unlit scale, 0.9999 discard, `×100000` sat idiom, `_UseShadowOnly` alpha routing) is **not
modeled 1:1** in the rebuild; it is cosmetic-tuning-relevant, not blocking.

## 5. What could not be recovered
- Texture identity is asm-invisible (no R5SO): resolved indirectly via `*_ST` slot pairing +
  material references (§1, tex map lines in the `_ann` files). Two slots stay name-only:
  mouth t8 / eye t11 (color-grading LUT texture3d, bound outside the material) and eye t4 =
  `_ShadingGradeMap` is named. `cb3` (float4-view of UnityLighting) is labeled by inference.
- The game-side mesh-channel→TEXCOORD binding ambiguity (§4.3) can't be closed from the
  export alone (needs the authored HLSL or an in-engine experiment).
- Sampler states (wrap/mip/filters) aren't in the DXBC; wrap-mode assumptions for negative v
  (row shift under CLAMP vs REPEAT) follow the texture import settings, not the asm.
