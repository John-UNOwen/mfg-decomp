# 15 - `/gget` and `/gpost`: the match command contract

Purpose: a mock/replacement game server must be able to run a full Mahjong Fight Girl
match against the *unmodified* recovered client. This document pins the exact wire
contract of the two endpoints that drive play, every server→client command and the
fields each one is parsed from, every client→server command and the exact parameter
string the server receives, the sequence/window protocol, the match and kyoku
lifecycles, the tile encoding on the wire, and which mahjong logic already exists
client-side and can be reused server-side.

Method: source reading only. No game, tooling, or mock was run for this document.

Citation convention: paths are repo-relative. `AC/` = `api/recovered/Assembly-CSharp/`.
`AC/…/GGet.cs:30` means line 30 of that file. Duplicate tree `api/recovered/out/decompiled/`
is ignored. Live-wire corroboration comes from
[capture/s2-evidence-sanitized.md](../capture/s2-evidence-sanitized.md) (identity values
masked as `<pcuid>`; the PCBID/card/PIN are not reproduced here).

`UNCONFIRMED:` marks an inference that the recovered sources cannot settle; each one names
the artifact that would confirm it.

---

## 1. Transport framing

### 1.1 Common envelope (every front-API POST, `/gget` and `/gpost` included)

| Fact | Evidence |
|---|---|
| POST with body `application/x-www-form-urlencoded`, written as UTF-8 bytes | `AC/Konami.System.Net/HttpWebRequestor.cs:14` (constant), `:106` (applied); `AC/Konami.System.Net/RequestCoroutine.cs:38`, `:126` |
| Requests are serialised: `MaxParallelRequestCount = 1`, one in flight per client | `AC/Konami.System.Net/RequestManagerBase.cs:10`, `:153-203` |
| URL = `FrontServerUrl` + endpoint path; `FrontServerUrl` comes from eAC3 `vfgac.service_list` XRPC (`GetServiceList.ServiceUrl` / `NetEnvironment…RequestUrl`) | `AC/MFG.GameRequest/RequestApiBase.cs:55`; `AC/TitleDemoUtility.cs:243-261`; `AC/CardEntryScene.cs:1068-1089`; live value `http://<provider-host>/aog/` → observed URL `http://<provider-host>/aog//gget` (the doubled slash is literal: base URL ends with `/`) — capture line 41 |
| On non-Konami-net (card/emulator) routes every body is prefixed `system_id=<PCBID>&sic=…&loc_id=ea&` | `AC/MFG.GameRequest/RequestApiBase.cs:41-51`, `:87-97`; visible in capture lines 40-43 |
| Response must be XML; its **root** element is handed to `OnParse` (root name is irrelevant — mock uses `<res>`), and `serv_st` **must** exist under it | `AC/MFG.GameRequest/RequestApiBase.cs:146` (`XDocument.Parse(...).Root`), `:162` (`NeedNotElement("serv_st")` throws `NeedNotNodeNotFoundException` if absent) |
| `serv_st/code` is read as the status code; `0` = OK, anything else throws `NetworkResponseException`; if `code` is missing/unparsable the code defaults to **630** (still an error) | `AC/MFG.GameRequest/RequestApiBase.cs:160-179` |
| A non-zero code or transport error routes to the error screen (`ExecErrorScreenTransition`, ERROR_STOP) | `AC/MFG.GameRequest/RequestApiBase.cs:130-140`, `:181-189` |
| Element access helpers: `MustNeedElement` throws `MustNeedNodeNotFoundException` when missing; `NeedNotElement` throws `NeedNotNodeNotFoundException`; `ValueInt/Int64/UInt/UInt64` = `int.Parse` of element text; `ValueArrayInt` = text split on **spaces** and parsed element-wise; `AttributeValueInt` = `int.Parse` of an attribute (`Attribute(s).Value` ⇒ missing attribute = NRE) | `AC/MFG.Sytem.Utilities.Xml/Extensions.cs:18-31`, `:47-60` |
| Hard-coded client identity/config constants sent in every body: `AppVersion = "2.0.2"`, `WebID = 1`, `AreaID = 1` | `AC/SystemConfigGameData.cs:24-28`; accessors `AC/MFG.GameRequest/RequestApiBase.cs:53`, `:59`, `:61` |
| `pcuid` == login `session_id` (from `login` → `auth/session_id`), not the card id | `AC/MFG.GameRequest/Login.cs:30-31`; `AC/GameDataManager.cs:140`; capture line 40 |
| `gserv_id` for a match comes from the `entry_game` response and is echoed in `gget`/`gpost`/`gchat` | `AC/MFG.GameRequest/RequestApiBase.cs:63`; `AC/MFG.GameData/TaikyokuGameData.cs:15`; `AC/MFG.Taikyoku/EntryTakuInfo.cs:49` |

### 1.2 `/gget` request body (verbatim template)

`AC/MFG.GameRequest/GGet.cs:29-30`:

```
web_id={WebID}&area_id={AreaID}&pcuid={AppSessionID}&gserv_id={GameServiceID}&ready={0|1}&must={AppVersion}/{AppSessionID}/{tid}/{pindex}/{mid}/{nextSeqNo}
```

| Field | Value | Source |
|---|---|---|
| `web_id` | `1` | `AC/SystemConfigGameData.cs:28` |
| `area_id` | `1` | `AC/SystemConfigGameData.cs:26` |
| `pcuid` | login session id | `AC/MFG.GameRequest/RequestApiBase.cs:65` |
| `gserv_id` | `entry_game` → `<entry><gserv_id>` | `AC/MFG.GameRequest/RequestApiBase.cs:63` |
| `ready` | `1` once the client called `NotifyReadyToKyokustart()`, else `0` | `AC/MFG.GameRequest/GGet.cs:27`,`:30`; armed by `AC/MahjongPlayFlow.cs:674` → `AC/MFG.Taikyoku.Command/CommandClient.cs:76-79` |
| `must` slot 1 | `AppVersion` = `2.0.2` | `AC/MFG.GameRequest/GGet.cs:30` |
| `must` slots 2-6 | `AppSessionID` / `tid` / `pindex` / `mid` / **`nextSeqNo`** | `AC/MFG.GameRequest/GGet.cs:30`; values passed from `AC/MFG.Taikyoku.Command/CommandClient.cs:123` (`(TakuID, PlayerIndex, PlayerID, isReady, m_cmdFetchPosition)`) |

Observed (`capture/s2-evidence-sanitized.md:41`):

```
…&gserv_id=1&ready=1&must=2.0.2/<pcuid>/1027/0/1466323762/554
```

The last slot is the client's **fetch cursor**: "give me everything from cell `554` on".

Generic parameter tuple: `(int TakuID, int PlayerIndex, int PlayerID, bool isReady, int nextSeqNo)` - `AC/MFG.GameRequest/GGet.cs:9`.

### 1.3 `/gpost` request body (verbatim template)

`AC/MFG.GameRequest/GPost.cs:43-44`:

```
web_id={WebID}&area_id={AreaID}&pcuid={AppSessionID}&gserv_id={GameServiceID}&tid={tid}&mid={mid}&pindex={pindex}
&must={AppVersion}/{AppSessionID}/{tid}/{pindex}/{mid}/{seqNo}/{kind}/{tepai_pos}/{tepai_pos2}/{pai_id}/{tepai_id}/{tepai_id2}/{reach_flg}/{tsumogiri_flg}/{chi_naki}
&app_param=0/0
```

`app_param` is hard-coded `0/0` (`GPost.cs:41-42`, `:44`). Field names below are the
server-side names the client itself prints in its own debug line
(`AC/MFG.GameRequest/GPost.cs:46`), which is also the format of the repo's captured
`GPost param:` lines.

`must=` slot table (`AC/MFG.GameRequest/GPost.cs:31-40`, `:44`):

| # | Slot | Client field | Meaning |
|---|---|---|---|
| 1 | version | `AppVersion` | `2.0.2` |
| 2 | app session | `AppSessionID` | login session id |
| 3 | `tid` | `cmd`-independent `TakuID` | table id from `entry_game` |
| 4 | `pindex` | `PlayerIndex` | seat 0-3 from `entry_game` |
| 5 | `mid` | `PlayerID` | **player** id from `get_menudata` (see §6.2) |
| 6 | `seqNo` | `cmd.SeqNo` | seq the client is answering / claims to be at (§5.5) |
| 7 | `kind` | `(int)cmd.Kind` | `SEND_COMMAND_TYPE` ordinal (§2.2) |
| 8 | `tepai_pos` | `cmd.TepaiPos` | hand-array index of the used/concealed tile (§4.2) |
| 9 | `tepai_pos2` | `cmd.TepaiPos2` | second hand-array index (chi/pon only) |
| 10 | `pai_id` | `cmd.PaiID` | tile id (claim: the claimed tile; discard: the discarded tile) |
| 11 | `tepai_id` | `cmd.TepaiID` | kind id of the tile at `tepai_pos` |
| 12 | `tepai_id2` | `cmd.TepaiID2` | kind id of the tile at `tepai_pos2` |
| 13 | `reach_flg` | `cmd.ReachFlag` | riichi **level**, not a bool (§4.2) |
| 14 | `tsumogiri_flg` | `cmd.TsumoGiriFlag` | 1 = discard the drawn tile |
| 15 | `chi_naki` | `cmd.ChiNaki` | `ChiPos` 0/1/2 (which part of the sequence came from the discard) |

Observed (`capture/s2-evidence-sanitized.md:42`), same session as the `gget` above:

```
…&tid=1027&mid=1466323762&pindex=0&must=2.0.2/<pcuid>/1027/0/1466323762/555/2/13/0/17/0/0/0/1/0&app_param=0/0
```

Decoded: seq 555, kind 2 = `SUTE_PAI`, `tepai_pos=13` (the drawn tile sits at index 13 of
a 14-tile hand), `tepai_pos2=0`, `pai_id=17` (= `Pai.S7`), `tepai_id=0`, `tepai_id2=0`,
`reach_flg=0`, `tsumogiri_flg=1`, `chi_naki=0`.

### 1.4 There is no `window=` or `seq=` form field

Grepping every recovered request builder finds **no** `window` or `seq` form field
anywhere; the two concepts the earlier notes called "window" and "seq" are slots inside
`must=`:

* `docs/13-s2-provider-capture.md:37` describes the chain as
  `must=2.0.2/<pcuid>/<tid>/<pindex>/<mid>/<window>/<cmd>/<sno>/…`. Reading the builder
  (`AC/MFG.GameRequest/GPost.cs:44`) the actual order after `mid` is
  `seqNo / kind / tepai_pos / tepai_pos2 / pai_id / …`, so that document's `<window>` slot
  **is** the seq/`next_sno` slot and its `<cmd>` is the kind. There is no separate window id.
* "Window" as a live concept = the range of cells the server returns per poll
  (`cell_sno start/count`, §5.1) and, in play terms, the choice windows
  `TSUMOCHOICES(15)` / `SUTECHOICES(16)`. Neither is a request field.

### 1.5 `/gget` response tree the client expects

Root of the response document → `AC/MFG.GameRequest/GGet.cs:34-78`.

| Element path | Parser | Required? | Meaning |
|---|---|---|---|
| `serv_st/code` | `RequestApiBase.ParseResultStatusCode` | **yes** (`serv_st` mandatory; `code` 0 = OK) | status (`AC/MFG.GameRequest/RequestApiBase.cs:160-179`) |
| `game` | gates all match parsing; absent ⇒ only `chat` is read | optional | `AC/MFG.GameRequest/GGet.cs:38-39` |
| `game/all_ready` | `ValueInt() != 0` → `Result.IsReadyToKyokuStartAll`; **missing element is treated as 1** | optional (defaults true) | "every seat is ready to start the kyoku" — `AC/MFG.GameRequest/GGet.cs:43-51` |
| `game/mwait` | `MatchingData.Parse` | optional (`NeedNotElement`, `GGet.cs:52-56`) | matching state; see below |
| `game/taikyoku` | `TaikyokuData.Parse` | optional (`NeedNotElement`, `GGet.cs:63-67`) | command log + seat connection state (`AC/MFG.Taikyoku/TaikyokuData.cs:19-64`) |
| `chat` (root level) | `StampData.Parse` | optional | stamp messages `chat/d` with attrs `idx/mid/pindex/time`, children `name/contents/param` (`AC/MFG.Taikyoku/StampData.cs:27-55`) |

`game/mwait` (parsed by `AC/MFG.Taikyoku/MatchingData.cs:27-106`): `status` (`≠0` ⇒
`IsMatchingEnd`, `MatchingData.cs:31-32`), `pnum` (`:33`), `cpu_num` (`:34`), `pindex` (read
and discarded - `MatchingData.cs:36-39`), optional `mend` roster with children
`player_{0..3}` (`ptype` 0-2 human with `zaseki` + base64 `client_states`; `ptype` 3 CPU
with `cpu_level`/`cpu_name`/`cpu_variant` - `AC/MFG.Taikyoku/PlayerInfo.cs:70-110`),
`g2event/e/id` (`MatchingData.cs:87-104`). Inside `mwait`, `status`/`pnum`/`cpu_num` use
`NeedNotElement` but the local handler only swallows `MustNeed…` (`MatchingData.cs:31-43`),
so a `mwait` missing `status` aborts match parsing for that response (caught at
`GGet.cs:58`).

`game/taikyoku` (`AC/MFG.Taikyoku/TaikyokuData.cs:19-64`):

| Element / attribute | Line | Meaning |
|---|---|---|
| `cell_info` (element itself) | `:23-24` | **dereferenced unguarded** — a `<taikyoku>` without `<cell_info>` NREs |
| `cell_info@available` | `:24`, `:42` | if present and `!= 1`, **no cells are read at all** and `NextFetchSeqNo` stays 0 |
| `cell_sno@start`, `cell_sno@count` | `:44-51` | first seq of this batch, number of cells |
| `cell_data_{start+i}` | `:55` | one command per cell; the suffix **is** the seq number |
| `cell_data_*@kind` | `:68` | `RECEIVE_COMMAND_TYPE` (§2.1) fed to the factory |
| `NextFetchSeqNo = start + count` | `:58` | new cursor |
| `pinfo_{i}/con_state`, `pinfo_{i}/app_player_state` | `:25-41` | both non-zero ⇒ seat *i* is in 代走 (auto-play); drives `ApplyCpu` (`AC/MFG.Taikyoku.Command/CommandClient.cs:141`) |

### 1.6 `/gpost` response tree

`AC/MFG.GameRequest/GPost.cs:49-78`. Same `serv_st/code`, plus `game/mwait`,
`game/taikyoku` (same `cell_info`/`cell_sno`/`cell_data_{seq}` log, §1.5), root `chat`,
and - only on `gpost` - root `recon`:

| Element | Line | Meaning |
|---|---|---|
| `recon` | `:67-72` | sets `IsReconnect` and re-parses the same document body as a full kyoku snapshot via `TaikyokuStartInfo.Parse` |

`GPost.OnParse` has **no** try/catch: `game/taikyoku` without `cell_info` (NRE at
`TaikyokuData.cs:24`) or an `mwait` missing `status` propagates out of `OnParse` into the
request wrapper's error path (`AC/MFG.GameRequest/RequestApiBase.cs:110-140`). The `gget`
path is more forgiving because `GGet.cs:41-71` swallows `NeedNot…` per block.

`CommandClient` consumes a `recon` by clearing its buffer, replacing it with the cells of
the *same* response, and re-basing the cursor at `recon/next_sno`
(`AC/MFG.Taikyoku.Command/CommandClient.cs:197-204`).

### 1.7 The `cell_data` element and seat addressing

Every command cell is `cell_data_{seqNo}` with attribute `kind`
(`AC/MFG.Taikyoku/TaikyokuData.cs:55`,`:68`) plus four **seat attributes** parsed by the
base class for every command: `pi0`, `pi1`, `pi2`, `pi3` (each `1` sets that seat's bit) -
`AC/MFG.Taikyoku.Command.Receive/ReceiveCommand.cs:28-47`, flags enum
`AC/MFG.Taikyoku.Command/PLAYER_INDEX_FLAG.cs:8-11` (`P0=1, P1=2, P2=4, P3=8`).
`ToPlayerIndex()` returns the **lowest set bit's** seat (`AC/MFG.Taikyoku.Command/PlayerIndexFlagExtention.cs:5-24`),
so a multi-bit mask addresses several seats but "the" player of a command resolves to the
lowest. The `pi*` attributes are **mandatory in practice**: `AttributeValueInt`
dereferences the attribute (`AC/MFG.Sytem.Utilities.Xml/Extensions.cs:28-31`) and
`ParsePlayerIndexFlag` runs from `base.Parse` *before* each concrete parser's own
`catch (NeedNotNodeNotFoundException)` (e.g. `AC/MFG.Taikyoku.Command.Receive/Ankan.cs:19`
outside `:20-27`), so a missing `pi*` escapes as an unhandled `NullReferenceException`
(§1.8).

### 1.8 Parse hazards a mock must respect

| Hazard | Why | Evidence |
|---|---|---|
| Missing `serv_st` → exception out of `OnParse` | `NeedNotElement` before the try | `AC/MFG.GameRequest/RequestApiBase.cs:162` |
| `serv_st` present but no parsable `code` → error 630 | default value then throw | `:163-178` |
| `<taikyoku>` without `<cell_info>` → NRE | `.Element("cell_info").Attribute(...)` | `AC/MFG.Taikyoku/TaikyokuData.cs:23-24` |
| `cell_sno@count = N` but `cell_data_{start+N-1}` missing → NRE | `AttributeValueInt("kind")` on null | `AC/MFG.Taikyoku/TaikyokuData.cs:55` → `AC/MFG.Sytem.Utilities.Xml/Extensions.cs:28-31` |
| Unknown `kind` → cell silently dropped (log `cmd not found`) | factory default branch returns null | `AC/MFG.Taikyoku.Command.Receive/ReceiveCommandFactory.cs:60-62` |
| `pi*` attribute missing on a cell → NRE inside `ParsePlayerIndexFlag` | `AttributeValueInt` derefs the attribute | `AC/MFG.Taikyoku.Command.Receive/ReceiveCommand.cs:31-43` |
| Missing inner element of a command (`pindex`, `pai`, …) → that command parses as zeroes | every concrete parser wraps its body in `catch (NeedNotNodeNotFoundException)` | e.g. `AC/MFG.Taikyoku.Command.Receive/Ankan.cs:20-27` |
| `dora`/`ura_dora` arrays shorter than 5 (9 in accel-dora mode) → `IndexOutOfRange` at kyoku start | client loops `j < 5` reading both arrays | `AC/ExecKyoku.cs:112-117` |
| `tepai` shorter than 13 per seat → `IndexOutOfRange` | client deals indices 0..12 | `AC/ExecKyoku.cs:137-140` |
| Missing `player_info3` in `KYOKUSTART` (2p/3p modes) → `Players` list ends up short and later indexing throws | loop is `i < 4` and a `NeedNot` aborts the whole loop | `AC/MFG.Taikyoku.Command.Receive/KyokuStart.cs:111-125`, `AC/ExecKyoku.cs:95-98` |

---

## 2. Command type enums

### 2.1 `MFG.Taikyoku.Command.RECEIVE_COMMAND_TYPE` (server → client)

`AC/MFG.Taikyoku.Command/RECEIVE_COMMAND_TYPE.cs:5-22` - explicit values, gaps are real
(11-13, 18, 20-22 are unassigned in the client):

| Value | Member | Line |
|---|---|---|
| 0 | `NONE` | `:5` |
| 1 | `TSUMO` | `:6` |
| 2 | `SUTEHAI` | `:7` |
| 3 | `TSUMOAGARI` | `:8` |
| 4 | `RON` | `:9` |
| 5 | `RYUKYOKU` | `:10` |
| 6 | `PON` | `:11` |
| 7 | `CHI` | `:12` |
| 8 | `ANKAN` | `:13` |
| 9 | `MINKAN` | `:14` |
| 10 | `KAKAN` | `:15` |
| 14 | `TYOKO` | `:16` |
| 15 | `TSUMOCHOICES` | `:17` |
| 16 | `SUTECHOICES` | `:18` |
| 17 | `KYOKUSTART` | `:19` |
| 19 | `RANNYU` | `:20` |
| 23 | `KYOKUEND` | `:21` |
| 24 | `SCORERANK` | `:22` |

`TSUMOCHOICES = 15` and `SUTECHOICES = 16` are the two "choice window" commands;
`KYOKUSTART = 17` is the deal. `RANNYU = 19` (乱入, hot-seat/insertion) has **no parser**
(`ReceiveCommandFactory.cs:60-62`), so a server that emits it is ignored by this client.

### 2.2 `MFG.Taikyoku.Command.SEND_COMMAND_TYPE` (client → server)

`AC/MFG.Taikyoku.Command/SEND_COMMAND_TYPE.cs:5-20` - **no explicit values**: implicit
0-based ordinals in declaration order.

| Ordinal | Member | Line |
|---|---|---|
| 0 | `NO_USE0` | `:5` |
| 1 | `ENTRY` | `:6` |
| 2 | `SUTE_PAI` | `:7` |
| 3 | `TSUMO_AGARI` | `:8` |
| 4 | `RON_AGARI` | `:9` |
| 5 | `PON` | `:10` |
| 6 | `CHI` | `:11` |
| 7 | `ANKAN` | `:12` |
| 8 | `MINKAN` | `:13` |
| 9 | `KAKAN` | `:14` |
| 10 | `KYUSYUKYUHAI` | `:15` |
| 11 | `NAKINASHI` | `:16` |
| 12 | `CYOUKOU` | `:17` |
| 13 | `KIKEN` | `:18` |
| 14 | `RECONNECT` | `:19` |
| 15 | `NEXT_KYOKU_READY` | `:20` |

Reconciliation with the live captures: `kind=2` appears with discard parameters
(capture line 42), and the client's own log lines label the same sends as
`SUTE_PAI(2)`, `NAKINASHI(11)`, `NEXT_KYOKU_READY(15)`
(the repo's `GPost param:` line, capture line 51; per-command vocabulary totals at capture
lines 45-48). `kind` on the wire == enum ordinal == position in `must=` slot 7
(`AC/MFG.GameRequest/GPost.cs:32`,`:44`).

### 2.3 Supporting enums referenced by the protocol

| Enum | Values | File |
|---|---|---|
| `PLAYER_INDEX_FLAG` | `P0=1, P1=2, P2=4, P3=8` | `AC/MFG.Taikyoku.Command/PLAYER_INDEX_FLAG.cs:8-11` |
| `SELECTABLE_TYPE_FLAG` | `NONE=1, PON=2, CHI=4, KAN=8, TSUMOAGARI=0x40, RON=0x80, KYUSYU=0x100, REACH=0x200, SUTE=0x400, FIREREACH=0x800, OPENREACH=0x1000, ATOMICREACH=0x2000` | `AC/MFG.Taikyoku.Command/SELECTABLE_TYPE_FLAG.cs:8-19` |
| `ReachLevel` (`reach_flg`) | `Normal=1, OpenReach=2, AtomicReach=3, FireReach=5` | `AC/MFG.Types/ReachLevel.cs:3-9` |
| `ChiPos` (`chi_naki`) | `Left=0, Center=1, Right=2` | `AC/MFG.Types/ChiPos.cs:3-8` |
| `FURO_TYPE` (`naki/type`) | `NO_USE=0, AN_SYUNTSU=1, AN_KOUTSU=2, AN_KANTSU=4, TOITSU=8, MIN_SYUNTSU=17, MIN_KOUTSU=18, MIN_KANTSU=20, KAKANTSU=24` | `AC/MFG.Taikyoku.Command/FURO_TYPE.cs:5-13` |
| `GAME_MODE` (`entry_game` `gmode`) | `GLOBAL_TONPU=1, GLOBAL_HANCHAN=2, GLOBAL_SANMA=3, GLOBAL_NIMA=4, …28 event modes` | `AC/MFG.Taikyoku.Command/GAME_MODE.cs:5-33` |
| `MATCH_MODE` | `None=0, WHOLE=1, PRIVATE=2` | `AC/MFG.Taikyoku.Command/MATCH_MODE.cs:5-7` |
| `TAKU_CLASS` | `NORMAL=1, KOURYU=2, CARRYOVER=3, SPECIAL=4` | `AC/MFG.Taikyoku.Command/TAKU_CLASS.cs:5-9` |
| `Constants.TAKU_PLAYER_MAX` | `4` (all player-indexed loops) | `AC/MFG.Taikyoku.Command.Receive/Constants.cs` |

---

## 3. Server → client: every `RECEIVE_COMMAND_TYPE`

### 3.1 Factory mapping (`AC/MFG.Taikyoku.Command.Receive/ReceiveCommandFactory.cs:7-65`)

| Kind | Concrete class | Map line |
|---|---|---|
| 1 TSUMO | `Tsumo` | `:12-13` |
| 2 SUTEHAI | `Sutehai` | `:15-16` |
| 3 TSUMOAGARI | `TsumoAgari` | `:18-19` |
| 4 RON | `Ron` | `:21-22` |
| 5 RYUKYOKU | `RyuKyoku` | `:24-25` |
| 6 PON | `Pon` | `:27-28` |
| 7 CHI | `Chi` | `:30-31` |
| 8 ANKAN | `Ankan` | `:33-34` |
| 9 MINKAN | `Minkan` | `:36-37` |
| 10 KAKAN | `Kakan` | `:39-40` |
| 14 TYOKO | `TyoKo` | `:42-43` |
| 15 TSUMOCHOICES | `TsumoChoices` | `:45-46` |
| 16 SUTECHOICES | `SuteChoices` | `:48-49` |
| 17 KYOKUSTART | `KyokuStart` | `:51-52` |
| 23 KYOKUEND | `KyokuEnd` | `:54-55` |
| 24 SCORERANK | `ScoreRank` | `:57-58` |
| 0, 11-13, 18, 19 (RANNYU), 20-22 | *null* + `Debug.Log("cmd not found")` | `:60-62` |

Dispatch to gameplay: `AC/MahjongPlayFlow.cs:1486-1505` (see §7.2).

### 3.2 Addressing: single-seat vs multi-seat vs broadcast

`pi0..pi3` are parsed for **every** command (`ReceiveCommand.cs:23-47`). What matters is
how the Exec handler uses them:

| Kind | Addressing in the client's own handling | Evidence |
|---|---|---|
| TSUMO (1) | **broadcast**: any seat's draw is applied, using the `pindex` child to pick the seat | `AC/ExecTsumo.cs:52-91` (no `PlayerIndexFlag` test) |
| SUTEHAI (2) | **broadcast**: own echo takes a short path, others animate + bookkeeping | `AC/ExecSutehai.cs:36-51`, `:57-118` |
| TSUMOCHOICES (15) | **single seat**: `playerIndex = PlayerIndexFlag.ToPlayerIndex()`, and if that is not my seat the handler returns before showing any window (⇒ sends nothing) | `AC/ExecTsumoChoices.cs:96`, `:193-196` |
| SUTECHOICES (16) | **multi-seat mask allowed**: returns immediately unless my own bit is set (`HasFlag(1 << my_seat)`), so the server may flag several claimants at once | `AC/ExecSuteChoices.cs:73-77` |
| PON (6), CHI (7), ANKAN (8), MINKAN (9), KAKAN (10) | **broadcast** (event announcement), actor from `pindex` (and `sute_pindex`) | `AC/ExecNaki.cs:34-129` |
| TSUMOAGARI (3), RON (4), RYUKYOKU (5) | **broadcast** settlement | `AC/ExecAgari.cs:66`, `:243`, `:482` |
| KYOKUSTART (17), KYOKUEND (23), SCORERANK (24) | **broadcast** (no seat test) | `AC/ExecKyoku.cs:68`, `:209`, `:249` |
| TYOKO (14) | announces a 長考 (long-think) for the seat in `pindex` | `AC/ExecKyoku.cs:57-66` |

**Mock consequence:** `pi0..pi3` must be present on every cell (else NRE, §1.8);
`TSUMOCHOICES` must carry exactly the acting seat's bit (`ToPlayerIndex()` takes the
lowest), while `SUTECHOICES` may carry several claimer bits but the inner `pindex` is
**not** the addressed seat (`ChoicesData.cs:110` reads it into an unused field -
see §3.4).

### 3.3 Per-command field lists

All element names below are children of the `cell_data_{seqNo}` element unless noted.
"required" = a missing child makes that command parse as empty/zero (never kills the
connection, but the action becomes nonsense).

**TSUMO (1)** - `AC/MFG.Taikyoku.Command.Receive/Tsumo.cs:17-27`

| Child | Req | Type | Meaning |
|---|---|---|---|
| `pindex` | yes | int | seat that draws |
| `pai` | yes | int | **tile id of the drawn tile, `Pai` encoding** (§8) — sent to every seat |

Applied: wall count `--` (`AC/ExecTsumo.cs:33-34`), tile added to that seat's hand
(`:57` first-draw path, `:91` normal path), last-tsumo warning at 0 (`:63-66`), dora
indicator flips after a kan (`:67-73`).

**SUTEHAI (2)** - `AC/MFG.Taikyoku.Command.Receive/Sutehai.cs:29-48`

| Child | Req | Type | Meaning |
|---|---|---|---|
| `pindex` | yes | int | discarder seat |
| `pai` | yes | int | discarded tile id (`Pai`) |
| `stat` | yes | int bitmask | bit0 riichi declared · bit1 tsumogiri · bit2 fire riichi · bit3 open riichi · bit4 atomic riichi (`:37-41`) |
| `cpu_pai` | no | int | CPU variant id for the discarded tile art (`:47`) |

Applied by `AC/ExecSutehai.cs` (river, furiten bookkeeping, open-riichi wait reveal
`:65-86`).

**TSUMOAGARI (3)** - `AC/MFG.Taikyoku.Command.Receive/TsumoAgari.cs:28-56`

| Child | Req | Type | Meaning |
|---|---|---|---|
| `pindex` | yes | int | winner |
| `yaku` | yes | element | winning hand breakdown — see **YakuData** (§3.4) |
| `dora_open` | yes | int | how many dora indicators are turned (ura-dora revealed) |
| `dora` | yes | int array | omote dora indicators (ids) |
| `ura_dora` | yes | int array | ura dora (ids) |
| `calc_score{0..3}` | yes | elements | per-seat settlement — see **ScoreData** (§3.4) |

Applied: `AC/ExecAgari.cs:66-241` (uradora reveal `:137`, per-seat score text `:189`,
final scores `:203`).

**RON (4)** - `AC/MFG.Taikyoku.Command.Receive/Ron.cs:30-65`

| Child | Req | Type | Meaning |
|---|---|---|---|
| `furikomi_pindex` | yes | int | seat that discarded the winning tile (the "discarder") |
| `ron_flg` | yes | int array (4) | per-seat 1 = that seat wins (multi-ron supported) |
| `dora_open`, `dora`, `ura_dora` | yes | as above (`:57-59`) | dora state at settlement; sets `HaveDoraData` |
| `yaku{0..3}` | yes | YakuData | per-seat winning hand (non-winners get an empty-ish block) |
| `calc_score{0..3}` | yes | ScoreData | per-seat settlement |

Applied: `AC/ExecAgari.cs:243-480` (per-seat ron flags `:257`, uradora reveal `:364`,
scores `:439`,`:464`).

**RYUKYOKU (5)** - `AC/MFG.Taikyoku.Command.Receive/RyuKyoku.cs:20-33`

| Child | Req | Type | Meaning |
|---|---|---|---|
| `ryukyoku_status{0..3}` | yes | element | per-seat tenpai state |
| ↳ `end_stat` | yes | int | 0/1 = that seat was in tenpai at the exhaustive draw |
| ↳ `machi_pai` | yes | int array | that seat's waiting tile ids |
| `calc_score{0..3}` | yes | ScoreData | tenpai payment settlement |

Applied: `AC/ExecAgari.cs:482-540`.

**PON (6)** - `AC/MFG.Taikyoku.Command.Receive/Pon.cs:21-32`

| Child | Req | Meaning |
|---|---|---|
| `pindex` | yes | claimer seat |
| `sute_pindex` | yes | seat whose discard was called |
| `pai` | yes | the claimed (discarded) tile id |
| `pon_pai` | yes | 4-int array = two pattern slots × 2 tiles ("the two tiles the claimer used"), filled pairwise (`:29` → `ChoicesData.cs:183-206` uses the same pairing rule) |

Applied: `AC/ExecNaki.cs:34-58` (`Players[pindex].Pon(Players[sute_pindex], pon_pai[0], pon_pai[1])` `:47`).

**CHI (7)** - `AC/MFG.Taikyoku.Command.Receive/Chi.cs:21-32`: same shape, array `chi_pai`
(`:29`); applied at `AC/ExecNaki.cs:60-79` (`:73`). Note the client ignores the *meld
shape* here and rebuilds it from the two ids; which of the three tiles came from the
discard is implied by the pair contents. `UNCONFIRMED:` exact `chi_pai` ordering convention
for the three call positions - confirm with one captured CHI cell per `chi_naki` value.

**ANKAN (8)** - `AC/MFG.Taikyoku.Command.Receive/Ankan.cs:17-28`: `pindex` (`:22`), `pai`
(`:23`). Applied `AC/ExecNaki.cs:81-94`.

**MINKAN (9)** - `AC/MFG.Taikyoku.Command.Receive/Minkan.cs:19-29`: `pindex` (`:24`),
`sute_pindex` (`:25`), `pai` (`:26`). Applied `AC/ExecNaki.cs:96-112` (remembers the
discarded-from seat for the rinshan animation at `:108`, used by `AC/ExecTsumo.cs:72`).

**KAKAN (10)** - `AC/MFG.Taikyoku.Command.Receive/Kakan.cs:17-28`:
`pindex` (`:22`), `pai` (`:23`). Applied `AC/ExecNaki.cs:114-129`.

**TYOKO (14)** - `AC/MFG.Taikyoku.Command.Receive/TyoKo.cs:15-25`: `pindex` (`:20`) only.
Sets a "long think" flag for that seat client-side (`AC/ExecKyoku.cs:57-66`).

**TSUMOCHOICES (15)** - `AC/MFG.Taikyoku.Command.Receive/TsumoChoices.cs:14-18` →
`ChoicesData.ParseTsumoChoices` (`AC/MFG.Taikyoku.Command.Receive/ChoicesData.cs:89-102`):

| Child | Req | Meaning |
|---|---|---|
| `select` | **yes** (missing ⇒ nothing is parsed at all, `:95-98`) | `SELECTABLE_TYPE_FLAG` of what this seat may do on its own draw (§3.4) |
| `kan_pai`, `kan_type` | no | up to 4 kan options: tile id + kan type (`:122-147`) |
| `ptn_num`, `ptn{0..n-1}` | no | tenpai/riichi patterns; each has `sute_pai`, `machi_num`, `machi_pai`, `stat` (`:208-232`, `:43-55`) |
| `kuikae_pai` | no | tile ids that must become **undiscardable** (kuikae 流局しぼみ protection) (`:234-249`; consumed at `AC/ExecTsumoChoices.cs:142-159`) |

No `pindex` child is read for this command - the addressed seat comes from `pi0..pi3`
(`AC/ExecTsumoChoices.cs:96`).

**SUTECHOICES (16)** - `AC/MFG.Taikyoku.Command.Receive/SuteChoices.cs:14-18` →
`ChoicesData.ParseSuteChoices` (`ChoicesData.cs:104-120`):

| Child | Req | Meaning |
|---|---|---|
| `select` | **yes** (missing ⇒ parse aborts, `:113-116`) | what the addressed seats may do (`SELECTABLE_TYPE_FLAG`) |
| `naki` | yes | second flag word for call sub-options (parsed, not used by the choice UI: only `select` drives the buttons — `AC/ExecSuteChoices.cs:293-296`) |
| `pindex` | yes | read into `ChoicesData.PlayerIndex`; **unused by the client** |
| `sute_pai` | yes | the discarded tile id being offered (`:111`) |
| `chi_pai` (+ optional `extra_chi_pai`) | no | chi patterns, 2 ints per pattern (`:149-181`) |
| `pon_pai` | no | pon patterns, 2 ints per pattern, max 2 patterns (`:183-206`) |
| `kan_pai`, `kan_type` | no | added kan options (`:122-147`) |

**KYOKUSTART (17)** - §7.1.
**KYOKUEND (23)** - `AC/MFG.Taikyoku.Command.Receive/KyokuEnd.cs:15-24`: `end_stat`
(`:20`): `1` ⇒ `IsEnd` ⇒ the whole match ends (client leaves the play scene), anything else
means "next kyoku coming" (`AC/ExecKyoku.cs:209-247`).
**SCORERANK (24)** - `AC/MFG.Taikyoku.Command.Receive/ScoreRank.cs:37-58`: optional
`riti_after{0..3}` blocks (element name built at `:44`) each with `score` and `rank`
(`:19-20`), plus optional `kyoutaku` (`:52-54`). Applied: riichi-stick count + all seat
scores/ranks are replaced wholesale (`AC/ExecKyoku.cs:249-266`).
`UNCONFIRMED:` exactly when the server emits 24 (field names say "after riichi 供託"; could
also be a generic mid-kyoku score sync) - one captured `cell_data_* kind="24"` resolves it.

### 3.4 Shared sub-structures

**`SELECTABLE_TYPE_FLAG`** (§2.3) is the button set. Observed usage:
`PON`/`CHI`/`KAN`/`RON` gate the call buttons (`AC/ExecSuteChoices.cs:293-296`),
`TSUMOAGARI` gates the tsumo-win analysis and button
(`AC/ExecTsumoChoices.cs:176`), `REACH`/`FIREREACH`/`OPENREACH`/`ATOMICREACH` gate riichi
declaration variants, `KYUSYU` is parsed but has no consumer found for it in the choice
handlers (九種九牌 has no client-side sender, §4.4).

**`ChoicesData.MachiPatternData`** (a `ptn{i}` block, `ChoicesData.cs:33-68`):
`sute_pai` = the tile whose discard keeps this wait; `machi_num`; `machi_pai` = waiting
tile ids; `stat` = per-wait status array where **bit 1 (`& 2`) == 0 means "this wait still
has tiles out there"** (`IsMachiPaiRemain`, `:57-68`). If `ptn_num` is present but zero
patterns survive, the client **clears `REACH` from `select`** (`:228-231`) - i.e. a server
that wants to forbid riichi for an empty wait sends `ptn_num=0`.

Pattern packing limits (fixed-size arrays, overflow is dropped silently):

| Element | Packing | Cap | Line |
|---|---|---|---|
| `chi_pai` + `extra_chi_pai` | 2 ints per pattern | **12 patterns** | `ChoicesData.cs:163-176` |
| `pon_pai` | 2 ints per pattern | **2 patterns** | `:188-201` |
| `kan_pai` / `kan_type` | 1 int per pattern, parallel arrays | **4 patterns**; `PaiID == 0` entries dropped | `:126-142` |
| `kuikae_pai` | 1 int each | unbounded | `:238-244` |
| `ptn{i}` | `ptn_num` count | unbounded | `:219-227` |

**`PlayerData`** (one per `player_info{i}`, `AC/MFG.Taikyoku.Command.Receive/PlayerData.cs:28-67`):
`jikaze` (seat wind, `:59`), `tepai` (space-separated 13 tile ids, `:60`), `score`
(int64, `:61`), `rank` (int, `:62`). The reconnect-only extras (`tsumo_pai`, `sute_pai[]`,
`sute_pai_state[]`, `reechi`, `okkake_reechi`, `naki{0..3}`) are read **only** by
`ParseFromReconnect` (`:33-53`).

**`FuroData`** (a `naki{i}` block): production `Parse` reads `type` (`FURO_TYPE`),
`kantype`, then `pai0..pai3` each with `pai_st` (orientation/display state) and `pai`
(`AC/MFG.Taikyoku.Command.Receive/FuroData.cs:37-47`, `:14-18`). The reconnect variant
reads `mentsu_type` instead of `type` and `pai_id` instead of `pai` (`:49-63`, `:20-24`).

**`YakuData`** (a `yaku`/`yaku{i}` block, `AC/MFG.Taikyoku.Command.Receive/YakuData.cs:28-50`):
all children are read with plain `Element()` + `ValueInt()` ⇒ **every one is mandatory**
(missing child = NRE, not a swallowed `NeedNot`): `pai` (winning tile id, `:30`),
`yaku_han` (`:31`), `han_num` (`:32`), `fu_num` (`:33`), `dora_num` (`:34`), optional
`bonus_han` (`:35-39`), `yaku1` + `yaku2` (two uint32 halves of a 64-bit yaku bitset,
`:40-42`), `tepai` (final concealed hand ids, `:43`), `naki0..3` (`:44-49`).
The bitset is decoded by `MahjongUtility.CreateYakusFromYakuBit` (`AC/MahjongUtility.cs:1594`)
where bit index == `MFG.Types.Yaku` enum ordinal.

**`ScoreData`** (a `calc_score{i}` block, `AC/MFG.Taikyoku.Command.Receive/ScoreData.cs:20-27`):
all mandatory (plain `Element` + `ValueInt`): `before_score` (i64), `yaku_score` (i64),
`kyotaku_score` (供託), `tumifu_score` (場積), `new_score` (i64), `wherefore`.
`wherefore == 1` makes the client **recompute** the payment with its own score table and
ignore `yaku_score` (`AC/Agari.cs:663`) - a useful lever: the server can either send final
numbers, or send han/fu in `yaku{i}` and let the client compute money.

---

## 4. Client → server: every `SEND_COMMAND_TYPE`

### 4.1 The generic parameter object

`AC/MFG.Taikyoku.Command.Send/SendCommand.cs:5-23`: ten ints (`SeqNo`, `Kind`, `TepaiPos`,
`TepaiPos2`, `PaiID`, `TepaiID`, `TepaiID2`, `ReachFlag`, `TsumoGiriFlag`, `ChiNaki`), all
default 0. Concrete senders just fill fields (`AC/MFG.Taikyoku.Command.Send/*.cs`); the
transport flattens them into `must=` slots 6-15 (§1.3). Therefore **any command the server
receives has 10 value slots regardless of whether they mean anything** - unused slots are
`0` (verified in the observed `NAKINASHI`/`NEXT_KYOKU_READY` shapes below).

### 4.2 Per-command meaning of the slots, with construction sites

| Kind | Constructor (file:line) | Slots 8-15 as the server sees them | Client call sites |
|---|---|---|---|
| 1 ENTRY | `Send/EntryOK.cs:5-8` | all 0. **Note: ENTRY is delivered as a `gget`, not a `gpost`** (`AC/MFG.Taikyoku.Command/CommandClient.cs:176-183`) | `AC/MatchingFlow.cs:230` |
| 2 SUTE_PAI | `Send/Sutehai.cs:7-14` | `tepai_pos` = index in the hand array of the discarded tile; `tepai_pos2`=0; `pai_id` = discarded tile id (`Pai`); `tepai_id`=0; `tepai_id2`=0; `reach_flg` = **`ReachLevel` when declaring riichi else 0** (`:12`); `tsumogiri_flg` = 1 if discarding the drawn tile (`:13`); `chi_naki`=0 | `AC/ExecTsumoChoices.cs:476` (timeout auto-discard), `:549`, `:575` (tap-to-discard paths) |
| 3 TSUMO_AGARI | `Send/TsumoAgari.cs:5-8` | all 0 (the server already knows the drawn tile from the prior `TSUMO`) | `AC/ExecTsumoChoices.cs:257`, `:813` |
| 4 RON_AGARI | `Send/Ron.cs:5-8` | all 0 (the offer is identified by the `must` seq, §5.5) | `AC/ExecSuteChoices.cs:245`, `:287` |
| 5 PON | `Send/Pon.cs:5-13` | `pai_id` = the claimed discard; `tepai_pos`,`tepai_id` = hand index + kind id of first calling tile; `tepai_pos2`,`tepai_id2` = same for the second; `chi_naki`=0 | `AC/ExecSuteChoices.cs:142` (constructed from `GetPaiPos([pon_pai[0], pon_pai[1]])` `:137-141`) |
| 6 CHI | `Send/Chi.cs:5-14` | same five, plus `chi_naki` = `ChiPos` 0/1/2 | `AC/ExecSuteChoices.cs:200` |
| 7 ANKAN | `Send/Ankan.cs:5-10` | `pai_id` = kan'd tile id; `tepai_pos` = hand index | `AC/ExecTsumoChoices.cs:957` |
| 8 MINKAN | `Send/Minkan.cs:5-9` | `pai_id` = the discarded tile id; `tepai_pos` = `min(hand index of the two calling tiles)` | `AC/ExecSuteChoices.cs:219` |
| 9 KAKAN | `Send/Kakan.cs:5-10` | `pai_id` = tile added to an open pon; `tepai_pos` = hand index | `AC/ExecTsumoChoices.cs:964` |
| 10 KYUSYUKYUHAI | `Send/Kyusyu.cs:5-8` | all 0 | **no gameplay caller** (only the class exists) |
| 11 NAKINASHI | `Send/Tsugi.cs:5-8` | all 0 | `AC/ExecSuteChoices.cs:256` (decline button), `:278` (no-input auto mode), `:327` (submenu cancel); totals in capture lines 45-46 |
| 12 CYOUKOU | `Send/Tyoko.cs:5-8` | all 0 | no gameplay caller |
| 13 KIKEN | `Send/Kiken.cs:5-8` | all 0 | `AC/MahjongPlayFlow.cs:761` (resign) |
| 14 RECONNECT | `Send/Reconnect.cs:5-8` | all 0 | only the test harness `AC/WebRequestTest.cs:399-405` |
| 15 NEXT_KYOKU_READY | `Send/NextKyokuReady.cs:5-8` | all 0 | `AC/ExecKyoku.cs:235` (after a non-final `KYOKUEND`) |

Real calibration line in the repo (same template, seat 0, discard of `Pai.S7` … the
capture's line is a `pai_id:32` = 南 discard):

```
GPost param: SUTE_PAI(2). pindex:0 next_sno:373 tepai_pos:11 tepai_pos2:0 pai_id:32(32) tepai_id:0(0) tepai_id2:0(0) reach_flg:0 tsumogiri_flg:0 chi_naki:0
```
(`capture/s2-evidence-sanitized.md:51`; the matching raw body is capture line 42.) That
line proves the manual-input case: the player selected a hand tile (index 11) and the
server accepted a **non-tsumogiri** discard.

### 4.3 `tepai_pos` semantics (what the server can rely on)

`tepai_pos` is an **index into the client's hand array**, not a tile kind, and the array
layout is deterministic:

* `AC/Hand.cs:186-191` — on a draw the client first sorts the existing tiles
  (`TileSort()` `:188`, comparator `:462-495`: by `Pai` value, red fives sorted next to
  their plain twin) and then **appends the drawn tile last without re-sorting**. So with 14
  tiles the drawn tile is always array index 13 (count − 1).
* `AC/Hand.cs:915-922` — `IsTsumogiri(pos)` is literally `pos == Tiles.Count - 1` (while in
  tsumo-choice mode), which is exactly what `tsumogiri_flg` reports
  (`AC/ExecTsumoChoices.cs:539`,`:559` → `AC/MFG.Taikyoku.Command.Send/Sutehai.cs:13`).
  This is why the captured discard reads `tepai_pos:13 … tsumogiri_flg:1` (capture line 42)
  and a hand-selected discard reads `tepai_pos:11 … tsumogiri_flg:0` (capture line 51).
* For pon/chi/kan the index comes from `AC/Hand.cs:235-258` (`GetPaiPos`, first match per
  requested id against the current array order, consumed tiles masked so duplicates map to
  distinct indices) and `AC/Hand.cs:289-299` (`GetItemPos`).
* `AC/ExecTsumoChoices.cs:476` timeout path uses `GetTimeUpPaiPos()`; the manual path uses
  the tapped tile's position - both pass the same `pai_id`, so the server can validate
  consistency of `tepai_pos`/`pai_id` against the hand it dealt, or ignore `tepai_pos`
  entirely. `UNCONFIRMED:` whether the live server validates `tepai_pos` at all
  (the client never re-reads it) - confirm by sending a deliberately wrong `tepai_pos` in a
  dev session and watching for a reject/`gpost` error code.

### 4.4 Sends the recovered client never emits

`KYUSYUKYUHAI(10)` (九種九牌 early draw), `CYOUKOU(12)` (長考 request) and `RECONNECT(14)`
have classes but **no gameplay caller** (grep of `new …(` across the assembly: only
`AC/WebRequestTest.cs`, `AC/TsumoGiriTestFlow.cs` harness call sites for discard/pass/ready,
and `AC/WebRequestTest.cs:399-405` for RECONNECT). A mock can accept-and-ignore them, but
the server still needs the equivalents for CPU seats.

---

## 5. Sequence numbers, windows, and gating

### 5.1 Where seqs come from

* The server owns the log. Each match has an append-only cell log; `gget`/`gpost` responses
  describe a slice of it with `cell_sno start/count` + `cell_data_{seq}` and the client's
  new cursor is `start + count` (`AC/MFG.Taikyoku/TaikyokuData.cs:44-58`).
* The client starts at `entry_game`'s `<entry><next_sno>`
  (`AC/MatchingFlow.cs:218` ← `AC/MFG.Taikyoku/EntryTakuInfo.cs:52`).
* Seqs are per-table and monotonic across kyokus (never reset mid-match; only `Reset()` on
  a new `entry_game` or a `recon` re-bases it: `AC/MFG.Taikyoku.Command/CommandClient.cs:253-262`, `:197-204`).

### 5.2 Cursor advance (subtle and load-bearing)

`m_cmdFetchPosition` is updated **only when the response carried at least one new
command** (`AC/MFG.Taikyoku.Command/CommandClient.cs:135-140` for `gget`, `:217-221` for
`gpost`). Consequences for the server:

* An empty window (`count="0"`) leaves the cursor where it was - safe, idempotent polling.
* A window with `available="0"` yields `NextFetchSeqNo = 0` and, if the client had already
  buffered commands, that `0` is **not** applied (count is 0). Do not send
  `available="0"` together with `count > 0`.
* Commands are de-duplicated on arrival: anything with `SeqNo <= max(seq in buffer)` is
  dropped (`:126-134`, `:208-216`). A server may therefore retransmit a window overlapping
  what the client already has - but a **retransmitted seq that the client has already
  consumed is silently skipped, never replayed**.

### 5.3 How the server "answers a window"

There is no explicit ack. Two mechanisms carry acceptance:

1. **`next_sno` on `entry_game` / `recon`** - server-side cursor for a fresh entry
   (`AC/MFG.Taikyoku/EntryTakuInfo.cs:52`; `AC/MFG.Taikyoku.Command.Receive/TaikyokuStartInfo.cs:100-104`
   → applied at `AC/MFG.Taikyoku.Command/CommandClient.cs:202`). `gpost` bodies carry **no**
   `next_sno` field; the seq they carry is inside `must=` slot 6.
2. **New cells in the same or a later window** - the answer to any send is "the log moved
   forward". A client-side send blocks on `IsBusy` while its `gpost` is in flight
   (`AC/MFG.Taikyoku.Command/CommandClient.cs:171`,`:234`; gameplay waits with
   `await UniTask.WaitWhile(() => m_commandClient.IsBusy)`, `AC/ExecKyoku.cs:244`).

The **`ready` flag** is a per-table latch, not per-kyoku:
`NotifyReadyToKyokustart()` sets it once (`AC/MahjongPlayFlow.cs:674` →
`AC/MFG.Taikyoku.Command/CommandClient.cs:76-79`) and `gget` then always sends `ready=1`
until the client is reset (`Reset` clears it, `:259`). Per-kyoku readiness is instead the
`NEXT_KYOKU_READY(15)` **gpost** (§7.4). The gate the client waits on is
`game/all_ready` in a `gget` response: it loops `Fetch()` until that is non-zero
(`AC/MahjongPlayFlow.cs:675-678`), and **an absent `all_ready` element counts as ready**
(`AC/MFG.GameRequest/GGet.cs:43-51`) - so the mock can simply omit `all_ready`, or send
`<all_ready>1</all_ready>` after the last seat latched.

### 5.4 What the client does when the server has nothing new

The play loop polls (`AC/MahjongPlayFlow.cs:1451-1457`: when the buffer is empty the client
fires `m_commandClient.Fetch().Forget()` and does not block on it). An empty window means
the buffer stays empty and the loop polls again - a `gget` roughly every other frame while
the client waits (326 `gget` for a 2-match session, capture lines 32-33). No
negative/empty status code is used: an idle `gget` still returns `code 0` with `count="0"`.

`ExecutableCount()`/`Current()`/`MoveNext()` are plain buffer-position accessors
(`AC/MFG.Taikyoku.Command/CommandClient.cs:81-104`); the client consumes commands in strict
seq order and never skips one it did not understand (unknown kinds were dropped at parse
time, §3.1).

### 5.5 Seq binding of a client answer (the part a mock must reproduce exactly)

`AC/MFG.Taikyoku.Command/CommandClient.cs:238-250` (`EvaluateGpostSeqNo`):

| Send | seq written into `must` slot 6 |
|---|---|
| `RON_AGARI`, `PON`, `CHI`, `MINKAN`, **`NAKINASHI`** | `max(SeqNo of every SUTECHOICES in the buffer) + 1` (`:242-247`) |
| everything else (SUTE_PAI, TSUMO_AGARI, ANKAN, KAKAN, KIKEN, NEXT_KYOKU_READY, …) | the current fetch cursor `m_cmdFetchPosition` (`:249`) |

Live corroboration: the `gget` at cursor 554 (capture line 41) is answered by a discard at
seq 555 (capture line 42) - the discard sits immediately after the window that delivered
the draw/choices.

Three hard consequences:

* A **claim-window answer is tied to the choices cell**, not to the poll cursor. The server
  must therefore, for the `SUTECHOICES` at seq *S*, accept the next cell at seq *S+1*
  carrying the seat's answer, and it must tolerate several seats each claiming
  `S+1`-derived numbering only via their own `gpost`s (each claimant answers the same
  window; the server sequences their effects into the log itself).
* `…Max()` is LINQ over the buffered SUTECHOICES and **throws
  `InvalidOperationException` if the buffer holds none** (`:247`): a server that
  (a) emits a `recon` replay whose window dropped the open `SUTECHOICES`, then (b) receives
  a `NAKINASHI`, hits a client-side crash. Real traffic always has the choices cell in the
  buffer because it is never trimmed except by a reconnect
  (`AC/MFG.Taikyoku.Command/CommandClient.cs:126-134`, `:197-204`).
* **The newest choices cell in the buffer is not the cell being answered.** A rejoiner is pointed at
  its kyoku's `KYOKUSTART` and is handed every `SUTECHOICES` cell of that kyoku, and it answers each
  one its `pi` bit names (`AC/ExecSuteChoices.cs:74`, `:325-328`, `:390-391`). For a seat behind the
  live edge, `max(...)+1` therefore names a window that is no longer live. This server reads the number
  as a floor, not an identity: a claim-window reply naming a choices cell older than the live window's
  own cell is refused, as is a `PON`/`CHI`/`MINKAN` whose `pai_id` is not the live discard. And a
  resolved offer is published inert - the cell stays in the log where the client walks it, with its
  `pi` bits cleared - so a seat is never handed a question the engine no longer waits on. Tracked as
  `docs/31-open-defect-ledger.md` L84; the reasoning and the checks are in
  `.audit/stale-offer-replay-report.md`.

### 5.6 Deadlines and auto-answers the server must expect without asking

* Think clock: 5 s (4-player) / 15 s (2-player) plus 20 s extra unless an agari is offered
  (`AC/ThinkCount.cs:18-24`, `InitKyoku` `:121-131`, `IsTimeUp` `:133-147`).
* On expiry the client **sends by itself**: tsumo window auto-discards (`AC/ExecTsumoChoices.cs:535-552`)
  and claim windows auto-pass / auto-ron with auto-agari setting
  (`AC/ExecSuteChoices.cs:251-259`, `:276-290`). A mock must therefore never assume a human
  seat answers promptly, and must keep accepting the same window's answer after a stall.
* Idle cabinet behavior (auto-tsumogiri screen) is documented live in
  `docs/13-s2-provider-capture.md:52-54`.

---

## 6. Match lifecycle (session → table → match end)

### 6.1 Endpoint order

| Step | Endpoint | Request class | Call site |
|---|---|---|---|
| boot | `appli_info` | `AC/MFG.GameRequest/AppliInfo.cs:24-25` | `AC/LogoFlow.cs:70`; `AC/GameDataManager.cs:137`,`:179` |
| boot (guest only) | `create_player` | `AC/MFG.GameRequest/CreatePlayer.cs:18-19` | `AC/GameDataManager.cs:135` |
| card login | `login` | `AC/MFG.GameRequest/Login.cs:19-21` | `AC/GameDataManager.cs:139` |
| identity | `get_menudata` | `AC/MFG.GameRequest/GetMenuData.cs:55-57` | `AC/GameDataManager.cs:141` |
| save state | `client_state_read` / `client_state_write` | `AC/MFG.GameRequest/ClientStateRead.cs:82-83` / `ClientStateWrite.cs:21-22` | `AC/GameDataManager.cs:146-161`, `:182-273`; `AC/ResultFlow.cs:217-235` |
| lobby misc | `mission_date`, `gacha_info`, `player_record` | `AC/MFG.GameRequest/MissionDate.cs:21`, `GachaInfoRequest.cs:19`, `GetRecord.cs:21` | `AC/GameDataManager.cs:155-161` |
| pick a table | `entry_game` | `AC/MFG.GameRequest/EntryGame.cs:59-60` | `AC/MatchingFlow.cs:216` (scene entered from `AC/HomeFlow.cs:1905`) |
| seat latch | `gget` with `ready=1` + `EntryOK`(→gget) | `AC/MFG.GameRequest/GGet.cs:29-30` | `AC/MatchingFlow.cs:230`; `AC/MFG.Taikyoku.Command/CommandClient.cs:176-183` |
| matching | `gget` loop until `mwait/status != 0` | 〃 | `AC/MatchingFlow.cs:243-246` |
| stamps during matching | `gchat` (`scene=MATCHING` fixed) | `AC/MFG.GameRequest/GChat.cs:25-26` | `AC/MatchingFlow.cs:500`,`:516` |
| kyoku gate | `gget` loop until `all_ready` | 〃 | `AC/MahjongPlayFlow.cs:674-678` |
| play | `gget` (poll) + `gpost` (every action) | §1 | `AC/MahjongPlayFlow.cs:1452-1455`; `AC/MFG.Taikyoku.Command/CommandClient.cs:186-187` |
| break windows | `keep_alive` (60 s) | `AC/MFG.GameRequest/KeepAlive.cs:15-16` | `AC/BreakTimeWindow.cs:227-236` |
| match end | `end_game` | `AC/MFG.GameRequest/EndGame.cs:21-22` | `AC/MahjongPlayFlow.cs:772` |
| resign | `gpost KIKEN(13)` then `kiken_game` | `AC/MFG.GameRequest/KikenGame.cs:21-23` | `AC/MahjongPlayFlow.cs:761-762` |
| result | `client_state_write` ×N | 〃 | `AC/ResultFlow.cs:190`,`:217-235` |
| teardown | `logout` | `AC/MFG.GameRequest/Logout.cs:15` | `AC/GameoverFlow.cs:82`; `AC/CardEntryScene.cs:932`,`:1047` |

### 6.2 Identity minting

| Id | Minted by | Stored in | Sent in |
|---|---|---|---|
| `pcuid` (login session) | `login` → `auth/session_id` (`AC/MFG.GameRequest/Login.cs:30-31`) | `AccountGameData.AppSessionID` (`AC/GameDataManager.cs:140`) | every body's `pcuid` (`AC/MFG.GameRequest/RequestApiBase.cs:65`) |
| `mid` (player id) | `get_menudata` → `menudata/mpdata/mid` (`AC/MFG.GameRequest/GetMenuData.cs:65-67` → `AC/MFG.GameData/PlayerGameData.cs:392-400`) | `AccountGameData.PlayerID` (`AC/MFG.GameData/AccountGameData.cs:30`) | `gget.must` slot 5, `gpost.mid` + `must` slot 5, `end_game.mid` |
| `tid` (table id) | `entry_game` → `<entry><tid>` (`AC/MFG.Taikyoku/EntryTakuInfo.cs:50`) | `TaikyokuGameData.TakuInfo` via `Apply` (`AC/MFG.GameData/TaikyokuGameData.cs:36-39`; called `AC/MatchingFlow.cs:217`) → `CommandClient.TakuID` (`AC/MatchingFlow.cs:219`) | `gpost.tid`, `gget.must` slot 3, `end_game.tid` |
| `pindex` (seat) | `entry_game` → `<entry><pindex>` (`EntryTakuInfo.cs:51`) | `CommandClient.PlayerIndex` (`AC/MatchingFlow.cs:220`) | `gpost.pindex`, `must` slot 4 (`mwait/pindex` is parsed and discarded, `AC/MFG.Taikyoku/MatchingData.cs:36-39`) |
| `gserv_id` | `entry_game` → `<entry><gserv_id>` (`EntryTakuInfo.cs:49`) | `TakuInfo.GameServiceID` | `gget`, `gpost`, `gchat` — **not** `end_game` (`AC/MFG.GameRequest/EndGame.cs:22`) |
| `gserv_url` | `entry_game` → `<entry><gserv_url>` (`EntryTakuInfo.cs:64`) | `TakuInfo.GameServiceUrl` | only the (uncalled) reconnect endpoint URL |
| taku session id | `<entry><session_id>`? | `EntryTakuInfo.SessionID` is **never assigned** (`:11`; `Parse` skips it) and `RequestApiBase.TaikyokuSessionID` (`:67`) is used by no body | — `UNCONFIRMED:` whether the live server even sends it; a captured `entry_game` response settles it |
| fetch cursor | `entry_game` → `<entry><next_sno>` (`EntryTakuInfo.cs:52`) | `CommandClient.m_cmdFetchPosition` (`AC/MatchingFlow.cs:218`) | `gget.must` slot 6 |

### 6.3 `entry_game` request and response

Request (`AC/MFG.GameRequest/EntryGame.cs:25-60`): `web_id`, `area_id`, `pcuid`,
`gmode` (from `MahjongGameData.GetGameMode`, `AC/MFG.GameData/MahjongGameData.cs:273-275`),
`taku_class=1` (`NORMAL`), `match_mode` 1/2 (`WHOLE`/`PRIVATE`), `passphrase`,
`teammatch`, `consume_oids`, `entry_id`, `pro_stay`, `competition_id`,
`shop_competition_id`, `debug_score=-1`.

Response `<entry>` - **all 17 children are `MustNeedElement`**
(`AC/MFG.Taikyoku/EntryTakuInfo.cs:47-67`): `gserv_id`, `tid`, `pindex`, `next_sno`,
`last_cyoukou_num`, `cyoukou_num`, `ste_oya1_limit_time`, `ste_limit_time`,
`ste_reechi1_limit_time`, `naki_limit_time`, `agari_limit_time`,
`naki_choice_limit_time`, `reechi_choice_limit_time`, `last_cyoukou_limit_time`,
`last_time`, `gserv_url`, `pay_mode`. An alternative root `preference_matching`
short-circuits this branch (`EntryGame.cs:67-83`). Units of the limit fields are
`UNCONFIRMED` (the recovered client only reads them into `TakuInfo`; the only consumer found
is the matching countdown `MachingLimitTime`, `AC/MatchingFlow.cs:223-227`).

### 6.4 First kyoku handshake

`MatchingFlow` → `GotoNextScene("MahjongPlay")` (`AC/MatchingFlow.cs:277`) →
`MahjongPlayFlow.OnInitialize`: builds seats from `PlayerInfos` in **absolute seat order**
(`AC/MahjongPlayFlow.cs:1584-1612`, so `Players[i]` ↔ `pindex i`), then
`NotifyReadyToKyokustart()` + `gget` loop until `all_ready` (`:672-679`) →
`OnExecute` → `TaikyokuStart()` → `ExecTaikyoku()` (`:725`, `:748`) → first buffered command
must be `KYOKUSTART(17)` (`:1501`).

### 6.5 Match end and settlement

Normal: loop exits after the final `KYOKUEND(end_stat=1)` (`AC/ExecKyoku.cs:213-221`) →
`end_game` with `(tid, pindex, mid)` (`AC/MahjongPlayFlow.cs:772`); body
`web_id&area_id&pcuid&tid&pindex&mid&version=2.0.2`, **no `gserv_id`**
(`AC/MFG.GameRequest/EndGame.cs:21-22`; observed verbatim at capture lines 43/54/61).
Response `mgresult` (`AC/MFG.GameData/MahjongResultInfo.cs:75-186`): `gmode`,
`taku_class`, `continue_state`, `continue_fee`, `old`/`new` player blocks,
`breakdown/exp/{base_exp,group_adjust_exp,bonus_list}`,
`breakdown/affinity_point/…`, `battle_item_use_info/mfg_item`, `player_{0..3}` with
`rank`/`score`/`uma` (`AC/MFG.GameData/PlayerResultInfo.cs:16-27`; live sample at capture
lines 56-59) and `g2result/r`. Stored at `AC/MahjongPlayFlow.cs:786-788`, consumed by
`ResultFlow` + 12× `client_state_write` (`AC/ResultFlow.cs:190`,`:217-235`).
Resign path: `gpost KIKEN(13)` then `kiken_game` (identical body/response,
`AC/MFG.GameRequest/KikenGame.cs:21-23`,`:30-31`).

### 6.6 Reconnect (two different things)

* **Mid-match, production path**: the client sends `gpost` `RECONNECT(14)`; the server
  answers that `gpost` with a `<recon>` element. `GPost` parses `recon` with
  `TaikyokuStartInfo.Parse` (`AC/MFG.GameRequest/GPost.cs:67-72`), whose player pass looks
  for **`player_info{i}`** (`AC/MFG.Taikyoku.Command.Receive/TaikyokuStartInfo.cs:158-172`),
  plus optional `next_sno`, `last_cyoukou_num`, `cyoukou_num`, `minkan_flag`
  (`:100-120`). `CommandClient` then clears its buffer, adopts the cells of the same
  response, and sets the cursor from `recon/next_sno` (`AC/MFG.Taikyoku.Command/CommandClient.cs:197-204`).
  `UNCONFIRMED:` a production caller of `new Send.Reconnect()` (only `AC/WebRequestTest.cs:401`
  exists) - a captured `gpost` with `kind=14` settles it.
* `TaikyokuStartInfo.ParseFromReconnect` / `ParsePlayerFromReconnect` expect
  **`player{i}`** with the extra live-state fields (`:66-79`, `:174-188` →
  `PlayerData.ParseFromReconnect` `:33-53` → `FuroData.ParseFromReconnect` `:49-63`) but
  **nothing in the recovered assembly calls the top-level `ParseFromReconnect`** ⇒ if the
  live server emits `player{i}` inside `recon`, the shipping client parses an empty Players
  list there. Confirm with one captured reconnect response.
* The standalone `reconnect/{ver}/{app_session}/{web_id}/` endpoint
  (`AC/MFG.GameRequest/Reconnect.cs:18-19`, empty POST, URL built from `gserv_id:00` +
  `gserv_url`) has **no call site** (`AC/MFG.GameRequest/Reconnect.cs:9` is the only hit).
* `get_menudata`'s `<recon>` block is parsed for `gmode` only
  (`AC/MFG.GameRequest/GetMenuData.cs:40-50`,`:68-72`); its consumer
  (`TaikyokuGameData.Apply(ReconnectData)`, `AC/MFG.GameData/TaikyokuGameData.cs:41-45`) has
  no caller.

Continue between matches does **not** reconnect: `AC/ContinueFlow.cs:412` →
`GameDataManager.ExecLoadAtContinue` (`AC/GameDataManager.cs:182-273`) + reset
(`:461-470`,`:504-509`) → a fresh `entry_game`.

### 6.7 Endpoints a mock must answer for a playable match

`/login`, `/get_menudata`, `/create_player` (guest), `/appli_info`, `/client_state_read`,
`/client_state_write`, `/entry_game`, `/gget`, `/gpost`, `/gchat`, `/end_game`,
`/kiken_game`, `/keep_alive`, `/logout`; optional/adjacent: `/mission_date`, `/gacha_info`,
`/player_record`, `/janpon_log`, `/present_done`, `/dojo_get_status`, `/chk_tabooword`
(the last four were seen in the live session totals, capture lines 19-37). Defined but never
called in recovered code: `/end_show` (`AC/MFG.GameRequest/EndShow.cs:24-25`),
`/gget_stamp_info` (`AC/MFG.GameRequest/GGetStampInfo.cs:26-27`),
`/reconnect/…` (§6.6).

---

## 7. Kyoku lifecycle as a command sequence

### 7.1 `KYOKUSTART(17)` contents and how the client applies them

Parsed by `AC/MFG.Taikyoku.Command.Receive/KyokuStart.cs:49-125`:

| Child | Req | Field | Client meaning (application site) |
|---|---|---|---|
| `chicya` | yes | int | 地邪 seat index (client logs/stores only) — `KyokuStart.cs:54` |
| `oya` | yes | int | dealer seat (`:55`) — **parsed but unused**: grep for `OyaPlayerIndex` finds only the two assignments (`KyokuStart.cs:55`, `AC/MFG.Taikyoku.Command.Receive/TaikyokuStartInfo.cs:86`); the client answers "am I dealer?" from `player_info{me}/jikaze == 0` (`AC/ExecKyoku.cs:143-146`) |
| `sai` | yes | int array | 賽 sub-mode/seed values; **parsed, never used** (`:56`) |
| `ba` | yes | int | round/wind index 0-3 (`:57`); display "東"/"南" (`AC/TakuInfo.cs:304`) and the engine's bakaze = `HaiID 27 + ba` (`AC/TaikyokuState.cs:300-306`) |
| `kyoku` | yes | int | 0-3 = 1st-4th kyoku of the round (`:58`, `AC/TakuInfo.cs:305-309`) |
| `all_last` | yes | int | 1 ⇒ 南場ラス display (`:59`, `AC/TakuInfo.cs:298-300`) |
| `honba` | yes | int | 本場 counter (`:60`, `AC/TakuInfo.cs:312`) |
| `rencyan` | yes | int | dealer-retained streak (`:61`; not consumed by the recovered UI) |
| `kyoutaku` | yes | int | 供託 (riichi boxes) total (`:62`, `AC/TakuInfo.cs:313`) |
| `nokori` | yes | int | **wall count before the first draw** (`:63` → `AC/ExecKyoku.cs:125`; each `TSUMO` decrements it, `AC/ExecTsumo.cs:33`) |
| `dora_open` | yes | int | how many indicators are open at kyoku start (`KyokuStart.cs:78` → `AC/ExecKyoku.cs:119-124`) |
| `dora` | yes | int array | omote dora indicators; **needs ≥5 (≥9 accel-dora)** (`:79` → `AC/ExecKyoku.cs:112-117`) |
| `ura_dora` | yes | int array | paired ura indicators; same length requirement (`:81`) |
| `yama_cnt`, `yama` | no | int / int array | whole wall order; **parsed, never used** (`:88-98`) |
| `rinshan` | no | int array | rinshan (kan-replacement) tiles; **parsed, never used** (`:100-109`) |
| `player_info{0..3}` | yes (all four, §1.8) | `PlayerData` | `jikaze`, `tepai` (13 ids), `score`, `rank` per absolute seat (`:111-125` → `AC/ExecKyoku.cs:189-207`, `:132-141`) |

So: the deal is conveyed as **13 ids per seat in `player_info{i}/tepai`**; the dealer's 14th
tile arrives as the first `TSUMO` (`AC/ExecKyoku.cs:182` arms `m_isFirstTsumo`,
`AC/ExecTsumo.cs:52-62` consumes it) - `UNCONFIRMED:` whether the live server ever puts 14
ids in the dealer's `tepai` (the client deals exactly 13 regardless); one captured
`KYOKUSTART` settles it.

### 7.2 The dispatch loop

`AC/MahjongPlayFlow.cs:1440-1514` - a per-frame state machine (`m_step`), not a coroutine
per command:

```
loop while m_isTaikyokuActive, one step per frame                                   // :1440-1443
step 0: if (ExecutableCount() <= 0) m_commandClient.Fetch().Forget(); m_step++      // :1451-1457
step 1: if (!m_commandClient.IsBusy) m_step++            (else stay)                // :1458-1463
step 2: if (ExecutableCount() <= 0) { m_step = 0; break; }                          // :1466-1469
        cmd = m_commandClient.Current();                                            // :1471
        if (m_isNextCmd) { m_isNextCmd = false; m_commandClient.MoveNext(); break; } // :1472-1477
        m_isNextCmd = cmd.Kind switch { … };   // C# switch expression              // :1486-1505
          TSUMO→ExecTsumo  SUTEHAI→ExecSutehai  TSUMOAGARI/RON/RYUKYOKU→ExecAgari
          PON/CHI/ANKAN/MINKAN/KAKAN→ExecNaki  TYOKO/KYOKUSTART/KYOKUEND/SCORERANK→ExecKyoku
          TSUMOCHOICES→ExecTsumoChoices  SUTECHOICES→ExecSuteChoices  _→true
```

A handler returns `true` when its command is fully consumed (the next tick advances to the
following command, `:1472-1477`) and `false` to be re-run on the next frame - that is how a
choice window (`ExecTsumoChoices`, `ExecSuteChoices`) stays open across frames while it
waits for touch input. The dispatch also logs seq/kind/`PlayerIndexFlag` per command when
`DebugFlag.EnableCommandClientLog` is on (`:1483`, `:1509`).

### 7.3 One kyoku, in the order the client expects

| # | Cell the server appends | Seq logic | What the client does next |
|---|---|---|---|
| 1 | `KYOKUSTART(17)` | next seq | deal, dora, scores, timers armed, `m_isFirstTsumo = true` (`AC/ExecKyoku.cs:68-187`) |
| 2 | `TSUMO(1)` `pi{dealer}` | next seq | wall −1, tile shown to everyone (`AC/ExecTsumo.cs:33-91`) |
| 3 | `TSUMOCHOICES(15)` `pi{dealer}` only | next seq | if mine: think window; I answer `SUTE_PAI(2)` (or `TSUMO_AGARI(3)`, `ANKAN(7)`, riichi via `reach_flg`) — seq = cursor (§5.5); if not mine: nothing is sent (`AC/ExecTsumoChoices.cs:193-196`) |
| 4 | `SUTEHAI(2)` | server's own seq | river update for everyone (`AC/ExecSutehai.cs`) |
| 5a | *(nobody can call)* — server may emit nothing and go to 2 for the next seat | — | — |
| 5b | `SUTECHOICES(16)` `pi` = claimers (may be several) | seq *S* | each flagged seat answers `PON(5)`/`CHI(6)`/`MINKAN(8)`/`RON_AGARI(4)`/`NAKINASHI(11)`, each with seq *S+1* (`AC/MFG.Taikyoku.Command/CommandClient.cs:242-247`); unflagged seats send nothing (`AC/ExecSuteChoices.cs:74-77`) |
| 6 | `PON/CHI/MINKAN/KAKAN` or `ANKAN/KAKAN` from the tsumo window | server's own seq | meld applied, then `TSUMO` again for the actor after a kan (`AC/ExecNaki.cs`, `AC/ExecTsumo.cs:67-73`) |
| 7 | `TSUMOAGARI(3)` / `RON(4)` / `RYUKYUKU(5)` | server's own seq | settlement UI, uradora reveal, score panels (`AC/ExecAgari.cs`) |
| 8 | `SCORERANK(24)` (when scores/ranks change) | server's own seq | replaces seat scores/ranks + 供託 count (`AC/ExecKyoku.cs:249-266`) |
| 9 | `KYOKUEND(23)` with `end_stat != 1` | server's own seq | client sends `NEXT_KYOKU_READY(15)` and waits (`AC/ExecKyoku.cs:222-245`) |
| 10 | after all four seats sent `NEXT_KYOKU_READY`, the next cell is `KYOKUSTART` of the next kyoku | — | repeat from 1 |

`end_stat == 1` in `KYOKUEND` is the **match** end: the client stops the loop
(`m_isTaikyokuActive = false`, `AC/ExecKyoku.cs:213-221`), leaves the scene
(`AC/MahjongPlayFlow.cs:732-737`) and calls `end_game` (`:772`).

Observed traffic mix consistent with this shape: one session's client sends were
`SUTE_PAI ×132`, `NAKINASHI ×26`, `NEXT_KYOKU_READY ×8` (capture lines 45-48) - i.e. ~8
kyokus, and the human seat passed on ~26 offered claim windows.

### 7.4 Per-kyoku readiness

`NEXT_KYOKU_READY(15)` is the only per-kyoku ready signal, and it is a `gpost` (§6.3 note on
`ready`). `UNCONFIRMED:` whether the server requires **all four** seats' `NEXT_KYOKU_READY`
before emitting the next `KYOKUSTART` (CPU seats answer server-side) or proceeds after the
human seat's. Test: in a mock, hold `KYOKUSTART` back after only the human's ready and watch
for a client timeout/retry (none exists client-side: it just displays 請等待 and keeps
polling, `AC/ExecKyoku.cs:236-244`).

---

## 8. Tile encoding (`pai_id`)

### 8.1 Two id spaces exist client-side; only one is on the wire

| Space | Type | Layout | Used for |
|---|---|---|---|
| `MFG.Types.Pai` | `int` enum | `suit*10 + number`: man 1-9, sou 11-19, pin 21-29, honors 31-37 (J1-E, J2-S, J3-W, J4-N, J5-C, J6-F, J7-P); "red" variants = normal **+64** with holes at 10/20/30 (M1R=65 … J7R=101); `NONE = 0` | **presentation and the network wire** — every `pai`, `tepai`, `dora`, `yama`, `pon_pai`, … value (`AC/MFG.Types/Pai.cs:5-73`) |
| `MFG.Types.HaiID` | `sbyte` enum | 34 kinds, contiguous: M1=0…M9=8, S1=9…S9=17, P1=18…P9=26, J1=27…J7=33, `Max=34`; red = normal **+64** (M1R=64, S1R=73, …) | internal logic only (shanten/yaku engine) (`AC/MFG.Types/HaiID.cs:5-50`) |

**There is no 0-135 wall-index type anywhere client-side.** The wall is conveyed as an id
array (`yama`, §7.1) and the client never uses it; tile identity on the wire is always the
`Pai` value.

Conversion (`AC/MFG.Types/PaiExtention.cs:17-35`): `HaiID = Pai/10*9 + (Pai%10 - 1)`
(`:18-23`); `Pai = SuID + 1 + mark*10` (`:26-34`). Red is stripped before conversion by
`ToNormal` (`:8-15`), so a red five converts to its plain kind for logic while the wire
value keeps the +64.

### 8.2 Wire examples proving the encoding

* `pai_id:32` with `tsumogiri_flg:0` - 32 = `Pai.J2` = 南 (South), discarded from hand
  position 11 (capture line 51). A `HaiID`-encoded South would be 28.
* `must=…/2/13/0/17/0/0/0/1/0` — discarded `pai_id = 17` = `Pai.S7` (7-sou) at hand index 13
  with `tsumogiri_flg=1` (capture line 42).
* Dora/hand arrays are the same encoding, space-separated text (`ValueArrayInt`,
  `AC/MFG.Sytem.Utilities.Xml/Extensions.cs:57-60`) cast straight to `(Pai)` in
  `AC/ExecKyoku.cs:115-116`,`:139` and in `AC/ExecTsumo.cs:57`,`:91`.

### 8.3 Dora, ura-dora, wall, initial hand - separate fields or one list?

* Omote dora and ura dora are **separate parallel arrays** (`dora` / `ura_dora`,
  `AC/MFG.Taikyoku.Command.Receive/KyokuStart.cs:79-81`), with `dora_open` saying how many
  pairs are revealed; the client indexes them pairwise (`AC/ExecKyoku.cs:113-117`,
  uradora reveal at `AC/ExecAgari.cs:137`,`:364` → `AC/Wanpai.cs` `OpenUradora`). They are
  **not** entries inside the wall list from the client's point of view (`yama` is a separate,
  unused field). `UNCONFIRMED:` whether the server physically places `ura[j]` behind
  `omote[j]` in `yama` (the client can't tell; needs a captured `yama` + `dora` pair).
* The wall: `nokori` (count) is authoritative for the client's counters; `yama_cnt`/`yama`
  (order) and `rinshan` are parsed and ignored (`KyokuStart.cs:88-109`), so a mock may send
  them empty - subject to the `NeedNot` guard ordering (they come before `player_info{i}`,
  so omitting them **skips player parsing** because the `NeedNot` is caught only at
  `KyokuStart.cs:69-71` / `:122-124`; send empty strings or the elements themselves).
* Initial tiles: 13 per seat in `player_info{i}/tepai` (§7.1); the 14th tile of any draw is
  always a `TSUMO.pai` (`AC/ExecTsumo.cs:57`,`:91`).
* Melds are conveyed only as **events** (`PON/CHI/MINKAN/KAKAN` cells, §3.3); there is no
  per-kyoku furo list outside reconnect (`FuroData`, §3.4). Discards likewise exist only as
  `SUTEHAI` events (plus `sute_pai[]`/`sute_pai_state[]` in the reconnect shape). A mock that
  wants replay must keep the log - that is exactly what the client does
  (`AC/MFG.Taikyoku.Command/CommandClient.cs:286-292`, kifu replay mode).

### 8.4 3-player / 2-player variants

Rule/mode flags live in `AC/MFG.GameData/MahjongGameData.cs` (TakuType/EventType/flags,
`:273-275` for mode selection); tile *availability* filtering is
`AC/MFG.Types/HaiIDExtensions.cs:155-183` (nima drops manzu + west + Chun, sanma drops
honors or 2-8 manzu). The wire carries plain `Pai` ids; the client's seat count comes from
`GameDataManager.Mahjong.TakuType` (set from `gmode`), not from the command stream - so the
server must keep `gmode` and the dealt tiles consistent (`AC/MahjongUtility.GetPlayerNum`
loops at `AC/ExecKyoku.cs:95`, `AC/MahjongPlayFlow.cs:1976`).

---

## 9. Mahjong engine available for reuse

### 9.1 Where it lives

Namespace `MFG.MahjonPlay.Hand` (misspelled in the original source) in
`AC/MFG.MahjonPlay.Hand/`, plus root-level helpers. There is **no `MFG.Mahjong/` folder**;
`MFG.Types/` holds the value types and `AC/MahjongUtility.cs` the score tables.

| File | Role |
|---|---|
| `AC/MFG.MahjonPlay.Hand/Counter.cs` | `byte[34]` tile-count bag (`m_counts`, `:37`) — the engine's currency |
| `AC/MFG.MahjonPlay.Hand/BlockDivider.cs` | hand decomposition: `Divide(HaiID[] concealed, BlockSet melded, bool ignoreTsumo)` → `List<StructuredHand>` (`:52`), `DivideAsNormalHand(Counter, BlockSet)` (`:73`), async one-by-one-discard variant (`:19`) |
| `AC/MFG.MahjonPlay.Hand/StructuredHand.cs` | one decomposition: `ShantenCount()` (`:38`), `GetMachiTypes()` (`:125`), `IsTenpai()` (`:489`), `EvaluateScore(TaikyokuState, doraCount)` (`:397`) |
| `AC/MFG.MahjonPlay.Hand/BlockSet.cs` | concealed + melded blocks, `GetFu(agariHai, machiType, baKaze, jiKaze, isTsumo)` (`:503`) |
| `AC/MFG.MahjonPlay.Hand/Block.cs`, `BlockType.cs`, `DivideType.cs`, `MachiType.cs`, `PatternFlag.cs`, `SuID.cs`, `HaiMarkType.cs` | supporting value types (`PatternFlag` is the 61-bit pattern key) |
| `AC/YakuEvaluator.cs` | `Evaluate(BlockSet, MachiType[], TaikyokuState, int shanten, List<HaiID> machiHais)` → `YakuInfo` (`:95`) |
| `AC/KaratenEvauator.cs` (sic) | karaten helpers |
| `AC/MahjongUtility.cs` | han/fu → score tables: `GetHanNum` (`:1226`), `GetBaseScore` (`:1636`), `GetHanType` (`:1664`), `GetScore(takuType, han, fu, isOya, isTsumo)` (`:1697`), `CreateYakusFromYakuBit` (`:1594`) |
| `AC/HaiNavi.cs` | the client's full "waits/han/score" advisor: `Analyze()`, `MachiHais()` (`:274`), shanten/han panels; drives the UI, useful as a reference for what the server must answer |
| `AC/TaikyokuState.cs` | rules/state view the evaluator consumes (bakaze from `ba`, dora lists, furiten, riichi flags, open/closed) |
| `AC/MFG.Types/ShantenInfo.cs`, `ScoreInfo.cs`, `Yaku.cs`, `HaiUtility.cs` | result value types |

A mock/replacement server can therefore reuse, unchanged (or lifted into a .NET build):
decomposition, shanten, wait identification, yaku recognition, fu, han and payment tables -
i.e. everything needed to answer `TSUMOCHOICES.select`/`ptn*`, `SUTECHOICES.select`/
`chi_pai`/`pon_pai`/`kan_pai` and to fill `yaku{i}`/`calc_score{i}`.

### 9.2 Coupling and what must be parameterized

| Coupling | Where | Fix for a server build |
|---|---|---|
| Global game state: `GameDataManager.Mahjong` (mode flags) read directly by the engine | e.g. `AC/MFG.Types/HaiIDExtensions.cs:161-181`, `AC/MFG.MahjonPlay.Hand/StructuredHand.cs:476`, `AC/MahjongUtility.cs` | inject a mode/rules object; do not ship `GameDataManager` |
| Unity-only surfaces: `[SerializeField]`/`MonoBehaviour` wrappers around the logic (`AC/Hand.cs`, `AC/Meld.cs`, `AC/Tile.cs`, `AC/Agari.cs`, `AC/HaiNavi.cs`, `AC/Wanpai.cs`, `AC/Dora.cs`, `AC/Uradora.cs`) | root-level files | take only the logic classes (`MFG.MahjonPlay.Hand/*`, `YakuEvaluator`, `MahjongUtility`, `TaikyokuState`); the `Exec*`/UI layer is client-only |
| `UniTask`/`UniRx`/DOTween/`UnityEngine.Debug` inside some paths | `AC/MFG.MahjonPlay.Hand/BlockDivider.cs:19`, `AC/HaiNavi.cs`, `AC/Exec*.cs` | keep `Debug.Log` shims; drop the async variant of `Divide` |
| Dora counts / furiten / remaining tiles passed as ambient state | `AC/TaikyokuState.cs`, `RestPaiNums` | supply per-seat remaining-tile counts (derivable from the log the server already keeps) |

### 9.3 What the client does **not** do (server-side duties you cannot delegate)

* No `IsAgari()`-style win tester: a win is decided by "drawn/claimed tile ∈ `HaiNavi.MachiHais()`"
  client-side (`AC/HaiNavi.cs:274`, `AC/ExecTsumoChoices.cs:257`) - the server must decide
  legality and scores.
* No tenhou/九種九牌/流局条件 detection client-side: yakuman *names* arrive as
  `yaku1`/`yaku2` bits (`AC/MFG.Taikyoku.Command.Receive/YakuData.cs:40-42`, decoded at
  `AC/MahjongUtility.cs:1594`) and exhaustive-draw results arrive fully computed (§3.3).
* The client never validates the server: it displays whatever `calc_score{i}` /
  `player_info{i}/score` says (`AC/ExecKyoku.cs:253-258`), and recomputes money only when
  `wherefore == 1` (`AC/Agari.cs:663`).
* The client sends no chi/pon "is this legal" check - legality of the *call* is implied by
  the server having offered the window; `tepai_pos`/`tepai_id` are self-reported (§4.3).

---

## 10. Minimum viable session (mock cheat-sheet)

1. `login` → `session_id`; `get_menudata` → `menudata/mpdata/mid`.
2. `entry_game` → `<entry>` with all 17 children (§6.3); pick `tid`, `pindex`, `gserv_id`,
   `next_sno`.
3. Client: `gget` (`ready=0`) … then `EntryOK` as a `gget`; keep answering `gget` with
   `mwait/status=0` until you seat everyone, then `status≠0` + `mend` roster
   (`AC/MFG.Taikyoku/MatchingData.cs:31-32`).
4. Client enters play and sends `gget ready=1`; answer `all_ready=1` (or omit `all_ready`).
5. Append cells: `KYOKUSTART` → `TSUMO`(dealer) → `TSUMOCHOICES`(pi=dealer) → read the
   `gpost` → `SUTEHAI` → `SUTECHOICES`(pi=claimers) or next `TSUMO` → … → settlement →
   `KYOKUEND(end_stat=0)` → wait four `NEXT_KYOKU_READY` gposts → next `KYOKUSTART`.
6. Every `gget`/`gpost` response carries the log slice `cell_sno start/count` +
   `cell_data_{seq}` with `kind` and `pi0..pi3` (§1.5, §1.7); advance `start+count`
   monotonically and never go backwards.
7. On the last kyoku: `KYOKUEND` with `end_stat=1`, then `end_game` → `mgresult` with
   `player_{0..3}` rank/score/uma.

---

## 11. Open questions (and the artifact that settles each)

1. Does the server emit a `SUTECHOICES` (with `select=NONE`) on **every** discard, or only
   when someone can call? Client tolerates both (§3.2, §7.3 5a/5b). - *one captured discard
   followed by an immediate next `TSUMO`.*
2. Is `TSUMO` really broadcast with the drawn tile id for **every** seat (i.e. the server
   reveals each draw to all clients)? The client applies it unconditionally
   (`AC/ExecTsumo.cs:52-91`), so "yes" is the only consistent reading, but the capture does
   not contain a `gget` body dump. - *any captured `gget` response with `cell_data_* kind="1"`.*
3. ~~`KYOKUEND.end_stat`~~ **Settled 2026-10-24 by two live S3 matches**: `1` = match over -
   the client sends `end_game` within seconds of a `KYOKUEND` with `end_stat=1` and goes to the
   ranking screen; `0` = kyoku over, match continues. The mock must send `0` mid-match and `1`
   only on the final kyoku (already its behavior; verified live end to end).
4. `SCORERANK(24)` trigger and cadence (§3.3). - *captured `kind="24"` cell.*
5. `kan_type` numbering in `kan_pai`/`kan_type`: the client's own kan chooser only acts on
   `1` → `Ankan` and `3` → `Kakan` and silently drops any other value
   (`AC/ExecTsumoChoices.cs:952-968`); what `2` (or others) would mean, and whether
   `SUTECHOICES`' `kan_pai` uses the same numbering, is unverified. - *captured
   `SUTECHOICES` with `kan_pai`.*
6. Semantics of the unused inner fields: `SUTECHOICES/pindex`, `naki` flag word (§3.3),
   `KYOKUSTART/sai`, `yama`, `rinshan`, `rencyan`, `oya`, `chicya`. Server-side they
   presumably drive CPU behavior; the client ignores them. - *server binary, or captured
   cells compared with observed play.*
7. `chi_pai` ordering convention per `chi_naki` position (§3.3 CHI). - *captured CHI cells
   for left/center/right calls.*
8. Whether the dealer's `player_info{dealer}/tepai` carries 13 or 14 ids (§7.1). -
   *captured `KYOKUSTART`.*
9. `available="0"` semantics (§1.5, §5.2) - what the live server uses it for (maintenance?
   log compaction?). - *captured `gget` with `available="0"`.*
10. Reconnect element naming inside `recon` (`player{i}` vs `player_info{i}`, §6.6) and the
    trigger request (`kind=14` gpost vs the unused `/reconnect/` endpoint). - *a captured
    mid-match reconnect.*
11. Whether the server validates the `must=` chain / `tepai_pos` at all, and what
    `serv_st/code` values it returns on rejection (§4.3). - *a dev-session send with a
    deliberately wrong seq or `tepai_pos`, plus the server binary.*
12. `gchat` during play: `GChat` hard-codes `scene=MATCHING` and is only called from
    `MatchingFlow` (`AC/MFG.GameRequest/GChat.cs:22`,`:26`; `AC/MatchingFlow.cs:500`,`:516`),
    yet stamps are seen mid-match in-game - the mid-match send path was not found in
    recovered code. - *captured `g_chat` request during `MahjongPlay`, or the missing caller.*
13. `RANNYU(19)` - no client parser (§2.1); what does it carry and when does the server emit
    it? - *captured `kind="19"` cell.*
14. Units/meaning of the `entry_game` limit fields (§6.3). - *server binary, or timing a live
    kyoku against `ste_limit_time`.*
15. Rank fields are 0-based `Rank` enum values (`KyokuStart/player_info{N}/rank` and
    `Mgresult/player_{N}/rank`; `ScorePanel.isWin = myRank == 0`, `m_ranks[rank]` has 4 entries,
    `ResultFlowLikability` prints `Rank + 1`位). A 1-based rank 4 crashed `ScorePanel.PlayFinal`
    live and `ExecKyoku.ProcessCommand` threw `IndexOutOfRange` on dealer `rank=4`.
16. Renchan dealer handling: real 4-player rules keep the dealer on an exhaustive-draw renchan;
    the current mock rotates seats on any tenpai draw and increments `kyoku` as if it advanced.
    Client renders whatever `oya`/`jikaze`/`kyoku` the server sends, so this is a rule-fidelity
    gap, not a protocol failure. - *captured renchan `KYOKUSTART` after a dealer tenpai draw.*
