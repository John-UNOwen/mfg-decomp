using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.GameRequest;
using MFG.MasterGameData;
using MFG.Types;
using MFG.Types.Story;
using UnityEngine;

namespace MFG.GameData;

[Serializable]
public class StoryGameData
{
	private const int CurrentDataVersion = 1;

	public int DataVersion = 1;

	public const string ClientStateName = "story";

	public List<StoryInfo> Stories = new List<StoryInfo>();

	[NonSerialized]
	public StoryID m_playStoryScriptID;

	[NonSerialized]
	public bool m_forcePlaying;

	[NonSerialized]
	public bool m_forceEventStoryPlaying;

	[NonSerialized]
	public List<EventStoryDateInfo> EventStoryDateInfos = new List<EventStoryDateInfo>();

	[NonSerialized]
	public List<StoryID> AutoDisplayEventStories;

	[NonSerialized]
	public List<StoryInfo> BeforeStories;

	public StoryGameData()
	{
		Reset();
		m_playStoryScriptID = StoryID.None;
	}

	public async UniTask Save(CancellationToken cancellationToken = default(CancellationToken))
	{
		string text = JsonUtility.ToJson(this);
		Debug.Log("StoryGameData Save : " + text);
		await RequestApiBase<ClientStateWrite, ClientStateWrite.ResultData, (string, string)>.Send(("story", text), enableErrorScreenTransition: false, enableSendLog: true, cancellationToken);
	}

	public static StoryGameData LoadFromJson(string jsonString)
	{
		Debug.Log("StoryGameData Load : " + jsonString);
		return JsonUtility.FromJson<StoryGameData>(jsonString);
	}

	public void SetupForceStory()
	{
		BeforeStories = new List<StoryInfo>(Stories);
		GameDataManager.Story.SetupEventStoryInfos();
		GameDataManager.Story.CheckUnlock();
		GameDataManager.Story.SetupForceEventStoryInfo();
	}

	public void AddStory(StoryID storyID, bool isAvailable)
	{
		if (Stories.FindIndex((StoryInfo x) => x.StoryID == storyID) == -1)
		{
			Stories.Add(new StoryInfo
			{
				StoryID = storyID,
				IsAvailable = isAvailable
			});
			if (isAvailable)
			{
				Debug.Log(StoryMaster.GetStory(storyID).Name + "を表示&解禁");
			}
			else
			{
				Debug.Log(StoryMaster.GetStory(storyID).Name + "を表示（未解禁）");
			}
		}
	}

	public void AddStory(StoryID storyID)
	{
		AddStory(storyID, isAvailable: true);
	}

	public bool IsCharaStoryUnlockCheck(Story story)
	{
		try
		{
			StoryType storyType = story.StoryType;
			return (uint)(storyType - 1) > 1u || GameDataManager.IsCharaEnable(story.CharaType);
		}
		catch
		{
			return false;
		}
	}

	public void CheckUnlock()
	{
		foreach (Story story3 in StoryMaster.GetStories((Story story) => story.UnlockingType == UnlockingType.Default))
		{
			if (IsCharaStoryUnlockCheck(story3))
			{
				GameDataManager.Story.AddStory(story3.StoryID);
			}
		}
		foreach (Story story2 in StoryMaster.GetStories((Story story) => story.UnlockingType == UnlockingType.Event))
		{
			EventStoryDateInfo eventStoryDateInfo = EventStoryDateInfos.Find((EventStoryDateInfo x) => x.ID == story2.StoryID);
			if (eventStoryDateInfo != null && eventStoryDateInfo.IsStart)
			{
				GameDataManager.Story.AddStory(story2.StoryID);
			}
		}
		foreach (Story story4 in StoryMaster.GetStories((Story story) => story.Display))
		{
			if (IsCharaStoryUnlockCheck(story4))
			{
				GameDataManager.Story.AddStory(story4.StoryID, isAvailable: false);
			}
		}
		foreach (StoryInfo item in GameDataManager.Story.Stories.FindAll((StoryInfo story) => !story.IsAvailable))
		{
			if (!IsCharaStoryUnlockCheck(item.Master))
			{
				continue;
			}
			switch (item.Master.UnlockingType)
			{
			case UnlockingType.Debut:
				if (GameDataManager.Player.CharaData[(int)item.Master.CharaType].IsDebut && (item.Master.CharaType != CharaType.Chara10 || GameDataManager.IsShioriStoryEnable()))
				{
					item.IsAvailable = true;
				}
				break;
			case UnlockingType.Favo:
				if (GameDataManager.Player.CharaData[(int)item.Master.CharaType].Level >= item.Master.UnlockingCondition && (item.Master.CharaType != CharaType.Chara10 || GameDataManager.IsShioriStoryEnable()))
				{
					item.IsAvailable = true;
				}
				break;
			}
		}
	}

	public void SetupEventStoryInfos()
	{
		foreach (GameEventType value in Enum.GetValues(typeof(GameEventType)))
		{
			GameEventData gameEventData = GameDataManager.System.GameControlInfo.EventStatus.GetGameEventData(value);
			if (gameEventData != null)
			{
				string text = gameEventData.ParamValue("story_oid", "");
				StoryID storyID = StoryMaster.ToStoryID(text);
				if (!string.IsNullOrEmpty(text) && storyID != 0 && !EventStoryDateInfos.Exists((EventStoryDateInfo x) => x.ID == storyID))
				{
					Debug.Log($"SetupEventStoryInfos : {storyID}");
					EventStoryDateInfos.Add(new EventStoryDateInfo
					{
						ID = storyID,
						StartDate = gameEventData.ParamValue("story_begin", ""),
						EndDate = gameEventData.ParamValue("story_end", "")
					});
				}
			}
		}
	}

	public void SetupForceEventStoryInfo()
	{
		try
		{
			AutoDisplayEventStories = (from x in EventStoryDateInfos
				where x.IsActive
				orderby x.EndDateTime descending
				select x.ID into x
				where !Stories.Find((StoryInfo y) => y.Master.StoryID == x).IsRead
				orderby StoryMaster.GetStory(x).Priority descending
				select x).ToList();
		}
		catch (NullReferenceException)
		{
			AutoDisplayEventStories = new List<StoryID>();
		}
	}

	public bool JudgeAndSetEventStoryPlay()
	{
		if (AutoDisplayEventStories != null && AutoDisplayEventStories.Count > 0)
		{
			StoryID item = (m_playStoryScriptID = AutoDisplayEventStories.First());
			m_forcePlaying = true;
			m_forceEventStoryPlaying = true;
			AutoDisplayEventStories.Remove(item);
			return true;
		}
		return false;
	}

	public void Reset()
	{
		Stories = StoryInfo.CreateDefaultStories();
	}
}
