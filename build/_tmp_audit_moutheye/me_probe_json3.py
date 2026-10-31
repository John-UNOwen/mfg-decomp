import json
import os

p = os.path.expandvars(r"C:\Users\%USERNAME%\Documents\GitHub\mfg-decomp\build\export-primary-content\Assets\Shaders\Character\{name}.json")
j = json.load(open(p.format(name="Mouth"), encoding="utf-8"))
sub = j["m_ParsedForm"]["m_SubShaders"][0]
for pi, ps in enumerate(sub["m_Passes"]):
    print("pass", pi, "tags:", ps.get("m_Tags"), "type:", ps.get("m_Type"), "mask:", ps.get("m_ProgramMask"))
ps0 = sub["m_Passes"][0]
for k in ("m_ProgFragment", "m_ProgVertex", "m_ProgDomain", "m_ProgGeometry", "m_ProgHull"):
    v = ps0.get(k)
    print(k, type(v).__name__, (len(v) if hasattr(v, "__len__") else v))
    if isinstance(v, dict):
        for k2 in list(v)[:3]:
            e = v[k2]
            print("  key", k2, "entry type:", type(e).__name__, sorted(e.keys()) if isinstance(e, dict) else str(e)[:120])
ni = ps0["m_NameIndices"]
print("m_NameIndices type:", type(ni).__name__, len(ni))
if isinstance(ni, dict):
    for k, v in list(ni.items())[:6]:
        print("   ", k, v)
elif isinstance(ni, list):
    print("  first 8:", ni[:8])
