# S2 - Real provider capture ( <provider-host>)

Status: PASS. A full arcade-mode session ran against a community e-amusement provider:
new card registration, paid play, one complete 4-player ranked match (server-driven),
settlement, session exit, and a second login. Everything the Option S shim must speak,
in the provider's own words, on the wire. Sanitized evidence: [capture/s2-evidence-sanitized.md](../capture/s2-evidence-sanitized.md).

## The decisive fact: the provider is full-stack

The client asks eAC3 `vfgac.service_list` at title; our mod logs the parse
(`[S2CAP]` lines, mods/S1Freeze v0.2). The provider answers:

```
service_url = http:// <provider-host>/aog/
services    = front -> http:// <provider-host>/aog/ (mode=operation)
              game  -> http:// <provider-host>/aog/ (mode=operation)
```

`SystemConfigGameData.FrontServerUrl` is set to exactly that, and every subsequent
front request lands there with `resultcode:0`. So the same host at `:80` serves the
entire HTTP game API that our mock (`mock/`) emulates. XRPC rides separately to
 <provider-ip>:8727 (spice2x `-url .../service/init`). This is the template for
"existing service provider" hosting: an Option S front server only has to reproduce
what this host reproduced.

## Front API actually exercised (session totals)

`appli_info` x5, `chk_tabooword`, `client_state_read` x4, `client_state_write` x19,
`create_player`, `dojo_status` x2, `entry_game`, `gacha_info`, `g_chat` x19, `g_get` x~350,
`g_post` x~190, `get_menudata`, `login`, `mission_date`, `player_record`, `present_done`,
`end_game`. Same vocabulary as the S1 mock templates - no endpoint the mock invented,
none missing that the flow needed.

## Match loop wire shapes

- Session context: `gget`/`gpost` carry `web_id`, `area_id`, `pcuid`, `gserv_id`, and a
  `must=2.0.2/<pcuid>/<tid>/<pindex>/<mid>/<window>/<cmd>/<sno>/...` chain; `gpost` adds
  `tid` (table id), `mid` (match id), `pindex`. The shim must mint `tid`/`mid` at
  `entry_game` and advance the sno chain exactly as the client expects.
- Client-to-server commands observed: `SUTE_PAI` (with `pindex`, `next_sno`, `tepai_pos`,
  `pai_id`, `reach_flg`, `tsumogiri_flg`), `NAKINASHI`, `NEXT_KYOKU_READY`.
- `end_game` posts `tid`/`mid`/`pindex`, then result scenes read server-computed scores
  and uma (`PlayerResultInfo Parse: index=1, rank=1, score=25200, uma=10000`).

## Manual input proof (the gameplay question Option S was waiting on)

Touch simulation works end to end: select a hand tile (click) -> tap the river -> the
client sends `SUTE_PAI ... tepai_pos:11 tsumogiri_flg:0` (selected tile, not the drawn
tile) and the server accepts it. Evidence line is in the sanitized capture.

Idle behavior matters for browser UX: after a few seconds of no input the cabinet shows
"無操作状態が続いた為、強制的にツモ切りを行います。画面タッチで解除されます" and auto-discards
the drawn tile. One screen touch cancels it. An idle self-play fallback like this is
probably worth keeping in the browser build.

## Card/login flow notes (arcade provider mode)

- New registration chain: consent dialog -> PIN set + confirm -> `cardmng.authpass`,
  `cardmng.bindmodel` -> player-name kana keyboard -> `chk_tabooword` -> PASELI payment
  screen. Returning logins: consent-free, one PIN entry, straight to lobby.
- The PIN pad shuffles its digit positions every session (anti-peek). Screenshot the
  screen, locate digits, tap. The spiceapi `keypads.write` module exists as a
  hardware-keypad alternative (never needed; touch worked).
- Card appears automatically (`-card0` injects a virtual card that auto-presents at the
  attract/card flow). F1 adds credit (owner's tip; PASELI balance makes it optional -
  the provider granted a 10000 PASELI demo balance and `vfgac.send_paylog` records bills).
- Session end: after credits run out the cabinet walks Result -> Gameover -> Login;
  re-login costs nothing extra beyond the PIN.
- Dev-server tables fill instantly with CPU opponents (rank-labeled), so a lone player
  is fine - matchmaking never starved.

## Ops lessons

- `-netdump` (automap) was active for the first arcade launch, which died right after
  `update_refer` (WER dump, like the earlier boot-click crash). The whole capture session
  ran with `-netdump` removed and never crashed. The mod already logs every front
  request/response and `log.txt` logs every XRPC verb, so `-netdump` buys nothing here;
  scripts/s2_run.ps1 omits it by default.
- Identity values (PCBID/card/PIN/pcuid) live only in the install-root `s2-local.txt`
  (gitignored) and never entered the repo; the evidence file masks them.
- Window focus: `SetForegroundWindow` before each click batch; if another app steals
  focus mid-session, clicks land on whatever is on top (one capture caught Explorer).

## Reproduce

1. `scripts/s2_run.ps1` (reads `s2-local.txt`; kills stale instance, launches arcade mode
   with mod + api, waits for card inquire).
2. On title: F1 (optional), tap attract (~960,1010).
3. Card flow: consent accept -> PIN on scrambled pad (screenshot first) -> name if new.
4. Payment: PASELI 100 yen row. Lobby -> 対局 (1080,685) -> 4人打ち 対局開始 (1385,330)
   -> close popup -> コツコツ.
5. Board: click-to-select, click-river-to-discard; the gget/gpost loop visible live in
   `dev/raw/log/output_log.txt` thanks to the mod.

## What this settles for the plan

- Track B Option S now has a reference implementation to imitate rather than guesswork:
  our shim needs service_list semantics, the login/menudata/record chain, the
  entry_game -> gget/gpost command engine, end_game settlement, and PASELI/billing stubs.
- "Eventually use an existing service provider" is viable exactly as-is: point `-url` at
  the provider and play. The browser reconstruction (Option S WebGL + our shim) inherits
  the same seam.
- Next code milestone unchanged: scripted gget/gpost mahjong engine in the mock
  (S3), now unblocked by having real command traces to test against.
