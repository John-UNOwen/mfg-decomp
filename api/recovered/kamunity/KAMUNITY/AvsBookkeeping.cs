using System.Runtime.InteropServices;

namespace KAMUNITY;

[KamunityDllApiClass]
public static class AvsBookkeeping
{
	[KamunityDllApiClass]
	internal static class InternalMethod
	{
		public delegate void FUNC_BookkeepingGetIncomeLast7days([Out][MarshalAs(UnmanagedType.LPArray, SizeConst = 8)] int[] last_7days_income, [In] int svc);

		public delegate void FUNC_BookkeepingGetIncomeLast52weeks([Out][MarshalAs(UnmanagedType.LPArray, SizeConst = 53)] int[] last_52weeks_income, [In] int svc);

		public delegate void FUNC_BookkeepingGetIncomeEachDays([Out][MarshalAs(UnmanagedType.LPArray, SizeConst = 7)] int[] each_days_income, [In] int svc);

		public delegate void FUNC_BookkeepingGetIncomeEachHour([Out][MarshalAs(UnmanagedType.LPArray, SizeConst = 24)] int[] each_hour_income, [In] int svc);

		public static FUNC_BookkeepingGetIncomeLast7days BookkeepingGetIncomeLast7days { get; private set; }

		public static FUNC_BookkeepingGetIncomeLast52weeks BookkeepingGetIncomeLast52weeks { get; private set; }

		public static FUNC_BookkeepingGetIncomeEachDays BookkeepingGetIncomeEachDays { get; private set; }

		public static FUNC_BookkeepingGetIncomeEachHour BookkeepingGetIncomeEachHour { get; private set; }
	}

	public delegate void FUNC_BookkeepingClockIsSet([In] int set);

	public delegate void FUNC_BookkeepingClockSetDiff([In] uint diff);

	public delegate void FUNC_BookkeepingStatusSetFactory();

	public delegate void FUNC_BookkeepingDataSetFactory();

	public delegate void FUNC_BookkeepingSetInitState();

	public delegate void FUNC_BookkeepingFlush();

	public delegate void FUNC_BookkeepingSetRecordEnable(int book_keeping_enable);

	public delegate int FUNC_BookkeepingIsRecordEnable();

	public delegate void FUNC_BookkeepingGetIncomeLast7daysStat(out int total_last_7days_income, out int average_last_7days_income, [In] int svc);

	public delegate void FUNC_BookkeepingGetIncomeLast52WeeksStat(out int total_last_52weeks_income, out int average_last_52weeks_income, [In] int svc);

	public delegate void FUNC_BookkeepingGetIncomeEachDaysStat(out int total_each_days_income, out int average_each_days_income, [In] int svc);

	public delegate void FUNC_BookkeepingGetIncomeEachHourStat(out int total_each_hour_income, out int average_each_hour_income, [In] int svc);

	public delegate uint FUNC_BookkeepingGetTotalActiveTime();

	public delegate uint FUNC_BookkeepingGetTotalPlayTime();

	public delegate uint FUNC_BookkeepingGetShortestPlayTime();

	public delegate uint FUNC_BookkeepingGetLongestPlayTime();

	public delegate uint FUNC_BookkeepingGetAveragePlayTime(uint variable_bits);

	public delegate void FUNC_BookkeepingSetPlayTime(uint play_time);

	public delegate uint FUNC_BookkeepingGetSumVariable(uint variable_bits);

	public delegate uint FUNC_BookkeepingGetVariable(uint index);

	public delegate void FUNC_BookkeepingSetVariable(uint index, uint value);

	public static FUNC_BookkeepingClockIsSet BookkeepingClockIsSet { get; private set; }

	public static FUNC_BookkeepingClockSetDiff BookkeepingClockSetDiff { get; private set; }

	public static FUNC_BookkeepingStatusSetFactory BookkeepingStatusSetFactory { get; private set; }

	public static FUNC_BookkeepingDataSetFactory BookkeepingDataSetFactory { get; private set; }

	public static FUNC_BookkeepingSetInitState BookkeepingSetInitState { get; private set; }

	public static FUNC_BookkeepingFlush BookkeepingFlush { get; private set; }

	public static FUNC_BookkeepingSetRecordEnable BookkeepingSetRecordEnable { get; private set; }

	public static FUNC_BookkeepingIsRecordEnable BookkeepingIsRecordEnable { get; private set; }

	public static FUNC_BookkeepingGetIncomeLast7daysStat BookkeepingGetIncomeLast7daysStat { get; private set; }

	public static FUNC_BookkeepingGetIncomeLast52WeeksStat BookkeepingGetIncomeLast52WeeksStat { get; private set; }

	public static FUNC_BookkeepingGetIncomeEachDaysStat BookkeepingGetIncomeEachDaysStat { get; private set; }

	public static FUNC_BookkeepingGetIncomeEachHourStat BookkeepingGetIncomeEachHourStat { get; private set; }

	public static FUNC_BookkeepingGetTotalActiveTime BookkeepingGetTotalActiveTime { get; private set; }

	public static FUNC_BookkeepingGetTotalPlayTime BookkeepingGetTotalPlayTime { get; private set; }

	public static FUNC_BookkeepingGetShortestPlayTime BookkeepingGetShortestPlayTime { get; private set; }

	public static FUNC_BookkeepingGetLongestPlayTime BookkeepingGetLongestPlayTime { get; private set; }

	public static FUNC_BookkeepingGetAveragePlayTime BookkeepingGetAveragePlayTime { get; private set; }

	public static FUNC_BookkeepingSetPlayTime BookkeepingSetPlayTime { get; private set; }

	public static FUNC_BookkeepingGetSumVariable BookkeepingGetSumVariable { get; private set; }

	public static FUNC_BookkeepingGetVariable BookkeepingGetVariable { get; private set; }

	public static FUNC_BookkeepingSetVariable BookkeepingSetVariable { get; private set; }

	public static int[] BookkeepingGetIncomeLast7days(bool svc)
	{
		int[] array = new int[8];
		InternalMethod.BookkeepingGetIncomeLast7days(array, svc ? 1 : 0);
		return array;
	}

	public static int[] BookkeepingGetIncomeLast52weeks(bool svc)
	{
		int[] array = new int[53];
		InternalMethod.BookkeepingGetIncomeLast52weeks(array, svc ? 1 : 0);
		return array;
	}

	public static int[] BookkeepingGetIncomeEachDays(bool svc)
	{
		int[] array = new int[7];
		InternalMethod.BookkeepingGetIncomeEachDays(array, svc ? 1 : 0);
		return array;
	}

	public static int[] BookkeepingGetIncomeEachHour(bool svc)
	{
		int[] array = new int[24];
		InternalMethod.BookkeepingGetIncomeEachHour(array, svc ? 1 : 0);
		return array;
	}

	public static void BookkeepingAddVariable(uint index, uint value)
	{
		uint num = BookkeepingGetVariable(index);
		num += value;
		BookkeepingSetVariable(index, num);
	}
}
