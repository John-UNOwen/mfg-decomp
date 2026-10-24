using System;
using System.Collections.Generic;
using System.Linq;
using MFG.Types;
using MFG.Types.CharaStamp;

namespace MFG.MasterGameData;

public class CharaStampMaster : MasterGameDataBase
{
	private static List<CharaStamp> _charaStamps = new List<CharaStamp>
	{
		new CharaStamp
		{
			CharaStampID = CharaStampID.Chaos,
			CharaType = CharaType.Chara11,
			StampEventType = GameEventType.ChaosStampEvent,
			CharaAppearanceEventType = GameEventType.ChaosAppearance,
			ClearStampNum = 20
		}
	};

	public static bool IsStampEventCharacter(CharaType chara)
	{
		return GetCharaStamps((CharaStamp x) => x.CharaType == chara) != null;
	}

	public static CharaStampID GetCurrentStampID()
	{
		for (int num = Enum.GetValues(typeof(CharaStampID)).Cast<int>().Max(); num > 0; num--)
		{
			if (IsEventEnable((CharaStampID)num))
			{
				return (CharaStampID)num;
			}
		}
		return CharaStampID.None;
	}

	public static bool IsFinishEvent(CharaStampID id)
	{
		if (id == CharaStampID.None)
		{
			return false;
		}
		if (!IsEventEnable(id))
		{
			return IsCharacterAppearance(id);
		}
		return false;
	}

	private static bool IsEventEnable(CharaStampID id)
	{
		if (GameDataManager.Account.IsGuestUser || id == CharaStampID.None)
		{
			return false;
		}
		CharaStamp charaStamp = GetCharaStamp(id);
		if (charaStamp != null)
		{
			return GameDataManager.System.GameControlInfo.EventStatus.IsActive(charaStamp.StampEventType);
		}
		return false;
	}

	public static bool IsCurrentEventEnable()
	{
		return IsEventEnable(GetCurrentStampID());
	}

	public static bool IsCharacterAppearance(CharaStampID id)
	{
		CharaStamp charaStamp = GetCharaStamp(id);
		if (charaStamp != null)
		{
			return GameDataManager.System.GameControlInfo.EventStatus.IsActive(charaStamp.CharaAppearanceEventType);
		}
		return false;
	}

	public static CharaStamp GetCharaStamp(CharaStampID charaStampID)
	{
		return _charaStamps.Find((CharaStamp x) => x.CharaStampID == charaStampID);
	}

	public static CharaStamp GetCharaStampFromCharaType(CharaType chara)
	{
		return _charaStamps.Find((CharaStamp x) => x.CharaType == chara);
	}

	public static List<CharaStamp> GetCharaStamps(Predicate<CharaStamp> match)
	{
		return _charaStamps.FindAll(match);
	}
}
