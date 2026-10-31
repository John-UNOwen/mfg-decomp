import json, re, os
OUT = r"C:\mfg\build\_tmp_audit_face"
P = r"C:\mfg\build\export-primary-content\Assets\Toon\Shader\Toon_DoubleShadeWithFeather_TransClipping_StencilMask.json"
j = json.load(open(P, encoding="utf-8"))
passes = j["m_ParsedForm"]["m_SubShaders"][0]["m_Passes"]

def pass_map(pi, kind):
    pa = passes[pi]
    rev = {v: k for k, v in pa["m_NameIndices"].items()}
    cp = pa[kind]["m_CommonParameters"]
    regof = {}
    for b in cp["m_ConstantBufferBindings"]:
        regof[b["m_NameIndex"]] = b["m_Index"]
    comps = {}  # (reg, slot, comp) -> name
    for cb in cp["m_ConstantBuffers"]:
        nm_i = cb["m_NameIndex"]
        reg = regof.get(nm_i)
        cbname = rev.get(nm_i, "?")
        for vp in cb["m_VectorParams"]:
            off = vp["m_OffsetInConstantBuffer"]
            nm = rev.get(vp["m_NameIndex"], "?")
            slot = off // 16
            comp = (off % 16) // 4
            if vp["m_Dim"] == 1:
                key = (reg, slot, comp)
                comps[key] = nm
            else:
                for c in range(min(vp["m_Dim"], 4)):
                    key = (reg, slot, c)
                    if key not in comps:
                        comps[key] = nm
    return comps

CMP = "xyzw"
def annotate(asmfile, comps, outname):
    txt = open(asmfile, encoding="utf-8").read()
    def sub(m):
        reg, slot = int(m.group(1)), int(m.group(2))
        names = []
        for c in range(4):
            names.append(comps.get((reg, slot, c), "?"))
        # collapse
        s = "/".join(names)
        return "cb%d[%s]#%s#" % (reg, slot, s)
    out = re.sub(r"cb(\d+)\[(\d+)\]", sub, txt)
    open(outname, "w", encoding="utf-8").write(out)
    print("wrote", outname, len(out))

for pi, kind, asmf in [(1, "m_ProgFragment", "asm29.txt"), (1, "m_ProgFragment", "asm31.txt"),
                       (0, "m_ProgFragment", "asm05.txt"), (3, "m_ProgFragment", "asm82.txt"),
                       (2, "m_ProgFragment", "asm65.txt")]:
    t = pass_map(pi, kind)
    annotate(os.path.join(OUT, asmf), t, os.path.join(OUT, asmf.replace(".txt", "_c.txt")))
