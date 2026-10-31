# Face shader reverse-engineering — final report
(TEMP analysis output; no project files modified. Scripts/dumps: C:\mfg\build\_tmp_audit_face\)

## 0. Headline
The shipped, compiled face shader for Mt_face.mat IS stock UnityChanToonShader v2.0.9
Toon_DoubleShadeWithFeather_TransClipping_StencilMask — instruction-for-instruction.
The material's custom properties (_GBlend, _SkinType, _RReplaceColor*, _FaceShadowColor,
_Chroma/_Contrast/_Brightness, ...) are DEAD DATA: none of them exists in the shader's
property table, in the blob, or in any compiled program. They belong to the
`Bsac/Character/Eye` and `Bsac/Character/Mouth` shaders (202/193 props, verified in
their json parsed forms) — Mt_face.mat carries leftovers from a shader reassignment
(its m_InvalidKeywords ['_'] is another reassignment trace).
Warm tan => lightColor at runtime (_LightColor0 / scene ambient SH), NOT missing HLSL.

## 1. Extraction & container format (documented)
- Face json: export-primary-content\Assets\Toon\Shader\Toon_DoubleShadeWithFeather_TransClipping_StencilMask.json
  m_CompressedBlob = base64 -> 22581 bytes -> LZ4 *block* decompress (python lz4, uncompressed_size
  = m_DecompressedLengths[0][0] = 330872) works at skip 0. Saved: face_blob.bin.
- Blob = "SerializedProgram"-style region table:
  u32 headerSize(84); then 84 entries of {u32 offset, u32 size, u32 flags=0}, contiguous from 1012 to
  end-of-blob. 52 entries are program records, 32 are metadata (per-variant parameter blobs at
  206268..234640 etc. — those contain $Globals param names like _HighColor_Power).
- Program record = 114-byte header: {u32 progHash, u32 nA, u32 nB, u32, u32, u32 type(3/4/5),
  u32 stage, then keyword-name stream: [u32 len][utf8] ... }, then DXBC container:
  'DXBC', 16B MD5, u32(1), u32 totalLen, u32 nsec=3, u32 offsets[3]; sections (at offsets):
  ISGN (D3D11 input signature), OSGN (output signature), SHDR (SM4 microcode, version dword first;
  all programs compiled sm_4_0). NO R5SO reflection section (stripped release build) — uniform NAMES
  come from Unity's own serialized tables in the json (m_NameIndices + m_CommonParameters
  m_OffsetInConstantBuffer), which is even more direct evidence than cbuffer reflection.
- Pass names ("FORWARD"...) are NOT stored as strings in this blob; pass identity established by
  json parsed form (4 passes) + per-pass $Globals sizes matching dcl array sizes exactly +
  per-record keyword sets + VS/PS classification.
- Shmear vs Eye job: that container differed; technique reused verbatim (auditF.py).

## 2. Pass <-> program map (52 DXBC programs, all disassembled: asm*.txt; names annotated: asm*_c.txt)
- Outline pass (LightMode ForwardBase, Cull Front): vs asm02/03 (kws _IS_OUTLINE_CLIPPING_YES ×
  _OUTLINE_NML/_POS), frag asm05/06 ($Globals 272B = 17 f4 = asm05 dcl CB0[17] exact; outline color +
  clip + _Is_OutlineTex lerp; unity_AmbientSky bound).
- FORWARD (ForwardBase): vs asm09..21 (DIRECTIONAL-family), frag asm29..35 (7 variants; byte-size-
  identical duplicate pairs: 29=30, 31=32, 33=34).
  FORWARD frag CB map (asm29): CB0=$Globals[55] (880B, 83 named uniforms — FULL list dumped in
  cb_layout.txt), CB1=UnityPerCamera[5] (_WorldSpaceCameraPos at slot 4), CB2=UnityLighting[47]
  (_WorldSpaceLightPos0 slot 0, unity_SHAr/Ag/Ab/Br/Bg/Bb/C slots 39-45, unity_OcclusionMaskSelector
  slot 46), CB3=UnityPerDraw[3] (unity_ObjectToWorld rows 0-2), CB4=UnityPerFrame[12] (unity_MatrixV
  rows), CB5=UnityProbeVolume[7]. SHADOWS_SCREEN variants asm31/32/35 insert CB3=UnityShadows[26] and
  one more texture2d t14 (screen shadow); t14/t15=texture3d unity_ProbeVolumeSH.
  Frag input signature (asm29): SV_POSITION(unused), TEXCOORD0=uv.xy + TEXCOORD5=mirrorFlag in z
  (packed reg1), TEXCOORD1=posWorld.xyzw, TEXCOORD2=normalDir.xyz, TEXCOORD3=tangentDir.xyz,
  TEXCOORD4=bitangentDir.xyz, SV_IsFrontFace(UNUSED — the `fixed facing : VFACE` param of frag).
  VS (asm09) input: POSITION/NORMAL/TANGENT/TEXCOORD0 — matches VertexInput (cginc L145-152).
- FORWARD_DELTA (ForwardAdd): vs asm42..54, frag asm65..77 (13 PS: POINT/SPOT/POINT_COOKIE/
  DIRECTIONAL_COOKIE/SHADOWS_DEPTH/CUBE/soft × _IS_CLIPPING_TRANSMODE). Consistent with cginc
  L388-422 (spot-check; not line-audited): _StepOffset, _LightIntensity luma*attenuation,
  HiCut filter, same 3-layer composition + HighColor; per-variant $Globals live in parameter
  blobs at 206268..234640 (contain _HighColor_Power etc. strings).
- ShadowCaster: vs asm79/80 (SHADOWS_DEPTH/CUBE), frag asm82/83 = TRIVIAL: samples t1=_MainTex
  (uv*_MainTex_ST) + t0=_ClippingMask (uv*_ClippingMask_ST), same clip chain as FORWARD
  (asm82_c: _IsBaseMapAlphaAsClippingMask / _Inverse_Clipping / +_Clipping_Level, `discard_nz`),
  then `mov o0.xyzw, 0; ret`. $Globals 128B = 5 props exactly (json PASS[3]) = dcl CB0[8].
  No SV_Depth. Offset 1,1 from .shader state.

## 3a. FORWARD frag base/skin color formula (instruction evidence, asm29_* line numbers -> cginc lines)
lightColor (asm 122-179):
  defaultLightColor = saturate(max(0.05*_Unlit_Intensity,
        max(ShadeSH9(0,0,0,1), ShadeSH9(0,-1,0,1).rgb)*_Unlit_Intensity))   [asm 133-155; SH decode
        (log*0.4167=1/2.4/exp +1.055/-0.055 = DecodeHDREnvelope) at 134-152 -> cginc L229]
  customLightDirection = normalize(mul(ObjectToWorld, (1,0,0)*_Offset_X_Axis_BLD*10
        + (0,1,0)*_Offset_Y_Axis_BLD*10 + (0,0,-1)*lerp(-1,1,_Inverse_Z_Axis_BLD)))  [asm 156-164 -> L230]
  lightDirection = normalize(lerp(normalize(V[2]+V[1]), _WorldSpaceLightPos0.xyz, any(...)));
  lightDirection = lerp(..., custom, _Is_BLD)                                  [asm 123-132,165-174 -> L227,231-232]
  lightColor = lerp(max(dflt,_LightColor0), max(dflt,saturate(_LightColor0)), _Is_Filter_LightColor)
        [asm 175-179 -> L234]
Set colors (asm 184-231 -> cginc 248-265), ALL lerps compiled branchless (uniform lerp factors):
  Set_BaseColor     = lerp(_BaseColor.rgb*_MainTex_var.rgb, (*Set_LightColor), _Is_LightColor_Base)
        [asm 184-186: mul r9=cb0[5](Base)*t1(main); mad r10=r9*LC-r9; mad r9=cb0[7].z*r10+r9]
  _1st_ShadeMap_var = lerp(tex2D(_1st_ShadeMap), _MainTex_var, _Use_BaseAs1st)
        [asm 187-190 -> L251];  Set_1st = lerp(_1st_ShadeColor*_1st_map, *LC, _Is_LightColor_1st) [191-193 -> L252]
  _2nd_ShadeMap_var = lerp(tex2D(_2nd_ShadeMap), _1st_ShadeMap_var, _Use_1stAs2nd) [194-197 -> L254];
  Set_2nd = lerp(_2nd_ShadeColor*_2nd_map, *LC, _Is_LightColor_2nd)               [198-200 -> L255]
  HalfLambert = 0.5*dot(lerp(i.normalDir, normalDirTS, _Is_NormalMapToBase), lightDir)+0.5 [201-204 -> L256]
  SystemShadows = (attenuation*0.5)+0.5+_Tweak_SystemShadowsLevel>0.001 ? ... : 0.0001 [209-213 -> L261]
        (attenuation: UNITY_LIGHT_ATTENUATION compiles to probe-volume-only path in asm29 -> 1)
  FinalShadowMask = saturate(1 + (lerp(HL, HL*sat(sys), _Set_SystemShadowsToBase)
        - (_BaseColor_Step-_BaseShade_Feather)) * ((1-_Set_1st_ShadePosition.r)-1)
        / (_BaseColor_Step-(_BaseColor_Step-_BaseShade_Feather)))                  [214-221 -> L262]
  FinalBaseColor = lerp(Set_BaseColor, lerp(Set_1st, Set_2nd, mask2nd(..._ShadeColor_Step,
        _1st2nd_Shades_Feather, _Set_2nd_ShadePosition)), FinalShadowMask)         [222-231 -> L265]
Then HighColor [232-266 -> L266-272], RimLight [267-313 -> L273-283], MatCap [314-430 -> L284-342],
GI = SHEvalLinearL0L1L2(normalDirTS)+DecodeHDREnvelope, smoothstep-filtered *_GI_Intensity
[432-458 -> L345-346,386], Emissive [449-451 -> L349-351], final o0 = saturate(...) + GI + emissive
[458-459 -> L386]; alpha = saturate(_Inverse_Clipping_var + _Tweak_transparency) [460 -> L443].
Clipping (TransClipping): m = lerp(_ClippingMask.r, _MainTex_var.a, _IsBaseMapAlphaAsClippingMask);
inverse; clip(sat(m+_Clipping_Level)-0.5) [92-104 -> L211-216].

ANSWER 3a: no ReplaceColor/Blend/SkinType/Chroma math exists. The art texture enters ONLY as
_MainTex.rgb * _BaseColor.rgb, multiplied by lightColor via the _Is_LightColor_Base lerp; the mask
for the shade step is (1-_Set_1st/2nd_ShadePosition.r) fed by the feather/step remap (NOT texture
alpha; texture alpha only drives clip()+output alpha). _GBlend/_RReplaceColor11/12/21/22/
_RShadowColor/_GShadowColor/_FaceShadowColor/_UseShadowColor/_UseShadowOnly/_Chroma/_Contrast/
_Brightness are NEVER read (0 string occurrences in entire 330872-byte blob; $Globals = 83 named
uniforms, all stock UTS2; 121-prop shader-level table all stock).

ANSWER 3b — textures sampled by FORWARD frag (slot proof = *_ST pairing in each mad):
t0 _NormalMap [77], t1 _MainTex [92], t2 _ClippingMask [94], t3 _1st_ShadeMap [187],
t4 _2nd_ShadeMap [194], t5 _Set_2nd_ShadePosition [205], t6 _Set_1st_ShadePosition [207],
t7 _Set_HighColorMask [232], t8 _HighColor_Tex [251], t9 _Set_RimLightMask [267],
t10 _NormalMapForMatCap [358], t11 _MatCap_Sampler (sample_l, LOD=_BlurLevelMatcap) [400-401],
t12 _Set_MatcapMask [402], t13 _Emissive_Tex [449], t14/t15 unity_ProbeVolumeSH (texture3d) [118],
plus t14(2d) unity_ScreenShadow in SHADOWS_SCREEN variants. NO face-shadow/gradient mask texture.
Unbound material textures bind to shader defaults ("white"): _1st_ShadeMap, _2nd_ShadeMap,
_Set_1st/2nd_ShadePosition, _Set_*Mask, _NormalMap, _Emissive_Tex (Mt_face.mat fileID: 0).

## 3c. Numbers for Character1 serialized values
With _BaseColor=1, _Use_BaseAs1st=1, _1st_ShadeColor=_2nd_ShadeColor=white, _Use_1stAs2nd=0 with
_2nd_ShadeMap unbound=white, _ShadeColor_Step=0 (2nd mask=0 always), _BaseColor_Step=0.55/feather
0.01, _Set_SystemShadowsToBase=0, attenuation=1, _MatCap=0, HighColor black, _GI_Intensity=0,
_Emissive 0: Set_BaseColor == Set_1st == Set_2nd(=flat LC, never selected) =>
FINAL = saturate(_MainTex.rgb * LC + rim), rim = LC * sat(step(0.5, pow(1-dot(Nts,V), 2.8284)) -
((1-(0.5*dot(n,LD)+0.5)) + 0.5))  [_RimLight=1, Power 0.5 -> pow 2.83, InsideMask 0.5 FeatherOff=1,
LightDirMask=1 Tweak 0.5, mask tex white] ; LD = BLD custom dir normalize((-4.8,0,+1) obj).
So: MULTIPLIER = LC per channel, identical on lit AND shaded pixels.
LC = saturate(max(ambientSH-decoded*_Unlit_Intensity(=1) vs 0.05 floor, saturate(_LightColor0))) ≤ 1.
Numeric table (python f17_numeric.py, output numeric_eval.txt):
  LC=(1,1,1)      pale(219,154,137) -> (219,154,137); silhouette-lit -> (255,255,255) [rim +0.47·LC]
  LC=(1,.95,.75)  pale -> (219,146,103) warm tan;  white -> (255,242,191); +rim at lit silhouette
  LC=(1,.88,.70)  pale -> (219,136,96);           white -> (255,224,178)
Warm tan therefore implies original scene LC ≈ (1, 0.90±0.05, 0.70±0.1) from warm _LightColor0 and/or
warm ambient probe; our rebuild's LC≈(1,1,1) gives flat pale. Alpha out = _MainTex.a (Tweak_transp 0).

## 4. Minimal patch (stock -> original face behavior)
Honest minimal answer: ZERO shader changes reproduce the ORIGINAL (shader source is identical);
the difference is runtime lighting. Preferred no-patch fix: give the rebuild the original scene
lighting — a warm color whose max(ambientSH, sat(LC0)) lands at ~(1,0.95,0.75), or set Mt_face
_BaseColor to that tint (material-only; _BaseColor multiplies art at asm184 / cginc L249 exactly
like lightColor but without needing lights).
If a lightColor-independent in-shader reproduction is wanted, scoped patch (duplicate shader for
Mt_face; stock materials unaffected), 8 diff lines:
  A) Copy Toon_DoubleShadeWithFeather_TransClipping_StencilMask.shader -> Toon_Face_TransClipping_StencilMask.shader
     L1   Shader "UnityChanToonShader/Toon_Face_TransClipping_StencilMask" {
     after L118 add:   _FaceTint ("FaceTint (original scene lightColor)", Vector) = (1, 0.95, 0.75)
     before L194 (FORWARD include) and before L228 (FWDDELTA include) add:  #define FACE_TINT
  B) UTS_DoubleShadeWithFeather.cginc:
     L143 (beside _GI_Intensity) add:  uniform float4 _FaceTint;
     after L241 (#endif closing the lightColor branches) insert:
         #ifdef FACE_TINT
                 // shipped face = UTS2 under a warm scene light; in light-less scenes
                 // lightColor collapses to neutral, so emulate the original LC ceiling.
                 lightColor = min(lightColor, _FaceTint.rgb);
         #endif
     L241/242 is the single point where BOTH FWDBASE (L234) and FWDDELTA (L240) lightColor exist;
     min() keeps stock variants pixel-identical when _FaceTint=(1,1,1,1) default in every other
     material (the #define only exists in the duplicated face shader). The tint then flows through
     Set_BaseColor, Set_1st/2nd, rim, matcap — exactly how the original warm LC behaved everywhere.
  Then re-point Mt_face.mat to the new shader (its serialized _FaceTint wins if present).

## 5. Delta/ShadowCaster/lightmap/probe notes
- FORWARD frag uses light PROBES unconditionally (unity_SHAr..SHC via SHEvalLinearL0L1/L2 +
  DecodeHDREnvelope, asm 133-152 & 432-447) and unity_ProbeVolumeSH 3D texture inside
  UNITY_LIGHT_ATTENUATION (asm 105-122, guarded by unity_ProbeVolumeParams.x) — identical to what
  stock UTS2 compiles under Unity 2022.3; no bespoke lighting code.
- NO lightmap path shipped: UnityPerDraw is dcl'd only rows 0-2 (unity_LightmapST never read), no
  RGBM/bc6h decode constants in any of the 7 frag variants despite LIGHTMAP_ON/DYNAMICLIGHTMAP in
  the serialized keyword scope -> variant stripping removed lightmaps. Light reaches the face only
  via _LightColor0 + ambient SH + (optional) probe volume/screen-shadow variants.
- ShadowCaster frag trivial & stock (see §2). FORWARD_DELTA consistent with stock L388-422.
- All programs sm_4_0, ps/vs_4_0, dynamic branching on uniform flags (no keyword branch residue);
  the material's valid keywords: _EMISSIVE_SIMPLE, _OUTLINE_NML.

## Key evidence files
face_blob.bin (330872B) | progs2.json (program inventory: region, keywords, sections) |
asm02..asm83.txt (D3DDisassemble of all 52) | asm29_ann.txt/asm29_c.txt (name+component annotated
FORWARD frag) | cb_layout.txt (per-pass cbuffer/uniform tables w/ byte offsets) |
numeric_eval.txt | f9/f10/f11/f16/f17 scripts.
