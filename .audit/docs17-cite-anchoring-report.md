# docs/17: every stale THEIRS cite re-anchored, and the four claims that were wrong

Scope: `C:\mfg\docs\17-ryunet-comparison.md` (673 lines, tracked). No plugin code was touched.

## Root cause

Every THEIRS range in docs/17 was written against the pre-takeover baseline: aog.ts 3,111 lines
(`6DD99D10210D…`), utils.ts 2,314 (`5F310FCE40FE…`), index.ts 263 (`C195EB805163…`). The phantom /
takeover round grew aog.ts to 3,568 (`84D9FA3AA182…`) and utils.ts to 3,182 (`D4B775D0DBE3…`),
+457 and +868 lines, so each range below the insertion points moved by a different amount. Only
three cites in row 396 were repaired in that round; the ranges around them, and every other row,
stayed on the baseline.

That these are baseline offsets and not typos is measurable. A same-length copy of the old utils.ts
survives at `C:\mfg\tmp\bonusparam\out\old\handlers\utils.ts` (2,314 lines, `7612F82928EA`). There,
`utils.ts:475-508` and `utils.ts:563-571` land on exactly the code docs/17 describes. In today's
utils.ts the same numbers land elsewhere: 563-571 is inside the phantom-pattern comment block
(551-571, `PHANTOM_REFID_RE` declared at 571) that this fix cycle added, so a reader following that
cite landed on the new phantom text instead of the session resolver the row was about. Same for
row 660's `aog.ts:1841`, which sits inside `seedFriendGameState` (1700-1849) rather than the gget
header the row names. Baseline A's aog.ts is not on disk anywhere (`tmp/` is git-ignored, no
3,111-line copy exists), so the ranges can no longer be resolved by looking them up.

Audit over the whole doc: 324 THEIRS cites checked, 140 already correct, 180 needing a new range,
4 where the sentence itself contradicts the code, 0 pointing at code that no longer exists. The
`taikyoku.ts` and `mahjong.ts` cites all hold; neither file changed.

## What I changed in docs/17

**1. Re-anchored cites.** 187 cite positions on 99 lines, 139 distinct ranges (aog.ts 146, utils.ts
36, index.ts 5). Each new range was chosen by walking the current file to the code the row names,
and each one is checked by anchor text that must appear inside the range. Headline ones:

| row | stale | now | code the new range covers |
|---|---|---|---|
| 164, 404, 661 | `utils.ts:563-571` | `1017-1035` | stamp cursor block, `STAMP_CURSOR` / `stampCursor` |
| 164 | `aog.ts:2002-2007` | `2413-2427` | `handle_gpost` table window (`table.cells_from(start)`) |
| 392 | `aog.ts:1007-1069` | `1024-1152` | `handle_login` |
| 396 | `1508-1522`, `1524-1528`, `1530-1560`, `1561-1579`, `1583-1606`, `1608-1626`, `1628-1651` | `1899-1913`, `1915-1919`, `1921-1951`, `1952-1970`, `1974-1997`, `2012-2030`, `2032-2055` | `handle_client_state_write`, end to end |
| 404 | `aog.ts:362-390` / `2104-2133` | `379-421` / `2525-2555` | `stampPost` + `maybeCpuStamp` / `handle_gchat` |
| 407 | `aog.ts:2963-2976` / `2759-2767`, `utils.ts:1267-1283` | `3420-3433` / `3168-3190`, `2065-2082` | `chara_enabled` / competition entry / `bannerHasPickupPool` |
| 409 | `aog.ts:1071-1074` / `1151-1154`, `utils.ts:731-786` | `1155-1174` / `1318-1321`, `1197-1256` | `handle_logout` / `handle_keep_alive` / `EVICTED_SESSIONS` |
| 411 | `aog.ts:1394-1448` / `2915-2961` / `2718-2743` / `615-633` | `1561-1614` / `3340-3419` / `3141-3167` / `629-650` | `borrowableLender` / `odekake_done` / `handle_present_done` / present box record |
| 581 | `utils.ts:495-508` / `202-213` | `763-776` / `210-221` | `sessionProfile` + `resolveProfile` / `playerIdFromRefid` |
| 590 | `aog.ts:375-390`, `:415`, `:426-427` | `450-465`, `:497`, `:508-509` | the seat/tid guard it describes |
| 660 | `aog.ts:1841` | `2258-2262` | `handle_gget` header (`const nextSno = mustInt(parts, 5, 0);`) |
| 661 | `aog.ts:1953-1958` | `2374-2379` | the gpost stamp backlog cursor comment |
| 375, 398, 614 | `index.ts:154-239` | `244-340` | the route table |
| 398, 662 | `utils.ts:2275-2304` | `3143-3174` | `gameBaseUrl` |

The full list is `C:\mfg\tmp\cite_applied.json` (187 rows: doc line, old range, new range, how it
was resolved).

**2. Named the snapshot the cites are as-of.** The Citations block (lines 17-23, same line count)
now reads, verbatim:

> Anchor for the THEIRS column: the copy on disk now — aog.ts `84D9FA3AA182…` (3,568 lines), utils.ts
> `D4B775D0DBE3…` (3,182), index.ts `3583EB3E15A7…` (353), all written 2026-11-01. Every THEIRS range
> below was walked against that snapshot on 2026-11-01; 187 that no longer described the code named
> were re-anchored, and `taikyoku.ts` / `mahjong.ts` are unchanged since the first pass, so their
> cites stand. The pre-takeover baseline older notes quote (`6DD99D10210D…`, 3,111 lines) is off disk.

**3. Fixed the four rows whose claim, not just the range, was wrong.**

- 392: `PresentID 91000000+YYYYMMDD` → `91000000+YYMMDD` (aog.ts:1130 builds it from
  `today.slice(2)`, so the year is two digits).
- 407: "its `end_date` is `2099-12-31 00:00:00`" → "`EVENT_END` = `2099/12/31 23:59:59`
  (utils.ts:3074)" — both the separator and the time were wrong.
- 409: "`logout` is a pure ack" and "after `logout` alone the old pcuid still resolves" → logout
  calls `tombstoneSessionOnProfile` and clears `session_id` (aog.ts:1155-1174), which is what lets a
  cleanly ended lane log straight back in instead of waiting out `VFG_SESSION_TAKEOVER_GRACE`
  (`loginGate`, utils.ts:1498-1536).
- 411: "id = lender **refid string**" → the id is the integer grant ticket the read issued for that
  lender (`odekakeGrantTicket`, utils.ts:928-937; looked up in the viewer's own `odekake_grants` at
  :941-951; echoed at aog.ts:3407). Ours answers a constant `900001`.

**4. `.audit/p9-mid-phantom-report.md` provenance.** Lines 48-49 quoted `session=c7bf2a5e…` and
`session=ada31fbd…` as readings from `tmp/p9repro/out_deploy_pre_unseeded.txt`. That file records
`session=270c07d1…` (line 31) and `session=6a06fe04…` (line 33). Both lines now match the artifact
character for character; codes, mids and mint flags already matched.

## Verification

Gate script `C:\mfg\tmp\cite_verify.py` (asserts the plugin tree still hashes to the snapshot the
cites were walked against, doc line count unchanged, the cite rewrite touched no prose outside the
citations block, the claim fixes touched no cite range, no stale range left anywhere in the doc,
every re-anchored range present, and every anchor text inside its own range):

```
doc lines 673; mappings 139; anchors checked 141; stale left 0; cites in doc (THEIRS) 443
note: prose changed only on the citations block lines: [17, 18, 19, 20, 21, 22, 23]
note: claim fixes touched lines: [392, 407, 409, 411]
PROBLEMS: 0
exit=0
```

Stale-versus-anchored, same cite, read against today's code (`C:\mfg\tmp\counter_example.py`):

```
utils.ts:563-571 -> 1017-1035   stale reads today as : // reports 118 profile documents, 3 MID-prefixed, ...
                                 anchored reads today : // What each seat has already been shown out of its table's stamp backlog.
aog.ts:1841      -> 2258-2262   stale reads today as : //     MissionFlow.cs:302-304, VoteWindow.cs:586 - "MemorialCard" rows), ...
                                 anchored reads today : // must = appver/pcuid/tid/pindex/mid/nextSeqNo (GGet.cs:30): segment 3 is t...
```

Type check from `C:\mfg\tmp\RyuNET-core` (unchanged baseline, 0 errors):

```
npx tsc --noEmit --skipLibCheck --target es2020 --module commonjs --moduleResolution node dist/plugins/mfg@asphyxia/handlers/aog.ts   exit=0
npx tsc --noEmit --skipLibCheck --target es2020 --module commonjs --moduleResolution node dist/plugins/mfg@asphyxia/handlers/utils.ts  exit=0
npx tsc --noEmit --skipLibCheck --target es2020 --module commonjs --moduleResolution node dist/plugins/mfg@asphyxia/index.ts          exit=0
```

Runtime copy versus staging copy, byte identical:

```
aog.ts    dist=84D9FA3AA182 staging=84D9FA3AA182 match=True
utils.ts  dist=D4B775D0DBE3  staging=D4B775D0DBE3  match=True
```

AsphyxiaCore was not restarted; nothing in this round talks to a running lane. Diff evidence:
`git -C C:\mfg --no-pager diff --stat -- docs/17-ryunet-comparison.md` reports 250 insertions / 158
deletions (was 249 / 157 before this round), status ` M`. `.audit/` is untracked, so the
audit-report fix is recorded by the snapshot `C:\mfg\tmp\p9report_before.md` plus the two-line diff
above. Nothing was committed or pushed.

## One thing left, flagged

`utils.ts:1027-1028` is a code comment that self-cites `aog.ts:2247/2294/2420` for the stamp-cursor
callers. The real call sites are 2378-2379, 2425-2426 and 2552-2553 (`stampCursor` /
`stampCursorMark` in `handle_gchat`, `handle_gpost` and the gget window). Fixing it means editing
plugin code and mirroring the edit into `tmp/asphyxia_plugins/mfg@asphyxia` with a fresh tsc pass,
which is outside the fix line for this round, so I left it and am reporting it instead.
