using System;
using System.Collections.Generic;
using System.Linq;
using MFG.MasterGameData;
using MFG.Types.Mission;
using UnityEngine;

namespace MFG.GameData;

[Serializable]
public class MissionProgressInfo : ISerializationCallbackReceiver
{
	[NonSerialized]
	public MissionID MissionID;

	[SerializeField]
	[HideInInspector]
	private string _objectID = "";

	public int ClearCount;

	public int Progress;

	public string MissionVersion = "";

	public string LimitDate = "";

	public bool IsDisplay = true;

	public Item Master => MissionMaster.GetItem(MissionID);

	public bool IsAllClear
	{
		get
		{
			if (Master.LoopType == LoopType.Endless || ClearCount < Master.AchievementList().Count())
			{
				List<MissionID> autoClearMissionIDs = Master.AutoClearMissionIDs;
				if (autoClearMissionIDs != null && autoClearMissionIDs.Count > 0)
				{
					return Master.AutoClearMissionIDs.All((MissionID x) => GameDataManager.Mission.m_missionProgressInfos.Find((MissionProgressInfo y) => y.MissionID == x)?.IsAllClear ?? false);
				}
				return false;
			}
			return true;
		}
	}

	public void OnBeforeSerialize()
	{
		_objectID = MissionMaster.ToObjectID(MissionID);
	}

	public void OnAfterDeserialize()
	{
		MissionID = MissionMaster.ToMissionID(_objectID);
	}

	public MissionProgressInfo(MissionID missionID)
	{
		MissionID = missionID;
		_objectID = MissionMaster.ToObjectID(MissionID);
	}

	public bool IsClear(int achievementIndex)
	{
		if (Master.LoopType != LoopType.Endless && achievementIndex > Master.AchievementList().Count())
		{
			return false;
		}
		return ClearCount > achievementIndex;
	}

	public void Reset()
	{
		ClearCount = 0;
		Progress = 0;
		Debug.Log($"Mission Reset : {MissionID}");
	}

	public void Convert(int progress)
	{
		Progress = ((Master.LoopType != LoopType.Endless && Master.AchievementList().Last() <= progress) ? Master.AchievementList().Last() : progress);
		ClearCount = Master.GetAchievementIndex(Progress);
	}
}
