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
public class BlowAwaySanmaEventGameData
{
	[Serializable]
	public class ScoreData
	{
		public long FinalScore;

		public Cha Cha;

		public int Rank;

		public CharaType CharaType;
	}

	public class ResultInfo
	{
		public int Rank;

		public int RankAcquiredMedal;

		public int BlowAwayBonus;

		public int BlowAwayBonusAcquiredMedal;

		public int AcquiredMedalCount;

		public int BeforeTotalAcquiredMedalCount;
	}

	private const int CurrentDataVersion = 1;

	public int DataVersion = 1;

	public const string ClientStateName = "blow_away_event_sanma";

	[NonSerialized]
	public const GameEventType CurrentEvent = GameEventType.BlowAwaySanma;

	[NonSerialized]
	public List<ScoreData> ScoreDatas = new List<ScoreData>();

	[NonSerialized]
	public Cha SourceCha;

	[NonSerialized]
	private ResultInfo _resultInfo = new ResultInfo();

	public List<BlowAwaySanmaInfo> BlowAwaySanmaInfos = new List<BlowAwaySanmaInfo>();

	public int CurrentButtobiMedalNum
	{
		get
		{
			if (GetCurrentInfo() == null)
			{
				return 0;
			}
			return GetCurrentInfo().ButtobiMedalNum;
		}
	}

	public ResultInfo GetResultInfo()
	{
		return _resultInfo;
	}

	public BlowAwaySanmaEventGameData()
	{
		InfoUpdate();
	}

	public void InfoUpdate()
	{
		if (GetCurrentInfo() == null)
		{
			BlowAwaySanmaInfos.Add(new BlowAwaySanmaInfo
			{
				EventType = GameEventType.BlowAwaySanma
			});
			Debug.Log("BlowAwaySanmaEventGameData");
		}
	}

	public void ResetForBeforeTaikyokuStart()
	{
		ScoreDatas.Clear();
		_resultInfo = new ResultInfo
		{
			BeforeTotalAcquiredMedalCount = GetButtobiMedal()
		};
	}

	public void ResetForContinueStart()
	{
		ScoreDatas.Clear();
		_resultInfo = new ResultInfo
		{
			BeforeTotalAcquiredMedalCount = GetButtobiMedal()
		};
	}

	public BlowAwaySanmaInfo GetCurrentInfo()
	{
		return BlowAwaySanmaInfos.Find((BlowAwaySanmaInfo x) => x.EventType == GameEventType.BlowAwaySanma);
	}

	public async UniTask Save(CancellationToken cancellationToken = default(CancellationToken))
	{
		string text = JsonUtility.ToJson(this);
		Debug.Log("BlowAwaySanmaEventGameData Save : " + text);
		await RequestApiBase<ClientStateWrite, ClientStateWrite.ResultData, (string, string)>.Send(("blow_away_event_sanma", text), enableErrorScreenTransition: false, enableSendLog: true, cancellationToken);
	}

	public static BlowAwaySanmaEventGameData LoadFromJson(string jsonString)
	{
		Debug.Log("BlowAwaySanmaEventGameData Load : " + jsonString);
		return JsonUtility.FromJson<BlowAwaySanmaEventGameData>(jsonString);
	}

	public static string GetBlowAwaySanmaEventDate()
	{
		return (GameDataManager.System.GameControlInfo.EventStatus.GetGameEventData(GameEventType.BlowAwaySanma)?.End ?? DateTimeOffset.MinValue).ToString("yyyy年MM月dd日（ddd）HH：mmまで");
	}

	public void AddButtobiMedal()
	{
		AddCustomButtobiMedal(_resultInfo.AcquiredMedalCount);
	}

	public void AddCustomButtobiMedal(int i)
	{
		BlowAwaySanmaInfo currentInfo = GetCurrentInfo();
		if (currentInfo != null && currentInfo.IsEnable())
		{
			currentInfo.ButtobiMedalNum += i;
		}
	}

	public int GetButtobiMedal()
	{
		if (GetCurrentInfo() == null)
		{
			return 0;
		}
		return GetCurrentInfo().ButtobiMedalNum;
	}

	public bool IsEffectiveTaikyoku(MFG.Types.EventType eventType, TakuType takuType)
	{
		if (GameDataManager.Account.IsGuestUser)
		{
			return false;
		}
		if (takuType != TakuType.Sanma)
		{
			return false;
		}
		if (eventType != MFG.Types.EventType.ToitoiButtobiGalaxy)
		{
			return false;
		}
		return true;
	}

	public bool IsEnable(bool takuTypeJudge = true, GameEventType eventType = GameEventType.BlowAwaySanma)
	{
		return BlowAwaySanmaInfos.Find((BlowAwaySanmaInfo x) => x.EventType == eventType)?.IsEnable(takuTypeJudge) ?? false;
	}

	public bool IsCurrentEventFirstDisp()
	{
		BlowAwaySanmaInfo currentInfo = GetCurrentInfo();
		if (currentInfo != null && currentInfo.IsEnable(takuTypeJudge: false) && currentInfo.IsFirstDisp)
		{
			currentInfo.IsFirstDisp = false;
			return true;
		}
		return false;
	}

	public void ApplyAcquiredPoint(Cha sourceCha, List<Player> players)
	{
		SourceCha = sourceCha;
		ScoreDatas.Clear();
		foreach (Player player in players)
		{
			ScoreDatas.Add(new ScoreData
			{
				FinalScore = player.m_score,
				Cha = player.m_cha,
				Rank = player.m_rank,
				CharaType = player.m_charaType
			});
		}
		_resultInfo = GenerateResultInfo(ScoreDatas, SourceCha, GetButtobiMedal());
	}

	private static ResultInfo GenerateResultInfo(List<ScoreData> scoreDatas, Cha sourceCha, int totalPoint)
	{
		ResultInfo resultInfo = new ResultInfo
		{
			BeforeTotalAcquiredMedalCount = totalPoint
		};
		bool flag = sourceCha == Cha.Jicha;
		int num = scoreDatas.Count((ScoreData x) => x.FinalScore <= 0);
		resultInfo.Rank = scoreDatas.Find((ScoreData x) => x.Cha == Cha.Jicha).Rank;
		ResultInfo resultInfo2 = resultInfo;
		resultInfo2.RankAcquiredMedal = (Rank)resultInfo.Rank switch
		{
			Rank.Rank1 => 4, 
			Rank.Rank2 => 2, 
			_ => 1, 
		};
		resultInfo.BlowAwayBonus = (flag ? num : 0);
		resultInfo.BlowAwayBonusAcquiredMedal = (flag ? (3 * num) : 0);
		resultInfo.AcquiredMedalCount = (resultInfo.RankAcquiredMedal + resultInfo.BlowAwayBonusAcquiredMedal) * ((!GameDataManager.Mission.IsMissionProgressBoost) ? 1 : 2);
		return resultInfo;
	}
}
