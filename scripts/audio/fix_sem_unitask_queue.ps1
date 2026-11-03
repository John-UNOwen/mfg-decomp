# v5: replace the Task/SemaphoreSlim bridge with a UniTask-native queued semaphore:
#   - permit handoff entirely on the main loop (UniTaskCompletionSource), no thread-pool continuations
#   - cancelled acquirer that had already been granted returns the permit
# Facade call pattern becomes: bool _semGot = await _playSemaphore.WaitAsync("X");
$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'
$f = 'C:\Users\Pure Fox\Documents\GitHub\mfg-decomp\build\unity-project\Assets\Assembly-CSharp\SoundManager.cs'
$lines = [string[]](Get-Content -LiteralPath $f)
$out = [System.Collections.Generic.List[string]]::new()
$cur = '?'
foreach ($ln in $lines) {
    if ($ln -match 'public static .*? (\w+)\s*\(') { $cur = $Matches[1] }
    if ($ln -match '^\s*System\.Threading\.Tasks\.Task<bool> _semTask = _playSemaphore\.WaitAsync\("\w+"\);\s*$') {
        $out.Add("`t`tbool _semGot = await _playSemaphore.WaitAsync(`"$cur`");")
        continue
    }
    # drop the old bridge + late-guard lines
    if ($ln -match '_semTask\.AsUniTask\(\)\.SuppressCancellationThrow' -or $ln -match '_semTask\.ContinueWith') { continue }
    $out.Add($ln)
}
# swap the LoggedSem class body
$text = ($out -join "`n")
$newCls = @'
	// 2026-11-03 MFGDBG/fix: UniTask-native queued semaphore. Permits hand off on the Unity main
	// loop via UniTaskCompletionSource - no SemaphoreSlim task bridge, no thread-pool continuations
	// (WebGL has no usable thread pool). A cancelled acquirer that was already granted its permit
	// returns it, so scene-switch cancellations can no longer leak the permit (the A1 Home-BGM
	// deadlock: a queued façade waiter cancelled mid-Title-unload consumed the permit forever).
	private class LoggedSem
	{
		private readonly System.Collections.Generic.List<Cysharp.Threading.Tasks.UniTaskCompletionSource<bool>> _queue = new System.Collections.Generic.List<Cysharp.Threading.Tasks.UniTaskCompletionSource<bool>>();
		private bool _permit = true;

		public async Cysharp.Threading.Tasks.UniTask<bool> WaitAsync(string who)
		{
			if (_permit)
			{
				_permit = false;
				UnityEngine.Debug.Log("MFGDBG SEM granted " + who);
				return true;
			}
			Cysharp.Threading.Tasks.UniTaskCompletionSource<bool> tcs = new Cysharp.Threading.Tasks.UniTaskCompletionSource<bool>();
			_queue.Add(tcs);
			bool cancelled;
			try
			{
				cancelled = (await tcs.Task.SuppressCancellationThrow()).Item1;
			}
			catch (System.Exception)
			{
				cancelled = true;
			}
			if (cancelled)
			{
				_queue.Remove(tcs);
				bool granted = false;
				try { granted = tcs.Task.Task != null && tcs.Task.Task.IsCompleted && tcs.Task.Task.Result; } catch (System.Exception) { }
				if (granted)
				{
					UnityEngine.Debug.Log("MFGDBG SEM cancel-with-permit " + who + " -> requeued");
					if (_queue.Count > 0)
					{
						Cysharp.Threading.Tasks.UniTaskCompletionSource<bool> next = _queue[0];
						_queue.RemoveAt(0);
						next.TrySetResult(true);
					}
					else
					{
						_permit = true;
					}
				}
				UnityEngine.Debug.Log("MFGDBG SEM cancelled " + who);
				return false;
			}
			UnityEngine.Debug.Log("MFGDBG SEM granted " + who + " queued");
			return true;
		}

		public void Release(string who)
		{
			if (_queue.Count > 0)
			{
				Cysharp.Threading.Tasks.UniTaskCompletionSource<bool> tcs = _queue[0];
				_queue.RemoveAt(0);
				tcs.TrySetResult(true);
				UnityEngine.Debug.Log("MFGDBG SEM release " + who + " -> handed to waiter");
			}
			else
			{
				_permit = true;
				UnityEngine.Debug.Log("MFGDBG SEM release " + who + " avail");
			}
		}
	}
'@
$text = [regex]::Replace($text, '(?s)// 2026-11-03 MFGDBG: logged semaphore wrapper.*?\n\t\}\n\}', ($newCls -replace '\$','$$$') + "`n}")
Set-Content -LiteralPath $f -Value $text -Encoding utf8
"semaphore rewritten; file lines now: $((Get-Content -LiteralPath $f).Count)"
