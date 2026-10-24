using System;
using System.Runtime.InteropServices;

namespace KAMUNITY.Network;

[KamunityDllApiClass]
public static class XrpcExApi
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.U1)]
	public delegate bool XrpcMethodCallbackInit(IntPtr pkt, IntPtr ap);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.U1)]
	public delegate bool XrpcMethodCallbackSender(IntPtr pkt, IntPtr node);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.U1)]
	public delegate bool XrpcMethodCallbackReceiver(IntPtr pkt, IntPtr node);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void XrpcCallbackExit(IntPtr pkt, AvsEa3XrpcTypes.XRPC_STATUS_DATA status, IntPtr cbexit_data);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	public delegate int Ea3XrpcAddModuleExDelegate([In] string moduleName, [In] string serviceName, [In] string[] methodNames, [In] int methodNamesLength, [In][MarshalAs(UnmanagedType.FunctionPtr)] XrpcMethodCallbackInit cb_init, [In][MarshalAs(UnmanagedType.FunctionPtr)] XrpcMethodCallbackSender cb_send, [In][MarshalAs(UnmanagedType.FunctionPtr)] XrpcMethodCallbackReceiver cb_recv);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void Ea3XrpcRemoveModuleExDelegate([In] string moduleName);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate IntPtr Ea3XrpcNewExDelegate([In] int buf_size, [In] string encoding, [In] uint nr_meta, [In] uint sz_meta);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void Ea3XrpcDestroyExDelegate([In] IntPtr xrpcObjectHandle);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate int Ea3XrpcApplyExDelegate([In] IntPtr xrpcObjectHandle, [In] string name, [In] IntPtr pkt, [In] XrpcCallbackExit cb_exit, [In] IntPtr cb_exit_data);

	public static Ea3XrpcAddModuleExDelegate Ea3XrpcAddModuleEx { get; private set; }

	public static Ea3XrpcRemoveModuleExDelegate Ea3XrpcRemoveModuleEx { get; private set; }

	public static Ea3XrpcNewExDelegate Ea3XrpcNewEx { get; private set; }

	public static Ea3XrpcDestroyExDelegate Ea3XrpcDestroyEx { get; private set; }

	public static Ea3XrpcApplyExDelegate Ea3XrpcApplyEx { get; private set; }
}
