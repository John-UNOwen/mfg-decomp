"""Scramble-festival record path probe (VFG_SCRAMBLE_SCHOOL must be set).

Checks the whole chain the client reads:
entry_game teammatch>0 -> mend/g2event (MatchingData.cs:87-104) -> played match ->
mgresult/g2result (MahjongResultInfo.cs:168-181) -> match_archive scramble rows ->
player_record total/info g2_scramble (PlayerTotalRecordInfo.cs:15-25).

usage: python .audit/probe_scramble.py
"""
import io
import json
import re
import sqlite3
import sys
import time
import urllib.request

sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding="utf-8", errors="replace")

BASE = "http://127.0.0.1:8083/aog/"
DB = r"C:\mfg\tmp\RyuNET-core\dist\savedata\mfg@asphyxia.db"
APPVER = "VFG-2026060900"
SCHOOL = "School003"
S_SUTE_PAI, S_NAKINASHI, S_NEXT_KYOKU_READY = 2, 11, 15
K_TSUMO, K_TSUMOCHOICES, K_SUTECHOICES, K_KYOKUEND = 1, 15, 16, 23
CELL_RE = re.compile(r'<cell_data_(\d+) kind="(\d+)"([^>]*)>(.*?)</cell_data_\1>', re.S)
results = []


def check(name, ok, detail=""):
    results.append((name, bool(ok)))
    print(f"  {'PASS' if ok else 'FAIL':4s}  {name}" + (f"  {detail}" if detail else ""))


def post(name, body):
    req = urllib.request.Request(BASE + name, data=body.encode(),
                                 headers={"Content-Type": "application/x-www-form-urlencoded"})
    with urllib.request.urlopen(req, timeout=60) as r:
        return r.read().decode("utf-8", "replace")


def code(xml):
    m = re.search(r"<serv_st><code>(\d+)</code>", xml)
    return int(m.group(1)) if m else -1


def cells(xml):
    out = []
    for m in CELL_RE.finditer(xml):
        pis = {int(i) for i in re.findall(r'pi(\d)="1"', m.group(3))}
        out.append((int(m.group(1)), int(m.group(2)), pis, m.group(4)))
    return out


def off_mode():
    """With VFG_SCRAMBLE_SCHOOL unset, a teammatch entry must stay an ordinary match."""
    x = post("login", "system_id=AUDIT2&sic=VFG:J:A:A:2026060900&loc_id=ea&web_id=1&area_id=1"
                      "&user_id=P6SCRAMBLEOFF&bypass_auth=true&guest=1")
    s = re.search(r"<session_id>([0-9a-f]+)</session_id>", x).group(1)
    x = post("entry_game", f"pcuid={s}&area_id=1&web_id=1&gmode=4&taku_class=1&match_mode=1"
                           f"&passphrase=%20&teammatch=1&consume_oids=&entry_id=-1&pro_stay=0"
                           f"&competition_id=-1&shop_competition_id=-1&debug_score=-1")
    tid = int(re.search(r"<tid>(\d+)</tid>", x).group(1))
    pindex = int(re.search(r"<pindex>(\d+)</pindex>", x).group(1))
    g = post("gget", f"web_id=1&area_id=1&pcuid={s}&gserv_id=1&ready=1&must={APPVER}/{s}/{tid}/{pindex}/0/0")
    check("teammatch entry without a configured school is an ordinary match",
          code(g) == 0 and "<g2event>" not in g, f"code={code(g)} g2event={'yes' if '<g2event>' in g else 'no'}")
    e = post("end_game", f"web_id=1&area_id=1&pcuid={s}&tid={tid}&pindex={pindex}&mid=0&version={APPVER}")
    check("no scramble is recorded for it", "<g2result>" not in e, f"code={code(e)}")
    rec = post("player_record", f"web_id=1&area_id=1&pcuid={s}")
    check("records stay empty of scramble history", f"<belong>{SCHOOL}</belong>" not in rec,
          f"belong seen={'yes' if f'<belong>{SCHOOL}</belong>' in rec else 'no'}")


def main():
    if (sys.argv[1] if len(sys.argv) > 1 else "") == "off":
        print("[off] VFG_SCRAMBLE_SCHOOL unset")
        off_mode()
        bad = [n for n, ok in results if not ok]
        print(f"\n{'PROBLEMS: ' + ', '.join(bad) if bad else 'ALL CHECKS PASS'} ({len(results)} checks)")
        return 1 if bad else 0

    s = login = ""
    x = post("login", "system_id=AUDIT2&sic=VFG:J:A:A:2026060900&loc_id=ea&web_id=1&area_id=1"
                      "&user_id=P6SCRAMBLE&bypass_auth=true&guest=1")
    s = re.search(r"<session_id>([0-9a-f]+)</session_id>", x).group(1)
    x = post("entry_game", f"pcuid={s}&area_id=1&web_id=1&gmode=4&taku_class=1&match_mode=1"
                           f"&passphrase=%20&teammatch=1&consume_oids=&entry_id=-1&pro_stay=0"
                           f"&competition_id=-1&shop_competition_id=-1&debug_score=-1")
    tid = int(re.search(r"<tid>(\d+)</tid>", x).group(1))
    pindex = int(re.search(r"<pindex>(\d+)</pindex>", x).group(1))
    print(f"entry tid={tid} pindex={pindex} teammatch=1")

    cursor, drawn, kyoku, game_over = 0, None, 0, False
    g2event_seen = False
    polls = 0
    while polls < 3000 and not game_over:
        polls += 1
        g = post("gget", f"web_id=1&area_id=1&pcuid={s}&gserv_id=1&ready=1"
                         f"&must={APPVER}/{s}/{tid}/{pindex}/0/{cursor}")
        if "<g2event>" in g:
            g2event_seen = True
        m = re.search(r'<cell_sno start="(\d+)" count="(\d+)"', g)
        start, count = (int(m.group(1)), int(m.group(2))) if m else (0, 0)
        for seq, kind, pis, inner in cells(g):
            if kind == K_TSUMO and pindex in pis:
                mm = re.search(r"<pai>(\d+)</pai>", inner)
                drawn = int(mm.group(1)) if mm else None
            elif kind == K_TSUMOCHOICES and pindex in pis:
                post("gpost", f"web_id=1&area_id=1&pcuid={s}&gserv_id=1&tid={tid}&mid=0&pindex={pindex}"
                              f"&must={APPVER}/{s}/{tid}/{pindex}/0/{seq}/{S_SUTE_PAI}/0/0/{drawn or 0}/0/0/0/1/0&app_param=0/0")
            elif kind == K_SUTECHOICES and pindex in pis:
                post("gpost", f"web_id=1&area_id=1&pcuid={s}&gserv_id=1&tid={tid}&mid=0&pindex={pindex}"
                              f"&must={APPVER}/{s}/{tid}/{pindex}/0/{seq}/{S_NAKINASHI}/0/0/0/0/0/0/0/0&app_param=0/0")
            elif kind == K_KYOKUEND:
                kyoku += 1
                if re.search(r"<end_stat>1</end_stat>", inner):
                    game_over = True
                else:
                    post("gpost", f"web_id=1&area_id=1&pcuid={s}&gserv_id=1&tid={tid}&mid=0&pindex={pindex}"
                                  f"&must={APPVER}/{s}/{tid}/{pindex}/0/{seq}/{S_NEXT_KYOKU_READY}/0/0/0/0/0/0/0/0&app_param=0/0")
        cursor = max(cursor, start + count)
        if count == 0 and not game_over:
            time.sleep(0.4)

    check("mwait announces the scramble match (mend>g2event>e>id=1)", g2event_seen,
          f"g2event={'yes' if g2event_seen else 'no'} polls={polls} kyoku={kyoku}")

    e = post("end_game", f"web_id=1&area_id=1&pcuid={s}&tid={tid}&pindex={pindex}&mid=0&version={APPVER}")
    mres = re.search(r"<g2result><r><id>1</id><sub_cate>(\w+)</sub_cate><win>(\d+)</win></r></g2result>", e)
    check("mgresult carries g2result for this player's own result", bool(mres),
          f"g2result={mres.groups() if mres else '-'} code={code(e)}")

    time.sleep(1.5)
    con = sqlite3.connect(f"file:{DB}?mode=ro", uri=True)
    docs = [json.loads(r[0]) for r in con.execute("SELECT data FROM documents WHERE data LIKE '%match_archive%'")]
    con.close()
    arts = [d for d in docs if d.get("tid") == tid]
    sc = [(p.get("name"), p.get("scramble")) for d in arts for p in d.get("players", [])]
    check("archive records the scramble result per participant",
          any(isinstance(s2, dict) and s2.get("belong") == SCHOOL for _n, s2 in sc),
          f"players={sc}")

    rec = post("player_record", f"web_id=1&area_id=1&pcuid={s}")
    total = re.search(r"<total><g2_scramble>(.*?)</g2_scramble></total>", rec, re.S)
    info = re.search(r"<g2_scramble><history><belong>{}\D*?<win_nr>(\d+)</win_nr>".format(SCHOOL), rec)
    check("player_record total counts the scramble win",
          bool(total) and f"<belong>{SCHOOL}</belong>" in (total.group(1) if total else ""),
          f"total g2_scramble={total.group(1) if total else '-'}")
    check("player_record info counts it too", bool(info), f"info win_nr={info.group(1) if info else '-'}")

    bad = [n for n, ok in results if not ok]
    print(f"\n{'PROBLEMS: ' + ', '.join(bad) if bad else 'ALL CHECKS PASS'} ({len(results)} checks)")
    return 1 if bad else 0


if __name__ == "__main__":
    sys.exit(main())
