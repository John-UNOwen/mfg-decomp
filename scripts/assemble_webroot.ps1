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
# a serving webfront holds handles inside the old webroot: stop only the gate this
# lane recorded in gate_pid.txt (checked by process name). Never touch other processes;
# whoever restarted the gate must restart it again after the assemble.
$gatePidFile = 'C:\mfgtmp\gate_pid.txt'
if (Test-Path $gatePidFile) {
    $gp = 0
    if ([int]::TryParse((Get-Content $gatePidFile -Raw).Trim(), [ref]$gp)) {
        $gpProc = Get-CimInstance Win32_Process -Filter "ProcessId=$gp" -ErrorAction SilentlyContinue
        if ($gpProc -and $gpProc.Name -eq 'WebFront.exe') { Stop-Process -Id $gp -Force -ErrorAction SilentlyContinue }
    }
}
if (Test-Path $Webroot) { Remove-Item $Webroot -Recurse -Force }
Copy-Item $Build $Webroot -Recurse
# belt and suspenders: inject explicit player args into the generated index.html so
# Environment.GetCommandLineArgs in the browser sees the rebuild flag cluster regardless
# of how URL-query-to-argv mapping behaves in this loader version. The 2022.3 loader reads
# config.arguments, so the key goes inside the config object, not the call parameters.
$idx = Get-ChildItem $Webroot -Filter 'index.html' | Select-Object -First 1
$html = Get-Content $idx.FullName -Raw
if ($html -notmatch 'arguments:') {
    # Bake the cluster as a merge function so URL-query args also reach argv even when the
    # gate splices config.arguments (config wins over the loader's arguments_ in 2022.3).
    # The literal "-AUTOSERVE" token stays splice-anchored: the gate replaces it with the
    # per-account cluster; harness extras (-SERVECONTINUE etc.) ride in via ?query.
    $merge = '(function(){var base=["-KONAMINET", "-FRONTURL", "' + $FrontUrl + '", "-GAMELOG", "-NOIO", "-appBehavior", "NETCAFE", "-AUTOSERVE", "-TRANSLATE"];try{var q=location.search.substring(1).split("&");for(var i=0;i<q.length;i++){if(!q[i])continue;var kv=q[i].split("=");var k=decodeURIComponent(kv[0].replace(/\+/g," "));if(kv.length>1){base.push(k);base.push(decodeURIComponent(kv[1].replace(/\+/g," ")))}else{base.push(k)}}}catch(e){}return base})()'
    $args2 = 'var config = { arguments: ' + $merge + ','
    if ($html -match 'var config = \{') {
        $html = $html.Replace('var config = {', $args2)
    } else {
        # fallback for loaders without the config literal: pass an args object as second parameter
        $html = $html.Replace('createUnityInstance(canvas, ', 'createUnityInstance(canvas, { arguments: ' + $merge + ' }, ')
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
  '#unity-loading-bar,#unity-logo,#unity-fullscreen-button,#unity-progress-bar-full,#unity-webgl-logo,#unity-build-title{display:none!important}</style>'
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
# Cloudflare edge caches .js/.css but treats .wasm/.data as dynamic. After a rebuild the
# tunnel browser gets a stale framework.js glued to a fresh wasm - the pair is version-
# coupled and boot dies (undefined .apply). Stamp every Build asset URL in index.html with
# a hash over the payload files: new build = new cache keys on every asset. The gate serves
# the file and ignores the query (verified: /Build/x.js?v=... -> 200).
$stampFiles = @((Join-Path $Webroot 'Build\webgl.loader.js'), (Join-Path $Webroot 'Build\webgl.framework.js'), (Join-Path $Webroot 'Build\webgl.wasm'), (Join-Path $Webroot 'Build\webgl.data')) | Where-Object { Test-Path $_ }
$joined = ($stampFiles | ForEach-Object { (Get-FileHash $_ -Algorithm SHA256).Hash }) -join ''
$sha = [System.Security.Cryptography.SHA256]::Create()
$stamp = (([System.BitConverter]::ToString($sha.ComputeHash([System.Text.Encoding]::UTF8.GetBytes($joined)))).Replace('-', '')).Substring(0, 12)
$html3 = Get-Content $idx2.FullName -Raw
foreach ($asset in 'webgl.loader.js', 'webgl.framework.js', 'webgl.data', 'webgl.wasm') {
    $from = '"/' + $asset + '"'
    $html3 = $html3.Replace($from, ('"/' + $asset + '?v=' + $stamp + '"'))
}
Set-Content $idx2.FullName $html3 -Encoding UTF8 -NoNewline
'stamped build asset urls v=' + $stamp
# brotli sidecars
& (Join-Path $root 'scripts\brotli-sidecar\bin\Release\net8.0\sidecar.exe') $Webroot
if ($Serve) {
    $env:WEBFRONT_ROOT = $Webroot
    $env:WEBFRONT_UPSTREAM = $Upstream
    $env:WEBFRONT_BIND = $Bind
    $env:WEBFRONT_GATE = '0'   # dev serve: relay mode (hosted gate is a deployment setting)
    Start-Process -FilePath (Join-Path $root 'webfront\bin\Release\net8.0\WebFront.exe') `
        -WorkingDirectory (Join-Path $root 'webfront\bin\Release\net8.0') `
        -RedirectStandardError (Join-Path $env:TEMP 'webfront.err.txt') `
        -RedirectStandardOutput (Join-Path $env:TEMP 'webfront.out.txt') `
        -WindowStyle Hidden
    'webfront started (bind=' + $Bind + ' WEBFRONT_UPSTREAM=' + $Upstream + ')'
}
