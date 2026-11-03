$ErrorActionPreference = 'Stop'
$targets = @{
  'data' = 'C:\Users\<you>\Documents\GitHub\mfg-decomp\build\unity-project\out\webgl\Build\webgl.data'
  'wasm' = 'C:\Users\<you>\Documents\GitHub\mfg-decomp\build\unity-project\out\webgl\Build\webgl.wasm'
}
$markers = @(
  'MFGDBG auto card present',
  'MFGDBG returning card player resumes from saved state',
  'MFGDBG pinned guest identity resumes from saved state',
  'MFGDBG xrpc web send',
  'MFGDBG xrpc web ok',
  'MFGDBG load: coop grant done',
  'MFGDBG consume exception',
  'CardEntryManager.CoCallAuthPass error',
  '0000000000000000',
  '^[0-9A-F]{16}$'
)
foreach ($k in $targets.Keys) {
  $p = $targets[$k]
  $bytes = [System.IO.File]::ReadAllBytes($p)
  $text = [System.Text.Encoding]::GetEncoding('ISO-8859-1').GetString($bytes)
  "=== $k  size=$($bytes.Length) ==="
  foreach ($m in $markers) {
    $i = $text.IndexOf($m, [System.StringComparison]::Ordinal)
    "{0,-60} {1}" -f $m, $(if ($i -ge 0) { "FOUND at $i" } else { 'absent' })
  }
}
