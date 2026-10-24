using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

namespace Konami.System.Net;

public abstract class RequestManagerBase<T> : SingletonBehaviour<T> where T : SingletonBehaviour<T>
{
	private int MaxParallelRequestCount = 1;

	private int MaxParallelResponseCount = 1;

	protected int nowParallelRequestCount;

	protected int nowParallelResponseCount;

	private Queue<RequestParameter> requestQueue = new Queue<RequestParameter>();

	private Queue<RequestParameter> responseQueue = new Queue<RequestParameter>();

	protected List<IResponseCache> responseCacheList = new List<IResponseCache>();

	[SerializeField]
	private bool dispDebugLog;

	[SerializeField]
	private bool sendBlock;

	[SerializeField]
	private bool useFakeResponse;

	[SerializeField]
	private float debugResponseWait;

	private bool isIndicator;

	public bool IsForceReceiveFailedErrorActive { get; private set; }

	public bool SendBlock
	{
		get
		{
			return sendBlock;
		}
		set
		{
			sendBlock = value;
		}
	}

	public bool UseFakeResponse
	{
		get
		{
			return useFakeResponse;
		}
		set
		{
			useFakeResponse = value;
		}
	}

	public float DebugResponseWait
	{
		get
		{
			return debugResponseWait;
		}
		set
		{
			debugResponseWait = value;
		}
	}

	public bool IsThrowRequest => isIndicator;

	public bool DispLoading { get; set; } = true;


	public bool isBusy
	{
		get
		{
			if (nowParallelRequestCount <= 0)
			{
				return nowParallelResponseCount > 0;
			}
			return true;
		}
	}

	public bool DispDebugLog
	{
		get
		{
			return dispDebugLog;
		}
		set
		{
			dispDebugLog = value;
		}
	}

	protected override void initialize()
	{
		base.initialize();
		requestQueue = new Queue<RequestParameter>();
		responseQueue = new Queue<RequestParameter>();
		responseCacheList = new List<IResponseCache>();
		nowParallelRequestCount = 0;
		nowParallelResponseCount = 0;
		CertificateValidation.Init();
		ServicePointManager.ServerCertificateValidationCallback = CertificateValidation.RemoteCertificateValidationCallback;
	}

	public void PushRequest(RequestParameter _parameter)
	{
		if (_parameter != null)
		{
			requestQueue.Enqueue(_parameter);
		}
	}

	private RequestParameter popRequest()
	{
		RequestParameter result = null;
		if (requestQueue.Count <= 0)
		{
			return result;
		}
		return requestQueue.Dequeue();
	}

	private void pushResponse(RequestParameter _parameter)
	{
		if (_parameter != null)
		{
			responseQueue.Enqueue(_parameter);
		}
	}

	private RequestParameter popResponse()
	{
		RequestParameter result = null;
		if (responseQueue.Count <= 0)
		{
			return result;
		}
		return responseQueue.Dequeue();
	}

	private IEnumerator ExecRequest(RequestParameter _parameter)
	{
		_parameter.Begin(dispDebugLog);
		RequestCoroutine requestCoroutine = new RequestCoroutine(_parameter.Url, _parameter.PostString)
		{
			EnvConnection = NetEnvironment.Instance.Connection,
			EnvProxy = NetEnvironment.Instance.Connection.Proxy,
			MethodType = _parameter.MethodType,
			ResponceType = _parameter.ResponseType,
			UseRandomKey = _parameter.UseRandomKey,
			IsForceRequestRejectActive = sendBlock,
			IsDummyResponseActive = useFakeResponse,
			IsForceTimeoutActive = IsForceReceiveFailedErrorActive,
			DebugResponseWait = debugResponseWait,
			IsDebugLogActive = dispDebugLog,
			CancellationToken = _parameter.CancellationToken
		};
		yield return StartCoroutine(requestCoroutine.Exec());
		_parameter.IsSystemTimeout = requestCoroutine.IsTimeout;
		_parameter.ResponseText = requestCoroutine.ResponseText;
		_parameter.ResponseByte = requestCoroutine.ResponseByte;
		if (requestCoroutine.IsFailed)
		{
			_parameter.IsDone = true;
			_parameter.IsSystemFailed = true;
			_parameter.Error = requestCoroutine.Error;
			_parameter.StatusCode = requestCoroutine.StatusCode;
			_parameter.UserError = requestCoroutine.UserError;
			_parameter.RequestWebException = requestCoroutine.RequestWebException;
			_parameter.Done();
		}
		else if (requestCoroutine.ResponseText.Length <= 0)
		{
			_parameter.IsDone = true;
			_parameter.IsSystemFailed = true;
			_parameter.Error = requestCoroutine.Error;
			_parameter.StatusCode = StatusCode.ResponseEmpty;
			_parameter.UserError = RequestStatus.GetStatusMessage(StatusCode.ResponseEmpty);
			_parameter.Done();
		}
		else
		{
			_parameter.StatusCode = StatusCode.OK;
			pushResponse(_parameter);
		}
		if (nowParallelRequestCount > 0)
		{
			nowParallelRequestCount--;
		}
		_parameter = null;
	}

	private IEnumerator responseCoroutine(RequestParameter _parameter)
	{
		yield return null;
		if (dispDebugLog)
		{
			Debug.Log("■■■ Response = " + _parameter.ResponseText);
		}
		_parameter.StatusCode = StatusCode.OK;
		_parameter.Error = "";
		if (_parameter.ResponseText != null)
		{
			_parameter.IsSystemFailed = !ResponseHandling(_parameter, ref _parameter.StatusCode, ref _parameter.Error);
		}
		_parameter.Done();
		if (nowParallelResponseCount > 0)
		{
			nowParallelResponseCount--;
		}
		_parameter = null;
	}

	protected virtual bool ResponseHandling(RequestParameter _parameter, ref StatusCode _statusCode, ref string _error)
	{
		return true;
	}

	protected virtual void OnParseResult(Hashtable result)
	{
		for (int i = 0; i < responseCacheList.Count; i++)
		{
			responseCacheList[i].Parse(result);
		}
	}

	protected virtual void OnLateParseResult(Hashtable result)
	{
		for (int i = 0; i < responseCacheList.Count; i++)
		{
			responseCacheList[i].LateParse();
		}
	}

	private void Update()
	{
		if (responseQueue.Count > 0 && nowParallelResponseCount < MaxParallelResponseCount)
		{
			RequestParameter requestParameter = popResponse();
			if (requestParameter != null)
			{
				nowParallelResponseCount++;
				StartCoroutine(responseCoroutine(requestParameter));
			}
		}
		if (requestQueue.Count > 0 && nowParallelRequestCount < MaxParallelRequestCount)
		{
			RequestParameter requestParameter2 = popRequest();
			if (requestParameter2 != null)
			{
				nowParallelRequestCount++;
				StartCoroutine(ExecRequest(requestParameter2));
			}
		}
	}
}
