using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class JanponObject
{
	public enum JanponKind
	{
		Sticker,
		Music,
		Others
	}

	public JanponKind Kind;

	public Button BtNormalPlay;

	public Button BtSuperPlay;

	public TextMeshProUGUI NumeratorCounter;

	public TextMeshProUGUI DenominatorCounter;

	public GameObject CompleteStamp;

	public GameObject NormalConsumeItem;

	public GameObject SuperConsumeItem;

	public GameObject NormalJanponIcon;

	public GameObject SuperJanponIcon;

	public void Refresh(int numerator, int denominator, bool isFirstDisp)
	{
		bool flag = numerator == denominator;
		NumeratorCounter.text = $"{numerator}";
		DenominatorCounter.text = $"{denominator}";
		CompleteStamp.SetActive(flag);
		BtNormalPlay.interactable = !isFirstDisp && !flag && GameDataManager.ScrambleFestival.CurrentInfo.IsLotteryEnable(isWithoutDuplicates: false);
		BtSuperPlay.interactable = !isFirstDisp && !flag && GameDataManager.ScrambleFestival.CurrentInfo.IsLotteryEnable(isWithoutDuplicates: true);
	}

	public void ModeChange(bool isToSuper)
	{
		BtNormalPlay.gameObject.SetActive(!isToSuper);
		BtSuperPlay.gameObject.SetActive(isToSuper);
		NormalConsumeItem.SetActive(!isToSuper);
		SuperConsumeItem.SetActive(isToSuper);
		NormalJanponIcon.SetActive(!isToSuper);
		SuperJanponIcon.SetActive(isToSuper);
	}
}
