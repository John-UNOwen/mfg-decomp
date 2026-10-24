using System;
using MFG.Types;
using MFG.Types.YellCampaign;

namespace MFG.GameData;

[Serializable]
public class YellInfo
{
	public CharaType Chara;

	public int YellCount;

	public YellRank GetYellRank()
	{
		if (YellCount >= 1000)
		{
			return YellRank.RankS;
		}
		if (YellCount >= 500)
		{
			return YellRank.RankA;
		}
		if (YellCount >= 200)
		{
			return YellRank.RankB;
		}
		if (YellCount >= 50)
		{
			return YellRank.RankC;
		}
		return YellRank.RankNothing;
	}

	public void AddYell(int addNum)
	{
		YellCount += addNum;
	}
}
