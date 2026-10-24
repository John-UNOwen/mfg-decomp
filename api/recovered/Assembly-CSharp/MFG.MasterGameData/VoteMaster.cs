using System.Collections.Generic;
using MFG.Types;
using MFG.Types.Vote;

namespace MFG.MasterGameData;

public class VoteMaster : MasterGameDataBase
{
	private static List<Item> _items = new List<Item>
	{
		new Item
		{
			FlagType = GameEventType.PopularityVote,
			Characters = new List<CharaType>
			{
				CharaType.Chara01,
				CharaType.Chara02,
				CharaType.Chara03,
				CharaType.Chara04,
				CharaType.Chara05,
				CharaType.Chara06,
				CharaType.Chara07,
				CharaType.Chara08,
				CharaType.Chara09,
				CharaType.Chara10,
				CharaType.Chara11,
				CharaType.Chara12
			},
			IsOnlyEnableChara = false,
			IsTaikyokuBonusEnable = true,
			IsPointMaxEnable = true,
			IsOneShotVote = false,
			RankBonus = new int[4][]
			{
				new int[4] { 5, 3, 2, 1 },
				new int[4] { 5, 3, 2, 1 },
				new int[3] { 5, 2, 1 },
				new int[2] { 2, 1 }
			},
			ProBonus = new int[2] { 5, 15 },
			VoteItem = new ExtraPlayerItemMaster
			{
				ObjectID = "VoteTicket",
				Name = "投票権",
				Description = "お気に入りのファイトガールに投票しよう！",
				IconGraphicPath = "CustomizeIcon/IconVoteTicket.png"
			},
			ConsumeWindowIconPathRoot = "VoteEvent/Common/Textures/ComsumeWindow/【chara】.png",
			VoteCompleteWindowItemNamePathRoot = "",
			VoteCompleteWindowIconPathRoot = "VoteEvent/PopularityVote/Textures/VoteCharacters/Vote【chara】.png"
		},
		new Item
		{
			FlagType = GameEventType.Xmas2024,
			Characters = new List<CharaType>
			{
				CharaType.Chara01,
				CharaType.Chara02,
				CharaType.Chara03,
				CharaType.Chara04,
				CharaType.Chara05,
				CharaType.Chara06,
				CharaType.Chara07,
				CharaType.Chara08,
				CharaType.Chara09,
				CharaType.Chara10,
				CharaType.Chara11,
				CharaType.Chara12,
				CharaType.Chara13,
				CharaType.Chara14
			},
			IsOnlyEnableChara = true,
			IsTaikyokuBonusEnable = false,
			IsPointMaxEnable = false,
			VoteMaxToOneChara = 10,
			IsCutinPlayInResult = true,
			RewardItemList = new List<string>
			{
				"OID_01NakiA02", "OID_02NakiA02", "OID_03NakiA02", "OID_04NakiA02", "OID_05NakiA02", "OID_06NakiA02", "OID_07NakiA02", "OID_08NakiA02", "OID_09NakiA02", "OID_10NakiA02",
				"OID_11NakiA02", "OID_12NakiA02", "OID_13NakiA02", "OID_14NakiA02"
			},
			IsOneShotVote = true,
			IsUnableIcon = true,
			RankBonus = null,
			ProBonus = null,
			VoteItem = new ExtraPlayerItemMaster
			{
				ObjectID = "XmasCookie",
				Name = "クッキー",
				Description = "10枚集めてプレゼントと交換しよう！",
				IconGraphicPath = "CustomizeIcon/IconXmasCookie.png"
			},
			ConsumeWindowIconPathRoot = "VoteEvent/Xmas2024/Textures/PresentBox/PresentBox【chara:D2】.png",
			VoteCompleteWindowItemNamePathRoot = "VoteEvent/Xmas2024/Textures/ItemName/CutInName【chara:D2】.png",
			VoteCompleteWindowIconPathRoot = "",
			IsCardConnectEnable = true
		},
		new Item
		{
			FlagType = GameEventType.CharaPai001,
			Characters = new List<CharaType>
			{
				CharaType.Chara01,
				CharaType.Chara03,
				CharaType.Chara04,
				CharaType.Chara07,
				CharaType.Chara09,
				CharaType.Chara11,
				CharaType.Chara13,
				CharaType.Chara15
			},
			IsOnlyEnableChara = false,
			IsTaikyokuBonusEnable = false,
			IsPointMaxEnable = false,
			VoteMaxToOneChara = 20,
			RewardItemList = new List<string> { "OID_TileSurface009", "OID_TileSurface011", "OID_TileSurface012", "OID_TileSurface015", "OID_TileSurface017", "OID_TileSurface019", "OID_TileSurface021", "OID_TileSurface023" },
			IsOneShotVote = true,
			IsUnableIcon = false,
			RankBonus = null,
			ProBonus = null,
			VoteItem = new ExtraPlayerItemMaster
			{
				ObjectID = "CharaPaiMedal001",
				Name = "痛牌メダル",
				Description = "20枚集めて痛牌と交換しよう！",
				IconGraphicPath = "CustomizeIcon/IconCharaPaiMedal001.png"
			},
			ConsumeWindowIconPathRoot = "RewardItemIcon",
			VoteCompleteWindowItemNamePathRoot = "VoteEvent/CharaPai001/Textures/ItemName/【chara】.png",
			VoteCompleteWindowIconPathRoot = "VoteEvent/CharaPai001/Textures/VoteCharacters/Vote【chara】.png"
		},
		new Item
		{
			FlagType = GameEventType.CharaPai002,
			Characters = new List<CharaType>
			{
				CharaType.Chara02,
				CharaType.Chara05,
				CharaType.Chara06,
				CharaType.Chara08,
				CharaType.Chara10,
				CharaType.Chara12,
				CharaType.Chara14,
				CharaType.Chara16
			},
			IsOnlyEnableChara = false,
			IsTaikyokuBonusEnable = false,
			IsPointMaxEnable = false,
			VoteMaxToOneChara = 20,
			RewardItemList = new List<string> { "OID_TileSurface010", "OID_TileSurface013", "OID_TileSurface014", "OID_TileSurface016", "OID_TileSurface018", "OID_TileSurface020", "OID_TileSurface022", "OID_TileSurface024" },
			IsOneShotVote = true,
			IsUnableIcon = false,
			RankBonus = null,
			ProBonus = null,
			VoteItem = new ExtraPlayerItemMaster
			{
				ObjectID = "CharaPaiMedal002",
				Name = "痛牌メダル",
				Description = "20枚集めて痛牌と交換しよう！",
				IconGraphicPath = "CustomizeIcon/IconCharaPaiMedal002.png"
			},
			ConsumeWindowIconPathRoot = "RewardItemIcon",
			VoteCompleteWindowItemNamePathRoot = "VoteEvent/CharaPai002/Textures/ItemName/【chara】.png",
			VoteCompleteWindowIconPathRoot = "VoteEvent/CharaPai002/Textures/VoteCharacters/Vote【chara】.png"
		},
		new Item
		{
			FlagType = GameEventType.Bonodori001,
			Characters = new List<CharaType>
			{
				CharaType.Chara03,
				CharaType.Chara04,
				CharaType.Chara06,
				CharaType.Chara07,
				CharaType.Chara08,
				CharaType.Chara11,
				CharaType.Chara12,
				CharaType.Chara13,
				CharaType.Chara17
			},
			IsOnlyEnableChara = false,
			IsHomeVote = true,
			IsTaikyokuBonusEnable = false,
			IsPointMaxEnable = false,
			VoteMaxToOneChara = 20,
			IsCutinPlayInResult = true,
			RewardItemList = new List<string> { "OID_03NakiA03", "OID_04NakiA03", "OID_06NakiA03", "OID_07NakiA03", "OID_08NakiA03", "OID_11NakiA03", "OID_12NakiA03", "OID_13NakiA03", "OID_17NakiA02" },
			IsOneShotVote = true,
			IsUnableIcon = false,
			RankBonus = null,
			ProBonus = null,
			VoteItem = new ExtraPlayerItemMaster
			{
				ObjectID = "BonodoriMedal001",
				Name = "お祭りメダル",
				Description = "20枚集めてカットインと交換しよう！",
				IconGraphicPath = "CustomizeIcon/IconBonodoriMedal001.png"
			},
			ConsumeWindowIconPathRoot = "RewardItemIcon",
			VoteCompleteWindowItemNamePathRoot = "VoteEvent/Bonodori001/Textures/ItemName/【chara】.png",
			VoteCompleteWindowIconPathRoot = "",
			IsCardConnectEnable = true
		},
		new Item
		{
			FlagType = GameEventType.Bonodori002,
			Characters = new List<CharaType>
			{
				CharaType.Chara01,
				CharaType.Chara02,
				CharaType.Chara05,
				CharaType.Chara09,
				CharaType.Chara10,
				CharaType.Chara14,
				CharaType.Chara15,
				CharaType.Chara16
			},
			IsOnlyEnableChara = false,
			IsHomeVote = true,
			IsTaikyokuBonusEnable = false,
			IsPointMaxEnable = false,
			VoteMaxToOneChara = 20,
			IsCutinPlayInResult = true,
			RewardItemList = new List<string> { "OID_01NakiA03", "OID_02NakiA03", "OID_05NakiA03", "OID_09NakiA03", "OID_10NakiA03", "OID_14NakiA03", "OID_15NakiA02", "OID_16NakiA02" },
			IsOneShotVote = true,
			IsUnableIcon = false,
			RankBonus = null,
			ProBonus = null,
			VoteItem = new ExtraPlayerItemMaster
			{
				ObjectID = "BonodoriMedal002",
				Name = "お祭りメダル",
				Description = "20枚集めてカットインと交換しよう！",
				IconGraphicPath = "CustomizeIcon/IconBonodoriMedal002.png"
			},
			ConsumeWindowIconPathRoot = "RewardItemIcon",
			VoteCompleteWindowItemNamePathRoot = "VoteEvent/Bonodori002/Textures/ItemName/【chara】.png",
			VoteCompleteWindowIconPathRoot = "",
			IsCardConnectEnable = true
		},
		new Item
		{
			FlagType = GameEventType.CharaPai003,
			Characters = new List<CharaType>
			{
				CharaType.Chara17,
				CharaType.Chara18,
				CharaType.Chara19,
				CharaType.Chara20
			},
			IsOnlyEnableChara = false,
			IsHomeVote = true,
			IsTaikyokuBonusEnable = false,
			IsPointMaxEnable = false,
			VoteMaxToOneChara = 40,
			RewardItemList = new List<string> { "OID_TileSurface026", "OID_TileSurface027", "OID_TileSurface028", "OID_TileSurface029" },
			IsOneShotVote = true,
			IsUnableIcon = false,
			RankBonus = null,
			ProBonus = null,
			VoteItem = new ExtraPlayerItemMaster
			{
				ObjectID = "CharaPaiMedal003",
				Name = "痛牌メダル",
				Description = "40枚集めて痛牌と交換しよう！",
				IconGraphicPath = "CustomizeIcon/IconCharaPaiMedal003.png"
			},
			ConsumeWindowIconPathRoot = "RewardItemIcon",
			VoteCompleteWindowItemNamePathRoot = "VoteEvent/CharaPai003/Textures/ItemName/【chara】.png",
			VoteCompleteWindowIconPathRoot = "VoteEvent/CharaPai003/Textures/VoteCharacters/Vote【chara】.png"
		},
		new Item
		{
			FlagType = GameEventType.RainyDay001,
			Characters = new List<CharaType>
			{
				CharaType.Chara01,
				CharaType.Chara04,
				CharaType.Chara05,
				CharaType.Chara07,
				CharaType.Chara09,
				CharaType.Chara12,
				CharaType.Chara16,
				CharaType.Chara17,
				CharaType.Chara18,
				CharaType.Chara20
			},
			IsOnlyEnableChara = false,
			IsHomeVote = true,
			IsTaikyokuBonusEnable = false,
			IsPointMaxEnable = false,
			VoteMaxToOneChara = 20,
			IsCutinPlayInResult = true,
			RewardItemList = new List<string> { "OID_01NakiA04", "OID_04NakiA04", "OID_05NakiA04", "OID_07NakiA04", "OID_09NakiA04", "OID_12NakiA04", "OID_16NakiA03", "OID_17NakiA03", "OID_18NakiA02", "OID_20NakiA02" },
			IsOneShotVote = true,
			IsUnableIcon = false,
			RankBonus = null,
			ProBonus = null,
			VoteItem = new ExtraPlayerItemMaster
			{
				ObjectID = "RainyDayMedal001",
				Name = "雨降りメダル",
				Description = "20枚集めてカットインと交換しよう！",
				IconGraphicPath = "CustomizeIcon/IconRainyDayMedal001.png"
			},
			ConsumeWindowIconPathRoot = "RewardItemIcon",
			VoteCompleteWindowItemNamePathRoot = "VoteEvent/RainyDay001/Textures/ItemName/【chara】.png",
			VoteCompleteWindowIconPathRoot = "",
			IsCardConnectEnable = true
		},
		new Item
		{
			FlagType = GameEventType.RainyDay002,
			Characters = new List<CharaType>
			{
				CharaType.Chara02,
				CharaType.Chara03,
				CharaType.Chara06,
				CharaType.Chara08,
				CharaType.Chara10,
				CharaType.Chara11,
				CharaType.Chara13,
				CharaType.Chara14,
				CharaType.Chara15,
				CharaType.Chara19,
				CharaType.Chara21
			},
			IsOnlyEnableChara = false,
			IsHomeVote = true,
			IsTaikyokuBonusEnable = false,
			IsPointMaxEnable = false,
			VoteMaxToOneChara = 20,
			IsCutinPlayInResult = true,
			RewardItemList = new List<string>
			{
				"OID_02NakiA04", "OID_03NakiA04", "OID_06NakiA04", "OID_08NakiA04", "OID_10NakiA04", "OID_11NakiA04", "OID_13NakiA04", "OID_14NakiA04", "OID_15NakiA03", "OID_19NakiA02",
				"OID_21NakiA02"
			},
			IsOneShotVote = true,
			IsUnableIcon = false,
			RankBonus = null,
			ProBonus = null,
			VoteItem = new ExtraPlayerItemMaster
			{
				ObjectID = "RainyDayMedal002",
				Name = "雨降りメダル",
				Description = "20枚集めてカットインと交換しよう！",
				IconGraphicPath = "CustomizeIcon/IconRainyDayMedal002.png"
			},
			ConsumeWindowIconPathRoot = "RewardItemIcon",
			VoteCompleteWindowItemNamePathRoot = "VoteEvent/RainyDay002/Textures/ItemName/【chara】.png",
			VoteCompleteWindowIconPathRoot = "",
			IsCardConnectEnable = true
		}
	};

	public static List<Item> Items => _items;

	public static Item GetItem(GameEventType eventType)
	{
		return _items.Find((Item item) => item.FlagType == eventType);
	}
}
