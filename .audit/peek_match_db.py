"""Peek at match_archive + profile docs in the live plugin DB (read-only).

usage: python .audit/peek_match_db.py [db-path]
"""
import collections
import io
import json
import sqlite3
import sys

sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding="utf-8", errors="replace")
DB = sys.argv[1] if len(sys.argv) > 1 else r"C:\mfg\tmp\RyuNET-core\dist\savedata\mfg@asphyxia.db"
con = sqlite3.connect(f"file:{DB}?mode=ro", uri=True)
cur = con.cursor()

tables = [r[0] for r in cur.execute("SELECT name FROM sqlite_master WHERE type='table'")]
print("tables:", tables)

rows = list(cur.execute("SELECT _id, createdAt, data FROM documents WHERE data LIKE '%match_archive%' ORDER BY createdAt"))
print(f"\nmatch_archive docs: {len(rows)}")
tids = collections.Counter()
keys = collections.Counter()
plkeys = collections.Counter()
gmodes = collections.Counter()
for _id, createdAt, d in rows:
    o = json.loads(d)
    keys.update(o.keys())
    tids[o.get("tid")] += 1
    gmodes[o.get("gmode")] += 1
    for p in (o.get("players") or []):
        plkeys.update(p.keys())
print("archive field keys:", dict(keys))
print("player field keys:", dict(plkeys))
print("gmode values:", dict(gmodes))
dupes = {k: v for k, v in tids.items() if v > 1}
print("duplicate tids:", dupes)
print("\nsample archive (trimmed):")
if rows:
    o = json.loads(rows[-1][2])
    o["cells"] = f"<{len(o.get('cells') or [])} cells>"
    print(json.dumps(o, ensure_ascii=False, indent=2)[:1800])

prof = list(cur.execute("SELECT _id, createdAt, data FROM documents WHERE data LIKE '%\"profile\"%' ORDER BY createdAt"))
print(f"\nprofile-like docs: {len(prof)}")
for _id, createdAt, d in prof[:6]:
    o = json.loads(d)
    if o.get("collection") != "profile":
        continue
    print(f"  refid={o.get('refid')} player_id={o.get('player_id')} name={o.get('name')} "
          f"is_guest={o.get('is_guest')} session={str(o.get('session_id'))[:8]}")

meta = list(cur.execute("SELECT _id, createdAt, data FROM documents WHERE data LIKE '%match_meta%'"))
print(f"\nmatch_meta docs: {len(meta)}")
for _id, createdAt, d in meta[:5]:
    print("  ", d[:300])
