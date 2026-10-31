import json, os, re
OUT = r"C:\mfg\build\_tmp_audit"
t = open(r"C:\mfg\build\export-primary-content\Assets\Shaders\Character\Eye.json", "r", encoding="utf-8", errors="ignore").read()
j = json.loads(t)
pf = j["m_ParsedForm"]
print("shader name:", pf["m_Name"], "custom editor:", pf["m_CustomEditorName"])
print("keywords:", pf["m_KeywordNames"])
print("keyword flags:", pf["m_KeywordFlags"])
ss = pf["m_SubShaders"][0]
print("subshader tags:", ss["m_Tags"], "passes:", len(ss["m_Passes"]))
for i, p in enumerate(ss["m_Passes"]):
    print(f"\n== PASS {i} keys: {sorted(p.keys())}")
    print("  name:", p.get("m_Name"), "tags:", p.get("m_Tags"))
    for k in ("m_ShaderType","m_BlendState","m_StencilState","m_ZTest","m_ZWrite","m_Cull","m_ColorMask","m_BlendOp","m_SrcMode","m_DstMode"):
        if k in p: print(f"  {k}: {json.dumps(p[k])[:400]}")
    prg = p.get("m_Programs")
    if prg: print("  programs:", json.dumps(prg)[:600])
    for k, v in p.items():
        if isinstance(v, str) and len(v) > 2000:
            fn = os.path.join(OUT, f"pass{i}_{k}.txt")
            open(fn, "w", encoding="utf-8").write(v)
            print(f"  [wrote {len(v)} chars of {k} -> {fn}]")
        if isinstance(v, dict):
            for k2, v2 in v.items():
                if isinstance(v2, str) and len(v2) > 2000:
                    fn = os.path.join(OUT, f"pass{i}_{k}_{k2}.txt")
                    open(fn, "w", encoding="utf-8").write(v2)
                    print(f"  [wrote {len(v2)} chars of {k}.{k2} -> {fn}]")
    # nested snippet dicts
    sn = p.get("m_Snippet") or p.get("m_Snippets")
    if sn: print("  snippet:", json.dumps(sn)[:300])
# also m_Platforms and stage counts
print("\nplatforms:", json.dumps(j["m_Platforms"])[:500])
print("stageCounts:", json.dumps(j["m_StageCounts"])[:300])
# list of files written
for fn in sorted(os.listdir(OUT)):
    if fn.startswith("pass"): print("FILE", fn, os.path.getsize(os.path.join(OUT, fn)))
