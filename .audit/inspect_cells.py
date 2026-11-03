import sqlite3, json, re, sys, io
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding="utf-8", errors="replace")
db = r"C:\mfg\tmp\RyuNET-core\dist\savedata\mfg@asphyxia.db"
con = sqlite3.connect(db)
cur = con.cursor()
row = cur.execute("SELECT data FROM documents WHERE data LIKE '%match_archive%' ORDER BY length(data) DESC").fetchone()
o = json.loads(row[0])
print("tid", o["tid"], "cells", len(o["cells"]))
prefix = {}
for i, c in enumerate(o["cells"]):
    m = re.match(r'^<(\S+?)\s', c)
    tag = m.group(1) if m else c[:30]
    km = re.search(r'kind="(\d+)"', c)
    key = f"{tag} kind={km.group(1) if km else '-'}"
    if key not in prefix:
        prefix[key] = (i, c)
for key, (i, c) in sorted(prefix.items()):
    print(f"\n=== [{i}] {key} len={len(c)} ===")
    print(c[:1200])
