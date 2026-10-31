import json, os

HERE = os.path.expandvars(r"C:\Users\%USERNAME%\Documents\GitHub\mfg-decomp\build\_tmp_audit_moutheye")
for tag in ("mouth", "eye"):
    inv = json.load(open(os.path.join(HERE, f"progs_{tag}.json"), encoding="utf-8"))
    seen = {}
    for pr in inv["programs"]:
        for r in pr["json_refs"]:
            kws = r["kws"]
            sig = tuple(k for k in kws if k.startswith("_IS_") or k in ("SHADOWS_SCREEN", "DIRECTIONAL", "DIRECTIONAL1", "POINT", "SPOT", "META"))
            seen.setdefault((r["pass"], sig), []).append(pr["region"])
    print("==", tag)
    for k in sorted(seen, key=lambda x: (x[0], len(x[1]))):
        print("  pass", k[0], "sig:", k[1], "regions:", seen[k][:6], "..." if len(seen[k]) > 6 else "")
