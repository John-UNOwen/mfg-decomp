using System;
using Cysharp.Threading.Tasks;
using MFG.GameRequest;

[Serializable]
public class GameControlInfo
{
	public DateTimeOffset LastModifiedTime;

	public DateTimeOffset ExpirationTime;

	public ProStats ProStats = new ProStats();

	public GameEventStatus EventStatus = new GameEventStatus();

	public bool IsNeedModify(bool isForceModify = false)
	{
		if (isForceModify)
		{
			return true;
		}
		if (ExpirationTime == default(DateTimeOffset))
		{
			return true;
		}
		return DateTimeOffsetManager.GetJSTTime() > ExpirationTime;
	}

	public void Apply(AppliInfo.ResultData resultData)
	{
		LastModifiedTime = DateTimeOffsetManager.GetJSTTime();
		ExpirationTime = LastModifiedTime + new TimeSpan(0, 0, 0, (int)resultData.ExpirationTimeSeconds);
		if (resultData.ProStats != null)
		{
			ProStats = resultData.ProStats;
			ResidentFlow.GetInstance().GetMahjongActorNoticeManager().Apply(ProStats);
		}
		if (resultData.GameControlParamInfo != null)
		{
			EventStatus = resultData.GameControlParamInfo;
		}
	}

	public async UniTask Modify(bool isForceModify = false, bool suppressException = true, bool isSendLog = true)
	{
		_ = 1;
		try
		{
			if (IsNeedModify(isForceModify))
			{
				if (!TitleDemoUtility.IsServiceUrlModified)
				{
					await TitleDemoUtility.ModifyServiceUrl();
				}
				if (TitleDemoUtility.IsServiceUrlModified)
				{
					Apply(await RequestApiBase<AppliInfo, AppliInfo.ResultData>.Send(enableErrorScreenTransition: false, isSendLog));
				}
			}
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
