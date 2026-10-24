using System;
using System.Collections.Generic;
using System.Linq;
using MFG.GameData;
using MFG.MasterGameData;
using MFG.Types.EventTakuMaster;
using UnityEngine;

namespace MFG.Types.Mission;

public class Item
{
	public MissionID MissionID;

	public Category Category;

	public GameEventType GameEventType;

	public UpdateInterval UpdateInterval;

	public Kind Kind;

	public bool IsNimaEnable = true;

	public bool IsKikenEnable = true;

	public List<JudgeLocation> JudgeLocation;

	public EventType EventType;

	public List<TakuType> TakuTypes = new List<TakuType>();

	public bool IsConstancyEventTaku;

	public CharaType Chara = (CharaType)(-1);

	public List<object> OtherArgs = new List<object>();

	public LoopType LoopType;

	public DisplayType DisplayType;

	public bool IsProgressDisplay = true;

	public string ItemOID;

	public int ItemNum;

	public bool IsAutoClearItemPossession;

	public bool IsResultAcquire;

	public bool IsJandriProgressEnable;

	public bool IsJandriRewardEnable;

	public int FirstTerm;

	public int CommonDifference;

	public int LastTerm = -1;

	public List<int> NonNumericalAchievementList = new List<int>();

	public Dictionary<int, (string oid, int num)> ExtraRewardList = new Dictionary<int, (string, int)>();

	public (CardConnectMissionID, List<(CharaType, string)>) CardConnectInfo = ((CardConnectMissionID)(-1), new List<(CharaType, string)>());

	public MissionID RequiredMissionID;

	public MissionID HideMissionID;

	public List<MissionID> AutoClearMissionIDs = new List<MissionID>();

	public string ObjectID => MissionMaster.ToObjectID(MissionID);

	public CategoryItem CategoryMaster => MissionCategoryMaster.GetItem(Category);

	public bool IsEnable
	{
		get
		{
			if (CategoryMaster.IsEnable() && (GameEventType == GameEventType.None || GameDataManager.System.GameControlInfo.EventStatus.IsActive(GameEventType)) && (Chara == (CharaType)(-1) || GameDataManager.IsCharaEnable(Chara)) && !GameDataManager.Account.IsGuestUser && GameDataManager.Mission.IsAutoClearItemPossessionDisplay(IsAutoClearItemPossession, MissionID, ItemOID))
			{
				if (IsConstancyEventTaku)
				{
					return !GameDataManager.EventTaku.AvailableModeGameDatasWithGuest().Any();
				}
				return true;
			}
			return false;
		}
	}

	public string RemainingTimeString()
	{
		DateTimeOffset dateTimeOffset = UpdateInterval switch
		{
			UpdateInterval.Daily => MissionCategoryMaster.GetItem(Category.JudgmentDaily).EndDateTime, 
			UpdateInterval.Weekly => MissionCategoryMaster.GetItem(Category.JudgmentWeekly).EndDateTime, 
			_ => CategoryMaster.EndDateTime, 
		};
		DateTimeOffset jSTTime = DateTimeOffsetManager.GetJSTTime();
		if (dateTimeOffset == DateTimeOffset.MaxValue || dateTimeOffset < jSTTime)
		{
			return "";
		}
		TimeSpan timeSpan = dateTimeOffset.Subtract(jSTTime);
		if (timeSpan.Days >= 100)
		{
			return "";
		}
		if (timeSpan.Days >= 1)
		{
			return $"残り{timeSpan.Days}日";
		}
		if (timeSpan.Hours >= 1)
		{
			return $"残り{timeSpan.Hours}時間";
		}
		return $"残り{timeSpan.Minutes}分";
	}

	public void CardConnectCheck()
	{
	}

	public (string oid, int num) AchievementItem(int achievementIndex)
	{
		if (ExtraRewardList.TryGetValue(AchievementCount(achievementIndex), out (string, int) value))
		{
			return value;
		}
		return (oid: ItemOID, num: ItemNum);
	}

	public int AchievementCount(int achievementIndex)
	{
		if (LoopType == LoopType.Endless)
		{
			Debug.Log($"{MissionID} / EndlessMission[{achievementIndex}] : {FirstTerm + CommonDifference * achievementIndex}");
			return FirstTerm + CommonDifference * achievementIndex;
		}
		if (AchievementList().Count() <= achievementIndex)
		{
			return AchievementList()[AchievementList().Count() - 1];
		}
		return AchievementList()?[achievementIndex] ?? 0;
	}

	public string AchievementIndex()
	{
		return Kind switch
		{
			Kind.MissionEventItem => ItemUtility.ItemName(CategoryMaster.EventItemOID), 
			Kind.EventItem => MFG.MasterGameData.EventTakuMaster.GetItem(EventType).EventItemDisplayName, 
			Kind.Bingo => "フライパン", 
			_ => "対局", 
		};
	}

	public string AchievementUnit()
	{
		return Kind switch
		{
			Kind.MissionEventItem => ItemUtility.ExtraItem(CategoryMaster.EventItemOID).Unit, 
			Kind.EventItem => MFG.MasterGameData.EventTakuMaster.GetItem(EventType).EventItemUnit, 
			Kind.Bingo => "枚", 
			_ => "回", 
		};
	}

	public List<int> AchievementList()
	{
		List<int> list = new List<int>();
		switch (LoopType)
		{
		case LoopType.Single:
			list.Add(FirstTerm);
			break;
		case LoopType.NonNumerical:
			list = NonNumericalAchievementList;
			break;
		case LoopType.Numerical:
			list = (from n in Enumerable.Range(0, (LastTerm - FirstTerm) / CommonDifference + 1)
				select FirstTerm + CommonDifference * n).ToList();
			break;
		}
		return list;
	}

	public int GetAchievementIndex(int progress)
	{
		switch (LoopType)
		{
		case LoopType.Single:
			if (AchievementCount(0) > progress)
			{
				return 0;
			}
			return 1;
		case LoopType.NonNumerical:
		case LoopType.Numerical:
			if (AchievementList().Last() > progress)
			{
				return AchievementList().IndexOf((from x in AchievementList()
					where progress < x
					select x).First());
			}
			return AchievementList().Count();
		case LoopType.Endless:
			if (progress - FirstTerm >= 0)
			{
				return (progress - FirstTerm) / CommonDifference + 1;
			}
			return 0;
		default:
			return 0;
		}
	}

	public string MissionText(int achievementIndex = 0)
	{
		int num = AchievementCount(achievementIndex);
		string text = (IsProgressDisplay ? $"{num}回" : "");
		string text2 = ((Chara >= CharaType.Chara01) ? MasterGameDataManager.Character.Properties(Chara).Profile.Name : "");
		string text3 = "";
		List<TakuType> takuTypes = TakuTypes;
		if (takuTypes != null && takuTypes.Count() == 1)
		{
			text3 = TakuTypes[0] switch
			{
				TakuType.Tonpu => "4人打ちで", 
				TakuType.Sanma => "3人打ちで", 
				TakuType.Nima => "2人打ちで", 
				_ => "", 
			};
		}
		string text4 = UpdateInterval switch
		{
			UpdateInterval.Daily => "【デイリー】", 
			UpdateInterval.Weekly => (Category != Category.Weekly) ? "【ウィークリー】" : "", 
			_ => "", 
		};
		switch (Kind)
		{
		case Kind.Others:
			return MissionID switch
			{
				MissionID.Player_Gacha => "ガチャを引いてみよう", 
				MissionID.Player_Gacha5 => "5回ガチャを引いてみよう", 
				MissionID.Player_SpiritGym => "スピリットジムにガールをセットしよう", 
				MissionID.Player_CharaChange => "キャラクターを切り替えよう", 
				MissionID.Player_SetCutin => "ガールズカットインを設定しよう", 
				MissionID.Player_SetGoods => "グッズを変更しよう", 
				_ => "", 
			};
		case Kind.PlayCount:
			if (LoopType == LoopType.Endless && CommonDifference == 1)
			{
				return "プレーしよう";
			}
			return $"{num}回プレーしよう";
		case Kind.TaikyokuCount:
		{
			string text6 = "";
			if (!IsNimaEnable && !IsKikenEnable)
			{
				text6 = " <size=70%>※途中終了・2人打ち対象外</size>";
			}
			else if (!IsNimaEnable)
			{
				text6 = " <size=70%>※2人打ち対象外</size>";
			}
			else if (!IsKikenEnable)
			{
				text6 = " <size=70%>※途中終了対象外</size>";
			}
			return text4 + text3 + text + "対局しよう" + text6;
		}
		case Kind.TopCount:
			return text3 + text + "トップを取ろう";
		case Kind.Login:
			if (LoopType == LoopType.Single && FirstTerm == 1 && MissionID.ToString().Contains("First"))
			{
				return "初回ログインボーナス";
			}
			if (!IsProgressDisplay)
			{
				return text4 + "ログインしよう";
			}
			return $"{text4}{num}日ログインしよう";
		case Kind.Dan:
			return DanLevelMaster.DanLevelMissionDispName((DanLevelType)num) + "になろう";
		case Kind.ManganCount:
		{
			Han han = (Han)OtherArgs[0];
			string text5 = ((han >= Han.Yakuman3) ? "以上で" : ((han == Han.Yakuman2) ? "で" : "を"));
			return MahjongUtility.GetHanName(han) + text5 + text + "アガろう";
		}
		case Kind.YakumanAgariCount:
			return MahjongUtility.GetYakuKanaName((Yaku)OtherArgs[0]) + "をアガろう";
		case Kind.Favo:
			return $"好感度を{num}にしよう";
		case Kind.Fan:
		{
			string[] array = new string[4] { "運", "速", "攻", "守" };
			return $"{array[(int)OtherArgs[0]]}ファン数を{num}人達成しよう";
		}
		case Kind.Debut:
			return "デビューさせよう";
		case Kind.EventItem:
		{
			MFG.Types.EventTakuMaster.Item item = MFG.MasterGameData.EventTakuMaster.GetItem(EventType);
			return $"{ItemUtility.ItemName(item.EventItemName)}を{num}{item.EventItemUnit}ゲットしよう";
		}
		case Kind.EventTaikyoku:
			return $"{text4}「{MFG.MasterGameData.EventTakuMaster.GetItem(EventType).EventName}」で{num}回対局しよう";
		case Kind.EventTop:
			return $"「{MFG.MasterGameData.EventTakuMaster.GetItem(EventType).EventName}」で{num}回トップを取ろう";
		case Kind.ScrambleTop:
			return $"スクランブルマッチ発生時に{MFG.MasterGameData.SchoolMaster.GetItem((SchoolID)OtherArgs[0]).ShortName}のガールで{num}回トップを取ろう";
		case Kind.ProMatching:
			return MFG.MasterGameData.MahjongActorMaster.GetItem(Chara).FullName + "プロとマッチングし、順位で上回ろう";
		case Kind.OverScore:
			return $"4人打ちを初期点+{(int)OtherArgs[0]}点以上、もしくは\n3人打ちを初期点+{(int)OtherArgs[1]}点以上で終えよう(イベント卓含む)";
		case Kind.MissionEventItem:
			return $"{ItemUtility.ItemName(CategoryMaster.EventItemOID)}を{num}{ItemUtility.ExtraItem(CategoryMaster.EventItemOID).Unit}ゲットしよう";
		case Kind.MusicGachaComp:
			if (GameEventType == GameEventType.ReachSongGachaComp1)
			{
				return "リーチソング＆ボイスガチャ「" + text2 + "」をコンプリートしよう";
			}
			return $"リーチソング＆ボイスガチャ「{text2}」を{num}回プレーしよう";
		case Kind.Bingo:
			return $"フライパンを{num}枚ゲットしよう";
		case Kind.MotherWars:
			return "マザーウォーズ シロヱ 対 ミツモト・ダイアで1位を取ろう";
		default:
			return "";
		}
	}
}
