import UnityPy, re, struct, os

BD = r"\\MYAPPSANDBOX\c\Games\MFG\game\MFGClient_Data\StreamingAssets\aa\StandaloneWindows64\3633e69d040cf55c4855557f2daec047.bundle"
env = UnityPy.load(BD)
mats = [o for o in env.objects if getattr(o.type, "name", "?") == "Material"]
obj = mats[0]
raw = obj.get_raw_data()
print("raw len:", len(raw))
i = raw.find(b"CharaSelectNum1")
print("needle at:", i)
seg = raw[max(0, i - 60):i + 80]
print("context:", seg)
# list all strings in raw (len-prefixed)
for m in re.finditer(rb"[ -~]{5,}", raw):
    print(m.start(), repr(m.group()))
# check external references (guids)
af = obj.assetsfile
ext = getattr(af, "external", None)
print("external files:", ext if ext is None else [getattr(e, "name", "?") for e in ext])
