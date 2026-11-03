# v4: swap _playSemaphore for a fully-logged wrapper (issue/completion/release with method names),
# plain WaitAsync (no timeout - WebGL has no thread-pool timers), keep suppression + late guard.
$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'
$f = 'C:\Users\Pure Fox\Documents\GitHub\mfg-decomp\build\unity-project\Assets\Assembly-CSharp\SoundManager.cs'
$lines = [string[]](Get-Content -LiteralPath $f)
$out = [System.Collections.Generic.List[string]]::new()
$cur = '?'
foreach ($ln in $lines) {
    if ($ln -match 'public static .*? (\w+)\s*\(') { $cur = $Matches[1] }
    if ($ln -match 'private static SemaphoreSlim _playSemaphore = new SemaphoreSlim\(1, 1\);') {
        $out.Add("`tprivate static LoggedSem _playSemaphore = new LoggedSem();")
        continue
    }
    if ($ln -match '^\s*System\.Threading\.Tasks\.Task<bool> _semTask = _playSemaphore\.WaitAsync\(4000\);\s*$') {
        $out.Add("`t`tSystem.Threading.Tasks.Task<bool> _semTask = _playSemaphore.WaitAsync(`"$cur`");")
        continue
    }
    if ($ln -match '^\s*if \(_semGot\) \{ _dbgSem = "free"; _playSemaphore\.Release\(\); \}\s*$') {
        $out.Add("`t`t`tif (_semGot) { _dbgSem = `"free`"; _playSemaphore.Release(`"$cur`"); }")
        continue
    }
    if ($ln -match '_playSemaphore\.Release\(\);' ) {
        # late guard release inside ContinueWith lambda
        $out.Add(($ln -replace '_playSemaphore\.Release\(\);', '_playSemaphore.Release("late-$cur")'))
        continue
    }
    $out.Add($ln)
}
# append wrapper class before final closing brace of SoundManager
$closeIdx = $out.Count - 1
while ($closeIdx -ge 0 -and $out[$closeIdx].Trim() -ne '}') { $closeIdx-- }
$cls = @'

	// 2026-11-03 MFGDBG: logged semaphore wrapper - WebGL-friendly (plain WaitAsync fast path only,
	// no timeout timers), full issue/completion/release lifecycle for the permit-leak hunt.
	private class LoggedSem
	{
		private readonly System.Threading.SemaphoreSlim _sem = new System.Threading.SemaphoreSlim(1, 1);

		public System.Threading.Tasks.Task<bool> WaitAsync(string who)
		{
			UnityEngine.Debug.Log("MFGDBG SEM issue " + who + " avail=" + _sem.CurrentAvailableCount);
			System.Threading.Tasks.Task<bool> t = _sem.WaitAsync();
			_ = t.ContinueWith(delegate (System.Threading.Tasks.Task<bool> x) { UnityEngine.Debug.Log("MFGDBG SEM done " + who + " got=" + x.Result); });
			return t;
		}

		public void Release(string who)
		{
			UnityEngine.Debug.Log("MFGDBG SEM release " + who + " avail=" + _sem.CurrentAvailableCount);
			try { _sem.Release(); } catch (System.Exception ex) { UnityEngine.Debug.Log("MFGDBG SEM release-overflow " + who + " " + ex.Message); }
		}
	}
'@
$out.Insert($closeIdx, $cls)
Set-Content -LiteralPath $f -Value $out -Encoding utf8
"wrapper inserted before line $closeIdx"
