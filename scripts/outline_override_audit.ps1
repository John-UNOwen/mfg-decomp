<#
.SYNOPSIS
  Audit the _Outline_Width overrides in the project's .mat files against the shader each of
  those materials actually binds, keep a baseline of what the tree carries, and restore values
  an outline sweep moved on materials that cannot bind them.

.DESCRIPTION
  An _Outline_Width override only does something when the shader the material binds declares
  that property in its Properties block. On a shader that does not declare it the number is
  dead data in a tracked file, and a HasProperty dump misreports the material. The 2026-11-04
  outline sweep (build repo 97a6a70419, "fix: sweep map-form outline widths to 0.1") moved
  values across the material set without checking that binding, which is ledger L49.

  This script resolves every material's m_Shader guid to the .shader asset the project carries,
  reads that shader's Properties block, and classifies each _Outline_Width override:

    bound          the shader declares the property and the value is inside 0..MaxWidth
    out-of-range   the shader declares it and the value is negative or above MaxWidth
    unbindable     the override is there but the bound shader has no such property
    unresolved     the material binds a built-in or missing shader, so nothing is claimed

  Modes:
    (no options)              report only; exit 1 if anything is unbindable or out of range
    -WriteBaseline -BaselinePath <file>
                              record the current inventory as the state the tree is meant to have
    -BaselinePath <file>      compare the tree with that inventory; exit 1 on any drift, i.e. a
                              new override, a removed one, a moved value, or an override whose
                              binding class changed (an inert one that went live)
    -RevertUnbindableTo <rev> for every inert override (unbindable or unresolved), restore the
                              value the same file carried at <rev> - the pre-sweep serialization
    -Apply is deliberately absent: inert overrides the shipped serialization wrote are the
                              game's own data and are kept, not deleted.

  The report always lands in $ReportPath; the baseline is JSON so a later run can diff against it.

.EXAMPLE
  pwsh -NoProfile -File scripts/outline_override_audit.ps1
  pwsh -NoProfile -File scripts/outline_override_audit.ps1 -RevertUnbindableTo 97a6a70419^ -BaselinePath build\analysis\outline_override_baseline.json
  pwsh -NoProfile -File scripts/outline_override_audit.ps1 -BaselinePath build\analysis\outline_override_baseline.json
#>
param(
  [string]$ProjectRoot = 'C:\mfg\build\unity-project',
  [switch]$WriteBaseline,
  [string]$BaselinePath = '',
  [string]$RevertUnbindableTo = '',
  [string]$CompareRoot = '',
  [double]$MaxWidth = 1.0,
  [string]$ReportPath = 'C:\mfgtmp\outline_override_audit.txt'
)

$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'

$assetsDir = Join-Path $ProjectRoot 'Assets'
if (-not (Test-Path -LiteralPath $assetsDir)) { throw "no Assets dir under $ProjectRoot" }

# ---- guid -> shader asset path, from the .shader.meta files the project carries ----
$shaderByGuid = @{}
foreach ($meta in (Get-ChildItem -LiteralPath $assetsDir -Recurse -File -Filter '*.shader.meta')) {
  $g = [regex]::Match((Get-Content -LiteralPath $meta.FullName -Raw), '(?m)^guid: ([0-9a-f]{32})').Groups[1].Value
  if ($g) { $shaderByGuid[$g] = $meta.FullName.Substring(0, $meta.FullName.Length - 5) }
}

# ---- does that shader declare the property (its Properties block only) ----
$declaresCache = @{}
function Test-ShaderDeclares([string]$shaderPath) {
  if ($declaresCache.ContainsKey($shaderPath)) { return $declaresCache[$shaderPath] }
  $text = Get-Content -LiteralPath $shaderPath -Raw
  $ok = $false
  $start = $text.IndexOf('Properties')
  if ($start -ge 0) {
    $open = $text.IndexOf('{', $start)
    if ($open -ge 0) {
      $depth = 0; $end = -1
      for ($i = $open; $i -lt $text.Length; $i++) {
        if ($text[$i] -eq '{') { $depth++ }
        elseif ($text[$i] -eq '}') { $depth--; if ($depth -eq 0) { $end = $i; break } }
      }
      if ($end -gt $open) { $ok = [regex]::IsMatch($text.Substring($open, $end - $open), '_Outline_Width\s*\(') }
    }
  }
  $declaresCache[$shaderPath] = $ok
  return $ok
}

function Scan-Overrides {
  $found = [System.Collections.Generic.List[object]]::new()
  foreach ($m in (Get-ChildItem -LiteralPath $assetsDir -Recurse -File -Filter '*.mat')) {
    $text = Get-Content -LiteralPath $m.FullName -Raw
    $ov = [regex]::Match($text, '(?m)^(\s*)_Outline_Width:\s*(-?[0-9.]+)\s*$')
    if (-not $ov.Success) { continue }
    $guid = [regex]::Match($text, 'm_Shader:\s*\{fileID:\s*-?[0-9]+,\s*guid:\s*([0-9a-f]{32})').Groups[1].Value
    $shaderPath = ''; $state = 'unbindable'
    if (-not $guid) { $state = 'unresolved'; $shaderPath = '<no m_Shader guid>' }
    elseif ($guid -eq '00000000000000000000000000000000') { $state = 'unresolved'; $shaderPath = '<built-in shader>' }
    elseif (-not $shaderByGuid.ContainsKey($guid)) { $state = 'unresolved'; $shaderPath = "<shader guid $guid not in project>" }
    else {
      $shaderPath = $shaderByGuid[$guid]
      $state = if (Test-ShaderDeclares $shaderPath) { 'bound' } else { 'unbindable' }
    }
    $value = [double]$ov.Groups[2].Value
    if ($state -eq 'bound' -and ($value -lt 0 -or $value -gt $MaxWidth)) { $state = 'out-of-range' }
    $found.Add([pscustomobject]@{
      Path = $m.FullName.Substring($ProjectRoot.Length + 1)
      Value = $value
      State = $state
      Shader = $shaderPath.Substring($ProjectRoot.Length + 1)
      Indent = $ov.Groups[1].Value
      Line = $ov.Groups[0].Value
    })
  }
  return $found
}

$rows = Scan-Overrides
$matTotal = @(Get-ChildItem -LiteralPath $assetsDir -Recurse -File -Filter '*.mat').Count

$report = [System.Collections.Generic.List[string]]::new()
function Add-Rows([string]$title, $rowsIn) {
  $report.Add($title)
  foreach ($r in ($rowsIn | Sort-Object Path)) { $report.Add("  $($r.Path) = $($r.Value)  shader=$($r.Shader)") }
  $report.Add('')
}

function Write-Report($rowsIn) {
  $bound = @($rowsIn | Where-Object { $_.State -eq 'bound' })
  $unbindable = @($rowsIn | Where-Object { $_.State -eq 'unbindable' })
  $range = @($rowsIn | Where-Object { $_.State -eq 'out-of-range' })
  $unresolved = @($rowsIn | Where-Object { $_.State -eq 'unresolved' })
  $report.Add("_Outline_Width override audit ($((Get-Date).ToString('s'))) project=$ProjectRoot maxWidth=$MaxWidth")
  $report.Add("mat files: $matTotal  overrides: $($rowsIn.Count)  bound: $($bound.Count)  unbindable: $($unbindable.Count)  out-of-range: $($range.Count)  unresolved: $($unresolved.Count)")
  $report.Add('')
  Add-Rows 'unbindable (the bound shader has no _Outline_Width in its Properties block):' $unbindable
  Add-Rows ('out-of-range (bindable shader, value outside 0..' + $MaxWidth + '):') $range
  Add-Rows 'unresolved (built-in or missing shader - not claimed either way):' $unresolved
  $report.Add('bound overrides, by shader:')
  foreach ($g in ($bound | Group-Object Shader | Sort-Object Count -Descending)) { $report.Add("  $($g.Count) material(s)  $($g.Name)") }
  return @{ bound = $bound; unbindable = $unbindable; range = $range; unresolved = $unresolved }
}

$counts = Write-Report $rows

if ($RevertUnbindableTo) {
  $report.Add('')
  $report.Add("revert pass against $RevertUnbindableTo (inert overrides only):")
  $inert = @($counts.unbindable) + @($counts.unresolved)
  $restored = 0
  foreach ($r in $inert) {
    $gitPath = ($r.Path -replace '\\', '/')
    $revText = git -C $ProjectRoot show "$RevertUnbindableTo`:$gitPath" 2>$null
    if ($LASTEXITCODE -ne 0) { $report.Add("  skipped $($r.Path): no such file at $RevertUnbindableTo"); continue }
    $revLine = [regex]::Match(($revText -join "`n"), '(?m)^\s*_Outline_Width:\s*(-?[0-9.]+)\s*$')
    if (-not $revLine.Success) { $report.Add("  skipped $($r.Path): no _Outline_Width line at $RevertUnbindableTo"); continue }
    $revValue = [double]$revLine.Groups[1].Value
    if ([Math]::Abs($revValue - $r.Value) -lt 1e-9) { continue }
    $full = Join-Path $ProjectRoot $r.Path
    $text = Get-Content -LiteralPath $full -Raw
    $eol = if ($text.Contains("`r`n")) { "`r`n" } else { "`n" }
    $lines = $text -split "`r?`n"
    $hit = 0
    for ($i = 0; $i -lt $lines.Count; $i++) {
      if ($lines[$i] -eq $r.Line) { $lines[$i] = ('{0}_Outline_Width: {1}' -f $r.Indent, $revValue); $hit++ }
    }
    if ($hit -ne 1) { throw "expected exactly one _Outline_Width line in $($r.Path), found $hit - nothing written" }
    Set-Content -LiteralPath $full -Value ($lines -join $eol) -Encoding utf8 -NoNewline
    $restored++
    $report.Add("  restored $($r.Path): $($r.Value) -> $revValue (what it carried at $RevertUnbindableTo)")
  }
  $report.Add("revert: $restored inert override value(s) put back to their pre-sweep serialization")
  $rows = Scan-Overrides
  $counts = Write-Report $rows
}

if ($CompareRoot) {
  $report.Add('')
  $report.Add("compare against $CompareRoot (same relative material paths, another state of the tree):")
  $same = 0; $changedLive = 0; $changedInert = 0; $missing = 0
  foreach ($r in $rows) {
    $other = Join-Path $CompareRoot $r.Path
    if (-not (Test-Path -LiteralPath $other)) { $missing++; continue }
    $m = [regex]::Match((Get-Content -LiteralPath $other -Raw), '(?m)^\s*_Outline_Width:\s*(-?[0-9.]+)\s*$')
    if (-not $m.Success) { $missing++; continue }
    $v = [double]$m.Groups[1].Value
    if ([Math]::Abs($v - $r.Value) -lt 1e-9) { $same++; continue }
    if ($r.State -eq 'bound' -or $r.State -eq 'out-of-range') {
      $changedLive++
      $report.Add("  live value differs: $($r.Path) was $v, now $($r.Value) (shader=$($r.Shader))")
    } else {
      $changedInert++
      $report.Add("  inert value differs: $($r.Path) was $v, now $($r.Value) (shader=$($r.Shader))")
    }
  }
  $report.Add("compare: same $same, live differs $changedLive, inert differs $changedInert, not comparable $missing")
}

$driftCount = -1
if ($BaselinePath) {
  if (Test-Path -LiteralPath $BaselinePath) {
    $baseline = Get-Content -LiteralPath $BaselinePath -Raw | ConvertFrom-Json
    $byPath = @{}
    foreach ($e in $baseline.entries) { $byPath[$e.Path] = $e }
    $drift = [System.Collections.Generic.List[string]]::new()
    $seen = [System.Collections.Generic.HashSet[string]]::new()
    foreach ($r in $rows) {
      [void]$seen.Add($r.Path)
      if (-not $byPath.ContainsKey($r.Path)) { $drift.Add("new override: $($r.Path) = $($r.Value) ($($r.State), shader=$($r.Shader))"); continue }
      $b = $byPath[$r.Path]
      if ([Math]::Abs([double]$b.Value - $r.Value) -ge 1e-9) { $drift.Add("value changed: $($r.Path) was $($b.Value) in the baseline, now $($r.Value)") }
      if ($b.State -ne $r.State) { $drift.Add("binding changed: $($r.Path) was $($b.State) in the baseline, now $($r.State) (shader=$($r.Shader))") }
    }
    foreach ($e in $baseline.entries) { if (-not $seen.Contains($e.Path)) { $drift.Add("override gone: $($e.Path) carried $($e.Value) ($($e.State)) in the baseline, carries none now") } }
    $driftCount = $drift.Count
    $report.Add('')
    $report.Add("baseline: $BaselinePath ($($baseline.entries.Count) overrides recorded $($baseline.when) at $($baseline.rev))")
    if ($driftCount -eq 0) { $report.Add('baseline match: every override sits where the baseline put it, at the recorded value') }
    else { foreach ($d in $drift) { $report.Add("DRIFT $d") } }
  } else {
    $report.Add("baseline: none at $BaselinePath yet - run with -WriteBaseline")
  }
}

if ($WriteBaseline) {
  if (-not $BaselinePath) { throw '-WriteBaseline needs -BaselinePath' }
  $obj = [ordered]@{
    when = (Get-Date).ToString('s')
    rev = git -C $ProjectRoot rev-parse --short HEAD
    project = $ProjectRoot
    maxWidth = $MaxWidth
    entries = @($rows | Sort-Object Path | ForEach-Object { [ordered]@{ Path = $_.Path; Value = $_.Value; State = $_.State; Shader = $_.Shader } })
  }
  $null = New-Item -ItemType Directory -Path (Split-Path -Parent $BaselinePath) -Force
  Set-Content -LiteralPath $BaselinePath -Value ($obj | ConvertTo-Json -Depth 4) -Encoding utf8
  $report.Add("baseline written: $($obj.entries.Count) overrides -> $BaselinePath")
}

$null = New-Item -ItemType Directory -Path (Split-Path -Parent $ReportPath) -Force
Set-Content -LiteralPath $ReportPath -Value ($report -join "`r`n") -Encoding utf8
$report | ForEach-Object { Write-Output $_ }
"report: $ReportPath"

if ($driftCount -ge 0) {
  # With a baseline the test is drift, not raw counts: inert overrides the shipped serialization
  # already carried are recorded as the tree's state, and any new or moved one fails the run.
  if ($driftCount -gt 0) { Write-Output "exit 1: $driftCount drift item(s) against the baseline"; exit 1 }
  exit 0
}
if ($counts.unbindable.Count -gt 0 -or $counts.range.Count -gt 0) { exit 1 }
exit 0
