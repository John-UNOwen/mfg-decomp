# Match lifecycle and records (P6) — root cause, fix, verification

Scope: the self-hosted plugin `tmp/asphyxia_plugins/mfg@asphyxia`. "client" means the
decompiled game under `api/recovered/Assembly-CSharp`, cross-checked against the request
mock in `mock/` (`MatchEngine.cs`, `specs/*.xml`). Nothing is committed.

## How it was run

The core loads plugins through ts-node (`transpileOnly`) from its `dist`, so a plugin edit
only takes effect after re-copying and restarting:

```
Copy-Item -Path tmp\asphyxia_plugins\mfg@asphyxia\* -Destination tmp\RyuNET-core\dist\plugins\mfg@asphyxia -Recurse -Force
cd tmp\RyuNET-core\dist; node AsphyxiaCore.js        # AOG 127.0.0.1:8083, native 22421
```

Every claim below is from a probe run against that server, before and after the change.
Probes added: `.audit/probe_match_p6.py`, `.audit/probe_match_multi.py` (modes
`tid|kiken|lobby|stamp|reconnect|record|replay`), `.audit/probe_scramble.py` (`off` mode),
plus the read-only DB peeks `.audit/inspect_archive_rows.py` and `.audit/peek_match_db.py`.

---

## (a) The match endpoints ignored the table the client addressed

**Client side.** Every match request names its table. `GGet.cs:30` sends
`must = AppVersion/AppSessionID/TakuID/PlayerIndex/PlayerID/nextSeqNo` — six segments, the
third being the table id. `GPost.cs:44` repeats `ver/pcuid/tid/pindex/mid/seqNo/kind/...`.
`EndGame.cs:22` and `KikenGame.cs:22` post `web_id&area_id&pcuid&tid&pindex&mid&version`.

**Server side, before.** None of it was consulted. `handle_gget` looked the seat up by
session only, and when the session had no seat it invented one (tid 1, pindex 0) and
answered a normal mwait. `handle_gpost` answered `<game></game>` with code 0 for a tid with
no table behind it. `handle_end_or_kiken` settled a table that did not exist by calling
`mgresultXml(null, { gmode: 1 })`, whose null-table branch fabricated four seat rows from
`GMODE_TENBO[gmode] || 25000` — the 4x25000 settlement. `PLAYER_SEAT` and `SHARED_TABLES`
are process memory, so this is exactly what a client sitting in a match does after a server
restart: it keeps polling a table that is gone, and the server keeps pretending otherwise.

**Fix.** `addressedSeat()` (aog.ts:338) resolves the seat from the session *and* the
addressed tid, logs the mismatch it saw, and returns null when nothing is seated at that
tid; `gget`, `gpost`, `end_game`, `kiken_game` answer `SERV_ST_NO_TABLE = 770`
(utils.ts:81) instead. 770 is the code the client already has a path for:
`RequestApiBase.ParseResultStatusCode` throws `NetworkResponseException` on any non-zero
code (`RequestApiBase.cs:111`), and these four endpoints are sent with
`enableErrorScreenTransition: true` (`CommandClient.cs:123,178,187`; `MatchingFlow.cs:216`),
so the client leaves the match rather than staring at a phantom table. A seated player
polling its own tid is untouched; a claimed `pindex` that is not the seat the account holds
is answered with the real seat (logged, not rejected).

## (b) Archive tids collided across restarts, and any account could read any kifu

**Root cause.** `nextTid()` was `_matchTidCounter++` seeded at 1000 per process while
`match_archive` documents live in the plugin DB. `inspect_archive_rows.py` on the running
DB showed tid 1001 carried three different archives, and a second pre-fix probe run after a
restart reused 1005 and 1006 on top of the matches already stored under those numbers.
`get_haifu_data`, `get_haifu_list` and `player_record` all key on tid, so a kifu lookup
answered a different match. `get_haifu_data` filtered nothing: any logged-in session could
pull any tid, and `player_record`/`get_haifu_list` matched rows on `mid` alone — with
`entry_game` stamping `mid = profile?.player_id || 1` onto every seat (aog.ts, pre-fix), an
anonymous guest match landed in the records of whoever legitimately holds mid 1.

**Fix.** `allocateTid()` (utils.ts:549) takes tids from a counter seeded once per process
from `max(persisted match_meta/tid_counter.last_tid, max tid across match_archive)`, and
writes the high-water mark back to the DB on each allocation. Nothing is awaited between
reading the counter and writing it, so two matches opened in the same tick still differ.
Attribution lives in one place: `ViewerIdentity`, `archiveParticipantMatches`,
`archiveParticipantSeat` (utils.ts:569-609) behind `myArchives()` (aog.ts:2374), used by all
three record endpoints. `refid` is authoritative; a row without one is accepted only on a
mid above 1; a row that recorded guest-ness opposite to the viewer's is rejected.
`get_haifu_data` serves only a participating archive of that tid and returns an empty body
otherwise. Archives now record `refid`, `guest` and mode per seat (aog.ts:1864-1884).

## (c) Every archive said gmode 1

`entry_game` never kept the mode it was entered with, the archive wrote `gmode: 1`, the
null-table settlement hardcoded `gmode 1`, and `get_haifu_list` hardcoded `taku_class 1`.
Entry now resolves the mode once (`resolveGmode`, gmode 0 is not a valid entry value) and
carries it on the seat, on the table (`ensureSharedTable(..., { gmode, taku_class, scramble
})`), into the settlement (`mgresultXml(table)` reports the recorded mode), into the archive
(`gmode`, `taku_class`) and into the record window, where matches are filed by
`recordKindOf(seats)` → Yonma / Sanma / Nima (`RecordKind.cs`).

## (d) entry_game limits were millisecond-shaped

Pre-fix literals: `ste_oya1_limit_time 15000`, `ste_limit_time 10000`, `last_time 30000`.
`EntryTakuInfo.cs:54-63` reads each of the seventeen entry children with `MustNeedElement`
and `ValueInt`, and `MatchingFlow.cs:223-227` feeds `last_time` straight into
`GetTimer().SetTimer(...)`. `mock/specs/entry_game.xml` carries seconds (5/5/5/5/5/10/10/30/300).
`ENTRY_LIMIT_SECONDS` (aog.ts) now emits 20/15/20/15/15/30/30/30 seconds with
`last_time 60`, which is also the interval after which a waiting lobby is forced into a
table, so the client's matching countdown and the server agree. One `entryXml()` builder
now serves both `entry_game` and `reconnect` (the duplicated inline copy is gone;
`Reconnect.cs:26` parses the same `entry` block).

## (e) player_record had no total and no scramble counts

`PlayerRecordGameData.Parse` reads `<total>` for the aggregate, `PlayerRecordInfo.cs:40-49`
reads `g2_scramble` per `info`, `PlayerTotalRecordInfo.cs:15-25` reads
`total>g2_scramble>history>{belong,win_nr}`. The server emitted neither. `g2ScrambleXml()`
(aog.ts:2400) now emits the block in `<total>` and in every `<info>`, always, so the trophy
panel reads a real 0 instead of a missing record; each `<info>` also always carries
`<histories>` (`PlayerRecordInfo.cs:50-52` dereferences it without a guard).

A scramble match is only counted when it was designated as one: the client says
`teammatch=(int)TeamType` (`EntryGame.cs:60`) but never says which school, and nothing in
the client's data maps a character to a school (`CharaProfile.SchoolID` comes from master
data the server does not have). So designation needs the operator's event setting
`VFG_SCRAMBLE_SCHOOL` (School001..School010, `SchoolID.cs`) declared in `index.ts`, empty by
default. `mend>g2event>e>id=1` is then announced for designated matches
(`MatchingData.cs:87-104` → `IsScrambleMatch`), `mgresult>g2result>r>{id,sub_cate,win}`
carries the school and this player's own result taken from the real settlement rank
(`MahjongResultInfo.cs:168-181`), and each archive row keeps `scramble {belong, win}`. With
the setting empty, nothing about a match changes.

## (f) gget read a stamp cursor the client never sends

`handle_gget` built the chat block with `stampXml("chat", tid, mustInt(parts, 6, 0))`.
`GGet.cs:30` sends six must segments, so segment 6 was always absent, the cursor was always
0, and every poll re-sent the whole stamp backlog. `handle_gpost` passed a cursor of
1000000000 (deliver nothing) and `handle_gchat` passed 0 (deliver everything, again).
The client keeps its own position — `StampData.cs:29` takes `SeqID` from the row's `idx`
attribute, and `MatchingFlow.cs:549,560` draws only rows above `_stampPosition` then advances
it — so the server has to remember it per seat: `STAMP_CURSOR` keyed on `tid|pcuid` with
`stampCursor` / `stampCursorMark` (utils.ts:504-516), used by `gget`, `gpost` and `gchat`.
`gget_stamp_info` is unchanged: `GGetStampInfo.cs:25` posts `stamp_info=RoomID,SeqID` and the
server honours that cursor.

---

## Verification

Reproduction before the fix, against the pre-fix staged build
(`python .audit/probe_match_p6.py --tag before`, exit 1):

```
FAIL gget unknown tid answers no-table        code=0 mwait=yes
FAIL gpost unknown tid answers no-table       code=0
FAIL end_game unknown tid answers no-table    code=0 synthesized_rows=4 mgresult=yes
FAIL kiken_game unknown tid answers no-table  code=0 mgresult=yes
FAIL entry limit times are seconds (<=99)     {ste_oya1_limit_time:15000, ... last_time:30000}
FAIL seated gget with foreign tid is refused  code=0 cells=3
FAIL stamps re-sent                           first=[2] second=[2] third=[2]
FAIL mgresult reports the entered gmode       mgresult gmode=1 (entered 4)
FAIL archive records the real gmode           archive gmode=1
FAIL archive records taku_class               keys=[cells,collection,gmode,players,seats,tid,timestamp]
FAIL player_record emits <total>              total=no
FAIL player_record g2_scramble                blocks=0
FAIL get_haifu_data refuses a non-participant peeker cells=30
FAIL card account inherits a guest match      list tids=['1007','1006','1005','1004']
FAIL sanma settlement / archive               gmode=1 (entered 3)
     archive tids=[...,1001,1001,1001,1004,1005,1006,1007,...] duplicates=[1001,1005,1006]
```

After the fix, same probe on the rebuilt server (`--tag final`, exit 0): **23 checks, all
pass.** Highlights:

```
PASS gget/gpost/end_game/kiken_game unknown tid   code=770, synthesized_rows=0, no mgresult
PASS entry limits                                  20/15/20/15/15/30/30/30, last_time 60
PASS seated gget with a foreign tid refused 770; its own tid answers code 0 (tid 1023)
PASS every stamp reaches the seat exactly once    gchat=[1] gget1=[] gget2=[] dupes=[]
PASS nima (gmode 4): mgresult gmode 4, 2 player rows, archive gmode 4 + taku_class,
     players=[('GUEST', 1134589128, 'P6NIMA', True), ('CPU', 0, '', False)]
PASS player_record: total yes, g2_scramble blocks 4 (one total + one per info)
PASS get_haifu_data: participant 273 cells, non-participant 0
PASS own list has tid 1024; MFGTESTGUEST1 and P6PEEPING lists do not contain it
PASS sanma (gmode 3): settlement gmode 3, 3 rows, archive gmode 3 seats 3
```

Restart collision, separate run: `probe_match_multi.py tid` — archived max and counter both
1014 after a restart, new match got **1015**, unique; counter document in the DB is
`{collection: match_meta, kind: tid_counter, last_tid: 1015}`. A second restart repeated it
(1021 → 1022). Legacy duplicates 1001/1005/1006 are still in the DB as data; no new
allocation collides.

No-new-problems checks, all green on the same build:

| probe | result |
|---|---|
| `probe_match_multi.py kiken` | kiken answers a real settlement (2 rows), writes no archive, later gget on that tid refused 770 |
| `probe_match_multi.py lobby` | two accounts, one passphrase: same tid, seats 0/1, both polls code 0, mwait lists both humans, A finishes the table after B goes silent, B's end_game still answers, archive names both refids, both lists contain the match, an unrelated account gets no kifu |
| `probe_match_multi.py reconnect` | same tid and seat handed back, limits in seconds too, match continues, a claimed seat that is not yours is answered with your real seat |
| `probe_match_multi.py stamp` | one lobby table, two seats: each sticker delivered exactly once to both seats, `gget_stamp_info` honours the client's `SeqID` cursor |
| `probe_match_multi.py record` | Nima match filed under `Nima` with a 2-entry `rank_list`, sanma under `Sanma` with 3, every info carries its own non-empty `<histories>` |
| `probe_match_multi.py replay` | list row carries gmode 3 / seat 0, kifu `core_io` gmode 3 pnum 3, 534 cells served = 534 recorded, 4 `zaseki` |
| `probe_match_p6.py` lobby-wait path | a seated player polling while the lobby has no table yet keeps getting code 0 until the 60 s force |
| `probe_scramble.py` with `VFG_SCRAMBLE_SCHOOL=School003` | mwait `g2event`, `g2result ('School003','0')`, archive `scramble {belong:'School003', win:0/1}` per seat, total and info both count it |
| `probe_scramble.py off` (setting empty) | a `teammatch=1` entry stays an ordinary match: no g2event, no g2result, no scramble history |

Type check: `.audit/ts_check_plugin.js` reports `ok` for all six plugin sources, and
`tsc --noEmit` over the plugin entry `index.ts` (which pulls in `handlers/aog.ts`,
`handlers/utils.ts`, `mahjong.ts`, `taikyoku.ts`, `handlers/eamuse.ts`) exits 0.

---

## Judgement calls worth knowing about

- **770 (DataNotFound)** was chosen for a table that no longer exists. Any non-zero code
  produces the same client outcome (leave the match); 770 is the semantically right one and
  the only thing it changes is what the log and the client's error text say. 630 and 930 were
  the alternatives.
- **mid is not an addressing key.** The session (`pcuid`) decides whose seat this is; the
  mid a request claims is ignored, and the archive takes the mid the seat actually has.
- **Legacy archives.** Rows written before a seat recorded its refid are matched on mid > 1
  only. A genuine mid-1 legacy row can now be missing from that account's list. That is
  deliberate: mid 1 was `entry_game`'s placeholder, so it is not evidence of participation.
- **Tables are not deleted on end_game.** The `archiveSaved` flag on the table object is what
  keeps a second human's end_game answering a real settlement instead of a duplicate archive.

## Left as it is

- `gchat` still accepts a tid with no seat behind it: a stranger who knows a tid can add a
  sticker to that table's backlog. Not in this fix's acceptance list, and the client sends
  gchat with `enableErrorScreenTransition: false` (`MatchingFlow.cs:500,516`) with the
  failure caught, so guarding it needs a decision about what the sender should see.
- `SHARED_TABLES`, `STAMPS`, `STAMP_CURSOR` and `MATCH_LOBBY` grow per tid and are never
  pruned; a finished table stays in memory for the life of the process.
- `RecordKind` also has `Tournament` and `ScrambleMatch` tabs; the server emits none.
- `get_haifu_list` caps at 50 rows.
- A lobby with four real humans was not exercised — one probe client, so the four-human
  match path is untested beyond two humans plus CPU fill.
- The probe left test accounts and archives (`P6NIMA`, `P6SANMA`, `P6LOBBYA/B`,
  `P6REPLAY`, ...) in `tmp/RyuNET-core/dist/savedata/mfg@asphyxia.db`. Say the word and they
  get cleared.

## Files

Changed: `tmp/asphyxia_plugins/mfg@asphyxia/handlers/aog.ts`, `handlers/utils.ts`, `index.ts`
(new `VFG_SCRAMBLE_SCHOOL` config declaration), staged into
`tmp/RyuNET-core/dist/plugins/mfg@asphyxia` and checked against the server that was running then.
That staging is how the divergence started: the tree moved on in `dist` afterwards and the staging
clone fell behind it. `dist/plugins/mfg@asphyxia` is the canonical copy now (the only one edited,
the only one the core loads) and `tmp/asphyxia_plugins` is a mirror refreshed from it -
`pwsh scripts/ryunet_stage.ps1 -Action status` lists every copy and whether it matches. The build
this report's "before" repro measured is the 01:19 copy, kept as
`tmp/RyuNET-core/.frozen-builds/check-p7` (`aog.ts 846540ad3de4`, `utils.ts c62c65d9dbda`, no core,
not probeable).

Added: `.audit/probe_match_p6.py`, `.audit/probe_match_multi.py`, `.audit/probe_scramble.py`,
`.audit/ts_check_plugin.js`, `.audit/inspect_archive_rows.py`, `.audit/peek_match_db.py`.

Nothing committed.
