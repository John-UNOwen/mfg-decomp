using System.Runtime.CompilerServices;

namespace MFG.Types;

public static class HaiMarkTypeExtention
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int ToHaiID(this HaiMarkType type, SuID suID)
	{
		return (int)((int)type * 9 + suID);
	}
}
