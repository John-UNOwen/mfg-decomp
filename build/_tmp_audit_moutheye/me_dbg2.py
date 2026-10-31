import struct, os
HERE = os.path.expandvars(r"C:\Users\%USERNAME%\Documents\GitHub\mfg-decomp\build\_tmp_audit_moutheye")
g = open(os.path.join(HERE, "mouth_blob.bin"), "rb").read()

def hdr(off, size, tag):
    print("==", tag, f"@{off} size={size}")
    seg = g[off:off + min(size, 340)]
    # dwords
    dws = struct.unpack_from("<" + "I" * (len(seg) // 4), seg, 0)
    for i in range(0, len(dws), 8):
        row = dws[i:i + 8]
        asc = "".join(chr(c) if 32 <= c < 127 else "." for c in struct.pack("<" + "I" * len(row), *row))
        print(f"  {off + i * 4:6d}: " + " ".join(f"{c:10d}" for c in row) + "  |" + asc + "|")

hdr(1100, 340, "Mouth region2 head (dx at 1426)")
hdr(190956, 324, "Mouth region61 full")
print("\nSHDR area of region2 dx=1426:")
import re
dx = 1426
ln, nsec = struct.unpack_from("<2I", g, dx + 24)
print("dxbc len word:", ln, "nsec:", nsec)
for si in range(nsec):
    so, = struct.unpack_from("<I", g, dx + 32 + si * 4)
    tag = g[dx + so:dx + so + 4].decode("latin-1", "ignore")
    ssize, = struct.unpack_from("<I", g, dx + so + 4)
    print(f"  sec {si}: tag={tag} off={dx + so} size={ssize}")
    if tag == "SHDR":
        print("   first 8 dwords:", struct.unpack_from("<8I", g, dx + so + 8))
