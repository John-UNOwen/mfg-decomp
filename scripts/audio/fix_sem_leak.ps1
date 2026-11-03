# Leak-proof _playSemaphore pattern (v2):
#   Task<bool> _semTask = _playSemaphore.WaitAsync(4000);
#   bool _semGot = (await _semTask.AsUniTask().SuppressCancellationThrow()).Item2;   # never abandons the method
#   if (!_semGot) attach a guard that releases a permit the Task may still deliver late
#   finally releases only when _semGot
# Applies to all 17 sites; keeps MFGDBG logs. Run once.
$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'
$f = 'C:\Users\Pure Fox\Documents\GitHub\mfg-decomp\build\unity-project\Assets\Assembly-CSharp\SoundManager.cs'
$lines = [string[]](Get-Content -LiteralPath $f)
$out = [System.Collections.Generic.List[string]]::new()
$cur = '?'
$acq = 0; $rel = 0
for ($i = 0; $i -lt $lines.Count; $i++) {
    $ln = $lines[$i]
    if ($ln -match 'public static .*? (\w+)\s*\(') { $cur = $Matches[1] }
    if ($ln -match '^\s*await _playSemaphore\.WaitAsync\(\);\s*$') {
        # consume the instrument log line that follows (if any)
        $j = $i + 1
        if ($j -lt $lines.Count -and $lines[$j] -match 'MFGDBG SEM acquired') { $i = $j }
        $out.Add("`t`tSystem.Threading.Tasks.Task<bool> _semTask = _playSemaphore.WaitAsync(4000);")
        $out.Add("`t`tbool _semGot = (await _semTask.AsUniTask().SuppressCancellationThrow()).Item2;")
        $out.Add("`t`tif (!_semGot) { _ = _semTask.ContinueWith(delegate (System.Threading.Tasks.Task<bool> _t) { if (_t.Status == System.Threading.Tasks.TaskStatus.RanToCompletion && _t.Result) { try { _playSemaphore.Release(); UnityEngine.Debug.Log(`"MFGDBG SEM late-release $cur`"); } catch (System.Exception) { } } }); }")
        $out.Add("`t`tUnityEngine.Debug.Log(`"MFGDBG SEM acquire $cur got=`" + _semGot); if (_semGot) { _dbgSem = `"$cur`"; }")
        $acq++
        continue
    }
    $out.Add($ln)
    if ($ln -match '^\s*_playSemaphore\.Release\(\);\s*$') {
        $out[$out.Count - 1] = "`t`t`tif (_semGot) { _dbgSem = `"free`"; _playSemaphore.Release(); }"
        $rel++
    }
}
Set-Content -LiteralPath $f -Value $out -Encoding utf8
"acquires rewritten: $acq  guarded releases: $rel"
