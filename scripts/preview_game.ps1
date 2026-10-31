# preview_game.ps1 - launch the rebuild game for manual preview: static server + mock
# front + a normal visible Edge window. Deliberately does NOT start the capture harness
# (no freeze/capture client) and, by default, NOT the in-game auto-play (-AUTOSERVE):
# you drive the UI. The window stays open until you close it.
#
# usage:
#   preview_game.ps1 / .bat          # interactive: holdless mock 8085, no -AUTOSERVE, you drive everything
#   preview_game.ps1 -Reassemble     # re-copy + re-patch out\webgl into the served webroot first
#   preview_game.ps1 -Park           # capture-style view: auto-play ON, FirstCharaSelect held on screen (1 h mock)
#   preview_game.ps1 -Stop           # close the preview window and kill mock/webfront procs
param(
    [switch]$Reassemble,
    [switch]$Park,
    [switch]$Stop,
    [string]$EdgeExe = 'C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe'
)
$ProgressPreference = 'SilentlyContinue'
$ErrorActionPreference = 'Stop'
$repo = 'C:\mfg\build\unity-project'
$webroot = 'C:\mfgtmp\webroot'
$profileDir = 'C:\mfgtmp\edge-preview'
$page = if ($Park) { 'index_park2.html' } else { 'index_play.html' }
$url = 'http://127.0.0.1:8094/aog/' + $page

if ($Stop) {
    Get-CimInstance Win32_Process -Filter "Name='msedge.exe'" |
        Where-Object { $_.CommandLine -like '*edge-preview*' } |
        ForEach-Object { Stop-Process -Id $_.ProcessId -Force -EA SilentlyContinue }
    Get-Process -Name MockServer, WebFront -EA SilentlyContinue | Stop-Process -Force
    'preview stopped (server mocks killed; static server 8094 left alone)'
    exit 0
}

# 1. served build freshness: reassemble when webroot missing or older than out\webgl
$src = Join-Path $repo 'out\webgl\Build\webgl.data'
$dst = Join-Path $webroot 'Build\webgl.data'
if (-not (Test-Path $dst)) { $Reassemble = $true }
else {
    $srcI = Get-Item $src -EA SilentlyContinue
    $dstI = Get-Item $dst
    if ($srcI -and $srcI.LastWriteTime -gt $dstI.LastWriteTime) { $Reassemble = $true }
}
if ($Reassemble) {
    'reassembling webroot from out\webgl ...'
    & 'C:\mfg\scripts\assemble_webroot.ps1' | Select-Object -Last 2
}

# 2. fresh mocks + park pages (mock8083 55 s hold, mock8084 1 h hold for park viewing)
Get-Process -Name MockServer, WebFront -EA SilentlyContinue | Stop-Process -Force
Start-Sleep -Seconds 1
& 'C:\mfgtmp\step2_park.ps1' | Select-Object -First 6 | ForEach-Object { "$_" }

# 2b. interactive lane: holdless mock on 8085 + play page without -AUTOSERVE
if (-not $Park) {
    $m85 = 'C:\mfgtmp\mock8085'
    if (-not (Test-Path (Join-Path $m85 'MockServer.exe'))) { Copy-Item 'C:\mfgtmp\mock8084' $m85 -Recurse }
    $env:MOCK_HOLD_CS_WRITE_MS = '0'
    Start-Process -FilePath (Join-Path $m85 'MockServer.exe') -ArgumentList '8085' -WorkingDirectory $m85 `
        -WindowStyle Hidden -PassThru `
        -RedirectStandardOutput (Join-Path $m85 'stdout.txt') -RedirectStandardError (Join-Path $m85 'stderr.txt') | Out-Null
    Remove-Item Env:MOCK_HOLD_CS_WRITE_MS -EA SilentlyContinue
    $html = Get-Content 'C:\mfgtmp\webroot\index.html' -Raw
    $new = $html.Replace('http://127.0.0.1:8090/aog', 'http://127.0.0.1:8085/aog')
    $new = $new.Replace(', "-AUTOSERVE"', '').Replace('"-AUTOSERVE", ', '')
    if ($new -eq $html -or $new -match '-AUTOSERVE') { throw 'play page rewrite failed (FRONTURL or AUTOSERVE)' }
    Set-Content 'C:\mfgtmp\webroot\index_play.html' $new -Encoding UTF8 -NoNewline
    Remove-Item 'C:\mfgtmp\webroot\index_play.html.br' -Force -EA SilentlyContinue
    'play mock 8085 (no hold) + index_play.html (no AUTOSERVE) ready'
}

# 3. static lane: node serve_aog on 8094 (bring up only if down)
if (-not (Get-NetTCPConnection -LocalPort 8094 -State Listen -EA SilentlyContinue)) {
    Start-Process -FilePath 'node' -ArgumentList 'C:\mfgtmp\serve_aog.js' -WindowStyle Hidden
    $t0 = Get-Date
    while (-not (Get-NetTCPConnection -LocalPort 8094 -State Listen -EA SilentlyContinue) -and ((Get-Date) - $t0).TotalSeconds -lt 10) { Start-Sleep -Milliseconds 500 }
    'static server started on 8094'
} else { 'static server already up on 8094' }
try { $null = Invoke-WebRequest -Uri $url -Method Head -TimeoutSec 8 -UseBasicParsing } catch { throw "park page missing on server: $url" }

# 4. visible game window: dedicated Edge profile so it never merges with your personal browser
if (-not (Test-Path $profileDir)) { New-Item -ItemType Directory -Path $profileDir | Out-Null }
$edge = Start-Process -FilePath $EdgeExe -PassThru -ArgumentList @(
    '--user-data-dir=' + $profileDir,
    '--no-first-run', '--no-default-browser-check', '--disable-features=msEdgeWelcomeExperience',
    '--app=' + $url
)
Start-Sleep -Seconds 3
if (-not (Get-Process -Id $edge.Id -EA SilentlyContinue)) {
    $any = Get-CimInstance Win32_Process -Filter "Name='msedge.exe'" | Where-Object { $_.CommandLine -like '*edge-preview*' }
    if (@($any).Count -eq 0) { throw 'preview window did not start (check msedge path)' }
}

'preview up: ' + $url
if ($Park) { 'mode: park (auto-play ON, FirstCharaSelect held ~1 h; interaction with select is frozen by design)' }
else { 'mode: interactive (no AUTOSERVE, no mock holds); skip the intro story with the on-screen skip button, then drive Home -> mode -> matching -> character select yourself' }
'no capture harness is running; the window stays open until you close it.'
'tidy up later with: preview_game.ps1 -Stop (or the .bat with -Stop)'
