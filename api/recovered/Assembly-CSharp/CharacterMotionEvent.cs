using UnityEngine;

public class CharacterMotionEvent : MonoBehaviour
{
	private bool m_IsTsumohaiTiming;

	private bool m_IsSutehaiTiming;

	public void SetTsumohaiTiming()
	{
		m_IsTsumohaiTiming = true;
	}

	public bool IsTsumohaiTiming()
	{
		return m_IsTsumohaiTiming;
	}

	public void ResetTsumohaiTiming()
	{
		m_IsTsumohaiTiming = false;
	}

	public void SetSutehaiTiming()
	{
		m_IsSutehaiTiming = true;
	}

	public bool IsSutehaiTiming()
	{
		return m_IsSutehaiTiming;
	}

	public void ResetSutehaiTiming()
	{
		m_IsSutehaiTiming = false;
	}
}
