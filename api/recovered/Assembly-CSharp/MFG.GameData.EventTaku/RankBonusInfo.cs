using System.Collections.Generic;
using MFG.Types;

namespace MFG.GameData.EventTaku;

public class RankBonusInfo
{
	public Rank Rank;

	public Dictionary<Rank, decimal> PointRatioByRank = new Dictionary<Rank, decimal>();

	public int TotalBonusPoint
	{
		get
		{
			if (!PointRatioByRank.ContainsKey(Rank))
			{
				return 0;
			}
			return decimal.ToInt32(PointRatioByRank[Rank]);
		}
	}
}
