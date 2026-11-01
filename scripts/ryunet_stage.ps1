<#
ryunet_stage.ps1 - one staging path for the self-hosted mfg@asphyxia plugin.

Why this exists: the plugin used to be copied by hand wherever a package happened to be
working - dist, tmp/asphyxia_plugins, and one whole dist copy (core JS + config.ini + plugin)
per verification run. Every copy froze at the revision it was made at, several of them were
bootable, and the reports then named those directories as if they were live builds. This script
makes one tree canonical and everything else derived from it, stamped, and retirable.

  C:\mfg\tmp\RyuNET-core\dist\plugins\mfg@asphyxia   the canonical tree: the only copy that is
                                                     edited, the only copy the core loads.

usage:
  pwsh scripts\ryunet_stage.ps1 -Action status
      list every copy of the plugin in the tree: canonical / identical / diverged, and whether a
      diverged copy can still boot a server. Exit 1 when a diverged copy is bootable.
  pwsh scripts\ryunet_stage.ps1 -Action lane [-Lane verify-dist] [-Port 8093]
      [-MfgHttpPort 22431] [-MatchingPort 5701] [-Tag LOCAL]
      wipe and re-derive a verification lane from the canonical tree: core runtime + the current
      plugin + its own empty savedata + config.ini on its own ports (mfg_service_url points at
      the lane, not at 8083) + LANE_STAMP.json naming the build it holds. Start it with
      cd <lane>; node AsphyxiaCore.js.
  pwsh scripts\ryunet_stage.ps1 -Action mirror
      refresh the staging clone tmp\asphyxia_plugins\mfg@asphyxia from the canonical tree
      (one direction only - the generators' outputs are staged here too, never the reverse).
  pwsh scripts\ryunet_stage.ps1 -Action retire -Lane <name> -Note 'which build this evidences'
      turn a lane into a read-only build snapshot under tmp\RyuNET-core\.frozen-builds: the plugin
      TypeScript and the run outputs stay so a report's "before" claim can still be re-checked, the
      core runtime, config.ini and savedata are removed so the snapshot cannot boot or be probed.
      The lane name is free afterwards: -Action lane re-derives a fresh lane under it from the
      canonical tree, which is how a round that superseded its own lane closes -Action status green.
#>
param(
  [ValidateSet('status', 'lane', 'mirror', 'retire')] [string]$Action = 'status',
  [string]$Lane = 'verify-dist',
  [int]$Port = 8093,
  [int]$MfgHttpPort = 22431,
  [int]$MatchingPort = 5701,
  [string]$Tag = 'LOCAL',
  [string]$Note = ''
)

$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'

$CoreRoot = 'C:\mfg\tmp\RyuNET-core'
$Canonical = Join-Path $CoreRoot 'dist'
$CanonicalPlugin = Join-Path $Canonical 'plugins\mfg@asphyxia'
$MirrorPlugin = 'C:\mfg\tmp\asphyxia_plugins\mfg@asphyxia'
$FrozenRoot = Join-Path $CoreRoot '.frozen-builds'
$PluginName = 'mfg@asphyxia'

function Get-TreeHashes([string]$Dir) {
  $out = [ordered]@{}
  if (-not (Test-Path $Dir)) { return $out }
  Get-ChildItem $Dir -Recurse -File | Sort-Object FullName | ForEach-Object {
    # Forward slashes and lowercase hex: .audit/smoke_common.py computes the same map and the
    # two have to agree, or a staged lane looks stale to the sweeps.
    $rel = (($_.FullName.Substring($Dir.Length + 1)) -replace '\\', '/')
    $out[$rel] = (Get-FileHash $_.FullName -Algorithm SHA256).Hash.ToLower()
  }
  return $out
}

function Get-TreeDiff($A, $B) {
  # Per-file comparison, so a mismatch names what moved instead of only saying "different".
  $keys = (($A.Keys + $B.Keys) | Sort-Object -Unique)
  $diff = @()
  foreach ($k in $keys) {
    if ($A[$k] -ne $B[$k]) { $diff += $k }
  }
  return , $diff
}

function Get-TreeClaim([string]$Dir) {
  # The number a report can quote: the handler and entry-point hashes, plus the file count.
  $h = Get-TreeHashes $Dir
  $claim = foreach ($rel in 'handlers/aog.ts', 'handlers/utils.ts', 'index.ts') {
    if ($h[$rel]) { "{0} {1}" -f (Split-Path $rel -Leaf), $h[$rel].Substring(0, 12) }
  }
  return ("{0} files, {1}" -f $h.Count, ($claim -join ', '))
}

function Test-Bootable([string]$Dir) {
  return (Test-Path (Join-Path $Dir 'AsphyxiaCore.js')) -and (Test-Path (Join-Path $Dir 'config.ini'))
}

function Resolve-LaneDir([string]$Name) {
  $dir = if ([System.IO.Path]::IsPathRooted($Name)) { $Name } else { Join-Path $CoreRoot $Name }
  # A retired lane's name is free, and -Action status tells you to re-stage it. Resolving it has to
  # work when the directory is gone, or the only way back to a lane is a hand copy of dist.
  $full = if (Test-Path $dir) { (Get-Item $dir).FullName } else { [System.IO.Path]::GetFullPath($dir) }
  if ($full -eq $Canonical) { throw "refusing to touch the canonical tree: $full" }
  if (-not $full.StartsWith($CoreRoot + '\')) { throw "lane must live under $CoreRoot : $full" }
  return $full
}

function Test-PortFree([int]$Port) {
  try {
    $c = Get-NetTCPConnection -State Listen -LocalPort $Port -ErrorAction SilentlyContinue
    if ($c) { return $false }
  } catch {
    $lines = (netstat -ano -p tcp) | Select-String (":" + $Port + "\s")
    if ($lines) { return $false }
  }
  return $true
}

function Copy-LaneRuntime([string]$Target) {
  # Everything the core needs to run, except the plugin (copied separately), its state and its logs.
  $skip = @('plugins', 'savedata', 'node_modules', 'config.ini', 'log.txt')
  Get-ChildItem $Canonical -Force | Where-Object {
    # No logs either: another lane's log.txt in a fresh lane reads like this lane's own output.
    ($skip -notcontains $_.Name) -and ($_.Name -notmatch '\.(log|out\.txt)$')
  } | ForEach-Object {
    Copy-Item $_.FullName -Destination $Target -Recurse -Force
  }
  New-Item -ItemType Directory -Force -Path (Join-Path $Target 'plugins') | Out-Null
  Copy-Item (Join-Path $Canonical 'plugins\tsconfig.json') -Destination (Join-Path $Target 'plugins') -Force
  Copy-Item (Join-Path $Canonical 'plugins\asphyxia-core.d.ts') -Destination (Join-Path $Target 'plugins') -Force
  Copy-Item $CanonicalPlugin -Destination (Join-Path $Target 'plugins') -Recurse -Force
  New-Item -ItemType Directory -Force -Path (Join-Path $Target 'savedata') | Out-Null
}

function Write-LaneConfig([string]$Target, [int]$Port, [int]$MfgHttpPort, [int]$MatchingPort, [string]$Tag) {
  $src = Get-Content (Join-Path $Canonical 'config.ini')
  $out = foreach ($line in $src) {
    if ($line -like 'port=*') { "port=$Port" }
    elseif ($line -like 'matching_port=*') { "matching_port=$MatchingPort" }
    elseif ($line -like 'mfg_http_port=*') { "mfg_http_port=$MfgHttpPort" }
    elseif ($line -like 'server_name=*') { "server_name=MFG lane $Tag" }
    elseif ($line -like 'server_tag=*') { "server_tag=$Tag" }
    # A lane that advertises 8083 sends its own clients to the other lane's build.
    elseif ($line -like 'mfg_service_url=*') { "mfg_service_url=http://127.0.0.1:$Port/aog" }
    else { $line }
  }
  Set-Content -Path (Join-Path $Target 'config.ini') -Value $out -Encoding utf8NoBOM
}

function Write-LaneStamp([string]$Target, [int]$Port, [int]$MfgHttpPort, [int]$MatchingPort, [string]$Tag) {
  $pluginDir = Join-Path $Target "plugins\$PluginName"
  $files = Get-TreeHashes $pluginDir
  $stamp = [ordered]@{
    lane             = (Split-Path $Target -Leaf)
    lane_dir         = $Target
    staged_from      = $CanonicalPlugin
    staged_at        = (Get-Date).ToUniversalTime().ToString('yyyy-MM-ddTHH:mm:ssZ')
    ports            = [ordered]@{ port = $Port; mfg_http_port = $MfgHttpPort; matching_port = $MatchingPort }
    server_tag       = $Tag
    plugin_files     = $files
  }
  $stamp | ConvertTo-Json -Depth 5 | Set-Content -Path (Join-Path $Target 'LANE_STAMP.json') -Encoding utf8NoBOM
  return $stamp
}

function Action-Status {
  Write-Host "canonical : $CanonicalPlugin"
  Write-Host ("          {0}" -f (Get-TreeClaim $CanonicalPlugin))
  $canonHashes = Get-TreeHashes $CanonicalPlugin
  foreach ($rel in $canonHashes.Keys) {
    if ($rel -match 'handlers/(aog|utils|eamuse)\.ts$|^index\.ts$|^data/') {
      Write-Host ("   {0,-34} {1}" -f $rel, $canonHashes[$rel].Substring(0, 12))
    }
  }

  $copies = Get-ChildItem 'C:\mfg\tmp' -Recurse -Filter aog.ts -File -ErrorAction SilentlyContinue
  Write-Host ""
  Write-Host "other copies of $PluginName in the tree:"
  $bad = 0
  $seen = @{}
  foreach ($f in $copies) {
    $plug = Split-Path (Split-Path $f.FullName -Parent) -Parent
    if ($plug -eq $CanonicalPlugin) { continue }
    if ($seen.ContainsKey($plug)) { continue }
    $seen[$plug] = $true
    $laneDir = Split-Path $plug -Parent           # ...\plugins
    $laneRoot = Split-Path $laneDir -Parent      # the directory a server is started from
    $diff = Get-TreeDiff $canonHashes (Get-TreeHashes $plug)
    $match = ($diff.Count -eq 0)
    $frozen = $laneRoot.StartsWith($FrozenRoot)
    $bootable = Test-Bootable $laneRoot
    $state = if ($match) { 'identical' } elseif ($frozen) { 'frozen snapshot' } else { 'DIVERGED' }
    $flag = if ((-not $match) -and $bootable -and (-not $frozen)) { '  <-- BOOTABLE STALE LANE' } else { '' }
    if ($flag) { $bad++ }
    $detail = if ($match) { '' } else { "   differs in: $(($diff | Select-Object -First 4) -join ', ')$(if ($diff.Count -gt 4) { ' +' + ($diff.Count - 4) + ' more' })" }
    Write-Host ("  {0,-11} bootable={1,-5} {2}{3}{4}" -f $state, $bootable, $plug, $flag, $detail)
  }
  Write-Host ""
  Write-Host ("bootable diverged copies: {0}" -f $bad)
  if ($bad -gt 0) { Write-Host 'fix: pwsh scripts\ryunet_stage.ps1 -Action lane   (re-stage) or -Action retire -Lane <name>'; exit 1 }
  Write-Host 'no divergent copy can boot a server.'
}

function Action-Lane {
  $target = Resolve-LaneDir $Lane
  foreach ($p in @($Port, $MfgHttpPort, $MatchingPort)) {
    if (-not (Test-PortFree $p)) { throw "port $p is already listening - pick lane ports nobody holds (pwsh .audit\server_status.ps1)" }
  }
  if (Test-Path $target) {
    if (-not (Test-Path (Join-Path $target 'LANE_STAMP.json')) -and -not (Test-Bootable $target)) {
      throw "refusing to wipe ${target} - it is not a staged lane"
    }
    Remove-Item $target -Recurse -Force
  }
  New-Item -ItemType Directory -Force -Path $target | Out-Null
  Copy-LaneRuntime $target
  Write-LaneConfig $target $Port $MfgHttpPort $MatchingPort $Tag
  $stamp = Write-LaneStamp $target $Port $MfgHttpPort $MatchingPort $Tag
  Write-Host "lane staged from the canonical tree:"
  Write-Host "  dir        $target"
  Write-Host ("  ports      port={0} mfg_http_port={1} matching_port={2}" -f $Port, $MfgHttpPort, $MatchingPort)
  Write-Host "  savedata   empty (its own DB, nothing inherited)"
  Write-Host ("  build      {0}" -f (Get-TreeClaim $CanonicalPlugin))
  Write-Host "  stamp      LANE_STAMP.json (the sweeps refuse to measure a lane without one)"
  Write-Host "start: cd $target; node AsphyxiaCore.js"
}

function Action-Mirror {
  $before = Get-TreeHashes $CanonicalPlugin
  if (Test-Path $MirrorPlugin) { Remove-Item $MirrorPlugin -Recurse -Force }
  New-Item -ItemType Directory -Force -Path (Split-Path $MirrorPlugin -Parent) | Out-Null
  Copy-Item $CanonicalPlugin -Destination (Split-Path $MirrorPlugin -Parent) -Recurse -Force
  $diff = Get-TreeDiff $before (Get-TreeHashes $MirrorPlugin)
  Write-Host "mirror refreshed FROM the canonical tree (one direction only):"
  Write-Host "  $MirrorPlugin"
  Write-Host ("  {0}" -f (Get-TreeClaim $MirrorPlugin))
  if ($diff.Count -gt 0) { Write-Host ("  MISMATCH in: {0}" -f ($diff -join ', ')); exit 1 }
  Write-Host "  identical to canonical"
}

function Action-Retire {
  if (-not $Note) { throw 'retire needs -Note "<which build / which report claim this evidences>"' }
  $source = Resolve-LaneDir $Lane
  if (-not (Test-Path $source)) { throw "no lane at $source" }
  New-Item -ItemType Directory -Force -Path $FrozenRoot | Out-Null
  $target = Join-Path $FrozenRoot (Split-Path $source -Leaf)
  if (Test-Path $target) { throw "snapshot already exists: $target" }
  Move-Item $source $target
  # Keep the evidence (plugin TypeScript + run outputs); remove what made it a runnable lane.
  foreach ($drop in @('AsphyxiaCore.js', 'AsphyxiaCore.js.map', 'config.ini', 'aog', 'discord',
                      'eamuse', 'middlewares', 'utils', 'webui', 'savedata', 'node_modules')) {
    $p = Join-Path $target $drop
    if (Test-Path $p) { Remove-Item $p -Recurse -Force }
  }
  $plug = Join-Path $target "plugins\$PluginName"
  $claim = if (Test-Path $plug) { Get-TreeClaim $plug } else { '(no plugin copy)' }
  $manifest = Join-Path $FrozenRoot 'FROZEN.md'
  if (-not (Test-Path $manifest)) {
    @(
      '# Frozen build snapshots',
      '',
      'These are read-only snapshots of a plugin build, kept so a report that quotes "the build as',
      'of HH:MM" can still be diffed. They are not lanes: there is no AsphyxiaCore.js, no config.ini',
      'and no savedata here, so nothing boots and nothing is probeable. Re-check a claim by diffing',
      'plugins/mfg@asphyxia against the canonical tree',
      '(tmp/RyuNET-core/dist/plugins/mfg@asphyxia, pwsh C:\mfg\scripts\ryunet_stage.ps1 -Action status),',
      'not by running anything against these directories.',
      '',
      '| snapshot | plugin build (handler / entry hashes) | frozen at | what it evidences |',
      '|---|---|---|---|'
    ) | Set-Content -Path $manifest -Encoding utf8NoBOM
  }
  $row = "| $(Split-Path $target -Leaf) | $claim | $((Get-Date).ToUniversalTime().ToString('yyyy-MM-dd HH:mm'))Z | $Note |"
  Add-Content -Path $manifest -Value $row -Encoding utf8NoBOM
  Write-Host "retired $source -> $target (core runtime, config.ini and savedata removed)"
  Write-Host "  $claim"
  Write-Host "  recorded in $manifest"
}

switch ($Action) {
  'status' { Action-Status }
  'lane'   { Action-Lane }
  'mirror' { Action-Mirror }
  'retire' { Action-Retire }
}

