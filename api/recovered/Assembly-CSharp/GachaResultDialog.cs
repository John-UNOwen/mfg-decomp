using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.Assets;
using MFG.GameData;
using MFG.GameRequest;
using MFG.MasterGameData;
using MFG.Types;
using MFG.Types.Customize.CutinItem;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class GachaResultDialog : MonoBehaviour
{
	[SerializeField]
	private GameObject m_root;

	[SerializeField]
	private Button m_closeButton;

	[SerializeField]
	private TextMeshProUGUI m_text;

	[SerializeField]
	private GameObject m_cardPrefab;

	[SerializeField]
	private GameObject m_cardParent;

	[SerializeField]
	private GameObject m_informationParent;

	[SerializeField]
	private GameObject m_fightSpiritIconParent;

	public BtGachaPlayRetry m_btGachaRetry;

	[SerializeField]
	private TextMeshProUGUI m_exchangeText;

	[SerializeField]
	private TextMeshProUGUI m_exchangeCount;

	[SerializeField]
	private GachaCharaGetAnimation CharaGetNotice;

	private void Start()
	{
		m_closeButton.OnClickAsObservable().Subscribe(delegate
		{
			SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
			m_root.SetActive(value: false);
		}).AddTo(this);
	}

	public void AllRetryButtonUnable()
	{
		m_btGachaRetry.AllRetryButton_Unable();
		m_btGachaRetry.AllRetryButton_Unactive();
	}

	public async UniTask SetItem(GachaResultInfo gachaResultInfo, GachaSeriesServerInfo seriesInfo, GachaPlayMode mode)
	{
		foreach (Transform item in m_cardParent.transform)
		{
			Object.Destroy(item.gameObject);
		}
		foreach (Transform item2 in m_fightSpiritIconParent.transform)
		{
			item2.gameObject.SetActive(value: false);
		}
		int i = 0;
		while (i < gachaResultInfo.FightSpiritInfos.Count)
		{
			GachaFightSpiritInfo gachaFightSpiritInfo = gachaResultInfo.FightSpiritInfos[i];
			Image iconImage = m_fightSpiritIconParent.transform.Find($"Spirit{i + 1}")?.GetComponent<Image>();
			if (iconImage != null)
			{
				IDisposableAsset<Sprite> disposableAsset = await AssetLoader.LoadAssetAsync<Sprite>(AssetPathResolver.Resolve(PathType.Dojo, gachaFightSpiritInfo.CharaType, DojoFlow.AssetType.FightSpirit));
				disposableAsset.AddTo(this);
				iconImage.gameObject.SetActive(value: true);
				iconImage.sprite = disposableAsset.Asset;
			}
			int num = i + 1;
			i = num;
		}
		m_exchangeText.text = seriesInfo.GetExchangePointLabel();
		m_exchangeCount.text = seriesInfo.GetExchangePoint().ToString();
		bool isNormalGift = false;
		bool isUnableCharaGift = false;
		foreach (GachaItemInfo info in gachaResultInfo.ItemInfos)
		{
			GameObject cardView = Object.Instantiate(m_cardPrefab, m_cardParent.transform);
			Item master = CutinItemMaster.GetItem(info.ItemID);
			IDisposableAsset<Sprite> disposableAsset2 = await AssetLoader.LoadAssetAsync<Sprite>(master.IconGraphicPath);
			disposableAsset2.AddTo(cardView);
			cardView.GetComponent<Image>().sprite = disposableAsset2.Asset;
			cardView.transform.Find("New").gameObject.SetActive(info.GiftType == GachaGiftType.IsNew);
			cardView.transform.Find("ChangeGift").gameObject.SetActive(info.GiftType == GachaGiftType.DuplicateGift);
			if (info.GiftType == GachaGiftType.DuplicateGift)
			{
				isNormalGift = true;
			}
			cardView.transform.Find("ChangeGiftUnableChara").gameObject.SetActive(info.GiftType == GachaGiftType.ConfigGift);
			if (info.GiftType == GachaGiftType.ConfigGift)
			{
				isUnableCharaGift = true;
			}
			disposableAsset2 = await AssetLoader.LoadAssetAsync<Sprite>("CharaSelect/Textures/CustomIconRare" + master.Rarity.ToString() + "Big.png");
			Transform transform = cardView.transform.Find("Rank").Find("Image");
			disposableAsset2.AddTo(transform);
			transform.GetComponent<Image>().sprite = disposableAsset2.Asset;
		}
		m_informationParent.SetActive(gachaResultInfo.AcquiredGiftCount > 0);
		m_informationParent.transform.Find("PresentsInformation/ InformationText").GetComponent<TextMeshProUGUI>().text = (isNormalGift ? "獲得済みのカットインはギフトに変換されました\n" : "") + (isUnableCharaGift ? "未所持ファイトガールのカットインはギフトに変換されました" : "");
		m_informationParent.transform.Find("PresentsInformation").Find("PresentsNumber").Find("BeforNumber")
			.GetComponent<TextMeshProUGUI>()
			.text = $"{gachaResultInfo.PrevGiftCount}";
		m_informationParent.transform.Find("PresentsInformation").Find("PresentsNumber").Find("AfterNumber")
			.GetComponent<TextMeshProUGUI>()
			.text = $"{gachaResultInfo.AfterGiftCount}";
		await GachaRetryButtonInit(mode, seriesInfo);
	}

	public async UniTask GachaRetryButtonInit(GachaPlayMode mode, GachaSeriesServerInfo seriesInfo)
	{
		try
		{
			GachaInfoRequest.ResultData resultData = await RequestApiBase<GachaInfoRequest, GachaInfoRequest.ResultData>.Send(enableErrorScreenTransition: false);
			GameDataManager.GachaInfo.GachaSeriesInfosFromServer = resultData.ResultGachaSeriesInfosFromServer;
			GameDataManager.Gacha.Validate();
			GachaSeriesServerInfo gachaSeriesServerInfo = GameDataManager.GachaInfo.GachaSeriesInfosFromServer.Find((GachaSeriesServerInfo x) => x.SeriesID == seriesInfo.SeriesID);
			if (gachaSeriesServerInfo != null && gachaSeriesServerInfo.IsDisplay())
			{
				m_btGachaRetry.RetryButtonSetup(mode, gachaSeriesServerInfo);
			}
			else
			{
				m_btGachaRetry.RetryUnableTask(mode);
			}
		}
		catch
		{
		}
	}

	public async UniTask Open(CancellationToken cancellationToken)
	{
		List<CharaType> infoNewGirlNoticeFromGacha = GameDataManager.Notice.InfoNewGirlNoticeFromGacha;
		bool isCharaGetNoticePlay = infoNewGirlNoticeFromGacha != null && infoNewGirlNoticeFromGacha.Count() > 0;
		bool isButtonToActive = m_btGachaRetry.gameObject.activeSelf && isCharaGetNoticePlay;
		if (isCharaGetNoticePlay)
		{
			m_btGachaRetry.gameObject.SetActive(value: false);
			m_closeButton.interactable = false;
		}
		SoundManager.PlaySe("SFX_SYS_GACYA7").Forget();
		m_root.SetActive(value: true);
		await CharaGetNotice.GachaGetCharaNotice(cancellationToken);
		if (isCharaGetNoticePlay)
		{
			m_btGachaRetry.gameObject.SetActive(isButtonToActive);
			m_closeButton.interactable = true;
		}
	}
}
