import sqlite3, json, collections, sys, io
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding="utf-8", errors="replace")

db = r"C:\mfg\tmp\RyuNET-core\dist\savedata\mfg@asphyxia.db"
con = sqlite3.connect(db)
cur = con.cursor()
for (_id, s, refid, d) in cur.execute("SELECT _id,__s,__refid,data FROM documents ORDER BY _id"):
    o = json.loads(d)
    states = o.get("states") or {}
    print(f"_id={_id} __s={s} refid={refid} coll={o.get('collection')}")
    if o.get("collection") == "profile":
        print(f"   name={o.get('name')} player_id={o.get('player_id')} session={str(o.get('session_id'))[:8]} "
              f"fav={o.get('favorite_character')} gacha_txn={bool(o.get('gacha_txn'))} last_daily={o.get('last_daily')} "
              f"welcome={o.get('welcome_gifts')} dojo={json.dumps(o.get('dojo'))[:200]}")
        print(f"   states({len(states)}): {sorted(states.keys())}")
        pg = states.get("player_game")
        if pg:
            try:
                pgo = json.loads(pg)
                keep = {k: pgo.get(k) for k in ("DataVersion", "PlayerName", "SelectChara", "FightStar", "m_danLevel",
                                                "GachaCharaUsableList", "PurchaseCharaList", "m_dojoSlotUnlocked",
                                                "StampHistory", "MatchingVersion", "TMakeVersion", "YellRank")}
                print("   player_game:", json.dumps(keep, ensure_ascii=False)[:600])
            except Exception as e:
                print("   player_game UNPARSEABLE:", repr(pg[:200]))
        pi = states.get("player_item")
        if pi:
            print("   player_item keys:", list(json.loads(pi).keys()) if pi.strip().startswith("{") else "notobj")
            try:
                pio = json.loads(pi)
                for k, v in pio.items():
                    print(f"      {k}: {type(v).__name__} len={len(v) if isinstance(v,(list,str,dict)) else v}")
            except Exception:
                pass
        pb = states.get("present_box_game")
        if pb:
            try:
                pbo = json.loads(pb)
                print("   present_box_game:", json.dumps(pbo, ensure_ascii=False)[:800])
            except Exception:
                pass
    elif o.get("collection") == "match_archive":
        cells = o.get("cells") or []
        print(f"   tid={o.get('tid')} gmode={o.get('gmode')} seats={o.get('seats')} ts={o.get('timestamp')} cells={len(cells)}")
        print("   cell tags:", [c[:40] for c in cells][:6])
        print("   players:", json.dumps([{k: p.get(k) for k in ('pindex','name','mid','refid','rank','score','uma','yakuman','buttobi')} for p in o.get('players',[])], ensure_ascii=False)[:700])
    else:
        print("   keys:", sorted(k for k in o if not k.startswith("__")))
    print()
