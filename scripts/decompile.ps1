# Recovers C# from managed assemblies with ilspycmd.
# Setup once: dotnet tool install --global ilspycmd
# Default targets are the arcade-platform wrapper layer (the port boundary), so output stays small
# enough to commit. Assembly-CSharp decompiles to out/ (gitignored) until we decide otherwise.
param(
  [string]$Install = '..',
  [string]$OutDir = 'api/recovered',
  [string[]]$Targets = @('kamunity.dll', 'Shirahama.dll', 'Shirahama.Rx.dll', 'Shirahama.Async.dll')
)
$ErrorActionPreference = 'Stop'
$ilspy = Join-Path $env:USERPROFILE '.dotnet\tools\ilspycmd.exe'
if (-not (Test-Path $ilspy)) { throw 'install first: dotnet tool install --global ilspycmd' }
$managed = Join-Path $Install 'game\MFGClient_Data\Managed'
foreach ($t in $Targets) {
  $dll = Join-Path $managed $t
  $dst = Join-Path $OutDir ($t -replace '\.dll$', '')
  New-Item -ItemType Directory -Force $dst | Out-Null
  & $ilspy -p -o $dst $dll
  "recovered: $t -> $dst"
}
