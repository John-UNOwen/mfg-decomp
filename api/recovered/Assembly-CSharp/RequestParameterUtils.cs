using System.Collections;
using Konami.System.Net;

public static class RequestParameterUtils
{
	public static bool IsErrorResponse(RequestParameter _parameter)
	{
		return false;
	}

	public static string GetPartialMaintenanceMessage(RequestParameter _parameter)
	{
		return "";
	}

	public static bool IsPartialMaintenance(RequestParameter _parameter, PartialMaintenanceType _type)
	{
		return false;
	}

	public static string GetPartialMaintenanceTitle(RequestParameter _parameter)
	{
		return "";
	}

	public static string GetErrorCode(RequestParameter _parameter)
	{
		return "";
	}

	public static string GetErrorMessage(RequestParameter _parameter)
	{
		_ = UserImplementsClass.Instance;
		return "";
	}

	public static string GetDebugMessage(RequestParameter _parameter)
	{
		return "";
	}

	public static bool IsNeedApplicationUpdate(RequestParameter _parameter)
	{
		return true;
	}

	public static (string url, string postString) ExtractParameterToPostData(string _url, MethodTypes _type, Hashtable _req)
	{
		string text = _url;
		string item = null;
		if (_type == MethodTypes.GET)
		{
			string text2 = "";
			string text3 = ((_url.IndexOf("?") < 0) ? "?" : "&");
			foreach (string key in _req.Keys)
			{
				if (_req[key] is Hashtable)
				{
					Hashtable hashtable = _req[key] as Hashtable;
					foreach (string key2 in hashtable.Keys)
					{
						text2 = text2 + text3 + key2 + "=" + hashtable[key2];
						text3 = "&";
					}
				}
				else
				{
					text2 = text2 + text3 + key + "=" + _req[key];
					text3 = "&";
				}
			}
			if (text2.Length > 0)
			{
				text += text2;
			}
		}
		else
		{
			_ = 1;
		}
		return (url: text, postString: item);
	}
}
