import re, json, struct, os
# 1) property diff: .mat serialized props vs shader declared props
mat = open(r"C:\mfg\build\unity-project\Assets\MFG\Characters\Character1\3D\Common\Mt_eye_L.mat", encoding="utf-8").read()
tex = set(re.findall(r"^      (_\w+):\n        m_Texture:", mat, re.M))
fl = set(re.findall(r"^      (_\w+): [-\d.e]+$", mat, re.M))
co = set(re.findall(r"^      (_\w+): \{r:", mat, re.M))
sh = open(r"C:\mfg\build\unity-project\Assets\Shaders\Character\Eye.shader", encoding="utf-8").read()
props_block = sh[sh.find("Properties {"):sh.find("\tSubShader")]
decl = set(re.findall(r"(_\w+)\s*\(\"", props_block))
allmat = tex | fl | co
print("mat props:", len(allmat), "(tex", len(tex), "float", len(fl), "color", len(co), ")  shader-declared:", len(decl))
print("MISSING from shader Properties:", sorted(allmat - decl))
# declared-but-never-used in shader code
code = sh[sh.find("CGPROGRAM"):]
unused = sorted(p for p in decl if p not in code and not p.endswith("_ST"))
print("declared but unreferenced in code:", unused)
print("code references undeclared:", sorted({m for m in re.findall(r"\b_[A-Za-z]\w+", code) if m not in decl and m not in ("_MainTex_ST","_WorldSpaceCameraPos","_WorldSpaceLightPos0","_LightColor0") and not m.startswith("__") and not re.match(r"^_[A-Z]", m) or False}))

# 2) Eye.json original property names
j = json.load(open(r"C:\mfg\build\export-primary-content\Assets\Shaders\Character\Eye.json", encoding="utf-8"))
props = j["m_ParsedForm"]["m_PropInfo"]["m_Props"]
print("\nEye.json first prop:", json.dumps(props[0])[:300])
names = []
for p in props:
    n = p.get("m_Name")
    names.append(n if isinstance(n, str) else json.dumps(n))
print("total:", len(names))
for probe in ("_StencilMask", "_StencilEyeMask", "_StencilMode", "_ClippingMask", "_EyeTex", "_EyeIrisColor", "_EyeScleraColor", "_Brightness", "_Chroma"):
    print("  ", probe, "->", probe in names)
print("first 12 names:", names[:12])

# 3) Mouth mesh uv + Mouth.shader mapping
gp = r"C:\mfg\build\export-primary-content\Assets\MFG\Characters\Character1\3D\C001\Fbxs\model_face_mouth.glb"
d = open(gp, "rb").read(); ln, = struct.unpack("<I", d[12:16])
jm = json.loads(d[20:20+ln].decode("utf-8"))
for an, ai in sorted(jm["meshes"][0]["primitives"][0]["attributes"].items()):
    a = jm["accessors"][ai]
    print("mouth", an, a.get("min"), a.get("max"))
ms = open(r"C:\mfg\build\unity-project\Assets\Shaders\Character\Mouth.shader", encoding="utf-8").read()
for m in re.finditer(r"texcoord\d", ms):
    pass
print("Mouth.shader texcoord usage:", re.findall(r"texcoord\d[^;\n]{0,60}", ms)[:10])

# 4) Hiyori_01.glb skin joints
gh = r"C:\mfg\build\export-primary-content\Assets\MFG\Characters\Character1\3D\C001\Fbxs\Hiyori_01.glb"
dh = open(gh, "rb").read(); lnh, = struct.unpack("<I", dh[12:16])
jh = json.loads(dh[20:20+lnh].decode("utf-8"))
skin = jh["skins"][0]
names = [jh["nodes"][j].get("name") for j in skin["joints"][:12]]
print("Hiyori skin joints[0:12]:", names)
print("Hiyori root nodes:", [(i, n.get("name"), n.get("translation"), n.get("rotation"), n.get("scale")) for i, n in enumerate(jh["nodes"]) if n.get("name") and i < 8])
