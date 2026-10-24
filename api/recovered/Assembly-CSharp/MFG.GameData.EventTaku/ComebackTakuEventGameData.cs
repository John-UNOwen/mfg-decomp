using System;
using System.Collections.Generic;
using MFG.GameData.EventTaku.ComebackTaku;
using MFG.Taikyoku.Command.Receive;
using MFG.Types;

namespace MFG.GameData.EventTaku;

[Serializable]
public class ComebackTakuEventGameData : BaseEventGameData
{
	public enum BonusType
	{
		None,
		Tobi
	}

	private MFG.GameData.EventTaku.ComebackTaku.ResultInfo _resultInfo = new MFG.GameData.EventTaku.ComebackTaku.ResultInfo();

	private RankBonusInfo _rankBonus = new RankBonusInfo
	{
		PointRatioByRank = new Dictionary<Rank, decimal>
		{
			{
				Rank.Rank1,
				7m
			},
			{
				Rank.Rank2,
				4m
			},
			{
				Rank.Rank3,
				3m
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

	private OtherBonusInfos _otherBonuses = new OtherBonusInfos
	{
		_datas = new Dictionary<string, BonusInfo> { 
		{
			BonusType.Tobi.ToString(),
			new BonusInfo
			{
				BonusTypeKey = BonusType.Tobi.ToString(),
				PointRatioByCount = 1m
			}
		} }
	};

	[NonSerialized]
	public bool IsExtraBGM;

	public override EventType EventType => EventType.Comeback;

	public override IResultInfo ResultInfo => _resultInfo;

	public override bool IsMatch(EventType eventType, TakuType takuType)
	{
		if (takuType == TakuType.Nima || eventType != EventType)
		{
			return false;
		}
		return true;
	}

	public override void ResetForBeforeTaikyokuStart(List<Player> players)
	{
		_resultInfo.BlowAwayInfo.Init(players);
	}

	public override void OnRyukyoku(RyuKyoku cmd, List<Player> players)
	{
		_resultInfo.BlowAwayInfo.BlowawayState_Reflesh((Cha)(-1), players);
	}

	public override void OnTsumoAgari(TsumoAgari cmd, List<Player> players)
	{
		_resultInfo.BlowAwayInfo.BlowawayState_Reflesh(players[cmd.PlayerIndex].m_cha, players);
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
		Cha blowawayCha = ((!array[player.m_playerIndexAtCommand]) ? players[Array.FindIndex(array, (bool x) => x)].m_cha : Cha.Jicha);
		_resultInfo.BlowAwayInfo.BlowawayState_Reflesh(blowawayCha, players);
	}

	public override IResultInfo RefleshResultInfo(int beforeTotalAcquiredPoint, MahjongGameData mahjongGameData, bool isJandri)
	{
		RankBonusInfo rankBonusInfo = ((mahjongGameData.TakuType == TakuType.Sanma) ? _rankBonusSanma : _rankBonus);
		rankBonusInfo.Rank = (Rank)mahjongGameData.Rank;
		_resultInfo.IsJandriEffective = isJandri;
		_resultInfo.RankBonus = rankBonusInfo;
		_resultInfo.OtherBonuses = _otherBonuses;
		_resultInfo.BeforeTotalAcquiredPoint = beforeTotalAcquiredPoint;
		_resultInfo.SetBonusCount(_resultInfo.OtherBonuses, mahjongGameData.TakuType);
		return _resultInfo;
	}

	public override void OnKyokuInit(List<Player> players)
	{
		IsExtraBGM = false;
		foreach (Player player in players)
		{
			if (MahjongUtility.ComebackTaku.BonusHan(players, player.m_cha) >= 2)
			{
				IsExtraBGM = true;
			}
		}
	}

	public override bool IsExtraBGMAvailable()
	{
		return IsExtraBGM;
	}
}
