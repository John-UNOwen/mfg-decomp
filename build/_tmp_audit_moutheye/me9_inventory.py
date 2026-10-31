import json, struct, hashlib, os, re

HERE = os.path.expandvars(r"C:\Users\%USERNAME%\Documents\GitHub\mfg-decomp\build\_tmp_audit_moutheye")
SRC = os.path.expandvars(r"C:\Users\%USERNAME%\Documents\GitHub\mfg-decomp\build\export-primary-content\Assets\Shaders\Character\{name}.json")
STAGE = {1: "VS", 0: "PS", 2: "GS", 3: "HS", 4: "DS", 5: "CS"}

for name in ("Mouth", "Eye"):
    tag = name.lower()
    j = json.load(open(SRC.format(name=name), encoding="utf-8"))
    g = open(os.path.join(HERE, f"{tag}_blob.bin"), "rb").read()
    kw_names = j["m_ParsedForm"]["m_KeywordNames"]
    sub = j["m_ParsedForm"]["m_SubShaders"][0]
    passes = sub["m_Passes"]
    subs = []
    for pi, pa in enumerate(passes):
        for kind in ("m_ProgVertex", "m_ProgFragment"):
            d = pa[kind]
            for gi, group in enumerate(d["m_PlayerSubPrograms"]):
                for si, sp in enumerate(group or []):
                    subs.append({"pass": pi, "kind": "VS" if kind.endswith("Vertex") else "PS",
                                 "group": gi, "sub": si, "blob": sp["m_BlobIndex"],
                                 "kws": [kw_names[i] for i in sp.get("m_KeywordIndices", [])
                                         if i < len(kw_names)]})
    hdrn = struct.unpack_from("<I", g, 0)[0]
    ents = [struct.unpack_from("<3I", g, 4 + i * 12) for i in range(hdrn)]

    def kw_prefix(off):
        p = off + 28
        kws = []
        while p + 4 <= len(g):
            L, = struct.unpack_from("<I", g, p)
            if L == 0 or L > 200 or not re.match(r"^[A-Za-z0-9_]+$", g[p + 4:p + 4 + L].decode("latin-1", "x")):
                break
            kws.append(g[p + 4:p + 4 + L].decode("latin-1", "replace"))
            p += 4 + L
        return kws

    def globals_size(pi, kind):
        pa = passes[pi]
        rev = {v: k for k, v in pa["m_NameIndices"].items()}
        cp = pa[kind]["m_CommonParameters"]
        for cb in cp["m_ConstantBuffers"]:
            if rev.get(cb["m_NameIndex"], "") == "$Globals":
                return cb["m_Size"]
        return 0

    progs = []
    used = set()
    for i, (off, size, flg) in enumerate(ents):
        dx = g.find(b"DXBC", off, off + size)
        if dx < 0:
            continue
        ln, nsec = struct.unpack_from("<2I", g, dx + 24)
        secs = {}
        for si in range(nsec):
            so, = struct.unpack_from("<I", g, dx + 32 + si * 4)
            tag2 = g[dx + so:dx + so + 4].decode("latin-1", "ignore")
            ssize, = struct.unpack_from("<I", g, dx + so + 4)
            secs[tag2] = [dx + so, ssize]
        shdr = secs.get("SHDR")
        stage = "?"
        vtok = None
        if shdr:
            phdr, = struct.unpack_from("<I", g, shdr[0] + 8)
            stage = STAGE.get(phdr >> 16, hex(phdr))
        m = [s for s in subs if s["blob"] == i]
        assert i not in used, (name, i)
        used.add(i)
        entry = dict(region=i, stage=stage, size=size, dx_off=dx, dxbc_len=ln,
                     kw_prefix=kw_prefix(off),
                     sections=secs,
                     shdr_hash=hashlib.sha1(g[shdr[0]:shdr[0] + shdr[1]]).hexdigest()[:12] if shdr else None,
                     json_refs=[{"pass": s["pass"], "pass_name": passes[s["pass"]].get("m_Name", ""),
                                "kind": s["kind"], "kws": s["kws"],
                                "globals_size": globals_size(s["pass"], "m_Prog" + ("Vertex" if s["kind"] == "VS" else "Fragment")),
                                "group": s["group"], "sub": s["sub"]} for s in m])
        progs.append(entry)

    ok_vs = ok_ps = 0
    for s in subs:
        pi, kind = s["pass"], s["kind"]
        arr = [p for p in progs if p["region"] == s["blob"]]
        if arr and arr[0]["stage"] == kind:
            if kind == "VS":
                ok_vs += 1
            else:
                ok_ps += 1
    nvs = sum(1 for s in subs if s["kind"] == "VS")
    nps = len(subs) - nvs
    fwd = {}
    for pi, pa in enumerate(passes):
        for kind in ("VS", "PS"):
            arr = [s for s in subs if s["pass"] == pi and s["kind"] == kind]
            if arr and kind == "VS":
                fwd[f"pass{pi}_VS"] = arr[0]["blob"]
            if arr and kind == "PS":
                fwd[f"pass{pi}_PS"] = arr[0]["blob"]
    inv = dict(shader=name, n_regions=hdrn, n_programs=len(progs),
               stage_agreement=dict(vs=f"{ok_vs}/{nvs}", ps=f"{ok_ps}/{nps}"),
               first_variant_regions=fwd,
               passes=[dict(idx=pi, name=pa.get("m_Name", ""),
                            vs_blobs=[s["blob"] for s in subs if s["pass"] == pi and s["kind"] == "VS"],
                            ps_blobs=[s["blob"] for s in subs if s["pass"] == pi and s["kind"] == "PS"],
                            pass_keywords=[kw_names[i] for i in pa.get("m_ValidKeywords", []) if i < len(kw_names)])
                           for pi, pa in enumerate(passes)],
               programs=progs)
    out = os.path.join(HERE, f"progs_{tag}.json")
    json.dump(inv, open(out, "w", encoding="utf-8"), indent=1)
    print(name, "regions:", hdrn, "programs:", len(progs), "stage agree:", inv["stage_agreement"])
    print("   pass first-variant regions:", fwd)
    for p in passes:
        pass
