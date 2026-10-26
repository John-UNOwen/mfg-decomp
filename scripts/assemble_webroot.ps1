# Assemble the WebGL webroot and serve it.
#   .\scripts\assemble_webroot.ps1            # assemble + brotli sidecars (no server)
#   .\scripts\assemble_webroot.ps1 -Serve     # also start webfront on :8090 against mock :8080
# Layout: copy of out\webgl (index.html, Build/, StreamingAssets/, TemplateData/) under
# C:\mfgtmp\webroot, .br sidecars next to compressible files. Docs/22 contract.
param(
    [switch]$Serve,
    [string]$Build = 'C:\mfg\build\unity-project\out\webgl',
    [string]$Webroot = 'C:\mfgtmp\webroot',
    [string]$Upstream = 'http://127.0.0.1:8080',
    # URL the injected player args use for the front; webfront proxies /aog to $Upstream.
    # For a browser in another machine (appsandbox guest) pass its reachable form,
    # e.g. http://192.168.2.27:8090/aog.
    [string]$FrontUrl = 'http://127.0.0.1:8090/aog',
    # 'any' serves on all interfaces so guest VMs can load the page; 'loopback' local only.
    [string]$Bind = 'any'
)
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path)

if (-not (Test-Path $Build)) { throw "no webgl build at $Build" }
# a serving webfront holds its own log files inside an old webroot: stop it before wiping
Get-Process -Name WebFront -ErrorAction SilentlyContinue | ForEach-Object { Stop-Process -Id $_.Id -Force }
if (Test-Path $Webroot) { Remove-Item $Webroot -Recurse -Force }
Copy-Item $Build $Webroot -Recurse
# belt and suspenders: inject explicit player args into the generated index.html so
# Environment.GetCommandLineArgs in the browser sees the rebuild flag cluster regardless
# of how URL-query-to-argv mapping behaves in this loader version. The 2022.3 loader reads
# config.arguments, so the key goes inside the config object, not the call parameters.
$idx = Get-ChildItem $Webroot -Filter 'index.html' | Select-Object -First 1
$html = Get-Content $idx.FullName -Raw
if ($html -notmatch 'arguments:') {
    $args2 = 'var config = { arguments: ["-KONAMINET", "-FRONTURL", "' + $FrontUrl + '", "-GAMELOG", "-NOIO", "-appBehavior", "NETCAFE", "-AUTOSERVE"],'
    if ($html -match 'var config = \{') {
        $html = $html -replace 'var config = \{', $args2
    } else {
        # fallback for loaders without the config literal: pass an args object as second parameter
        $html = $html -replace '(createUnityInstance\(([^,]+),\s*)', ('$1' + '{ arguments: ["-KONAMINET", "-FRONTURL", "' + $FrontUrl + '", "-GAMELOG", "-NOIO", "-appBehavior", "NETCAFE", "-AUTOSERVE"] }, ')
    }
    Set-Content $idx.FullName $html -Encoding UTF8 -NoNewline
    'patched index.html config.arguments'
} else { 'index.html already has arguments' }
# brotli sidecars
& (Join-Path $root 'scripts\brotli-sidecar\bin\Release\net8.0\sidecar.exe') $Webroot
if ($Serve) {
    $env:WEBFRONT_ROOT = $Webroot
    $env:WEBFRONT_UPSTREAM = $Upstream
    $env:WEBFRONT_BIND = $Bind
    Start-Process -FilePath (Join-Path $root 'webfront\bin\Release\net8.0\WebFront.exe') `
        -WorkingDirectory (Join-Path $root 'webfront\bin\Release\net8.0') `
        -RedirectStandardError (Join-Path $env:TEMP 'webfront.err.txt') `
        -RedirectStandardOutput (Join-Path $env:TEMP 'webfront.out.txt') `
        -WindowStyle Hidden
    'webfront started (bind=' + $Bind + ' WEBFRONT_UPSTREAM=' + $Upstream + ')'
}
