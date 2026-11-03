import urllib.request, base64, json, sys, io
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding="utf-8", errors="replace")
BASE = "http://127.0.0.1:8083/aog/"

def post(name, body):
    req = urllib.request.Request(BASE + name, data=body.encode(), headers={"Content-Type": "application/x-www-form-urlencoded"})
    with urllib.request.urlopen(req, timeout=25) as r:
        return r.read().decode("utf-8", "replace")

msg = "AUDITGACHALOG " + json.dumps({"state": "Get", "note": "alpha+bravo+charlie delta/echo"}, ensure_ascii=False)
b = base64.b64encode(msg.encode()).decode()
print("intended text :", msg)
print("base64        :", b, "| contains '+':", "+" in b)
post("gacha_log", f"web_id=1&area_id=1&pcuid=0123456789abcdef&log={b}")   # literal '+' in body (what URLSearchParams turns into a space)
post("item_gain_log", f"web_id=1&area_id=1&pcuid=0123456789abcdef&log={b}")
print("sent unescaped-base64 log entries")
