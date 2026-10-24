using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.Assets;
using MFG.GameData;
using MFG.Types.Competition;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class CompetitionOriginScorePlay : CompetitionScoreBase
{
	[SerializeField]
	private GameObject _entryCharaNamePlate;

	[SerializeField]
	private Image _entryCharaNameImage;

	[SerializeField]
	private GameObject _thisTimeScoreFrame;

	[SerializeField]
	private CompetitionScoreDisp _thisTimeScoreDisp;

	[SerializeField]
	private List<CompetitionScoreDisp> _competitionScoreDisps = new List<CompetitionScoreDisp>();

	[SerializeField]
	private CompetitionScoreDisp _currentTotalScoreDisp;

	private const int HISCORE_DELAY_FRAME = 60;

	private const int SCORE_DELAY_FRAME = 10;

	private const int SCORE_IN_FRAME = 20;

	private const int SCORE_OUT_FRAME = 20;

	private const int SCORE_TOTAL_FRAME = 40;

	public override async UniTask InitAtHome(CompetitionInfo info)
	{
		await base.InitAtHome(info);
		await HomeScoreAllRefresh(info);
	}

	public override async UniTask InitAtResult(CompetitionInfo beforeCompetitionInfo, CompetitionInfo afterCompetitionInfo, CancellationToken cancellationToken = default(CancellationToken))
	{
		await base.InitAtResult(beforeCompetitionInfo, afterCompetitionInfo, cancellationToken);
		await ResultScoreAllRefresh(beforeCompetitionInfo, afterCompetitionInfo, cancellationToken);
	}

	private async UniTask HomeScoreAllRefresh(CompetitionInfo competitionInfo)
	{
		await EntryCharaReflesh(competitionInfo);
		bool num = competitionInfo.CompetitionStatus == CompetitionStatus.InProgress;
		_thisTimeScoreFrame.SetActive(value: false);
		_thisTimeScoreDisp.Reflesh(-1);
		List<int> scores = (num ? competitionInfo.Scores : competitionInfo.BestScores);
		int score = (num ? competitionInfo.TotalScore : competitionInfo.BestTotalScore);
		ScoreDisps_Reflesh(competitionInfo.SpecifiedGameCount, scores);
		_currentTotalScoreDisp.gameObject.SetActive(value: true);
		_currentTotalScoreDisp.Reflesh(score);
	}

	private async UniTask ResultScoreAllRefresh(CompetitionInfo beforeCompetitionInfo, CompetitionInfo afterCompetitionInfo, CancellationToken cancellationToken = default(CancellationToken))
	{
		await EntryCharaReflesh(beforeCompetitionInfo);
		_thisTimeScoreFrame.SetActive(value: true);
		_currentTotalScoreDisp.gameObject.SetActive(value: true);
		_currentTotalScoreDisp.Reflesh(beforeCompetitionInfo.TotalScore);
		ScoreDisps_Reflesh(beforeCompetitionInfo.SpecifiedGameCount, beforeCompetitionInfo.Scores);
	}

	public override async UniTask ResultAnimationPlay(CompetitionInfo afterCompetitionInfo, CancellationToken token = default(CancellationToken))
	{
		if (afterCompetitionInfo.CompetitionStatus != 0)
		{
			await _thisTimeScoreDisp.RefleshWithEnterAnimation(afterCompetitionInfo.LatestScore, token, delegate
			{
				SoundManager.PlaySe("SFX_MJG_URADORA1").Forget();
			});
		}
		if (afterCompetitionInfo.LatestScoreIndex >= 0)
		{
			await ScoreDisps_RefleshWithDelay(afterCompetitionInfo, afterCompetitionInfo.LatestScoreIndex, token);
			await _currentTotalScoreDisp.RefleshWithDelay(afterCompetitionInfo.TotalScore, 500, bestScoreEffectEnable: true, default(CancellationToken), null, delegate
			{
				SoundManager.PlaySe("SFX_MJG_YAKU2").Forget();
			});
		}
		if (afterCompetitionInfo.IsHiScore)
		{
			await HiScoreAnimationPlay(afterCompetitionInfo, token);
		}
	}

	private async UniTask EntryCharaReflesh(CompetitionInfo competitionInfo)
	{
		if (competitionInfo.EntrySystemType != EntrySystemType.Character)
		{
			_entryCharaNamePlate.SetActive(value: false);
			return;
		}
		IDisposableAsset<Sprite> disposableAsset = await AssetLoader.LoadAssetAsync<Sprite>(AssetPathResolver.ResolveCompetitionScoreWindowAssets(competitionInfo.EntryCharaType, CompetitionScoreWindowAssetType.EntryCharaNameTexture));
		disposableAsset.AddTo(this);
		_entryCharaNameImage.sprite = disposableAsset.Asset;
		_entryCharaNamePlate.SetActive(value: true);
	}

	private void ScoreDisps_Reflesh(int specifiedGameCount, List<int> scores)
	{
		foreach (CompetitionScoreDisp competitionScoreDisp2 in _competitionScoreDisps)
		{
			competitionScoreDisp2.gameObject.SetActive(value: false);
		}
		for (int i = 0; i < specifiedGameCount; i++)
		{
			CompetitionScoreDisp competitionScoreDisp = _competitionScoreDisps[i];
			int score = ((i < scores.Count) ? scores[i] : (-1));
			competitionScoreDisp.Reflesh(score);
			competitionScoreDisp.gameObject.SetActive(value: true);
		}
	}

	private (int hiScoreDelayMilliSec, int modifyElementCount) ScoreDisps_GetHiScoreDelayMilliSec(CompetitionInfo competitionInfo, int hiScoreIndex)
	{
		int num = competitionInfo.SpecifiedGameCount - hiScoreIndex;
		int num2 = 0;
		num2 = ((num != 1) ? ((int)((float)(10 * (num - 1) - 20 + 40 + 60) / 60f * 1000f)) : 1000);
		return (hiScoreDelayMilliSec: num2, modifyElementCount: num);
	}

	private async UniTask ScoreDisps_RefleshWithDelay(CompetitionInfo competitionInfo, int hiScoreIndex, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		try
		{
			(int, int) tuple = ScoreDisps_GetHiScoreDelayMilliSec(competitionInfo, hiScoreIndex);
			int hiScoreDelayMilliSec = tuple.Item1;
			int modifyElementCount = tuple.Item2;
			int num = ((competitionInfo.ScoreType == ScoreType.BestScore || competitionInfo.ScoreType == ScoreType.AccelDoraBestScore || competitionInfo.ScoreType == ScoreType.MentanpinBestScore) ? hiScoreIndex : 0);
			int i = num;
			while (i < competitionInfo.SpecifiedGameCount)
			{
				CompetitionScoreDisp competitionScoreDisp = _competitionScoreDisps[i];
				int score = ((i < competitionInfo.Scores.Count) ? competitionInfo.Scores[i] : (-1));
				if (i == hiScoreIndex)
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
				if (i < competitionInfo.SpecifiedGameCount - 1)
				{
					await UniTask.Delay(166, ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
				}
				int num2 = i + 1;
				i = num2;
			}
			if (modifyElementCount == 1)
			{
				await UniTask.Delay(1666, ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
			}
			else
			{
				await UniTask.Delay(2000, ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
			}
		}
		catch (OperationCanceledException)
		{
		}
	}
}
