import json
import os
HERE = os.path.expandvars(r"C:\Users\%USERNAME%\Documents\GitHub\mfg-decomp\build\_tmp_audit_moutheye")
for tag in ("mouth", "eye"):
    d = json.load(open(HERE + rf"\progs_{tag}_disasm.json"))
    print("===", tag, "hypothesis:", d["hypothesis"])
    reg2sub = {}
    for s in d["subs"]:
        reg2sub[s["region"]] = s
    for p in d["progs"]:
        s = reg2sub.get(p["region"])
        info = f"P{p['region']:02d} {p.get('stage','?'):8s} len={p['dxbc_len']:6d} cbs={p.get('cbs')} t2d={p.get('tex2d')} tc={p.get('texcube')} sm={p.get('sampler')} h={p.get('shdr_hash')}"
        print(info)
        if s:
            print(f"    -> pass{s['passid']} {s['kind']} sub{s['sub']} kws={s['kws']}")
    # parameter blob regions (no DXBC)
    dxc = {p["region"] for p in d["progs"]}
    print("non-program regions:", [i for i in range(d["regions"]) if i not in dxc])
