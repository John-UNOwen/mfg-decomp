# 33. The shade band on the bottom of the face (research only)

Owner report: when a character's head is slanted, the bottom of the face carries a light grey shade.
Owner follow-up: it shows in every view, and the character is **トイトイ / Toitoi (Character7)**.
Reference captures for that character: `C:\mfg\ref\vncviewer_vcAe76NLpo.png` (440x418 bust) and
`vncviewer_3EQLRU24PD.png` (1920x1080), folder 7 of `C:\mfg\ref\MANIFEST.md`.
Nothing shipped was changed for this pass. `Assets/Editor/FaceLightProbe.cs` and
`Assets/Editor/MfgAmbientProbe.shader` (Editor-only, never packed, no scene saved, material flips on
throwaway instances only) produced everything below. The earlier version of this document is superseded:
its sampler read mirrored rows and a local-space mesh bake, and §8 lists what that invalidated.

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

## 4. The washed skin on the plate is the card's own rim light

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

## 8. Measurement integrity, and what was withdrawn

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

## 9. Open items

1. Coverage, not light, is where the difference lives: the original keeps the chin centre pale down to
   row 1.1 and puts its shade at/below the chin line, while our card's painted lower edge (row 819 at
   pose 0,0) leaves the shell's authored chin texels exposed across the chin. What sets that edge — the
   card's authored mesh and `uv1`, the `_MaskTex`/map alpha clip (Mouth.shader:715-718), or the runtime
   card position — is the next thing to measure, against the folder-7 capture rather than against our own
   frame.
2. Whether the card's rim belongs there at all. `Mt_mouth` has `_RimLight 1` with a white rim colour and
   no `_Set_RimLightMask` bound; the original mouth program has no rim stage in its cbuffer layout
   (`cb_layout_mouth.txt`). Turning it off removes the wash in-editor; on the player it needs a rebuild.
3. What the unbound `_Set_RimLightMask`, `_Set_1st_ShadePosition`, `_Set_HighColorMask` sample as in this
   build (white default or black), which decides how far the rim reaches into the plate interior.
4. The live band is ~16 units desaturated relative to the Editor render (§7); only an in-browser
   frame-diff can say what adds it.
5. Runtime mouth state: serialized `_MouthIndex 0` vs live index 12 (`Face_Smile.anim`), and the
   live-only card offset.
6. Owner-side verdicts still open: the deployed mouth-bias removal (stamp F80A81F86F35), the 14 disabled
   `Mt_outline_facepart` mouth-outline renderers, the fitted −0.13 v-shift, outline sign-off, stutter
   retest.
