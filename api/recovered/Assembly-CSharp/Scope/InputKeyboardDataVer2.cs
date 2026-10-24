using System.Runtime.InteropServices;

namespace Scope;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct InputKeyboardDataVer2
{
	public int type;

	public ulong vtable_pointer;

	public byte Action;

	public byte Padding;

	public ushort Key;

	public uint LocalTime;
}
