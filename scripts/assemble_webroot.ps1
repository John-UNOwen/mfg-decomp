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
# Canvas: the shipped Default template pins the canvas to 960x600 (aspect 1.6), but the game's
# CanvasScaler reference resolution is 1920x1080 (16:9), so that size clips the bottom UI (the
# end-of-match/continue button falls off-screen). Make the canvas fill the browser window instead;
# Unity matches the WebGL render target to the canvas DOM size, so a 16:9 window yields the full UI.
$idx2 = Get-ChildItem $Webroot -Filter 'index.html' | Select-Object -First 1
$html2 = Get-Content $idx2.FullName -Raw
$html2 = $html2 -replace '<canvas id="unity-canvas" width=960 height=600 ', '<canvas id="unity-canvas" width=1280 height=720 '
$css = '<style>/*MFG-CANVAS*/html,body{margin:0;padding:0;overflow:hidden;background:#000}' +
  '#unity-container{position:fixed!important;left:0!important;top:0!important;width:100vw!important;height:100vh!important;transform:none!important}' +
  '#unity-canvas{position:fixed!important;left:50%!important;top:50%!important;transform:translate(-50%,-50%)!important;' +
  'width:min(100vw,177.78vh)!important;height:min(56.25vw,100vh)!important;display:block!important}' +
  '#unity-loading-bar,#unity-logo,#unity-fullscreen-button,#unity-progress-bar-full{display:none!important}</style>'
if ($html2 -notmatch 'MFG-CANVAS') { $html2 = $html2 -replace '</head>', ($css + '</head>') }
Set-Content $idx2.FullName $html2 -Encoding UTF8 -NoNewline
'patched index.html canvas->letterbox-fill'
# The 2022.3 loader factory freezes `var arguments_=[]` at script load, before any
# config.arguments can reach it, so Environment.GetCommandLineArgs() in the wasm is empty.
# Rewrite the first `var arguments_=[]` in the built framework.js to parse the page URL
# query into argv tokens (-FLAG and -KEY=VALUE pairs), honoring a pre-set Module.arguments.
$fw = Join-Path $Webroot 'Build\webgl.framework.js'
if (Test-Path $fw) {
    $fwText = Get-Content $fw -Raw
    if ($fwText -notmatch 'MFG-URLARGV') {
        $inject = 'var arguments_=/*MFG-URLARGV*/(function(){var out=[];try{var s=location.search.substring(1).split("&");for(var i=0;i<s.length;i++){if(!s[i])continue;var kv=s[i].split("=");var k=decodeURIComponent(kv[0].replace(/\+/g," "));if(kv.length>1){out.push(k);out.push(decodeURIComponent(kv[1].replace(/\+/g," ")))}else{out.push(k)}}if(typeof Module!=="undefined"&&Module["arguments"]){out=Module["arguments"].concat(out)}}catch(e){}return out})()'
        $rx = [regex]'var arguments_=\[\]'
        $fwText = $rx.Replace($fwText, $inject, 1)
        Set-Content $fw $fwText -Encoding UTF8 -NoNewline
        'patched framework.js url->argv'
    } else { 'framework.js already url->argv patched' }
}
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
