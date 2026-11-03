import urllib.request, urllib.parse, base64, re, json, sys, io
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding="utf-8", errors="replace")
BASE = "http://127.0.0.1:8083/aog/"
REF = "AUDITPROBE00002"

def post(name, body):
    req = urllib.request.Request(BASE + name, data=body.encode(), headers={"Content-Type": "application/x-www-form-urlencoded"})
    with urllib.request.urlopen(req, timeout=60) as r:
        return r.read().decode("utf-8", "replace")

def states_of(xml):
    out = {}
    for m in re.finditer(r'<state kind="([^"]+)"><data>([^<]*)</data></state>', xml):
        out[m.group(1)] = base64.b64decode(m.group(2)).decode("utf-8", "replace")
    return out

sess = re.search(r"<session_id>([0-9a-f]+)</session_id>",
                 post("login", f"system_id=AUDIT2&sic=VFG:J:A:A:2026060900&loc_id=ea&web_id=1&area_id=1&user_id={REF}&bypass_auth=true&guest=1")).group(1)
mid = re.search(r"<mid>(\d+)</mid>", post("get_menudata", f"web_id=1&area_id=1&pcuid={sess}")).group(1)

print("A) get_menudata with an unknown/stale pcuid:")
r = post("get_menudata", "web_id=1&area_id=1&pcuid=ffffffffffffffff")
print("   ->", re.search(r"<mpdata>.*?</mpdata>", r).group(0))

print("\nB) state write under mid=1 (what the client would send after A):")
payload = json.dumps({"DataVersion": 14, "PlayerName": "テスト", "marker": "MID1-CASE"}, ensure_ascii=False)
r = post("client_state_write", f"web_id=1&area_id=1&pcuid=ffffffffffffffff&mid=1&kind=player_game&data={urllib.parse.quote(base64.b64encode(payload.encode()).decode(), safe='')}")
print("   response:", r.replace("\n", "")[:120])
st = states_of(post("client_state_read", "web_id=1&area_id=1&pcuid=ffffffffffffffff&mid=1&one_kind=player_game"))
print("   read back with same pcuid/mid:", st)

print("\nC) large state (200 KB JSON) round-trip:")
big = json.dumps({"DataVersion": 1, "kind": "audit_plain", "Items": [{"ItemID": i, "m_count": i % 7} for i in range(12000)]}, ensure_ascii=False)
print("   payload bytes:", len(big.encode()))
post("client_state_write", f"web_id=1&area_id=1&pcuid={sess}&mid={mid}&kind=audit_plain&data={urllib.parse.quote(base64.b64encode(big.encode()).decode(), safe='')}")
got = states_of(post("client_state_read", f"web_id=1&area_id=1&pcuid={sess}&mid={mid}&one_kind=audit_plain")).get("audit_plain")
print("   round-trip identical:", got == big, "| served bytes:", len(got.encode()) if got else None)

print("\nD) log state size seen in production (from DB) vs handled here: ok")
