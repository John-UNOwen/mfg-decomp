# prepare_project.ps1 - reorganize build/unity-project so Unity 2022.3.13f1 can open it
# with the recovered source trees compiled as Unity's predefined assemblies, and play mode
# able to boot the game from the shipped addressable bundles. See docs/21-project-merge.md
# for the reasoning; this script performs decisions 1-3 idempotently.
#
#   1. api/recovered/Assembly-CSharp            -> Assets/Assembly-CSharp              (Assembly-CSharp)
#      api/recovered/Assembly-CSharp-firstpass  -> Assets/Plugins/Assembly-CSharp-firstpass (firstpass)
#      kamunity / Shirahama / Shirahama.Async / Shirahama.Rx / VirtualSurround /
#      Win10actLog-Runtime                      -> Assets/<tree>                       (merged into Assembly-CSharp)
#      - project copies get file-scoped namespaces converted to braced form so Unity's
#        C# 9 compiles them (api/recovered itself is never modified);
#      - merged plugin trees drop Properties/AssemblyInfo.cs and
#        UnitySourceGeneratedAssemblyMonoScriptTypes_v1.cs (assembly attributes / generated
#        type table belong to one assembly, Unity generates the latter);
#      - script GUID metas AssetRipper wrote for asset-referenced types are carried onto
#        the recovered .cs files (scenes/prefabs reference scripts by guid + type).
#   2. loose dlls in Assets/Plugins:
#      - deleted because their source is copied in (kamunity, Shirahama*, VirtualSurround,
#        Win10actLog-Runtime) or because a manifest-listed UPM package provides the same
#        assembly (Unity.Addressables, Unity.ResourceManager, Unity.ScriptableBuildPipeline,
#        Unity.AI.Navigation, Unity.Recorder(.Base), Unity.MemoryProfiler, Unity.Profiling.Core,
#        Unity.Formats.Fbx.Runtime, Autodesk.Fbx, FbxBuildTestAssets) or is BCL-provided
#        (System.Runtime.CompilerServices.Unsafe);
#      - kept (sourceless middleware + loose dlls that CARRY serialized script references,
#        see the census in build/project-merge-analysis/script_refs.json): DOTween, UniRx,
#        UniTask*, Springbone.Runtime, zxing.unity, Unity_userfully, Unity.TextMeshPro,
#        Unity.Timeline, UnityEngine.UI, Unity.RenderPipelines.Core.* (+ShaderGraph library),
#        Unity.ProBuilder* - and their .dll.meta Editor platform flags are switched to
#        enabled so the editor loads them (AssetRipper writes them editor-disabled).
#   3. original addressable bundles: robocopy install ...\StreamingAssets\aa ->
#      Assets/StreamingAssets/aa (catalog + settings + StandaloneWindows64/*.bundle;
#      play mode = shipped code's catalog-local mode, no group rebuild in v1).
#   4. Packages/manifest.json gets the package entries; Assets/csc.rsp gets -unsafe
#      (Unity 2022 predefined assemblies have no UI toggle for unsafe code).
#
# Rerunning is safe and reproduces the same layout without destroying project work (guid map is
# persisted to build/project-merge-analysis/script_guid_map.json + kept export files snapshotted
# there). Phase 2 never deletes a client tree and never overwrites an existing project file whose
# content differs from the pristine copy of its api\recovered twin (namespace transform applied);
# such edited files - every in-place client fix (SoundManager.cs, CardEntryScene.cs, the gacha
# flows, ...) - and twinless hand-written files (RebuildJst.cs) are preserved and each one is
# printed and listed in prepare_report.json. -ResetClientCode opts back in to a pristine rebuild
# from api\recovered: it MOVES the current trees to build/project-merge-analysis/reset_backup/
# (undoable) and reverts every client fix that exists only in the project copies, including the
# hand-written RebuildJst.cs without which the recovered code dies on
# TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time") under the WebGL BCL. Export folders
# retired in phase 3 (Assets\Scripts, Assets\Plugins\<merged tree>) are moved to
# build/project-merge-analysis/export_discarded/ instead of being deleted.
# The game install share is only ever READ, never written; if it is unreachable the run must
# pass -SkipBundles, which also skips all other install staging (native plugins, videos).
# Nothing is committed to git.
#
# usage: powershell -ExecutionPolicy Bypass -File scripts\prepare_project.ps1 [-SkipBundles]
#          [-ResetClientCode]  (pristine rebuild of the client trees; see above - reverts fixes)
#        roots are overridable so fixtures can drive this script: -Repo -Proj -Recov -Ana -InstallShare

param(
  [string]$Repo = '',
  [switch]$SkipBundles,
  [switch]$ResetClientCode,
  [string]$Proj = '',
  [string]$Recov = '',
  [string]$Ana = '',
  [string]$InstallShare = '\\MYAPPSANDBOX\c\Games\MFG\game\MFGClient_Data'
)
$ErrorActionPreference = 'Stop'

if (-not $Repo) { $Repo = Split-Path -Parent $PSScriptRoot }
$proj     = if ($Proj) { $Proj } else { Join-Path $Repo 'build\unity-project' }
$recov    = if ($Recov) { $Recov } else { Join-Path $Repo 'api\recovered' }
$ana      = if ($Ana) { $Ana } else { Join-Path $Repo 'build\project-merge-analysis' }
$mapFile  = Join-Path $ana 'script_guid_map.json'
$keepDir  = Join-Path $ana 'kept_cs'
$install  = $InstallShare
$utf8NoBom = New-Object System.Text.UTF8Encoding($false)

if (-not (Test-Path $proj))  { throw "missing project: $proj" }
if (-not (Test-Path $recov)) { throw "missing recovered trees: $recov" }
if (-not (Test-Path $ana))   { New-Item -ItemType Directory -Path $ana -Force | Out-Null }
$skipInstall = -not (Test-Path -LiteralPath $install)
if ($skipInstall -and -not $SkipBundles) {
  throw "install share unreachable: $install - pass -SkipBundles to run without install staging (bundles, native plugins, videos)"
}

$pluginTrees = 'kamunity','Shirahama','Shirahama.Async','Shirahama.Rx','VirtualSurround','Win10actLog-Runtime'
# assembly key -> recovered tree, export location (AssetRipper), placement target
$treeMap = [ordered]@{
  'Assembly-CSharp'             = @{ recov='Assembly-CSharp';             export='Assets\Scripts\Assembly-CSharp';             target='Assets\Assembly-CSharp' }
  'Assembly-CSharp-firstpass'   = @{ recov='Assembly-CSharp-firstpass';   export='Assets\Plugins\Assembly-CSharp-firstpass';   target='Assets\Plugins\Assembly-CSharp-firstpass' }
}
foreach ($t in $pluginTrees) { $treeMap[$t] = @{ recov=$t; export=$null; target="Assets\$t" } }

$dllDeleteSource = 'kamunity','Shirahama','Shirahama.Async','Shirahama.Rx','VirtualSurround','Win10actLog-Runtime'
$dllDeletePackage = 'Unity.Addressables','Unity.ResourceManager','Unity.ScriptableBuildPipeline','Unity.AI.Navigation',
  'Unity.Recorder','Unity.Recorder.Base','Unity.MemoryProfiler','Unity.Profiling.Core','Unity.Formats.Fbx.Runtime',
  'Autodesk.Fbx','FbxBuildTestAssets'
$dllDeleteBcl = 'System.Runtime.CompilerServices.Unsafe'
$dllKeep = 'DOTween','Springbone.Runtime','UniRx','UniTask','UniTask.Addressables','UniTask.DOTween','UniTask.Linq',
  'UniTask.TextMeshPro','Unity.RenderPipelines.Core.Runtime','Unity.RenderPipelines.Core.ShaderLibrary',
  'Unity.RenderPipelines.ShaderGraph.ShaderGraphLibrary','Unity.TextMeshPro','Unity.Timeline','Unity_userfully',
  'UnityEngine.UI','Unity.ProBuilder','Unity.ProBuilder.Csg','Unity.ProBuilder.KdTree','Unity.ProBuilder.Poly2Tri',
  'Unity.ProBuilder.Stl','zxing.unity'
$manifestPackages = [ordered]@{
  # boot-critical only; vendored offline by scripts/vendor_packages.ps1 (embedded into Packages/)
  # Recorder/Formats.Fbx/MemoryProfiler/Profiling.Core stay dropped: their shipped dlls are deleted
  # above-to-package-equivalent and no recovered assembly references them; restore from build/reftype
  # if the serializer ever demands them. Timeline is NOT here: Unity.Timeline.dll stays loose (81 baked
  # GUID refs) and a package would double the assembly name.
  'com.unity.addressables'            = '1.21.19'
  'com.unity.scriptablebuildpipeline' = '1.21.21'
  'com.unity.ai.navigation'           = '1.1.5'
}

# ---------- helpers ----------

function Read-TextRaw([string]$path) { return [System.IO.File]::ReadAllText($path) }
function Write-TextRaw([string]$path, [string]$text) {
  [System.IO.File]::WriteAllText($path, $text, $utf8NoBom)
}
function Write-BytesRaw([string]$path, $bytes) { [System.IO.File]::WriteAllBytes($path, $bytes) }

$rxNamespace = [regex]'(?m)^[ \t]*(?:namespace\s+)([\w.]+)(?:[ \t]*;|[ \t]*\{?[ \t]*$)'
$rxTypeDecl  = [regex]'(?m)^[ \t]*(?:[\w \t]*\s)?\b(?:class|struct|interface|enum)\b\s+([A-Za-z_]\w*)'
$rxMetaGuid  = [regex]'(?m)^guid:\s*(\w{32})'

function Get-FileFqns([string]$path) {
  # returns @{ primary = <fqn matching file stem, else first declared>; all = [fqn...] }
  $text = Read-TextRaw $path
  $ns = ''
  $found = New-Object System.Collections.Generic.List[string]
  foreach ($line in ($text -split "\r?\n")) {
    $t = $line.Trim()
    if ($t.StartsWith('//')) { continue }
    $mn = $rxNamespace.Match($line)
    if ($mn.Success) { $ns = $mn.Groups[1].Value; continue }
    $mt = $rxTypeDecl.Match($line)
    if ($mt.Success) {
      $prefix = ''
      if ($ns) { $prefix = "$ns." }
      $found.Add($prefix + $mt.Groups[1].Value)
    }
  }
  $stem = [System.IO.Path]::GetFileNameWithoutExtension($path)
  $primary = ''
  foreach ($f in $found) { if ($f -like "*.$stem" -or $f -eq $stem) { $primary = $f; break } }
  if (-not $primary -and $found.Count -gt 0) { $primary = $found[0] }
  return [pscustomobject]@{ primary = $primary; all = @($found) }
}

function Convert-FileScopedText([string]$text, [string]$forPath) {
  # 'namespace X;' -> 'namespace X { ... }' (wrapped to EOF). At most one such line per file.
  # Returns the rewritten text, or $null when the text needs no rewrite.
  $rx = [regex]'(?m)^([ \t]*)(namespace\s+[\w.]+)[ \t]*;[ \t]*(\r?\n)'
  $ms = $rx.Matches($text)
  if ($ms.Count -eq 0) { return $null }
  if ($ms.Count -gt 1) { throw "file-scoped namespace x$($ms.Count) in $forPath" }
  $newText = $rx.Replace($text, [System.Text.RegularExpressions.MatchEvaluator]{ param($mm) $mm.Groups[1].Value + $mm.Groups[2].Value + ' {' + $mm.Groups[3].Value })
  return $newText + '}' + "`r`n"
}

function Transform-FileScoped([string]$path) {
  $newText = Convert-FileScopedText (Read-TextRaw $path) $path
  if ($null -eq $newText) { return $false }
  Write-TextRaw $path $newText
  return $true
}

function Get-MappedRel([string]$rel) {
  # project-side path of a recovered file: same Unity-magic folder renames the copies get
  # (a directory literally named Editor/Plugins would reassign assembly, so _Editor/_Plugins).
  return (($rel -split '\\') | ForEach-Object { if ($_ -eq 'Editor' -or $_ -eq 'Plugins') { "_$_" } else { $_ } }) -join '\'
}

function Test-ExcludedLeaf([string]$leafName, $patterns) {
  # robocopy /XF semantics for the plain name patterns used here (*.csproj, AssemblyInfo.cs, ...)
  foreach ($pat in $patterns) { if ($leafName -like $pat) { return $true } }
  return $false
}

function Move-ProjectTreeOut([string]$path, [string]$backupName) {
  # retire a folder from Assets by MOVING it under build\project-merge-analysis\export_discarded;
  # no unguarded recursive delete of a tree that could hold hand-fixed files. The destination
  # holds only what this function wrote in an earlier run, so replacing it is safe.
  $dest = Join-Path (Join-Path $ana 'export_discarded') $backupName
  $parent = Split-Path $dest -Parent
  if (-not (Test-Path -LiteralPath $parent)) { New-Item -ItemType Directory -Path $parent -Force | Out-Null }
  if (Test-Path -LiteralPath $dest) { Remove-Item -LiteralPath $dest -Recurse -Force }
  Move-Item -LiteralPath $path -Destination $dest -Force
}

function Invoke-Robocopy($roArgs) {
  $out = & robocopy @roArgs 2>&1
  $code = $LASTEXITCODE
  if ($code -ge 8) { throw "robocopy failed ($code): $($roArgs -join ' ')`n$out" }
  return @{ out = $out; code = $code }
}

# ---------- phase 1: guid map (export metas -> recovered files) ----------

$referencedGuids = @{}
$referencedDllGuids = @{}
$refsFile = Join-Path $ana 'script_refs.json'
if (Test-Path $refsFile) {
  $rj = Get-Content $refsFile -Raw | ConvertFrom-Json
  foreach ($p in $rj.cs_providers.PSObject.Properties) { $referencedGuids[$p.Name] = $true }
  foreach ($p in $rj.dll_providers.PSObject.Properties) { $referencedDllGuids[$p.Name] = $true }
}

$map = $null
if (Test-Path $mapFile) { $map = Get-Content $mapFile -Raw | ConvertFrom-Json }

$builtNow = $false
$acExport = Join-Path $proj $treeMap['Assembly-CSharp'].export
if (-not $map -and -not (Test-Path $acExport)) {
  throw "no guid map ($mapFile) and no AssetRipper export ($acExport). Re-export the UnityProject or restore the map."
}
if (-not $map) {
  Write-Host 'phase 1: building script guid map from AssetRipper export (first run)'
  $builtNow = $true
  $mapObj = [ordered]@{ builtAt = (Get-Date).ToString('s'); assemblies = [ordered]@{} }
  foreach ($asmName in $treeMap.Keys) {
    $info = $treeMap[$asmName]
    if (-not $info.export) { continue }
    $exportRoot = Join-Path $proj $info.export
    if (-not (Test-Path $exportRoot)) { continue }
    $recovRoot = Join-Path $recov $info.recov
    $index = @{}
    foreach ($f in (Get-ChildItem $recovRoot -Recurse -File -Filter *.cs)) {
      $p = Get-FileFqns $f.FullName
      if ($p.primary -and -not $index.ContainsKey($p.primary)) {
        $index[$p.primary] = $f.FullName.Substring($recovRoot.Length + 1)
      }
    }
    $carries = New-Object System.Collections.Generic.List[object]
    $keeps   = New-Object System.Collections.Generic.List[object]
    $dupes   = New-Object System.Collections.Generic.List[object]
    $orphans = New-Object System.Collections.Generic.List[object]
    $noMeta  = 0
    $recoveredAll = @{}
    foreach ($f in (Get-ChildItem $recovRoot -Recurse -File -Filter *.cs)) {
      foreach ($fq in (Get-FileFqns $f.FullName).all) { $recoveredAll[$fq] = $true }
    }
    foreach ($f in (Get-ChildItem $exportRoot -Recurse -File -Filter *.cs | Sort-Object FullName)) {
      $rel  = $f.FullName.Substring($exportRoot.Length + 1)
      $metaPath = "$($f.FullName).meta"
      $stem = [System.IO.Path]::GetFileNameWithoutExtension($f.Name)
      if ($stem -eq 'AssemblyInfo') { continue }  # assembly attributes: recovered tree ships its own
      $p = Get-FileFqns $f.FullName
      $hasMeta = Test-Path $metaPath
      if (-not $hasMeta) {
        if (-not $p.primary -or $index.ContainsKey($p.primary)) { $noMeta++; continue }  # covered by recovered copy
        $keeps.Add([pscustomobject]@{ exportRel = $rel; reason = 'unmatched-dependency' })
        continue
      }
      $metaText = Read-TextRaw $metaPath
      $gm = $rxMetaGuid.Match($metaText)
      if (-not $gm.Success) { throw "no guid in $metaPath" }
      $guid = $gm.Groups[1].Value
      if ($p.primary -and $index.ContainsKey($p.primary)) {
        $carries.Add([pscustomobject]@{ guid = $guid; targetRel = $index[$p.primary]; exportRel = $rel; primary = $p.primary; metaB64 = [Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes($metaText)) })
      } elseif ($p.all.Count -gt 0 -and ($p.all | Where-Object { -not $recoveredAll.ContainsKey($_) }).Count -eq 0) {
        # all declared types already exist in the recovered copy: delete export, guid orphaned
        $orphans.Add([pscustomobject]@{ exportRel = $rel; guid = $guid; types = $p.all })
      } else {
        $keeps.Add([pscustomobject]@{ exportRel = $rel; reason = 'unmatched-type' })
      }
    }
    # resolve carries: one target file can only keep one guid -> prefer a referenced guid
    $byTarget = @{}
    foreach ($c in $carries) { $k = $c.targetRel.ToLower(); if (-not $byTarget.ContainsKey($k)) { $byTarget[$k] = New-Object System.Collections.Generic.List[object] }; $byTarget[$k].Add($c) }
    $dupeDrops = New-Object System.Collections.Generic.List[object]
    $carriesFinal = New-Object System.Collections.Generic.List[object]
    foreach ($k in $byTarget.Keys) {
      $list = $byTarget[$k]
      if ($list.Count -eq 1) { $carriesFinal.Add($list[0]); continue }
      $ordered = $list | Sort-Object { if ($referencedGuids.ContainsKey($_.guid)) { 0 } else { 1 } }, { $_.exportRel }
      $carriesFinal.Add($ordered[0])
      foreach ($d in $ordered[1..($ordered.Count - 1)]) { $dupeDrops.Add([pscustomobject]@{ exportRel = $d.exportRel; guid = $d.guid; targetRel = $d.targetRel; referenced = [bool]$referencedGuids.ContainsKey($d.guid) }) }
    }
    $mapObj.assemblies[$asmName] = [ordered]@{
      carries = $carriesFinal; keeps = $keeps; guidOrphans = $orphans; guidDupeDrops = $dupeDrops
      stats = [ordered]@{ exportCs = (Get-ChildItem $exportRoot -Recurse -File -Filter *.cs).Count; carries = $carriesFinal.Count; keeps = $keeps.Count; guidOrphans = $orphans.Count; guidDupeDrops = $dupeDrops.Count; exportCsNoMetaCovered = $noMeta }
    }
    # snapshot kept files once (they are deleted with the export below)
    $asmKeepDir = Join-Path $keepDir $asmName
    if (Test-Path $asmKeepDir) { Remove-Item $asmKeepDir -Recurse -Force }
    New-Item -ItemType Directory -Path $asmKeepDir -Force | Out-Null
    foreach ($kp in $keeps) {
      $src = Join-Path $exportRoot $kp.exportRel
      $dst = Join-Path $asmKeepDir $kp.exportRel
      New-Item -ItemType Directory -Path (Split-Path $dst -Parent) -Force | Out-Null
      Copy-Item $src $dst -Force
      if (Test-Path "$src.meta") { Copy-Item "$src.meta" "$dst.meta" -Force }
    }
  }
  # guards: make sure nothing carrying serialized references is silently dropped
  $carriedG = @{}; $keptG = @{}
  foreach ($asmName2 in $treeMap.Keys) {
    $am = $mapObj.assemblies[$asmName2]; if (-not $am) { continue }
    foreach ($c in $am.carries) { $carriedG[$c.guid] = $true }
    $snapRoot2 = Join-Path $keepDir $asmName2
    foreach ($kp in $am.keeps) {
      $mp = Join-Path $snapRoot2 ($kp.exportRel + '.meta')
      if (Test-Path $mp) { $gm2 = $rxMetaGuid.Match((Read-TextRaw $mp)); if ($gm2.Success) { $keptG[$gm2.Groups[1].Value] = $true } }
    }
  }
  $otherExportRefs = New-Object System.Collections.Generic.List[object]
  $covered = @(((Join-Path $proj $treeMap['Assembly-CSharp'].export).ToLower()), ((Join-Path $proj $treeMap['Assembly-CSharp-firstpass'].export).ToLower()))
  foreach ($mm in (Get-ChildItem (Join-Path $proj 'Assets') -Recurse -File -Filter *.cs.meta)) {
    $fp = $mm.FullName.ToLower(); $inRoot = $false
    foreach ($cv in $covered) { if ($fp.StartsWith($cv)) { $inRoot = $true; break } }
    if ($inRoot) { continue }
    $gm3 = $rxMetaGuid.Match((Read-TextRaw $mm.FullName)); if (-not $gm3.Success) { continue }
    if ($referencedGuids.ContainsKey($gm3.Groups[1].Value)) {
      $otherExportRefs.Add([pscustomobject]@{ guid = $gm3.Groups[1].Value; path = $mm.FullName.Substring($proj.Length + 1) })
    }
  }
  $lostRefs = New-Object System.Collections.Generic.List[object]
  foreach ($g in $referencedGuids.Keys) {
    if (-not $carriedG.ContainsKey($g) -and -not $keptG.ContainsKey($g) -and -not $referencedDllGuids.ContainsKey($g)) { $lostRefs.Add($g) }
  }
  $mapObj.guards = [ordered]@{ referencedInOtherExportFolders = $otherExportRefs; referencedGuidsNotCarriedOrKept = $lostRefs }
  Write-Host ("guards: referenced metas outside placement scope = {0}; referenced guids not carried/kept = {1}" -f $otherExportRefs.Count, $lostRefs.Count)

  $mapFileJson = $mapObj | ConvertTo-Json -Depth 8
  Write-TextRaw $mapFile $mapFileJson
  # always continue with the JSON round-tripped shape so first run and reruns behave identically
  $map = ConvertFrom-Json $mapFileJson
  "map written: $mapFile"
} else {
  Write-Host 'phase 1: using persisted guid map (rerun)'
  if ($map.guards) {
    Write-Host ("guards (persisted): outside-scope referenced metas = {0}; referenced guids not carried/kept = {1}" -f @($map.guards.referencedInOtherExportFolders).Count, @($map.guards.referencedGuidsNotCarriedOrKept).Count)
  }
}

# ---------- phase 2: place trees ----------
#
# A rerun must never revert client work. Every shipped fix lives only inside these project
# copies, so there is no wipe: each recovered file is placed only where the project has no
# file for it, and an existing project file is left byte-for-byte alone whenever its content
# differs from the pristine copy of its twin (the namespace transform and the folder renames
# the copies get are applied to the twin first, so decompiler-transformed files that nobody
# edited still count as pristine). Files edited in the project and files with no twin in
# api\recovered at all are listed as preserved. -ResetClientCode is the only destructive path:
# it moves the current tree to build\project-merge-analysis\reset_backup\ and rebuilds pristine,
# reverting every project-only fix - deliberate, and said so on the console.

$transformTotal = 0
$metaCarryTotal = 0
$preservedReport = [ordered]@{}
foreach ($asmName in $treeMap.Keys) {
  $info = $treeMap[$asmName]
  $target = Join-Path $proj $info.target
  $srcRoot = Join-Path $recov $info.recov
  $xf = @('*.csproj')
  if ($pluginTrees -contains $info.recov) { $xf += 'AssemblyInfo.cs','UnitySourceGeneratedAssemblyMonoScriptTypes_v1.cs' }

  if ($ResetClientCode -and (Test-Path -LiteralPath $target)) {
    $bakRoot = Join-Path $ana 'reset_backup'
    if (-not (Test-Path -LiteralPath $bakRoot)) { New-Item -ItemType Directory -Path $bakRoot -Force | Out-Null }
    $bak = Join-Path $bakRoot $asmName
    if (Test-Path -LiteralPath $bak) { Remove-Item -LiteralPath $bak -Recurse -Force }   # written by our own earlier reset
    Move-Item -LiteralPath $target -Destination $bak -Force
    Write-Host ("  RESET-CLIENT-CODE: {0} moved to build\project-merge-analysis\reset_backup\{1}; rebuilding from api\recovered (this reverts every project-only client fix)" -f $info.target, $asmName)
  }

  if (-not (Test-Path -LiteralPath $srcRoot)) { throw "missing recovered tree: $srcRoot" }
  $preservedEdited = New-Object System.Collections.Generic.List[string]
  foreach ($f in (Get-ChildItem -LiteralPath $srcRoot -Recurse -File)) {
    if (Test-ExcludedLeaf $f.Name $xf) { continue }
    $relFixed = Get-MappedRel $f.FullName.Substring($srcRoot.Length + 1)
    $tgt = Join-Path $target $relFixed
    $srcText = Read-TextRaw $f.FullName
    $priText = Convert-FileScopedText $srcText $f.FullName
    if ($null -eq $priText) { $priText = $srcText }
    if (Test-Path -LiteralPath $tgt) {
      # existing project file: keep it unless it is still the pristine twin content
      if ((Read-TextRaw $tgt) -ne $priText) { $preservedEdited.Add((Join-Path $info.target $relFixed)) }
      continue
    }
    $dir = Split-Path $tgt -Parent
    if (-not (Test-Path -LiteralPath $dir)) { New-Item -ItemType Directory -Path $dir -Force | Out-Null }
    if ($priText -ceq $srcText) { Copy-Item -LiteralPath $f.FullName -Destination $tgt -Force }
    else { Write-TextRaw $tgt $priText }
  }
  # Unity folder magic: a directory named Editor or Plugins reassigns .cs under it to the
  # editor / firstpass assembly. In decompiled trees such names come from namespaces, so
  # rename those folders in the project copies (the recovered trees are never touched).
  # The copy loop above already routes new files to _Editor/_Plugins; this pass only cleans
  # stray folders an older run may have left. Rename deepest-first against a materialized
  # list so renames never break enumeration.
  $preservedTwinless = New-Object System.Collections.Generic.List[string]
  if (Test-Path -LiteralPath $target) {
    $dirs = @(Get-ChildItem $target -Recurse -Directory | Sort-Object { $_.FullName.Length } -Descending)
    foreach ($d in $dirs) {
      $leaf = Split-Path -Leaf $d.FullName
      if ($leaf -eq 'Editor' -or $leaf -eq 'Plugins') {
        Rename-Item $d.FullName "_$leaf"
        Write-Host ("  renamed Unity-magic folder in copy: {0} -> {1}" -f $d.FullName.Substring($proj.Length + 1), "_$leaf")
      }
    }
    # transform file-scoped namespaces in the copy
    foreach ($f in (Get-ChildItem $target -Recurse -File -Filter *.cs)) {
      if (Transform-FileScoped $f.FullName) { $transformTotal++ }
    }
    # hand-written files with no twin in api\recovered are by definition preserved; list them
    foreach ($f in (Get-ChildItem $target -Recurse -File -Filter *.cs)) {
      $rel = $f.FullName.Substring($target.Length + 1)
      if ($rel -like '_assetripper_only\*') { continue }   # export snapshots, kept by design
      $hasTwin = Test-Path -LiteralPath (Join-Path $srcRoot $rel)
      if (-not $hasTwin) {
        $back = (($rel -split '\\') | ForEach-Object { if ($_ -eq '_Editor' -or $_ -eq '_Plugins') { $_.Substring(1) } else { $_ } }) -join '\'
        $hasTwin = Test-Path -LiteralPath (Join-Path $srcRoot $back)
      }
      if (-not $hasTwin) { $preservedTwinless.Add((Join-Path $info.target $rel)) }
    }
  }
  # carry metas
  $asmMap = $map.assemblies.$asmName
  if ($asmMap) {
    foreach ($c in $asmMap.carries) {
      # apply the same Unity-magic folder renames the copy got
      $relFixed = ($c.targetRel -split '\\') | ForEach-Object { if ($_ -eq 'Editor' -or $_ -eq 'Plugins') { "_$_" } else { $_ } }
      $tgtCs = Join-Path $target (($relFixed -join '\'))
      if (-not (Test-Path $tgtCs)) { throw "carry target missing: $tgtCs" }
      Write-BytesRaw "$tgtCs.meta" ([Convert]::FromBase64String($c.metaB64))
      $metaCarryTotal++
    }
    # kept AssetRipper-only files -> _assetripper_only
    $snapRoot = Join-Path $keepDir $asmName
    foreach ($kp in $asmMap.keeps) {
      $src = Join-Path $snapRoot $kp.exportRel
      if (-not (Test-Path $src)) { continue }
      $relFixed2 = ($kp.exportRel -split '\\') | ForEach-Object { if ($_ -eq 'Editor' -or $_ -eq 'Plugins') { "_$_" } else { $_ } }
      $dst = Join-Path (Join-Path $target '_assetripper_only') (($relFixed2 -join '\'))
      New-Item -ItemType Directory -Path (Split-Path $dst -Parent) -Force | Out-Null
      Copy-Item $src $dst -Force
      if (Test-Path "$src.meta") { Copy-Item "$src.meta" "$dst.meta" -Force }
    }
  }
  $csCount = if (Test-Path -LiteralPath $target) { @(Get-ChildItem $target -Recurse -File -Filter *.cs).Count } else { 0 }
  "placed: {0,-35} cs={1,-5} (target: {2})" -f $asmName, $csCount, $info.target
  Write-Host ("  preserved project code: {0} file(s) edited away from api\recovered, {1} with no twin" -f $preservedEdited.Count, $preservedTwinless.Count)
  foreach ($p in $preservedEdited)   { Write-Host ("    preserved (edited):  {0}" -f $p) }
  foreach ($p in $preservedTwinless) { Write-Host ("    preserved (no twin): {0}" -f $p) }
  if ($ResetClientCode) { Write-Host '    (-ResetClientCode: this tree was rebuilt pristine from api\recovered)' }
  $preservedReport[$asmName] = [ordered]@{ edited = @($preservedEdited); noTwin = @($preservedTwinless) }
}

# The old AssetRipper Assembly-CSharp export (Assets\Scripts) is retired in phase 3
# (moved out of Assets, see export_discarded).

# Unity 2022 compiles its own UnitySourceGeneratedAssemblyMonoScriptTypes_v1 into both
# assemblies through Roslyn source generators; the decompiled copies in the copied trees
# would define the same class a second time (CS0101/CS0579/CS0111). Delete them.
foreach ($g in @((Join-Path $proj 'Assets\Assembly-CSharp\UnitySourceGeneratedAssemblyMonoScriptTypes_v1.cs'),
                 (Join-Path $proj 'Assets\Plugins\Assembly-CSharp-firstpass\UnitySourceGeneratedAssemblyMonoScriptTypes_v1.cs'))) {
  foreach ($f in @($g, "$g.meta")) { if (Test-Path $f) { Remove-Item $f -Force } }
}

# ---------- phase 3: loose dll disposition ----------

$pluginsDir = Join-Path $proj 'Assets\Plugins'
# Native plugins from the shipped build (audio effects like KONAMI/VirtualSurround and the
# arcade io libs). spice2x supplied these under S3; the standalone player needs them for
# shipping parity. WebGL ignores x86_64 plugin folders.
$nativeSrc = Join-Path $install 'Plugins\x86_64'
$nativeDst = Join-Path $pluginsDir 'x86_64'
if (-not $skipInstall -and (Test-Path $nativeSrc)) {
  $null = Invoke-Robocopy @($nativeSrc, $nativeDst, '/E', '/NFL', '/NDL', '/NJH', '/NP')
  Write-Host 'native plugins installed to Assets\Plugins\x86_64'
}
$deleted = New-Object System.Collections.Generic.List[string]
# AssetRipper per-plugin-tree export folders (decompiles of the six dlls whose sources are
# now copied to Assets\<tree>): same types as the copied trees, so deleting them is required
# (as .cs under Plugins they would compile a second time into Assembly-CSharp-firstpass).
$folderDeletions = New-Object System.Collections.Generic.List[string]
# Export folders must go (their types are already in the placed trees), but they go OUT, not
# away: moved under build\project-merge-analysis\export_discarded\ instead of a recursive delete.
$scriptsDir = Join-Path $proj 'Assets\Scripts'
if (Test-Path $scriptsDir) { Move-ProjectTreeOut $scriptsDir 'Assets_Scripts'; $folderDeletions.Add('Assets\Scripts (Assembly-CSharp export)') }
foreach ($t in $pluginTrees) {
  $pdir = Join-Path $pluginsDir $t
  if (Test-Path $pdir) { Move-ProjectTreeOut $pdir "Plugins_$t"; $folderDeletions.Add("Assets\Plugins\$t") }
}
foreach ($n in ($dllDeleteSource + $dllDeletePackage + $dllDeleteBcl)) {
  $metaF = Join-Path $pluginsDir "$n.dll.meta"
  if ((Test-Path $metaF) -and $referencedDllGuids.Count -gt 0) {
    $gm4 = $rxMetaGuid.Match((Read-TextRaw $metaF))
    if ($gm4.Success -and $referencedDllGuids.ContainsKey($gm4.Groups[1].Value)) {
      throw "refusing to delete $n.dll: its importer guid carries serialized script references"
    }
  }
  foreach ($f in @((Join-Path $pluginsDir "$n.dll"), $metaF)) {
    if (Test-Path $f) { Remove-Item $f -Force; $deleted.Add((Split-Path -Leaf $f)) }
  }
}
$flagged = 0
foreach ($n in $dllKeep) {
  $meta = Join-Path $pluginsDir "$n.dll.meta"
  if (Test-Path $meta) {
    $t = Read-TextRaw $meta
    $rx = [regex]'(?is)(first:\s*Editor:\s*Editor\s*second:\s*enabled:\s*)0\b'
    if ($rx.IsMatch($t)) { Write-TextRaw $meta ($rx.Replace($t, '${1}1')); $flagged++ }
  }
}
"loose dlls: deleted $($deleted.Count) items (files+metas), metas editor-enabled for $flagged kept dlls"

# ---------- phase 3.6: editor automation entry points (compiled against UnityEditor modules) ----------
# Newest-wins, not a blind overwrite: the build tree carries its own Assets\Editor copies and
# its own history (BuildWebGLPlayerFast, added in the build repo on 2026-11-03), and a -Force
# copy from scripts/editor-automation reverts them every time this prep runs (ledger L42).
& (Join-Path $PSScriptRoot 'sync_editor_automation.ps1') -SourceDir (Join-Path $PSScriptRoot 'editor-automation') -ProjectDir $proj

# ---------- phase 4: manifest + csc.rsp ----------

$manifestPath = Join-Path $proj 'Packages\manifest.json'
$mj = Get-Content $manifestPath -Raw | ConvertFrom-Json
$deps = [ordered]@{}
foreach ($p in $mj.dependencies.PSObject.Properties) { $deps[$p.Name] = $p.Value }
foreach ($k in $manifestPackages.Keys) {
  # skip if already vendored as an embedded package folder (Packages/<name>) - embedded wins and
  # a redundant manifest pin only risks a version fight; also skip names whose assembly we keep loose
  if (Test-Path (Join-Path (Split-Path $manifestPath) $k)) { $deps.Remove($k) | Out-Null; continue }
  if ($k -eq 'com.unity.timeline') { $deps.Remove($k) | Out-Null; continue }  # Unity.Timeline.dll stays loose (81 baked GUID refs)
  $deps[$k] = $manifestPackages[$k]
}
$orderedDeps = [ordered]@{}
foreach ($k in ($deps.Keys | Sort-Object)) { $orderedDeps[$k] = $deps[$k] }
$manifestJson = ([pscustomobject]@{ dependencies = [pscustomobject]$orderedDeps } | ConvertTo-Json -Depth 5)
$null = (ConvertFrom-Json $manifestJson)  # well-formedness check
Write-TextRaw $manifestPath ($manifestJson + "`n")
"manifest: $($orderedDeps.Count) entries (modules + $($manifestPackages.Count) packages), json re-parsed OK"

$csc = Join-Path $proj 'Assets\csc.rsp'
if (-not (Test-Path $csc)) { Write-TextRaw $csc ("-unsafe`r`n-nowarn:1701,1702,0436,1591`r`n") }
"csc.rsp: ensured (-unsafe; recovered trees use unsafe blocks)"

# ---------- phase 5: addressable bundles + staged videos ---------- (-SkipBundles / unreachable install guard)

$aaDst = Join-Path $proj 'Assets\StreamingAssets\aa'
if (-not $SkipBundles -and -not $skipInstall) {
  $aaSrc = Join-Path $install 'StreamingAssets\aa'
  $ro = Invoke-Robocopy @($aaSrc, $aaDst, '/E', '/NFL', '/NDL', '/NJH', '/NP')
  $tail = ($ro.out | Select-Object -Last 12) -join "`n"
  Write-Host "robocopy aa tail:`n$tail"
  $bundleStats = @{ files = (Get-ChildItem $aaDst -Recurse -File).Count; bytes = ((Get-ChildItem $aaDst -Recurse -File | Measure-Object Length -Sum).Sum); robocopyExit = $ro.code }
} else {
  $bundleStats = @{ files = 0; bytes = 0; robocopyExit = 'skipped' }
  if (Test-Path -LiteralPath $aaDst) {
    $bundleStats.files = @(Get-ChildItem $aaDst -Recurse -File).Count
    $bundleStats.bytes = (@(Get-ChildItem $aaDst -Recurse -File | Measure-Object Length -Sum).Sum)
  }
}
$charDir = Join-Path $proj 'Assets\StreamingAssets\Characters'
$yakDir  = Join-Path $proj 'Assets\StreamingAssets\Yakuman'
$charCount  = if (Test-Path -LiteralPath $charDir) { @(Get-ChildItem $charDir -Recurse -File -Filter *.webm).Count } else { 0 }
$yakCount   = if (Test-Path -LiteralPath $yakDir)  { @(Get-ChildItem $yakDir  -Recurse -File -Filter *.webm).Count } else { 0 }
if ($skipInstall) {
  Write-Host "install share unreachable ($install) - bundle, native plugin and video staging skipped"
} elseif ($charCount -ne 121 -or $yakCount -ne 13) {
  Write-Host "video staging mismatch (Characters=$charCount expect 121, Yakuman=$yakCount expect 13) - recopying from install"
  foreach ($d in 'Characters','Yakuman') {
    $null = Invoke-Robocopy @((Join-Path $install "StreamingAssets\$d"), (Join-Path $proj "Assets\StreamingAssets\$d"), '/E', '/NFL', '/NDL', '/NJH', '/NP')
  }
  $charCount  = @(Get-ChildItem (Join-Path $proj 'Assets\StreamingAssets\Characters') -Recurse -File -Filter *.webm).Count
  $yakCount   = @(Get-ChildItem (Join-Path $proj 'Assets\StreamingAssets\Yakuman')    -Recurse -File -Filter *.webm).Count
}

# ---------- report ----------

$totals = [ordered]@{
  assetsTotalFiles   = @(Get-ChildItem (Join-Path $proj 'Assets') -Recurse -File).Count
  assetsCsFiles      = @(Get-ChildItem (Join-Path $proj 'Assets') -Recurse -File -Filter *.cs).Count
  assetsMetaFiles    = @(Get-ChildItem (Join-Path $proj 'Assets') -Recurse -File -Filter *.meta).Count
  looseDllsRemaining = @(Get-ChildItem $pluginsDir -File -Filter *.dll).Count
  foldersDeleted     = $folderDeletions
  bundleFiles        = $bundleStats.files
  bundleBytes        = $bundleStats.bytes
  charactersWebm     = $charCount
  yakumanWebm        = $yakCount
  metasCarried       = $metaCarryTotal
  namespacesTransformed = $transformTotal
  metasEditorEnabled = $flagged
  looseItemsDeleted  = $deleted
  manifestEntries    = $orderedDeps.Count
}
$report = [ordered]@{
  when = (Get-Date).ToString('s')
  phases = 'trees+metas, loose dll disposition, manifest+csc.rsp, bundle staging'
  totals = $totals
  clientCode = [ordered]@{ resetClientCode = [bool]$ResetClientCode; preserved = $preservedReport }
  assemblyStats = [ordered]@{}
}
foreach ($asmName in $treeMap.Keys) {
  $asmMap = $map.assemblies.$asmName
  if ($asmMap -and $asmMap.stats) { $report.assemblyStats[$asmName] = $asmMap.stats }
  else { $report.assemblyStats[$asmName] = 'plugin-tree (no AssetRipper cs export existed)' }
}
if ($map.guards) { $report.guards = $map.guards }
$reportJson = $report | ConvertTo-Json -Depth 6
Write-TextRaw (Join-Path $ana 'prepare_report.json') $reportJson
Write-Host ''
Write-Host '=== summary ==='
Write-Host ("trees placed with {0} carried script metas; {1} files transformed (file-scoped->braced)" -f $metaCarryTotal, $transformTotal)
Write-Host ("loose dll items deleted: {0}; kept loose dlls: {1}; editor-enabled metas: {2}" -f $deleted.Count, (Get-ChildItem $pluginsDir -File -Filter *.dll).Count, $flagged)
Write-Host ("bundles: {0} files, {1:N1} GB in Assets/StreamingAssets/aa" -f $bundleStats.files, ($bundleStats.bytes / 1GB))
Write-Host ("videos: Characters={0} webm, Yakuman={1} webm" -f $charCount, $yakCount)
Write-Host ("folders moved out of Assets (kept under build\project-merge-analysis\export_discarded): {0}" -f ($folderDeletions -join '; '))
$presEd = 0; $presTw = 0
foreach ($v in $preservedReport.Values) { $presEd += @($v.edited).Count; $presTw += @($v.noTwin).Count }
if ($ResetClientCode) { Write-Host 'client code: REBUILT PRISTINE (-ResetClientCode) - previous trees saved under build\project-merge-analysis\reset_backup\' }
else { Write-Host ("client code: rerun non-destructive - {0} edited file(s) and {1} twinless file(s) preserved (listed above + prepare_report.json)" -f $presEd, $presTw) }
if ($map.guards) {
  $gOut = @($map.guards.referencedInOtherExportFolders).Count
  $gLost = @($map.guards.referencedGuidsNotCarriedOrKept).Count
  Write-Host ("guards: referenced-in-other-export={0} referenced-not-carried-or-kept={1}" -f $gOut, $gLost)
}
Write-Host "report: $ana\prepare_report.json"
