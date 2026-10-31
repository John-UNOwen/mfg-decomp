import json
import os

p = os.path.expandvars(r"C:\Users\%USERNAME%\Documents\GitHub\mfg-decomp\build\export-primary-content\Assets\Shaders\Character\{name}.json")
j = json.load(open(p.format(name="Mouth"), encoding="utf-8"))
pf = j["m_ParsedForm"]
print("KeywordNames:", pf.get("m_KeywordNames"))
ps0 = pf["m_SubShaders"][0]["m_Passes"][0]
cp = ps0["m_ProgFragment"]["m_CommonParameters"]
for k in ("m_ConstantBuffers", "m_BufferParams", "m_VectorParams", "m_MatrixParams",
          "m_TextureParams", "m_Samplers", "m_ConstantBufferBindings", "m_UAVParams"):
    v = cp[k]
    print("==", k, "len", len(v))
    for e in v[:4]:
        print("   ", str(e)[:300])
