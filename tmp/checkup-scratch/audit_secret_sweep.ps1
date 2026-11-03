$ProgressPreference = 'SilentlyContinue'
$secretFile = Join-Path $env:USERPROFILE '.mfg-web\presenter.secret'
$secret = (Get-Content $secretFile -Raw).Trim()
$code = (Get-Content (Join-Path $env:USERPROFILE '.mfg-web\access.code') -Raw).Trim()
$trees = @{
  main   = 'C:\mfg'
  core   = 'C:\mfg\tmp\RyuNET-core'
  plugin = 'C:\mfg\tmp\asphyxia_plugins'
}
foreach ($k in $trees.Keys) {
  $t = $trees[$k]
  foreach ($term in @($secret, $code, '<account-id>', '<DataID>', '<DataID>', '<owner-acct>')) {
    if ([string]::IsNullOrWhiteSpace($term)) { continue }
    $hits = git -C $t --no-pager grep -n -I --fixed-strings -- $term 2>$null
    $n = @($hits).Count
    if ($n -gt 0) {
      $label = if ($term -eq $secret) { 'PRESENTER_SECRET' } elseif ($term -eq $code) { 'ACCESS_CODE' } else { $term }
      "HIT tree=$k term=$label count=$n"
      $hits | Select-Object -First 5 | ForEach-Object { "   " + $_.Substring(0, [Math]::Min(150, $_.Length)) }
    }
  }
}
'grep sweep done'
