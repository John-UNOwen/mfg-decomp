using System;
using System.Runtime.CompilerServices;
using System.Threading;
using DG.Tweening;

public struct TweenAwaiter : ICriticalNotifyCompletion, INotifyCompletion
{
	private Tween tween;

	private CancellationToken token;

	public bool IsCompleted => !tween.IsPlaying();

	public TweenAwaiter(Tween tween, CancellationToken token)
	{
		this.tween = tween;
		this.token = token;
	}

	public void GetResult()
	{
		token.ThrowIfCancellationRequested();
	}

	public void OnCompleted(Action continuation)
	{
		UnsafeOnCompleted(continuation);
	}

	public void UnsafeOnCompleted(Action continuation)
	{
		CancellationTokenRegistration regist = default(CancellationTokenRegistration);
		Tween tween = this.tween;
		tween.OnKill(delegate
		{
			regist.Dispose();
			continuation();
		});
		regist = token.Register(delegate
		{
			tween.Kill(complete: true);
		});
	}

	public TweenAwaiter GetAwaiter()
	{
		return this;
	}
}
