using System.Runtime.InteropServices;

namespace Konami.Aio;

public class aioIobBi2a : aioNCtlIob
{
	public enum CONSTVAL
	{
		SIZEOF_INPUTDATA = 16,
		SIZEOF_OUTPUTDATA = 40,
		SIZEOF_OUTPUTDATA_BI2A = 40,
		SIZEOF_OUTPUTDATA_BIOA = 32,
		SIZEOF_AMPVOLDATA = 4,
		SIZEOF_LEDDATA = 90,
		SIZEOF_LEDDATA_MFC = 90
	}

	public enum INITMODE
	{
		INITMODE_MFC = 252
	}

	public struct INPUTDATA
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
		public byte[] Data;
	}

	public struct OUTPUTDATA
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
		public byte[] Data;
	}

	public struct DEVSTATUS
	{
		public byte bIsBi2a;

		public byte IoCounter;

		public INPUTDATA InputData;

		public OUTPUTDATA OutputData;
	}

	public DEVSTATUS m_DevStatus;

	[DllImport("libaio-iob_video")]
	public unsafe static extern AIO_SCI_COMM* aioIobBi2a_OpenSciUsbCdc(uint i_SerialNumber);

	[DllImport("libaio-iob_video")]
	public unsafe static extern AIO_IOB_BI2A* aioIobBi2a_Create(AIO_NMGR_IOB* i_pNodeMgr, uint i_DevId, byte i_InitMode);

	[DllImport("libaio-iob_video")]
	public unsafe static extern uint aioIobBi2a_GetDeviceStatus(AIO_IOB_BI2A* i_pNodeCtl, ref DEVSTATUS o_pDevStatus, uint i_cbDevStatus);

	[DllImport("libaio-iob_video")]
	public unsafe static extern void aioIobBi2a_SetWatchDogTimer(AIO_IOB_BI2A* i_pNodeCtl, ushort i_Count);

	[DllImport("libaio-iob_video")]
	public unsafe static extern void aioIobBi2a_SetOutputData(AIO_IOB_BI2A* i_pNodeCtl, byte[] i_pData, uint i_cbData);

	[DllImport("libaio-iob_video")]
	public unsafe static extern void aioIobBi2a_SetAmpVolumeData(AIO_IOB_BI2A* i_pNodeCtl, byte[] i_pData, uint i_cbData);

	[DllImport("libaio-iob_video")]
	public unsafe static extern void aioIobBi2a_SetLedData(AIO_IOB_BI2A* i_pNodeCtl, byte[] i_pData, uint i_cbData);

	public unsafe aioIobBi2a(AIO_IOB_BI2A* i_pNodeCtl)
		: base((AIO_NCTL_IOB*)i_pNodeCtl)
	{
	}

	public unsafe static aioSciComm OpenSciUsbCdc(uint i_SerialNumber)
	{
		AIO_SCI_COMM* ptr = aioIobBi2a_OpenSciUsbCdc(i_SerialNumber);
		if (ptr == null)
		{
			return null;
		}
		return new aioSciComm(ptr);
	}

	public unsafe static aioIobBi2a Create(aioNMgrIob i_pNodeMgr, uint i_DevId, INITMODE i_InitMode)
	{
		AIO_IOB_BI2A* ptr = aioIobBi2a_Create((AIO_NMGR_IOB*)i_pNodeMgr.m_pNodeMgr, i_DevId, (byte)i_InitMode);
		if (ptr == null)
		{
			return null;
		}
		return new aioIobBi2a(ptr);
	}

	public unsafe virtual void GetDeviceStatus()
	{
		aioIobBi2a_GetDeviceStatus((AIO_IOB_BI2A*)m_pNodeCtl, ref m_DevStatus, (uint)Marshal.SizeOf(typeof(DEVSTATUS)));
	}

	public unsafe void SetWatchDogTimer(ushort i_Count)
	{
		aioIobBi2a_SetWatchDogTimer((AIO_IOB_BI2A*)m_pNodeCtl, i_Count);
	}

	public unsafe void SetOutputData(byte[] i_pData, uint i_cbData)
	{
		aioIobBi2a_SetOutputData((AIO_IOB_BI2A*)m_pNodeCtl, i_pData, i_cbData);
	}

	public unsafe void SetAmpVolumeData(byte[] i_pData, uint i_cbData)
	{
		aioIobBi2a_SetAmpVolumeData((AIO_IOB_BI2A*)m_pNodeCtl, i_pData, i_cbData);
	}

	public unsafe void SetLedData(byte[] i_pData, uint i_cbData)
	{
		aioIobBi2a_SetLedData((AIO_IOB_BI2A*)m_pNodeCtl, i_pData, i_cbData);
	}
}
