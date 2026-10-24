using System;
using System.IO;
using System.Net;

namespace KAMUNITY;

public class HttpDownloader
{
	public Uri uri { get; private set; }

	public Stream outStream { get; private set; }

	public HttpWebResponse response { get; private set; }

	public Exception exception { get; private set; }

	public WebException webException { get; private set; }

	public bool isError
	{
		get
		{
			if (webException != null)
			{
				return true;
			}
			if (exception != null)
			{
				return true;
			}
			if (response != null && response.StatusCode != HttpStatusCode.OK)
			{
				return true;
			}
			return false;
		}
	}

	public void SetRequest(Uri _uri, Stream _outStream)
	{
		uri = _uri;
		outStream = _outStream;
	}

	public void Execute()
	{
		try
		{
			HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
			httpWebRequest.Method = "GET";
			httpWebRequest.Proxy = null;
			httpWebRequest.Host = uri.Host;
			httpWebRequest.UserAgent = "KAMUNITY";
			httpWebRequest.KeepAlive = false;
			response = (HttpWebResponse)httpWebRequest.GetResponse();
			byte[] array = new byte[4096];
			long num = 0L;
			using (Stream stream = response.GetResponseStream())
			{
				int num2;
				do
				{
					num2 = stream.Read(array, 0, array.Length);
					if (num2 > 0)
					{
						outStream.Write(array, 0, num2);
						num += num2;
					}
				}
				while (num2 > 0);
			}
			if (response.ContentLength >= 0)
			{
				_ = response.ContentLength;
			}
		}
		catch (WebException ex)
		{
			WebException ex4 = (WebException)(exception = (webException = ex));
			if (ex4.Response != null)
			{
				response = (HttpWebResponse)ex4.Response;
			}
			Logger.LogException(ex4);
		}
		catch (Exception ex5)
		{
			Exception e = (exception = ex5);
			Logger.LogException(e);
		}
	}
}
