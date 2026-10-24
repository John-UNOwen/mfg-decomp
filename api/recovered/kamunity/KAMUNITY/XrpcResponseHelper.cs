using System.Collections;
using UnityEngine;

namespace KAMUNITY;

public class XrpcResponseHelper<T>
{
	public AvsEa3XrpcTypes.XRPC_STATUS_DATA status;

	public T result;

	public bool isEnd { get; private set; }

	public bool isError
	{
		get
		{
			if (isEnd)
			{
				return status.HasError();
			}
			return false;
		}
	}

	public void Callback(XrpcBase xrpc, T response, object param)
	{
		status = xrpc.Status;
		if (!status.HasError())
		{
			result = response;
		}
		isEnd = true;
	}

	public IEnumerator CoWaitForXrpcFinish()
	{
		yield return new WaitWhile(() => !isEnd);
	}
}
