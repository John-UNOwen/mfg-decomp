# Standalone player smoke: boot the built player against the offline mock front,
# watch boot markers in the redirected player stdout and request traffic in the mock log.
# Default args = gate-2 config: -KONAMINET + mock FRONTURL + -GAMELOG + -NOIO + NETCAFE behavior.
# With -AUTOSERVE in $ArgsExtra the in-player AutoServeBehaviour drives the UI (see
# scripts/runtime-harness/AutoServeBehaviour.cs; rules live in C:\mfgtmp\autocode.cfg).
# usage: pwsh -File scripts\smoke_player.ps1 [-Exe <path>] [-TimeoutSec 240] [-ArgsExtra @()]
param(
  [string]$Exe = 'C:\mfg\build\unity-project\out\standalone\MFG.exe',
  [int]$TimeoutSec = 240,
  [string[]]$ArgsExtra = @()
)
$ProgressPreference = 'SilentlyContinue'
$repoRoot = (Get-Location).Path
$mockLog = Join-Path $repoRoot 'mock\mock.log.txt'
$stdout = 'C:\mfgtmp\player_stdout.log'
Remove-Item $stdout -EA SilentlyContinue
$plArgs = @('-KONAMINET', '-FRONTURL', 'http://127.0.0.1:8080/aog', '-GAMELOG', '-NOIO', '-appBehavior', 'NETCAFE', '-popupwindow') + $ArgsExtra
$exeDir = [IO.Path]::GetDirectoryName($Exe)
if (-not $exeDir) { $exeDir = (Get-Location).Path }
$proc = Start-Process -FilePath $Exe -WorkingDirectory $exeDir -ArgumentList $plArgs -RedirectStandardOutput $stdout -PassThru
"player pid=$($proc.Id)"
$deadline = (Get-Date).AddSeconds($TimeoutSec)
$markers = @('BootSceneFlow', 'ResidnetFlow:Start1', 'ResidnetFlow:Initialize8')
$hit = ''
$seen = @()
while ((Get-Date) -lt $deadline) {
  Start-Sleep -Milliseconds 800
  $t = Get-Content $stdout -Raw -EA SilentlyContinue
  if ($t) {
    foreach ($m in $markers) { if ($t.Contains($m)) { $hit = $m } }
    $mockHit = Test-Path $mockLog
    if ($hit -or $proc.HasExited) { break }
  }
}
"last marker: $(if ($hit) { $hit } else { 'none' })"
'--- player stdout: game lines ---'
if (Test-Path $stdout) {
  Get-Content $stdout | Where-Object { $_ -match 'ResidnetFlow|Boot|Mission|LoadScene|args:|FRONTURL|Exception|Error' } |
    Select-Object -First 40 | ForEach-Object { $_.Substring(0, [Math]::Min(130, $_.Length)) }
  '--- stdout tail ---'
  Get-Content $stdout -Tail 12 | ForEach-Object { $_.Substring(0, [Math]::Min(130, $_.Length)) }
}
'--- mock tail ---'
if (Test-Path $mockLog) { Get-Content $mockLog -Tail 8 | ForEach-Object { $_.Substring(0, [Math]::Min(130, $_.Length)) } } else { 'no mock log' }
"alive: $(-not $proc.HasExited) pid=$($proc.Id)"
