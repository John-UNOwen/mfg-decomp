using System.Collections.Generic;
using MFG.GameData.EventTaku;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EventTakuRankBonusDisp : MonoBehaviour
{
	[SerializeField]
	protected TextMeshProUGUI _ranktText;

	[SerializeField]
	protected TextMeshProUGUI _totalBonusPointText;

	[SerializeField]
	protected Image _rankImage;

	[SerializeField]
	protected List<Sprite> _rankSprites;

	public virtual void Reflesh(RankBonusInfo rankBonus)
	{
		if (_ranktText != null)
		{
			_ranktText.text = $"{(int)(rankBonus.Rank + 1)}";
		}
		if (_rankImage != null)
		{
			_rankImage.sprite = _rankSprites[(int)rankBonus.Rank];
		}
		if ((bool)_totalBonusPointText)
		{
			_totalBonusPointText.text = $"{rankBonus.TotalBonusPoint}";
		}
	}
}
