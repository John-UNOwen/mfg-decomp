using Cysharp.Threading.Tasks;
using MFG.Types;
using UnityEngine;

public class LastTsumo : MonoBehaviour
{
	[SerializeField]
	private GameObject m_lastTsumoEffect;

	public async UniTask Play(CharaType type)
	{
		m_lastTsumoEffect.SetActive(value: true);
		m_lastTsumoEffect.GetComponent<Animator>().SetTrigger("Next");
		SoundManager.PlaySe("SFX_MJG_DISPLAY5").Forget();
		await UniTask.Delay(2000);
	}
}
