using Cysharp.Threading.Tasks;
using UnityEngine;

public class StampAnimeEvent : MonoBehaviour
{
	public void PushStampSE()
	{
		SoundManager.PlaySe("SFX_MJG_INFO4").Forget();
	}
}
