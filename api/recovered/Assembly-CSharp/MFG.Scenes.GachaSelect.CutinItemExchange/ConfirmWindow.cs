using System.Threading;
using Cysharp.Threading.Tasks;
using Konami.System.Net;
using MFG.GameData;
using MFG.GameRequest;
using MFG.MasterGameData;
using MFG.Types;
using MFG.Types.Customize.CutinItem;
using MFG.Types.Customize.MatchingBackground;
using MFG.Types.PlayerItem;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace MFG.Scenes.GachaSelect.CutinItemExchange;

public class ConfirmWindow : MonoBehaviour
{
	[SerializeField]
	private GachaSelectFlow m_scene;

	[SerializeField]
	private ItemIcon m_itemIcon;

	[SerializeField]
	private TextMeshProUGUI m_itemNameText;

	[SerializeField]
	private TextMeshProUGUI m_exchangeTextConfirm;

	[SerializeField]
	private TextMeshProUGUI m_requiredExchangePointText;

	[SerializeField]
	private TextMeshProUGUI m_bonusFightSpiritText;

	[SerializeField]
	private TextMeshProUGUI m_beforeExchangeText;

	[SerializeField]
	private TextMeshProUGUI m_afterExchangeText;

	[SerializeField]
	private Button m_noButton;

	[SerializeField]
	private Button m_yesButton;

	[SerializeField]
	private ResultWindow m_resultWindow;

	[SerializeField]
	private GameObject m_raycastMaskPanel;

	[SerializeField]
	private GameObject m_caution;

	[SerializeField]
	private GameObject m_caution2;

	[SerializeField]
	private GameObject m_caution3;

	[SerializeField]
	private CardConnectWindow m_cardConnectWindow;

	private bool m_isInitialized;

	private bool m_enableForceTimeOutExit;

	private GachaSeriesServerInfo m_gachaSeriesInfo;

	private MFG.Types.Customize.CutinItem.ItemID m_exchangeItemID;

	private MFG.Types.Customize.CutinItem.Item m_exchangeItem;

	private int m_requiredExchangePoint;

	private bool m_isOpen;

	public bool IsOpen => m_isOpen;

	private void Start()
	{
	}

	private void OnEnable()
	{
	}

	private void Update()
	{
		if (m_enableForceTimeOutExit && ResidentFlow.GetInstance().GetTimer().IsEnd())
		{
			m_enableForceTimeOutExit = false;
			ExecuteExit();
		}
	}

	private void ExecuteExit(bool callSe = true)
	{
		if (callSe)
		{
			SoundManager.PlaySe("SFX_SYS_CANCEL1").Forget();
		}
		m_yesButton.enabled = false;
		m_noButton.enabled = false;
		base.gameObject.GetComponent<Animator>().SetTrigger("Out");
		m_raycastMaskPanel?.SetActive(value: false);
		m_isOpen = false;
	}

	private async UniTask Init(GachaSeriesServerInfo gachaSeriesInfo, MFG.Types.Customize.CutinItem.ItemID exchangeItemID, CancellationToken cancellationToken = default(CancellationToken))
	{
		m_gachaSeriesInfo = gachaSeriesInfo;
		m_exchangeItemID = exchangeItemID;
		m_exchangeItem = CutinItemMaster.GetItem(m_exchangeItemID);
		m_requiredExchangePoint = MFG.Types.Customize.CutinItem.Item.GetRequiredExchangePoint(m_exchangeItem.Rarity);
		await m_itemIcon.Reflesh(m_exchangeItemID);
		m_requiredExchangePointText.text = $"{m_requiredExchangePoint}";
		m_exchangeTextConfirm.text = m_gachaSeriesInfo.GetExchangePointLabel(1);
		m_beforeExchangeText.text = $"{m_gachaSeriesInfo.GetExchangePoint()}";
		m_afterExchangeText.text = $"{m_gachaSeriesInfo.GetExchangePoint() - m_requiredExchangePoint}";
		MFG.Types.Customize.CutinItem.Item item = CutinItemMaster.GetItem(m_exchangeItemID);
		m_itemNameText.text = item.Name ?? "";
		if (m_isInitialized)
		{
			return;
		}
		m_enableForceTimeOutExit = true;
		m_noButton.OnClickAsObservable().Subscribe(delegate
		{
			ExecuteExit();
		}).AddTo(this);
		m_yesButton.OnClickAsObservable().Subscribe(async delegate
		{
			ResidentFlow.GetInstance().GetTimer().SetPause(pause: true);
			m_yesButton.enabled = false;
			m_noButton.enabled = false;
			m_enableForceTimeOutExit = false;
			SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
			m_gachaSeriesInfo.ConsumeExchangePoint(m_requiredExchangePoint);
			bool flag = (m_gachaSeriesInfo.SeriesType != GachaSeriesType.Pickup && m_gachaSeriesInfo.SeriesType != GachaSeriesType.Limited) || GameDataManager.IsCharaSelectable(CutinItemMaster.GetItem(m_exchangeItemID).CharaType) || GameDataManager.PlayerConfig.UnableCharaGachaState == UnableCharaGachaSelectWindow.State.Get;
			bool isNew;
			if (!GameDataManager.Customize.CutinItems.Exists((CutinItemInfo x) => x.ItemID == m_exchangeItemID) && flag)
			{
				GameDataManager.Customize.CutinItems.Add(new CutinItemInfo
				{
					ItemID = m_exchangeItemID,
					IsNew = true,
					IsOn = true
				});
				isNew = true;
				MFG.Types.Customize.MatchingBackground.Item matchinBGItem = MatchingBackgroundMaster.GetGachaCutinItems().Find((MFG.Types.Customize.MatchingBackground.Item x) => x.UnlockingCutinID == m_exchangeItemID);
				if (matchinBGItem != null && !GameDataManager.Customize.MatchingBGItems.Exists((MatchingBGInfo x) => x.ItemID == matchinBGItem.ItemID) && !GameDataManager.Player.IsPro())
				{
					GameDataManager.Customize.MatchingBGItems.Add(new MatchingBGInfo
					{
						ItemID = matchinBGItem.ItemID
					});
				}
			}
			else
			{
				GameDataManager.PlayerItem.Add(MFG.Types.PlayerItem.ItemID.Present01, 1);
				isNew = false;
			}
			try
			{
				CharaType getCharaType = (CharaType)(-1);
				if (GachaUtility.CharaUnlockItemIDDict.ContainsValue(m_exchangeItemID))
				{
					CharaType chara = GachaUtility.GetGachaUnlockCharaType(m_exchangeItemID);
					if (!GameDataManager.Player.GachaCharaUsableList.Exists((CharaType x) => x == chara))
					{
						GameDataManager.Player.GachaCharaUsableList.Add(chara);
						GameDataManager.Notice.InfoNewGirlNoticeFromGacha.Clear();
						GameDataManager.Notice.InfoNewGirlNoticeFromGacha.Add(chara);
						getCharaType = chara;
					}
				}
				await GameDataManager.Gacha.Save();
				await SendCardConnect(m_exchangeItemID);
				await GameDataManager.Customize.Save();
				await GameDataManager.Player.Save();
				await GameDataManager.PlayerItem.Save();
				await GameDataManager.PlayerPaidItem.Save();
				if (getCharaType >= CharaType.Chara12)
				{
					CharaEnabledSend.ResultData resultData = await RequestApiBase<CharaEnabledSend, CharaEnabledSend.ResultData, CharaType>.Send(getCharaType);
					if (resultData.IsGachaEnable)
					{
						GameDataManager.Notice.PickupGachaNoticeCharaType = getCharaType;
						GameDataManager.Notice.PickupGachaNoticeDate = resultData.EndDateString;
					}
				}
			}
			catch (NetworkResponseException ex)
			{
				GameDataManager.PlayerConfig.IsDataSaveFailed = true;
				Debug.Log("NetworkResponseException Catched. " + ex.Message);
			}
			await m_resultWindow.Open(gachaSeriesInfo, m_exchangeItemID, isNew);
			if (GameDataManager.PlayerConfig.IsMemorialPrintNotice)
			{
				await m_cardConnectWindow.DispMemorialPrintNotice();
			}
			else
			{
				GameDataManager.Notice.MemorialPrintNoticeInfos.Clear();
			}
			await m_scene.CharaGetNotice.GachaGetCharaNotice(cancellationToken);
			ResidentFlow.GetInstance().GetTimer().SetPause(pause: false);
			m_enableForceTimeOutExit = true;
			ExecuteExit(callSe: false);
		}).AddTo(this);
		m_isInitialized = true;
	}

	private async UniTask SendCardConnect(MFG.Types.Customize.CutinItem.ItemID id)
	{
		CardConnectGameData cardConnect = GameDataManager.CardConnect;
		MFG.Types.Customize.CutinItem.Item item = CutinItemMaster.GetItem(id);
		if (item != null && item.Rarity >= MFG.Types.Customize.CutinItem.Rarity.SR)
		{
			cardConnect.m_ccInfos.Add(new CardConnectInfo(cardConnect.now_idx, "MemorialCard", item.ObjectID, item.ShortName));
			cardConnect.now_idx++;
			await cardConnect.Save();
			MemorialPrintNoticeInfo item2 = new MemorialPrintNoticeInfo
			{
				ItemID = id
			};
			GameDataManager.Notice.MemorialPrintNoticeInfos.Add(item2);
		}
	}

	public async UniTask Open(GachaSeriesServerInfo gachaSeriesInfo, MFG.Types.Customize.CutinItem.ItemID exchangeItemID, CancellationToken cancellationToken = default(CancellationToken))
	{
		SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
		m_isOpen = true;
		m_yesButton.enabled = true;
		m_noButton.enabled = true;
		m_raycastMaskPanel?.SetActive(value: true);
		MFG.Types.Customize.CutinItem.Item item = CutinItemMaster.GetItem(exchangeItemID);
		CautionActive(exchangeItemID, item);
		await Init(gachaSeriesInfo, exchangeItemID, cancellationToken);
		base.gameObject.GetComponent<Animator>().SetTrigger("In");
	}

	public void CautionActive(MFG.Types.Customize.CutinItem.ItemID exchangeItemID, MFG.Types.Customize.CutinItem.Item item)
	{
		if (GameDataManager.Customize.CutinItems.Exists((CutinItemInfo x) => x.ItemID == exchangeItemID) && (item.Rarity == MFG.Types.Customize.CutinItem.Rarity.UR || item.Rarity == MFG.Types.Customize.CutinItem.Rarity.SR))
		{
			m_caution.SetActive(value: true);
			m_caution2.SetActive(value: false);
			m_caution3.SetActive(value: false);
		}
		else if (!GachaUtility.CharaUnlockItemIDDict.ContainsValue(item.ItemID) && GameDataManager.PlayerConfig.UnableCharaGachaState != 0 && !GameDataManager.IsCharaSelectable(item.CharaType))
		{
			m_caution.SetActive(value: false);
			m_caution2.SetActive(GameDataManager.PlayerConfig.UnableCharaGachaState == UnableCharaGachaSelectWindow.State.Get);
			m_caution3.SetActive(GameDataManager.PlayerConfig.UnableCharaGachaState == UnableCharaGachaSelectWindow.State.Gift);
		}
		else
		{
			m_caution.SetActive(value: false);
			m_caution2.SetActive(value: false);
			m_caution3.SetActive(value: false);
		}
	}
}
