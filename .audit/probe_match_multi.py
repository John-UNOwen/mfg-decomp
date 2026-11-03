"""Regression probes for the mfg@asphyxia match lifecycle after the P6 fix:
tid allocation across restarts, the kiken (abort) path, and a two-human lobby.

usage:
  python .audit/probe_match_multi.py tid
  python .audit/probe_match_multi.py kiken
  python .audit/probe_match_multi.py lobby
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
S_SUTE_PAI, S_NAKINASHI, S_NEXT_KYOKU_READY, S_KIKEN = 2, 11, 15, 13
K_TSUMO, K_TSUMOCHOICES, K_SUTECHOICES, K_KYOKUEND = 1, 15, 16, 23
CELL_RE = re.compile(r'<cell_data_(\d+) kind="(\d+)"([^>]*)>(.*?)</cell_data_\1>', re.S)
results = []


def check(name, ok, detail=""):
    results.append((name, bool(ok)))
    print(f"  {'PASS' if ok else 'FAIL':4s}  {name}" + (f"  {detail}" if detail else ""))


def post(name, body):
    req = urllib.request.Request(BASE + name, data=body.encode(),
                                 headers={"Content-Type": "application/x-www-form-urlencoded"})
    try:
        with urllib.request.urlopen(req, timeout=60) as r:
            return r.read().decode("utf-8", "replace")
    except urllib.error.HTTPError as e:
        return f"HTTPERROR {e.code}"


def code(xml):
    m = re.search(r"<serv_st><code>(\d+)</code>", xml)
    return int(m.group(1)) if m else -1


def login(ref, guest=True):
    x = post("login", f"system_id=AUDIT2&sic=VFG:J:A:A:2026060900&loc_id=ea&web_id=1&area_id=1"
                      f"&user_id={ref}&bypass_auth=true&guest={1 if guest else 0}")
    m = re.search(r"<session_id>([0-9a-f]+)</session_id>", x)
    return m.group(1) if m else ""


def entry(sess, gmode, passphrase=" "):
    x = post("entry_game", f"pcuid={sess}&area_id=1&web_id=1&gmode={gmode}&taku_class=1&match_mode=1"
                           f"&passphrase={urllib.parse.quote(passphrase)}&teammatch=0&consume_oids=&entry_id=-1"
                           f"&pro_stay=0&competition_id=-1&shop_competition_id=-1&debug_score=-1")
    f = {}
    for m in re.finditer(r"<(tid|pindex|last_time|ste_limit_time)>([^<]*)</", x):
        f[m.group(1)] = m.group(2)
    return f


def gget(sess, tid, pindex, mid, cursor, ready=1):
    return post("gget", f"web_id=1&area_id=1&pcuid={sess}&gserv_id=1&ready={ready}"
                        f"&must={APPVER}/{sess}/{tid}/{pindex}/{mid}/{cursor}")


def gpost(sess, tid, pindex, mid, seq, kind, pai=0, tsumo=0):
    return post("gpost", f"web_id=1&area_id=1&pcuid={sess}&gserv_id=1&tid={tid}&mid={mid}&pindex={pindex}"
                         f"&must={APPVER}/{sess}/{tid}/{pindex}/{mid}/{seq}/{kind}/0/0/{pai}/0/0/0/{tsumo}/0&app_param=0/0")


def cells(xml):
    out = []
    for m in CELL_RE.finditer(xml):
        pis = {int(i) for i in re.findall(r'pi(\d)="1"', m.group(3))}
        out.append((int(m.group(1)), int(m.group(2)), pis, m.group(4)))
    return out


def window(xml):
    m = re.search(r'<cell_sno start="(\d+)" count="(\d+)"', xml)
    return (int(m.group(1)), int(m.group(2))) if m else (0, 0)


def db_archives():
    con = sqlite3.connect(f"file:{DB}?mode=ro", uri=True)
    docs = [json.loads(r[0]) for r in con.execute("SELECT data FROM documents WHERE data LIKE '%match_archive%'")]
    meta = [json.loads(r[0]) for r in con.execute("SELECT data FROM documents WHERE data LIKE '%tid_counter%'")]
    con.close()
    return docs, meta


def play_to(sess, tid, pindex, mid, polls_limit=400, stop_at_kyoku=0):
    """Poll and answer windows until stop_at_kyoku kyokus are done (0 = run to match end)."""
    cursor, drawn, kyoku, polls, game_over = 0, None, 0, 0, False
    while polls < polls_limit:
        polls += 1
        x = gget(sess, tid, pindex, mid, cursor)
        start, count = window(x)
        for seq, kind, pis, inner in cells(x):
            if kind == K_TSUMO and pindex in pis:
                m = re.search(r"<pai>(\d+)</pai>", inner)
                drawn = int(m.group(1)) if m else None
            elif kind == K_TSUMOCHOICES and pindex in pis:
                gpost(sess, tid, pindex, mid, seq, S_SUTE_PAI, pai=drawn or 0, tsumo=1)
            elif kind == K_SUTECHOICES and pindex in pis:
                gpost(sess, tid, pindex, mid, seq, S_NAKINASHI)
            elif kind == K_KYOKUEND:
                kyoku += 1
                if re.search(r"<end_stat>1</end_stat>", inner):
                    game_over = True
                else:
                    gpost(sess, tid, pindex, mid, seq, S_NEXT_KYOKU_READY)
        cursor = max(cursor, start + count)
        if game_over:
            break
        if stop_at_kyoku and kyoku >= stop_at_kyoku:
            break
        if count == 0:
            time.sleep(0.4)
    return polls, kyoku


def main():
    mode = sys.argv[1] if len(sys.argv) > 1 else "tid"
    import urllib.parse  # entry() uses it

    docs, meta = db_archives()
    tids = [d.get("tid") for d in docs]
    last = max([max(tids) if tids else 0] + [int(m.get("last_tid") or 0) for m in meta])

    if mode == "tid":
        print(f"[tid] archived max / counter last_tid = {last}")
        s = login("P6RESTARTTID")
        f = entry(s, 4)
        tid = int(f["tid"])
        check("tid continues above the archived maximum after a restart", tid > last, f"new tid={tid} (previous max {last})")
        docs2, meta2 = db_archives()
        t2 = [d.get("tid") for d in docs2 if d.get("tid") == tid]
        check("the new tid is unique in the archive", len(t2) <= 1, f"docs with tid {tid}: {len(t2)}")
        print(f"     counter doc: {meta2[-1] if meta2 else '-'}")

    elif mode == "kiken":
        print("[kiken] abort path: mid-match kiken_game must still answer, must not archive")
        s = login("P6KIKEN")
        f = entry(s, 4)
        tid, pindex = int(f["tid"]), int(f["pindex"])
        mid = 0
        m = re.search(r"<mid>(\d+)</mid>", post("get_menudata", f"web_id=1&area_id=1&pcuid={s}"))
        mid = int(m.group(1)) if m else 0
        polls, kyoku = play_to(s, tid, pindex, mid, polls_limit=250, stop_at_kyoku=1)
        before = len([d for d in db_archives()[0] if d.get("tid") == tid])
        x = post("kiken_game", f"web_id=1&area_id=1&pcuid={s}&tid={tid}&pindex={pindex}&mid={mid}&version={APPVER}")
        rows = len(re.findall(r"<player_\d>", x))
        check("kiken_game answers a real settlement", code(x) == 0 and "<mgresult" in x and rows == 2,
              f"code={code(x)} rows={rows} polls={polls} kyoku={kyoku}")
        after = len([d for d in db_archives()[0] if d.get("tid") == tid])
        check("kiken writes no archive", after == before, f"archives before={before} after={after}")
        y = gget(s, tid, pindex, mid, 0)
        check("gget after kiken is refused", code(y) == 770, f"code={code(y)}")

    elif mode == "lobby":
        print("[lobby] two humans in one passphrase lobby share the table")
        sa, sb = login("P6LOBBYA"), login("P6LOBBYB")
        fa, fb = entry(sa, 1, "p6test"), entry(sb, 1, "p6test")
        ta, tb = int(fa["tid"]), int(fb["tid"])
        pa, pb = int(fa["pindex"]), int(fb["pindex"])
        check("both players are seated at the same table", ta == tb and pa != pb, f"tid {ta}/{tb} pindex {pa}/{pb}")
        ga = gget(sa, ta, pa, 0, 0)
        gb = gget(sb, tb, pb, 0, 0)
        check("both polls are served at that table", code(ga) == 0 and code(gb) == 0, f"codes {code(ga)}/{code(gb)}")
        check("each player is told their own seat", f"<pindex>{pa}</pindex>" in ga and f"<pindex>{pb}</pindex>" in gb,
              f"A sees {re.search('<pindex>([0-9]+)</pindex>', ga) and re.search('<pindex>([0-9]+)</pindex>', ga).group(1)}, "
              f"B sees {re.search('<pindex>([0-9]+)</pindex>', gb) and re.search('<pindex>([0-9]+)</pindex>', gb).group(1)}")
        check("mwait lists both humans", ga.count('ptype="1"') == 2 and gb.count('ptype="1"') == 2,
              f"human rows={ga.count('ptype=\"1\"')}")
        # B walks away: after the 60 s window the seat becomes CPU and A finishes
        polls, kyoku = play_to(sa, ta, pa, 0, polls_limit=3000, stop_at_kyoku=0)
        x = post("end_game", f"web_id=1&area_id=1&pcuid={sa}&tid={ta}&pindex={pa}&mid=0&version={APPVER}")
        check("A can close the table", code(x) == 0 and "<mgresult" in x, f"code={code(x)} polls={polls}")
        y = post("end_game", f"web_id=1&area_id=1&pcuid={sb}&tid={tb}&pindex={pb}&mid=0&version={APPVER}")
        check("B closing the same table still answers", code(y) == 0 and "<mgresult" in y, f"code={code(y)}")
        docs, _ = db_archives()
        arts = [d for d in docs if d.get("tid") == ta]
        refs = sorted({p.get("refid") for d in arts for p in d.get("players", []) if p.get("refid")})
        check("the archive names both accounts", {"P6LOBBYA", "P6LOBBYB"} <= set(refs), f"refids={refs}")
        la = re.findall(r"<tid>(\d+)</tid>", post("get_haifu_list", f"version={APPVER}&pcuid={sa}&web_id=1&area_id=1"))
        lb = re.findall(r"<tid>(\d+)</tid>", post("get_haifu_list", f"version={APPVER}&pcuid={sb}&web_id=1&area_id=1"))
        check("both accounts see the match in their own list", ta in [int(t) for t in la] and ta in [int(t) for t in lb],
              f"A list={la[:6]} B list={lb[:6]}")
        other = login("P6OUTSIDER")
        peek = post("get_haifu_data", f"version={APPVER}&pcuid={other}&web_id=1&area_id=1&tid={ta}")
        check("an unrelated account cannot read that kifu", "<taikyoku" not in peek, f"cells={len(cells(peek))}")

    elif mode == "stamp":
        print("[stamp] stamp backlog is delivered once per seat (gget carries no cursor)")
        sa, sb = login("P6STAMPA"), login("P6STAMPB")
        fa, fb = entry(sa, 1, "p6stamp"), entry(sb, 1, "p6stamp")
        ta, tb = int(fa["tid"]), int(fb["tid"])
        pa, pb = int(fa["pindex"]), int(fb["pindex"])
        check("both seats share one lobby table", ta == tb, f"tid {ta}/{tb} pindex {pa}/{pb}")

        def rows_of(xml):
            return [int(m) for m in re.findall(r'<d idx="(\d+)"', xml)]

        x = post("gchat", f"web_id=1&area_id=1&pcuid={sb}&tid={ta}&mid=0&pindex={pb}"
                          f"&name=B&contents=TableSticker002&param=")
        posted = rows_of(x)
        check("the posting seat gets its own sticker", len(posted) >= 1, f"idx={posted}")
        r1 = rows_of(gget(sa, ta, pa, 0, 0))
        r2 = rows_of(gget(sa, ta, pa, 0, 0))
        r3 = rows_of(gget(sa, ta, pa, 0, 0))
        check("the other seat receives it exactly once", len(r1) >= 1 and not (set(r2) | set(r3)),
              f"first={r1} second={r2} third={r3}")
        x2 = post("gchat", f"web_id=1&area_id=1&pcuid={sa}&tid={ta}&mid=0&pindex={pa}"
                           f"&name=A&contents=TableSticker003&param=")
        posted2 = rows_of(x2)
        r4 = rows_of(gget(sa, ta, pa, 0, 0))
        r5 = rows_of(gget(sb, ta, pb, 0, 0))
        fresh = sorted((set(posted2) | set(r4)) - set(r1) - set(posted))
        in_gchat = sorted(set(fresh) & set(posted2))
        in_gget = sorted(set(fresh) & set(r4))
        twice = sorted(set(in_gchat) & set(in_gget))
        check("a new sticker reaches its seat exactly once", bool(fresh) and not twice,
              f"new idx={fresh} gchat={in_gchat} gget={in_gget} twice={twice}")
        check("and to the other seat too", bool(set(r5) - set(r1) - set(posted)),
              f"B new idx={sorted(set(r5) - set(r1) - set(posted))}")
        # the sticker window carries its own cursor in stamp_info=RoomID,SeqID
        w1 = rows_of(post("gget_stamp_info", f"web_id=1&area_id=1&pcuid={sa}&gserv_id=1"
                                             f"&must={APPVER}/{sa}/{ta}/{pa}/0/0&stamp_info={ta},0"))
        w2 = rows_of(post("gget_stamp_info", f"web_id=1&area_id=1&pcuid={sa}&gserv_id=1"
                                             f"&must={APPVER}/{sa}/{ta}/{pa}/0/0&stamp_info={ta},{max(w1) if w1 else 0}"))
        check("gget_stamp_info honours the client-provided SeqID cursor",
              bool(w1) and not (set(w1) & set(w2)), f"since0={w1} sinceMax={w2}")
        post("kiken_game", f"web_id=1&area_id=1&pcuid={sa}&tid={ta}&pindex={pa}&mid=0&version={APPVER}")
        post("kiken_game", f"web_id=1&area_id=1&pcuid={sb}&tid={tb}&pindex={pb}&mid=0&version={APPVER}")

    elif mode == "reconnect":
        print("[reconnect] reconnect hands back the same seat, with usable limits")
        s = login("P6RECONNECT")
        f = entry(s, 4)
        tid, pindex = int(f["tid"]), int(f["pindex"])
        polls, kyoku = play_to(s, tid, pindex, 0, polls_limit=200, stop_at_kyoku=1)
        r = post("reconnect", f"pcuid={s}&web_id=1&area_id=1")
        same = f"<tid>{tid}</tid>" in r and f"<pindex>{pindex}</pindex>" in r
        limits = {m.group(1): int(m.group(2)) for m in re.finditer(r"<(\w*_limit_time|last_time)>(\d+)</", r)}
        bad_limits = {k: v for k, v in limits.items() if v > 99 or v <= 0}
        check("reconnect answers the entry block for the same seat", code(r) == 0 and same,
              f"code={code(r)} tid={tid} polls={polls}")
        check("reconnect limits are seconds too", bool(limits) and not bad_limits,
              f"limits={limits} offenders={bad_limits}")
        g = gget(s, tid, pindex, 0, 0)
        check("the match continues after reconnect", code(g) == 0 and cells(g),
              f"code={code(g)} cells={len(cells(g))}")
        g2 = gget(s, tid, 1 - pindex, 0, 0)
        mine = re.search(r"<mwait[^>]*>.*?<pindex>(\d+)</pindex>", g2, re.S)
        check("a claimed seat that is not yours is answered with your real seat",
              code(g2) == 0 and mine and int(mine.group(1)) == pindex,
              f"code={code(g2)} answered pindex={mine.group(1) if mine else '-'} (real {pindex})")
        post("kiken_game", f"web_id=1&area_id=1&pcuid={s}&tid={tid}&pindex={pindex}&mid=0&version={APPVER}")

    elif mode == "record":
        print("[record] player_record files each match under the mode it was played as")
        for ref, expect in (("P6NIMA", "Nima"), ("P6SANMA", "Sanma")):
            s = login(ref, True)
            rec = post("player_record", f"web_id=1&area_id=1&pcuid={s}")
            kinds = re.findall(r"<info><kind>(\w+)</kind>", rec)
            infos = list(re.finditer(r"<info><kind>\w+</kind>.*?</info>", rec, re.S))
            nrank = 0
            for m2 in infos:
                m = re.search(r"<rank_list>((?:<rank_count>\d+</rank_count>)+)", m2.group(0))
                if m:
                    nrank = max(nrank, len(re.findall(r"<rank_count>", m.group(1))))
            check(f"{ref} lists a {expect} record", expect in kinds, f"kinds={kinds}")
            check(f"{ref} rank_list has as many counts as its seats",
                  expect in kinds and nrank == (2 if expect == "Nima" else 3), f"max rank_count fields={nrank}")
            histories = re.findall(r"<histories>(.*?)</histories>", rec, re.S)
            check(f"{ref} every info carries its own <histories>",
                  len(histories) == len(kinds) and all(h.strip() for h in histories),
                  f"infos={len(kinds)} histories={len(histories)}")

    elif mode == "replay":
        print("[replay] the kifu the list offers is the match that was played")
        s = login("P6REPLAY", True)
        f = entry(s, 3)
        tid, pindex = int(f["tid"]), int(f["pindex"])
        polls, kyoku = play_to(s, tid, pindex, 0, polls_limit=3000, stop_at_kyoku=0)
        post("end_game", f"web_id=1&area_id=1&pcuid={s}&tid={tid}&pindex={pindex}&mid=0&version={APPVER}")
        time.sleep(1.5)
        docs, _ = db_archives()
        art = [d for d in docs if d.get("tid") == tid]
        recorded = len(art[0].get("cells") or []) if art else 0
        lst = post("get_haifu_list", f"version={APPVER}&pcuid={s}&web_id=1&area_id=1")
        row = re.search(rf"<haifu><gmode>(\d+)</gmode><taku_class>(\d+)</taku_class><tid>{tid}</tid><pindex>(\d+)</pindex>", lst)
        check("the list row carries the archived mode and seat", bool(row) and int(row.group(1)) == 3,
              f"list row={row.groups() if row else '-'} polls={polls} kyoku={kyoku}")
        d = post("get_haifu_data", f"version={APPVER}&pcuid={s}&web_id=1&area_id=1&tid={tid}")
        core = re.search(r"<core_io><in><tmake><play_mode><gmode>(\d+)</gmode></play_mode><pnum>(\d+)</pnum>", d)
        nz = len(re.findall(r"<zaseki_\d>", d))
        check("the kifu replays the right match (gmode/pnum from the archive)",
              bool(core) and int(core.group(1)) == 3 and int(core.group(2)) == 3,
              f"core_io gmode/pnum={core.groups() if core else '-'}")
        got = len(cells(d))
        check("the kifu carries the recorded cell stream", got == recorded and got > 0,
              f"cells served={got} recorded={recorded}")
        check("zaseki covers every seat", nz == 4, f"zaseki={nz}")

    bad = [n for n, ok in results if not ok]
    print(f"\n{'PROBLEMS: ' + ', '.join(bad) if bad else 'ALL CHECKS PASS'} ({len(results)} checks)")
    return 1 if bad else 0


if __name__ == "__main__":
    import urllib.parse
    sys.exit(main())
