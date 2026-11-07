# A replayed call offer answered the window that was live

Package: `tmp/stale_offer_fix`. Engine commit: `asphyxia_plugins 38632a2` (`fix: a call reply answers
only the live offer`), parent `071e6bf`. Ledger: `docs/31-open-defect-ledger.md` L84.

## Scope and how the build was run

No server was started and no lane was staged: everything here drives the engine in-process.
`pwsh C:\mfg\tmp\stale_offer_fix\gate.ps1 -Tag fix` compiles the canonical tree
(`tmp\RyuNET-core\dist\plugins\mfg@asphyxia`, the only tree edited) with the core's own
`node_modules\typescript\bin\tsc` into `C:\mfgtmp\adv_engine\fix\js`, then runs probe_B, probe_D,
probe_E, this package's two checks, and the three offline suites that transpile the tree themselves.
Its whole output is saved as `.audit/stale_offer_replay.out.txt`. `taikyoku.ts` and `handlers/aog.ts`
in the canonical tree and in `tmp\asphyxia_plugins\mfg@asphyxia` are the same files
(`ryunet_stage.ps1 -Action status`: `identical to canonical`; `git diff HEAD` in the plugin repo is
empty), so the commit is the build the numbers were taken from. `handlers/aog.ts` was also typechecked
with `tsc --noEmit`: the only errors in the tree are the pre-existing `process` / `__dirname` ones in
`index.ts` and `handlers/utils.ts`, which come from this ad-hoc invocation not loading `@types/node`.

## What the client does, from the recovered source

A `SUTECHOICES` cell is a question to the seats its `pi` bits name, and the client answers it whether
or not the player touches anything:

- `ExecSuteChoices.cs:74` - the whole cell is skipped unless its `pi` bit for this seat is set. That
  bit is the client's only notion of "this question is for me".
- `ExecSuteChoices.cs:325-328` - naki-off answers the window with a `Tsugi` the moment it opens;
  `:390-391` - otherwise the action bar times out and answers it. Observed live on a guest cabinet in
  `docs/16-s3-live-match-report.md:46-48` (windows time out and are auto-passed).
- `Tsugi.cs` and `Ron.cs` set `Kind` and nothing else, so a decline and a ron name no tile. `Pon`
  (`ExecSuteChoices.cs:142`), `Chi` (`:200`) and `Minkan` (`:219`) are built from the choices cell, so
  they carry the tile being called (`pai_id`, always `suteChoices.Choices.PaiID`) and the hand tile or
  tiles being spent (`tepai_id`, `tepai_id2`; a minkan names one).
- `CommandClient.cs:242-247` - a claim-window answer carries `max(seqNo of every SUTECHOICES in the
  client's buffer) + 1` in `must` slot 6. Not the cell it is answering: the newest one it holds.
- `MatchingFlow.cs:218` seeds the cell cursor from `entry/next_sno`, and `CommandClient.cs:253-262`
  `Reset()` clears `m_receivedCommands` (so the dedupe at `:126-134` cannot filter a replay) and turns
  data transmission on; only a kifu `Apply()` (`:286-292`) turns it off.

`422a975` made `entry/next_sno` the `KYOKUSTART` of the kyoku the table is in, which is what a
returning client needs to rebuild that kyoku (see `docs/31` L76). It follows that the stream a
rejoiner is served contains the offers it answered before it dropped - and it answers them again, for
real, into a table that moved on.

## Finding 1 - the reply path keyed a reply on the seat alone

`_handle_sute_reply` used to ask one question: `if (!this.sute_choices[seat]) return;`. A seat owed an
answer to exactly one window, so it took the reply for that window's answer. That is only sound while
the seat's next reply can belong to one window. It is the seat, not the offer, that the reply is
matched to, and a seat that is behind the live edge has old offers of its own on its screen.

The corruption, from probe_D on `071e6bf`: seat 1 holds two 5s and two 6s. Window A offers it a 6
(declined, resolved). Window B offers it a 5 (live). Its replayed answer to A arrives while B is open.

- a `PON` naming A's tile and A's hand tiles (`pai_id` = 6-man, `tepai_id` = `tepai_id2` = 6-man) was
  applied to B: `melds1=[pon 5/5/5]`, the seat's pair of 6s deleted from its hand, both 5s still in
  the hand it did not spend (`D7`, `D8`, `D9` FAIL).
- the same through `/gpost`, at the transport, is `verify_aog.cjs` G3 pre: `melds=["6/6/6"]` with the
  hand unchanged - a triplet of the live tile built out of nothing, because
  `_resolve_sute_choices` fell back to `own = [tile, tile]` at `taikyoku.ts:1644-1646` and `_apply_pon`
  splices what it finds.

Now: `_answers_live_offer` (`taikyoku.ts:1568-1592`) runs before the reply is recorded
(`taikyoku.ts:1594-1601`), and refuses a reply that provably belongs to another discard - a
`PON`/`CHI`/`MINKAN` whose `pai_id` decodes to a tile other than the live `pending_call.tile`, a `PON`
naming hand tiles other than the live tile's pair, and any claim-window reply whose `seqNo` names a
choices cell older than the live window's own cell. `handle_gpost` reads `must` slot 5 and passes it in
(`handlers/aog.ts:2989`, `:3014`); `on_command` gained the optional eighth argument (`taikyoku.ts:1441`)
so callers that drive the engine directly leave it unknown rather than guessing `0`.

Refusing is the safe direction because it leaves the window and the seat's offer untouched: the seat
still owes a real answer, and the ways that wait already ends are untouched - the client answers the
live cell when it reaches it, `handle_gget` converts a seat that goes quiet, `resign()` drives the gate
(the rule `docs/31` L75 fixed). `verify.cjs` F13 shows a refusal followed by the seat's real pon
landing normally.

## Finding 2 - the engine published offers it no longer stood behind

The stale `PON` can be refused because it names something. A `NAKINASHI` names nothing, and no reply
rule can catch it, so the fix for the decline half has to be where the offer is handed out.

`cells_from` used to hand back the log slice unchanged, so a rejoiner got `pi1="1"` on an offer the
engine had resolved cells earlier - which, by the client's own rule at `ExecSuteChoices.cs:74`, means
"you owe an answer to this". Now `_publish` (`taikyoku.ts:384-391`) clears the `pi` bits of a
`SUTECHOICES` cell that is not one of the live window's, and `_live_offer_seqs` (`taikyoku.ts:361-371`)
derives the live set from `sute_choices` itself, which is already the wait set: an offer leaves it when
the seat answers, when the seat stops being a human seat, and when the window resolves. No second
record of which offers are past, so nothing to keep in sync. `offer_seq` (`taikyoku.ts:834`) is what
ties an offer in `sute_choices` to the cell it was published under.

The log itself is untouched: `this.cells` is what `/gpost` answers from, what the match archive copies
(`handlers/aog.ts:3126`), and what a kifu replays. The cell stays where the log put it and only stops
addressing anybody, and the client walks its cells in order either way. `verify.cjs` F4 asserts the log
is byte-identical to the pre-fix log for the same seeded table; F5 asserts the stream is the pre-fix
stream with exactly the resolved offers made inert and nothing else changed.

## Why probe_D's D4 stays red

`tmp\adv_engine\probe_D.cjs:94` posts the replayed decline straight into the engine:
`t.on_command(S_NAKINASHI, 1, 0, 0, 0, 0, 0)`. `probe_D.cjs:60` posts the seat's legitimate decline of
window A with byte-identical arguments, and D0 and D1 require the engine to accept that one. The reply
has three tile fields to say anything with and all three are `0`, and its eighth argument, the `seqNo`,
is not passed at all. `D4` asks the engine to take one call and refuse the other.

`tmp\stale_offer_fix\dump_state.cjs` prints everything `on_command` can see at those two moments. Every
field is the same shape - `state=call`, one offered seat (`choice_seats=1`), `sute_replies={}`, a bare
NAKINASHI with `pai=tepai_id=tepai_id2=0` - and the fields that differ only carry the live window's
identity: `pending_call 6@0` vs `5@2`, `offer_for_seat1` for 6 vs 5, `turn 0` vs `2`, `cells_len 11` vs
`16`, `discard_count 1` vs `3`. In both moments the seat is legitimately owed exactly one answer for
exactly one live window, and in the stale moment the live offer is also the last cell in the log, so
"is the answer about the newest cell" separates nothing. A rule that refuses the second call and accepts
the first has to key on one of those differences, and each of them describes ordinary play: declines
arrive at every `discard_count`, on every turn, about every tile. Refusing a bare NAKINASHI from a seat
that owes one, whenever that seat has answered a call window earlier in the kyoku, would hang the
engine's own table (a kyoku has several windows per seat, and `probe_B` R5 needs the second one).

So `D4` is red by construction, and the honest claim is the one `verify.cjs` proves instead: the client
that produces that reply never gets one. It models the recovered client over the stream the cursor
hands it - walk the cells, answer each choices cell addressed to this seat with what the recovered code
posts, with `EvaluateGpostSeqNo`'s number. Pre-fix (F6) it posts two answers, `posted=[PON@10, PON@15]`
- the first about an offer resolved five cells earlier, and it melds `5/5/5` while the seat's pair of 6s
vanishes. Fixed (F7) it posts one, `posted=[PON@15]`, and the pon is the live tile, spent from the
pair it was offered for (F8), with the seat's tile count intact (F9) and the table continuing (F10).
Where a stale reply can still be named by its seqNo - the shape a real client produces when the live
window opened after its last fetch, so the newest choices cell in its buffer is the old one - the engine
does refuse it, and D4's scenario run that way goes the other way: `verify.cjs` F14 refuses the reply and
keeps the window at `5@2`, F15 applies the same seat's answer once it carries the live window's number
and the table moves on to `5@3`.

## Verification

`.audit/stale_offer_replay.out.txt`, produced by the command at the top. Counts:

| check | result |
| --- | --- |
| `probe_B` (call-window and inter-kyoku gates, honba, resume cursor, CPU-only match) | `checks=25 fails=0` |
| `probe_D` (the reviewer's probe) | `checks=9 fails=1` - `D7`, `D8`, `D9` now PASS; `D4` red, see above |
| `probe_E` (a ready answering the next kyoku's wait) | `checks=4 fails=2` - `E3`, `E4` red, unchanged from `071e6bf` |
| `tmp\stale_offer_fix\verify.cjs` (stream + client walk, head build as control) | `checks=18 fails=0` |
| `tmp\stale_offer_fix\verify_aog.cjs` through `/gpost` | pre `checks=7 fails=4` → after `checks=7 fails=0` |
| `tmp\seat_liveness\verify.cjs` | `14/14 checks pass` |
| `tmp\kiken_fix\verify.cjs` | `31/31 checks pass` |
| `tmp\waitingroom_fix\verify.cjs` | `31/31 checks passed` |

`probe_D` against the pre-fix build the reviewer left in `C:\mfgtmp\adv_engine\head\js` (`checks=9
fails=4`), the three corruption checks verbatim:

```
FAIL  D7 a call replayed from the old offer is not applied to the live discard  [melds1=[pon:5/5/5] hand1=[2,4,5,5,8,12,14,16,18,20,32]]
FAIL  D8 a pon consumes the called tile from the caller own hand  [meld=5/5/5 fives still in seat1 hand=2 (was 2)]
FAIL  D9 the seat keeps the tiles it did not call with (its pair of 6s)  [sixes left in seat1=0]
```

The same three against the fixed tree, from `stale_offer_replay.out.txt`:

```
PASS  D7 a call replayed from the old offer is not applied to the live discard  [melds1=[] hand1=[2,4,5,5,6,6,8,12,14,16,18,20,32]]
PASS  D8 a pon consumes the called tile from the caller own hand  [meld=none fives still in seat1 hand=2 (was 2)]
PASS  D9 the seat keeps the tiles it did not call with (its pair of 6s)  [sixes left in seat1=2]
```

`D4` prints the same evidence in both runs, which is the point of it: the reply it posts is
byte-identical to the legitimate decline earlier in the same file.

The red lines that matter, verbatim, from the pre-fix `/gpost` run:

```
FAIL  G2 /gpost hands the client seqNo to the engine, and a decline naming an older window is refused
        state=call choices=[0] pending=2@3 (live 6@2)
FAIL  G3 a PON naming another discard is refused and spends nothing
        melds=["6/6/6"] hand=26,27,28,29,0,2,4,8,10,12,6,6,36 (was 26,27,28,29,0,2,4,8,10,12,6,6,36)
FAIL  G5 an offer that has been answered travels in the stream without addressing the seat again
        resolvedOfferSeq=4 stillAnswerableForSeat=[4] currentLiveOfferSeq=-1 cellsOfSeatOffer=1
```

and the same three against the fixed tree:

```
PASS  G2 ... state=call choices=[1] pending=6@2 (live 6@2)
PASS  G3 ... melds=[] hand=26,27,28,29,0,2,4,8,10,12,6,6,36 (was 26,27,28,29,0,2,4,8,10,12,6,6,36)
PASS  G5 ... resolvedOfferSeq=4 stillAnswerableForSeat=[] currentLiveOfferSeq=-1 cellsOfSeatOffer=1
```

`probe_B` I2e ("a human pon is applied") drove one round of editing. It posts
`on_command(5, 1, 0, 0, 0, 0, 0)` from a seat that was offered a CHI only, and the pre-fix engine
applied that as a pon by inventing the pair. The check is a fixture artifact, not a protocol
expectation: the client can only post a `PON` when the engine set `F_PON` in the offer. The guard was
narrowed so a reply that names no tiles keeps the fallback the resolve path always had, and I2e passes
unchanged. A reply that names tiles - which is every reply a real client posts - is checked.

## Judgement calls worth knowing

- The `pi`-bit clearing is done for every reader, not per seat. A resolved window is resolved for
  everybody, so there is no per-reader variant to build and `cells_from` needs no seat argument.
- A `RON_AGARI` reply names no tile (`Ron.cs`), so the tile check cannot cover it. It is covered by
  the publish rule (the seat is never handed the old question again) and by the seqNo floor. Left as
  the residual it is; the engine's `_win_result` still has to agree the seat can win on the live tile.
- A reply with no seqNo is accepted on the floor that it is unknown, not provably stale. That is what
  keeps `on_command`'s existing callers working - the CPU paths and `kiken_fix`/`waitingroom_fix`,
  whose `gpost` helper posts `0` in slot 6 - and refusing them would refuse probe_D's legitimate
  decline in the same breath.
- The seat number that answers is still the one the server assigned at entry (`handlers/aog.ts:3012-3013`);
  nothing here touches that.

## Left as it is

- `probe_E`'s E3/E4: `S_NEXT_KYOKU_READY` is added to `next_kyoku_ready` with no state guard, so a ready
  from one kyoku pre-answers the next kyoku's wait. It fails identically on `071e6bf` and on the fixed
  tree - a different gate, a different fix, its own work package.
- `_apply_pon` spending fewer tiles than the meld claims, when a reply names no tile ids at all. The
  narrow guard keeps that path's old behaviour on purpose (see I2e above); closing it properly means
  deciding what the engine owes a client that names nothing, and probe_B I2e currently rewards not
  deciding.
- `recon` (`S_RECONNECT(14)`) is still unimplemented on the server, so a rejoiner always re-plays its
  kyoku instead of being shown its state - the reason this whole shape is reachable at all
  (`docs/31` L76, `docs/15` §5.5).

