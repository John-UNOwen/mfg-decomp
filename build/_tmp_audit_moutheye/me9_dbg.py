import struct, os

for tag in ("mouth", "eye"):
    p = os.path.join(os.path.expandvars(r"C:\Users\%USERNAME%\Documents\GitHub\mfg-decomp\build\_tmp_audit_moutheye"), tag + "_blob.bin")
    g = open(p, "rb").read()
    hdrn, = struct.unpack_from("<I", g, 0)
    print(tag, "len:", len(g), "regions:", hdrn)
    ents = [struct.unpack_from("<3I", g, 4 + i * 12) for i in range(hdrn)]
    bad = [i for i, (o, s, f) in enumerate(ents) if o + s > len(g)]
    print("  out-of-range regions:", bad[:10])
    total_dx = g.count(b"DXBC")
    print("  total DXBC occurrences:", total_dx)
    found = 0
    first_bad = []
    for i, (o, s, f) in enumerate(ents):
        dx = g.find(b"DXBC", o, o + min(s, 200))
        if dx >= 0:
            found += 1
        elif i in (2, 8, 17, 20, 40, 45, 50):
            first_bad.append((i, o, s, g.find(b"DXBC", o, o + min(s, 5000))))
    print("  found in 200B window:", found, "sample bad entries:", first_bad)
    # show entries around known program regions 2, 8
    for i in (1, 2, 3, 6, 7, 8, 9):
        if i < hdrn:
            print("   region", i, ents[i])
