import sqlite3, json, collections, sys

DB = r"C:\mfg\tmp\RyuNET-core\dist\savedata\mfg@asphyxia.db"
con = sqlite3.connect(f"file:{DB}?mode=ro&nolock=1", uri=True)
cur = con.cursor()
print("== tables ==")
rows = cur.execute("SELECT name FROM sqlite_master WHERE type='table'").fetchall()
print([r[0] for r in rows])

# find the documents table used by asphyxia plugin DB
for t in [r[0] for r in rows]:
    cols = [c[1] for c in cur.execute(f"PRAGMA table_info({t})").fetchall()]
    n = cur.execute(f"SELECT COUNT(*) FROM {t}").fetchone()[0]
    print(f"table {t} cols={cols} rows={n}")
con.close()
