# mfg-decomp

Reverse-engineering a Konami Mahjong Game (Unity 2022.3.13f1, Mono) so it can be played in a browser. As of November 2026 the browser play works: a WebGL build hosted behind our own gate, talking to a self-hosted server stack.

This repo holds research notes, tooling, inventories, recovered code, and the reconstruction.

## Where things stand (November 2026)

- **Hosted play is live.** The WebGL build is served by the `webfront/` gate (behind a Cloudflare tunnel): password signup/login on the core WebUI, one access code gating the public entrance, the account's own DataID pinned into the play page per session, and card entry automated at boot - login reaches the menu in seconds with no PIN flow. The full match chain runs end to end: matching, play, results, archives, records. Gacha, Spirit Gym, music gacha, missions, and notices all work.
- **Server stack.** A vendored RyuNET core (`tmp/RyuNET-core`, its own git tree) plus a purpose-built plugin for this title (`tmp/RyuNET-core/dist/plugins/mfg@asphyxia`, the canonical tree per `docs/28`) implements the whole AOG protocol for the supported build. The browser client reaches it through an HTTP shim (`XrpcWebTransport` -> gate `/aog`).
- **Reconstruction tree.** `build/unity-project` (its own git tree) is the compilable Unity project: decompiled code with WebGL-target fixes (flow endgames, async lanes, content fallbacks), content exports under `build/export-primary-content`, build pipeline in `scripts/` (`rebuild_autocard`, `assemble_webroot`).
- **Desktop harness.** The recovered C# lives in `api/recovered/` (also mirrored into the unity tree); the mock front server under `mock/` remains for offline protocol work.

## Repo map

| Path | Contents |
|---|---|
| `api/recovered/` | decompiled game C#, per-assembly |
| `build/unity-project/` | compilable Unity project (own git tree; gitignored from this one) |
| `build/export-primary-content/` | extracted content set (addressables-ready) |
| `webfront/` | .NET 8 public-edge gate (lobby, session pinning, `/aog` relay) |
| `tmp/RyuNET-core/` | vendored server core + canonical plugin (own git tree) |
| `mock/` | desktop front-server mock (historical harness) |
| `scripts/` | build, staging, hosting, and probe scripts |
| `docs/` | numbered plans and findings, see index below |
| `out/` | build outputs (gitignored) |

## Docs index

| Doc | Contents |
|---|---|
| [00-game-findings](docs/00-game-findings.md) | What the game is, version, content layout, billing model, how it boots without an .exe |
| [01-spice2x-findings](docs/01-spice2x-findings.md) | What spice2x emulates vs what the game's own DLLs do; the launch contract |
| [03-decompile-difficulty](docs/03-decompile-difficulty.md) | Measured difficulty assessment: easy parts, hard parts, effort estimates |
| [04-decompilation-plan](docs/04-decompilation-plan.md) | Master phased plan with gates and exit criteria |
| [10-api-inventory](docs/10-api-inventory.md) | Phase 1 exit: classification totals, the reimplement core, the two server surfaces |
| [11-port-boundary](docs/11-port-boundary.md) | Rebuild design: wrapper ABI, front-server shim vs eAC3, gate sequence |
| [inventory/](docs/inventory/) | Generated + traced evidence: assemblies, native exports, ea3 API surface, full flow trace, platform callsites |
| 12-13 | First desktop light; provider capture evidence |
| 15-18 | Match command contract, live-match report, RyuNET comparison, local runbook |
| 19-23 | Assembly compile, content extraction, project merge, browser front, WebGL bring-up |
| 24, 26 | Face asset inventory and face-match fix rounds, kept local and out of git |
| [28-web-identity-hosting-plan](docs/28-web-identity-hosting-plan.md) | The hosted lane: identity model, gate design, delivery matrix |
| [29-checkup-2026-11-02-findings](docs/29-checkup-2026-11-02-findings.md) | Checkup findings against the shipped lane (open defects with evidence) |

## Ground rules

- Game code and assets are Konami copyright. Research material (recovered C#, notes) lives here for study; hosting rights are a separate question.
- The backend is self-hosted (vendored core + our plugin). The public edge is gated by one access code. Never commit lobby codes, DataIDs, card numbers, PCB IDs, PINs, or the `-p` value from launch lines.
- The analysis scripts that read the game install are safe to re-run; they regenerate `docs/inventory/` inside this repo.
