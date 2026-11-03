# Report 1 — legacy MID phantom profiles (corrected record)

This replaces the evidence section of the Report 1 that closed the round-3 problem
"`PHANTOM_REFID_RE` matches only the exactly-8-digit spelling". The fix itself stands. Two of its
claims did not.

Everything below was re-measured on 2026-11-01 against `tmp/RyuNET-core/dist/savedata` with the
core still running (no restart, no live probes).

## What the last report claimed, and what the savedata shows

| Claim in the last report | Measured | Verdict |
|---|---|---|
| "the documents the fork actually left are `MID1793446050`-shaped ... and `/^MID\d{8}$/` does not match them" | 118 profile documents, 3 MID-prefixed: `MID00998877`, `MID88000123`, `MID88000124`. All are MID + exactly 8 digits. Rows missed by `/^MID\d{8}$/`: **0** | false on this host — no document of that length exists (and `MID1793446050` is 13 characters, not 11) |
| "Those docs are still live read identities, write targets, adoptable by `/login`" | The 3 real rows were refused by `/login` **before** this fix (`out_pre.txt:26` → `code=770`, guard at the old exactly-8 pattern). `getProfileByRefid(MID00998877)` did resolve pre-fix (a read identity), so "read identity" was right and "adoptable by /login" was not | half true, and true of the wrong digit counts |
| "profile docs 121, MID-prefixed 6" | 121 = 118 real + 3 inserted by the harness (`tmp/p9repro/seed_lane.cjs:10`, `repro.cjs` seeds `1793446050`, `1793447266`, `123456789`); 6 = 3 real + 3 synthesized | fixture numbers presented as deployment numbers |

Where the invented rows came from, in the harness's own words: `seed_lane.cjs`
"`const shape = (mid) => 'MID' + String(mid).padStart(8, '0')`" plus an `INSERT`, and `repro.cjs`'s
header used to say "the documents the removed saveState fork produced for the mids that are
actually in this savedata" — the sentence that let a synthesized row be reported as a found one.

## Root cause

**Code (unchanged from the last report, still correct).** Identity resolution read the stored
refid against `/^MID\d{8}$/`, and the fork's shape was `'MID' + String(mid).padStart(8, '0')`.
`padStart` pads, it never truncates, so a 9- or 10-digit mid yields a longer refid that the pattern
does not name. That formula is recorded from the earlier analysis (`docs/18` §5; the removal itself
is documented at `handlers/utils.ts:788-792`), and no code in the tree implements it any more —
which is why the mint-path evidence below does not rest on it.
`playerIdFromRefid` is `parseInt(refid[0:8], 16) & 0x7fffffff`, and this server's
accounts produce 10-digit mids from that (`6AE5D0A2F7B97EB7` → `1793446050`), so the wider family
is a live possibility, just not a stored one.

Where the hole actually bites is the **mint path**, and it did not need fabricated rows to prove
it. Run the pre-fix build against a copy of the deployment's own savedata with nothing added
(`tmp/p9repro/out_deploy_pre_unseeded.txt`):

```
fixture        : C:/mfg/tmp/p9repro/data/plugin.db + ... - profile docs 118, MID-prefixed 3: MID00998877, MID88000123, MID88000124
harness seeds  : NONE - deployment-faithful run, every document below came from the fixture

  getProfileByRefid(MID00998877   ) -> RESOLVES as MID00998877 mid=998877
  ensureProfile(MID1234567    ) -> RETURNED MID1234567 mid=579803029; document before=false after=true
  ensureProfile(MID12345678   ) -> null; document before=false after=false
  ensureProfile(MID1793449999 ) -> RETURNED MID1793449999 mid=1388273673; document before=false after=true
  login user_id=MID00998877    -> code=770 session=none document existed=true newly minted=false mid=998877
  login user_id=MID123456789   -> code=0 session=270c07d1... document existed=false newly minted=true mid=45304724
  login user_id=MID1793446050  -> code=0 session=6a06fe04... document existed=false newly minted=true mid=1388731306
```

The two exactly-8 shapes in that list, the typed `MID12345678` and the stored `MID00998877`, were
refused. Every other digit count was accepted, and `MID1793446050` shows what that costs: the row
appears with a live session behind it.

`/login` accepted a posted string, `ensureProfile` wrote a profile document for it, and the
session-keyed handlers behind it (dojo slots, cutin gacha, music gacha reserve, favorite character)
resolve through `getProfileBySession`, which answered that document. That is the exposure: a caller
who types a MID-shaped `user_id` gets an account, and if the digit count is outside the pattern the
old code had no idea it was an orphan.

**Reporting (what this round fixed).** The deviation is a provenance error, not a data error: the
harness synthesizes the long rows and labels them "the documents the fork produced", so its counts
describe its own fixture, and the report quoted them as facts about the deployment. The same
sentences then sat in the plugin README, in `handlers/utils.ts` ("so the fork minted
`MID1793446050` for them") and in `docs/18` ("a 10-digit mid produced `MID1793446050`"), where they
read as measured facts about this server.

Provenance of the 3 real rows, from the payloads the repair saved (`tmp/p9repair/undo-*.json`):
`MID00998877` carries `states.audit_plain` (`mid=998877` posted by `.audit/probe_live.py:105`), and
`MID88000123` / `MID88000124` carry `states.audit_probe1` with markers `GHOST` and `NOPCUID`. So
they are MID-shaped orphans that this project's own probes minted through the same fallback the
fork used, from 6- and 8-digit mids — not fossils of the fork era.

## What is in the code and the savedata

Code (runtime copy `tmp/RyuNET-core/dist/plugins/mfg@asphyxia`, staging mirror byte-identical):

- `handlers/utils.ts:571` `PHANTOM_REFID_RE = /^MID\d{8,}$/` — recognises MID + 8 or more digits.
- `handlers/utils.ts:575` `MINT_FORBIDDEN_REFID_RE = /^MID\d/` — MID + digits at any length, so the
  near-miss `MID1234567` is covered too. A real DataID is hex, so no legitimate refid is inside it.
- Refusal at every resolver: `getProfileByRefid` (utils.ts:299), `getProfileBySession` (:342, :610),
  `getProfileByPlayerId` (:424-441), `sessionProfile`.
- Refusal at the two write doors: `ensureProfile` (utils.ts:489) and `saveProfile` (:538).
- Refusal at the two identity-issuing endpoints: `handle_login` (aog.ts:1052), `handle_create_player`
  (:1217), both before a session is minted. `odekake_done` refuses them as lenders (aog.ts:1570).
- Quarantine of stored rows: `quarantinePhantomProfiles` (utils.ts:716), run at plugin load
  (index.ts:237) and from the read path (`_repairPhantomOnTouch` / `_repairPhantomByRefid`),
  clearing `player_id`, `session_id`, card and share fields, keeping `states`, and writing
  `phantom_quarantine` so the row stays dead even if something renames its refid.
- Offline pass for a savedata nobody boots from: `.audit/p9_phantom_repair.js` (idempotent, writes
  an undo record).

Stored state of the deployment (`tmp/RyuNET-core/dist/savedata/mfg@asphyxia.db`):

```
profile docs  : 118
MID-prefixed  : 3  (matched by old /^MID\d{8}$/: 3; MISSED by it: 0)
quarantined   : 3
  MID00998877    mid=0  name=GUEST  states=1  session=""  quarantine=true
  MID88000123    mid=0  name=GUEST  states=1  session=""  quarantine=true
  MID88000124    mid=0  name=GUEST  states=1  session=""  quarantine=true
```

`.audit/p9_db_fingerprint.js` before/after the repair: 3 rows changed out of 138. Fingerprint of the
live file now vs. right after the repair: 0 differences — nothing else has moved.

## Verification, labelled by where it was measured

`tmp/p9repro/repro.cjs` now prints its own provenance before any number, and `P9_NO_SEED=1` runs
the probes against a fixture with nothing added.

1. **Deployment, no harness writes, fixed build** (`out_deploy_post_unseeded.txt`, fixture = clone
   of the live savedata):
   ```
   fixture        : 118 profile docs, MID-prefixed 3 (MID + 8 digits, all quarantined)
   harness seeds  : NONE - deployment-faithful run
   getProfileByRefid(MID00998877  ) -> refused (row exists in the fixture, guard refused it)
   getProfileByRefid(MID1793446050) -> refused (no such row in this fixture)
   ensureProfile(MID1234567   ) -> null; document before=false after=false
   ensureProfile(MID12345678  ) -> null; document before=false after=false
   ensureProfile(MID1793449999) -> null; document before=false after=false
   login user_id=MID1793446050  -> code=770 session=none document existed=false newly minted=false
   login user_id=MID1234567     -> code=770 session=none document existed=false newly minted=false
   quarantine pass: scanned=118 quarantined=0 already=3 (idempotent on the second pass)
   MID-prefixed documents still carrying a session_id or a player_id: 0
   control account P9CONTROLACCOUNT01: login 0, write 0, read back 11 states
   non-MID profile documents: 115, of them marked quarantined: 0
   ```
2. **Deployment, no harness writes, pre-fix build** (`out_deploy_pre_unseeded.txt`, fixture = the
   same savedata before the repair): `MID00998877` resolved as a read identity, `/login` minted
   `MID123456789` and `MID1793446050` from posted user_ids, `ensureProfile` minted `MID1234567` and
   `MID1793449999`, and `quarantinePhantomProfiles` did not exist in that build.
3. **Harness-seeded lane, fixed build** (`out_deploy_post_seeded.txt`) — the only run where
   `121 documents / 6 MID-prefixed` is a true statement, and it says so on its header line:
   ```
   harness seeds  : MID1793446050, MID1793447266, MID123456789 (synthesized here from the fork
                    formula; NOT deployment rows)
   profile docs   : 121, MID-prefixed 6 (fixture 3 + harness-seeded 3)
   getProfileByRefid(MID123456789  ) -> refused (row exists in the fixture, guard refused it)
   getProfileByRefid(MID1793446050 ) -> refused (row exists in the fixture, guard refused it)
   getProfileBySession(seeded phantom pcuid) -> refused
   getProfileByPlayerId(123456789) -> refused / no owner
   client_state_read(seeded phantom pcuid) -> code=770 states=0
   client_state_write -> code=770 payload landed on MID1793446050: false (row present before this write: true)
   ```
   The 9/10-digit refusal is defence-in-depth: it proves the widened pattern refuses shapes this
   server does not have yet. It is not evidence that it had them.
4. **Core side** (`.audit/p9_core_scan.js`): core.db carries 118 `refid_alias` rows, 3 of them
   MID-prefixed (`MID00998877`, `MID88000123`, `MID88000124`), no `profile` rows, no MID-shaped
   `user_account`.

Re-run all of it:

```powershell
node .audit\p9_phantom_scan.js tmp\RyuNET-core\dist\savedata\mfg@asphyxia.db
node .audit\p9_phantom_repair.js --dry-run
node .audit\p9_core_scan.js
node .audit\p9_db_clone.js tmp\RyuNET-core\dist\savedata\mfg@asphyxia.db tmp\p9repro\data\deploy_live_plugin.db
node .audit\p9_db_clone.js tmp\RyuNET-core\dist\savedata\core.db             tmp\p9repro\data\deploy_live_core.db
$env:P9_NO_SEED='1'; $env:P9_SRC_PLUGIN_DB='C:/mfg/tmp/p9repro/data/deploy_live_plugin.db'; $env:P9_SRC_CORE_DB='C:/mfg/tmp/p9repro/data/deploy_live_core.db'
node tmp\p9repro\repro.cjs deploy_post_unseeded 'C:/mfg/tmp/RyuNET-core/dist/plugins/mfg@asphyxia'
node tmp\p9repro\repro.cjs deploy_post_seeded   'C:/mfg/tmp/RyuNET-core/dist/plugins/mfg@asphyxia'   # adds the 9/10-digit shapes
```

## Left over

- The three rows stay in the database as quarantined dead data (`player_id=0`, empty session,
  `phantom_quarantine`, `states` kept for audit). Deleting them is a different decision and the
  undo file (`tmp/p9repair/undo-1793528093912.json`) is the way back.
- core.db still holds the 3 `refid_alias` rows. They are the lightweight alias that satisfies the
  core's upsert guard, not an account; nothing in the plugin will resolve them now. Cleaning them
  means a core-side delete, which is not the plugin's to make.
- The running core is still a pre-fix process: `AsphyxiaCore.js` pid 30244, started 2026-11-01
  01:35:54, against plugin files last written 05:43 (`node .audit\server_status.ps1`). The data half
  happened (rows repaired offline); the code half arms on the central restart. Until then a live
  `/login` on that process can still mint a MID-shaped account.
- `MIDGUESS0001` is a normal profile document that happens to start with `MID`. It is outside the
  family (`MID` + digits), is not refused, and was left alone.
