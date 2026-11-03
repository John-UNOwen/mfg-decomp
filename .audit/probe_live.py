import urllib.request, urllib.parse, base64, re, json, sys, io
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding="utf-8", errors="replace")

BASE = "http://127.0.0.1:8083/aog/"
REF = "AUDITPROBE00001"

def post(name, fields):
    body = urllib.parse.urlencode(fields).encode()
    req = urllib.request.Request(BASE + name, data=body, headers={"Content-Type": "application/x-www-form-urlencoded"})
    with urllib.request.urlopen(req, timeout=20) as r:
        return r.read().decode("utf-8", "replace")

def b64e(s):
    return base64.b64encode(s.encode("utf-8")).decode()

def states_of(xml):
    out = {}
    for m in re.finditer(r'<state kind="([^"]+)"><data>([^<]*)</data></state>', xml):
        kind = m.group(1).replace("&quot;", '"').replace("&amp;", "&")
        try:
            out[kind] = base64.b64decode(m.group(2)).decode("utf-8", "replace")
        except Exception as e:
            out[kind] = f"<base64 decode error: {e}>"
    return out

KINDS = ["bingo_event", "bingo_event_remake", "blow_away_event_sanma", "cc_request",
         "chara_stamp_event", "customize_item", "ea_shop", "event_taku", "friend_game",
         "gacha", "log", "mission", "notice_game", "others_event", "player_config",
         "player_game", "player_item", "player_paid_item", "present_box_game",
         "scramble_festival", "story", "tenshi_gowan_event", "version_game",
         "vote_event", "yell_campaign", "music_gacha", "receive_only",
         "competition_game", "competition_latest_result", "shop_competition_game",
         "shop_competition_latest_result", "audit_plain"]

print("=== login/create/menudata ===")
r = post("login", {"system_id": "AUDIT", "sic": "VFG:J:A:A:2026060900", "loc_id": "ea",
                   "web_id": "1", "area_id": "1", "user_id": REF, "bypass_auth": "true", "guest": "1"})
sess = re.search(r"<session_id>([0-9a-f]+)</session_id>", r)
sess = sess.group(1) if sess else ""
print("session:", sess[:8], "code:", re.search(r"<code>(\d+)</code>", r).group(1))
r = post("create_player", {"web_id": "1", "area_id": "1", "pcuid": sess, "user_id": REF, "name": "PLAYER"})
r = post("get_menudata", {"web_id": "1", "area_id": "1", "pcuid": sess})
mid = re.search(r"<mid>(\d+)</mid>", r)
mid = mid.group(1) if mid else "0"
print("mid:", mid)

print("\n=== write all kinds (Unity-style EscapeURL of base64) ===")
sent = {}
for k in KINDS:
    payload = json.dumps({"DataVersion": 1, "probe": k, "marker": f"MARK-{k}"}, ensure_ascii=False)
    sent[k] = payload
    # EscapeURL: escape everything non-unreserved, so '+' -> %2B. Use quote with safe=''
    body = {"web_id": "1", "area_id": "1", "pcuid": sess, "mid": mid, "kind": k,
            "data": urllib.parse.quote(payload, safe="")}
    rr = post("client_state_write", body)
    ok = "<serv_st><code>0</code>" in rr
    if not ok:
        print("write FAILED", k, rr[:200])

r = post("client_state_read", {"web_id": "1", "area_id": "1", "pcuid": sess, "mid": mid})
got = states_of(r)
print(f"\nread returned {len(got)} states: {sorted(got)}")
for k in KINDS:
    g = got.get(k)
    if g is None:
        print(f"  MISSING   {k}")
    elif g != sent[k]:
        print(f"  MUTATED   {k}\n     sent: {sent[k]}\n     got : {g[:400]}")

print("\n=== one_kind reads ===")
for one in ["mission", "player_game", "friend_game", "present_box_game", "not_a_kind"]:
    r = post("client_state_read", {"web_id": "1", "area_id": "1", "pcuid": sess, "mid": mid, "one_kind": one})
    g = states_of(r)
    print(f"  one_kind={one} -> {sorted(g)}")

print("\n=== corrupted-JSON write (silent drop?) ===")
good = json.dumps({"DataVersion": 1, "probe": "mission", "marker": "GOOD"})
post("client_state_write", {"web_id": "1", "area_id": "1", "pcuid": sess, "mid": mid,
                            "kind": "mission", "data": urllib.parse.quote(good, safe="")})
broken = '{"DataVersion":1,"probe":"mission","marker":"BROKEN","Items":[{"ItemID":1,"m_cou'
rr = post("client_state_write", {"web_id": "1", "area_id": "1", "pcuid": sess, "mid": mid,
                                 "kind": "mission", "data": urllib.parse.quote(broken, safe="")})
print("  response to corrupted write:", rr.replace("\n", "")[:220])
r = post("client_state_read", {"web_id": "1", "area_id": "1", "pcuid": sess, "mid": mid, "one_kind": "mission"})
print("  mission state now:", states_of(r).get("mission"))

print("\n=== literal '+' in base64 (unescaped in body) ===")
# craft payload whose base64 contains '+' and '/'
p = json.dumps({"DataVersion": 1, "probe": "audit_plain", "v": "~~~~~~~~~~~~~"}, ensure_ascii=False)
b = b64e(p)
print("  payload:", p)
print("  base64  :", b, "contains+:", "+" in b, "contains/:", "/" in b)
raw_body = f"web_id=1&area_id=1&pcuid={sess}&mid={mid}&kind=audit_plain&data={b}"  # unescaped
req = urllib.request.Request(BASE + "client_state_write", data=raw_body.encode(),
                             headers={"Content-Type": "application/x-www-form-urlencoded"})
with urllib.request.urlopen(req, timeout=20) as resp:
    rr = resp.read().decode("utf-8", "replace")
r = post("client_state_read", {"web_id": "1", "area_id": "1", "pcuid": sess, "mid": mid, "one_kind": "audit_plain"})
g = states_of(r).get("audit_plain")
print("  unescaped-plus write -> stored:", repr(g))
print("  match:", g == p)

print("\n=== write with stale pcuid (orphan profile fork?) ===")
rr = post("client_state_write", {"web_id": "1", "area_id": "1", "pcuid": "deadbeefdeadbeef",
                                 "mid": "998877", "kind": "audit_plain",
                                 "data": urllib.parse.quote('{"orphan":1}', safe="")})
print("  response:", rr.replace("\n", "")[:200])

print("\n=== player_game prune check on test profile ===")
pg = json.dumps({"DataVersion": 14, "PlayerName": "AUDIT", "SelectChara": 0, "DataVersionX": 1,
                 "GachaCharaUsableList": [11, 12, 13], "PurchaseCharaList": ["Chara15", 9]},
                ensure_ascii=False)
post("client_state_write", {"web_id": "1", "area_id": "1", "pcuid": sess, "mid": mid,
                            "kind": "player_game", "data": urllib.parse.quote(pg, safe="")})
cust = json.dumps({"DataVersion": 1, "CutinItems": [{"ItemID": 176, "IsOn": True}, {"ItemID": 206, "IsOn": True}]}, ensure_ascii=False)
post("client_state_write", {"web_id": "1", "area_id": "1", "pcuid": sess, "mid": mid,
                            "kind": "customize_item", "data": urllib.parse.quote(cust, safe="")})
r = post("client_state_read", {"web_id": "1", "area_id": "1", "pcuid": sess, "mid": mid, "one_kind": "player_game"})
print("  served player_game:", states_of(r).get("player_game"))
