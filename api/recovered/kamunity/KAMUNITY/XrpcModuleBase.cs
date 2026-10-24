using System;

namespace KAMUNITY;

public static class XrpcModuleBase
{
	public class UserResponseCallbackDataSimple
	{
		public Action<XrpcBase, object> callback;

		public object param;

		public UserResponseCallbackDataSimple(Action<XrpcBase, object> _callback, object _param)
		{
			callback = _callback;
			param = _param;
		}
	}

	public class UserResponseCallbackData<T>
	{
		public Action<XrpcBase, T, object> callback;

		public object param;

		public UserResponseCallbackData(Action<XrpcBase, T, object> _callback, object _param)
		{
			callback = _callback;
			param = _param;
		}
	}

	public static void ResponseSimple(XrpcBase xrpc, object cb_param)
	{
		UserResponseCallbackDataSimple userResponseCallbackDataSimple = (UserResponseCallbackDataSimple)cb_param;
		if (Utility.IsSafeDelegate(userResponseCallbackDataSimple.callback))
		{
			try
			{
				userResponseCallbackDataSimple.callback(xrpc, userResponseCallbackDataSimple.param);
			}
			catch (Exception e)
			{
				Logger.LogException(e);
			}
		}
	}
}
