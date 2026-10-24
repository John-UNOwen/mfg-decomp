using System;

namespace Konami.System.Net;

public class MaintainanceModeException : NetworkResponseException
{
	public MaintainanceModeException()
	{
	}

	public MaintainanceModeException(StatusCode code, string message)
		: base(code, message)
	{
	}

	public MaintainanceModeException(StatusCode code, string message, Exception inner)
		: base(code, message, inner)
	{
	}
}
