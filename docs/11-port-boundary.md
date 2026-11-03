# 11 - Port boundary and rebuild design

Design draft after Phase 1. Everything marked OBSERVED is cited in [inventory/platform-callsites.md](inventory/platform-callsites.md) or [inventory/flow-trace.md](inventory/flow-trace.md); INFERRED items are labeled.

## The boundary is the wrapper ABI, and it is already C#

`KamunityInstance.Boot()` binds every `[KamunityDllApiClass]` delegate property by name from `GetProcAddress` (OBSERVED, `api/recovered/kamunity/KAMUNITY/KamunityInstance.cs:84-111`). The rebuild keeps the wrapper classes exactly as shipped source and replaces only the bootstrap: bind the same delegate properties to managed implementations instead of a native DLL. Game code then runs unmodified across the seam.

`IEaPlatform` below is the shape of that managed implementation layer (one method per native-backed behavior, summarized from the six clusters in docs/10; final signatures come from the wrapper delegate types themselves):

```csharp
interface IEaPlatform {
  // status + identity (backed today by ea3 boot FSM + env store)
  Ea3Status GetStatus(); Ea3BootStatus GetBootStatus(out BootCode, out int);
  PpStatus GetPpStatus();  string StdGetEnv(string path);
  void QueryFacility(string path, StringBuilder out);  ClockTime TimeGet(ClockKind);
  // XRPC: the whole RPC corpus behind 6 functions (OBSERVED XrpcBase.cs:65-120)
  XrpcHandle XrpcApply(string method, byte[] requestXml);
  bool XrpcIsFinish(XrpcHandle); XrpcStatus XrpcGetStatus(XrpcHandle);
  byte[] XrpcGetResponseData(XrpcHandle); void XrpcDestroy(XrpcHandle);
  // eacoin wallet (~30 exports; state machines stay in the shipped wrapper code,
  // this layer provides the per-call transport + status answers)
  EacoinRet EacoinCheckin(string slot, string cardType, string cardId, string pass);
  EacoinRet EacoinConsume(string articlePath, int price); /* ... query family ... */
  // POS/PCB event reporting
  void PosevReport(string eventPath, int num);
  // local store
  AvsFsResult FsOpenW/FsOpenR/FsFsize/FsFileIsExist(...);  // nvram XML + markers
  // card identity
  bool ConvertCardNumberToUserCode(CardType, byte[] id, StringBuilder, int);
}
```

## Two deployment options for the server side

Phase 1 found the game already has a second, non-arcade server path. That changes the plan from "reimplement eAC3 or die" to a fork with a cheap first light.

### Option S: KonamiNet mode + front-server shim (first light, recommended)

Run the game with its own `-KONAMINET` switch (OBSERVED: parsed at `ResidentFlow.cs:961`; 92 gate sites). In this mode the game skips kamunity entirely, auto-passes the network boot check, drops PASELI/PCB/PP reporting to no-ops, uses local clock, and talks only to `FrontServerUrl` over plain HTTP (OBSERVED, flow-trace section 9). Konami itself uses this as a dev/QA path (INFERRED from the staging AOG URL and `IsEamDevConnection` default).

Work then moves to a shim server (panel backend, any language):

- Implement the 35 front-server endpoints. Request forms are fully derivable from client code (`MFG.GameRequest/*` build the exact POST fields); response XML shapes are enumerable from the parser code in the same classes (INFERRED: parsers show required nodes; server-only validation beyond client checks is the residual risk).
- Run tables behind `/entry_game`, `/gget`, `/gpost`, `/gchat`, `/end_game`: accept the client's bare intents and emit the exact server command vocabulary (KYOKUSTART with te/yama/dora, TSUMOCHOICES/SUTECHOICES flags, per-seat Scores, SCORERANK, KYOKUEND). The client's own engine (`StructuredHand`, `BlockSet`, `YakuEvaluator`, `MahjongUtility`) ports directly into the shim as the rules/score core (INFERRED: it is complete enough for evaluation; legality flags and wall order are the parts only the server defines).
- Identity: front server accepts a DataID with `bypass_auth=true` (OBSERVED, `Login.cs:20`); accounts get provisioned/DataID-mapped by the panel (a "card number" can just be a DataID alias; no reader, no PIN RPCs needed in this mode).
- The game-side re-platforming that Option S still requires: `System.Net.WebRequest` transport must move to UnityWebRequest for WebGL (`RequestCoroutine`/`HttpWebRequestor`/`HttpDownloader`, OBSERVED no WebRequest on WebGL), `dev/raw` backup path to browser persistent storage (IndexedDB), strip streaming/Dx11/execexe classes.

What Option S gives up in v1: real card/PIN authentication, PASELI billing, and the provider's card data. Those stay available through the streaming MVP meanwhile.

### Option M: arcade mode against the eAC3 server (deferred, capture-gated)

Keep kamunity ON and implement the ~60 native-backed functions over a managed `eac3-cs` client (XRPC wire format per Phase 2 original scope, eacoin/POS semantics). Open evidence question first: where does `vfgac.service_list` point `FrontServerUrl` on the provider, and does that host serve the 35 endpoints? Unknown (the 1.5 KB game log on disk is inconclusive). A one-night capture session (spice2x `-netdump` + a packet trace of the HTTP side) answers it. If the provider has no front server, the fallback is a hybrid: ea3/XRPC for card+POS, our shim for `FrontServerUrl` (client override point exists: `CardEntryScene.cs:1070-1088`).

### Recommended order

S to playable first (it needs no proprietary protocol at all), M later for card accounts and billing parity once the capture says what the server actually serves. Both share everything downstream: same wrapper ABI, same WebGL rebuild, same content pipeline.

## Mechanical strip list for the rebuild

From the census DELETE bucket + mode analysis (census section 6): delete `EXECEXE.cs` (and the prewarm call site `ResidentFlow.cs:528`), `ExtDrm`, `DistributeLoader`, `PluginAccess`/`SampleDriver`/`PluginDriver`/`CloudStreamingInputDevice`/`ScopeLib`/`Cloudstrap`/`ScopeManager`/`ScopeCloseScene`, `Win32Utility`/`MouseInputWrapper`, `AvsProperty*` (UNUSED), binding attributes. Keep compiled but inert: `Shirahama*`, `Tabooword*` (asset-reference risk, census R5), `VirtualSurround*` (serialized in assets, ships unchanged), `Win10actLog` (2-line stub). `Konami.Aio`/`Konami.Acc`/`TCSIO`: replace with no-op stubs, or run under the existing `-NOIO` switch.

## Revised gate sequence

1. **Gate 1 (days): mock first light.** Hosts-file pin `aog03-stg.pvt.mo.konami.net` (and the reconnect template's URL parts, `Reconnect.cs:18`) to a local mock; boot the *current install* with `-KONAMINET` against canned XML for `/appli_info`, `/login`, `/get_menudata`, `/client_state_read`; reaching the lobby proves the seam and the mockability claim before any WebGL or reconstruction work.
2. **Gate 1b (one night, parallel): Option M capture.** Full provider session with `-netdump` plus HTTP-side packet trace; answers the ServiceUrl/front-server question and seeds Phase 2 wire work if we keep M.
3. **Gate 2 (unchanged intent, new cheapest path): desktop editor build reaches lobby** through the shim (S) or real ea3 (M). Kill-switch still lives here.
4. Gates 3/4 (project reconstruction, WebGL) unchanged from [04-decompilation-plan.md](04-decompilation-plan.md).

## Decision log additions

- 2026-10-24: Phase 1 complete; port boundary fixed at the kamunity wrapper ABI (docs/10).
- 2026-10-24: Option S (`-KONAMINET` + front-server shim) proposed as the first-light path; Option M capture session scheduled before committing to eAC3 reimplementation. Owner sign-off pending.
