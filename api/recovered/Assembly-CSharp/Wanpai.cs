using Cysharp.Threading.Tasks;
using MFG.MahjonPlay.Hand;
using UnityEngine;

public class Wanpai : MonoBehaviour
{
	[SerializeField]
	public Rinshan rinshan;

	[SerializeField]
	public Dora dora;

	[SerializeField]
	public Uradora uradora;

	public Counter GetOpenHaiCounter()
	{
		return dora.GetOpenHaiCounter();
	}

	public async UniTask OpenUradora(int openNum)
	{
		dora.MoveUradoraOpenPosition().Forget();
		await uradora.SetOpenNum(openNum);
	}

	public void Destruct()
	{
		dora.Destruct();
		uradora.Destruct();
	}
}
