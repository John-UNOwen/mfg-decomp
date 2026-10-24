using System;
using System.Collections.Generic;
using UnityEngine;

namespace KAMUNITY;

public class XrpcService : MonoBehaviour
{
	private class QueueElemet
	{
		public XrpcBase xrpc;

		public Action<XrpcBase, object> callback;

		public object cb_param;

		public int retry_count;

		public QueueElemet(XrpcBase _xrpc, Action<XrpcBase, object> _callback, object _cb_param, int _retry_count)
		{
			xrpc = _xrpc;
			callback = _callback;
			cb_param = _cb_param;
			retry_count = _retry_count;
		}

		public void CallCallback()
		{
			if (Utility.IsSafeDelegate(callback))
			{
				try
				{
					callback(xrpc, cb_param);
				}
				catch (Exception e)
				{
					Logger.LogException(e);
				}
			}
			callback = null;
		}
	}

	private enum Step
	{
		Idle,
		RequestWait,
		RetryWait
	}

	public const int XRPC_DEFAULT_SIZE = 4096;

	public const int XRPC_DEFAULT_RETRY_COUNT = 3;

	public const float XRPC_RETRY_INTERVAL_BASE = 5f;

	public const float XRPC_RETRY_INTERVAL_RAND = 3f;

	private Queue<QueueElemet> m_RequestQueue = new Queue<QueueElemet>();

	private QueueElemet m_RequestCur;

	private int m_RetryCounter;

	private float m_RetryWaitTime;

	private Step m_Step;

	public static XrpcService Instance { get; private set; }

	private void Awake()
	{
		Instance = this;
	}

	private void OnDestroy()
	{
		Instance = null;
	}

	private void Update()
	{
		switch (m_Step)
		{
		case Step.Idle:
			if (m_RequestQueue.Count > 0)
			{
				m_RequestCur = m_RequestQueue.Dequeue();
				m_RequestCur.xrpc.StartRequest();
				m_RetryCounter = 0;
				m_RetryWaitTime = 0f;
				m_Step = Step.RequestWait;
			}
			break;
		case Step.RequestWait:
			if (m_RequestCur.xrpc.UpdateRequest())
			{
				break;
			}
			if (m_RequestCur.xrpc.Status.HasError())
			{
				AvsEa3XrpcTypes.XRPC_STATUS status = m_RequestCur.xrpc.Status.GetStatus();
				bool flag = false;
				if (status == AvsEa3XrpcTypes.XRPC_STATUS.EHTTP || status == AvsEa3XrpcTypes.XRPC_STATUS.EINPROGRESS)
				{
					flag = true;
				}
				m_RetryCounter++;
				if (flag && m_RetryCounter <= m_RequestCur.retry_count)
				{
					float num = 5f + UnityEngine.Random.Range(0f, 3f) + (float)m_RetryCounter * 2f;
					m_RetryWaitTime = Time.realtimeSinceStartup + num;
					Logger.LogFormat("Retry{0}, wait while {1} sec", m_RetryCounter, num);
					m_Step = Step.RetryWait;
				}
				else
				{
					m_RequestCur.CallCallback();
					m_RequestCur = null;
					m_Step = Step.Idle;
				}
			}
			else
			{
				m_RequestCur.CallCallback();
				m_RequestCur = null;
				m_Step = Step.Idle;
			}
			break;
		case Step.RetryWait:
			if (!(m_RetryWaitTime > Time.realtimeSinceStartup))
			{
				m_RequestCur.xrpc.StartRequest();
				m_Step = Step.RequestWait;
			}
			break;
		}
	}

	public void Add(XrpcBase xrpc, Action<XrpcBase, object> callback, object cb_param, int retry_count)
	{
		m_RequestQueue.Enqueue(new QueueElemet(xrpc, callback, cb_param, retry_count));
	}

	public XrpcBase Request(string module_name, string method_name, string request_xml, Action<XrpcBase, object> callback = null, object cb_param = null, int xrpc_size = 4096, int retry_count = 3)
	{
		XrpcBase xrpcBase = new XrpcBase();
		xrpcBase.SetRequest(module_name, method_name, request_xml, xrpc_size);
		Add(xrpcBase, callback, cb_param, retry_count);
		return xrpcBase;
	}
}
