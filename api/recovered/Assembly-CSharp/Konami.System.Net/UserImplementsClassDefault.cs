using UnityEngine;

namespace Konami.System.Net;

public class UserImplementsClassDefault : IUserImplementsClass
{
	public string GetWord(Word word)
	{
		return word switch
		{
			Word.TitleConnectionFailed => "接続失敗", 
			Word.TitleAppUpdate => "アプリを更新してください", 
			Word.TitleDownloadNotice => "最新のデータがあります。", 
			Word.TitleConnectionFailedAccounBanned => "アカウント停止中", 
			Word.MessageConnectionError => "申し訳ありません。通信に失敗しました。しばらくしてからもう一度お試しください。", 
			Word.MessageConnectionTimeout => "申し訳ありません。通信がタイムアウトしました。しばらくしてからもう一度お試しください。", 
			Word.MessageWaitCommunication => "通信中", 
			Word.ButtonRetry => "リトライ", 
			_ => "", 
		};
	}

	public void OnRequestFailedDefault(string title, string message)
	{
	}

	public void OnForceTransitGeneralError(string title, string message)
	{
		Debug.Log(title + message);
	}

	public void OnForceTransitApplicationUpdate(string title, string message, string openUrl)
	{
		Debug.Log(title + message + openUrl);
		Application.OpenURL(openUrl);
	}

	public void OnForceTransitAssetUpdate(string title, string message)
	{
		Debug.Log(title + message);
	}

	public void OnForceTransitUnderMaintenance(string title, string message, string starttime, string endtime)
	{
		Debug.Log(title + message);
	}

	public void OnForceTransitAccountBan(string message)
	{
		Debug.Log(GetWord(Word.TitleConnectionFailedAccounBanned) + message);
	}

	public string GetDeviceHash()
	{
		return "";
	}

	public string GetAssetBundleVersion()
	{
		return "";
	}

	public string GetApplicationVersion()
	{
		return "1.13.0";
	}

	public void OnChangeNotificationSetting(bool bOn)
	{
	}

	public int GetPolicyRevision(Policy policy)
	{
		return 0;
	}

	public Platform GetPlatform()
	{
		Platform platform = Platform.Other;
		return Application.platform switch
		{
			RuntimePlatform.Android => Platform.Android, 
			RuntimePlatform.IPhonePlayer => Platform.IPhone, 
			_ => Platform.Other, 
		};
	}

	public void OnAskRetryOrRestart(string title, string message, OnPushRetry onPushRetry, OnPushRestart onPushRestart)
	{
		Debug.Log("DialogOk : " + title + " " + message);
		onPushRestart?.Invoke();
	}

	public void OnAccountFileCreateFailed(string message)
	{
		Debug.Log(message);
	}
}
