using EagDef;
using KAMUNITY;

public class EagUtility
{
	private static bool m_isNotAgree = true;

	public static bool IsNeedConfirm()
	{
		if (EagCore.Instance.Status == STATUS.NO_CONFIRM)
		{
			return m_isNotAgree;
		}
		return false;
	}

	public static void ConfirmAgree()
	{
		AvsEa3.Ea3PcbtrackerReportLicenseAgree();
		m_isNotAgree = false;
	}
}
