param([switch]$Tunnel)
$ProgressPreference = 'SilentlyContinue'
$ErrorActionPreference = 'Stop'
# Production start (docs/28): core armed with the presenter secret + WebFront in
# hosted gate mode + optional cloudflared tunnel. Loopback binds throughout;
# Cloudflare reaches the origin only through the tunnel.

# Start from a clean posture (review round 2 major #13): leftover WEBFRONT_* /
# mfg_http_* / presenter env from earlier sessions or test lanes must never
# leak into a production launch.
$publicUrl = $env:WEBFRONT_PUBLIC_URL   # caller-supplied input, kept across the clear
foreach ($n in @('WEBFRONT_ROOT','WEBFRONT_UPSTREAM','WEBFRONT_AUTH','WEBFRONT_PORT','WEBFRONT_BIND','WEBFRONT_GATE',
                 'WEBFRONT_TRUST_PROXY','WEBFRONT_PRESENTER_SECRET','WEBFRONT_PRESENTER_SECRET_FILE',
                 'WEBFRONT_ACCESS_CODE','WEBFRONT_ACCESS_CODE_FILE','WEBFRONT_PAGE_FRONTURL',
                 'mfg_http_bind','mfg_http_port','VFG_PRESENTER_SECRET','WEB_SECURE_COOKIE')) {
    Remove-Item "env:$n" -EA SilentlyContinue
}

$secFile = Join-Path $env:USERPROFILE '.mfg-web\presenter.secret'
if (-not (Test-Path $secFile)) { throw 'presenter secret missing: create ' + $secFile + ' first (random bytes, see hosting_deploy.md)' }
$secret = (Get-Content $secFile -Raw).Trim()

$repoReal = (Get-Item 'C:\mfg' -EA SilentlyContinue).Target
if (-not $repoReal) { $repoReal = 'C:\mfg' }
$ryuDist = Join-Path $repoReal.TrimEnd('\') 'tmp\RyuNET-core\dist'

if (Get-Process -Name WebFront -EA SilentlyContinue) { throw 'a WebFront is already running - stop the lane first (preview_game.ps1 -Stop)' }
if (Get-NetTCPConnection -LocalPort 8083 -State Listen -EA SilentlyContinue) { throw 'a core already answers 8083 - stop the lane first' }

# core: presenter armed, native 22421 listener loopback-only, https-ready cookie
# config.ini is patched while the core is stopped (core rewrites it live);
# process env wins over config.ini in the plugin's bind precedence.
$ini = Join-Path $ryuDist 'config.ini'
if (Test-Path $ini) {
    (Get-Content $ini -Raw) -replace 'mfg_http_bind=[^\r\n]*', 'mfg_http_bind=127.0.0.1' | Set-Content $ini -Encoding UTF8 -NoNewline
}
elseif ($env:mfg_http_bind -ne '127.0.0.1') { throw 'no config.ini at ' + $ryuDist + ' - keep mfg_http_bind=127.0.0.1 in the environment' }
$env:VFG_PRESENTER_SECRET = $secret
$env:mfg_http_bind = '127.0.0.1'
# Secure cookies only make sense when the lane really terminates https (the
# -Tunnel shape). express-session silently drops the session cookie when it is
# marked Secure but the request is plain http - on a LAN http origin login
# would never stick (review round 2 major #8).
if ($Tunnel) { $env:WEB_SECURE_COOKIE = '1' }
Start-Process node -ArgumentList 'AsphyxiaCore.js' -WorkingDirectory $ryuDist -WindowStyle Hidden `
    -RedirectStandardOutput 'C:\mfgtmp\ryunet.out.txt' -RedirectStandardError 'C:\mfgtmp\ryunet.err.txt' | Out-Null
$up = $false
for ($i = 0; $i -lt 120; $i++) {
    if (Get-NetTCPConnection -LocalPort 8083 -State Listen -EA SilentlyContinue) { $up = $true; break }
    Start-Sleep -Milliseconds 500
}
if (-not $up) { Get-Content 'C:\mfgtmp\ryunet.err.txt' -Tail 12 -EA SilentlyContinue; throw 'core did not bind 8083' }
'core up: presenter armed, mfg_http_bind=127.0.0.1' + $(if ($Tunnel) { ', secure cookies on (https tunnel)' } else { ', http LAN posture' })

# webfront: hosted gate
$env:WEBFRONT_ROOT = 'C:\mfgtmp\webroot'
$env:WEBFRONT_UPSTREAM = 'http://127.0.0.1:8083'
$env:WEBFRONT_AUTH = 'http://127.0.0.1:8083'
$env:WEBFRONT_PORT = '8090'
$env:WEBFRONT_BIND = 'loopback'
$env:WEBFRONT_GATE = '1'
$env:WEBFRONT_PRESENTER_SECRET_FILE = $secFile
if ($Tunnel) { $env:WEBFRONT_TRUST_PROXY = 'cloudflare' }  # CF-Connecting-IP becomes the visitor identity
# Access-code lobby when the code file exists (hosting_deploy.md item 11).
$codeFile = Join-Path $env:USERPROFILE '.mfg-web\access.code'
if (Test-Path $codeFile) { $env:WEBFRONT_ACCESS_CODE_FILE = $codeFile }
if ($publicUrl) { $env:WEBFRONT_PAGE_FRONTURL = $publicUrl }  # optional absolute -FRONTURL
$wd = Join-Path $repoReal.TrimEnd('\') 'webfront\bin\Release\net8.0'
Start-Process -FilePath (Join-Path $wd 'WebFront.exe') -WorkingDirectory $wd -WindowStyle Hidden `
    -RedirectStandardOutput 'C:\mfgtmp\webfront8090.out.txt' -RedirectStandardError 'C:\mfgtmp\webfront8090.err.txt' | Out-Null
$up = $false
for ($i = 0; $i -lt 60; $i++) {
    if (Get-NetTCPConnection -LocalPort 8090 -State Listen -EA SilentlyContinue) { $up = $true; break }
    Start-Sleep -Milliseconds 500
}
if (-not $up) { Get-Content 'C:\mfgtmp\webfront8090.err.txt' -Tail 6 -EA SilentlyContinue; throw 'webfront did not bind 8090' }
Get-Content 'C:\mfgtmp\webfront8090.out.txt' -Tail 2

if ($Tunnel) {
    Start-Process 'C:\Program Files (x86)\cloudflared\cloudflared.exe' -ArgumentList 'tunnel', 'run', 'mfg' -WindowStyle Hidden `
        -RedirectStandardOutput 'C:\mfgtmp\cloudflared.out.txt' -RedirectStandardError 'C:\mfgtmp\cloudflared.err.txt' | Out-Null
    'cloudflared tunnel "mfg" starting (see C:\mfgtmp\cloudflared.out.txt)'
}
'hosted lane up - test at https://your-hostname: / anonymous must bounce to /login'
