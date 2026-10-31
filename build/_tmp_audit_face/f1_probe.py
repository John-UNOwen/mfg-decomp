import json, base64, lz4.block, os
OUT = r"C:\mfg\build\_tmp_audit_face"
p = r"C:\mfg\build\export-primary-content\Assets\Toon\Shader\Toon_DoubleShadeWithFeather_TransClipping_StencilMask.json"
j = json.load(open(p, encoding="utf-8"))
print("top keys:", sorted(j.keys()))
print("DecompressedLengths:", j.get("m_DecompressedLengths"))
print("CompBlob len(b64):", len(j.get("m_CompressedBlob","")))
for k in ("m_DecompressedLengths2","m_Name","m_Shader","m_ParsedForm","m_GpuPrograms","m_Script"):
    if k in j: print(k, type(j[k]))
