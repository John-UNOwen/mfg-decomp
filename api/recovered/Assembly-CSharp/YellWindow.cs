using System;
using Cysharp.Threading.Tasks;
using MFG.Assets;
using MFG.GameData;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class YellWindow : MonoBehaviour
{
	[SerializeField]
	private GameObject windowObj;

	[SerializeField]
	private Button closeButton;

	[SerializeField]
	private Button benefitButton;

	[SerializeField]
	private Button benefitCloseButton;

	[SerializeField]
	private GameObject benefitWindow;

	[SerializeField]
	private Button yellRateButton;

	[SerializeField]
	private Button yellRateCloseButton;

	[SerializeField]
	private GameObject yellRateWindow;

	[SerializeField]
	private GameObject charaParent;

	[SerializeField]
	private TextMeshProUGUI dateText;

	[SerializeField]
	private TextMeshProUGUI addDebugYellDisp;

	public bool isOpen;

	private bool isBenefitOpen;

	private bool isYellRateOpen;

	private YellCampaignInfo yellInfo;

	private async void Start()
	{
		yellInfo = GameDataManager.YellCampaign.GetCurrentCampaignInfo();
		if (yellInfo != null)
		{
			foreach (YellInfo charaInfo in yellInfo.YellInfos)
			{
				if (GameDataManager.YellCampaign.IsCharacterEnable(charaInfo.Chara))
				{
					Transform pref = charaParent.transform.Find("Image" + ((int)(charaInfo.Chara + 1)).ToString("D2"));
					string path = AssetPathResolver.ResolveYellRankImage(charaInfo.GetYellRank());
					if (await AssetLoader.Exists(path))
					{
						IDisposableAsset<Sprite> disposableAsset = await AssetLoader.LoadAssetAsync<Sprite>(path);
						disposableAsset.AddTo(this);
						pref.Find("Rank").GetComponent<Image>().sprite = disposableAsset.Asset;
					}
					pref.Find("YellNum").GetComponent<TextMeshProUGUI>().text = charaInfo.YellCount.ToString();
				}
			}
		}
		GameEventData gameEventData = GameDataManager.System.GameControlInfo.EventStatus.GetGameEventData(GameEventType.YellCampaign);
		if (gameEventData != null)
		{
			dateText.text = gameEventData.End.ToString("yyyy年MM月dd日（ddd）HH：mmまで");
		}
		closeButton.OnClickAsObservable().Subscribe(delegate
		{
			CloseWindow().Forget();
		}).AddTo(this);
		benefitButton.OnClickAsObservable().Subscribe(delegate
		{
			if (!isBenefitOpen)
			{
				isBenefitOpen = true;
				benefitWindow.SetActive(value: true);
				SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
				benefitWindow.GetComponent<Animator>().SetTrigger("In");
			}
		}).AddTo(this);
		yellRateButton.OnClickAsObservable().Subscribe(delegate
		{
			if (!isYellRateOpen)
			{
				isYellRateOpen = true;
				yellRateWindow.SetActive(value: true);
				SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
				yellRateWindow.GetComponent<Animator>().SetTrigger("In");
			}
		}).AddTo(this);
		benefitCloseButton.OnClickAsObservable().Subscribe(delegate
		{
			if (isBenefitOpen)
			{
				SoundManager.PlaySe("SFX_SYS_CANCEL2").Forget();
				benefitWindow.GetComponent<Animator>().SetTrigger("Out");
				benefitWindow.SetActive(value: false);
				isBenefitOpen = false;
			}
		}).AddTo(this);
		yellRateCloseButton.OnClickAsObservable().Subscribe(delegate
		{
			if (isYellRateOpen)
			{
				SoundManager.PlaySe("SFX_SYS_CANCEL2").Forget();
				yellRateWindow.GetComponent<Animator>().SetTrigger("Out");
				yellRateWindow.SetActive(value: false);
				isYellRateOpen = false;
			}
		}).AddTo(this);
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

	private void Update()
	{
	}
}
