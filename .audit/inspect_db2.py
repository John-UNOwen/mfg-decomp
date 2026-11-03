import sqlite3, json, collections

db = r"C:\mfg\tmp\RyuNET-core\dist\savedata\mfg@asphyxia.db"
con = sqlite3.connect(db)
cur = con.cursor()
for (_id, s, refid, d) in cur.execute("SELECT _id,__s,__refid,data FROM documents"):
    o = json.loads(d)
    keys = sorted(k for k in o.keys() if not k.startswith("__"))
    print(f"_id={_id} __s={s} __refid={refid} keys={keys}")
    states = o.get("states") or {}
    print("   states:", sorted(states.keys()))
    if o.get("dojo") is not None:
        print("   dojo:", json.dumps(o["dojo"])[:400])
    for k in ("favorite_character", "gacha_txn", "last_daily", "welcome_gifts", "name", "player_id", "session_id", "collection", "tid", "gmode", "seats", "timestamp", "cells"):
        if k in o:
            v = o[k]
            if isinstance(v, list) and k == "cells":
                v = f"<{len(v)} cells> first={v[0][:120] if v else ''}"
            print(f"   {k} = {str(v)[:300]}")
    if "players" in o:
        print("   players:", json.dumps(o["players"])[:600])
    print()
