using System.Net;

namespace Konami.System.Net;

public class RequestStatus
{
	public static StatusCode GetStatusCode(StatusCode _defaultCode, WebException _e = null)
	{
		StatusCode result = _defaultCode;
		if (_e == null || _e.Response == null)
		{
			return result;
		}
		if (_e.Status == WebExceptionStatus.ProtocolError)
		{
			result = (StatusCode)((HttpWebResponse)_e.Response).StatusCode;
		}
		return result;
	}

	public static string GetStatusMessage(StatusCode _statusCode, WebException _e = null)
	{
		IUserImplementsClass instance = UserImplementsClass.Instance;
		int num = (int)_statusCode;
		string text = num.ToString() ?? "";
		if (_e != null && _e.Status != WebExceptionStatus.ProtocolError)
		{
			text = text + "_" + (int)_e.Status;
		}
		else if (_e != null && _e.Status == WebExceptionStatus.ProtocolError && _e.Response != null && _e.Response is HttpWebResponse)
		{
			text = text + "_" + (int)_e.Status + "_" + (int)((HttpWebResponse)_e.Response).StatusCode;
		}
		string result = "【" + text + "】\n" + instance.GetWord(Word.MessageConnectionError);
		if (_statusCode == StatusCode.ClientTimeout)
		{
			result = "【" + text + "】\n" + instance.GetWord(Word.MessageConnectionTimeout);
		}
		if (_e != null)
		{
			switch (_e.Status)
			{
			case WebExceptionStatus.NameResolutionFailure:
				result = "【" + text + "】\n" + instance.GetWord(Word.MessageConnectionError);
				break;
			case WebExceptionStatus.ConnectFailure:
				result = "【" + text + "】\n" + instance.GetWord(Word.MessageConnectionError);
				break;
			}
		}
		return result;
	}
}
