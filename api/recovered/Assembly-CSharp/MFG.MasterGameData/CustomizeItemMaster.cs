using System;
using System.Collections.Generic;
using System.Linq;
using MFG.Types;
using MFG.Types.Customize.CustomizeItem;

namespace MFG.MasterGameData;

public class CustomizeItemMaster : MasterGameDataBase
{
	private static Dictionary<CharaType, ItemID> ChocolateEffectReachStick = new Dictionary<CharaType, ItemID>
	{
		{
			(CharaType)(-2),
			ItemID.ReachStick154
		},
		{
			(CharaType)(-1),
			ItemID.ReachStick155
		},
		{
			CharaType.Chara01,
			ItemID.ReachStick156
		},
		{
			CharaType.Chara02,
			ItemID.ReachStick157
		},
		{
			CharaType.Chara03,
			ItemID.ReachStick158
		},
		{
			CharaType.Chara04,
			ItemID.ReachStick159
		},
		{
			CharaType.Chara05,
			ItemID.ReachStick160
		},
		{
			CharaType.Chara06,
			ItemID.ReachStick161
		},
		{
			CharaType.Chara07,
			ItemID.ReachStick162
		},
		{
			CharaType.Chara08,
			ItemID.ReachStick163
		},
		{
			CharaType.Chara09,
			ItemID.ReachStick164
		},
		{
			CharaType.Chara10,
			ItemID.ReachStick165
		},
		{
			CharaType.Chara11,
			ItemID.ReachStick166
		},
		{
			CharaType.Chara12,
			ItemID.ReachStick167
		},
		{
			CharaType.Chara13,
			ItemID.ReachStick168
		},
		{
			CharaType.Chara14,
			ItemID.ReachStick169
		},
		{
			CharaType.Chara15,
			ItemID.ReachStick170
		},
		{
			CharaType.Chara16,
			ItemID.ReachStick214
		},
		{
			CharaType.Chara17,
			ItemID.ReachStick215
		},
		{
			CharaType.Chara18,
			ItemID.ReachStick216
		},
		{
			CharaType.Chara19,
			ItemID.ReachStick217
		}
	};

	private static List<ItemID> TohoMusicList = new List<ItemID>
	{
		ItemID.ReachBgm078,
		ItemID.ReachBgm079,
		ItemID.ReachBgm080,
		ItemID.ReachBgm081,
		ItemID.ReachBgm082,
		ItemID.ReachBgm083,
		ItemID.ReachBgm084,
		ItemID.ReachBgm085,
		ItemID.ReachBgm086,
		ItemID.ReachBgm087,
		ItemID.ReachBgm088,
		ItemID.ReachBgm089,
		ItemID.ReachBgm090,
		ItemID.ReachBgm091,
		ItemID.ReachBgm092,
		ItemID.ReachBgm093,
		ItemID.ReachBgm094,
		ItemID.ReachBgm095,
		ItemID.ReachBgm096,
		ItemID.ReachBgm097,
		ItemID.ReachBgm098,
		ItemID.ReachBgm099,
		ItemID.ReachBgm100,
		ItemID.ReachBgm101,
		ItemID.ReachBgm102,
		ItemID.ReachBgm103,
		ItemID.MyRoomBgm033,
		ItemID.MyRoomBgm034,
		ItemID.MyRoomBgm035,
		ItemID.MyRoomBgm036,
		ItemID.MyRoomBgm037,
		ItemID.MyRoomBgm038,
		ItemID.MyRoomBgm039,
		ItemID.MyRoomBgm040,
		ItemID.MyRoomBgm041,
		ItemID.MyRoomBgm042,
		ItemID.MyRoomBgm043,
		ItemID.MyRoomBgm044,
		ItemID.MyRoomBgm045,
		ItemID.MyRoomBgm046,
		ItemID.MyRoomBgm047
	};

	private static List<ItemID> PonnomichiList = new List<ItemID>
	{
		ItemID.TableSticker269,
		ItemID.TableSticker270,
		ItemID.TableSticker271,
		ItemID.TableSticker272,
		ItemID.TableSticker273,
		ItemID.PlayerAccessory251
	};

	private static Dictionary<ItemID, GameEventType> s_stageBgEventDic = new Dictionary<ItemID, GameEventType>
	{
		{
			ItemID.StageBg002,
			GameEventType.Blooming
		},
		{
			ItemID.StageBg003,
			GameEventType.Poolside
		},
		{
			ItemID.StageBg004,
			GameEventType.AutumnLeaves
		}
	};

	private static Dictionary<Category, ItemID> m_defaultEquipItems = new Dictionary<Category, ItemID>
	{
		{
			Category.ReachStick,
			ItemID.ReachStick001
		},
		{
			Category.PlayerAccessory,
			ItemID.PlayerAccessoryNone
		},
		{
			Category.TableSticker,
			ItemID.TableStickerNone
		},
		{
			Category.TileSurface,
			ItemID.TileSurface001
		},
		{
			Category.ReachBgm,
			ItemID.ReachBgm001
		},
		{
			Category.MyRoomBgm,
			ItemID.MyRoomBgm001
		},
		{
			Category.StageBg,
			ItemID.StageBgNone
		}
	};

	private static List<ItemID> m_defaultPossessionItems = new List<ItemID>
	{
		ItemID.ReachStick001,
		ItemID.PlayerAccessoryNone,
		ItemID.TableStickerNone,
		ItemID.TileSurface001,
		ItemID.TileSurface002,
		ItemID.ReachBgm001,
		ItemID.MyRoomBgm001,
		ItemID.MyRoomBgm002,
		ItemID.MyRoomBgm003,
		ItemID.MyRoomBgm004,
		ItemID.MyRoomBgm005,
		ItemID.StageBgNone,
		ItemID.StageBg001
	};

	private static List<Item> m_items = new List<Item>
	{
		new Item
		{
			ObjectID = "OID_ReachStick001",
			ItemID = ItemID.ReachStick001,
			charaType = CharaType.Chara01,
			Name = "ノーマル",
			Category = Category.ReachStick,
			Sort = SortType.Sort01
		},
		new Item
		{
			ObjectID = "OID_ReachStick002",
			ItemID = ItemID.ReachStick002,
			charaType = CharaType.Chara01,
			Name = "ペンライト（青）",
			Category = Category.ReachStick,
			Sort = SortType.Sort01
		},
		new Item
		{
			ObjectID = "OID_ReachStick003",
			ItemID = ItemID.ReachStick003,
			charaType = CharaType.Chara01,
			Name = "ペンライト（白）",
			Category = Category.ReachStick,
			Sort = SortType.Sort01
		},
		new Item
		{
			ObjectID = "OID_ReachStick004",
			ItemID = ItemID.ReachStick004,
			charaType = CharaType.Chara01,
			Name = "ペンライト（赤）",
			Category = Category.ReachStick,
			Sort = SortType.Sort01
		},
		new Item
		{
			ObjectID = "OID_ReachStick005",
			ItemID = ItemID.ReachStick005,
			charaType = CharaType.Chara01,
			Name = "ペンライト（緑）",
			Category = Category.ReachStick,
			Sort = SortType.Sort01
		},
		new Item
		{
			ObjectID = "OID_ReachStick006",
			ItemID = ItemID.ReachStick006,
			charaType = CharaType.Chara01,
			Name = "食べかけ団子",
			Category = Category.ReachStick,
			Sort = SortType.Sort01
		},
		new Item
		{
			ObjectID = "OID_ReachStick007",
			ItemID = ItemID.ReachStick007,
			charaType = CharaType.Chara01,
			Name = "あたり棒",
			Category = Category.ReachStick,
			Sort = SortType.Sort01
		},
		new Item
		{
			ObjectID = "OID_ReachStick008",
			ItemID = ItemID.ReachStick008,
			charaType = CharaType.Chara01,
			Name = "ヒヨリのマイク",
			Category = Category.ReachStick,
			Sort = SortType.Sort01
		},
		new Item
		{
			ObjectID = "OID_ReachStick009",
			ItemID = ItemID.ReachStick009,
			charaType = CharaType.Chara01,
			Name = "ピースアロー",
			Category = Category.ReachStick,
			Sort = SortType.Sort01
		},
		new Item
		{
			ObjectID = "OID_ReachStick010",
			ItemID = ItemID.ReachStick010,
			charaType = CharaType.Chara02,
			Name = "しおり（青）",
			Category = Category.ReachStick,
			Sort = SortType.Sort02
		},
		new Item
		{
			ObjectID = "OID_ReachStick011",
			ItemID = ItemID.ReachStick011,
			charaType = CharaType.Chara02,
			Name = "しおり（白）",
			Category = Category.ReachStick,
			Sort = SortType.Sort02
		},
		new Item
		{
			ObjectID = "OID_ReachStick012",
			ItemID = ItemID.ReachStick012,
			charaType = CharaType.Chara02,
			Name = "しおり（赤）",
			Category = Category.ReachStick,
			Sort = SortType.Sort02
		},
		new Item
		{
			ObjectID = "OID_ReachStick013",
			ItemID = ItemID.ReachStick013,
			charaType = CharaType.Chara02,
			Name = "しおり（緑）",
			Category = Category.ReachStick,
			Sort = SortType.Sort02
		},
		new Item
		{
			ObjectID = "OID_ReachStick014",
			ItemID = ItemID.ReachStick014,
			charaType = CharaType.Chara02,
			Name = "わんわんボーン",
			Category = Category.ReachStick,
			Sort = SortType.Sort02
		},
		new Item
		{
			ObjectID = "OID_ReachStick015",
			ItemID = ItemID.ReachStick015,
			charaType = CharaType.Chara02,
			Name = "センのブラシ",
			Category = Category.ReachStick,
			Sort = SortType.Sort02
		},
		new Item
		{
			ObjectID = "OID_ReachStick016",
			ItemID = ItemID.ReachStick016,
			charaType = CharaType.Chara02,
			Name = "チューリップ",
			Category = Category.ReachStick,
			Sort = SortType.Sort02
		},
		new Item
		{
			ObjectID = "OID_ReachStick017",
			ItemID = ItemID.ReachStick017,
			charaType = CharaType.Chara02,
			Name = "家の鍵",
			Category = Category.ReachStick,
			Sort = SortType.Sort02
		},
		new Item
		{
			ObjectID = "OID_ReachStick018",
			ItemID = ItemID.ReachStick018,
			charaType = CharaType.Chara03,
			Name = "フライ返し（青）",
			Category = Category.ReachStick,
			Sort = SortType.Sort03
		},
		new Item
		{
			ObjectID = "OID_ReachStick019",
			ItemID = ItemID.ReachStick019,
			charaType = CharaType.Chara03,
			Name = "フライ返し（白）",
			Category = Category.ReachStick,
			Sort = SortType.Sort03
		},
		new Item
		{
			ObjectID = "OID_ReachStick020",
			ItemID = ItemID.ReachStick020,
			charaType = CharaType.Chara03,
			Name = "フライ返し（赤）",
			Category = Category.ReachStick,
			Sort = SortType.Sort03
		},
		new Item
		{
			ObjectID = "OID_ReachStick021",
			ItemID = ItemID.ReachStick021,
			charaType = CharaType.Chara03,
			Name = "フライ返し（緑）",
			Category = Category.ReachStick,
			Sort = SortType.Sort03
		},
		new Item
		{
			ObjectID = "OID_ReachStick022",
			ItemID = ItemID.ReachStick022,
			charaType = CharaType.Chara03,
			Name = "インゴット",
			Category = Category.ReachStick,
			Sort = SortType.Sort03
		},
		new Item
		{
			ObjectID = "OID_ReachStick023",
			ItemID = ItemID.ReachStick023,
			charaType = CharaType.Chara03,
			Name = "ハタキ",
			Category = Category.ReachStick,
			Sort = SortType.Sort03
		},
		new Item
		{
			ObjectID = "OID_ReachStick024",
			ItemID = ItemID.ReachStick024,
			charaType = CharaType.Chara03,
			Name = "お箸",
			Category = Category.ReachStick,
			Sort = SortType.Sort03
		},
		new Item
		{
			ObjectID = "OID_ReachStick025",
			ItemID = ItemID.ReachStick025,
			charaType = CharaType.Chara03,
			Name = "焼き魚",
			Category = Category.ReachStick,
			Sort = SortType.Sort03
		},
		new Item
		{
			ObjectID = "OID_ReachStick026",
			ItemID = ItemID.ReachStick026,
			charaType = CharaType.Chara04,
			Name = "あめちゃん（青）",
			Category = Category.ReachStick,
			Sort = SortType.Sort04
		},
		new Item
		{
			ObjectID = "OID_ReachStick027",
			ItemID = ItemID.ReachStick027,
			charaType = CharaType.Chara04,
			Name = "あめちゃん（白）",
			Category = Category.ReachStick,
			Sort = SortType.Sort04
		},
		new Item
		{
			ObjectID = "OID_ReachStick028",
			ItemID = ItemID.ReachStick028,
			charaType = CharaType.Chara04,
			Name = "あめちゃん（赤）",
			Category = Category.ReachStick,
			Sort = SortType.Sort04
		},
		new Item
		{
			ObjectID = "OID_ReachStick029",
			ItemID = ItemID.ReachStick029,
			charaType = CharaType.Chara04,
			Name = "あめちゃん（黒）",
			Category = Category.ReachStick,
			Sort = SortType.Sort04
		},
		new Item
		{
			ObjectID = "OID_ReachStick030",
			ItemID = ItemID.ReachStick030,
			charaType = CharaType.Chara04,
			Name = "エンジェルサイズ",
			Category = Category.ReachStick,
			Sort = SortType.Sort04
		},
		new Item
		{
			ObjectID = "OID_ReachStick031",
			ItemID = ItemID.ReachStick031,
			charaType = CharaType.Chara04,
			Name = "エンジェルソード",
			Category = Category.ReachStick,
			Sort = SortType.Sort04
		},
		new Item
		{
			ObjectID = "OID_ReachStick032",
			ItemID = ItemID.ReachStick032,
			charaType = CharaType.Chara04,
			Name = "エンジェルニンニク",
			Category = Category.ReachStick,
			Sort = SortType.Sort04
		},
		new Item
		{
			ObjectID = "OID_ReachStick033",
			ItemID = ItemID.ReachStick033,
			charaType = CharaType.Chara04,
			Name = "エンジェルろうそく",
			Category = Category.ReachStick,
			Sort = SortType.Sort04
		},
		new Item
		{
			ObjectID = "OID_ReachStick034",
			ItemID = ItemID.ReachStick034,
			Name = "黄金のリーチ棒",
			Category = Category.ReachStick,
			Sort = SortType.Sort90
		},
		new Item
		{
			ObjectID = "OID_ReachStick035",
			ItemID = ItemID.ReachStick035,
			Name = "口紅",
			Category = Category.ReachStick,
			Sort = SortType.Sort90
		},
		new Item
		{
			ObjectID = "OID_ReachStick036",
			ItemID = ItemID.ReachStick036,
			Name = "絆創膏",
			Category = Category.ReachStick,
			Sort = SortType.Sort90
		},
		new Item
		{
			ObjectID = "OID_ReachStick037",
			ItemID = ItemID.ReachStick037,
			charaType = CharaType.Chara05,
			Name = "チューチューアイス（青）",
			Category = Category.ReachStick,
			Sort = SortType.Sort05
		},
		new Item
		{
			ObjectID = "OID_ReachStick038",
			ItemID = ItemID.ReachStick038,
			charaType = CharaType.Chara05,
			Name = "チューチューアイス（白）",
			Category = Category.ReachStick,
			Sort = SortType.Sort05
		},
		new Item
		{
			ObjectID = "OID_ReachStick039",
			ItemID = ItemID.ReachStick039,
			charaType = CharaType.Chara05,
			Name = "チューチューアイス（赤）",
			Category = Category.ReachStick,
			Sort = SortType.Sort05
		},
		new Item
		{
			ObjectID = "OID_ReachStick040",
			ItemID = ItemID.ReachStick040,
			charaType = CharaType.Chara05,
			Name = "チューチューアイス（緑）",
			Category = Category.ReachStick,
			Sort = SortType.Sort05
		},
		new Item
		{
			ObjectID = "OID_ReachStick041",
			ItemID = ItemID.ReachStick041,
			charaType = CharaType.Chara05,
			Name = "タイフーンポテト",
			Category = Category.ReachStick,
			Sort = SortType.Sort05
		},
		new Item
		{
			ObjectID = "OID_ReachStick042",
			ItemID = ItemID.ReachStick042,
			charaType = CharaType.Chara05,
			Name = "フランクフルト",
			Category = Category.ReachStick,
			Sort = SortType.Sort05
		},
		new Item
		{
			ObjectID = "OID_ReachStick043",
			ItemID = ItemID.ReachStick043,
			charaType = CharaType.Chara05,
			Name = "とろたく巻",
			Category = Category.ReachStick,
			Sort = SortType.Sort05
		},
		new Item
		{
			ObjectID = "OID_ReachStick044",
			ItemID = ItemID.ReachStick044,
			charaType = CharaType.Chara05,
			Name = "フランスパン",
			Category = Category.ReachStick,
			Sort = SortType.Sort05
		},
		new Item
		{
			ObjectID = "OID_ReachStick045",
			ItemID = ItemID.ReachStick045,
			charaType = CharaType.Chara06,
			Name = "暗黒羽ペン（青）",
			Category = Category.ReachStick,
			Sort = SortType.Sort06
		},
		new Item
		{
			ObjectID = "OID_ReachStick046",
			ItemID = ItemID.ReachStick046,
			charaType = CharaType.Chara06,
			Name = "暗黒羽ペン（白）",
			Category = Category.ReachStick,
			Sort = SortType.Sort06
		},
		new Item
		{
			ObjectID = "OID_ReachStick047",
			ItemID = ItemID.ReachStick047,
			charaType = CharaType.Chara06,
			Name = "暗黒羽ペン（赤）",
			Category = Category.ReachStick,
			Sort = SortType.Sort06
		},
		new Item
		{
			ObjectID = "OID_ReachStick048",
			ItemID = ItemID.ReachStick048,
			charaType = CharaType.Chara06,
			Name = "暗黒羽ペン（緑）",
			Category = Category.ReachStick,
			Sort = SortType.Sort06
		},
		new Item
		{
			ObjectID = "OID_ReachStick049",
			ItemID = ItemID.ReachStick049,
			charaType = CharaType.Chara06,
			Name = "暗黒精神注入杖",
			Category = Category.ReachStick,
			Sort = SortType.Sort06
		},
		new Item
		{
			ObjectID = "OID_ReachStick050",
			ItemID = ItemID.ReachStick050,
			charaType = CharaType.Chara06,
			Name = "暗黒千点棒",
			Category = Category.ReachStick,
			Sort = SortType.Sort06
		},
		new Item
		{
			ObjectID = "OID_ReachStick051",
			ItemID = ItemID.ReachStick051,
			charaType = CharaType.Chara06,
			Name = "そこにあったいい感じの枝",
			Category = Category.ReachStick,
			Sort = SortType.Sort06
		},
		new Item
		{
			ObjectID = "OID_ReachStick052",
			ItemID = ItemID.ReachStick052,
			charaType = CharaType.Chara06,
			Name = "暗黒なすび",
			Category = Category.ReachStick,
			Sort = SortType.Sort06
		},
		new Item
		{
			ObjectID = "OID_ReachStick053",
			ItemID = ItemID.ReachStick053,
			charaType = CharaType.Chara07,
			Name = "スペースレバー（青）",
			Category = Category.ReachStick,
			Sort = SortType.Sort07
		},
		new Item
		{
			ObjectID = "OID_ReachStick054",
			ItemID = ItemID.ReachStick054,
			charaType = CharaType.Chara07,
			Name = "スペースレバー（白）",
			Category = Category.ReachStick,
			Sort = SortType.Sort07
		},
		new Item
		{
			ObjectID = "OID_ReachStick055",
			ItemID = ItemID.ReachStick055,
			charaType = CharaType.Chara07,
			Name = "スペースレバー（赤）",
			Category = Category.ReachStick,
			Sort = SortType.Sort07
		},
		new Item
		{
			ObjectID = "OID_ReachStick056",
			ItemID = ItemID.ReachStick056,
			charaType = CharaType.Chara07,
			Name = "スペースレバー（緑）",
			Category = Category.ReachStick,
			Sort = SortType.Sort07
		},
		new Item
		{
			ObjectID = "OID_ReachStick057",
			ItemID = ItemID.ReachStick057,
			charaType = CharaType.Chara07,
			Name = "宇宙ひも",
			Category = Category.ReachStick,
			Sort = SortType.Sort07
		},
		new Item
		{
			ObjectID = "OID_ReachStick058",
			ItemID = ItemID.ReachStick058,
			charaType = CharaType.Chara07,
			Name = "スペーススマホ",
			Category = Category.ReachStick,
			Sort = SortType.Sort07
		},
		new Item
		{
			ObjectID = "OID_ReachStick059",
			ItemID = ItemID.ReachStick059,
			charaType = CharaType.Chara07,
			Name = "スペースハンド",
			Category = Category.ReachStick,
			Sort = SortType.Sort07
		},
		new Item
		{
			ObjectID = "OID_ReachStick060",
			ItemID = ItemID.ReachStick060,
			charaType = CharaType.Chara07,
			Name = "スペースアイマスク",
			Category = Category.ReachStick,
			Sort = SortType.Sort07
		},
		new Item
		{
			ObjectID = "OID_ReachStick061",
			ItemID = ItemID.ReachStick061,
			charaType = CharaType.Chara08,
			Name = "五輪の書（青）",
			Category = Category.ReachStick,
			Sort = SortType.Sort08
		},
		new Item
		{
			ObjectID = "OID_ReachStick062",
			ItemID = ItemID.ReachStick062,
			charaType = CharaType.Chara08,
			Name = "五輪の書（白）",
			Category = Category.ReachStick,
			Sort = SortType.Sort08
		},
		new Item
		{
			ObjectID = "OID_ReachStick063",
			ItemID = ItemID.ReachStick063,
			charaType = CharaType.Chara08,
			Name = "五輪の書（赤）",
			Category = Category.ReachStick,
			Sort = SortType.Sort08
		},
		new Item
		{
			ObjectID = "OID_ReachStick064",
			ItemID = ItemID.ReachStick064,
			charaType = CharaType.Chara08,
			Name = "五輪の書（緑）",
			Category = Category.ReachStick,
			Sort = SortType.Sort08
		},
		new Item
		{
			ObjectID = "OID_ReachStick065",
			ItemID = ItemID.ReachStick065,
			charaType = CharaType.Chara08,
			Name = "鍛錬木刀",
			Category = Category.ReachStick,
			Sort = SortType.Sort08
		},
		new Item
		{
			ObjectID = "OID_ReachStick066",
			ItemID = ItemID.ReachStick066,
			charaType = CharaType.Chara08,
			Name = "鍛錬毛筆",
			Category = Category.ReachStick,
			Sort = SortType.Sort08
		},
		new Item
		{
			ObjectID = "OID_ReachStick067",
			ItemID = ItemID.ReachStick067,
			charaType = CharaType.Chara08,
			Name = "ごほうびおイモ",
			Category = Category.ReachStick,
			Sort = SortType.Sort08
		},
		new Item
		{
			ObjectID = "OID_ReachStick068",
			ItemID = ItemID.ReachStick068,
			charaType = CharaType.Chara08,
			Name = "金重・改",
			Category = Category.ReachStick,
			Sort = SortType.Sort08
		},
		new Item
		{
			ObjectID = "OID_ReachStick069",
			ItemID = ItemID.ReachStick069,
			charaType = CharaType.Chara09,
			Name = "プリティアイス（パイン）",
			Category = Category.ReachStick,
			Sort = SortType.Sort09
		},
		new Item
		{
			ObjectID = "OID_ReachStick070",
			ItemID = ItemID.ReachStick070,
			charaType = CharaType.Chara09,
			Name = "プリティアイス（プルーン）",
			Category = Category.ReachStick,
			Sort = SortType.Sort09
		},
		new Item
		{
			ObjectID = "OID_ReachStick071",
			ItemID = ItemID.ReachStick071,
			charaType = CharaType.Chara09,
			Name = "プリティアイス（メロン）",
			Category = Category.ReachStick,
			Sort = SortType.Sort09
		},
		new Item
		{
			ObjectID = "OID_ReachStick072",
			ItemID = ItemID.ReachStick072,
			charaType = CharaType.Chara09,
			Name = "プリティアイス（モモコ）",
			Category = Category.ReachStick,
			Sort = SortType.Sort09
		},
		new Item
		{
			ObjectID = "OID_ReachStick073",
			ItemID = ItemID.ReachStick073,
			charaType = CharaType.Chara09,
			Name = "マジカルポテトマッシャー",
			Category = Category.ReachStick,
			Sort = SortType.Sort09
		},
		new Item
		{
			ObjectID = "OID_ReachStick074",
			ItemID = ItemID.ReachStick074,
			charaType = CharaType.Chara09,
			Name = "Uマジカルポテトマッシャー",
			Category = Category.ReachStick,
			Sort = SortType.Sort09
		},
		new Item
		{
			ObjectID = "OID_ReachStick075",
			ItemID = ItemID.ReachStick075,
			charaType = CharaType.Chara09,
			Name = "パイナップル",
			Category = Category.ReachStick,
			Sort = SortType.Sort09
		},
		new Item
		{
			ObjectID = "OID_ReachStick076",
			ItemID = ItemID.ReachStick076,
			charaType = CharaType.Chara09,
			Name = "モモコロングクッション",
			Category = Category.ReachStick,
			Sort = SortType.Sort09
		},
		new Item
		{
			ObjectID = "OID_ReachStick077",
			ItemID = ItemID.ReachStick077,
			Name = "ナナツノグループ オリジナルリーチ棒",
			Category = Category.ReachStick,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_ReachStick078",
			ItemID = ItemID.ReachStick078,
			Name = "エメラルドリーチ棒",
			Category = Category.ReachStick,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_ReachStick079",
			ItemID = ItemID.ReachStick079,
			Name = "豪運リーチ棒",
			Category = Category.ReachStick,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_ReachStick080",
			ItemID = ItemID.ReachStick080,
			Name = "ルビーリーチ棒",
			Category = Category.ReachStick,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_ReachStick081",
			ItemID = ItemID.ReachStick081,
			charaType = CharaType.Chara10,
			Name = "ヘアバンド（青）",
			Category = Category.ReachStick,
			Sort = SortType.Sort10
		},
		new Item
		{
			ObjectID = "OID_ReachStick082",
			ItemID = ItemID.ReachStick082,
			charaType = CharaType.Chara10,
			Name = "ヘアバンド（黄）",
			Category = Category.ReachStick,
			Sort = SortType.Sort10
		},
		new Item
		{
			ObjectID = "OID_ReachStick083",
			ItemID = ItemID.ReachStick083,
			charaType = CharaType.Chara10,
			Name = "ヘアバンド（赤）",
			Category = Category.ReachStick,
			Sort = SortType.Sort10
		},
		new Item
		{
			ObjectID = "OID_ReachStick084",
			ItemID = ItemID.ReachStick084,
			charaType = CharaType.Chara10,
			Name = "ヘアバンド（緑）",
			Category = Category.ReachStick,
			Sort = SortType.Sort10
		},
		new Item
		{
			ObjectID = "OID_ReachStick085",
			ItemID = ItemID.ReachStick085,
			charaType = CharaType.Chara10,
			Name = "ピンクのリップ",
			Category = Category.ReachStick,
			Sort = SortType.Sort10
		},
		new Item
		{
			ObjectID = "OID_ReachStick086",
			ItemID = ItemID.ReachStick086,
			charaType = CharaType.Chara10,
			Name = "卒業証書",
			Category = Category.ReachStick,
			Sort = SortType.Sort10
		},
		new Item
		{
			ObjectID = "OID_ReachStick087",
			ItemID = ItemID.ReachStick087,
			charaType = CharaType.Chara10,
			Name = "第45回 卒業式",
			Category = Category.ReachStick,
			Sort = SortType.Sort10
		},
		new Item
		{
			ObjectID = "OID_ReachStick088",
			ItemID = ItemID.ReachStick088,
			charaType = CharaType.Chara10,
			Name = "ときめきリーチ棒",
			Category = Category.ReachStick,
			Sort = SortType.Sort10
		},
		new Item
		{
			ObjectID = "OID_ReachStick089",
			ItemID = ItemID.ReachStick089,
			Name = "スペースソックスリーチ棒",
			Category = Category.ReachStick,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_ReachStick090",
			ItemID = ItemID.ReachStick090,
			Name = "サファイアリーチ棒",
			Category = Category.ReachStick,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_ReachStick091",
			ItemID = ItemID.ReachStick091,
			Name = "ニコニコリーチ棒",
			Category = Category.ReachStick,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_ReachStick092",
			ItemID = ItemID.ReachStick092,
			Name = "無双リーチ棒",
			Category = Category.ReachStick,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_ReachStick093",
			ItemID = ItemID.ReachStick093,
			Name = "トパーズリーチ棒",
			Category = Category.ReachStick,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_ReachStick094",
			ItemID = ItemID.ReachStick094,
			charaType = CharaType.Chara11,
			Name = "ライフル弾（青）",
			Category = Category.ReachStick,
			Sort = SortType.Sort11
		},
		new Item
		{
			ObjectID = "OID_ReachStick095",
			ItemID = ItemID.ReachStick095,
			charaType = CharaType.Chara11,
			Name = "ライフル弾（白）",
			Category = Category.ReachStick,
			Sort = SortType.Sort11
		},
		new Item
		{
			ObjectID = "OID_ReachStick096",
			ItemID = ItemID.ReachStick096,
			charaType = CharaType.Chara11,
			Name = "ライフル弾（赤）",
			Category = Category.ReachStick,
			Sort = SortType.Sort11
		},
		new Item
		{
			ObjectID = "OID_ReachStick097",
			ItemID = ItemID.ReachStick097,
			charaType = CharaType.Chara11,
			Name = "ライフル弾（緑）",
			Category = Category.ReachStick,
			Sort = SortType.Sort11
		},
		new Item
		{
			ObjectID = "OID_ReachStick098",
			ItemID = ItemID.ReachStick098,
			charaType = CharaType.Chara11,
			Name = "ライフル弾倉",
			Category = Category.ReachStick,
			Sort = SortType.Sort11
		},
		new Item
		{
			ObjectID = "OID_ReachStick099",
			ItemID = ItemID.ReachStick099,
			charaType = CharaType.Chara11,
			Name = "ライフルスコープ",
			Category = Category.ReachStick,
			Sort = SortType.Sort11
		},
		new Item
		{
			ObjectID = "OID_ReachStick100",
			ItemID = ItemID.ReachStick100,
			charaType = CharaType.Chara11,
			Name = "焼肉トング",
			Category = Category.ReachStick,
			Sort = SortType.Sort11
		},
		new Item
		{
			ObjectID = "OID_ReachStick101",
			ItemID = ItemID.ReachStick101,
			charaType = CharaType.Chara11,
			Name = "焼肉タレ ゴールデン",
			Category = Category.ReachStick,
			Sort = SortType.Sort11
		},
		new Item
		{
			ObjectID = "OID_ReachStick102",
			ItemID = ItemID.ReachStick102,
			Name = "紅葉の枝",
			Category = Category.ReachStick,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_ReachStick103",
			ItemID = ItemID.ReachStick103,
			Name = "惑星直列",
			Category = Category.ReachStick,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_ReachStick104",
			ItemID = ItemID.ReachStick104,
			Name = "キャンディケイン",
			Category = Category.ReachStick,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_ReachStick105",
			ItemID = ItemID.ReachStick105,
			charaType = CharaType.Chara12,
			Name = "イッシキおバール（青）",
			Category = Category.ReachStick,
			Sort = SortType.Sort12
		},
		new Item
		{
			ObjectID = "OID_ReachStick106",
			ItemID = ItemID.ReachStick106,
			charaType = CharaType.Chara12,
			Name = "イッシキおバール（白）",
			Category = Category.ReachStick,
			Sort = SortType.Sort12
		},
		new Item
		{
			ObjectID = "OID_ReachStick107",
			ItemID = ItemID.ReachStick107,
			charaType = CharaType.Chara12,
			Name = "イッシキおバール（赤）",
			Category = Category.ReachStick,
			Sort = SortType.Sort12
		},
		new Item
		{
			ObjectID = "OID_ReachStick108",
			ItemID = ItemID.ReachStick108,
			charaType = CharaType.Chara12,
			Name = "イッシキおバール（緑）",
			Category = Category.ReachStick,
			Sort = SortType.Sort12
		},
		new Item
		{
			ObjectID = "OID_ReachStick109",
			ItemID = ItemID.ReachStick109,
			charaType = CharaType.Chara12,
			Name = "お天突き",
			Category = Category.ReachStick,
			Sort = SortType.Sort12
		},
		new Item
		{
			ObjectID = "OID_ReachStick110",
			ItemID = ItemID.ReachStick110,
			charaType = CharaType.Chara12,
			Name = "お手袋",
			Category = Category.ReachStick,
			Sort = SortType.Sort12
		},
		new Item
		{
			ObjectID = "OID_ReachStick111",
			ItemID = ItemID.ReachStick111,
			charaType = CharaType.Chara12,
			Name = "おカンノーロ",
			Category = Category.ReachStick,
			Sort = SortType.Sort12
		},
		new Item
		{
			ObjectID = "OID_ReachStick112",
			ItemID = ItemID.ReachStick112,
			charaType = CharaType.Chara12,
			Name = "お社長プレート",
			Category = Category.ReachStick,
			Sort = SortType.Sort12
		},
		new Item
		{
			ObjectID = "OID_ReachStick113",
			ItemID = ItemID.ReachStick113,
			Name = "暗黒リーチ棒",
			Category = Category.ReachStick,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_ReachStick114",
			ItemID = ItemID.ReachStick114,
			Name = "わんぬーリーチ棒",
			Category = Category.ReachStick,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_ReachStick115",
			ItemID = ItemID.ReachStick115,
			Name = "つくし",
			Category = Category.ReachStick,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_ReachStick116",
			ItemID = ItemID.ReachStick116,
			Name = "1st アニバーサリーリーチ棒",
			Category = Category.ReachStick,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_ReachStick117",
			ItemID = ItemID.ReachStick117,
			Name = "焼き鳥(ねぎま)",
			Category = Category.ReachStick,
			Sort = SortType.Sort97
		},
		new Item
		{
			ObjectID = "OID_ReachStick118",
			ItemID = ItemID.ReachStick118,
			Name = "炎のリーチ棒",
			Category = Category.ReachStick,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_ReachStick119",
			ItemID = ItemID.ReachStick119,
			Name = "暗黒バリア",
			Category = Category.ReachStick,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_ReachStick120",
			ItemID = ItemID.ReachStick120,
			Name = "純白リーチ棒",
			Category = Category.ReachStick,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_ReachStick121",
			ItemID = ItemID.ReachStick121,
			Name = "いか焼き",
			Category = Category.ReachStick,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_ReachStick122",
			ItemID = ItemID.ReachStick122,
			Name = "暗黒糸通し",
			Category = Category.ReachStick,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_ReachStick123",
			ItemID = ItemID.ReachStick123,
			charaType = CharaType.Chara13,
			Name = "乾電池（青）",
			Category = Category.ReachStick,
			Sort = SortType.Sort13
		},
		new Item
		{
			ObjectID = "OID_ReachStick124",
			ItemID = ItemID.ReachStick124,
			charaType = CharaType.Chara13,
			Name = "乾電池（白）",
			Category = Category.ReachStick,
			Sort = SortType.Sort13
		},
		new Item
		{
			ObjectID = "OID_ReachStick125",
			ItemID = ItemID.ReachStick125,
			charaType = CharaType.Chara13,
			Name = "乾電池（赤）",
			Category = Category.ReachStick,
			Sort = SortType.Sort13
		},
		new Item
		{
			ObjectID = "OID_ReachStick126",
			ItemID = ItemID.ReachStick126,
			charaType = CharaType.Chara13,
			Name = "乾電池（緑）",
			Category = Category.ReachStick,
			Sort = SortType.Sort13
		},
		new Item
		{
			ObjectID = "OID_ReachStick127",
			ItemID = ItemID.ReachStick127,
			charaType = CharaType.Chara13,
			Name = "ゴーヤ",
			Category = Category.ReachStick,
			Sort = SortType.Sort13
		},
		new Item
		{
			ObjectID = "OID_ReachStick128",
			ItemID = ItemID.ReachStick128,
			charaType = CharaType.Chara13,
			Name = "真空管",
			Category = Category.ReachStick,
			Sort = SortType.Sort13
		},
		new Item
		{
			ObjectID = "OID_ReachStick129",
			ItemID = ItemID.ReachStick129,
			charaType = CharaType.Chara13,
			Name = "おしぼり",
			Category = Category.ReachStick,
			Sort = SortType.Sort13
		},
		new Item
		{
			ObjectID = "OID_ReachStick130",
			ItemID = ItemID.ReachStick130,
			charaType = CharaType.Chara13,
			Name = "メモリ",
			Category = Category.ReachStick,
			Sort = SortType.Sort13
		},
		new Item
		{
			ObjectID = "OID_ReachStick131",
			ItemID = ItemID.ReachStick131,
			Name = "大幣",
			Category = Category.ReachStick,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_ReachStick132",
			ItemID = ItemID.ReachStick132,
			Name = "ウサウサリーチ棒",
			Category = Category.ReachStick,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_ReachStick133",
			ItemID = ItemID.ReachStick133,
			Name = "1.5th アニバーサリーリーチ棒",
			Category = Category.ReachStick,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_ReachStick134",
			ItemID = ItemID.ReachStick134,
			Name = "ハロウィンリーチ棒",
			Category = Category.ReachStick,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_ReachStick135",
			ItemID = ItemID.ReachStick135,
			charaType = CharaType.Chara14,
			Name = "ほうき（青）",
			Category = Category.ReachStick,
			Sort = SortType.Sort14
		},
		new Item
		{
			ObjectID = "OID_ReachStick136",
			ItemID = ItemID.ReachStick136,
			charaType = CharaType.Chara14,
			Name = "ほうき（白）",
			Category = Category.ReachStick,
			Sort = SortType.Sort14
		},
		new Item
		{
			ObjectID = "OID_ReachStick137",
			ItemID = ItemID.ReachStick137,
			charaType = CharaType.Chara14,
			Name = "ほうき（赤）",
			Category = Category.ReachStick,
			Sort = SortType.Sort14
		},
		new Item
		{
			ObjectID = "OID_ReachStick138",
			ItemID = ItemID.ReachStick138,
			charaType = CharaType.Chara14,
			Name = "ほうき（緑）",
			Category = Category.ReachStick,
			Sort = SortType.Sort14
		},
		new Item
		{
			ObjectID = "OID_ReachStick139",
			ItemID = ItemID.ReachStick139,
			charaType = CharaType.Chara14,
			Name = "魔法の杖",
			Category = Category.ReachStick,
			Sort = SortType.Sort14
		},
		new Item
		{
			ObjectID = "OID_ReachStick140",
			ItemID = ItemID.ReachStick140,
			charaType = CharaType.Chara14,
			Name = "アロエの葉",
			Category = Category.ReachStick,
			Sort = SortType.Sort14
		},
		new Item
		{
			ObjectID = "OID_ReachStick141",
			ItemID = ItemID.ReachStick141,
			charaType = CharaType.Chara14,
			Name = "マロンボー",
			Category = Category.ReachStick,
			Sort = SortType.Sort14
		},
		new Item
		{
			ObjectID = "OID_ReachStick142",
			ItemID = ItemID.ReachStick142,
			charaType = CharaType.Chara14,
			Name = "グリモワール",
			Category = Category.ReachStick,
			Sort = SortType.Sort14
		},
		new Item
		{
			ObjectID = "OID_ReachStick143",
			ItemID = ItemID.ReachStick143,
			Name = "参塾リーチ棒",
			Category = Category.ReachStick,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_ReachStick144",
			ItemID = ItemID.ReachStick144,
			charaType = CharaType.Chara15,
			Name = "鉄アレイ（青）",
			Category = Category.ReachStick,
			Sort = SortType.Sort15
		},
		new Item
		{
			ObjectID = "OID_ReachStick145",
			ItemID = ItemID.ReachStick145,
			charaType = CharaType.Chara15,
			Name = "鉄アレイ（白）",
			Category = Category.ReachStick,
			Sort = SortType.Sort15
		},
		new Item
		{
			ObjectID = "OID_ReachStick146",
			ItemID = ItemID.ReachStick146,
			charaType = CharaType.Chara15,
			Name = "鉄アレイ（赤）",
			Category = Category.ReachStick,
			Sort = SortType.Sort15
		},
		new Item
		{
			ObjectID = "OID_ReachStick147",
			ItemID = ItemID.ReachStick147,
			charaType = CharaType.Chara15,
			Name = "鉄アレイ（緑）",
			Category = Category.ReachStick,
			Sort = SortType.Sort15
		},
		new Item
		{
			ObjectID = "OID_ReachStick148",
			ItemID = ItemID.ReachStick148,
			charaType = CharaType.Chara15,
			Name = "さんげんステッキ",
			Category = Category.ReachStick,
			Sort = SortType.Sort15
		},
		new Item
		{
			ObjectID = "OID_ReachStick149",
			ItemID = ItemID.ReachStick149,
			charaType = CharaType.Chara15,
			Name = "恋愛漫画",
			Category = Category.ReachStick,
			Sort = SortType.Sort15
		},
		new Item
		{
			ObjectID = "OID_ReachStick150",
			ItemID = ItemID.ReachStick150,
			charaType = CharaType.Chara15,
			Name = "LOVEハム",
			Category = Category.ReachStick,
			Sort = SortType.Sort15
		},
		new Item
		{
			ObjectID = "OID_ReachStick151",
			ItemID = ItemID.ReachStick151,
			charaType = CharaType.Chara15,
			Name = "マジカルサック",
			Category = Category.ReachStick,
			Sort = SortType.Sort15
		},
		new Item
		{
			ObjectID = "OID_ReachStick152",
			ItemID = ItemID.ReachStick152,
			Name = "焼き鳥(もも)",
			Category = Category.ReachStick,
			Sort = SortType.Sort97
		},
		new Item
		{
			ObjectID = "OID_ReachStick153",
			ItemID = ItemID.ReachStick153,
			Name = "無双羽子板",
			Category = Category.ReachStick,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_ReachStick154",
			ItemID = ItemID.ReachStick154,
			Name = "義理チョコ",
			Category = Category.ReachStick,
			Sort = SortType.Sort98
		},
		new Item
		{
			ObjectID = "OID_ReachStick155",
			ItemID = ItemID.ReachStick155,
			Name = "友チョコ",
			Category = Category.ReachStick,
			Sort = SortType.Sort98
		},
		new Item
		{
			ObjectID = "OID_ReachStick156",
			ItemID = ItemID.ReachStick156,
			charaType = CharaType.Chara01,
			Name = "本命チョコ(ヒヨリ)",
			Category = Category.ReachStick,
			Sort = SortType.Sort98
		},
		new Item
		{
			ObjectID = "OID_ReachStick157",
			ItemID = ItemID.ReachStick157,
			charaType = CharaType.Chara02,
			Name = "本命チョコ(セン)",
			Category = Category.ReachStick,
			Sort = SortType.Sort98
		},
		new Item
		{
			ObjectID = "OID_ReachStick158",
			ItemID = ItemID.ReachStick158,
			charaType = CharaType.Chara03,
			Name = "本命チョコ(ツミレ)",
			Category = Category.ReachStick,
			Sort = SortType.Sort98
		},
		new Item
		{
			ObjectID = "OID_ReachStick159",
			ItemID = ItemID.ReachStick159,
			charaType = CharaType.Chara04,
			Name = "本命チョコ(テンシ)",
			Category = Category.ReachStick,
			Sort = SortType.Sort98
		},
		new Item
		{
			ObjectID = "OID_ReachStick160",
			ItemID = ItemID.ReachStick160,
			charaType = CharaType.Chara05,
			Name = "本命チョコ(ヤオ)",
			Category = Category.ReachStick,
			Sort = SortType.Sort98
		},
		new Item
		{
			ObjectID = "OID_ReachStick161",
			ItemID = ItemID.ReachStick161,
			charaType = CharaType.Chara06,
			Name = "本命チョコ(ミツバ)",
			Category = Category.ReachStick,
			Sort = SortType.Sort98
		},
		new Item
		{
			ObjectID = "OID_ReachStick162",
			ItemID = ItemID.ReachStick162,
			charaType = CharaType.Chara07,
			Name = "本命チョコ(トイトイ)",
			Category = Category.ReachStick,
			Sort = SortType.Sort98
		},
		new Item
		{
			ObjectID = "OID_ReachStick163",
			ItemID = ItemID.ReachStick163,
			charaType = CharaType.Chara08,
			Name = "本命チョコ(ムサシ)",
			Category = Category.ReachStick,
			Sort = SortType.Sort98
		},
		new Item
		{
			ObjectID = "OID_ReachStick164",
			ItemID = ItemID.ReachStick164,
			charaType = CharaType.Chara09,
			Name = "本命チョコ(パイン)",
			Category = Category.ReachStick,
			Sort = SortType.Sort98
		},
		new Item
		{
			ObjectID = "OID_ReachStick165",
			ItemID = ItemID.ReachStick165,
			charaType = CharaType.Chara10,
			Name = "本命チョコ(詩織)",
			Category = Category.ReachStick,
			Sort = SortType.Sort98
		},
		new Item
		{
			ObjectID = "OID_ReachStick166",
			ItemID = ItemID.ReachStick166,
			charaType = CharaType.Chara11,
			Name = "本命チョコ(カオス)",
			Category = Category.ReachStick,
			Sort = SortType.Sort98
		},
		new Item
		{
			ObjectID = "OID_ReachStick167",
			ItemID = ItemID.ReachStick167,
			charaType = CharaType.Chara12,
			Name = "本命チョコ(クリア)",
			Category = Category.ReachStick,
			Sort = SortType.Sort98
		},
		new Item
		{
			ObjectID = "OID_ReachStick168",
			ItemID = ItemID.ReachStick168,
			charaType = CharaType.Chara13,
			Name = "本命チョコ(イヨ)",
			Category = Category.ReachStick,
			Sort = SortType.Sort98
		},
		new Item
		{
			ObjectID = "OID_ReachStick169",
			ItemID = ItemID.ReachStick169,
			charaType = CharaType.Chara14,
			Name = "本命チョコ(グリム・アロエ)",
			Category = Category.ReachStick,
			Sort = SortType.Sort98
		},
		new Item
		{
			ObjectID = "OID_ReachStick170",
			ItemID = ItemID.ReachStick170,
			charaType = CharaType.Chara15,
			Name = "本命チョコ(ココア)",
			Category = Category.ReachStick,
			Sort = SortType.Sort98
		},
		new Item
		{
			ObjectID = "OID_ReachStick171",
			ItemID = ItemID.ReachStick171,
			Name = "らぶらぶリーチ棒",
			Category = Category.ReachStick,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_ReachStick172",
			ItemID = ItemID.ReachStick172,
			charaType = CharaType.Chara16,
			Name = "マジカルバーベル（青）",
			Category = Category.ReachStick,
			Sort = SortType.Sort16
		},
		new Item
		{
			ObjectID = "OID_ReachStick173",
			ItemID = ItemID.ReachStick173,
			charaType = CharaType.Chara16,
			Name = "マジカルバーベル（白）",
			Category = Category.ReachStick,
			Sort = SortType.Sort16
		},
		new Item
		{
			ObjectID = "OID_ReachStick174",
			ItemID = ItemID.ReachStick174,
			charaType = CharaType.Chara16,
			Name = "マジカルバーベル（赤）",
			Category = Category.ReachStick,
			Sort = SortType.Sort16
		},
		new Item
		{
			ObjectID = "OID_ReachStick175",
			ItemID = ItemID.ReachStick175,
			charaType = CharaType.Chara16,
			Name = "マジカルバーベル（緑）",
			Category = Category.ReachStick,
			Sort = SortType.Sort16
		},
		new Item
		{
			ObjectID = "OID_ReachStick176",
			ItemID = ItemID.ReachStick176,
			charaType = CharaType.Chara16,
			Name = "だいさんげんステッキ",
			Category = Category.ReachStick,
			Sort = SortType.Sort16
		},
		new Item
		{
			ObjectID = "OID_ReachStick177",
			ItemID = ItemID.ReachStick177,
			charaType = CharaType.Chara16,
			Name = "布団たたき",
			Category = Category.ReachStick,
			Sort = SortType.Sort16
		},
		new Item
		{
			ObjectID = "OID_ReachStick178",
			ItemID = ItemID.ReachStick178,
			charaType = CharaType.Chara16,
			Name = "熟成LOVEハム",
			Category = Category.ReachStick,
			Sort = SortType.Sort16
		},
		new Item
		{
			ObjectID = "OID_ReachStick179",
			ItemID = ItemID.ReachStick179,
			charaType = CharaType.Chara16,
			Name = "さんげん指示棒",
			Category = Category.ReachStick,
			Sort = SortType.Sort16
		},
		new Item
		{
			ObjectID = "OID_ReachStick180",
			ItemID = ItemID.ReachStick180,
			Name = "2nd アニバーサリーリーチ棒",
			Category = Category.ReachStick,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_ReachStick181",
			ItemID = ItemID.ReachStick181,
			Name = "焼き鳥(つくね)",
			Category = Category.ReachStick,
			Sort = SortType.Sort97
		},
		new Item
		{
			ObjectID = "OID_ReachStick182",
			ItemID = ItemID.ReachStick182,
			Name = "カーネーション",
			Category = Category.ReachStick,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_ReachStick183",
			ItemID = ItemID.ReachStick183,
			Name = "イッシキおウィップ",
			Category = Category.ReachStick,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_ReachStick184",
			ItemID = ItemID.ReachStick184,
			charaType = CharaType.Chara17,
			Name = "マヨネーズ（青）",
			Category = Category.ReachStick,
			Sort = SortType.Sort17
		},
		new Item
		{
			ObjectID = "OID_ReachStick185",
			ItemID = ItemID.ReachStick185,
			charaType = CharaType.Chara17,
			Name = "マヨネーズ（白）",
			Category = Category.ReachStick,
			Sort = SortType.Sort17
		},
		new Item
		{
			ObjectID = "OID_ReachStick186",
			ItemID = ItemID.ReachStick186,
			charaType = CharaType.Chara17,
			Name = "マヨネーズ（赤）",
			Category = Category.ReachStick,
			Sort = SortType.Sort17
		},
		new Item
		{
			ObjectID = "OID_ReachStick187",
			ItemID = ItemID.ReachStick187,
			charaType = CharaType.Chara17,
			Name = "マヨネーズ（緑）",
			Category = Category.ReachStick,
			Sort = SortType.Sort17
		},
		new Item
		{
			ObjectID = "OID_ReachStick188",
			ItemID = ItemID.ReachStick188,
			charaType = CharaType.Chara17,
			Name = "カットピザ",
			Category = Category.ReachStick,
			Sort = SortType.Sort17
		},
		new Item
		{
			ObjectID = "OID_ReachStick189",
			ItemID = ItemID.ReachStick189,
			charaType = CharaType.Chara17,
			Name = "コーラ",
			Category = Category.ReachStick,
			Sort = SortType.Sort17
		},
		new Item
		{
			ObjectID = "OID_ReachStick190",
			ItemID = ItemID.ReachStick190,
			charaType = CharaType.Chara17,
			Name = "まごの手",
			Category = Category.ReachStick,
			Sort = SortType.Sort17
		},
		new Item
		{
			ObjectID = "OID_ReachStick191",
			ItemID = ItemID.ReachStick191,
			charaType = CharaType.Chara17,
			Name = "電気羊",
			Category = Category.ReachStick,
			Sort = SortType.Sort17
		},
		new Item
		{
			ObjectID = "OID_ReachStick192",
			ItemID = ItemID.ReachStick192,
			Name = "ウォール・パイン",
			Category = Category.ReachStick,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_ReachStick193",
			ItemID = ItemID.ReachStick193,
			Name = "焼き鳥(砂肝)",
			Category = Category.ReachStick,
			Sort = SortType.Sort97
		},
		new Item
		{
			ObjectID = "OID_ReachStick194",
			ItemID = ItemID.ReachStick194,
			charaType = CharaType.Chara18,
			Name = "スタイラスペン（青）",
			Category = Category.ReachStick,
			Sort = SortType.Sort18
		},
		new Item
		{
			ObjectID = "OID_ReachStick195",
			ItemID = ItemID.ReachStick195,
			charaType = CharaType.Chara18,
			Name = "スタイラスペン（白）",
			Category = Category.ReachStick,
			Sort = SortType.Sort18
		},
		new Item
		{
			ObjectID = "OID_ReachStick196",
			ItemID = ItemID.ReachStick196,
			charaType = CharaType.Chara18,
			Name = "スタイラスペン（赤）",
			Category = Category.ReachStick,
			Sort = SortType.Sort18
		},
		new Item
		{
			ObjectID = "OID_ReachStick197",
			ItemID = ItemID.ReachStick197,
			charaType = CharaType.Chara18,
			Name = "スタイラスペン（緑）",
			Category = Category.ReachStick,
			Sort = SortType.Sort18
		},
		new Item
		{
			ObjectID = "OID_ReachStick198",
			ItemID = ItemID.ReachStick198,
			charaType = CharaType.Chara18,
			Name = "骨ガム",
			Category = Category.ReachStick,
			Sort = SortType.Sort18
		},
		new Item
		{
			ObjectID = "OID_ReachStick199",
			ItemID = ItemID.ReachStick199,
			charaType = CharaType.Chara18,
			Name = "板ガム（コーヒー味）",
			Category = Category.ReachStick,
			Sort = SortType.Sort18
		},
		new Item
		{
			ObjectID = "OID_ReachStick200",
			ItemID = ItemID.ReachStick200,
			charaType = CharaType.Chara18,
			Name = "ブロッククッキー",
			Category = Category.ReachStick,
			Sort = SortType.Sort18
		},
		new Item
		{
			ObjectID = "OID_ReachStick201",
			ItemID = ItemID.ReachStick201,
			charaType = CharaType.Chara18,
			Name = "マンガペン",
			Category = Category.ReachStick,
			Sort = SortType.Sort18
		},
		new Item
		{
			ObjectID = "OID_ReachStick202",
			ItemID = ItemID.ReachStick202,
			Name = "2.5th アニバーサリーリーチ棒",
			Category = Category.ReachStick,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_ReachStick203",
			ItemID = ItemID.ReachStick203,
			Name = "電源コード",
			Category = Category.ReachStick,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_ReachStick204",
			ItemID = ItemID.ReachStick204,
			Name = "松茸",
			Category = Category.ReachStick,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_ReachStick205",
			ItemID = ItemID.ReachStick205,
			charaType = CharaType.Chara19,
			Name = "さいばし（青）",
			Category = Category.ReachStick,
			Sort = SortType.Sort19
		},
		new Item
		{
			ObjectID = "OID_ReachStick206",
			ItemID = ItemID.ReachStick206,
			charaType = CharaType.Chara19,
			Name = "さいばし（白）",
			Category = Category.ReachStick,
			Sort = SortType.Sort19
		},
		new Item
		{
			ObjectID = "OID_ReachStick207",
			ItemID = ItemID.ReachStick207,
			charaType = CharaType.Chara19,
			Name = "さいばし（赤）",
			Category = Category.ReachStick,
			Sort = SortType.Sort19
		},
		new Item
		{
			ObjectID = "OID_ReachStick208",
			ItemID = ItemID.ReachStick208,
			charaType = CharaType.Chara19,
			Name = "さいばし（緑）",
			Category = Category.ReachStick,
			Sort = SortType.Sort19
		},
		new Item
		{
			ObjectID = "OID_ReachStick209",
			ItemID = ItemID.ReachStick209,
			charaType = CharaType.Chara19,
			Name = "ほこりとり",
			Category = Category.ReachStick,
			Sort = SortType.Sort19
		},
		new Item
		{
			ObjectID = "OID_ReachStick210",
			ItemID = ItemID.ReachStick210,
			charaType = CharaType.Chara19,
			Name = "ラバーカップ",
			Category = Category.ReachStick,
			Sort = SortType.Sort19
		},
		new Item
		{
			ObjectID = "OID_ReachStick211",
			ItemID = ItemID.ReachStick211,
			charaType = CharaType.Chara19,
			Name = "つっぱり棒",
			Category = Category.ReachStick,
			Sort = SortType.Sort19
		},
		new Item
		{
			ObjectID = "OID_ReachStick212",
			ItemID = ItemID.ReachStick212,
			charaType = CharaType.Chara19,
			Name = "ねぎ",
			Category = Category.ReachStick,
			Sort = SortType.Sort19
		},
		new Item
		{
			ObjectID = "OID_ReachStick213",
			ItemID = ItemID.ReachStick213,
			Name = "麻雀格闘倶楽部 UNION",
			Category = Category.ReachStick,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_ReachStick214",
			ItemID = ItemID.ReachStick214,
			charaType = CharaType.Chara16,
			Name = "本命チョコ(ダイア)",
			Category = Category.ReachStick,
			Sort = SortType.Sort98
		},
		new Item
		{
			ObjectID = "OID_ReachStick215",
			ItemID = ItemID.ReachStick215,
			charaType = CharaType.Chara17,
			Name = "本命チョコ(ダブリエル)",
			Category = Category.ReachStick,
			Sort = SortType.Sort98
		},
		new Item
		{
			ObjectID = "OID_ReachStick216",
			ItemID = ItemID.ReachStick216,
			charaType = CharaType.Chara18,
			Name = "本命チョコ(イッパツ)",
			Category = Category.ReachStick,
			Sort = SortType.Sort98
		},
		new Item
		{
			ObjectID = "OID_ReachStick217",
			ItemID = ItemID.ReachStick217,
			charaType = CharaType.Chara19,
			Name = "本命チョコ(シロヱ)",
			Category = Category.ReachStick,
			Sort = SortType.Sort98
		},
		new Item
		{
			ObjectID = "OID_ReachStick218",
			ItemID = ItemID.ReachStick218,
			Name = "白服リーチ棒",
			Category = Category.ReachStick,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_ReachStick219",
			ItemID = ItemID.ReachStick219,
			Name = "マジカルベル",
			Category = Category.ReachStick,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_ReachStick220",
			ItemID = ItemID.ReachStick220,
			charaType = CharaType.Chara20,
			Name = "金棒（青）",
			Category = Category.ReachStick,
			Sort = SortType.Sort20
		},
		new Item
		{
			ObjectID = "OID_ReachStick221",
			ItemID = ItemID.ReachStick221,
			charaType = CharaType.Chara20,
			Name = "金棒（白）",
			Category = Category.ReachStick,
			Sort = SortType.Sort20
		},
		new Item
		{
			ObjectID = "OID_ReachStick222",
			ItemID = ItemID.ReachStick222,
			charaType = CharaType.Chara20,
			Name = "金棒（赤）",
			Category = Category.ReachStick,
			Sort = SortType.Sort20
		},
		new Item
		{
			ObjectID = "OID_ReachStick223",
			ItemID = ItemID.ReachStick223,
			charaType = CharaType.Chara20,
			Name = "金棒（緑）",
			Category = Category.ReachStick,
			Sort = SortType.Sort20
		},
		new Item
		{
			ObjectID = "OID_ReachStick224",
			ItemID = ItemID.ReachStick224,
			charaType = CharaType.Chara20,
			Name = "チェーン",
			Category = Category.ReachStick,
			Sort = SortType.Sort20
		},
		new Item
		{
			ObjectID = "OID_ReachStick225",
			ItemID = ItemID.ReachStick225,
			charaType = CharaType.Chara20,
			Name = "釘バット",
			Category = Category.ReachStick,
			Sort = SortType.Sort20
		},
		new Item
		{
			ObjectID = "OID_ReachStick226",
			ItemID = ItemID.ReachStick226,
			charaType = CharaType.Chara20,
			Name = "夫婦箸",
			Category = Category.ReachStick,
			Sort = SortType.Sort20
		},
		new Item
		{
			ObjectID = "OID_ReachStick227",
			ItemID = ItemID.ReachStick227,
			charaType = CharaType.Chara20,
			Name = "結婚式招待状",
			Category = Category.ReachStick,
			Sort = SortType.Sort20
		},
		new Item
		{
			ObjectID = "OID_ReachStick228",
			ItemID = ItemID.ReachStick228,
			Name = "3rd アニバーサリーリーチ棒",
			Category = Category.ReachStick,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_ReachStick229",
			ItemID = ItemID.ReachStick229,
			Name = "タケノコ",
			Category = Category.ReachStick,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_ReachStick230",
			ItemID = ItemID.ReachStick230,
			Name = "リレーバトン",
			Category = Category.ReachStick,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_ReachStick231",
			ItemID = ItemID.ReachStick231,
			Name = "焼き鳥(なんこつ)",
			Category = Category.ReachStick,
			Sort = SortType.Sort97
		},
		new Item
		{
			ObjectID = "OID_ReachStick232",
			ItemID = ItemID.ReachStick232,
			charaType = CharaType.Chara21,
			Name = "ちゅるるんリップ（青）",
			Category = Category.ReachStick,
			Sort = SortType.Sort21
		},
		new Item
		{
			ObjectID = "OID_ReachStick233",
			ItemID = ItemID.ReachStick233,
			charaType = CharaType.Chara21,
			Name = "ちゅるるんリップ（薄ピンク）",
			Category = Category.ReachStick,
			Sort = SortType.Sort21
		},
		new Item
		{
			ObjectID = "OID_ReachStick234",
			ItemID = ItemID.ReachStick234,
			charaType = CharaType.Chara21,
			Name = "ちゅるるんリップ（真紅）",
			Category = Category.ReachStick,
			Sort = SortType.Sort21
		},
		new Item
		{
			ObjectID = "OID_ReachStick235",
			ItemID = ItemID.ReachStick235,
			charaType = CharaType.Chara21,
			Name = "ちゅるるんリップ（オレンジ）",
			Category = Category.ReachStick,
			Sort = SortType.Sort21
		},
		new Item
		{
			ObjectID = "OID_ReachStick236",
			ItemID = ItemID.ReachStick236,
			charaType = CharaType.Chara21,
			Name = "ちゅーちゅースマホ",
			Category = Category.ReachStick,
			Sort = SortType.Sort21
		},
		new Item
		{
			ObjectID = "OID_ReachStick237",
			ItemID = ItemID.ReachStick237,
			charaType = CharaType.Chara21,
			Name = "ろうそく（燭台付）",
			Category = Category.ReachStick,
			Sort = SortType.Sort21
		},
		new Item
		{
			ObjectID = "OID_ReachStick238",
			ItemID = ItemID.ReachStick238,
			charaType = CharaType.Chara21,
			Name = "ガラスヤスリ",
			Category = Category.ReachStick,
			Sort = SortType.Sort21
		},
		new Item
		{
			ObjectID = "OID_ReachStick239",
			ItemID = ItemID.ReachStick239,
			charaType = CharaType.Chara21,
			Name = "アンプル",
			Category = Category.ReachStick,
			Sort = SortType.Sort21
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessoryNone",
			ItemID = ItemID.PlayerAccessoryNone,
			charaType = CharaType.Chara01,
			Name = "なし",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort01
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory001",
			ItemID = ItemID.PlayerAccessory001,
			charaType = CharaType.Chara01,
			Name = "ピースうさぎマスコット(青）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort01
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory002",
			ItemID = ItemID.PlayerAccessory002,
			charaType = CharaType.Chara01,
			Name = "ピースうさぎマスコット(白）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort01
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory003",
			ItemID = ItemID.PlayerAccessory003,
			charaType = CharaType.Chara01,
			Name = "ピースうさぎマスコット(赤）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort01
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory004",
			ItemID = ItemID.PlayerAccessory004,
			charaType = CharaType.Chara01,
			Name = "ピースうさぎマスコット(緑）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort01
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory005",
			ItemID = ItemID.PlayerAccessory005,
			charaType = CharaType.Chara01,
			Name = "ピースうさぎ風船（青）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort01
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory006",
			ItemID = ItemID.PlayerAccessory006,
			charaType = CharaType.Chara01,
			Name = "ピースうさぎ風船（白）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort01
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory007",
			ItemID = ItemID.PlayerAccessory007,
			charaType = CharaType.Chara01,
			Name = "ピースうさぎ風船（赤）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort01
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory008",
			ItemID = ItemID.PlayerAccessory008,
			charaType = CharaType.Chara01,
			Name = "ピースうさぎ風船（緑）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort01
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory009",
			ItemID = ItemID.PlayerAccessory009,
			charaType = CharaType.Chara02,
			Name = "サバ缶ぬいぐるみ（青）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort02
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory010",
			ItemID = ItemID.PlayerAccessory010,
			charaType = CharaType.Chara02,
			Name = "サバ缶ぬいぐるみ（白）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort02
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory011",
			ItemID = ItemID.PlayerAccessory011,
			charaType = CharaType.Chara02,
			Name = "サバ缶ぬいぐるみ（赤）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort02
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory012",
			ItemID = ItemID.PlayerAccessory012,
			charaType = CharaType.Chara02,
			Name = "サバ缶ぬいぐるみ（緑）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort02
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory013",
			ItemID = ItemID.PlayerAccessory013,
			charaType = CharaType.Chara02,
			Name = "ちょうちょ（青）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort02
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory014",
			ItemID = ItemID.PlayerAccessory014,
			charaType = CharaType.Chara02,
			Name = "ちょうちょ（白）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort02
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory015",
			ItemID = ItemID.PlayerAccessory015,
			charaType = CharaType.Chara02,
			Name = "ちょうちょ（赤）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort02
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory016",
			ItemID = ItemID.PlayerAccessory016,
			charaType = CharaType.Chara02,
			Name = "ちょうちょ（緑）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort02
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory017",
			ItemID = ItemID.PlayerAccessory017,
			charaType = CharaType.Chara03,
			Name = "ぴよぴよマスコット（青）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort03
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory018",
			ItemID = ItemID.PlayerAccessory018,
			charaType = CharaType.Chara03,
			Name = "ぴよぴよマスコット（白）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort03
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory019",
			ItemID = ItemID.PlayerAccessory019,
			charaType = CharaType.Chara03,
			Name = "ぴよぴよマスコット（赤）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort03
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory020",
			ItemID = ItemID.PlayerAccessory020,
			charaType = CharaType.Chara03,
			Name = "ぴよぴよマスコット（緑）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort03
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory021",
			ItemID = ItemID.PlayerAccessory021,
			charaType = CharaType.Chara03,
			Name = "ぴよぴよ時計（青）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort03
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory022",
			ItemID = ItemID.PlayerAccessory022,
			charaType = CharaType.Chara03,
			Name = "ぴよぴよ時計（白）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort03
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory023",
			ItemID = ItemID.PlayerAccessory023,
			charaType = CharaType.Chara03,
			Name = "ぴよぴよ時計（赤）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort03
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory024",
			ItemID = ItemID.PlayerAccessory024,
			charaType = CharaType.Chara03,
			Name = "ぴよぴよ時計（緑）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort03
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory025",
			ItemID = ItemID.PlayerAccessory025,
			charaType = CharaType.Chara04,
			Name = "ザーバンドグン（青）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort04
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory026",
			ItemID = ItemID.PlayerAccessory026,
			charaType = CharaType.Chara04,
			Name = "ザーバンドグン（白）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort04
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory027",
			ItemID = ItemID.PlayerAccessory027,
			charaType = CharaType.Chara04,
			Name = "ザーバンドグン（赤）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort04
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory028",
			ItemID = ItemID.PlayerAccessory028,
			charaType = CharaType.Chara04,
			Name = "ザーバンドグン（緑）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort04
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory029",
			ItemID = ItemID.PlayerAccessory029,
			charaType = CharaType.Chara04,
			Name = "ランタン（青）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort04
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory030",
			ItemID = ItemID.PlayerAccessory030,
			charaType = CharaType.Chara04,
			Name = "ランタン（白）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort04
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory031",
			ItemID = ItemID.PlayerAccessory031,
			charaType = CharaType.Chara04,
			Name = "ランタン（赤）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort04
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory032",
			ItemID = ItemID.PlayerAccessory032,
			charaType = CharaType.Chara04,
			Name = "ランタン（緑）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort04
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory033",
			ItemID = ItemID.PlayerAccessory033,
			Name = "応援旗",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort90
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory034",
			ItemID = ItemID.PlayerAccessory034,
			Name = "風鈴",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort90
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory035",
			ItemID = ItemID.PlayerAccessory035,
			Name = "提灯",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort90
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory036",
			ItemID = ItemID.PlayerAccessory036,
			charaType = CharaType.Chara05,
			Name = "ニャオ（黒）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort05
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory037",
			ItemID = ItemID.PlayerAccessory037,
			charaType = CharaType.Chara05,
			Name = "ニャオ（白）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort05
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory038",
			ItemID = ItemID.PlayerAccessory038,
			charaType = CharaType.Chara05,
			Name = "ニャオ（トラ）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort05
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory039",
			ItemID = ItemID.PlayerAccessory039,
			charaType = CharaType.Chara05,
			Name = "ニャオ（三毛）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort05
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory040",
			ItemID = ItemID.PlayerAccessory040,
			charaType = CharaType.Chara05,
			Name = "おにぎり（青）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort05
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory041",
			ItemID = ItemID.PlayerAccessory041,
			charaType = CharaType.Chara05,
			Name = "おにぎり（白）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort05
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory042",
			ItemID = ItemID.PlayerAccessory042,
			charaType = CharaType.Chara05,
			Name = "おにぎり（赤）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort05
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory043",
			ItemID = ItemID.PlayerAccessory043,
			charaType = CharaType.Chara05,
			Name = "おにぎり（緑）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort05
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory044",
			ItemID = ItemID.PlayerAccessory044,
			charaType = CharaType.Chara06,
			Name = "そこにいたカラス（青）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort06
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory045",
			ItemID = ItemID.PlayerAccessory045,
			charaType = CharaType.Chara06,
			Name = "そこにいたカラス（白）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort06
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory046",
			ItemID = ItemID.PlayerAccessory046,
			charaType = CharaType.Chara06,
			Name = "そこにいたカラス（赤）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort06
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory047",
			ItemID = ItemID.PlayerAccessory047,
			charaType = CharaType.Chara06,
			Name = "そこにいたカラス（緑）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort06
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory048",
			ItemID = ItemID.PlayerAccessory048,
			charaType = CharaType.Chara06,
			Name = "暗黒三つ葉（青）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort06
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory049",
			ItemID = ItemID.PlayerAccessory049,
			charaType = CharaType.Chara06,
			Name = "暗黒三つ葉（白）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort06
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory050",
			ItemID = ItemID.PlayerAccessory050,
			charaType = CharaType.Chara06,
			Name = "暗黒三つ葉（赤）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort06
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory051",
			ItemID = ItemID.PlayerAccessory051,
			charaType = CharaType.Chara06,
			Name = "暗黒三つ葉（緑）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort06
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory052",
			ItemID = ItemID.PlayerAccessory052,
			charaType = CharaType.Chara07,
			Name = "スペース火星人（青）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort07
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory053",
			ItemID = ItemID.PlayerAccessory053,
			charaType = CharaType.Chara07,
			Name = "スペース火星人（白）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort07
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory054",
			ItemID = ItemID.PlayerAccessory054,
			charaType = CharaType.Chara07,
			Name = "スペース火星人（赤）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort07
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory055",
			ItemID = ItemID.PlayerAccessory055,
			charaType = CharaType.Chara07,
			Name = "スペース火星人（緑）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort07
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory056",
			ItemID = ItemID.PlayerAccessory056,
			charaType = CharaType.Chara07,
			Name = "スペースUFO（青）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort07
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory057",
			ItemID = ItemID.PlayerAccessory057,
			charaType = CharaType.Chara07,
			Name = "スペースUFO（白）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort07
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory058",
			ItemID = ItemID.PlayerAccessory058,
			charaType = CharaType.Chara07,
			Name = "スペースUFO（赤）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort07
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory059",
			ItemID = ItemID.PlayerAccessory059,
			charaType = CharaType.Chara07,
			Name = "スペースUFO（緑）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort07
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory060",
			ItemID = ItemID.PlayerAccessory060,
			charaType = CharaType.Chara08,
			Name = "無双ぐま（青）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort08
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory061",
			ItemID = ItemID.PlayerAccessory061,
			charaType = CharaType.Chara08,
			Name = "無双ぐま（白）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort08
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory062",
			ItemID = ItemID.PlayerAccessory062,
			charaType = CharaType.Chara08,
			Name = "無双ぐま（赤）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort08
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory063",
			ItemID = ItemID.PlayerAccessory063,
			charaType = CharaType.Chara08,
			Name = "無双ぐま（緑）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort08
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory064",
			ItemID = ItemID.PlayerAccessory064,
			charaType = CharaType.Chara08,
			Name = "鍛錬巻藁（青）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort08
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory065",
			ItemID = ItemID.PlayerAccessory065,
			charaType = CharaType.Chara08,
			Name = "鍛錬巻藁（白）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort08
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory066",
			ItemID = ItemID.PlayerAccessory066,
			charaType = CharaType.Chara08,
			Name = "鍛錬巻藁（赤）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort08
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory067",
			ItemID = ItemID.PlayerAccessory067,
			charaType = CharaType.Chara08,
			Name = "鍛錬巻藁（緑）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort08
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory068",
			ItemID = ItemID.PlayerAccessory068,
			charaType = CharaType.Chara09,
			Name = "パイねこ（デフォ）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort09
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory069",
			ItemID = ItemID.PlayerAccessory069,
			charaType = CharaType.Chara09,
			Name = "パイねこ（桃）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort09
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory070",
			ItemID = ItemID.PlayerAccessory070,
			charaType = CharaType.Chara09,
			Name = "パイねこ（紫）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort09
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory071",
			ItemID = ItemID.PlayerAccessory071,
			charaType = CharaType.Chara09,
			Name = "パイねこ（緑）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort09
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory072",
			ItemID = ItemID.PlayerAccessory072,
			charaType = CharaType.Chara09,
			Name = "パインロケット（青）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort09
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory073",
			ItemID = ItemID.PlayerAccessory073,
			charaType = CharaType.Chara09,
			Name = "パインロケット（白）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort09
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory074",
			ItemID = ItemID.PlayerAccessory074,
			charaType = CharaType.Chara09,
			Name = "パインロケット（赤）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort09
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory075",
			ItemID = ItemID.PlayerAccessory075,
			charaType = CharaType.Chara09,
			Name = "パインロケット（緑）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort09
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory076",
			ItemID = ItemID.PlayerAccessory076,
			Name = "ナナツノグループ オリジナルマスコット",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory077",
			ItemID = ItemID.PlayerAccessory077,
			Name = "エメラルド杯プラチナトロフィー",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory078",
			ItemID = ItemID.PlayerAccessory078,
			Name = "エメラルド杯ゴールドトロフィー",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory079",
			ItemID = ItemID.PlayerAccessory079,
			Name = "エメラルド杯シルバートロフィー",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory080",
			ItemID = ItemID.PlayerAccessory080,
			Name = "エメラルド杯ブロンズトロフィー",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory081",
			ItemID = ItemID.PlayerAccessory081,
			Name = "まてまて摩天楼",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory082",
			ItemID = ItemID.PlayerAccessory082,
			Name = "ルビー杯プラチナトロフィー",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory083",
			ItemID = ItemID.PlayerAccessory083,
			Name = "ルビー杯ゴールドトロフィー",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory084",
			ItemID = ItemID.PlayerAccessory084,
			Name = "ルビー杯シルバートロフィー",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory085",
			ItemID = ItemID.PlayerAccessory085,
			Name = "ルビー杯ブロンズトロフィー",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory086",
			ItemID = ItemID.PlayerAccessory086,
			charaType = CharaType.Chara10,
			Name = "爆弾（青）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort10
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory087",
			ItemID = ItemID.PlayerAccessory087,
			charaType = CharaType.Chara10,
			Name = "爆弾（グレー）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort10
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory088",
			ItemID = ItemID.PlayerAccessory088,
			charaType = CharaType.Chara10,
			Name = "爆弾（赤）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort10
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory089",
			ItemID = ItemID.PlayerAccessory089,
			charaType = CharaType.Chara10,
			Name = "爆弾（緑）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort10
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory090",
			ItemID = ItemID.PlayerAccessory090,
			charaType = CharaType.Chara10,
			Name = "伝説の樹（夏）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort10
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory091",
			ItemID = ItemID.PlayerAccessory091,
			charaType = CharaType.Chara10,
			Name = "伝説の樹（秋）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort10
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory092",
			ItemID = ItemID.PlayerAccessory092,
			charaType = CharaType.Chara10,
			Name = "伝説の樹（春）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort10
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory093",
			ItemID = ItemID.PlayerAccessory093,
			charaType = CharaType.Chara10,
			Name = "伝説の樹（冬）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort10
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory094",
			ItemID = ItemID.PlayerAccessory094,
			Name = "ギガンティックトイトイ",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory095",
			ItemID = ItemID.PlayerAccessory095,
			Name = "サファイア杯プラチナトロフィー",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory096",
			ItemID = ItemID.PlayerAccessory096,
			Name = "サファイア杯ゴールドトロフィー",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory097",
			ItemID = ItemID.PlayerAccessory097,
			Name = "サファイア杯シルバートロフィー",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory098",
			ItemID = ItemID.PlayerAccessory098,
			Name = "サファイア杯ブロンズトロフィー",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory099",
			ItemID = ItemID.PlayerAccessory099,
			Name = "暗黒テント",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory100",
			ItemID = ItemID.PlayerAccessory100,
			Name = "KAC予選4人打ち部門プラチナトロフィー",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory101",
			ItemID = ItemID.PlayerAccessory101,
			Name = "KAC予選4人打ち部門ゴールドトロフィー",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory102",
			ItemID = ItemID.PlayerAccessory102,
			Name = "KAC予選4人打ち部門シルバートロフィー",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory103",
			ItemID = ItemID.PlayerAccessory103,
			Name = "KAC予選4人打ち部門ブロンズトロフィー",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory104",
			ItemID = ItemID.PlayerAccessory104,
			Name = "KAC予選3人打ち部門プラチナトロフィー",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory105",
			ItemID = ItemID.PlayerAccessory105,
			Name = "KAC予選3人打ち部門ゴールドトロフィー",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory106",
			ItemID = ItemID.PlayerAccessory106,
			Name = "KAC予選3人打ち部門シルバートロフィー",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory107",
			ItemID = ItemID.PlayerAccessory107,
			Name = "KAC予選3人打ち部門ブロンズトロフィー",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory108",
			ItemID = ItemID.PlayerAccessory108,
			Name = "うさぎのぬいぐるみ",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory109",
			ItemID = ItemID.PlayerAccessory109,
			Name = "トパーズ杯プラチナトロフィー",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory110",
			ItemID = ItemID.PlayerAccessory110,
			Name = "トパーズ杯ゴールドトロフィー",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory111",
			ItemID = ItemID.PlayerAccessory111,
			Name = "トパーズ杯シルバートロフィー",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory112",
			ItemID = ItemID.PlayerAccessory112,
			Name = "トパーズ杯ブロンズトロフィー",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory113",
			ItemID = ItemID.PlayerAccessory113,
			charaType = CharaType.Chara11,
			Name = "マンターゲット（青）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort11
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory114",
			ItemID = ItemID.PlayerAccessory114,
			charaType = CharaType.Chara11,
			Name = "マンターゲット（白）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort11
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory115",
			ItemID = ItemID.PlayerAccessory115,
			charaType = CharaType.Chara11,
			Name = "マンターゲット（赤）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort11
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory116",
			ItemID = ItemID.PlayerAccessory116,
			charaType = CharaType.Chara11,
			Name = "マンターゲット（緑）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort11
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory117",
			ItemID = ItemID.PlayerAccessory117,
			charaType = CharaType.Chara11,
			Name = "弾薬箱（青）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort11
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory118",
			ItemID = ItemID.PlayerAccessory118,
			charaType = CharaType.Chara11,
			Name = "弾薬箱（白）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort11
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory119",
			ItemID = ItemID.PlayerAccessory119,
			charaType = CharaType.Chara11,
			Name = "弾薬箱（赤）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort11
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory120",
			ItemID = ItemID.PlayerAccessory120,
			charaType = CharaType.Chara11,
			Name = "弾薬箱（緑）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort11
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory121",
			ItemID = ItemID.PlayerAccessory121,
			Name = "赤とんぼ",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory122",
			ItemID = ItemID.PlayerAccessory122,
			Name = "トイトイ雪だるま",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory123",
			ItemID = ItemID.PlayerAccessory123,
			Name = "クリスマスブーツ(赤)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory124",
			ItemID = ItemID.PlayerAccessory124,
			Name = "クリスマスブーツ(緑)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory125",
			ItemID = ItemID.PlayerAccessory125,
			Name = "銅の盾(壱高)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort90
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory126",
			ItemID = ItemID.PlayerAccessory126,
			Name = "銀の盾(壱高)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort90
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory127",
			ItemID = ItemID.PlayerAccessory127,
			Name = "金の盾(壱高)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort90
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory128",
			ItemID = ItemID.PlayerAccessory128,
			Name = "銅の盾(弐女)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort90
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory129",
			ItemID = ItemID.PlayerAccessory129,
			Name = "銀の盾(弐女)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort90
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory130",
			ItemID = ItemID.PlayerAccessory130,
			Name = "金の盾(弐女)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort90
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory131",
			ItemID = ItemID.PlayerAccessory131,
			Name = "銅の盾(参塾)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort90
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory132",
			ItemID = ItemID.PlayerAccessory132,
			Name = "銀の盾(参塾)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort90
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory133",
			ItemID = ItemID.PlayerAccessory133,
			Name = "金の盾(参塾)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort90
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory134",
			ItemID = ItemID.PlayerAccessory134,
			Name = "銅の盾(四女)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort90
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory135",
			ItemID = ItemID.PlayerAccessory135,
			Name = "銀の盾(四女)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort90
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory136",
			ItemID = ItemID.PlayerAccessory136,
			Name = "金の盾(四女)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort90
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory137",
			ItemID = ItemID.PlayerAccessory137,
			Name = "銅の盾(六学)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort90
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory138",
			ItemID = ItemID.PlayerAccessory138,
			Name = "銀の盾(六学)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort90
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory139",
			ItemID = ItemID.PlayerAccessory139,
			Name = "金の盾(六学)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort90
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory140",
			ItemID = ItemID.PlayerAccessory140,
			Name = "銅の盾(帝役)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort90
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory141",
			ItemID = ItemID.PlayerAccessory141,
			Name = "銀の盾(帝役)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort90
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory142",
			ItemID = ItemID.PlayerAccessory142,
			Name = "金の盾(帝役)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort90
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory143",
			ItemID = ItemID.PlayerAccessory143,
			Name = "銅の盾(ドルプロ)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort90
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory144",
			ItemID = ItemID.PlayerAccessory144,
			Name = "銀の盾(ドルプロ)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort90
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory145",
			ItemID = ItemID.PlayerAccessory145,
			Name = "金の盾(ドルプロ)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort90
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory146",
			ItemID = ItemID.PlayerAccessory146,
			Name = "銅の盾(きらめき高校)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort90
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory147",
			ItemID = ItemID.PlayerAccessory147,
			Name = "銀の盾(きらめき高校)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort90
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory148",
			ItemID = ItemID.PlayerAccessory148,
			Name = "金の盾(きらめき高校)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort90
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory149",
			ItemID = ItemID.PlayerAccessory149,
			charaType = CharaType.Chara12,
			Name = "青服おフィギア",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort12
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory150",
			ItemID = ItemID.PlayerAccessory150,
			charaType = CharaType.Chara12,
			Name = "白服おフィギア",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort12
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory151",
			ItemID = ItemID.PlayerAccessory151,
			charaType = CharaType.Chara12,
			Name = "赤服おフィギア",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort12
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory152",
			ItemID = ItemID.PlayerAccessory152,
			charaType = CharaType.Chara12,
			Name = "緑服おフィギア",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort12
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory153",
			ItemID = ItemID.PlayerAccessory153,
			charaType = CharaType.Chara12,
			Name = "お工事看板（青）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort12
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory154",
			ItemID = ItemID.PlayerAccessory154,
			charaType = CharaType.Chara12,
			Name = "お工事看板（白）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort12
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory155",
			ItemID = ItemID.PlayerAccessory155,
			charaType = CharaType.Chara12,
			Name = "お工事看板（赤）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort12
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory156",
			ItemID = ItemID.PlayerAccessory156,
			charaType = CharaType.Chara12,
			Name = "お工事看板（緑）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort12
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory157",
			ItemID = ItemID.PlayerAccessory157,
			Name = "暗黒自画像",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory158",
			ItemID = ItemID.PlayerAccessory158,
			Name = "パーフェクトおにぎり",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory159",
			ItemID = ItemID.PlayerAccessory159,
			Name = "イースターエッグ",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory160",
			ItemID = ItemID.PlayerAccessory160,
			Name = "1st アニバーサリー花輪",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory161",
			ItemID = ItemID.PlayerAccessory161,
			Name = "1500万点箱",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory162",
			ItemID = ItemID.PlayerAccessory162,
			Name = "1stアニバーサリー杯プラチナトロフィー",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory163",
			ItemID = ItemID.PlayerAccessory163,
			Name = "1stアニバーサリー杯ゴールドトロフィー",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory164",
			ItemID = ItemID.PlayerAccessory164,
			Name = "1stアニバーサリー杯シルバートロフィー",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory165",
			ItemID = ItemID.PlayerAccessory165,
			Name = "1stアニバーサリー杯ブロンズトロフィー",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory166",
			ItemID = ItemID.PlayerAccessory166,
			Name = "鍛錬万点棒",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory167",
			ItemID = ItemID.PlayerAccessory167,
			Name = "雨宿りイーソーちゃん",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory168",
			ItemID = ItemID.PlayerAccessory168,
			Name = "てるてるセンち",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory169",
			ItemID = ItemID.PlayerAccessory169,
			Name = "イッシキおチェアー",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory170",
			ItemID = ItemID.PlayerAccessory170,
			charaType = CharaType.Chara01,
			Name = "推しうちわ(ヒヨリ)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory171",
			ItemID = ItemID.PlayerAccessory171,
			charaType = CharaType.Chara02,
			Name = "推しうちわ(セン)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory172",
			ItemID = ItemID.PlayerAccessory172,
			charaType = CharaType.Chara03,
			Name = "推しうちわ(ツミレ)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory173",
			ItemID = ItemID.PlayerAccessory173,
			charaType = CharaType.Chara04,
			Name = "推しうちわ(テンシ)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory174",
			ItemID = ItemID.PlayerAccessory174,
			charaType = CharaType.Chara05,
			Name = "推しうちわ(ヤオ)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory175",
			ItemID = ItemID.PlayerAccessory175,
			charaType = CharaType.Chara06,
			Name = "推しうちわ(ミツバ)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory176",
			ItemID = ItemID.PlayerAccessory176,
			charaType = CharaType.Chara07,
			Name = "推しうちわ(トイトイ)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory177",
			ItemID = ItemID.PlayerAccessory177,
			charaType = CharaType.Chara08,
			Name = "推しうちわ(ムサシ)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory178",
			ItemID = ItemID.PlayerAccessory178,
			charaType = CharaType.Chara09,
			Name = "推しうちわ(パイン)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory179",
			ItemID = ItemID.PlayerAccessory179,
			charaType = CharaType.Chara10,
			Name = "推しうちわ(詩織)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory180",
			ItemID = ItemID.PlayerAccessory180,
			charaType = CharaType.Chara11,
			Name = "推しうちわ(カオス)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory181",
			ItemID = ItemID.PlayerAccessory181,
			charaType = CharaType.Chara12,
			Name = "推しうちわ(クリア)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory182",
			ItemID = ItemID.PlayerAccessory182,
			Name = "蚊取りイーソー",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory183",
			ItemID = ItemID.PlayerAccessory183,
			Name = "暗黒ぐるみ(トイトイ)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory184",
			ItemID = ItemID.PlayerAccessory184,
			charaType = CharaType.Chara13,
			Name = "ゲーム基盤（青）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort13
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory185",
			ItemID = ItemID.PlayerAccessory185,
			charaType = CharaType.Chara13,
			Name = "ゲーム基盤（白）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort13
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory186",
			ItemID = ItemID.PlayerAccessory186,
			charaType = CharaType.Chara13,
			Name = "ゲーム基盤（赤）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort13
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory187",
			ItemID = ItemID.PlayerAccessory187,
			charaType = CharaType.Chara13,
			Name = "ゲーム基盤（緑）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort13
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory188",
			ItemID = ItemID.PlayerAccessory188,
			charaType = CharaType.Chara13,
			Name = "全自動雀卓人形（青）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort13
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory189",
			ItemID = ItemID.PlayerAccessory189,
			charaType = CharaType.Chara13,
			Name = "全自動雀卓人形（白）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort13
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory190",
			ItemID = ItemID.PlayerAccessory190,
			charaType = CharaType.Chara13,
			Name = "全自動雀卓人形（赤）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort13
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory191",
			ItemID = ItemID.PlayerAccessory191,
			charaType = CharaType.Chara13,
			Name = "全自動雀卓人形（緑）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort13
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory192",
			ItemID = ItemID.PlayerAccessory192,
			Name = "ゆっくりヒヨリ",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory193",
			ItemID = ItemID.PlayerAccessory193,
			Name = "ゆっくりセン",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory194",
			ItemID = ItemID.PlayerAccessory194,
			Name = "犬小屋",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory195",
			ItemID = ItemID.PlayerAccessory195,
			Name = "ジャッくみゃンタン",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory196",
			ItemID = ItemID.PlayerAccessory196,
			charaType = CharaType.Chara14,
			Name = "QMA筐体（青）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort14
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory197",
			ItemID = ItemID.PlayerAccessory197,
			charaType = CharaType.Chara14,
			Name = "QMA筐体（白）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort14
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory198",
			ItemID = ItemID.PlayerAccessory198,
			charaType = CharaType.Chara14,
			Name = "QMA筐体（赤）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort14
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory199",
			ItemID = ItemID.PlayerAccessory199,
			charaType = CharaType.Chara14,
			Name = "QMA筐体（緑）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort14
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory200",
			ItemID = ItemID.PlayerAccessory200,
			charaType = CharaType.Chara14,
			Name = "アロエちゃん人形（青）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort14
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory201",
			ItemID = ItemID.PlayerAccessory201,
			charaType = CharaType.Chara14,
			Name = "アロエちゃん人形（白）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort14
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory202",
			ItemID = ItemID.PlayerAccessory202,
			charaType = CharaType.Chara14,
			Name = "アロエちゃん人形（赤）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort14
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory203",
			ItemID = ItemID.PlayerAccessory203,
			charaType = CharaType.Chara14,
			Name = "アロエちゃん人形（緑）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort14
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory204",
			ItemID = ItemID.PlayerAccessory204,
			Name = "銅の盾(マジックアカデミー)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort90
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory205",
			ItemID = ItemID.PlayerAccessory205,
			Name = "銀の盾(マジックアカデミー)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort90
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory206",
			ItemID = ItemID.PlayerAccessory206,
			Name = "金の盾(マジックアカデミー)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort90
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory207",
			ItemID = ItemID.PlayerAccessory207,
			Name = "七輪",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory208",
			ItemID = ItemID.PlayerAccessory208,
			charaType = CharaType.Chara15,
			Name = "プルコギ貯金箱（青）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort15
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory209",
			ItemID = ItemID.PlayerAccessory209,
			charaType = CharaType.Chara15,
			Name = "プルコギ貯金箱（白）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort15
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory210",
			ItemID = ItemID.PlayerAccessory210,
			charaType = CharaType.Chara15,
			Name = "プルコギ貯金箱（赤）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort15
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory211",
			ItemID = ItemID.PlayerAccessory211,
			charaType = CharaType.Chara15,
			Name = "プルコギ貯金箱（緑）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort15
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory212",
			ItemID = ItemID.PlayerAccessory212,
			charaType = CharaType.Chara15,
			Name = "マジカルサンドバッグ（青）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort15
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory213",
			ItemID = ItemID.PlayerAccessory213,
			charaType = CharaType.Chara15,
			Name = "マジカルサンドバッグ（白）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort15
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory214",
			ItemID = ItemID.PlayerAccessory214,
			charaType = CharaType.Chara15,
			Name = "マジカルサンドバッグ（赤）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort15
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory215",
			ItemID = ItemID.PlayerAccessory215,
			charaType = CharaType.Chara15,
			Name = "マジカルサンドバッグ（緑）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort15
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory216",
			ItemID = ItemID.PlayerAccessory216,
			Name = "スペースゲーミングチェア",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory217",
			ItemID = ItemID.PlayerAccessory217,
			Name = "炎のぶっとびギャラクシー杯プラチナトロフィー",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory218",
			ItemID = ItemID.PlayerAccessory218,
			Name = "炎のぶっとびギャラクシー杯ゴールドトロフィー",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory219",
			ItemID = ItemID.PlayerAccessory219,
			Name = "炎のぶっとびギャラクシー杯シルバートロフィー",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory220",
			ItemID = ItemID.PlayerAccessory220,
			Name = "炎のぶっとびギャラクシー杯ブロンズトロフィー",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory221",
			ItemID = ItemID.PlayerAccessory221,
			Name = "トイトイ凧",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory222",
			ItemID = ItemID.PlayerAccessory222,
			Name = "恋愛教本",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory223",
			ItemID = ItemID.PlayerAccessory223,
			charaType = CharaType.Chara16,
			Name = "ボード（青）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort16
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory224",
			ItemID = ItemID.PlayerAccessory224,
			charaType = CharaType.Chara16,
			Name = "ボード（白）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort16
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory225",
			ItemID = ItemID.PlayerAccessory225,
			charaType = CharaType.Chara16,
			Name = "ボード（赤）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort16
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory226",
			ItemID = ItemID.PlayerAccessory226,
			charaType = CharaType.Chara16,
			Name = "ボード（緑）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort16
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory227",
			ItemID = ItemID.PlayerAccessory227,
			charaType = CharaType.Chara16,
			Name = "マジカル木人（青）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort16
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory228",
			ItemID = ItemID.PlayerAccessory228,
			charaType = CharaType.Chara16,
			Name = "マジカル木人（白）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort16
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory229",
			ItemID = ItemID.PlayerAccessory229,
			charaType = CharaType.Chara16,
			Name = "マジカル木人（赤）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort16
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory230",
			ItemID = ItemID.PlayerAccessory230,
			charaType = CharaType.Chara16,
			Name = "マジカル木人（緑）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort16
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory231",
			ItemID = ItemID.PlayerAccessory231,
			Name = "2nd アニバーサリー花輪",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory232",
			ItemID = ItemID.PlayerAccessory232,
			Name = "G-14 イヨ アクリルスタンド",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory233",
			ItemID = ItemID.PlayerAccessory233,
			Name = "1億5000万点風呂",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory234",
			ItemID = ItemID.PlayerAccessory234,
			Name = "お花見セット",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory235",
			ItemID = ItemID.PlayerAccessory235,
			Name = "暗黒ホームランダービープラチナトロフィー",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory236",
			ItemID = ItemID.PlayerAccessory236,
			Name = "暗黒ホームランダービーゴールドトロフィー",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory237",
			ItemID = ItemID.PlayerAccessory237,
			Name = "暗黒ホームランダービーシルバートロフィー",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory238",
			ItemID = ItemID.PlayerAccessory238,
			Name = "暗黒ホームランダービーブロンズトロフィー",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory239",
			ItemID = ItemID.PlayerAccessory239,
			Name = "母娘の一幕",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory240",
			ItemID = ItemID.PlayerAccessory240,
			Name = "きゃおすぬい",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory241",
			ItemID = ItemID.PlayerAccessory241,
			charaType = CharaType.Chara17,
			Name = "メガフロート（青）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort17
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory242",
			ItemID = ItemID.PlayerAccessory242,
			charaType = CharaType.Chara17,
			Name = "メガフロート（白）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort17
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory243",
			ItemID = ItemID.PlayerAccessory243,
			charaType = CharaType.Chara17,
			Name = "メガフロート（赤）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort17
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory244",
			ItemID = ItemID.PlayerAccessory244,
			charaType = CharaType.Chara17,
			Name = "メガフロート（緑）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort17
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory245",
			ItemID = ItemID.PlayerAccessory245,
			charaType = CharaType.Chara17,
			Name = "ピザ空箱（青）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort17
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory246",
			ItemID = ItemID.PlayerAccessory246,
			charaType = CharaType.Chara17,
			Name = "ピザ空箱（白）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort17
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory247",
			ItemID = ItemID.PlayerAccessory247,
			charaType = CharaType.Chara17,
			Name = "ピザ空箱（赤）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort17
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory248",
			ItemID = ItemID.PlayerAccessory248,
			charaType = CharaType.Chara17,
			Name = "ピザ空箱（緑）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort17
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory249",
			ItemID = ItemID.PlayerAccessory249,
			Name = "起爆ボタン",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory250",
			ItemID = ItemID.PlayerAccessory250,
			Name = "見守りイーソーちゃん",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory251",
			ItemID = ItemID.PlayerAccessory251,
			Name = "ぽんのみちクッション",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory252",
			ItemID = ItemID.PlayerAccessory252,
			Name = "メンタンピンフェスティバルプラチナトロフィー",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory253",
			ItemID = ItemID.PlayerAccessory253,
			Name = "メンタンピンフェスティバルゴールドトロフィー",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory254",
			ItemID = ItemID.PlayerAccessory254,
			Name = "メンタンピンフェスティバルシルバートロフィー",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory255",
			ItemID = ItemID.PlayerAccessory255,
			Name = "メンタンピンフェスティバルブロンズトロフィー",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory256",
			ItemID = ItemID.PlayerAccessory256,
			charaType = CharaType.Chara18,
			Name = "液タブ（青）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort18
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory257",
			ItemID = ItemID.PlayerAccessory257,
			charaType = CharaType.Chara18,
			Name = "液タブ（白）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort18
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory258",
			ItemID = ItemID.PlayerAccessory258,
			charaType = CharaType.Chara18,
			Name = "液タブ（赤）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort18
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory259",
			ItemID = ItemID.PlayerAccessory259,
			charaType = CharaType.Chara18,
			Name = "液タブ（緑）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort18
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory260",
			ItemID = ItemID.PlayerAccessory260,
			charaType = CharaType.Chara18,
			Name = "デッサン人形（青）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort18
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory261",
			ItemID = ItemID.PlayerAccessory261,
			charaType = CharaType.Chara18,
			Name = "デッサン人形（白）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort18
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory262",
			ItemID = ItemID.PlayerAccessory262,
			charaType = CharaType.Chara18,
			Name = "デッサン人形（赤）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort18
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory263",
			ItemID = ItemID.PlayerAccessory263,
			charaType = CharaType.Chara18,
			Name = "デッサン人形（緑）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort18
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory264",
			ItemID = ItemID.PlayerAccessory264,
			Name = "ダブリエル人形",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory265",
			ItemID = ItemID.PlayerAccessory265,
			Name = "イヨぬい",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory266",
			ItemID = ItemID.PlayerAccessory266,
			Name = "りぃそく！タペストリー",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory267",
			ItemID = ItemID.PlayerAccessory267,
			charaType = CharaType.Chara19,
			Name = "スーパーメガトンボム（青）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort19
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory268",
			ItemID = ItemID.PlayerAccessory268,
			charaType = CharaType.Chara19,
			Name = "スーパーメガトンボム（白）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort19
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory269",
			ItemID = ItemID.PlayerAccessory269,
			charaType = CharaType.Chara19,
			Name = "スーパーメガトンボム（赤）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort19
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory270",
			ItemID = ItemID.PlayerAccessory270,
			charaType = CharaType.Chara19,
			Name = "スーパーメガトンボム（緑）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort19
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory271",
			ItemID = ItemID.PlayerAccessory271,
			charaType = CharaType.Chara19,
			Name = "シロ",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort19
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory272",
			ItemID = ItemID.PlayerAccessory272,
			charaType = CharaType.Chara19,
			Name = "シロン",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort19
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory273",
			ItemID = ItemID.PlayerAccessory273,
			charaType = CharaType.Chara19,
			Name = "シロヱ",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort19
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory274",
			ItemID = ItemID.PlayerAccessory274,
			charaType = CharaType.Chara19,
			Name = "ボンバークイーン",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort19
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory275",
			ItemID = ItemID.PlayerAccessory275,
			Name = "銅の盾(アカデミア)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort90
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory276",
			ItemID = ItemID.PlayerAccessory276,
			Name = "銀の盾(アカデミア)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort90
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory277",
			ItemID = ItemID.PlayerAccessory277,
			Name = "金の盾(アカデミア)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort90
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory278",
			ItemID = ItemID.PlayerAccessory278,
			Name = "シロヱ人形",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory279",
			ItemID = ItemID.PlayerAccessory279,
			Name = "グリアロちゃん人形",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory280",
			ItemID = ItemID.PlayerAccessory280,
			Name = "おみくじ筒",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory281",
			ItemID = ItemID.PlayerAccessory281,
			Name = "麻雀格闘倶楽部 UNION",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory282",
			ItemID = ItemID.PlayerAccessory282,
			Name = "最強店舗決定戦ヒヨリ像(プラチナ)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort91
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory283",
			ItemID = ItemID.PlayerAccessory283,
			Name = "最強店舗決定戦ヒヨリ像(ゴールド)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort91
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory284",
			ItemID = ItemID.PlayerAccessory284,
			Name = "最強店舗決定戦ヒヨリ像(シルバー)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort91
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory285",
			ItemID = ItemID.PlayerAccessory285,
			Name = "最強店舗決定戦ヒヨリ像(ブロンズ)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort91
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory286",
			ItemID = ItemID.PlayerAccessory286,
			Name = "最強店舗決定戦セン像(プラチナ)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort91
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory287",
			ItemID = ItemID.PlayerAccessory287,
			Name = "最強店舗決定戦セン像(ゴールド)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort91
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory288",
			ItemID = ItemID.PlayerAccessory288,
			Name = "最強店舗決定戦セン像(シルバー)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort91
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory289",
			ItemID = ItemID.PlayerAccessory289,
			Name = "最強店舗決定戦セン像(ブロンズ)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort91
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory290",
			ItemID = ItemID.PlayerAccessory290,
			Name = "最強店舗決定戦ツミレ像(プラチナ)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort91
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory291",
			ItemID = ItemID.PlayerAccessory291,
			Name = "最強店舗決定戦ツミレ像(ゴールド)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort91
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory292",
			ItemID = ItemID.PlayerAccessory292,
			Name = "最強店舗決定戦ツミレ像(シルバー)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort91
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory293",
			ItemID = ItemID.PlayerAccessory293,
			Name = "最強店舗決定戦ツミレ像(ブロンズ)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort91
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory294",
			ItemID = ItemID.PlayerAccessory294,
			Name = "最強店舗決定戦テンシ像(プラチナ)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort91
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory295",
			ItemID = ItemID.PlayerAccessory295,
			Name = "最強店舗決定戦テンシ像(ゴールド)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort91
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory296",
			ItemID = ItemID.PlayerAccessory296,
			Name = "最強店舗決定戦テンシ像(シルバー)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort91
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory297",
			ItemID = ItemID.PlayerAccessory297,
			Name = "最強店舗決定戦テンシ像(ブロンズ)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort91
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory298",
			ItemID = ItemID.PlayerAccessory298,
			Name = "最強店舗決定戦ヤオ像(プラチナ)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort91
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory299",
			ItemID = ItemID.PlayerAccessory299,
			Name = "最強店舗決定戦ヤオ像(ゴールド)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort91
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory300",
			ItemID = ItemID.PlayerAccessory300,
			Name = "最強店舗決定戦ヤオ像(シルバー)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort91
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory301",
			ItemID = ItemID.PlayerAccessory301,
			Name = "最強店舗決定戦ヤオ像(ブロンズ)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort91
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory302",
			ItemID = ItemID.PlayerAccessory302,
			Name = "最強店舗決定戦ミツバ像(プラチナ)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort91
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory303",
			ItemID = ItemID.PlayerAccessory303,
			Name = "最強店舗決定戦ミツバ像(ゴールド)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort91
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory304",
			ItemID = ItemID.PlayerAccessory304,
			Name = "最強店舗決定戦ミツバ像(シルバー)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort91
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory305",
			ItemID = ItemID.PlayerAccessory305,
			Name = "最強店舗決定戦ミツバ像(ブロンズ)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort91
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory306",
			ItemID = ItemID.PlayerAccessory306,
			Name = "最強店舗決定戦トイトイ像(プラチナ)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort91
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory307",
			ItemID = ItemID.PlayerAccessory307,
			Name = "最強店舗決定戦トイトイ像(ゴールド)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort91
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory308",
			ItemID = ItemID.PlayerAccessory308,
			Name = "最強店舗決定戦トイトイ像(シルバー)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort91
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory309",
			ItemID = ItemID.PlayerAccessory309,
			Name = "最強店舗決定戦トイトイ像(ブロンズ)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort91
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory310",
			ItemID = ItemID.PlayerAccessory310,
			Name = "最強店舗決定戦ムサシ像(プラチナ)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort91
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory311",
			ItemID = ItemID.PlayerAccessory311,
			Name = "最強店舗決定戦ムサシ像(ゴールド)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort91
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory312",
			ItemID = ItemID.PlayerAccessory312,
			Name = "最強店舗決定戦ムサシ像(シルバー)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort91
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory313",
			ItemID = ItemID.PlayerAccessory313,
			Name = "最強店舗決定戦ムサシ像(ブロンズ)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort91
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory314",
			ItemID = ItemID.PlayerAccessory314,
			Name = "最強店舗決定戦パイン像(プラチナ)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort91
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory315",
			ItemID = ItemID.PlayerAccessory315,
			Name = "最強店舗決定戦パイン像(ゴールド)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort91
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory316",
			ItemID = ItemID.PlayerAccessory316,
			Name = "最強店舗決定戦パイン像(シルバー)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort91
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory317",
			ItemID = ItemID.PlayerAccessory317,
			Name = "最強店舗決定戦パイン像(ブロンズ)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort91
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory318",
			ItemID = ItemID.PlayerAccessory318,
			Name = "最強店舗決定戦詩織像(プラチナ)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort91
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory319",
			ItemID = ItemID.PlayerAccessory319,
			Name = "最強店舗決定戦詩織像(ゴールド)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort91
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory320",
			ItemID = ItemID.PlayerAccessory320,
			Name = "最強店舗決定戦詩織像(シルバー)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort91
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory321",
			ItemID = ItemID.PlayerAccessory321,
			Name = "最強店舗決定戦詩織像(ブロンズ)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort91
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory322",
			ItemID = ItemID.PlayerAccessory322,
			Name = "最強店舗決定戦カオス像(プラチナ)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort91
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory323",
			ItemID = ItemID.PlayerAccessory323,
			Name = "最強店舗決定戦カオス像(ゴールド)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort91
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory324",
			ItemID = ItemID.PlayerAccessory324,
			Name = "最強店舗決定戦カオス像(シルバー)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort91
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory325",
			ItemID = ItemID.PlayerAccessory325,
			Name = "最強店舗決定戦カオス像(ブロンズ)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort91
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory326",
			ItemID = ItemID.PlayerAccessory326,
			Name = "最強店舗決定戦クリア像(プラチナ)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort91
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory327",
			ItemID = ItemID.PlayerAccessory327,
			Name = "最強店舗決定戦クリア像(ゴールド)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort91
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory328",
			ItemID = ItemID.PlayerAccessory328,
			Name = "最強店舗決定戦クリア像(シルバー)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort91
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory329",
			ItemID = ItemID.PlayerAccessory329,
			Name = "最強店舗決定戦クリア像(ブロンズ)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort91
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory330",
			ItemID = ItemID.PlayerAccessory330,
			Name = "最強店舗決定戦イヨ像(プラチナ)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort91
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory331",
			ItemID = ItemID.PlayerAccessory331,
			Name = "最強店舗決定戦イヨ像(ゴールド)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort91
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory332",
			ItemID = ItemID.PlayerAccessory332,
			Name = "最強店舗決定戦イヨ像(シルバー)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort91
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory333",
			ItemID = ItemID.PlayerAccessory333,
			Name = "最強店舗決定戦イヨ像(ブロンズ)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort91
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory334",
			ItemID = ItemID.PlayerAccessory334,
			Name = "最強店舗決定戦グリム・アロエ像(プラチナ)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort91
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory335",
			ItemID = ItemID.PlayerAccessory335,
			Name = "最強店舗決定戦グリム・アロエ像(ゴールド)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort91
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory336",
			ItemID = ItemID.PlayerAccessory336,
			Name = "最強店舗決定戦グリム・アロエ像(シルバー)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort91
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory337",
			ItemID = ItemID.PlayerAccessory337,
			Name = "最強店舗決定戦グリム・アロエ像(ブロンズ)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort91
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory338",
			ItemID = ItemID.PlayerAccessory338,
			Name = "最強店舗決定戦ココア像(プラチナ)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort91
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory339",
			ItemID = ItemID.PlayerAccessory339,
			Name = "最強店舗決定戦ココア像(ゴールド)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort91
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory340",
			ItemID = ItemID.PlayerAccessory340,
			Name = "最強店舗決定戦ココア像(シルバー)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort91
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory341",
			ItemID = ItemID.PlayerAccessory341,
			Name = "最強店舗決定戦ココア像(ブロンズ)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort91
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory342",
			ItemID = ItemID.PlayerAccessory342,
			Name = "最強店舗決定戦ダイア像(プラチナ)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort91
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory343",
			ItemID = ItemID.PlayerAccessory343,
			Name = "最強店舗決定戦ダイア像(ゴールド)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort91
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory344",
			ItemID = ItemID.PlayerAccessory344,
			Name = "最強店舗決定戦ダイア像(シルバー)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort91
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory345",
			ItemID = ItemID.PlayerAccessory345,
			Name = "最強店舗決定戦ダイア像(ブロンズ)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort91
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory346",
			ItemID = ItemID.PlayerAccessory346,
			Name = "最強店舗決定戦ダブリエル像(プラチナ)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort91
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory347",
			ItemID = ItemID.PlayerAccessory347,
			Name = "最強店舗決定戦ダブリエル像(ゴールド)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort91
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory348",
			ItemID = ItemID.PlayerAccessory348,
			Name = "最強店舗決定戦ダブリエル像(シルバー)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort91
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory349",
			ItemID = ItemID.PlayerAccessory349,
			Name = "最強店舗決定戦ダブリエル像(ブロンズ)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort91
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory350",
			ItemID = ItemID.PlayerAccessory350,
			Name = "最強店舗決定戦イッパツ像(プラチナ)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort91
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory351",
			ItemID = ItemID.PlayerAccessory351,
			Name = "最強店舗決定戦イッパツ像(ゴールド)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort91
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory352",
			ItemID = ItemID.PlayerAccessory352,
			Name = "最強店舗決定戦イッパツ像(シルバー)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort91
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory353",
			ItemID = ItemID.PlayerAccessory353,
			Name = "最強店舗決定戦イッパツ像(ブロンズ)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort91
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory354",
			ItemID = ItemID.PlayerAccessory354,
			Name = "最強店舗決定戦シロヱ像(プラチナ)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort91
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory355",
			ItemID = ItemID.PlayerAccessory355,
			Name = "最強店舗決定戦シロヱ像(ゴールド)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort91
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory356",
			ItemID = ItemID.PlayerAccessory356,
			Name = "最強店舗決定戦シロヱ像(シルバー)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort91
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory357",
			ItemID = ItemID.PlayerAccessory357,
			Name = "最強店舗決定戦シロヱ像(ブロンズ)",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort91
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory358",
			ItemID = ItemID.PlayerAccessory358,
			Name = "クリアぬい",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory359",
			ItemID = ItemID.PlayerAccessory359,
			Name = "ダイアぬい",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory360",
			ItemID = ItemID.PlayerAccessory360,
			charaType = CharaType.Chara20,
			Name = "バス停標識（系統1）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort20
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory361",
			ItemID = ItemID.PlayerAccessory361,
			charaType = CharaType.Chara20,
			Name = "バス停標識（系統2）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort20
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory362",
			ItemID = ItemID.PlayerAccessory362,
			charaType = CharaType.Chara20,
			Name = "バス停標識（系統3）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort20
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory363",
			ItemID = ItemID.PlayerAccessory363,
			charaType = CharaType.Chara20,
			Name = "バス停標識（系統4）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort20
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory364",
			ItemID = ItemID.PlayerAccessory364,
			charaType = CharaType.Chara20,
			Name = "ブリバリ応援旗（参塾）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort20
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory365",
			ItemID = ItemID.PlayerAccessory365,
			charaType = CharaType.Chara20,
			Name = "ブリバリ応援旗（口上）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort20
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory366",
			ItemID = ItemID.PlayerAccessory366,
			charaType = CharaType.Chara20,
			Name = "ブリバリ応援旗（焼肉）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort20
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory367",
			ItemID = ItemID.PlayerAccessory367,
			charaType = CharaType.Chara20,
			Name = "ブリバリ応援旗（禍我裂忌ＤＩＥ志）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort20
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory368",
			ItemID = ItemID.PlayerAccessory368,
			Name = "クレンホウぶちゃうぬい",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory369",
			ItemID = ItemID.PlayerAccessory369,
			Name = "武者イーソーちゃん",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory370",
			ItemID = ItemID.PlayerAccessory370,
			Name = "オペレーションぶっとびギャラクシー杯プラチナトロフィー",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory371",
			ItemID = ItemID.PlayerAccessory371,
			Name = "オペレーションぶっとびギャラクシー杯ゴールドトロフィー",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory372",
			ItemID = ItemID.PlayerAccessory372,
			Name = "オペレーションぶっとびギャラクシー杯シルバートロフィー",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory373",
			ItemID = ItemID.PlayerAccessory373,
			Name = "オペレーションぶっとびギャラクシー杯ブロンズトロフィー",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory374",
			ItemID = ItemID.PlayerAccessory374,
			Name = "紅組イーソーちゃん",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory375",
			ItemID = ItemID.PlayerAccessory375,
			Name = "白組イーソーちゃん",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory376",
			ItemID = ItemID.PlayerAccessory376,
			charaType = CharaType.Chara21,
			Name = "オトモコウモリ（青）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort21
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory377",
			ItemID = ItemID.PlayerAccessory377,
			charaType = CharaType.Chara21,
			Name = "オトモコウモリ（白）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort21
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory378",
			ItemID = ItemID.PlayerAccessory378,
			charaType = CharaType.Chara21,
			Name = "オトモコウモリ（赤）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort21
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory379",
			ItemID = ItemID.PlayerAccessory379,
			charaType = CharaType.Chara21,
			Name = "オトモコウモリ（緑）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort21
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory380",
			ItemID = ItemID.PlayerAccessory380,
			charaType = CharaType.Chara21,
			Name = "棺桶（青）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort21
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory381",
			ItemID = ItemID.PlayerAccessory381,
			charaType = CharaType.Chara21,
			Name = "棺桶（白）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort21
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory382",
			ItemID = ItemID.PlayerAccessory382,
			charaType = CharaType.Chara21,
			Name = "棺桶（赤）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort21
		},
		new Item
		{
			ObjectID = "OID_PlayerAccessory383",
			ItemID = ItemID.PlayerAccessory383,
			charaType = CharaType.Chara21,
			Name = "棺桶（緑）",
			Category = Category.PlayerAccessory,
			Sort = SortType.Sort21
		},
		new Item
		{
			ObjectID = "OID_TableStickerNone",
			ItemID = ItemID.TableStickerNone,
			charaType = CharaType.Chara01,
			Name = "なし",
			Category = Category.TableSticker,
			Sort = SortType.Sort01
		},
		new Item
		{
			ObjectID = "OID_TableSticker001",
			ItemID = ItemID.TableSticker001,
			charaType = CharaType.Chara01,
			Name = "制服ヒヨリ（青）その1",
			Category = Category.TableSticker,
			Sort = SortType.Sort01
		},
		new Item
		{
			ObjectID = "OID_TableSticker002",
			ItemID = ItemID.TableSticker002,
			charaType = CharaType.Chara01,
			Name = "制服ヒヨリ（白）その1",
			Category = Category.TableSticker,
			Sort = SortType.Sort01
		},
		new Item
		{
			ObjectID = "OID_TableSticker003",
			ItemID = ItemID.TableSticker003,
			charaType = CharaType.Chara01,
			Name = "制服ヒヨリ（赤）その1",
			Category = Category.TableSticker,
			Sort = SortType.Sort01
		},
		new Item
		{
			ObjectID = "OID_TableSticker004",
			ItemID = ItemID.TableSticker004,
			charaType = CharaType.Chara01,
			Name = "制服ヒヨリ（緑）その1",
			Category = Category.TableSticker,
			Sort = SortType.Sort01
		},
		new Item
		{
			ObjectID = "OID_TableSticker005",
			ItemID = ItemID.TableSticker005,
			charaType = CharaType.Chara01,
			Name = "制服ヒヨリ（青）その2",
			Category = Category.TableSticker,
			Sort = SortType.Sort01
		},
		new Item
		{
			ObjectID = "OID_TableSticker006",
			ItemID = ItemID.TableSticker006,
			charaType = CharaType.Chara01,
			Name = "制服ヒヨリ（白）その2",
			Category = Category.TableSticker,
			Sort = SortType.Sort01
		},
		new Item
		{
			ObjectID = "OID_TableSticker007",
			ItemID = ItemID.TableSticker007,
			charaType = CharaType.Chara01,
			Name = "制服ヒヨリ（赤）その2",
			Category = Category.TableSticker,
			Sort = SortType.Sort01
		},
		new Item
		{
			ObjectID = "OID_TableSticker008",
			ItemID = ItemID.TableSticker008,
			charaType = CharaType.Chara01,
			Name = "制服ヒヨリ（緑）その2",
			Category = Category.TableSticker,
			Sort = SortType.Sort01
		},
		new Item
		{
			ObjectID = "OID_TableSticker009",
			ItemID = ItemID.TableSticker009,
			charaType = CharaType.Chara02,
			Name = "制服セン（青）その1",
			Category = Category.TableSticker,
			Sort = SortType.Sort02
		},
		new Item
		{
			ObjectID = "OID_TableSticker010",
			ItemID = ItemID.TableSticker010,
			charaType = CharaType.Chara02,
			Name = "制服セン（白）その1",
			Category = Category.TableSticker,
			Sort = SortType.Sort02
		},
		new Item
		{
			ObjectID = "OID_TableSticker011",
			ItemID = ItemID.TableSticker011,
			charaType = CharaType.Chara02,
			Name = "制服セン（赤）その1",
			Category = Category.TableSticker,
			Sort = SortType.Sort02
		},
		new Item
		{
			ObjectID = "OID_TableSticker012",
			ItemID = ItemID.TableSticker012,
			charaType = CharaType.Chara02,
			Name = "制服セン（緑）その1",
			Category = Category.TableSticker,
			Sort = SortType.Sort02
		},
		new Item
		{
			ObjectID = "OID_TableSticker013",
			ItemID = ItemID.TableSticker013,
			charaType = CharaType.Chara02,
			Name = "制服セン（青）その2",
			Category = Category.TableSticker,
			Sort = SortType.Sort02
		},
		new Item
		{
			ObjectID = "OID_TableSticker014",
			ItemID = ItemID.TableSticker014,
			charaType = CharaType.Chara02,
			Name = "制服セン（白）その2",
			Category = Category.TableSticker,
			Sort = SortType.Sort02
		},
		new Item
		{
			ObjectID = "OID_TableSticker015",
			ItemID = ItemID.TableSticker015,
			charaType = CharaType.Chara02,
			Name = "制服セン（赤）その2",
			Category = Category.TableSticker,
			Sort = SortType.Sort02
		},
		new Item
		{
			ObjectID = "OID_TableSticker016",
			ItemID = ItemID.TableSticker016,
			charaType = CharaType.Chara02,
			Name = "制服セン（緑）その2",
			Category = Category.TableSticker,
			Sort = SortType.Sort02
		},
		new Item
		{
			ObjectID = "OID_TableSticker017",
			ItemID = ItemID.TableSticker017,
			charaType = CharaType.Chara03,
			Name = "制服ツミレ（青）その1",
			Category = Category.TableSticker,
			Sort = SortType.Sort03
		},
		new Item
		{
			ObjectID = "OID_TableSticker018",
			ItemID = ItemID.TableSticker018,
			charaType = CharaType.Chara03,
			Name = "制服ツミレ（白）その1",
			Category = Category.TableSticker,
			Sort = SortType.Sort03
		},
		new Item
		{
			ObjectID = "OID_TableSticker019",
			ItemID = ItemID.TableSticker019,
			charaType = CharaType.Chara03,
			Name = "制服ツミレ（赤）その1",
			Category = Category.TableSticker,
			Sort = SortType.Sort03
		},
		new Item
		{
			ObjectID = "OID_TableSticker020",
			ItemID = ItemID.TableSticker020,
			charaType = CharaType.Chara03,
			Name = "制服ツミレ（緑）その1",
			Category = Category.TableSticker,
			Sort = SortType.Sort03
		},
		new Item
		{
			ObjectID = "OID_TableSticker021",
			ItemID = ItemID.TableSticker021,
			charaType = CharaType.Chara03,
			Name = "制服ツミレ（青）その2",
			Category = Category.TableSticker,
			Sort = SortType.Sort03
		},
		new Item
		{
			ObjectID = "OID_TableSticker022",
			ItemID = ItemID.TableSticker022,
			charaType = CharaType.Chara03,
			Name = "制服ツミレ（白）その2",
			Category = Category.TableSticker,
			Sort = SortType.Sort03
		},
		new Item
		{
			ObjectID = "OID_TableSticker023",
			ItemID = ItemID.TableSticker023,
			charaType = CharaType.Chara03,
			Name = "制服ツミレ（赤）その2",
			Category = Category.TableSticker,
			Sort = SortType.Sort03
		},
		new Item
		{
			ObjectID = "OID_TableSticker024",
			ItemID = ItemID.TableSticker024,
			charaType = CharaType.Chara03,
			Name = "制服ツミレ（緑）その2",
			Category = Category.TableSticker,
			Sort = SortType.Sort03
		},
		new Item
		{
			ObjectID = "OID_TableSticker025",
			ItemID = ItemID.TableSticker025,
			charaType = CharaType.Chara04,
			Name = "制服テンシ（青）その1",
			Category = Category.TableSticker,
			Sort = SortType.Sort04
		},
		new Item
		{
			ObjectID = "OID_TableSticker026",
			ItemID = ItemID.TableSticker026,
			charaType = CharaType.Chara04,
			Name = "制服テンシ（白）その1",
			Category = Category.TableSticker,
			Sort = SortType.Sort04
		},
		new Item
		{
			ObjectID = "OID_TableSticker027",
			ItemID = ItemID.TableSticker027,
			charaType = CharaType.Chara04,
			Name = "制服テンシ（赤）その1",
			Category = Category.TableSticker,
			Sort = SortType.Sort04
		},
		new Item
		{
			ObjectID = "OID_TableSticker028",
			ItemID = ItemID.TableSticker028,
			charaType = CharaType.Chara04,
			Name = "制服テンシ（緑）その1",
			Category = Category.TableSticker,
			Sort = SortType.Sort04
		},
		new Item
		{
			ObjectID = "OID_TableSticker029",
			ItemID = ItemID.TableSticker029,
			charaType = CharaType.Chara04,
			Name = "制服テンシ（青）その2",
			Category = Category.TableSticker,
			Sort = SortType.Sort04
		},
		new Item
		{
			ObjectID = "OID_TableSticker030",
			ItemID = ItemID.TableSticker030,
			charaType = CharaType.Chara04,
			Name = "制服テンシ（白）その2",
			Category = Category.TableSticker,
			Sort = SortType.Sort04
		},
		new Item
		{
			ObjectID = "OID_TableSticker031",
			ItemID = ItemID.TableSticker031,
			charaType = CharaType.Chara04,
			Name = "制服テンシ（赤）その2",
			Category = Category.TableSticker,
			Sort = SortType.Sort04
		},
		new Item
		{
			ObjectID = "OID_TableSticker032",
			ItemID = ItemID.TableSticker032,
			charaType = CharaType.Chara04,
			Name = "制服テンシ（緑）その2",
			Category = Category.TableSticker,
			Sort = SortType.Sort04
		},
		new Item
		{
			ObjectID = "OID_TableSticker033",
			ItemID = ItemID.TableSticker033,
			Name = "もう、鳴かないと決めた日",
			Category = Category.TableSticker,
			Sort = SortType.Sort90
		},
		new Item
		{
			ObjectID = "OID_TableSticker034",
			ItemID = ItemID.TableSticker034,
			Name = "やる気満々",
			Category = Category.TableSticker,
			Sort = SortType.Sort90
		},
		new Item
		{
			ObjectID = "OID_TableSticker035",
			ItemID = ItemID.TableSticker035,
			charaType = CharaType.Chara05,
			Name = "制服ヤオ（青）その1",
			Category = Category.TableSticker,
			Sort = SortType.Sort05
		},
		new Item
		{
			ObjectID = "OID_TableSticker036",
			ItemID = ItemID.TableSticker036,
			charaType = CharaType.Chara05,
			Name = "制服ヤオ（白）その1",
			Category = Category.TableSticker,
			Sort = SortType.Sort05
		},
		new Item
		{
			ObjectID = "OID_TableSticker037",
			ItemID = ItemID.TableSticker037,
			charaType = CharaType.Chara05,
			Name = "制服ヤオ（赤）その1",
			Category = Category.TableSticker,
			Sort = SortType.Sort05
		},
		new Item
		{
			ObjectID = "OID_TableSticker038",
			ItemID = ItemID.TableSticker038,
			charaType = CharaType.Chara05,
			Name = "制服ヤオ（緑）その1",
			Category = Category.TableSticker,
			Sort = SortType.Sort05
		},
		new Item
		{
			ObjectID = "OID_TableSticker039",
			ItemID = ItemID.TableSticker039,
			charaType = CharaType.Chara05,
			Name = "制服ヤオ（青）その2",
			Category = Category.TableSticker,
			Sort = SortType.Sort05
		},
		new Item
		{
			ObjectID = "OID_TableSticker040",
			ItemID = ItemID.TableSticker040,
			charaType = CharaType.Chara05,
			Name = "制服ヤオ（白）その2",
			Category = Category.TableSticker,
			Sort = SortType.Sort05
		},
		new Item
		{
			ObjectID = "OID_TableSticker041",
			ItemID = ItemID.TableSticker041,
			charaType = CharaType.Chara05,
			Name = "制服ヤオ（赤）その2",
			Category = Category.TableSticker,
			Sort = SortType.Sort05
		},
		new Item
		{
			ObjectID = "OID_TableSticker042",
			ItemID = ItemID.TableSticker042,
			charaType = CharaType.Chara05,
			Name = "制服ヤオ（緑）その2",
			Category = Category.TableSticker,
			Sort = SortType.Sort05
		},
		new Item
		{
			ObjectID = "OID_TableSticker043",
			ItemID = ItemID.TableSticker043,
			charaType = CharaType.Chara06,
			Name = "制服ミツバ（青）その1",
			Category = Category.TableSticker,
			Sort = SortType.Sort06
		},
		new Item
		{
			ObjectID = "OID_TableSticker044",
			ItemID = ItemID.TableSticker044,
			charaType = CharaType.Chara06,
			Name = "制服ミツバ（白）その1",
			Category = Category.TableSticker,
			Sort = SortType.Sort06
		},
		new Item
		{
			ObjectID = "OID_TableSticker045",
			ItemID = ItemID.TableSticker045,
			charaType = CharaType.Chara06,
			Name = "制服ミツバ（赤）その1",
			Category = Category.TableSticker,
			Sort = SortType.Sort06
		},
		new Item
		{
			ObjectID = "OID_TableSticker046",
			ItemID = ItemID.TableSticker046,
			charaType = CharaType.Chara06,
			Name = "制服ミツバ（緑）その1",
			Category = Category.TableSticker,
			Sort = SortType.Sort06
		},
		new Item
		{
			ObjectID = "OID_TableSticker047",
			ItemID = ItemID.TableSticker047,
			charaType = CharaType.Chara06,
			Name = "制服ミツバ（青）その2",
			Category = Category.TableSticker,
			Sort = SortType.Sort06
		},
		new Item
		{
			ObjectID = "OID_TableSticker048",
			ItemID = ItemID.TableSticker048,
			charaType = CharaType.Chara06,
			Name = "制服ミツバ（白）その2",
			Category = Category.TableSticker,
			Sort = SortType.Sort06
		},
		new Item
		{
			ObjectID = "OID_TableSticker049",
			ItemID = ItemID.TableSticker049,
			charaType = CharaType.Chara06,
			Name = "制服ミツバ（赤）その2",
			Category = Category.TableSticker,
			Sort = SortType.Sort06
		},
		new Item
		{
			ObjectID = "OID_TableSticker050",
			ItemID = ItemID.TableSticker050,
			charaType = CharaType.Chara06,
			Name = "制服ミツバ（緑）その2",
			Category = Category.TableSticker,
			Sort = SortType.Sort06
		},
		new Item
		{
			ObjectID = "OID_TableSticker051",
			ItemID = ItemID.TableSticker051,
			charaType = CharaType.Chara07,
			Name = "制服トイトイ（青）その1",
			Category = Category.TableSticker,
			Sort = SortType.Sort07
		},
		new Item
		{
			ObjectID = "OID_TableSticker052",
			ItemID = ItemID.TableSticker052,
			charaType = CharaType.Chara07,
			Name = "制服トイトイ（白）その1",
			Category = Category.TableSticker,
			Sort = SortType.Sort07
		},
		new Item
		{
			ObjectID = "OID_TableSticker053",
			ItemID = ItemID.TableSticker053,
			charaType = CharaType.Chara07,
			Name = "制服トイトイ（赤）その1",
			Category = Category.TableSticker,
			Sort = SortType.Sort07
		},
		new Item
		{
			ObjectID = "OID_TableSticker054",
			ItemID = ItemID.TableSticker054,
			charaType = CharaType.Chara07,
			Name = "制服トイトイ（緑）その1",
			Category = Category.TableSticker,
			Sort = SortType.Sort07
		},
		new Item
		{
			ObjectID = "OID_TableSticker055",
			ItemID = ItemID.TableSticker055,
			charaType = CharaType.Chara07,
			Name = "制服トイトイ（青）その2",
			Category = Category.TableSticker,
			Sort = SortType.Sort07
		},
		new Item
		{
			ObjectID = "OID_TableSticker056",
			ItemID = ItemID.TableSticker056,
			charaType = CharaType.Chara07,
			Name = "制服トイトイ（白）その2",
			Category = Category.TableSticker,
			Sort = SortType.Sort07
		},
		new Item
		{
			ObjectID = "OID_TableSticker057",
			ItemID = ItemID.TableSticker057,
			charaType = CharaType.Chara07,
			Name = "制服トイトイ（赤）その2",
			Category = Category.TableSticker,
			Sort = SortType.Sort07
		},
		new Item
		{
			ObjectID = "OID_TableSticker058",
			ItemID = ItemID.TableSticker058,
			charaType = CharaType.Chara07,
			Name = "制服トイトイ（緑）その2",
			Category = Category.TableSticker,
			Sort = SortType.Sort07
		},
		new Item
		{
			ObjectID = "OID_TableSticker059",
			ItemID = ItemID.TableSticker059,
			charaType = CharaType.Chara08,
			Name = "制服ムサシ（青）その1",
			Category = Category.TableSticker,
			Sort = SortType.Sort08
		},
		new Item
		{
			ObjectID = "OID_TableSticker060",
			ItemID = ItemID.TableSticker060,
			charaType = CharaType.Chara08,
			Name = "制服ムサシ（白）その1",
			Category = Category.TableSticker,
			Sort = SortType.Sort08
		},
		new Item
		{
			ObjectID = "OID_TableSticker061",
			ItemID = ItemID.TableSticker061,
			charaType = CharaType.Chara08,
			Name = "制服ムサシ（赤）その1",
			Category = Category.TableSticker,
			Sort = SortType.Sort08
		},
		new Item
		{
			ObjectID = "OID_TableSticker062",
			ItemID = ItemID.TableSticker062,
			charaType = CharaType.Chara08,
			Name = "制服ムサシ（緑）その1",
			Category = Category.TableSticker,
			Sort = SortType.Sort08
		},
		new Item
		{
			ObjectID = "OID_TableSticker063",
			ItemID = ItemID.TableSticker063,
			charaType = CharaType.Chara08,
			Name = "制服ムサシ（青）その2",
			Category = Category.TableSticker,
			Sort = SortType.Sort08
		},
		new Item
		{
			ObjectID = "OID_TableSticker064",
			ItemID = ItemID.TableSticker064,
			charaType = CharaType.Chara08,
			Name = "制服ムサシ（白）その2",
			Category = Category.TableSticker,
			Sort = SortType.Sort08
		},
		new Item
		{
			ObjectID = "OID_TableSticker065",
			ItemID = ItemID.TableSticker065,
			charaType = CharaType.Chara08,
			Name = "制服ムサシ（赤）その2",
			Category = Category.TableSticker,
			Sort = SortType.Sort08
		},
		new Item
		{
			ObjectID = "OID_TableSticker066",
			ItemID = ItemID.TableSticker066,
			charaType = CharaType.Chara08,
			Name = "制服ムサシ（緑）その2",
			Category = Category.TableSticker,
			Sort = SortType.Sort08
		},
		new Item
		{
			ObjectID = "OID_TableSticker067",
			ItemID = ItemID.TableSticker067,
			charaType = CharaType.Chara09,
			Name = "チャイナパイン（青）その1",
			Category = Category.TableSticker,
			Sort = SortType.Sort09
		},
		new Item
		{
			ObjectID = "OID_TableSticker068",
			ItemID = ItemID.TableSticker068,
			charaType = CharaType.Chara09,
			Name = "チャイナパイン（白）その1",
			Category = Category.TableSticker,
			Sort = SortType.Sort09
		},
		new Item
		{
			ObjectID = "OID_TableSticker069",
			ItemID = ItemID.TableSticker069,
			charaType = CharaType.Chara09,
			Name = "チャイナパイン（赤）その1",
			Category = Category.TableSticker,
			Sort = SortType.Sort09
		},
		new Item
		{
			ObjectID = "OID_TableSticker070",
			ItemID = ItemID.TableSticker070,
			charaType = CharaType.Chara09,
			Name = "チャイナパイン（緑）その1",
			Category = Category.TableSticker,
			Sort = SortType.Sort09
		},
		new Item
		{
			ObjectID = "OID_TableSticker071",
			ItemID = ItemID.TableSticker071,
			charaType = CharaType.Chara09,
			Name = "チャイナパイン（青）その2",
			Category = Category.TableSticker,
			Sort = SortType.Sort09
		},
		new Item
		{
			ObjectID = "OID_TableSticker072",
			ItemID = ItemID.TableSticker072,
			charaType = CharaType.Chara09,
			Name = "チャイナパイン（白）その2",
			Category = Category.TableSticker,
			Sort = SortType.Sort09
		},
		new Item
		{
			ObjectID = "OID_TableSticker073",
			ItemID = ItemID.TableSticker073,
			charaType = CharaType.Chara09,
			Name = "チャイナパイン（赤）その2",
			Category = Category.TableSticker,
			Sort = SortType.Sort09
		},
		new Item
		{
			ObjectID = "OID_TableSticker074",
			ItemID = ItemID.TableSticker074,
			charaType = CharaType.Chara09,
			Name = "チャイナパイン（緑）その2",
			Category = Category.TableSticker,
			Sort = SortType.Sort09
		},
		new Item
		{
			ObjectID = "OID_TableSticker075",
			ItemID = ItemID.TableSticker075,
			Name = "ナナツノグループ ロゴマーク",
			Category = Category.TableSticker,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_TableSticker076",
			ItemID = ItemID.TableSticker076,
			Name = "かわいい子にはリーチさせよ",
			Category = Category.TableSticker,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_TableSticker077",
			ItemID = ItemID.TableSticker077,
			charaType = CharaType.Chara08,
			Name = "ムサシ誕生日お祝い",
			Category = Category.TableSticker,
			Sort = SortType.Sort08
		},
		new Item
		{
			ObjectID = "OID_TableSticker078",
			ItemID = ItemID.TableSticker078,
			Name = "魔法陣ドラドラ",
			Category = Category.TableSticker,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_TableSticker079",
			ItemID = ItemID.TableSticker079,
			charaType = CharaType.Chara03,
			Name = "ツミレ誕生日お祝い",
			Category = Category.TableSticker,
			Sort = SortType.Sort03
		},
		new Item
		{
			ObjectID = "OID_TableSticker080",
			ItemID = ItemID.TableSticker080,
			charaType = CharaType.Chara01,
			Name = "ぶっとびヒヨリ",
			Category = Category.TableSticker,
			Sort = SortType.Sort01
		},
		new Item
		{
			ObjectID = "OID_TableSticker081",
			ItemID = ItemID.TableSticker081,
			charaType = CharaType.Chara02,
			Name = "ぶっとびセン",
			Category = Category.TableSticker,
			Sort = SortType.Sort02
		},
		new Item
		{
			ObjectID = "OID_TableSticker082",
			ItemID = ItemID.TableSticker082,
			charaType = CharaType.Chara03,
			Name = "ぶっとびツミレ",
			Category = Category.TableSticker,
			Sort = SortType.Sort03
		},
		new Item
		{
			ObjectID = "OID_TableSticker083",
			ItemID = ItemID.TableSticker083,
			charaType = CharaType.Chara04,
			Name = "ぶっとびテンシ",
			Category = Category.TableSticker,
			Sort = SortType.Sort04
		},
		new Item
		{
			ObjectID = "OID_TableSticker084",
			ItemID = ItemID.TableSticker084,
			charaType = CharaType.Chara05,
			Name = "ぶっとびヤオ",
			Category = Category.TableSticker,
			Sort = SortType.Sort05
		},
		new Item
		{
			ObjectID = "OID_TableSticker085",
			ItemID = ItemID.TableSticker085,
			charaType = CharaType.Chara06,
			Name = "ぶっとびミツバ",
			Category = Category.TableSticker,
			Sort = SortType.Sort06
		},
		new Item
		{
			ObjectID = "OID_TableSticker086",
			ItemID = ItemID.TableSticker086,
			charaType = CharaType.Chara07,
			Name = "ぶっとびトイトイ",
			Category = Category.TableSticker,
			Sort = SortType.Sort07
		},
		new Item
		{
			ObjectID = "OID_TableSticker087",
			ItemID = ItemID.TableSticker087,
			charaType = CharaType.Chara08,
			Name = "ぶっとびムサシ",
			Category = Category.TableSticker,
			Sort = SortType.Sort08
		},
		new Item
		{
			ObjectID = "OID_TableSticker088",
			ItemID = ItemID.TableSticker088,
			charaType = CharaType.Chara01,
			Name = "七夕短冊(ヒヨリ)",
			Category = Category.TableSticker,
			Sort = SortType.Sort91
		},
		new Item
		{
			ObjectID = "OID_TableSticker089",
			ItemID = ItemID.TableSticker089,
			charaType = CharaType.Chara02,
			Name = "七夕短冊(セン)",
			Category = Category.TableSticker,
			Sort = SortType.Sort91
		},
		new Item
		{
			ObjectID = "OID_TableSticker090",
			ItemID = ItemID.TableSticker090,
			charaType = CharaType.Chara03,
			Name = "七夕短冊(ツミレ)",
			Category = Category.TableSticker,
			Sort = SortType.Sort91
		},
		new Item
		{
			ObjectID = "OID_TableSticker091",
			ItemID = ItemID.TableSticker091,
			charaType = CharaType.Chara04,
			Name = "七夕短冊(テンシ)",
			Category = Category.TableSticker,
			Sort = SortType.Sort91
		},
		new Item
		{
			ObjectID = "OID_TableSticker092",
			ItemID = ItemID.TableSticker092,
			charaType = CharaType.Chara05,
			Name = "七夕短冊(ヤオ)",
			Category = Category.TableSticker,
			Sort = SortType.Sort91
		},
		new Item
		{
			ObjectID = "OID_TableSticker093",
			ItemID = ItemID.TableSticker093,
			charaType = CharaType.Chara06,
			Name = "七夕短冊(ミツバ)",
			Category = Category.TableSticker,
			Sort = SortType.Sort91
		},
		new Item
		{
			ObjectID = "OID_TableSticker094",
			ItemID = ItemID.TableSticker094,
			charaType = CharaType.Chara07,
			Name = "七夕短冊(トイトイ)",
			Category = Category.TableSticker,
			Sort = SortType.Sort91
		},
		new Item
		{
			ObjectID = "OID_TableSticker095",
			ItemID = ItemID.TableSticker095,
			charaType = CharaType.Chara08,
			Name = "七夕短冊(ムサシ)",
			Category = Category.TableSticker,
			Sort = SortType.Sort91
		},
		new Item
		{
			ObjectID = "OID_TableSticker096",
			ItemID = ItemID.TableSticker096,
			charaType = CharaType.Chara09,
			Name = "七夕短冊(パイン)",
			Category = Category.TableSticker,
			Sort = SortType.Sort91
		},
		new Item
		{
			ObjectID = "OID_TableSticker097",
			ItemID = ItemID.TableSticker097,
			Name = "猫に親番(クロ)",
			Category = Category.TableSticker,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_TableSticker098",
			ItemID = ItemID.TableSticker098,
			Name = "猫に親番(トラ)",
			Category = Category.TableSticker,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_TableSticker099",
			ItemID = ItemID.TableSticker099,
			Name = "転生★雀士",
			Category = Category.TableSticker,
			Sort = SortType.Sort90
		},
		new Item
		{
			ObjectID = "OID_TableSticker100",
			ItemID = ItemID.TableSticker100,
			charaType = CharaType.Chara10,
			Name = "きらめき高校制服詩織（青）その1",
			Category = Category.TableSticker,
			Sort = SortType.Sort10
		},
		new Item
		{
			ObjectID = "OID_TableSticker101",
			ItemID = ItemID.TableSticker101,
			charaType = CharaType.Chara10,
			Name = "きらめき高校制服詩織（白）その1",
			Category = Category.TableSticker,
			Sort = SortType.Sort10
		},
		new Item
		{
			ObjectID = "OID_TableSticker102",
			ItemID = ItemID.TableSticker102,
			charaType = CharaType.Chara10,
			Name = "きらめき高校制服詩織（赤）その1",
			Category = Category.TableSticker,
			Sort = SortType.Sort10
		},
		new Item
		{
			ObjectID = "OID_TableSticker103",
			ItemID = ItemID.TableSticker103,
			charaType = CharaType.Chara10,
			Name = "きらめき高校制服詩織（緑）その1",
			Category = Category.TableSticker,
			Sort = SortType.Sort10
		},
		new Item
		{
			ObjectID = "OID_TableSticker104",
			ItemID = ItemID.TableSticker104,
			charaType = CharaType.Chara10,
			Name = "きらめき高校制服詩織（青）その2",
			Category = Category.TableSticker,
			Sort = SortType.Sort10
		},
		new Item
		{
			ObjectID = "OID_TableSticker105",
			ItemID = ItemID.TableSticker105,
			charaType = CharaType.Chara10,
			Name = "きらめき高校制服詩織（白）その2",
			Category = Category.TableSticker,
			Sort = SortType.Sort10
		},
		new Item
		{
			ObjectID = "OID_TableSticker106",
			ItemID = ItemID.TableSticker106,
			charaType = CharaType.Chara10,
			Name = "きらめき高校制服詩織（赤）その2",
			Category = Category.TableSticker,
			Sort = SortType.Sort10
		},
		new Item
		{
			ObjectID = "OID_TableSticker107",
			ItemID = ItemID.TableSticker107,
			charaType = CharaType.Chara10,
			Name = "きらめき高校制服詩織（緑）その2",
			Category = Category.TableSticker,
			Sort = SortType.Sort10
		},
		new Item
		{
			ObjectID = "OID_TableSticker108",
			ItemID = ItemID.TableSticker108,
			Name = "スペースゲーム機",
			Category = Category.TableSticker,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_TableSticker109",
			ItemID = ItemID.TableSticker109,
			charaType = CharaType.Chara06,
			Name = "ミツバ誕生日お祝い",
			Category = Category.TableSticker,
			Sort = SortType.Sort06
		},
		new Item
		{
			ObjectID = "OID_TableSticker110",
			ItemID = ItemID.TableSticker110,
			Name = "黄龍とセン",
			Category = Category.TableSticker,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_TableSticker111",
			ItemID = ItemID.TableSticker111,
			Name = "宇宙人",
			Category = Category.TableSticker,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_TableSticker112",
			ItemID = ItemID.TableSticker112,
			charaType = CharaType.Chara06,
			Name = "シンデレラミツバ",
			Category = Category.TableSticker,
			Sort = SortType.Sort06
		},
		new Item
		{
			ObjectID = "OID_TableSticker113",
			ItemID = ItemID.TableSticker113,
			charaType = CharaType.Chara01,
			Name = "ヒヨリ誕生日お祝い",
			Category = Category.TableSticker,
			Sort = SortType.Sort01
		},
		new Item
		{
			ObjectID = "OID_TableSticker114",
			ItemID = ItemID.TableSticker114,
			charaType = CharaType.Chara01,
			Name = "KAC(2023) ヒヨリの意気込み",
			Category = Category.TableSticker,
			Sort = SortType.Sort92
		},
		new Item
		{
			ObjectID = "OID_TableSticker115",
			ItemID = ItemID.TableSticker115,
			charaType = CharaType.Chara02,
			Name = "KAC(2023) センの意気込み",
			Category = Category.TableSticker,
			Sort = SortType.Sort92
		},
		new Item
		{
			ObjectID = "OID_TableSticker116",
			ItemID = ItemID.TableSticker116,
			charaType = CharaType.Chara03,
			Name = "KAC(2023) ツミレの意気込み",
			Category = Category.TableSticker,
			Sort = SortType.Sort92
		},
		new Item
		{
			ObjectID = "OID_TableSticker117",
			ItemID = ItemID.TableSticker117,
			charaType = CharaType.Chara04,
			Name = "KAC(2023) テンシの意気込み",
			Category = Category.TableSticker,
			Sort = SortType.Sort92
		},
		new Item
		{
			ObjectID = "OID_TableSticker118",
			ItemID = ItemID.TableSticker118,
			charaType = CharaType.Chara05,
			Name = "KAC(2023) ヤオの意気込み",
			Category = Category.TableSticker,
			Sort = SortType.Sort92
		},
		new Item
		{
			ObjectID = "OID_TableSticker119",
			ItemID = ItemID.TableSticker119,
			charaType = CharaType.Chara06,
			Name = "KAC(2023) ミツバの意気込み",
			Category = Category.TableSticker,
			Sort = SortType.Sort92
		},
		new Item
		{
			ObjectID = "OID_TableSticker120",
			ItemID = ItemID.TableSticker120,
			charaType = CharaType.Chara07,
			Name = "KAC(2023) トイトイの意気込み",
			Category = Category.TableSticker,
			Sort = SortType.Sort92
		},
		new Item
		{
			ObjectID = "OID_TableSticker121",
			ItemID = ItemID.TableSticker121,
			charaType = CharaType.Chara08,
			Name = "KAC(2023) ムサシの意気込み",
			Category = Category.TableSticker,
			Sort = SortType.Sort92
		},
		new Item
		{
			ObjectID = "OID_TableSticker122",
			ItemID = ItemID.TableSticker122,
			charaType = CharaType.Chara09,
			Name = "KAC(2023) パインの意気込み",
			Category = Category.TableSticker,
			Sort = SortType.Sort92
		},
		new Item
		{
			ObjectID = "OID_TableSticker123",
			ItemID = ItemID.TableSticker123,
			charaType = CharaType.Chara10,
			Name = "KAC(2023) 詩織の意気込み",
			Category = Category.TableSticker,
			Sort = SortType.Sort92
		},
		new Item
		{
			ObjectID = "OID_TableSticker124",
			ItemID = ItemID.TableSticker124,
			charaType = CharaType.Chara04,
			Name = "テンシ誕生日お祝い",
			Category = Category.TableSticker,
			Sort = SortType.Sort04
		},
		new Item
		{
			ObjectID = "OID_TableSticker125",
			ItemID = ItemID.TableSticker125,
			Name = "くみゃ",
			Category = Category.TableSticker,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_TableSticker126",
			ItemID = ItemID.TableSticker126,
			charaType = CharaType.Chara02,
			Name = "セン誕生日お祝い",
			Category = Category.TableSticker,
			Sort = SortType.Sort02
		},
		new Item
		{
			ObjectID = "OID_TableSticker127",
			ItemID = ItemID.TableSticker127,
			Name = "ファイトガールはじめました",
			Category = Category.TableSticker,
			Sort = SortType.Sort90
		},
		new Item
		{
			ObjectID = "OID_TableSticker128",
			ItemID = ItemID.TableSticker128,
			charaType = CharaType.Chara11,
			Name = "制服カオス（青）その1",
			Category = Category.TableSticker,
			Sort = SortType.Sort11
		},
		new Item
		{
			ObjectID = "OID_TableSticker129",
			ItemID = ItemID.TableSticker129,
			charaType = CharaType.Chara11,
			Name = "制服カオス（白）その1",
			Category = Category.TableSticker,
			Sort = SortType.Sort11
		},
		new Item
		{
			ObjectID = "OID_TableSticker130",
			ItemID = ItemID.TableSticker130,
			charaType = CharaType.Chara11,
			Name = "制服カオス（赤）その1",
			Category = Category.TableSticker,
			Sort = SortType.Sort11
		},
		new Item
		{
			ObjectID = "OID_TableSticker131",
			ItemID = ItemID.TableSticker131,
			charaType = CharaType.Chara11,
			Name = "制服カオス（緑）その1",
			Category = Category.TableSticker,
			Sort = SortType.Sort11
		},
		new Item
		{
			ObjectID = "OID_TableSticker132",
			ItemID = ItemID.TableSticker132,
			charaType = CharaType.Chara11,
			Name = "制服カオス（青）その2",
			Category = Category.TableSticker,
			Sort = SortType.Sort11
		},
		new Item
		{
			ObjectID = "OID_TableSticker133",
			ItemID = ItemID.TableSticker133,
			charaType = CharaType.Chara11,
			Name = "制服カオス（白）その2",
			Category = Category.TableSticker,
			Sort = SortType.Sort11
		},
		new Item
		{
			ObjectID = "OID_TableSticker134",
			ItemID = ItemID.TableSticker134,
			charaType = CharaType.Chara11,
			Name = "制服カオス（赤）その2",
			Category = Category.TableSticker,
			Sort = SortType.Sort11
		},
		new Item
		{
			ObjectID = "OID_TableSticker135",
			ItemID = ItemID.TableSticker135,
			charaType = CharaType.Chara11,
			Name = "制服カオス（緑）その2",
			Category = Category.TableSticker,
			Sort = SortType.Sort11
		},
		new Item
		{
			ObjectID = "OID_TableSticker136",
			ItemID = ItemID.TableSticker136,
			Name = "秋の新作",
			Category = Category.TableSticker,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_TableSticker137",
			ItemID = ItemID.TableSticker137,
			Name = "メンダコ",
			Category = Category.TableSticker,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_TableSticker138",
			ItemID = ItemID.TableSticker138,
			Name = "雪の結晶",
			Category = Category.TableSticker,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_TableSticker139",
			ItemID = ItemID.TableSticker139,
			charaType = CharaType.Chara01,
			Name = "年賀状(ヒヨリ)",
			Category = Category.TableSticker,
			Sort = SortType.Sort93
		},
		new Item
		{
			ObjectID = "OID_TableSticker140",
			ItemID = ItemID.TableSticker140,
			charaType = CharaType.Chara02,
			Name = "年賀状(セン)",
			Category = Category.TableSticker,
			Sort = SortType.Sort93
		},
		new Item
		{
			ObjectID = "OID_TableSticker141",
			ItemID = ItemID.TableSticker141,
			charaType = CharaType.Chara03,
			Name = "年賀状(ツミレ)",
			Category = Category.TableSticker,
			Sort = SortType.Sort93
		},
		new Item
		{
			ObjectID = "OID_TableSticker142",
			ItemID = ItemID.TableSticker142,
			charaType = CharaType.Chara04,
			Name = "年賀状(テンシ)",
			Category = Category.TableSticker,
			Sort = SortType.Sort93
		},
		new Item
		{
			ObjectID = "OID_TableSticker143",
			ItemID = ItemID.TableSticker143,
			charaType = CharaType.Chara05,
			Name = "年賀状(ヤオ)",
			Category = Category.TableSticker,
			Sort = SortType.Sort93
		},
		new Item
		{
			ObjectID = "OID_TableSticker144",
			ItemID = ItemID.TableSticker144,
			charaType = CharaType.Chara06,
			Name = "年賀状(ミツバ)",
			Category = Category.TableSticker,
			Sort = SortType.Sort93
		},
		new Item
		{
			ObjectID = "OID_TableSticker145",
			ItemID = ItemID.TableSticker145,
			charaType = CharaType.Chara07,
			Name = "年賀状(トイトイ)",
			Category = Category.TableSticker,
			Sort = SortType.Sort93
		},
		new Item
		{
			ObjectID = "OID_TableSticker146",
			ItemID = ItemID.TableSticker146,
			charaType = CharaType.Chara08,
			Name = "年賀状(ムサシ)",
			Category = Category.TableSticker,
			Sort = SortType.Sort93
		},
		new Item
		{
			ObjectID = "OID_TableSticker147",
			ItemID = ItemID.TableSticker147,
			charaType = CharaType.Chara09,
			Name = "年賀状(パイン)",
			Category = Category.TableSticker,
			Sort = SortType.Sort93
		},
		new Item
		{
			ObjectID = "OID_TableSticker148",
			ItemID = ItemID.TableSticker148,
			charaType = CharaType.Chara10,
			Name = "年賀状(詩織)",
			Category = Category.TableSticker,
			Sort = SortType.Sort93
		},
		new Item
		{
			ObjectID = "OID_TableSticker149",
			ItemID = ItemID.TableSticker149,
			charaType = CharaType.Chara11,
			Name = "年賀状(カオス)",
			Category = Category.TableSticker,
			Sort = SortType.Sort93
		},
		new Item
		{
			ObjectID = "OID_TableSticker150",
			ItemID = ItemID.TableSticker150,
			charaType = CharaType.Chara12,
			Name = "制服クリア（青）その1",
			Category = Category.TableSticker,
			Sort = SortType.Sort12
		},
		new Item
		{
			ObjectID = "OID_TableSticker151",
			ItemID = ItemID.TableSticker151,
			charaType = CharaType.Chara12,
			Name = "制服クリア（白）その1",
			Category = Category.TableSticker,
			Sort = SortType.Sort12
		},
		new Item
		{
			ObjectID = "OID_TableSticker152",
			ItemID = ItemID.TableSticker152,
			charaType = CharaType.Chara12,
			Name = "制服クリア（赤）その1",
			Category = Category.TableSticker,
			Sort = SortType.Sort12
		},
		new Item
		{
			ObjectID = "OID_TableSticker153",
			ItemID = ItemID.TableSticker153,
			charaType = CharaType.Chara12,
			Name = "制服クリア（緑）その1",
			Category = Category.TableSticker,
			Sort = SortType.Sort12
		},
		new Item
		{
			ObjectID = "OID_TableSticker154",
			ItemID = ItemID.TableSticker154,
			charaType = CharaType.Chara12,
			Name = "制服クリア（青）その2",
			Category = Category.TableSticker,
			Sort = SortType.Sort12
		},
		new Item
		{
			ObjectID = "OID_TableSticker155",
			ItemID = ItemID.TableSticker155,
			charaType = CharaType.Chara12,
			Name = "制服クリア（白）その2",
			Category = Category.TableSticker,
			Sort = SortType.Sort12
		},
		new Item
		{
			ObjectID = "OID_TableSticker156",
			ItemID = ItemID.TableSticker156,
			charaType = CharaType.Chara12,
			Name = "制服クリア（赤）その2",
			Category = Category.TableSticker,
			Sort = SortType.Sort12
		},
		new Item
		{
			ObjectID = "OID_TableSticker157",
			ItemID = ItemID.TableSticker157,
			charaType = CharaType.Chara12,
			Name = "制服クリア（緑）その2",
			Category = Category.TableSticker,
			Sort = SortType.Sort12
		},
		new Item
		{
			ObjectID = "OID_TableSticker158",
			ItemID = ItemID.TableSticker158,
			charaType = CharaType.Chara05,
			Name = "ヤオ誕生日お祝い",
			Category = Category.TableSticker,
			Sort = SortType.Sort05
		},
		new Item
		{
			ObjectID = "OID_TableSticker159",
			ItemID = ItemID.TableSticker159,
			Name = "暗黒パッチワーク",
			Category = Category.TableSticker,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_TableSticker160",
			ItemID = ItemID.TableSticker160,
			charaType = CharaType.Chara09,
			Name = "パイン誕生日お祝い",
			Category = Category.TableSticker,
			Sort = SortType.Sort09
		},
		new Item
		{
			ObjectID = "OID_TableSticker161",
			ItemID = ItemID.TableSticker161,
			Name = "ファイトガール平和条約",
			Category = Category.TableSticker,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_TableSticker162",
			ItemID = ItemID.TableSticker162,
			Name = "桜の花びら",
			Category = Category.TableSticker,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_TableSticker163",
			ItemID = ItemID.TableSticker163,
			Name = "1st アニバーサリーステッカー",
			Category = Category.TableSticker,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_TableSticker164",
			ItemID = ItemID.TableSticker164,
			charaType = CharaType.Chara07,
			Name = "トイトイ誕生日お祝い",
			Category = Category.TableSticker,
			Sort = SortType.Sort07
		},
		new Item
		{
			ObjectID = "OID_TableSticker165",
			ItemID = ItemID.TableSticker165,
			Name = "急がば門前",
			Category = Category.TableSticker,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_TableSticker166",
			ItemID = ItemID.TableSticker166,
			Name = "煩悩退散",
			Category = Category.TableSticker,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_TableSticker167",
			ItemID = ItemID.TableSticker167,
			charaType = CharaType.Chara11,
			Name = "カオス誕生日お祝い",
			Category = Category.TableSticker,
			Sort = SortType.Sort11
		},
		new Item
		{
			ObjectID = "OID_TableSticker168",
			ItemID = ItemID.TableSticker168,
			charaType = CharaType.Chara12,
			Name = "クリア誕生日お祝い",
			Category = Category.TableSticker,
			Sort = SortType.Sort12
		},
		new Item
		{
			ObjectID = "OID_TableSticker169",
			ItemID = ItemID.TableSticker169,
			Name = "紫陽花",
			Category = Category.TableSticker,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_TableSticker170",
			ItemID = ItemID.TableSticker170,
			Name = "イッシキ建設求人ポスター",
			Category = Category.TableSticker,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_TableSticker171",
			ItemID = ItemID.TableSticker171,
			charaType = CharaType.Chara10,
			Name = "詩織誕生日お祝い",
			Category = Category.TableSticker,
			Sort = SortType.Sort10
		},
		new Item
		{
			ObjectID = "OID_TableSticker172",
			ItemID = ItemID.TableSticker172,
			charaType = CharaType.Chara08,
			Name = "ムサシ推し",
			Category = Category.TableSticker,
			Sort = SortType.Sort08
		},
		new Item
		{
			ObjectID = "OID_TableSticker173",
			ItemID = ItemID.TableSticker173,
			charaType = CharaType.Chara03,
			Name = "ツミレ推し",
			Category = Category.TableSticker,
			Sort = SortType.Sort03
		},
		new Item
		{
			ObjectID = "OID_TableSticker174",
			ItemID = ItemID.TableSticker174,
			charaType = CharaType.Chara01,
			Name = "推しうちわ(ヒヨリ)",
			Category = Category.TableSticker,
			Sort = SortType.Sort94
		},
		new Item
		{
			ObjectID = "OID_TableSticker175",
			ItemID = ItemID.TableSticker175,
			charaType = CharaType.Chara02,
			Name = "推しうちわ(セン)",
			Category = Category.TableSticker,
			Sort = SortType.Sort94
		},
		new Item
		{
			ObjectID = "OID_TableSticker176",
			ItemID = ItemID.TableSticker176,
			charaType = CharaType.Chara03,
			Name = "推しうちわ(ツミレ)",
			Category = Category.TableSticker,
			Sort = SortType.Sort94
		},
		new Item
		{
			ObjectID = "OID_TableSticker177",
			ItemID = ItemID.TableSticker177,
			charaType = CharaType.Chara04,
			Name = "推しうちわ(テンシ)",
			Category = Category.TableSticker,
			Sort = SortType.Sort94
		},
		new Item
		{
			ObjectID = "OID_TableSticker178",
			ItemID = ItemID.TableSticker178,
			charaType = CharaType.Chara05,
			Name = "推しうちわ(ヤオ)",
			Category = Category.TableSticker,
			Sort = SortType.Sort94
		},
		new Item
		{
			ObjectID = "OID_TableSticker179",
			ItemID = ItemID.TableSticker179,
			charaType = CharaType.Chara06,
			Name = "推しうちわ(ミツバ)",
			Category = Category.TableSticker,
			Sort = SortType.Sort94
		},
		new Item
		{
			ObjectID = "OID_TableSticker180",
			ItemID = ItemID.TableSticker180,
			charaType = CharaType.Chara07,
			Name = "推しうちわ(トイトイ)",
			Category = Category.TableSticker,
			Sort = SortType.Sort94
		},
		new Item
		{
			ObjectID = "OID_TableSticker181",
			ItemID = ItemID.TableSticker181,
			charaType = CharaType.Chara08,
			Name = "推しうちわ(ムサシ)",
			Category = Category.TableSticker,
			Sort = SortType.Sort94
		},
		new Item
		{
			ObjectID = "OID_TableSticker182",
			ItemID = ItemID.TableSticker182,
			charaType = CharaType.Chara09,
			Name = "推しうちわ(パイン)",
			Category = Category.TableSticker,
			Sort = SortType.Sort94
		},
		new Item
		{
			ObjectID = "OID_TableSticker183",
			ItemID = ItemID.TableSticker183,
			charaType = CharaType.Chara10,
			Name = "推しうちわ(詩織)",
			Category = Category.TableSticker,
			Sort = SortType.Sort94
		},
		new Item
		{
			ObjectID = "OID_TableSticker184",
			ItemID = ItemID.TableSticker184,
			charaType = CharaType.Chara11,
			Name = "推しうちわ(カオス)",
			Category = Category.TableSticker,
			Sort = SortType.Sort94
		},
		new Item
		{
			ObjectID = "OID_TableSticker185",
			ItemID = ItemID.TableSticker185,
			charaType = CharaType.Chara12,
			Name = "推しうちわ(クリア)",
			Category = Category.TableSticker,
			Sort = SortType.Sort94
		},
		new Item
		{
			ObjectID = "OID_TableSticker186",
			ItemID = ItemID.TableSticker186,
			Name = "ビーチサイドエンジェル",
			Category = Category.TableSticker,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_TableSticker187",
			ItemID = ItemID.TableSticker187,
			Name = "ナナツノ切手",
			Category = Category.TableSticker,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_TableSticker188",
			ItemID = ItemID.TableSticker188,
			charaType = CharaType.Chara13,
			Name = "制服イヨ（青）その1",
			Category = Category.TableSticker,
			Sort = SortType.Sort13
		},
		new Item
		{
			ObjectID = "OID_TableSticker189",
			ItemID = ItemID.TableSticker189,
			charaType = CharaType.Chara13,
			Name = "制服イヨ（白）その1",
			Category = Category.TableSticker,
			Sort = SortType.Sort13
		},
		new Item
		{
			ObjectID = "OID_TableSticker190",
			ItemID = ItemID.TableSticker190,
			charaType = CharaType.Chara13,
			Name = "制服イヨ（赤）その1",
			Category = Category.TableSticker,
			Sort = SortType.Sort13
		},
		new Item
		{
			ObjectID = "OID_TableSticker191",
			ItemID = ItemID.TableSticker191,
			charaType = CharaType.Chara13,
			Name = "制服イヨ（緑）その1",
			Category = Category.TableSticker,
			Sort = SortType.Sort13
		},
		new Item
		{
			ObjectID = "OID_TableSticker192",
			ItemID = ItemID.TableSticker192,
			charaType = CharaType.Chara13,
			Name = "制服イヨ（青）その2",
			Category = Category.TableSticker,
			Sort = SortType.Sort13
		},
		new Item
		{
			ObjectID = "OID_TableSticker193",
			ItemID = ItemID.TableSticker193,
			charaType = CharaType.Chara13,
			Name = "制服イヨ（白）その2",
			Category = Category.TableSticker,
			Sort = SortType.Sort13
		},
		new Item
		{
			ObjectID = "OID_TableSticker194",
			ItemID = ItemID.TableSticker194,
			charaType = CharaType.Chara13,
			Name = "制服イヨ（赤）その2",
			Category = Category.TableSticker,
			Sort = SortType.Sort13
		},
		new Item
		{
			ObjectID = "OID_TableSticker195",
			ItemID = ItemID.TableSticker195,
			charaType = CharaType.Chara13,
			Name = "制服イヨ（緑）その2",
			Category = Category.TableSticker,
			Sort = SortType.Sort13
		},
		new Item
		{
			ObjectID = "OID_TableSticker196",
			ItemID = ItemID.TableSticker196,
			charaType = CharaType.Chara06,
			Name = "ミツバ推し",
			Category = Category.TableSticker,
			Sort = SortType.Sort06
		},
		new Item
		{
			ObjectID = "OID_TableSticker197",
			ItemID = ItemID.TableSticker197,
			Name = "博麗のお札",
			Category = Category.TableSticker,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_TableSticker198",
			ItemID = ItemID.TableSticker198,
			charaType = CharaType.Chara01,
			Name = "ヒヨリ推し",
			Category = Category.TableSticker,
			Sort = SortType.Sort01
		},
		new Item
		{
			ObjectID = "OID_TableSticker199",
			ItemID = ItemID.TableSticker199,
			Name = "1日1クール!",
			Category = Category.TableSticker,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_TableSticker200",
			ItemID = ItemID.TableSticker200,
			charaType = CharaType.Chara04,
			Name = "テンシ推し",
			Category = Category.TableSticker,
			Sort = SortType.Sort04
		},
		new Item
		{
			ObjectID = "OID_TableSticker201",
			ItemID = ItemID.TableSticker201,
			charaType = CharaType.Chara09,
			Name = "ぶっとびパイン",
			Category = Category.TableSticker,
			Sort = SortType.Sort09
		},
		new Item
		{
			ObjectID = "OID_TableSticker202",
			ItemID = ItemID.TableSticker202,
			charaType = CharaType.Chara10,
			Name = "ぶっとび詩織",
			Category = Category.TableSticker,
			Sort = SortType.Sort10
		},
		new Item
		{
			ObjectID = "OID_TableSticker203",
			ItemID = ItemID.TableSticker203,
			charaType = CharaType.Chara11,
			Name = "ぶっとびカオス",
			Category = Category.TableSticker,
			Sort = SortType.Sort11
		},
		new Item
		{
			ObjectID = "OID_TableSticker204",
			ItemID = ItemID.TableSticker204,
			charaType = CharaType.Chara12,
			Name = "ぶっとびクリア",
			Category = Category.TableSticker,
			Sort = SortType.Sort12
		},
		new Item
		{
			ObjectID = "OID_TableSticker205",
			ItemID = ItemID.TableSticker205,
			charaType = CharaType.Chara13,
			Name = "ぶっとびイヨ",
			Category = Category.TableSticker,
			Sort = SortType.Sort13
		},
		new Item
		{
			ObjectID = "OID_TableSticker206",
			ItemID = ItemID.TableSticker206,
			charaType = CharaType.Chara11,
			Name = "七夕短冊(カオス)",
			Category = Category.TableSticker,
			Sort = SortType.Sort91
		},
		new Item
		{
			ObjectID = "OID_TableSticker207",
			ItemID = ItemID.TableSticker207,
			charaType = CharaType.Chara12,
			Name = "七夕短冊(クリア)",
			Category = Category.TableSticker,
			Sort = SortType.Sort91
		},
		new Item
		{
			ObjectID = "OID_TableSticker208",
			ItemID = ItemID.TableSticker208,
			charaType = CharaType.Chara13,
			Name = "七夕短冊(イヨ)",
			Category = Category.TableSticker,
			Sort = SortType.Sort91
		},
		new Item
		{
			ObjectID = "OID_TableSticker209",
			ItemID = ItemID.TableSticker209,
			Name = "HAPPY HALLOWEEN",
			Category = Category.TableSticker,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_TableSticker210",
			ItemID = ItemID.TableSticker210,
			charaType = CharaType.Chara14,
			Name = "チャイナグリム・アロエ（青）その1",
			Category = Category.TableSticker,
			Sort = SortType.Sort14
		},
		new Item
		{
			ObjectID = "OID_TableSticker211",
			ItemID = ItemID.TableSticker211,
			charaType = CharaType.Chara14,
			Name = "チャイナグリム・アロエ（白）その1",
			Category = Category.TableSticker,
			Sort = SortType.Sort14
		},
		new Item
		{
			ObjectID = "OID_TableSticker212",
			ItemID = ItemID.TableSticker212,
			charaType = CharaType.Chara14,
			Name = "チャイナグリム・アロエ（赤）その1",
			Category = Category.TableSticker,
			Sort = SortType.Sort14
		},
		new Item
		{
			ObjectID = "OID_TableSticker213",
			ItemID = ItemID.TableSticker213,
			charaType = CharaType.Chara14,
			Name = "チャイナグリム・アロエ（緑）その1",
			Category = Category.TableSticker,
			Sort = SortType.Sort14
		},
		new Item
		{
			ObjectID = "OID_TableSticker214",
			ItemID = ItemID.TableSticker214,
			charaType = CharaType.Chara14,
			Name = "チャイナグリム・アロエ（青）その2",
			Category = Category.TableSticker,
			Sort = SortType.Sort14
		},
		new Item
		{
			ObjectID = "OID_TableSticker215",
			ItemID = ItemID.TableSticker215,
			charaType = CharaType.Chara14,
			Name = "チャイナグリム・アロエ（白）その2",
			Category = Category.TableSticker,
			Sort = SortType.Sort14
		},
		new Item
		{
			ObjectID = "OID_TableSticker216",
			ItemID = ItemID.TableSticker216,
			charaType = CharaType.Chara14,
			Name = "チャイナグリム・アロエ（赤）その2",
			Category = Category.TableSticker,
			Sort = SortType.Sort14
		},
		new Item
		{
			ObjectID = "OID_TableSticker217",
			ItemID = ItemID.TableSticker217,
			charaType = CharaType.Chara14,
			Name = "チャイナグリム・アロエ（緑）その2",
			Category = Category.TableSticker,
			Sort = SortType.Sort14
		},
		new Item
		{
			ObjectID = "OID_TableSticker218",
			ItemID = ItemID.TableSticker218,
			charaType = CharaType.Chara02,
			Name = "セン推し",
			Category = Category.TableSticker,
			Sort = SortType.Sort02
		},
		new Item
		{
			ObjectID = "OID_TableSticker219",
			ItemID = ItemID.TableSticker219,
			Name = "イッシキカード",
			Category = Category.TableSticker,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_TableSticker220",
			ItemID = ItemID.TableSticker220,
			Name = "黄龍とミツバ",
			Category = Category.TableSticker,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_TableSticker221",
			ItemID = ItemID.TableSticker221,
			charaType = CharaType.Chara15,
			Name = "制服ココア（青）その1",
			Category = Category.TableSticker,
			Sort = SortType.Sort15
		},
		new Item
		{
			ObjectID = "OID_TableSticker222",
			ItemID = ItemID.TableSticker222,
			charaType = CharaType.Chara15,
			Name = "制服ココア（白）その1",
			Category = Category.TableSticker,
			Sort = SortType.Sort15
		},
		new Item
		{
			ObjectID = "OID_TableSticker223",
			ItemID = ItemID.TableSticker223,
			charaType = CharaType.Chara15,
			Name = "制服ココア（赤）その1",
			Category = Category.TableSticker,
			Sort = SortType.Sort15
		},
		new Item
		{
			ObjectID = "OID_TableSticker224",
			ItemID = ItemID.TableSticker224,
			charaType = CharaType.Chara15,
			Name = "制服ココア（緑）その1",
			Category = Category.TableSticker,
			Sort = SortType.Sort15
		},
		new Item
		{
			ObjectID = "OID_TableSticker225",
			ItemID = ItemID.TableSticker225,
			charaType = CharaType.Chara15,
			Name = "制服ココア（青）その2",
			Category = Category.TableSticker,
			Sort = SortType.Sort15
		},
		new Item
		{
			ObjectID = "OID_TableSticker226",
			ItemID = ItemID.TableSticker226,
			charaType = CharaType.Chara15,
			Name = "制服ココア（白）その2",
			Category = Category.TableSticker,
			Sort = SortType.Sort15
		},
		new Item
		{
			ObjectID = "OID_TableSticker227",
			ItemID = ItemID.TableSticker227,
			charaType = CharaType.Chara15,
			Name = "制服ココア（赤）その2",
			Category = Category.TableSticker,
			Sort = SortType.Sort15
		},
		new Item
		{
			ObjectID = "OID_TableSticker228",
			ItemID = ItemID.TableSticker228,
			charaType = CharaType.Chara15,
			Name = "制服ココア（緑）その2",
			Category = Category.TableSticker,
			Sort = SortType.Sort15
		},
		new Item
		{
			ObjectID = "OID_TableSticker229",
			ItemID = ItemID.TableSticker229,
			Name = "イヨにリー棒",
			Category = Category.TableSticker,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_TableSticker230",
			ItemID = ItemID.TableSticker230,
			charaType = CharaType.Chara13,
			Name = "イヨ推し",
			Category = Category.TableSticker,
			Sort = SortType.Sort13
		},
		new Item
		{
			ObjectID = "OID_TableSticker231",
			ItemID = ItemID.TableSticker231,
			charaType = CharaType.Chara14,
			Name = "ぶっとびグリアロ",
			Category = Category.TableSticker,
			Sort = SortType.Sort14
		},
		new Item
		{
			ObjectID = "OID_TableSticker232",
			ItemID = ItemID.TableSticker232,
			charaType = CharaType.Chara15,
			Name = "ぶっとびココア",
			Category = Category.TableSticker,
			Sort = SortType.Sort15
		},
		new Item
		{
			ObjectID = "OID_TableSticker233",
			ItemID = ItemID.TableSticker233,
			Name = "Happy Valentine's Day",
			Category = Category.TableSticker,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_TableSticker234",
			ItemID = ItemID.TableSticker234,
			charaType = CharaType.Chara05,
			Name = "ヤオ推し",
			Category = Category.TableSticker,
			Sort = SortType.Sort05
		},
		new Item
		{
			ObjectID = "OID_TableSticker235",
			ItemID = ItemID.TableSticker235,
			Name = "チョンボーローシール",
			Category = Category.TableSticker,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_TableSticker236",
			ItemID = ItemID.TableSticker236,
			charaType = CharaType.Chara14,
			Name = "グリアロ推し",
			Category = Category.TableSticker,
			Sort = SortType.Sort14
		},
		new Item
		{
			ObjectID = "OID_TableSticker237",
			ItemID = ItemID.TableSticker237,
			charaType = CharaType.Chara15,
			Name = "ココア推し",
			Category = Category.TableSticker,
			Sort = SortType.Sort15
		},
		new Item
		{
			ObjectID = "OID_TableSticker238",
			ItemID = ItemID.TableSticker238,
			charaType = CharaType.Chara16,
			Name = "教師ダイア（青）その1",
			Category = Category.TableSticker,
			Sort = SortType.Sort16
		},
		new Item
		{
			ObjectID = "OID_TableSticker239",
			ItemID = ItemID.TableSticker239,
			charaType = CharaType.Chara16,
			Name = "教師ダイア（白）その1",
			Category = Category.TableSticker,
			Sort = SortType.Sort16
		},
		new Item
		{
			ObjectID = "OID_TableSticker240",
			ItemID = ItemID.TableSticker240,
			charaType = CharaType.Chara16,
			Name = "教師ダイア（赤）その1",
			Category = Category.TableSticker,
			Sort = SortType.Sort16
		},
		new Item
		{
			ObjectID = "OID_TableSticker241",
			ItemID = ItemID.TableSticker241,
			charaType = CharaType.Chara16,
			Name = "教師ダイア（緑）その1",
			Category = Category.TableSticker,
			Sort = SortType.Sort16
		},
		new Item
		{
			ObjectID = "OID_TableSticker242",
			ItemID = ItemID.TableSticker242,
			charaType = CharaType.Chara16,
			Name = "教師ダイア（青）その2",
			Category = Category.TableSticker,
			Sort = SortType.Sort16
		},
		new Item
		{
			ObjectID = "OID_TableSticker243",
			ItemID = ItemID.TableSticker243,
			charaType = CharaType.Chara16,
			Name = "教師ダイア（白）その2",
			Category = Category.TableSticker,
			Sort = SortType.Sort16
		},
		new Item
		{
			ObjectID = "OID_TableSticker244",
			ItemID = ItemID.TableSticker244,
			charaType = CharaType.Chara16,
			Name = "教師ダイア（赤）その2",
			Category = Category.TableSticker,
			Sort = SortType.Sort16
		},
		new Item
		{
			ObjectID = "OID_TableSticker245",
			ItemID = ItemID.TableSticker245,
			charaType = CharaType.Chara16,
			Name = "教師ダイア（緑）その2",
			Category = Category.TableSticker,
			Sort = SortType.Sort16
		},
		new Item
		{
			ObjectID = "OID_TableSticker246",
			ItemID = ItemID.TableSticker246,
			charaType = CharaType.Chara09,
			Name = "パイン推し",
			Category = Category.TableSticker,
			Sort = SortType.Sort09
		},
		new Item
		{
			ObjectID = "OID_TableSticker247",
			ItemID = ItemID.TableSticker247,
			Name = "2nd アニバーサリーステッカー",
			Category = Category.TableSticker,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_TableSticker248",
			ItemID = ItemID.TableSticker248,
			charaType = CharaType.Chara16,
			Name = "ダイア推し",
			Category = Category.TableSticker,
			Sort = SortType.Sort16
		},
		new Item
		{
			ObjectID = "OID_TableSticker249",
			ItemID = ItemID.TableSticker249,
			charaType = CharaType.Chara07,
			Name = "トイトイ推し",
			Category = Category.TableSticker,
			Sort = SortType.Sort07
		},
		new Item
		{
			ObjectID = "OID_TableSticker250",
			ItemID = ItemID.TableSticker250,
			charaType = CharaType.Chara16,
			Name = "ぶっとびダイア",
			Category = Category.TableSticker,
			Sort = SortType.Sort16
		},
		new Item
		{
			ObjectID = "OID_TableSticker251",
			ItemID = ItemID.TableSticker251,
			Name = "イッシキタトゥー",
			Category = Category.TableSticker,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_TableSticker252",
			ItemID = ItemID.TableSticker252,
			charaType = CharaType.Chara11,
			Name = "カオス推し",
			Category = Category.TableSticker,
			Sort = SortType.Sort11
		},
		new Item
		{
			ObjectID = "OID_TableSticker253",
			ItemID = ItemID.TableSticker253,
			charaType = CharaType.Chara12,
			Name = "クリア推し",
			Category = Category.TableSticker,
			Sort = SortType.Sort12
		},
		new Item
		{
			ObjectID = "OID_TableSticker254",
			ItemID = ItemID.TableSticker254,
			charaType = CharaType.Chara10,
			Name = "詩織推し",
			Category = Category.TableSticker,
			Sort = SortType.Sort10
		},
		new Item
		{
			ObjectID = "OID_TableSticker255",
			ItemID = ItemID.TableSticker255,
			charaType = CharaType.Chara08,
			Name = "ムサシ命",
			Category = Category.TableSticker,
			Sort = SortType.Sort08
		},
		new Item
		{
			ObjectID = "OID_TableSticker256",
			ItemID = ItemID.TableSticker256,
			charaType = CharaType.Chara01,
			Name = "World of Peace",
			Category = Category.TableSticker,
			Sort = SortType.Sort01
		},
		new Item
		{
			ObjectID = "OID_TableSticker257",
			ItemID = ItemID.TableSticker257,
			charaType = CharaType.Chara02,
			Name = "Reach for …",
			Category = Category.TableSticker,
			Sort = SortType.Sort02
		},
		new Item
		{
			ObjectID = "OID_TableSticker258",
			ItemID = ItemID.TableSticker258,
			charaType = CharaType.Chara17,
			Name = "シスターダブリエル（青）その1",
			Category = Category.TableSticker,
			Sort = SortType.Sort17
		},
		new Item
		{
			ObjectID = "OID_TableSticker259",
			ItemID = ItemID.TableSticker259,
			charaType = CharaType.Chara17,
			Name = "シスターダブリエル（白）その1",
			Category = Category.TableSticker,
			Sort = SortType.Sort17
		},
		new Item
		{
			ObjectID = "OID_TableSticker260",
			ItemID = ItemID.TableSticker260,
			charaType = CharaType.Chara17,
			Name = "シスターダブリエル（赤）その1",
			Category = Category.TableSticker,
			Sort = SortType.Sort17
		},
		new Item
		{
			ObjectID = "OID_TableSticker261",
			ItemID = ItemID.TableSticker261,
			charaType = CharaType.Chara17,
			Name = "シスターダブリエル（緑）その1",
			Category = Category.TableSticker,
			Sort = SortType.Sort17
		},
		new Item
		{
			ObjectID = "OID_TableSticker262",
			ItemID = ItemID.TableSticker262,
			charaType = CharaType.Chara17,
			Name = "シスターダブリエル（青）その2",
			Category = Category.TableSticker,
			Sort = SortType.Sort17
		},
		new Item
		{
			ObjectID = "OID_TableSticker263",
			ItemID = ItemID.TableSticker263,
			charaType = CharaType.Chara17,
			Name = "シスターダブリエル（白）その2",
			Category = Category.TableSticker,
			Sort = SortType.Sort17
		},
		new Item
		{
			ObjectID = "OID_TableSticker264",
			ItemID = ItemID.TableSticker264,
			charaType = CharaType.Chara17,
			Name = "シスターダブリエル（赤）その2",
			Category = Category.TableSticker,
			Sort = SortType.Sort17
		},
		new Item
		{
			ObjectID = "OID_TableSticker265",
			ItemID = ItemID.TableSticker265,
			charaType = CharaType.Chara17,
			Name = "シスターダブリエル（緑）その2",
			Category = Category.TableSticker,
			Sort = SortType.Sort17
		},
		new Item
		{
			ObjectID = "OID_TableSticker266",
			ItemID = ItemID.TableSticker266,
			charaType = CharaType.Chara03,
			Name = "ツミレ命",
			Category = Category.TableSticker,
			Sort = SortType.Sort03
		},
		new Item
		{
			ObjectID = "OID_TableSticker267",
			ItemID = ItemID.TableSticker267,
			Name = "パイねこステッカー",
			Category = Category.TableSticker,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_TableSticker268",
			ItemID = ItemID.TableSticker268,
			charaType = CharaType.Chara05,
			Name = "Rice up！",
			Category = Category.TableSticker,
			Sort = SortType.Sort05
		},
		new Item
		{
			ObjectID = "OID_TableSticker269",
			ItemID = ItemID.TableSticker269,
			Name = "十返舎なしこ",
			Category = Category.TableSticker,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_TableSticker270",
			ItemID = ItemID.TableSticker270,
			Name = "河東ぱい",
			Category = Category.TableSticker,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_TableSticker271",
			ItemID = ItemID.TableSticker271,
			Name = "徳富泉",
			Category = Category.TableSticker,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_TableSticker272",
			ItemID = ItemID.TableSticker272,
			Name = "林リーチェ",
			Category = Category.TableSticker,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_TableSticker273",
			ItemID = ItemID.TableSticker273,
			Name = "江見跳",
			Category = Category.TableSticker,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_TableSticker274",
			ItemID = ItemID.TableSticker274,
			charaType = CharaType.Chara06,
			Name = "ミツバ命",
			Category = Category.TableSticker,
			Sort = SortType.Sort06
		},
		new Item
		{
			ObjectID = "OID_TableSticker275",
			ItemID = ItemID.TableSticker275,
			charaType = CharaType.Chara17,
			Name = "ぶっとびダブリエル",
			Category = Category.TableSticker,
			Sort = SortType.Sort17
		},
		new Item
		{
			ObjectID = "OID_TableSticker276",
			ItemID = ItemID.TableSticker276,
			charaType = CharaType.Chara01,
			Name = "ヒヨリ命",
			Category = Category.TableSticker,
			Sort = SortType.Sort01
		},
		new Item
		{
			ObjectID = "OID_TableSticker277",
			ItemID = ItemID.TableSticker277,
			charaType = CharaType.Chara04,
			Name = "豪運♪ロマンス",
			Category = Category.TableSticker,
			Sort = SortType.Sort04
		},
		new Item
		{
			ObjectID = "OID_TableSticker278",
			ItemID = ItemID.TableSticker278,
			charaType = CharaType.Chara18,
			Name = "私服イッパツ（青）その1",
			Category = Category.TableSticker,
			Sort = SortType.Sort18
		},
		new Item
		{
			ObjectID = "OID_TableSticker279",
			ItemID = ItemID.TableSticker279,
			charaType = CharaType.Chara18,
			Name = "私服イッパツ（白）その1",
			Category = Category.TableSticker,
			Sort = SortType.Sort18
		},
		new Item
		{
			ObjectID = "OID_TableSticker280",
			ItemID = ItemID.TableSticker280,
			charaType = CharaType.Chara18,
			Name = "私服イッパツ（赤）その1",
			Category = Category.TableSticker,
			Sort = SortType.Sort18
		},
		new Item
		{
			ObjectID = "OID_TableSticker281",
			ItemID = ItemID.TableSticker281,
			charaType = CharaType.Chara18,
			Name = "私服イッパツ（緑）その1",
			Category = Category.TableSticker,
			Sort = SortType.Sort18
		},
		new Item
		{
			ObjectID = "OID_TableSticker282",
			ItemID = ItemID.TableSticker282,
			charaType = CharaType.Chara18,
			Name = "私服イッパツ（青）その2",
			Category = Category.TableSticker,
			Sort = SortType.Sort18
		},
		new Item
		{
			ObjectID = "OID_TableSticker283",
			ItemID = ItemID.TableSticker283,
			charaType = CharaType.Chara18,
			Name = "私服イッパツ（白）その2",
			Category = Category.TableSticker,
			Sort = SortType.Sort18
		},
		new Item
		{
			ObjectID = "OID_TableSticker284",
			ItemID = ItemID.TableSticker284,
			charaType = CharaType.Chara18,
			Name = "私服イッパツ（赤）その2",
			Category = Category.TableSticker,
			Sort = SortType.Sort18
		},
		new Item
		{
			ObjectID = "OID_TableSticker285",
			ItemID = ItemID.TableSticker285,
			charaType = CharaType.Chara18,
			Name = "私服イッパツ（緑）その2",
			Category = Category.TableSticker,
			Sort = SortType.Sort18
		},
		new Item
		{
			ObjectID = "OID_TableSticker286",
			ItemID = ItemID.TableSticker286,
			charaType = CharaType.Chara04,
			Name = "テンシ命",
			Category = Category.TableSticker,
			Sort = SortType.Sort04
		},
		new Item
		{
			ObjectID = "OID_TableSticker287",
			ItemID = ItemID.TableSticker287,
			Name = "お世話券 1年分",
			Category = Category.TableSticker,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_TableSticker288",
			ItemID = ItemID.TableSticker288,
			Name = "緑型壱式取扱説明書",
			Category = Category.TableSticker,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_TableSticker289",
			ItemID = ItemID.TableSticker289,
			charaType = CharaType.Chara18,
			Name = "イッパツ命",
			Category = Category.TableSticker,
			Sort = SortType.Sort18
		},
		new Item
		{
			ObjectID = "OID_TableSticker290",
			ItemID = ItemID.TableSticker290,
			charaType = CharaType.Chara02,
			Name = "セン命",
			Category = Category.TableSticker,
			Sort = SortType.Sort02
		},
		new Item
		{
			ObjectID = "OID_TableSticker291",
			ItemID = ItemID.TableSticker291,
			Name = "おさんぽサバ缶",
			Category = Category.TableSticker,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_TableSticker292",
			ItemID = ItemID.TableSticker292,
			Name = "おさんぽハンニバル",
			Category = Category.TableSticker,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_TableSticker293",
			ItemID = ItemID.TableSticker293,
			charaType = CharaType.Chara17,
			Name = "ダブリエル命",
			Category = Category.TableSticker,
			Sort = SortType.Sort17
		},
		new Item
		{
			ObjectID = "OID_TableSticker294",
			ItemID = ItemID.TableSticker294,
			charaType = CharaType.Chara08,
			Name = "戦七彩 いざ舞乱",
			Category = Category.TableSticker,
			Sort = SortType.Sort08
		},
		new Item
		{
			ObjectID = "OID_TableSticker295",
			ItemID = ItemID.TableSticker295,
			Name = "ハテナ",
			Category = Category.TableSticker,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_TableSticker296",
			ItemID = ItemID.TableSticker296,
			Name = "ビックリ",
			Category = Category.TableSticker,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_TableSticker297",
			ItemID = ItemID.TableSticker297,
			Name = "ハート",
			Category = Category.TableSticker,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_TableSticker298",
			ItemID = ItemID.TableSticker298,
			Name = "ドンッ",
			Category = Category.TableSticker,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_TableSticker299",
			ItemID = ItemID.TableSticker299,
			Name = "お花",
			Category = Category.TableSticker,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_TableSticker300",
			ItemID = ItemID.TableSticker300,
			Name = "キラキラ",
			Category = Category.TableSticker,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_TableSticker301",
			ItemID = ItemID.TableSticker301,
			Name = "おひさま",
			Category = Category.TableSticker,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_TableSticker302",
			ItemID = ItemID.TableSticker302,
			Name = "相合傘",
			Category = Category.TableSticker,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_TableSticker303",
			ItemID = ItemID.TableSticker303,
			charaType = CharaType.Chara19,
			Name = "チャイナシロヱ（青）その1",
			Category = Category.TableSticker,
			Sort = SortType.Sort19
		},
		new Item
		{
			ObjectID = "OID_TableSticker304",
			ItemID = ItemID.TableSticker304,
			charaType = CharaType.Chara19,
			Name = "チャイナシロヱ（白）その1",
			Category = Category.TableSticker,
			Sort = SortType.Sort19
		},
		new Item
		{
			ObjectID = "OID_TableSticker305",
			ItemID = ItemID.TableSticker305,
			charaType = CharaType.Chara19,
			Name = "チャイナシロヱ（赤）その1",
			Category = Category.TableSticker,
			Sort = SortType.Sort19
		},
		new Item
		{
			ObjectID = "OID_TableSticker306",
			ItemID = ItemID.TableSticker306,
			charaType = CharaType.Chara19,
			Name = "チャイナシロヱ（緑）その1",
			Category = Category.TableSticker,
			Sort = SortType.Sort19
		},
		new Item
		{
			ObjectID = "OID_TableSticker307",
			ItemID = ItemID.TableSticker307,
			charaType = CharaType.Chara19,
			Name = "チャイナシロヱ（青）その2",
			Category = Category.TableSticker,
			Sort = SortType.Sort19
		},
		new Item
		{
			ObjectID = "OID_TableSticker308",
			ItemID = ItemID.TableSticker308,
			charaType = CharaType.Chara19,
			Name = "チャイナシロヱ（白）その2",
			Category = Category.TableSticker,
			Sort = SortType.Sort19
		},
		new Item
		{
			ObjectID = "OID_TableSticker309",
			ItemID = ItemID.TableSticker309,
			charaType = CharaType.Chara19,
			Name = "チャイナシロヱ（赤）その2",
			Category = Category.TableSticker,
			Sort = SortType.Sort19
		},
		new Item
		{
			ObjectID = "OID_TableSticker310",
			ItemID = ItemID.TableSticker310,
			charaType = CharaType.Chara19,
			Name = "チャイナシロヱ（緑）その2",
			Category = Category.TableSticker,
			Sort = SortType.Sort19
		},
		new Item
		{
			ObjectID = "OID_TableSticker311",
			ItemID = ItemID.TableSticker311,
			Name = "ボンバーガール レインボー",
			Category = Category.TableSticker,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_TableSticker312",
			ItemID = ItemID.TableSticker312,
			Name = "クイズマジックアカデミー 緋のアルカディア",
			Category = Category.TableSticker,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_TableSticker313",
			ItemID = ItemID.TableSticker313,
			Name = "稼働1000日記念",
			Category = Category.TableSticker,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_TableSticker314",
			ItemID = ItemID.TableSticker314,
			charaType = CharaType.Chara18,
			Name = "ぶっとびイッパツ",
			Category = Category.TableSticker,
			Sort = SortType.Sort18
		},
		new Item
		{
			ObjectID = "OID_TableSticker315",
			ItemID = ItemID.TableSticker315,
			Name = "黄龍とテンシ",
			Category = Category.TableSticker,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_TableSticker316",
			ItemID = ItemID.TableSticker316,
			charaType = CharaType.Chara13,
			Name = "イヨ命",
			Category = Category.TableSticker,
			Sort = SortType.Sort13
		},
		new Item
		{
			ObjectID = "OID_TableSticker317",
			ItemID = ItemID.TableSticker317,
			charaType = CharaType.Chara19,
			Name = "シロヱ命",
			Category = Category.TableSticker,
			Sort = SortType.Sort19
		},
		new Item
		{
			ObjectID = "OID_TableSticker318",
			ItemID = ItemID.TableSticker318,
			charaType = CharaType.Chara05,
			Name = "ヤオ命",
			Category = Category.TableSticker,
			Sort = SortType.Sort05
		},
		new Item
		{
			ObjectID = "OID_TableSticker319",
			ItemID = ItemID.TableSticker319,
			Name = "choco for you",
			Category = Category.TableSticker,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_TableSticker320",
			ItemID = ItemID.TableSticker320,
			Name = "イッシキ建設 ロゴマーク",
			Category = Category.TableSticker,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_TableSticker321",
			ItemID = ItemID.TableSticker321,
			charaType = CharaType.Chara06,
			Name = "終焉の戯曲",
			Category = Category.TableSticker,
			Sort = SortType.Sort06
		},
		new Item
		{
			ObjectID = "OID_TableSticker322",
			ItemID = ItemID.TableSticker322,
			charaType = CharaType.Chara14,
			Name = "グリアロ命",
			Category = Category.TableSticker,
			Sort = SortType.Sort14
		},
		new Item
		{
			ObjectID = "OID_TableSticker323",
			ItemID = ItemID.TableSticker323,
			charaType = CharaType.Chara15,
			Name = "ココア命",
			Category = Category.TableSticker,
			Sort = SortType.Sort15
		},
		new Item
		{
			ObjectID = "OID_TableSticker324",
			ItemID = ItemID.TableSticker324,
			Name = "らぶはむスタンプ",
			Category = Category.TableSticker,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_TableSticker325",
			ItemID = ItemID.TableSticker325,
			charaType = CharaType.Chara20,
			Name = "制服ジュン（青）その1",
			Category = Category.TableSticker,
			Sort = SortType.Sort20
		},
		new Item
		{
			ObjectID = "OID_TableSticker326",
			ItemID = ItemID.TableSticker326,
			charaType = CharaType.Chara20,
			Name = "制服ジュン（白）その1",
			Category = Category.TableSticker,
			Sort = SortType.Sort20
		},
		new Item
		{
			ObjectID = "OID_TableSticker327",
			ItemID = ItemID.TableSticker327,
			charaType = CharaType.Chara20,
			Name = "制服ジュン（赤）その1",
			Category = Category.TableSticker,
			Sort = SortType.Sort20
		},
		new Item
		{
			ObjectID = "OID_TableSticker328",
			ItemID = ItemID.TableSticker328,
			charaType = CharaType.Chara20,
			Name = "制服ジュン（緑）その1",
			Category = Category.TableSticker,
			Sort = SortType.Sort20
		},
		new Item
		{
			ObjectID = "OID_TableSticker329",
			ItemID = ItemID.TableSticker329,
			charaType = CharaType.Chara20,
			Name = "制服ジュン（青）その2",
			Category = Category.TableSticker,
			Sort = SortType.Sort20
		},
		new Item
		{
			ObjectID = "OID_TableSticker330",
			ItemID = ItemID.TableSticker330,
			charaType = CharaType.Chara20,
			Name = "制服ジュン（白）その2",
			Category = Category.TableSticker,
			Sort = SortType.Sort20
		},
		new Item
		{
			ObjectID = "OID_TableSticker331",
			ItemID = ItemID.TableSticker331,
			charaType = CharaType.Chara20,
			Name = "制服ジュン（赤）その2",
			Category = Category.TableSticker,
			Sort = SortType.Sort20
		},
		new Item
		{
			ObjectID = "OID_TableSticker332",
			ItemID = ItemID.TableSticker332,
			charaType = CharaType.Chara20,
			Name = "制服ジュン（緑）その2",
			Category = Category.TableSticker,
			Sort = SortType.Sort20
		},
		new Item
		{
			ObjectID = "OID_TableSticker333",
			ItemID = ItemID.TableSticker333,
			charaType = CharaType.Chara09,
			Name = "パイン命",
			Category = Category.TableSticker,
			Sort = SortType.Sort09
		},
		new Item
		{
			ObjectID = "OID_TableSticker334",
			ItemID = ItemID.TableSticker334,
			charaType = CharaType.Chara07,
			Name = "トイトイ命",
			Category = Category.TableSticker,
			Sort = SortType.Sort07
		},
		new Item
		{
			ObjectID = "OID_TableSticker335",
			ItemID = ItemID.TableSticker335,
			charaType = CharaType.Chara16,
			Name = "ダイア命",
			Category = Category.TableSticker,
			Sort = SortType.Sort16
		},
		new Item
		{
			ObjectID = "OID_TableSticker336",
			ItemID = ItemID.TableSticker336,
			Name = "柏餅",
			Category = Category.TableSticker,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_TableSticker337",
			ItemID = ItemID.TableSticker337,
			Name = "こいのぼり",
			Category = Category.TableSticker,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_TableSticker338",
			ItemID = ItemID.TableSticker338,
			Name = "桜の木の下",
			Category = Category.TableSticker,
			Sort = SortType.Sort99,
			IsBigSize = true
		},
		new Item
		{
			ObjectID = "OID_TableSticker339",
			ItemID = ItemID.TableSticker339,
			charaType = CharaType.Chara11,
			Name = "カオス命",
			Category = Category.TableSticker,
			Sort = SortType.Sort11
		},
		new Item
		{
			ObjectID = "OID_TableSticker340",
			ItemID = ItemID.TableSticker340,
			charaType = CharaType.Chara12,
			Name = "クリア命",
			Category = Category.TableSticker,
			Sort = SortType.Sort12
		},
		new Item
		{
			ObjectID = "OID_TableSticker341",
			ItemID = ItemID.TableSticker341,
			charaType = CharaType.Chara10,
			Name = "詩織命",
			Category = Category.TableSticker,
			Sort = SortType.Sort10
		},
		new Item
		{
			ObjectID = "OID_TableSticker342",
			ItemID = ItemID.TableSticker342,
			Name = "玉入れ",
			Category = Category.TableSticker,
			Sort = SortType.Sort99,
			IsBigSize = true
		},
		new Item
		{
			ObjectID = "OID_TableSticker343",
			ItemID = ItemID.TableSticker343,
			Name = "かけっこダブリエル",
			Category = Category.TableSticker,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_TableSticker344",
			ItemID = ItemID.TableSticker344,
			Name = "かけっこダイアちゃん",
			Category = Category.TableSticker,
			Sort = SortType.Sort99
		},
		new Item
		{
			ObjectID = "OID_TableSticker345",
			ItemID = ItemID.TableSticker345,
			charaType = CharaType.Chara19,
			Name = "ぶっとびシロヱ",
			Category = Category.TableSticker,
			Sort = SortType.Sort19
		},
		new Item
		{
			ObjectID = "OID_TableSticker346",
			ItemID = ItemID.TableSticker346,
			charaType = CharaType.Chara20,
			Name = "ぶっとびジュン",
			Category = Category.TableSticker,
			Sort = SortType.Sort20
		},
		new Item
		{
			ObjectID = "OID_TableSticker347",
			ItemID = ItemID.TableSticker347,
			charaType = CharaType.Chara07,
			Name = "こイかイめイかイ",
			Category = Category.TableSticker,
			Sort = SortType.Sort07
		},
		new Item
		{
			ObjectID = "OID_TableSticker348",
			ItemID = ItemID.TableSticker348,
			charaType = CharaType.Chara08,
			Name = "ムサシ誕生日(2026)",
			Category = Category.TableSticker,
			Sort = SortType.Sort08
		},
		new Item
		{
			ObjectID = "OID_TableSticker349",
			ItemID = ItemID.TableSticker349,
			charaType = CharaType.Chara21,
			Name = "制服トウカ（青）その1",
			Category = Category.TableSticker,
			Sort = SortType.Sort21
		},
		new Item
		{
			ObjectID = "OID_TableSticker350",
			ItemID = ItemID.TableSticker350,
			charaType = CharaType.Chara21,
			Name = "制服トウカ（白）その1",
			Category = Category.TableSticker,
			Sort = SortType.Sort21
		},
		new Item
		{
			ObjectID = "OID_TableSticker351",
			ItemID = ItemID.TableSticker351,
			charaType = CharaType.Chara21,
			Name = "制服トウカ（赤）その1",
			Category = Category.TableSticker,
			Sort = SortType.Sort21
		},
		new Item
		{
			ObjectID = "OID_TableSticker352",
			ItemID = ItemID.TableSticker352,
			charaType = CharaType.Chara21,
			Name = "制服トウカ（緑）その1",
			Category = Category.TableSticker,
			Sort = SortType.Sort21
		},
		new Item
		{
			ObjectID = "OID_TableSticker353",
			ItemID = ItemID.TableSticker353,
			charaType = CharaType.Chara21,
			Name = "制服トウカ（青）その2",
			Category = Category.TableSticker,
			Sort = SortType.Sort21
		},
		new Item
		{
			ObjectID = "OID_TableSticker354",
			ItemID = ItemID.TableSticker354,
			charaType = CharaType.Chara21,
			Name = "制服トウカ（白）その2",
			Category = Category.TableSticker,
			Sort = SortType.Sort21
		},
		new Item
		{
			ObjectID = "OID_TableSticker355",
			ItemID = ItemID.TableSticker355,
			charaType = CharaType.Chara21,
			Name = "制服トウカ（赤）その2",
			Category = Category.TableSticker,
			Sort = SortType.Sort21
		},
		new Item
		{
			ObjectID = "OID_TableSticker356",
			ItemID = ItemID.TableSticker356,
			charaType = CharaType.Chara21,
			Name = "制服トウカ（緑）その2",
			Category = Category.TableSticker,
			Sort = SortType.Sort21
		},
		new Item
		{
			ObjectID = "OID_TableSticker357",
			ItemID = ItemID.TableSticker357,
			charaType = CharaType.Chara20,
			Name = "ジュン誕生日(2026)",
			Category = Category.TableSticker,
			Sort = SortType.Sort20
		},
		new Item
		{
			ObjectID = "OID_TableSticker358",
			ItemID = ItemID.TableSticker358,
			charaType = CharaType.Chara03,
			Name = "ツミレ誕生日(2026)",
			Category = Category.TableSticker,
			Sort = SortType.Sort03
		},
		new Item
		{
			ObjectID = "OID_TileSurface001",
			ItemID = ItemID.TileSurface001,
			Name = "麻雀ファイトガール牌",
			Category = Category.TileSurface,
			Sort = SortType.Sort01
		},
		new Item
		{
			ObjectID = "OID_TileSurface002",
			ItemID = ItemID.TileSurface002,
			Name = "麻雀格闘倶楽部牌",
			Category = Category.TileSurface,
			Sort = SortType.Sort02
		},
		new Item
		{
			ObjectID = "OID_TileSurface003",
			ItemID = ItemID.TileSurface003,
			Name = "秋色牌",
			Category = Category.TileSurface,
			Sort = SortType.Sort04
		},
		new Item
		{
			ObjectID = "OID_TileSurface004",
			ItemID = ItemID.TileSurface004,
			Name = "冬色牌",
			Category = Category.TileSurface,
			Sort = SortType.Sort04
		},
		new Item
		{
			ObjectID = "OID_TileSurface005",
			ItemID = ItemID.TileSurface005,
			Name = "春色牌",
			Category = Category.TileSurface,
			Sort = SortType.Sort03
		},
		new Item
		{
			ObjectID = "OID_TileSurface006",
			ItemID = ItemID.TileSurface006,
			Name = "1st アニバーサリー牌",
			Category = Category.TileSurface,
			Sort = SortType.Sort01
		},
		new Item
		{
			ObjectID = "OID_TileSurface007",
			ItemID = ItemID.TileSurface007,
			Name = "夏色牌",
			Category = Category.TileSurface,
			Sort = SortType.Sort03
		},
		new Item
		{
			ObjectID = "OID_TileSurface008",
			ItemID = ItemID.TileSurface008,
			Name = "ハロウィン牌",
			Category = Category.TileSurface,
			Sort = SortType.Sort05
		},
		new Item
		{
			ObjectID = "OID_TileSurface009",
			ItemID = ItemID.TileSurface009,
			Name = "ヒヨリ牌",
			Category = Category.TileSurface,
			Sort = SortType.Sort06
		},
		new Item
		{
			ObjectID = "OID_TileSurface010",
			ItemID = ItemID.TileSurface010,
			Name = "セン牌",
			Category = Category.TileSurface,
			Sort = SortType.Sort06
		},
		new Item
		{
			ObjectID = "OID_TileSurface011",
			ItemID = ItemID.TileSurface011,
			Name = "ツミレ牌",
			Category = Category.TileSurface,
			Sort = SortType.Sort06
		},
		new Item
		{
			ObjectID = "OID_TileSurface012",
			ItemID = ItemID.TileSurface012,
			Name = "テンシ牌",
			Category = Category.TileSurface,
			Sort = SortType.Sort06
		},
		new Item
		{
			ObjectID = "OID_TileSurface013",
			ItemID = ItemID.TileSurface013,
			Name = "ヤオ牌",
			Category = Category.TileSurface,
			Sort = SortType.Sort06
		},
		new Item
		{
			ObjectID = "OID_TileSurface014",
			ItemID = ItemID.TileSurface014,
			Name = "ミツバ牌",
			Category = Category.TileSurface,
			Sort = SortType.Sort06
		},
		new Item
		{
			ObjectID = "OID_TileSurface015",
			ItemID = ItemID.TileSurface015,
			Name = "トイトイ牌",
			Category = Category.TileSurface,
			Sort = SortType.Sort06
		},
		new Item
		{
			ObjectID = "OID_TileSurface016",
			ItemID = ItemID.TileSurface016,
			Name = "ムサシ牌",
			Category = Category.TileSurface,
			Sort = SortType.Sort06
		},
		new Item
		{
			ObjectID = "OID_TileSurface017",
			ItemID = ItemID.TileSurface017,
			Name = "パイン牌",
			Category = Category.TileSurface,
			Sort = SortType.Sort06
		},
		new Item
		{
			ObjectID = "OID_TileSurface018",
			ItemID = ItemID.TileSurface018,
			Name = "詩織牌",
			Category = Category.TileSurface,
			Sort = SortType.Sort06
		},
		new Item
		{
			ObjectID = "OID_TileSurface019",
			ItemID = ItemID.TileSurface019,
			Name = "カオス牌",
			Category = Category.TileSurface,
			Sort = SortType.Sort06
		},
		new Item
		{
			ObjectID = "OID_TileSurface020",
			ItemID = ItemID.TileSurface020,
			Name = "クリア牌",
			Category = Category.TileSurface,
			Sort = SortType.Sort06
		},
		new Item
		{
			ObjectID = "OID_TileSurface021",
			ItemID = ItemID.TileSurface021,
			Name = "イヨ牌",
			Category = Category.TileSurface,
			Sort = SortType.Sort06
		},
		new Item
		{
			ObjectID = "OID_TileSurface022",
			ItemID = ItemID.TileSurface022,
			Name = "グリアロ牌",
			Category = Category.TileSurface,
			Sort = SortType.Sort06
		},
		new Item
		{
			ObjectID = "OID_TileSurface023",
			ItemID = ItemID.TileSurface023,
			Name = "ココア牌",
			Category = Category.TileSurface,
			Sort = SortType.Sort06
		},
		new Item
		{
			ObjectID = "OID_TileSurface024",
			ItemID = ItemID.TileSurface024,
			Name = "ダイア牌",
			Category = Category.TileSurface,
			Sort = SortType.Sort06
		},
		new Item
		{
			ObjectID = "OID_TileSurface025",
			ItemID = ItemID.TileSurface025,
			Name = "純金牌",
			Category = Category.TileSurface,
			Sort = SortType.Sort05
		},
		new Item
		{
			ObjectID = "OID_TileSurface026",
			ItemID = ItemID.TileSurface026,
			Name = "ダブリエル牌",
			Category = Category.TileSurface,
			Sort = SortType.Sort06
		},
		new Item
		{
			ObjectID = "OID_TileSurface027",
			ItemID = ItemID.TileSurface027,
			Name = "イッパツ牌",
			Category = Category.TileSurface,
			Sort = SortType.Sort06
		},
		new Item
		{
			ObjectID = "OID_TileSurface028",
			ItemID = ItemID.TileSurface028,
			Name = "シロヱ牌",
			Category = Category.TileSurface,
			Sort = SortType.Sort06
		},
		new Item
		{
			ObjectID = "OID_TileSurface029",
			ItemID = ItemID.TileSurface029,
			Name = "ジュン牌",
			Category = Category.TileSurface,
			Sort = SortType.Sort06
		},
		new Item
		{
			ObjectID = "OID_ReachBgm001",
			ItemID = ItemID.ReachBgm001,
			Name = "ノーマルリーチBGM",
			Category = Category.ReachBgm
		},
		new Item
		{
			ObjectID = "OID_ReachBgm002",
			ItemID = ItemID.ReachBgm002,
			charaType = CharaType.Chara01,
			Name = "ダブピー♪ランデヴー / オフボーカル （ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort01
		},
		new Item
		{
			ObjectID = "OID_ReachBgm003",
			ItemID = ItemID.ReachBgm003,
			charaType = CharaType.Chara01,
			Name = "ダブピー♪ランデヴー / オフボーカル （フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort01
		},
		new Item
		{
			ObjectID = "OID_ReachBgm004",
			ItemID = ItemID.ReachBgm004,
			charaType = CharaType.Chara01,
			Name = "ダブピー♪ランデヴー / オンボーカル （ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort01
		},
		new Item
		{
			ObjectID = "OID_ReachBgm005",
			ItemID = ItemID.ReachBgm005,
			charaType = CharaType.Chara01,
			Name = "ダブピー♪ランデヴー / オンボーカル （フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort01
		},
		new Item
		{
			ObjectID = "OID_ReachBgm006",
			ItemID = ItemID.ReachBgm006,
			charaType = CharaType.Chara02,
			Name = "まっすぐ×なんばーわん！ / オフボーカル （ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort02
		},
		new Item
		{
			ObjectID = "OID_ReachBgm007",
			ItemID = ItemID.ReachBgm007,
			charaType = CharaType.Chara02,
			Name = "まっすぐ×なんばーわん！ / オフボーカル （フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort02
		},
		new Item
		{
			ObjectID = "OID_ReachBgm008",
			ItemID = ItemID.ReachBgm008,
			charaType = CharaType.Chara02,
			Name = "まっすぐ×なんばーわん！ / オンボーカル （ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort02
		},
		new Item
		{
			ObjectID = "OID_ReachBgm009",
			ItemID = ItemID.ReachBgm009,
			charaType = CharaType.Chara02,
			Name = "まっすぐ×なんばーわん！ / オンボーカル （フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort02
		},
		new Item
		{
			ObjectID = "OID_ReachBgm010",
			ItemID = ItemID.ReachBgm010,
			charaType = CharaType.Chara03,
			Name = "2個って♥ニコニコ / オフボーカル （ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort03
		},
		new Item
		{
			ObjectID = "OID_ReachBgm011",
			ItemID = ItemID.ReachBgm011,
			charaType = CharaType.Chara03,
			Name = "2個って♥ニコニコ / オフボーカル （フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort03
		},
		new Item
		{
			ObjectID = "OID_ReachBgm012",
			ItemID = ItemID.ReachBgm012,
			charaType = CharaType.Chara03,
			Name = "2個って♥ニコニコ / オンボーカル （ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort03
		},
		new Item
		{
			ObjectID = "OID_ReachBgm013",
			ItemID = ItemID.ReachBgm013,
			charaType = CharaType.Chara03,
			Name = "2個って♥ニコニコ / オンボーカル （フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort03
		},
		new Item
		{
			ObjectID = "OID_ReachBgm014",
			ItemID = ItemID.ReachBgm014,
			charaType = CharaType.Chara04,
			Name = "豪運♪天使 / オフボーカル （ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort04
		},
		new Item
		{
			ObjectID = "OID_ReachBgm015",
			ItemID = ItemID.ReachBgm015,
			charaType = CharaType.Chara04,
			Name = "豪運♪天使 / オフボーカル （フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort04
		},
		new Item
		{
			ObjectID = "OID_ReachBgm016",
			ItemID = ItemID.ReachBgm016,
			charaType = CharaType.Chara04,
			Name = "豪運♪天使 / オンボーカル （ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort04
		},
		new Item
		{
			ObjectID = "OID_ReachBgm017",
			ItemID = ItemID.ReachBgm017,
			charaType = CharaType.Chara04,
			Name = "豪運♪天使 / オンボーカル （フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort04
		},
		new Item
		{
			ObjectID = "OID_ReachBgm018",
			ItemID = ItemID.ReachBgm018,
			charaType = CharaType.Chara05,
			Name = "Speed Star / オフボーカル （ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort05
		},
		new Item
		{
			ObjectID = "OID_ReachBgm019",
			ItemID = ItemID.ReachBgm019,
			charaType = CharaType.Chara05,
			Name = "Speed Star / オフボーカル （フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort05
		},
		new Item
		{
			ObjectID = "OID_ReachBgm020",
			ItemID = ItemID.ReachBgm020,
			charaType = CharaType.Chara05,
			Name = "Speed Star / オンボーカル （ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort05
		},
		new Item
		{
			ObjectID = "OID_ReachBgm021",
			ItemID = ItemID.ReachBgm021,
			charaType = CharaType.Chara05,
			Name = "Speed Star / オンボーカル （フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort05
		},
		new Item
		{
			ObjectID = "OID_ReachBgm022",
			ItemID = ItemID.ReachBgm022,
			charaType = CharaType.Chara06,
			Name = "崇めよ！暗黒神 / オフボーカル （ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort06
		},
		new Item
		{
			ObjectID = "OID_ReachBgm023",
			ItemID = ItemID.ReachBgm023,
			charaType = CharaType.Chara06,
			Name = "崇めよ！暗黒神 / オフボーカル （フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort06
		},
		new Item
		{
			ObjectID = "OID_ReachBgm024",
			ItemID = ItemID.ReachBgm024,
			charaType = CharaType.Chara06,
			Name = "崇めよ！暗黒神 / オンボーカル （ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort06
		},
		new Item
		{
			ObjectID = "OID_ReachBgm025",
			ItemID = ItemID.ReachBgm025,
			charaType = CharaType.Chara06,
			Name = "崇めよ！暗黒神 / オンボーカル （フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort06
		},
		new Item
		{
			ObjectID = "OID_ReachBgm026",
			ItemID = ItemID.ReachBgm026,
			charaType = CharaType.Chara07,
			Name = "あイまイこイとイ / オフボーカル （ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort07
		},
		new Item
		{
			ObjectID = "OID_ReachBgm027",
			ItemID = ItemID.ReachBgm027,
			charaType = CharaType.Chara07,
			Name = "あイまイこイとイ / オフボーカル （フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort07
		},
		new Item
		{
			ObjectID = "OID_ReachBgm028",
			ItemID = ItemID.ReachBgm028,
			charaType = CharaType.Chara07,
			Name = "あイまイこイとイ / オンボーカル （ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort07
		},
		new Item
		{
			ObjectID = "OID_ReachBgm029",
			ItemID = ItemID.ReachBgm029,
			charaType = CharaType.Chara07,
			Name = "あイまイこイとイ / オンボーカル （フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort07
		},
		new Item
		{
			ObjectID = "OID_ReachBgm030",
			ItemID = ItemID.ReachBgm030,
			charaType = CharaType.Chara08,
			Name = "無双ノ暁 / オフボーカル （ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort08
		},
		new Item
		{
			ObjectID = "OID_ReachBgm031",
			ItemID = ItemID.ReachBgm031,
			charaType = CharaType.Chara08,
			Name = "無双ノ暁 / オフボーカル （フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort08
		},
		new Item
		{
			ObjectID = "OID_ReachBgm032",
			ItemID = ItemID.ReachBgm032,
			charaType = CharaType.Chara08,
			Name = "無双ノ暁 / オンボーカル （ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort08
		},
		new Item
		{
			ObjectID = "OID_ReachBgm033",
			ItemID = ItemID.ReachBgm033,
			charaType = CharaType.Chara08,
			Name = "無双ノ暁 / オンボーカル （フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort08
		},
		new Item
		{
			ObjectID = "OID_ReachBgm034",
			ItemID = ItemID.ReachBgm034,
			charaType = CharaType.Chara09,
			Name = "レッツゴー！ボンバーガール☆レインボー / オフボーカル （ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort09
		},
		new Item
		{
			ObjectID = "OID_ReachBgm035",
			ItemID = ItemID.ReachBgm035,
			charaType = CharaType.Chara09,
			Name = "レッツゴー！ボンバーガール☆レインボー / オフボーカル （フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort09
		},
		new Item
		{
			ObjectID = "OID_ReachBgm036",
			ItemID = ItemID.ReachBgm036,
			charaType = CharaType.Chara09,
			Name = "レッツゴー！ボンバーガール☆レインボー / オンボーカル （ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort09
		},
		new Item
		{
			ObjectID = "OID_ReachBgm037",
			ItemID = ItemID.ReachBgm037,
			charaType = CharaType.Chara09,
			Name = "レッツゴー！ボンバーガール☆レインボー / オンボーカル （フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort09
		},
		new Item
		{
			ObjectID = "OID_ReachBgm038",
			ItemID = ItemID.ReachBgm038,
			charaType = CharaType.Chara10,
			Name = "もっと！モット！ときめき / オフボーカル （ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort10
		},
		new Item
		{
			ObjectID = "OID_ReachBgm039",
			ItemID = ItemID.ReachBgm039,
			charaType = CharaType.Chara10,
			Name = "もっと！モット！ときめき / オフボーカル （フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort10
		},
		new Item
		{
			ObjectID = "OID_ReachBgm040",
			ItemID = ItemID.ReachBgm040,
			charaType = CharaType.Chara10,
			Name = "もっと！モット！ときめき / オンボーカル （ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort10
		},
		new Item
		{
			ObjectID = "OID_ReachBgm041",
			ItemID = ItemID.ReachBgm041,
			charaType = CharaType.Chara10,
			Name = "もっと！モット！ときめき / オンボーカル （フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort10
		},
		new Item
		{
			ObjectID = "OID_ReachBgm042",
			ItemID = ItemID.ReachBgm042,
			charaType = CharaType.Chara11,
			Name = "浄化 / オフボーカル （ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort11
		},
		new Item
		{
			ObjectID = "OID_ReachBgm043",
			ItemID = ItemID.ReachBgm043,
			charaType = CharaType.Chara11,
			Name = "浄化 / オフボーカル （フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort11
		},
		new Item
		{
			ObjectID = "OID_ReachBgm044",
			ItemID = ItemID.ReachBgm044,
			charaType = CharaType.Chara11,
			Name = "浄化 / オンボーカル （ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort11
		},
		new Item
		{
			ObjectID = "OID_ReachBgm045",
			ItemID = ItemID.ReachBgm045,
			charaType = CharaType.Chara11,
			Name = "浄化 / オンボーカル （フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort11
		},
		new Item
		{
			ObjectID = "OID_ReachBgm046",
			ItemID = ItemID.ReachBgm046,
			charaType = CharaType.Chara12,
			Name = "壊花 / オフボーカル （ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort12
		},
		new Item
		{
			ObjectID = "OID_ReachBgm047",
			ItemID = ItemID.ReachBgm047,
			charaType = CharaType.Chara12,
			Name = "壊花 / オフボーカル （フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort12
		},
		new Item
		{
			ObjectID = "OID_ReachBgm048",
			ItemID = ItemID.ReachBgm048,
			charaType = CharaType.Chara12,
			Name = "壊花 / オンボーカル （ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort12
		},
		new Item
		{
			ObjectID = "OID_ReachBgm049",
			ItemID = ItemID.ReachBgm049,
			charaType = CharaType.Chara12,
			Name = "壊花 / オンボーカル （フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort12
		},
		new Item
		{
			ObjectID = "OID_ReachBgm050",
			ItemID = ItemID.ReachBgm050,
			charaType = CharaType.Chara01,
			Name = "VVelcome!!（ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort01
		},
		new Item
		{
			ObjectID = "OID_ReachBgm051",
			ItemID = ItemID.ReachBgm051,
			charaType = CharaType.Chara01,
			Name = "VVelcome!!（フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort01
		},
		new Item
		{
			ObjectID = "OID_ReachBgm052",
			ItemID = ItemID.ReachBgm052,
			charaType = CharaType.Chara02,
			Name = "お菓子の王国（ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort02
		},
		new Item
		{
			ObjectID = "OID_ReachBgm053",
			ItemID = ItemID.ReachBgm053,
			charaType = CharaType.Chara02,
			Name = "お菓子の王国（フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort02
		},
		new Item
		{
			ObjectID = "OID_ReachBgm054",
			ItemID = ItemID.ReachBgm054,
			charaType = CharaType.Chara03,
			Name = "夏色DIARY Original rough demo version（ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort03
		},
		new Item
		{
			ObjectID = "OID_ReachBgm055",
			ItemID = ItemID.ReachBgm055,
			charaType = CharaType.Chara03,
			Name = "夏色DIARY Original rough demo version（フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort03
		},
		new Item
		{
			ObjectID = "OID_ReachBgm056",
			ItemID = ItemID.ReachBgm056,
			charaType = CharaType.Chara04,
			Name = "iLLness LiLin（ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort04
		},
		new Item
		{
			ObjectID = "OID_ReachBgm057",
			ItemID = ItemID.ReachBgm057,
			charaType = CharaType.Chara04,
			Name = "iLLness LiLin（フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort04
		},
		new Item
		{
			ObjectID = "OID_ReachBgm058",
			ItemID = ItemID.ReachBgm058,
			charaType = CharaType.Chara05,
			Name = "toy boxer（ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort05
		},
		new Item
		{
			ObjectID = "OID_ReachBgm059",
			ItemID = ItemID.ReachBgm059,
			charaType = CharaType.Chara05,
			Name = "toy boxer（フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort05
		},
		new Item
		{
			ObjectID = "OID_ReachBgm060",
			ItemID = ItemID.ReachBgm060,
			charaType = CharaType.Chara06,
			Name = "深淵に捧ぐレクイエム（ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort06
		},
		new Item
		{
			ObjectID = "OID_ReachBgm061",
			ItemID = ItemID.ReachBgm061,
			charaType = CharaType.Chara06,
			Name = "深淵に捧ぐレクイエム（フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort06
		},
		new Item
		{
			ObjectID = "OID_ReachBgm062",
			ItemID = ItemID.ReachBgm062,
			charaType = CharaType.Chara07,
			Name = "Evans（ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort07
		},
		new Item
		{
			ObjectID = "OID_ReachBgm063",
			ItemID = ItemID.ReachBgm063,
			charaType = CharaType.Chara07,
			Name = "Evans（フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort07
		},
		new Item
		{
			ObjectID = "OID_ReachBgm064",
			ItemID = ItemID.ReachBgm064,
			charaType = CharaType.Chara08,
			Name = "疾風迅雷（ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort08
		},
		new Item
		{
			ObjectID = "OID_ReachBgm065",
			ItemID = ItemID.ReachBgm065,
			charaType = CharaType.Chara08,
			Name = "疾風迅雷（フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort08
		},
		new Item
		{
			ObjectID = "OID_ReachBgm066",
			ItemID = ItemID.ReachBgm066,
			charaType = CharaType.Chara09,
			Name = "ぷろぐれっしぶ時空少女！うらしまたろ子ちゃん！（ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort09
		},
		new Item
		{
			ObjectID = "OID_ReachBgm067",
			ItemID = ItemID.ReachBgm067,
			charaType = CharaType.Chara09,
			Name = "ぷろぐれっしぶ時空少女！うらしまたろ子ちゃん！（フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort09
		},
		new Item
		{
			ObjectID = "OID_ReachBgm068",
			ItemID = ItemID.ReachBgm068,
			charaType = CharaType.Chara10,
			Name = "50th Memorial Songs -二人の時～under the cherry blossoms～-（ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort10
		},
		new Item
		{
			ObjectID = "OID_ReachBgm069",
			ItemID = ItemID.ReachBgm069,
			charaType = CharaType.Chara10,
			Name = "50th Memorial Songs -二人の時～under the cherry blossoms～-（フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort10
		},
		new Item
		{
			ObjectID = "OID_ReachBgm070",
			ItemID = ItemID.ReachBgm070,
			charaType = CharaType.Chara11,
			Name = "CHAOS Terror-Tech Mix（ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort11
		},
		new Item
		{
			ObjectID = "OID_ReachBgm071",
			ItemID = ItemID.ReachBgm071,
			charaType = CharaType.Chara11,
			Name = "CHAOS Terror-Tech Mix（フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort11
		},
		new Item
		{
			ObjectID = "OID_ReachBgm072",
			ItemID = ItemID.ReachBgm072,
			charaType = CharaType.Chara12,
			Name = "ENDYMION（ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort12
		},
		new Item
		{
			ObjectID = "OID_ReachBgm073",
			ItemID = ItemID.ReachBgm073,
			charaType = CharaType.Chara12,
			Name = "ENDYMION（フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort12
		},
		new Item
		{
			ObjectID = "OID_ReachBgm074",
			ItemID = ItemID.ReachBgm074,
			charaType = CharaType.Chara13,
			Name = "たたかえ！全緑機神 G-14！！ / オフボーカル （ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort13
		},
		new Item
		{
			ObjectID = "OID_ReachBgm075",
			ItemID = ItemID.ReachBgm075,
			charaType = CharaType.Chara13,
			Name = "たたかえ！全緑機神 G-14！！ / オフボーカル （フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort13
		},
		new Item
		{
			ObjectID = "OID_ReachBgm076",
			ItemID = ItemID.ReachBgm076,
			charaType = CharaType.Chara13,
			Name = "たたかえ！全緑機神 G-14！！ / オンボーカル （ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort13
		},
		new Item
		{
			ObjectID = "OID_ReachBgm077",
			ItemID = ItemID.ReachBgm077,
			charaType = CharaType.Chara13,
			Name = "たたかえ！全緑機神 G-14！！ / オンボーカル （フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort13
		},
		new Item
		{
			ObjectID = "OID_ReachBgm078",
			ItemID = ItemID.ReachBgm078,
			charaType = CharaType.Chara01,
			Name = "少女綺想曲 -G.X.N. Remix-（ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort01
		},
		new Item
		{
			ObjectID = "OID_ReachBgm079",
			ItemID = ItemID.ReachBgm079,
			charaType = CharaType.Chara01,
			Name = "少女綺想曲 -G.X.N. Remix-（フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort01
		},
		new Item
		{
			ObjectID = "OID_ReachBgm080",
			ItemID = ItemID.ReachBgm080,
			charaType = CharaType.Chara02,
			Name = "私の恋色。（ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort02
		},
		new Item
		{
			ObjectID = "OID_ReachBgm081",
			ItemID = ItemID.ReachBgm081,
			charaType = CharaType.Chara02,
			Name = "私の恋色。（フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort02
		},
		new Item
		{
			ObjectID = "OID_ReachBgm082",
			ItemID = ItemID.ReachBgm082,
			charaType = CharaType.Chara03,
			Name = "残像二繋ガレタ追憶ノHIDEAWAY（ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort03
		},
		new Item
		{
			ObjectID = "OID_ReachBgm083",
			ItemID = ItemID.ReachBgm083,
			charaType = CharaType.Chara03,
			Name = "残像二繋ガレタ追憶ノHIDEAWAY（フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort03
		},
		new Item
		{
			ObjectID = "OID_ReachBgm084",
			ItemID = ItemID.ReachBgm084,
			charaType = CharaType.Chara04,
			Name = "閉塞的フレーション（ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort04
		},
		new Item
		{
			ObjectID = "OID_ReachBgm085",
			ItemID = ItemID.ReachBgm085,
			charaType = CharaType.Chara04,
			Name = "閉塞的フレーション（フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort04
		},
		new Item
		{
			ObjectID = "OID_ReachBgm086",
			ItemID = ItemID.ReachBgm086,
			charaType = CharaType.Chara05,
			Name = "Mysterious Nights（ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort05
		},
		new Item
		{
			ObjectID = "OID_ReachBgm087",
			ItemID = ItemID.ReachBgm087,
			charaType = CharaType.Chara05,
			Name = "Mysterious Nights（フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort05
		},
		new Item
		{
			ObjectID = "OID_ReachBgm088",
			ItemID = ItemID.ReachBgm088,
			charaType = CharaType.Chara06,
			Name = "Scarlet Moon（ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort06
		},
		new Item
		{
			ObjectID = "OID_ReachBgm089",
			ItemID = ItemID.ReachBgm089,
			charaType = CharaType.Chara06,
			Name = "Scarlet Moon（フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort06
		},
		new Item
		{
			ObjectID = "OID_ReachBgm090",
			ItemID = ItemID.ReachBgm090,
			charaType = CharaType.Chara07,
			Name = "Russian Caravan Rhapsody（ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort07
		},
		new Item
		{
			ObjectID = "OID_ReachBgm091",
			ItemID = ItemID.ReachBgm091,
			charaType = CharaType.Chara07,
			Name = "Russian Caravan Rhapsody（フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort07
		},
		new Item
		{
			ObjectID = "OID_ReachBgm092",
			ItemID = ItemID.ReachBgm092,
			charaType = CharaType.Chara08,
			Name = "Clash of swords（ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort08
		},
		new Item
		{
			ObjectID = "OID_ReachBgm093",
			ItemID = ItemID.ReachBgm093,
			charaType = CharaType.Chara08,
			Name = "Clash of swords（フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort08
		},
		new Item
		{
			ObjectID = "OID_ReachBgm094",
			ItemID = ItemID.ReachBgm094,
			charaType = CharaType.Chara09,
			Name = "蓬莱フェスティボー（ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort09
		},
		new Item
		{
			ObjectID = "OID_ReachBgm095",
			ItemID = ItemID.ReachBgm095,
			charaType = CharaType.Chara09,
			Name = "蓬莱フェスティボー（フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort09
		},
		new Item
		{
			ObjectID = "OID_ReachBgm096",
			ItemID = ItemID.ReachBgm096,
			charaType = CharaType.Chara10,
			Name = "Princess K（ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort10
		},
		new Item
		{
			ObjectID = "OID_ReachBgm097",
			ItemID = ItemID.ReachBgm097,
			charaType = CharaType.Chara10,
			Name = "Princess K（フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort10
		},
		new Item
		{
			ObjectID = "OID_ReachBgm098",
			ItemID = ItemID.ReachBgm098,
			charaType = CharaType.Chara11,
			Name = "そして紫の幻想曲は全てを受け入れる（ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort11
		},
		new Item
		{
			ObjectID = "OID_ReachBgm099",
			ItemID = ItemID.ReachBgm099,
			charaType = CharaType.Chara11,
			Name = "そして紫の幻想曲は全てを受け入れる（フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort11
		},
		new Item
		{
			ObjectID = "OID_ReachBgm100",
			ItemID = ItemID.ReachBgm100,
			charaType = CharaType.Chara12,
			Name = "妖隠し -あやかしかくし-（ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort12
		},
		new Item
		{
			ObjectID = "OID_ReachBgm101",
			ItemID = ItemID.ReachBgm101,
			charaType = CharaType.Chara12,
			Name = "妖隠し -あやかしかくし-（フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort12
		},
		new Item
		{
			ObjectID = "OID_ReachBgm102",
			ItemID = ItemID.ReachBgm102,
			charaType = CharaType.Chara13,
			Name = "ホメ猫☆センセーション（ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort13
		},
		new Item
		{
			ObjectID = "OID_ReachBgm103",
			ItemID = ItemID.ReachBgm103,
			charaType = CharaType.Chara13,
			Name = "ホメ猫☆センセーション（フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort13
		},
		new Item
		{
			ObjectID = "OID_ReachBgm104",
			ItemID = ItemID.ReachBgm104,
			charaType = CharaType.Chara14,
			Name = "T♥Y ＃1 / オフボーカル （ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort14
		},
		new Item
		{
			ObjectID = "OID_ReachBgm105",
			ItemID = ItemID.ReachBgm105,
			charaType = CharaType.Chara14,
			Name = "T♥Y ＃1 / オフボーカル （フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort14
		},
		new Item
		{
			ObjectID = "OID_ReachBgm106",
			ItemID = ItemID.ReachBgm106,
			charaType = CharaType.Chara14,
			Name = "T♥Y ＃1 / オンボーカル （ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort14
		},
		new Item
		{
			ObjectID = "OID_ReachBgm107",
			ItemID = ItemID.ReachBgm107,
			charaType = CharaType.Chara14,
			Name = "T♥Y ＃1 / オンボーカル （フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort14
		},
		new Item
		{
			ObjectID = "OID_ReachBgm108",
			ItemID = ItemID.ReachBgm108,
			charaType = CharaType.Chara15,
			Name = "魔法少女 らぶはむ♡ココア / オフボーカル （ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort15
		},
		new Item
		{
			ObjectID = "OID_ReachBgm109",
			ItemID = ItemID.ReachBgm109,
			charaType = CharaType.Chara15,
			Name = "魔法少女 らぶはむ♡ココア / オフボーカル （フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort15
		},
		new Item
		{
			ObjectID = "OID_ReachBgm110",
			ItemID = ItemID.ReachBgm110,
			charaType = CharaType.Chara15,
			Name = "魔法少女 らぶはむ♡ココア / オンボーカル （ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort15
		},
		new Item
		{
			ObjectID = "OID_ReachBgm111",
			ItemID = ItemID.ReachBgm111,
			charaType = CharaType.Chara15,
			Name = "魔法少女 らぶはむ♡ココア / オンボーカル （フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort15
		},
		new Item
		{
			ObjectID = "OID_ReachBgm112",
			ItemID = ItemID.ReachBgm112,
			charaType = CharaType.Chara16,
			Name = "大魔法少女 らぶはむ♡ダイア / オフボーカル （ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort16
		},
		new Item
		{
			ObjectID = "OID_ReachBgm113",
			ItemID = ItemID.ReachBgm113,
			charaType = CharaType.Chara16,
			Name = "大魔法少女 らぶはむ♡ダイア / オフボーカル （フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort16
		},
		new Item
		{
			ObjectID = "OID_ReachBgm114",
			ItemID = ItemID.ReachBgm114,
			charaType = CharaType.Chara16,
			Name = "大魔法少女 らぶはむ♡ダイア / オンボーカル （ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort16
		},
		new Item
		{
			ObjectID = "OID_ReachBgm115",
			ItemID = ItemID.ReachBgm115,
			charaType = CharaType.Chara16,
			Name = "大魔法少女 らぶはむ♡ダイア / オンボーカル （フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort16
		},
		new Item
		{
			ObjectID = "OID_ReachBgm116",
			ItemID = ItemID.ReachBgm116,
			charaType = CharaType.Chara01,
			Name = "Sing a Song Sign（ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort01
		},
		new Item
		{
			ObjectID = "OID_ReachBgm117",
			ItemID = ItemID.ReachBgm117,
			charaType = CharaType.Chara01,
			Name = "Sing a Song Sign（フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort01
		},
		new Item
		{
			ObjectID = "OID_ReachBgm118",
			ItemID = ItemID.ReachBgm118,
			charaType = CharaType.Chara02,
			Name = "惑星☆ロリポップ（ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort02
		},
		new Item
		{
			ObjectID = "OID_ReachBgm119",
			ItemID = ItemID.ReachBgm119,
			charaType = CharaType.Chara02,
			Name = "惑星☆ロリポップ（フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort02
		},
		new Item
		{
			ObjectID = "OID_ReachBgm120",
			ItemID = ItemID.ReachBgm120,
			charaType = CharaType.Chara03,
			Name = "murmur twins（ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort03
		},
		new Item
		{
			ObjectID = "OID_ReachBgm121",
			ItemID = ItemID.ReachBgm121,
			charaType = CharaType.Chara03,
			Name = "murmur twins（フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort03
		},
		new Item
		{
			ObjectID = "OID_ReachBgm122",
			ItemID = ItemID.ReachBgm122,
			charaType = CharaType.Chara04,
			Name = "べィスドロップ・フリークス （ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort04
		},
		new Item
		{
			ObjectID = "OID_ReachBgm123",
			ItemID = ItemID.ReachBgm123,
			charaType = CharaType.Chara04,
			Name = "べィスドロップ・フリークス （フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort04
		},
		new Item
		{
			ObjectID = "OID_ReachBgm124",
			ItemID = ItemID.ReachBgm124,
			charaType = CharaType.Chara05,
			Name = "蒼が消えるとき（ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort05
		},
		new Item
		{
			ObjectID = "OID_ReachBgm125",
			ItemID = ItemID.ReachBgm125,
			charaType = CharaType.Chara05,
			Name = "蒼が消えるとき（フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort05
		},
		new Item
		{
			ObjectID = "OID_ReachBgm126",
			ItemID = ItemID.ReachBgm126,
			charaType = CharaType.Chara06,
			Name = "Burn Burn Burning（ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort06
		},
		new Item
		{
			ObjectID = "OID_ReachBgm127",
			ItemID = ItemID.ReachBgm127,
			charaType = CharaType.Chara06,
			Name = "Burn Burn Burning（フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort06
		},
		new Item
		{
			ObjectID = "OID_ReachBgm128",
			ItemID = ItemID.ReachBgm128,
			charaType = CharaType.Chara07,
			Name = "恋する☆宇宙戦争っ！！（ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort07
		},
		new Item
		{
			ObjectID = "OID_ReachBgm129",
			ItemID = ItemID.ReachBgm129,
			charaType = CharaType.Chara07,
			Name = "恋する☆宇宙戦争っ！！（フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort07
		},
		new Item
		{
			ObjectID = "OID_ReachBgm130",
			ItemID = ItemID.ReachBgm130,
			charaType = CharaType.Chara08,
			Name = "黒髪乱れし修羅となりて（ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort08
		},
		new Item
		{
			ObjectID = "OID_ReachBgm131",
			ItemID = ItemID.ReachBgm131,
			charaType = CharaType.Chara08,
			Name = "黒髪乱れし修羅となりて（フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort08
		},
		new Item
		{
			ObjectID = "OID_ReachBgm132",
			ItemID = ItemID.ReachBgm132,
			charaType = CharaType.Chara09,
			Name = "び（ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort09
		},
		new Item
		{
			ObjectID = "OID_ReachBgm133",
			ItemID = ItemID.ReachBgm133,
			charaType = CharaType.Chara09,
			Name = "び（フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort09
		},
		new Item
		{
			ObjectID = "OID_ReachBgm134",
			ItemID = ItemID.ReachBgm134,
			charaType = CharaType.Chara10,
			Name = "MAX LOVE（ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort10
		},
		new Item
		{
			ObjectID = "OID_ReachBgm135",
			ItemID = ItemID.ReachBgm135,
			charaType = CharaType.Chara10,
			Name = "MAX LOVE（フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort10
		},
		new Item
		{
			ObjectID = "OID_ReachBgm136",
			ItemID = ItemID.ReachBgm136,
			charaType = CharaType.Chara11,
			Name = "JOMANDA（ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort11
		},
		new Item
		{
			ObjectID = "OID_ReachBgm137",
			ItemID = ItemID.ReachBgm137,
			charaType = CharaType.Chara11,
			Name = "JOMANDA（フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort11
		},
		new Item
		{
			ObjectID = "OID_ReachBgm138",
			ItemID = ItemID.ReachBgm138,
			charaType = CharaType.Chara12,
			Name = "VALLIS-NERIA（ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort12
		},
		new Item
		{
			ObjectID = "OID_ReachBgm139",
			ItemID = ItemID.ReachBgm139,
			charaType = CharaType.Chara12,
			Name = "VALLIS-NERIA（フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort12
		},
		new Item
		{
			ObjectID = "OID_ReachBgm140",
			ItemID = ItemID.ReachBgm140,
			charaType = CharaType.Chara13,
			Name = "ポチコの幸せな日常（ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort13
		},
		new Item
		{
			ObjectID = "OID_ReachBgm141",
			ItemID = ItemID.ReachBgm141,
			charaType = CharaType.Chara13,
			Name = "ポチコの幸せな日常（フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort13
		},
		new Item
		{
			ObjectID = "OID_ReachBgm142",
			ItemID = ItemID.ReachBgm142,
			charaType = CharaType.Chara14,
			Name = "Sleepless days（ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort14
		},
		new Item
		{
			ObjectID = "OID_ReachBgm143",
			ItemID = ItemID.ReachBgm143,
			charaType = CharaType.Chara14,
			Name = "Sleepless days（フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort14
		},
		new Item
		{
			ObjectID = "OID_ReachBgm144",
			ItemID = ItemID.ReachBgm144,
			charaType = CharaType.Chara15,
			Name = "キャトられ♥恋はモ～モク（ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort15
		},
		new Item
		{
			ObjectID = "OID_ReachBgm145",
			ItemID = ItemID.ReachBgm145,
			charaType = CharaType.Chara15,
			Name = "キャトられ♥恋はモ～モク（フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort15
		},
		new Item
		{
			ObjectID = "OID_ReachBgm146",
			ItemID = ItemID.ReachBgm146,
			charaType = CharaType.Chara16,
			Name = "抱きしめてモナムール （ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort16
		},
		new Item
		{
			ObjectID = "OID_ReachBgm147",
			ItemID = ItemID.ReachBgm147,
			charaType = CharaType.Chara16,
			Name = "抱きしめてモナムール （フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort16
		},
		new Item
		{
			ObjectID = "OID_ReachBgm148",
			ItemID = ItemID.ReachBgm148,
			charaType = CharaType.Chara01,
			Name = "World of Peace / オフボーカル （ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort01
		},
		new Item
		{
			ObjectID = "OID_ReachBgm149",
			ItemID = ItemID.ReachBgm149,
			charaType = CharaType.Chara01,
			Name = "World of Peace / オフボーカル （フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort01
		},
		new Item
		{
			ObjectID = "OID_ReachBgm150",
			ItemID = ItemID.ReachBgm150,
			charaType = CharaType.Chara01,
			Name = "World of Peace / オンボーカル （ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort01
		},
		new Item
		{
			ObjectID = "OID_ReachBgm151",
			ItemID = ItemID.ReachBgm151,
			charaType = CharaType.Chara01,
			Name = "World of Peace / オンボーカル （フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort01
		},
		new Item
		{
			ObjectID = "OID_ReachBgm152",
			ItemID = ItemID.ReachBgm152,
			charaType = CharaType.Chara02,
			Name = "Reach for … / オフボーカル （ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort02
		},
		new Item
		{
			ObjectID = "OID_ReachBgm153",
			ItemID = ItemID.ReachBgm153,
			charaType = CharaType.Chara02,
			Name = "Reach for … / オフボーカル （フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort02
		},
		new Item
		{
			ObjectID = "OID_ReachBgm154",
			ItemID = ItemID.ReachBgm154,
			charaType = CharaType.Chara02,
			Name = "Reach for … / オンボーカル （ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort02
		},
		new Item
		{
			ObjectID = "OID_ReachBgm155",
			ItemID = ItemID.ReachBgm155,
			charaType = CharaType.Chara02,
			Name = "Reach for … / オンボーカル （フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort02
		},
		new Item
		{
			ObjectID = "OID_ReachBgm156",
			ItemID = ItemID.ReachBgm156,
			charaType = CharaType.Chara17,
			Name = "堕天比翼のリアライズ / オフボーカル （ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort17
		},
		new Item
		{
			ObjectID = "OID_ReachBgm157",
			ItemID = ItemID.ReachBgm157,
			charaType = CharaType.Chara17,
			Name = "堕天比翼のリアライズ / オフボーカル （フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort17
		},
		new Item
		{
			ObjectID = "OID_ReachBgm158",
			ItemID = ItemID.ReachBgm158,
			charaType = CharaType.Chara17,
			Name = "堕天比翼のリアライズ / オンボーカル （ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort17
		},
		new Item
		{
			ObjectID = "OID_ReachBgm159",
			ItemID = ItemID.ReachBgm159,
			charaType = CharaType.Chara17,
			Name = "堕天比翼のリアライズ / オンボーカル （フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort17
		},
		new Item
		{
			ObjectID = "OID_ReachBgm160",
			ItemID = ItemID.ReachBgm160,
			charaType = CharaType.Chara05,
			Name = "Rice up！ / オフボーカル （ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort05
		},
		new Item
		{
			ObjectID = "OID_ReachBgm161",
			ItemID = ItemID.ReachBgm161,
			charaType = CharaType.Chara05,
			Name = "Rice up！ / オフボーカル （フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort05
		},
		new Item
		{
			ObjectID = "OID_ReachBgm162",
			ItemID = ItemID.ReachBgm162,
			charaType = CharaType.Chara05,
			Name = "Rice up！ / オンボーカル （ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort05
		},
		new Item
		{
			ObjectID = "OID_ReachBgm163",
			ItemID = ItemID.ReachBgm163,
			charaType = CharaType.Chara05,
			Name = "Rice up！ / オンボーカル （フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort05
		},
		new Item
		{
			ObjectID = "OID_ReachBgm164",
			ItemID = ItemID.ReachBgm164,
			charaType = CharaType.Chara04,
			Name = "豪運♪ロマンス / オフボーカル （ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort04
		},
		new Item
		{
			ObjectID = "OID_ReachBgm165",
			ItemID = ItemID.ReachBgm165,
			charaType = CharaType.Chara04,
			Name = "豪運♪ロマンス / オフボーカル （フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort04
		},
		new Item
		{
			ObjectID = "OID_ReachBgm166",
			ItemID = ItemID.ReachBgm166,
			charaType = CharaType.Chara04,
			Name = "豪運♪ロマンス / オンボーカル （ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort04
		},
		new Item
		{
			ObjectID = "OID_ReachBgm167",
			ItemID = ItemID.ReachBgm167,
			charaType = CharaType.Chara04,
			Name = "豪運♪ロマンス / オンボーカル （フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort04
		},
		new Item
		{
			ObjectID = "OID_ReachBgm168",
			ItemID = ItemID.ReachBgm168,
			charaType = CharaType.Chara18,
			Name = "わんか先生のマンガ描くには / オフボーカル （ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort18
		},
		new Item
		{
			ObjectID = "OID_ReachBgm169",
			ItemID = ItemID.ReachBgm169,
			charaType = CharaType.Chara18,
			Name = "わんか先生のマンガ描くには / オフボーカル （フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort18
		},
		new Item
		{
			ObjectID = "OID_ReachBgm170",
			ItemID = ItemID.ReachBgm170,
			charaType = CharaType.Chara18,
			Name = "わんか先生のマンガ描くには / オンボーカル （ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort18
		},
		new Item
		{
			ObjectID = "OID_ReachBgm171",
			ItemID = ItemID.ReachBgm171,
			charaType = CharaType.Chara18,
			Name = "わんか先生のマンガ描くには / オンボーカル （フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort18
		},
		new Item
		{
			ObjectID = "OID_ReachBgm172",
			ItemID = ItemID.ReachBgm172,
			charaType = CharaType.Chara08,
			Name = "戦七彩 いざ舞乱 / オフボーカル （ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort08
		},
		new Item
		{
			ObjectID = "OID_ReachBgm173",
			ItemID = ItemID.ReachBgm173,
			charaType = CharaType.Chara08,
			Name = "戦七彩 いざ舞乱 / オフボーカル （フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort08
		},
		new Item
		{
			ObjectID = "OID_ReachBgm174",
			ItemID = ItemID.ReachBgm174,
			charaType = CharaType.Chara08,
			Name = "戦七彩 いざ舞乱 / オンボーカル （ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort08
		},
		new Item
		{
			ObjectID = "OID_ReachBgm175",
			ItemID = ItemID.ReachBgm175,
			charaType = CharaType.Chara08,
			Name = "戦七彩 いざ舞乱 / オンボーカル （フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort08
		},
		new Item
		{
			ObjectID = "OID_ReachBgm176",
			ItemID = ItemID.ReachBgm176,
			charaType = CharaType.Chara19,
			Name = "笑顔のメモリー / オフボーカル （ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort19
		},
		new Item
		{
			ObjectID = "OID_ReachBgm177",
			ItemID = ItemID.ReachBgm177,
			charaType = CharaType.Chara19,
			Name = "笑顔のメモリー / オフボーカル （フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort19
		},
		new Item
		{
			ObjectID = "OID_ReachBgm178",
			ItemID = ItemID.ReachBgm178,
			charaType = CharaType.Chara19,
			Name = "笑顔のメモリー / オンボーカル （ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort19
		},
		new Item
		{
			ObjectID = "OID_ReachBgm179",
			ItemID = ItemID.ReachBgm179,
			charaType = CharaType.Chara19,
			Name = "笑顔のメモリー / オンボーカル （フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort19
		},
		new Item
		{
			ObjectID = "OID_ReachBgm180",
			ItemID = ItemID.ReachBgm180,
			charaType = CharaType.Chara06,
			Name = "終焉の戯曲 / オフボーカル （ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort06
		},
		new Item
		{
			ObjectID = "OID_ReachBgm181",
			ItemID = ItemID.ReachBgm181,
			charaType = CharaType.Chara06,
			Name = "終焉の戯曲 / オフボーカル （フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort06
		},
		new Item
		{
			ObjectID = "OID_ReachBgm182",
			ItemID = ItemID.ReachBgm182,
			charaType = CharaType.Chara06,
			Name = "終焉の戯曲 / オンボーカル （ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort06
		},
		new Item
		{
			ObjectID = "OID_ReachBgm183",
			ItemID = ItemID.ReachBgm183,
			charaType = CharaType.Chara06,
			Name = "終焉の戯曲 / オンボーカル （フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort06
		},
		new Item
		{
			ObjectID = "OID_ReachBgm184",
			ItemID = ItemID.ReachBgm184,
			charaType = CharaType.Chara20,
			Name = "愛師天流 亜鉈嶽悪 / オフボーカル （ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort20
		},
		new Item
		{
			ObjectID = "OID_ReachBgm185",
			ItemID = ItemID.ReachBgm185,
			charaType = CharaType.Chara20,
			Name = "愛師天流 亜鉈嶽悪 / オフボーカル （フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort20
		},
		new Item
		{
			ObjectID = "OID_ReachBgm186",
			ItemID = ItemID.ReachBgm186,
			charaType = CharaType.Chara20,
			Name = "愛師天流 亜鉈嶽悪 / オンボーカル （ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort20
		},
		new Item
		{
			ObjectID = "OID_ReachBgm187",
			ItemID = ItemID.ReachBgm187,
			charaType = CharaType.Chara20,
			Name = "愛師天流 亜鉈嶽悪 / オンボーカル （フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort20
		},
		new Item
		{
			ObjectID = "OID_ReachBgm188",
			ItemID = ItemID.ReachBgm188,
			charaType = CharaType.Chara01,
			Name = "僕の気持ちを描く（ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort01
		},
		new Item
		{
			ObjectID = "OID_ReachBgm189",
			ItemID = ItemID.ReachBgm189,
			charaType = CharaType.Chara01,
			Name = "僕の気持ちを描く（フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort01
		},
		new Item
		{
			ObjectID = "OID_ReachBgm190",
			ItemID = ItemID.ReachBgm190,
			charaType = CharaType.Chara02,
			Name = "Cookie Bouquets（ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort02
		},
		new Item
		{
			ObjectID = "OID_ReachBgm191",
			ItemID = ItemID.ReachBgm191,
			charaType = CharaType.Chara02,
			Name = "Cookie Bouquets（フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort02
		},
		new Item
		{
			ObjectID = "OID_ReachBgm192",
			ItemID = ItemID.ReachBgm192,
			charaType = CharaType.Chara03,
			Name = "アルレシャ（ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort03
		},
		new Item
		{
			ObjectID = "OID_ReachBgm193",
			ItemID = ItemID.ReachBgm193,
			charaType = CharaType.Chara03,
			Name = "アルレシャ（フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort03
		},
		new Item
		{
			ObjectID = "OID_ReachBgm194",
			ItemID = ItemID.ReachBgm194,
			charaType = CharaType.Chara04,
			Name = "CHERNOBOG（ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort04
		},
		new Item
		{
			ObjectID = "OID_ReachBgm195",
			ItemID = ItemID.ReachBgm195,
			charaType = CharaType.Chara04,
			Name = "CHERNOBOG（フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort04
		},
		new Item
		{
			ObjectID = "OID_ReachBgm196",
			ItemID = ItemID.ReachBgm196,
			charaType = CharaType.Chara05,
			Name = "眠りの国のステラ（ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort05
		},
		new Item
		{
			ObjectID = "OID_ReachBgm197",
			ItemID = ItemID.ReachBgm197,
			charaType = CharaType.Chara05,
			Name = "眠りの国のステラ（フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort05
		},
		new Item
		{
			ObjectID = "OID_ReachBgm198",
			ItemID = ItemID.ReachBgm198,
			charaType = CharaType.Chara06,
			Name = "Hades Doll（ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort06
		},
		new Item
		{
			ObjectID = "OID_ReachBgm199",
			ItemID = ItemID.ReachBgm199,
			charaType = CharaType.Chara06,
			Name = "Hades Doll（フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort06
		},
		new Item
		{
			ObjectID = "OID_ReachBgm200",
			ItemID = ItemID.ReachBgm200,
			charaType = CharaType.Chara07,
			Name = "FLOWER（ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort07
		},
		new Item
		{
			ObjectID = "OID_ReachBgm201",
			ItemID = ItemID.ReachBgm201,
			charaType = CharaType.Chara07,
			Name = "FLOWER（フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort07
		},
		new Item
		{
			ObjectID = "OID_ReachBgm202",
			ItemID = ItemID.ReachBgm202,
			charaType = CharaType.Chara08,
			Name = "朧月覆う雲をも裂きぬ（ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort08
		},
		new Item
		{
			ObjectID = "OID_ReachBgm203",
			ItemID = ItemID.ReachBgm203,
			charaType = CharaType.Chara08,
			Name = "朧月覆う雲をも裂きぬ（フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort08
		},
		new Item
		{
			ObjectID = "OID_ReachBgm204",
			ItemID = ItemID.ReachBgm204,
			charaType = CharaType.Chara09,
			Name = "爆なな☆てすとロイヤー（ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort09
		},
		new Item
		{
			ObjectID = "OID_ReachBgm205",
			ItemID = ItemID.ReachBgm205,
			charaType = CharaType.Chara09,
			Name = "爆なな☆てすとロイヤー（フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort09
		},
		new Item
		{
			ObjectID = "OID_ReachBgm206",
			ItemID = ItemID.ReachBgm206,
			charaType = CharaType.Chara10,
			Name = "White Lovers（ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort10
		},
		new Item
		{
			ObjectID = "OID_ReachBgm207",
			ItemID = ItemID.ReachBgm207,
			charaType = CharaType.Chara10,
			Name = "White Lovers（フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort10
		},
		new Item
		{
			ObjectID = "OID_ReachBgm208",
			ItemID = ItemID.ReachBgm208,
			charaType = CharaType.Chara11,
			Name = "アモ（ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort11
		},
		new Item
		{
			ObjectID = "OID_ReachBgm209",
			ItemID = ItemID.ReachBgm209,
			charaType = CharaType.Chara11,
			Name = "アモ（フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort11
		},
		new Item
		{
			ObjectID = "OID_ReachBgm210",
			ItemID = ItemID.ReachBgm210,
			charaType = CharaType.Chara12,
			Name = "Preserved Valkyria（ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort12
		},
		new Item
		{
			ObjectID = "OID_ReachBgm211",
			ItemID = ItemID.ReachBgm211,
			charaType = CharaType.Chara12,
			Name = "Preserved Valkyria（フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort12
		},
		new Item
		{
			ObjectID = "OID_ReachBgm212",
			ItemID = ItemID.ReachBgm212,
			charaType = CharaType.Chara13,
			Name = "ALBA -黎明-（ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort13
		},
		new Item
		{
			ObjectID = "OID_ReachBgm213",
			ItemID = ItemID.ReachBgm213,
			charaType = CharaType.Chara13,
			Name = "ALBA -黎明-（フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort13
		},
		new Item
		{
			ObjectID = "OID_ReachBgm214",
			ItemID = ItemID.ReachBgm214,
			charaType = CharaType.Chara14,
			Name = "スラッシュ//シスターズ（ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort14
		},
		new Item
		{
			ObjectID = "OID_ReachBgm215",
			ItemID = ItemID.ReachBgm215,
			charaType = CharaType.Chara14,
			Name = "スラッシュ//シスターズ（フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort14
		},
		new Item
		{
			ObjectID = "OID_ReachBgm216",
			ItemID = ItemID.ReachBgm216,
			charaType = CharaType.Chara15,
			Name = "ロクブテ（ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort15
		},
		new Item
		{
			ObjectID = "OID_ReachBgm217",
			ItemID = ItemID.ReachBgm217,
			charaType = CharaType.Chara15,
			Name = "ロクブテ（フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort15
		},
		new Item
		{
			ObjectID = "OID_ReachBgm218",
			ItemID = ItemID.ReachBgm218,
			charaType = CharaType.Chara16,
			Name = "湘南ロンリーMIDNIGHT RAIN （ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort16
		},
		new Item
		{
			ObjectID = "OID_ReachBgm219",
			ItemID = ItemID.ReachBgm219,
			charaType = CharaType.Chara16,
			Name = "湘南ロンリーMIDNIGHT RAIN （フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort16
		},
		new Item
		{
			ObjectID = "OID_ReachBgm220",
			ItemID = ItemID.ReachBgm220,
			charaType = CharaType.Chara17,
			Name = "pastel@sweets labo(*'v'*) （ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort17
		},
		new Item
		{
			ObjectID = "OID_ReachBgm221",
			ItemID = ItemID.ReachBgm221,
			charaType = CharaType.Chara17,
			Name = "pastel@sweets labo(*'v'*) （フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort17
		},
		new Item
		{
			ObjectID = "OID_ReachBgm222",
			ItemID = ItemID.ReachBgm222,
			charaType = CharaType.Chara18,
			Name = "黎明スケッチブック （ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort18
		},
		new Item
		{
			ObjectID = "OID_ReachBgm223",
			ItemID = ItemID.ReachBgm223,
			charaType = CharaType.Chara18,
			Name = "黎明スケッチブック （フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort18
		},
		new Item
		{
			ObjectID = "OID_ReachBgm224",
			ItemID = ItemID.ReachBgm224,
			charaType = CharaType.Chara19,
			Name = "アマイヒミツ （ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort19
		},
		new Item
		{
			ObjectID = "OID_ReachBgm225",
			ItemID = ItemID.ReachBgm225,
			charaType = CharaType.Chara19,
			Name = "アマイヒミツ （フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort19
		},
		new Item
		{
			ObjectID = "OID_ReachBgm226",
			ItemID = ItemID.ReachBgm226,
			charaType = CharaType.Chara20,
			Name = "RAGE feat.H14 of LEONAIR （ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort20
		},
		new Item
		{
			ObjectID = "OID_ReachBgm227",
			ItemID = ItemID.ReachBgm227,
			charaType = CharaType.Chara20,
			Name = "RAGE feat.H14 of LEONAIR （フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort20
		},
		new Item
		{
			ObjectID = "OID_ReachBgm228",
			ItemID = ItemID.ReachBgm228,
			charaType = CharaType.Chara07,
			Name = "こイかイめイかイ / オフボーカル （ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort07
		},
		new Item
		{
			ObjectID = "OID_ReachBgm229",
			ItemID = ItemID.ReachBgm229,
			charaType = CharaType.Chara07,
			Name = "こイかイめイかイ / オフボーカル （フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort07
		},
		new Item
		{
			ObjectID = "OID_ReachBgm230",
			ItemID = ItemID.ReachBgm230,
			charaType = CharaType.Chara07,
			Name = "こイかイめイかイ / オンボーカル （ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort07
		},
		new Item
		{
			ObjectID = "OID_ReachBgm231",
			ItemID = ItemID.ReachBgm231,
			charaType = CharaType.Chara07,
			Name = "こイかイめイかイ / オンボーカル （フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort07
		},
		new Item
		{
			ObjectID = "OID_ReachBgm232",
			ItemID = ItemID.ReachBgm232,
			charaType = CharaType.Chara21,
			Name = "ユー久ガチ恋ちゅーイ報 / オフボーカル （ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort21
		},
		new Item
		{
			ObjectID = "OID_ReachBgm233",
			ItemID = ItemID.ReachBgm233,
			charaType = CharaType.Chara21,
			Name = "ユー久ガチ恋ちゅーイ報 / オフボーカル （フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort21
		},
		new Item
		{
			ObjectID = "OID_ReachBgm234",
			ItemID = ItemID.ReachBgm234,
			charaType = CharaType.Chara21,
			Name = "ユー久ガチ恋ちゅーイ報 / オンボーカル （ショート）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort21
		},
		new Item
		{
			ObjectID = "OID_ReachBgm235",
			ItemID = ItemID.ReachBgm235,
			charaType = CharaType.Chara21,
			Name = "ユー久ガチ恋ちゅーイ報 / オンボーカル （フル）",
			Category = Category.ReachBgm,
			Sort = SortType.Sort21
		},
		new Item
		{
			ObjectID = "OID_MyRoomBgm001",
			ItemID = ItemID.MyRoomBgm001,
			Name = "ホーム画面",
			Category = Category.MyRoomBgm,
			Sort = SortType.Sort01
		},
		new Item
		{
			ObjectID = "OID_MyRoomBgm002",
			ItemID = ItemID.MyRoomBgm002,
			Name = "ストーリー その１",
			Category = Category.MyRoomBgm,
			Sort = SortType.Sort01
		},
		new Item
		{
			ObjectID = "OID_MyRoomBgm003",
			ItemID = ItemID.MyRoomBgm003,
			Name = "ストーリー その２",
			Category = Category.MyRoomBgm,
			Sort = SortType.Sort01
		},
		new Item
		{
			ObjectID = "OID_MyRoomBgm004",
			ItemID = ItemID.MyRoomBgm004,
			Name = "じゃんジャン雀ピン Fu～♥ / オフボーカル",
			Category = Category.MyRoomBgm,
			Sort = SortType.Sort01
		},
		new Item
		{
			ObjectID = "OID_MyRoomBgm005",
			ItemID = ItemID.MyRoomBgm005,
			Name = "じゃんジャン雀ピン Fu～♥ / オンボーカル",
			Category = Category.MyRoomBgm,
			Sort = SortType.Sort01
		},
		new Item
		{
			ObjectID = "OID_MyRoomBgm006",
			ItemID = ItemID.MyRoomBgm006,
			Name = "パーフェクトイーター",
			Category = Category.MyRoomBgm,
			Sort = SortType.Sort01
		},
		new Item
		{
			ObjectID = "OID_MyRoomBgm007",
			ItemID = ItemID.MyRoomBgm007,
			Name = "クリスマスのテーマ",
			Category = Category.MyRoomBgm,
			Sort = SortType.Sort01
		},
		new Item
		{
			ObjectID = "OID_MyRoomBgm008",
			ItemID = ItemID.MyRoomBgm008,
			Name = "正月のテーマ",
			Category = Category.MyRoomBgm,
			Sort = SortType.Sort01
		},
		new Item
		{
			ObjectID = "OID_MyRoomBgm009",
			ItemID = ItemID.MyRoomBgm009,
			charaType = CharaType.Chara01,
			Name = "ダブピー♪ランデヴー / カラオケ",
			Category = Category.MyRoomBgm,
			Sort = SortType.Sort02
		},
		new Item
		{
			ObjectID = "OID_MyRoomBgm010",
			ItemID = ItemID.MyRoomBgm010,
			charaType = CharaType.Chara02,
			Name = "まっすぐ×なんばーわん！ / カラオケ",
			Category = Category.MyRoomBgm,
			Sort = SortType.Sort02
		},
		new Item
		{
			ObjectID = "OID_MyRoomBgm011",
			ItemID = ItemID.MyRoomBgm011,
			charaType = CharaType.Chara03,
			Name = "2個って♥ニコニコ / カラオケ",
			Category = Category.MyRoomBgm,
			Sort = SortType.Sort02
		},
		new Item
		{
			ObjectID = "OID_MyRoomBgm012",
			ItemID = ItemID.MyRoomBgm012,
			charaType = CharaType.Chara04,
			Name = "豪運♪天使 / カラオケ",
			Category = Category.MyRoomBgm,
			Sort = SortType.Sort02
		},
		new Item
		{
			ObjectID = "OID_MyRoomBgm013",
			ItemID = ItemID.MyRoomBgm013,
			charaType = CharaType.Chara05,
			Name = "Speed Star / カラオケ",
			Category = Category.MyRoomBgm,
			Sort = SortType.Sort02
		},
		new Item
		{
			ObjectID = "OID_MyRoomBgm014",
			ItemID = ItemID.MyRoomBgm014,
			charaType = CharaType.Chara06,
			Name = "崇めよ！暗黒神 / カラオケ",
			Category = Category.MyRoomBgm,
			Sort = SortType.Sort02
		},
		new Item
		{
			ObjectID = "OID_MyRoomBgm015",
			ItemID = ItemID.MyRoomBgm015,
			charaType = CharaType.Chara07,
			Name = "あイまイこイとイ / カラオケ",
			Category = Category.MyRoomBgm,
			Sort = SortType.Sort02
		},
		new Item
		{
			ObjectID = "OID_MyRoomBgm016",
			ItemID = ItemID.MyRoomBgm016,
			charaType = CharaType.Chara08,
			Name = "無双ノ暁 / カラオケ",
			Category = Category.MyRoomBgm,
			Sort = SortType.Sort02
		},
		new Item
		{
			ObjectID = "OID_MyRoomBgm017",
			ItemID = ItemID.MyRoomBgm017,
			charaType = CharaType.Chara09,
			Name = "レッツゴー！ボンバーガール☆レインボー / カラオケ",
			Category = Category.MyRoomBgm,
			Sort = SortType.Sort02
		},
		new Item
		{
			ObjectID = "OID_MyRoomBgm018",
			ItemID = ItemID.MyRoomBgm018,
			charaType = CharaType.Chara10,
			Name = "もっと！モット！ときめき / カラオケ",
			Category = Category.MyRoomBgm,
			Sort = SortType.Sort02
		},
		new Item
		{
			ObjectID = "OID_MyRoomBgm019",
			ItemID = ItemID.MyRoomBgm019,
			charaType = CharaType.Chara11,
			Name = "浄化 / カラオケ",
			Category = Category.MyRoomBgm,
			Sort = SortType.Sort02
		},
		new Item
		{
			ObjectID = "OID_MyRoomBgm020",
			ItemID = ItemID.MyRoomBgm020,
			charaType = CharaType.Chara12,
			Name = "壊花 / カラオケ",
			Category = Category.MyRoomBgm,
			Sort = SortType.Sort02
		},
		new Item
		{
			ObjectID = "OID_MyRoomBgm021",
			ItemID = ItemID.MyRoomBgm021,
			charaType = CharaType.Chara01,
			Name = "VVelcome!!",
			Category = Category.MyRoomBgm,
			Sort = SortType.Sort03
		},
		new Item
		{
			ObjectID = "OID_MyRoomBgm022",
			ItemID = ItemID.MyRoomBgm022,
			charaType = CharaType.Chara02,
			Name = "お菓子の王国",
			Category = Category.MyRoomBgm,
			Sort = SortType.Sort03
		},
		new Item
		{
			ObjectID = "OID_MyRoomBgm023",
			ItemID = ItemID.MyRoomBgm023,
			charaType = CharaType.Chara03,
			Name = "夏色DIARY Original rough demo version",
			Category = Category.MyRoomBgm,
			Sort = SortType.Sort03
		},
		new Item
		{
			ObjectID = "OID_MyRoomBgm024",
			ItemID = ItemID.MyRoomBgm024,
			charaType = CharaType.Chara04,
			Name = "iLLness LiLin",
			Category = Category.MyRoomBgm,
			Sort = SortType.Sort03
		},
		new Item
		{
			ObjectID = "OID_MyRoomBgm025",
			ItemID = ItemID.MyRoomBgm025,
			charaType = CharaType.Chara05,
			Name = "toy boxer",
			Category = Category.MyRoomBgm,
			Sort = SortType.Sort03
		},
		new Item
		{
			ObjectID = "OID_MyRoomBgm026",
			ItemID = ItemID.MyRoomBgm026,
			charaType = CharaType.Chara06,
			Name = "深淵に捧ぐレクイエム",
			Category = Category.MyRoomBgm,
			Sort = SortType.Sort03
		},
		new Item
		{
			ObjectID = "OID_MyRoomBgm027",
			ItemID = ItemID.MyRoomBgm027,
			charaType = CharaType.Chara07,
			Name = "Evans",
			Category = Category.MyRoomBgm,
			Sort = SortType.Sort03
		},
		new Item
		{
			ObjectID = "OID_MyRoomBgm028",
			ItemID = ItemID.MyRoomBgm028,
			charaType = CharaType.Chara08,
			Name = "疾風迅雷",
			Category = Category.MyRoomBgm,
			Sort = SortType.Sort03
		},
		new Item
		{
			ObjectID = "OID_MyRoomBgm029",
			ItemID = ItemID.MyRoomBgm029,
			charaType = CharaType.Chara09,
			Name = "ぷろぐれっしぶ時空少女！うらしまたろ子ちゃん！",
			Category = Category.MyRoomBgm,
			Sort = SortType.Sort03
		},
		new Item
		{
			ObjectID = "OID_MyRoomBgm030",
			ItemID = ItemID.MyRoomBgm030,
			charaType = CharaType.Chara10,
			Name = "50th Memorial Songs -二人の時～under the cherry blossoms～-",
			Category = Category.MyRoomBgm,
			Sort = SortType.Sort03
		},
		new Item
		{
			ObjectID = "OID_MyRoomBgm031",
			ItemID = ItemID.MyRoomBgm031,
			charaType = CharaType.Chara11,
			Name = "CHAOS Terror-Tech Mix",
			Category = Category.MyRoomBgm,
			Sort = SortType.Sort03
		},
		new Item
		{
			ObjectID = "OID_MyRoomBgm032",
			ItemID = ItemID.MyRoomBgm032,
			charaType = CharaType.Chara12,
			Name = "ENDYMION",
			Category = Category.MyRoomBgm,
			Sort = SortType.Sort03
		},
		new Item
		{
			ObjectID = "OID_MyRoomBgm033",
			ItemID = ItemID.MyRoomBgm033,
			Name = "弾幕信仰",
			Category = Category.MyRoomBgm,
			Sort = SortType.Sort04
		},
		new Item
		{
			ObjectID = "OID_MyRoomBgm034",
			ItemID = ItemID.MyRoomBgm034,
			Name = "SUPER HEROINE!!",
			Category = Category.MyRoomBgm,
			Sort = SortType.Sort04
		},
		new Item
		{
			ObjectID = "OID_MyRoomBgm035",
			ItemID = ItemID.MyRoomBgm035,
			charaType = CharaType.Chara01,
			Name = "少女綺想曲 -G.X.N. Remix-",
			Category = Category.MyRoomBgm,
			Sort = SortType.Sort04
		},
		new Item
		{
			ObjectID = "OID_MyRoomBgm036",
			ItemID = ItemID.MyRoomBgm036,
			charaType = CharaType.Chara02,
			Name = "私の恋色。",
			Category = Category.MyRoomBgm,
			Sort = SortType.Sort04
		},
		new Item
		{
			ObjectID = "OID_MyRoomBgm037",
			ItemID = ItemID.MyRoomBgm037,
			charaType = CharaType.Chara03,
			Name = "残像二繋ガレタ追憶ノHIDEAWAY",
			Category = Category.MyRoomBgm,
			Sort = SortType.Sort04
		},
		new Item
		{
			ObjectID = "OID_MyRoomBgm038",
			ItemID = ItemID.MyRoomBgm038,
			charaType = CharaType.Chara04,
			Name = "閉塞的フレーション",
			Category = Category.MyRoomBgm,
			Sort = SortType.Sort04
		},
		new Item
		{
			ObjectID = "OID_MyRoomBgm039",
			ItemID = ItemID.MyRoomBgm039,
			charaType = CharaType.Chara05,
			Name = "Mysterious Nights",
			Category = Category.MyRoomBgm,
			Sort = SortType.Sort04
		},
		new Item
		{
			ObjectID = "OID_MyRoomBgm040",
			ItemID = ItemID.MyRoomBgm040,
			charaType = CharaType.Chara06,
			Name = "Scarlet Moon",
			Category = Category.MyRoomBgm,
			Sort = SortType.Sort04
		},
		new Item
		{
			ObjectID = "OID_MyRoomBgm041",
			ItemID = ItemID.MyRoomBgm041,
			charaType = CharaType.Chara07,
			Name = "Russian Caravan Rhapsody",
			Category = Category.MyRoomBgm,
			Sort = SortType.Sort04
		},
		new Item
		{
			ObjectID = "OID_MyRoomBgm042",
			ItemID = ItemID.MyRoomBgm042,
			charaType = CharaType.Chara08,
			Name = "Clash of swords",
			Category = Category.MyRoomBgm,
			Sort = SortType.Sort04
		},
		new Item
		{
			ObjectID = "OID_MyRoomBgm043",
			ItemID = ItemID.MyRoomBgm043,
			charaType = CharaType.Chara09,
			Name = "蓬莱フェスティボー",
			Category = Category.MyRoomBgm,
			Sort = SortType.Sort04
		},
		new Item
		{
			ObjectID = "OID_MyRoomBgm044",
			ItemID = ItemID.MyRoomBgm044,
			charaType = CharaType.Chara10,
			Name = "Princess K",
			Category = Category.MyRoomBgm,
			Sort = SortType.Sort04
		},
		new Item
		{
			ObjectID = "OID_MyRoomBgm045",
			ItemID = ItemID.MyRoomBgm045,
			charaType = CharaType.Chara11,
			Name = "そして紫の幻想曲は全てを受け入れる",
			Category = Category.MyRoomBgm,
			Sort = SortType.Sort04
		},
		new Item
		{
			ObjectID = "OID_MyRoomBgm046",
			ItemID = ItemID.MyRoomBgm046,
			charaType = CharaType.Chara12,
			Name = "妖隠し -あやかしかくし-",
			Category = Category.MyRoomBgm,
			Sort = SortType.Sort04
		},
		new Item
		{
			ObjectID = "OID_MyRoomBgm047",
			ItemID = ItemID.MyRoomBgm047,
			charaType = CharaType.Chara13,
			Name = "ホメ猫☆センセーション",
			Category = Category.MyRoomBgm,
			Sort = SortType.Sort04
		},
		new Item
		{
			ObjectID = "OID_MyRoomBgm048",
			ItemID = ItemID.MyRoomBgm048,
			charaType = CharaType.Chara01,
			Name = "Sing a Song Sign",
			Category = Category.MyRoomBgm,
			Sort = SortType.Sort05
		},
		new Item
		{
			ObjectID = "OID_MyRoomBgm049",
			ItemID = ItemID.MyRoomBgm049,
			charaType = CharaType.Chara02,
			Name = "惑星☆ロリポップ",
			Category = Category.MyRoomBgm,
			Sort = SortType.Sort05
		},
		new Item
		{
			ObjectID = "OID_MyRoomBgm050",
			ItemID = ItemID.MyRoomBgm050,
			charaType = CharaType.Chara03,
			Name = "murmur twins",
			Category = Category.MyRoomBgm,
			Sort = SortType.Sort05
		},
		new Item
		{
			ObjectID = "OID_MyRoomBgm051",
			ItemID = ItemID.MyRoomBgm051,
			charaType = CharaType.Chara04,
			Name = "べィスドロップ・フリークス ",
			Category = Category.MyRoomBgm,
			Sort = SortType.Sort05
		},
		new Item
		{
			ObjectID = "OID_MyRoomBgm052",
			ItemID = ItemID.MyRoomBgm052,
			charaType = CharaType.Chara05,
			Name = "蒼が消えるとき",
			Category = Category.MyRoomBgm,
			Sort = SortType.Sort05
		},
		new Item
		{
			ObjectID = "OID_MyRoomBgm053",
			ItemID = ItemID.MyRoomBgm053,
			charaType = CharaType.Chara06,
			Name = "Burn Burn Burning",
			Category = Category.MyRoomBgm,
			Sort = SortType.Sort05
		},
		new Item
		{
			ObjectID = "OID_MyRoomBgm054",
			ItemID = ItemID.MyRoomBgm054,
			charaType = CharaType.Chara07,
			Name = "恋する☆宇宙戦争っ！！",
			Category = Category.MyRoomBgm,
			Sort = SortType.Sort05
		},
		new Item
		{
			ObjectID = "OID_MyRoomBgm055",
			ItemID = ItemID.MyRoomBgm055,
			charaType = CharaType.Chara08,
			Name = "黒髪乱れし修羅となりて",
			Category = Category.MyRoomBgm,
			Sort = SortType.Sort05
		},
		new Item
		{
			ObjectID = "OID_MyRoomBgm056",
			ItemID = ItemID.MyRoomBgm056,
			charaType = CharaType.Chara09,
			Name = "び",
			Category = Category.MyRoomBgm,
			Sort = SortType.Sort05
		},
		new Item
		{
			ObjectID = "OID_MyRoomBgm057",
			ItemID = ItemID.MyRoomBgm057,
			charaType = CharaType.Chara10,
			Name = "MAX LOVE",
			Category = Category.MyRoomBgm,
			Sort = SortType.Sort05
		},
		new Item
		{
			ObjectID = "OID_MyRoomBgm058",
			ItemID = ItemID.MyRoomBgm058,
			charaType = CharaType.Chara11,
			Name = "JOMANDA",
			Category = Category.MyRoomBgm,
			Sort = SortType.Sort05
		},
		new Item
		{
			ObjectID = "OID_MyRoomBgm059",
			ItemID = ItemID.MyRoomBgm059,
			charaType = CharaType.Chara12,
			Name = "VALLIS-NERIA",
			Category = Category.MyRoomBgm,
			Sort = SortType.Sort05
		},
		new Item
		{
			ObjectID = "OID_MyRoomBgm060",
			ItemID = ItemID.MyRoomBgm060,
			charaType = CharaType.Chara13,
			Name = "ポチコの幸せな日常",
			Category = Category.MyRoomBgm,
			Sort = SortType.Sort05
		},
		new Item
		{
			ObjectID = "OID_MyRoomBgm061",
			ItemID = ItemID.MyRoomBgm061,
			charaType = CharaType.Chara14,
			Name = "Sleepless days",
			Category = Category.MyRoomBgm,
			Sort = SortType.Sort05
		},
		new Item
		{
			ObjectID = "OID_MyRoomBgm062",
			ItemID = ItemID.MyRoomBgm062,
			charaType = CharaType.Chara15,
			Name = "キャトられ♥恋はモ～モク",
			Category = Category.MyRoomBgm,
			Sort = SortType.Sort05
		},
		new Item
		{
			ObjectID = "OID_MyRoomBgm063",
			ItemID = ItemID.MyRoomBgm063,
			charaType = CharaType.Chara16,
			Name = "抱きしめてモナムール ",
			Category = Category.MyRoomBgm,
			Sort = SortType.Sort05
		},
		new Item
		{
			ObjectID = "OID_MyRoomBgm064",
			ItemID = ItemID.MyRoomBgm064,
			Name = "キャラクターセレクト",
			Category = Category.MyRoomBgm,
			Sort = SortType.Sort06
		},
		new Item
		{
			ObjectID = "OID_MyRoomBgm065",
			ItemID = ItemID.MyRoomBgm065,
			Name = "Brilliant Destiny",
			Category = Category.MyRoomBgm,
			Sort = SortType.Sort06
		},
		new Item
		{
			ObjectID = "OID_MyRoomBgm066",
			ItemID = ItemID.MyRoomBgm066,
			charaType = CharaType.Chara01,
			Name = "僕の気持ちを描く",
			Category = Category.MyRoomBgm,
			Sort = SortType.Sort07
		},
		new Item
		{
			ObjectID = "OID_MyRoomBgm067",
			ItemID = ItemID.MyRoomBgm067,
			charaType = CharaType.Chara02,
			Name = "Cookie Bouquets",
			Category = Category.MyRoomBgm,
			Sort = SortType.Sort07
		},
		new Item
		{
			ObjectID = "OID_MyRoomBgm068",
			ItemID = ItemID.MyRoomBgm068,
			charaType = CharaType.Chara03,
			Name = "アルレシャ",
			Category = Category.MyRoomBgm,
			Sort = SortType.Sort07
		},
		new Item
		{
			ObjectID = "OID_MyRoomBgm069",
			ItemID = ItemID.MyRoomBgm069,
			charaType = CharaType.Chara04,
			Name = "CHERNOBOG",
			Category = Category.MyRoomBgm,
			Sort = SortType.Sort07
		},
		new Item
		{
			ObjectID = "OID_MyRoomBgm070",
			ItemID = ItemID.MyRoomBgm070,
			charaType = CharaType.Chara05,
			Name = "眠りの国のステラ",
			Category = Category.MyRoomBgm,
			Sort = SortType.Sort07
		},
		new Item
		{
			ObjectID = "OID_MyRoomBgm071",
			ItemID = ItemID.MyRoomBgm071,
			charaType = CharaType.Chara06,
			Name = "Hades Doll",
			Category = Category.MyRoomBgm,
			Sort = SortType.Sort07
		},
		new Item
		{
			ObjectID = "OID_MyRoomBgm072",
			ItemID = ItemID.MyRoomBgm072,
			charaType = CharaType.Chara07,
			Name = "FLOWER",
			Category = Category.MyRoomBgm,
			Sort = SortType.Sort07
		},
		new Item
		{
			ObjectID = "OID_MyRoomBgm073",
			ItemID = ItemID.MyRoomBgm073,
			charaType = CharaType.Chara08,
			Name = "朧月覆う雲をも裂きぬ",
			Category = Category.MyRoomBgm,
			Sort = SortType.Sort07
		},
		new Item
		{
			ObjectID = "OID_MyRoomBgm074",
			ItemID = ItemID.MyRoomBgm074,
			charaType = CharaType.Chara09,
			Name = "爆なな☆てすとロイヤー",
			Category = Category.MyRoomBgm,
			Sort = SortType.Sort07
		},
		new Item
		{
			ObjectID = "OID_MyRoomBgm075",
			ItemID = ItemID.MyRoomBgm075,
			charaType = CharaType.Chara10,
			Name = "White Lovers",
			Category = Category.MyRoomBgm,
			Sort = SortType.Sort07
		},
		new Item
		{
			ObjectID = "OID_MyRoomBgm076",
			ItemID = ItemID.MyRoomBgm076,
			charaType = CharaType.Chara11,
			Name = "アモ",
			Category = Category.MyRoomBgm,
			Sort = SortType.Sort07
		},
		new Item
		{
			ObjectID = "OID_MyRoomBgm077",
			ItemID = ItemID.MyRoomBgm077,
			charaType = CharaType.Chara12,
			Name = "Preserved Valkyria",
			Category = Category.MyRoomBgm,
			Sort = SortType.Sort07
		},
		new Item
		{
			ObjectID = "OID_MyRoomBgm078",
			ItemID = ItemID.MyRoomBgm078,
			charaType = CharaType.Chara13,
			Name = "ALBA -黎明-",
			Category = Category.MyRoomBgm,
			Sort = SortType.Sort07
		},
		new Item
		{
			ObjectID = "OID_MyRoomBgm079",
			ItemID = ItemID.MyRoomBgm079,
			charaType = CharaType.Chara14,
			Name = "スラッシュ//シスターズ",
			Category = Category.MyRoomBgm,
			Sort = SortType.Sort07
		},
		new Item
		{
			ObjectID = "OID_MyRoomBgm080",
			ItemID = ItemID.MyRoomBgm080,
			charaType = CharaType.Chara15,
			Name = "ロクブテ",
			Category = Category.MyRoomBgm,
			Sort = SortType.Sort07
		},
		new Item
		{
			ObjectID = "OID_MyRoomBgm081",
			ItemID = ItemID.MyRoomBgm081,
			charaType = CharaType.Chara16,
			Name = "湘南ロンリーMIDNIGHT RAIN ",
			Category = Category.MyRoomBgm,
			Sort = SortType.Sort07
		},
		new Item
		{
			ObjectID = "OID_MyRoomBgm082",
			ItemID = ItemID.MyRoomBgm082,
			charaType = CharaType.Chara17,
			Name = "pastel@sweets labo(*'v'*) ",
			Category = Category.MyRoomBgm,
			Sort = SortType.Sort07
		},
		new Item
		{
			ObjectID = "OID_MyRoomBgm083",
			ItemID = ItemID.MyRoomBgm083,
			charaType = CharaType.Chara18,
			Name = "黎明スケッチブック ",
			Category = Category.MyRoomBgm,
			Sort = SortType.Sort07
		},
		new Item
		{
			ObjectID = "OID_MyRoomBgm084",
			ItemID = ItemID.MyRoomBgm084,
			charaType = CharaType.Chara19,
			Name = "アマイヒミツ ",
			Category = Category.MyRoomBgm,
			Sort = SortType.Sort07
		},
		new Item
		{
			ObjectID = "OID_MyRoomBgm085",
			ItemID = ItemID.MyRoomBgm085,
			charaType = CharaType.Chara20,
			Name = "RAGE feat.H14 of LEONAIR ",
			Category = Category.MyRoomBgm,
			Sort = SortType.Sort07
		},
		new Item
		{
			ObjectID = "OID_StageBgNone",
			ItemID = ItemID.StageBgNone,
			Name = "おまかせ",
			Category = Category.StageBg,
			Sort = SortType.Sort01
		},
		new Item
		{
			ObjectID = "OID_StageBg001",
			ItemID = ItemID.StageBg001,
			Name = "スタジアム",
			Category = Category.StageBg,
			Sort = SortType.Sort01
		},
		new Item
		{
			ObjectID = "OID_StageBg002",
			ItemID = ItemID.StageBg002,
			Name = "お花見",
			Category = Category.StageBg,
			Sort = SortType.Sort01
		},
		new Item
		{
			ObjectID = "OID_StageBg003",
			ItemID = ItemID.StageBg003,
			Name = "プール",
			Category = Category.StageBg,
			Sort = SortType.Sort01
		},
		new Item
		{
			ObjectID = "OID_StageBg004",
			ItemID = ItemID.StageBg004,
			Name = "紅葉狩り",
			Category = Category.StageBg,
			Sort = SortType.Sort01
		}
	};

	public static Item GetItem(ItemID itemID)
	{
		return m_items.Find((Item item) => item.ItemID == itemID);
	}

	public static Item GetItem(string oid)
	{
		return m_items.Find((Item a) => a.ObjectID == oid);
	}

	public static ItemID ToItemID(string oid)
	{
		return m_items.Find((Item _item) => _item.ObjectID == oid)?.ItemID ?? ItemID.None;
	}

	public static string ToObjectID(ItemID itemID)
	{
		Item item = m_items.Find((Item _item) => _item.ItemID == itemID);
		if (item == null)
		{
			return "";
		}
		return item.ObjectID;
	}

	public static List<Item> GetItems(Predicate<Item> match)
	{
		return m_items.FindAll(match);
	}

	public static ItemID GetDefaultEquipItemID(Category category)
	{
		return m_defaultEquipItems[category];
	}

	public static List<ItemID> GetDefaultItemIDs()
	{
		return m_defaultPossessionItems;
	}

	public static bool IsDefaultItemID(ItemID itemID)
	{
		return m_defaultPossessionItems.Exists((ItemID x) => x == itemID);
	}

	public static bool IsMultiEquipEnableCategory(Category category)
	{
		if (category == Category.ReachStick || category == Category.ReachBgm)
		{
			return true;
		}
		return false;
	}

	public static bool IsUpperEquipEnableCategory(Category category)
	{
		if (category == Category.TableSticker)
		{
			return true;
		}
		return false;
	}

	public static bool IsChocolateEffectReachStick(CharaType chara, ItemID itemID)
	{
		return ChocolateEffectReachStick.ContainsValue(itemID);
	}

	public static bool IsTohoMusic(ItemID itemId)
	{
		return TohoMusicList.Contains(itemId);
	}

	public static bool IsPonnomichi(ItemID itemID)
	{
		return PonnomichiList.Contains(itemID);
	}

	public static ItemID GetStageBgEquip()
	{
		try
		{
			ItemID itemID = GameDataManager.Player.GetCurrentChara().CustomizeItemEquips[Category.StageBg][0];
			if (itemID == ItemID.StageBgNone)
			{
				return s_stageBgEventDic.Where((KeyValuePair<ItemID, GameEventType> x) => GameDataManager.System.GameControlInfo.EventStatus.IsActive(x.Value))?.Max((KeyValuePair<ItemID, GameEventType> x) => x.Key) ?? ItemID.StageBg001;
			}
			return itemID;
		}
		catch
		{
		}
		return ItemID.StageBg001;
	}
}
