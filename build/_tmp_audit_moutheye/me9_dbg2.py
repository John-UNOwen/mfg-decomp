import struct, os, json

g = open(os.path.join(os.path.expandvars(r"C:\Users\%USERNAME%\Documents\GitHub\mfg-decomp\build\_tmp_audit_moutheye"), "mouth_blob.bin"), "rb").read()
hdrn, = struct.unpack_from("<I", g, 0)
ents = [struct.unpack_from("<3I", g, 4 + i * 12) for i in range(hdrn)]
for i in (2, 3, 8, 9, 40, 54):
    off, size, flg = ents[i]
    dx = g.find(b"DXBC", off, off + size)
    ln, nsec = struct.unpack_from("<2I", g, dx + 24)
    print("region", i, "dx@", dx, "nsec", nsec, "len", ln)
    for si in range(nsec):
        so, = struct.unpack_from("<I", g, dx + 32 + si * 4)
        tag = g[dx + so:dx + so + 4].decode("latin-1", "ignore")
        ssize, = struct.unpack_from("<I", g, dx + so + 4)
        head = struct.unpack_from("<4I", g, dx + so)
        print("   sec", tag, "size", ssize, "head", [hex(x) for x in head])
