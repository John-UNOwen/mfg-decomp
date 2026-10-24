# mfg-decomp

Reverse-engineering **Mahjong Fight Girl** (Konami arcade, model `VFG`, Unity 2022.3.13f1, Mono) so it can run in a browser.

The game install is one level up (`../`): `game/`, `modules/`, `prop/`, `dev/` plus the spice2x runtime (`spice64.exe`, source snapshot in `../spice2x/`). This repo holds research notes, tooling, inventories, and recovered code. Nothing from the live install (`dev/nvram`, `ee_tmp`, logs) belongs here.

## Current status

- [x] Phase 0: install analysis, runtime analysis (spice2x log + configs), inventories, difficulty assessment
- [ ] Phase 1: recover C# (`scripts/decompile.ps1`), API-surface inventory (`docs/inventory/`)
- [ ] Phase 2: eAC3 client reimplementation spike
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
| [inventory/](docs/inventory/) | Generated inventories: assemblies, native modules, ea3 API surface |

## Ground rules

- Game code and assets are Konami copyright. Research material (recovered C#, notes) lives here for study; hosting rights are a separate question.
- The backend is a private e-Amusement server (` <provider-host>`). Be considerate with test load. Never commit card numbers, PCB IDs, PINs, or the `-p` value from launch lines.
- Scripts expect the install to stay at `../`; they are safe to re-run and regenerate `docs/inventory/`.

## Push to GitHub when ready

```
gh repo create John-UNOwen/mfg-decomp --private --source . --push
```
