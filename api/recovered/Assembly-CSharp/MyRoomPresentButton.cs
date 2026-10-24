using UnityEngine;

public class MyRoomPresentButton : MonoBehaviour
{
	[SerializeField]
	private MyRoomFavoWindow m_favoWindow;

	public void PlusButtonPush()
	{
		m_favoWindow.m_plusPushing = true;
	}

	public void PlusButtonRelease()
	{
		m_favoWindow.m_plusPushing = false;
	}

	public void MinusButtonPush()
	{
		m_favoWindow.m_minusPushing = true;
	}

	public void MinusButtonRelease()
	{
		m_favoWindow.m_minusPushing = false;
	}
}
