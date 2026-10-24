using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.GameRequest;
using UnityEngine;

namespace MFG.GameData;

[Serializable]
public class OthersEventGameData
{
	private const int CurrentDataVersion = 1;

	public int DataVersion = 1;

	public const string ClientStateName = "others_event";

	[NonSerialized]
	public const GameEventType CurrentOmikujiEvent = GameEventType.Omikuji2026;

	public List<OmikujiEventInfo> OmikujiEventInfos = new List<OmikujiEventInfo>();

	public OmikujiEventInfo CurrentOmikujiInfo => OmikujiEventInfos.Find((OmikujiEventInfo x) => x.FlagType == GameEventType.Omikuji2026);

	public OthersEventGameData()
	{
		if (CurrentOmikujiInfo == null)
		{
			OmikujiEventInfos.Add(new OmikujiEventInfo(GameEventType.Omikuji2026));
		}
	}

	public async UniTask Save(CancellationToken cancellationToken = default(CancellationToken))
	{
		string text = JsonUtility.ToJson(this);
		Debug.Log("OthersEventGameData Save : " + text);
		await RequestApiBase<ClientStateWrite, ClientStateWrite.ResultData, (string, string)>.Send(("others_event", text), enableErrorScreenTransition: false, enableSendLog: true, cancellationToken);
	}

	public static OthersEventGameData LoadFromJson(string jsonString)
	{
		Debug.Log("OthersEventGameData Load : " + jsonString);
		OthersEventGameData othersEventGameData = JsonUtility.FromJson<OthersEventGameData>(jsonString);
		if (othersEventGameData == null)
		{
			othersEventGameData = new OthersEventGameData();
		}
		othersEventGameData.Validate();
		return othersEventGameData;
	}

	private void Validate()
	{
		if (CurrentOmikujiInfo == null)
		{
			OmikujiEventInfos.Add(new OmikujiEventInfo(GameEventType.Omikuji2026));
		}
	}
}
