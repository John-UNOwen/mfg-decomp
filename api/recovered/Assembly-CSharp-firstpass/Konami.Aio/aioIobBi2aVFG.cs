using System.Runtime.InteropServices;

namespace Konami.Aio;

public class aioIobBi2aVFG : aioIobBi2a
{
	public new enum CONSTVAL
	{
		SIZEOF_LEDDATA = 90,
		VOLUME_MIN = 96
	}

	public enum MECHTYPE
	{
		MECHTYPE_UNKNOWN,
		MECHTYPE_A,
		MECHTYPE_B,
		MECHTYPE_C,
		MECHTYPE_D,
		MECHTYPE_HG,
		MECHTYPE_UKS,
		MECHTYPE_TBS,
		MECHTYPE_UJK
	}

	public enum SLOT
	{
		SLOT_1
	}

	public enum COUNTER
	{
		COUNTER_COIN
	}

	public enum AMP
	{
		AMP_HP_L,
		AMP_HP_R
	}

	public enum LAMP
	{
		LAMP_JCCC1,
		LAMP_JCCC2,
		LAMP_JCCC3,
		LAMP_JCCC4
	}

	public struct INPUT
	{
		public byte bPcPowerCheck;

		public byte CoinCount;

		public byte bTest;

		public byte bService;

		public byte bCoinSw;

		public byte bCoinJam;

		public byte bCabinet;

		public byte bHPDetect;

		public byte bRecDetect;

		public byte bStart;
	}

	public new struct DEVSTATUS
	{
		public byte MechType;

		public byte bIsBi2a;

		public byte IoCounter;

		public INPUT Input;

		public INPUTDATA InputData;

		public OUTPUTDATA OutputData;
	}

	public new DEVSTATUS m_DevStatus;

	[DllImport("libaio-iob_video")]
	public unsafe static extern AIO_IOB_BI2A_VFG* aioIobBi2aVFG_Create(AIO_NMGR_IOB* i_pNodeMgr, uint i_DevId);

	[DllImport("libaio-iob_video")]
	public unsafe static extern uint aioIobBi2aVFG_GetDeviceStatus(AIO_IOB_BI2A_VFG* i_pNodeCtl, ref DEVSTATUS o_pDevStatus, uint i_cbDevStatus);

	[DllImport("libaio-iob_video")]
	public unsafe static extern void aioIobBi2aVFG_SetMechType(AIO_IOB_BI2A_VFG* i_pNodeCtl, uint i_MechType);

	[DllImport("libaio-iob_video")]
	public unsafe static extern void aioIobBi2aVFG_SetWatchDogTimer(AIO_IOB_BI2A_VFG* i_pNodeCtl, ushort i_Count);

	[DllImport("libaio-iob_video")]
	public unsafe static extern void aioIobBi2aVFG_ControlCoinBlocker(AIO_IOB_BI2A_VFG* i_pNodeCtl, uint i_Slot, bool i_bOpen);

	[DllImport("libaio-iob_video")]
	public unsafe static extern void aioIobBi2aVFG_AddCounter(AIO_IOB_BI2A_VFG* i_pNodeCtl, uint i_Counter, uint i_Count);

	[DllImport("libaio-iob_video")]
	public unsafe static extern void aioIobBi2aVFG_SetAmpVolume(AIO_IOB_BI2A_VFG* i_pNodeCtl, uint i_Amp, uint i_Volume);

	[DllImport("libaio-iob_video")]
	public unsafe static extern void aioIobBi2aVFG_EnableUsbCharger(AIO_IOB_BI2A_VFG* i_pNodeCtl, bool i_bEnable);

	[DllImport("libaio-iob_video")]
	public unsafe static extern void aioIobBi2aVFG_SetLamp(AIO_IOB_BI2A_VFG* i_pNodeCtl, uint i_Lamp, byte i_Bright);

	[DllImport("libaio-iob_video")]
	public unsafe static extern void aioIobBi2aVFG_SetIccrLed(AIO_IOB_BI2A_VFG* i_pNodeCtl, uint i_RGB);

	[DllImport("libaio-iob_video")]
	public unsafe static extern void aioIobBi2aVFG_SetLedData(AIO_IOB_BI2A_VFG* i_pNodeCtl, byte[] i_pData, uint i_cbData);

	public unsafe aioIobBi2aVFG(AIO_IOB_BI2A_VFG* i_pNodeCtl)
		: base((AIO_IOB_BI2A*)i_pNodeCtl)
	{
	}

	public unsafe static aioIobBi2aVFG Create(aioNMgrIob i_pNodeMgr, uint i_DevId)
	{
		AIO_IOB_BI2A_VFG* ptr = aioIobBi2aVFG_Create((AIO_NMGR_IOB*)i_pNodeMgr.m_pNodeMgr, i_DevId);
		if (ptr == null)
		{
			return null;
		}
		return new aioIobBi2aVFG(ptr);
	}

	public unsafe override void GetDeviceStatus()
	{
		base.GetDeviceStatus();
		aioIobBi2aVFG_GetDeviceStatus((AIO_IOB_BI2A_VFG*)m_pNodeCtl, ref m_DevStatus, (uint)Marshal.SizeOf(typeof(DEVSTATUS)));
	}

	public unsafe void SetMechType(MECHTYPE i_MechType)
	{
		aioIobBi2aVFG_SetMechType((AIO_IOB_BI2A_VFG*)m_pNodeCtl, (uint)i_MechType);
	}

	public new unsafe void SetWatchDogTimer(ushort i_Count)
	{
		aioIobBi2aVFG_SetWatchDogTimer((AIO_IOB_BI2A_VFG*)m_pNodeCtl, i_Count);
	}

	public unsafe void ControlCoinBlocker(SLOT i_Slot, bool i_bOpen)
	{
		aioIobBi2aVFG_ControlCoinBlocker((AIO_IOB_BI2A_VFG*)m_pNodeCtl, (uint)i_Slot, i_bOpen);
	}

	public unsafe void AddCounter(COUNTER i_Counter, uint i_Count)
	{
		aioIobBi2aVFG_AddCounter((AIO_IOB_BI2A_VFG*)m_pNodeCtl, (uint)i_Counter, i_Count);
	}

	public unsafe void SetAmpVolume(AMP i_Amp, uint i_Volume)
	{
		aioIobBi2aVFG_SetAmpVolume((AIO_IOB_BI2A_VFG*)m_pNodeCtl, (uint)i_Amp, i_Volume);
	}

	public unsafe void EnableUsbCharger(bool i_bEnable)
	{
		aioIobBi2aVFG_EnableUsbCharger((AIO_IOB_BI2A_VFG*)m_pNodeCtl, i_bEnable);
	}

	public unsafe void SetLamp(LAMP i_Lamp, byte i_Bright)
	{
		aioIobBi2aVFG_SetLamp((AIO_IOB_BI2A_VFG*)m_pNodeCtl, (uint)i_Lamp, i_Bright);
	}

	public unsafe void SetIccrLed(uint i_RGB)
	{
		aioIobBi2aVFG_SetIccrLed((AIO_IOB_BI2A_VFG*)m_pNodeCtl, i_RGB);
	}

	public new unsafe void SetLedData(byte[] i_pData, uint i_cbData)
	{
		aioIobBi2aVFG_SetLedData((AIO_IOB_BI2A_VFG*)m_pNodeCtl, i_pData, i_cbData);
	}
}
