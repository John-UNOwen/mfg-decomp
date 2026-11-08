# S3 live-test launcher: play the shipping build against the repo mock front server
# in KONAMINET mode, with the stateful match engine answering gget/gpost.
#
#   1. refuses to touch a game or mock process it did not start (see the ownership block below)
#   2. pins proxy-jp100.konami + aog03-stg.pvt.mo.konami.net to loopback (reverts with -Unpin)
#   3. starts mock/bin MockServer.exe on 127.0.0.1:8080 (log: mock/mock.log.txt)
#   4. launches -KONAMINET with the BepInEx doorstop mod
#   5. polls output_log.txt for LoadScene-->Title, then prints the click runbook
#
# Usage: pwsh -File scripts/s3_run.ps1 [-Unpin] [-NoMod]
param(
  [string]$InstallRoot = '\\MYAPPSANDBOX\c\Games\MFG',
  # Where this launcher records the pids it starts (pid on line 1, the command line as WMI sees it on
  # line 2 - the hosting_up.ps1:38/63-67 shape), in its own files under the kit's scratch root. Shared
  # with s2_run.ps1, including the 'game' label, so either launcher can stop the instance the other one
  # recorded. -PidDir / -MockExe / -HostsFile exist so a test can drive this script against a scratch
  # record dir, a scratch mock and a scratch hosts file instead of the real ones - the same reason
  # hosting_down.ps1:34-38 takes -PidDir and -CorePort.
  [string]$PidDir = 'C:\mfgtmp\s23_pids',
  [string]$MockExe = '',
  [string]$HostsFile = 'C:\Windows\System32\drivers\etc\hosts',
  [switch]$Unpin,
  [switch]$NoMod
)
$ErrorActionPreference = 'Stop'
$repo = Split-Path -Parent $PSScriptRoot
$hostsPath = $HostsFile
$lines = @('127.0.0.1 proxy-jp100.konami', '127.0.0.1 aog03-stg.pvt.mo.konami.net')
if (-not $MockExe) { $MockExe = Join-Path $repo 'mock\bin\Debug\net8.0\MockServer.exe' }

# Process ownership (the rule hosting_down.ps1:8-15 and :60-87 settled for the hosted lane, and
# preview_game.ps1:96-135 for the preview window): signal only a pid this kit recorded itself, and
# re-prove the process before signalling - the image name AND the command line recorded at launch, which
# on Windows never changes after creation. This launcher used to sweep the machine twice: `Stop-Process
# -Name spice64 -Force` (both launchers share one cabinet binary) and `Get-Process -Name MockServer |
# Stop-Process -Force` in -Unpin, which also reached a mock someone else started. A name filter is not an
# identity:
#   own record proved            -> that pid stopped, record removed;
#   own record unprovable        -> REFUSED, record KEPT (deleting it strands a live process with no
#                                   handle and the next start stacks a second server on its port);
#   live process, no record here -> reported and left running, and this launcher refuses to start its
#                                   own copy next to it. The name scan is used to REFUSE, never to
#                                   choose a signal target.
# The checks run BEFORE the hosts pins are written, so a refusal never leaves this machine pointing the
# konami hosts names at loopback with nothing serving them.
if (-not (Test-Path -LiteralPath $PidDir)) { New-Item -ItemType Directory -Path $PidDir | Out-Null }
$kidRefused = @()

function Write-KidPid($label, $proc) {
    # hosting_up.ps1:63-67 / preview_game.ps1 Write-PreviewPid:182-190 shape. Written the moment the child
    # starts: a record made after a later failure would leave that child unowned.
    $rec = Get-CimInstance Win32_Process -Filter "ProcessId=$($proc.Id)" -EA SilentlyContinue
    Set-Content -LiteralPath (Join-Path $PidDir ($label + '.pid')) -Value @("$($proc.Id)", [string]$rec.CommandLine) -Encoding utf8
    'recorded ' + $label + ' pid ' + $proc.Id + ' in ' + $PidDir
}

function Stop-RecordedKid($label, $expectName) {
    # The shipped identity rule (hosting_down.ps1 Test-LaneIdentity:60-87; for a process with no port of
    # its own to prove, preview_game.ps1 Stop-PreviewWindow:96-135): image name plus the command line
    # recorded when this kit started it. A pid-only record is refused here - hosting_down.ps1 can accept
    # one because hosting_deploy.md documents adopting a bare pid, this launcher has no adoption path, so
    # it keeps the stricter preview_game.ps1:229-231 rule.
    $file = Join-Path $PidDir ($label + '.pid')
    if (-not (Test-Path -LiteralPath $file)) { "${label}: no record ($file); nothing recorded, nothing signalled"; return }
    $recLines = @(Get-Content -LiteralPath $file -EA SilentlyContinue | ForEach-Object { "$_".Trim() })
    $rawPid = if ($recLines.Count -gt 0) { $recLines[0] } else { '' }
    $recordedCmd = if ($recLines.Count -gt 1) { ($recLines[1..($recLines.Count - 1)] -join ' ').Trim() } else { '' }
    if ($rawPid -notmatch '^\d+$') { "${label}: REFUSED - record $file holds no plain pid ('$rawPid'); not signalled, record KEPT"; $script:kidRefused += $label; return }
    $kidPid = [int]$rawPid
    if ($kidPid -le 4) { "${label}: REFUSED - record holds pid $kidPid; system pids are never signalled, record KEPT"; $script:kidRefused += $label; return }
    $parentPid = (Get-CimInstance Win32_Process -Filter "ProcessId=$PID" -EA SilentlyContinue).ParentProcessId
    if ($kidPid -eq $PID -or ($parentPid -and $kidPid -eq [int]$parentPid)) { "${label}: REFUSED - record holds this script's pid or its parent's ($kidPid); record KEPT"; $script:kidRefused += $label; return }
    $p = Get-CimInstance Win32_Process -Filter "ProcessId=$kidPid" -EA SilentlyContinue
    if (-not $p) { "${label}: pid $kidPid is not running; stale record $file removed"; Remove-Item -LiteralPath $file -Force -EA SilentlyContinue; return }
    $why = $null
    if ($p.Name -ne $expectName) { $why = "$($p.Name) is not $expectName" }
    elseif (-not $recordedCmd) { $why = 'the record carries no command line, so nothing in it proves this pid is still the process the record named' }
    elseif ("$($p.CommandLine)" -cne $recordedCmd) {
        $why = "the live command line is not the one recorded when this child started - a recycled pid is not this child`n         recorded: $recordedCmd`n         live:     $($p.CommandLine)"
    }
    if ($why) {
        "${label}: pid $kidPid is $($p.Name) [$($p.CommandLine)] - $why; NOT signalled, record KEPT"
        "${label}: (deleting the record would strand a live process with no handle - hosting_down.ps1:28-29)"
        $script:kidRefused += $label; return
    }
    $stopErr = $null
    try { Stop-Process -Id $kidPid -Force -ErrorAction Stop } catch { $stopErr = $_.Exception.Message }
    if ($stopErr) { "${label}: REFUSED - could not signal pid $kidPid ($stopErr); record KEPT"; $script:kidRefused += $label; return }
    $gone = $false
    for ($i = 0; $i -lt 20; $i++) {
        if (-not (Get-Process -Id $kidPid -EA SilentlyContinue)) { $gone = $true; break }
        Start-Sleep -Milliseconds 300
    }
    if (-not $gone) { "${label}: pid $kidPid survived Stop-Process -Force; investigate what it is blocking on; record KEPT"; $script:kidRefused += $label; return }
    "${label}: stopped $expectName pid=$kidPid (the process this launcher recorded); record removed"
    Remove-Item -LiteralPath $file -Force -EA SilentlyContinue
}

function Stop-OwnKid($label, $exeBaseName, $when) {
    # Stop this launcher's recorded child, then refuse to continue while a process of that image is still
    # running: the one left was not started by this run, so it is not ours to signal, and starting our own
    # copy next to it (a second mock on 8080 would fail to bind, a second cabinet client would fight for
    # the card reader) is the failure this block exists to avoid.
    Stop-RecordedKid $label ($exeBaseName + '.exe')
    foreach ($f in @(Get-Process -Name $exeBaseName -EA SilentlyContinue)) {
        $cl = (Get-CimInstance Win32_Process -Filter "ProcessId=$($f.Id)" -EA SilentlyContinue).CommandLine
        "${label}: pid $($f.Id) ($exeBaseName.exe) [$cl] is already running and was not started by this run (no provable record in $PidDir)"
        "${label}: NOT signalled - this launcher never kills by name - and no replacement started: check what that process is, stop it yourself (Stop-Process -Id $($f.Id)), then run this again"
        $script:kidRefused += $label
    }
    if ($script:kidRefused.Count -gt 0) { "${label}: $when refused - an unowned $exeBaseName.exe is still running"; exit 1 }
}

if ($Unpin) {
  (Get-Content $hostsPath) | Where-Object { $lines -notcontains $_.Trim() } | Set-Content $hostsPath -Encoding ascii
  'hosts pins removed; stopping the mock this launcher recorded'
  Stop-OwnKid 'mock' 'MockServer' '-Unpin'
  return
}

# Ownership first: nothing is mutated (hosts file, mock, game) until every process that is already there
# has been proved to be this launcher's own child.
Stop-OwnKid 'mock' 'MockServer' 'start'
Stop-OwnKid 'game' 'spice64' 'start'

foreach ($entry in $lines) {
  if (-not (Select-String -Path $hostsPath -SimpleMatch $entry -Quiet)) { Add-Content -Path $hostsPath -Value $entry -Encoding ascii }
}
'hosts pinned (S1 wiring: docs/12)'

# mock front server
if (-not (Test-Path $MockExe)) { throw "build the mock first: dotnet build mock  ($MockExe missing)" }
$mockProc = Start-Process -FilePath $MockExe -WorkingDirectory (Split-Path -Parent $MockExe) -WindowStyle Hidden -PassThru
Write-KidPid 'mock' $mockProc
Start-Sleep -Seconds 2
'mock started on 127.0.0.1:8080 (pid ' + $mockProc.Id + ', mock/mock.log.txt)'

# game
$argv = @('-w', '-windowborder', '2', '-KONAMINET', '-api', '1337')
if (-not $NoMod) { $argv += @('-k', 'doorstop.dll') }

Remove-Item "$InstallRoot\dev\raw\log\output_log.txt" -ErrorAction SilentlyContinue
Remove-Item "$InstallRoot\BepInEx\LogOutput.log" -ErrorAction SilentlyContinue

$p = Start-Process -FilePath "$InstallRoot\spice64.exe" -ArgumentList $argv -WorkingDirectory $InstallRoot -PassThru
Write-KidPid 'game' $p
"game pid=$($p.Id)"

$logFile = "$InstallRoot\dev\raw\log\output_log.txt"
for ($i = 0; $i -lt 20; $i++) {
  Start-Sleep -Seconds 15
  if (-not (Get-Process -Id $p.Id -ErrorAction SilentlyContinue)) { 'PROCESS GONE (do not tap; check logs)'; break }
  $hit = Get-Content $logFile -ErrorAction SilentlyContinue | Select-String 'LoadScene-->Title' | Select-Object -First 1
  if ($hit) { 'TITLE READY - safe to tap'; break }
}
'next: this launcher stops the mock/game it recorded (pids in ' + $PidDir + ') the next time it runs; a process it did not start is never touched.'
'runbook: tap attract 960,1010 -> guest start 1349,932 -> story skip-all 1765,921 then confirm 764,569' +
  ' -> char select decide 1270,975 -> lobby; match: tap 対局 1080,685 -> 対局開始 1385,330 -> payment normal-play row.'

