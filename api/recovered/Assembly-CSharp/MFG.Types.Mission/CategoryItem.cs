using System;

namespace MFG.Types.Mission;

public class CategoryItem
{
	public Category Category;

	public string Name;

	public GameEventType MissionEventType;

	public bool IsBingo;

	public bool IsCharaMainMission;

	public MissionID MainMissionID;

	public string EventItemOID = "";

	public BGType MapBGType;

	public bool IsJandriEnable = true;

	public bool IsKonastaEnable = true;

	public bool IsDispPlayCountWindow = true;

	public bool IsJudgeCategory;

	public bool IsWelcomeComebackEvent;

	public bool IsWelcomeComebackUser => (GameDataManager.ReceiveOnly.PlayerInfo.ComebackCampaignInfos?.Find((ComebackCampaignInfo x) => x.EventType == MissionEventType)?.IsEnable).GetValueOrDefault();

	public MissionData ScheduleData => GameDataManager.MissionSchedule?.ControlInfo?.MissionStatus?.GetMissionData(Category);

	public DateTimeOffset BeginDateTime => ScheduleData?.Begin ?? DateTimeOffset.MinValue;

	public DateTimeOffset EndDateTime => ScheduleData?.End ?? DateTimeOffset.MaxValue;

	public bool IsEnable()
	{
		if (GameDataManager.MissionSchedule.ControlInfo.MissionStatus.IsActive(Category) && (MissionEventType == GameEventType.None || (GameDataManager.System.GameControlInfo.EventStatus.IsActive(MissionEventType) && (!IsWelcomeComebackEvent || IsWelcomeComebackUser))))
		{
			if (GameUtility.IsKonastaOrNetcafeBehavior)
			{
				return IsKonastaEnable;
			}
			return true;
		}
		return false;
	}

	public string ItemExpireDate()
	{
		if (DebugFlag.IsActive(DebugFlag.Flag.MissionExpireItemForceEnable))
		{
			return DateTimeOffsetManager.GetJSTTime().AddDays(30.0).ToString();
		}
		return ScheduleData?.ParamValue("item_expire", "") ?? "";
	}
}
