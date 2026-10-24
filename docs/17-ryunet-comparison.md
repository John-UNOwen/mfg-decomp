# 17 — Our mock vs the live  <provider> provider (`mfg@asphyxia`)

Purpose: audit the accuracy of [docs/15-gget-gpost-command-contract.md](15-gget-gpost-command-contract.md)
and `mock/` against the server that actually runs the game today —  <provider>'s Asphyxia plugin
(`tmp/asphyxia_plugins/mfg@asphyxia`, TypeScript; `handlers/aog.ts`, `taikyoku.ts`, `mahjong.ts`,
`handlers/utils.ts`, `index.ts`).

Method: line-by-line comparison of the two implementations, plus the recovered client
(`AC/` = `api/recovered/Assembly-CSharp/`) as the tie-breaker on "who is right", plus 900 live
requests captured against our mock in [mock/mock.log.txt](../mock/mock.log.txt).
Citations: THEIRS = `handlers/aog.ts:NNNN` / `taikyoku.ts:NNNN` (relative to
`tmp/asphyxia_plugins/mfg@asphyxia/`); OURS = `mock/MatchEngine.cs:NNNN`, `mock/Mahjong.cs:NNNN`,
`mock/Program.cs:NNNN`, `mock/specs/x.xml:NN`.

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
2. **Semantic choices docs/15 left open** — theirs answers 8 of the 16 open questions in §11
   (see *Open questions resolved by theirs*); several of its answers say "the server never does
   that either" (`recon`, `RANNYU`, mid-match `gchat`, seq/`tepai_pos` validation).
3. **Depth we deliberately skipped** — theirs is a full game server: riichi (with 供託 accounting
   and a `SCORERANK(24)` cadence), chi/pon/kan incl. chankan and rinshan, multi-ron resolution,
   red-free walls, real han/fu → payment tables, honba/kyoutaku transfers, abortive 九種九牌,
   CPU AI with shanten/danger evaluation, 23 gmodes × seat/tenbo tables, 60 s disconnect→CPU.
   Ours is 4-seat tonnan, no calls, nominal mangan money.
4. **Their panel layer is state-driven, ours is canned** — theirs persists a profile per `refid`,
   serves *every* stored `client_state` kind plus server-synthesized `friend_game` /
   `present_box_game`, and drives My Room / costumes / friend-borrow / records / replays off that.
   Ours answers 2 canned states and ignores every write. A stricter client or any flow beyond
   quick-play will find ours empty.

Also verified against the client, on points where the two servers disagree, **ours is the
correct one**: `chara_enabled` must be root-level (ours) — theirs wraps it and silently disables
its own gacha notice (aog.ts:1906 vs `AC/MFG.GameRequest/CharaEnabledSend.cs:34-41`);
`competition_entry` needs root-level `entry`/`displayed_result` (ours) — theirs sends
`<competition><entry_result>` which the client cannot read (aog.ts:1786 vs
`AC/MFG.GameRequest/CompetitionEntry.cs:44-61`); `entry_game` limit fields are consumed as
**seconds** (ours, 5-300) — theirs writes millisecond-shaped numbers (15000/30000) into a field
the client decrements with `Time.deltaTime` (aog.ts:1009-1017 vs `AC/CountDownTimer.cs:238,40`).

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
| SUTECHOICES `select` | `0x81` (RON|NONE) (:394,440) | `F_RON`/`F_PON`/`F_CHI`/`F_KAN` bitmask (taikyoku.ts:681-695) | same bit for the offer we support; ours has no call offers |
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
- **`gpost` response body**: THEIRS returns only the new cells + a suppressed `chat`
  (`<game>{cells}</game><chat></chat>`, `since=1e9`, aog.ts:1203-1214) — **no `mwait`**. OURS
  re-states the full roster `mwait` in every gpost ack (mock/MatchEngine.cs:265-268). Both parse;
  ours is the noisier one. THEIRS also suppresses the `chat` payload in gpost, so mid-match
  stamps can only ever arrive through `gget` (which the client stops calling during play).
- **Root/envelope**: THEIRS `<?xml …?>\n<root><serv_st><code>0</code></serv_st>…`
  (utils.ts:58-61); OURS `<res><serv_st><code>0</code></serv_st><code>0</code>…`
  (mock/MatchEngine.cs:777). Root name is irrelevant and the root-level `<code>` is inert
  (`AC/MFG.GameRequest/RequestApiBase.cs:146,162-178`). Ours also ships the spec files' `<!-- -->`
  comments in responses (mock/Program.cs:285-317); theirs ships none. Cosmetic.
- **`must=` slot usage**: theirs reads tid(3), kind(7), pindex(4), pai(10), tepai_id(11),
  tepai_id2(12), reach(13), tsumogiri(14) and **ignores seq(6), tepai_pos(8) and chi_naki(15)**
  (aog.ts:1186-1194 via `mustInt`, utils.ts:77-88); it then *discards the client's `pindex`* and
  uses the server's seat record (:1205). Ours reads seq(6, logged only), kind, pai, tsumogiri
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
only when `seats` humans have entered, otherwise it is CPU-filled **on a `gget` that arrives
> 60 s after the lobby was created** (aog.ts:1087-1095). Since `MatchingFlow` keeps `Fetch()`ing
`gget` while `mwait/status == 0` (`AC/MatchingFlow.cs:243-246`), a solo guest does get a table —
after ~60 s, not instantly like ours (poll 2, mock/MatchEngine.cs:153,171-188). A 2-seat (Nima)
`gmode` is special-cased to an instant private table (aog.ts:906-911).

`gget` while unseated: THEIRS still emits `mwait` with `status 0`, `pnum = humans in lobby`,
`cpu_num 0` and a `mend` containing exactly `player_0..pnum-1`, plus `epdata_{0..pnum-1}`
(name+mid) and **always a `<mend>` tag**; `all_ready 0`; no `taikyoku` at all unless
`ready=1 && table exists` (aog.ts:154-205,1169-1181). OURS: `status 0` on poll 1 but with
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

- **No server-side timers exist on either side.** THEIRS has no `must`/`wait`/`timeup` concept in
  the match layer; CPU seats act inside the same request that drew them (taikyoku.ts:401-416), and
  the only deadlines are the `entry_game` limit fields, which are display input
  (`AC/CountDownTimer.cs` countdown) — not enforcement. Ours: same (mock/MatchEngine.cs never
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
`KYOKUSTART` in the same response (aog.ts:1203-1212 + taikyoku.ts:279-292) — exactly what our
capture shows the client accepting (~2.9 KB on the inter-kyoku hop, `mock/mock.log.txt`
seq=37, bytes=2931). OURS: same single-answer latch (mock/MatchEngine.cs:253-257). So docs/15
§11 Q4's "does the server need all four" answers as: *needed for human seats only*.

Resign: THEIRS `S_KIKEN` marks the table over server-side (:1343-1346) and `kiken_game` is the
*same handler* as `end_game` (aog.ts:1825-1826 → 1217-1268) — it archives only `finished` tables,
so a kiken writes no `player_record` row (:1224-1259). OURS: `kind=13` clears the match
(mock/MatchEngine.cs:246-251) and `kiken_game` returns the same `mgresult` as `end_game` (:130-137)
— but ours also returns a `g2result` block in kiken that theirs never sends (mock/specs/kiken_game.xml).

Disconnect / kiken / reconnect: THEIRS' only liveness mechanism is in `handle_gget`: a human seat
whose `lastSeen` (bumped by `gget`) is > 60 s stale is removed from `humans`, and if that seat was
mid-discard the CPU discards for it, or mid-claim its offer is dropped and CPU calls are resolved
(aog.ts:1099-1141). **This never fires mid-match for the shipping client, which does not `gget`
during play** (0 mid-match ggets in the capture) — so theirs can stall a table on a dead human
until someone else polls. Reconnect: THEIRS implements only the standalone `/reconnect` endpoint,
returning an `entry`-shaped body (with `<gmode>` and absolute `gserv_url`) and re-adding the seat
to `humans` to take back control from the CPU; an unknown pcuid falls through to `handle_entry_game`
(aog.ts:1026-1069). It **never emits `recon`** in a `gpost` answer and does not handle
`S_RECONNECT(14)` at all (:1289-1347 dispatch) — so docs/15 §6.6's production reconnect path is
implemented by *neither* server, and their `reconnect` response shape (`next_sno 0`, full `entry`)
is a table-rejoin, not a snapshot. `next_sno 0` is on both sides' `entry_game` too
(aog.ts:1006, mock/specs/entry_game.xml:13), so a mid-match re-entry replays the whole cell log —
legal for the client (it re-applies the log) but heavy.

`mgresult` / final delivery: THEIRS emits `gmode` (from the posted `gmode` param) + `taku_class 1`
+ `continue_state 0` + `continue_fee 0` + `player_i{rank,score,uma}` for i < seats, where `i` is
the **seat** and `uma` is indexed by **rank** from a per-seat-count table (4p 20000/10000/−10000/−20000,
3p 20000/0/−20000, 2p 10000/−10000) (aog.ts:131-152; taikyoku.ts:1460-1474). No `breakdown`, no
`old`/`new`, no `g2result`, no exp/affinity. The client tolerates that completely: every child of
`mgresult` is `NeedNot`/null-checked, including `breakdown`, `battle_item_use_info` and
`player_{0..3}` (`AC/MFG.GameData/MahjongResultInfo.cs:75-186`). OURS sends `breakdown` exp +
affinity and a `g2result` in kiken (mock/MatchEngine.cs:756-773, mock/specs/end_game.xml,
kiken_game.xml) with tonnan uma `{30,20,10,−60}` — parse-safe either way, but the uma **numbers**
differ, and ours has no 3-seat/2-seat uma table.

---

## Panel endpoint deltas

Non-match endpoints: OURS = canned `mock/specs/*.xml` served by name-matching in
`mock/Program.cs:217-239` (`/aog/x`, bare `/x`, any path segment, longest substring, `_default`);
THEIRS = `AOG_HANDLER_MAP` (aog.ts:1812-1908) registered via `R.AogRoute` (aog.ts:1913-1933) →
core middleware keyed on the **last path segment** (`tmp/RyuNET-core/src/middlewares/AogMiddleware.ts:13-18`),
plus a second native listener on `mfg_http_port` 22421 (index.ts:114-203). Their unhandled path
returns `<root><serv_st><code>0</code></serv_st></root>` (index.ts:214-221) — same contract as
our `_default.xml`. Names theirs serves that we route to `_default`: `get_record` (→ their
player_record, aog.ts:1844) and `coop_done` (:1902).

Both sides agree on `serv_st/code = 0` and always HTTP 200 `text/xml; charset=utf-8`. Deltas that
matter for a stricter client or a fuller flow:

| Endpoint | THEIRS (verified) | OURS | Why it matters |
|---|---|---|---|
| `login` | `<auth><session_id>` = 32 random hex, **no `user_id` echo**; refid = uppercased `user_id\|dataid\|refid\|ref_id`; profile created as `GUEST` (name owned by `create_player`); once-per-JST-day auto-apply present appended to `present_box_game` (aog.ts:361-404) | `session_id = mock_pcuid_<user_id>` + `<user_id>` echo (mock/specs/login.xml:8-11) | The client reads only `auth>session_id` (`AC/MFG.GameRequest/Login.cs:30-31`), so ours is protocol-legal; but theirs is where identity, saves and daily bonus actually live. Ours is stateless: every later call re-derives from the posted `pcuid` string. |
| `create_player` | ack only; stores `name` from `name\|player_name`, resolves refid via **pcuid session** when `user_id` absent, mirrors name into `states.player_game.PlayerName`, binds card if any (aog.ts:411-454) | ack only (mock/specs/create_player.xml) | Ordering: THEIRS requires `login` before `create_player` (session→refid resolution, :417-421) or it forks a random refid (:422). Our capture order (`create_player` **then** `login`) still works against theirs because both carry `user_id`. |
| `get_menudata` | `mid` = profile `player_id` (derived from refid) else `1`, name else `GUEST`; **23 playable modes** as `<playmode_list><mode>`; **`battle_item_settings`** with `<setting gmode taku_class>` per mode (aog.ts:299-321,456-473) | mid 1000001 + one `<playmode>`, no `battle_item_settings`, no `recon` (mock/specs/get_menudata.xml:10-28) | Theirs is not fail-closed: unknown pcuid ⇒ mid 1 + `GUEST` (docs/15 §6.2 identity minting still satisfied). Client iterates the mode list name-agnostically (`AC/MFG.GameRequest/GetMenuData.cs:73-82`) but *does* read `battle_item_settings` (:83-87). One mode = our mock can't be used to test sanma/nima/event gmodes. |
| `client_state_read` | every stored kind, control-chars stripped; `player_game` healed (chara lists int-normalized, `GachaCharaUsableList` pruned to proven unlock cutins 176…493); `cc_request` sanitized/quarantined; **server-synthesized `friend_game`** (all other profiles as lenders) and **seeded `present_box_game`** (PresentID 9001-9003); honors `one_kind` (aog.ts:479-683) | exactly 2 canned states (`version_game`, `player_game` w/ `CharaData:[]`), no `one_kind` (mock/specs/client_state_read.xml:10-11) | Biggest panel gap. Ours never returns `customize_item`, `player_item`, `player_config`, `mission`, `gacha`, `ea_shop`, `log`, `music_gacha`, `friend_game`, `present_box_game`, `cc_request`…, so My Room/costumes/missions/presents/friend-borrow have nothing to load. Also note theirs never seeds `version_game`, so a fresh profile returns **zero** `<state>` nodes (client keeps its defaults) — ours pins `SaveDataVersion 68`, which is the safer shape but a difference. |
| `client_state_write` | ack; tries 4 base64 decodes (`+`-as-space / raw+ / raw / plain, first valid JSON wins), drops unparseable JSON with a log, quarantines bad `cc_request`, mirrors `PlayerName`→profile; `saveState` refuses mid 1 and forks `MID%08d` profiles otherwise (aog.ts:729-849; utils.ts:367-394) | ack, `data` ignored entirely (mock/specs/client_state_write.xml) | Live writes are ~26.5 KB of base64 with URL-encoded padding (`…%3d%3d`) — the client sends the full PlayerGameData on every launch (2× per launch, `kind=player_game` only). Theirs' 4-variant decode is the lesson: **`+` in urlencoded base64 is ambiguous**; ours never touches it, so we've never hit it. If we ever store states, copy their variant order. |
| `mission_date` | `info_data kind="missions"` with **7 categories** (`JudgmentDaily,JudgmentWeekly,Player,Character,Weekly,Yaku,StartDash`), begin 2020-01-01 / end 2099-12-31, `param:""` (aog.ts:1620-1622; utils.ts:920-940) | 1 category (`Player`) in a 2026 window with an `item_expire` param (mock/specs/mission_date.xml:9) | 1 vs 7 active mission groups; the redundant int `Category` key theirs sends is discarded by the client (`Enum.TryParse(category)` wins). |
| `appli_boot` | same nodes; `moserv_url = http://<host>:<CONFIG.port|8083>/aog` (aog.ts:335-350; utils.ts:24-25) | Konami stg host + `reviewed_version false` (mock/specs/appli_boot.xml:9-17) | THEIRS can advertise 8083 while its native AOG listener is 22421 (index.ts:114-203) — `moserv_url`/`gserv_url` only matter to the unused reconnect path, but a client that followed them would hit the wrong port. Ours advertises an unresolvable host that only works via the test proxy. |
| `appli_info` | `expire_seconds 3600`, then `events` (~110 active entries incl. `CostumeAddition1..77`), then `pro_stats` (aog.ts:352-359; utils.ts:1017-1082) | `expire_seconds`, `pro_stats`, `events {"list":[]}` (mock/specs/appli_info.xml:9-11) | Kind order is cosmetic (`kind` attr dispatch). Our empty event list switches off every event-gated feature (costumes, event taku, odekake) — deliberate, but state it in docs/15. |
| `gacha_info` / `req_draw_gacha` / `get_gacha_result` / `cutin_gacha_play_*` / `music_gacha_*` | 52 curated series (ids must equal client `GachaSeriesName` ordinals), rates 400/300/200/100, `custom_pickup_items`, `force_end_date`; opaque 16-hex transaction id stashed on the profile; `times` result rows + `<gift>` nodes; draw resolves real OIDs from a pool with chara/exclude filters, `fight_spirits` = drawn CharaType, echoes posted `request_id`; music play dedupes owned items, `gift 2`, empty `fight_spirits` (aog.ts:1363-1610; utils.ts:553-611,797-815,865-893) | 1 banner (`pickup_rate 70/20/9/1`), fixed numeric txn id, **1 result row regardless of `times`**, fixed OID + `gift 1` on cutin draw, hardcoded request_id 1 (mock/specs/gacha_info.xml, get_gacha_result.xml, cutin_gacha_play_draw.xml) | Row count vs `times` is load-bearing (the client loops `play_count` rows); ours' single row for a 10-pull is a fidelity bug. `gift` values differ (ours 1 = DuplicateGift-shaped, theirs 0 in cutin / 2 in music = gift-config-shaped). |
| `player_record` | archive-driven: `player_record` with **no `<total/>`**, one `<info>` per Yonma/Sanma/Nima actually present, `rank_count` one per seat, `yakuman_num` summed, ≤20 `<history>` with `YYYY/MM/DD HH:MM:SS`; **empty `player_record` when no session/archive** (aog.ts:1632-1693) | populated Yonma block with `<total/>`, one `rank_count`, dash-separated date (mock/specs/player_record.xml:9-27) | Empty-vs-populated is the behavioral difference a real provider has and ours can't fake. `rank_count` is iterated (count is display-only), `<total/>` is safe both ways. |
| `get_haifu_list` / `get_haifu_data` | ≤50 own archive rows (`pindex` = my seat, `end_date` = ms/1000, 4 name/rank/score slots padded with `CPU`, **no `copy_tid`**); data = recorded `cell_data_N` stream with `cell_sno count=N`, `pnum = seats`, **no `assign_cpu`**; empty tags when no archive (aog.ts:1695-1777) | one fake table with `copy_tid` and a 1-human/3-CPU stub (`assign_cpu>cpu_0..2`, `pinfo_0`, one `cell_data_0 kind=11`) (mock/specs/get_haifu_list.xml, get_haifu_data.xml:9-31) | THEIRS proves the replay shape: with `pnum == GetPlayerNum(gmode)` the `assign_cpu` block is legitimately absent (our own spec comment says we need it because `pnum < 4`). |
| `gget_stamp_info` | real cursor: tid/pindex/mid from `must` slots 3/4/5, `since` = field 2 of the posted `stamp_info`, posts the new stamp (fields 3/4) + random CPU reply, returns `idx > since` (aog.ts:1304-1317) | static one stamp `idx="1"` forever, request ignored (mock/specs/gget_stamp_info.xml:8-14) | Client dedupes by `idx` cursor (`AC/MFG.GameRequest/GGetStampInfo.cs:25`, `AC/MatchingFlow.cs:549-560`), so ours re-serves idx 1 and only the first sticker ever appears. |
| `gchat` | posts the stamp, returns the **whole room** (`since 0`) with idx up to a global counter + 40-entry cap, CPU sticker replies ~45% of posts (aog.ts:221-263,1290-1302) | one canned `d` per poll with `idx = {{seq}}` (per-request counter) and `contents 0` (mock/specs/gchat.xml:10) | Because ours increments `idx` on every request, the client sees a "new" stamp every poll (`SeqID > _stampPosition`); harmless only because `contents=0` = `ItemID.None`. THEIRS' full-room reply is safe because the client filters by idx and ignores its own seat. `time` is epoch **millis** on both (`AC/MFG.Taikyoku/StampData.cs:32`). |
| `end_show` | echoes posted `voltage`/`contribute_percent`/`bonus`, `get_point = floor(voltage/10)` (aog.ts:1270-1285) | canned 1200/100/120 (mock/specs/end_show.xml:9-16) | Same nodes; ours ignores the client's numbers. Not exercised live (0 hits). |
| `end_game` limit fields / `reconnect` | `ste_oya1_limit_time 15000`, `ste_limit_time 10000`, `ste_reechi1_limit_time 15000`, `naki_limit_time 8000`, `agari_limit_time 10000`, `naki_choice_limit_time 8000`, `reechi_choice_limit_time 8000`, `last_cyoukou_limit_time 30000`, `last_time 30000`, absolute `gserv_url http://host:port/aog/`, extra `<gmode>` child (aog.ts:1001-1022,1043-1063) | 5/5/5/5/5/10/10/30/**300**, `gserv_url game1/`, no `gmode` (mock/specs/entry_game.xml:16-25, reconnect.xml:8-26) | The only consumer, `MachingLimitTime` (`AC/MFG.Taikyoku/EntryTakuInfo.cs:63`), feeds a countdown decremented by `Time.deltaTime` with alert at 5 and display cap 99 (`AC/MatchingFlow.cs:223-227`; `AC/CountDownTimer.cs:36,40,50,80-97,238`) — i.e. **seconds**, so their ms-shaped values pin the display at 99 and ours' 300 does too. Neither side shows a real countdown; use ≤99. |
| `chara_enabled`, `competition_entry` | THEIRS is wrong for this client on both: `<chara_enabled><is_gacha_enabled>` wrapped (aog.ts:1904-1907) while the client reads them at root (`AC/MFG.GameRequest/CharaEnabledSend.cs:34-41`); `<competition><entry_result>1</entry_result>` (:1785-1787) while the client reads root-level `entry`/`displayed_result` (`AC/MFG.GameRequest/CompetitionEntry.cs:44-61`) | root-level nodes on both (mock/specs/chara_enabled.xml:4-9, competition_entry.xml:8-16) | Ours is the correct shape; theirs silently fails those two features. Do not "fix ours to match theirs" here. |
| `dojo_get_status`/`set_slot`/`gain_soul` | same node set as ours, per-profile dynamic timers; 4 slots / stock max 3 / lesson 300 s (aog.ts:1322-1358; utils.ts:1126-1128); unknown pcuid ⇒ one shared `GUEST` dojo (:289-296) | same constants, fixed unix stamps, slot 0 always ready (mock/specs/dojo_get_status.xml:9-23) | Constants agree — good cross-check. Ours' "slot 0 always has a soul" is a permanent lie; theirs advances with real time. |
| `keep_alive` / `logout` / session TTL | pure acks; sessions have **no TTL**, invalidated only by the next `login` (aog.ts:475-477,406-409; utils.ts:251-276) — after `logout` the old pcuid still resolves | acks; ours drops the match on `logout`/`end_game` only (mock/MatchEngine.cs:130-140) | Neither expires; ours leaks session entries for the process lifetime. Live client never called `keep_alive` (docs/15 §6.1 says every 60 s in `BreakTimeWindow`) — unverified path. |
| `get_jongstone_info` / `get_mg` | `free_point 0`, `mg 0` (aog.ts:1612-1618) | `free_point 10`, `mg 100` (mock/specs/get_jongstone_info.xml, get_mg.xml) | Ours deliberately lets purchases proceed; theirs has no currency. Both shapes identical. |
| `odekake_done` / `present_done` | odekake hard-disabled (`success 0`; live path returns lender `customize_item`+`player_game`, id = lender **refid string**, order customize→player) (aog.ts:1868-1901); `present_done` returns one `<data>` **per id in `done_ids`** with empty content/amount 0 (:1779-1783) | odekake always succeeds with ids/`state` order reversed (`player_game` first, numeric id) (mock/specs/odekake_done.xml:9-14); one `<data id="1">` for any claim (mock/specs/present_done.xml:8-15) | Ours' present id `1` never matches a claimed PresentID, so the client's `PresentBoxGameData.Apply` skips it; theirs matches per-id. THEIRS' state order/id type is the one to copy for odekake. |

---

## Open questions resolved by theirs

For each docs/15 §11 item: what THEIRS does, with cites. "Silent" = their server never exercises
it either, which is itself useful (it means the shipping client cannot be relying on it).

| Q | Settled by theirs? | Their answer |
|---|---|---|
| 1 — `SUTECHOICES` on every discard? | **Yes** | Only when a *human* can actually call, one cell per claimant, and never a `select=NONE` cell: `_after_discard` builds offers for humans only and otherwise goes straight to `_cpu_calls` → next `TSUMO` (taikyoku.ts:654-678). A human seat that can call always gets `select != 0` (:680-726). |
| 2 — `TSUMO` broadcast with the drawn `pai`? | **Yes** | `_begin_turn` emits one `TSUMO` cell per draw, untargeted-by-default ⇒ `pi0..pi3` all 1, with `<pai>` = the drawn tile id for everyone (taikyoku.ts:401-416, `_cell` default targets :269-277). |
| 3 — `KYOKUEND.end_stat` | **Yes (re-confirms)** | `1` only when `game_over = busted || (last && !renchan)`, else `0` (taikyoku.ts:1259-1262); the client's `end_game` follows the `1` (already live-verified in our own capture). |
| 4 — `SCORERANK(24)` trigger/cadence | **Yes** | Two triggers: immediately after any riichi-declaring discard (`_do_discard` → `_score_rank_cell`, taikyoku.ts:613-614) and once at every kyoku end before `KYOKUEND` (:1254). Body = `kyoutaku` + `riti_after{0..3}{score,rank}` (:623-631) — element names match `AC/MFG.Taikyoku.Command.Receive/ScoreRank.cs:44`. It is *not* a generic mid-kyoku sync. |
| 5 — `kan_type` numbering | **Partly** | TSUMOCHOICES: `1` = ankan, `3` = kakan (taikyoku.ts:472,477) — matching the client's only two handled values. SUTECHOICES: `2` for the minkan offer (:713), and a minkan answer arrives as `MINKAN(8)` resolved from seat+offered tile, never from `kan_pai` (:1425-1429). So `2` = "minshan kan offer" and it is inert client-side. |
| 6 — semantics of the ignored inner fields | **Yes (server-side meaning)** | `oya` = dealer seat, drives wind/rank/settlement (taikyoku.ts:222-240,1142-1147); `sai` = two decorative random 1-6 (:318); `rencyan` = always 0 (:323); `chicya` = 0 (:316); `yama`/`rinshan` = the real wall/rinshan order, sent but only used server-side (:329-331); `SUTECHOICES/pindex` = the **discarder** (:699); `naki` = mirror of the PON/CHI/KAN bits (:684-695). |
| 7 — `chi_pai` ordering per `chi_naki` | **Yes (as far as it goes)** | The wire carries only **the two concealed tiles, ascending by kind** (`_chi_options` returns ascending pairs, taikyoku.ts:639-648; cell body :798, `_ints` :64-68) and no `chi_naki` field at all; the meld shape is rebuilt from the pair contents, and their own resolver matches chi by sorted-pair comparison (:1437-1449). So: no positional convention exists to honor. |
| 8 — dealer `tepai` 13 or 14 ids | **Yes** | 13 for every seat including the dealer; the dealer's 14th arrives as the first `TSUMO` (`start_kyoku` slices 13 per seat, then `_begin_turn(oya)` draws, taikyoku.ts:305-311,333-357,360). |
| 9 — `available="0"` semantics | **Yes** | "nothing new for this cursor": emitted exactly when `cursor >= cells.length`, and in that case **without a `cell_sno` element** (taikyoku.ts:279-292). Not maintenance/compaction, and safe because `available != 1` short-circuits before `cell_sno` is read. |
| 10 — reconnect element naming + trigger | **Partly / negative** | THEIRS implements neither production path: no `recon` in any `gpost` answer, `S_RECONNECT(14)` unhandled, and its `/reconnect` endpoint returns a fresh-looking `<entry>` (18 children incl. `gmode`, `next_sno 0`) or falls through to `entry_game` (aog.ts:1026-1069). So `player{i}` vs `player_info{i}` inside `recon` is still unresolved — but it's now known that a working provider can skip reconnect entirely. |
| 11 — does the server validate `must=` / `tepai_pos`? what codes on reject? | **Yes** | No validation of seq (slot 6 is never read; the answer slice is taken from `cells.length`, aog.ts:1203), no validation of `tepai_pos` (slot 8 unread), and `pindex` is overridden by the server's seat record (:1205). Legality is enforced *functionally* instead: a discard not in hand silently becomes the drawn tile / last tile (taikyoku.ts:582-591), a bad pon pair falls back to `[tile,tile]`, a bad chi falls back to the first legal option (:1421-1445), and an illegal agari just re-offers the window (:1298-1330). **Rejection codes do not exist: `serv_st/code` is always 0.** |
| 12 — `gchat` during play | **Yes (negative)** | THEIRS' `gchat` is scene-agnostic and its stamp room is keyed by `tid` (aog.ts:1290-1302), but its `gpost` answers suppress chat with `since=1e9` (:1213) and the client stops `gget`ting during play — so mid-match stamps are **not delivered** by a working provider. Confirms the recovered-code reading: no mid-match stamp send path is needed. |
| 13 — `RANNYU(19)` | **Silent** | Never emitted; not even defined in their kind constants (taikyoku.ts:17-32). Consistent with the client having no parser. |
| 14 — units of the `entry_game` limit fields | **Partly** | THEIRS writes ms-shaped numbers (15000/10000/8000/30000, aog.ts:1009-1017), i.e. it treats them as ms. The only client consumer is the matching countdown, which ticks in **seconds** (`AC/CountDownTimer.cs:238` `m_time -= Time.deltaTime`; alert 5 s :36; display cap 99 :50) — so theirs freezes the digits at 99 and ours' `last_time 300` does too. Practical answer: seconds, ≤ 99. |
| 15 — rank fields 0-based | **Yes** | `_ranks()` assigns 0 to the highest score, tie-broken dealer-relative (taikyoku.ts:363-375); `mgresult.player_i/rank` and `player_info_i/rank` use the same 0-based values (:355,1460-1474). Ours matches (mock/MatchEngine.cs:747-754). |
| 16 — renchan dealer handling | **Yes** | Dealer keeps dealing exactly when the **dealer** was tenpai at the exhaustive draw (or on an abortive draw): `renchan = abortive || tenpai.includes(oya)` → `honba += 1`, `advance_kyoku = false` (`kyoku_index`, `kyoku`, `oya`, `ba`, `all_last` unchanged) (taikyoku.ts:1249,1253-1277,222-236). A non-tenpai dealer (even with other seats tenpai) → `honba = 0` and the kyoku advances. **Ours is wrong on both halves** (mock/MatchEngine.cs:611,625-638 grants honba on any-tenpai and rotates the dealer). |

New questions theirs raises that docs/15 doesn't ask:

- N1. `kyoutaku` accounting contract: is the 供託 subtracted from the declarer at declaration
  (theirs: `score −= 1000` at discard, taikyoku.ts:606) or only at settlement? Either works if
  `calc_score.before_score` is consistent with it, but a mock mixing the two will show double
  deductions. Theirs' invariant: `new_score == before + yaku + kyotaku + tumifu` for all four
  seats, always (`_calc_score_xml`, :1117-1135) — worth stating as a self-test rule.
- N2. `gmode` → (seats, kyoku count, start score, wall kinds) as one table: theirs'
  `GMODE_TAKU`/`SEATS_OF`/`KYOKU_COUNT`/`START_SCORE` + sanma (M2-M8 removed) / nima (no manzu,
  no West/North) tile filtering (aog.ts:75-100; mahjong.ts:67-108) is a complete answer to
  docs/15 §8.4's "keep `gmode` and the dealt tiles consistent". Nima dora-indicator wrap
  (South→East) is handled too (mahjong.ts:146-166).
- N3. `pnum`/`cpu_num` ⇒ required `mend` child count (`PlayerNum = pnum + cpu_num`, loop
  unguarded, `AC/MFG.Taikyoku/MatchingData.cs:35,76-80`) and `pnum` ⇒ `epdata_{0..pnum-1}`
  (:52-60). Docs/15 §1.5 mentions neither coupling.
- N4. Is `client_states` inside `mend/player_i` the mechanism by which the client renders *other*
  players' characters/cosmetics? THEIRS sends `player_game` + `customize_item` blobs for each
  human (aog.ts:105-114); the client rebuilds `PlayerGameData` from them and takes name/dan from
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
    theirs' PlayerName mirror, aog.ts:828-846) so continue-flow state round-trips are actually
    tested; add `gmode` support for sanma/nima with theirs' seat/kyoku/tenbo/wall tables and the
    `player_info{i}` filler for unused seats (taikyoku.ts:344-349).
11. **Session hygiene**: expire `sessions` entries (ours leaks them; mock/MatchEngine.cs:83,130-140),
    and if we ever want theirs-comparable identity, key state by uppercased `user_id` (= their
    `refid`) and issue an opaque 32-hex `session_id` at login (aog.ts:361-404).
12. **Accept-and-route names ours lacks**: `get_record` (alias of `player_record`) and
    `coop_done` — today both fall to `_default` (mock/Program.cs:217-239).

Deliberate non-changes (ours is the correct side): root-level `chara_enabled` and
`competition_entry` shape; `serv_st/code` only (root-level `<code>` is inert but harmless);
dealing red fives; `last_time` in seconds; `breakdown`-rich `mgresult` (client-optional, useful
for our exp/affinity display tests).

---

## What to change in docs/15

1. **§2.3/§3.4 — flag `SELECTABLE_TYPE_FLAG` usage as the single most error-prone constant pair**:
   add the two worked values (`TSUMOCHOICES` win-possible = `0x400|0x40`;
   `SUTECHOICES` ron = `0x80`(+0x1)), and note that `SUTECHOICES.naki` mirrors the PON/CHI/KAN
   bits while `SUTECHOICES.pindex` is the **discarder**, both server-informational
   (taikyoku.ts:684-699).
2. **§3.3 `SCORERANK(24)`** — replace the `UNCONFIRMED` with the observed provider cadence and body
   (riichi + kyoku end, `riti_after{i}{score,rank}` + `kyoutaku`) and keep one caveat: this is
   what the  <provider> provider does, not proof about Konami's server.
3. **§3.4 `ScoreData`** — document the invariant `new_score = before + yaku + kyotaku + tumifu`,
   that 供託 is debited at riichi declaration by the live provider, and that `tumifu_score` is the
   honba transfer (100/seat tsumo, 300 ron). Also record `yaku_han` → `MFG.Types.Han` tier mapping
   with the `han_rank` cut-offs, since docs/15 §3.4 currently just says "`yaku_han`".
4. **§7.1 KYOKUSTART** — settle the `renchan` row: dealer-tenpai (or abortive) ⇒ honba+1 with
   `oya`/`kyoku`/`ba` unchanged; `kyoku = index % seats`, `ba = index < seats ? 0 : 1`,
   `all_last = index ≥ KYOKU_COUNT[taku] − 1`; `KYOKU_COUNT` = 4/8/3/2 and start scores
   25000/25000/35000/35000; dealer `tepai` is 13 ids; `sai` is decorative.
5. **§6.3 `entry_game`** — units note: `last_time` is consumed as **seconds** with a 99-display cap
   and 5 s alert (`AC/CountDownTimer.cs:36,50,238`), the  <provider> provider writes ms-shaped values
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
9. **§6.6** — downgrade reconnect to "not implemented by the working provider"; add that its
   `/reconnect` returns an `entry`-shaped table rejoin (with `<gmode>`, `next_sno 0`) and falls
   back to `entry_game`, so the `player{i}` vs `player_info{i}` question is still open and
   currently unexercised.
10. **§11** — mark Q1,Q2,Q3,Q4,Q6,Q8,Q9,Q11,Q15,Q16 as answered by an implementation (theirs),
    Q5,Q7,Q10,Q12,Q14 as partially answered, Q13 as still open, and add N1-N4 from the previous
    section. Add a pointer to this document from §11's header.
11. **§1.1/§6.2 identity** — record that the live provider treats the posted `user_id`
    (uppercased) as the durable identity (`refid`), issues a fresh 32-hex `session_id` per
    `login` with no TTL, resolves profiles session-first then `mid`, mints `mid` as
    `parseInt(refid[0:8],16) & 0x7fffffff` with a djb2 fallback (utils.ts:161-172), and **refuses
    `client_state_write` for `mid == 1`** — i.e. a client that skips `login` gets a profile it
    cannot save to (aog.ts:479-487,729-738; utils.ts:367-394). Cross-check with our own capture:
    the shipping client's minted `user_id` has **Unix-seconds in its high 32 bits**
    (`6ADC2910…` → 1792813328 = the request's own timestamp, `mock/mock.log.txt` seq=6), so
    theirs' `mid` rule is literally `seconds & 0x7fffffff` for guests — stable across launches for
    one DataID, and colliding for two players minted in the same second (their collision repair is
    theirs' mid-collision repair `findFreePlayerId` (utils.ts:301-316, used from `ensureProfile`
    at 322-330).
12. **§8** — note that the live provider never deals red fives (no `+64` ids, mahjong.ts:137-144)
    while the client's enum, art layer and akadora counter clearly support them, and that dora
    indicator wraps differ per taku (nima: South→East) — our red-five mock is the more faithful one.

---

## Notes for using theirs directly

- **Route prefix.** Their plugin registers bare names (aog.ts:1812-1908) via `R.AogRoute`
  (:1913-1933); the `/aog` prefix comes from the core, not the plugin — the core's AOG middleware
  dispatches on the **last URL segment** and only accepts `url.startsWith('/aog')` or a name in its
  43-entry `KNOWN_AOG` list (`tmp/RyuNET-core/src/middlewares/AogMiddleware.ts:13-37`), mounted
  app-wide before the e-amusement layer (`src/AsphyxiaCore.ts:79-81`). A second native listener on
  `mfg_http_port` (22421) applies the same name rule (index.ts:114-203). So both
  `http:// <provider-host>/aog//gget` (the doubled slash the client actually emits, docs/15 §1.1) and
  `/aog/gget` work; `/aog/login/` (trailing slash ⇒ empty last segment) does **not**.
- **Body decoding**: theirs URL-decodes the body with `URLSearchParams`
  (`AogMiddleware.ts:5-11`) and then re-tries four base64 strategies on `client_state_write`
  (aog.ts:747-776) — the `+`-as-space trap is real and already bitten once there (their comment at
  :739-745). Any client that posts raw base64 will corrupt silently under naive form decoding.
- **AOG-only (no card) flow works**, and it is what their guest path is built for: `guest=1` is
  only a flag (aog.ts:362,373); identity is the client-minted `user_id`; `get_menudata` with an
  unknown pcuid still returns mid `1`/`GUEST` (:456-473) but `client_state_write` for mid 1 is
  **refused** (:823 + utils.ts:371-381), so a launcher that never logs in cannot persist. Our
  guest loop (fresh client-minted `user_id` per launch, `pcuid` = the returned `session_id`,
  stable for the whole launch incl. `end_game`) works against theirs — state persists only because
  the same `user_id` re-binds the profile, and each launch gets a brand-new session (aog.ts:361-404;
  `AC/MFG.GameRequest/Login.cs:30-31`). Skipping `login`, or inventing your own `pcuid`, degrades
  to mid 1 / empty states / no records / no seat mapping.
- **Costs of using theirs for protocol experiments**: no `recon`/`RANNYU`/mid-match stamps (those
  code paths cannot be exercised there at all); ~60 s CPU-fill wait for a solo guest
  (aog.ts:1087-1095); tables are keyed by `tid` in a global map with no TTL, and `end_game` marks
  the table finished and deletes only the caller's seat record (aog.ts:1217-1268), so a stale
  table can absorb a later `entry_game` into the same `tid`; no timers means a human who stalls
  mid-kyoku stalls the table until the client's own think clock answers, and a human who
  disconnects mid-claim is only reclaimed by a `gget` > 60 s later (:1099-1141) — which the
  shipping client does not send during play.
- **Their engine's deliberate simplifications to expect in any capture**: no red fives ever
  (mahjong.ts:137-144), so no akadora and no `+64` ids on the wire; no open-riichi/fire-riichi/
  atomic-riichi (`stat` bits 2-4 never set, `reach_flg` treated as boolean — taikyoku.ts:1297);
  `kan_type` `2` in sute choices; `rencyan` always 0; `chicya` always 0; `dora_open` capped at 5;
  abortive-draw support limited to 九種九牌 (`S_KYUSYUKYUHAI` → tenpai-less renchan, :1331-1333);
  multi-ron supported in scoring but CPU→CPU ron is taken by the first seat in turn order (:728-739);
  no `kyuushu` for CPU seats, no 流局満開/四風連打 style abortive draws.
- **Bug-shaped behaviors to copy only knowingly**: `chara_enabled` wrapped and
  `competition_entry` node names (both unreadable by this client, see Panel table); ms-shaped
  `entry_game` limits; `mustInt(parts, i) || def` (a legitimate `0` collapses to the default,
  utils.ts:81-88); the gget chat cursor read from slot 6 of a 6-field `must` (`aog.ts:1179` ⇒
  always 0 ⇒ full stamp room); `moserv_url`/`gserv_url` possibly pointing at the wrong port
  (:341-343,994-999 vs index.ts:117-120).
- **Quick parity checklist for our client-vs-theirs smoke test**: `web_id=1`, `area_id=1`,
  `AppVersion=2.0.2`, `matching_ver=70` (live `appli_info` body is exactly `matching_ver=70`);
  `guest=1`; `passphrase` sent as a single space when empty — theirs trims it or every public
  lobby collapses into one key (aog.ts:859-861); `entry_id`/`passphrase` with their literal
  trailing spaces and `debug_score=-1` (live bodies, `mock/mock.log.txt`); `app_param=0/0`
  constant; `end_game` carries `version=2.0.2` and no `gserv_id` (docs/15 §6.5 confirmed live).
