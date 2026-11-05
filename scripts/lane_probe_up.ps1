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

function Stop-Recorded($file, $expectName) {
    if (-not (Test-Path $file)) { "no pid file $file"; return }
    $old = (Get-Content $file -Raw).Trim()
    if ($old -match '^\d+$') {
        $p = Get-CimInstance Win32_Process -Filter "ProcessId=$old" -EA SilentlyContinue
        if ($p -and $p.Name -eq $expectName) { Stop-Process -Id ([int]$old) -Force; "stopped $expectName pid=$old" }
        elseif ($p) { "pid $old is $($p.Name), not $expectName; left alone" }
    }
    Remove-Item $file -Force -EA SilentlyContinue
}

function Stop-ProbeLane {
    Stop-Recorded $pidFile 'WebFront.exe'
    Stop-Recorded $staticPidFile 'node.exe'
}

if ($Stop) { Stop-ProbeLane; exit 0 }
if ($Status) {
    foreach ($port in 8090, $RelayPort, $StaticPort) {
        $c = Get-NetTCPConnection -LocalPort $port -State Listen -EA SilentlyContinue | Select-Object -First 1
        "port $port " + $(if ($c) { "up pid " + $c.OwningProcess } else { 'down' })
    }
    exit 0
}

Stop-ProbeLane
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
Set-Content -LiteralPath $pidFile $relay.Id -Encoding utf8
"probe relay pid=$($relay.Id) port=$RelayPort upstream=8083 root=$Webroot"

# static front for the probe webroot (COOP/COEP + range requests, no gate)
$env:WEBFRONT_ROOT = $Webroot
$env:WEBFRONT_PORT = "$StaticPort"
$static = Start-Process -FilePath 'node' -ArgumentList $staticScript -WindowStyle Hidden -PassThru `
    -RedirectStandardOutput 'C:\mfgtmp\probe_static.out.txt' -RedirectStandardError 'C:\mfgtmp\probe_static.err.txt'
Set-Content -LiteralPath $staticPidFile $static.Id -Encoding utf8
"probe static front pid=$($static.Id) port=$StaticPort root=$Webroot"

$up = $false
for ($i = 0; $i -lt 60; $i++) {
    $relayUp = Get-NetTCPConnection -LocalPort $RelayPort -State Listen -ErrorAction SilentlyContinue
    $staticUp = Get-NetTCPConnection -LocalPort $StaticPort -State Listen -ErrorAction SilentlyContinue
    if ($relayUp -and $staticUp) { $up = $true; break }
    Start-Sleep -Milliseconds 500
}
if (-not $up) { throw "probe lane ports did not come up ($RelayPort / $StaticPort)" }

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
