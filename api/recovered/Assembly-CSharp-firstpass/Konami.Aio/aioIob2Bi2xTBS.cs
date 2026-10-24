using System.Runtime.InteropServices;

namespace Konami.Aio;

public class aioIob2Bi2xTBS : aioIob2Bi2x
{
	public enum SLOT
	{
		SLOT_1,
		SLOT_MAX
	}

	public enum COUNTER
	{
		COUNTER_COIN,
		COUNTER_MAX
	}

	public enum VOLUME
	{
		VOLUME_DEFAULT = 0,
		VOLUME_MAX = 13
	}

	public enum TAPELED_CNT
	{
		TAPELED_CNT_TITLE = 48,
		TAPELED_CNT_CABINET_LEFT
	}

	public enum TAPELED
	{
		TAPELED_TITLE,
		TAPELED_CABINET_LEFT,
		TAPELED_MAX
	}

	public enum IORESET
	{
		IORESET_EXIO_AB = 1,
		IORESET_COIN_JAM = 0x10
	}

	public struct INPUTDATA
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
		public byte[] Data;
	}

	public struct OUTPUTDATA
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
		public byte[] Data;
	}

	public struct IORESETDATA
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
		public byte[] Data;
	}

	public struct INPUT
	{
		public byte DevIoCounter;

		public byte bExIoAErr;

		public byte bExIoBErr;

		public byte bPcPowerOn;

		public byte bPcPowerCheck;

		public byte CoinCount;

		public byte bTest;

		public byte bService;

		public byte bCoinSw;

		public byte bCoinJam;

		public byte bHPDetect;

		public ushort StickX;

		public ushort StickY;

		public byte bStickBtn;

		public byte bTrigger1;

		public byte bTrigger2;

		public byte bButton0;

		public byte bButton1;

		public byte bButton2;

		public byte bButton3;
	}

	public struct DEVSTATUS
	{
		public byte InputCounter;

		public byte OutputCounter;

		public byte TapeLedCounter;

		public INPUT Input;

		public INPUTDATA InputData;

		public OUTPUTDATA OutputData;

		public IORESETDATA IoResetData;
	}

	public DEVSTATUS m_DevStatus;

	[DllImport("aio-iob2_video")]
	public unsafe static extern AIO_IOB2_BI2X_TBS* aioIob2Bi2xTBS_Create(AIO_NMGR_IOB2* i_pNodeMgr, uint i_DevId);

	[DllImport("aio-iob2_video")]
	public unsafe static extern uint aioIob2Bi2xTBS_GetDeviceStatus(AIO_IOB2_BI2X_TBS* i_pNodeCtl, ref DEVSTATUS o_pDevStatus, uint i_cbDevStatus);

	[DllImport("aio-iob2_video")]
	public unsafe static extern void aioIob2Bi2xTBS_IoReset(AIO_IOB2_BI2X_TBS* i_pNodeCtl, IORESET i_bfIoReset);

	[DllImport("aio-iob2_video")]
	public unsafe static extern void aioIob2Bi2xTBS_SetWatchDogTimer(AIO_IOB2_BI2X_TBS* i_pNodeCtl, byte i_Count);

	[DllImport("aio-iob2_video")]
	public unsafe static extern void aioIob2Bi2xTBS_ControlCoinBlocker(AIO_IOB2_BI2X_TBS* i_pNodeCtl, SLOT i_Slot, bool i_bOpen);

	[DllImport("aio-iob2_video")]
	public unsafe static extern void aioIob2Bi2xTBS_AddCounter(AIO_IOB2_BI2X_TBS* i_pNodeCtl, COUNTER i_Counter, uint i_Count);

	[DllImport("aio-iob2_video")]
	public unsafe static extern void aioIob2Bi2xTBS_SetAmpVolume(AIO_IOB2_BI2X_TBS* i_pNodeCtl, byte i_Volume);

	[DllImport("aio-iob2_video")]
	public unsafe static extern void aioIob2Bi2xTBS_EnableUsbCharger(AIO_IOB2_BI2X_TBS* i_pNodeCtl, bool i_bEnable);

	[DllImport("aio-iob2_video")]
	public unsafe static extern void aioIob2Bi2xTBS_SetIrLed(AIO_IOB2_BI2X_TBS* i_pNodeCtl, bool i_bOn);

	[DllImport("aio-iob2_video")]
	public unsafe static extern void aioIob2Bi2xTBS_SetButton0Lamp(AIO_IOB2_BI2X_TBS* i_pNodeCtl, bool i_bOn);

	[DllImport("aio-iob2_video")]
	public unsafe static extern void aioIob2Bi2xTBS_SetIccrLed(AIO_IOB2_BI2X_TBS* i_pNodeCtl, uint i_RGB);

	[DllImport("aio-iob2_video")]
	public unsafe static extern void aioIob2Bi2xTBS_SetStickLed(AIO_IOB2_BI2X_TBS* i_pNodeCtl, uint i_RGB);

	[DllImport("aio-iob2_video")]
	public unsafe static extern void aioIob2Bi2xTBS_SetTapeLedData(AIO_IOB2_BI2X_TBS* i_pNodeCtl, TAPELED i_TapeLed, byte[] i_pData);

	public unsafe aioIob2Bi2xTBS(AIO_IOB2_BI2X_TBS* i_pNodeCtl)
		: base((AIO_IOB2_BI2X*)i_pNodeCtl)
	{
	}

	public unsafe static aioIob2Bi2xTBS Create(aioNMgrIob2 i_pNodeMgr, uint i_DevId)
	{
		AIO_IOB2_BI2X_TBS* ptr = aioIob2Bi2xTBS_Create((AIO_NMGR_IOB2*)i_pNodeMgr.m_pNodeMgr, i_DevId);
		if (ptr != null)
		{
			aioIob2Bi2xTBS aioIob2Bi2xTBS2 = new aioIob2Bi2xTBS(ptr);
			if (aioIob2Bi2xTBS2 != null)
			{
				return aioIob2Bi2xTBS2;
			}
			aioNodeCtl.aioNodeCtl_Destroy((AIO_NCTL*)ptr);
		}
		return null;
	}

	public unsafe void GetDeviceStatus()
	{
		aioIob2Bi2xTBS_GetDeviceStatus((AIO_IOB2_BI2X_TBS*)m_pNodeCtl, ref m_DevStatus, (uint)Marshal.SizeOf(typeof(DEVSTATUS)));
	}

	public unsafe void IoReset(IORESET i_bfIoReset)
	{
		aioIob2Bi2xTBS_IoReset((AIO_IOB2_BI2X_TBS*)m_pNodeCtl, i_bfIoReset);
	}

	public unsafe void SetWatchDogTimer(byte i_Count)
	{
		aioIob2Bi2xTBS_SetWatchDogTimer((AIO_IOB2_BI2X_TBS*)m_pNodeCtl, i_Count);
	}

	public unsafe void ControlCoinBlocker(SLOT i_Slot, bool i_bOpen)
	{
		aioIob2Bi2xTBS_ControlCoinBlocker((AIO_IOB2_BI2X_TBS*)m_pNodeCtl, i_Slot, i_bOpen);
	}

	public unsafe void AddCounter(COUNTER i_Counter, uint i_Count)
	{
		aioIob2Bi2xTBS_AddCounter((AIO_IOB2_BI2X_TBS*)m_pNodeCtl, i_Counter, i_Count);
	}

	public unsafe void SetHeadPhoneVolume(byte i_Volume)
	{
		aioIob2Bi2xTBS_SetAmpVolume((AIO_IOB2_BI2X_TBS*)m_pNodeCtl, i_Volume);
	}

	public unsafe void EnableUsbCharger(bool i_bEnable)
	{
		aioIob2Bi2xTBS_EnableUsbCharger((AIO_IOB2_BI2X_TBS*)m_pNodeCtl, i_bEnable);
	}

	public unsafe void SetIrLed(bool i_bOn)
	{
		aioIob2Bi2xTBS_SetIrLed((AIO_IOB2_BI2X_TBS*)m_pNodeCtl, i_bOn);
	}

	public unsafe void SetButton0Lamp(bool i_bOn)
	{
		aioIob2Bi2xTBS_SetButton0Lamp((AIO_IOB2_BI2X_TBS*)m_pNodeCtl, i_bOn);
	}

	public unsafe void SetIccrLed(uint i_RGB)
	{
		aioIob2Bi2xTBS_SetIccrLed((AIO_IOB2_BI2X_TBS*)m_pNodeCtl, i_RGB);
	}

	public unsafe void SetStickLed(uint i_RGB)
	{
		aioIob2Bi2xTBS_SetStickLed((AIO_IOB2_BI2X_TBS*)m_pNodeCtl, i_RGB);
	}

	public unsafe void SetTapeLedData(TAPELED i_TapeLed, byte[] i_pData)
	{
		aioIob2Bi2xTBS_SetTapeLedData((AIO_IOB2_BI2X_TBS*)m_pNodeCtl, i_TapeLed, i_pData);
	}
}
