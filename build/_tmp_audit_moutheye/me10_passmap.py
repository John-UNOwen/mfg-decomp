import json, os

HERE = os.path.expandvars(r"C:\Users\%USERNAME%\Documents\GitHub\mfg-decomp\build\_tmp_audit_moutheye")
for tag in ("mouth", "eye"):
    inv = json.load(open(os.path.join(HERE, f"progs_{tag}.json"), encoding="utf-8"))
    print("==", tag, "passes:")
    for p in inv["passes"]:
        kws = [k for k in p["pass_keywords"] if k.startswith("_IS_")]
        print("  pass", p["idx"], "name:", repr(p["name"]), "vs blobs:", len(p["vs_blobs"]),
              "ps blobs:", len(p["ps_blobs"]), "IS kws:", kws[:6])
    # stage/region table compact
    print("  region -> pass/kind:", [(pr["region"], pr["stage"], pr["json_refs"][0]["pass"] if pr["json_refs"] else None)
                                    for pr in inv["programs"]])
