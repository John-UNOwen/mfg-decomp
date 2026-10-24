using System;
using System.Collections.Generic;
using System.Linq;
using MFG.MasterGameData;
using MFG.Types;
using MFG.Types.Omikuji;

namespace MFG.GameData;

[Serializable]
public class OmikujiEventInfo
{
	public GameEventType FlagType;

	public bool IsFirstDisp = true;

	public string TimeStamp = "";

	public string NoticeTimeStamp = "";

	public List<CharaType> DoneCharaTypes = new List<CharaType>();

	public CharaType TodayCharaTypes = (CharaType)(-1);

	public bool IsEnable => GameDataManager.System.GameControlInfo.EventStatus.IsActive(FlagType);

	public DateTimeOffset BeginTime => GameDataManager.System.GameControlInfo.EventStatus.GetGameEventData(FlagType)?.Begin ?? DateTimeOffset.MaxValue;

	public string TodayCharaImagePath => Master.ItemImagePath(TodayCharaTypes);

	public Item Master => OmikujiMaster.GetItem(FlagType);

	public List<CharaType> GetLotteryCharaTypes()
	{
		return (from CharaType x in Enum.GetValues(typeof(CharaType))
			where x <= Master.MaxCharaType && !DoneCharaTypes.Contains(x)
			select x).ToList();
	}

	public void OmikujiDone(CharaType chara)
	{
		if (!GameUtility.IsKamunityBootEnable() || BeginTime != DateTimeOffset.MaxValue)
		{
			TimeStamp = BeginTime.ToString();
			if (!DoneCharaTypes.Contains(chara))
			{
				DoneCharaTypes.Add(chara);
			}
		}
	}

	public bool IsOmikujiPlayEnable()
	{
		if (!IsEnable || (GameUtility.IsKamunityBootEnable() && BeginTime == DateTimeOffset.MaxValue))
		{
			return false;
		}
		if (string.IsNullOrEmpty(TimeStamp))
		{
			return true;
		}
		if (TimeStamp.Equals(BeginTime.ToString()))
		{
			return false;
		}
		return true;
	}

	public void NoticeDone()
	{
		if (!GameUtility.IsKamunityBootEnable() || BeginTime != DateTimeOffset.MaxValue)
		{
			NoticeTimeStamp = BeginTime.ToString();
		}
	}

	public bool IsOmikujiNoticePlay()
	{
		if (!IsEnable || (GameUtility.IsKamunityBootEnable() && BeginTime == DateTimeOffset.MaxValue))
		{
			return false;
		}
		if (string.IsNullOrEmpty(NoticeTimeStamp))
		{
			return true;
		}
		if (NoticeTimeStamp.Equals(BeginTime.ToString()))
		{
			return false;
		}
		return true;
	}

	public OmikujiEventInfo(GameEventType eventType)
	{
		FlagType = eventType;
	}

	public void ResetTimeStamp()
	{
	}

	public void ResetDoneChara()
	{
	}
}
