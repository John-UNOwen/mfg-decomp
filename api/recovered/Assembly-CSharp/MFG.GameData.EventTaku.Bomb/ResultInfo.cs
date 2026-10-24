namespace MFG.GameData.EventTaku.Bomb;

public class ResultInfo : MFG.GameData.EventTaku.ResultInfo
{
	private int _totalScore;

	public void Init()
	{
		_totalScore = 0;
	}

	public void AddScore()
	{
		_totalScore++;
	}

	public void SetBonusCount(OtherBonusInfos otherBonusInfos)
	{
		otherBonusInfos.BonusInfo(BombEventGameData.BonusType.Bomber.ToString()).BonusCount = _totalScore;
	}
}
