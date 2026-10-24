using System.Collections;
using Cysharp.Threading.Tasks;
using GameContents;
using KAMUNITY;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CreditConsumeButton : MonoBehaviour
{
	[SerializeField]
	private Button m_button;

	[SerializeField]
	private Button m_buttonShort;

	[SerializeField]
	private TextMeshProUGUI m_consumeText;

	[SerializeField]
	private TextMeshProUGUI m_consumeTextShort;

	[SerializeField]
	private TextMeshProUGUI m_valueText;

	[SerializeField]
	private TextMeshProUGUI m_valueTextShort;

	[SerializeField]
	private GameObject m_lackCredit;

	[SerializeField]
	private GameObject m_lackCreditShort;

	private int m_value;

	private int m_credit;

	private GAME_MODE m_mode;

	private int m_num;

	private UnityAction m_onClick;

	private UnityAction<bool, ConsumeWindow.EndType> m_onEnd;

	private bool m_isDecide;

	private float m_agingCount;

	private bool m_isRenewAccessDate;

	private bool m_isFreeContinue;

	public void SetInteractable(bool interactable)
	{
		m_button.interactable = interactable;
		m_buttonShort.interactable = interactable;
	}

	public void Set(GAME_MODE mode, int num, UnityAction onClick, UnityAction<bool, ConsumeWindow.EndType> onEnd, bool isRenewAccessDate, bool isFree = false)
	{
		m_num = num;
		m_mode = mode;
		m_value = GameUtility.GetPrice(mode, CONSUME_TYPE.CREDIT, isFree) * m_num;
		SetCredit(GameUtility.GetCurrentCredit());
		m_onClick = onClick;
		m_onEnd = onEnd;
		m_isFreeContinue = isFree;
		if ((uint)(mode - 3) <= 2u || mode == GAME_MODE.MUSICGACHA)
		{
			m_buttonShort.onClick.AddListener(OnClick);
			m_button.gameObject.SetActive(value: false);
			m_buttonShort.gameObject.SetActive(value: true);
		}
		else
		{
			m_button.onClick.AddListener(OnClick);
			m_button.gameObject.SetActive(value: true);
			m_buttonShort.gameObject.SetActive(value: false);
		}
		m_agingCount = 0f;
		m_isRenewAccessDate = isRenewAccessDate;
	}

	private void Update()
	{
		if (!m_isDecide)
		{
			int currentCredit = GameUtility.GetCurrentCredit();
			if (currentCredit != m_credit)
			{
				m_credit = currentCredit;
				SetCredit(m_credit);
			}
		}
	}

	private void SetCredit(int credit)
	{
		if (credit < m_value)
		{
			m_button.interactable = false;
			m_buttonShort.interactable = false;
			string arg = string.Format((m_value - credit > 1) ? "{0}<size=80%>CREDITS" : "{0}<size=80%>CREDIT", m_value - credit);
			m_consumeText.SetText((m_value > 1) ? "{0}<size=80%>CREDITS" : "{0}<size=80%>CREDIT", m_value);
			m_valueText.SetText(string.Format(TextManager.Get(TextManager.KEY.CONSUME_CREDIT_SHORT_NUM), arg));
			m_consumeTextShort.SetText((m_value > 1) ? "{0}<size=80%>CREDITS" : "{0}<size=80%>CREDIT", m_value);
			m_valueTextShort.SetText(string.Format(TextManager.Get(TextManager.KEY.CONSUME_CREDIT_SHORT_NUM), arg));
		}
		else
		{
			m_button.interactable = true;
			m_buttonShort.interactable = true;
			m_consumeText.SetText((m_value > 1) ? "{0}<size=80%>CREDITS" : "{0}<size=80%>CREDIT", m_value);
			m_lackCredit.SetActive(value: false);
			m_consumeTextShort.SetText((m_value > 1) ? "{0}<size=80%>CREDITS" : "{0}<size=80%>CREDIT", m_value);
			m_lackCreditShort.SetActive(value: false);
		}
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
		if (GameUtility.IsKamunityBootEnable() && m_isRenewAccessDate)
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
		GameUtility.ComsumeCredit(m_mode, m_num, m_isFreeContinue);
		m_onEnd?.Invoke(arg0: true, ConsumeWindow.EndType.Success);
	}
}
