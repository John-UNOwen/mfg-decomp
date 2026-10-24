using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using MFG.GameRequest;
using MFG.Types;
using MFG.Types.Competition;
using UnityEngine;

namespace MFG.GameData;

[Serializable]
public class CompetitionGameData
{
	public List<CompetitionInfo> CompetitionInfos = new List<CompetitionInfo>();

	[NonSerialized]
	private List<CompetitionInfo> _latestCompetitionInfos;

	private const int CurrentDataVersion = 1;

	public int DataVersion = 1;

	public const string ClientStateName = "competition_game";

	public const string LatestResultClientStateName = "competition_latest_result";

	public CompetitionInfo GetInProgressCompetitionInfo()
	{
		return CompetitionInfos.Find((CompetitionInfo a) => a.IsInProgress);
	}

	public CompetitionInfo GetLatestCompetitionInfo()
	{
		try
		{
			return (from x in CompetitionInfos
				where x.CompetitionStatus >= CompetitionStatus.InProgress
				orderby x.CompetitionStatus, x.CompetitionID descending
				select x).FirstOrDefault();
		}
		catch
		{
		}
		return null;
	}

	public CompetitionID GetLatestCompetitionID()
	{
		return GetLatestCompetitionInfo()?.CompetitionID ?? CompetitionID.None;
	}

	public CompetitionInfo GetFinalResultCompetitionInfo()
	{
		try
		{
			return (from x in CompetitionInfos
				where x.IsEntried && x.CompetitionStatus == CompetitionStatus.Result && !x.IsFinalResultDisplayed
				orderby x.CompetitionID
				select x).FirstOrDefault();
		}
		catch
		{
		}
		return null;
	}

	public CompetitionInfo GetInProgressEntriedCompetitionInfo()
	{
		return CompetitionInfos.Find((CompetitionInfo a) => a.IsInProgress && a.IsEntried);
	}

	public List<CompetitionInfo> GetKAC2023CompetitionInfos()
	{
		return CompetitionInfos.FindAll((CompetitionInfo x) => x.CompetitionID.IsKAC2023());
	}

	private CompetitionInfo GetBeforeCompetitionInfo()
	{
		return GetInProgressEntriedCompetitionInfo();
	}

	public async UniTask<(CompetitionInfo beforeCompetitionInfo, CompetitionInfo afterCompetitionInfo)> GetLatestResult()
	{
		CompetitionInfo beforeCompetitionInfo = GetBeforeCompetitionInfo();
		_latestCompetitionInfos = null;
		try
		{
			_latestCompetitionInfos = (await RequestApiBase<ClientStateRead, ClientStateRead.ResultData, ClientStateRead.Param>.Send(new ClientStateRead.Param
			{
				OneKindType = "competition_latest_result"
			}, enableErrorScreenTransition: false)).LatestCompetition?.CompetitionInfos;
		}
		catch
		{
			Debug.LogWarning("ClientStateRead: 大会スコア取得通信に失敗しました。");
		}
		CompetitionInfo competitionInfo = _latestCompetitionInfos?.Find((CompetitionInfo x) => x.CompetitionID == beforeCompetitionInfo.CompetitionID);
		if (competitionInfo == null)
		{
			Debug.LogWarning("大会スコアが見つかりませんでした。");
			(beforeCompetitionInfo, competitionInfo) = GenerateCompetitionInfoOnNetworkError();
		}
		Debug.Log("-- before scores--");
		beforeCompetitionInfo.Log_Scores();
		Debug.Log("-- after  scores--");
		competitionInfo.Log_Scores();
		return (beforeCompetitionInfo, competitionInfo);
	}

	public async UniTask Validate()
	{
		await InvalidateOldCompetitionFinalResultDisplayed(GetLatestCompetitionID());
	}

	private async UniTask InvalidateOldCompetitionFinalResultDisplayed(CompetitionID latestCompetitionID)
	{
		if (latestCompetitionID == CompetitionID.None)
		{
			return;
		}
		foreach (CompetitionInfo competitionInfo in CompetitionInfos)
		{
			if (competitionInfo.CompetitionID < latestCompetitionID && !competitionInfo.IsFinalResultDisplayed && (competitionInfo.CompetitionStatus == CompetitionStatus.AfterEnd || (competitionInfo.CompetitionStatus == CompetitionStatus.Result && !competitionInfo.IsEntried)))
			{
				competitionInfo.IsFinalResultDisplayed = true;
				try
				{
					await RequestApiBase<CompetitionEntry, CompetitionEntry.ResultData, CompetitionEntry.Param>.Send(new CompetitionEntry.Param
					{
						CompetitionID = competitionInfo.CompetitionID,
						IsEntryRequest = false
					}, enableErrorScreenTransition: false);
					Debug.LogWarning($"{GetType().Name}.Validate: {competitionInfo.CompetitionID}を結果閲覧済みにしました。");
				}
				catch (Exception)
				{
					Debug.LogWarning("CompetitionEntry: 結果閲覧済み通信に失敗しました。");
				}
			}
		}
	}

	public void DebugKACForceActivate(bool isForceOverWrite = false)
	{
	}

	public void DebugForceActivate(CompetitionID competitionID, CompetitionStatus status)
	{
		CompetitionInfo competitionInfo = CompetitionInfos.Find((CompetitionInfo x) => x.CompetitionID == competitionID);
		if (competitionInfo == null)
		{
			competitionInfo = GenerateDebugCompetitionInfo(competitionID, status);
			if (status == CompetitionStatus.Result)
			{
				competitionInfo.IsEntried = true;
				competitionInfo.IsFinalResultDisplayed = false;
			}
			CompetitionInfos.Add(competitionInfo);
		}
	}

	public string GetNextSceneWhenGotoHome()
	{
		if (GameDataManager.Account.IsGuestUser)
		{
			return "Home";
		}
		if (GetFinalResultCompetitionInfo() != null)
		{
			return "EventResult";
		}
		CompetitionInfo inProgressCompetitionInfo = GetInProgressCompetitionInfo();
		if (inProgressCompetitionInfo != null && inProgressCompetitionInfo.IsEnableEntry && !inProgressCompetitionInfo.IsEntried && GameDataManager.Player.EntryConfirmedCompetitionID < inProgressCompetitionInfo.CompetitionID)
		{
			return "EventStart";
		}
		return "Home";
	}

	public static CompetitionInfo GenerateDebugCompetitionInfo(CompetitionID competitionID, CompetitionStatus competitionStatus, bool isAfter = false, bool isButtobi = false)
	{
		return new CompetitionInfo
		{
			CompetitionID = competitionID,
			Name = "オペレーションぶっとびギャラクシー杯",
			ScoreType = ScoreType.ButtobiHalfIt,
			SpecifiedGameCount = -1,
			Start = DateTimeOffset.MinValue,
			End = DateTimeOffset.MaxValue,
			CompetitionStatus = competitionStatus,
			IsEntried = false,
			EntrySystemType = EntrySystemType.Normal,
			EntryCharaType = (CharaType)(-1),
			EntryRequirementType = EntryRequirementType.Normal,
			IsNotQualifiedEntry = false,
			IsInTime = true,
			Scores = new List<int>(),
			LatestScore = ((isAfter && !isButtobi) ? 924 : 0),
			LatestScoreIndex = 0,
			TotalScore = ((!isAfter) ? 600 : (isButtobi ? 420 : 1524)),
			IsButtobi = isButtobi,
			ButtobashiCount = (isButtobi ? 0 : 0),
			BaseLatestScore = 500,
			ExtraLatestScore = (isButtobi ? (-300) : 300),
			BonusRatio = 1200,
			BestScores = new List<int>(),
			BestTotalScore = ((isAfter && !isButtobi) ? 1524 : 600),
			BestScoreIndex = ((isAfter && !isButtobi) ? 1 : (-1)),
			Rank = ((isAfter && !isButtobi) ? 1000 : 5890),
			RankRate = ((isAfter && !isButtobi) ? 450 : 700),
			NextBorderScore = 1500,
			RankRateCategoryType = 2,
			_rankRateCategory = ((isAfter && !isButtobi) ? 4 : 5),
			_nextBorderRankRateCategory = ((isAfter && !isButtobi) ? 3 : 4),
			Period = "2026/4/29(水) 10:00 ～ 2026/5/17(日) 23:59",
			Join = "e-amusement passを使ってプレーされている方",
			Rule = "・対局を行うことで結果に応じてスコアが増減し、期間中での最高スコアを競います。\n・開催期間中の10:00～23:59にプレーを開始した対局をスコアの集計対象とします。",
			Mode = "オペレーションぶっとびギャラクシー(４人打ち)\nオペレーションぶっとびギャラクシー(３人打ち)",
			Remarks = "・スコア上位者は公式サイトのランキングにて表示される場合があります。\n・不適切行為が行われたと当社が認めた場合、ランキングの除外等の対応をさせていただきます。あらかじめご了承ください。\n・期間内であれば後から参加することができます。",
			WinRanks = new List<string> { "上位1%以上", "上位10％以上", "上位30％以上", "上位50％以上" },
			WinPrizes = new List<string> { "プラチナトロフィー", "ゴールドトロフィー", "シルバートロフィー", "ブロンズトロフィー" },
			HomeScoreText = ""
		};
	}

	private (CompetitionInfo before, CompetitionInfo after) GenerateCompetitionInfoOnNetworkError()
	{
		CompetitionInfo competitionInfo = GetBeforeCompetitionInfo().Clone();
		CompetitionInfo competitionInfo2 = GetBeforeCompetitionInfo().Clone();
		competitionInfo.Invalidate();
		competitionInfo2.Invalidate();
		return (before: competitionInfo, after: competitionInfo2);
	}

	public static async UniTask<CompetitionGameData> LoadFromJson(string jsonString)
	{
		Debug.Log("CompetitionGameData Load : " + jsonString);
		CompetitionGameData gamedata = JsonUtility.FromJson<CompetitionGameData>(jsonString);
		if (gamedata == null)
		{
			gamedata = new CompetitionGameData();
		}
		await gamedata.Validate();
		return gamedata;
	}
}
