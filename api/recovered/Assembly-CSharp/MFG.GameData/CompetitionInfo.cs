using System;
using System.Collections.Generic;
using System.Linq;
using MFG.Assets;
using MFG.Types;
using MFG.Types.Competition;
using UnityEngine;

namespace MFG.GameData;

[Serializable]
public class CompetitionInfo : ISerializationCallbackReceiver
{
	public CompetitionID CompetitionID;

	public string Name;

	public ScoreType ScoreType;

	public int SpecifiedGameCount;

	public DateTimeOffset Start;

	public DateTimeOffset End;

	[SerializeField]
	[HideInInspector]
	private string StartDateTime;

	[SerializeField]
	[HideInInspector]
	private string EndDateTime;

	public EntrySystemType EntrySystemType;

	public EntryRequirementType EntryRequirementType;

	public CharaType EntryCharaType = (CharaType)(-1);

	public string Period;

	public string Join;

	public string Rule;

	public string Mode;

	public string InPrize;

	public string Remarks;

	public List<string> WinRanks = new List<string>();

	public List<string> WinPrizes = new List<string>();

	public string JoinPrize;

	public CompetitionStatus CompetitionStatus;

	public bool IsEntried;

	public bool IsFinalResultDisplayed;

	public bool IsInTime;

	public bool IsNotQualifiedEntry;

	public List<int> Scores = new List<int>();

	public int LatestScore;

	public int LatestScoreIndex = -1;

	public bool IsButtobi;

	public int ButtobashiCount;

	public int BaseLatestScore;

	public int ExtraLatestScore;

	public int BonusRatio;

	public int TotalScore;

	public List<int> BestScores = new List<int>();

	public int BestScoreIndex = -1;

	public int BestTotalScore;

	public string HomeScoreText = "";

	public int Rank;

	public int RankRate;

	public int NextBorderScore;

	[SerializeField]
	[HideInInspector]
	private int _untilNextBorderScore;

	public int RankRateCategoryType;

	[SerializeField]
	[HideInInspector]
	public int _rankRateCategory;

	[SerializeField]
	[HideInInspector]
	public int _nextBorderRankRateCategory;

	[SerializeField]
	[HideInInspector]
	private int _trophyType;

	private const int CurrentDataVersion = 1;

	public int DataVersion = 1;

	public bool IsInProgress => CompetitionStatus == CompetitionStatus.InProgress;

	public bool IsEnableEntry => !IsNotQualifiedEntry;

	public MFG.Types.EventType CompeEventType => CompetitionID switch
	{
		CompetitionID.Competition006 => MFG.Types.EventType.FireGalaxy, 
		CompetitionID.Competition007 => MFG.Types.EventType.AccelDora, 
		CompetitionID.Competition008 => MFG.Types.EventType.Mentanpin, 
		CompetitionID.Competition009 => MFG.Types.EventType.PCKGalaxy, 
		_ => MFG.Types.EventType.None, 
	};

	public bool IsHiScore
	{
		get
		{
			if (BestScoreIndex >= 0)
			{
				return LatestScore > 0;
			}
			return false;
		}
	}

	public int UntilNextBorderScore
	{
		get
		{
			if (NextBorderScore >= 0)
			{
				return _untilNextBorderScore;
			}
			return -1;
		}
	}

	public TrophyType TrophyType => (TrophyType)_trophyType;

	public string ScoreTypeSummary => ScoreType switch
	{
		ScoreType.BestScore => $"期間中最も成績の良い\n{SpecifiedGameCount}対局の合計スコアで競います", 
		ScoreType.ConsecutiveScore => $"期間中最も成績の良い連続する\n{SpecifiedGameCount}対局の合計スコアで競います", 
		ScoreType.ButtobiHalfIt => "期間中の最高スコアを競います", 
		ScoreType.AccelDoraBestScore => $"期間中最も成績の良い{SpecifiedGameCount}対局の合計スコアで競います", 
		ScoreType.MentanpinBestScore => $"期間中最も成績の良い{SpecifiedGameCount}対局の合計スコアで競います", 
		_ => "", 
	};

	public string BgPath
	{
		get
		{
			if (EntrySystemType == EntrySystemType.Character)
			{
				return AssetPathResolver.ResolveCompetitionScoreWindowAssets(EntryCharaType, CompetitionScoreWindowAssetType.BgTexture);
			}
			return $"Competition/{CompetitionID}/Bg.png";
		}
	}

	public bool IsValid(TakuType takuType, MFG.Types.EventType eventType, bool isConsiderIsInTime = true)
	{
		if (!IsEntried || (isConsiderIsInTime && !IsInTime))
		{
			return false;
		}
		if (eventType != CompeEventType)
		{
			return false;
		}
		switch (CompetitionID)
		{
		case CompetitionID.Competition003:
			switch (takuType)
			{
			case TakuType.Sanma:
				return false;
			case TakuType.Tonpu:
				return true;
			}
			break;
		case CompetitionID.Competition004:
			switch (takuType)
			{
			case TakuType.Sanma:
				return true;
			case TakuType.Tonpu:
				return false;
			}
			break;
		default:
			if (takuType != 0 && takuType != TakuType.Sanma)
			{
				break;
			}
			return true;
		}
		return false;
	}

	public void Invalidate()
	{
		CompetitionStatus = CompetitionStatus.None;
		Scores = new List<int>();
		LatestScore = -1;
		LatestScoreIndex = -1;
		TotalScore = -1;
		BestScores = new List<int>();
		BestScoreIndex = -1;
		BestTotalScore = -1;
		Rank = 0;
		RankRate = -1;
		NextBorderScore = -1;
		_untilNextBorderScore = -1;
	}

	public CompetitionInfo Clone()
	{
		CompetitionInfo obj = (CompetitionInfo)MemberwiseClone();
		obj.Scores = new List<int>(Scores);
		obj.BestScores = new List<int>(Scores);
		return obj;
	}

	public void Log_Scores()
	{
		Debug.Log(string.Format("Scores.Count:{0}, Count.Values:{1}", Scores.Count, string.Join(", ", Scores.Select((int x) => x.ToString()))));
		Debug.Log($"LatestScore:{LatestScore}, LatestScoreIndex:{LatestScoreIndex}, TotalScore:{TotalScore}");
		Debug.Log($"Rank:{Rank}, RankRate:{RankRate}");
		Debug.Log($"UntilNextBorderScore:{UntilNextBorderScore}");
	}

	public void OnBeforeSerialize()
	{
	}

	public void OnAfterDeserialize()
	{
		if (string.IsNullOrEmpty(StartDateTime))
		{
			Start = DateTimeOffset.MinValue;
		}
		else
		{
			Start = DateTimeOffsetManager.GetJSTFromString(StartDateTime);
			if (Start == default(DateTimeOffset))
			{
				Start = DateTimeOffset.MinValue;
			}
		}
		if (string.IsNullOrEmpty(EndDateTime))
		{
			End = DateTimeOffset.MaxValue;
			return;
		}
		End = DateTimeOffsetManager.GetJSTFromString(EndDateTime);
		if (End == default(DateTimeOffset))
		{
			End = DateTimeOffset.MaxValue;
		}
	}

	public string RankRateLabel(bool isNext)
	{
		return RankRateCategoryType switch
		{
			0 => (RankRateCategory)(isNext ? _nextBorderRankRateCategory : _rankRateCategory) switch
			{
				RankRateCategory.Top10 => "上位10位", 
				RankRateCategory.Top10Percent => "上位10％", 
				RankRateCategory.Top30Percent => "上位30％", 
				RankRateCategory.Top70Percent => "上位70％", 
				RankRateCategory.LessThan70Percent => "上位70％未満", 
				_ => "", 
			}, 
			1 => (RankRateCategoryVer2)(isNext ? _nextBorderRankRateCategory : _rankRateCategory) switch
			{
				RankRateCategoryVer2.Top10 => "上位10位", 
				RankRateCategoryVer2.Top10Percent => "上位10％", 
				RankRateCategoryVer2.Top30Percent => "上位30％", 
				RankRateCategoryVer2.Top50Percent => "上位50％", 
				RankRateCategoryVer2.LessThan50Percent => "上位50％未満", 
				_ => "", 
			}, 
			2 => (RankRateCategoryVer3)(isNext ? _nextBorderRankRateCategory : _rankRateCategory) switch
			{
				RankRateCategoryVer3.Top1Percent => "上位1%", 
				RankRateCategoryVer3.Top10Percent => "上位10％", 
				RankRateCategoryVer3.Top30Percent => "上位30％", 
				RankRateCategoryVer3.Top50Percent => "上位50％", 
				RankRateCategoryVer3.LessThan50Percent => "上位50％未満", 
				_ => "", 
			}, 
			_ => "", 
		};
	}
}
