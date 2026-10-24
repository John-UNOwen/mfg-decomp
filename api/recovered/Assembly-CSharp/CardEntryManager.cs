using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameContents;
using KAMUNITY;
using MFG.GameRequest;
using TCSNET;
using UnityEngine;
using UnityEngine.Events;

public class CardEntryManager : MonoBehaviour
{
	private enum CARDENTRY_STATE
	{
		WAIT,
		CREATE_TENKEY,
		CHECKIN_REQUEST,
		CHECKIN_WAIT,
		CHECKIN_COMPLETE
	}

	private enum NEW_CARDENTRY_STATE
	{
		WAIT,
		CREATE_TENKEY,
		CHECK_PASSWORD,
		CHECKIN_REQUEST,
		CHECKIN_WAIT,
		CHECKIN_COMPLETE
	}

	public enum ErrorStopStateType
	{
		None,
		PasswordError,
		NetworkError,
		Cancel
	}

	private CARDENTRY_STATE m_state;

	private NEW_CARDENTRY_STATE m_newEntryState;

	private int MISS_COUNT = 1;

	private TenkeyManager m_tenkey;

	private bool m_isAuthPassProcessFinished;

	private bool m_bEnd;

	private string m_cardId = "";

	private AvsTypes.CardType m_cardType = AvsTypes.CardType.ICCARD;

	private string m_refId = "";

	private int m_passwordErrorCount;

	private string m_firstPassword = "";

	private bool m_isExecutingEntryTask;

	private bool m_bBinded;

	public ErrorStopStateType m_errorStopStateType;

	[SerializeField]
	private GameObject m_tenkeyObj;

	private UnityAction<string> m_errorAction;

	private CountDownTimer _countDownTimer;

	public bool IsErrorStopState { get; set; }

	public string Password => m_tenkey.GetInputPassword();

	public void Clear()
	{
		m_bEnd = false;
		m_passwordErrorCount = 0;
		m_isAuthPassProcessFinished = false;
		IsErrorStopState = false;
		m_errorStopStateType = ErrorStopStateType.None;
	}

	public IEnumerator CreateTenkey(bool _bBinded, UnityAction<string> _errorAction = null, CountDownTimer countDownTimer = null)
	{
		Clear();
		_countDownTimer = countDownTimer;
		m_tenkey = m_tenkeyObj.GetComponent<TenkeyManager>();
		m_tenkey.CreatePasswordAuthTenkey(delegate
		{
			CallAuthPass();
		}, delegate
		{
			CancelAuthPass();
		});
		m_tenkey.SetActiveTenkeyObject = false;
		m_tenkey.SetAnnounceText = TextManager.Get(TextManager.KEY.ENTRY_PASSWORD);
		m_bBinded = _bBinded;
		m_errorAction = _errorAction;
		m_state = CARDENTRY_STATE.CREATE_TENKEY;
		m_isExecutingEntryTask = true;
		yield return null;
	}

	public IEnumerator CoCardEntryRequest()
	{
		ResidentFlow.GetInstance().GetTouchEffect().SetEnable(enable: false);
		ResidentFlow.GetInstance().GetMouseCursor().SetEnable(enable: false);
		base.gameObject.SetActive(value: true);
		while (m_isExecutingEntryTask)
		{
			switch (m_state)
			{
			case CARDENTRY_STATE.CREATE_TENKEY:
				m_tenkey.SetActiveTenkeyObject = true;
				m_state = CARDENTRY_STATE.CHECKIN_WAIT;
				break;
			case CARDENTRY_STATE.CHECKIN_WAIT:
				if (m_isAuthPassProcessFinished)
				{
					bool isPause = CountDownTimer_IsPause();
					CountDownTimer_SetPause(isPause: true);
					yield return new WaitForSeconds(2f);
					CountDownTimer_SetPause(isPause);
					if (!IsErrorStopState)
					{
						m_tenkey.SetAnnounceText = TextManager.Get(TextManager.KEY.ENTRY_CONFIRM);
					}
					m_state = CARDENTRY_STATE.CHECKIN_COMPLETE;
				}
				break;
			case CARDENTRY_STATE.CHECKIN_COMPLETE:
				m_bEnd = true;
				m_isExecutingEntryTask = false;
				ResidentFlow.GetInstance().GetTouchEffect().SetEnable(enable: true);
				ResidentFlow.GetInstance().GetMouseCursor().SetEnable(enable: true);
				base.gameObject.SetActive(value: false);
				break;
			}
			yield return null;
		}
		yield return null;
	}

	private void CallAuthPass()
	{
		CountDownTimer_SetPause(isPause: true);
		StartCoroutine(CoCallAuthPass());
	}

	private void ReAuthPass()
	{
		string text = "";
		if (PaseliUtility.Reauth(m_tenkey.GetInputPassword()))
		{
			m_isAuthPassProcessFinished = true;
			return;
		}
		if (m_passwordErrorCount > MISS_COUNT)
		{
			text = TextManager.Get(TextManager.KEY.ENTRY_NG);
			m_isAuthPassProcessFinished = true;
			IsErrorStopState = true;
			m_errorStopStateType = ErrorStopStateType.PasswordError;
			ResidentFlow.GetInstance().GetTouchEffect().SetEnable(enable: true);
			ResidentFlow.GetInstance().GetMouseCursor().SetEnable(enable: true);
		}
		else
		{
			text = TextManager.Get(TextManager.KEY.ENTRY_RETRY);
			m_tenkey.ResetInput();
			m_passwordErrorCount++;
		}
		m_tenkey.SetAnnounceText = text;
		m_errorAction(text);
	}

	public IEnumerator CoCallAuthPass()
	{
		m_tenkey.SetAnnounceText = TextManager.Get(TextManager.KEY.ENTRY_WAIT);
		m_refId = GameDataManager.Account.RefID;
		if (m_errorAction != null)
		{
			ReAuthPass();
		}
		else
		{
			XrpcResponseHelperSimple xrpcHelper = new XrpcResponseHelperSimple();
			XrpcModuleCardmng.Authpass(m_refId, m_tenkey.GetInputPassword(), xrpcHelper.Callback, null);
			yield return xrpcHelper.CoWaitForXrpcFinish();
			if (xrpcHelper.isError)
			{
				Debug.Log($"CardEntryManager.CoCallAuthPass error:{xrpcHelper.status.GetStatusCode()}");
				if (xrpcHelper.status.GetStatusCode() == AvsEa3XrpcTypes.XPRC_STATUS_CODE.EPASSWD)
				{
					if (m_passwordErrorCount > MISS_COUNT)
					{
						m_tenkey.SetAnnounceText = TextManager.Get(TextManager.KEY.ENTRY_NG);
						yield return new WaitForSeconds(1f);
						m_isAuthPassProcessFinished = true;
						IsErrorStopState = true;
						m_errorStopStateType = ErrorStopStateType.PasswordError;
						ResidentFlow.GetInstance().GetTouchEffect().SetEnable(enable: true);
						ResidentFlow.GetInstance().GetMouseCursor().SetEnable(enable: true);
					}
					else
					{
						m_tenkey.SetAnnounceText = TextManager.Get(TextManager.KEY.ENTRY_RETRY);
						yield return new WaitForSeconds(1f);
						m_tenkey.ResetInput();
						m_passwordErrorCount++;
					}
				}
				else
				{
					m_isAuthPassProcessFinished = true;
					IsErrorStopState = true;
					m_errorStopStateType = ErrorStopStateType.NetworkError;
					ResidentFlow.GetInstance().GetTouchEffect().SetEnable(enable: true);
					ResidentFlow.GetInstance().GetMouseCursor().SetEnable(enable: true);
				}
			}
			else
			{
				if (!m_bBinded)
				{
					XrpcResponseHelper<XrpcModuleCardmngTypes.CardmngBindmodelResponse> responseCardMngBind = new XrpcResponseHelper<XrpcModuleCardmngTypes.CardmngBindmodelResponse>();
					XrpcModuleCardmng.Bindmodel(null, m_refId, newflg: true, responseCardMngBind.Callback, null);
					yield return responseCardMngBind.CoWaitForXrpcFinish();
					if (!responseCardMngBind.isError)
					{
						GameDataManager.Account.RefID = m_refId;
						GameDataManager.Account.DataID = responseCardMngBind.result.dataid;
						m_tenkey.SetAnnounceText = TextManager.Get(TextManager.KEY.ENTRY_CONFIRM);
						yield return UniTask.ToCoroutine(async delegate
						{
							CancellationTokenSource tokenSource = new CancellationTokenSource();
							CancellationToken token = tokenSource.Token;
							try
							{
								await RequestApiBase<CreatePlayer, CreatePlayer.ResultData, (string, string)>.Send(("PLAYER", GameDataManager.Account.DataID), enableErrorScreenTransition: false, enableSendLog: true, token);
							}
							catch
							{
								IsErrorStopState = true;
								m_errorStopStateType = ErrorStopStateType.NetworkError;
							}
							finally
							{
								tokenSource.Dispose();
							}
						});
						if (!IsErrorStopState)
						{
							TcsXrpcModule.XrpcMethodUpdateRefer.Request(new XrpcResponseHelper<TcsXrpcModuleTypes.UpdateReferResponse>().Callback, null);
							yield return xrpcHelper.CoWaitForXrpcFinish();
							GameDataManager.Account.IsFirstPlay = true;
							GameUtility.SendPcbEvent(PCB_EVENT.EVENT.PLAYERDATA_CREATE);
						}
					}
				}
				else
				{
					m_tenkey.SetAnnounceText = TextManager.Get(TextManager.KEY.ENTRY_CONFIRM);
				}
				yield return new WaitForSeconds(1f);
				m_isAuthPassProcessFinished = true;
			}
		}
		CountDownTimer_SetPause(isPause: false);
	}

	public bool IsEnd()
	{
		return m_bEnd;
	}

	public IEnumerator CreateNewEntryTenkey(AvsTypes.CardType _cardType, string _cardId, CountDownTimer countDownTimer = null)
	{
		Clear();
		_countDownTimer = countDownTimer;
		m_cardType = _cardType;
		m_cardId = _cardId;
		m_tenkey = m_tenkeyObj.GetComponent<TenkeyManager>();
		m_tenkey.CreateNewCardDataTenkey(delegate
		{
			CheckPasswordAndExecRegistration();
		}, delegate
		{
			CancelAuthPass();
		});
		m_tenkey.SetActiveTenkeyObject = false;
		m_tenkey.SetAnnounceText = TextManager.Get(TextManager.KEY.NEW_NUMBER_INPUT);
		m_isExecutingEntryTask = true;
		m_newEntryState = NEW_CARDENTRY_STATE.CREATE_TENKEY;
		yield return null;
	}

	public IEnumerator CoNewCardEntryRequest()
	{
		ResidentFlow.GetInstance().GetTouchEffect().SetEnable(enable: false);
		ResidentFlow.GetInstance().GetMouseCursor().SetEnable(enable: false);
		while (m_isExecutingEntryTask)
		{
			switch (m_newEntryState)
			{
			case NEW_CARDENTRY_STATE.CREATE_TENKEY:
				m_tenkey.SetActiveTenkeyObject = true;
				m_newEntryState = NEW_CARDENTRY_STATE.CHECKIN_WAIT;
				break;
			case NEW_CARDENTRY_STATE.CHECKIN_WAIT:
				if (m_isAuthPassProcessFinished)
				{
					bool isPause = CountDownTimer_IsPause();
					CountDownTimer_SetPause(isPause: true);
					yield return new WaitForSeconds(2f);
					CountDownTimer_SetPause(isPause);
					m_bEnd = true;
					m_isExecutingEntryTask = false;
					ResidentFlow.GetInstance().GetTouchEffect().SetEnable(enable: true);
					ResidentFlow.GetInstance().GetMouseCursor().SetEnable(enable: true);
					m_newEntryState = NEW_CARDENTRY_STATE.CHECKIN_COMPLETE;
				}
				break;
			}
			yield return null;
		}
	}

	public void CheckPasswordAndExecRegistration()
	{
		if (m_tenkey.IsFirstInput)
		{
			if (IsRepdigit(m_tenkey.GetInputPassword()))
			{
				ExitIfErrorCountExceeded(TextManager.Get(TextManager.KEY.NEW_NUMBER_ERROR), TextManager.Get(TextManager.KEY.NEW_NUMBER_NOT_ENTRY));
				return;
			}
			m_firstPassword = m_tenkey.GetInputPassword();
			m_tenkey.SetAnnounceText = TextManager.Get(TextManager.KEY.NEW_NUMBER_RETRY);
			m_tenkey.NextSetNumber();
		}
		else if (!IsMatchPassword())
		{
			ExitIfErrorCountExceeded(TextManager.Get(TextManager.KEY.NEW_NUMBER_ERROR), TextManager.Get(TextManager.KEY.NEW_NUMBER_NOT_AUTH));
		}
		else
		{
			ExecNewCardRegistration();
		}
		void ExitIfErrorCountExceeded(string exitMessage, string retryMessage)
		{
			if (m_passwordErrorCount > MISS_COUNT)
			{
				m_tenkey.SetAnnounceText = exitMessage;
				Debug.Log("CardEntryManager.CheckPasswordAndExecRegistration: 規定回数オーバー");
				IsErrorStopState = true;
				m_errorStopStateType = ErrorStopStateType.PasswordError;
				m_isAuthPassProcessFinished = true;
			}
			else
			{
				m_tenkey.SetAnnounceText = retryMessage;
				m_tenkey.ResetInput();
				m_passwordErrorCount++;
			}
		}
	}

	private void ExecNewCardRegistration()
	{
		CountDownTimer_SetPause(isPause: true);
		m_tenkey.SetAnnounceText = TextManager.Get(TextManager.KEY.ENTRY_WAIT);
		StartCoroutine(CoNewCardRegistration());
	}

	private IEnumerator CoNewCardRegistration()
	{
		XrpcResponseHelper<XrpcModuleCardmngTypes.CardmngGetrefidResponse> responseCardMngRefid = new XrpcResponseHelper<XrpcModuleCardmngTypes.CardmngGetrefidResponse>();
		XrpcModuleCardmng.Getrefid(null, m_cardType, m_cardId, newflg: true, m_tenkey.GetInputPassword(), responseCardMngRefid.Callback, null);
		yield return responseCardMngRefid.CoWaitForXrpcFinish();
		if (!responseCardMngRefid.isError)
		{
			string dataID = responseCardMngRefid.result.dataid;
			GameDataManager.Account.RefID = responseCardMngRefid.result.refid;
			GameDataManager.Account.DataID = dataID;
			m_tenkey.SetAnnounceText = TextManager.Get(TextManager.KEY.NEW_NUMBER_FINISH);
			yield return UniTask.ToCoroutine(async delegate
			{
				CancellationTokenSource tokenSource = new CancellationTokenSource();
				CancellationToken token = tokenSource.Token;
				try
				{
					await RequestApiBase<CreatePlayer, CreatePlayer.ResultData, (string, string)>.Send(("PLAYER", dataID), enableErrorScreenTransition: false, enableSendLog: true, token);
				}
				catch
				{
					IsErrorStopState = true;
					m_errorStopStateType = ErrorStopStateType.NetworkError;
				}
				finally
				{
					tokenSource.Dispose();
				}
			});
			if (!IsErrorStopState)
			{
				XrpcResponseHelper<TcsXrpcModuleTypes.UpdateReferResponse> xrpc = new XrpcResponseHelper<TcsXrpcModuleTypes.UpdateReferResponse>();
				TcsXrpcModule.XrpcMethodUpdateRefer.Request(xrpc.Callback, null);
				yield return xrpc.CoWaitForXrpcFinish();
				if (xrpc.isError)
				{
					IsErrorStopState = true;
					m_errorStopStateType = ErrorStopStateType.NetworkError;
				}
				else
				{
					GameDataManager.Account.IsFirstPlay = true;
					GameUtility.SendPcbEvent(PCB_EVENT.EVENT.PLAYERDATA_CREATE);
				}
			}
			Debug.Log($"CardEntryManager.CoNewCardRegistration: IsErrorStopState={IsErrorStopState}");
			m_isAuthPassProcessFinished = true;
		}
		else
		{
			m_tenkey.SetAnnounceText = TextManager.Get(TextManager.KEY.ENTRY_NETWORK_ERROR);
			IsErrorStopState = true;
			m_isAuthPassProcessFinished = true;
			ResidentFlow.GetInstance().GetTouchEffect().SetEnable(enable: true);
			ResidentFlow.GetInstance().GetMouseCursor().SetEnable(enable: true);
		}
		yield return new WaitForSeconds(1f);
		CountDownTimer_SetPause(isPause: false);
		yield return null;
	}

	private bool CountDownTimer_IsPause()
	{
		if (_countDownTimer == null)
		{
			return ResidentFlow.GetInstance().GetTimer().IsPause();
		}
		return _countDownTimer.IsPause();
	}

	private void CountDownTimer_SetPause(bool isPause)
	{
		if (_countDownTimer == null)
		{
			ResidentFlow.GetInstance().GetTimer().SetPause(isPause);
		}
		else
		{
			_countDownTimer.SetPause(isPause);
		}
	}

	private bool IsRepdigit(string digits)
	{
		for (int i = 0; i < digits.Length - 1; i++)
		{
			string text = digits.Substring(0, 1);
			string text2 = digits.Substring(i + 1, 1);
			if (text != text2)
			{
				return false;
			}
		}
		return true;
	}

	private bool IsMatchPassword()
	{
		return m_firstPassword == m_tenkey.GetInputPassword();
	}

	public void Exit()
	{
		if (m_tenkey != null)
		{
			m_tenkey.Exit();
			Object.Destroy(m_tenkeyObj);
		}
	}

	private void CancelAuthPass()
	{
		m_isAuthPassProcessFinished = true;
		IsErrorStopState = true;
		m_errorStopStateType = ErrorStopStateType.Cancel;
		ResidentFlow.GetInstance().GetTouchEffect().SetEnable(enable: true);
		ResidentFlow.GetInstance().GetMouseCursor().SetEnable(enable: true);
	}
}
