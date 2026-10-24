using System.Linq;
using EagDef;
using KAMUNITY;

public class EagCore
{
	private static EagCore instance;

	private bool m_isEaModeOn = true;

	private bool m_isOfflineMode;

	private EA_ERROR m_error;

	private bool m_bInitialized;

	private bool m_isStop;

	private bool m_forceError;

	private int m_forceErrorCode = 2002;

	public static EagCore Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new EagCore();
			}
			return instance;
		}
	}

	public int Code { get; private set; }

	public int SubCode { get; private set; }

	public STATUS Status { get; private set; }

	public bool IsInitialized => m_bInitialized;

	public bool Init()
	{
		bool flag = false;
		AvsTypes.EA3_BOOT_CODE boot_code;
		int boot_subcode;
		while (Ea3GetBootStatus(out boot_code, out boot_subcode) == AvsTypes.EA3_BOOT_STATUS.BOOT)
		{
		}
		bool flag2 = false;
		if (!m_isEaModeOn)
		{
			flag2 = true;
		}
		else if (m_forceError)
		{
			flag2 = false;
			boot_code = (AvsTypes.EA3_BOOT_CODE)m_forceErrorCode;
			boot_subcode = 0;
		}
		else
		{
			flag2 = boot_code == AvsTypes.EA3_BOOT_CODE.OK;
		}
		if (!flag2)
		{
			if (!m_isOfflineMode)
			{
				Code = (int)boot_code;
				SubCode = boot_subcode;
			}
			flag = false;
		}
		else
		{
			flag = true;
		}
		m_bInitialized = true;
		return flag;
	}

	public static AvsTypes.EA3_BOOT_STATUS Ea3GetBootStatus(out AvsTypes.EA3_BOOT_CODE boot_code, out int boot_subcode)
	{
		return AvsEa3.Ea3GetBootStatus(out boot_code, out boot_subcode);
	}

	public static AvsTypes.EA3_STATUS Ea3GetStatus(out int code, out int subcode)
	{
		AvsTypes.EA3_STATUS result = AvsEa3.Ea3GetStatus(out code, out subcode);
		if (DebugFlag.IsActive(DebugFlag.Flag.ForceNetworkDown))
		{
			result = AvsTypes.EA3_STATUS.DOWN;
			code = 0;
			subcode = 0;
		}
		return result;
	}

	public static AvsTypes.EA3_PP_STATUS Ea3GetPpStatus()
	{
		AvsTypes.EA3_PP_STATUS result = AvsEa3.Ea3GetPpStatus();
		if (DebugFlag.IsActive(DebugFlag.Flag.ForcePPErrror))
		{
			result = AvsTypes.EA3_PP_STATUS.NOT_AVAILABLE;
		}
		return result;
	}

	public void UpdateStatus()
	{
		Status = STATUS.OK;
		m_error = EA_ERROR.NONE;
		Code = 0;
		SubCode = 0;
		if (!m_isEaModeOn)
		{
			Status = STATUS.LOCAL;
		}
		else if (m_isOfflineMode)
		{
			Status = STATUS.OFFLINE;
		}
		else if (!m_bInitialized)
		{
			Status = STATUS.SETUP;
		}
		else
		{
			if (m_isStop)
			{
				return;
			}
			AvsTypes.EA3_BOOT_CODE boot_code;
			int boot_subcode;
			AvsTypes.EA3_BOOT_STATUS eA3_BOOT_STATUS = Ea3GetBootStatus(out boot_code, out boot_subcode);
			if (eA3_BOOT_STATUS != 0)
			{
				Code = (int)boot_code;
				SubCode = boot_subcode;
				switch (boot_code)
				{
				case AvsTypes.EA3_BOOT_CODE.PCBTRACKER_CONFIRM:
					Status = STATUS.NO_CONFIRM;
					Code = 2012;
					m_error = EA_ERROR.NO_CONFIRM;
					return;
				case AvsTypes.EA3_BOOT_CODE.PCBTRACKER_CONTACT:
					Status = STATUS.OFFLINE;
					Code = 2015;
					return;
				case AvsTypes.EA3_BOOT_CODE.PCBTRACKER_REVISION:
					Status = STATUS.OFFLINE;
					Code = 2014;
					return;
				}
				if (eA3_BOOT_STATUS == AvsTypes.EA3_BOOT_STATUS.RESUME && AvsEa3.NetCtrlAddress().All((AvsTypes.avs_net_in4_addr o) => o.addr8.All((byte p) => p == 0) || (o.addr8[0] == 169 && o.addr8[1] == 254)))
				{
					Status = STATUS.OFFLINE;
					Code = 2000;
					SubCode = 0;
					return;
				}
				if (eA3_BOOT_STATUS == AvsTypes.EA3_BOOT_STATUS.RESUME)
				{
					Status = STATUS.SETUP;
				}
			}
			if (eA3_BOOT_STATUS != AvsTypes.EA3_BOOT_STATUS.DONE)
			{
				return;
			}
			int code;
			int subcode;
			AvsTypes.EA3_STATUS eA3_STATUS = Ea3GetStatus(out code, out subcode);
			if (code == 2012)
			{
				Status = STATUS.NO_CONFIRM;
				m_error = EA_ERROR.NO_CONFIRM;
				return;
			}
			if (m_forceError)
			{
				Status = STATUS.DOWN;
				return;
			}
			switch (eA3_STATUS)
			{
			case AvsTypes.EA3_STATUS.ONLINE:
				Status = STATUS.OK;
				break;
			case AvsTypes.EA3_STATUS.BOOT:
				Status = STATUS.SETUP;
				break;
			case AvsTypes.EA3_STATUS.DOWN:
				Status = STATUS.DOWN;
				break;
			case AvsTypes.EA3_STATUS.OFFLINE:
				Status = STATUS.OFFLINE;
				Code = code;
				SubCode = subcode;
				break;
			case AvsTypes.EA3_STATUS.MAINTE:
				Status = STATUS.MAINTE;
				break;
			case AvsTypes.EA3_STATUS.RUNTIME_ERROR:
				Status = STATUS.RUNTIME_ERROR;
				break;
			case AvsTypes.EA3_STATUS.DISABLED:
				Status = STATUS.DISABLED;
				break;
			default:
				Status = STATUS.OK;
				break;
			}
		}
	}
}
