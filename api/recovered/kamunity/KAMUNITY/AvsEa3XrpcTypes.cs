using System;

namespace KAMUNITY;

public static class AvsEa3XrpcTypes
{
	[Serializable]
	public struct XRPC_STATUS_DATA
	{
		public short status;

		public short subcode;

		public short status_code;

		public short fault_code;

		public XRPC_STATUS_DATA(short _status, short _subcode, short _status_code, short _fault_code)
		{
			status = _status;
			subcode = _subcode;
			status_code = _status_code;
			fault_code = _fault_code;
		}

		public XRPC_STATUS GetStatus()
		{
			return (XRPC_STATUS)status;
		}

		public XRPC_SUBCODE GetSubcode()
		{
			return (XRPC_SUBCODE)subcode;
		}

		public XPRC_STATUS_CODE GetStatusCode()
		{
			return (XPRC_STATUS_CODE)status_code;
		}

		public XRPC_FAULT_CODE GetFaultCode()
		{
			return (XRPC_FAULT_CODE)fault_code;
		}

		public void Clear()
		{
			status = 0;
			subcode = 0;
			status_code = 0;
			fault_code = 0;
		}

		public bool HasError()
		{
			if (GetStatus() != 0 || GetStatusCode() != 0 || GetFaultCode() != 0)
			{
				return true;
			}
			return false;
		}
	}

	public enum XRPC_STATUS
	{
		OK = 0,
		EDATA = -10,
		EHTTP = -11,
		ENOBUFS = -12,
		ENORES = -13,
		EURL = -14,
		EINPROGRESS = -15,
		EPARSE = -16,
		EFAULT = -17,
		ESTATUS = -18,
		ENOHDL = -19,
		ENOCRPLV = -20,
		ELONGKEY = -21,
		EUNSYMCRP = -22,
		ENOCOMPR = -23,
		EREQUEST = -99,
		EOFFLINE = -100
	}

	public enum XRPC_SUBCODE
	{
		OTHER = 900,
		SOCKET = 1000,
		HTTP = 2000
	}

	public enum XPRC_STATUS_CODE
	{
		OK = 0,
		EPCBID = 100,
		ELOCATION = 101,
		EMODEL = 102,
		EKEY = 103,
		EASGNEND = 104,
		EDOCTYPE = 105,
		EARGRANGE = 106,
		EUPDATE = 107,
		EATTRIB = 108,
		ENRCARDID = 109,
		EXPCARDID = 110,
		EPASTTIME = 111,
		ENOTTIED = 112,
		EDUPUSER = 113,
		EDUPCARD = 114,
		ENOUSER = 115,
		EPASSWD = 116,
		EOUTOFSERV = 117,
		ETOOLONG = 118,
		EUNEXPECTED = 119,
		ENAME = 120,
		ESERVEND = 121,
		ENOMAP = 122,
		ENODATA = 123,
		EALREADY = 124,
		ERPCFAIL = 125,
		ECONFIG = 126,
		EOPAGREE = 127,
		EMAINTE = 128,
		ELOGIN = 129,
		ESID = 130,
		ESUSPID = 131,
		EPERMDENIED = 132,
		EREVISION = 133,
		ECONTRACT = 134,
		EDEPLOY = 135,
		EURGENT = 136,
		ETERM = 137,
		EPREPSERV = 138,
		ELACKOFPTS = 139,
		ESORRY = 140,
		EDBINIT = 200,
		EDBSELECT = 201,
		EDBSELECTM = 202,
		EDBUPDATE = 203,
		EDBUPDATEM = 204,
		EDBREGISTER = 205,
		EDBCOMMIT = 206,
		EDBLOCREAT = 207,
		EDBLOUNLINK = 208,
		EDBLOOPEN = 209,
		EDBLOCLOSE = 210,
		EDBLOREAD = 211,
		EDBLOWRITE = 212,
		EDBUNEXPECTED = 213
	}

	public enum XRPC_FAULT_CODE
	{
		FNONE = 0,
		FPARSE = 100,
		FMODULE = 200,
		FMETHOD = 201
	}
}
