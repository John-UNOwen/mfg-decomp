using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RaycastInfo : MonoBehaviour
{
	private GameObject m_info;

	private TextMeshProUGUI m_text;

	private bool m_bMain;

	private static bool m_bExec;

	private static string m_log;

	private void GetFullLayerName(GameObject gameObject, out string name)
	{
		name = "";
		List<string> list = new List<string>();
		GameObject gameObject2 = gameObject;
		do
		{
			list.Add(gameObject2.name);
			if (gameObject2.transform.parent?.gameObject == null)
			{
				break;
			}
			gameObject2 = gameObject2.transform.parent.gameObject;
		}
		while (gameObject2 != null);
		list.Reverse();
		foreach (string item in list)
		{
			name = name + "/" + item;
		}
	}

	private void Awake()
	{
		if (!m_bExec)
		{
			m_bMain = true;
			m_bExec = true;
		}
	}

	private void Start()
	{
		m_info = base.transform.Find("RaycastInfo").gameObject;
		m_text = base.transform.Find("RaycastInfo/Text").GetComponent<TextMeshProUGUI>();
	}

	private void Update()
	{
	}
}
