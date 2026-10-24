using System.Runtime.CompilerServices;

namespace MFG.Types;

public static class HaiUtility
{
	private static readonly HaiID[] s_yaochuHais = new HaiID[13]
	{
		HaiID.M1,
		HaiID.M9,
		HaiID.S1,
		HaiID.S9,
		HaiID.P1,
		HaiID.P9,
		HaiID.J1,
		HaiID.J2,
		HaiID.J3,
		HaiID.J4,
		HaiID.J5,
		HaiID.J6,
		HaiID.J7
	};

	private static HaiID[] s_routouHais = new HaiID[6]
	{
		HaiID.M1,
		HaiID.M9,
		HaiID.S1,
		HaiID.S9,
		HaiID.P1,
		HaiID.P9
	};

	private static HaiID[] s_sangenHais = new HaiID[3]
	{
		HaiID.J5,
		HaiID.J6,
		HaiID.J7
	};

	private static HaiID[] s_ryuisoHais = new HaiID[6]
	{
		HaiID.S2,
		HaiID.S3,
		HaiID.S4,
		HaiID.S6,
		HaiID.S8,
		HaiID.J6
	};

	private static HaiID[] s_kazeHais = new HaiID[4]
	{
		HaiID.J1,
		HaiID.J2,
		HaiID.J3,
		HaiID.J4
	};

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static (HaiID, HaiID) GetRange(HaiMarkType markType)
	{
		return markType switch
		{
			HaiMarkType.Manzu => (HaiID.M1, HaiID.M9), 
			HaiMarkType.Souzu => (HaiID.S1, HaiID.S9), 
			HaiMarkType.Pinzu => (HaiID.P1, HaiID.P9), 
			HaiMarkType.Jihai => (HaiID.J1, HaiID.J7), 
			_ => (HaiID.M1, HaiID.M1), 
		};
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static HaiID ToHaiID(HaiMarkType mark, SuID suhaiID)
	{
		return (HaiID)((int)mark * 9 + suhaiID);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static HaiID[] YaochuHais()
	{
		return s_yaochuHais;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static HaiID[] RoutouHais()
	{
		return s_routouHais;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static HaiID[] SangenHais()
	{
		return s_sangenHais;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static HaiID[] RyuisoHais()
	{
		return s_ryuisoHais;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static HaiID[] KazeHais()
	{
		return s_kazeHais;
	}
}
