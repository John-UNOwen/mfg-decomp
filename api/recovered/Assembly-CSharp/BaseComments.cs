using Cysharp.Threading.Tasks;
using UnityEngine;

public class BaseComments : MonoBehaviour
{
	public bool isPlaySe;

	public void PlaySnsSE()
	{
		if (isPlaySe)
		{
			SoundManager.PlaySe("SFX_STR_MANPU5").Forget();
		}
	}
}
