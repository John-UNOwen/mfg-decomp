using UnityEngine;

public class HaiShantenIcon : MonoBehaviour
{
	public enum State
	{
		None,
		Up,
		Down
	}

	[SerializeField]
	private GameObject m_iconUp;

	[SerializeField]
	private GameObject m_iconDown;

	public void SetIconState(State _state)
	{
		m_iconUp.SetActive(_state == State.Up);
		m_iconDown.SetActive(_state == State.Down);
	}
}
