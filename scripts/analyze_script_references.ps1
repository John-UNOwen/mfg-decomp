# analyze_script_references.ps1 - census of m_Script guid references in serialized text
# assets of the AssetRipper project export, joined against the .meta guid tables.
# Writes JSON + prints summary. Read-only against the project tree (docs/21 verification).
#
# Usage: pwsh -File scripts/analyze_script_references.ps1 -Project <build/unity-project> -Out <json>
param(
  [Parameter(Mandatory=$true)][string]$Project,
  [Parameter(Mandatory=$true)][string]$OutFile
)
$ErrorActionPreference = 'Stop'
$assets = Join-Path $Project 'Assets'

# 1. guid tables from metas
$csGuids = @{}     # guid -> path   (AssetRipper-exported .cs metas)
$dllGuids = @{}    # guid -> assembly file name
Get-ChildItem $assets -Recurse -File -Filter *.cs.meta | ForEach-Object {
  $g = (Select-String -Path $_.FullName -Pattern '^guid: (\w+)' -List).Matches[0].Groups[1].Value
  $csGuids[$g] = $_.FullName.Substring($Project.Length+1)
}
Get-ChildItem $assets -Recurse -File -Filter *.dll.meta | ForEach-Object {
  $g = (Select-String -Path $_.FullName -Pattern '^guid: (\w+)' -List).Matches[0].Groups[1].Value
  $dllGuids[$g] = (Split-Path $_.Name) -replace '\.dll\.meta$',''
}

# 2. m_Script references in serialized files
$exts = '*.unity','*.prefab','*.asset','*.mat','*.anim','*.controller','*.overrideController','*.playable','*.lighting','*.shader','*.rendertexture'
$rx = [regex]'m_Script:\s*\{fileID:\s*(-?\d+),\s*guid:\s*(\w{32}),\s*type:\s*(\d+)\}'
$counts = @{}     # guid -> [count, fileIds(hashtable)]
$byFile = 0
foreach ($e in $exts) {
  Get-ChildItem $assets -Recurse -File -Filter $e | ForEach-Object {
    $m = $rx.Matches((Get-Content $_.FullName -Raw))
    foreach ($x in $m) {
      $g = $x.Groups[2].Value
      if (-not $counts.ContainsKey($g)) { $counts[$g] = [pscustomobject]@{ count=0; fileIds=@{} } }
      $counts[$g].count += 1
      $fid = $x.Groups[1].Value
      if (-not $counts[$g].fileIds.ContainsKey($fid)) { $counts[$g].fileIds[$fid] = 0 }
      $counts[$g].fileIds[$fid] += 1
      $byFile += 1
    }
  }
}

# 3. classify
$resolvedCs = @{}; $resolvedDll = @{}; $builtin = @{}; $unresolved = @{}
foreach ($g in $counts.Keys) {
  $c = $counts[$g]
  if ($csGuids.ContainsKey($g)) { $resolvedCs[$g] = [pscustomobject]@{ count=$c.count; fileIds=$c.fileIds.Keys; path=$csGuids[$g] } }
  elseif ($dllGuids.ContainsKey($g)) { $resolvedDll[$g] = [pscustomobject]@{ count=$c.count; fileIds=$c.fileIds.Keys; dll=$dllGuids[$g] } }
  elseif ($g -match '^0{24}[0-9a-f]{8}$') { $builtin[$g] = $c.count }
  else { $unresolved[$g] = $c.count }
}

$report = [ordered]@{
  total_refs = $byFile
  distinct_guids = $counts.Count
  resolved_to_cs_meta = [pscustomobject]@{ guids = $resolvedCs.Count; refs = (($resolvedCs.Values | ForEach-Object { $_.count }) | Measure-Object -Sum).Sum }
  resolved_to_dll_meta = [pscustomobject]@{ guids = $resolvedDll.Count; refs = (($resolvedDll.Values | ForEach-Object { $_.count }) | Measure-Object -Sum).Sum }
  builtin_pseudo = [pscustomobject]@{ guids = $builtin.Count; refs = (($builtin.Values | Measure-Object -Sum).Sum) }
  unresolved = [pscustomobject]@{ guids = $unresolved.Count; refs = (($unresolved.Values | Measure-Object -Sum).Sum); detail = $unresolved }
  dll_providers = $resolvedDll
  cs_providers = $resolvedCs
}
$d = Split-Path $OutFile -Parent
if ($d -and -not (Test-Path $d)) { New-Item -ItemType Directory -Path $d -Force | Out-Null }
$report | ConvertTo-Json -Depth 8 | Set-Content $OutFile -Encoding UTF8
"total m_Script refs: {0}; distinct guids: {1}" -f $byFile, $counts.Count
"resolved to exported .cs metas: {0} refs / {1} guids" -f $report.resolved_to_cs_meta.refs, $report.resolved_to_cs_meta.guids
"resolved to loose .dll metas:   {0} refs / {1} guids" -f $report.resolved_to_dll_meta.refs, $report.resolved_to_dll_meta.guids
"builtin pseudo-guids:           {0} refs / {1} guids" -f $report.builtin_pseudo.refs, $report.builtin_pseudo.guids
"unresolved:                     {0} refs / {1} guids" -f $report.unresolved.refs, $report.unresolved.guids
"--- dll-provider script guids (loose dlls carrying serialized MonoBehaviours) ---"
foreach ($g in $resolvedDll.Keys) { "  {0,-42} refs={1,-6} fileIds={2}" -f $resolvedDll[$g].dll, $resolvedDll[$g].count, (($resolvedDll[$g].fileIds | Sort-Object) -join ',') }
"wrote $OutFile"
