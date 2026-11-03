import urllib.request, urllib.parse, base64, re, json, sys, io, shutil, sqlite3, os
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding="utf-8", errors="replace")
BASE = "http://127.0.0.1:8083/aog/"
REF = "AUDITPROBE00002"

def post(name, body):
    req = urllib.request.Request(BASE + name, data=body.encode(), headers={"Content-Type": "application/x-www-form-urlencoded"})
    with urllib.request.urlopen(req, timeout=30) as r:
        return r.read().decode("utf-8", "replace")

def states_of(xml):
    out = {}
    for m in re.finditer(r'<state kind="([^"]+)"><data>([^<]*)</data></state>', xml):
        out[m.group(1)] = base64.b64decode(m.group(2)).decode("utf-8", "replace")
    return out

sess = re.search(r"<session_id>([0-9a-f]+)</session_id>",
                 post("login", f"system_id=AUDIT2&sic=VFG:J:A:A:2026060900&loc_id=ea&web_id=1&area_id=1&user_id={REF}&bypass_auth=true&guest=1")).group(1)
mid = re.search(r"<mid>(\d+)</mid>", post("get_menudata", f"web_id=1&area_id=1&pcuid={sess}")).group(1)

def w(kind, obj):
    post("client_state_write", f"web_id=1&area_id=1&pcuid={sess}&mid={mid}&kind={kind}&data={urllib.parse.quote(base64.b64encode(json.dumps(obj, ensure_ascii=False).encode()).decode(), safe='')}")

w("customize_item", {"DataVersion": 1, "CustomizeItems": [], "CutinItems": [{"ItemID": 176}, {"ItemID": 206}]})
w("player_game", {"DataVersion": 14, "PlayerName": "テスト", "GachaCharaUsableList": [11, 12, 13, 14], "PurchaseCharaList": []})
served = json.loads(states_of(post("client_state_read", f"web_id=1&area_id=1&pcuid={sess}&mid={mid}&one_kind=player_game"))["player_game"])
print("client sent GachaCharaUsableList [11,12,13,14] with CutinItems [176,206]")
print("served by asphyxia           :", served["GachaCharaUsableList"])

src = r"C:\mfg\tmp\RyuNET-core\dist\savedata\mfg@asphyxia.db"
for suffix in ("", "-wal", "-shm"):
    if os.path.exists(src + suffix):
        shutil.copy2(src + suffix, r".audit\copy4.db" + suffix)
con = sqlite3.connect(r".audit\copy4.db")
row = con.execute("SELECT data FROM documents WHERE __refid=?", (REF,)).fetchone()
stored = json.loads(row[0])
print("stored in profile DB         :", stored["states"]["player_game"][:160])
print("stored GachaCharaUsableList  :", json.loads(stored["states"]["player_game"])["GachaCharaUsableList"])
print("prune persisted server-side? :", json.loads(stored["states"]["player_game"])["GachaCharaUsableList"] == [11, 12])
