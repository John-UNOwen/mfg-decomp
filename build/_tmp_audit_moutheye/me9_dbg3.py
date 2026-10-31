import json, os

HERE = os.path.expandvars(r"C:\Users\%USERNAME%\Documents\GitHub\mfg-decomp\build\_tmp_audit_moutheye")
for tag in ("mouth", "eye"):
    inv = json.load(open(os.path.join(HERE, f"progs_{tag}.json"), encoding="utf-8"))
    print("==", tag)
    for p in inv["programs"]:
        if p["region"] in (2, 8):
            print("  region", p["region"], "stage:", p["stage"], "shdr hash:", p["shdr_hash"])
            print("   kw_prefix:", p["kw_prefix"])
            for r in p["json_refs"]:
                print("   ref pass", r["pass"], r["pass_name"], r["kind"], "globals", r["globals_size"], "kws:", r["kws"])
