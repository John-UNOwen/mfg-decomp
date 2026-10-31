import re, struct, json, os

def decode_asset(path):
    t = open(path, "r", encoding="utf-8", errors="ignore").read()
    vc = int(re.search(r"m_VertexCount: (\d+)", t).group(1))
    data = bytes.fromhex(re.search(r"_typelessdata: ([0-9a-fA-F]+)", t).group(1))
    m = re.search(r"m_Center: \{x: ([-\d.eInf]+), y: ([-\d.eInf]+), z: ([-\d.eInf]+)\}\s+m_Extent: \{x: ([-\d.eInf]+), y: ([-\d.eInf]+), z: ([-\d.eInf]+)\}", t)
    c = [float(m.group(i)) for i in range(1,4)]; e = [float(m.group(i)) for i in range(4,7)]
    print(f"== {os.path.basename(path)} vc={vc} bytes={len(data)}")
    print(f"   AABB: x[{c[0]-e[0]:.6f},{c[0]+e[0]:.6f}] y[{c[1]-e[1]:.6f},{c[1]+e[1]:.6f}] z[{c[2]-e[2]:.6f},{c[2]+e[2]:.6f}]")
    s0_stride, s1_stride = 28, 32
    off1 = vc*s0_stride
    def ranges(off, stride, spec, name="stream"):
        for fi, cnt, lbl in spec:
            vals = [[] for _ in range(cnt)]
            for v in range(vc):
                base = off + v*stride
                for k in range(cnt):
                    vals[k].append(struct.unpack_from("<f", data, base + (sum(s[1] for s in spec[:fi])+k)*4)[0])
            r = ", ".join(f"{lbl}.{chr(120+k)}[{min(vs):.6f},{max(vs):.6f}]" for k, vs in enumerate(vals))
            print(f"   {r}")
    ranges(off1, s1_stride, [(0,2,"uv0"),(2,2,"uv1"),(4,2,"uv2"),(6,2,"uv3")])
    ranges(0, s0_stride, [(0,3,"pos"),(3,3,"nrm"),(6,4,"tan")])

for f in ("model_face_eye_L.asset", "model_face_eye_R.asset"):
    decode_asset(rf"C:\mfg\build\unity-project\Assets\MFG\Characters\Character1\3D\C001\Fbxs\{f}")

def glb(path):
    d = open(path, "rb").read()
    ln, = struct.unpack("<I", d[12:16])
    return json.loads(d[20:20+ln].decode("utf-8"))

gp = r"C:\mfg\build\export-primary-content\Assets\MFG\Characters\Character1\3D\C001\Fbxs\model_face_eye_L.glb"
j = glb(gp)
print(f"\n== GLB eye_L ==")
for mi, m in enumerate(j["meshes"]):
    for pi, p in enumerate(m["primitives"]):
        print(f" mesh[{mi}] name={m.get('name')} prim[{pi}] mode={p.get('mode')} material={p.get('material')} indices={p.get('indices')}")
        for an, ai in sorted(p["attributes"].items()):
            a = j["accessors"][ai]
            print(f"   {an}: acc={ai} type={a['type']} count={a['count']} min={['%.6f'%v for v in a.get('min',[])]} max={['%.6f'%v for v in a.get('max',[])]}")
print("nodes:")
for i, n in enumerate(j["nodes"]):
    print("  ", i, json.dumps(n))
print("skins:", json.dumps(j.get("skins"))[:400])
print("materials:", json.dumps(j.get("materials"), indent=1))
print("textures:", json.dumps(j.get("textures")))
print("images:", json.dumps([{ "name": im.get("name"), "mime": im.get("mimeType"), "len": im.get("byteLength"), "bv": im.get("bufferView")} for im in j.get("images", [])]))

# glb eye_R too
jr = glb(gp.replace("_L.glb", "_R.glb"))
print("\n== GLB eye_R summary ==")
for mi, m in enumerate(jr["meshes"]):
    for pi, p in enumerate(m["primitives"]):
        for an, ai in sorted(p["attributes"].items()):
            a = jr["accessors"][ai]
            if an in ("POSITION","TEXCOORD_0"):
                print(f"   {an}: count={a['count']} min={['%.6f'%v for v in a.get('min',[])]} max={['%.6f'%v for v in a.get('max',[])]}")
print("nodes:", json.dumps([{ "i":i, "name":n.get("name"), "t":n.get("translation"), "r":n.get("rotation"), "s":n.get("scale"), "mesh":n.get("mesh"), "children":n.get("children")} for i, n in enumerate(jr["nodes"])], indent=0)[:800])
