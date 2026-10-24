using System.Threading;
using DG.Tweening;

public static class TweenAwaiterEx
{
	public static TweenAwaiter ToAwaiter(this Tween self, CancellationToken token = default(CancellationToken))
	{
		return new TweenAwaiter(self, token);
	}
}
