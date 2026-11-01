# preview_game.ps1 - launch the rebuild game for manual preview on the self-hosted
# RyuNET server (Asphyxia fork + mfg@asphyxia plugin, repo tmp\RyuNET-core).
# Static pages come from the node server on 8094; the AOG front is webfront on
# 8090, which relays a non-browser User-Agent to RyuNET (it refuses Mozilla UAs)
# and carries the CORS/COOP/COEP headers. No -AUTOSERVE by default: you drive.
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
#   preview_game.ps1 -Park       # capture-style frozen FirstCharaSelect (mock 8084, 1 h hold)
#   preview_game.ps1 -Stop       # close the preview window and kill RyuNET/mock/webfront procs
#   preview_game.ps1 -Reassemble # re-copy + re-patch out\webgl into the served webroot first
param(
    [switch]$Reassemble,
    [switch]$Mock,
    [switch]$Park,
    [switch]$Stop,
    [switch]$Guest,
    [string]$DataID = '6AE73807BA1CE8CB',
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

function Stop-Lane {
    # close the preview window only (never touch personal Edge or harness nodes)
    Get-CimInstance Win32_Process -Filter "Name='msedge.exe'" |
        Where-Object { $_.CommandLine -like '*edge-preview*' } |
        ForEach-Object { Stop-Process -Id $_.ProcessId -Force -EA SilentlyContinue }
    Get-Process -Name MockServer, WebFront -EA SilentlyContinue | Stop-Process -Force
    Get-CimInstance Win32_Process -Filter "Name='node.exe'" |
        Where-Object { $_.CommandLine -match 'AsphyxiaCore\.js' } |
        ForEach-Object { Stop-Process -Id $_.ProcessId -Force -EA SilentlyContinue }
    Start-Sleep -Seconds 1
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
    # gate (WEBFRONT_GATE=1 + presenter secret) is a deployment thing.
    $env:WEBFRONT_GATE = '0'
    $env:WEBFRONT_AUTH = 'http://127.0.0.1:8083'
    $wd = 'C:\mfg\webfront\bin\Release\net8.0'
    $p = Start-Process -FilePath (Join-Path $wd 'WebFront.exe') -WorkingDirectory $wd -WindowStyle Hidden -PassThru `
        -RedirectStandardOutput ("C:\mfgtmp\webfront$port.out.txt") -RedirectStandardError ("C:\mfgtmp\webfront$port.err.txt")
    'WEBFRONT port={0} pid={1} upstream={2}' -f $port, $p.Id, $upstream
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
    $new = $html.Replace(', "-AUTOSERVE"', ', ' + $pin).Replace('"-AUTOSERVE", ', $pin + ', ').Replace('http://127.0.0.1:8090/aog', $frontUrl)
    if ($new -match '-AUTOSERVE') { throw 'play page rewrite failed (AUTOSERVE still present)' }
    Set-Content (Join-Path $webroot 'index_play.html') $new -Encoding UTF8 -NoNewline
    Remove-Item (Join-Path $webroot 'index_play.html.br') -Force -EA SilentlyContinue
}

# 0. stop lane
if ($Stop) { Stop-Lane; 'preview stopped (RyuNET/mock/webfront killed; static server 8094 left alone)'; exit 0 }
Stop-Lane

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
    $env:MOCK_HOLD_CS_WRITE_MS = '0'
    Start-Process -FilePath (Join-Path $m85 'MockServer.exe') -ArgumentList '8085' -WorkingDirectory $m85 `
        -WindowStyle Hidden -PassThru `
        -RedirectStandardOutput (Join-Path $m85 'stdout.txt') -RedirectStandardError (Join-Path $m85 'stderr.txt') | Out-Null
    Remove-Item Env:MOCK_HOLD_CS_WRITE_MS -EA SilentlyContinue
    if (-not (Wait-Port 8085 30)) { throw 'mock 8085 did not come up' }
    New-PlayPage 'http://127.0.0.1:8085/aog'
    'mock 8085 (no hold) + index_play.html (no AUTOSERVE) ready'
} else {
    # RyuNET core (asphyxia fork + mfg plugin), integrated AOG on 8083
    Start-Process node -ArgumentList 'AsphyxiaCore.js' -WorkingDirectory $ryuDist -WindowStyle Hidden `
        -RedirectStandardOutput 'C:\mfgtmp\ryunet.out.txt' -RedirectStandardError 'C:\mfgtmp\ryunet.err.txt' | Out-Null
    if (-not (Wait-Port 8083 120)) {
        'ryunet stderr tail:'; Get-Content 'C:\mfgtmp\ryunet.err.txt' -Tail 10 -EA SilentlyContinue
        throw 'RyuNET did not bind 8083'
    }
    $smoke = Invoke-WebRequest -Uri 'http://127.0.0.1:8083/' -UserAgent 'curl/8' -TimeoutSec 10 -UseBasicParsing
    if ($smoke.Content -notmatch 'RyuNET') { throw 'RyuNET smoke GET / missing VFG banner' }
    'RyuNET up: ' + ($smoke.Content.Trim() -join ' ')
    Start-Front 8090 'http://127.0.0.1:8083' 'loopback' | Out-Null
    if (-not (Wait-Port 8090 30)) { throw 'webfront 8090 did not come up' }
    $relay = Invoke-WebRequest -Uri 'http://127.0.0.1:8090/aog/appli_info' -Method Post -Body 'web_id=1' `
        -ContentType 'application/x-www-form-urlencoded' -UserAgent 'Mozilla/5.0 preview-probe' -TimeoutSec 20 -UseBasicParsing
    if ($relay.StatusCode -ne 200 -or $relay.Content -notmatch '<root>') { throw 'webfront relay to RyuNET failed' }
    'relay smoke: browser-UA POST /aog/appli_info -> 200 XML via RyuNET'
    New-PlayPage 'http://127.0.0.1:8090/aog'
    'index_play.html ready (no AUTOSERVE, FRONTURL via webfront relay)'
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
'tidy up later with: preview_game.ps1 -Stop (or the .bat with -Stop)'
