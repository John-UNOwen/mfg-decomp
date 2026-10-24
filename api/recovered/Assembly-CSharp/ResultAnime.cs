using Cysharp.Threading.Tasks;
using UnityEngine;

public class ResultAnime : MonoBehaviour
{
	public static bool m_levelUpStart;

	public static bool m_bonusNext;

	private void Start()
	{
		m_levelUpStart = false;
	}

	private void Update()
	{
	}

	public void StartLevelUp()
	{
		m_levelUpStart = true;
	}

	public static bool IsStartLevelUp()
	{
		return m_levelUpStart;
	}

	public static void ResetStartLevelUp()
	{
		m_levelUpStart = false;
	}

	public void SoundCallMedal()
	{
		switch (GameDataManager.Mahjong.Rank)
		{
		case 0:
			SoundManager.PlaySe("SFX_MJG_RANK1").Forget();
			break;
		case 1:
			SoundManager.PlaySe("SFX_MJG_RANK2").Forget();
			break;
		case 2:
			SoundManager.PlaySe("SFX_MJG_RANK3").Forget();
			break;
		case 3:
			SoundManager.PlaySe("SFX_MJG_RANK4").Forget();
			break;
		}
	}

	public void BonusNext()
	{
		m_bonusNext = true;
	}

	public static void ResetBonusNext()
	{
		m_bonusNext = false;
	}

	public static bool IsBonusNext()
	{
		return m_bonusNext;
	}
}
