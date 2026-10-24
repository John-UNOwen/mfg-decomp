using Cysharp.Threading.Tasks;
using UnityEngine;

public class DangerSEEvent : MonoBehaviour
{
	public void DangerSE()
	{
		if (base.gameObject.activeSelf)
		{
			SoundManager.PlaySe("SFX_MJG_DISPLAY22").Forget();
		}
	}
}
