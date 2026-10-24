using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using Konami.System.Net;

public class RequestManager : RequestManagerBase<RequestManager>
{
	protected string switchUrl = "";

	public string SwitchUrl
	{
		get
		{
			return switchUrl;
		}
		set
		{
			switchUrl = value;
		}
	}

	protected override void initialize()
	{
		base.initialize();
		responseCacheList.Add(ResponseCacheBase<SessionCache>.Instance);
		ResponseCacheBase<SessionCache>.Instance.ClearSession();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	public async UniTask<Response> PushRequest(string _url, string _postString = null, bool _manualRetry = false, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		return await PushRequest(_url, _postString, _manualRetry, defaultFaildCallBack, null, retryCallBack, cancellationToken);
	}

	public async UniTask<Response> PushRequest(string _url, string _postString, bool _manualRetry, RequestDelegate _failedFunc, FoceMoving _forceMoving, RequestDelegate _retryFunc, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		string requestId = ServerTime.GetDateTime.ToString("yyyyMMddHHmmssfff");
		_ = NetEnvironment.Instance.Connection;
		UniTaskCompletionSource<RequestParameter> source = new UniTaskCompletionSource<RequestParameter>();
		RequestParameter parameter = new RequestParameter
		{
			RequestType = RequestType.ApiXml,
			PostString = _postString,
			Url = _url,
			RequestId = requestId,
			RetryInfo = ServerTime.Get,
			UserDelegate = delegate(RequestParameter response)
			{
				source.TrySetResult(response);
			},
			CloseDelegate = delegate(RequestParameter response)
			{
				source.TrySetResult(response);
			},
			FaildDelegate = delegate(RequestParameter response)
			{
				source.TrySetResult(response);
			},
			ManualRetry = _manualRetry,
			RetryDelegate = delegate(RequestParameter response)
			{
				source.TrySetResult(response);
			},
			CancellationToken = cancellationToken
		};
		PushRequest(parameter);
		return (Response)(await source.Task);
	}

	public void RetryRequest(RequestParameter _parameter)
	{
		RequestParameter parameter = new RequestParameter
		{
			RequestType = _parameter.RequestType,
			Url = _parameter.Url,
			MethodType = _parameter.MethodType,
			PostString = _parameter.PostString,
			RequestId = _parameter.RequestId,
			RetryInfo = _parameter.RetryInfo,
			UserDelegate = _parameter.UserDelegate,
			CloseDelegate = _parameter.CloseDelegate,
			FaildDelegate = _parameter.FaildDelegate,
			ManualRetry = _parameter.ManualRetry,
			RetryDelegate = _parameter.RetryDelegate,
			EnableRetry = false
		};
		PushRequest(parameter);
	}

	private void defaultFaildCallBack(RequestParameter _parameter)
	{
		IUserImplementsClass userImplementsClass = UserImplementsClass.Instance;
		string word = userImplementsClass.GetWord(Word.TitleConnectionFailed);
		string error = _parameter.Error;
		userImplementsClass.OnRequestFailedDefault(word, error);
	}

	private void retryCallBack(RequestParameter _parameter)
	{
		UserImplementsClass.Instance.OnAskRetryOrRestart(null, _parameter.Error, delegate
		{
			SingletonBehaviour<RequestManager>.Instance.RetryRequest(_parameter);
		}, delegate
		{
			if (_parameter.CloseDelegate != null && !_parameter.CloseDelegate.Target.Equals(null))
			{
				_parameter.CloseDelegate(_parameter);
			}
		});
	}

	protected override bool ResponseHandling(RequestParameter _parameter, ref StatusCode _statusCode, ref string _error)
	{
		return base.ResponseHandling(_parameter, ref _statusCode, ref _error);
	}

	protected override void OnParseResult(Hashtable _result)
	{
		base.OnParseResult(_result);
		IUserImplementsClass userImplementsClass = UserImplementsClass.Instance;
		if (_result.ContainsKey("notification") && _result["notification"] != null)
		{
			bool bOn = int.Parse(_result["notification"]?.ToString() ?? "") != 0;
			userImplementsClass.OnChangeNotificationSetting(bOn);
		}
	}
}
