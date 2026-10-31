import json
p = r"C:\mfg\build\export-primary-content\Assets\Toon\Shader\Toon_DoubleShadeWithFeather_TransClipping_StencilMask.json"
j = json.load(open(p, encoding="utf-8"))
pf = j["m_ParsedForm"]
def walk(o, path, depth=0):
    if depth>3: return
    if isinstance(o, dict):
        for k,v in o.items():
            t = type(v).__name__
            if isinstance(v,(dict,list)):
                print(f"{path}.{k}: {t} len={len(v)}")
                walk(v, path+"."+k, depth+1)
            else:
                s = str(v)[:80]
                print(f"{path}.{k}: {s}")
    elif isinstance(o, list):
        if o: walk(o[0], path+"[0]", depth+1)
walk(pf, "pf")
