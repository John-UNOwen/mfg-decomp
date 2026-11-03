import sqlite3, json, sys, io
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding="utf-8", errors="replace")
con = sqlite3.connect(r"C:\mfg\tmp\RyuNET-core\dist\savedata\mfg@asphyxia.db")
q = "SELECT __refid,data FROM documents WHERE json_extract(data,'$.collection')='profile' ORDER BY createdAt"
for refid, d in con.execute(q):
    o = json.loads(d)
    st = o.get("states") or {}
    pi = st.get("player_item")
    pb = st.get("present_box_game")
    out = {"refid": refid}
    if pi:
        try:
            pio = json.loads(pi)
            out["player_item"] = [(it.get("ItemID"), it.get("m_count")) for it in pio.get("Items", [])]
        except Exception as e:
            out["player_item_err"] = str(e)
    if pb:
        try:
            pbo = json.loads(pb)
            out["presents"] = [(p.get("PresentID"), p.get("Content"), p.get("Count"), p.get("IsReceived"), p.get("ReceivedDate")) for p in pbo.get("PresentInfos", [])]
        except Exception as e:
            out["present_err"] = str(e)
    print(json.dumps(out, ensure_ascii=False))
