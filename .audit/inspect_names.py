import sqlite3, json, sys, io
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding="utf-8", errors="replace")
con = sqlite3.connect(r"C:\mfg\tmp\RyuNET-core\dist\savedata\mfg@asphyxia.db")
q = "SELECT _id,__refid,data FROM documents WHERE json_extract(data,'$.collection')='profile' ORDER BY createdAt"
for _id, refid, d in con.execute(q):
    o = json.loads(d)
    pg = (o.get("states") or {}).get("player_game")
    pn = None
    if pg:
        try:
            pn = json.loads(pg).get("PlayerName")
        except Exception:
            pn = "<unparseable>"
    print(f"{_id} refid={refid} name={o.get('name')!r} statePlayerName={pn!r} mid={o.get('player_id')} states={len(o.get('states') or {})}")
