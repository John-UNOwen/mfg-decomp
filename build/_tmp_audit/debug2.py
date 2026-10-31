import re, struct
path = r"C:\mfg\build\unity-project\Assets\MFG\Characters\Character1\3D\C001\Fbxs\model_face_eye_L.asset"
t = open(path, "r", encoding="utf-8", errors="ignore").read()
occ = [m.start() for m in re.finditer(r"_typelessdata", t)]
print("_typelessdata occurrences:", len(occ), occ[:10])
for i, o in enumerate(occ):
    m = re.match(r"_typelessdata: ([0-9a-fA-F]*)", t[o:])
    print(f"  occ{i} char {o}: hexlen={len(m.group(1))} first40={m.group(1)[:40]}")
hexs = re.search(r"_typelessdata: ([0-9a-fA-F]+)", t).group(1)
print("pos of 'e2b759bc' in hex:", hexs.find("e2b759bc"))
print("pos of '0000803f' first:", hexs.find("0000803f"))
data = bytes.fromhex(hexs)
print("first 14 floats:", ["%.5f"%struct.unpack_from("<f", data, i*4)[0] for i in range(14)])
# find the true vertex stride: look for repeated pattern of tangent w=±1.0 at fixed stride:
# positions y should cluster ~1.44-1.58. scan for stride candidates:
import collections
def count_ylike(stride, start):
    n = 0; tot = 0
    off = start
    while off + 8 < len(data):
        try:
            y = struct.unpack_from("<f", data, off+4)[0]
        except Exception: break
        if 1.4 < y < 1.6: n += 1
        tot += 1
        off += stride
    return n, tot
for st in (28, 32, 36, 40, 44, 48):
    print(f"stride {st}: ylike {count_ylike(st,0)}")
# maybe pos stride 28 but stream0 length 7588 then padding; check floats at 28*k for k in 0..5
for k in range(6):
    base = k*28
    print(f"vertex {k}: pos={['%.5f'%struct.unpack_from('<f', data, base+i*4)[0] for i in range(3)]}")
