import json, sys
import os

for name in ("Mouth", "Eye"):
    p = os.path.expandvars(rf"C:\Users\%USERNAME%\Documents\GitHub\mfg-decomp\build\export-primary-content\Assets\Shaders\Character\{name}.json")
    j = json.load(open(p, encoding="utf-8"))
    print("===", name, "top keys:", sorted(j.keys()))
    for k in ("m_Name", "m_Shader", "m_DecompressedLengths"):
        if k in j:
            print(" ", k, "=", str(j[k])[:200])
    if "m_CompressedBlob" in j:
        print("  blob b64 len:", len(j["m_CompressedBlob"]))
    pf = j.get("m_ParsedForm") or j.get("ParsedForm") or {}
    print("  parsed keys:", sorted(pf.keys()) if isinstance(pf, dict) else type(pf))
