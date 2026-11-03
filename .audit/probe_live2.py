import urllib.request, urllib.parse, base64, re, json, sys, io
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding="utf-8", errors="replace")

BASE = "http://127.0.0.1:8083/aog/"
REF = "AUDITPROBE00002"

def post(name, raw_body):
    req = urllib.request.Request(BASE + name, data=raw_body.encode("utf-8"),
                                 headers={"Content-Type": "application/x-www-form-urlencoded"})
    with urllib.request.urlopen(req, timeout=25) as r:
        return r.read().decode("utf-8", "replace")

def esc(s):
    # Unity UnityWebRequest.EscapeURL: percent-encode everything except A-Za-z0-9-_.~
    return urllib.parse.quote(s, safe="")

def b64e(s):
    return base64.b64encode(s.encode("utf-8")).decode()

def b64d(s):
    return base64.b64decode(s).decode("utf-8", "replace")

def states_of(xml):
    out = {}
    for m in re.finditer(r'<state kind="([^"]+)"><data>([^<]*)</data></state>', xml):
        kind = m.group(1).replace("&quot;", '"').replace("&amp;", "&")
        try:
            out[kind] = b64d(m.group(2))
        except Exception as e:
            out[kind] = f"<b64err {e}>"
    return out

def write(sess, mid, kind, payload, raw=False):
    b = b64e(payload)
    data = b if raw else esc(b)
    body = f"web_id=1&area_id=1&pcuid={sess}&mid={mid}&kind={esc(kind)}&data={data}"
    return post("client_state_write", body)

def read(sess, mid, one=None):
    body = f"web_id=1&area_id=1&pcuid={sess}&mid={mid}"
    if one:
        body += f"&one_kind={esc(one)}"
    return states_of(post("client_state_read", body))

r = post("login", f"system_id=AUDIT2&sic=VFG:J:A:A:2026060900&loc_id=ea&web_id=1&area_id=1&user_id={REF}&bypass_auth=true&guest=1")
sess = re.search(r"<session_id>([0-9a-f]+)</session_id>", r).group(1)
post("create_player", f"web_id=1&area_id=1&pcuid={sess}&user_id={REF}&name=PLAYER")
r = post("get_menudata", f"web_id=1&area_id=1&pcuid={sess}")
mid = re.search(r"<mid>(\d+)</mid>", r).group(1)
name = re.search(r"<name>([^<]*)</name>", r).group(1)
print(f"probe2 refid={REF} session={sess[:8]} mid={mid} menudata_name={name}")

KINDS = ["bingo_event", "bingo_event_remake", "blow_away_event_sanma", "cc_request",
         "chara_stamp_event", "customize_item", "ea_shop", "event_taku", "friend_game",
         "gacha", "log", "mission", "notice_game", "others_event", "player_config",
         "player_item", "player_paid_item", "scramble_festival", "story",
         "tenshi_gowan_event", "version_game", "vote_event", "yell_campaign",
         "music_gacha", "receive_only", "competition_game", "competition_latest_result",
         "shop_competition_game", "shop_competition_latest_result", "audit_plain"]

print("\n=== faithful client round-trip (base64 + EscapeURL) ===")
sent = {}
for k in KINDS:
    payload = json.dumps({"DataVersion": 1, "probe": k, "marker": f"MARK-{k}"}, ensure_ascii=False)
    sent[k] = payload
    write(sess, mid, k, payload)
got = read(sess, mid)
bad = []
for k in KINDS:
    if k not in got:
        bad.append(f"MISSING {k}")
    elif got[k] != sent[k]:
        bad.append(f"MUTATED {k}\n   sent={sent[k]}\n   got ={got[k][:200]}")
print(f"served {len(got)} states; mismatches: {len(bad)}")
for b in bad:
    print("  " + b)

print("\n=== base64 containing literal '+' and '/' (properly escaped, like client) ===")
p = json.dumps({"DataVersion": 1, "kind": "audit_plain", "v": "\u30c6\u30b9\u30c8 \u65e5\u672c\u8a9e \u3042\u3044\u3046"}, ensure_ascii=False)
b = b64e(p)
print("  base64:", b, "| has+:", "+" in b, "has/:", "/" in b)
write(sess, mid, "audit_plain", p)
g = read(sess, mid, "audit_plain").get("audit_plain")
print("  roundtrip identical:", g == p, "| value:", g)

print("\n=== unescaped '+' in body (middleware turns + into space) ===")
p2 = json.dumps({"DataVersion": 1, "kind": "audit_plain", "v": "zzzzzzzzzzzzzzzzzz"}, ensure_ascii=False)
b2 = b64e(p2)
print("  base64:", b2, "has+:", "+" in b2)
write(sess, mid, "audit_plain", p2, raw=True)   # send raw base64 -> '+' stays literal in body
g = read(sess, mid, "audit_plain").get("audit_plain")
print("  stored after unescaped-plus write:", repr(g))
print("  identical:", g == p2)

print("\n=== corrupted JSON write: silent drop? ===")
good = json.dumps({"DataVersion": 1, "marker": "GOOD"}, ensure_ascii=False)
write(sess, mid, "audit_plain", good)
print("  before:", read(sess, mid, "audit_plain").get("audit_plain"))
broken = '{"DataVersion":1,"marker":"BROKEN","Items":[{"ItemID":1'   # valid base64, invalid JSON
resp = write(sess, mid, "audit_plain", broken)
print("  server response to broken write:", resp.replace("\n", "")[:160])
after = read(sess, mid, "audit_plain").get("audit_plain")
print("  after:", repr(after), "| unchanged(silent drop):", after == good)

print("\n=== non-JSON (plain text) state write ===")
write(sess, mid, "audit_plain", "hello-not-json")
print("  stored:", repr(read(sess, mid, "audit_plain").get("audit_plain")))

print("\n=== player_game normalize/prune (asphyxia read-time rewrite) ===")
pg = json.dumps({"DataVersion": 14, "PlayerName": "\u30c6\u30b9\u30c8", "SelectChara": 0,
                 "GachaCharaUsableList": [11, 12, 13, 14], "PurchaseCharaList": [14, 15]}, ensure_ascii=False)
write(sess, mid, "player_game", pg)
cust = json.dumps({"DataVersion": 1, "CutinItems": [{"ItemID": 176, "IsOn": True}, {"ItemID": 206, "IsOn": True}]}, ensure_ascii=False)
write(sess, mid, "customize_item", cust)
g = read(sess, mid, "player_game").get("player_game")
print("  sent player_game:", pg)
print("  served player_game:", g)
print("  DB stored player_game after write (read again):", repr(read(sess, mid, "player_game").get("player_game")))

print("\n=== cc_request canonicalisation ===")
cc = json.dumps({"DataVersion": 1, "now_idx": 9, "done_idx": 3,
                 "m_ccInfos": [{"idx": 2, "kind": "memorial", "param": "a", "name": "old"},
                               {"idx": 5, "kind": "memorial", "param": "b", "name": "keep"},
                               {"idx": 7, "kind": 5, "param": "c", "name": "wrongtype"}]}, ensure_ascii=False)
write(sess, mid, "cc_request", cc)
print("  sent:", cc)
print("  served:", read(sess, mid, "cc_request").get("cc_request"))

print("\n=== stale session write (session rotated by re-login) ===")
r = post("login", f"system_id=AUDIT2&sic=VFG:J:A:A:2026060900&loc_id=ea&web_id=1&area_id=1&user_id={REF}&bypass_auth=true&guest=1")
newsess = re.search(r"<session_id>([0-9a-f]+)</session_id>", r).group(1)
write(sess, mid, "audit_plain", json.dumps({"marker": "OLD-SESSION-WRITE"}, ensure_ascii=False))
g = read(newsess, mid, "audit_plain").get("audit_plain")
print("  old-pcuid write visible under new session:", repr(g))

print("\n=== write with mid=1 and bogus pcuid ===")
resp = post("client_state_write", f"web_id=1&area_id=1&pcuid=ffffffffffffffff&mid=1&kind=audit_plain&data={esc(b64e('{\"marker\":\"MID1\"}'))}")
print("  response:", resp.replace("\n", "")[:120])
