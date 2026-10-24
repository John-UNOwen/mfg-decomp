# 04 - Master decompilation plan

Goal: Mahjong Fight Girl running natively in a browser (Unity WebGL). v1 accounts are provisioned by our panel (DataID login, per docs/11 Option S); card/PIN parity with the private e-Amusement server arrives with Option M or Phase 5. Pin the dump version `2026060900`; do not chase updates until Phase 6.

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

**Status: complete 2026-10-24.** Deliverables: [10-api-inventory.md](10-api-inventory.md) (603 members classified), [11-port-boundary.md](11-port-boundary.md) (design), `docs/inventory/flow-trace.md` + `platform-callsites.md`. The Go/No-Go question is resolved in code: gameplay sync IS server-authoritative, but as a plain HTTP command log (`/gget`//`/gpost`), not RPC choreography, and the client carries a complete mahjong engine (hint-only today) that a shim can reuse. See docs/10 before re-deriving anything here.

## Phase 2 - Server-side path: front-server shim (Option S) + eAC3 capture (Option M)

Purpose: make the game talk to something we control and prove the login path. Design and evidence: [11-port-boundary.md](11-port-boundary.md).

Phase 1 finding that shapes this phase: all gameplay is front-server HTTP (35 endpoints + command log); eAC3/XRPC only carries card/PIN/PASELI/POS/boot; and the game has a first-party `-KONAMINET` mode that skips the arcade layer entirely.

Spikes:
1. **S1, days, first light.** Local mock front server (canned XML per the `MFG.GameRequest` parsers) + hosts pin + the *current install* booted with `-KONAMINET`, aiming at lobby. Proves the seam and the mockability claim before any reconstruction work.
2. **S2, one night, parallel.** Full  <provider> session with spice2x `-netdump` plus an HTTP-side packet trace; capture the `vfgac.service_list` ServiceUrl value and whether the private server hosts the front-server endpoints. Determines Option M scope.
3. **S3, 2-4 weeks.** Real shim server: the 35 endpoints, table engine behind `/gget`//`/gpost` (port `StructuredHand`/`BlockSet`/`YakuEvaluator`/`MahjongUtility` for rules/score; the shim declares legality flags and wall order), DataID + `bypass_auth` account provisioning from the panel.
4. **M1, 2-6 weeks, only if the S2 capture justifies it.** eAC3 wire protocol in `eac3-cs/` (property-tree codec, XRPC framing, eacoin/POS semantics), validated against netdump captures and `spice2x/easrv/responses_raw/`; Ghidra only as an oracle for unclear opcodes. Hybrid deployments override `FrontServerUrl` (override point: `CardEntryScene.cs:1070-1088`).

Exit criteria (Gate 2): a desktop Unity 2022.3 editor build (recovered game code + shim or ea3 client) reaches the lobby and plays one kyoku. The whole web port lives or dies here.

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
| eAC3 wire format deeper than expected | Medium (Option M only) | Option S needs no proprietary protocol; M1 is capture-gated; netdump-first, Ghidra targeted |
| Gameplay authority | Resolved | server-authoritative command log over front-server HTTP, verified in code (docs/10); client engine is the shim's seed |
| Shim rules-engine fidelity (mahjong edge cases, CPU fill, timeouts) | Medium-High | port the client's own engine classes; use kifu replay (title demo) as the regression harness |
| Front-server response semantics beyond parser-required nodes | Medium | spike S2 packet trace of a real session; mock iterates against live parser failures |
| 2022 LTS reconstruction gaps (AssetRipper) | Medium | custom UnityPy pipeline fallback; GUID-preserving import |
| WebGL perf on phones | Medium | IL2CPP, lazy character packs, first-load budget; desktop-first release |
| Private server etiquette/capacity | Medium | rate-limit test scripts, ask server owner before public beta |
| Copyright of redistributed rebuilt client | Open question | v1 = self-hosted password-gated only; awesome-list style = users supply their own dump; revisit before any public asset hosting |

## Decision log

- 2026-10-23: pin target version `2026060900` (this dump).
- 2026-10-23: repo starts local-first (`mfg-decomp`), push to GitHub later at owner's call.
- 2026-10-23: streaming MVP proceeds in parallel as interim product (docs/02).
- 2026-10-24: gameplay sync resolved: server-authoritative HTTP command log at the front-server seam (docs/10).
- 2026-10-24: Option S (`-KONAMINET` + front-server shim) adopted as the first-light path; Option M eAC3 work is capture-gated behind spike S2. Sign-off: proposed, owner to confirm.
