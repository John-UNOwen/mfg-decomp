using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MFG.Assets;
using MFG.GameData;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class CompetitionHalfItResultScore : MonoBehaviour
{
	[SerializeField]
	private GameObject buttobiScore;

	[SerializeField]
	private Image buttobiCharaImage;

	[SerializeField]
	private GameObject nonButtobiScore;

	[SerializeField]
	private CompetitionScoreDisp baseScore;

	[SerializeField]
	private CompetitionScoreDisp buttobasiScore;

	[SerializeField]
	private CompetitionScoreDisp diffScore;

	[SerializeField]
	private CompetitionScoreDisp totalScore;

	private bool isButtobi;

	private CompetitionInfo beforeCompetitionInfo;

	private CompetitionInfo afterCompetitionInfo;

	public async UniTask Init(CompetitionInfo beforeInfo, CompetitionInfo afterInfo)
	{
		isButtobi = afterInfo.IsButtobi;
		beforeCompetitionInfo = beforeInfo;
		afterCompetitionInfo = afterInfo;
		if (isButtobi)
		{
			buttobiScore.SetActive(value: true);
			nonButtobiScore.SetActive(value: false);
			IDisposableAsset<Sprite> disposableAsset = await AssetLoader.LoadAssetAsync<Sprite>($"CutinItem/{GameDataManager.Player.GetCurrentChara().charaType}/Houju.png");
			disposableAsset.AddTo(this);
			buttobiCharaImage.sprite = disposableAsset.Asset;
		}
		else
		{
			buttobiScore.SetActive(value: false);
			nonButtobiScore.SetActive(value: true);
		}
		totalScore.Reflesh(beforeInfo.TotalScore);
		diffScore.transform.Find("ScoreIndex").gameObject.SetActive(!isButtobi);
		diffScore.transform.Find("TobiIndex").gameObject.SetActive(isButtobi);
	}

	public async UniTask Play(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (afterCompetitionInfo.CompetitionStatus != 0)
		{
			if (!isButtobi)
			{
				await UniTask.WhenAny(UniTask.WaitUntil(() => ResultFlow.Skip()), baseScore.RefleshWithEnterAnimation(afterCompetitionInfo.LatestScore, cancellationToken, delegate
				{
					SoundManager.PlaySe("SFX_MJG_URADORA1").Forget();
				}));
				await UniTask.WhenAny(UniTask.WaitUntil(() => ResultFlow.Skip()), buttobasiScore.RefleshWithEnterAnimation(afterCompetitionInfo.ExtraLatestScore, cancellationToken, delegate
				{
					SoundManager.PlaySe("SFX_MJG_URADORA1").Forget();
				}));
			}
			else
			{
				Animator anim = buttobiScore.GetComponent<Animator>();
				anim.SetTrigger("In");
				await UniTask.WhenAny(UniTask.WaitUntil(() => ResultFlow.Skip()), UniTask.WaitUntil(() => anim.GetCurrentAnimatorStateInfo(0).IsName("CompeScoreBest1Loop"), PlayerLoopTiming.Update, cancellationToken));
				await UniTask.WhenAny(UniTask.WaitUntil(() => ResultFlow.Skip()), UniTask.Delay(TimeSpan.FromSeconds(0.5), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken));
			}
		}
		int diff = afterCompetitionInfo.LatestScore + afterCompetitionInfo.ExtraLatestScore;
		if (isButtobi)
		{
			await UniTask.WhenAny(UniTask.WaitUntil(() => ResultFlow.Skip()), diffScore.RefleshWithEnterAnimation("0.7倍", cancellationToken, delegate
			{
				SoundManager.PlaySe("SFX_MJG_INFO4").Forget();
			}));
		}
		else if (diff >= 0)
		{
			await UniTask.WhenAny(UniTask.WaitUntil(() => ResultFlow.Skip()), diffScore.RefleshWithEnterAnimation(diff, cancellationToken, delegate
			{
				SoundManager.PlaySe("SFX_MJG_URADORA1").Forget();
			}));
		}
		await UniTask.WhenAny(UniTask.WaitUntil(() => ResultFlow.Skip()), UniTask.Delay(TimeSpan.FromSeconds(0.5), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken));
		if (diff != 0)
		{
			List<Tweener> list = new List<Tweener>();
			float tweenTime = 1.5f;
			if (!isButtobi)
			{
				list.Add(DOVirtual.Float(diff, 0f, tweenTime, delegate(float value)
				{
					diffScore.Reflesh((int)value);
				}).OnComplete(delegate
				{
					diffScore.Reflesh(0);
				}));
			}
			list.Add(DOVirtual.Float(beforeCompetitionInfo.TotalScore, afterCompetitionInfo.TotalScore, tweenTime, delegate(float value)
			{
				totalScore.Reflesh((int)value);
			}).OnComplete(delegate
			{
				totalScore.Reflesh(afterCompetitionInfo.TotalScore);
			}));
			if (diff <= 0)
			{
				await SoundManager.PlaySe("SFX_SYS_METER2");
			}
			else
			{
				await SoundManager.PlaySe("SFX_MJG_POINT1");
			}
			await UniTask.WhenAny(UniTask.WaitUntil(() => ResultFlow.Skip()), UniTask.Delay(TimeSpan.FromSeconds(tweenTime)));
			if (diff > 0)
			{
				SoundManager.StopSe("SFX_MJG_POINT1").Forget();
				SoundManager.PlaySe("SFX_MJG_POINT2").Forget();
			}
			else
			{
				SoundManager.StopSe("SFX_SYS_METER2").Forget();
			}
		}
		await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(1.0)));
	}
}
