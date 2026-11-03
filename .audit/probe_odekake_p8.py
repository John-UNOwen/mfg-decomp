import urllib.request, urllib.parse, base64, re, json, sys, io

sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding="utf-8", errors="replace")
BASE = "http://127.0.0.1:8083/aog/"
LENDER = (sys.argv[1] if len(sys.argv) > 1 else "P8LENDER0001")[:16]
BORROWER = (sys.argv[2] if len(sys.argv) > 2 else "P8BORROW0001")[:16]
fails = []


def check(name, ok, detail=""):
    print(("PASS  " if ok else "FAIL  ") + name + (f"   {detail}" if detail else ""))
    if not ok:
        fails.append(name)


def post(name, fields):
    body = urllib.parse.urlencode(fields).encode()
    req = urllib.request.Request(BASE + name, data=body,
                                 headers={"Content-Type": "application/x-www-form-urlencoded"})
    with urllib.request.urlopen(req, timeout=25) as r:
        return r.read().decode("utf-8", "replace")


def code_of(xml):
    m = re.search(r"<serv_st><code>(\d+)</code></serv_st>", xml)
    return m.group(1) if m else "?"


def states_of(xml):
    out = {}
    for m in re.finditer(r'<state kind="([^"]+)"><data>([^<]*)</data></state>', xml):
        out[m.group(1)] = base64.b64decode(m.group(2)).decode("utf-8", "replace")
    return out


def login(user, pname):
    r = post("login", {"system_id": "P8", "sic": "VFG:J:A:A:2026060900", "loc_id": "ea", "web_id": "1",
                       "area_id": "1", "user_id": user, "bypass_auth": "true", "guest": "1"})
    sess = re.search(r"<session_id>([0-9a-f]+)</session_id>", r).group(1)
    post("create_player", {"web_id": "1", "area_id": "1", "pcuid": sess, "user_id": user, "name": pname})
    r = post("get_menudata", {"web_id": "1", "area_id": "1", "pcuid": sess})
    return sess, re.search(r"<mid>(\d+)</mid>", r).group(1)


def write(sess, mid, kind, payload):
    return post("client_state_write", {"web_id": "1", "area_id": "1", "pcuid": sess, "mid": mid,
                                       "kind": kind, "data": base64.b64encode(payload.encode("utf-8")).decode()})


# The lender: a real name plus the two states a borrow hands over. CharaData[2] is the
# chara on offer (OdekakeDone.cs:38-42 reads CharaData[SelectChara] of the lender).
lsess, lmid = login(LENDER, "P8LENDERNAME")
pg = json.dumps({"DataVersion": 1, "PlayerName": "P8LENDERNAME", "SelectChara": 2, "m_danLevel": 3,
                 "CharaData": [{"CharaType": i, "Level": 1} for i in range(5)]}, ensure_ascii=False)
ci = json.dumps({"DataVersion": 1, "CostumeDatas": [{"CharaType": 2, "CostumeID": 1}]}, ensure_ascii=False)
check("lender player_game write accepted", code_of(write(lsess, lmid, "player_game", pg)) == "0")
check("lender customize_item write accepted", code_of(write(lsess, lmid, "customize_item", ci)) == "0")

# The borrower: no states of its own, so the server's own bounds check is the one in play.
bsess, bmid = login(BORROWER, "P8BORROWERNAME")

r = post("client_state_read", {"web_id": "1", "area_id": "1", "pcuid": bsess, "mid": bmid, "one_kind": "friend_game"})
fr = json.loads(states_of(r).get("friend_game", "{}")).get("FriendInfos", [])
row = next((f for f in fr if f.get("OdekakeDataID") == int(lmid)), None)
check("friend_game publishes the lender as a row", row is not None, f"rows={len(fr)} {[f.get('PlayerName') for f in fr]}")
if row:
    check("the row is a servable borrow (IsUsable, CharaType, FriendID)",
          row.get("IsUsable") is True and row.get("CharaType") == 2 and row.get("FriendID"),
          json.dumps(row))

r = post("odekake_done", {"web_id": "1", "area_id": "1", "pcuid": bsess, "mid": bmid, "odekake_id": lmid})
m = re.search(r"<odekake><id>([^<]*)</id><success>(\d)</success>", r)
st = states_of(r)
check("odekake_done answers the shape OdekakeDone.cs:23-49 parses", m is not None, r[:180])
check("the servable borrow succeeds with the lender's two states",
      m is not None and m.group(2) == "1" and "player_game" in st and "customize_item" in st,
      f"success={m.group(2) if m else '-'} id={m.group(1) if m else '-'} states={sorted(st)}")
if "player_game" in st:
    borrowed = json.loads(st["player_game"])
    check("the borrowed player_game is the lender's document",
          borrowed.get("PlayerName") == "P8LENDERNAME" and borrowed.get("SelectChara") == 2,
          json.dumps({k: borrowed.get(k) for k in ("PlayerName", "SelectChara", "m_danLevel")}))

# A placeholder account is not a lender: an unclaimed guest account must be refused.
gsess, gmid = login("P8PLACEHOLDER1", "PLAYER")
r = post("odekake_done", {"web_id": "1", "area_id": "1", "pcuid": bsess, "mid": bmid, "odekake_id": gmid})
m = re.search(r"<odekake><id>([^<]*)</id><success>(\d)</success>", r)
check("a placeholder account is refused (code 0 + success 0, no error screen)",
      code_of(r) == "0" and m is not None and m.group(2) == "0", f"code={code_of(r)} body={r[-120:]}")

print("\n" + ("ALL ODEKAKE CHECKS PASSED" if not fails else f"{len(fails)} FAILED: {fails}"))
sys.exit(1 if fails else 0)
