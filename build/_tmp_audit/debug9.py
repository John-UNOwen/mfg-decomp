import re, struct
ap = r"C:\mfg\build\unity-project\Assets\MFG\Characters\Character1\3D\C001\Fbxs\model_face_mouth.asset"
t = open(ap, encoding="utf-8", errors="ignore").read()
vc = int(re.search(r"m_VertexCount: (\d+)", t).group(1))
data = bytes.fromhex(re.search(r"_typelessdata: ([0-9a-fA-F]+)", t).group(1))
chans = re.findall(r"- stream: (\d+)\s+offset: (\d+)\s+format: (\d+)\s+dimension: (\d+)", t)
dims = [int(c[3]) for c in chans]
print("mouth vc", vc, "bytes", len(data), "dims", dims)
s0_stride = 40
S1 = vc*s0_stride
def af(o): return struct.unpack_from("<f", data, o)[0]
for k in range(4):
    if dims[4+k] == 2:
        xs = [af(S1+v*16+k*8) for v in range(vc)]; ys = [af(S1+v*16+k*8+4) for v in range(vc)]
        print(f"  uv{k}: x[{min(xs):.6f},{max(xs):.6f}] y[{min(ys):.6f},{max(ys):.6f}] zeros={sum(1 for a,b in zip(xs,ys) if a==0 and b==0)}")
    else:
        print(f"  uv{k}: absent")
# validate s0 stride 40 by y-likes
n = sum(1 for v in range(vc) if 1.3 < af(v*s0_stride+4) < 1.55)
print("  ylike@40:", n, "/", vc)
# raw m_Bones block in prefab
pp = r"C:\mfg\build\unity-project\Assets\MFG\Characters\Character1\3D\C001\Prefabs\CharaModel.prefab"
pt = open(pp, encoding="utf-8", errors="ignore").read()
i = pt.find("m_Bones:")
print("\nprefab m_Bones sample:", pt[i:i+320].replace("\n", " ") if i >= 0 else "NOT FOUND")
nb = len(re.findall(r"m_Bones:", pt))
print("m_Bones blocks:", nb)
bp = pt.find("m_BindPose:")
print("m_BindPose present:", bp >= 0)
