import urllib.request, urllib.parse, base64, re, json, sys, io

sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding="utf-8", errors="replace")
BASE = "http://127.0.0.1:8083/aog/"
REF = (sys.argv[1] if len(sys.argv) > 1 else "P8NOTICE0001")[:16]


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
        kind = m.group(1).replace("&quot;", '"').replace("&amp;", "&")
        try:
            out[kind] = base64.b64decode(m.group(2)).decode("utf-8", "replace")
        except Exception as e:
            out[kind] = f"<decode error: {e}>"
    return out


r = post("login", {"system_id": "P8", "sic": "VFG:J:A:A:2026060900", "loc_id": "ea", "web_id": "1",
                    "area_id": "1", "user_id": REF, "bypass_auth": "true", "guest": "1"})
sess = re.search(r"<session_id>([0-9a-f]+)</session_id>", r).group(1)
post("create_player", {"web_id": "1", "area_id": "1", "pcuid": sess, "user_id": REF, "name": "NOTICEPROBE"})
r = post("get_menudata", {"web_id": "1", "area_id": "1", "pcuid": sess})
mid = re.search(r"<mid>(\d+)</mid>", r).group(1)
print(f"REF={REF} session={sess[:12]} mid={mid}")

r = post("client_state_read", {"web_id": "1", "area_id": "1", "pcuid": sess, "mid": mid})
st = states_of(r)
print("serv_st =", code_of(r))
print("state kinds served:", sorted(st))
for kind in ("notice_game", "receive_only"):
    doc = json.loads(st.get(kind, "{}"))
    if kind == "notice_game":
        ids = [n.get("NoticeID") for n in doc.get("NoticeInfos", [])]
        print(f"{kind}: keys={sorted(doc)} NoticeInfos ids={ids}")
    else:
        print(f"{kind}: keys={sorted(doc)} ImportantNotices={[n.get('NoticeID') for n in doc.get('ImportantNotices', [])]}")
print("NO acks were posted by this probe.")
