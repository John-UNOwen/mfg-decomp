using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MeldBonusTakuBonusCounter : MonoBehaviour
{
	[SerializeField]
	private Image _frameBase;

	[SerializeField]
	private Image _frameGrow;

	[SerializeField]
	private Image _numberImage;

	[SerializeField]
	private List<Sprite> _numbers;

	[SerializeField]
	private List<Color> _baseColors;

	[SerializeField]
	private List<Color> _growColors;

	private void Start()
	{
	}

	public void Reflesh(List<Player> players, bool isDisp = true)
	{
		bool flag = isDisp && MahjongUtility.MeldBonus.IsEffectiveTaiyoku();
		base.gameObject.SetActive(flag);
		if (flag)
		{
			int num = MahjongUtility.MeldBonus.MeldCount(players);
			_numberImage.sprite = _numbers[num];
			_frameBase.color = _baseColors[(num > 0) ? ((num < 4) ? 1 : 2) : 0];
			_frameGrow.color = _growColors[(num > 0) ? ((num < 4) ? 1 : 2) : 0];
		}
	}

	public void ForceDisp()
	{
	}
}
