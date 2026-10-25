# 19 - Assembly compile probe (plain .NET, no Unity editor)

Question: does the recovered C# of the shipped game assemblies compile with nothing but the
dotnet SDK, or does it secretly depend on Unity-editor-only references?

Answer: it compiles. All eight recovered-source assemblies - `Assembly-CSharp` (1315 files,
the flagship) plus the seven plugin assemblies - build with **0 errors and 0 warnings**, in
Debug and Release, with `dotnet build` on SDK 8.0.402 and no Unity anywhere. Three recovered
files needed edits; everything else was settled with project configuration. `Assembly-CSharp`
references **no** editor-only assembly, so nothing on that axis blocks the rebuild.

The interesting part is *which* reference set makes it compile. The shipped assemblies link
Unity's Mono base class library, not Microsoft's .NET Framework 4.7.2 one, and the difference
is 69 error sites, all of them framework facts rather than code defects (§5, §6).

## 1. Environment

| | |
|---|---|
| Machine | Windows, no Unity editor or Unity license installed |
| Toolchain | `dotnet` SDK 8.0.402 (Roslyn from the SDK), msbuild targets only |
| Shipped assemblies | `\\MYAPPSANDBOX\c\Games\MFG\game\MFGClient_Data\Managed` - 129 dlls, Unity 2022.3.13f1 Mono player, **read-only, never written** |
| `MonoBleedingEdge` | ships `EmbedRuntime` only, no reference assemblies, so the BCL had to come from `Managed` |
| Framework dev packs | no .NET Framework targeting pack installed; `Microsoft.NETFramework.ReferenceAssemblies.net472` 1.0.3 present in the local NuGet cache (used read-only by one profile) |
| NuGet | no package is referenced or restored in the default profile |

## 2. What gets compiled

Recovered trees (`api\recovered\...`) and the projects that compile them
(`build\asmdef-probe\*.csproj`, SDK-style, one project per asmdef, recursive globs):

| Project (= assembly) | .cs compiled | notes |
|---|---|---|
| `Assembly-CSharp.csproj` | 1315 | flagship; SDK-style `<Compile Include="..\..\api\recovered\Assembly-CSharp\**\*.cs" />` |
| `Assembly-CSharp-firstpass.csproj` | 73 | |
| `kamunity.csproj` | 67 | depends on `zxing.unity` from `build\reftype` |
| `Shirahama.csproj` | 16 | |
| `Shirahama.Async.csproj` | 5 | `ProjectReference` -> Shirahama |
| `Shirahama.Rx.csproj` | 2 | |
| `VirtualSurround.csproj` | 7 | |
| `Win10actLog-Runtime.csproj` | 3 | |
| **Total** | **1488** | every recovered file is compiled; none excluded |

Assembly graph follows Unity's: `Assembly-CSharp` is built *after* firstpass / kamunity /
VirtualSurround / Win10actLog-Runtime as `ProjectReference`s, so the flagship is compiled
against the freshly rebuilt plugin assemblies, not against their shipped copies.
`-p:UseShippedPluginRefs=true` swaps the four for the shipped binaries in
`build\reftype-rebuilt` to isolate a plugin-tree failure from an Assembly-CSharp failure; both
modes are green. The plugin side is documented separately in
[PLUGINS-REPORT.md](../build/asmdef-probe/PLUGINS-REPORT.md).

Every tree also carries the two files Unity generates - `Properties\AssemblyInfo.cs` and
`UnitySourceGeneratedAssemblyMonoScriptTypes_v1.cs` (the MonoScript name/type byte table). Both
compile unchanged against the shipped `UnityEngine.dll`; the recovered `AssemblyInfo.cs` is the
reason `GenerateAssemblyInfo=false`. There is not a single `#if` in `api\recovered` (the export
was fully preprocessed), so no Unity defines are needed.

## 3. Reference sets

`build\reftype\copylist.txt` lists every dll copied out of the install, by folder, with sizes
and exclusions. Summary:

| Folder | Contents | Role |
|---|---|---|
| `build\reftype` | 100 dlls: `UnityEngine.dll` facade + 67 `UnityEngine.*Module`, 21 Unity package / FBX runtime, 11 middleware (`DOTween`, `UniRx`, `UniTask*`, `Springbone.Runtime`, `zxing.unity`, `UnityEngine.UI`, `Unity_userfully`, ...) | glob-referenced by every probe project |
| `build\reftype-bcl\.NETFramework\v4.7.2` | 21 dlls: the game's own Mono BCL (`mscorlib` 4.0.0.0 pkt `b77a5c561934e089`, `System`, `System.Core`, `System.Xml*`, `System.Numerics`, `netstandard` **2.1.0.0** pkt `cc7b13ffcd2ddd51`, ...) laid out as an MSBuild targeting pack with a generated `RedistList\FrameworkList.xml` | framework reference set for the default profile |
| `build\reftype-rebuilt` | shipped copies of the 7 source-recovered plugin assemblies + `netstandard.dll` | fallback for `-p:UseShippedPluginRefs=true` only |
| excluded | `Assembly-CSharp.dll` | the rebuild target; comparing against it happens after the build (§7) |

The targeting pack layout is the trick `Microsoft.NETFramework.ReferenceAssemblies` uses:
`TargetFrameworkRootPath` + `NoStdLib=true` + `EnableFrameworkPathOverride=false`, with
`FrameworkList.xml` written from the identities read out of the shipped dlls. Nothing from the
install was modified; everything is a copy.

### Profiles (`-p:ProbeProfile=...`, default `shipped`)

| Profile | TargetFramework | Framework reference set | NuGet |
|---|---|---|---|
| `shipped` | net472 | the game's Mono BCL as targeting pack | none |
| `pack` | net472 | Microsoft's net472 reference assemblies, read in place from the NuGet cache via `TargetFrameworkRootPath` | none (no PackageReference; cache path overridable with `-p:Net472RefPackRoot=`) |
| `netstandard` | netstandard2.1 | the game's Mono BCL as plain references | none |

Why three: the shipped `Assembly-CSharp.dll` carries **no** `TargetFrameworkAttribute` at all
(only `ExtensionAttribute`, `CompilationRelaxations`, `RuntimeCompatibility`, `Debuggable`), so
the moniker is a compile-time contract, not a shipped fact. What matters is which library the
code is compiled *against*. `shipped` keeps the requested net472 moniker and supplies the
library the shipped code really links; `pack` measures a stock Microsoft 4.7.2 reference set;
`netstandard2.1` shows the clean result does not depend on the moniker - and it is the level
Unity itself declares: the shipped `UnityEngine.dll` facade references `netstandard 2.1.0.0`.

## 4. Results

| Run | Result |
|---|---|
| `dotnet build Assembly-CSharp.csproj` (defaults: `shipped`, Debug, rebuilt plugin sources) | **exit 0, 0 errors, 0 warnings** |
| all 8 projects, `shipped`, Debug | 8 x exit 0, 0 errors, 0 warnings |
| all 8 projects, `shipped`, Release | 8 x exit 0, 0 errors, 0 warnings |
| flagship `shipped` with `-p:UseShippedPluginRefs=true` | exit 0, 0 errors, 0 warnings |
| all 8 projects, `netstandard` | 8 x exit 0, 0 errors, 0 warnings |
| 7 plugin projects, `pack` | exit 0, 0 errors, 0 warnings |
| flagship, `pack`, with the build-side DIM lowering (§6.2) | exit 1, **56 errors**, 0 warnings |
| flagship, `pack`, recovered DIM file kept (`-p:KeepNetFx472Dim=true`) | exit 1, **13 errors** (CS8701), 0 warnings |

Logs: `build\asmdef-probe\logs\build-<profile>-<config>[-extra]-<project>.log`.
Outputs: `build\asmdef-probe\bin\{Debug,Release}\net472\` (default profile). Rebuild
everything for a profile with `pwsh -NoProfile -File build\asmdef-probe\build-all.ps1
-Profile shipped` (wipes `obj`/`bin` per profile, since a profile changes the framework
reference set and the restore graph).

## 5. Edits to recovered sources: 3

Only decompiler artifacts were touched. Every one is a construct ilspy emitted that C# cannot
compile, in a shape where the intended meaning is forced by the surrounding code. 3 edits in
1488 files; no other file in `api\recovered` was modified (`git status` shows exactly these
three plus the new `build\` tree, nothing committed).

| # | File | Cause | Symptom |
|---|---|---|---|
| 1 | `Assembly-CSharp\MahjongPlayFlow.cs:640` | async state-machine decompilation left an unassigned dead local | CS0165 |
| 2 | `Assembly-CSharp\MFG.MasterGameData\FanMaster.cs:3016` | tuple element names propagated onto a nameless `ValueTuple` | CS1061 |
| 3 | `Assembly-CSharp\TestModeSkeleton\TestModeLocalization.cs:405` | `.ctor` rendered as the identifier `_002Ector` | CS0117 |

**(1) unassigned dead local.** ilspy emitted, inside `OnInitialize` (lines 615-692):

```csharp
int num;
_ = num - 3;
_ = 2;
```

`num` is never assigned, never used again in the method - the only other `num` in the file is
line 942, in a different method - and the two discarded expressions are stack leftovers of the
state-machine rewrite. The language forbids reading it (CS0165); the runtime cannot observe the
difference, because hoisted state-machine fields are zero-initialised and the result is dropped.
Fixed in place with the value the shipped IL would have seen:

```csharp
int num = 0;
_ = num - 3;
_ = 2;
```

**(2) invented tuple member names.** `source` at line 2972 is `(Player, int)[]`, a nameless
`ValueTuple<Player, int>` array. ilspy reused the *lambda* parameter names (`(Player player,
int index) p`) for a `var` iteration variable over that array:

```csharp
foreach (var item3 in source.Where(((Player player, int index) p) => p.player.m_cha != Cha.Jicha))
{
    int item = item3.index;   // CS1061: '(Player, int)' has no definition for 'index'
```

Only the lambda parameters carry the names, so the decompiler's `item3.index` never existed in
the IL. `Item2` is the field the name stands for:

```csharp
    int item = item3.Item2;
```

**(3) `_002Ector`.** The constructor of `TestModeLocalization : SingletonBehaviour<TestModeLocalization>`
ends with `base._002Ector();` - ilspy's `\u002E` escape of `.ctor` when it failed to fold the
base-class constructor call into the initializer list. C# issues that call implicitly in every
constructor, so the line is deleted (left as a comment). It is the only occurrence of
`_002Ector` anywhere in `api\recovered`.

Nothing else needed a source edit. The 8 `CS1998` warnings in firstpass `USnd\AudioManager.cs`
(async without await) are state-machine decompilation noise and were left alone - listed in
`NoWarn` rather than rewritten (§6.1). No `api\recovered` file was deleted, reordered or
excluded from compilation.

## 6. Settled with build configuration instead of source edits

### 6.1 Project-level settings (`build\asmdef-probe\Directory.Build.props`)

| Setting | Why |
|---|---|
| `LangVersion=10` | ilspy emits file-scoped namespaces (`namespace X;`), a C# 10 syntax feature: CS8773 under C# 9 in 63 sites of one file. Semantics are identical to the braced form Unity compiled; raising the language version beats rewriting 1300 files. (`api\recovered\*\*.csproj` exports by ilspy itself declare 11.0.) |
| `AllowUnsafeBlocks=true` | recovered code uses `unsafe` (e.g. kamunity heap marshalling) |
| `GenerateAssemblyInfo=false` | each tree ships its own recovered `Properties\AssemblyInfo.cs`; otherwise duplicate `AssemblyVersion`/`AssemblyTitle` attributes |
| `EnableDefaultItems=false`, `EnableDefaultCompileItems=false` + explicit recursive globs | keep `obj\` generated files out of the compile and make the compiled file set auditable (counted per profile with `dotnet msbuild -getItem:Compile`) |
| `ProduceReferenceAssembly=false`, `AutoGenerateBindingRedirects=false`, `Deterministic=true` | no ref-assemblies wanted for the metadata diff; no app.config semantics in a library probe |
| `NoWarn=1701;1702;0436;0435;0105;0108;0114;0169;0414;0649;1591;1998` | reference-identity and forwarding noise from linking the game's own BCL (1701/1702/0436/0435), decompiler-emitted hidden/duplicated inherited members and unused fields (0105/0108/0114/0169/0414/0649), missing doc comments (1591), state-machine async-without-await (1998). All warnings; **no error is silenced** - that matters for §6.2 |
| profile machinery (`TargetFrameworkRootPath`, `NoStdLib`, `EnableFrameworkPathOverride`, `DisableImplicitFrameworkReferences`, `AutomaticallyUseReferenceAssemblyPackages`) | the only way to point a net472 build at a chosen reference set when no developer pack is installed |

### 6.2 Build-side compat, `pack` profile only (`shims\`, `compat\`)

The `shipped` and `netstandard` profiles need **no** compat file at all - they bind the real
BCL members. The Microsoft net472 reference set does not have them, so three build-side
artifacts exist, all conditioned on `'$(ProbeProfile)' == 'pack'`, none inside
`api\recovered`:

| File | What it is | Why it is not a source fix |
|---|---|---|
| `shims\MathFShim.cs` | internal global `MathF` with `static readonly float PI, E` (fields, not constants, so the emitted IL keeps the float32 runtime load instead of folding with doubles) | `System.MathF` exists in the shipped Mono `mscorlib`; absence is a property of the reference set, not of the code |
| `shims\KeyValuePairCreateShim.cs` | internal `Shirahama.KeyValuePair.Create` - enclosing-namespace member that shadows the imported generic struct only inside `namespace Shirahama` | same reason; scoped so nothing else changes binding |
| `compat\pack\IPlayerInteractor.cs` | the recovered interface's 30 abstract members unchanged + its 13 default bodies re-expressed as extension methods on `IPlayerInteractor` | the .NET Framework 4.7.2 runtime cannot host interface method bodies, so the *target* rejects valid shipped IL; lowering is a target capability, not a bug fix |

The lowering is safe here, and that was checked rather than assumed: in the shipped assembly
`MFG.MahjongPlay.Cutin.IPlayerInteractor` declares 43 methods, **13 of which carry IL bodies**,
and neither implementer (`PlayerInteractor`, 34 methods; `PreviewPlayerInteractor`, 33) declares
any of those 13 names, so there is no override to break; every call site is a direct
invocation on a static receiver typed `IPlayerInteractor` (34 files checked; the only other
`IsFakeClear` declarations in the tree are `MFG.Types.Player.IsFakeClear` - a property on a
different type - and `CharaTypeExtentions.IsFakeClear(this CharaType, int)`).

## 7. What a stock net472 reference set refuses, and why that is not the code's fault

Flagship under `pack`: 56 error sites, zero warnings, none of them about Unity:

| Count | Diagnostic | Missing member | Sites (first of each group) |
|---|---|---|---|
| 25 | CS1503 | `string.Split(string)` / `string.Join(char, IEnumerable<T>)` overloads | `GameEventData.cs:113`, `MahjongPlayFlow.cs:1061`, `BingoEventRemakeGameData.cs:449`, `CardConnectGameData.cs:41`, `EventTakuGameData.cs:150` |
| 16 | CS0308 | generic `Enum.Parse<T>(string)` | `MahjongPlayDebug.cs:400`, `CharaGameData.cs:204`, `Gread2EventResultInfo.cs:17`, `VoiceMaster.cs:2361` |
| 7 | CS0117 | `Math.Clamp` | `BingoEventDisp.cs:40`, `CompetitionRankGraph.cs:63`, `PlayerGameData.cs:477`, `StoryPlayFlow.cs:403`, `VoiceManager.cs:470` |
| 6 | CS1061 | `Dictionary<,>.TryAdd`, `Enumerable.TakeLast` | `LoadableAssetSet.cs:46`, `PlayerRecordInfo.cs:47`, `GameSettings.cs:103`, `VoiceManager.cs:568` |
| 2 | CS0518 | predefined types `System.Range` / `System.Index` for `text[..n]` | `TestModeLocalization.cs:89` |
| 13 | CS8701 | default interface implementations, when the recovered `IPlayerInteractor.cs` is compiled (`.NET Framework 4.7.2` cannot host interface method bodies; `NoWarn` cannot suppress an error) | `MFG.MahjongPlay.Cutin\IPlayerInteractor.cs`, 13 declaration sites |

Zero `CS0246`/`CS0234` against `UnityEngine*` or any plugin: in every profile every Unity and
middleware type resolves. The residual set is entirely "this BCL is older than the one the game
ships". `Math.Clamp`, `Enum.Parse<T>`, `Dictionary.TryAdd`, `Enumerable.TakeLast`,
`string.Split(string)`, `string.Join(char, IEnumerable)`, `System.Range/Index`, `System.MathF`
are .NET Core 2.0 / .NET Standard 2.1 era additions - which is exactly what the shipped
`netstandard.dll 2.1.0.0` in `build\reftype-bcl` provides, and why the `shipped` and
`netstandard` profiles compile the same sources unchanged.

One measurement quirk worth recording: with the recovered DIM file compiled in `pack` mode the
build reports 13 errors, not 69. Roslyn skips method-body binding once declaration-level errors
exist, so the 13 CS8701 hide the 56 body-level API gaps. Both variants were run; 69 is the
union, and the two logs are kept.

Two things were tried and do not work, so they are not in the design: `<NoWarn>8701</NoWarn>`
(CS8701 is an error; 13 errors persisted) and `TargetFrameworkRootPath` against a synthetic
pack layout that omitted the generated `RedistList\FrameworkList.xml` (MSB3644: reference
assemblies not found - the generated FrameworkList is what makes the shipped-BCL pack resolve).

## 8. Fidelity of the recovered code, measured against the shipped assemblies

`build\tools\asmmeta` (net8.0, `System.Reflection.Metadata` only, no execution) compares a
rebuild with the shipped dll: `tfm` (identity, assembly attributes, reference list), `members`
(counts), `dim` (which declared methods carry IL bodies), `diff` (three member-set projections:
hand-written members with signatures, public/protected hand-written, and everything including
compiler-generated names; nested-type names are qualified through `GetDeclaringType`, otherwise
unrelated closure classes collide).

`Assembly-CSharp`, rebuild from the default profile vs the shipped dll:

| Measure | shipped | rebuilt |
|---|---|---|
| assembly identity | `Assembly-CSharp 0.0.0.0`, no `TargetFrameworkAttribute` | `Assembly-CSharp 0.0.0.0`, same attribute set |
| assembly references | 35 | 35 - **identical names and versions** |
| hand-written members (every member of a non-generated type, signatures included) | 26736 | 26736 - **0 differences** |
| public/protected hand-written members | 18550 | 18550 - 0 differences |
| all members incl. compiler-generated (Debug) | 44811 | 49187 (5811 only-shipped / 10187 only-rebuilt) |
| all members incl. compiler-generated (Release) | 44811 | 44800 (5426 / 5415) |
| `IPlayerInteractor` methods carrying IL bodies | 13 of 43 | 13 of 43 |
| file size | 4,291,072 | 4,688,384 Debug / 4,288,000 Release |

The same check for the plugin assemblies (Debug, default profile): hand-written surface
identical for all eight (firstpass 1832, kamunity 3075, Shirahama 96, Shirahama.Async 23,
Shirahama.Rx 7, VirtualSurround 106, Win10actLog 11 - zero differences in each), and the full
member sets are identical for kamunity, Shirahama.Async, Shirahama.Rx, VirtualSurround and
Win10actLog-Runtime. The residual differences are confined to compiler-generated names:
closures, `<>c__DisplayClassN`, async state machines and their hoisted fields, which the
decompile-then-recompile round trip necessarily regenerates differently (and which shrink
by half in Release, where the shipped assembly was built: flagship 5811/10187 Debug vs 5426/5415
Release, firstpass 209/257 vs 210/209, `Shirahama` 12/13).

Reading of these numbers: every hand-written name, signature, access level, attribute set and
inheritance relationship in the shipped assemblies is reproduced by the recovered source,
including private and `[SerializeField]` members and the 13 interface bodies; what differs is
machine-generated plumbing that the recovered source never contained. That is the strongest
statement available short of running the game.

## 9. Blockers

For compilation: none. No recovered assembly references anything that only a Unity editor
supplies - the compile-time surface is `UnityEngine*` + shipped middleware + the shipped BCL,
all available from the install. Nothing had to be excluded, stubbed or wrapped.

Standing caveats, stated as limits rather than failures:

- This proves the recovered source compiles to the same assembly contract. It does not execute
  it; behaviour parity still rests on the runtime-evidence work in `docs\`.
- `pack` mode is a measurement, not a shipping target: 69 latent error sites are framework
  facts about a BCL older than the one the game carries. If the rebuild ever has to run on
  stock .NET Framework 4.7.2 instead of Unity's Mono, those APIs (and the 13 default interface
  bodies) are the migration surface.
- The default profile deliberately reuses the game's own Mono BCL as framework assemblies. The
  output is a plain .NET assembly (same 35 references as the shipped one) but it is only
  loadable by a runtime that carries that BCL - Unity's Mono, or any runtime providing the same
  identities. `netstandard2.1` mode produces the same thing through a different door.
- Per-profile `obj\`/`bin\` must be wiped between profiles: a profile changes the framework
  reference set and the restore graph, and a reused `obj\` reports stale results (this produced
  one spurious `MSB3644` during the probe before `build-all.ps1` started wiping per run).

## 10. Reproduce

```powershell
Set-Location '<repo>\build\asmdef-probe'

# flagship, defaults (shipped profile, Debug): exit 0, 0 errors, 0 warnings
dotnet build Assembly-CSharp.csproj

# whole graph per profile, with logs, obj/bin wiped per profile
pwsh -NoProfile -File .\build-all.ps1 -Profile shipped
pwsh -NoProfile -File .\build-all.ps1 -Profile shipped -Configuration Release
pwsh -NoProfile -File .\build-all.ps1 -Profile netstandard
pwsh -NoProfile -File .\build-all.ps1 -Profile pack              # flagship 56 errors
pwsh -NoProfile -File .\build-all.ps1 -Profile pack -Projects Assembly-CSharp.csproj `
    -Extra '-p:KeepNetFx472Dim=true' -NoClean                    # flagship 13 CS8701
pwsh -NoProfile -File .\build-all.ps1 -Profile shipped -Projects Assembly-CSharp.csproj `
    -Extra '-p:UseShippedPluginRefs=true' -NoClean                # isolation mode, green

# metadata comparison against the shipped dlls
$asmmeta = 'build\tools\asmmeta\bin\Debug\net8.0\asmmeta.exe'
dotnet build build\tools\asmmeta\asmmeta.csproj
& $asmmeta tfm    build\asmdef-probe\bin\Debug\net472\Assembly-CSharp.dll
& $asmmeta dim    build\asmdef-probe\bin\Debug\net472\Assembly-CSharp.dll 'Cutin\.IPlayerInteractor$'
& $asmmeta diff   tmp\refinspect\Assembly-CSharp.dll build\asmdef-probe\bin\Debug\net472\Assembly-CSharp.dll
pwsh -NoProfile -File build\asmdef-probe\make-copylist.ps1        # regenerate copylist.txt
```

## 11. Paths

Created:

- `build\asmdef-probe\Directory.Build.props` - shared settings + the three profiles
- `build\asmdef-probe\Assembly-CSharp.csproj`, `Assembly-CSharp-firstpass.csproj`,
  `kamunity.csproj`, `Shirahama.csproj`, `Shirahama.Async.csproj`, `Shirahama.Rx.csproj`,
  `VirtualSurround.csproj`, `Win10actLog-Runtime.csproj`
- `build\asmdef-probe\shims\MathFShim.cs`, `shims\KeyValuePairCreateShim.cs`
- `build\asmdef-probe\compat\pack\IPlayerInteractor.cs`
- `build\asmdef-probe\build-all.ps1`, `build\asmdef-probe\make-copylist.ps1`
- `build\asmdef-probe\logs\build-<profile>-<config>[-extra]-<project>.log`,
  `build\asmdef-probe\bin\{Debug,Release}\net472\*.dll`, `build\asmdef-probe\obj\`
- `build\reftype\*.dll` (100) + `build\reftype\copylist.txt`
- `build\reftype-bcl\.NETFramework\v4.7.2\*.dll` (21) + `RedistList\FrameworkList.xml`
- `build\reftype-rebuilt\*.dll` (8, fallback only)
- `build\tools\asmmeta\asmmeta.csproj`, `build\tools\asmmeta\Program.cs`
- `docs\19-assembly-compile.md` (this file)
- kept from the plugin pass: `build\asmdef-probe\PLUGINS-REPORT.md` and its logs

Modified (3 files, nothing committed):

- `api\recovered\Assembly-CSharp\MahjongPlayFlow.cs`
- `api\recovered\Assembly-CSharp\MFG.MasterGameData\FanMaster.cs`
- `api\recovered\Assembly-CSharp\TestModeSkeleton\TestModeLocalization.cs`

Read-only inputs, unmodified: `\\MYAPPSANDBOX\c\Games\MFG` (game install),
`api\recovered\*` apart from the three files above, `%USERPROFILE%\.nuget\packages\...`
(net472 targeting assemblies read in place by the `pack` profile only).

## 2026-10-25 recheck: the project-copy build itself

The merged project copy (what Unity will actually compile; namespace-flattened variants of the
recovered trees, all six plugin trees merged into Assembly-CSharp) was rebuilt through
`build/project-merge-analysis/verify/ac9` + `firstpass9` and diffed against the shipped DLLs
with asmmeta:

- firstpass: 1832/1832 hand-written members identical (public/protected 1407/1407), 0 missing,
  0 extra; only compiler-generated closure names differ.
- merged Assembly-CSharp: every shipped hand-written member present. The 38 nominal
  "shipped-only" entries are all the same member with a type-qualification artifact (e.g.
  `GetCardType(CardType)` vs `GetCardType(KAMUNITY.AvsTypes/CardType)`): plugin types now live in
  the merged assembly so the signature prints unqualified. Left-only entries are the six plugin
  trees` members (kamunity et al.) landing in Assembly-CSharp by design, plus compiler-generated
  anonymous types/state machines.

Unity-facing consequence: no signature drift between what the extracted prefabs/scenes reference
(type names unchanged) and what the project compiles.

