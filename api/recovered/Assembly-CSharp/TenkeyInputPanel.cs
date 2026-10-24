using TMPro;
using UnityEngine;

public class TenkeyInputPanel : MonoBehaviour
{
	[SerializeField]
	private TenkeyInputParts[] m_inputParts;

	[SerializeField]
	private TextMeshProUGUI m_titleText;

	public int PanelLength => m_inputParts.Length;

	public void CreatePanel(bool _bFirst)
	{
		for (int i = 0; i < m_inputParts.Length; i++)
		{
			m_inputParts[i].Input(_input: false);
			m_inputParts[i].NowPosition(_now: false);
		}
		m_inputParts[0].NowPosition(_now: true);
		if (_bFirst)
		{
			m_titleText.text = "新規入力";
		}
		else
		{
			m_titleText.text = "確認入力";
		}
	}

	public void UpdateDispStatus(int _frameIndex)
	{
		int num = Mathf.Clamp(_frameIndex, 0, m_inputParts.Length - 1);
		int num2 = Mathf.Clamp(_frameIndex + 1, 0, m_inputParts.Length - 1);
		m_inputParts[num].Input(_input: true);
		m_inputParts[num].NowPosition(_now: false);
		m_inputParts[num2].NowPosition(_now: true);
	}

	public void SetInputPartsNowPosition(int _setIndex, bool _enable)
	{
		int num = Mathf.Clamp(_setIndex, 0, m_inputParts.Length - 1);
		m_inputParts[num].NowPosition(_enable);
	}

	public void SetInputPartsNumberPosition(int _setIndex, bool _enable)
	{
		int num = Mathf.Clamp(_setIndex, 0, m_inputParts.Length - 1);
		m_inputParts[num].Input(_enable);
	}
}
