using System;
using System.Collections.Generic;
using MFG.GameData;
using MFG.Types;
using MFG.Types.Customize.Costume;
using MFG.Types.Customize.CutinItem;

namespace MFG.MasterGameData;

public class CostumeItemMaster : MasterGameDataBase
{
	private static Dictionary<CharaType, MFG.Types.Customize.Costume.ItemID> m_defaultEquipItems = new Dictionary<CharaType, MFG.Types.Customize.Costume.ItemID>
	{
		{
			CharaType.Chara01,
			MFG.Types.Customize.Costume.ItemID.Costume01_001
		},
		{
			CharaType.Chara02,
			MFG.Types.Customize.Costume.ItemID.Costume02_001
		},
		{
			CharaType.Chara03,
			MFG.Types.Customize.Costume.ItemID.Costume03_001
		},
		{
			CharaType.Chara04,
			MFG.Types.Customize.Costume.ItemID.Costume04_001
		},
		{
			CharaType.Chara05,
			MFG.Types.Customize.Costume.ItemID.Costume05_001
		},
		{
			CharaType.Chara06,
			MFG.Types.Customize.Costume.ItemID.Costume06_001
		},
		{
			CharaType.Chara07,
			MFG.Types.Customize.Costume.ItemID.Costume07_001
		},
		{
			CharaType.Chara08,
			MFG.Types.Customize.Costume.ItemID.Costume08_001
		},
		{
			CharaType.Chara09,
			MFG.Types.Customize.Costume.ItemID.Costume09_001
		},
		{
			CharaType.Chara10,
			MFG.Types.Customize.Costume.ItemID.Costume10_001
		},
		{
			CharaType.Chara11,
			MFG.Types.Customize.Costume.ItemID.Costume11_001
		},
		{
			CharaType.Chara12,
			MFG.Types.Customize.Costume.ItemID.Costume12_001
		},
		{
			CharaType.Chara13,
			MFG.Types.Customize.Costume.ItemID.Costume13_001
		},
		{
			CharaType.Chara14,
			MFG.Types.Customize.Costume.ItemID.Costume14_001
		},
		{
			CharaType.Chara15,
			MFG.Types.Customize.Costume.ItemID.Costume15_001
		},
		{
			CharaType.Chara16,
			MFG.Types.Customize.Costume.ItemID.Costume16_001
		},
		{
			CharaType.Chara17,
			MFG.Types.Customize.Costume.ItemID.Costume17_001
		},
		{
			CharaType.Chara18,
			MFG.Types.Customize.Costume.ItemID.Costume18_001
		},
		{
			CharaType.Chara19,
			MFG.Types.Customize.Costume.ItemID.Costume19_001
		},
		{
			CharaType.Chara20,
			MFG.Types.Customize.Costume.ItemID.Costume20_001
		},
		{
			CharaType.Chara21,
			MFG.Types.Customize.Costume.ItemID.Costume21_001
		}
	};

	public static Dictionary<CharaType, MFG.Types.Customize.Costume.ItemID> m_debutCostumeItems = new Dictionary<CharaType, MFG.Types.Customize.Costume.ItemID>
	{
		{
			CharaType.Chara01,
			MFG.Types.Customize.Costume.ItemID.Costume01_002
		},
		{
			CharaType.Chara02,
			MFG.Types.Customize.Costume.ItemID.Costume02_002
		},
		{
			CharaType.Chara03,
			MFG.Types.Customize.Costume.ItemID.Costume03_002
		},
		{
			CharaType.Chara04,
			MFG.Types.Customize.Costume.ItemID.Costume04_002
		},
		{
			CharaType.Chara05,
			MFG.Types.Customize.Costume.ItemID.Costume05_002
		},
		{
			CharaType.Chara06,
			MFG.Types.Customize.Costume.ItemID.Costume06_002
		},
		{
			CharaType.Chara07,
			MFG.Types.Customize.Costume.ItemID.Costume07_002
		},
		{
			CharaType.Chara08,
			MFG.Types.Customize.Costume.ItemID.Costume08_002
		},
		{
			CharaType.Chara09,
			MFG.Types.Customize.Costume.ItemID.Costume09_002
		},
		{
			CharaType.Chara10,
			MFG.Types.Customize.Costume.ItemID.Costume10_002
		},
		{
			CharaType.Chara11,
			MFG.Types.Customize.Costume.ItemID.Costume11_002
		},
		{
			CharaType.Chara12,
			MFG.Types.Customize.Costume.ItemID.Costume12_002
		},
		{
			CharaType.Chara13,
			MFG.Types.Customize.Costume.ItemID.Costume13_002
		},
		{
			CharaType.Chara14,
			MFG.Types.Customize.Costume.ItemID.Costume14_002
		},
		{
			CharaType.Chara15,
			MFG.Types.Customize.Costume.ItemID.Costume15_002
		},
		{
			CharaType.Chara16,
			MFG.Types.Customize.Costume.ItemID.Costume16_002
		},
		{
			CharaType.Chara17,
			MFG.Types.Customize.Costume.ItemID.Costume17_002
		},
		{
			CharaType.Chara18,
			MFG.Types.Customize.Costume.ItemID.Costume18_002
		},
		{
			CharaType.Chara19,
			MFG.Types.Customize.Costume.ItemID.Costume19_002
		},
		{
			CharaType.Chara20,
			MFG.Types.Customize.Costume.ItemID.Costume20_002
		},
		{
			CharaType.Chara21,
			MFG.Types.Customize.Costume.ItemID.Costume21_002
		}
	};

	public static Dictionary<MFG.Types.Customize.Costume.ItemID, List<MFG.Types.Customize.Costume.ItemID>> m_sameTimeUnlockItems = new Dictionary<MFG.Types.Customize.Costume.ItemID, List<MFG.Types.Customize.Costume.ItemID>> { 
	{
		MFG.Types.Customize.Costume.ItemID.Costume10_002,
		new List<MFG.Types.Customize.Costume.ItemID> { MFG.Types.Customize.Costume.ItemID.Costume10_004 }
	} };

	private static List<MFG.Types.Customize.Costume.ItemID> m_defaultPossessionItems = new List<MFG.Types.Customize.Costume.ItemID>
	{
		MFG.Types.Customize.Costume.ItemID.Costume01_001,
		MFG.Types.Customize.Costume.ItemID.Costume02_001,
		MFG.Types.Customize.Costume.ItemID.Costume03_001,
		MFG.Types.Customize.Costume.ItemID.Costume04_001,
		MFG.Types.Customize.Costume.ItemID.Costume05_001,
		MFG.Types.Customize.Costume.ItemID.Costume06_001,
		MFG.Types.Customize.Costume.ItemID.Costume07_001,
		MFG.Types.Customize.Costume.ItemID.Costume08_001,
		MFG.Types.Customize.Costume.ItemID.Costume09_001,
		MFG.Types.Customize.Costume.ItemID.Costume10_001,
		MFG.Types.Customize.Costume.ItemID.Costume10_003,
		MFG.Types.Customize.Costume.ItemID.Costume11_001,
		MFG.Types.Customize.Costume.ItemID.Costume12_001,
		MFG.Types.Customize.Costume.ItemID.Costume13_001,
		MFG.Types.Customize.Costume.ItemID.Costume14_001,
		MFG.Types.Customize.Costume.ItemID.Costume15_001,
		MFG.Types.Customize.Costume.ItemID.Costume16_001,
		MFG.Types.Customize.Costume.ItemID.Costume17_001,
		MFG.Types.Customize.Costume.ItemID.Costume18_001,
		MFG.Types.Customize.Costume.ItemID.Costume19_001,
		MFG.Types.Customize.Costume.ItemID.Costume20_001,
		MFG.Types.Customize.Costume.ItemID.Costume21_001
	};

	private static List<MFG.Types.Customize.Costume.Item> m_items = new List<MFG.Types.Customize.Costume.Item>
	{
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume01_001",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume01_001,
			CharaType = CharaType.Chara01,
			Name = "制服",
			UnlockFightSpiritNum = 0,
			UnlockEventType = GameEventType.None,
			SortID = SortID.Default
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume01_002",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume01_002,
			CharaType = CharaType.Chara01,
			Name = "デビューウェア",
			UnlockFightSpiritNum = 100,
			UnlockEventType = GameEventType.None,
			SortID = SortID.Debut
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume02_001",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume02_001,
			CharaType = CharaType.Chara02,
			Name = "制服",
			UnlockFightSpiritNum = 0,
			UnlockEventType = GameEventType.None,
			SortID = SortID.Default
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume02_002",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume02_002,
			CharaType = CharaType.Chara02,
			Name = "デビューウェア",
			UnlockFightSpiritNum = 100,
			UnlockEventType = GameEventType.None,
			SortID = SortID.Debut
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume03_001",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume03_001,
			CharaType = CharaType.Chara03,
			Name = "制服",
			UnlockFightSpiritNum = 0,
			UnlockEventType = GameEventType.None,
			SortID = SortID.Default
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume03_002",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume03_002,
			CharaType = CharaType.Chara03,
			Name = "デビューウェア",
			UnlockFightSpiritNum = 100,
			UnlockEventType = GameEventType.None,
			SortID = SortID.Debut
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume04_001",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume04_001,
			CharaType = CharaType.Chara04,
			Name = "制服",
			UnlockFightSpiritNum = 0,
			UnlockEventType = GameEventType.None,
			SortID = SortID.Default
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume04_002",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume04_002,
			CharaType = CharaType.Chara04,
			Name = "デビューウェア",
			UnlockFightSpiritNum = 100,
			UnlockEventType = GameEventType.None,
			SortID = SortID.Debut
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume05_001",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume05_001,
			CharaType = CharaType.Chara05,
			Name = "制服",
			UnlockFightSpiritNum = 0,
			UnlockEventType = GameEventType.None,
			SortID = SortID.Default
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume05_002",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume05_002,
			CharaType = CharaType.Chara05,
			Name = "デビューウェア",
			UnlockFightSpiritNum = 100,
			UnlockEventType = GameEventType.None,
			SortID = SortID.Debut
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume06_001",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume06_001,
			CharaType = CharaType.Chara06,
			Name = "制服",
			UnlockFightSpiritNum = 0,
			UnlockEventType = GameEventType.None,
			SortID = SortID.Default
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume06_002",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume06_002,
			CharaType = CharaType.Chara06,
			Name = "デビューウェア",
			UnlockFightSpiritNum = 100,
			UnlockEventType = GameEventType.None,
			SortID = SortID.Debut
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume07_001",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume07_001,
			CharaType = CharaType.Chara07,
			Name = "制服",
			UnlockFightSpiritNum = 0,
			UnlockEventType = GameEventType.None,
			SortID = SortID.Default
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume07_002",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume07_002,
			CharaType = CharaType.Chara07,
			Name = "デビューウェア",
			UnlockFightSpiritNum = 100,
			UnlockEventType = GameEventType.None,
			SortID = SortID.Debut
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume08_001",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume08_001,
			CharaType = CharaType.Chara08,
			Name = "制服",
			UnlockFightSpiritNum = 0,
			UnlockEventType = GameEventType.None,
			SortID = SortID.Default
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume08_002",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume08_002,
			CharaType = CharaType.Chara08,
			Name = "デビューウェア",
			UnlockFightSpiritNum = 100,
			UnlockEventType = GameEventType.None,
			SortID = SortID.Debut
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume09_001",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume09_001,
			CharaType = CharaType.Chara09,
			Name = "チャイナ",
			UnlockFightSpiritNum = 0,
			UnlockEventType = GameEventType.None,
			SortID = SortID.Default
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume09_002",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume09_002,
			CharaType = CharaType.Chara09,
			Name = "デビューウェア",
			UnlockFightSpiritNum = 100,
			UnlockEventType = GameEventType.None,
			SortID = SortID.Debut
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume02_003",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume02_003,
			CharaType = CharaType.Chara02,
			Name = "水着",
			UnlockFightSpiritNum = 100,
			UnlockEventType = GameEventType.CostumeAddition1,
			SortID = SortID.Mizugi
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume06_003",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume06_003,
			CharaType = CharaType.Chara06,
			Name = "水着",
			UnlockFightSpiritNum = 100,
			UnlockEventType = GameEventType.CostumeAddition2,
			SortID = SortID.Mizugi
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume01_003",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume01_003,
			CharaType = CharaType.Chara01,
			Name = "水着",
			UnlockFightSpiritNum = 100,
			UnlockEventType = GameEventType.CostumeAddition4,
			SortID = SortID.Mizugi
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume08_003",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume08_003,
			CharaType = CharaType.Chara08,
			Name = "水着",
			UnlockFightSpiritNum = 100,
			UnlockEventType = GameEventType.CostumeAddition3,
			SortID = SortID.Mizugi
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume04_003",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume04_003,
			CharaType = CharaType.Chara04,
			Name = "水着",
			UnlockFightSpiritNum = 100,
			UnlockEventType = GameEventType.CostumeAddition7,
			SortID = SortID.Mizugi
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume05_003",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume05_003,
			CharaType = CharaType.Chara05,
			Name = "水着",
			UnlockFightSpiritNum = 100,
			UnlockEventType = GameEventType.CostumeAddition5,
			SortID = SortID.Mizugi
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume05_004",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume05_004,
			CharaType = CharaType.Chara05,
			Name = "水着 (クールver.)",
			UnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition5,
			needUnlockItemID = MFG.Types.Customize.Costume.ItemID.Costume05_003,
			SortID = SortID.Mizugi
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume09_003",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume09_003,
			CharaType = CharaType.Chara09,
			Name = "水着",
			UnlockFightSpiritNum = 100,
			UnlockEventType = GameEventType.CostumeAddition6,
			SortID = SortID.Mizugi
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume09_004",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume09_004,
			CharaType = CharaType.Chara09,
			Name = "水着 (白衣ver.)",
			UnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition6,
			needUnlockItemID = MFG.Types.Customize.Costume.ItemID.Costume09_003,
			SortID = SortID.Mizugi
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume10_001",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume10_001,
			CharaType = CharaType.Chara10,
			Name = "きらめき高校制服",
			UnlockFightSpiritNum = 0,
			UnlockEventType = GameEventType.None,
			SortID = SortID.Default
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume10_002",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume10_002,
			CharaType = CharaType.Chara10,
			Name = "デビューウェア",
			UnlockFightSpiritNum = 100,
			UnlockEventType = GameEventType.None,
			SortID = SortID.Debut
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume03_003",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume03_003,
			CharaType = CharaType.Chara03,
			Name = "水着",
			UnlockFightSpiritNum = 100,
			UnlockEventType = GameEventType.CostumeAddition8,
			SortID = SortID.Mizugi
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume07_003",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume07_003,
			CharaType = CharaType.Chara07,
			Name = "水着",
			UnlockFightSpiritNum = 100,
			UnlockEventType = GameEventType.CostumeAddition10,
			SortID = SortID.Mizugi
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume07_004",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume07_004,
			CharaType = CharaType.Chara07,
			Name = "水着 (パージver.)",
			UnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition10,
			needUnlockItemID = MFG.Types.Customize.Costume.ItemID.Costume07_003,
			SortID = SortID.Mizugi
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume06_004",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume06_004,
			CharaType = CharaType.Chara06,
			Name = "シンデレラ",
			UnlockFightSpiritNum = 100,
			UnlockEventType = GameEventType.CostumeAddition9,
			SortID = SortID.Sort1
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume01_004",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume01_004,
			CharaType = CharaType.Chara01,
			Name = "水着 (NDKカラー)",
			UnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition11,
			needUnlockItemID = MFG.Types.Customize.Costume.ItemID.Costume01_003,
			SortID = SortID.Mizugi
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume02_004",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume02_004,
			CharaType = CharaType.Chara02,
			Name = "水着 (NDKカラー)",
			UnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition11,
			needUnlockItemID = MFG.Types.Customize.Costume.ItemID.Costume02_003,
			SortID = SortID.Mizugi
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume05_005",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume05_005,
			CharaType = CharaType.Chara05,
			Name = "水着 (NDKカラー)",
			UnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition11,
			needUnlockItemID = MFG.Types.Customize.Costume.ItemID.Costume05_003,
			SortID = SortID.Mizugi
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume04_004",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume04_004,
			CharaType = CharaType.Chara04,
			Name = "水着 (NBDカラー)",
			UnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition13,
			needUnlockItemID = MFG.Types.Customize.Costume.ItemID.Costume04_003,
			SortID = SortID.Mizugi
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume08_005",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume08_005,
			CharaType = CharaType.Chara08,
			Name = "水着 (NBDカラー)",
			UnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition13,
			needUnlockItemID = MFG.Types.Customize.Costume.ItemID.Costume08_003,
			SortID = SortID.Mizugi
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume09_005",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume09_005,
			CharaType = CharaType.Chara09,
			Name = "水着 (マッド)",
			UnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition13,
			needUnlockItemID = MFG.Types.Customize.Costume.ItemID.Costume09_003,
			SortID = SortID.Mizugi
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume10_003",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume10_003,
			CharaType = CharaType.Chara10,
			Name = "きらめき高校制服 (ファイトガールver.)",
			UnlockFightSpiritNum = 0,
			UnlockEventType = GameEventType.CostumeAddition12,
			SortID = SortID.Default
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume10_004",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume10_004,
			CharaType = CharaType.Chara10,
			Name = "デビューウェア (ファイトガールver.)",
			UnlockFightSpiritNum = 0,
			UnlockEventType = GameEventType.CostumeAddition12,
			needUnlockItemID = MFG.Types.Customize.Costume.ItemID.Costume10_002,
			SortID = SortID.Debut
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume05_006",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume05_006,
			CharaType = CharaType.Chara05,
			Name = "水着 (NDKクールver.)",
			UnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition11,
			needUnlockItemID = MFG.Types.Customize.Costume.ItemID.Costume05_003,
			SortID = SortID.Mizugi
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume09_006",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume09_006,
			CharaType = CharaType.Chara09,
			Name = "水着 (マッド白衣ver.)",
			UnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition13,
			needUnlockItemID = MFG.Types.Customize.Costume.ItemID.Costume09_003,
			SortID = SortID.Mizugi
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume03_004",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume03_004,
			CharaType = CharaType.Chara03,
			Name = "水着 (AFDカラー)",
			UnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition14,
			needUnlockItemID = MFG.Types.Customize.Costume.ItemID.Costume03_003,
			SortID = SortID.Mizugi
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume06_005",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume06_005,
			CharaType = CharaType.Chara06,
			Name = "水着 (AFDカラー)",
			UnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition14,
			needUnlockItemID = MFG.Types.Customize.Costume.ItemID.Costume06_003,
			SortID = SortID.Mizugi
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume07_005",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume07_005,
			CharaType = CharaType.Chara07,
			Name = "水着 (AFDカラー)",
			UnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition14,
			needUnlockItemID = MFG.Types.Customize.Costume.ItemID.Costume07_003,
			SortID = SortID.Mizugi
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume07_006",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume07_006,
			CharaType = CharaType.Chara07,
			Name = "水着 (AFDパージver.)",
			UnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition14,
			needUnlockItemID = MFG.Types.Customize.Costume.ItemID.Costume07_003,
			SortID = SortID.Mizugi
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume02_005",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume02_005,
			CharaType = CharaType.Chara02,
			Name = "雀喫茶メイド",
			UnlockFightSpiritNum = 100,
			UnlockEventType = GameEventType.CostumeAddition15,
			SortID = SortID.Sort1
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume08_004",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume08_004,
			CharaType = CharaType.Chara08,
			Name = "ハロウィン",
			UnlockFightSpiritNum = 100,
			UnlockEventType = GameEventType.CostumeAddition16,
			SortID = SortID.Sort1
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume11_001",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume11_001,
			CharaType = CharaType.Chara11,
			Name = "制服",
			UnlockFightSpiritNum = 0,
			UnlockEventType = GameEventType.None,
			SortID = SortID.Default
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume11_002",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume11_002,
			CharaType = CharaType.Chara11,
			Name = "デビューウェア",
			UnlockFightSpiritNum = 100,
			UnlockEventType = GameEventType.None,
			SortID = SortID.Debut
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume04_005",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume04_005,
			CharaType = CharaType.Chara04,
			Name = "クリスマス",
			UnlockFightSpiritNum = 100,
			UnlockEventType = GameEventType.CostumeAddition17,
			SortID = SortID.Sort1
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume03_005",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume03_005,
			CharaType = CharaType.Chara03,
			Name = "お正月",
			UnlockFightSpiritNum = 100,
			UnlockEventType = GameEventType.CostumeAddition18,
			SortID = SortID.Sort1
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume05_007",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume05_007,
			CharaType = CharaType.Chara05,
			Name = "お正月",
			UnlockFightSpiritNum = 100,
			UnlockEventType = GameEventType.CostumeAddition19,
			SortID = SortID.Sort1
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume07_007",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume07_007,
			CharaType = CharaType.Chara07,
			Name = "バレンタイン",
			UnlockFightSpiritNum = 100,
			UnlockEventType = GameEventType.CostumeAddition20,
			SortID = SortID.Sort1
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume12_001",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume12_001,
			CharaType = CharaType.Chara12,
			Name = "制服",
			UnlockFightSpiritNum = 0,
			UnlockEventType = GameEventType.None,
			SortID = SortID.Default
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume12_002",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume12_002,
			CharaType = CharaType.Chara12,
			Name = "デビューウェア",
			UnlockFightSpiritNum = 100,
			UnlockEventType = GameEventType.None,
			SortID = SortID.Debut
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume12_003",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume12_003,
			CharaType = CharaType.Chara12,
			Name = "仮面",
			UnlockFightSpiritNum = 100,
			UnlockEventType = GameEventType.CostumeAddition21,
			SortID = SortID.Unused
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume04_006",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume04_006,
			CharaType = CharaType.Chara04,
			Name = "サキュバスチアコス",
			UnlockFightSpiritNum = 100,
			UnlockEventType = GameEventType.CostumeAddition22,
			SortID = SortID.Sort2
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume03_006",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume03_006,
			CharaType = CharaType.Chara03,
			Name = "パロディウス サンバカーニバルコス",
			UnlockFightSpiritNum = 100,
			UnlockEventType = GameEventType.CostumeAddition23,
			SortID = SortID.Sort2
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume01_005",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume01_005,
			CharaType = CharaType.Chara01,
			Name = "レースクイーン",
			UnlockFightSpiritNum = 100,
			UnlockEventType = GameEventType.CostumeAddition24,
			SortID = SortID.Sort1
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume11_003",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume11_003,
			CharaType = CharaType.Chara11,
			Name = "水着",
			UnlockFightSpiritNum = 100,
			UnlockEventType = GameEventType.CostumeAddition25,
			SortID = SortID.Mizugi
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume09_007",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume09_007,
			CharaType = CharaType.Chara09,
			Name = "アリスメイド",
			UnlockFightSpiritNum = 100,
			UnlockEventType = GameEventType.CostumeAddition26,
			SortID = SortID.Sort1
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume09_008",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume09_008,
			CharaType = CharaType.Chara09,
			Name = "アリスメイド (ロイヤルカラー)",
			UnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition26,
			needUnlockItemID = MFG.Types.Customize.Costume.ItemID.Costume09_007,
			SortID = SortID.Sort1
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume12_004",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume12_004,
			CharaType = CharaType.Chara12,
			Name = "水着",
			UnlockFightSpiritNum = 100,
			UnlockEventType = GameEventType.CostumeAddition27,
			SortID = SortID.Mizugi
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume06_006",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume06_006,
			CharaType = CharaType.Chara06,
			Name = "暗黒部屋着",
			UnlockFightSpiritNum = 100,
			UnlockEventType = GameEventType.CostumeAddition28,
			SortID = SortID.Sort2
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume06_007",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume06_007,
			CharaType = CharaType.Chara06,
			Name = "暗黒部屋着 (パージver.)",
			UnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition28,
			needUnlockItemID = MFG.Types.Customize.Costume.ItemID.Costume06_006,
			SortID = SortID.Sort2
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume08_006",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume08_006,
			CharaType = CharaType.Chara08,
			Name = "役満高等学校体操服",
			UnlockFightSpiritNum = 100,
			UnlockEventType = GameEventType.CostumeAddition29,
			SortID = SortID.Sort2
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume02_006",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume02_006,
			CharaType = CharaType.Chara02,
			Name = "青春チアリーダー",
			UnlockFightSpiritNum = 100,
			UnlockEventType = GameEventType.CostumeAddition30,
			SortID = SortID.Sort2
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume02_007",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume02_007,
			CharaType = CharaType.Chara02,
			Name = "青春チアリーダー (ポンポン無しver.)",
			UnlockFightSpiritNum = 100,
			UnlockEventType = GameEventType.CostumeAddition30,
			SortID = SortID.Sort2
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume05_008",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume05_008,
			CharaType = CharaType.Chara05,
			Name = "壱役高等学校夏制服",
			UnlockFightSpiritNum = 100,
			UnlockEventType = GameEventType.CostumeAddition31,
			SortID = SortID.Sort2
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume07_008",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume07_008,
			CharaType = CharaType.Chara07,
			Name = "ゲーマー浴衣",
			UnlockFightSpiritNum = 100,
			UnlockEventType = GameEventType.CostumeAddition32,
			SortID = SortID.Sort2
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume13_001",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume13_001,
			CharaType = CharaType.Chara13,
			Name = "制服",
			UnlockFightSpiritNum = 0,
			UnlockEventType = GameEventType.None,
			SortID = SortID.Default
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume13_002",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume13_002,
			CharaType = CharaType.Chara13,
			Name = "デビューウェア",
			UnlockFightSpiritNum = 100,
			UnlockEventType = GameEventType.None,
			SortID = SortID.Debut
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume13_003",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume13_003,
			CharaType = CharaType.Chara13,
			Name = "水着",
			UnlockFightSpiritNum = 100,
			UnlockEventType = GameEventType.None,
			SortID = SortID.Mizugi
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume13_004",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume13_004,
			CharaType = CharaType.Chara13,
			Name = "デビューウェア",
			UnlockFightSpiritNum = 100,
			UnlockEventType = GameEventType.CostumeAddition33,
			SortID = SortID.Unused
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume01_006",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume01_006,
			CharaType = CharaType.Chara01,
			Name = "霊夢の巫女服",
			UnlockFightSpiritNum = 100,
			UnlockEventType = GameEventType.CostumeAddition34,
			SortID = SortID.Sort2
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume04_007",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume04_007,
			CharaType = CharaType.Chara04,
			Name = "フランのドレス",
			UnlockFightSpiritNum = 100,
			UnlockEventType = GameEventType.CostumeAddition35,
			SortID = SortID.Sort3
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume12_005",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume12_005,
			CharaType = CharaType.Chara12,
			Name = "八雲紫の道士服",
			UnlockFightSpiritNum = 100,
			UnlockEventType = GameEventType.CostumeAddition36,
			SortID = SortID.Sort1
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume02_008",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume02_008,
			CharaType = CharaType.Chara02,
			Name = "水着(小麦色ver.)",
			UnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition37,
			needUnlockItemID = MFG.Types.Customize.Costume.ItemID.Costume02_003,
			SortID = SortID.Mizugi
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume05_009",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume05_009,
			CharaType = CharaType.Chara05,
			Name = "水着(小麦色ver.)",
			UnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition37,
			needUnlockItemID = MFG.Types.Customize.Costume.ItemID.Costume05_003,
			SortID = SortID.Mizugi
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume06_008",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume06_008,
			CharaType = CharaType.Chara06,
			Name = "水着(小麦色ver.)",
			UnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition37,
			needUnlockItemID = MFG.Types.Customize.Costume.ItemID.Costume06_003,
			SortID = SortID.Mizugi
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume07_009",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume07_009,
			CharaType = CharaType.Chara07,
			Name = "水着(小麦色ver.)",
			UnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition37,
			needUnlockItemID = MFG.Types.Customize.Costume.ItemID.Costume07_003,
			SortID = SortID.Mizugi
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume09_009",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume09_009,
			CharaType = CharaType.Chara09,
			Name = "水着(小麦色ver.)",
			UnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition37,
			needUnlockItemID = MFG.Types.Customize.Costume.ItemID.Costume09_003,
			SortID = SortID.Mizugi
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume03_007",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume03_007,
			CharaType = CharaType.Chara03,
			Name = "慈愛シスター",
			UnlockFightSpiritNum = 100,
			UnlockEventType = GameEventType.CostumeAddition38,
			SortID = SortID.Sort3
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume03_008",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume03_008,
			CharaType = CharaType.Chara03,
			Name = "慈愛シスター (パージver.)",
			UnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition38,
			needUnlockItemID = MFG.Types.Customize.Costume.ItemID.Costume03_007,
			SortID = SortID.Sort3
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume02_009",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume02_009,
			CharaType = CharaType.Chara02,
			Name = "桃色チアリーダー",
			UnlockFightSpiritNum = 100,
			UnlockEventType = GameEventType.CostumeAddition39,
			SortID = SortID.Sort2
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume02_010",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume02_010,
			CharaType = CharaType.Chara02,
			Name = "桃色チアリーダー (ポンポン無しver.)",
			UnlockFightSpiritNum = 100,
			UnlockEventType = GameEventType.CostumeAddition39,
			SortID = SortID.Sort2
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume09_010",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume09_010,
			CharaType = CharaType.Chara09,
			Name = "プリティーボンバーズ",
			UnlockFightSpiritNum = 100,
			UnlockEventType = GameEventType.CostumeAddition40,
			SortID = SortID.Sort2
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume09_011",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume09_011,
			CharaType = CharaType.Chara09,
			Name = "プリティーボンバーズ (キラービーカラー)",
			UnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition40,
			needUnlockItemID = MFG.Types.Customize.Costume.ItemID.Costume09_010,
			SortID = SortID.Sort2
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume14_001",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume14_001,
			CharaType = CharaType.Chara14,
			Name = "チャイナ",
			UnlockFightSpiritNum = 0,
			UnlockEventType = GameEventType.None,
			SortID = SortID.Default
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume14_002",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume14_002,
			CharaType = CharaType.Chara14,
			Name = "デビューウェア",
			UnlockFightSpiritNum = 100,
			UnlockEventType = GameEventType.None,
			SortID = SortID.Debut
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume14_003",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume14_003,
			CharaType = CharaType.Chara14,
			Name = "水着",
			UnlockFightSpiritNum = 100,
			UnlockEventType = GameEventType.None,
			SortID = SortID.Mizugi
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume06_009",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume06_009,
			CharaType = CharaType.Chara06,
			Name = "ダークネスローブ",
			UnlockFightSpiritNum = 100,
			UnlockEventType = GameEventType.CostumeAddition41,
			SortID = SortID.Sort3
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume06_010",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume06_010,
			CharaType = CharaType.Chara06,
			Name = "ダークネスローブ (仮面ver.)",
			UnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition41,
			needUnlockItemID = MFG.Types.Customize.Costume.ItemID.Costume06_009,
			SortID = SortID.Sort3
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume11_004",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume11_004,
			CharaType = CharaType.Chara11,
			Name = "普通のナース",
			UnlockFightSpiritNum = 100,
			UnlockEventType = GameEventType.CostumeAddition42,
			SortID = SortID.Sort1
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume11_005",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume11_005,
			CharaType = CharaType.Chara11,
			Name = "普通の黒ナース",
			UnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition42,
			needUnlockItemID = MFG.Types.Customize.Costume.ItemID.Costume11_004,
			SortID = SortID.Sort1
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume15_001",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume15_001,
			CharaType = CharaType.Chara15,
			Name = "制服",
			UnlockFightSpiritNum = 0,
			UnlockEventType = GameEventType.None,
			SortID = SortID.Default
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume15_002",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume15_002,
			CharaType = CharaType.Chara15,
			Name = "デビューウェア",
			UnlockFightSpiritNum = 100,
			UnlockEventType = GameEventType.None,
			SortID = SortID.Debut
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume15_003",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume15_003,
			CharaType = CharaType.Chara15,
			Name = "水着",
			UnlockFightSpiritNum = 100,
			UnlockEventType = GameEventType.None,
			SortID = SortID.Mizugi
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume13_005",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume13_005,
			CharaType = CharaType.Chara13,
			Name = "キョンシー（白蛇）",
			UnlockFightSpiritNum = 100,
			UnlockEventType = GameEventType.CostumeAddition43,
			SortID = SortID.Sort1
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume13_006",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume13_006,
			CharaType = CharaType.Chara13,
			Name = "キョンシー（碧蛇）",
			UnlockFightSpiritNum = 100,
			UnlockEventType = GameEventType.CostumeAddition43,
			SortID = SortID.Sort1
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume04_008",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume04_008,
			CharaType = CharaType.Chara04,
			Name = "クリスマス (ブラックサンタ)",
			UnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition44,
			needUnlockItemID = MFG.Types.Customize.Costume.ItemID.Costume04_005,
			SortID = SortID.Sort1
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume08_007",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume08_007,
			CharaType = CharaType.Chara08,
			Name = "くみゃはっぴ(帝役)",
			UnlockFightSpiritNum = 100,
			UnlockEventType = GameEventType.CostumeAddition45,
			SortID = SortID.Sort3
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume08_008",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume08_008,
			CharaType = CharaType.Chara08,
			Name = "くみゃはっぴ(鍛錬)",
			UnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition45,
			needUnlockItemID = MFG.Types.Customize.Costume.ItemID.Costume08_007,
			SortID = SortID.Sort3
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume05_010",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume05_010,
			CharaType = CharaType.Chara05,
			Name = "アラビニャンダンサー",
			UnlockFightSpiritNum = 100,
			UnlockEventType = GameEventType.CostumeAddition46,
			SortID = SortID.Sort3
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume05_011",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume05_011,
			CharaType = CharaType.Chara05,
			Name = "アラビニャンダンサー(黒)",
			UnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition46,
			needUnlockItemID = MFG.Types.Customize.Costume.ItemID.Costume05_010,
			SortID = SortID.Sort3
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume07_010",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume07_010,
			CharaType = CharaType.Chara07,
			Name = "バレンタイン（対ありver.）",
			UnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition47,
			needUnlockItemID = MFG.Types.Customize.Costume.ItemID.Costume07_007,
			SortID = SortID.Sort1
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume04_009",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume04_009,
			CharaType = CharaType.Chara04,
			Name = "ホーリーエンジェル",
			UnlockFightSpiritNum = 100,
			UnlockEventType = GameEventType.CostumeAddition48,
			SortID = SortID.Sort3
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume04_010",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume04_010,
			CharaType = CharaType.Chara04,
			Name = "ホーリーエンジェル (仮面ver.)",
			UnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition48,
			needUnlockItemID = MFG.Types.Customize.Costume.ItemID.Costume04_009,
			SortID = SortID.Sort3
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume07_011",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume07_011,
			CharaType = CharaType.Chara07,
			Name = "ロリータドレス",
			UnlockFightSpiritNum = 100,
			UnlockEventType = GameEventType.CostumeAddition49,
			SortID = SortID.Sort3
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume07_012",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume07_012,
			CharaType = CharaType.Chara07,
			Name = "ロリータドレス（チョコミントver.）",
			UnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition49,
			needUnlockItemID = MFG.Types.Customize.Costume.ItemID.Costume07_011,
			SortID = SortID.Sort3
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume16_001",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume16_001,
			CharaType = CharaType.Chara16,
			Name = "教師",
			UnlockFightSpiritNum = 0,
			UnlockEventType = GameEventType.None,
			SortID = SortID.Default
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume16_002",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume16_002,
			CharaType = CharaType.Chara16,
			Name = "デビューウェア",
			UnlockFightSpiritNum = 100,
			UnlockEventType = GameEventType.None,
			SortID = SortID.Debut
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume16_003",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume16_003,
			CharaType = CharaType.Chara16,
			Name = "水着",
			UnlockFightSpiritNum = 100,
			UnlockEventType = GameEventType.None,
			SortID = SortID.Mizugi
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume01_007",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume01_007,
			CharaType = CharaType.Chara01,
			Name = "ネリマギャル",
			UnlockFightSpiritNum = 100,
			UnlockEventType = GameEventType.CostumeAddition50,
			SortID = SortID.Sort3
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume01_008",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume01_008,
			CharaType = CharaType.Chara01,
			Name = "ネリマギャル(キャラメル)",
			UnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition50,
			needUnlockItemID = MFG.Types.Customize.Costume.ItemID.Costume01_007,
			SortID = SortID.Sort3
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume12_006",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume12_006,
			CharaType = CharaType.Chara12,
			Name = "セイジョウギャル",
			UnlockFightSpiritNum = 100,
			UnlockEventType = GameEventType.CostumeAddition52,
			SortID = SortID.Sort2
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume12_007",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume12_007,
			CharaType = CharaType.Chara12,
			Name = "セイジョウギャル(ローズ)",
			UnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition52,
			needUnlockItemID = MFG.Types.Customize.Costume.ItemID.Costume12_006,
			SortID = SortID.Sort2
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume14_004",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume14_004,
			CharaType = CharaType.Chara14,
			Name = "サキュバスギャル",
			UnlockFightSpiritNum = 100,
			UnlockEventType = GameEventType.CostumeAddition51,
			SortID = SortID.Sort1
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume14_005",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume14_005,
			CharaType = CharaType.Chara14,
			Name = "サキュバスギャル(パンサー)",
			UnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition51,
			needUnlockItemID = MFG.Types.Customize.Costume.ItemID.Costume14_004,
			SortID = SortID.Sort1
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume15_004",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume15_004,
			CharaType = CharaType.Chara15,
			Name = "アニマルフォーム(しろ)",
			UnlockFightSpiritNum = 200,
			MitigationUnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition53,
			mitigationCutinItemIDs = new List<MFG.Types.Customize.CutinItem.ItemID>
			{
				MFG.Types.Customize.CutinItem.ItemID.Item15AgariSR07,
				MFG.Types.Customize.CutinItem.ItemID.Item15AgariSR08
			},
			SortID = SortID.Sort1
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume15_005",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume15_005,
			CharaType = CharaType.Chara15,
			Name = "アニマルフォーム(くろ)",
			UnlockFightSpiritNum = 200,
			MitigationUnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition53,
			mitigationCutinItemIDs = new List<MFG.Types.Customize.CutinItem.ItemID>
			{
				MFG.Types.Customize.CutinItem.ItemID.Item15AgariSR07,
				MFG.Types.Customize.CutinItem.ItemID.Item15AgariSR08
			},
			SortID = SortID.Sort1
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume08_009",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume08_009,
			CharaType = CharaType.Chara08,
			Name = "アニマルぱじゃま(くま)",
			UnlockFightSpiritNum = 200,
			MitigationUnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition54,
			mitigationCutinItemIDs = new List<MFG.Types.Customize.CutinItem.ItemID>
			{
				MFG.Types.Customize.CutinItem.ItemID.Item08AgariSR10,
				MFG.Types.Customize.CutinItem.ItemID.Item08AgariSR11
			},
			SortID = SortID.Sort4
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume08_010",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume08_010,
			CharaType = CharaType.Chara08,
			Name = "アニマルぱじゃま(くみゃ)",
			UnlockFightSpiritNum = 200,
			MitigationUnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition54,
			mitigationCutinItemIDs = new List<MFG.Types.Customize.CutinItem.ItemID>
			{
				MFG.Types.Customize.CutinItem.ItemID.Item08AgariSR10,
				MFG.Types.Customize.CutinItem.ItemID.Item08AgariSR11
			},
			SortID = SortID.Sort4
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume16_004",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume16_004,
			CharaType = CharaType.Chara16,
			Name = "アニマルフォーム(しろ)",
			UnlockFightSpiritNum = 200,
			MitigationUnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition55,
			mitigationCutinItemIDs = new List<MFG.Types.Customize.CutinItem.ItemID>
			{
				MFG.Types.Customize.CutinItem.ItemID.Item16AgariSR07,
				MFG.Types.Customize.CutinItem.ItemID.Item16AgariSR08
			},
			SortID = SortID.Sort1
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume16_005",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume16_005,
			CharaType = CharaType.Chara16,
			Name = "アニマルフォーム(くろ)",
			UnlockFightSpiritNum = 200,
			MitigationUnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition55,
			mitigationCutinItemIDs = new List<MFG.Types.Customize.CutinItem.ItemID>
			{
				MFG.Types.Customize.CutinItem.ItemID.Item16AgariSR07,
				MFG.Types.Customize.CutinItem.ItemID.Item16AgariSR08
			},
			SortID = SortID.Sort1
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume11_006",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume11_006,
			CharaType = CharaType.Chara11,
			Name = "スパイドレス",
			UnlockFightSpiritNum = 200,
			MitigationUnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition56,
			mitigationCutinItemIDs = new List<MFG.Types.Customize.CutinItem.ItemID>
			{
				MFG.Types.Customize.CutinItem.ItemID.Item11AgariSR08,
				MFG.Types.Customize.CutinItem.ItemID.Item11AgariSR09
			},
			SortID = SortID.Sort2
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume11_007",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume11_007,
			CharaType = CharaType.Chara11,
			Name = "スパイドレス(スノウ)",
			UnlockFightSpiritNum = 200,
			MitigationUnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition56,
			mitigationCutinItemIDs = new List<MFG.Types.Customize.CutinItem.ItemID>
			{
				MFG.Types.Customize.CutinItem.ItemID.Item11AgariSR08,
				MFG.Types.Customize.CutinItem.ItemID.Item11AgariSR09
			},
			SortID = SortID.Sort2
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume01_009",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume01_009,
			CharaType = CharaType.Chara01,
			Name = "レースクイーン(NDKカラー)",
			UnlockFightSpiritNum = 200,
			MitigationUnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition57,
			mitigationCutinItemIDs = new List<MFG.Types.Customize.CutinItem.ItemID>
			{
				MFG.Types.Customize.CutinItem.ItemID.Item01AgariSR06,
				MFG.Types.Customize.CutinItem.ItemID.Item01AgariSR10
			},
			SortID = SortID.Sort1
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume03_009",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume03_009,
			CharaType = CharaType.Chara03,
			Name = "雀喫茶メイド(ナナツノグリーン)",
			UnlockFightSpiritNum = 200,
			MitigationUnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition58,
			mitigationCutinItemIDs = new List<MFG.Types.Customize.CutinItem.ItemID>
			{
				MFG.Types.Customize.CutinItem.ItemID.Item03AgariSR11,
				MFG.Types.Customize.CutinItem.ItemID.Item03AgariSR12
			},
			SortID = SortID.Sort3
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume03_010",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume03_010,
			CharaType = CharaType.Chara03,
			Name = "雀喫茶メイド(リッチブラック)",
			UnlockFightSpiritNum = 200,
			MitigationUnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition58,
			mitigationCutinItemIDs = new List<MFG.Types.Customize.CutinItem.ItemID>
			{
				MFG.Types.Customize.CutinItem.ItemID.Item03AgariSR11,
				MFG.Types.Customize.CutinItem.ItemID.Item03AgariSR12
			},
			SortID = SortID.Sort3
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume17_001",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume17_001,
			CharaType = CharaType.Chara17,
			Name = "シスター",
			UnlockFightSpiritNum = 0,
			UnlockEventType = GameEventType.None,
			SortID = SortID.Default
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume17_002",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume17_002,
			CharaType = CharaType.Chara17,
			Name = "デビューウェア",
			UnlockFightSpiritNum = 100,
			UnlockEventType = GameEventType.None,
			SortID = SortID.Debut
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume17_003",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume17_003,
			CharaType = CharaType.Chara17,
			Name = "水着",
			UnlockFightSpiritNum = 100,
			UnlockEventType = GameEventType.None,
			SortID = SortID.Mizugi
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume06_011",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume06_011,
			CharaType = CharaType.Chara06,
			Name = "雀バーガースタッフ",
			UnlockFightSpiritNum = 200,
			MitigationUnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition59,
			mitigationCutinItemIDs = new List<MFG.Types.Customize.CutinItem.ItemID>
			{
				MFG.Types.Customize.CutinItem.ItemID.Item06AgariSR12,
				MFG.Types.Customize.CutinItem.ItemID.Item06AgariSR13
			},
			SortID = SortID.Sort4
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume06_012",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume06_012,
			CharaType = CharaType.Chara06,
			Name = "雀バーガースタッフ(リーダー)",
			UnlockFightSpiritNum = 200,
			MitigationUnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition59,
			mitigationCutinItemIDs = new List<MFG.Types.Customize.CutinItem.ItemID>
			{
				MFG.Types.Customize.CutinItem.ItemID.Item06AgariSR12,
				MFG.Types.Customize.CutinItem.ItemID.Item06AgariSR13
			},
			SortID = SortID.Sort4
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume02_011",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume02_011,
			CharaType = CharaType.Chara02,
			Name = "壱高スクール水着",
			UnlockFightSpiritNum = 200,
			MitigationUnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition60,
			mitigationCutinItemIDs = new List<MFG.Types.Customize.CutinItem.ItemID>
			{
				MFG.Types.Customize.CutinItem.ItemID.Item02AgariSR13,
				MFG.Types.Customize.CutinItem.ItemID.Item02AgariSR14
			},
			SortID = SortID.Sort3
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume02_012",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume02_012,
			CharaType = CharaType.Chara02,
			Name = "壱高スクール水着(えんじ)",
			UnlockFightSpiritNum = 200,
			MitigationUnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition60,
			mitigationCutinItemIDs = new List<MFG.Types.Customize.CutinItem.ItemID>
			{
				MFG.Types.Customize.CutinItem.ItemID.Item02AgariSR13,
				MFG.Types.Customize.CutinItem.ItemID.Item02AgariSR14
			},
			SortID = SortID.Sort3
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume13_007",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume13_007,
			CharaType = CharaType.Chara13,
			Name = "帝役スクール水着",
			UnlockFightSpiritNum = 200,
			MitigationUnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition61,
			mitigationCutinItemIDs = new List<MFG.Types.Customize.CutinItem.ItemID>
			{
				MFG.Types.Customize.CutinItem.ItemID.Item13AgariSR09,
				MFG.Types.Customize.CutinItem.ItemID.Item13AgariSR10
			},
			SortID = SortID.Sort2
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume13_008",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume13_008,
			CharaType = CharaType.Chara13,
			Name = "帝役スクール水着（くろ）",
			UnlockFightSpiritNum = 200,
			MitigationUnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition61,
			mitigationCutinItemIDs = new List<MFG.Types.Customize.CutinItem.ItemID>
			{
				MFG.Types.Customize.CutinItem.ItemID.Item13AgariSR09,
				MFG.Types.Customize.CutinItem.ItemID.Item13AgariSR10
			},
			SortID = SortID.Sort2
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume07_013",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume07_013,
			CharaType = CharaType.Chara07,
			Name = "弐女スクール水着",
			UnlockFightSpiritNum = 200,
			MitigationUnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition62,
			mitigationCutinItemIDs = new List<MFG.Types.Customize.CutinItem.ItemID>
			{
				MFG.Types.Customize.CutinItem.ItemID.Item07AgariSR14,
				MFG.Types.Customize.CutinItem.ItemID.Item07AgariSR15
			},
			SortID = SortID.Sort4
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume07_014",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume07_014,
			CharaType = CharaType.Chara07,
			Name = "弐女スクール水着（みどり）",
			UnlockFightSpiritNum = 200,
			MitigationUnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition62,
			mitigationCutinItemIDs = new List<MFG.Types.Customize.CutinItem.ItemID>
			{
				MFG.Types.Customize.CutinItem.ItemID.Item07AgariSR14,
				MFG.Types.Customize.CutinItem.ItemID.Item07AgariSR15
			},
			SortID = SortID.Sort4
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume09_012",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume09_012,
			CharaType = CharaType.Chara09,
			Name = "ベリーダンス (黄色)",
			UnlockFightSpiritNum = 200,
			MitigationUnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition63,
			mitigationCutinItemIDs = new List<MFG.Types.Customize.CutinItem.ItemID>
			{
				MFG.Types.Customize.CutinItem.ItemID.Item09AgariSR13,
				MFG.Types.Customize.CutinItem.ItemID.Item09AgariSR14,
				MFG.Types.Customize.CutinItem.ItemID.Item09AgariSR15,
				MFG.Types.Customize.CutinItem.ItemID.Item09AgariSR16
			},
			SortID = SortID.Sort3
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume09_013",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume09_013,
			CharaType = CharaType.Chara09,
			Name = "ベリーダンス (黄色ベールver.)",
			UnlockFightSpiritNum = 200,
			MitigationUnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition63,
			mitigationCutinItemIDs = new List<MFG.Types.Customize.CutinItem.ItemID>
			{
				MFG.Types.Customize.CutinItem.ItemID.Item09AgariSR13,
				MFG.Types.Customize.CutinItem.ItemID.Item09AgariSR14,
				MFG.Types.Customize.CutinItem.ItemID.Item09AgariSR15,
				MFG.Types.Customize.CutinItem.ItemID.Item09AgariSR16
			},
			SortID = SortID.Sort3
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume09_014",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume09_014,
			CharaType = CharaType.Chara09,
			Name = "ベリーダンス (水色)",
			UnlockFightSpiritNum = 200,
			MitigationUnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition63,
			mitigationCutinItemIDs = new List<MFG.Types.Customize.CutinItem.ItemID>
			{
				MFG.Types.Customize.CutinItem.ItemID.Item09AgariSR13,
				MFG.Types.Customize.CutinItem.ItemID.Item09AgariSR14,
				MFG.Types.Customize.CutinItem.ItemID.Item09AgariSR15,
				MFG.Types.Customize.CutinItem.ItemID.Item09AgariSR16
			},
			SortID = SortID.Sort3
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume09_015",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume09_015,
			CharaType = CharaType.Chara09,
			Name = "ベリーダンス (水色ベールver.)",
			UnlockFightSpiritNum = 200,
			MitigationUnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition63,
			mitigationCutinItemIDs = new List<MFG.Types.Customize.CutinItem.ItemID>
			{
				MFG.Types.Customize.CutinItem.ItemID.Item09AgariSR13,
				MFG.Types.Customize.CutinItem.ItemID.Item09AgariSR14,
				MFG.Types.Customize.CutinItem.ItemID.Item09AgariSR15,
				MFG.Types.Customize.CutinItem.ItemID.Item09AgariSR16
			},
			SortID = SortID.Sort3
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume05_012",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume05_012,
			CharaType = CharaType.Chara05,
			Name = "ねこ浴衣（藍）",
			UnlockFightSpiritNum = 200,
			MitigationUnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition64,
			mitigationCutinItemIDs = new List<MFG.Types.Customize.CutinItem.ItemID>
			{
				MFG.Types.Customize.CutinItem.ItemID.Item05AgariSR15,
				MFG.Types.Customize.CutinItem.ItemID.Item05AgariSR16
			},
			SortID = SortID.Sort4
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume05_013",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume05_013,
			CharaType = CharaType.Chara05,
			Name = "ねこ浴衣（白）",
			UnlockFightSpiritNum = 200,
			MitigationUnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition64,
			mitigationCutinItemIDs = new List<MFG.Types.Customize.CutinItem.ItemID>
			{
				MFG.Types.Customize.CutinItem.ItemID.Item05AgariSR15,
				MFG.Types.Customize.CutinItem.ItemID.Item05AgariSR16
			},
			SortID = SortID.Sort4
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume18_001",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume18_001,
			CharaType = CharaType.Chara18,
			Name = "私服",
			UnlockFightSpiritNum = 0,
			UnlockEventType = GameEventType.None,
			SortID = SortID.Default
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume18_002",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume18_002,
			CharaType = CharaType.Chara18,
			Name = "デビューウェア",
			UnlockFightSpiritNum = 100,
			UnlockEventType = GameEventType.None,
			SortID = SortID.Debut
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume18_003",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume18_003,
			CharaType = CharaType.Chara18,
			Name = "水着",
			UnlockFightSpiritNum = 100,
			UnlockEventType = GameEventType.None,
			SortID = SortID.Mizugi
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume04_011",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume04_011,
			CharaType = CharaType.Chara04,
			Name = "テニスウェア",
			UnlockFightSpiritNum = 200,
			MitigationUnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition65,
			mitigationCutinItemIDs = new List<MFG.Types.Customize.CutinItem.ItemID>
			{
				MFG.Types.Customize.CutinItem.ItemID.Item04AgariSR14,
				MFG.Types.Customize.CutinItem.ItemID.Item04AgariSR15
			},
			SortID = SortID.Sort5
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume04_012",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume04_012,
			CharaType = CharaType.Chara04,
			Name = "テニスウェア（帝役）",
			UnlockFightSpiritNum = 200,
			MitigationUnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition65,
			mitigationCutinItemIDs = new List<MFG.Types.Customize.CutinItem.ItemID>
			{
				MFG.Types.Customize.CutinItem.ItemID.Item04AgariSR14,
				MFG.Types.Customize.CutinItem.ItemID.Item04AgariSR15
			},
			SortID = SortID.Sort5
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume16_006",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume16_006,
			CharaType = CharaType.Chara16,
			Name = "バレーボールウェア(帝役)",
			UnlockFightSpiritNum = 200,
			MitigationUnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition66,
			mitigationCutinItemIDs = new List<MFG.Types.Customize.CutinItem.ItemID>
			{
				MFG.Types.Customize.CutinItem.ItemID.Item16AgariSR09,
				MFG.Types.Customize.CutinItem.ItemID.Item16AgariSR10
			},
			SortID = SortID.Sort2
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume16_007",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume16_007,
			CharaType = CharaType.Chara16,
			Name = "バレーボールウェア(ママさん)",
			UnlockFightSpiritNum = 200,
			MitigationUnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition66,
			mitigationCutinItemIDs = new List<MFG.Types.Customize.CutinItem.ItemID>
			{
				MFG.Types.Customize.CutinItem.ItemID.Item16AgariSR09,
				MFG.Types.Customize.CutinItem.ItemID.Item16AgariSR10
			},
			SortID = SortID.Sort2
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume17_004",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume17_004,
			CharaType = CharaType.Chara17,
			Name = "ランニングウェア (サイダーブルー)",
			UnlockFightSpiritNum = 200,
			MitigationUnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition67,
			mitigationCutinItemIDs = new List<MFG.Types.Customize.CutinItem.ItemID>
			{
				MFG.Types.Customize.CutinItem.ItemID.Item17AgariSR07,
				MFG.Types.Customize.CutinItem.ItemID.Item17AgariSR08
			},
			SortID = SortID.Sort1
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume17_005",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume17_005,
			CharaType = CharaType.Chara17,
			Name = "ランニングウェア (カロリーオレンジ)",
			UnlockFightSpiritNum = 200,
			MitigationUnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition67,
			mitigationCutinItemIDs = new List<MFG.Types.Customize.CutinItem.ItemID>
			{
				MFG.Types.Customize.CutinItem.ItemID.Item17AgariSR07,
				MFG.Types.Customize.CutinItem.ItemID.Item17AgariSR08
			},
			SortID = SortID.Sort1
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume19_001",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume19_001,
			CharaType = CharaType.Chara19,
			Name = "チャイナ",
			UnlockFightSpiritNum = 0,
			UnlockEventType = GameEventType.None,
			SortID = SortID.Default
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume19_002",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume19_002,
			CharaType = CharaType.Chara19,
			Name = "デビューウェア",
			UnlockFightSpiritNum = 100,
			UnlockEventType = GameEventType.None,
			SortID = SortID.Debut
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume19_003",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume19_003,
			CharaType = CharaType.Chara19,
			Name = "水着",
			UnlockFightSpiritNum = 100,
			UnlockEventType = GameEventType.None,
			SortID = SortID.Mizugi
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume14_006",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume14_006,
			CharaType = CharaType.Chara14,
			Name = "クリスマス",
			UnlockFightSpiritNum = 200,
			MitigationUnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition68,
			mitigationCutinItemIDs = new List<MFG.Types.Customize.CutinItem.ItemID>
			{
				MFG.Types.Customize.CutinItem.ItemID.Item14AgariSR09,
				MFG.Types.Customize.CutinItem.ItemID.Item14AgariSR10
			},
			SortID = SortID.Sort2
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume14_007",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume14_007,
			CharaType = CharaType.Chara14,
			Name = "クリスマス（ナイトメア）",
			UnlockFightSpiritNum = 200,
			MitigationUnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition68,
			mitigationCutinItemIDs = new List<MFG.Types.Customize.CutinItem.ItemID>
			{
				MFG.Types.Customize.CutinItem.ItemID.Item14AgariSR09,
				MFG.Types.Customize.CutinItem.ItemID.Item14AgariSR10
			},
			SortID = SortID.Sort2
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume12_008",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume12_008,
			CharaType = CharaType.Chara12,
			Name = "初詣（白百合）",
			UnlockFightSpiritNum = 200,
			MitigationUnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition69,
			mitigationCutinItemIDs = new List<MFG.Types.Customize.CutinItem.ItemID>
			{
				MFG.Types.Customize.CutinItem.ItemID.Item12AgariSR10,
				MFG.Types.Customize.CutinItem.ItemID.Item12AgariSR11
			},
			SortID = SortID.Sort3
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume12_009",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume12_009,
			CharaType = CharaType.Chara12,
			Name = "初詣（黒百合）",
			UnlockFightSpiritNum = 200,
			MitigationUnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition69,
			mitigationCutinItemIDs = new List<MFG.Types.Customize.CutinItem.ItemID>
			{
				MFG.Types.Customize.CutinItem.ItemID.Item12AgariSR10,
				MFG.Types.Customize.CutinItem.ItemID.Item12AgariSR11
			},
			SortID = SortID.Sort3
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume18_004",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume18_004,
			CharaType = CharaType.Chara18,
			Name = "りりぃコス",
			UnlockFightSpiritNum = 200,
			MitigationUnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition70,
			mitigationCutinItemIDs = new List<MFG.Types.Customize.CutinItem.ItemID>
			{
				MFG.Types.Customize.CutinItem.ItemID.Item18AgariSR07,
				MFG.Types.Customize.CutinItem.ItemID.Item18AgariSR08
			},
			SortID = SortID.Sort1
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume18_005",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume18_005,
			CharaType = CharaType.Chara18,
			Name = "りりぃコス (ホワイト)",
			UnlockFightSpiritNum = 200,
			MitigationUnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition70,
			mitigationCutinItemIDs = new List<MFG.Types.Customize.CutinItem.ItemID>
			{
				MFG.Types.Customize.CutinItem.ItemID.Item18AgariSR07,
				MFG.Types.Customize.CutinItem.ItemID.Item18AgariSR08
			},
			SortID = SortID.Sort1
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume01_010",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume01_010,
			CharaType = CharaType.Chara01,
			Name = "バレンタイン(ミルク)",
			UnlockFightSpiritNum = 200,
			MitigationUnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition71,
			mitigationCutinItemIDs = new List<MFG.Types.Customize.CutinItem.ItemID>
			{
				MFG.Types.Customize.CutinItem.ItemID.Item01AgariSR13,
				MFG.Types.Customize.CutinItem.ItemID.Item01AgariSR14
			},
			SortID = SortID.Sort4
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume01_011",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume01_011,
			CharaType = CharaType.Chara01,
			Name = "バレンタイン(ビター)",
			UnlockFightSpiritNum = 200,
			MitigationUnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition71,
			mitigationCutinItemIDs = new List<MFG.Types.Customize.CutinItem.ItemID>
			{
				MFG.Types.Customize.CutinItem.ItemID.Item01AgariSR13,
				MFG.Types.Customize.CutinItem.ItemID.Item01AgariSR14
			},
			SortID = SortID.Sort4
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume20_001",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume20_001,
			CharaType = CharaType.Chara20,
			Name = "制服",
			UnlockFightSpiritNum = 0,
			UnlockEventType = GameEventType.None,
			SortID = SortID.Default
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume20_002",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume20_002,
			CharaType = CharaType.Chara20,
			Name = "デビューウェア",
			UnlockFightSpiritNum = 100,
			UnlockEventType = GameEventType.None,
			SortID = SortID.Debut
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume20_003",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume20_003,
			CharaType = CharaType.Chara20,
			Name = "水着",
			UnlockFightSpiritNum = 100,
			UnlockEventType = GameEventType.None,
			SortID = SortID.Mizugi
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume02_013",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume02_013,
			CharaType = CharaType.Chara02,
			Name = "ネリマギャル",
			UnlockFightSpiritNum = 200,
			MitigationUnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition72,
			mitigationCutinItemIDs = new List<MFG.Types.Customize.CutinItem.ItemID>
			{
				MFG.Types.Customize.CutinItem.ItemID.Item02AgariSR16,
				MFG.Types.Customize.CutinItem.ItemID.Item02AgariSR17
			},
			SortID = SortID.Sort4
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume02_014",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume02_014,
			CharaType = CharaType.Chara02,
			Name = "ネリマギャル(カヌレ)",
			UnlockFightSpiritNum = 200,
			MitigationUnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition72,
			mitigationCutinItemIDs = new List<MFG.Types.Customize.CutinItem.ItemID>
			{
				MFG.Types.Customize.CutinItem.ItemID.Item02AgariSR16,
				MFG.Types.Customize.CutinItem.ItemID.Item02AgariSR17
			},
			SortID = SortID.Sort4
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume15_006",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume15_006,
			CharaType = CharaType.Chara15,
			Name = "ブクロギャル",
			UnlockFightSpiritNum = 200,
			MitigationUnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition72,
			mitigationCutinItemIDs = new List<MFG.Types.Customize.CutinItem.ItemID>
			{
				MFG.Types.Customize.CutinItem.ItemID.Item15AgariSR09,
				MFG.Types.Customize.CutinItem.ItemID.Item15AgariSR10
			},
			SortID = SortID.Sort2
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume15_007",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume15_007,
			CharaType = CharaType.Chara15,
			Name = "ブクロギャル(オニキス)",
			UnlockFightSpiritNum = 200,
			MitigationUnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition72,
			mitigationCutinItemIDs = new List<MFG.Types.Customize.CutinItem.ItemID>
			{
				MFG.Types.Customize.CutinItem.ItemID.Item15AgariSR09,
				MFG.Types.Customize.CutinItem.ItemID.Item15AgariSR10
			},
			SortID = SortID.Sort2
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume16_008",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume16_008,
			CharaType = CharaType.Chara16,
			Name = "ナガタチョウギャル",
			UnlockFightSpiritNum = 200,
			MitigationUnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition72,
			mitigationCutinItemIDs = new List<MFG.Types.Customize.CutinItem.ItemID>
			{
				MFG.Types.Customize.CutinItem.ItemID.Item16AgariSR11,
				MFG.Types.Customize.CutinItem.ItemID.Item16AgariSR12
			},
			SortID = SortID.Sort3
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume16_009",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume16_009,
			CharaType = CharaType.Chara16,
			Name = "ナガタチョウギャル(ラベンダー)",
			UnlockFightSpiritNum = 200,
			MitigationUnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition72,
			mitigationCutinItemIDs = new List<MFG.Types.Customize.CutinItem.ItemID>
			{
				MFG.Types.Customize.CutinItem.ItemID.Item16AgariSR11,
				MFG.Types.Customize.CutinItem.ItemID.Item16AgariSR12
			},
			SortID = SortID.Sort3
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume01_012",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume01_012,
			CharaType = CharaType.Chara01,
			Name = "ネリマギャル(ショコラ)",
			UnlockFightSpiritNum = 200,
			MitigationUnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition72,
			mitigationCutinItemIDs = new List<MFG.Types.Customize.CutinItem.ItemID>
			{
				MFG.Types.Customize.CutinItem.ItemID.Item01AgariSR08,
				MFG.Types.Customize.CutinItem.ItemID.Item01AgariSR09,
				MFG.Types.Customize.CutinItem.ItemID.Item01AgariSR15
			},
			SortID = SortID.Sort3
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume12_010",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume12_010,
			CharaType = CharaType.Chara12,
			Name = "セイジョウギャル(マーガレット)",
			UnlockFightSpiritNum = 200,
			MitigationUnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition72,
			mitigationCutinItemIDs = new List<MFG.Types.Customize.CutinItem.ItemID>
			{
				MFG.Types.Customize.CutinItem.ItemID.Item12AgariSR08,
				MFG.Types.Customize.CutinItem.ItemID.Item12AgariSR09,
				MFG.Types.Customize.CutinItem.ItemID.Item12AgariSR12
			},
			SortID = SortID.Sort2
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume14_008",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume14_008,
			CharaType = CharaType.Chara14,
			Name = "サキュバスギャル(アロエ)",
			UnlockFightSpiritNum = 200,
			MitigationUnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition72,
			mitigationCutinItemIDs = new List<MFG.Types.Customize.CutinItem.ItemID>
			{
				MFG.Types.Customize.CutinItem.ItemID.Item14AgariSR07,
				MFG.Types.Customize.CutinItem.ItemID.Item14AgariSR08,
				MFG.Types.Customize.CutinItem.ItemID.Item14AgariSR13
			},
			SortID = SortID.Sort1
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume11_008",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume11_008,
			CharaType = CharaType.Chara11,
			Name = "スパイスーツ",
			UnlockFightSpiritNum = 200,
			MitigationUnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition73,
			mitigationCutinItemIDs = new List<MFG.Types.Customize.CutinItem.ItemID>
			{
				MFG.Types.Customize.CutinItem.ItemID.Item11AgariSR10,
				MFG.Types.Customize.CutinItem.ItemID.Item11AgariSR11
			},
			SortID = SortID.Sort3
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume11_009",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume11_009,
			CharaType = CharaType.Chara11,
			Name = "スパイスーツ(ピンク)",
			UnlockFightSpiritNum = 200,
			MitigationUnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition73,
			mitigationCutinItemIDs = new List<MFG.Types.Customize.CutinItem.ItemID>
			{
				MFG.Types.Customize.CutinItem.ItemID.Item11AgariSR10,
				MFG.Types.Customize.CutinItem.ItemID.Item11AgariSR11
			},
			SortID = SortID.Sort3
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume19_004",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume19_004,
			CharaType = CharaType.Chara19,
			Name = "バレーボールウェア (ママさん)",
			UnlockFightSpiritNum = 200,
			MitigationUnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition74,
			mitigationCutinItemIDs = new List<MFG.Types.Customize.CutinItem.ItemID>
			{
				MFG.Types.Customize.CutinItem.ItemID.Item19AgariSR07,
				MFG.Types.Customize.CutinItem.ItemID.Item19AgariSR08
			},
			SortID = SortID.Sort1
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume19_005",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume19_005,
			CharaType = CharaType.Chara19,
			Name = "バレーボールウェア (帝役)",
			UnlockFightSpiritNum = 200,
			MitigationUnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition74,
			mitigationCutinItemIDs = new List<MFG.Types.Customize.CutinItem.ItemID>
			{
				MFG.Types.Customize.CutinItem.ItemID.Item19AgariSR07,
				MFG.Types.Customize.CutinItem.ItemID.Item19AgariSR08
			},
			SortID = SortID.Sort1
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume08_011",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume08_011,
			CharaType = CharaType.Chara08,
			Name = "袴メイド",
			UnlockFightSpiritNum = 200,
			MitigationUnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition75,
			mitigationCutinItemIDs = new List<MFG.Types.Customize.CutinItem.ItemID>
			{
				MFG.Types.Customize.CutinItem.ItemID.Item08AgariSR13,
				MFG.Types.Customize.CutinItem.ItemID.Item08AgariSR14
			},
			SortID = SortID.Sort5
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume08_012",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume08_012,
			CharaType = CharaType.Chara08,
			Name = "袴メイド(抹茶)",
			UnlockFightSpiritNum = 200,
			MitigationUnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition75,
			mitigationCutinItemIDs = new List<MFG.Types.Customize.CutinItem.ItemID>
			{
				MFG.Types.Customize.CutinItem.ItemID.Item08AgariSR13,
				MFG.Types.Customize.CutinItem.ItemID.Item08AgariSR14
			},
			SortID = SortID.Sort5
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume03_011",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume03_011,
			CharaType = CharaType.Chara03,
			Name = "弐女スクール水着",
			UnlockFightSpiritNum = 200,
			MitigationUnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition76,
			mitigationCutinItemIDs = new List<MFG.Types.Customize.CutinItem.ItemID>
			{
				MFG.Types.Customize.CutinItem.ItemID.Item03AgariSR13,
				MFG.Types.Customize.CutinItem.ItemID.Item03AgariSR14
			},
			SortID = SortID.Sort4
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume03_012",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume03_012,
			CharaType = CharaType.Chara03,
			Name = "弐女スクール水着(金)",
			UnlockFightSpiritNum = 200,
			MitigationUnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition76,
			mitigationCutinItemIDs = new List<MFG.Types.Customize.CutinItem.ItemID>
			{
				MFG.Types.Customize.CutinItem.ItemID.Item03AgariSR13,
				MFG.Types.Customize.CutinItem.ItemID.Item03AgariSR14
			},
			SortID = SortID.Sort4
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume21_001",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume21_001,
			CharaType = CharaType.Chara21,
			Name = "制服",
			UnlockFightSpiritNum = 0,
			UnlockEventType = GameEventType.None,
			SortID = SortID.Default
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume21_002",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume21_002,
			CharaType = CharaType.Chara21,
			Name = "デビューウェア",
			UnlockFightSpiritNum = 100,
			UnlockEventType = GameEventType.None,
			SortID = SortID.Debut
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume21_003",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume21_003,
			CharaType = CharaType.Chara21,
			Name = "水着",
			UnlockFightSpiritNum = 100,
			UnlockEventType = GameEventType.None,
			SortID = SortID.Mizugi
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume20_004",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume20_004,
			CharaType = CharaType.Chara20,
			Name = "雀喫茶メイド",
			UnlockFightSpiritNum = 200,
			MitigationUnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition77,
			mitigationCutinItemIDs = new List<MFG.Types.Customize.CutinItem.ItemID>
			{
				MFG.Types.Customize.CutinItem.ItemID.Item20AgariSR07,
				MFG.Types.Customize.CutinItem.ItemID.Item20AgariSR08
			},
			SortID = SortID.Sort1
		},
		new MFG.Types.Customize.Costume.Item
		{
			ObjectID = "OID_Costume20_005",
			ItemID = MFG.Types.Customize.Costume.ItemID.Costume20_005,
			CharaType = CharaType.Chara20,
			Name = "雀喫茶メイド (カワサキピンク)",
			UnlockFightSpiritNum = 200,
			MitigationUnlockFightSpiritNum = 50,
			UnlockEventType = GameEventType.CostumeAddition77,
			mitigationCutinItemIDs = new List<MFG.Types.Customize.CutinItem.ItemID>
			{
				MFG.Types.Customize.CutinItem.ItemID.Item20AgariSR07,
				MFG.Types.Customize.CutinItem.ItemID.Item20AgariSR08
			},
			SortID = SortID.Sort1
		}
	};

	public static MFG.Types.Customize.Costume.Item GetItem(MFG.Types.Customize.Costume.ItemID itemID)
	{
		return m_items.Find((MFG.Types.Customize.Costume.Item item) => item.ItemID == itemID);
	}

	public static List<MFG.Types.Customize.Costume.Item> GetItems(Predicate<MFG.Types.Customize.Costume.Item> match)
	{
		return m_items.FindAll(match);
	}

	public static MFG.Types.Customize.Costume.ItemID GetDefaultEquipItemID(CharaType charaType)
	{
		return m_defaultEquipItems[charaType];
	}

	public static List<MFG.Types.Customize.Costume.ItemID> GetDefaultItemIDs()
	{
		return m_defaultPossessionItems;
	}

	public static bool IsDefaultItemID(MFG.Types.Customize.Costume.ItemID itemID)
	{
		return m_defaultPossessionItems.Exists((MFG.Types.Customize.Costume.ItemID x) => x == itemID);
	}

	public static MFG.Types.Customize.Costume.ItemID GetDebutItemID(CharaType charaType)
	{
		return m_debutCostumeItems[charaType];
	}

	public static int NeedUnlockFightSpiritNum(MFG.Types.Customize.Costume.ItemID itemID)
	{
		MFG.Types.Customize.Costume.Item item = GetItem(itemID);
		List<MFG.Types.Customize.CutinItem.ItemID> mitigationCutinItemIDs = item.mitigationCutinItemIDs;
		if (mitigationCutinItemIDs != null && mitigationCutinItemIDs.Count > 0)
		{
			if (!ItemUtility.IsCutinItemPossessions(item.mitigationCutinItemIDs))
			{
				return item.UnlockFightSpiritNum;
			}
			return item.MitigationUnlockFightSpiritNum;
		}
		return item.UnlockFightSpiritNum;
	}

	public static (int, bool) NeedUnlockSpiritIsMitigated(MFG.Types.Customize.Costume.ItemID itemID)
	{
		MFG.Types.Customize.Costume.Item item = GetItem(itemID);
		List<MFG.Types.Customize.CutinItem.ItemID> mitigationCutinItemIDs = item.mitigationCutinItemIDs;
		if (mitigationCutinItemIDs != null && mitigationCutinItemIDs.Count > 0 && ItemUtility.IsCutinItemPossessions(item.mitigationCutinItemIDs))
		{
			return (item.MitigationUnlockFightSpiritNum, true);
		}
		return (item.UnlockFightSpiritNum, false);
	}

	public static (int, GachaSeriesName) NeedSpiritWithIsMitigation(MFG.Types.Customize.Costume.ItemID itemID)
	{
		MFG.Types.Customize.Costume.Item item = GetItem(itemID);
		List<MFG.Types.Customize.CutinItem.ItemID> mitigationCutinItemIDs = item.mitigationCutinItemIDs;
		if (mitigationCutinItemIDs != null && mitigationCutinItemIDs.Count > 0)
		{
			if (ItemUtility.IsCutinItemPossessions(item.mitigationCutinItemIDs))
			{
				return (item.MitigationUnlockFightSpiritNum, (GachaSeriesName)(-1));
			}
			return (item.UnlockFightSpiritNum, GameDataManager.GachaInfo.GetPickupItemContainSeries(item.mitigationCutinItemIDs));
		}
		return (item.UnlockFightSpiritNum, (GachaSeriesName)(-1));
	}
}
