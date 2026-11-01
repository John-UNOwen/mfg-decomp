$ProgressPreference = 'SilentlyContinue'
$ErrorActionPreference = 'Stop'
# Nightly savedata backup (docs/28). Schedule:
#   schtasks /create /tn mfg-backup /sc daily /st 03:30 /f ^
#     /tr "pwsh -NoProfile -File C:\Users\<you>\Documents\GitHub\mfg-decomp\scripts\hosting_backups.ps1"

$repoReal = (Get-Item 'C:\mfg' -EA SilentlyContinue).Target
if (-not $repoReal) { $repoReal = 'C:\mfg' }
$saved = Join-Path $repoReal.TrimEnd('\') 'tmp\RyuNET-core\dist\savedata'
if (-not (Test-Path $saved)) { throw 'savedata dir not found: ' + $saved }

$dest = Join-Path $env:USERPROFILE '.mfg-web\backups'
New-Item -ItemType Directory -Path $dest -Force | Out-Null
$stamp = Get-Date -Format 'yyyy-MM-dd_HHmmss'
$out = Join-Path $dest ('savedata_' + $stamp)
Copy-Item $saved $out -Recurse
'backed up savedata -> ' + $out

# keep 14 newest copies
Get-ChildItem $dest -Directory -Filter 'savedata_*' |
    Sort-Object LastWriteTime -Descending |
    Select-Object -Skip 14 |
    ForEach-Object { Remove-Item $_.FullName -Recurse -Force }
