using System;

namespace Konami.System.Net;

public class NoSupportedVersionException : NetworkResponseException
{
	public NoSupportedVersionException()
	{
	}

	public NoSupportedVersionException(StatusCode code, string message)
		: base(code, message)
	{
	}

	public NoSupportedVersionException(StatusCode code, string message, Exception inner)
		: base(code, message, inner)
	{
	}
}
