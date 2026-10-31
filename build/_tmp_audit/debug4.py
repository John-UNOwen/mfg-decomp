import re, struct, json, os, zlib
# ---- per-vertex correspondence asset vs glb (L) ----
ap = r"C:\mfg\build\unity-project\Assets\MFG\Characters\Character1\3D\C001\Fbxs\model_face_eye_L.asset"
t = open(ap, encoding="utf-8", errors="ignore").read()
vc = 271
data = bytes.fromhex(re.search(r"_typelessdata: ([0-9a-fA-F]+)", t).group(1))
def af(o): return struct.unpack_from("<f", data, o)[0]
S1 = vc*40
def auv(ch, v):  # ch 0..3 within stream1
    return af(S1 + v*32 + ch*8), af(S1 + v*32 + ch*8 + 4)

gp = r"C:\mfg\build\export-primary-content\Assets\MFG\Characters\Character1\3D\C001\Fbxs\model_face_L_or_L.glb".replace("_L_or_L", "")
gp = r"C:\mfg\build\export-primary-content\Assets\MFG\Characters\Character1\3D\C001\Fbxs\model_face_eye_L.glb"
d = open(gp, "rb").read(); ln, = struct.unpack("<I", d[12:16])
j = json.loads(d[20:20+ln].decode("utf-8"))
def accdata(ai):
    a = j["accessors"][ai]; bv = j["bufferViews"][a["bufferView"]]
    off = 20+ln+8+bv.get("byteOffset",0)+a.get("byteOffset",0)
    fmt = {5120:("b",1),5121:("B",1),5122:("h",2),5123:("H",2),5125:("I",4),5126:("f",4)}[a["componentType"]]
    nch = {"SCALAR":1,"VEC2":2,"VEC3":3,"VEC4":4}[a["type"]]
    stride = bv.get("byteStride", nch*fmt[1])
    return [tuple(struct.unpack_from("<"+fmt[0]*nch, d, off+i*stride)) for i in range(a["count"])]
att = j["meshes"][0]["primitives"][0]["attributes"]
T = {k: accdata(v) for k, v in att.items() if k.startswith("TEXCOORD")}
POSg = accdata(att["POSITION"]); POSa = [(af(v*40), af(v*40+4), af(v*40+8)) for v in range(vc)]
def cmp2(A, B, flip):
    mx = 0
    for a, b in zip(A, B):
        bb = (b[0], 1-b[1]) if flip else b
        mx = max(mx, abs(a[0]-bb[0]), abs(a[1]-bb[1]))
    return mx
def cmp3(A, B, flipx):
    mx = 0
    for a, b in zip(A, B):
        bb = (-b[0], b[1], b[2]) if flipx else b
        mx = max(mx, abs(a[0]-bb[0]), abs(a[1]-bb[1]), abs(a[2]-bb[2]))
    return mx
def cmp2s(A, B, flip):  # set-based (vertex order differs)
    A2 = sorted((round(a[0],5), round(a[1],5)) for a in A)
    B2 = sorted((round(b[0],5), round((1-b[1]) if flip else b[1],5)) for b in B)
    return max(abs(a[0]-b[0]) + abs(a[1]-b[1]) for a, b in zip(A2, B2))
print("asset uv_k vs glb TEXCOORD_j  (max abs err, v-flip applied):")
for k in range(4):
    Au = [auv(k, v) for v in range(vc)]
    for name in ("TEXCOORD_0","TEXCOORD_1","TEXCOORD_2","TEXCOORD_3"):
        e_flip = cmp2(Au, T[name], True); e_raw = cmp2(Au, T[name], False)
        s_flip = cmp2s(Au, T[name], True); s_raw = cmp2s(Au, T[name], False)
        print(f"   asset uv{k} vs {name}: idx flip {e_flip:.4f} idx raw {e_raw:.4f} | set flip {s_flip:.2e} set raw {s_raw:.3f}" + ("  <<< MATCH(flip)" if s_flip < 1e-4 else ("  <<< MATCH(raw)" if s_raw < 1e-4 else "")))
def cmp3s(A, B, flipx):
    key = (lambda p: (round(-p[0],5), round(p[1],5), round(p[2],5))) if flipx else (lambda p: (round(p[0],5), round(p[1],5), round(p[2],5)))
    A2 = sorted(key(a) for a in A); B2 = sorted(key(b) for b in B)
    return max(max(abs(a[i]-b[i]) for i in range(3)) for a, b in zip(A2, B2))
print("POSITION set vs -x(glb):", cmp3s(POSa, POSg, True), " vs raw:", cmp3s(POSa, POSg, False))

# ---- fragment simulation with CURRENT rebuild mapping ----
COMMON = r"C:\mfg\build\unity-project\Assets\MFG\Characters\Character1\3D\Common"
def png_decode(path):
    dd = open(path, "rb").read(); pos = 8; idat = b""
    while pos < len(dd):
        lnn, typ = struct.unpack(">I4s", dd[pos:pos+8]); pos += 8
        dataa = dd[pos:pos+lnn]; pos += lnn + 4
        if typ == b"IHDR": w, h, bd, ct, *_ = struct.unpack(">IIBBBBB", dataa)
        elif typ == b"IDAT": idat += dataa
        elif typ == b"IEND": break
    raw = zlib.decompress(idat)
    ch = {0:1,2:3,3:1,4:2,6:4}[ct]; stride = w*ch
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
def sample(img, u, v):
    w, h, ch, out = img
    # Unity convention: v=0 bottom. bilinear
    x = u*w - 0.5; y = (1-v)*h - 0.5  # wrap
    import math
    x0 = int(math.floor(x)); y0 = int(math.floor(y))
    fx = x - x0; fy = y - y0
    def px(xx, yy):
        xx %= w; yy %= h
        o = yy*w*ch + xx*ch
        return (out[o], out[o+1], out[o+2]) if ch >= 3 else (out[o], out[o], out[o])
    c00 = px(x0, y0); c10 = px(x0+1, y0); c01 = px(x0, y0+1); c11 = px(x0+1, y0+1)
    r = [[ (c00[k]*(1-fx)+c10[k]*fx)*(1-fy) + (c01[k]*(1-fx)+c11[k]*fx)*fy for k in range(3)] ]
    return tuple(int(round(c)) for c in r[0])

eye = png_decode(os.path.join(COMMON, "mdl_eye.png"))
emsk = png_decode(os.path.join(COMMON, "mdl_eye_msk.png"))
pup = png_decode(os.path.join(COMMON, "mdl_pupil_L.png"))
browmsk = png_decode(os.path.join(COMMON, "mdl_eyebrow_msk.png"))
brow = png_decode(os.path.join(COMMON, "mdl_eyebrow.png"))
print("brow solid sample:", sample(brow, 0.5, 0.5))

RSHAD = (0.5, 0.40060243, 0.3493976); BSHAD = (0.1, 0.1, 0.2)
def cell_orig(idx, cols, cs):
    return ((idx % cols)*cs, 1.0 - (idx//cols + 1)*cs)
def simulate(v):
    uv0 = auv(0, v); uv2 = auv(2, v); uv3 = auv(3, v)
    # CURRENT rebuild shader mapping
    eyeUV = (uv0[0], uv0[1]*0.25 + 0.75)
    pupilUV = (uv3[0], uv3[1]*0.5 + 0.5)
    browUV = (uv2[0], uv2[1]*0.25 + 0.75)
    em = sample(emsk, *eyeUV)
    col = list(sample(eye, *eyeUV))
    pcol = sample(pup, *pupilUV)
    g = min(1.0, em[1]/255)
    col = [col[k] + (pcol[k]-col[k])*g for k in range(3)]
    b = min(1.0, em[2]/255)
    col = [col[k]*(1 - b*(1-BSHAD[k])) for k in range(3)]
    r2 = min(1.0, em[0]/255)
    col = [col[k]*(1 - r2*(1-RSHAD[k])) for k in range(3)]
    bm = sample(browmsk, *browUV)[0]/255
    bc = sample(brow, *browUV)
    col = [col[k] + (bc[k]-col[k])*min(1.0, bm) for k in range(3)]
    return tuple(int(c) for c in col), em, tuple(int(c) for c in sample(eye, *eyeUV)), pcol

from collections import Counter
cnt = Counter()
for v in range(vc):
    c, em, ecol, pcol = simulate(v)
    def bucket(x):
        r, g, b = x
        if r > 200 and g > 200 and b > 190: return "near-white(>200)"
        if max(x)-min(x) < 25: return "achrome(>190)" if min(x) > 150 else ("achrome-dark(<80)" if max(x) < 80 else "achrome-mid")
        if r > 180 and 120 < g < 200 and b < 170: return "skin-tan"
        if r > 120 and g < 120 and b < 100: return "amber/warm"
        if max(x) < 90: return "dark/black"
        return "other"
    cnt[bucket(c)] += 1
print("CURRENT mapping sim vertex-color buckets:", dict(cnt))
# sample a few representative vertices: max uv0.x vertex, zero-uv0 vertices
import random
for v in range(0, vc, 40):
    c, em, ecol, pcol = simulate(v)
    print(f" v{v}: uv0={tuple(round(x,3) for x in auv(0,v))} eyeUV={tuple(round(x,3) for x in (auv(0,v)[0], auv(0,v)[1]*0.25+0.75))} eyeSample={ecol} emask={em} pupil={pcol} final={c}")

# ---- corrected mapping: eye from texcoord1 (Unity uv1), face-layout uv2, brow uv3, pupil uv0 ----
def simulate2(v):
    uv1 = auv(1, v); uv0 = auv(0, v); uv3 = auv(3, v)
    eyeUV = (uv1[0], uv1[1]*0.25 + 0.75)
    pupilUV = (uv0[0], uv0[1]*0.5 + 0.5)
    browUV = (uv3[0], uv3[1]*0.25 + 0.75)
    em = sample(emsk, *eyeUV)
    col = list(sample(eye, *eyeUV))
    pcol = sample(pup, *pupilUV)
    g = min(1.0, em[1]/255)
    col = [col[k] + (pcol[k]-col[k])*g for k in range(3)]
    b = min(1.0, em[2]/255)
    col = [col[k]*(1 - b*(1-BSHAD[k])) for k in range(3)]
    r2 = min(1.0, em[0]/255)
    col = [col[k]*(1 - r2*(1-RSHAD[k])) for k in range(3)]
    bm = sample(browmsk, *browUV)[0]/255
    bc = sample(brow, *browUV)
    col = [col[k] + (bc[k]-col[k])*min(1.0, bm) for k in range(3)]
    return tuple(int(c) for c in col)
cnt2 = Counter()
for v in range(vc):
    c = simulate2(v)
    r, g, b = c
    if r > 200 and g > 200 and b > 190: cnt2["near-white"] += 1
    elif max(c)-min(c) < 25: cnt2["achrome"] += 1
    elif r > 180 and 120 < g < 200 and b < 170: cnt2["skin-tan"] += 1
    elif r > 120 and g < 120 and b < 100: cnt2["amber/warm"] += 1
    elif max(c) < 90: cnt2["dark"] += 1
    else: cnt2["other"] += 1
print("CORRECTED mapping sim buckets:", dict(cnt2))
for v in (0, 40, 80, 120, 160, 200, 240):
    print(f" v{v}: corrected final={simulate2(v)} current final={simulate(v)[0]}")
