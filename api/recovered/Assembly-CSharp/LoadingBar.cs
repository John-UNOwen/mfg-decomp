using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

public class LoadingBar : MonoBehaviour
{
	[SerializeField]
	private GameObject _root;

	[SerializeField]
	private TextMeshProUGUI _text;

	public async UniTask Play(TextManager.KEY key, TimeSpan durationTimeSpan, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		await Play(TextManager.Get(key), durationTimeSpan, cancellationToken);
	}

	public async UniTask Play(string text, TimeSpan durationTimeSpan, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		try
		{
			await UniTask.Yield();
			SetDisp(isDisp: false);
			await UniTask.Yield();
			SetText(text);
			SetDisp(isDisp: true);
			await UniTask.Delay(durationTimeSpan, ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
			SetDisp(isDisp: false);
			await UniTask.Yield();
		}
		catch (OperationCanceledException)
		{
		}
	}

	public void SetText(string text)
	{
		_text.text = text;
	}

	public void SetDisp(bool isDisp)
	{
		_root.SetActive(isDisp);
	}
}
