using System;

namespace JubeatPlus;

[Serializable]
public class ServiceStatus
{
	public string FrontServer { get; private set; }

	public string ServiceEntryPoint { get; private set; }

	public string ExternalContentServer { get; private set; }

	public string PlatformStoreLink { get; private set; }

	public bool IsAdmobServiceValid { get; private set; }

	public string ContentsPolicyLink { get; private set; }

	public string PrivacyPolicyLink { get; private set; }

	public string VirtualCoinPolicyLink { get; private set; }

	public string FundamentalPolicyLink { get; private set; }

	public string SpecifiedlTransactionPolicyLink { get; private set; }

	public string AdvertisePolicyLink { get; private set; }

	public string LicenseLink { get; private set; }

	public string ProhibitionLink { get; private set; }

	public string SupportEntryPoint { get; private set; }

	public string OfficialCorporateLink { get; private set; }

	public static ServiceStatus Default { get; private set; } = new ServiceStatus();


	public ServiceStatus(string frontServer = null, string serviceEntryPoint = null, string externalContentServer = null, string platformStoreLink = null, bool isAdmobServiceValid = false, string contentsPolicyLink = null, string privacyPolicyLink = null, string virtualCoinPolicyLink = null, string specifiedlTransactionPolicyLink = null, string fundamentalPolicyLink = null, string licenseLink = null, string prohibitionPolicyLink = null, string advertisePolicyLink = null, string supportEntryPoint = null, string officialCorporateLink = null)
	{
		FrontServer = frontServer;
		ServiceEntryPoint = serviceEntryPoint;
		ExternalContentServer = externalContentServer;
		PlatformStoreLink = platformStoreLink;
		IsAdmobServiceValid = isAdmobServiceValid;
		ContentsPolicyLink = contentsPolicyLink;
		VirtualCoinPolicyLink = virtualCoinPolicyLink;
		PrivacyPolicyLink = privacyPolicyLink;
		SpecifiedlTransactionPolicyLink = specifiedlTransactionPolicyLink;
		FundamentalPolicyLink = fundamentalPolicyLink;
		LicenseLink = licenseLink;
		AdvertisePolicyLink = advertisePolicyLink;
		ProhibitionLink = prohibitionPolicyLink;
		SupportEntryPoint = supportEntryPoint;
		OfficialCorporateLink = officialCorporateLink;
	}
}
