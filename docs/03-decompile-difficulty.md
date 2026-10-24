# 03 - Decompile difficulty assessment (measured)

Numbers below are measurements from this install, not guesses.

## Measured easy

| Part | Evidence | Effort |
|---|---|---|
| Game C# | Mono, unobfuscated (readable identifiers in Assembly-CSharp), 129 managed DLLs | days with ilspycmd |
| Native coupling | Assembly-CSharp references only kernel32/user32/iphlpapi/mscoree + ScopeCloudGaming.dll; all arcade surface isolated in managed `kamunity.dll` wrapper (44 `ea3_*` names) | bounded, verifiable boundary |
| Bundles | 28,292 plain `UnityFS 2022.3.13f1` bundles, no encryption; 47 MB `catalog.json` GUID map | mechanical extraction (UnityPy) |
| Video | WebM/VP8, natively decodable in browsers | reusable as-is |
| Audio | no FMOD; Unity built-in + VirtualSurroundNative (stub) | easy |
| Scenes | 62 serialized YAML-family scene files, standard format | tedious, automatable |
| DRM | already plaintext on disk; edrm/execexe is boot plumbing a rebuild omits | none |

## The four hard jobs (ranked)

1. **eAC3 client reimplementation** (2-6 weeks). No open-source eAC3 client exists; must reproduce binary property-tree codec + service.init/pcbtracker/cardmng/POS/eacoin flows against  <provider>. Mitigation: spice2x `-netdump` property XML dumps, captured blobs, observable server.
2. **Multiplayer table sync** (wildcard, +weeks). Whether mahjong rules run client-side and which RPCs sync tables; read recovered match code in Phase 1 to collapse this unknown.
3. **Project reconstruction at 2022-LTS addressables scale** (2-6 weeks grind). AssetRipper maturity is uneven for 2022 bundles; plan a custom UnityPy + catalog pipeline preserving GUIDs, re-bake content for WebGL (BC -> ASTC texture re-encode).
4. **Browser performance/footprint** (weeks). IL2CPP wasm + URP UI on mid phones; per-character lazy loading via the already-granular bundle layout; first-load budget engineering.

## Anchors and reality check

- Easier than native static recompiles (IIDX-style, years) because ~90% of game logic is managed.
- Comparable to known Unity dump-to-editor rebuilds, with the twist being the platform layer instead of engine RE.
- No precedent exists for a complete community browser decomp of a Konami eAC3 arcade Unity title; schedule risk is real (Bombergirl's web play is first-party, not a decomp).

## Effort summary (1 experienced RE+Unity dev, full time)

- Toy: title screen + fake server, 3-6 weeks.
- Playable online build vs  <provider>: 4-6 months.
- Comfortable product incl. mobile: 8-12 months. Two people roughly halves calendar time.
