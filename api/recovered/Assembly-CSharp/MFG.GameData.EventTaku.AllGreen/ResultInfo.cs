namespace MFG.GameData.EventTaku.AllGreen;

public class ResultInfo : MFG.GameData.EventTaku.ResultInfo
{
	private int _totalScore;

	public void Init()
	{
		_totalScore = 0;
	}

	public void AddScore(int point)
	{
		_totalScore += point;
	}

	public void SetBonusCount(OtherBonusInfos otherBonusInfos)
	{
		otherBonusInfos.BonusInfo(AllGreenEventGameData.BonusType.AllGreen.ToString()).BonusCount = _totalScore;
	}
}
