# Clean shutdown for the hosted lane (docs/28): core, gate and tunnel.
#
# It stops exactly the pids scripts/hosting_up.ps1 recorded in -PidDir (core.pid, gate.pid,
# tunnel.pid), and it re-proves each recorded pid is still the component its record names before any
# signal is sent. Same contract as scripts/linux/lane_down.sh, which replaced this shape on the
# Linux side.
#
# What this replaces (checkup 2026-11-06 item 14): the script used to enumerate every process on the
# machine and kill whatever matched a process attribute - WebFront.exe, cloudflared.exe, or node.exe
# whose command line merely CONTAINED 'AsphyxiaCore.js'. A substring of a command line is not an
# identity. Measured with the shipping predicate run verbatim (filter only, no Stop-Process), the
# match set held the real core plus an unrelated node runner that owns no lane port plus a scratch
# node whose argument only mentioned the filename, and the two name filters matched every WebFront
# and every cloudflared on the box - the owner's other lane or another tunnel included. Nothing is
# ever chosen by name, pattern or port ownership any more: only the records written at lane start.
#
# Record format (written by hosting_up.ps1): line 1 = the pid, line 2 = that process's command line
# as WMI sees it at start. A Windows process command line never changes after creation, so the
# recorded line is an exact proof at stop time; lane_down.sh needs a working-directory fallback
# because AsphyxiaCore rewrites the node process title, and that rewrite cannot alter the command
# line Windows reads here.
#
# Per entry:
#   no record            -> reported; nothing recorded, nothing signalled
#   malformed record     -> REFUSED, record kept
#   pid 0-4 / this script's own pid or its parent -> REFUSED, record kept
#   pid not running      -> reported stale, record removed
#   identity unproved    -> NOT signalled, record KEPT (deleting it would strand a live process
#                           with no handle, and the next start would stack a second server on its port)
#   identity proved      -> stopped; record removed
# Exit code: 0 when every entry was stopped, absent or stale-cleaned and no lane port still answers;
# 1 when an entry was refused or survived, or a lane port is still answering.
param(
    [string]$PidDir = 'C:\mfgtmp\lane_pids',
    # The ports hosting_up.ps1 binds. Read-only: used only to report what is still answering after
    # the stop pass. Port ownership never decides who is signalled.
    [int]$CorePort = 8083,
    [int]$GatePort = 8090
)
$ProgressPreference = 'SilentlyContinue'
$ErrorActionPreference = 'Stop'

function Get-LaneProcess([int]$processId) {
    Get-CimInstance Win32_Process -Filter "ProcessId=$processId" -EA SilentlyContinue
}

function Get-Argv([string]$commandLine) {
    # Quote-aware argv split (the shape of CommandLineToArgvW): a quoted argument is ONE argument.
    # Splitting on whitespace instead would turn a quoted script body that happens to mention
    # AsphyxiaCore.js into bare AsphyxiaCore.js "arguments" - which is how the old predicate's
    # substring match got dressed up as a token rule without getting any safer.
    $argv = @()
    foreach ($m in [regex]::Matches("$commandLine", '"([^"]*)"|(\S+)')) {
        $v = if ($m.Groups[1].Success) { $m.Groups[1].Value } else { $m.Groups[2].Value }
        if ($v) { $argv += $v.Trim('"').Trim("'") }
    }
    return $argv
}

function Test-LaneIdentity {
    # Role rules mirror lane_down.sh:87-108 and decide on WHOLE arguments, never on a substring: the
    # core is node with an argument whose file name is exactly AsphyxiaCore.js, so a syntax check, a
    # file reader or a harness argument that mentions the file name is not the core. On top of the
    # role rule, a record that carries the command line seen at start must still match the live one:
    # that pid is the process the record named, or nothing.
    param([string]$kind, $p, [string]$recordedCmd)
    $why = $null
    switch ($kind) {
        'core' {
            if ($p.Name -ne 'node.exe') { $why = "$($p.Name) is not node.exe" }
            else {
                $coreArg = $false
                foreach ($t in Get-Argv $p.CommandLine) {
                    if ($t -match '(^|[\\/])AsphyxiaCore\.js$') { $coreArg = $true; break }
                }
                if (-not $coreArg) { $why = 'no argument whose file name is AsphyxiaCore.js (mentioning it is not identity)' }
            }
        }
        'gate'   { if ($p.Name -ne 'WebFront.exe')   { $why = "$($p.Name) is not WebFront.exe" } }
        'tunnel' { if ($p.Name -ne 'cloudflared.exe') { $why = "$($p.Name) is not cloudflared.exe" } }
        default  { $why = "unknown lane role '$kind'" }
    }
    if (-not $why -and $recordedCmd -and ("$($p.CommandLine)" -cne $recordedCmd)) {
        $why = "live command line is not the one recorded when this lane started - a recycled pid is not the lane process`n         recorded: $recordedCmd`n         live:     $($p.CommandLine)"
    }
    return @{ ok = (-not $why); why = $why }
}

$parentPid = (Get-LaneProcess $PID).ParentProcessId
$stopped = 0; $refused = 0; $stale = 0; $absent = 0; $survived = 0

foreach ($entry in @(
    @{ kind = 'core';   file = Join-Path $PidDir 'core.pid' }
    @{ kind = 'gate';   file = Join-Path $PidDir 'gate.pid' }
    @{ kind = 'tunnel'; file = Join-Path $PidDir 'tunnel.pid' }
)) {
    $kind = $entry.kind
    if (-not (Test-Path -LiteralPath $entry.file)) {
        "${kind}: no record ($($entry.file)); nothing recorded, nothing signalled"
        $absent++
        continue
    }
    $lines = @(Get-Content -LiteralPath $entry.file -EA SilentlyContinue | ForEach-Object { "$_".Trim() })
    $rawPid = if ($lines.Count -gt 0) { $lines[0] } else { '' }
    $recordedCmd = if ($lines.Count -gt 1) { ($lines[1..($lines.Count - 1)] -join ' ').Trim() } else { '' }
    if ($rawPid -notmatch '^\d+$') {
        "${kind}: REFUSED - record $($entry.file) holds no plain pid ('$rawPid'); not signalled, record KEPT"
        $refused++; continue
    }
    $oldPid = [int]$rawPid
    if ($oldPid -le 4) {
        "${kind}: REFUSED - record holds pid $oldPid; system pids are never signalled, record KEPT"
        $refused++; continue
    }
    if ($oldPid -eq $PID -or ($parentPid -and $oldPid -eq [int]$parentPid)) {
        "${kind}: REFUSED - record holds this script's pid or its parent's ($oldPid); record KEPT"
        $refused++; continue
    }
    $p = Get-LaneProcess $oldPid
    if (-not $p) {
        "${kind}: pid $oldPid is not running; stale record $($entry.file) removed"
        Remove-Item -LiteralPath $entry.file -Force -EA SilentlyContinue
        $stale++; continue
    }
    $id = Test-LaneIdentity $kind $p $recordedCmd
    if (-not $id.ok) {
        "${kind}: pid $oldPid is $($p.Name) [$($p.CommandLine)] - $($id.why); NOT signalled, record KEPT"
        "${kind}: (deleting the record would strand a live process with no handle - lane_down.sh:17-19)"
        $refused++; continue
    }
    $stopErr = $null
    try { Stop-Process -Id $oldPid -Force -ErrorAction Stop } catch { $stopErr = $_.Exception.Message }
    if ($stopErr) {
        "${kind}: REFUSED - could not signal pid $oldPid ($stopErr); record KEPT"
        $refused++; continue
    }
    $gone = $false
    for ($i = 0; $i -lt 20; $i++) {
        if (-not (Get-Process -Id $oldPid -EA SilentlyContinue)) { $gone = $true; break }
        Start-Sleep -Milliseconds 300
    }
    if (-not $gone) {
        "${kind}: pid $oldPid survived Stop-Process -Force; investigate what it is blocking on; record KEPT"
        $survived++; continue
    }
    "${kind}: stopped $($p.Name) pid=$oldPid; record removed"
    Remove-Item -LiteralPath $entry.file -Force -EA SilentlyContinue
    $stopped++
}

"summary: $stopped stopped, $refused refused, $stale stale-cleaned, $absent not recorded, $survived survived"

$stillUp = @()
foreach ($port in @($CorePort, $GatePort)) {
    $c = Get-NetTCPConnection -LocalPort $port -State Listen -EA SilentlyContinue | Select-Object -First 1
    if ($c) {
        $who = Get-LaneProcess $c.OwningProcess
        $stillUp += ('port {0} still answers (pid {1} {2})' -f $port, $c.OwningProcess, $who.Name)
    }
}
if ($refused -or $survived) { 'lane down: INCOMPLETE - see the refusal reasons above'; exit 1 }
if ($stillUp.Count -gt 0) {
    $stillUp | ForEach-Object { "lane still up: $_" }
    'nothing was signalled for those: this script never kills by name, pattern or port ownership ' +
        '(checkup 2026-11-06 item 14), and it found no record for them in ' + $PidDir + '.'
    'a lane started by scripts/hosting_up.bat is stopped with Ctrl+C in its window. To retire a lane ' +
        'that started before pid records existed, stop the pids named above yourself after checking them: ' +
        'Stop-Process -Id <pid> -Force'
    exit 1
}
if (($stopped + $stale) -eq 0 -and $absent -eq 3) { 'lane already down (no records in ' + $PidDir + ')' }
else { 'lane down' }
exit 0
