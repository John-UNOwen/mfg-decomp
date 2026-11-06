<#
.SYNOPSIS
  Install the editor automation entry points into a Unity project's Assets\Editor without
  reverting a copy the project has already moved past.
.DESCRIPTION
  scripts/editor-automation/ holds the main-repo copies of the batch-mode entry points Unity
  compiles out of Assets\Editor. The build tree has its own copies and its own history: it
  carries BuildWebGLPlayerFast (the artifacts-only WebGL build, added in the build repo on
  2026-11-03) while the scripts/ copies are the older BuildWebGLPlayer-only shape. A plain
  Copy-Item -Force therefore reverts the tree every time scripts/prepare_project.ps1 runs,
  which is ledger L42. Newest-wins keeps both sides honest: whichever file was written last
  is the one Unity compiles.

  Metas are not touched: the .cs.meta beside an existing Assets\Editor copy carries its guid,
  and scenes reference the entry points by that guid.
#>
param(
  [string]$SourceDir = (Join-Path $PSScriptRoot 'editor-automation'),
  [string]$ProjectDir = (Join-Path $PSScriptRoot '..\build\unity-project')
)

$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'

if (-not (Test-Path -LiteralPath $SourceDir)) { throw "no editor automation source at $SourceDir" }
$dstDir = Join-Path $ProjectDir 'Assets\Editor'
if (-not (Test-Path -LiteralPath $dstDir)) { New-Item -ItemType Directory -Path $dstDir -Force | Out-Null }

$copied = 0
$kept = 0
foreach ($src in (Get-ChildItem -LiteralPath $SourceDir -Filter '*.cs')) {
  $dst = Join-Path $dstDir $src.Name
  if (Test-Path -LiteralPath $dst) {
    $d = Get-Item -LiteralPath $dst
    if ($d.LastWriteTime -gt $src.LastWriteTime) {
      $kept++
      Write-Host ("  kept {0}: the Assets\Editor copy ({1:yyyy-MM-dd}) is newer than scripts\editor-automation ({2:yyyy-MM-dd})" -f $src.Name, $d.LastWriteTime, $src.LastWriteTime)
      continue
    }
  }
  Copy-Item -LiteralPath $src.FullName -Destination $dst -Force
  $copied++
}

"editor automation: copied $copied script(s), kept $kept newer Assets\Editor copy(ies)"
