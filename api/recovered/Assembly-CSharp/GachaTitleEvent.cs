using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class GachaTitleEvent : MonoBehaviour
{
	[SerializeField]
	private List<GameObject> m_cpChara;

	private void TitleIn()
	{
		for (int i = 0; i < 4; i++)
		{
			if (m_cpChara[i].activeInHierarchy)
			{
				m_cpChara[i].GetComponent<Animator>().SetTrigger("In");
			}
			Debug.Log($"GachaPlay ガチャ前兆キャラインその{i + 1} / m_cpChara[{i}] IsActice: {m_cpChara[i].activeSelf}");
		}
		if (m_cpChara[0].activeInHierarchy)
		{
			SoundManager.PlaySe("SFX_SYS_GACYA8").Forget();
		}
	}
}
