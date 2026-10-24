using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class UnableCharaGachaSelectWindow : MonoBehaviour
{
	public enum State
	{
		None,
		Get,
		Gift
	}

	[SerializeField]
	private GameObject windowObj;

	[SerializeField]
	private GameObject confirmWindowObj;

	[SerializeField]
	private TextMeshProUGUI confirmText;

	[SerializeField]
	private Button btGet;

	[SerializeField]
	private Button btGift;

	[SerializeField]
	private Button closeButton;

	[SerializeField]
	private Button allCloseButton;

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
		allCloseButton.OnClickAsObservable().Subscribe(delegate
		{
			CloseWindow().Forget();
		}).AddTo(this);
		btGet.OnClickAsObservable().Subscribe(delegate
		{
			ConfirmOpen(isGet: true);
		}).AddTo(this);
		btGift.OnClickAsObservable().Subscribe(delegate
		{
			ConfirmOpen(isGet: false);
		}).AddTo(this);
	}

	public void OpenWindow()
	{
		if (!isOpen)
		{
			closeButton.gameObject.SetActive(GameDataManager.PlayerConfig.UnableCharaGachaState != State.None);
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
			if (isConfirmOpen)
			{
				confirmWindowObj.GetComponent<Animator>().SetTrigger("Out");
				isConfirmOpen = false;
				confirmWindowObj.SetActive(value: false);
			}
			SoundManager.PlaySe("SFX_SYS_CANCEL2").Forget();
			windowObj.GetComponent<Animator>().SetTrigger("Out");
			isOpen = false;
			base.gameObject.SetActive(value: false);
			await UniTask.Delay(TimeSpan.FromSeconds(1.0));
		}
	}

	public void ConfirmOpen(bool isGet)
	{
		if (!isConfirmOpen)
		{
			isConfirmOpen = true;
			if (isGet)
			{
				confirmText.text = "「使用不可カットインをそのまま受け取る」";
				GameDataManager.PlayerConfig.UnableCharaGachaState = State.Get;
			}
			else
			{
				confirmText.text = "「使用不可カットインをギフトに変換する」";
				GameDataManager.PlayerConfig.UnableCharaGachaState = State.Gift;
			}
			confirmWindowObj.SetActive(value: true);
			SoundManager.PlaySe("SFX_MJG_INFO1").Forget();
			confirmWindowObj.GetComponent<Animator>().SetTrigger("In");
		}
	}
}
