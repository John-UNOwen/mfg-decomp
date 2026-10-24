using Konami.System.Net;

public static class EnumExtend
{
	public static bool IsSuccess(this StatusCode code)
	{
		return code == StatusCode.OK;
	}

	public static bool IsCancelled(this StatusCode code)
	{
		return code == StatusCode.UserAbort;
	}

	public static bool IsServerError(this StatusCode code)
	{
		return code == StatusCode.SystemError;
	}

	public static bool IsNotFound(this StatusCode code)
	{
		return code == StatusCode.DataError;
	}
}
