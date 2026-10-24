using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Cysharp.Threading.Tasks;
using Konami.System.Net;
using MFG.Assets;
using MFG.GameData;
using MFG.GameData.EventTaku;
using MFG.GameRequest;
using MFG.MasterGameData;
using MFG.Scenes.CommonSystem;
using MFG.Scenes.Home;
using MFG.Types;
using MFG.Types.Competition;
using MFG.Types.EventTakuMaster;
using MFG.Types.FightGirlNews;
using MFG.Types.Mission;
using MFG.Types.OneShotNotice;
using MFG.Types.OnlyPeraichiNotice;
using MFG.Types.ScrambleFestival;
using MFG.Types.ShopCompetition;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class HomeFlow : BaseFlow
{
	public enum TaikyokuModeSelectStatus
	{
		None,
		SelectingNormal,
		SelectingEvent
	}

	[SerializeField]
	public Canvas _canvas;

	[SerializeField]
	private Button m_taikyokuButton;

	[SerializeField]
	private Button m_eventTaikyokuButton;

	[SerializeField]
	private Button m_charaSelectButton;

	[SerializeField]
	private Button m_gachaButton;

	[SerializeField]
	private GameObject m_guestUnableMessage;

	[SerializeField]
	private Button m_dojoButton;

	[SerializeField]
	private Button m_missionButton;

	[SerializeField]
	private Button m_noticeButton;

	[SerializeField]
	private Button m_presentButton;

	[SerializeField]
	private Button m_gameOptionButton;

	[SerializeField]
	private Button m_eventTaikyokuScheduleButton;

	[SerializeField]
	private Button m_tsumikomiButton;

	[SerializeField]
	private Button m_tsumikomi2Button;

	[SerializeField]
	private Button m_cutinRecordButton;

	[SerializeField]
	private Button m_gameDataTestButton;

	[SerializeField]
	private Button m_soundTestButton;

	[SerializeField]
	private Button m_competitionButton;

	[SerializeField]
	private TextMeshProUGUI m_competitionButtonStatusText;

	[SerializeField]
	private Button m_debutNoticeButton;

	[SerializeField]
	private Button m_debutNoticeCloseButton;

	[SerializeField]
	private MissionWindow m_missionWindow;

	[SerializeField]
	private HomeNoticeWindow m_noticeWindow;

	[SerializeField]
	private HomePresentBoxWindow m_presentBoxWindow;

	[SerializeField]
	private HomeEventTaikyokuScheduleWindow m_eventTaikyokuScheduleWindow;

	[SerializeField]
	private GameObject m_eventButtonParent;

	[SerializeField]
	private GameObject m_btButtonPrefab;

	[SerializeField]
	private GameObject m_eventMask;

	[SerializeField]
	private GameObject m_eventParent;

	[SerializeField]
	private GameObject m_debutNoticeWindow;

	private BingoWindow m_bingoWindow;

	private TenshiGowanWindow m_tenshiGowanWindow;

	private CompetitionEntryWindow m_competitionEntryWindow;

	private CompetitionScoreWindow m_competitionScoreWindow;

	private ShopCompetitionHomeWindow m_shopCmpetitionHomeWindow;

	private YellWindow m_yellWindow;

	private Dictionary<BaseEventGameData, BaseEventTakuHomeWindow> _eventTakus = new Dictionary<BaseEventGameData, BaseEventTakuHomeWindow>();

	private CharaStampEventWindow m_charaStampEventWindow;

	private Dictionary<CategoryItem, MissionEventWindow> _missionEventWindows = new Dictionary<CategoryItem, MissionEventWindow>();

	private Dictionary<MFG.Types.OneShotNotice.Item, OneShotNoticeWindow> _oneShotNoticeWindows = new Dictionary<MFG.Types.OneShotNotice.Item, OneShotNoticeWindow>();

	private Dictionary<MFG.Types.OnlyPeraichiNotice.Item, OnlyPeraichiNoticeWindow> _onlyPeraichiNoticeWindows = new Dictionary<MFG.Types.OnlyPeraichiNotice.Item, OnlyPeraichiNoticeWindow>();

	private FightGirlNewsNoticeWindow m_fightGirlNewsNoticeWindow;

	[SerializeField]
	private Button m_fightGirlNewsNoticeButton;

	private FightGirlNewsNoticeWindow m_clearAppearanceEventNoticeWindow;

	[SerializeField]
	private HomeEventSelectWindow m_modeSelectWindow;

	[SerializeField]
	private HomeGachaModeSelectWindow m_gachaModeSelectWindow;

	[SerializeField]
	private GameObject m_charaInfo;

	[SerializeField]
	private CatchphrasePlate _catchphrasePlate;

	[SerializeField]
	private StandingCharaManager m_charaManager;

	[SerializeField]
	private GameObject m_topInfo;

	[SerializeField]
	private NoticeCarouselBanner m_carouselBanner;

	[SerializeField]
	private GameOption m_gameOption;

	[SerializeField]
	private MissionWindowAcquisition m_missionWindowAcquisition;

	[SerializeField]
	private GameObject m_proStayBanner;

	[SerializeField]
	private GameObject m_eventModeSelectProStayBanner;

	[SerializeField]
	private GameObject m_tsumikomiSet;

	[SerializeField]
	private GameObject m_tsumikomi2Set;

	[SerializeField]
	private GameObject m_cutinRecordSet;

	[SerializeField]
	private GameObject m_gameDataTestSet;

	[SerializeField]
	private GameObject m_soundTestSet;

	[SerializeField]
	private HomeFightGirlNews m_homeFightGirlNews;

	[SerializeField]
	private CharaPickupGachaNoticeWindow m_charaPickupGachaNoticeWindow;

	[SerializeField]
	private GameObject m_debugScoreText;

	private TaikyokuModeSelectStatus _taikyokuModeSelectStatus;

	private VoteHomeWindow m_voteHomeWindow;

	private ScrambleHomeWindow m_scrambleHomeWindow;

	private ScrambleTeamMatchHomeWindow m_scrambleTeamMatchHomeWindow;

	private GameObject m_scrambleLotteryHomeWindow;

	private ImportantNoticeWindow m_importantNoticeWindow;

	private OmikujiHomeWindow m_omikujiHomeWindow;

	[SerializeField]
	private PrivateMatchTenkeyWindow m_privateMatchWindow;

	[SerializeField]
	private GameObject m_privateMatchButtonParent;

	[SerializeField]
	private Button m_privateMatchButtonToOn;

	[SerializeField]
	private Button m_privateMatchButtonToOff;

	[SerializeField]
	private GameObject m_privateMatchButtonAttention;

	[SerializeField]
	private GameObject m_trophyParent;

	private bool IsPrivateMatchAddTimer;

	private bool IsDebutNoticeOpen;

	private Dictionary<KeyCode, (float repeatSec, float pressSec)> _keyRepeatTime = new Dictionary<KeyCode, (float, float)>
	{
		{
			KeyCode.UpArrow,
			(0f, 0f)
		},
		{
			KeyCode.DownArrow,
			(0f, 0f)
		}
	};

	private const float _repeatDelaySec = 0.3f;

	private const float _repeatIntervalSec = 0.1f;

	[SerializeField]
	private PlayerGameData m_playerGameData => GameDataManager.Player;

	[SerializeField]
	private AccountGameData m_accountGameData => GameDataManager.Account;

	protected override async UniTask OnInitialize(CancellationToken cancellationToken)
	{
		m_charaInfo.SetActive(value: false);
		m_eventMask.SetActive(value: true);
		m_tsumikomiButton.gameObject.SetActive(value: false);
		m_tsumikomi2Button.gameObject.SetActive(value: false);
		m_gameDataTestButton.gameObject.SetActive(value: false);
		m_soundTestButton.gameObject.SetActive(value: false);
		if (GameUtility.IsKonastaOrNetcafeBehavior && GameDataManager.Account.IsGuestUser)
		{
			m_gameOptionButton.gameObject.SetActive(value: false);
		}
		GameDataManager.Gacha.SelectedSeriesInfo = null;
		try
		{
			await OnInitializeBeforeIntroduction(cancellationToken);
			ResidentFlow.GetInstance().GetTimer().MaxTime = 99;
			ResidentFlow.GetInstance().GetTimer().SetTimer(60f, disp: true);
			ResidentFlow.GetInstance().GetTimer().StartTimer();
			ResidentFlow.GetInstance().GetHeader().SetDisp(disp: true);
			ResidentFlow.GetInstance().GetHeader().SetSceneName("ホーム");
			SoundManager.PlayMyCharaMyRoomBGM().Forget();
		}
		catch (OperationCanceledException)
		{
			Debug.Log(GetType().Name + MethodBase.GetCurrentMethod().Name + ": OperationCanceledException Catched.");
		}
	}

	protected override async UniTask OnExecute(CancellationToken cancellationToken)
	{
		_ = 2;
		try
		{
			cancellationToken.ThrowIfCancellationRequested();
			ResidentFlow.GetInstance().ExitLoading();
			if (SystemConfigGameData.IsDualMonitor)
			{
				SubMonitorFlow.GetInstance().TransitionAnimClose();
			}
			await ResidentFlow.GetInstance().TransitionEffectClose();
			ResidentFlow.GetInstance().GetMahjongActorNoticeManager().SetUpperDisp(isUpperDisp: true);
			await ExecIntroduction(cancellationToken);
			await OnInitializeAfterIntroduction(cancellationToken);
			SoundManager.StopVoiceAll().Forget();
			SoundManager.PlayMyCharaVoice(GameDataManager.Player.IsOdekake ? "VOX_GXX_APPEAL1" : "VOX_GXX_MENU1").Forget();
		}
		catch (OperationCanceledException)
		{
			Debug.Log(GetType().Name + MethodBase.GetCurrentMethod().Name + ": OperationCanceledException Catched.");
		}
	}

	private void ForceModeDecideForAging()
	{
	}

	private async UniTask OnInitializeBeforeIntroduction(CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		await GameDataManager.GachaInfo.Validate();
		ResidentFlow.GetInstance().SetLoadingProgress(0.2f);
		bool isGuestUser = GameDataManager.Account.IsGuestUser;
		bool flag = DebugFlag.IsActive(DebugFlag.Flag.EnableUnlimitedButton);
		if (isGuestUser)
		{
			m_gachaButton.interactable = false;
			m_charaSelectButton.interactable = false;
			m_guestUnableMessage.SetActive(value: true);
			m_missionButton.interactable = false;
			m_noticeButton.interactable = false;
			m_presentButton.interactable = false;
			m_dojoButton.interactable = false;
			m_dojoButton.transform.Find("AppealIcon").gameObject.SetActive(value: false);
			m_debutNoticeButton.gameObject.SetActive(value: false);
		}
		else
		{
			m_gachaButton.interactable = ((!GameUtility.IsFreePlay() && !GameDataManager.Account.IsForceFreePlayUser) || (GameUtility.IsKonastaOrNetcafeBehavior && (GameDataManager.Player.IsPro() || GameUtility.IsProCardId(GameDataManager.Account.CardID)))) && GameDataManager.GachaInfo.IsAnyActive() && ((!GameDataManager.PlayerConfig.IsGachaSceneVisited && !GameUtility.IsReservedGameOver) || flag);
			m_charaSelectButton.interactable = !GameDataManager.PlayerConfig.IsMyRoomSceneVisited || flag;
			m_guestUnableMessage.SetActive(value: false);
			m_missionButton.interactable = true;
			m_noticeButton.interactable = true;
			m_presentButton.interactable = true;
			m_dojoButton.interactable = !GameDataManager.PlayerConfig.IsDojoSceneVisited || flag;
			m_debutNoticeButton.gameObject.SetActive(!GameDataManager.Player.IsOdekake && !GameDataManager.Player.GetCurrentChara().IsDebut);
			if (GameDataManager.SpiritGymBonusEventEnable())
			{
				m_dojoButton.transform.Find("AppealIcon").gameObject.SetActive(value: true);
				int spiritGymBonusRatio = GameDataManager.GetSpiritGymBonusRatio();
				m_dojoButton.transform.Find("AppealIcon").Find("Number").GetComponent<TextMeshProUGUI>()
					.text = spiritGymBonusRatio.ToString();
			}
			else
			{
				m_dojoButton.transform.Find("AppealIcon").gameObject.SetActive(value: false);
			}
		}
		if (GameDataManager.PlayerConfig.IsContinued)
		{
			m_taikyokuButton.interactable = GameDataManager.Mahjong.TakuType == TakuType.Nima || !GameDataManager.PlayerConfig.IsEventTakuSelected;
			m_eventTaikyokuButton.interactable = (AnyEventTakuActive() && GameDataManager.Mahjong.TakuType == TakuType.Nima) || GameDataManager.PlayerConfig.IsEventTakuSelected;
		}
		else
		{
			m_taikyokuButton.interactable = true;
			m_eventTaikyokuButton.interactable = AnyEventTakuActive();
		}
		if (GameDataManager.IsPrivateMatchDisplay())
		{
			if (GameDataManager.IsPrivateMatchEnable())
			{
				m_privateMatchButtonToOn.OnClickAsObservable().Subscribe(async delegate
				{
					SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
					if (!IsPrivateMatchAddTimer)
					{
						ResidentFlow.GetInstance().GetTimer().AddTimer(20f);
						IsPrivateMatchAddTimer = true;
					}
					m_privateMatchWindow.OpenWindow();
				}).AddTo(this);
				m_privateMatchButtonToOff.OnClickAsObservable().Subscribe(async delegate
				{
					SoundManager.PlaySe("SFX_SYS_CANCEL2").Forget();
					GameDataManager.PlayerConfig.IsPrivateMatch = false;
				}).AddTo(this);
				m_privateMatchButtonAttention.SetActive(value: false);
				m_privateMatchButtonToOn.interactable = true;
				m_privateMatchButtonParent.SetActive(value: true);
			}
			else
			{
				m_privateMatchButtonToOn.interactable = false;
				m_privateMatchButtonAttention.SetActive(!GameDataManager.Player.IsPro());
				m_privateMatchButtonToOn.gameObject.SetActive(value: true);
				m_privateMatchButtonParent.SetActive(value: true);
			}
		}
		else
		{
			m_privateMatchButtonParent.SetActive(value: false);
		}
		if (GameDataManager.AnyConstancyEventTakuScheduleDisp())
		{
			m_eventTaikyokuScheduleButton.gameObject.SetActive(value: true);
			m_eventTaikyokuScheduleButton.OnClickAsObservable().Subscribe(delegate
			{
				SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
				m_eventTaikyokuScheduleWindow.OpenWindow();
			}).AddTo(this);
		}
		if (AnyConstancyEventTakuActive())
		{
			foreach (BaseEventGameData item in GameDataManager.EventTaku.ConstancyTakuAvailables())
			{
				EventTaikyokuButtonNoticeDisp(item.Master.HomeNoticeText);
			}
		}
		IsNewDispReflesh(isFirstReflesh: true, cancellationToken).Forget();
		if (!GameDataManager.Account.IsGuestUser)
		{
			ResidentFlow.GetInstance().SetLoadingProgress(0.4f);
		}
		await EventWindowInit();
		await EvnetWindowButtonInit();
		ResidentFlow.GetInstance().SetLoadingProgress(0.6f);
		HomeCarouselBannerInit().Forget();
		await GameDataManager.PresentBox.AutoAssignment();
		ShopCompetitionInfo trophyDisplayCompetitionInfo = GameDataManager.ShopCompetition.GetTrophyDisplayCompetitionInfo();
		if (trophyDisplayCompetitionInfo != null && (trophyDisplayCompetitionInfo.TrophyType != 0 || DebugFlag.IsActive(DebugFlag.Flag.ShopCompe001TrophyForceEnable)))
		{
			IDisposableAsset<GameObject> obj = await AssetLoader.LoadAssetAsync<GameObject>(trophyDisplayCompetitionInfo.GetTrophyInfo().Item2);
			obj.AddTo(this);
			UnityEngine.Object.Instantiate(obj.Asset, m_trophyParent.transform, worldPositionStays: false);
			m_trophyParent.SetActive(value: true);
		}
		else
		{
			m_trophyParent.SetActive(value: false);
		}
		m_proStayBanner.SetActive(GameDataManager.IsProNormalTakuStay());
		m_eventModeSelectProStayBanner.SetActive(GameDataManager.IsProEventTakuStay() && AnyEventTakuActive());
		m_charaInfo.SetActive(value: true);
		m_charaInfo.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = MasterGameDataManager.Character.Properties(GameDataManager.Player.SelectChara).Profile.Name;
		m_charaInfo.transform.Find("Love").GetComponent<TextMeshProUGUI>().text = GameDataManager.Player.GetCurrentChara(IsGetMyCharaData: true).Level.ToString("N0");
		m_charaInfo.transform.Find("FanNum").GetComponent<TextMeshProUGUI>().text = GameDataManager.Player.GetCurrentChara(IsGetMyCharaData: true).TotalFan.ToString("N0");
		GameDataManager.Player.GetCurrentChara().CostumeItemEquip = GameUtility.CostumeUtility.CostumeConversion(GameDataManager.Player.GetCurrentChara().CostumeItemEquip);
		await m_charaManager.Load(GameDataManager.Player.GetCurrentChara().CostumeItemEquip);
		m_charaManager.EnableTouchRotate = false;
		m_charaManager.IsTouchReactionEnable = false;
		ResidentFlow.GetInstance().SetLoadingProgress(0.8f);
		_catchphrasePlate.Reflesh(GameDataManager.Player.GetCurrentChara().CatchphraseItemEquip, cancellationToken).Forget();
		m_modeSelectWindow.gameObject.SetActive(value: true);
		m_modeSelectWindow.OnDecided.Subscribe(async delegate
		{
			await GotoNextScene();
		}).AddTo(this);
		m_modeSelectWindow.OnClose.Subscribe(delegate
		{
			_taikyokuModeSelectStatus = TaikyokuModeSelectStatus.None;
		}).AddTo(this);
		ResidentFlow.GetInstance().SetLoadingProgress(1f, 0f);
		await UniTask.Yield();
	}

	private async UniTask OnInitializeAfterIntroduction(CancellationToken cancellationToken)
	{
		m_charaManager.EnableTouchRotate = true;
		m_charaManager.IsTouchReactionEnable = true;
		m_carouselBanner.TouchEnable = true;
		m_charaSelectButton.OnClickAsObservable().Subscribe(async delegate
		{
			if (!_isNextSceneDecided)
			{
				GameDataManager.PlayerConfig.IsMyRoomSceneVisited = true;
				SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
				GotoNextScene("CharaSelect");
			}
		}).AddTo(this);
		bool active = false;
		if (m_charaSelectButton.interactable)
		{
			int num = CostumeItemMaster.NeedUnlockFightSpiritNum(CostumeItemMaster.GetDebutItemID(GameDataManager.Player.SelectChara));
			if (!GameDataManager.Player.GetCurrentChara().IsDebut && GameDataManager.PlayerItem.GetUsableSpiritCount(GameDataManager.Player.SelectChara) >= num)
			{
				active = true;
			}
			if (GameDataManager.IsOdekakeEnable() && !GameDataManager.Account.IsGuestUser && !GameDataManager.Player.IsPro())
			{
				List<FriendInfo> friendInfos = GameDataManager.Friend.FriendInfos;
				if (friendInfos != null && friendInfos.Count((FriendInfo x) => x.IsUsable) > 0)
				{
					active = true;
				}
			}
		}
		m_charaSelectButton.transform.Find("Bt/IconUpdate").gameObject.SetActive(active);
		m_gachaButton.OnClickAsObservable().Subscribe(async delegate
		{
			await OnGachaButton();
		}).AddTo(this);
		m_noticeButton.OnClickAsObservable().Subscribe(async delegate
		{
			SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
			if (m_noticeButton.transform.Find("IconUpdate").gameObject.activeSelf)
			{
				SoundManager.PlayMyCharaVoice("VOX_GXX_MENU3").Forget();
			}
			await m_noticeWindow.Play(cancellationToken);
			await IsNewDispReflesh();
		}).AddTo(this);
		m_presentButton.OnClickAsObservable().Subscribe(async delegate
		{
			SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
			if (GameDataManager.PresentBox.PresentInfos.Find((PresentInfo a) => !a.IsReceived) != null)
			{
				SoundManager.PlayMyCharaVoice("VOX_GXX_MENU4").Forget();
			}
			await m_presentBoxWindow.Play();
			await IsNewDispReflesh();
		}).AddTo(this);
		m_dojoButton.OnClickAsObservable().Subscribe(async delegate
		{
			if (!_isNextSceneDecided)
			{
				GameDataManager.PlayerConfig.IsDojoSceneVisited = true;
				SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
				GotoNextScene("Dojo");
			}
		}).AddTo(this);
		m_missionButton.OnClickAsObservable().Subscribe(async delegate
		{
			SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
			SoundManager.PlayMyCharaVoice("VOX_GXX_MENU5").Forget();
			await m_missionWindow.Play();
		}).AddTo(this);
		m_gameOptionButton.OnClickAsObservable().Subscribe(delegate
		{
			SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
			m_gameOption.SetActive(active: true);
		}).AddTo(this);
		m_debutNoticeButton.OnClickAsObservable().Subscribe(delegate
		{
			SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
			IsDebutNoticeOpen = true;
			m_debutNoticeWindow.SetActive(value: true);
			m_debutNoticeWindow.transform.GetComponent<Animator>().SetTrigger("In");
		}).AddTo(this);
		m_debutNoticeCloseButton.OnClickAsObservable().Subscribe(delegate
		{
			SoundManager.PlaySe("SFX_SYS_CANCEL1").Forget();
			IsDebutNoticeOpen = false;
			m_debutNoticeWindow.transform.GetComponent<Animator>().SetTrigger("Out");
			m_debutNoticeWindow.SetActive(value: false);
		}).AddTo(this);
		m_taikyokuButton.OnClickAsObservable().Subscribe(delegate
		{
			OnTaikyokuButton();
		}).AddTo(this);
		m_eventTaikyokuButton.OnClickAsObservable().Subscribe(delegate
		{
			OnEventTaikyokuButton();
		}).AddTo(this);
	}

	private async void OnTaikyokuButton()
	{
		if (_taikyokuModeSelectStatus != 0)
		{
			return;
		}
		_taikyokuModeSelectStatus = TaikyokuModeSelectStatus.SelectingNormal;
		if (GameDataManager.PlayerConfig.IsContinued && GameDataManager.Mahjong.TakuType != TakuType.Nima)
		{
			Debug.Log("HomeFlow: 通常卓コンティニュー");
			if (GameDataManager.PlayerConfig.IsPrivateMatch)
			{
				await GotoNextScene();
			}
			else
			{
				await m_modeSelectWindow.OpenContinue(ModeSelectWindowType.Normal);
			}
		}
		else
		{
			Debug.Log("HomeFlow: 通常卓モード選択");
			await m_modeSelectWindow.Open(ModeSelectWindowType.Normal);
		}
	}

	private async void OnEventTaikyokuButton()
	{
		if (_taikyokuModeSelectStatus != 0)
		{
			return;
		}
		_taikyokuModeSelectStatus = TaikyokuModeSelectStatus.SelectingEvent;
		if (GameDataManager.PlayerConfig.IsContinued && GameDataManager.Mahjong.TakuType != TakuType.Nima)
		{
			Debug.Log("HomeFlow: イベント卓コンティニュー");
			if (GameDataManager.PlayerConfig.IsPrivateMatch)
			{
				await GotoNextScene();
			}
			else
			{
				await m_modeSelectWindow.OpenContinue(ModeSelectWindowType.Event);
			}
		}
		else
		{
			Debug.Log("HomeFlow: イベント卓モード選択");
			await m_modeSelectWindow.Open(ModeSelectWindowType.Event);
		}
	}

	private async UniTask OnGachaButton(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!_isNextSceneDecided && await m_gachaModeSelectWindow.Open(cancellationToken))
		{
			GotoNextScene("GachaSelect");
		}
	}

	private async UniTask ImportantNoticeAppearance(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!(m_importantNoticeWindow != null))
		{
			return;
		}
		List<ImportantNoticeInfo> importantNotices = GameDataManager.ReceiveOnly.ImportantNotices;
		if (importantNotices == null)
		{
			return;
		}
		foreach (ImportantNoticeInfo item in importantNotices)
		{
			m_importantNoticeWindow.OpenWindow(item.NoticeID);
			await UniTask.WaitWhile(() => m_importantNoticeWindow.isOpen, PlayerLoopTiming.Update, cancellationToken);
		}
		ResidentFlow.GetInstance().GetTimer().SetTimer(60f, disp: true);
		ResidentFlow.GetInstance().GetTimer().StartTimer();
		if (m_importantNoticeWindow.readList != null && m_importantNoticeWindow.readList.Count > 0)
		{
			await RequestApiBase<ImportantNoticeDone, ImportantNoticeDone.ResultData, List<int>>.Send(m_importantNoticeWindow.readList);
			GameDataManager.ReceiveOnly.ImportantNotices.Clear();
			Debug.Log("重要なお知らせ 既読 " + string.Join(",", m_importantNoticeWindow.readList));
		}
	}

	private async UniTask CharaPickupGachaNotice(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (GameDataManager.Notice.PickupGachaNoticeCharaType == CharaType.Chara10)
		{
			ResidentFlow.GetInstance().GetTimer().SetPause(pause: true);
			m_charaPickupGachaNoticeWindow.OpenWindow(null);
			await UniTask.WhenAny(UniTask.WaitWhile(() => m_charaPickupGachaNoticeWindow.isOpen), UniTask.Delay(TimeSpan.FromSeconds(20.0), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken));
			await m_charaPickupGachaNoticeWindow.CloseWindow();
			ResidentFlow.GetInstance().GetTimer().SetPause(pause: false);
		}
	}

	private async UniTask FightGirlNewsAppearance(CancellationToken cancellationToken = default(CancellationToken))
	{
		ResidentFlow.GetInstance().GetTimer().SetPause(pause: true);
		foreach (NewsID item in from x in Enum.GetValues(typeof(NewsID)).Cast<NewsID>().ToList()
			orderby x descending
			select x)
		{
			await m_homeFightGirlNews.Play(item, cancellationToken);
		}
		ResidentFlow.GetInstance().GetTimer().SetPause(pause: false);
	}

	private async UniTask OneShotNoticeAppearance(CancellationToken cancellationToken = default(CancellationToken))
	{
		Dictionary<MFG.Types.OneShotNotice.Item, OneShotNoticeWindow> oneShotNoticeWindows = _oneShotNoticeWindows;
		if (oneShotNoticeWindows == null || oneShotNoticeWindows.Count <= 0)
		{
			return;
		}
		ResidentFlow.GetInstance().GetTimer().SetPause(pause: true);
		foreach (KeyValuePair<MFG.Types.OneShotNotice.Item, OneShotNoticeWindow> oneShotNoticeWindow in _oneShotNoticeWindows)
		{
			await oneShotNoticeWindow.Value.OpenWindow();
		}
		ResidentFlow.GetInstance().GetTimer().SetPause(pause: false);
	}

	private async UniTask OnlyPeraichiNoticeAppearance(CancellationToken cancellationToken = default(CancellationToken))
	{
		Dictionary<MFG.Types.OnlyPeraichiNotice.Item, OnlyPeraichiNoticeWindow> onlyPeraichiNoticeWindows = _onlyPeraichiNoticeWindows;
		if (onlyPeraichiNoticeWindows == null || onlyPeraichiNoticeWindows.Count <= 0)
		{
			return;
		}
		ResidentFlow.GetInstance().GetTimer().SetPause(pause: true);
		foreach (KeyValuePair<MFG.Types.OnlyPeraichiNotice.Item, OnlyPeraichiNoticeWindow> item in _onlyPeraichiNoticeWindows)
		{
			if (!GameDataManager.PlayerConfig.OnlyPeraichiNoticeFinishIDs.Contains(item.Key.ID))
			{
				GameDataManager.PlayerConfig.OnlyPeraichiNoticeFinishIDs.Add(item.Key.ID);
				await item.Value.OpenWindow();
				await UniTask.WhenAny(UniTask.WaitWhile(() => item.Value.isOpen), UniTask.Delay(TimeSpan.FromSeconds(10.0), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken));
				item.Value.CloseWindow().Forget();
			}
		}
		ResidentFlow.GetInstance().GetTimer().SetPause(pause: false);
	}

	public void FinishPlayingVideo(VideoPlayer vp)
	{
		vp.Stop();
	}

	private async UniTask GuideDisp(CancellationToken cancellationToken = default(CancellationToken))
	{
		float timeLimit = 20f;
		ResidentFlow.GetInstance().GetTimer().SetPause(pause: true);
		m_topInfo.SetActive(value: true);
		m_topInfo.GetComponent<Animator>().SetTrigger("FirstTutorial");
		await UniTask.Delay(TimeSpan.FromSeconds(2.0), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
		await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(timeLimit), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken), UniTask.WaitUntil(() => ResidentFlow.CustomInput.GetMouseButtonDown(0), PlayerLoopTiming.Update, cancellationToken));
		m_topInfo.GetComponent<Animator>().SetTrigger("Skip");
		await UniTask.Delay(TimeSpan.FromSeconds(1.0), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
		await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(timeLimit), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken), UniTask.WaitUntil(() => ResidentFlow.CustomInput.GetMouseButtonDown(0), PlayerLoopTiming.Update, cancellationToken));
		m_topInfo.GetComponent<Animator>().SetTrigger("Skip");
		await UniTask.Delay(TimeSpan.FromSeconds(1.0), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
		await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(timeLimit), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken), UniTask.WaitUntil(() => ResidentFlow.CustomInput.GetMouseButtonDown(0), PlayerLoopTiming.Update, cancellationToken));
		m_topInfo.GetComponent<Animator>().SetTrigger("Skip");
		await UniTask.Delay(TimeSpan.FromSeconds(1.0), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
		await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(timeLimit), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken), UniTask.WaitUntil(() => ResidentFlow.CustomInput.GetMouseButtonDown(0), PlayerLoopTiming.Update, cancellationToken));
		m_topInfo.GetComponent<Animator>().SetTrigger("Skip");
		SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
		m_topInfo.SetActive(value: false);
		ResidentFlow.GetInstance().GetTimer().SetPause(pause: false);
	}

	private void DebugKey_Update()
	{
		KeyCode[] array = _keyRepeatTime.Keys.ToArray();
		foreach (KeyCode keyCode in array)
		{
			if (Bi2xInput.GetKey(keyCode))
			{
				(float repeatSec, float pressSec) tuple = _keyRepeatTime[keyCode];
				float item = tuple.repeatSec;
				float item2 = tuple.pressSec;
				item += Time.deltaTime;
				item2 += Time.deltaTime;
				if (item - 0.3f >= 0.1f)
				{
					item = 0.3f;
				}
				_keyRepeatTime[keyCode] = (item, item2);
			}
			else
			{
				_keyRepeatTime[keyCode] = (0f, 0f);
			}
		}
	}

	private bool DebugKey_IsRepeat(KeyCode keyCode)
	{
		if (Bi2xInput.GetKeyDown(keyCode))
		{
			return true;
		}
		return Mathf.Approximately(_keyRepeatTime[keyCode].repeatSec, 0.3f);
	}

	private float DebugKey_GetSecAfterRepeat(KeyCode keyCode)
	{
		return Mathf.Max(_keyRepeatTime[keyCode].pressSec - 0.3f, 0f);
	}

	private float DebugKey_Lerp(KeyCode keyCode, float min, float max, float sec)
	{
		if (!DebugKey_IsRepeat(keyCode))
		{
			return 0f;
		}
		float num = DebugKey_GetSecAfterRepeat(keyCode);
		return Mathf.Lerp(min, max, Mathf.Clamp(num / sec, 0f, 1f));
	}

	protected override void OnUpdate()
	{
		if (GameDataManager.IsPrivateMatchDisplay())
		{
			if (GameDataManager.IsPrivateMatchEnable())
			{
				if (GameDataManager.Player.IsPro())
				{
					m_privateMatchButtonToOn.interactable = false;
					m_privateMatchButtonToOn.gameObject.SetActive(value: true);
				}
				else
				{
					m_privateMatchButtonToOff.gameObject.SetActive(GameDataManager.PlayerConfig.IsPrivateMatch);
					m_privateMatchButtonToOn.gameObject.SetActive(!GameDataManager.PlayerConfig.IsPrivateMatch);
				}
			}
			else
			{
				GameDataManager.PlayerConfig.IsPrivateMatch = false;
			}
		}
		else
		{
			GameDataManager.PlayerConfig.IsPrivateMatch = false;
		}
		if (ResidentFlow.GetInstance().GetTimer().IsEnd())
		{
			TimeupForceDecide();
		}
	}

	private void TimeupForceDecide()
	{
		if (m_importantNoticeWindow != null && m_importantNoticeWindow.isOpen)
		{
			m_importantNoticeWindow.CloseWindow().Forget();
			return;
		}
		if (m_privateMatchWindow.IsOpen)
		{
			m_privateMatchWindow.CloseWindow().Forget();
			GameDataManager.PlayerConfig.PrivateMatchPassCode = "";
			GameDataManager.PlayerConfig.IsPrivateMatch = false;
		}
		if (IsDebutNoticeOpen)
		{
			m_debutNoticeWindow.SetActive(value: false);
			IsDebutNoticeOpen = false;
		}
		if (m_presentBoxWindow._isOpen)
		{
			m_presentBoxWindow.Close().Forget();
		}
		if (m_noticeWindow._isOpen)
		{
			m_noticeWindow.Close(isTimeup: true).Forget();
		}
		if (m_missionWindow.isOpen)
		{
			m_missionWindow.Close();
		}
		if (m_eventTaikyokuScheduleWindow.isOpen)
		{
			m_eventTaikyokuScheduleWindow.CloseWindow();
		}
		EventWindowClose();
		m_gameOption.SetActive(active: false);
		if (_taikyokuModeSelectStatus == TaikyokuModeSelectStatus.None && !_isNextSceneDecided)
		{
			Debug.Log("HomeFlow: タイムアップモード強制決定");
			m_modeSelectWindow.CloseButton_SetActive(isActive: false);
			if (GameDataManager.PlayerConfig.IsContinued && GameDataManager.PlayerConfig.IsEventTakuSelected)
			{
				OnEventTaikyokuButton();
			}
			else
			{
				OnTaikyokuButton();
			}
			if (!GameDataManager.PlayerConfig.IsContinued || GameDataManager.Mahjong.TakuType == TakuType.Nima)
			{
				m_taikyokuButton.transition = Selectable.Transition.None;
				m_taikyokuButton.GetComponent<Animator>().SetTrigger("Selected");
				ResidentFlow.GetInstance().GetTimer().SetTimer(20f, disp: true);
				ResidentFlow.GetInstance().GetTimer().StartTimer();
			}
		}
	}

	private bool AnyEventTakuActive()
	{
		if (GameDataManager.Account.IsGuestUser)
		{
			if (!GameDataManager.EventTaku.AvailableModeGameDatasWithGuest().Any() && !GameDataManager.EventTaku.CompetitionTakuAvailables().Any())
			{
				return GameDataManager.EventTaku.ConstancyTakuAvailables().Any();
			}
			return false;
		}
		if (!GameDataManager.EventTaku.AvailableModeGameDatas().Any() && !GameDataManager.EventTaku.CompetitionTakuAvailables().Any())
		{
			return GameDataManager.EventTaku.ConstancyTakuAvailables().Any();
		}
		return true;
	}

	private bool AnyConstancyEventTakuActive()
	{
		if (!GameDataManager.EventTaku.AvailableModeGameDatasWithGuest().Any() && !GameDataManager.EventTaku.CompetitionTakuAvailables().Any())
		{
			return GameDataManager.EventTaku.ConstancyTakuAvailables().Any();
		}
		return false;
	}

	private bool AnyCompetitionEventTakuActive()
	{
		if (!GameDataManager.EventTaku.AvailableModeGameDatasWithGuest().Any() && GameDataManager.EventTaku.CompetitionTakuAvailables().Any())
		{
			return !GameDataManager.EventTaku.ConstancyTakuAvailables().Any();
		}
		return false;
	}

	private async UniTask IsNewDispReflesh(bool isFirstReflesh = false, CancellationToken cancellationToken = default(CancellationToken))
	{
		m_missionButton.transform.Find("IconUpdate").gameObject.SetActive(value: false);
		if (GameDataManager.Account.IsGuestUser)
		{
			m_noticeButton.transform.Find("IconUpdate").gameObject.SetActive(value: false);
			m_presentButton.transform.Find("IconUpdate").gameObject.SetActive(value: false);
		}
		else
		{
			NoticeInfo noticeInfo = GameDataManager.Notice.NoticeInfos.Find((NoticeInfo a) => a.IsNew);
			m_noticeButton.transform.Find("IconUpdate").gameObject.SetActive(noticeInfo != null);
			PresentInfo presentInfo = GameDataManager.PresentBox.PresentInfos.Find((PresentInfo a) => !a.IsReceived);
			m_presentButton.transform.Find("IconUpdate").gameObject.SetActive(presentInfo != null);
		}
		if (!isFirstReflesh)
		{
			return;
		}
		try
		{
			if (m_dojoButton.interactable)
			{
				DojoGetStatus.ResultData resultData = await RequestApiBase<DojoGetStatus, DojoGetStatus.ResultData>.Send(enableErrorScreenTransition: false, enableSendLog: true, cancellationToken);
				if (resultData.StatusCode != StatusCode.OK)
				{
					m_dojoButton.transform.Find("IconUpdate").gameObject.SetActive(value: false);
				}
				else
				{
					bool flag = resultData.IsSpiritFullSlotExist();
					m_dojoButton.transform.Find("IconUpdate").gameObject.SetActive(flag);
					m_dojoButton.transform.Find("Bt").gameObject.SetActive(!flag);
					m_dojoButton.transform.Find("BtAppear").gameObject.SetActive(flag);
				}
				if (resultData.IsNonSetSlotExist())
				{
					m_dojoButton.transform.Find("DebutNotice").GetComponent<Animator>().ResetTrigger("Out");
					m_dojoButton.transform.Find("DebutNotice").GetComponent<Animator>().SetTrigger("In");
				}
				else
				{
					m_dojoButton.transform.Find("DebutNotice").GetComponent<Animator>().ResetTrigger("In");
					m_dojoButton.transform.Find("DebutNotice").GetComponent<Animator>().SetTrigger("Out");
				}
			}
		}
		catch
		{
			if (m_dojoButton != null)
			{
				m_dojoButton.transform.Find("IconUpdate").gameObject.SetActive(value: false);
			}
		}
	}

	private async UniTask HomeCarouselBannerInit()
	{
		List<NoticeInfo> carouselBannerList = GameDataManager.Notice.NoticeInfos.Where((NoticeInfo a) => a.IsDisplayHomeBanner()).ToList();
		m_carouselBanner._tenshiGowanWindow = m_tenshiGowanWindow;
		if (GameDataManager.Account.IsGuestUser || carouselBannerList.Count < 1)
		{
			m_carouselBanner.gameObject.SetActive(value: false);
			return;
		}
		m_carouselBanner.TouchEnable = false;
		await m_carouselBanner.Init(carouselBannerList);
		if (carouselBannerList.Count == 1)
		{
			m_carouselBanner.AutoScrollUnable();
		}
		m_carouselBanner.OnBannerChangeBegin.Subscribe(delegate((NoticeInfo bannerInfo, bool isAutoMove) data)
		{
			if (!data.isAutoMove)
			{
				SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
			}
		}).AddTo(this);
		m_carouselBanner.OnBannerChangeEnd.Subscribe(delegate
		{
		}).AddTo(this);
	}

	private bool CompetitionButton_IsNeedDisp()
	{
		if (GameDataManager.Account.IsGuestUser)
		{
			return false;
		}
		CompetitionInfo competitionInfo = GameDataManager.Competition.CompetitionInfos.Find((CompetitionInfo x) => x.CompetitionStatus == CompetitionStatus.InProgress || x.CompetitionStatus == CompetitionStatus.Aggregating);
		if (competitionInfo == null)
		{
			return false;
		}
		if (competitionInfo.CompetitionStatus == CompetitionStatus.InProgress && (competitionInfo.IsEntried || competitionInfo.IsEnableEntry))
		{
			return true;
		}
		if (competitionInfo.CompetitionStatus == CompetitionStatus.Aggregating)
		{
			return competitionInfo.IsEntried;
		}
		return false;
	}

	private bool ShopCompetitionButton_IsNeedDisp()
	{
		if (GameDataManager.Account.IsGuestUser)
		{
			return false;
		}
		if (CompetitionButton_IsNeedDisp())
		{
			return false;
		}
		if (GameDataManager.ShopCompetition.GetInProgressCompetitionInfo() != null)
		{
			return true;
		}
		return false;
	}

	private bool JandriNoticeButton_IsNeedDisp()
	{
		if (!CompetitionButton_IsNeedDisp() && !ShopCompetitionButton_IsNeedDisp())
		{
			return GameDataManager.IsAdJandriAppear();
		}
		return false;
	}

	private async UniTask CompetitionButton_BannerReflesh()
	{
		CompetitionInfo competitionInfo = GameDataManager.Competition.CompetitionInfos.Find((CompetitionInfo x) => x.CompetitionStatus == CompetitionStatus.InProgress || x.CompetitionStatus == CompetitionStatus.Aggregating);
		if (competitionInfo != null)
		{
			MFG.Types.Competition.CompetitionID competitionID = competitionInfo.CompetitionID;
			Image image1 = m_competitionButton.transform.Find("Bt").GetComponent<Image>();
			Image image2 = m_competitionButton.transform.Find("BtAdd").GetComponent<Image>();
			IDisposableAsset<Sprite> disposableAsset = await AssetLoader.LoadAssetAsync<Sprite>(AssetPathResolver.ResolveCompetitionAssets(competitionID, CompetitionAssetType.Title));
			disposableAsset.AddTo(this);
			image1.sprite = disposableAsset.Asset;
			image2.sprite = disposableAsset.Asset;
		}
	}

	private async UniTask ShopCompetitionButton_BannerReflesh()
	{
		ShopCompetitionInfo inProgressCompetitionInfo = GameDataManager.ShopCompetition.GetInProgressCompetitionInfo();
		if (inProgressCompetitionInfo != null)
		{
			MFG.Types.ShopCompetition.CompetitionID competitionID = inProgressCompetitionInfo.CompetitionID;
			Image image1 = m_competitionButton.transform.Find("Bt").GetComponent<Image>();
			Image image2 = m_competitionButton.transform.Find("BtAdd").GetComponent<Image>();
			IDisposableAsset<Sprite> disposableAsset = await AssetLoader.LoadAssetAsync<Sprite>($"ShopCompetition/{competitionID}/Title.png");
			disposableAsset.AddTo(this);
			image1.sprite = disposableAsset.Asset;
			image2.sprite = disposableAsset.Asset;
		}
	}

	private void CompetitionButton_ModifyStatusText()
	{
		CompetitionInfo competitionInfo = GameDataManager.Competition.CompetitionInfos.Find((CompetitionInfo x) => x.CompetitionStatus == CompetitionStatus.InProgress || x.CompetitionStatus == CompetitionStatus.Aggregating);
		if (competitionInfo != null)
		{
			TextMeshProUGUI competitionButtonStatusText = m_competitionButtonStatusText;
			competitionButtonStatusText.text = competitionInfo.CompetitionStatus switch
			{
				CompetitionStatus.InProgress => "大会開催中！", 
				CompetitionStatus.Aggregating => "大会集計中！", 
				_ => "", 
			};
		}
	}

	private void ShopCompetitionButton_ModifyStatusText()
	{
		if (ShopCompetitionButton_IsNeedDisp())
		{
			m_competitionButtonStatusText.text = "店舗対抗戦開催中！";
		}
	}

	private void CompetitionEntriedBanner_Reflesh()
	{
		Transform transform = m_taikyokuButton.transform.Find("Bt/EventNotice");
		Transform transform2 = m_eventTaikyokuButton.transform.Find("Bt/EventNotice");
		Animator component = transform.GetComponent<Animator>();
		Animator component2 = transform2.GetComponent<Animator>();
		CompetitionInfo inProgressEntriedCompetitionInfo = GameDataManager.Competition.GetInProgressEntriedCompetitionInfo();
		if (inProgressEntriedCompetitionInfo == null)
		{
			component.Play("ModeCompetitionIconWait");
			component2.Play("ModeCompetitionIconWait");
			return;
		}
		MFG.Types.EventType compeEventType = inProgressEntriedCompetitionInfo.CompeEventType;
		bool isInTime = inProgressEntriedCompetitionInfo.IsInTime;
		string text = (isInTime ? "<size=23>大会エントリー中" : "<size=24>大会エントリー中\n【対象時間外】");
		if (compeEventType == MFG.Types.EventType.None)
		{
			component2.Play("ModeCompetitionIconWait");
			transform.Find("Base/Text (TMP)").GetComponent<TextMeshProUGUI>().text = text;
			component.SetTrigger(isInTime ? "In" : "Dark");
		}
		else
		{
			component.Play("ModeCompetitionIconWait");
			transform2.Find("Base/Text (TMP)").GetComponent<TextMeshProUGUI>().text = text;
			component2.SetTrigger(isInTime ? "In" : "Dark");
		}
	}

	private void EventTaikyokuButtonNoticeDisp(string dispText = "")
	{
		if (!string.IsNullOrEmpty(dispText))
		{
			Transform obj = m_eventTaikyokuButton.transform.Find("Bt/EventNotice");
			Animator component = obj.GetComponent<Animator>();
			obj.Find("Base/Text (TMP)").GetComponent<TextMeshProUGUI>().text = dispText;
			component.SetTrigger("In");
		}
	}

	private async UniTask EventWindowInit()
	{
		if (GameDataManager.ReceiveOnly.ImportantNotices.Count > 0)
		{
			IDisposableAsset<GameObject> obj = await AssetLoader.LoadAssetAsync<GameObject>("ImportantNotice/Prefabs/NoticeWindow.prefab");
			obj.AddTo(this);
			GameObject gameObject = UnityEngine.Object.Instantiate(obj.Asset, m_eventParent.transform, worldPositionStays: false);
			m_importantNoticeWindow = gameObject.GetComponent<ImportantNoticeWindow>();
			m_importantNoticeWindow.gameObject.SetActive(value: false);
		}
		if (GameDataManager.Account.IsGuestUser)
		{
			return;
		}
		await MissionEventWindowInit();
		if (GameDataManager.BingoEventRemake.IsEnable())
		{
			IDisposableAsset<GameObject> obj2 = await AssetLoader.LoadAssetAsync<GameObject>("Bingo/Prefabs/BingoHome.prefab");
			obj2.AddTo(this);
			GameObject gameObject2 = UnityEngine.Object.Instantiate(obj2.Asset, m_eventParent.transform, worldPositionStays: false);
			m_bingoWindow = gameObject2.GetComponent<BingoWindow>();
		}
		if (GameDataManager.TenshiGowanEvent.IsEnable(takuTypeJudge: false))
		{
			IDisposableAsset<GameObject> obj3 = await AssetLoader.LoadAssetAsync<GameObject>("TenshiGowan/Prefabs/GowanHome.prefab");
			obj3.AddTo(this);
			GameObject gameObject3 = UnityEngine.Object.Instantiate(obj3.Asset, m_eventParent.transform, worldPositionStays: false);
			m_tenshiGowanWindow = gameObject3.GetComponent<TenshiGowanWindow>();
			m_tenshiGowanWindow.gameObject.SetActive(value: false);
		}
		if (GameDataManager.YellCampaign.IsEnable())
		{
			GameDataManager.YellCampaign.RefreshYellRank();
			IDisposableAsset<GameObject> obj4 = await AssetLoader.LoadAssetAsync<GameObject>("YellCampaign/Prefabs/YellHome.prefab");
			obj4.AddTo(this);
			GameObject gameObject4 = UnityEngine.Object.Instantiate(obj4.Asset, m_eventParent.transform, worldPositionStays: false);
			m_yellWindow = gameObject4.GetComponent<YellWindow>();
			m_yellWindow.gameObject.SetActive(value: false);
		}
		foreach (BaseEventGameData gameData in GameDataManager.EventTaku.AvailableModeGameDatas())
		{
			MFG.Types.EventTakuMaster.Item master = gameData.Master;
			IDisposableAsset<GameObject> obj5 = await AssetLoader.LoadAssetAsync<GameObject>(master.HomeWindowPrefabPath);
			obj5.AddTo(this);
			BaseEventTakuHomeWindow component = UnityEngine.Object.Instantiate(obj5.Asset, m_eventParent.transform, worldPositionStays: false).GetComponent<BaseEventTakuHomeWindow>();
			_eventTakus.Add(gameData, component);
			component.gameObject.SetActive(value: false);
			EventTaikyokuButtonNoticeDisp(master.HomeNoticeText);
		}
		if (CompetitionButton_IsNeedDisp())
		{
			await CompetitionButton_BannerReflesh();
			CompetitionButton_ModifyStatusText();
			m_competitionButton.gameObject.SetActive(CompetitionButton_IsNeedDisp());
			m_eventParent.transform.Find("CompetitionEntryWindow");
			IDisposableAsset<GameObject> obj6 = await AssetLoader.LoadAssetAsync<GameObject>("Competition/Common/Prefabs/CompetitionsEntry.prefab");
			obj6.AddTo(this);
			GameObject gameObject5 = UnityEngine.Object.Instantiate(obj6.Asset, m_eventParent.transform.Find("CompetitionEntryWindow"), worldPositionStays: false);
			m_competitionEntryWindow = gameObject5.GetComponent<CompetitionEntryWindow>();
			await m_competitionEntryWindow.Init(isOpenAtHome: true);
			IDisposableAsset<GameObject> obj7 = await AssetLoader.LoadAssetAsync<GameObject>("Competition/Common/Prefabs/CompeScoreWindow.prefab");
			obj7.AddTo(this);
			GameObject gameObject6 = UnityEngine.Object.Instantiate(obj7.Asset, m_eventParent.transform.Find("CompetitionScoreWindow"), worldPositionStays: false);
			m_competitionScoreWindow = gameObject6.GetComponent<CompetitionScoreWindow>();
			m_competitionScoreWindow._competitionEntryWindow = m_competitionEntryWindow;
			m_competitionScoreWindow._missionWindow = m_missionWindow;
			CompetitionEntriedBanner_Reflesh();
		}
		if (ShopCompetitionButton_IsNeedDisp())
		{
			await ShopCompetitionButton_BannerReflesh();
			ShopCompetitionButton_ModifyStatusText();
			m_competitionButton.gameObject.SetActive(ShopCompetitionButton_IsNeedDisp());
			IDisposableAsset<GameObject> obj8 = await AssetLoader.LoadAssetAsync<GameObject>($"ShopCompetition/{GameDataManager.ShopCompetition.CurrentGameEventType}/HomeWindow.prefab");
			obj8.AddTo(this);
			GameObject gameObject7 = UnityEngine.Object.Instantiate(obj8.Asset, m_eventParent.transform.Find("CompetitionScoreWindow"), worldPositionStays: false);
			m_shopCmpetitionHomeWindow = gameObject7.GetComponent<ShopCompetitionHomeWindow>();
		}
		if (JandriNoticeButton_IsNeedDisp())
		{
			m_fightGirlNewsNoticeButton.gameObject.SetActive(value: true);
			IDisposableAsset<GameObject> obj9 = await AssetLoader.LoadAssetAsync<GameObject>("FightGirlNews/Common/Prefabs/FightGirlNewsNoticeHome.prefab");
			obj9.AddTo(this);
			GameObject gameObject8 = UnityEngine.Object.Instantiate(obj9.Asset, m_eventParent.transform, worldPositionStays: false);
			m_fightGirlNewsNoticeWindow = gameObject8.GetComponent<FightGirlNewsNoticeWindow>();
			m_fightGirlNewsNoticeWindow.gameObject.SetActive(value: false);
		}
		if (GameDataManager.CharaStampEvent.IsHomeDisplay())
		{
			IDisposableAsset<GameObject> obj10 = await AssetLoader.LoadAssetAsync<GameObject>($"CharaStampEvent/Prefabs/{CharaStampMaster.GetCurrentStampID()}/CharaStampEventWindow.prefab");
			obj10.AddTo(this);
			GameObject gameObject9 = UnityEngine.Object.Instantiate(obj10.Asset, m_eventParent.transform, worldPositionStays: false);
			m_charaStampEventWindow = gameObject9.GetComponent<CharaStampEventWindow>();
			m_charaStampEventWindow.gameObject.SetActive(value: false);
		}
		await OneShotNoticeWindowInit();
		await OnlyPeraichiNoticeWindowInit();
	}

	private async UniTask OnlyPeraichiNoticeWindowInit()
	{
		List<MFG.Types.OnlyPeraichiNotice.Item> enableItems = OnlyPeraichiNoticeMaster.GetEnableItems();
		if (enableItems == null || enableItems.Count <= 0)
		{
			return;
		}
		foreach (MFG.Types.OnlyPeraichiNotice.Item item in enableItems)
		{
			IDisposableAsset<GameObject> obj = await AssetLoader.LoadAssetAsync<GameObject>(item.AssetPath);
			obj.AddTo(this);
			GameObject gameObject = UnityEngine.Object.Instantiate(obj.Asset, m_eventParent.transform, worldPositionStays: false);
			_onlyPeraichiNoticeWindows.Add(item, gameObject.GetComponent<OnlyPeraichiNoticeWindow>());
			_onlyPeraichiNoticeWindows[item].gameObject.SetActive(value: false);
			_onlyPeraichiNoticeWindows[item].Master = item;
		}
	}

	private async UniTask MissionEventWindowInit()
	{
		foreach (CategoryItem missionEvent in MissionCategoryMaster.MissionEvents)
		{
			if (missionEvent.IsEnable())
			{
				GameEventType eventType = missionEvent.MissionEventType;
				IDisposableAsset<GameObject> obj = await AssetLoader.LoadAssetAsync<GameObject>($"MissionEvent/{eventType}/HomeWindow.prefab");
				obj.AddTo(this);
				GameObject gameObject = UnityEngine.Object.Instantiate(obj.Asset, m_eventParent.transform, worldPositionStays: false);
				_missionEventWindows.Add(missionEvent, gameObject.GetComponent<MissionEventWindow>());
				_missionEventWindows[missionEvent].gameObject.SetActive(value: false);
				_missionEventWindows[missionEvent].eventType = eventType;
				await _missionEventWindows[missionEvent].SetProgress();
			}
		}
	}

	private async UniTask OneShotNoticeWindowInit()
	{
		List<MFG.Types.OneShotNotice.Item> enableItems = OneShotNoticeMaster.GetEnableItems();
		if (enableItems == null || enableItems.Count <= 0)
		{
			return;
		}
		foreach (MFG.Types.OneShotNotice.Item item in enableItems)
		{
			IDisposableAsset<GameObject> obj = await AssetLoader.LoadAssetAsync<GameObject>(item.AssetPath);
			obj.AddTo(this);
			GameObject gameObject = UnityEngine.Object.Instantiate(obj.Asset, m_eventParent.transform, worldPositionStays: false);
			_oneShotNoticeWindows.Add(item, gameObject.GetComponent<OneShotNoticeWindow>());
			_oneShotNoticeWindows[item].gameObject.SetActive(value: false);
			_oneShotNoticeWindows[item].Master = item;
		}
	}

	private async UniTask<Sprite> GetSprite(string addr)
	{
		IDisposableAsset<Sprite> obj = await AssetLoader.LoadAssetAsync<Sprite>(addr);
		obj.AddTo(this);
		return obj.Asset;
	}

	private async UniTask EvnetWindowButtonInit()
	{
		if (GameDataManager.Account.IsGuestUser)
		{
			return;
		}
		await MissionEventButtonInit();
		if (GameDataManager.BingoEventRemake.IsEnable())
		{
			GameObject button3 = UnityEngine.Object.Instantiate(m_btButtonPrefab, m_eventButtonParent.transform, worldPositionStays: false);
			Image component = button3.transform.Find("Bt").GetComponent<Image>();
			component.sprite = await GetSprite("Bingo/Textures/BtBingo.png");
			button3.GetComponent<Button>().OnClickAsObservable().Subscribe(delegate
			{
				m_bingoWindow.OpenWindow();
			})
				.AddTo(this);
		}
		if (GameDataManager.TenshiGowanEvent.IsEnable(takuTypeJudge: false))
		{
			GameObject button3 = UnityEngine.Object.Instantiate(m_btButtonPrefab, m_eventButtonParent.transform, worldPositionStays: false);
			Image component = button3.transform.Find("Bt").GetComponent<Image>();
			component.sprite = await GetSprite("TenshiGowan/Textures/BtModeGowan.png");
			button3.GetComponent<Button>().OnClickAsObservable().Subscribe(delegate
			{
				if (m_tenshiGowanWindow != null)
				{
					m_tenshiGowanWindow.OpenWindow();
				}
			})
				.AddTo(this);
		}
		if (GameDataManager.YellCampaign.IsEnable())
		{
			GameObject button3 = UnityEngine.Object.Instantiate(m_btButtonPrefab, m_eventButtonParent.transform, worldPositionStays: false);
			Image component = button3.transform.Find("Bt").GetComponent<Image>();
			component.sprite = await GetSprite("YellCampaign/Textures/BtModeYell.png");
			button3.GetComponent<Button>().OnClickAsObservable().Subscribe(delegate
			{
				if (m_yellWindow != null)
				{
					m_yellWindow.OpenWindow();
				}
			})
				.AddTo(this);
		}
		foreach (KeyValuePair<BaseEventGameData, BaseEventTakuHomeWindow> eventTaku in _eventTakus)
		{
			BaseEventGameData key = eventTaku.Key;
			BaseEventTakuHomeWindow window = eventTaku.Value;
			GameObject button3 = UnityEngine.Object.Instantiate(m_btButtonPrefab, m_eventButtonParent.transform, worldPositionStays: false);
			Image component = button3.transform.Find("Bt").GetComponent<Image>();
			component.sprite = await GetSprite(key.Master.HomeButtonIconSpritePath);
			button3.GetComponent<Button>().OnClickAsObservable().Subscribe(delegate
			{
				window.Open(eventTaku.Key);
			})
				.AddTo(this);
		}
		m_competitionButton.OnClickAsObservable().Subscribe(async delegate
		{
			if (!CompetitionButton_IsNeedDisp())
			{
				if (ShopCompetitionButton_IsNeedDisp())
				{
					await SoundManager.PlaySe("SFX_SYS_INPUT1");
					m_shopCmpetitionHomeWindow.Open();
				}
			}
			else
			{
				CompetitionInfo competitionInfo = GameDataManager.Competition.CompetitionInfos.Find((CompetitionInfo x) => x.CompetitionStatus == CompetitionStatus.InProgress || x.CompetitionStatus == CompetitionStatus.Aggregating);
				if (competitionInfo != null)
				{
					if (competitionInfo.IsEntried)
					{
						m_competitionScoreWindow.OpenAsScoreView(competitionInfo).Forget();
					}
					else if (competitionInfo.CompetitionStatus == CompetitionStatus.InProgress)
					{
						await SoundManager.PlaySe("SFX_SYS_INPUT1");
						await m_competitionEntryWindow.WindowOpen();
						CompetitionEntriedBanner_Reflesh();
					}
				}
			}
		}).AddTo(this);
		if (GameDataManager.CharaStampEvent.IsHomeDisplay())
		{
			GameObject button3 = UnityEngine.Object.Instantiate(m_btButtonPrefab, m_eventButtonParent.transform, worldPositionStays: false);
			Image component = button3.transform.Find("Bt").GetComponent<Image>();
			component.sprite = await GetSprite($"CharaStampEvent/Textures/{CharaStampMaster.GetCurrentStampID()}/BtModeCharaStamp.png");
			button3.GetComponent<Button>().OnClickAsObservable().Subscribe(delegate
			{
				if (m_charaStampEventWindow != null)
				{
					m_charaStampEventWindow.OpenWindow();
				}
			})
				.AddTo(this);
		}
		if (JandriNoticeButton_IsNeedDisp())
		{
			m_fightGirlNewsNoticeButton.gameObject.SetActive(value: true);
			m_fightGirlNewsNoticeButton.OnClickAsObservable().Subscribe(delegate
			{
				if (m_fightGirlNewsNoticeWindow != null)
				{
					m_fightGirlNewsNoticeWindow.OpenWindow();
				}
			}).AddTo(this);
		}
		else
		{
			m_fightGirlNewsNoticeButton.gameObject.SetActive(value: false);
		}
		if (GameDataManager.IsClearProEventEnable())
		{
			IDisposableAsset<GameObject> obj = await AssetLoader.LoadAssetAsync<GameObject>("ClearAppearanceEvent/Prefabs/HomeNotice.prefab");
			obj.AddTo(this);
			GameObject gameObject = UnityEngine.Object.Instantiate(obj.Asset, m_eventParent.transform, worldPositionStays: false);
			m_clearAppearanceEventNoticeWindow = gameObject.GetComponent<FightGirlNewsNoticeWindow>();
			m_clearAppearanceEventNoticeWindow.gameObject.SetActive(value: false);
			GameObject button3 = UnityEngine.Object.Instantiate(m_btButtonPrefab, m_eventButtonParent.transform, worldPositionStays: false);
			Image component = button3.transform.Find("Bt").GetComponent<Image>();
			component.sprite = await GetSprite("ClearAppearanceEvent/Textures/BtMode.png");
			button3.GetComponent<Button>().OnClickAsObservable().Subscribe(delegate
			{
				if (m_clearAppearanceEventNoticeWindow != null)
				{
					m_clearAppearanceEventNoticeWindow.OpenWindow();
				}
			})
				.AddTo(this);
		}
		if (GameDataManager.Vote.IsEnable)
		{
			VoteEventInfo gameData2 = GameDataManager.Vote.CurrentInfo;
			IDisposableAsset<GameObject> obj2 = await AssetLoader.LoadAssetAsync<GameObject>(gameData2.Master.HomeWindowPath);
			obj2.AddTo(this);
			GameObject gameObject2 = UnityEngine.Object.Instantiate(obj2.Asset, m_eventParent.transform, worldPositionStays: false);
			m_voteHomeWindow = gameObject2.GetComponent<VoteHomeWindow>();
			m_voteHomeWindow.gameObject.SetActive(value: false);
			GameObject button3 = UnityEngine.Object.Instantiate(m_btButtonPrefab, m_eventButtonParent.transform, worldPositionStays: false);
			Image component = button3.transform.Find("Bt").GetComponent<Image>();
			component.sprite = await GetSprite(gameData2.Master.BtHomePath);
			button3.GetComponent<Button>().OnClickAsObservable().Subscribe(delegate
			{
				if (m_voteHomeWindow != null)
				{
					m_voteHomeWindow.Open();
				}
			})
				.AddTo(this);
		}
		if (GameDataManager.ScrambleFestival.IsEnable)
		{
			ScrambleFestivalEventInfo gameData = GameDataManager.ScrambleFestival.CurrentInfo;
			IDisposableAsset<GameObject> disposableAsset = await AssetLoader.LoadAssetAsync<GameObject>(gameData.Master.LotteryHomeWindowPath);
			disposableAsset.AddTo(this);
			m_scrambleLotteryHomeWindow = UnityEngine.Object.Instantiate(disposableAsset.Asset, m_eventParent.transform, worldPositionStays: false);
			if (gameData.Master.FesType < FestivalType.SF003)
			{
				await m_scrambleLotteryHomeWindow.transform.GetComponent<ScrambleLotteryHomeWindow>().Init();
			}
			else
			{
				await m_scrambleLotteryHomeWindow.transform.GetComponent<JanponHomeWindow>().Init();
			}
			m_scrambleLotteryHomeWindow.SetActive(value: false);
			GameObject button3 = UnityEngine.Object.Instantiate(m_btButtonPrefab, m_eventButtonParent.transform, worldPositionStays: false);
			Image component = button3.transform.Find("Bt").GetComponent<Image>();
			component.sprite = await GetSprite(gameData.Master.BtLotteryPath);
			button3.GetComponent<Button>().OnClickAsObservable().Subscribe(delegate
			{
				if (m_scrambleLotteryHomeWindow != null)
				{
					if (gameData.Master.FesType >= FestivalType.SF003)
					{
						m_scrambleLotteryHomeWindow.transform.GetComponent<JanponHomeWindow>().Open(IsFirstDisp: false);
					}
					else
					{
						m_scrambleLotteryHomeWindow.transform.GetComponent<ScrambleLotteryHomeWindow>().Open(IsFirstDisp: false);
					}
				}
			})
				.AddTo(this);
			if (!gameData.Master.IsOnlyJanpon)
			{
				disposableAsset = await AssetLoader.LoadAssetAsync<GameObject>(gameData.Master.HomeWindowPath);
				disposableAsset.AddTo(this);
				GameObject gameObject3 = UnityEngine.Object.Instantiate(disposableAsset.Asset, m_eventParent.transform, worldPositionStays: false);
				m_scrambleHomeWindow = gameObject3.GetComponent<ScrambleHomeWindow>();
				m_scrambleHomeWindow.gameObject.SetActive(value: false);
				button3 = UnityEngine.Object.Instantiate(m_btButtonPrefab, m_eventButtonParent.transform, worldPositionStays: false);
				component = button3.transform.Find("Bt").GetComponent<Image>();
				component.sprite = await GetSprite(gameData.Master.BtHomePath);
				button3.GetComponent<Button>().OnClickAsObservable().Subscribe(delegate
				{
					if (m_scrambleHomeWindow != null)
					{
						m_scrambleHomeWindow.Open();
					}
				})
					.AddTo(this);
				disposableAsset = await AssetLoader.LoadAssetAsync<GameObject>(gameData.Master.TeamMatchHomeWindowPath);
				disposableAsset.AddTo(this);
				gameObject3 = UnityEngine.Object.Instantiate(disposableAsset.Asset, m_eventParent.transform, worldPositionStays: false);
				m_scrambleTeamMatchHomeWindow = gameObject3.GetComponent<ScrambleTeamMatchHomeWindow>();
				m_scrambleTeamMatchHomeWindow.gameObject.SetActive(value: false);
				button3 = UnityEngine.Object.Instantiate(m_btButtonPrefab, m_eventButtonParent.transform, worldPositionStays: false);
				component = button3.transform.Find("Bt").GetComponent<Image>();
				component.sprite = await GetSprite(gameData.Master.BtTeamMatchPath);
				button3.GetComponent<Button>().OnClickAsObservable().Subscribe(delegate
				{
					if (m_scrambleTeamMatchHomeWindow != null)
					{
						m_scrambleTeamMatchHomeWindow.Open(_catchphrasePlate, isNotice: false).Forget();
					}
				})
					.AddTo(this);
			}
		}
		if (GameDataManager.OthersEvent.CurrentOmikujiInfo.IsEnable)
		{
			IDisposableAsset<GameObject> obj3 = await AssetLoader.LoadAssetAsync<GameObject>(GameDataManager.OthersEvent.CurrentOmikujiInfo.Master.HomeWindowPath);
			obj3.AddTo(this);
			GameObject gameObject4 = UnityEngine.Object.Instantiate(obj3.Asset, m_eventParent.transform, worldPositionStays: false);
			m_omikujiHomeWindow = gameObject4.GetComponent<OmikujiHomeWindow>();
			m_omikujiHomeWindow.gameObject.SetActive(value: false);
			GameObject button3 = UnityEngine.Object.Instantiate(m_btButtonPrefab, m_eventButtonParent.transform, worldPositionStays: false);
			Image component = button3.transform.Find("Bt").GetComponent<Image>();
			component.sprite = await GetSprite(GameDataManager.OthersEvent.CurrentOmikujiInfo.Master.BtHomePath);
			button3.GetComponent<Button>().OnClickAsObservable().Subscribe(delegate
			{
				if (m_omikujiHomeWindow != null)
				{
					m_omikujiHomeWindow.Open();
				}
			})
				.AddTo(this);
		}
		if (GameDataManager.ScrambleFestival.IsResultEnable)
		{
			IDisposableAsset<GameObject> obj4 = await AssetLoader.LoadAssetAsync<GameObject>(GameDataManager.ScrambleFestival.CurrentInfo.Master.TeamMatchHomeWindowPath);
			obj4.AddTo(this);
			GameObject gameObject5 = UnityEngine.Object.Instantiate(obj4.Asset, m_eventParent.transform, worldPositionStays: false);
			m_scrambleTeamMatchHomeWindow = gameObject5.GetComponent<ScrambleTeamMatchHomeWindow>();
			m_scrambleTeamMatchHomeWindow.gameObject.SetActive(value: false);
		}
		Dictionary<MFG.Types.OnlyPeraichiNotice.Item, OnlyPeraichiNoticeWindow> onlyPeraichiNoticeWindows = _onlyPeraichiNoticeWindows;
		if (onlyPeraichiNoticeWindows == null || onlyPeraichiNoticeWindows.Count <= 0)
		{
			return;
		}
		foreach (KeyValuePair<MFG.Types.OnlyPeraichiNotice.Item, OnlyPeraichiNoticeWindow> peraichi in _onlyPeraichiNoticeWindows)
		{
			GameObject button3 = UnityEngine.Object.Instantiate(m_btButtonPrefab, m_eventButtonParent.transform, worldPositionStays: false);
			Image component = button3.transform.Find("Bt").GetComponent<Image>();
			component.sprite = await GetSprite(peraichi.Value.Master.BtHomePath);
			button3.GetComponent<Button>().OnClickAsObservable().Subscribe(async delegate
			{
				if (peraichi.Value != null)
				{
					await peraichi.Value.OpenWindow();
				}
			})
				.AddTo(this);
		}
	}

	private async UniTask MissionEventButtonInit()
	{
		foreach (CategoryItem item in _missionEventWindows.Keys)
		{
			if (!item.IsEnable())
			{
				continue;
			}
			GameObject button = UnityEngine.Object.Instantiate(m_btButtonPrefab, m_eventButtonParent.transform, worldPositionStays: false);
			Image component = button.transform.Find("Bt").GetComponent<Image>();
			component.sprite = await GetSprite($"MissionEvent/{item.MissionEventType}/BtMode.png");
			button.GetComponent<Button>().OnClickAsObservable().Subscribe(delegate
			{
				Debug.Log($"MissionEventButton : {item.Category}");
				if (_missionEventWindows[item] != null)
				{
					_missionEventWindows[item].OpenWindow();
				}
			})
				.AddTo(this);
		}
	}

	private async UniTask EventWindowFirstExplanation(CancellationToken token = default(CancellationToken))
	{
		if (GameDataManager.Account.IsGuestUser)
		{
			return;
		}
		if (GameDataManager.BingoEventRemake.IsEnable() && GameDataManager.BingoEventRemake.IsFirstBingoDisplay)
		{
			ResidentFlow.GetInstance().GetTimer().SetPause(pause: true);
			GameDataManager.BingoEventRemake.MakeBingoCard();
			m_bingoWindow.OpenWindow();
			await UniTask.WhenAny(UniTask.WaitWhile(() => m_bingoWindow.isOpen), UniTask.Delay(TimeSpan.FromSeconds(10.0), ignoreTimeScale: false, PlayerLoopTiming.Update, token));
			await m_bingoWindow.CloseWindow();
			ResidentFlow.GetInstance().GetTimer().SetPause(pause: false);
		}
		if (GameDataManager.TenshiGowanEvent.IsEnable(takuTypeJudge: false) && GameDataManager.PlayerConfig.IsTenshiGowanEventFirst)
		{
			ResidentFlow.GetInstance().GetTimer().SetPause(pause: true);
			m_tenshiGowanWindow.OpenWindow();
			GameDataManager.PlayerConfig.IsTenshiGowanEventFirst = false;
			await UniTask.WhenAny(UniTask.WaitWhile(() => m_tenshiGowanWindow.isOpen), UniTask.Delay(TimeSpan.FromSeconds(10.0), ignoreTimeScale: false, PlayerLoopTiming.Update, token));
			await m_tenshiGowanWindow.CloseWindow();
			ResidentFlow.GetInstance().GetTimer().SetPause(pause: false);
		}
		if (GameDataManager.YellCampaign.IsEnable() && GameDataManager.YellCampaign.GetCurrentCampaignInfo().IsFirstDisp)
		{
			ResidentFlow.GetInstance().GetTimer().SetPause(pause: true);
			m_yellWindow.OpenWindow();
			GameDataManager.YellCampaign.GetCurrentCampaignInfo().IsFirstDisp = false;
			await UniTask.WhenAny(UniTask.WaitWhile(() => m_yellWindow.isOpen), UniTask.Delay(TimeSpan.FromSeconds(10.0), ignoreTimeScale: false, PlayerLoopTiming.Update, token));
			await m_yellWindow.CloseWindow();
			ResidentFlow.GetInstance().GetTimer().SetPause(pause: false);
		}
		foreach (KeyValuePair<BaseEventGameData, BaseEventTakuHomeWindow> eventTaku in _eventTakus)
		{
			BaseEventGameData gameData = eventTaku.Key;
			BaseEventTakuHomeWindow window = eventTaku.Value;
			if (!gameData.IsFirstExplanationDisplayed)
			{
				ResidentFlow.GetInstance().GetTimer().SetPause(pause: true);
				window.Open(gameData);
				await UniTask.WhenAny(UniTask.WaitWhile(() => window.IsOpen), UniTask.Delay(TimeSpan.FromSeconds(30.0), ignoreTimeScale: false, PlayerLoopTiming.Update, token));
				await window.Close();
				ResidentFlow.GetInstance().GetTimer().SetPause(pause: false);
				gameData.IsFirstExplanationDisplayed = true;
			}
		}
		await MissionEventFirstDisp(token);
		if (GameDataManager.CharaStampEvent.IsEnable() && !GameDataManager.CharaStampEvent.GetCurrentEventInfo().IsHomeFirstDispFinish && m_charaStampEventWindow != null)
		{
			ResidentFlow.GetInstance().GetTimer().SetPause(pause: true);
			GameDataManager.CharaStampEvent.GetCurrentEventInfo().IsHomeFirstDispFinish = true;
			m_charaStampEventWindow.OpenWindow();
			await UniTask.WhenAny(UniTask.WaitWhile(() => m_charaStampEventWindow.isOpen), UniTask.Delay(TimeSpan.FromSeconds(10.0), ignoreTimeScale: false, PlayerLoopTiming.Update, token));
			await m_charaStampEventWindow.CloseWindow();
			ResidentFlow.GetInstance().GetTimer().SetPause(pause: false);
		}
		if (GameDataManager.IsClearProEventEnable() && m_clearAppearanceEventNoticeWindow != null && !GameDataManager.PlayerConfig.IsClearAppearanceEventFirstExplanationDisplayed)
		{
			GameDataManager.PlayerConfig.IsClearAppearanceEventFirstExplanationDisplayed = true;
			m_clearAppearanceEventNoticeWindow.OpenWindow();
			await UniTask.WhenAny(UniTask.WaitWhile(() => m_clearAppearanceEventNoticeWindow.isOpen), UniTask.Delay(TimeSpan.FromSeconds(10.0), ignoreTimeScale: false, PlayerLoopTiming.Update, token));
			m_clearAppearanceEventNoticeWindow.CloseWindow().Forget();
		}
		if (GameDataManager.Vote.IsEnable && m_voteHomeWindow != null && GameDataManager.Vote.CurrentInfo.IsFirstDisp)
		{
			GameDataManager.Vote.CurrentInfo.IsFirstDisp = false;
			m_voteHomeWindow.Open();
			await UniTask.WhenAny(UniTask.WaitWhile(() => m_voteHomeWindow.IsOpen), UniTask.Delay(TimeSpan.FromSeconds(10.0), ignoreTimeScale: false, PlayerLoopTiming.Update, token));
			m_voteHomeWindow.Close().Forget();
		}
		if (GameDataManager.ScrambleFestival.IsEnable && GameDataManager.ScrambleFestival.CurrentInfo.IsFirstDisp)
		{
			ResidentFlow.GetInstance().GetTimer().SetPause(pause: true);
			GameDataManager.ScrambleFestival.CurrentInfo.IsFirstDisp = false;
			if (GameDataManager.ScrambleFestival.CurrentInfo.Master.IsOnlyJanpon && m_scrambleLotteryHomeWindow != null)
			{
				if (GameDataManager.ScrambleFestival.CurrentInfo.Master.FesType >= FestivalType.SF003)
				{
					m_scrambleLotteryHomeWindow.transform.GetComponent<JanponHomeWindow>().Open(IsFirstDisp: true);
					await UniTask.WhenAny(UniTask.WaitWhile(() => m_scrambleLotteryHomeWindow.transform.GetComponent<JanponHomeWindow>().IsOpen), UniTask.Delay(TimeSpan.FromSeconds(10.0), ignoreTimeScale: false, PlayerLoopTiming.Update, token));
					m_scrambleLotteryHomeWindow.transform.GetComponent<JanponHomeWindow>().Close().Forget();
				}
				else
				{
					m_scrambleLotteryHomeWindow.transform.GetComponent<ScrambleLotteryHomeWindow>().Open(IsFirstDisp: true);
					await UniTask.WhenAny(UniTask.WaitWhile(() => m_scrambleLotteryHomeWindow.transform.GetComponent<ScrambleLotteryHomeWindow>().IsOpen), UniTask.Delay(TimeSpan.FromSeconds(10.0), ignoreTimeScale: false, PlayerLoopTiming.Update, token));
					m_scrambleLotteryHomeWindow.transform.GetComponent<ScrambleLotteryHomeWindow>().Close().Forget();
				}
			}
			else if (m_scrambleHomeWindow != null)
			{
				m_scrambleHomeWindow.Open();
				await UniTask.WhenAny(UniTask.WaitWhile(() => m_scrambleHomeWindow.IsOpen), UniTask.Delay(TimeSpan.FromSeconds(10.0), ignoreTimeScale: false, PlayerLoopTiming.Update, token));
				m_scrambleHomeWindow.Close().Forget();
			}
			ResidentFlow.GetInstance().GetTimer().SetPause(pause: false);
		}
		if (GameDataManager.OthersEvent.CurrentOmikujiInfo.IsEnable && m_omikujiHomeWindow != null && GameDataManager.OthersEvent.CurrentOmikujiInfo.IsOmikujiNoticePlay())
		{
			ResidentFlow.GetInstance().GetTimer().SetPause(pause: true);
			GameDataManager.OthersEvent.CurrentOmikujiInfo.NoticeDone();
			m_omikujiHomeWindow.Open(isFirstDisp: true);
			await UniTask.WhenAny(UniTask.WaitWhile(() => m_omikujiHomeWindow.IsOpen), UniTask.Delay(TimeSpan.FromSeconds(10.0), ignoreTimeScale: false, PlayerLoopTiming.Update, token));
			await UniTask.WaitWhile(() => m_omikujiHomeWindow.IsPlay);
			m_omikujiHomeWindow.Close().Forget();
			ResidentFlow.GetInstance().GetTimer().SetPause(pause: false);
		}
		string text = GameDataManager.ReceiveOnly.ScrambleFestivalScoreInfos?.Find((ScrambleFestivalInfo x) => x.EventType == GameEventType.Janpon4)?.RefleshDate ?? "";
		bool flag = !string.IsNullOrEmpty(text) && !text.Equals(GameDataManager.ScrambleFestival.CurrentInfo.NoticeTimeStamp);
		if (GameDataManager.ScrambleFestival.IsEnable && m_scrambleTeamMatchHomeWindow != null && flag)
		{
			ResidentFlow.GetInstance().GetTimer().SetPause(pause: true);
			GameDataManager.ScrambleFestival.CurrentInfo.NoticeTimeStamp = text;
			await m_scrambleTeamMatchHomeWindow.Open(_catchphrasePlate, isNotice: true);
			await UniTask.WhenAny(UniTask.WaitWhile(() => m_scrambleTeamMatchHomeWindow.IsOpen), UniTask.Delay(TimeSpan.FromSeconds(30.0), ignoreTimeScale: false, PlayerLoopTiming.Update, token));
			m_scrambleTeamMatchHomeWindow.Close().Forget();
			ResidentFlow.GetInstance().GetTimer().SetPause(pause: false);
		}
		if ((GameDataManager.ScrambleFestival.IsResultEnable && m_scrambleTeamMatchHomeWindow != null && GameDataManager.ScrambleFestival.CurrentInfo.IsResultDisp) || DebugFlag.IsActive(DebugFlag.Flag.TeamMatchResultForceActive))
		{
			ResidentFlow.GetInstance().GetTimer().SetPause(pause: true);
			GameDataManager.ScrambleFestival.CurrentInfo.IsResultDisp = false;
			await m_scrambleTeamMatchHomeWindow.Open(_catchphrasePlate, isNotice: true, isResult: true);
			await UniTask.WhenAny(UniTask.WaitWhile(() => m_scrambleTeamMatchHomeWindow.IsOpen), UniTask.Delay(TimeSpan.FromSeconds(30.0), ignoreTimeScale: false, PlayerLoopTiming.Update, token));
			m_scrambleTeamMatchHomeWindow.Close().Forget();
			ResidentFlow.GetInstance().GetTimer().SetPause(pause: false);
		}
		if (ShopCompetitionButton_IsNeedDisp() && m_shopCmpetitionHomeWindow != null)
		{
			ShopCompetitionInfo inProgressCompetitionInfo = GameDataManager.ShopCompetition.GetInProgressCompetitionInfo();
			if (inProgressCompetitionInfo != null && !GameDataManager.PlayerConfig.ShopCompetitionCheckedIDs.Contains(inProgressCompetitionInfo.CompetitionID))
			{
				ResidentFlow.GetInstance().GetTimer().SetPause(pause: true);
				GameDataManager.PlayerConfig.ShopCompetitionCheckedIDs.Add(inProgressCompetitionInfo.CompetitionID);
				m_shopCmpetitionHomeWindow.Open(isFirstDisplay: true);
				await UniTask.WhenAny(UniTask.WaitWhile(() => m_shopCmpetitionHomeWindow.IsOpen), UniTask.Delay(TimeSpan.FromSeconds(15.0), ignoreTimeScale: false, PlayerLoopTiming.Update, token));
				if (m_shopCmpetitionHomeWindow.IsOpen)
				{
					if (m_shopCmpetitionHomeWindow.IsAnonymousForceOpen)
					{
						await UniTask.WaitWhile(() => m_shopCmpetitionHomeWindow.IsOpen);
					}
					else
					{
						await m_shopCmpetitionHomeWindow.Close();
					}
				}
				ResidentFlow.GetInstance().GetTimer().SetPause(pause: false);
			}
		}
		await OneShotNoticeAppearance();
		await OnlyPeraichiNoticeAppearance();
	}

	private async UniTask MissionEventFirstDisp(CancellationToken token)
	{
		foreach (CategoryItem item in _missionEventWindows.Keys)
		{
			if (item.IsEnable() && !GameDataManager.Mission.MissionEventDispFinishes.Contains(item.MissionEventType))
			{
				ResidentFlow.GetInstance().GetTimer().SetPause(pause: true);
				_missionEventWindows[item].OpenWindow();
				await UniTask.WhenAny(UniTask.WaitWhile(() => _missionEventWindows[item].isOpen), UniTask.Delay(TimeSpan.FromSeconds(10.0), ignoreTimeScale: false, PlayerLoopTiming.Update, token));
				await _missionEventWindows[item].CloseWindow();
				GameDataManager.Mission.MissionEventDispFinishes.Add(item.MissionEventType);
				ResidentFlow.GetInstance().GetTimer().SetPause(pause: false);
			}
		}
	}

	private void EventWindowClose()
	{
		if (m_bingoWindow != null && m_bingoWindow.isOpen)
		{
			m_bingoWindow.CloseWindow().Forget();
		}
		if (m_tenshiGowanWindow != null && m_tenshiGowanWindow.isOpen)
		{
			m_tenshiGowanWindow.CloseWindow().Forget();
		}
		if (m_competitionEntryWindow != null && m_competitionEntryWindow.m_isOpen)
		{
			m_competitionEntryWindow.WindowClose();
		}
		if (m_competitionScoreWindow != null && m_competitionScoreWindow.IsOpen)
		{
			m_competitionScoreWindow.Close().Forget();
		}
		if (m_shopCmpetitionHomeWindow != null && m_shopCmpetitionHomeWindow.IsOpen)
		{
			m_shopCmpetitionHomeWindow.Close().Forget();
		}
		if (m_yellWindow != null && m_yellWindow.isOpen)
		{
			m_yellWindow.CloseWindow().Forget();
		}
		foreach (KeyValuePair<BaseEventGameData, BaseEventTakuHomeWindow> eventTaku in _eventTakus)
		{
			BaseEventTakuHomeWindow value = eventTaku.Value;
			if (value.IsOpen)
			{
				value.Close().Forget();
			}
		}
		foreach (CategoryItem key in _missionEventWindows.Keys)
		{
			if (_missionEventWindows[key] != null && _missionEventWindows[key].isOpen)
			{
				_missionEventWindows[key].CloseWindow().Forget();
			}
		}
		if (m_charaStampEventWindow != null && m_charaStampEventWindow.isOpen)
		{
			m_charaStampEventWindow.CloseWindow().Forget();
		}
		if (m_fightGirlNewsNoticeWindow != null && m_fightGirlNewsNoticeWindow.isOpen)
		{
			m_fightGirlNewsNoticeWindow.CloseWindow().Forget();
		}
		if (m_clearAppearanceEventNoticeWindow != null && m_clearAppearanceEventNoticeWindow.isOpen)
		{
			m_clearAppearanceEventNoticeWindow.CloseWindow().Forget();
		}
		if (m_voteHomeWindow != null && m_voteHomeWindow.IsOpen)
		{
			m_voteHomeWindow.Close().Forget();
		}
		if (m_omikujiHomeWindow != null && m_omikujiHomeWindow.IsOpen)
		{
			m_omikujiHomeWindow.Close().Forget();
		}
		if (m_scrambleHomeWindow != null && m_scrambleHomeWindow.IsOpen)
		{
			m_scrambleHomeWindow.Close().Forget();
		}
		if (m_scrambleTeamMatchHomeWindow != null && m_scrambleTeamMatchHomeWindow.IsOpen)
		{
			m_scrambleTeamMatchHomeWindow.Close().Forget();
		}
		if (!(m_scrambleLotteryHomeWindow != null))
		{
			return;
		}
		if (GameDataManager.ScrambleFestival.CurrentInfo.Master.FesType >= FestivalType.SF003)
		{
			if (m_scrambleLotteryHomeWindow.transform.GetComponent<JanponHomeWindow>().IsOpen)
			{
				m_scrambleLotteryHomeWindow.transform.GetComponent<JanponHomeWindow>().Close().Forget();
			}
		}
		else if (m_scrambleLotteryHomeWindow.transform.GetComponent<ScrambleLotteryHomeWindow>().IsOpen)
		{
			m_scrambleLotteryHomeWindow.transform.GetComponent<ScrambleLotteryHomeWindow>().Close().Forget();
		}
	}

	private async UniTask ExecIntroduction(CancellationToken cancellationToken)
	{
		if (GameDataManager.Account.IsFirstPlay && !GameDataManager.PlayerConfig.IsDispHelpFirstPlayAtHomeScene && !DebugFlag.IsActive(DebugFlag.Flag.SkipGuideDisp))
		{
			await GuideDisp(cancellationToken);
			GameDataManager.PlayerConfig.IsDispHelpFirstPlayAtHomeScene = true;
		}
		await ImportantNoticeAppearance(cancellationToken);
		await CharaPickupGachaNotice(cancellationToken);
		await FightGirlNewsAppearance(cancellationToken);
		await EventWindowFirstExplanation(cancellationToken);
		if (m_noticeButton.transform.Find("IconUpdate").gameObject.activeSelf)
		{
			await m_noticeWindow.ForcePlay(cancellationToken);
			await IsNewDispReflesh();
		}
		if (!GameDataManager.Account.IsGuestUser)
		{
			GameDataManager.Mission.ResultProgressAnimInit();
			await m_missionWindowAcquisition.Open(JudgeLocation.Home);
		}
		m_eventMask.SetActive(value: false);
	}

	private async UniTask GotoNextScene()
	{
		SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
		ResidentFlow.GetInstance().GetTimer().SetPause(pause: true);
		_canvas.GetComponent<GraphicRaycaster>().enabled = false;
		CancellationToken cancellationTokenOnDestroy = this.GetCancellationTokenOnDestroy();
		GameDataManager.Player.GetCurrentChara().MatchingBGEquip = (GameDataManager.Player.IsOdekake ? GameDataManager.Player.OdekakeCustomizeData.GetRandomMatchingBG() : GameDataManager.Customize.GetRandomMatchingBG());
		await GameDataManager.Player.RefleshTMakeVersionAndSave(cancellationTokenOnDestroy);
		GameDataManager.Mahjong.IsPrivateMatch = GameDataManager.PlayerConfig.IsPrivateMatch;
		SoundManager.StopVoiceAll().Forget();
		SoundManager.PlayMyCharaVoice("VOX_GXX_MENU2").Forget();
		if (GameDataManager.Account.IsFirstPlay && !DebugFlag.IsActive(DebugFlag.Flag.SkipGuideDisp))
		{
			GotoNextScene("Guide");
		}
		else
		{
			GotoNextScene("Matching");
		}
	}
}
