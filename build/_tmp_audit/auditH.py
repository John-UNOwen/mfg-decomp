import struct, re, os
OUT = r"C:\mfg\build\_tmp_audit"
g = open(os.path.join(OUT, "shader_blob.bin"), "rb").read()

def strings_near(off, n=400):
    seg = g[off:off+n]
    return [(off+m.start(), m.group().decode("latin-1")) for m in re.finditer(rb"[\x20-\x7e]{3,}", seg)]

def parse_sgn(off):
    tag = g[off:off+4].decode("latin-1", "ignore")
    size, = struct.unpack_from("<I", g, off+4)
    body = g[off+8:off+8+size-8]
    if tag == "ISGN":
        streamCount = struct.unpack_from("<I", body, 0)[0]
        base = 4
        if size > 40:  # D3D11: extra fields
            pass
        # try 28-byte entries first, then 24
        for esz in (28, 24):
            hdr = 4 if tag == "ISGN" else 0
            nEnt = struct.unpack_from("<I", body, 0)[0]
            break
        nEnt = struct.unpack_from("<I", body, 4)[0]  # streamCount then entryCount? probe
        # brute: assume [streamCount u32][entryCount u32][entries]
        nEnt = struct.unpack_from("<I", body, 4)[0]
        esz = (len(body) - 8) // nEnt if nEnt else 0
        out = []
        for i in range(nEnt):
            e = body[8+i*esz:8+(i+1)*esz]
            nameOff, index, kind, fmt, stream, reads = struct.unpack_from("<6I", e, 0)[:6]
            name = g[off+size+nameOff:g.index(b"\x00", off+size+nameOff)].decode("latin-1")
            out.append((name, index, kind, fmt, stream, reads))
        return tag, nEnt, esz, out
    return tag, 0, 0, []

def show(off, label):
    try:
        tag, n, esz, ents = parse_sgn(off)
        print(f"== {label}: {tag} entries={n} entrysize={esz}")
        for e in ents: print("    ", e)
    except Exception as ex:
        print(f"== {label} parse fail:", ex)
        print("   raw dwords:", struct.unpack_from("<" + "I"*((1686-off)//4), g, off))

for m in re.finditer(rb"ISGN", g):
    pass
# locate all ISGN and OSGN offsets
sgs = [m.start() for m in re.finditer(rb"ISGN", g)]
ogs = [m.start() for m in re.finditer(rb"OSGN", g)]
stn = [m.start() for m in re.finditer(rb"STENCIL_MASK", g)]
eye_mode = [m.start() for m in re.finditer(rb"EYE_MODE", g)]
print("ISGN:", sgs)
print("OSGN:", ogs)
print("STENCIL_MASK tables at:", stn)
print("EYE_MODE tables at:", eye_mode)
