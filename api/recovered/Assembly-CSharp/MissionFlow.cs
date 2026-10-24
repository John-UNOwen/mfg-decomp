using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.GameData;
using MFG.MasterGameData;
using MFG.Types;
using MFG.Types.Mission;

public static class MissionFlow
{
	public static async UniTask Save(JudgeLocation location, CancellationToken token = default(CancellationToken))
	{
		switch (location)
		{
		case JudgeLocation.MyRoom:
			await GameDataManager.PlayerPaidItem.Save(token);
			await GameDataManager.PlayerItem.Save(token);
			await GameDataManager.Story.Save(token);
			break;
		case JudgeLocation.Result:
			await GameDataManager.PlayerPaidItem.Save(token);
			await GameDataManager.PlayerItem.Save(token);
			await GameDataManager.Customize.Save(token);
			await GameDataManager.CardConnect.Save();
			break;
		}
		await GameDataManager.Mission.Save(token);
	}

	public static void ProgressTask(MissionProgressInfo info, int addCount, ref List<(MissionID, int, string, int)> list)
	{
		if (addCount <= 0)
		{
			return;
		}
		int i = info.ClearCount;
		if (info.Master.IsJandriProgressEnable && GameDataManager.Mission.IsMissionProgressBoost)
		{
			addCount *= 2;
		}
		info.Progress += addCount;
		int achievementIndex;
		for (achievementIndex = info.Master.GetAchievementIndex(info.Progress); i < achievementIndex; i++)
		{
			string oid;
			int num;
			(oid, num) = info.Master.AchievementItem(i);
			(oid, num) = ItemUtility.AddItemSelector(oid, num);
			if (info.Master.IsJandriRewardEnable && GameDataManager.Mission.IsMissionProgressBoost)
			{
				num *= 2;
			}
			list.Add((info.MissionID, i, oid, num));
			ItemUtility.AddItem(oid, num, info.Master.Chara, info.Master.CategoryMaster?.ItemExpireDate() ?? "");
			CardConnectSend(info.Master, oid);
			if (info.Master.IsResultAcquire && ItemUtility.IsCustomizeItem(oid))
			{
				GameDataManager.Notice.MusicAcquireInfos.Add(new MusicAcquireInfo
				{
					MissionID = info.MissionID,
					ID = CustomizeItemMaster.ToItemID(oid)
				});
			}
		}
		info.ClearCount = achievementIndex;
	}

	public static void UpdateTask(MissionProgressInfo info, int newProgress, ref List<(MissionID, int, string, int)> list)
	{
		int i = info.ClearCount;
		info.Progress = newProgress;
		int achievementIndex;
		for (achievementIndex = info.Master.GetAchievementIndex(info.Progress); i < achievementIndex; i++)
		{
			string oid;
			int num;
			(oid, num) = info.Master.AchievementItem(i);
			(oid, num) = ItemUtility.AddItemSelector(oid, num);
			if (info.Master.IsJandriRewardEnable && GameDataManager.Mission.IsMissionProgressBoost)
			{
				num *= 2;
			}
			list.Add((info.MissionID, i, oid, num));
			ItemUtility.AddItem(oid, num, info.Master.Chara, info.Master.CategoryMaster?.ItemExpireDate() ?? "");
			CardConnectSend(info.Master, oid);
			if (info.Master.IsResultAcquire && ItemUtility.IsCustomizeItem(oid))
			{
				GameDataManager.Notice.MusicAcquireInfos.Add(new MusicAcquireInfo
				{
					MissionID = info.MissionID,
					ID = CustomizeItemMaster.ToItemID(oid)
				});
			}
		}
		info.ClearCount = achievementIndex;
	}

	public static List<(MissionID missionID, int achievementIndex, string itemOID, int itemNum)> Judge(JudgeLocation location)
	{
		if (GameDataManager.Account.IsGuestUser)
		{
			return new List<(MissionID, int, string, int)>();
		}
		List<(MissionID, int, string, int)> list = new List<(MissionID, int, string, int)>();
		foreach (Item master in from x in MissionMaster.ActiveItems()
			where x.JudgeLocation.Contains(location)
			select x)
		{
			MissionProgressInfo missionProgressInfo = GameDataManager.Mission.m_missionProgressInfos.Find((MissionProgressInfo x) => x.MissionID == master.MissionID);
			if (missionProgressInfo == null || missionProgressInfo.IsAllClear)
			{
				continue;
			}
			switch (master.Kind)
			{
			case Kind.Others:
				if ((master.MissionID == MissionID.Player_Gacha && GameDataManager.Customize.IsGachaTried) || (master.MissionID == MissionID.Player_Gacha5 && GameDataManager.Customize.TotalGacha5Count > 0) || (master.MissionID == MissionID.Player_SpiritGym && GameDataManager.Mission.LessonCount > 0) || (master.MissionID == MissionID.Player_CharaChange && GameDataManager.Mission.IsChangeChara) || (master.MissionID == MissionID.Player_SetCutin && GameDataManager.Mission.IsChangeCutIn) || (master.MissionID == MissionID.Player_SetGoods && GameDataManager.Mission.IsChangeEquips))
				{
					ProgressTask(missionProgressInfo, 1, ref list);
				}
				break;
			case Kind.PlayCount:
				if (location != JudgeLocation.Home || !GameDataManager.Mission.IsHomeJudgeFinished)
				{
					ProgressTask(missionProgressInfo, 1, ref list);
				}
				break;
			case Kind.TaikyokuCount:
				if ((master.IsNimaEnable || GameDataManager.Mahjong.TakuType != TakuType.Nima) && (master.IsKikenEnable || !GameDataManager.Mahjong.IsKiken) && (master.TakuTypes.Count() == 0 || master.TakuTypes.Contains(GameDataManager.Mahjong.TakuType)) && (master.Chara < CharaType.Chara01 || master.Chara == GameDataManager.Player.SelectChara))
				{
					ProgressTask(missionProgressInfo, 1, ref list);
				}
				break;
			case Kind.TopCount:
				if ((master.TakuTypes.Count() == 0 || master.TakuTypes.Contains(GameDataManager.Mahjong.TakuType)) && GameDataManager.Mahjong.Rank == 0)
				{
					ProgressTask(missionProgressInfo, 1, ref list);
				}
				break;
			case Kind.Login:
			{
				string timeStamp = MissionCategoryMaster.GetTimeStamp(UpdateInterval.Daily);
				if (!string.IsNullOrEmpty(timeStamp) && !timeStamp.Equals(missionProgressInfo.MissionVersion))
				{
					ProgressTask(missionProgressInfo, 1, ref list);
					missionProgressInfo.MissionVersion = timeStamp;
				}
				break;
			}
			case Kind.Dan:
				if (missionProgressInfo.Progress < (int)GameDataManager.Player.DanLevel)
				{
					UpdateTask(missionProgressInfo, (int)GameDataManager.Player.DanLevel, ref list);
				}
				break;
			case Kind.ManganCount:
				if (!GameDataManager.Mahjong.IsAotenjoMode)
				{
					Han han = (Han)master.OtherArgs[0];
					int addCount = GameDataManager.Mission.HoraHanList.Where((Han x) => (han < Han.Yakuman) ? (x == han) : (x >= han)).Count();
					ProgressTask(missionProgressInfo, addCount, ref list);
				}
				break;
			case Kind.YakumanAgariCount:
				ProgressTask(missionProgressInfo, GameDataManager.Mission.HoraYakuList.Count((Yaku x) => x == (Yaku)master.OtherArgs[0]), ref list);
				break;
			case Kind.Favo:
				if (master.Chara == GameDataManager.Player.SelectChara)
				{
					UpdateTask(missionProgressInfo, GameDataManager.Player.GetCurrentChara(IsGetMyCharaData: true).Level, ref list);
				}
				break;
			case Kind.Fan:
				if (master.Chara == GameDataManager.Player.SelectChara)
				{
					UpdateTask(missionProgressInfo, GameDataManager.Player.GetCurrentChara(IsGetMyCharaData: true).m_fans[(int)master.OtherArgs[0]], ref list);
				}
				break;
			case Kind.Debut:
				if (GameDataManager.Player.CharaData[(int)master.Chara].IsDebut)
				{
					ProgressTask(missionProgressInfo, 1, ref list);
				}
				break;
			case Kind.EventItem:
				UpdateTask(missionProgressInfo, GameDataManager.EventTaku.Current(master.EventType).TotalPoint, ref list);
				break;
			case Kind.EventTaikyoku:
				if (GameDataManager.Mahjong.EventType == master.EventType && master.TakuTypes.Contains(GameDataManager.Mahjong.TakuType) && GameDataManager.Mahjong.IsConstancyTaku == master.IsConstancyEventTaku)
				{
					ProgressTask(missionProgressInfo, 1, ref list);
				}
				break;
			case Kind.EventTop:
				if (GameDataManager.Mahjong.EventType == master.EventType && master.TakuTypes.Contains(GameDataManager.Mahjong.TakuType) && GameDataManager.Mahjong.IsConstancyTaku == master.IsConstancyEventTaku && GameDataManager.Mahjong.Rank == 0)
				{
					ProgressTask(missionProgressInfo, 1, ref list);
				}
				break;
			case Kind.ScrambleTop:
			{
				(bool, SchoolID) tuple = GameDataManager.Mahjong.ResultInfo.IsScrambleMatchWin();
				Dictionary<SchoolID, int> scrambleMatchTotalWinCount = GameDataManager.PlayerRecord.GetScrambleMatchTotalWinCount();
				SchoolID item = tuple.Item2;
				if (item == (SchoolID)master.OtherArgs[0] && tuple.Item1 && scrambleMatchTotalWinCount.ContainsKey(item))
				{
					UpdateTask(missionProgressInfo, scrambleMatchTotalWinCount[item], ref list);
				}
				break;
			}
			case Kind.ProMatching:
				if (GameDataManager.Mission.WinningPro != 0 && master.Chara >= CharaType.Chara01 && master.Chara == MahjongActorMaster.GetItem(GameDataManager.Mission.WinningPro).CharaType)
				{
					ProgressTask(missionProgressInfo, 1, ref list);
				}
				break;
			case Kind.OverScore:
				if (master.Chara < CharaType.Chara01 || master.Chara == GameDataManager.Player.SelectChara)
				{
					bool flag = master.IsNimaEnable || GameDataManager.Mahjong.TakuType != TakuType.Nima;
					List<object> otherArgs = master.OtherArgs;
					int num = (int)otherArgs[GameDataManager.Mahjong.TakuType switch
					{
						TakuType.Tonpu => 0, 
						TakuType.Sanma => 1, 
						_ => 2, 
					}];
					if (flag && GameDataManager.Mahjong.Score >= GameDataManager.Mission.StartScore + num)
					{
						ProgressTask(missionProgressInfo, 1, ref list);
					}
				}
				break;
			case Kind.MissionEventItem:
			{
				if (GameDataManager.Mission.MissionEventItemDict.TryGetValue(missionProgressInfo.Master.Category, out var value))
				{
					ProgressTask(missionProgressInfo, value, ref list);
				}
				break;
			}
			case Kind.MusicGachaComp:
				try
				{
					MusicGachaInfo musicGachaInfo = GameDataManager.MusicGacha?.MusicGachaInfos?.Find((MusicGachaInfo x) => x.GachaID == (int)master.OtherArgs[0]) ?? null;
					if (musicGachaInfo == null)
					{
						break;
					}
					if (musicGachaInfo.GachaID <= 92)
					{
						if (musicGachaInfo.IsComplete)
						{
							ProgressTask(missionProgressInfo, 1, ref list);
						}
					}
					else
					{
						UpdateTask(missionProgressInfo, musicGachaInfo.PossessionCount, ref list);
					}
				}
				catch
				{
				}
				break;
			case Kind.Bingo:
				try
				{
					if (GameDataManager.BingoEventRemake.IsEnable())
					{
						UpdateTask(missionProgressInfo, GameDataManager.BingoEventRemake.BingoFryPanNumList.Find((FryPanInfo x) => x.EventType == master.CategoryMaster.MissionEventType).FryPanNum, ref list);
					}
				}
				catch
				{
				}
				break;
			case Kind.MotherWars:
				if (GameDataManager.Mahjong.ResultInfo.IsMotherWarsWin())
				{
					ProgressTask(missionProgressInfo, 1, ref list);
				}
				break;
			}
		}
		if (location == JudgeLocation.Home && !GameDataManager.Mission.IsHomeJudgeFinished)
		{
			GameDataManager.Mission.IsHomeJudgeFinished = true;
		}
		return list;
	}

	public static void CardConnectSend(Item master, string oid)
	{
		CardConnectMissionID item = master.CardConnectInfo.Item1;
		List<(CharaType, string)> item2 = master.CardConnectInfo.Item2;
		if (item >= CardConnectMissionID.NewYear2024 && item2 != null && item2.Exists(((CharaType, string) x) => x.Item2 == oid))
		{
			bool num = master.CardConnectInfo.Item1 == CardConnectMissionID.CutinItem;
			CharaType item3 = item2.Find(((CharaType, string) x) => x.Item2 == oid).Item1;
			string param = (num ? oid : ((item3 >= CharaType.Chara01) ? ("OID_" + ((int)(item3 + 1)).ToString("D2") + "MissionEvent" + ((int)(item + 1)).ToString("D2")) : (oid + "MissionEvent" + ((int)(item + 1)).ToString("D2"))));
			string name = (num ? CutinItemMaster.GetItem(CutinItemMaster.GetItemID(oid)).ShortName : ItemUtility.ItemName(oid));
			GameDataManager.CardConnect.m_ccInfos.Add(new CardConnectInfo(GameDataManager.CardConnect.now_idx, "MemorialCard", param, name));
			GameDataManager.CardConnect.now_idx++;
			MissionMemorialPrintNoticeInfo item4 = new MissionMemorialPrintNoticeInfo
			{
				CCID = item,
				Chara = item3,
				ItemName = ItemUtility.ItemName(oid),
				ItemOID = oid
			};
			GameDataManager.Notice.MissionMemorialPrintNoticeInfos.Add(item4);
		}
	}
}
