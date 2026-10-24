using UnityEngine;
using UnityEngine.UI;

public class DebugPlaySoundLabel : MonoBehaviour
{
	[SerializeField]
	public Text m_text;

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void AddPlaySoundLable(string label)
	{
		string text = "<color=white>";
		if (label.StartsWith("BGM"))
		{
			text = "<color=red>";
		}
		else if (label.StartsWith("VOX"))
		{
			text = "<color=orange>";
		}
		m_text.text = text + label + "</color>\n" + m_text.text;
	}
}
