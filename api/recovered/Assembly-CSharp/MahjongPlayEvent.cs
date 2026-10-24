using MFG.MahjongPlay.Cutin;
using UnityEngine;

public class MahjongPlayEvent : MonoBehaviour
{
	private static Player m_player;

	private static bool m_IsReachbouDispTiming;

	[SerializeField]
	private CutinManager m_cutinManager;

	public static void StartReachBouDisp(Player player)
	{
		m_player = player;
	}

	public void SetReachbouDispTiming()
	{
		if (m_player != null)
		{
			m_player.ReachBouDisp();
			m_player = null;
		}
	}

	public static bool IsReachbouDispTiming()
	{
		return m_IsReachbouDispTiming;
	}

	public static void ResetTsumohaiTiming()
	{
		m_IsReachbouDispTiming = false;
	}

	public void MahjongPlayEvent_OnReachBouLanding()
	{
		m_cutinManager.NotifyReachStickLanded();
	}
}
