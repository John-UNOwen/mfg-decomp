import json, base64, os, struct
import lz4.block

HERE = os.path.expandvars(r"C:\Users\%USERNAME%\Documents\GitHub\mfg-decomp\build\_tmp_audit_moutheye")
SRC = os.path.expandvars(rf"C:\Users\%USERNAME%\Documents\GitHub\mfg-decomp\build\export-primary-content\Assets\Shaders\Character\{{name}}.json")

for name in ("Mouth", "Eye"):
    j = json.load(open(SRC.format(name=name), encoding="utf-8"))
    raw = base64.b64decode(j["m_CompressedBlob"])
    target = j["m_DecompressedLengths"][0][0]
    got = None
    for skip in (0, 2, 4, 6, 8, 12):
        try:
            got = lz4.block.decompress(raw[skip:], uncompressed_size=target)
            print(name, "lz4 ok skip", skip, "len", len(got))
            break
        except Exception as e:
            print(name, "skip", skip, "->", str(e)[:80])
    assert got, name
    open(os.path.join(HERE, f"{name.lower()}_blob.bin"), "wb").write(got)
    g = got
    hdr = struct.unpack_from("<I", g, 0)[0]
    print(name, "u32@0 =", hdr)
    # region-table hypothesis: hdr entries of {u32 off,size,flags=0} starting at 4
    ents, bad = [], 0
    for i in range(hdr):
        o = 4 + i * 12
        off, size, flg = struct.unpack_from("<3I", g, o)
        ents.append((off, size, flg))
        if flg != 0:
            bad += 1
    offs = sorted(e[0] for e in ents)
    print(name, "table entries:", len(ents), "nonzero flags:", bad,
          "first:", ents[0], "min off:", offs[0], "data start guess:", 4 + 12 * hdr,
          "last end:", max(o + s for o, s, _ in ents), "blob len:", len(g))
    contiguous = all(ents[i][0] + ents[i][1] == ents[i + 1][0] for i in range(len(ents) - 1))
    print(name, "regions contiguous:", contiguous)
    # DXBC count fallback
    n_dx = g.count(b"DXBC")
    print(name, "DXBC occurrences:", n_dx)
