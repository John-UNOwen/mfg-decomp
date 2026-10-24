using System.Linq;
using EagDef;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DemoErrorMessage : MonoBehaviour
{
	private enum COLOR
	{
		GREEN,
		YELLOW,
		RED
	}

	[SerializeField]
	private Color[] m_color;

	[SerializeField]
	private Image m_frame;

	[SerializeField]
	private TextMeshProUGUI m_title;

	[SerializeField]
	private TextMeshProUGUI m_message;

	[SerializeField]
	private GameObject m_layoutRect;

	private STATUS m_status;

	private void Update()
	{
		STATUS status = EagCore.Instance.Status;
		if (m_status != status)
		{
			m_status = status;
			SetError(status);
		}
	}

	private void SetError(STATUS status)
	{
		if (!GameUtility.IsKamunityBootEnable())
		{
			m_layoutRect.SetActive(value: false);
			return;
		}
		if (status == STATUS.OK)
		{
			m_layoutRect.SetActive(value: false);
			return;
		}
		string text = "";
		COLOR cOLOR = COLOR.RED;
		switch (status)
		{
		case STATUS.SETUP:
		case STATUS.DOWN:
			text = "DEMO_CHECK_NETWORK";
			cOLOR = COLOR.RED;
			break;
		case STATUS.OFFLINE:
			text = "DEMO_OFFLINE_MODE";
			cOLOR = COLOR.RED;
			break;
		case STATUS.LOCAL:
			text = "DEMO_LOCAL_MODE";
			cOLOR = COLOR.RED;
			break;
		case STATUS.MAINTE:
			text = "DEMO_MAINTENANCE";
			cOLOR = COLOR.YELLOW;
			break;
		case STATUS.NO_CONFIRM:
			text = "DEMO_EA_CHECK";
			cOLOR = COLOR.RED;
			break;
		case STATUS.RUNTIME_ERROR:
		case STATUS.DISABLED:
			text = "DEMO_DISABLE_REBOOT";
			cOLOR = COLOR.RED;
			break;
		}
		m_frame.color = m_color[(int)cOLOR];
		m_title.text = TextManager.Get(text);
		m_message.text = TextManager.Get(text + "_MSG");
		m_layoutRect.SetActive(value: true);
		foreach (ContentSizeFitter item in m_layoutRect.GetComponentsInChildren<ContentSizeFitter>().Reverse())
		{
			LayoutRebuilder.ForceRebuildLayoutImmediate(item.GetComponent<RectTransform>());
		}
	}
}
