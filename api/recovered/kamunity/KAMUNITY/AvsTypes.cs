using System;
using System.Runtime.InteropServices;

namespace KAMUNITY;

public static class AvsTypes
{
	public enum CardType
	{
		MCARD = 0,
		ICCARD = 1,
		FELICA = 2,
		VIRTUAL = 4,
		CARDLESS = 5
	}

	public enum EA3_BOOT_STATUS
	{
		BOOT,
		RESUME,
		DONE
	}

	public enum EA3_BOOT_CODE
	{
		OK = 0,
		DHCP_ERROR = 2000,
		SERVICES_ERROR = 2002,
		EA3_BOOT_CODE_SERVICES_FORBIDDEN = 2004,
		PCBTRACKER_ERROR = 2003,
		PCBTRACKER_FORBIDDEN = 2005,
		PCBTRACKER_SERVEND = 2006,
		MESSAGE_ERROR = 2007,
		POSEVENT_ERROR = 2008,
		FACILITY_ERROR = 2009,
		PCBTRACKER_CONFIRM = 2012,
		PCBTRACKER_REVISION = 2014,
		PCBTRACKER_CONTACT = 2015,
		APSMANAGER_ERROR = 2016,
		APSMANAGER_CONTACT = 2017,
		APSMANAGER_SUPPLY = 2018,
		PCBEVENT_QUEUE = 2019,
		APSMANAGER_MAINTE = 2020
	}

	public enum EA3_STATUS
	{
		BOOT,
		DOWN,
		OFFLINE,
		ONLINE,
		MAINTE,
		RUNTIME_ERROR,
		DISABLED
	}

	public enum EA3_STATUS_DOWN
	{
		RESUME,
		CANT_GET,
		DOWN
	}

	public enum EA3_PP_STATUS
	{
		BOOT,
		DISABLE,
		NOT_AVAILABLE,
		AVAILABLE
	}

	public enum EA3_PP_SUBCODE
	{
		QUEUE_STUCK = 1,
		QUEUE_ALERT
	}

	public enum EA3_EACOIN_STATUS
	{
		AVAILABLE,
		NOT_AVAILABLE,
		IGNORE
	}

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

	public enum EA3_EACOIN_SESSION
	{
		CLOSE,
		OPENING,
		OPEN,
		CLOSING
	}

	[Flags]
	public enum FS_FLAG
	{
		RDONLY = 1,
		WRONLY = 2,
		RDWR = 3,
		CREAT = 0x10,
		TRUNC = 0x20,
		APPEND = 0x40,
		EXCL = 0x80,
		OPEN_R = 1,
		OPEN_W = 0x32,
		OPEN_RW = 0x33
	}

	[Flags]
	public enum FS_MODE
	{
		UR = 0x100,
		UW = 0x80,
		UX = 0x40,
		GR = 0x20,
		GW = 0x10,
		GX = 8,
		OR = 4,
		OW = 2,
		OX = 1,
		URWX = 0x1C0,
		GRWX = 0x38,
		ORWX = 7,
		ACCESS = 0x1FF,
		PMODE_DATA = 0x1A4,
		PMODE_EXEC = 0x1ED
	}

	public enum FS_SEEK
	{
		SET,
		CUR,
		END
	}

	public enum FS_STAT : ushort
	{
		UNKNOWN,
		DIR,
		REG,
		LINK,
		SOCK,
		DEVICE
	}

	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	public struct AVS_FS_STAT
	{
		public ulong ctime;

		public ulong mtime;

		public ulong atime;

		public uint link_count;

		public uint size;

		public uint hi_size;

		public ushort mode;

		public ushort permission;
	}

	public enum AVS_CLOCK
	{
		LOCAL,
		SERVER,
		NTP
	}

	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	public struct avs_net_ifconfig
	{
		private byte _valid;

		private byte type;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
		public byte[] mac;

		public avs_net_in4_addr addr;

		public avs_net_in4_addr bcast;

		public avs_net_in4_addr netmask;

		public avs_net_in4_addr gateway;

		public avs_net_in4_addr dhcp;

		public bool valid => _valid != 0;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	public struct avs_net_in4_addr
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
		public byte[] addr8;

		public int addr32 => addr8[0] | (addr8[1] << 8) | (addr8[2] << 16) | (addr8[3] << 24);

		public override string ToString()
		{
			return addr8[0] + "." + addr8[1] + "." + addr8[2] + "." + addr8[3];
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct avs_net_mac_addr
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
		public byte[] mac_addr;
	}

	public enum AVS_NET_CTRL
	{
		ADDRESS,
		MAC,
		BCAST,
		NETMASK,
		GATEWAY,
		DHCP,
		TYPE,
		IFCONFIG,
		DNS0,
		DNS1,
		FLUSH,
		CTRL
	}

	public enum AVS_NET_TYPE
	{
		DOWN,
		DHCP,
		STATIC,
		ID
	}

	public enum AVS_KEEPALIVE_STATUS
	{
		DOWN,
		MAY_DOWN,
		MAY_UP,
		UP
	}

	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	public struct KEEPALIVE_STATUS
	{
		public AVS_KEEPALIVE_STATUS status;

		public uint send_ctr;

		public uint recv_ctr;

		public uint time_of_status;
	}

	public enum AVS_THREAD_PRIORITY
	{
		HIGHEST,
		HIGH,
		NORMAL,
		LOW,
		LOWEST
	}

	[Flags]
	public enum PROPERTY_MODE : uint
	{
		READ = 1u,
		WRITE = 2u,
		CREATE = 4u,
		ATYPE_MASK = 7u,
		RDONLY = 1u,
		WRONLY = 6u,
		RDWR = 7u,
		TEXT = 0u,
		BINARY = 8u,
		JSON = 0x800u,
		FORMAT_MASK = 0x808u,
		DUPNODE = 0x10u,
		NO_XML_HEADER = 0x100u,
		NO_EXTENSION = 0x200u,
		VERBOSE = 0x400u,
		INPUT_TEXT = 0u,
		INPUT_BINARY = 0x80000000u,
		INPUT_JSON = 0x40000000u,
		INPUT_MASK = 0xC0000000u,
		EXTEND = 0x1000u
	}

	public enum PROPERTY_ENCODE
	{
		NONE = 0,
		ASCII = 32,
		ISO_8859_1 = 64,
		EUCJP = 96,
		SJIS = 128,
		UTF8 = 160,
		ENCODE_MASK = 224
	}

	public enum PROPERTY_TYPE
	{
		VOID = 1,
		S8 = 2,
		U8 = 3,
		S16 = 4,
		U16 = 5,
		S32 = 6,
		U32 = 7,
		S64 = 8,
		U64 = 9,
		BINARY = 10,
		STRING = 11,
		IP4 = 12,
		TIME = 13,
		FLOAT = 14,
		DOUBLE = 15,
		ATTRIB = 46,
		ARRAY = 47,
		BOOL = 52,
		MAX = 63,
		MASK = 63,
		HAS_ATTRIB = 64,
		HAS_CHILD = 128
	}

	public enum PROPERTY_NODE_TRAVERSE
	{
		PARENT,
		CHILD,
		ATTRIB,
		FIRST,
		NEXT,
		PREV,
		LAST,
		NEXT_SAME_NAME,
		PREV_SAME_NAME
	}

	public enum PROPERTY_NODE_HAS
	{
		PARENT,
		CHILD,
		SIBLING,
		ATTRIB,
		SAME_NAME_SIBLING
	}

	public const string AVS_KEEPALIVE_NAME_SERVER = "server";

	public const string AVS_KEEPALIVE_NAME_CENTER = "center";

	public const string AVS_KEEPALIVE_NAME_ROUTER = "router";

	public const string AVS_KEEPALIVE_NAME_COM = "com";

	public const string AVS_KEEPALIVE_NAME_FUN = "fun";
}
