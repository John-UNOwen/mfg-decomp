using System;
using Cysharp.Threading.Tasks;
using MFG.GameRequest;

[Serializable]
public class MissionControlInfo
{
	public DateTimeOffset LastModifiedTime;

	public MissionStatus MissionStatus = new MissionStatus();

	public void Apply(MissionDate.ResultData resultData)
	{
		LastModifiedTime = DateTimeOffsetManager.GetJSTTime();
		if (resultData.MissionScheduleParamInfo != null)
		{
			MissionStatus = resultData.MissionScheduleParamInfo;
		}
	}

	public async UniTask Modify(bool isForceModify = false, bool suppressException = true, bool isSendLog = true)
	{
		try
		{
			Apply(await RequestApiBase<MissionDate, MissionDate.ResultData>.Send(enableErrorScreenTransition: false, isSendLog));
		}
		catch
		{
			if (!suppressException)
			{
				throw;
			}
		}
	}
}
