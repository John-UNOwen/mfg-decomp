import json, struct, re, os
OUT = r"C:\mfg\build\_tmp_audit_face"
g = open(os.path.join(OUT,"face_blob.bin"),"rb").read()
ents = []
o = 4
while o <= 4+83*12:
    off, size, flg = struct.unpack_from("<3I", g, o)
    if flg != 0: break
    ents.append((off,size)); o += 12
def parse_kw(off):
    d = struct.unpack_from("<8I", g, off)
    kws = []; p = off+28
    while True:
        if p+4 > len(g): break
        L, = struct.unpack_from("<I", g, p)
        if L==0 or L>200: break
        s = g[p+4:p+4+L].decode("latin-1")
        if not re.match(r"^[A-Za-z0-9_]+$", s): break
        kws.append(s); p += 4+L
    return d, kws, p
progs=[]
for i,(off,size) in enumerate(ents):
    dx = g.find(b"DXBC", off, off+min(size,200))
    if dx<0: continue
    ln, nsec = struct.unpack_from("<2I", g, dx+24)
    secs={}
    for si in range(nsec):
        so, = struct.unpack_from("<I", g, dx+32+si*4)
        tag = g[dx+so:dx+so+4].decode("latin-1","ignore")
        ssize, = struct.unpack_from("<I", g, dx+so+4)
        secs[tag]=(dx+so, ssize)
    hdr, kws, kend = parse_kw(off)
    so, ss = secs["SHDR"]
    # SHDR data: dump first 12 dwords for progs 0..2 later; version token = data[0]?
    vtok, vword = struct.unpack_from("<2I", g, so+8)
    styp = {0x9:"VS",0x11:"PS",0x19:"GS",0x21:"HS",0x29:"DS",0x31:"CS"}.get(vword & 0xFFFF, hex(vword & 0xFFFF))
    mmaj, mmin = (vword>>20)&0xF, (vword>>16)&0xF
    stage = f"{styp}_{mmaj}{mmin}"
    progs.append(dict(idx=i, region=off, size=size, dx=dx, dxbc_len=ln, hdr=hdr, kws=kws, stage=stage, vtok=vtok, vword=vword, secs={t:(s-s0 if False else s, ss) for t,(s,ss) in secs.items()}, isgn=secs["ISGN"], osgn=secs["OSGN"], shdr=secs["SHDR"]))
print("programs:", len(progs))
from collections import Counter
print("stage counts:", dict(Counter(p["stage"] for p in progs)))
for pr in progs:
    print(f"P{pr['idx']:02d} @{pr['region']:6d} sz={pr['size']:6d} stage={pr['stage']:8s} ISGN={pr['isgn'][1]:4d} OSGN={pr['osgn'][1]:4d} SHDR={pr['shdr'][1]:6d} vtok={hex(pr['vtok'])} vword={hex(pr['vword'])}")
    print(f"     kws({len(pr['kws'])}): {pr['kws']}")
json.dump([{k:str(v) for k,v in pr.items() if k!='secs'} for pr in progs], open(os.path.join(OUT,"progs2.json"),"w"))
