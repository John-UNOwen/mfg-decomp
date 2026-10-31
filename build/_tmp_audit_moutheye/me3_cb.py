import json
import os

HERE = os.path.expandvars(r"C:\Users\%USERNAME%\Documents\GitHub\mfg-decomp\build\_tmp_audit_moutheye")
SRC = os.path.expandvars(r"C:\Users\%USERNAME%\Documents\GitHub\mfg-decomp\build\export-primary-content\Assets\Shaders\Character\{name}.json")

for name in ("Mouth", "Eye"):
    j = json.load(open(SRC.format(name=name), encoding="utf-8"))
    passes = j["m_ParsedForm"]["m_SubShaders"][0]["m_Passes"]
    out = []
    for pi, pa in enumerate(passes):
        rev = {v: k for k, v in pa["m_NameIndices"].items()}
        for kind in ("m_ProgVertex", "m_ProgFragment"):
            cp = pa[kind]["m_CommonParameters"]
            if not cp["m_ConstantBuffers"]:
                continue
            regof = {b["m_NameIndex"]: b["m_Index"] for b in cp["m_ConstantBufferBindings"]}
            out.append(f"== PASS[{pi}] {kind} cb bindings: " + str(sorted(regof.items(), key=lambda kv: kv[1])))
            for cb in cp["m_ConstantBuffers"]:
                nm = rev.get(cb["m_NameIndex"], "?")
                reg = regof.get(cb["m_NameIndex"], None)
                out.append(f"-- CB '{nm}' reg={reg} size={cb['m_Size']}")
                items = []
                for vp in cb["m_VectorParams"]:
                    off = vp["m_OffsetInConstantBuffer"]
                    n = rev.get(vp["m_NameIndex"], "?")
                    items.append((off, n, vp["m_Dim"], vp.get("m_ArraySize", 0)))
                for mp in cb["m_MatrixParams"]:
                    off = mp["m_OffsetInConstantBuffer"]
                    items.append((off, rev.get(mp["m_NameIndex"], "?"), f"mat{mp.get('m_Rows')}x{mp.get('m_Columns')}", 0))
                for sp in cb["m_StructParams"]:
                    off = sp.get("m_OffsetInConstantBuffer", -1)
                    items.append((off, rev.get(sp["m_NameIndex"], "?") + f"[{sp.get('m_ElementCount')}]", 0, 0))
                for off, n, dim, arr in sorted(items, key=lambda t: (isinstance(t[2], str), t[0])):
                    cc = "xyzw" if not isinstance(dim, str) and dim > 1 else ("xyzw"[(off % 16) // 4] if isinstance(dim, int) and dim == 1 else "")
                    out.append(f"   off={off:5d} slot={off // 16:3d} comp={cc} {n} dim={dim} arr={arr}")
    open(HERE + rf"\cb_layout_{name.lower()}.txt", "w", encoding="utf-8").write("\n".join(out) + "\n")
    print(name, "wrote cb_layout", len(out), "lines")
    # also list shader-level Props count and texture props
    props = j["m_ParsedForm"]["m_PropInfo"]["m_Props"]
    texes = [p for p in props if p.get("m_Type") == 2]
    print(name, "props:", len(props), "texture props:", len(texes))
