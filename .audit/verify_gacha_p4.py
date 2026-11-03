"""Acceptance check for the P4 gacha fix: advertised odds vs rolled odds, per-banner pools,
gift_config / enable_charas semantics, gift accounting, unlock grants.

usage: python .audit/verify_gacha_p4.py
"""
import base64
import collections
import io
import json
import math
import re
import statistics
import sys
import time
import urllib.parse
import urllib.request
from pathlib import Path

sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding="utf-8", errors="replace")

BASE = "http://127.0.0.1:8083/aog/"
ROOT = Path(__file__).resolve().parent.parent
MASTER = ROOT / "api/recovered/Assembly-CSharp/MFG.MasterGameData/CutinItemMaster.cs"
# unique per run: the gift/queue checks need accounts that have not been pulled
# on by an earlier run of this script
REF = "GACHAP4V" + time.strftime("%m%d%H%M%S")
FAILS = []


def fail(msg):
    FAILS.append(msg)
    print("   FAIL:", msg)


def post(name, body):
    req = urllib.request.Request(
        BASE + name, data=body.encode(),
        headers={"Content-Type": "application/x-www-form-urlencoded"},
    )
    with urllib.request.urlopen(req, timeout=30) as r:
        return r.read().decode("utf-8", "replace")


def master_facts():
    text = MASTER.read_text(encoding="utf-8", errors="replace")
    out = {}
    for m in re.finditer(
        r'ObjectID = "(OID_[^"]+)",\s*\n\s*ItemID = ItemID\.(\w+),.*?Rarity = Rarity\.(\w+),'
        r'\s*\n\s*CharaType = CharaType\.Chara(\d{2})', text, re.S):
        out[m.group(1)] = (int(m.group(4)) - 1, m.group(3))
    return out


FACTS = master_facts()
RAR_ORDER = ["N", "R", "SR", "UR"]


def bucket(rarity):
    # the server folds the master's "Nothing" rank into the advertised N bucket
    return rarity if rarity in RAR_ORDER else "N"


def states_of(xml):
    out = {}
    for m in re.finditer(r'<state kind="([^"]+)"><data>([^<]*)</data></state>', xml):
        out[m.group(1)] = base64.b64decode(m.group(2)).decode("utf-8", "replace")
    return out


def login(ref):
    sess = re.search(r"<session_id>([0-9a-f]+)</session_id>", post(
        "login", f"system_id=AUDIT2&sic=VFG:J:A:A:2026060900&loc_id=ea&web_id=1&area_id=1"
                 f"&user_id={ref}&bypass_auth=true&guest=1")).group(1)
    mid = re.search(r"<mid>(\d+)</mid>", post("get_menudata", f"web_id=1&area_id=1&pcuid={sess}")).group(1)
    return sess, mid


def read_state(sess, mid, kind):
    xml = post("client_state_read", f"web_id=1&area_id=1&pcuid={sess}&mid={mid}&one_kind={kind}")
    raw = states_of(xml).get(kind)
    return json.loads(raw) if raw else None


def parse_banners():
    info = post("gacha_info", f"web_id=1&area_id=1&pcuid=x&api_ver=2")
    banners = {}
    for m in re.finditer(r"<info>(.*?)</info>", info, re.S):
        b = m.group(1)

        def g(tag):
            r = re.search(rf"<{tag}>(.*?)</{tag}>", b, re.S)
            return r.group(1) if r else ""

        bid = int(g("id"))
        banners[bid] = {
            "id": bid, "label": g("label"), "type": g("series_type"),
            "items": re.findall(r"<item>(OID_[^<]+)</item>", g("items")),
            "pickup": re.findall(r"<item>(OID_[^<]+)</item>", g("pickup_items")),
            "limited": re.findall(r"<item>(OID_[^<]+)</item>", g("limited_pickup_items")),
            "custom": re.findall(r"<item>(OID_[^<]+)</item>", g("custom_pickup_items")),
            "charas": re.findall(r"<chara>([^<]+)</chara>", g("pickup_charas")),
            "rate": {grp: [int(x) for x in re.findall(
                rf"<{grp}>.*?<n>(\d+)</n><r>(\d+)</r><sr>(\d+)</sr><ur>(\d+)</ur>", b, re.S)[0]]
                for grp in ("normal", "pickup", "limited")},
        }
    return banners


def pull(sess, gid, count, enable, gift_cfg, times):
    rows = []
    gifts = 0
    for _ in range(times):
        r = post("cutin_gacha_play_draw",
                 f"web_id=1&area_id=1&pcuid={sess}&gacha_id={gid}&play_count={count}&is_ticket=0"
                 f"&gift_config={gift_cfg}&enable_charas={enable}&debug_param=None")
        if "<is_success>1</is_success>" not in r:
            fail(f"draw failed on {gid}: {r[:160]}")
            break
        for oid, gt in re.findall(r"<oid>(OID_[^<]+)</oid><gift_type>(\d+)</gift_type>", r):
            rows.append((oid, int(gt)))
        gifts += int(re.search(r"<gift>(\d+)</gift>", r).group(1))
        got = len(re.findall(r"<oid>", r))
        spirits = len(re.findall(r"<item>\d+</item>", re.search(r"<fight_spirits>(.*?)</fight_spirits>", r, re.S).group(1))) if "<fight_spirits>" in r else 0
        if got != count and spirits != got:
            fail(f"{gid}: {got} items but {spirits} spirits (items and spirits must stay paired)")
        req = re.search(r"<request_id>(\d+)</request_id>", r).group(1)
        post("cutin_gacha_play_apply", f"web_id=1&area_id=1&pcuid={sess}&request_id={req}")
    return rows, gifts


def main():
    sess, mid = login(REF)

    # ---- schedule structure -------------------------------------------------
    banners = parse_banners()
    print(f"scheduled banners: {len(banners)}")
    totals_ok = 0
    for b in banners.values():
        total = sum(sum(v) for v in b["rate"].values())
        if total != 1000:
            fail(f"banner {b['id']} {b['label']} advertises pickup_rate total {total}, expected 1000")
        else:
            totals_ok += 1
        for grp in ("pickup", "limited"):
            listed = set(b[grp])
            unknown = [o for o in listed if o not in b["items"]]
            if unknown:
                fail(f"banner {b['id']} {grp}_items lists {len(unknown)} item(s) not in <items>: {unknown[:3]}")
            has_items = bool(listed)
            weights = sum(b["rate"][grp])
            if has_items and weights == 0:
                fail(f"banner {b['id']} has {len(listed)} {grp}_items but advertises 0 rate for that group")
            if not has_items and weights != 0:
                fail(f"banner {b['id']} has no {grp}_items but advertises rate {weights}")
            for i, oid in enumerate(listed):
                w = b["rate"][grp][RAR_ORDER.index(bucket(FACTS.get(oid, (-1, "N"))[1]))]
                if w == 0:
                    fail(f"banner {b['id']} advertises {oid} in {grp} at 0% ({FACTS.get(oid)})")
        if b["type"] == "Limited" and not b["limited"]:
            fail(f"Limited banner {b['id']} {b['label']} advertises no limited_pickup_items")
        if b["type"] == "Pickup" and not b["pickup"]:
            fail(f"Pickup banner {b['id']} {b['label']} advertises no pickup_items")
    print(f"   rate totals == 1000: {totals_ok}/{len(banners)}")

    pick = [b for b in banners.values() if b["type"] == "Pickup"]
    lim = [b for b in banners.values() if b["type"] == "Limited"]
    print(f"   Pickup banners {len(pick)} filled {sum(1 for b in pick if b['pickup'])}"
          f" | Limited banners {len(lim)} filled {sum(1 for b in lim if b['limited'])}")
    sigs = collections.Counter(json.dumps(b["rate"]) for b in banners.values())
    print(f"   distinct rate signatures: {len(sigs)}")
    for b in (165, 166, 148):
        bb = banners.get(b)
        if not bb:
            fail(f"banner {b} not advertised")
            continue
        print(f"   id={b} {bb['label']} type={bb['type']} items={len(bb['items'])} "
              f"pickup={len(bb['pickup'])} limited={len(bb['limited'])} charas={bb['charas']} rate={bb['rate']}")

    # ---- advertised odds == rolled odds, per banner ---------------------------
    enable_low = ",".join(str(i) for i in range(9))
    print("\nrealized vs advertised (100 x 5 = 500 items each)")
    for gid in (165, 166, 148, 101, 0):
        b = banners.get(gid)
        if not b or b["type"] == "Music":
            continue
        rows, _ = pull(sess, gid, 5, enable_low, "Gift", 100)
        n = len(rows)
        if not n:
            continue
        rar_hist = collections.Counter(bucket(FACTS[o][1]) for o, _ in rows)
        pickset = set(b["pickup"])
        limset = set(b["limited"])
        grp_hist = collections.Counter("limited" if o in limset else "pickup" if o in pickset else "normal"
                                       for o, _ in rows)
        exp_rar = {r: sum(b["rate"][g][i] for g in ("normal", "pickup", "limited")) / 10.0
                   for i, r in enumerate(RAR_ORDER)}
        exp_grp = {g: sum(b["rate"][g]) / 10.0 for g in ("normal", "pickup", "limited")}
        print(f"  id={gid} {b['label']} [{b['type']}] {n} items")
        worst = 0.0
        for r in RAR_ORDER:
            got = 100.0 * rar_hist.get(r, 0) / n
            exp = exp_rar[r]
            sd = math.sqrt(max(exp, 1e-9) * (100 - exp) / n)
            z = abs(got - exp) / sd if sd else 0.0
            worst = max(worst, z)
            print(f"     {r:2s} advertised {exp:5.1f}%  realized {got:5.1f}%  ({z:.1f} sd)")
        for g in ("normal", "pickup", "limited"):
            got = 100.0 * grp_hist.get(g, 0) / n
            exp = exp_grp[g]
            sd = math.sqrt(max(exp, 1e-9) * (100 - exp) / n)
            z = abs(got - exp) / sd if sd else 0.0
            worst = max(worst, z)
            print(f"     {g:7s} group advertised {exp:5.1f}%  realized {got:5.1f}%  ({z:.1f} sd)")
        if worst > 4.0:
            fail(f"banner {gid} realized odds deviate {worst:.1f} sd from the advertised table")
        else:
            print(f"     max deviation {worst:.1f} sd -> matches the advertised table")

    # ---- gift_config / enable_charas semantics on a fresh account -----------
    print("\ngift_config x enable_charas (fresh account, Chara01-09 selectable)")
    gsess, gmid = login(REF + "G")
    selectable = set(range(9))
    b = banners[165]
    owned_before = read_state(gsess, gmid, "player_item")
    for cfg in ("Gift", "Get", "None"):
        rows, gifts = pull(gsess, 165, 5, enable_low, cfg, 40)
        gt = collections.Counter(t for _, t in rows)
        bad2 = [o for o, t in rows if t == 2 and (FACTS[o][0] in selectable or b["type"] not in ("Pickup", "Limited"))]
        bad0 = [o for o, t in rows if t == 0 and FACTS[o][0] not in selectable]
        unselectable_new = [o for o, t in rows if t == 0 and FACTS[o][0] not in selectable]
        print(f"  gift_config={cfg:5s}: items={len(rows)} gift_type={dict(sorted(gt.items()))} gift_points={gifts}")
        if bad2:
            fail(f"{cfg}: gift_type 2 used for a selectable chara or a non-boosted banner: {bad2[:3]}")
        if cfg == "Get" and gt.get(2):
            fail("Get state still produced ConfigGift items")
        if cfg == "Gift" and not gt.get(2):
            fail("Gift state produced no ConfigGift on unselectable charas")
        if cfg == "None":
            # every item the client would try to grant for an unselectable girl
            # must be absent from gain_items (GachaResultInfo.cs:130 cutinItems[-1])
            if bad0:
                fail(f"None state still hands out unselectable-chara items: {bad0[:3]}")
            missing_from_rows = gifts - (gt.get(1, 0) + gt.get(2, 0))
            print(f"     gift points not explained by gift_type 1/2 (dropped items): {missing_from_rows}")
            if missing_from_rows <= 0:
                fail("None state credited no gift points for the items it dropped")

    # gift accounting: the server's own Present01 copy moved by exactly the sum
    before = read_state(gsess, gmid, "player_item")

    def present01(state):
        if not state:
            return 0
        for x in state.get("Items", []):
            if x.get("ItemID") == 1:
                return int(x.get("Count", 0))
        return 0

    c0 = present01(before)
    acc = 0
    for cfg in ("Gift", "Get"):
        _, gifts = pull(gsess, 165, 5, enable_low, cfg, 10)
        acc += gifts
    c1 = present01(read_state(gsess, gmid, "player_item"))
    print(f"  server Present01 {c0} -> {c1} after crediting {acc} gift points")
    if c1 - c0 != acc:
        fail(f"server gift credit mismatch: delta {c1 - c0} != {acc}")

    # ---- unlock grant: drawing an unlock cutin unlocks her, pruning never ----
    def write_state(sess, mid, kind, obj):
        data = urllib.parse.quote(base64.b64encode(json.dumps(obj, ensure_ascii=False).encode()).decode(), safe="")
        post("client_state_write", f"web_id=1&area_id=1&pcuid={sess}&mid={mid}&kind={kind}&data={data}")

    print("\nunlock grants")
    unlock_oids = set()

    def unlock_pulls(sess, times, tag):
        found = set()
        for _ in range(times):
            rows, _ = pull(sess, 166, 5, ",".join(str(i) for i in range(9)), "Get", 1)
            for oid, _t in rows:
                if re.match(r"OID_(1[2-9]|20)AgariSR01$", oid):
                    found.add(oid)
        unlock_oids.update(found)
        return found

    # normal client order: the account has a player_game state on the server
    sessA, midA = login(REF + "A")
    write_state(sessA, midA, "player_game", {"DataVersion": 14, "PlayerName": "VA",
                                             "GachaCharaUsableList": [], "PurchaseCharaList": []})
    foundA = unlock_pulls(sessA, 40, "A")
    afterA = sorted((read_state(sessA, midA, "player_game") or {}).get("GachaCharaUsableList") or [])
    wantA = {int(re.match(r"OID_(\d{2})", o).group(1)) - 1 for o in foundA}
    print(f"   account with player_game state: drew {sorted(foundA)}")
    print(f"   GachaCharaUsableList after draws: {afterA}")
    if wantA - set(afterA):
        fail(f"draws granted no unlock: missing {sorted(wantA - set(afterA))}")

    # fallback order: unlock drawn before any player_game copy exists
    sessB, midB = login(REF + "B")
    print("   account without player_game state:", read_state(sessB, midB, "player_game"))
    foundB = unlock_pulls(sessB, 40, "B")
    afterB0 = sorted((read_state(sessB, midB, "player_game") or {}).get("GachaCharaUsableList") or [])
    write_state(sessB, midB, "player_game", {"DataVersion": 14, "PlayerName": "VB",
                                             "GachaCharaUsableList": [], "PurchaseCharaList": []})
    afterB = sorted((read_state(sessB, midB, "player_game") or {}).get("GachaCharaUsableList") or [])
    wantB = {int(re.match(r"OID_(\d{2})", o).group(1)) - 1 for o in foundB}
    print(f"   drew {sorted(foundB)}; first player_game write served {afterB} (was {afterB0})")
    if wantB - set(afterB):
        fail(f"queued unlocks lost on the first player_game write: missing {sorted(wantB - set(afterB))}")

    # the state the client writes back is kept, not pruned
    write_state(sessA, midA, "player_game", {"DataVersion": 14, "PlayerName": "VA",
                                             "GachaCharaUsableList": [11, 12, 13, 14, 15, 16, 17, 18, 19, 20],
                                             "PurchaseCharaList": []})
    served = read_state(sessA, midA, "player_game")
    if sorted(served.get("GachaCharaUsableList") or []) != list(range(11, 21)):
        fail(f"read-time pruning still active: wrote [11..20], served {served.get('GachaCharaUsableList')}")
    else:
        print("   wrote [11..20] owning a subset of unlock cutins -> served [11..20] (no pruning)")

    # ---- pickup gate ----------------------------------------------------------
    ce = post("chara_enabled", f"web_id=1&area_id=1&pcuid={sess}&enabled_chara=12")
    gate = re.search(r"<is_gacha_enabled>(\d+)</is_gacha_enabled>", ce).group(1)
    has_pickup_banners = any(bb["type"] in ("Pickup", "Limited") and (bb["pickup"] or bb["limited"])
                             for bb in banners.values())
    print(f"\npickup gate: chara_enabled is_gacha_enabled={gate} with {sum(1 for bb in banners.values() if bb['type'] in ('Pickup','Limited'))} pickup/limited banners advertised")
    if gate != ("1" if has_pickup_banners else "0"):
        fail(f"gate answer {gate} does not follow the advertised pickup pools")

    print("\n" + ("PROBLEMS: " + str(len(FAILS)) if FAILS else "ALL CHECKS PASSED"))
    for f in FAILS:
        print("  -", f)


if __name__ == "__main__":
    main()
