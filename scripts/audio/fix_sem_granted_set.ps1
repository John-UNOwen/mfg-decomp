# Splice the final LoggedSem implementation: granted-set bookkeeping, no UniTask APIs beyond
# UniTaskCompletionSource/SuppressCancellationThrow (compatible with the bundled UniTask version).
$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'
$f = 'C:\Users\Pure Fox\Documents\GitHub\mfg-decomp\build\unity-project\Assets\Assembly-CSharp\SoundManager.cs'
$t = Get-Content -LiteralPath $f -Raw
$newCls = @'
	// 2026-11-03 MFGDBG/fix: UniTask-native queued semaphore. Permits hand off on the Unity main
	// loop via UniTaskCompletionSource - no SemaphoreSlim task bridge, no thread-pool continuations
	// (WebGL has no usable thread pool). A cancelled acquirer that was already granted its permit
	// returns it, so scene-switch cancellations can no longer leak the permit (the A1 Home-BGM
	// deadlock: a queued facade waiter cancelled mid-Title-unload consumed the permit forever).
	private class LoggedSem
	{
		private readonly System.Collections.Generic.List<Cysharp.Threading.Tasks.UniTaskCompletionSource<bool>> _queue = new System.Collections.Generic.List<Cysharp.Threading.Tasks.UniTaskCompletionSource<bool>>();
		private readonly System.Collections.Generic.HashSet<Cysharp.Threading.Tasks.UniTaskCompletionSource<bool>> _granted = new System.Collections.Generic.HashSet<Cysharp.Threading.Tasks.UniTaskCompletionSource<bool>>();
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
				if (_granted.Remove(tcs))
				{
					UnityEngine.Debug.Log("MFGDBG SEM cancel-with-permit " + who + " -> rehanded");
					Handoff();
				}
				UnityEngine.Debug.Log("MFGDBG SEM cancelled " + who);
				return false;
			}
			_granted.Remove(tcs);
			UnityEngine.Debug.Log("MFGDBG SEM granted " + who + " queued");
			return true;
		}

		public void Release(string who)
		{
			if (_queue.Count > 0)
			{
				Cysharp.Threading.Tasks.UniTaskCompletionSource<bool> tcs = _queue[0];
				_queue.RemoveAt(0);
				if (tcs.TrySetResult(true))
				{
					_granted.Add(tcs);
					UnityEngine.Debug.Log("MFGDBG SEM release " + who + " -> handed to waiter");
				}
				else
				{
					// waiter vanished mid-handoff; fall back to the free-permit path
					_permit = true;
					UnityEngine.Debug.Log("MFGDBG SEM release " + who + " avail (handoff lost)");
				}
			}
			else
			{
				_permit = true;
				UnityEngine.Debug.Log("MFGDBG SEM release " + who + " avail");
			}
		}

		private void Handoff()
		{
			if (_queue.Count > 0)
			{
				Cysharp.Threading.Tasks.UniTaskCompletionSource<bool> tcs = _queue[0];
				_queue.RemoveAt(0);
				if (tcs.TrySetResult(true))
				{
					_granted.Add(tcs);
				}
				else
				{
					_permit = true;
				}
			}
			else
			{
				_permit = true;
			}
		}
	}
'@
$pattern = '(?s)// 2026-11-03 MFGDBG/fix: UniTask-native queued semaphore.*?\n\t\}\n\}'
if (-not [regex]::IsMatch($t, $pattern)) { throw 'wrapper class pattern not found' }
$t2 = [regex]::Replace($t, $pattern, ($newCls -replace '\$', '$$$$') + "`n}")
Set-Content -LiteralPath $f -Value $t2 -Encoding utf8
"wrapper spliced; lines: $((Get-Content -LiteralPath $f).Count)"
