import re, struct, os, zlib
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
def px(img, u, v):  # Unity uv: v=0 bottom
    w, h, ch, out = img
    x = int(u*w) % w; y = int((1-v)*h) % h
    o = y*w*ch + x*ch
    return tuple(out[o:o+min(ch,4)])
eye = png_decode(os.path.join(COMMON, "mdl_eye.png"))
emsk = png_decode(os.path.join(COMMON, "mdl_eye_msk.png"))
pup = png_decode(os.path.join(COMMON, "mdl_pupil_L.png"))
bmsk = png_decode(os.path.join(COMMON, "mdl_eyebrow_msk.png"))

N = 24
def cellmap(img, gx, gy, chan, label):
    print(f"-- {label} -- (top row = PNG top = Unity v=1)")
    rows = []
    for r in range(N):
        line = ""
        for c in range(N):
            u = (gx + (c+0.5)/N) / 4.0
            v = 1.0 - ((gy + (r+0.5)/N) / 4.0)
            p = px(img, u, v)
            val = p[chan] if chan < len(p) else 255
            k = min(8, int(val/32))
            line += ".........:-=#@@"[k] if False else ".........:-=#@@"[k]
        rows.append(line)
    for r in range(N): print("   ", rows[r])
def huemap(img, gx, gy, label):
    print(f"-- {label} --")
    chars = {0:" ",1:"s",2:"a",3:"A",4:"d",5:"K",6:"w",7:"h"}
    for r in range(N):
        line = ""
        for c in range(N):
            u = (gx + (c+0.5)/N) / 4.0
            v = 1.0 - ((gy + (r+0.5)/N) / 4.0)
            R, G, B = px(img, u, v)[:3]
            if R > 215 and G > 205 and B > 195: chn = "w"
            elif max(R,G,B)-min(R,G,B) < 22: chn = "g" if min(R,G,B) > 140 else ("K" if max(R,G,B) < 60 else "x")
            elif R > 180 and G > 110 and B > 70: chn = "s"
            elif R > 110 and B < 90: chn = "a" if R > 170 else "A"
            elif max(R,G,B) < 75: chn = "K"
            else: chn = "o"
            line += chn
        print("   ", line)
cellmap(eye, 0, 0, 0, "eye cell(0,0) R")
huemap(eye, 0, 0, "eye cell(0,0) hue (s=skin a/A=amber K=black w=white g=gray)")
cellmap(emsk, 0, 0, 0, "eye_msk cell(0,0) R")
cellmap(emsk, 0, 0, 1, "eye_msk cell(0,0) G")
cellmap(emsk, 0, 0, 2, "eye_msk cell(0,0) B")
# pupil atlas 2x2
Np = 24
print("-- pupil_L cell(0,0) --")
for r in range(Np):
    line = ""
    for c in range(Np):
        u = (c+0.5)/Np * 0.5
        v = 1.0 - ((r+0.5)/Np * 0.5)
        R, G, B = px(pup, u, v)[:3]
        line += "K" if R+G+B < 240 else ("w" if R > 248 and G > 248 and B > 248 else ("." if abs(R-203)<12 and abs(G-203)<12 else ("+" if max(R,G,B)-min(R,G,B)>40 else "x")))
    print("   ", line)
print("pup_L cell(0,0) frac: bg203", end=" ")
tot = dark = light = col = 0
for r in range(512):
    for c in range(512):
        R, G, B = px(pup, c/1024, 1 - r/1024)[:3]
        tot += 1
        if R+G+B < 240: dark += 1
        if R > 248 and G > 248 and B > 248: light += 1
        if abs(R-203)<10 and abs(G-203)<10 and abs(B-203)<10: col = col  # bg counted later
print(f"dark={dark/tot:.3f} white>248={light/tot:.3f}")
# correlation: where emsk.g>127 in eye cell(0,0): eye hue and pupil art values
cnt = 0; esum = [0,0,0]; psum = [0,0,0]; e_dark = 0
for r in range(128):
    for c in range(128):
        u = (c+0.5)/128*0.25; v = 1 - (r+0.5)/128*0.25
        em = px(emsk, u, v)
        if len(em) > 1 and em[1] > 127:
            cnt += 1
            e = px(eye, u, v); esum = [esum[i]+e[i] for i in range(3)]
            pu = (c+0.5)/128*0.4243  # crude: pupil uv approx same rect (island covers iris)
            pv = 1 - (r+0.5)/128*0.9443
            p = px(pup, pu*1.0, pv*1.0)
            psum = [psum[i]+p[i] for i in range(3)]
            if sum(e[:3]) < 260: e_dark += 1
print(f"emask.g>0.5 px={cnt}/{128*128} meanEye={tuple(s//cnt for s in esum)} meanPupilAtSameUV={tuple(s//cnt for s in psum)} eyeDark%={100*e_dark/cnt:.1f}")
# uv0 zero count in asset
ap = r"C:\mfg\build\unity-project\Assets\MFG\Characters\Character1\3D\C001\Fbxs\model_face_eye_L.asset"
t = open(ap, encoding="utf-8", errors="ignore").read()
data = bytes.fromhex(re.search(r"_typelessdata: ([0-9a-fA-F]+)", t).group(1))
def af(o): return struct.unpack_from("<f", data, o)[0]
S1 = 271*40
z0 = sum(1 for v in range(271) if af(S1+v*32)==0 and af(S1+v*32+4)==0)
z3 = sum(1 for v in range(271) if af(S1+v*32+24)==0 and af(S1+v*32+28)==0)
print(f"verts with uv0==(0,0): {z0}/271, uv3==(0,0): {z3}/271")
# mdl_eye alpha channel stats cell(0,0)
amin = 255; a255 = 0; tot = 0
for r in range(512):
    for c in range(512):
        p = px(eye, c/2048, 1 - r/2048)
        if len(p) > 3:
            tot += 1
            if p[3] == 255: a255 += 1
print(f"eye cell(0,0) alpha==255 frac: {a255/tot:.3f}")
