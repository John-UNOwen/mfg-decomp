import sqlite3, json, sys, io
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding="utf-8", errors="replace")

db = r"C:\Users\<you>\Documents\GitHub\mfg-decomp\.audit\copy2.db"
con = sqlite3.connect(db)
cur = con.cursor()
rows = list(cur.execute("SELECT _id,__s,__refid,data FROM documents ORDER BY _id"))
print("total docs:", len(rows))
for (_id, s, refid, d) in rows:
    o = json.loads(d)
    coll = o.get("collection")
    if coll != "profile":
        continue
    if not (str(refid).startswith("MID") or "AUDIT" in str(refid)):
        continue
    st = o.get("states") or {}
    print(f"\n_id={_id} __s={s} refid={refid} name={o.get('name')!r} mid={o.get('player_id')} guest={o.get('is_guest')} session={str(o.get('session_id'))[:8]}")
    for k in sorted(st):
        v = st[k]
        ok = "JSON-OK"
        try:
            json.loads(v)
        except Exception as e:
            ok = f"NOT-JSON({type(e).__name__})"
        print(f"   {k}: len={len(v)} {ok} repr={repr(v[:120])}")
con.close()
