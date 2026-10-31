import json
p = r"C:\mfg\build\export-primary-content\Assets\Toon\Shader\Toon_DoubleShadeWithFeather_TransClipping_StencilMask.json"
j = json.load(open(p, encoding="utf-8"))
passes = j["m_ParsedForm"]["m_SubShaders"][0]["m_Passes"]
for pi, pa in enumerate(passes):
    ni = pa["m_NameIndices"]
    rev = {v: k for k, v in ni.items()}
    print("== PASS[%d] nameIndices count=%d ; sample values:" % (pi, len(ni)), dict(list(ni.items())[:6]))
    for kind in ("m_ProgVertex", "m_ProgFragment"):
        cp = pa[kind]["m_CommonParameters"]
        print("  -- %s" % kind)
        for b in cp["m_ConstantBufferBindings"]:
            nm = rev.get(b["m_NameIndex"], "?")
            print("    CBIND reg%d -> cbuffer '%s' (nameIdx %d) arrSize=%d" % (b["m_Index"], nm, b["m_NameIndex"], b["m_ArraySize"]))
        for cb in cp["m_ConstantBuffers"]:
            nm = rev.get(cb["m_NameIndex"], "?")
            print("    CB '%s' size=%s partial=%s nVec=%d nMat=%d nStruct=%d" % (
                nm, cb["m_Size"], cb.get("m_IsPartialCB"),
                len(cb["m_VectorParams"]), len(cb["m_MatrixParams"]), len(cb["m_StructParams"])))
            for vp in cb["m_VectorParams"]:
                print("      vec  '%s' dim=%d arr=%d  (nameIdx %d)" % (rev.get(vp["m_NameIndex"], "?"), vp["m_Dim"], vp.get("m_ArraySize", 0), vp["m_NameIndex"]))
            for mp in cb["m_MatrixParams"]:
                print("      mat  '%s' rows=%d cols=%d" % (rev.get(mp["m_NameIndex"], "?"), mp.get("m_Rows", 0), mp.get("m_Columns", 0)))
            for sp in cb["m_StructParams"]:
                print("      struct '%s' elements=%d" % (rev.get(sp["m_NameIndex"], "?"), sp.get("m_ElementCount", 0)))
