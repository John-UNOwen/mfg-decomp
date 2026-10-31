import json, struct, re, os
OUT = r"C:\mfg\build\_tmp_audit_face"
g = open(os.path.join(OUT,"face_blob.bin"),"rb").read()
print("== first 120 dwords ==")
for i in range(0, 480, 4):
    vals = struct.unpack_from("<" + "I"*4, g, i)
    print(i, " ".join(f"{v:10d}" for v in vals))
print("== bytes 0..64 hex:", g[:64].hex())
print("== first DXBC hdr @1534:", g[1534:1538], "ver", hex(struct.unpack_from("<I",g,1534+20)[0]), "len", struct.unpack_from("<I",g,1534+24)[0], "nsec", struct.unpack_from("<I",g,1534+28)[0])
d = g[1534:1534+29+4*struct.unpack_from("<I",g,1534+28)[0]]
secs=[]
for si in range(struct.unpack_from("<I",g,1534+28)[0]):
    so, ss = struct.unpack_from("<2I", g, 1534+29+si*8)
    tag = g[1534+29+si*8-4:1534+29+si*8]
    secs.append((tag, so, ss))
print("sections:", secs)
