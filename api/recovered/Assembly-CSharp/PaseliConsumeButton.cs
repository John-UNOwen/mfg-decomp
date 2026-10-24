using System.Collections;
using Cysharp.Threading.Tasks;
using GameContents;
using KAMUNITY;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PaseliConsumeButton : MonoBehaviour
{
	[SerializeField]
	private Button m_button;

	[SerializeField]
	private Button m_buttonShort;

	[SerializeField]
	private TextMeshProUGUI m_consumeText;

	[SerializeField]
	private TextMeshProUGUI m_consumeShortText;

	[SerializeField]
	private TextMeshProUGUI m_playText;

	[SerializeField]
	private Transform m_root;

	[SerializeField]
	private GameObject m_paseliPointCampaignBanner;

	private int m_value;

	private GAME_MODE m_mode;

	private int m_num;

	private UnityAction m_onClick;

	private UnityAction<bool, ConsumeWindow.EndType> m_onEnd;

	private UnityAction m_onReturn;

	private GameObject m_tenkeyWindow;

	private bool m_isRenewAccessDate;

	private bool m_isDecide;

	public void SetInteractable(bool interactable)
	{
		m_button.interactable = interactable;
		m_buttonShort.interactable = interactable;
	}

	public void SetActive(bool active)
	{
		base.gameObject.SetActive(active);
	}

	public void Set(GAME_MODE mode, int num, UnityAction onClick, UnityAction<bool, ConsumeWindow.EndType> onEnd, UnityAction onReturn, GameObject tenkeyWindow, bool isRenewAccessDate)
	{
		m_mode = mode;
		m_num = num;
		m_value = GameUtility.GetPrice(mode, CONSUME_TYPE.PASELI) * num;
		m_onClick = onClick;
		m_onEnd = onEnd;
		m_onReturn = onReturn;
		if ((uint)(mode - 3) <= 2u || mode == GAME_MODE.MUSICGACHA)
		{
			m_consumeShortText.SetText("{0}<size=0.75em>円(税込)</size>", m_value);
			m_buttonShort.onClick.AddListener(OnClick);
			m_button.gameObject.SetActive(value: false);
			m_buttonShort.gameObject.SetActive(value: true);
		}
		else
		{
			m_playText.text = ((mode == GAME_MODE.PREMIUMSTART || mode == GAME_MODE.PREMIUMCONTINUE) ? "プレミアム" : "ノーマル") + "プレー" + (GameUtility.IsKonastaOrNetcafeBehavior ? "×1" : "");
			m_consumeText.SetText("{0}<size=0.75em>円(税込)</size>", m_value);
			m_button.onClick.AddListener(OnClick);
			m_button.gameObject.SetActive(value: true);
			m_buttonShort.gameObject.SetActive(value: false);
		}
		m_tenkeyWindow = tenkeyWindow;
		m_isRenewAccessDate = isRenewAccessDate;
		m_paseliPointCampaignBanner.SetActive(GameDataManager.System.GameControlInfo.EventStatus.IsActive(GameEventType.PaseliPointCampaign));
	}

	private void OnClick()
	{
		if (!m_isDecide)
		{
			m_isDecide = true;
			SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
			m_onClick?.Invoke();
			StartCoroutine(CoConsume());
		}
	}

	private IEnumerator CoConsume()
	{
		if (!PaseliUtility.IsSessionOpened())
		{
			m_onEnd?.Invoke(arg0: false, ConsumeWindow.EndType.NetworkError);
			yield break;
		}
		if (PaseliUtility.IsNeedReauth())
		{
			yield return CoReauth();
			if (!PaseliUtility.IsSessionOpened())
			{
				m_onEnd?.Invoke(arg0: false, ConsumeWindow.EndType.NetworkError);
				yield break;
			}
		}
		if (m_isRenewAccessDate)
		{
			XrpcResponseHelper<XrpcModuleCardmngTypes.CardmngInquireResponse> responseCardMngInquire = new XrpcResponseHelper<XrpcModuleCardmngTypes.CardmngInquireResponse>();
			XrpcModuleCardmng.XrpcMethodInquire.Request(null, GameDataManager.Account.CardType, GameDataManager.Account.CardID, update: true, responseCardMngInquire.Callback, null);
			yield return responseCardMngInquire.CoWaitForXrpcFinish();
			if (responseCardMngInquire.isError)
			{
				m_onEnd?.Invoke(arg0: false, ConsumeWindow.EndType.NetworkError);
				yield break;
			}
		}
		PaseliWindow window = DialogManager.Instance.CreatePaseliDialog(m_root);
		bool is_shortbalance = false;
		AvsEa3CoinTypes.EA3_EACOIN_ERROR status = PaseliUtility.ComsumePaseliWithPrice(m_mode, m_value);
		Debug.Log($"ComsumePaseliRequest: mode:{m_mode}, status:{status}");
		if (status == (AvsEa3CoinTypes.EA3_EACOIN_ERROR)0)
		{
			status = PaseliUtility.GetComsumePaseliStatus(m_mode);
			Debug.Log($"GetComsumePaseliStatus: mode:{m_mode}, status:{status}");
			if (status == AvsEa3CoinTypes.EA3_EACOIN_ERROR.PROCESSING)
			{
				window.Open(status);
				yield return new WaitUntil(delegate
				{
					status = PaseliUtility.GetComsumePaseliStatus(m_mode);
					Debug.Log($"GetComsumePaseliStatus: mode:{m_mode}, status:{status}");
					return status != AvsEa3CoinTypes.EA3_EACOIN_ERROR.PROCESSING;
				});
			}
		}
		yield return null;
		window.Open(status, m_mode, m_value, PaseliUtility.GetBalance());
		bool success = status == (AvsEa3CoinTypes.EA3_EACOIN_ERROR)0;
		if (status == AvsEa3CoinTypes.EA3_EACOIN_ERROR.SHORTBALANCE)
		{
			is_shortbalance = true;
		}
		yield return new WaitWhile(() => window != null);
		if (is_shortbalance)
		{
			if (PaseliUtility.IsShortBalanceRetryEnable())
			{
				if (m_mode != GAME_MODE.START && m_mode != GAME_MODE.PREMIUMSTART)
				{
					PaseliUtility.AddShortBalanceRetryCount();
					m_onReturn?.Invoke();
					m_isDecide = false;
					yield break;
				}
				GameUtility.IsReservedGameOver = true;
			}
			else
			{
				GameUtility.IsReservedGameOver = true;
			}
		}
		if (success && !PaseliUtility.IsReauthEnable())
		{
			PaseliUtility.GetLastNotch();
			PaseliUtility.CheckOut();
			GameUtility.IsReservedGameOver = true;
			window = DialogManager.Instance.CreatePaseliDialog(m_root);
			string message = string.Format(TextManager.Get(TextManager.KEY.REAUTH_DISABLE), PaseliUtility.LastNotch);
			window.Open(PaseliWindow.COLOR.YELLOW, message, 5f, enable_skip: false);
			yield return new WaitWhile(() => window != null);
		}
		m_onEnd?.Invoke(success, success ? ConsumeWindow.EndType.Success : ConsumeWindow.EndType.Error);
	}

	private IEnumerator CoReauth()
	{
		bool reauth = false;
		PaseliWindow window = DialogManager.Instance.CreatePaseliDialog(m_root);
		window.ReauthConfirm(delegate(bool is_yes)
		{
			reauth = is_yes;
		});
		yield return new WaitWhile(() => window != null);
		if (!reauth)
		{
			PaseliUtility.CheckOut();
			GameUtility.IsReservedGameOver = true;
			yield break;
		}
		CardEntryManager entry = m_tenkeyWindow.GetComponent<CardEntryManager>();
		CountDownTimer timer = ResidentFlow.GetInstance().GetTimer();
		timer.SetTimer(GameDef.CARD_ENTRY_TIMEOUT, disp: false);
		yield return entry.CreateTenkey(_bBinded: true, delegate
		{
		});
		StartCoroutine(entry.CoCardEntryRequest());
		timer.StartTimer();
		yield return new WaitUntil(() => entry.IsEnd() || entry.IsErrorStopState || timer.IsEnd());
		window = DialogManager.Instance.CreatePaseliDialog(m_root);
		if (!entry.IsErrorStopState && !timer.IsEnd())
		{
			window.Open(PaseliWindow.COLOR.GREEN, TextManager.Get(TextManager.KEY.REAUTH_OK), 1f);
		}
		else
		{
			PaseliUtility.CheckOut();
			GameUtility.IsReservedGameOver = true;
			string message = string.Format(TextManager.Get(TextManager.KEY.REAUTH_CANCEL), GameUtility.GetGameModeStr(m_mode));
			window.Open(PaseliWindow.COLOR.RED, message, 5f);
		}
		timer.SetPause(pause: true);
		yield return new WaitWhile(() => window != null);
	}

	private IEnumerator CoPassError(CardEntryManager entry, string message, CountDownTimer timer)
	{
		timer.SetPause(pause: true);
		PaseliWindow window = DialogManager.Instance.CreatePaseliDialog(m_root);
		window.Open(PaseliWindow.COLOR.RED, message, 1f);
		yield return new WaitWhile(() => window != null);
		if (!entry.IsErrorStopState)
		{
			timer.SetPause(pause: false);
		}
	}
}
