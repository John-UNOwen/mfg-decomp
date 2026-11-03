import json, re, sys, io, collections
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding="utf-8", errors="replace")

pools = json.load(open(r"C:\mfg\tmp\RyuNET-core\dist\plugins\mfg@asphyxia\data\gacha_pools.json", encoding="utf-8"))
oids = set()
for o in pools.get("standard_pool") or []:
    oids.add(o)
for sid, e in (pools.get("series") or {}).items():
    for key in ("custom_pickup_items", "extra_items", "music_items"):
        for o in (e.get(key) or []):
            oids.add(o)
for o in (pools.get("item_index") or {}):
    oids.add(o)
for e in (pools.get("chara_unlock_items") or {}).values():
    oids.add(e)

# gather client master sources
import pathlib
roots = [r"C:\mfg\build\unity-project\Assets\Assembly-CSharp\MFG.MasterGameData",
         r"C:\mfg\build\unity-project\Assets\Assembly-CSharp"]
texts = {}
for r in roots:
    p = pathlib.Path(r)
    for name in ("CutinItemMaster.cs", "CustomizeItemMaster.cs", "MusicGachaMaster.cs", "PlayerItemMaster.cs"):
        f = p / name
        if f.exists() and name not in texts:
            texts[name] = f.read_text(encoding="utf-8", errors="replace")
print("master files loaded:", {k: len(v) for k, v in texts.items()})

cutin = texts.get("CutinItemMaster.cs", "")
cust = texts.get("CustomizeItemMaster.cs", "")
music = texts.get("MusicGachaMaster.cs", "")
allmaster = cutin + cust + music
known = set(re.findall(r'"(OID_[^"]+)"', allmaster))
print("distinct OIDs in masters:", len(known))

missing = sorted(o for o in oids if o not in known)
print("asphyxia pool OIDs total:", len(oids))
print("missing from client masters:", len(missing))
print("\n".join(missing[:60]))
