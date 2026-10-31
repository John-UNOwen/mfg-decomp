import re, struct, json, os

path = r"C:\mfg\build\unity-project\Assets\MFG\Characters\Character1\3D\C001\Fbxs\model_face_eye_%s.asset"
for side in "LR":
    t = open(path % side, "r", encoding="utf-8", errors="ignore").read()
    vc = int(re.search(r"m_VertexCount: (\d+)", t).group(1))
    data = bytes.fromhex(re.search(r"_typelessdata: ([0-9a-fA-F]+)", t).group(1))
    S0, S1 = 40, vc*40
    def f(o): return struct.unpack_from("<f", data, o)[0]
    print(f"== asset eye_{side}: s0@0 stride40, s1@{S1}, s2@{S1+vc*32} (len {len(data)})")
    def ranges(name, off, stride, spec):
        cols = {}
        for k, lbl in spec:
            vals = [f(off+v*stride+k*4) for v in range(vc)]
            print(f"   {lbl}: [{min(vals):.6f},{max(vals):.6f}]")
    ranges("pos", 0, 40, [(0,"pos.x"),(1,"pos.y"),(2,"pos.z")])
    ranges("w28-39", 0, 40, [(7,"f7"),(8,"f8"),(9,"f9")])
    ranges("uv", S1, 32, [(0,"uv0.x"),(1,"uv0.y"),(2,"uv1.x"),(3,"uv1.y"),(4,"uv2.x"),(5,"uv2.y"),(6,"uv3.x"),(7,"uv3.y")])
    o2 = S1 + vc*32
    print("   s2 first 10 as float:", ["%.6g"%f(o2+i*4) for i in range(10)])
    print("   s2 first 10 as uint32:", struct.unpack_from("<10I", data, o2))
    # any uv2/uv3 vertex ==0 exactly?
    z3 = sum(1 for v in range(vc) if f(S1+v*32+6*4)==0 and f(S1+v*32+7*4)==0)
    print("   verts with uv3==(0,0):", z3)

# glb JOINTS_0 / WEIGHTS_0
gp = r"C:\mfg\build\export-primary-content\Assets\MFG\Characters\Character1\3D\C001\Fbxs\model_face_eye_L.glb"
d = open(gp,"rb").read(); ln, = struct.unpack("<I", d[12:16])
j = json.loads(d[20:20+ln].decode("utf-8"))
def accdata(ai):
    a = j["accessors"][ai]; bv = j["bufferViews"][a["bufferView"]]
    off = 20+ln+8+bv.get("byteOffset",0)+a.get("byteOffset",0)
    fmt = {5120:("b",1),5121:("B",1),5122:("h",2),5123:("H",2),5125:("I",4),5126:("f",4)}[a["componentType"]]
    nch = {"SCALAR":1,"VEC2":2,"VEC3":3,"VEC4":4}[a["type"]]
    stride = bv.get("byteStride", nch*fmt[1])
    return [tuple(struct.unpack_from("<"+fmt[0]*nch, d, off+i*stride)) for i in range(a["count"])]
att = j["meshes"][0]["primitives"][0]["attributes"]
jo = accdata(att["JOINTS_0"]); we = accdata(att["WEIGHTS_0"])
print("\nglb JOINTS_0 unique:", sorted(set(jo))[:5], "count", len(set(jo)))
print("glb WEIGHTS_0 unique:", sorted(set(we))[:5])

# prefab: names, transforms, skinned renderers
pp = r"C:\mfg\build\unity-project\Assets\MFG\Characters\Character1\3D\C001\Prefabs\CharaModel.prefab"
t = open(pp, "r", encoding="utf-8", errors="ignore").read()
print(f"\n== CharaModel.prefab size {len(t)} ==")
# split into documents
docs = re.split(r"^--- ", t, flags=re.M)
print("documents:", len(docs))
go = {}   # fileID -> name
for doc in docs:
    m = re.match(r"!u!1 &(\d+)", doc)
    if m:
        n = re.search(r"m_Name: (.*)", doc)
        go[m.group(1)] = n.group(1).strip() if n else "?"
print("named GameObjects:", {k:v for k,v in go.items() if re.search(r"eye|face|mouth|brow|group", v, re.I)})
for doc in docs:
    if re.search(r"!u!(4|23) &", doc):  # Transform / RectTransform
        m = re.match(r"!u!(4|23) &(\d+)(?: &(\d+))?", doc)
        if not m: continue
        fid, father = m.group(2), re.search(r"m_Father: \{fileID: (\d+)\}", doc)
        for key in ("m_LocalPosition","m_LocalRotation","m_LocalScale"):
            pass
        lp = re.search(r"m_LocalPosition: \{([^}]*)\}", doc)
        lr = re.search(r"m_LocalRotation: \{([^}]*)\}", doc)
        ls = re.search(r"m_LocalScale: \{([^}]*)\}", doc)
        hdr = re.search(r"--- !u!(4|23) &(\d+)\s+GameObject: \{fileID: (\d+)\}", "--- " + doc)
        gid = re.search(r"m_GameObject: \{fileID: (\d+)\}", doc)
        nm = go.get(gid.group(1) if gid else "", "?")
        if re.search(r"eye|face|mouth|group", nm, re.I):
            print(f"  TRS {nm}: pos={lp.group(1) if lp else None} rot={lr.group(1) if lr else None} scale={ls.group(1) if ls else None} father={father.group(1) if father else None}")
    if "!u!137 &" in doc[:20]:  # SkinnedMeshRenderer
        nm_g = re.search(r"m_GameObject: \{fileID: (\d+)\}", doc)
        nm = go.get(nm_g.group(1) if nm_g else "", "?")
        mesh = re.search(r"m_Mesh: \{fileID: (-?\d+), guid: ([0-9a-f]+), type: (\d+)\}", doc)
        mats = re.findall(r"m_Materials:.*?guid: ([0-9a-f]+)", doc, re.S)
        if re.search(r"eye", nm, re.I):
            print(f"  SMR {nm}: mesh={mesh.groups() if mesh else None} mats={mats}")
            bones = re.findall(r"-\{fileID: (\d+), guid: ([0-9a-f]+), type: (\d+)\}", doc)
            b = re.search(r"m_Bones:(.*?)(?=\nm_CullingUpdate)", doc, re.S)
            print("     bones:", re.findall(r"guid: ([0-9a-f]+), type: 3\}", b.group(1))[:8] if b else None)
