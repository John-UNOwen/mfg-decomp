$ErrorActionPreference = 'Stop'
$f = 'C:\Users\<you>\Documents\GitHub\mfg-decomp\build\unity-project\Assets\MFG\Scenes\Login\Login.unity'
$lines = Get-Content -LiteralPath $f -Encoding utf8

$tmap = @{}   # transform fileID -> @{go;father;children}
$goname = @{} # go fileID -> name
$goactive = @{}
for ($i = 0; $i -lt $lines.Count; $i++) {
    $l = $lines[$i]
    if ($l -match '^--- !u!1 &(\d+)$') {
        $id = $matches[1]; $nm = ''; $act = ''
        for ($j = $i + 1; $j -lt [Math]::Min($i + 40, $lines.Count); $j++) {
            if ($lines[$j] -match '^--- ') { break }
            if ($lines[$j] -match '^\s*m_Name: (.*)$' -and $nm -eq '') { $nm = $matches[1].Trim() }
            if ($lines[$j] -match '^\s*m_IsActive: (\d)') { $act = $matches[1] }
        }
        $goname[$id] = $nm
        $goactive[$id] = $act
        continue
    }
    if ($l -match '^--- !u!(224|4) &(\d+)$') {
        $id = $matches[2]
        $go = ''; $father = ''; $children = @(); $inKid = $false
        for ($j = $i + 1; $j -lt $lines.Count -and $lines[$j] -notmatch '^--- '; $j++) {
            $t = $lines[$j]
            if ($t -match '^\s*m_GameObject: \{fileID: (\d+)\}') { $go = $matches[1] }
            elseif ($t -match '^\s*m_Father: \{fileID: (\d+)\}') { $father = $matches[1] }
            elseif ($t -match '^\s*m_Children:\s*$') { $inKid = $true }
            elseif ($inKid -and $t -match '^\s*- \{fileID: (\d+)\}') { $children += $matches[1] }
            elseif ($inKid -and $t -match '^\s*m_') { $inKid = $false }
        }
        $tmap[$id] = @{ go = $go; father = $father; children = $children }
    }
}

function ChildNames($tid) {
    if (-not $tmap.ContainsKey($tid)) { return '(none)' }
    $o = @()
    foreach ($ch in $tmap[$tid].children) { $o += "'" + $goname[$tmap[$ch].go] + "'" }
    return ($o -join ', ')
}

foreach ($start in '1409', '1368') {
    "=== ancestor chain from transform $start (" + $goname[$tmap[$start].go] + ") ==="
    $cur = $start; $d = 0
    while ($cur -and $cur -ne '0' -and $tmap.ContainsKey($cur)) {
        $info = $tmap[$cur]
        ("  " * $d) + "tid=$cur GO=" + $info.go + " name='" + $goname[$info.go] + "' isActive=" + $goactive[$info.go] + " children: " + (ChildNames $cur)
        $cur = $info.father
        $d++
    }
}
