using System.Collections;
using KAMUNITY;
using UnityEngine;
using UnityEngine.UI;

public class CreditCoinDisp : MonoBehaviour
{
	[SerializeField]
	private Text m_text;

	private int m_coin;

	private int m_credit;

	private bool m_isVisible;

	private AvsTypes.EA3_EACOIN_STATUS m_status = AvsTypes.EA3_EACOIN_STATUS.IGNORE;

	private bool m_isSessionOpened;

	private bool m_isDispPaseliBalance;

	private bool m_isEaPassAuthed;

	private bool m_isInGame;

	public static bool IsForceDisp { get; set; }

	private IEnumerator Start()
	{
		yield return new WaitUntil(() => DeviceManager.Instance.Initialized);
		SetText();
	}

	private void Update()
	{
		if (DeviceManager.Instance.Initialized)
		{
			bool flag = !string.IsNullOrEmpty(GameDataManager.Account.AppSessionID);
			if (GameUtility.GetCurrentFractionCoin() != m_coin || GameUtility.GetCurrentCredit() != m_credit || (DeviceManager.Instance.Coin().IsActive || IsForceDisp) != m_isVisible || IsNeedRewritePaseli() || PaseliUtility.IsCheckinProcessFinished != m_isEaPassAuthed || flag != m_isInGame)
			{
				SetText();
			}
		}
	}

	private void SetText()
	{
		m_credit = GameUtility.GetCurrentCredit();
		m_coin = GameUtility.GetCurrentFractionCoin();
		m_isVisible = DeviceManager.Instance.Coin().IsActive || IsForceDisp;
		m_isDispPaseliBalance = PaseliUtility.IsDispPaseliBalance;
		m_isEaPassAuthed = PaseliUtility.IsCheckinProcessFinished;
		m_isInGame = !string.IsNullOrEmpty(GameDataManager.Account.AppSessionID);
		if (!m_isVisible)
		{
			m_text.text = "";
			return;
		}
		if (GameUtility.IsFreePlay())
		{
			if (GameUtility.IsEventMode())
			{
				m_text.text = "EVENT MODE";
			}
			else
			{
				m_text.text = "FREE PLAY";
			}
			return;
		}
		if (GameUtility.IsKonastaOrNetcafeBehavior)
		{
			m_text.text = "";
		}
		else
		{
			int coinNumFromCurCoinSlotType = GameUtility.GetCoinNumFromCurCoinSlotType();
			if (coinNumFromCurCoinSlotType > 1)
			{
				m_text.text = string.Format("{0} {1} {2} {3}/{4}", (m_credit > 1) ? "CREDITS" : "CREDIT", m_credit, (m_coin > 1) ? "COINS" : "COIN", m_coin, coinNumFromCurCoinSlotType);
			}
			else
			{
				m_text.text = string.Format("{0} {1}", (m_credit > 1) ? "CREDITS" : "CREDIT", m_credit);
			}
		}
		if (!GameUtility.IsKamunityBootEnable())
		{
			return;
		}
		m_status = AvsEa3Coin.Ea3EacoinGetStatus();
		m_isSessionOpened = PaseliUtility.IsSessionOpened();
		switch (m_status)
		{
		case AvsTypes.EA3_EACOIN_STATUS.NOT_AVAILABLE:
			m_text.text += " PASELI NOT AVAILABLE";
			break;
		case AvsTypes.EA3_EACOIN_STATUS.AVAILABLE:
			if (m_isSessionOpened)
			{
				if (PaseliUtility.IsDispPaseliBalance)
				{
					int balance = PaseliUtility.GetBalance();
					if (balance < 0)
					{
						m_text.text += " PASELI  - ";
					}
					else if (balance >= 1000000)
					{
						m_text.text += " PASELI 000000";
					}
					else
					{
						m_text.text += $" PASELI {balance}";
					}
				}
				else
				{
					m_text.text += " PASELI ******";
				}
				if (PaseliUtility.GetExCoin() > 0)
				{
					m_text.text += $" + {PaseliUtility.GetExCoin()}";
				}
			}
			else if (PaseliUtility.IsCheckinProcessFinished && !GameUtility.IsKonastaOrNetcafeBehavior)
			{
				m_text.text += " PASELI NO ACCOUNT";
			}
			else if (m_isInGame || GameUtility.IsKonastaOrNetcafeBehavior)
			{
				Text text = m_text;
				text.text = text.text ?? "";
			}
			else
			{
				m_text.text += $" EXTRA PASELI {PaseliUtility.GetExCoin()}";
			}
			break;
		case AvsTypes.EA3_EACOIN_STATUS.IGNORE:
			break;
		}
	}

	private bool IsNeedRewritePaseli()
	{
		if (!GameUtility.IsKamunityBootEnable())
		{
			return false;
		}
		if (m_status == AvsEa3Coin.Ea3EacoinGetStatus() && m_isSessionOpened == PaseliUtility.IsSessionOpened())
		{
			return m_isDispPaseliBalance != PaseliUtility.IsDispPaseliBalance;
		}
		return true;
	}
}
