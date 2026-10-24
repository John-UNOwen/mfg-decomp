using KAM.Usefully;
using TMPro;
using UnityEngine;

public class SerialIDDispCommand : MonoBehaviour
{
	[SerializeField]
	private GameObject m_disp;

	private int m_step;

	private float m_lastTime;

	private bool IsTrigger()
	{
		if (!Bi2xInput.GetKeyDown(KeyCode.Alpha0))
		{
			return ResidentFlow.CustomInput.GetMouseButtonDown(0);
		}
		return true;
	}

	private bool IsRelease()
	{
		if (!Bi2xInput.GetKeyUp(KeyCode.Alpha0))
		{
			return ResidentFlow.CustomInput.GetMouseButtonUp(0);
		}
		return true;
	}

	private void Start()
	{
		m_disp.GetComponentInChildren<TextMeshProUGUI>().text = "SOFTWARE ID <indent=52%>" + SystemUtility.Instance.SoftID + "</indent>\nSYSTEM ID <indent=52%>" + SystemUtility.Instance.SystemID + "</indent>\nHARDWARE ID <indent=52%>" + SystemUtility.Instance.HardwareID + "</indent>\n" + $"GAME <indent=52%>{GetGitCommitInfoString()}.{Screen.width}.{Screen.height}</indent>\n";
	}

	private string GetGitCommitInfoString()
	{
		GitVersionInfo gitVersionInfo = GitVersionInfo.Load();
		if (gitVersionInfo == null)
		{
			return "";
		}
		return gitVersionInfo.CommitTime + "." + gitVersionInfo.Hash;
	}

	private void Update()
	{
		float num = Time.time - m_lastTime;
		switch (m_step)
		{
		case 0:
			m_step++;
			break;
		case 1:
			if (IsTrigger())
			{
				if (num >= 3f)
				{
					m_lastTime = Time.time;
					m_step = 2;
				}
				else
				{
					m_step = 0;
				}
			}
			break;
		case 2:
			if (IsTrigger() || num >= 1f)
			{
				if (num >= 1f)
				{
					m_step = 0;
				}
				else
				{
					m_step = 3;
				}
			}
			break;
		case 3:
			if (IsTrigger() || num >= 1f)
			{
				if (num >= 1f)
				{
					m_step = 0;
					break;
				}
				m_lastTime = Time.time;
				m_step = 4;
			}
			break;
		case 4:
			if (IsRelease() || num >= 2f)
			{
				if (num < 2f)
				{
					m_step = 0;
					break;
				}
				m_lastTime = Time.time;
				m_disp.SetActive(value: true);
				m_step = 5;
			}
			break;
		case 5:
			if (IsTrigger() || num >= 20f)
			{
				m_disp.SetActive(value: false);
				m_step = 0;
			}
			break;
		}
	}
}
