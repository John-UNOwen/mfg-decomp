using Cysharp.Threading.Tasks;
using UnityEngine;

public class AccelDoraAnimationEventNotifier : MonoBehaviour
{
	public void RankSe()
	{
		SoundManager.PlaySe($"SFX_MJG_RANK{GameDataManager.Mahjong.Rank + 1}").Forget();
	}

	public void GetWindowOpen()
	{
		SoundManager.PlaySe("SFX_SYS_SLIDE2").Forget();
	}
}
