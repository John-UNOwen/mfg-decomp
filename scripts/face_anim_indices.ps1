# Extract face-expression index curves from Face_*.anim clips (Unity anim YAML:
# "- curve:" block lists keyframe "value:" lines, then "attribute:" names the curve).
param([string]$CharDir = 'C:\mfg\build\unity-project\Assets\MFG\Characters\Character7\3D\Animations\Motion\Face',
       [string]$Out = 'C:\mfgtmp\anim_index_map.txt')
$attrs = '_leftEyeIndex','_leftEyebrowIndex','_leftPupliIndex','_rightEyeIndex','_rightEyebrowIndex','_rightPupliIndex','_mouthIndex','_defaultCheekValue','CheekValue'
$lines = @()
Get-ChildItem $CharDir -Filter *.anim | Sort-Object Name | ForEach-Object {
    $clip = $_.BaseName
    $buf = @(); $curve = ''
    foreach ($ln in (Get-Content $_.FullName)) {
        $t = $ln.Trim()
        if ($t -eq '- curve:') { $buf = @(); $curve = ''; continue }
        if ($t.StartsWith('value: ')) { $buf += $t.Substring(7); continue }
        if ($t.StartsWith('attribute: ')) {
            $a = $t.Substring(11)
            if ($attrs -contains $a -and $buf.Count) { $lines += "$clip`t$a`t$(($buf | Select-Object -Unique) -join ',')" }
            $buf = @(); $curve = ''
            continue
        }
    }
}
$lines | Set-Content -Encoding UTF8 $Out
"lines=$($lines.Count) -> $Out"
