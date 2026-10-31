import json, struct, os, re, hashlib, ctypes
from ctypes import *

HERE = os.path.expandvars(r"C:\Users\%USERNAME%\Documents\GitHub\mfg-decomp\build\_tmp_audit_moutheye")
dc = windll.LoadLibrary("d3dcompiler_47")
class ID3DBlob(Structure): pass
D3DDisassemble = dc.D3DDisassemble
D3DDisassemble.restype = c_long
D3DDisassemble.argtypes = [c_void_p, c_size_t, c_uint, c_char_p, POINTER(c_void_p)]
def blob_text(ptr):
    q = struct.unpack_from("<4Q", string_at(ptr, 32))
    return string_at(q[3], q[2])

def sgn_names(g, off, size):
    # ISGN/OSGN: fourcc+size then [u32 n][u32 payloadSize][entries][names] (face-proven layout)
    try:
        body = g[off + 8:off + size]
        n, psize = struct.unpack_from("<2I", body, 0)
        esz = psize // n if n else 0
        names = body[8 + psize:]
        out = []
        for i in range(n):
            e = body[8 + i * esz:8 + (i + 1) * esz]
            nameOff = struct.unpack_from("<I", e, 0)[0]
            nm = names[nameOff:names.index(b"\x00", nameOff)].decode("latin-1")
            out.append(nm)
        return out
    except Exception:
        return None

for name in ("Mouth", "Eye"):
    tag = name.lower()
    j = json.load(open(os.path.expandvars(rf"C:\Users\%USERNAME%\Documents\GitHub\mfg-decomp\build\export-primary-content\Assets\Shaders\Character\{name}.json"), encoding="utf-8"))
    g = open(os.path.join(HERE, f"{tag}_blob.bin"), "rb").read()
    kw_names = j["m_ParsedForm"]["m_KeywordNames"]
    passes = j["m_ParsedForm"]["m_SubShaders"][0]["m_Passes"]
    hdrn = struct.unpack_from("<I", g, 0)[0]
    ents = [struct.unpack_from("<3I", g, 4 + i * 12) for i in range(hdrn)]

    subs = []
    for pi, pa in enumerate(passes):
        for kind in ("m_ProgVertex", "m_ProgFragment"):
            stage_kind = "VS" if kind.endswith("Vertex") else "PS"
            for gi, group in enumerate(pa[kind]["m_PlayerSubPrograms"]):
                for si, sp in enumerate(group):
                    subs.append(dict(passid=pi, kind=stage_kind, sub=si, blob=sp["m_BlobIndex"],
                                     gpst=sp.get("m_GpuProgramType"),
                                     kws=[kw_names[i] for i in sp.get("m_KeywordIndices", [])]))

    progs = []
    for i, (off, size, flg) in enumerate(ents):
        dx = g.find(b"DXBC", off, off + size)
        if dx < 0:
            continue
        ln, nsec = struct.unpack_from("<2I", g, dx + 24)
        secs = {}
        for si in range(nsec):
            so, = struct.unpack_from("<I", g, dx + 32 + si * 4)
            st = g[dx + so:dx + so + 4].decode("latin-1", "ignore")
            ssize, = struct.unpack_from("<I", g, dx + so + 4)
            secs[st] = [dx + so, ssize]
        data = g[dx:dx + ln]
        blob = c_void_p()
        hr = D3DDisassemble(cast(c_char_p(data), c_void_p), len(data), 0, b"", byref(blob))
        txt = blob_text(blob.value).decode("latin-1") if (hr == 0 and blob.value) else None
        rec = dict(region=i, region_size=size, dx=dx, dxbc_len=ln, secs=secs)
        if txt is None:
            rec["disasm_error"] = hex(hr & 0xFFFFFFFF)
            print(name, f"region {i} DISASM FAIL hr={hex(hr & 0xFFFFFFFF)}")
        else:
            fn = f"{tag}_asm{i:02d}.txt"
            open(os.path.join(HERE, fn), "w", encoding="utf-8").write(txt)
            rec["asm"] = fn
            m = re.search(r"^(?:// )?(vs|ps)_(\d)_(\d)", txt, re.M)
            rec["stage"] = f"{m.group(1).upper()}_{m.group(2)}_{m.group(3)}" if m else "?"
            cbs = [(int(a), int(b)) for a, b in re.findall(r"dcl_constantbuffer CB(\d+)\[(\d+)\]", txt)]
            rec["cbs"] = cbs
            rec["tex2d"] = re.findall(r"dcl_resource_texture2d \([^)]*\)\s*t(\d+)", txt)
            rec["texcube"] = re.findall(r"dcl_resource_texturecube.*?\bt(\d+)", txt)
            rec["sampler"] = re.findall(r"dcl_sampler.*?s(\d+)", txt)
            rec["insgn"] = sgn_names(g, *secs["ISGN"]) if "ISGN" in secs else None
            rec["osgn"] = sgn_names(g, *secs["OSGN"]) if "OSGN" in secs else None
            sh = secs["SHDR"]
            rec["shdr_hash"] = hashlib.sha1(g[sh[0] + 8:sh[0] + sh[1]]).hexdigest()[:12]
        progs.append(rec)

    progset = {p["region"]: p for p in progs}
    dxrank = {b: n for n, b in enumerate(sorted(progset))}
    for hyp in ("region_idx", "dx_rank"):
        ok = 0
        for s in subs:
            ri = s["blob"] if hyp == "region_idx" else dxrank.get(s["blob"])
            p = progset.get(ri)
            s["region" + "_" + hyp] = ri if p and p.get("stage", "")[:2] == s["kind"] else None
            if p and p.get("stage", "")[:2] == s["kind"]:
                ok += 1
        print(name, f"map hypothesis {hyp}: {ok}/{len(subs)} stage-agree")
    # choose: prefer dx_rank if that one is perfect
    best = None
    for hyp in ("dx_rank", "region_idx"):
        vals = [s.get("region_" + hyp) for s in subs]
        if all(v is not None for v in vals) and len(set(vals)) == len(vals):
            best = hyp
            break
    print(name, "chosen hypothesis:", best)
    for s in subs:
        ri = s["region_" + best]
        p = progset[ri]
        s["region"] = ri
        # cross-check: record-prefix keyword strings == json keyword set
        pre = g[ents[ri][0]:p["dx"]]
        toks = set(re.findall(rb"[A-Z0-9_]{4,}", pre))
        jset = {k.encode() for k in s["kws"]}
        s["kw_prefix_match"] = (jset <= toks) and (len(toks - jset) == 0)
        s["extra_prefix_toks"] = sorted(t.decode() for t in toks - jset)
    nmatch = sum(1 for s in subs if s.get("kw_prefix_match"))
    print(name, f"keyword-set cross-check: exact match {nmatch}/{len(subs)}")
    badex = [(s["passid"], s["kind"], s["sub"], s["extra_prefix_toks"][:6]) for s in subs if not s.get("kw_prefix_match")][:6]
    print(name, "mismatch samples:", badex)

    # pass<->program table
    for pi in range(len(passes)):
        for kind in ("VS", "PS"):
            arr = [s for s in subs if s["passid"] == pi and s["kind"] == kind]
            if arr:
                print(f"  pass{pi} {kind}: n={len(arr)} regions={[s['region'] for s in arr]}")
                print(f"     kws[0]: {arr[0]['kws']}")
                print(f"     globalSize(json pass frag): {[p.get('m_Size') for pa2 in [passes[pi]] for p in pa2[('m_ProgVertex' if kind=='VS' else 'm_ProgFragment')]['m_CommonParameters']['m_ConstantBuffers'] if p.get('m_NameIndex') is not None][:1]}")
    # global cb size per pass for $Globals: find nameIndex of $Globals from m_NameIndices
    json.dump(dict(name=name, regions=hdrn, progs=progs, subs=subs, hypothesis=best),
              open(os.path.join(HERE, f"progs_{tag}_disasm.json"), "w"), indent=1, default=str)
    print(name, "wrote progs json; asm files:", sum(1 for p in progs if "asm" in p), "/", len(progs))
