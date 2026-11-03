"""Pickup-gacha gate probe: does chara_enabled follow the advertised pickup pools?

usage: python .audit/probe_gacha_gate.py            -> banner ids by type + gate answer
"""
import io
import re
import sys
import urllib.request

sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding="utf-8", errors="replace")
BASE = "http://127.0.0.1:8083/aog/"


def post(name, body):
    req = urllib.request.Request(BASE + name, data=body.encode(),
                                 headers={"Content-Type": "application/x-www-form-urlencoded"})
    with urllib.request.urlopen(req, timeout=30) as r:
        return r.read().decode("utf-8", "replace")


info = post("gacha_info", "web_id=1&area_id=1&pcuid=x&api_ver=2")
by_type = {}
pooled = []
for m in re.finditer(r"<info>(.*?)</info>", info, re.S):
    b = m.group(1)
    bid = int(re.search(r"<id>(\d+)</id>", b).group(1))
    stype = re.search(r"<series_type>([^<]*)</series_type>", b).group(1)
    n_pick = len(re.findall(r"<item>", re.search(r"<pickup_items>(.*?)</pickup_items>", b, re.S).group(1)))
    n_lim = len(re.findall(r"<item>", re.search(r"<limited_pickup_items>(.*?)</limited_pickup_items>", b, re.S).group(1)))
    n_ch = len(re.findall(r"<chara>", re.search(r"<pickup_charas>(.*?)</pickup_charas>", b, re.S).group(1)))
    by_type.setdefault(stype, []).append(bid)
    if stype in ("Pickup", "Limited") and (n_pick or n_lim):
        pooled.append(bid)

print("banners by type:", {k: len(v) for k, v in sorted(by_type.items())})
print("pickup/limited banners with a real pool:", len(pooled))
for stype in ("Pickup", "Limited"):
    ids = by_type.get(stype, [])
    print(f"  {stype} ids: {ids}")
ce = post("chara_enabled", "web_id=1&area_id=1&pcuid=x&enabled_chara=12")
print("chara_enabled is_gacha_enabled:", re.search(r"<is_gacha_enabled>(\d+)</is_gacha_enabled>", ce).group(1))
