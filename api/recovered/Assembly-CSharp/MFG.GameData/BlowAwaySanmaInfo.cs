using System;
using MFG.Types;

namespace MFG.GameData;

[Serializable]
public class BlowAwaySanmaInfo
{
	public GameEventType EventType;

	public int ButtobiMedalNum;

	public bool IsFirstDisp = true;

	public bool IsEnable(bool takuTypeJudge = true)
	{
		if (!IsBlowAwaySanmaEventEnable())
		{
			return false;
		}
		if (GameDataManager.Account.IsGuestUser)
		{
			return false;
		}
		if (takuTypeJudge && (GameDataManager.Mahjong.TakuType != TakuType.Sanma || GameDataManager.Mahjong.EventType != MFG.Types.EventType.ToitoiButtobiGalaxy))
		{
			return false;
		}
		return true;
	}

	private bool IsBlowAwaySanmaEventEnable()
	{
		return GameDataManager.System.GameControlInfo.EventStatus.IsActive(EventType);
	}
}
