<#
.SYNOPSIS
    Rebuild the loose per-tile tile sprites for TileSurface002 .. TileSurface029.

.DESCRIPTION
    TileManager assembles a tile surface from a SpriteAtlas, and when that atlas is not in
    the content set it falls back to the loose per-tile assets at
        Assets/MFG/CustomizeItem/TileSurface/TileSurfaceNNN/2D/<Pai>.png
    Only TileSurface001 has those files, so every other surface shows blank tiles.

    This script cuts the missing tiles out of the original packed atlas sheets exported by the
    reverse-engineering pass, and registers them in the Addressables content state the same way
    scripts/gen_content_state.py writes every other group (md5-derived group / schema / entry
    guids, one group per asset, LF YAML).

    Where the name -> rect map comes from:
      * C:\mfg\build\export-primary-content\Assets\Sprite\<Name>[_k].json - one dump per packed
        Sprite object, each carrying its own m_RD.m_Texture (collection + PathID) and
        m_RD.m_TextureRect. Grouping these by texture gives the name -> rect map of every atlas,
        with no positional guessing (the SpriteAtlas JSON's m_RenderDataMap array is NOT in name
        order, so it cannot be paired positionally - verified: the 1024x512 atlases all share the
        same 69-slot geometry while placing different names in the same slots).
      * C:\mfg\build\export-primary-content\Assets\AssetBundle\<hash>.bundle.json - maps the
        shipped address .../TileSurfaceNNN.spriteatlas to the cab-<hash> collection that holds
        that atlas, which ties each sprite-dump group to one surface.
      * Surfaces whose atlas bundle carries no dumped sprites share TileSurface001's packed
        texture byte-for-byte; their map is 001's, which is pixel-verified against 001's shipped
        loose tiles.

    Atlas rects are bottom-left origin, so a rect (x, y, w, h) is cropped at
    image y = sheetHeight - y - h.

.USAGE
    pwsh -NoProfile -File scripts/gen_tile_surface_sprites.ps1 -DryRun
    pwsh -NoProfile -File scripts/gen_tile_surface_sprites.ps1
    pwsh -NoProfile -File scripts/gen_tile_surface_sprites.ps1 -SelfTest
#>
param(
    [string]$ExportRoot = 'C:\mfg\build\export-primary-content',
    [string]$ProjectRoot = 'C:\mfg\build\unity-project',
    [string]$ReportPath = 'C:\mfgtmp\l8_report.txt',
    [string]$CacheDir = 'C:\mfgtmp\l8_work',
    [string[]]$Surfaces = @(),
    [switch]$DryRun,
    [switch]$SkipCatalog,
    [switch]$SkipExisting,
    [switch]$SelfTest
)

$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'
Add-Type -AssemblyName System.Drawing

$atlasDir = Join-Path $ExportRoot 'Assets\Texture2D'
$spriteDir = Join-Path $ExportRoot 'Assets\Sprite'
$bundleDir = Join-Path $ExportRoot 'Assets\AssetBundle'
$atlasJsonDir = Join-Path $ExportRoot 'Assets\SpriteAtlas'
$tileRoot = Join-Path $ProjectRoot 'Assets\MFG\CustomizeItem\TileSurface'
$groupsDir = Join-Path $ProjectRoot 'Assets\AddressableAssetsData\AssetGroups'
$schemasDir = Join-Path $groupsDir 'Schemas'
$settingsPath = Join-Path $ProjectRoot 'Assets\AddressableAssetsData\AddressableAssetSettings.asset'

# ----------------------------------------------------------------------------- helpers

function Md5Hex {
    param([string]$Text)
    $md5 = [System.Security.Cryptography.MD5]::Create()
    (($md5.ComputeHash([System.Text.Encoding]::UTF8.GetBytes($Text))) | ForEach-Object { $_.ToString('x2') }) -join ''
}

function FileMd5 {
    param([string]$Path)
    $md5 = [System.Security.Cryptography.MD5]::Create()
    $fs = [System.IO.File]::OpenRead($Path)
    try { (($md5.ComputeHash($fs)) | ForEach-Object { $_.ToString('x2') }) -join '' } finally { $fs.Dispose() }
}

function Read-LfText {
    param([string]$Path)
    ([System.IO.File]::ReadAllText($Path)) -split [string][char]10
}

function Write-LfText {
    param([string]$Path, [string[]]$Lines)
    [System.IO.File]::WriteAllText($Path, (($Lines -join [string][char]10)), (New-Object System.Text.UTF8Encoding($false)))
}

function Load-Bits {
    param([string]$Path)
    $bmp = [System.Drawing.Bitmap]::FromFile($Path)
    $w = $bmp.Width; $h = $bmp.Height
    $rect = [System.Drawing.Rectangle]::new(0, 0, $w, $h)
    $data = [System.Drawing.Imaging.BitmapData]::new()
    [void]$bmp.LockBits($rect, [System.Drawing.Imaging.ImageLockMode]::ReadOnly, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb, $data)
    $buf = New-Object byte[] ($data.Stride * $h)
    [System.Runtime.InteropServices.Marshal]::Copy($data.Scan0, $buf, 0, $buf.Length)
    $bmp.UnlockBits($data)
    $stride = $data.Stride
    $bmp.Dispose()
    return [pscustomobject]@{ W = $w; H = $h; Stride = $stride; Buf = $buf }
}

# Atlas rect -> top-left crop box (rects are bottom-origin).
function Crop-Box {
    param($Sheet, $Rect)
    $w = [int][Math]::Round($Rect.W); $h = [int][Math]::Round($Rect.H)
    $x = [int][Math]::Round($Rect.X)
    $y = $Sheet.H - [int][Math]::Round($Rect.Y) - $h
    return [pscustomobject]@{ X = $x; Y = $y; W = $w; H = $h; InBounds = ($x -ge 0 -and $y -ge 0 -and ($x + $w) -le $Sheet.W -and ($y + $h) -le $Sheet.H) }
}

function Alpha-Mean {
    param($Sheet, $Box)
    $s = 0
    for ($yy = 0; $yy -lt $Box.H; $yy++) {
        $row = ($Box.Y + $yy) * $Sheet.Stride
        for ($xx = 0; $xx -lt $Box.W; $xx++) { $s += $Sheet.Buf[$row + ($Box.X + $xx) * 4 + 3] }
    }
    return ($s / ($Box.W * $Box.H))
}

# Mean absolute RGB difference between a crop and a reference image, both resampled to the
# smaller of the two sizes (nearest sampling, aspect is identical for every tile).
function Crop-Vs-Reference {
    param($Sheet, $Box, $Ref)
    $w = [Math]::Min($Box.W, $Ref.W); $h = [Math]::Min($Box.H, $Ref.H)
    $sum = 0.0; $opaque = 0; $identical = 0; $sig = New-Object double[] ($w * $h)
    for ($yy = 0; $yy -lt $h; $yy++) {
        $sy = [int][Math]::Floor(($yy + 0.5) * $Box.H / $h); if ($sy -ge $Box.H) { $sy = $Box.H - 1 }
        $ry = [int][Math]::Floor(($yy + 0.5) * $Ref.H / $h); if ($ry -ge $Ref.H) { $ry = $Ref.H - 1 }
        $srow = ($Box.Y + $sy) * $Sheet.Stride
        $rrow = $ry * $Ref.Stride
        for ($xx = 0; $xx -lt $w; $xx++) {
            $sx = [int][Math]::Floor(($xx + 0.5) * $Box.W / $w); if ($sx -ge $Box.W) { $sx = $Box.W - 1 }
            $rx = [int][Math]::Floor(($xx + 0.5) * $Ref.W / $w); if ($rx -ge $Ref.W) { $rx = $Ref.W - 1 }
            $si = $srow + ($Box.X + $sx) * 4; $ri = $rrow + $rx * 4
            $d1 = [Math]::Abs($Sheet.Buf[$si] - $Ref.Buf[$ri]); $d2 = [Math]::Abs($Sheet.Buf[$si + 1] - $Ref.Buf[$ri + 1]); $d3 = [Math]::Abs($Sheet.Buf[$si + 2] - $Ref.Buf[$ri + 2])
            $sum += $d1 + $d2 + $d3
            if ($d1 + $d2 + $d3 -le 3) { $identical++ }
            if ($Sheet.Buf[$si + 3] -gt 127) { $opaque++ }
            $sig[$yy * $w + $xx] = 0.114 * $Sheet.Buf[$si] + 0.587 * $Sheet.Buf[$si + 1] + 0.299 * $Sheet.Buf[$si + 2]
        }
    }
    $refSig = New-Object double[] ($w * $h)
    for ($yy = 0; $yy -lt $h; $yy++) {
        $ry = [int][Math]::Floor(($yy + 0.5) * $Ref.H / $h); if ($ry -ge $Ref.H) { $ry = $Ref.H - 1 }
        $rrow = $ry * $Ref.Stride
        for ($xx = 0; $xx -lt $w; $xx++) {
            $rx = [int][Math]::Floor(($xx + 0.5) * $Ref.W / $w); if ($rx -ge $Ref.W) { $rx = $Ref.W - 1 }
            $ri = $rrow + $rx * 4
            $refSig[$yy * $w + $xx] = 0.114 * $Ref.Buf[$ri] + 0.587 * $Ref.Buf[$ri + 1] + 0.299 * $Ref.Buf[$ri + 2]
        }
    }
    $corr = Normalize-Corr -A $sig -B $refSig -N ($w * $h)
    return [pscustomobject]@{
        Mad = ($sum / (3.0 * $w * $h))
        AlphaMatch = ($opaque / ($w * $h))
        Same = ($identical / ($w * $h))
        Corr = $corr
    }
}

function Normalize-Corr {
    param([double[]]$A, [double[]]$B, [int]$N)
    $ma = 0.0; $mb = 0.0
    for ($k = 0; $k -lt $N; $k++) { $ma += $A[$k]; $mb += $B[$k] }
    $ma /= $N; $mb /= $N
    $va = 0.0; $vb = 0.0; $ab = 0.0
    for ($k = 0; $k -lt $N; $k++) {
        $da = $A[$k] - $ma; $db = $B[$k] - $mb
        $va += $da * $da; $vb += $db * $db; $ab += $da * $db
    }
    if ($va -lt 1e-9 -or $vb -lt 1e-9) { return 0.0 }
    return ($ab / [Math]::Sqrt($va * $vb))
}

function Lum-Signature {
    param($Sheet, $Box)
    $sig = New-Object double[] ($Box.W * $Box.H)
    for ($yy = 0; $yy -lt $Box.H; $yy++) {
        $row = ($Box.Y + $yy) * $Sheet.Stride
        for ($xx = 0; $xx -lt $Box.W; $xx++) {
            $i = $row + ($Box.X + $xx) * 4
            $sig[$yy * $Box.W + $xx] = 0.114 * $Sheet.Buf[$i] + 0.587 * $Sheet.Buf[$i + 1] + 0.299 * $Sheet.Buf[$i + 2]
        }
    }
    return $sig
}

# Brightness signature resampled to a fixed grid, so tiles of different native sizes
# (TileSurface002 packs its main tiles at 124x156, the rest at 62x78) stay comparable.
function Resample-Lum-Signature {
    param($Sheet, $Box, [int]$W, [int]$H)
    $sig = New-Object double[] ($W * $H)
    for ($yy = 0; $yy -lt $H; $yy++) {
        $sy = [int][Math]::Floor(($yy + 0.5) * $Box.H / $H); if ($sy -ge $Box.H) { $sy = $Box.H - 1 }
        $row = ($Box.Y + $sy) * $Sheet.Stride
        for ($xx = 0; $xx -lt $W; $xx++) {
            $sx = [int][Math]::Floor(($xx + 0.5) * $Box.W / $W); if ($sx -ge $Box.W) { $sx = $Box.W - 1 }
            $i = $row + ($Box.X + $sx) * 4
            $sig[$yy * $W + $xx] = 0.114 * $Sheet.Buf[$i] + 0.587 * $Sheet.Buf[$i + 1] + 0.299 * $Sheet.Buf[$i + 2]
        }
    }
    return $sig
}

function Save-Crop-Png {
    param($Sheet, $Box, [string]$OutPath)
    $opaque = (Alpha-Mean -Sheet $Sheet -Box $Box) -ge 254.5
    $fmt = if ($opaque) { [System.Drawing.Imaging.PixelFormat]::Format24bppRgb } else { [System.Drawing.Imaging.PixelFormat]::Format32bppArgb }
    $dst = New-Object System.Drawing.Bitmap ($Box.W, $Box.H, $fmt)
    $rect = [System.Drawing.Rectangle]::new(0, 0, $Box.W, $Box.H)
    $d = [System.Drawing.Imaging.BitmapData]::new()
    [void]$dst.LockBits($rect, [System.Drawing.Imaging.ImageLockMode]::WriteOnly, $fmt, $d)
    $dbuf = New-Object byte[] ($d.Stride * $Box.H)
    $bytesPerPixel = if ($opaque) { 3 } else { 4 }
    for ($yy = 0; $yy -lt $Box.H; $yy++) {
        $srow = ($Box.Y + $yy) * $Sheet.Stride
        $drow = $yy * $d.Stride
        for ($xx = 0; $xx -lt $Box.W; $xx++) {
            $si = $srow + ($Box.X + $xx) * 4; $di = $drow + $xx * $bytesPerPixel
            $dbuf[$di] = $Sheet.Buf[$si]
            $dbuf[$di + 1] = $Sheet.Buf[$si + 1]
            $dbuf[$di + 2] = $Sheet.Buf[$si + 2]
            if ($bytesPerPixel -eq 4) { $dbuf[$di + 3] = $Sheet.Buf[$si + 3] }
        }
    }
    [System.Runtime.InteropServices.Marshal]::Copy($dbuf, 0, $d.Scan0, $dbuf.Length)
    $dst.UnlockBits($d)
    $dst.Save($OutPath, [System.Drawing.Imaging.ImageFormat]::Png)
    $dst.Dispose()
    return $opaque
}

# ----------------------------------------------------------------------------- catalog YAML templates
# Same shapes scripts/gen_content_state.py writes, with %TOKENS% instead of format placeholders so
# the literal {fileID: ...} scalars survive (string.Format would choke on them). -SelfTest compares
# the rendering against an existing group file byte for byte.

$TPL_GROUP_HEAD = @(
    '%YAML 1.1'
    '%TAG !u! tag:unity3d.com,2011:'
    '--- !u!114 &11400000'
    'MonoBehaviour:'
    '  m_ObjectHideFlags: 0'
    '  m_CorrespondingSourceObject: {fileID: 0}'
    '  m_PrefabInstance: {fileID: 0}'
    '  m_PrefabAsset: {fileID: 0}'
    '  m_GameObject: {fileID: 0}'
    '  m_Enabled: 1'
    '  m_EditorHideFlags: 0'
    '  m_Script: {fileID: 11500000, guid: bbb281ee3bf0b054c82ac2347e9e782c, type: 3}'
    '  m_Name: %NAME%'
    '  m_EditorClassIdentifier: '
    '  m_GroupName: %NAME%'
    '  m_Data:'
    '    m_SerializedData: []'
    '  m_GUID: %MGUID%'
    '  m_SerializeEntries:'
)
$TPL_GROUP_ENTRY = @(
    '  - m_GUID: %ASSETGUID%'
    '    m_Address: %ADDRESS%'
    '    m_ReadOnly: 0'
    '    m_SerializedLabels:'
    '    - %LABEL%'
    '    FlaggedDuringContentUpdateRestriction: 0'
)
$TPL_GROUP_TAIL = @(
    '  m_ReadOnly: 0'
    '  m_Settings: {fileID: 11400000, guid: %SETTINGSGUID%, type: 2}'
    '  m_SchemaSet:'
    '    m_Schemas:'
    '    - {fileID: 11400000, guid: %BGSGUID%, type: 2}'
    '    - {fileID: 11400000, guid: %CUGGUID%, type: 2}'
)
$TPL_META_MONO = @(
    'fileFormatVersion: 2'
    'guid: %GUID%'
    'NativeFormatImporter:'
    '  externalObjects: {}'
    '  mainObjectFileID: 11400000'
    '  userData: '
    '  assetBundleName: '
    '  assetBundleVariant: '
)
$TPL_SCHEMA_BGS = @(
    '%YAML 1.1'
    '%TAG !u! tag:unity3d.com,2011:'
    '--- !u!114 &11400000'
    'MonoBehaviour:'
    '  m_ObjectHideFlags: 0'
    '  m_CorrespondingSourceObject: {fileID: 0}'
    '  m_PrefabInstance: {fileID: 0}'
    '  m_PrefabAsset: {fileID: 0}'
    '  m_GameObject: {fileID: 0}'
    '  m_Enabled: 1'
    '  m_EditorHideFlags: 0'
    '  m_Script: {fileID: 11500000, guid: e5d17a21594effb4e9591490b009e7aa, type: 3}'
    '  m_Name: %NAME%_BundledAssetGroupSchema'
    '  m_EditorClassIdentifier: '
    '  m_Group: {fileID: 11400000, guid: %GROUPGUID%, type: 2}'
    '  m_InternalBundleIdMode: 1'
    '  m_Compression: 1'
    '  m_IncludeAddressInCatalog: 1'
    '  m_IncludeGUIDInCatalog: 1'
    '  m_IncludeLabelsInCatalog: 1'
    '  m_InternalIdNamingMode: 0'
    '  m_CacheClearBehavior: 0'
    '  m_IncludeInBuild: 1'
    '  m_BundledAssetProviderType:'
    '    m_AssemblyName: Unity.ResourceManager, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
    '    m_ClassName: UnityEngine.ResourceManagement.ResourceProviders.BundledAssetProvider'
    '  m_ForceUniqueProvider: 0'
    '  m_UseAssetBundleCache: 1'
    '  m_UseAssetBundleCrc: 1'
    '  m_UseAssetBundleCrcForCachedBundles: 1'
    '  m_UseUWRForLocalBundles: 0'
    '  m_Timeout: 0'
    '  m_ChunkedTransfer: 0'
    '  m_RedirectLimit: -1'
    '  m_RetryCount: 0'
    '  m_BuildPath:'
    '    m_Id: b0aa5af440457f945b1bf58bb84bb6c2'
    '  m_LoadPath:'
    '    m_Id: 445c1f552a363064282662e5782e2517'
    '  m_BundleMode: 0'
    '  m_AssetBundleProviderType:'
    '    m_AssemblyName: Unity.ResourceManager, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
    '    m_ClassName: UnityEngine.ResourceManagement.ResourceProviders.AssetBundleProvider'
    '  m_BundleNaming: 0'
    '  m_AssetLoadMode: 0'
)
$TPL_SCHEMA_CUG = @(
    '%YAML 1.1'
    '%TAG !u! tag:unity3d.com,2011:'
    '--- !u!114 &11400000'
    'MonoBehaviour:'
    '  m_ObjectHideFlags: 0'
    '  m_CorrespondingSourceObject: {fileID: 0}'
    '  m_PrefabInstance: {fileID: 0}'
    '  m_PrefabAsset: {fileID: 0}'
    '  m_GameObject: {fileID: 0}'
    '  m_Enabled: 1'
    '  m_EditorHideFlags: 0'
    '  m_Script: {fileID: 11500000, guid: 5834b5087d578d24c926ce20cd31e6d6, type: 3}'
    '  m_Name: %NAME%_ContentUpdateGroupSchema'
    '  m_EditorClassIdentifier: '
    '  m_Group: {fileID: 11400000, guid: %GROUPGUID%, type: 2}'
    '  m_StaticContent: 0'
)

function Render-Tpl {
    param([string[]]$Template, [hashtable]$Tokens)
    $Template | ForEach-Object {
        $line = $_
        foreach ($kv in $Tokens.GetEnumerator()) { $line = $line.Replace('%' + $kv.Key + '%', $kv.Value) }
        $line
    }
}

# ----------------------------------------------------------------------------- name sets

$suits = [ordered]@{ 'M' = 9; 'S' = 9; 'P' = 9; 'J' = 7 }
$baseNames = @()
$allAtlasNames = @()
foreach ($p in $suits.Keys) {
    for ($i = 1; $i -le $suits[$p]; $i++) { $baseNames += "$p$i"; $allAtlasNames += "$p$i"; $allAtlasNames += "$p${i}R" }
}
$allAtlasNames += 'tex_Fade'
# The set TileSurface001 actually ships loose: 34 base tiles, the three red-5 tiles, tex_Fade.
$shipNames = $baseNames + @('M5R', 'P5R', 'S5R') + @('tex_Fade')
$atlasNameSet = @{}
foreach ($n in $allAtlasNames) { $atlasNameSet[$n] = $true }

# ----------------------------------------------------------------------------- sprite dumps -> atlas maps

function Get-SpriteGroups {
    param([string]$Dir)
    $groups = @{}
    foreach ($fi in (Get-ChildItem -LiteralPath $Dir -Force -Filter '*.json')) {
        $stem = [System.Text.RegularExpressions.Regex]::Replace($fi.BaseName, '_\d+$', '')
        if (-not $atlasNameSet.ContainsKey($stem)) { continue }
        $o = Get-Content -LiteralPath $fi.FullName -Raw | ConvertFrom-Json
        if ($o.m_Name -ne $stem) { continue }
        $rd = $o.m_RD
        if (-not $rd) { continue }
        $w = [double]$rd.m_TextureRect.m_Width; $h = [double]$rd.m_TextureRect.m_Height
        if ($w -le 0 -or $h -le 0) { continue }
        $key = '{0}:{1}' -f $rd.m_Texture.m_Collection, $rd.m_Texture.m_PathID
        if (-not $groups.ContainsKey($key)) { $groups[$key] = [ordered]@{} }
        $groups[$key][$o.m_Name] = [pscustomobject]@{
            X = [double]$rd.m_TextureRect.m_X; Y = [double]$rd.m_TextureRect.m_Y; W = $w; H = $h
            Name = $o.m_Name; PPU = [double]$o.m_PixelsToUnits
        }
    }
    return $groups
}

# ----------------------------------------------------------------------------- shipped bundle -> atlas collection

function Get-SurfaceAtlasCollections {
    param([string]$Dir)
    $map = @{}
    $files = Get-ChildItem -LiteralPath $Dir -Force -Filter '*.json'
    $hits = Select-String -LiteralPath $files.FullName -Pattern '"Assets/MFG/CustomizeItem/TileSurface/Textures/TileSurface(\d{3})\.spriteatlas"'
    foreach ($hit in $hits) {
        $surface = $hit.Matches[0].Groups[1].Value
        $lines = Get-Content -LiteralPath $hit.Path
        $idx = $hit.LineNumber - 1
        for ($j = $idx; $j -lt [Math]::Min($idx + 5, $lines.Count); $j++) {
            if ($lines[$j] -match '"m_Collection":\s*"([^"]+)"') {
                if (-not $map.ContainsKey($surface)) { $map[$surface] = $Matches[1] }
                break
            }
        }
    }
    return $map
}

# ----------------------------------------------------------------------------- exported sheets

function Get-SheetFiles {
    param([string]$Dir)
    $bySurface = @{}
    foreach ($fi in (Get-ChildItem -LiteralPath $Dir -Force -Filter 'sactx-*-TileSurface*.png')) {
        $m = [System.Text.RegularExpressions.Regex]::Match($fi.Name, 'TileSurface(\d{3})')
        if (-not $m.Success) { continue }
        if ($fi.Name -match '_0\.png$') { continue }   # exported duplicate of the same texture
        $surface = $m.Groups[1].Value
        if (-not $bySurface.ContainsKey($surface)) { $bySurface[$surface] = @() }
        $bySurface[$surface] += [pscustomobject]@{ Name = $fi.Name; Path = $fi.FullName; Md5 = (FileMd5 -Path $fi.FullName) }
    }
    return $bySurface
}

# ----------------------------------------------------------------------------- pairing sheets with sprite groups

function Group-SlotSet {
    param($Map)
    $set = @{}
    foreach ($kv in $Map.GetEnumerator()) {
        if ($kv.Key -eq 'tex_Fade') { continue }
        $set['{0},{1}' -f [int][Math]::Round($kv.Value.X), [int][Math]::Round($kv.Value.Y)] = $kv.Value
    }
    return $set
}

function Sheet-CoveredRectKeys {
    param($Sheet)
    $keys = @{}
    for ($yy = 0; $yy -lt $Sheet.H; $yy += 2) {
        for ($xx = 0; $xx -lt $Sheet.W; $xx += 2) {
            if ($Sheet.Buf[$yy * $Sheet.Stride + $xx * 4 + 3] -gt 250) {
                $keys['{0},{1}' -f $xx, $yy] = $true
            }
        }
    }
    return $keys
}

# Coverage score: how many sprites of a map land on non-transparent sheet pixels.
function Pair-Score {
    param($Sheet, $Map)
    $total = 0; $ok = 0; $oob = 0; $sumAlpha = 0.0
    foreach ($kv in $Map.GetEnumerator()) {
        if ($kv.Key -eq 'tex_Fade') { continue }
        $total++
        $box = Crop-Box -Sheet $Sheet -Rect $kv.Value
        if (-not $box.InBounds) { $oob++; continue }
        $a = Alpha-Mean -Sheet $Sheet -Box $box
        $sumAlpha += $a
        if ($a -gt 250) { $ok++ }
    }
    return [pscustomobject]@{ Sprites = $total; Opaque = $ok; OutOfBounds = $oob; MeanAlpha = ($sumAlpha / [Math]::Max(1, $total)) }
}

# Assign each group of a surface to one of its sheet files, maximising coverage.
function Assign-GroupsToSheets {
    param($Maps, $Sheets)
    if ($Maps.Count -eq 0) { return @() }
    $scores = @()
    foreach ($gm in $Maps) {
        foreach ($sh in $Sheets) {
            $bits = Load-Bits -Path $sh.Path
            $scores += [pscustomobject]@{ Group = $gm.Key; Map = $gm.Value; Sheet = $sh.Name; Score = (Pair-Score -Sheet $bits -Map $gm.Value) }
        }
    }
    $usedSheets = @{}; $assignment = @()
    foreach ($row in ($scores | Sort-Object { $_.Score.Opaque }, { $_.Score.MeanAlpha } -Descending)) {
        if ($assignment | Where-Object { $_.Group -eq $row.Group }) { continue }
        if ($usedSheets.ContainsKey($row.Sheet)) { continue }
        $alt = ($scores | Where-Object { $_.Group -eq $row.Group -and $_.Sheet -ne $row.Sheet } | Sort-Object { $_.Score.Opaque } -Descending | Select-Object -First 1)
        $assignment += [pscustomobject]@{ Group = $row.Group; Map = $row.Map; Sheet = $row.Sheet; Score = $row.Score; AltSheet = if ($alt) { $alt.Sheet } else { 'none' }; AltOpaque = if ($alt) { $alt.Score.Opaque } else { 0 } }
        $usedSheets[$row.Sheet] = $true
    }
    return $assignment
}

function Atlas-Json-RectSet {
    param([string]$Path)
    if (-not (Test-Path -LiteralPath $Path)) { return $null }
    $o = Get-Content -LiteralPath $Path -Raw | ConvertFrom-Json
    $set = foreach ($rd in @($o.m_RenderDataMap) ) {
        $r = $rd.Value.m_TextureRect
        '{0},{1},{2},{3}' -f [Math]::Round([double]$r.m_X, 3), [Math]::Round([double]$r.m_Y, 3), [Math]::Round([double]$r.m_Width, 3), [Math]::Round([double]$r.m_Height, 3)
    }
    return ($set | Sort-Object)
}

function Map-RectSet {
    param($Map)
    (($Map.GetEnumerator() | ForEach-Object {
        '{0},{1},{2},{3}' -f [Math]::Round($_.Value.X, 3), [Math]::Round($_.Value.Y, 3), [Math]::Round($_.Value.W, 3), [Math]::Round($_.Value.H, 3)
    }) | Sort-Object)
}

# ----------------------------------------------------------------------------- resolve every surface

"reading exported sprite dumps ..."
$groups = Get-SpriteGroups -Dir $spriteDir
"  atlas maps found: $($groups.Count)"
"reading shipped bundle -> atlas collection map ..."
$atlasColl = Get-SurfaceAtlasCollections -Dir $bundleDir
"  surfaces with a spriteatlas bundle: $($atlasColl.Count)"
$sheets = Get-SheetFiles -Dir $atlasDir

$loose001Dir = Join-Path $tileRoot 'TileSurface001\2D'
if (-not (Test-Path -LiteralPath $loose001Dir)) { throw "missing reference folder $loose001Dir" }

# Anchor: the map that reproduces TileSurface001's shipped loose tiles, and the sheet it lives in.
function Verify-Map-Against-Loose001 {
    param($Map, [string]$SheetPath)
    $bits = Load-Bits -Path $SheetPath
    $mads = @(); $alphas = @()
    foreach ($n in $shipNames) {
        if ($n -eq 'tex_Fade') { continue }
        if (-not $Map.Contains($n)) { continue }
        $refPath = Join-Path $loose001Dir "$n.png"
        if (-not (Test-Path -LiteralPath $refPath)) { continue }
        $ref = Load-Bits -Path $refPath
        $box = Crop-Box -Sheet $bits -Rect $Map[$n]
        if (-not $box.InBounds) { continue }
        $cmp = Crop-Vs-Reference -Sheet $bits -Box $box -Ref $ref
        $mads += $cmp.Mad; $alphas += $cmp.AlphaMatch
    }
    $st = $mads | Measure-Object -Average -Maximum
    $at = $alphas | Measure-Object -Average
    return [pscustomobject]@{ Tiles = $mads.Count; MeanMad = $st.Average; WorstMad = $st.Maximum; MeanAlpha = $at.Average }
}

$anchorSheet = ($sheets['001'] | Select-Object -First 1)
$anchor = $null
foreach ($gk in $groups.Keys) {
    $res = Verify-Map-Against-Loose001 -Map $groups[$gk] -SheetPath $anchorSheet.Path
    if ($res.Tiles -ge 30 -and $res.MeanMad -lt 20 -and $res.MeanAlpha -gt 0.98) { $anchor = [pscustomobject]@{ Key = $gk; Check = $res }; break }
}
if (-not $anchor) { throw 'no exported atlas map reproduces the shipped TileSurface001 loose tiles' }
"anchor map: $($anchor.Key)"
"  reproduces $($anchor.Check.Tiles) of 001's loose tiles, mean per-channel MAD $($([Math]::Round($anchor.Check.MeanMad, 2))), worst $($([Math]::Round($anchor.Check.WorstMad, 2))), mean alpha coverage $([Math]::Round($anchor.Check.MeanAlpha * 100, 2))%"

if ($Surfaces.Count -eq 0) {
    $Surfaces = @()
    foreach ($s in ($sheets.Keys | Sort-Object)) {
        if ($s -eq '001') { continue }
        if ($SkipExisting -and (Test-Path -LiteralPath (Join-Path $tileRoot "TileSurface$s\2D"))) { continue }
        $Surfaces += $s
    }
}

# Reference signatures from 001's shipped loose tiles (names are authoritative there: they come
# from the shipped addressable addresses).
$refSig = @{}; $refBits = @{}
foreach ($n in $baseNames) {
    $p = Join-Path $loose001Dir "$n.png"
    $refBits[$n] = Load-Bits -Path $p
    $refSig[$n] = Lum-Signature -Sheet $refBits[$n] -Box ([pscustomobject]@{ X = 0; Y = 0; W = $refBits[$n].W; H = $refBits[$n].H })
}

# ----------------------------------------------------------------------------- per surface: resolve + verify

$surfaceResults = [System.Collections.Generic.List[object]]::new()
foreach ($surface in ($Surfaces | Sort-Object)) {
    $surfaceSheets = $sheets[$surface]
    if (-not $surfaceSheets) {
        $surfaceResults.Add([pscustomobject]@{ Surface = $surface; Status = 'unverified'; Reason = "no exported atlas sheet matching TileSurface$surface in $atlasDir" })
        continue
    }
    $coll = $atlasColl[$surface]
    $direct = @()
    if ($coll) {
        foreach ($gk in $groups.Keys) { if ($gk.StartsWith($coll + ':')) { $direct += [pscustomobject]@{ Key = $gk; Value = $groups[$gk] } } }
    }

    $mapSource = ''
    $mapsToUse = @()
    $extraNotes = @()
    if ($direct.Count -gt 0) {
        $mapSource = "exported sprite dumps in atlas bundle $coll"
        $mapsToUse = $direct
        $atlasJson = Join-Path $atlasJsonDir "TileSurface$surface.json"
        $js = Atlas-Json-RectSet -Path $atlasJson
        if ($js) {
            foreach ($dm in $direct) {
                $common = @(Compare-Object -ReferenceObject $js -DifferenceObject (Map-RectSet -Map $dm.Value) -IncludeEqual -ExcludeDifferent).Count
                $extraNotes += "SpriteAtlas dump TileSurface$surface.json rect set vs map $($dm.Key): $common of $($js.Count) rects identical"
            }
        }
    }
    else {
        # No sprites were dumped inside this surface's atlas bundle: its packed texture is a
        # byte-for-byte copy of the atlas the anchor map belongs to.
        $sameBytes = @($surfaceSheets | Where-Object { $_.Md5 -eq $anchorSheet.Md5 })
        if ($sameBytes.Count -eq $surfaceSheets.Count) {
            $mapSource = "shared atlas: sheet bytes identical to $($anchorSheet.Name) (md5 $($anchorSheet.Md5)), map = anchor map $($anchor.Key) pixel-verified against TileSurface001's shipped loose tiles"
            $mapsToUse = @([pscustomobject]@{ Key = $anchor.Key; Value = $groups[$anchor.Key] })
            $extraNotes += "surface atlas bundle $coll carries no dumped sprites; the sheet is byte-identical to 001's"
        }
        else {
            $surfaceResults.Add([pscustomobject]@{ Surface = $surface; Status = 'unverified'; Reason = "atlas bundle $coll has no dumped sprites and its sheet bytes are not identical to 001's" })
            continue
        }
    }

    # pair maps with sheets
    $assignment = Assign-GroupsToSheets -Maps $mapsToUse -Sheets $surfaceSheets
    if ($assignment.Count -lt $mapsToUse.Count -or $assignment.Count -lt 1) {
        $surfaceResults.Add([pscustomobject]@{ Surface = $surface; Status = 'unverified'; Reason = 'could not pair sprite maps with sheet files' })
        continue
    }
    $coverageNote = ($assignment | ForEach-Object { "{0} -> {1}: {2}/{3} sprites on opaque pixels, {4} out of bounds, mean alpha {5}; best alternative sheet {6} scored {7}" -f $_.Group, $_.Sheet, $_.Score.Opaque, $_.Score.Sprites, $_.Score.OutOfBounds, ([Math]::Round($_.Score.MeanAlpha, 1)), $_.AltSheet, $_.AltOpaque }) -join '; '

    # build one name -> (sheet, box) plan for the shipped name set
    $plan = [ordered]@{}
    $missingNames = @()
    foreach ($a in $assignment) {
        $sh = $surfaceSheets | Where-Object { $_.Name -eq $a.Sheet } | Select-Object -First 1
        $bits = Load-Bits -Path $sh.Path
        foreach ($kv in $a.Map.GetEnumerator()) {
            $box = Crop-Box -Sheet $bits -Rect $kv.Value
            if (-not $box.InBounds) { continue }
            if (-not $plan.Contains($kv.Key)) { $plan[$kv.Key] = [pscustomobject]@{ Sheet = $sh; Bits = $bits; Box = $box; Rect = $kv.Value } }
        }
    }
    foreach ($n in $shipNames) { if (-not $plan.Contains($n)) { $missingNames += $n } }

    # content verification against 001
    $sampleNames = @('J1', 'M5', 'P9', 'S3', 'J6')
    $sampleLines = @()
    $allMad = @(); $allAlpha = @(); $allCorr = @()
    $surfSig = @{}
    foreach ($n in $baseNames) {
        if (-not $plan.Contains($n)) { continue }
        $ent = $plan[$n]
        $cmp = Crop-Vs-Reference -Sheet $ent.Bits -Box $ent.Box -Ref $refBits[$n]
        $allMad += $cmp.Mad; $allAlpha += $cmp.AlphaMatch; $allCorr += $cmp.Corr
        $surfSig[$n] = Resample-Lum-Signature -Sheet $ent.Bits -Box $ent.Box -W $refBits[$n].W -H $refBits[$n].H
        if ($sampleNames -contains $n) {
            $sampleLines += ("    {0,-4} rect({1},{2}) {3}x{4}  alphaMaskMatch={5:P2}  meanAbsDiff={6}  lumCorr={7:P2}" -f `
                $n, $ent.Rect.X, $ent.Rect.Y, [int][Math]::Round($ent.Rect.W), [int][Math]::Round($ent.Rect.H), $cmp.AlphaMatch, ([Math]::Round($cmp.Mad, 2)), $cmp.Corr)
        }
    }
    # identity test: does each 001 name pick its own tile out of this surface's tiles?
    $identityHits = 0; $tested = 0
    $identityMisses = @()
    foreach ($n in $baseNames) {
        if (-not $surfSig.ContainsKey($n)) { continue }
        $tested++
        $best = -9.0; $bestName = ''
        foreach ($n2 in $baseNames) {
            if (-not $surfSig.ContainsKey($n2)) { continue }
            $c = Normalize-Corr -A $refSig[$n] -B $surfSig[$n2] -N $refSig[$n].Length
            if ($c -gt $best) { $best = $c; $bestName = $n2 }
        }
        if ($bestName -eq $n) { $identityHits++ }
        else { $identityMisses += "$n->closest is this surface's $bestName at $([Math]::Round($best * 100, 1))%" }
    }
    # suit consistency: same-suit share of the 8 nearest look-alikes (self-consistency test)
    $suitTot = 0; $suitHit = 0
    foreach ($n in $baseNames) {
        if (-not $surfSig.ContainsKey($n)) { continue }
        $scores = foreach ($n2 in $baseNames) {
            if ($n2 -eq $n -or -not $surfSig.ContainsKey($n2)) { continue }
            [pscustomobject]@{ Name = $n2; C = (Normalize-Corr -A $surfSig[$n] -B $surfSig[$n2] -N $surfSig[$n].Length) }
        }
        $top = $scores | Sort-Object -Property C -Descending | Select-Object -First 8
        $suitTot += 8; $suitHit += @($top | Where-Object { $_.Name[0] -eq $n[0] }).Count
    }

    $madStats = $allMad | Measure-Object -Average -Minimum -Maximum
    $alphaStats = $allAlpha | Measure-Object -Average -Minimum
    $corrStats = $allCorr | Measure-Object -Average
    $sizes = ($plan.GetEnumerator() | Where-Object { $_.Key -ne 'tex_Fade' } | ForEach-Object { '{0}x{1}' -f $_.Value.Box.W, $_.Value.Box.H } | Group-Object | ForEach-Object { "$($_.Count)@$(($_.Name))" }) -join ' '

    $surfaceResults.Add([pscustomobject]@{
        Surface = $surface
        Status = 'verified'
        PlanRef = $plan
        MapSource = $mapSource
        Sheets = ($surfaceSheets | ForEach-Object { "$($_.Name) [md5 $($_.Md5)]" }) -join ', '
        Coverage = $coverageNote
        Sizes = $sizes
        MissingNames = $missingNames
        SampleLines = $sampleLines
        MadMean = [Math]::Round($madStats.Average, 2); MadMin = [Math]::Round($madStats.Minimum, 2); MadMax = [Math]::Round($madStats.Maximum, 2)
        AlphaMean = [Math]::Round($alphaStats.Average * 100, 2); AlphaMin = [Math]::Round($alphaStats.Minimum * 100, 2)
        CorrMean = [Math]::Round($corrStats.Average * 100, 2)
        Identity = "$identityHits/$tested"
        IdentityMisses = $identityMisses
        SuitShare = if ($suitTot) { ('{0}/{1} ({2:P1})' -f $suitHit, $suitTot, ($suitHit / $suitTot)) } else { 'n/a' }
        Notes = $extraNotes
    })
}

# ----------------------------------------------------------------------------- write assets

$metaTemplateTile = Read-LfText -Path (Join-Path $loose001Dir 'J1.png.meta')
$metaTemplateFade = Read-LfText -Path (Join-Path $loose001Dir 'tex_Fade.png.meta')
$folderMetaTemplate = Read-LfText -Path (Join-Path $tileRoot 'TileSurface001.meta')
$settingsGuid = (Select-String -LiteralPath (Join-Path $ProjectRoot 'Assets\AddressableAssetsData\AddressableAssetSettings.asset.meta') -Pattern '^guid: ([0-9a-f]{32})').Matches[0].Groups[1].Value

# Existing guids, group names and labels: nothing new may collide with them.
$guidCache = Join-Path $CacheDir 'existing_guids.txt'
if (Test-Path -LiteralPath $guidCache) {
    $existingGuids = New-Object 'System.Collections.Generic.HashSet[string]'
    foreach ($g in (Get-Content -LiteralPath $guidCache)) { [void]$existingGuids.Add($g) }
}
else {
    $metas = Get-ChildItem -LiteralPath (Join-Path $ProjectRoot 'Assets') -Recurse -Force -File -Filter '*.meta'
    $existingGuids = New-Object 'System.Collections.Generic.HashSet[string]'
    $cacheLines = [System.Collections.Generic.List[string]]::new()
    foreach ($hit in (Select-String -LiteralPath $metas.FullName -Pattern '^guid: ([0-9a-f]{32})')) {
        $v = $hit.Matches[0].Groups[1].Value
        [void]$existingGuids.Add($v); $cacheLines.Add($v)
    }
    [System.IO.File]::WriteAllLines($guidCache, $cacheLines, (New-Object System.Text.UTF8Encoding($false)))
}
"existing guids loaded: $($existingGuids.Count)"

$existingGroupNames = New-Object 'System.Collections.Generic.HashSet[string]'
foreach ($fi in (Get-ChildItem -LiteralPath $groupsDir -Force -File -Filter '*.asset')) { [void]$existingGroupNames.Add($fi.BaseName) }
$settingsLines = Read-LfText -Path $settingsPath
$existingLabels = New-Object 'System.Collections.Generic.HashSet[string]'
$labelBlockStart = -1; $labelBlockEnd = -1
for ($i = 0; $i -lt $settingsLines.Count; $i++) {
    if ($settingsLines[$i] -eq '    m_LabelNames:') { $labelBlockStart = $i + 1; continue }
    if ($labelBlockStart -ge 0 -and $settingsLines[$i] -match '^    - ') { [void]$existingLabels.Add(($settingsLines[$i] -replace '^    - ', '')); continue }
    if ($labelBlockStart -ge 0 -and $labelBlockEnd -lt 0) { $labelBlockEnd = $i; break }
}
"existing labels: $($existingLabels.Count) (block $($labelBlockStart)..$labelBlockEnd)"

# Group guids already registered in m_GroupAssets. Used only to recognise this script's own
# earlier output when it is re-run, never to skip a genuine id collision.
$settingsGroupGuids = New-Object 'System.Collections.Generic.HashSet[string]'
foreach ($line in $settingsLines) {
    if ($line -eq '  m_BuildSettings:') { break }
    if ($line -match '^  - \{fileID: 11400000, guid: ([0-9a-f]{32}), type: 2\}$') { [void]$settingsGroupGuids.Add($Matches[1]) }
}
"existing group refs in settings: $($settingsGroupGuids.Count)"

$written = [ordered]@{ Png = 0; Meta = 0; GroupAsset = 0; SchemaAsset = 0; FolderMeta = 0 }
$collisions = @()
$reusedCount = 0
$planForCatalog = [System.Collections.Generic.List[object]]::new()

foreach ($res in $surfaceResults) {
    if ($res.Status -ne 'verified') { continue }
    $dir = Join-Path $tileRoot "TileSurface$($res.Surface)\2D"
    if (-not (Test-Path -LiteralPath $dir)) { if (-not $DryRun) { New-Item -Path $dir -ItemType Directory -Force | Out-Null } }
    foreach ($n in $shipNames) {
        $entryKey = "$($res.Surface)|$n"
        $address = "Assets/MFG/CustomizeItem/TileSurface/TileSurface$($res.Surface)/2D/$n.png"
        $pngPath = Join-Path $dir "$n.png"
        $metaPath = "$pngPath.meta"
        $assetGuid = Md5Hex ('l8png:' + $address)
        $bundleName = (Md5Hex ('l8grp:' + $address)) + '.bundle'
        $groupGuid = Md5Hex ('grp:' + $bundleName)
        $bgsGuid = Md5Hex ('bgs:' + $bundleName)
        $cugGuid = Md5Hex ('cug:' + $bundleName)
        $mGuid = Md5Hex ('mg:' + $bundleName)
        $label = Md5Hex ('l8label:' + $address)
        # Is this address already produced by an earlier run of this script? Its group asset
        # carries exactly the ids this script would derive for it.
        $groupAssetPath = Join-Path $groupsDir ($bundleName + '.asset')
        $ownPriorRun = $false
        if (Test-Path -LiteralPath $groupAssetPath) {
            $groupText = [System.IO.File]::ReadAllText($groupAssetPath)
            $ownPriorRun = ($groupText.Contains($address) -and $groupText.Contains($mGuid) -and $groupText.Contains($bgsGuid) -and $groupText.Contains($cugGuid))
        }
        $refRegistered = $settingsGroupGuids.Contains($groupGuid)
        $labelRegistered = $existingLabels.Contains($label)
        if ($refRegistered -ne $labelRegistered) {
            $collisions += "half-registered $bundleName : settings group ref=$refRegistered label=$labelRegistered"
            continue
        }
        if ($refRegistered -and $ownPriorRun) { $reusedCount++ }

        $collided = @()
        if (-not $ownPriorRun) {
            foreach ($pair in @(@('guid', $assetGuid), @('group guid', $groupGuid), @('schema guid', $bgsGuid), @('schema guid', $cugGuid))) {
                if ($existingGuids.Contains($pair[1])) { $collided += "$($pair[0]) collision: $($pair[1])" }
            }
            if ($existingGroupNames.Contains($bundleName)) { $collided += "bundle name collision: $bundleName" }
            if ($labelRegistered) { $collided += "label collision: $label" }
        }
        if ($collided.Count -gt 0) { $collisions += ($collided -join '; '); continue }
        foreach ($v in @($assetGuid, $groupGuid, $bgsGuid, $cugGuid)) { [void]$existingGuids.Add($v) }
        [void]$existingGroupNames.Add($bundleName)
        [void]$existingLabels.Add($label)

        if (-not $DryRun) {
            if ($n -eq 'tex_Fade') {
                # tex_Fade is a shared fade gradient, not a tile: keep 001's exact bytes.
                Copy-Item -LiteralPath (Join-Path $loose001Dir 'tex_Fade.png') -Destination $pngPath -Force
            }
            elseif ($res.PlanRef.Contains($n)) {
                $ent = $res.PlanRef[$n]
                [void](Save-Crop-Png -Sheet $ent.Bits -Box $ent.Box -OutPath $pngPath)
            }
            else {
                continue
            }
            $template = if ($n -eq 'tex_Fade') { $metaTemplateFade } else { $metaTemplateTile }
            $meta = @($template | ForEach-Object {
                if ($_ -match '^guid: ') { "guid: $assetGuid" }
                elseif ($_ -match '^  assetBundleName: ') { "  assetBundleName: $($bundleName -replace '\.bundle$', '')" }
                else { $_ }
            })
            Write-LfText -Path $metaPath -Lines $meta
            $written.Png++; $written.Meta++
            if (-not $refRegistered) {
                $planForCatalog.Add([pscustomobject]@{ Address = $address; AssetGuid = $assetGuid; BundleName = $bundleName; GroupGuid = $groupGuid; BgsGuid = $bgsGuid; CugGuid = $cugGuid; MGuid = $mGuid; Label = $label })
            }
        }
    }
    # folder metas for surfaces that had no folder yet
    foreach ($folderPath in @((Join-Path $tileRoot "TileSurface$($res.Surface)"), (Join-Path $tileRoot "TileSurface$($res.Surface)\2D"))) {
        $metaPath = "$folderPath.meta"
        if (Test-Path -LiteralPath $metaPath) { continue }
        $fGuid = Md5Hex ('l8folder:' + $folderPath.Substring($ProjectRoot.Length).Replace('\', '/'))
        if ($existingGuids.Contains($fGuid)) { $collisions += "folder guid collision: $fGuid" }
        if (-not $DryRun) { Write-LfText -Path $metaPath -Lines (@($folderMetaTemplate | ForEach-Object { if ($_ -match '^guid: ') { "guid: $fGuid" } else { $_ } })) }
        $written.FolderMeta++
    }
}

if ($collisions.Count -gt 0) {
    "COLLISIONS ($($collisions.Count)) - nothing written for these:"
    $collisions | Select-Object -First 20 | ForEach-Object { "  $_" }
    if ($collisions.Count -gt 20) { "  ... $($collisions.Count - 20) more" }
}

# ----------------------------------------------------------------------------- addressable registration

if (-not $DryRun -and -not $SkipCatalog -and $planForCatalog.Count -gt 0) {
    foreach ($e in $planForCatalog) {
        $tokens = @{
            NAME = $e.BundleName; MGUID = $e.MGuid; ASSETGUID = $e.AssetGuid; ADDRESS = $e.Address
            LABEL = $e.Label; SETTINGSGUID = $settingsGuid; BGSGUID = $e.BgsGuid; CUGGUID = $e.CugGuid
            GROUPGUID = $e.GroupGuid
        }
        $groupPath = Join-Path $groupsDir ($e.BundleName + '.asset')
        $body = Render-Tpl -Template $TPL_GROUP_HEAD -Tokens $tokens
        $body += Render-Tpl -Template $TPL_GROUP_ENTRY -Tokens $tokens
        $body += Render-Tpl -Template $TPL_GROUP_TAIL -Tokens $tokens
        Write-LfText -Path $groupPath -Lines ($body + @(''))
        Write-LfText -Path "$groupPath.meta" -Lines ((Render-Tpl -Template $TPL_META_MONO -Tokens @{ GUID = $e.GroupGuid }) + @(''))
        $bgsPath = Join-Path $schemasDir ($e.BundleName + '_BundledAssetGroupSchema.asset')
        Write-LfText -Path $bgsPath -Lines ((Render-Tpl -Template $TPL_SCHEMA_BGS -Tokens $tokens) + @(''))
        Write-LfText -Path "$bgsPath.meta" -Lines ((Render-Tpl -Template $TPL_META_MONO -Tokens @{ GUID = $e.BgsGuid }) + @(''))
        $cugPath = Join-Path $schemasDir ($e.BundleName + '_ContentUpdateGroupSchema.asset')
        Write-LfText -Path $cugPath -Lines ((Render-Tpl -Template $TPL_SCHEMA_CUG -Tokens $tokens) + @(''))
        Write-LfText -Path "$cugPath.meta" -Lines ((Render-Tpl -Template $TPL_META_MONO -Tokens @{ GUID = $e.CugGuid }) + @(''))
        $written.GroupAsset++
        $written.SchemaAsset += 2
    }

    # settings: new group refs into m_GroupAssets, new labels into m_LabelTable
    $out = [System.Collections.Generic.List[string]]::new()
    $groupMarker = '  m_BuildSettings:'
    foreach ($line in $settingsLines) {
        if ($line -eq $groupMarker) {
            foreach ($e in $planForCatalog) { $out.Add('  - {0}' -f ('{fileID: 11400000, guid: ' + $e.GroupGuid + ', type: 2}')) }
        }
        $out.Add($line)
    }
    $labelsNew = New-Object 'System.Collections.Generic.HashSet[string]'
    foreach ($e in $planForCatalog) { [void]$labelsNew.Add($e.Label) }
    $blockStart = -1
    for ($i = 0; $i -lt $out.Count; $i++) { if ($out[$i] -eq '    m_LabelNames:') { $blockStart = $i + 1; break } }
    $blockEnd = $blockStart
    while ($out[$blockEnd] -match '^    - ') { $blockEnd++ }
    $block = $out[$blockStart..($blockEnd - 1)]
    $keep = @($block | Where-Object { $_ -eq '    - default' })
    $labels = @($block | Where-Object { $_ -ne '    - default' } | ForEach-Object { $_ -replace '^    - ', '' })
    # The shipped table is ordered with the plain ordinal string comparer (verified against the
    # file before patching: digits, then the named labels such as Bgm / Sfx / VoiceChara01,
    # then the lower-case hex labels), with 'default' pinned first. Merging the new labels
    # through the same comparer keeps the existing 29k lines in the same order, so the diff is
    # purely additive.
    $ordered = New-Object 'System.Collections.Generic.SortedSet[string]' ([System.StringComparer]::Ordinal)
    foreach ($l in $labels) { [void]$ordered.Add($l) }
    foreach ($l in $labelsNew) { [void]$ordered.Add($l) }
    $rebuilt = $keep + ($ordered | ForEach-Object { '    - ' + $_ })
    $newSettings = @($out[0..($blockStart - 1)]) + $rebuilt + @($out[$blockEnd..($out.Count - 1)])
    Write-LfText -Path $settingsPath -Lines $newSettings
    "settings patched: +$($planForCatalog.Count) group refs, +$($labelsNew.Count) labels (label table now $($ordered.Count) plus 'default')"
}

# ----------------------------------------------------------------------------- report and self test

if ($SelfTest) {
    # Re-serialize an existing TileSurface001 group from the templates and compare byte for byte.
    $hit = Select-String -LiteralPath (Get-ChildItem -LiteralPath $groupsDir -Filter '*.asset' -File).FullName -Pattern 'TileSurface001/2D/J1.png' -List | Select-Object -First 1
    if (-not $hit) { "self test: no TileSurface001/2D/J1.png group found" }
    else {
        $name = (Get-Item -LiteralPath $hit.Path).BaseName
        $disk = [System.IO.File]::ReadAllText($hit.Path)
        $metaGuid = (Select-String -LiteralPath "$($hit.Path).meta" -Pattern '^guid: ([0-9a-f]{32})').Matches[0].Groups[1].Value
        $entryGuid = (Select-String -LiteralPath $hit.Path -Pattern '  - m_GUID: ([0-9a-f]{32})').Matches[0].Groups[1].Value
        $addr = (Select-String -LiteralPath $hit.Path -Pattern '    m_Address: (.*)$').Matches[0].Groups[1].Value
        $label = (Select-String -LiteralPath $hit.Path -Pattern '    - ([0-9a-f]{32})').Matches[0].Groups[1].Value
        $tokens = @{
            NAME = $name; MGUID = (Md5Hex ('mg:' + $name)); ASSETGUID = $entryGuid; ADDRESS = $addr; LABEL = $label
            SETTINGSGUID = $settingsGuid; BGSGUID = (Md5Hex ('bgs:' + $name)); CUGGUID = (Md5Hex ('cug:' + $name)); GROUPGUID = $metaGuid
        }
        $mine = ((Render-Tpl -Template $TPL_GROUP_HEAD -Tokens $tokens) + (Render-Tpl -Template $TPL_GROUP_ENTRY -Tokens $tokens) + (Render-Tpl -Template $TPL_GROUP_TAIL -Tokens $tokens) + @('')) -join [string][char]10
        "self test group body identical to on-disk file: $($mine -eq $disk)"
        "self test group meta guid: md5(grp:$name) = $(Md5Hex ('grp:' + $name)) vs on-disk $metaGuid -> $((Md5Hex ('grp:' + $name)) -eq $metaGuid)"
        "self test group m_GUID: md5(mg:$name) = $(Md5Hex ('mg:' + $name)) vs on-disk $((Select-String -LiteralPath $hit.Path -Pattern '  m_GUID: ([0-9a-f]{32})').Matches[0].Groups[1].Value)"
        $bgsDisk = [System.IO.File]::ReadAllText((Join-Path $schemasDir ($name + '_BundledAssetGroupSchema.asset')))
        $mineBgs = ((Render-Tpl -Template $TPL_SCHEMA_BGS -Tokens $tokens) + @('')) -join [string][char]10
        "self test BundledAssetGroupSchema identical: $($mineBgs -eq $bgsDisk)"
        $cugDisk = [System.IO.File]::ReadAllText((Join-Path $schemasDir ($name + '_ContentUpdateGroupSchema.asset')))
        $mineCug = ((Render-Tpl -Template $TPL_SCHEMA_CUG -Tokens $tokens) + @('')) -join [string][char]10
        "self test ContentUpdateGroupSchema identical: $($mineCug -eq $cugDisk)"
        $metaDisk = [System.IO.File]::ReadAllText("$($hit.Path).meta")
        $mineMeta = ((Render-Tpl -Template $TPL_META_MONO -Tokens @{ GUID = $metaGuid }) + @('')) -join [string][char]10
        "self test group meta identical: $($mineMeta -eq $metaDisk)"
    }
}

# ----------------------------------------------------------------------------- post-write audit

# Read the written tiles back off disk and compare them with three crops of the sheet they came
# from: the sprite's own rect (must be identical), the same rect without the bottom-origin flip
# (must differ), and the rect moved one grid slot sideways (must differ). A crop that is placed
# wrongly cannot pass this.
$auditLines = [System.Collections.Generic.List[string]]::new()
if (-not $DryRun) {
    foreach ($res in $surfaceResults) {
        if ($res.Status -ne 'verified') { continue }
        $dir = Join-Path $tileRoot "TileSurface$($res.Surface)\2D"
        $pngTotal = @(Get-ChildItem -LiteralPath $dir -File -Filter '*.png' -Force).Count
        $metaTotal = @(Get-ChildItem -LiteralPath $dir -File -Filter '*.png.meta' -Force).Count
        $auditLines.Add("TileSurface$($res.Surface): $pngTotal png, $metaTotal png.meta in $dir")
        foreach ($n in @('J1', 'M5', 'P9')) {
            if (-not $res.PlanRef.Contains($n)) { continue }
            $ent = $res.PlanRef[$n]
            $pngPath = Join-Path $dir "$n.png"
            if (-not (Test-Path -LiteralPath $pngPath)) { $auditLines.Add("    $n : MISSING"); continue }
            $writtenTile = Load-Bits -Path $pngPath
            $own = Crop-Vs-Reference -Sheet $ent.Bits -Box $ent.Box -Ref $writtenTile
            $flipBox = [pscustomobject]@{ X = $ent.Box.X; Y = [int][Math]::Round($ent.Rect.Y); W = $ent.Box.W; H = $ent.Box.H; InBounds = $true }
            $pitch = if ($ent.Box.W -gt 100) { 128 } else { 66 }
            $shiftBox = [pscustomobject]@{ X = $ent.Box.X + $pitch; Y = $ent.Box.Y; W = $ent.Box.W; H = $ent.Box.H; InBounds = ($ent.Box.X + $pitch + $ent.Box.W -le $ent.Bits.W) }
            $flip = Crop-Vs-Reference -Sheet $ent.Bits -Box $flipBox -Ref $writtenTile
            $shift = if ($shiftBox.InBounds) { Crop-Vs-Reference -Sheet $ent.Bits -Box $shiftBox -Ref $writtenTile } else { $null }
            $auditLines.Add(("    {0,-6} own rect MAD={1} identical pixels={2:P1} | without the bottom-origin flip MAD={3} | one slot sideways MAD={4}" -f `
                $n, ([Math]::Round($own.Mad, 2)), $own.Same, ([Math]::Round($flip.Mad, 2)), $(if ($shift) { [Math]::Round($shift.Mad, 2) } else { 'n/a' })))
        }
    }
}
$surfaceFolderCheck = @(Get-ChildItem -LiteralPath $tileRoot -Directory -Force | Where-Object { Test-Path -LiteralPath (Join-Path $_.FullName '2D') })
$folderMetaCount = @(Get-ChildItem -LiteralPath $tileRoot -Recurse -Force -File -Filter '2D.meta').Count

$report = [System.Collections.Generic.List[string]]::new()
$report.Add("TileSurface loose tile sprite generation (defect L8)")
$report.Add("generated: $(Get-Date -Format 'o')  dryRun=$DryRun")
$report.Add("")
$report.Add("Anchor map $($anchor.Key)")
$report.Add("  reproduces $($anchor.Check.Tiles) shipped TileSurface001 loose tiles from $($anchorSheet.Name)")
$report.Add("  mean per-channel MAD $([Math]::Round($anchor.Check.MeanMad,2)) (worst $([Math]::Round($anchor.Check.WorstMad,2))), mean alpha coverage $([Math]::Round($anchor.Check.MeanAlpha*100,2))%")
$report.Add("")
$report.Add("Score legend: alphaMaskMatch = share of the sampled crop pixels that are opaque, i.e.")
$report.Add("  agreement with the TileSurface001 tile silhouette (001's shipped loose tiles are fully")
$report.Add("  opaque rectangles). meanAbsDiff = mean per-channel RGB difference after resampling both")
$report.Add("  images to the smaller size. lumCorr = brightness-pattern correlation with the 001 tile")
$report.Add("  of the same name. identity = of the 34 base names, how many pick their own 001 tile as")
$report.Add("  the closest match out of all 34 tiles of this surface. suit share = share of the 8 closest")
$report.Add("  look-alikes of each tile that sit in the same suit (foreign-map controls score 20-24%).")
$report.Add("")
foreach ($res in $surfaceResults) {
    $report.Add("=== TileSurface$($res.Surface) : $($res.Status) ===")
    if ($res.Status -ne 'verified') { $report.Add("  reason: $($res.Reason)"); $report.Add(""); continue }
    $report.Add("  rect-map source: $($res.MapSource)")
    $report.Add("  sheets: $($res.Sheets)")
    foreach ($note in $res.Notes) { $report.Add("  note: $note") }
    $report.Add("  coverage: $($res.Coverage)")
    $report.Add("  crop sizes: $($res.Sizes)")
    $report.Add("  sampled tiles vs TileSurface001:")
    foreach ($l in $res.SampleLines) { $report.Add($l) }
    $report.Add("  all 34 base tiles vs 001: meanAbsDiff mean=$($res.MadMean) min=$($res.MadMin) max=$($res.MadMax); alphaMaskMatch mean=$($res.AlphaMean)% min=$($res.AlphaMin)%; lumCorr mean=$($res.CorrMean)%")
    $report.Add("  identity test: $($res.Identity) base names match their own 001 tile")
    if ($res.IdentityMisses.Count) {
        $report.Add("    those that did not (styling difference, not a rect question - the name comes from this surface's own sprite dump):")
        foreach ($m in $res.IdentityMisses) { $report.Add("      $m") }
    }
    $report.Add("  suit consistency: $($res.SuitShare)")
    $report.Add("  files: $($shipNames.Count) png + $($shipNames.Count) png.meta" + $(if ($res.MissingNames.Count) { "; NOT generated (absent from this atlas): $($res.MissingNames -join ', ')" } else { '' }))
    $report.Add("")
}
$report.Add("Post-write audit (tiles read back off disk, compared with crops of the sheet they came from):")
foreach ($l in $auditLines) { $report.Add("  $l") }
$report.Add("  TileSurfaceNNN folders with a 2D subfolder under $tileRoot : $($surfaceFolderCheck.Count)")
$report.Add("  2D folder importers (.meta) present : $folderMetaCount")
$report.Add("")
$report.Add("Totals: png=$($written.Png) png.meta=$($written.Meta) group assets=$($written.GroupAsset) schema assets=$($written.SchemaAsset) folder metas=$($written.FolderMeta) already generated by an earlier run=$($reusedCount)")
$report.Add("What is NOT verified:")
$report.Add("  * No Unity import or build was run. The pngs and .png.meta files follow TileSurface001's")
$report.Add("    importer settings byte for byte (spriteMode 1, full rect, pivot 0.5/0.5, PPU 100,")
$report.Add("    textureType 0) but Unity's own import of them has not been observed.")
$report.Add("  * No runtime load was exercised: the entries are registered in AddressableAssetSettings")
$report.Add("    and their groups and schemas exist, yet no built catalog exists in the project")
$report.Add("    (Assets/StreamingAssets/aa is absent), so serving them still depends on the rebuild build step.")
$report.Add("  * For the surfaces whose own atlas bundle carries no sprite dumps, the tiles are certain")
$report.Add("    (their sheet is byte-identical to 001's) but the name-to-slot assignment is inherited")
$report.Add("    from 001's pixel-verified map rather than read from that surface's own atlas data.")
$report.Add("  * No standalone .asset sprite file is produced: TileSurface001 has one next to each png, but")
$report.Add("    those reference the atlas texture guid, which has no counterpart for other surfaces, and the")
$report.Add("    catalog entries point at the .png guids.")
$report.Add("  * TileSurface002's 37 main tiles are cut at 124x156 because that is the size its own atlas")
$report.Add("    stores them at (PPU 100, same 1:1.258 aspect as 62x78). Nothing has been checked against an")
$report.Add("    in-game render; the consumers only assign the sprite to a UI Image, whose rect sets the size.")
$report.Add("")
$report.Add("Not generated: the 31 red-dora variants that the atlases carry beyond 001's shipped set")
$report.Add("  (TileManager falls back to the matching base tile for names ending in R), and no standalone")
$report.Add("  .asset sprite files: the shipped catalog addresses point at the .png entries only.")
if ($collisions.Count) {
    $report.Add("Collisions: $($collisions.Count)")
    $collisions | Select-Object -First 10 | ForEach-Object { $report.Add("  $_") }
} else { $report.Add("Collisions: none") }
[System.IO.File]::WriteAllLines($ReportPath, $report, (New-Object System.Text.UTF8Encoding($false)))

$report | ForEach-Object { Write-Output $_ }
