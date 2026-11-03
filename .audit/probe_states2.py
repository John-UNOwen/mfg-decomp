import sqlite3, json, sys, io, collections
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding="utf-8", errors="replace")

db = r"C:\Users\<you>\Documents\GitHub\mfg-decomp\.audit\copy.db"
con = sqlite3.connect(db)
cur = con.cursor()

UNLOCK = {176:11,206:12,247:13,279:14,305:15,338:16,386:17,411:18,439:19,493:20}

for (_id, refid, d) in cur.execute("SELECT _id,__refid,data FROM documents WHERE __s='plugins_profile' ORDER BY _id"):
    o = json.loads(d)
    st = o.get("states") or {}
    pg = st.get("player_game")
    if not pg:
        continue
    pgo = json.loads(pg)
    cust = json.loads(st.get("customize_item") or "{}")
    cut = cust.get("CutinItems") or []
    cutids = set()
    for x in cut:
        v = x.get("ItemID") if isinstance(x, dict) else x
        if isinstance(v, int):
            cutids.add(v)
    proven = sorted(ch for iid, ch in UNLOCK.items() if iid in cutids)
    gcl = pgo.get("GachaCharaUsableList")
    pcl = pgo.get("PurchaseCharaList")
    print(f"--- refid={refid} mid={o.get('player_id')}")
    print(f"    player_game keys({len(pgo)}): {sorted(pgo.keys())}")
    print(f"    GachaCharaUsableList={gcl}  PurchaseCharaList={pcl}  cutin_count={len(cut)}  proven_by_unlockitem={proven}")
    print(f"    customize keys: {sorted(cust.keys())}")
    for k in ("player_item", "player_paid_item"):
        v = st.get(k)
        if v is not None:
            print(f"    {k} = {v[:400]}")
    for k in ("version_game", "cc_request", "chara_stamp_event", "bingo_event_remake", "others_event", "scramble_festival"):
        if k in st:
            print(f"    {k} = {st[k][:300]}")
    pgflat = {k: pgo.get(k) for k in ("FightStar","YellRank","m_danLevel","SelectChara","PlayerName","m_dojoSlotUnlocked","MatchingVersion","TMakeVersion","DataVersion","IsOdekake","odekake_id")}
    print("    player_game flat:", json.dumps(pgflat, ensure_ascii=False))
    sh = pgo.get("StampHistory")
    print("    StampHistory type/len:", type(sh).__name__, len(sh) if isinstance(sh,(list,dict,str)) else sh)
    con.close() if False else None
con.close()
