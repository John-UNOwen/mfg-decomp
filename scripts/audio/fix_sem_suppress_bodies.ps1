# v3: make every AudioManager await inside SoundManager façade bodies cancellation-immune
# via .SuppressCancellationThrow() so silent UniTask cancellation can never skip a finally.
$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'
$f = 'C:\Users\Pure Fox\Documents\GitHub\mfg-decomp\build\unity-project\Assets\Assembly-CSharp\SoundManager.cs'
$t = Get-Content -LiteralPath $f -Raw
# value awaits: int num = await AudioManager.Play(...);
$t = [regex]::Replace($t, 'int num = await AudioManager\.(Play|PlayOption)\(([^;]+?)\);', 'int num = (await AudioManager.$1($2).SuppressCancellationThrow()).Item2;')
# inline await in expression: AssertLabelError(_label, await AudioManager.Play(...) != ...)
$t = [regex]::Replace($t, 'await AudioManager\.(Play|PlayOption)\(([^;]+?)\) != AudioDefine\.INSTANCE_ID_ERROR', '(await AudioManager.$1($2).SuppressCancellationThrow()).Item2 != AudioDefine.INSTANCE_ID_ERROR')
# statement awaits: await AudioManager.X(...);  (skip already-suppressed)
$t = [regex]::Replace($t, 'await AudioManager\.(PrepareAudioClip|Stop|StopLabel|StopAll|StopCategory|OnPauseCategory|OffPauseCategory)\(([^;]*?)\);(?!\s*// suppressed)(?!\s*\.Suppress)', 'await AudioManager.$1($2).SuppressCancellationThrow();')
[System.IO.File]::WriteAllText($f, $t)
"suppressed AudioManager awaits: " + ([regex]::Matches($t, '\.SuppressCancellationThrow\(\)').Count)
