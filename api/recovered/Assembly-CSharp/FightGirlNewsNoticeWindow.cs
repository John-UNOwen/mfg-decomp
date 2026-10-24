using System;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class FightGirlNewsNoticeWindow : MonoBehaviour
{
	[SerializeField]
	private GameObject windowObj;

	[SerializeField]
	private GameObject imageObj;

	public bool isOpen;

	private async void Start()
	{
		imageObj.GetComponent<Button>().OnClickAsObservable().Subscribe(delegate
		{
			CloseWindow().Forget();
		})
			.AddTo(this);
	}

	public void OpenWindow()
	{
		if (!isOpen)
		{
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
			await UniTask.Delay(TimeSpan.FromSeconds(1.0));
		}
	}
}
