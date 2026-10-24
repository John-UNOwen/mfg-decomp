using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class YellUnableResult : MonoBehaviour
{
	[SerializeField]
	private GameObject windowObj;

	[SerializeField]
	private Button closeButton;

	public bool isOpen;

	private async void Start()
	{
		closeButton.OnClickAsObservable().Subscribe(delegate
		{
			CloseWindow().Forget();
		}).AddTo(this);
	}

	public async UniTask OpenWindow(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!isOpen)
		{
			isOpen = true;
			base.gameObject.SetActive(value: true);
			windowObj.GetComponent<Animator>().SetTrigger("In");
			await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(8.0), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken), UniTask.WaitUntil(() => ResultFlow.Skip(), PlayerLoopTiming.Update, cancellationToken), UniTask.WaitUntil(() => !isOpen, PlayerLoopTiming.Update, cancellationToken));
			await CloseWindow(cancellationToken);
		}
	}

	public async UniTask CloseWindow(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (isOpen)
		{
			SoundManager.StopSeAll().Forget();
			SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
			windowObj.GetComponent<Animator>().SetTrigger("Out");
			isOpen = false;
			await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(1.0), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken), UniTask.WaitUntil(() => ResultFlow.Skip(), PlayerLoopTiming.Update, cancellationToken));
		}
	}
}
