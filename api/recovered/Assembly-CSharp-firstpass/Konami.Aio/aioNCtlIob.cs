using System.Runtime.InteropServices;

namespace Konami.Aio;

public class aioNCtlIob : aioNodeCtl
{
	public aioNMgrIob.NODEINFO m_NodeInfo;

	[DllImport("libaio-iob")]
	public unsafe static extern uint aioNCtlIob_GetNodeInfo(AIO_NCTL_IOB* i_pNodeCtl, ref aioNMgrIob.NODEINFO o_pNodeInfo, uint i_cbNodeInfo);

	public unsafe aioNCtlIob(AIO_NCTL_IOB* i_pNodeCtl)
		: base((AIO_NCTL*)i_pNodeCtl)
	{
	}

	public unsafe void GetNodeInfo()
	{
		aioNCtlIob_GetNodeInfo((AIO_NCTL_IOB*)m_pNodeCtl, ref m_NodeInfo, (uint)Marshal.SizeOf(typeof(aioNMgrIob.NODEINFO)));
	}
}
