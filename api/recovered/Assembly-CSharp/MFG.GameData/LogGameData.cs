using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.GameRequest;
using MFG.Types;
using UnityEngine;

namespace MFG.GameData;

[Serializable]
public class LogGameData
{
	[Serializable]
	public class TaikyokuLogData
	{
		public int TsumoChoicesAverageConsumeTime;

		public int SuteChoicesAverageConsumeTime;

		public int TsumoAgariAverageConsumeTime;

		public int RonAgariAverageConsumeTime;

		public Rank Rank;

		public string DateTimeOffset;

		public MFG.Types.EventType EventType;

		public TakuType TakuType = (TakuType)(-1);

		public GainLossType GainLossType;

		public int RevengeContinueValue;

		public bool IsPrivateMatch;

		public List<string> AgariButtonOverlaps = new List<string>();

		public TouchPanelTelemetry AgariTouchPanelTelemetry = new TouchPanelTelemetry();

		public List<int> TsumoAgariConsumeTimeList = new List<int>();

		public List<int> RonAgariConsumeTimeList = new List<int>();

		public int TsumoChoiceChokoAverage;

		public void DebugLog()
		{
			Debug.Log("AgariTouchPanelTelemetry: " + AgariTouchPanelTelemetry.DebugLogString());
		}
	}

	private const int CurrentDataVersion = 1;

	public int DataVersion = 1;

	public const string ClientStateName = "log";

	[NonSerialized]
	public const int SlotNum = 50;

	public int ProceedingSlotNo;

	public List<TaikyokuLogData> TaikyokuLogs = new List<TaikyokuLogData>();

	[NonSerialized]
	public float TsumoChoicesTotalConsumeTime;

	[NonSerialized]
	public int TsumoChoicesCount;

	[NonSerialized]
	public int TsumoChoicesChokoCount;

	[NonSerialized]
	public float SuteChoicesTotalConsumeTime;

	[NonSerialized]
	public int SuteChoicesCount;

	[NonSerialized]
	public float TsumoAgariTotalConsumeTime;

	[NonSerialized]
	public int TsumoAgariCount;

	[NonSerialized]
	public List<int> TsumoAgariConsumeTimeList = new List<int>();

	[NonSerialized]
	public float RonAgariTotalConsumeTime;

	[NonSerialized]
	public int RonAgariCount;

	[NonSerialized]
	public List<int> RonAgariConsumeTimeList = new List<int>();

	[NonSerialized]
	public List<string> OverlapsAgari = new List<string>();

	[NonSerialized]
	private TouchPanelTelemetry _agariTouchPanelTelemetry = new TouchPanelTelemetry();

	public int LoopSlotNo => ProceedingSlotNo % 50;

	public void TaikyokuInit()
	{
		TsumoChoicesTotalConsumeTime = 0f;
		TsumoChoicesCount = 0;
		TsumoChoicesChokoCount = 0;
		SuteChoicesTotalConsumeTime = 0f;
		SuteChoicesCount = 0;
		TsumoAgariTotalConsumeTime = 0f;
		TsumoAgariCount = 0;
		RonAgariTotalConsumeTime = 0f;
		RonAgariCount = 0;
		TsumoAgariConsumeTimeList = new List<int>();
		RonAgariConsumeTimeList = new List<int>();
		OverlapsAgari = new List<string>();
		_agariTouchPanelTelemetry.Reset();
	}

	public void TaikyokuFinish(Rank rank)
	{
		TaikyokuLogData taikyokuLogData = TaikyokuLogs[LoopSlotNo];
		taikyokuLogData.TsumoChoicesAverageConsumeTime = SafeDivide(TsumoChoicesTotalConsumeTime, TsumoChoicesCount);
		taikyokuLogData.SuteChoicesAverageConsumeTime = SafeDivide(SuteChoicesTotalConsumeTime, SuteChoicesCount);
		taikyokuLogData.TsumoAgariAverageConsumeTime = SafeDivide(TsumoAgariTotalConsumeTime, TsumoAgariCount);
		taikyokuLogData.RonAgariAverageConsumeTime = SafeDivide(RonAgariTotalConsumeTime, RonAgariCount);
		taikyokuLogData.Rank = rank;
		taikyokuLogData.DateTimeOffset = DateTimeOffsetManager.GetJSTTime().ToString();
		taikyokuLogData.EventType = GameDataManager.Mahjong.EventType;
		taikyokuLogData.TakuType = GameDataManager.Mahjong.TakuType;
		taikyokuLogData.GainLossType = GameDataManager.Mahjong.GainLossType;
		taikyokuLogData.RevengeContinueValue = GameDataManager.PlayerConfig.RevengeContinueValue;
		taikyokuLogData.IsPrivateMatch = GameDataManager.Mahjong.IsPrivateMatch;
		taikyokuLogData.AgariButtonOverlaps = OverlapsAgari;
		taikyokuLogData.AgariTouchPanelTelemetry = _agariTouchPanelTelemetry.Clone();
		taikyokuLogData.TsumoAgariConsumeTimeList = TsumoAgariConsumeTimeList;
		taikyokuLogData.RonAgariConsumeTimeList = RonAgariConsumeTimeList;
		taikyokuLogData.TsumoChoiceChokoAverage = SafeDivide(TsumoChoicesChokoCount * 100, TsumoChoicesCount);
		taikyokuLogData.DebugLog();
		ProceedingSlotNo++;
	}

	private int SafeDivide(float a, int b)
	{
		if (b == 0)
		{
			return (int)a;
		}
		return (int)(a / (float)b);
	}

	public void AddTsumoChoicesData(float consumeTime)
	{
		TsumoChoicesTotalConsumeTime += consumeTime;
		TsumoChoicesCount++;
		if (consumeTime >= 5f)
		{
			TsumoChoicesChokoCount++;
		}
	}

	public void AddSuteChoicesData(float consumeTime)
	{
		SuteChoicesTotalConsumeTime += consumeTime;
		SuteChoicesCount++;
	}

	public void AddTsumoAgariData(float consumeTime, TouchPanelTelemetry touchPanelTelemetry)
	{
		TsumoAgariTotalConsumeTime += consumeTime;
		TsumoAgariCount++;
		_agariTouchPanelTelemetry.Add(touchPanelTelemetry);
		TsumoAgariConsumeTimeList.Add((int)consumeTime);
	}

	public void AddRonAgariData(float consumeTime, TouchPanelTelemetry touchPanelTelemetry)
	{
		RonAgariTotalConsumeTime += consumeTime;
		RonAgariCount++;
		_agariTouchPanelTelemetry.Add(touchPanelTelemetry);
		RonAgariConsumeTimeList.Add((int)consumeTime);
	}

	public void AddOverlapsAgariData(List<string> list)
	{
		OverlapsAgari.AddRange(list);
		OverlapsAgari = OverlapsAgari.Distinct().ToList();
	}

	public LogGameData()
	{
		for (int i = 0; i < 50; i++)
		{
			TaikyokuLogs.Add(new TaikyokuLogData());
		}
	}

	public async UniTask Save(CancellationToken cancellationToken = default(CancellationToken))
	{
		string text = JsonUtility.ToJson(this);
		Debug.Log("LogGameData Save : " + text);
		await RequestApiBase<ClientStateWrite, ClientStateWrite.ResultData, (string, string)>.Send(("log", text), enableErrorScreenTransition: false, enableSendLog: true, cancellationToken);
	}

	public static LogGameData LoadFromJson(string jsonString)
	{
		Debug.Log("LogGameData Load : " + jsonString);
		return JsonUtility.FromJson<LogGameData>(jsonString);
	}
}
