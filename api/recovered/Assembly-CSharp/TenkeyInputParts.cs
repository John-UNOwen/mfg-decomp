using UnityEngine;

public class TenkeyInputParts : MonoBehaviour
{
	[SerializeField]
	private GameObject m_now;

	[SerializeField]
	private GameObject m_number;

	public void NowPosition(bool _now)
	{
		m_now.SetActive(_now);
	}

	public void Input(bool _input)
	{
		m_number.SetActive(_input);
	}
}
