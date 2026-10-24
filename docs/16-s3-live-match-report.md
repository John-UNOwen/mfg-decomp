# S3 - scripted mahjong engine, live against the shipping client

Date: 2026-10-24. Scope: close the last Option S unknown from docs/12/S1 - can a scripted
`/gget` + `/gpost` command engine drive the real client through actual gameplay?
Verdict: yes, end to end, with zero client exceptions. The engine is `mock/MatchEngine.cs`
(rule core `mock/Mahjong.cs`, in-process harness `mock/SelfTest.cs`: 12 seeded matches,
tsumo 5 / ron 4, all score-conserved: `SELFTEST PASS`).

## What was driven live

Mock topology is the S1 one (hosts pin, `MockServer` on 8080, `-KONAMINET`, BepInEx S1Freeze),
so no card and no provider is involved; the game enters as a fresh guest each run
(the launcher's persisted card options are stripped - see KB runbook spice2x-launcher-state).

Guest join path (all client-side, mock only answers): attract -> guest -> story skip ->
chara select -> Home -> match mode 4-player tonnan -> rate type dokidoki -> rules question
-> tile design -> view type -> matching -> KYOKUSTART. Tap coordinates for the 1920x1080
window are in the KB runbook.

Verified against the live client in one session each:

1. Deal + windows + cascade: KYOKUSTART, TSUMO cells, SUTE window for the guest seat, CPU
   draws/discards, dora display, rivers, tenpai panels.
2. Ryukyoku settlement: exhaustive draw, tenpai payments applied live (guest gained +3000
   with three notai), honba/renchan rollover (kyoku 1 started honba=1 with the next dealer).
3. Agari settlement: with the test-deal hook (see below) a CPU seat tsumos on the forced
   win tile two draws in; the client rendered the agari banner, the yaku/score panel
   (30 fu 1 han, +16,000 points - matching the engine's KYOKUEND line exactly), the score
   update, and advanced to the next kyoku.
4. Match flow: four kyoku -> final `KYOKUEND end_stat=1` -> client `end_game` -> junmei
   happyou (ranking board, uma, rank badges) -> result screens (fight star, affinity +100,
   fan totals) -> Home. Two full matches, zero Unity-log exceptions.

## Protocol findings settled live (details in docs/15 §11)

- `KYOKUEND end_stat`: `1` = match over (client sends `end_game` within seconds); `0` =
  kyoku over, continue. Verified both directions.
- Rank fields are 0-based `Rank` enum values. A 1-based rank 4 threw in `ExecKyoku` on the
  dealer and crashed `ScorePanel.PlayFinal` (`m_ranks[4]`), freezing the junmei screen;
  sending 0..3 fixed both. `kyoku-start player_info rank` is the player's rank tier
  (0 = Rank1 newcomer), not a standing.
- The client echoes its own physical tile ids in `gpost SUTE` (wire ids; red fives plain+64),
  confirming the id space documented in docs/15 §1.3.
- `gchat` polls ~1/s from the match scene (open question §11.12 caller still unidentified).

## Client automation notes (matters for automation, not for the protocol)

- Guest defaults auto-play: auto discard ~10 s per turn; win windows time out (~20 s action
  bar) and are auto-passed - the "instant win" board toggle defaults OFF per fresh guest;
  first-kyoku tutorial balloons swallow taps.
- Free-play emulation (docs/12 mod + this session's patch, `GameUtility.IsFreePlay` -> true)
  is what lets guest entry bypass the unemulated cabinet credit/PASELI path cleanly.

## Known gaps (next iteration, not blocking)

- v1 rules: no chi/pon/kan/riichi, no multipoint yaku math beyond dora, nominal mangan-ish
  scoring, no furiten/kuikae checks, renchan extends east-4, and exhaustive-draw renchan
  rotates seats instead of retaining the dealer (rule-fidelity note docs/15 §11.16).
- First kyoku of the very first live session threw `ArgumentOutOfRangeException` in
  `ExecTsumo.ProcessCommand` at the first CPU tsumo; it never reproduced afterwards (the
  mod keeps a finalizer diag: `[S3DIAG] ... players=4 nokori=...`), and the decompile shows
  the client's seat table is populated before processing - root cause open.

## Repro

`MockServer selftest` (in-process, no client). Live: KB runbook
`s3-mock-match-loop-runbook` - hosts pin, mock with optional `MFG_TEST_TEPAI/WIN/SEAT` env
vars (test deal), `-KONAMINET` launch, tap sequence. Screenshots: `out/s3/agari_*.png`
(banner + settlement panel), `out/s3/final_result*.png`, `out/s3/shot4*.png`.
