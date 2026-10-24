using System.Runtime.InteropServices;

namespace Konami.Aio;

public class aioSci
{
	public struct COMMSTATUS
	{
		public ushort cntOpened;

		public ulong cbSend;

		public ulong cbRecv;

		public uint cntSend;

		public uint cntRecv;

		public uint cntSendErr;

		public uint cntRecvErr;
	}

	public unsafe AIO_SCI* m_pSci = null;

	public COMMSTATUS m_CommStatus;

	[DllImport("libaio")]
	public unsafe static extern void aioSci_Destroy(AIO_SCI* i_pSci);

	[DllImport("libaio")]
	public unsafe static extern uint aioSci_GetCommStatus(AIO_SCI* i_pSci, ref COMMSTATUS o_pCommStatus, uint i_cbCommStatus);

	public unsafe aioSci(AIO_SCI* i_pSci)
	{
		m_pSci = i_pSci;
	}

	~aioSci()
	{
		Destroy();
	}

	public unsafe void Destroy()
	{
		if (m_pSci != null)
		{
			aioSci_Destroy(m_pSci);
			m_pSci = null;
		}
	}

	public unsafe void GetCommStatus()
	{
		aioSci_GetCommStatus(m_pSci, ref m_CommStatus, (uint)Marshal.SizeOf(typeof(COMMSTATUS)));
	}
}
