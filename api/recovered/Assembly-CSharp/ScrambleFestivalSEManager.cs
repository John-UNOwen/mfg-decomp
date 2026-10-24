using Cysharp.Threading.Tasks;
using UnityEngine;

public class ScrambleFestivalSEManager : MonoBehaviour
{
	public void Janpon1()
	{
		SoundManager.PlaySe("SFX_SYS_JANPON1").Forget();
	}

	public void Janpon2()
	{
		SoundManager.PlaySe("SFX_SYS_JANPON2").Forget();
	}

	public void Janpon3()
	{
		SoundManager.PlaySe("SFX_SYS_JANPON3").Forget();
	}

	public void Janpon4()
	{
		SoundManager.PlaySe("SFX_SYS_BONUS5").Forget();
	}

	public void EvolutionPlay()
	{
		SoundManager.PlaySe("SFX_SYS_RANKUP2").Forget();
	}

	public void FukidashiIn()
	{
		SoundManager.PlaySe("SFX_SYS_BONUS1").Forget();
	}
}
