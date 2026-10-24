using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public static class ImageExtensions
{
	public static void SetAlpha(this Image image, float alpha)
	{
		Color color = image.color;
		image.color = new Color(color.r, color.g, color.b, alpha);
	}

	public static async UniTask SetSpriteAndFadeIn(this Image iconImage, Sprite sprite, float startValue = 0f, float endValue = 1f, float duration = 0.2f, CancellationToken cancellationToken = default(CancellationToken))
	{
		iconImage.SetAlpha(startValue);
		iconImage.sprite = sprite;
		await iconImage.DOFade(endValue, duration).ToUniTask(TweenCancelBehaviour.Kill, cancellationToken);
	}
}
