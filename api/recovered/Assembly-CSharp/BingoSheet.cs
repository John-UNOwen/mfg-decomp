using System.Collections.Generic;
using MFG.Types;
using UnityEngine;
using UnityEngine.UI;

public class BingoSheet : MonoBehaviour
{
	[SerializeField]
	private List<BingoHole> _holes = new List<BingoHole>();

	[SerializeField]
	private Image _sheet;

	[SerializeField]
	private Image _glow;

	[SerializeField]
	private Image _title;

	[SerializeField]
	private Sprite _nomalCard;

	[SerializeField]
	private Sprite _spCard;

	[SerializeField]
	private Sprite _nomalTitle;

	[SerializeField]
	private Sprite _spTitle;

	public void CloseAllYakuNaviWindow()
	{
		foreach (BingoHole hole in _holes)
		{
			hole.CloseYakuNaviWindow();
		}
	}

	public bool PlayClearHoleEffect(int holeNo, int score)
	{
		if (0 <= holeNo && holeNo < 9)
		{
			return _holes[holeNo].PlayClearHoleEffect(score);
		}
		return false;
	}

	public bool PlayClearLoopHoleEffect(int holeNo, int score)
	{
		if (0 <= holeNo && holeNo < 9)
		{
			return _holes[holeNo].PlayClearLoopHoleEffect(score);
		}
		return false;
	}

	public void CardSetUp(Yaku[] yakuList)
	{
		Color sheetColor = GetSheetColor(GameDataManager.BingoEventRemake.BingoCardNo % 9);
		_sheet.color = sheetColor;
		_glow.color = sheetColor;
		_sheet.sprite = (GameDataManager.BingoEventRemake.IsSpecialCard ? _spCard : _nomalCard);
		_title.sprite = (GameDataManager.BingoEventRemake.IsSpecialCard ? _spTitle : _nomalTitle);
		for (int i = 0; i < 9; i++)
		{
			_holes[i].YakuSet(yakuList[i]);
		}
	}

	public Color GetSheetColor(int sheetNo = 0)
	{
		if (ColorUtility.TryParseHtmlString(new List<string> { "#FFFFFF", "#39FFEA", "#FF39CF", "#39A8FF", "#1CA914", "#FFED00", "#FFFFFF", "#FF9D00", "#FF000A" }[sheetNo], out var color))
		{
			return color;
		}
		return Color.white;
	}
}
