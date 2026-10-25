# analyze_dlls.ps1 - read assembly identities, package versions, module refs and type
# surfaces out of managed dlls without executing them.
#   identity/refs/attributes: Assembly.LoadFile (reflection only; types never resolved)
#   type name surface:        System.Reflection.Metadata (PEReader, strings only)
# Used by docs/21-project-merge.md. Output: JSON + console summary.
#
# Usage:
#   pwsh -File scripts/analyze_dlls.ps1 -Path <dir with dlls> -Out <json path> [-Types]
param(
  [Parameter(Mandatory=$true)][string]$Path,
  [Parameter(Mandatory=$true)][string]$Out,
  [switch]$Types
)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Reflection.Metadata

function Get-TypeNames([string]$file) {
  # full type names (nested joined with '.'); strings only, no reference binding
  $fs = [System.IO.File]::OpenRead($file)
  try {
    $pe = [System.Reflection.PortableExecutable.PEReader]::new($fs)
    try {
      if (-not $pe.HasMetadata) { return $null }
      $md = [System.Reflection.Metadata.PEReaderExtensions]::GetMetadataReader($pe)
      $tl = New-Object System.Collections.Generic.List[string]
      foreach ($th in $md.TypeDefinitions) {
        $td = $md.GetTypeDefinition($th)
        $ns = $md.GetString($td.Namespace)
        $fq = if ($ns) { "$ns.$($md.GetString($td.Name))" } else { $md.GetString($td.Name) }
        $scope = @()
        $dh = $td.GetDeclaringType()
        while (-not $dh.IsNil) {
          $dt = $md.GetTypeDefinition($dh)
          $scope = @($md.GetString($dt.Name)) + $scope
          $dh = $dt.GetDeclaringType()
        }
        if ($scope.Count) { $fq = (($scope + $fq) -join '.') }
        $tl.Add($fq)
      }
      return $tl
    } finally { $pe.Dispose() }
  } finally { $fs.Dispose() }
}

$results = New-Object System.Collections.Generic.List[object]
Get-ChildItem $Path -Filter *.dll -File | Sort-Object Name | ForEach-Object {
  $file = $_.FullName
  $name = $_.Name
  try {
    $a = [System.Reflection.Assembly]::LoadFile($file)
    $info = $null; $pkg = $null; $tfm = $null; $meta = @()
    foreach ($ca in $a.GetCustomAttributes($true)) {
      switch ($ca.GetType().Name) {
        'AssemblyInformationalVersionAttribute' { $info = $ca.InformationalVersion }
        'AssemblyMetadataAttribute' {
          $meta += "$($ca.Name)=$($ca.Value)"
          if ($ca.Name -eq 'PackageVersion' -or $ca.Name -eq 'UnityPackageVersion') { $pkg = $ca.Value }
        }
        'TargetFrameworkAttribute' { $tfm = $ca.FrameworkName }
      }
    }
    if (-not $pkg -and $info) { $pkg = ($info -split '\+')[0] }
    $refs = $a.GetReferencedAssemblies() | ForEach-Object {
      [pscustomobject]@{ name = $_.Name; version = $_.Version.ToString() }
    }
    $item = [ordered]@{
      file      = $name
      name      = $a.GetName().Name
      version   = $a.GetName().Version.ToString()
      informational = $info
      package_version = $pkg
      tfm       = $tfm
      refs      = @($refs)
    }
    if ($Types) { $item['types'] = @(Get-TypeNames $file) }
    $results.Add([pscustomobject]$item)
  } catch {
    $results.Add([pscustomobject]@{ file = $name; error = $_.Exception.Message })
  }
}
$dir = Split-Path $Out -Parent
if ($dir -and -not (Test-Path $dir)) { New-Item -ItemType Directory -Path $dir -Force | Out-Null }
$results | ConvertTo-Json -Depth 6 | Set-Content $Out -Encoding utf8
"=== summary ==="
foreach ($r in $results) {
  if ($r.error) { "{0,-58} ERROR {1}" -f $r.file, $r.error; continue }
  "{0,-58} v={1,-12} info={2,-24} pkg={3}" -f $r.file, $r.version, $r.informational, $r.package_version
}
"wrote $Out ($($results.Count) assemblies)"
