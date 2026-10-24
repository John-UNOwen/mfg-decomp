using Cysharp.Threading.Tasks;
using MFG.Types;
using MFG.Types.Customize.CutinItem;
using UnityEngine;

public class GachaRarityChangeNotifier : MonoBehaviour
{
	[SerializeField]
	private GachaPlayFlow m_gachaPlayFlow;

	[SerializeField]
	private Gacha3DManager m_gacha3D;

	private bool _changeThumbnaiRarity1stAllCalled;

	public void ThumbnaiIn(int index)
	{
		index--;
		SoundManager.PlaySe("SFX_SYS_GACYA2", -1f, (m_gachaPlayFlow.GachaCount() == 1) ? 0f : ((float)index * 0.25f - 0.5f)).Forget();
	}

	public void Thumbnai2ndCloseup(int index)
	{
		index--;
		SoundManager.PlaySe("SFX_SYS_GACYA4").Forget();
	}

	public void Thumbnai2ndCloseup2(int index)
	{
		index--;
		SoundManager.PlaySe("SFX_SYS_GACYA5").Forget();
	}

	public void RarityBigEnter()
	{
		Rarity rarity = m_gachaPlayFlow.CurrentItemMaster?.Rarity ?? Rarity.None;
		SoundManager.PlaySe("SFX_SYS_GACYA_" + ((rarity == Rarity.UR) ? "UR" : "SR") + "1").Forget();
	}

	public void RarityLanding()
	{
		Rarity rarity = m_gachaPlayFlow.CurrentItemMaster?.Rarity ?? Rarity.None;
		SoundManager.PlaySe(string.Format("SFX_SYS_GACYA_{0}{1}", rarity, (rarity >= Rarity.SR) ? "2" : "")).Forget();
	}

	public void RarityChange1stAll()
	{
		ChangeThumbnaiRarity1stAll();
	}

	public void RarityChange2nd(int index)
	{
		index--;
		ChangeThumbnaiRarity2nd(index);
	}

	private void ChangeThumbnaiRarity1stAll()
	{
		if (_changeThumbnaiRarity1stAllCalled)
		{
			return;
		}
		_changeThumbnaiRarity1stAllCalled = true;
		bool flag = false;
		for (int i = 0; i < m_gachaPlayFlow.GachaCount(); i++)
		{
			if (m_gacha3D.ChangeThumbnaiRarity(i, ThumbnailChangePhase.Change1st))
			{
				flag = true;
			}
		}
		if (flag)
		{
			SoundManager.PlaySe("SFX_SYS_GACYA3").Forget();
		}
	}

	private void ChangeThumbnaiRarity2nd(int itemIndex)
	{
		if (m_gacha3D.ChangeThumbnaiRarity(itemIndex, ThumbnailChangePhase.Change2nd))
		{
			SoundManager.PlaySe("SFX_SYS_GACYA6").Forget();
		}
	}
}
