<#
.SYNOPSIS
Rerunnable AssetRipper extraction recipe for the Mahjong Fight Girl dump (no Unity editor needed).
.DESCRIPTION
Drives AssetRipper GUI.Free 2.0.0 over its HTTP surface (it has no classic CLI). Verified endpoints:
  GET  /openapi.json                         machine-readable route list
  POST /Settings/Update  (form fields)       all export settings; a checkbox field present == true
  POST /LoadFolder {Path}                    load a game folder (works on <Name>_Data with no exe)
  POST /Export/UnityProject {Path, CreateSubfolder}   full project export straight to disk (Assets+ProjectSettings+Packages, with .meta GUIDs)
  POST /Export/PrimaryContent {Path, CreateSubfolder} JSON/GLB dump of the same collection (+ full decompiled scripts)
  POST /Reset                                drop the loaded collection
Notes: /Scenes/View, /Collections/View, /Assets/View, /FailedFiles/View return 404 in 2.0.0 unless you come from a
loaded view page, so all statistics here are computed from the export tree on disk. Loads of the full install take
~15 min and peak near 18 GB RSS; export takes ~5-7 min. Only the PID this script starts is ever stopped.
.EXAMPLE
  pwsh -File scripts/extract_content.ps1                      # full project export -> build/unity-project
  pwsh -File scripts/extract_content.ps1 -Kind PrimaryContent -OutDir build/export-primary-content
#>
param(
    [string]$Install = '\\MYAPPSANDBOX\c\Games\MFG\game\MFGClient_Data',
    [string]$Exe = 'C:\Users\ <you>\Documents\GitHub\mfg-decomp\tmp\AssetRipperCLI\AssetRipper.GUI.Free.exe',
    [string]$OutDir = 'C:\Users\ <you>\Documents\GitHub\mfg-decomp\build\unity-project',
    [ValidateSet('UnityProject', 'PrimaryContent')][string]$Kind = 'UnityProject',
    [int]$Port = 8123,
    [string]$UnityVersion = '2022.3.13f1',
    [switch]$KeepServer
)
$ErrorActionPreference = 'Stop'
$repo = Split-Path -Parent (Split-Path -Parent $PSCommandPath)
$work = Join-Path $repo 'tmp\assetripper'
New-Item -ItemType Directory -Force -Path $work | Out-Null
$logPath = Join-Path $work ("ar-{0}-{1}.log" -f $Kind, (Get-Date -Format 'yyyyMMdd-HHmmss'))

# 1. start the headless server. Quote the log path because the repo path contains a space.
$argLine = "--headless --port $Port --log-path `"$logPath`""
$proc = Start-Process -FilePath $Exe -ArgumentList $argLine -WorkingDirectory $work -PassThru -WindowStyle Hidden `
    -RedirectStandardOutput (Join-Path $work 'stdout.txt') -RedirectStandardError (Join-Path $work 'stderr.txt')
Write-Host "AssetRipper pid=$($proc.Id) port=$Port log=$logPath"
try {
    $ready = $false
    for ($i = 0; $i -lt 60; $i++) {
        Start-Sleep -Seconds 3
        try { Invoke-WebRequest -Uri "http://127.0.0.1:$Port/openapi.json" -UseBasicParsing -TimeoutSec 5 | Out-Null; $ready = $true; break } catch {}
    }
    if (-not $ready) { throw "server did not come up on port $Port" }

    # 2. settings. Selects are sent explicitly; every checkbox that must be ON is present with an empty value.
    $settings = @{
        DefaultVersion = $UnityVersion; TargetVersion = $UnityVersion
        BundledAssetsExportMode = 'DirectExport'; ScriptContentLevel = 'Level2'
        AudioExportFormat = 'Default'; ImageExportFormat = 'Png'; LightmapTextureExportFormat = 'Yaml'
        SpriteExportMode = 'Yaml'; ShaderExportMode = 'Dummy'; TextExportMode = 'Parse'
        ScriptLanguageVersion = 'AutoSafe'; ScriptExportMode = 'Hybrid'
        PreferOriginalTextureExtension = ''; SaveSettingsToDisk = ''
        IgnoreStreamingAssets = ''; EnablePrefabOutlining = ''
    }
    Invoke-WebRequest -Uri "http://127.0.0.1:$Port/Settings/Update" -Method Post -Body $settings -UseBasicParsing | Out-Null

    # 3. load the game (no MFGClient.exe exists; the _Data folder is the serializable root).
    $sw = [Diagnostics.Stopwatch]::StartNew()
    $r = Invoke-WebRequest -Uri "http://127.0.0.1:$Port/LoadFolder" -Method Post -Body @{ Path = $Install } -UseBasicParsing -TimeoutSec 3600
    Write-Host ("load: {0} in {1}s" -f $r.StatusCode, [math]::Round($sw.Elapsed.TotalSeconds, 1))

    # 4. export straight to disk (no zip). CreateSubfolder=false keeps the exact output path.
    New-Item -ItemType Directory -Force -Path $OutDir | Out-Null
    $sw = [Diagnostics.Stopwatch]::StartNew()
    $uri = "http://127.0.0.1:$Port/Export/$Kind"
    $r = Invoke-WebRequest -Uri $uri -Method Post -Body @{ Path = $OutDir; CreateSubfolder = 'false' } -UseBasicParsing -TimeoutSec 5400
    Write-Host ("export {0}: {1} in {2}s -> {3}" -f $Kind, $r.StatusCode, [math]::Round($sw.Elapsed.TotalSeconds, 1), $OutDir)
}
finally {
    if (-not $KeepServer) {
        Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue   # PID we started only; never kill by name
        Write-Host "stopped pid=$($proc.Id)"
    }
}
