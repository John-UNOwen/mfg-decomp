import struct, re, os
OUT = r"C:\mfg\build\_tmp_audit"
g = open(os.path.join(OUT, "shader_blob.bin"), "rb").read()

def parse_sgn(off):
    size, = struct.unpack_from("<I", g, off+4)
    data = g[off+8:off+8+size]
    nEnt, psize = struct.unpack_from("<2I", data, 0)
    esz = psize // nEnt
    names = data[8+psize:]
    ents = []
    for i in range(nEnt):
        e = data[8+i*esz: 8+(i+1)*esz]
        nameOff, index = struct.unpack_from("<2I", e, 0)
        nm = names[nameOff:names.index(b"\x00", nameOff)].decode("latin-1")
        rest = struct.unpack_from("<" + "I"*(esz//4-2), e, 8)
        ents.append((nm, index) + rest)
    return g[off:off+4].decode("latin-1"), nEnt, esz, ents

def show(off, label):
    tag, n, esz, ents = parse_sgn(off)
    print(f"== {label} @{off}: {tag} entries={n} esz={esz}")
    for e in ents: print("    ", e)

for off, label in [
    (1502, "FORWARD vert IN"), (1722, "FORWARD vert OUT"), (16342, "FORWARD frag IN"),
    (195134, "EYE-pass vert IN"), (195354, "EYE-pass vert OUT"), (197326, "EYE-pass frag IN"), (197606, "EYE-pass frag OUT"),
]:
    try: show(off, label)
    except Exception as ex: print("fail", label, ex)

print("\n== region 194200-197400 strings ==")
seg = g[194200:197400]
for m in re.finditer(rb"[\x20-\x7e]{4,}", seg):
    print(f"{194200+m.start():7d}  {m.group().decode('latin-1')}")
