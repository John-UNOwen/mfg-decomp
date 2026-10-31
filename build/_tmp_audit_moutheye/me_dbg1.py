import struct, os, re
HERE = os.path.expandvars(r"C:\Users\%USERNAME%\Documents\GitHub\mfg-decomp\build\_tmp_audit_moutheye")
for name in ("Mouth", "Eye"):
    g = open(os.path.join(HERE, f"{name.lower()}_blob.bin"), "rb").read()
    n = struct.unpack_from("<I", g, 0)[0]
    ents = [struct.unpack_from("<3I", g, 4 + i * 12) for i in range(n)]
    dxs = [m.start() for m in re.finditer(rb"DXBC", g)]
    print("==", name, "regions:", n, "dxbc:", len(dxs))
    # which region contains each DXBC
    import bisect
    starts = [e[0] for e in ents]
    owners = [bisect.bisect_right(starts, d) - 1 for d in dxs]
    from collections import Counter
    c = Counter(owners)
    print("   dxbc-per-region (idx:count):", dict(sorted(c.items())))
    print("   region table (idx,off,size,hasdxbc):")
    for i, (off, size, flg) in enumerate(ents):
        d = [dx for dx in dxs if off <= dx < off + size]
        print(f"     {i:2d} {off:6d} {size:6d} dx={d}")
