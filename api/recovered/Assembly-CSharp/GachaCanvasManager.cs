using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class GachaCanvasManager : MonoBehaviour
{
	[SerializeField]
	private GachaPlayFlow gachaPlayFlow;

	[SerializeField]
	private List<GameObject> sdChara;

	[SerializeField]
	private GameObject touchPlease;

	[SerializeField]
	private GameObject titleLogo;

	private int sdCharaIndex;

	private void Update()
	{
		if (gachaPlayFlow.IsForceSkip)
		{
			touchPlease.SetActive(value: false);
			titleLogo.SetActive(value: false);
		}
	}

	public void RouletteAnimEnd()
	{
		titleLogo.transform.Find("TitleAnime").GetComponent<Animator>().SetTrigger("In");
	}

	public void SDCharaInit(int index)
	{
		sdCharaIndex = index;
		for (int i = 0; i < sdCharaIndex; i++)
		{
			sdChara[i].SetActive(value: true);
		}
	}

	public async UniTask TouchPlease(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!gachaPlayFlow.IsForceSkip)
		{
			touchPlease.transform.GetComponent<Animator>().SetTrigger("In");
			SoundManager.PlaySe("SFX_SYS_GACYA9").Forget();
			await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(10.0), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken), UniTask.WaitUntil(() => gachaPlayFlow.IsTouchSkipAction(), PlayerLoopTiming.Update, cancellationToken));
			for (int i = 0; i < sdCharaIndex; i++)
			{
				sdChara[i].GetComponent<Animator>().SetTrigger("Out");
			}
			touchPlease.transform.GetComponent<Animator>().SetTrigger("Out");
			titleLogo.SetActive(value: false);
		}
	}
}
