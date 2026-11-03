"""Smoke test of the mfg endpoints the gacha fix touches: states, schedule, draws, music gacha.

usage: python .audit/smoke_mfg_endpoints.py
       SMOKE_REF=<account> python .audit/smoke_mfg_endpoints.py   # pin one fixture account
"""
import base64
import io
import json
import os
import re
import sys
import urllib.parse
import urllib.request

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from smoke_common import music_gacha_sweep, pick_fixture, report_build, report_liveness

sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding="utf-8", errors="replace")
BASE = "http://127.0.0.1:8083/aog/"
# Fixture rotation. Music possession is server-owned and permanent (recordMusicGain ->
# profile.states.music_gacha, handlers/aog.ts:860-879), so one account reserves each
# series once in the life of a database: a single fixed name was a single-run probe.
# The sweep uses the first fixture that still owns an unfinished series.
REFS = ([os.environ["SMOKE_REF"]] if os.environ.get("SMOKE_REF")
        else ["GACHAP4SMOKE"] + [f"GACHAP4SMOKE{i}" for i in range(2, 7)])
bad = []


def post(name, body):
    req = urllib.request.Request(BASE + name, data=body.encode(),
                                 headers={"Content-Type": "application/x-www-form-urlencoded"})
    with urllib.request.urlopen(req, timeout=30) as r:
        return r.read().decode("utf-8", "replace")


def code(xml):
    m = re.search(r"<serv_st><code>(\d+)</code>", xml)
    return m.group(1) if m else "?"


def check(name, xml, want="0"):
    got = code(xml)
    ok = got == want
    if not ok:
        bad.append(f"{name}: res_status {got}")
    print(f"  {name:26s} code={got}{' OK' if ok else ' << ' + xml[:120]}")
    return xml


# Name the build first. 8083 is the canonical tree's own port, so the sweep prints the tree
# it is claiming - and the caveat that the process behind the port transpiles the plugin at
# start-up, which is why a shipped-file claim needs a check that loads those files directly.
identity = report_build(BASE)
if not identity["ok"]:
    print("\nPROBLEMS: ['" + identity["detail"] + "']")
    sys.exit(2)

# Measure the server before probing it: a sweep may only claim a live core that it
# actually reached, with the pid the OS attributes to the listening socket.
live_start = report_liveness(BASE)
if not live_start["ok"]:
    print("\nPROBLEMS: ['nothing is listening on " + BASE + " - start the core (.audit/README.md)']")
    sys.exit(2)

# Fixture rotation, resolved against the server's own ledger: state kind music_gacha
# is authored by the server only (MusicGachaGameData.cs:15 has no ClientStateWrite
# call site anywhere in the client), so the ledger - not a remembered account name -
# decides which fixture can still be pulled.
picked = pick_fixture(post, REFS, bad)
if picked is None:
    print(f"\nPROBLEMS: {bad}")
    sys.exit(2)
fixture_ref, sess, mid, music_state = picked
print(f"session={sess} mid={mid} fixture={fixture_ref}")

print("state round trip")
data = urllib.parse.quote(base64.b64encode(json.dumps({"DataVersion": 14, "PlayerName": "SMOKE",
      "GachaCharaUsableList": [11, 12], "PurchaseCharaList": []}).encode()).decode(), safe="")
check("client_state_write", post("client_state_write",
      f"web_id=1&area_id=1&pcuid={sess}&mid={mid}&kind=player_game&data={data}"))
r = post("client_state_read", f"web_id=1&area_id=1&pcuid={sess}&mid={mid}")
kinds = re.findall(r'<state kind="([^"]+)"', r)
print(f"  client_state_read            {len(kinds)} kinds: {sorted(kinds)[:12]}")
pg = json.loads(base64.b64decode(re.search(r'<state kind="player_game"><data>([^<]*)</data>', r).group(1)).decode())
print("  player_game served:", {k: pg[k] for k in ("PlayerName", "GachaCharaUsableList") if k in pg})

print("schedule / draws")
gi = check("gacha_info", post("gacha_info", f"web_id=1&area_id=1&pcuid={sess}&api_ver=2"))
print("   banners:", len(re.findall(r"<info>", gi)))
check("chara_enabled", post("chara_enabled", f"web_id=1&area_id=1&pcuid={sess}&enabled_chara=12"))
for gid in (165, 166, 148, 101, 91, 1, 2):
    r = check(f"draw gacha_id={gid}", post(
        "cutin_gacha_play_draw",
        f"web_id=1&area_id=1&pcuid={sess}&gacha_id={gid}&play_count=5&is_ticket=0"
        f"&gift_config=Gift&enable_charas=0,1,2,3,4,5,6,7,8&debug_param=None"))
    items = re.findall(r"<oid>(OID_[^<]+)</oid>", r)
    kinds_ok = all(re.match(r"OID_[0-9]{2}[A-Za-z]+\d+$", o) for o in items)
    if not kinds_ok:
        bad.append(f"gacha_id={gid}: malformed oid in {items[:3]}")
    req = re.search(r"<request_id>(\d+)</request_id>", r)
    if req:
        post("cutin_gacha_play_apply", f"web_id=1&area_id=1&pcuid={sess}&request_id={req.group(1)}")
    print(f"     {len(items)} items, oids ok={kinds_ok}, sample={items[:3]}")

print("state lottery (legacy gacha endpoints)")
check("req_draw_gacha", post("req_draw_gacha", f"web_id=1&area_id=1&pcuid={sess}&gacha_id=1"))
check("get_gacha_result", post("get_gacha_result", f"web_id=1&area_id=1&pcuid={sess}"))

print("music gacha")
# The play used to be posted off the reserve's serv_st, which stays 0 on a refusal:
# the refusal is in the body (<is_success>0</is_success><request_id>0</request_id>,
# handlers/aog.ts:2450-2463), and cashing that request_id is correctly 930
# (handlers/aog.ts:2497). Both branches are asserted now.
music_gacha_sweep(post, check, bad, sess, mid, 91)

print("logs")
check("gacha_log", post("gacha_log", f"web_id=1&area_id=1&pcuid={sess}&log=" + urllib.parse.quote(
    base64.b64encode(b'{"probe":"smoke"}').decode())))

live_end = report_liveness(BASE)
if not live_end["ok"]:
    bad.append(f"server stopped answering during the sweep: {live_end['detail']}")
pid = live_start["pid"] if live_start["pid"] == live_end["pid"] else f"{live_start['pid']}->{live_end['pid']}"
print(f"server claim for this run: {BASE} pid={pid} build={identity['tree'][:12]} "
      f"({identity['kind']} {identity.get('lane_dir', '')}; both ends measured by this sweep)")

print("\n" + (f"PROBLEMS: {bad}" if bad else "SMOKE OK"))
