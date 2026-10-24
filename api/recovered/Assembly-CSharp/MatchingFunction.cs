using UnityEngine;

public class MatchingFunction : MonoBehaviour
{
	public static bool m_next;

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void SetNext()
	{
		m_next = true;
	}

	public static void SetNextStatic()
	{
		m_next = true;
	}

	public static bool IsNext()
	{
		return m_next;
	}

	public static void ResetNext()
	{
		m_next = false;
	}
}
