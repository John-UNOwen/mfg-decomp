import urllib.request, urllib.parse, base64, re, json, sys, io
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

def write(kind, obj):
    post("client_state_write", f"web_id=1&area_id=1&pcuid={sess}&mid={mid}&kind={kind}&data={urllib.parse.quote(base64.b64encode(json.dumps(obj, ensure_ascii=False).encode()).decode(), safe='')}")

def name_in_menu():
    return re.search(r"<mpdata><mid>(\d+)</mid><name>(.*?)</name></mpdata>", post("get_menudata", f"web_id=1&area_id=1&pcuid={sess}")).group(2)

def stored_name():
    st = states_of(post("client_state_read", f"web_id=1&area_id=1&pcuid={sess}&mid={mid}&one_kind=player_game"))
    return json.loads(st["player_game"]).get("PlayerName")

write("player_game", {"DataVersion": 14, "PlayerName": "AuditNameA", "GachaCharaUsableList": [], "PurchaseCharaList": []})
print("after write A: menudata name =", name_in_menu(), "| player_game state name =", stored_name())
write("player_game", {"DataVersion": 14, "PlayerName": "AuditNameB", "GachaCharaUsableList": [], "PurchaseCharaList": []})
print("after write B: menudata name =", name_in_menu(), "| player_game state name =", stored_name())

# plus-in-base64 log garble
import base64 as b64
text = "AUDITLOG plus-case marker"
cand = None
for n in range(200):
    s = text + str(n)
    enc = b64.b64encode(s.encode()).decode()
    if "+" in enc:
        cand = (s, enc); break
s, enc = cand
print("\nlog text  :", s)
print("base64    :", enc, "(contains literal '+')")
post("gacha_log", f"web_id=1&area_id=1&pcuid={sess}&log={enc}")
