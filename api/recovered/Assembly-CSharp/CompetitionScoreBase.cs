using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.GameData;
using MFG.Types.Competition;
using TMPro;
using UnityEngine;

public class CompetitionScoreBase : MonoBehaviour
{
	[SerializeField]
	protected TextMeshProUGUI _explainText;

	[SerializeField]
	protected CompetitionBestRecord _competitionRecord;

	[SerializeField]
	protected CompetitionRankGraph _graph;

	public virtual async UniTask InitAtHome(CompetitionInfo info)
	{
		base.gameObject.SetActive(value: true);
		_explainText.text = info.ScoreTypeSummary;
		await _graph.Init(info);
		_competitionRecord.Init(info);
	}

	public virtual async UniTask InitAtResult(CompetitionInfo beforeCompetitionInfo, CompetitionInfo afterCompetitionInfo, CancellationToken cancellationToken = default(CancellationToken))
	{
		base.gameObject.SetActive(value: true);
		_explainText.text = beforeCompetitionInfo.ScoreTypeSummary;
		await _graph.Init(beforeCompetitionInfo, afterCompetitionInfo.CompetitionStatus != CompetitionStatus.None);
		_competitionRecord.Init(beforeCompetitionInfo);
	}

	public virtual async UniTask ResultAnimationPlay(CompetitionInfo afterCompetitionInfo, CancellationToken token = default(CancellationToken))
	{
	}

	public async UniTask HiScoreAnimationPlay(CompetitionInfo afterCompetitionInfo, CancellationToken token = default(CancellationToken))
	{
		await UniTask.WhenAny(UniTask.WaitUntil(() => ResultFlow.Skip()), _competitionRecord.Reflesh(afterCompetitionInfo, token));
		await UniTask.WhenAny(UniTask.WaitUntil(() => ResultFlow.Skip()), _graph.GraphReflesh(afterCompetitionInfo, isArrowAnimation: true, token));
		await UniTask.WhenAny(UniTask.WaitUntil(() => ResultFlow.Skip()), _graph.UntilTextAnimation(afterCompetitionInfo, token));
	}

	public virtual void Close()
	{
		_competitionRecord.ClosingTask();
		base.gameObject.SetActive(value: false);
	}
}
