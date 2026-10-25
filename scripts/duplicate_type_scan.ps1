# duplicate_type_scan.ps1 - post-merge safety scan (docs/21-project-merge.md verification).
# 1) every type declared in the placed project copies vs the type lists of the kept loose
#    dlls (loose_dlls.ndjson): same full name in both = Unity CS0436 "imported type" warning
#    at compile, own-assembly definition wins; flag anything unexpected.
# 2) same full name declared in two placed files (compile already proves 0, recorded anyway).
# 3) assembly-qualified runtime lookups (Type.GetType("X, kamunity"), Assembly.Load("..."))
#    naming assemblies that no longer exist after the merge (kamunity, Shirahama*,
#    VirtualSurround, Win10actLog-Runtime) = play-mode reflection risk list.
# Read-only; writes build/project-merge-analysis/duplicate_types.json.
param([string]$Repo = '')
$ErrorActionPreference = 'Stop'
if (-not $Repo) { $Repo = Split-Path -Parent $PSScriptRoot }
$proj = Join-Path $Repo 'build\unity-project'
$ana  = Join-Path $Repo 'build\project-merge-analysis'
$utf8NoBom = New-Object System.Text.UTF8Encoding($false)

$rxNamespace = [regex]'(?m)^[ \t]*(?:namespace\s+)([\w.]+)(?:[ \t]*;|[ \t]*\{?[ \t]*$)'
$rxTypeDecl  = [regex]'(?m)^[ \t]*(?:[\w \t]*\s)?\b(?:class|struct|interface|enum)\b\s+([A-Za-z_]\w*)'

function Get-Declared([string]$path) {
  $text = [System.IO.File]::ReadAllText($path)
  $ns = ''; $found = New-Object System.Collections.Generic.List[string]
  foreach ($line in ($text -split "\r?\n")) {
    $t = $line.Trim(); if ($t.StartsWith('//')) { continue }
    $mn = $rxNamespace.Match($line); if ($mn.Success) { $ns = $mn.Groups[1].Value; continue }
    $mt = $rxTypeDecl.Match($line)
    if ($mt.Success) {
      $prefix = ''; if ($ns) { $prefix = "$ns." }
      $found.Add($prefix + $mt.Groups[1].Value)
    }
  }
  return $found
}

$placedRoots = 'Assets\Assembly-CSharp','Assets\Plugins\Assembly-CSharp-firstpass','Assets\kamunity',
  'Assets\Shirahama','Assets\Shirahama.Async','Assets\Shirahama.Rx','Assets\VirtualSurround',
  'Assets\Win10actLog-Runtime','Assets\Editor'
$placed = @{}     # fqn -> list of files
$placedFileCount = 0
foreach ($r in $placedRoots) {
  $root = Join-Path $proj $r
  if (-not (Test-Path $root)) { continue }
  foreach ($f in (Get-ChildItem $root -Recurse -File -Filter *.cs)) {
    $placedFileCount++
    foreach ($fq in (Get-Declared $f.FullName)) {
      if (-not $placed.ContainsKey($fq)) { $placed[$fq] = New-Object System.Collections.Generic.List[string] }
      $placed[$fq].Add($f.FullName.Substring($proj.Length + 1))
    }
  }
}

# kept loose dll type lists (from analyze_dlls.ps1 output, dot-separated TypeFqn)
$dllTypes = @{}   # dll name -> HashSet fqn
$keptDlls = @{}
foreach ($line in (Get-Content (Join-Path $ana 'loose_dlls.ndjson') -Encoding UTF8)) {
  if (-not $line.Trim()) { continue }
  $o = $line | ConvertFrom-Json
  $name = $o.name
  if (-not (Test-Path (Join-Path $proj "Assets\Plugins\$name.dll"))) { continue }  # deleted already
  $set = New-Object 'System.Collections.Generic.HashSet[string]'
  foreach ($t in @($o.types)) { if ($t) { [void]$set.Add($t) } }
  $dllTypes[$name] = $set
  $keptDlls[$name] = $true
}

# 1) placed vs kept dlls
$collisions = New-Object System.Collections.Generic.List[object]
foreach ($name in ($dllTypes.Keys | Sort-Object)) {
  $set = $dllTypes[$name]
  foreach ($fq in $placed.Keys) {
    if ($set.Contains($fq)) {
      $collisions.Add([pscustomobject]@{ fqn = $fq; dll = $name; files = @($placed[$fq]) })
    }
  }
}
# 2) placed vs placed
$dupesPlaced = New-Object System.Collections.Generic.List[object]
foreach ($fq in $placed.Keys) { if ($placed[$fq].Count -gt 1) { $dupesPlaced.Add([pscustomobject]@{ fqn = $fq; files = @($placed[$fq]) }) } }

# 3) assembly-qualified / Assembly.Load references to merged-away assemblies
$gone = 'kamunity','Shirahama','Shirahama.Async','Shirahama.Rx','VirtualSurround','Win10actLog-Runtime'
$rxAsmQ = [regex]'"[^"]*,\s*(kamunity|Shirahama(?:\.Async|\.Rx)?|VirtualSurround|Win10actLog-Runtime)\s*(,|")'
$rxAsmLoad = [regex]'Assembly\.(Load|ReflectionOnlyLoad)\s*\(\s*"(kamunity|Shirahama(?:\.Async|\.Rx)?|VirtualSurround|Win10actLog-Runtime)"'
$reflectionRisk = New-Object System.Collections.Generic.List[object]
foreach ($r in $placedRoots) {
  $root = Join-Path $proj $r
  if (-not (Test-Path $root)) { continue }
  foreach ($f in (Get-ChildItem $root -Recurse -File -Filter *.cs)) {
    $text = [System.IO.File]::ReadAllText($f.FullName)
    foreach ($pat in @($rxAsmQ, $rxAsmLoad)) {
      foreach ($m in $pat.Matches($text)) {
        $lineNo = ($text.Substring(0, $m.Index) -split "\r?\n").Count
        $reflectionRisk.Add([pscustomobject]@{ file = $f.FullName.Substring($proj.Length + 1); line = $lineNo; match = $m.Value })
      }
    }
  }
}

$reportObj = [ordered]@{
  when = (Get-Date).ToString('s')
  placedCsFiles = $placedFileCount
  placedDistinctTypes = $placed.Count
  keptLooseDlls = @($dllTypes.Keys | Sort-Object)
  keptLooseDllTypeCounts = [ordered]@{}
  placedVsLooseDllCollisions = $collisions
  duplicateFqnsInPlacedCode = $dupesPlaced
  assemblyQualifiedLookupsToMergedAssemblies = $reflectionRisk
}
foreach ($k in ($dllTypes.Keys | Sort-Object)) { $reportObj.keptLooseDllTypeCounts[$k] = $dllTypes[$k].Count }
$json = $reportObj | ConvertTo-Json -Depth 5
[System.IO.File]::WriteAllText((Join-Path $ana 'duplicate_types.json'), $json, $utf8NoBom)

"placed .cs files:       $placedFileCount  (distinct declared types: $($placed.Count))"
"kept loose dlls:        $($dllTypes.Count)"
"placed<->loose collisions: $($collisions.Count)"
$collisions | ForEach-Object { "  COLLISION $($_.dll) <- $($_.fqn)" } | Select-Object -First 40
"duplicate fqns in placed code: $($dupesPlaced.Count)"
"assembly-qualified/Assembly.Load hits to merged assemblies: $($reflectionRisk.Count)"
$reflectionRisk | Select-Object -First 20 | ForEach-Object { "  $($_.file):$($_.line)  $($_.match)" }
"report: $ana\duplicate_types.json"
