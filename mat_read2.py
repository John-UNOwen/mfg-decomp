import re
base = r"C:\mfg\build\unity-project\Assets\MFG\Characters\Character1\3D"
files = [
 base+r"\C001\Materials\Mt_outline_hand.mat",
 base+r"\C001\Materials\Mt_outline_legs.mat",
 base+r"\C001\Materials\Mt_outline_skirt.mat",
 base+r"\C001\Materials\Mt_outline_jacket.mat",
 base+r"\C001\Materials\Mt_outline_ribbon.mat",
 base+r"\Common\Mt_outline_face.mat",
 base+r"\Common\Mt_outline_hair.mat",
 base+r"\C002\Materials\Mt_outline_hand.mat",
 base+r"\Common\Mt_face.mat",
]
for f in files:
    lines = open(f, encoding='utf-8', errors='replace').read().splitlines()
    print("="*8, f.replace(base+"\\",""))
    for i,l in enumerate(lines,1):
        s=l.strip()
        if s.startswith('shader:') or s.startswith('_width:') or s.startswith('_Color:') or s.startswith('m_CustomRenderQueue') or s.startswith('_Saturation:') or s.startswith('_Texture:') or s.startswith('m_Name:'):
            print(f"  L{i}: {s}")
    tex = [l.strip() for l in lines if 'guid:' in l and ('m_TexEnvs' in '\n'.join(lines[max(0,lines.index(l)-4):lines.index(l)]) if False else False)]
    # print m_TexEnvs block raw
    try:
        i0 = next(i for i,l in enumerate(lines) if l.startswith('  m_TexEnvs:'))
        i1 = next(i for i,l in enumerate(lines) if l.startswith('  m_Floats:') or l.startswith('  m_Colors:') or l.startswith('  m_Ints'))
        blk = lines[i0:min(i0+14,len(lines))]
        print("  TexEnvs block:", " || ".join(x.strip() for x in lines[i0:i0+12]))
    except StopIteration:
        print("  (no TexEnvs)")
