using System.Net;
using System.Threading;
using UnityEngine;

namespace Konami.System.Net;

public class RequestParameter
{
	public string CommandName = "";

	public string RequestId = "";

	public long RetryInfo;

	public string Url = "";

	public string PostString;

	public RequestType RequestType = RequestType.ApiXml;

	public ResponseTypes ResponseType = ResponseTypes.TextXml;

	public MethodTypes MethodType = MethodTypes.POST;

	public bool UseRandomKey;

	public RequestDelegate UserDelegate;

	public RequestDelegate CloseDelegate;

	public RequestDelegate FaildDelegate;

	public RequestDelegate RetryDelegate;

	public bool IsDone;

	public bool IsSystemFailed;

	public bool IsSystemTimeout;

	public bool ManualRetry;

	public bool EnableRetry;

	public byte[] ResponseByte;

	public string ResponseText = "";

	public string Error = "";

	public string UserError = "";

	public float StartTime;

	public float EndTime;

	public StatusCode StatusCode = StatusCode.OK;

	public WebException RequestWebException;

	public CancellationToken CancellationToken;

	public void Begin(bool _displog)
	{
		StartTime = Time.realtimeSinceStartup;
	}

	public void Done()
	{
		IsDone = true;
		EndTime = Time.realtimeSinceStartup;
		if (IsSystemFailed)
		{
			if (FaildDelegate != null && !FaildDelegate.Target.Equals(null))
			{
				FaildDelegate(this);
			}
		}
		else if (UserDelegate != null && UserDelegate.Target != null && !UserDelegate.Target.Equals(null))
		{
			UserDelegate(this);
		}
	}
}
