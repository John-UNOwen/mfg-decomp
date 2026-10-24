using System;
using UniRx;
using UniRx.Triggers;
using UnityEngine.UI;

public static class UIExtensions
{
	public static IObservable<Unit> OnLongTapAsObservable(this Button button, float pressSeconds = 0.5f)
	{
		return button.OnPointerDownAsObservable().Throttle(TimeSpan.FromSeconds(pressSeconds)).TakeUntil(button.OnPointerExitAsObservable())
			.TakeUntil(button.OnPointerUpAsObservable())
			.RepeatUntilDestroy(button)
			.AsUnitObservable();
	}

	public static IObservable<Unit> OnClickAsObservableSafety(this Button button, float duplicateSafetySeconds = 0.5f, float pressSafetySeconds = 0.5f)
	{
		return button.OnClickAsObservable().ThrottleFirst(TimeSpan.FromSeconds(duplicateSafetySeconds)).SkipUntil(button.OnPointerDownAsObservable())
			.TakeUntil(button.OnLongTapAsObservable(pressSafetySeconds))
			.RepeatUntilDestroy(button)
			.AsUnitObservable();
	}
}
