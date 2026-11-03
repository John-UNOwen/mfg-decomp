"""Same smoke as .audit/smoke_mfg_endpoints.py, but the base URL comes from argv
so it can run against a verification lane instead of the 8083 preview server.

The default is the lane scripts/ryunet_stage.ps1 stages (port 8093), not a port a
package happened to use once: the old default 8095 pointed at tmp/RyuNET-core/verify-p7,
a whole copy of dist that froze at 01:54 and was never re-staged, so a re-run here
measured a pre-P6/P7 build and printed SMOKE OK about it. A sweep now names the build it
measured and exits 2 when it cannot.

usage: python .audit/smoke_mfg_endpoints_p7.py [http://127.0.0.1:8093/aog/] [REF]
       (REF pins one fixture account; without it the sweep rotates, see below)
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
BASE = (sys.argv[1] if len(sys.argv) > 1 else "http://127.0.0.1:8093/aog/")
# Fixture rotation: music possession is server-owned and permanent (recordMusicGain ->
# profile.states.music_gacha, handlers/aog.ts:860-879), so a single fixed name is a
# single-run probe. Pass a REF to re-run one account on purpose.
REFS = ([sys.argv[2]] if len(sys.argv) > 2
        else ["GACHAP7SMOKE"] + [f"GACHAP7SMOKE{i}" for i in range(2, 7)])
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


# Name the build before touching it: the core transpiles the plugin at start-up, so a
# listening port is not proof of which build answered. Refuse first, then probe - a stale
# lane must not be written into by fixture logins on the way to a meaningless SMOKE OK.
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

picked = pick_fixture(post, REFS, bad)
if picked is None:
    print(f"\nPROBLEMS: {bad}")
    sys.exit(2)
fixture_ref, sess, mid, music_state = picked
print(f"base={BASE} session={sess} mid={mid} fixture={fixture_ref}")

print("state round trip")
data = urllib.parse.quote(base64.b64encode(json.dumps({"DataVersion": 14, "PlayerName": "SMOKE",
      "GachaCharaUsableList": [11, 12], "PurchaseCharaList": []}).encode()).decode(), safe="")
check("client_state_write", post("client_state_write",
      f"web_id=1&area_id=1&pcuid={sess}&mid={mid}&kind=player_game&data={data}"))
r = post("client_state_read", f"web_id=1&area_id=1&pcuid={sess}&mid={mid}")
kinds = re.findall(r'<state kind="([^"]+)"', r)
print(f"  client_state_read            {len(kinds)} kinds: {sorted(kinds)}")
pg = json.loads(base64.b64decode(re.search(r'<state kind="player_game"><data>([^<]*)</data>', r).group(1)).decode())
print("  player_game served:", {k: pg[k] for k in ("PlayerName", "GachaCharaUsableList") if k in pg})
fg = re.search(r'<state kind="friend_game"><data>([^<]*)</data>', r)
print("  friend_game served:", json.loads(base64.b64decode(fg.group(1)).decode()) if fg else "missing")

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

print("other endpoints")
for name, body in (
    ("mission_date", f"web_id=1&area_id=1&pcuid={sess}"),
    ("player_record", f"web_id=1&area_id=1&pcuid={sess}&mid={mid}"),
    ("get_haifu_list", f"web_id=1&area_id=1&pcuid={sess}&mid={mid}"),
    ("get_jongstone_info", f"web_id=1&area_id=1&pcuid={sess}&mid={mid}"),
    ("present_done", f"web_id=1&area_id=1&pcuid={sess}&present_id=9001"),
    ("competition_entry", f"web_id=1&area_id=1&pcuid={sess}&mid={mid}"),
    ("chk_tabooword", f"web_id=1&area_id=1&pcuid={sess}&word=" + urllib.parse.quote("test")),
    ("keep_alive", f"web_id=1&area_id=1&pcuid={sess}"),
    ("set_favorite_character", f"web_id=1&area_id=1&pcuid={sess}&object_id=OID_CHARACTER_1"),
    ("notice_done", f"web_id=1&area_id=1&pcuid={sess}&notice_ids="),
    ("eashop_done", f"web_id=1&area_id=1&pcuid={sess}&mid={mid}"),
    ("item_gain_log", f"web_id=1&area_id=1&pcuid={sess}"),
):
    check(name, post(name, body))

live_end = report_liveness(BASE)
if not live_end["ok"]:
    bad.append(f"server stopped answering during the sweep: {live_end['detail']}")
pid = live_start["pid"] if live_start["pid"] == live_end["pid"] else f"{live_start['pid']}->{live_end['pid']}"
print(f"server claim for this run: {BASE} pid={pid} build={identity['tree'][:12]} "
      f"({identity['kind']} {identity.get('lane_dir', '')}; both ends measured by this sweep)")

print("\n" + (f"PROBLEMS: {bad}" if bad else "SMOKE OK"))
