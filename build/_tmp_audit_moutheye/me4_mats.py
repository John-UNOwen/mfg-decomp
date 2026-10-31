import re, os, glob

MD = r"C:\mfg\build\unity-project\Assets\Material"
KEYS = ("_MouthIndex", "_MouthOffsetX", "_MouthOffsetY", "_MouthTex", "_MouthMaskTex",
        "_EyeIndex", "_PupilIndex", "_EyeBrowIndex", "_EyeOffsetX", "_EyeOffsetY",
        "_PupliOffsetX", "_PupliOffsetY", "_EyeBrowOffsetX", "_EyeBrowOffsetY",
        "_EyeTex", "_EyeMaskTex", "_PupilTex", "_EyeBrowTex", "_EyeBrowMaskTex",
        "_Height", "_HeightTex", "_MouthTex_ST", "_MouthMaskTex_ST")

def vals(text):
    out = {}
    # m_Floats: - first: NAME\n second: value  OR second: {x: .., y: ..}
    for m in re.finditer(r"- first: (_[\w]+)\n\s+second: \{([^}]*)\}", text):
        n, v = m.group(1), m.group(2)
        if any(k == n for k in KEYS) or n.endswith("_ST"):
            out.setdefault(n, []).append("vec{" + " ".join(v.split()) + "}")
    for m in re.finditer(r"- first: (_[\w]+)\n\s+second: (-?[\d.eE+-]+)", text):
        n, v = m.group(1), m.group(2)
        if any(k == n for k in KEYS):
            out.setdefault(n, []).append(v)
    # texture envs
    for m in re.finditer(r"- first: (_[\w]+)\n\s+second:\n\s+m_Texture: \{[^}]*guid: ([0-9a-f]+)[^}]*\}\n\s+m_Scale: \{([^}]*)\}\n\s+m_Offset: \{([^}]*)\}", text):
        n, g, sc, off = m.groups()
        if any(k == n or n.startswith(k) for k in KEYS if not k.endswith("_ST")):
            out.setdefault(n, []).append(f"tex g={g[:8]} scale={{{sc}}} offset={{{off}}}")
    return out

files = sorted(glob.glob(MD + r"\Mt_mouth*.mat") + glob.glob(MD + r"\Mt_eye_*.mat"))
for f in files:
    t = open(f, encoding="utf-8").read()
    sh = re.search(r"m_Shader: \{fileID: \d+, guid: ([0-9a-f]+)", t)
    name = os.path.basename(f)
    d = vals(t)
    keep = {k: v for k, v in d.items() if any(k == kk or k.startswith(kk + "_") for kk in
            ("_MouthIndex", "_MouthOffset", "_MouthTex", "_MouthMaskTex", "_EyeIndex",
             "_PupilIndex", "_EyeBrowIndex", "_EyeOffset", "_PupliOffset", "_EyeBrowOffset",
             "_EyeTex", "_EyeMaskTex", "_PupilTex", "_EyeBrowTex", "_EyeBrowMaskTex", "_Height"))}
    print(name, "shader:", (sh.group(1)[:12] if sh else "?"))
    for k in sorted(keep):
        print("   ", k, keep[k][:2])
