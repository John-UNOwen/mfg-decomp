import json, struct, os, re
from collections import Counter

HERE = os.path.expandvars(r"C:\Users\%USERNAME%\Documents\GitHub\mfg-decomp\build\_tmp_audit_moutheye")
SRC = os.path.expandvars(r"C:\Users\%USERNAME%\Documents\GitHub\mfg-decomp\build\export-primary-content\Assets\Shaders\Character\{name}.json")

STAGE = {0x9: "VS", 0x11: "PS", 0x19: "GS", 0x21: "HS", 0x29: "DS", 0x31: "CS"}

for name in ("Mouth", "Eye"):
    j = json.load(open(SRC.format(name=name), encoding="utf-8"))
    g = open(os.path.join(HERE, f"{name.lower()}_blob.bin"), "rb").read()
    kw_names = j["m_ParsedForm"]["m_KeywordNames"]
    kw_set = set(kw_names)
    sub = j["m_ParsedForm"]["m_SubShaders"][0]
    passes = sub["m_Passes"]

    # json-side subprogram inventory
    subs = []  # (pass, kind, subidx, blob_index, kws decoded, typeset)
    for pi, pa in enumerate(passes):
        for kind in ("m_ProgVertex", "m_ProgFragment"):
            d = pa[kind]
            plist = d["m_PlayerSubPrograms"]
            for gi, group in enumerate(plist):
                if not group:
                    continue
                for si, sp in enumerate(group):
                    kws = [kw_names[i] for i in sp.get("m_KeywordIndices", []) if i < len(kw_names)]
                    subs.append(dict(passid=pi, kind="VS" if kind.endswith("Vertex") else "PS",
                                     group=gi, sub=si, blob=sp["m_BlobIndex"],
                                     gpst=sp.get("m_GpuProgramType"), kws=kws,
                                     npass=len(sp.get("m_needParamsConstBuffer", []))
                                     if isinstance(sp.get("m_needParamsConstBuffer"), list) else None))
    groups = sorted(set(s["group"] for s in subs))
    gpsts = Counter((s["group"], s["gpst"], s["kind"]) for s in subs)
    print(f"== {name}: passes={len(passes)} subprograms={len(subs)} groups={groups}")
    print("   group/gpst/kind counts:", dict(gpsts))

    # blob regions
    hdrn = struct.unpack_from("<I", g, 0)[0]
    ents = [struct.unpack_from("<3I", g, 4 + i * 12) for i in range(hdrn)]

    def parse_kw(off):
        d = struct.unpack_from("<8I", g, off)
        kws, p = [], off + 28
        while True:
            if p + 4 > len(g):
                break
            L, = struct.unpack_from("<I", g, p)
            if L == 0 or L > 200:
                break
            s = g[p + 4:p + 4 + L].decode("latin-1", "replace")
            if not re.match(r"^[A-Za-z0-9_]+$", s):
                break
            kws.append(s)
            p += 4 + L
        return d, kws, p

    progs = []
    for i, (off, size, flg) in enumerate(ents):
        dx = g.find(b"DXBC", off, off + min(size, 200))
        if dx < 0:
            continue
        ln, nsec = struct.unpack_from("<2I", g, dx + 24)
        secs = {}
        for si in range(nsec):
            so, = struct.unpack_from("<I", g, dx + 32 + si * 4)
            tag = g[dx + so:dx + so + 4].decode("latin-1", "ignore")
            ssize, = struct.unpack_from("<I", g, dx + so + 4)
            secs[tag] = [dx + so, ssize]
        hdr, kws, kend = parse_kw(off)
        rec = dict(idx=i, region=off, size=size, dx=dx, dxbc_len=ln, hdr=list(hdr),
                   kws=kws, kw_ok=all(k in kw_set for k in kws), stage=None, secs=secs)
        if "SHDR" in secs:
            so, ss = secs["SHDR"]
            vtok, vword = struct.unpack_from("<2I", g, so + 8)
            rec["vtok"] = vtok
            rec["vword"] = vword
            rec["stage"] = STAGE.get(vword & 0xFFFF, hex(vword & 0xFFFF))
            rec["ver"] = ((vword >> 24) & 0xF, (vword >> 20) & 0xF)
        progs.append(rec)
    bad_kw = [p["idx"] for p in progs if not p["kw_ok"]]
    print(f"   DXBC programs: {len(progs)}; keyword-parse bad regions: {bad_kw}")
    print("   stages:", dict(Counter(p["stage"] for p in progs)))
    print("   section tags:", sorted(set(t for p in progs for t in p["secs"])))

    # mapping hypotheses
    progset = {p["idx"]: p for p in progs}
    dxbc_only = sorted(progset)
    n_dx_before = {ri: sum(1 for q in dxbc_only if q < ri) for ri in progset}
    for hyp in ("idx", "dx_rank"):
        mism = 0
        for s in subs:
            if hyp == "idx":
                p = progset.get(s["blob"])
            else:
                p = dxbc_only[s["blob"]] if s["blob"] < len(dxbc_only) else None
            if p is None or p["stage"] != s["kind"]:
                mism += 1
        print(f"   blob-map hypothesis {hyp}: mismatches={mism}/{len(subs)}")

    # attach json mapping under chosen hypothesis (idx first; caller confirms via mism=0)
    for s in subs:
        ri = s["blob"]
        s["map_ok"] = ri in progset and progset[ri]["stage"] == s["kind"]
        s["mapped_kws"] = progset[ri]["kws"] if ri in progset else None
    ok_frac = sum(1 for s in subs if s["map_ok"]) / max(1, len(subs))
    print(f"   idx-hypothesis stage agreement: {ok_frac:.2f}")

    json.dump(dict(name=name, n_regions=hdrn, progs=progs, subs=subs),
              open(os.path.join(HERE, f"progs_{name.lower()}_raw.json"), "w"), indent=1)
    # sample pass/keyword summary
    for pi in range(len(passes)):
        for kind in ("VS", "PS"):
            arr = [s for s in subs if s["passid"] == pi and s["kind"] == kind]
            if arr:
                print(f"   pass{pi} {kind}: {len(arr)} variants, kws[0]={arr[0]['kws'][:8]}")
    if name == "Mouth":
        print("   Mouth region tags without DXBC:", [i for i in range(hdrn) if progset.get(i) is None][:40])
