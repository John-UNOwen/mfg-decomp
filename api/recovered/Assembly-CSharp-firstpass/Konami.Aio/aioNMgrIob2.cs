using System.Runtime.InteropServices;

namespace Konami.Aio;

public class aioNMgrIob2 : aioNMgrIob
{
	[DllImport("libaio-iob")]
	public unsafe static extern AIO_NMGR_IOB2* aioNMgrIob2_Create(AIO_SCI* i_pSci, uint i_bfMode);

	public unsafe aioNMgrIob2(AIO_NMGR_IOB2* i_pNodeMgr)
		: base((AIO_NMGR_IOB*)i_pNodeMgr)
	{
	}

	public new unsafe static aioNMgrIob2 Create(aioSci i_pSci, MODEFLAG i_bfMode)
	{
		AIO_NMGR_IOB2* ptr = aioNMgrIob2_Create(i_pSci.m_pSci, (uint)i_bfMode);
		if (ptr == null)
		{
			return null;
		}
		return new aioNMgrIob2(ptr);
	}
}
