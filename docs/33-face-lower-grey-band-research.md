# 33. The shade band on the bottom of the face (research only)

Owner report: when a character's head is slanted, the bottom of the face carries a light grey shade.
Owner follow-up: it shows in every view, and the character is **トイトイ / Toitoi (Character7)**.
Reference captures for that character: `C:\mfg\ref\vncviewer_vcAe76NLpo.png` (440x418 bust) and
`vncviewer_3EQLRU24PD.png` (1920x1080), folder 7 of `C:\mfg\ref\MANIFEST.md`.
Nothing in the shipped build was changed for this pass; everything below is measurement.

## 1. The band is in the original too — ours is wider and darker

Both images were sampled on the same normalized axis: row 0 = eye centre, row 1.0 = the chin line
(landmarks from the vision groundings: original eyes y 229-265 / chin 312; live crop eyes y 259-399 /
chin 546-568), at the face centre column and at cheeks ±0.28 of the eye span. Values are
`rgb` plus saturation `(max-min)/max`.

| normalized row | original select, cheek | live lane, cheek | original, face centre | live, face centre |
|---|---|---|---|---|
| 0.4 | (247,202,186) 0.25 | (237,205,188) 0.21 | hair strand | (240,220,205) 0.15 |
| 0.6 | (247,208,195) 0.21 | (238,220,207) 0.13 | (221,170,154) 0.30 | (233,220,211) 0.09 |
| 0.8 | (247,223,214) 0.13 | (255,254,250) 0.02 | (239,208,194) 0.19 | (241,225,214) 0.11 |
| 0.9 | **(225,158,132) 0.41** | **(211,162,131) 0.38** | (249,226,215) 0.14 | (229,211,199) 0.13 |
| 1.0 | **(225,158,132) 0.41** | **(211,162,131) 0.38** | (247,221,210) 0.15 | (228,207,196) 0.14 |
| 1.1 | **(225,158,132) 0.41** | **(211,162,131) 0.38** | (247,216,206) 0.17 | **(211,162,131) 0.38** |
| 1.2 | (225,158,132)/(247,213,209) | (242,220,214) 0.12 | **(225,158,132) 0.41** | (216,172,146) 0.32 |
| 1.4 | (249,219,214) 0.14 | (242,220,214) 0.12 | (249,219,214) 0.14 | (242,220,214) 0.12 |

Read the rows 0.9-1.2: the original does have a warm saturated band (225,158,132, sat 0.41) sitting on
the jaw at the chin line, and it is bounded by pale skin above (247,220,208) and pale neck below
(249,219,214). Ours has the same colour family (211,162,131, sat 0.38) — the difference is extent, not
invention: in the original the chin **centre** stays bright down to row 1.1 (247,216,206) and the band
is on the jaw sides; ours floods the centre at row 1.1 as well, sits about one sample step higher, and
is ~16 units darker in R. The eye check on the matched crop (`C:\mfg\tmp\ref_vs_live_face.png`) agrees:
the original keeps cheek blush and a localized shadow at the chin line; ours is "lighter and flatter,
with a soft, streaky desaturated band across the lower cheek/jaw above the chin line".

So the target is a band that is **too wide, slightly too high, and missing the bright chin centre**.

## 2. What is actually on those pixels for Toitoi (MahjongPlay, ScorePanel camera)

`Assets/Editor/FaceLightProbe.cs` (Editor-only, never packed) instantiates the shipped C7 prefab
`Assets/MFG/Characters/Character7/3D/C001/Prefabs/CharaModel.prefab` into the real MahjongPlay scene,
renders 900x1200 offscreen and switches one renderer at a time. `CONTRIB` counts the pixels that
disappear when that renderer is turned off — i.e. who really paints the frame:

| renderer | material / shader | visible pixels | frame rows | frame cols |
|---|---|---|---|---|
| `model_face_mouth` | `Mt_mouth` → `Bsac/Character/Mouth` | **12559** | 717-819 | 352-547 |
| `model_face_eye_L/R` | `Mt_eye_*` → `Bsac/Character/Eye` | 4836 / 7946 | 622-742 | 345-557 |
| `model_face` | `Mt_face` → `Toon_DoubleShadeWithFeather_TransClipping_StencilMask` | **2423** | 718-855 | 415-484 |
| `model_hair_bangs / side / back / side2 / braid` | `Mt_hair` | 48823 / 42684 / 38458 / 6041 / 3762 | 416-880 | — |
| `model_cloth_hoodie` | `Mt_jacket` | 186182 | 674-1199 | 0-899 |
| `model_bodyneck` | `Mt_upper` | 8314 | 834-927 | 334-564 |
| outline renderers (`Mt_outline_*` → `sg_cha_outline`) | | 15 (face) … 643 | — | — |

Rows are top-down frame rows, verified against the PNG diff (see §6). Two things fall out:

* Toitoi's face is mostly **hair**: the bangs and side hair cover 95k+ pixels of the head area, while
  the face shell shows only 2423. In this framing the shell is visible only as a strip **below** the
  mouth card (rows 718-855 against the card's 717-819).
* The mouth card is not a decorative plate: `PIXDIFF full_vs_nocard = 12559 px` (1.2% of the frame).

The column profile through the card region at x=449 (pose 0,0), `full` against the card-off render:

| frame row | full | card off |
|---|---|---|
| 717 | (245,213,203) | (255,255,255) |
| 729 | (217,168,156) | (255,255,255) |
| 741 | (254,231,222) | (255,255,255) |
| 753-777 | (250,228,222) … (248,224,215) | (194,33,115) |
| 789-813 | (246,220,210) … (243,206,195) | (255,255,255) |
| **825-849** | **(219,144,126)** | (219,144,126) |
| **861** | **(225,156,132)** | (225,156,132) |
| 873 | (248,217,214) | (248,217,214) |

With the card off the mouth region shows white background and a crimson (194,33,115): the face map is
**alpha-clipped open at the mouth** (`_MainTex` alpha × `_MaskTex` alpha, Mouth.shader:715-718 on the
card, `_TransCut/_ClipTreshold 0.5` on the shell), and the card is what closes that hole. Below row
819 the card is gone and the shell paints (219,144,126)/(225,156,132) — that dark strip is the band,
and it is **unchanged by the card**. So the band belongs to the shell; the card only ends where it
begins.

## 3. Live lighting in MahjongPlay (read from the loaded scene)

```
AMBIENT  mode=Flat intensity=1 sky=0.414,0.388,0.458 equator=0.114,0.125,0.133 ground=0.047,0.043,0.035
SKYBOX   sceneMat=NONE     LIGHTMAPS bakedMaps=0 lightProbeSet=NONE     fog=False
AMBPROBE at head: UNITY_LIGHTMODEL_AMBIENT=106,99,117 · _LightColor0=236,233,239
LITBY    charMask=256(Chara3DMyChara) lights=4:
  Spot Light        Spot        (0,0,0)      cull -1        (no intensity)
  Directional Light Dir         (236,233,239) cull 256       shadows None   <- seat light, rot 75,0,180 pos 0,0,-1.74
  SpotLightStage    Dir         (408,408,408) cull 32631     = white x1.6
  SpotLightAgari    Spot        (510,510,510) cull -1        = white x2
SCENELIGHT main=Directional Light toLight=0.000,0.966,-0.259 measured _LightColor0=236,233,239 match=1.000
```

`m_AmbientMode: 3` is `AmbientMode.Flat` (not tracked/probed), there are no lightmaps and no probe
group, so `UNITY_LIGHTMODEL_AMBIENT` at the head is 106,99,117 — a grey-lavender. The brightest two
lights (stage ×1.6 and agari ×2) are *not* the seat light, yet `_LightColor0` matches the seat light
exactly: Unity binds only the main *directional* light to `unity_LightColor`, so the shell's base is
lit by the seat light and the two brighter lights land in the **ForwardAdd** pass, which is
`Blend One One` (additive, no clamp) in every `Toon_DoubleShadeWithFeather_*.shader` forward delta
block. Colour space is Gamma, so the probe's read-back values are directly comparable with the
screenshot values.

## 4. The material bound on the live mouth renderer

`Assets/MFG/Characters/Character7/3D/Common/Mt_mouth.mat` → shader `Bsac/Character/Mouth`, queue 2450:
`_UtsLighting 1`, `_MouthOverlayMode 1`, `_MouthUVMode 1`, `_MouthIndex 0` serialized,
`_MouthMaskTex = mdl_mouth_msk`, `_MouthTex = mdl_mouth`, `_GI_Intensity 0`, `_Unlit_Intensity 1`,
`_ShadowTint (0.565,0.617,0.698)`, `_RimLightColor (1,1,1,1)`, `_ClipTreshold 0.5`.
CPU re-implementation of `MouthAtlasUV` + `mouthWeight` over the 486 card triangles: 14 triangles carry
mouth ink above 0.04 (2.9% of the card, mean weight 0.019) — the animated mouth overlay paints a small
part of the plate; the rest is the face-map copy.

Two knock-on facts that changed the earlier reading of this file:

* All 22 mouth materials on our Mouth shader already serialize `_UtsLighting 1`, so the card never
  takes the `UNITY_LIGHTMODEL_AMBIENT * 2` branch (Mouth.shader:635-636). Only 9 eye materials lack
  it. The ambient-doubled ramp described in an earlier pass of this document does not apply to the
  live mouth card. The 16 builtin `Diffuse` card materials exist only in motion/cut-in prefabs
  (`Character1 …/Motion/Toho_Hum`, `Character2 C011/Fbxs`, `Character6 C001/C006/C007`,
  `Character8 C006`).
* `UtsLightColor()` (Mouth.shader:407-412) collapses to `_LightColor0` clamped, with
  `defaultLightColor` from `ShadeSH9` × `_Unlit_Intensity`. `_1st_ShadeColor_Step` and
  `_1st_ShadeColor_Feather` are **not shader properties**, so `mat.GetFloat` on them yields 0 →
  `shadeRamp = _BaseColor` → the card's own ramp is inert and its paint is `map(uv1) × light`
  (`auxUV = TRANSFORM_TEX(uv1, _MainTex)`, Mouth.shader:566, sampled at 640). `_HeightTex` is unbound
  with a `"black"` default, so the view-parallax term is inert too.

## 5. Two different ramps in one face, measured as directions

The shell's ramp light is authored, not the scene light: `_Is_BLD 1` with `_Offset_X_Axis_BLD -0.48`,
`_Offset_Y_Axis_BLD 0`, `_Inverse_Z_Axis_BLD 0` gives the object-space vector
`(-4.8, 0, +1)` scaled by 0.1 (cginc:283-285). Reproduced on the CPU through the renderer chain
`model_face < face_group < toitoi_01_model < Chara < PlayerZi` (rot 0,180,0):

```
XFM fakeLightWorld=0.979,0.000,-0.204   sceneLightWorld=0.000,0.966,-0.259   dotFakeScene=0.053
XFM(model_face_mouth) fakeLightWorld=0.000,0.966,-0.259  dotFakeScene=1.000   (Mouth has no _Is_BLD)
```

The shell shades as if lit from the character's right side — nearly perpendicular (dot 0.05) to the
light that actually reaches it from above-front. The card shades against the real scene light
(dot 1.000). With `_Use_BaseAs1st 1` and `_BaseColor`/`_1st_ShadeColor` both white (cginc:301-318) the
base/1st lerp is a colour no-op on the shell, so its only angle-dependent term left is the rim
(`_RimLight 1`, `_RimLight_Power 0.5` → exponent 2.828, `_RimLight_InsideMask 0.5` → needs
`dot(N,V) ≲ 0.217`, i.e. silhouette-only), and the additive stage/agari lights in `Blend One One`.

Card multiplier at pose 0,0 (gated to pixels the card actually paints, 228 samples):

```
corr(mulLum, dotScene) = -0.547   corr(mulLum, dotView) = +0.695
texel (231,176,160) -> painted (244,218,211)  multiplier 1.228   (9 of 65 differ by >90)
texel (246,217,206) -> painted (248,224,216)  multiplier 1.033
texel (249,226,215) -> painted (247,219,212)  multiplier 0.977
```

Where the map is already bright the card is a copy; where the map is dark and saturated (the mouth
shadow texels) the card lifts it by up to +42 G / +51 B toward pale skin. That is the wash mechanism on
the mouth plate itself — a lift of the map's own shading, strongest where the plate turns away from the
scene light.

## 6. Measurement integrity

* `Texture2D.GetPixels32()` row 0 is the **bottom** row; the first sampler generation flipped it
  (`RTH-1-y`) and sampled mirrored positions. Fixed at all sample sites and at `PixelIndex`.
* Ground truth now checked twice: `MASKBOX` scans the raw diff between the render and the
  renderer-off render (`card diffPixels=12559 rows=380-482 cols=352-547` in buffer order) against the
  projected triangle box (`projectedVertexRows=436-514 cols=407-493`), and an independent Python diff of
  the written PNGs (`C:\mfg\tmp\probe_png_orient.py`: nocard rows 717-819 cols 352-547, noshell rows
  718-855 cols 415-484) reproduces the same boxes in display order. Claims from the flipped generation
  are withdrawn: "the card changes nothing on this character" and the chin-brighter-than-cheek ordering
  both came from mirrored samples.
* The probe never saves a scene (`NewScene` after each scene, no `MarkSceneDirty`) and material flips
  live on per-renderer instances that die with the throwaway instance. The renders are Editor-side
  evidence only: per `webgl-shader-variant-gotchas`, an Editor offscreen render is inert to keyword and
  variant effects and must not be used as proof about the WebGL player.
* The `Home` and `FirstCharaSelect` probe runs are void: the throwaway instance lands on the `Default`
  layer, `LITBY charMask=1 lights=0`, `_LightColor0=0,0,0`, and the face renders near-black
  (`CARDPIX mean=(12,8,30)`, `CONTRIB` 0 visible pixels for face/eyes/mouth). Those scenes need their
  real character layer and camera before any number from them is trusted. Each scene does ship
  `Assets/MFG/Scenes/<Scene>/<Scene>/LightingData.asset` (124 of them), so Home's light information is
  likely baked rather than realtime.

## 7. Open items

1. Attribute the shell's visible strip properly: `MULTIPLIER` currently samples triangle centroids, and
   the shell's 2423 visible pixels are slivers that no centroid lands on (`shellSamples=0` while
   `PIXDIFF=2423`). The coverage sampler (walk the diff mask, ray-test each pixel onto a baked skinned
   triangle) is written and needs its self-test to pass before its numbers are quoted.
2. `FirstCharaSelect` framing for the throwaway instance (layer + camera) so the select scene can be
   measured against the folder-7 reference capture directly.
3. Runtime mouth state: serialized `_MouthIndex 0` vs live index 12 (`Face_Smile.anim`); and the
   card's live-only offset — the painted card lower edge vs the shell band start (~30-56 px depending
   on pose and frame scale).
4. Unbound masks: what `_Set_RimLightMask`, `_Set_1st_ShadePosition`, `_Set_HighColorMask` sample as in
   this build (white default vs black) decides whether the rim term contributes at all.
5. Owner-side verdicts still open: the deployed mouth-bias removal (stamp F80A81F86F35), the 14 disabled
   `Mt_outline_facepart` mouth-outline renderers, the fitted −0.13 v-shift, outline sign-off, stutter
   retest.
