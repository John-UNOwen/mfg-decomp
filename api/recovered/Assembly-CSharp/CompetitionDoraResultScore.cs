using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.GameData;
using UnityEngine;

public class CompetitionDoraResultScore : MonoBehaviour
{
	[SerializeField]
	private CompetitionScoreDisp baseScore;

	[SerializeField]
	private CompetitionScoreDisp doraScore;

	[SerializeField]
	private CompetitionScoreDisp scoreRate;

	[SerializeField]
	private CompetitionScoreDisp totalScore;

	public async UniTask Play(CompetitionInfo afterCompetitionInfo, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (afterCompetitionInfo.CompetitionStatus != 0)
		{
			await baseScore.RefleshWithEnterAnimation(afterCompetitionInfo.BaseLatestScore, cancellationToken, delegate
			{
				SoundManager.PlaySe("SFX_MJG_URADORA1").Forget();
			});
			await doraScore.RefleshWithEnterAnimation(afterCompetitionInfo.ExtraLatestScore, cancellationToken, delegate
			{
				SoundManager.PlaySe("SFX_MJG_URADORA1").Forget();
			});
			await scoreRate.RefleshWithEnterAnimation(((float)afterCompetitionInfo.BonusRatio / 1000f).ToString("F3") ?? "", cancellationToken, delegate
			{
				SoundManager.PlaySe("SFX_MJG_URADORA1").Forget();
			});
		}
		await totalScore.RefleshWithEnterAnimation(afterCompetitionInfo.LatestScore, cancellationToken, delegate
		{
			SoundManager.PlaySe("SFX_MJG_URADORA1").Forget();
		});
		await UniTask.Delay(TimeSpan.FromSeconds(0.5), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
	}

	private int GetTotalScore(int baseScore, int ratio)
	{
		int num = baseScore * ratio;
		bool num2 = num % 1000 >= 500;
		num -= num % 1000;
		if (num2)
		{
			num += 1000;
		}
		return num / 1000;
	}
}
