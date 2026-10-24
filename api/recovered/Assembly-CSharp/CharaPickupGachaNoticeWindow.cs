using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MFG.Scenes.GachaSelect;
using MFG.Types;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class CharaPickupGachaNoticeWindow : MonoBehaviour
{
	[Serializable]
	public class GachaNoticeImage
	{
		public CharaType Chara;

		public Sprite Image;

		public Sprite CharaImage;

		public int DatePos = 1;
	}

	[SerializeField]
	private GameObject windowObj;

	[SerializeField]
	private Image noticeImage;

	[SerializeField]
	private Image charaImage;

	[SerializeField]
	private Button closeButton;

	[SerializeField]
	private Button pickupButton;

	[SerializeField]
	private TextMeshProUGUI dateText;

	[SerializeField]
	private TextMeshProUGUI dateText2;

	[NonSerialized]
	public bool isOpen;

	[NonSerialized]
	public bool isToGacha;

	[SerializeField]
	private List<GachaNoticeImage> noticeImages;

	private async void Start()
	{
		closeButton.OnClickAsObservable().Subscribe(delegate
		{
			CloseWindow().Forget();
		}).AddTo(this);
	}

	public void OpenWindow(GachaSelectFlow flow)
	{
		if (isOpen)
		{
			return;
		}
		GachaNoticeImage gachaNoticeImage = noticeImages.Find((GachaNoticeImage x) => x.Chara == GameDataManager.Notice.PickupGachaNoticeCharaType);
		if (gachaNoticeImage == null)
		{
			return;
		}
		noticeImage.sprite = gachaNoticeImage.Image;
		charaImage.sprite = gachaNoticeImage.CharaImage;
		dateText.text = GameDataManager.Notice.PickupGachaNoticeDate;
		dateText2.text = GameDataManager.Notice.PickupGachaNoticeDate;
		dateText.gameObject.SetActive(gachaNoticeImage.DatePos == 1);
		dateText2.gameObject.SetActive(gachaNoticeImage.DatePos == 2);
		if (flow != null)
		{
			pickupButton.gameObject.SetActive(value: true);
			pickupButton.OnClickAsObservable().Subscribe(delegate
			{
				isToGacha = true;
				SoundManager.PlaySe("SFX_MJG_INFO1").Forget();
				flow.GoToPickupGacha();
			}).AddTo(this);
		}
		else
		{
			pickupButton.gameObject.SetActive(value: false);
		}
		GameDataManager.Notice.PickupGachaNoticeCharaType = (CharaType)(-1);
		isOpen = true;
		base.gameObject.SetActive(value: true);
		SoundManager.PlaySe("SFX_MJG_INFO1").Forget();
		windowObj.GetComponent<Animator>().SetTrigger("In");
	}

	public async UniTask CloseWindow()
	{
		if (isOpen && !isToGacha)
		{
			SoundManager.PlaySe("SFX_SYS_CANCEL2").Forget();
			try
			{
				windowObj.GetComponent<Animator>().SetTrigger("Out");
			}
			catch
			{
			}
			isOpen = false;
			base.gameObject.SetActive(value: false);
			await UniTask.Delay(TimeSpan.FromSeconds(1.0));
		}
	}
}
