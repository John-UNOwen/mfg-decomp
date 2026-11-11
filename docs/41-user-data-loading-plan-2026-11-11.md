# 41. User-supplied data loading: deep dive and staged plan

Date: 2026-11-11. Status: proposal. The owner asked for a thorough plan for letting people
load their own data through this project and was unavailable for questions, so assumptions
are stated up front and open decisions are marked **DECIDE**. Foundation: a read-only
investigation of three subsystems (client content loading, core data handling, asset
pipeline). Every claim below carries a `file:line` anchor; the full index is Appendix A.
No code was changed by the investigation.

## 0. Assumptions

1. "Load their own data" means three things, handled in tiers: **T1** per-account game data
   (settings, collections, presets), **T2** asset packs (replace or add art, audio,
   animation, prefabs), **T3** fully custom characters (models, rigs, voices).
2. The client and the core stay ours. Both walls this plan runs into (S2) are inside code
   we already maintain, so patching them is in-scope, not a request upstream.
3. Packs carry data, not code. The WebGL player is a fixed IL2CPP wasm image: a bundle can
   only instantiate the 30 asset types compiled into the build (catalog `m_resourceTypes`),
   and MonoBehaviour references bind to shipped script types by name. A pack therefore
   cannot run new code; the worst it can do is data the game misbehaves on. Validation and
   quotas still matter, but there is no arbitrary-execution surface.
4. One hosting lane (webfront + core + webroot) on the current box stays the deployment
   shape for now. Per-user files ride the same origin.
5. Packs are opt-in per account and never change what a non-opted account downloads.

## 1. How it works today

### 1.1 Client content seam (A)

The built game resolves everything through one addressables catalog:
`out/webgl/StreamingAssets/aa/catalog.json`, 56 MB, flat `ContentCatalogData`
(127,114 keys, 70,390 entries, 59,997 internal ids). There is no runtime URL logic:
`AssetBundleResource.CreateWebRequest` takes `TransformInternalId(location)`, which is the
catalog internal id verbatim unless someone sets `ResourceManager.InternalIdTransformFunc`
(vendored `ResourceManager.cs:136-146`; the game never sets it). Bundle ids expand to
relative same-origin URLs, `StreamingAssets/aa/WebGL/<guid>_<hash>.bundle_assets_all_<hash>.bundle`,
fetched by webfront's static layer with brotli negotiation and one-year `immutable` cache
(`webfront/Program.cs:1041-1098`). Gating is page+API only; static asset paths are public
URL space (`Program.cs:665-667`).

Two stock merge mechanisms exist in vendored 1.21.19 and are unused:
`Addressables.LoadContentCatalogAsync` loads another catalog at runtime and registers a
locator (`InitializationOperation.cs:258-298`); key lookups then union across locators
(`AddressablesImpl.cs:266-306`). Or a custom `IResourceLocator` can be registered with
`Addressables.AddResourceLocator` (`Addressables.cs:2324-2333`). Because union returns both
original and mod locations and `LoadAssetAsync` takes the first, **override** is better
done with `InternalIdTransformFunc` rewriting chosen ids to pack files, while **additions**
ride a second catalog. The catalog blob format is fully decoded in `scripts/catalog_decode.py`,
which reads our built catalog unchanged, so pack tooling can read and diff it.

Content updates today mean "swap the whole webroot" plus the stamp/watchdog channel
(`assemble_webroot.ps1:482-532`). Because bundle names carry their content hash and are
served immutable, a pack must ship its own hashed bundle names and its own catalog; reusing
a shipped name with different bytes would serve browser-cached originals for a year.

### 1.2 Core data seam (B)

One SQLite document per account with an open `states: Record<string,string>` map
(`plugins/mfg@asphyxia/handlers/utils.ts:285`), written only through `saveProfile`
(`utils.ts:589`). `client_state_write` has **no kind allowlist** (`handlers/aog.ts:2571`):
any kind name is accepted as long as the body parses as JSON (`aog.ts:2630-2648`) and
passes the taboo screen (`aog.ts:2689`); the six server-owned kinds are refused, three more
re-authored on read (`aog.ts:2721-2756`). Boot delivery iterates every kind
(`aog.ts:2034`), so a new kind is stored and served to the client with zero server work.
Transport caps: 1 MiB per AOG POST (`AogMiddleware.ts:14`), 5 MiB at the gate
(`webfront/Program.cs:323`); real blobs today run 34 B to 51 KB, profile average 19 KB.

The wall is client-side: `ClientStateRead.OnParse` is a hardcoded switch over 27 known kinds
and **silently drops unknown kinds** (`api/recovered/Assembly-CSharp/ClientStateRead.cs:192-194`).
User data visible in-game must either ride an existing client-parsed kind (shaped to the
client's `JsonUtility` classes, the way `customize_item` round-trips verbatim) or extend
that switch, which is a patch in our own recovered sources. The PluginSpace pattern
(`match_archive`, `aog.ts:836-847`) is the precedent for data that is not per-profile state.

### 1.3 Asset pipeline seam (C)

Content state is deterministic YAML: 29,320 groups named by original bundle hash, 30,539
entries, 30,611 labels, generated by `scripts/gen_content_state.py` from the rebuild spec
produced by `scripts/catalog_rebuild_spec.py` (documented in `Assets/Editor/Gate2Content.cs:1-19`).
Addresses are original asset paths; every secondary key the shipped game used survives as a
label, so the same address flavors are available to packs.

Build cost is not a blocker: a full content pack of all 29,322 groups is 6:44 warm
(`C:\mfgtmp\side.log.txt`, SBP cache `Library/BuildCache` 20.5 GB keyed by content hash,
unchanged bundles byte-identical), and content packs without a player rebuild
(`AddressableAssetSettings.BuildPlayerContent`, `AddressableAssetSettings.cs:2709`).
The stock one-group content-update route is dead (needs `BuildRemoteCatalog` and
`StaticContent` groups, both off: `ContentUpdateScript.cs:658-668,729-733`), and
`ProcessAllGroups` has no filter parameter (`BuildScriptBase.cs:138-175`), so the practical
route is a second addressables settings/profile that packs only pack content: same project,
same GUIDs, its own output and catalog.

Import surface for the dominant types is narrow and scriptable: textures need the right
regime (82% Sprite vs 18% Default+mipmaps, per directory), ogg imports with defaults and
re-encodes per target, `.anim` needs no importer at all. What does not exist here is an FBX
route: this project has zero `.fbx` files, rigs are AssetRipper-native `.asset` pieces
(avatars `!u!90`, meshes `!u!43`), and `CharaModel.prefab` hard-binds Avatar, controller,
12 mesh slots, 13 material GUIDs, and component scripts (`CharaModel.prefab:3030-3031`,
`:3236ff`). Custom models work through Unity's own ModelImporter, but that is a new contract
to write and document, which is why T3 is last.

## 2. What blocks what

The three seams exist and none is wired to another. Nothing on the server blocks:
webfront serves any webroot path ungated, the states map is open, storage tolerates new
collections. The two walls are both in client code, and both are ours:

- **S1**: the client initializes one catalog and never calls the merge/transform APIs.
- **S2**: unknown state kinds are dropped by the OnParse switch (`ClientStateRead.cs:192`).

S1 is the load-bearing fix; one boot hook unlocks all of T2 and most of T3.

## 3. Design

```
 user upload (png/ogg/anim/prefab/fbx)            game boot
      |                                              |
 [validation + quota]                                v
      |                                    boot hook (new, our client)
      v                                       |  GET /packs/<refid>.json
 [import farm: Unity batch import]            v
      |                                    manifest (session-gated)
      v                                       |
 [pack build: pack settings profile]          v
      |                                LoadContentCatalogAsync(pack.catalog)
      v                                + InternalIdTransformFunc table
 [pack store: /packs/<id>/...]              |
      |                                     v
 [activation: per-account manifest]   game loads pack assets
```

### 3.1 Pack format

A pack is a directory in the webroot:

```
/packs/<packid>/catalog.json      small addressables catalog (stock format, stock loader)
/packs/<packid>/<guid>_<hash>.bundle_assets_all_<hash>.bundle ...  pack bundles
/packs/<packid>/pack.json         manifest: pack name, author, version, content-state stamp,
                                  entries: [{address, kind: replace|add, bytes, sha256}]
```

Discipline rules:
- Bundle names always carry fresh content hashes (immutability rule from A4; catalog stamp
  machinery already handles the catalog.json refetch).
- `content-state stamp` pins the pack to one official build. Assembles already write
  `build_stamp.txt`; the pack manifest records it. Mismatch = pack inert, with a clear
  in-page notice. Addresses are stable across rebuilds, so repinning a pack is usually
  just re-recording the stamp, validated by a decode diff (`catalog_decode.py`).
- Overrides never use `remove`; a replacement is a replace-entry whose internal id rewrites
  the shipped one. Removals (e.g. hide a costume) are add-entries pointing at a shipped
  blank asset.

### 3.2 Boot hook (client patch, kills S1)

Small new file in the recovered sources, invoked early in `ResidentFlow` (which already
parses player args, `ResidentFlow.cs:995-1009`):

1. GET `packs.json` for this account (webfront route, session cookie; see 3.3). On any
   error: zero packs, game boots exactly as today. This matters: the default official
   path must stay bit-identical when no packs are on.
2. For each pack: `await Addressables.LoadContentCatalogAsync(pack/catalog.json)`.
3. Build an override map from the manifests and assign it to
   `Addressables.ResourceManager.InternalIdTransformFunc`; the transform consults the map,
   falls through to the stock behavior otherwise. Additions resolve through the pack's own
   locator (union lookup), so the transform only needs to handle replaces.
4. Stash loaded-pack state on a static for the UI layer (pack names for a settings page,
   and later for the T1 surfaces).

Risk to note: `InternalIdTransformFunc` rewrites every provider id, including the main
catalog location itself; the map is keyed by exact address string, and the hook must never
rewrite ids outside the pack table. This is a unit-testable pure function; test it before
touching boot order.

### 3.3 Serving and activation (webfront additions)

- Static serving already covers `/packs/...` with zero changes (same origin, brotli
  sidecars via `sidecar.exe`, immutable naming). Generate `.br` for pack files on ingest.
- New routes, reusing the existing session identity plumbing (`Program.cs:1189-1249`):
  - `GET /packs/<refid>.json` (session-gated) returns the per-account activation manifest.
    Per-user privacy here is cheap: the manifest is gated; bundle URLs inside it are the
    public-with-known-URL asset space, same property shipped content already has.
    **DECIDE**: accept public URL space for pack bundles (consistent with shipped content),
    or add signed short-lived paths. Recommendation: accept for v1.
  - `POST /packs/upload` (multipart, session-gated, quota-checked) writes to the ingest
    spool. It deliberately bypasses the AOG 1 MiB cap: packs are not game protocol.
- Activation store: a small JSON file or PluginSpace doc keyed by refid listing enabled
  packs and versions. The `odekake` consent/grant pattern (`utils.ts:901-944`) is the
  model for any later pack-sharing feature.

### 3.4 Import farm (server-side Unity)

End users upload source files, never bundles: requiring users to run Unity 2022.3.13f1
with our project would be the worst distribution story, and building bundles outside the
same project would break script/shader binding. So the box that already builds the game is
also the pack compiler:

1. Ingest spool receives sources with the entry list (addresses to replace/add).
2. Validation: extension allowlist against known regimes (png/ogg/anim/prefab/mat with
   known importer settings, later fbx), declared byte sizes vs actual, sha256, taboo screen
   N/A (binary) but text/label fields screened, hard quota per pack and per account.
3. Import: copy sources into `Assets/UserContent/<packid>/...` with generated `.meta`
   matching the target regime (importer settings from 12k-sample survey: Sprite regime for
   UI, Default+mips for 3D `mdl_*`), run Unity batch mode with a new editor method that
   registers entries under a **separate settings instance** (own profile, own build output)
   and calls `BuildPlayerContent` (packs only; main content untouched, warm caches make it
   minutes not 7 min since main groups are excluded).
4. Publish: new pack id/version written to pack store, `.br` sidecars generated, activation
   never automatic (user opts in).

This is the only component with real operational weight, and it inherits our existing
constraints: build runs serialize (one Unity at a time; the farm queues), the box needs the
headless license, and pack builds must not race webroot swaps. A pack queue keyed by packid
with the existing assemble/swap discipline covers both.

### 3.5 T1: per-user game data

Cheapest tier, do parts of it early. Two delivery shapes:

- **Ride existing kinds**: anything that fits `player_config` or `customize_item` semantics
  needs zero client change (they round-trip verbatim; the server reads but never authors
  `customize_item`). Only worthwhile for data the client already understands.
- **New kind + OnParse patch (recommended)**: choose a kind name outside the reserved set,
  validate on write against a known shape using the `sanitizeCcRequest` parse-canonicalize-
  quarantine pattern (`utils.ts:174-207`), and extend `ClientStateRead.OnParse` with a case
  that feeds a small `JsonUtility`-backed store in the recovered sources. Storage, delivery,
  and auth come free from the existing protocol.
- Bulk import/export for tooling can skip the wire: `ensureProfile`-shaped doc writes
  (`utils.ts:536-560`), but any direct SQL write must keep `__refid` and the core alias
  invariants (`EamuseIO.ts:1381-1438`), so prefer the API path for third-party tooling.
- **DECIDE**: which T1 payloads matter (deck/room presets? lobby cosmetics? notes?). The
  mechanics are the same either way; this is product choice, not engineering.

### 3.6 T3: custom characters (last)

The contract a character pack must satisfy, from the actual prefab:
`Avatar` (native `.asset` today, or a ModelImporter avatar from FBX once the farm supports
it), an `AnimatorController` from the shipped motion family, 12+ `SkinnedMeshRenderer`
slots bound to mesh sub-assets with the bone-name map (`CharaModel.prefab:3237-3243`), 13
materials on the project toon shader (wrong shader = invisible against the shipped variant
set), and the script component graph (ModelManager/Eye/Face/Mouth/CharacterPartsDisp,
Springbone). Server side needs no change (`player_game` chara lists are stored verbatim and
never range-checked), but select-list exposure, `chara_enabled` semantics, voice event keys,
and the matching pool (`MFG_CPU_PIN` 1..21, seat logic) all assume the fixed 21; those are
each small client patches, together a real tier. First step is a spike: one user-made mesh
retargeted into an existing CharaModel via the farm, proving the FBX→avatar→slot path,
before committing to full custom characters. **DECIDE**: worth the tier at all, or cap the
program at "custom skins on shipped characters" (T2 with prefab replaces, already
covered)? Recommendation: cap at reskins for now; the prefab graph is the moat and the
contract is the hard 20% for 80% of the value.

## 4. Security, moderation, operations

- No code execution by construction (S3 assumption, verified against `m_resourceTypes`).
- Abuse vectors are: size (quota + brotli caps), MIME/import surprises (allowlist + import
  farm controls all Unity-side import; raw sources never enter the webroot), content
  moderation of art (manual review before publish is the only honest option; moderation
  queue is real ongoing labor, **DECIDE** who reviews), cross-account exposure (manifest is
  session-gated; public pack library later rides the consent/grant pattern).
- Operational cost to state plainly: a queue of Unity batch builds, one more long-lived
  service (pack store + spool), moderation time per pack, and disk (pack store grows,
  sources kept only until publish, bundle cache already 20.5 GB).
- Failure containment: boot hook fail-softs to stock boot; pack manifest mismatch (stamp)
  disables a pack rather than breaking the game; a pack that loads but renders wrong can be
  disabled by the account owner alone, no server deploy.

## 5. Risks

| Risk | Mitigation |
|---|---|
| Override transform rewrites non-pack ids | exact-match map, pure-function unit tests, fall-through default |
| Pack built against old content state, addresses moved | stamp pin + decode diff at repin; mismatch = inert |
| Name reuse collides with immutable cache | pack bundles always fresh-hash names (rule 3.1) |
| Unity farm serializes game builds behind pack queue | separate editor profile/output, queue with lease, build-time budget |
| WebGL memory grows with pack bytes | per-pack size quota (start small: 50 MB), boot hook logs pack bytes |
| Boot latency: extra catalog fetches | pack catalogs are small (tens of KB); load before first scene, log cost |
| Malicious manifest via upload | manifests generated farm-side, never user-supplied verbatim |

## 6. Phases

Rough engineering-days for one person who knows the stack (us). Each phase is independently
shippable; the order is dependency-driven.

| Phase | Deliverable | Days | Kills |
|---|---|---|---|
| P0 | Boot hook + one hand-built dev pack override end-to-end (local lane only) | 2-3 | S1 |
| P1 | Pack format spec, webfront `/packs/...` routes, activation manifest, quota shell | 3-5 | — |
| P2 | Import farm (batch import, pack settings profile, pack build, publish queue) | 5-10 | manual bundle authoring |
| P3 | T1 new-kind path: write validation + OnParse patch + UI read-back | 2-4 | S2 |
| P4 | Pack upload UX + moderation queue + per-account enable/disable page | 3-5 | CLI-only life |
| P5 (optional) | T3 spike: one FBX mesh into an existing CharaModel; then decide | 3 spike | open question |

Total for the asset-pack product without T3: ~15-27 engineer-days of focused work,
with P0/P1/P3 proving the mechanism end-to-end in under two weeks of effort.

## 7. Open decisions (owner)

1. Pack privacy: public-with-known-URL bundle paths (recommended v1) vs signed paths.
2. Quota numbers and who reviews art.
3. T1 payloads that actually matter to players.
4. Cap at reskins (recommended) vs commit to T3 custom characters.
5. Whether packs may be shared between accounts in v1 (consent/grant pattern) or stay
   private per account.

## Appendix A. Evidence index

A: catalog.json `out/webgl/StreamingAssets/aa/` (56 MB; 127,114 keys / 70,390 entries /
59,997 ids / 30 resource types). `AddressablesImpl.cs:163-188` runtime path;
`AddressablesRuntimeProperties.cs:123-126` token eval;
`AssetBundleProvider.cs:873,912-924` + `AssetBundleResource.cs:310-363` url from internal id;
`ResourceManager.cs:136-146` InternalIdTransformFunc;
`InitializationOperation.cs:156-176,251-253,258-298` multi-catalog;
`Addressables.cs:2324-2333` AddResourceLocator; `IResourceLocator.cs:10-37`;
`AddressablesImpl.cs:266-306` union lookup; `ContentUpdateScript.cs:628-679` dead remote path;
catalog blob format + tooling `scripts/catalog_decode.py`, `scripts/catalog_rebuild_spec.py`;
stamp/watchdog `assemble_webroot.ps1:482-532`; gate static policy `webfront/Program.cs:1041-1098`,
identity plumbing `:1189-1249`, relay caps `:323`.

B: routes table `handlers/aog.ts:4305-4471`; states map `utils.ts:285`; saveProfile
`utils.ts:589-631`; write handler `aog.ts:2571-2774` (JSON gate `:2630-2648`, taboo
`:2689`, owned-kind refusals `:2721-2756`); read-all `aog.ts:1981-2204` (kinds loop `:2034`);
cc_request sanitize template `utils.ts:174-207`; caps `AogMiddleware.ts:14`,
`webfront/Program.cs:323`; OnParse wall `ClientStateRead.cs:92-197`; grant pattern
`utils.ts:901-944`; match_archive precedent `aog.ts:836-847`; db facts: 169 profiles avg
19 KB (`savedata/mfg@asphyxia.db`, read-only).

C: content state `AddressableAssetData/` (29,320 groups / 30,539 entries / 30,611 labels),
generator `scripts/gen_content_state.py`, pipeline notes `Assets/Editor/Gate2Content.cs:1-19`;
no group filter in `BuildScriptBase.cs:138-175`, standalone pack `BuildPlayerContent`
`AddressableAssetSettings.cs:2709`; build timing `C:\mfgtmp\side.log.txt` (6:44 warm),
cache behavior `docs/39` L41-49; importer survey 400 metas (Sprite 82% / Default 18%, zero
platform overrides, zero fbx/wav/mp3); character contract `CharaModel.prefab:3030-3031,
3236ff`.
