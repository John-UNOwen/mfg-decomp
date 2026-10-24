using System;
using System.Collections.Generic;
using MFG.Types;
using MFG.Types.Customize.Catchphrase;

namespace MFG.MasterGameData;

public class CatchphraseItemMaster : MasterGameDataBase
{
	private static List<ItemID> _defaultPossessionItems = new List<ItemID>
	{
		ItemID.Catchphrase_00_000,
		ItemID.Catchphrase_00_001,
		ItemID.Catchphrase_00_002,
		ItemID.Catchphrase_00_003
	};

	private static List<Item> _items = new List<Item>
	{
		new Item
		{
			ItemID = ItemID.Catchphrase_00_000,
			CharaType = Item.CharaTypeNone,
			BaseImageID = BaseImageID.Base_00_000,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "なし",
			Sort = SortRules.None
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_00_001,
			CharaType = Item.CharaTypeNone,
			BaseImageID = BaseImageID.Base_00_001,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "麻雀大好き",
			Sort = SortRules.Common
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_00_002,
			CharaType = Item.CharaTypeNone,
			BaseImageID = BaseImageID.Base_00_002,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "カジュアル雀士",
			Sort = SortRules.Common
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_00_003,
			CharaType = Item.CharaTypeNone,
			BaseImageID = BaseImageID.Base_00_003,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "ストイック雀士",
			Sort = SortRules.Common
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_00_004,
			CharaType = Item.CharaTypeNone,
			BaseImageID = BaseImageID.Base_00_004,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "ギガンティックフィスト",
			Sort = SortRules.Common
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_00_005,
			CharaType = Item.CharaTypeNone,
			BaseImageID = BaseImageID.Base_00_005,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "聖夜のファイトガール",
			Sort = SortRules.Common
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_00_006,
			CharaType = Item.CharaTypeNone,
			BaseImageID = BaseImageID.Base_00_006,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "暗黒信徒",
			Sort = SortRules.Common
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_00_007,
			CharaType = Item.CharaTypeNone,
			BaseImageID = BaseImageID.Base_00_007,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "暗黒神官",
			Sort = SortRules.Common
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_00_008,
			CharaType = Item.CharaTypeNone,
			BaseImageID = BaseImageID.Base_00_008,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "暗黒大神官",
			Sort = SortRules.Common
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_00_009,
			CharaType = Item.CharaTypeNone,
			BaseImageID = BaseImageID.Base_00_009,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "名誉ネリマ区民",
			Sort = SortRules.Common
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_00_010,
			CharaType = Item.CharaTypeNone,
			BaseImageID = BaseImageID.Base_00_010,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "上級ネリマ区民",
			Sort = SortRules.Common
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_00_011,
			CharaType = Item.CharaTypeNone,
			BaseImageID = BaseImageID.Base_00_011,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "特級ネリマ区民",
			Sort = SortRules.Common
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_00_012,
			CharaType = Item.CharaTypeNone,
			BaseImageID = BaseImageID.Base_00_012,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "ファイトガール春一番",
			Sort = SortRules.Common
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_00_013,
			CharaType = Item.CharaTypeNone,
			BaseImageID = BaseImageID.Base_00_013,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "炎の門下生",
			Sort = SortRules.Common
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_00_014,
			CharaType = Item.CharaTypeNone,
			BaseImageID = BaseImageID.Base_00_014,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "炎の師範代",
			Sort = SortRules.Common
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_00_015,
			CharaType = Item.CharaTypeNone,
			BaseImageID = BaseImageID.Base_00_015,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "炎のくみゃ皆伝",
			Sort = SortRules.Common
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_00_016,
			CharaType = Item.CharaTypeNone,
			BaseImageID = BaseImageID.Base_00_016,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "雨ガール",
			Sort = SortRules.Common
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_00_017,
			CharaType = Item.CharaTypeNone,
			BaseImageID = BaseImageID.Base_00_017,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "イッシキ建設従業員",
			Sort = SortRules.Common
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_00_018,
			CharaType = Item.CharaTypeNone,
			BaseImageID = BaseImageID.Base_00_018,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "イッシキ建設現場監督",
			Sort = SortRules.Common
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_00_019,
			CharaType = Item.CharaTypeNone,
			BaseImageID = BaseImageID.Base_00_019,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "イッシキ建設社長秘書",
			Sort = SortRules.Common
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_00_020,
			CharaType = Item.CharaTypeNone,
			BaseImageID = BaseImageID.Base_00_020,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "夏めくファイトガール",
			Sort = SortRules.Common
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_00_021,
			CharaType = Item.CharaTypeNone,
			BaseImageID = BaseImageID.Base_00_021,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "暗黒ご近所",
			Sort = SortRules.Common
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_00_022,
			CharaType = Item.CharaTypeNone,
			BaseImageID = BaseImageID.Base_00_022,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "暗黒隣人",
			Sort = SortRules.Common
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_00_023,
			CharaType = Item.CharaTypeNone,
			BaseImageID = BaseImageID.Base_00_023,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "暗黒同居人",
			Sort = SortRules.Common
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_00_024,
			CharaType = Item.CharaTypeNone,
			BaseImageID = BaseImageID.Base_00_024,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "麻雀を打つ程度の能力",
			Sort = SortRules.Common
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_00_025,
			CharaType = Item.CharaTypeNone,
			BaseImageID = BaseImageID.Base_00_025,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "Extra雀士",
			Sort = SortRules.Common
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_00_026,
			CharaType = Item.CharaTypeNone,
			BaseImageID = BaseImageID.Base_00_026,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "Lunatic雀士",
			Sort = SortRules.Common
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_00_027,
			CharaType = Item.CharaTypeNone,
			BaseImageID = BaseImageID.Base_00_027,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "大根農家",
			Sort = SortRules.Common
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_00_028,
			CharaType = Item.CharaTypeNone,
			BaseImageID = BaseImageID.Base_00_028,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "大根長者",
			Sort = SortRules.Common
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_00_029,
			CharaType = Item.CharaTypeNone,
			BaseImageID = BaseImageID.Base_00_029,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "大根財閥",
			Sort = SortRules.Common
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_00_030,
			CharaType = Item.CharaTypeNone,
			BaseImageID = BaseImageID.Base_00_030,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "好きな役：ピンフ",
			Sort = SortRules.Common
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_00_031,
			CharaType = Item.CharaTypeNone,
			BaseImageID = BaseImageID.Base_00_031,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "好きな役：タンヤオ",
			Sort = SortRules.Common
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_00_032,
			CharaType = Item.CharaTypeNone,
			BaseImageID = BaseImageID.Base_00_032,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "好きな役：チートイツ",
			Sort = SortRules.Common
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_00_033,
			CharaType = Item.CharaTypeNone,
			BaseImageID = BaseImageID.Base_00_033,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "好きな役：ピンフ",
			Sort = SortRules.Common
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_00_034,
			CharaType = Item.CharaTypeNone,
			BaseImageID = BaseImageID.Base_00_034,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "好きな役：タンヤオ",
			Sort = SortRules.Common
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_00_035,
			CharaType = Item.CharaTypeNone,
			BaseImageID = BaseImageID.Base_00_035,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "好きな役：チートイツ",
			Sort = SortRules.Common
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_00_036,
			CharaType = Item.CharaTypeNone,
			BaseImageID = BaseImageID.Base_00_036,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "Trick or Treat",
			Sort = SortRules.Common
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_00_037,
			CharaType = Item.CharaTypeNone,
			BaseImageID = BaseImageID.Base_00_037,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "イッシキエージェント",
			Sort = SortRules.Common
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_00_038,
			CharaType = Item.CharaTypeNone,
			BaseImageID = BaseImageID.Base_00_038,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "高級エージェント",
			Sort = SortRules.Common
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_00_039,
			CharaType = Item.CharaTypeNone,
			BaseImageID = BaseImageID.Base_00_039,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "特選エージェント",
			Sort = SortRules.Common
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_00_040,
			CharaType = Item.CharaTypeNone,
			BaseImageID = BaseImageID.Base_00_040,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "遊び初めファイトガール",
			Sort = SortRules.Common
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_00_041,
			CharaType = Item.CharaTypeNone,
			BaseImageID = BaseImageID.Base_00_041,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "らぶらぶ一直線",
			Sort = SortRules.Common
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_00_042,
			CharaType = Item.CharaTypeNone,
			BaseImageID = BaseImageID.Base_00_042,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "らぶらぶ特急",
			Sort = SortRules.Common
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_00_043,
			CharaType = Item.CharaTypeNone,
			BaseImageID = BaseImageID.Base_00_043,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "らぶらぶ超特急",
			Sort = SortRules.Common
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_00_044,
			CharaType = Item.CharaTypeNone,
			BaseImageID = BaseImageID.Base_00_044,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "お花見ファイトガール",
			Sort = SortRules.Common
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_00_045,
			CharaType = Item.CharaTypeNone,
			BaseImageID = BaseImageID.Base_00_045,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "PCK雀士",
			Sort = SortRules.Common
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_00_046,
			CharaType = Item.CharaTypeNone,
			BaseImageID = BaseImageID.Base_00_046,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "PCKトレーナー",
			Sort = SortRules.Common
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_00_047,
			CharaType = Item.CharaTypeNone,
			BaseImageID = BaseImageID.Base_00_047,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "PCKマイスター",
			Sort = SortRules.Common
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_00_048,
			CharaType = Item.CharaTypeNone,
			BaseImageID = BaseImageID.Base_00_048,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "いっちょまえガール",
			Sort = SortRules.Common
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_00_049,
			CharaType = Item.CharaTypeNone,
			BaseImageID = BaseImageID.Base_00_049,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "すごうでガール",
			Sort = SortRules.Common
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_00_050,
			CharaType = Item.CharaTypeNone,
			BaseImageID = BaseImageID.Base_00_050,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "伝説のガール",
			Sort = SortRules.Common
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_00_051,
			CharaType = Item.CharaTypeNone,
			BaseImageID = BaseImageID.Base_00_051,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "プールサイドファイトガール",
			Sort = SortRules.Common
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_00_052,
			CharaType = Item.CharaTypeNone,
			BaseImageID = BaseImageID.Base_00_052,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "ローシャンテ常連",
			Sort = SortRules.Common
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_00_053,
			CharaType = Item.CharaTypeNone,
			BaseImageID = BaseImageID.Base_00_053,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "ローシャンテゴールド会員",
			Sort = SortRules.Common
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_00_054,
			CharaType = Item.CharaTypeNone,
			BaseImageID = BaseImageID.Base_00_054,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "ローシャンテプラチナ会員",
			Sort = SortRules.Common
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_00_055,
			CharaType = Item.CharaTypeNone,
			BaseImageID = BaseImageID.Base_00_055,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "紅葉狩りファイトガール",
			Sort = SortRules.Common
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_00_056,
			CharaType = Item.CharaTypeNone,
			BaseImageID = BaseImageID.Base_00_056,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "革命はじめました",
			Sort = SortRules.Common
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_00_057,
			CharaType = Item.CharaTypeNone,
			BaseImageID = BaseImageID.Base_00_057,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "革命のヒットマン",
			Sort = SortRules.Common
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_00_058,
			CharaType = Item.CharaTypeNone,
			BaseImageID = BaseImageID.Base_00_058,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "革命の黒幕",
			Sort = SortRules.Common
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_00_059,
			CharaType = Item.CharaTypeNone,
			BaseImageID = BaseImageID.Base_00_059,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "愛の伝道師",
			Sort = SortRules.Common
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_00_060,
			CharaType = Item.CharaTypeNone,
			BaseImageID = BaseImageID.Base_00_060,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "闘将伝道師",
			Sort = SortRules.Common
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_00_061,
			CharaType = Item.CharaTypeNone,
			BaseImageID = BaseImageID.Base_00_061,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "らぶはむ伝道師",
			Sort = SortRules.Common
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_00_062,
			CharaType = Item.CharaTypeNone,
			BaseImageID = BaseImageID.Base_00_062,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "ファイトガール春祭り",
			Sort = SortRules.Common
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_00_063,
			CharaType = Item.CharaTypeNone,
			BaseImageID = BaseImageID.Base_00_063,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "ファイトガール大運動会",
			Sort = SortRules.Common
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_00_501,
			CharaType = Item.CharaTypeNone,
			BaseImageID = BaseImageID.Base_00_501,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "レディベルセルク",
			Sort = SortRules.EndContents
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_00_502,
			CharaType = Item.CharaTypeNone,
			BaseImageID = BaseImageID.Base_00_501,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "ミス・パーフェクト",
			Sort = SortRules.EndContents
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_00_503,
			CharaType = Item.CharaTypeNone,
			BaseImageID = BaseImageID.Base_00_501,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "完全武装アフロディーテ",
			Sort = SortRules.EndContents
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_00_504,
			CharaType = Item.CharaTypeNone,
			BaseImageID = BaseImageID.Base_00_501,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "閃光プリンセス",
			Sort = SortRules.EndContents
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_00_505,
			CharaType = Item.CharaTypeNone,
			BaseImageID = BaseImageID.Base_00_501,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "赤と黒のルピナス",
			Sort = SortRules.EndContents
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_00_506,
			CharaType = Item.CharaTypeNone,
			BaseImageID = BaseImageID.Base_00_501,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "コスプレイヤー雀士",
			Sort = SortRules.EndContents
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_00_507,
			CharaType = Item.CharaTypeNone,
			BaseImageID = BaseImageID.Base_00_501,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "愛夢プラチナハート",
			Sort = SortRules.EndContents
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_00_508,
			CharaType = Item.CharaTypeNone,
			BaseImageID = BaseImageID.Base_00_501,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "朱きヴァルキュリア",
			Sort = SortRules.EndContents
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_00_511,
			CharaType = Item.CharaTypeNone,
			BaseImageID = BaseImageID.Base_00_501,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "白衣の雀士",
			Sort = SortRules.EndContents
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_00_512,
			CharaType = Item.CharaTypeNone,
			BaseImageID = BaseImageID.Base_00_501,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "World Wanderer",
			Sort = SortRules.EndContents
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_01_001,
			CharaType = CharaType.Chara01,
			BaseImageID = BaseImageID.Base_01_001,
			RequiredFightSpiritNum = 10,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "ピンフガール",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_01_002,
			CharaType = CharaType.Chara01,
			BaseImageID = BaseImageID.Base_01_002,
			RequiredFightSpiritNum = 20,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_01_001,
			Name = "ド根性大根のように",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_01_003,
			CharaType = CharaType.Chara01,
			BaseImageID = BaseImageID.Base_01_003,
			RequiredFightSpiritNum = 30,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_01_002,
			Name = "ニコニコ脳内リセット",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_01_004,
			CharaType = CharaType.Chara01,
			BaseImageID = BaseImageID.Base_01_004,
			RequiredFightSpiritNum = 40,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_01_003,
			Name = "おきらくムードメーカー",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_01_005,
			CharaType = CharaType.Chara01,
			BaseImageID = BaseImageID.Base_01_005,
			RequiredFightSpiritNum = 50,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_01_004,
			Name = "ラブとピースは世界の基本",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_01_006,
			CharaType = CharaType.Chara01,
			BaseImageID = BaseImageID.Base_01_004,
			RequiredFightSpiritNum = 60,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_01_005,
			Name = "麻雀やれば世界は平和",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_01_007,
			CharaType = CharaType.Chara01,
			BaseImageID = BaseImageID.Base_01_003,
			RequiredFightSpiritNum = 70,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_01_006,
			Name = "快速超特急イチ抜けピ♪",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_01_008,
			CharaType = CharaType.Chara01,
			BaseImageID = BaseImageID.Base_01_002,
			RequiredFightSpiritNum = 80,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_01_007,
			Name = "ネリマの大根ウサギ",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_01_009,
			CharaType = CharaType.Chara01,
			BaseImageID = BaseImageID.Base_01_007,
			RequiredFightSpiritNum = 90,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_01_008,
			Name = "ピ～ッピッピッピッピ",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_01_010,
			CharaType = CharaType.Chara01,
			BaseImageID = BaseImageID.Base_01_006,
			RequiredFightSpiritNum = 100,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_01_009,
			Name = "ウサウサピース伝道師",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_01_701,
			CharaType = CharaType.Chara01,
			BaseImageID = BaseImageID.Base_01_009,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "ヒヨリファンクラブ会員",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_01_702,
			CharaType = CharaType.Chara01,
			BaseImageID = BaseImageID.Base_01_009,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "ヒヨリガチ勢",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_01_703,
			CharaType = CharaType.Chara01,
			BaseImageID = BaseImageID.Base_01_009,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "ヒヨリの一番",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_01_901,
			CharaType = CharaType.Chara01,
			BaseImageID = BaseImageID.Base_01_008,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "KAC2023 チャンピオン",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_01_902,
			CharaType = CharaType.Chara01,
			BaseImageID = BaseImageID.Base_01_008,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "KAC2023 ファイナリスト",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_02_001,
			CharaType = CharaType.Chara02,
			BaseImageID = BaseImageID.Base_02_001,
			RequiredFightSpiritNum = 10,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "リーチガール",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_02_002,
			CharaType = CharaType.Chara02,
			BaseImageID = BaseImageID.Base_02_002,
			RequiredFightSpiritNum = 20,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_02_001,
			Name = "ふわふわくせっ毛",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_02_003,
			CharaType = CharaType.Chara02,
			BaseImageID = BaseImageID.Base_02_003,
			RequiredFightSpiritNum = 30,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_02_002,
			Name = "うっかりなことばっかり",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_02_004,
			CharaType = CharaType.Chara02,
			BaseImageID = BaseImageID.Base_02_004,
			RequiredFightSpiritNum = 40,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_02_003,
			Name = "サバ缶の飼い主",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_02_005,
			CharaType = CharaType.Chara02,
			BaseImageID = BaseImageID.Base_02_005,
			RequiredFightSpiritNum = 50,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_02_004,
			Name = "千里の道も一直線！",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_02_006,
			CharaType = CharaType.Chara02,
			BaseImageID = BaseImageID.Base_02_004,
			RequiredFightSpiritNum = 60,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_02_005,
			Name = "ワンちゃん至上主義",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_02_007,
			CharaType = CharaType.Chara02,
			BaseImageID = BaseImageID.Base_02_005,
			RequiredFightSpiritNum = 70,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_02_006,
			Name = "信じて進むしかないよね",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_02_008,
			CharaType = CharaType.Chara02,
			BaseImageID = BaseImageID.Base_02_006,
			RequiredFightSpiritNum = 80,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_02_007,
			Name = "ネリマのガンバリーヌ",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_02_009,
			CharaType = CharaType.Chara02,
			BaseImageID = BaseImageID.Base_02_008,
			RequiredFightSpiritNum = 90,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_02_008,
			Name = "麻雀が打ちたいお年頃",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_02_010,
			CharaType = CharaType.Chara02,
			BaseImageID = BaseImageID.Base_02_007,
			RequiredFightSpiritNum = 100,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_02_009,
			Name = "ローシャンテの看板娘",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_02_701,
			CharaType = CharaType.Chara02,
			BaseImageID = BaseImageID.Base_02_010,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "センファンクラブ会員",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_02_702,
			CharaType = CharaType.Chara02,
			BaseImageID = BaseImageID.Base_02_010,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "センガチ勢",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_02_703,
			CharaType = CharaType.Chara02,
			BaseImageID = BaseImageID.Base_02_010,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "センの一番",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_02_901,
			CharaType = CharaType.Chara02,
			BaseImageID = BaseImageID.Base_02_009,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "KAC2023 チャンピオン",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_02_902,
			CharaType = CharaType.Chara02,
			BaseImageID = BaseImageID.Base_02_009,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "KAC2023 ファイナリスト",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_03_001,
			CharaType = CharaType.Chara03,
			BaseImageID = BaseImageID.Base_03_001,
			RequiredFightSpiritNum = 10,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "チートイガール",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_03_002,
			CharaType = CharaType.Chara03,
			BaseImageID = BaseImageID.Base_03_002,
			RequiredFightSpiritNum = 20,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_03_001,
			Name = "やさしい寮母さん",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_03_003,
			CharaType = CharaType.Chara03,
			BaseImageID = BaseImageID.Base_03_006,
			RequiredFightSpiritNum = 30,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_03_002,
			Name = "おいくらかしらね？",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_03_004,
			CharaType = CharaType.Chara03,
			BaseImageID = BaseImageID.Base_03_003,
			RequiredFightSpiritNum = 40,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_03_003,
			Name = "ナナツノグループ令嬢",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_03_005,
			CharaType = CharaType.Chara03,
			BaseImageID = BaseImageID.Base_03_005,
			RequiredFightSpiritNum = 50,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_03_004,
			Name = "ニコニコ笑顔でみんなに愛を",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_03_006,
			CharaType = CharaType.Chara03,
			BaseImageID = BaseImageID.Base_03_003,
			RequiredFightSpiritNum = 60,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_03_005,
			Name = "お世話十段",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_03_007,
			CharaType = CharaType.Chara03,
			BaseImageID = BaseImageID.Base_03_002,
			RequiredFightSpiritNum = 70,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_03_006,
			Name = "勝利の女神がニコッ♪",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_03_008,
			CharaType = CharaType.Chara03,
			BaseImageID = BaseImageID.Base_03_005,
			RequiredFightSpiritNum = 80,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_03_007,
			Name = "財力×体格×笑顔＝お世話力",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_03_009,
			CharaType = CharaType.Chara03,
			BaseImageID = BaseImageID.Base_03_008,
			RequiredFightSpiritNum = 90,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_03_008,
			Name = "金持ち",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_03_010,
			CharaType = CharaType.Chara03,
			BaseImageID = BaseImageID.Base_03_007,
			RequiredFightSpiritNum = 100,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_03_009,
			Name = "激愛のフライパン使い",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_03_701,
			CharaType = CharaType.Chara03,
			BaseImageID = BaseImageID.Base_03_010,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "ツミレファンクラブ会員",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_03_702,
			CharaType = CharaType.Chara03,
			BaseImageID = BaseImageID.Base_03_010,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "ツミレガチ勢",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_03_703,
			CharaType = CharaType.Chara03,
			BaseImageID = BaseImageID.Base_03_010,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "ツミレの一番",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_03_901,
			CharaType = CharaType.Chara03,
			BaseImageID = BaseImageID.Base_03_009,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "KAC2023 チャンピオン",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_03_902,
			CharaType = CharaType.Chara03,
			BaseImageID = BaseImageID.Base_03_009,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "KAC2023 ファイナリスト",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_04_001,
			CharaType = CharaType.Chara04,
			BaseImageID = BaseImageID.Base_04_001,
			RequiredFightSpiritNum = 10,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "テンホーガール",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_04_002,
			CharaType = CharaType.Chara04,
			BaseImageID = BaseImageID.Base_04_002,
			RequiredFightSpiritNum = 20,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_04_001,
			Name = "吸い取り天使",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_04_003,
			CharaType = CharaType.Chara04,
			BaseImageID = BaseImageID.Base_04_003,
			RequiredFightSpiritNum = 30,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_04_002,
			Name = "格上すぎちゃわない？\u3000",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_04_004,
			CharaType = CharaType.Chara04,
			BaseImageID = BaseImageID.Base_04_004,
			RequiredFightSpiritNum = 40,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_04_003,
			Name = "デビリッシュエンジェル",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_04_005,
			CharaType = CharaType.Chara04,
			BaseImageID = BaseImageID.Base_04_004,
			RequiredFightSpiritNum = 50,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_04_004,
			Name = "幸運、豪運、大天運！",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_04_006,
			CharaType = CharaType.Chara04,
			BaseImageID = BaseImageID.Base_04_005,
			RequiredFightSpiritNum = 60,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_04_005,
			Name = "ちょっとまてまて摩天楼",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_04_007,
			CharaType = CharaType.Chara04,
			BaseImageID = BaseImageID.Base_04_003,
			RequiredFightSpiritNum = 70,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_04_006,
			Name = "百億銀河年早いでーす！",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_04_008,
			CharaType = CharaType.Chara04,
			BaseImageID = BaseImageID.Base_04_005,
			RequiredFightSpiritNum = 80,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_04_007,
			Name = "ワンオペツッコミ",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_04_009,
			CharaType = CharaType.Chara04,
			BaseImageID = BaseImageID.Base_04_007,
			RequiredFightSpiritNum = 90,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_04_008,
			Name = "CV：井澤詩織",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_04_010,
			CharaType = CharaType.Chara04,
			BaseImageID = BaseImageID.Base_04_006,
			RequiredFightSpiritNum = 100,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_04_009,
			Name = "33万分の1の奇跡",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_04_701,
			CharaType = CharaType.Chara04,
			BaseImageID = BaseImageID.Base_04_009,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "テンシファンクラブ会員",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_04_702,
			CharaType = CharaType.Chara04,
			BaseImageID = BaseImageID.Base_04_009,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "テンシガチ勢",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_04_703,
			CharaType = CharaType.Chara04,
			BaseImageID = BaseImageID.Base_04_009,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "テンシの一番",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_04_901,
			CharaType = CharaType.Chara04,
			BaseImageID = BaseImageID.Base_04_008,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "KAC2023 チャンピオン",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_04_902,
			CharaType = CharaType.Chara04,
			BaseImageID = BaseImageID.Base_04_008,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "KAC2023 ファイナリスト",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_05_001,
			CharaType = CharaType.Chara05,
			BaseImageID = BaseImageID.Base_05_001,
			RequiredFightSpiritNum = 10,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "タンヤオガール",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_05_002,
			CharaType = CharaType.Chara05,
			BaseImageID = BaseImageID.Base_05_002,
			RequiredFightSpiritNum = 20,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_05_001,
			Name = "COOOOOOOL!!",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_05_003,
			CharaType = CharaType.Chara05,
			BaseImageID = BaseImageID.Base_05_003,
			RequiredFightSpiritNum = 30,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_05_002,
			Name = "ちょっぴりカッコつけたくて",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_05_004,
			CharaType = CharaType.Chara05,
			BaseImageID = BaseImageID.Base_05_002,
			RequiredFightSpiritNum = 40,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_05_003,
			Name = "壱高のおしゃれ番長",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_05_005,
			CharaType = CharaType.Chara05,
			BaseImageID = BaseImageID.Base_05_003,
			RequiredFightSpiritNum = 50,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_05_004,
			Name = "勝負は常にシンプル、クール",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_05_006,
			CharaType = CharaType.Chara05,
			BaseImageID = BaseImageID.Base_05_004,
			RequiredFightSpiritNum = 60,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_05_005,
			Name = "食は命",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_05_007,
			CharaType = CharaType.Chara05,
			BaseImageID = BaseImageID.Base_05_005,
			RequiredFightSpiritNum = 70,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_05_006,
			Name = "信念を貫け価値のため",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_05_008,
			CharaType = CharaType.Chara05,
			BaseImageID = BaseImageID.Base_05_006,
			RequiredFightSpiritNum = 80,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_05_007,
			Name = "ネリマの黒豹",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_05_009,
			CharaType = CharaType.Chara05,
			BaseImageID = BaseImageID.Base_05_008,
			RequiredFightSpiritNum = 90,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_05_008,
			Name = "肉でも食べてBeクール",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_05_010,
			CharaType = CharaType.Chara05,
			BaseImageID = BaseImageID.Base_05_007,
			RequiredFightSpiritNum = 100,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_05_009,
			Name = "安く見てるとケガするよ",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_05_701,
			CharaType = CharaType.Chara05,
			BaseImageID = BaseImageID.Base_05_010,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "ヤオファンクラブ会員",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_05_702,
			CharaType = CharaType.Chara05,
			BaseImageID = BaseImageID.Base_05_010,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "ヤオガチ勢",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_05_703,
			CharaType = CharaType.Chara05,
			BaseImageID = BaseImageID.Base_05_010,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "ヤオの一番",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_05_901,
			CharaType = CharaType.Chara05,
			BaseImageID = BaseImageID.Base_05_009,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "KAC2023 チャンピオン",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_05_902,
			CharaType = CharaType.Chara05,
			BaseImageID = BaseImageID.Base_05_009,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "KAC2023 ファイナリスト",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_06_001,
			CharaType = CharaType.Chara06,
			BaseImageID = BaseImageID.Base_06_001,
			RequiredFightSpiritNum = 10,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "サンアンガール",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_06_002,
			CharaType = CharaType.Chara06,
			BaseImageID = BaseImageID.Base_06_002,
			RequiredFightSpiritNum = 20,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_06_001,
			Name = "暗黒信徒募集中",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_06_003,
			CharaType = CharaType.Chara06,
			BaseImageID = BaseImageID.Base_06_003,
			RequiredFightSpiritNum = 30,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_06_002,
			Name = "因果よ秩序よ狂うがいいわ",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_06_004,
			CharaType = CharaType.Chara06,
			BaseImageID = BaseImageID.Base_06_003,
			RequiredFightSpiritNum = 40,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_06_003,
			Name = "路上で見かける暗黒神",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_06_005,
			CharaType = CharaType.Chara06,
			BaseImageID = BaseImageID.Base_06_004,
			RequiredFightSpiritNum = 50,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_06_004,
			Name = "我が望むは冥き刻！",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_06_006,
			CharaType = CharaType.Chara06,
			BaseImageID = BaseImageID.Base_06_005,
			RequiredFightSpiritNum = 60,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_06_005,
			Name = "ソロキャンマイスター",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_06_007,
			CharaType = CharaType.Chara06,
			BaseImageID = BaseImageID.Base_06_002,
			RequiredFightSpiritNum = 70,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_06_006,
			Name = "征け茨の道を",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_06_008,
			CharaType = CharaType.Chara06,
			BaseImageID = BaseImageID.Base_06_005,
			RequiredFightSpiritNum = 80,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_06_007,
			Name = "鍛え抜かれた自活力",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_06_009,
			CharaType = CharaType.Chara06,
			BaseImageID = BaseImageID.Base_06_007,
			RequiredFightSpiritNum = 90,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_06_008,
			Name = "ア～ンコクックックッ",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_06_010,
			CharaType = CharaType.Chara06,
			BaseImageID = BaseImageID.Base_06_006,
			RequiredFightSpiritNum = 100,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_06_009,
			Name = "闇の裁縫職人",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_06_701,
			CharaType = CharaType.Chara06,
			BaseImageID = BaseImageID.Base_06_009,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "ミツバファンクラブ会員",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_06_702,
			CharaType = CharaType.Chara06,
			BaseImageID = BaseImageID.Base_06_009,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "ミツバガチ勢",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_06_703,
			CharaType = CharaType.Chara06,
			BaseImageID = BaseImageID.Base_06_009,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "ミツバの一番",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_06_901,
			CharaType = CharaType.Chara06,
			BaseImageID = BaseImageID.Base_06_008,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "KAC2023 チャンピオン",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_06_902,
			CharaType = CharaType.Chara06,
			BaseImageID = BaseImageID.Base_06_008,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "KAC2023 ファイナリスト",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_07_001,
			CharaType = CharaType.Chara07,
			BaseImageID = BaseImageID.Base_07_001,
			RequiredFightSpiritNum = 10,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "トイトイガール",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_07_002,
			CharaType = CharaType.Chara07,
			BaseImageID = BaseImageID.Base_07_002,
			RequiredFightSpiritNum = 20,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_07_001,
			Name = "超銀河ゲーマー",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_07_003,
			CharaType = CharaType.Chara07,
			BaseImageID = BaseImageID.Base_07_003,
			RequiredFightSpiritNum = 30,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_07_002,
			Name = "チートなしでもワンパン",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_07_004,
			CharaType = CharaType.Chara07,
			BaseImageID = BaseImageID.Base_07_004,
			RequiredFightSpiritNum = 40,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_07_003,
			Name = "遠い星から来たぐーたら",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_07_005,
			CharaType = CharaType.Chara07,
			BaseImageID = BaseImageID.Base_07_003,
			RequiredFightSpiritNum = 50,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_07_004,
			Name = "コツをつかめば100パー勝利",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_07_006,
			CharaType = CharaType.Chara07,
			BaseImageID = BaseImageID.Base_07_002,
			RequiredFightSpiritNum = 60,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_07_005,
			Name = "乱数調整中",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_07_007,
			CharaType = CharaType.Chara07,
			BaseImageID = BaseImageID.Base_07_005,
			RequiredFightSpiritNum = 70,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_07_006,
			Name = "爆アゲエキスパンション",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_07_008,
			CharaType = CharaType.Chara07,
			BaseImageID = BaseImageID.Base_07_004,
			RequiredFightSpiritNum = 80,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_07_007,
			Name = "侵略は明日から",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_07_009,
			CharaType = CharaType.Chara07,
			BaseImageID = BaseImageID.Base_07_007,
			RequiredFightSpiritNum = 90,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_07_008,
			Name = "スペーシアン",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_07_010,
			CharaType = CharaType.Chara07,
			BaseImageID = BaseImageID.Base_07_006,
			RequiredFightSpiritNum = 100,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_07_009,
			Name = "果てなき甘やかされライフ",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_07_701,
			CharaType = CharaType.Chara07,
			BaseImageID = BaseImageID.Base_07_009,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "トイトイファンクラブ会員",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_07_702,
			CharaType = CharaType.Chara07,
			BaseImageID = BaseImageID.Base_07_009,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "トイトイガチ勢",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_07_703,
			CharaType = CharaType.Chara07,
			BaseImageID = BaseImageID.Base_07_009,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "トイトイの一番",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_07_901,
			CharaType = CharaType.Chara07,
			BaseImageID = BaseImageID.Base_07_008,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "KAC2023 チャンピオン",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_07_902,
			CharaType = CharaType.Chara07,
			BaseImageID = BaseImageID.Base_07_008,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "KAC2023 ファイナリスト",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_08_001,
			CharaType = CharaType.Chara08,
			BaseImageID = BaseImageID.Base_08_001,
			RequiredFightSpiritNum = 10,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "コクシガール",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_08_002,
			CharaType = CharaType.Chara08,
			BaseImageID = BaseImageID.Base_08_002,
			RequiredFightSpiritNum = 20,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_08_001,
			Name = "大学芋命",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_08_003,
			CharaType = CharaType.Chara08,
			BaseImageID = BaseImageID.Base_08_003,
			RequiredFightSpiritNum = 30,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_08_002,
			Name = "まだまだまだまだ",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_08_004,
			CharaType = CharaType.Chara08,
			BaseImageID = BaseImageID.Base_08_004,
			RequiredFightSpiritNum = 40,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_08_003,
			Name = "鍛錬！鍛錬！鍛錬！",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_08_005,
			CharaType = CharaType.Chara08,
			BaseImageID = BaseImageID.Base_08_004,
			RequiredFightSpiritNum = 50,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_08_004,
			Name = "わが征くは比類なき強者が道",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_08_006,
			CharaType = CharaType.Chara08,
			BaseImageID = BaseImageID.Base_08_005,
			RequiredFightSpiritNum = 60,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_08_005,
			Name = "点棒居合い切り",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_08_007,
			CharaType = CharaType.Chara08,
			BaseImageID = BaseImageID.Base_08_002,
			RequiredFightSpiritNum = 70,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_08_006,
			Name = "乱れた心が鳴き叫んでいる",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_08_008,
			CharaType = CharaType.Chara08,
			BaseImageID = BaseImageID.Base_08_006,
			RequiredFightSpiritNum = 80,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_08_007,
			Name = "くみゃ",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_08_009,
			CharaType = CharaType.Chara08,
			BaseImageID = BaseImageID.Base_08_008,
			RequiredFightSpiritNum = 90,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_08_008,
			Name = "遊びは終わりだ",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_08_010,
			CharaType = CharaType.Chara08,
			BaseImageID = BaseImageID.Base_08_007,
			RequiredFightSpiritNum = 100,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_08_009,
			Name = "帯刀堅物風紀委員",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_08_701,
			CharaType = CharaType.Chara08,
			BaseImageID = BaseImageID.Base_08_010,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "ムサシファンクラブ会員",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_08_702,
			CharaType = CharaType.Chara08,
			BaseImageID = BaseImageID.Base_08_010,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "ムサシガチ勢",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_08_703,
			CharaType = CharaType.Chara08,
			BaseImageID = BaseImageID.Base_08_010,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "ムサシの一番",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_08_901,
			CharaType = CharaType.Chara08,
			BaseImageID = BaseImageID.Base_08_009,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "KAC2023 チャンピオン",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_08_902,
			CharaType = CharaType.Chara08,
			BaseImageID = BaseImageID.Base_08_009,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "KAC2023 ファイナリスト",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_09_001,
			CharaType = CharaType.Chara09,
			BaseImageID = BaseImageID.Base_09_001,
			RequiredFightSpiritNum = 10,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "ボンバーガール",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_09_002,
			CharaType = CharaType.Chara09,
			BaseImageID = BaseImageID.Base_09_002,
			RequiredFightSpiritNum = 20,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_09_001,
			Name = "よろろんにゃ～ん♪",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_09_003,
			CharaType = CharaType.Chara09,
			BaseImageID = BaseImageID.Base_09_003,
			RequiredFightSpiritNum = 30,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_09_002,
			Name = "黄色い声をかしましく",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_09_004,
			CharaType = CharaType.Chara09,
			BaseImageID = BaseImageID.Base_09_004,
			RequiredFightSpiritNum = 40,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_09_003,
			Name = "IQ100億",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_09_005,
			CharaType = CharaType.Chara09,
			BaseImageID = BaseImageID.Base_09_005,
			RequiredFightSpiritNum = 50,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_09_004,
			Name = "にゃにゃにゃにゃ～ん",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_09_006,
			CharaType = CharaType.Chara09,
			BaseImageID = BaseImageID.Base_09_007,
			RequiredFightSpiritNum = 60,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_09_005,
			Name = "パラレルトラベラー",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_09_007,
			CharaType = CharaType.Chara09,
			BaseImageID = BaseImageID.Base_09_005,
			RequiredFightSpiritNum = 70,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_09_006,
			Name = "はち切れそうな胸の",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_09_008,
			CharaType = CharaType.Chara09,
			BaseImageID = BaseImageID.Base_09_005,
			RequiredFightSpiritNum = 80,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_09_007,
			Name = "打倒モモさん",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_09_009,
			CharaType = CharaType.Chara09,
			BaseImageID = BaseImageID.Base_09_008,
			RequiredFightSpiritNum = 90,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_09_008,
			Name = "ニャガーン!!",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_09_010,
			CharaType = CharaType.Chara09,
			BaseImageID = BaseImageID.Base_09_006,
			RequiredFightSpiritNum = 100,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_09_009,
			Name = "ザ・ジーニアス",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_09_701,
			CharaType = CharaType.Chara09,
			BaseImageID = BaseImageID.Base_09_010,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "パインファンクラブ会員",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_09_702,
			CharaType = CharaType.Chara09,
			BaseImageID = BaseImageID.Base_09_010,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "パインガチ勢",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_09_703,
			CharaType = CharaType.Chara09,
			BaseImageID = BaseImageID.Base_09_010,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "パインの一番",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_09_901,
			CharaType = CharaType.Chara09,
			BaseImageID = BaseImageID.Base_09_009,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "KAC2023 チャンピオン",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_09_902,
			CharaType = CharaType.Chara09,
			BaseImageID = BaseImageID.Base_09_009,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "KAC2023 ファイナリスト",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_10_001,
			CharaType = CharaType.Chara10,
			BaseImageID = BaseImageID.Base_10_001,
			RequiredFightSpiritNum = 10,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "ときめきガール",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_10_002,
			CharaType = CharaType.Chara10,
			BaseImageID = BaseImageID.Base_10_002,
			RequiredFightSpiritNum = 20,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_10_001,
			Name = "クラシックとかを良く聴きます",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_10_003,
			CharaType = CharaType.Chara10,
			BaseImageID = BaseImageID.Base_10_003,
			RequiredFightSpiritNum = 30,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_10_002,
			Name = "Take the Chance",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_10_004,
			CharaType = CharaType.Chara10,
			BaseImageID = BaseImageID.Base_10_004,
			RequiredFightSpiritNum = 40,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_10_003,
			Name = "家が隣の幼なじみ",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_10_005,
			CharaType = CharaType.Chara10,
			BaseImageID = BaseImageID.Base_10_005,
			RequiredFightSpiritNum = 50,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_10_004,
			Name = "仕上げは上出来、準備はOK",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_10_006,
			CharaType = CharaType.Chara10,
			BaseImageID = BaseImageID.Base_10_003,
			RequiredFightSpiritNum = 60,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_10_005,
			Name = "きらめき高校のスーパーアイドル",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_10_007,
			CharaType = CharaType.Chara10,
			BaseImageID = BaseImageID.Base_10_002,
			RequiredFightSpiritNum = 70,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_10_006,
			Name = "あなただけに見つめてほしい",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_10_008,
			CharaType = CharaType.Chara10,
			BaseImageID = BaseImageID.Base_10_006,
			RequiredFightSpiritNum = 80,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_10_007,
			Name = "伝説の樹の下で待っています",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_10_009,
			CharaType = CharaType.Chara10,
			BaseImageID = BaseImageID.Base_10_008,
			RequiredFightSpiritNum = 90,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_10_008,
			Name = "大先輩",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_10_010,
			CharaType = CharaType.Chara10,
			BaseImageID = BaseImageID.Base_10_007,
			RequiredFightSpiritNum = 100,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_10_009,
			Name = "デートは現地集合",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_10_701,
			CharaType = CharaType.Chara10,
			BaseImageID = BaseImageID.Base_10_010,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "詩織ファンクラブ会員",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_10_702,
			CharaType = CharaType.Chara10,
			BaseImageID = BaseImageID.Base_10_010,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "詩織ガチ勢",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_10_703,
			CharaType = CharaType.Chara10,
			BaseImageID = BaseImageID.Base_10_010,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "詩織の一番",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_10_901,
			CharaType = CharaType.Chara10,
			BaseImageID = BaseImageID.Base_10_009,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "KAC2023 チャンピオン",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_10_902,
			CharaType = CharaType.Chara10,
			BaseImageID = BaseImageID.Base_10_009,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "KAC2023 ファイナリスト",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_11_001,
			CharaType = CharaType.Chara11,
			BaseImageID = BaseImageID.Base_11_001,
			RequiredFightSpiritNum = 10,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "ホンイツガール",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_11_002,
			CharaType = CharaType.Chara11,
			BaseImageID = BaseImageID.Base_11_002,
			RequiredFightSpiritNum = 20,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_11_001,
			Name = "タマ川ウォーカー",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_11_003,
			CharaType = CharaType.Chara11,
			BaseImageID = BaseImageID.Base_11_003,
			RequiredFightSpiritNum = 30,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_11_002,
			Name = "一瞬のスキ忍び寄る",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_11_004,
			CharaType = CharaType.Chara11,
			BaseImageID = BaseImageID.Base_11_004,
			RequiredFightSpiritNum = 40,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_11_003,
			Name = "普通の女子高生",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_11_005,
			CharaType = CharaType.Chara11,
			BaseImageID = BaseImageID.Base_11_003,
			RequiredFightSpiritNum = 50,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_11_004,
			Name = "世界はキレイなだけじゃない",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_11_006,
			CharaType = CharaType.Chara11,
			BaseImageID = BaseImageID.Base_11_002,
			RequiredFightSpiritNum = 60,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_11_005,
			Name = "肉を食ったら家族",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_11_007,
			CharaType = CharaType.Chara11,
			BaseImageID = BaseImageID.Base_11_005,
			RequiredFightSpiritNum = 70,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_11_006,
			Name = "嵌めて欺いて消し去って",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_11_008,
			CharaType = CharaType.Chara11,
			BaseImageID = BaseImageID.Base_11_006,
			RequiredFightSpiritNum = 80,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_11_007,
			Name = "卓上のヒットマン",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_11_009,
			CharaType = CharaType.Chara11,
			BaseImageID = BaseImageID.Base_11_007,
			RequiredFightSpiritNum = 90,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_11_008,
			Name = "食欲 > 仕事欲",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_11_010,
			CharaType = CharaType.Chara11,
			BaseImageID = BaseImageID.Base_11_006,
			RequiredFightSpiritNum = 100,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_11_009,
			Name = "デストロイゼムオール",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_11_701,
			CharaType = CharaType.Chara11,
			BaseImageID = BaseImageID.Base_11_008,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "カオスファンクラブ会員",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_11_702,
			CharaType = CharaType.Chara11,
			BaseImageID = BaseImageID.Base_11_008,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "カオスガチ勢",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_11_703,
			CharaType = CharaType.Chara11,
			BaseImageID = BaseImageID.Base_11_008,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "カオスの一番",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_12_001,
			CharaType = CharaType.Chara12,
			BaseImageID = BaseImageID.Base_12_001,
			RequiredFightSpiritNum = 10,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "チンイツガール",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_12_002,
			CharaType = CharaType.Chara12,
			BaseImageID = BaseImageID.Base_12_002,
			RequiredFightSpiritNum = 20,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_12_001,
			Name = "純白ワンマン社長",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_12_003,
			CharaType = CharaType.Chara12,
			BaseImageID = BaseImageID.Base_12_003,
			RequiredFightSpiritNum = 30,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_12_002,
			Name = "彩りなど許されない",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_12_004,
			CharaType = CharaType.Chara12,
			BaseImageID = BaseImageID.Base_12_002,
			RequiredFightSpiritNum = 40,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_12_003,
			Name = "イッシキファミリーのボス",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_12_005,
			CharaType = CharaType.Chara12,
			BaseImageID = BaseImageID.Base_12_004,
			RequiredFightSpiritNum = 50,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_12_004,
			Name = "世界よ染まれ一色に！",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_12_006,
			CharaType = CharaType.Chara12,
			BaseImageID = BaseImageID.Base_12_005,
			RequiredFightSpiritNum = 60,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_12_005,
			Name = "優良人材募集中",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_12_007,
			CharaType = CharaType.Chara12,
			BaseImageID = BaseImageID.Base_12_003,
			RequiredFightSpiritNum = 70,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_12_006,
			Name = "咲かせるわ咲き誇るわ",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_12_008,
			CharaType = CharaType.Chara12,
			BaseImageID = BaseImageID.Base_12_004,
			RequiredFightSpiritNum = 80,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_12_007,
			Name = "きゃおすちゅわ～ん♪",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_12_009,
			CharaType = CharaType.Chara12,
			BaseImageID = BaseImageID.Base_12_007,
			RequiredFightSpiritNum = 90,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_12_008,
			Name = "白服タッコー",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_12_010,
			CharaType = CharaType.Chara12,
			BaseImageID = BaseImageID.Base_12_006,
			RequiredFightSpiritNum = 100,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_12_009,
			Name = "驚きの白さ",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_12_701,
			CharaType = CharaType.Chara12,
			BaseImageID = BaseImageID.Base_12_008,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "クリアファンクラブ会員",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_12_702,
			CharaType = CharaType.Chara12,
			BaseImageID = BaseImageID.Base_12_008,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "クリアガチ勢",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_12_703,
			CharaType = CharaType.Chara12,
			BaseImageID = BaseImageID.Base_12_008,
			RequiredFightSpiritNum = 0,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "クリアの一番",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_13_001,
			CharaType = CharaType.Chara13,
			BaseImageID = BaseImageID.Base_13_001,
			RequiredFightSpiritNum = 10,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "リューイーソーガール",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_13_002,
			CharaType = CharaType.Chara13,
			BaseImageID = BaseImageID.Base_13_007,
			RequiredFightSpiritNum = 20,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_13_001,
			Name = "今日も元気に人類反逆",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_13_003,
			CharaType = CharaType.Chara13,
			BaseImageID = BaseImageID.Base_13_003,
			RequiredFightSpiritNum = 30,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_13_002,
			Name = "索子最高\u3000赤捨てろ",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_13_004,
			CharaType = CharaType.Chara13,
			BaseImageID = BaseImageID.Base_13_004,
			RequiredFightSpiritNum = 40,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_13_003,
			Name = "ポンコツという機械はない",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_13_005,
			CharaType = CharaType.Chara13,
			BaseImageID = BaseImageID.Base_13_005,
			RequiredFightSpiritNum = 50,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_13_004,
			Name = "心も体もオールグリーン！",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_13_006,
			CharaType = CharaType.Chara13,
			BaseImageID = BaseImageID.Base_13_006,
			RequiredFightSpiritNum = 60,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_13_005,
			Name = "マスターしゅきー♪",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_13_007,
			CharaType = CharaType.Chara13,
			BaseImageID = BaseImageID.Base_13_001,
			RequiredFightSpiritNum = 70,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_13_006,
			Name = "全緑機神",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_13_008,
			CharaType = CharaType.Chara13,
			BaseImageID = BaseImageID.Base_13_005,
			RequiredFightSpiritNum = 80,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_13_007,
			Name = "親からもらった緑のボディ",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_13_009,
			CharaType = CharaType.Chara13,
			BaseImageID = BaseImageID.Base_13_008,
			RequiredFightSpiritNum = 90,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_13_008,
			Name = "バッチこーい！",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_13_010,
			CharaType = CharaType.Chara13,
			BaseImageID = BaseImageID.Base_13_006,
			RequiredFightSpiritNum = 100,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_13_009,
			Name = "恋する雀ドロイド",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_14_001,
			CharaType = CharaType.Chara14,
			BaseImageID = BaseImageID.Base_14_001,
			RequiredFightSpiritNum = 10,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "小悪魔ガール",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_14_002,
			CharaType = CharaType.Chara14,
			BaseImageID = BaseImageID.Base_14_002,
			RequiredFightSpiritNum = 20,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_14_001,
			Name = "わからせてみたら？",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_14_003,
			CharaType = CharaType.Chara14,
			BaseImageID = BaseImageID.Base_14_001,
			RequiredFightSpiritNum = 30,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_14_002,
			Name = "最高の堕落教えちゃう",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_14_004,
			CharaType = CharaType.Chara14,
			BaseImageID = BaseImageID.Base_14_003,
			RequiredFightSpiritNum = 40,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_14_003,
			Name = "自称悪い子",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_14_005,
			CharaType = CharaType.Chara14,
			BaseImageID = BaseImageID.Base_14_004,
			RequiredFightSpiritNum = 50,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_14_004,
			Name = "魔法の国のいたずらっこ！",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_14_006,
			CharaType = CharaType.Chara14,
			BaseImageID = BaseImageID.Base_14_005,
			RequiredFightSpiritNum = 60,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_14_005,
			Name = "きゃ～おまわりさ～ん♪",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_14_007,
			CharaType = CharaType.Chara14,
			BaseImageID = BaseImageID.Base_14_002,
			RequiredFightSpiritNum = 70,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_14_006,
			Name = "のーみそトロトロしてあげる",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_14_008,
			CharaType = CharaType.Chara14,
			BaseImageID = BaseImageID.Base_14_004,
			RequiredFightSpiritNum = 80,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_14_007,
			Name = "ぐりあろはQMAのこです",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_14_009,
			CharaType = CharaType.Chara14,
			BaseImageID = BaseImageID.Base_14_006,
			RequiredFightSpiritNum = 90,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_14_008,
			Name = "グリム・アロエに悲しき過去",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_14_010,
			CharaType = CharaType.Chara14,
			BaseImageID = BaseImageID.Base_14_001,
			RequiredFightSpiritNum = 100,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_14_009,
			Name = "サキュバスチアコスの祖",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_15_001,
			CharaType = CharaType.Chara15,
			BaseImageID = BaseImageID.Base_15_001,
			RequiredFightSpiritNum = 10,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "小三ガール",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_15_002,
			CharaType = CharaType.Chara15,
			BaseImageID = BaseImageID.Base_15_002,
			RequiredFightSpiritNum = 20,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_15_001,
			Name = "恋に恋するお年頃",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_15_003,
			CharaType = CharaType.Chara15,
			BaseImageID = BaseImageID.Base_15_003,
			RequiredFightSpiritNum = 30,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_15_002,
			Name = "告白\u3000告発\u3000百発百中",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_15_004,
			CharaType = CharaType.Chara15,
			BaseImageID = BaseImageID.Base_15_004,
			RequiredFightSpiritNum = 40,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_15_003,
			Name = "マジカル怪力童女",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_15_005,
			CharaType = CharaType.Chara15,
			BaseImageID = BaseImageID.Base_15_002,
			RequiredFightSpiritNum = 50,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_15_004,
			Name = "みなさん恋してますか？",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_15_006,
			CharaType = CharaType.Chara15,
			BaseImageID = BaseImageID.Base_15_005,
			RequiredFightSpiritNum = 60,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_15_005,
			Name = "Power is LOVE♪",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_15_007,
			CharaType = CharaType.Chara15,
			BaseImageID = BaseImageID.Base_15_004,
			RequiredFightSpiritNum = 70,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_15_006,
			Name = "ラブリーナックル非道暴動",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_15_008,
			CharaType = CharaType.Chara15,
			BaseImageID = BaseImageID.Base_15_001,
			RequiredFightSpiritNum = 80,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_15_007,
			Name = "魔法少女 らぶはむココア",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_15_009,
			CharaType = CharaType.Chara15,
			BaseImageID = BaseImageID.Base_15_007,
			RequiredFightSpiritNum = 90,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_15_008,
			Name = "ロメロスペシャル",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_15_010,
			CharaType = CharaType.Chara15,
			BaseImageID = BaseImageID.Base_15_006,
			RequiredFightSpiritNum = 100,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_15_009,
			Name = "私の方が強いんですから♪",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_16_001,
			CharaType = CharaType.Chara16,
			BaseImageID = BaseImageID.Base_16_001,
			RequiredFightSpiritNum = 10,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "大三元ガール",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_16_002,
			CharaType = CharaType.Chara16,
			BaseImageID = BaseImageID.Base_16_002,
			RequiredFightSpiritNum = 20,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_16_001,
			Name = "生涯現役",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_16_003,
			CharaType = CharaType.Chara16,
			BaseImageID = BaseImageID.Base_16_003,
			RequiredFightSpiritNum = 30,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_16_002,
			Name = "魅惑のウエポン",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_16_004,
			CharaType = CharaType.Chara16,
			BaseImageID = BaseImageID.Base_16_004,
			RequiredFightSpiritNum = 40,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_16_003,
			Name = "マジカル拳骨教師",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_16_005,
			CharaType = CharaType.Chara16,
			BaseImageID = BaseImageID.Base_16_005,
			RequiredFightSpiritNum = 50,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_16_004,
			Name = "キサマら愛し合ってるか？",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_16_006,
			CharaType = CharaType.Chara16,
			BaseImageID = BaseImageID.Base_16_006,
			RequiredFightSpiritNum = 60,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_16_005,
			Name = "LOVE is Power♪",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_16_007,
			CharaType = CharaType.Chara16,
			BaseImageID = BaseImageID.Base_16_007,
			RequiredFightSpiritNum = 70,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_16_006,
			Name = "愛が最強",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_16_008,
			CharaType = CharaType.Chara16,
			BaseImageID = BaseImageID.Base_16_001,
			RequiredFightSpiritNum = 80,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_16_007,
			Name = "魔法少女 らぶはむダイア",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_16_009,
			CharaType = CharaType.Chara16,
			BaseImageID = BaseImageID.Base_16_008,
			RequiredFightSpiritNum = 90,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_16_008,
			Name = "M・M・P",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_16_010,
			CharaType = CharaType.Chara16,
			BaseImageID = BaseImageID.Base_16_005,
			RequiredFightSpiritNum = 100,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_16_009,
			Name = "ダイアちゃんに全てお任せ",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_17_001,
			CharaType = CharaType.Chara17,
			BaseImageID = BaseImageID.Base_17_001,
			RequiredFightSpiritNum = 10,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "ダブリーガール",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_17_002,
			CharaType = CharaType.Chara17,
			BaseImageID = BaseImageID.Base_17_002,
			RequiredFightSpiritNum = 20,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_17_001,
			Name = "マヨは飲み物",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_17_003,
			CharaType = CharaType.Chara17,
			BaseImageID = BaseImageID.Base_17_003,
			RequiredFightSpiritNum = 30,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_17_002,
			Name = "明日の自分に丸投げ",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_17_004,
			CharaType = CharaType.Chara17,
			BaseImageID = BaseImageID.Base_17_004,
			RequiredFightSpiritNum = 40,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_17_003,
			Name = "堕天使シスター",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_17_005,
			CharaType = CharaType.Chara17,
			BaseImageID = BaseImageID.Base_17_005,
			RequiredFightSpiritNum = 50,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_17_004,
			Name = "天使がだらけて何が悪い",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_17_006,
			CharaType = CharaType.Chara17,
			BaseImageID = BaseImageID.Base_17_006,
			RequiredFightSpiritNum = 60,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_17_005,
			Name = "リアタイ至上主義",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_17_007,
			CharaType = CharaType.Chara17,
			BaseImageID = BaseImageID.Base_17_005,
			RequiredFightSpiritNum = 70,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_17_006,
			Name = "おフトゥンの中最高じゃん",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_17_008,
			CharaType = CharaType.Chara17,
			BaseImageID = BaseImageID.Base_17_007,
			RequiredFightSpiritNum = 80,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_17_007,
			Name = "重い女",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_17_009,
			CharaType = CharaType.Chara17,
			BaseImageID = BaseImageID.Base_17_008,
			RequiredFightSpiritNum = 90,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_17_008,
			Name = "ピッツァターイム",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_17_010,
			CharaType = CharaType.Chara17,
			BaseImageID = BaseImageID.Base_17_003,
			RequiredFightSpiritNum = 100,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_17_009,
			Name = "カロリーから逃げるな",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_18_001,
			CharaType = CharaType.Chara18,
			BaseImageID = BaseImageID.Base_18_001,
			RequiredFightSpiritNum = 10,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "イッパツガール",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_18_002,
			CharaType = CharaType.Chara18,
			BaseImageID = BaseImageID.Base_18_002,
			RequiredFightSpiritNum = 20,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_18_001,
			Name = "タテナオリ妹",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_18_003,
			CharaType = CharaType.Chara18,
			BaseImageID = BaseImageID.Base_18_003,
			RequiredFightSpiritNum = 30,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_18_002,
			Name = "清書ペン入れベタ塗り",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_18_004,
			CharaType = CharaType.Chara18,
			BaseImageID = BaseImageID.Base_18_002,
			RequiredFightSpiritNum = 40,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_18_003,
			Name = "天才漫画家",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_18_005,
			CharaType = CharaType.Chara18,
			BaseImageID = BaseImageID.Base_18_004,
			RequiredFightSpiritNum = 50,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_18_004,
			Name = "人生万事一発勝負",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_18_006,
			CharaType = CharaType.Chara18,
			BaseImageID = BaseImageID.Base_18_002,
			RequiredFightSpiritNum = 60,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_18_005,
			Name = "重版出来",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_18_007,
			CharaType = CharaType.Chara18,
			BaseImageID = BaseImageID.Base_18_005,
			RequiredFightSpiritNum = 70,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_18_006,
			Name = "何がどうしてこうなった",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_18_008,
			CharaType = CharaType.Chara18,
			BaseImageID = BaseImageID.Base_18_006,
			RequiredFightSpiritNum = 80,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_18_007,
			Name = "ハァハァハァハァ",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_18_009,
			CharaType = CharaType.Chara18,
			BaseImageID = BaseImageID.Base_18_007,
			RequiredFightSpiritNum = 90,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_18_008,
			Name = "りッ…りりぃッッッ",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_18_010,
			CharaType = CharaType.Chara18,
			BaseImageID = BaseImageID.Base_18_003,
			RequiredFightSpiritNum = 100,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_18_009,
			Name = "りぃそく！好評連載中",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_19_001,
			CharaType = CharaType.Chara19,
			BaseImageID = BaseImageID.Base_19_001,
			RequiredFightSpiritNum = 10,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "ボンバーガール",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_19_002,
			CharaType = CharaType.Chara19,
			BaseImageID = BaseImageID.Base_19_002,
			RequiredFightSpiritNum = 20,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_19_001,
			Name = "がんばるお母さん",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_19_003,
			CharaType = CharaType.Chara19,
			BaseImageID = BaseImageID.Base_19_003,
			RequiredFightSpiritNum = 30,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_19_002,
			Name = "あの日のフォトグラフ",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_19_004,
			CharaType = CharaType.Chara19,
			BaseImageID = BaseImageID.Base_19_004,
			RequiredFightSpiritNum = 40,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_19_003,
			Name = "元ボンバークイーン",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_19_005,
			CharaType = CharaType.Chara19,
			BaseImageID = BaseImageID.Base_19_005,
			RequiredFightSpiritNum = 50,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_19_004,
			Name = "ごく普通なボンバー主婦",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_19_006,
			CharaType = CharaType.Chara19,
			BaseImageID = BaseImageID.Base_19_006,
			RequiredFightSpiritNum = 60,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_19_005,
			Name = "二児の母",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_19_007,
			CharaType = CharaType.Chara19,
			BaseImageID = BaseImageID.Base_19_002,
			RequiredFightSpiritNum = 70,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_19_006,
			Name = "色褪せないメモリー",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_19_008,
			CharaType = CharaType.Chara19,
			BaseImageID = BaseImageID.Base_19_001,
			RequiredFightSpiritNum = 80,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_19_007,
			Name = "メガトン大爆発",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_19_009,
			CharaType = CharaType.Chara19,
			BaseImageID = BaseImageID.Base_19_007,
			RequiredFightSpiritNum = 90,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_19_008,
			Name = "ボーナスちゃぶ台♪",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_19_010,
			CharaType = CharaType.Chara19,
			BaseImageID = BaseImageID.Base_19_005,
			RequiredFightSpiritNum = 100,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_19_009,
			Name = "ボンバーフィーバー塩レバー",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_20_001,
			CharaType = CharaType.Chara20,
			BaseImageID = BaseImageID.Base_20_001,
			RequiredFightSpiritNum = 10,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "純チャンガール",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_20_002,
			CharaType = CharaType.Chara20,
			BaseImageID = BaseImageID.Base_20_002,
			RequiredFightSpiritNum = 20,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_20_001,
			Name = "カワサキの鬼",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_20_003,
			CharaType = CharaType.Chara20,
			BaseImageID = BaseImageID.Base_20_003,
			RequiredFightSpiritNum = 30,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_20_002,
			Name = "人生ニケツで行こう",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_20_004,
			CharaType = CharaType.Chara20,
			BaseImageID = BaseImageID.Base_20_004,
			RequiredFightSpiritNum = 40,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_20_003,
			Name = "地元最高",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_20_005,
			CharaType = CharaType.Chara20,
			BaseImageID = BaseImageID.Base_20_005,
			RequiredFightSpiritNum = 50,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_20_004,
			Name = "花嫁目指して全国制覇",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_20_006,
			CharaType = CharaType.Chara20,
			BaseImageID = BaseImageID.Base_20_003,
			RequiredFightSpiritNum = 60,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_20_005,
			Name = "純情ミニマム番長",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_20_007,
			CharaType = CharaType.Chara20,
			BaseImageID = BaseImageID.Base_20_001,
			RequiredFightSpiritNum = 70,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_20_006,
			Name = "鬼・愛羅武勇",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_20_008,
			CharaType = CharaType.Chara20,
			BaseImageID = BaseImageID.Base_20_006,
			RequiredFightSpiritNum = 80,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_20_007,
			Name = "デコチャリライダー",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_20_009,
			CharaType = CharaType.Chara20,
			BaseImageID = BaseImageID.Base_20_007,
			RequiredFightSpiritNum = 90,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_20_008,
			Name = "デコチャリで来た。",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_20_010,
			CharaType = CharaType.Chara20,
			BaseImageID = BaseImageID.Base_20_004,
			RequiredFightSpiritNum = 100,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_20_009,
			Name = "最強最高の鬼嫁",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_21_001,
			CharaType = CharaType.Chara21,
			BaseImageID = BaseImageID.Base_21_001,
			RequiredFightSpiritNum = 10,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.None,
			Name = "ちゅーれんガール",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_21_002,
			CharaType = CharaType.Chara21,
			BaseImageID = BaseImageID.Base_21_002,
			RequiredFightSpiritNum = 20,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_21_001,
			Name = "あいうぉんちゅ♪",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_21_003,
			CharaType = CharaType.Chara21,
			BaseImageID = BaseImageID.Base_21_003,
			RequiredFightSpiritNum = 30,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_21_002,
			Name = "ヒミツのヴァンパイア",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_21_004,
			CharaType = CharaType.Chara21,
			BaseImageID = BaseImageID.Base_21_004,
			RequiredFightSpiritNum = 40,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_21_003,
			Name = "無軌道奔放部長",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_21_005,
			CharaType = CharaType.Chara21,
			BaseImageID = BaseImageID.Base_21_005,
			RequiredFightSpiritNum = 50,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_21_004,
			Name = "無敵な部長はちゅーけつき",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_21_006,
			CharaType = CharaType.Chara21,
			BaseImageID = BaseImageID.Base_21_006,
			RequiredFightSpiritNum = 60,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_21_005,
			Name = "サイカワメイク術",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_21_007,
			CharaType = CharaType.Chara21,
			BaseImageID = BaseImageID.Base_21_007,
			RequiredFightSpiritNum = 70,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_21_006,
			Name = "忠愛御中",
			Sort = SortRules.Chara
		},
		new Item
		{
			ItemID = ItemID.Catchphrase_21_008,
			CharaType = CharaType.Chara21,
			BaseImageID = BaseImageID.Base_21_008,
			RequiredFightSpiritNum = 80,
			RequiredGameEventType = GameEventType.None,
			RequiredItemID = ItemID.Catchphrase_21_007,
			Name = "最強メンタル最弱ボディ",
			Sort = SortRules.Chara
		}
	};

	public static Item GetItem(ItemID itemID)
	{
		return _items.Find((Item item) => item.ItemID == itemID);
	}

	public static List<Item> GetItems(Predicate<Item> match)
	{
		return _items.FindAll(match);
	}

	public static List<Item> GetItemAll()
	{
		return _items;
	}

	public static ItemID GetCPUDefaultEquipItemID(CharaType chara, bool isPro = false)
	{
		if (isPro)
		{
			return chara switch
			{
				CharaType.Chara12 => ItemID.Catchphrase_12_001, 
				CharaType.Chara13 => ItemID.Catchphrase_13_001, 
				_ => ItemID.Catchphrase_00_000, 
			};
		}
		return ItemID.Catchphrase_00_000;
	}

	public static ItemID GetDefaultEquipItemID()
	{
		return ItemID.Catchphrase_00_000;
	}

	public static List<ItemID> GetDefaultItemIDs()
	{
		return _defaultPossessionItems;
	}

	public static bool IsDefaultItemID(ItemID itemID)
	{
		return _defaultPossessionItems.Exists((ItemID x) => x == itemID);
	}

	public static string ToObjectID(ItemID itemID)
	{
		return $"OID_{itemID}";
	}

	public static ItemID ToItemID(string oid)
	{
		try
		{
			return Enum.Parse<ItemID>(oid.Remove(0, 4));
		}
		catch
		{
			return ItemID.None;
		}
	}
}
