# S2 arcade capture launcher
# Reads runtime identity from a non-committed file (default: install-root s2-local.txt
# with lines EA_URL=..., PCBID=..., CARD0=..., PIN=...) and launches the shipping
# build in arcade mode against a real provider, with the logging mod injected.
# Do NOT commit the identity file or this script's console output: they carry PCBID/card.
param(
  [string]$LocalFile = '\\MYAPPSANDBOX\c\Games\MFG\s2-local.txt',
  [string]$InstallRoot = '\\MYAPPSANDBOX\c\Games\MFG',
  # Where this launcher records the pids it starts: line 1 the pid, line 2 the command line as WMI sees
  # it - the hosting_up.ps1:38/63-67 record shape, in its own files under the kit's scratch root.
  # -PidDir is the same knob hosting_down.ps1:34 offers, so a test drives this script against scratch
  # records instead of the real ones. s2_run.ps1 and s3_run.ps1 share the directory and the 'game'
  # label: either launcher may stop the instance the other one recorded (see the ownership block below).
  [string]$PidDir = 'C:\mfgtmp\s23_pids',
  [switch]$NoMod,
  [switch]$NoApi
)
$cfg = @{}
Get-Content $LocalFile | ForEach-Object { $kv = $_ -split '=', 2; if ($kv.Count -eq 2) { $cfg[$kv[0].Trim()] = $kv[1].Trim() } }
foreach ($k in 'EA_URL','PCBID','CARD0') { if (-not $cfg[$k]) { throw "s2-local.txt missing key $k" } }

# Process ownership (the rule hosting_down.ps1:8-15 and :60-87 settled for the hosted lane, and
# preview_game.ps1:96-135 for the preview window): signal only a pid this kit recorded itself, and
# re-prove the process before signalling - the image name AND the command line recorded at launch, which
# on Windows never changes after creation. This block used to read `Stop-Process -Name spice64 -Force`,
# i.e. every spice64.exe on the box: an instance the operator started by hand and the instance
# scripts/s3_run.ps1 is holding share the binary name and are not this launcher's to signal. So:
#   own record proved            -> that pid stopped, record removed;
#   own record unprovable        -> REFUSED, record KEPT (deleting it strands a live process with no
#                                   handle and the next start stacks a second cabinet client on it);
#   live process, no record here -> reported and left running, and this launcher refuses to start a
#                                   second game instance on top of it instead of killing what it
#                                   cannot prove. A name scan is used to REFUSE, never to choose a target.
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
    $lines = @(Get-Content -LiteralPath $file -EA SilentlyContinue | ForEach-Object { "$_".Trim() })
    $rawPid = if ($lines.Count -gt 0) { $lines[0] } else { '' }
    $recordedCmd = if ($lines.Count -gt 1) { ($lines[1..($lines.Count - 1)] -join ' ').Trim() } else { '' }
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
    "${label}: stopped $expectName pid=$kidPid (the instance this launcher recorded); record removed"
    Remove-Item -LiteralPath $file -Force -EA SilentlyContinue
}

function Stop-OwnKid($label, $exeBaseName) {
    # Stop this launcher's recorded child, then refuse to continue while a process of that image is still
    # running: the remaining one was not started by this run, so it is not ours to signal, and starting a
    # second cabinet client next to it is the failure this whole block exists to avoid.
    Stop-RecordedKid $label ($exeBaseName + '.exe')
    foreach ($f in @(Get-Process -Name $exeBaseName -EA SilentlyContinue)) {
        $cl = (Get-CimInstance Win32_Process -Filter "ProcessId=$($f.Id)" -EA SilentlyContinue).CommandLine
        "${label}: pid $($f.Id) ($exeBaseName.exe) [$cl] is already running and was not started by this run (no provable record in $PidDir)"
        "${label}: NOT signalled - this launcher never kills by name - and no second instance started: check what that process is, stop it yourself (Stop-Process -Id $($f.Id)), then run this again"
        $script:kidRefused += $label
    }
    if ($script:kidRefused.Count -gt 0) { "${label}: refusing to start a new instance while an unowned process is running"; exit 1 }
}

Stop-OwnKid 'game' 'spice64'

$argv = @('-url', $cfg['EA_URL'], '-p', $cfg['PCBID'], '-card0', $cfg['CARD0'], '-w', '-windowborder', '2')
if (-not $NoApi)  { $argv += @('-api', '1337') }
if (-not $NoMod)  { $argv += @('-k', 'doorstop.dll') }
# -netdump is deliberately absent: it was implicated in process crashes during S2.

Remove-Item "$InstallRoot\dev\raw\log\output_log.txt" -ErrorAction SilentlyContinue
Remove-Item "$InstallRoot\BepInEx\LogOutput.log" -ErrorAction SilentlyContinue

$p = Start-Process -FilePath "$InstallRoot\spice64.exe" -ArgumentList $argv -WorkingDirectory $InstallRoot -PassThru
Write-KidPid 'game' $p
"game pid=$($p.Id) (args kept in this process only)"

$logFile = "$InstallRoot\dev\raw\log\output_log.txt"
for ($i = 0; $i -lt 24; $i++) {
  Start-Sleep -Seconds 25
  if (-not (Get-Process -Id $p.Id -ErrorAction SilentlyContinue)) { "poll $i : PROCESS GONE"; break }
  $hit = Get-Content $logFile -ErrorAction SilentlyContinue | Select-String 'cardmng.inquire' | Select-Object -First 1
  if ($hit) { 'poll {0} : card inquire seen - cabinet is on the card flow' -f $i; break }
}
'tidy up later: the next run of this launcher stops the instance it recorded (pid in ' + $PidDir + '); a game it did not start is never closed.'
'next manual steps: F1 (credit) -> tap attract -> insert card handled by -card0/-api -> PIN on scrambled pad -> login chain.'

