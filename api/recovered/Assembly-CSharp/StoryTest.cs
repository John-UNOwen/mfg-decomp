using System;
using System.Collections.Generic;
using System.Linq;
using MFG.GameData;
using MFG.MasterGameData;
using MFG.Types.Story;
using UnityEngine;
using UnityEngine.UI;

public class StoryTest : MonoBehaviour
{
	[SerializeField]
	private GameObject storyTestDispPrefab;

	private List<StoryTestDisp> storyTestDispScripts = new List<StoryTestDisp>(Enum.GetNames(typeof(StoryID)).Length);

	[SerializeField]
	private Button resetButton;

	[SerializeField]
	private Button allOnButton;

	[SerializeField]
	private Button allOffButton;

	[SerializeField]
	private ScrollView scrollView;

	private void OnDestroy()
	{
	}

	public void CreateDisp()
	{
		foreach (StoryInfo item in GameDataManager.Story.Stories.OrderBy((StoryInfo a) => a.StoryID))
		{
			GameObject gameObject = scrollView.Add(storyTestDispPrefab);
			storyTestDispScripts.Add(gameObject.GetComponent<StoryTestDisp>());
			storyTestDispScripts.Last().SetStoryID(item.StoryID, item.IsRead);
		}
		resetButton.onClick.AddListener(delegate
		{
			ResetPossessing();
		});
		allOnButton.onClick.AddListener(delegate
		{
			ChangeAllPossessing(possessing: true);
		});
		allOffButton.onClick.AddListener(delegate
		{
			ChangeAllPossessing(possessing: false);
		});
	}

	private void ResetPossessing()
	{
		foreach (StoryTestDisp storyTestDispScript in storyTestDispScripts)
		{
			if (StoryMaster.GetDefaultStoryIDs().Contains(storyTestDispScript.storyID))
			{
				storyTestDispScript.SetPossessing(pos: true);
			}
			else
			{
				storyTestDispScript.SetPossessing(pos: false);
			}
		}
	}

	private void ChangeAllPossessing(bool possessing)
	{
		foreach (StoryTestDisp storyTestDispScript in storyTestDispScripts)
		{
			storyTestDispScript.SetPossessing(possessing);
		}
	}
}
