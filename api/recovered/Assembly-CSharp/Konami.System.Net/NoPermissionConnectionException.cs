using System;

namespace Konami.System.Net;

public class NoPermissionConnectionException : NetworkResponseException
{
	public NoPermissionConnectionException()
	{
	}

	public NoPermissionConnectionException(StatusCode code, string message)
		: base(code, message)
	{
	}

	public NoPermissionConnectionException(StatusCode code, string message, Exception inner)
		: base(code, message, inner)
	{
	}
}
