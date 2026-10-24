# 04 - Master decompilation plan

Goal: Mahjong Fight Girl running natively in a browser (Unity WebGL), talking to the same private e-Amusement server, with users logging in with their own card/PIN against their own or the shared server URL. Pin the dump version `2026060900`; do not chase updates until Phase 6.

Non-goals for v1: faithful cabinet I/O (coin mech, lamps), Konami cloud hooks, the update flow, other cabinet types.

## Phase 0 - Foundations (done, this repo)

- Install + runtime analysis: docs 00, 01. Inventories: `docs/inventory/`. Toolchain proven: ilspycmd 8.2 recovers the wrapper layer (`api/recovered/`).
- Exit: this repo exists, inventories regenerate, wrappers decompiled. Done.

## Phase 1 - Code surface and port boundary (est. 1-2 weeks)

Purpose: turn "decompile the game" into a closed list of things to reimplement.

Tasks:
1. `scripts/decompile.ps1` across all game-relevant assemblies; commit the wrapper layer, keep `Assembly-CSharp` decompiled output in `out/` (large) with a pointer doc.
2. Build the API inventory: every managed entry point that touches the arcade platform (kamunity wrapper methods, `ea3_*` bindings, `ScopeCloudGaming` usage, plugin P/Invokes), classified as:
   - `reimplement` (ea3 boot, card/PIN, userdata, POS consume, eacoin, facility, message/pcbevent, env/profile),
   - `local-stub` (coin mech, cabinet I/O, actlog, dongle/shield, SNTP/keepalive, board test),
   - `delete` (cloud gaming, Dx11 graphics plugin quirks, update flow).
3. Trace the login/match flows top-down from `ResidentFlow.InitializeOnBoot` through service init, card, lobby, and into a match; document which RPCs a match consumes (collapses the Phase-2 table-sync unknown).
4. Draft the replacement interface (`IEaPlatform`) + property-store semantics in `docs/11-port-boundary.md`.

Exit criteria: a doc listing 100% of platform entry points with proposed semantics, and the match-sync RPC list. Go/No-Go checkpoint: if match sync turns out server-authoritative with heavy RPC choreography, re-evaluate streaming-only.

## Phase 2 - eAC3 client reimplementation (est. 2-6 weeks)

Purpose: a C# library that boots like `kamunity.dll` does, against the real private server.

Tasks:
1. Capture ground truth: run spice2x with `-netdump`/`-automap` for a full session (boot, card, PIN, lobby, gacha, match); save dumps + `../dev/nvram` snapshots into `protocol/captures/` (sanitized, no card IDs).
2. Reverse the wire format: XRPC framing (plain binary property trees; esign off; HTTP 8727), property encoding variants, error/status semantics. Use Ghidra on `avs2-ea3.dll`/`kws.dll` only for unclear opcodes; validate byte-for-byte against captured blobs in `spice2x/easrv/responses_raw/` and dumps.
3. Implement `eac3-cs/`: property store with path semantics (`/ea3/...`, `/opt/...`), service.init + version handshake, pcbtracker/pcbevent/message loops, cardmng (inquire/bindmodel/authpass/getdatalist), POS consume + queue locally, eacoin balance sync, bookkeeping blob format.
4. Bridge to recovered C#: implement `IEaPlatform` over `eac3-cs`.

Exit criteria: a desktop Unity 2022.3 test scene (new project, recovered wrapper + eac3-cs) reaches the game's login flow against  <provider> with a real card and PIN typed. This is Gate 2: the whole web port lives or dies here.

## Phase 3 - Project reconstruction (est. 2-6 weeks, overlaps Phase 2)

Purpose: an editable Unity 2022.3.13f1 project that builds and replays content.

Tasks:
1. Extract: UnityPy pipeline over 28,292 bundles preserving asset GUIDs; AssetRipper pass for built-in resources + scenes (`globalgamemanagers`, 62 levels); audit results for missing script GUID mapping (remap using recovered assembly types by name/hash).
2. Rebuild Addressables: import `aa/settings.json` + `catalog.json` semantics into a regenerated addressable content set pointing at extracted assets; keep per-character grouping so WebGL can lazy-load.
3. Content checks: video codec audit (VP8 confirmed natively decodable; re-encode any stragglers), audio reimport (no FMOD), texture compression plan (BC -> ASTC at WebGL build time).
4. Wire recovered `Assembly-CSharp` + plugins' managed faces into the project with platform calls routed to `IEaPlatform` stubs (Phase 1 boundary).

Exit criteria: editor play mode boots to title and into an offline match against canned responses from `protocol/captures/` at stable 60 fps. Gate 3.

## Phase 4 - WebGL bring-up (est. 2-4 weeks)

Tasks: IL2CPP build; COOP/COEP headers for SharedArrayBuffer threading; strip/stub the native plugin call sites; input verification on desktop browser (keyboard tiles, touch) and a documented keymap; first-load budget work (initial bundle group <= ~150 MB gzipped, videos via streamed URLs, character packs lazy); hosting recipes for the domain (Caddy + COOP/COEP + brotli).

Exit criteria: a visitor with the site password logs in with their own card/PIN in Chrome and plays a real match on the private server. This replaces the streaming MVP for one-player flows.

## Phase 5 - Online feature completion and polish (ongoing)

Multiplayer tables (2tn/3tn/4tn) end to end; gacha flows incl. music gacha and PPEX articles; bookkeeping/gacha log persistence on the server path; mobile pass (phone touch layout, memory, iOS Safari quirks: COOP/COEP support, video autoplay policies); telemetry on boot failures; version-pinning tool (`scripts/inventory.ps1` + dump re-application notes) for future game updates.

## Phase 6 - Optional

Submission candidate for awesome-decomp-games once playable in browser (they list it; site only links open-source projects, game files stay BYO or streamed). Maintenance plan for Konami version bumps; consider upstreaming useful findings to spice2x (GPLv3 compatible).

## Toolchain

| Tool | Use | Status |
|---|---|---|
| ilspycmd 8.2 (dotnet tool) | C# recovery | installed, proven on wrappers |
| UnityPy (python) | bundle/asset extraction with GUIDs | Phase 3 |
| AssetRipper | scenes + built-in resources pass | Phase 3, validate 2022 support |
| Unity 2022.3.13f1 + WebGL module | rebuild + export | must install matching editor |
| spice2x `-netdump`/`-automap`, `responses_raw/` | protocol ground truth | Phase 2 |
| Ghidra | opcode oracle for avs2-ea3/kws | Phase 2, limited scope |
| REA (`rea-agents`, managed/native inspection workflows) | structured evidence for the wire-format and binary questions; can drive Ghidra | Phase 1/2, optional driver |
| ffmpeg | video transcode fallback | Phase 3/4 |

## Risk register

| Risk | Severity | Mitigation |
|---|---|---|
| eAC3 wire format deeper than expected | High | netdump-first approach; Ghidra targeted; Gate 2 kills cheaply |
| Match sync is server-authoritative RPC choreography | High | Phase 1 flow trace decides before Phase 2 spends weeks |
| 2022 LTS reconstruction gaps (AssetRipper) | Medium | custom UnityPy pipeline fallback; GUID-preserving import |
| WebGL perf on phones | Medium | IL2CPP, lazy character packs, first-load budget; desktop-first release |
| Private server etiquette/capacity | Medium | rate-limit test scripts, ask server owner before public beta |
| Copyright of redistributed rebuilt client | Open question | v1 = self-hosted password-gated only; awesome-list style = users supply their own dump; revisit before any public asset hosting |

## Decision log

- 2026-10-23: pin target version `2026060900` (this dump).
- 2026-10-23: repo starts local-first (`mfg-decomp`), push to GitHub later at owner's call.
- 2026-10-23: streaming MVP proceeds in parallel as interim product (docs/02).
