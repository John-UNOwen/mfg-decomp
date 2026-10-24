using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GameContents;
using IO;
using KAMUNITY;
using Konami.System.Net;
using MFG.GameData;
using MFG.GameRequest;
using MFG.Types;
using MFG.Types.Story;
using TCSNET;
using TMPro;
using TestModeSkeleton;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class CardEntryScene : BaseFlow
{
	private enum Step
	{
		NextSceneDecided,
		Idle,
		EntryWindow_Wait,
		EntryWindow_Inquire,
		EntryWindow_InquireFinishedProcess,
		CompletelyNewCard_SelectMenuInit,
		CompletelyNewCard_SelectMenuWait,
		CompletelyNewCard_HandoverInfoInit,
		CompletelyNewCard_HandoverInfoWait,
		NormalNewCard_AgreeInit,
		NormalNewCard_AgreeWait,
		CompletelyNewCard_AgreeInit,
		CompletelyNewCard_AgreeWait,
		CompletelyNewCard_InputPassAndAuthInit,
		CompletelyNewCard_InputPassAndAuthWait,
		InputPassAndAuthInit,
		InputPassAndAuthWait,
		CompletelyNewCard_AfterRegisteredInfoInit,
		CompletelyNewCard_AfterRegisteredInfoWait,
		EaCoin_CheckinInit,
		EaCoin_CheckinWait,
		EaCoin_CheckinEnd,
		PreFinish,
		ErrorFinish,
		Finish,
		End
	}

	private enum FreeStartBehaviorType
	{
		None,
		NormalFree,
		Campaign,
		RestartRight
	}

	[SerializeField]
	private GameObject m_tenkeyWindow;

	[SerializeField]
	private Button m_TitleButton;

	[SerializeField]
	private Button m_skipBuuton;

	[SerializeField]
	private Transform m_windowRoot;

	[SerializeField]
	private TextMeshProUGUI m_cardNumberText;

	[SerializeField]
	private Button m_cardNumberDispButton;

	[SerializeField]
	private GameObject m_waitCardEntryObj;

	[SerializeField]
	private GameObject m_ePassNumberObj;

	[SerializeField]
	private GameObject m_newCardObj;

	[SerializeField]
	private Button m_newCardEntryButton;

	[SerializeField]
	private Button m_newCardEndButton;

	[SerializeField]
	private GameObject m_infoObj;

	[SerializeField]
	private TextMeshProUGUI m_cardNumberInfoText;

	[SerializeField]
	private TextMeshProUGUI m_cardNumberOverInfoText;

	[SerializeField]
	private Button m_infoOkButton;

	[SerializeField]
	private GameObject m_agreeObj;

	[SerializeField]
	private Button m_newEntryYesButton;

	[SerializeField]
	private Button m_newEntryNoButton;

	[SerializeField]
	private GameObject m_afterInfoObj;

	[SerializeField]
	private TextMeshProUGUI m_cardNumberAfterInfoText;

	[SerializeField]
	private TextMeshProUGUI m_cardNumberOverAfterInfoText;

	[SerializeField]
	private GameObject m_cardNumberobj;

	[SerializeField]
	private Button m_afterInfoSkipButton;

	[SerializeField]
	private CardEntryManager m_cardEntry;

	[SerializeField]
	private CautionSet m_caution;

	[SerializeField]
	private GameObject m_penaltyWindow;

	[SerializeField]
	private Button m_penaltyCloseButton;

	[SerializeField]
	private GameObject m_qrDisplay;

	[SerializeField]
	private Image m_qrImage;

	[SerializeField]
	private Button m_btQrChange;

	private bool qrButtonPushed;

	private bool bNewCard;

	private bool bEndState;

	private bool m_bSucess;

	private bool m_bBinded;

	private bool m_bAgree;

	private bool m_bCardEntryConnecting;

	private bool m_bCardNumberDisp;

	private bool m_bQRChanging;

	private string m_cardId = "";

	private string m_cardNumber = "";

	private PaseliWindow m_paseliWindow;

	private Step m_state = Step.Idle;

	private CountDownTimer m_timer;

	private AvsTypes.CardType m_cardType = AvsTypes.CardType.ICCARD;

	private AvsEa3CoinTypes.EA3_EACOIN_ERROR m_checkInStatus = AvsEa3CoinTypes.EA3_EACOIN_ERROR.PROCESSING;

	private float m_timeCount;

	private bool m_voiceCalled;

	[SerializeField]
	private Canvas m_canvas;

	protected override async UniTask DebugLogin(CancellationToken cancellationToken = default(CancellationToken))
	{
	}

	protected override async UniTask OnInitialize(CancellationToken cancellationToken = default(CancellationToken))
	{
		ResidentFlow.GetInstance().IsTestModeEffective = false;
		if (GameUtility.IsKonastaOrNetcafeBehavior)
		{
			m_TitleButton.GetComponentInChildren<TextMeshProUGUI>().text = "終了する";
		}
		await ResidentFlow.GetInstance().m_fadePanel.GetComponent<CanvasGroup>().DOFade(0f, 0.5f).ToAwaiter();
		ResidentFlow.GetInstance().GetHeader().SetDisp(disp: true);
		ResidentFlow.GetInstance().GetHeader().SetStatusDisp(disp: false);
		ResidentFlow.GetInstance().GetHeader().SetSceneName("認証");
		SoundManager.PlayBgm("BGM_MENU").Forget();
		await ServiceURL_Reflesh(cancellationToken);
		m_TitleButton.onClick.AddListener(delegate
		{
			BackToTitle();
		});
		m_skipBuuton.onClick.AddListener(async delegate
		{
			await DecideGuestEntry(isPlayButtonSe: true, cancellationToken);
		});
		m_infoOkButton.onClick.AddListener(delegate
		{
			m_infoObj.SetActive(value: false);
			m_bCardEntryConnecting = false;
			m_state = Step.ErrorFinish;
		});
		m_newEntryYesButton.onClick.AddListener(delegate
		{
			m_agreeObj.SetActive(value: false);
			SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
			if (m_state == Step.NormalNewCard_AgreeWait)
			{
				m_bAgree = true;
			}
			else
			{
				m_newCardObj.SetActive(value: false);
				m_state = Step.CompletelyNewCard_InputPassAndAuthInit;
			}
		});
		m_newEntryNoButton.onClick.AddListener(delegate
		{
			m_agreeObj.SetActive(value: false);
			m_bCardEntryConnecting = false;
			m_state = Step.ErrorFinish;
		});
		m_afterInfoSkipButton.onClick.AddListener(delegate
		{
			m_afterInfoObj.SetActive(value: false);
			SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
			m_state = Step.EaCoin_CheckinInit;
		});
		m_newCardEntryButton.onClick.AddListener(delegate
		{
			m_newCardObj.SetActive(value: false);
			SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
			m_state = Step.CompletelyNewCard_AgreeInit;
		});
		m_newCardEndButton.onClick.AddListener(delegate
		{
			m_newCardObj.SetActive(value: false);
			SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
			m_state = Step.CompletelyNewCard_HandoverInfoInit;
		});
		m_cardNumberDispButton.onClick.AddListener(delegate
		{
			DispCardNumber();
		});
		qrButtonPushed = false;
		m_btQrChange.OnClickAsObservable().Subscribe(async delegate
		{
			if (!qrButtonPushed)
			{
				qrButtonPushed = true;
				Sprite sprite = await QRLoginManager.QRBeginToDisplay();
				if (sprite != null)
				{
					m_btQrChange.gameObject.SetActive(value: false);
					m_qrImage.sprite = sprite;
					m_qrImage.gameObject.SetActive(value: true);
				}
				else
				{
					m_qrDisplay.SetActive(value: false);
					SingletonBehaviour<Loading>.Instance.Connecting("二次元バーコードの表示に失敗しました");
					await UniTask.Delay(TimeSpan.FromSeconds(3.0));
					SingletonBehaviour<Loading>.Instance.DisConnect();
				}
			}
		}).AddTo(this);
		m_paseliWindow = DialogManager.Instance.CreatePaseliDialog(m_windowRoot);
		m_paseliWindow.gameObject.SetActive(value: false);
		m_cardNumberText.text = "**** **** **** ****";
		m_newCardObj.SetActive(value: false);
		m_ePassNumberObj.SetActive(value: false);
		m_infoObj.SetActive(value: false);
		m_agreeObj.SetActive(value: false);
		m_afterInfoObj.SetActive(value: false);
		m_waitCardEntryObj.SetActive(value: true);
		bool flag = string.IsNullOrEmpty(GameDataManager.Account.CardID);
		if (flag)
		{
			flag = await QRLoginManager.QRLibraryEnableCheck();
		}
		bool active = flag;
		m_qrDisplay.SetActive(active);
		SingletonBehaviour<Loading>.Instance.DisConnect();
		m_timer = ResidentFlow.GetInstance().GetTimer();
		m_timer.SetTimer(GameDef.QR_ENTRY_TIMEOUT, disp: true);
		m_timer.StartTimer();
	}

	protected override async UniTask OnExecute(CancellationToken cancellationToken)
	{
		bool flag = false;
		if (GameUtility.IsNetcafeBehavior)
		{
			flag = GameUtility.IsNetcafeForceFreeGuestPlayUser(GameDataManager.Account.CardID);
			GameDataManager.Account.IsForceFreePlayUser = flag;
		}
		if (flag)
		{
			m_state = Step.EntryWindow_Wait;
			GameUtility.SendPcbEvent(PCB_EVENT.EVENT.GAME_MODE);
			GameUtility.SendPcbEvent(PCB_EVENT.EVENT.GUEST_START);
			GameUtility.SendPcbEvent(PCB_EVENT.EVENT.GAME_START_FREE);
			Bi2xInput.IsTrigger(BUTTON.TEST);
			DecideGuestEntry(isPlayButtonSe: false, cancellationToken).Forget();
			return;
		}
		if (!string.IsNullOrEmpty(GameDataManager.Account.CardID))
		{
			m_cardType = GameDataManager.Account.CardType;
			m_cardId = GameDataManager.Account.CardID;
			m_TitleButton.interactable = false;
			m_skipBuuton.interactable = false;
		}
		else
		{
			m_waitCardEntryObj.SetActive(value: true);
			SoundManager.PlayVoice("VOX_G01_SYSTEM1").Forget();
		}
		m_state = Step.EntryWindow_Wait;
		GameUtility.SendPcbEvent(PCB_EVENT.EVENT.GAME_MODE);
		Bi2xInput.IsTrigger(BUTTON.TEST);
		await CoCheckMain(cancellationToken);
	}

	protected override void OnUpdate()
	{
		if ((m_state == Step.InputPassAndAuthInit || m_state == Step.CompletelyNewCard_InputPassAndAuthInit) && m_timer.IsEnd() && !bEndState)
		{
			if (m_cardEntry.IsEnd())
			{
				ExecConsume();
			}
			else
			{
				BackToTitle();
			}
			bEndState = true;
		}
	}

	private void OnDestroy()
	{
		try
		{
			QRLogin.End();
		}
		catch
		{
		}
	}

	private async UniTask CoCheckMain(CancellationToken cancellationToken = default(CancellationToken))
	{
		XrpcResponseHelper<XrpcModuleCardmngTypes.CardmngInquireResponse> responseCardMngInquire = new XrpcResponseHelper<XrpcModuleCardmngTypes.CardmngInquireResponse>();
		try
		{
			while (!m_timer.IsEnd() && !cancellationToken.IsCancellationRequested)
			{
				switch (m_state)
				{
				case Step.EntryWindow_Wait:
					m_timeCount += Time.deltaTime;
					if (m_qrDisplay.activeSelf)
					{
						(int, string) cardTypeID = QRLogin.GetCardTypeID();
						if (cardTypeID.Item1 != -1 && !string.IsNullOrEmpty(cardTypeID.Item2))
						{
							GameDataManager.Account.CardType = (AvsTypes.CardType)cardTypeID.Item1;
							GameDataManager.Account.CardID = cardTypeID.Item2;
							GameDataManager.Account.IsCardless = true;
							QRLogin.End();
						}
					}
					if (Bi2xInput.Instance.IsCardOn() || !string.IsNullOrEmpty(GameDataManager.Account.CardID))
					{
						m_TitleButton.interactable = false;
						m_skipBuuton.interactable = false;
						m_timer.SetPause(pause: true);
						if (string.IsNullOrEmpty(GameDataManager.Account.CardID))
						{
							if (Bi2xInput.Instance.IsCardOn())
							{
								GameDataManager.Account.CardID = Bi2xInput.Instance.GetCardID();
								GameDataManager.Account.CardType = Bi2xInput.Instance.GetCardType();
								GameDataManager.Account.IsCardless = false;
								m_cardType = GameDataManager.Account.CardType;
								m_cardId = GameDataManager.Account.CardID;
							}
						}
						else
						{
							m_cardType = GameDataManager.Account.CardType;
							m_cardId = GameDataManager.Account.CardID;
						}
						SoundManager.StopVoiceAllWithFadeout().Forget();
						SoundManager.PlaySe("SFX_SYS_EPASS1").Forget();
						SoundManager.PlayVoice("VOX_G01_SYSTEM3").Forget();
						m_cardNumber = GameUtility.ConvertCardNumberToUserCode(m_cardType, m_cardId);
						GameDataManager.Account.CardNumber = string.Copy(m_cardNumber);
						if (m_cardNumber.Length > 0)
						{
							m_cardNumberText.text = m_cardNumber.Substring(0, 4) + " **** **** ****";
						}
						SetNewEntryCardNumber();
						m_bCardEntryConnecting = true;
						SingletonBehaviour<Loading>.Instance.Connecting(TextManager.Get(TextManager.KEY.ENTRY_PROCESSING));
						Bi2xInput.Instance.ICReaderOff();
						m_state = Step.EntryWindow_Inquire;
					}
					else if (m_timeCount > 5f && !m_voiceCalled)
					{
						m_voiceCalled = true;
						SoundManager.PlayVoice("VOX_G01_SYSTEM2").Forget();
					}
					break;
				case Step.EntryWindow_Inquire:
					XrpcModuleCardmng.Inquire(m_cardType, m_cardId, update: false, responseCardMngInquire.Callback, null);
					await responseCardMngInquire.CoWaitForXrpcFinish();
					await UniTask.Delay(TimeSpan.FromSeconds(1.0));
					SingletonBehaviour<Loading>.Instance.DisConnect();
					if (responseCardMngInquire.isError)
					{
						if (responseCardMngInquire.status.status_code == 112)
						{
							m_state = Step.EntryWindow_InquireFinishedProcess;
							break;
						}
						m_bCardEntryConnecting = false;
						SoundManager.PlayVoice("VOX_G01_SYSTEM6").Forget();
						if (responseCardMngInquire.status.status_code == 109 || responseCardMngInquire.status.status_code == 110)
						{
							SingletonBehaviour<Loading>.Instance.Connecting(TextManager.Get(TextManager.KEY.ENTRY_NO_ID));
						}
						else
						{
							SingletonBehaviour<Loading>.Instance.Connecting(TextManager.Get(TextManager.KEY.ENTRY_NETWORK_ERROR));
						}
						await UniTask.Delay(TimeSpan.FromSeconds(3.0));
						SingletonBehaviour<Loading>.Instance.DisConnect();
						m_state = Step.ErrorFinish;
					}
					else
					{
						GameDataManager.Account.RefID = responseCardMngInquire.result.refid;
						GameDataManager.Account.DataID = responseCardMngInquire.result.dataid;
						XrpcResponseHelper<TcsXrpcModuleTypes.UpdateReferResponse> xrpcResponseHelper = new XrpcResponseHelper<TcsXrpcModuleTypes.UpdateReferResponse>();
						TcsXrpcModule.XrpcMethodUpdateRefer.Request(xrpcResponseHelper.Callback, null);
						await xrpcResponseHelper.CoWaitForXrpcFinish();
						SingletonBehaviour<Loading>.Instance.Connecting(TextManager.Get(TextManager.KEY.ENTRY_CONFIRM));
						await UniTask.Delay(TimeSpan.FromSeconds(1.0));
						SingletonBehaviour<Loading>.Instance.DisConnect();
						m_state = Step.EntryWindow_InquireFinishedProcess;
					}
					break;
				case Step.EntryWindow_InquireFinishedProcess:
					SoundManager.PlaySe("SFX_SYS_EPASS2").Forget();
					SoundManager.PlayVoice("VOX_G01_SYSTEM5").Forget();
					if (responseCardMngInquire.result == null)
					{
						SingletonBehaviour<Loading>.Instance.Connecting(TextManager.Get(TextManager.KEY.ENTRY_CONFIRM));
						await UniTask.Delay(TimeSpan.FromSeconds(1.0));
						SingletonBehaviour<Loading>.Instance.DisConnect();
						bNewCard = true;
						m_waitCardEntryObj.SetActive(value: false);
						m_state = Step.CompletelyNewCard_SelectMenuInit;
					}
					else if (responseCardMngInquire.result.refid == null)
					{
						m_state = Step.CompletelyNewCard_SelectMenuInit;
					}
					else
					{
						m_waitCardEntryObj.SetActive(value: false);
						m_bBinded = true;
						if (!responseCardMngInquire.result.binded || responseCardMngInquire.result.expired)
						{
							m_bBinded = false;
							m_state = Step.NormalNewCard_AgreeInit;
						}
						else
						{
							m_state = Step.InputPassAndAuthInit;
						}
					}
					break;
				case Step.NormalNewCard_AgreeInit:
					m_timer.SetTimer(GameDef.CONSUME_TIME, disp: true);
					m_timer.SetPause(pause: false);
					m_agreeObj.SetActive(value: true);
					m_state = Step.NormalNewCard_AgreeWait;
					break;
				case Step.NormalNewCard_AgreeWait:
					if (m_bAgree)
					{
						m_agreeObj.SetActive(value: false);
						m_state = Step.InputPassAndAuthInit;
					}
					break;
				case Step.CompletelyNewCard_SelectMenuInit:
					if (bNewCard)
					{
						m_timer.SetTimer(GameDef.CARD_NEW_ENTRY_TIMEOUT, disp: true);
						m_timer.SetPause(pause: false);
						m_newCardObj.SetActive(value: true);
						m_state = Step.CompletelyNewCard_SelectMenuWait;
					}
					else
					{
						m_waitCardEntryObj.SetActive(value: false);
						m_state = Step.CompletelyNewCard_AgreeInit;
					}
					break;
				case Step.CompletelyNewCard_HandoverInfoInit:
					m_timer.SetTimer(GameDef.CARD_ENTRY_INFO_DISP_TIME, disp: true);
					m_timer.SetPause(pause: false);
					m_newCardObj.SetActive(value: false);
					m_infoObj.SetActive(value: true);
					m_state = Step.CompletelyNewCard_HandoverInfoWait;
					break;
				case Step.CompletelyNewCard_AgreeInit:
					m_timer.SetTimer(GameDef.CONSUME_TIME, disp: true);
					m_timer.SetPause(pause: false);
					m_agreeObj.SetActive(value: true);
					m_state = Step.CompletelyNewCard_AgreeWait;
					break;
				case Step.CompletelyNewCard_InputPassAndAuthInit:
					m_agreeObj.SetActive(value: false);
					m_newCardObj.SetActive(value: false);
					m_ePassNumberObj.SetActive(value: true);
					m_timer.SetTimer(GameDef.CARD_ENTRY_INPUT_NEW_PASS_TIME, disp: true);
					m_timer.SetPause(pause: false);
					await m_cardEntry.CreateNewEntryTenkey(m_cardType, m_cardId);
					await m_cardEntry.CoNewCardEntryRequest();
					m_state = Step.CompletelyNewCard_InputPassAndAuthWait;
					m_timer.SetPause(pause: true);
					break;
				case Step.CompletelyNewCard_InputPassAndAuthWait:
					if (m_cardEntry.IsEnd() && !m_cardEntry.IsErrorStopState)
					{
						m_ePassNumberObj.SetActive(value: false);
						await UniTask.Delay(TimeSpan.FromSeconds(1.0));
						m_timer.SetPause(pause: true);
						SingletonBehaviour<Loading>.Instance.Connecting();
						var (flag2, key2) = await GameDataManager.ExecLoadAtCardEntry();
						SingletonBehaviour<Loading>.Instance.DisConnect();
						if (!flag2)
						{
							KAMUNITY.Logger.LogWarning("CoLoad error");
							SingletonBehaviour<Loading>.Instance.Connecting(TextManager.Get(key2));
							await UniTask.Delay(TimeSpan.FromSeconds(3.0));
							SingletonBehaviour<Loading>.Instance.DisConnect();
							m_cardEntry.Exit();
							m_bCardEntryConnecting = false;
							m_state = Step.ErrorFinish;
						}
						else
						{
							m_state = Step.CompletelyNewCard_AfterRegisteredInfoInit;
						}
					}
					else
					{
						if (m_cardEntry.m_errorStopStateType != CardEntryManager.ErrorStopStateType.NetworkError)
						{
							await UniTask.Delay(TimeSpan.FromSeconds(1.0));
						}
						else
						{
							SingletonBehaviour<Loading>.Instance.Connecting(TextManager.Get(TextManager.KEY.ENTRY_NETWORK_ERROR));
							await UniTask.Delay(TimeSpan.FromSeconds(3.0));
							SingletonBehaviour<Loading>.Instance.DisConnect();
						}
						m_bCardEntryConnecting = false;
						m_state = Step.ErrorFinish;
					}
					break;
				case Step.InputPassAndAuthInit:
					m_timer.SetTimer(GameDef.CARD_ENTRY_INPUT_PASS_TIME, disp: true);
					m_timer.SetPause(pause: false);
					await m_cardEntry.CreateTenkey(m_bBinded);
					m_ePassNumberObj.SetActive(value: true);
					await m_cardEntry.CoCardEntryRequest();
					m_timer.SetPause(pause: true);
					m_state = Step.InputPassAndAuthWait;
					break;
				case Step.InputPassAndAuthWait:
					if (m_cardEntry.IsEnd() && !m_cardEntry.IsErrorStopState)
					{
						m_ePassNumberObj.SetActive(value: false);
						m_timer.SetPause(pause: true);
						SingletonBehaviour<Loading>.Instance.Connecting();
						var (flag, key) = await GameDataManager.ExecLoadAtCardEntry();
						SingletonBehaviour<Loading>.Instance.DisConnect();
						if (!flag)
						{
							KAMUNITY.Logger.LogWarning("CoLoad error");
							SingletonBehaviour<Loading>.Instance.Connecting(TextManager.Get(key));
							await UniTask.Delay(TimeSpan.FromSeconds(3.0));
							SingletonBehaviour<Loading>.Instance.DisConnect();
							m_cardEntry.Exit();
							m_bCardEntryConnecting = false;
							m_state = Step.ErrorFinish;
						}
						else
						{
							GameUtility.SendPcbEvent(PCB_EVENT.EVENT.PLAYERDATA_READ);
							m_state = Step.EaCoin_CheckinInit;
						}
					}
					else
					{
						if (m_cardEntry.m_errorStopStateType != CardEntryManager.ErrorStopStateType.NetworkError)
						{
							await UniTask.Delay(TimeSpan.FromSeconds(1.0));
						}
						else
						{
							SingletonBehaviour<Loading>.Instance.Connecting(TextManager.Get(TextManager.KEY.ENTRY_NETWORK_ERROR));
							await UniTask.Delay(TimeSpan.FromSeconds(3.0));
							SingletonBehaviour<Loading>.Instance.DisConnect();
						}
						m_bCardEntryConnecting = false;
						m_state = Step.ErrorFinish;
					}
					break;
				case Step.CompletelyNewCard_AfterRegisteredInfoInit:
					GameUtility.SendPcbEvent(PCB_EVENT.EVENT.PLAYERDATA_READ);
					m_timer.SetTimer(GameDef.CARD_ENTRY_INFO_DISP_TIME, disp: true);
					m_timer.SetPause(pause: false);
					m_ePassNumberObj.SetActive(value: false);
					m_afterInfoObj.SetActive(value: true);
					m_state = Step.CompletelyNewCard_AfterRegisteredInfoWait;
					break;
				case Step.EaCoin_CheckinInit:
					m_cardEntry.Exit();
					if (PaseliUtility.IsEnable())
					{
						if (responseCardMngInquire.result != null)
						{
							if (responseCardMngInquire.result.ecflag)
							{
								m_timer.SetPause(pause: true);
								PaseliUtility.CheckInRequest(m_cardType, m_cardId, m_cardEntry.Password);
								m_paseliWindow.Open(AvsEa3CoinTypes.EA3_EACOIN_ERROR.PROCESSING);
								m_state = Step.EaCoin_CheckinWait;
							}
							else
							{
								m_bCardEntryConnecting = false;
								m_state = Step.PreFinish;
							}
						}
						else
						{
							m_bCardEntryConnecting = false;
							m_state = Step.Finish;
						}
					}
					else
					{
						m_bCardEntryConnecting = false;
						m_state = Step.PreFinish;
					}
					break;
				case Step.EaCoin_CheckinWait:
					if (PaseliUtility.CheckInStatus(out m_bSucess, out m_checkInStatus))
					{
						m_state = Step.EaCoin_CheckinEnd;
					}
					break;
				case Step.EaCoin_CheckinEnd:
					if (m_bSucess)
					{
						m_paseliWindow.Open((AvsEa3CoinTypes.EA3_EACOIN_ERROR)0);
					}
					else
					{
						m_paseliWindow.Open(m_checkInStatus);
					}
					m_state = Step.PreFinish;
					break;
				case Step.PreFinish:
					m_state = Step.Finish;
					break;
				case Step.ErrorFinish:
					if (m_paseliWindow == null || !m_bCardEntryConnecting)
					{
						BackToTitle();
						m_state = Step.End;
					}
					break;
				case Step.Finish:
					if (m_paseliWindow == null || !m_bCardEntryConnecting)
					{
						ExecConsume(cancellationToken);
						m_state = Step.End;
					}
					break;
				}
				await UniTask.Yield();
			}
			if (cancellationToken.IsCancellationRequested)
			{
				Debug.Log("Operation Canceled.");
			}
			else if (m_cardEntry.IsEnd())
			{
				ExecConsume();
			}
			else
			{
				BackToTitle();
			}
		}
		catch (OperationCanceledException ex)
		{
			Debug.Log("CardEntry: OperationCanceledException Catched. " + ex.Message);
		}
	}

	private void DispCardNumber()
	{
		SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
		if (!m_bCardNumberDisp && m_cardNumber.Length > 0)
		{
			m_cardNumberText.text = m_cardNumber;
			m_bCardNumberDisp = true;
		}
	}

	private void DispCardNumberAfterInfo()
	{
		SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
		if (!m_bCardNumberDisp)
		{
			for (int i = 4; i < m_cardNumber.Length; i += 5)
			{
				m_cardNumber = m_cardNumber.Insert(i, " ");
			}
			m_cardNumberAfterInfoText.text = m_cardNumber;
			m_cardNumberOverAfterInfoText.text = m_cardNumber;
			m_bCardNumberDisp = true;
		}
		else
		{
			m_cardNumberAfterInfoText.text = m_cardNumber.Substring(0, 4) + " **** **** ****";
			m_cardNumberOverAfterInfoText.text = m_cardNumber.Substring(0, 4) + " **** **** ****";
			m_bCardNumberDisp = false;
		}
	}

	private void SetNewEntryCardNumber()
	{
		for (int i = 4; i < m_cardNumber.Length; i += 5)
		{
			m_cardNumber = m_cardNumber.Insert(i, " ");
		}
		m_cardNumberInfoText.text = m_cardNumber;
		m_cardNumberOverInfoText.text = m_cardNumber;
		m_cardNumberAfterInfoText.text = m_cardNumber;
		m_cardNumberOverAfterInfoText.text = m_cardNumber;
	}

	private async UniTask DecideGuestEntry(bool isPlayButtonSe = true, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (m_state == Step.EntryWindow_Wait)
		{
			m_TitleButton.interactable = false;
			m_skipBuuton.interactable = false;
			if (isPlayButtonSe)
			{
				SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
			}
			m_timer.SetPause(pause: true);
			m_state = Step.NextSceneDecided;
			GameDataManager.Account.NotifyGuestUserStart();
			SingletonBehaviour<Loading>.Instance.Connecting();
			bool item = (await GameDataManager.ExecLoadAtCardEntry()).Item1;
			SingletonBehaviour<Loading>.Instance.DisConnect();
			Bi2xInput.Instance.ICReaderOff();
			if (!item)
			{
				KAMUNITY.Logger.LogWarning("CoLoad error");
				SingletonBehaviour<Loading>.Instance.Connecting(TextManager.Get(TextManager.KEY.ENTRY_NETWORK_ERROR));
				await UniTask.Delay(TimeSpan.FromSeconds(3.0));
				SingletonBehaviour<Loading>.Instance.DisConnect();
				BackToTitle();
			}
			else
			{
				GameUtility.SendPcbEvent(PCB_EVENT.EVENT.PLAYERDATA_READ);
				ExecConsume(cancellationToken);
				m_state = Step.End;
			}
		}
	}

	public async void EntryWithDebugUserID()
	{
	}

	private async UniTask ExecConsume(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (GameDataManager.ReceiveOnly.IsPenaltyPlayer || DebugFlag.IsActive(DebugFlag.Flag.IsPenaltyPlayerCheck))
		{
			m_penaltyCloseButton.OnClickAsObservable().Subscribe(async delegate
			{
				SoundManager.PlaySe("SFX_SYS_CANCEL1").Forget();
				m_penaltyWindow.SetActive(value: false);
			}).AddTo(this);
			ResidentFlow.GetInstance().GetTimer().SetDisp(disp: false);
			m_penaltyWindow.SetActive(value: true);
			await UniTask.WhenAny(UniTask.WaitWhile(() => m_penaltyWindow.activeSelf), UniTask.Delay(TimeSpan.FromSeconds(20.0)));
			if (m_penaltyWindow.activeSelf)
			{
				m_penaltyWindow.SetActive(value: false);
			}
			ResidentFlow.GetInstance().GetTimer().SetDisp(disp: true);
		}
		var (flag, currentDateTimeOffset) = await GameUtility.FreePlayCampaignUtility.IsEnableFreePlay();
		if (GameDataManager.Account.IsForceFreePlayUser || GameUtility.IsFreePlay())
		{
			CardEntryScene cardEntryScene = this;
			CancellationToken cancellationToken2 = cancellationToken;
			await cardEntryScene.ExecFreePlay(FreeStartBehaviorType.NormalFree, default(DateTimeOffset), cancellationToken2);
		}
		else if (GameUtility.IsKonastaOrNetcafeBehavior && Cloudstrap.UserEvent.NeedCompensation(Cloudstrap.UserEventType.Play))
		{
			CardEntryScene cardEntryScene2 = this;
			CancellationToken cancellationToken2 = cancellationToken;
			await cardEntryScene2.ExecFreePlay(FreeStartBehaviorType.RestartRight, default(DateTimeOffset), cancellationToken2);
		}
		else if (flag)
		{
			await ExecFreePlay(FreeStartBehaviorType.Campaign, currentDateTimeOffset, cancellationToken);
		}
		else
		{
			ConsumeDialog_Open();
		}
	}

	private async UniTask ExecFreePlay(FreeStartBehaviorType freeStartBehaviorType, DateTimeOffset currentDateTimeOffset = default(DateTimeOffset), CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!(await RefleshExpirationDate()))
		{
			OnEnd(is_success: false, CONSUME_MODE.NONE, ConsumeWindow.EndType.NetworkError);
			return;
		}
		string text = freeStartBehaviorType switch
		{
			FreeStartBehaviorType.RestartRight => "以前のゲームが正常に動作しなかった可能性がある為\n無料でプレーできます。", 
			FreeStartBehaviorType.Campaign => "無料プレーキャンペーン条件達成！\n無料でプレーできます。", 
			_ => "", 
		};
		if (freeStartBehaviorType == FreeStartBehaviorType.Campaign)
		{
			GameDataManager.Player.LastestFreePlayCampaignDateTime = currentDateTimeOffset;
		}
		if (!string.IsNullOrEmpty(text))
		{
			DialogWindow dialogWindow = DialogManager.Instance.CreateNormalDialog(m_canvas.transform);
			dialogWindow.SetMessage(text);
			bool isEnd = false;
			dialogWindow.Open(5f, enable_skip: true, delegate
			{
				isEnd = true;
			});
			await UniTask.WaitUntil(() => isEnd, PlayerLoopTiming.Update, cancellationToken);
		}
		OnEnd(is_success: true, CONSUME_MODE.NONE, ConsumeWindow.EndType.Success);
	}

	private void ConsumeDialog_Open()
	{
		ConsumeWindow consumeWindow = DialogManager.Instance.CreateConsumeDialog(m_canvas.transform);
		bool enableJandriPurchaseMenu = false;
		consumeWindow.Set("", GAME_TYPE.START, PRICE_TYPE.PRICE_1, 1, (GachaSeriesName)(-1), "プレー料金支払い", OnEnd, m_tenkeyWindow, resetTimer: true, !GameDataManager.Account.IsGuestUser, isJandriActiveIconDisp: true, enableJandriPurchaseMenu, isEntry: true).Forget();
	}

	private async void OnEnd(bool is_success, CONSUME_MODE mode, ConsumeWindow.EndType endType)
	{
		if (is_success)
		{
			if (mode == CONSUME_MODE.JANDRI_PASELI)
			{
				GameDataManager.CoopData.RightStats.Add(new RightStat
				{
					ObjectID = "OID_CoopJanDri000",
					Available = true,
					ExpireAt = DateTimeOffsetManager.GetJSTTime().AddDays(1.0).ToString()
				});
				ConsumeDialog_Open();
			}
			else
			{
				GameDataManager.Account.IsPremiumPlayer = mode == CONSUME_MODE.PREMIUM_START_PASELI || mode == CONSUME_MODE.PREMIUM_START_CREDIT;
				await AdvanceToGameScene(mode);
			}
			return;
		}
		if (endType == ConsumeWindow.EndType.NetworkError)
		{
			SingletonBehaviour<Loading>.Instance.Connecting(TextManager.Get(TextManager.KEY.ENTRY_NETWORK_ERROR));
			await UniTask.Delay(TimeSpan.FromSeconds(3.0));
			SingletonBehaviour<Loading>.Instance.DisConnect();
		}
		SoundManager.StopBgmWithFadeout().Forget();
		SoundManager.StopVoiceAllWithFadeout().Forget();
		SingletonBehaviour<Loading>.Instance.DisConnect();
		if (!string.IsNullOrEmpty(GameDataManager.Account.AppSessionID))
		{
			try
			{
				await RequestApiBase<Logout, Logout.ResultData>.Send(enableErrorScreenTransition: false);
			}
			catch (Exception arg)
			{
				Debug.Log($"CardEntryScene.OnEnd Logout exception:{arg}");
			}
		}
		PaseliUtility.CheckOut();
		PaseliUtility.Clear();
		GameDataManager.Reset();
		ResidentFlow.GetInstance().GetTouchEffect().SetEnable(enable: true);
		ResidentFlow.GetInstance().GetMouseCursor().SetEnable(enable: true);
		if (GameUtility.IsNetcafeBehavior)
		{
			GotoNextScene("ScopeClose");
		}
		else
		{
			GotoNextScene("Logo");
		}
	}

	private async UniTask AdvanceToGameScene(CONSUME_MODE mode)
	{
		CabinetManager.Book.StartGamePlayTime();
		CONSUME_MODE cONSUME_MODE = mode;
		if ((uint)cONSUME_MODE <= 2u || cONSUME_MODE == CONSUME_MODE.PREMIUM_START_PASELI || cONSUME_MODE == CONSUME_MODE.PREMIUM_START_CREDIT)
		{
			if (mode == CONSUME_MODE.NONE)
			{
				mode = CONSUME_MODE.START_CREDIT;
			}
			GameDataManager.Account.ParticipationBillingEvent.Reserve((mode: mode, count: 1));
			CabinetManager.Book.AddPlayCount(BookKeepingSettings.Variable.GameStarting);
			await GameDataManager.YellCampaign.YellStack(mode);
		}
		else
		{
			Debug.LogError("想定外のケース");
		}
		if (GameUtility.IsKonastaOrNetcafeBehavior)
		{
			Cloudstrap.UserEvent.Begin(Cloudstrap.UserEventType.Play);
		}
		float userHeadPhoneVolume = GameDataManager.PlayerConfig.headPhoneVolume;
		if (GameUtility.IsKonastaOrNetcafeBehavior)
		{
			userHeadPhoneVolume = 0.5f;
		}
		SoundManager.SetUserHeadPhoneVolume(userHeadPhoneVolume);
		LEDManager.Instance.Apply(GameDataManager.PlayerConfig);
		ResidentFlow.GetInstance().GetHeader().SetDisp(disp: false);
		ResidentFlow.GetInstance().GetTimer().SetDisp(disp: false);
		if (!GameUtility.IsKonastaOrNetcafeBehavior)
		{
			await m_caution.Play();
		}
		GameDataManager.Story.SetupForceStory();
		GameDataManager.Mission.MissionUpdate();
		GameDataManager.MusicGacha.AcquisitionProcess();
		if (GameDataManager.Account.IsFirstPlay)
		{
			if (GameDataManager.Account.IsGuestUser)
			{
				GameDataManager.Player.PlayerName = "ゲスト";
				ResidentFlow.GetInstance().GetHeader().SetPlayerName(GameDataManager.Player.PlayerName);
				GameDataManager.Story.m_playStoryScriptID = StoryID.StoryEvent0000;
				GameDataManager.Story.m_forcePlaying = true;
				GotoNextScene("StoryPlay");
			}
			else
			{
				GotoNextScene("NewRegistration");
			}
			return;
		}
		if (!GameDataManager.PlayerConfig.IsNameDecided)
		{
			GameDataManager.Account.IsFirstPlay = true;
			GotoNextScene("NewRegistration");
			return;
		}
		GameDataManager.Version.Save().Forget();
		if (!GameDataManager.Account.IsGuestUser)
		{
			if (!GameDataManager.IsCharaSelectable(GameDataManager.Player.SelectChara))
			{
				GameDataManager.Player.SelectChara = CharaType.Chara01;
			}
			if (GameDataManager.Story.JudgeAndSetEventStoryPlay())
			{
				GotoNextScene("StoryPlay");
				return;
			}
			Debug.Log("イベントストーリー持ってるけど、「イベント開催していない」or「イベントストーリーもう読んだ」のでHomeに飛ばす");
			GotoNextScene(GameDataManager.Competition.GetNextSceneWhenGotoHome());
		}
		else
		{
			GotoNextScene(GameDataManager.Competition.GetNextSceneWhenGotoHome());
		}
	}

	public async void BackToTitle()
	{
		m_TitleButton.interactable = false;
		m_skipBuuton.interactable = false;
		m_state = Step.NextSceneDecided;
		SoundManager.PlaySe("SFX_SYS_CANCEL1").Forget();
		SoundManager.StopBgmWithFadeout().Forget();
		SoundManager.StopVoiceAllWithFadeout().Forget();
		if (!string.IsNullOrEmpty(GameDataManager.Account.AppSessionID))
		{
			try
			{
				await RequestApiBase<Logout, Logout.ResultData>.Send(enableErrorScreenTransition: false);
			}
			catch (Exception arg)
			{
				Debug.Log($"CardEntryScene.BackToTitle Logout exception:{arg}");
			}
		}
		SingletonBehaviour<Loading>.Instance.DisConnect();
		GameDataManager.Reset();
		ResidentFlow.GetInstance().GetTouchEffect().SetEnable(enable: true);
		ResidentFlow.GetInstance().GetMouseCursor().SetEnable(enable: true);
		if (GameUtility.IsNetcafeBehavior)
		{
			GotoNextScene("ScopeClose");
		}
		else
		{
			GotoNextScene("Logo");
		}
	}

	private async UniTask ServiceURL_Reflesh(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (GameUtility.IsKamunityBootEnable())
		{
			XrpcResponseHelper<TcsXrpcModuleTypes.GetServiceListResponse> xrpcHelper = new XrpcResponseHelper<TcsXrpcModuleTypes.GetServiceListResponse>();
			TcsXrpcModule.XrpcMethodGetServiceList.Request(xrpcHelper.Callback, null);
			await xrpcHelper.CoWaitForXrpcFinish();
			if (!xrpcHelper.isError)
			{
				SystemConfigGameData.FrontServerUrl = xrpcHelper.result.ServiceUrl;
				return;
			}
			SingletonBehaviour<Loading>.Instance.Connecting(TextManager.Get(TextManager.KEY.ENTRY_NETWORK_ERROR));
			await UniTask.Delay(TimeSpan.FromSeconds(3.0));
			SingletonBehaviour<Loading>.Instance.DisConnect();
			BackToTitle();
		}
		else
		{
			SystemConfigGameData.FrontServerUrl = NetEnvironment.Instance.Connection.RequestUrl;
		}
	}

	public async UniTask<bool> RefleshExpirationDate()
	{
		if (!GameUtility.IsKamunityBootEnable() || GameDataManager.Account.IsGuestUser)
		{
			return true;
		}
		XrpcResponseHelper<XrpcModuleCardmngTypes.CardmngInquireResponse> inquireHelper = new XrpcResponseHelper<XrpcModuleCardmngTypes.CardmngInquireResponse>();
		XrpcModuleCardmng.XrpcMethodInquire.Request(null, GameDataManager.Account.CardType, GameDataManager.Account.CardID, update: true, inquireHelper.Callback, null);
		await inquireHelper.CoWaitForXrpcFinish();
		return !inquireHelper.isError;
	}
}
