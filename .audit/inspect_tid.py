import sqlite3, json, sys, io, collections
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding="utf-8", errors="replace")
con = sqlite3.connect(r"C:\mfg\tmp\RyuNET-core\dist\savedata\mfg@asphyxia.db")
cur = con.cursor()
rows = list(cur.execute("SELECT _id,createdAt,data FROM documents WHERE data LIKE '%match_archive%' ORDER BY createdAt"))
c = collections.Counter()
print("archives in createdAt order (order DB.Find returns, since APIFind sorts createdAt asc):")
for _id, createdAt, d in rows:
    o = json.loads(d)
    c[o["tid"]] += 1
    print(f"  _id={_id} createdAt={createdAt} tid={o['tid']} ts={o['timestamp']} cells={len(o.get('cells') or [])} gmode={o.get('gmode')} seats={o.get('seats')}")
print("tid duplication:", {k: v for k, v in c.items() if v > 1})
