import json, re, sys, os
OUT = r"C:\mfg\build\_tmp_audit_face"
P = r"C:\mfg\build\export-primary-content\Assets\Toon\Shader\Toon_DoubleShadeWithFeather_TransClipping_StencilMask.json"
j = json.load(open(P, encoding="utf-8"))
passes = j["m_ParsedForm"]["m_SubShaders"][0]["m_Passes"]

def pass_map(pi, kind):
    pa = passes[pi]
    rev = {v: k for k, v in pa["m_NameIndices"].items()}
    cp = pa[kind]["m_CommonParameters"]
    regof = {}  # cbuffer nameIndex -> reg
    for b in cp["m_ConstantBufferBindings"]:
        regof[b["m_NameIndex"]] = b["m_Index"]
    table = {}  # (reg, slot) -> label
    for cb in cp["m_ConstantBuffers"]:
        nm_i = cb["m_NameIndex"]
        reg = regof.get(nm_i)
        cbname = rev.get(nm_i, "?")
        for vp in cb["m_VectorParams"]:
            off = vp["m_OffsetInConstantBuffer"]
            nm = rev.get(vp["m_NameIndex"], "?")
            slot = off // 16
            key = (reg, slot)
            lab = ("%s.%s" % (cbname, nm)) if cbname not in ("$Globals",) else nm
            if key in table:
                table[key] += "|" + lab
            else:
                table[key] = lab
        for mp in cb["m_MatrixParams"]:
            nm = rev.get(mp["m_NameIndex"], "?")
            # matrices occupy 4 consecutive slots starting somewhere: find by matching offsets? m_MatrixParams lacks offset field in dump; probe:
    return table

def annotate(asmfile, table, outname):
    txt = open(asmfile, encoding="utf-8").read()
    def sub(m):
        reg, slot = int(m.group(1)), int(m.group(2))
        lab = table.get((reg, slot))
        return "cb%d[%s]%s" % (reg, slot, ("{%s}" % lab) if lab else "{?}")
    out = re.sub(r"cb(\d+)\[(\d+)\]", sub, txt)
    open(outname, "w", encoding="utf-8").write(out)
    print("wrote", outname)

for pi, kind, asmf in [(1, "m_ProgFragment", "asm29.txt"), (1, "m_ProgFragment", "asm31.txt"),
                       (1, "m_ProgFragment", "asm33.txt"), (1, "m_ProgFragment", "asm35.txt"),
                       (0, "m_ProgFragment", "asm05.txt"), (3, "m_ProgFragment", "asm82.txt"),
                       (2, "m_ProgFragment", "asm65.txt"), (2, "m_ProgFragment", "asm70.txt")]:
    src = os.path.join(OUT, asmf)
    if not os.path.exists(src):
        print("missing", asmf); continue
    t = pass_map(pi, kind)
    annotate(src, t, os.path.join(OUT, asmf.replace(".txt", "_ann.txt")))
