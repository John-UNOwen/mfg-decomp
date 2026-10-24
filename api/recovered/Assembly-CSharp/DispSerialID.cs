using KAM.Usefully;
using TMPro;
using UnityEngine;

public class DispSerialID : MonoBehaviour
{
	private enum Step
	{
		INIT,
		FIRST,
		SECOND,
		THIRD,
		FOURTH,
		DISP,
		WAIT
	}

	private GameObject m_dispUp;

	private GameObject m_bgUp;

	private Step m_step;

	private float m_time;

	private int m_counter;

	private static GameObject s_enterObject;

	private string dispFormat = "SOFT ID: {0}\nSYSTEM ID: {1}\nBUILD: {2}({3})";

	private string GetFormattedSystemID()
	{
		string text = SystemUtility.Instance.SystemID.PadLeft(20, '0');
		return text.Substring(0, 4) + "-" + text.Substring(4, 4) + "-" + text.Substring(8, 4) + "-" + text.Substring(12, 4) + "-" + text.Substring(16, 4);
	}

	private void Start()
	{
		m_dispUp = base.transform.root.Find("Konasta").gameObject;
		if (m_dispUp == null)
		{
			Debug.Log("m_dispUp is null!");
		}
		m_bgUp = m_dispUp.transform.Find("SerialID").gameObject;
		if (m_bgUp == null)
		{
			Debug.Log("m_bgUp is null!");
		}
		TextMeshProUGUI component = base.transform.Find("SerialID/SerialIDText").GetComponent<TextMeshProUGUI>();
		if (component == null)
		{
			Debug.Log("textObj is null!");
		}
		GitVersionInfo gitVersionInfo = GitVersionInfo.Load();
		component.text = string.Format(dispFormat, SystemUtility.Instance.SoftID, GetFormattedSystemID(), gitVersionInfo ? gitVersionInfo.CommitTime : "null", gitVersionInfo ? gitVersionInfo.Hash : "null");
	}

	private void Update()
	{
		if (base.gameObject != m_dispUp)
		{
			return;
		}
		bool flag = Bi2xInput.GetKeyDown(KeyCode.I) || ResidentFlow.CustomInput.GetMouseButtonDown(0);
		bool flag2 = Bi2xInput.GetKeyUp(KeyCode.I) || ResidentFlow.CustomInput.GetMouseButtonUp(0);
		switch (m_step)
		{
		case Step.INIT:
			s_enterObject = null;
			m_bgUp.SetActive(value: false);
			m_counter = 0;
			m_time = Time.time;
			m_step = Step.FIRST;
			break;
		case Step.FIRST:
			if ((bool)s_enterObject && s_enterObject != base.gameObject)
			{
				m_step = Step.WAIT;
				break;
			}
			if (flag)
			{
				m_step = Step.INIT;
			}
			if (Time.time - m_time >= 3f)
			{
				m_time = Time.time;
				m_step = Step.SECOND;
			}
			break;
		case Step.SECOND:
			if ((bool)s_enterObject && s_enterObject != base.gameObject)
			{
				m_step = Step.WAIT;
			}
			else if (flag)
			{
				s_enterObject = base.gameObject;
				m_time = Time.time;
				m_counter++;
				m_step = Step.THIRD;
			}
			break;
		case Step.THIRD:
			if ((bool)s_enterObject && s_enterObject != base.gameObject)
			{
				m_step = Step.WAIT;
				break;
			}
			if (Time.time - m_time >= 1f)
			{
				m_step = Step.INIT;
			}
			if (flag)
			{
				m_counter++;
			}
			if (m_counter >= 3)
			{
				m_step = Step.FOURTH;
			}
			break;
		case Step.FOURTH:
			if ((bool)s_enterObject && s_enterObject != base.gameObject)
			{
				m_step = Step.WAIT;
				break;
			}
			if (flag2)
			{
				m_step = Step.INIT;
			}
			if (Time.time - m_time >= 2f)
			{
				m_bgUp.SetActive(value: true);
				m_time = Time.time;
				m_step = Step.DISP;
			}
			break;
		case Step.DISP:
			if (Time.time - m_time >= 20f || flag)
			{
				m_step = Step.INIT;
			}
			break;
		case Step.WAIT:
			if (!m_bgUp.activeSelf)
			{
				m_step = Step.INIT;
			}
			break;
		}
	}
}
