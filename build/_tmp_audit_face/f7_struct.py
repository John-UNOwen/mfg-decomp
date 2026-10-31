import json, struct, re, os
OUT = r"C:\mfg\build\_tmp_audit_face"
g = open(os.path.join(OUT,"face_blob.bin"),"rb").read()
# hexdump region 1420..1560 to document program record header
print("== region 1400..1560 ==")
for off in range(1400,1560,32):
    b = g[off:off+32]
    print(f"{off:6d}  {b.hex()}   {''.join(chr(c) if 32<=c<127 else '.' for c in b)}")
j = json.load(open(r"C:\mfg\build\export-primary-content\Assets\Toon\Shader\Toon_DoubleShadeWithFeather_TransClipping_StencilMask.json", encoding="utf-8"))
pa = j["m_ParsedForm"]["m_SubShaders"][0]["m_Passes"][1]
sp = pa["m_ProgFragment"]
print("\n== PASS[1] ProgFragment keys:", sorted(sp.keys()))
print("CommonParameters:", json.dumps(sp["m_CommonParameters"])[:400])
print("ParameterBlobIndices:", json.dumps(sp["m_ParameterBlobIndices"])[:300])
print("SerializedKeywordStateMask:", json.dumps(sp["m_SerializedKeywordStateMask"])[:200])
psp = sp["m_PlayerSubPrograms"]; subs = sp["m_SubPrograms"]
print("PlayerSubPrograms len:", len(psp), "SubPrograms len:", len(subs))
if psp: print("PlayerSubPrograms[0]:", json.dumps(psp[0])[:800])
if subs: print("SubPrograms[0]:", json.dumps(subs[0])[:500])
