# Find SoundManager methods that acquire _playSemaphore and then await another semaphore-taking facade method (deadlock).
$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'
$f = 'C:\Users\Pure Fox\Documents\GitHub\mfg-decomp\build\unity-project\Assets\Assembly-CSharp\SoundManager.cs'
$lines = Get-Content -LiteralPath $f
$cur = ''; $acq = $false; $takeLine = 0
$results = @()
foreach ($i in 0..($lines.Count - 1)) {
    $ln = $lines[$i]
    if ($ln -match 'public static .*? (\w+)\s*\(') {
        if ($cur) { $results += [pscustomobject]@{ M = $cur; Acq = $acq; Takes = $takes } }
        $cur = $Matches[1]; $acq = $false; $takes = @()
    }
    if ($ln -match '_playSemaphore\.WaitAsync') { if (-not $acq) { $acq = $true; $takeLine = $i + 1 } }
    if ($acq -and $ln -match 'await\s+(Stop\w+|Play\w+)\s*\(') {
        $callee = $Matches[1]
        if ($callee -ne $cur) { $takes += "$($i+1): await $callee" }
    }
}
if ($cur) { $results += [pscustomobject]@{ M = $cur; Acq = $acq; Takes = $takes } }
foreach ($r in $results) {
    if ($r.Acq -and $r.Takes.Count -gt 0) {
        "DEADLOCK-CANDIDATE $($r.M) (acq line $($r.Acq)): " + ($r.Takes -join ' | ')
    }
}
"methods scanned: $($results.Count)"
