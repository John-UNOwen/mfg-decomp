using System;

namespace Konami.System.Net;

public class NetworkResponseException : Exception
{
	public StatusCode Code { get; private set; }

	public NetworkResponseException()
	{
	}

	public NetworkResponseException(StatusCode code, string message)
		: base(message)
	{
		Code = code;
	}

	public NetworkResponseException(StatusCode code, string message, Exception inner)
		: base(message, inner)
	{
		Code = code;
	}
}
