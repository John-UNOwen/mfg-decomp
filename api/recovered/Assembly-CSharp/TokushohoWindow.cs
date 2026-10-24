using System;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class TokushohoWindow : MonoBehaviour
{
	[SerializeField]
	private GameObject windowObj;

	[SerializeField]
	private GameObject agreementObj;

	[SerializeField]
	private Button btYes;

	[SerializeField]
	private Button btNo;

	[SerializeField]
	private Button closeButton;

	[NonSerialized]
	public bool isOpen;

	[NonSerialized]
	public bool isConfirmOpen;

	private async void Start()
	{
		closeButton.OnClickAsObservable().Subscribe(delegate
		{
			CloseWindow().Forget();
		}).AddTo(this);
		btYes.OnClickAsObservable().Subscribe(delegate
		{
			GameDataManager.PlayerConfig.IsTokushohoAgreed = true;
			CloseWindow().Forget();
		}).AddTo(this);
		btNo.OnClickAsObservable().Subscribe(delegate
		{
			CloseWindow().Forget();
		}).AddTo(this);
	}

	public void OpenWindow(bool isForceOpen)
	{
		if (!isOpen)
		{
			agreementObj.SetActive(isForceOpen);
			closeButton.gameObject.SetActive(!isForceOpen);
			isOpen = true;
			base.gameObject.SetActive(value: true);
			SoundManager.PlaySe("SFX_MJG_INFO1").Forget();
			windowObj.GetComponent<Animator>().SetTrigger("In");
		}
	}

	public async UniTask CloseWindow()
	{
		if (isOpen)
		{
			SoundManager.PlaySe("SFX_SYS_CANCEL2").Forget();
			windowObj.GetComponent<Animator>().SetTrigger("Out");
			isOpen = false;
			base.gameObject.SetActive(value: false);
			await UniTask.Delay(TimeSpan.FromSeconds(1.0));
		}
	}
}
