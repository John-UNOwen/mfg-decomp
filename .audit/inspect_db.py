import sqlite3, json, collections, sys

db = r"C:\mfg\tmp\RyuNET-core\dist\savedata\mfg@asphyxia.db"
con = sqlite3.connect(db)
cur = con.cursor()
tables = [r[0] for r in cur.execute("SELECT name FROM sqlite_master WHERE type='table'")]
print("tables:", tables)
for t in tables:
    cols = [c[1] for c in cur.execute(f"PRAGMA table_info({t})")]
    n = cur.execute(f"SELECT COUNT(*) FROM {t}").fetchone()[0]
    print(f"-- {t} rows={n} cols={cols}")

# find state kinds
kinds = collections.Counter()
kinds_by_col = collections.Counter()
for t in tables:
    cols = [c[1] for c in cur.execute(f"PRAGMA table_info({t})")]
    if "data" not in cols:
        continue
    for (d,) in cur.execute(f"SELECT data FROM {t}"):
        if d is None:
            continue
        try:
            o = json.loads(d)
        except Exception:
            continue
        s = o.get("__s")
        for k, v in (o.get("states") or {}).items():
            kinds[k] += 1
            kinds_by_col[f"{s}|{k}"] += 1
print("state kinds:", dict(kinds))
print("by (space,kind):", dict(kinds_by_col))
