using System;
using System.Runtime.CompilerServices;

namespace MFG.Types;

public static class HaiIDExtensions
{
	public static HaiID ToNormal(this HaiID haiID)
	{
		if (haiID.ToString().Contains("R") && Enum.TryParse<HaiID>(haiID.ToString().Substring(0, 2), out var result))
		{
			return result;
		}
		return haiID;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsManzu(this HaiID haiID)
	{
		if (HaiID.M1 <= haiID)
		{
			return haiID <= HaiID.M9;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsSouzu(this HaiID haiID)
	{
		if (HaiID.S1 <= haiID)
		{
			return haiID <= HaiID.S9;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsPinzu(this HaiID haiID)
	{
		if (HaiID.P1 <= haiID)
		{
			return haiID <= HaiID.P9;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsJihai(this HaiID haiID)
	{
		if (HaiID.J1 <= haiID)
		{
			return haiID <= HaiID.J7;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsSuhai(this HaiID haiID)
	{
		if (HaiID.M1 <= haiID)
		{
			return haiID <= HaiID.P9;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsRoutou(this HaiID haiID)
	{
		if (haiID != 0 && haiID != HaiID.M9 && haiID != HaiID.S1 && haiID != HaiID.S9 && haiID != HaiID.P1)
		{
			return haiID == HaiID.P9;
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsYaochu(this HaiID haiID)
	{
		if (!haiID.IsRoutou())
		{
			return haiID.IsJihai();
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsKazehai(this HaiID haiID)
	{
		if (haiID != HaiID.J1 && haiID != HaiID.J2 && haiID != HaiID.J3)
		{
			return haiID == HaiID.J4;
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsSangen(this HaiID haiID)
	{
		if (haiID != HaiID.J5 && haiID != HaiID.J6)
		{
			return haiID == HaiID.J7;
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsRyuiso(this HaiID haiID)
	{
		if (haiID != HaiID.S2 && haiID != HaiID.S3 && haiID != HaiID.S4 && haiID != HaiID.S6 && haiID != HaiID.S8)
		{
			return haiID == HaiID.J6;
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static HaiMarkType GetMark(this HaiID haiID)
	{
		return (HaiMarkType)((int)haiID / 9);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static SuID GetSu(this HaiID haiID)
	{
		return (SuID)((int)haiID % 9);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsMark(this HaiID haiID, HaiMarkType mark)
	{
		return haiID.GetMark() == mark;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsValid(this HaiID haiID)
	{
		if (HaiID.M1 <= haiID)
		{
			return haiID <= HaiID.Max;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsValidSu(this SuID SuID)
	{
		if (SuID.SU1 <= SuID)
		{
			return SuID <= SuID.SU9;
		}
		return false;
	}

	public static bool IsAvailable(this HaiID haiID)
	{
		if (!haiID.IsValid())
		{
			return false;
		}
		if (GameDataManager.Mahjong.TakuType == TakuType.Nima)
		{
			if ((HaiID.M1 <= haiID && haiID <= HaiID.M9) || haiID == HaiID.J3 || haiID == HaiID.J4)
			{
				return false;
			}
		}
		else if (GameDataManager.Mahjong.TakuType == TakuType.Sanma)
		{
			if (GameDataManager.Mahjong.IsJihaiUnusedSanma)
			{
				if (HaiID.J1 <= haiID && haiID <= HaiID.J7)
				{
					return false;
				}
			}
			else if (HaiID.M2 <= haiID && haiID <= HaiID.M8)
			{
				return false;
			}
		}
		return true;
	}
}
