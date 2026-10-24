namespace Konami.System.Net;

public interface IUserImplementsClass
{
	string GetWord(Word word);

	void OnRequestFailedDefault(string title, string message);

	void OnForceTransitGeneralError(string title, string message);

	void OnForceTransitApplicationUpdate(string title, string message, string appStoreUrl);

	void OnForceTransitAssetUpdate(string title, string message);

	void OnForceTransitUnderMaintenance(string title, string message, string starttime, string endtime);

	void OnForceTransitAccountBan(string message);

	void OnAccountFileCreateFailed(string message);

	void OnAskRetryOrRestart(string title, string message, OnPushRetry onPushRetry, OnPushRestart onPushRestart);

	string GetDeviceHash();

	string GetAssetBundleVersion();

	string GetApplicationVersion();

	void OnChangeNotificationSetting(bool bOn);

	int GetPolicyRevision(Policy policy);

	Platform GetPlatform();
}
