using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.GameData;
using MFG.Types.Competition;
using UnityEngine;

public class CompetitionBestScoreTable : MonoBehaviour
{
	[SerializeField]
	private List<CompetitionScoreDisp> scoreLists;

	[SerializeField]
	private CompetitionScoreDisp bestScore;

	private const int HISCORE_DELAY_FRAME = 60;

	private const int SCORE_DELAY_FRAME = 10;

	private const int SCORE_IN_FRAME = 20;

	private const int SCORE_OUT_FRAME = 20;

	private const int SCORE_TOTAL_FRAME = 40;

	public void SetScore(CompetitionInfo info)
	{
		bool num = info.CompetitionStatus == CompetitionStatus.InProgress;
		List<int> list = (num ? info.Scores : info.BestScores);
		int score = (num ? info.TotalScore : info.BestTotalScore);
		int num2 = 0;
		foreach (CompetitionScoreDisp scoreList in scoreLists)
		{
			if (num2 < info.SpecifiedGameCount)
			{
				scoreList.Reflesh((list != null && list.Count > num2) ? list[num2] : (-1));
				scoreList.gameObject.SetActive(value: true);
			}
			else
			{
				scoreList.gameObject.SetActive(value: false);
			}
			num2++;
		}
		bestScore.Reflesh(score);
	}

	public async UniTask ResultPlay(CompetitionInfo info, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		try
		{
			(int, int) tuple = ScoreDisps_GetHiScoreDelayMilliSec(info);
			int hiScoreDelayMilliSec = tuple.Item1;
			int modifyElementCount = tuple.Item2;
			int num = ((info.ScoreType == ScoreType.BestScore || info.ScoreType == ScoreType.AccelDoraBestScore || info.ScoreType == ScoreType.MentanpinBestScore) ? info.LatestScoreIndex : 0);
			int i = num;
			while (i < info.SpecifiedGameCount)
			{
				CompetitionScoreDisp competitionScoreDisp = scoreLists[i];
				int score = ((i < info.Scores.Count) ? info.Scores[i] : (-1));
				if (i == info.LatestScoreIndex)
				{
					competitionScoreDisp.RefleshWithDelay(score, hiScoreDelayMilliSec, bestScoreEffectEnable: true, cancellationToken, delegate
					{
						SoundManager.PlaySe("SFX_EVT_BINGO2").Forget();
					}, delegate
					{
						SoundManager.PlaySe("SFX_MJG_YAKU1").Forget();
					}).Forget();
				}
				else
				{
					competitionScoreDisp.RefleshWithDelay(score, 0, bestScoreEffectEnable: false, cancellationToken, delegate
					{
						SoundManager.PlaySe("SFX_EVT_BINGO2").Forget();
					}, delegate
					{
						SoundManager.PlaySe("SFX_MJG_YAKU1").Forget();
					}).Forget();
				}
				if (i < info.SpecifiedGameCount - 1)
				{
					await UniTask.Delay(166, ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
				}
				int num2 = i + 1;
				i = num2;
			}
			if (modifyElementCount != 1)
			{
				await UniTask.Delay(2000, ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
			}
			else
			{
				await UniTask.Delay(1666, ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
			}
			await bestScore.RefleshWithDelay(info.TotalScore, 500, bestScoreEffectEnable: true, default(CancellationToken), null, delegate
			{
				SoundManager.PlaySe("SFX_MJG_YAKU2").Forget();
			});
		}
		catch (OperationCanceledException)
		{
		}
	}

	private (int hiScoreDelayMilliSec, int modifyElementCount) ScoreDisps_GetHiScoreDelayMilliSec(CompetitionInfo info)
	{
		int num = info.SpecifiedGameCount - info.LatestScoreIndex;
		int num2 = 0;
		num2 = ((num != 1) ? ((int)((float)(10 * (num - 1) - 20 + 40 + 60) / 60f * 1000f)) : 1000);
		return (hiScoreDelayMilliSec: num2, modifyElementCount: num);
	}
}
