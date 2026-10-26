using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using KAMUNITY;
using UnityEngine;
#if UNITY_WEBGL
using UnityEngine.Networking;
#endif

namespace Konami.System.Net {

public class HttpWebRequestor
{
	private const string ContentType = "application/x-www-form-urlencoded";

	private static string userAgent = "";

	private string proxyHost = "";

	private int proxyPort;

	private string url = "";

	private byte[] form;

	private HttpWebRequest webRequest;

	private long totalLength;

	private long currentLength;

	private byte[] bytes;

	private MemoryStream memoryStream;

	private bool dispDebugLog;

	public string RequestContentType { get; set; } = "application/x-www-form-urlencoded";


	public bool IsDone { get; private set; }

	public bool IsFailed { get; private set; }

	public string Error { get; private set; } = "";


	public string UserError { get; private set; } = "";


	public StatusCode StatusCode { get; private set; } = StatusCode.OK;


	public WebException RequestWebException { get; private set; }

	public string ResponceText { get; private set; } = "";


	public string ResponceContentType { get; private set; } = "";


	public byte[] ResponceBytes { get; private set; }

	public float Progress
	{
		get
		{
			float result = 0f;
			if (totalLength > 0)
			{
				result = (float)currentLength / (float)totalLength;
			}
			return result;
		}
	}

	public static string UserAgent()
	{
		return $"{SystemInfo.operatingSystem}/{SystemInfo.deviceModel}/{Application.unityVersion}/{SystemInfo.graphicsDeviceName}";
	}

	private static bool RemoteCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
	{
		bool flag = false;
		return sslPolicyErrors switch
		{
			SslPolicyErrors.None => true, 
			SslPolicyErrors.RemoteCertificateChainErrors => true, 
			SslPolicyErrors.RemoteCertificateNameMismatch => true, 
			SslPolicyErrors.RemoteCertificateNotAvailable => true, 
			_ => true, 
		};
	}

	public HttpWebRequestor(string _proxyHost, int _proxyPort, string _url, byte[] _form, bool _dispDebugLog = false)
	{
		try
		{
			proxyHost = _proxyHost;
			proxyPort = _proxyPort;
			url = _url;
			form = _form;
			bytes = new byte[1024];
			memoryStream = new MemoryStream();
			dispDebugLog = _dispDebugLog;
			RequestContentType = "application/x-www-form-urlencoded";
			Error = "";
			UserError = "";
			StatusCode = StatusCode.OK;
			if (userAgent == "")
			{
				userAgent = UserAgent();
			}
		}
		catch (Exception)
		{
		}
	}

	private void done(string _error = "", string _userError = "", StatusCode _statusCode = StatusCode.OK, WebException _webException = null)
	{
		if (_error.Length > 0)
		{
			Error = _error;
			IsFailed = true;
		}
		if (_userError.Length > 0)
		{
			UserError = _userError;
			IsFailed = true;
		}
		StatusCode = _statusCode;
		RequestWebException = _webException;
		IsDone = true;
		if (IsFailed)
		{
			KAMUNITY.Logger.LogFormat("HttpWebRequestor.done-->failed msg:{0} code:{1} {2}", _error, (int)StatusCode, StatusCode);
		}
	}

	private string ToErrorString(WebException e)
	{
		return string.Format("{0}/{1}", e.Status, (e.Status == WebExceptionStatus.ProtocolError) ? ((object)(int)((HttpWebResponse)e.Response).StatusCode) : "-");
	}

	public void Request()
	{
		if (IsDone || url.Length <= 0)
		{
			return;
		}
#if UNITY_WEBGL
		// Rebuild: HttpWebRequest's BeginGet* pair needs threads and sockets, both absent in
		// the browser (BeginGetRequestStream threw DllNotFoundException on every front call).
		// UnityWebRequest is the supported XHR transport; the completed callback fills the
		// same result fields the coroutine pump already polls (IsDone/Progress/Responce*).
		{
			UnityWebRequest uwr;
			if (form != null)
			{
				uwr = new UnityWebRequest(url, "POST");
				uwr.uploadData = form;
				uwr.SetRequestHeader("Content-Type", RequestContentType);
			}
			else
			{
				uwr = UnityWebRequest.Get(url);
			}
			if (userAgent.Length > 0)
			{
				uwr.SetRequestHeader("User-Agent", userAgent);
			}
			uwr.timeout = 60;
			UnityWebRequestAsyncOperation op = uwr.SendWebRequest();
			op.completed += delegate
			{
				if (IsDone)
				{
					return;
				}
				if (uwr.result == UnityWebRequest.Result.Success)
				{
					ResponceBytes = uwr.downloadHandler.data;
					ResponceText = uwr.downloadHandler.text;
					ResponceContentType = uwr.GetResponseHeader("Content-Type") ?? "";
					totalLength = ResponceBytes?.LongLength ?? 0;
					currentLength = totalLength;
					done();
				}
				else
				{
					StatusCode statusCode = StatusCode.ConnectionBeginError;
					string statusMessage = RequestStatus.GetStatusMessage(statusCode);
					done("wgl:" + uwr.responseCode + ":" + uwr.error, statusMessage, statusCode);
				}
				uwr.Dispose();
			};
			return;
		}
#endif
		try
		{
			CertificateValidation.Init();
			ServicePointManager.ServerCertificateValidationCallback = CertificateValidation.RemoteCertificateValidationCallback;
			webRequest = (HttpWebRequest)WebRequest.Create(url);
			ServicePointManager.Expect100Continue = false;
			if (webRequest == null)
			{
				StatusCode statusCode = StatusCode.UnConnectionError;
				string statusMessage = RequestStatus.GetStatusMessage(statusCode);
				done("rq1", statusMessage, statusCode);
				return;
			}
			webRequest.UserAgent = userAgent;
			webRequest.CookieContainer = new CookieContainer();
			webRequest.KeepAlive = false;
			webRequest.Proxy = null;
			if (proxyHost.Length > 0)
			{
				WebProxy proxy = new WebProxy(proxyHost, proxyPort);
				webRequest.Proxy = proxy;
			}
			webRequest.ContentLength = ((form != null) ? form.Length : 0);
			if (form != null)
			{
				webRequest.Method = "POST";
				webRequest.ContentType = RequestContentType;
				webRequest.ContentLength = form.Length;
				if (webRequest.BeginGetRequestStream(requestCallback, webRequest) == null)
				{
					StatusCode statusCode2 = StatusCode.ConnectionBeginError;
					string statusMessage2 = RequestStatus.GetStatusMessage(statusCode2);
					done("rq2", statusMessage2, statusCode2);
				}
			}
			else
			{
				webRequest.Method = "GET";
				webRequest.ContentType = "application/x-www-form-urlencoded";
				webRequest.SendChunked = false;
				if (webRequest.BeginGetResponse(responseCallback, webRequest) == null)
				{
					StatusCode statusCode3 = StatusCode.ConnectionBeginError;
					string statusMessage3 = RequestStatus.GetStatusMessage(statusCode3);
					done("rq3", statusMessage3, statusCode3);
				}
			}
		}
		catch (WebException ex)
		{
			StatusCode statusCode4 = RequestStatus.GetStatusCode(StatusCode.ConnectionBeginErrorWebException, ex);
			string statusMessage4 = RequestStatus.GetStatusMessage(statusCode4, ex);
			done("rq4:" + ToErrorString(ex), statusMessage4, statusCode4, ex);
		}
		catch (Exception ex2)
		{
			StatusCode statusCode5 = StatusCode.ConnectionBeginErrorException;
			string statusMessage5 = RequestStatus.GetStatusMessage(statusCode5);
			done("rq5:" + ex2.GetType().Name, statusMessage5, statusCode5);
		}
	}

	private void requestCallback(IAsyncResult ar)
	{
		Stream stream = null;
		try
		{
			if (!IsDone)
			{
				HttpWebRequest httpWebRequest = (HttpWebRequest)ar.AsyncState;
				if (httpWebRequest == null)
				{
					StatusCode statusCode = StatusCode.ConnectFailure;
					string statusMessage = RequestStatus.GetStatusMessage(statusCode);
					done("rqCb1", statusMessage, statusCode);
				}
				else
				{
					stream = httpWebRequest.EndGetRequestStream(ar);
					stream.BeginWrite(form, 0, form.Length, writeCallback, stream);
				}
			}
		}
		catch (WebException ex)
		{
			if (stream != null)
			{
				stream.Close();
				stream = null;
			}
			StatusCode statusCode2 = RequestStatus.GetStatusCode(StatusCode.ConnectFailureWebException, ex);
			string statusMessage2 = RequestStatus.GetStatusMessage(statusCode2, ex);
			done("rqCb2:" + ToErrorString(ex), statusMessage2, statusCode2, ex);
		}
		catch (Exception ex2)
		{
			if (stream != null)
			{
				stream.Close();
				stream = null;
			}
			StatusCode statusCode3 = StatusCode.ConnectFailureException;
			string statusMessage3 = RequestStatus.GetStatusMessage(statusCode3);
			done("rqCb3:" + ex2.GetType().Name, statusMessage3, statusCode3);
		}
	}

	private void writeCallback(IAsyncResult ar)
	{
		Stream stream = (Stream)ar.AsyncState;
		try
		{
			if (!IsDone)
			{
				stream.EndWrite(ar);
				stream.Close();
				if (webRequest.BeginGetResponse(responseCallback, webRequest) == null)
				{
					StatusCode statusCode = StatusCode.SendFailure;
					string statusMessage = RequestStatus.GetStatusMessage(statusCode);
					done("wtCb1", statusMessage, statusCode);
				}
			}
		}
		catch (WebException ex)
		{
			stream?.Close();
			StatusCode statusCode2 = RequestStatus.GetStatusCode(StatusCode.SendFailureWebException, ex);
			string statusMessage2 = RequestStatus.GetStatusMessage(statusCode2, ex);
			done("wtCb2:" + ToErrorString(ex), statusMessage2, statusCode2, ex);
		}
		catch (Exception ex2)
		{
			StatusCode statusCode3 = StatusCode.SendFailureException;
			string statusMessage3 = RequestStatus.GetStatusMessage(statusCode3);
			done("wtCb3:" + ex2.GetType().Name, statusMessage3, statusCode3);
			stream?.Close();
		}
	}

	private void responseCallback(IAsyncResult ar)
	{
		Stream stream = null;
		try
		{
			if (IsDone)
			{
				return;
			}
			HttpWebRequest httpWebRequest = (HttpWebRequest)ar.AsyncState;
			if (httpWebRequest == null)
			{
				StatusCode statusCode = StatusCode.ResponseBegineError;
				string statusMessage = RequestStatus.GetStatusMessage(statusCode);
				done("resCb1", statusMessage, statusCode);
				return;
			}
			HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.EndGetResponse(ar);
			if (httpWebResponse == null)
			{
				StatusCode statusCode2 = StatusCode.ResponseBegineError;
				string statusMessage2 = RequestStatus.GetStatusMessage(statusCode2);
				done("resCb2", statusMessage2, statusCode2);
				return;
			}
			ResponceContentType = httpWebResponse.ContentType;
			totalLength = httpWebResponse.ContentLength;
			stream = httpWebResponse.GetResponseStream();
			if (stream == null)
			{
				httpWebResponse.Close();
				httpWebResponse = null;
				StatusCode statusCode3 = StatusCode.ResponseBegineError;
				string statusMessage3 = RequestStatus.GetStatusMessage(statusCode3);
				done("resCb3", statusMessage3, statusCode3);
			}
			else
			{
				stream.BeginRead(bytes, 0, bytes.Length, readCallback, new Tuple<Stream, WebResponse>(stream, httpWebResponse));
			}
		}
		catch (WebException ex)
		{
			if (stream != null)
			{
				stream.Close();
				stream = null;
			}
			StatusCode statusCode4 = RequestStatus.GetStatusCode(StatusCode.ResponseBegineErrorWebException, ex);
			string statusMessage4 = RequestStatus.GetStatusMessage(statusCode4, ex);
			done("resCb4:" + ToErrorString(ex), statusMessage4, statusCode4, ex);
		}
		catch (Exception ex2)
		{
			if (stream != null)
			{
				stream.Close();
				stream = null;
			}
			StatusCode statusCode5 = StatusCode.ResponseBegineErrorException;
			string statusMessage5 = RequestStatus.GetStatusMessage(statusCode5);
			done("resCb5:" + ex2.GetType().Name, statusMessage5, statusCode5);
		}
	}

	private void readCallback(IAsyncResult ar)
	{
		Stream stream = null;
		WebResponse webResponse = null;
		try
		{
			if (IsDone)
			{
				return;
			}
			Tuple<Stream, WebResponse> tuple = (Tuple<Stream, WebResponse>)ar.AsyncState;
			stream = tuple.Item1;
			webResponse = tuple.Item2;
			if (stream == null)
			{
				StatusCode statusCode = StatusCode.ResponseStreamFailed;
				string statusMessage = RequestStatus.GetStatusMessage(statusCode);
				done("rdCb1", statusMessage, statusCode);
				return;
			}
			int num = stream.EndRead(ar);
			if (num > 0)
			{
				memoryStream.Write(bytes, 0, num);
				currentLength += num;
				stream.BeginRead(bytes, 0, bytes.Length, readCallback, tuple);
				return;
			}
			ResponceBytes = memoryStream.ToArray();
			ResponceText = Encoding.UTF8.GetString(ResponceBytes);
			done();
			stream.Close();
			stream = null;
			memoryStream.Close();
			memoryStream = null;
			webResponse.Dispose();
			webResponse = null;
		}
		catch (WebException ex)
		{
			if (stream != null)
			{
				stream.Close();
				stream = null;
			}
			if (webResponse != null)
			{
				webResponse.Dispose();
				webResponse = null;
			}
			StatusCode statusCode2 = RequestStatus.GetStatusCode(StatusCode.ResponseFailedWebException, ex);
			string statusMessage2 = RequestStatus.GetStatusMessage(statusCode2, ex);
			done("rdCb2:" + ToErrorString(ex), statusMessage2, statusCode2, ex);
		}
		catch (Exception ex2)
		{
			if (stream != null)
			{
				stream.Close();
				stream = null;
			}
			if (webResponse != null)
			{
				webResponse.Dispose();
				webResponse = null;
			}
			StatusCode statusCode3 = StatusCode.ResponseFailedException;
			string statusMessage3 = RequestStatus.GetStatusMessage(statusCode3);
			done("rdCb3:" + ex2.GetType().Name, statusMessage3, statusCode3);
		}
	}
}
}
