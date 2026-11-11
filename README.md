# mfg-decomp

Reverse-engineering and reconstruction of a Konami arcade mahjong title (Unity 2022.3.13f1, Mono/il2cpp) so it plays in a browser. The result runs in production: a WebGL build served behind a self-built gate, talking to a self-hosted server stack that speaks the game's own AOG protocol.

This repository holds the research notes, evidence, tooling, recovered code, and the reconstruction pipeline.

## Status (November 2026)

| | |
|---|---|
| Browser play | Live. Login through matching, play, results, archives; gacha, Spirit Gym, music gacha, missions, notices all work |
| Live edge | Ubuntu VPS, systemd-managed lane behind a Cloudflare tunnel |
| Staging | A second lane running the identical tree; every change is proven there before promotion |
| Build parity | Live and staging serve the same webroot, byte-verified (`build_stamp.txt` scheme; asset URLs carry `?v=<stamp>` cache keys) |
| Defects | One ledger, [docs/31](docs/31-open-defect-ledger.md): 108 rows; the bulk closed through the 2026-11-03 delivery ([docs/32](docs/32-delivery-report-2026-11-03.md)). Still open: L102 (catalog `TileSurface` gap), L103 (reveal-cancellation logging), L104 (unproven semaphore contention, downgraded); L105 (BGM loop seams) closed owner-accepted 2026-11-10 |
| Retired approach | The spice2x + remote-desktop streaming product (Track A) was dropped 2026-10-24; docs 01-02 are historical |

## Architecture

Four parts, in request order:

1. **Client** - a compilable reconstruction of the game (`build/unity-project`, its own git tree): decompiled C# with WebGL-target fixes (flow endgames, async lanes, content fallbacks), merged with extracted content (`build/export-primary-content`, addressables-ready). Built with Unity batchmode (`MfgRebuild.Gate2Content.BuildWebGLPlayer`).
2. **Gate** - `webfront/`, a .NET 8 single public edge: access-code lobby, username/password signup with a minted 16-hex DataID pinned into the play page per session, card entry automated at boot, and an `/aog` relay the client's HTTP transport (`XrpcWebTransport`) targets in place of the arcade front server.
3. **Server** - a vendored RyuNET core plus a purpose-built plugin (`mfg@asphyxia`) implementing the full AOG protocol for this title. Identity, savedata, matching and settlement all run server-side. The canonical plugin tree and its rationale: [docs/28](docs/28-web-identity-hosting-plan.md).
4. **Tunnel** - a Cloudflare tunnel per lane; the gate trusts `X-Forwarded-*` only from the tunnel's proxy marker.

The client reaches the stack over plain HTTPS; there is no streaming, no video encoding, and the game runs in the player's browser.

## Repository layout

| Path | Contents |
|---|---|
| `api/recovered/` | decompiled game C#, per-assembly |
| `build/` | reconstruction working area: `unity-project/` (own git tree, gitignored here), `export-primary-content/`, content-state artifacts |
| `webfront/` | the .NET 8 gate (lobby, session pinning, `/aog` relay; serves the brotli sidecars generated during assembly) |
| `scripts/` | Windows-side kit: `assemble_webroot.ps1` (staging + stamping + patches), `hosting_up/down`, content extraction, probes; `scripts/linux/` holds the Linux kit twin (below) |
| `scripts/linux/` | bash lane kit: `transfer_build.sh` / `verify_transfer.sh` (manifest-verified webroot transfer), `swap_webroot.sh` (authoritative promotion swap), `lane_up/down`, `lane_smoke.sh`, `assemble_webroot.sh`, `install_systemd.sh` |
| `tmp/` | vendored server core (`RyuNET-core/`, own git tree, largely gitignored) and scratch |
| `.audit/` | lane probe drivers and DB inspectors used by defect work |
| `mods/` | small in-game mods built during research (e.g. `S1Freeze`) |
| `mock/` | desktop front-server mock; historical offline harness |
| `docs/` | numbered findings and plans; index below |
| `AGENTS.md` | working rules for agents operating in this repo (scratch discipline included) |

## Build & deploy

The path from Unity to players:

1. **Build** the WebGL player (Unity batchmode) or fast-rebuild content only.
2. **Assemble** a webroot: `scripts/assemble_webroot.ps1 -Upstream ... -FrontUrl ...` stages the tree, splices the boot argument cluster, stamps asset URLs with a content hash, generates brotli sidecars, and swaps the staged root in behind a proof that the front answers it. Optional patch variants (e.g. `-L105Baked16`, the BGM buffer-path allowlist) are explicit opt-ins; each refuses loudly if its needle is missing.
3. **Prove on staging.** `lane_smoke.sh`-equivalent smoke (17 PASS / 0 FAIL / 3 SKIP with credentials) plus the in-browser harness against the staging edge.
4. **Promote.** `transfer_build.sh` ships the tree as per-directory chunks with per-file sha256 manifests; a convergence rerun must report every chunk verified. The swap itself is `scripts/linux/swap_webroot.sh`: it re-verifies the staged tree against the manifest before stopping anything, renames behind a service stop, polls the stamp over HTTP as proof, and rolls the renames back on any failure.
5. **Disk**: measure hardlink rollback handles with `df` before/after, never `du`.

A fixture harness exercises the kit offline with transport shims; no fixture run touches a real ssh target.

## Audio subsystem

The waiting-room/matching music defect (L105) took three diagnoses in one day; docs 37-40 are the full arc:

- [docs/37](docs/37-music-loop-rca-2026-11-09.md) — RCA of the decode seam (AAC-in-MP4 re-encode, Chrome media-element loop gap). Real, but not the headline.
- [docs/38](docs/38-bgm-wiring-l105-exposure-2026-11-09.md) — BGM wiring audit and exposure map.
- [docs/39](docs/39-bgm-loop-padding-2026-11-09.md) — the padding attempt: shipped, verified against instruments, then **reverted**. The owner's ear correctly reported "nothing changed".
- [docs/40](docs/40-bgm-crossfade-loop-bake-2026-11-09.md) — the actual root cause: the exposed loops are authored as one render of an endless loop, the final ~0.5 s being a baked crossfade into the next pass; whole-file looping therefore replays the start (the "hard skip"). Fixed by skip-head baking 16 clips plus a size-allowlist patch moving exactly those clips onto the gapless `decodeAudioData` buffer path. Owner-accepted 2026-11-09, live on the production lane 2026-11-10. Three tracks with over-long tails remain flagged for hand-tuning.

[docs/30](docs/30-audio-inventory.md) is the original-game audio inventory that preceded all of it.

## Docs index

Foundation and planning:

| Doc | Contents |
|---|---|
| [00](docs/00-game-findings.md) | What the game is: version, content layout, billing model, boot without an .exe |
| [01](docs/01-spice2x-findings.md) · [02](docs/02-streaming-mvp-plan.md) | spice2x emulation surface and the (retired) streaming MVP plan; historical |
| [03](docs/03-decompile-difficulty.md) | Measured difficulty: easy parts, hard parts, effort estimates |
| [04](docs/04-decompilation-plan.md) | Master phased plan with gates and exit criteria |

Reconstruction:

| Doc | Contents |
|---|---|
| [10](docs/10-api-inventory.md) | Phase 1 exit: classification totals, the reimplement core, the two server surfaces |
| [11](docs/11-port-boundary.md) | Rebuild design: wrapper ABI, front-server shim vs eAC3, gate sequence |
| [12](docs/12-s1-first-light.md) · [13](docs/13-s2-provider-capture.md) | First desktop light; provider capture evidence |
| [15](docs/15-gget-gpost-command-contract.md) | Match command contract (`gget`/`gpost`) |
| [16](docs/16-s3-live-match-report.md) | Live match report |
| [17](docs/17-ryunet-comparison.md) · [18](docs/18-ryunet-local-runbook-draft.md) | RyuNET comparison; local runbook draft |
| [19](docs/19-assembly-compile.md) → [23](docs/23-webgl-browser-play.md) | Assembly compile, content extraction, project merge, browser front, WebGL bring-up |
| [inventory/](docs/inventory/) | Generated + traced evidence: assemblies, native exports, ea3 API surface, full flow trace, platform callsites |

Hosting and identity:

| Doc | Contents |
|---|---|
| [28](docs/28-web-identity-hosting-plan.md) | The hosted lane: identity model, gate design, delivery matrix |

Quality and defects:

| Doc | Contents |
|---|---|
| [29](docs/29-checkup-2026-11-02-findings.md) | Adversarial checkup findings against the shipped lane |
| [31](docs/31-open-defect-ledger.md) | open-defect ledger, the single source of truth; every fix round closes or annotates a row |
| [32](docs/32-delivery-report-2026-11-03.md) | Delivery report closing the first ledger phase |
| [34](docs/34-defect-dossier-2026-11-08.md) → [36](docs/36-seat-animation-handoff-2026-11-09.md) | Waiting-room reveal and seat animation dossier, fixes, handoff |
| [37](docs/37-music-loop-rca-2026-11-09.md) → [40](docs/40-bgm-crossfade-loop-bake-2026-11-09.md) | The L105 BGM loop arc (see the Audio subsystem section) |

Visual research: [33](docs/33-face-lower-grey-band-research.md) (face shade-band measurement, research only). Deeper face-asset work lives in docs 24/26, which are kept local and out of git.

## Ground rules

- Game code and assets are Konami copyright. The recovered code and notes here are for study; distribution and hosting rights are a separate, unresolved question.
- Never commit lobby/access codes, session secrets, DataIDs, card numbers, PCB IDs, PINs, or the `-p` value from launch lines. The gate's lobby code lives in a file outside git on every box.
- Public webroots must never contain build bookkeeping (manifests, ledgers): the transfer kit's `BOOKKEEPING_DIR` defaults outside the served tree for exactly this reason.
- The analysis scripts that read a game install are safe to re-run; they regenerate `docs/inventory/`.
- Scratch belongs in the documented temp area (`C:\mfgtmp` on the Windows box), never in the repo tree.
