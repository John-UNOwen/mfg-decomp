using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.GameRequest;
using MFG.MasterGameData;
using MFG.Types.ScrambleFestival;
using UnityEngine;

namespace MFG.GameData;

[Serializable]
public class ScrambleFestivalGameData
{
	private const int CurrentDataVersion = 1;

	public int DataVersion = 1;

	public const string ClientStateName = "scramble_festival";

	[NonSerialized]
	public const GameEventType CurrentEvent = GameEventType.Janpon4;

	public List<ScrambleFestivalEventInfo> ScrambleFestivalEventInfos = new List<ScrambleFestivalEventInfo>();

	public List<string> BeforeCatchphrases = new List<string>();

	public ScrambleFestivalEventInfo CurrentInfo => ScrambleFestivalEventInfos.Find((ScrambleFestivalEventInfo x) => x.FlagType == GameEventType.Janpon4);

	public bool IsEnable
	{
		get
		{
			if (CurrentInfo != null)
			{
				return CurrentInfo.IsEnable;
			}
			return false;
		}
	}

	public bool IsResultEnable
	{
		get
		{
			if (CurrentInfo != null && CurrentInfo.IsResultEnable)
			{
				return CurrentInfo.Team != TeamType.None;
			}
			return false;
		}
	}

	public ScrambleFestivalGameData()
	{
		if (CurrentInfo == null)
		{
			ScrambleFestivalEventInfos.Add(new ScrambleFestivalEventInfo(GameEventType.Janpon4));
		}
	}

	public async UniTask Save(CancellationToken cancellationToken = default(CancellationToken))
	{
		string text = JsonUtility.ToJson(this);
		Debug.Log("ScrambleFestivalGameData Save : " + text);
		await RequestApiBase<ClientStateWrite, ClientStateWrite.ResultData, (string, string)>.Send(("scramble_festival", text), enableErrorScreenTransition: false, enableSendLog: true, cancellationToken);
	}

	public static ScrambleFestivalGameData LoadFromJson(string jsonString)
	{
		Debug.Log("ScrambleFestivalGameData Load : " + jsonString);
		ScrambleFestivalGameData scrambleFestivalGameData = JsonUtility.FromJson<ScrambleFestivalGameData>(jsonString);
		if (scrambleFestivalGameData == null)
		{
			scrambleFestivalGameData = new ScrambleFestivalGameData();
		}
		scrambleFestivalGameData.Validate();
		return scrambleFestivalGameData;
	}

	private void Validate()
	{
		if (CurrentInfo == null)
		{
			ScrambleFestivalEventInfos.Add(new ScrambleFestivalEventInfo(GameEventType.Janpon4));
		}
	}

	public void CatchphraseUpdate()
	{
		if ((IsEnable && CurrentInfo.Team != 0) || BeforeCatchphrases == null || BeforeCatchphrases.Count <= 0)
		{
			return;
		}
		int num = 0;
		foreach (CharaGameData charaDatum in GameDataManager.Player.CharaData)
		{
			try
			{
				charaDatum.CatchphraseItemEquip = CatchphraseItemMaster.ToItemID(BeforeCatchphrases[num]);
			}
			catch
			{
			}
			num++;
		}
		BeforeCatchphrases.Clear();
	}
}
