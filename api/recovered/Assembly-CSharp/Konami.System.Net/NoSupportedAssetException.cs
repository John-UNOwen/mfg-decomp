using System;

namespace Konami.System.Net;

public class NoSupportedAssetException : NetworkResponseException
{
	public NoSupportedAssetException()
	{
	}

	public NoSupportedAssetException(StatusCode code, string message)
		: base(code, message)
	{
	}

	public NoSupportedAssetException(StatusCode code, string message, Exception inner)
		: base(code, message, inner)
	{
	}
}
