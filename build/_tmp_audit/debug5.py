import re, struct, json, os, zlib, math
from collections import Counter
ap = r"C:\mfg\build\unity-project\Assets\MFG\Characters\Character1\3D\C001\Fbxs\model_face_eye_L.asset"
t = open(ap, encoding="utf-8", errors="ignore").read()
vc = 271
data = bytes.fromhex(re.search(r"_typelessdata: ([0-9a-fA-F]+)", t).group(1))
def af(o): return struct.unpack_from("<f", data, o)[0]
S1 = vc*40
def auv(ch, v): return (af(S1+v*32+ch*8), af(S1+v*32+ch*8+4))
# POSITION outlier vs glb (flip x)
gp = r"C:\mfg\build\export-primary-content\Assets\MFG\Characters\Character1\3D\C001\Fbxs\model_face_eye_L.glb"
d = open(gp, "rb").read(); ln, = struct.unpack("<I", d[12:16])
j = json.loads(d[20:20+ln].decode("utf-8"))
def accdata(ai):
    a = j["accessors"][ai]; bv = j["bufferViews"][a["bufferView"]]
    off = 20+ln+8+bv.get("byteOffset",0)+a.get("byteOffset",0)
    fmt = {5126:("f",4),5123:("H",2)}[a["componentType"]]
    nch = {"SCALAR":1,"VEC2":2,"VEC3":3,"VEC4":4}[a["type"]]
    stride = bv.get("byteStride", nch*fmt[1])
    return [tuple(struct.unpack_from("<"+fmt[0]*nch, d, off+i*stride)) for i in range(a["count"])]
POSg = accdata(j["meshes"][0]["primitives"][0]["attributes"]["POSITION"])
gset = {}
for p in POSg: gset.setdefault((round(p[0],4), round(p[1],4), round(p[2],4)), 0)
outl = []
for v in range(vc):
    p = (af(v*40), af(v*40+4), af(v*40+8))
    key = (round(-p[0],4), round(p[1],4), round(p[2],4))
    if key not in gset:
        # nearest
        best = min(gset, key=lambda q: max(abs(q[0]-key[0]), abs(q[1]-key[1]), abs(q[2]-key[2])))
        dd = max(abs(best[0]-key[0]), abs(best[1]-key[1]), abs(best[2]-key[2]))
        if dd > 0.002:
            outl.append((v, p, best, round(dd,4), [round(x,4) for x in auv(1, v)]))
print("POSITION outliers (asset vs glb flipX, >2mm):", len(outl))
for o in outl[:12]: print("   v%d asset=%s nearest-glb=%s dist=%.4f uv1=%s" % o)
outl2 = [v for v in range(vc) if (round(-af(v*40),4), round(af(v*40+4),4), round(af(v*40+8),4)) not in gset]
print("asset verts with no exact -x match:", len(outl2), "of", vc)

# ---------- atlases ----------
COMMON = r"C:\mfg\build\unity-project\Assets\MFG\Characters\Character1\3D\Common"
def png_decode(path):
    dd = open(path, "rb").read(); pos = 8; idat = b""
    while pos < len(dd):
        lnn, typ = struct.unpack(">I4s", dd[pos:pos+8]); pos += 8
        da = dd[pos:pos+lnn]; pos += lnn + 4
        if typ == b"IHDR": w, h, bd, ct, *_ = struct.unpack(">IIBBBBB", da)
        elif typ == b"IDAT": idat += da
        elif typ == b"IEND": break
    raw = zlib.decompress(idat); ch = {0:1,2:3,3:1,4:2,6:4}[ct]; stride = w*ch
    out = bytearray(h*stride); prev = bytearray(stride); rp = 0
    for y in range(h):
        f = raw[rp]; rp += 1
        line = bytearray(raw[rp:rp+stride]); rp += stride
        if f == 1:
            for i in range(ch, stride): line[i] = (line[i]+line[i-ch]) & 255
        elif f == 2:
            for i in range(stride): line[i] = (line[i]+prev[i]) & 255
        elif f == 3:
            for i in range(stride):
                a2 = line[i-ch] if i>=ch else 0
                line[i] = (line[i]+((a2+prev[i])>>1)) & 255
        elif f == 4:
            for i in range(stride):
                a2 = line[i-ch] if i>=ch else 0; b2 = prev[i]; c2 = prev[i-ch] if i>=ch else 0
                pp = a2+b2-c2; pa=abs(pp-a2); pb=abs(pp-b2); pc=abs(pp-c2)
                pr = a2 if (pa<=pb and pa<=pc) else (b2 if pb<=pc else c2)
                line[i] = (line[i]+pr) & 255
        out[y*stride:(y+1)*stride] = line; prev = line
    return (w, h, ch, out)
def nearest(img, u, v):
    w, h, ch, out = img
    x = int(u*w) % w; y = int((1-v)*h) % h
    o = y*w*ch + x*ch
    return (out[o], out[o+1], out[o+2]) if ch >= 3 else (out[o], out[o], out[o])

eye = png_decode(os.path.join(COMMON, "mdl_eye.png"))
emsk = png_decode(os.path.join(COMMON, "mdl_eye_msk.png"))
pup = png_decode(os.path.join(COMMON, "mdl_pupil_L.png"))

def bucket(c):
    r, g, b = c
    if r > 215 and g > 210 and b > 200: return "white/pale"
    if max(c)-min(c) < 22: return "achrome-light" if min(c) > 140 else ("black(<60)" if max(c) < 60 else "achrome-mid")
    if r > 190 and g > 120 and b > 80 and b < 180: return "skin-tan"
    if r > 110 and g > 40 and g < 150 and b < 90: return "amber"
    if max(c) < 75: return "dark/black"
    return "other-color"

def grid_region(uvals, vvals, mapper, label, n=96):
    cnt = Counter()
    for i in range(n):
        for k in range(n):
            u = uvals[0] + (uvals[1]-uvals[0])*(i+0.5)/n
            v = vvals[0] + (vvals[1]-vvals[0])*(k+0.5)/n
            cnt[bucket(mapper(u, v))] += 1
    print(f"{label}: {dict(cnt)}")

def comp_current(u, v):
    eUV = (u, v*0.25+0.75); pUV = (u, v*0.5+0.5)
    em = nearest(emsk, *eUV); col = list(nearest(eye, *eUV)); p = nearest(pup, *pUV)
    g = em[1]/255; col = [col[k]+(p[k]-col[k])*g for k in range(3)]
    b = em[2]/255; col = [col[k]*(1-b*(1-0.1)) if k<2 else col[k]*(1-b*(1-0.2)) for k in range(3)]
    r = em[0]/255; col = [col[k]*(1-r*(1-c)) for k, c in enumerate((0.5,0.4006,0.3494))]
    return tuple(int(x) for x in col)
def comp_fixed(u, v):
    eUV = (u, v*0.25+0.75)
    em = nearest(emsk, *eUV); col = list(nearest(eye, *eUV))
    return tuple(int(x) for x in col)
def raw_eye(u, v): return nearest(eye, u, v*0.25+0.75)

# current island = uv0 rect (pupil-strip layout); fixed island = uv1 rect (eye strip)
grid_region((0,0.4243),(0,0.9443), comp_current, "CURRENT mapping (uv0 island over eye atlas, full composite)")
grid_region((0,0.4243),(0,0.9443), lambda u,v: nearest(eye, u, v*0.25+0.75), "CURRENT mapping eye-only (no mask/pupil)")
grid_region((0,0.2328),(0,0.9959), raw_eye, "FIXED mapping eye-only (uv1 island over eye cell)")
grid_region((0,0.2328),(0,0.9959), comp_fixed, "FIXED mapping full composite")

# emask zone stats within eye cell (0,0): sample grid, report R/G/B bands
cnt = Counter(); gs = []
for i in range(64):
    for k in range(64):
        u = (i+0.5)/64*0.25; v = 1 - (k+0.5)/64*0.25
        r, g, b = nearest(emsk, u, v)
        gs.append((r,g,b))
import statistics
def band(xs, name):
    xs = sorted(xs)
    print(f"   {name}: median {xs[len(xs)//2]} p90 {xs[int(len(xs)*0.9)]} p99 {xs[int(len(xs)*0.99)]} max {xs[-1]} frac>0.5*255 {sum(1 for x in xs if x>127)/len(xs):.3f}")
print("eye_msk cell(0,0) channel bands:")
band([c[0] for c in gs], "R"); band([c[1] for c in gs], "G"); band([c[2] for c in gs], "B")
# where G is high: relative position of G>127 pixels
hi = [(u,v) for i in range(64) for k in range(64) for u,v in [((i+0.5)/64, 1-(k+0.5)/64)] if nearest(emsk, u, v)[1] > 127]
if hi:
    us=[p[0] for p in hi]; vs=[p[1] for p in hi]
    print(f"   G>0.5 pixels: {len(hi)}/{64*64} u[{min(us):.3f},{max(us):.3f}] v[{min(vs):.3f},{max(vs):.3f}]")
# pupil art structure: dark pixels fraction & highlights
dark = sum(1 for i in range(1024) for k in range(1024) if sum(pup[3][k*1024*pup[0]*4 + i*4:k*1024*pup[0]*4+i*4+3])<200)
print(f"pup_L dark(sum<200) px over full 1024x1024: {dark}")
