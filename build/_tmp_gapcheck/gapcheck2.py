import json

P = r"\\MYAPPSANDBOX\c\Games\MFG\game\MFGClient_Data\StreamingAssets\aa\catalog.json"
cat = json.load(open(P, encoding="utf-8"))
ids = cat["m_InternalIds"]
print("total internal ids:", len(ids))
print("prefixes:", cat.get("m_InternalIdPrefixes"))
idx = [i for i, s in enumerate(ids) if isinstance(s, str) and "CharaSelectNum1" in s]
print("idx:", idx)
for i in idx:
    for j in range(max(0, i - 2), min(len(ids), i + 3)):
        print((">> " if j == i else "   ") + str(ids[j]))
nobar = [s for s in ids if isinstance(s, str) and ".bundle" not in s]
print("entries with no '.bundle' substring:", len(nobar))
for s in nobar[:8]:
    print("   ", s)
black = [s for s in ids if isinstance(s, str) and "W5 Black.mat" in s]
print("W5 Black.mat forms:", len(black))
for s in black[:4]:
    print("   ", s[:200])
strs = [s for s in ids if isinstance(s, str)]
print("non-str entries:", len(ids) - len(strs))
bundled = [s for s in strs if ".bundle/" in s]
print("entries with '.bundle/':", len(bundled))
