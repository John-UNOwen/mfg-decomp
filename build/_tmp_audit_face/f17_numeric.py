import math
# Exact evaluation of the compiled FORWARD frag formula (asm29) for Mt_face.mat values.
# Material: _BaseColor=(1,1,1,1) _Use_BaseAs1st=1 _1st_ShadeColor=white _Use_1stAs2nd=0
#           _2nd_ShadeMap unbound->"white" _ShadeColor_Step=0 feather=0.0001
#           _BaseColor_Step=0.55 _BaseShade_Feather=0.01 _Set_SystemShadowsToBase=0
#           _Is_Filter_LightColor=1 _Unlit_Intensity=1 _Is_BLD=1 (X=-0.48,Y=0,InvZ=0)
#           _RimLight=1 Power=0.5 InsideMask=0.5 FeatherOff=1 LightDirMask=1 TweakMaskLvl=0.5
#           _MatCap=0 _HighColor=(0,0,0) _GI_Intensity=0 _EMISSIVE black _Tweak_transparency=0
# With Use_BaseAs1st=1 + white shade colors + Step/feather masks, all 3 layers == tex*LC
# (shown below), so FinalBaseColor = art.rgb * LC for every pixel.
# LC = lightColor = max(defaultLightColor, sat(_LightColor0))  (filter=1)

def evaluate(art_rgb, LC, dotNV, dotNL, label):
    tex = art_rgb
    base   = [t*1.0*c for t, c in zip(tex, LC)]          # Set_BaseColor (cginc 249)
    first  = [t*1.0*c for t, c in zip(tex, LC)]          # 1st = MainTex(Use_BaseAs1st)*white*LC (cginc 251-252)
    second = [1.0*c for c in LC]                          # 2nd = white(unbound map)*white*LC (cginc 254-255)
    HL = 0.5*dotNL+0.5                                    # HalfLambert with BLD lightdir
    m2 = max(0.0, min(1.0, 1.0 + (HL-(-0.0001))*(-1.0)/0.0001))   # 2nd mask, Step=0
    m1 = max(0.0, min(1.0, 1.0 + (HL-(0.55-0.01))*(-1.0)/0.01))   # FinalShadowMask (sysShadow off)
    inner = [a + m2*(b-a) for a, b in zip(first, second)]
    final3 = [a + m1*(b-a) for a, b in zip(base, inner)]
    # Rim (cginc 274-281, FeatherOff=1 => step branch; LightDirMask=1)
    rimArea = max(0.0, 1.0 - dotNV)
    powterm = rimArea ** (2.0**(3.0-0.5*3.0))             # exp2(lerp(3,0,0.5))=2^1.5=2.8284
    inside = 1.0 if powterm >= 0.5 else 0.0               # step(InsideMask=0.5, pow)
    vertHL = 0.5*dotNL+0.5
    mask = max(0.0, min(1.0, inside - ((1.0-vertHL)+0.5)))
    rim = [c*mask for c in LC]                             # RimLightColor white * LC
    final = [min(1.0, a+r) for a, r in zip(final3, rim)]  # saturate (cginc 283, 386)
    px = lambda v: tuple(round(x*255) for x in v)
    print(f"{label}: HL={HL:.3f} m1={m1:.2f} m2={m2:.2f} rim={mask:.2f}")
    print(f"    base/1st/2nd = {px(base)} / {px(first)} / {px(second)}  -> FinalBaseColor={px(final3)}  final(+rim)={px(final)}")

pale  = (219/255, 154/255, 137/255)
white = (1.0, 1.0, 1.0)
for LC, tag in [((1,1,1), "LC=(1,1,1) neutral (our rebuild)"),
                ((1.0, 0.95, 0.75), "LC=(1.0,0.95,0.75) warm example"),
                ((1.0, 0.88, 0.70), "LC=(1.0,0.88,0.70) warm example 2")]:
    print("\n==", tag)
    evaluate(pale,  LC, 0.95, 0.90, "pale pixel, front, lit   ")
    evaluate(pale,  LC, 0.10, 0.95, "pale pixel, silhouette,lit")
    evaluate(pale,  LC, 0.95, 0.10, "pale pixel, front, unlit  ")
    evaluate(white, LC, 0.95, 0.90, "white pixel, front, lit   ")
    evaluate(white, LC, 0.10, 0.95, "white pixel, silhouet,lit ")
