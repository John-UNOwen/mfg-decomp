using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class AttentionMessage : MonoBehaviour
{
	public enum Text
	{
		Text1000Les,
		TextNoYaku,
		TextFuriten,
		TextMissedRon,
		Text5thKan,
		TextMachiChangeKan,
		TextFormChangeKan,
		TextNoAgariHai,
		TextKuikae,
		TextButtobi,
		TextMissedRonAfterReach,
		TextMax
	}

	[SerializeField]
	private GameObject m_textRoot;

	[SerializeField]
	private List<GameObject> m_texts = new List<GameObject>();

	private Text m_dispText = Text.TextMax;

	public void DispAttention(Text text)
	{
		if (text == m_dispText)
		{
			return;
		}
		base.gameObject.SetActive(value: true);
		SoundManager.PlaySe("SFX_MJG_COUNT1").Forget();
		foreach (GameObject text2 in m_texts)
		{
			text2.SetActive(value: false);
		}
		if (Text.Text1000Les <= text && text < Text.TextMax)
		{
			m_texts[(int)text].SetActive(value: true);
		}
		m_dispText = text;
	}

	public void Exit()
	{
		base.gameObject.SetActive(value: false);
		m_dispText = Text.TextMax;
	}
}
