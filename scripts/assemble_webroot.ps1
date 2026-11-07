# Assemble the WebGL webroot and serve it.
#   .\scripts\assemble_webroot.ps1            # assemble + brotli sidecars (no server)
#   .\scripts\assemble_webroot.ps1 -Serve     # also start webfront on :8090 against mock :8080
# Layout: copy of out\webgl (index.html, Build/, StreamingAssets/, TemplateData/) under
# C:\mfgtmp\webroot, .br sidecars next to compressible files. Docs/22 contract.
#
# Order (checkup 2026-11-06 item 13 - the Windows twin of ledger L60, fixed for the .sh in d4a1946):
# everything that can fail is checked BEFORE anything is destroyed, and the webroot is no longer
# edited in place. Steps 1-3 refuse, step 4 builds a staging copy, step 11 swaps it in:
#   1. validate the build tree, the tools and the -Webroot path itself
#   2. stop the gate this lane recorded, re-identified against the live process; a stale record is
#      cleared, an unidentifiable live pid is a refusal that keeps the record
#   3. refuse to replace a webroot a live front is serving - by what that front answers, not by a
#      pid file (the record nothing writes was the reason step 2 never stopped the running gate)
#   4. mirror the build into a staging directory next to the target   (was: wipe, then mirror)
#  5-9. the patch / stamp / sidecar pipeline, unchanged, on the staging copy
#  10. brotli sidecars, with the exit code checked (it was never checked)
#  11. swap the staged webroot in: old root aside, staged root in, aside deleted - reversible both
#      ways, so a failure after the swap cannot leave the served root half-deleted
#  12. optional webfront serve, on a probed port, recording the pid it started
param(
    [switch]$Serve,
    # Probe assembly: leave every gate process alone (the owner's gate may be serving another webroot).
    # It does not mean "replace the webroot whatever is serving it": step 3 still refuses a webroot a
    # live front answers on. -NoGateStop skips the stop, never the check.
    [switch]$NoGateStop,
    # Skip the brotli sidecar pass (a loopback probe lane serves plain files fine; saves ~5 min).
    [switch]$NoSidecar,
    [string]$Build = 'C:\mfg\build\unity-project\out\webgl',
    [string]$Webroot = 'C:\mfgtmp\webroot',
    [string]$Upstream = 'http://127.0.0.1:8080',
    # URL the injected player args use for the front; webfront proxies /aog to $Upstream.
    # For a browser in another machine (appsandbox guest) pass its reachable form,
    # e.g. http://192.168.2.27:8090/aog.
    [string]$FrontUrl = 'http://127.0.0.1:8090/aog',
    # 'any' serves on all interfaces so guest VMs can load the page; 'loopback' local only.
    [string]$Bind = 'any',
    # The gate pid record: read by step 2, written by step 12, written by tmp/restart_gate_only.ps1:26.
    # hosting_up.ps1 does not write it, which is why the stop step used to aim at a stale pid.
    [string]$GatePidFile = 'C:\mfgtmp\gate_pid.txt',
    # The cloudflared config whose ingress ports this script must never disturb: a front owning one of
    # those is what the public hostname serves, and taking it down is an operator decision.
    [string]$TunnelConfig = (Join-Path $env:USERPROFILE '.cloudflared\config.yml')
)
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path)

# ---------------------------- 0. helpers ----------------------------
# One failure voice, so every refusal says which script refused and why (assemble_webroot.sh:132).
function Fail([string]$m) { throw ('assemble_webroot: ' + $m) }

# Lexical normalisation: a trailing slash or a .. spelling must not slip past a path comparison
# (assemble_webroot.sh:241 _abs - readlink -f has no Windows equivalent for a missing path).
function Get-NormPath([string]$p) {
    if ([string]::IsNullOrWhiteSpace($p)) { return $null }
    try { return [System.IO.Path]::GetFullPath($p.TrimEnd('\')) } catch { return $null }
}

# ---------------------------- 1. validate before destroying ----------------------------
# The shipping script checked one thing (Test-Path $Build) and wiped: an interrupted Unity player
# build (Build\ present, index.html missing) or a missing sidecar was discovered AFTER the served
# root was gone, and nothing looked at what -Webroot pointed at at all. All of it is here now, in
# front of the gate stop and the swap.
$sidecarExe = Join-Path $root 'scripts\brotli-sidecar\bin\Release\net8.0\sidecar.exe'
$webfrontExe = Join-Path $root 'webfront\bin\Release\net8.0\WebFront.exe'

if (-not (Test-Path -LiteralPath $Build -PathType Container)) { Fail "no webgl build at $Build" }
if (-not (Test-Path -LiteralPath (Join-Path $Build 'index.html') -PathType Leaf)) {
    Fail "no index.html in ${Build}: refusing to replace $Webroot for a build that cannot be assembled (this is what a failed or interrupted player build leaves behind)"
}
$haveArtifact = $false
foreach ($a in 'webgl.loader.js', 'webgl.framework.js', 'webgl.wasm', 'webgl.data') {
    if (Test-Path -LiteralPath (Join-Path $Build (Join-Path 'Build' $a)) -PathType Leaf) { $haveArtifact = $true }
}
if (-not $haveArtifact) {
    Fail "no webgl.loader.js / webgl.framework.js / webgl.wasm / webgl.data under $(Join-Path $Build 'Build'): refusing to replace $Webroot"
}
if (-not $NoSidecar -and -not (Test-Path -LiteralPath $sidecarExe -PathType Leaf)) {
    Fail "no sidecar tool at ${sidecarExe}: refusing to replace $Webroot for an assembly that cannot finish (pass -NoSidecar to skip the sidecars)"
}
if ($Serve -and -not (Test-Path -LiteralPath $webfrontExe -PathType Leaf)) {
    Fail "no webfront build at ${webfrontExe} (-Serve): refusing to replace $Webroot"
}
# -Serve prerequisites, checked here too: an assembly that is asked to serve on a port somebody
# else already holds must fail before the webroot moves, not after (the lane_up.sh:356-363 posture).
$servePort = 8090      # WEBFRONT_PORT unset -> WebFront's own default (Program.cs:27)
if ($env:WEBFRONT_PORT -match '^\d+$') { $servePort = [int]$env:WEBFRONT_PORT }
if ($Serve) {
    if (Get-NetTCPConnection -LocalPort $servePort -State Listen -EA SilentlyContinue) {
        Fail "something already answers 127.0.0.1:$servePort - refusing to start a webfront on it, so nothing is replaced. Stop that front first, or set WEBFRONT_PORT to a free port before -Serve."
    }
    if (Test-Path -LiteralPath $GatePidFile) {
        $rawRec = (Get-Content -LiteralPath $GatePidFile -Raw -EA SilentlyContinue)
        $recPid = if ($rawRec) { $rawRec.Trim() } else { '' }
        if ($recPid -match '^\d+$' -and (Get-Process -Id ([int]$recPid) -EA SilentlyContinue)) {
            Fail "-Serve would overwrite the gate record $GatePidFile while pid $recPid is live: stop that gate first, or assemble without -Serve."
        }
    }
}

# -Webroot itself: the wipe was Remove-Item -Recurse -Force with no guard, so any path was a
# candidate. Refuse the paths that are not a webroot, and both containment directions with the
# build tree (a root holding the build would delete the build; a root inside the build mirrors
# the build into itself) - the same list as assemble_webroot.sh:240-260.
$webrootFull = Get-NormPath $Webroot
if (-not $webrootFull) { Fail 'refusing to replace an empty -Webroot' }
if ($webrootFull -notmatch '^[A-Za-z]:\\.') { Fail "refusing to replace '$webrootFull': -Webroot must be a directory below a drive root, not a drive root or a UNC share" }
if (Test-Path -LiteralPath $Webroot -PathType Leaf) { Fail "-Webroot $Webroot is a file, not a webroot directory" }
$buildFull = Get-NormPath $Build
$refuseNames = [ordered]@{
    'operator home'                         = $env:USERPROFILE
    'repo root (this script lives under it)' = $root
    'lane data dir (gate record, lane logs)' = 'C:\mfgtmp'
    'Windows directory'                      = $env:SystemRoot
}
foreach ($what in $refuseNames.Keys) {
    $n = Get-NormPath $refuseNames[$what]
    if ($n -and ($webrootFull -ieq $n)) { Fail "refusing to replace ${Webroot}: it is the $what ($n), not a webroot" }
}
if ($buildFull -and $webrootFull -ieq $buildFull) { Fail "refusing to replace ${Webroot}: it is the build tree ($buildFull) itself" }
if ($buildFull -and $buildFull.StartsWith($webrootFull + '\', [System.StringComparison]::OrdinalIgnoreCase)) {
    Fail "refusing to replace ${Webroot}: the build tree ($buildFull) is inside it, so the swap would delete the build"
}
if ($buildFull -and $webrootFull.StartsWith($buildFull + '\', [System.StringComparison]::OrdinalIgnoreCase)) {
    Fail "refusing to assemble ${Webroot} inside the build tree ($buildFull): the mirror would copy the build into itself"
}
$targetFull = $webrootFull

# ---------------------------- 2. who serves this root, and is this run allowed to stop it ------------
# Two facts were conflated here, and separating them is what the 2026-11-07 lane outages came down to:
#   (a) a pid record named "the gate" - an ownership claim written by whoever restarted the gate last
#       (tmp/restart_gate_only.ps1 writes it, step 12 writes it), which says nothing about whether
#       THIS run owns that process;
#   (b) which front actually serves the webroot about to be replaced - provable with no record at all,
#       because a front answers /build_stamp.txt out of its own root (Program.cs:908-910 ServeStatic)
#       and this script writes that file on every run (measured on the live gate: GET
#       http://127.0.0.1:8090/build_stamp.txt -> 200, byte-identical to the file on disk).
# So the probe comes first. The recorded gate is stopped only when the probe says it is the front
# serving THIS root, and never when it owns a port the cloudflared ingress forwards to: that front is
# what the public hostname serves, and pulling it down is an operator decision, never a side effect of
# assembling a webroot. A gate that is live but serves a different root is left exactly as it is.
function Get-TunnelIngressPorts {
    # read-only: the local ports a cloudflared ingress forwards to
    if (-not (Test-Path -LiteralPath $TunnelConfig)) { return @() }
    @(Select-String -LiteralPath $TunnelConfig -Pattern '^\s*service:\s*https?://(?:localhost|127\.0\.0\.1|\[::1\]|0\.0\.0\.0):(\d+)' -EA SilentlyContinue |
        ForEach-Object { [int]$_.Matches[0].Groups[1].Value } | Select-Object -Unique)
}

function Get-FrontListener {
    $ownPids = @(Get-NetTCPConnection -State Listen -EA Stop | Select-Object -ExpandProperty OwningProcess -Unique)
    foreach ($op in $ownPids) {
        if ($op -le 3) { continue }
        $pr = Get-CimInstance Win32_Process -Filter "ProcessId=$op" -EA SilentlyContinue
        if (-not $pr) { continue }
        $isFront = ($pr.Name -ieq 'WebFront.exe') -or ($pr.Name -ieq 'dotnet.exe' -and $pr.CommandLine -imatch 'WebFront\.dll')
        if (-not $isFront) { continue }
        foreach ($port in @(Get-NetTCPConnection -State Listen -OwningProcess $op -EA SilentlyContinue | Select-Object -ExpandProperty LocalPort -Unique)) {
            [pscustomobject]@{ Pid = $op; Name = $pr.Name; Port = $port }
        }
    }
}

# Probe-Webroot <- <pid>:<port>. Returns $null (no answer) or @{ Status = <int>; Body = <string> }.
# A raw HTTP/1.0 request, because Invoke-WebRequest gives back no response object for a 404 (measured),
# and a 404 is exactly the evidence that the front serves some other root.
function Probe-Front([int]$port) {
    $c = New-Object System.Net.Sockets.TcpClient
    try {
        $c.Connect('127.0.0.1', $port)
        $s = $c.GetStream()
        $s.ReadTimeout = 4000
        $s.WriteTimeout = 4000
        $req = [System.Text.Encoding]::ASCII.GetBytes("GET /build_stamp.txt HTTP/1.0`r`nHost: 127.0.0.1`r`nConnection: close`r`n`r`n")
        $s.Write($req, 0, $req.Length)
        $ms = New-Object System.IO.MemoryStream
        $buf = New-Object byte[] 4096
        try { while ($true) { $n = $s.Read($buf, 0, $buf.Length); if ($n -le 0) { break }; $ms.Write($buf, 0, $n); if ($ms.Length -gt 65536) { break } } } catch { }
        $bytes = $ms.ToArray()
    } finally { $c.Close() }
    if ($bytes.Length -eq 0) { return $null }
    $text = [System.Text.Encoding]::UTF8.GetString($bytes)
    $status = 0
    if ($text -match '^HTTP/\d(?:\.\d)? (\d{3})') { $status = [int]$Matches[1] }
    $body = ''
    $cut = $text.IndexOf("`r`n`r`n")
    if ($cut -ge 0) { $body = $text.Substring($cut + 4) }
    return @{ Status = $status; Body = $body.Trim() }
}

function Get-DiskStamp {
    $f = Join-Path $targetFull 'build_stamp.txt'
    if (Test-Path -LiteralPath $f -PathType Leaf) { return (Get-Content -LiteralPath $f -Raw).Trim() }
    return ''
}

# The front answering with this webroot's own stamp, or $null. Matching stamps is the only positive
# proof available that a given process serves the root this script is about to replace.
function Find-ServingFront($listeners) {
    $diskStamp = Get-DiskStamp
    if (-not $diskStamp) { return $null }
    foreach ($l in $listeners) {
        $probe = $null
        try { $probe = Probe-Front $l.Port } catch { $probe = $null }
        if ($probe -and ($probe.Body -ieq $diskStamp)) { return $l }
    }
    return $null
}

$ingressPorts = Get-TunnelIngressPorts
if ($ingressPorts.Count -gt 0) { 'tunnel ingress ports this script will never disturb: ' + ($ingressPorts -join ', ') }
$fronts = @()
try { $fronts = @(Get-FrontListener) } catch {
    Fail "cannot list the listening ports to check who serves $targetFull ($($_.Exception.Message)): refusing to replace a webroot that may be served."
}
$serving = $null
if ((Test-Path -LiteralPath $targetFull) -and $fronts.Count -gt 0) { $serving = Find-ServingFront $fronts }
if ($serving) { "this webroot is being served right now by pid $($serving.Pid) ($($serving.Name)) on 127.0.0.1:$($serving.Port)" }

function Stop-RecordedGate {
    if ($NoGateStop) { 'gate stop skipped (-NoGateStop): no process was touched'; return }
    if (-not (Test-Path -LiteralPath $GatePidFile)) { return }
    $raw = (Get-Content -LiteralPath $GatePidFile -Raw -EA SilentlyContinue)
    $rec = if ($raw) { $raw.Trim() } else { '' }
    if ($rec -notmatch '^\d+$') { "gate record $GatePidFile holds no usable pid ('$rec'): nothing signalled, record kept"; return }
    $gp = [int]$rec
    if ($gp -le 3) { "gate record holds pid ${gp}: system pids are never signalled, record kept"; return }
    if ($gp -eq $PID) { "gate record holds this script's own pid ${gp}: nothing signalled, record kept"; return }
    $gpProc = Get-CimInstance Win32_Process -Filter "ProcessId=$gp" -EA SilentlyContinue
    if (-not $gpProc) { "gate pid $gp is not running: stale record $GatePidFile removed"; Remove-Item -LiteralPath $GatePidFile -Force -EA SilentlyContinue; return }
    if ($gpProc.Name -ne 'WebFront.exe') {
        Fail "the gate record $GatePidFile holds pid $gp, which is $($gpProc.Name), not WebFront.exe: refusing to replace $Webroot while a live process this kit cannot identify is recorded as its gate. Fix or remove the record first."
    }
    # A front owning a tunnel ingress port is what the public hostname serves. Not this script's to
    # stop, whatever the pid record claims: on 2026-11-07 an assemble run on a preview lane stopped
    # the hosted gate through exactly such a record and took the public site down.
    $gpPorts = @(Get-NetTCPConnection -State Listen -OwningProcess $gp -EA SilentlyContinue | Select-Object -ExpandProperty LocalPort -Unique)
    foreach ($ip in $ingressPorts) {
        if ($gpPorts -contains $ip) {
            Fail "the recorded gate pid $gp owns 127.0.0.1:$ip, which the cloudflared ingress in $TunnelConfig forwards to: that is the hosted front behind the public hostname. Stop the hosted lane first (scripts/hosting_down.ps1) if this root really is meant to be replaced."
        }
    }
    # The probe decides who is safe to stop, not the record: a live gate serving a different root is
    # left alone, and a different pid serving THIS root is not ours to stop either.
    if (-not $serving) {
        "the recorded gate pid $gp is live but does not serve ${targetFull}: left running, record kept"
        return
    }
    if ($serving.Pid -ne $gp) {
        Fail "pid $gp is the recorded gate, but the front serving $targetFull is pid $($serving.Pid) ($($serving.Name)) on 127.0.0.1:$($serving.Port): this script will not stop a process that is not the one serving the root. Stop that front yourself, or point -Webroot at another root."
    }
    # Identity proven twice over - recorded, and proved by probe to serve this root: stop this one
    # pid, gently first, then force, both bounded (the TERM-then-KILL
    # ladder of assemble_webroot.sh:321-332).
    Stop-Process -Id $gp -EA SilentlyContinue
    $gone = $false
    for ($i = 0; $i -lt 20; $i++) {
        if (-not (Get-Process -Id $gp -EA SilentlyContinue)) { $gone = $true; break }
        Start-Sleep -Milliseconds 250
    }
    if (-not $gone) {
        Stop-Process -Id $gp -Force -EA SilentlyContinue
        for ($i = 0; $i -lt 20; $i++) {
            if (-not (Get-Process -Id $gp -EA SilentlyContinue)) { $gone = $true; break }
            Start-Sleep -Milliseconds 250
        }
    }
    if (-not $gone) { Fail "gate pid $gp survived Stop-Process: refusing to replace $Webroot under a serving gate" }
    "stopped recorded gate pid ${gp}: record $GatePidFile cleared"
    Remove-Item -LiteralPath $GatePidFile -Force -EA SilentlyContinue
}
Stop-RecordedGate

# ---------------------------- 3. refuse to replace a webroot a live front is serving ----------------
# The last net, and the one that does not depend on a pid record at all: re-probe after the stop step
# and refuse if anything still answers this root (Get-FrontListener / Probe-Front live in step 2).
# Read-only - it never signals anything, so it also runs for -NoGateStop probe assembly.
function Assert-WebrootNotServed {
    if (-not (Test-Path -LiteralPath $targetFull)) { return }   # nothing can be served out of a root that is not there
    $listeners = @()
    try { $listeners = @(Get-FrontListener) } catch {
        Fail "cannot list the listening ports to check who serves $targetFull ($($_.Exception.Message)): refusing to replace a webroot that may be served."
    }
    if ($listeners.Count -eq 0) { return }
    $stampPath = Join-Path $targetFull 'build_stamp.txt'
    $diskStamp = ''
    if (Test-Path -LiteralPath $stampPath -PathType Leaf) { $diskStamp = (Get-Content -LiteralPath $stampPath -Raw).Trim() }
    foreach ($l in $listeners) {
        $probe = $null
        try { $probe = Probe-Front $l.Port } catch { $probe = $null }
        # The refusal only needs one thing to line up: the stamp the front answers is the stamp this
        # webroot carries. A different 200 body, or a 404 (this webroot HAS the file, so a root that
        # answers 404 is not this root), both prove the front serves something else. Anything else is
        # unknown, and unknown is a refusal - the whole point of the check is the case where nothing
        # in scripts/ recorded the gate for it to find.
        if ($probe -and $diskStamp -and ($probe.Body -ieq $diskStamp)) {
            Fail "pid $($l.Pid) ($($l.Name)) is serving $targetFull on 127.0.0.1:$($l.Port): it answers /build_stamp.txt with $($probe.Body), the stamp in this webroot. Stop that front before assembling this root (hosting_down.ps1, or stop the front you started), or point -Webroot at another root."
        }
        elseif ($probe -and $probe.Status -eq 200 -and $diskStamp) {
            "front pid $($l.Pid) port $($l.Port) serves a different build (stamp '$($probe.Body)' != '$diskStamp'): not this webroot"
        }
        elseif ($probe -and $diskStamp) {
            "front pid $($l.Pid) port $($l.Port) answers /build_stamp.txt with HTTP $($probe.Status), not this webroot's stamp: not this webroot"
        }
        elseif ($probe) {
            Fail "cannot prove pid $($l.Pid) ($($l.Name)) on 127.0.0.1:$($l.Port) is not serving ${targetFull}: it answered HTTP $($probe.Status) and the target has no build_stamp.txt to compare. Assemble into a fresh root (or stop the front) first."
        }
        else {
            Fail "cannot prove pid $($l.Pid) ($($l.Name)) on 127.0.0.1:$($l.Port) is not serving ${targetFull}: it holds the port but did not answer /build_stamp.txt. Stop it first or choose another -Webroot."
        }
    }
}
Assert-WebrootNotServed

# ---------------------------- 4. stage: mirror the build next to the target, not over it ----------------------------
# The old shape was Remove-Item $Webroot -Recurse -Force; Copy-Item $Build $Webroot -Recurse: the
# served root was gone before the copy could fail, and every check after it (index.html at :43, the
# sidecar at :134) could leave the lane with a half-deleted root and no undo.
$staged = $null
$keepStaged = $false
trap {
    if ($staged -and (Test-Path -LiteralPath $staged)) {
        if ($keepStaged) { 'assemble_webroot: the staged copy was kept for inspection: ' + $staged }
        else {
            Remove-Item -LiteralPath $staged -Recurse -Force -EA SilentlyContinue
            'assemble_webroot: assembly failed; the staged copy was removed and ' + $targetFull + ' was left untouched'
        }
    }
    break
}
$staged = $targetFull + '.assembling-' + [System.Guid]::NewGuid().ToString('N').Substring(0, 8)
# Same Copy-Item call as before, and still to a destination that does not exist yet: that is what
# makes the build tree's CONTENTS the webroot instead of a <build leaf> subdirectory of it.
Copy-Item $Build $staged -Recurse
$Webroot = $staged        # steps 5-10 patch the staged copy; step 11 swaps it into $targetFull
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
# Auto-refresh watchdog. The stamp file lives in the webroot; open game pages poll it every
# 2 minutes with a random nonce (?n= makes the URL unique, so neither the browser cache nor
# the Cloudflare default text cache can answer from a stale copy). On stamp mismatch the page
# shows a 'New game version available / Reload' prompt; the player picks the moment.
# Catalog fetch stamp: addressables fetch StreamingAssets/aa/catalog.json at a fixed URL that
# the gate caches for an hour; a new page with a browser-cached catalog requests last build's
# bundle file names -> 404 storm after any full repack. Unity WebGL UnityWebRequest rides
# XMLHttpRequest, so stamping every catalog XHR with ?v=<stamp> gives it a per-build cache key
# (the gate ignores query strings). Must run before the loader scripts, hence injected first.
Set-Content -LiteralPath (Join-Path $Webroot 'build_stamp.txt') $stamp -Encoding UTF8 -NoNewline
$inject = @'
<script>
(function(){var V="__STAMP__";var o=XMLHttpRequest.prototype.open;XMLHttpRequest.prototype.open=function(m,u){try{if(typeof u==="string"&&/\/aa\/catalog\.json(\?|$)/.test(u)){u+=(u.indexOf("?")>=0?"&":"?")+"v="+V;}}catch(e){}return o.apply(this,arguments);};})();
</script>
<script>
(function(){var STAMP="__STAMP__";function chk(){fetch("/build_stamp.txt?n="+Math.random(),{cache:"no-store"}).then(function(r){return r.ok?r.text():"";}).then(function(t){var s=(t||"").trim();if(s&&s!==STAMP)show();}).catch(function(){});}function show(){if(document.getElementById("mfg-update"))return;var d=document.createElement("div");d.id="mfg-update";d.style.cssText="position:fixed;left:50%;bottom:18px;transform:translateX(-50%);z-index:99999;background:#181820;color:#fff;padding:10px 14px;border-radius:8px;font:14px system-ui,sans-serif;box-shadow:0 4px 14px rgba(0,0,0,.45);display:flex;gap:10px;align-items:center";var m=document.createElement("span");m.textContent="New game version available";var b=document.createElement("button");b.textContent="Reload";b.style.cssText="background:#4a6cf7;color:#fff;border:0;border-radius:6px;padding:6px 12px;cursor:pointer";b.onclick=function(){location.reload();};d.appendChild(m);d.appendChild(b);document.body.appendChild(d);}setTimeout(chk,20000);setInterval(chk,120000);})();
</script>
'@
$inject = $inject.Replace('__STAMP__', $stamp)
$inject = $inject.Replace('__STAMP__', $stamp)
if ($html3 -match '</head>') {
    $html3 = $html3.Replace('</head>', $inject + '</head>')
} else {
    $html3 += $inject
}
Set-Content $idx2.FullName $html3 -Encoding UTF8 -NoNewline
'stamped build asset urls v=' + $stamp + '; catalog-fetch stamp + auto-refresh watchdog baked'
# ---------------------------- 10. brotli sidecars ----------------------------
# ps1 :133-135 called. Now on the staged copy, and its exit code is read: the old call threw the
# result away, so a sidecar that died half way still stamped the webroot as finished.
if ($NoSidecar) { 'skipped brotli sidecars (-NoSidecar)' } else {
    & $sidecarExe $Webroot
    if ($LASTEXITCODE -ne 0) {
        $keepStaged = $true
        Fail "the brotli sidecar exited ${LASTEXITCODE}: refusing to publish a webroot without its sidecars (the staged copy is kept; re-run with -NoSidecar to skip the pass)"
    }
}

# ---------------------------- 11. swap the staged webroot into place ----------------------------
# Two renames instead of a wipe: the previous root only leaves the tree once the new one stands in
# its place, and Rename-Item is refused while a process holds the directory, so a front that
# somehow survived the checks in step 3 blocks the swap instead of being pulled out from under.
function Publish-StagedWebroot {
    $leaf = Split-Path -Leaf $targetFull
    $asideLeaf = $leaf + '.old-' + (Get-Date -Format 'yyyyMMddHHmmss') + '-' + $PID
    $aside = $null
    if (Test-Path -LiteralPath $targetFull) {
        try { Rename-Item -LiteralPath $targetFull -NewName $asideLeaf -EA Stop } catch {
            Fail "cannot move the existing webroot $targetFull aside ($($_.Exception.Message)) - a process is holding it. Nothing was replaced."
        }
        $aside = Join-Path (Split-Path -Parent $targetFull) $asideLeaf
    }
    try {
        Rename-Item -LiteralPath $staged -NewName $leaf -EA Stop
    } catch {
        $why = $_.Exception.Message
        if ($aside) {
            Rename-Item -LiteralPath $aside -NewName $leaf -EA SilentlyContinue
            Fail "cannot move the assembled webroot into place ($why): the previous webroot was put back at $targetFull, the staged copy is at $staged"
        }
        Fail "cannot move the assembled webroot into place ($why): the target was not there to begin with, the staged copy is at $staged"
    }
    if ($aside) { Remove-Item -LiteralPath $aside -Recurse -Force -EA SilentlyContinue }
    'webroot swapped in: ' + $targetFull + ' (the previous root was removed only after the new one was in place)'
    $script:staged = $null        # published: the trap in step 4 must not clean the target up
}
Publish-StagedWebroot
$Webroot = $targetFull

# ---------------------------- 12. optional serve ----------------------------
if ($Serve) {
    # The port and the gate record were probed in step 1, before anything was replaced; $servePort
    # is the port that was probed, so the front binds exactly what was checked.
    $env:WEBFRONT_ROOT = $Webroot
    $env:WEBFRONT_UPSTREAM = $Upstream
    $env:WEBFRONT_PORT = "$servePort"
    $env:WEBFRONT_BIND = $Bind
    $env:WEBFRONT_GATE = '0'   # dev serve: relay mode (hosted gate is a deployment setting)
    $started = Start-Process -FilePath $webfrontExe `
        -WorkingDirectory (Split-Path -Parent $webfrontExe) `
        -RedirectStandardError (Join-Path $env:TEMP 'webfront.err.txt') `
        -RedirectStandardOutput (Join-Path $env:TEMP 'webfront.out.txt') `
        -WindowStyle Hidden -PassThru
    # Record the front this script started in the file step 2 reads: an unrecorded gate is exactly
    # why the stop step used to aim at a stale pid while the serving gate kept running (ledger L47,
    # the Windows half that was never applied).
    try {
        New-Item -ItemType Directory -Force -Path (Split-Path -Parent $GatePidFile) | Out-Null
        $started.Id | Set-Content -LiteralPath $GatePidFile -Encoding utf8
        'webfront started (pid ' + $started.Id + ', recorded in ' + $GatePidFile + ') bind=' + $Bind + ' port=' + $servePort + ' WEBFRONT_UPSTREAM=' + $Upstream
    } catch {
        'webfront started (pid ' + $started.Id + ') but its pid could NOT be recorded in ' + $GatePidFile + ' (' + $_.Exception.Message + ')'
    }
}
