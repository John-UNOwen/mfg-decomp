import os, re, glob, json

TREE = r"C:\mfg\build\unity-project\Assets"
MD = os.path.join(TREE, "Material")

# guid -> file name index for Texture2D + Shader dirs
idx = {}
for sub in ("Texture2D", "Shader", "Font", "Material"):
    for root, _, files in os.walk(os.path.join(TREE, sub)):
        for f in files:
            if f.endswith(".meta"):
                p = os.path.join(root, f)
                try:
                    t = open(p, encoding="utf-8", errors="ignore").read(200)
                except Exception:
                    continue
                m = re.search(r"guid: ([0-9a-f]{32})", t)
                if m:
                    idx[m.group(1)] = f[:-5]
mats = sorted(glob.glob(os.path.join(MD, "Mt_mouth*.mat")) + glob.glob(os.path.join(MD, "Mt_eye*.mat"))
              + glob.glob(os.path.join(MD, "Mt_*Eye*.mat")) + glob.glob(os.path.join(MD, "Mt_*Iris*.mat")))
print("materials found:", len(mats))

def parse(path):
    t = open(path, encoding="utf-8").read()
    out = {"shader": None, "kws": "", "tex": {}, "floats": {}, "colors": {}}
    m = re.search(r"m_Shader: \{fileID: \d+, guid: ([0-9a-f]{32})", t)
    if m:
        out["shader"] = idx.get(m.group(1), m.group(1))
    out["kws"] = (re.search(r"m_ShaderKeywords: (.*)", t) or ["", ""])[1].strip()
    i = t.find("m_TexEnvs:")
    j = t.find("m_Floats:")
    for b in re.finditer(r"    (_\w+):\n\s+m_Texture: \{fileID: (-?\d+)(?:, guid: ([0-9a-f]{32}))?[^}]*\}\n\s+m_Scale: \{x: ([-\d.eE]+), y: ([-\d.eE]+)\}\n\s+m_Offset: \{x: ([-\d.eE]+), y: ([-\d.eE]+)\}", t[i:j if j > i else len(t)]):
        nm, fid, g, sx, sy, ox, oy = b.groups()
        out["tex"][nm] = ((idx.get(g, g) if g else "fileID" + fid), (float(sx), float(sy)), (float(ox), float(oy)))
    seg = t[j:t.find("m_Colors:", j)] if j >= 0 else ""
    for b in re.finditer(r"      (_\w+): ([-\d.eE]+)", seg):
        out["floats"][b.group(1)] = float(b.group(2))
    cseg = t[t.find("m_Colors:"):] if "m_Colors:" in t else ""
    for b in re.finditer(r"      (_\w+): \{r: ([-\d.eE]+), g: ([-\d.eE]+), b: ([-\d.eE]+), a: ([-\d.eE]+)\}", cseg):
        out["colors"][b.group(1)] = tuple(float(x) for x in b.groups()[1:])
    return out

KEEP_F = ("_MouthIndex", "_MouthOffsetX", "_MouthOffsetY", "_EyeIndex", "_EyeBrowIndex", "_PupilIndex",
          "_EyeOffsetX", "_EyeOffsetY", "_PupliOffsetX", "_PupliOffsetY", "_EyeBrowOffsetX", "_EyeBrowOffsetY",
          "_Height", "_SheetRectX", "_SheetRectY", "_SheetRectW", "_SheetRectH", "_UseSheet", "_StencilMask",
          "_Stencil", "_ZWrite")
KEEP_C = ("_BaseColor", "_RShadowColor", "_FaceShadowColor", "_1st_ShadeColor", "_2nd_ShadeColor",
          "_DissolveColor", "_Emissive_Color", "_EffectEmissive_Color", "_Emission_RimLight_Color")
KEEP_T = ("_MainTex", "_MouthTex", "_MouthMaskTex", "_MaskTex", "_MaterialMaskTex", "_DissolveTex",
          "_HeightTex", "_ShadingGradeMap", "_Emissive_Tex", "_EyeTex", "_EyeMaskTex", "_PupilTex",
          "_EyeBrowTex", "_EyeBrowMaskTex")
for p in mats:
    o = parse(p)
    nm = os.path.basename(p)
    tex = {k: v for k, v in o["tex"].items() if k in KEEP_T}
    fl = {k: v for k, v in o["floats"].items() if k in KEEP_F}
    cl = {k: tuple(round(x, 4) for x in v) for k, v in o["colors"].items() if k in KEEP_C}
    print("==", nm, "| shader:", o["shader"], "| kws:", o["kws"][:70])
    print("   tex:", json.dumps(tex))
    print("   floats:", json.dumps(fl))
    print("   colors:", json.dumps(cl))
