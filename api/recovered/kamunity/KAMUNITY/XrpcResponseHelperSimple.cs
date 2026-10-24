using System.Collections;
using UnityEngine;

namespace KAMUNITY;

public class XrpcResponseHelperSimple
{
	public AvsEa3XrpcTypes.XRPC_STATUS_DATA status;

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

	public void Callback(XrpcBase xrpc, object param)
	{
		status = xrpc.Status;
		isEnd = true;
	}

	public IEnumerator CoWaitForXrpcFinish()
	{
		yield return new WaitWhile(() => !isEnd);
	}
}
