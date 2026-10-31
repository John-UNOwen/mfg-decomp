import json
import os

for name in ("Mouth", "Eye"):
    p = os.path.expandvars(rf"C:\Users\%USERNAME%\Documents\GitHub\mfg-decomp\build\export-primary-content\Assets\Shaders\Character\{name}.json")
    j = json.load(open(p, encoding="utf-8"))
    pf = j["m_ParsedForm"]
    subs = pf["m_SubShaders"]
    print("===", name, "subshaders:", len(subs))
    for si, s in enumerate(subs):
        print(" sub", si, sorted(s.keys()))
        passes = s.get("m_Passes", [])
        print("  passes:", len(passes), [p.get("m_Name") for p in passes])
        if passes:
            p0 = passes[0]
            print("  pass0 keys:", sorted(p0.keys()))
            break
    pi_ = pf.get("m_PropInfo")
    if pi_:
        print(" PropInfo keys:", sorted(pi_.keys()) if isinstance(pi_, dict) else type(pi_))
