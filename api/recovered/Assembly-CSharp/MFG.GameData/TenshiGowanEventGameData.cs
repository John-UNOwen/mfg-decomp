using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.GameRequest;
using MFG.Types;
using UnityEngine;

namespace MFG.GameData;

[Serializable]
public class TenshiGowanEventGameData
{
	private const int CurrentDataVersion = 1;

	public int DataVersion = 1;

	public const string ClientStateName = "tenshi_gowan_event";

	public int TenshiWingNum;

	[NonSerialized]
	public const int YonmaWingRate = 1000;

	[NonSerialized]
	public const int SanmaWingRate = 2000;

	[NonSerialized]
	public long ThisGameSumScore;

	public async UniTask Save(CancellationToken cancellationToken = default(CancellationToken))
	{
		string text = JsonUtility.ToJson(this);
		Debug.Log("TenshiGowanEventGameData Save : " + text);
		await RequestApiBase<ClientStateWrite, ClientStateWrite.ResultData, (string, string)>.Send(("tenshi_gowan_event", text), enableErrorScreenTransition: false, enableSendLog: true, cancellationToken);
	}

	public static TenshiGowanEventGameData LoadFromJson(string jsonString)
	{
		Debug.Log("TenshiGowanEventGameData Load : " + jsonString);
		return JsonUtility.FromJson<TenshiGowanEventGameData>(jsonString);
	}

	public TenshiGowanEventGameData()
	{
		Init();
		Reset();
	}

	public void Init()
	{
		ThisGameSumScore = 0L;
	}

	public void Reset()
	{
		TenshiWingNum = 0;
	}

	public void AddThisGameScore(long score = 0L)
	{
		ThisGameSumScore += score;
	}

	public int GetResultTenshiWing()
	{
		return GetEventItemCalculator(GameDataManager.Mahjong.TakuType, ThisGameSumScore);
	}

	public void AddWing()
	{
		TenshiWingNum += GetResultTenshiWing();
	}

	public bool IsEnable(bool takuTypeJudge = true)
	{
		return false;
	}

	public static int GetEventItemCalculator(TakuType takuType, long score)
	{
		return takuType switch
		{
			TakuType.Tonpu => (int)score / 1000, 
			TakuType.Sanma => (int)score / 2000, 
			_ => 0, 
		};
	}
}
