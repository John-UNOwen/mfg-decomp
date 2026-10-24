using System.Runtime.InteropServices;

namespace Scope;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct InputAppDefinedData
{
	public int type;

	public ulong vtable_pointer;

	public uint Param1;

	public uint Param2;

	public uint LocalTime;

	private const int CLIENT_SIGNAL_KIND_SYSTEM_MESSAGE = 1;

	private const int CLIENT_SIGNAL_PARAM_CLIENT_CLOSE_REQUEST = 1;

	public bool IsAppTerminateConfirmRequest
	{
		get
		{
			if (Param1 == 1)
			{
				return Param2 == 1;
			}
			return false;
		}
	}
}
