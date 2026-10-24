using System.Runtime.InteropServices;

namespace Konami.Aio;

public class aioIob2Bi2xAC1 : aioIob2Bi2x
{
	public new enum CONSTVAL
	{
		SIZEOF_INPUTDATA = 241,
		SIZEOF_OUTPUTDATA = 48,
		SIZEOF_IORESETDATA = 4,
		IO_COUNTER_TIME_DEFAULT = 2000,
		IO_COUNTER_TIME_MIN = 1000,
		ICNPIN_HIST = 18
	}

	public enum SLOT
	{
		SLOT_1,
		SLOT_2,
		SLOT_3,
		SLOT_4,
		SLOT_MAX
	}

	public enum COUNTER
	{
		COUNTER_1,
		COUNTER_2,
		COUNTER_3,
		COUNTER_4,
		COUNTER_MAX
	}

	public enum AMP
	{
		AMP_1,
		AMP_2,
		AMP_3,
		AMP_4,
		AMP_MAX
	}

	public enum OCNPIN_CN
	{
		OCNPIN_CN8_4,
		OCNPIN_CN8_6,
		OCNPIN_CN9_4,
		OCNPIN_CN9_6,
		OCNPIN_CN10_1,
		OCNPIN_CN10_2,
		OCNPIN_CN10_3,
		OCNPIN_CN10_4,
		OCNPIN_CN10_5,
		OCNPIN_CN10_6,
		OCNPIN_CN10_7,
		OCNPIN_CN10_8,
		OCNPIN_CN11_3,
		OCNPIN_CN11_4,
		OCNPIN_CN11_5,
		OCNPIN_CN11_6,
		OCNPIN_CN11_7,
		OCNPIN_CN11_8,
		OCNPIN_CN11_9,
		OCNPIN_CN11_10,
		OCNPIN_CN12_3,
		OCNPIN_CN12_4,
		OCNPIN_CN12_5,
		OCNPIN_CN12_6,
		OCNPIN_CN12_7,
		OCNPIN_CN12_8,
		OCNPIN_CN12_9,
		OCNPIN_CN12_10,
		OCNPIN_CN14_1,
		OCNPIN_CN14_2,
		OCNPIN_CN14_3,
		OCNPIN_CN14_4,
		OCNPIN_CN19_1,
		OCNPIN_CN19_2,
		OCNPIN_CN19_3,
		OCNPIN_CN19_4,
		OCNPIN_CN19_5,
		OCNPIN_CN19_6,
		OCNPIN_CN19_7
	}

	public enum TAPELED
	{
		dummy0,
		TAPELED_CTRL_PANEL,
		TAPELED_LEFT_PANEL,
		TAPELED_RIGHT_PANEL,
		TAPELED_MAX
	}

	public enum TAPELED_CNT
	{
		TAPELED_CNT_MAX = 0x800
	}

	public enum IORESET
	{
		IORESET_EXIO_AB = 1,
		IORESET_COIN_JAM = 0x10
	}

	public enum ICNPIN_CN
	{
		ICNPIN_CN8_8,
		ICNPIN_CN8_9,
		ICNPIN_CN8_10,
		ICNPIN_CN9_8,
		ICNPIN_CN9_9,
		ICNPIN_CN9_10,
		ICNPIN_CN11_11,
		ICNPIN_CN11_12,
		ICNPIN_CN11_13,
		ICNPIN_CN11_14,
		ICNPIN_CN11_15,
		ICNPIN_CN11_16,
		ICNPIN_CN11_17,
		ICNPIN_CN11_18,
		ICNPIN_CN11_19,
		ICNPIN_CN11_20,
		ICNPIN_CN12_11,
		ICNPIN_CN12_12,
		ICNPIN_CN12_13,
		ICNPIN_CN12_14,
		ICNPIN_CN12_15,
		ICNPIN_CN12_16,
		ICNPIN_CN12_17,
		ICNPIN_CN12_18,
		ICNPIN_CN12_19,
		ICNPIN_CN12_20,
		ICNPIN_CN12_21,
		ICNPIN_CN12_22,
		ICNPIN_CN12_23,
		ICNPIN_CN12_24,
		ICNPIN_CN15_3,
		ICNPIN_CN15_4,
		ICNPIN_CN15_5,
		ICNPIN_CN15_6,
		ICNPIN_CN15_7,
		ICNPIN_CN15_8,
		ICNPIN_CN15_9,
		ICNPIN_CN15_10,
		ICNPIN_CN15_11,
		ICNPIN_CN15_12,
		ICNPIN_CN15_13,
		ICNPIN_CN15_14,
		ICNPIN_CN15_15,
		ICNPIN_CN15_16,
		ICNPIN_CN15_17,
		ICNPIN_CN15_18,
		ICNPIN_CN15_19,
		ICNPIN_CN15_20,
		ICNPIN_CN19_8,
		ICNPIN_CN19_9,
		ICNPIN_CN19_10,
		ICNPIN_CN19_11,
		ICNPIN_CN19_12,
		ICNPIN_CN19_13,
		ICNPIN_CN19_14,
		ICNPIN_CN19_15
	}

	public struct ICNPIN
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
		public ushort[] Ain;

		public ulong CnPin;
	}

	public struct INPUTDATA
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 241)]
		public byte[] Data;
	}

	public struct OUTPUTDATA
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 48)]
		public byte[] Data;
	}

	public struct IORESETDATA
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
		public byte[] Data;
	}

	public struct SETTING_COIN
	{
		public byte m_Connector;

		public byte m_Pin;

		public byte m_OnTime;

		public byte m_OffTime;

		public ushort m_JamTimeout;
	}

	public struct SETTING_COUNTER
	{
		public byte m_Connector;

		public byte m_Pin;

		public ushort m_OnTime;

		public ushort m_OffTime;
	}

	public struct SETTING_AMPVOL
	{
		public byte m_CnDown;

		public byte m_PinDown;

		public byte m_CnUp;

		public byte m_PinUp;

		public byte m_CnMute;

		public byte m_PinMute;

		public ushort m_OnTime;

		public ushort m_OffTime;
	}

	public struct SETTING
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
		public SETTING_COIN[] m_aCoin;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
		public SETTING_COUNTER[] m_aCounter;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
		public SETTING_AMPVOL[] m_aAmpVol;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
		public ushort[] m_aTapeLed;

		public ushort m_IoInterval;

		public byte m_TestFirm;

		public byte m_bEnableDbgMon;

		public byte m_DbgMonPort;

		public byte m_bEnableSciCmd;

		public byte m_SciCmdPort;

		public byte m_bEnablePcPwrCtl;
	}

	public struct INPUT
	{
		public byte DevIoCounter;

		public byte bExIoAErr;

		public byte bExIoBErr;

		public byte bPcPowerOn;

		public byte bPcPowerCheck;

		public byte bCoin1Jam;

		public byte bCoin2Jam;

		public byte bCoin3Jam;

		public byte bCoin4Jam;

		public byte Coin1Count;

		public byte Coin2Count;

		public byte Coin3Count;

		public byte Coin4Count;

		public ushort AnalogCh1;

		public ushort AnalogCh2;

		public ushort AnalogCh3;

		public ushort AnalogCh4;

		public byte CN8_8;

		public byte CN8_9;

		public byte CN8_10;

		public byte CN9_8;

		public byte CN9_9;

		public byte CN9_10;

		public byte CN11_11;

		public byte CN11_12;

		public byte CN11_13;

		public byte CN11_14;

		public byte CN11_15;

		public byte CN11_16;

		public byte CN11_17;

		public byte CN11_18;

		public byte CN11_19;

		public byte CN11_20;

		public byte CN12_11;

		public byte CN12_12;

		public byte CN12_13;

		public byte CN12_14;

		public byte CN12_15;

		public byte CN12_16;

		public byte CN12_17;

		public byte CN12_18;

		public byte CN12_19;

		public byte CN12_20;

		public byte CN12_21;

		public byte CN12_22;

		public byte CN12_23;

		public byte CN12_24;

		public byte CN15_3;

		public byte CN15_4;

		public byte CN15_5;

		public byte CN15_6;

		public byte CN15_7;

		public byte CN15_8;

		public byte CN15_9;

		public byte CN15_10;

		public byte CN15_11;

		public byte CN15_12;

		public byte CN15_13;

		public byte CN15_14;

		public byte CN15_15;

		public byte CN15_16;

		public byte CN15_17;

		public byte CN15_18;

		public byte CN15_19;

		public byte CN15_20;

		public byte CN19_8;

		public byte CN19_9;

		public byte CN19_10;

		public byte CN19_11;

		public byte CN19_12;

		public byte CN19_13;

		public byte CN19_14;

		public byte CN19_15;
	}

	public struct DEVSTATUS
	{
		public byte InputCounter;

		public byte OutputCounter;

		public byte TapeLedCounter;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
		public byte[] TapeLedRate;

		public INPUT Input;

		public INPUTDATA InputData;

		public OUTPUTDATA OutputData;

		public IORESETDATA IoResetData;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 18)]
		public ICNPIN[] ICnPinHist;

		public SETTING Setting;
	}

	public DEVSTATUS m_DevStatus;

	[DllImport("aio-iob2_video")]
	public unsafe static extern AIO_IOB2_BI2X_AC1* aioIob2Bi2xAC1_Create(AIO_NMGR_IOB2* i_pNodeMgr, uint i_DevId, ref SETTING i_pSetting, uint i_cbSetting);

	[DllImport("aio-iob2_video")]
	public unsafe static extern uint aioIob2Bi2xAC1_GetDeviceStatus(AIO_IOB2_BI2X_AC1* i_pNodeCtl, ref DEVSTATUS o_pDevStatus, uint i_cbDevStatus);

	[DllImport("aio-iob2_video")]
	public unsafe static extern void aioIob2Bi2xAC1_IoReset(AIO_IOB2_BI2X_AC1* i_pNodeCtl, uint i_bfIoReset);

	[DllImport("aio-iob2_video")]
	public unsafe static extern void aioIob2Bi2xAC1_SetWatchDogTimer(AIO_IOB2_BI2X_AC1* i_pNodeCtl, byte i_Count);

	[DllImport("aio-iob2_video")]
	public unsafe static extern void aioIob2Bi2xAC1_AddCounter(AIO_IOB2_BI2X_AC1* i_pNodeCtl, uint i_Counter, uint i_Count);

	[DllImport("aio-iob2_video")]
	public unsafe static extern void aioIob2Bi2xAC1_SetAmpVolume(AIO_IOB2_BI2X_AC1* i_pNodeCtl, uint i_Amp, uint i_Volume);

	[DllImport("aio-iob2_video")]
	public unsafe static extern void aioIob2Bi2xAC1_SetOutputData(AIO_IOB2_BI2X_AC1* i_pNodeCtl, uint i_CnPin, byte i_Data);

	[DllImport("aio-iob2_video")]
	public unsafe static extern void aioIob2Bi2xAC1_SetTapeLedDataPart(AIO_IOB2_BI2X_AC1* i_pNodeCtl, uint i_TapeLedCh, uint i_Offset, byte[] i_pData, uint i_cntTapeLed, bool i_bReverse);

	public unsafe aioIob2Bi2xAC1(AIO_IOB2_BI2X_AC1* i_pNodeCtl)
		: base((AIO_IOB2_BI2X*)i_pNodeCtl)
	{
	}

	public unsafe static aioIob2Bi2xAC1 Create(aioNMgrIob2 i_pNodeMgr, uint i_DevId, ref SETTING i_pSetting)
	{
		AIO_IOB2_BI2X_AC1* ptr = aioIob2Bi2xAC1_Create((AIO_NMGR_IOB2*)i_pNodeMgr.m_pNodeMgr, i_DevId, ref i_pSetting, (uint)Marshal.SizeOf(typeof(SETTING)));
		if (ptr == null)
		{
			return null;
		}
		return new aioIob2Bi2xAC1(ptr);
	}

	public unsafe virtual void GetDeviceStatus()
	{
		aioIob2Bi2xAC1_GetDeviceStatus((AIO_IOB2_BI2X_AC1*)m_pNodeCtl, ref m_DevStatus, (uint)Marshal.SizeOf(typeof(DEVSTATUS)));
	}

	public unsafe void IoReset(IORESET i_bfIoReset)
	{
		aioIob2Bi2xAC1_IoReset((AIO_IOB2_BI2X_AC1*)m_pNodeCtl, (uint)i_bfIoReset);
	}

	public unsafe void SetWatchDogTimer(byte i_Count)
	{
		aioIob2Bi2xAC1_SetWatchDogTimer((AIO_IOB2_BI2X_AC1*)m_pNodeCtl, i_Count);
	}

	public unsafe void AddCounter(COUNTER i_Counter, uint i_Count)
	{
		aioIob2Bi2xAC1_AddCounter((AIO_IOB2_BI2X_AC1*)m_pNodeCtl, (uint)i_Counter, i_Count);
	}

	public unsafe void SetAmpVolume(AMP i_Amp, uint i_Volume)
	{
		aioIob2Bi2xAC1_SetAmpVolume((AIO_IOB2_BI2X_AC1*)m_pNodeCtl, (uint)i_Amp, i_Volume);
	}

	public unsafe void SetOutputData(OCNPIN_CN i_CnPin, byte i_Data)
	{
		aioIob2Bi2xAC1_SetOutputData((AIO_IOB2_BI2X_AC1*)m_pNodeCtl, (uint)i_CnPin, i_Data);
	}

	public unsafe void SetTapeLedDataPart(uint i_TapeLedCh, uint i_Offset, byte[] i_pData, uint i_cntTapeLed, bool i_bReverse)
	{
		aioIob2Bi2xAC1_SetTapeLedDataPart((AIO_IOB2_BI2X_AC1*)m_pNodeCtl, i_TapeLedCh, i_Offset, i_pData, i_cntTapeLed, i_bReverse);
	}
}
