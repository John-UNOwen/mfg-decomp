using Cysharp.Threading.Tasks;
using UnityEngine;

public class TenshiGowanAnimeEvent : MonoBehaviour
{
	public void PlayMeter()
	{
		SoundManager.PlaySe("SFX_SYS_METER3").Forget();
	}

	public void StartSlide()
	{
		SoundManager.PlaySe("SFX_SYS_SLIDE2").Forget();
	}
}
