# Switch BGM importer loadType from CompressedInMemory (2) to DecompressOnLoad (0)
# Goal: make BGM play as decoded AudioBuffers on WebGL so BufferSourceNode.loop loops seamlessly.
# Default mode = report only. -Apply writes; -Restore reverts (2).
# Scope: Assets/MFG/Sound/BGM/*.ogg.meta (loop-critical BGM family).
param(
    [switch]$Apply,
    [switch]$Restore
)
$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'
$bgmDir = 'C:\Users\Pure Fox\Documents\GitHub\mfg-decomp\build\unity-project\Assets\MFG\Sound\BGM'
$metas = Get-ChildItem $bgmDir -Filter '*.ogg.meta' -File
"metas: $($metas.Count)"
$changed = 0
foreach ($m in $metas) {
    $txt = Get-Content -LiteralPath $m.FullName -Raw
    # only the defaultSettings block (first loadType line in the file)
    if ($Restore) {
        $new = [regex]::Replace($txt, '(defaultSettings:\r?\n(?:\s+\w+.*\r?\n)*?\s+loadType: )0', '${1}2')
    } else {
        $new = [regex]::Replace($txt, '(defaultSettings:\r?\n(?:\s+\w+.*\r?\n)*?\s+loadType: )2', '${1}0')
    }
    if ($new -ne $txt) {
        $changed++
        if ($Apply -or $Restore) { Set-Content -LiteralPath $m.FullName -Value $new -Encoding utf8 -NoNewline }
    }
}
"loadType-changed: $changed  apply=$([bool]$Apply) restore=$([bool]$Restore)"
