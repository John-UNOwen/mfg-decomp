using System;
using System.Diagnostics;
using System.Threading;
using System.Xml.Linq;
using Cysharp.Threading.Tasks;
using KAMUNITY;
using Konami.System.Net;
using MFG.Sytem.Utilities.Xml;

namespace MFG.GameRequest;

public abstract class RequestApiBase<ApiType, ResultType> where ApiType : RequestApiBase<ApiType, ResultType>, new() where ResultType : ResultBase, new()
{
	protected string m_sendUrl;

	protected string m_postStr;

	protected string m_response;

	protected string m_errorMessage = "";

	protected string m_serverLog = "";

	protected string m_moduleName = "game";

	protected string m_methodName;

	private static string s_eaEnvParam;

	protected Stopwatch m_stopWatch = new Stopwatch();

	protected ResultType Result { get; private set; } = new ResultType();


	public bool EnableErrorScreenTransition { get; set; } = true;


	public bool EnableSendLog { get; set; } = true;


	private static string EaEnvParam
	{
		get
		{
			if (string.IsNullOrEmpty(s_eaEnvParam) || string.IsNullOrEmpty(SystemUtility.Instance.LocationID))
			{
				s_eaEnvParam = "system_id=" + SystemUtility.Instance.SystemID + "&sic=" + SystemUtility.Instance.SoftID + "&loc_id=" + SystemUtility.Instance.LocationID;
			}
			return s_eaEnvParam;
		}
	}

	protected string AppVersion => GameDataManager.System.AppVersion;

	protected string DefaultFrontServerUrl => SystemConfigGameData.FrontServerUrl;

	protected string MobileUrl => GameDataManager.System.MobileUrl;

	protected int AreaID => GameDataManager.System.AreaID;

	protected int WebID => GameDataManager.System.WebID;

	protected int GameServiceID => GameDataManager.Taikyoku.TakuInfo.GameServiceID;

	protected string AppSessionID => GameDataManager.Account.AppSessionID;

	protected string TaikyokuSessionID => GameDataManager.Taikyoku.TakuInfo.SessionID;

	public static async UniTask<ResultType> Send(bool enableErrorScreenTransition = true, bool enableSendLog = true, CancellationToken cancellationToken = default(CancellationToken))
	{
		ApiType api = new ApiType
		{
			EnableErrorScreenTransition = enableErrorScreenTransition,
			EnableSendLog = enableSendLog
		};
		await api.PushRequest(cancellationToken);
		return api.Result;
	}

	public async UniTask PushRequest(CancellationToken cancellationToken = default(CancellationToken))
	{
		_ = 1;
		try
		{
			cancellationToken.ThrowIfCancellationRequested();
			OnCreateSendParameter();
			if (!GameUtility.IsKonamiNetConnection)
			{
				if (string.IsNullOrEmpty(m_postStr))
				{
					m_postStr = EaEnvParam;
				}
				else
				{
					m_postStr = EaEnvParam + "&" + m_postStr;
				}
			}
			Logger.Log($"{DateTime.Now.Second}:{DateTime.Now.Millisecond} {GetType().Name}.PushRequest--> url: {m_sendUrl} data: {m_postStr}");
			m_stopWatch.Start();
			Response response = await SingletonBehaviour<RequestManager>.Instance.PushRequest(m_sendUrl, m_postStr, _manualRetry: false, cancellationToken);
			cancellationToken.ThrowIfCancellationRequested();
			m_response = response.Contents;
			m_errorMessage = response.Message;
			m_stopWatch.Stop();
			if (DebugFlag.IsActive(DebugFlag.Flag.EnableRequestAPIResponseLog))
			{
				Logger.Log(GetType().Name + " response: " + m_response);
			}
			if (string.IsNullOrEmpty(m_response))
			{
				throw new NetworkResponseException(StatusCode.ResponseEmpty, m_errorMessage);
			}
			XElement root = XDocument.Parse(m_response).Root;
			await OnParse(root, cancellationToken);
		}
		catch (NeedNotNodeNotFoundException)
		{
		}
		catch (OperationCanceledException)
		{
			if (!EnableErrorScreenTransition)
			{
				Logger_Error("通信キャンセル " + GetType().Name + " url: " + m_sendUrl + " data: " + m_postStr);
				throw;
			}
		}
		catch (Exception ex3)
		{
			Logger_Error(FormatErrorMessageForConsoleLog(ex3));
			if (EnableSendLog && !TitleDemoUtility.IsNetworkMaintenance())
			{
				GameUtility.ServerLog.Reserve("network_error", FormatErrorMessageForServerLog());
			}
			if (EnableErrorScreenTransition)
			{
				ExecErrorScreenTransition();
				return;
			}
			throw new NetworkResponseException(Result.StatusCode, ex3.Message);
		}
	}

	public async UniTask<ResultType> DebugOnParse(string responseTxt, CancellationToken cancellationToken = default(CancellationToken))
	{
		m_response = responseTxt;
		XElement root = XDocument.Parse(m_response).Root;
		await OnParse(root, cancellationToken);
		m_response = responseTxt;
		return Result;
	}

	protected abstract void OnCreateSendParameter();

	protected virtual async UniTask OnParse(XElement baseElem, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		ParseResultStatusCode(baseElem);
	}

	protected void ParseResultStatusCode(XElement baseElem)
	{
		XElement baseElement = baseElem.NeedNotElement("serv_st");
		int num = 630;
		try
		{
			num = baseElement.MustNeedElement("code").ValueInt();
			Logger.Log($"{DateTime.Now.Second}:{DateTime.Now.Millisecond} {GetType().Name}--> resultcode:{num} sec:{m_stopWatch.Elapsed.TotalSeconds:F1}");
		}
		catch
		{
		}
		if (num == 0)
		{
			Result.StatusCode = StatusCode.OK;
			return;
		}
		Result.StatusCode = (StatusCode)num;
		throw new NetworkResponseException(Result.StatusCode, m_errorMessage);
	}

	private void ExecErrorScreenTransition()
	{
		int errorCode = 2600;
		if (!GameUtility.IsKonamiNetConnection && !TitleDemoUtility.IsNetworkActive() && EagCore.Instance.Code == 2000)
		{
			errorCode = EagCore.Instance.Code;
		}
		ErrorControl.SetError((ERROR)errorCode, 0u, ERROR_LEVEL.ERROR_STOP);
	}

	protected virtual string FormatErrorMessageForConsoleLog(Exception e = null)
	{
		return "通信エラー " + GetType().Name + " url: " + m_sendUrl + " data: " + m_postStr + " response: " + m_response + " msg:" + e.Message;
	}

	protected virtual string FormatErrorMessageForServerLog()
	{
		return $"{DateTimeOffsetManager.GetJSTTime()},{GetType().Name},{Result.StatusCode},{GameDataManager.Account.DataID},{m_serverLog},{m_errorMessage}";
	}

	private void Logger_Error(string errorLogString)
	{
		Logger.Log(errorLogString);
	}
}
public abstract class RequestApiBase<ApiType, ResultType, SendParameterType> : RequestApiBase<ApiType, ResultType> where ApiType : RequestApiBase<ApiType, ResultType, SendParameterType>, new() where ResultType : ResultBase, new() where SendParameterType : new()
{
	protected SendParameterType SendParameter { get; set; } = new SendParameterType();


	public static async UniTask<ResultType> Send(SendParameterType param, bool enableErrorScreenTransition = true, bool enableSendLog = true, CancellationToken cancellationToken = default(CancellationToken))
	{
		ApiType api = new ApiType
		{
			EnableErrorScreenTransition = enableErrorScreenTransition,
			SendParameter = param,
			EnableSendLog = enableSendLog
		};
		await api.PushRequest(cancellationToken);
		return api.Result;
	}
}
