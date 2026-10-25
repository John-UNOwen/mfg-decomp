# Vendores UPM packages (+ transitive deps) from packages.unity.com into
# build/unity-project/Packages as embedded packages, so the editor resolves them offline.
$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'
$repo = (Resolve-Path "$PSScriptRoot\..").Path
$proj = Join-Path $repo 'build\unity-project'
$pkgsDir = Join-Path $proj 'Packages'
$manifestPath = Join-Path $pkgsDir 'manifest.json'
$cache = Join-Path $repo 'tmp\upm-cache'
New-Item -ItemType Directory -Force $cache | Out-Null
$ProgressPreference = 'SilentlyContinue'

$manifest = Get-Content $manifestPath -Raw | ConvertFrom-Json
$wanted = [ordered]@{}
foreach ($p in $manifest.dependencies.PSObject.Properties) {
  if ($p.Name -like 'com.unity.modules.*') { continue }
  $wanted[$p.Name] = $p.Value
}

$installed = [ordered]@{}
$resolved  = [ordered]@{}

function Resolve-Version([string]$name, [string]$spec) {
  if ($resolved.Contains($name) -and $resolved[$name] -eq $spec) { return $resolved[$name] }
  $meta = (Invoke-WebRequest "https://packages.unity.com/$name" -UserAgent 'curl/8' -UseBasicParsing -TimeoutSec 60).Content | ConvertFrom-Json
  $names = @($meta.versions.PSObject.Properties.Name)
  $pick = $null
  if ($names -contains $spec) { $pick = $spec }
  elseif ($spec -match '^(\d+)\.x$') { $maj = $Matches[1]; $cands = @($names | Where-Object { $_ -match "^$maj\." }); if ($cands) { $pick = $cands[-1] } }
  else {
    # caret-ish or range: take highest exact-prefix, else highest with same major
    $cands = @($names | Where-Object { $_ -like "$($spec.TrimStart('^','>','='))*)*" })
    if (-not $cands) { $maj = ($spec -replace '[^\d.].*$','') -replace '\.\d+$',''; $cands = @($names | Where-Object { $_ -match "^$maj\." }) }
    if ($cands) { $pick = $cands[-1] } else { $pick = $names[-1] }
  }
  $resolved[$name] = $pick
  return $pick
}

$queue = New-Object System.Collections.Generic.Queue[object]
foreach ($k in $wanted.Keys) { $queue.Enqueue(@($k, $wanted[$k])) }
while ($queue.Count -gt 0) {
  $item = $queue.Dequeue()
  $name = $item[0]; $spec = $item[1]
  if ($name -like 'com.unity.modules.*' -or $installed.Contains($name)) { continue }
  $ver = Resolve-Version $name $spec
  $tgz = Join-Path $cache "$name-$ver.tgz"
  if (-not (Test-Path $tgz)) {
    $meta = (Invoke-WebRequest "https://packages.unity.com/$name" -UserAgent 'curl/8' -UseBasicParsing -TimeoutSec 60).Content | ConvertFrom-Json
    $ball = $meta.versions.$ver.dist.tarball
    try { Invoke-WebRequest $ball -OutFile $tgz -UserAgent 'curl/8' -TimeoutSec 300 }
    catch { Invoke-WebRequest "https://packages.unity.com/$name/-/$($name.Split('/')[-1])-$ver.tgz" -OutFile $tgz -UserAgent 'curl/8' -TimeoutSec 300 }
  }
  $stage = Join-Path $cache "x-$name"
  if (Test-Path $stage) { Remove-Item $stage -Recurse -Force }
  New-Item -ItemType Directory -Force $stage | Out-Null
  tar -xzf $tgz -C $stage
  $pkgRoot = Join-Path $stage 'package'
  $pj = Get-Content (Join-Path $pkgRoot 'package.json') -Raw | ConvertFrom-Json
  $dest = Join-Path $pkgsDir $name
  if (Test-Path $dest) { Remove-Item $dest -Recurse -Force }
  Move-Item $pkgRoot $dest
  $installed[$name] = "$($pj.version) (spec $spec)"
  foreach ($d in $pj.dependencies.PSObject.Properties) { $queue.Enqueue(@($d.Name, $d.Value)) }
}

# embedded packages live in Packages/<name>; strip their registry pins from the manifest
foreach ($n in $installed.Keys) { $manifest.dependencies.PSObject.Properties.Remove($n) }
$manifest | ConvertTo-Json -Depth 8 | Set-Content $manifestPath -Encoding ascii
Get-Content $manifestPath -Raw | ConvertFrom-Json | Out-Null  # re-parse guard

'=== vendored ==='
foreach ($k in $installed.Keys) { "{0,-42} {1}" -f $k, $installed[$k] }
$size = (Get-ChildItem $pkgsDir -Recurse -File | Measure-Object Length -Sum).Sum
'total Packages dir: {0:N1} MB, manifest entries left: {1}' -f ($size/1MB), @($manifest.dependencies.PSObject.Properties).Count
