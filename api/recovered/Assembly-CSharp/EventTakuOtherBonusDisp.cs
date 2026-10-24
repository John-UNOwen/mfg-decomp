using MFG.GameData.EventTaku;
using TMPro;
using UnityEngine;

public class EventTakuOtherBonusDisp : MonoBehaviour
{
	[SerializeField]
	protected string _bonusTypeKey;

	[SerializeField]
	protected TextMeshProUGUI _bonusCountText;

	[SerializeField]
	protected TextMeshProUGUI _acquiredBonusPointText;

	public virtual void Reflesh(OtherBonusInfos otherBonuses)
	{
		if (otherBonuses.ContainsKey(_bonusTypeKey))
		{
			Reflesh(otherBonuses.BonusInfo(_bonusTypeKey));
		}
	}

	public virtual void Reflesh(BonusInfo bonusInfo)
	{
		_bonusCountText.text = $"{bonusInfo.BonusCount}";
		_acquiredBonusPointText.text = $"{bonusInfo.TotalBonusPoint}";
	}
}
