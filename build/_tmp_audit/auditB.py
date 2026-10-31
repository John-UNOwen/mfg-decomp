import re, struct, json, os

# ---------- rebuilt mesh .asset decode ----------
def decode_asset(path):
    t = open(path, "r", encoding="utf-8", errors="ignore").read()
    m = re.search(r"m_VertexCount: (\d+)", t)
    vc = int(m.group(1))
    m = re.search(r"_typelessdata: ([0-9a-fA-F]+)", t)
    data = bytes.fromhex(m.group(1))
    nch = re.findall(r"- stream: (\d+)\s+offset: (\d+)\s+format: (\d+)\s+dimension: (\d+)", t)
    chans = [tuple(map(int, c)) for c in nch]
    names = ["position","normal","tangent","color","uv0","uv1","uv2","uv3","uv4","uv5","uv6","uv7","blendIndices?","blendChannels"]
    print(f"== {os.path.basename(path)}: vertcount={vc}, databytes={len(data)}, channels:")
    for i, (s, o, f, d) in enumerate(chans):
        if d: print(f"   ch{i:<13} stream={s} offset={o} format={f} dim={d}")
    # stream layout (from dims): stream0 stride 28 (pos@0,nrm@12,tan@24), stream1 stride 32 (uv0..3), stream2 stride 16
    s0_stride, s1_stride, s2_stride = 28, 32, 16
    off0 = 0; off1 = vc*s0_stride; off2 = off1 + vc*s1_stride
    need = off2 + vc*s2_stride
    print(f"   layout check: {off0}..{off1}..{off2}..{off2+vc*s2_stride} vs {len(data)}")
    def stream_ranges(off, stride, spec):  # spec list of (floats, label)
        mins = [None]*len(spec); maxs = [None]*len(spec)
        for v in range(vc):
            base = off + v*stride
            cur = 0
            for fi, (cnt, lbl) in enumerate(spec):
                for c in range(cnt):
                    val = struct.unpack_from("<f", data, base + (cur+c)*4)[0]
                    if mins[fi] is None or val < mins[fi][c]: 
                        if mins[fi] is None: mins[fi] = [1e30]*cnt; maxs[fi] = [-1e30]*cnt
                        mins[fi][c] = min(mins[fi][c] if isinstance(mins[fi], list) else val, val)
                        maxs[fi][c] = max(maxs[fi][c] if isinstance(maxs[fi], list) else val, val)
                cur += cnt
        for fi, (cnt, lbl) in enumerate(spec):
            if mins[fi]: print(f"   {lbl}: min={['%.6f'%v for v in mins[fi]]} max={['%.6f'%v for v in maxs[fi]]}")
    stream_ranges(off0, s0_stride, [(3,"POSITION"),(3,"NORMAL"),(4,"TANGENT")])
    stream_ranges(off1, s1_stride, [(2,"uv0"),(2,"uv1"),(2,"uv2"),(2,"uv3")])
    return vc

for f in ("model_face_eye_L.asset", "model_face_eye_R.asset"):
    decode_asset(rf"C:\mfg\build\unity-project\Assets\MFG\Characters\Character1\3D\C001\Fbxs\{f}")

# ---------- glb decode ----------
def glb(path):
    d = open(path, "rb").read()
    ln, = struct.unpack("<I", d[12:16])
    j = json.loads(d[20:20+ln].decode("utf-8"))
    binoff = 20 + ln + 8
    return j, d, binoff

def bufminmax(j, d, binoff, acc_i):
    a = j["accessors"][acc_i]
    bv = j["bufferViews"][a.get("bufferView", 0)] if "bufferView" in a else None
    if bv and "min" in a:
        return a["min"], a["max"]
    return None, None

gp = r"C:\mfg\build\export-primary-content\Assets\MFG\Characters\Character1\3D\C001\Fbxs\model_face_eye_L.glb"
j, d, binoff = glb(gp)
print(f"\n== GLB {os.path.basename(gp)} ==")
print("asset:", j.get("asset"))
print("meshes:", [(m["name"], [(p.get('mode'), p.get('attributes'), p.get('indices')) for p in m["primitives"]]) for m in j["meshes"]])
for i, a in enumerate(j["accessors"]):
    if a["type"] in ("VEC2", "VEC3", "VEC4") and "min" in a:
        print(f"  accessor[{i}] type={a['type']} comp={a['componentType']} count={a['count']} min={['%.6f'%v for v in a['min']]} max={['%.6f'%v for v in a['max']]}")
    elif a["type"] == "SCALAR":
        print(f"  accessor[{i}] SCALAR count={a['count']}")
for i, m in enumerate(j["meshes"][0]["primitives"][0]["attributes"].items()):
    print("  prim attr:", m)
print("nodes:", json.dumps([{ "i":i, "name":n.get("name"), "mesh":n.get("mesh"), "translation":n.get("translation"), "rotation":n.get("rotation"), "scale":n.get("scale"), "children":n.get("children"), "skin":n.get("skin")} for i, n in enumerate(j["nodes"])], indent=0))
print("skins:", json.dumps(j.get("skins"))[:600])
print("materials:", json.dumps(j.get("materials"), indent=0)[:2000])
print("textures:", json.dumps(j.get("textures"))[:800])
print("images:", json.dumps([{"name":im.get("name"),"mimeType":im.get("mimeType"),"bufferView":im.get("bufferView"),"byteLength":im.get("byteLength")} for im in j.get("images",[])])[:800])

# glb world y of eye card: node translation + POSITION range
# ---------- prefab ----------
pp = r"C:\mfg\build\unity-project\Assets\MFG\Characters\Character1\3D\C001\Prefabs\CharaModel.prefab"
t = open(pp, "r", encoding="utf-8", errors="ignore").read()
print(f"\n== CharaModel.prefab size {len(t)} ==")
for m in re.finditer(r"m_Name: ([^\n]*(?:eye|face|brow|mouth|group)[^\n]*)", t, re.I):
    print("  GameObject/obj name:", m.group(1), "at char", m.start())
