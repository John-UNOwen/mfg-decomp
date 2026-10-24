using System;
using System.Collections.Generic;
using System.Linq;
using MFG.GameData;
using MFG.Types;
using MFG.Types.Customize.CutinItem;

namespace MFG.MasterGameData;

public class CutinItemMaster : MasterGameDataBase
{
	public static Dictionary<CharaType, ItemID> AppearanceMovieCutins = new Dictionary<CharaType, ItemID>
	{
		{
			CharaType.Chara01,
			ItemID.Item01AgariA02
		},
		{
			CharaType.Chara02,
			ItemID.Item02AgariA02
		},
		{
			CharaType.Chara03,
			ItemID.Item03AgariA02
		},
		{
			CharaType.Chara04,
			ItemID.Item04AgariA02
		},
		{
			CharaType.Chara05,
			ItemID.Item05AgariA02
		},
		{
			CharaType.Chara06,
			ItemID.Item06AgariA02
		},
		{
			CharaType.Chara07,
			ItemID.Item07AgariA02
		},
		{
			CharaType.Chara08,
			ItemID.Item08AgariA02
		},
		{
			CharaType.Chara09,
			ItemID.Item09AgariA02
		},
		{
			CharaType.Chara10,
			ItemID.Item10AgariA02
		},
		{
			CharaType.Chara11,
			ItemID.Item11AgariA02
		},
		{
			CharaType.Chara12,
			ItemID.Item12AgariA01
		}
	};

	private static List<ItemID> _defaultItems = new List<ItemID>
	{
		ItemID.Item01NakiA01,
		ItemID.Item01ReachA01,
		ItemID.Item01AgariA01,
		ItemID.Item02NakiA01,
		ItemID.Item02ReachA01,
		ItemID.Item02AgariA01,
		ItemID.Item03NakiA01,
		ItemID.Item03ReachA01,
		ItemID.Item03AgariA01,
		ItemID.Item04NakiA01,
		ItemID.Item04ReachA01,
		ItemID.Item04AgariA01,
		ItemID.Item05NakiA01,
		ItemID.Item05ReachA01,
		ItemID.Item05AgariA01,
		ItemID.Item06NakiA01,
		ItemID.Item06ReachA01,
		ItemID.Item06AgariA01,
		ItemID.Item07NakiA01,
		ItemID.Item07ReachA01,
		ItemID.Item07AgariA01,
		ItemID.Item08NakiA01,
		ItemID.Item08ReachA01,
		ItemID.Item08AgariA01,
		ItemID.Item09NakiA01,
		ItemID.Item09ReachA01,
		ItemID.Item09AgariA01,
		ItemID.Item10NakiA01,
		ItemID.Item10ReachA01,
		ItemID.Item10AgariA01,
		ItemID.Item11NakiA01,
		ItemID.Item11ReachA01,
		ItemID.Item11AgariA01,
		ItemID.Item12NakiA01,
		ItemID.Item12ReachA01,
		ItemID.Item13NakiA01,
		ItemID.Item13ReachA01,
		ItemID.Item14NakiA01,
		ItemID.Item14ReachA01,
		ItemID.Item15NakiA01,
		ItemID.Item15ReachA01,
		ItemID.Item16NakiA01,
		ItemID.Item16ReachA01,
		ItemID.Item17NakiA01,
		ItemID.Item17ReachA01,
		ItemID.Item18NakiA01,
		ItemID.Item18ReachA01,
		ItemID.Item19NakiA01,
		ItemID.Item19ReachA01,
		ItemID.Item20NakiA01,
		ItemID.Item20ReachA01,
		ItemID.Item21NakiA01,
		ItemID.Item21ReachA01
	};

	private static List<Item> _items = new List<Item>
	{
		new Item
		{
			ObjectID = "OID_01AgariA01",
			ItemID = ItemID.Item01AgariA01,
			ShortName = "アガリ「スタンダード」",
			Category = Category.AgariSmall,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara01,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_01AgariR01",
			ItemID = ItemID.Item01AgariR01,
			ShortName = "アガリ「コミカル」",
			Category = Category.AgariSmall,
			Rarity = Rarity.R,
			CharaType = CharaType.Chara01,
			IsComical = true
		},
		new Item
		{
			ObjectID = "OID_01AgariSR01",
			ItemID = ItemID.Item01AgariSR01,
			ShortName = "アガリ「スマッシュ」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara01,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_01AgariSR02",
			ItemID = ItemID.Item01AgariSR02,
			ShortName = "アガリ「バニー」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara01,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_01AgariSR03",
			ItemID = ItemID.Item01AgariSR03,
			ShortName = "アガリ「コミカルスマッシュ」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara01,
			IsComical = true
		},
		new Item
		{
			ObjectID = "OID_01AgariUR01",
			ItemID = ItemID.Item01AgariUR01,
			ShortName = "アガリ「スペシャルアニメ 1」",
			Category = Category.AgariBig,
			Rarity = Rarity.UR,
			CharaType = CharaType.Chara01,
			IsComical = false,
			IsMovie = true
		},
		new Item
		{
			ObjectID = "OID_01NakiA01",
			ItemID = ItemID.Item01NakiA01,
			ShortName = "鳴き「プチキャラ（笑顔）」",
			Category = Category.Naki,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara01,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_01NakiN01",
			ItemID = ItemID.Item01NakiN01,
			ShortName = "鳴き「プチキャラ（ダブルピース）」",
			Category = Category.Naki,
			Rarity = Rarity.N,
			CharaType = CharaType.Chara01,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_01NakiN02",
			ItemID = ItemID.Item01NakiN02,
			ShortName = "鳴き「プチキャラ（真剣）」",
			Category = Category.Naki,
			Rarity = Rarity.N,
			CharaType = CharaType.Chara01,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_01NakiN03",
			ItemID = ItemID.Item01NakiN03,
			ShortName = "鳴き「プチキャラ（陽気）」",
			Category = Category.Naki,
			Rarity = Rarity.N,
			CharaType = CharaType.Chara01,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_01ReachA01",
			ItemID = ItemID.Item01ReachA01,
			ShortName = "リーチ「プチキャラ」",
			Category = Category.Reach,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara01,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_01ReachR01",
			ItemID = ItemID.Item01ReachR01,
			ShortName = "リーチ「スタンダード」",
			Category = Category.Reach,
			Rarity = Rarity.R,
			CharaType = CharaType.Chara01,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_01ReachR02",
			ItemID = ItemID.Item01ReachR02,
			ShortName = "リーチ「コミカル」",
			Category = Category.Reach,
			Rarity = Rarity.R,
			CharaType = CharaType.Chara01,
			IsComical = true
		},
		new Item
		{
			ObjectID = "OID_02AgariA01",
			ItemID = ItemID.Item02AgariA01,
			ShortName = "アガリ「スタンダード」",
			Category = Category.AgariSmall,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara02,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_02AgariR01",
			ItemID = ItemID.Item02AgariR01,
			ShortName = "アガリ「コミカル」",
			Category = Category.AgariSmall,
			Rarity = Rarity.R,
			CharaType = CharaType.Chara02,
			IsComical = true
		},
		new Item
		{
			ObjectID = "OID_02AgariSR01",
			ItemID = ItemID.Item02AgariSR01,
			ShortName = "アガリ「スマッシュ」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara02,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_02AgariSR02",
			ItemID = ItemID.Item02AgariSR02,
			ShortName = "アガリ「バニー」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara02,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_02AgariSR03",
			ItemID = ItemID.Item02AgariSR03,
			ShortName = "アガリ「コミカルスマッシュ」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara02,
			IsComical = true
		},
		new Item
		{
			ObjectID = "OID_02AgariSR04",
			ItemID = ItemID.Item02AgariSR04,
			ShortName = "アガリ「水着」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara02,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_02AgariUR01",
			ItemID = ItemID.Item02AgariUR01,
			ShortName = "アガリ「スペシャルアニメ 1」",
			Category = Category.AgariBig,
			Rarity = Rarity.UR,
			CharaType = CharaType.Chara02,
			IsComical = false,
			IsMovie = true
		},
		new Item
		{
			ObjectID = "OID_02NakiA01",
			ItemID = ItemID.Item02NakiA01,
			ShortName = "鳴き「プチキャラ（指差し）」",
			Category = Category.Naki,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara02,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_02NakiN01",
			ItemID = ItemID.Item02NakiN01,
			ShortName = "鳴き「プチキャラ（笑顔）」",
			Category = Category.Naki,
			Rarity = Rarity.N,
			CharaType = CharaType.Chara02,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_02NakiN02",
			ItemID = ItemID.Item02NakiN02,
			ShortName = "鳴き「プチキャラ（強気）」",
			Category = Category.Naki,
			Rarity = Rarity.N,
			CharaType = CharaType.Chara02,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_02NakiN03",
			ItemID = ItemID.Item02NakiN03,
			ShortName = "鳴き「プチキャラ（不安気）」",
			Category = Category.Naki,
			Rarity = Rarity.N,
			CharaType = CharaType.Chara02,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_02ReachA01",
			ItemID = ItemID.Item02ReachA01,
			ShortName = "リーチ「プチキャラ」",
			Category = Category.Reach,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara02,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_02ReachR01",
			ItemID = ItemID.Item02ReachR01,
			ShortName = "リーチ「スタンダード」",
			Category = Category.Reach,
			Rarity = Rarity.R,
			CharaType = CharaType.Chara02,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_02ReachR02",
			ItemID = ItemID.Item02ReachR02,
			ShortName = "リーチ「コミカル」",
			Category = Category.Reach,
			Rarity = Rarity.R,
			CharaType = CharaType.Chara02,
			IsComical = true
		},
		new Item
		{
			ObjectID = "OID_03AgariA01",
			ItemID = ItemID.Item03AgariA01,
			ShortName = "アガリ「スタンダード」",
			Category = Category.AgariSmall,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara03,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_03AgariR01",
			ItemID = ItemID.Item03AgariR01,
			ShortName = "アガリ「コミカル」",
			Category = Category.AgariSmall,
			Rarity = Rarity.R,
			CharaType = CharaType.Chara03,
			IsComical = true
		},
		new Item
		{
			ObjectID = "OID_03AgariSR01",
			ItemID = ItemID.Item03AgariSR01,
			ShortName = "アガリ「スマッシュ」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara03,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_03AgariSR02",
			ItemID = ItemID.Item03AgariSR02,
			ShortName = "アガリ「バニー」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara03,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_03AgariSR03",
			ItemID = ItemID.Item03AgariSR03,
			ShortName = "アガリ「コミカルスマッシュ」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara03,
			IsComical = true
		},
		new Item
		{
			ObjectID = "OID_03AgariUR01",
			ItemID = ItemID.Item03AgariUR01,
			ShortName = "アガリ「スペシャルアニメ 1」",
			Category = Category.AgariBig,
			Rarity = Rarity.UR,
			CharaType = CharaType.Chara03,
			IsComical = false,
			IsMovie = true
		},
		new Item
		{
			ObjectID = "OID_03NakiA01",
			ItemID = ItemID.Item03NakiA01,
			ShortName = "鳴き「プチキャラ（笑顔）」",
			Category = Category.Naki,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara03,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_03NakiN01",
			ItemID = ItemID.Item03NakiN01,
			ShortName = "鳴き「プチキャラ（満面の笑み）」",
			Category = Category.Naki,
			Rarity = Rarity.N,
			CharaType = CharaType.Chara03,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_03NakiN02",
			ItemID = ItemID.Item03NakiN02,
			ShortName = "鳴き「プチキャラ（余裕）」",
			Category = Category.Naki,
			Rarity = Rarity.N,
			CharaType = CharaType.Chara03,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_03NakiN03",
			ItemID = ItemID.Item03NakiN03,
			ShortName = "鳴き「プチキャラ（微笑）」",
			Category = Category.Naki,
			Rarity = Rarity.N,
			CharaType = CharaType.Chara03,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_03ReachA01",
			ItemID = ItemID.Item03ReachA01,
			ShortName = "リーチ「プチキャラ」",
			Category = Category.Reach,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara03,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_03ReachR01",
			ItemID = ItemID.Item03ReachR01,
			ShortName = "リーチ「スタンダード」",
			Category = Category.Reach,
			Rarity = Rarity.R,
			CharaType = CharaType.Chara03,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_03ReachR02",
			ItemID = ItemID.Item03ReachR02,
			ShortName = "リーチ「コミカル」",
			Category = Category.Reach,
			Rarity = Rarity.R,
			CharaType = CharaType.Chara03,
			IsComical = true
		},
		new Item
		{
			ObjectID = "OID_04AgariA01",
			ItemID = ItemID.Item04AgariA01,
			ShortName = "アガリ「スタンダード」",
			Category = Category.AgariSmall,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara04,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_04AgariR01",
			ItemID = ItemID.Item04AgariR01,
			ShortName = "アガリ「コミカル」",
			Category = Category.AgariSmall,
			Rarity = Rarity.R,
			CharaType = CharaType.Chara04,
			IsComical = true
		},
		new Item
		{
			ObjectID = "OID_04AgariSR01",
			ItemID = ItemID.Item04AgariSR01,
			ShortName = "アガリ「スマッシュ」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara04,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_04AgariSR02",
			ItemID = ItemID.Item04AgariSR02,
			ShortName = "アガリ「バニー」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara04,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_04AgariSR03",
			ItemID = ItemID.Item04AgariSR03,
			ShortName = "アガリ「コミカルスマッシュ」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara04,
			IsComical = true
		},
		new Item
		{
			ObjectID = "OID_04AgariUR01",
			ItemID = ItemID.Item04AgariUR01,
			ShortName = "アガリ「スペシャルアニメ 1」",
			Category = Category.AgariBig,
			Rarity = Rarity.UR,
			CharaType = CharaType.Chara04,
			IsComical = false,
			IsMovie = true
		},
		new Item
		{
			ObjectID = "OID_04NakiA01",
			ItemID = ItemID.Item04NakiA01,
			ShortName = "鳴き「プチキャラ（笑い）」",
			Category = Category.Naki,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara04,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_04NakiN01",
			ItemID = ItemID.Item04NakiN01,
			ShortName = "鳴き「プチキャラ（高笑い）」",
			Category = Category.Naki,
			Rarity = Rarity.N,
			CharaType = CharaType.Chara04,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_04NakiN02",
			ItemID = ItemID.Item04NakiN02,
			ShortName = "鳴き「プチキャラ（投げキッス）」",
			Category = Category.Naki,
			Rarity = Rarity.N,
			CharaType = CharaType.Chara04,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_04NakiN03",
			ItemID = ItemID.Item04NakiN03,
			ShortName = "鳴き「プチキャラ（ウィンク）」",
			Category = Category.Naki,
			Rarity = Rarity.N,
			CharaType = CharaType.Chara04,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_04ReachA01",
			ItemID = ItemID.Item04ReachA01,
			ShortName = "リーチ「プチキャラ」",
			Category = Category.Reach,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara04,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_04ReachR01",
			ItemID = ItemID.Item04ReachR01,
			ShortName = "リーチ「スタンダード」",
			Category = Category.Reach,
			Rarity = Rarity.R,
			CharaType = CharaType.Chara04,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_04ReachR02",
			ItemID = ItemID.Item04ReachR02,
			ShortName = "リーチ「コミカル」",
			Category = Category.Reach,
			Rarity = Rarity.R,
			CharaType = CharaType.Chara04,
			IsComical = true
		},
		new Item
		{
			ObjectID = "OID_05AgariA01",
			ItemID = ItemID.Item05AgariA01,
			ShortName = "アガリ「スタンダード」",
			Category = Category.AgariSmall,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara05,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_05AgariR01",
			ItemID = ItemID.Item05AgariR01,
			ShortName = "アガリ「コミカル」",
			Category = Category.AgariSmall,
			Rarity = Rarity.R,
			CharaType = CharaType.Chara05,
			IsComical = true
		},
		new Item
		{
			ObjectID = "OID_05AgariSR01",
			ItemID = ItemID.Item05AgariSR01,
			ShortName = "アガリ「スマッシュ」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara05,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_05AgariSR02",
			ItemID = ItemID.Item05AgariSR02,
			ShortName = "アガリ「バニー」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara05,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_05AgariSR03",
			ItemID = ItemID.Item05AgariSR03,
			ShortName = "アガリ「コミカルスマッシュ」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara05,
			IsComical = true
		},
		new Item
		{
			ObjectID = "OID_05AgariUR01",
			ItemID = ItemID.Item05AgariUR01,
			ShortName = "アガリ「スペシャルアニメ 1」",
			Category = Category.AgariBig,
			Rarity = Rarity.UR,
			CharaType = CharaType.Chara05,
			IsComical = false,
			IsMovie = true
		},
		new Item
		{
			ObjectID = "OID_05NakiA01",
			ItemID = ItemID.Item05NakiA01,
			ShortName = "鳴き「プチキャラ（ねこ）」",
			Category = Category.Naki,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara05,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_05NakiN01",
			ItemID = ItemID.Item05NakiN01,
			ShortName = "鳴き「プチキャラ（ウィンク）」",
			Category = Category.Naki,
			Rarity = Rarity.N,
			CharaType = CharaType.Chara05,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_05NakiN02",
			ItemID = ItemID.Item05NakiN02,
			ShortName = "鳴き「プチキャラ（頬張り）」",
			Category = Category.Naki,
			Rarity = Rarity.N,
			CharaType = CharaType.Chara05,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_05NakiN03",
			ItemID = ItemID.Item05NakiN03,
			ShortName = "鳴き「プチキャラ（挑発）」",
			Category = Category.Naki,
			Rarity = Rarity.N,
			CharaType = CharaType.Chara05,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_05ReachA01",
			ItemID = ItemID.Item05ReachA01,
			ShortName = "リーチ「プチキャラ」",
			Category = Category.Reach,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara05,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_05ReachR01",
			ItemID = ItemID.Item05ReachR01,
			ShortName = "リーチ「スタンダード」",
			Category = Category.Reach,
			Rarity = Rarity.R,
			CharaType = CharaType.Chara05,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_05ReachR02",
			ItemID = ItemID.Item05ReachR02,
			ShortName = "リーチ「コミカル」",
			Category = Category.Reach,
			Rarity = Rarity.R,
			CharaType = CharaType.Chara05,
			IsComical = true
		},
		new Item
		{
			ObjectID = "OID_06AgariA01",
			ItemID = ItemID.Item06AgariA01,
			ShortName = "アガリ「スタンダード」",
			Category = Category.AgariSmall,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara06,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_06AgariR01",
			ItemID = ItemID.Item06AgariR01,
			ShortName = "アガリ「コミカル」",
			Category = Category.AgariSmall,
			Rarity = Rarity.R,
			CharaType = CharaType.Chara06,
			IsComical = true
		},
		new Item
		{
			ObjectID = "OID_06AgariSR01",
			ItemID = ItemID.Item06AgariSR01,
			ShortName = "アガリ「スマッシュ」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara06,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_06AgariSR02",
			ItemID = ItemID.Item06AgariSR02,
			ShortName = "アガリ「バニー」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara06,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_06AgariSR03",
			ItemID = ItemID.Item06AgariSR03,
			ShortName = "アガリ「コミカルスマッシュ」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara06,
			IsComical = true
		},
		new Item
		{
			ObjectID = "OID_06AgariSR04",
			ItemID = ItemID.Item06AgariSR04,
			ShortName = "アガリ「水着」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara06,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_06AgariUR01",
			ItemID = ItemID.Item06AgariUR01,
			ShortName = "アガリ「スペシャルアニメ 1」",
			Category = Category.AgariBig,
			Rarity = Rarity.UR,
			CharaType = CharaType.Chara06,
			IsComical = false,
			IsMovie = true
		},
		new Item
		{
			ObjectID = "OID_06NakiA01",
			ItemID = ItemID.Item06NakiA01,
			ShortName = "鳴き「プチキャラ（指差し）」",
			Category = Category.Naki,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara06,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_06NakiN01",
			ItemID = ItemID.Item06NakiN01,
			ShortName = "鳴き「プチキャラ（うっとり）」",
			Category = Category.Naki,
			Rarity = Rarity.N,
			CharaType = CharaType.Chara06,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_06NakiN02",
			ItemID = ItemID.Item06NakiN02,
			ShortName = "鳴き「プチキャラ（挑発）」",
			Category = Category.Naki,
			Rarity = Rarity.N,
			CharaType = CharaType.Chara06,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_06NakiN03",
			ItemID = ItemID.Item06NakiN03,
			ShortName = "鳴き「プチキャラ（高笑い）」",
			Category = Category.Naki,
			Rarity = Rarity.N,
			CharaType = CharaType.Chara06,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_06ReachA01",
			ItemID = ItemID.Item06ReachA01,
			ShortName = "リーチ「プチキャラ」",
			Category = Category.Reach,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara06,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_06ReachR01",
			ItemID = ItemID.Item06ReachR01,
			ShortName = "リーチ「スタンダード」",
			Category = Category.Reach,
			Rarity = Rarity.R,
			CharaType = CharaType.Chara06,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_06ReachR02",
			ItemID = ItemID.Item06ReachR02,
			ShortName = "リーチ「コミカル」",
			Category = Category.Reach,
			Rarity = Rarity.R,
			CharaType = CharaType.Chara06,
			IsComical = true
		},
		new Item
		{
			ObjectID = "OID_07AgariA01",
			ItemID = ItemID.Item07AgariA01,
			ShortName = "アガリ「スタンダード」",
			Category = Category.AgariSmall,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara07,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_07AgariR01",
			ItemID = ItemID.Item07AgariR01,
			ShortName = "アガリ「コミカル」",
			Category = Category.AgariSmall,
			Rarity = Rarity.R,
			CharaType = CharaType.Chara07,
			IsComical = true
		},
		new Item
		{
			ObjectID = "OID_07AgariSR01",
			ItemID = ItemID.Item07AgariSR01,
			ShortName = "アガリ「スマッシュ」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara07,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_07AgariSR02",
			ItemID = ItemID.Item07AgariSR02,
			ShortName = "アガリ「バニー」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara07,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_07AgariSR03",
			ItemID = ItemID.Item07AgariSR03,
			ShortName = "アガリ「コミカルスマッシュ」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara07,
			IsComical = true
		},
		new Item
		{
			ObjectID = "OID_07AgariUR01",
			ItemID = ItemID.Item07AgariUR01,
			ShortName = "アガリ「スペシャルアニメ 1」",
			Category = Category.AgariBig,
			Rarity = Rarity.UR,
			CharaType = CharaType.Chara07,
			IsComical = false,
			IsMovie = true
		},
		new Item
		{
			ObjectID = "OID_07NakiA01",
			ItemID = ItemID.Item07NakiA01,
			ShortName = "鳴き「プチキャラ（スマホ）」",
			Category = Category.Naki,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara07,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_07NakiN01",
			ItemID = ItemID.Item07NakiN01,
			ShortName = "鳴き「プチキャラ（ピース）」",
			Category = Category.Naki,
			Rarity = Rarity.N,
			CharaType = CharaType.Chara07,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_07NakiN02",
			ItemID = ItemID.Item07NakiN02,
			ShortName = "鳴き「プチキャラ（挨拶）」",
			Category = Category.Naki,
			Rarity = Rarity.N,
			CharaType = CharaType.Chara07,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_07NakiN03",
			ItemID = ItemID.Item07NakiN03,
			ShortName = "鳴き「プチキャラ（チラ見）」",
			Category = Category.Naki,
			Rarity = Rarity.N,
			CharaType = CharaType.Chara07,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_07ReachA01",
			ItemID = ItemID.Item07ReachA01,
			ShortName = "リーチ「プチキャラ」",
			Category = Category.Reach,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara07,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_07ReachR01",
			ItemID = ItemID.Item07ReachR01,
			ShortName = "リーチ「スタンダード」",
			Category = Category.Reach,
			Rarity = Rarity.R,
			CharaType = CharaType.Chara07,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_07ReachR02",
			ItemID = ItemID.Item07ReachR02,
			ShortName = "リーチ「コミカル」",
			Category = Category.Reach,
			Rarity = Rarity.R,
			CharaType = CharaType.Chara07,
			IsComical = true
		},
		new Item
		{
			ObjectID = "OID_08AgariA01",
			ItemID = ItemID.Item08AgariA01,
			ShortName = "アガリ「スタンダード」",
			Category = Category.AgariSmall,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara08,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_08AgariR01",
			ItemID = ItemID.Item08AgariR01,
			ShortName = "アガリ「コミカル」",
			Category = Category.AgariSmall,
			Rarity = Rarity.R,
			CharaType = CharaType.Chara08,
			IsComical = true
		},
		new Item
		{
			ObjectID = "OID_08AgariSR01",
			ItemID = ItemID.Item08AgariSR01,
			ShortName = "アガリ「スマッシュ」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara08,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_08AgariSR02",
			ItemID = ItemID.Item08AgariSR02,
			ShortName = "アガリ「バニー」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara08,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_08AgariSR03",
			ItemID = ItemID.Item08AgariSR03,
			ShortName = "アガリ「コミカルスマッシュ」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara08,
			IsComical = true
		},
		new Item
		{
			ObjectID = "OID_08AgariUR01",
			ItemID = ItemID.Item08AgariUR01,
			ShortName = "アガリ「スペシャルアニメ 1」",
			Category = Category.AgariBig,
			Rarity = Rarity.UR,
			CharaType = CharaType.Chara08,
			IsComical = false,
			IsMovie = true
		},
		new Item
		{
			ObjectID = "OID_08NakiA01",
			ItemID = ItemID.Item08NakiA01,
			ShortName = "鳴き「プチキャラ（誇らし気）」",
			Category = Category.Naki,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara08,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_08NakiN01",
			ItemID = ItemID.Item08NakiN01,
			ShortName = "鳴き「プチキャラ（指差し）」",
			Category = Category.Naki,
			Rarity = Rarity.N,
			CharaType = CharaType.Chara08,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_08NakiN02",
			ItemID = ItemID.Item08NakiN02,
			ShortName = "鳴き「プチキャラ（威嚇）」",
			Category = Category.Naki,
			Rarity = Rarity.N,
			CharaType = CharaType.Chara08,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_08NakiN03",
			ItemID = ItemID.Item08NakiN03,
			ShortName = "鳴き「プチキャラ（構え）」",
			Category = Category.Naki,
			Rarity = Rarity.N,
			CharaType = CharaType.Chara08,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_08ReachA01",
			ItemID = ItemID.Item08ReachA01,
			ShortName = "リーチ「プチキャラ」",
			Category = Category.Reach,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara08,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_08ReachR01",
			ItemID = ItemID.Item08ReachR01,
			ShortName = "リーチ「スタンダード」",
			Category = Category.Reach,
			Rarity = Rarity.R,
			CharaType = CharaType.Chara08,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_08ReachR02",
			ItemID = ItemID.Item08ReachR02,
			ShortName = "リーチ「コミカル」",
			Category = Category.Reach,
			Rarity = Rarity.R,
			CharaType = CharaType.Chara08,
			IsComical = true
		},
		new Item
		{
			ObjectID = "OID_09AgariA01",
			ItemID = ItemID.Item09AgariA01,
			ShortName = "アガリ「スタンダード」",
			Category = Category.AgariSmall,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara09,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_09AgariR01",
			ItemID = ItemID.Item09AgariR01,
			ShortName = "アガリ「コミカル」",
			Category = Category.AgariSmall,
			Rarity = Rarity.R,
			CharaType = CharaType.Chara09,
			IsComical = true
		},
		new Item
		{
			ObjectID = "OID_09AgariSR01",
			ItemID = ItemID.Item09AgariSR01,
			ShortName = "アガリ「スマッシュ」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara09,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_09AgariSR02",
			ItemID = ItemID.Item09AgariSR02,
			ShortName = "アガリ「バニー」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara09,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_09AgariSR03",
			ItemID = ItemID.Item09AgariSR03,
			ShortName = "アガリ「コミカルスマッシュ」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara09,
			IsComical = true
		},
		new Item
		{
			ObjectID = "OID_09AgariUR01",
			ItemID = ItemID.Item09AgariUR01,
			ShortName = "アガリ「スペシャルアニメ 1」",
			Category = Category.AgariBig,
			Rarity = Rarity.UR,
			CharaType = CharaType.Chara09,
			IsComical = false,
			IsMovie = true
		},
		new Item
		{
			ObjectID = "OID_09NakiA01",
			ItemID = ItemID.Item09NakiA01,
			ShortName = "鳴き「プチキャラ（誇らし気）」",
			Category = Category.Naki,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara09,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_09NakiN01",
			ItemID = ItemID.Item09NakiN01,
			ShortName = "鳴き「プチキャラ（投げキッス）」",
			Category = Category.Naki,
			Rarity = Rarity.N,
			CharaType = CharaType.Chara09,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_09NakiN02",
			ItemID = ItemID.Item09NakiN02,
			ShortName = "鳴き「プチキャラ（おねだり）」",
			Category = Category.Naki,
			Rarity = Rarity.N,
			CharaType = CharaType.Chara09,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_09NakiN03",
			ItemID = ItemID.Item09NakiN03,
			ShortName = "鳴き「プチキャラ（天才）」",
			Category = Category.Naki,
			Rarity = Rarity.N,
			CharaType = CharaType.Chara09,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_09ReachA01",
			ItemID = ItemID.Item09ReachA01,
			ShortName = "リーチ「プチキャラ」",
			Category = Category.Reach,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara09,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_09ReachR01",
			ItemID = ItemID.Item09ReachR01,
			ShortName = "リーチ「スタンダード」",
			Category = Category.Reach,
			Rarity = Rarity.R,
			CharaType = CharaType.Chara09,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_09ReachR02",
			ItemID = ItemID.Item09ReachR02,
			ShortName = "リーチ「コミカル」",
			Category = Category.Reach,
			Rarity = Rarity.R,
			CharaType = CharaType.Chara09,
			IsComical = true
		},
		new Item
		{
			ObjectID = "OID_01AgariSR04",
			ItemID = ItemID.Item01AgariSR04,
			ShortName = "アガリ「水着」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara01,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_08AgariSR04",
			ItemID = ItemID.Item08AgariSR04,
			ShortName = "アガリ「水着」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara08,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_05AgariSR04",
			ItemID = ItemID.Item05AgariSR04,
			ShortName = "アガリ「水着」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara05,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_09AgariSR04",
			ItemID = ItemID.Item09AgariSR04,
			ShortName = "アガリ「水着」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara09,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_04AgariSR04",
			ItemID = ItemID.Item04AgariSR04,
			ShortName = "アガリ「水着」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara04,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_05AgariSR05",
			ItemID = ItemID.Item05AgariSR05,
			ShortName = "アガリ「水着(クールver.)」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara05,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_09AgariSR05",
			ItemID = ItemID.Item09AgariSR05,
			ShortName = "アガリ「水着(白衣ver.)」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara09,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_10AgariA01",
			ItemID = ItemID.Item10AgariA01,
			ShortName = "アガリ「スタンダード」",
			Category = Category.AgariSmall,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara10,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_10AgariR01",
			ItemID = ItemID.Item10AgariR01,
			ShortName = "アガリ「コミカル」",
			Category = Category.AgariSmall,
			Rarity = Rarity.R,
			CharaType = CharaType.Chara10,
			IsComical = true
		},
		new Item
		{
			ObjectID = "OID_10AgariSR01",
			ItemID = ItemID.Item10AgariSR01,
			ShortName = "アガリ「スマッシュ」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara10,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_10AgariSR02",
			ItemID = ItemID.Item10AgariSR02,
			ShortName = "アガリ「きらめき高校体操服」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara10,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_10AgariSR03",
			ItemID = ItemID.Item10AgariSR03,
			ShortName = "アガリ「コミカルスマッシュ」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara10,
			IsComical = true
		},
		new Item
		{
			ObjectID = "OID_10AgariUR01",
			ItemID = ItemID.Item10AgariUR01,
			ShortName = "アガリ「スペシャルアニメ 1」",
			Category = Category.AgariBig,
			Rarity = Rarity.UR,
			CharaType = CharaType.Chara10,
			IsComical = false,
			IsMovie = true
		},
		new Item
		{
			ObjectID = "OID_10NakiA01",
			ItemID = ItemID.Item10NakiA01,
			ShortName = "鳴き「プチキャラ（ササヤキ）」",
			Category = Category.Naki,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara10,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_10NakiN01",
			ItemID = ItemID.Item10NakiN01,
			ShortName = "鳴き「プチキャラ（キラメキ）」",
			Category = Category.Naki,
			Rarity = Rarity.N,
			CharaType = CharaType.Chara10,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_10NakiN02",
			ItemID = ItemID.Item10NakiN02,
			ShortName = "鳴き「プチキャラ（ドキドキ）」",
			Category = Category.Naki,
			Rarity = Rarity.N,
			CharaType = CharaType.Chara10,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_10NakiN03",
			ItemID = ItemID.Item10NakiN03,
			ShortName = "鳴き「プチキャラ（ダーイ好き）」",
			Category = Category.Naki,
			Rarity = Rarity.N,
			CharaType = CharaType.Chara10,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_10ReachA01",
			ItemID = ItemID.Item10ReachA01,
			ShortName = "リーチ「プチキャラ」",
			Category = Category.Reach,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara10,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_10ReachR01",
			ItemID = ItemID.Item10ReachR01,
			ShortName = "リーチ「スタンダード」",
			Category = Category.Reach,
			Rarity = Rarity.R,
			CharaType = CharaType.Chara10,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_10ReachR02",
			ItemID = ItemID.Item10ReachR02,
			ShortName = "リーチ「コミカル」",
			Category = Category.Reach,
			Rarity = Rarity.R,
			CharaType = CharaType.Chara10,
			IsComical = true
		},
		new Item
		{
			ObjectID = "OID_03AgariSR04",
			ItemID = ItemID.Item03AgariSR04,
			ShortName = "アガリ「水着」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara03,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_07AgariSR04",
			ItemID = ItemID.Item07AgariSR04,
			ShortName = "アガリ「水着」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara07,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_07AgariSR05",
			ItemID = ItemID.Item07AgariSR05,
			ShortName = "アガリ「水着(パージver.)」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara07,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_01AgariSR05",
			ItemID = ItemID.Item01AgariSR05,
			ShortName = "アガリ「水着(NDKカラー)」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara01,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_02AgariSR05",
			ItemID = ItemID.Item02AgariSR05,
			ShortName = "アガリ「水着(NDKカラー)」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara02,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_05AgariSR06",
			ItemID = ItemID.Item05AgariSR06,
			ShortName = "アガリ「水着(NDKカラー)」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara05,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_04AgariSR05",
			ItemID = ItemID.Item04AgariSR05,
			ShortName = "アガリ「水着(NBDカラー)」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara04,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_08AgariSR05",
			ItemID = ItemID.Item08AgariSR05,
			ShortName = "アガリ「水着(NBDカラー)」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara08,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_09AgariSR06",
			ItemID = ItemID.Item09AgariSR06,
			ShortName = "アガリ「水着(マッド)」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara09,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_02AgariSR06",
			ItemID = ItemID.Item02AgariSR06,
			ShortName = "アガリ「雀喫茶メイド」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara02,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_05AgariSR07",
			ItemID = ItemID.Item05AgariSR07,
			ShortName = "アガリ「水着(NDKクールver.)」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara05,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_09AgariSR07",
			ItemID = ItemID.Item09AgariSR07,
			ShortName = "アガリ「水着(マッド白衣ver.)」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara09,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_03AgariSR05",
			ItemID = ItemID.Item03AgariSR05,
			ShortName = "アガリ「水着(AFDカラー)」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara03,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_06AgariSR05",
			ItemID = ItemID.Item06AgariSR05,
			ShortName = "アガリ「水着(AFDカラー)」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara06,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_07AgariSR06",
			ItemID = ItemID.Item07AgariSR06,
			ShortName = "アガリ「水着(AFDカラー)」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara07,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_07AgariSR07",
			ItemID = ItemID.Item07AgariSR07,
			ShortName = "アガリ「水着(AFDパージver.)」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara07,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_08AgariSR06",
			ItemID = ItemID.Item08AgariSR06,
			ShortName = "アガリ「ハロウィン」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara08,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_11AgariA01",
			ItemID = ItemID.Item11AgariA01,
			ShortName = "アガリ「スタンダード」",
			Category = Category.AgariSmall,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara11,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_11AgariR01",
			ItemID = ItemID.Item11AgariR01,
			ShortName = "アガリ「コミカル」",
			Category = Category.AgariSmall,
			Rarity = Rarity.R,
			CharaType = CharaType.Chara11,
			IsComical = true
		},
		new Item
		{
			ObjectID = "OID_11AgariSR01",
			ItemID = ItemID.Item11AgariSR01,
			ShortName = "アガリ「スマッシュ」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara11,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_11AgariSR02",
			ItemID = ItemID.Item11AgariSR02,
			ShortName = "アガリ「ホワイトバニー」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara11,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_11AgariSR03",
			ItemID = ItemID.Item11AgariSR03,
			ShortName = "アガリ「ブラックバニー」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara11,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_11AgariSR04",
			ItemID = ItemID.Item11AgariSR04,
			ShortName = "アガリ「コミカルスマッシュ」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara11,
			IsComical = true
		},
		new Item
		{
			ObjectID = "OID_11AgariUR01",
			ItemID = ItemID.Item11AgariUR01,
			ShortName = "アガリ「スペシャルアニメ 1」",
			Category = Category.AgariBig,
			Rarity = Rarity.UR,
			CharaType = CharaType.Chara11,
			IsComical = false,
			IsMovie = true
		},
		new Item
		{
			ObjectID = "OID_11NakiA01",
			ItemID = ItemID.Item11NakiA01,
			ShortName = "鳴き「プチキャラ（クイックファイア）」",
			Category = Category.Naki,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara11,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_11NakiN01",
			ItemID = ItemID.Item11NakiN01,
			ShortName = "鳴き「プチキャラ（ずっきゅんエイム）」",
			Category = Category.Naki,
			Rarity = Rarity.N,
			CharaType = CharaType.Chara11,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_11NakiN02",
			ItemID = ItemID.Item11NakiN02,
			ShortName = "鳴き「プチキャラ（ダブルファイア）」",
			Category = Category.Naki,
			Rarity = Rarity.N,
			CharaType = CharaType.Chara11,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_11NakiN03",
			ItemID = ItemID.Item11NakiN03,
			ShortName = "鳴き「プチキャラ（シングルファイア）」",
			Category = Category.Naki,
			Rarity = Rarity.N,
			CharaType = CharaType.Chara11,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_11ReachA01",
			ItemID = ItemID.Item11ReachA01,
			ShortName = "リーチ「プチキャラ」",
			Category = Category.Reach,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara11,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_11ReachR01",
			ItemID = ItemID.Item11ReachR01,
			ShortName = "リーチ「スタンダード」",
			Category = Category.Reach,
			Rarity = Rarity.R,
			CharaType = CharaType.Chara11,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_11ReachR02",
			ItemID = ItemID.Item11ReachR02,
			ShortName = "リーチ「コミカル」",
			Category = Category.Reach,
			Rarity = Rarity.R,
			CharaType = CharaType.Chara11,
			IsComical = true
		},
		new Item
		{
			ObjectID = "OID_06AgariSR06",
			ItemID = ItemID.Item06AgariSR06,
			ShortName = "アガリ「シンデレラ」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara06,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_04AgariSR06",
			ItemID = ItemID.Item04AgariSR06,
			ShortName = "アガリ「クリスマス」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara04,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_03AgariSR06",
			ItemID = ItemID.Item03AgariSR06,
			ShortName = "アガリ「お正月」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara03,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_05AgariSR08",
			ItemID = ItemID.Item05AgariSR08,
			ShortName = "アガリ「お正月」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara05,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_07AgariSR08",
			ItemID = ItemID.Item07AgariSR08,
			ShortName = "アガリ「バレンタイン」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara07,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_12AgariSR01",
			ItemID = ItemID.Item12AgariSR01,
			ShortName = "アガリ「スタンダード」",
			Category = Category.AgariSmall,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara12,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_12AgariR01",
			ItemID = ItemID.Item12AgariR01,
			ShortName = "アガリ「コミカル」",
			Category = Category.AgariSmall,
			Rarity = Rarity.R,
			CharaType = CharaType.Chara12,
			IsComical = true
		},
		new Item
		{
			ObjectID = "OID_12AgariSR02",
			ItemID = ItemID.Item12AgariSR02,
			ShortName = "アガリ「スマッシュ」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara12,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_12AgariSR03",
			ItemID = ItemID.Item12AgariSR03,
			ShortName = "アガリ「ホワイトバニー」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara12,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_12AgariSR04",
			ItemID = ItemID.Item12AgariSR04,
			ShortName = "アガリ「ブラックバニー」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara12,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_12AgariSR05",
			ItemID = ItemID.Item12AgariSR05,
			ShortName = "アガリ「コミカルスマッシュ」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara12,
			IsComical = true
		},
		new Item
		{
			ObjectID = "OID_12AgariUR01",
			ItemID = ItemID.Item12AgariUR01,
			ShortName = "アガリ「スペシャルアニメ 1」",
			Category = Category.AgariBig,
			Rarity = Rarity.UR,
			CharaType = CharaType.Chara12,
			IsComical = false,
			IsMovie = true
		},
		new Item
		{
			ObjectID = "OID_12NakiA01",
			ItemID = ItemID.Item12NakiA01,
			ShortName = "鳴き「プチキャラ（誇らし気）」",
			Category = Category.Naki,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara12,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_12NakiN01",
			ItemID = ItemID.Item12NakiN01,
			ShortName = "鳴き「プチキャラ（フルスイング）」",
			Category = Category.Naki,
			Rarity = Rarity.N,
			CharaType = CharaType.Chara12,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_12NakiN02",
			ItemID = ItemID.Item12NakiN02,
			ShortName = "鳴き「プチキャラ（投げキッス）」",
			Category = Category.Naki,
			Rarity = Rarity.N,
			CharaType = CharaType.Chara12,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_12NakiN03",
			ItemID = ItemID.Item12NakiN03,
			ShortName = "鳴き「プチキャラ（振り返り）」",
			Category = Category.Naki,
			Rarity = Rarity.N,
			CharaType = CharaType.Chara12,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_12ReachA01",
			ItemID = ItemID.Item12ReachA01,
			ShortName = "リーチ「プチキャラ」",
			Category = Category.Reach,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara12,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_12ReachR01",
			ItemID = ItemID.Item12ReachR01,
			ShortName = "リーチ「スタンダード」",
			Category = Category.Reach,
			Rarity = Rarity.R,
			CharaType = CharaType.Chara12,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_12ReachR02",
			ItemID = ItemID.Item12ReachR02,
			ShortName = "リーチ「コミカル」",
			Category = Category.Reach,
			Rarity = Rarity.R,
			CharaType = CharaType.Chara12,
			IsComical = true
		},
		new Item
		{
			ObjectID = "OID_04AgariSR07",
			ItemID = ItemID.Item04AgariSR07,
			ShortName = "アガリ「サキュバスチアコス」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara04,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_04AgariSR08",
			ItemID = ItemID.Item04AgariSR08,
			ShortName = "アガリ「銃王無尽グレイスコス」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara04,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_03AgariSR07",
			ItemID = ItemID.Item03AgariSR07,
			ShortName = "アガリ「パロディウス サンバカーニバルコス」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara03,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_03AgariSR08",
			ItemID = ItemID.Item03AgariSR08,
			ShortName = "アガリ「銃王無尽グレイスコス」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara03,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_01AgariSR06",
			ItemID = ItemID.Item01AgariSR06,
			ShortName = "アガリ「レースクイーン」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara01,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_11AgariSR05",
			ItemID = ItemID.Item11AgariSR05,
			ShortName = "アガリ「水着」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara11,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_12AgariSR06",
			ItemID = ItemID.Item12AgariSR06,
			ShortName = "アガリ「水着」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara12,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_09AgariSR08",
			ItemID = ItemID.Item09AgariSR08,
			ShortName = "アガリ「アリスメイド」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara09,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_09AgariSR09",
			ItemID = ItemID.Item09AgariSR09,
			ShortName = "アガリ「アリスメイド(ロイヤルカラー)」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara09,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_06AgariSR07",
			ItemID = ItemID.Item06AgariSR07,
			ShortName = "アガリ「暗黒部屋着」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara06,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_06AgariSR08",
			ItemID = ItemID.Item06AgariSR08,
			ShortName = "アガリ「暗黒部屋着(パージver.)」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara06,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_08AgariSR07",
			ItemID = ItemID.Item08AgariSR07,
			ShortName = "アガリ「役満高等学校体操服」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara08,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_02AgariSR07",
			ItemID = ItemID.Item02AgariSR07,
			ShortName = "アガリ「青春チアリーダー」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara02,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_05AgariSR09",
			ItemID = ItemID.Item05AgariSR09,
			ShortName = "アガリ「壱役高等学校夏制服」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara05,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_07AgariSR09",
			ItemID = ItemID.Item07AgariSR09,
			ShortName = "アガリ「ゲーマー浴衣」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara07,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_05AgariSR10",
			ItemID = ItemID.Item05AgariSR10,
			ShortName = "アガリ「壱役高等学校夏制服(!?ver.)」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara05,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_13AgariSR01",
			ItemID = ItemID.Item13AgariSR01,
			ShortName = "アガリ「スタンダード」",
			Category = Category.AgariSmall,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara13,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_13AgariR01",
			ItemID = ItemID.Item13AgariR01,
			ShortName = "アガリ「コミカル」",
			Category = Category.AgariSmall,
			Rarity = Rarity.R,
			CharaType = CharaType.Chara13,
			IsComical = true
		},
		new Item
		{
			ObjectID = "OID_13AgariSR02",
			ItemID = ItemID.Item13AgariSR02,
			ShortName = "アガリ「スマッシュ」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara13,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_13AgariSR03",
			ItemID = ItemID.Item13AgariSR03,
			ShortName = "アガリ「バニー」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara13,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_13AgariSR04",
			ItemID = ItemID.Item13AgariSR04,
			ShortName = "アガリ「コミカルスマッシュ」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara13,
			IsComical = true
		},
		new Item
		{
			ObjectID = "OID_13AgariSR05",
			ItemID = ItemID.Item13AgariSR05,
			ShortName = "アガリ「デビュー」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara13,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_13AgariSR06",
			ItemID = ItemID.Item13AgariSR06,
			ShortName = "アガリ「水着」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara13,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_13NakiA01",
			ItemID = ItemID.Item13NakiA01,
			ShortName = "鳴き「プチキャラ（イヨがいちばん！）」",
			Category = Category.Naki,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara13,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_13NakiN01",
			ItemID = ItemID.Item13NakiN01,
			ShortName = "鳴き「プチキャラ（指ふりふり）」",
			Category = Category.Naki,
			Rarity = Rarity.N,
			CharaType = CharaType.Chara13,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_13NakiN02",
			ItemID = ItemID.Item13NakiN02,
			ShortName = "鳴き「プチキャラ（おいしくなーれ）」",
			Category = Category.Naki,
			Rarity = Rarity.N,
			CharaType = CharaType.Chara13,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_13NakiN03",
			ItemID = ItemID.Item13NakiN03,
			ShortName = "鳴き「プチキャラ（GUILTY）」",
			Category = Category.Naki,
			Rarity = Rarity.N,
			CharaType = CharaType.Chara13,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_13ReachA01",
			ItemID = ItemID.Item13ReachA01,
			ShortName = "リーチ「プチキャラ」",
			Category = Category.Reach,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara13,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_13ReachR01",
			ItemID = ItemID.Item13ReachR01,
			ShortName = "リーチ「スタンダード」",
			Category = Category.Reach,
			Rarity = Rarity.R,
			CharaType = CharaType.Chara13,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_13ReachR02",
			ItemID = ItemID.Item13ReachR02,
			ShortName = "リーチ「コミカル」",
			Category = Category.Reach,
			Rarity = Rarity.R,
			CharaType = CharaType.Chara13,
			IsComical = true
		},
		new Item
		{
			ObjectID = "OID_01AgariA02",
			ItemID = ItemID.Item01AgariA02,
			ShortName = "アガリ「スペシャルアニメ 2」",
			Category = Category.AgariBig,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara01,
			IsComical = false,
			IsMovie = true
		},
		new Item
		{
			ObjectID = "OID_02AgariA02",
			ItemID = ItemID.Item02AgariA02,
			ShortName = "アガリ「スペシャルアニメ 2」",
			Category = Category.AgariBig,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara02,
			IsComical = false,
			IsMovie = true
		},
		new Item
		{
			ObjectID = "OID_03AgariA02",
			ItemID = ItemID.Item03AgariA02,
			ShortName = "アガリ「スペシャルアニメ 2」",
			Category = Category.AgariBig,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara03,
			IsComical = false,
			IsMovie = true
		},
		new Item
		{
			ObjectID = "OID_04AgariA02",
			ItemID = ItemID.Item04AgariA02,
			ShortName = "アガリ「スペシャルアニメ 2」",
			Category = Category.AgariBig,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara04,
			IsComical = false,
			IsMovie = true
		},
		new Item
		{
			ObjectID = "OID_05AgariA02",
			ItemID = ItemID.Item05AgariA02,
			ShortName = "アガリ「スペシャルアニメ 2」",
			Category = Category.AgariBig,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara05,
			IsComical = false,
			IsMovie = true
		},
		new Item
		{
			ObjectID = "OID_06AgariA02",
			ItemID = ItemID.Item06AgariA02,
			ShortName = "アガリ「スペシャルアニメ 2」",
			Category = Category.AgariBig,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara06,
			IsComical = false,
			IsMovie = true
		},
		new Item
		{
			ObjectID = "OID_07AgariA02",
			ItemID = ItemID.Item07AgariA02,
			ShortName = "アガリ「スペシャルアニメ 2」",
			Category = Category.AgariBig,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara07,
			IsComical = false,
			IsMovie = true
		},
		new Item
		{
			ObjectID = "OID_08AgariA02",
			ItemID = ItemID.Item08AgariA02,
			ShortName = "アガリ「スペシャルアニメ 2」",
			Category = Category.AgariBig,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara08,
			IsComical = false,
			IsMovie = true
		},
		new Item
		{
			ObjectID = "OID_09AgariA02",
			ItemID = ItemID.Item09AgariA02,
			ShortName = "アガリ「スペシャルアニメ 2」",
			Category = Category.AgariBig,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara09,
			IsComical = false,
			IsMovie = true
		},
		new Item
		{
			ObjectID = "OID_10AgariA02",
			ItemID = ItemID.Item10AgariA02,
			ShortName = "アガリ「スペシャルアニメ 2」",
			Category = Category.AgariBig,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara10,
			IsComical = false,
			IsMovie = true
		},
		new Item
		{
			ObjectID = "OID_11AgariA02",
			ItemID = ItemID.Item11AgariA02,
			ShortName = "アガリ「スペシャルアニメ 2」",
			Category = Category.AgariBig,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara11,
			IsComical = false,
			IsMovie = true
		},
		new Item
		{
			ObjectID = "OID_12AgariA01",
			ItemID = ItemID.Item12AgariA01,
			ShortName = "アガリ「スペシャルアニメ 2」",
			Category = Category.AgariBig,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara12,
			IsComical = false,
			IsMovie = true
		},
		new Item
		{
			ObjectID = "OID_01AgariSR07",
			ItemID = ItemID.Item01AgariSR07,
			ShortName = "アガリ「霊夢の巫女服」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara01,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_04AgariSR09",
			ItemID = ItemID.Item04AgariSR09,
			ShortName = "アガリ「フランのドレス」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara04,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_12AgariSR07",
			ItemID = ItemID.Item12AgariSR07,
			ShortName = "アガリ「八雲紫の道士服」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara12,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_02AgariSR08",
			ItemID = ItemID.Item02AgariSR08,
			ShortName = "アガリ「水着(小麦色ver.)」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara02,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_05AgariSR11",
			ItemID = ItemID.Item05AgariSR11,
			ShortName = "アガリ「水着(小麦色ver.)」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara05,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_06AgariSR09",
			ItemID = ItemID.Item06AgariSR09,
			ShortName = "アガリ「水着(小麦色ver.)」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara06,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_07AgariSR10",
			ItemID = ItemID.Item07AgariSR10,
			ShortName = "アガリ「水着(小麦色ver.)」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara07,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_09AgariSR10",
			ItemID = ItemID.Item09AgariSR10,
			ShortName = "アガリ「水着(小麦色ver.)」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara09,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_02AgariSR09",
			ItemID = ItemID.Item02AgariSR09,
			ShortName = "アガリ「プライベートデート水着」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara02,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_02AgariSR10",
			ItemID = ItemID.Item02AgariSR10,
			ShortName = "アガリ「プライベートデート水着(小麦色ver.)」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara02,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_03AgariSR09",
			ItemID = ItemID.Item03AgariSR09,
			ShortName = "アガリ「慈愛シスター」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara03,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_03AgariSR10",
			ItemID = ItemID.Item03AgariSR10,
			ShortName = "アガリ「慈愛シスター(パージver.)」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara03,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_02AgariSR11",
			ItemID = ItemID.Item02AgariSR11,
			ShortName = "アガリ「桃色チアリーダー」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara02,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_09AgariSR11",
			ItemID = ItemID.Item09AgariSR11,
			ShortName = "アガリ「プリティーボンバーズ」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara09,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_09AgariSR12",
			ItemID = ItemID.Item09AgariSR12,
			ShortName = "アガリ「プリティーボンバーズ (キラービーカラー)」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara09,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_14AgariSR01",
			ItemID = ItemID.Item14AgariSR01,
			ShortName = "アガリ「スタンダード」",
			Category = Category.AgariSmall,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara14,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_14AgariR01",
			ItemID = ItemID.Item14AgariR01,
			ShortName = "アガリ「コミカル」",
			Category = Category.AgariSmall,
			Rarity = Rarity.R,
			CharaType = CharaType.Chara14,
			IsComical = true
		},
		new Item
		{
			ObjectID = "OID_14AgariSR02",
			ItemID = ItemID.Item14AgariSR02,
			ShortName = "アガリ「スマッシュ」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara14,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_14AgariSR03",
			ItemID = ItemID.Item14AgariSR03,
			ShortName = "アガリ「バニー」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara14,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_14AgariSR04",
			ItemID = ItemID.Item14AgariSR04,
			ShortName = "アガリ「コミカルスマッシュ」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara14,
			IsComical = true
		},
		new Item
		{
			ObjectID = "OID_14AgariSR05",
			ItemID = ItemID.Item14AgariSR05,
			ShortName = "アガリ「デビュー」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara14,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_14AgariSR06",
			ItemID = ItemID.Item14AgariSR06,
			ShortName = "アガリ「水着」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara14,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_14NakiA01",
			ItemID = ItemID.Item14NakiA01,
			ShortName = "鳴き「プチキャラ（アロエちゃん大好き♡）」",
			Category = Category.Naki,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara14,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_14NakiN01",
			ItemID = ItemID.Item14NakiN01,
			ShortName = "鳴き「プチキャラ（ク・イ・ズ♡）」",
			Category = Category.Naki,
			Rarity = Rarity.N,
			CharaType = CharaType.Chara14,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_14NakiN02",
			ItemID = ItemID.Item14NakiN02,
			ShortName = "鳴き「プチキャラ（あれあれぇ？♡）」",
			Category = Category.Naki,
			Rarity = Rarity.N,
			CharaType = CharaType.Chara14,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_14NakiN03",
			ItemID = ItemID.Item14NakiN03,
			ShortName = "鳴き「プチキャラ（トロトロにしてあ・げ・る♡）」",
			Category = Category.Naki,
			Rarity = Rarity.N,
			CharaType = CharaType.Chara14,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_14ReachA01",
			ItemID = ItemID.Item14ReachA01,
			ShortName = "リーチ「プチキャラ」",
			Category = Category.Reach,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara14,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_14ReachR01",
			ItemID = ItemID.Item14ReachR01,
			ShortName = "リーチ「スタンダード」",
			Category = Category.Reach,
			Rarity = Rarity.R,
			CharaType = CharaType.Chara14,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_14ReachR02",
			ItemID = ItemID.Item14ReachR02,
			ShortName = "リーチ「コミカル」",
			Category = Category.Reach,
			Rarity = Rarity.R,
			CharaType = CharaType.Chara14,
			IsComical = true
		},
		new Item
		{
			ObjectID = "OID_06AgariSR10",
			ItemID = ItemID.Item06AgariSR10,
			ShortName = "アガリ「ダークネスローブ」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara06,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_06AgariSR11",
			ItemID = ItemID.Item06AgariSR11,
			ShortName = "アガリ「ダークネスローブ(仮面ver.)」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara06,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_11AgariSR06",
			ItemID = ItemID.Item11AgariSR06,
			ShortName = "アガリ「普通のナース」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara11,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_11AgariSR07",
			ItemID = ItemID.Item11AgariSR07,
			ShortName = "アガリ「普通の黒ナース」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara11,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_01NakiA02",
			ItemID = ItemID.Item01NakiA02,
			ShortName = "鳴き「プチキャラ（クリスマス）」",
			Category = Category.Naki,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara01,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_02NakiA02",
			ItemID = ItemID.Item02NakiA02,
			ShortName = "鳴き「プチキャラ（クリスマス）」",
			Category = Category.Naki,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara02,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_03NakiA02",
			ItemID = ItemID.Item03NakiA02,
			ShortName = "鳴き「プチキャラ（クリスマス）」",
			Category = Category.Naki,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara03,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_04NakiA02",
			ItemID = ItemID.Item04NakiA02,
			ShortName = "鳴き「プチキャラ（クリスマス）」",
			Category = Category.Naki,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara04,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_05NakiA02",
			ItemID = ItemID.Item05NakiA02,
			ShortName = "鳴き「プチキャラ（クリスマス）」",
			Category = Category.Naki,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara05,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_06NakiA02",
			ItemID = ItemID.Item06NakiA02,
			ShortName = "鳴き「プチキャラ（クリスマス）」",
			Category = Category.Naki,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara06,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_07NakiA02",
			ItemID = ItemID.Item07NakiA02,
			ShortName = "鳴き「プチキャラ（クリスマス）」",
			Category = Category.Naki,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara07,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_08NakiA02",
			ItemID = ItemID.Item08NakiA02,
			ShortName = "鳴き「プチキャラ（クリスマス）」",
			Category = Category.Naki,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara08,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_09NakiA02",
			ItemID = ItemID.Item09NakiA02,
			ShortName = "鳴き「プチキャラ（クリスマス）」",
			Category = Category.Naki,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara09,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_10NakiA02",
			ItemID = ItemID.Item10NakiA02,
			ShortName = "鳴き「プチキャラ（クリスマス）」",
			Category = Category.Naki,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara10,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_11NakiA02",
			ItemID = ItemID.Item11NakiA02,
			ShortName = "鳴き「プチキャラ（クリスマス）」",
			Category = Category.Naki,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara11,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_12NakiA02",
			ItemID = ItemID.Item12NakiA02,
			ShortName = "鳴き「プチキャラ（クリスマス）」",
			Category = Category.Naki,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara12,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_13NakiA02",
			ItemID = ItemID.Item13NakiA02,
			ShortName = "鳴き「プチキャラ（クリスマス）」",
			Category = Category.Naki,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara13,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_14NakiA02",
			ItemID = ItemID.Item14NakiA02,
			ShortName = "鳴き「プチキャラ（クリスマス）」",
			Category = Category.Naki,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara14,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_15AgariSR01",
			ItemID = ItemID.Item15AgariSR01,
			ShortName = "アガリ「スタンダード」",
			Category = Category.AgariSmall,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara15,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_15AgariR01",
			ItemID = ItemID.Item15AgariR01,
			ShortName = "アガリ「コミカル」",
			Category = Category.AgariSmall,
			Rarity = Rarity.R,
			CharaType = CharaType.Chara15,
			IsComical = true
		},
		new Item
		{
			ObjectID = "OID_15AgariSR02",
			ItemID = ItemID.Item15AgariSR02,
			ShortName = "アガリ「スマッシュ」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara15,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_15AgariSR03",
			ItemID = ItemID.Item15AgariSR03,
			ShortName = "アガリ「バニー」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara15,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_15AgariSR04",
			ItemID = ItemID.Item15AgariSR04,
			ShortName = "アガリ「コミカルスマッシュ」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara15,
			IsComical = true
		},
		new Item
		{
			ObjectID = "OID_15AgariSR05",
			ItemID = ItemID.Item15AgariSR05,
			ShortName = "アガリ「デビュー」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara15,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_15AgariSR06",
			ItemID = ItemID.Item15AgariSR06,
			ShortName = "アガリ「水着」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara15,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_15NakiA01",
			ItemID = ItemID.Item15NakiA01,
			ShortName = "鳴き「プチキャラ（ラブパワーが…♡）」",
			Category = Category.Naki,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara15,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_15NakiN01",
			ItemID = ItemID.Item15NakiN01,
			ShortName = "鳴き「プチキャラ（プルコギ！）」",
			Category = Category.Naki,
			Rarity = Rarity.N,
			CharaType = CharaType.Chara15,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_15NakiN02",
			ItemID = ItemID.Item15NakiN02,
			ShortName = "鳴き「プチキャラ（こぶしで！）」",
			Category = Category.Naki,
			Rarity = Rarity.N,
			CharaType = CharaType.Chara15,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_15NakiN03",
			ItemID = ItemID.Item15NakiN03,
			ShortName = "鳴き「プチキャラ（へんしんっ！）」",
			Category = Category.Naki,
			Rarity = Rarity.N,
			CharaType = CharaType.Chara15,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_15ReachA01",
			ItemID = ItemID.Item15ReachA01,
			ShortName = "リーチ「プチキャラ」",
			Category = Category.Reach,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara15,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_15ReachR01",
			ItemID = ItemID.Item15ReachR01,
			ShortName = "リーチ「スタンダード」",
			Category = Category.Reach,
			Rarity = Rarity.R,
			CharaType = CharaType.Chara15,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_15ReachR02",
			ItemID = ItemID.Item15ReachR02,
			ShortName = "リーチ「コミカル」",
			Category = Category.Reach,
			Rarity = Rarity.R,
			CharaType = CharaType.Chara15,
			IsComical = true
		},
		new Item
		{
			ObjectID = "OID_13AgariSR07",
			ItemID = ItemID.Item13AgariSR07,
			ShortName = "アガリ「キョンシー（白蛇）」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara13,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_13AgariSR08",
			ItemID = ItemID.Item13AgariSR08,
			ShortName = "アガリ「キョンシー（碧蛇）」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara13,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_04AgariSR10",
			ItemID = ItemID.Item04AgariSR10,
			ShortName = "アガリ「クリスマス（ブラックサンタ）」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara04,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_08AgariSR08",
			ItemID = ItemID.Item08AgariSR08,
			ShortName = "アガリ「くみゃはっぴ(帝役)」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara08,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_08AgariSR09",
			ItemID = ItemID.Item08AgariSR09,
			ShortName = "アガリ「くみゃはっぴ(鍛錬)」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara08,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_05AgariSR12",
			ItemID = ItemID.Item05AgariSR12,
			ShortName = "アガリ「アラビニャンダンサー」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara05,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_05AgariSR13",
			ItemID = ItemID.Item05AgariSR13,
			ShortName = "アガリ「アラビニャンダンサー(黒)」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara05,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_07AgariSR11",
			ItemID = ItemID.Item07AgariSR11,
			ShortName = "アガリ「バレンタイン（対ありver.）」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara07,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_04AgariSR11",
			ItemID = ItemID.Item04AgariSR11,
			ShortName = "アガリ「ホーリーエンジェル」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara04,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_04AgariSR12",
			ItemID = ItemID.Item04AgariSR12,
			ShortName = "アガリ「ホーリーエンジェル（仮面ver.）」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara04,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_07AgariSR12",
			ItemID = ItemID.Item07AgariSR12,
			ShortName = "アガリ「ロリータドレス」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara07,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_07AgariSR13",
			ItemID = ItemID.Item07AgariSR13,
			ShortName = "アガリ「ロリータドレス（チョコミントver.）」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara07,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_16AgariSR01",
			ItemID = ItemID.Item16AgariSR01,
			ShortName = "アガリ「スタンダード」",
			Category = Category.AgariSmall,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara16,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_16AgariR01",
			ItemID = ItemID.Item16AgariR01,
			ShortName = "アガリ「コミカル」",
			Category = Category.AgariSmall,
			Rarity = Rarity.R,
			CharaType = CharaType.Chara16,
			IsComical = true
		},
		new Item
		{
			ObjectID = "OID_16AgariSR02",
			ItemID = ItemID.Item16AgariSR02,
			ShortName = "アガリ「スマッシュ」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara16,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_16AgariSR03",
			ItemID = ItemID.Item16AgariSR03,
			ShortName = "アガリ「バニー」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara16,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_16AgariSR04",
			ItemID = ItemID.Item16AgariSR04,
			ShortName = "アガリ「コミカルスマッシュ」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara16,
			IsComical = true
		},
		new Item
		{
			ObjectID = "OID_16AgariSR05",
			ItemID = ItemID.Item16AgariSR05,
			ShortName = "アガリ「デビュー」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara16,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_16AgariSR06",
			ItemID = ItemID.Item16AgariSR06,
			ShortName = "アガリ「水着」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara16,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_16NakiA01",
			ItemID = ItemID.Item16NakiA01,
			ShortName = "鳴き「プチキャラ（愛の正拳突き）」",
			Category = Category.Naki,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara16,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_16NakiN01",
			ItemID = ItemID.Item16NakiN01,
			ShortName = "鳴き「プチキャラ（ハイ！注目！）」",
			Category = Category.Naki,
			Rarity = Rarity.N,
			CharaType = CharaType.Chara16,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_16NakiN02",
			ItemID = ItemID.Item16NakiN02,
			ShortName = "鳴き「プチキャラ（こぶしで！）」",
			Category = Category.Naki,
			Rarity = Rarity.N,
			CharaType = CharaType.Chara16,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_16NakiN03",
			ItemID = ItemID.Item16NakiN03,
			ShortName = "鳴き「プチキャラ（へんしんっ！）」",
			Category = Category.Naki,
			Rarity = Rarity.N,
			CharaType = CharaType.Chara16,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_16ReachA01",
			ItemID = ItemID.Item16ReachA01,
			ShortName = "リーチ「プチキャラ」",
			Category = Category.Reach,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara16,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_16ReachR01",
			ItemID = ItemID.Item16ReachR01,
			ShortName = "リーチ「スタンダード」",
			Category = Category.Reach,
			Rarity = Rarity.R,
			CharaType = CharaType.Chara16,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_16ReachR02",
			ItemID = ItemID.Item16ReachR02,
			ShortName = "リーチ「コミカル」",
			Category = Category.Reach,
			Rarity = Rarity.R,
			CharaType = CharaType.Chara16,
			IsComical = true
		},
		new Item
		{
			ObjectID = "OID_01AgariSR08",
			ItemID = ItemID.Item01AgariSR08,
			ShortName = "アガリ「ネリマギャル」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara01,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_01AgariSR09",
			ItemID = ItemID.Item01AgariSR09,
			ShortName = "アガリ「ネリマギャル(キャラメル)」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara01,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_12AgariSR08",
			ItemID = ItemID.Item12AgariSR08,
			ShortName = "アガリ「セイジョウギャル」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara12,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_12AgariSR09",
			ItemID = ItemID.Item12AgariSR09,
			ShortName = "アガリ「セイジョウギャル(ローズ)」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara12,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_14AgariSR07",
			ItemID = ItemID.Item14AgariSR07,
			ShortName = "アガリ「サキュバスギャル」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara14,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_14AgariSR08",
			ItemID = ItemID.Item14AgariSR08,
			ShortName = "アガリ「サキュバスギャル(パンサー)」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara14,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_15AgariSR07",
			ItemID = ItemID.Item15AgariSR07,
			ShortName = "アガリ「アニマルフォーム(しろ)」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara15,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_15AgariSR08",
			ItemID = ItemID.Item15AgariSR08,
			ShortName = "アガリ「アニマルフォーム(くろ)」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara15,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_08AgariSR10",
			ItemID = ItemID.Item08AgariSR10,
			ShortName = "アガリ「アニマルぱじゃま(くま)」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara08,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_08AgariSR11",
			ItemID = ItemID.Item08AgariSR11,
			ShortName = "アガリ「アニマルぱじゃま(くみゃ)」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara08,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_16AgariSR07",
			ItemID = ItemID.Item16AgariSR07,
			ShortName = "アガリ「アニマルフォーム(しろ)」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara16,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_16AgariSR08",
			ItemID = ItemID.Item16AgariSR08,
			ShortName = "アガリ「アニマルフォーム(くろ)」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara16,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_11AgariSR08",
			ItemID = ItemID.Item11AgariSR08,
			ShortName = "アガリ「スパイドレス」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara11,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_11AgariSR09",
			ItemID = ItemID.Item11AgariSR09,
			ShortName = "アガリ「スパイドレス(スノウ)」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara11,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_01AgariSR10",
			ItemID = ItemID.Item01AgariSR10,
			ShortName = "アガリ「レースクイーン(NDKカラー)」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara01,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_03AgariSR11",
			ItemID = ItemID.Item03AgariSR11,
			ShortName = "アガリ「雀喫茶メイド(ナナツノグリーン)」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara03,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_03AgariSR12",
			ItemID = ItemID.Item03AgariSR12,
			ShortName = "アガリ「雀喫茶メイド(リッチブラック)」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara03,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_01AgariSR11",
			ItemID = ItemID.Item01AgariSR11,
			ShortName = "アガリ「雀アイドル」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara01,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_02AgariSR12",
			ItemID = ItemID.Item02AgariSR12,
			ShortName = "アガリ「雀アイドル」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara02,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_17AgariSR01",
			ItemID = ItemID.Item17AgariSR01,
			ShortName = "アガリ「スタンダード」",
			Category = Category.AgariSmall,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara17,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_17AgariR01",
			ItemID = ItemID.Item17AgariR01,
			ShortName = "アガリ「コミカル」",
			Category = Category.AgariSmall,
			Rarity = Rarity.R,
			CharaType = CharaType.Chara17,
			IsComical = true
		},
		new Item
		{
			ObjectID = "OID_17AgariSR02",
			ItemID = ItemID.Item17AgariSR02,
			ShortName = "アガリ「スマッシュ」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara17,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_17AgariSR03",
			ItemID = ItemID.Item17AgariSR03,
			ShortName = "アガリ「バニー」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara17,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_17AgariSR04",
			ItemID = ItemID.Item17AgariSR04,
			ShortName = "アガリ「コミカルスマッシュ」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara17,
			IsComical = true
		},
		new Item
		{
			ObjectID = "OID_17AgariSR05",
			ItemID = ItemID.Item17AgariSR05,
			ShortName = "アガリ「デビュー」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara17,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_17AgariSR06",
			ItemID = ItemID.Item17AgariSR06,
			ShortName = "アガリ「水着」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara17,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_17NakiA01",
			ItemID = ItemID.Item17NakiA01,
			ShortName = "鳴き「プチキャラ（もらっちゃっていいのお？）」",
			Category = Category.Naki,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara17,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_17NakiN01",
			ItemID = ItemID.Item17NakiN01,
			ShortName = "鳴き「プチキャラ（ほうねつ）」",
			Category = Category.Naki,
			Rarity = Rarity.N,
			CharaType = CharaType.Chara17,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_17NakiN02",
			ItemID = ItemID.Item17NakiN02,
			ShortName = "鳴き「プチキャラ（おいの…ｚｚｚ）」",
			Category = Category.Naki,
			Rarity = Rarity.N,
			CharaType = CharaType.Chara17,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_17NakiN03",
			ItemID = ItemID.Item17NakiN03,
			ShortName = "鳴き「プチキャラ（めへへ）」",
			Category = Category.Naki,
			Rarity = Rarity.N,
			CharaType = CharaType.Chara17,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_17ReachA01",
			ItemID = ItemID.Item17ReachA01,
			ShortName = "リーチ「プチキャラ」",
			Category = Category.Reach,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara17,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_17ReachR01",
			ItemID = ItemID.Item17ReachR01,
			ShortName = "リーチ「スタンダード」",
			Category = Category.Reach,
			Rarity = Rarity.R,
			CharaType = CharaType.Chara17,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_17ReachR02",
			ItemID = ItemID.Item17ReachR02,
			ShortName = "リーチ「コミカル」",
			Category = Category.Reach,
			Rarity = Rarity.R,
			CharaType = CharaType.Chara17,
			IsComical = true
		},
		new Item
		{
			ObjectID = "OID_06AgariSR12",
			ItemID = ItemID.Item06AgariSR12,
			ShortName = "アガリ「雀バーガースタッフ」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara06,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_06AgariSR13",
			ItemID = ItemID.Item06AgariSR13,
			ShortName = "アガリ「雀バーガースタッフ(リーダー)」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara06,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_02AgariSR13",
			ItemID = ItemID.Item02AgariSR13,
			ShortName = "アガリ「壱高スクール水着」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara02,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_02AgariSR14",
			ItemID = ItemID.Item02AgariSR14,
			ShortName = "アガリ「壱高スクール水着(えんじ)」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara02,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_13AgariSR09",
			ItemID = ItemID.Item13AgariSR09,
			ShortName = "アガリ「帝役スクール水着」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara13,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_13AgariSR10",
			ItemID = ItemID.Item13AgariSR10,
			ShortName = "アガリ「帝役スクール水着（くろ）」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara13,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_05AgariSR14",
			ItemID = ItemID.Item05AgariSR14,
			ShortName = "アガリ「雀アイドル」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara05,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_04AgariA03",
			ItemID = ItemID.Item04AgariA03,
			ShortName = "アガリ「ぽんのみち(チョンボ)」",
			Category = Category.AgariBig,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara04,
			IsComical = true
		},
		new Item
		{
			ObjectID = "OID_01NakiA03",
			ItemID = ItemID.Item01NakiA03,
			ShortName = "鳴き「プチキャラ（盆踊り）」",
			Category = Category.Naki,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara01,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_02NakiA03",
			ItemID = ItemID.Item02NakiA03,
			ShortName = "鳴き「プチキャラ（盆踊り）」",
			Category = Category.Naki,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara02,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_03NakiA03",
			ItemID = ItemID.Item03NakiA03,
			ShortName = "鳴き「プチキャラ（盆踊り）」",
			Category = Category.Naki,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara03,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_04NakiA03",
			ItemID = ItemID.Item04NakiA03,
			ShortName = "鳴き「プチキャラ（盆踊り）」",
			Category = Category.Naki,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara04,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_05NakiA03",
			ItemID = ItemID.Item05NakiA03,
			ShortName = "鳴き「プチキャラ（盆踊り）」",
			Category = Category.Naki,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara05,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_06NakiA03",
			ItemID = ItemID.Item06NakiA03,
			ShortName = "鳴き「プチキャラ（盆踊り）」",
			Category = Category.Naki,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara06,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_07NakiA03",
			ItemID = ItemID.Item07NakiA03,
			ShortName = "鳴き「プチキャラ（盆踊り）」",
			Category = Category.Naki,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara07,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_08NakiA03",
			ItemID = ItemID.Item08NakiA03,
			ShortName = "鳴き「プチキャラ（盆踊り）」",
			Category = Category.Naki,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara08,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_09NakiA03",
			ItemID = ItemID.Item09NakiA03,
			ShortName = "鳴き「プチキャラ（盆踊り）」",
			Category = Category.Naki,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara09,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_10NakiA03",
			ItemID = ItemID.Item10NakiA03,
			ShortName = "鳴き「プチキャラ（盆踊り）」",
			Category = Category.Naki,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara10,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_11NakiA03",
			ItemID = ItemID.Item11NakiA03,
			ShortName = "鳴き「プチキャラ（盆踊り）」",
			Category = Category.Naki,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara11,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_12NakiA03",
			ItemID = ItemID.Item12NakiA03,
			ShortName = "鳴き「プチキャラ（盆踊り）」",
			Category = Category.Naki,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara12,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_13NakiA03",
			ItemID = ItemID.Item13NakiA03,
			ShortName = "鳴き「プチキャラ（盆踊り）」",
			Category = Category.Naki,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara13,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_14NakiA03",
			ItemID = ItemID.Item14NakiA03,
			ShortName = "鳴き「プチキャラ（盆踊り）」",
			Category = Category.Naki,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara14,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_15NakiA02",
			ItemID = ItemID.Item15NakiA02,
			ShortName = "鳴き「プチキャラ（盆踊り）」",
			Category = Category.Naki,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara15,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_16NakiA02",
			ItemID = ItemID.Item16NakiA02,
			ShortName = "鳴き「プチキャラ（盆踊り）」",
			Category = Category.Naki,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara16,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_17NakiA02",
			ItemID = ItemID.Item17NakiA02,
			ShortName = "鳴き「プチキャラ（盆踊り）」",
			Category = Category.Naki,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara17,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_07AgariSR14",
			ItemID = ItemID.Item07AgariSR14,
			ShortName = "アガリ「弐女スクール水着」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara07,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_07AgariSR15",
			ItemID = ItemID.Item07AgariSR15,
			ShortName = "アガリ「弐女スクール水着（みどり）」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara07,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_09AgariSR13",
			ItemID = ItemID.Item09AgariSR13,
			ShortName = "アガリ「ベリーダンス（黄色）」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara09,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_09AgariSR14",
			ItemID = ItemID.Item09AgariSR14,
			ShortName = "アガリ「ベリーダンス（黄色ベールver.）」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara09,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_09AgariSR15",
			ItemID = ItemID.Item09AgariSR15,
			ShortName = "アガリ「ベリーダンス（水色）」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara09,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_09AgariSR16",
			ItemID = ItemID.Item09AgariSR16,
			ShortName = "アガリ「ベリーダンス（水色ベールver.）」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara09,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_05AgariSR15",
			ItemID = ItemID.Item05AgariSR15,
			ShortName = "アガリ「ねこ浴衣(藍)」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara05,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_05AgariSR16",
			ItemID = ItemID.Item05AgariSR16,
			ShortName = "アガリ「ねこ浴衣(白)」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara05,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_04AgariSR13",
			ItemID = ItemID.Item04AgariSR13,
			ShortName = "アガリ「雀アイドル」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara04,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_18AgariSR01",
			ItemID = ItemID.Item18AgariSR01,
			ShortName = "アガリ「スタンダード」",
			Category = Category.AgariSmall,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara18,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_18AgariR01",
			ItemID = ItemID.Item18AgariR01,
			ShortName = "アガリ「コミカル」",
			Category = Category.AgariSmall,
			Rarity = Rarity.R,
			CharaType = CharaType.Chara18,
			IsComical = true
		},
		new Item
		{
			ObjectID = "OID_18AgariSR02",
			ItemID = ItemID.Item18AgariSR02,
			ShortName = "アガリ「スマッシュ」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara18,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_18AgariSR03",
			ItemID = ItemID.Item18AgariSR03,
			ShortName = "アガリ「バニー」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara18,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_18AgariSR04",
			ItemID = ItemID.Item18AgariSR04,
			ShortName = "アガリ「コミカルスマッシュ」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara18,
			IsComical = true
		},
		new Item
		{
			ObjectID = "OID_18AgariSR05",
			ItemID = ItemID.Item18AgariSR05,
			ShortName = "アガリ「デビュー」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara18,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_18AgariSR06",
			ItemID = ItemID.Item18AgariSR06,
			ShortName = "アガリ「水着」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara18,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_18NakiA01",
			ItemID = ItemID.Item18NakiA01,
			ShortName = "鳴き「プチキャラ（じまんのおねぇ♪）」",
			Category = Category.Naki,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara18,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_18NakiN01",
			ItemID = ItemID.Item18NakiN01,
			ShortName = "鳴き「プチキャラ（黄金比）」",
			Category = Category.Naki,
			Rarity = Rarity.N,
			CharaType = CharaType.Chara18,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_18NakiN02",
			ItemID = ItemID.Item18NakiN02,
			ShortName = "鳴き「プチキャラ（MySweetハンニバル）」",
			Category = Category.Naki,
			Rarity = Rarity.N,
			CharaType = CharaType.Chara18,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_18NakiN03",
			ItemID = ItemID.Item18NakiN03,
			ShortName = "鳴き「プチキャラ（イッパツ入魂）」",
			Category = Category.Naki,
			Rarity = Rarity.N,
			CharaType = CharaType.Chara18,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_18ReachA01",
			ItemID = ItemID.Item18ReachA01,
			ShortName = "リーチ「プチキャラ」",
			Category = Category.Reach,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara18,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_18ReachR01",
			ItemID = ItemID.Item18ReachR01,
			ShortName = "リーチ「スタンダード」",
			Category = Category.Reach,
			Rarity = Rarity.R,
			CharaType = CharaType.Chara18,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_18ReachR02",
			ItemID = ItemID.Item18ReachR02,
			ShortName = "リーチ「コミカル」",
			Category = Category.Reach,
			Rarity = Rarity.R,
			CharaType = CharaType.Chara18,
			IsComical = true
		},
		new Item
		{
			ObjectID = "OID_04AgariSR14",
			ItemID = ItemID.Item04AgariSR14,
			ShortName = "アガリ「テニスウェア」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara04,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_04AgariSR15",
			ItemID = ItemID.Item04AgariSR15,
			ShortName = "アガリ「テニスウェア（帝役）」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara04,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_16AgariSR09",
			ItemID = ItemID.Item16AgariSR09,
			ShortName = "アガリ「バレーボールウェア(帝役)」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara16,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_16AgariSR10",
			ItemID = ItemID.Item16AgariSR10,
			ShortName = "アガリ「バレーボールウェア(ママさん)」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara16,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_07AgariSR16",
			ItemID = ItemID.Item07AgariSR16,
			ShortName = "アガリ「雀喫茶メイド」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara07,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_13AgariSR11",
			ItemID = ItemID.Item13AgariSR11,
			ShortName = "アガリ「雀喫茶メイド」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara13,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_07AgariSR17",
			ItemID = ItemID.Item07AgariSR17,
			ShortName = "アガリ「雀喫茶メイド(スペースノワール)」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara07,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_13AgariSR12",
			ItemID = ItemID.Item13AgariSR12,
			ShortName = "アガリ「雀喫茶メイド(ハニービー)」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara13,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_17AgariSR07",
			ItemID = ItemID.Item17AgariSR07,
			ShortName = "アガリ「ランニングウェア(サイダーブルー)」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara17,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_17AgariSR08",
			ItemID = ItemID.Item17AgariSR08,
			ShortName = "アガリ「ランニングウェア(カロリーオレンジ)」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara17,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_08AgariSR12",
			ItemID = ItemID.Item08AgariSR12,
			ShortName = "アガリ「雀アイドル」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara08,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_19AgariSR01",
			ItemID = ItemID.Item19AgariSR01,
			ShortName = "アガリ「スタンダード」",
			Category = Category.AgariSmall,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara19,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_19AgariR01",
			ItemID = ItemID.Item19AgariR01,
			ShortName = "アガリ「コミカル」",
			Category = Category.AgariSmall,
			Rarity = Rarity.R,
			CharaType = CharaType.Chara19,
			IsComical = true
		},
		new Item
		{
			ObjectID = "OID_19AgariSR02",
			ItemID = ItemID.Item19AgariSR02,
			ShortName = "アガリ「スマッシュ」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara19,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_19AgariSR03",
			ItemID = ItemID.Item19AgariSR03,
			ShortName = "アガリ「バニー」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara19,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_19AgariSR04",
			ItemID = ItemID.Item19AgariSR04,
			ShortName = "アガリ「コミカルスマッシュ」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara19,
			IsComical = true
		},
		new Item
		{
			ObjectID = "OID_19AgariSR05",
			ItemID = ItemID.Item19AgariSR05,
			ShortName = "アガリ「デビュー」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara19,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_19AgariSR06",
			ItemID = ItemID.Item19AgariSR06,
			ShortName = "アガリ「水着」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara19,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_19NakiA01",
			ItemID = ItemID.Item19NakiA01,
			ShortName = "鳴き「プチキャラ（カレー…作りすぎちゃった）」",
			Category = Category.Naki,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara19,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_19NakiN01",
			ItemID = ItemID.Item19NakiN01,
			ShortName = "鳴き「プチキャラ（よしよし…♪）」",
			Category = Category.Naki,
			Rarity = Rarity.N,
			CharaType = CharaType.Chara19,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_19NakiN02",
			ItemID = ItemID.Item19NakiN02,
			ShortName = "鳴き「プチキャラ（立派なダイコン…♡）」",
			Category = Category.Naki,
			Rarity = Rarity.N,
			CharaType = CharaType.Chara19,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_19NakiN03",
			ItemID = ItemID.Item19NakiN03,
			ShortName = "鳴き「プチキャラ（おかいもの♪）」",
			Category = Category.Naki,
			Rarity = Rarity.N,
			CharaType = CharaType.Chara19,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_19ReachA01",
			ItemID = ItemID.Item19ReachA01,
			ShortName = "リーチ「プチキャラ」",
			Category = Category.Reach,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara19,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_19ReachR01",
			ItemID = ItemID.Item19ReachR01,
			ShortName = "リーチ「スタンダード」",
			Category = Category.Reach,
			Rarity = Rarity.R,
			CharaType = CharaType.Chara19,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_19ReachR02",
			ItemID = ItemID.Item19ReachR02,
			ShortName = "リーチ「コミカル」",
			Category = Category.Reach,
			Rarity = Rarity.R,
			CharaType = CharaType.Chara19,
			IsComical = true
		},
		new Item
		{
			ObjectID = "OID_14AgariSR09",
			ItemID = ItemID.Item14AgariSR09,
			ShortName = "アガリ「クリスマス」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara14,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_14AgariSR10",
			ItemID = ItemID.Item14AgariSR10,
			ShortName = "アガリ「クリスマス(ナイトメア)」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara14,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_01AgariSR12",
			ItemID = ItemID.Item01AgariSR12,
			ShortName = "アガリ「マーチングバンド」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara01,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_02AgariSR15",
			ItemID = ItemID.Item02AgariSR15,
			ShortName = "アガリ「マーチングバンド」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara02,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_05AgariSR17",
			ItemID = ItemID.Item05AgariSR17,
			ShortName = "アガリ「マーチングバンド」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara05,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_12AgariSR10",
			ItemID = ItemID.Item12AgariSR10,
			ShortName = "アガリ「初詣(白百合)」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara12,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_12AgariSR11",
			ItemID = ItemID.Item12AgariSR11,
			ShortName = "アガリ「初詣(黒百合)」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara12,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_18AgariSR07",
			ItemID = ItemID.Item18AgariSR07,
			ShortName = "アガリ「りりぃコス」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara18,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_18AgariSR08",
			ItemID = ItemID.Item18AgariSR08,
			ShortName = "アガリ「りりぃコス(ホワイト)」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara18,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_14AgariSR11",
			ItemID = ItemID.Item14AgariSR11,
			ShortName = "アガリ「雀喫茶メイド」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara14,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_14AgariSR12",
			ItemID = ItemID.Item14AgariSR12,
			ShortName = "アガリ「サキュバスバレエ」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara14,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_01AgariSR13",
			ItemID = ItemID.Item01AgariSR13,
			ShortName = "アガリ「バレンタイン(ミルク)」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara01,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_01AgariSR14",
			ItemID = ItemID.Item01AgariSR14,
			ShortName = "アガリ「バレンタイン(ビター)」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara01,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_06AgariSR14",
			ItemID = ItemID.Item06AgariSR14,
			ShortName = "アガリ「雀アイドル」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara06,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_20AgariSR01",
			ItemID = ItemID.Item20AgariSR01,
			ShortName = "アガリ「スタンダード」",
			Category = Category.AgariSmall,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara20,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_20AgariR01",
			ItemID = ItemID.Item20AgariR01,
			ShortName = "アガリ「コミカル」",
			Category = Category.AgariSmall,
			Rarity = Rarity.R,
			CharaType = CharaType.Chara20,
			IsComical = true
		},
		new Item
		{
			ObjectID = "OID_20AgariSR02",
			ItemID = ItemID.Item20AgariSR02,
			ShortName = "アガリ「スマッシュ」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara20,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_20AgariSR03",
			ItemID = ItemID.Item20AgariSR03,
			ShortName = "アガリ「バニー」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara20,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_20AgariSR04",
			ItemID = ItemID.Item20AgariSR04,
			ShortName = "アガリ「コミカルスマッシュ」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara20,
			IsComical = true
		},
		new Item
		{
			ObjectID = "OID_20AgariSR05",
			ItemID = ItemID.Item20AgariSR05,
			ShortName = "アガリ「デビュー」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara20,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_20AgariSR06",
			ItemID = ItemID.Item20AgariSR06,
			ShortName = "アガリ「水着」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara20,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_20NakiA01",
			ItemID = ItemID.Item20NakiA01,
			ShortName = "鳴き「プチキャラ（おにうでっ！）」",
			Category = Category.Naki,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara20,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_20NakiN01",
			ItemID = ItemID.Item20NakiN01,
			ShortName = "鳴き「プチキャラ（ぷんすこっ）」",
			Category = Category.Naki,
			Rarity = Rarity.N,
			CharaType = CharaType.Chara20,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_20NakiN02",
			ItemID = ItemID.Item20NakiN02,
			ShortName = "鳴き「プチキャラ（おにオニ♪）」",
			Category = Category.Naki,
			Rarity = Rarity.N,
			CharaType = CharaType.Chara20,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_20NakiN03",
			ItemID = ItemID.Item20NakiN03,
			ShortName = "鳴き「プチキャラ（鬼ヤクい！）」",
			Category = Category.Naki,
			Rarity = Rarity.N,
			CharaType = CharaType.Chara20,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_20ReachA01",
			ItemID = ItemID.Item20ReachA01,
			ShortName = "リーチ「プチキャラ」",
			Category = Category.Reach,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara20,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_20ReachR01",
			ItemID = ItemID.Item20ReachR01,
			ShortName = "リーチ「スタンダード」",
			Category = Category.Reach,
			Rarity = Rarity.R,
			CharaType = CharaType.Chara20,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_20ReachR02",
			ItemID = ItemID.Item20ReachR02,
			ShortName = "リーチ「コミカル」",
			Category = Category.Reach,
			Rarity = Rarity.R,
			CharaType = CharaType.Chara20,
			IsComical = true
		},
		new Item
		{
			ObjectID = "OID_02AgariSR16",
			ItemID = ItemID.Item02AgariSR16,
			ShortName = "アガリ「ネリマギャル」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara02,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_02AgariSR17",
			ItemID = ItemID.Item02AgariSR17,
			ShortName = "アガリ「ネリマギャル(カヌレ)」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara02,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_15AgariSR09",
			ItemID = ItemID.Item15AgariSR09,
			ShortName = "アガリ「ブクロギャル」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara15,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_15AgariSR10",
			ItemID = ItemID.Item15AgariSR10,
			ShortName = "アガリ「ブクロギャル(オニキス)」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara15,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_16AgariSR11",
			ItemID = ItemID.Item16AgariSR11,
			ShortName = "アガリ「ナガタチョウギャル」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara16,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_16AgariSR12",
			ItemID = ItemID.Item16AgariSR12,
			ShortName = "アガリ「ナガタチョウギャル(ラベンダー)」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara16,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_01AgariSR15",
			ItemID = ItemID.Item01AgariSR15,
			ShortName = "アガリ「ネリマギャル(ショコラ)」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara01,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_12AgariSR12",
			ItemID = ItemID.Item12AgariSR12,
			ShortName = "アガリ「セイジョウギャル(マーガレット)」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara12,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_14AgariSR13",
			ItemID = ItemID.Item14AgariSR13,
			ShortName = "アガリ「サキュバスギャル(アロエ)」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara14,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_11AgariSR10",
			ItemID = ItemID.Item11AgariSR10,
			ShortName = "アガリ「スパイスーツ」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara11,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_11AgariSR11",
			ItemID = ItemID.Item11AgariSR11,
			ShortName = "アガリ「スパイスーツ(ピンク)」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara11,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_19AgariSR07",
			ItemID = ItemID.Item19AgariSR07,
			ShortName = "アガリ「バレーボールウェア(ママさん)」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara19,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_19AgariSR08",
			ItemID = ItemID.Item19AgariSR08,
			ShortName = "アガリ「バレーボールウェア(帝役)」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara19,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_08AgariSR13",
			ItemID = ItemID.Item08AgariSR13,
			ShortName = "アガリ「袴メイド」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara08,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_08AgariSR14",
			ItemID = ItemID.Item08AgariSR14,
			ShortName = "アガリ「袴メイド(抹茶)」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara08,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_03AgariSR13",
			ItemID = ItemID.Item03AgariSR13,
			ShortName = "アガリ「弐女スクール水着」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara03,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_03AgariSR14",
			ItemID = ItemID.Item03AgariSR14,
			ShortName = "アガリ「弐女スクール水着(金)」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara03,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_01NakiA04",
			ItemID = ItemID.Item01NakiA04,
			ShortName = "鳴き「プチキャラ（雨の日）」",
			Category = Category.Naki,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara01,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_02NakiA04",
			ItemID = ItemID.Item02NakiA04,
			ShortName = "鳴き「プチキャラ（雨の日）」",
			Category = Category.Naki,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara02,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_03NakiA04",
			ItemID = ItemID.Item03NakiA04,
			ShortName = "鳴き「プチキャラ（雨の日）」",
			Category = Category.Naki,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara03,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_04NakiA04",
			ItemID = ItemID.Item04NakiA04,
			ShortName = "鳴き「プチキャラ（雨の日）」",
			Category = Category.Naki,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara04,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_05NakiA04",
			ItemID = ItemID.Item05NakiA04,
			ShortName = "鳴き「プチキャラ（雨の日）」",
			Category = Category.Naki,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara05,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_06NakiA04",
			ItemID = ItemID.Item06NakiA04,
			ShortName = "鳴き「プチキャラ（雨の日）」",
			Category = Category.Naki,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara06,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_07NakiA04",
			ItemID = ItemID.Item07NakiA04,
			ShortName = "鳴き「プチキャラ（雨の日）」",
			Category = Category.Naki,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara07,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_08NakiA04",
			ItemID = ItemID.Item08NakiA04,
			ShortName = "鳴き「プチキャラ（雨の日）」",
			Category = Category.Naki,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara08,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_09NakiA04",
			ItemID = ItemID.Item09NakiA04,
			ShortName = "鳴き「プチキャラ（雨の日）」",
			Category = Category.Naki,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara09,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_10NakiA04",
			ItemID = ItemID.Item10NakiA04,
			ShortName = "鳴き「プチキャラ（雨の日）」",
			Category = Category.Naki,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara10,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_11NakiA04",
			ItemID = ItemID.Item11NakiA04,
			ShortName = "鳴き「プチキャラ（雨の日）」",
			Category = Category.Naki,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara11,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_12NakiA04",
			ItemID = ItemID.Item12NakiA04,
			ShortName = "鳴き「プチキャラ（雨の日）」",
			Category = Category.Naki,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara12,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_13NakiA04",
			ItemID = ItemID.Item13NakiA04,
			ShortName = "鳴き「プチキャラ（雨の日）」",
			Category = Category.Naki,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara13,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_14NakiA04",
			ItemID = ItemID.Item14NakiA04,
			ShortName = "鳴き「プチキャラ（雨の日）」",
			Category = Category.Naki,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara14,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_15NakiA03",
			ItemID = ItemID.Item15NakiA03,
			ShortName = "鳴き「プチキャラ（雨の日）」",
			Category = Category.Naki,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara15,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_16NakiA03",
			ItemID = ItemID.Item16NakiA03,
			ShortName = "鳴き「プチキャラ（雨の日）」",
			Category = Category.Naki,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara16,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_17NakiA03",
			ItemID = ItemID.Item17NakiA03,
			ShortName = "鳴き「プチキャラ（雨の日）」",
			Category = Category.Naki,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara17,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_18NakiA02",
			ItemID = ItemID.Item18NakiA02,
			ShortName = "鳴き「プチキャラ（雨の日）」",
			Category = Category.Naki,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara18,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_19NakiA02",
			ItemID = ItemID.Item19NakiA02,
			ShortName = "鳴き「プチキャラ（雨の日）」",
			Category = Category.Naki,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara19,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_20NakiA02",
			ItemID = ItemID.Item20NakiA02,
			ShortName = "鳴き「プチキャラ（雨の日）」",
			Category = Category.Naki,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara20,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_07AgariSR18",
			ItemID = ItemID.Item07AgariSR18,
			ShortName = "アガリ「雀アイドル」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara07,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_16AgariSR13",
			ItemID = ItemID.Item16AgariSR13,
			ShortName = "アガリ「トレイン(黒)」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara16,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_16AgariSR14",
			ItemID = ItemID.Item16AgariSR14,
			ShortName = "アガリ「トレイン(白)」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara16,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_21AgariSR01",
			ItemID = ItemID.Item21AgariSR01,
			ShortName = "アガリ「スタンダード」",
			Category = Category.AgariSmall,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara21,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_21AgariR01",
			ItemID = ItemID.Item21AgariR01,
			ShortName = "アガリ「コミカル」",
			Category = Category.AgariSmall,
			Rarity = Rarity.R,
			CharaType = CharaType.Chara21,
			IsComical = true
		},
		new Item
		{
			ObjectID = "OID_21AgariSR02",
			ItemID = ItemID.Item21AgariSR02,
			ShortName = "アガリ「スマッシュ」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara21,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_21AgariSR03",
			ItemID = ItemID.Item21AgariSR03,
			ShortName = "アガリ「バニー」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara21,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_21AgariSR04",
			ItemID = ItemID.Item21AgariSR04,
			ShortName = "アガリ「コミカルスマッシュ」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara21,
			IsComical = true
		},
		new Item
		{
			ObjectID = "OID_21AgariSR05",
			ItemID = ItemID.Item21AgariSR05,
			ShortName = "アガリ「デビュー」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara21,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_21AgariSR06",
			ItemID = ItemID.Item21AgariSR06,
			ShortName = "アガリ「水着」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara21,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_21NakiA01",
			ItemID = ItemID.Item21NakiA01,
			ShortName = "鳴き「プチキャラ（ヴァンパイア！）」",
			Category = Category.Naki,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara21,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_21NakiN01",
			ItemID = ItemID.Item21NakiN01,
			ShortName = "鳴き「プチキャラ（吸わせて～♡）」",
			Category = Category.Naki,
			Rarity = Rarity.N,
			CharaType = CharaType.Chara21,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_21NakiN02",
			ItemID = ItemID.Item21NakiN02,
			ShortName = "鳴き「プチキャラ（飲む～？）」",
			Category = Category.Naki,
			Rarity = Rarity.N,
			CharaType = CharaType.Chara21,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_21NakiN03",
			ItemID = ItemID.Item21NakiN03,
			ShortName = "鳴き「プチキャラ（おめかし完了！）」",
			Category = Category.Naki,
			Rarity = Rarity.N,
			CharaType = CharaType.Chara21,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_21ReachA01",
			ItemID = ItemID.Item21ReachA01,
			ShortName = "リーチ「プチキャラ」",
			Category = Category.Reach,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara21,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_21ReachR01",
			ItemID = ItemID.Item21ReachR01,
			ShortName = "リーチ「スタンダード」",
			Category = Category.Reach,
			Rarity = Rarity.R,
			CharaType = CharaType.Chara21,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_21ReachR02",
			ItemID = ItemID.Item21ReachR02,
			ShortName = "リーチ「コミカル」",
			Category = Category.Reach,
			Rarity = Rarity.R,
			CharaType = CharaType.Chara21,
			IsComical = true
		},
		new Item
		{
			ObjectID = "OID_20AgariSR07",
			ItemID = ItemID.Item20AgariSR07,
			ShortName = "アガリ「雀喫茶メイド」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara20,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_20AgariSR08",
			ItemID = ItemID.Item20AgariSR08,
			ShortName = "アガリ「雀喫茶メイド(カワサキピンク)」",
			Category = Category.AgariBig,
			Rarity = Rarity.SR,
			CharaType = CharaType.Chara20,
			IsComical = false
		},
		new Item
		{
			ObjectID = "OID_21NakiA02",
			ItemID = ItemID.Item21NakiA02,
			ShortName = "鳴き「プチキャラ（雨の日）」",
			Category = Category.Naki,
			Rarity = Rarity.Nothing,
			CharaType = CharaType.Chara21,
			IsComical = false
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

	public static ItemID GetItemID(string oid)
	{
		return _items.Find((Item x) => x.ObjectID == oid)?.ItemID ?? ItemID.None;
	}

	public static List<ItemID> GetDefaultItemIDs()
	{
		return _defaultItems;
	}

	public static List<ItemID> GetGachaCharaCpuAdditionalItemIDs()
	{
		return GachaUtility.CharaUnlockItemIDDict.Values.ToList();
	}

	public static bool IsDefaultItemID(ItemID itemID)
	{
		return _defaultItems.Exists((ItemID x) => x == itemID);
	}

	public static int GetItemDataNumExceptDefaultItem(CharaType charaType)
	{
		return _items.Count((Item a) => a.CharaType == charaType && a.IsDetachable);
	}

	public static bool IsAppearanceMovie(ItemID itemID)
	{
		return AppearanceMovieCutins.Values.ToList().Exists((ItemID x) => x == itemID);
	}

	public static bool IsTohoCollaboration(ItemID itemID)
	{
		if (itemID != ItemID.Item01AgariSR07 && itemID != ItemID.Item04AgariSR09)
		{
			return itemID == ItemID.Item12AgariSR07;
		}
		return true;
	}

	public static bool IsPonnomichi(ItemID itemID)
	{
		return itemID == ItemID.Item04AgariA03;
	}
}
