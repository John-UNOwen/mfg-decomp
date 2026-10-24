using System;
using System.Runtime.InteropServices;

namespace KAMUNITY;

[KamunityDllApiClass]
public static class Bootstrap
{
	[KamunityDllApiClass]
	internal class InternalMethod
	{
		[StructLayout(LayoutKind.Sequential, Pack = 8)]
		public struct bt_ikey_status_item
		{
			public uint count_found;

			public byte nr_token;

			public byte f_disable;

			public byte available;

			public byte padding;

			public uint not_before;

			public uint not_after;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 8)]
		public struct bt_ikey_status
		{
			public bt_ikey_status_item account;

			public bt_ikey_status_item license;
		}

		public delegate void FUNC_GetIkeyStatus(out bt_ikey_status status);

		public delegate void FUNC_FcheckInit([In] int do_bg, [In] BT_FCHECK_MODE do_mode);

		public delegate BT_FCHECK FUNC_FcheckMain(out bt_fcheck_stat st);

		public delegate BT_FCHECK FUNC_FcheckFinish();

		public static FUNC_GetIkeyStatus GetIkeyStatus { get; private set; }

		public static FUNC_FcheckInit FcheckInit { get; private set; }

		public static FUNC_FcheckMain FcheckMain { get; private set; }

		public static FUNC_FcheckFinish FcheckFinish { get; private set; }
	}

	public class IKeyStatusItem
	{
		public uint count_found;

		public byte nr_token;

		public bool f_disable;

		public bool available;

		public DateTime not_before;

		public DateTime not_after;
	}

	public class IKeyStatus
	{
		public IKeyStatusItem account = new IKeyStatusItem();

		public IKeyStatusItem license = new IKeyStatusItem();
	}

	public enum BT_FCHECK_MODE
	{
		FULL,
		SIMPLE,
		FILEINFO
	}

	public enum BT_FCHECK
	{
		RUNNING = 1,
		ERROR_NONE = 0,
		ERROR_UNINITIALIZE = -1,
		ERROR_MISC = -2,
		ERROR_ALLFILE = -3,
		ERROR_LICENSE = -4,
		ERROR_COMPUTE = -5,
		ERROR_SUM = -6,
		ERROR_FILEPATH = -7,
		ERROR_FILEPATH_NOT_FOUND = -8,
		ERROR_MEDIA = -9
	}

	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	public struct bt_fcheck_stat
	{
		public ulong total_size;

		public ulong checked_size;

		public uint total_files;

		public uint checked_files;

		public uint broken_files;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
		private char[] _filename;

		private IntPtr p;

		public string filename => new string(_filename);
	}

	private static IKeyStatusItem ConvertFromInnerIkeyStatus(InternalMethod.bt_ikey_status_item _item)
	{
		return new IKeyStatusItem
		{
			count_found = _item.count_found,
			nr_token = _item.nr_token,
			f_disable = (_item.f_disable != 0),
			available = (_item.available != 0),
			not_before = Utility.Epoch2DateTime(_item.not_before),
			not_after = Utility.Epoch2DateTime(_item.not_after)
		};
	}

	public static void GetIkeyStatus(out IKeyStatus status)
	{
		InternalMethod.bt_ikey_status status2 = default(InternalMethod.bt_ikey_status);
		InternalMethod.GetIkeyStatus(out status2);
		status = new IKeyStatus();
		status.account = ConvertFromInnerIkeyStatus(status2.account);
		status.license = ConvertFromInnerIkeyStatus(status2.license);
	}

	[Obsolete("use FcheckInit")]
	public static void BootstrapFcheckInit([In] int do_bg, [In] BT_FCHECK_MODE do_mode)
	{
		FcheckInit(do_bg, do_mode);
	}

	public static void FcheckInit(int do_bg, [In] BT_FCHECK_MODE do_mode)
	{
		InternalMethod.FcheckInit(do_bg, do_mode);
	}

	public static BT_FCHECK FcheckMain(out bt_fcheck_stat st)
	{
		return InternalMethod.FcheckMain(out st);
	}

	public static BT_FCHECK FcheckFinish()
	{
		return InternalMethod.FcheckFinish();
	}
}
