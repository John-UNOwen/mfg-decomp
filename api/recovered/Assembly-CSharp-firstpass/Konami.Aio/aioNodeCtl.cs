using System.Runtime.InteropServices;

namespace Konami.Aio;

public class aioNodeCtl
{
	public unsafe AIO_NCTL* m_pNodeCtl = null;

	[DllImport("libaio")]
	public unsafe static extern void aioNodeCtl_Destroy(AIO_NCTL* i_pNodeCtl);

	[DllImport("libaio")]
	public unsafe static extern int aioNodeCtl_GetState(AIO_NCTL* i_pNodeCtl);

	[DllImport("libaio")]
	[return: MarshalAs(UnmanagedType.U1)]
	public unsafe static extern bool aioNodeCtl_IsReady(AIO_NCTL* i_pNodeCtl, int i_State);

	[DllImport("libaio")]
	[return: MarshalAs(UnmanagedType.U1)]
	public unsafe static extern bool aioNodeCtl_IsError(AIO_NCTL* i_pNodeCtl, int i_State);

	[DllImport("libaio")]
	public static extern void aioNodeCtl_UpdateDevicesStatus();

	public unsafe aioNodeCtl(AIO_NCTL* i_pNodeCtl)
	{
		m_pNodeCtl = i_pNodeCtl;
	}

	~aioNodeCtl()
	{
		Destroy();
	}

	public unsafe void Destroy()
	{
		if (m_pNodeCtl != null)
		{
			aioNodeCtl_Destroy(m_pNodeCtl);
			m_pNodeCtl = null;
		}
	}

	public unsafe int GetState()
	{
		return aioNodeCtl_GetState(m_pNodeCtl);
	}

	public unsafe bool IsReady(int i_State)
	{
		return aioNodeCtl_IsReady(m_pNodeCtl, i_State);
	}

	public unsafe bool IsError(int i_State)
	{
		return aioNodeCtl_IsError(m_pNodeCtl, i_State);
	}

	public static void UpdateDevicesStatus()
	{
		aioNodeCtl_UpdateDevicesStatus();
	}
}
