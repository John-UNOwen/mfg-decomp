import urllib.request, urllib.parse, base64, re, json, sys, io
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding="utf-8", errors="replace")
BASE = "http://127.0.0.1:8083/aog/"
REF = "AUDITPROBE00002"

def post(name, body):
    req = urllib.request.Request(BASE + name, data=body.encode(), headers={"Content-Type": "application/x-www-form-urlencoded"})
    with urllib.request.urlopen(req, timeout=25) as r:
        return r.read().decode("utf-8", "replace")

def states_of(xml):
    out = {}
    for m in re.finditer(r'<state kind="([^"]+)"><data>([^<]*)</data></state>', xml):
        out[m.group(1)] = base64.b64decode(m.group(2)).decode("utf-8", "replace")
    return out

r = post("login", f"system_id=AUDIT2&sic=VFG:J:A:A:2026060900&loc_id=ea&web_id=1&area_id=1&user_id={REF}&bypass_auth=true&guest=1")
sess = re.search(r"<session_id>([0-9a-f]+)</session_id>", r).group(1)
mid = re.search(r"<mid>(\d+)</mid>", post("get_menudata", f"web_id=1&area_id=1&pcuid={sess}")).group(1)

def pb():
    st = states_of(post("client_state_read", f"web_id=1&area_id=1&pcuid={sess}&mid={mid}&one_kind=present_box_game"))
    return json.loads(st["present_box_game"])["PresentInfos"]

lst = pb()
print("presents served (1st read):")
for p in lst:
    print(f"   PresentID={p['PresentID']} auto={p['IsAutoApply']} received={p['IsReceived']} content={p['Content']} count={p['Count']}")

ids = ",".join(str(p["PresentID"]) for p in lst if not p["IsReceived"] and p["IsAutoApply"])
print("\nclient would auto-apply these ids:", ids)
resp = post("present_done", f"web_id=1&area_id=1&pcuid={sess}&done_ids={ids}")
print("present_done response:", resp.replace("\n", "")[:400])

lst2 = pb()
print("\npresents served (2nd read, after present_done):")
for p in lst2:
    print(f"   PresentID={p['PresentID']} auto={p['IsAutoApply']} received={p['IsReceived']}")
same = [p for p in lst2 if not p["IsReceived"] and p["IsAutoApply"]]
print(f"\nstill auto-applicable after present_done: {len(same)} of {len(lst2)}")
lst3 = pb()
print("3rd read identical to 2nd:", json.dumps(lst3) == json.dumps(lst2))
