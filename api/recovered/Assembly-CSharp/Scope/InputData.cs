using System.Runtime.InteropServices;

namespace Scope;

[StructLayout(LayoutKind.Explicit)]
public struct InputData
{
	[FieldOffset(0)]
	public int type;

	[FieldOffset(0)]
	public InputMouseMoveDataVer2 mouseMoveDataVer2;

	[FieldOffset(0)]
	public InputMouseButtonDataVer2 mouseButtonDataVer2;

	[FieldOffset(0)]
	public InputKeyboardDataVer2 keyboardDataVer2;

	[FieldOffset(0)]
	public InputTouchDataVer2 touchDataVer2;

	[FieldOffset(0)]
	public InputClose closeData;

	[FieldOffset(0)]
	public InputAppDefinedData appDefinedData;

	[FieldOffset(0)]
	public InputDataMax dummy;
}
