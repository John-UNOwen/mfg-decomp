# 17 — Our mock vs the live private provider (`mfg@asphyxia`)

Purpose: audit the accuracy of [docs/15-gget-gpost-command-contract.md](15-gget-gpost-command-contract.md)
and `mock/` against the server that actually runs the game today — the provider's Asphyxia plugin
(`mfg@asphyxia`, TypeScript; `handlers/aog.ts`, `taikyoku.ts`, `mahjong.ts`, `handlers/utils.ts`,
`index.ts`).

Method: line-by-line comparison of the two implementations, plus the recovered client
(`AC/` = `api/recovered/Assembly-CSharp/`) as the tie-breaker on "who is right", plus 900 live
requests captured against our mock in [mock/mock.log.txt](../mock/mock.log.txt).
Citations: THEIRS = `handlers/aog.ts:NNNN` / `taikyoku.ts:NNNN` (relative to
`tmp/RyuNET-core/dist/plugins/mfg@asphyxia/` — the copy AsphyxiaCore actually loads, staged per
docs/18 §"Stage the runtime layout"); OURS = `mock/MatchEngine.cs:NNNN`, `mock/Mahjong.cs:NNNN`,
`mock/Program.cs:NNNN`, `mock/specs/x.xml:NN`. `tmp/asphyxia_plugins/mfg@asphyxia/` is a staging
mirror refreshed *from* the runtime copy (`pwsh scripts/ryunet_stage.ps1 -Action mirror`) and is
byte-identical to it, so no copy of the plugin is edited there; `pwsh scripts/ryunet_stage.ps1
-Action status` lists every copy in the tree and says which match. Before that rule the staging
mirror lagged the runtime copy (aog.ts 3,086 vs 3,111 lines) and pointed a reader at the wrong code.
Anchor for the THEIRS column: the copy on disk now — aog.ts `84D9FA3AA182…` (3,568 lines), utils.ts
`D4B775D0DBE3…` (3,182), index.ts `3583EB3E15A7…` (353), all written 2026-11-01. Every THEIRS range
below was walked against that snapshot on 2026-11-01; 187 that no longer described the code named
were re-anchored, and `taikyoku.ts` / `mahjong.ts` are unchanged since the first pass, so their
cites stand. The pre-takeover baseline older notes quote (`6DD99D10210D…`, 3,111 lines) is off disk.

---

## Verdict

**Our wire format is right; our rule/lifecycle layer is where we drift.** Everything the
recovered client *parses structurally* — `must=` grammar, `cell_sno`/`cell_data_{seq}` envelope,
`pi0..pi3`, kind numbers, per-command child names, `Pai` tile encoding, 13-id deals,
`end_stat` semantics, `calc_score` blocks — is confirmed by theirs, which is serving the
unmodified shipping client right now. Nothing THEIRS does contradicts a structural claim in
docs/15. Deltas are in four classes:

1. **Two real bugs in our mock** — the tsumo-window win bit (`0x80` where the client needs
   `0x40`, mock/MatchEngine.cs:371-372) and renchan on a tenpai exhaustive draw (we rotate the
   dealer, mock/MatchEngine.cs:611,625-638 vs taikyoku.ts:1249-1258). Both are invisible to the
   client's parser but visible in play, and #1 explains why the shipping client never once sent
   `TSUMO_AGARI(3)` in 264 live `gpost`s.
2. **Semantic choices docs/15 left open** — theirs answers 12 of the 16 open questions in §11 and
   partly answers 2 (see *Open questions resolved by theirs*); the ones that still read "the server
   never does that either" are `recon` and `RANNYU`. Mid-match stamps are no longer one of them —
   the count went up because Q12 flipped: a `gpost` answer carries `<chat>` against a per-seat
   cursor (aog.ts:2424-2426) and the client parses it (`AC/MFG.GameRequest/GPost.cs:73-77`).
3. **Depth we deliberately skipped** — theirs is a full game server: riichi (with 供託 accounting
   and a `SCORERANK(24)` cadence), chi/pon/kan incl. chankan and rinshan, multi-ron resolution,
   red-free walls, real han/fu → payment tables, honba/kyoutaku transfers, abortive 九種九牌,
   CPU AI with shanten/danger evaluation, 23 gmodes × seat/tenbo tables, 60 s disconnect→CPU.
   Ours is 4-seat tonnan, no calls, nominal mangan money.
4. **Their panel layer is state-driven, ours is canned** — theirs persists a profile per `refid`,
   serves *every* stored `client_state` kind, and authors the kinds no client can save
   (`friend_game`, `present_box_game`, `music_gacha`, `receive_only`, `notice_game`, the four
   competition records) once per account rather than synthesizing them on every read. My Room,
   costumes, friend-borrow, presents, notices, records and replays all read off that. Ours answers
   2 canned states and ignores every write. A stricter client or any flow beyond quick-play will
   find ours empty.

Three points where the servers disagreed and **ours was the correct one** are now agreed on both
sides, each fixed on theirs: `chara_enabled` answers `is_gacha_enabled` / `end_date` at the document
root, where `AC/MFG.GameRequest/CharaEnabledSend.cs:34-41` reads them (aog.ts:3420-3433 — the
wrapped shape used to disable theirs' own pickup-gacha notice); `competition_entry` answers
root-level `<entry><result>/<is_entry>/<reason>` plus `<displayed_result>`, which is what
`AC/MFG.GameRequest/CompetitionEntry.cs:44-61` parses (aog.ts:3168-3190); and `entry_game` limit
fields are **seconds** — 20/15/20/15/15/30/30/30/60 (aog.ts:188-203) — which is how
`MachingLimitTime` consumes them, one unit per `Time.deltaTime` with a 99 display cap
(`AC/CountDownTimer.cs:238,50`). Ours is still the more faithful side on red fives and on a
`breakdown`-rich `mgresult` (see *Deliberate non-changes*).

---

## Command vocabulary deltas

### Numbers (both sides agree, and both agree with `AC/MFG.Taikyoku.Command/*_COMMAND_TYPE.cs`)

`K_*`: 1 TSUMO, 2 SUTEHAI, 3 TSUMOAGARI, 4 RON, 5 RYUKYOKU, 6 PON, 7 CHI, 8 ANKAN, 9 MINKAN,
10 KAKAN, 14 TYOKO, 15 TSUMOCHOICES, 16 SUTECHOICES, 17 KYOKUSTART, 23 KYOKUEND, 24 SCORERANK
(taikyoku.ts:17-32) == docs/15 §2.1. `S_*`: 2 SUTE_PAI, 3 TSUMO_AGARI, 4 RON_AGARI, 5 PON,
6 CHI, 7 ANKAN, 8 MINKAN, 9 KAKAN, 10 KYUSYUKYUHAI, 11 NAKINASHI, 12 CYOUKOU, 13 KIKEN,
14 RECONNECT, 15 NEXT_KYOKU_READY (taikyoku.ts:35-49) == docs/15 §2.2. THEIRS defines no
`RANNYU(19)` and never emits it; ours neither. Neither side emits `TYOKO(14)` unless the client
sends `CYOUKOU(12)` (taikyoku.ts:1336-1337) — which the recovered client never sends.

`SELECTABLE_TYPE_FLAG` — theirs' constants match the client exactly (taikyoku.ts:52-60:
`F_NONE=0x1, PON=0x2, CHI=0x4, KAN=0x8, TSUMOAGARI=0x40, RON=0x80, KYUSYU=0x100, REACH=0x200,
SUTE=0x400`). **OURS IS WRONG HERE**: we put the RON bit in the *tsumo* window.

| Cell | OURS | THEIRS | Client consequence |
|---|---|---|---|
| TSUMOCHOICES `select` (win possible) | `0x400\|0x80` (mock/MatchEngine.cs:371-372, CPU 0x480 at :381) | `0x400\|0x40` (taikyoku.ts:494-505) | `AC/ExecTsumoChoices.cs:176` gates the tsumo-win analysis on `TSUMOAGARI=0x40`; with `0x80` the client never shows a 和了 button, so `S_TSUMO_AGARI(3)` is never sent. Live proof: 0 × kind 3 in 264 gposts. |
| TSUMOCHOICES extra fields | `select`, `kuikae_pai` (last draw only, :374) | `select`, `ptn_num`+`ptn{i}{sute_pai,machi_num,machi_pai,stat}`, `kan_pai`+`kan_type` (taikyoku.ts:515-539) | we never offer riichi or kan; `ptn_num=0` is the documented way to strip `REACH` from `select` (docs/15 §3.4) |
| SUTECHOICES `select` | `0x81` (RON\|NONE) (:394,440) | `F_RON`/`F_PON`/`F_CHI`/`F_KAN` bitmask (taikyoku.ts:681-695) | same bit for the offer we support; ours has no call offers |
| SUTECHOICES `naki` | always `0` (`SuteChoicesBody`, :680-685) | mirrors the PON/CHI/KAN bits (taikyoku.ts:684-695) | client parses it and ignores it (`ChoicesData.cs:110-120`) — informational only |
| SUTECHOICES `pindex` | **addressed seat** (:682) | **discarder** (taikyoku.ts:699) | client ignores the field; theirs is the meaningful reading |
| SUTECHOICES patterns | none | `chi_pai` (≤6 patterns), `pon_pai` (1), `kan_pai`+`kan_type=2` (taikyoku.ts:701-714) | client caps: 12 chi / 2 pon / 4 kan (docs/15 §3.4) |
| `kan_type` numbering | n/a | `1`=ankan, `3`=kakan in TSUMOCHOICES (taikyoku.ts:472,477); `2`=minkan in SUTECHOICES (:713) | client acts only on 1/3 in the tsumo chooser (`AC/ExecTsumoChoices.cs:952-968`); for minkan the client answers `MINKAN(8)` from the offer, never from `kan_pai` — so `2` is a server-side label only |

### Cells we never emit that theirs emits in every match

| Cell | THEIRS | OURS |
|---|---|---|
| `SCORERANK(24)` | emitted on every riichi declaration (taikyoku.ts:613-614) **and** once per kyoku end, immediately before `KYOKUEND` (:1254); body = `kyoutaku` + `riti_after{0..3}{score,rank}` (:623-631) | never emitted (no riichi, no mid-kyoku sync) — `mock/MatchEngine.cs` has no kind 24 |
| `PON(6)/CHI(7)/MINKAN(9)/ANKAN(8)/KAKAN(10)` | full set with `sute_pindex` and `pon_pai`/`chi_pai` pairs (taikyoku.ts:773-878) | never (calls unsupported) |
| `RON(4)` | `ron_flg` array supports several simultaneous winners, `yaku{0..3}` filled for every seat (:1192-1206) | single-winner-ish (`ron_flg` from mask, mock/MatchEngine.cs:545-565) |
| `TSUMOCHOICES` on CPU seats | **absent** — CPU seats get no choice cell at all (taikyoku.ts:411-415) | emitted to every seat (`Append(match, 15, 1<<seat, …)`, mock/MatchEngine.cs:381) |
| `available="0"` window | used whenever the client's cursor is already at the tip; `<cell_info available="0"/>` with **no `cell_sno`** (taikyoku.ts:279-292) | never; ours always `available="1"` + `count="0"` and echoes the client's cursor as `start` (mock/MatchEngine.cs:652-676) |

### Cells/fields where OURS adds or omits vs THEIRS

- **`__count` attribute on every int-array tag** — theirs emits `<dora __count="5">…</dora>`,
  `<machi_pai __count="1">0</machi_pai>`, and substitutes a literal `0` for empty arrays
  (taikyoku.ts:64-68). Ours sends no attribute (mock/Mahjong.cs:242 `JoinPais`) but the same
  `0` filler. Client reads only the text (`ValueArrayInt`), so both parse; `__count` is almost
  certainly the Konami serializer's artifact, and the safest mock copies it.
- **`KYOKUSTART` field values** (docs/15 §7.1): `rencyan` — theirs hardcodes `0`
  (taikyoku.ts:323), ours sends a real streak counter (:339). `chicya` 0 both. `sai` — theirs
  sends two random 1-6 (`_ints("sai", [randint(1,6)×2])`, :318), ours hardcodes `1 1` (:337).
  `ba`/`kyoku`/`all_last` — theirs derives from a flat `kyoku_index` (`ba = idx<seats?0:1`,
  `kyoku = idx%seats`, `all_last = idx>=total-1`, taikyoku.ts:222-236), ours hardcodes `ba=0` and
  `all_last = kyoku==3` (:337-338): equivalent for tonnan, wrong for hanchan.
  `kyoutaku` — theirs dynamic (riichi 供託), ours always `0` (:339).
  `nokori`/`yama_cnt`/`yama`/`rinshan` — both send the real thing (theirs 70 live tiles for 4p,
  taikyoku.ts:312,325,329-331; ours 70, mock/MatchEngine.cs:339-342). `dora`/`ura_dora` 5 ids
  each on both, `dora_open` 1 at start on both.
- **`player_info{i}` for unused seats** — theirs fills `tepai` with 13 copies of Pai 1,
  `score 0`, `rank i`, `jikaze i` for `i >= seats` (taikyoku.ts:344-349) so sanma/nima still pass
  the client's 4-seat `MustNeed` loop; ours is 4-seat only, so it never hits this. Any sanma mock
  must copy theirs' filler (docs/15 §1.8 hazard 9).
- **SUTEHAI `stat`** — identical bit usage (`riichi|1`, `tsumogiri|2`; taikyoku.ts:611 vs
  mock/MatchEngine.cs:412,426). Theirs never sets bits 2-4 (fire/open/atomic riichi) because it
  implements none of them; ours has no riichi at all.
- **Tile encoding / red fives** — ours deals 3 red fives as `plain+64`
  (mock/Mahjong.cs:123-139) and the live client echoed `69/79/89` in real discards; theirs
  **never emits a `+64` id**: `build_wall` is 4 copies of each of the 34 kinds
  (mahjong.ts:137-144), and `pai_to_idx` merely tolerates inbound `+64` via `pai_norm`
  (mahjong.ts:19-31). Ours matches the client's enum + art layer better; theirs' engine has no
  akadora, so its `dora_num`/`han` under-reports real payments and its `Kirisame`-event hooks are
  dead. Both agree the wire id is `Pai`, never `HaiID` (docs/15 §8).
- **`yaku_han` is a tier code, not the han count.** THEIRS sends `res.rank` from
  `han_rank(total_han, fu, yakuman)` → 1-4 for 1-4 han, `5`=mangan, `6`=haneman, `7`=baiman,
  `8`=sanbaiman, `9`=yakuman, `9+n`=n-yakuman (mahjong.ts:1286-1295, used at taikyoku.ts:1097-1104);
  the client casts it straight to `MFG.Types.Han` (`AC/MFG.Types/Han.cs:3-21`,
  `AC/MFG.Taikyoku.Command.Receive/YakuData.cs:31`, consumed at `AC/Agari.cs:713,760`,
  `AC/ExecAgari.cs:133`). OURS sends the raw han (`<yaku_han>1+dora</yaku_han>`,
  mock/MatchEngine.cs:697), which is only accidentally right below mangan. `fu_num`: theirs
  computed, ours hardcoded 30 (:697). `wherefore`: both `0` (server-computed money wins).
- **Settlement money decomposition.** THEIRS: `kyotaku_score = 1000×供託` to the winner,
  `tumifu_score = 100×honba×(seats-1)` (tsumo) / `300×honba` (ron), `yaku_score` = table payments
  from `payments(taku, rank, fu, is_oya, is_tsumo)` with dealer-weighted splits,
  `new_score = before+yaku+kyotaku+tumifu` (taikyoku.ts:1117-1135,1137-1208; mahjong.ts:1321-1344).
  Riichi costs 1000 at declaration and increments 供託 (:606-607), which is then paid out at
  settlement. OURS: `yaku_score` = flat delta, `kyotaku_score`/`tumifu_score` always `0`
  (mock/MatchEngine.cs:717-730), nominal mangan only (12000 ron / 8000-dealer-or-4000 tsumo,
  :511,541) and **no honba bonus at all**, so a 1-本場 agari pays the same as 本場 0.
- **Tenpai payments agree** (both real): theirs `gain=floor(3000/n)`, `loss=floor(3000/(seats-n))`
  (taikyoku.ts:1226-1233); ours `+1000×(4-t)/-1000×t` (mock/MatchEngine.cs:596-599) — same numbers
  at 4 seats, ours fixed to 4 (theirs generalizes to 3/2 seats).
- **Ranks are 0-based on both** (theirs `_ranks()` taikyoku.ts:363-375 ties toward the dealer-
  nearer seat; ours `RankOf` mock/MatchEngine.cs:747-754 ties toward the lower seat index), which
  matches the client (`ScorePanel`/`Rank` enum; docs/15 §11 Q15). Tie-break differs slightly
  (theirs is dealer-relative, ours absolute index) — cosmetic, but pick theirs if you want the
  same bracket as the live service.
- **`gpost` response body**: THEIRS returns the new cells plus a `chat` block filtered by its own
  per-seat cursor (`<game>{cells}</game><chat>…</chat>`, aog.ts:2413-2427; cursor utils.ts:1017-1035)
  — **no `mwait`**. OURS re-states the full roster `mwait` in every gpost ack
  (mock/MatchEngine.cs:265-268). Both parse; ours is the noisier one. Theirs no longer suppresses
  `chat` with `since=1e9` the way the first pass did, so mid-match stamps do arrive in `gpost`
  answers — the only poll the client keeps making during play.
- **Root/envelope**: THEIRS `<?xml …?>\n<root><serv_st><code>0</code></serv_st>…`
  (utils.ts:91-106; a rejection uses the same envelope with a non-zero code, utils.ts:107-111);
  OURS `<res><serv_st><code>0</code></serv_st><code>0</code>…`
  (mock/MatchEngine.cs:777). Root name is irrelevant and the root-level `<code>` is inert
  (`AC/MFG.GameRequest/RequestApiBase.cs:146,162-178`). Ours also ships the spec files' `<!-- -->`
  comments in responses (mock/Program.cs:285-317); theirs ships none. Cosmetic.
- **`must=` slot usage**: theirs reads tid(3), kind(7), pindex(4), pai(10), tepai_id(11),
  tepai_id2(12), reach(13), tsumogiri(14) and **ignores the seq slot (6), tepai_pos(8) and
  chi_naki(15)** of a `gpost` must (aog.ts:2386-2396 via `mustInt`, utils.ts:130-137) — slot 6 is
  not nothing, it is `gget`'s `next_sno` (aog.ts:2258-2262), which is how the same index looks ignored
  from the match side. The tid is not ignored any more:
  `addressedSeat` (aog.ts:360-374) requires it to be the table its own seat record names, and a
  claimed `pindex` that differs from the server's seat is logged and overridden (:2414-2416).
  Ours reads seq(6, logged only), kind, pai, tsumogiri
  (mock/MatchEngine.cs:197-201) and likewise validates nothing. Consequence for docs/15 §5.5: the
  seq binding of claim answers (`max(SUTECHOICES.seq)+1`) is a **client-side invariant that
  neither server enforces**; and `tepai_pos`/`tepai_id`/`tepai_id2` are used by theirs only for
  pon/chi tile identity (`pai_to_idx`, taikyoku.ts:1421,1438), with a fallback to
  `[tile, tile]` / first legal option when the client's self-report is nonsense (:1422,1443-1445).

---

## Match lifecycle deltas

### Endpoint order (live capture, ours) vs theirs' requirements

Live guest loop (900 requests, `mock/mock.log.txt`): `appli_info` → `create_player` → `login` →
`get_menudata` → `mission_date` → `gacha_info` → `client_state_read` → `player_record` →
`client_state_write` ×2 → `entry_game` → `gget(ready=0)` ×2 (60-70 ms apart) → `gchat` ×17-21 at
1.00 Hz (`scene=MATCHING` only) → `gget(ready=1)` (carries `KYOKUSTART`) → **`gpost`-only for the
whole match** → `end_game` → sometimes `player_record`. `appli_boot`, `keep_alive`, `reconnect`,
`kiken_game`, any mid-match `gget`/`gchat`: 0 hits.

Their server requires exactly this order too, but the *matching phase is slower*: a lobby fills
only when `seats` humans have entered, otherwise it is CPU-filled **on a `gget` that arrives more
than `LOBBY_FORCE_SECONDS` (60 — the same window `entry_game` advertises as `last_time`) after the
lobby was created** (aog.ts:205-207, 2276-2283). Since `MatchingFlow` keeps `Fetch()`ing `gget`
while `mwait/status == 0` (`AC/MatchingFlow.cs:243-246`), a solo guest does get a table — after
~60 s, not instantly like ours (poll 2, mock/MatchEngine.cs:153,171-188). A 2-seat (Nima) `gmode`
with no passphrase is short-circuited to an instant private table, while a passphrase lobby keeps
pairing humans for private Versus (aog.ts:2143-2152, 2112). A `gget` whose tid matches neither a
table nor its lobby is answered `serv_st 770` rather than an invented seat (aog.ts:2285-2292).

`gget` while unseated: THEIRS still emits `mwait` with `status 0`, `pnum = humans in lobby`,
`cpu_num 0` and a `mend` containing exactly `player_0..pnum-1`, plus `epdata_{0..pnum-1}`
(name+mid) and **always a `<mend>` tag**; `all_ready 0`; no `taikyoku` at all unless
`ready=1 && table exists` (`matchingXml` aog.ts:270-326, handler :2337-2382). Matched, the same
helper switches to `cpu_num = seats - pnum` and pads `mend` to `seats` children with `ptype="3"`
CPU rows (`matchingPlayerCpu` :161-168), and announces
`<g2event><e><id>1</id></e></g2event>` only when it designated the table a scramble match
(:309-314). OURS: `status 0` on poll 1 but with
`cpu_num 3` already, `mend` only from poll 2, `all_ready 1` always (mock/MatchEngine.cs:164,171-188),
no `epdata_N`. The client's `MatchingData.Parse` sets `PlayerNum = pnum + cpu_num` and then loops
`i < PlayerNum` over `mend` children dereferenced unguarded (`AC/MFG.Taikyoku/MatchingData.cs:33-35,67-86`
→ `AC/MFG.Taikyoku/PlayerInfo.cs:66-70`), so **`mend` must contain exactly `pnum+cpu_num` players**:
theirs is self-consistent by construction; ours is consistent only because we always send 4
children whenever we send `mend`. Emitting `cpu_num 3` with no `mend` is safe (element absent ⇒
loop skipped), but emitting `mend` with fewer than `pnum+cpu_num` children is an NRE risk —
worth pinning in docs/15 §1.5. `epdata_{i}` is genuinely parsed (`MatchingData.cs:52-60`) — ours
omits it, so other humans' identities never populate in our roster path.

### Kyoku start / turn windows / who owns the clock

- **No turn-window timers exist on either side.** THEIRS has no `must`/`wait`/`timeup` concept in
  the match layer; CPU seats act inside the same request that drew them (taikyoku.ts:401-416), and
  the only deadlines a client sees are the `entry_game` limit fields, which are display input
  (`AC/CountDownTimer.cs` countdown) — not enforcement. What theirs does carry is two wall-clock
  timers in the matching layer, both 60 s: the lobby CPU-fill window (aog.ts:205-207, 2276-2283) and the
  disconnect→CPU sweep (aog.ts:2293-2335). Neither acts inside a kyoku. Ours: same (mock/MatchEngine.cs never
  times out a window).
- **Turn window ownership**: THEIRS decides *what* the seat may do (`select` from real
  shanten/tenpai/kan/九種九牌 analysis, taikyoku.ts:492-513) and then blocks until the client
  answers. It does **not** auto-answer a stalled human; the client's own 5 s/20 s think clock
  (docs/15 §5.6) is what eventually produces `SUTE_PAI`/`NAKINASHI`. Ours identical.
- **The claim barrier differs and theirs is stricter**: THEIRS waits for **every** offered seat to
  answer (`sute_replies` count == `sute_choices` count, taikyoku.ts:1367-1375) before resolving,
  then resolves in real priority order: all `RON_AGARI` seats simultaneously → `PON`/`MINKAN` →
  `CHI` → decline (`_resolve_sute_choices`, :1377-1457). CPU seats never hold the barrier: they
  are resolved server-side in `_cpu_calls` after the humans reply (:728-760). OURS offers one
  window to seat 0 and resolves on that one answer (mock/MatchEngine.cs:226-244). If a 4-human
  table is ever built, ours must adopt theirs' all-replies barrier or a passed seat will hang
  the table.
- **Chankan**: THEIRS offers `SUTECHOICES(select=RON, chankan)` to every human after a `KAKAN`
  and resumes the kanner's rinshan draw on declines (taikyoku.ts:834-878,762-764). Ours has no kan.
- **Kan bookkeeping**: theirs caps kans at 4 and opens one more dora pair per kan, capped at 5
  pairs (`kan_count`, `dora_open = min(5, +1)`, :815-816,827-828,844-845) — exactly what the
  client's 5-element `dora`/`ura_dora` requirement implies (docs/15 §1.8). Ours pins `DoraOpen=1`
  forever (mock/MatchEngine.cs:60).
- **CPU seat behavior**: THEIRS CPU = full AI: tsumo win check → ankan/kakan evaluation →
  discard choice by `shanten×120 − ukeire×1.5 − handValue×5 + keep(dora/yakuhai)×6 + danger×weight`
  with riichi declaration at tenpai (:901-989), call willingness by shanten improvement +
  yakuhai/all-sequences heuristics (:996-1038), CPU→CPU ron priority before pon/chi (:728-750).
  Ours: CPU tsumogiri-discards its draw and wins whenever the hand is a win
  (mock/MatchEngine.cs:368-403) — no melds, no AI, so our CPU hands are structurally weaker and
  our kyoku are shorter than theirs.

### Agari settlement and kyoku scores

| Aspect | THEIRS | OURS |
|---|---|---|
| tsumo payment | `payments()` → base×(4·ko + 2·oya etc.), dealer-weighted, rounded up to 100 (taikyoku.ts:1143-1155) | flat: dealer-win 8000 each, else dealer 8000 + others 4000 (mock/MatchEngine.cs:507-514) |
| ron payment | `total` from rank/fu, discarder pays, +`300×honba` transfer (taikyoku.ts:1176-1190) | flat 12000 per winner from the discarder (:540-543) |
| 供託 | −1000 at riichi, `kyoutaku++`; winner takes `1000×kyoutaku` (first winner on multi-ron), 供託 zeroed (:606-607,1154,1185) | none (no riichi) |
| honba | `tumifu_score` = 100/seat (tsumo) or 300 (ron) × honba (:1149-1153,1181-1182) | always 0 (mock/MatchEngine.cs:717-730) |
| ura-dora | `dora_open` at settlement = 1 + kans, ura counted only when riichi (mahjong.ts:1274-1283) | `dora_open` fixed 1, ura never counted (mock/Mahjong.cs:230-240 via `Take(DoraOpen)`) |
| yakuman | full evaluator incl. tenhou/chiho/rinshan/haitei/houtei/chankan/ippatsu/double-riichi + kazoeakuman bit (:1274-1373, taikyoku.ts:550-571) | nominal mangan + optional haitei bit only (mock/MatchEngine.cs:689-701) |
| bust | any seat `< 0` ends the **match** immediately (taikyoku.ts:1260-1262) | no bust rule; ours starts at 30000 so it is rarely reachable |
| rank recomputation | `_ranks()` after every settlement and after each riichi (:623-631,1254) | ours sends ranks in `player_info`/`mgresult` only (mock/MatchEngine.cs:328-334,756-773) |

### Kyoku count, renchan, and `end_stat` (docs/15 §11 Q3 / Q16)

THEIRS, in one block: `renchan = abortive || tenpai.includes(oya)` (taikyoku.ts:1249); on
renchan `honba += 1` and `advance_kyoku = false` → `kyoku_index` is **not** incremented, so
`oya = kyoku_index % seats`, `kyoku = kyoku_index % seats`, `ba` and `all_last` are all unchanged
(:222-236,1253-1277); on a non-renchan draw/loss `honba = 0` and the index advances.
`game_over = busted || (kyoku_index >= KYOKU_COUNT[taku] − 1 && !renchan)` (:1259-1262), and only
then is `KYOKUEND.end_stat = 1`. `KYOKU_COUNT` = tonnan 4, hanchan 8, sanma 3, nima 2
(mahjong.ts:74-79); `SEATS_OF` 4/4/3/2 (:67-72); start score 25000/25000/35000/35000 (:81-86).

Therefore, for a **tonnan guest match**: 4 kyoku, extended past east-4 by any renchan (dealer agari,
or dealer-tenpai exhaustive draw) — and `end_stat=1` is emitted only on the kyoku where the
condition finally fails. Abortive draws (九種九牌, `S_KYUSYUKYUHAI` → `_ryuukyoku(true)`) force
renchan with no tenpai payments (:1210-1251,1331-1333).

OURS differs on both halves (mock/MatchEngine.cs:611,619-638): a tenpai exhaustive draw of **any**
seat increments `Honba` **and** rotates `DealerSeat` and advances `KyokuCompleted` — i.e. we grant
honba without renchan, and we deny renchan to a tenpai dealer; a non-tenpai draw correctly resets
honba and rotates. Agari renchan (dealer wins) is right in ours. Our match-end cap is
`KyokuCompleted >= 4 || RenchanCount >= 4` (:629,637), which caps a renchan streak at 4 and gives
a different east-4-extended shape than theirs.

`KYOKUEND.end_stat` meaning is confirmed the same on both: `1` = match over (client leaves the
scene and calls `end_game`), `0` = next kyoku (docs/15 §11 Q3 already settled live; theirs'
`_end_kyoku` and our `FinishKyoku` both emit exactly that).

`RYUKYOKU.ryukyoku_status{i}.end_stat` = "that seat was in tenpai" on both
(taikyoku.ts:1238-1246 vs mock/MatchEngine.cs:603-608), with `machi_pai` = waiting ids or the
`0` filler. Note both servers judge the last drawer's tenpai on 13 tiles *after* removing the
final draw (taikyoku.ts:1216-1225 skips seats whose count %3 ≠ 1; mock/MatchEngine.cs:581-587
removes `LastDrawPai` explicitly). Consistent.

### Inter-kyoku latch and match end

THEIRS: `S_NEXT_KYOKU_READY` records the seat and starts the next kyoku once
`next_kyoku_ready.size === humans.length` (taikyoku.ts:1338-1342) — with 1 human + 3 CPU that is
the human's single gpost (CPU readiness is implicit, and the humans set shrinks when a seat is
converted), with 4 humans it needs **all four**. THEIRS answers that gpost with the whole next
`KYOKUSTART` in the same response (aog.ts:2413-2427 + taikyoku.ts:279-292) — exactly what our
capture shows the client accepting (~2.9 KB on the inter-kyoku hop, `mock/mock.log.txt`
seq=37, bytes=2931). OURS: same single-answer latch (mock/MatchEngine.cs:253-257). So docs/15
§11 Q4's "does the server need all four" answers as: *needed for human seats only*.

Resign: `end_game` and `kiken_game` share one handler (`handle_end_or_kiken` aog.ts:2790) because
`EndGame.cs:21` and `KikenGame.cs:21` post the same fields, so what separates them is the route:
`kiken_game: (f, c) => handle_end_or_kiken(f, c, true)` (aog.ts:3744). They used to share an outcome
as well — `S_KIKEN` set `state="game_end"` and `finished`, and the settlement ranked the quitting
seat by standing, so a dealer who quit before any scoring came home 1st with +20000 uma. Both sides
now do what the original game does with 棄和 (DanLevelMaster.cs:1336 is the client branch that makes
the *last* rank the one a quit costs): `Table.resign(seat)` records the seat, drops it from `humans`
and hands it to the CPU for the rest of the match (taikyoku.ts:253, dispatch :1382), the table stays
open for the seats that stayed, and `result_rows()` settles resigned seats behind them whatever their
points say, so the quitter takes the last rank and its uma (taikyoku.ts:1503, :1515). Only a match
the engine played out is archived: the engine stamps `completed` at its last kyoku (taikyoku.ts:1306)
and the handler archives on that rather than on `finished` (aog.ts:2814, :2838), so a resign
mid-match writes no `match_archive` row and does not burn the latch for the seats still playing. The
quitting session's seat record is released — a re-login, even a fresh session on the same account,
cannot take the seat back — with its account kept in `table.seat_quit` so the archive still names who
sat there (`kiken: 1`). Both still demand the tid they address: a request whose tid is not the
caller's table is answered `serv_st 770`. OURS: the mock stops the match (mock/MatchEngine.cs:247)
because it has no second live seat to play on for, and mirrors the settlement through `SettleRanks`
(mock/MatchEngine.cs:765), asserted by `MockServer selftest`; its `g2result` in kiken
(mock/specs/kiken_game.xml) is still ours alone, sent by theirs only when the match was
scramble-designated.

Disconnect / kiken / reconnect: THEIRS' only liveness mechanism is in `handle_gget`: a human seat
whose `lastSeen` — bumped by any request that addresses its table (`addressedSeat`, aog.ts:360-374) —
is > 60 s stale is removed from `humans`, and if that seat was mid-discard the CPU discards for it,
or mid-claim its offer is dropped and CPU calls are resolved (aog.ts:2293-2335). **This never fires
mid-match for the shipping client, which does not `gget` during play** (0 mid-match ggets in the
capture) — so theirs can stall a table on a dead human until someone else polls. Reconnect: THEIRS
implements only the standalone `/reconnect` endpoint, returning the same `entry` body `entry_game`
builds (`handle_reconnect` aog.ts:2238-2252 → `entryXml` :222-240, with `<gmode>` and an absolute
`gserv_url`) after re-adding the seat to `humans` to take control back from the CPU; a pcuid with no
seat behind it falls through to `handle_entry_game` and gets a fresh table. It **never emits `recon`**
in a `gpost` answer and does not handle `S_RECONNECT(14)` at all (taikyoku.ts:1289-1347 dispatch) —
so docs/15 §6.6's production reconnect path is implemented by *neither* server, and their
`reconnect` response shape (`next_sno 0`, full `entry`) is a table-rejoin, not a snapshot. What
changed is reachability: the plugin now pulls the endpoint name out of the path by scanning
segments longest-first (aog.ts:3454-3480), where the core's last-segment rule read the mandatory
trailing slash as an empty name and never saw a mid-path `reconnect` at all. `next_sno 0` is on both
sides' `entry_game` too (aog.ts:231, mock/specs/entry_game.xml:13), so a mid-match re-entry replays
the whole cell log — legal for the client (it re-applies the log) but heavy.

`mgresult` / final delivery: THEIRS emits `gmode` and `taku_class` from **the table's own record**
(stamped when `entry_game` created it, aog.ts:176-186, 328-339 — no longer read off the posted
`gmode` param, which `end_game` does not even send) + `continue_state 0` + `continue_fee 0` +
`player_i{rank,score,uma}` for i < seats, where `i` is the **seat** and `uma` is indexed by
**rank** from a per-seat-count table (4p 20000/10000/−10000/−20000, 3p 20000/0/−20000,
2p 10000/−10000) (aog.ts:246-268; taikyoku.ts:1460-1474). A scramble-designated table also carries
`<g2result><r>` with the school and the win flag (aog.ts:259-266). No `breakdown`, no
`old`/`new`, no exp/affinity. The client tolerates that completely: every child of
`mgresult` is `NeedNot`/null-checked, including `breakdown`, `battle_item_use_info` and
`player_{0..3}` (`AC/MFG.GameData/MahjongResultInfo.cs:75-186`). OURS sends `breakdown` exp +
affinity and a `g2result` in kiken (mock/MatchEngine.cs:756-773, mock/specs/end_game.xml,
kiken_game.xml) with tonnan uma `{30,20,10,−60}` — parse-safe either way, but the uma **numbers**
differ, and ours has no 3-seat/2-seat uma table.

---

## Panel endpoint deltas

Non-match endpoints: OURS = canned `mock/specs/*.xml` served by name-matching in
`mock/Program.cs:217-239` (`/aog/x`, bare `/x`, any path segment, longest substring, `_default`);
THEIRS = `AOG_HANDLER_MAP` (aog.ts:3285-3434) registered name-by-name through `R.AogRoute`
(`registerAogRoutes`, aog.ts:3541-3568) plus the `R.AogUnhandled` fallback (:3489-3533). The core
middleware still keys on the **last path segment** (`src/middlewares/AogMiddleware.ts:13-18`), and
both the fallback and the plugin's own `extractAogName` (:3454-3480) exist to undo that: doubled
slashes collapsed, the `/aog` prefix — including one glued to a `gserv_url` — stripped, exact
segment match before a substring match, longest registered name first so `gget_stamp_info` is never
read as `gget`. A second native listener on `mfg_http_port` 22421 (index.ts:244-340) runs the same
extractor. Anything left over is answered `<root><serv_st><code>0</code></serv_st></root>`, through
the fallback (aog.ts:3521-3524) and at the tail of every native request (index.ts:323-328) — the
same contract as our `_default.xml`, and now guaranteed: the first pass could leave a request
unanswered entirely, because the fallback was written to `R.AogUnhandled(ctx)` while the core calls
`fb(form, ctx)`. The invented aliases `get_record` and `coop_done` are no longer in the plugin's
map, so they fall to that empty success on theirs — the core's own 45-name `KNOWN_AOG` list still
admits them (`src/middlewares/AogMiddleware.ts:20-26`), which is exactly our `_default.xml`.

Both sides agree on HTTP 200 `text/xml; charset=utf-8`. `serv_st/code = 0` is still the answer to
everything that succeeds; theirs now also answers non-zero codes — 770 (session gone / no such
table), 803 (state payload rejected), 930 (gacha ticket already spent) — on a well-formed envelope
(utils.ts:66-81), where the first pass had no way to say "no" at all. Deltas that matter for a
stricter client or a fuller flow:

| Endpoint | THEIRS (verified) | OURS | Why it matters |
|---|---|---|---|
| `login` | `<auth><session_id>` = 32 random hex, **no `user_id` echo**; refid = uppercased `user_id\|dataid\|refid\|ref_id`; profile created as `GUEST` (name owned by `create_player`); the pcuid the previous login issued is tombstoned onto the profile document, so a client still holding it is answered `serv_st 770` and takes its re-login path; one dated auto-apply present per JST day (`PresentID 91000000+YYMMDD`, aog.ts:1130), granted and receipted in the same write (aog.ts:1024-1152) | `session_id = mock_pcuid_<user_id>` + `<user_id>` echo (mock/specs/login.xml:8-11) | The client reads only `auth>session_id` (`AC/MFG.GameRequest/Login.cs:30-31`), so ours is protocol-legal; but theirs is where identity, saves and daily bonus actually live. Ours is stateless: every later call re-derives from the posted `pcuid` string. Re-login on theirs used to strand the first client mid-flow on an answer it could not act on. |
| `create_player` | ack only; stores `name` from `name\|player_name`, resolves refid via **pcuid session** when `user_id` absent, mirrors name into `states.player_game.PlayerName`, binds card if any (aog.ts:1175-1287) | ack only (mock/specs/create_player.xml) | Ordering: THEIRS requires `login` before `create_player` (session→refid resolution, :1196-1209) or it forks a random refid (:1210-1211). Our capture order (`create_player` **then** `login`) still works against theirs because both carry `user_id`. |
| `get_menudata` | `mid` = profile `player_id`; name = `mpdata>name`; **23 playable modes** as `<playmode_list><mode>`; **`battle_item_settings`** with `<setting gmode taku_class>` per mode; a pcuid with no live session is **refused** (`serv_st 770`) instead of answering mid `1` / `GUEST` (aog.ts:1288-1316; list built :471-491) | mid 1000001 + one `<playmode>`, no `battle_item_settings`, no `recon` (mock/specs/get_menudata.xml:10-28) | Theirs is fail-closed on identity now: the `mid 1` placeholder is gone from this endpoint (mid 1 is a legal mid for a real account, so stamping it onto a session was a way of reading someone else's panel). The client calls menudata with `enableErrorScreenTransition:false`, so a dead session re-logs in instead of playing as GUEST. Client iterates the mode list name-agnostically (`AC/MFG.GameRequest/GetMenuData.cs:73-82`) but *does* read `battle_item_settings` (:83-87). One mode = our mock can't be used to test sanma/nima/event gmodes. |
| `client_state_read` | session-only identity: a pcuid with no account behind it is answered `serv_st 770`, and no posted `mid` can name one (aog.ts:1326-1333; utils.ts:743-776). Every stored kind, control-chars stripped; `player_game` healed (chara lists int-normalized; `GachaCharaUsableList` left exactly as stored — the unlock proof against cutins 176…493 is a log now, and unlocks are granted where the draw hands the unlock item out, aog.ts:1386-1475, 697-739); `cc_request` sanitized/quarantined (:1487-1506); no `others_event` special case any more, it comes back as the client wrote it; the kinds no client call site writes are authored **once and then kept** — `present_box_game` (welcome 9001-9003 plus the dated daily bonus, receipts persisted by `present_done`), `music_gacha` (:794-833), `friend_game` (seeded once; the stored document is then served as saved with only `IsUsable` / name / dan / chara re-checked against live profiles, :1686-1790), `receive_only`, `notice_game` and the four competition records (:835-854; utils.ts:2684-2709); honors `one_kind` (:1323-1526). A brand-new account is served **10 kinds** — `friend_game`, `music_gacha`, `notice_game`, `player_item`, `present_box_game`, `receive_only`, `competition_game`, `competition_latest_result`, `shop_competition_game`, `shop_competition_latest_result` (`tmp/checkprobe/fresh_kinds.py`); after a full write sweep, **30** (`tmp/checkprobe/kinds.mjs`) | exactly 2 canned states (`version_game`, `player_game` w/ `CharaData:[]`), no `one_kind` (mock/specs/client_state_read.xml:10-11) | Biggest panel gap. Ours never returns `customize_item`, `player_item`, `player_config`, `mission`, `gacha`, `ea_shop`, `log`, `music_gacha`, `friend_game`, `present_box_game`, `cc_request`…, so My Room/costumes/missions/presents/friend-borrow have nothing to load. Theirs still seeds no `version_game`, so a profile only gets one from a write, but a fresh account is no longer the zero-`<state>` read the first pass produced — ours pins `SaveDataVersion 68`, which is the safer shape but a difference. |
| `client_state_write` | ack on success; session-only identity — the posted `mid` is a claim, not a route: a mismatch is logged and the payload lands on the session's own profile (aog.ts:1899-1913); tries 4 base64 decodes (`+`-as-space / raw+ / raw / plain, first valid JSON wins, :1921-1951); **an unparsable payload answers `serv_st 803`, and so does an empty one** (:1915-1919, :1952-1970); the `MID`+digits orphan fork is **gone** — phantoms (`PHANTOM_REFID_RE`, `MID` + 8-or-more digits, utils.ts:571) are recognised and are never a write target (utils.ts:481-489, 531-538; every MID row in the deployment is MID + 8 digits — the wider family is what the pre-fix build could still mint, `.audit/p9-mid-phantom-report.md`); `cc_request` quarantined with the last-good copy kept (aog.ts:1974-1997); `PlayerName`→`profile.name` mirrored on **every** `player_game` write, with pending unlock grants merged into the same document (:2012-2030); `music_gacha` and `receive_only` writes refused as server-owned (:2032-2055) | ack, `data` ignored entirely (mock/specs/client_state_write.xml) | Live writes are ~26.5 KB of base64 with URL-encoded padding (`…%3d%3d`) — the client sends the full PlayerGameData on every launch (2× per launch, `kind=player_game` only). Theirs' 4-variant decode is the lesson: **`+` in urlencoded base64 is ambiguous**; ours never touches it, so we've never hit it. If we ever store states, copy their variant order — and their order of refusal: no profile, then unparsable payload, then server-owned kind. |
| `mission_date` | `info_data kind="missions"` with **all 103 `Mission/Category` rows**, **7 active** (`JudgmentDaily,JudgmentWeekly,Player,Character,Weekly,Yaku,StartDash`), begin 2020-01-01 / end 2099-12-31, `param:""`; the rows come from `data/mission_categories.json`, generated from the client enum (`scripts/gen_mission_categories.py`), and `VFG_MISSION_ACTIVE` (`all` / `none` / a name list) overrides the per-row flags (aog.ts:2971-2973; utils.ts:2827-2860) | 1 category (`Player`) in a 2026 window with an `item_expire` param (mock/specs/mission_date.xml:9) | 1 row vs 103, 7 of them active. Serving the whole enum with 96 inactive costs nothing at parse time, and Category 0 (`None`) is included because `MissionStatus.OnAfterDeserialize` skips it anyway. The redundant int `Category` key theirs sends is discarded by the client (`Enum.TryParse(category)` wins). |
| `appli_boot` | same nodes; `moserv_url = http://<request host>:<CONFIG.port\|8083>/aog` (aog.ts:998-1014; utils.ts:24-25, 3143-3174) | Konami stg host + `reviewed_version false` (mock/specs/appli_boot.xml:9-17) | THEIRS still advertises 8083 while its native AOG listener is 22421 (config.ini `port=8083`, `mfg_http_port=22421`; index.ts:244-340) — `moserv_url`/`gserv_url` only mattered to the unused reconnect path, but a client that followed them would hit the wrong port. Ours advertises an unresolvable host that only works via the test proxy. |
| `appli_info` | `expire_seconds 3600`, then `events`, then `pro_stats` (aog.ts:1015-1022). The event list is **the client's own `GameEventType` enumeration — 304 rows**, 117 of them active: 37 `BASE_EVENTS` (incl. `EnableOdekake`, `FavoBonusEvent FavoRatio=2`, `FanBonusEvent FanRatio=2`), `CostumeAddition1..77` — the whole range, so no costume in My Room is hidden by its own unlock event — and the 3 event-taku flags from `VFG_EVENT_TAKU=min` (`FireReach2`, `ComebackTakuEvent2`, `KirisameTakuEvent`); the remaining 187 rows are served inactive in the same 2020→2099 window with an empty param, `VFG_EVENTS_ACTIVE` / `VFG_EVENTS_OFF` override, and ratio params are normalized so a fan/likability event can never arrive as ratio 0 (utils.ts:2979-3137) | `expire_seconds`, `pro_stats`, `events {"list":[]}` (mock/specs/appli_info.xml:9-11) | Kind order is cosmetic (`kind` attr dispatch). Ours' empty list is not neutral: `GameEventStatus` seeds one inactive row per `GameEventType` and every `EventStatus.IsActive(...)` gate then reads false, so we switch off costumes, event taku and odekake on purpose. Theirs now has the opposite problem to design against: 117 active flags is a *policy*, set by config, not a capture of what the provider runs. |
| `gacha_info` / `req_draw_gacha` / `get_gacha_result` / `cutin_gacha_play_*` / `music_gacha_*` | **53 advertised banners** (ids must equal client `GachaSeriesName` ordinals; curated table utils.ts:1813-1871, the 167-row full table behind `VFG_GACHA_ALL` :1872-2041, and a Pickup/Limited banner is advertised only when its own data names a pickup pool, utils.ts:2057-2107), **per-banner `pickup_rate` tables** the draw itself rolls (group × rarity buckets normalized to 1000 per-mil, utils.ts:2330-2395), per-banner `pickup_items` / `limited_pickup_items` / `custom_pickup_items`, `force_end_date`; opaque 16-hex transaction id stashed on the profile (aog.ts:2633-2642); `times` result rows + `<gift>` nodes; draw resolves real OIDs from that banner's own pools (`VFG_GACHA_CHARAS` / `VFG_GACHA_SAFE` / `VFG_GACHA_EXCLUDE` applied to the advertised pools too), `gift_type` 1 for an owned cutin, 2 for a Pickup/Limited pull of an unselectable girl under `gift_config=Gift` and 0 under `Get` (0/1/2 = client `GachaGiftType`, aog.ts:2726-2775), gift points credited once into the server's own `player_item` copy (:2793-2801), unlock cutins grant `GachaCharaUsableList` at the draw (:2786-2792, 697-739), `fight_spirits` = one per drawn item's chara, **server-minted random `request_id`** tracked as a pending draw that `play_apply` answers against (:2776-2848); music play validates the reserve ticket against the profile that reserved it — unknown, replayed or someone else's id is `serv_st 930` — dedupes against the server-owned ledger, `gift 2`, empty `fight_spirits` (aog.ts:2897-2962, `takeMusicReserve` :898-916; utils.ts:2857-2914) | 1 banner (`pickup_rate 70/20/9/1`), fixed numeric txn id, **1 result row regardless of `times`**, fixed OID + `gift 1` on cutin draw, hardcoded request_id 1 (mock/specs/gacha_info.xml, get_gacha_result.xml, cutin_gacha_play_draw.xml) | Row count vs `times` is load-bearing (the client loops `play_count` rows); ours' single row for a 10-pull is a fidelity bug. `gift_type` is the client's `GachaGiftType` now (0 IsNew / 1 DuplicateGift / 2 ConfigGift) and `<gift>` is what `AcquiredGiftCount` trusts (GachaResultInfo.cs:73), so both sides credit the same Present01 total. |
| `player_record` | archive-driven and **scoped to this account's own matches** (`myArchives`, aog.ts:2993-3012): `<total>` **with a `g2_scramble` block**, always emitted so the trophy panel reads a real 0 rather than a missing record; one `<info>` per Yonma/Sanma/Nima actually present, `rank_count` one per seat, `yakuman_num` summed, its own `g2_scramble`, ≤20 `<history>` with `YYYY/MM/DD HH:MM:SS`; empty when the account has no archive (:3019-3074) | populated Yonma block with `<total/>`, one `rank_count`, dash-separated date (mock/specs/player_record.xml:9-27) | Empty-vs-populated is the behavioral difference a real provider has and ours can't fake. `rank_count` is iterated (count is display-only); `<total/>` is safe both ways, and theirs no longer omits it. |
| `get_haifu_list` / `get_haifu_data` | ≤50 rows, **only tables this account actually sat at** (`myArchives` + the participant rule, aog.ts:2993-3012; utils.ts:1094-1131), `gmode`/`taku_class` taken from the archived match, `pindex` = my seat, `end_date` = ms/1000, 4 name/rank/score slots padded with `CPU`, **no `copy_tid`**; data = recorded `cell_data_N` stream with `cell_sno count=N`, `pnum = seats`, **no `assign_cpu`**, newest row for that tid, and a tid the viewer did not play answers empty (:3075-3140); tids come from a persisted counter so they no longer restart after a boot (utils.ts:1044-1093) | one fake table with `copy_tid` and a 1-human/3-CPU stub (`assign_cpu>cpu_0..2`, `pinfo_0`, one `cell_data_0 kind=11`) (mock/specs/get_haifu_list.xml, get_haifu_data.xml:9-31) | THEIRS proves the replay shape: with `pnum == GetPlayerNum(gmode)` the `assign_cpu` block is legitimately absent (our own spec comment says we need it because `pnum < 4`). |
| `gget_stamp_info` | real cursor: tid/pindex/mid from `must` slots 3/4/5, `since` = field 2 of the posted `stamp_info`, posts the new stamp (fields 3/4, taboo-screened first) + random CPU reply, returns `idx > since` (aog.ts:2557-2579) | static one stamp `idx="1"` forever, request ignored (mock/specs/gget_stamp_info.xml:8-14) | Client dedupes by `idx` cursor (`AC/MFG.GameRequest/GGetStampInfo.cs:25`, `AC/MatchingFlow.cs:549-560`), so ours re-serves idx 1 and only the first sticker ever appears. |
| `gchat` | posts the stamp (name + contents screened against `data/taboowords.json` before they enter the room), returns only `idx >` **the server's own per-seat cursor** (`stampCursor` / `stampCursorMark`, utils.ts:1017-1035) with a 40-entry room cap, CPU sticker replies ~45% of posts (aog.ts:379-421, 2525-2555) | one canned `d` per poll with `idx = {{seq}}` (per-request counter) and `contents 0` (mock/specs/gchat.xml:10) | Because ours increments `idx` on every request, the client sees a "new" stamp every poll (`SeqID > _stampPosition`); harmless only because `contents=0` = `ItemID.None`. Theirs used to answer with the whole room (`since 0`) on every poll — that only survived because the client filters by idx; the cursor is server-side now, so a poll is answered with what that seat has not seen. `time` is epoch **millis** on both (`AC/MFG.Taikyoku/StampData.cs:32`). |
| `end_show` | echoes posted `voltage`/`contribute_percent`/`bonus`, `get_point = floor(voltage/10)` (aog.ts:2505-2521) | canned 1200/100/120 (mock/specs/end_show.xml:9-16) | Same nodes; ours ignores the client's numbers. Not exercised live (0 hits). |
| `entry_game` limit fields / `reconnect` | `ste_oya1_limit_time 20`, `ste_limit_time 15`, `ste_reechi1_limit_time 20`, `naki_limit_time 15`, `agari_limit_time 15`, `naki_choice_limit_time 30`, `reechi_choice_limit_time 30`, `last_cyoukou_limit_time 30`, `last_time 60` — **seconds**, named as `ENTRY_LIMIT_SECONDS` (aog.ts:188-203, emitted by `entryXml` :222-240); absolute `gserv_url http://host:port/aog/`; extra `<gmode>` child; `/reconnect` answers the same `entry` for a seat that still exists and re-enters otherwise (:2238-2252) | 5/5/5/5/5/10/10/30/**300**, `gserv_url game1/`, no `gmode` (mock/specs/entry_game.xml:16-25, reconnect.xml:8-26) | The only consumer, `MachingLimitTime` (`AC/MFG.Taikyoku/EntryTakuInfo.cs:63`), feeds a countdown decremented by `Time.deltaTime` with alert at 5 and display cap 99 (`AC/MatchingFlow.cs:223-227`; `AC/CountDownTimer.cs:36,40,50,80-97,238`) — i.e. **seconds**. Theirs' first pass wrote 15000/30000 and pinned its own display at 99; ours' `last_time 300` does the same thing, so ours is the one left to fix. `last_time 60` is also the lobby CPU-fill window, which is now one number used by both (aog.ts:205-207). |
| `chara_enabled`, `competition_entry` | both now root-level, the shape the client parses: `<is_gacha_enabled>` + `<end_date>` outside any wrapper, gated on advertised banners that really have a pickup pool (aog.ts:3420-3433; utils.ts:2065-2082), and `<entry><result>/<is_entry>/<reason>` + `<displayed_result><result>/<is_check_result>` for competition entry (:3168-3190) | root-level nodes on both (mock/specs/chara_enabled.xml:4-9, competition_entry.xml:8-16) | This row used to read "theirs is wrong on both, do not copy it". Both sides match now; the remaining difference is content, not shape — theirs answers `is_entry 0` because it runs no competition, and its `end_date` is `EVENT_END` = `2099/12/31 23:59:59` (utils.ts:3074). |
| `dojo_get_status`/`set_slot`/`gain_soul` | same node set as ours, per-profile dynamic timers; 4 slots / stock max 3 / lesson 300 s (aog.ts:426-458, 2584-2624; utils.ts:3179-3182); an unknown pcuid is answered `serv_st 770` — **no shared `GUEST` dojo** (aog.ts:459-491) | same constants, fixed unix stamps, slot 0 always ready (mock/specs/dojo_get_status.xml:9-23) | Constants agree — good cross-check. Ours' "slot 0 always has a soul" is a permanent lie; theirs advances with real time and no longer lets an unauthenticated caller train whoever's dojo the shared slot belonged to. |
| `keep_alive` / `logout` / session TTL | `keep_alive` is an ack **gated on a live session** (`serv_st 770` otherwise, aog.ts:1318-1321); `logout` is an ack that also retires the pcuid it was issued to (`tombstoneSessionOnProfile`, :1155-1174); sessions have **no TTL**, but the pcuid a re-login superseded is tombstoned onto the profile document (`evicted_sessions`, utils.ts:1197-1256) and stays dead across a restart — `logout` tombstones it too, which is what lets a lane that ended cleanly log straight back in instead of waiting out `VFG_SESSION_TAKEOVER_GRACE` | acks; ours drops the match on `logout`/`end_game` only (mock/MatchEngine.cs:130-140) | Neither expires. Theirs used to invalidate a session in memory only, so a restart resurrected it and a re-login answered the first client with something it could not recover from. Live client never called `keep_alive` (docs/15 §6.1 says every 60 s in `BreakTimeWindow`) — unverified path. |
| `get_jongstone_info` / `get_mg` | `free_point 0`, `mg 0` (aog.ts:2963-2969) | `free_point 10`, `mg 100` (mock/specs/get_jongstone_info.xml, get_mg.xml) | Ours deliberately lets purchases proceed; theirs has no currency. Both shapes identical. |
| `odekake_done` / `present_done` | odekake **implemented**: `success 1` with the lender's live `customize_item` + `player_game`, id = an issued per-viewer **grant ticket** (`odekakeGrantTicket`, utils.ts:928-937, looked up in the viewer's own `odekake_grants` at :941-951), order customize→player — but only for a lender that passes `borrowableLender` (live refid and player_id, a real name, `CharaData[SelectChara]` present and inside the borrower's own chara list), denial is `success 0` because a non-zero code on this endpoint is the client's hard error page (aog.ts:1561-1614, 3340-3419); `present_done` returns one `<data>` **per id in `done_ids`** with the stored `content`/`amount` and **receipts them into the stored `present_box_game`** (:3141-3167, 629-650) | odekake always succeeds with ids/`state` order reversed (`player_game` first, numeric id) (mock/specs/odekake_done.xml:9-14); one `<data id="1">` for any claim (mock/specs/present_done.xml:8-15) | Ours' present id `1` never matches a claimed PresentID, so the client's `PresentBoxGameData.Apply` skips it; theirs matches per-id and now records the receipt — the one present fact the client cannot keep for itself, since it never saves its box. THEIRS' state order and its looked-up ticket id are the ones to copy for odekake. |
| `notice_done` / `important_notice_done` / `item_gain_log` / `item_consume_log` / `chk_tabooword` / `set_favorite_character` / `janpon_log` / `eashop_done` | all answered and most of them remembered: notice acks stored per profile and subtracted from the `notice_game` the next read serves (3 notice + 2 important rows in `data/notices.json`, utils.ts:2595-2660), gain/consume logs appended to the profile document, `chk_tabooword` screens the text against `data/taboowords.json` and answers the hit (`is_available 0` / `taboo_chk 1`), `set_favorite_character` records the posted `object_id` on the profile, `janpon_log` / `gacha_log` are logged acks, `eashop_done` is an ack (aog.ts:3192-3279, 3329-3340, 3419; utils.ts:2718-2800) | every one of them falls to `_default` — code 0, body ignored (mock/Program.cs:217-239) | These are where a code-0 mock is indistinguishable from a provider that remembers. `chk_tabooword` is the one that changes behaviour: a non-zero answer blocks the name in the registration flow (`ChkTabooword.cs:37`, `NewRegistrationFlow.cs:243-259`) and blocks a table stamp on the same list. |

---

## Open questions resolved by theirs

For each docs/15 §11 item: what THEIRS does, with cites. "Silent" = their server never exercises
it either, which is itself useful (it means the shipping client cannot be relying on it).

| Q | Settled by theirs? | Their answer |
|---|---|---|
| 1 — `SUTECHOICES` on every discard? | **Yes** | Only when a *human* can actually call, one cell per claimant, and never a `select=NONE` cell: `_after_discard` builds offers for humans only and otherwise goes straight to `_cpu_calls` → next `TSUMO` (taikyoku.ts:654-678). A human seat that can call always gets `select != 0` (:680-726). |
| 2 — `TSUMO` broadcast with the drawn `pai`? | **Yes** | `_begin_turn` emits one `TSUMO` cell per draw, untargeted-by-default ⇒ `pi0..pi3` all 1, with `<pai>` = the drawn tile id for everyone (taikyoku.ts:401-416, `_cell` default targets :269-277). |
| 3 — `KYOKUEND.end_stat` | **Yes (re-confirms)** | `1` only when `game_over = busted \|\| (last && !renchan)`, else `0` (taikyoku.ts:1259-1262); the client's `end_game` follows the `1` (already live-verified in our own capture). |
| 4 — `SCORERANK(24)` trigger/cadence | **Yes** | Two triggers: immediately after any riichi-declaring discard (`_do_discard` → `_score_rank_cell`, taikyoku.ts:613-614) and once at every kyoku end before `KYOKUEND` (:1254). Body = `kyoutaku` + `riti_after{0..3}{score,rank}` (:623-631) — element names match `AC/MFG.Taikyoku.Command.Receive/ScoreRank.cs:44`. It is *not* a generic mid-kyoku sync. |
| 5 — `kan_type` numbering | **Partly** | TSUMOCHOICES: `1` = ankan, `3` = kakan (taikyoku.ts:472,477) — matching the client's only two handled values. SUTECHOICES: `2` for the minkan offer (:713), and a minkan answer arrives as `MINKAN(8)` resolved from seat+offered tile, never from `kan_pai` (:1425-1429). So `2` = "minshan kan offer" and it is inert client-side. |
| 6 — semantics of the ignored inner fields | **Yes (server-side meaning)** | `oya` = dealer seat, drives wind/rank/settlement (taikyoku.ts:222-240,1142-1147); `sai` = two decorative random 1-6 (:318); `rencyan` = always 0 (:323); `chicya` = 0 (:316); `yama`/`rinshan` = the real wall/rinshan order, sent but only used server-side (:329-331); `SUTECHOICES/pindex` = the **discarder** (:699); `naki` = mirror of the PON/CHI/KAN bits (:684-695). |
| 7 — `chi_pai` ordering per `chi_naki` | **Yes (as far as it goes)** | The wire carries only **the two concealed tiles, ascending by kind** (`_chi_options` returns ascending pairs, taikyoku.ts:639-648; cell body :798, `_ints` :64-68) and no `chi_naki` field at all; the meld shape is rebuilt from the pair contents, and their own resolver matches chi by sorted-pair comparison (:1437-1449). So: no positional convention exists to honor. |
| 8 — dealer `tepai` 13 or 14 ids | **Yes** | 13 for every seat including the dealer; the dealer's 14th arrives as the first `TSUMO` (`start_kyoku` slices 13 per seat, then `_begin_turn(oya)` draws, taikyoku.ts:305-311,333-357,360). |
| 9 — `available="0"` semantics | **Yes** | "nothing new for this cursor": emitted exactly when `cursor >= cells.length`, and in that case **without a `cell_sno` element** (taikyoku.ts:279-292). Not maintenance/compaction, and safe because `available != 1` short-circuits before `cell_sno` is read. |
| 10 — reconnect element naming + trigger | **Partly / negative** | THEIRS implements neither production path: no `recon` in any `gpost` answer, `S_RECONNECT(14)` unhandled (taikyoku.ts:1289-1347), and its `/reconnect` endpoint returns the same `<entry>` `entry_game` builds (18 children incl. `gmode`, `next_sno 0`) or, when no seat is behind the pcuid, re-enters (aog.ts:2238-2252). The endpoint name is now recovered from mid-path segments, so the route is dispatchable rather than dead (aog.ts:3454-3480). So `player{i}` vs `player_info{i}` inside `recon` is still unresolved — but it's known that a working provider can skip reconnect entirely. |
| 11 — does the server validate `must=` / `tepai_pos`? what codes on reject? | **Yes** | No validation of seq (slot 6 is never read; the answer slice is taken from `cells.length`, aog.ts:2413-2423), no validation of `tepai_pos` (slot 8 unread), and `pindex` is overridden by the server's seat record with the mismatch logged (:367-371, 2414-2416). Legality is enforced *functionally* instead: a discard not in hand silently becomes the drawn tile / last tile (taikyoku.ts:582-591), a bad pon pair falls back to `[tile,tile]`, a bad chi falls back to the first legal option (:1421-1445), and an illegal agari just re-offers the window (:1298-1330). **Codes exist for identity, not for legality**: `serv_st 770` when the addressed tid is not the caller's table (`addressedSeat` :360-374; `gpost` :2398-2411, `end_game` :2433-2448) — a command that breaks a mahjong rule is still answered code 0. |
| 12 — `gchat` during play | **No longer negative** | THEIRS' `gchat` is scene-agnostic and its stamp room is keyed by `tid` (aog.ts:379-421, 2525-2555), and its `gpost` answers carry `chat` filtered by the server's own per-seat cursor (:2424-2426) — which `AC/MFG.GameRequest/GPost.cs:73-77` parses into `Result.Stamp`. Mid-match stamps **are** delivered by a working provider; what is never used is `gchat` while playing. |
| 13 — `RANNYU(19)` | **Silent** | Never emitted; not even defined in their kind constants (taikyoku.ts:17-32). Consistent with the client having no parser. |
| 14 — units of the `entry_game` limit fields | **Yes** | THEIRS writes **seconds**: 20/15/20/15/15/30/30/30/60 under the name `ENTRY_LIMIT_SECONDS` (aog.ts:188-203). The only client consumer is the matching countdown, which ticks in **seconds** (`AC/CountDownTimer.cs:238` `m_time -= Time.deltaTime`; alert 5 s :36; display cap 99 :50) — so the ms-shaped numbers the first pass wrote froze the digits at 99. Practical answer: seconds, ≤ 99, and ours' `last_time 300` is the value still out of range. |
| 15 — rank fields 0-based | **Yes** | `_ranks()` assigns 0 to the highest score, tie-broken dealer-relative (taikyoku.ts:363-375); `mgresult.player_i/rank` and `player_info_i/rank` use the same 0-based values (:360-374,1460-1474). Ours matches (mock/MatchEngine.cs:747-754). |
| 16 — renchan dealer handling | **Yes** | Dealer keeps dealing exactly when the **dealer** was tenpai at the exhaustive draw (or on an abortive draw): `renchan = abortive \|\| tenpai.includes(oya)` → `honba += 1`, `advance_kyoku = false` (`kyoku_index`, `kyoku`, `oya`, `ba`, `all_last` unchanged) (taikyoku.ts:1249,1253-1277,222-236). A non-tenpai dealer (even with other seats tenpai) → `honba = 0` and the kyoku advances. **Ours is wrong on both halves** (mock/MatchEngine.cs:611,625-638 grants honba on any-tenpai and rotates the dealer). |

New questions theirs raises that docs/15 doesn't ask:

- N1. `kyoutaku` accounting contract: is the 供託 subtracted from the declarer at declaration
  (theirs: `score −= 1000` at discard, taikyoku.ts:606) or only at settlement? Either works if
  `calc_score.before_score` is consistent with it, but a mock mixing the two will show double
  deductions. Theirs' invariant: `new_score == before + yaku + kyotaku + tumifu` for all four
  seats, always (`_calc_score_xml`, :1117-1135) — worth stating as a self-test rule.
- N2. `gmode` → (seats, kyoku count, start score, wall kinds) as one table: theirs'
  `GMODE_TAKU`/`SEATS_OF`/`KYOKU_COUNT`/`START_SCORE` + sanma (M2-M8 removed) / nima (no manzu,
  no West/North) tile filtering (aog.ts:120-146; mahjong.ts:67-108) is a complete answer to
  docs/15 §8.4's "keep `gmode` and the dealt tiles consistent". Nima dora-indicator wrap
  (South→East) is handled too (mahjong.ts:146-166).
- N3. `pnum`/`cpu_num` ⇒ required `mend` child count (`PlayerNum = pnum + cpu_num`, loop
  unguarded, `AC/MFG.Taikyoku/MatchingData.cs:35,76-80`) and `pnum` ⇒ `epdata_{0..pnum-1}`
  (:52-60). Docs/15 §1.5 mentions neither coupling.
- N4. Is `client_states` inside `mend/player_i` the mechanism by which the client renders *other*
  players' characters/cosmetics? THEIRS sends `player_game` + `customize_item` blobs for each human
  when its profile has them (`matchingPlayerHuman`, aog.ts:150-159); the client rebuilds
  `PlayerGameData` from them and takes name/dan from
  the blob (`AC/MFG.Taikyoku/PlayerInfo.cs:86-92`). Ours sends `player_game = e30=` (empty JSON),
  so other-human avatars fall back to defaults.

---

## What to change in our mock

Priority order; each is a small, targeted edit. (Not applied here — this document is the deliverable.)

1. **Fix the tsumo-window win bit.** `mock/MatchEngine.cs:371-372,381`: use `0x40`
   (`SELECTABLE_TYPE_FLAG.TSUMOAGARI`) instead of `0x80` (RON) — and label it correctly. Without
   this the shipping client can never press 和了, so our loop has never exercised `S_TSUMO_AGARI(3)`
   or a human agari path (0 × kind 3 in 264 live gposts). Add a selftest assertion that a winnable
   human draw emits `select & 0x40`.
2. **Fix renchan.** `mock/MatchEngine.cs:611,625-638`: on exhaustive draw, `renchan = dealer in
   tenpai`; renchan ⇒ honba+1, **dealer and kyoku number unchanged**; else honba 0, kyoku advances.
   Drop the `RenchanCount >= 4` cap and end on "last kyoku of `KYOKU_COUNT[gmode]` && !renchan"
   (add bust: any seat `< 0` ends the match) as at taikyoku.ts:1249-1262.
3. **Emit `SCORERANK(24)`** with the theirs-documented cadence — after any riichi-declaring
   `SUTEHAI`, and immediately before every `KYOKUEND`, body `kyoutaku` + `riti_after{0..3}{score,
   rank}` (taikyoku.ts:613-614,1254). This also gives us a live test of `AC/ExecKyoku.cs:249-266`,
   which our client loop has never seen.
4. **`yaku_han` = `Han` tier code**, not the han count: map (han, fu, yakuman) → 1-4 / 5 mangan /
   6 haneman / 7 baiman / 8 sanbaiman / 9 yakuman (+n for multi-yakuman) exactly as
   `han_rank` (mahjong.ts:1286-1295); keep raw han in `han_num`. Compute `fu_num` instead of
   hardcoding 30 (mock/MatchEngine.cs:697), and fill `kyotaku_score`/`tumifu_score` instead of
   emitting zeros (:725-727) — honour-bonushi and 供託 are the visible-in-text parts of settlement
   (`tumifu` drives the client's 場積 line, `AC/Agari.cs` panel).
5. **Claim barrier + call vocabulary** (only if we add calls): one `SUTECHOICES` per claimant
   (single-seat `pi`), `pindex` = discarder, `naki` = call bits mirror, patterns `chi_pai` (2 ints
   per pattern, ascending) / `pon_pai` / `kan_pai`+`kan_type=2`; require **all offered seats** to
   answer before resolving, then resolve ron(all) → pon/minkan → chi → decline
   (taikyoku.ts:1367-1457). Add `ptn_num`/`ptn{i}` + `kan_pai/kan_type 1|3` to `TSUMOCHOICES` and
   `REACH=0x200`/`KAN=0x8` bits so riichi/kan windows exist at all (mock/MatchEngine.cs:371-381).
6. **Adopt theirs' available/cursor shape** as an option: `available="0"` with no `cell_sno`
   when the cursor is already current (taikyoku.ts:279-292). It is the shape the live service
   produces, so it is what our client has actually been eating; keep `available="1"`+`count=0`
   for the "nothing new but cursor valid" case too, since both parse.
7. **Copy `__count` onto array tags** (`<dora __count="5">`, `<machi_pai __count="1">0</machi_pai>`,
   taikyoku.ts:64-68). Harmless to the client, and it is what the real serializer emits.
8. **Roster/identity gaps**: add `epdata_{i}` for each human in `mwait` (parsed at
   `AC/MFG.Taikyoku/MatchingData.cs:52-60`); keep `mend` child count == `pnum + cpu_num` exactly
   (docs/15 §1.5 gap N3); send `player_game` **and** `customize_item` blobs in `mend/player_0`
   instead of `e30=` so the other-seat rendering path is exercised (gap N4); stop restating
   `mwait` in every `gpost` ack (`gpost.xml`/mock/MatchEngine.cs:266) or note that theirs omits it.
9. **`entry_game` limit fields → seconds, ≤ 99** (e.g. `last_time 60`) with a comment citing
   `AC/CountDownTimer.cs:50,238`; also fix the stale log line `MATCH entry_game tid=1027` →
   the tid actually served (mock/MatchEngine.cs:110 vs mock/specs/entry_game.xml:11) — the
   capture shows the client using `tid=1`.
10. **Panel realism, cheapest wins first**: multi-row `get_gacha_result` honoring `times`;
    per-id `present_done` rows echoing the claimed ids; `gget_stamp_info` cursor (`idx > since`)
    instead of a pinned `idx="1"`; `battle_item_settings` in `get_menudata`; store at least
    `player_game` from `client_state_write` and echo it back on `client_state_read` (mirroring
    theirs' PlayerName mirror, aog.ts:2012-2030) so continue-flow state round-trips are actually
    tested; add `gmode` support for sanma/nima with theirs' seat/kyoku/tenbo/wall tables and the
    `player_info{i}` filler for unused seats (taikyoku.ts:344-349).
11. **Session hygiene**: expire `sessions` entries (ours leaks them; mock/MatchEngine.cs:83,130-140),
    and if we ever want theirs-comparable identity, key state by uppercased `user_id` (= their
    `refid`) and issue an opaque 32-hex `session_id` at login (aog.ts:1057-1121), tombstoning the
    superseded one on the profile so it stays dead (utils.ts:1197-1256).
12. **Names served**: both sides now serve what the client actually
    posts — `player_record` (`GetRecord.cs:21`) and `eashop_done` (`EashopDone.cs:18-20`):
    ours via `mock/specs/player_record.xml` and `mock/specs/eashop_done.xml`, theirs via
    handlers (aog.ts:3316 `player_record`, aog.ts:3419 `eashop_done`). The invented
    `get_record`/`coop_done` aliases were removed from their route map (aog.ts:3282-3284);
    no client posts those names.

Deliberate non-changes (ours is the correct side): root-level `chara_enabled` and
`competition_entry` shape — theirs moved to that shape, so this row is now "both sides agree,
ours was right first"; `serv_st/code` only (root-level `<code>` is inert but harmless); dealing red
fives (their engine never deals one); `breakdown`-rich `mgresult` (client-optional, useful for our
exp/affinity display tests). `last_time` stays in **seconds**, but ours is still 300 and theirs is
60 — that one is a real change, listed at item 9.

---

## What to change in docs/15

1. **§2.3/§3.4 — flag `SELECTABLE_TYPE_FLAG` usage as the single most error-prone constant pair**:
   add the two worked values (`TSUMOCHOICES` win-possible = `0x400|0x40`;
   `SUTECHOICES` ron = `0x80`(+0x1)), and note that `SUTECHOICES.naki` mirrors the PON/CHI/KAN
   bits while `SUTECHOICES.pindex` is the **discarder**, both server-informational
   (taikyoku.ts:684-699).
2. **§3.3 `SCORERANK(24)`** — replace the `UNCONFIRMED` with the observed provider cadence and body
   (riichi + kyoku end, `riti_after{i}{score,rank}` + `kyoutaku`) and keep one caveat: this is
   what the provider does, not proof about Konami's server.
3. **§3.4 `ScoreData`** — document the invariant `new_score = before + yaku + kyotaku + tumifu`,
   that 供託 is debited at riichi declaration by the live provider, and that `tumifu_score` is the
   honba transfer (100/seat tsumo, 300 ron). Also record `yaku_han` → `MFG.Types.Han` tier mapping
   with the `han_rank` cut-offs, since docs/15 §3.4 currently just says "`yaku_han`".
4. **§7.1 KYOKUSTART** — settle the `renchan` row: dealer-tenpai (or abortive) ⇒ honba+1 with
   `oya`/`kyoku`/`ba` unchanged; `kyoku = index % seats`, `ba = index < seats ? 0 : 1`,
   `all_last = index ≥ KYOKU_COUNT[taku] − 1`; `KYOKU_COUNT` = 4/8/3/2 and start scores
   25000/25000/35000/35000; dealer `tepai` is 13 ids; `sai` is decorative.
5. **§6.3 `entry_game`** — units note: `last_time` is consumed as **seconds** with a 99-display cap
   and 5 s alert (`AC/CountDownTimer.cs:36,50,238`), the provider writes ms-shaped values
   and freezes its countdown at 99; add that the live provider appends an 18th child `<gmode>`
   (client ignores it) and returns an absolute `gserv_url`.
6. **§1.5 `game/taikyoku` + `mwait`** — add the two couplings from gap N3 (`mend` child count ==
   `pnum + cpu_num`; `epdata_{0..pnum-1}` keyed by `pnum`), the exact `available="0"` shape
   (no `cell_sno`), and the `__count` attribute convention.
7. **§5.5** — state plainly that neither observed server binds or even reads `must` slot 6, that
   `pindex` is server-authoritative on the live provider, and that legality is enforced
   functionally (silent substitution/re-offer) with **no non-zero `serv_st/code` ever returned** —
   i.e. the seq binding is a client invariant, not a handshake.
8. **§6.1/§6.5** — record the provider's real matching policy (lobby per `gmode`+passphrase,
   CPU-fill only on a `gget` > 60 s after lobby creation, instant private table for 2-seat modes,
   passphrase lobbies match humans together), the > 60 s `gget`-only disconnect→CPU conversion,
   and that `end_game`/`kiken_game` are one handler that archives only natural finishes.
9. **§6.6** — downgrade reconnect to "not implemented by the working provider": no `recon` in
   any `gpost` answer, `S_RECONNECT(14)` unhandled, and `/reconnect` answers the same
   `entry`-shaped table rejoin `entry_game` builds (`handle_reconnect`, aog.ts:2238-2252 →
   `entryXml` :222-240, `<gmode>` and `next_sno 0` included) or re-enters when the seat is gone.
   The `player{i}` vs `player_info{i}` question is still open and unexercised either way.
10. **§11** — mark Q1,Q2,Q3,Q4,Q6,Q7,Q8,Q9,Q11,Q14,Q15,Q16 as answered by an implementation
    (theirs), Q12 as answered in the opposite direction from docs/15's guess (mid-match stamps are
    delivered through `gpost` `chat`), Q5,Q10 as partially answered, Q13 as still open — a working
    provider never emits `RANNYU` — and add N1-N4 from the previous section. Add a pointer to
    this document from §11's header.
11. **§1.1/§6.2 identity** — record that the live provider treats the posted `user_id`
    (uppercased) as the durable identity (`refid`), issues a fresh 32-hex `session_id` per
    `login` with no TTL, and resolves an account **from the login session and nothing else**
    (`resolveProfile` *is* `sessionProfile`, utils.ts:763-776): null for a session this server
    never issued, for one a re-login superseded (the profile's own `evicted_sessions` record,
    :1197-1256), for one that resolves to a legacy phantom document (`MID` + 8-or-more digits, utils.ts:571; refused at :289-336, :407-448, :481-489, :531-538). `mid` is
    `playerIdFromRefid`'s 31-bit refid-prefix hash, minted as
    `parseInt(refid[0:8],16) & 0x7fffffff` with a djb2 fallback (utils.ts:210-221), and it is **no
    longer a fallback identity**: mid 10/11/13/14 are accounts in the live savedata, and a posted
    mid used to resolve profiles, which is how an invented pcuid read someone else's 32 states and
    overwrote their `player_config` (:743-757 names the client call sites that post no mid at all).
    `mid` is minted lazily in `ensureProfile` and its collisions repaired by `findFreePlayerId`
    (utils.ts:450-465, called at :497 and :508-509). Cross-check with our own capture:
    the shipping client's minted `user_id` has **Unix-seconds in its high 32 bits**
    (`6ADC2910…` → 1792813328 = the request's own timestamp, `mock/mock.log.txt` seq=6), so
    theirs' `mid` rule is literally `seconds & 0x7fffffff` for guests — stable across launches for
    one DataID, and colliding for two players minted in the same second — which is what
    `findFreePlayerId` exists to fix, and why a mid is a display id rather than a key.
12. **§8** — note that the live provider never deals red fives (no `+64` ids, mahjong.ts:137-144)
    while the client's enum, art layer and akadora counter clearly support them, and that dora
    indicator wraps differ per taku (nima: South→East) — our red-five mock is the more faithful one.

---

## Notes for using theirs directly

- **Route prefix.** Their plugin registers bare names in `AOG_HANDLER_MAP` (aog.ts:3285-3434), one
  `R.AogRoute` per name in `registerAogRoutes` (:3541-3568); the `/aog` prefix comes from the core,
  whose middleware extracts the name from the **last URL segment**
  (`src/middlewares/AogMiddleware.ts:13-18`) and only treats a request as AOG when the url starts
  with `/aog` or that last segment is one of its 45 `KNOWN_AOG` names (:20-26,36-38) — a `Mozilla`
  user-agent or an `x-eamuse-info`/`x-compress` header routes the request away from AOG entirely
  (:32-35). Two shapes the client actually emits break that extraction: the doubled slash in
  `…/aog//gget` (docs/15 §1.1) and a trailing slash, which yields `""`; and a name in the middle of
  a path — `/aog/reconnect/<gserv_url>` — is never seen at all. `extractAogName` (aog.ts:3454-3480)
  is the fix, applied by the unhandled fallback (:3489-3533) and by the second native listener on
  `mfg_http_port` 22421 (index.ts:244-340): collapse doubled slashes, strip `/aog` even where a
  `gserv_url` is glued to it, match exact segments from the deepest end, then the longest
  registered name as a substring, so a `gget_stamp_info` segment is never read as `gget`. `/aog/login/` reaches `handle_login` now — via the fallback,
  which is the fragile part: the core calls it as `fb(form, ctx)` (`AogMiddleware.ts:67-76`) and the
  first version wrote it as `R.AogUnhandled(ctx)`, so the request was never answered at all
  (aog.ts:3482-3488). A handler that throws is invisible too: the middleware answers code 0
  (`AogMiddleware.ts:82-87`).
- **Body decoding**: theirs URL-decodes the body with `URLSearchParams`
  (`AogMiddleware.ts:5-11`), merges the query string over it (:50-55) and then re-tries four base64
  strategies on `client_state_write` (aog.ts:1921-1951) — the `+`-as-space trap is real and already
  bitten once there (their own comment at :1921-1926). Any client that posts raw base64 will
  corrupt silently under naive form decoding.
- **AOG-only (no card) flow works**, and it is what their guest path is built for: `guest=1` is
  only a flag recorded on the profile (aog.ts:1025,1098); identity is the client-minted `user_id`;
  `get_menudata` for a pcuid with no live session is **refused** (`serv_st 770`, :1290-1296) rather
  than answered as mid `1`/`GUEST`, and `entry_game` for one stamps mid `0` so the match archive is
  never attributed to somebody else's account (:2086-2096). Our guest loop (fresh client-minted
  `user_id` per launch, `pcuid` = the returned `session_id`, stable for the whole launch incl.
  `end_game`) works against theirs — state persists because the same `user_id` re-binds the profile,
  and each launch gets a brand-new session, with the superseded pcuid tombstoned so the first
  client cannot keep playing on it (aog.ts:1093-1106, 1099-1106; `AC/MFG.GameRequest/Login.cs:30-31`).
  Skipping `login`, or inventing your own `pcuid`, now ends in 770s rather than a phantom profile.
- **Costs of using theirs for protocol experiments**: no `recon` and no `RANNYU` at all (those code
  paths cannot be exercised there); mid-match stamps do arrive, through `gpost` `chat`
  (aog.ts:2424-2426), which is the only way to see them. A solo guest in a 4-seat public lobby
  waits the ~60 s CPU-fill window (`LOBBY_FORCE_SECONDS`, :205-207, applied :2276-2283); a 2-seat
  Versus entry is an instant private table and never shares a lobby (:2143-2152). Tables are keyed
  by `tid`, tids come from a persisted counter so a restarted server no longer reissues a live
  table's number (utils.ts:1044-1093), a repeated `entry_game` re-takes the caller's own seat in the
  same lobby instead of opening another table (:2191-2225), and `end_game` archives only a table
  that finished naturally, for the seat the request addresses (:2452-2495). No timers means a human
  who stalls mid-kyoku stalls the table until the client's own think clock answers, and a human who
  disconnects mid-claim is reclaimed only by the sweep inside `gget` once `lastSeen` is 60 s stale
  (:2293-2335; `lastSeen` itself is bumped by any request that addresses the table, :360-374) — and the
  shipping client sends no `gget` during play, so that reclaim is a server-side assumption, not an
  observed flow.
- **Their engine's deliberate simplifications to expect in any capture**: no red fives ever
  (mahjong.ts:137-144), so no akadora and no `+64` ids on the wire; no open-riichi/fire-riichi/
  atomic-riichi (`stat` bits 2-4 never set, `reach_flg` treated as boolean — taikyoku.ts:1297);
  `kan_type` `2` in sute choices; `rencyan` always 0; `chicya` always 0; `dora_open` capped at 5;
  abortive-draw support limited to 九種九牌 (`S_KYUSYUKYUHAI` → tenpai-less renchan, :1331-1333);
  multi-ron supported in scoring but CPU→CPU ron is taken by the first seat in turn order (:728-739);
  no `kyuushu` for CPU seats, no 流局満開/四風連打 style abortive draws.
- **Bug-shaped behaviors to copy only knowingly**: `mustInt(parts, i) || def` — a legitimate `0`
  collapses to the default (utils.ts:130-137, still the shape, still wrong for a real zero); the
  stamp cursor is not the client's at all, `gget` reads slot 6 of the posted `must` as `nextSno`
  for match cells (aog.ts:2258-2262) and answers `chat` from the server's own per-seat record
  (:2374-2379, utils.ts:1017-1035), so an always-0 client cursor silently means "whatever the server
  thinks you have not seen"; `moserv_url`/`gserv_url` advertise `CONFIG.port` (8083) while the
  native AOG listener sits on `mfg_http_port` (22421) — utils.ts:3143-3174 and aog.ts:998-1014
  against index.ts:244-250; `entry_game` defines `sendDiscordWebhook` inline and fires it on every
  lobby create and join when `VFG_DISCORD_WEBHOOK` is set (aog.ts:2114-2188, 2195-2220). Out of
  this list now: the wrapped `chara_enabled` and `competition_entry` names and the ms-shaped
  `entry_game` limits — all three are fixed in the shipped copy.
- **Quick parity checklist for our client-vs-theirs smoke test**: `web_id=1`, `area_id=1`,
  `AppVersion=2.0.2`, `matching_ver=70` (live `appli_info` body is exactly `matching_ver=70`);
  `guest=1`; `passphrase` sent as a single space when empty — theirs trims it or every public
  lobby collapses into one key (aog.ts:2086-2088); `entry_id`/`passphrase` with their literal
  trailing spaces and `debug_score=-1` (live bodies, `mock/mock.log.txt`); `app_param=0/0`
  constant; `end_game` carries `version=2.0.2` and no `gserv_id` (docs/15 §6.5 confirmed live).
