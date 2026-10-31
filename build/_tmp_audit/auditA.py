import os, re, json, zlib, struct, sys
sys.setrecursionlimit(10000)
ROOT = r"C:\mfg\build\unity-project\Assets"
COMMON = os.path.join(ROOT, r"MFG\Characters\Character1\3D\Common")

GUIDS = {
 "5a506ce52672e604a95b8367a20454cb":"EyeTex(L)",
 "a8f6314026fbe8d4fb6239b1791b05ab":"EyeMaskTex",
 "62c8ea874ac3a394b9f5e82552a7783c":"PupilTex L",
 "c1955ac25fca31c408e56f29324201a9":"PupilTex R",
 "bc20677c17e2e8a48bb622aec7697e6d":"EyeBrowTex",
 "b51bf74e00b67fb44bf3d90af6302b0e":"EyeBrowMaskTex",
 "a463f365d80ee26419310da4951ee9b1":"MainTex",
 "21d07f72b80a5f846ba2efdf65965fe4":"MaskTex",
 "3f41381436a22054aa1a9aefa70aea1f":"MaterialMaskTex",
 "26b1ec7257886c040ae8d01c3bb4bce3":"Eye.shader",
}
pat = re.compile(r"^guid: ([0-9a-f]{32})", re.M)
hits = {}
for dirpath, dirs, files in os.walk(ROOT):
    for fn in files:
        if fn.endswith(".meta"):
            p = os.path.join(dirpath, fn)
            try:
                t = open(p, "r", encoding="utf-8", errors="ignore").read(2000)
            except Exception:
                continue
            m = pat.search(t)
            if m and m.group(1) in GUIDS:
                hits[GUIDS[m.group(1)]] = p[:-5]
print("== GUID MAP ==")
for k, v in GUIDS.items():
    print(v, "->", hits.get(v, "NOT FOUND"))

# ---------- PNG decode (stdlib) ----------
def png_decode(path):
    d = open(path, "rb").read()
    assert d[:8] == b"\x89PNG\r\n\x1a\n", path
    pos = 8; idat = b""; pal = None; trns = None
    while pos < len(d):
        ln, typ = struct.unpack(">I4s", d[pos:pos+8]); pos += 8
        data = d[pos:pos+ln]; pos += ln + 4
        if typ == b"IHDR":
            w, h, bd, ct, comp, filt, inter = struct.unpack(">IIBBBBB", data)
        elif typ == b"PLTE": pal = data
        elif typ == b"tRNS": trns = data
        elif typ == b"IDAT": idat += data
        elif typ == b"IEND": break
    raw = zlib.decompress(idat)
    ch = {0:1, 2:3, 3:1, 4:2, 6:4}[ct]
    assert bd == 8, ("bitdepth", bd)
    bpp = ch
    stride = w*bpp
    out = bytearray(h*stride)
    prev = bytearray(stride)
    rp = 0
    for y in range(h):
        f = raw[rp]; rp += 1
        line = bytearray(raw[rp:rp+stride]); rp += stride
        if f == 1:
            for i in range(bpp, stride): line[i] = (line[i]+line[i-bpp]) & 255
        elif f == 2:
            for i in range(stride): line[i] = (line[i]+prev[i]) & 255
        elif f == 3:
            for i in range(stride):
                a = line[i-bpp] if i>=bpp else 0
                line[i] = (line[i]+((a+prev[i])>>1)) & 255
        elif f == 4:
            for i in range(stride):
                a = line[i-bpp] if i>=bpp else 0
                b = prev[i]; c = prev[i-bpp] if i>=bpp else 0
                pp = a+b-c; pa=abs(pp-a); pb=abs(pp-b); pc=abs(pp-c)
                pr = a if (pa<=pb and pa<=pc) else (b if pb<=pc else c)
                line[i] = (line[i]+pr) & 255
        out[y*stride:(y+1)*stride] = line
        prev = line
    return w, h, ct, out

def px(img, x, y):
    w, h, ct, out = img
    stride = w*({0:1,2:3,3:1,4:2,6:4}[ct])
    o = y*stride + x*({0:1,2:3,3:1,4:2,6:4}[ct])
    c = out[o:o+({0:1,2:3,3:1,4:2,6:4}[ct])]
    if ct == 6: return (c[0], c[1], c[2], c[3])
    if ct == 2: return (c[0], c[1], c[2], 255)
    if ct == 0: return (c[0], c[0], c[0], 255)
    if ct == 4: return (c[0], c[0], c[0], c[1])
    return (c[0], c[0], c[0], 255)

def cell_stats(img, gx, gy, gx0, gy0):
    w, h, ct, out = img
    cw, chh = w//gx, h//gy
    x0, y0 = gx0*cw, gy0*chh
    n = 0; sR=sG=sB=sA=0
    colored = 0; opaque = 0; transp = 0
    bg = px(img, x0+1, y0+1)
    diffbg = 0
    for y in range(y0, min(y0+chh, h)):
        for x in range(x0, min(x0+cw, w)):
            r, g, b, a = px(img, x, y)
            n += 1; sR+=r; sG+=g; sB+=b; sA+=a
            if max(r,g,b)-min(r,g,b) > 40: colored += 1
            if a > 10: opaque += 1
            if a <= 10: transp += 1
            if abs(r-bg[0])+abs(g-bg[1])+abs(b-bg[2]) > 24: diffbg += 1
    return dict(mean=(round(sR/n), round(sG/n), round(sB/n), round(sA/n)),
                colored=round(100*colored/n,1), opaque=round(100*opaque/n,1),
                diffbg=round(100*diffbg/n,1), bg=bg)

def probe(name, path, gx, gy, label):
    print(f"\n== {label}: {path} ==")
    img = png_decode(path)
    w, h, ct, _ = img
    print(f"  dims {w}x{h} colortype {ct}")
    for gy in range(min(gy, 4) if label.startswith("EyeAtlas") else gy):
        pass
    for row in range(gy):
        for col in range(gx):
            st = cell_stats(img, gx, gy, col, row)
            print(f"  cell(col={col},row={row}) meanRGBA={st['mean']} colored%={st['colored']} opaque%={st['opaque']} diffBG%={st['diffbg']} bg={st['bg']}")
    return img

P = lambda f: os.path.join(COMMON, f)
img_eye = probe("eye", P("mdl_eye.png"), 4, 4, "EyeAtlas mdl_eye.png")
probe("eyemsk", P("mdl_eye_msk.png"), 4, 4, "EyeMask mdl_eye_msk.png")
probe("pupilL", P("mdl_pupil_L.png"), 2, 2, "Pupil mdl_pupil_L.png")
probe("pupilR", P("mdl_pupil_R.png"), 2, 2, "Pupil mdl_pupil_R.png")
probe("brow", P("mdl_eyebrow.png"), 1, 1, "Eyebrow mdl_eyebrow.png")
probe("browmsk", P("mdl_eyebrow_msk.png"), 4, 4, "EyebrowMask mdl_eyebrow_msk.png")

# face composite: stats over eye-L face-layout rect x=[0.5,0.7241] y=[0.4344,0.7238]
img_face = png_decode(P("mdl_face.png"))
w, h, ct, _ = img_face
print(f"\n== mdl_face.png dims {w}x{h} ct {ct} ==")
x0, x1 = int(0.50*w), int(0.7241*w); y0, y1 = int((1-0.7238)*h), int((1-0.4344)*h)
n=0; sR=sG=sB=sA=0; transp=0
for y in range(y0, y1, max(1,(y1-y0)//64)):
    for x in range(x0, x1, max(1,(x1-x0)//64)):
        r,g,b,a = px(img_face, x, y); n+=1; sR+=r; sG+=g; sB+=b; sA+=a
        if a<=10: transp+=1
print(f"  eye-L face-layout rect meanRGBA=({sR//n},{sG//n},{sB//n},{sA//n}) transparent%={round(100*transp/n,1)}")

# resolved mask textures
for label, path in hits.items():
    if label in ("MaskTex", "MaterialMaskTex") and path and path.endswith(".png"):
        probe(label, path, 1, 1, label)

# import settings of key textures
for f in ("mdl_eye.png", "mdl_eye_msk.png", "mdl_pupil_L.png"):
    meta = P(f) + ".meta"
    t = open(meta, "r", encoding="utf-8", errors="ignore").read()
    keys = [l.strip() for l in t.splitlines() if re.search(r"m_BuildTargetOverrides|m_Compression|m_MipCount|m_EnableMipmaps|m_sRGBTexture|m_TextureCompression|m_IgnoreMasterTextureLimit|m_Readable|m_NPOTScale|m_SpriteMode|crunched|m_WrapMode|m_FilterMode", l, re.I)]
    print(f"\n== meta {f} key settings ==")
    for k in keys: print("  ", k)

# Eye.json structure
t = open(r"C:\mfg\build\export-primary-content\Assets\Shaders\Character\Eye.json", "r", encoding="utf-8", errors="ignore").read()
print(f"\n== Eye.json size {len(t)} ==")
for kw in ("HLSLSOURCE","Varyings","m_Scripts","m_ParsedForm","Snippet",'"code"',"GpuProgramType","m_Properties","propName","m_Name"):
    print(f"  contains {kw}: {t.count(kw)}")
try:
    j = json.loads(t)
    def walk(o, depth=0, path=""):
        if depth > 2: return
        if isinstance(o, dict):
            for k, v in o.items():
                print("  "*depth + f"{path+k}: {type(v).__name__}" + (f" len={len(v)}" if isinstance(v,(list,dict,str)) else f" = {v}"))
                if isinstance(v,(dict,)) and depth<2: walk(v, depth+1, "")
                if isinstance(v, list) and v and depth<2 and isinstance(v[0], dict):
                    walk(v[0], depth+1, "[0].")
    walk(j)
except Exception as e:
    print("  json parse fail:", e)
print("  property-style names found:", sorted(set(re.findall(r"_(?:[A-Za-z0-9_]*(?:Eye|Iris|Pupil|Brow|Stencil|Color|Tex|Brightness|Contrast|Chroma)[A-Za-z0-9_]*)", t)))[:80])
