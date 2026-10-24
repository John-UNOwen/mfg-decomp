using System.Runtime.InteropServices;

namespace Scope;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct InputClose
{
	public int type;

	public ulong vtable_pointer;

	public uint Reason;
}
