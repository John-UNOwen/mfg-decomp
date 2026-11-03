$ErrorActionPreference = 'Stop'
$anim = 'C:\Users\<you>\Documents\GitHub\mfg-decomp\build\unity-project\Assets\AnimationClip\CardEntryLoadingBgEffect.anim'
$lines = Get-Content -LiteralPath $anim -Encoding utf8
$times = @(); $guids = @()
$rows = @()
foreach ($ln in $lines) {
    $t = $ln.Trim()
    if ($t -match '^- time: ([0-9.]+)$') { $times += [double]$matches[1]; $guids += ''; continue }
    if ($t -match '^value: \{fileID: \d+, guid: ([0-9a-f]+),') {
        if ($guids.Count -gt 0) { $guids[$guids.Count - 1] = $matches[1] }
        continue
    }
    if ($t -match '^path: (Line\d+)$') {
        $rows += , @{ line = $matches[1]; times = $times.Clone(); guids = $guids.Clone() }
        $times = @(); $guids = @()
        continue
    }
}
$map = @{}
Get-ChildItem -LiteralPath 'C:\Users\<you>\Documents\GitHub\mfg-decomp\build\unity-project\Assets\Sprite' -Filter 'CardEntryEfColorBar*.asset.meta' -File | ForEach-Object {
    $head = Get-Content -LiteralPath $_.FullName -TotalCount 3 -Encoding utf8
    $g = ($head | Select-String -Pattern 'guid: ([0-9a-f]+)').Matches[0].Groups[1].Value
    $map[$g] = ($_.Name -replace '\.asset\.meta$', '')
}
foreach ($r in $rows) {
    $n = $r.guids.Count
    $f = $map[$r.guids[0]]; $l = $map[$r.guids[$n - 1]]
    "line=$($r.line) n=$n t0=$($r.times[0]) tEnd=$($r.times[$n-1]) first=$f last=$l"
}
