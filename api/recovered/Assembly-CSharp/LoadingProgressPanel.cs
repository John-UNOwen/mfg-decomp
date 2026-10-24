using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class LoadingProgressPanel : MonoBehaviour
{
	[SerializeField]
	private Slider _loadingProgress;

	private Tweener _progressTweener;

	public void SetProgress(float progress, float duration = 0.3f)
	{
		ProgressTweener_Clear();
		if (duration == 0f)
		{
			_loadingProgress.GetComponent<Slider>().value = progress;
			return;
		}
		_progressTweener = _loadingProgress.GetComponent<Slider>().DOValue(progress, duration).OnComplete(delegate
		{
			_progressTweener = null;
		});
	}

	public void Open(float progress = 0f)
	{
		SetProgress(progress, 0f);
		base.gameObject.SetActive(value: true);
	}

	public void Close()
	{
		ProgressTweener_Clear();
		base.gameObject.SetActive(value: false);
	}

	private void ProgressTweener_Clear()
	{
		if (_progressTweener != null)
		{
			_progressTweener.Kill();
			_progressTweener = null;
		}
	}
}
