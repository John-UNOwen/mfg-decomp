using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TMPro;
using UnityEngine;

public static class TextMeshProUGUIExtensions
{
	public static TweenerCore<Color, Color, ColorOptions> DOColor(this TextMeshProUGUI target, Color endValue, float durationSec)
	{
		TweenerCore<Color, Color, ColorOptions> tweenerCore = DOTween.To(() => target.color, delegate(Color x)
		{
			target.color = x;
		}, endValue, durationSec);
		tweenerCore.SetTarget(target);
		return tweenerCore;
	}

	public static TweenerCore<Color, Color, ColorOptions> DOFade(this TextMeshProUGUI target, float endValue, float durationSec)
	{
		TweenerCore<Color, Color, ColorOptions> tweenerCore = DOTween.ToAlpha(() => target.color, delegate(Color x)
		{
			target.color = x;
		}, endValue, durationSec);
		tweenerCore.SetTarget(target);
		return tweenerCore;
	}

	public static void SetAlpha(this TextMeshProUGUI target, float alpha)
	{
		Color color = target.color;
		target.color = new Color(color.r, color.g, color.b, alpha);
	}
}
