using System.Runtime.InteropServices;

namespace Konami.Aio;

public class aioIobIcca : aioNCtlIob
{
	public enum CONSTVAL
	{
		SIZEOF_INPUTDATA = 16,
		SIZEOF_OUTPUTDATA = 4
	}

	public enum INITMODE
	{
		INITMODE_TENKEY,
		INITMODE_IO
	}

	public enum CUSTATUS
	{
		CUST_DNI = 0,
		CUST_BUSY = 1,
		CUST_READY = 2,
		CUST_ERROR = 4,
		CUST_STSCLR = 5
	}

	public enum CUERROR
	{
		CUERR_SUCCEEDED = 3,
		CUERR_NO_RESPONSE,
		CUERR_ANTENNA,
		CUERR_CARD
	}

	public enum CUCARDTYPE
	{
		CUCT_ISO15693 = 0,
		CUCT_FeliCa = 1,
		CUCT_Mifare = 2,
		CUCT_UNKNOWN = 3,
		CUCT_TYPE_MASK = 3,
		CU_GETCARDID_MODE = 3
	}

	public enum KEYNO
	{
		KEY_0,
		KEY_1,
		KEY_4,
		KEY_7,
		KEY_00,
		KEY_2,
		KEY_5,
		KEY_8,
		KEY_BK,
		KEY_3,
		KEY_6,
		KEY_9
	}

	public struct INPUTDATA
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
		public byte[] Data;
	}

	public struct OUTPUTDATA
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
		public byte[] Data;
	}

	public struct INPUT
	{
		public byte cuStatus;

		public byte cuError;

		public aioIobCardId.CARDID CardId;

		public byte KeyNo;

		public byte KeyCount;

		public byte bKeyDataValid;

		public byte Key2No;

		public byte Key2Count;

		public byte bKey2DataValid;

		public byte bKey0;

		public byte bKey1;

		public byte bKey4;

		public byte bKey7;

		public byte bKey00;

		public byte bKey2;

		public byte bKey5;

		public byte bKey8;

		public byte bKeyBK;

		public byte bKey3;

		public byte bKey6;

		public byte bKey9;

		public byte CN1_1;

		public byte CN1_2;

		public byte CN1_3;

		public byte CN1_4;
	}

	public struct DEVSTATUS
	{
		public byte InputCounter;

		public byte OutputCounter;

		public INPUT Input;

		public INPUTDATA InputData;

		public OUTPUTDATA OutputData;
	}

	public DEVSTATUS m_DevStatus;

	[DllImport("libaio-iob_video")]
	public unsafe static extern AIO_IOB_ICCA* aioIobIcca_Create(AIO_NMGR_IOB* i_pNodeMgr, uint i_DevId, byte i_InitMode);

	[DllImport("libaio-iob_video")]
	public unsafe static extern uint aioIobIcca_GetDeviceStatus(AIO_IOB_ICCA* i_pNodeCtl, ref DEVSTATUS o_pDevStatus, uint i_cbDevStatus);

	[DllImport("libaio-iob_video")]
	public unsafe static extern void aioIobIcca_BeginGetCardId(AIO_IOB_ICCA* i_pNodeCtl);

	[DllImport("libaio-iob_video")]
	public unsafe static extern void aioIobIcca_EndGetCardId(AIO_IOB_ICCA* i_pNodeCtl);

	public unsafe aioIobIcca(AIO_IOB_ICCA* i_pNodeCtl)
		: base((AIO_NCTL_IOB*)i_pNodeCtl)
	{
	}

	public unsafe static aioIobIcca Create(aioNMgrIob i_pNodeMgr, uint i_DevId, INITMODE i_InitMode)
	{
		AIO_IOB_ICCA* ptr = aioIobIcca_Create((AIO_NMGR_IOB*)i_pNodeMgr.m_pNodeMgr, i_DevId, (byte)i_InitMode);
		if (ptr == null)
		{
			return null;
		}
		return new aioIobIcca(ptr);
	}

	public unsafe void GetDeviceStatus()
	{
		aioIobIcca_GetDeviceStatus((AIO_IOB_ICCA*)m_pNodeCtl, ref m_DevStatus, (uint)Marshal.SizeOf(typeof(DEVSTATUS)));
	}

	public unsafe void BeginGetCardId()
	{
		aioIobIcca_BeginGetCardId((AIO_IOB_ICCA*)m_pNodeCtl);
	}

	public unsafe void EndGetCardId()
	{
		aioIobIcca_EndGetCardId((AIO_IOB_ICCA*)m_pNodeCtl);
	}
}
