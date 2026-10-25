# Standalone player smoke: boot the built player against the offline mock front,
# watch boot markers in the Unity player log and request traffic in the mock log.
# usage: pwsh -File scripts\smoke_player.ps1 [-Exe <path>] [-TimeoutSec 180]
param(
  [string]$Exe = 'C:\mfg\build\unity-project\out\standalone\MFG.exe',
  [int]$TimeoutSec = 180
)
$ProgressPreference = 'SilentlyContinue'
$pl = Join-Path $env:USERPROFILE 'AppData\LocalLow\DefaultCompany\MFGClient\Player.log'
Remove-Item $pl -EA SilentlyContinue
$repoRoot = (Get-Location).Path
$mockLog = Join-Path $repoRoot 'mock\bin\Release\net8.0\mock.log.txt'
$stdout = 'C:\mfgtmp\player_stdout.log'
Remove-Item $stdout -EA SilentlyContinue
$proc = Start-Process -FilePath $Exe -ArgumentList '-KONAMINET', '-FRONTURL', 'http://127.0.0.1:8080/aog', '-popupwindow', '-screen-width', '1280', '-screen-height', '720' -RedirectStandardOutput $stdout -PassThru
"player pid=$($proc.Id)"
$deadline = (Get-Date).AddSeconds($TimeoutSec)
$bootMarkers = @('ResidnetFlow:Initialize8', 'ResidnetFlow:Start1', 'BootSceneFlow')
$hit = ''
while ((Get-Date) -lt $deadline) {
  Start-Sleep -Milliseconds 800
  if (Test-Path $pl) {
    $t = Get-Content $pl -Raw -EA SilentlyContinue
    if ($t) {
      foreach ($m in $bootMarkers) { if ($t.Contains($m)) { $hit = $m; break } }
    }
  }
  if ($hit -or $proc.HasExited) { break }
}
"last marker: $(if ($hit) { $hit } else { 'none' })"
if (Test-Path $pl) { '--- player log tail ---'; Get-Content $pl -Tail 25 | ForEach-Object { $_.Substring(0, [Math]::Min(130, $_.Length)) } } else { 'no player log written' }
'--- mock tail ---'
if (Test-Path $mockLog) { Get-Content $mockLog -Tail 6 | ForEach-Object { $_.Substring(0, [Math]::Min(130, $_.Length)) } } else { 'no mock log' }
'--- stdout tail ---'
if (Test-Path $stdout) { Get-Content $stdout -Tail 8 }
"alive: $(-not $proc.HasExited)"
'--- out dir ---'
Get-ChildItem (Split-Path $Exe) -EA SilentlyContinue | Select-Object -First 8 | ForEach-Object Name
