# Remap band-placed expression-mesh UVs inside Unity Mesh .asset _typelessdata blobs.
# Rewrites ONLY the v float (channel offset+4) per record: v' = (v - 0.75) / 0.25.
# Streams are laid out per m_Channels list, each stream padded to 8 bytes (validated vs m_DataSize).
param([switch]$DryRun)
$ProgressPreference = 'SilentlyContinue'
$ErrorActionPreference = 'Stop'
$proj = 'C:\mfg\build\unity-project\Assets\'

function Remap([string]$rel, [int]$slot, [double]$expMin, [double]$expMax) {
    $path = $proj + $rel
    $raw = Get-Content $path -Raw
    $mCh = [regex]::Match($raw, 'm_Channels:\s*([\s\S]*?)m_DataSize:\s*(\d+)')
    if (-not $mCh.Success) { "SKIP $rel (no channels)"; return }
    $ds = [int]$mCh.Groups[2].Value
    $entries = [regex]::Matches($mCh.Groups[1].Value, '-\s*([\s\S]*?)(?=\n\s*-\s|\s*$)')
    $chs = @(); $i = 0
    foreach ($e in $entries) {
        $v = $e.Groups[1].Value
        $f = { param($k) [int][regex]::Match($v, "$k\s*:\s*(\d+)").Groups[1].Value }
        $chs += ,@{ stream=(& $f 'stream'); offset=(& $f 'offset'); format=(& $f 'format'); dimension=(& $f 'dimension') }
        $i++
    }
    if ($chs.Count -le $slot) { "SKIP $rel (channel list short: $($chs.Count))"; return }
    $tc = $chs[$slot]
    if ($tc.dimension -ne 2 -or $tc.format -ne 0) { "SKIP $rel slot$slot dim=$($tc.dimension) fmt=$($tc.format)"; return }
    $vc = [int][regex]::Match($raw, 'm_VertexCount:\s*(\d+)').Groups[1].Value
    $stride = @{}
    foreach ($c in $chs) { if ($c.dimension -gt 0) { $need = $c.offset + 4 * $c.dimension; if (-not $stride.ContainsKey($c.stream) -or $need -gt $stride[$c.stream]) { $stride[$c.stream] = $need } } }
    $start = 0; $sum = 0
    foreach ($s in ($stride.Keys | Sort-Object)) {
        if ($s -eq $tc.stream) { $start = $sum; break }
        $sum += ([math]::Ceiling($stride[$s] * $vc / 8) * 8)
    }
    $total = 0; foreach ($s in $stride.Keys) { $total += ([math]::Ceiling($stride[$s] * $vc / 8) * 8) }
    if ($total -ne $ds) { "SKIP $rel (layout sum $total != m_DataSize $ds)"; return }
    $rec = $stride[$tc.stream]
    $dlm = [regex]::Match($raw, '(?m)^(\s*)_typelessdata:\s*([0-9a-f]+)\s*$')
    if (-not $dlm.Success) { "SKIP $rel (no _typelessdata)"; return }
    $hex = $dlm.Groups[2].Value
    function GetFloat([int]$bytePos) { $h = $hex.Substring($bytePos * 2, 8); $b = [byte[]]::new(4); for ($k = 0; $k -lt 4; $k++) { $b[$k] = [Convert]::ToByte($h.Substring($k * 2, 2), 16) }; return [BitConverter]::ToSingle($b, 0) }
    $vbase = $start + $tc.offset + 4
    $mn = [double]::MaxValue; $mx = [double]::MinValue; $sumv = 0.0
    $old = [double[]]::new($vc)
    for ($i = 0; $i -lt $vc; $i++) { $f = GetFloat ($vbase + $i * $rec); $old[$i] = $f; $sumv += $f; if ($f -lt $mn) { $mn = $f }; if ($f -gt $mx) { $mx = $f } }
    $mean = $sumv / $vc
    if ([math]::Abs($mean - 0.8675) -lt 0.06 -or ($mn -gt ($expMin - 0.004) -and $mx -lt ($expMax + 0.004))) { $orig = $true } else { $orig = $false }
    "DECODE $rel slot$slot stream$($tc.stream) stride=$rec vbyte=$($tc.offset+4) n=$vc v=[$([math]::Round($mn,4)),${mx}] expected=[$expMin,$expMax] matchesOriginal=$orig"
    if (-not $orig) { return }
    if ($DryRun) { return }
    $sb = [System.Text.StringBuilder]::new($hex)
    for ($i = 0; $i -lt $vc; $i++) {
        $b = [BitConverter]::GetBytes([single](($old[$i] - 0.75) / 0.25))
        $pos = ($vbase + $i * $rec) * 2
        for ($k = 0; $k -lt 4; $k++) { [void]$sb.Remove($pos + $k * 2, 2).Insert($pos + $k * 2, $b[$k].ToString('x2')) }
    }
    $newHex = $sb.ToString()
    for ($i = 0; $i -lt $vc; $i++) {
        $h = $newHex.Substring(($vbase + $i * $rec) * 2, 8); $b = [byte[]]::new(4); for ($k = 0; $k -lt 4; $k++) { $b[$k] = [Convert]::ToByte($h.Substring($k * 2, 2), 16) }
        $chk = [BitConverter]::ToSingle($b, 0); if ([math]::Abs($chk - (($old[$i] - 0.75) / 0.25)) -gt 0.0001) { "VERIFY-FAIL $rel vertex $i"; return }
    }
    $newRaw = $raw.Substring(0, $dlm.Index) + $dlm.Groups[1].Value + '_typelessdata: ' + $newHex + "`n" + $raw.Substring($dlm.Index + $dlm.Length)
    [System.IO.File]::WriteAllText($path, $newRaw)
    $mn2 = [double]::MaxValue; $mx2 = [double]::MinValue
    $hex = $newHex
    for ($i = 0; $i -lt $vc; $i++) { $f = GetFloat ($vbase + $i * $rec); if ($f -lt $mn2) { $mn2 = $f }; if ($f -gt $mx2) { $mx2 = $f } }
    "WROTE  $rel newV=[$([math]::Round($mn2,4)),${mx2}]"
}

$mouths = @(
 @('MFG\Characters\Character2\3D\C001\Fbxs\model_face_mouth.asset', 0.8157, 0.9181),
 @('MFG\Characters\Character2\3D\C002\Fbxs\model_face_mouth.asset', 0.8157, 0.9181),
 @('MFG\Characters\Character2\3D\C003\Fbxs\model_face_mouth.asset', 0.8157, 0.9181),
 @('MFG\Characters\Character2\3D\C005\Fbxs\Sen_meido_mouth.asset',  0.8157, 0.9181),
 @('MFG\Characters\Character2\3D\C006\Fbxs\model_face_mouth.asset', 0.8157, 0.9181),
 @('MFG\Characters\Character2\3D\C011\Fbxs\model_face_mouth.asset', 0.8157, 0.9181),
 @('MFG\Characters\Character2\3D\C013\Fbxs\model_face_mouth.asset', 0.8157, 0.9181),
 @('MFG\Characters\Character4\3D\C001\Fbxs\model_face_mouth.asset', 0.8460, 0.9618),
 @('MFG\Characters\Character4\3D\C002\Fbxs\model_face_mouth.asset', 0.8460, 0.9618),
 @('MFG\Characters\Character4\3D\C003\Fbxs\model_face_mouth.asset', 0.8460, 0.9618),
 @('MFG\Characters\Character4\3D\C005\Fbxs\model_face_mouth.asset', 0.8460, 0.9618),
 @('MFG\Characters\Character4\3D\C006\Fbxs\model_face_mouth.asset', 0.8460, 0.9618),
 @('MFG\Characters\Character4\3D\C007\Fbxs\model_face_mouth.asset', 0.8460, 0.9618),
 @('MFG\Characters\Character4\3D\C009\Fbxs\model_face_mouth.asset', 0.8460, 0.9618),
 @('MFG\Characters\Character4\3D\C011\Fbxs\model_face_mouth.asset', 0.8460, 0.9618))
$brows = @(
 @('Mesh\model_face_eye_L_0.asset',                              0.7555, 0.9968),
 @('Mesh\model_face_eye_R_0.asset',                              0.7555, 0.9968),
 @('Mesh\model_face_eye_Left_0.asset',                           0.7555, 0.9968),
 @('Mesh\model_face_eye_Right_0.asset',                          0.7555, 0.9968),
 @('Mesh\model_face_eye_Left.asset',                             0.7555, 0.9968),
 @('Mesh\model_face_eye_Right.asset',                            0.7555, 0.9968),
 @('MFG\Characters\Character18\3D\C004\Fbxs\model_face_eye_Left.asset',  0.7555, 0.9968),
 @('MFG\Characters\Character18\3D\C004\Fbxs\model_face_eye_Right.asset', 0.7555, 0.9968))
'--- mouth uv0 (slot 4) ---'
foreach ($t in $mouths) { Remap $t[0] 4 $t[1] $t[2] }
'--- eye brows uv2 (slot 6) ---'
foreach ($t in $brows) { Remap $t[0] 6 $t[1] $t[2] }
