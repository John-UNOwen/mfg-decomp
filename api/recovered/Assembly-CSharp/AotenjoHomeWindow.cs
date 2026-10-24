using System.Numerics;
using MFG.GameData.EventTaku;
using MFG.Types;

public class AotenjoHomeWindow : BaseEventTakuHomeWindow
{
	protected override void Reflesh()
	{
		if (_totalPointText != null)
		{
			if (BigInteger.TryParse(((AotenjoEventGameData)GameDataManager.EventTaku.Current(EventType.Aotenjo)).BestScore, out var result))
			{
				_totalPointText.text = MahjongUtility.Aotenjo.GetScoreString(result, isYakuResult: false, "#000000") ?? "";
			}
			else
			{
				_totalPointText.text = "0";
			}
		}
		if (_dateText != null)
		{
			_dateText.text = _baseEventGameData.GetEventDateText();
		}
	}
}
