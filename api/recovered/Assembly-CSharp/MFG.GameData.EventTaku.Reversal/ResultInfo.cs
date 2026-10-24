namespace MFG.GameData.EventTaku.Reversal;

public class ResultInfo : MFG.GameData.EventTaku.ResultInfo
{
	private long _totalScore;

	public void Init()
	{
		_totalScore = 0L;
	}

	public void AddScore(long score)
	{
		_totalScore += score;
	}

	public void SetBonusCount(OtherBonusInfos otherBonusInfos)
	{
		otherBonusInfos.BonusInfo(ReversalEventGameData.BonusType.Score.ToString()).BonusCount = (int)(_totalScore / 10000);
	}
}
