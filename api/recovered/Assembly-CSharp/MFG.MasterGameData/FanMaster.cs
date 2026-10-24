using System;
using System.Collections.Generic;
using System.Linq;
using MFG.Taikyoku.Command.Receive;
using MFG.Types;
using MFG.Types.Customize.CustomizeItem;
using MFG.Types.Fan;

namespace MFG.MasterGameData;

public class FanMaster : MasterGameDataBase
{
	public static List<FanMasterData> m_fanMastarTable = new List<FanMasterData>
	{
		new FanMasterData
		{
			ObjectID = "OID_Fan01_01",
			ID = FanItemID.Fan01_01,
			charaType = CharaType.Chara01,
			FanNum = 300,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.TableSticker001,
				ItemID.TableSticker003,
				ItemID.TableSticker002,
				ItemID.TableSticker004
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan01_02",
			ID = FanItemID.Fan01_02,
			charaType = CharaType.Chara01,
			FanNum = 600,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan01_03",
			ID = FanItemID.Fan01_03,
			charaType = CharaType.Chara01,
			FanNum = 1200,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.ReachStick002,
				ItemID.ReachStick004,
				ItemID.ReachStick003,
				ItemID.ReachStick005
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan01_04",
			ID = FanItemID.Fan01_04,
			charaType = CharaType.Chara01,
			FanNum = 2000,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan01_05",
			ID = FanItemID.Fan01_05,
			charaType = CharaType.Chara01,
			FanNum = 3000,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.PlayerAccessory001,
				ItemID.PlayerAccessory003,
				ItemID.PlayerAccessory002,
				ItemID.PlayerAccessory004
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan01_06",
			ID = FanItemID.Fan01_06,
			charaType = CharaType.Chara01,
			FanNum = 4500,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan01_07",
			ID = FanItemID.Fan01_07,
			charaType = CharaType.Chara01,
			FanNum = 6000,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan01_08",
			ID = FanItemID.Fan01_08,
			charaType = CharaType.Chara01,
			FanNum = 9000,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.TableSticker005,
				ItemID.TableSticker007,
				ItemID.TableSticker006,
				ItemID.TableSticker008
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan01_09",
			ID = FanItemID.Fan01_09,
			charaType = CharaType.Chara01,
			FanNum = 12000,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan01_10",
			ID = FanItemID.Fan01_10,
			charaType = CharaType.Chara01,
			FanNum = 20000,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.PlayerAccessory005,
				ItemID.PlayerAccessory007,
				ItemID.PlayerAccessory006,
				ItemID.PlayerAccessory008
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan01_11",
			ID = FanItemID.Fan01_11,
			charaType = CharaType.Chara01,
			FanNum = 30000,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.ReachStick009,
				ItemID.ReachStick007,
				ItemID.ReachStick008,
				ItemID.ReachStick006
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan02_01",
			ID = FanItemID.Fan02_01,
			charaType = CharaType.Chara02,
			FanNum = 300,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.TableSticker009,
				ItemID.TableSticker011,
				ItemID.TableSticker010,
				ItemID.TableSticker012
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan02_02",
			ID = FanItemID.Fan02_02,
			charaType = CharaType.Chara02,
			FanNum = 600,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan02_03",
			ID = FanItemID.Fan02_03,
			charaType = CharaType.Chara02,
			FanNum = 1200,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.ReachStick010,
				ItemID.ReachStick012,
				ItemID.ReachStick011,
				ItemID.ReachStick013
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan02_04",
			ID = FanItemID.Fan02_04,
			charaType = CharaType.Chara02,
			FanNum = 2000,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan02_05",
			ID = FanItemID.Fan02_05,
			charaType = CharaType.Chara02,
			FanNum = 3000,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.PlayerAccessory009,
				ItemID.PlayerAccessory011,
				ItemID.PlayerAccessory010,
				ItemID.PlayerAccessory012
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan02_06",
			ID = FanItemID.Fan02_06,
			charaType = CharaType.Chara02,
			FanNum = 4500,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan02_07",
			ID = FanItemID.Fan02_07,
			charaType = CharaType.Chara02,
			FanNum = 6000,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan02_08",
			ID = FanItemID.Fan02_08,
			charaType = CharaType.Chara02,
			FanNum = 9000,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.TableSticker013,
				ItemID.TableSticker015,
				ItemID.TableSticker014,
				ItemID.TableSticker016
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan02_09",
			ID = FanItemID.Fan02_09,
			charaType = CharaType.Chara02,
			FanNum = 12000,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan02_10",
			ID = FanItemID.Fan02_10,
			charaType = CharaType.Chara02,
			FanNum = 20000,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.PlayerAccessory013,
				ItemID.PlayerAccessory015,
				ItemID.PlayerAccessory014,
				ItemID.PlayerAccessory016
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan02_11",
			ID = FanItemID.Fan02_11,
			charaType = CharaType.Chara02,
			FanNum = 30000,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.ReachStick017,
				ItemID.ReachStick016,
				ItemID.ReachStick014,
				ItemID.ReachStick015
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan03_01",
			ID = FanItemID.Fan03_01,
			charaType = CharaType.Chara03,
			FanNum = 300,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.TableSticker017,
				ItemID.TableSticker019,
				ItemID.TableSticker018,
				ItemID.TableSticker020
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan03_02",
			ID = FanItemID.Fan03_02,
			charaType = CharaType.Chara03,
			FanNum = 600,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan03_03",
			ID = FanItemID.Fan03_03,
			charaType = CharaType.Chara03,
			FanNum = 1200,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.ReachStick018,
				ItemID.ReachStick020,
				ItemID.ReachStick019,
				ItemID.ReachStick021
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan03_04",
			ID = FanItemID.Fan03_04,
			charaType = CharaType.Chara03,
			FanNum = 2000,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan03_05",
			ID = FanItemID.Fan03_05,
			charaType = CharaType.Chara03,
			FanNum = 3000,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.PlayerAccessory017,
				ItemID.PlayerAccessory019,
				ItemID.PlayerAccessory018,
				ItemID.PlayerAccessory020
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan03_06",
			ID = FanItemID.Fan03_06,
			charaType = CharaType.Chara03,
			FanNum = 4500,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan03_07",
			ID = FanItemID.Fan03_07,
			charaType = CharaType.Chara03,
			FanNum = 6000,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan03_08",
			ID = FanItemID.Fan03_08,
			charaType = CharaType.Chara03,
			FanNum = 9000,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.TableSticker021,
				ItemID.TableSticker023,
				ItemID.TableSticker022,
				ItemID.TableSticker024
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan03_09",
			ID = FanItemID.Fan03_09,
			charaType = CharaType.Chara03,
			FanNum = 12000,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan03_10",
			ID = FanItemID.Fan03_10,
			charaType = CharaType.Chara03,
			FanNum = 20000,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.PlayerAccessory021,
				ItemID.PlayerAccessory023,
				ItemID.PlayerAccessory022,
				ItemID.PlayerAccessory024
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan03_11",
			ID = FanItemID.Fan03_11,
			charaType = CharaType.Chara03,
			FanNum = 30000,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.ReachStick024,
				ItemID.ReachStick023,
				ItemID.ReachStick025,
				ItemID.ReachStick022
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan04_01",
			ID = FanItemID.Fan04_01,
			charaType = CharaType.Chara04,
			FanNum = 300,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.TableSticker025,
				ItemID.TableSticker027,
				ItemID.TableSticker026,
				ItemID.TableSticker028
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan04_02",
			ID = FanItemID.Fan04_02,
			charaType = CharaType.Chara04,
			FanNum = 600,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan04_03",
			ID = FanItemID.Fan04_03,
			charaType = CharaType.Chara04,
			FanNum = 1200,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.ReachStick026,
				ItemID.ReachStick028,
				ItemID.ReachStick027,
				ItemID.ReachStick029
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan04_04",
			ID = FanItemID.Fan04_04,
			charaType = CharaType.Chara04,
			FanNum = 2000,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan04_05",
			ID = FanItemID.Fan04_05,
			charaType = CharaType.Chara04,
			FanNum = 3000,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.PlayerAccessory025,
				ItemID.PlayerAccessory027,
				ItemID.PlayerAccessory026,
				ItemID.PlayerAccessory028
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan04_06",
			ID = FanItemID.Fan04_06,
			charaType = CharaType.Chara04,
			FanNum = 4500,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan04_07",
			ID = FanItemID.Fan04_07,
			charaType = CharaType.Chara04,
			FanNum = 6000,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan04_08",
			ID = FanItemID.Fan04_08,
			charaType = CharaType.Chara04,
			FanNum = 9000,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.TableSticker029,
				ItemID.TableSticker031,
				ItemID.TableSticker030,
				ItemID.TableSticker032
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan04_09",
			ID = FanItemID.Fan04_09,
			charaType = CharaType.Chara04,
			FanNum = 12000,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan04_10",
			ID = FanItemID.Fan04_10,
			charaType = CharaType.Chara04,
			FanNum = 20000,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.PlayerAccessory029,
				ItemID.PlayerAccessory031,
				ItemID.PlayerAccessory030,
				ItemID.PlayerAccessory032
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan04_11",
			ID = FanItemID.Fan04_11,
			charaType = CharaType.Chara04,
			FanNum = 30000,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.ReachStick031,
				ItemID.ReachStick030,
				ItemID.ReachStick032,
				ItemID.ReachStick033
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan05_01",
			ID = FanItemID.Fan05_01,
			charaType = CharaType.Chara05,
			FanNum = 300,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.TableSticker035,
				ItemID.TableSticker037,
				ItemID.TableSticker036,
				ItemID.TableSticker038
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan05_02",
			ID = FanItemID.Fan05_02,
			charaType = CharaType.Chara05,
			FanNum = 600,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan05_03",
			ID = FanItemID.Fan05_03,
			charaType = CharaType.Chara05,
			FanNum = 1200,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.ReachStick037,
				ItemID.ReachStick039,
				ItemID.ReachStick038,
				ItemID.ReachStick040
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan05_04",
			ID = FanItemID.Fan05_04,
			charaType = CharaType.Chara05,
			FanNum = 2000,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan05_05",
			ID = FanItemID.Fan05_05,
			charaType = CharaType.Chara05,
			FanNum = 3000,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.PlayerAccessory036,
				ItemID.PlayerAccessory038,
				ItemID.PlayerAccessory037,
				ItemID.PlayerAccessory039
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan05_06",
			ID = FanItemID.Fan05_06,
			charaType = CharaType.Chara05,
			FanNum = 4500,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan05_07",
			ID = FanItemID.Fan05_07,
			charaType = CharaType.Chara05,
			FanNum = 6000,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan05_08",
			ID = FanItemID.Fan05_08,
			charaType = CharaType.Chara05,
			FanNum = 9000,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.TableSticker039,
				ItemID.TableSticker041,
				ItemID.TableSticker040,
				ItemID.TableSticker042
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan05_09",
			ID = FanItemID.Fan05_09,
			charaType = CharaType.Chara05,
			FanNum = 12000,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan05_10",
			ID = FanItemID.Fan05_10,
			charaType = CharaType.Chara05,
			FanNum = 20000,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.PlayerAccessory040,
				ItemID.PlayerAccessory042,
				ItemID.PlayerAccessory041,
				ItemID.PlayerAccessory043
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan05_11",
			ID = FanItemID.Fan05_11,
			charaType = CharaType.Chara05,
			FanNum = 30000,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.ReachStick041,
				ItemID.ReachStick042,
				ItemID.ReachStick043,
				ItemID.ReachStick044
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan06_01",
			ID = FanItemID.Fan06_01,
			charaType = CharaType.Chara06,
			FanNum = 300,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.TableSticker043,
				ItemID.TableSticker045,
				ItemID.TableSticker044,
				ItemID.TableSticker046
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan06_02",
			ID = FanItemID.Fan06_02,
			charaType = CharaType.Chara06,
			FanNum = 600,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan06_03",
			ID = FanItemID.Fan06_03,
			charaType = CharaType.Chara06,
			FanNum = 1200,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.ReachStick045,
				ItemID.ReachStick047,
				ItemID.ReachStick046,
				ItemID.ReachStick048
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan06_04",
			ID = FanItemID.Fan06_04,
			charaType = CharaType.Chara06,
			FanNum = 2000,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan06_05",
			ID = FanItemID.Fan06_05,
			charaType = CharaType.Chara06,
			FanNum = 3000,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.PlayerAccessory044,
				ItemID.PlayerAccessory046,
				ItemID.PlayerAccessory045,
				ItemID.PlayerAccessory047
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan06_06",
			ID = FanItemID.Fan06_06,
			charaType = CharaType.Chara06,
			FanNum = 4500,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan06_07",
			ID = FanItemID.Fan06_07,
			charaType = CharaType.Chara06,
			FanNum = 6000,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan06_08",
			ID = FanItemID.Fan06_08,
			charaType = CharaType.Chara06,
			FanNum = 9000,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.TableSticker047,
				ItemID.TableSticker049,
				ItemID.TableSticker048,
				ItemID.TableSticker050
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan06_09",
			ID = FanItemID.Fan06_09,
			charaType = CharaType.Chara06,
			FanNum = 12000,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan06_10",
			ID = FanItemID.Fan06_10,
			charaType = CharaType.Chara06,
			FanNum = 20000,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.PlayerAccessory048,
				ItemID.PlayerAccessory050,
				ItemID.PlayerAccessory049,
				ItemID.PlayerAccessory051
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan06_11",
			ID = FanItemID.Fan06_11,
			charaType = CharaType.Chara06,
			FanNum = 30000,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.ReachStick049,
				ItemID.ReachStick050,
				ItemID.ReachStick051,
				ItemID.ReachStick052
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan07_01",
			ID = FanItemID.Fan07_01,
			charaType = CharaType.Chara07,
			FanNum = 300,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.TableSticker051,
				ItemID.TableSticker053,
				ItemID.TableSticker052,
				ItemID.TableSticker054
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan07_02",
			ID = FanItemID.Fan07_02,
			charaType = CharaType.Chara07,
			FanNum = 600,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan07_03",
			ID = FanItemID.Fan07_03,
			charaType = CharaType.Chara07,
			FanNum = 1200,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.ReachStick053,
				ItemID.ReachStick055,
				ItemID.ReachStick054,
				ItemID.ReachStick056
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan07_04",
			ID = FanItemID.Fan07_04,
			charaType = CharaType.Chara07,
			FanNum = 2000,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan07_05",
			ID = FanItemID.Fan07_05,
			charaType = CharaType.Chara07,
			FanNum = 3000,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.PlayerAccessory052,
				ItemID.PlayerAccessory054,
				ItemID.PlayerAccessory053,
				ItemID.PlayerAccessory055
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan07_06",
			ID = FanItemID.Fan07_06,
			charaType = CharaType.Chara07,
			FanNum = 4500,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan07_07",
			ID = FanItemID.Fan07_07,
			charaType = CharaType.Chara07,
			FanNum = 6000,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan07_08",
			ID = FanItemID.Fan07_08,
			charaType = CharaType.Chara07,
			FanNum = 9000,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.TableSticker055,
				ItemID.TableSticker057,
				ItemID.TableSticker056,
				ItemID.TableSticker058
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan07_09",
			ID = FanItemID.Fan07_09,
			charaType = CharaType.Chara07,
			FanNum = 12000,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan07_10",
			ID = FanItemID.Fan07_10,
			charaType = CharaType.Chara07,
			FanNum = 20000,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.PlayerAccessory056,
				ItemID.PlayerAccessory058,
				ItemID.PlayerAccessory057,
				ItemID.PlayerAccessory059
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan07_11",
			ID = FanItemID.Fan07_11,
			charaType = CharaType.Chara07,
			FanNum = 30000,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.ReachStick057,
				ItemID.ReachStick058,
				ItemID.ReachStick059,
				ItemID.ReachStick060
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan08_01",
			ID = FanItemID.Fan08_01,
			charaType = CharaType.Chara08,
			FanNum = 300,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.TableSticker059,
				ItemID.TableSticker061,
				ItemID.TableSticker060,
				ItemID.TableSticker062
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan08_02",
			ID = FanItemID.Fan08_02,
			charaType = CharaType.Chara08,
			FanNum = 600,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan08_03",
			ID = FanItemID.Fan08_03,
			charaType = CharaType.Chara08,
			FanNum = 1200,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.ReachStick061,
				ItemID.ReachStick063,
				ItemID.ReachStick062,
				ItemID.ReachStick064
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan08_04",
			ID = FanItemID.Fan08_04,
			charaType = CharaType.Chara08,
			FanNum = 2000,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan08_05",
			ID = FanItemID.Fan08_05,
			charaType = CharaType.Chara08,
			FanNum = 3000,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.PlayerAccessory060,
				ItemID.PlayerAccessory062,
				ItemID.PlayerAccessory061,
				ItemID.PlayerAccessory063
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan08_06",
			ID = FanItemID.Fan08_06,
			charaType = CharaType.Chara08,
			FanNum = 4500,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan08_07",
			ID = FanItemID.Fan08_07,
			charaType = CharaType.Chara08,
			FanNum = 6000,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan08_08",
			ID = FanItemID.Fan08_08,
			charaType = CharaType.Chara08,
			FanNum = 9000,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.TableSticker063,
				ItemID.TableSticker065,
				ItemID.TableSticker064,
				ItemID.TableSticker066
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan08_09",
			ID = FanItemID.Fan08_09,
			charaType = CharaType.Chara08,
			FanNum = 12000,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan08_10",
			ID = FanItemID.Fan08_10,
			charaType = CharaType.Chara08,
			FanNum = 20000,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.PlayerAccessory064,
				ItemID.PlayerAccessory066,
				ItemID.PlayerAccessory065,
				ItemID.PlayerAccessory067
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan08_11",
			ID = FanItemID.Fan08_11,
			charaType = CharaType.Chara08,
			FanNum = 30000,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.ReachStick065,
				ItemID.ReachStick066,
				ItemID.ReachStick067,
				ItemID.ReachStick068
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan09_01",
			ID = FanItemID.Fan09_01,
			charaType = CharaType.Chara09,
			FanNum = 300,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.TableSticker067,
				ItemID.TableSticker069,
				ItemID.TableSticker068,
				ItemID.TableSticker070
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan09_02",
			ID = FanItemID.Fan09_02,
			charaType = CharaType.Chara09,
			FanNum = 600,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan09_03",
			ID = FanItemID.Fan09_03,
			charaType = CharaType.Chara09,
			FanNum = 1200,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.ReachStick069,
				ItemID.ReachStick072,
				ItemID.ReachStick070,
				ItemID.ReachStick071
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan09_04",
			ID = FanItemID.Fan09_04,
			charaType = CharaType.Chara09,
			FanNum = 2000,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan09_05",
			ID = FanItemID.Fan09_05,
			charaType = CharaType.Chara09,
			FanNum = 3000,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.PlayerAccessory068,
				ItemID.PlayerAccessory069,
				ItemID.PlayerAccessory070,
				ItemID.PlayerAccessory071
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan09_06",
			ID = FanItemID.Fan09_06,
			charaType = CharaType.Chara09,
			FanNum = 4500,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan09_07",
			ID = FanItemID.Fan09_07,
			charaType = CharaType.Chara09,
			FanNum = 6000,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan09_08",
			ID = FanItemID.Fan09_08,
			charaType = CharaType.Chara09,
			FanNum = 9000,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.TableSticker071,
				ItemID.TableSticker073,
				ItemID.TableSticker072,
				ItemID.TableSticker074
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan09_09",
			ID = FanItemID.Fan09_09,
			charaType = CharaType.Chara09,
			FanNum = 12000,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan09_10",
			ID = FanItemID.Fan09_10,
			charaType = CharaType.Chara09,
			FanNum = 20000,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.PlayerAccessory072,
				ItemID.PlayerAccessory074,
				ItemID.PlayerAccessory073,
				ItemID.PlayerAccessory075
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan09_11",
			ID = FanItemID.Fan09_11,
			charaType = CharaType.Chara09,
			FanNum = 30000,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.ReachStick074,
				ItemID.ReachStick073,
				ItemID.ReachStick075,
				ItemID.ReachStick076
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan10_01",
			ID = FanItemID.Fan10_01,
			charaType = CharaType.Chara10,
			FanNum = 300,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.TableSticker100,
				ItemID.TableSticker102,
				ItemID.TableSticker101,
				ItemID.TableSticker103
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan10_02",
			ID = FanItemID.Fan10_02,
			charaType = CharaType.Chara10,
			FanNum = 600,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan10_03",
			ID = FanItemID.Fan10_03,
			charaType = CharaType.Chara10,
			FanNum = 1200,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.ReachStick081,
				ItemID.ReachStick083,
				ItemID.ReachStick082,
				ItemID.ReachStick084
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan10_04",
			ID = FanItemID.Fan10_04,
			charaType = CharaType.Chara10,
			FanNum = 2000,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan10_05",
			ID = FanItemID.Fan10_05,
			charaType = CharaType.Chara10,
			FanNum = 3000,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.PlayerAccessory086,
				ItemID.PlayerAccessory088,
				ItemID.PlayerAccessory087,
				ItemID.PlayerAccessory089
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan10_06",
			ID = FanItemID.Fan10_06,
			charaType = CharaType.Chara10,
			FanNum = 4500,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan10_07",
			ID = FanItemID.Fan10_07,
			charaType = CharaType.Chara10,
			FanNum = 6000,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan10_08",
			ID = FanItemID.Fan10_08,
			charaType = CharaType.Chara10,
			FanNum = 9000,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.TableSticker104,
				ItemID.TableSticker106,
				ItemID.TableSticker105,
				ItemID.TableSticker107
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan10_09",
			ID = FanItemID.Fan10_09,
			charaType = CharaType.Chara10,
			FanNum = 12000,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan10_10",
			ID = FanItemID.Fan10_10,
			charaType = CharaType.Chara10,
			FanNum = 20000,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.PlayerAccessory090,
				ItemID.PlayerAccessory092,
				ItemID.PlayerAccessory091,
				ItemID.PlayerAccessory093
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan10_11",
			ID = FanItemID.Fan10_11,
			charaType = CharaType.Chara10,
			FanNum = 30000,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.ReachStick085,
				ItemID.ReachStick086,
				ItemID.ReachStick087,
				ItemID.ReachStick088
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan11_01",
			ID = FanItemID.Fan11_01,
			charaType = CharaType.Chara11,
			FanNum = 300,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.TableSticker128,
				ItemID.TableSticker130,
				ItemID.TableSticker129,
				ItemID.TableSticker131
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan11_02",
			ID = FanItemID.Fan11_02,
			charaType = CharaType.Chara11,
			FanNum = 600,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan11_03",
			ID = FanItemID.Fan11_03,
			charaType = CharaType.Chara11,
			FanNum = 1200,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.ReachStick094,
				ItemID.ReachStick096,
				ItemID.ReachStick095,
				ItemID.ReachStick097
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan11_04",
			ID = FanItemID.Fan11_04,
			charaType = CharaType.Chara11,
			FanNum = 2000,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan11_05",
			ID = FanItemID.Fan11_05,
			charaType = CharaType.Chara11,
			FanNum = 3000,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.PlayerAccessory113,
				ItemID.PlayerAccessory115,
				ItemID.PlayerAccessory114,
				ItemID.PlayerAccessory116
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan11_06",
			ID = FanItemID.Fan11_06,
			charaType = CharaType.Chara11,
			FanNum = 4500,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan11_07",
			ID = FanItemID.Fan11_07,
			charaType = CharaType.Chara11,
			FanNum = 6000,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan11_08",
			ID = FanItemID.Fan11_08,
			charaType = CharaType.Chara11,
			FanNum = 9000,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.TableSticker132,
				ItemID.TableSticker134,
				ItemID.TableSticker133,
				ItemID.TableSticker135
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan11_09",
			ID = FanItemID.Fan11_09,
			charaType = CharaType.Chara11,
			FanNum = 12000,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan11_10",
			ID = FanItemID.Fan11_10,
			charaType = CharaType.Chara11,
			FanNum = 20000,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.PlayerAccessory117,
				ItemID.PlayerAccessory119,
				ItemID.PlayerAccessory118,
				ItemID.PlayerAccessory120
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan11_11",
			ID = FanItemID.Fan11_11,
			charaType = CharaType.Chara11,
			FanNum = 30000,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.ReachStick098,
				ItemID.ReachStick099,
				ItemID.ReachStick100,
				ItemID.ReachStick101
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan12_01",
			ID = FanItemID.Fan12_01,
			charaType = CharaType.Chara12,
			FanNum = 300,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.TableSticker150,
				ItemID.TableSticker152,
				ItemID.TableSticker151,
				ItemID.TableSticker153
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan12_02",
			ID = FanItemID.Fan12_02,
			charaType = CharaType.Chara12,
			FanNum = 600,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan12_03",
			ID = FanItemID.Fan12_03,
			charaType = CharaType.Chara12,
			FanNum = 1200,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.ReachStick105,
				ItemID.ReachStick107,
				ItemID.ReachStick106,
				ItemID.ReachStick108
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan12_04",
			ID = FanItemID.Fan12_04,
			charaType = CharaType.Chara12,
			FanNum = 2000,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan12_05",
			ID = FanItemID.Fan12_05,
			charaType = CharaType.Chara12,
			FanNum = 3000,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.PlayerAccessory149,
				ItemID.PlayerAccessory151,
				ItemID.PlayerAccessory150,
				ItemID.PlayerAccessory152
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan12_06",
			ID = FanItemID.Fan12_06,
			charaType = CharaType.Chara12,
			FanNum = 4500,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan12_07",
			ID = FanItemID.Fan12_07,
			charaType = CharaType.Chara12,
			FanNum = 6000,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan12_08",
			ID = FanItemID.Fan12_08,
			charaType = CharaType.Chara12,
			FanNum = 9000,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.TableSticker154,
				ItemID.TableSticker156,
				ItemID.TableSticker155,
				ItemID.TableSticker157
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan12_09",
			ID = FanItemID.Fan12_09,
			charaType = CharaType.Chara12,
			FanNum = 12000,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan12_10",
			ID = FanItemID.Fan12_10,
			charaType = CharaType.Chara12,
			FanNum = 20000,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.PlayerAccessory153,
				ItemID.PlayerAccessory155,
				ItemID.PlayerAccessory154,
				ItemID.PlayerAccessory156
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan12_11",
			ID = FanItemID.Fan12_11,
			charaType = CharaType.Chara12,
			FanNum = 30000,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.ReachStick109,
				ItemID.ReachStick110,
				ItemID.ReachStick111,
				ItemID.ReachStick112
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan13_01",
			ID = FanItemID.Fan13_01,
			charaType = CharaType.Chara13,
			FanNum = 300,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.TableSticker188,
				ItemID.TableSticker190,
				ItemID.TableSticker189,
				ItemID.TableSticker191
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan13_02",
			ID = FanItemID.Fan13_02,
			charaType = CharaType.Chara13,
			FanNum = 600,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan13_03",
			ID = FanItemID.Fan13_03,
			charaType = CharaType.Chara13,
			FanNum = 1200,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.ReachStick123,
				ItemID.ReachStick125,
				ItemID.ReachStick124,
				ItemID.ReachStick126
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan13_04",
			ID = FanItemID.Fan13_04,
			charaType = CharaType.Chara13,
			FanNum = 2000,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan13_05",
			ID = FanItemID.Fan13_05,
			charaType = CharaType.Chara13,
			FanNum = 3000,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.PlayerAccessory184,
				ItemID.PlayerAccessory186,
				ItemID.PlayerAccessory185,
				ItemID.PlayerAccessory187
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan13_06",
			ID = FanItemID.Fan13_06,
			charaType = CharaType.Chara13,
			FanNum = 4500,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan13_07",
			ID = FanItemID.Fan13_07,
			charaType = CharaType.Chara13,
			FanNum = 6000,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan13_08",
			ID = FanItemID.Fan13_08,
			charaType = CharaType.Chara13,
			FanNum = 9000,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.TableSticker192,
				ItemID.TableSticker194,
				ItemID.TableSticker193,
				ItemID.TableSticker195
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan13_09",
			ID = FanItemID.Fan13_09,
			charaType = CharaType.Chara13,
			FanNum = 12000,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan13_10",
			ID = FanItemID.Fan13_10,
			charaType = CharaType.Chara13,
			FanNum = 20000,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.PlayerAccessory188,
				ItemID.PlayerAccessory190,
				ItemID.PlayerAccessory189,
				ItemID.PlayerAccessory191
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan13_11",
			ID = FanItemID.Fan13_11,
			charaType = CharaType.Chara13,
			FanNum = 30000,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.ReachStick127,
				ItemID.ReachStick129,
				ItemID.ReachStick128,
				ItemID.ReachStick130
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan14_01",
			ID = FanItemID.Fan14_01,
			charaType = CharaType.Chara14,
			FanNum = 300,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.TableSticker210,
				ItemID.TableSticker212,
				ItemID.TableSticker211,
				ItemID.TableSticker213
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan14_02",
			ID = FanItemID.Fan14_02,
			charaType = CharaType.Chara14,
			FanNum = 600,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan14_03",
			ID = FanItemID.Fan14_03,
			charaType = CharaType.Chara14,
			FanNum = 1200,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.ReachStick135,
				ItemID.ReachStick137,
				ItemID.ReachStick136,
				ItemID.ReachStick138
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan14_04",
			ID = FanItemID.Fan14_04,
			charaType = CharaType.Chara14,
			FanNum = 2000,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan14_05",
			ID = FanItemID.Fan14_05,
			charaType = CharaType.Chara14,
			FanNum = 3000,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.PlayerAccessory196,
				ItemID.PlayerAccessory198,
				ItemID.PlayerAccessory197,
				ItemID.PlayerAccessory199
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan14_06",
			ID = FanItemID.Fan14_06,
			charaType = CharaType.Chara14,
			FanNum = 4500,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan14_07",
			ID = FanItemID.Fan14_07,
			charaType = CharaType.Chara14,
			FanNum = 6000,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan14_08",
			ID = FanItemID.Fan14_08,
			charaType = CharaType.Chara14,
			FanNum = 9000,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.TableSticker214,
				ItemID.TableSticker216,
				ItemID.TableSticker215,
				ItemID.TableSticker217
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan14_09",
			ID = FanItemID.Fan14_09,
			charaType = CharaType.Chara14,
			FanNum = 12000,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan14_10",
			ID = FanItemID.Fan14_10,
			charaType = CharaType.Chara14,
			FanNum = 20000,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.PlayerAccessory200,
				ItemID.PlayerAccessory202,
				ItemID.PlayerAccessory201,
				ItemID.PlayerAccessory203
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan14_11",
			ID = FanItemID.Fan14_11,
			charaType = CharaType.Chara14,
			FanNum = 30000,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.ReachStick139,
				ItemID.ReachStick141,
				ItemID.ReachStick140,
				ItemID.ReachStick142
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan15_01",
			ID = FanItemID.Fan15_01,
			charaType = CharaType.Chara15,
			FanNum = 300,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.TableSticker221,
				ItemID.TableSticker223,
				ItemID.TableSticker222,
				ItemID.TableSticker224
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan15_02",
			ID = FanItemID.Fan15_02,
			charaType = CharaType.Chara15,
			FanNum = 600,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan15_03",
			ID = FanItemID.Fan15_03,
			charaType = CharaType.Chara15,
			FanNum = 1200,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.ReachStick144,
				ItemID.ReachStick146,
				ItemID.ReachStick145,
				ItemID.ReachStick147
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan15_04",
			ID = FanItemID.Fan15_04,
			charaType = CharaType.Chara15,
			FanNum = 2000,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan15_05",
			ID = FanItemID.Fan15_05,
			charaType = CharaType.Chara15,
			FanNum = 3000,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.PlayerAccessory208,
				ItemID.PlayerAccessory210,
				ItemID.PlayerAccessory209,
				ItemID.PlayerAccessory211
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan15_06",
			ID = FanItemID.Fan15_06,
			charaType = CharaType.Chara15,
			FanNum = 4500,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan15_07",
			ID = FanItemID.Fan15_07,
			charaType = CharaType.Chara15,
			FanNum = 6000,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan15_08",
			ID = FanItemID.Fan15_08,
			charaType = CharaType.Chara15,
			FanNum = 9000,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.TableSticker225,
				ItemID.TableSticker227,
				ItemID.TableSticker226,
				ItemID.TableSticker228
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan15_09",
			ID = FanItemID.Fan15_09,
			charaType = CharaType.Chara15,
			FanNum = 12000,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan15_10",
			ID = FanItemID.Fan15_10,
			charaType = CharaType.Chara15,
			FanNum = 20000,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.PlayerAccessory212,
				ItemID.PlayerAccessory214,
				ItemID.PlayerAccessory213,
				ItemID.PlayerAccessory215
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan15_11",
			ID = FanItemID.Fan15_11,
			charaType = CharaType.Chara15,
			FanNum = 30000,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.ReachStick148,
				ItemID.ReachStick150,
				ItemID.ReachStick149,
				ItemID.ReachStick151
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan16_01",
			ID = FanItemID.Fan16_01,
			charaType = CharaType.Chara16,
			FanNum = 300,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.TableSticker238,
				ItemID.TableSticker240,
				ItemID.TableSticker239,
				ItemID.TableSticker241
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan16_02",
			ID = FanItemID.Fan16_02,
			charaType = CharaType.Chara16,
			FanNum = 600,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan16_03",
			ID = FanItemID.Fan16_03,
			charaType = CharaType.Chara16,
			FanNum = 1200,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.ReachStick172,
				ItemID.ReachStick174,
				ItemID.ReachStick173,
				ItemID.ReachStick175
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan16_04",
			ID = FanItemID.Fan16_04,
			charaType = CharaType.Chara16,
			FanNum = 2000,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan16_05",
			ID = FanItemID.Fan16_05,
			charaType = CharaType.Chara16,
			FanNum = 3000,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.PlayerAccessory223,
				ItemID.PlayerAccessory225,
				ItemID.PlayerAccessory224,
				ItemID.PlayerAccessory226
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan16_06",
			ID = FanItemID.Fan16_06,
			charaType = CharaType.Chara16,
			FanNum = 4500,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan16_07",
			ID = FanItemID.Fan16_07,
			charaType = CharaType.Chara16,
			FanNum = 6000,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan16_08",
			ID = FanItemID.Fan16_08,
			charaType = CharaType.Chara16,
			FanNum = 9000,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.TableSticker242,
				ItemID.TableSticker244,
				ItemID.TableSticker243,
				ItemID.TableSticker245
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan16_09",
			ID = FanItemID.Fan16_09,
			charaType = CharaType.Chara16,
			FanNum = 12000,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan16_10",
			ID = FanItemID.Fan16_10,
			charaType = CharaType.Chara16,
			FanNum = 20000,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.PlayerAccessory227,
				ItemID.PlayerAccessory229,
				ItemID.PlayerAccessory228,
				ItemID.PlayerAccessory230
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan16_11",
			ID = FanItemID.Fan16_11,
			charaType = CharaType.Chara16,
			FanNum = 30000,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.ReachStick176,
				ItemID.ReachStick178,
				ItemID.ReachStick177,
				ItemID.ReachStick179
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan17_01",
			ID = FanItemID.Fan17_01,
			charaType = CharaType.Chara17,
			FanNum = 300,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.TableSticker258,
				ItemID.TableSticker260,
				ItemID.TableSticker259,
				ItemID.TableSticker261
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan17_02",
			ID = FanItemID.Fan17_02,
			charaType = CharaType.Chara17,
			FanNum = 600,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan17_03",
			ID = FanItemID.Fan17_03,
			charaType = CharaType.Chara17,
			FanNum = 1200,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.ReachStick184,
				ItemID.ReachStick186,
				ItemID.ReachStick185,
				ItemID.ReachStick187
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan17_04",
			ID = FanItemID.Fan17_04,
			charaType = CharaType.Chara17,
			FanNum = 2000,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan17_05",
			ID = FanItemID.Fan17_05,
			charaType = CharaType.Chara17,
			FanNum = 3000,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.PlayerAccessory241,
				ItemID.PlayerAccessory243,
				ItemID.PlayerAccessory242,
				ItemID.PlayerAccessory244
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan17_06",
			ID = FanItemID.Fan17_06,
			charaType = CharaType.Chara17,
			FanNum = 4500,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan17_07",
			ID = FanItemID.Fan17_07,
			charaType = CharaType.Chara17,
			FanNum = 6000,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan17_08",
			ID = FanItemID.Fan17_08,
			charaType = CharaType.Chara17,
			FanNum = 9000,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.TableSticker262,
				ItemID.TableSticker264,
				ItemID.TableSticker263,
				ItemID.TableSticker265
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan17_09",
			ID = FanItemID.Fan17_09,
			charaType = CharaType.Chara17,
			FanNum = 12000,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan17_10",
			ID = FanItemID.Fan17_10,
			charaType = CharaType.Chara17,
			FanNum = 20000,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.PlayerAccessory245,
				ItemID.PlayerAccessory247,
				ItemID.PlayerAccessory246,
				ItemID.PlayerAccessory248
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan17_11",
			ID = FanItemID.Fan17_11,
			charaType = CharaType.Chara17,
			FanNum = 30000,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.ReachStick188,
				ItemID.ReachStick190,
				ItemID.ReachStick189,
				ItemID.ReachStick191
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan18_01",
			ID = FanItemID.Fan18_01,
			charaType = CharaType.Chara18,
			FanNum = 300,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.TableSticker278,
				ItemID.TableSticker280,
				ItemID.TableSticker279,
				ItemID.TableSticker281
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan18_02",
			ID = FanItemID.Fan18_02,
			charaType = CharaType.Chara18,
			FanNum = 600,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan18_03",
			ID = FanItemID.Fan18_03,
			charaType = CharaType.Chara18,
			FanNum = 1200,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.ReachStick194,
				ItemID.ReachStick196,
				ItemID.ReachStick195,
				ItemID.ReachStick197
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan18_04",
			ID = FanItemID.Fan18_04,
			charaType = CharaType.Chara18,
			FanNum = 2000,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan18_05",
			ID = FanItemID.Fan18_05,
			charaType = CharaType.Chara18,
			FanNum = 3000,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.PlayerAccessory256,
				ItemID.PlayerAccessory258,
				ItemID.PlayerAccessory257,
				ItemID.PlayerAccessory259
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan18_06",
			ID = FanItemID.Fan18_06,
			charaType = CharaType.Chara18,
			FanNum = 4500,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan18_07",
			ID = FanItemID.Fan18_07,
			charaType = CharaType.Chara18,
			FanNum = 6000,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan18_08",
			ID = FanItemID.Fan18_08,
			charaType = CharaType.Chara18,
			FanNum = 9000,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.TableSticker282,
				ItemID.TableSticker284,
				ItemID.TableSticker283,
				ItemID.TableSticker285
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan18_09",
			ID = FanItemID.Fan18_09,
			charaType = CharaType.Chara18,
			FanNum = 12000,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan18_10",
			ID = FanItemID.Fan18_10,
			charaType = CharaType.Chara18,
			FanNum = 20000,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.PlayerAccessory260,
				ItemID.PlayerAccessory262,
				ItemID.PlayerAccessory261,
				ItemID.PlayerAccessory263
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan18_11",
			ID = FanItemID.Fan18_11,
			charaType = CharaType.Chara18,
			FanNum = 30000,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.ReachStick198,
				ItemID.ReachStick200,
				ItemID.ReachStick199,
				ItemID.ReachStick201
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan19_01",
			ID = FanItemID.Fan19_01,
			charaType = CharaType.Chara19,
			FanNum = 300,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.TableSticker303,
				ItemID.TableSticker305,
				ItemID.TableSticker304,
				ItemID.TableSticker306
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan19_02",
			ID = FanItemID.Fan19_02,
			charaType = CharaType.Chara19,
			FanNum = 600,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan19_03",
			ID = FanItemID.Fan19_03,
			charaType = CharaType.Chara19,
			FanNum = 1200,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.ReachStick205,
				ItemID.ReachStick207,
				ItemID.ReachStick206,
				ItemID.ReachStick208
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan19_04",
			ID = FanItemID.Fan19_04,
			charaType = CharaType.Chara19,
			FanNum = 2000,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan19_05",
			ID = FanItemID.Fan19_05,
			charaType = CharaType.Chara19,
			FanNum = 3000,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.PlayerAccessory267,
				ItemID.PlayerAccessory269,
				ItemID.PlayerAccessory268,
				ItemID.PlayerAccessory270
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan19_06",
			ID = FanItemID.Fan19_06,
			charaType = CharaType.Chara19,
			FanNum = 4500,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan19_07",
			ID = FanItemID.Fan19_07,
			charaType = CharaType.Chara19,
			FanNum = 6000,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan19_08",
			ID = FanItemID.Fan19_08,
			charaType = CharaType.Chara19,
			FanNum = 9000,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.TableSticker307,
				ItemID.TableSticker309,
				ItemID.TableSticker308,
				ItemID.TableSticker310
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan19_09",
			ID = FanItemID.Fan19_09,
			charaType = CharaType.Chara19,
			FanNum = 12000,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan19_10",
			ID = FanItemID.Fan19_10,
			charaType = CharaType.Chara19,
			FanNum = 20000,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.PlayerAccessory271,
				ItemID.PlayerAccessory273,
				ItemID.PlayerAccessory272,
				ItemID.PlayerAccessory274
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan19_11",
			ID = FanItemID.Fan19_11,
			charaType = CharaType.Chara19,
			FanNum = 30000,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.ReachStick209,
				ItemID.ReachStick211,
				ItemID.ReachStick210,
				ItemID.ReachStick212
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan20_01",
			ID = FanItemID.Fan20_01,
			charaType = CharaType.Chara20,
			FanNum = 300,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.TableSticker325,
				ItemID.TableSticker327,
				ItemID.TableSticker326,
				ItemID.TableSticker328
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan20_02",
			ID = FanItemID.Fan20_02,
			charaType = CharaType.Chara20,
			FanNum = 600,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan20_03",
			ID = FanItemID.Fan20_03,
			charaType = CharaType.Chara20,
			FanNum = 1200,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.ReachStick220,
				ItemID.ReachStick222,
				ItemID.ReachStick221,
				ItemID.ReachStick223
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan20_04",
			ID = FanItemID.Fan20_04,
			charaType = CharaType.Chara20,
			FanNum = 2000,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan20_05",
			ID = FanItemID.Fan20_05,
			charaType = CharaType.Chara20,
			FanNum = 3000,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.PlayerAccessory360,
				ItemID.PlayerAccessory362,
				ItemID.PlayerAccessory361,
				ItemID.PlayerAccessory363
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan20_06",
			ID = FanItemID.Fan20_06,
			charaType = CharaType.Chara20,
			FanNum = 4500,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan20_07",
			ID = FanItemID.Fan20_07,
			charaType = CharaType.Chara20,
			FanNum = 6000,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan20_08",
			ID = FanItemID.Fan20_08,
			charaType = CharaType.Chara20,
			FanNum = 9000,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.TableSticker329,
				ItemID.TableSticker331,
				ItemID.TableSticker330,
				ItemID.TableSticker332
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan20_09",
			ID = FanItemID.Fan20_09,
			charaType = CharaType.Chara20,
			FanNum = 12000,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan20_10",
			ID = FanItemID.Fan20_10,
			charaType = CharaType.Chara20,
			FanNum = 20000,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.PlayerAccessory364,
				ItemID.PlayerAccessory366,
				ItemID.PlayerAccessory365,
				ItemID.PlayerAccessory367
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan20_11",
			ID = FanItemID.Fan20_11,
			charaType = CharaType.Chara20,
			FanNum = 30000,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.ReachStick224,
				ItemID.ReachStick226,
				ItemID.ReachStick225,
				ItemID.ReachStick227
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan21_01",
			ID = FanItemID.Fan21_01,
			charaType = CharaType.Chara21,
			FanNum = 300,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.TableSticker349,
				ItemID.TableSticker351,
				ItemID.TableSticker350,
				ItemID.TableSticker352
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan21_02",
			ID = FanItemID.Fan21_02,
			charaType = CharaType.Chara21,
			FanNum = 600,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan21_03",
			ID = FanItemID.Fan21_03,
			charaType = CharaType.Chara21,
			FanNum = 1200,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.ReachStick232,
				ItemID.ReachStick234,
				ItemID.ReachStick233,
				ItemID.ReachStick235
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan21_04",
			ID = FanItemID.Fan21_04,
			charaType = CharaType.Chara21,
			FanNum = 2000,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan21_05",
			ID = FanItemID.Fan21_05,
			charaType = CharaType.Chara21,
			FanNum = 3000,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.PlayerAccessory376,
				ItemID.PlayerAccessory378,
				ItemID.PlayerAccessory377,
				ItemID.PlayerAccessory379
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan21_06",
			ID = FanItemID.Fan21_06,
			charaType = CharaType.Chara21,
			FanNum = 4500,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan21_07",
			ID = FanItemID.Fan21_07,
			charaType = CharaType.Chara21,
			FanNum = 6000,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan21_08",
			ID = FanItemID.Fan21_08,
			charaType = CharaType.Chara21,
			FanNum = 9000,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.TableSticker353,
				ItemID.TableSticker355,
				ItemID.TableSticker354,
				ItemID.TableSticker356
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan21_09",
			ID = FanItemID.Fan21_09,
			charaType = CharaType.Chara21,
			FanNum = 12000,
			UnlockItemIDs = new ItemID[4]
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan21_10",
			ID = FanItemID.Fan21_10,
			charaType = CharaType.Chara21,
			FanNum = 20000,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.PlayerAccessory380,
				ItemID.PlayerAccessory382,
				ItemID.PlayerAccessory381,
				ItemID.PlayerAccessory383
			}
		},
		new FanMasterData
		{
			ObjectID = "OID_Fan21_11",
			ID = FanItemID.Fan21_11,
			charaType = CharaType.Chara21,
			FanNum = 30000,
			UnlockItemIDs = new ItemID[4]
			{
				ItemID.ReachStick236,
				ItemID.ReachStick238,
				ItemID.ReachStick237,
				ItemID.ReachStick239
			}
		}
	};

	private static float DefenceFanRateOnTsumo = 1f / 3f;

	public static FanMasterData GetFanData(FanItemID itemID)
	{
		return m_fanMastarTable.Find((FanMasterData a) => a.ID == itemID);
	}

	public static List<int> FanNumList(CharaType chara)
	{
		return (from x in m_fanMastarTable
			where x.charaType == chara
			select x.FanNum into x
			orderby x
			select x).ToList();
	}

	public static Dictionary<int, (string, int)> FanMissionTable(CharaType chara, FanType fanType)
	{
		IOrderedEnumerable<FanMasterData> orderedEnumerable = from x in m_fanMastarTable
			where x.charaType == chara && x.UnlockItemIDs[(int)fanType] != ItemID.None
			orderby x.FanNum
			select x;
		Dictionary<int, (string, int)> dictionary = new Dictionary<int, (string, int)>();
		foreach (FanMasterData item in orderedEnumerable)
		{
			dictionary.Add(item.FanNum, (CustomizeItemMaster.ToObjectID(item.UnlockItemIDs[(int)fanType]), 1));
		}
		return dictionary;
	}

	private static float LuckFansForOmoteDora(int players)
	{
		return players switch
		{
			2 => 11.1f, 
			3 => 18.53f, 
			4 => 31f, 
			_ => 0f, 
		};
	}

	private static float LuckFansForUraDora(int players)
	{
		return players switch
		{
			2 => 59.9f, 
			3 => 97.7f, 
			4 => 187f, 
			_ => 0f, 
		};
	}

	private static float SpeedFansForAgari(int players)
	{
		return players switch
		{
			2 => 26.1f, 
			3 => 34.2f, 
			4 => 45f, 
			_ => 0f, 
		};
	}

	private static float SpeedFansForAgariTurn(int players, int turn)
	{
		return players switch
		{
			4 => turn switch
			{
				0 => 200, 
				1 => 180, 
				2 => 170, 
				3 => 160, 
				4 => 130, 
				5 => 120, 
				6 => 90, 
				7 => 80, 
				8 => 70, 
				9 => 60, 
				10 => 45, 
				11 => 40, 
				12 => 35, 
				13 => 20, 
				14 => 15, 
				15 => 12, 
				16 => 10, 
				17 => 8, 
				_ => 6, 
			}, 
			3 => turn switch
			{
				0 => 150, 
				1 => 135, 
				2 => 128, 
				3 => 120, 
				4 => 98, 
				5 => 90, 
				6 => 68, 
				7 => 60, 
				8 => 53, 
				9 => 45, 
				10 => 34, 
				11 => 30, 
				12 => 26, 
				13 => 15, 
				14 => 11, 
				15 => 9, 
				16 => 8, 
				17 => 6, 
				_ => 5, 
			}, 
			2 => turn switch
			{
				0 => 110, 
				1 => 105, 
				2 => 100, 
				3 => 94, 
				4 => 78, 
				5 => 68, 
				6 => 54, 
				7 => 48, 
				8 => 40, 
				9 => 33, 
				10 => 25, 
				11 => 24, 
				12 => 20, 
				13 => 12, 
				14 => 9, 
				15 => 7, 
				16 => 6, 
				17 => 5, 
				_ => 4, 
			}, 
			_ => 0, 
		};
	}

	private static float AttackFansPerHan(int players)
	{
		return players switch
		{
			2 => 4.6f, 
			3 => 8f, 
			4 => 14f, 
			_ => 0f, 
		};
	}

	private static float AttackFansForReach(int players)
	{
		return players switch
		{
			2 => 29.8f, 
			3 => 42.5f, 
			4 => 61f, 
			_ => 0f, 
		};
	}

	private static float DefenceFansForRyukyoku(int players)
	{
		return players switch
		{
			2 => 4, 
			3 => 5, 
			4 => 5, 
			_ => 0, 
		};
	}

	private static float DefenceFansForNotFurikomi(int players)
	{
		return players switch
		{
			2 => 8, 
			3 => 10, 
			4 => 12, 
			_ => 0, 
		};
	}

	private static float DefenceFansForFurikomi(int players, Han han)
	{
		return players switch
		{
			4 => han switch
			{
				Han.Han1 => 4, 
				Han.Han2 => 4, 
				Han.Han3 => 4, 
				Han.Han4 => 4, 
				Han.Mangan => 2, 
				Han.Haneman => 1, 
				_ => 0, 
			}, 
			3 => han switch
			{
				Han.Han1 => 3, 
				Han.Han2 => 3, 
				Han.Han3 => 3, 
				Han.Han4 => 3, 
				Han.Mangan => 2, 
				Han.Haneman => 1, 
				_ => 0, 
			}, 
			2 => han switch
			{
				Han.Han1 => 2, 
				Han.Han2 => 2, 
				Han.Han3 => 2, 
				Han.Han4 => 2, 
				Han.Mangan => 1, 
				_ => 0, 
			}, 
			_ => 0, 
		};
	}

	private static float DefenceFansForEvasion(int players, Han han)
	{
		return players switch
		{
			4 => han switch
			{
				Han.Nothing => 0, 
				Han.Han1 => 10, 
				Han.Han2 => 10, 
				Han.Han3 => 10, 
				Han.Han4 => 10, 
				Han.Mangan => 30, 
				Han.Haneman => 75, 
				Han.Baiman => 150, 
				Han.Sanbaiman => 180, 
				_ => 200, 
			}, 
			3 => han switch
			{
				Han.Nothing => 0, 
				Han.Han1 => 10, 
				Han.Han2 => 10, 
				Han.Han3 => 10, 
				Han.Han4 => 10, 
				Han.Mangan => 30, 
				Han.Haneman => 70, 
				Han.Baiman => 120, 
				Han.Sanbaiman => 140, 
				_ => 150, 
			}, 
			2 => han switch
			{
				Han.Nothing => 0, 
				Han.Han1 => 30, 
				Han.Han2 => 30, 
				Han.Han3 => 30, 
				Han.Han4 => 30, 
				Han.Mangan => 50, 
				Han.Haneman => 100, 
				Han.Baiman => 200, 
				Han.Sanbaiman => 250, 
				_ => 300, 
			}, 
			_ => 0, 
		};
	}

	private static float DefenceFansForMachiKeep(int players)
	{
		return players switch
		{
			4 => 1, 
			3 => 1, 
			2 => 2, 
			_ => 0, 
		};
	}

	public static (int luckFan, int speedFan, int attackFan, int defenceFan) FansOnTsumo(MahjongPlayFlow flow, TsumoAgari tsumoCommand)
	{
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		float num4 = 0f;
		int count = flow.Players.Count;
		Player me = null;
		foreach (Player player in flow.Players)
		{
			if (player.m_cha == Cha.Jicha)
			{
				me = player;
			}
		}
		if (me == null)
		{
			return (luckFan: 0, speedFan: 0, attackFan: 0, defenceFan: 0);
		}
		bool num5 = flow.Players[tsumoCommand.PlayerIndex].m_cha == Cha.Jicha;
		if (num5)
		{
			int num6 = me.hand.GetOmoteDoraCount() + me.hand.GetAkaDoraCount() + me.meld.GetOmoteDoraCount() + me.meld.GetAkaDoraCount();
			num += (float)num6 * LuckFansForOmoteDora(count);
			int num7 = me.hand.GetUraDoraCount() + me.meld.GetUraDoraCount();
			bool flag = (tsumoCommand.Yaku.YakuBit & 0x400000000000L) != 0;
			num += (float)(num7 + (flag ? 1 : 0)) * LuckFansForUraDora(count);
		}
		if (num5)
		{
			num2 += SpeedFansForAgari(count);
			int count2 = me.dump.TileList.Count;
			num2 += SpeedFansForAgariTurn(count, count2);
		}
		if (me.m_isReach)
		{
			num3 += AttackFansForReach(count);
		}
		if (num5)
		{
			if (tsumoCommand.Yaku.HanID >= 9)
			{
				int num8 = tsumoCommand.Yaku.HanID - 9 + 1;
				num3 += AttackFansPerHan(count) * (float)num8 * 13f;
			}
			else
			{
				int hanNum = tsumoCommand.Yaku.HanNum;
				num3 += AttackFansPerHan(count) * (float)hanNum;
			}
		}
		num4 += DefenceFansForNotFurikomi(count);
		if (!num5)
		{
			num4 += DefenceFansForEvasion(count, (Han)tsumoCommand.Yaku.HanID) * DefenceFanRateOnTsumo;
			List<Pai> source = (from pai in flow.m_haiNavi[tsumoCommand.PlayerIndex].MachiHais().Select(PaiExtention.HaiIDToPai).Distinct()
					.ToList()
				where me.hand.GetPais().Contains(pai)
				select pai).ToList();
			num4 += (float)source.Count() * DefenceFansForMachiKeep(count);
		}
		return (luckFan: (int)Math.Round(num), speedFan: (int)Math.Round(num2), attackFan: (int)Math.Round(num3), defenceFan: (int)Math.Round(num4));
	}

	public static (int luckFan, int speedFan, int attackFan, int defenceFan) FansOnRon(MahjongPlayFlow flow, Ron ronCommand)
	{
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		float num4 = 0f;
		int count = flow.Players.Count;
		int num5 = flow.Players.FindIndex((Player player) => player.m_cha == Cha.Jicha);
		Player me = flow.Players.ElementAt(num5);
		bool flag = ronCommand.RonFlag[num5] > 0;
		Player player2 = flow.Players[ronCommand.PlayerIndex];
		(Player, int)[] source = (from pair in flow.Players.Select((Player player, int index) => (player: player, index: index))
			where ronCommand.RonFlag[pair.index] > 0
			select pair).ToArray();
		Player[] array = source.Select(((Player player, int index) p) => p.player).ToArray();
		if (me == null)
		{
			return (luckFan: 0, speedFan: 0, attackFan: 0, defenceFan: 0);
		}
		if (flag)
		{
			int num6 = me.hand.GetOmoteDoraCount() + me.hand.GetAkaDoraCount() + me.meld.GetOmoteDoraCount() + me.meld.GetAkaDoraCount();
			num += (float)num6 * LuckFansForOmoteDora(count);
			int num7 = me.hand.GetUraDoraCount() + me.meld.GetUraDoraCount();
			bool flag2 = (ronCommand.Yakus[num5].YakuBit & 0x400000000000L) != 0;
			num += (float)(num7 + (flag2 ? 1 : 0)) * LuckFansForUraDora(count);
		}
		if (flag)
		{
			num2 += SpeedFansForAgari(count);
			int count2 = me.dump.TileList.Count;
			num2 += SpeedFansForAgariTurn(count, count2);
		}
		if (me.m_isReach)
		{
			num3 += AttackFansForReach(count);
		}
		if (flag)
		{
			if (ronCommand.Yakus[num5].HanID >= 9)
			{
				int num8 = ronCommand.Yakus[num5].HanID - 9 + 1;
				num3 += AttackFansPerHan(count) * (float)num8 * 13f;
			}
			else
			{
				int hanNum = ronCommand.Yakus[num5].HanNum;
				num3 += AttackFansPerHan(count) * (float)hanNum;
			}
		}
		if (player2.m_cha != 0)
		{
			num4 += DefenceFansForNotFurikomi(count);
			foreach (var item3 in source.Where(((Player player, int index) p) => p.player.m_cha != Cha.Jicha))
			{
				int item = item3.Item2;
				num4 += DefenceFansForEvasion(count, (Han)ronCommand.Yakus[item].HanID);
			}
			List<Pai> source2 = (from pai in source.Where(((Player player, int index) pair) => pair.player.m_cha != Cha.Jicha).SelectMany(((Player player, int index) pair) => flow.m_haiNavi[pair.index].MachiHais().Select(PaiExtention.HaiIDToPai).Distinct()).ToList()
				where me.hand.GetPais().Contains(pai)
				select pai).ToList();
			num4 += (float)source2.Count() * DefenceFansForMachiKeep(count);
		}
		else if (array.Length <= 1 && array.Length == 1)
		{
			int item2 = source.First().Item2;
			num4 += DefenceFansForFurikomi(count, (Han)ronCommand.Yakus[item2].HanID);
		}
		return (luckFan: (int)Math.Round(num), speedFan: (int)Math.Round(num2), attackFan: (int)Math.Round(num3), defenceFan: (int)Math.Round(num4));
	}

	public static (int luckFan, int speedFan, int attackFan, int defenceFan) FansOnRyukyoku(MahjongPlayFlow flow, RyuKyoku ryukyokuCommand)
	{
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		float num4 = 0f;
		int count = flow.Players.Count;
		Player me = null;
		foreach (Player player in flow.Players)
		{
			if (player.m_cha == Cha.Jicha)
			{
				me = player;
			}
		}
		if (me == null)
		{
			return (luckFan: 0, speedFan: 0, attackFan: 0, defenceFan: 0);
		}
		if (me.m_isReach)
		{
			num3 += AttackFansForReach(count);
		}
		num4 += DefenceFansForNotFurikomi(count);
		num4 += DefenceFansForRyukyoku(count);
		List<Pai> source = (from pai in (from pair in flow.Players.Select((Player player, int index) => (player: player, index: index))
				where pair.player.m_cha != Cha.Jicha
				select pair).SelectMany(((Player player, int index) pair) => flow.m_haiNavi[pair.index].MachiHais()).Select(PaiExtention.HaiIDToPai).Distinct()
				.ToList()
			where me.hand.GetPais().Contains(pai)
			select pai).ToList();
		num4 += (float)source.Count() * DefenceFansForMachiKeep(count);
		return (luckFan: (int)Math.Round(num), speedFan: (int)Math.Round(num2), attackFan: (int)Math.Round(num3), defenceFan: (int)Math.Round(num4));
	}
}
