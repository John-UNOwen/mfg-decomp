$ErrorActionPreference = 'Stop'
$f = 'C:\Users\<you>\Documents\GitHub\mfg-decomp\build\unity-project\Assets\MFG\Scenes\Login\Login.unity'
$lines = Get-Content -LiteralPath $f -Encoding utf8

# Map: transform fileID -> { go, father, children }
$tmap = @{}
$goname = @{}
for ($i = 0; $i -lt $lines.Count; $i++) {
    $l = $lines[$i]
    if ($l -match '^--- !u!1 &(\d+)$') {
        $id = $matches[1]; $nm = ''
        for ($j = $i + 1; $j -lt [Math]::Min($i + 40, $lines.Count); $j++) {
            if ($lines[$j] -match '^--- ') { break }
            if ($lines[$j] -match '^\s*m_Name: (.*)$') { $nm = $matches[1].Trim(); break }
        }
        $goname[$id] = $nm
        continue
    }
    if ($l -match '^--- !u!(224|4|114|95|212) &(\d+)$') {
        if ($matches[1] -ne '224' -and $matches[1] -ne '4') { continue }
        $id = $matches[2]
        $go = ''; $father = ''; $children = @(); $inKid = $false
        $j = $i + 1
        while ($j -lt $lines.Count -and $lines[$j] -notmatch '^--- ') {
            $t = $lines[$j]
            if ($t -match '^\s*m_GameObject: \{fileID: (\d+)\}') { $go = $matches[1] }
            elseif ($t -match '^\s*m_Father: \{fileID: (\d+)\}') { $father = $matches[1] }
            elseif ($t -match '^\s*m_Children:\s*$') { $inKid = $true }
            elseif ($inKid -and $t -match '^\s*- \{fileID: (\d+)\}') { $children += $matches[1] }
            elseif ($inKid -and $t -match '^\s*m_|^\s*$') { if ($t -match '^\s*m_') { $inKid = $false } }
            $j++
        }
        $tmap[$id] = @{ go = $go; father = $father; children = $children }
    }
}
"transforms: $($tmap.Count) gameobjects: $($goname.Count)"

function Walk($tid, $depth) {
    if (-not $tmap.ContainsKey($tid)) { ("  " * $depth) + "tid=$tid (transform block not found)"; return }
    $info = $tmap[$tid]
    ("  " * $depth) + "tid=$tid name='" + $goname[$info.go] + "'"
    foreach ($ch in $info.children) { Walk $ch ($depth + 1) }
}

Walk '1409' 0
