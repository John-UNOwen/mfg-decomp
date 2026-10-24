using System.Runtime.InteropServices;

namespace Scope;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct InputMouseMoveDataVer2
{
	public int type;

	public ulong vtable_pointer;

	public int PointX;

	public int PointY;

	public uint LocalTime;
}
