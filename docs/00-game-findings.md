# 00 - Findings: the game

Verified against the install at `../` on 2026-10. All paths below relative to the install root.

## Identity

- Konami arcade mahjong gacha title: **Mahjong Fight Girl**, model code `VFG`, soft id `VFG:J:A:A:2026060900` (`prop/ea3-ident.xml`, `game/release_code.xml`). Arcade release March 2023 (Japan), still receiving updates; this dump is the June 2026 build.
- Served today through the private e-Amusement server ` <provider-host>` (` <provider-ip>`), XRPC port 8727, plain HTTP. The game thinks it is an arcade PCB talking to Konami's eAC3 infrastructure.

## Engine and layout

- Unity **2022.3.13f1**, Mono backend (fully managed, `game/MonoBleedingEdge/`), x64. No FMOD; audio is Unity built-in plus Konami `VirtualSurroundNative.dll`.
- `game/MFGClient_Data/`: 62 serialized scenes (`level0..level61`), `globalgamemanagers`, `resources.assets`. `Managed/` has 129 DLLs: game code `Assembly-CSharp.dll` (4.29 MB IL, unobfuscated), Konami wrappers `kamunity.dll` (186 KB, the arcade platform bridge) and `Shirahama*.dll`, plus third-party libs (UniRx, UniTask, DOTween, SpringBone, zxing, Addressables, URP).
- Content: `StreamingAssets/aa/` 28,292 Addressables bundles (5.0 GB, plain unencrypted `UnityFS 2022.3.13f1`) + 47 MB `catalog.json` (GUID map); `StreamingAssets/Characters/` 21 playable characters (`Chara1..Chara21`), 121 WebM videos (VP8), 1.13 GB; `StreamingAssets/Yakuman/` 13 yaku announcement videos.
- Native arcade stack in `modules/` (the game's own Konami DLLs): `avs2-core.dll`/`avs2-ea3.dll` 2.17.4 (AVS2 board + eAC3 client), `kamunity.dll` (native side), `execexe.dll`/`execexe-ex.dll` (loader/path remap, config in `prop/ee-config.xml`), `kdm.dll` (content FS), `kws.dll` (TLS/XRPC helpers), card/wallet/leaderboard modules (`kwl`, `kld`, `kuc`, `ksd`), `libacc.dll`, `win10actlog.dll`, cabinet I/O in `game/MFGClient_Data/Plugins/x86_64/` (`libaio*`, `Dx11UnityGraphicPlugin`, `win10actlog`).

## How it boots (no game .exe by design)

`spice64.exe` (or the arcade's official bootstrap) loads `modules/kamunity.dll` as the game entry; Konami's `execexe.dll` remaps `game/MFGClient_Data/` and `game/` into the host process so UnityPlayer.dll runs in-process (`log.txt:353-355` ReplacePath, `log.txt:382` synthesized Unity cmdline `-screen-fullscreen 0 -hp 0000000180000000`). There is no `MFGClient.exe` anywhere; the host exe provides the Unity entry.

## Billing / service model (from `dev/nvram/ea3-config.xml`, live POS log)

- POS events: `VFG.game.s/c/e` (entry/continue), `VFG.game.gacha` (1) and `gacha5` (5 pulls), `VFG.game.premium.s` / `premium.s.c`, 2/3/4-player mahjong variants (`VFG.game.2tn/3tn/4tn`), `VFG.game.mgacha` (riichi song voice gacha), guest flow `VFG.guest.start/end`, freeplay, service button.
- PASELI-style virtual currency (eacoin) prices: start 100, continue 100, gacha 100, gacha5 500, premium 200, music gacha 300 (`dev/nvram/eacoin.xml`). Participation (PP) events `pp.VFG.*` and PPEX articles for the "jandri" and music gacha.
- Card system: FeliCa-UID based (ICCR unit, `cardmng.inquire/getrefid/bindmodel/authpass/getdatalist`), Smart Player Pass (`sppass.*`), 4-digit PIN typical. Bookkeeping: `bookkeep0/1/2.xml` blobs in nvram. Server-driven timers (`tmrlist`: message poll 1m, pcbtracker 7m, services ~2.5h).

## Input facts that matter for the port

spice2x only emulates Service/Test/Coin + the ICCB card reader for this game; mahjong tiles and the touchscreen are read by the game's own Unity input path (keyboard HID / Windows touch). For a web port this means tile input is already game-side; verify exact key codes in Phase 1 by reading recovered C#.
