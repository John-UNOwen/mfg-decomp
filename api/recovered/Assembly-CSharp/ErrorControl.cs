using UnityEngine;

public class ErrorControl
{
	private static ErrorLogData lastErrorData = null;

	private static float time = float.MaxValue;

	private const float RETRY_TIME = 5f;

	public static void SetError(ERROR errorCode, uint errorOption, ERROR_LEVEL level = ERROR_LEVEL.ERROR)
	{
		if (errorCode != 0)
		{
			ErrorLogData log = new ErrorLogData(errorCode, errorOption, level);
			ErrorLogManager.GetInstance().Set(log);
			if (lastErrorData == null)
			{
				lastErrorData = log;
				time = Time.realtimeSinceStartup;
			}
			else if (lastErrorData.m_level < level)
			{
				lastErrorData = log;
				time = Time.realtimeSinceStartup;
			}
			else if (lastErrorData.m_code != errorCode)
			{
				lastErrorData = log;
				time = Time.realtimeSinceStartup;
			}
			else if (errorCode == ERROR.PRINTER && errorOption != lastErrorData.m_option)
			{
				lastErrorData = log;
				time = Time.realtimeSinceStartup;
			}
		}
	}

	public static bool IsNeedRetry(ERROR errorCode, uint errorOption)
	{
		if (errorCode != ERROR.CAMERA)
		{
			return errorCode == ERROR.IO;
		}
		return true;
	}

	public static bool IsEndRetryTime(ErrorLogData error)
	{
		if (IsNeedRetry(error.m_code, error.m_option))
		{
			return Time.realtimeSinceStartup - time > 5f;
		}
		return true;
	}

	public static void ResetError()
	{
		lastErrorData = null;
		time = float.MaxValue;
	}

	public static ErrorLogData GetLastError()
	{
		return lastErrorData;
	}

	public static bool ResumeLastError()
	{
		return false;
	}
}
