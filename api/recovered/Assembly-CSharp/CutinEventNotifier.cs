using System;
using MFG.MahjongPlay.Cutin;
using MFG.Types;
using UnityEngine;

public class CutinEventNotifier : MonoBehaviour
{
	public GameObject m_cutinHandTilesParent;

	[NonSerialized]
	public GameObject m_subCutinParent;

	[NonSerialized]
	public IPlayerInteractor m_sourcePlayerInteractor;

	[NonSerialized]
	public CutinManager m_cutinManager;

	public void CutinHandTiles_Play()
	{
		if (!(m_cutinHandTilesParent == null))
		{
			CutinHaiController component = m_cutinHandTilesParent.GetComponent<CutinHaiController>();
			if (!(component == null))
			{
				component.Play();
			}
		}
	}

	public void SubCutin_PlayIfYakuman()
	{
		if (m_sourcePlayerInteractor != null && m_sourcePlayerInteractor.PaymentAmount() >= Han.Yakuman && !(m_subCutinParent == null))
		{
			CutinController component = m_subCutinParent.GetComponent<CutinController>();
			if (!(component == null))
			{
				component.Play();
			}
		}
	}

	public void PlayHantextSE()
	{
		SmashAgariCutin component = GetComponent<SmashAgariCutin>();
		if (component != null)
		{
			component.NotifyPlayHantextSE();
		}
	}

	public void ExecScreenShot()
	{
		if (m_cutinManager != null)
		{
			m_cutinManager.ExecScreenShot();
		}
	}
}
