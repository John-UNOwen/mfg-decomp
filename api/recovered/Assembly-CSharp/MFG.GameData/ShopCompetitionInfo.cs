using System;
using MFG.Types;
using MFG.Types.Customize.CustomizeItem;
using MFG.Types.ShopCompetition;
using UnityEngine;

namespace MFG.GameData;

[Serializable]
public class ShopCompetitionInfo : ISerializationCallbackReceiver
{
	public CompetitionID CompetitionID;

	public string Name;

	public string ShopName;

	public DateTimeOffset Start;

	public DateTimeOffset End;

	[SerializeField]
	[HideInInspector]
	private string StartDateTime;

	[SerializeField]
	[HideInInspector]
	private string EndDateTime;

	public Status CompetitionStatus;

	public bool IsInTime;

	public int TaikyokuScore;

	public int PersonalScore;

	public int ShopScore;

	public int PersonalRank;

	public int ShopRank;

	[SerializeField]
	[HideInInspector]
	private int _trophyType;

	public CharaType TrophyCharaType;

	private const int CurrentDataVersion = 1;

	public int DataVersion = 1;

	public bool IsEnable => CompetitionStatus == Status.InProgress;

	public bool IsTrophyDisplay => CompetitionStatus == Status.TrophyDisp;

	public TrophyType TrophyType => (TrophyType)_trophyType;

	public bool IsValid(TakuType takuType, MFG.Types.EventType eventType, bool isConsiderIsInTime = true)
	{
		if (isConsiderIsInTime && !IsInTime)
		{
			return false;
		}
		if (GameDataManager.Account.IsGuestUser)
		{
			return false;
		}
		if (GameDataManager.Player.DanLevel < DanLevelType.KYU9)
		{
			return false;
		}
		if (takuType != TakuType.Nima)
		{
			return eventType != MFG.Types.EventType.Aotenjo;
		}
		return false;
	}

	public void Invalidate()
	{
		CompetitionStatus = Status.None;
		TaikyokuScore = 0;
		PersonalScore = 0;
		ShopScore = 0;
		PersonalRank = 0;
		ShopRank = 0;
	}

	public ShopCompetitionInfo Clone()
	{
		return (ShopCompetitionInfo)MemberwiseClone();
	}

	public void Log_Scores()
	{
		Debug.Log($"TaikyokuScore {TaikyokuScore} / PersonalScore {PersonalScore} / ShopScore {ShopScore} / PersonalRank {PersonalRank} / ShopRank {ShopRank}");
	}

	public void OnBeforeSerialize()
	{
	}

	public void OnAfterDeserialize()
	{
		if (string.IsNullOrEmpty(StartDateTime))
		{
			Start = DateTimeOffset.MinValue;
		}
		else
		{
			Start = DateTimeOffsetManager.GetJSTFromString(StartDateTime);
			if (Start == default(DateTimeOffset))
			{
				Start = DateTimeOffset.MinValue;
			}
		}
		if (string.IsNullOrEmpty(EndDateTime))
		{
			End = DateTimeOffset.MaxValue;
			return;
		}
		End = DateTimeOffsetManager.GetJSTFromString(EndDateTime);
		if (End == default(DateTimeOffset))
		{
			End = DateTimeOffset.MaxValue;
		}
	}

	public (ItemID, string) GetTrophyInfo()
	{
		TrophyType trophyType = TrophyType;
		ItemID itemID = (ItemID)(1094 + (int)TrophyCharaType * 4 + (trophyType - 1));
		return (itemID, $"CustomizeItem/{itemID}.prefab");
	}
}
