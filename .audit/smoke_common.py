"""Shared plumbing for the .audit mfg smoke sweeps
(smoke_mfg_endpoints.py = integrated 8083 lane, smoke_mfg_endpoints_p7.py = a lane
you pass on argv). Both sweeps used to carry their own copy of the music gacha
check and of the "server is running" sentence, and both copies were wrong the
same way:

* The play was posted off the reserve's serv_st code, which stays 0 even when the
  reserve refused. The refusal lives in the body: MusicGachaPlayReserve.cs:29-34
  reads <is_success> and <request_id> as chained Element() hops, and
  handle_music_gacha_play_reserve (handlers/aog.ts:2450-2463) answers a refusal as
  <is_success>0</is_success><request_id>0</request_id> on purpose, because
  GachaSelectFlow.cs:436-449 re-enables the buttons on is_success 0 while an error
  serv_st throws NetworkResponseException -> ERROR_STOP. Playing that request_id is
  then correctly refused with 930 (StatusCode.AlreadyFinishedTransaction,
  handlers/aog.ts:2497), so the sweep printed a regression the server never had.
* The fixture account was fixed. Music possession is server-owned and permanent
  (recordMusicGain -> profile.states.music_gacha, handlers/aog.ts:860-879), so one
  account can reserve a given series once in the life of a database: the sweep was
  good for one run and failed deterministically on every later one.
* "server left running (pid N)" was written from memory and was wrong. A sweep now
  measures the listener before it probes anything and re-measures it at the end;
  a live-server claim has to quote those lines.
* The lane a sweep ran against was named from memory too - "verify-dist", port 8095 - and
  the plugin copy in that directory was whatever revision it happened to be copied at, so
  the run verified a build nobody could name. A sweep now refuses to probe a base whose
  build it cannot identify (build_identity / report_build): either the canonical integrated
  port, or a lane staged from that tree whose LANE_STAMP.json still matches it.
"""
import base64
import hashlib
import json
import os
import re
import shutil
import subprocess
import time
import urllib.parse
import urllib.request

# One canonical plugin tree: the only copy that is edited and the only copy the core loads.
# Everything else is derived from it (scripts/ryunet_stage.ps1) and has to say so.
REPO_ROOT = r"C:\mfg"
CORE_ROOT = os.path.join(REPO_ROOT, "tmp", "RyuNET-core")
CANONICAL_PLUGIN = os.path.join(CORE_ROOT, "dist", "plugins", "mfg@asphyxia")


def post(base, name, body, timeout=30):
    req = urllib.request.Request(base + name, data=body.encode(),
                                 headers={"Content-Type": "application/x-www-form-urlencoded"})
    with urllib.request.urlopen(req, timeout=timeout) as r:
        return r.read().decode("utf-8", "replace")


def code(xml):
    m = re.search(r"<serv_st><code>(\d+)</code>", xml)
    return m.group(1) if m else "?"


def listener_pid(base):
    """Which process owns the LISTENING socket, asked from the OS.

    Returns None when the OS will not say - the sweep then prints "pid unknown"
    instead of inventing a pid. pwsh .audit/server_status.ps1 gives the same
    answer with the command line and start time.
    """
    parts = urllib.parse.urlsplit(base)
    host, port = parts.hostname or "127.0.0.1", parts.port or 80
    try:
        import psutil  # type: ignore
        status = getattr(psutil, "CONN_LISTEN", "LISTEN")
        for c in psutil.net_connections(kind="tcp"):
            if c.status == status and c.laddr and int(c.laddr.port) == int(port):
                return c.pid
        return None
    except Exception:
        pass
    try:
        exe = shutil.which("netstat") or shutil.which("ss")
        if not exe:
            return None
        netstat = "netstat" in exe.lower()
        argv = [exe, "-ano", "-p", "TCP"] if netstat else [exe, "-ltnp"]
        out = subprocess.run(argv, capture_output=True, text=True, timeout=15).stdout
        for line in out.splitlines():
            cols = line.split()
            local, pid = None, None
            if netstat:
                # TCP  127.0.0.1:8083  0.0.0.0:0  LISTENING  30244
                if len(cols) >= 5 and cols[0].upper() == "TCP" and cols[3].upper() in ("LISTENING", "LISTEN"):
                    local, pid = cols[1], cols[4]
            else:
                # LISTEN 0 511 127.0.0.1:8083 *:* users:(("node",pid=30244,fd=24))
                if len(cols) >= 5 and cols[0].upper() in ("LISTEN", "UNCONN"):
                    local = cols[3]
                    m = re.search(r"pid=(\d+)", line)
                    pid = m.group(1) if m else None
            if not local or not local.endswith(f":{port}") or not pid or not pid.isdigit():
                continue
            addr = local.rsplit(":", 1)[0]
            if addr in ("0.0.0.0", "*", "::", "::0", host, "127.0.0.1"):
                return int(pid)
        return None
    except Exception:
        return None


def liveness(base):
    """Measured proof that a server answers on this base, plus its pid when known.

    Uses /aog/appli_boot (a registered endpoint, handlers/aog.ts:2861): an unrouted
    name is never answered on the integrated port at all, so it cannot be used to
    ask whether something is listening.
    """
    t0 = time.perf_counter()
    try:
        xml = post(base, "appli_boot", "web_id=1&area_id=1", timeout=15)
        return {"ok": True, "code": code(xml), "ms": round((time.perf_counter() - t0) * 1000),
                "pid": listener_pid(base), "detail": ""}
    except Exception as e:
        return {"ok": False, "code": "-", "ms": round((time.perf_counter() - t0) * 1000),
                "pid": None, "detail": f"{type(e).__name__}: {e}"}


def report_liveness(base, tag="server"):
    r = liveness(base)
    pid = r["pid"] if r["pid"] is not None else "unknown (pwsh .audit/server_status.ps1)"
    if r["ok"]:
        print(f"{tag:7s} {base} LISTENING pid={pid} appli_boot code={r['code']} {r['ms']}ms")
    else:
        print(f"{tag:7s} {base} NOT LISTENING ({r['detail']}) - nothing to probe; start the "
              f"core (see .audit/README.md) before quoting a live-server claim")
    return r


def plugin_tree_hashes(plugin_dir=CANONICAL_PLUGIN):
    """sha256 of every file in a plugin tree, keyed by forward-slash relative path."""
    out = {}
    for root, _dirs, files in os.walk(plugin_dir):
        for name in files:
            path = os.path.join(root, name)
            rel = os.path.relpath(path, plugin_dir).replace("\\", "/")
            h = hashlib.sha256()
            with open(path, "rb") as f:
                for chunk in iter(lambda: f.read(1 << 16), b""):
                    h.update(chunk)
            out[rel] = h.hexdigest()
    return out


def tree_digest(hashes):
    body = "\n".join(f"{k}={hashes[k]}" for k in sorted(hashes))
    return hashlib.sha256(body.encode("utf-8")).hexdigest()


def canonical_tree_digest():
    return tree_digest(plugin_tree_hashes())


def canonical_port():
    """The port the canonical tree's own core is configured for (dist/config.ini)."""
    cfg = os.path.join(CORE_ROOT, "dist", "config.ini")
    try:
        with open(cfg, encoding="utf-8", errors="replace") as f:
            for line in f:
                if line.startswith("port="):
                    return int(line.split("=", 1)[1])
    except OSError:
        pass
    return 8083


def find_staged_lane(port):
    """The lane directory staged for this port, read from its LANE_STAMP.json."""
    try:
        names = sorted(os.listdir(CORE_ROOT))
    except OSError:
        return None
    for name in names:
        stamp = os.path.join(CORE_ROOT, name, "LANE_STAMP.json")
        if not os.path.isfile(stamp):
            continue
        try:
            with open(stamp, encoding="utf-8") as f:
                data = json.load(f)
        except (OSError, ValueError):
            continue
        if int((data.get("ports") or {}).get("port") or 0) == int(port):
            data["_lane_dir"] = os.path.join(CORE_ROOT, name)
            return data
    return None


def build_identity(base):
    """Name the build a base URL answers with, or say why it cannot be named.

    ok=False has to stop a sweep. The core transpiles the plugin TypeScript at start-up, so
    "a server answered on this port" says nothing about *which* build answered, and a
    directory holding a plugin copy says nothing about whether anything boots from it. A
    lane is only identifiable when it was staged from the canonical tree by
    `pwsh scripts/ryunet_stage.ps1 -Action lane`, which writes the sha256 of every plugin file it
    holds into LANE_STAMP.json. A lane whose stamped files no longer match the canonical tree is
    an older build - measuring it and reporting it as the shipped build is what the P3/P4/P6/P7
    reports did with verify-dist / verify-p7 / check-p7.
    """
    port = urllib.parse.urlsplit(base).port or 80
    canon_files = plugin_tree_hashes()
    digest = tree_digest(canon_files)
    if port == canonical_port():
        return {"ok": True, "kind": "canonical", "lane_dir": os.path.join(CORE_ROOT, "dist"),
                "tree": digest, "staged_at": None,
                "note": "integrated port: the plugin is transpiled at start-up, so the process may "
                        "predate these files; pair a shipped-file claim with a check that loads "
                        "them directly (pwsh .audit/server_status.ps1 gives the pid and start time)"}
    lane = find_staged_lane(port)
    if lane is None:
        return {"ok": False, "kind": "unstaged", "tree": digest,
                "detail": f"no staged lane answers port {port}; stage one from the canonical tree: "
                          f"pwsh scripts/ryunet_stage.ps1 -Action lane -Lane <dir> -Port {port}"}
    # Map equality, not a combined digest: it is collation-free across PowerShell and Python
    # and it names the files that moved instead of only saying "different".
    lane_files = lane.get("plugin_files") or {}
    if lane_files != canon_files:
        lane_name = os.path.basename(lane["_lane_dir"])
        moved = [k for k in sorted(set(lane_files) | set(canon_files))
                 if lane_files.get(k) != canon_files.get(k)]
        shown = ", ".join(moved[:6]) + (f" (+{len(moved) - 6} more)" if len(moved) > 6 else "")
        return {"ok": False, "kind": "stale-lane", "lane_dir": lane["_lane_dir"], "tree": digest,
                "staged_at": lane.get("staged_at"), "moved_files": moved,
                "detail": f"{lane['_lane_dir']} holds the plugin build it was staged with at "
                          f"{lane.get('staged_at')}, and that is not the current one "
                          f"({shown or 'file set differs'}); re-stage it: "
                          f"pwsh scripts/ryunet_stage.ps1 -Action lane -Lane {lane_name}"}
    return {"ok": True, "kind": "lane", "lane_dir": lane["_lane_dir"], "tree": digest,
            "staged_at": lane.get("staged_at"), "note": "lane staged from the canonical tree"}


def report_build(base):
    b = build_identity(base)
    if not b["ok"]:
        print(f"build   {base} UNIDENTIFIED BUILD - {b['detail']}")
        return b
    staged = f" staged {b['staged_at']}" if b.get("staged_at") else ""
    print(f"build   {base} {b['kind']} {b['lane_dir']} tree={b['tree'][:12]}{staged}")
    if b.get("note"):
        print(f"        {b['note']}")
    return b


def music_gacha_state(post, sess, mid):
    """The server's own ownership ledger: state kind music_gacha, which only the
    server authors (MusicGachaGameData.cs:15 ClientStateName; no ClientStateWrite
    call site for it exists in the client). {GachaID: info} or None when not served.
    """
    r = post("client_state_read", f"web_id=1&area_id=1&pcuid={sess}&mid={mid}&one_kind=music_gacha")
    m = re.search(r'<state kind="music_gacha"><data>([^<]*)</data>', r)
    if not m:
        return None
    try:
        obj = json.loads(base64.b64decode(m.group(1)).decode("utf-8"))
    except Exception:
        return None
    out = {}
    for info in obj.get("MusicGachaInfos") or []:
        gid = info.get("GachaID")
        if gid is not None:
            out[int(gid)] = info
    return out


def reserve_state(xml):
    """(is_success, request_id) out of the body, the way the client reads it.

    serv_st is 0 on a refusal too, so this - not the code - is the gate.
    """
    ok = re.search(r"<gacha_reserve><is_success>(\d+)</is_success>", xml)
    rid = re.search(r"<request_id>(\d+)</request_id>", xml)
    return (ok.group(1) if ok else None, rid.group(1) if rid else None)


def pick_fixture(post, refs, bad):
    """Log in as the fixture accounts until one still has a music series it has not
    completed, and return (ref, session, mid, ledger).

    The rotation is what makes the sweep idempotent against a reused database: an
    account reserves each series once ever, so the choice has to come from the
    server's ledger rather than from a remembered account name. If every fixture is
    finished, the last one is returned and only the completed branch is assertable.
    """
    fallback = None
    for ref in refs:
        login = post("login", f"system_id=AUDIT2&sic=VFG:J:A:A:2026060900&loc_id=ea&web_id=1&area_id=1"
                              f"&user_id={ref}&bypass_auth=true&guest=1")
        sess_m = re.search(r"<session_id>([0-9a-f]+)</session_id>", login)
        if not sess_m:
            bad.append(f"fixture {ref}: login returned no <session_id>: {login[:140]}")
            return None
        sess = sess_m.group(1)
        menu = post("get_menudata", f"web_id=1&area_id=1&pcuid={sess}")
        mid_m = re.search(r"<mid>(\d+)</mid>", menu)
        if not mid_m:
            bad.append(f"fixture {ref}: get_menudata returned no <mid>: {menu[:140]}")
            return None
        mid = mid_m.group(1)
        state = music_gacha_state(post, sess, mid)
        if state is None:
            bad.append(f"fixture {ref}: server did not serve state kind music_gacha")
            return (ref, sess, mid, {})
        open_series = [gid for gid in sorted(state) if not state[gid].get("IsComplete")]
        print(f"fixture {ref}: mid={mid} unfinished music series={open_series or 'none (all complete)'}")
        if open_series:
            return (ref, sess, mid, state)
        fallback = (ref, sess, mid, state)
    return fallback


def pick_series(state, preferred=91):
    """First advertised series this account has not finished (the pull branch needs
    one), else the preferred series (then only the completed branch is assertable).
    """
    for gid in sorted(state):
        if not state[gid].get("IsComplete"):
            return gid
    return preferred


def music_gacha_sweep(post, check, bad, sess, mid, preferred=91):
    """One music series through both branches the server owns.

    The ledger is read fresh here, not carried over from fixture selection: it is
    what the server authored, and it decides how many pulls are left. Pulls happen
    only behind a reserve the server granted (is_success 1); the finished series is
    asserted as the refusal shape the client can recover from, and the ledger is
    checked against what was actually granted.
    """
    state = music_gacha_state(post, sess, mid)
    if state is None:
        bad.append("music gacha: server did not serve state kind music_gacha")
        return
    series = pick_series(state, preferred)
    info = state.get(series) or {}
    supply = int(info.get("GachaSupplyCount") or 0)
    owned0 = {str(o).upper() for o in (info.get("PossessionItemOIDs") or [])}
    print(f"  series={series} server ledger owned={len(owned0)}/{supply} "
          f"IsComplete={bool(info.get('IsComplete'))}")
    if supply <= 0:
        bad.append(f"music_gacha series {series}: GachaSupplyCount 0 - not an advertised series")
        return

    gained, refused = [], False
    while len(gained) <= supply + 1:
        r = post("music_gacha_play_reserve", f"web_id=1&area_id=1&pcuid={sess}&gacha_id={series}")
        check(f"reserve series={series}", r)
        ok, rid = reserve_state(r)
        if ok is None or rid is None:
            bad.append(f"music_gacha_play_reserve series {series}: no is_success/request_id in body")
            return
        if ok == "0":
            refused = True
            # The client cannot render is_success 0 out of a play (MusicGachaPlay.cs:112
            # indexes GainItems[0] unchecked), so the honest answer for a ticket that is
            # not open is 930, not a fake success.
            check(f"play on refused id={rid}",
                  post("music_gacha_play", f"web_id=1&area_id=1&pcuid={sess}&request_id={rid}"), want="930")
            break
        p = post("music_gacha_play", f"web_id=1&area_id=1&pcuid={sess}&request_id={rid}")
        check(f"music_gacha_play #{len(gained) + 1} req={rid}", p)
        items = re.findall(r"<gain_items><item>([^<]*)</item>", p)
        if len(items) != 1:
            bad.append(f"music_gacha_play #{len(gained) + 1}: {len(items)} gain items")
            return
        oid = items[0]
        if oid.upper() in owned0:
            bad.append(f"music_gacha_play granted {oid} which the ledger already owned (dedup dead again)")
        gained.append(oid)
        print(f"     pull {len(gained)}: request_id={rid} oid={oid}")

    if not refused:
        bad.append(f"music series {series}: reserve never refused after {len(gained)} pulls")
    after = music_gacha_state(post, sess, mid) or {}
    info2 = after.get(series) or {}
    owned2 = {str(o).upper() for o in (info2.get("PossessionItemOIDs") or [])}
    missing = [o for o in gained if o.upper() not in owned2]
    if missing:
        bad.append(f"music_gacha state did not record the grants: {missing}")
    if refused and len(owned2) < supply:
        bad.append(f"music series {series}: reserve refused at {len(owned2)}/{supply} owned")
    if len(owned2) >= supply and not info2.get("IsComplete"):
        bad.append(f"music series {series}: ledger has {len(owned2)}/{supply} owned but IsComplete "
                   f"is not true (client keeps the banner interactable)")
    print(f"  after: owned={len(owned2)}/{supply} IsComplete={bool(info2.get('IsComplete'))} "
          f"pulled={len(gained)} refused={refused}")
