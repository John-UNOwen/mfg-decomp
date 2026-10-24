using System;

namespace KAMUNITY;

[KamunityDllApiClass]
public static class AvsEa3Xrpc
{
	[KamunityDllApiClass]
	internal static class InternalMethod
	{
		public delegate void FUNC_Ea3XrpcAddModule(string name, string serv_name, string[] method_names, int method_count);

		public static FUNC_Ea3XrpcAddModule Ea3XrpcAddModule { get; private set; }
	}

	public delegate void FUNC_Ea3XrpcRemoveModule(string name);

	public delegate IntPtr FUNC_Ea3XrpcApply(string name, string xml, int size);

	public delegate int FUNC_Ea3XrpcIsFinish(IntPtr request);

	public delegate int FUNC_Ea3XrpcGetResponseSize(IntPtr request);

	public delegate IntPtr FUNC_Ea3XrpcGetResponseData(IntPtr request);

	public delegate void FUNC_Ea3XrpcGetStatus(IntPtr request, out AvsEa3XrpcTypes.XRPC_STATUS_DATA status);

	public delegate void FUNC_Ea3XrpcDestroy(IntPtr request);

	public static FUNC_Ea3XrpcRemoveModule Ea3XrpcRemoveModule { get; private set; }

	public static FUNC_Ea3XrpcApply Ea3XrpcApply { get; private set; }

	public static FUNC_Ea3XrpcIsFinish Ea3XrpcIsFinish { get; private set; }

	public static FUNC_Ea3XrpcGetResponseSize Ea3XrpcGetResponseSize { get; private set; }

	public static FUNC_Ea3XrpcGetResponseData Ea3XrpcGetResponseData { get; private set; }

	public static FUNC_Ea3XrpcGetStatus Ea3XrpcGetStatus { get; private set; }

	public static FUNC_Ea3XrpcDestroy Ea3XrpcDestroy { get; private set; }

	public static void Ea3XrpcAddModule(string name, string serv_name, string[] method_names)
	{
		InternalMethod.Ea3XrpcAddModule(name, serv_name, method_names, method_names.Length);
	}
}
