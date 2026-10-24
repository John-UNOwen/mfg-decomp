using System.Runtime.InteropServices;

namespace Scope;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct InputTouchDataVer2
{
	public int type;

	public ulong vtable_pointer;

	public byte MultiTouchIndex;

	public byte Action;

	public ushort PointX;

	public ushort PointY;

	public byte OptionType;

	public byte State;

	public uint LocalTime;

	public byte MultiTouchNum;

	public byte MultiTouchMax;
}
