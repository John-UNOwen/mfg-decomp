# 33. The grey band on the bottom of the face (research only)

Owner report: when a character's head is slanted, the bottom of the face carries a light grey
shade. Investigated from the screenshot only — no shipped file was changed for this pass.
Screenshot attachment `sha256:0987a76e…f10e`, 814x1046, MahjongPlay seat view.

## 1. What is on those pixels (measured)

Cell grid over the lower face (17x15 px cells, mean RGB):

| region (preview px) | measured |
|---|---|
| lit cheek, x 495-540 / y 440-490 | (232,216,200) mode `#e8d8c8`, plus `#e8c8b8` (232,200,184) |
| brightest skin (under-lip / cheek rim), x 390-480 / y 470-495 | (248,232,216) `#f8e8d8` |
| chin band, x 379-492 / y 499-536 | mode `#d8a888` (216,168,136) 30% of the box, mean (221,186,158) |
| rows through the chin, x 415-455 | y 505-520: (216,173,146)…(223,194,173) |
| below the chin, y 550-575 | (184,184,200) `#b8b8c8`, (177,178,201) `#b1b2c9` — collar/neck cloth, not skin |

So the disputed band sits on the chin under the mouth, spans cheek to cheek, and its lower edge
crosses the jaw line. It is darker and duller than the cheek next to it: (216,168,136) against
(232,216,200) is 7% down in R, 22% down in G, 32% down in B.

## 2. Two different skin sources in one face

The face is drawn by two systems that light the same face map differently.

**Face shell** (`Mt_face` → `UnityChanToonShader/Toon_DoubleShadeWithFeather_TransClipping_StencilMask`,
guid `eeb96954b951f114896f527593eb2614`, 432 materials, plus our restored
`Assets/Toon/Shader/UCTS_DoubleShadeWithFeather.cginc`). With the shipped knobs
(`_Use_BaseAs1st 1`, `_Use_1stAs2nd 0`, `_ShadeColor_Step 0`, `_1st2nd_Shades_Feather 0.0001`,
`_1st_ShadeColor`/`_2nd_ShadeColor`/`_BaseColor` all white, `_Is_LightColor_Base =
_Is_LightColor_1st_Shade = 1`) the whole base/1st/2nd composition collapses:
`Set_1st_ShadeColor == Set_BaseColor` (cginc:302,305) and the 1st/2nd ramp at
`_ShadeColor_Step 0` evaluates to the 1st colour (cginc:318). **The shell therefore paints
`mdl_face × saturate(_LightColor0)` everywhere — it has no toon shade at all.**
Prediction with the seat light (0.9254902, 0.9137255, 0.9372549) × 1: (232,213,203). Measured lit
cheek (232,216,200). That is the shell.

The shell's only angle-dependent term is the rim light (`_RimLight 1`, `_RimLight_FeatherOff 1`,
`_LightDirection_MaskOn 1`, `_Tweak_LightDirection_MaskLevel 0.5`, `_RimLight_InsideMask 0.5`) plus
the ForwardDelta passes from the stage spots. Its "light direction" is authored, not the scene
light: `_Is_BLD 1` with `_Offset_X_Axis_BLD -0.48`, `_Offset_Y_Axis_BLD 0`, `_Inverse_Z_Axis_BLD 0`
resolves to object-space `(-0.98, 0, -0.20)` (cginc:280-287). Rim brightening explains the
near-white (248,232,216) skin, not the band.

**Cards** (`Mt_mouth` → `Assets/Shaders/Character/Mouth.shader`, `Mt_eye` → `Eye.shader`). Both take
their light term from:

```
float3 light = LightTint(UNITY_LIGHTMODEL_AMBIENT * 2.0, float4(0,0,0,1));   // Mouth.shader:635
if (_UtsLighting > 0.5) light = UtsLightColor();                             // Mouth.shader:636
```

`_UtsLighting` is serialized by **1 of 28** `Mt_mouth*.mat` files (only
`Character1/3D/Common/Mt_mouth.mat`) and by **0 of 63** `Mt_eye*.mat` files. Everywhere else it
takes the shader default 0 (Mouth.shader:84, Eye.shader:84), so the card is lit by the scene
ambient doubled. MahjongPlay has `m_AmbientMode: 3` with `m_SkyboxMaterial {fileID: 0}`, i.e. the SH
comes from `m_AmbientSkyColor (0.414,0.388,0.458)` / equator (0.114,0.125,0.133) / ground
(0.047,0.043,0.035); the sky term doubled is (0.828,0.776,0.916) — a grey-lavender.

Applying both terms to the same face-map texels the card's uv1 rect covers
(x 0.357-0.644, y 0.449-0.649):

| strip (card uv1) | map mean | shell: map × light | card: map × ambient×2 | delta |
|---|---|---|---|---|
| C1 chin strip v 0.449-0.505 | (248,217,205) | (229,198,192) | (205,168,188) | (-24,-29,-4), 13% less luminance |
| C1 mouth strip v 0.505-0.565 | (249,217,205) | (230,198,192) | (206,168,188) | same |
| C9 chin strip | (253,223,206) | (234,204,193) | (210,173,189) | same |

That difference — cooler, dimmer, unlit-looking skin — is what reads as "lightly grey shaded".

It is not a thin seam either. The mouth artwork only covers 3-13% of each atlas cell (measured
`maskR > 200` share per cell on `mdl_mouth_msk.png` for C1 and C9; `artAlpha > 250` is 100%), and
the composite is mask-gated (`mouthWeight = saturate(mmask.r * art.a)`, Mouth.shader:615). About
nine tenths of the card footprint paints `litBase` — face-map skin with the wrong light term —
over the whole mouth-and-chin area.

## 3. Why the slant turns it on

- The card is cheek-to-cheek and sits ~1.6 cm proud of the shell (authored card protrusion,
  `MouthDepthProbe` round). The shell is curved; the card is nearly flat. Tilting the head slides
  the card's footprint across the features and exposes more of its non-ink area on the underside.
- The card's base is lit by ambient, so it **never darkens or brightens with the light**: it stays a
  constant-value patch while the shell around it changes with the pose, the rim light and the stage
  spot deltas. Contrast, not the patch itself, is what changes with the angle.
- The live card projects ~31 px lower than the original reference (open item from the face-match
  rounds, with the fitted −0.13 v-shift still standing in for the authored mapping), which pushes
  the mismatched base below the mouth onto the chin and past the jaw line.

## 4. Ruled out, with the evidence

1. **Face toon shade.** Algebra above: with these knobs the shade blend is a no-op; the shell
   cannot draw a band. The grey also cannot come from `_1st_ShadeColor`, which is white on the face
   (hair/hands/cloth use coloured 1st shades — `Mt_hair._1st_ShadeColor` (0.6132,0.4368,0.5240) —
   which is why only the face reads grey).
2. **`_FaceShadowColor` grey 0.502.** It is real in the shipped programs — `col = lerp(col,
   col·_FaceShadowColor, firstShadeMask)` at `mouth_asm08_ann.txt:309-310` (cb slot 40,
   `cb_layout_mouth.txt:48`) and the same multiply for the eye composite at
   `eye_asm08_ann.txt:333-334`. But its gate is the feathered 1st-shade ramp built from
   `_ShadingGradeMap` × half-Lambert (asm 206-224), `_1st_ShadeColor_Step = 0` with feather 0.01
   collapses the ramp, and `_ShadingGradeMap` is not bound anywhere in the rip (no slot in
   `Mt_mouth.m_TexEnvs`, and Mouth.shader never samples it). Dormant on both sides.
3. **Cheek blush overlay.** `MFG_FACE_BLUSH` (cginc:478-506, declared in the face shader at 217,260)
   only runs when `_UseCheekBlush > 0.5`; no material serializes it and the only code that sets it
   is the `Assets/Editor/FaceAmbientSweep.cs` probe.
4. **Material-mask zone replacement.** `_MaterialMaskTex` is bound (guid `3f413814…`), the G-zone
   replace colours are black (`_GReplaceColor11/12 0,0,0` → `ZoneReplace` weight 0,
   Mouth.shader:498-509), so the lavender `_BReplaceColor*` set is not what paints the chin.
5. **Cast shadows.** The four per-seat `Directional Light` objects have `m_Shadows.m_Type: 0`, and
   `Mt_face._Set_SystemShadowsToBase 0` for C9 Common, so nothing is shadow-mapped onto the face.
6. **`model_face_front` / the shell aperture.** That object exists only as `m_IsActive: 0` in the
   C1/C2/C5/C8 C001 Fbxs; nothing moves the card at runtime (FaceController pushes only
   `_MouthIndex`, `_MouthOffsetX/Y`, `_GBlend`).

## 5. Original-side comparison

`C:\mfgtmp\ref_original_charselect.png` (shipped game, 1778x1026, 3D model face at x 268-405 /
y 355-505): lit cheek ≈ (255,226,206); under-jaw/chin shading ≈ (221,187,169), a deeper band
trending to (195,158,138). All warm (R−B ≈ 52), soft-edged, following the chin contour — no neutral
grey band, no straight edge. The shipped mouth cbuffer carries `_LightColor0` at slot 2 and gates
its SH ambient with `_GI_Intensity` (slot 85), which these materials set to **0**: the original
never lights the card from ambient, and its card base uses the same main light as the shell.
`_UtsLighting`, `_MouthOverlayMode`, `_MouthUVMode` and `_FaceLightNormalEditRange` do not appear in
`cb_layout_mouth.txt` at all — they are reconstruction knobs, and 27 of 28 mouth materials leave them
at the defaults.

## 6. Open residuals

- The measured chin value (216,168,136) is **warmer** than the pure ambient prediction
  (211,181,198), so a warm term is mixing in that area: the interior-AO multiply
  `col = lerp(col, col * _RShadowColor(0.5,0.4006,0.3494), mask.r)` (Mouth.shader:604) which is not
  gated by `mouthWeight`, the atlas cell background itself (measured mean (229,171,135) for every
  cell), or the ForwardDelta spot light landing on the shell around the card. Closing the colour
  budget needs the live `UNITY_LIGHTMODEL_AMBIENT` value, which only an in-game probe can give.
- The live character/costume in the screenshot is not pinned to a `CharacterN` folder, so which of
  the 28 `Mt_mouth` files is bound on that renderer is unconfirmed. The conclusion holds for 27 of
  them.
- Whether the live mesh needs `_MouthUVMode 1` (the authored mapping, used only by C1 Common) or the
  legacy mode 0 default is untested; mode 0 samples a different sub-rect of the atlas cell
  (`uv.y*0.25 + cellOrigin`, Mouth.shader:467-476), which can change how much flat cell background
  lands on the chin.

## 7. What would settle it

1. Editor-only probe (same shape as `Assets/Editor/MouthDepthProbe.cs`): dump the live
   `UNITY_LIGHTMODEL_AMBIENT` at the MahjongPlay camera, the material actually bound on the live
   mouth renderer with its `_UtsLighting`/`_MouthUVMode`/`_MouthOverlayMode`, and the projected
   card-vs-shell boundary for that exact pose, so the band's edge can be matched to the card's
   silhouette rather than inferred.
2. One-knob A/B capture: `_UtsLighting 1` on that character's `Mt_mouth`. Predicted effect: the card
   base moves from (211,181,198) to (232,213,203), i.e. the same skin as the shell, and the grey cast
   goes away. Revert if it re-opens the mouth-line work.
3. If the band survives, measure how far the card's lower edge actually sits below the authored
   position in that pose (the +31 px item) before touching any shift.
