using System;
using System.Linq;
using MFG.MasterGameData;
using MFG.Types.Story;

namespace MFG.GameData;

public class EventStoryDateInfo
{
	public StoryID ID;

	public string StartDate;

	public string EndDate;

	public DateTimeOffset StartDateTime => DateTimeOffsetManager.GetJSTFromString(StartDate);

	public DateTimeOffset EndDateTime => DateTimeOffsetManager.GetJSTFromString(EndDate);

	public bool IsStart
	{
		get
		{
			if (IsStartChapter())
			{
				return !string.IsNullOrEmpty(StartDate);
			}
			return false;
		}
	}

	public bool IsEnd
	{
		get
		{
			if (DateTimeOffsetManager.GetJSTTime() < EndDateTime)
			{
				return !string.IsNullOrEmpty(EndDate);
			}
			return false;
		}
	}

	public bool IsActive
	{
		get
		{
			if (IsStart)
			{
				return IsEnd;
			}
			return false;
		}
	}

	private bool IsStartChapter()
	{
		if (StartDateTime > DateTimeOffsetManager.GetJSTTime())
		{
			return false;
		}
		if (!IsEnd)
		{
			return true;
		}
		int chapter = StoryMaster.GetStory(ID).Chapter;
		if (chapter == 1)
		{
			return true;
		}
		int part = StoryMaster.GetStory(ID).Part;
		Story beforeStory = StoryMaster.GetStories((Story x) => x.Part == part && x.Chapter == chapter - 1)?.First();
		if (beforeStory != null && GameDataManager.Story.BeforeStories.Find((StoryInfo x) => x.StoryID == beforeStory.StoryID) != null)
		{
			return true;
		}
		return false;
	}
}
