using System.Collections;
using System.Linq;
using Cysharp.Threading.Tasks;
using GameContents;
using KAMUNITY;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PaseliWindow : PopupWindow
{
	public enum COLOR
	{
		GRAY,
		GREEN,
		RED,
		YELLOW,
		NUM
	}

	public enum BUTTON
	{
		YES,
		NO,
		NUM
	}

	[SerializeField]
	private Color[] m_color = new Color[4];

	[SerializeField]
	private TextMeshProUGUI m_text;

	[SerializeField]
	private RectTransform m_layoutRect;

	[SerializeField]
	private GameObject m_buttonRoot;

	[SerializeField]
	private Button[] m_button = new Button[2];

	private AvsEa3CoinTypes.EA3_EACOIN_ERROR m_status = (AvsEa3CoinTypes.EA3_EACOIN_ERROR)2147483647;

	private Coroutine m_coOpen;

	private bool m_isSkip;

	public bool Open(AvsEa3CoinTypes.EA3_EACOIN_ERROR status)
	{
		if (m_status == status)
		{
			return false;
		}
		m_status = status;
		string text = "";
		TextManager.Get(TextManager.KEY.CONSUME_POINT);
		switch (status)
		{
		case (AvsEa3CoinTypes.EA3_EACOIN_ERROR)0:
			Open(COLOR.YELLOW, TextManager.Get(TextManager.KEY.CHECK_IN), 10f);
			break;
		case AvsEa3CoinTypes.EA3_EACOIN_ERROR.PROCESSING:
			text = TextManager.Get(TextManager.KEY.PROCESSING);
			Open(COLOR.GRAY, text, 0f, enable_skip: false);
			break;
		case AvsEa3CoinTypes.EA3_EACOIN_ERROR.INVAL:
		case AvsEa3CoinTypes.EA3_EACOIN_ERROR.LOCATION:
		case AvsEa3CoinTypes.EA3_EACOIN_ERROR.UNEXPECTED:
		case AvsEa3CoinTypes.EA3_EACOIN_ERROR.NOACCOUNT:
		case AvsEa3CoinTypes.EA3_EACOIN_ERROR.AUTHACCOUNT:
		case AvsEa3CoinTypes.EA3_EACOIN_ERROR.NOTECCARD:
		case AvsEa3CoinTypes.EA3_EACOIN_ERROR.NOTCHECKIN:
		case AvsEa3CoinTypes.EA3_EACOIN_ERROR.ECINVALID:
			text = TextManager.Get(TextManager.KEY.DISABLED);
			Open(COLOR.RED, text);
			break;
		case AvsEa3CoinTypes.EA3_EACOIN_ERROR.MAINTENANCE:
			text = TextManager.Get(TextManager.KEY.MAINTENANCE);
			Open(COLOR.YELLOW, text);
			break;
		case AvsEa3CoinTypes.EA3_EACOIN_ERROR.DISABLED:
			text = TextManager.Get(TextManager.KEY.DISABLED);
			Open(COLOR.RED, text);
			break;
		}
		return true;
	}

	public bool Open(AvsEa3CoinTypes.EA3_EACOIN_ERROR status, GAME_MODE mode, int price, int balance)
	{
		if (m_status == status)
		{
			return false;
		}
		m_status = status;
		string text = "";
		string format = TextManager.Get(TextManager.KEY.CONSUME_POINT);
		switch (status)
		{
		case (AvsEa3CoinTypes.EA3_EACOIN_ERROR)0:
			SoundManager.PlaySe("SFX_SYS_PASELI_OK").Forget();
			text = TextManager.Get(TextManager.KEY.CONSUME);
			text += "\n";
			text += string.Format(format, price, balance);
			Open(COLOR.GREEN, text);
			break;
		case AvsEa3CoinTypes.EA3_EACOIN_ERROR.PROCESSING:
			text = TextManager.Get(TextManager.KEY.PROCESSING);
			Open(COLOR.GRAY, text, 0f, enable_skip: false);
			break;
		case AvsEa3CoinTypes.EA3_EACOIN_ERROR.SHORTBALANCE:
			SoundManager.PlaySe("SFX_SYS_PASELI_ERROR").Forget();
			text = TextManager.Get(TextManager.KEY.SHORTBALANCE);
			text += "\n";
			text += string.Format(format, "------", balance);
			Open(COLOR.RED, text, 3f, enable_skip: false);
			break;
		case AvsEa3CoinTypes.EA3_EACOIN_ERROR.PLNOTHELD:
		case AvsEa3CoinTypes.EA3_EACOIN_ERROR.PLEXPIRED:
		case AvsEa3CoinTypes.EA3_EACOIN_ERROR.ABORTED:
		case AvsEa3CoinTypes.EA3_EACOIN_ERROR.LOCATION:
		case AvsEa3CoinTypes.EA3_EACOIN_ERROR.UNEXPECTED:
		case AvsEa3CoinTypes.EA3_EACOIN_ERROR.NOSUCHPATH:
		case AvsEa3CoinTypes.EA3_EACOIN_ERROR.NOACCOUNT:
		case AvsEa3CoinTypes.EA3_EACOIN_ERROR.NOTCHECKIN:
		case AvsEa3CoinTypes.EA3_EACOIN_ERROR.ECINVALID:
			SoundManager.PlaySe("SFX_SYS_PASELI_ERROR").Forget();
			text = TextManager.Get(TextManager.KEY.FAILED);
			text += "\n";
			text += string.Format(format, "------", "------");
			Open(COLOR.RED, text);
			PaseliUtility.CheckOut();
			break;
		case AvsEa3CoinTypes.EA3_EACOIN_ERROR.MAINTENANCE:
			SoundManager.PlaySe("SFX_SYS_PASELI_ERROR").Forget();
			text = TextManager.Get(TextManager.KEY.MAINTENANCE);
			text += "\n";
			text += string.Format(format, "------", "------");
			Open(COLOR.YELLOW, text);
			break;
		case AvsEa3CoinTypes.EA3_EACOIN_ERROR.DISABLED:
			text = TextManager.Get(TextManager.KEY.DISABLED);
			text += "\n";
			text += string.Format(format, "------", "------");
			Open(COLOR.RED, text);
			PaseliUtility.CheckOut();
			break;
		}
		return true;
	}

	public void CheckIn()
	{
		Open(COLOR.YELLOW, TextManager.Get(TextManager.KEY.CHECK_IN), 10f);
	}

	public void Open(COLOR color, string message, float disp_time = 3f, bool enable_skip = true)
	{
		m_text.text = message;
		base.gameObject.SetActive(value: true);
		m_coOpen = StartCoroutine(CoOpen(disp_time, enable_skip));
	}

	private IEnumerator CoOpen(float disp_time, bool enable_skip)
	{
		foreach (ContentSizeFitter item in m_layoutRect.GetComponentsInChildren<ContentSizeFitter>().Reverse())
		{
			LayoutRebuilder.ForceRebuildLayoutImmediate(item.GetComponent<RectTransform>());
		}
		float wait_time = 0.5f;
		yield return new WaitForSecondsRealtime(wait_time);
		disp_time -= wait_time;
		float time = Time.time;
		m_isSkip = false;
		yield return new WaitUntil(() => (disp_time > 0f && Time.time - time > disp_time) || (enable_skip && m_isSkip));
		base.gameObject.SetActive(value: false);
		Exit();
	}

	public void OnClick()
	{
		m_isSkip = true;
	}

	public void Exit()
	{
		Object.Destroy(base.gameObject);
	}

	public void ReauthConfirm(UnityAction<bool> onClick)
	{
		m_text.SetText(TextManager.Get(TextManager.KEY.REAUTH), PaseliUtility.GetLastNotch());
		m_buttonRoot.SetActive(value: true);
		base.gameObject.SetActive(value: true);
		m_coOpen = StartCoroutine(CoReauthConfirm(onClick));
	}

	private IEnumerator CoReauthConfirm(UnityAction<bool> onClick)
	{
		bool is_clicked = false;
		m_button[0].onClick.AddListener(delegate
		{
			is_clicked = true;
			SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
			onClick(arg0: true);
			Button[] button2 = m_button;
			for (int j = 0; j < button2.Length; j++)
			{
				button2[j].enabled = false;
			}
		});
		m_button[1].onClick.AddListener(delegate
		{
			is_clicked = true;
			SoundManager.PlaySe("SFX_SYS_CANCEL1").Forget();
			onClick(arg0: false);
			Button[] button = m_button;
			for (int i = 0; i < button.Length; i++)
			{
				button[i].enabled = false;
			}
		});
		foreach (ContentSizeFitter item in m_layoutRect.GetComponentsInChildren<ContentSizeFitter>().Reverse())
		{
			LayoutRebuilder.ForceRebuildLayoutImmediate(item.GetComponent<RectTransform>());
		}
		CountDownTimer timer = ResidentFlow.GetInstance().GetTimer();
		timer.SetTimer(10f, disp: true);
		float time = 0.5f;
		yield return new WaitForSecondsRealtime(time);
		timer.StartTimer();
		m_isSkip = false;
		yield return new WaitUntil(() => is_clicked || timer.IsEnd());
		if (!is_clicked)
		{
			m_button[1].onClick.Invoke();
		}
		base.gameObject.SetActive(value: false);
		Exit();
	}
}
