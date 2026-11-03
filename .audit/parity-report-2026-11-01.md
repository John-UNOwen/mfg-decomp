# Game-logic parity audit - asphyxia vs the original front (2026-11-01)

What was audited: every endpoint the decompiled client calls (`MFG.GameRequest`'s 50 request
classes and their `MustNeedElement` parsers) and the state kinds the client serializes, against
the MockServer protocol reference (`mock/specs/*.xml`) and the live asphyxia stack
(`tmp/RyuNET-core/dist`, canonical plugin tree). A 54-finding checkup (4 critical / 26 major /
24 minor) drove two fix rounds; this file is the verification pass over the fixed build.

Server measured: `http://127.0.0.1:8083/aog/` pid 4304, build `c386d88cb904`
(`.audit/server_status.ps1` and the sweeps' own `LISTENING pid=N` lines agree). Plugin edits
take effect only on restart, and every sweep here quotes the pid it measured.

# 1. Sessions and accounts

| claim | verdict | measured by |
| --- | --- | --- |
| login rotates the account's session; the evicted pcuid answers 770 instead of degrading to `mid 1`/GUEST scratch | PASS | `probe_p7_verify.js` B0/B5: old pcuid dojo_get_status 770, new session code 0 keeps the dojo - immediate re-login, no waiting |
| the card-less re-login refusal (120 s idle rule) is retired: the shipping client logs in twice per boot and was locking itself out mid-boot | FIXED 2026-11-01 | lane stall evidence (`ACCOUNT REFUSED ... rule=idle session_idle=0s` vs `LoadScene-->Home` after the change); decision in KB `cardless-login-rotates-grace-is-opt-in` |
| card-bound accounts: bearer claim refused without a spendable card credential; create_player cannot rename them | PASS (code path; no card account exists in this deployment - 0 profiles carry `card_id`) | `tmp/p11acct` harness + `ACCOUNT REFUSED` log semantics |
| phantom MID family: mint barrier `MID*` in `ensureProfile`, read/write refusal, quarantine of stored rows | PASS | `p9_phantom_scan.js`: 4 MID-prefixed rows, 4 quarantined, sessions cleared, mid=0; `MIDGUESS0001` (letters, missed by the old digit-only pattern) included; boot pass retries 4x and always logs (`scanned=133 quarantined=0 already-repaired=4`) |
| trailing slash, nested client reconnect URL (`/aoghttp://…/reconnect/2.0.2/<sess>/12345/`), unknown endpoint name | PASS | direct probe: 200 on all three; reconnect answer carries the `<entry>` the client's parser requires; unknown name gets the empty-success XML, never a stall |

# 2. Match lifecycle and records

entry_game/gget/gpost/end_game/kiken_game address the tid `entry_game` handed out (probe reads
`<tid>`/`<pindex>` off the response the way `EntryTakuInfo.cs`→`GGet.cs` do), tid allocation from
the DB high-water mark, and 770 both ways (held tid code 0, unheld tid 770): `p2repro` reports
`EXPECTATIONS: all held`. `get_haifu_list` code 0. `player_record` serves `<player_record><total…`,
`gget_stamp_info` serves the kept/sticker record. `janpon_log` answers code 0 at `/aog/`. Live
lane proof: the WebGL client ran `entry_game → gget×n → kiken_game (60 s skip) → end → gameover →
logout`, and the core logged `logout retired session 3d23e46b… of refid=MFGTESTGUEST1 (no live
session on the account now)`.

# 3. Gacha

- Banner draws (12×5 on 165): zero dup-in-pull (no-replacement holds), `gift_type=1` appears only
  after an item is owned, `<gift>` counts reported per draw - probe `central_probe.cjs`.
- Weighted odds per banner pool: verified by the P4 round's before/after probes (fix report
  `17c5764aaee4`), pools read from the extracted gacha data.
- Legacy path `req_draw_gacha`/`get_gacha_result` still answers (smoke).
- Music gacha is server-authored: reserve/play ledger, pulls stop at ownership, a play on a
  refused reservation is 930 (`StatusCode.AlreadyFinishedTransaction` - a real code because
  `MusicGachaExec.cs` arms the error screen), `music_gacha.IsComplete` true once the ledger is
  full: `smoke_mfg_endpoints.py` (4/4 owned, refused, 930) on pid 4304.

# 4. Presents, items, economy

`present_done` acks are a per-profile ledger (re-grant loop broken; a receipt is never served
again - sweep rule that makes pinned fixtures single-use, `smoke_common.py`). `item_gain_log` /
`item_consume_log` code 0. Duplicate gifts land as Present01 counters in the server-owned
`player_item` (P4 verification).

# 5. Dojo, friends, borrow

- Dojo is per-account: `dojo_set_slot` on the lender leaves the borrower's 4 slots empty and
  untouched (probe B2/B3/B4, F2 DB read); no call writes through the shared `GUEST` document
  (F1: its `updatedAt` predates the run). Bogus pcuid refused 770 (A1-A3).
- `friend_game` is server-owned: a client write is refused 803, the seed lists the published
  accounts with stable grant tickets (C1-C5, two reads, same ticket).
- Borrow: `odekake_done` resolves only the grant table issued on the borrower's own profile -
  the lender's mid buys nothing (D9), unknown ids answer `code 0` + `success 0` never an error
  code (D4/D5), the served states are the lender's projection the client indexes (D1/D2/D3), and
  the borrower's own post-borrow save round-trips unchanged (D6/D7). 28/28 PASS.
- Publish consent: with `data/odekake.json publish_default: true` (this deployment's choice -
  the client can never publish, so an opt-in default would leave the feature permanently dead),
  every real account is borrowable; `VFG_ODEKAKE_PUBLISH=off` is the kill switch. Grant
  security is unchanged by it: tickets are per-viewer FNV values on the viewer's own document.

# 6. Content completeness

- Events: all 304 `GameEventType` kinds are served as rows (the client's enumeration), 117
  active in the default schedule including every feature gate the client branches on
  (`EnableOdekake`, `PremiumStartEnable`, `RevengeContinueEnable`, `ItemGainLogEnable`,
  `PrivateMatching*`, `CostumeAddition1-77`, appearances, bonus events with their ratio keys so
  an active row cannot become a penalty). Was 4 active kinds at audit time.
- Missions: all 103 categories served, the 7 everyday groups active; dated/seasonal groups
  switch on through `VFG_MISSION_ACTIVE` (unit-checked without a restart).
- Notices: authored content served (`NoticeInfo` fields the client parses), acks persist, acked
  entries are gone from the read, client writes of `receive_only` refused 803. `verify_p8.py`:
  ALL CHECKS PASSED against pid 4304.
- Screening: `chk_tabooword` no longer a no-op - 62 file words + `VFG_TABOO_EXTRA`, word-boundary
  matcher, `isTabooText` screens names in gchat/stamps and state writes, echo-accept for already
  stored taboo substrings so a save is never bricked (`p11_taboo_gates.js`: no save-brick; plain
  entries 10/10 blocked). Known hole, accepted: compound forms (カス野郎, 売春婦 inside longer
  strings, fuckyou, rapes) pass the boundary test - 0/14 blocked, listed in full by the probe.
- `chara_enabled` answers the flat root shape (`<is_gacha_enabled>1</is_gacha_enabled>`,
  `end_date`), `competition_entry` the root `<entry>`/`<displayed_result>` the parsers index.

# 7. Client regression (the functional bottom line)

WebGL lane (`-AUTOSERVE -KONAMINET -NETCAFE` guest `MFGTESTGUEST1`, webfront 8090 → core 8083):
boot → FirstCharaSelect → Home → Guide → Matching → MahjongPlay (60 s exit-skip) → Result →
gameover path back to ScopeClose, `GGet--> resultcode:0` throughout, and the core-side logout
retirement above. The lane had been BROKEN at `ScopeClose` by the 120 s idle refusal; it runs
again on the rotation semantics. Translation pack and result-audio fixes held from the prior
span (same baked page flags).

# 8. Deployment changes made by this audit

| knob | before → after | why |
| --- | --- | --- |
| `VFG_SESSION_TAKEOVER_GRACE` (config.ini + data file + built-in) | 120 → 0 | the client's own boot re-login was refused at `session_idle=0s`; card-less takeover rotates as the original front did, tombstone kept |
| `data/odekake.json publish_default` | false → true | no client-side publish exists; opt-in would mean the borrow feature never lights up on this private LAN |
| probe harnesses | pinned fixtures → per-run suffixed refids | permanent per-account state + rotation make shared accounts fight each other |

# 9. Known and accepted gaps

1. Compound taboo forms pass the word-boundary screen (listed by `p11_taboo_gates.js`); single
   words are screened everywhere.
2. `chara_enabled`/`janpon_log` are not in the core middleware's `KNOWN_AOG` set: posted at the
   bare root they 404. Every URL the client actually uses carries `/aog`, where both dispatch
   correctly; the core-side set is the one gap no plugin-side API can close.
3. Pre-fix residue the owner chose to keep: legacy dojo state on the shared `GUEST` document, a
   legacy `PLAYER` name on one old fork-era account, and the audit probe profiles in the DB.
4. Friend list = the server's published accounts; the original's cross-player social graph
   (Konami accounts) does not exist on a private LAN by construction.
5. Seasonal mission/event schedules ship inactive; flipping them is a config edit
   (`VFG_MISSION_ACTIVE`, `VFG_EVENTS_ACTIVE`), not a code change.

# 10. Sweep ledger (all against pid 4304, build c386d88cb904)

| sweep | result |
| --- | --- |
| `python .audit/smoke_mfg_endpoints.py` | SMOKE OK (gacha core + music ledger + 930) |
| `python .audit/smoke_mfg_endpoints_p7.py http://127.0.0.1:8083/aog/` | SMOKE OK (~20 endpoints) |
| `node .audit/probe_p7_verify.js http://127.0.0.1:8083` | ALL CHECKS PASSED (28/28) |
| `node .audit/probe_p7_roundtrip.js http://127.0.0.1:8083` | round-trips; only note: posted docs without `GachaCharaUsableList`/`PurchaseCharaList` come back with those arrays normalized to `[]` (client classes expect them) |
| `python tmp/p8repro/verify_p8.py` | ALL CHECKS PASSED (54) |
| `node tmp/p2repro/repro.mjs` (run from the junction cwd) | EXPECTATIONS: all held |
| `node .audit/p8_content_unit_check.js` / `p8_config_unit_check.js` | all passed (count assertions now measured, not hardcoded) |
| `node .audit/p9_phantom_scan.js tmp/RyuNET-core/dist/savedata/mfg@asphyxia.db` | 4 MID rows, 4 quarantined |
| `node .audit/p11_taboo_gates.js` | no save-brick; compound hole disclosed |
| WebGL lane `pwsh C:\mfgtmp\run_endgame_asp.ps1` | full chain + core logout (logs `C:\mfgtmp\endgame_asp_proof.log.txt`) |
| `pwsh scripts\ryunet_stage.ps1 -Action status` | bootable diverged copies: 0 |
