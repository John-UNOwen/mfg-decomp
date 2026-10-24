using System;
using System.Collections.Generic;
using System.Linq;
using MFG.GameData;
using MFG.MasterGameData;
using MFG.Types.Customize.Catchphrase;
using MFG.Types.Customize.CustomizeItem;
using MFG.Types.Customize.CutinItem;
using MFG.Types.Customize.MatchingBackground;
using MFG.Types.EventTakuMaster;
using MFG.Types.Favo;
using MFG.Types.Mission;
using MFG.Types.PlayerItem;
using MFG.Types.PlayerPaidItem;
using MFG.Types.ScrambleFestival;
using MFG.Types.Story;
using MFG.Types.Vote;

namespace MFG.Types;

public static class ItemUtility
{
	public static readonly List<ExtraPlayerItemMaster> ExtraItems = new List<ExtraPlayerItemMaster>
	{
		new ExtraPlayerItemMaster
		{
			ObjectID = "TenshiWing",
			Name = "テンシの羽",
			Description = "イベント期間中にたくさん集めよう！",
			IconGraphicPath = "CustomizeIcon/IconTenshiWing.png"
		},
		new ExtraPlayerItemMaster
		{
			ObjectID = "YellEpassTicket",
			Name = "e-amusement passカード抽選権",
			Description = "e-amusement passカードキャンペーンに応募しよう！",
			IconGraphicPath = "CustomizeIcon/IconYellEpassTicket.png"
		},
		new ExtraPlayerItemMaster
		{
			ObjectID = "PopularityVoteAllUchiwaAccessory",
			Name = "推しうちわ全種セット（アクセサリ）",
			Description = "ガールの隣に置くことができるアクセサリ。",
			IconGraphicPath = "CustomizeIcon/IconPopularityVoteAllUchiwa.png"
		},
		new ExtraPlayerItemMaster
		{
			ObjectID = "PopularityVoteAllUchiwaSticker",
			Name = "推しうちわ全種セット（卓ステッカー）",
			Description = "卓に貼ることができるステッカー。",
			IconGraphicPath = "CustomizeIcon/IconPopularityVoteAllUchiwa.png"
		},
		new ExtraPlayerItemMaster
		{
			ObjectID = "ValentineReachStick",
			Name = "本命チョコ",
			Description = "リーチ棒のデザインを変更することができる。",
			IconGraphicPath = "CustomizeIcon/IconValentineReachStick.png",
			ExtentionItems = new List<string>
			{
				"OID_ReachStick156", "OID_ReachStick157", "OID_ReachStick158", "OID_ReachStick159", "OID_ReachStick160", "OID_ReachStick161", "OID_ReachStick162", "OID_ReachStick163", "OID_ReachStick164", "OID_ReachStick165",
				"OID_ReachStick166", "OID_ReachStick167", "OID_ReachStick168", "OID_ReachStick169", "OID_ReachStick170", "OID_ReachStick214", "OID_ReachStick215", "OID_ReachStick216", "OID_ReachStick217"
			}
		},
		new ExtraPlayerItemMaster
		{
			ObjectID = "ValentineLove",
			Name = "ラブ",
			Description = "イベント期間中にたくさん集めよう！",
			IconGraphicPath = "CustomizeIcon/IconValentineLove.png",
			Unit = "個"
		},
		new ExtraPlayerItemMaster
		{
			ObjectID = "HotenFightStar",
			Name = "ファイトスター",
			Description = "たくさん集めよう！"
		},
		new ExtraPlayerItemMaster
		{
			ObjectID = "ValentineLove2",
			Name = "ラブ",
			Description = "イベント期間中にたくさん集めよう！",
			IconGraphicPath = "CustomizeIcon/IconValentineLove.png",
			Unit = "個"
		}
	};

	public static MFG.Types.EventTakuMaster.Item GetEventTakuMaster(string oid)
	{
		if (!string.IsNullOrEmpty(oid))
		{
			return MFG.MasterGameData.EventTakuMaster.Items.Find((MFG.Types.EventTakuMaster.Item x) => x.EventItemName.Equals(oid));
		}
		return null;
	}

	public static bool IsEventTaku(string oid)
	{
		return GetEventTakuMaster(oid) != null;
	}

	public static bool IsPlayerItem(string oid)
	{
		return PlayerItemMaster.GetItem(PlayerItemMaster.ToItemID(oid)) != null;
	}

	public static bool IsCatchphrase(string oid)
	{
		return CatchphraseItemMaster.ToItemID(oid) != MFG.Types.Customize.Catchphrase.ItemID.None;
	}

	public static bool IsFavoMissionItem(string oid)
	{
		return FavoMaster.GetFavoData(oid) != null;
	}

	public static bool IsExtraItem(string oid)
	{
		return ExtraItem(oid) != null;
	}

	public static bool IsCustomizeItem(string oid)
	{
		return CustomizeItemMaster.ToItemID(oid) != MFG.Types.Customize.CustomizeItem.ItemID.None;
	}

	public static bool IsCutinItem(string oid)
	{
		return CutinItemMaster.GetItemID(oid) != MFG.Types.Customize.CutinItem.ItemID.None;
	}

	public static MFG.Types.Vote.Item GetVoteItem(string oid)
	{
		return VoteMaster.Items.Find((MFG.Types.Vote.Item x) => x.VoteItem.ObjectID.Equals(oid));
	}

	public static bool IsVoteItem(string oid)
	{
		return GetVoteItem(oid) != null;
	}

	public static bool IsPaidItem(string oid)
	{
		return PlayerPaidItemMaster.GetItem(PlayerPaidItemMaster.ToItemID(oid)) != null;
	}

	public static MFG.Types.ScrambleFestival.Item GetScrambleFesItem(string oid)
	{
		return ScrambleFestivalMaster.Items.Find((MFG.Types.ScrambleFestival.Item x) => x.LotteryItem.ObjectID.Equals(oid));
	}

	public static bool IsScrambleFestivalItem(string oid)
	{
		return GetScrambleFesItem(oid) != null;
	}

	public static bool IsVoiceItem(string oid)
	{
		return VoiceMaster.GetItem(oid) != null;
	}

	public static bool IsGachaTicket(string oid)
	{
		if ((PlayerItemMaster.GetItem(PlayerItemMaster.ToItemID(oid))?.Category ?? MFG.Types.PlayerItem.Category.None) != MFG.Types.PlayerItem.Category.GachaTicket)
		{
			return (PlayerPaidItemMaster.GetItem(PlayerPaidItemMaster.ToItemID(oid))?.Category ?? MFG.Types.PlayerPaidItem.Category.None) == MFG.Types.PlayerPaidItem.Category.GachaTicket;
		}
		return true;
	}

	public static (string, int) AddItemSelector(string oid, int num)
	{
		if (oid.Equals("ValentineReachStick"))
		{
			foreach (CharaGameData item in from a in GameDataManager.Player.CharaData
				orderby Guid.NewGuid()
				select a into x
				orderby x.Level descending
				select x)
			{
				try
				{
					string text = ExtraItem(oid).ExtentionItems?[(int)item.charaType] ?? "";
					if (!IsPossession(text))
					{
						return (text, 1);
					}
				}
				catch
				{
				}
			}
			return ("OID_FightSpirit_Wildcard", 5);
		}
		if ((oid.Equals("OID_ReachStick154") || oid.Equals("OID_ReachStick155")) && IsPossession(oid))
		{
			return ("OID_FightSpirit_Wildcard", 5);
		}
		return (oid, num);
	}

	public static void AddItem(string oid, int addCount = 1, CharaType chara = CharaType.Chara01, string expire = "")
	{
		if (IsPlayerItem(oid))
		{
			GameDataManager.PlayerItem.Add(PlayerItemMaster.ToItemID(oid), addCount);
		}
		else if (IsEventTaku(oid))
		{
			GameDataManager.EventTaku.Current(GetEventTakuMaster(oid).EventType).TotalPoint += addCount;
		}
		else if (IsVoteItem(oid))
		{
			GameDataManager.Vote.CurrentInfo.AddPoint(addCount);
		}
		else if (IsScrambleFestivalItem(oid))
		{
			GameDataManager.ScrambleFestival.CurrentInfo.AddMedal(addCount);
		}
		else if (IsPaidItem(oid))
		{
			GameDataManager.PlayerPaidItem.Add(PlayerPaidItemMaster.ToItemID(oid), addCount, expire);
		}
		else if (oid.Equals("TenshiWing"))
		{
			GameDataManager.TenshiGowanEvent.TenshiWingNum += addCount;
		}
		else if (IsFavoMissionItem(oid))
		{
			FavoMasterData favo = FavoMaster.GetFavoData(FavoMaster.ToItemID(oid));
			switch (favo.type)
			{
			case FavoItemType.Story:
			{
				foreach (MFG.Types.Story.Story story in StoryMaster.GetStories((MFG.Types.Story.Story x) => x.CharaType == chara && x.UnlockingCondition == favo.FavoNum && x.UnlockingType == UnlockingType.Favo))
				{
					bool isAvailable = story.CharaType != CharaType.Chara10 || GameDataManager.IsShioriStoryEnable();
					GameDataManager.Story.AddStory(story.StoryID, isAvailable);
				}
				break;
			}
			case FavoItemType.Bgm:
			{
				MFG.Types.Customize.CustomizeItem.ItemID bgmItemID = ItemIDExtentions.ReachBgmFromFavoOID(favo.ObjectID, chara);
				if (bgmItemID != 0 && !GameDataManager.Customize.CustomizeItems.Exists((CustomizeItemInfo x) => x.ItemID == bgmItemID))
				{
					GameDataManager.Customize.CustomizeItems.Add(new CustomizeItemInfo
					{
						ItemID = bgmItemID
					});
					EquipReachBGM(chara, bgmItemID);
				}
				break;
			}
			case FavoItemType.Cutin:
				if (favo.ID == FavoItemID.Favo01_145 && CutinItemMaster.AppearanceMovieCutins.Keys.Contains(chara))
				{
					MFG.Types.Customize.CutinItem.ItemID cutinItemID = CutinItemMaster.AppearanceMovieCutins[chara];
					if (!GameDataManager.Customize.CutinItems.Exists((CutinItemInfo x) => x.ItemID == cutinItemID))
					{
						GameDataManager.Customize.CutinItems.Add(new CutinItemInfo
						{
							ItemID = cutinItemID,
							IsOn = true
						});
					}
				}
				break;
			}
		}
		else if (IsCatchphrase(oid))
		{
			if (!IsPossession(oid))
			{
				GameDataManager.Customize.CatchphraseItems.Add(new CatchphraseItemInfo
				{
					ItemID = CatchphraseItemMaster.ToItemID(oid)
				});
			}
		}
		else if (IsCustomizeItem(oid))
		{
			if (!IsPossession(oid))
			{
				MFG.Types.Customize.CustomizeItem.ItemID itemID2 = CustomizeItemMaster.ToItemID(oid);
				GameDataManager.Customize.CustomizeItems.Add(new CustomizeItemInfo
				{
					ItemID = itemID2
				});
				EquipReachBGM(CustomizeItemMaster.GetItem(itemID2).charaType, itemID2);
			}
		}
		else if (IsCutinItem(oid))
		{
			if (!IsPossession(oid))
			{
				MFG.Types.Customize.CutinItem.ItemID itemID = CutinItemMaster.GetItemID(oid);
				GameDataManager.Customize.CutinItems.Add(new CutinItemInfo
				{
					ItemID = itemID,
					IsOn = true
				});
				MFG.Types.Customize.MatchingBackground.Item matchingBGItem = MatchingBackgroundMaster.GetGachaCutinItems().Find((MFG.Types.Customize.MatchingBackground.Item x) => x.UnlockingCutinID == itemID);
				if (matchingBGItem != null && !GameDataManager.Customize.MatchingBGItems.Exists((MatchingBGInfo x) => x.ItemID == matchingBGItem.ItemID) && !GameDataManager.Player.IsPro())
				{
					GameDataManager.Customize.MatchingBGItems.Add(new MatchingBGInfo
					{
						ItemID = matchingBGItem.ItemID
					});
				}
			}
		}
		else if (oid.Equals("ValentineLove"))
		{
			if (!GameDataManager.Mission.MissionEventItemDict.ContainsKey(MFG.Types.Mission.Category.Valentine2025))
			{
				GameDataManager.Mission.MissionEventItemDict.Add(MFG.Types.Mission.Category.Valentine2025, addCount);
			}
			else
			{
				GameDataManager.Mission.MissionEventItemDict[MFG.Types.Mission.Category.Valentine2025] += addCount;
			}
		}
		else if (oid.Equals("ValentineLove2"))
		{
			if (!GameDataManager.Mission.MissionEventItemDict.ContainsKey(MFG.Types.Mission.Category.Valentine2026))
			{
				GameDataManager.Mission.MissionEventItemDict.Add(MFG.Types.Mission.Category.Valentine2026, addCount);
			}
			else
			{
				GameDataManager.Mission.MissionEventItemDict[MFG.Types.Mission.Category.Valentine2026] += addCount;
			}
		}
		else if (oid.Equals("HotenFightStar"))
		{
			GameDataManager.PlayerConfig.HotenFightStar += addCount;
		}
		else if (IsVoiceItem(oid) && !IsPossession(oid))
		{
			chara = VoiceMaster.GetCharaType(oid);
			if (chara >= CharaType.Chara01)
			{
				GameDataManager.Player.CharaData[(int)chara].GachaVoiceItems.Add(new VoiceInfo
				{
					ItemID = VoiceMaster.GetItem(oid).ItemID
				});
			}
		}
	}

	private static void EquipReachBGM(CharaType chara, MFG.Types.Customize.CustomizeItem.ItemID itemID)
	{
		MFG.Types.Customize.CustomizeItem.Category category = CustomizeItemMaster.GetItem(itemID).Category;
		if (category != MFG.Types.Customize.CustomizeItem.Category.ReachBgm)
		{
			return;
		}
		try
		{
			GameDataManager.Player.CharaData.Find((CharaGameData x) => x.charaType == chara).CustomizeItemEquip_SetEquip(category, itemID, isEquip: true);
			if (GameDataManager.Customize.CustomizeItems.Where((CustomizeItemInfo x) => x.Master.Category == category && (x.ItemID == MFG.Types.Customize.CustomizeItem.ItemID.ReachBgm001 || x.Master.charaType == chara)).Count() == 2)
			{
				GameDataManager.Player.CharaData.Find((CharaGameData x) => x.charaType == chara).CustomizeItemEquip_SetEquip(category, MFG.Types.Customize.CustomizeItem.ItemID.ReachBgm001, isEquip: false);
			}
		}
		catch
		{
		}
	}

	public static string ItemIconGraphicPath(string oid)
	{
		if (IsPlayerItem(oid))
		{
			return PlayerItemMaster.GetItem(PlayerItemMaster.ToItemID(oid)).IconGraphicPath;
		}
		if (IsEventTaku(oid))
		{
			return GetEventTakuMaster(oid).EventItemIconPath;
		}
		if (IsPaidItem(oid))
		{
			return PlayerPaidItemMaster.GetItem(PlayerPaidItemMaster.ToItemID(oid)).IconGraphicPath;
		}
		if (IsExtraItem(oid))
		{
			return ExtraItem(oid).IconGraphicPath;
		}
		if (IsVoteItem(oid))
		{
			return GetVoteItem(oid).VoteItem.IconGraphicPath;
		}
		if (IsScrambleFestivalItem(oid))
		{
			return GetScrambleFesItem(oid).LotteryItem.IconGraphicPath;
		}
		if (IsCatchphrase(oid))
		{
			return "CustomizeIcon/IconCatchphrase.png";
		}
		if (IsCustomizeItem(oid))
		{
			return CustomizeItemMaster.GetItem(CustomizeItemMaster.ToItemID(oid)).IconGraphicPath;
		}
		if (IsFavoMissionItem(oid))
		{
			return FavoMaster.GetFavoData(oid).type switch
			{
				FavoItemType.Voice => PlayerItemMaster.GetItem(MFG.Types.PlayerItem.ItemID.Voice).IconGraphicPath, 
				FavoItemType.Bgm => "CustomizeIcon/IconReachBgm001.png", 
				FavoItemType.Story => "CustomizeIcon/IconStory.png", 
				FavoItemType.Cutin => "CustomizeIcon/IconCutin.png", 
				_ => PlayerItemMaster.GetItem(MFG.Types.PlayerItem.ItemID.Voice).IconGraphicPath, 
			};
		}
		if (IsCutinItem(oid))
		{
			return "CustomizeIcon/IconCutin.png";
		}
		if (IsVoiceItem(oid))
		{
			return PlayerItemMaster.GetItem(MFG.Types.PlayerItem.ItemID.Voice).IconGraphicPath;
		}
		return "";
	}

	public static string ItemName(string oid, CharaType charaType = (CharaType)(-1))
	{
		if (IsPlayerItem(oid))
		{
			return PlayerItemMaster.GetItem(PlayerItemMaster.ToItemID(oid)).Name;
		}
		if (IsEventTaku(oid))
		{
			return GetEventTakuMaster(oid).EventItemDisplayName;
		}
		if (IsPaidItem(oid))
		{
			return PlayerPaidItemMaster.GetItem(PlayerPaidItemMaster.ToItemID(oid)).Name;
		}
		if (IsExtraItem(oid))
		{
			return ExtraItem(oid).Name;
		}
		if (IsVoteItem(oid))
		{
			return GetVoteItem(oid).VoteItem.Name;
		}
		if (IsScrambleFestivalItem(oid))
		{
			return GetScrambleFesItem(oid).LotteryItem.Name;
		}
		if (IsCatchphrase(oid))
		{
			return CatchphraseItemMaster.GetItem(CatchphraseItemMaster.ToItemID(oid)).DispName;
		}
		if (IsCustomizeItem(oid))
		{
			return CustomizeItemMaster.GetItem(CustomizeItemMaster.ToItemID(oid)).DispName;
		}
		if (IsFavoMissionItem(oid) && charaType >= CharaType.Chara01)
		{
			return FavoMaster.GetFavoData(oid).GetName(charaType);
		}
		if (IsCutinItem(oid))
		{
			return CutinItemMaster.GetItem(CutinItemMaster.GetItemID(oid)).Name;
		}
		if (IsVoiceItem(oid))
		{
			return VoiceMaster.GetItem(oid).DispName;
		}
		return "";
	}

	public static string DisplayNameInMission(string oid, CharaType chara = (CharaType)(-1))
	{
		string text = ItemName(oid, chara);
		if (SaniTize.IsSaniTized(text))
		{
			text = SaniTize.AddColor(text, "#000000");
		}
		if (IsCustomizeItem(oid) || IsCatchphrase(oid) || IsFavoMissionItem(oid))
		{
			return ItemCategory(oid) + "\n" + text;
		}
		return text;
	}

	public static string ItemCategory(string oid)
	{
		if (IsPlayerItem(oid))
		{
			return PlayerItemMaster.GetItem(PlayerItemMaster.ToItemID(oid)).Name;
		}
		if (IsEventTaku(oid))
		{
			return GetEventTakuMaster(oid).EventItemDisplayName;
		}
		if (IsPaidItem(oid))
		{
			return PlayerPaidItemMaster.GetItem(PlayerPaidItemMaster.ToItemID(oid)).Name;
		}
		if (IsExtraItem(oid))
		{
			return ExtraItem(oid).Name;
		}
		if (IsVoteItem(oid))
		{
			return GetVoteItem(oid).VoteItem.Name;
		}
		if (IsScrambleFestivalItem(oid))
		{
			return GetScrambleFesItem(oid).LotteryItem.Name;
		}
		if (IsCatchphrase(oid))
		{
			return "キャッチフレーズ";
		}
		if (IsFavoMissionItem(oid))
		{
			return FavoMaster.GetFavoData(oid).type switch
			{
				FavoItemType.Voice => "ボイス", 
				FavoItemType.Bgm => "リーチBGM", 
				FavoItemType.Story => "ストーリー", 
				FavoItemType.Cutin => "ガールズカットイン", 
				_ => "", 
			};
		}
		if (IsCustomizeItem(oid))
		{
			return CustomizeItemMaster.GetItem(CustomizeItemMaster.ToItemID(oid)).Category switch
			{
				MFG.Types.Customize.CustomizeItem.Category.ReachStick => "リーチ棒", 
				MFG.Types.Customize.CustomizeItem.Category.PlayerAccessory => "アクセサリ", 
				MFG.Types.Customize.CustomizeItem.Category.TableSticker => "卓ステッカー", 
				MFG.Types.Customize.CustomizeItem.Category.TileSurface => "牌デザイン", 
				MFG.Types.Customize.CustomizeItem.Category.ReachBgm => "リーチBGM", 
				MFG.Types.Customize.CustomizeItem.Category.MyRoomBgm => "マイルームBGM", 
				MFG.Types.Customize.CustomizeItem.Category.StageBg => "対局背景", 
				_ => "", 
			};
		}
		if (IsCutinItem(oid))
		{
			return "ガールズカットイン";
		}
		if (IsVoiceItem(oid))
		{
			return "ボイス";
		}
		return "";
	}

	public static bool IsDuplicateEnable(string oid)
	{
		if (!IsPlayerItem(oid) && !IsPaidItem(oid) && !IsEventTaku(oid) && !IsVoteItem(oid))
		{
			return IsScrambleFestivalItem(oid);
		}
		return true;
	}

	public static string ItemDescription(string oid)
	{
		if (IsPlayerItem(oid))
		{
			return PlayerItemMaster.GetItem(oid).Description;
		}
		if (IsCatchphrase(oid))
		{
			return "キャッチフレーズを設定することができる。";
		}
		if (IsPaidItem(oid))
		{
			return PlayerPaidItemMaster.GetItem(oid).Description;
		}
		if (IsEventTaku(oid))
		{
			return "イベント期間中にたくさん集めよう！";
		}
		if (IsVoteItem(oid))
		{
			return GetVoteItem(oid).VoteItem.Description;
		}
		if (IsScrambleFestivalItem(oid))
		{
			return GetScrambleFesItem(oid).LotteryItem.Description;
		}
		if (IsExtraItem(oid))
		{
			return ExtraItem(oid).Description;
		}
		if (IsFavoMissionItem(oid))
		{
			return FavoMaster.GetFavoData(oid).type switch
			{
				FavoItemType.Voice => PlayerItemMaster.GetItem(MFG.Types.PlayerItem.ItemID.Voice).Description, 
				FavoItemType.Story => "ガール達のストーリーを読むことができる。", 
				FavoItemType.Bgm => "リーチ時のBGMを変更することができる。", 
				FavoItemType.Cutin => "ガールズカットインを変更することができる。", 
				_ => "", 
			};
		}
		if (IsCustomizeItem(oid))
		{
			return CustomizeItemMaster.GetItem(oid).Category switch
			{
				MFG.Types.Customize.CustomizeItem.Category.ReachStick => "リーチ棒のデザインを変更することができる。", 
				MFG.Types.Customize.CustomizeItem.Category.PlayerAccessory => "ガールの隣に置くことができるアクセサリ。", 
				MFG.Types.Customize.CustomizeItem.Category.TableSticker => "卓に貼ることができるステッカー。", 
				MFG.Types.Customize.CustomizeItem.Category.TileSurface => "麻雀牌のデザインを変更することができる。", 
				MFG.Types.Customize.CustomizeItem.Category.ReachBgm => "リーチ時のBGMを変更することができる。", 
				MFG.Types.Customize.CustomizeItem.Category.MyRoomBgm => "マイルームのBGMを変更することができる。", 
				MFG.Types.Customize.CustomizeItem.Category.StageBg => "対局画面の背景を変更することができる。", 
				_ => "", 
			};
		}
		if (IsCutinItem(oid))
		{
			return "ガールズカットインを変更することができる。";
		}
		if (IsVoiceItem(oid))
		{
			return PlayerItemMaster.GetItem(MFG.Types.PlayerItem.ItemID.Voice).Description;
		}
		return "";
	}

	public static bool IsPossession(string oid)
	{
		if (IsPlayerItem(oid))
		{
			return GameDataManager.PlayerItem.Count(PlayerItemMaster.ToItemID(oid)) > 0;
		}
		if (IsEventTaku(oid))
		{
			return GameDataManager.EventTaku.Current(GetEventTakuMaster(oid).EventType).TotalPoint > 0;
		}
		if (IsPaidItem(oid))
		{
			return GameDataManager.PlayerPaidItem.Count(PlayerPaidItemMaster.ToItemID(oid)) > 0;
		}
		if (IsVoteItem(oid))
		{
			return GameDataManager.Vote.CurrentInfo.Point > 0;
		}
		if (IsScrambleFestivalItem(oid))
		{
			return GameDataManager.ScrambleFestival.CurrentInfo.Medal > 0;
		}
		if (IsCatchphrase(oid))
		{
			return GameDataManager.Customize.CatchphraseItems.Exists((CatchphraseItemInfo x) => x.Master.ObjectID == oid);
		}
		if (IsCustomizeItem(oid))
		{
			return GameDataManager.Customize.CustomizeItems.Exists((CustomizeItemInfo x) => x.Master.ObjectID == oid);
		}
		if (IsCutinItem(oid))
		{
			return GameDataManager.Customize.CutinItems.Exists((CutinItemInfo x) => x.Master.ObjectID == oid);
		}
		if (IsVoiceItem(oid))
		{
			CharaType charaType = VoiceMaster.GetCharaType(oid);
			if (charaType >= CharaType.Chara01)
			{
				return GameDataManager.Player.CharaData[(int)charaType].GachaVoiceItems.Exists((VoiceInfo x) => x.ItemID == VoiceMaster.GetItem(oid).ItemID);
			}
		}
		return false;
	}

	public static bool IsCutinItemPossessions(List<MFG.Types.Customize.CutinItem.ItemID> ids, bool isAll = false)
	{
		if (ids != null && ids.Count > 0)
		{
			IEnumerable<MFG.Types.Customize.CutinItem.ItemID> possessionItems = GameDataManager.Customize.CutinItems.Select((CutinItemInfo x) => x.ItemID);
			if (isAll)
			{
				return ids.All((MFG.Types.Customize.CutinItem.ItemID x) => possessionItems.Contains(x));
			}
			return ids.Exists((MFG.Types.Customize.CutinItem.ItemID x) => possessionItems.Contains(x));
		}
		return false;
	}

	public static ExtraPlayerItemMaster ExtraItem(string oid)
	{
		return ExtraItems.Find((ExtraPlayerItemMaster x) => x.ObjectID == oid);
	}
}
