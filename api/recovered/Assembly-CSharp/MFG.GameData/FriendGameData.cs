using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.GameRequest;
using UnityEngine;

namespace MFG.GameData;

[Serializable]
public class FriendGameData
{
	private const int CurrentDataVersion = 1;

	public int DataVersion = 1;

	public const string ClientStateName = "friend_game";

	public List<FriendInfo> FriendInfos = new List<FriendInfo>();

	public async UniTask Save(CancellationToken cancellationToken = default(CancellationToken))
	{
		string text = JsonUtility.ToJson(this);
		Debug.Log("FriendGameData Save : " + text);
		await RequestApiBase<ClientStateWrite, ClientStateWrite.ResultData, (string, string)>.Send(("friend_game", text), enableErrorScreenTransition: false, enableSendLog: true, cancellationToken);
	}

	public static FriendGameData LoadFromJson(string jsonString)
	{
		Debug.Log("FriendGameData Load : " + jsonString);
		FriendGameData friendGameData = JsonUtility.FromJson<FriendGameData>(jsonString);
		if (friendGameData == null)
		{
			friendGameData = new FriendGameData();
		}
		friendGameData.Validate();
		return friendGameData;
	}

	private void Validate()
	{
	}
}
