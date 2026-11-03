$ErrorActionPreference = 'Stop'
$scene = 'C:\Users\<you>\Documents\GitHub\mfg-decomp\build\unity-project\Assets\MFG\Scenes\Login\Login.unity'
$lines = Get-Content -LiteralPath $scene -Encoding utf8

# script guid -> class name (from Assembly-CSharp .cs.meta)
$sg = @{}
Get-ChildItem -LiteralPath 'C:\Users\<you>\Documents\GitHub\mfg-decomp\build\unity-project\Assets\Assembly-CSharp' -Recurse -Filter '*.cs.meta' -File | ForEach-Object {
    $head = Get-Content -LiteralPath $_.FullName -TotalCount 3 -Encoding utf8
    $m = ($head | Select-String -Pattern 'guid: ([0-9a-f]+)').Matches
    if ($m.Count -gt 0) { $sg[$m[0].Groups[1].Value] = ($_.Name -replace '\.cs\.meta$', '') }
}

# gather: names of objects named Window; components of GO 339/303; children of transforms 1409/1368
$goComponents = @{}
$curGo = $null
$tChildren = @{}
$tGo = @{}
foreach ($l in $lines) {
    if ($l -match '^--- !u!(\d+) &(\d+)') {
        $cls = $matches[1]; $id = $matches[2]
        if ($cls -eq '1') { $curGo = $id; $goComponents[$curGo] = @() }
        elseif ($cls -eq '224' -or $cls -eq '4') { $curGo = $null }
        else { $scriptGuid = '' ; $tChildren[$id] = @(); $script = $null }
        $script = $null; $goRef = $null; $kids = @(); $inKid = $false
        continue
    }
}
"placeholder"
