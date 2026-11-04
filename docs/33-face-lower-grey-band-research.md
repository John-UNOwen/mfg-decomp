# 33. The shade band on the bottom of the face (research only)

Owner report: when a character's head is slanted, the bottom of the face carries a light grey shade.
Owner follow-up: it shows in every view, and the character is **トイトイ / Toitoi (Character7)**.
Reference captures for that character: `C:\mfg\ref\vncviewer_vcAe76NLpo.png` (440x418 bust) and
`vncviewer_3EQLRU24PD.png` (1920x1080), folder 7 of `C:\mfg\ref\MANIFEST.md`.
Nothing shipped was changed for this pass. `Assets/Editor/FaceLightProbe.cs` and
`Assets/Editor/MfgAmbientProbe.shader` (Editor-only, never packed, no scene saved, material flips on
throwaway instances only) produced everything below. The earlier version of this document is superseded:
its sampler read mirrored rows and a local-space mesh bake, and §10 lists what that invalidated.

## 1. The band is in the original too — ours is wider and higher

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

The original does have a warm saturated band (225,158,132, sat 0.41) on the jaw, bounded by pale skin
above (247,220,208) and pale neck below (249,219,214). Ours is the same colour family (211,162,131,
sat 0.38): the difference is extent, not invention. In the original the chin **centre** stays bright down
to row 1.1 and the band only appears **at and below the chin line**; ours floods the centre at 1.1 and
starts about one sample step higher. The matched-crop check (`C:\mfg\tmp\ref_vs_live_face.png`) agrees:
the original keeps cheek blush and a localized shadow at the chin line, ours is "lighter and flatter,
with a soft, streaky desaturated band across the lower cheek/jaw above the chin line".

## 2. Who paints those pixels (MahjongPlay, ScorePanel camera, C7/C001)

The probe instantiates the shipped `Assets/MFG/Characters/Character7/3D/C001/Prefabs/CharaModel.prefab`
into the real MahjongPlay scene, clones the `ScorePanel` camera (`fov 10`, head fills 0.268 of the
viewport height), renders 900x1200 offscreen, and switches one renderer off at a time. Counting the
pixels that change is what says who actually paints the frame:

| renderer | material / shader | pixels | frame rows | frame cols |
|---|---|---|---|---|
| `model_face_mouth` | `Mt_mouth` → `Bsac/Character/Mouth` | **12559** | 717-819 | 352-547 |
| `model_face` | `Mt_face` → `Toon_DoubleShadeWithFeather_TransClipping_StencilMask` | **2423** | 718-855 | 415-484 |
| `model_face_eye_L / _R` | `Mt_eye_*` → `Bsac/Character/Eye` | 4836 / 7946 | 622-742 | 345-557 |
| `model_hair_bangs / side / back / side2 / braid` | `Mt_hair` | 48823 / 42684 / 38458 / 6041 / 3762 | 416-880 | — |
| `model_cloth_hoodie` | `Mt_jacket` | 186182 | 674-1199 | 0-899 |
| `model_cloth_tie / coat1 / bodyneck / accessorie_logo / sticks / tshirt` | various | 50660 / 16712 / 8314 / 7207 / 3498 / 351 | — | — |
| outline renderers (`Mt_outline_*` → `sg_cha_outline`) | | 15 (face) … 643 | — | — |

Rows are top-down frame rows. The face is mostly **hair**: the bangs and side hair cover ~91k pixels of
the head while the shell shows 2423. A ray census over each diff mask (nearest skinned surface under
every changed pixel, `probeAllTris` = all 42 renderers baked) confirms the masks are not leakage:

```
CENSUS kind=shell maskPixels=2423 noSurface=0 nearest Shell:Mt_face=2157 Outline:Mt_outline_face=266
CENSUS kind=card  maskPixels=12559 noSurface=0 nearest Card:Mt_mouth=12559
```

Three renders side by side down one column (x=449, pose 0,0; x=480 agrees) settles attribution outright:

| frame row | full | card off | shell off | painted by |
|---|---|---|---|---|
| 700-710 | (205,156,57) / (246,217,214) | same | same | hair / neck |
| 720-810 | (239…254, 204…232, 195…222) | (255,255,255) or (194,33,115) | same as full | **mouth card** |
| **820-850** | **(219,144,126)** | (219,144,126) | (147,78,51) then (255,202,179) | **face shell** |
| 850-870 | (225,156,132) | same | same | neither (surface behind) |
| 870-900 | (248,217,214) | same | same | neither |

With the card off the mouth region shows flat white and a crimson (194,33,115): the face map is
alpha-clipped open at the mouth (`_TransCut/_ClipTreshold 0.5` on the shell, Mouth.shader:715-718 on the
card) and the card is what closes that hole. The band rows do not move when the card is removed and do
move when the shell is removed. **The band is the shell.** The card only ends where the band begins.

## 3. The band is the face map itself, multiplied by 1.0

The coverage sampler walks each diff pixel, ray-tests it onto a baked skinned triangle, then compares
the painted colour with the map texel that triangle samples (`mdl_face`, `_MainTex_ST = 1,1,0,0`; the
shell samples through `uv0`, `UCTS_DoubleShadeWithFeather.cginc:249,255`; the card samples its face-map
copy through `uv1`, Mouth.shader:566,640). Bins are over `dot(normal, axis)`; `mulLum` is painted vs
texel luminance; `suspect` counts samples differing by more than 90 in any channel.

Face shell, every pose, every bin:

```
pose 0,0    n=2423  obs=(219,144,126) texel=(220,146,126) mulLum 0.985-0.991  sat 0.41-0.43  suspect 0
pose -24,0  n=2122  obs=(219,144,126) texel=(220,146,126) mulLum 0.989-1.014  sat 0.42-0.43  suspect 0
corr(mulLum, dotFake) = -0.011   corr(mulLum, dotView) = -0.099   (flat with the rim off as well)
```

The band's colour is the authored `mdl_face` chin texels (220,146,126, sat 0.42) drawn essentially
unchanged. There is no ramp response, no rim contribution, no additive stage/agari contribution anywhere
in that strip: the multiplier is flat against the authored BLD direction (`fakeLightWorld 0.979,0,-0.204`,
`dotFakeScene 0.053`), against the seat light (`0,0.966,-0.259`, `_LightColor0 236,233,239`) and against
the view direction. With `_RimLight` forced to 0 the strip is unchanged except its grazing bin
(214,144,127 → 211,141,124). Consistent with the shell's shade algebra (`_Use_BaseAs1st 1`, white
`_BaseColor`/`_1st_ShadeColor`, cginc:301-318): the toon shade on this material is a colour no-op.

## 4. The card's rim light: real, confined to a fringe

Mouth card at pose 0,0, `axis=view`, baseline against the same render with `_RimLight` set to 0 on the
20 instance materials that carry it:

| `dot(N,V)` | pixels | baseline painted | baseline texel | mulLum | sat | suspect | rim off painted | mulLum | sat | suspect |
|---|---|---|---|---|---|---|---|---|---|---|
| 0.00-0.25 | 172→259 | **(252,241,237)** | (239,199,186) | **1.213** | **0.062** | **52** | (232,187,176) | 0.990 | 0.243 | 0 |
| 0.25-0.50 | 689→821 | (248,219,210) | (240,201,189) | 1.098 | 0.154 | 19 | (241,204,193) | 1.026 | 0.200 | 19 |
| 0.50-0.75 | 1390 | (247,220,211) | (244,214,202) | 1.043 | 0.145 | 14 | (242,211,202) | 1.004 | 0.168 | 14 |
| 0.75-1.00 | 10308 | (246,216,207) | (248,219,208) | 0.988 | 0.160 | 56 | (246,216,207) | 0.988 | 0.160 | 56 |

```
corr(mulLum, dotView) = -0.368 baseline  ->  -0.086 with the rim off
CARDPIX card mean (246,220,208) -> (242,211,197)   PIXDIFF card 12559 -> 12778 pixels
```

Facing the camera, the card is a faithful copy of the map (0.988). Where its surface turns away — the
plate's upper and lower edges — the multiplier climbs to 1.21 and saturation collapses 0.22 → 0.06:
exactly the pale, desaturated cast, and it disappears when `_RimLight` is turned off (suspect samples
52 → 0). `Mt_mouth` carries `_RimLight 1`, `_RimLightColor (1,1,1,1)`, `_RimLight_Power 0.5`
(exponent 2.828), `_RimLight_InsideMask 0.5`, and `_Set_RimLightMask` is not bound, so nothing masks the
rim back off the plate interior. The rim also changes the card's alpha coverage (+219 px).

Two knock-on facts that already changed the reading of this file:

* All 22 mouth materials on our Mouth shader serialize `_UtsLighting 1`, so the card never takes the
  `UNITY_LIGHTMODEL_AMBIENT * 2` branch (Mouth.shader:635-636); only 9 eye materials lack it. The
  ambient-doubled ramp described in an earlier pass of this document does not apply to the live card.
  The 16 builtin `Diffuse` card materials exist only in motion/cut-in prefabs.
* `_1st_ShadeColor_Step` and `_1st_ShadeColor_Feather` are not shader properties on Mouth.shader, so
  `mat.GetFloat` on them yields 0 → `shadeRamp = _BaseColor` → the card's own ramp is inert.

### How big the rim actually is

The table above compares bins, not the frame. Switching off only the mouth card's rim
(`MFG_FL_RIM=off_card`, `RIM mode=off_card _RimLight set to 0 on 1 instance materials`) and diffing the two
full renders: **883 changed pixels out of 1 080 000**, box rows 723-819 cols 352-547, with the busiest
columns at the plate's outer extremities (352-359 and 540-547) and rows 767-800. Those pixels go from
(254,241,235) sat 0.072 with the rim to (232,190,178) sat 0.234 without it. Card mean
`CARDPIX (246,220,208) → (242,211,197)`.

So the rim is a thin achromatic fringe on the plate's lower and outer boundary — a shallow arc along the
lower jaw line — not a wash across the plate and not the chin band. The mouth artwork and the chin read the
same with the rim on or off. It is still worth removing on principle: the shipped mouth cbuffer has no rim
stage at all (`cb_layout_mouth.txt`), so this highlight is ours, not the original's. But it is not the lever
that closes the gap in §1.

Against the folder-7 capture on the same normalized eye-to-chin scale (`tmp/rim_vs_ref.py`): the original
turns saturated at t 1.2 (225,158,132 sat 0.41) and is pale neck by t 1.4 (249,219,214); our Editor render
is saturated at 1.2 (219,144,126 sat 0.42) **and still at 1.4** (225,156,132 sat 0.41), only pale by 1.6.
The colour of the band matches; its extent does not — ours runs about 0.2 normalized units further down.

## 5. Scene lighting, for the record

```
AMBIENT  mode=Flat (m_AmbientMode 3) intensity=1 sky=0.414,0.388,0.458 equator=0.114,0.125,0.133
         ground=0.047,0.043,0.035      SKYBOX sceneMat=NONE   LIGHTMAPS bakedMaps=0 probes=NONE
AMBPROBE at head: UNITY_LIGHTMODEL_AMBIENT=106,99,117  _LightColor0=236,233,239
LITBY    charMask=256 (Chara3DMyChara) lights=4:
  Spot Light       Spot        (0,0,0)       cull -1
  Directional Light Directional (236,233,239) cull 256  shadows None   <- seat light, rot 75,0,180
  SpotLightStage   Directional (408,408,408) cull 32631  = white x1.6
  SpotLightAgari   Spot        (510,510,510) cull -1     = white x2
SCENELIGHT main=Directional Light toLight=0.000,0.966,-0.259 measured _LightColor0=236,233,239 match=1.000
```

`_LightColor0` is the *dimmest* directional light in the scene: the stage and agari lights land only in
the ForwardAdd pass, which is `Blend One One` in every `Toon_DoubleShadeWithFeather_*.shader`. Colour
space is Gamma, so probe read-back is directly comparable with the screenshot values. §3 shows none of
this reaches the band.

## 6. The band grows with the slant because coverage changes

| pose pitch,yaw | card rows | card cols | card px | shell rows | shell cols | shell px |
|---|---|---|---|---|---|---|
| 0,0 | 717-819 | 352-547 | 12559 | 718-855 | 415-484 | 2423 |
| 0,-15 | 691-794 | 353-546 | 12838 | 768-855 | 368-531 | 3345 |
| -24,0 | 715-817 | 334-504 | 11191 | **574-855** | 415-499 | 2122 |
| -24,-15 | 687-795 | 335-503 | 11610 | **545-855** | 415-502 | 2713 |

The card's footprint barely moves with the head; the shell's exposed area does. Tipping the head down
24° starts the exposed shell 144 rows higher (718 → 574), so the authored chin texels show across a much
taller region above and below the plate. That is the pose dependence the owner sees, and it is coverage,
not lighting: the multiplier stays 0.989 in every pose.

Card vs shell depth, cast from each card triangle centre along the view ray:

```
pose 0,0    shellBehindCardTris=440 noShellBehind=46 cardFacesAway=56  shellBehindCard_cm min=1.88 mean=12.01 max=19.69
pose -24,0  shellBehindCardTris=412 noShellBehind=74 cardFacesAway=44  shellBehindCard_cm min=0.36 mean=11.23 max=17.28
```

The minimum is the meaningful number (the card sits ~2 cm proud of the skin under it); the mean and max
are rays that leave the mouth aperture and hit shell triangles on the far side of the head.

## 7. Live vs Editor

The Editor render gives the band as (219,144,126) sat 0.42, i.e. the map texel. The live lane gives
(211,162,131) sat 0.38 — the same place, slightly desaturated (G +16, B +5 over the texel). That last
step is a player-side effect the probe cannot see: per `webgl-shader-variant-gotchas`, an Editor
offscreen render is inert to keyword and variant effects, so these numbers describe the reconstruction's
data and materials, not proof about the WebGL frame.

## 8. The mouth mesh does carry a zero-uv backing piece — owner's report, verified

Report from the owner (mesh and shader data only, nothing changed or built): every mouth mesh holds a
hidden second piece behind the front mouth piece, larger, spanning cheek to cheek under the mouth, whose
texture coordinates all point at one blank spot on the mouth sheet, so it never shows mouth art and only
ever paints face skin; Character1 has 172 of them out of 486, always stored after the front piece, and
they appear in characters 1, 3, 5-16 but not 2 and 4, whose mouth UVs were remapped.

Measured on the meshes this build actually uses (`MESHUV` scan over characters 1-16, probe `eff228e27d`):

| Char | mouth mesh | tris | flat-uv0 tris | shared uv | triangle range |
|---|---|---|---|---|---|
| 1 | model_face_mouth | 486 | 172 | (0,0) | 314-485 |
| 2 | model_face_mouth | 446 | 0 | uv box starts 0.008,0.816 | - |
| 3 | model_face_mouth | 474 | 144 | (0,0) | 330-473 |
| 4 | model_face_mouth | 492 | 0 | uv box starts 0.012,0.846 | - |
| 5 | model_face_mouth | 486 | 124 | (0,0) | 362-485 |
| 6 | model_face_mouth | 484 | 152 | (0,0) | 332-483 |
| 7 | model_face_mouth | 486 | 136 | (0,0) | 350-485 |
| 8 | model_face_mouth | 486 | 152 | (0,0) | 334-485 |
| 9 | model_face_mouth | 486 | 172 | (0,0) | 314-485 |
| 10 | model_face_mouth | 484 | 160 | (0,0) | 324-483 |
| 11 | model_face_mouth | 486 | 290 | (0,0) | 196-485 |
| 12 | face_mouth | 486 | 290 | (0,0) | 196-485 |
| 13 | model_face_mouth | 486 | 172 | (0,0) | 314-485 |
| 14 | mdl_face_mouth | 486 | 136 | (0,0) | 350-485 |
| 15 | mdl_mouth | 486 | 136 | (0,0) | 350-485 |
| 16 | model_mouth | 486 | 136 | (0,0) | 350-485 |

Every count, the tail position, and the 2-and-4 exception are exactly as reported. Details that matter for
any fix:

* Each flagged triangle has all three vertices at `uv0 = (0,0)`, and they are the tail of a **single
  submesh with a single material** (`subMeshes=1 mats=1`) — so the piece cannot be switched off per
  renderer; a shader-side test is the only lever, as the owner says.
* `uv0` is the atlas channel; `uv1` (the aux channel the Mouth shader samples at Mouth.shader:566) is
  **live** on these triangles. That is why a first pass of mine that tested both channels for zero found
  nothing. What the zero `uv0` kills is the art overlay: at the atlas corner `_MouthTex` is (230,172,136)
  — the sheet's own background — and `_MouthMaskTex` is (0,0,0), so `mouthWeight = maskR x artA = 0`
  (`ZEROTEX`). The piece paints real `mdl_face` skin through `uv1`.
* Its normals do lean away, as reported: mean `dot(normal, view)` 0.650 for the backing against 0.834 for
  the front piece (Character7), 0.715 against 0.838 (Character1).

## 9. What the backing piece is responsible for, and what it is not

Ray attribution over the pixels that change when the mouth renderer is switched off (`BACKING`, `MULGROUP`,
MahjongPlay, ScorePanel camera, pose 0,0):

| Char | card pixels | painted by backing | backing rows-cols | front piece rows-cols |
|---|---|---|---|---|
| 1 | 13984 | 5292 (38%) | 496-580, 346-553 | 474-559, 380-519 |
| 7 | 12559 | 2533 (20%) | 740-819, 352-547 | 717-810, 369-530 |

So the piece is on screen, it is wider and lower than the front piece, and on Character7 its lower edge
(row 819) is exactly where the shell's exposed chin strip begins (rows 718-855, §2). It is what makes the
painted plate reach cheek to cheek.

What it is **not** is a darker surface. Against the face map both pieces multiply at about 1:

* Character7: front `obs 247,218,209 sat 0.154 mulLum 0.992`, backing `obs 247,216,206 sat 0.164 mulLum
  1.048`, exposed shell `obs 219,144,126 sat 0.424 mulLum 0.989`.
* Character1: front `246,216,206 sat 0.163 mulLum 0.995`, backing `240,197,185 sat 0.228 mulLum 1.000`,
  exposed shell `212,123,102 sat 0.518 mulLum 0.994`.

The colour step that reads as an oval is **card against shell** (saturated chin texels against pale ones),
not the mouth program lighting the backing differently — consistent with §3 and §4, where every axis
correlates with the multiplier at about zero. On Character1 the backing is indeed a shade darker than the
front piece, but it is still far brighter than the shell behind it, so that shade is not what forms the band.

The depth half of the report is half right, and the difference is ours. The shipped Mouth forward pass
writes depth; ours is `ZWrite Off` on purpose (Mouth.shader 334-341) because our restored Eye forward pass
tests `LEqual` where the shipped eye composite tests `Always`. With no depth write inside the card's own
draw call, the backing — stored after the front piece — paints over it wherever both are visible, which is
a real mechanism for flattening mouth art in **our** build and would not happen with the shipped pass
states. The census sees the overlap: `behind: ... Card:Mt_mouth=299` on Character7 and 208 on Character1.

Skipping the piece would not open a hole to the *background* — `nothingBehind=0` on both characters, and
what lies behind is already-drawn character geometry: Character7 `Shell:Mt_face=1220 Hair:Mt_hair=705
Outline:Mt_outline_face=309 Card:Mt_mouth=299`, Character1 `Shell:Mt_face=4696 Hair:Mt_hair=388
Card:Mt_mouth=208`. What that geometry actually looks like is a separate question, and rendering it answers
it.

### Removing the piece, measured

`MFG_FL_DROPBACK=1` (probe `3825252a18`) renders that fix candidate without touching a shader or an asset:
the throwaway instance gets a private copy of its mouth mesh with the 136 zero-uv0 triangles deleted
(`DROPBACK smr=model_face_mouth dropped=136 kept=350`, `ZEROUV tris=350 zeroUvTris=0`, card footprint
12559 → 10028 px, `BACKING onScreenPixels=0`). The chin strip is untouched; the 2510 pixels the piece used
to paint do not become skin:

| pose | the piece painted | what shows after removal |
|---|---|---|
| 0 | (247,216,206) sat 0.166 | (216,200,223) sat 0.105 — cooler than the skin it replaced, blue above red |
| −24 | (246,217,209) sat 0.151 | (167,118,151) sat 0.293 — dark crimson, hard edge under the mouth |

Those two rows are level-camera measurements. Re-run at the select-like 24° (`MFG_FL_ELEV=24`, report
`flp_report_dropback_e24.txt`), the same removal exposes far less and shows something different: 948 px at
pose 0 and 1274 px at pose −24 (against 2510 and 2174), reporting `obs 238,203,195 texel 221,148,129
mulLum 1.317 sat 0.182 dotView 0.106` and `obs 236,197,188 texel 220,146,126 mulLum 1.299 sat 0.205`, with
442 and 664 of those pixels chin-coloured. So at the angle the player actually sees, the hole left behind is a
small, over-lit pale patch — the unclamped `Blend One One` ForwardAdd lift, multiplier 1.3 — rather than the
crimson cavity the level camera produced. Still not the flat-lit chin skin it replaces (the texel is
(221,148,129) at saturation 0.42, what lands on screen is (238,203,195) at 0.18), and the chin-region mean in
that band moves to (247,219,210): paler than the card it removed.

The shell now reports `obs 234,189,179 texel 220,147,127 mulLum 1.248 sat 0.235 dotView 0.335` across 4278
exposed px against 2423 before removal: the surfaces revealed are edge-on ones carrying the unclamped
`Blend One One` ForwardAdd lift, not the flat-lit chin texels of §3. In `tmp/backing_ab.png` (scratch
render, not committed) the mouth still reads as a mouth at pose 0 with no hole, while at pose −24 a dark
crimson cavity with hard boundaries appears under and to the right of the mouth.

So skipping the zero-uv0 triangles trades the pale wash for a cool grey patch at level gaze and a visible
cavity when the head tips down. The piece is load-bearing, which is the risk the owner flagged.

The pattern is not mouth-only: `model_face_eye_L/R` carry 61 zero-uv0 triangles each out of 321. Any
shader-side test has to be scoped to the mouth program, not applied wherever uv is zero.

### What decides how far the card paints

Three candidates set that lower edge. Two of them are dead, measured.

**The mask does not cut coverage.** `Mt_mouth` ships `USE_MASK_TEX` enabled, and the forward pass ends with
`baseTexAlpha = tex2D(_MainTex, auxUV).a; alpha = tex2D(_MaskTex, auxUV).a * baseTexAlpha;` written into
`res.a` (Mouth.shader:715-721). There is no `clip()` in that pass — the only `clip()` calls in the file are
the dissolve ones (:577, :814, :898) — so a low-alpha fragment would blend what is behind it rather than
being cut away. It does not get low: `_MaskTex` on this character resolves to `msk_black`, an opaque 16×16
fill (`MASKTEX mat=Mt_mouth USE_MASK_TEX=1 maskTex=msk_black 16x16 readable=1 baseColorA=1`), and sampling
`_MaskTex.a(auxUV) × _MainTex.a(auxUV)` at every painted card pixel gives `alpha=1.000` on both pieces
(front 10026 px, backing 2533 px, `alphaUnder098=0`). Alpha is 1 across the whole footprint, so the painted
area *is* the triangle coverage: `PIXDIFF full_vs_nocard=12559` equals the ray-attributed 10026 + 2533.

**Nothing runtime moves it either.** `MOUTHSTATE smr=model_face_mouth mesh=model_face_mouth blendShapes=0
localPos=0.000,-0.079,0.014 localScale=1.000,1.000,1.000 anim=none controller=none`: the mouth mesh has no
blend shapes and no Animator reaches that node, so the only runtime mouth switch is which sheet of the atlas
is sampled (`_MouthIndex`), which changes the art and not the coverage. The "+31 px card offset" an earlier
pass kept as a runtime lever was traced to the registration tool and retracted — `docs/26-claude-face-match-rounds.md`
records "Defect B (the '+31 px mouth offset') does not exist".

**What is left is authored geometry**, the same shipped mesh both builds carry: front piece rows 717-810,
backing 740-819 at pose 0,0. The stencil relationship runs the other way: Mouth.shader:343-348 writes
`Ref [_StencilMask] … Comp Always Pass [_StencilMode]` (`_StencilMask 4`, `_StencilMode 2` = Replace) so the
mouth stamps a mask for `Mt_face` (`_StencilMask 4`, `_StencilNo 1`) to mask against; nothing gates the mouth.
`_ClippingMask` is unbound on `Mt_face` and on `Mt_mouth`, so the UTS clipping path is inactive on this face.

**Measured against folder 7 on anatomy, not frame rows.** With `t = (row − eye)/(chin − eye)` (original bust
eye 247 / chin 312 / mouth bottom 303; live crop eye 329 / chin 557 / mouth bottom 512; Editor render eye 674 /
chin 807 / mouth bottom 759), the shell's saturated chin texels show up in the face-centre column between
these multiples of that column's own mouth-to-eye distance:

| frame | chin texels exposed, measured down from the bottom of the mouth art | what the chin-line row looks like |
|---|---|---|
| folder-7 original bust | 0.39-0.45 | pale chin centre, saturated jaw patches left and right of it |
| live lane crop | 0.37-0.50 | the same: pale centre, two lateral saturated patches |
| Editor probe (ScorePanel camera, pose 0,0) | 0.72-1.19 | one continuous saturated band across the chin |

The two real select-screen frames agree within what the reference allows: that bust spans only 65 px from eye
to chin, so 2 px of landmark error is 0.03 of the scale, and the live band begins 0.37 against the original's
0.39 — a few pixels closer to the mouth, not a different placement. Only the probe frame disagrees, and by a
wide margin, because the ScorePanel camera sits level with the head and reads the jaw underside, where the
select camera looks down onto the chin. The row-by-row scratch maps (`tmp/coverage_rows.py`, scratch) say the
same thing: in the original the chin-class texels form two lateral runs with a pale centre, in the probe
render they form one run spanning the centre. So the footprints in §2 and §6 (shell 718-855 against card
717-819) describe the jaw underside at that camera, not what the select screen frames, and coverage is not
what separates our frame from the original's.

A scan for a boundary line (`tmp/boundary_line.py`, scratch; mean channel < 135, row by row from t 0.85 to
1.80) found none in either of our frames — 0 dark pixels in the live crop from t 0.90 to 1.50 — while the
original carries a handful of dark warm pixels just under the mouth corners (rows 312-322, (170,112,84),
1-2 px per run at cols 174-191 and 205-223). Nothing card-sized about them; they are where the 14 disabled
`Mt_outline_facepart` renderers would have been.

### The probe camera looks the wrong way, and `MFG_FL_ELEV` fixes it

The ScorePanel clone sits **level with the mouth card** (`SELFTEST camPos 0.000,1.370,-7.040`, card centre
0.000,1.242,0.042): it reads the jaw underside. The player's select view is level with the **face**, which is
above the card's height — reading the crop (`tmp/shot_crops.py`, source 814×1046 live-lane select capture,
Toy-Toy nameplate, head upright) says the head is upright and the view is not tilted down onto the chin. So
the elevation to look for is measured from the card centre upward, and the framing that matches the captures
lands near 24°, not at 0°. `MFG_FL_ELEV`(`probe d8318422b0`) puts the chosen camera — cloned or synthetic — at
that many degrees above the card centre on the face axis, keeping its distance, and `LANDMARK` / `MOUTHART`
report where the eyes and the inked mouth triangles land so a render and a capture can be lined up on the same
landmarks.

The chin texels respond to that angle far more than to anything else. Same pose, same lighting, same
materials, `MULGROUP kind=shell` painted pixels:

| camera elevation above the face axis | shell pixels painted | shell rows | card front rows | card painted px |
|---|---|---|---|---|
| 0° (level, every earlier number in this note) | 2548 | 549-689 | 549-643 | 10091 |
| 12° | 1397 | 637-671 | 555-643 | 9331 |
| 18° | 759 | 634-661 | 559-642 | 8795 |
| 24° | 171 | 635-650 | 563-641 | 8162 |
| 30° | **1** | 567 | 567-639 | 7396 |

At a select-like 24-30° the card covers the shell's chin texels almost completely; at the level angle the
same geometry leaves a wide strip of them in view. The earlier §2 and §6 numbers were measured at 0°.

Where the chin texels first show, per frame, measured down from that frame's own mouth-art bottom in units
of its own eye-to-mouth distance, at the face centre and ±0.15 / ±0.30 of the card's width
(`tmp/framing_match.py`, scratch):

| frame | −0.30 | −0.15 | centre | +0.15 | +0.30 |
|---|---|---|---|---|---|
| folder-7 original bust | 0.04 | 0.23 | 0.36 | 0.25 | 0.07 |
| live lane crop | 0.02 | 0.23 | 0.35 | 0.19 | 0.01 |
| render 0° | none | 0.52 | 0.40 | 0.58 | none |
| render 12° | 0.36 | 0.26 | 0.32 | 0.23 | 0.36 |
| render 18° | 0.23 | 0.18 | 0.31 | 0.18 | 0.23 |
| render 24° | 0.08 | 0.13 | 0.27 | 0.14 | 0.08 |
| render 30° | 0.01 | 0.10 | 0.25 | 0.11 | 0.01 |
| render 36° | 0.01 | 0.06 | 0.21 | 0.05 | 0.01 |

Two things fall out. The **live lane frame and the folder-7 original agree** on this signature almost
column for column (0.02/0.23/0.35/0.19/0.01 against 0.04/0.23/0.36/0.25/0.07): the shipped select screen and
our select screen frame the same chin in the same way. And the **level render is the odd one out** — it
shows no chin texels at the outer columns at all and its latest onset is in the middle, the reverse of both
captures. Raising the camera reproduces the captures' early lateral onset (24-30°) but not their late centre
onset: no elevation tried, with head pitches 0, −12 and −24, puts the centre onset as low as the captures'.
The centre-vs-side gradient our card produces is flatter than the shipped frame's, and that is the part still
unexplained.

Attribution, so this is not colour guesswork: `MULGROUP … chinColouredPixels=` counts samples whose own
observed colour falls in the chin band (saturation ≥ 0.28, luminance 120-200). At 18° the shell's 759 exposed
pixels are 759 chin-coloured; the card's front piece contributes 67 out of 8795 and the backing 0 out of 1956.
The chin band is drawn by the shell, not by the card sampling chin texels through `uv1`.

### At the select angle the Editor render reproduces the original; the live frame is the desaturated one

Sampling all four frames on one axis — row 0.0 = eye centre, 1.0 = that frame's own mouth-art bottom — at the
cheeks ±0.28 of the face width and at the centre, 5×5 averages (`tmp/select_profile.py`, scratch):

| row | original, cheek −0.28 | live select crop, cheek −0.28 | render 24°, cheek −0.28 | render 0°, cheek −0.28 |
|---|---|---|---|---|
| 0.80 | (247,218,207) 16 | (229,213,201) 12 | (246,213,205) 16 | (246,206,196) 20 |
| 0.95 | (247,223,213) 13 | (237,223,210) 11 | (251,236,228) 9 | (246,219,210) 14 |
| 1.10 | **(217,152,126) 41** | **(211,162,131) 37** | **(225,156,132) 41** | (245,212,204) 16 |
| 1.20 | **(225,158,132) 41** | **(211,162,131) 37** | **(225,156,132) 41** | (194,33,115) 82 |
| 1.35 | **(225,158,132) 41** | **(211,162,131) 37** | (229,168,148) 35 | (194,33,115) 82 |
| 1.50 | (249,219,214) 14 | (242,220,214) 11 | (242,209,195) 19 | (194,33,115) 82 |

Centre column, same axis: original 1.35 **(219,169,148) 32**; live 1.35 **(217,175,149) 31**; render 24° 1.35
**(225,156,132) 41**; render 0° 1.35 (233,195,182) 21, and its saturated jaw texel only appears at 1.50.

At 24° the Editor render lands on the original's band: **(225,156,132) sat 41** against the original's
**(225,158,132) sat 41**, on the same rows (1.10-1.20 at the cheeks, 1.35 at the centre). 18° puts it too
late (1.35 at the cheeks), 30-36° too early (0.95), and the 0° level camera never shows the jaw band at the
cheeks at all — at 1.20-1.35 that column is hair and collar ((33,85,181) sat 81 at the right cheek,
(194,33,115) at the left). Every §2 and §6 number in this note was taken at that 0° angle.

The live frame is the one that does not match: its band is **(211,162,131) sat 37** where both the original
and our own matched-framing Editor render give **(225,157,132) sat 41** — 14 units less red, 5 more blue,
saturation down 4 points; at the centre (217,175,149) 31 against (219,169,148) 32. The band's *rows* agree
with the original at these columns; its *colour* is washed toward grey. That is the same direction and about
the same size as the live-vs-Editor gap recorded in §7, now measured on the band itself rather than on
cheeks, and with the framing taken out of the way.

## 10. Measurement integrity, and what was withdrawn

* `Texture2D.GetPixels32()` row 0 is the **bottom** row. The first sampler generation flipped it
  (`RTH-1-y`) and sampled mirrored positions.
* `SkinnedMeshRenderer.BakeMesh` returns vertices in the renderer's **local** space. Proof: the old bake
  put the mouth card 7.085 m from a camera 7.040 m from the character's z. Multiplying the baked vertices
  by `localToWorldMatrix` fixes it, and the self-test now agrees: `hitDist 5.321 / centroidDist 5.321`
  (shell) and `5.257 / 5.257` (card), 37-40 hits of 40 projected probes.
* `Camera.ScreenToWorldPoint` maps through `pixelRect`, which for a clone rendering into a render texture
  is still the **640x480 editor screen**, not the 900x1200 target: `SELFTEST … pixelRect=640x480` with
  0 hits of 40. Rays are now built from the frustum (`PixelRay`) with `camera.aspect` pinned to the
  target aspect.
* `CARDDEPTH`'s `gap` subtracted the eye distance from a distance measured from the card, which printed
  `513 cm`. Now `shellBehindCard_cm`.
* Withdrawn with those bugs: "the card changes nothing on this character", the chin-brighter-than-cheek
  ordering, the 228-sample table `corr(mulLum,dotScene) -0.547 / corr(dotView) +0.695` with its
  `1.228 / 1.033 / 0.977` bins, and the ~2.3x card width mismatch between projection and paint (a
  combined row-flip and local-bake artifact; `CARDPIX rows 720-819` now matches the painted 717-819).
* The `Home` and `FirstCharaSelect` probe runs remain void: the throwaway instance lands on the `Default`
  layer, `LITBY charMask=1 lights=0`, `_LightColor0=0,0,0`, and the face renders near-black. Each scene
  does ship `Assets/MFG/Scenes/<Scene>/<Scene>/LightingData.asset` (124 of them), so those views likely
  carry baked rather than realtime light.
* A `CharaSelect` run (same layer problem, no seat to read the character layer off) renders nothing worth
  reading either: `CAMUSE synthetic dist=2.0 fov=10.0 charMask=1`, `PIXDIFF tag=baseline full_vs_nocard=0
  full_vs_noshell=0`, `cardSamples=0 shellSamples=0`. The face is not in that frame at all; treat its PNGs
  as void until the instance is placed on the scene's character layer and the scene's own camera is cloned.

## 11. Open items

1. Coverage is settled as authored geometry (see §9's last subsection): alpha 1 across the card footprint, no
   clip in the mouth forward pass, no blend shapes. With the framing taken out of the way, the band's rows at
   the sampled columns agree with the folder-7 bust; what separates our live frame from the original's is tone.
2. Framing is now a knob, not a guess. `MFG_FL_ELEV` tilts the cloned or synthetic camera to that elevation
   above the card centre on the face axis, and 24° reproduces the original's band colour and rows. Read §2 and
   §6 as **0° measurements**: the wide strip of exposed chin texels, the crimson cavity in the removal test and
   the chin-brighter-than-cheek ordering are all products of a camera level with the mouth card. The part
   elevation does not reproduce is the captures' centre-vs-side onset gradient (0.36 centre against 0.04 side),
   which no elevation from 0° to 36°, with head pitches 0, −12 and −24, matched — that points at where the card
   sits on the head, not at where the camera is.
3. The mouth pass depth pairing is the other live lever. Ours is `ZWrite Off` where the shipped pass writes
   depth, and that is what lets the tail-ordered backing flatten mouth art. Matching the shipped pairing
   (`ZWrite On` plus the eye composite testing `Always`) is untested here; it needs owner sign-off and a
   `-Full` rebuild, and the eye card overlap (mouth band 1.384-1.466 against eye band 1.444-1.573) is what
   the current pairing was chosen to avoid.
4. Whether the card's rim belongs there at all. `Mt_mouth` has `_RimLight 1` with a white rim colour and
   no `_Set_RimLightMask` bound; the original mouth program has no rim stage in its cbuffer layout
   (`cb_layout_mouth.txt`). Turning it off removes the wash in-editor; on the player it needs a rebuild. The
   §8 measurement of that fringe (883 px, rows 723-819) is a 0°-camera number and has not been re-run at 24°.
5. What the unbound `_Set_RimLightMask`, `_Set_1st_ShadePosition`, `_Set_HighColorMask` sample as in this
   build (white default or black), which decides how far the rim reaches into the plate interior.
6. The live band is ~16 units desaturated relative to the Editor render (§7), and at matched 24° framing the
   gap shows up on the band itself: live (211,162,131) sat 37 against (225,156,132) sat 41 in the render and
   (225,158,132) sat 41 in the original. Against the original the live card skin reads (240,219,212) sat 0.117
   versus (247,219,212) sat 0.142. Only an in-browser frame-diff can say what adds it; no browser harness for
   that exists in the repo yet.
7. Runtime mouth state, narrowed: the mouth mesh has no blend shapes and no Animator reaches it, so the only
   runtime switch left is the art sheet (`_MouthIndex`: serialized 0 vs live 12, `Face_Smile.anim`). The
   "+31 px card offset" is not a lever; it was a registration-tool artifact.
8. Owner-side verdicts still open: the deployed mouth-bias removal (stamp F80A81F86F35), the 14 disabled
   `Mt_outline_facepart` mouth-outline renderers, the fitted −0.13 v-shift, outline sign-off, stutter
   retest.
9. The artefact does not need the mouth animation. The owner's live capture of Character 4 (Amato Tenshi) shows the
   same hard-edged darker area under a closed mouth. On that character, at the 24° framing with the serialized
   `_MouthIndex 0`, the full and shell-dropped renders are identical at every sampled pixel in the mouth-card window,
   so the mouth card alone paints it: `MULGROUP kind=card … mulLum=0.996 chinColouredPixels=1165` against
   `kind=shell … mulLum=1.183`. `ARTPIX` at its 22 inked art triangles reads `obs=233,187,174` against
   `artTexel=204,138,121` and `faceMapTexel=251,217,202` at `mouthWeight=0.648`. `_MouthOverlayMode 0` moves those
   pixels 22 units; `_RShadowColor.a 0` and `_FaceShadowColor (1,1,1,1)` move nothing. The frame comparison and the
   full numbers are in docs/27 §"Measured 2026-11-05".
