using System;

namespace MFG.Taikyoku.Command;

[Flags]
public enum SELECTABLE_TYPE_FLAG
{
	NONE = 1,
	PON = 2,
	CHI = 4,
	KAN = 8,
	TSUMOAGARI = 0x40,
	RON = 0x80,
	KYUSYU = 0x100,
	REACH = 0x200,
	SUTE = 0x400,
	FIREREACH = 0x800,
	OPENREACH = 0x1000,
	ATOMICREACH = 0x2000
}
