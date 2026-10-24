using System;
using System.Collections.Generic;
using System.Numerics;
using MFG.Taikyoku;
using MFG.Taikyoku.Command;
using MFG.Types;
using MFG.Types.PlayerItem;
using UnityEngine;

namespace MFG.GameData;

[Serializable]
public class MahjongGameData : GameDataBase
{
	private const int CurrentDataVersion = 1;

	public int DataVersion = 1;

	public TakuType TakuType;

	public MFG.Types.EventType EventType;

	public bool IsConstancyTaku;

	public bool IsPrivateMatch;

	public bool IsCompetitionTaku;

	public List<ItemID> TaikyokuItems = new List<ItemID>();

	public int Rank;

	public long Score;

	public bool IsKiken;

	public GainLossType GainLossType = GainLossType.Risky;

	public MahjongResultInfo ResultInfo = new MahjongResultInfo();

	public ShowResultInfo ShowResultInfo = new ShowResultInfo();

	public (float luck, float speed, float attack, float defence) Fans = (luck: 0f, speed: 0f, attack: 0f, defence: 0f);

	[NonSerialized]
	public BigInteger AotenjoScore = 0;

	public int TsumikomiID;

	public bool TsumikomiCpuTsumogiri = true;

	public Kaze TsumikomiChicha;

	public bool IsDebugPlay;

	public bool IsHaifuPlay;

	public List<Pai> CustomTsumikomi;

	public int DebugScore = 20000;

	public bool IsDebugScoreEnable;

	[NonSerialized]
	public GameObject DebugSelectingPai;

	public bool IsHaifuPlayToExit;

	[NonSerialized]
	public bool IsRecNakiCutIn;

	[NonSerialized]
	public bool IsRecReachCutIn;

	[NonSerialized]
	public bool IsRecAgariCutIn;

	[NonSerialized]
	public string CutinRecItemOID = "";

	public bool IsAotenjoMode => EventType == MFG.Types.EventType.Aotenjo;

	public bool IsAccelDoraMode
	{
		get
		{
			if (EventType != MFG.Types.EventType.AccelDora)
			{
				return IsAotenjoMode;
			}
			return true;
		}
	}

	public bool IsFireReachMode
	{
		get
		{
			if (EventType != MFG.Types.EventType.FireReach && EventType != MFG.Types.EventType.FireGalaxy)
			{
				return IsAotenjoMode;
			}
			return true;
		}
	}

	public bool IsBlowAwayMode
	{
		get
		{
			if (EventType != MFG.Types.EventType.ToitoiButtobiGalaxy && EventType != MFG.Types.EventType.FireGalaxy && EventType != MFG.Types.EventType.PCKGalaxy)
			{
				return IsAotenjoMode;
			}
			return true;
		}
	}

	public bool IsMentanpinMode
	{
		get
		{
			if (EventType != MFG.Types.EventType.Mentanpin)
			{
				return IsAotenjoMode;
			}
			return true;
		}
	}

	public bool IsJihaiUnusedSanma
	{
		get
		{
			if (EventType == MFG.Types.EventType.Mentanpin)
			{
				return TakuType == TakuType.Sanma;
			}
			return false;
		}
	}

	public bool IsComebackMode
	{
		get
		{
			if (EventType != MFG.Types.EventType.Comeback)
			{
				return IsAotenjoMode;
			}
			return true;
		}
	}

	public bool IsKirisameMode
	{
		get
		{
			if (EventType != MFG.Types.EventType.Kirisame)
			{
				return IsAotenjoMode;
			}
			return true;
		}
	}

	public bool IsMeldBonusMode
	{
		get
		{
			if (EventType != MFG.Types.EventType.MeldBonus && EventType != MFG.Types.EventType.PCKGalaxy)
			{
				return IsAotenjoMode;
			}
			return true;
		}
	}

	public bool IsAllGreenBonusMode
	{
		get
		{
			if (EventType != MFG.Types.EventType.AllGreen)
			{
				return IsAotenjoMode;
			}
			return true;
		}
	}

	public bool IsBonusEnableMode
	{
		get
		{
			if (!IsMeldBonusMode && !IsComebackMode && !IsAllGreenBonusMode)
			{
				return IsAotenjoMode;
			}
			return true;
		}
	}

	public bool IsReversalEnableMode
	{
		get
		{
			if (EventType != MFG.Types.EventType.Reversal)
			{
				return IsAotenjoMode;
			}
			return true;
		}
	}

	public bool IsBombEnableMode
	{
		get
		{
			if (EventType != MFG.Types.EventType.Bomb)
			{
				return IsAotenjoMode;
			}
			return true;
		}
	}

	public bool IsAtomicEnableMode
	{
		get
		{
			if (EventType != MFG.Types.EventType.Atomic)
			{
				return IsAotenjoMode;
			}
			return true;
		}
	}

	public bool IsOpenReachEnableMode => IsReversalEnableMode;

	public bool IsAtomicReachEnableMode => IsAtomicEnableMode;

	public int IntScore()
	{
		if (IsAotenjoMode)
		{
			if (AotenjoScore >= MahjongUtility.Aotenjo.MaxScore)
			{
				return (int)MahjongUtility.Aotenjo.MaxScore;
			}
			if (AotenjoScore <= MahjongUtility.Aotenjo.MaxScore * -1)
			{
				return (int)MahjongUtility.Aotenjo.MaxScore * -1;
			}
		}
		return (int)Score;
	}

	public void ResetForContinueStart()
	{
		TaikyokuItems.Clear();
	}

	public void Apply(HaifuData haifuData)
	{
		TakuType = haifuData.TakuType;
		EventType = haifuData.EventType;
		IsConstancyTaku = false;
		IsPrivateMatch = false;
		IsCompetitionTaku = false;
	}

	public GAME_MODE GetGameMode()
	{
		return MahjongTypesUtility.GetGameMode(TakuType, EventType);
	}
}
