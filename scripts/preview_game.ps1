# preview_game.ps1 - launch the rebuild game for manual preview on the self-hosted
# RyuNET server (Asphyxia fork + mfg@asphyxia plugin, repo tmp\RyuNET-core).
# Static pages come from the node server on 8094; the AOG front is webfront on
# the preview relay port (default 8096, -FrontPort), which relays a non-browser
# User-Agent to RyuNET (it refuses Mozilla UAs) and carries the CORS/COOP/COEP
# headers. No -AUTOSERVE by default: you drive.
# No capture harness is started; the window stays open until you close it.
#
# usage:
#   preview_game.ps1 / .bat      # RyuNET preview: full server features, you drive everything
#   preview_game.ps1 -DataID NAME # identity to log in as (default 6AE73807BA1CE8CB: the account
#                                  # your first preview sessions already built up: 26 saved states);
#                                  # stays the same across launches, so the saved account comes back
#   preview_game.ps1 -Guest      # legacy trial semantics: pinned account boots as GUEST
#                                  # (default is REGULAR: registered player, My Room etc. unlocked;
#                                  # an account with no registered name gets the original name-setup flow)
#   preview_game.ps1 -Mock       # offline fallback lane: repo mock on 8085, no hold
#   preview_game.ps1 -Park       # capture-style frozen FirstCharaSelect (mock 8084, 1 h hold);
#                                  # C:\mfgtmp\step2_park.ps1 starts those mock/front processes and owns them
#   preview_game.ps1 -Stop       # close the preview window and stop the processes THIS script started
#   preview_game.ps1 -Reassemble # re-copy + re-patch out\webgl into the served webroot first
#   preview_game.ps1 -FrontPort N# preview relay port (default 8096). Never 8090: that is the hosted
#                                  # gate port and the port the cloudflared tunnel ingress forwards to
#   preview_game.ps1 -CorePort N # the lane core port (default 8083): reused if it already answers, and
#                                  # the port a recorded core must own before -Stop will signal it
#   preview_game.ps1 -PidDir DIR # where this lane's pid records live (default C:\mfgtmp\preview_pids),
#                                  # same knob hosting_down.ps1:34 offers for C:\mfgtmp\lane_pids
#
# Lane ownership (2026-11-06 checkup item 5). This script used to reclaim "the lane" by process
# name on every launch (every WebFront.exe, plus every node.exe whose command line merely
# mentioned AsphyxiaCore.js) and then restarted the front on 8090 with WEBFRONT_GATE=0. Beside a
# live hosted lane that destroyed the gated production front and put an ungated relay on the port
# the owner's cloudflared tunnel serves, or left the hosted lane with no front at all. The
# contract now:
#   * every process this script starts is recorded in -PidDir as one file per child (pid on line 1,
#     the command line WMI sees at start on line 2, the hosting_up.ps1:63-67 shape), and -Stop (or a
#     relaunch) stops only those pids - each one re-proved with hosting_down.ps1's Test-LaneIdentity
#     rule (the image name; for node, an ARGUMENT whose file name is AsphyxiaCore.js; the command line
#     recorded at launch) plus the port that record claims, as scripts/linux/lane_down.sh does. A
#     record that cannot be proved is a refusal that keeps the file, never something to kill;
#   * a process this script did not start is never signalled: a port held by a foreign listener
#     is a refusal with a reason, not something to kill;
#   * a core already answering 8083 is reused, not replaced - killing it drops every live session
#     and the core's in-memory state with it;
#   * the preview relay (gate OFF by design) never binds a port a cloudflared ingress points at,
#     so an ungated relay can never be what the public hostname serves.
#   * the hosted lane starts and stops with scripts/hosting_up.ps1 and scripts/hosting_down.ps1.
param(
    [switch]$Reassemble,
    [switch]$Mock,
    [switch]$Park,
    [switch]$Stop,
    [switch]$Guest,
    [string]$DataID = '6AE73807BA1CE8CB',
    [int]$FrontPort = 8096,
    # The lane core port. It is a parameter for the same reason hosting_down.ps1:37-38 takes -CorePort:
    # a child process has to be able to exercise the stop path against a scratch port instead of the
    # live 8083 (which belongs to the hosted core and is never touched here).
    [int]$CorePort = 8083,
    # Where this lane writes its one-pid-per-child records. Same knob as hosting_down.ps1:34, so a
    # harness can drive the real script against scratch records instead of C:\mfgtmp\preview_pids.
    [string]$PidDir = 'C:\mfgtmp\preview_pids',
    [string]$EdgeExe = 'C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe'
)
$ProgressPreference = 'SilentlyContinue'
$ErrorActionPreference = 'Stop'
$webroot = 'C:\mfgtmp\webroot'
$profileDir = 'C:\mfgtmp\edge-preview'
# RyuNET must run from the REAL repo path, not the C:\mfg junction: the core's
# plugin caller detection compares stack frame paths against its cwd-derived
# plugins path, and node reports realpath - a junction cwd makes every DB call fail.
$repoReal = 'C:\mfg'
$tgt = (Get-Item 'C:\mfg' -EA SilentlyContinue).Target
if ($tgt) { $repoReal = $tgt.TrimEnd('\') }
$ryuDist = Join-Path $repoReal 'tmp\RyuNET-core\dist'
# Presenter secret (docs/28): once an account is claimed/web-issued it is
# card-bound server-side, and its logins must be presented by the front.
# Both core (plugin side) and webfront need the same secret for this to work
# on the LAN lane too. Generated once by hosted_lane/hosting runbook.
$secFile = Join-Path $env:USERPROFILE '.mfg-web\presenter.secret'
if (Test-Path $secFile) {
    $env:VFG_PRESENTER_SECRET = (Get-Content $secFile -Raw).Trim()
    $env:WEBFRONT_PRESENTER_SECRET_FILE = $secFile
} else { 'note: no presenter secret at ' + $secFile + ' - claimed accounts will not log in on this lane' }
$page = if ($Park) { 'index_park2.html' } else { 'index_play.html' }
$url = 'http://127.0.0.1:8094/aog/' + $page
# The lane core port is the -CorePort parameter (used below as $corePort: PowerShell names are
# case-insensitive), so a harness can exercise the whole lane against a scratch port.
$mockPort = 8085
# Ownership record: one pid file per child this script starts (-PidDir, default C:\mfgtmp\preview_pids).
# Each file is line 1 = the pid, line 2 = the command line WMI sees at start - the shape
# hosting_up.ps1:63-67 writes for the hosted lane, and the only shape that carries enough to re-prove
# the process later. Only these pids are ever signalled, and each one is re-identified from the live
# process before the signal.

function Stop-PreviewWindow {
    # Close ONLY the preview window this script started, by the pid it recorded for it. This used to
    # enumerate every msedge.exe on the box and force-kill any whose command line merely CONTAINED the
    # profile path ('edge-preview'): a browser is dozens of processes sharing one binary name and a
    # substring of a command line is not an identity - the exact fault hosting_down.ps1 replaced for
    # the lane processes (checkup 2026-11-06 item 14). The Edge launched at :295 is now recorded (pid +
    # WMI command line, the hosting_up.ps1:63-67 shape) and this stops exactly that pid, re-proving
    # before the signal, with hosting_down.ps1's Test-LaneIdentity rule, that it is still the process
    # the record named: image name msedge.exe AND the command line Windows saw at launch (which never
    # changes after creation) must both match. Anything else is a refusal that KEEPS the record
    # (deleting it would strand a live window with no handle); a window with no record is reported,
    # never swept by pattern.
    if (-not $pidDir) { 'edge : $pidDir is unset - this script always records the window under it, so there is no record to act on; nothing closed'; return }
    $file = Join-Path $pidDir 'edge.pid'
    if (-not (Test-Path -LiteralPath $file)) { 'edge : no record (' + $file + '); no preview window was recorded by this script, so nothing is closed (it never closes browser windows by pattern)'; return }
    $lines = @(Get-Content -LiteralPath $file -EA SilentlyContinue | ForEach-Object { "$_".Trim() })
    $rawPid = if ($lines.Count -gt 0) { $lines[0] } else { '' }
    $recordedCmd = if ($lines.Count -gt 1) { ($lines[1..($lines.Count - 1)] -join ' ').Trim() } else { '' }
    if ($rawPid -notmatch '^\d+$') { "edge : REFUSED - record $file holds no plain pid ('$rawPid'); not signalled, record KEPT"; return }
    $edgePid = [int]$rawPid
    if ($edgePid -le 3) { "edge : REFUSED - record holds pid $edgePid; system pids are never signalled, record KEPT"; return }
    if ($edgePid -eq $PID) { "edge : REFUSED - record holds this script's pid ($edgePid); record KEPT"; return }
    $p = Get-CimInstance Win32_Process -Filter "ProcessId=$edgePid" -EA SilentlyContinue
    if (-not $p) { "edge : pid $edgePid is not running; stale record removed"; Remove-Item -LiteralPath $file -Force -EA SilentlyContinue; return }
    if ($p.Name -ne 'msedge.exe') {
        "edge : pid $edgePid is $($p.Name) [$($p.CommandLine)] - not msedge.exe; NOT signalled, record KEPT"
        return
    }
    if (-not $recordedCmd) {
        # A record with no second line is the shape this script wrote before the 2026-11-07 fix, and it
        # proves nothing about who holds the pid now: msedge.exe is the image of every browser window on
        # this box, so the name alone is not an identity - the rule Stop-RecordedPid:229-231 and
        # lane_probe_up.ps1:86-93 already apply to their own records.
        "edge : pid $edgePid is msedge.exe but the record carries no command line, so nothing in it proves this pid is still the window that was recorded; NOT signalled, record KEPT"
        "       live: $($p.CommandLine)"
        return
    }
    if ("$($p.CommandLine)" -cne $recordedCmd) {
        "edge : pid $edgePid is msedge.exe but the live command line is not the one recorded when this window started - a recycled pid is not this window's process; NOT signalled, record KEPT"
        "       recorded: $recordedCmd"
        "       live:     $($p.CommandLine)"
        return
    }
    $stopErr = $null
    try { Stop-Process -Id $edgePid -Force -ErrorAction Stop } catch { $stopErr = $_.Exception.Message }
    if ($stopErr) { "edge : REFUSED - could not signal pid $edgePid ($stopErr); record KEPT"; return }
    $edgeGone = $false
    foreach ($try in 1..8) {
        if (-not (Get-Process -Id $edgePid -EA SilentlyContinue)) { $edgeGone = $true; break }
        Start-Sleep -Milliseconds 250
    }
    if (-not $edgeGone) { "edge : pid $edgePid survived Stop-Process -Force; record KEPT - the window is still open and this is the only handle to it"; return }
    "edge : stopped msedge.exe pid=$edgePid (the preview window this script recorded); record removed"
    Remove-Item -LiteralPath $file -Force -EA SilentlyContinue
}

function Get-PortOwnerPid($port) {
    (Get-NetTCPConnection -LocalPort $port -State Listen -EA SilentlyContinue |
        Select-Object -First 1).OwningProcess
}

function Get-TunnelIngressPorts {
    # read-only: the local ports a cloudflared ingress in the owner's config forwards to
    $cfg = Join-Path $env:USERPROFILE '.cloudflared\config.yml'
    if (-not (Test-Path -LiteralPath $cfg)) { return @() }
    @(Select-String -LiteralPath $cfg -Pattern '^\s*service:\s*https?://(?:localhost|127\.0\.0\.1|\[::1\]|0\.0\.0\.0):(\d+)' -EA SilentlyContinue |
        ForEach-Object { [int]$_.Matches[0].Groups[1].Value } | Select-Object -Unique)
}

function Assert-FrontPortFree($port) {
    # The preview relay runs with WEBFRONT_GATE=0. Two things must be true before it binds:
    # nobody else is already serving the port (this script never kills a foreign listener),
    # and the port is not one a cloudflared ingress forwards to (an ungated relay must never
    # be what the public hostname serves - the checkup item 5 incident).
    $owner = Get-PortOwnerPid $port
    if ($owner) {
        $who = Get-CimInstance Win32_Process -Filter "ProcessId=$owner" -EA SilentlyContinue
        throw ('port {0} is already served by pid {1} ({2}); this preview lane never stops processes ' +
               'it did not start - free the port or choose another with -FrontPort') -f $port, $owner, $who.Name
    }
    foreach ($ing in Get-TunnelIngressPorts) {
        if ($ing -eq $port) {
            throw ('port {0} is a cloudflared tunnel ingress port (see %USERPROFILE%\.cloudflared\config.yml): ' +
                   'the gate-off preview relay must not sit behind the public hostname - use another -FrontPort') -f $port
        }
    }
}

function Get-Argv([string]$commandLine) {
    # Quote-aware argv split, the same implementation as hosting_down.ps1:47-58: a quoted argument is
    # ONE argument. Splitting on whitespace instead turns a quoted argument that happens to mention
    # AsphyxiaCore.js into bare AsphyxiaCore.js "arguments", which is a substring match wearing a token
    # rule's clothes - that is exactly how this script proved its core (checkup 2026-11-08).
    $argv = @()
    foreach ($m in [regex]::Matches("$commandLine", '"([^"]*)"|(\S+)')) {
        $v = if ($m.Groups[1].Success) { $m.Groups[1].Value } else { $m.Groups[2].Value }
        if ($v) { $argv += $v.Trim('"').Trim("'") }
    }
    return $argv
}

function Write-PreviewPid($label, $proc) {
    # The ownership record, in the hosting_up.ps1:63-67 Write-LanePid shape: line 1 the pid, line 2 the
    # command line as WMI sees it now. Stop-RecordedPid compares that line against the live one, so a
    # pid that has since been recycled onto some other process is refused instead of signalled (a
    # Windows command line never changes after creation). Written the moment the child starts: a record
    # made after a later failure would leave that child unowned.
    $rec = Get-CimInstance Win32_Process -Filter "ProcessId=$($proc.Id)" -EA SilentlyContinue
    Set-Content -LiteralPath (Join-Path $pidDir ($label + '.pid')) -Value @("$($proc.Id)", [string]$rec.CommandLine) -Encoding utf8
}

function Stop-RecordedPid($label, $expectName, $port) {
    # Stop exactly one child this script started. The pid file is the ownership record; the live
    # process is re-identified before any signal, and a mismatch is a refusal that KEEPS the file
    # (deleting it would strand a live process with no handle - scripts/linux/lane_down.sh:17-19).
    # The proof is hosting_down.ps1's Test-LaneIdentity (:60-87), which replaced this shape on the
    # hosted lane: the image name, plus for node an ARGUMENT whose file name is AsphyxiaCore.js
    # (:70-77, lane_down.sh:100-106) decided on whole arguments and never on a whitespace-split token,
    # plus the command line recorded when this child started. It used to prove the core with the split
    # alone, claim no port at all (Stop-OwnLane passed 0, which skips the port check below) and read a
    # pid-only record with nothing to compare: a stale core.pid whose pid had been recycled onto the
    # hosted core - node.exe, AsphyxiaCore.js, the listener on 8083 - satisfied every one of those and
    # the next preview launch force-killed it. A record that carries no command line (the shape this
    # script wrote before this fix) proves nothing about who holds the pid now, so it is refused and
    # kept: unlike hosting_down.ps1, which documents adopting a bare pid in hosting_deploy.md, this
    # lane has no adoption path - every record here is written by Write-PreviewPid, with the line.
    $file = Join-Path $pidDir ($label + '.pid')
    if (-not (Test-Path -LiteralPath $file)) { return }
    $lines = @(Get-Content -LiteralPath $file -EA SilentlyContinue | ForEach-Object { "$_".Trim() })
    $old = if ($lines.Count -gt 0) { $lines[0] } else { '' }
    $recordedCmd = if ($lines.Count -gt 1) { ($lines[1..($lines.Count - 1)] -join ' ').Trim() } else { '' }
    if ($old -notmatch '^\d+$') { "$label : pid file $file holds no usable pid ('$old'); left alone"; return }
    $oldPid = [int]$old
    if ($oldPid -le 3) { "$label : pid file holds pid $oldPid; system pids are never signalled; file kept"; return }
    if ($oldPid -eq $PID) { "$label : pid file holds this script (pid $oldPid); left alone"; return }
    $p = Get-CimInstance Win32_Process -Filter "ProcessId=$oldPid" -EA SilentlyContinue
    if (-not $p) { "$label : pid $oldPid is not running; stale pid file removed"; Remove-Item -LiteralPath $file -Force -EA SilentlyContinue; return }
    $why = $null
    if ($p.Name -ne $expectName) {
        $why = "not this lane's $expectName"
    } elseif ($expectName -eq 'node.exe') {
        # node is the shared interpreter: only an argument whose FILE NAME is AsphyxiaCore.js is this
        # lane's core; the hosted core is the same binary and the same script, so the file name alone
        # is not enough either - the recorded command line and the port below decide.
        $coreArg = $false
        foreach ($t in Get-Argv $p.CommandLine) { if ($t -match '(^|[\\/])AsphyxiaCore\.js$') { $coreArg = $true; break } }
        if (-not $coreArg) { $why = 'no argument whose file name is AsphyxiaCore.js (mentioning it is not identity)' }
    }
    if (-not $why -and -not $recordedCmd) {
        $why = 'the record carries no command line, so nothing in it proves this pid is still the process the record named'
    }
    if (-not $why -and ("$($p.CommandLine)" -cne $recordedCmd)) {
        $why = 'the live command line is not the one recorded when this child started - a recycled pid is not this child'
    }
    $portClaim = if ($port -gt 0) { [string]$port } else { '<none>' }
    $portOwned = $null
    if (-not $why -and $port -gt 0) {
        # a listener we own is proved by the lane port it serves - the strongest signal available
        $portOwned = [bool](Get-NetTCPConnection -LocalPort $port -State Listen -EA SilentlyContinue |
            Where-Object { $_.OwningProcess -eq $oldPid })
        if (-not $portOwned) { $why = "it does not own the port the record claims ($port)" }
    }
    if ($why) {
        "$label : pid $oldPid is $($p.Name) [$($p.CommandLine)] - $why; NOT signalled; pid file KEPT"
        "       recorded cmd: $(if ($recordedCmd) { $recordedCmd } else { '<none - pid-only record, nothing to compare>' })"
        "       live cmd:     $($p.CommandLine)"
        "       port claimed: $portClaim" + $(if ($null -ne $portOwned) { "; owned by pid $oldPid : $portOwned" } else { '' })
        return
    }
    # A swallowed stop is a false claim: this used to be Stop-Process -EA SilentlyContinue followed by
    # "stopped" and Remove-Item unconditionally, so a refused signal printed success and then deleted the
    # only handle to a live process - the outcome every refusal branch above exists to avoid. Report what
    # the signal did, and if it did not, keep the record (the shape hosting_down.ps1:132-143 and
    # s2_run.ps1:75-82 already use).
    $stopErr = $null
    try { Stop-Process -Id $oldPid -Force -ErrorAction Stop } catch { $stopErr = $_.Exception.Message }
    $gone = $false
    foreach ($try in 1..8) {
        if (-not (Get-Process -Id $oldPid -EA SilentlyContinue)) { $gone = $true; break }
        Start-Sleep -Milliseconds 250
    }
    if (-not $gone) {
        $why = if ($stopErr) { "could not be signalled ($stopErr)" } else { 'survived Stop-Process -Force' }
        "$label : pid $oldPid ($expectName) $why; pid file KEPT - it is still running and this is the only handle to it"
        return
    }
    "$label : stopped $expectName pid=$oldPid" + $(if ($port -gt 0) { " (port $port)" } else { '' })
    Remove-Item -LiteralPath $file -Force -EA SilentlyContinue
}

function Stop-OwnLane {
    # this lane's own children, and nothing else - no name match, no command-line substring
    Stop-PreviewWindow
    Stop-RecordedPid 'front' 'WebFront.exe' $FrontPort
    Stop-RecordedPid 'mock' 'MockServer.exe' $mockPort
    # the core is claimed on the lane core port: a recorded node pid that no longer owns $corePort is a
    # stale or recycled record, and the hosted core (node.exe, AsphyxiaCore.js, the 8083 listener) must
    # never be signalled on the strength of a pid file alone.
    Stop-RecordedPid 'core' 'node.exe' $corePort
}

function Wait-Port($port, $tries = 80) {
    for ($i = 0; $i -lt $tries; $i++) {
        if (Get-NetTCPConnection -LocalPort $port -State Listen -EA SilentlyContinue) { return $true }
        Start-Sleep -Milliseconds 500
    }
    return $false
}

function Start-Front($port, $upstream, $bind) {
    $env:WEBFRONT_ROOT = $webroot
    $env:WEBFRONT_UPSTREAM = $upstream
    $env:WEBFRONT_PORT = "$port"
    $env:WEBFRONT_BIND = $bind
    # LAN dev lane: relay mode stays open (no web sessions here); the hosted
    # gate (WEBFRONT_GATE=1 + presenter secret) is a deployment thing. That is
    # safe only because Assert-FrontPortFree keeps this relay off every port the
    # public tunnel ingress forwards to - an open relay must never be what the
    # hostname serves. It binds loopback and never 8090 (the hosted gate port).
    $env:WEBFRONT_GATE = '0'
    $env:WEBFRONT_AUTH = 'http://127.0.0.1:' + $corePort
    $wd = 'C:\mfg\webfront\bin\Release\net8.0'
    $p = Start-Process -FilePath (Join-Path $wd 'WebFront.exe') -WorkingDirectory $wd -WindowStyle Hidden -PassThru `
        -RedirectStandardOutput ("C:\mfgtmp\webfront$port.out.txt") -RedirectStandardError ("C:\mfgtmp\webfront$port.err.txt")
    Write-PreviewPid 'front' $p
    'WEBFRONT port={0} pid={1} upstream={2} (gate off, loopback, recorded in {3})' -f $port, $p.Id, $upstream, $pidDir
}

function New-PlayPage($frontUrl) {
    # -DATAID pins the identity: the shipping client rolls a new random DataID every
    # launch, so without this each preview starts as a brand-new account (tutorial, story,
    # notices all over again) and the saved server profile is left behind.
    # -REGULAR (default, unless -Guest) makes the pinned account boot as a registered
    # (non-guest) player: full Home hub, My Room character select available. The account
    # flips is_guest=false server-side on its first such login.
    $pin = '"-ENTERCARD", "-TRANSLATE", '
    if (-not $Guest) { $pin += '"-REGULAR", ' }
    $pin += '"-DATAID=' + $DataID + '"'
    $html = Get-Content (Join-Path $webroot 'index.html') -Raw
    $new = $html.Replace(', "-AUTOSERVE"', ', ' + $pin).Replace('"-AUTOSERVE", ', $pin + ', ')
    # Point the page at THIS lane's front whatever the webroot was assembled with;
    # a silent no-op here would leave the page talking to the hosted gate on 8090.
    $new = $new -replace '(-FRONTURL",\s*")[^"]*(")', ('${1}' + $frontUrl + '${2}')
    if ($new -match '-AUTOSERVE') { throw 'play page rewrite failed (AUTOSERVE still present)' }
    if ($new -notmatch [regex]::Escape('"' + $frontUrl + '"')) {
        throw ('play page rewrite failed (-FRONTURL=' + $frontUrl + ' not in the page): re-assemble the webroot with assemble_webroot.ps1')
    }
    Set-Content (Join-Path $webroot 'index_play.html') $new -Encoding UTF8 -NoNewline
    Remove-Item (Join-Path $webroot 'index_play.html.br') -Force -EA SilentlyContinue
}

# 0. lane ownership: stop this script's own children (recorded pids) and its window.
#    Nothing is ever reclaimed by process name or command-line substring: the hosted
#    lane's gate and core match those filters and are not ours to stop.
if ($Stop) { Stop-OwnLane; 'preview stopped (preview window + processes recorded in ' + $pidDir + '); the hosted lane (8090/8083) and the static server 8094 are left alone'; exit 0 }
Stop-OwnLane
if (-not (Test-Path -LiteralPath $pidDir)) { New-Item -ItemType Directory -Path $pidDir | Out-Null }

# 1. webroot freshness
$data = 'C:\mfg\build\unity-project\out\webgl\Build\webgl.data'
$stale = -not (Test-Path (Join-Path $webroot 'Build\webgl.data'))
if (Test-Path $data) {
    $src = (Get-Item $data).LastWriteTime
    $dst = Get-Item (Join-Path $webroot 'Build\webgl.data') -EA SilentlyContinue
    if ($dst) { $stale = $src -gt $dst.LastWriteTime }
}
if ($Reassemble -or $stale) {
    'webroot stale or -Reassemble: running assemble_webroot.ps1'
    & 'C:\mfg\scripts\assemble_webroot.ps1' | Select-Object -Last 3
}

# 2. static front on 8094 (pages/assets; kept alive between launches)
if (-not (Get-NetTCPConnection -LocalPort 8094 -State Listen -EA SilentlyContinue)) {
    Start-Process node -ArgumentList 'C:\mfgtmp\serve_aog.js' -WindowStyle Hidden `
        -RedirectStandardOutput 'C:\mfgtmp\serve_aog.out.txt' -RedirectStandardError 'C:\mfgtmp\serve_aog.err.txt' | Out-Null
    if (-not (Wait-Port 8094 30)) { throw 'static server 8094 did not come up' }
    'static server started on 8094'
} else { 'static server already up on 8094' }

# 3. lane per mode
if ($Park) {
    & 'C:\mfgtmp\step2_park.ps1' | Select-Object -First 6 | ForEach-Object { "$_" }
} elseif ($Mock) {
    $m85 = 'C:\mfgtmp\mock8085'
    $mockOwner = Get-PortOwnerPid $mockPort
    if ($mockOwner) {
        $who = Get-CimInstance Win32_Process -Filter "ProcessId=$mockOwner" -EA SilentlyContinue
        throw ('port {0} is already served by pid {1} ({2}); this preview lane never stops processes it did not start') -f $mockPort, $mockOwner, $who.Name
    }
    $env:MOCK_HOLD_CS_WRITE_MS = '0'
    # $mockProc, not $mock: the script parameter [switch]$Mock is a typed variable, and
    # PowerShell enforces that type on every assignment to $mock (case-insensitive name).
    $mockProc = Start-Process -FilePath (Join-Path $m85 'MockServer.exe') -ArgumentList "$mockPort" -WorkingDirectory $m85 `
        -WindowStyle Hidden -PassThru `
        -RedirectStandardOutput (Join-Path $m85 'stdout.txt') -RedirectStandardError (Join-Path $m85 'stderr.txt')
    Write-PreviewPid 'mock' $mockProc
    Remove-Item Env:MOCK_HOLD_CS_WRITE_MS -EA SilentlyContinue
    if (-not (Wait-Port $mockPort 30)) { throw ('mock ' + $mockPort + ' did not come up') }
    New-PlayPage ('http://127.0.0.1:' + $mockPort + '/aog')
    'mock ' + $mockPort + ' (no hold, pid ' + $mockProc.Id + ') + index_play.html (no AUTOSERVE) ready'
} else {
    # RyuNET core (asphyxia fork + mfg plugin), integrated AOG on 8083. A core already
    # answering 8083 is reused, never replaced: it may be the hosted core, and replacing it
    # signs out every live session and drops the core's in-memory state with it.
    $coreOwner = Get-PortOwnerPid $corePort
    if ($coreOwner) {
        'core already answers ' + $corePort + ' (pid ' + $coreOwner + '): reusing it - this lane did not start it and will not stop it'
    } else {
        $core = Start-Process node -ArgumentList 'AsphyxiaCore.js' -WorkingDirectory $ryuDist -WindowStyle Hidden `
            -RedirectStandardOutput 'C:\mfgtmp\ryunet.out.txt' -RedirectStandardError 'C:\mfgtmp\ryunet.err.txt' -PassThru
        Write-PreviewPid 'core' $core
        if (-not (Wait-Port $corePort 120)) {
            'ryunet stderr tail:'; Get-Content 'C:\mfgtmp\ryunet.err.txt' -Tail 10 -EA SilentlyContinue
            throw ('RyuNET did not bind ' + $corePort)
        }
    }
    # whatever answers 8083 has to prove it is a RyuNET core before it becomes the upstream
    $smoke = Invoke-WebRequest -Uri ('http://127.0.0.1:' + $corePort + '/') -UserAgent 'curl/8' -TimeoutSec 10 -UseBasicParsing
    if ($smoke.Content -notmatch 'RyuNET') { throw 'RyuNET smoke GET / missing VFG banner' }
    'RyuNET up: ' + ($smoke.Content.Trim() -join ' ')
    Assert-FrontPortFree $FrontPort
    Start-Front $FrontPort ('http://127.0.0.1:' + $corePort) 'loopback' | Out-Null
    if (-not (Wait-Port $FrontPort 30)) { throw ('webfront ' + $FrontPort + ' did not come up') }
    $relay = Invoke-WebRequest -Uri ('http://127.0.0.1:' + $FrontPort + '/aog/appli_info') -Method Post -Body 'web_id=1' `
        -ContentType 'application/x-www-form-urlencoded' -UserAgent 'Mozilla/5.0 preview-probe' -TimeoutSec 20 -UseBasicParsing
    if ($relay.StatusCode -ne 200 -or $relay.Content -notmatch '<root>') { throw 'webfront relay to RyuNET failed' }
    'relay smoke: browser-UA POST /aog/appli_info -> 200 XML via RyuNET (front ' + $FrontPort + ')'
    New-PlayPage ('http://127.0.0.1:' + $FrontPort + '/aog')
    'index_play.html ready (no AUTOSERVE, FRONTURL via webfront relay on ' + $FrontPort + ')'
    'identity: ' + $DataID + '  boot: ' + $(if ($Guest) { 'guest (trial)' } else { 'regular (registered player)' }) + '  (press Enter in the game window to sign in)'
}

# 4. visible game window: dedicated Edge profile so it never merges with your personal browser
if (-not (Test-Path $profileDir)) { New-Item -ItemType Directory -Path $profileDir | Out-Null }
$head = Invoke-WebRequest -Uri $url -Method Head -TimeoutSec 10 -UseBasicParsing
if ($head.StatusCode -ne 200) { throw "page $url not served (status $($head.StatusCode))" }
$edge = Start-Process -FilePath $EdgeExe -PassThru -ArgumentList @(
    '--user-data-dir=' + $profileDir,
    '--no-first-run', '--no-default-browser-check', '--disable-features=msEdgeWelcomeExperience',
    '--window-size=1600,976', '--window-position=80,40',
    '--app=' + $url
)
# Record the window we just started through the same writer front/mock/core use (pid + the command line
# WMI sees at launch), so -Stop and a relaunch can close exactly this pid after re-proving it - never
# by sweeping msedge by pattern. Written before the wait so a later failure never leaves it unowned.
Write-PreviewPid 'edge' $edge
Start-Sleep -Seconds 3
if (-not (Get-Process -Id $edge.Id -EA SilentlyContinue)) {
    $any = Get-CimInstance Win32_Process -Filter "Name='msedge.exe'" | Where-Object { $_.CommandLine -like '*edge-preview*' }
    if (@($any).Count -eq 0) { throw 'preview window did not start (check msedge path)' }
}

'preview up: ' + $url
if ($Park) { 'mode: park (auto-play ON, FirstCharaSelect held ~1 h; interaction frozen by design)' }
elseif ($Mock) { 'mode: mock (offline canned front, no AUTOSERVE); full-protocol features live in RyuNET mode' }
else { 'mode: RyuNET (self-hosted asphyxia fork; real gacha, records, match engine). No AUTOSERVE: you drive.' }
'no capture harness is running; the window stays open until you close it.'
if ($Park) { 'this lane: the park mocks and fronts are started by C:\mfgtmp\step2_park.ps1, which owns them' }
elseif ($Mock) { 'this lane: mock on ' + $mockPort + ' (pid recorded in ' + $pidDir + '); the hosted lane on 8090/' + $corePort + ' was not touched' }
else { 'this lane: gate-off relay front on ' + $FrontPort + ' (pid recorded in ' + $pidDir + '); the hosted lane on 8090/' + $corePort + ' was not touched' }
'tidy up later with: preview_game.ps1 -Stop (or the .bat with -Stop) - it stops only what it started'
