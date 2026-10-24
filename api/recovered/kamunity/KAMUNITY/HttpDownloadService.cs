using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;

namespace KAMUNITY;

public class HttpDownloadService : MonoBehaviour
{
	private class RequestData
	{
		public HttpDownloader downloader;

		public Action<HttpDownloader, object> callback;

		public object parameter;

		public bool disposeAfterCallback;
	}

	private Queue<RequestData> m_Request = new Queue<RequestData>();

	private RequestData m_Cur;

	private Thread m_Thread;

	public static HttpDownloadService Instance { get; private set; }

	private void Awake()
	{
		Instance = this;
	}

	private void OnDestroy()
	{
		Instance = null;
	}

	public void Add(HttpDownloader downloader, bool disposeAfterCallback, Action<HttpDownloader, object> callback, object param)
	{
		RequestData requestData = new RequestData();
		requestData.downloader = downloader;
		requestData.callback = callback;
		requestData.parameter = param;
		requestData.disposeAfterCallback = disposeAfterCallback;
		m_Request.Enqueue(requestData);
	}

	public void Request(Uri uri, Stream outStream, bool disposeAfterCallback, Action<HttpDownloader, object> callback, object param)
	{
		HttpDownloader httpDownloader = new HttpDownloader();
		httpDownloader.SetRequest(uri, outStream);
		Add(httpDownloader, disposeAfterCallback, callback, param);
	}

	private void Update()
	{
		if (m_Cur == null && m_Request.Count > 0)
		{
			m_Cur = m_Request.Dequeue();
			m_Thread = new Thread(m_Cur.downloader.Execute);
			m_Thread.Start();
		}
		else
		{
			if (m_Cur == null || m_Thread == null || !m_Thread.ThreadState.HasFlag(ThreadState.Stopped))
			{
				return;
			}
			if (Utility.IsSafeDelegate(m_Cur.callback))
			{
				try
				{
					m_Cur.callback(m_Cur.downloader, m_Cur.parameter);
				}
				catch (Exception e)
				{
					Logger.LogException(e);
				}
				finally
				{
					if (m_Cur.disposeAfterCallback)
					{
						m_Cur.downloader.outStream.Dispose();
					}
				}
			}
			m_Thread = null;
			m_Cur = null;
		}
	}
}
