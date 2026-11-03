$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'
$root = 'C:\mfg\tmp\RyuNET-core'
$lane = Join-Path $root 'scratch-run\lane-id'
if (Test-Path $lane) { Remove-Item -LiteralPath $lane -Recurse -Force }
New-Item -ItemType Directory -Force -Path (Join-Path $lane 'dist') | Out-Null
New-Item -ItemType Directory -Force -Path (Join-Path $lane 'build-env\assets') | Out-Null
New-Item -ItemType Directory -Force -Path (Join-Path $lane 'dist\savedata') | Out-Null
New-Item -ItemType Directory -Force -Path (Join-Path $lane 'dist\uploads') | Out-Null

# copy the live dist tree, minus runtime data
$src = Join-Path $root 'dist'
Get-ChildItem -LiteralPath $src -Force | Where-Object { $_.Name -notin @('savedata','uploads','config.ini','log.txt','ryunet.err.log','ryunet.out.log') } | ForEach-Object {
    if ($_.PSIsContainer) {
        Copy-Item -LiteralPath $_.FullName -Destination (Join-Path (Join-Path $lane 'dist') $_.Name) -Recurse -Force
    } else {
        Copy-Item -LiteralPath $_.FullName -Destination (Join-Path (Join-Path $lane 'dist') $_.Name) -Force
    }
}
Copy-Item -LiteralPath (Join-Path $root 'build-env\assets') -Destination (Join-Path $lane 'build-env\assets') -Recurse -Force

$ini = @'
port=8191
bind=127.0.0.1
ping_ip=127.0.0.1
matching_port=5791
allow_register=true
maintenance_mode=false
enable_paseli=false
webui_on_startup=false
server_name=AuditLane
server_tag=AUDIT
require_pcbid_auth=false
discord_bot_token=
discord_client_id=
tachi_client_id=
tachi_client_secret=
discord_client_secret=
ea_service_url=http://127.0.0.1:8191/aog
rich_presence_lookup=true
sdvx_music_root=
sdvx_custom_music_root=

[mfg@asphyxia]
VFG_VERBOSE=false
mfg_service_url=http://127.0.0.1:8191/aog
mfg_http_port=22431
mfg_http_bind=127.0.0.1
VFG_CARDMNG_MODE=compat
VFG_ACCOUNT_AUTH=card
VFG_CARD_AUTH_WINDOW=900
VFG_PRESENTER_SECRET=
'@
Set-Content -LiteralPath (Join-Path $lane 'dist\config.ini') -Value $ini -Encoding utf8 -NoNewline

$p = Start-Process node -ArgumentList 'AsphyxiaCore.js' -WorkingDirectory (Join-Path $lane 'dist') -WindowStyle Hidden -PassThru `
    -RedirectStandardOutput 'C:\mfg\scratch\lane_id.out.txt' -RedirectStandardError 'C:\mfg\scratch\lane_id.err.txt'
Set-Content -LiteralPath 'C:\mfg\scratch\lane_id.pid.txt' -Value $p.Id -Encoding utf8
"lane pid=$($p.Id)"
$up = $false
for ($i = 0; $i -lt 120; $i++) {
    if (Get-NetTCPConnection -LocalPort 8191 -State Listen -EA SilentlyContinue) { $up = $true; break }
    Start-Sleep -Milliseconds 500
}
"lane up=$up"
if (-not $up) { Get-Content 'C:\mfg\scratch\lane_id.err.txt' -Tail 15 }
