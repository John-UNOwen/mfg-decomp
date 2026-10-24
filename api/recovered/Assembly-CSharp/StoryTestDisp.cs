using MFG.GameData;
using MFG.MasterGameData;
using MFG.Types.Story;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StoryTestDisp : MonoBehaviour
{
	public StoryID storyID;

	public bool possessing;

	[SerializeField]
	private Button itemOnOffButton;

	private void Start()
	{
		itemOnOffButton.onClick.AddListener(delegate
		{
			ChangePossession();
		});
	}

	private void Update()
	{
		itemOnOffButton.transform.Find("Text").gameObject.GetComponent<TextMeshProUGUI>().text = StoryMaster.GetStory(storyID).DispName + "：" + (possessing ? "既読" : "未読");
	}

	private void ChangePossession()
	{
		possessing = !possessing;
		SetButtonColor();
		GameDataManager.Story.Stories.Find((StoryInfo a) => a.StoryID == storyID).IsRead = possessing;
	}

	public async void SetStoryID(StoryID id, bool isRead)
	{
		storyID = id;
		SetPossessing(isRead);
	}

	public void SetPossessing(bool pos)
	{
		possessing = pos;
		SetButtonColor();
		GameDataManager.Story.Stories.Find((StoryInfo a) => a.StoryID == storyID).IsRead = possessing;
	}

	private void SetButtonColor()
	{
		itemOnOffButton.GetComponent<Image>().color = (possessing ? new Color(1f, 1f, 1f, 1f) : new Color(0.7f, 0.7f, 0.7f, 1f));
	}
}
