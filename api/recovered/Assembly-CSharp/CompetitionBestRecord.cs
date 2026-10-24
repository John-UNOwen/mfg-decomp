using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.GameData;
using UnityEngine;

public class CompetitionBestRecord : MonoBehaviour
{
	[SerializeField]
	private Animator hiscoreIconAnimator;

	private Animator resultSummaryAnimator => base.transform.GetComponent<Animator>();

	private CompetitionRankRateDisp currentBestRankRateDisp => base.transform.GetComponent<CompetitionRankRateDisp>();

	private CompetitionScoreDisp currentBestTotalScoreDisp => base.transform.GetComponent<CompetitionScoreDisp>();

	public void Init(CompetitionInfo competitionInfo)
	{
		currentBestRankRateDisp.Reflesh(competitionInfo.Rank, competitionInfo.RankRate);
		currentBestTotalScoreDisp.Reflesh(competitionInfo.BestTotalScore);
		if (resultSummaryAnimator != null)
		{
			resultSummaryAnimator.gameObject.SetActive(value: true);
		}
	}

	public async UniTask Reflesh(CompetitionInfo competitionInfo, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		try
		{
			if (resultSummaryAnimator != null)
			{
				resultSummaryAnimator.gameObject.SetActive(value: true);
				resultSummaryAnimator.SetTrigger("In");
				resultSummaryAnimator.Update(0f);
			}
			await UniTask.Delay(333, ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
			currentBestRankRateDisp.Reflesh(competitionInfo.Rank, competitionInfo.RankRate);
			currentBestTotalScoreDisp.Reflesh(competitionInfo.BestTotalScore);
			await UniTask.Delay(416, ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
			if (competitionInfo.IsHiScore)
			{
				await UniTask.Delay(500, ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
				SoundManager.PlaySe("SFX_MJG_BONUS1").Forget();
				if (hiscoreIconAnimator != null)
				{
					hiscoreIconAnimator.gameObject.SetActive(value: true);
					hiscoreIconAnimator.SetTrigger("In");
				}
			}
			else if (hiscoreIconAnimator != null)
			{
				hiscoreIconAnimator.gameObject.SetActive(value: false);
			}
		}
		catch (OperationCanceledException)
		{
			currentBestRankRateDisp.Reflesh(competitionInfo.Rank, competitionInfo.RankRate);
			currentBestTotalScoreDisp.Reflesh(competitionInfo.BestTotalScore);
			if (resultSummaryAnimator != null)
			{
				resultSummaryAnimator.Play("CompeScoreOut2", 0, 1f);
			}
			if (competitionInfo.IsHiScore)
			{
				if ((bool)hiscoreIconAnimator)
				{
					hiscoreIconAnimator.gameObject.SetActive(value: true);
					hiscoreIconAnimator.SetTrigger("In");
				}
			}
			else if ((bool)hiscoreIconAnimator)
			{
				hiscoreIconAnimator.gameObject.SetActive(value: false);
			}
		}
	}

	public void ClosingTask()
	{
		if (resultSummaryAnimator != null)
		{
			resultSummaryAnimator.gameObject.SetActive(value: false);
		}
		if (hiscoreIconAnimator != null)
		{
			hiscoreIconAnimator.gameObject.SetActive(value: false);
		}
	}

	public void RankSEPlay()
	{
		SoundManager.PlaySe("SFX_MJG_YAKU1").Forget();
	}

	public void ScoreSEPlay()
	{
		SoundManager.PlaySe("SFX_MJG_YAKU2").Forget();
	}
}
