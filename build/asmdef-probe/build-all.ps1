param(
  [string]$Profile = 'shipped',
  [string]$Projects = '',
  [string]$LogDir = 'C:\Users\ <you>\Documents\GitHub\mfg-decomp\tmp',
  [string]$Extra = '',
  [string]$Configuration = 'Debug',
  [switch]$NoClean = $false
)

$ErrorActionPreference = 'Continue'
Set-Location 'C:\Users\ <you>\Documents\GitHub\mfg-decomp\build\asmdef-probe'

if ($Projects -eq '') {
  $Projects = 'Shirahama.csproj,Shirahama.Async.csproj,Shirahama.Rx.csproj,VirtualSurround.csproj,Win10actLog-Runtime.csproj,kamunity.csproj,Assembly-CSharp-firstpass.csproj,Assembly-CSharp.csproj'
}

$extraArgs = @()
if ($Extra -ne '') { $extraArgs = $Extra -split ' ' }

# A profile changes the framework reference set and the restore graph (only the pack
# profile pulls a targeting-pack package), so obj/bin are wiped per profile: a profile
# run that reuses another profile's restore or up-to-date state reports stale results.
if (-not $NoClean) {
  Remove-Item -LiteralPath "$PWD\obj", "$PWD\bin" -Recurse -Force -ErrorAction SilentlyContinue
}

$summary = @()
foreach ($proj in ($Projects -split ',')) {
  $tag = ($Profile -replace '[^a-z0-9]', '') + '-' + ($Configuration -replace '[^a-zA-Z]', '').ToLower() + ($(if ($Extra -ne '') { '-extra' } else { '' }))
  $log = Join-Path $LogDir ("build-$tag-$(($proj -replace '\.csproj$', '')).log")
  $out = dotnet build $proj -nologo -v:q -clp:ErrorsOnly -p:ProbeProfile=$Profile -p:Configuration=$Configuration @extraArgs -tl:false 2>&1
  $code = $LASTEXITCODE
  $out | Set-Content $log -Encoding utf8
  $errs = @($out | Where-Object { $_ -match ': error ' } | Sort-Object -Unique)
  $warns = @($out | Where-Object { $_ -match ': warning ' } | Sort-Object -Unique)
  $line = ('{0,-32} profile={1,-9} config={2,-7} exit={3} errors={4,3} warnings={5,3}' -f $proj, $Profile, $Configuration, $code, $errs.Count, $warns.Count)
  Write-Output $line
  $summary += [pscustomobject]@{ Project = $proj; Exit = $code; Errors = $errs.Count; Warnings = $warns.Count; Log = $log }
}

Write-Output ''
Write-Output '=== error code histogram (all projects, unique error lines) ==='
$all = @()
foreach ($s in $summary) { $all += (Get-Content $s.Log | Where-Object { $_ -match ': error ' }) }
$all = @($all | Sort-Object -Unique)
$h = @{}
foreach ($e in $all) {
  $c = [regex]::Match($e, 'error (CS\d+)').Groups[1].Value
  if ($h.ContainsKey($c)) { $h[$c] += 1 } else { $h[$c] = 1 }
}
$h.GetEnumerator() | Sort-Object Value -Descending | ForEach-Object { '{0,4}  {1}' -f $_.Value, $_.Key }

Write-Output ''
Write-Output '=== distinct error messages with counts ==='
$msgs = @{}
foreach ($e in $all) {
  $m = [regex]::Match($e, ': error (CS\d+): (.*)$')
  if ($m.Success) {
    $k = $m.Groups[1].Value + ' :: ' + ($m.Groups[2].Value -replace ' \[C:.*$', '')
    if ($msgs.ContainsKey($k)) { $msgs[$k] += 1 } else { $msgs[$k] = 1 }
  }
}
$msgs.GetEnumerator() | Sort-Object Value -Descending | ForEach-Object { '{0,4}  {1}' -f $_.Value, $_.Key.Substring(0, [Math]::Min(150, $_.Key.Length)) } | Select-Object -First 40

Write-Output ''
Write-Output '=== outputs produced ==='
Get-ChildItem 'C:\Users\ <you>\Documents\GitHub\mfg-decomp\build\asmdef-probe\bin' -Recurse -Filter '*.dll' |
  ForEach-Object { '{0,-34} {1,9}  {2}  {3}' -f $_.Name, $_.Length, $_.LastWriteTime.ToString('HH:mm:ss'), $_.Directory.FullName.Replace('C:\Users\ <you>\Documents\GitHub\mfg-decomp\build\asmdef-probe\bin\Debug\', '') }
