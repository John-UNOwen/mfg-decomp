# 20 - Content extraction (AssetRipper, no Unity editor)

Ran 2026-10-24 against the pinned dump `2026060900` (docs/00). Tool: AssetRipper GUI.Free **2.0.0** (`tmp/AssetRipperCLI/AssetRipper.GUI.Free.exe`, build 2026-08-24), driven headless over HTTP. No Unity editor touched. Nothing committed to git.

**Verdict: full extraction achieved via the `UnityProject` export.** `build/unity-project` is a real Unity-project skeleton (62 scenes at original paths, `ProjectSettings/`, 58,761 `.meta` files with preserved GUIDs) containing **29,404 of the 29,409 addressable assets in `aa/catalog.json` (99.98%)**. A second, complementary variant (full decompiled C# solution + JSON/GLB dumps) is kept at `build/export-primary-content`. Not copied: the 28,292 `.bundle` binaries themselves (4.85 GB, still in the read-only install).

## 1. Install layout that the tool consumes

Install is read-only; all writes went into the repo. The game has **no `MFGClient.exe`** (docs/00: `spice64.exe` hosts `kamunity.dll` and `execexe.dll` remaps the data paths), so the serializable root handed to AssetRipper is the `_Data` folder itself.

| path (relative to install root) | what it is |
|---|---|
| `game/MFGClient_Data/` | the AssetRipper input: `globalgamemanagers`(+`.assets`/`.resS`), `level0..level61`(+`.resS`), `resources.assets`, `sharedassets0..61`, `boot.config`, `app.info`, `Managed/`, `Plugins/`, `Resources/`, `StreamingAssets/` |
| `game/MFGClient_Data/Managed/` | 129+ Mono DLLs (AssetRipper's Mono init reported **139 assemblies**, skipping 10 native ones) |
| `game/MFGClient_Data/StreamingAssets/aa/StandaloneWindows64/` | 28,292 `.bundle` = 4.85 GB, plain UnityFS |
| `game/MFGClient_Data/StreamingAssets/aa/catalog.json` | 46,945,442 B addressables catalog (JSON-serialized `ContentCatalogData`) |
| `game/MFGClient_Data/StreamingAssets/aa/settings.json` | `m_buildTarget: StandaloneWindows64`, `m_AddressablesVersion: 1.21.19`, single catalog `AddressablesMainContentCatalog` loaded by `ContentCatalogProvider` from `{Addressables.RuntimePath}/catalog.json`, providers = `AssetBundleProvider` + `BundledAssetProvider` |
| `game/MFGClient_Data/StreamingAssets/Characters/`, `.../Yakuman/` | 121 + 13 `.webm` (1,129.6 MB + 130.1 MB) - raw video, never AssetRipper's business |

AssetRipper's own view of the input (from `build/export-logs/assetripper-unityproject.log`): `Mixed game structure has been found at \\MYAPPSANDBOX\c\Games\MFG\game\MFGClient_Data`, `Files use the 'Mono' scripting backend`, all 28,292 bundles enumerated as `Asset bundle '<hash>' has been found`.

## 2. HTTP surface actually used (GUI.Free 2.0.0 has no classic CLI)

Server: `AssetRipper.GUI.Free.exe --headless --port 8123 --log-path "<repo>/tmp/assetripper/ar.log"` (quote the log path - the repo path contains a space; an unquoted `Start-Process -ArgumentList` array produced "Too many arguments were supplied"). Content root = the process working dir; settings persist to `tmp/AssetRipperCLI/AssetRipper.Settings.json` (copy in `build/export-logs/`), default export root when `CreateSubfolder=true` is `<exe dir>/Ripped`.

| endpoint | payload used | result |
|---|---|---|
| `GET /openapi.json` | - | the machine-readable route list (`/swagger/index.html` serves the Swagger UI; `/docs.json` and `/swagger/swagger.json` 404) |

| `POST /Settings/Update` | form fields (`application/x-www-form-urlencoded`); selects sent by their option key, every checkbox that must be ON is *present* with an empty value | 200; re-read `GET /Settings/Edit` to confirm |
| `POST /LoadFolder` | `Path=\\MYAPPSANDBOX\c\Games\MFG\game\MFGClient_Data` | 302->200, 938 s then 878 s; works without a game exe |
| `POST /Export/UnityProject` | `Path=<out dir>&CreateSubfolder=false` | 302->200 in **308 s**, writes straight to disk (no zip) |
| `POST /Export/PrimaryContent` | same shape | 200 in 407 s (different file format, see §4) |
| `POST /Reset` | - | drops the loaded collection |
| `GET /IO/{File,Directory}/Exists`, `GET /IO/Directory/Empty` | `?Path=` | what the Vue export form uses to gate the submit button |

Gotchas for the editor phase: `GET /Scenes/View`, `/Collections/View`, `/Collections/Count`, `/Assets/View`, `/Bundles/View`, `/FailedFiles/View` all **404** when hit directly (they need state/params the generated spec does not describe), and `/Search/View` only renders a form. So *all* statistics below were computed from the export tree on disk, not from the UI. Second gotcha measured with a deliberately bad path: `POST /LoadFolder` with a nonexistent `Path` answers **200 with an HTML error page** (the server logs an unhandled `LoadFolder` stack trace) instead of 4xx - so trust the log/collection, never the status code. Verified again afterwards: an empty-collection `Export/UnityProject` over an existing export writes nothing back, so the real export survives such a mistake.


Settings used (persisted copy: `build/export-logs/AssetRipper.Settings.json`): `DefaultVersion=TargetVersion=2022.3.13f1`, `StreamingAssetsMode=Ignore` (`IgnoreStreamingAssets` checked), `BundledAssetsExportMode=DirectExport`, `ScriptContentLevel=Level2`, `ScriptExportMode=Hybrid`, `ScriptLanguageVersion=AutoSafe`, `ImageExportFormat=Png` + `PreferOriginalTextureExtension`, `AudioExportFormat=Default`, `ShaderExportMode=Dummy` (decompilation is a paid feature), `SpriteExportMode=Yaml`, `LightmapTextureExportFormat=Yaml`, `TextExportMode=Parse`, `EnablePrefabOutlining=true`.

## 3. Rerun recipe

```powershell
# full project export -> build/unity-project   (starts its own server, kills only its own PID)
pwsh -File scripts/extract_content.ps1

# the JSON/GLB + full decompiled-solution variant -> build/export-primary-content
pwsh -File scripts/extract_content.ps1 -Kind PrimaryContent -OutDir `
  '<repo>\build\export-primary-content' -Port 8124
```

Same steps by hand (`Invoke-WebRequest`, no script). `<repo>` = path to this repo checkout:

```powershell
$dir='<repo>\tmp\assetripper'
$p = Start-Process '<repo>\tmp\AssetRipperCLI\AssetRipper.GUI.Free.exe' `
  -ArgumentList "--headless --port 8123 --log-path `"$dir\ar.log`"" -WorkingDirectory $dir -PassThru -WindowStyle Hidden
Invoke-WebRequest http://127.0.0.1:8123/Settings/Update -Method Post -Body @{ DefaultVersion='2022.3.13f1'; TargetVersion='2022.3.13f1'; BundledAssetsExportMode='DirectExport'; ScriptContentLevel='Level2'; AudioExportFormat='Default'; ImageExportFormat='Png'; LightmapTextureExportFormat='Yaml'; SpriteExportMode='Yaml'; ShaderExportMode='Dummy'; TextExportMode='Parse'; ScriptLanguageVersion='AutoSafe'; ScriptExportMode='Hybrid'; PreferOriginalTextureExtension=''; SaveSettingsToDisk=''; IgnoreStreamingAssets=''; EnablePrefabOutlining='' }
Invoke-WebRequest http://127.0.0.1:8123/LoadFolder -Method Post -Body @{ Path='\\MYAPPSANDBOX\c\Games\MFG\game\MFGClient_Data' } -TimeoutSec 3600
Invoke-WebRequest http://127.0.0.1:8123/Export/UnityProject -Method Post -Body @{ Path='<repo>\build\unity-project'; CreateSubfolder='false' } -TimeoutSec 5400
Stop-Process -Id $p.Id -Force          # PID only - never Stop-Process by name (the harness runs on node)
```

Streaming files (videos + addressables metadata) are not AssetRipper's job; they were staged then dropped into the project:

```powershell
robocopy '\\MYAPPSANDBOX\c\Games\MFG\game\MFGClient_Data\StreamingAssets\Characters' "$stage\Characters" /E /NFL /NDL /NJH /NP
robocopy '\\MYAPPSANDBOX\c\Games\MFG\game\MFGClient_Data\StreamingAssets\Yakuman'    "$stage\Yakuman"    /E /NFL /NDL /NJH /NP
Copy-Item '\\MYAPPSANDBOX\c\Games\MFG\game\MFGClient_Data\StreamingAssets\aa\catalog.json'  "$stage\aa\catalog.json"
Copy-Item '\\MYAPPSANDBOX\c\Games\MFG\game\MFGClient_Data\StreamingAssets\aa\settings.json' "$stage\aa\settings.json"
robocopy $stage 'build\unity-project\Assets\StreamingAssets' /E /NFL /NDL /NJH /NP      # 136 files, 1,304.5 MB
# optional, if the shipped bundle packaging is wanted next to the project (4.85 GB):
robocopy '\\MYAPPSANDBOX\c\Games\MFG\game\MFGClient_Data\StreamingAssets\aa\StandaloneWindows64' `
  'build\unity-project\Assets\StreamingAssets\aa\StandaloneWindows64' /E /NFL /NDL /NJH /NP
```

Analysis scripts (all read-only against the install, all write inside the repo): `scripts/catalog_deep.py` (catalog shape/stats -> `build/catalog-stats.json`), `scripts/catalog_coverage.py` (catalog vs export tree coverage -> `build/catalog-coverage*.json`), `scripts/guid_integrity.py` (GUID/reference integrity + bundle-name cross-check), `scripts/extract_missing_unitypy.py` (UnityPy 1.25.4 fallback that fills the `.anim`/`.mat`/`.controller` gap when only the PrimaryContent variant exists; run against the project export it reports 0 gaps).

## 4. Outputs and stats

| folder | files | size | what it is |
|---|---|---|---|
| `build/unity-project/` | 118,269 | **11.87 GB** | editor-loadable project: `Assets/`, `ProjectSettings/`, `Packages/manifest.json`, + `Assets/StreamingAssets/` (videos + `aa/`) copied in |
| `build/unity-project-auxiliary/` | 132 | 0.04 GB | `AuxiliaryFiles`: `path_id_map.json` (8.47 MB, PathID->name/type per serialized file) + `GameAssemblies/` (131 shipped DLLs incl. `Assembly-CSharp.dll` 4.29 MB, `kamunity.dll`) |
| `build/export-primary-content/` | 93,656 | 8.43 GB | other export mode: per-asset **JSON dumps** + `.glb` mesh/scene previews + **full decompiled C#** (14,458 `.cs` incl. UnityEngine modules, 131 `.dll`, 131 `.csproj`, `.slnx`) |
| `build/export-logs/` | 4 | 12 MB | both AssetRipper logs, the served `openapi.json`, the exact settings JSON |

Nothing was committed. `.gitignore` already covers `tmp/` but **not** these new `build/` trees, so before any commit the owner will probably want: `build/unity-project/`, `build/unity-project-auxiliary/`, `build/export-primary-content/`, `build/export-logs/` (the repo has committed `build/` subfolders before, so this is a call for the owner, not made here).


`build/unity-project/Assets/` by type (files / MB; `.meta` excluded from the counts below where noted):

```
.meta 58,761 (64 MB)   .asset 19,994 (784)   .png 16,695 (5,337.7)   .ogg 10,363 (2,326.7)
.anim 4,371 (1,086)    .prefab 3,070 (866.7)  .mat 2,398 (12.4)      .unity 62 (325.8)
.controller 545 (16.6) .overrideController 57 .shader 40 (0.2, stubs) .cs 1,387 (5.9)
.dll 39 plugins (4.7)  .ttf 7 (14.2)          .csv 218 (3.1)         .webm 134 (1,259.7 staged)
.lighting 36  .playable 13  .rendertexture 33  .bytes 3  TMP: 130 .asset + 3 .shader
```

Top-level asset folders: `MFG/` (85,620 files, 8.10 GB - everything that had an original asset path: `MFG/Scenes`, `MFG/Characters/Character1..21`, `MFG/Addressables`, `MFG/Sound`, `MFG/Story`, ...), plus type-name folders for pathless built-ins (`Texture2D`, `AnimationClip`, `AnimatorController`, `Material`, `Mesh`, `Sprite`, `MonoBehaviour`, `GameObject`, `Avatar`, `Font`, `Cubemap`, `Shader(s)`, `TextMesh Pro`, `Resources`, `Scripts`, `Plugins`, `Editor`, `StreamingAssets`). `ProjectSettings/` = 16 `.asset` files rebuilt from `globalgamemanagers` (+ `ProjectVersion.txt: m_EditorVersion: 2022.3.13f1`); `EditorBuildSettings.asset` lists all **62 scenes**.


Export totals from the logs: UnityProject mode exported 58,007 asset objects, **0 error lines**, 1 warning; PrimaryContent mode exported 78,923 objects with 21 `Unable to read MonoBehaviour Structure` errors, all on `UnityEngine.Timeline.{Animation,Control,Audio}Track` ("layout mismatched binary content"), plus `Dependency '18cfe25740c82fe4cb6c273a72cceab9' wasn't found`. Timings: load 938 s / 878 s, exports 308 s / 407 s, peak private bytes ~17.7 GB on a 31.8 GB box (do not run two loads at once on this host).

## 5. GUID mapping and reference integrity

- GUIDs live in the **`.meta` files** next to every asset (`build/unity-project/Assets/**/*.meta`), plus `build/unity-project-auxiliary/path_id_map.json` for PathID provenance. Verified: **58,761 GUIDs, 0 duplicates**.
- Reference integrity, measured over all `.unity/.prefab/.asset/.mat/.anim/.controller/.overrideController/.playable/.lighting/.shader/.rendertexture` text: **377,324 guid references, 364,066 (96.49%) resolve to an exported `.meta` GUID**; the remaining 13,258 were classified and are **all Unity built-in pseudo-GUID references** (`0000000000000000e000000000000000` etc.: built-in meshes `fileID:10210`, legacy font `10102`, internal UI sprites `109xx`, default light cookie `10001`) - the editor resolves those natively, so the export is internally consistent.
- Script mapping is intact: **6,239 `m_Script` references across 1,935 YAML files, 438 distinct script GUIDs, 100% resolved** against `Assets/Scripts/Assembly-CSharp/*.cs.meta` and `Assets/Plugins/*.dll.meta`. No manual GUID remap is needed for asset->script references (docs/04 Phase 3 step 1's "audit missing script GUID mapping" is satisfied for these).

## 6. Addressables: what the export gives the rebuild

Catalog semantics (measured, not assumed): `m_InternalIds` holds **57,799 entries** = 28,292 bundle-file entries + 29,507 bundled-asset entries; asset ids are `<bundle-file>.bundle/<original asset path>` (some are bare `<asset path>`), the key/bucket/entry/extra payloads are base64 binary blobs (`m_KeyDataString`, `m_BucketDataString`, `m_EntryDataString`, `m_ExtraDataString`) - so the JSON catalog is *not* plain JSON data for the interesting parts. Declared `m_resourceTypes` (26) that matter: `Texture2D`, `Material`, `Sprite`, `SpriteAtlas`, `Mesh`, `Avatar`, `GameObject`, `RuntimeAnimatorController`, `AnimationClip`, `AnimatorOverrideController`, `AudioClip`, `Shader`, `RenderTexture`, `TextAsset`, `TMP_FontAsset`, `TMP_SpriteAsset`, `TimelineAsset` + Timeline tracks, `MFG.Assets.CharacterProperties`, `CharacterCommonProperties`.

- **Coverage**: `python scripts/catalog_coverage.py <catalog> build/unity-project` -> **29,404 / 29,409 present (99.98%)**. The 5 misses: `Assets/Shaders/DefaultResourcesExtra/{Standard,Normal-VertexLit,UI/UI-Default,Mobile/Mobile-Particle-Add}.shader` (built-in extras) and one name-collided `MFG/Fonts/DFG-PopMix-W5 CharaSelectNum1.mat`. Asset-type families all came out intact: 10,362 of the exported `.ogg` are exactly catalog `Sound` 9,306 + `BGM` 751 + `SFX` 305 (the 10,363rd lives in `resources.assets`), textures/icons/prefabs/meshes/faces, plus the classes AssetRipper skipped in the *other* mode (`anim` 4,371, `mat` 2,398, `controller` 545, `overrideController` 57).

- **Bundle membership survives**: 42,801 `.meta` files carry `assetBundleName: <32-hex>` covering **28,258 distinct bundle names** - that is the asset -> shipped-bundle join the regenerated addressable groups need. Caveat measured: those meta bundle-name GUIDs are **not** the shipped file names (0 of 28,292 file-name hashes match). Join by asset path instead: catalog entry `<bundle file>/<asset path>` <-> `build/unity-project/Assets/<asset path>.meta`.
- Per-character grouping (docs/04 Phase 3 requirement) is preserved structurally: the export tree keeps original paths, so `Assets/MFG/Characters/CharacterN/**` and `Assets/MFG/Sound/**` fold straight into addressable groups.
- `aa/settings.json` + `aa/catalog.json` now sit at `build/unity-project/Assets/StreamingAssets/aa/` so `Addressables.RuntimePath` resolves in-editor; regenerated content build still needed (see §7/§8).

## 7. What is still missing for an editor-loadable project

1. **Shader source**: `ShaderExportMode=Dummy` (decompilation is a Premium feature of GUI.Free) -> 40 `.shader` stubs that keep `Properties` but have a placeholder `SubShader` (`//DummyShaderTextExporter`), including Shader Graph assets (`sg_cha_outline` etc.). URP/ShaderGraph shaders must be re-authored, imported from source control of the art team, or recovered with the paid decompiler / HLSLc route.
2. **Render pipeline asset absent**: `ProjectSettings/GraphicsSettings.asset` has `m_CustomRenderPipeline: {fileID: 0}` and there is no URP pipeline/renderer asset anywhere in the export - the URP asset + renderer must be created and assigned, and URP is only present as `Unity.RenderPipelines.Core.Runtime.dll` (+ ShaderLibrary) plugin DLLs, not the Universal package.
3. **Packages**: `Packages/manifest.json` lists only built-in modules. Everything else came out as 39 managed plugin DLLs under `Assets/Plugins/` (`Unity.Addressables.dll`, `Unity.ResourceManager.dll`, `Unity.TextMeshPro.dll`, `Unity.Timeline.dll`, `UnityEngine.UI.dll`, `kamunity.dll`, `UniRx/UniTask*`, `DOTween`, `Springbone.Runtime`, `zxing.unity`, `VirtualSurround`, `Win10actLog-Runtime`, `Unity_userfully`, ProBuilder, ...). For editors and shader variants these should become matching UPM packages (Addressables **1.21.19** per settings.json, URP/TMP for 2022.3.13f1) instead of loose DLLs - otherwise expect duplicate-assembly and missing-editor-script errors.
4. **Game code**: only 1,387 `.cs` are in the project (asset-referenced types + plugin stubs). Full code is `Assembly-CSharp.dll` in `build/unity-project-auxiliary/GameAssemblies/` and its decompilation (14,458 `.cs` + solution) in `build/export-primary-content/Scripts|Assemblies`; wiring that into the project is the docs/19 compile work with platform calls routed to `IEaPlatform` (docs/10/11).
5. **Native plugins**: cabinet/audio natives are not part of a Mono project export (`libaio*`, `Dx11UnityGraphicPlugin`, `VirtualSurroundNative`, `avs2-*`) - stub or P/Invoke-shim them per the Phase 1 boundary.
6. **Shipped packaging**: the 28,292 `.bundle` files (4.85 GB) were not copied (command in §3); no `.spriteatlas` assets were exported (shipped atlases must be re-packed by the editor); lightmap textures exist but 36 `LightingDataAsset (.lighting)` files are the legacy built-in format and likely need a re-bake under URP; 21 Timeline track MonoBehaviours failed to deserialize in the PrimaryContent run (scene Timeline data may be degraded - recheck after import).
7. **Texture pipeline**: PNG total 5.34 GB (BC sources decoded losslessly); WebGL needs ASTC re-compression at build time (docs/04 Phase 3 step 3) and the first-load budget still has to be cut.

## 8. Next steps (Unity phase)

1. Open `build/unity-project` with Unity **2022.3.13f1** (exact match per `ProjectVersion.txt`), let it re-import; expect the first import to be long (11.9 GB, 118k files) and to complain about the DLL-plugins - replace them with the matching packages from item §7.3, then re-open.
2. Create/assign URP asset + renderer, restore the 40 stub shaders, then re-pack sprite atlases and re-bake lighting for the 62 scenes.
3. Rebuild Assembly-CSharp into the project from the recovered decompile (docs/19 tooling: `build/reftype*`, `build/asmdef-probe`) with `IEaPlatform` stubs in place of the arcade natives.
4. Regenerate addressable content: read `aa/catalog.json` + per-asset `assetBundleName` metas to rebuild groups per character/sound family (docs/04 Phase 3 step 2), keeping `Assets/StreamingAssets/Characters/*.webm` as streamed URLs for WebGL (Phase 4).
5. Re-run `python scripts/catalog_coverage.py` and `python scripts/guid_integrity.py` after each material change - they are the regression checks that extraction stayed complete.
