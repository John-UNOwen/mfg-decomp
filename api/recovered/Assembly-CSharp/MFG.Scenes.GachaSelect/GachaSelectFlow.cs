using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameContents;
using KAMUNITY;
using Konami.System.Net;
using MFG.GameData;
using MFG.GameRequest;
using MFG.Scenes.CommonSystem;
using MFG.Scenes.GachaSelect.CutinItemExchange;
using MFG.Scenes.GachaSelect.SupplyItem;
using MFG.Sytem.Utilities;
using MFG.Types;
using MFG.Types.PlayerItem;
using TMPro;
using TestModeSkeleton;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace MFG.Scenes.GachaSelect;

public class GachaSelectFlow : BaseFlow
{
	[SerializeField]
	private GameObject m_tenkeyWindow;

	[SerializeField]
	private Button m_ticketButton;

	[SerializeField]
	private Button m_ticketMultipleButton;

	[SerializeField]
	private Button m_singleButton;

	[SerializeField]
	private Button m_multipleButton;

	[SerializeField]
	private Button m_musicSingleButton;

	[SerializeField]
	private Button m_exitButton;

	[SerializeField]
	private Button m_gachaItemListButton;

	[SerializeField]
	private Button m_exchangeItemListButton;

	[SerializeField]
	private Button m_musicItemListButton;

	[SerializeField]
	private List<GameObject> m_musicGachaUnableObjects;

	[SerializeField]
	private List<GameObject> m_musicGachaEnableObjects;

	[SerializeField]
	private GachaInfoDialog m_detailDialog;

	[SerializeField]
	private SupplyItemListWindow m_supplyItemListDialog;

	[SerializeField]
	private SupplyPickupItemListWindow m_supplyPickUpItemListDialog;

	[SerializeField]
	private SupplyPickupItemListWindow m_supplyUnlockItemListDialog;

	[SerializeField]
	private SupplyPickupItemListWindow m_supplyMusicItemListDialog;

	[SerializeField]
	private GachaVoiceListWindow m_supplyMusicItemWindow;

	[SerializeField]
	private SelectWindow m_exchangeItemSelectWindow;

	[SerializeField]
	private TextMeshProUGUI m_exchangeText;

	[SerializeField]
	private TextMeshProUGUI m_exchangePointText;

	[SerializeField]
	private Canvas m_canvas;

	[SerializeField]
	private Animator m_seriesChangeAnimatorOfButton;

	[SerializeField]
	private TextMeshProUGUI m_gachaTicketCountText;

	[SerializeField]
	private GameObject m_gachaTicketCounter;

	[SerializeField]
	private GachaTicketDetailWindow m_gachaTicketDetailWindow;

	[SerializeField]
	private GameObject m_creditFrameOnce;

	[SerializeField]
	private TextMeshProUGUI m_creditTextOnce;

	[SerializeField]
	private GameObject m_paseliFrameOnce;

	[SerializeField]
	private TextMeshProUGUI m_paseliTextOnce;

	[SerializeField]
	private GameObject _compensationFrameOnce;

	[SerializeField]
	private GameObject m_musicCreditFrameOnce;

	[SerializeField]
	private TextMeshProUGUI m_musicCreditTextOnce;

	[SerializeField]
	private GameObject m_musicPaseliFrameOnce;

	[SerializeField]
	private TextMeshProUGUI m_musicPaseliTextOnce;

	[SerializeField]
	private GameObject _compensationFrameMusicOnce;

	[SerializeField]
	private GameObject m_creditFrameFiveTimes;

	[SerializeField]
	private TextMeshProUGUI m_creditTextFiveTimes;

	[SerializeField]
	private GameObject m_paseliFrameFiveTimes;

	[SerializeField]
	private TextMeshProUGUI m_paseliTextFiveTimes;

	[SerializeField]
	private GameObject _compensationFrameFiveTimes;

	[SerializeField]
	private GachaSeriesCarouselBanner m_carouselBanner;

	[SerializeField]
	private GameObject m_unableText;

	[SerializeField]
	private GameObject m_charaEnableText;

	[SerializeField]
	private GameObject m_charaUnableText;

	[SerializeField]
	private UnableCharaGachaSelectWindow m_unableCharaGachaSelectWindow;

	[SerializeField]
	private Button m_btUnableCharaGachaSelectGet;

	[SerializeField]
	private Button m_btUnableCharaGachaSelectGift;

	[SerializeField]
	private TokushohoWindow m_tokushohoWindow;

	[SerializeField]
	private Button m_btTokushoho;

	private GachaModePriceDisp m_onceGachaPriceDisp = new GachaModePriceDisp();

	private GachaModePriceDisp m_musicOnceGachaPriceDisp = new GachaModePriceDisp();

	private GachaModePriceDisp m_fiveTimesGachaPriceDisp = new GachaModePriceDisp();

	private List<GachaSeriesServerInfo> m_gachaSeriesServerInfos;

	private GachaSeriesServerInfo m_currentGachaSeriesInfo;

	[SerializeField]
	private GameObject m_topInfo;

	[SerializeField]
	private Button m_topInfoSkip;

	[SerializeField]
	private Button m_topInfoAllSkip;

	[SerializeField]
	private CardConnectWindow m_cardConnectNotice;

	public GachaCharaGetAnimation CharaGetNotice;

	[SerializeField]
	private Button m_gachaCheckButton;

	[SerializeField]
	private Button m_memorialNoticeButtonToOn;

	[SerializeField]
	private Button m_memorialNoticeButtonToOff;

	[SerializeField]
	private MusicGachaPlay m_musicGachaPlay;

	[SerializeField]
	private GameObject m_musicGachaCounter;

	[SerializeField]
	private TextMeshProUGUI m_musicGachaSupllyCounterText;

	[SerializeField]
	private GameObject m_musicGachaCompAnnounce;

	[SerializeField]
	private GameObject m_musicGachaMissionAnnounce;

	[SerializeField]
	private GachaSelectBgMovie m_bgMovie;

	[SerializeField]
	private CharaPickupGachaNoticeWindow m_charaPickupGachaNoticeWindow;

	private int m_defaultTime = 60;

	private bool m_decidedNextScene;

	private bool m_enableForceTimeOutExit;

	private bool isGachaRequestSuccess;

	private ulong gachaRequestID;

	private bool isMusicGachaEnable;

	private float m_prevLeftTime;

	private bool isAllButtonEnable;

	private bool IsMusicGacha => GameDataManager.Gacha.EntryGachaSelectSceneMode == GachaSeriesType.Music;

	protected override async UniTask OnInitialize(CancellationToken cancellationToken)
	{
		AllButton_Enable(isEnable: false);
		m_carouselBanner.TouchEnable = false;
		try
		{
			await PaseliUtility.RefleshPrice(Fraction.One, isFetchExternalBasicRateInfo: true);
		}
		catch
		{
			Debug.Log("価格情報の更新に失敗");
			GotoNextScene(GetExitSceneName());
			return;
		}
		try
		{
			GachaInfoRequest.ResultData resultData = await RequestApiBase<GachaInfoRequest, GachaInfoRequest.ResultData>.Send(enableErrorScreenTransition: false, enableSendLog: true, cancellationToken);
			GameDataManager.GachaInfo.GachaSeriesInfosFromServer = resultData.ResultGachaSeriesInfosFromServer;
			GameDataManager.Gacha.Validate();
		}
		catch
		{
		}
		if (!(await GameDataManager.GachaInfo.Validate(GameDataManager.Gacha.EntryGachaSelectSceneMode)))
		{
			GotoNextScene(GetExitSceneName());
			return;
		}
		m_gachaSeriesServerInfos = new List<GachaSeriesServerInfo>(GameDataManager.GachaInfo.ModeGachaInfo(GameDataManager.Gacha.EntryGachaSelectSceneMode));
		switch (GameDataManager.Gacha.EntryGachaSelectSceneMode)
		{
		case GachaSeriesType.Unlock:
			ResidentFlow.GetInstance().GetHeader().SetSceneName("SPガチャ");
			m_gachaSeriesServerInfos = (from a in m_gachaSeriesServerInfos
				orderby !a.IsPickitemsAllGet() descending, DateTimeOffsetManager.GetJSTFromString(a.StartDate) descending, a.SeriesID descending
				select a).ToList();
			break;
		case GachaSeriesType.Music:
			ResidentFlow.GetInstance().GetHeader().SetSceneName("リーチソング＆ボイスガチャ");
			m_gachaSeriesServerInfos = (from a in m_gachaSeriesServerInfos
				orderby !GameDataManager.MusicGacha.IsAllItemPossession(a.SeriesID) descending, DateTimeOffsetManager.GetJSTFromString(a.StartDate) descending, a.SeriesID
				select a).ToList();
			m_memorialNoticeButtonToOff.gameObject.SetActive(value: false);
			m_memorialNoticeButtonToOn.gameObject.SetActive(value: false);
			break;
		default:
			ResidentFlow.GetInstance().GetHeader().SetSceneName("ガールズカットインガチャ");
			m_gachaSeriesServerInfos = (from a in m_gachaSeriesServerInfos
				orderby DateTimeOffsetManager.GetJSTFromString(a.StartDate) descending, !a.ExtraMaster.IsReturens descending, a.SeriesType == GachaSeriesType.Pickup descending, a.SeriesID == 0 descending, a.SeriesID descending
				select a).ToList();
			break;
		}
		m_currentGachaSeriesInfo = m_gachaSeriesServerInfos?.Find((GachaSeriesServerInfo x) => x.SeriesID == (GameDataManager.Gacha.SelectedSeriesInfo?.SeriesID ?? (-1))) ?? m_gachaSeriesServerInfos[0];
		if (GameDataManager.Gacha.EntryGachaSelectSceneMode == GachaSeriesType.Music)
		{
			SoundManager.PlayMusicGachaBGM(m_currentGachaSeriesInfo).Forget();
		}
		else
		{
			SoundManager.PlayMyCharaMyRoomBGM().Forget();
		}
		int num = m_gachaSeriesServerInfos.Select((GachaSeriesServerInfo a) => a.SeriesID).ToList().IndexOf(m_currentGachaSeriesInfo.SeriesID);
		if (num > 0)
		{
			List<GachaSeriesServerInfo> range = m_gachaSeriesServerInfos.GetRange(0, num);
			m_gachaSeriesServerInfos.AddRange(range);
			m_gachaSeriesServerInfos.RemoveRange(0, num);
		}
		switch (GameDataManager.Gacha.EntryGachaSelectSceneMode)
		{
		case GachaSeriesType.Unlock:
			GachaPlayButton(isTicket: false, isMusic: false);
			break;
		case GachaSeriesType.Music:
			GachaPlayButton(isTicket: false, isMusic: true);
			break;
		default:
			GachaPlayButton(m_currentGachaSeriesInfo.IsTicketGacha(), isMusic: false);
			break;
		}
		m_unableText.transform.Find("UnableText").GetComponent<TextMeshProUGUI>().text = m_currentGachaSeriesInfo.UnableReasonText();
		m_unableText.SetActive(!m_currentGachaSeriesInfo.IsGachaEnableFromChara());
		IsAllPickupItemsGetDisp();
		m_bgMovie.Init(m_currentGachaSeriesInfo);
		ResidentFlow.GetInstance().GetHeader().SetDisp(disp: true);
		ResidentFlow.GetInstance().GetHeader().SetStatusDisp(disp: false);
		m_enableForceTimeOutExit = true;
		m_onceGachaPriceDisp.Set(GAME_MODE.GACHA, m_creditFrameOnce, m_paseliFrameOnce, _compensationFrameOnce, m_creditTextOnce, m_paseliTextOnce);
		m_fiveTimesGachaPriceDisp.Set(GAME_MODE.GACHA5, m_creditFrameFiveTimes, m_paseliFrameFiveTimes, _compensationFrameFiveTimes, m_creditTextFiveTimes, m_paseliTextFiveTimes);
		m_ticketButton.transform.Find("NeedTicket").Find("NeedNum").GetComponent<TextMeshProUGUI>()
			.text = $"{GachaGameData.GetRequiredGachaTicket()}";
		m_ticketMultipleButton.transform.Find("NeedTicket").Find("NeedNum").GetComponent<TextMeshProUGUI>()
			.text = $"{GachaGameData.GetRequiredGachaTicket(5)}";
		PPEXPriceRefresh();
		m_gachaTicketCountText.text = $"{GameDataManager.PlayerItem.Count(ItemID.GachaTicket)}";
		m_exchangeText.text = m_currentGachaSeriesInfo.GetExchangePointLabel();
		m_exchangePointText.text = $"{m_currentGachaSeriesInfo.GetExchangePoint()}";
		MusicGachaSupplyCountRefresh();
		await m_exchangeItemSelectWindow.Init(m_currentGachaSeriesInfo);
		m_carouselBanner.OnBannerChangeBegin.Subscribe(async delegate((GachaSeriesServerInfo bannerInfo, bool isAutoMove) data)
		{
			AllButton_Enable(isEnable: false);
			if (!data.isAutoMove)
			{
				SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
			}
			m_currentGachaSeriesInfo = data.bannerInfo;
			GachaPlayButton(m_currentGachaSeriesInfo.SeriesID == 1, IsMusicGacha);
			m_seriesChangeAnimatorOfButton.SetTrigger("In");
			m_bgMovie.GachaSeriesChange(m_currentGachaSeriesInfo);
			m_musicGachaCounter.SetActive(value: false);
			if (GameDataManager.Gacha.EntryGachaSelectSceneMode == GachaSeriesType.Music)
			{
				SoundManager.PlayMusicGachaBGM(m_currentGachaSeriesInfo).Forget();
			}
		}).AddTo(this);
		m_carouselBanner.OnBannerChangeEnd.Subscribe(delegate
		{
			m_unableText.transform.Find("UnableText").GetComponent<TextMeshProUGUI>().text = m_currentGachaSeriesInfo.UnableReasonText();
			m_unableText.SetActive(!m_currentGachaSeriesInfo.IsGachaEnableFromChara());
			IsAllPickupItemsGetDisp();
			m_exchangeText.text = m_currentGachaSeriesInfo.GetExchangePointLabel();
			m_exchangePointText.text = $"{m_currentGachaSeriesInfo.GetExchangePoint()}";
			AllButton_Enable(isEnable: true, m_currentGachaSeriesInfo.IsGachaEnableFromChara());
			MusicGachaSupplyCountRefresh();
		}).AddTo(this);
		m_ticketButton.OnClickAsObservable().Subscribe(async delegate
		{
			AllButton_Enable(isEnable: false);
			if (!m_decidedNextScene)
			{
				m_prevLeftTime = ResidentFlow.GetInstance().GetTimer().GetLeftTime();
				SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
				if (await RequestGachaDraw(1, isTicket: true))
				{
					GameDataManager.Gacha.SelectedMode = GachaPlayMode.TicketOne;
					GameDataManager.Gacha.SelectedSeriesInfo = m_currentGachaSeriesInfo;
					GameDataManager.PlayerConfig.IsGachaSelectIn = false;
					GotoNextScene("GachaPlay");
				}
			}
		}).AddTo(this);
		m_ticketMultipleButton.OnClickAsObservable().Subscribe(async delegate
		{
			AllButton_Enable(isEnable: false);
			if (!m_decidedNextScene)
			{
				m_prevLeftTime = ResidentFlow.GetInstance().GetTimer().GetLeftTime();
				SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
				if (await RequestGachaDraw(5, isTicket: true))
				{
					GameDataManager.Gacha.SelectedMode = GachaPlayMode.TicketFive;
					GameDataManager.Gacha.SelectedSeriesInfo = m_currentGachaSeriesInfo;
					GameDataManager.PlayerConfig.IsGachaSelectIn = false;
					GotoNextScene("GachaPlay");
				}
			}
		}).AddTo(this);
		m_singleButton.OnClickAsObservable().Subscribe(async delegate
		{
			AllButton_Enable(isEnable: false);
			if (!m_decidedNextScene)
			{
				m_prevLeftTime = ResidentFlow.GetInstance().GetTimer().GetLeftTime();
				SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
				if (await RequestGachaDraw(1, isTicket: false))
				{
					if (GameUtility.IsKonastaOrNetcafeBehavior && (GameDataManager.Player.IsPro() || GameUtility.IsProCardId(GameDataManager.Account.CardID) || Cloudstrap.UserEvent.NeedCompensation(Cloudstrap.UserEventType.Gacha1)))
					{
						OnEnd(is_success: true, CONSUME_MODE.GACHA_CREDIT, ConsumeWindow.EndType.Success);
					}
					else
					{
						DialogManager.Instance.CreateConsumeDialog(m_canvas.transform).Set("", GAME_TYPE.GACHA, PRICE_TYPE.PRICE_1, 1, m_currentGachaSeriesInfo.GachaSeriesName, "1回ガチャ", OnEnd, m_tenkeyWindow, resetTimer: false).Forget();
					}
				}
			}
		}).AddTo(this);
		m_musicSingleButton.OnClickAsObservable().Subscribe(async delegate
		{
			AllButton_Enable(isEnable: false);
			if (!m_decidedNextScene)
			{
				m_prevLeftTime = ResidentFlow.GetInstance().GetTimer().GetLeftTime();
				SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
				try
				{
					MusicGachaPlayReserve.ResultData resultData2 = await RequestApiBase<MusicGachaPlayReserve, MusicGachaPlayReserve.ResultData, int>.Send(m_currentGachaSeriesInfo.SeriesID);
					isGachaRequestSuccess = resultData2.IsSuccess;
					if (!isGachaRequestSuccess)
					{
						AllButton_Enable(isEnable: true, m_currentGachaSeriesInfo.IsGachaEnableFromChara());
						return;
					}
					gachaRequestID = resultData2.RequestID;
				}
				catch (NetworkResponseException)
				{
					ErrorData.GetInstance().ExtraMessage = "「リーチソング＆ボイスガチャ」が実行できませんでした。";
					ErrorControl.SetError(ERROR.CONNECTION, 0u, ERROR_LEVEL.ERROR_STOP);
				}
				if (GameUtility.IsKonastaOrNetcafeBehavior && (GameDataManager.Player.IsPro() || GameUtility.IsProCardId(GameDataManager.Account.CardID) || (IsMusicGacha && Cloudstrap.UserEvent.NeedCompensation(Cloudstrap.UserEventType.MusicGacha))))
				{
					OnEnd(is_success: true, CONSUME_MODE.MUSIC_GACHA_CREDIT, ConsumeWindow.EndType.Success);
				}
				else
				{
					DialogManager.Instance.CreateConsumeDialog(m_canvas.transform).Set("", GAME_TYPE.MUSICGACHA, PRICE_TYPE.PRICE_1, 1, m_currentGachaSeriesInfo.GachaSeriesName, "1回ガチャ", OnEnd, m_tenkeyWindow, resetTimer: false).Forget();
				}
			}
		}).AddTo(this);
		m_multipleButton.OnClickAsObservable().Subscribe(async delegate
		{
			AllButton_Enable(isEnable: false);
			if (!m_decidedNextScene)
			{
				m_prevLeftTime = ResidentFlow.GetInstance().GetTimer().GetLeftTime();
				SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
				if (await RequestGachaDraw(5, isTicket: false))
				{
					if (GameUtility.IsKonastaOrNetcafeBehavior && (GameDataManager.Player.IsPro() || GameUtility.IsProCardId(GameDataManager.Account.CardID) || (!IsMusicGacha && Cloudstrap.UserEvent.NeedCompensation(Cloudstrap.UserEventType.Gacha5))))
					{
						OnEnd(is_success: true, CONSUME_MODE.GACHA5_CREDIT, ConsumeWindow.EndType.Success);
					}
					else
					{
						DialogManager.Instance.CreateConsumeDialog(m_canvas.transform).Set("", GAME_TYPE.GACHA5, PRICE_TYPE.PRICE_1, 1, m_currentGachaSeriesInfo.GachaSeriesName, "5回ガチャ", OnEnd, m_tenkeyWindow, resetTimer: false).Forget();
					}
				}
			}
		}).AddTo(this);
		m_gachaItemListButton.OnClickAsObservable().Subscribe(delegate
		{
			if (m_currentGachaSeriesInfo.SeriesType == GachaSeriesType.Pickup)
			{
				m_supplyPickUpItemListDialog.Open(m_currentGachaSeriesInfo);
			}
			else if (m_currentGachaSeriesInfo.SeriesType == GachaSeriesType.Unlock)
			{
				m_supplyUnlockItemListDialog.Open(m_currentGachaSeriesInfo);
			}
			else if (m_currentGachaSeriesInfo.SeriesType == GachaSeriesType.Music)
			{
				m_supplyMusicItemListDialog.Open(m_currentGachaSeriesInfo);
			}
			else
			{
				m_supplyItemListDialog.Open(m_currentGachaSeriesInfo);
			}
		}).AddTo(this);
		m_exchangeItemListButton.OnClickAsObservable().Subscribe(async delegate
		{
			AllButton_Enable(isEnable: false);
			GameDataManager.Notice.MemorialPrintNoticeInfos.Clear();
			await m_exchangeItemSelectWindow.Open(m_currentGachaSeriesInfo);
			try
			{
				await UniTask.WaitWhile(() => m_exchangeItemSelectWindow.IsOpen, PlayerLoopTiming.Update, cancellationToken);
				m_exchangeText.text = m_currentGachaSeriesInfo.GetExchangePointLabel();
				m_exchangePointText.text = $"{m_currentGachaSeriesInfo.GetExchangePoint()}";
				ResidentFlow.GetInstance().GetTimer().SetPause(pause: true);
				await CharaPickupGachaOpenNotice();
				ResidentFlow.GetInstance().GetTimer().SetPause(pause: false);
				IsAllPickupItemsGetDisp();
				AllButton_Enable(isEnable: true, m_currentGachaSeriesInfo.IsGachaEnableFromChara());
			}
			catch
			{
			}
		}).AddTo(this);
		m_memorialNoticeButtonToOn.OnClickAsObservable().Subscribe(async delegate
		{
			SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
			m_memorialNoticeButtonToOn.gameObject.SetActive(value: false);
			m_memorialNoticeButtonToOff.gameObject.SetActive(value: true);
			GameDataManager.PlayerConfig.IsMemorialPrintNotice = true;
		}).AddTo(this);
		m_memorialNoticeButtonToOff.OnClickAsObservable().Subscribe(async delegate
		{
			SoundManager.PlaySe("SFX_SYS_CANCEL2").Forget();
			m_memorialNoticeButtonToOff.gameObject.SetActive(value: false);
			m_memorialNoticeButtonToOn.gameObject.SetActive(value: true);
			GameDataManager.PlayerConfig.IsMemorialPrintNotice = false;
		}).AddTo(this);
		m_musicItemListButton.OnClickAsObservable().Subscribe(delegate
		{
			m_supplyMusicItemWindow.Open(m_currentGachaSeriesInfo);
		}).AddTo(this);
		BtUnableCharaGachaSelectGetActive();
		m_btUnableCharaGachaSelectGet.OnClickAsObservable().Subscribe(async delegate
		{
			m_unableCharaGachaSelectWindow.OpenWindow();
			await UniTask.WaitWhile(() => m_unableCharaGachaSelectWindow.isOpen);
			BtUnableCharaGachaSelectGetActive();
		}).AddTo(this);
		m_btUnableCharaGachaSelectGift.OnClickAsObservable().Subscribe(async delegate
		{
			m_unableCharaGachaSelectWindow.OpenWindow();
			await UniTask.WaitWhile(() => m_unableCharaGachaSelectWindow.isOpen);
			BtUnableCharaGachaSelectGetActive();
		}).AddTo(this);
		m_btTokushoho.gameObject.SetActive(GameDataManager.Gacha.EntryGachaSelectSceneMode == GachaSeriesType.Music);
		m_btTokushoho.OnClickAsObservable().Subscribe(async delegate
		{
			m_tokushohoWindow.OpenWindow(isForceOpen: false);
			await UniTask.WaitWhile(() => m_tokushohoWindow.isOpen);
		}).AddTo(this);
		m_exitButton.OnClickAsObservable().Subscribe(delegate
		{
			if (!m_decidedNextScene)
			{
				ForceTimeOutExit();
			}
		}).AddTo(this);
		DeviceManager.Instance.Coin().OnTriggerIncome.Subscribe(delegate
		{
			Timer_Set(m_defaultTime);
		}).AddTo(this);
		await m_carouselBanner.Init(m_gachaSeriesServerInfos);
		if (SystemConfigGameData.IsDualMonitor)
		{
			SubMonitorFlow.GetInstance().TransitionAnimClose();
		}
		await ResidentFlow.GetInstance().TransitionEffectClose();
	}

	protected override async UniTask OnExecute(CancellationToken cancellationToken)
	{
		if (GameDataManager.PlayerConfig.IsMemorialPrintNotice)
		{
			await m_cardConnectNotice.DispMemorialPrintNotice();
		}
		else
		{
			GameDataManager.Notice.MemorialPrintNoticeInfos.Clear();
		}
		await CharaPickupGachaOpenNotice();
		IsAllPickupItemsGetDisp();
		if (GameDataManager.PlayerConfig.IsGachaSelectIn)
		{
			Timer_Set(GameDataManager.PlayerConfig.GachaSelectRemainTime);
		}
		else
		{
			GameDataManager.PlayerConfig.GachaSelectRemainTime = m_defaultTime;
			Timer_Set(m_defaultTime);
		}
		ResidentFlow.GetInstance().GetTimer().StartTimer();
		GameDataManager.PlayerConfig.IsGachaSelectIn = true;
		if (GameDataManager.Gacha.EntryGachaSelectSceneMode == GachaSeriesType.Pickup && GameDataManager.PlayerConfig.UnableCharaGachaState == UnableCharaGachaSelectWindow.State.None)
		{
			m_unableCharaGachaSelectWindow.OpenWindow();
			await UniTask.WaitWhile(() => m_unableCharaGachaSelectWindow.isOpen);
			BtUnableCharaGachaSelectGetActive();
		}
		if (GameDataManager.Gacha.EntryGachaSelectSceneMode == GachaSeriesType.Music && !GameDataManager.PlayerConfig.IsTokushohoAgreed)
		{
			m_tokushohoWindow.OpenWindow(isForceOpen: true);
			await UniTask.WaitWhile(() => m_tokushohoWindow.isOpen);
			if (!GameDataManager.PlayerConfig.IsTokushohoAgreed)
			{
				GotoNextScene(GetExitSceneName());
				return;
			}
		}
		if (!IsMusicGacha)
		{
			m_memorialNoticeButtonToOff.gameObject.SetActive(GameDataManager.PlayerConfig.IsMemorialPrintNotice);
			m_memorialNoticeButtonToOn.gameObject.SetActive(!GameDataManager.PlayerConfig.IsMemorialPrintNotice);
		}
		AllButton_Enable(isEnable: true, m_currentGachaSeriesInfo.IsGachaEnableFromChara());
		m_carouselBanner.TouchEnable = true;
	}

	protected override void OnUpdate()
	{
		if (m_enableForceTimeOutExit && ResidentFlow.GetInstance().GetTimer().IsEnd())
		{
			m_enableForceTimeOutExit = false;
			ForceTimeOutExit();
		}
	}

	private void ForceTimeOutExit()
	{
		SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
		GameDataManager.PlayerConfig.IsGachaSelectIn = false;
		string exitSceneName = GetExitSceneName();
		GotoNextScene(exitSceneName);
	}

	private string GetExitSceneName()
	{
		return "Home";
	}

	protected override void GotoNextScene(string sceneName, bool enableDecidedCheck)
	{
		if (!enableDecidedCheck || !_isNextSceneDecided)
		{
			AllButton_Enable(isEnable: false);
			_isNextSceneDecided = true;
			if (sceneName != GetExitSceneName() && !sceneName.Equals("GachaSelect"))
			{
				SoundManager.StopBgmWithFadeout().Forget();
			}
			LoadScene(sceneName);
		}
	}

	private void Timer_Set(float time)
	{
		ResidentFlow.GetInstance().GetTimer().SetTimer(time, disp: true);
	}

	public void Onclick_OpenDetailDialog()
	{
		m_detailDialog.Open();
	}

	private async void OnEnd(bool is_success, CONSUME_MODE mode, ConsumeWindow.EndType endType)
	{
		if (is_success)
		{
			GameDataManager.Gacha.SelectedSeriesInfo = m_currentGachaSeriesInfo;
			GameDataManager.Gacha.ConsumeMode = mode;
			switch (mode)
			{
			case CONSUME_MODE.GACHA_CREDIT:
			case CONSUME_MODE.GACHA_PASELI:
				CabinetManager.Book.AddPlayCount(BookKeepingSettings.Variable.Gacha);
				CabinetManager.Book.AddPlayCount(BookKeepingSettings.Variable.Gacha1);
				if (GameUtility.IsKonastaOrNetcafeBehavior)
				{
					Cloudstrap.UserEvent.Begin(Cloudstrap.UserEventType.Gacha1);
				}
				GameDataManager.Gacha.SelectedMode = GachaPlayMode.One;
				GameDataManager.PlayerConfig.IsGachaSelectIn = false;
				GotoNextScene("GachaPlay");
				break;
			case CONSUME_MODE.GACHA5_CREDIT:
			case CONSUME_MODE.GACHA5_PASELI:
				GameDataManager.Gacha.SelectedMode = GachaPlayMode.Five;
				CabinetManager.Book.AddPlayCount(BookKeepingSettings.Variable.Gacha, 5);
				CabinetManager.Book.AddPlayCount(BookKeepingSettings.Variable.Gacha5);
				if (GameUtility.IsKonastaOrNetcafeBehavior)
				{
					Cloudstrap.UserEvent.Begin(Cloudstrap.UserEventType.Gacha5);
				}
				GameDataManager.PlayerConfig.IsGachaSelectIn = false;
				GotoNextScene("GachaPlay");
				break;
			case CONSUME_MODE.GACHA_TICKET_EX:
				GameDataManager.Gacha.SelectedMode = GachaPlayMode.One;
				GameDataManager.PlayerConfig.IsGachaSelectIn = false;
				GotoNextScene("GachaPlay");
				break;
			case CONSUME_MODE.GACHA5_TICKET_EX:
				GameDataManager.Gacha.SelectedMode = GachaPlayMode.Five;
				GameDataManager.PlayerConfig.IsGachaSelectIn = false;
				GotoNextScene("GachaPlay");
				break;
			case CONSUME_MODE.MUSIC_GACHA_CREDIT:
			case CONSUME_MODE.MUSIC_GACHA_PASELI:
				Timer_Set(m_defaultTime);
				CabinetManager.Book.AddPlayCount(BookKeepingSettings.Variable.MusicGacha);
				if (GameUtility.IsKonastaOrNetcafeBehavior)
				{
					Cloudstrap.UserEvent.Begin(Cloudstrap.UserEventType.MusicGacha);
				}
				GameDataManager.Gacha.SelectedMode = GachaPlayMode.MusicOne;
				GameDataManager.PlayerConfig.IsGachaSelectIn = false;
				await m_musicGachaPlay.Open(m_currentGachaSeriesInfo, gachaRequestID);
				MusicGachaSupplyCountRefresh();
				IsAllPickupItemsGetDisp();
				PPEXPriceRefresh();
				AllButton_Enable(isEnable: true, m_currentGachaSeriesInfo.IsGachaEnableFromChara());
				break;
			case CONSUME_MODE.JANDRI_CREDIT:
			case CONSUME_MODE.JANDRI_PASELI:
			case CONSUME_MODE.PREMIUM_START_PASELI:
			case CONSUME_MODE.PREMIUM_CONTINUE_PASELI:
			case CONSUME_MODE.CONTINUE_FREE:
			case CONSUME_MODE.PREMIUM_START_CREDIT:
			case CONSUME_MODE.PREMIUM_CONTINUE_CREDIT:
				break;
			}
		}
		else
		{
			if (!m_decidedNextScene)
			{
				AllButton_Enable(isEnable: true, m_currentGachaSeriesInfo.IsGachaEnableFromChara());
			}
			ResidentFlow.GetInstance().GetTimer().SetDisp(disp: true);
			ResidentFlow.GetInstance().GetTimer().SetPause(pause: false);
		}
	}

	private async UniTask<bool> RequestGachaDraw(int playCount, bool isTicket)
	{
		try
		{
			CutinGachaPlayDraw.ResultData resultData = await RequestApiBase<CutinGachaPlayDraw, CutinGachaPlayDraw.ResultData, (int, int, bool)>.Send((m_currentGachaSeriesInfo.SeriesID, playCount, isTicket));
			isGachaRequestSuccess = resultData.GachaResult.IsSuccess;
			if (isGachaRequestSuccess)
			{
				GameDataManager.Gacha.GachaResult = new GachaResultInfo(resultData);
				return true;
			}
			AllButton_Enable(isEnable: true, m_currentGachaSeriesInfo.IsGachaEnableFromChara());
			return false;
		}
		catch (NetworkResponseException)
		{
			ErrorData.GetInstance().ExtraMessage = $"「{playCount}回ガチャ」が実行できませんでした。";
			ErrorControl.SetError(ERROR.CONNECTION, 0u, ERROR_LEVEL.ERROR_STOP);
		}
		return false;
	}

	public void BtUnableCharaGachaSelectGetActive()
	{
		m_btUnableCharaGachaSelectGet.gameObject.SetActive(GameDataManager.Gacha.EntryGachaSelectSceneMode == GachaSeriesType.Pickup && GameDataManager.PlayerConfig.UnableCharaGachaState == UnableCharaGachaSelectWindow.State.Get);
		m_btUnableCharaGachaSelectGift.gameObject.SetActive(GameDataManager.Gacha.EntryGachaSelectSceneMode == GachaSeriesType.Pickup && GameDataManager.PlayerConfig.UnableCharaGachaState == UnableCharaGachaSelectWindow.State.Gift);
		IsAllPickupItemsGetDisp();
	}

	public void IsAllPickupItemsGetDisp()
	{
		if (IsMusicGacha)
		{
			m_charaEnableText.transform.Find("UnableText").GetComponent<TextMeshProUGUI>().text = "コンプリート済";
			m_charaEnableText.SetActive(GameDataManager.MusicGacha.IsAllItemPossession(m_currentGachaSeriesInfo.SeriesID));
		}
		else
		{
			m_charaEnableText.transform.Find("UnableText").GetComponent<TextMeshProUGUI>().text = "ピックアップ対象所持済";
			m_charaEnableText.SetActive(m_currentGachaSeriesInfo.IsPickitemsAllGet());
		}
		try
		{
			if ((m_currentGachaSeriesInfo.SeriesType == GachaSeriesType.Pickup || m_currentGachaSeriesInfo.SeriesType == GachaSeriesType.Limited) && m_currentGachaSeriesInfo.IsGachaEnableFromChara() && !m_currentGachaSeriesInfo.IsGachaAllEnableFromChara())
			{
				m_charaUnableText.transform.Find("UnableText").GetComponent<TextMeshProUGUI>().text = ((GameDataManager.PlayerConfig.UnableCharaGachaState == UnableCharaGachaSelectWindow.State.Get) ? "未所持ファイトガールのカットインを\nそのまま受け取る設定になっています。" : "未所持ファイトガールのカットインを\nギフトに変換する設定になっています。");
				m_charaUnableText.SetActive(value: true);
				return;
			}
		}
		catch
		{
		}
		m_charaUnableText.SetActive(value: false);
	}

	private void AllButton_Enable(bool isEnable, bool isCharacterUsable = true)
	{
		isAllButtonEnable = isEnable;
		m_singleButton.interactable = isEnable && isCharacterUsable && !GameUtility.IsReservedGameOver && (Cloudstrap.UserEvent.NeedCompensation(Cloudstrap.UserEventType.Gacha1) || !Cloudstrap.UserEvent.NeedCompensation(Cloudstrap.UserEventType.Gacha5));
		m_multipleButton.interactable = isEnable && isCharacterUsable && !GameUtility.IsReservedGameOver && (Cloudstrap.UserEvent.NeedCompensation(Cloudstrap.UserEventType.Gacha5) || !Cloudstrap.UserEvent.NeedCompensation(Cloudstrap.UserEventType.Gacha1));
		m_ticketButton.interactable = isEnable && isCharacterUsable && GameDataManager.PlayerItem.Count(ItemID.GachaTicket) >= GachaGameData.GetRequiredGachaTicket();
		m_ticketMultipleButton.interactable = isEnable && isCharacterUsable && GameDataManager.PlayerItem.Count(ItemID.GachaTicket) >= GachaGameData.GetRequiredGachaTicket(5);
		m_musicSingleButton.interactable = isMusicGachaEnable && isEnable && isCharacterUsable && !GameUtility.IsReservedGameOver && !GameDataManager.MusicGacha.IsAllItemPossession(m_currentGachaSeriesInfo.SeriesID);
		if (m_singleButton.IsActive())
		{
			m_singleButton.animator.Update(0f);
		}
		if (m_multipleButton.IsActive())
		{
			m_multipleButton.animator.Update(0f);
		}
		if (m_musicSingleButton.IsActive())
		{
			m_musicSingleButton.animator.Update(0f);
		}
		if (m_ticketButton.IsActive())
		{
			m_ticketButton.animator.Update(0f);
		}
		if (m_ticketMultipleButton.IsActive())
		{
			m_ticketMultipleButton.animator.Update(0f);
		}
		m_musicItemListButton.interactable = isEnable;
		m_musicItemListButton.animator.Update(0f);
		m_gachaItemListButton.interactable = isEnable;
		m_gachaItemListButton.animator.Update(0f);
		m_exchangeItemListButton.interactable = isEnable;
		m_exchangeItemListButton.animator.Update(0f);
		m_exitButton.interactable = isEnable;
		m_exitButton.animator.Update(0f);
		m_memorialNoticeButtonToOff.interactable = isEnable;
		m_memorialNoticeButtonToOn.interactable = isEnable;
		m_btUnableCharaGachaSelectGet.interactable = isEnable;
		m_btUnableCharaGachaSelectGift.interactable = isEnable;
		m_btTokushoho.interactable = isEnable;
		m_btTokushoho.interactable = isEnable;
	}

	private void GachaPlayButton(bool isTicket, bool isMusic)
	{
		m_singleButton.gameObject.SetActive(!isTicket && !isMusic);
		m_multipleButton.gameObject.SetActive(!isTicket && !isMusic);
		m_musicSingleButton.gameObject.SetActive(isMusic);
		m_gachaTicketCounter.SetActive(isTicket);
		m_ticketButton.gameObject.SetActive(isTicket);
		m_ticketMultipleButton.gameObject.SetActive(isTicket);
		m_musicItemListButton.gameObject.SetActive(isMusic);
		foreach (GameObject musicGachaUnableObject in m_musicGachaUnableObjects)
		{
			musicGachaUnableObject.SetActive(!isMusic);
		}
		foreach (GameObject musicGachaEnableObject in m_musicGachaEnableObjects)
		{
			musicGachaEnableObject.SetActive(isMusic);
		}
	}

	private void PPEXPriceRefresh()
	{
		if (PaseliUtility.PPEX.IsEnablePurchaseItem)
		{
			List<AvsEa3CoinTypes.ea3_eacoin_pricelist_article> list = PaseliUtility.PPEX.RefleshPrice();
			string musicGachaID = EaCoin.GetPathData(GAME_MODE.MUSICGACHA).ppexArticleID;
			bool flag = list.Exists((AvsEa3CoinTypes.ea3_eacoin_pricelist_article x) => x.atid == musicGachaID);
			isMusicGachaEnable = flag && !GameUtility.IsReservedGameOver;
		}
		else if (GameUtility.IsKonastaOrNetcafeBehavior)
		{
			isMusicGachaEnable = !GameUtility.IsReservedGameOver;
		}
		else
		{
			isMusicGachaEnable = false;
		}
		if (isMusicGachaEnable)
		{
			m_musicOnceGachaPriceDisp.Set(GAME_MODE.MUSICGACHA, m_musicCreditFrameOnce, m_musicPaseliFrameOnce, _compensationFrameMusicOnce, m_musicCreditTextOnce, m_musicPaseliTextOnce);
			return;
		}
		m_musicCreditFrameOnce.SetActive(value: false);
		m_musicPaseliFrameOnce.SetActive(value: false);
		_compensationFrameMusicOnce.SetActive(value: false);
	}

	private void MusicGachaSupplyCountRefresh()
	{
		MusicGachaInfo musicGachaInfo = GameDataManager.MusicGacha.MusicGachaInfos.Find((MusicGachaInfo x) => x.GachaID == m_currentGachaSeriesInfo.SeriesID);
		if (IsMusicGacha && musicGachaInfo != null)
		{
			m_musicGachaCounter.SetActive(value: true);
			m_musicGachaSupllyCounterText.text = $"{musicGachaInfo.PossessionCount} / {musicGachaInfo.GachaSupplyCount}";
			m_musicGachaCompAnnounce.SetActive(!musicGachaInfo.IsComplete && musicGachaInfo.GachaID <= 92);
			m_musicGachaMissionAnnounce.SetActive(musicGachaInfo.PossessionCount < 20 && musicGachaInfo.GachaID > 92);
		}
		else
		{
			m_musicGachaCounter.SetActive(value: false);
		}
	}

	public async UniTask CharaPickupGachaOpenNotice()
	{
		if (GameDataManager.Notice.PickupGachaNoticeCharaType >= CharaType.Chara12)
		{
			m_charaPickupGachaNoticeWindow.OpenWindow(this);
			await UniTask.WhenAny(UniTask.WaitWhile(() => m_charaPickupGachaNoticeWindow.isOpen), UniTask.Delay(TimeSpan.FromSeconds(20.0)));
			await m_charaPickupGachaNoticeWindow.CloseWindow();
		}
	}

	public void GoToPickupGacha()
	{
		GameDataManager.PlayerConfig.IsCutinGachaSceneVisited = true;
		GameDataManager.Gacha.EntryGachaSelectSceneMode = GachaSeriesType.Pickup;
		GotoNextScene("GachaSelect");
	}
}
