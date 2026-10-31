import json, os, re, collections

P = r"\\MYAPPSANDBOX\c\Games\MFG\game\MFGClient_Data\StreamingAssets\aa\catalog.json"
cat = json.load(open(P, encoding="utf-8"))
ids = cat["m_InternalIds"]
forms = collections.Counter()
samples = {}
for s in ids:
    if s.startswith("{"):
        m = re.search(r"/([^/]+)\.(bundle|assetsif|assets)\b", s)
        key = "PREFIXED " + (m.group(2) if m else "other")
    elif s.startswith("Assets/"):
        key = "ASSETS_PATH " + os.path.splitext(s)[1]
    else:
        key = "OTHER " + os.path.splitext(s)[1]
    forms[key] += 1
    samples.setdefault(key, s)
for k, v in forms.most_common(15):
    print(f"{k}: {v}")
    print("   e.g.", samples[k][:180])
for s in ids:
    if s.startswith("{"):
        print("first prefixed:", s[:220])
        break

bd = r"\\MYAPPSANDBOX\c\Games\MFG\game\MFGClient_Data\StreamingAssets\aa\StandaloneWindows64"
files = os.listdir(bd)
print("files in bundle dir:", len(files))
byext = collections.Counter(os.path.splitext(f)[1] for f in files)
print(dict(byext))
fonts = [f for f in files if "font" in f.lower() or "Font" in f]
print("font-ish files:", fonts[:10])
