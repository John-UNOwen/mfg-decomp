using System;
using System.Runtime.InteropServices;
using System.Text;

namespace KAMUNITY;

[KamunityDllApiClass]
public static class Avs
{
	[KamunityDllApiClass]
	internal static class InternalMethod
	{
		public delegate int FUNC_StdGetEnv([In] string path, [Out] IntPtr dst, int dstlen);

		public delegate int FUNC_AvsLog([In] int type, [In] string sect, [In] string message);

		public static FUNC_StdGetEnv StdGetEnv { get; private set; }

		public static FUNC_AvsLog AvsLog { get; private set; }
	}

	public delegate int FUNC_FsOpen([In] string path, [In] AvsTypes.FS_FLAG flag, AvsTypes.FS_MODE mode);

	public delegate int FUNC_FsClose([In] int fd);

	public delegate int FUNC_FsRename([In] string oldname, [In] string newname);

	public delegate int FUNC_FsFlush([In] int fd);

	public delegate int FUNC_FsWrite([In] int fd, [In] byte[] buf, int size);

	public delegate int FUNC_FsRead([In] int fd, [Out] byte[] buf, int size);

	public delegate int FUNC_FsSeek([In] int fd, [In] long offs, AvsTypes.FS_SEEK whence);

	public delegate int FUNC_FsFstat([In] int fd, out AvsTypes.AVS_FS_STAT st);

	public delegate int FUNC_FsMakedir([In] string path, [In] int mode);

	public delegate byte FUNC_FsLstat([In] string path, out AvsTypes.AVS_FS_STAT st);

	public delegate int FUNC_FsRemove([In] string path);

	public delegate void FUNC_FsCleanupDir([In] string path, [In] string[] names, [In] uint nr_names, [In] byte file_only);

	public delegate int FUNC_FsCopy([In] string sname, [In] string dname);

	public delegate int FUNC_FsStatistics();

	public delegate void FUNC_FsDumpMountpoint();

	public delegate int FUNC_FsConvertPath([Out] StringBuilder fsPath, [In] string avsPath);

	public delegate ulong FUNC_ClockGet([In] AvsTypes.AVS_CLOCK from);

	public delegate int FUNC_SntpGetAddr(out AvsTypes.avs_net_in4_addr addr);

	public delegate void ThreadCallback(string param);

	public delegate int FUNC_AvsThreadCreate([In] ThreadCallback callback, [In] uint sz_stack, [In] AvsTypes.AVS_THREAD_PRIORITY priority, [In] string parameter);

	public delegate int FUNC_AvsThreadDestroy([In] int avs_thread_id);

	public delegate int FUNC_AvsThreadJoin([In] int avs_thread_id);

	public delegate void FUNC_AvsThreadExit([In] IntPtr value);

	public delegate int FUNC_AvsThreadAttrSetPriority([In] int avs_thread_id, [In] AvsTypes.AVS_THREAD_PRIORITY priority);

	public static FUNC_FsOpen FsOpen { get; private set; }

	public static FUNC_FsClose FsClose { get; private set; }

	public static FUNC_FsRename FsRename { get; private set; }

	public static FUNC_FsFlush FsFlush { get; private set; }

	public static FUNC_FsWrite FsWrite { get; private set; }

	public static FUNC_FsRead FsRead { get; private set; }

	public static FUNC_FsSeek FsSeek { get; private set; }

	public static FUNC_FsFstat FsFstat { get; private set; }

	public static FUNC_FsMakedir FsMakedir { get; private set; }

	public static FUNC_FsLstat FsLstat { get; private set; }

	public static FUNC_FsRemove FsRemove { get; private set; }

	public static FUNC_FsCleanupDir FsCleanupDir { get; private set; }

	public static FUNC_FsCopy FsCopy { get; private set; }

	public static FUNC_FsStatistics FsStatistics { get; private set; }

	public static FUNC_FsDumpMountpoint FsDumpMountpoint { get; private set; }

	public static FUNC_FsConvertPath FsConvertPath { get; private set; }

	public static FUNC_ClockGet ClockGet { get; private set; }

	public static FUNC_SntpGetAddr SntpGetAddr { get; private set; }

	public static FUNC_AvsThreadCreate AvsThreadCreate { get; private set; }

	public static FUNC_AvsThreadDestroy AvsThreadDestroy { get; private set; }

	public static FUNC_AvsThreadJoin AvsThreadJoin { get; private set; }

	public static FUNC_AvsThreadExit AvsThreadExit { get; private set; }

	public static FUNC_AvsThreadAttrSetPriority AvsThreadAttrSetPriority { get; private set; }

	public static string StdGetEnv(string path, int buff_len = 128)
	{
		string result = "";
		using (MarshalHeap marshalHeap = new MarshalHeap(2 * buff_len))
		{
			int num = InternalMethod.StdGetEnv(path, marshalHeap.buffer, buff_len);
			if (num >= 0)
			{
				result = Marshal.PtrToStringAnsi(marshalHeap.buffer);
			}
			else
			{
				Logger.LogWarningFormat("StdGetEnv error :{0} path:{1}", num, path);
			}
		}
		return result;
	}

	public static string GetSoftIdCode()
	{
		return StdGetEnv("/env/profile/soft_id_code");
	}

	public static string GetSystemId()
	{
		return StdGetEnv("/env/profile/system_id");
	}

	public static string GetHardwareId()
	{
		return StdGetEnv("/env/profile/hardware_id");
	}

	public static string GetLicenseId()
	{
		return StdGetEnv("/env/profile/license_id");
	}

	public static string GetSecurityCode()
	{
		return StdGetEnv("/env/profile/security_code");
	}

	public static string GetAccountId()
	{
		return StdGetEnv("/env/profile/account_id");
	}

	public static string GetAvsVersion()
	{
		return StdGetEnv("/env/avs2/version");
	}

	public static string GetBootstrapVersion()
	{
		return StdGetEnv("/env/boot/version");
	}

	public static int FsOpenW(string path)
	{
		return FsOpen(path, AvsTypes.FS_FLAG.OPEN_W, AvsTypes.FS_MODE.PMODE_DATA);
	}

	public static int FsOpenR(string path)
	{
		return FsOpen(path, AvsTypes.FS_FLAG.RDONLY, AvsTypes.FS_MODE.PMODE_DATA);
	}

	public static bool IsFsOpenSuccess(int ret)
	{
		return ret > 0;
	}

	public static long FsFsize(int fd)
	{
		AvsTypes.AVS_FS_STAT st = default(AvsTypes.AVS_FS_STAT);
		if (FsFstat(fd, out st) == 0)
		{
			return -1L;
		}
		return (long)(((ulong)st.hi_size << 32) + st.size);
	}

	public static bool FsPathIsExist(string path, out AvsTypes.AVS_FS_STAT st)
	{
		return FsLstat(path, out st) != 0;
	}

	public static bool FsFileIsExist(string path, out AvsTypes.AVS_FS_STAT st)
	{
		if (FsPathIsExist(path, out st))
		{
			return st.mode == 2;
		}
		return false;
	}

	public static bool FsDirectoryIsExist(string path, out AvsTypes.AVS_FS_STAT st)
	{
		if (FsPathIsExist(path, out st))
		{
			return st.mode == 1;
		}
		return false;
	}

	public static ulong TimeGet(AvsTypes.AVS_CLOCK from)
	{
		return ClockGet(from) / 1000;
	}

	public static DateTime GetDateTime(AvsTypes.AVS_CLOCK from = AvsTypes.AVS_CLOCK.SERVER)
	{
		ulong num = ClockGet(from);
		return new DateTime(1970, 1, 1, 0, 0, 0).AddMilliseconds(num);
	}

	public static void AvsLogMisc(string sect, string message)
	{
		InternalMethod.AvsLog(1, sect, message);
	}

	public static void AvsLogInfo(string sect, string message)
	{
		InternalMethod.AvsLog(2, sect, message);
	}

	public static void AvsLogWarning(string sect, string message)
	{
		InternalMethod.AvsLog(3, sect, message);
	}
}
