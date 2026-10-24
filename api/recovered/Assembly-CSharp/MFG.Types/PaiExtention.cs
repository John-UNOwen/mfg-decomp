using System;
using System.Runtime.CompilerServices;

namespace MFG.Types;

public static class PaiExtention
{
	public static Pai ToNormal(this Pai pai)
	{
		if (pai.ToString().Contains("R") && Enum.TryParse<Pai>(pai.ToString().Substring(0, 2), out var result))
		{
			return result;
		}
		return pai;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static HaiID ToHaiID(this Pai _pai)
	{
		Pai num = _pai.ToNormal();
		int num2 = (int)num / 10;
		int num3 = (int)num % 10;
		return (HaiID)(num2 * 9 + (num3 - 1));
	}

	public static Pai HaiIDToPai(this HaiID _haiID)
	{
		if (!_haiID.IsValid())
		{
			return Pai.NONE;
		}
		SuID su = _haiID.GetSu();
		HaiMarkType mark = _haiID.GetMark();
		return (Pai)(su + 1 + (int)mark * 10);
	}
}
