"""Generate data/cutin_items.json for the mfg@asphyxia plugin.

The plugin needs the cutin ObjectID <-> MFG.Types.Customize.CutinItem.ItemID
int mapping to decide, server-side, whether a gacha draw is a duplicate of
something the profile already owns (the client keeps its cutins as ItemID
ints in the "customize_item" state).

It also needs each cutin's CharaType and Rarity: the per-banner gacha pools
are built from the featured charas of the banner, and the rarity buckets are
what the advertised pickup_rate numbers roll against.

Both sides come from the recovered client source:
  api/recovered/Assembly-CSharp/MFG.Types.Customize.CutinItem/ItemID.cs   (enum order = ints)
  api/recovered/Assembly-CSharp/MFG.MasterGameData/CutinItemMaster.cs     (ObjectID <-> ItemID, Rarity, CharaType, _defaultItems)
"""
import json
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
ENUM = ROOT / "api/recovered/Assembly-CSharp/MFG.Types.Customize.CutinItem/ItemID.cs"
MASTER = ROOT / "api/recovered/Assembly-CSharp/MFG.MasterGameData/CutinItemMaster.cs"
# Canonical plugin tree - the copy the core loads. The tmp/asphyxia_plugins clone is a
# staging mirror refreshed from it (pwsh scripts/ryunet_stage.ps1 -Action mirror), never a
# target of its own.
CANON = ROOT / "tmp/RyuNET-core/dist/plugins/mfg@asphyxia/data"
OUT = CANON / "cutin_items.json"

enum_text = ENUM.read_text(encoding="utf-8")
body = re.search(r"public enum ItemID\s*\{(.*?)\n\}", enum_text, re.S).group(1)
names = [n.strip() for n in body.split(",") if n.strip()]
enum_int = {n: i for i, n in enumerate(names)}  # None = 0, then 1..n

master = MASTER.read_text(encoding="utf-8")
pairs = re.findall(r'ObjectID = "(OID_[^"]+)",\s*\n\s*ItemID = ItemID\.(\w+),', master)
# ObjectID -> (Rarity, CharaType) of the same master entry; the plugin rolls
# per-banner rarity buckets off these, so the odds it advertises are the
# rarities the client itself shows for the item.
entry_facts = re.findall(
    r'ObjectID = "(OID_[^"]+)",\s*\n\s*ItemID = ItemID\.(\w+),'
    r'.*?Rarity = Rarity\.(\w+),\s*\n\s*CharaType = CharaType\.(\w+)',
    master,
    re.S,
)

oid_to_id = {}
oid_info = {}
dupes = []
missing = []
for oid, name in pairs:
    if name not in enum_int:
        missing.append(f"{oid} -> {name}")
        continue
    if oid in oid_to_id:
        dupes.append(oid)
    oid_to_id[oid] = enum_int[name]

for oid, name, rarity, chara in entry_facts:
    if oid not in oid_to_id:
        continue
    m = re.match(r"Chara(\d{2})$", chara)
    if not m:
        continue
    oid_info[oid] = {"item_id": enum_int[name], "rarity": rarity,
                     "chara": int(m.group(1)) - 1}  # CharaType int: Chara01 = 0

dm = re.search(r"_defaultItems = new List<ItemID>\s*\{(.*?)\n\t\};", master, re.S)
default_ids = sorted({enum_int[n] for n in re.findall(r"ItemID\.(\w+),", dm.group(1)) if n in enum_int})

# every OID the gacha pools hand out must resolve, or the server cannot tell
# duplicate from new for it
pools = json.loads((CANON / "gacha_pools.json").read_text(encoding="utf-8"))
pool_oids = set(pools.get("item_index") or {})
for oid in pools.get("standard_pool") or []:
    pool_oids.add(oid)
for series in (pools.get("series") or {}).values():
    # music_items are ReachBgm/Voice OIDs (CustomizeItem/Voice masters), not
    # cutins, so they have no CutinItem.ItemID and are checked separately.
    for key in ("extra_items", "custom_pickup_items", "pickup_items"):
        for oid in series.get(key) or []:
            pool_oids.add(oid)
unmapped = sorted(o for o in pool_oids if o not in oid_to_id)
no_facts = sorted(o for o in pool_oids if o not in oid_info)

print(f"enum names: {len(names)}  master pairs: {len(pairs)}  mapped oids: {len(oid_to_id)}")
print(f"unresolved master ItemIDs: {missing[:5]}  duplicate oids: {dupes[:5]}")
print(f"pool oids: {len(pool_oids)}  unmapped pool oids: {len(unmapped)} {unmapped[:10]}")
print(f"pool oids without chara/rarity facts: {len(no_facts)} {no_facts[:10]}")
print(f"default item ids: {len(default_ids)}")

if missing or unmapped or no_facts:
    print("REFUSING to write: the mapping is incomplete")
    sys.exit(1)

OUT.write_text(json.dumps({
    "_generated_by": "scripts/gen_cutin_item_ids.py",
    "_source": "recovered Assembly-CSharp CutinItemMaster + MFG.Types.Customize.CutinItem.ItemID",
    "oid_to_item_id": oid_to_id,
    "oid_info": oid_info,
    "default_item_ids": default_ids,
}, ensure_ascii=False, indent=1) + "\n", encoding="utf-8")
print(f"wrote {OUT} ({OUT.stat().st_size} bytes)")
