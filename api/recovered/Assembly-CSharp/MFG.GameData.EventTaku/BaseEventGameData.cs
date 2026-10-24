using System;
using System.Collections.Generic;
using MFG.MasterGameData;
using MFG.Taikyoku.Command.Receive;
using MFG.Types;
using MFG.Types.EventTakuMaster;

namespace MFG.GameData.EventTaku;

[Serializable]
public abstract class BaseEventGameData
{
	public GameEventType FlagType;

	public int TotalPoint;

	public bool IsFirstExplanationDisplayed;

	public abstract EventType EventType { get; }

	public abstract IResultInfo ResultInfo { get; }

	public Item Master => EventTakuMaster.GetItem(EventType);

	public virtual bool IsAvailable()
	{
		if (!GameDataManager.System.GameControlInfo.EventStatus.IsActive(FlagType))
		{
			return false;
		}
		if (FlagType == Master.CompetitionFlagType && GameDataManager.System.GameControlInfo.EventStatus.IsActive(Master.CompetitionFlagType))
		{
			return false;
		}
		if (GameDataManager.Account.IsGuestUser)
		{
			return false;
		}
		return true;
	}

	public bool IsConstancyTakuAvailable()
	{
		GameEventType constancyFlagType = Master.ConstancyFlagType;
		if (constancyFlagType == GameEventType.None)
		{
			return false;
		}
		if (IsAvailable())
		{
			return false;
		}
		return GameDataManager.System.GameControlInfo.EventStatus.IsActive(constancyFlagType);
	}

	public bool IsCompetitionTakuAvailable()
	{
		if (!GameDataManager.System.GameControlInfo.EventStatus.IsActive(Master.CompetitionFlagType))
		{
			return false;
		}
		if (GameDataManager.Competition.GetInProgressCompetitionInfo() == null)
		{
			return false;
		}
		if (GameDataManager.Account.IsGuestUser)
		{
			return false;
		}
		if (IsAvailable())
		{
			return false;
		}
		return true;
	}

	public virtual void ResetForContinueStart()
	{
	}

	public virtual void ResetForBeforeTaikyokuStart(List<Player> players)
	{
	}

	public abstract bool IsMatch(EventType eventType, TakuType takuType);

	public virtual bool IsActiveAtTaikyoku(EventType eventType, TakuType takuType, bool isConstancyTaku, bool isCompetitionTaku)
	{
		if (!IsMatch(eventType, takuType))
		{
			return false;
		}
		if (!isCompetitionTaku)
		{
			if (!isConstancyTaku)
			{
				return IsAvailable();
			}
			return IsConstancyTakuAvailable();
		}
		return IsCompetitionTakuAvailable();
	}

	public virtual void OnRyukyoku(RyuKyoku cmd, List<Player> players)
	{
	}

	public virtual void OnTsumoAgari(TsumoAgari cmd, List<Player> players)
	{
	}

	public virtual void OnRonAgari(Ron cmd, List<Player> players)
	{
	}

	public virtual void OnKyokuStart(KyokuStart cmd, List<Player> players)
	{
	}

	public abstract IResultInfo RefleshResultInfo(int beforeTotalAcquiredPoint, MahjongGameData mahjongGameData, bool isJandri);

	public virtual void ApplyResult(IResultInfo resultInfo)
	{
		TotalPoint += resultInfo.AcquiredPoint;
	}

	public virtual string GetEventDateText()
	{
		return (GameDataManager.System.GameControlInfo.EventStatus.GetGameEventData(FlagType)?.End ?? DateTimeOffset.MinValue).ToString("yyyy年MM月dd日（ddd）HH：mmまで");
	}

	public virtual void OnKyokuInit(List<Player> players)
	{
	}

	public virtual bool IsExtraBGMAvailable()
	{
		return false;
	}
}
