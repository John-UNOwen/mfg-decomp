import json

P = r"\\MYAPPSANDBOX\c\Games\MFG\game\MFGClient_Data\StreamingAssets\aa\catalog.json"
raw = open(P, encoding="utf-8").read()
i = raw.find("CharaSelectNum1.mat")
print("context:")
print(raw[max(0, i - 400):i + 60])
cat = json.loads(raw)
print("catalog keys:", list(cat.keys()))
ids = [s for s in map(str, cat.get("m_InternalIds", [])) if "CharaSelectNum" in s]
print("internalid hits:", len(ids))
for h in ids[:3]:
    print("  ", h)
# look for where asset paths live if not in internal ids
for key in cat:
    blob = json.dumps(cat[key])
    if "CharaSelectNum1" in blob:
        print("found in section:", key)
