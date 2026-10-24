using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace KAMUNITY;

[KamunityDllApiClass]
public static class AvsEa3
{
	[KamunityDllApiClass]
	internal static class InternalMethod
	{
		public delegate int FUNC_Ea3PcbtrackerReport([In] string path);

		public delegate int FUNC_NetCtrl([In] AvsTypes.AVS_NET_CTRL tag, [In][Out] IntPtr buf, [In] int size);

		public delegate int FUNC_Ea3MessageSearch(string label, int scope, out int start, out int end, IntPtr buf, int bufsize, out int trylen);

		public delegate int FUNC_Ea3QueryFacilityString([In] string path, [Out] IntPtr sb, int size);

		public static FUNC_Ea3PcbtrackerReport Ea3PcbtrackerReport { get; private set; }

		public static FUNC_NetCtrl NetCtrl { get; private set; }

		public static FUNC_Ea3MessageSearch Ea3MessageSearch { get; private set; }

		public static FUNC_Ea3QueryFacilityString Ea3QueryFacilityString { get; private set; }

		public static T[] NetCtrlAsByteEnum<T>(AvsTypes.AVS_NET_CTRL tag) where T : struct
		{
			byte[] array = new byte[8];
			int size = array.Length;
			using MarshalHeap marshalHeap = new MarshalHeap(size);
			int num = NetCtrl(tag, marshalHeap.buffer, size);
			Marshal.Copy(marshalHeap.buffer, array, 0, num);
			T[] array2 = new T[num];
			for (int i = 0; i < num; i++)
			{
				array2[i] = (T)(object)(int)array[i];
			}
			Marshal.Copy(marshalHeap.buffer, array, 0, num);
			return array2;
		}

		public static T[] NetCtrlAsType<T>(AvsTypes.AVS_NET_CTRL tag)
		{
			T[] array = new T[8];
			int num = Marshal.SizeOf<T>();
			int size = num * array.Length;
			using MarshalHeap marshalHeap = new MarshalHeap(size);
			int num2 = NetCtrl(tag, marshalHeap.buffer, size) / num;
			IntPtr buffer = marshalHeap.buffer;
			for (int i = 0; i < num2; i++)
			{
				array[i] = Marshal.PtrToStructure<T>(buffer);
				buffer += num;
			}
			Array.Resize(ref array, num2);
			return array;
		}
	}

	public delegate AvsTypes.EA3_STATUS FUNC_Ea3GetStatus(out int code, out int subcode);

	public delegate AvsTypes.EA3_BOOT_STATUS FUNC_Ea3GetBootStatus(out AvsTypes.EA3_BOOT_CODE code, out int subcode);

	public delegate AvsTypes.EA3_PP_STATUS FUNC_Ea3GetApsStatus(out AvsTypes.EA3_BOOT_CODE code, out int subcode);

	public delegate AvsTypes.EA3_PP_STATUS FUNC_Ea3GetPpStatus();

	public delegate int FUNC_Ea3PosevReport([In] string path);

	public delegate int FUNC_Ea3PosevReportWithNum([In] string path, [In] int num);

	public delegate int FUNC_Ea3TimeIsHoliday([In] ulong t);

	public delegate int FUNC_KeepaliveFindByName([In] string name);

	public delegate void FUNC_KeepalivePing([In] int desc);

	public delegate int FUNC_KeepaliveGetStatus([In] int desc, out AvsTypes.KEEPALIVE_STATUS status);

	public delegate int FUNC_Ea3QueryServicesProductDomain(out int product_domain);

	public enum FACILITY_PATH_STRING
	{
		LOCATION_ID,
		LOCATION_COUNTRY,
		LOCATION_REGION,
		LOCATION_NAME,
		LOCATION_COUNTRYNAME,
		LOCATION_COUNTRYJNAME,
		LOCATION_REGIONNAME,
		LOCATION_REGIONJNAME,
		LOCATION_CUSTOMERCODE,
		LOCATION_COMPANYCODE,
		LINE_ID
	}

	public enum FACILITY_PATH_U8
	{
		LOCATION_TYPE,
		LOCATION_ACCURACY,
		LINE_CLASS
	}

	public enum FACILITY_PATH_U16
	{
		PORTFW_GLOBALPORT,
		PORTFW_PRIVATEPORT
	}

	public enum FACILITY_PATH_S32
	{
		LOCATION_LATITUDE,
		LOCATION_LONGITUDE
	}

	public enum FACILITY_PATH_IN4ADDR
	{
		PORTFW_GLOBALIP
	}

	public delegate int FUNC_Ea3QueryFacilityU8([In] string path, out byte dst);

	public delegate int FUNC_Ea3QueryFacilityU16([In] string path, out ushort dst);

	public delegate int FUNC_Ea3QueryFacilityS32([In] string path, out int dst);

	public delegate int FUNC_Ea3QueryFacilityIn4addr([In] string path, out AvsTypes.avs_net_in4_addr dst);

	private static readonly Dictionary<FACILITY_PATH_STRING, string> s_FacilityPathString = new Dictionary<FACILITY_PATH_STRING, string>
	{
		{
			FACILITY_PATH_STRING.LOCATION_ID,
			"location/id"
		},
		{
			FACILITY_PATH_STRING.LOCATION_COUNTRY,
			"location/country"
		},
		{
			FACILITY_PATH_STRING.LOCATION_REGION,
			"location/region"
		},
		{
			FACILITY_PATH_STRING.LOCATION_NAME,
			"location/name"
		},
		{
			FACILITY_PATH_STRING.LOCATION_COUNTRYNAME,
			"location/countryname"
		},
		{
			FACILITY_PATH_STRING.LOCATION_COUNTRYJNAME,
			"location/countryjname"
		},
		{
			FACILITY_PATH_STRING.LOCATION_REGIONNAME,
			"location/regionname"
		},
		{
			FACILITY_PATH_STRING.LOCATION_REGIONJNAME,
			"location/regionjname"
		},
		{
			FACILITY_PATH_STRING.LOCATION_CUSTOMERCODE,
			"location/customercode"
		},
		{
			FACILITY_PATH_STRING.LOCATION_COMPANYCODE,
			"location/companycode"
		},
		{
			FACILITY_PATH_STRING.LINE_ID,
			"line/id"
		}
	};

	private static readonly Dictionary<FACILITY_PATH_U8, string> s_FacilityPathU8 = new Dictionary<FACILITY_PATH_U8, string>
	{
		{
			FACILITY_PATH_U8.LOCATION_TYPE,
			"location/type"
		},
		{
			FACILITY_PATH_U8.LOCATION_ACCURACY,
			"location/accuracy"
		},
		{
			FACILITY_PATH_U8.LINE_CLASS,
			"line/class"
		}
	};

	private static readonly Dictionary<FACILITY_PATH_U16, string> s_FacilityPathU16 = new Dictionary<FACILITY_PATH_U16, string>
	{
		{
			FACILITY_PATH_U16.PORTFW_GLOBALPORT,
			"portfw/globalport"
		},
		{
			FACILITY_PATH_U16.PORTFW_PRIVATEPORT,
			"portfw/privateport"
		}
	};

	private static readonly Dictionary<FACILITY_PATH_S32, string> s_FacilityPathS32 = new Dictionary<FACILITY_PATH_S32, string>
	{
		{
			FACILITY_PATH_S32.LOCATION_LATITUDE,
			"location/latitude"
		},
		{
			FACILITY_PATH_S32.LOCATION_LONGITUDE,
			"location/longitude"
		}
	};

	private static readonly Dictionary<FACILITY_PATH_IN4ADDR, string> s_FacilityPathIn4addr = new Dictionary<FACILITY_PATH_IN4ADDR, string> { 
	{
		FACILITY_PATH_IN4ADDR.PORTFW_GLOBALIP,
		"portfw/globalip"
	} };

	public static FUNC_Ea3GetStatus Ea3GetStatus { get; private set; }

	public static FUNC_Ea3GetBootStatus Ea3GetBootStatus { get; private set; }

	public static FUNC_Ea3GetApsStatus Ea3GetApsStatus { get; private set; }

	public static FUNC_Ea3GetPpStatus Ea3GetPpStatus { get; private set; }

	public static FUNC_Ea3PosevReport Ea3PosevReport { get; private set; }

	public static FUNC_Ea3PosevReportWithNum Ea3PosevReportWithNum { get; private set; }

	public static FUNC_Ea3TimeIsHoliday Ea3TimeIsHoliday { get; private set; }

	public static FUNC_KeepaliveFindByName KeepaliveFindByName { get; private set; }

	public static FUNC_KeepalivePing KeepalivePing { get; private set; }

	public static FUNC_KeepaliveGetStatus KeepaliveGetStatus { get; private set; }

	public static FUNC_Ea3QueryServicesProductDomain Ea3QueryServicesProductDomain { get; private set; }

	public static FUNC_Ea3QueryFacilityU8 Ea3QueryFacilityU8 { get; private set; }

	public static FUNC_Ea3QueryFacilityU16 Ea3QueryFacilityU16 { get; private set; }

	public static FUNC_Ea3QueryFacilityS32 Ea3QueryFacilityS32 { get; private set; }

	public static FUNC_Ea3QueryFacilityIn4addr Ea3QueryFacilityIn4addr { get; private set; }

	public static int Ea3MessageSearch(string label, int scope, out int start, out int end, StringBuilder buf, int bufsize, out int trylen)
	{
		using MarshalHeap marshalHeap = new MarshalHeap(bufsize);
		int num = InternalMethod.Ea3MessageSearch(label, scope, out start, out end, marshalHeap.buffer, marshalHeap.size, out trylen);
		if (num == 1)
		{
			buf.Append(marshalHeap.ToStringAnsi());
		}
		return num;
	}

	public static int Ea3MainteSearch(int scope, out int start, out int end, StringBuilder buf, int bufsize, out int trylen)
	{
		return Ea3MessageSearch("sys.mainte", scope, out start, out end, buf, bufsize, out trylen);
	}

	public static void Ea3PcbtrackerReportLicenseAgree()
	{
		InternalMethod.Ea3PcbtrackerReport("/license/agree");
	}

	public static AvsTypes.avs_net_in4_addr[] NetCtrlAddress()
	{
		return InternalMethod.NetCtrlAsType<AvsTypes.avs_net_in4_addr>(AvsTypes.AVS_NET_CTRL.ADDRESS);
	}

	public static AvsTypes.avs_net_mac_addr[] NetCtrlMac()
	{
		return InternalMethod.NetCtrlAsType<AvsTypes.avs_net_mac_addr>(AvsTypes.AVS_NET_CTRL.MAC);
	}

	public static AvsTypes.avs_net_in4_addr[] NetCtrlBcast()
	{
		return InternalMethod.NetCtrlAsType<AvsTypes.avs_net_in4_addr>(AvsTypes.AVS_NET_CTRL.BCAST);
	}

	public static AvsTypes.avs_net_in4_addr[] NetCtrlNetmask()
	{
		return InternalMethod.NetCtrlAsType<AvsTypes.avs_net_in4_addr>(AvsTypes.AVS_NET_CTRL.NETMASK);
	}

	public static AvsTypes.avs_net_in4_addr[] NetCtrlGateway()
	{
		return InternalMethod.NetCtrlAsType<AvsTypes.avs_net_in4_addr>(AvsTypes.AVS_NET_CTRL.GATEWAY);
	}

	public static AvsTypes.avs_net_in4_addr[] NetCtrlDhcp()
	{
		return InternalMethod.NetCtrlAsType<AvsTypes.avs_net_in4_addr>(AvsTypes.AVS_NET_CTRL.DHCP);
	}

	public static AvsTypes.AVS_NET_TYPE[] NetCtrlType()
	{
		return InternalMethod.NetCtrlAsByteEnum<AvsTypes.AVS_NET_TYPE>(AvsTypes.AVS_NET_CTRL.TYPE);
	}

	public static AvsTypes.avs_net_ifconfig[] NetCtrlIfconfig()
	{
		return InternalMethod.NetCtrlAsType<AvsTypes.avs_net_ifconfig>(AvsTypes.AVS_NET_CTRL.IFCONFIG);
	}

	public static AvsTypes.avs_net_in4_addr[] NetCtrlDns0()
	{
		return InternalMethod.NetCtrlAsType<AvsTypes.avs_net_in4_addr>(AvsTypes.AVS_NET_CTRL.DNS0);
	}

	public static AvsTypes.avs_net_in4_addr[] NetCtrlDns1()
	{
		return InternalMethod.NetCtrlAsType<AvsTypes.avs_net_in4_addr>(AvsTypes.AVS_NET_CTRL.DNS1);
	}

	public static int NetCtrlFlush()
	{
		return InternalMethod.NetCtrl(AvsTypes.AVS_NET_CTRL.FLUSH, IntPtr.Zero, 0);
	}

	[Obsolete]
	public static int Ea3QueryFacilityLocationId(StringBuilder sb, int size)
	{
		return Ea3QueryFacilityPathString(FACILITY_PATH_STRING.LOCATION_ID, sb, size);
	}

	public static int Ea3QueryFacilityPathString(FACILITY_PATH_STRING type, StringBuilder sb, int size)
	{
		if (s_FacilityPathString.ContainsKey(type))
		{
			return Ea3QueryFacilityString(s_FacilityPathString[type], sb, size);
		}
		return -1;
	}

	public static int Ea3QueryFacilityPathU8(FACILITY_PATH_U8 type, out byte dst)
	{
		if (s_FacilityPathU8.ContainsKey(type))
		{
			return Ea3QueryFacilityU8(s_FacilityPathU8[type], out dst);
		}
		dst = 0;
		return -1;
	}

	public static int Ea3QueryFacilityPathU16(FACILITY_PATH_U16 type, out ushort dst)
	{
		if (s_FacilityPathU16.ContainsKey(type))
		{
			return Ea3QueryFacilityU16(s_FacilityPathU16[type], out dst);
		}
		dst = 0;
		return -1;
	}

	public static int Ea3QueryFacilityPathS32(FACILITY_PATH_S32 type, out int dst)
	{
		if (s_FacilityPathS32.ContainsKey(type))
		{
			return Ea3QueryFacilityS32(s_FacilityPathS32[type], out dst);
		}
		dst = 0;
		return -1;
	}

	public static int Ea3QueryFacilityPathIn4addr(FACILITY_PATH_IN4ADDR type, out AvsTypes.avs_net_in4_addr dst)
	{
		if (s_FacilityPathIn4addr.ContainsKey(type))
		{
			return Ea3QueryFacilityIn4addr(s_FacilityPathIn4addr[type], out dst);
		}
		dst = default(AvsTypes.avs_net_in4_addr);
		dst.addr8[0] = 0;
		dst.addr8[1] = 0;
		dst.addr8[2] = 0;
		dst.addr8[3] = 0;
		return -1;
	}

	public static int Ea3QueryFacilityString([In] string path, [Out] StringBuilder sb, int size)
	{
		using MarshalHeap marshalHeap = new MarshalHeap(size);
		int num = InternalMethod.Ea3QueryFacilityString(path, marshalHeap.buffer, marshalHeap.size);
		if (num >= 0)
		{
			sb.Append(marshalHeap.ToStringAnsi());
		}
		return num;
	}
}
