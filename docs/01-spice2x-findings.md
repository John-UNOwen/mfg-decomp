# 01 - Findings: spice2x and the launch contract

spice2x ([spice2x.github.io](https://spice2x.github.io/)) is the GPLv3 open-source continuation of Konami's official SpiceTools bootstrap. Installed build here: `1.0-V-2026-10-03T21:31:57` (= release 26-10-03). Source snapshot extracted at `../spice2x/`.

## What spice2x does for this game

- Detects MFG by DLL fingerprint (`kamunity.dll` + `game/MFGClient_Data` dir, `launcher/launcher.cpp:2295-2303`), loads `kamunity.dll` in-process, applies version-matched hex patches (`patch id VFG-686360e1_3b5c0`).
- Rewrites the eAC3 services URL in memory from `-url` (`avs/ea3.cpp:655-660`); seeds `/env/profile` identity (security code `G*VFGJAA`, PCB ID `-p`), default dev account/hardware IDs.
- Virtual COM3 ICCB card reader via wire-level ACIO bus emulator (`games/mfg/mfg.cpp:30-36`, `acioemu/icca.cpp:109-113` answers "ICCB" for model VFG); card from `-card0` (16-hex FeliCa UID) or physical readers.
- Hooks the game's cabinet I/O (`games/mfg/bi2a_hook.cpp`: `aioIobBi2aVFG_*` in `libaio-iob_video.dll`) for service/test/coin only; all outputs (coin blocker, lamps, LEDs) accepted and discarded. Fakes I/O node managers as "ready". Stubs `kbt.dll`/`kld.dll` board checks.
- Transport support for a foreign server: Schannel TLS registered into AVS net layer (`avs/ssl.cpp`), NIC/LAN spoof (`hooks/networkhook`, `nicspoof`), ICMP keepalive emulation, response field synthesis (`-automap/-netdump`).
- Seeds `dev/nvram` from `prop/defaults` (ea3-config, coin, eacoin). Injects Unity windowed args via `util/unity_player.cpp`. ImGui overlay via D3D11 swapchain hooks.

## What spice2x does NOT do (matters for the rebuild)

- No XRPC/eAC3 protocol code at all. Client protocol lives in Konami's `avs2-ea3.dll` + `kws.dll`: property-tree binary encoding, XRPC framing, esign options, timer schedules. The private server answers; spice2x is only the AVS2 kernel/OS emulator underneath.
- The `-ea` built-in offline server replays canned blobs (`easrv/`, `responses_raw/*.bin`) and does not serve card/eacoin/bookkeeping.
- Mahjong tile/touch input is untouched (game-side), so a browser port does not inherit any spice2x input emulation either.

## Launch contract observed in `../log.txt`

```
spice64.exe -url http://<provider-host>/service/init -p <PCBID> -card0 <UID16HEX> -w -windowborder 2
```

Boot order: banner -> system dumps -> game DLL load (kamunity) -> patching -> ea3 boot (`VFGJAA2026060900`, develop mode) -> execexe instance (ee_tmp) -> service.init handshake -> pcbtracker/cardmng -> title/attract -> card insert -> lobby loops (`message.get`, `pcbevent.put`).

## Assets for the reimplementation (ground truth we can capture)

- spice2x `-netdump`/`-automap` dumps every XRPC call as property-tree XML (`avs/automap.cpp`), ideal for reverse-engineering the schema the rebuilt client must reproduce.
- `spice2x/easrv/responses_raw/*.bin`: captured raw wire responses.
- `dev/nvram/*` and log POS traces: expected server-side state semantics.
- Ghidra only as an oracle for `avs2-ea3.dll`/`kws.dll` wire formats, not for wholesale decompilation.
