using System.Runtime.InteropServices;

namespace Konami.Aio;

public class aioIob2Bi2xTCS : aioIob2Bi2xAC1
{
	public new enum CONSTVAL
	{
		IO_COUNTER_TIME = 2000
	}

	public new enum SLOT
	{
		SLOT_1,
		SLOT_MAX
	}

	public new enum COUNTER
	{
		COUNTER_COIN,
		COUNTER_MAX
	}

	public new enum TAPELED_CNT
	{
		TAPELED_CNT_SIDE_PANEL = 30
	}

	public new enum TAPELED
	{
		TAPELED_LEFT_PANEL,
		TAPELED_RIGHT_PANEL,
		TAPELED_MAX
	}

	public new struct INPUT
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
	}

	public new struct DEVSTATUS
	{
		public byte InputCounter;

		public byte OutputCounter;

		public byte TapeLedCounter;

		public INPUT Input;

		public INPUTDATA InputData;

		public OUTPUTDATA OutputData;

		public IORESETDATA IoResetData;
	}

	public new DEVSTATUS m_DevStatus;

	[DllImport("aio-iob2_video")]
	public unsafe static extern AIO_IOB2_BI2X_TCS* aioIob2Bi2xTCS_Create(AIO_NMGR_IOB2* i_pNodeMgr, uint i_DevId);

	[DllImport("aio-iob2_video")]
	public unsafe static extern uint aioIob2Bi2xTCS_GetDeviceStatus(AIO_IOB2_BI2X_TCS* i_pNodeCtl, ref DEVSTATUS o_pDevStatus, uint i_cbDevStatus);

	[DllImport("aio-iob2_video")]
	public unsafe static extern void aioIob2Bi2xTCS_IoReset(AIO_IOB2_BI2X_TCS* i_pNodeCtl, uint i_bfIoReset);

	[DllImport("aio-iob2_video")]
	public unsafe static extern void aioIob2Bi2xTCS_SetWatchDogTimer(AIO_IOB2_BI2X_TCS* i_pNodeCtl, byte i_Count);

	[DllImport("aio-iob2_video")]
	public unsafe static extern void aioIob2Bi2xTCS_ControlCoinBlocker(AIO_IOB2_BI2X_TCS* i_pNodeCtl, uint i_Slot, bool i_bOpen);

	[DllImport("aio-iob2_video")]
	public unsafe static extern void aioIob2Bi2xTCS_AddCounter(AIO_IOB2_BI2X_TCS* i_pNodeCtl, uint i_Counter, uint i_Count);

	[DllImport("aio-iob2_video")]
	public unsafe static extern void aioIob2Bi2xTCS_SetPhotoLight(AIO_IOB2_BI2X_TCS* i_pNodeCtl, bool i_bOn);

	[DllImport("aio-iob2_video")]
	public unsafe static extern void aioIob2Bi2xTCS_SetIccrLed(AIO_IOB2_BI2X_TCS* i_pNodeCtl, uint i_RGB);

	[DllImport("aio-iob2_video")]
	public unsafe static extern void aioIob2Bi2xTCS_SetTapeLedData(AIO_IOB2_BI2X_TCS* i_pNodeCtl, uint i_TapeLed, byte[] i_pData);

	public unsafe aioIob2Bi2xTCS(AIO_IOB2_BI2X_TCS* i_pNodeCtl)
		: base((AIO_IOB2_BI2X_AC1*)i_pNodeCtl)
	{
	}

	public unsafe static aioIob2Bi2xTCS Create(aioNMgrIob2 i_pNodeMgr, uint i_DevId)
	{
		AIO_IOB2_BI2X_TCS* ptr = aioIob2Bi2xTCS_Create((AIO_NMGR_IOB2*)i_pNodeMgr.m_pNodeMgr, i_DevId);
		if (ptr == null)
		{
			return null;
		}
		return new aioIob2Bi2xTCS(ptr);
	}

	public unsafe override void GetDeviceStatus()
	{
		base.GetDeviceStatus();
		aioIob2Bi2xTCS_GetDeviceStatus((AIO_IOB2_BI2X_TCS*)m_pNodeCtl, ref m_DevStatus, (uint)Marshal.SizeOf(typeof(DEVSTATUS)));
	}

	public new unsafe void IoReset(IORESET i_bfIoReset)
	{
		aioIob2Bi2xTCS_IoReset((AIO_IOB2_BI2X_TCS*)m_pNodeCtl, (uint)i_bfIoReset);
	}

	public new unsafe void SetWatchDogTimer(byte i_Count)
	{
		aioIob2Bi2xTCS_SetWatchDogTimer((AIO_IOB2_BI2X_TCS*)m_pNodeCtl, i_Count);
	}

	public unsafe void ControlCoinBlocker(SLOT i_Slot, bool i_bOpen)
	{
		aioIob2Bi2xTCS_ControlCoinBlocker((AIO_IOB2_BI2X_TCS*)m_pNodeCtl, (uint)i_Slot, i_bOpen);
	}

	public unsafe void AddCounter(COUNTER i_Counter, uint i_Count)
	{
		aioIob2Bi2xTCS_AddCounter((AIO_IOB2_BI2X_TCS*)m_pNodeCtl, (uint)i_Counter, i_Count);
	}

	public unsafe void SetPhotoLight(bool i_bOn)
	{
		aioIob2Bi2xTCS_SetPhotoLight((AIO_IOB2_BI2X_TCS*)m_pNodeCtl, i_bOn);
	}

	public unsafe void SetIccrLed(uint i_RGB)
	{
		aioIob2Bi2xTCS_SetIccrLed((AIO_IOB2_BI2X_TCS*)m_pNodeCtl, i_RGB);
	}

	public unsafe void SetTapeLedData(TAPELED i_TapeLed, byte[] i_pData)
	{
		aioIob2Bi2xTCS_SetTapeLedData((AIO_IOB2_BI2X_TCS*)m_pNodeCtl, (uint)i_TapeLed, i_pData);
	}
}
