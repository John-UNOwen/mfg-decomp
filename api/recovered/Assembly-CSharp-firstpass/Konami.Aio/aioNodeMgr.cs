using System.Runtime.InteropServices;

namespace Konami.Aio;

public class aioNodeMgr
{
	public unsafe AIO_NMGR* m_pNodeMgr = null;

	[DllImport("libaio")]
	public unsafe static extern void aioNodeMgr_Destroy(AIO_NMGR* i_pNodeMgr);

	[DllImport("libaio")]
	public unsafe static extern int aioNodeMgr_GetState(AIO_NMGR* i_pNodeMgr);

	[DllImport("libaio")]
	[return: MarshalAs(UnmanagedType.U1)]
	public unsafe static extern bool aioNodeMgr_IsReady(AIO_NMGR* i_pNodeMgr, int i_State);

	[DllImport("libaio")]
	[return: MarshalAs(UnmanagedType.U1)]
	public unsafe static extern bool aioNodeMgr_IsError(AIO_NMGR* i_pNodeMgr, int i_State);

	public unsafe aioNodeMgr(AIO_NMGR* i_pNodeMgr)
	{
		m_pNodeMgr = i_pNodeMgr;
	}

	~aioNodeMgr()
	{
		Destroy();
	}

	public unsafe void Destroy()
	{
		if (m_pNodeMgr != null)
		{
			aioNodeMgr_Destroy(m_pNodeMgr);
			m_pNodeMgr = null;
		}
	}

	public unsafe int GetState()
	{
		return aioNodeMgr_GetState(m_pNodeMgr);
	}

	public unsafe bool IsReady(int i_State)
	{
		return aioNodeMgr_IsReady(m_pNodeMgr, i_State);
	}

	public unsafe bool IsError(int i_State)
	{
		return aioNodeMgr_IsError(m_pNodeMgr, i_State);
	}
}
