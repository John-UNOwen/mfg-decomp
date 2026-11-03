$ErrorActionPreference = 'Stop'
$scene = 'C:\Users\<you>\Documents\GitHub\mfg-decomp\build\unity-project\Assets\MFG\Scenes\Login\Login.unity'
$lines = Get-Content -LiteralPath $scene -Encoding utf8

# script guid -> class
$sg = @{}
Get-ChildItem -LiteralPath 'C:\Users\<you>\Documents\GitHub\mfg-decomp\build\unity-project\Assets\Assembly-CSharp' -Recurse -Filter '*.cs.meta' -File | ForEach-Object {
    $head = Get-Content -LiteralPath $_.FullName -TotalCount 3 -Encoding utf8
    $m = ($head | Select-String -Pattern 'guid: ([0-9a-f]+)').Matches
    if ($m.Count -gt 0) { $sg[$m[0].Groups[1].Value] = ($_.Name -replace '\.cs\.meta$', '') }
}

# GameObject fileID -> name ; GameObject -> component fileIDs
$goName = @{}
$goComps = @{}
# component fileID -> { class, go, scriptGuid }
$comp = @{}
# transform fileID -> { go, children }
$tmap = @{}

for ($i = 0; $i -lt $lines.Count; $i++) {
    $l = $lines[$i]
    if ($l -notmatch '^--- !u!(\d+) &(\d+)$') { continue }
    $cls = $matches[1]; $id = $matches[2]
    $block = @()
    for ($j = $i + 1; $j -lt $lines.Count; $j++) {
        if ($lines[$j] -match '^--- ') { break }
        $block += $lines[$j]
    }
    if ($cls -eq '1') {
        $nm = ''
        foreach ($b in $block) { if ($b -match '^\s*m_Name: (.*)$') { $nm = $matches[1].Trim(); break } }
        $goName[$id] = $nm
        $comps = @()
        foreach ($b in $block) { if ($b -match '^\s*- component: \{fileID: (\d+)\}') { $comps += $matches[1] } }
        $goComps[$id] = $comps
    }
    elseif ($cls -eq '114') {
        $go = ''; $g2 = ''
        foreach ($b in $block) {
            if ($b -match '^\s*m_GameObject: \{fileID: (\d+)\}') { $go = $matches[1] }
            if ($b -match '^\s*m_Script: \{fileID: 11500000, guid: ([0-9a-f]+)') { $g2 = $matches[1] }
        }
        $comp[$id] = @{ go = $go; script = $g2; cls = 'MonoBehaviour' }
    }
    elseif ($cls -eq '224' -or $cls -eq '4') {
        $go = ''; $kids = @(); $inKid = $false
        foreach ($b in $block) {
            if ($b -match '^\s*m_GameObject: \{fileID: (\d+)\}') { $go = $matches[1] }
            elseif ($b -match '^\s*m_Children:\s*$') { $inKid = $true }
            elseif ($inKid -and $b -match '^\s*- \{fileID: (\d+)\}') { $kids += $matches[1] }
            elseif ($inKid -and $b -match '^\s*m_') { $inKid = $false }
        }
        $tmap[$id] = @{ go = $go; children = $kids }
    }
}

function ChildNames($tid) {
    if (-not $tmap.ContainsKey($tid)) { return '(no transform)' }
    $out = @()
    foreach ($ch in $tmap[$tid].children) {
        if ($tmap.ContainsKey($ch)) { $out += "'" + $goName[$tmap[$ch].go] + "'" } else { $out += '?'+$ch }
    }
    return ($out -join ', ')
}

foreach ($cid in $comp.Keys) {
    $clsName = $sg[$comp[$cid].script]
    if ($clsName -ne 'CardEntryManager' -and $clsName -ne 'TenkeyManager') { continue }
    $go = $comp[$cid].go
    # find transform of that go
    $tid = $null
    foreach ($t in $tmap.Keys) { if ($tmap[$t].go -eq $go) { $tid = $t; break } }
    "$clsName comp=$cid on GO $go '" + $goName[$go] + "' transform=$tid children: " + (ChildNames $tid)
}
