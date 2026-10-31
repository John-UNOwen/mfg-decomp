import json, struct, re, os, ctypes
from ctypes import *
OUT = r"C:\mfg\build\_tmp_audit_face"
g = open(os.path.join(OUT,"face_blob.bin"),"rb").read()

# ---- region table ----
ents = []
o = 4
while True:
    off, size, flg = struct.unpack_from("<3I", g, o)
    if flg != 0 or o >= 4+84*12: break
    ents.append((off,size)); o += 12
print("table entries:", len(ents), "first", ents[0], "last real", ents[-1], "sum end", ents[-1][0]+ents[-1][1]==len(g))

def parse_kw(off):
    # header dwords then keyword stream: [u8 len? no: u32 len][chars] repeated until terminator dword looks wrong
    d = struct.unpack_from("<8I", g, off)
    kws = []; p = off+32  # after 'u32 nKwBytes' at +28
    nb = d[7]
    p0 = off+32
    p = p0
    while True:
        L = g[p]
        if L==0 or L>64: break
        s = g[p+1:p+1+L].decode("latin-1")
        if not re.match(r"^[A-Za-z0-9_]+$", s): break
        kws.append(s); p += 1+L
    return d, kws, p

# ---- DXBC parse ----
SECT = set()
progs = []
for i,(off,size) in enumerate(ents):
    dx = g.find(b"DXBC", off, off+min(size,200))
    if dx < 0:
        continue
    ver1, ln, nsec = struct.unpack_from("<3I", g, dx+20)
    secs = {}
    for si in range(nsec):
        so, = struct.unpack_from("<I", g, dx+32+si*4)
        tag = g[dx+so:dx+so+4].decode("latin-1", "ignore")
        ssize, = struct.unpack_from("<I", g, dx+so+4)
        secs[tag] = (dx+so, ssize)
        SECT.add(tag)
    hdr, kws, kend = parse_kw(off)
    # microcode section: find version dword = first dword of microcode section data (+8)
    stage = None; vword = None
    for t,(so,ss) in secs.items():
        if t not in ("ISGN","OSGN","ISG1","OSG1","OSG2","SHEB","R5SO","R5IN","FCFB","STAT","PRIM","TEXC","SASI"):
            vword, = struct.unpack_from("<I", g, so+8)
            t_ = (vword & 0xFFF8) >> 3
            stage = {1:"VS",2:"PS",3:"GS",4:"HS",5:"DS",6:"CS"}.get(t_, hex(t_))
            mmaj, mmin = (vword>>24)&0xF, (vword>>20)&0xF
            stage = f"{stage}_{mmaj}_{mmin}"
    progs.append(dict(idx=i, region=off, size=size, dx=dx, dxbc_len=ln, hdr=hdr, kws=kws, secs=sorted(secs.keys()), stage=stage, vword=vword, secmap=secs))
print("programs:", len(progs))
print("section tags seen:", sorted(SECT))
stages = {}
for pr in progs: stages.setdefault(pr["stage"], []).append(pr["idx"])
print("stages:", {k:len(v) for k,v in stages.items()})
for pr in progs[:12]:
    print(f"P{pr['idx']:02d} region@{pr['region']} size={pr['size']} dx@{pr['dx']} len={pr['dxbc_len']} stage={pr['stage']} hdr={pr['hdr']}")
    print(f"    kws({len(pr['kws'])}): {pr['kws']}")
json.dump([{k:v for k,v in pr.items() if k!='secmap'} for pr in progs], open(os.path.join(OUT,"progs.json"),"w"), default=str)

# ---- save each program ----
for pr in progs:
    open(os.path.join(OUT, f"prog{pr['idx']:02d}_{pr['stage']}.dxbc"), "wb").write(g[pr['dx']:pr['dx']+pr['dxbc_len']])
