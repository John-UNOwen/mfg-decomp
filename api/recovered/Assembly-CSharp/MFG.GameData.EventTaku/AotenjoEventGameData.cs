using System;
using System.Collections.Generic;
using System.Numerics;
using MFG.GameData.EventTaku.Aotenjo;
using MFG.Taikyoku.Command.Receive;
using MFG.Types;

namespace MFG.GameData.EventTaku;

[Serializable]
public class AotenjoEventGameData : BaseEventGameData
{
	private MFG.GameData.EventTaku.Aotenjo.ResultInfo _resultInfo = new MFG.GameData.EventTaku.Aotenjo.ResultInfo();

	public string BestScore = "0";

	private RankBonusInfo _rankBonus = new RankBonusInfo
	{
		PointRatioByRank = new Dictionary<Rank, decimal>
		{
			{
				Rank.Rank1,
				0m
			},
			{
				Rank.Rank2,
				0m
			},
			{
				Rank.Rank3,
				0m
			},
			{
				Rank.Rank4,
				0m
			}
		}
	};

	private OtherBonusInfos _otherBonuses = new OtherBonusInfos();

	private const Cha Cha_None = (Cha)(-1);

	public override EventType EventType => EventType.Aotenjo;

	public override IResultInfo ResultInfo => _resultInfo;

	public override bool IsMatch(EventType eventType, TakuType takuType)
	{
		if (!IsAvailable())
		{
			return false;
		}
		if (takuType != TakuType.Sanma || eventType != EventType)
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
		return _resultInfo;
	}

	public void SetBestScore(BigInteger score)
	{
		if (GameDataManager.Mahjong.IsPrivateMatch)
		{
			return;
		}
		try
		{
			BigInteger bigInteger = BigInteger.Parse(BestScore);
			if (score > bigInteger)
			{
				BestScore = score.ToString();
			}
		}
		catch
		{
		}
	}
}
