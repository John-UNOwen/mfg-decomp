import json, collections, pathlib, sys, io
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding="utf-8", errors="replace")

ROOT = pathlib.Path(r"C:\mfg\tmp\RyuNET-core\dist\plugins\mfg@asphyxia\data")
pools = json.loads((ROOT / "gacha_pools.json").read_text(encoding="utf-8"))
std = pools["standard_pool"]
idx = pools["item_index"]
cut = json.loads((ROOT / "cutin_items.json").read_text(encoding="utf-8"))
oid2item = {k.upper(): v for k, v in cut["oid_to_item_id"].items()}

print("standard_pool:", len(std), "item_index:", len(idx))
missing_idx = [o for o in std if o not in idx]
print("std items with no item_index entry:", len(missing_idx), missing_idx[:8])
rar = collections.Counter(v[1] for v in idx.values())
print("rarity vocabulary:", dict(rar))
chra = collections.Counter(v[0] for v in idx.values())
print("chara keys:", sorted(chra)[:25])
# how many std items have no cutin item id mapping
print("std items not in cutin_items map:", [o for o in std if o.upper() not in oid2item][:10])

per_chara_rarity = collections.defaultdict(collections.Counter)
for o in std:
    v = idx.get(o)
    if v:
        per_chara_rarity[v[0]][v[1]] += 1
for c in sorted(per_chara_rarity):
    print(c, dict(per_chara_rarity[c]))

# per-banner pool modelling: featured items
series = pools["series"]
counts = collections.Counter()
for sid, e in series.items():
    if e["type"] != "Pickup":
        continue
    nums = {c.replace("Chara", "") for c in e["pickup_charas"]}
    feat = [o for o in std if idx.get(o, [""])[0] in nums]
    counts[(len(e["pickup_charas"]), len(feat))] += 1
print("featured-pool sizes for Pickup banners (charas,items):", dict(counts))

# limited
for sid, e in series.items():
    if e["type"] == "Limited":
        print("limited", sid, e["name"], "extra:", e["extra_items"])
        print("   rarities:", [idx.get(o, [None, None])[1] for o in e["extra_items"]])
print("chara_unlock_items:", pools["chara_unlock_items"])
