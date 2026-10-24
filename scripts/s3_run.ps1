# S3 live-test launcher: play the shipping build against the repo mock front server
# in KONAMINET mode, with the stateful match engine answering gget/gpost.
#
#   1. pins proxy-jp100.konami + aog03-stg.pvt.mo.konami.net to loopback (reverts with -Unpin)
#   2. starts mock/bin MockServer.exe on 127.0.0.1:8080 (log: mock/mock.log.txt)
#   3. kills any running spice64, launches -KONAMINET with the BepInEx doorstop mod
#   4. polls output_log.txt for LoadScene-->Title, then prints the click runbook
#
# Usage: pwsh -File scripts/s3_run.ps1 [-Unpin] [-NoMod]
param(
  [string]$InstallRoot = '\\MYAPPSANDBOX\c\Games\MFG',
  [switch]$Unpin,
  [switch]$NoMod
)
$ErrorActionPreference = 'Stop'
$repo = Split-Path -Parent $PSScriptRoot
$hostsPath = 'C:\Windows\System32\drivers\etc\hosts'
$lines = @('127.0.0.1 proxy-jp100.konami', '127.0.0.1 aog03-stg.pvt.mo.konami.net')

if ($Unpin) {
  (Get-Content $hostsPath) | Where-Object { $lines -notcontains $_.Trim() } | Set-Content $hostsPath -Encoding ascii
  'hosts pins removed; stopping mock'
  Get-Process -Name MockServer -ErrorAction SilentlyContinue | Stop-Process -Force
  return
}

foreach ($entry in $lines) {
  if (-not (Select-String -Path $hostsPath -SimpleMatch $entry -Quiet)) { Add-Content -Path $hostsPath -Value $entry -Encoding ascii }
}
'hosts pinned (S1 wiring: docs/12)'

# mock front server
if (-not (Get-Process -Name MockServer -ErrorAction SilentlyContinue)) {
  $mockExe = Join-Path $repo 'mock\bin\Debug\net8.0\MockServer.exe'
  if (-not (Test-Path $mockExe)) { throw "build the mock first: dotnet build mock  ($mockExe missing)" }
  Start-Process -FilePath $mockExe -WorkingDirectory (Split-Path -Parent $mockExe) -WindowStyle Hidden
  Start-Sleep -Seconds 2
  'mock started on 127.0.0.1:8080 (mock/mock.log.txt)'
} else { 'mock already running' }

# game
$gone = Get-Process -Name spice64 -ErrorAction SilentlyContinue
if ($gone) { "stopping existing game: $($gone.Id -join ',')"; Stop-Process -Name spice64 -Force; Start-Sleep -Seconds 4 }

$argv = @('-w', '-windowborder', '2', '-KONAMINET', '-api', '1337')
if (-not $NoMod) { $argv += @('-k', 'doorstop.dll') }

Remove-Item "$InstallRoot\dev\raw\log\output_log.txt" -ErrorAction SilentlyContinue
Remove-Item "$InstallRoot\BepInEx\LogOutput.log" -ErrorAction SilentlyContinue

$p = Start-Process -FilePath "$InstallRoot\spice64.exe" -ArgumentList $argv -WorkingDirectory $InstallRoot -PassThru
"game pid=$($p.Id)"

$logFile = "$InstallRoot\dev\raw\log\output_log.txt"
for ($i = 0; $i -lt 20; $i++) {
  Start-Sleep -Seconds 15
  if (-not (Get-Process -Id $p.Id -ErrorAction SilentlyContinue)) { 'PROCESS GONE (do not tap; check logs)'; break }
  $hit = Get-Content $logFile -ErrorAction SilentlyContinue | Select-String 'LoadScene-->Title' | Select-Object -First 1
  if ($hit) { 'TITLE READY - safe to tap'; break }
}
'runbook: tap attract 960,1010 -> guest start 1349,932 -> story skip-all 1765,921 then confirm 764,569' +
  ' -> char select decide 1270,975 -> lobby; match: tap 対局 1080,685 -> 対局開始 1385,330 -> payment normal-play row.'
