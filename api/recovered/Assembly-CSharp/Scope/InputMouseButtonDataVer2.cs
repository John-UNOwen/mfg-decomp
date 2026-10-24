using System.Runtime.InteropServices;

namespace Scope;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct InputMouseButtonDataVer2
{
	public int type;

	public ulong vtable_pointer;

	public byte Button;

	public byte Action;

	public byte EventFrom;

	public byte Padding;

	public uint LocalTime;
}
