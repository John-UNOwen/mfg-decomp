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
]
for f in files:
    t = open(f, encoding='utf-8', errors='replace').read()
    lines = t.splitlines()
    print("="*10, f.replace(base+"\\",""))
    m = re.search(r'shader: ([0-9a-f]+)', t)
    print("  shader guid:", m.group(1) if m else None, "| shader line:", next((i+1 for i,l in enumerate(lines) if l.startswith('  shader:')), None))
    print("  m_ShaderKeywords:", re.search(r'm_ShaderKeywords: (.*)', t).group(1).strip() if re.search(r'm_ShaderKeywords:',t) else "-")
    print("  m_CustomRenderQueue:", re.search(r'm_CustomRenderQueue: (-?\d+)', t).group(1))
    # properties: name: value  (floats/ints/Colors) under m_SavedProperties
    for mm in re.finditer(r'^\s{4}(\w+):\s*(.+)$', t, re.M):
        pass
    sp = t.split('m_SavedProperties:')
    if len(sp)>1:
        block = sp[1]
        for mm in re.finditer(r'\s+(\w+):\s*\{r: ([\d.eE+-]+), g: ([\d.eE+-]+), b: ([\d.eE+-]+), a: ([\d.eE+-]+)\}', block):
            print(f"  Color {mm.group(1)} = rgba({mm.group(2)},{mm.group(3)},{mm.group(4)},{mm.group(5)})")
        for mm in re.finditer(r'\s+(\w+): ([\d.eE+-]+)\s*$', block, re.M):
            print(f"  float {mm.group(1)} = {mm.group(2)}")
        vecs = re.finditer(r'\s+(\w+):\s*\{x: ([\d.eE+-]+), y: ([\d.eE+-]+), z: ([\d.eE+-]+), w: ([\d.eE+-]+)\}', block)
        for mm in vecs:
            print(f"  vec4 {mm.group(1)} = ({mm.group(2)},{mm.group(3)},{mm.group(4)},{mm.group(5)})")
    # texture refs
    for mm in re.finditer(r'\s+(\w+):\s*\{m_Texture: \{fileID: (-?\d+), guid: ([0-9a-f]+)', block if len(sp)>1 else ''):
        print(f"  tex {mm.group(1)} guid={mm.group(3)}")
