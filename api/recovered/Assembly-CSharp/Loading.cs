using TMPro;
using UnityEngine;

public class Loading : SingletonBehaviour<Loading>
{
	[SerializeField]
	private GameObject m_root;

	[SerializeField]
	private TextMeshProUGUI m_text;

	public void Connecting()
	{
		m_text.text = TextManager.Get(TextManager.KEY.COMMON_CONNECTING);
		if (m_root != null)
		{
			m_root.SetActive(value: true);
		}
	}

	public void Connecting(string _string)
	{
		m_text.text = _string;
		if (m_root != null)
		{
			m_root.SetActive(value: true);
		}
	}

	public void DisConnect()
	{
		if (m_root != null)
		{
			m_root.SetActive(value: false);
		}
	}
}
