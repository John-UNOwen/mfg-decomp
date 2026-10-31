import re, struct, json, os

path = r"C:\mfg\build\unity-project\Assets\MFG\Characters\Character1\3D\C001\Fbxs\model_face_eye_L.asset"
t = open(path, "r", encoding="utf-8", errors="ignore").read()
vc = 271
data = bytes.fromhex(re.search(r"_typelessdata: ([0-9a-fA-F]+)", t).group(1))
print("len", len(data))
def fl(off): return struct.unpack_from("<f", data, off)[0]

# candidate stream1 starts
for cand, lbl in [(7588,"s0_end(271*28)"), (7592,"7592"), (7584,"7584"), (8,"prefix8")]:
    vals = [fl(cand+i*4) for i in range(8)]
    print(f"floats at {cand} ({lbl}):", ["%.5f"%v for v in vals])
# floats near end to locate stream2
for cand in [16260, 16256, 11932]:
    print(f"floats at {cand}:", ["%.5f"%fl(cand+i*4) for i in range(8)], "bytes:", data[cand:cand+8].hex())
# last 40 bytes hex
print("tail 40:", data[-40:].hex())
# vertices with pos.y < 1
susp = []
for v in range(vc):
    y = fl(v*28+4)
    if y < 1.0: susp.append((v, fl(v*28), y, fl(v*28+8)))
print("pos vertices with y<1:", susp[:10], "count", len(susp))
# uv region sanity: count floats in [0,1] for offset candidates
def sanity(off, stride=32, n=vc):
    good = 0; bad = []
    for v in range(n):
        b = off + v*stride
        for k in range(8):
            x = fl(b+k*4)
            if 0.0 <= x <= 1.6: good += 1
            else: bad.append((v,k,round(x,4)))
    return good, bad[:12]
print("sanity uv@7588:", sanity(7588))
print("sanity uv@11932:", sanity(11932))

# glb COLOR_0 decode
gp = r"C:\mfg\build\export-primary-content\Assets\MFG\Characters\Character1\3D\C001\Fbxs\model_face_eye_L.glb"
d = open(gp, "rb").read()
ln, = struct.unpack("<I", d[12:16])
j = json.loads(d[20:20+ln].decode("utf-8"))
def accdata(ai):
    a = j["accessors"][ai]
    bv = j["bufferViews"][a["bufferView"]]
    off = 20+ln+8+bv.get("byteOffset",0)+a.get("byteOffset",0)
    comp = {5120:("b",1),5121:("B",1),5122:("h",2),5123:("H",2),5125:("I",4),5126:("f",4)}[a["componentType"]]
    nch = {"SCALAR":1,"VEC2":2,"VEC3":3,"VEC4":4}[a["type"]]
    out = []
    stride = bv.get("byteStride", nch*comp[1])
    for i in range(a["count"]):
        base = off + i*stride
        out.append(tuple(struct.unpack_from("<"+comp[0]*nch, d, base)))
    return out
col = accdata(j["meshes"][0]["primitives"][0]["attributes"]["COLOR_0"])
print("\nCOLOR_0[0:5]:", col[:5])
uniq = {}
for c in col: uniq[c] = uniq.get(c,0)+1
print("COLOR_0 unique count:", len(uniq))
for k,v in sorted(uniq.items(), key=lambda x:-x[1])[:10]: print("   ", k, v)
uv0 = accdata(j["meshes"][0]["primitives"][0]["attributes"]["TEXCOORD_0"])
col0 = accdata(j["meshes"][0]["primitives"][0]["attributes"]["TEXCOORD_3"])
# correlate color with uv
import statistics
groups = {}
for c, uvc in zip(col, uv0):
    groups.setdefault(c, []).append(uvc)
for k, vs in sorted(groups.items(), key=lambda x: -len(x[1]))[:6]:
    us = [p[0] for p in vs]; vs_ = [p[1] for p in vs]
    print(f"color {k}: n={len(vs)} uv0 u[{min(us):.4f},{max(us):.4f}] v[{min(vs_):.4f},{max(vs_):.4f}]")
