using System;
using MFG.MasterGameData;
using MFG.Types.CharaStamp;

namespace MFG.GameData;

[Serializable]
public class CharaStampInfo
{
	public CharaStampID ID;

	public bool IsUsable;

	public bool IsHomeDispFinish;

	public bool IsHomeFirstDispFinish;

	public int StampNum;

	public CharaStamp Master => CharaStampMaster.GetCharaStamp(ID);

	public string GetEventDate()
	{
		DateTimeOffset dateTimeOffset = GameDataManager.System.GameControlInfo.EventStatus.GetGameEventData(Master.StampEventType)?.Begin ?? DateTimeOffset.MinValue;
		if (!(dateTimeOffset != DateTimeOffset.MinValue))
		{
			return "";
		}
		return dateTimeOffset.ToString("yyyy年MM月dd日（ddd）HH：mm ～");
	}

	public int Progress(bool isJandri = false)
	{
		int result = 0;
		if (!IsUsable)
		{
			StampNum++;
			result = 1;
			if (isJandri && StampNum < Master.ClearStampNum)
			{
				StampNum++;
				result = 2;
			}
			if (StampNum >= Master.ClearStampNum)
			{
				IsUsable = true;
			}
		}
		return result;
	}

	public bool IsDuringForceDisplay()
	{
		if (Master.StampEventType == GameEventType.ChaosStampEvent && GameDataManager.IsChaosForceActive())
		{
			return false;
		}
		GameEventData gameEventData = GameDataManager.System.GameControlInfo.EventStatus.GetGameEventData(Master.StampEventType);
		if (gameEventData != null)
		{
			string text = gameEventData.ParamValue("force_disp_end_date", "");
			if (text == "")
			{
				return true;
			}
			return DateTimeOffsetManager.GetJSTFromString(text) > DateTimeOffsetManager.GetJSTTime();
		}
		return false;
	}

	public void ForceActive()
	{
		IsUsable = true;
		IsHomeDispFinish = true;
		StampNum = 20;
	}
}
