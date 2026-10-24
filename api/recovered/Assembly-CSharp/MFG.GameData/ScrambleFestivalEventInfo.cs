using System;
using MFG.MasterGameData;
using MFG.Types.PlayerItem;
using MFG.Types.ScrambleFestival;
using UnityEngine;

namespace MFG.GameData;

[Serializable]
public class ScrambleFestivalEventInfo
{
	public GameEventType FlagType;

	public bool IsFirstDisp = true;

	public string NoticeTimeStamp = "";

	public bool IsResultDisp = true;

	public TeamType Team;

	public int Score;

	public int Medal;

	public bool IsEnable
	{
		get
		{
			if (!GameDataManager.Account.IsGuestUser)
			{
				return GameDataManager.System.GameControlInfo.EventStatus.IsActive(FlagType);
			}
			return false;
		}
	}

	public bool IsResultEnable
	{
		get
		{
			if (!Master.IsOnlyJanpon && !GameDataManager.Account.IsGuestUser)
			{
				return GameDataManager.System.GameControlInfo.EventStatus.IsActive(Master.FinishFlagType);
			}
			return false;
		}
	}

	public bool IsCatchphraseChangeUnable
	{
		get
		{
			if (!Master.IsOnlyJanpon && IsEnable)
			{
				return Team != TeamType.None;
			}
			return false;
		}
	}

	public MFG.Types.ScrambleFestival.Item Master => ScrambleFestivalMaster.GetItem(FlagType);

	public void AddMedal(int addCount)
	{
		Medal += addCount;
	}

	public void ConsumeMedal(bool isWithoutDuplicates)
	{
		if (IsLotteryEnable(isWithoutDuplicates))
		{
			Medal -= (isWithoutDuplicates ? Master.SpiritLotteryConsume.Medal : Master.LotteryConsumeMedal);
		}
	}

	public bool ExchangeFightSpirit(int count)
	{
		if (Master.SpiritExchangeMedal * count <= Medal)
		{
			GameDataManager.PlayerItem.Add(ItemID.FightSpiritW, count);
			Medal -= Master.SpiritExchangeMedal * count;
			return true;
		}
		return false;
	}

	public bool IsExchangeEnable()
	{
		return Medal >= Master.SpiritExchangeMedal;
	}

	public bool IsLotteryEnable(bool isWithoutDuplicates)
	{
		Debug.Log($"メダル {Medal}");
		return Medal >= (isWithoutDuplicates ? Master.SpiritLotteryConsume.Medal : Master.LotteryConsumeMedal);
	}

	public ScrambleFestivalEventInfo(GameEventType eventType)
	{
		FlagType = eventType;
	}
}
