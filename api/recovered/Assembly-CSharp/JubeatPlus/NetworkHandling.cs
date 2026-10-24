using System.Collections.Generic;
using Konami.System.Net;
using UnityEngine;

namespace JubeatPlus;

public class NetworkHandling : IUserImplementsClass
{
	private Dictionary<Word, string> errorMessages = new Dictionary<Word, string>
	{
		{
			Word.TitleConnectionFailed,
			"接続失敗"
		},
		{
			Word.TitleConnectionFailedAccounBanned,
			"アカウント停止中"
		},
		{
			Word.TitleDownloadNotice,
			"最新のデータがあります"
		},
		{
			Word.TitleAppUpdate,
			"アプリを更新してください"
		},
		{
			Word.MessageConnectionError,
			"申し訳ありません。通信に失敗しました。"
		},
		{
			Word.MessageConnectionTimeout,
			"申し訳ありません。通信に失敗しました"
		},
		{
			Word.MessageWaitCommunication,
			"通信中"
		},
		{
			Word.ButtonRetry,
			"リトライ"
		}
	};

	[SerializeField]
	private readonly string deviceHash = "";

	string IUserImplementsClass.GetApplicationVersion()
	{
		return Application.version;
	}

	string IUserImplementsClass.GetAssetBundleVersion()
	{
		return "";
	}

	string IUserImplementsClass.GetDeviceHash()
	{
		return deviceHash;
	}

	Platform IUserImplementsClass.GetPlatform()
	{
		return Application.platform switch
		{
			RuntimePlatform.Android => Platform.Android, 
			RuntimePlatform.IPhonePlayer => Platform.IPhone, 
			_ => Platform.Other, 
		};
	}

	int IUserImplementsClass.GetPolicyRevision(Policy policy)
	{
		return PolicyAgreeFile.Get(policy);
	}

	string IUserImplementsClass.GetWord(Word word)
	{
		if (errorMessages.ContainsKey(word))
		{
			return errorMessages[word];
		}
		return "";
	}

	void IUserImplementsClass.OnAccountFileCreateFailed(string message)
	{
	}

	void IUserImplementsClass.OnAskRetryOrRestart(string title, string message, OnPushRetry onPushRetry, OnPushRestart onPushRestart)
	{
	}

	void IUserImplementsClass.OnChangeNotificationSetting(bool bOn)
	{
	}

	void IUserImplementsClass.OnForceTransitAccountBan(string message)
	{
	}

	void IUserImplementsClass.OnForceTransitApplicationUpdate(string title, string message, string appStoreUrl)
	{
	}

	void IUserImplementsClass.OnForceTransitAssetUpdate(string title, string message)
	{
	}

	void IUserImplementsClass.OnForceTransitGeneralError(string title, string message)
	{
	}

	void IUserImplementsClass.OnForceTransitUnderMaintenance(string title, string message, string starttime, string endtime)
	{
	}

	void IUserImplementsClass.OnRequestFailedDefault(string title, string message)
	{
	}
}
