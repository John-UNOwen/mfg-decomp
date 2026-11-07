# lane_probe_up.ps1 - private probe lane for the in-browser frame harness.
# Starts a second WebFront in open-relay mode (no access code, no gate) on 8091 upstream to the
# owner's asphyxia core on 8083, plus a loopback static front for the probe webroot on 8095, so a
# candidate build can be measured without touching the lane webroot or the owner's gate on 8090.
# The assemble that fills the probe webroot must be run with -NoGateStop -NoSidecar and
# -FrontUrl http://127.0.0.1:8091/aog (see assemble_webroot.ps1).
#   lane_probe_up.ps1                  bring the probe lane up (webroot_probe, relay 8091, static 8095)
#   lane_probe_up.ps1 -Webroot X       use another webroot
#   lane_probe_up.ps1 -Stop            stop only the processes this script started
#   lane_probe_up.ps1 -Status          show what is up
# Pid records (C:\mfgtmp\probe_lane_pid.txt, probe_static_pid.txt) follow the lane_down.sh rule
# settled in ledger L41: a record is removed only when the process it names was stopped or found
# not running. A live process that is refused keeps its pid file, and a start that had to refuse
# one is aborted instead of stacking a second front on the port the stale one is holding.
# -Stop exits 1 when an entry was refused for the same reason.
param(
    [switch]$Stop,
    [switch]$Status,
    [string]$Webroot = 'C:\mfgtmp\webroot_probe',
    [int]$RelayPort = 8091,
    [int]$StaticPort = 8095
)
$ProgressPreference = 'SilentlyContinue'
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path)
$pidFile = 'C:\mfgtmp\probe_lane_pid.txt'
$staticPidFile = 'C:\mfgtmp\probe_static_pid.txt'
$staticScript = Join-Path $root 'scripts\serve_aog.js'

# Reasons Stop-Recorded refused to signal an entry. Non-empty means a live process is still
# holding its port with its pid file kept, so the probe lane must not be started on top of it.
$stopRefused = @()

function Stop-Recorded($file, $expectName, $port = 0) {
    # Same contract as scripts/linux/lane_down.sh:12-22 (the settled L41 rule): the pid record is
    # removed only when the recorded process was stopped or found not running. A live process that
    # is refused keeps its pid file - deleting it strands a live server with no handle, and the next
    # start stacks a second front on its port while readiness is answered by the stale one.
    # Identity is proved the way hosting_down.ps1 proves it (Test-LaneIdentity:83-85: the recorded
    # command line must still match the live one) and the way preview_game.ps1 proves a front
    # (Stop-RecordedPid:167-172: the listener we own is proven by the port it serves). The image name
    # alone is NEVER enough: the probe relay and the hosted gate are both WebFront.exe, the probe
    # static front and the hosted core are both node.exe, so a stale or recycled record that matched
    # only on name would signal the live lane (the 2026-11-07 incident). A front is bound to a port,
    # so the port is a required proof here; that also breaks the shared-image-name collision - a pid
    # recycled onto the hosted gate owns 8090, never this relay's port, so it is refused even if the
    # command line happens to match. A record in the old pid-only shape (no command line) can only be
    # proved by the port; with no port claimed either, it is refused, not trusted on the name.
    if (-not (Test-Path $file)) { "no pid file $file"; return }
    $lines = @(Get-Content -LiteralPath $file -EA SilentlyContinue | ForEach-Object { "$_".Trim() })
    $raw = if ($lines.Count -gt 0) { $lines[0] } else { '' }
    $recordedCmd = if ($lines.Count -gt 1) { ($lines[1..($lines.Count - 1)] -join ' ').Trim() } else { '' }
    $oldNum = 0
    if ($raw -notmatch '^\d+$' -or -not [int]::TryParse($raw, [ref]$oldNum)) {
        $why = "pid file $file does not hold a plain pid ('$raw'); not signalled, pid file KEPT"
        $script:stopRefused += $why; $why; return
    }
    if ($oldNum -le 4) {
        $why = "pid file $file holds pid $oldNum; system pids are never signalled, pid file KEPT"
        $script:stopRefused += $why; $why; return
    }
    $parentPid = (Get-CimInstance Win32_Process -Filter "ProcessId=$PID" -EA SilentlyContinue).ParentProcessId
    if ($oldNum -eq $PID -or ($parentPid -and $oldNum -eq [int]$parentPid)) {
        $why = "pid file $file holds this script's pid or its parent's ($oldNum); not signalled, pid file KEPT"
        $script:stopRefused += $why; $why; return
    }
    if (-not (Get-Process -Id $oldNum -EA SilentlyContinue)) {
        "pid $oldNum is not running; stale pid file $file removed"
        Remove-Item $file -Force -EA SilentlyContinue
        return
    }
    $p = Get-CimInstance Win32_Process -Filter "ProcessId=$oldNum" -EA SilentlyContinue
    if (-not $p) {
        $why = "pid $oldNum is alive but its process record cannot be read, so it cannot be proved to be $expectName; not signalled, pid file KEPT"
        $script:stopRefused += $why; $why; return
    }
    if ($p.Name -ne $expectName) {
        $why = "pid $oldNum is $($p.Name), not $expectName; not signalled, pid file KEPT (deleting it would strand a live process with no handle)"
        $script:stopRefused += $why; $why; return
    }
    # The name only says the image matches, which is shared with the hosted lane; demand the second
    # proof (hosting_down.ps1:83-85 command line, Stop-RecordedPid:167-172 port) before any signal.
    $cmdOk = [bool]$recordedCmd -and ("$($p.CommandLine)" -ceq $recordedCmd)
    $portOk = ($port -gt 0) -and [bool](Get-NetTCPConnection -LocalPort $port -State Listen -EA SilentlyContinue |
        Where-Object { $_.OwningProcess -eq $oldNum })
    $proved = if ($port -gt 0) { $portOk -and (-not $recordedCmd -or $cmdOk) } else { $cmdOk }
    if (-not $proved) {
        $script:stopRefused += "pid $oldNum is $expectName but its identity is not proved; not signalled, pid file KEPT"
        "  port claimed: $(if ($port -gt 0) { $port } else { '<none>' }); owned by pid $oldNum : $portOk"
        "  recorded cmd: $(if ($recordedCmd) { $recordedCmd } else { '<none - pid-only record, no command line to match>' })"
        "  live cmd:     $($p.CommandLine)"
        return
    }
    $stopErr = $null
    try { Stop-Process -Id $oldNum -Force -ErrorAction Stop } catch { $stopErr = $_.Exception.Message }
    if ($stopErr) {
        $why = "pid $oldNum ($expectName): could not signal it ($stopErr); not signalled, pid file KEPT"
        $script:stopRefused += $why; $why; return
    }
    Remove-Item $file -Force -EA SilentlyContinue
    "stopped $expectName pid=$oldNum" + $(if ($portOk) { " (owns port $port)" } else { " (recorded command line matched)" })
}

function Stop-ProbeLane {
    $script:stopRefused = @()
    # Pass the port each record is bound to: a recorded pid that no longer owns its lane port is a
    # stale or recycled record, and it must not be signalled on the image name it shares with the
    # hosted gate (8090) or core (8083). Reading the port table is a query, never a request to it.
    Stop-Recorded $pidFile 'WebFront.exe' $RelayPort
    Stop-Recorded $staticPidFile 'node.exe' $StaticPort
}

function Assert-NoRefusedEntry($when) {
    if ($script:stopRefused.Count -eq 0) { return }
    foreach ($why in $script:stopRefused) { "refused: $why" }
    throw "$when - $($script:stopRefused.Count) recorded process(es) were left running with their pid files kept. Stop them yourself (they are not this lane's, or the record cannot be trusted) and run again."
}

function Write-ProbePid($file, $proc) {
    # Line 1 is the pid, line 2 the WMI command line at start - exactly hosting_up.ps1:63-67. The old
    # pid-only record left the image name as the only proof, and the hosted gate/core share WebFront.exe
    # and node.exe with this lane (the 2026-11-07 fix); the command line gives Stop-Recorded a second
    # proof that survives pid recycling, since a Windows command line never changes after creation.
    $rec = Get-CimInstance Win32_Process -Filter "ProcessId=$($proc.Id)" -EA SilentlyContinue
    Set-Content -LiteralPath $file -Value @("$($proc.Id)", [string]$rec.CommandLine) -Encoding utf8
}

if ($Stop) {
    Stop-ProbeLane
    if ($script:stopRefused.Count -gt 0) { "probe lane stop: INCOMPLETE - pid file kept"; exit 1 }
    exit 0
}
if ($Status) {
    foreach ($port in 8090, $RelayPort, $StaticPort) {
        $c = Get-NetTCPConnection -LocalPort $port -State Listen -EA SilentlyContinue | Select-Object -First 1
        "port $port " + $(if ($c) { "up pid " + $c.OwningProcess } else { 'down' })
    }
    exit 0
}

Stop-ProbeLane
Assert-NoRefusedEntry 'probe lane start refused'
if (-not (Test-Path (Join-Path $Webroot 'index.html'))) { throw "no index.html in $Webroot - assemble the probe webroot first" }
if (-not (Test-Path $staticScript)) { throw "missing static front script $staticScript" }

# relay: same wiring preview_game.ps1 uses for the LAN dev lane, on its own port
$wd = 'C:\mfg\webfront\bin\Release\net8.0'
$env:WEBFRONT_ROOT = $Webroot
$env:WEBFRONT_UPSTREAM = 'http://127.0.0.1:8083'
$env:WEBFRONT_PORT = "$RelayPort"
$env:WEBFRONT_BIND = 'loopback'
$env:WEBFRONT_GATE = '0'
$env:WEBFRONT_AUTH = 'http://127.0.0.1:8083'
$relay = Start-Process -FilePath (Join-Path $wd 'WebFront.exe') -WorkingDirectory $wd -WindowStyle Hidden -PassThru `
    -RedirectStandardOutput 'C:\mfgtmp\probe_relay.out.txt' -RedirectStandardError 'C:\mfgtmp\probe_relay.err.txt'
Write-ProbePid $pidFile $relay
"probe relay pid=$($relay.Id) port=$RelayPort upstream=8083 root=$Webroot"

# static front for the probe webroot (COOP/COEP + range requests, no gate)
$env:WEBFRONT_ROOT = $Webroot
$env:WEBFRONT_PORT = "$StaticPort"
$static = Start-Process -FilePath 'node' -ArgumentList $staticScript -WindowStyle Hidden -PassThru `
    -RedirectStandardOutput 'C:\mfgtmp\probe_static.out.txt' -RedirectStandardError 'C:\mfgtmp\probe_static.err.txt'
Write-ProbePid $staticPidFile $static
"probe static front pid=$($static.Id) port=$StaticPort root=$Webroot"

$up = $false
for ($i = 0; $i -lt 60; $i++) {
    # Readiness means THIS run's front is answering on the port, not just "something is listening":
    # a relay left over from an earlier run holds the port and would otherwise be reported as up.
    $relayUp = Get-NetTCPConnection -LocalPort $RelayPort -State Listen -ErrorAction SilentlyContinue |
        Where-Object { $_.OwningProcess -eq $relay.Id }
    $staticUp = Get-NetTCPConnection -LocalPort $StaticPort -State Listen -ErrorAction SilentlyContinue |
        Where-Object { $_.OwningProcess -eq $static.Id }
    if ($relayUp -and $staticUp) { $up = $true; break }
    Start-Sleep -Milliseconds 500
}
if (-not $up) {
    foreach ($e in [pscustomobject]@{ Port = $RelayPort; Mine = $relay.Id }, [pscustomobject]@{ Port = $StaticPort; Mine = $static.Id }) {
        $c = Get-NetTCPConnection -LocalPort $e.Port -State Listen -ErrorAction SilentlyContinue | Select-Object -First 1
        if (-not $c) { "port $($e.Port): no listener (this run started pid $($e.Mine))" }
        elseif ($c.OwningProcess -ne $e.Mine) {
            $h = Get-CimInstance Win32_Process -Filter "ProcessId=$($c.OwningProcess)" -EA SilentlyContinue
            "port $($e.Port): held by pid $($c.OwningProcess) ($($h.Name)), not this run's pid $($e.Mine) - a stale front owns it"
        } else { "port $($e.Port): this run's pid $($e.Mine) is not listening yet" }
    }
    throw "probe lane ports did not come up ($RelayPort / $StaticPort)"
}

# The front the player talks to must be the relay, not the owner's gate.
$html = Get-Content (Join-Path $Webroot 'index.html') -Raw
if ($html -notmatch ('http://127\.0\.0\.1:' + $RelayPort + '/aog')) {
    throw "index.html in $Webroot does not point -FRONTURL at the relay on $RelayPort - re-assemble with -FrontUrl http://127.0.0.1:$RelayPort/aog"
}
if ($html -notmatch '-AUTOSERVE') { throw 'probe page lost the -AUTOSERVE guest boot' }

$pageUrl = "http://127.0.0.1:$StaticPort/aog/index.html"
$smoke = Invoke-WebRequest -Uri "http://127.0.0.1:$RelayPort/aog/appli_info" -Method Post -Body 'web_id=1' `
    -ContentType 'application/x-www-form-urlencoded' -UserAgent 'Mozilla/5.0 probe-relay' -TimeoutSec 20 -UseBasicParsing
if ($smoke.StatusCode -ne 200 -or $smoke.Content -notmatch '<root>') { throw 'probe relay smoke failed' }
"relay smoke: POST /aog/appli_info -> 200 XML via core 8083"
$head = Invoke-WebRequest -Uri $pageUrl -Method Head -TimeoutSec 10 -UseBasicParsing
"probe page $pageUrl status $($head.StatusCode) (guest boot: -AUTOSERVE, no -REGULAR/-DATAID)"
"probe target scene: FirstCharaSelect; character pick with ?-MFGCHARA=N"
