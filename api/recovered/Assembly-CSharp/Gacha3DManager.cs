using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.Types;
using MFG.Types.Customize.CutinItem;
using UnityEngine;

public class Gacha3DManager : MonoBehaviour
{
	[SerializeField]
	private GachaPlayFlow gachaPlayFlow;

	[SerializeField]
	private Animator anim;

	[SerializeField]
	private List<GameObject> smokes;

	public List<GameObject> Thumbnails;

	private GachaResultInfo Result;

	private int gachaCount = 1;

	private bool isRainbowSmoke;

	public void Init(GachaResultInfo gachaResultInfo, bool isRainbow)
	{
		Result = gachaResultInfo;
		isRainbowSmoke = isRainbow;
		gachaCount = Result.ItemInfos.Count;
		anim.SetInteger("GachaCount", gachaCount);
		anim.Update(0f);
	}

	public async UniTask PlayIn(CancellationToken cancellationToken = default(CancellationToken))
	{
		anim.SetTrigger("PlayIn");
		anim.Update(0f);
		await UniTask.WhenAny(UniTask.WaitUntil(() => anim.GetCurrentAnimatorStateInfo(0).IsName("GachaPlayLoop"), PlayerLoopTiming.Update, cancellationToken), UniTask.WaitUntil(() => gachaPlayFlow.IsTouchSkipAction(isOnlySkipButton: true), PlayerLoopTiming.Update, cancellationToken));
	}

	public async UniTask AnimStart(CancellationToken cancellationToken = default(CancellationToken))
	{
		anim.SetTrigger("Start");
		if (!gachaPlayFlow.IsForceSkip)
		{
			SoundManager.PlaySe("SFX_SYS_GACYA10").Forget();
		}
		anim.transform.Find("Blurred").Find("FireWorks").gameObject.SetActive(Result.IsUR1stConfirmed);
		await UniTask.WhenAny(UniTask.WaitUntil(() => anim.GetCurrentAnimatorStateInfo(0).IsName($"GachaPlayPai{gachaCount}In"), PlayerLoopTiming.Update, cancellationToken), UniTask.WaitUntil(() => gachaPlayFlow.IsTouchSkipAction(isOnlySkipButton: true), PlayerLoopTiming.Update, cancellationToken));
		SmokeStart();
		await UniTask.WhenAny(UniTask.WaitUntil(() => anim.GetCurrentAnimatorStateInfo(0).IsName("GachaChangeEnd"), PlayerLoopTiming.Update, cancellationToken), UniTask.WaitUntil(() => gachaPlayFlow.IsTouchSkipAction(), PlayerLoopTiming.Update, cancellationToken));
	}

	public async UniTask RarityChangeWait(int decisionIndex, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!Result.ItemInfos[decisionIndex].IsSameRarityIn1stAnd2nd)
		{
			string stateName = string.Format("GachaOpen{0}-{1}Loop", gachaCount, (gachaCount == 5) ? $"{decisionIndex + 1}" : "");
			await UniTask.WhenAny(UniTask.WaitUntil(() => anim.GetCurrentAnimatorStateInfo(0).IsName(stateName), PlayerLoopTiming.Update, cancellationToken), UniTask.WaitUntil(() => gachaPlayFlow.IsTouchSkipAction(isOnlySkipButton: true), PlayerLoopTiming.Update, cancellationToken));
		}
	}

	public void ThumbnailCloseUpStart(int index)
	{
		string stateName = ((index < gachaCount) ? string.Format("GachaOpen{0}{1}", gachaCount, (gachaCount == 5) ? $"-{index + 1}" : "") : $"GachaEnd{gachaCount}");
		anim.Play(stateName, 0, 0f);
		anim.Update(0f);
	}

	public void ThumbnailWaitingStart()
	{
		anim.Play($"GachaChange{gachaCount}", 0, 0f);
		anim.Update(0f);
	}

	public int ThumbnailChange(int curIndex)
	{
		bool flag = curIndex == -1 && gachaPlayFlow.IsTouchSkipAction();
		int num = ((gachaPlayFlow.IsForceSkip || curIndex + 1 >= gachaCount) ? gachaCount : (curIndex + 1));
		if (num >= gachaCount)
		{
			ThumbnailCloseUpStart(num);
			SkipThumbnaiRarityAll(ThumbnailChangePhase.Change2nd);
			return num;
		}
		if (!Result.ItemInfos[num].IsSameRarityIn1stAnd2nd)
		{
			ThumbnailCloseUpStart(num);
		}
		else
		{
			ThumbnailWaitingStart();
		}
		if (flag)
		{
			SkipThumbnaiRarityAll(ThumbnailChangePhase.Change1st);
		}
		return num;
	}

	private void SkipThumbnaiRarityAll(ThumbnailChangePhase thumbnailChangePhase)
	{
		StopCommonSe();
		for (int i = 0; i < gachaCount; i++)
		{
			ChangeThumbnaiRarity(i, thumbnailChangePhase, isSkip: true);
		}
	}

	public bool ChangeThumbnaiRarity(int itemIndex, ThumbnailChangePhase thumbnailChangePhase, bool isSkip = false)
	{
		GachaItemInfo gachaItemInfo = Result.ItemInfos[itemIndex];
		Animator component = Thumbnails[itemIndex].transform.Find("ThumbnailIn").GetComponent<Animator>();
		Rarity rarity = gachaItemInfo.ThumbnailChangeRaritys[thumbnailChangePhase];
		bool result = false;
		try
		{
			if (isSkip)
			{
				if (rarity == Rarity.N)
				{
					component.Play("ThumbnailLoop", 0, 1f);
				}
				else
				{
					component.Play("ThumbnailChange" + GachaPlayUtility.RarityColorName(rarity) + ((thumbnailChangePhase == ThumbnailChangePhase.Change2nd) ? "2nd" : ""), 0, 1f);
				}
			}
			else if ((thumbnailChangePhase == ThumbnailChangePhase.Change1st) ? (rarity > Rarity.N) : (rarity != gachaItemInfo.ThumbnailChangeRaritys[thumbnailChangePhase - 1]))
			{
				component.SetTrigger(GachaPlayUtility.RarityColorName(rarity) + ((thumbnailChangePhase == ThumbnailChangePhase.Change2nd) ? "2nd" : "1st"));
				result = true;
			}
		}
		catch
		{
			Debug.LogError("想定外のケース");
		}
		return result;
	}

	private void StopCommonSe()
	{
		float fade_time = 0.1f;
		SoundManager.StopSe("SFX_SYS_GACYA1", fade_time).Forget();
		SoundManager.StopSe("SFX_SYS_GACYA2", fade_time).Forget();
		SoundManager.StopSe("SFX_SYS_GACYA3", fade_time).Forget();
		SoundManager.StopSe("SFX_SYS_GACYA4", fade_time).Forget();
		SoundManager.StopSe("SFX_SYS_GACYA5", fade_time).Forget();
		SoundManager.StopSe("SFX_SYS_GACYA_N", fade_time).Forget();
		SoundManager.StopSe("SFX_SYS_GACYA_R", fade_time).Forget();
		SoundManager.StopSe("SFX_SYS_GACYA_SR1", fade_time).Forget();
		SoundManager.StopSe("SFX_SYS_GACYA_SR2", fade_time).Forget();
		SoundManager.StopSe("SFX_SYS_GACYA_UR1", fade_time).Forget();
		SoundManager.StopSe("SFX_SYS_GACYA_UR2", fade_time).Forget();
	}

	private void SmokeStart()
	{
		int num = (isRainbowSmoke ? 1 : 0);
		GameObject obj = smokes[num];
		obj.SetActive(value: true);
		obj.transform.Find("SmokeLeft").GetComponent<ParticleSystem>().Play();
		obj.transform.Find("SmokeRight").GetComponent<ParticleSystem>().Play();
		if (!gachaPlayFlow.IsForceSkip)
		{
			SoundManager.PlaySe($"SFX_SYS_GACYA{num + 11}").Forget();
		}
	}
}
