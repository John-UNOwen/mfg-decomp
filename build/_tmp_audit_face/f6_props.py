import json
p = r"C:\mfg\build\export-primary-content\Assets\Toon\Shader\Toon_DoubleShadeWithFeather_TransClipping_StencilMask.json"
j = json.load(open(p, encoding="utf-8"))
props = j["m_ParsedForm"]["m_PropInfo"]["m_Props"]
print("TOTAL PROPS:", len(props))
names = [pr["m_Name"] for pr in props]
custom = [n for n in names if any(k in n for k in ("GBlend","BBlend","SkinType","ReplaceColor","ShadowColor","Chroma","Contrast","Brightness","HighColor_Power","Smoothness","Specular","FaceShadow"))]
print("CUSTOM-ish names:", custom)
for i,pr in enumerate(props):
    print(i, pr["m_Name"], "| type", pr["m_Type"], "| def", pr["m_DefValue_0_"], pr["m_DefValue_1_"], pr["m_DefValue_2_"], pr["m_DefValue_3_"])
print("CorrespondingSourceObject:", j.get("m_CorrespondingSourceObject"))
print("Dependencies:", j.get("m_Dependencies"))
print("Platforms:", j.get("m_Platforms"), "| ShaderIsBaked:", j.get("m_ShaderIsBaked"))
