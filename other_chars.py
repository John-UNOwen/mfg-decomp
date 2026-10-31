import re, glob, os
pats = glob.glob(r"C:\mfg\build\unity-project\Assets\MFG\Characters\Character*\3D\*\Prefabs\CharaModel.prefab", recursive=False)
pats += glob.glob(r"C:\mfg\build\unity-project\Assets\MFG\Characters\Character*\3D\*\*\Prefabs\CharaModel.prefab")
pats = sorted(set(pats))
print(len(pats), "prefabs found")
rootp = r"C:\mfg\build\unity-project\Assets\MFG\Characters"
for p in pats:
    t = open(p, encoding='utf-8', errors='replace').read()
    blocks = re.findall(r'--- !u!1 &(\d+)\nGameObject:.*?m_Name: (.+?)\n.*?m_IsActive: (\d)', t, re.S)
    onames = [(nm, act) for gid,nm,act in blocks if 'outlien' in nm.lower() or 'outline' in nm.lower()]
    short = os.path.relpath(p, rootp)
    print(short, "->", onames if onames else "NO outline-named GO")
