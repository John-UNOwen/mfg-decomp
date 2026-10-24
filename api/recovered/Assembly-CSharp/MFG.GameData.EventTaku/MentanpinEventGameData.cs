using System;
using System.Collections.Generic;
using MFG.Types;

namespace MFG.GameData.EventTaku;

[Serializable]
public class MentanpinEventGameData : BaseEventGameData
{
	private ResultInfo _resultInfo = new ResultInfo();

	private RankBonusInfo _rankBonus = new RankBonusInfo
	{
		PointRatioByRank = new Dictionary<Rank, decimal>
		{
			{
				Rank.Rank1,
				5m
			},
			{
				Rank.Rank2,
				3m
			},
			{
				Rank.Rank3,
				2m
			},
			{
				Rank.Rank4,
				1m
			}
		}
	};

	private RankBonusInfo _rankBonusSanma = new RankBonusInfo
	{
		PointRatioByRank = new Dictionary<Rank, decimal>
		{
			{
				Rank.Rank1,
				4m
			},
			{
				Rank.Rank2,
				2m
			},
			{
				Rank.Rank3,
				1m
			}
		}
	};

	private OtherBonusInfos _otherBonuses = new OtherBonusInfos();

	public override EventType EventType => EventType.Mentanpin;

	public override IResultInfo ResultInfo => _resultInfo;

	public override bool IsMatch(EventType eventType, TakuType takuType)
	{
		if (takuType == TakuType.Nima || eventType != EventType)
		{
			return false;
		}
		return true;
	}

	public override IResultInfo RefleshResultInfo(int beforeTotalAcquiredPoint, MahjongGameData mahjongGameData, bool isJandri)
	{
		RankBonusInfo rankBonusInfo = ((mahjongGameData.TakuType == TakuType.Sanma) ? _rankBonusSanma : _rankBonus);
		rankBonusInfo.Rank = (Rank)mahjongGameData.Rank;
		_resultInfo.IsJandriEffective = isJandri;
		_resultInfo.RankBonus = rankBonusInfo;
		_resultInfo.OtherBonuses = _otherBonuses;
		_resultInfo.BeforeTotalAcquiredPoint = beforeTotalAcquiredPoint;
		return _resultInfo;
	}
}
