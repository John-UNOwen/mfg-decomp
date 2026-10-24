using System;
using System.Runtime.InteropServices;

public class ScopeLib
{
	public struct ScopeCloudGamingStatusInformationParam
	{
		[MarshalAs(UnmanagedType.U4)]
		public uint Version;

		[MarshalAs(UnmanagedType.U4)]
		public uint LastCheckCounter;

		[MarshalAs(UnmanagedType.U8)]
		public ulong LastCheckRawTime;

		[MarshalAs(UnmanagedType.U8)]
		public ulong LastAccessRawTime;

		[MarshalAs(UnmanagedType.U1)]
		public byte ControlConnecting;

		[MarshalAs(UnmanagedType.U1)]
		public byte VideoConnecting;

		[MarshalAs(UnmanagedType.U1)]
		public byte Reserved1;

		[MarshalAs(UnmanagedType.U1)]
		public byte Reserved2;
	}

	[DllImport("cloudstrap.exe")]
	public static extern void cstrap_terminate_scope();

	[DllImport("ScopeCloudGaming.dll")]
	public static extern int ScopeCloudGamingTermStart();

	[DllImport("ScopeCloudGaming.dll")]
	public static extern int ScopeCloudGamingTermCheck();

	[DllImport("ScopeCloudGaming.dll")]
	public static extern int ScopeCloudGamingGetStatusInformation(IntPtr pParam);
}
