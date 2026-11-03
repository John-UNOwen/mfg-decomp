"""Live probe of the P4 gacha findings against the running RyuNET mfg plugin.

usage: python .audit/probe_gacha_p4.py [before|after]
"""
import base64
import collections
import json
import re
import sys
import io
import urllib.parse
import urllib.request
from pathlib import Path

sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding="utf-8", errors="replace")

BASE = "http://127.0.0.1:8083/aog/"
ROOT = Path(__file__).resolve().parent.parent
MASTER = ROOT / "api/recovered/Assembly-CSharp/MFG.MasterGameData/CutinItemMaster.cs"
REF = "GACHAP4PROBE"
MODE = (sys.argv[1] if len(sys.argv) > 1 else "before").lower()


def post(name, body):
    req = urllib.request.Request(
        BASE + name,
        data=body.encode(),
        headers={"Content-Type": "application/x-www-form-urlencoded"},
    )
    with urllib.request.urlopen(req, timeout=30) as r:
        return r.read().decode("utf-8", "replace")


def master_facts():
    """ObjectID -> (chara_index, rarity) straight from the client master."""
    text = MASTER.read_text(encoding="utf-8", errors="replace")
    out = {}
    for m in re.finditer(
        r'ObjectID = "(OID_[^"]+)",\s*\n\s*ItemID = ItemID\.(\w+),.*?Rarity = Rarity\.(\w+),\s*\n\s*CharaType = CharaType\.Chara(\d{2})',
        text,
        re.S,
    ):
        oid, _item, rarity, ch = m.group(1), m.group(2), m.group(3), int(m.group(4))
        out[oid] = (ch - 1, rarity)
    return out


FACTS = master_facts()


def states_of(xml):
    out = {}
    for m in re.finditer(r'<state kind="([^"]+)"><data>([^<]*)</data></state>', xml):
        out[m.group(1)] = base64.b64decode(m.group(2)).decode("utf-8", "replace")
    return out


def w(kind, obj, sess, mid):
    data = urllib.parse.quote(
        base64.b64encode(json.dumps(obj, ensure_ascii=False).encode()).decode(), safe=""
    )
    return post(
        "client_state_write",
        f"web_id=1&area_id=1&pcuid={sess}&mid={mid}&kind={kind}&data={data}",
    )


def main():
    sess = re.search(
        r"<session_id>([0-9a-f]+)</session_id>",
        post(
            "login",
            f"system_id=AUDIT2&sic=VFG:J:A:A:2026060900&loc_id=ea&web_id=1&area_id=1&user_id={REF}&bypass_auth=true&guest=1",
        ),
    ).group(1)
    mid = re.search(
        r"<mid>(\d+)</mid>", post("get_menudata", f"web_id=1&area_id=1&pcuid={sess}")
    ).group(1)

    # ---- finding (a): read-time pruning ------------------------------
    # account owns only the Chara12 unlock cutin (ItemID 176) but its saved
    # GachaCharaUsableList says Chara12,13,14 (11,12,13 as CharaType ints).
    w("customize_item", {"DataVersion": 1, "CustomizeItems": [],
                         "CutinItems": [{"ItemID": 176}]}, sess, mid)
    w("player_game", {"DataVersion": 14, "PlayerName": "P4PROBE",
                      "GachaCharaUsableList": [11, 12, 13], "PurchaseCharaList": []},
      sess, mid)
    served = json.loads(
        states_of(post("client_state_read",
                       f"web_id=1&area_id=1&pcuid={sess}&mid={mid}&one_kind=player_game"))["player_game"]
    )
    print("A) wrote GachaCharaUsableList [11,12,13], owns only unlock cutin 176")
    print("   served back:", served.get("GachaCharaUsableList"))

    # ---- findings (b)(c)(d): advertised banner surface -----------------
    info = post("gacha_info", f"web_id=1&area_id=1&pcuid={sess}&api_ver=2")
    banners = []
    for m in re.finditer(r"<info>(.*?)</info>", info, re.S):
        b = m.group(1)
        g = lambda tag: (re.search(rf"<{tag}>(.*?)</{tag}>", b, re.S) or [None, ""])[1]
        banners.append({
            "id": int(g("id")),
            "label": g("label"),
            "type": g("series_type"),
            "items": re.findall(r"<items>(.*?)</items>", b, re.S)[0].count("<item>") if "<items>" in b else 0,
            "pickup_items": re.findall(r"<pickup_items>(.*?)</pickup_items>", b, re.S)[0].count("<item>") if "<pickup_items>" in b else 0,
            "limited_items": re.findall(r"<limited_pickup_items>(.*?)</limited_pickup_items>", b, re.S)[0].count("<item>") if "<limited_pickup_items>" in b else 0,
            "charas": re.findall(r"<chara>([^<]+)</chara>", g("pickup_charas")),
            "rate": re.findall(r"<n>(\d+)</n><r>(\d+)</r><sr>(\d+)</sr><ur>(\d+)</ur>", b),
        })
    print(f"\nB) advertised banners: {len(banners)}")
    for bid in (165, 166, 148, 2):
        b = next((x for x in banners if x["id"] == bid), None)
        if not b:
            print(f"   banner {bid}: NOT ADVERTISED")
            continue
        print(f"   id={b['id']} {b['label']} type={b['type']} items={b['items']} "
              f"pickup={b['pickup_items']} limited={b['limited_items']} charas={b['charas']}")
    rate_sigs = collections.Counter(json.dumps(b["rate"]) for b in banners)
    print(f"   distinct advertised pickup_rate signatures: {len(rate_sigs)}")
    for sig, n in rate_sigs.most_common(4):
        print(f"      x{n}: {sig}")
    lim = [b for b in banners if b["type"] == "Limited"]
    print(f"   Limited banners: {len(lim)}; limited_pickup_items filled: {sum(1 for b in lim if b['limited_items'])}")
    pick = [b for b in banners if b["type"] == "Pickup"]
    print(f"   Pickup banners: {len(pick)}; pickup_items filled: {sum(1 for b in pick if b['pickup_items'])}")

    # ---- finding (c): realized odds vs advertised ----------------------
    def pull(gid, count, enable, gift_cfg, times):
        hist_r = collections.Counter()
        hist_c = collections.Counter()
        gift_types = collections.Counter()
        gifts = 0
        oid_sample = []
        for _ in range(times):
            r = post(
                "cutin_gacha_play_draw",
                f"web_id=1&area_id=1&pcuid={sess}&gacha_id={gid}&play_count={count}&is_ticket=0"
                f"&gift_config={gift_cfg}&enable_charas={enable}&debug_param=None",
            )
            if "<is_success>1</is_success>" not in r:
                print("   draw failed:", r[:200])
                break
            for oid, gt in re.findall(r"<oid>(OID_[^<]+)</oid><gift_type>(\d+)</gift_type>", r):
                ch, rar = FACTS.get(oid, (-1, "?"))
                hist_r[rar] += 1
                hist_c[f"Chara{ch + 1:02d}"] += 1
                gift_types[gt] += 1
                if len(oid_sample) < 6:
                    oid_sample.append((oid, gt))
            gifts += int(re.search(r"<gift>(\d+)</gift>", r).group(1))
            req = re.search(r"<request_id>(\d+)</request_id>", r).group(1)
            post("cutin_gacha_play_apply", f"web_id=1&area_id=1&pcuid={sess}&request_id={req}")
        return hist_r, hist_c, gift_types, gifts, oid_sample

    enable_low = ",".join(str(i) for i in range(9))  # Chara01..Chara09 selectable
    for gid, note in ((165, "PickupMaidJun / featured Chara20"), (166, "LimitedCharaReturns3")):
        hr, hc, gt, gifts, sample = pull(gid, 5, enable_low, "Gift", 100)
        total = sum(hr.values())
        print(f"\nC) gacha_id={gid} ({note}): {total} items rolled")
        print("   rarity histogram:", {k: f"{100.0 * v / total:.1f}%" for k, v in hr.most_common()})
        print("   top charas:", dict(hc.most_common(6)))
        print("   gift_type histogram:", dict(gt), "gift points:", gifts)
        print("   sample:", sample)

    # ---- finding (a) grant path: does drawing an unlock cutin unlock? --
    hr, hc, gt, gifts, sample = pull(148, 5, enable_low, "Gift", 20)  # UnlockJun: pickup item OID_20AgariSR01
    drawn_unlock = [o for o in sample if o[0] == "OID_20AgariSR01"]
    r = post("cutin_gacha_play_draw",
             f"web_id=1&area_id=1&pcuid={sess}&gacha_id=148&play_count=5&is_ticket=0&gift_config=Get&enable_charas={enable_low}&debug_param=None")
    oids = re.findall(r"<oid>(OID_[^<]+)</oid>", r)
    served = json.loads(
        states_of(post("client_state_read",
                       f"web_id=1&area_id=1&pcuid={sess}&mid={mid}&one_kind=player_game"))["player_game"]
    )
    print(f"\nD) UnlockJun draws: {len(hr)} items, OID_20AgariSR01 seen in sample: {bool(drawn_unlock)}")
    print("   last draw oids:", oids)
    print("   server GachaCharaUsableList after draws:", served.get("GachaCharaUsableList"))

    # ---- finding (e): pickup gate -------------------------------------
    ce = post("chara_enabled", f"web_id=1&area_id=1&pcuid={sess}&enabled_chara=12")
    print("\nE) chara_enabled:", re.search(r"<is_gacha_enabled>(\d+)</is_gacha_enabled>", ce).group(1))


if __name__ == "__main__":
    print(f"mode={MODE}  master facts loaded: {len(FACTS)}")
    main()
