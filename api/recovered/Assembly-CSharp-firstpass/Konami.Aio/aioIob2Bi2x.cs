using System.Runtime.InteropServices;

namespace Konami.Aio;

public class aioIob2Bi2x : aioNCtlIob2
{
	public enum CONSTVAL
	{
		TAPELED_CH_MAX = 8
	}

	public enum WF_IOB
	{
		WF_IOB_BIO2 = 1,
		WF_IOB_BI2A
	}

	[DllImport("libaio-iob2_video")]
	public unsafe static extern AIO_SCI_COMM* aioIob2Bi2x_OpenSciUsbCdc(uint i_SerialNumber);

	[DllImport("libaio-iob2_video")]
	public unsafe static extern AIO_SCI_COMM* aioIob2Bi2x_SciOpenB8PNS1(AIO_IOB2_BI2X* i_pNodeCtl, uint i_Port, uint i_BaudRate);

	[DllImport("libaio-iob2_video")]
	public unsafe static extern void aioIob2Bi2x_SetTapeLedDataGroup(AIO_IOB2_BI2X* i_pNodeCtl, byte i_bfGroup);

	[DllImport("libaio-iob2_video")]
	public unsafe static extern void aioIob2Bi2x_SetTapeLedDataLimit(AIO_IOB2_BI2X* i_pNodeCtl, uint i_Channel, byte i_Scale, byte i_Limit);

	[DllImport("libaio-iob2_video")]
	public unsafe static extern AIO_IOB2_BI2X_WRFIRM* aioIob2Bi2x_CreateWriteFirmContext(uint i_SerialNumber, uint i_bfIob);

	[DllImport("libaio-iob2_video")]
	public unsafe static extern void aioIob2Bi2x_DestroyWriteFirmContext(AIO_IOB2_BI2X_WRFIRM* i_pWrFirm);

	[DllImport("libaio-iob2_video")]
	public unsafe static extern int aioIob2Bi2x_WriteFirmGetState(AIO_IOB2_BI2X_WRFIRM* i_pWrFirm);

	[DllImport("libaio-iob2_video")]
	[return: MarshalAs(UnmanagedType.U1)]
	public static extern bool aioIob2Bi2x_WriteFirmIsCompleted(int i_State);

	[DllImport("libaio-iob2_video")]
	[return: MarshalAs(UnmanagedType.U1)]
	public static extern bool aioIob2Bi2x_WriteFirmIsError(int i_State);

	public unsafe aioIob2Bi2x(AIO_IOB2_BI2X* i_pNodeCtl)
		: base((AIO_NCTL_IOB2*)i_pNodeCtl)
	{
	}

	public unsafe static aioSciComm OpenSciUsbCdc(uint i_SerialNumber)
	{
		AIO_SCI_COMM* ptr = aioIob2Bi2x_OpenSciUsbCdc(i_SerialNumber);
		if (ptr == null)
		{
			return null;
		}
		return new aioSciComm(ptr);
	}

	public unsafe aioSciComm SciOpenB8PNS1(uint i_Port, uint i_BaudRate)
	{
		AIO_SCI_COMM* ptr = aioIob2Bi2x_SciOpenB8PNS1((AIO_IOB2_BI2X*)m_pNodeCtl, i_Port, i_BaudRate);
		if (ptr == null)
		{
			return null;
		}
		return new aioSciComm(ptr);
	}

	public unsafe void SetTapeLedDataGroup(byte i_bfGroup)
	{
		aioIob2Bi2x_SetTapeLedDataGroup((AIO_IOB2_BI2X*)m_pNodeCtl, i_bfGroup);
	}

	public unsafe void SetTapeLedDataLimit(uint i_Channel, byte i_Scale, byte i_Limit)
	{
		aioIob2Bi2x_SetTapeLedDataLimit((AIO_IOB2_BI2X*)m_pNodeCtl, i_Channel, i_Scale, i_Limit);
	}

	public unsafe static AIO_IOB2_BI2X_WRFIRM* CreateWriteFirmContext(uint i_SerialNumber, WF_IOB i_bfIob)
	{
		return aioIob2Bi2x_CreateWriteFirmContext(i_SerialNumber, (uint)i_bfIob);
	}

	public unsafe static void DestroyWriteFirmContext(AIO_IOB2_BI2X_WRFIRM* i_pWrFirm)
	{
		aioIob2Bi2x_DestroyWriteFirmContext(i_pWrFirm);
	}

	public unsafe static int WriteFirmGetState(AIO_IOB2_BI2X_WRFIRM* i_pWrFirm)
	{
		return aioIob2Bi2x_WriteFirmGetState(i_pWrFirm);
	}

	public static bool WriteFirmIsCompleted(int i_State)
	{
		return aioIob2Bi2x_WriteFirmIsCompleted(i_State);
	}

	public static bool WriteFirmIsError(int i_State)
	{
		return aioIob2Bi2x_WriteFirmIsError(i_State);
	}
}
