using System;
using System.Collections.Generic;
using MFG.MasterGameData;
using MFG.Types.Story;
using UnityEngine;

namespace MFG.GameData;

[Serializable]
public class StoryInfo : ISerializationCallbackReceiver
{
	public StoryID StoryID;

	public bool IsAvailable = true;

	public bool IsRead;

	public Story Master => StoryMaster.GetStory(StoryID);

	public void OnBeforeSerialize()
	{
	}

	public void OnAfterDeserialize()
	{
	}

	public static List<StoryInfo> CreateDefaultStories()
	{
		List<StoryInfo> list = new List<StoryInfo>();
		foreach (StoryID defaultStoryID in StoryMaster.GetDefaultStoryIDs())
		{
			list.Add(new StoryInfo
			{
				StoryID = defaultStoryID
			});
		}
		return list;
	}

	public bool IsAvailableCharaStory()
	{
		if (Master.StoryType == StoryType.EventStory)
		{
			return true;
		}
		return GameDataManager.IsCharaSelectable(Master.CharaType);
	}
}
