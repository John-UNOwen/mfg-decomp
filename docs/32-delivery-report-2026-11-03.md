# 32. Delivery report - 2026-11-03 (defect-ledger phase close-out)

Lane: `https://mfg.0006767.xyz` (gate pid 2136, binary rebuilt, stamp `v=D47444325EDD`, core pid 7824 untouched). Live build: 29,331 brotli sidecars, 29/29 TileSurface groups in the catalog.

## What shipped this phase

**Gate (WebFront, `webfront/Program.cs`, commits `1a90313` and prior `ba13651`..`5f2411d`)**
- Identity-route budget 600 -> 1500/min per visitor (L29): the 600 cap 429'd a legitimate mid-match `POST /aog/gget` burst and threw the lane into ErrorMode; three consecutive proofs after the raise all walk Boot->Home->Matching->MahjongPlay->Result with zero rate-limit events. Concurrency cap untouched.
- Earlier in the phase: profile-page gating (L2), card-mint 404 (L3), lobby lockout ladder with fail history + code-clears-lockout (L11-L13), `/about` method-agnostic redirect (L14), `Secure` lobby cookie (L15), 5 MB body cap + `/uploads` relay (L16). All verified by curl matrix or private-instance ladder tests (docs/31 rows).

**Gacha: honest 5-pull restored (L1/L4/L5/L6/L21/L22)**
Owner decision on the ledger's critical item: revert, not implement 20-pull. Build-repo `ddee40e293` reverts `58b6561055` cleanly - `GachaPlayUtility.Count()=5`, "5回ガチャ" labels in code, both scenes, translation and cabinet bookkeeping. `dbca14dd01` makes `PlayerItemInfo.Consume` refuse short balances before touching state, killing the wipe-and-persist path. The 17:03 fast build (11 min il2cpp, post-revert) and the 17:52 full build produced byte-identical player data (same stamp both assembles): the live lane has carried the 5-pull semantics since 17:04.

**Tile surfaces (L8)**
Full content build packed the 1,064 loose tile sprites for surfaces 002-029 (from the earlier slicer commit `3151c387b9`). Live `catalog.json` covers 29/29 surfaces x 38 keys. Proof 225520 shows the TileManager fallback working on the lane: `TileSurface001.spriteatlas` address miss -> loose sprites -> match completes with zero tile request failures. Surfaces 002+ now have real content behind the same fallback; which equipped surface the lane *renders* depends on the profile served by the core (see queued).

**Audio (A1/A2, earlier in phase)**
UniTask-native semaphore (permit re-handoff) live: every proof of the phase recorded the full chain TITLE->MENU->HOME->CASTING->4MJG1_1->RESULT2->RESULT3, `got=False` 0, `is not found` 0, 33 VOX loads, no cancellation leaks. Loop-seam measurement stands (4-13 ms, inaudible). Final acceptance: by ear on the lane.

**Build tooling**
`tmp/build_webgl_full.ps1` + `tmp/rebuild_full_deploy_proof.ps1`: full-content-build chain (packing new/changed bundled assets) alongside the existing fast chain (reuses staged bundles). Fast chain carries the `link.xml` restore guard (L26 closed). Full build repacked 29,322 groups in 42 s; fast build 11 min when scripts change.

## Proof record (headless, account audo8y45)

| run | stamp | last scene | notes |
|---|---|---|---|
| 214330 | 25299D8E63C9 | Result | first clean match under raised cap; 0 got=False |
| 220631 | D47444325EDD | Boot | transient `ERR_NO_BUFFER_SPACE` on one 11 KB bundle (bundle healthy on disk) — network flake, not a build fault |
| 223554 | D47444325EDD | Result | full BGM chain; 0×429; 4 reqfail = blob + 3× webm news video, pre-existing |
| 225520 | D47444325EDD | Result | after full-content repack; same clean numbers; tile fallback proven live |

## Queued - needs a core restart (PID 7824 is not agent-owned)

1. **A3** `player_item` m_count normalization: plugin fix mirrored + committed (`43232e3`), loads only when core restarts.
2. **L24** core dist parity (62/62 compiled files hash-equal; running core keeps old modules until restart).
3. **L10** storage unique index / boot migration for case-collision defense (script-side already fixed).
4. **L8 render**: audit profile's tiles are served by the core profile pipeline; to see surfaces 002+ *rendered* on the lane, the core must re-serve the profile after restart (equipment flip is in storage).

## Queued - needs a manual lane check

- L5 gacha click-through (draw 5, tickets 50, labels 5回ガチャ) on the lane.
- L9/L23 reserved-over Home + unlimited-button lane scoping confirm.
- L27 telop corner bar alignment at next visit. L28 (content-dependent rect) stays queued by design.

Ledger state: docs/31-open-defect-ledger.md - every item is now fixed+deployed (most proof-verified), resolved, or explicitly queued as above.
