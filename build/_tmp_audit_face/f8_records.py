import json, struct, re, os
OUT = r"C:\mfg\build\_tmp_audit_face"
g = open(os.path.join(OUT,"face_blob.bin"),"rb").read()
hdr = struct.unpack_from("<I", g, 0)[0]
print("blob len", len(g), "header dword", hdr)
# read (offset,size,0) table
def read_table(start):
    ents = []
    o = start
    while True:
        off, size, flg = struct.unpack_from("<3I", g, o)
        if flg != 0 or (off, size) == (0,0): break
        ents.append((off, size))
        o += 12
        if o > len(g): break
    return ents
ents = read_table(4)
print("table entries (raw stream):", len(ents))
# dedupe consecutive
uniq = []
for e in ents:
    if not uniq or uniq[-1] != e: uniq.append(e)
print("unique regions:", len(uniq))
print("first 6:", uniq[:6], "last 3:", uniq[-3:])
print("coverage: last region end:", uniq[-1][0]+uniq[-1][1])
# Now per region: classify
recs = []
for idx,(off,size) in enumerate(uniq):
    dx = g.find(b"DXBC", off, off+size)
    tag = None
    if dx >= 0:
        ver, ln, nsec = struct.unpack_from("<3I", g, dx+20)
        tag = (dx, ln, nsec)
    recs.append((idx, off, size, tag))
progs = [r for r in recs if r[3]]
print("regions:", len(recs), "program regions:", len(progs), "small/meta:", [ (r[1],r[2]) for r in recs if not r[3] ])
# hexdump one program header + DXBC head
off,size,tag = 1420, 2888, None
print("== region @1420 header 114B ==")
for i in range(1420,1420+114,16):
    b=g[i:i+16]
    print(f"{i:6d}  {b.hex()}   {''.join(chr(c) if 32<=c<127 else '.' for c in b)}")
print("== DXBC @1534 first 96 bytes ==")
for i in range(1534,1534+96,16):
    b=g[i:i+16]
    print(f"{i:6d}  {b.hex()}   {''.join(chr(c) if 32<=c<127 else '.' for c in b)}")
print("dwords at 1534+32 (offset table, 3):", struct.unpack_from("<3I", g, 1534+32))
