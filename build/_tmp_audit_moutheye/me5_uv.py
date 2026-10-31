import json, struct, base64, os, sys

def glb_uv_ranges(path):
    raw = open(path, "rb").read()
    jlen, = struct.unpack_from("<I", raw, 12)
    j = json.loads(raw[20:20 + jlen].decode("utf-8"))
    binp = raw[20 + jlen:]
    buf = binp
    if j.get("buffers"):
        b0 = j["buffers"][0]
        if b0.get("uri"):
            buf = base64.b64decode(b0["uri"].split(",", 1)[1])
    out = []
    seen = set()
    for mi, m in enumerate(j["meshes"]):
        for pi, pr in enumerate(m["primitives"]):
            for attr in ("TEXCOORD_0", "TEXCOORD_1"):
                if attr not in pr.get("attributes", {}):
                    continue
                ai = pr["attributes"][attr]
                acc = j["accessors"][ai]
                if acc.get("type") != "VEC2":
                    continue
                bv = j["bufferViews"][acc["bufferView"]]
                off = bv.get("byteOffset", 0) + acc.get("byteOffset", 0)
                n = acc["count"]
                comp = acc["componentType"]
                fmt = {5126: "f", 5123: "H", 5121: "B"}[comp]
                esz = {5126: 4, 5123: 2, 5121: 1}[comp]
                stride = bv.get("byteStride", esz * 2)
                mn = [1e9, 1e9]
                mx = [-1e9, -1e9]
                for i in range(n):
                    v = struct.unpack_from("<2" + fmt, buf, off + i * stride)
                    for c in (0, 1):
                        if v[c] < mn[c]:
                            mn[c] = v[c]
                        if v[c] > mx[c]:
                            mx[c] = v[c]
                key = (mi, pi, attr)
                out.append((m.get("name", f"mesh{mi}"), pi, attr, n, [round(x, 4) for x in mn], [round(x, 4) for x in mx]))
    return out

files = [
    os.path.expandvars(r"C:\Users\%USERNAME%\Documents\GitHub\mfg-decomp\build\export-primary-content\Assets\MFG\Characters\Character1\3D\C001\Fbxs\model_face_mouth.glb"),
    os.path.expandvars(r"C:\Users\%USERNAME%\Documents\GitHub\mfg-decomp\build\export-primary-content\Assets\Mesh\model_face_mouth.glb"),
    os.path.expandvars(r"C:\Users\%USERNAME%\Documents\GitHub\mfg-decomp\build\export-primary-content\Assets\Mesh\model_mouth.glb"),
]
for f in files:
    print("==", os.path.basename(os.path.dirname(os.path.dirname(os.path.dirname(f)))) + "/" + os.path.basename(f))
    try:
        for r in glb_uv_ranges(f):
            print("   ", r)
    except Exception as e:
        print("    fail:", repr(e)[:120])
