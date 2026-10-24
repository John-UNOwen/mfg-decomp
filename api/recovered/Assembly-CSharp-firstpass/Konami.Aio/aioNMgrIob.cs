using System.Runtime.InteropServices;

namespace Konami.Aio;

public class aioNMgrIob : aioNodeMgr
{
	public enum MODEFLAG
	{
		MF_ENABLE_SENDRECV_FAILED_RETRY = 1
	}

	public struct COMMSTATUS
	{
		public ushort cntOpened;

		public ulong cbSend;

		public ulong cbRecv;

		public uint cntSend;

		public uint cntRecv;

		public uint cntSendErr;

		public uint cntRecvErr;

		public ulong cbRecvDiscard;

		public uint cntPktSend;

		public uint cntPktRecv;

		public uint cntRecvPktDiscard;

		public uint cntRecvPktLost;

		public uint cntSendRecvFailed;
	}

	public struct FIRMVER
	{
		public byte Major;

		public byte Minor;

		public byte Revision;
	}

	public struct FIRMINFO
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
		public byte[] Type;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
		public byte[] Flag;

		public FIRMVER FirmVer;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
		public sbyte[] PCode;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
		public byte[] BuildTime;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
		public byte[] ExFlags;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
		public byte[] PRomSum;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
		public byte[] BRomSum;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
		public sbyte[] Detail;
	}

	public struct NODEINFO
	{
		public byte NodeIdSetResult;

		public byte NodeId;

		public byte DevId;

		public FIRMINFO FirmInfo;
	}

	public COMMSTATUS m_CommStatus;

	[DllImport("libaio-iob")]
	public unsafe static extern AIO_NMGR_IOB* aioNMgrIob_Create(AIO_SCI* i_pSci, uint i_bfMode);

	[DllImport("libaio-iob")]
	public unsafe static extern uint aioNMgrIob_GetCommStatus(AIO_NMGR_IOB* i_pNodeMgr, ref COMMSTATUS o_pCommStatus, uint i_cbCommStatus);

	[DllImport("libaio-iob")]
	public unsafe static extern void aioNMgrIob_BeginManage(AIO_NMGR_IOB* i_pNodeMgr);

	public unsafe aioNMgrIob(AIO_NMGR_IOB* i_pNodeMgr)
		: base((AIO_NMGR*)i_pNodeMgr)
	{
	}

	public unsafe static aioNMgrIob Create(aioSci i_pSci, MODEFLAG i_bfMode)
	{
		AIO_NMGR_IOB* ptr = aioNMgrIob_Create(i_pSci.m_pSci, (uint)i_bfMode);
		if (ptr == null)
		{
			return null;
		}
		return new aioNMgrIob(ptr);
	}

	public unsafe void GetCommStatus()
	{
		aioNMgrIob_GetCommStatus((AIO_NMGR_IOB*)m_pNodeMgr, ref m_CommStatus, (uint)Marshal.SizeOf(typeof(COMMSTATUS)));
	}

	public unsafe void BeginManage()
	{
		aioNMgrIob_BeginManage((AIO_NMGR_IOB*)m_pNodeMgr);
	}
}
