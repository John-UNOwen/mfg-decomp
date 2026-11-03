"""Dump match_archive participant attribution + profile mid map (read-only).

usage: python .audit/inspect_archive_rows.py [db-path]
"""
import collections
import io
import json
import sqlite3
import sys

sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding="utf-8", errors="replace")
DB = sys.argv[1] if len(sys.argv) > 1 else r"C:\mfg\tmp\RyuNET-core\dist\savedata\mfg@asphyxia.db"
con = sqlite3.connect(f"file:{DB}?mode=ro", uri=True)

print("== match_archive participant rows ==")
tids = collections.Counter()
for row in con.execute("SELECT data FROM documents WHERE data LIKE '%match_archive%' ORDER BY createdAt"):
    o = json.loads(row[0])
    tids[o.get("tid")] += 1
    humans = [(p.get("name"), p.get("mid"), p.get("refid")) for p in o.get("players", []) if p.get("mid")]
    print(f"tid={o.get('tid')} gmode={o.get('gmode')} seats={o.get('seats')} humans={humans}")
print("tid multiplicity:", dict(tids))

print("\n== profiles (mid / guest) ==")
mids = collections.Counter()
for row in con.execute("SELECT data FROM documents WHERE data LIKE '%player_id%' ORDER BY createdAt"):
    o = json.loads(row[0])
    if o.get("collection") != "profile":
        continue
    mids[o.get("player_id")] += 1
    if len(str(o.get("player_id"))) < 8 or o.get("refid") in ("GUEST", "MFGTESTGUEST1"):
        print(f"  notable: refid={o.get('refid')} mid={o.get('player_id')} name={o.get('name')} guest={o.get('is_guest')}")
print(f"profiles={sum(mids.values())} distinct_mid={len(mids)} collisions={ {k: v for k, v in mids.items() if v > 1} }")
