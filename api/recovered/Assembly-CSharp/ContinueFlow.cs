using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameContents;
using MFG.GameData;
using MFG.Sytem.Utilities;
using MFG.Types;
using TMPro;
using TestModeSkeleton;
using TestModeSkeleton.CabinetDef;
using TestModeSkeleton.Price;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class ContinueFlow : BaseFlow
{
	private enum ContinueMode
	{
		Normal,
		Smart
	}

	[SerializeField]
	private GameObject m_tenkeyWindow;

	[SerializeField]
	private Button m_continueButton;

	[SerializeField]
	private Button m_exitButton;

	[SerializeField]
	private List<Sprite> m_numSprites = new List<Sprite>();

	[SerializeField]
	private GameObject m_continueTimer;

	[SerializeField]
	private GameObject m_continueNumber;

	[SerializeField]
	private Canvas m_canvas;

	[SerializeField]
	private StandingCharaManager m_charaManager;

	[SerializeField]
	private List<GameObject> m_priceGameObjects;

	[SerializeField]
	private ContinuePriceGauge m_continuePriceGauge;

	[SerializeField]
	private GameObject m_continueWindow;

	[SerializeField]
	private GameObject m_continueAttention;

	[SerializeField]
	private GameObject m_revengeWindow;

	[SerializeField]
	private Button m_exitButtonAtContinueWindow;

	[SerializeField]
	private ViewManager.ViewData2D m_charaViewData;

	[SerializeField]
	private ViewManager.ViewData2D m_continueWindowViewData;

	[SerializeField]
	private Button m_breakTimeButton;

	[SerializeField]
	private TextMeshProUGUI m_breakTimeLengthText;

	[SerializeField]
	private BreakTimeWindow m_breakTimeWindow;

	private bool m_isPaseliAvailable;

	private bool _isFreePlay;

	private (Fraction priceRate, int lowerScore, int upperScore) m_priceRate;

	private PriceInfo m_priceInfo = new PriceInfo();

	[SerializeField]
	private bool m_isDebug;

	[SerializeField]
	private TakuType m_debugTakuType;

	[SerializeField]
	private bool m_debugPaseliAvailable;

	[SerializeField]
	private int m_debugScore;

	[SerializeField]
	private bool m_isEndless;

	private CancellationTokenSource m_gaugeCancellationTokenSource = new CancellationTokenSource();

	private ContinueMode m_continueMode;

	private bool m_breakTimeUsed;

	[SerializeField]
	private AccountGameData m_accountGameData => GameDataManager.Account;

	protected override async UniTask OnInitialize(CancellationToken cancellationToken)
	{
		CountDownTimer_Reset();
		await RefleshPrice();
		try
		{
			await m_charaManager.Load(GameDataManager.Player.GetCurrentChara().CostumeItemEquip);
			ResidentFlow.GetInstance().GetHeader().SetDisp(disp: true);
			ResidentFlow.GetInstance().GetHeader().SetStatusDisp(disp: false);
			ResidentFlow.GetInstance().GetHeader().SetSceneName("舞台下");
			await SoundManager.PlayBgm("BGM_RESULT3");
			await SoundManager.PlayMyCharaVoice("VOX_GXX_CONTINUE1");
			GameDataManager.Account.IsTaikyokuEnd = false;
			m_continueButton.OnClickAsObservable().Subscribe(async delegate
			{
				if (!_isNextSceneDecided)
				{
					_isNextSceneDecided = true;
					await SoundManager.PlaySe("SFX_SYS_INPUT1");
					await SoundManager.PlayMyCharaVoice("VOX_GXX_CONTINUE2");
					m_continueTimer.SetActive(value: false);
					m_continueNumber.SetActive(value: false);
					m_breakTimeButton.gameObject.SetActive(value: false);
					if (_isFreePlay)
					{
						GameDataManager.Account.ParticipationBillingEvent.Reserve((mode: CONSUME_MODE.CONTINUE_CREDIT, count: 100));
						await GameDataManager.YellCampaign.YellStack(GameDataManager.YellCampaign.BeforeConsumeMode);
						GotoNextScene(isContinue: true);
					}
					else
					{
						DialogManager.Instance.CreateConsumeDialog(m_canvas.transform).Set("", GAME_TYPE.CONTINUE, PRICE_TYPE.PRICE_1, 1, (GachaSeriesName)(-1), "コンティニュー料金を支払います", OnEnd, m_tenkeyWindow, resetTimer: true, isRenewAccessDate: false, isJandriActiveIconDisp: true, enableJandriPurchaseMenu: false, isEntry: false, m_priceInfo.ContinuePrice <= 0).Forget();
					}
				}
			}).AddTo(this);
			m_breakTimeButton.OnClickAsObservable().Subscribe(async delegate
			{
				if (!_isNextSceneDecided)
				{
					_isNextSceneDecided = true;
					m_breakTimeUsed = true;
					m_breakTimeButton.gameObject.SetActive(value: false);
					await SoundManager.PlaySe("SFX_SYS_INPUT1");
					ResidentFlow.GetInstance().GetTimer().SetPause(pause: true);
					ResidentFlow.GetInstance().GetHeader().SetDisp(disp: false);
					await m_breakTimeWindow.Open(cancellationToken);
					await UniTask.WaitWhile(() => m_breakTimeWindow.IsOpen);
					ResidentFlow.GetInstance().GetHeader().SetDisp(disp: true);
					ResidentFlow.GetInstance().GetHeader().SetStatusDisp(disp: false);
					if (m_breakTimeWindow.IsAuthed)
					{
						CountDownTimer_Reset();
						ResidentFlow.GetInstance().GetTimer().StartTimer();
						m_breakTimeButton.gameObject.SetActive(value: false);
						_isNextSceneDecided = false;
					}
					else
					{
						GotoNextScene(isContinue: false);
					}
				}
			}).AddTo(this);
			m_exitButtonAtContinueWindow.OnClickAsObservable().Subscribe(delegate
			{
				OnExitButton();
			}).AddTo(this);
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
			if (m_continueMode != 0)
			{
				await ExecSmartContinue();
			}
			else
			{
				ExecNormalContinue();
			}
			bool isBreakTimeEnable = await BreakTimeButton_IsDispEnable();
			m_breakTimeButton.gameObject.SetActive(isBreakTimeEnable);
			m_breakTimeLengthText.text = $"{CabinetManager.Game.Data.BreakTimeLength * 60}";
			await UniTask.Delay(1000, ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
			m_breakTimeButton.interactable = isBreakTimeEnable;
		}
		catch (OperationCanceledException)
		{
			Debug.Log(GetType().Name + MethodBase.GetCurrentMethod().Name + ": OperationCanceledException Catched.");
		}
	}

	private void CountDownTimer_Reset()
	{
		CountDownTimer timer = ResidentFlow.GetInstance().GetTimer();
		timer.SetTimer(20f, disp: false);
		timer.ResetTimer(disp: false);
		timer.AlertSeEnable(isEnable: true);
		timer.DispTime();
	}

	private async UniTask<bool> BreakTimeButton_IsDispEnable()
	{
		if (CabinetManager.Game.Data.BreakTime == GameOptionsDef.BreakTime.Off)
		{
			return false;
		}
		if (GameDataManager.Account.IsCardless)
		{
			bool flag = !GameDataManager.Account.IsGuestUser && !m_breakTimeUsed && GameUtility.IsKamunityBootEnable() && !GameUtility.IsKonastaOrNetcafeBehavior;
			if (flag)
			{
				flag = await QRLoginManager.QRLibraryEnableCheck();
			}
			return flag;
		}
		return !GameDataManager.Account.IsGuestUser && !m_breakTimeUsed;
	}

	private void ExecNormalContinue()
	{
		ResidentFlow.GetInstance().GetTimer().StartTimer();
		m_continueAttention.SetActive(GameDataManager.Mahjong.TakuType != TakuType.Nima);
		RevengeSetActive(isActive: true);
		m_continueWindow.SetActive(value: true);
	}

	private async UniTask ExecSmartContinue()
	{
		await m_continuePriceGauge.Play(m_gaugeCancellationTokenSource.Token);
		await UniTask.Delay(1000);
		ResidentFlow.GetInstance().GetTimer().StartTimer();
		if (!_isNextSceneDecided)
		{
			m_continueAttention.SetActive(GameDataManager.Mahjong.TakuType != TakuType.Nima);
			RevengeSetActive(isActive: true);
			m_continueWindow.SetActive(value: true);
		}
	}

	private async void OnExitButton()
	{
		if (!_isNextSceneDecided)
		{
			_isNextSceneDecided = true;
			m_gaugeCancellationTokenSource.Cancel();
			await SoundManager.PlaySe("SFX_SYS_CANCEL1");
			GotoNextScene(isContinue: false);
		}
	}

	private async void Debug_Play()
	{
		await RefleshPrice();
		if (m_continueMode == ContinueMode.Normal)
		{
			ExecNormalContinue();
		}
		else
		{
			ExecSmartContinue().Forget();
		}
	}

	private void RevengeSetActive(bool isActive)
	{
		m_revengeWindow.transform.Find("RevengeNum").GetComponent<TextMeshProUGUI>().text = $"{GameDataManager.PlayerConfig.RevengeContinueValue}";
		m_revengeWindow.SetActive(isActive && GameDataManager.PlayerConfig.RevengeContinueValue > 0);
	}

	private async UniTask RefleshPrice()
	{
		m_isPaseliAvailable = PaseliUtility.IsEnablePaseli;
		bool isCreditAvailable = !GameUtility.IsKonastaOrNetcafeBehavior;
		_isFreePlay = GameDataManager.Account.IsForceFreePlayUser || GameUtility.IsFreePlay();
		m_continueMode = ((GameDataManager.Mahjong.TakuType != TakuType.Nima && !_isFreePlay) ? ContinueMode.Smart : ContinueMode.Normal);
		if (m_continueMode == ContinueMode.Smart)
		{
			RevengeSetActive(isActive: false);
			m_continueWindow.SetActive(value: false);
			m_continuePriceGauge.gameObject.SetActive(value: true);
		}
		else
		{
			m_continueAttention.SetActive(GameDataManager.Mahjong.TakuType != TakuType.Nima);
			RevengeSetActive(isActive: true);
			m_continueWindow.SetActive(value: true);
			m_continuePriceGauge.gameObject.SetActive(value: false);
		}
		m_charaViewData.Reflesh((ViewType)m_continueMode);
		m_continueWindowViewData.Reflesh((ViewType)m_continueMode);
		m_priceRate = GameUtility.ContinuePriceUtility.GetPriceRate(GameDataManager.Mahjong.TakuType, GameDataManager.Mahjong.EventType, GameDataManager.Mahjong.IntScore());
		await PaseliUtility.RefleshPrice(m_priceRate.priceRate, isFetchExternalBasicRateInfo: false);
		m_priceInfo = CabinetManager.VirtualCoin.CurrentPrice;
		Debug.Log($"【スマートコンティニュー】価格:{m_priceInfo.ContinuePrice} スコア:{GameDataManager.Mahjong.Score} 価格率:{(float)m_priceRate.priceRate.Numerator / (float)m_priceRate.priceRate.Denominator:F1} 下限スコア:{m_priceRate.lowerScore} 上限スコア:{m_priceRate.upperScore} 下限スコア価格:{m_priceInfo.ContinueLowerPrice} 上限スコア価格:{m_priceInfo.ContinueUpperPrice}");
		GameUtility.GetPrice(GAME_MODE.CONTINUE, CONSUME_TYPE.CREDIT);
		GameUtility.GetPrice(GAME_MODE.CONTINUE, CONSUME_TYPE.PASELI);
		int num;
		int num2;
		if (m_priceInfo.ContinuePrice <= 0 || _isFreePlay)
		{
			num = 0;
			num2 = 0;
			m_priceGameObjects[0].SetActive(value: false);
			m_priceGameObjects[1].SetActive(value: true);
			m_priceGameObjects[1].GetComponentInChildren<TextMeshProUGUI>().text = "<color=#FFE800>無料</color><color=#FFFFFF>でコンティニューできます</color>";
			m_priceGameObjects[2].SetActive(value: false);
		}
		else
		{
			num = GameUtility.GetPrice(GAME_MODE.CONTINUE, CONSUME_TYPE.CREDIT);
			num2 = GameUtility.GetPrice(GAME_MODE.CONTINUE, CONSUME_TYPE.PASELI);
			m_priceGameObjects[0].SetActive(value: true);
			m_priceGameObjects[0].GetComponentInChildren<TextMeshProUGUI>().text = "コンティニューには";
			m_priceGameObjects[1].SetActive(value: true);
			string arg = ((num >= 2) ? "CREDITS" : "CREDIT") ?? "";
			string text = (isCreditAvailable ? $"<color=#FFE800>{num} {arg}</color>" : "") ?? "";
			string arg2 = (isCreditAvailable ? " または " : "");
			string text2 = (m_isPaseliAvailable ? $"{arg2}<color=#62FF00>PASELI {num2}円(税込)</color>" : "") ?? "";
			m_priceGameObjects[1].GetComponentInChildren<TextMeshProUGUI>().text = text + text2;
			m_priceGameObjects[2].SetActive(value: true);
			m_priceGameObjects[2].GetComponentInChildren<TextMeshProUGUI>().text = "必要です";
		}
		m_continuePriceGauge.Init(GameDataManager.Mahjong.IntScore(), num2, num, m_priceRate.lowerScore, m_priceRate.upperScore, m_priceInfo.ContinueLowerPrice, m_priceInfo.ContinueUpperPrice, GameUtility.GetPrice(GAME_MODE.CONTINUE, CONSUME_TYPE.CREDIT), m_isPaseliAvailable);
	}

	protected override void OnUpdate()
	{
		SetNumSprite(ResidentFlow.GetInstance().GetTimer().GetDispTime());
		if (ResidentFlow.GetInstance().GetTimer().IsEnd() && !_isNextSceneDecided)
		{
			_isNextSceneDecided = true;
			GotoNextScene(isContinue: false);
		}
	}

	private void OnDestroy()
	{
		Debug.Log("ContinueFlow.OnDestroy");
	}

	private async void OnEnd(bool is_success, CONSUME_MODE mode, ConsumeWindow.EndType endType)
	{
		if (is_success)
		{
			switch (mode)
			{
			case CONSUME_MODE.CONTINUE_CREDIT:
				GameDataManager.Account.ParticipationBillingEvent.Reserve((mode: mode, count: 100));
				GameDataManager.Account.IsPremiumPlayer = false;
				await GameDataManager.YellCampaign.YellStack(mode);
				break;
			case CONSUME_MODE.CONTINUE_PASELI:
				GameDataManager.Account.ParticipationBillingEvent.Reserve((mode: mode, count: m_priceInfo.ContinueFactoryPrice));
				GameDataManager.Account.IsPremiumPlayer = false;
				await GameDataManager.YellCampaign.YellStack(mode);
				break;
			case CONSUME_MODE.PREMIUM_CONTINUE_PASELI:
				GameDataManager.Account.ParticipationBillingEvent.Reserve((mode: mode, count: m_priceInfo.PremiumContinueFactoryPrice));
				GameDataManager.Account.IsPremiumPlayer = true;
				await GameDataManager.YellCampaign.YellStack(mode);
				break;
			case CONSUME_MODE.PREMIUM_CONTINUE_CREDIT:
				GameDataManager.Account.ParticipationBillingEvent.Reserve((mode: mode, count: (m_priceInfo.PremiumContinueFactoryPrice > 100) ? 200 : 100));
				GameDataManager.Account.IsPremiumPlayer = true;
				await GameDataManager.YellCampaign.YellStack(mode);
				break;
			case CONSUME_MODE.CONTINUE_FREE:
				Debug.Log("無料コンティニュー");
				GameDataManager.Account.IsPremiumPlayer = false;
				break;
			default:
				Debug.LogWarning("未対応のモード");
				break;
			}
		}
		GotoNextScene(is_success);
	}

	private async void GotoNextScene(bool isContinue)
	{
		if (isContinue)
		{
			CancellationToken cancellationTokenOnDestroy = this.GetCancellationTokenOnDestroy();
			string voiceLabel = SoundManager.GetMyCharaVoiceLabel("VOX_GXX_CONTINUE2");
			await UniTask.WaitWhile(() => SoundManager.IsPlay(voiceLabel), PlayerLoopTiming.Update, cancellationTokenOnDestroy);
			if (m_priceInfo.ContinuePrice <= 0 || _isFreePlay)
			{
				GameUtility.SendPcbEvent(PCB_EVENT.EVENT.GAME_CONTINUE_FREE);
			}
			CabinetManager.Book.AddPlayCount(BookKeepingSettings.Variable.Continuation);
			await GameDataManager.ExecLoadAtContinue();
			GameDataManager.ResetForContinueStart();
			if (GameUtility.IsKonastaOrNetcafeBehavior)
			{
				Cloudstrap.UserEvent.Begin(Cloudstrap.UserEventType.Play);
			}
			await SoundManager.StopBgmWithFadeout();
			if (!GameDataManager.Account.IsGuestUser && GameDataManager.Story.JudgeAndSetEventStoryPlay())
			{
				GotoNextScene("StoryPlay", enableDecidedCheck: false);
			}
			else
			{
				GotoNextScene(GameDataManager.Competition.GetNextSceneWhenGotoHome(), enableDecidedCheck: false);
			}
		}
		else
		{
			GotoNextScene("Gameover", enableDecidedCheck: false);
		}
	}

	private void SetNumSprite(int num)
	{
		if (num < 0)
		{
			num = 0;
		}
		int index = num % 10;
		int index2 = num / 10 % 10;
		m_continueNumber.transform.Find("Ten").GetComponent<Image>().sprite = m_numSprites[index2];
		m_continueNumber.transform.Find("One").GetComponent<Image>().sprite = m_numSprites[index];
	}
}
