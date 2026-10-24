using System;

namespace KAMUNITY;

public abstract class XrpcMethodBase<T> : XrpcMethodInternal
{
	public Action<XrpcBase, T, object> EndCallback { get; set; }

	public object EndParam { get; set; }

	public XrpcMethodBase()
	{
	}

	public XrpcMethodBase(Action<XrpcBase, T, object> _endCallback, object _endParam)
	{
		SetCallback(_endCallback, _endParam);
	}

	public void SetCallback(Action<XrpcBase, T, object> _endCallback, object _endParam)
	{
		EndCallback = _endCallback;
		EndParam = _endParam;
	}

	protected override void ResponseHandler(XrpcBase xrpc, object cb_param)
	{
		T arg;
		if (!xrpc.Status.HasError())
		{
			try
			{
				arg = ParseHandler(xrpc, cb_param);
			}
			catch (Exception e)
			{
				Logger.LogException(e);
				AvsEa3XrpcTypes.XRPC_STATUS_DATA status = new AvsEa3XrpcTypes.XRPC_STATUS_DATA(-16, 900, 0, 0);
				xrpc.Status = status;
				arg = default(T);
			}
		}
		else
		{
			arg = default(T);
		}
		if (Utility.IsSafeDelegate(EndCallback))
		{
			try
			{
				EndCallback(xrpc, arg, EndParam);
				EndCallback = null;
			}
			catch (Exception e2)
			{
				Logger.LogException(e2);
			}
		}
	}

	protected abstract T ParseHandler(XrpcBase xrpc, object param);
}
public abstract class XrpcMethodBase : XrpcMethodInternal
{
	public Action<XrpcBase, object> EndCallback { get; set; }

	public object EndParam { get; set; }

	public XrpcMethodBase()
	{
	}

	public XrpcMethodBase(Action<XrpcBase, object> _endCallback, object _endParam)
	{
		SetCallback(_endCallback, _endParam);
	}

	public void SetCallback(Action<XrpcBase, object> _endCallback, object _endParam)
	{
		EndCallback = _endCallback;
		EndParam = _endParam;
	}

	protected override void ResponseHandler(XrpcBase xrpc, object cb_param)
	{
		if (!xrpc.Status.HasError())
		{
			try
			{
				ParseHandler(xrpc, cb_param);
			}
			catch (Exception e)
			{
				Logger.LogException(e);
				AvsEa3XrpcTypes.XRPC_STATUS_DATA status = new AvsEa3XrpcTypes.XRPC_STATUS_DATA(-16, 900, 0, 0);
				xrpc.Status = status;
			}
		}
		if (Utility.IsSafeDelegate(EndCallback))
		{
			try
			{
				EndCallback(xrpc, EndParam);
				EndCallback = null;
			}
			catch (Exception e2)
			{
				Logger.LogException(e2);
			}
		}
	}

	protected abstract void ParseHandler(XrpcBase xrpc, object param);
}
