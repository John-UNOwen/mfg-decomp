using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.GameData;
using UnityEngine;
using UnityEngine.UI;

public class CompetitionDoraBonusScorePlay : CompetitionScoreBase
{
	[SerializeField]
	protected GameObject _homeScoreAll;

	[SerializeField]
	protected GameObject _resultScoreAll;

	[SerializeField]
	private CompetitionDoraResultScore _resultScore;

	[SerializeField]
	private VerticalLayoutGroup _bgVertical;

	private CompetitionBestScoreTable _scoreTable;

	public override async UniTask InitAtHome(CompetitionInfo info)
	{
		_homeScoreAll.SetActive(value: true);
		_resultScoreAll.SetActive(value: false);
		_scoreTable = _homeScoreAll.transform.Find("BestScoreTable").GetComponent<CompetitionBestScoreTable>();
		_scoreTable.SetScore(info);
		await base.InitAtHome(info);
	}

	public override async UniTask InitAtResult(CompetitionInfo beforeCompetitionInfo, CompetitionInfo afterCompetitionInfo, CancellationToken cancellationToken = default(CancellationToken))
	{
		_homeScoreAll.SetActive(value: false);
		_resultScoreAll.SetActive(value: true);
		_scoreTable = _resultScoreAll.transform.Find("BestScoreTable").GetComponent<CompetitionBestScoreTable>();
		_bgVertical.spacing = 0f;
		_bgVertical.padding.top = 30;
		_scoreTable.SetScore(beforeCompetitionInfo);
		await base.InitAtResult(beforeCompetitionInfo, afterCompetitionInfo, cancellationToken);
	}

	public override async UniTask ResultAnimationPlay(CompetitionInfo afterCompetitionInfo, CancellationToken token = default(CancellationToken))
	{
		await _resultScore.Play(afterCompetitionInfo);
		if (afterCompetitionInfo.LatestScoreIndex >= 0)
		{
			await _scoreTable.ResultPlay(afterCompetitionInfo, token);
		}
		if (afterCompetitionInfo.IsHiScore)
		{
			await HiScoreAnimationPlay(afterCompetitionInfo, token);
		}
	}
}
