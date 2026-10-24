# mfg-decomp

Reverse-engineering **Mahjong Fight Girl** (Konami arcade, model `VFG`, Unity 2022.3.13f1, Mono) so it can run in a browser.

The game install lives in the AppSandbox container at `\\MYAPPSANDBOX\c\Games\MFG`: `game/`, `modules/`, `prop/`, `dev/` plus the spice2x runtime (`spice64.exe`, source snapshot under `spice2x/` in that path). This repo holds research notes, tooling, inventories, and recovered code. Nothing from the live install (`dev/nvram`, `ee_tmp`, logs) belongs here.

## Current status

- [x] Phase 0: install analysis, runtime analysis (spice2x log + configs), inventories, difficulty assessment
- [x] Phase 1: recovered all game C# (`api/recovered/`), flow trace + platform census (`docs/inventory/`), API inventory and port boundary (`docs/10`, `docs/11`)
- [ ] Phase 2: front-server shim first light (Option S, `-KONAMINET` mock) +  <provider> capture session (Option M evidence)
- [ ] Phase 3: project reconstruction spike
- [ ] Phase 4: WebGL bring-up
- [ ] Phase 5: online feature completion, mobile, scale

Master plan: [docs/04-decompilation-plan.md](docs/04-decompilation-plan.md)

## Docs index

| Doc | Contents |
|---|---|
| [00-game-findings](docs/00-game-findings.md) | What the game is, version, content layout, billing model, how it boots without an .exe |
| [01-spice2x-findings](docs/01-spice2x-findings.md) | What spice2x emulates vs what the game's own DLLs do; the launch contract |
| [02-streaming-mvp-plan](docs/02-streaming-mvp-plan.md) | Interim product: Sunshine + Moonlight Web streaming while decomp proceeds |
| [03-decompile-difficulty](docs/03-decompile-difficulty.md) | Measured difficulty assessment: easy parts, hard parts, effort estimates |
| [04-decompilation-plan](docs/04-decompilation-plan.md) | Master phased plan with gates and exit criteria |
| [10-api-inventory](docs/10-api-inventory.md) | Phase 1 exit: classification totals, the ~60-function reimplement core, the two server surfaces |
| [11-port-boundary](docs/11-port-boundary.md) | Rebuild design: wrapper ABI, Option S (front-server shim) vs Option M (eAC3), gate sequence |
| [inventory/](docs/inventory/) | Generated + traced evidence: assemblies, native exports, ea3 API surface, full flow trace, platform callsites |

## Ground rules

- Game code and assets are Konami copyright. Research material (recovered C#, notes) lives here for study; hosting rights are a separate question.
- The backend is a private e-Amusement server (` <provider-host>`). Be considerate with test load. Never commit card numbers, PCB IDs, PINs, or the `-p` value from launch lines.
- Scripts default to the sandbox install at `\\MYAPPSANDBOX\c\Games\MFG` and are safe to re-run; they regenerate `docs/inventory/` inside this repo.

## Push to GitHub when ready

```
gh repo create John-UNOwen/mfg-decomp --private --source . --push
```
