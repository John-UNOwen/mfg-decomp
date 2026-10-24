# 10 - API inventory (Phase 1 exit)

Phase 1 asked for a closed list of everything the rebuilt game must reimplement, stub, or delete. That list exists. The authoritative tables are:

- [platform-callsites.md](inventory/platform-callsites.md): 603 exported wrapper members inventoried one by one, classified, each row cited to game-code call sites.
- [flow-trace.md](inventory/flow-trace.md): boot -> ea3 service -> card login -> lobby -> match traced in recovered C#, with a closed RPC enumeration and a gameplay-authority verdict.

## Classification totals (from the census, mechanically summed)

| Bucket | Members | Meaning for the rebuild |
|---|---|---|
| REIMPLEMENT | 227 | needed for online play; 121 of them are pure-managed code that ships unchanged, ~106 are native-backed |
| LOCAL-STUB | 171 | cabinet hardware/telemetry that just answer "ready/zero/constant" (107 are firstpass `Konami.Aio`/`Konami.Acc`/`TCSIO` P/Invoke externs) |
| DELETE | 22 | execexe bridge, edrm RomCheck, Dx11/cloud game plumbing, binding attributes |
| UNUSED | 183 | exported but never called, including the whole `AvsProperty*` API stack and all of `Shirahama*` |
| **Total** | **603** | |

The actual reimplement work concentrates in 6 clusters of about 60 distinct native-backed functions (~85-100 function points with status/error semantics):

| Cluster | Core APIs | Size |
|---|---|---|
| boot + gates | `Kamunity.Startup`, `isActive`, `isEa3Boot` (replace GetProcAddress binding with managed implementations) | ~3 |
| ea3 status/env/facility/identity | `Ea3GetStatus`, `Ea3GetBootStatus`, `Ea3GetPpStatus`, `StdGetEnv` + identity getters, `Ea3QueryFacility*`, `Ea3PosevReport*`, `Ea3PcbtrackerReport`, clock | ~20 |
| XRPC transport | exactly 6 delegates (`Ea3XrpcApply/IsFinish/GetStatus/GetResponseSize/GetResponseData/Destroy`) + `Ea3XrpcAddModule`; every RPC funnels through them | ~7, hardest |
| eacoin wallet (PASELI) | ~30 called functions: checkin/checkout/consume/query family + pricelist cursor + reauth | ~35 |
| card/PIN/QR | `Cardnumber.ConvertCardNumberToUserCode` (algorithm), `cardmng` verbs (4 RPCs, already managed), `QRLogin` state machine | ~10 |
| local persistence | nvram XML via `Avs.Fs*` (6 FS functions), `dev/raw` backup store | ~12 |

## The two server surfaces (the finding that reshapes Phase 2)

Verified in code and logged here so no later phase re-derives it:

1. **e-Amusement / eAC3 surface** (native kamunity/avs2): boot FSM, status, license report, cardmng XRPC, vfgac/vfglog XRPC, PASELI/eacoin, PCB POS events. Closed XRPC list: 15 declared module.method names, 11 with real call sites (`vfgac.service_list/update_refer/ext_campaign/send_paylog`, `vfglog.put_msg/error_log`, `cardmng.inquire/authpass/bindmodel/getrefid`, 4 wrapper-only uploader variants).
2. **Front-server HTTP surface** (`SystemConfigGameData.FrontServerUrl` + `MFG.GameRequest/`): 35 XML-over-POST endpoints including `/login`, `/create_player`, `/client_state_read|write`, `/entry_game`, `/gget`, `/gpost`, `/gchat`, `/end_game`, all gacha endpoints. **All gameplay** (userdata, matchmaking, the entire mahjong command log) rides this surface, not XRPC. In arcade mode the URL itself is server-supplied by `vfgac.service_list`; in `-KONAMINET` mode the game falls back to hardcoded Konami AOG URLs.

## Gameplay authority verdict

MahjongPlay is a deterministic renderer of a server-produced command log. The client never adjudicates legality (server sends `TSUMOCHOICES`/`SUTECHOICES` flags) and never computes scores (server sends `cmd.Scores`), but it contains a complete mahjong engine (shanten/yaku/fu/han/points) used only for the HaiNavi hint panel. The title demo already replays full matches through the same dispatcher with zero network. Port consequence: matches run wherever a command-log producer runs; the client-side engine is the natural seed for one. See [flow-trace.md section 7](inventory/flow-trace.md).

## Coverage proof (why this is "100%")

- Managed assemblies: all 129 listed; after excluding framework + known third-party, no unexamined custom assembly remains (census section 5).
- Platform entry points: the kamunity binding surface is enumerable by pattern (196 delegate-typed properties), the census walks every wrapper file; all 129 `DllImport` declarations in game code are tabulated by target DLL.
- RPC surface: `CreateRequestNode`'s first argument is the module name, so one grep closes the set; negatives (`message.get`, `pcbevent` strings, `/ea3/` and `/usr/` property literals) are verified absences, not oversights.

Open items live in [flow-trace.md section 8](inventory/flow-trace.md) and feed Phase 2; the design consequences are in [11-port-boundary.md](11-port-boundary.md).
