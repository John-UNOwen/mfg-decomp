using System.Runtime.InteropServices;

namespace KAMUNITY;

public static class AvsEa3CoinTypes
{
	public enum EA3_EACOIN_ERROR
	{
		PROCESSING = 1,
		ECINVALID = -1,
		SLOTINUSE = -2,
		CARDINUSE = -3,
		NOTCHECKIN = -4,
		NOTECCARD = -5,
		AUTHACCOUNT = -6,
		NOACCOUNT = -7,
		SHORTBALANCE = -8,
		CHANGEACCOUNT = -9,
		UNKNOWNBALANCE = -10,
		NOARTICLE = -11,
		NOSLOT = -12,
		COMM = -13,
		NOSUCHPATH = -14,
		DISABLED = -15,
		UNEXPECTED = -16,
		MAINTENANCE = -17,
		AUTHAGAIN = -18,
		CHECKINAGAIN = -19,
		TOOBIG = -20,
		LOCATION = -21,
		ABORTED = -22,
		INVAL = -23,
		PLEXPIRED = -24,
		PLNOTHELD = -25,
		EXOVERUSE = -26,
		NOTQUICKECCARD = -27
	}

	public enum REPORT_TYPE
	{
		RESET,
		RELOAD_OPTION
	}

	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	public struct ea3_eacoin_log_last7days
	{
		public byte processing;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 23)]
		private char[] _sumdate;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 23)]
		private char[] _sumfrom;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 23)]
		private char[] _sumto;

		public int today;

		public int average;

		public int total;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]
		public int[] summary;

		public string sumdate => new string(_sumdate).TrimEnd('\0');

		public string sumfrom => new string(_sumfrom).TrimEnd('\0');

		public string sumto => new string(_sumto).TrimEnd('\0');
	}

	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	public struct ea3_eacoin_log_last52weeks
	{
		public byte processing;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 23)]
		private char[] _sumdate;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 23)]
		private char[] _sumfrom;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 23)]
		private char[] _sumto;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 52)]
		public int[] summary;

		public string sumdate => new string(_sumdate).TrimEnd('\0');

		public string sumfrom => new string(_sumfrom).TrimEnd('\0');

		public string sumto => new string(_sumto).TrimEnd('\0');
	}

	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	public struct ea3_eacoin_log_eachday
	{
		public byte processing;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 23)]
		private char[] _sumdate;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 23)]
		private char[] _sumfrom;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 23)]
		private char[] _sumto;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]
		public int[] summary;

		public string sumdate => new string(_sumdate).TrimEnd('\0');

		public string sumfrom => new string(_sumfrom).TrimEnd('\0');

		public string sumto => new string(_sumto).TrimEnd('\0');
	}

	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	public struct ea3_eacoin_log_eachhour
	{
		public byte processing;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 23)]
		private char[] _sumdate;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 23)]
		private char[] _sumfrom;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 23)]
		private char[] _sumto;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 24)]
		public int[] summary;

		public string sumdate => new string(_sumdate).TrimEnd('\0');

		public string sumfrom => new string(_sumfrom).TrimEnd('\0');

		public string sumto => new string(_sumto).TrimEnd('\0');
	}

	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	public struct ea3_eacoin_log_detail_item
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 23)]
		private char[] _date;

		public int consume;

		public int service;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
		private char[] _cardtype;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 41)]
		private char[] _cardno;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
		private char[] _title;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
		private char[] _systemid;

		public string date => new string(_date).TrimEnd('\0');

		public string cardtype => new string(_cardtype).TrimEnd('\0');

		public string cardno => new string(_cardno).TrimEnd('\0');

		public string title => new string(_title).TrimEnd('\0');

		public string systemid => new string(_systemid).TrimEnd('\0');
	}

	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	public struct ea3_eacoin_log_detail
	{
		public byte processing;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 23)]
		private char[] _sumdate;

		public int balance;

		public int detail_size;

		public int detail_len;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
		public ea3_eacoin_log_detail_item[] detail;

		public string sumdate => new string(_sumdate).TrimEnd('\0');
	}

	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	public struct ea3_eacoin_log_monthly_item
	{
		public int year;

		public int month;

		public int mdays;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 31)]
		public int[] summary;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	public struct ea3_eacoin_log_lastmonths
	{
		public byte processing;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 23)]
		private char[] _sumdate;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 23)]
		private char[] _sumfrom;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 23)]
		private char[] _sumto;

		public int monthly_size;

		public int monthly_len;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
		public ea3_eacoin_log_monthly_item[] monthly;

		public string sumdate => new string(_sumdate).TrimEnd('\0');

		public string sumfrom => new string(_sumfrom).TrimEnd('\0');

		public string sumto => new string(_sumto).TrimEnd('\0');
	}

	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	public struct ea3_eacoin_logs
	{
		public ea3_eacoin_log_last7days last7days;

		public ea3_eacoin_log_last52weeks last52weeks;

		public ea3_eacoin_log_eachday eachday;

		public ea3_eacoin_log_eachhour eachhour;

		public ea3_eacoin_log_detail detail;

		public ea3_eacoin_log_lastmonths lastmonths;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	public struct ea3_eacoin_pricelist_info
	{
		public uint recvtime;

		public uint basetime;

		public int expire;

		public int refresh;

		public int revision;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 49)]
		private char[] _hash;

		public string hash => new string(_hash).TrimEnd('\0');
	}

	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	public struct ea3_eacoin_pricelist_showcase
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 9)]
		private char[] _scid;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 121)]
		private char[] _name;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 9)]
		private char[] _areacode;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 121)]
		private char[] _areaname;

		public string scid => new string(_scid).TrimEnd('\0');

		public string name => new string(_name).TrimEnd('\0');

		public string areacode => new string(_areacode).TrimEnd('\0');

		public string areaname => new string(_areaname).TrimEnd('\0');
	}

	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	public struct ea3_eacoin_pricelist_campaign
	{
		private int cpno;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 121)]
		private char[] _name;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 361)]
		private char[] _desc;

		private uint stime;

		private uint etime;

		private int fhour;

		private int thour;

		public string name => new string(_name).TrimEnd('\0');

		public string desc => new string(_desc).TrimEnd('\0');
	}

	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	public struct ea3_eacoin_pricelist_period
	{
		public int after;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	public struct ea3_eacoin_pricelist_article
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
		private char[] _atid;

		public int mask;

		public ushort price;

		public ushort value;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 121)]
		private char[] _name;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
		private char[] _type;

		public int seq;

		public int cpno;

		public string atid => new string(_atid).TrimEnd('\0');

		public string name => new string(_atid).TrimEnd('\0');

		public string type => new string(_atid).TrimEnd('\0');
	}

	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	public struct ea3_eacoin_pricelist_showcaseid_array
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 90)]
		public char[][] scid;

		public int nr_scid;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	public struct ea3_eacoin_pricelist_campaignno_array
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 31)]
		public int[] cpno;

		public int nr_cpno;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	public struct ea3_eacoin_pricelist_period_array
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 31)]
		private ea3_eacoin_pricelist_period[] period;

		private int nr_period;
	}
}
