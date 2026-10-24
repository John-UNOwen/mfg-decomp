using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MFG.StoryPlay;

public class SelectWindow : Command
{
	private string choice1;

	private string choice2;

	private string choice3;

	private string choice4;

	private string choice5;

	private string[] gotoLabels = new string[5];

	private string special;

	public override async UniTask Parse(string[] values)
	{
		await base.Parse(values);
		type = CommandType.SelectWindow;
		choice1 = values[17];
		choice2 = values[18];
		choice3 = values[19];
		choice4 = values[20];
		choice5 = values[21];
		gotoLabels[0] = values[22];
		gotoLabels[1] = values[23];
		gotoLabels[2] = values[24];
		gotoLabels[3] = values[25];
		gotoLabels[4] = values[26];
		special = values[3];
		sec = Utility.ParseSec(values[6], 10f);
	}

	public override async UniTask Exec()
	{
		ResidentFlow.GetInstance().GetTimer().SetTimer(sec, disp: true);
		ResidentFlow.GetInstance().GetTimer().StartTimer();
		ButtonInteractiveControl();
		StoryPlayFlow.GetInstance().selectWindow.GetComponent<Animator>().SetTrigger("In");
		StoryPlayFlow.GetInstance().m_selectedIndex = -1;
		await UniTask.WhenAny(UniTask.WaitUntil(() => ResidentFlow.GetInstance().GetTimer().IsEnd(), PlayerLoopTiming.Update, token), UniTask.WaitWhile(() => StoryPlayFlow.GetInstance().m_selectedIndex < 0, PlayerLoopTiming.Update, token));
		ResidentFlow.GetInstance().GetTimer().ResetTimer(disp: false);
		StoryPlayFlow.GetInstance().selectWindow.GetComponent<Animator>().SetTrigger("Out");
		ForceDecideChoice();
	}

	private void ButtonInteractiveControl()
	{
		StoryPlayFlow.GetInstance().selectButton1.gameObject.SetActive(value: true);
		StoryPlayFlow.GetInstance().selectButton2.gameObject.SetActive(value: true);
		StoryPlayFlow.GetInstance().selectButton3.gameObject.SetActive(value: true);
		StoryPlayFlow.GetInstance().selectButton4.gameObject.SetActive(value: true);
		StoryPlayFlow.GetInstance().selectButton5.gameObject.SetActive(value: true);
		StoryPlayFlow.GetInstance().selectButton1.GetComponentInChildren<TextMeshProUGUI>().text = choice1;
		StoryPlayFlow.GetInstance().selectButton2.GetComponentInChildren<TextMeshProUGUI>().text = choice2;
		StoryPlayFlow.GetInstance().selectButton3.GetComponentInChildren<TextMeshProUGUI>().text = choice3;
		StoryPlayFlow.GetInstance().selectButton4.GetComponentInChildren<TextMeshProUGUI>().text = choice4;
		StoryPlayFlow.GetInstance().selectButton5.GetComponentInChildren<TextMeshProUGUI>().text = choice5;
		if (choice2 == "")
		{
			StoryPlayFlow.GetInstance().selectButton2.gameObject.SetActive(value: false);
		}
		if (choice3 == "")
		{
			StoryPlayFlow.GetInstance().selectButton3.gameObject.SetActive(value: false);
		}
		if (choice4 == "")
		{
			StoryPlayFlow.GetInstance().selectButton4.gameObject.SetActive(value: false);
		}
		if (choice5 == "")
		{
			StoryPlayFlow.GetInstance().selectButton5.gameObject.SetActive(value: false);
		}
	}

	public override void ForceDecideChoice()
	{
		ButtonInteractiveControl();
		if (StoryPlayFlow.GetInstance().m_selectedIndex < 0)
		{
			if (StoryPlayFlow.GetInstance().selectButton1.gameObject.activeSelf && StoryPlayFlow.GetInstance().selectButton1.GetComponent<Button>().IsInteractable())
			{
				StoryPlayFlow.GetInstance().m_selectedIndex = 0;
			}
			else if (StoryPlayFlow.GetInstance().selectButton2.gameObject.activeSelf && StoryPlayFlow.GetInstance().selectButton2.GetComponent<Button>().IsInteractable())
			{
				StoryPlayFlow.GetInstance().m_selectedIndex = 1;
			}
			else if (StoryPlayFlow.GetInstance().selectButton3.gameObject.activeSelf && StoryPlayFlow.GetInstance().selectButton3.GetComponent<Button>().IsInteractable())
			{
				StoryPlayFlow.GetInstance().m_selectedIndex = 2;
			}
			else if (StoryPlayFlow.GetInstance().selectButton4.gameObject.activeSelf && StoryPlayFlow.GetInstance().selectButton4.GetComponent<Button>().IsInteractable())
			{
				StoryPlayFlow.GetInstance().m_selectedIndex = 3;
			}
			else if (StoryPlayFlow.GetInstance().selectButton5.gameObject.activeSelf && StoryPlayFlow.GetInstance().selectButton5.GetComponent<Button>().IsInteractable())
			{
				StoryPlayFlow.GetInstance().m_selectedIndex = 4;
			}
			else
			{
				StoryPlayFlow.GetInstance().m_selectedIndex = 0;
			}
		}
		StoryPlayFlow.GetInstance().SetNextCommandLabel(gotoLabels[StoryPlayFlow.GetInstance().m_selectedIndex]);
	}
}
