using KAMUNITY;
using SYSTEM;

public class IkeyDevice : IODeviceBase
{
	public Bootstrap.IKeyStatus keyStatus = new Bootstrap.IKeyStatus();

	public bool IsFactoryBoot => SystemUtility.Instance.IsFactoryBoot;

	public bool IsEnableCheck { get; set; } = true;


	public IkeyError CheckSecurityKeyOnBoot()
	{
		if (!GameUtility.IsKamunityBootEnable())
		{
			IsEnableCheck = false;
			return IkeyError.OK;
		}
		Bootstrap.GetIkeyStatus(out var status);
		keyStatus = status;
		if (status.license.nr_token == 0)
		{
			return IkeyError.LICENSE_KEY_NOT_FOUND;
		}
		if (status.license.nr_token > 1)
		{
			return IkeyError.MULTIPLE_LICENSEKEY;
		}
		if (!IsFactoryBoot)
		{
			if (status.account.nr_token == 0)
			{
				return IkeyError.ACCOUNT_KEY_NOT_FOUND;
			}
			if (status.account.nr_token > 1)
			{
				return IkeyError.MULTIPLE_ACCOUNTKEY;
			}
		}
		return IkeyError.OK;
	}

	public IkeyError CheckSecurityKey()
	{
		if (!GameUtility.IsKamunityBootEnable())
		{
			return IkeyError.OK;
		}
		Bootstrap.GetIkeyStatus(out var status);
		keyStatus = status;
		if (status.license.f_disable)
		{
			return IkeyError.LICENSE_KEY_NOT_FOUND;
		}
		if (!IsFactoryBoot && status.account.f_disable)
		{
			return IkeyError.ACCOUNT_KEY_NOT_FOUND;
		}
		return IkeyError.OK;
	}

	public override void Update()
	{
		if (IsEnableCheck && KamunityInstance.isActive)
		{
			IkeyError ikeyError = CheckSecurityKey();
			if (ikeyError != 0)
			{
				ErrorControl.SetError(ERROR.SECURITY, (uint)ikeyError, ERROR_LEVEL.HALT);
			}
		}
	}

	public override void PreEnterTestMode()
	{
	}

	public override void PostExitTestMode()
	{
	}
}
