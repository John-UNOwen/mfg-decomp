import struct, re, os
OUT = r"C:\mfg\build\_tmp_audit"
g = open(os.path.join(OUT, "shader_blob.bin"), "rb").read()

def parse_sgn(off):
    tag = g[off:off+4].decode("latin-1", "ignore")
    size, = struct.unpack_from("<I", g, off+4)
    body = g[off+8:off+size]
    nEnt, = struct.unpack_from("<I", body, 0)
    # D3D11 vertex-input signature: [u32 entryCount? streamCount][u32 entryCount][u32 structSize=8?]...
    # probe: entries occupy body[hdr:]; string table follows at end.
    # find where strings start: first occurrence of first name in body
    # standard D3D11 ISGN: uint StreamCount? Actually: [entryCount][pad/struct 8][entries 28B][names]
    for hdr, esz in ((12, 28), (12, 24), (8, 28), (8, 24), (16, 28), (16, 24)):
        remain = len(body) - hdr - esz * nEnt
        if remain <= 0: continue
        ok = True
        ents = []
        for i in range(nEnt):
            e = body[hdr+i*esz: hdr+(i+1)*esz]
            vals = struct.unpack_from("<6I", e, 0)
            nameOff, index = vals[0], vals[1]
            if nameOff >= remain:  # nameOff is relative to end-of-entries = start of names table
                ok = False; break
            nstart = hdr + esz*nEnt + nameOff
            if nstart >= len(body): ok = False; break
            end = body.index(b"\x00", nstart)
            ents.append((body[nstart:end].decode("latin-1"), index, vals[2], vals[3], vals[4], vals[5]))
        if ok and all(re.match(r"^[A-Za-z_]", e[0]) for e in ents):
            return tag, nEnt, esz, hdr, ents
    return tag, nEnt, None, None, []

def show(off, label):
    tag, n, esz, hdr, ents = parse_sgn(off)
    print(f"== {label} @{off}: {tag} entries={n} esz={esz} hdr={hdr}")
    for e in ents: print("    ", e)

for off, label in [
    (1502, "FORWARD vert IN"), (1722, "FORWARD vert OUT"), (16342, "FORWARD frag IN"),
    (195134, "Eye-pass vert IN"), (195354, "Eye-pass vert OUT"), (197326, "Eye-pass frag IN"), (197606, "Eye-pass frag OUT"),
    (204810, "silhouette? vert IN"), (205542, "? IN"),
]:
    show(off, label)

print("\n== Eye-pass region strings 193000-200000 ==")
seg = g[193000:200000]
for m in re.finditer(rb"[\x20-\x7e]{4,}", seg):
    print(f"{193000+m.start():7d}  {m.group().decode('latin-1')}")
