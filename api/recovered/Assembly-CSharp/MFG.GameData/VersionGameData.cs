using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.GameRequest;
using UnityEngine;

namespace MFG.GameData;

[Serializable]
public class VersionGameData : GameDataBase
{
	public const string ClientStateName = "version_game";

	public const int CurrentDataVersion = 0;

	public int DataVersion;

	public const int CurrentSaveDataVersion = 68;

	public int SaveDataVersion = 68;

	public static VersionGameData LoadFromJson(string jsonString)
	{
		Debug.Log("VersionGameData Load : " + jsonString);
		VersionGameData versionGameData = JsonUtility.FromJson<VersionGameData>(jsonString);
		if (versionGameData == null)
		{
			versionGameData = new VersionGameData();
		}
		return versionGameData;
	}

	public async UniTask Save(CancellationToken cancellationToken = default(CancellationToken))
	{
		SaveDataVersion = 68;
		string text = JsonUtility.ToJson(this);
		Debug.Log("VersionGameData Save : " + text);
		await RequestApiBase<ClientStateWrite, ClientStateWrite.ResultData, (string, string)>.Send(("version_game", text), enableErrorScreenTransition: false, enableSendLog: true, cancellationToken);
	}

	public static bool IsNewVersion(int targetVersion)
	{
		bool flag = targetVersion > 68;
		Debug.Log($"VersionGameData.IsNewVersion: {flag} (saveDataVersion={targetVersion}, clientVersion={68})");
		return flag;
	}
}
