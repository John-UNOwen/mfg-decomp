using System;
using System.Collections.Generic;
using MFG.GameData.EventTaku.FireReach;
using MFG.Taikyoku.Command.Receive;
using MFG.Types;

namespace MFG.GameData.EventTaku;

[Serializable]
public class FireReachEventGameData : BaseEventGameData
{
	public enum BonusType
	{
		None,
		OverHaneman
	}

	private MFG.GameData.EventTaku.FireReach.ResultInfo _resultInfo = new MFG.GameData.EventTaku.FireReach.ResultInfo();

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

	private OtherBonusInfos _otherBonuses = new OtherBonusInfos
	{
		_datas = new Dictionary<string, BonusInfo> { 
		{
			BonusType.OverHaneman.ToString(),
			new BonusInfo
			{
				BonusTypeKey = BonusType.OverHaneman.ToString(),
				PointRatioByCount = 1m
			}
		} }
	};

	public override EventType EventType => EventType.FireReach;

	public override IResultInfo ResultInfo => _resultInfo;

	public override bool IsMatch(EventType eventType, TakuType takuType)
	{
		if (takuType != 0 || eventType != EventType)
		{
			return false;
		}
		return true;
	}

	public override void ResetForBeforeTaikyokuStart(List<Player> players)
	{
		_resultInfo.Init();
	}

	public override void OnTsumoAgari(TsumoAgari cmd, List<Player> players)
	{
		if (players[cmd.PlayerIndex].IsJicha())
		{
			_resultInfo.AddScore(cmd.Yaku);
		}
	}

	public override void OnRonAgari(Ron cmd, List<Player> players)
	{
		bool[] array = new bool[Enum.GetNames(typeof(MFG.Types.Player)).Length];
		foreach (Player player2 in players)
		{
			int num = players.IndexOf(player2);
			array[num] = cmd.RonFlag[num] != 0;
		}
		Player player = players.Find((Player x) => x.m_cha == Cha.Jicha);
		if (array[player.m_playerIndexAtCommand])
		{
			_resultInfo.AddScore(cmd.Yakus[player.m_playerIndexAtCommand]);
		}
	}

	public override IResultInfo RefleshResultInfo(int beforeTotalAcquiredPoint, MahjongGameData mahjongGameData, bool isJandri)
	{
		_rankBonus.Rank = (Rank)mahjongGameData.Rank;
		_resultInfo.IsJandriEffective = isJandri;
		_resultInfo.RankBonus = _rankBonus;
		_resultInfo.OtherBonuses = _otherBonuses;
		_resultInfo.BeforeTotalAcquiredPoint = beforeTotalAcquiredPoint;
		_resultInfo.SetBonusCount(_resultInfo.OtherBonuses);
		return _resultInfo;
	}
}
