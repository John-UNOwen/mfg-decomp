# Platform callsite inventory (Phase 1 port boundary)

Generated 2026-10 census over `api/recovered/` (ilspycmd output, dump `2026060900`).
Read-only reverse engineering; no files besides this one were written.

## Method and counting rules (read first)

- Call counts: occurrences of the exact API name (word-bounded regex, case-sensitive) in the recovered sources of `api/recovered/Assembly-CSharp` and `api/recovered/Assembly-CSharp-firstpass` (1388 files, 232225 lines loaded once). A count is name occurrences in game code, so a line can be a call, a field declaration, or a gate condition. Representative sites given are file:line from that scan (repo-relative).
- Counting rule: every exported public member counts once: static methods, delegate-typed static properties (the kamunity native-binding surface), instance methods, and exported types (struct/enum/class rows are grouped and labeled as type groups). The `N` column is the authoritative member count; bucket totals are sums of `N` by classification.
- OBSERVED = directly verified in recovered source lines. INFERRED = judgment about port semantics or behavior the IL cannot fully prove; flagged inline.
- Wrapper mechanics (OBSERVED, `api/recovered/kamunity/KAMUNITY/KamunityInstance.cs:84-111`): `KamunityInstance.Boot()` resolves the module handle from the `-hp <hex>` command line argument (the loader passes its own `kamunity.dll` handle) or falls back to `LoadLibraryA("kamunity.dll")`, then for every class marked `[KamunityDllApiClass]` it binds every static delegate-typed property by `GetProcAddress` + `Marshal.GetDelegateForFunctionPointer` (`api/recovered/kamunity/KAMUNITY/MarshalUtil.cs:8-25`). So the game-visible "functions" are delegate properties auto-filled from the native DLL; wrapper methods add marshaling. If the module handle is absent (`-hp` missing, dll load fails) `isActive` is false and most platform calls are simply never made by the game (see gate note below).
- Master gate (OBSERVED): `GameUtility.IsKamunityBootEnable() => !IsKonamiNetConnection` (`api/recovered/Assembly-CSharp/GameUtility.cs:356-357`), and `IsKonamiNetConnection` is set by the `-KONAMINET` switch at boot (`api/recovered/Assembly-CSharp/ResidentFlow.cs:961`). Cloud streaming is selected by `-appBehavior KNST|NETCAFE` (`api/recovered/Assembly-CSharp/ResidentFlow.cs:963-969`, `api/recovered/Assembly-CSharp/SystemConfigGameData.cs:50-60`). The game already bypasses most arcade plumbing in "non-Konami network" mode, which is exactly the seam the port reuses.
- Classification buckets (per `docs/04-decompilation-plan.md` Phase 1): REIMPLEMENT = needed for online play (ea3 boot/property store/XRPC/card/PIN/userdata/POS consume/eacoin/message loops/timers); LOCAL-STUB = cabinet hardware / telemetry / board checks that only need inert answers (coin mech, lamps, actlog, shield/dongle, SNTP/keepalive, test mode, cloud gaming hooks); DELETE = Windows/native plumbing the WebGL rebuild lacks and does not need (execexe bridge, edrm/kdm external-drm file layer, Dx11 graphics plugin hooks, VirtualSurround native, mscoree, update flow); UNUSED = exported, never called by game code, no stub needed.
- "Pure managed" note: many REIMPLEMENT rows are pure C# classes that ship unchanged (XrpcBase, XrpcUtil, Logger, etc.). For them the work is implementing the *native-backed* APIs they call, listed in the same table.

## Gate map (who guards what)

- REIMPLEMENT gameplay calls run only when kamunity is live: login/card/POS paths are guarded by `GameUtility.IsKamunityBootEnable()` (e.g. `api/recovered/Assembly-CSharp/ConsumeWindow.cs:336`, `api/recovered/Assembly-CSharp/CardEntryScene.cs:1070,1093`, `api/recovered/Assembly-CSharp/BootSceneFlow.cs:211,347,658`) or by `KamunityInstance.isActive` / `isEa3Boot` (e.g. `api/recovered/Assembly-CSharp/ResidentFlow.cs:231,555,745`, `api/recovered/Assembly-CSharp/IkeyDevice.cs:65`).
- Test-mode cabinet screens (`api/recovered/Assembly-CSharp/TestModeSkeleton/*`) are behind `TestModeMain` (`api/recovered/Assembly-CSharp/TestModeSkeleton/TestModeMain.cs:162`) and `KamunityInstance.isActive` (`api/recovered/Assembly-CSharp/TestModeSkeleton/RomCheck.cs:84,110,125,150,215`).
- Cloud/streaming surface is behind `GameUtility.IsCloudStreaming` (26 files, 62 `Cloudstrap.` occurrences, e.g. `api/recovered/Assembly-CSharp/MahjongPlayFlow.cs:1382`, `api/recovered/Assembly-CSharp/GachaPlayFlow.cs:111`, `api/recovered/Assembly-CSharp/BaseFlow.cs:72`).

# 1. WrapperAPIInventory (grouped by wrapper file)

Columns: API member or group | N members | C game-code call occurrences | classification | evidence and notes.

## kamunity/KAMUNITY/Kamunity.cs

| API | N | C | Class | Evidence / notes |
|---|---|---|---|---|
| `Kamunity.Startup()` | 1 | 1 | REIMPLEMENT | `api/recovered/Assembly-CSharp/ResidentFlow.cs:230` in `InitializeOnBoot`. Pure managed; internally adds `XrpcService` + `HttpDownloadService` components and calls `XrpcModuleCardmng.AddModule()` when `isEa3Boot` (`api/recovered/kamunity/KAMUNITY/Kamunity.cs:9-16`). |

## kamunity/KAMUNITY/KamunityInstance.cs

| API | N | C | Class | Evidence / notes |
|---|---|---|---|---|
| `Startup()` (internal entry used by Kamunity.Startup) | 1 | 0 | REIMPLEMENT | called from `Kamunity.Startup`; spawns DontDestroyOnLoad GameObject + `Boot()` (`api/recovered/kamunity/KAMUNITY/KamunityInstance.cs:66-77`). WebGL side must replace the `-hp`/LoadLibrary logic with managed delegate implementations. |
| `isActive` | 1 | 8 | REIMPLEMENT | `api/recovered/Assembly-CSharp/IkeyDevice.cs:65`, `api/recovered/Assembly-CSharp/ResidentFlow.cs:555,745`, `api/recovered/Assembly-CSharp/TestModeSkeleton/RomCheck.cs:84` +3 more. Gate for cabinet-side init. |
| `isEa3Boot` | 1 | 3 | REIMPLEMENT | `api/recovered/Assembly-CSharp/ResidentFlow.cs:231`, `api/recovered/Assembly-CSharp/ErrorLogger.cs:35`, `api/recovered/Assembly-CSharp/ErrorModeMenu.cs:94`. Gate for online identity/pcbevent paths. INFERRED: wrapper returns isActive verbatim (`KamunityInstance.cs:44-54`), so "ea3 boot" == "module handle found". |
| `isLocalBoot` | 1 | 0 | UNUSED | never read in game code (OBSERVED, 0 name occurrences). |
| `shutdownCallback` | 1 | 0 | UNUSED | wrapper fires it on quit (`KamunityInstance.cs:113-134`); no game setter found (OBSERVED, 0). |
| `hDLL` | 1 | 0 | UNUSED | 0 game occurrences. |
| `GetProcDelegate<T>(string)` + `GetProcDelegate(string, Type)` | 2 | 0 | DELETE | dynamic export binding machinery replaced by managed implementations in the rebuild (OBSERVED in wrapper only). |

## kamunity/KAMUNITY/Bootstrap.cs (ikey dongle + media file check)

| API | N | C | Class | Evidence / notes |
|---|---|---|---|---|
| `GetIkeyStatus(out IKeyStatus)` | 1 | 2 | LOCAL-STUB | `api/recovered/Assembly-CSharp/IkeyDevice.cs:20,50` (security key presence/disable checks that HALT the game if missing). Stub: return one enabled account+license key. |
| `FcheckInit` / `FcheckMain` / `FcheckFinish` | 3 | 3 | LOCAL-STUB | test-mode ROM check only: `api/recovered/Assembly-CSharp/TestModeSkeleton/RomCheck.cs:93,103,220`. |
| `BootstrapFcheckInit` (obsolete alias) | 1 | 0 | UNUSED | 0 game occurrences. |
| Types `IKeyStatus`, `IKeyStatusItem`, `BT_FCHECK_MODE`, `BT_FCHECK`, `bt_fcheck_stat` | 5 | 8 | LOCAL-STUB (support types) | `api/recovered/Assembly-CSharp/IkeyDevice.cs:6`, `api/recovered/Assembly-CSharp/TestModeSkeleton/QCMode.cs:364,369`, `RomCheck.cs:102`. |

## kamunity/KAMUNITY/Avs.cs (env store, avs file system, clock, log)

Wrapper methods:

| API | N | C | Class | Evidence / notes |
|---|---|---|---|---|
| `StdGetEnv(path)` | 1 | 3 | REIMPLEMENT | `api/recovered/Assembly-CSharp/TitleDemoUtility.cs:145,147` (cloud-pass card args), `api/recovered/Assembly-CSharp/TestModeSkeleton/SystemInformation.cs:81`. Backs every identity getter below. |
| `GetSoftIdCode()` | 1 | 5 | REIMPLEMENT | `api/recovered/Assembly-CSharp/SystemUtility.cs:107`, `api/recovered/Assembly-CSharp/TCSNET/TcsXrpcModule.cs:236`, `api/recovered/Assembly-CSharp/TestModeSkeleton/CabinetUtility.cs:33`. Reads `/env/profile/soft_id_code` (`api/recovered/kamunity/KAMUNITY/Avs.cs:134-137`). |
| `GetSystemId()` | 1 | 2 | REIMPLEMENT | `api/recovered/Assembly-CSharp/SystemUtility.cs:108` feeds front-server env params (`RequestApiBase.cs:47`). |
| `GetHardwareId()` | 1 | 2 | REIMPLEMENT | `api/recovered/Assembly-CSharp/SystemUtility.cs:109`. PCB id for logs/telemetry. |
| `GetLicenseId()` / `GetAccountId()` / `GetAvsVersion()` | 3 | 3 | LOCAL-STUB | test-mode only: `api/recovered/Assembly-CSharp/TestModeSkeleton/SystemInformation.cs:82,85,86`. |
| `GetSecurityCode()` | 1 | 0 | UNUSED | 0 game occurrences (wrapper defines it, `Avs.cs:154-157`). |
| `GetBootstrapVersion()` | 1 | 0 | UNUSED | 0 direct occurrences (SystemInformation uses `StdGetEnv("/env/boot/version")` instead). |
| `FsOpenW(path)` | 1 | 3 | REIMPLEMENT | nvram XML persistence: `api/recovered/Assembly-CSharp/TestModeSkeleton/CabinetSerializer.cs:26`, `CoinSettings.cs:189` (`/dev/nvram/coin.xml`), `VirtualCoinSettings.cs:190` (`/dev/nvram/eacoin.xml`). |
| `FsOpenR(path)` | 1 | 1 | REIMPLEMENT | `api/recovered/Assembly-CSharp/TestModeSkeleton/CabinetSerializer.cs:46` (cabinet settings load/save used at gameplay boot, `CabinetSettings.InitLoadSettings`, `SystemConfiguration.cs:63`). |
| `IsFsOpenSuccess(ret)` | 1 | 4 | REIMPLEMENT | `api/recovered/Assembly-CSharp/TestModeSkeleton/CabinetSerializer.cs:27,47`, `CoinSettings.cs:190` +1. Pure helper. |
| `FsFsize(fd)` | 1 | 1 | REIMPLEMENT | `api/recovered/Assembly-CSharp/TestModeSkeleton/CabinetSerializer.cs:48`. |
| `FsFileIsExist(path)` | 1 | 1 | REIMPLEMENT | factory-boot marker `api/recovered/Assembly-CSharp/SystemUtility.cs:149` (`/dev/drive/g/vfgfact00.txt`). |
| `FsPathIsExist` / `FsDirectoryIsExist` | 2 | 0 | UNUSED | 0 direct occurrences (internal use inside `Avs.cs:199-220`). |
| `TimeGet(AVS_CLOCK)` | 1 | 5 | REIMPLEMENT | server/monotonic clock: `api/recovered/Assembly-CSharp/ServiceButtonLog.cs:155`, `api/recovered/Assembly-CSharp/MFG.GameData/AccountGameData.cs:69` (SERVER clock stamps access dates). Part of the timer/message loop dependency. |
| `GetDateTime(AVS_CLOCK)` | 1 | 0 | UNUSED | 0 direct occurrences. |
| `AvsLogMisc` / `AvsLogInfo` / `AvsLogWarning` | 3 | 0 | UNUSED (as direct APIs) | 0 game occurrences; the `AvsLog` delegate is consumed indirectly via `Logger` (see Logger section). |

Delegate-typed properties (the native-import surface, `Avs.cs:86-114`):

| API group | N | C | Class | Evidence / notes |
|---|---|---|---|---|
| `StdGetEnv`, `AvsLog`, `FsOpen`, `FsWrite`, `FsRead`, `FsFstat`, `FsLstat`, `ClockGet` | 8 | n/a | REIMPLEMENT | backing for the REIMPLEMENT wrappers above; also reached via `Logger` (`Logger.cs:19-95`) and `AvsFileStream` path. |
| `FsClose`, `FsSeek`, `FsFlush` | 3 | 0 | UNUSED | only consumed by `AvsFileStream`, which the game never uses. |
| `FsRename`, `FsMakedir`, `FsRemove`, `FsCleanupDir`, `FsCopy`, `FsStatistics`, `FsDumpMountpoint`, `FsConvertPath`, `SntpGetAddr` | 9 | 0 | UNUSED (SntpGetAddr: LOCAL-STUB candidate) | 0 game occurrences. SNTP is a cabinet/board hook per plan bucket. (`FsLstat` moved to the REIMPLEMENT group: it backs the used `FsFileIsExist`.) |
| `AvsThreadCreate`, `AvsThreadDestroy`, `AvsThreadJoin`, `AvsThreadExit`, `AvsThreadAttrSetPriority` | 5 | 0 | UNUSED | only consumed by `AvsThread` (unused class). |

## kamunity/KAMUNITY/AvsBookkeeping.cs (cabinet bookkeeping blob)

| API group | N | C | Class | Evidence / notes |
|---|---|---|---|---|
| `BookkeepingGetIncomeLast7days/Last52weeks/EachDays/EachHour(bool)` wrappers + their 4 delegates | 8 | 4 | LOCAL-STUB | test-mode income charts only: `api/recovered/Assembly-CSharp/TestModeSkeleton/BookKeeping.cs:145,151,157,163`. |
| `BookkeepingAddVariable`, `BookkeepingSetInitState`, `BookkeepingFlush`, `BookkeepingClockIsSet`, `BookkeepingClockSetDiff`, `BookkeepingStatusSetFactory`, `BookkeepingDataSetFactory`, `BookkeepingSetRecordEnable`, `BookkeepingIsRecordEnable`, `BookkeepingGetSumVariable`, `BookkeepingGetVariable`, `BookkeepingSetVariable`, `BookkeepingGetTotalActiveTime`, `BookkeepingGetTotalPlayTime`, `BookkeepingGetShortestPlayTime`, `BookkeepingGetLongestPlayTime`, `BookkeepingGetAveragePlayTime`, `BookkeepingSetPlayTime`, 4 `*Stat` delegates (18 named members + 4 Stat delegates = 22) | 22 | 0 | UNUSED | 0 game occurrences each (OBSERVED via name scan); income `*Stat` delegates unreferenced. |

## kamunity/KAMUNITY/AvsEa3.cs (ea3 status, posev, pcbtracker, keepalive, facility, netctrl)

| API group | N | C | Class | Evidence / notes |
|---|---|---|---|---|
| `Ea3GetStatus` (delegate) | 1 | 9 | REIMPLEMENT | online gate polled everywhere: `api/recovered/Assembly-CSharp/EagCore.cs:90`, `api/recovered/Assembly-CSharp/Konami.System.Net/RequestCoroutine.cs:157` (blocks HTTP API when not ONLINE), `api/recovered/Assembly-CSharp/XrpcBase.cs:69` (blocks all XRPC when not ONLINE). |
| `Ea3GetBootStatus` (delegate) | 1 | 5 | REIMPLEMENT | boot spin + error map: `api/recovered/Assembly-CSharp/EagCore.cs:48,85`, `api/recovered/Assembly-CSharp/BootSceneFlow.cs:593`, `api/recovered/Assembly-CSharp/TitleFlow.cs:350`. `EA3_BOOT_CODE` appears 31 times in game code (`BootSceneFlow.cs:407-410`); the port must reproduce boot-code values (PCBTRACKER_CONFIRM etc.) or error screens diverge. |
| `Ea3GetPpStatus` (delegate) | 1 | 8 | REIMPLEMENT | participation status gate for consume UX: `api/recovered/Assembly-CSharp/ConsumeWindow.cs:353`, `api/recovered/Assembly-CSharp/EagCore.cs:102`, `api/recovered/Assembly-CSharp/TestModeSkeleton/IncomeLog.cs:153`. |
| `Ea3GetApsStatus` (delegate) | 1 | 0 | UNUSED | 0 game occurrences. |
| `Ea3PosevReport`, `Ea3PosevReportWithNum` (delegates) | 2 | 5 | REIMPLEMENT | pcbevent/PP event upload: `api/recovered/Assembly-CSharp/GameUtility.cs:802,830,839` (`VFG.cs`/`VFG.premium` game-event + PP send), test mode `TestModeMain.cs:167`, `CoinSettings.cs:203`. |
| `Ea3PcbtrackerReport` (delegate) + `Ea3PcbtrackerReportLicenseAgree()` | 2 | 2 | REIMPLEMENT | license agree report on first boot: `api/recovered/Assembly-CSharp/EagUtility.cs:19`, `api/recovered/Assembly-CSharp/MFG.GameData/NoticeInfo.cs:19`. Path `/license/agree` is hardcoded in wrapper (`api/recovered/kamunity/KAMUNITY/AvsEa3.cs` facility/license helpers). |
| `Ea3QueryServicesProductDomain` (delegate) | 1 | 2 | REIMPLEMENT | dev/server domain detection: `api/recovered/Assembly-CSharp/GameUtility.cs:367` (`IsEamDevConnection`), `api/recovered/Assembly-CSharp/TCSNET/TcsXrpcModule.cs:221`. Stub to the constant matching the target server. |
| `NetCtrl` (delegate) + `NetCtrlAddress()` | 2 | 3 | REIMPLEMENT | DHCP-failure detection on RESUME boot: `api/recovered/Assembly-CSharp/EagCore.cs:157`, `api/recovered/Assembly-CSharp/TestModeSkeleton/NetworkCheck.cs:290,383`. |
| `NetCtrlGatewayIp`, `NetCtrlSubnetMask`, `NetCtrlMAC`, `NetCtrlGatewayMac`, `NetCtrlInterfaceConfig`, `NetCtrlNetType` | 6 | 5 | LOCAL-STUB | network self-test/QC screens: `api/recovered/Assembly-CSharp/TestModeSkeleton/NetworkCheck.cs:384,385,386,314`, `EagUtility.cs:22` (spec string). Return synthetic values. |
| `KeepaliveFindByName`, `KeepalivePing`, `KeepaliveGetStatus` (delegates) + `KeepaliveFind(name)` | 4 | 18 | LOCAL-STUB | board link keepalive: `api/recovered/Assembly-CSharp/BootSceneFlow.cs:660-662` (ping router/center/server on boot exit), test-mode `NetworkCheck.cs:209,606`, `QCMode.cs:481`. |
| `Ea3TimeIsHoliday` (delegate) + wrapper method | 2 | 1 | LOCAL-STUB | test mode only: `api/recovered/Assembly-CSharp/TestModeSkeleton/ClockSettings.cs:166`. |
| `Ea3MessageSearch` (delegate) | 1 | 2 | REIMPLEMENT | maintenance message reads (`"sys.eacoin.mainte"` label): `api/recovered/Assembly-CSharp/MaintenanceWindow.cs:122`, `api/recovered/Assembly-CSharp/ShopCloseUtility.cs:151`. This is the only message-loop API the game touches directly; server "message.get" polling is done by the avs2 side (INFERRED from docs/01 boot trace, no C# caller found). |
| `Ea3QueryFacilityString(path,sb,size)` method + `Ea3QueryFacilityU8/U16/S32/In4addr` delegates | 5 | 0 (backing) | REIMPLEMENT | internal backing for the facility path wrappers below. |
| `QueryFacilityLocationId`, `QueryFacilityPathString`, `QueryFacilityPathU8`, `QueryFacilityPathU16`, `QueryFacilityPathS32`, `QueryFacilityPathIn4addr` | 6 | 12 | REIMPLEMENT | facility identity feeds server payloads: `api/recovered/Assembly-CSharp/SystemUtility.cs:133` (LocationID), `api/recovered/Assembly-CSharp/XrpcCabinet.cs:11,27` (error log + `location/id`), `api/recovered/Assembly-CSharp/TCSNET/TcsXrpcModule.cs:141`, `api/recovered/Assembly-CSharp/TestModeSkeleton/SystemInformation.cs:91,92`, `QCMode.cs:157`. Port answers these from the property store / configured constants. |
| Types `FACILITY_PATH_STRING/U8/U16/S32/IN4ADDR` (path-name tables) | 5 | 6 | REIMPLEMENT (support types) | path name switch tables in wrapper (`AvsEa3.cs:86-124`) map enums to `/facility/...` paths server-side; game passes enums only. |

## kamunity/KAMUNITY/AvsEa3Coin.cs (eacoin wallet / PASELI session machine)

All game sites are inside `IsKamunityBootEnable()` or `PaseliUtility` gates (INFERRED: this whole API is the billing core; bucket REIMPLEMENT unless never called).

| API group | N | C | Class | Evidence / notes |
|---|---|---|---|---|
| `Ea3EacoinCheckin` (internal delegate) | 1 | 1 | REIMPLEMENT | card+PIN wallet checkin: `api/recovered/Assembly-CSharp/CardEntryScene.cs:180,448` with `SLOT_PATH = "/opt/eacoin/slot1/player"`, `ECTYPE = "1"` (`api/recovered/Assembly-CSharp/EaCoin.cs:25-27`). |
| `Ea3EacoinCheckout` | 1 | 1 | REIMPLEMENT | `api/recovered/Assembly-CSharp/CardEntryScene.cs:600`. |
| `Ea3EacoinGetStatus` | 1 | 6 | REIMPLEMENT | wallet state polling: `api/recovered/Assembly-CSharp/PaseliUtility.cs:57,207,421` (+3). |
| `Ea3EacoinQuerySessionState`, `Ea3EacoinQuerySessionIsOpen` | 2 | 2 | REIMPLEMENT | `api/recovered/Assembly-CSharp/PaseliUtility.cs:146,150` returns `EA3_EACOIN_SESSION`. |
| `Ea3EacoinConsume(articlepath)`, `Ea3EacoinIconsume`, `Ea3EacoinConsumeWithPrice` + `Consume(GAME_MODE)` wrapper | 4 | 5 | REIMPLEMENT | POS consume by article: `api/recovered/Assembly-CSharp/PaseliConsumeButton.cs:316,320,334`, `api/recovered/Assembly-CSharp/TestModeSkeleton/QCMode.cs:1097`; articles from `api/recovered/Assembly-CSharp/EaCoin.cs:29-91` (`/eacoin/game_s_ec`, `/coin/game_s`, ...). |
| `Ea3EacoinQueryConsumeStatus` | 1 | 1 | REIMPLEMENT | `api/recovered/Assembly-CSharp/PaseliConsumeButton.cs:433` (consume result poll). |
| `Ea3EacoinQueryBalance` | 1 | 1 | REIMPLEMENT | `api/recovered/Assembly-CSharp/PaseliConsumeButton.cs:380`. |
| `Ea3EacoinQueryCheckinStatus` | 1 | 1 | REIMPLEMENT | `api/recovered/Assembly-CSharp/CardEntryScene.cs:266,458`. |
| `Ea3EacoinQueryPrice` | 1 | 2 | REIMPLEMENT | `api/recovered/Assembly-CSharp/GameUtility.cs:731` (`GetPaseliPrice`), `api/recovered/Assembly-CSharp/TestModeSkeleton/PriceUtility.cs:100`. |
| `Ea3EacoinSupplyExtra` + `Ea3EacoinQueryExtra` | 2 | 3 | REIMPLEMENT | free-credit supply UX: `api/recovered/Assembly-CSharp/PaseliConsumeButton.cs:600,613`, `TestModeSkeleton/VirtualCoinSettings.cs:207`. |
| `Ea3EacoinQueryNotchAmount`, `Ea3EacoinQueryNotchCount` | 2 | 2 | REIMPLEMENT | `api/recovered/Assembly-CSharp/PaseliUtility.cs:409` (notch = service credit threshold). |
| `Ea3EacoinQuerySupplyLimit` | 1 | 1 | REIMPLEMENT | `api/recovered/Assembly-CSharp/PaseliUtility.cs:372`. |
| `Ea3EacoinQueryTotalConsume` | 1 | 1 | REIMPLEMENT | `api/recovered/Assembly-CSharp/PaseliConsumeButton.cs:632`. |
| `Ea3EacoinQueryLastNotch` | 1 | 1 | REIMPLEMENT | `api/recovered/Assembly-CSharp/PaseliConsumeButton.cs:532`. |
| `Ea3EacoinQueryConsumeExcess` | 1 | 1 | REIMPLEMENT | `api/recovered/Assembly-CSharp/PaseliConsumeButton.cs:547`. |
| `Ea3EacoinQuertReauthNeed` + `Ea3EacoinReauth` | 2 | 2 | REIMPLEMENT | wallet PIN re-auth flow: `api/recovered/Assembly-CSharp/PaseliUtility.cs:433,439`. PIN handling lives here, card PIN is cardmng `authpass`. |
| `Ea3EacoinOpchpass` | 1 | 1 | REIMPLEMENT | wallet password change: `api/recovered/Assembly-CSharp/CardEntryScene.cs:276`. |
| `Ea3EacoinQuertGeneralStatus` | 1 | 3 | LOCAL-STUB | `api/recovered/Assembly-CSharp/TestModeSkeleton/IncomeLog.cs:151`, `VirtualCoinSettings.cs:180`, `IncomeLog.cs:502` (income log gate). |
| `Ea3EacoinAbort` + `Ea3EacoinQueryAbortStatus` | 2 | 2 | REIMPLEMENT | session abort: `api/recovered/Assembly-CSharp/PaseliConsumeButton.cs:492,498,506`. |
| `Ea3EacoinQueryConsignIsValid` | 1 | 1 | REIMPLEMENT | `api/recovered/Assembly-CSharp/CardEntryScene.cs:717`. |
| `Ea3EacoinQueryHoldPrice` | 1 | 1 | REIMPLEMENT | `api/recovered/Assembly-CSharp/PaseliConsumeButton.cs:397`. |
| `Ea3EacoinReport` (internal delegate) | 1 | 2 | LOCAL-STUB | wallet income report write, test-menu only path: `api/recovered/Assembly-CSharp/TestModeSkeleton/TestModeMain.cs:166` (`/eacoin/last52weeks.xml` path), `VirtualCoinSettings.cs:204`. |
| `Ea3EacoinPlayerlog` (internal delegate) + `Playerlog(CardType,...)` wrapper | 2 | 2 | LOCAL-STUB | `api/recovered/Assembly-CSharp/TestModeSkeleton/IncomeLog.cs:506` (player history fetch). |
| `Ea3EacoinIncomelog` (internal delegate) + `Incomelog(int)` wrapper | 2 | 1 | LOCAL-STUB | `api/recovered/Assembly-CSharp/TestModeSkeleton/IncomeLog.cs:463`. INFERRED: wrapper+delegate pair needed for the gacha/income history screen. |
| `Ea3EacoinIncomelogGet(string)` + `Ea3EacoinPlayerlogGet(string)` | 2 | 2 | LOCAL-STUB | detail parse: `api/recovered/Assembly-CSharp/TestModeSkeleton/IncomeLog.cs:303,464` returning `ea3_eacoin_logs` / `ea3_eacoin_log_detail`. |
| `OpenPricelist` (delegate + `string[]` wrapper) + `NextPricelistArticle` (delegate + int wrapper) + `GetPricelistArticle` (delegate + wrapper) + `ClosePricelist` (delegate + wrapper) + `Ea3EacoinQueryPricelistInfo` | 9 | 4 | REIMPLEMENT | price-list enumeration loop (article cursor over eacoin articles): `api/recovered/Assembly-CSharp/PaseliConsumeButton.cs:318,339`, `api/recovered/Assembly-CSharp/ConsumeWindow.cs:376`, `GameUtility.cs:731` path. |
| `Ea3EaCoinGetStatus` (obsolete alias delegate) | 1 | 0 | UNUSED | 0 occurrences (case-sensitive scan; game uses `Ea3EacoinGetStatus`). |
| `Ea3EacoinQueryPoint` | 1 | 0 | UNUSED | 0 occurrences. |
| `Ea3EacoinQueryAutochargeStatus`, `Ea3EacoinQueryAutochargeIsTriggerd` | 2 | 0 | UNUSED | 0 occurrences. |
| `Ea3EacoinQueryInshopchargeStatus`, `Ea3EacoinQueryInshopchargeIsDetected` | 2 | 0 | UNUSED | 0 occurrences. |
| `Ea3EacoinQueryEacoinIsContracted`, `Ea3EacoinQueryConsignIsContracted` | 2 | 0 | UNUSED | 0 occurrences. |
| `Ea3EacoinSetPricelistShowcaseidArrayFilter`, `Ea3EacoinSetPricelistCampaignnoArrayFilter` | 2 | 0 | UNUSED | 0 occurrences. |
| `Ea3EacoinQueryArticle`, `Ea3EacoinQueryPricelistShowcase`, `Ea3EacoinQueryPricelistCampaign`, `Ea3EacoinQueryPricelistArticle`, `Ea3EacoinQueryPricelistShowcaseidArray`, `Ea3EacoinQueryPricelistCampaignnoArray`, `Ea3EacoinQueryPricelistPeriodArray` | 7 | 0 | UNUSED | 0 occurrences (pricelist showcase/campaign views not used by this title). |
| `Ea3MainteSearch` (eacoin mainte convenience wrapper) | 1 | 0 | UNUSED | game calls the `AvsEa3.Ea3MainteSearch` variant instead. |

## kamunity/KAMUNITY/AvsEa3CoinTypes.cs

| API group | N | C | Class | Evidence / notes |
|---|---|---|---|---|
| `EA3_EACOIN_ERROR`, `EA3_EACOIN_SESSION`, `ea3_eacoin_logs`, `ea3_eacoin_log_detail` | 4 | 88 | REIMPLEMENT (support types) | `EA3_EACOIN_ERROR` 81 occurrences (`api/recovered/Assembly-CSharp/CardEntryScene.cs:180,661`, `PaseliUtility.cs` heavily); error-code branching all over the billing flow. |
| `ea3_eacoin_pricelist*` marshaled structs (9) | 9 | 0 | UNUSED (marshal detail) | not named in game code; still needed to marshal `Ea3EacoinQueryPricelistInfo` results, so keep with the pricelist REIMPLEMENT row. |

## kamunity/KAMUNITY/AvsEa3Xrpc.cs + AvsEa3XrpcTypes.cs (XRPC engine hooks)

| API group | N | C | Class | Evidence / notes |
|---|---|---|---|---|
| `Ea3XrpcAddModule(name, serv, methods[])` | 1 | 2 | REIMPLEMENT | game-side RPC service registration (netcafe/cloud): `api/recovered/Assembly-CSharp/TCSNET/TcsXrpcModule.cs:221` (`"vfgac"`/`"send_paylog"`, service `"local"`), `api/recovered/Assembly-CSharp/GameUtility.cs:367` context, plus `XrpcModuleCardmng.AddModule()` from `Kamunity.Startup`. INFERRED: "local" service modules are routed back to the board/host by avs2; who must answer them in the port is an open question (see risk 3 below). |
| `Ea3XrpcApply`, `Ea3XrpcIsFinish`, `Ea3XrpcGetStatus`, `Ea3XrpcGetResponseSize`, `Ea3XrpcGetResponseData`, `Ea3XrpcDestroy` (delegate properties) | 6 | n/a (via XrpcBase) | REIMPLEMENT | every game RPC goes through them in `api/recovered/kamunity/KAMUNITY/XrpcBase.cs:65-120` (`StartRequest`/`UpdateRequest`); game call count is indirect via `XrpcService`/`XrpcMethodBase` (see XRPC managed rows below). These 6 + status semantics are the entire native RPC surface. |
| `Ea3XrpcRemoveModule` | 1 | 0 | UNUSED | only referenced by unused `XrpcModuleUploader`/`CcUploader` helpers. |
| Types `XRPC_STATUS_DATA`, `XRPC_STATUS`, `XRPC_SUBCODE`, `XPRC_STATUS_CODE`, `XRPC_FAULT_CODE`, `XRPC_FAULT_SECT` | 6 | 12 | REIMPLEMENT (support types) | `XPRC_STATUS_CODE` named at `api/recovered/Assembly-CSharp/CardEntryManager.cs:198`, `api/recovered/Assembly-CSharp/TestModeSkeleton/GachaTicketLog.cs:119`; `XrpcBase.Status.HasError()` semantics drive all retries. |

## kamunity/KAMUNITY/AvsPackage.cs

| API group | N | C | Class | Evidence / notes |
|---|---|---|---|---|
| `AvsPackageGetStatus(out exists, out finish, out progress)` | 1 | 1 | DELETE | update-flow progress bar only: `api/recovered/Assembly-CSharp/OnlineUpdateMessage.cs:161`; update flow is a plan non-goal. |
| `AvsEa3PkglistUpdateList`, `AvsEa3PkglistGetCount`, `AvsEa3PkglistGet`, `AvsEa3PkglistFileCopy` (+ `PACKAGE_INFO`, `EA3_PKGLIST_TYPE` types) | 4 | 0 | UNUSED | 0 occurrences (`PACKAGE_INFO` named 0). |

## kamunity/KAMUNITY/Cardnumber.cs

| API | N | C | Class | Evidence / notes |
|---|---|---|---|---|
| `ConvertCardNumberToUserCode(CardType, cardid, sb, len)` | 1 | 1 (+2 via `GameUtility.cs:426` alias) | REIMPLEMENT | derives the wallet/user code from the FeliCa UID: `api/recovered/Assembly-CSharp/GameUtility.cs:426` wrapper consumed by card entry (`CardEntryManager.cs`, `PaseliUtility.cs:126`). Native algorithm must be replicated byte-for-byte or existing users get a new account id. |

## kamunity/KAMUNITY/QRLogin.cs (Smart Player Pass / QR login)

| API group | N | C | Class | Evidence / notes |
|---|---|---|---|---|
| `Begin()` | 1 | 1 | REIMPLEMENT (optional feature) | `api/recovered/Assembly-CSharp/BreakTimeWindow.cs:116` (phone-pass re-login during break). |
| `End()` | 1 | 1 | REIMPLEMENT (optional) | `api/recovered/Assembly-CSharp/CardEntryScene.cs:354`. |
| `GetStatus()` | 1 | 7 | REIMPLEMENT (optional) | `api/recovered/Assembly-CSharp/CardEntryScene.cs:529`, `api/recovered/Assembly-CSharp/PaseliConsumeButton.cs:213`, `api/recovered/Assembly-CSharp/PickYourCharaFlow.cs:1281`. |
| `GetQRCode()` | 1 | 6 | REIMPLEMENT (optional) | sprite render for the QR: `api/recovered/Assembly-CSharp/BreakTimeWindow.cs:145`, `CardEntryScene.cs:532`. Uses ZXing (`api/recovered/kamunity/KAMUNITY/QRLogin.cs:56-93`), so the pure part ports; the native `FUNC_GetQRStatus` (URL + cardtype + cardid out) is the unknown part. |
| `GetCardTypeID()` | 1 | 1 | REIMPLEMENT (optional) | `api/recovered/Assembly-CSharp/CardEntryScene.cs:366` returns `(cardtype, cardid)` after phone confirms. |
| Type `QRLogin.STATUS` (+STATUS_SUBCODE not exported) | 1 | 7 | REIMPLEMENT (support) | `api/recovered/Assembly-CSharp/CardEntryScene.cs:353,359,531`. |

## kamunity/KAMUNITY/XRPC managed machinery (used by game subclasses)

| API group | N | C | Class | Evidence / notes |
|---|---|---|---|---|
| `XrpcService` component (`Instance`, `Request`, `Add`) | 3 | 0 direct (reached via `XrpcUtil.ExecRequest` at `api/recovered/kamunity/KAMUNITY/XrpcUtil.cs:27`) | REIMPLEMENT (pure managed) | single-lane request queue with retry on EHTTP/EINPROGRESS (`api/recovered/kamunity/KAMUNITY/XrpcService.cs:81-138`). Created by `Kamunity.Startup`. |
| `XrpcBase` (ctor, `SetRequest`, `StartRequest`, `UpdateRequest`, `Status`, `ResponseXml*` accessors) | 6 | 13 | REIMPLEMENT (pure managed) | base of ~40 game request classes: `api/recovered/Assembly-CSharp/CardEntryManager.cs:45,124,161` etc. |
| `XrpcMethodInternal.ExecRequest` + `Xrpc` property | 2 | 37 | REIMPLEMENT (pure managed) | e.g. `api/recovered/Assembly-CSharp/TCSNET/TcsXrpcModule.cs:86` (`vfgac.send_paylog`), `api/recovered/Assembly-CSharp/XrpcCabinet.cs:34` (`vfglog.error_log`). |
| `XrpcMethodBase` + `XrpcMethodBase<T>` (SetCallback, ResponseHandler, ParseHandler) | 6 | 6 | REIMPLEMENT (pure managed) | subclassed pervasively: `api/recovered/Assembly-CSharp/XrpcCabinet.cs:6`, `TcsXrpcModule.cs:14`, `api/recovered/Assembly-CSharp/TestModeSkeleton/GachaTicketLog.cs:21`, plus `api/recovered/Assembly-CSharp/KAMUNITY/XrpcBaseExtensions.cs` (game-side extension file living in the KAMUNITY namespace). |
| `XrpcModuleBase` | 1 | 1 | REIMPLEMENT (pure managed) | `api/recovered/Assembly-CSharp/TestModeSkeleton/GachaTicketLog.cs:21` base. |
| `XrpcUtil.CreateRequestNode`, `ExecRequest`, `TryGetNodeValue`, `TryGetNodeString`, `Int2Bool` | 5 | 6 | REIMPLEMENT (pure managed) | `api/recovered/Assembly-CSharp/XrpcCabinet.cs:24`, `TcsXrpcModule.cs:73`, `CardEntryManager.cs` parsing. |
| `XrpcResponseHelper` (GetResponseInt64/Int32, GetErrorResponseCode, GetErrorLog, AddModule/ExecRequest) | 5 | 20 | REIMPLEMENT (pure managed) | response parsing: `api/recovered/Assembly-CSharp/CardEntryManager.cs:54,56,62`, `api/recovered/Assembly-CSharp/TestModeSkeleton/GachaTicketLog.cs:81`. |
| `XrpcResponseHelperSimple` | 1 | 0 | UNUSED | 0 occurrences. |
| `XmlPropUtil` (11 create/get helpers) | 11 | 6 | REIMPLEMENT (pure managed) | request XML building: `api/recovered/Assembly-CSharp/XrpcCabinet.cs:26,29,30,31`, `CardEntryManager.cs:137`. |
| `XrpcModuleCardmng` statics (`AddModule`, `Getrefid`, `Bindmodel`, `Authpass`, `Getdatalist`, obsolete `Inquire` ×2) + method classes `XrpcMethodAuthpass`, `XrpcMethodBindmodel`, `XrpcMethodGetdatalist`, `XrpcMethdoGetrefid`, `XrpcMethodInquire` | 12 | 10 | REIMPLEMENT | card login RPCs: `api/recovered/Assembly-CSharp/CardEntryManager.cs:105,124,161,191,232` and `api/recovered/Assembly-CSharp/CardEntryScene.cs:98,124,161,191,232` (`cardmng.getrefid/bindmodel/authpass/getdatalist` XML verbs in `api/recovered/kamunity/KAMUNITY/XrpcModuleCardmng.cs:12,30,56,76`). |
| `XrpcModuleCardmngTypes` (4 response types) | 4 | 9 | REIMPLEMENT (support) | `api/recovered/Assembly-CSharp/CardEntryManager.cs:54,103` (RefID/DataID extraction). |
| `XrpcModuleUploader.*` + `XrpcModuleCcUploader.*` (module + 6 method classes + 2 Add/Remove statics) | 10 | 0 | UNUSED | 0 occurrences (board log upload not used by this title). |
| `XrpcModuleUploaderTypes`, `XrpcModuleCcUploaderTypes` | 2 | 0 | UNUSED | 0 occurrences. |
| `FileUploaderHelper` (5 methods), `FileCcUploaderHelper` (5 methods), `UploaderFileData` (3), `UploaderUtil`, `CcUploaderUtil` (1 each) | 16 | 0 | UNUSED | 0 occurrences each (name scan). |

## kamunity/KAMUNITY/Logger.cs, LoggerInstance.cs, LogHandler.cs

| API group | N | C | Class | Evidence / notes |
|---|---|---|---|---|
| `Logger.Log/LogFormat/LogWarning/LogWarningFormat/LogError/LogErrorFormat/LogException` (both overloads) | 13 | 55 | LOCAL-STUB (keep, trivial) | used everywhere (`api/recovered/Assembly-CSharp/RequestApiBase`-adjacent calls, `api/recovered/Assembly-CSharp/CardEntryScene.cs:565,613,800`, `api/recovered/Assembly-CSharp/GameUtility.cs:838`). Internally routes to `Avs.AvsLog` when active (`api/recovered/kamunity/KAMUNITY/Logger.cs:19-95`); WebGL keeps the Debug branch only. |
| `LoggerInstance` (Replace ×2, OnDestroy) | 3 | 0 | UNUSED | 0 occurrences (the board would install a `LogHandler` here). |
| `LogHandler` (ctor, LogFormat, LogException, Dispose) | 4 | 0 | UNUSED | 0 occurrences. |

## kamunity/KAMUNITY/HttpDownloader.cs, HttpDownloadService.cs, Uploader delegates surface

| API group | N | C | Class | Evidence / notes |
|---|---|---|---|---|
| `HttpDownloader` (SetRequest, Execute, isError, uri/outStream/response/exception props) | 8 | 1 | LOCAL-STUB | direct synchronous use for a DDS notice texture: `api/recovered/Assembly-CSharp/MFG.GameData/NoticeInfo.cs:88-96` (`HttpWebRequest` GET into memory). INFERRED risk: `System.Net.HttpWebRequest` does not exist on Unity WebGL, so this and `Konami.System.Net.RequestCoroutine`/`HttpWebRequestor` must be re-platformed to UnityWebRequest (game-side port work, not an arcade API). |
| `HttpDownloadService` (Instance, Add, Request) | 3 | 0 direct | REIMPLEMENT (pure managed, replaced) | component added by `Kamunity.Startup`; only a queue for `HttpDownloader` on a worker thread (`api/recovered/kamunity/KAMUNITY/HttpDownloadService.cs:57-92`). |

## kamunity/KAMUNITY.Network/XrpcExApi.cs

| API group | N | C | Class | Evidence / notes |
|---|---|---|---|---|
| `Ea3XrpcAddModuleEx`, `Ea3XrpcRemoveModuleEx`, `Ea3XrpcNewEx`, `Ea3XrpcDestroyEx`, `Ea3XrpcApplyEx` (delegate properties) + 3 callback delegate types | 8 | 0 | UNUSED | 0 occurrences; server-to-client callback XRPC variant unused by game code. |

## kamunity/KAMUNITY.IO/AvsFileStream.cs, KAMUNITY.THREADING/AvsThread.cs, MarshalHeap, Tabooword, EXECEXE, ExtDrm, DistributeLoader, CommandLine, Utility

| API group | N | C | Class | Evidence / notes |
|---|---|---|---|---|
| `AvsFileStream` (Stream impl over `Avs.Fs*`, 11 members) | 11 | 0 | UNUSED | game never instantiates it; the game instead uses `Avs.FsOpenW/R` directly (`CabinetSerializer.cs`) or plain `System.IO` under `Utility.GetRawPath()` (`api/recovered/Assembly-CSharp/BaseBackupData.cs:53`, INFERRED: plain `System.IO` works on cabinet because execexe remaps relative paths, WebGL needs its own storage mapping). |
| `AvsThread`, `ThreadWithParam`, `ThreadWithParamCallback` | 3 classes | 0 | UNUSED | avs2-side threads unused by game code. |
| `MarshalHeap` (4 members) | 4 | 0 direct | REIMPLEMENT (pure managed helper) | only used inside kamunity wrappers for ANSI buffers; keep if wrappers are kept. |
| `Tabooword.Check/CheckResut/LoadTaboowordData` | 3 | 0 | UNUSED | local tabooord check unused (server check is used instead: `api/recovered/Assembly-CSharp/MFG.GameRequest/ChkTabooword.cs`). `TaboowordListAsset` (ScriptableObject) also 0 code refs; INFERRED risk: a serialized asset could still reference the class, keeping it avoids a script de-serialization error. |
| `EXECEXE` (CreateFileCache, DebugPuts, GetModuleHandle(A), GetProcAddress, WaitForSingleObject, CloseHandle + 2 delegate fields) | 8 | 1 line (2 calls) | DELETE | execexe file-cache prewarm of 4 resident webm files: `api/recovered/Assembly-CSharp/ResidentFlow.cs:528`; kernel32 P/Invokes + `execexe.dll` export lookup (`api/recovered/kamunity/EXECEXE.cs:28-40`). WebGL has no execexe; drop the prewarm call and the class. |
| `ExtDrm.CreateInstance/ReleaseInstance/CheckStart/CheckAbort/GetResult/GetTotalFileSize/GetProcessedFileSize/GetTotalFiles/GetProcessedFiles` | 9 | 25 | DELETE | edrm/kdm external-drm ROM verification, test mode only: `api/recovered/Assembly-CSharp/TestModeSkeleton/RomCheck.cs:61-216` (gated by `KamunityInstance.isActive`). Plan buckets edrm/kdm into DELETE. |
| `DistributeLoader.DistributeLoaderInitialize/InitializeEx/Finalize/Tick/GetCheckIsValid` | 5 | 0 | UNUSED | 0 occurrences (content distribute/update loader). |
| `CommandLine.GetCommandLineArgs()` | 1 | 0 | UNUSED | game calls `Environment.GetCommandLineArgs()` directly (`api/recovered/Assembly-CSharp/ResidentFlow.cs:959`). |
| `Utility.IsSafeDelegate/ByteArrayToHexString/MakeHashSHA256(Stream)/MakeHashSHA256/MakeHashSHA1/MakeHashMD5/MakeMultiPartBoundaryCode/WriteAsciiStringToStream/Epoch2DateTime×2/GetRawPath` | 11 | 3 | REIMPLEMENT (pure managed, ship as-is) | `api/recovered/Assembly-CSharp/BaseBackupData.cs:53` (`Utility.GetRawPath()` = `"dev/raw"`), `Epoch2DateTime` 2 uses; the rest unreferenced by game code but pure. |

## kamunity root and misc types

| API group | N | C | Class | Evidence / notes |
|---|---|---|---|---|
| `AvsTypes` nested enums/structs (`CardType`, `EA3_BOOT_CODE`, `EA3_BOOT_STATUS`, `EA3_STATUS`, `EA3_PP_STATUS(+SUBCODE)`, `EA3_EACOIN_STATUS`, `EA3_EACOIN_SESSION`(alias), `KEEPALIVE_STATUS`, `AVS_CLOCK`, `AVS_NET_CTRL`, `AVS_NET_TYPE`, `AVS_FS_STAT`, `avs_net_in4_addr`, `avs_net_mac_addr`, `FS_FLAG`, `FS_MODE`, `FS_SEEK`, plus unused `PROPERTY_*`, `AVS_THREAD_*`, `AVS_SCHED_*`, `avs_net_ifconfig`, `AVS_PARAM_SCHED`) | 22 | 137 | REIMPLEMENT (support types) | `AvsTypes.` qualified occurrences 127 total: `EA3_EACOIN_STATUS` 12 (`api/recovered/Assembly-CSharp/CreditCoinDisp.cs:17,96`), `EA3_BOOT_CODE` 31 (`BootSceneFlow.cs:407-410`), `CardType` mapping `api/recovered/Assembly-CSharp/Bi2xInput.cs:165`, `avs_net_in4_addr` `NetworkCheck.cs:290`. Unused: `PROPERTY_*`, thread/sched, ifconfig (count 0 each). |
| `KAMUNITY.AVS.ErrorCode/ErrorSubCode/ErrorSect` enum tables | 3 | 28 (ambiguous) | REIMPLEMENT (support) / mostly UNUSED (INFERRED) | 28 bare `ErrorCode` name matches are mostly the game's own `ErrorCode` properties (`api/recovered/Assembly-CSharp/BootSceneFlow.cs:601,606,610`); `ErrorSect`/`ErrorSubCode` name refs 0. Keep enums for avs error codes surfaced by `AvsThread`/`Xrpc` error paths. |
| `KamunityDllApiAttribute` / `KamunityDllApiClassAttribute` | 2 | 0 | DELETE | binding metadata only for the GetProcDelegate scan; a managed rebuild does not need them. |
| `Bootstrap.IKeyStatus` types already counted in Bootstrap.cs row (not double-counted). | 0 | - | - | - |

## kamunity binding-surface total

Delegate-typed properties bound from `kamunity.dll` by the `KamunityDllApi` scan (the true "kamunity ABI" as far as managed code sees, counted mechanically over the wrapper sources with the `static <FUNC_*> <Name> { get; ... }` pattern plus the `Ea3Xrpc*Delegate`/`Func_Edrm*` variants, OBSERVED): Avs 25 + AvsBookkeeping 25 + AvsEa3 19 + AvsEa3Coin 54 + AvsEa3Xrpc 8 + AvsPackage 5 + AvsPropertyApi 33 + Bootstrap 4 + Cardnumber 1 + QRLogin 3 + DistributeLoader 5 + XrpcExApi 5 + ExtDrm 9 = **196 delegate-typed properties** (the AvsEa3/AvsEa3Coin/AvsEa3Xrpc totals include their `InternalMethod` delegates). The property store API `AvsPropertyApi` (`api/recovered/kamunity/KAMUNITY/AvsPropertyApi.cs`) is fully UNUSED by game code (0 refs to `AvsProperty`, `AvsPropertyNode`, `AvsPropertyApi`; INFERRED: raw property-tree access lives in native avs2; the managed property API was never adopted by this game, so the WebGL side only needs property semantics inside the ea3/XRPC implementation, not as a C# API).

## Shirahama (addressables resolvers + bundle crypto)

| API group | N | C | Class | Evidence / notes |
|---|---|---|---|---|
| `EncryptedAssetProvider` (Initialize, LoadAssetBundle, GetLocationType, GetAssetHash + nested ProviderType/BundlePaths/AssetPaths/ResourcePaths) | 10 | 0 | UNUSED | zero name occurrences in Assembly-CSharp(-firstpass); `aa/settings.json` (1607 bytes) has zero "Shirahama"/"Encrypted" matches; `globalgamemanagers` string scan has zero "Shirahama" matches. OBSERVED: the dump's bundles are plain UnityFS (`docs/00-game-findings.md`), so the decryption path is dead in this build. INFERRED risk: an editor-time addressable setting could wire the provider only for encrypted player builds; keep the assembly compiled to be safe, implement nothing. |
| `BundleHashCache` + `BundleHashCacheSupport` (`LoadBundleHashCache` static, instance Save/Init) | 4 | 0 | UNUSED | 0 occurrences. |
| `DeliveryResolver`, `LocalResolver`, `InternalStorageResolver`, `CompareSelector`, `CachedAssetBundleLocation`, `BundleHashCacheLocation`, `AssetGuard`, `AssetBundleCryptoStream`, `LoadedAssetBundle`, `Chiper`, `DownloadProgress`, `IResourceLocationSupport`, `IRuntimeResolver`, `LocationType` | 14 | 0 | UNUSED | 0 occurrences each (name scan, case-sensitive). |

## VirtualSurround (Konami.VirtualSurround)

| API group | N | C | Class | Evidence / notes |
|---|---|---|---|---|
| `VirtualSurroundGain` (`ch1..ch8`, `speaker*` 8 props, `SetGain`, `GetGain`, `SetFrontGain`, `SetSurroundGain`, `SetWooferGain`, `mixer` field; 19 exported members) | 19 | 2 + 8 property writes | REIMPLEMENT (pure managed, ship as-is) | `api/recovered/Assembly-CSharp/SoundManager.cs:20,60` then writes `_virtualSurroundGain.ch1..ch8` (`SoundManager.cs:66-73`) with per-channel dB from the AudioMixer snapshot logic. OBSERVED: the facade has no P/Invoke at all (`api/recovered/VirtualSurround/Konami.VirtualSurround/VirtualSurroundGain.cs:203-219` is pure `AudioMixer.SetFloat/GetFloat` on exposed params `Gain1..Gain8`), so WebGL keeps it unchanged. INFERRED: the cabinet's spatial channel upmix lives in `VirtualSurroundNative.dll` (Plugins/x86_64) reached through the mixer plugin chain, not through C#; the managed face does nothing cabinet-specific. |
| `SetFrontGain`, `SetSurroundGain`, `SetWooferGain`, direct `SetGain/GetGain` calls | 0 | 0 | UNUSED (already inside the class row) | game writes only the `chN` properties; direct SetGain/GetGain/Set*Gain calls absent. |
| `VUMeter`, Demo `Player`, `VolumeController`, `VUMeterUI` | 4 | 0 | UNUSED (probably scene-attached) | 0 code refs. INFERRED risk: `globalgamemanagers` contains 1 and `resources.assets` 4 "VirtualSurround" strings, so at least one MonoBehaviour from this assembly (likely `VUMeter`) is serialized in assets; keep the assembly, do not delete types. |

## Win10actLog-Runtime (KAM)

| API group | N | C | Class | Evidence / notes |
|---|---|---|---|---|
| `Win10ActLog.GetLastActLog(out ACTIVATION_INFO)` | 1 | 2 | LOCAL-STUB | `api/recovered/Assembly-CSharp/SystemUtility.cs:160`, `api/recovered/Assembly-CSharp/TestModeSkeleton/TestModeMain.cs:40`; OS-activation telemetry string builder. Stub: zero `count` renders `"-"` (`SystemUtility.cs:162-170`). Native `win10actlog.dll` P/Invoke `DllImport("win10actlog")` (`api/recovered/Win10actLog-Runtime/KAM/Win10ActLog.cs:16-20`). |
| `Win10ActLog.GetTimestampString(uint, sb, len)` | 1 | 2 | LOCAL-STUB | `api/recovered/Assembly-CSharp/SystemUtility.cs:164`, `TestModeMain.cs:42`. |
| Type `ACTIVATION_INFO` | 1 | 2 | LOCAL-STUB (support) | same sites. |

## Assembly-CSharp-firstpass: Konami.Aio / Konami.Acc / TCSIO (cabinet hardware faces, P/Invoke-backed)

These live in firstpass, are driven by `Assembly-CSharp` cabinet classes (`api/recovered/Assembly-CSharp/Bi2xInput.cs:3` uses `Konami.Aio` + `TCSIO`, `Bi2xInputVFG.cs:7`, `Bi2xInputTBS.cs:7-8`, `Bi2xInputUKS.cs:7-8`, `api/recovered/Assembly-CSharp/BootSceneFlow.cs:8`), and are all cabinet I/O. Per plan: LOCAL-STUB (spice2x today hooks `aioIobBi2aVFG_*` and discards outputs, `docs/01-spice2x-findings.md`).

| API group | N | C (from Assembly-CSharp) | Class | Evidence / notes |
|---|---|---|---|---|
| `Konami.Aio.aioIobBi2aVFG` (11 DllImports: Open/Close/ClearOutput/GetPara/SetPara/SetPara2/GetInput/SetInput/GetVersion/SetWatchdogTimer/ClearInputStatus) | 11 | driven via `Bi2xInputVFG` | LOCAL-STUB | `api/recovered/Assembly-CSharp-firstpass/Konami.Aio/aioIobBi2aVFG.cs`; VFG board service/test/coin + watchdog. Watchdog: `BootSceneFlow.cs:644,648` (`Bi2xInput.Instance.SetWatchDogTimer`); tile/touch input is game-side (`docs/00-game-findings.md`). |
| `Konami.Aio.aioIobBi2a` (7 DllImports: Open/Close/Release/GetPara/SetPara/InputStatus/InputStatus2) + `aioIobCardId` | 8 | driven via `Bi2xInput*` | LOCAL-STUB | card-puller/SCSI layer, `api/recovered/Assembly-CSharp-firstpass/Konami.Aio/aioIobBi2a.cs:49`; `aioIobCardId.OpenWithSlot` used by card reader setup path. |
| `Konami.Aio.aioIobIcca` (5 DllImports: Open/OpenWithSlot/BeginGetCardId/EndGetCardId/GetDetectedCardId) | 5 | via `Bi2xInput` card detect loop | LOCAL-STUB (card reader: the port replaces it with server-side card entry / typed UID) | `api/recovered/Assembly-CSharp-firstpass/Konami.Aio/aioIobIcca.cs:143`; this is the FeliCa tap face that yields the UID fed to `Cardnumber`/`cardmng`. |
| `Konami.Aio.aioIob2Bi2x` (9), `aioIob2Bi2xAC1` (8), `aioIob2Bi2xTBS` (13), `aioIob2Bi2xTCS` (9) | 39 | only for CabinetType TBS/UKS/AC1 modes | LOCAL-STUB | non-VFG cabinet variants (`api/recovered/Assembly-CSharp/Bi2xInputTBS.cs`, `Bi2xInputUKS.cs`); plan lists other cabinet types as non-goal. |
| `Konami.Aio.aioNCtlIob` (1), `aioNCtlIob2` (1), `aioNMgrIob` (3), `aioNMgrIob2` (1), `aioNodeCtl` (5), `aioNodeMgr` (4), `aioSci` (2+1 delegate-style), `aioSciComm` (1), `aioSystem` (2) | 21 | via `Bi2xInput*`, node manager startup | LOCAL-STUB | node manager/nmgr readiness faked by spice2x today (`docs/01-spice2x-findings.md`). |
| `Konami.Acc.acSystem.Initialize/Release/SetProductInfo` (3 DllImports to `libacc`) + `AC_LOG_LEVEL` | 4 | via `Bi2xInputVFG.cs:7` (using) / cabinet init | LOCAL-STUB | anti-cheat/system lib (`api/recovered/Assembly-CSharp-firstpass/Konami.Acc/acSystem.cs:15-22`). |
| `TCSIO.BoardBase/IOBoard/ICReader/TapeLED` + `TCSIO.Serial.*` (SerialManager et al., System.IO.Ports based) | 17 classes | used only in TBS/UKS paths | LOCAL-STUB | `api/recovered/Assembly-CSharp-firstpass/TCSIO/BoardBase.cs:31-115` serial negotiation; irrelevant on VFG cabinets (INFERRED from cabinet-type gating). |
| `SetIO`, `NewLabel` (firstpass root, test-mode label printers) | 2 classes | 4 | LOCAL-STUB | `api/recovered/Assembly-CSharp-firstpass/SetIO.cs:4`, `NewLabel.cs:3`. |
| `USnd*` audio SDK face (`USndPlugin`, `USndAndroidNativePlayer`, `USnd.AudioManager/Define/Audio3DSettings`) | 6 classes | 43 (`AudioManager` refs from `api/recovered/Assembly-CSharp/SoundManager.cs:48`) | not arcade platform | pure managed wrappers (zero DllImport found in firstpass USnd files, OBSERVED via the DllImport scan); part of the audio stack, ships unchanged. Listed for completeness, excluded from bucket totals. |

# 2. Classification tallies

Authoritative sums of the `N` columns over all table rows in section 1 (members = exported methods, delegate-typed properties, and exported types, each counted once; mechanically re-verified against this file after writing). The P/Invoke tables in section 3 restate firstpass members grouped per DLL and are deliberately NOT added again; rows marked "not arcade platform" (USnd audio stack) are excluded from buckets.

| Bucket | Members |
|---|---|
| REIMPLEMENT | 227 |
| LOCAL-STUB | 171 |
| DELETE | 22 |
| UNUSED | 183 |
| Total inventoried | 603 |

Of the 227 REIMPLEMENT members, 121 are pure-managed code or support types that ship unchanged (rows so labeled "pure managed / ship as-is / support"); ~106 are native-backed members whose behavior must actually be implemented by the WebGL-side platform layer, drawn from kamunity's 196 delegate bindings. The 171 LOCAL-STUB total includes 107 cabinet P/Invoke-backed members in Assembly-CSharp-firstpass (Konami.Aio/Acc/TCSIO).

# 3. PInvokeAndDynamicLoad

## 3.1 DllImport census (Assembly-CSharp + Assembly-CSharp-firstpass)

Total: **129 `[DllImport]` declarations across 28 files** (OBSERVED by scanning both trees; full list below by DLL name).

| Target module | # externs | Files (call context) | Port bucket |
|---|---|---|---|
| `kernel32.dll` / `kernel32` | 8 | `Assembly-CSharp/BaseFlow.cs:133,136` (QueryPerformance* used inside `ScopeConnectCheck`), `Bi2xInputTBS.cs:384,387`, `Bi2xInputUKS.cs:389,392`, `Bi2xInputVFG.cs:250,253` (SetDllDirectoryA/OutputDebugStringA) | DELETE (perf counter replaced by Date/Stopwatch; SetDllDirectory/OutputDebug are Windows-only boot plumbing) |
| `libaio-iob_video` | 24 | `Assembly-CSharp/Bi2xInputVFG.cs:487` (aioIobBi2a_OpenSciUsbCdc), firstpass `Konami.Aio/aioIobBi2a.cs` (7), `aioIobBi2aVFG.cs` (11), `aioIobIcca.cs` (5) | LOCAL-STUB (VFG board: service/test/coin/watchdog/ICCA card) |
| `aio-iob2_video` | 30 | firstpass `aioIob2Bi2xAC1.cs` (8), `aioIob2Bi2xTBS.cs` (13), `aioIob2Bi2xTCS.cs` (9) | LOCAL-STUB (alt cabinets; mostly never reached on VFG) |
| `libaio-iob2_video` | 9 | firstpass `aioIob2Bi2x.cs` | LOCAL-STUB |
| `libaio-iob` | 5 | firstpass `aioNCtlIob.cs`, `aioNMgrIob.cs` (3), `aioNMgrIob2.cs` | LOCAL-STUB |
| `libaio` | 14 | firstpass `aioNodeCtl.cs` (5), `aioNodeMgr.cs` (4), `aioSci.cs` (2), `aioSciComm.cs`, `aioSystem.cs` (2) | LOCAL-STUB |
| `libacc` | 3 | firstpass `Konami.Acc/acSystem.cs:15,18,21` (Initialize/Finalize/acLogger_Report) | LOCAL-STUB |
| `cloudstrap.exe` | 7 | `Assembly-CSharp/Cloudstrap.cs:138-153` (cstrap_uevent_have_session/begin/end/get_callcnt_begin/get_callcnt_end, cstrap_restart_get_count), `Assembly-CSharp/ScopeLib.cs:33` (cstrap_terminate_scope) | DELETE (Konami cloud host; imported from the *host exe*, only present when launched as cloudstrap) |
| `ScopeCloudGaming.dll` | 3 | `Assembly-CSharp/ScopeLib.cs:36,39,42` (ScopeCloudGamingTermStart / TermCheck / GetStatusInformation) | DELETE |
| `Dx11UnityGraphicPlugin` | 4 | `Assembly-CSharp/PluginAccess.cs:18-30` (ShowValue, GetRenderEventFunc, SetRenderCallback, GetDxDevice) | DELETE |
| `Dx11HighLevelScopeControllerPlugin` | 8 | `Assembly-CSharp/PluginAccess.cs:32-69` (PluginCreate/Destroy, ScopeSetup/Cleanup, SetCopyTexture, CreateRenderTargetTexture, GetCaptureCallbackFunction, GetInputDataPlus) | DELETE |
| `user32.dll` / `User32` | 13 | `Assembly-CSharp/MahjongPlayFlow.cs:1188` (GetKeyState, keyboard state peek in gameplay), `MouseInputWrapper.cs:9,12,15` (SendInput/GetMessageExtraInfo/SetCursorPos), `Win32Utility.cs:69-110` (SetDllDirectory/GetDllDirectory/SetCurrentDirectory/ProcessStart/FindWindow/FindWindowEx/GetActiveWindow/ShowWindow/PostMessage) | DELETE (browser input handles keys; window mgmt is Windows-only; note Win32Utility's ProcessStart used to launch helper processes, cabinet-launch plumbing) |
| `iphlpapi.dll` | 1 | `Assembly-CSharp/TestModeSkeleton/NetworkCheck.cs:29` (GetNetworkParams, test-mode NIC dump; buffer via `Marshal.AllocHGlobal` at `:292,307,345`) | LOCAL-STUB (test mode) |

Notes:
- `ScopeCloudGaming.dll` and `cloudstrap.exe` are **not present anywhere in the install** (recursive search over the game folder found neither; OBSERVED), and every call site is gated by `GameUtility.IsCloudStreaming` (e.g. `api/recovered/Assembly-CSharp/BaseFlow.cs:72` calls `ScopeConnectCheck()` only in streaming mode; `api/recovered/Assembly-CSharp/ScopeCloseScene.cs:51-58` runs TermStart/TermCheck handshake; `api/recovered/Assembly-CSharp/ScopeManager.cs` handles the "continue streaming?" prompt using user32 window text). So on a normal desktop launch these imports never execute.
- When invoked (OBSERVED): `ScopeCloudGamingGetStatusInformation` is polled from every flow screen's `Update` through `BaseFlow.ScopeConnectCheck` (`api/recovered/Assembly-CSharp/BaseFlow.cs:139-170`, detects 10s of stale LastAccessRawTime via kernel32 QueryPerformanceCounter => marks `Cloudstrap.IsDisconnect`); `ScopeCloudGamingTermStart/TermCheck` run in the shutdown scene only. `ScopeStatus`/`IsDisconnect` then gates resume UX in 26 files (`api/recovered/Assembly-CSharp/MahjongPlayFlow.cs:1382`, `api/recovered/Assembly-CSharp/GachaPlayFlow.cs:111`, `api/recovered/Assembly-CSharp/GameoverFlow.cs:61`, `api/recovered/Assembly-CSharp/CardEntryScene.cs:844`).
- Native plugin faces from game code: `VirtualSurround` = only `VirtualSurroundGain` AudioMixer writes (`api/recovered/Assembly-CSharp/SoundManager.cs:20,60-73`; no P/Invoke in its assembly); `Win10actLog` = `GetLastActLog`/`GetTimestampString` (`api/recovered/Assembly-CSharp/SystemUtility.cs:160-164`, `api/recovered/Assembly-CSharp/TestModeSkeleton/TestModeMain.cs:40-42`). The Dx11 plugin pair (`Dx11UnityGraphicPlugin`, `Dx11HighLevelScopeControllerPlugin`) is reachable only through `PluginAccess`/`SampleDriver`/`PluginDriver`/`CloudStreamingInputDevice` (`api/recovered/Assembly-CSharp/SampleDriver.cs:22,35,43`, `api/recovered/Assembly-CSharp/CloudStreamingInputDevice.cs:80`), all streaming-gated.
- Marshal-based dynamic binding in game code itself: none. No `Marshal.GetDelegateForFunctionPointer`, `LoadLibrary`, or `GetProcAddress` in Assembly-CSharp(-firstpass) (OBSERVED, 0 matches; `GetDelegateForFunctionPointer` lives only in kamunity/EXECEXE). Game-code `Marshal` use is memory plumbing only: `Marshal.AllocHGlobal/PtrToStructure` for the Scope status struct (`api/recovered/Assembly-CSharp/BaseFlow.cs:145-149`) and iphlpapi buffers (`NetworkCheck.cs:292,307,345`).
- `mscoree`/CLR hosting imports: none in either assembly (OBSERVED; the mscoree concern applies to the native loader side, not this managed code).
- INFERRED: firstpass `Konami.Aio/AIO_*` structs with `[return: MarshalAs(UnmanagedType.U1)]` return raw hardware status structs by ref; a WebGL stub only needs constant "board ready" returns (spice2x precedent: outputs accepted and discarded, `docs/01-spice2x-findings.md`).

## 3.2 kamunity dynamic load (for contrast)

- `-hp <hex>` handle injection or `LoadLibraryA("kamunity.dll")`, then 196 delegate bindings via `GetProcAddress` (see "binding-surface total"). `api/recovered/kamunity/KAMUNITY/KamunityInstance.cs:84-111`, `api/recovered/kamunity/KAMUNITY/NativeMethods.cs:8-15`. DELETE this mechanism in the rebuild; implement the same delegate shapes as managed C#.

# 4. PropertyPathLiterals (Assembly-CSharp game code)

Deduplicated scan of string literals matching `"/..."` across Assembly-CSharp + firstpass (OBSERVED counts = literal occurrences, deduped per line occurrence, grouped by top-level path section). Headline: **the game code contains zero `/ea3/...` literals**. eAC3 property-tree work is entirely inside native avs2/XRPC; the managed side only passes article paths, slot paths, env paths, avs-FS paths, and XRPC XML.

| Section | Count | Examples (file:line) | What it is |
|---|---|---|---|
| `/coin` | 44 | `api/recovered/Assembly-CSharp/EaCoin.cs:33` (`/coin/game_s`), `api/recovered/Assembly-CSharp/EaCoin.cs:129` (`/coin/pp_vfg_premium_start`) | POS credit + participation (PP) consume article paths, keyed by GAME_MODE/CONSUME_MODE tables `EaCoin.cs:29-143`; consumed by `Ea3EacoinConsume`/`Ea3EacoinIconsume` and `PaseliConsumeButton`. |
| `/eacoin` | 8 | `api/recovered/Assembly-CSharp/EaCoin.cs:33` (`/eacoin/game_s_ec` paired article), `api/recovered/Assembly-CSharp/PaseliConsumeButton.cs:320` (consume call) | eacoin (virtual currency) article paths incl. PPEX items (`/eacoin/vfg_jandri_ppex` `EaCoin.cs:49`). |
| `/opt` | 3 | `api/recovered/Assembly-CSharp/EaCoin.cs:25` (`/opt/eacoin/slot1/player`), `api/recovered/Assembly-CSharp/TestModeSkeleton/TestModeMain.cs:166` (`/eacoin/last52weeks.xml` reported via path arg) | eacoin wallet slot path (player slot 1). |
| `/env` | 3 | `api/recovered/Assembly-CSharp/TitleDemoUtility.cs:145` (`/env/eacloud/agentarg/card_id`), `api/recovered/Assembly-CSharp/TestModeSkeleton/SystemInformation.cs:81` (`/env/boot/version`) | env-store reads via `Avs.StdGetEnv`; cloud-pass card args (Konasta/Netcafe only). |
| `/dev` | 4 | `api/recovered/Assembly-CSharp/SystemUtility.cs:149` (`/dev/drive/g/vfgfact00.txt`), `api/recovered/Assembly-CSharp/TestModeSkeleton/CabinetSerializer.cs:23` (`/dev/nvram/cb_` + label) | avs2 file-system paths: nvram cabinet/coin/eacoin XML + factory-boot marker (`CoinSettings.cs:189`, `VirtualCoinSettings.cs:190`). |
| (no-leading-slash) | 3 | `api/recovered/Assembly-CSharp/BaseBackupData.cs:53` (`Utility.GetRawPath() + filename` = `dev/raw/...`), `api/recovered/Assembly-CSharp/GameUtility.cs` (raw path users) | `dev/raw` goes through plain `System.IO` after execexe path remap (INFERRED; on WebGL replace with IndexedDB/persistent storage). |

Not platform property paths (listed so they are not mistaken for `/ea3` paths): ~40 one-off HTTP front-server endpoint literals in `api/recovered/Assembly-CSharp/MFG.GameRequest/*` (`/login` `Login.cs:19`, `/gchat` `GChat.cs:25`, `/gget`, `/keep_alive`, `/appli_boot`, `/janpon_log`, `/entry_game`, `/mission_date`, `/match_result`, plus `MFG.SNSConstants.cs` SNS endpoints) - these target `SystemConfigGameData.FrontServerUrl` over HTTP via `RequestCoroutine`, not the ea3 property store; and content/file path literals (`/Characters/*.webm` 12, `/MFG/Story`, `/Plugins`, `/Tsumikomi*.json` cutin configs). Wrapper-side literals live in kamunity (`/env/profile/soft_id_code` etc. `api/recovered/kamunity/KAMUNITY/Avs.cs:136-171`, `/eacoin/reset` `api/recovered/kamunity/KAMUNITY/AvsEa3Coin.cs`, `/license/agree`, facility names in `api/recovered/kamunity/KAMUNITY/AvsEa3.cs`): those are part of the reimplementation contract, not game literals.

# 5. SmallCustomAssemblies

Managed folder (`game/MFGClient_Data/Managed/`, 129 DLLs, OBSERVED list). After excluding `Unity*`, `System*`, `Mono*`, `mscorlib`, `UniRx`, `UniTask*`, `DOTween*`, `zxing`, `netstandard`, `Springbone*`, `Autodesk.Fbx`, `FbxBuildTestAssets`, `kamunity`, `Shirahama*`, `Assembly-CSharp*`, `VirtualSurround`, `Win10actLog*`, the leftover set is **empty**: no other non-third-party managed assembly exists. Specifically checked the two already-decompiled custom ones:

- `VirtualSurround.dll` -> `api/recovered/VirtualSurround/` (OBSERVED present). Pure managed AudioMixer facade (no P/Invoke); its native partner `Plugins/x86_64/VirtualSurroundNative.dll` is referenced only through the mixer chain (INFERRED).
- `Win10actLog-Runtime.dll` -> `api/recovered/Win10actLog-Runtime/` (OBSERVED present). `DllImport("win10actlog")` -> `Plugins/x86_64/win10actlog.dll`.

Adjacent non-managed observations (for completeness, OBSERVED directory listings): `Plugins/x86_64/` holds `Dx11UnityGraphicPlugin.dll`, `Dx11HighLevelScopeControllerPlugin.dll`, `libacc.dll`, `libaio*.dll` set, `libaio-jcs.dll` (no managed references found), `VirtualSurroundNative.dll`, `win10actlog.dll`. `ScopeCloudGaming.dll` / `cloudstrap.exe` are absent from the whole install (P/Invokes would fail to resolve unless launched by the Konami cloud host; game gates them off in AC mode). INFERRED: arcade/cloud deployments supply them in-process via the host.

# 6. ReimplementSurfaceSummary

(Counts are the section 2 tallies, mechanically summed from the `N` columns of the tables above; type rows counted once each.)

## Must-reimplement core (the real WebGL/native-side work)

The game-facing REIMPLEMENT surface totals **227 exported members** (section 2), of which 121 are pure-managed code or support types that ship unchanged. The *native-behavior* work is **~106 native-backed members**, but it concentrates in 6 clusters of roughly 60 distinct native-equivalent functions:

1. ea3 status/boot/PP + clock + facility + license agree + product domain: ~20 functions (`Ea3GetStatus`, `Ea3GetBootStatus`, `Ea3GetPpStatus`, `Ea3PosevReport(+WithNum)`, `Ea3PcbtrackerReport`, `Ea3QueryServicesProductDomain`, `Ea3QueryFacility*` 6+backing, `Ea3MessageSearch`(mainte), `Avs.StdGetEnv` + `ClockGet`, identity getters). Rough function points: ~20.
2. XRPC: exactly **6 native functions** (`Ea3XrpcApply/IsFinish/GetStatus/GetResponseSize/GetResponseData/Destroy`) + `Ea3XrpcAddModule`, carrying the whole RPC corpus (cardmng verbs, game XRPC modules `vfgac`/`vfglog` via TcsXrpcModule/XrpcCabinet, status-code semantics `XPRC_STATUS_CODE` etc.). Function points dominated by the wire format behind Apply/GetResponseData, not the API count. ~7 APIs but the hardest ~10-15 function-equivalents including property-tree codec and retry/error semantics.
3. eacoin wallet: **~30 called functions** (checkin/checkout/status/session/consume/query family, pricelist enumeration 5, reauth/opchpass, abort, incomelog/playerslog + detail structs). Function points ~35-45 given per-call status machines (INFERRED).
4. card/PIN: `Cardnumber.ConvertCardNumberToUserCode` (algorithm), `cardmng.getrefid/bindmodel/authpass/getdatalist` (server RPCs), QR login 5 (`FUNC_GetQRStatus` out-param state machine, native semantics opaque). ~10.
5. env/identity + userdata persistence: `StdGetEnv` + 4 identity getters, `Avs.FsOpenW/R/Fsize/FileIsExist/IsFsOpenSuccess` nvram XML (6 FS functions), `dev/raw` storage mapping. ~12.
6. bootstrap entry + gates: `Kamunity.Startup`/`isActive`/`isEa3Boot` semantics replacing `-hp`/LoadLibrary binding. ~3.

Total native-behavior function-point estimate: **~85-100 function equivalents** (OBSERVED: ~60 distinct native-backed functions inside the ~106 native-backed REIMPLEMENT members; INFERRED multiplier for state semantics per call, error-code tables `EA3_BOOT_CODE` 31 refs / `EA3_EACOIN_ERROR` 81 refs / `XPRC_STATUS_CODE` are the hidden bulk). LOCAL-STUB totals 171 members: 107 are firstpass cabinet externs (Konami.Aio/Acc/TCSIO), 64 are kamunity-side cabinet hooks (keepalive, netctrl, ikey/fcheck, bookkeeping, actlog, Logger, HttpDownloader); all are "return ready/zero/constant" stubs except the aio* "board ready" contract consumed by `Bi2xInput` and the ICCA card path (or its replacement by typed card entry).

## Wrapper files needing a WebGL-side implementation

- `api/recovered/kamunity/KAMUNITY/Avs.cs` (env+fs+clock+log delegates) - implement delegates in managed C#.
- `api/recovered/kamunity/KAMUNITY/AvsEa3.cs`, `AvsEa3Coin.cs`, `AvsEa3Xrpc.cs`, `Cardnumber.cs`, `QRLogin.cs` - the online core, implement over `IEaPlatform`.
- `api/recovered/kamunity/KAMUNITY/KamunityInstance.cs` (binding bootstrap replacement), `Kamunity.cs`, `XrpcService.cs` + `XrpcBase.cs` + `XrpcUtil.cs` + `XrpcResponseHelper.cs` + `XrpcModuleCardmng.cs` (ship source; verify retry/status semantics).
- `api/recovered/kamunity/KAMUNITY/AvsProperty*.cs` - optionally delete outright (UNUSED).
- Stub-only: `Bootstrap.cs`, `AvsBookkeeping.cs`, `Logger.cs` (route to console), `AvsPackage.cs` (stub progress), `Utility.cs` (ship), `NetworkConfirmWindow`-side keepalive/netctrl constants.
- firstpass `Konami.Aio/*` + `Konami.Acc/*` + `TCSIO/*`: replace with inert "no cabinet" stubs (or strip `Bi2xInput` IO calls under `IsIOEnable=false`, which already exists: `api/recovered/Assembly-CSharp/ResidentFlow.cs:962` `-NOIO` switch).
- Delete: `EXECEXE.cs`, `ExtDrm.cs`, `DistributeLoader.cs`, `PluginAccess.cs`/`SampleDriver.cs`/`PluginDriver.cs`/`CloudStreamingInputDevice.cs`/`ScopeLib.cs`/`Cloudstrap.cs`/`ScopeManager.cs`/`ScopeCloseScene.cs`/`Win32Utility.cs`/`MouseInputWrapper.cs` (game-side, streaming/window plumbing), `Shirahama*` (unreferenced; keep compiled), `Win10actLog` (stub).

## Five riskiest unknowns

1. **R1 XRPC wire semantics.** Everything online funnels through 6 functions (`XrpcBase.cs:65-120`); request/response are opaque XML strings to managed code, but framing/property-encoding/status codes behind `Ea3XrpcApply`/`Ea3XrpcGetResponseData` live only in `avs2-ea3.dll`/`kws.dll` (docs/01). Mitigation: netdump captures, spice2x `responses_raw` (planned in docs/04 Phase 2).
2. **R2 eacoin state machines.** ~30 individually-status-returning APIs with an implicit client-side state machine (checkin -> consume -> query consume status -> balance/hold price/notch). Only return codes are visible; ordering and retry legality come from board behavior not provable from C#. (81 `EA3_EACOIN_ERROR` branch sites show how much semantics live there.)
3. **R3 "local" XRPC modules.** `vfgac` (netcafe paylog) and `vfglog` (error log) are registered with service name `"local"` (`TcsXrpcModule.cs:73`, `XrpcCabinet.cs:24` + `GameUtility` AddModule context); who must answer them in a foreign-server deployment (avs2 local loopback? front server? nothing?) is unproven. If unmatched, gacha/payment logs break in cloud/netcafe mode (acceptable for AC-mode v1? INFERRED yes because AC mode does not register them unless cloud-gated).
4. **R4 card UX replacement.** ICCA reads (`aioIobIcca`, `api/recovered/Assembly-CSharp-firstpass/Konami.Aio/aioIobIcca.cs:143`) feed the UID into `Cardnumber` + `cardmng` + eacoin checkin; browser has no reader, so the port must inject a typed/scanned UID at the `Bi2xInput` seam without breaking the detection loop (`api/recovered/Assembly-CSharp/TitleDemoUtility.cs:159`, `Bi2xInput` gates). `GetQRStatus`'s out-params are equally opaque.
5. **R5 serialization/asset coupling + System.Net.** `resources.assets`/`globalgamemanagers` contain `VirtualSurround` type strings (OBSERVED counts 1 and 4), so deleting assemblies that *scenes* reference (VirtualSurround, possibly TaboowordListAsset) would de-serialize as missing scripts. And the game's whole HTTP layer (`Konami.System.Net/HttpWebRequestor.cs`, `RequestCoroutine.cs`) plus `HttpDownloader` uses `System.Net.WebRequest`, unusable on WebGL; re-platforming it is mandatory even though it is not an arcade API. Also INFERRED: `Shirahama.EncryptedAssetProvider` may be wired through editor-only addressable settings invisible in this dump.
