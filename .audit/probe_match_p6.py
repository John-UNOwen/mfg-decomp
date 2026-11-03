"""P6 match lifecycle + records probe against the live mfg@asphyxia front server.

Reproduces each audit finding on the running server and prints PASS/FAIL per
finding, so the same script is the before/after measurement.

usage:
  python .audit/probe_match_p6.py                # everything, incl. playing out a match
  python .audit/probe_match_p6.py --no-play      # addressing / limits / records only
  python .audit/probe_match_p6.py --tag before   # label the run
"""
import io
import json
import re
import sqlite3
import sys
import time
import urllib.parse
import urllib.request

sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding="utf-8", errors="replace")

BASE = "http://127.0.0.1:8083/aog/"
DB = r"C:\mfg\tmp\RyuNET-core\dist\savedata\mfg@asphyxia.db"
APPVER = "VFG-2026060900"
BOGUS_TID = 424242
LIMIT_KEYS = [
    "ste_oya1_limit_time", "ste_limit_time", "ste_reechi1_limit_time",
    "naki_limit_time", "agari_limit_time", "naki_choice_limit_time",
    "reechi_choice_limit_time", "last_cyoukou_limit_time", "last_time",
]

# SEND_COMMAND_TYPE (AC/MFG.Taikyoku.Command.Send.*_COMMAND_TYPE.cs)
S_SUTE_PAI, S_NAKINASHI, S_NEXT_KYOKU_READY = 2, 11, 15
# RECEIVE_COMMAND_TYPE
K_TSUMO, K_TSUMOCHOICES, K_SUTECHOICES, K_KYOKUSTART, K_KYOKUEND = 1, 15, 16, 17, 23

results = []


def check(name, ok, detail=""):
    results.append((name, bool(ok)))
    print(f"  {'PASS' if ok else 'FAIL':4s}  {name}" + (f"  {detail}" if detail else ""))


def post(name, body):
    req = urllib.request.Request(
        BASE + name, data=body.encode(),
        headers={"Content-Type": "application/x-www-form-urlencoded"})
    try:
        with urllib.request.urlopen(req, timeout=60) as r:
            return r.read().decode("utf-8", "replace")
    except urllib.error.HTTPError as e:
        return f"HTTPERROR {e.code}"


def code(xml):
    m = re.search(r"<serv_st><code>(\d+)</code>", xml)
    return int(m.group(1)) if m else -1


def login(ref, guest):
    x = post("login", f"system_id=AUDIT2&sic=VFG:J:A:A:2026060900&loc_id=ea&web_id=1&area_id=1"
                      f"&user_id={ref}&bypass_auth=true&guest={1 if guest else 0}")
    m = re.search(r"<session_id>([0-9a-f]+)</session_id>", x)
    sess = m.group(1) if m else ""
    m = re.search(r"<mid>(\d+)</mid>", post("get_menudata", f"web_id=1&area_id=1&pcuid={sess}"))
    return sess, (int(m.group(1)) if m else 0)


def entry(sess, gmode, extra=""):
    x = post("entry_game", f"pcuid={sess}&area_id=1&web_id=1&gmode={gmode}&taku_class=1"
                           f"&match_mode=1&passphrase=%20&teammatch=0&consume_oids=&entry_id=-1"
                           f"&pro_stay=0&competition_id=-1&shop_competition_id=-1&debug_score=-1{extra}")
    fields = {}
    for m in re.finditer(r"<(gserv_id|tid|pindex|next_sno|last_cyoukou_num|cyoukou_num|"
                         + "|".join(LIMIT_KEYS) + r"|pay_mode)>([^<]*)</", x):
        fields[m.group(1)] = m.group(2)
    return fields, x


def gget(sess, tid, pindex, mid, cursor, ready=1):
    must = f"{APPVER}/{sess}/{tid}/{pindex}/{mid}/{cursor}"
    return post("gget", f"web_id=1&area_id=1&pcuid={sess}&gserv_id=1&ready={ready}&must={must}")


def gpost(sess, tid, pindex, mid, seq, kind, pai=0, tepai=0, tepai2=0, reach=0, tsumo=0, naki=0):
    must = f"{APPVER}/{sess}/{tid}/{pindex}/{mid}/{seq}/{kind}/0/0/{pai}/{tepai}/{tepai2}/{reach}/{tsumo}/{naki}"
    return post("gpost", f"web_id=1&area_id=1&pcuid={sess}&gserv_id=1&tid={tid}&mid={mid}"
                         f"&pindex={pindex}&must={must}&app_param=0/0")


CELL_RE = re.compile(r'<cell_data_(\d+) kind="(\d+)"([^>]*)>(.*?)</cell_data_\1>', re.S)


def cells(xml):
    out = []
    for m in CELL_RE.finditer(xml):
        seq, kind, attrs, inner = int(m.group(1)), int(m.group(2)), m.group(3), m.group(4)
        pis = {int(i) for i in re.findall(r'pi(\d)="1"', attrs)}
        out.append((seq, kind, pis, inner))
    return out


def cell_window(xml):
    m = re.search(r'<cell_sno start="(\d+)" count="(\d+)"', xml)
    return (int(m.group(1)), int(m.group(2))) if m else (0, 0)


def stamp_rows(xml):
    return [int(m.group(1)) for m in re.finditer(r'<d idx="(\d+)"', xml)]


def db_archives():
    con = sqlite3.connect(f"file:{DB}?mode=ro", uri=True)
    out = []
    for row in con.execute("SELECT data FROM documents WHERE data LIKE '%match_archive%' ORDER BY createdAt"):
        out.append(json.loads(row[0]))
    con.close()
    return out


def play_match(sess, tid, pindex, mid, kyoku_limit=8, max_polls=6000):
    """Auto-play one human seat: discard the drawn tile, decline calls, advance kyokus."""
    cursor = 0
    drawn = None
    kyoku_done = 0
    polls = 0
    last_end_stat = None
    while polls < max_polls:
        polls += 1
        x = gget(sess, tid, pindex, mid, cursor)
        start, count = cell_window(x)
        acted = False
        for seq, kind, pis, inner in cells(x):
            if kind == K_TSUMO and pindex in pis:
                m = re.search(r"<pai>(\d+)</pai>", inner)
                drawn = int(m.group(1)) if m else None
            if kind == K_TSUMOCHOICES and pindex in pis:
                gpost(sess, tid, pindex, mid, seq, S_SUTE_PAI, pai=drawn or 0, tsumo=1)
                acted = True
            elif kind == K_SUTECHOICES and pindex in pis:
                gpost(sess, tid, pindex, mid, seq, S_NAKINASHI)
                acted = True
            elif kind == K_KYOKUEND:
                kyoku_done += 1
                m = re.search(r"<end_stat>(\d+)</end_stat>", inner)
                last_end_stat = int(m.group(1)) if m else 0
                if not last_end_stat:
                    gpost(sess, tid, pindex, mid, seq, S_NEXT_KYOKU_READY)
                    acted = True
        cursor = max(cursor, start + count)
        if last_end_stat == 1 or (kyoku_limit and kyoku_done >= kyoku_limit):
            break
        if not acted and count == 0:
            time.sleep(0.5)  # waiting for the lobby force-match window
    return polls, kyoku_done, last_end_stat


def main():
    tag = "after"
    play = True
    new_tids = []
    args = sys.argv[1:]
    for i, a in enumerate(args):
        if a == "--no-play":
            play = False
        if a == "--tag":
            tag = args[i + 1]
    print(f"probe run: tag={tag} play={play}")

    # --- finding (a): table addressing -------------------------------------
    print("\n[a] request addressing (no table behind it)")
    s1, m1 = login("P6NOSEAT", True)
    x = gget(s1, BOGUS_TID, 0, m1, 0)
    check("gget unknown tid answers no-table", code(x) == 770, f"code={code(x)} mwait={'yes' if '<mwait' in x else 'no'}")
    x = gpost(s1, BOGUS_TID, 0, m1, 0, S_SUTE_PAI, pai=32, tsumo=1)
    check("gpost unknown tid answers no-table", code(x) == 770, f"code={code(x)}")
    x = post("end_game", f"web_id=1&area_id=1&pcuid={s1}&tid={BOGUS_TID}&pindex=0&mid={m1}&version={APPVER}")
    scores = re.findall(r"<score>(\d+)</score>", x)
    check("end_game unknown tid answers no-table", code(x) == 770,
          f"code={code(x)} synthesized_rows={len(scores)} mgresult={'yes' if '<mgresult' in x else 'no'}")
    x = post("kiken_game", f"web_id=1&area_id=1&pcuid={s1}&tid={BOGUS_TID}&pindex=0&mid={m1}&version={APPVER}")
    check("kiken_game unknown tid answers no-table", code(x) == 770,
          f"code={code(x)} mgresult={'yes' if '<mgresult' in x else 'no'}")

    # --- finding (d): entry limit units ------------------------------------
    print("\n[d] entry_game limit units")
    s2, m2 = login("P6LIMITS", True)
    f, raw = entry(s2, 4)
    vals = {k: int(f.get(k, -1)) for k in LIMIT_KEYS}
    over = {k: v for k, v in vals.items() if v > 99 or v <= 0}
    check("entry limit times are seconds (<=99)", not over, f"{vals} offenders={over}")
    tid2, pindex2 = int(f.get("tid", 0)), int(f.get("pindex", 0))

    # --- finding (a): a foreign tid must not serve your own table ----------
    print("\n[a] addressing a foreign tid while seated")
    x = gget(s2, BOGUS_TID, pindex2, m2, 0)
    check("seated gget with foreign tid is refused", code(x) == 770, f"code={code(x)} cells={len(cells(x))}")
    x = gget(s2, tid2, pindex2, m2, 0)
    check("seated gget with its own tid still works", code(x) == 0 and "<mwait" in x,
          f"code={code(x)} tid={tid2}")

    # --- finding (f): stamp cursor ----------------------------------------
    print("\n[f] stamp cursor on gget (GGet.cs sends no stamp cursor)")
    xg = post("gchat", f"web_id=1&area_id=1&pcuid={s2}&tid={tid2}&mid={m2}&pindex={pindex2}"
                        f"&name=P6&contents=TableSticker001&param=")
    a = stamp_rows(xg)
    b = stamp_rows(gget(s2, tid2, pindex2, m2, 0))
    c = stamp_rows(gget(s2, tid2, pindex2, m2, 0))
    allrows = a + b + c
    dupes = sorted({i for i in allrows if allrows.count(i) > 1})
    check("every stamp reaches the seat exactly once", bool(allrows) and not dupes,
          f"gchat={a} gget1={b} gget2={c} dupes={dupes}")

    # --- findings (b)(c)(e): a played match, its archive and its records ----
    if play:
        print("\n[b,c,e] nima match (gmode 4) -> mgresult / archive / records")
        s3, m3 = login("P6NIMA", True)
        f3, raw3 = entry(s3, 4)
        tid3, pindex3 = int(f3["tid"]), int(f3["pindex"])
        polls, kyoku, end_stat = play_match(s3, tid3, pindex3, m3, kyoku_limit=0)
        print(f"     tid={tid3} polls={polls} kyoku={kyoku} end_stat={end_stat}")
        x = post("end_game", f"web_id=1&area_id=1&pcuid={s3}&tid={tid3}&pindex={pindex3}&mid={m3}&version={APPVER}")
        gm = re.search(r"<mgresult><gmode>(\d+)</gmode>", x)
        check("mgresult reports the entered gmode", gm and int(gm.group(1)) == 4,
              f"mgresult gmode={gm.group(1) if gm else '-'} (entered 4) code={code(x)}")
        rows4 = len(re.findall(r"<player_\d>", x))
        check("mgresult covers the nima seats", rows4 == 2, f"player_N rows={rows4}")

        time.sleep(1.2)  # archive write is fire-and-forget
        arts = [a for a in db_archives() if a.get("tid") == tid3]
        check("archive records the real gmode", bool(arts) and arts[-1].get("gmode") == 4,
              f"archive gmode={arts[-1].get('gmode') if arts else '-'}")
        check("archive records taku_class", bool(arts) and "taku_class" in arts[-1],
              f"archive keys={sorted(arts[-1].keys()) if arts else '-'}")
        check("archive attributes the human seat by refid",
              bool(arts) and any(p.get("refid") for p in arts[-1].get("players", [])),
              f"players={[(p.get('name'), p.get('mid'), p.get('refid'), p.get('guest')) for p in arts[-1].get('players', [])] if arts else '-'}")

        rec = post("player_record", f"web_id=1&area_id=1&pcuid={s3}")
        check("player_record emits <total>", "<total>" in rec or "<total/>" in rec,
              f"total={'yes' if '<total' in rec else 'no'}")
        check("player_record emits g2_scramble in total and infos",
              rec.count("g2_scramble") >= 2, f"g2_scramble blocks={rec.count('g2_scramble')}")

        mine = post("get_haifu_data", f"version={APPVER}&pcuid={s3}&web_id=1&area_id=1&tid={tid3}")
        other_s, other_m = login("P6PEEPING", False)
        peek = post("get_haifu_data", f"version={APPVER}&pcuid={other_s}&web_id=1&area_id=1&tid={tid3}")
        check("get_haifu_data serves the participant", "<taikyoku" in mine and len(cells(mine)) > 0,
              f"cells={len(cells(mine))}")
        check("get_haifu_data refuses a non-participant", "<taikyoku" not in peek,
              f"peeker cells={len(cells(peek))}")

        gl = re.findall(r"<tid>(\d+)</tid>", post("get_haifu_list", f"version={APPVER}&pcuid={s3}&web_id=1&area_id=1"))
        check("the player's own list has the match", tid3 in [int(t) for t in gl], f"tids={gl[:8]}")
        for ref in ("MFGTESTGUEST1", "P6PEEPING"):
            s_card, m_card = login(ref, False)
            lst = re.findall(r"<tid>(\d+)</tid>", post("get_haifu_list", f"version={APPVER}&pcuid={s_card}&web_id=1&area_id=1"))
            rec_card = post("player_record", f"web_id=1&area_id=1&pcuid={s_card}")
            leak = tid3 in [int(t) for t in lst] or f"tid={tid3}" in rec_card
            check(f"account {ref} (mid {m_card}) does not inherit a guest match", not leak,
                  f"list tids={lst[:8]}")

        print("\n[c] sanma match (gmode 3)")
        s4, m4 = login("P6SANMA", True)
        f4, raw4 = entry(s4, 3)
        tid4, pindex4 = int(f4["tid"]), int(f4["pindex"])
        polls, kyoku, end_stat = play_match(s4, tid4, pindex4, m4, kyoku_limit=0)
        print(f"     tid={tid4} polls={polls} kyoku={kyoku} end_stat={end_stat}")
        x = post("end_game", f"web_id=1&area_id=1&pcuid={s4}&tid={tid4}&pindex={pindex4}&mid={m4}&version={APPVER}")
        gm = re.search(r"<mgresult><gmode>(\d+)</gmode>", x)
        check("sanma settlement says gmode 3", gm and int(gm.group(1)) == 3,
              f"mgresult gmode={gm.group(1) if gm else '-'}")
        check("sanma settlement covers 3 seats", len(re.findall(r"<player_\d>", x)) == 3,
              f"player_N rows={len(re.findall('<player_\\d>', x))}")
        time.sleep(1.2)
        arts = [a for a in db_archives() if a.get("tid") == tid4]
        summary = f"gmode={arts[-1].get('gmode')} seats={arts[-1].get('seats')}" if arts else "-"
        check("sanma archive records gmode 3 and 3 seats",
              bool(arts) and arts[-1].get("gmode") == 3 and arts[-1].get("seats") == 3,
              f"archive {summary}")
        new_tids.append(tid3)
        new_tids.append(tid4)

    # --- tid allocation summary -------------------------------------------
    print("\n[b] tid allocation")
    arts = db_archives()
    tids = [a.get("tid") for a in arts]
    dupes = sorted({t for t in tids if tids.count(t) > 1})
    print(f"     archive tids={tids} duplicates={dupes}")
    print(f"     max archive tid={max(tids) if tids else 0}")

    bad = [n for n, ok in results if not ok]
    print(f"\n{'PROBLEMS: ' + ', '.join(bad) if bad else 'ALL CHECKS PASS'} ({len(results)} checks)")
    return 1 if bad else 0


if __name__ == "__main__":
    sys.exit(main())
