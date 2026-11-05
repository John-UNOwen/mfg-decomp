# lane_probe_run.ps1 - one measured frame run on the private probe lane.
# Boots a guest session in headless Edge, skips the story, lands on FirstCharaSelect with a chosen
# character, and captures frames. Measurement knobs go to the player through the url query, which the
# assembled loader splices into argv (params separated by &, "name=value" split into two argv tokens).
#   .\scripts\lane_probe_run.ps1 -Chara 7                      Character 7 select frames
#   .\scripts\lane_probe_run.ps1 -Chara 4 -Mouth 1,1           Character 4 with mouth ramp 1 / tint 1
#   .\scripts\lane_probe_run.ps1 -Chara 4 -Hide model_face_mouth   frame pair that shows card-owned pixels
#   .\scripts\lane_probe_run.ps1 -Chara 4 -Decide              let the flow decide after the pick
param(
    [int]$Chara = 1,
    [string]$Mouth = '',            # "<ramp>,<tint>" written onto Mt_mouth (see Mouth.shader)
    [string]$Overlay = '',          # 0 or 1: write _MouthOverlayMode on Mt_mouth
    [string]$Hide = '',             # renderer-name substrings to disable
    [switch]$Decide,
    [switch]$NoMatDump,
    [string]$Label = '',
    [int]$Frames = 6,
    [int]$TimeoutSec = 420
)
$ProgressPreference = 'SilentlyContinue'
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path)
$page = 'http://127.0.0.1:8095/aog/index.html'
$out = 'C:\mfgtmp\lane_frames'

$query = '-MFGDBG&-MFGMATDUMP&-MFGCHARA=' + $Chara
if ($Mouth -ne '') { $query += '&-MFGMOUTH=' + $Mouth }
if ($Overlay -ne '') { $query += '&-MFGOVERLAY=' + $Overlay }
if ($Hide -ne '') { $query += '&-MFGHIDE=' + $Hide }
if ($Decide) { $query += '&-MFGCHARADECIDE' }
$url = $page + '?' + $query

if ($Label -eq '') {
    $Label = 'c' + $Chara
    if ($Mouth -ne '') { $Label += '_m' + $Mouth.Replace(',', '') }
    if ($Overlay -ne '') { $Label += '_o' + $Overlay }
    if ($Hide -ne '') { $Label += '_hid' }
    if ($Decide) { $Label += '_dec' }
}
if ($NoMatDump) {
    # -MFGMATDUMP is always sent unless asked off: the material dump line is what proves which face
    # map variant and which mouth index the player is actually rendering with.
    $url = $url.Replace('&-MFGMATDUMP', '')
}

"probe run label=$Label chara=$Chara mouth='$Mouth' hide='$Hide' decide=$Decide"
"url=$url"
$env:TARGET_SCENE = 'FirstCharaSelect'
$env:FRAME_COUNT = "$Frames"
# Measured on the live lane (plumb_probe1, 2026-11-04): FirstCharaSelect appears at ~93 s and the flow walks on
# to Guide at ~105 s, with an INFORMATION modal covering the screen by frame 4. Settle short so the picked
# character is captured inside that window.
$env:SETTLE_MS = '2000'
$env:OUT_DIR = $out
$env:LABEL = $Label
# Story skip-all button, then the confirm dialog's skip button (canvas fractions, measured on a
# StoryPlay frame: skip-all circular button ~(1186,598), confirm ~(518,383) at 1280x720).
$env:CLICK_PLAN = 'StoryPlay:0.927,0.831;StoryPlay:0.405,0.532'
$env:POST_CLICK_ROUNDS = '2'
$outFile = 'C:\mfgtmp\lane_frames\' + $Label + '_probe.out.txt'
& node (Join-Path $root 'scripts\lane_frame_probe.cjs') $url ($TimeoutSec * 1000) *> $outFile
if ($LASTEXITCODE -ne 0) { Get-Content $outFile -Tail 25; throw "frame probe exited $LASTEXITCODE" }
# Never pipe the harness stdout through Select-Object -First: it closes the pipeline and kills the run.
Get-Content $outFile -Tail 12

$log = Join-Path $out ($Label + '_page.json')
$logTxt = Join-Path $out ($Label + '_console.log.txt')
foreach ($pattern in 'WEBGLARGS', 'AUTOSERVE installed', 'chara pick', 'MATDUMP', 'Standing CharaManager: Loaded ItemID') {
    $hits = Select-String -LiteralPath $logTxt -Pattern $pattern -EA SilentlyContinue | Select-Object -First 6
    if ($hits) { foreach ($h in $hits) { ($h.Line.Length -gt 260 ? $h.Line.Substring(0,260) : $h.Line) } } else { "no console line for '$pattern'" }
}
