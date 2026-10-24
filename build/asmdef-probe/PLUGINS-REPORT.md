# Plugin-assembly compile probe — findings

Scope: the seven source-recovered plugin assemblies built as plain .NET (net472) with
`dotnet build <proj>.csproj -nologo -v:q -clp:ErrorsOnly` from `build\asmdef-probe`.
Assembly-CSharp (flagship) excluded. Unity 2022.3.13f1 Mono target emulated via the
shared `Directory.Build.props` (net472 + shipped dll wildcard refs); no file under
`Directory.Build.props` or `api\recovered` was modified. Nothing committed to git.

## Verdict

**All seven plugin assemblies compile clean: 0 errors, 0 blockers. Zero recovered-source edits.**

Every fix was csproj-level config or an internal compile-only shim file. Outputs land in the
shared `build\asmdef-probe\bin\Debug\net472\` (preconfigured common output dir): all seven
DLLs produced (`Shirahama.dll`, `Shirahama.Async.dll`, `Shirahama.Rx.dll`, `VirtualSurround.dll`,
`Win10actLog-Runtime.dll`, `kamunity.dll`, `Assembly-CSharp-firstpass.dll`).

## Per project

| Project | Files compiled | Final build result | LangVersion | Settings / additions |
|---|---|---|---|---|
| Shirahama | 16 | `Build succeeded. 0 Warning(s) 0 Error(s)` | **10** (bumped from 9) | — |
| Shirahama.Async | 5 + 1 shim | `Build succeeded. 0 Warning(s) 0 Error(s)` | **10** (bumped from 9) | `shims\KeyValuePairCreateShim.cs` Compile-Include |
| Shirahama.Rx | 2 | `Build succeeded. 0 Warning(s) 0 Error(s)` | 9 (inherited, no bump needed) | — |
| VirtualSurround | 7 | `Build succeeded. 0 Warning(s) 0 Error(s)` | **10** (bumped from 9) | — |
| Win10actLog-Runtime | 3 | `Build succeeded. 0 Warning(s) 0 Error(s)` | **10** (bumped from 9) | — |
| kamunity | 67 | `Build succeeded. 0 Warning(s) 0 Error(s)` | **10** (bumped from 9) | — |
| Assembly-CSharp-firstpass | 73 + 1 shim | `Build succeeded. 0 Warning(s) 0 Error(s)` | **10** (bumped from 9) | `shims\MathFShim.cs` Compile-Include |

No project needed `NoWarn` additions (CS8701 never fired on these seven — no default
interface implementations in their recovered source), nor `AllowUnsafeBlocks` changes
(already inherited true), nor `System.ValueTuple` NuGet.

## Errors seen, and how each was cleared

1. **CS8773 “file-scoped namespace not available in C# 9”** — Shirahama (14 files),
   Shirahama.Async (3), VirtualSurround (5), Win10actLog-Runtime (1), kamunity (40+),
   firstpass (40+). Cause class: decompiler emits `namespace X;` (C# 10 syntax).
   Fix: `<LangVersion>10</LangVersion>` in each affected .csproj (commented). Shirahama.Rx
   is the only tree without file-scoped namespaces (its 2 files use block namespaces).
2. **CS0305 at `KeyValuePair.Create(e.Key, e.Hash)`** — Shirahama.Async
   [BundleHashCacheSupport.cs:36](../../api/recovered/Shirahama.Async/Shirahama/BundleHashCacheSupport.cs#L36).
   The shipped Mono runtime (Unity 2022.3 fork) carries the .NET Standard 2.1 helper
   `System.Collections.Generic.KeyValuePair.Create<TKey,TValue>`; net472 reference
   assemblies do not. Fix: shim file (below). Zero call-site edits; only 1 such call
   exists in all seven trees.
3. **CS0103 `MathF` (×2)** — firstpass
   [AudioDefine.cs:496](../../api/recovered/Assembly-CSharp-firstpass/USnd/AudioDefine.cs#L496)
   and :1282 (`MathF.PI * 41f / 202f`, `MathF.E * 41f / 111f` in two big float tables).
   `System.MathF` is .NET Core 2.1+ BCL shipped by Unity's Mono fork, missing from net472
   refs. Fix: shim file (below). Same class of issue will hit the flagship
   (`MathF.PI` at [DefaultDebugTapper.cs:21](../../api/recovered/Assembly-CSharp/MFG/DefaultDebugTapper.cs#L21), :57 — other agent).

## Source edits

**None.** All seven trees compiled as-decompiled. No semantic or decompiler-artifact edits
were required.

## Shim files (compile-only, internal — not part of recovered source)

- [`shims\KeyValuePairCreateShim.cs`](shims/KeyValuePairCreateShim.cs) — internal static class
  `Shirahama.KeyValuePair` with generic `Create<TKey,TValue>` returning the BCL struct.
  Enclosing-namespace members outrank `using System.Collections.Generic;` imports, so
  `KeyValuePair.Create(...)` binds to the shim inside `namespace Shirahama` only; the BCL
  generic struct is untouched everywhere else. Included by Shirahama.Async.csproj.
- [`shims\MathFShim.cs`](shims/MathFShim.cs) — internal global static class `MathF` with
  `static readonly float PI/E` (field shape, not `const`, so the compiler still emits the
  runtime float32 load/mul/div exactly as the shipped IL does instead of double-intermediate
  constant folding). Included by Assembly-CSharp-firstpass.csproj.

Both mimic APIs the shipped Mono runtime actually has; they add only internal types to the
rebuilt assemblies and change no public API.

## Warnings (non-blocking, recorded for the record)

Only firstpass emits warnings: **CS1998 ×8**, all in
[AudioManager.cs](../../api/recovered/Assembly-CSharp-firstpass/USnd/AudioManager.cs)
(2934/2943/3030/3038/3046/3251/3275/3283) — decompiled `async` methods whose bodies lost
their `await` in state-machine reconstruction. Left as-is (changing them would alter
`AsyncMethodBuilder` shape, i.e. semantics). All other projects: 0 warnings.

## Fidelity spot-check

Loaded the shipped `build\reftype-rebuilt\Shirahama.Async.dll` via reflection: its type list
(`Shirahama.BundleHashCacheSupport`, `Shirahama.Chiper`, `Shirahama.DownloadProgress` +
compiler-generated nests) matches the recovered source set 1:1.

## Artifacts created

- Edited (csproj config only): `Shirahama.csproj`, `Shirahama.Async.csproj`,
  `VirtualSurround.csproj`, `Win10actLog-Runtime.csproj`, `kamunity.csproj`,
  `Assembly-CSharp-firstpass.csproj` (LangVersion 10; the last two also Compile-Include shims).
  `Shirahama.Rx.csproj` untouched.
- New: `shims\KeyValuePairCreateShim.cs`, `shims\MathFShim.cs`,
  `logs\<name>.log` ×7 (final clean builds, `ErrorsOnly`), `logs\<name>.warnings.log` ×7
  (forced-full-recompile `WarningsOnly` passes), this report.
- Output assemblies: `bin\Debug\net472\<name>.dll` (+ `.pdb`) for all seven.
- `Directory.Build.props`, `api\recovered\**`, `reftype\`, `reftype-rebuilt\`,
  `Assembly-CSharp.csproj` untouched. Nothing staged/committed.
