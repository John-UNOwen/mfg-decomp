using System;
using System.Collections;
using System.Net;
using System.Text;
using System.Threading;
using KAMUNITY;
using UnityEngine;

namespace Konami.System.Net;

public class RequestCoroutine
{
	private string m_url = "";

	private string m_request;

	private RequestCoroutineDelegate m_userDelegate;

	public CancellationToken CancellationToken;

	public NetEnvironmentConnection EnvConnection { get; set; } = NetEnvironmentConnection.Default;


	public NetEnvironmentProxy EnvProxy { get; set; } = NetEnvironmentProxy.Default;


	public float Progress { get; private set; }

	public MethodTypes MethodType { get; set; } = MethodTypes.POST;


	public bool IsTimeout { get; private set; }

	public bool IsFailed { get; private set; }

	public bool IsDone { get; private set; }

	public string RequestContentType { get; set; } = "application/x-www-form-urlencoded";


	public ResponseTypes ResponceType { get; set; }

	public bool UseRandomKey { get; set; }

	public object UserData { get; }

	public string ResponseText { get; private set; } = "";


	public byte[] ResponseByte { get; private set; }

	public string Error { get; private set; } = "";


	public string UserError { get; private set; } = "";


	public StatusCode StatusCode { get; private set; } = StatusCode.OK;


	public WebException RequestWebException { get; private set; }

	public bool IsMismatchContentType { get; private set; }

	public bool IsDebugLogActive { get; set; }

	public bool IsForceRequestRejectActive { get; set; }

	public bool IsDummyResponseActive { get; set; }

	public bool IsForceTimeoutActive { get; set; }

	public float DebugResponseWait { get; set; }

	public RequestCoroutine(string _url, string _request)
	{
		m_url = _url;
		m_request = _request;
		m_userDelegate = null;
		UserData = null;
	}

	public RequestCoroutine(string _url, string _request, RequestCoroutineDelegate _func, object _data)
	{
		m_url = _url;
		m_request = _request;
		m_userDelegate = _func;
		UserData = _data;
	}

	public IEnumerator Exec()
	{
		HttpWebRequestor requestor = null;
		int num2;
		for (int i = 0; i < EnvConnection.NetworkRetry; num2 = i + 1, i = num2)
		{
			if (ExitIfCancelled())
			{
				_ = EnvConnection.NetworkRetry;
				break;
			}
			if (i >= 1)
			{
				KAMUNITY.Logger.Log($"RequestCoroutine.Request: retry -->url:{m_url}, param:{m_request}, reretry count:{i}");
			}
			IsDone = false;
			IsFailed = false;
			IsTimeout = false;
			Error = "";
			UserError = "";
			StatusCode = StatusCode.OK;
			Progress = 0f;
			requestor = null;
			if (i > 0)
			{
				yield return new WaitForSeconds(EnvConnection.NetworkRetryWait);
			}
			if (ExitIfCancelled())
			{
				_ = EnvConnection.NetworkRetry;
				break;
			}
			byte[] form = null;
			if (m_request != null)
			{
				form = Encoding.UTF8.GetBytes(m_request);
			}
			if (IsDebugLogActive)
			{
				Debug.Log("■■■ Request = " + m_request);
			}
			requestor = new HttpWebRequestor(EnvProxy.ProxyHost, EnvProxy.ProxyPort, m_url, form, IsDebugLogActive)
			{
				RequestContentType = RequestContentType
			};
			if (IsForceRequestRejectActive || (!GameUtility.IsKamunityBootEnable() && DebugFlag.IsActive(DebugFlag.Flag.ForceNetworkDown)))
			{
				IsFailed = true;
				IsTimeout = true;
				Error = "rc1";
				StatusCode = StatusCode.ClientTimeout;
				UserError = RequestStatus.GetStatusMessage(StatusCode);
				KAMUNITY.Logger.Log("RequestCoroutine.Request: timeout -->url:" + m_url + ", param:" + m_request);
				continue;
			}
			if (GameUtility.IsKamunityBootEnable())
			{
				int code;
				int subcode;
				AvsTypes.EA3_STATUS eA3_STATUS = EagCore.Ea3GetStatus(out code, out subcode);
				if (eA3_STATUS != AvsTypes.EA3_STATUS.ONLINE)
				{
					IsFailed = true;
					Error = $"rc2:{eA3_STATUS}";
					StatusCode = StatusCode.UnConnectionError;
					UserError = RequestStatus.GetStatusMessage(StatusCode);
					KAMUNITY.Logger.Log($"RequestCoroutine.Request: ea status is not online -->url:{m_url}, param:{m_request}, status:{eA3_STATUS}");
					continue;
				}
			}
			if (IsDummyResponseActive)
			{
				ResponseText = "{\"dummy\":\"dummy\"}";
				break;
			}
			requestor.Request();
			float startTime = Time.realtimeSinceStartup;
			float localProgress = 0f;
			yield return null;
			while (!requestor.IsDone)
			{
				if (localProgress != requestor.Progress)
				{
					startTime = Time.realtimeSinceStartup;
				}
				localProgress = requestor.Progress;
				float num = Time.realtimeSinceStartup - startTime;
				if (ExitIfCancelled())
				{
					break;
				}
				if (EnvConnection.NetworkTimeout > 0f && num >= EnvConnection.NetworkTimeout)
				{
					IsTimeout = true;
					break;
				}
				Progress = requestor.Progress;
				if (requestor.IsFailed)
				{
					break;
				}
				yield return null;
			}
			if (ExitIfCancelled())
			{
				_ = EnvConnection.NetworkRetry;
				break;
			}
			string responceContentType = requestor.ResponceContentType;
			if (responceContentType != null && responceContentType.Length > 0)
			{
				IsMismatchContentType = requestor.RequestContentType != requestor.ResponceContentType;
			}
			if (IsForceTimeoutActive)
			{
				IsFailed = true;
				IsTimeout = true;
				Error = "rc3:" + requestor.Error;
				StatusCode = StatusCode.ClientTimeout;
				UserError = RequestStatus.GetStatusMessage(StatusCode);
				KAMUNITY.Logger.Log("RequestCoroutine.Request: timeout -->url:" + m_url + ", param:" + m_request);
				continue;
			}
			if (IsTimeout)
			{
				IsFailed = true;
				Error = "rc4:timeout";
				StatusCode = StatusCode.ClientTimeout;
				UserError = RequestStatus.GetStatusMessage(StatusCode);
				KAMUNITY.Logger.Log("RequestCoroutine.Request: timeout -->url:" + m_url + ", param:" + m_request);
				continue;
			}
			if (requestor.IsFailed)
			{
				IsFailed = true;
				Error = requestor.Error;
				StatusCode = requestor.StatusCode;
				UserError = requestor.UserError;
				RequestWebException = requestor.RequestWebException;
				KAMUNITY.Logger.Log("RequestCoroutine.Request: requestor failed -->url:" + m_url + ", param:" + m_request);
				continue;
			}
			try
			{
				if (requestor.ResponceText != "" || requestor.ResponceBytes != null)
				{
					switch (ResponceType)
					{
					case ResponseTypes.BYTE:
						ResponseByte = requestor.ResponceBytes;
						break;
					case ResponseTypes.TextJson:
					case ResponseTypes.TextXml:
					{
						byte[] responceBytes = requestor.ResponceBytes;
						ResponseText = Encoding.UTF8.GetString(responceBytes);
						break;
					}
					}
				}
			}
			catch (Exception ex)
			{
				IsFailed = true;
				Error = "rc5:" + ex.GetType().Name;
				StatusCode = StatusCode.ResponseCryptFailed;
				UserError = RequestStatus.GetStatusMessage(StatusCode);
			}
			break;
		}
		if (DebugResponseWait > 0f)
		{
			yield return new WaitForSeconds(DebugResponseWait);
		}
		IsDone = true;
		if (requestor != null)
		{
			Progress = requestor.Progress;
		}
		if (m_userDelegate != null && !m_userDelegate.Target.Equals(null))
		{
			m_userDelegate(this);
		}
	}

	private bool ExitIfCancelled()
	{
		if (CancellationToken.IsCancellationRequested)
		{
			IsFailed = true;
			Error = "Cancelled";
			StatusCode = StatusCode.UserAbort;
			UserError = RequestStatus.GetStatusMessage(StatusCode);
			return true;
		}
		return false;
	}
}
