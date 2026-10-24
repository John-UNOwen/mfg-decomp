using Cysharp.Threading.Tasks;
using MFG.MahjongPlay.Cutin;
using MFG.Types;
using UnityEngine;

public class SoundCall : MonoBehaviour
{
	public CharaType MemorialPrintCharaType;

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void PlaySe(string label)
	{
		if (!(label == "SFX_MJG_CHEER2"))
		{
			if (label.Contains("VOX_GXX_"))
			{
				SoundManager.PlayMyCharaVoice(label).Forget();
			}
			else
			{
				SoundManager.PlaySe(label).Forget();
			}
		}
	}

	public void StopSe(string label)
	{
		SoundManager.StopSe(label).Forget();
	}

	public void PlaySeMahjongPlayTopTitle1()
	{
		if (GameDataManager.Mahjong.EventType == MFG.Types.EventType.Bomb)
		{
			SoundManager.PlaySe("SFX_STR_EVENT20").Forget();
		}
		else
		{
			SoundManager.PlaySe("SFX_MJG_TITLE1").Forget();
		}
		SoundManager.PlayAllCharaVoice("VOX_GXX_MJG_TITLE1");
	}

	public void PlaySeMahjongPlayTopTitle2()
	{
		SoundManager.PlaySe("SFX_MJG_TITLE2").Forget();
		SoundManager.PlayAllCharaVoice("VOX_GXX_MJG_TITLE2");
	}

	public void PlaySeMahjongPlayHaiShantenCount()
	{
		GetComponentInParent<HaiShantenCounter>()?.PlaySe();
	}

	public void PlaySEBombIn()
	{
		SoundManager.PlaySe("SFX_MJG_DISPLAY34").Forget();
	}

	public void PlaySeMemorialPrintChara()
	{
		SoundManager.PlaySe(SoundManager.GetCharaVoiceLabel("VOX_GXX_SYSTEM8", MemorialPrintCharaType)).Forget();
	}

	public void ButtobiGalaxy_PlayCharaKo1()
	{
		BlowAwayEventCutin componentInParent = GetComponentInParent<BlowAwayEventCutin>();
		if (componentInParent != null)
		{
			componentInParent.PlayBlowAwayKo1SE();
		}
	}

	public void ButtobiGalaxy_PlayCharaKo2()
	{
		BlowAwayEventCutin componentInParent = GetComponentInParent<BlowAwayEventCutin>();
		if (componentInParent != null)
		{
			componentInParent.PlayBlowAwayKo2SE();
		}
	}

	public void ButtobiGalaxy_PlayEyeFlash()
	{
		BlowAwayEventCutin componentInParent = GetComponentInParent<BlowAwayEventCutin>();
		if (componentInParent != null)
		{
			componentInParent.PlayEyeFlashSE();
		}
	}
}
