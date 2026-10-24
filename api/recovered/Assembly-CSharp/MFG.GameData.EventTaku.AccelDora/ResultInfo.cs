using System.Collections.Generic;
using MFG.Taikyoku.Command.Receive;
using MFG.Types;

namespace MFG.GameData.EventTaku.AccelDora;

public class ResultInfo : MFG.GameData.EventTaku.ResultInfo
{
	private int _totalDoraUseCount;

	private int _totalYakumanCount;

	public void Init()
	{
		_totalDoraUseCount = 0;
		_totalYakumanCount = 0;
	}

	public void AddScore(YakuData yakuData)
	{
		List<Yaku> list = MahjongUtility.CreateYakusFromYakuBit(yakuData.YakuBit);
		bool num = yakuData.HanID >= 9;
		bool flag = list.Contains(Yaku.Kazoeyakuman);
		if (num && !flag)
		{
			int num2 = yakuData.HanID - 9 + 1;
			_totalYakumanCount += num2;
		}
		else
		{
			_totalDoraUseCount += yakuData.DoraNum;
		}
	}

	public void SetBonusCount(OtherBonusInfos otherBonusInfos)
	{
		otherBonusInfos.BonusInfo(AccelDoraEventGameData.BonusType.DoraCount.ToString()).BonusCount = _totalDoraUseCount;
		otherBonusInfos.BonusInfo(AccelDoraEventGameData.BonusType.YakumanCount.ToString()).BonusCount = _totalYakumanCount;
	}
}
