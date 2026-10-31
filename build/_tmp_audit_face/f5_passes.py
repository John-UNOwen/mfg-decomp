import json
p = r"C:\mfg\build\export-primary-content\Assets\Toon\Shader\Toon_DoubleShadeWithFeather_TransClipping_StencilMask.json"
j = json.load(open(p, encoding="utf-8"))
pf = j["m_ParsedForm"]
print("NAME:", pf["m_Name"], "| fallback:", pf["m_FallbackName"], "| editor:", pf["m_CustomEditorName"])
print("KEYWORDS(", len(pf["m_KeywordNames"]), "):", pf["m_KeywordNames"])
ss = pf["m_SubShaders"][0]
print("SUBSHADER tags:")
for k,v in ss["m_Tags"]["m_Tags"].items(): print("   tag", k, "=", v)
for i,pa in enumerate(ss["m_Passes"]):
    print(f"\n== PASS[{i}] keys:", sorted(pa.keys()))
    for k,v in sorted(pa.items()):
        if isinstance(v,(str,int,float,bool)) or v is None:
            print(f"   {k} = {v}")
        elif isinstance(v,dict):
            print(f"   {k}: dict keys {sorted(v.keys())}")
        elif isinstance(v,list):
            print(f"   {k}: list len {len(v)}", (v[:3] if len(str(v[:2]))<200 else ""))
