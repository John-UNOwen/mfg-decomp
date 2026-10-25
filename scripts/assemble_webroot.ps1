# Assemble the WebGL webroot and serve it.
#   .\scripts\assemble_webroot.ps1            # assemble + brotli sidecars (no server)
#   .\scripts\assemble_webroot.ps1 -Serve     # also start webfront on :8090 against mock :8080
# Layout: copy of out\webgl (index.html, Build/, StreamingAssets/, TemplateData/) under
# C:\mfgtmp\webroot, .br sidecars next to compressible files. Docs/22 contract.
param(
    [switch]$Serve,
    [string]$Build = 'C:\mfg\build\unity-project\out\webgl',
    [string]$Webroot = 'C:\mfgtmp\webroot',
    [string]$Upstream = 'http://127.0.0.1:8080'
)
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path)

if (-not (Test-Path $Build)) { throw "no webgl build at $Build" }
if (Test-Path $Webroot) { Remove-Item $Webroot -Recurse -Force }
Copy-Item $Build $Webroot -Recurse
# belt and suspenders: inject explicit player args into the generated index.html so
# Environment.GetCommandLineArgs in the browser sees the rebuild flag cluster regardless
# of how URL-query-to-argv mapping behaves in this loader version.
$idx = Get-ChildItem $Webroot -Filter 'index.html' | Select-Object -First 1
$html = Get-Content $idx.FullName -Raw
if ($html -notmatch 'arguments:') {
    $args2 = 'arguments: ["-KONAMINET", "-FRONTURL", "http://127.0.0.1:8090/aog", "-GAMELOG", "-NOIO", "-appBehavior", "NETCAFE", "-AUTOSERVE"], '
    $html = $html -replace '(createUnityInstance\(([^,]+),\s*)', ('$1' + $args2)
    Set-Content $idx.FullName $html -NoNewline
    'patched index.html createUnityInstance arguments'
} else { 'index.html already has arguments' }
# brotli sidecars
& (Join-Path $root 'scripts\brotli-sidecar\bin\Release\net8.0\sidecar.exe') $Webroot
if ($Serve) {
    $env:WEBFRONT_ROOT = $Webroot
    $env:WEBFRONT_UPSTREAM = $Upstream
    Start-Process -FilePath (Join-Path $root 'webfront\bin\Release\net8.0\WebFront.exe') `
        -WorkingDirectory (Join-Path $root 'webfront\bin\Release\net8.0') `
        -RedirectStandardError (Join-Path $Webroot 'webfront.err.txt') `
        -RedirectStandardOutput (Join-Path $Webroot 'webfront.out.txt') `
        -WindowStyle Hidden
    'webfront started (WEBFRONT_UPSTREAM=' + $Upstream + ')'
}
