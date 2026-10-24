using MFG.Taikyoku.Command.Receive;

namespace MFG.GameData.EventTaku.FireReach;

public class ResultInfo : MFG.GameData.EventTaku.ResultInfo
{
	private int _totalYakumanCount;

	private int _totalSanbaimanCount;

	private int _totalBaimanCount;

	private int _totalHanemanCount;

	public void Init()
	{
		_totalYakumanCount = 0;
		_totalSanbaimanCount = 0;
		_totalBaimanCount = 0;
		_totalHanemanCount = 0;
	}

	public void AddScore(YakuData yakuData)
	{
		if (yakuData.HanID >= 9)
		{
			int num = yakuData.HanID - 9 + 1;
			_totalYakumanCount += num;
		}
		else if (yakuData.HanID >= 8)
		{
			_totalSanbaimanCount++;
		}
		else if (yakuData.HanID >= 7)
		{
			_totalBaimanCount++;
		}
		else if (yakuData.HanID >= 6)
		{
			_totalHanemanCount++;
		}
	}

	public void SetBonusCount(OtherBonusInfos otherBonusInfos)
	{
		otherBonusInfos.BonusInfo(FireReachEventGameData.BonusType.OverHaneman.ToString()).BonusCount = _totalYakumanCount * 5 + _totalSanbaimanCount * 3 + _totalBaimanCount * 2 + _totalHanemanCount;
	}
}
