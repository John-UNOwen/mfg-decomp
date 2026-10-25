# 21. Project merge — build/unity-project reorganized for Unity 2022.3.13f1

Docs so far: [19-assembly-compile.md](19-assembly-compile.md) proved every recovered tree compiles
against the shipped reference set; [20-content-extraction.md](20-content-extraction.md) exported the
AssetRipper project and staged StreamingAssets. This doc records the third step: merging the recovered
trees into the exported project so the editor opens it with the code compiled as Unity's **predefined
assemblies**, and play mode can boot the game from the shipped addressable bundles — without ever
running the editor (license pending).

Everything here is produced by [`scripts/prepare_project.ps1`](../scripts/prepare_project.ps1)
(idempotent — ran twice with identical results; run 2 deleted nothing) and verified by the artifacts
under `build/project-merge-analysis/`. Nothing is committed to git; the game install share is read-only
input, never written.

## 1. Final layout

| recovered tree (untouched) | placed at | compiled as | .cs placed |
|---|---|---|---|
| `api/recovered/Assembly-CSharp` | `Assets/Assembly-CSharp/` | Assembly-CSharp | 1,315 |
| `api/recovered/Assembly-CSharp-firstpass` | `Assets/Plugins/Assembly-CSharp-firstpass/` | Assembly-CSharp-firstpass | 73 |
| `api/recovered/kamunity` | `Assets/kamunity/` | merged into Assembly-CSharp | 65 |
| `api/recovered/Shirahama` | `Assets/Shirahama/` | merged into Assembly-CSharp | 14 |
| `api/recovered/Shirahama.Async` | `Assets/Shirahama.Async/` | merged into Assembly-CSharp | 3 |
| `api/recovered/Shirahama.Rx` | `Assets/Shirahama.Rx/` | merged into Assembly-CSharp | 0 (tree holds only assembly metadata) |
| `api/recovered/VirtualSurround` | `Assets/VirtualSurround/` | merged into Assembly-CSharp | 5 |
| `api/recovered/Win10actLog-Runtime` | `Assets/Win10actLog-Runtime/` | merged into Assembly-CSharp | 1 |

Unity compiles every `.cs` under `Assets/` (outside `Plugins/`/`Editor/`) into Assembly-CSharp and
everything under `Assets/Plugins/` into Assembly-CSharp-firstpass; there are **0 `.asmdef`/`.asmref`
files** in the project, so the predefined-assembly assignment above is exactly what the editor does.
`Assets/Editor/AssetRipperPatches/AudioMixerPostprocessor.cs` is the only AssetRipper-exported `.cs`
with no recovered equivalent — kept (compiles into Assembly-CSharp-Editor).

Totals after merge: **Assets = 146,599 files / 16.72 GB**, of which 1,477 `.cs` (the table above +
1 editor patch) and 58,743 `.meta`. The AssetRipper code export is gone: `Assets/Scripts/`
(1,314 `.cs`, including the six plugin assemblies AssetRipper exported there) and the firstpass export
(72 `.cs`) were deleted after their metas were carried (below); the recovered trees cover every type
they declared — 0 export files had to be kept (`keeps = 0`, no `_assetripper_only` folders needed),
confirming the recovered trees are a complete superset.

### 1.1 Compile-setting deltas (recorded, per decision)

The docs/19 profile compiles the trees with `LangVersion 10 + AllowUnsafeBlocks`. Unity 2022.3
predefined assemblies compile at **C# 9** and have no UI toggle for unsafe, so:

- `Assets/csc.rsp` = `-unsafe` (+ `-nowarn:1701,1702,0436,1591`): the trees use `unsafe` blocks
  throughout (Konami interop, kamunity, Springbone-adjacent interop).
- The only C# 10 feature present is **file-scoped namespaces** (ilspy output): **909 project-copy
  files** were mechanically rewritten `namespace X;` → `namespace X { … }` (wrapped to EOF; semantics
  identical, verified residual count 0). `api/recovered` itself was never modified, as required.
- Merged plugin trees drop `Properties/AssemblyInfo.cs` (would duplicate `[assembly: AssemblyVersion]`
  seven times in one assembly = CS0579) and `UnitySourceGeneratedAssemblyMonoScriptTypes_v1.cs`
  (one same-named internal class per assembly; seven copies in Assembly-CSharp would collide).
  Assembly-CSharp and firstpass keep their own single copies of both files. Unity regenerates the
  latter itself at player-build time (irrelevant for editor/play mode v1; strip then — see §6).
- Directories literally named `Editor`/`Plugins` inside namespace folders would trigger Unity's
  magic-folder assembly reassignment; the script renames any it finds to `_Editor`/`_Plugins` in the
  copies. None were present in this run.
- `*.csproj` from the recovery builds are excluded from the copies; `ProjectSettings/` (16 `.asset` +
  `ProjectVersion.txt` = 2022.3.13f1) was never hand-edited; 0 asmdefs means none needed removing.

### 1.2 Script GUID carrying (why scene references resolve)

AssetRipper serialized every `.unity/.prefab/.asset` with metas whose GUIDs it **generated** — the
byte-probe (`build/project-merge-analysis/guid_bytes_probe.py` → `guid_bytes_probe.json`) searched 22
meta GUIDs in 5 byte layouts across `globalgamemanagers*`, `resources.assets` and `level0*` and found
zero hits: the original project GUIDs are not recoverable from the player data. The exported tree is,
however, internally consistent, so its metas must be carried into the project — including the metas
AssetRipper generated for types that live in loose dlls (`PluginImporter` metas).

`m_Script` census over the export (`scripts/analyze_script_references.ps1` → `script_refs.json`):
**103,467 refs / 446 distinct script GUIDs** — 6,239 refs across **438** `.cs`-file metas, 97,228
refs across **8** loose-dll importer metas, 0 builtin pseudo-guids, 0 unresolved.

`prepare_project.ps1` built `build/project-merge-analysis/script_guid_map.json` mapping every
exported script meta to its recovered-file equivalent (matched by namespace-qualified type identity),
then wrote the exact original meta bytes onto the placed `.cs` files: **794 metas carried**
(753 Assembly-CSharp + 41 firstpass). Guards in the script verify against the census: **438/438
referenced `.cs` metas are carried** (`referencedGuidsNotCarriedOrKept = 0`,
`referencedInOtherExportFolders = 0`). The 6,239 scene/prefab/asset references bind to the recovered
sources through the carried metas.

The 8 loose-dll provider metas are preserved by keeping those dlls (§2), so their 97,228 references
bind to the retained `PluginImporter` metas. The 21 kept dll metas additionally had their `Editor`
platform switched `enabled: 0 → 1` — AssetRipper exports player dlls editor-**disabled**, and the
editor must load these assemblies or every one of those references resolves to "missing script".
All 21 kept metas verified `ed:1`.

## 2. Loose dll disposition (39 → 21 kept)

All 39 loose dlls carry `AssemblyVersion 0.0.0.0` with no informational-version metadata (stripped
player builds), so package versions were derived from **embedded version string literals** in each
dll (full-literal scan in `loose_dlls.ndjson` + this run) — never from assembly attributes.
`loose_dlls.ndjson` also records each dll's full type list (used by §4) and assembly references.

**Deleted — 18 dlls (+ their .meta; 36 items):**

| dll | why | replacement | version basis |
|---|---|---|---|
| kamunity, Shirahama, Shirahama.Async, Shirahama.Rx, VirtualSurround, Win10actLog-Runtime | source copied into project | recovered trees | n/a |
| Unity.Addressables | manifest package | `com.unity.addressables 1.21.19` | `m_AddressablesVersion: 1.21.19` in shipped `aa/settings.json` + 25× literal `1.21.19` in the dll |
| Unity.ResourceManager | provided by addressables package | (dependency of com.unity.addressables) | 43× literal `1.21.19` |
| Unity.ScriptableBuildPipeline | manifest package (explicit pin = what shipped) | `com.unity.scriptablebuildpipeline 1.21.21` | literal `1.21.21` |
| Unity.AI.Navigation | manifest package | `com.unity.ai.navigation 1.1.5` | 5× literal `1.1.5` |
| Unity.Recorder, Unity.Recorder.Base | manifest package | `com.unity.recorder 4.0.2` | literal `4.0.2` |
| Unity.MemoryProfiler | manifest package | `com.unity.memoryprofiler 1.1.0` | literal `1.1.0` |
| Unity.Profiling.Core | manifest package (also needed by ResourceManager package assembly) | `com.unity.profiling.core 1.0.2` | 7× literal `1.0.2` |
| Unity.Formats.Fbx.Runtime | manifest package (pulls `com.autodesk.fbx` as dependency) | `com.unity.formats.fbx 4.2.1` | literals `4.2.1` (also in FbxBuildTestAssets) |
| Autodesk.Fbx | provided by formats.fbx dependency chain | (com.autodesk.fbx dep of com.unity.formats.fbx) | as above |
| FbxBuildTestAssets | test-sample assembly, 0 serialized refs, 0 loose referrers, provided among formats.fbx package tests | — | literal `4.2.1` |
| System.Runtime.CompilerServices.Unsafe | BCL facade: 0 referrers among the 39 loose assemblies; Unity 2022.3's own managed BCL set ships the same assembly name → keeping it risks a duplicate-assembly load error | Unity built-in BCL | n/a |

Deletion safety guard (in-script): every deleted dll's importer guid is checked against the census —
a deleted dll must carry **0** serialized script references (all 18 do; the 8 provider dlls are all in
the keep set). Cross-assembly reference check from `loose_dlls.ndjson`: the only deleted-assembly
reference among kept loose dlls was `Shirahama.Async → Shirahama`, and both trees merged together into
Assembly-CSharp, so the reference is now intra-assembly. Nothing references kamunity/Shirahama*/
VirtualSurround/Win10actLog except the recovered sources themselves (which are merged alongside).

**Kept loose — 21 dlls (4.2 MB total, all metas editor-enabled):**

| dll | serialized m_Script refs (type fileIDs) | version literal(s) | why kept |
|---|---|---|---|
| UnityEngine.UI | **54,108** (26) | 1.0.0 (+14.0.9) | no recovered source; **provider** — 54k scene refs bind to its meta; `com.unity.ugui` package would re-home those scripts under different GUIDs and orphan them |
| Springbone.Runtime | **27,633** (6) | — | sourceless middleware (SpringBone components on every character prefab) |
| Unity.TextMeshPro | **10,306** (8) | 3.0.6 | **provider**; package (3.0.6) has different script GUIDs — see deviation note |
| UniRx | 4,926 (3) | — | sourceless middleware, provider |
| Unity.RenderPipelines.Core.Runtime | 92 (32) | 14.0.9 | **provider** (Volume/DebugUI-family serialized objects); companion ShaderLibrary + ShaderGraphLibrary dlls kept so no core-package collision is forced |
| Unity.Timeline | 81 (9) | 1.7.6 | **provider** |
| Unity_userfully | 13 (10) | — | sourceless middleware (userfully SDK), provider |
| UniTask | 69 (1) | — | middleware, provider |
| UniTask.Linq / UniTask.Addressables / UniTask.DOTween / UniTask.TextMeshPro | 0 | — | middleware modules; reference UniTask/UI/Addressables/ResourceManager by name — satisfied by the kept loose copies + the addressables package assemblies |
| DOTween | 0 | — | middleware; firstpass tree ships the `DG.Tweening` module files (they are *not* inside DOTween.dll — duplicate scan confirms) |
| zxing.unity | 0 | — | referenced by merged kamunity source at compile time |
| Unity.ProBuilder ×5 | 0 | 5.2.2 | no referrers; package *not* installed: `com.unity.probuilder` depends on `com.unity.render-pipelines.core`, whose assemblies would collide with the kept loose `Unity.RenderPipelines.Core.Runtime.dll` (multiple-precompiled-assemblies hard error) |

**Deviation from decision-2's default, with measured reason:** the decision said Unity-package
assemblies shipped loose should become manifest entries. For TMP/UI/Timeline/RenderPipelines.Core that
is not expressible without breaking the export: their metas are AssetRipper-**generated** GUIDs
(§1.2 — the originals are unrecoverable), while the UPM packages ship the same scripts under the
packages' own fixed GUIDs. Installing them would leave **65,201 serialized references (UI 54,108 +
TMP 10,306 + RPCore 92 + Timeline 81) pointing at GUIDs that exist nowhere** → guaranteed missing
components in every scene, which is worse for a play-mode boot than the cosmetic loss of those
packages' editor-side inspectors. The escape hatch ("keep the loose dll and say so") is applied to
those four + companions + ProBuilder ×5. Consequences recorded in §6.

**`Packages/manifest.json`** (well-formedness machine-checked by re-parse): the 31
`com.unity.modules.*` entries AssetRipper exported, plus exactly the 7 evidence-backed packages
(`com.unity.addressables 1.21.19`, `com.unity.scriptablebuildpipeline 1.21.21`,
`com.unity.ai.navigation 1.1.5`, `com.unity.recorder 4.0.2`, `com.unity.memoryprofiler 1.1.0`,
`com.unity.profiling.core 1.0.2`, `com.unity.formats.fbx 4.2.1`). No
`com.unity.render-pipelines.universal` entry — see §3. No textmeshpro/timeline/ugui/probuilder/core
entries (§2 deviation). Package assemblies resolve into the predefined assemblies automatically,
including into Assembly-CSharp-firstpass (`UniTask.Addressables`/code references to
Unity.Addressables/Unity.ResourceManager bind by name to the package-compiled assemblies, identity
`Version=0.0.0.0` as everything else here).

## 3. Render pipeline verdict

`ProjectSettings/GraphicsSettings.asset` line 68: `m_CustomRenderPipeline: {fileID: 0}` — no pipeline
GUID reference. Deep name scan across all exported `.asset/.unity/.prefab`
(`UniversalRenderPipelineAsset|RenderPipelineGlobalSettings|HDRIPipelineAsset|VolumeProfile`): **0
hits**. No Universal/InputSystem dlls exist in the shipped `Managed/` or `Plugins/`.
**Verdict: the shipped project is Built-in RP; the pipeline resolves with no package and no complaint.**
Materials point at the ~40 AssetRipper dummy shader stubs, so expect **pink materials** until shader
recovery — unrelated to the pipeline slot. The kept loose `Unity.RenderPipelines.Core.*` dlls only
back the Volume/DebugUI-family serialized objects the shipped build carried (§2); they do not change
the active pipeline.

## 4. Duplicate-type scan

Method (`scripts/duplicate_type_scan.ps1` → `duplicate_types.json`): parse every `class/struct/
interface/enum` declaration (namespace-tracked) in the placed copies (1,477 `.cs`, 1,932 distinct
declared names) and intersect against the type lists of all kept loose dlls (from `loose_dlls.ndjson`)
and across placed files; separately regex for assembly-qualified runtime lookups
(`Type.GetType("X, kamunity")`, `Assembly.Load("kamunity")`) naming merged-away assemblies.

- **placed ↔ kept-dll collisions: 19 — all the same name**:
  `UnitySourceGeneratedAssemblyMonoScriptTypes_v1`, Unity's assembly-internal generated class that
  every precompiled assembly legitimately carries its own copy of (it exists in all 21 kept dlls and
  in the single Assembly-CSharp/firstpass copies). No real type collision: DOTween.dll vs the
  firstpass `DG.Tweening` module files, TMP/UI/UniRx/Springbone/zxing vs the trees — zero overlaps.
- **duplicate names within placed code: 63 groups — all flattened-nesting artifacts** (`ViewElement`
  nested in different parents, same enum name in 48 outers, etc.) plus 2 legitimate cross-assembly
  name shadows between Assembly-CSharp and Assembly-CSharp-firstpass. The 0-error merged compile
  (§5) is the proof no true same-assembly duplicates exist (CS0101 would be an error).
- **reflection lookups to merged assemblies: 0 hits** — no code string-load depends on the
  `kamunity`/`Shirahama*`/`VirtualSurround`/`Win10actLog-Runtime` *assembly names* disappearing.
  (Bundle-internal type strings still name them — §6.2.)

## 5. Compile verification without the editor

Plain msbuild at **`LangVersion 9`** (Unity 2022.3's actual default) against the shipped reference set
(`build/reftype` + the shipped Mono BCL targeting pack via the [asmdef-probe props](../build/asmdef-probe/Directory.Build.props)):

| project | inputs | result |
|---|---|---|
| `build/project-merge-analysis/verify/firstpass9/firstpass9.csproj` | placed firstpass copy (transformed) | **Build succeeded, 0 errors**, 0 warnings → `Assembly-CSharp-firstpass.dll` |
| `build/project-merge-analysis/verify/ac9/ac9.csproj` | placed Assembly-CSharp + 6 merged trees, referencing the firstpass dll + reftype middleware stand-ins (= what the editor will reference) | **Build succeeded, 0 errors**, 19 benign decompiler warnings (CS0219 unused locals, CS4014 un-awaited calls) → `Assembly-CSharp.dll` (4.9 MB) |

This proves the namespace transform is complete and semantic-preserving, the six-tree merge introduces
no type/attribute collisions, and the reference graph survives the dll dispositions (reftype supplies
`Unity.Addressables`/`Unity.ResourceManager`/`UniTask.*`/`zxing`/etc. under the same identities the
packages + kept loose dlls will provide).

## 6. Play-mode staging + residual risks

**Addressables v1 strategy (no group rebuild).** The shipped runtime content was copied from the
install (robocopy, incremental, re-run copies 0): `Assets/StreamingAssets/aa/` = **28,295 files,
4.891 GB** — `catalog.json` (46,945,442 B), `settings.json` (1,607 B: `m_buildTarget:
StandaloneWindows64`, `m_AddressablesVersion: 1.21.19`, providers `AssetBundleProvider` +
`BundledAssetProvider`, catalog path `{Addressables.RuntimePath}/catalog.json`) +
**28,292 original bundles** under `StandaloneWindows64/`. The shipped code initializes addressables
from exactly this catalog — that is the runtime's own catalog-local loading mode, so the staged tree
reproduces the shipped play experience with zero rebuilds. `AddressableAssetsData/` (build-only
authoring data) does not exist in any player export and is absent here. Characters/Yakuman on-demand
videos staged: **121 + 13 `.webm`**.

Residual risks, honestly listed:

1. **Shader stubs pink.** Measured 2026-10-25: 2394 material shader refs = 284 built-in + 2110
   pointing at the 40 exported stubs (all ~0-byte placeholders) + 0 unresolved; stub names show the
   real set: Shader Graph assets, Unity-chan toon family (DoubleShadeWithFeather/ShadingGradeMap +
   stencil variants), TMP SDF, particle blends, Konami UI (Bsac_*), ProBuilder pickers. The 40 makes most materials render pink in the editor.
   Play-mode logic is unaffected; visual checks need shader recovery.
2. **Merged assemblies vs bundle type strings.** MonoBehaviour references inside the 28k bundles
   serialize assembly names `kamunity`/`Shirahama*`/`VirtualSurround`/`Win10actLog-Runtime`, which no
   longer exist as separate assemblies after the merge. Editor play mode resolves MonoBehaviours from
   bundles through Unity's serialized-type table with a type-name fallback across loaded assemblies —
   the same fallback that makes script moving between predefined assemblies usually survive at
   runtime — but the first play session should explicitly verify character/audio/QR-scanner
   components bind (signature if it ever misses: `The referenced script (Unknown) on this Behaviour is
   missing!` on instantiated prefab prefabs + null component fields). Fallback plan if needed: ship
   those six trees as asmdef packages under their original names, or keep their loose dlls instead of
   copying the sources.
3. **Bundle↔script GUID rebinding for the kept loose dlls** works only while those dlls stay at their
   AssetRipper metas (that is why they are kept — §2). Editor play mode loads the original bundles
   with original fileIDs; binding uses the kept importer metas + the serialized type table.
4. **Package resolution is offline now (supersedes the registry note):** `scripts/vendor_packages.ps1`
   fetches pinned versions (+ transitive deps) anonymously from packages.unity.com and embeds them
   under `Packages/`. Final embedded set = `com.unity.addressables` 1.21.19 +
   `com.unity.scriptablebuildpipeline` 1.21.21 + `com.unity.ai.navigation` 1.1.5; dependency closure
   verified package.json-by-package.json with zero unsatisfied edges. Dropped as optional (nothing
   references them; restore from `build/reftype` if the serializer ever asks): recorder,
   formats.fbx/autodesk.fbx, memoryprofiler, profiling.core, editorcoroutines. Timeline stays loose
   for its 81 baked GUIDs; as a package it would also collide with the loose dll's assembly name and
   its only consumers were the dropped optional trio. Manifest = built-in modules only + embedded
   packages; first open needs no package registry.
5. **Editor-side assemblies missing** (they were never in the player): TMP and UnityEngine.UI
   inspector drawers, Timeline editors, addressables editor UI after install (the package ships its
   own editor assembly — fine). Runtime components serialize/load normally; inspectors for those
   components look bare. TMP may prompt to "Import TMP Essentials" — the shipped Resources already
   carry the TMP settings/font assets, so dismiss rather than import if prompted.
6. **Player build later (not v1):** Unity regenerates
   `UnitySourceGeneratedAssemblyMonoScriptTypes_v1.cs` at build time → strip the two kept copies
   (Assembly-CSharp + firstpass) first to avoid CS0101; and the merged assembly layout differs from
   the shipped 8-assembly layout (assembly-qualified identity), fine for Mono/editor use, matters for
   re-signing reproductions.
7. **`Assets/Assembly-CSharp` as a folder** is a normal folder (any non-`Plugins`/`Editor` location
   compiles into Assembly-CSharp); placement deviates from "Assets root" only to keep it out of the
   85k-file asset folder — same resulting assembly.
8. Console will show import noise: decompiler warnings (CS0219/CS4014 — same set the probe build
   emits), possible CS0436-family name-shadow warnings around the internal generated class name, and
   importer upgrade chatter on the legacy-version metas. None are blockers.

## 7. First open in 2022.3.13f1 — exact steps

```powershell
# with Unity Hub (recommended): Add project from disk ->
#   <repo>\build\unity-project
# or headless-launch:
& "C:\Program Files\Unity\Hub\Editor\2022.3.13f1\Editor\Unity.exe" `
  -projectPath "<repo>\build\unity-project" `
  -logFile "<repo>\build\unity-project-first-open.log"
```

Expect on first open: package resolution (7 pins) → asset import of 146,599 files (118k of them
small; the 28k-bundle/4.9 GB StreamingAssets tree imports as raw bytes — realistically 20–60 min on
an SSD, Library several GB) → compile of Assembly-CSharp-firstpass then Assembly-CSharp (1,477 files,
C# 9 + csc.rsp `-unsafe`) → auto-open of the startup scene: the exported `EditorBuildSettings.asset`
carries the full shipped scene list (62 entries, index 0 = `Assets/MFG/Scenes/Boot/Boot.unity`, all
62 `.unity` files present on disk).
Expected console content per §6.8 + §6.5. **Play mode:** press Play on the startup scene;
addressables initializes from `StreamingAssets/aa` (catalog-local, §6). If addressables logs a content
load error because the Groups window has no `AddressableAssetsData` yet: open
Window ▸ Asset Management ▸ Addressables ▸ Groups (it creates settings), and pick **“Use Existing
Build”** (loads the staged shipped catalog — do **not** choose Build New Groups, which would rebuild
and is impossible without the original project anyway).

Rerun after any redo: `powershell -ExecutionPolicy Bypass -File scripts\prepare_project.ps1`
(optionally `-SkipBundles`; add `-SkipBundles` while iterating on code placement to skip the 28k-file
bundle diff pass). The persisted guid map + kept-file snapshots under `build/project-merge-analysis/`
make reruns deterministic even though the export folder they were derived from is now deleted; a full
re-export from AssetRipper would rebuild the map from scratch.

## 8. Artifacts

| path | content |
|---|---|
| `scripts/prepare_project.ps1` | the merge (phases §1–§6), idempotent, guards built in |
| `scripts/duplicate_type_scan.ps1` | §4 scanner |
| `scripts/analyze_dlls.ps1`, `scripts/analyze_script_references.ps1`, `scripts/guid_bytes_probe.py` | inputs: dll identities/versions/types, 103,467-ref m_Script census, original-GUID-unrecoverable proof |
| `build/project-merge-analysis/script_guid_map.json` | 794 carried metas + guards (`438/438` coverage proof) |
| `build/project-merge-analysis/loose_dlls.ndjson` | all 39 dlls: identities (all `0.0.0.0`, metadata stripped), refs, full type lists, version literals |
| `build/project-merge-analysis/script_refs.json` | provider table (8 dll metas / 438 cs metas / 0 unresolved) |
| `build/project-merge-analysis/duplicate_types.json` | §4 result detail |
| `build/project-merge-analysis/prepare_report.json` | run totals (rerun-safe record) |
| `build/project-merge-analysis/verify/firstpass9/`, `verify/ac9/` | §5 LangVersion-9 builds |
| `build/unity-project/` | the mergeable project (nothing git-added) |

## 9. Editor run gotchas (first headless import, 2026-10-25)

- License: Unity Personal was granted through Unity Hub sign-in (Store Hub package,
  UIA-driven sign-in; Personal cannot use the manual .alf page). Entitlements land in
  `%LOCALAPPDATA%\Unity\licenses\UnityEntitlementLicense.xml`.
- `Unity.exe` splits quoted argument values on spaces: always launch with space-free paths.
  A junction `C:\mfg -> <repo>` makes `C:\mfg\build\unity-project` usable.
- Embedded vendored packages ship their own NUnit test assemblies; with no test framework in
  the project they produced 5,475 CS0246/CS0538/CS0234 errors and aborted compilation.
  Stripped every `*Tests*` directory from the three embedded packages (vendor_packages.ps1 now
  does this automatically) and deleted the stale Library (its SourceGenerated duplicate
  AssemblyMonoScriptTypes.gen.cs caused CS0579/CS0101/CS0111).
- First refresh also runs AssemblyUpdater over the loose Plugins DLLs (Timeline, zxing,
  userfully...); that is expected, let it finish.

- The decompiled trees carry the game-era generated file
  UnitySourceGeneratedAssemblyMonoScriptTypes_v1.cs (AC + firstpass). Unity 2022 generates the
  same class itself, so the copies must go; prepare_project.ps1 now deletes them (they caused
  CS0101/CS0579/CS0111 even on a clean Library).
