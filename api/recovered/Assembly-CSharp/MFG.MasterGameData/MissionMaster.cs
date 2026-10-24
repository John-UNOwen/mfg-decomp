using System.Collections.Generic;
using System.Linq;
using MFG.GameData;
using MFG.Types;
using MFG.Types.Mission;

namespace MFG.MasterGameData;

public class MissionMaster : MasterGameDataBase
{
	private static List<Item> _items = new List<Item>
	{
		new Item
		{
			MissionID = MissionID.Player_Vs4,
			Category = Category.Player,
			Kind = Kind.TaikyokuCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			TakuTypes = new List<TakuType> { TakuType.Tonpu },
			LoopType = LoopType.Numerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_GachaTicket",
			ItemNum = 10,
			FirstTerm = 1,
			CommonDifference = 1,
			LastTerm = 30,
			HideMissionID = MissionID.Player_Vs4_Endless
		},
		new Item
		{
			MissionID = MissionID.Player_Vs4_Endless,
			Category = Category.Player,
			Kind = Kind.TaikyokuCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			TakuTypes = new List<TakuType> { TakuType.Tonpu },
			LoopType = LoopType.Endless,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_GachaTicket",
			ItemNum = 10,
			FirstTerm = 40,
			CommonDifference = 10,
			RequiredMissionID = MissionID.Player_Vs4
		},
		new Item
		{
			MissionID = MissionID.Player_Vs3,
			Category = Category.Player,
			Kind = Kind.TaikyokuCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			TakuTypes = new List<TakuType> { TakuType.Sanma },
			LoopType = LoopType.Numerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_GachaTicket",
			ItemNum = 10,
			FirstTerm = 1,
			CommonDifference = 1,
			LastTerm = 30,
			HideMissionID = MissionID.Player_Vs3_Endless
		},
		new Item
		{
			MissionID = MissionID.Player_Vs3_Endless,
			Category = Category.Player,
			Kind = Kind.TaikyokuCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			TakuTypes = new List<TakuType> { TakuType.Sanma },
			LoopType = LoopType.Endless,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_GachaTicket",
			ItemNum = 10,
			FirstTerm = 40,
			CommonDifference = 10,
			RequiredMissionID = MissionID.Player_Vs3
		},
		new Item
		{
			MissionID = MissionID.Player_Vs2,
			Category = Category.Player,
			Kind = Kind.TaikyokuCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			TakuTypes = new List<TakuType> { TakuType.Nima },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_GachaTicket",
			ItemNum = 10,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Player_Top4,
			Category = Category.Player,
			Kind = Kind.TopCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			TakuTypes = new List<TakuType> { TakuType.Tonpu },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Wildcard",
			ItemNum = 3,
			NonNumericalAchievementList = new List<int> { 1, 3, 5, 10, 30, 50 },
			HideMissionID = MissionID.Player_Top4_Endless
		},
		new Item
		{
			MissionID = MissionID.Player_Top4_Endless,
			Category = Category.Player,
			Kind = Kind.TopCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			TakuTypes = new List<TakuType> { TakuType.Tonpu },
			LoopType = LoopType.Endless,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Wildcard",
			ItemNum = 3,
			FirstTerm = 100,
			CommonDifference = 50,
			RequiredMissionID = MissionID.Player_Top4
		},
		new Item
		{
			MissionID = MissionID.Player_Top3,
			Category = Category.Player,
			Kind = Kind.TopCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			TakuTypes = new List<TakuType> { TakuType.Sanma },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Wildcard",
			ItemNum = 3,
			NonNumericalAchievementList = new List<int> { 1, 3, 5, 10, 30, 50 },
			HideMissionID = MissionID.Player_Top3_Endless
		},
		new Item
		{
			MissionID = MissionID.Player_Top3_Endless,
			Category = Category.Player,
			Kind = Kind.TopCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			TakuTypes = new List<TakuType> { TakuType.Sanma },
			LoopType = LoopType.Endless,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Wildcard",
			ItemNum = 3,
			FirstTerm = 100,
			CommonDifference = 50,
			RequiredMissionID = MissionID.Player_Top3
		},
		new Item
		{
			MissionID = MissionID.Player_UntilDan10,
			Category = Category.Player,
			Kind = Kind.Dan,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			LoopType = LoopType.Numerical,
			DisplayType = DisplayType.All,
			IsProgressDisplay = false,
			ItemOID = "OID_FightSpirit_Wildcard",
			ItemNum = 5,
			FirstTerm = 2,
			CommonDifference = 1,
			LastTerm = 20,
			ExtraRewardList = new Dictionary<int, (string, int)>
			{
				{
					2,
					("OID_TableSticker033", 1)
				},
				{
					11,
					("OID_PlayerAccessory033", 1)
				}
			}
		},
		new Item
		{
			MissionID = MissionID.Player_UpperJansei,
			Category = Category.Player,
			Kind = Kind.Dan,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			LoopType = LoopType.Numerical,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_FightSpirit_Wildcard",
			ItemNum = 5,
			FirstTerm = 21,
			CommonDifference = 1,
			LastTerm = 1604,
			ExtraRewardList = new Dictionary<int, (string, int)>
			{
				{
					21,
					("OID_ReachStick034", 1)
				},
				{
					120,
					("OID_TableSticker099", 1)
				}
			}
		},
		new Item
		{
			MissionID = MissionID.Player_Login,
			Category = Category.Player,
			Kind = Kind.Login,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_TableSticker127",
			ItemNum = 1,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Player_Gacha,
			Category = Category.Player,
			Kind = Kind.Others,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_FightSpirit_Wildcard",
			ItemNum = 1,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Player_Gacha5,
			Category = Category.Player,
			Kind = Kind.Others,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_FightSpirit_Wildcard",
			ItemNum = 5,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Player_SpiritGym,
			Category = Category.Player,
			Kind = Kind.Others,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_FightSpirit_Wildcard",
			ItemNum = 1,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Player_CharaChange,
			Category = Category.Player,
			Kind = Kind.Others,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.MyRoom },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_FightSpirit_Wildcard",
			ItemNum = 1,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Player_SetCutin,
			Category = Category.Player,
			Kind = Kind.Others,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.MyRoom },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_FightSpirit_Wildcard",
			ItemNum = 1,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Player_SetGoods,
			Category = Category.Player,
			Kind = Kind.Others,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.MyRoom },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_FightSpirit_Wildcard",
			ItemNum = 1,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Yaku_Mangan,
			Category = Category.Yaku,
			Kind = Kind.ManganCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			OtherArgs = new List<object> { Han.Mangan },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Wildcard",
			ItemNum = 1,
			NonNumericalAchievementList = new List<int> { 1, 5, 10, 30, 50 },
			HideMissionID = MissionID.Yaku_Mangan_Endless
		},
		new Item
		{
			MissionID = MissionID.Yaku_Mangan_Endless,
			Category = Category.Yaku,
			Kind = Kind.ManganCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			OtherArgs = new List<object> { Han.Mangan },
			LoopType = LoopType.Endless,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Wildcard",
			ItemNum = 1,
			FirstTerm = 100,
			CommonDifference = 50,
			RequiredMissionID = MissionID.Yaku_Mangan
		},
		new Item
		{
			MissionID = MissionID.Yaku_Haneman,
			Category = Category.Yaku,
			Kind = Kind.ManganCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			OtherArgs = new List<object> { Han.Haneman },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Wildcard",
			ItemNum = 2,
			NonNumericalAchievementList = new List<int> { 1, 5, 10, 30, 50 },
			HideMissionID = MissionID.Yaku_Haneman_Endless
		},
		new Item
		{
			MissionID = MissionID.Yaku_Haneman_Endless,
			Category = Category.Yaku,
			Kind = Kind.ManganCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			OtherArgs = new List<object> { Han.Haneman },
			LoopType = LoopType.Endless,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Wildcard",
			ItemNum = 2,
			FirstTerm = 100,
			CommonDifference = 50,
			RequiredMissionID = MissionID.Yaku_Haneman
		},
		new Item
		{
			MissionID = MissionID.Yaku_Baiman,
			Category = Category.Yaku,
			Kind = Kind.ManganCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			OtherArgs = new List<object> { Han.Baiman },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Wildcard",
			ItemNum = 3,
			NonNumericalAchievementList = new List<int> { 1, 5, 10, 25 },
			HideMissionID = MissionID.Yaku_Baiman_Endless
		},
		new Item
		{
			MissionID = MissionID.Yaku_Baiman_Endless,
			Category = Category.Yaku,
			Kind = Kind.ManganCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			OtherArgs = new List<object> { Han.Baiman },
			LoopType = LoopType.Endless,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Wildcard",
			ItemNum = 3,
			FirstTerm = 50,
			CommonDifference = 25,
			RequiredMissionID = MissionID.Yaku_Baiman
		},
		new Item
		{
			MissionID = MissionID.Yaku_Sanbaiman,
			Category = Category.Yaku,
			Kind = Kind.ManganCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			OtherArgs = new List<object> { Han.Sanbaiman },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Wildcard",
			ItemNum = 4,
			NonNumericalAchievementList = new List<int> { 1, 3, 5, 10 },
			HideMissionID = MissionID.Yaku_Sanbaiman_Endless
		},
		new Item
		{
			MissionID = MissionID.Yaku_Sanbaiman_Endless,
			Category = Category.Yaku,
			Kind = Kind.ManganCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			OtherArgs = new List<object> { Han.Sanbaiman },
			LoopType = LoopType.Endless,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Wildcard",
			ItemNum = 4,
			FirstTerm = 20,
			CommonDifference = 10,
			RequiredMissionID = MissionID.Yaku_Sanbaiman
		},
		new Item
		{
			MissionID = MissionID.Yaku_Yakuman,
			Category = Category.Yaku,
			Kind = Kind.ManganCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			OtherArgs = new List<object> { Han.Yakuman },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Wildcard",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int> { 1, 3, 5 },
			HideMissionID = MissionID.Yaku_Yakuman_Endless
		},
		new Item
		{
			MissionID = MissionID.Yaku_Yakuman_Endless,
			Category = Category.Yaku,
			Kind = Kind.ManganCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			OtherArgs = new List<object> { Han.Yakuman },
			LoopType = LoopType.Endless,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Wildcard",
			ItemNum = 5,
			FirstTerm = 10,
			CommonDifference = 5,
			RequiredMissionID = MissionID.Yaku_Yakuman
		},
		new Item
		{
			MissionID = MissionID.Yaku_Yakuman2,
			Category = Category.Yaku,
			Kind = Kind.ManganCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			OtherArgs = new List<object> { Han.Yakuman2 },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_FightSpirit_Wildcard",
			ItemNum = 10,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Yaku_Yakuman3,
			Category = Category.Yaku,
			Kind = Kind.ManganCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			OtherArgs = new List<object> { Han.Yakuman3 },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_FightSpirit_Wildcard",
			ItemNum = 10,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Yaku_Tenho,
			Category = Category.Yaku,
			Kind = Kind.YakumanAgariCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			OtherArgs = new List<object> { Yaku.Tenho },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_FightSpirit_Wildcard",
			ItemNum = 5,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Yaku_Chiho,
			Category = Category.Yaku,
			Kind = Kind.YakumanAgariCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			OtherArgs = new List<object> { Yaku.Chiho },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_FightSpirit_Wildcard",
			ItemNum = 5,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Yaku_Tyuren9,
			Category = Category.Yaku,
			Kind = Kind.YakumanAgariCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			OtherArgs = new List<object> { Yaku.Tyuren9 },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_FightSpirit_Wildcard",
			ItemNum = 5,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Yaku_TyurenTanki,
			Category = Category.Yaku,
			Kind = Kind.YakumanAgariCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			OtherArgs = new List<object> { Yaku.TyurenTanki },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_FightSpirit_Wildcard",
			ItemNum = 5,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Yaku_Chinroto,
			Category = Category.Yaku,
			Kind = Kind.YakumanAgariCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			OtherArgs = new List<object> { Yaku.Chinroto },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_FightSpirit_Wildcard",
			ItemNum = 5,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Yaku_Tsuiso,
			Category = Category.Yaku,
			Kind = Kind.YakumanAgariCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			OtherArgs = new List<object> { Yaku.Tsuiso },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_FightSpirit_Wildcard",
			ItemNum = 5,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Yaku_Ryuiso,
			Category = Category.Yaku,
			Kind = Kind.YakumanAgariCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			OtherArgs = new List<object> { Yaku.Ryuiso },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_FightSpirit_Wildcard",
			ItemNum = 5,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Yaku_Kokushi13,
			Category = Category.Yaku,
			Kind = Kind.YakumanAgariCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			OtherArgs = new List<object> { Yaku.Kokushi13 },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_FightSpirit_Wildcard",
			ItemNum = 5,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Yaku_KokushiTanki,
			Category = Category.Yaku,
			Kind = Kind.YakumanAgariCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			OtherArgs = new List<object> { Yaku.KokushiTanki },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_FightSpirit_Wildcard",
			ItemNum = 5,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Yaku_Sukantsu,
			Category = Category.Yaku,
			Kind = Kind.YakumanAgariCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			OtherArgs = new List<object> { Yaku.Sukantsu },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_FightSpirit_Wildcard",
			ItemNum = 5,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Yaku_Suanko,
			Category = Category.Yaku,
			Kind = Kind.YakumanAgariCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			OtherArgs = new List<object> { Yaku.Suanko },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_FightSpirit_Wildcard",
			ItemNum = 5,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Yaku_SuankoTanki,
			Category = Category.Yaku,
			Kind = Kind.YakumanAgariCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			OtherArgs = new List<object> { Yaku.SuankoTanki },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_FightSpirit_Wildcard",
			ItemNum = 5,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Yaku_Daisushi,
			Category = Category.Yaku,
			Kind = Kind.YakumanAgariCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			OtherArgs = new List<object> { Yaku.Daisushi },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_FightSpirit_Wildcard",
			ItemNum = 5,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Yaku_Syosushi,
			Category = Category.Yaku,
			Kind = Kind.YakumanAgariCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			OtherArgs = new List<object> { Yaku.Syosushi },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_FightSpirit_Wildcard",
			ItemNum = 5,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Yaku_Daisangen,
			Category = Category.Yaku,
			Kind = Kind.YakumanAgariCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			OtherArgs = new List<object> { Yaku.Daisangen },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_FightSpirit_Wildcard",
			ItemNum = 5,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Character_01_Favo,
			Category = Category.Character,
			Kind = Kind.Favo,
			JudgeLocation = new List<JudgeLocation>
			{
				JudgeLocation.MyRoom,
				JudgeLocation.Result
			},
			Chara = CharaType.Chara01,
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_Favo01",
			ItemNum = 1,
			NonNumericalAchievementList = new List<int>(FavoMaster.FavoNumList(CharaType.Chara01)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FavoMaster.FavoMissionTable(CharaType.Chara01))
		},
		new Item
		{
			MissionID = MissionID.Character_01_Fan01,
			Category = Category.Character,
			Kind = Kind.Fan,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara01,
			OtherArgs = new List<object> { FanType.Seiryu },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara01",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int>(FanMaster.FanNumList(CharaType.Chara01)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FanMaster.FanMissionTable(CharaType.Chara01, FanType.Seiryu))
		},
		new Item
		{
			MissionID = MissionID.Character_01_Fan02,
			Category = Category.Character,
			Kind = Kind.Fan,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara01,
			OtherArgs = new List<object> { FanType.Suzaku },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara01",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int>(FanMaster.FanNumList(CharaType.Chara01)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FanMaster.FanMissionTable(CharaType.Chara01, FanType.Suzaku))
		},
		new Item
		{
			MissionID = MissionID.Character_01_Fan03,
			Category = Category.Character,
			Kind = Kind.Fan,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara01,
			OtherArgs = new List<object> { FanType.Byakko },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara01",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int>(FanMaster.FanNumList(CharaType.Chara01)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FanMaster.FanMissionTable(CharaType.Chara01, FanType.Byakko))
		},
		new Item
		{
			MissionID = MissionID.Character_01_Fan04,
			Category = Category.Character,
			Kind = Kind.Fan,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara01,
			OtherArgs = new List<object> { FanType.Genbu },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara01",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int>(FanMaster.FanNumList(CharaType.Chara01)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FanMaster.FanMissionTable(CharaType.Chara01, FanType.Genbu))
		},
		new Item
		{
			MissionID = MissionID.Character_01_Debut,
			Category = Category.Character,
			Kind = Kind.Debut,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.MyRoom },
			Chara = CharaType.Chara01,
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_FightSpirit_Chara01",
			ItemNum = 10,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Character_01_MusicGachaComp_1,
			Category = Category.Character,
			GameEventType = GameEventType.ReachSongGachaComp1,
			Kind = Kind.MusicGachaComp,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			OtherArgs = new List<object> { GachaSeriesName.MusicHiyori },
			Chara = CharaType.Chara01,
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_01AgariSR11",
			ItemNum = 1,
			FirstTerm = 1,
			CardConnectInfo = (CardConnectMissionID.CutinItem, new List<(CharaType, string)> { (CharaType.Chara01, "OID_01AgariSR11") })
		},
		new Item
		{
			MissionID = MissionID.Character_01_MusicGachaComp_2,
			Category = Category.Character,
			GameEventType = GameEventType.ReachSongGachaComp1,
			Kind = Kind.MusicGachaComp,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			OtherArgs = new List<object> { GachaSeriesName.MusicHiyori },
			Chara = CharaType.Chara01,
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_TableSticker256",
			ItemNum = 1,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Character_01_MusicGachaComp_3,
			Category = Category.Character,
			GameEventType = GameEventType.ReachSongGachaComp1,
			Kind = Kind.MusicGachaComp,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			OtherArgs = new List<object> { GachaSeriesName.MusicHiyori },
			Chara = CharaType.Chara01,
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_FightSpirit_Chara01",
			ItemNum = 50,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Character_02_Favo,
			Category = Category.Character,
			Kind = Kind.Favo,
			JudgeLocation = new List<JudgeLocation>
			{
				JudgeLocation.MyRoom,
				JudgeLocation.Result
			},
			Chara = CharaType.Chara02,
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_Favo02",
			ItemNum = 1,
			NonNumericalAchievementList = new List<int>(FavoMaster.FavoNumList(CharaType.Chara02)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FavoMaster.FavoMissionTable(CharaType.Chara02))
		},
		new Item
		{
			MissionID = MissionID.Character_02_Fan01,
			Category = Category.Character,
			Kind = Kind.Fan,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara02,
			OtherArgs = new List<object> { FanType.Seiryu },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara02",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int>(FanMaster.FanNumList(CharaType.Chara02)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FanMaster.FanMissionTable(CharaType.Chara02, FanType.Seiryu))
		},
		new Item
		{
			MissionID = MissionID.Character_02_Fan02,
			Category = Category.Character,
			Kind = Kind.Fan,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara02,
			OtherArgs = new List<object> { FanType.Suzaku },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara02",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int>(FanMaster.FanNumList(CharaType.Chara02)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FanMaster.FanMissionTable(CharaType.Chara02, FanType.Suzaku))
		},
		new Item
		{
			MissionID = MissionID.Character_02_Fan03,
			Category = Category.Character,
			Kind = Kind.Fan,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara02,
			OtherArgs = new List<object> { FanType.Byakko },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara02",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int>(FanMaster.FanNumList(CharaType.Chara02)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FanMaster.FanMissionTable(CharaType.Chara02, FanType.Byakko))
		},
		new Item
		{
			MissionID = MissionID.Character_02_Fan04,
			Category = Category.Character,
			Kind = Kind.Fan,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara02,
			OtherArgs = new List<object> { FanType.Genbu },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara02",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int>(FanMaster.FanNumList(CharaType.Chara02)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FanMaster.FanMissionTable(CharaType.Chara02, FanType.Genbu))
		},
		new Item
		{
			MissionID = MissionID.Character_02_Debut,
			Category = Category.Character,
			Kind = Kind.Debut,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.MyRoom },
			Chara = CharaType.Chara02,
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_FightSpirit_Chara02",
			ItemNum = 10,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Character_02_MusicGachaComp_1,
			Category = Category.Character,
			GameEventType = GameEventType.ReachSongGachaComp1,
			Kind = Kind.MusicGachaComp,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			OtherArgs = new List<object> { GachaSeriesName.MusicSen },
			Chara = CharaType.Chara02,
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_02AgariSR12",
			ItemNum = 1,
			FirstTerm = 1,
			CardConnectInfo = (CardConnectMissionID.CutinItem, new List<(CharaType, string)> { (CharaType.Chara02, "OID_02AgariSR12") })
		},
		new Item
		{
			MissionID = MissionID.Character_02_MusicGachaComp_2,
			Category = Category.Character,
			GameEventType = GameEventType.ReachSongGachaComp1,
			Kind = Kind.MusicGachaComp,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			OtherArgs = new List<object> { GachaSeriesName.MusicSen },
			Chara = CharaType.Chara02,
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_TableSticker257",
			ItemNum = 1,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Character_02_MusicGachaComp_3,
			Category = Category.Character,
			GameEventType = GameEventType.ReachSongGachaComp1,
			Kind = Kind.MusicGachaComp,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			OtherArgs = new List<object> { GachaSeriesName.MusicSen },
			Chara = CharaType.Chara02,
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_FightSpirit_Chara02",
			ItemNum = 50,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Character_03_Favo,
			Category = Category.Character,
			Kind = Kind.Favo,
			JudgeLocation = new List<JudgeLocation>
			{
				JudgeLocation.MyRoom,
				JudgeLocation.Result
			},
			Chara = CharaType.Chara03,
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_Favo03",
			ItemNum = 1,
			NonNumericalAchievementList = new List<int>(FavoMaster.FavoNumList(CharaType.Chara03)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FavoMaster.FavoMissionTable(CharaType.Chara03))
		},
		new Item
		{
			MissionID = MissionID.Character_03_Fan01,
			Category = Category.Character,
			Kind = Kind.Fan,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara03,
			OtherArgs = new List<object> { FanType.Seiryu },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara03",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int>(FanMaster.FanNumList(CharaType.Chara03)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FanMaster.FanMissionTable(CharaType.Chara03, FanType.Seiryu))
		},
		new Item
		{
			MissionID = MissionID.Character_03_Fan02,
			Category = Category.Character,
			Kind = Kind.Fan,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara03,
			OtherArgs = new List<object> { FanType.Suzaku },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara03",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int>(FanMaster.FanNumList(CharaType.Chara03)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FanMaster.FanMissionTable(CharaType.Chara03, FanType.Suzaku))
		},
		new Item
		{
			MissionID = MissionID.Character_03_Fan03,
			Category = Category.Character,
			Kind = Kind.Fan,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara03,
			OtherArgs = new List<object> { FanType.Byakko },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara03",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int>(FanMaster.FanNumList(CharaType.Chara03)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FanMaster.FanMissionTable(CharaType.Chara03, FanType.Byakko))
		},
		new Item
		{
			MissionID = MissionID.Character_03_Fan04,
			Category = Category.Character,
			Kind = Kind.Fan,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara03,
			OtherArgs = new List<object> { FanType.Genbu },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara03",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int>(FanMaster.FanNumList(CharaType.Chara03)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FanMaster.FanMissionTable(CharaType.Chara03, FanType.Genbu))
		},
		new Item
		{
			MissionID = MissionID.Character_03_Debut,
			Category = Category.Character,
			Kind = Kind.Debut,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.MyRoom },
			Chara = CharaType.Chara03,
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_FightSpirit_Chara03",
			ItemNum = 10,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Character_04_Favo,
			Category = Category.Character,
			Kind = Kind.Favo,
			JudgeLocation = new List<JudgeLocation>
			{
				JudgeLocation.MyRoom,
				JudgeLocation.Result
			},
			Chara = CharaType.Chara04,
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_Favo04",
			ItemNum = 1,
			NonNumericalAchievementList = new List<int>(FavoMaster.FavoNumList(CharaType.Chara04)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FavoMaster.FavoMissionTable(CharaType.Chara04))
		},
		new Item
		{
			MissionID = MissionID.Character_04_Fan01,
			Category = Category.Character,
			Kind = Kind.Fan,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara04,
			OtherArgs = new List<object> { FanType.Seiryu },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara04",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int>(FanMaster.FanNumList(CharaType.Chara04)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FanMaster.FanMissionTable(CharaType.Chara04, FanType.Seiryu))
		},
		new Item
		{
			MissionID = MissionID.Character_04_Fan02,
			Category = Category.Character,
			Kind = Kind.Fan,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara04,
			OtherArgs = new List<object> { FanType.Suzaku },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara04",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int>(FanMaster.FanNumList(CharaType.Chara04)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FanMaster.FanMissionTable(CharaType.Chara04, FanType.Suzaku))
		},
		new Item
		{
			MissionID = MissionID.Character_04_Fan03,
			Category = Category.Character,
			Kind = Kind.Fan,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara04,
			OtherArgs = new List<object> { FanType.Byakko },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara04",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int>(FanMaster.FanNumList(CharaType.Chara04)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FanMaster.FanMissionTable(CharaType.Chara04, FanType.Byakko))
		},
		new Item
		{
			MissionID = MissionID.Character_04_Fan04,
			Category = Category.Character,
			Kind = Kind.Fan,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara04,
			OtherArgs = new List<object> { FanType.Genbu },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara04",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int>(FanMaster.FanNumList(CharaType.Chara04)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FanMaster.FanMissionTable(CharaType.Chara04, FanType.Genbu))
		},
		new Item
		{
			MissionID = MissionID.Character_04_Debut,
			Category = Category.Character,
			Kind = Kind.Debut,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.MyRoom },
			Chara = CharaType.Chara04,
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_FightSpirit_Chara04",
			ItemNum = 10,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Character_04_MusicGachaComp_1,
			Category = Category.Character,
			GameEventType = GameEventType.ReachSongGachaComp3,
			Kind = Kind.MusicGachaComp,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			OtherArgs = new List<object> { GachaSeriesName.MusicTenshi },
			Chara = CharaType.Chara04,
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_04AgariSR13",
			ItemNum = 1,
			FirstTerm = 20,
			CardConnectInfo = (CardConnectMissionID.CutinItem, new List<(CharaType, string)> { (CharaType.Chara04, "OID_04AgariSR13") })
		},
		new Item
		{
			MissionID = MissionID.Character_04_MusicGachaComp_2,
			Category = Category.Character,
			GameEventType = GameEventType.ReachSongGachaComp3,
			Kind = Kind.MusicGachaComp,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			OtherArgs = new List<object> { GachaSeriesName.MusicTenshi },
			Chara = CharaType.Chara04,
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_TableSticker277",
			ItemNum = 1,
			FirstTerm = 30
		},
		new Item
		{
			MissionID = MissionID.Character_04_MusicGachaComp_3,
			Category = Category.Character,
			GameEventType = GameEventType.ReachSongGachaComp3,
			Kind = Kind.MusicGachaComp,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			OtherArgs = new List<object> { GachaSeriesName.MusicTenshi },
			Chara = CharaType.Chara04,
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_FightSpirit_Chara04",
			ItemNum = 50,
			FirstTerm = 40
		},
		new Item
		{
			MissionID = MissionID.Character_05_Favo,
			Category = Category.Character,
			Kind = Kind.Favo,
			JudgeLocation = new List<JudgeLocation>
			{
				JudgeLocation.MyRoom,
				JudgeLocation.Result
			},
			Chara = CharaType.Chara05,
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_Favo05",
			ItemNum = 1,
			NonNumericalAchievementList = new List<int>(FavoMaster.FavoNumList(CharaType.Chara05)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FavoMaster.FavoMissionTable(CharaType.Chara05))
		},
		new Item
		{
			MissionID = MissionID.Character_05_Fan01,
			Category = Category.Character,
			Kind = Kind.Fan,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara05,
			OtherArgs = new List<object> { FanType.Seiryu },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara05",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int>(FanMaster.FanNumList(CharaType.Chara05)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FanMaster.FanMissionTable(CharaType.Chara05, FanType.Seiryu))
		},
		new Item
		{
			MissionID = MissionID.Character_05_Fan02,
			Category = Category.Character,
			Kind = Kind.Fan,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara05,
			OtherArgs = new List<object> { FanType.Suzaku },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara05",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int>(FanMaster.FanNumList(CharaType.Chara05)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FanMaster.FanMissionTable(CharaType.Chara05, FanType.Suzaku))
		},
		new Item
		{
			MissionID = MissionID.Character_05_Fan03,
			Category = Category.Character,
			Kind = Kind.Fan,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara05,
			OtherArgs = new List<object> { FanType.Byakko },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara05",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int>(FanMaster.FanNumList(CharaType.Chara05)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FanMaster.FanMissionTable(CharaType.Chara05, FanType.Byakko))
		},
		new Item
		{
			MissionID = MissionID.Character_05_Fan04,
			Category = Category.Character,
			Kind = Kind.Fan,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara05,
			OtherArgs = new List<object> { FanType.Genbu },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara05",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int>(FanMaster.FanNumList(CharaType.Chara05)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FanMaster.FanMissionTable(CharaType.Chara05, FanType.Genbu))
		},
		new Item
		{
			MissionID = MissionID.Character_05_Debut,
			Category = Category.Character,
			Kind = Kind.Debut,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.MyRoom },
			Chara = CharaType.Chara05,
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_FightSpirit_Chara05",
			ItemNum = 10,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Character_05_MusicGachaComp_1,
			Category = Category.Character,
			GameEventType = GameEventType.ReachSongGachaComp2,
			Kind = Kind.MusicGachaComp,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			OtherArgs = new List<object> { GachaSeriesName.MusicYao },
			Chara = CharaType.Chara05,
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_05AgariSR14",
			ItemNum = 1,
			FirstTerm = 20,
			CardConnectInfo = (CardConnectMissionID.CutinItem, new List<(CharaType, string)> { (CharaType.Chara05, "OID_05AgariSR14") })
		},
		new Item
		{
			MissionID = MissionID.Character_05_MusicGachaComp_2,
			Category = Category.Character,
			GameEventType = GameEventType.ReachSongGachaComp2,
			Kind = Kind.MusicGachaComp,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			OtherArgs = new List<object> { GachaSeriesName.MusicYao },
			Chara = CharaType.Chara05,
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_TableSticker268",
			ItemNum = 1,
			FirstTerm = 30
		},
		new Item
		{
			MissionID = MissionID.Character_05_MusicGachaComp_3,
			Category = Category.Character,
			GameEventType = GameEventType.ReachSongGachaComp2,
			Kind = Kind.MusicGachaComp,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			OtherArgs = new List<object> { GachaSeriesName.MusicYao },
			Chara = CharaType.Chara05,
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_FightSpirit_Chara05",
			ItemNum = 50,
			FirstTerm = 40
		},
		new Item
		{
			MissionID = MissionID.Character_06_Favo,
			Category = Category.Character,
			Kind = Kind.Favo,
			JudgeLocation = new List<JudgeLocation>
			{
				JudgeLocation.MyRoom,
				JudgeLocation.Result
			},
			Chara = CharaType.Chara06,
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_Favo06",
			ItemNum = 1,
			NonNumericalAchievementList = new List<int>(FavoMaster.FavoNumList(CharaType.Chara06)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FavoMaster.FavoMissionTable(CharaType.Chara06))
		},
		new Item
		{
			MissionID = MissionID.Character_06_Fan01,
			Category = Category.Character,
			Kind = Kind.Fan,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara06,
			OtherArgs = new List<object> { FanType.Seiryu },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara06",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int>(FanMaster.FanNumList(CharaType.Chara06)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FanMaster.FanMissionTable(CharaType.Chara06, FanType.Seiryu))
		},
		new Item
		{
			MissionID = MissionID.Character_06_Fan02,
			Category = Category.Character,
			Kind = Kind.Fan,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara06,
			OtherArgs = new List<object> { FanType.Suzaku },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara06",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int>(FanMaster.FanNumList(CharaType.Chara06)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FanMaster.FanMissionTable(CharaType.Chara06, FanType.Suzaku))
		},
		new Item
		{
			MissionID = MissionID.Character_06_Fan03,
			Category = Category.Character,
			Kind = Kind.Fan,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara06,
			OtherArgs = new List<object> { FanType.Byakko },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara06",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int>(FanMaster.FanNumList(CharaType.Chara06)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FanMaster.FanMissionTable(CharaType.Chara06, FanType.Byakko))
		},
		new Item
		{
			MissionID = MissionID.Character_06_Fan04,
			Category = Category.Character,
			Kind = Kind.Fan,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara06,
			OtherArgs = new List<object> { FanType.Genbu },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara06",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int>(FanMaster.FanNumList(CharaType.Chara06)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FanMaster.FanMissionTable(CharaType.Chara06, FanType.Genbu))
		},
		new Item
		{
			MissionID = MissionID.Character_06_Debut,
			Category = Category.Character,
			Kind = Kind.Debut,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.MyRoom },
			Chara = CharaType.Chara06,
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_FightSpirit_Chara06",
			ItemNum = 10,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Character_06_MusicGachaComp_1,
			Category = Category.Character,
			GameEventType = GameEventType.ReachSongGachaComp5,
			Kind = Kind.MusicGachaComp,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			OtherArgs = new List<object> { GachaSeriesName.MusicMitsuba },
			Chara = CharaType.Chara06,
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_06AgariSR14",
			ItemNum = 1,
			FirstTerm = 20,
			CardConnectInfo = (CardConnectMissionID.CutinItem, new List<(CharaType, string)> { (CharaType.Chara06, "OID_06AgariSR14") })
		},
		new Item
		{
			MissionID = MissionID.Character_06_MusicGachaComp_2,
			Category = Category.Character,
			GameEventType = GameEventType.ReachSongGachaComp5,
			Kind = Kind.MusicGachaComp,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			OtherArgs = new List<object> { GachaSeriesName.MusicMitsuba },
			Chara = CharaType.Chara06,
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_TableSticker321",
			ItemNum = 1,
			FirstTerm = 30
		},
		new Item
		{
			MissionID = MissionID.Character_06_MusicGachaComp_3,
			Category = Category.Character,
			GameEventType = GameEventType.ReachSongGachaComp5,
			Kind = Kind.MusicGachaComp,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			OtherArgs = new List<object> { GachaSeriesName.MusicMitsuba },
			Chara = CharaType.Chara06,
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_FightSpirit_Chara06",
			ItemNum = 50,
			FirstTerm = 40
		},
		new Item
		{
			MissionID = MissionID.Character_07_Favo,
			Category = Category.Character,
			Kind = Kind.Favo,
			JudgeLocation = new List<JudgeLocation>
			{
				JudgeLocation.MyRoom,
				JudgeLocation.Result
			},
			Chara = CharaType.Chara07,
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_Favo07",
			ItemNum = 1,
			NonNumericalAchievementList = new List<int>(FavoMaster.FavoNumList(CharaType.Chara07)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FavoMaster.FavoMissionTable(CharaType.Chara07))
		},
		new Item
		{
			MissionID = MissionID.Character_07_Fan01,
			Category = Category.Character,
			Kind = Kind.Fan,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara07,
			OtherArgs = new List<object> { FanType.Seiryu },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara07",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int>(FanMaster.FanNumList(CharaType.Chara07)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FanMaster.FanMissionTable(CharaType.Chara07, FanType.Seiryu))
		},
		new Item
		{
			MissionID = MissionID.Character_07_Fan02,
			Category = Category.Character,
			Kind = Kind.Fan,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara07,
			OtherArgs = new List<object> { FanType.Suzaku },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara07",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int>(FanMaster.FanNumList(CharaType.Chara07)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FanMaster.FanMissionTable(CharaType.Chara07, FanType.Suzaku))
		},
		new Item
		{
			MissionID = MissionID.Character_07_Fan03,
			Category = Category.Character,
			Kind = Kind.Fan,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara07,
			OtherArgs = new List<object> { FanType.Byakko },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara07",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int>(FanMaster.FanNumList(CharaType.Chara07)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FanMaster.FanMissionTable(CharaType.Chara07, FanType.Byakko))
		},
		new Item
		{
			MissionID = MissionID.Character_07_Fan04,
			Category = Category.Character,
			Kind = Kind.Fan,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara07,
			OtherArgs = new List<object> { FanType.Genbu },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara07",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int>(FanMaster.FanNumList(CharaType.Chara07)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FanMaster.FanMissionTable(CharaType.Chara07, FanType.Genbu))
		},
		new Item
		{
			MissionID = MissionID.Character_07_Debut,
			Category = Category.Character,
			Kind = Kind.Debut,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.MyRoom },
			Chara = CharaType.Chara07,
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_FightSpirit_Chara07",
			ItemNum = 10,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Character_07_MusicGachaComp_1,
			Category = Category.Character,
			GameEventType = GameEventType.ReachSongGachaComp6,
			Kind = Kind.MusicGachaComp,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			OtherArgs = new List<object> { GachaSeriesName.MusicToytoy },
			Chara = CharaType.Chara07,
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_07AgariSR18",
			ItemNum = 1,
			FirstTerm = 20,
			CardConnectInfo = (CardConnectMissionID.CutinItem, new List<(CharaType, string)> { (CharaType.Chara07, "OID_07AgariSR18") })
		},
		new Item
		{
			MissionID = MissionID.Character_07_MusicGachaComp_2,
			Category = Category.Character,
			GameEventType = GameEventType.ReachSongGachaComp6,
			Kind = Kind.MusicGachaComp,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			OtherArgs = new List<object> { GachaSeriesName.MusicToytoy },
			Chara = CharaType.Chara07,
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_TableSticker347",
			ItemNum = 1,
			FirstTerm = 30
		},
		new Item
		{
			MissionID = MissionID.Character_07_MusicGachaComp_3,
			Category = Category.Character,
			GameEventType = GameEventType.ReachSongGachaComp6,
			Kind = Kind.MusicGachaComp,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			OtherArgs = new List<object> { GachaSeriesName.MusicToytoy },
			Chara = CharaType.Chara07,
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_FightSpirit_Chara07",
			ItemNum = 50,
			FirstTerm = 40
		},
		new Item
		{
			MissionID = MissionID.Character_08_Favo,
			Category = Category.Character,
			Kind = Kind.Favo,
			JudgeLocation = new List<JudgeLocation>
			{
				JudgeLocation.MyRoom,
				JudgeLocation.Result
			},
			Chara = CharaType.Chara08,
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_Favo08",
			ItemNum = 1,
			NonNumericalAchievementList = new List<int>(FavoMaster.FavoNumList(CharaType.Chara08)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FavoMaster.FavoMissionTable(CharaType.Chara08))
		},
		new Item
		{
			MissionID = MissionID.Character_08_Fan01,
			Category = Category.Character,
			Kind = Kind.Fan,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara08,
			OtherArgs = new List<object> { FanType.Seiryu },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara08",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int>(FanMaster.FanNumList(CharaType.Chara08)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FanMaster.FanMissionTable(CharaType.Chara08, FanType.Seiryu))
		},
		new Item
		{
			MissionID = MissionID.Character_08_Fan02,
			Category = Category.Character,
			Kind = Kind.Fan,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara08,
			OtherArgs = new List<object> { FanType.Suzaku },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara08",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int>(FanMaster.FanNumList(CharaType.Chara08)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FanMaster.FanMissionTable(CharaType.Chara08, FanType.Suzaku))
		},
		new Item
		{
			MissionID = MissionID.Character_08_Fan03,
			Category = Category.Character,
			Kind = Kind.Fan,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara08,
			OtherArgs = new List<object> { FanType.Byakko },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara08",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int>(FanMaster.FanNumList(CharaType.Chara08)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FanMaster.FanMissionTable(CharaType.Chara08, FanType.Byakko))
		},
		new Item
		{
			MissionID = MissionID.Character_08_Fan04,
			Category = Category.Character,
			Kind = Kind.Fan,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara08,
			OtherArgs = new List<object> { FanType.Genbu },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara08",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int>(FanMaster.FanNumList(CharaType.Chara08)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FanMaster.FanMissionTable(CharaType.Chara08, FanType.Genbu))
		},
		new Item
		{
			MissionID = MissionID.Character_08_Debut,
			Category = Category.Character,
			Kind = Kind.Debut,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.MyRoom },
			Chara = CharaType.Chara08,
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_FightSpirit_Chara08",
			ItemNum = 10,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Character_08_MusicGachaComp_1,
			Category = Category.Character,
			GameEventType = GameEventType.ReachSongGachaComp4,
			Kind = Kind.MusicGachaComp,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			OtherArgs = new List<object> { GachaSeriesName.MusicMusashi },
			Chara = CharaType.Chara08,
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_08AgariSR12",
			ItemNum = 1,
			FirstTerm = 20,
			CardConnectInfo = (CardConnectMissionID.CutinItem, new List<(CharaType, string)> { (CharaType.Chara08, "OID_08AgariSR12") })
		},
		new Item
		{
			MissionID = MissionID.Character_08_MusicGachaComp_2,
			Category = Category.Character,
			GameEventType = GameEventType.ReachSongGachaComp4,
			Kind = Kind.MusicGachaComp,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			OtherArgs = new List<object> { GachaSeriesName.MusicMusashi },
			Chara = CharaType.Chara08,
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_TableSticker294",
			ItemNum = 1,
			FirstTerm = 30
		},
		new Item
		{
			MissionID = MissionID.Character_08_MusicGachaComp_3,
			Category = Category.Character,
			GameEventType = GameEventType.ReachSongGachaComp4,
			Kind = Kind.MusicGachaComp,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			OtherArgs = new List<object> { GachaSeriesName.MusicMusashi },
			Chara = CharaType.Chara08,
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_FightSpirit_Chara08",
			ItemNum = 50,
			FirstTerm = 40
		},
		new Item
		{
			MissionID = MissionID.Character_09_Favo,
			Category = Category.Character,
			Kind = Kind.Favo,
			JudgeLocation = new List<JudgeLocation>
			{
				JudgeLocation.MyRoom,
				JudgeLocation.Result
			},
			Chara = CharaType.Chara09,
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_Favo09",
			ItemNum = 1,
			NonNumericalAchievementList = new List<int>(FavoMaster.FavoNumList(CharaType.Chara09)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FavoMaster.FavoMissionTable(CharaType.Chara09))
		},
		new Item
		{
			MissionID = MissionID.Character_09_Fan01,
			Category = Category.Character,
			Kind = Kind.Fan,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara09,
			OtherArgs = new List<object> { FanType.Seiryu },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara09",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int>(FanMaster.FanNumList(CharaType.Chara09)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FanMaster.FanMissionTable(CharaType.Chara09, FanType.Seiryu))
		},
		new Item
		{
			MissionID = MissionID.Character_09_Fan02,
			Category = Category.Character,
			Kind = Kind.Fan,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara09,
			OtherArgs = new List<object> { FanType.Suzaku },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara09",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int>(FanMaster.FanNumList(CharaType.Chara09)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FanMaster.FanMissionTable(CharaType.Chara09, FanType.Suzaku))
		},
		new Item
		{
			MissionID = MissionID.Character_09_Fan03,
			Category = Category.Character,
			Kind = Kind.Fan,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara09,
			OtherArgs = new List<object> { FanType.Byakko },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara09",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int>(FanMaster.FanNumList(CharaType.Chara09)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FanMaster.FanMissionTable(CharaType.Chara09, FanType.Byakko))
		},
		new Item
		{
			MissionID = MissionID.Character_09_Fan04,
			Category = Category.Character,
			Kind = Kind.Fan,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara09,
			OtherArgs = new List<object> { FanType.Genbu },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara09",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int>(FanMaster.FanNumList(CharaType.Chara09)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FanMaster.FanMissionTable(CharaType.Chara09, FanType.Genbu))
		},
		new Item
		{
			MissionID = MissionID.Character_09_Debut,
			Category = Category.Character,
			Kind = Kind.Debut,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.MyRoom },
			Chara = CharaType.Chara09,
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_FightSpirit_Chara09",
			ItemNum = 10,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Character_10_Favo,
			Category = Category.Character,
			Kind = Kind.Favo,
			JudgeLocation = new List<JudgeLocation>
			{
				JudgeLocation.MyRoom,
				JudgeLocation.Result
			},
			Chara = CharaType.Chara10,
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_Favo10",
			ItemNum = 1,
			NonNumericalAchievementList = new List<int>(FavoMaster.FavoNumList(CharaType.Chara10)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FavoMaster.FavoMissionTable(CharaType.Chara10))
		},
		new Item
		{
			MissionID = MissionID.Character_10_Fan01,
			Category = Category.Character,
			Kind = Kind.Fan,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara10,
			OtherArgs = new List<object> { FanType.Seiryu },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara10",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int>(FanMaster.FanNumList(CharaType.Chara10)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FanMaster.FanMissionTable(CharaType.Chara10, FanType.Seiryu))
		},
		new Item
		{
			MissionID = MissionID.Character_10_Fan02,
			Category = Category.Character,
			Kind = Kind.Fan,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara10,
			OtherArgs = new List<object> { FanType.Suzaku },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara10",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int>(FanMaster.FanNumList(CharaType.Chara10)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FanMaster.FanMissionTable(CharaType.Chara10, FanType.Suzaku))
		},
		new Item
		{
			MissionID = MissionID.Character_10_Fan03,
			Category = Category.Character,
			Kind = Kind.Fan,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara10,
			OtherArgs = new List<object> { FanType.Byakko },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara10",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int>(FanMaster.FanNumList(CharaType.Chara10)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FanMaster.FanMissionTable(CharaType.Chara10, FanType.Byakko))
		},
		new Item
		{
			MissionID = MissionID.Character_10_Fan04,
			Category = Category.Character,
			Kind = Kind.Fan,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara10,
			OtherArgs = new List<object> { FanType.Genbu },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara10",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int>(FanMaster.FanNumList(CharaType.Chara10)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FanMaster.FanMissionTable(CharaType.Chara10, FanType.Genbu))
		},
		new Item
		{
			MissionID = MissionID.Character_10_Debut,
			Category = Category.Character,
			Kind = Kind.Debut,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.MyRoom },
			Chara = CharaType.Chara10,
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_FightSpirit_Chara10",
			ItemNum = 10,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Character_11_Favo,
			Category = Category.Character,
			Kind = Kind.Favo,
			JudgeLocation = new List<JudgeLocation>
			{
				JudgeLocation.MyRoom,
				JudgeLocation.Result
			},
			Chara = CharaType.Chara11,
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_Favo11",
			ItemNum = 1,
			NonNumericalAchievementList = new List<int>(FavoMaster.FavoNumList(CharaType.Chara11)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FavoMaster.FavoMissionTable(CharaType.Chara11))
		},
		new Item
		{
			MissionID = MissionID.Character_11_Fan01,
			Category = Category.Character,
			Kind = Kind.Fan,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara11,
			OtherArgs = new List<object> { FanType.Seiryu },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara11",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int>(FanMaster.FanNumList(CharaType.Chara11)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FanMaster.FanMissionTable(CharaType.Chara11, FanType.Seiryu))
		},
		new Item
		{
			MissionID = MissionID.Character_11_Fan02,
			Category = Category.Character,
			Kind = Kind.Fan,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara11,
			OtherArgs = new List<object> { FanType.Suzaku },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara11",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int>(FanMaster.FanNumList(CharaType.Chara11)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FanMaster.FanMissionTable(CharaType.Chara11, FanType.Suzaku))
		},
		new Item
		{
			MissionID = MissionID.Character_11_Fan03,
			Category = Category.Character,
			Kind = Kind.Fan,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara11,
			OtherArgs = new List<object> { FanType.Byakko },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara11",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int>(FanMaster.FanNumList(CharaType.Chara11)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FanMaster.FanMissionTable(CharaType.Chara11, FanType.Byakko))
		},
		new Item
		{
			MissionID = MissionID.Character_11_Fan04,
			Category = Category.Character,
			Kind = Kind.Fan,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara11,
			OtherArgs = new List<object> { FanType.Genbu },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara11",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int>(FanMaster.FanNumList(CharaType.Chara11)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FanMaster.FanMissionTable(CharaType.Chara11, FanType.Genbu))
		},
		new Item
		{
			MissionID = MissionID.Character_11_Debut,
			Category = Category.Character,
			Kind = Kind.Debut,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.MyRoom },
			Chara = CharaType.Chara11,
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_FightSpirit_Chara11",
			ItemNum = 10,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Character_12_Favo,
			Category = Category.Character,
			Kind = Kind.Favo,
			JudgeLocation = new List<JudgeLocation>
			{
				JudgeLocation.MyRoom,
				JudgeLocation.Result
			},
			Chara = CharaType.Chara12,
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_Favo12",
			ItemNum = 1,
			NonNumericalAchievementList = new List<int>(FavoMaster.FavoNumList(CharaType.Chara12)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FavoMaster.FavoMissionTable(CharaType.Chara12))
		},
		new Item
		{
			MissionID = MissionID.Character_12_Fan01,
			Category = Category.Character,
			Kind = Kind.Fan,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara12,
			OtherArgs = new List<object> { FanType.Seiryu },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara12",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int>(FanMaster.FanNumList(CharaType.Chara12)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FanMaster.FanMissionTable(CharaType.Chara12, FanType.Seiryu))
		},
		new Item
		{
			MissionID = MissionID.Character_12_Fan02,
			Category = Category.Character,
			Kind = Kind.Fan,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara12,
			OtherArgs = new List<object> { FanType.Suzaku },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara12",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int>(FanMaster.FanNumList(CharaType.Chara12)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FanMaster.FanMissionTable(CharaType.Chara12, FanType.Suzaku))
		},
		new Item
		{
			MissionID = MissionID.Character_12_Fan03,
			Category = Category.Character,
			Kind = Kind.Fan,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara12,
			OtherArgs = new List<object> { FanType.Byakko },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara12",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int>(FanMaster.FanNumList(CharaType.Chara12)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FanMaster.FanMissionTable(CharaType.Chara12, FanType.Byakko))
		},
		new Item
		{
			MissionID = MissionID.Character_12_Fan04,
			Category = Category.Character,
			Kind = Kind.Fan,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara12,
			OtherArgs = new List<object> { FanType.Genbu },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara12",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int>(FanMaster.FanNumList(CharaType.Chara12)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FanMaster.FanMissionTable(CharaType.Chara12, FanType.Genbu))
		},
		new Item
		{
			MissionID = MissionID.Character_12_Debut,
			Category = Category.Character,
			Kind = Kind.Debut,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.MyRoom },
			Chara = CharaType.Chara12,
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_FightSpirit_Chara12",
			ItemNum = 10,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Character_13_Favo,
			Category = Category.Character,
			Kind = Kind.Favo,
			JudgeLocation = new List<JudgeLocation>
			{
				JudgeLocation.MyRoom,
				JudgeLocation.Result
			},
			Chara = CharaType.Chara13,
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_Favo13",
			ItemNum = 1,
			NonNumericalAchievementList = new List<int>(FavoMaster.FavoNumList(CharaType.Chara13)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FavoMaster.FavoMissionTable(CharaType.Chara13))
		},
		new Item
		{
			MissionID = MissionID.Character_13_Fan01,
			Category = Category.Character,
			Kind = Kind.Fan,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara13,
			OtherArgs = new List<object> { FanType.Seiryu },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara13",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int>(FanMaster.FanNumList(CharaType.Chara13)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FanMaster.FanMissionTable(CharaType.Chara13, FanType.Seiryu))
		},
		new Item
		{
			MissionID = MissionID.Character_13_Fan02,
			Category = Category.Character,
			Kind = Kind.Fan,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara13,
			OtherArgs = new List<object> { FanType.Suzaku },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara13",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int>(FanMaster.FanNumList(CharaType.Chara13)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FanMaster.FanMissionTable(CharaType.Chara13, FanType.Suzaku))
		},
		new Item
		{
			MissionID = MissionID.Character_13_Fan03,
			Category = Category.Character,
			Kind = Kind.Fan,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara13,
			OtherArgs = new List<object> { FanType.Byakko },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara13",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int>(FanMaster.FanNumList(CharaType.Chara13)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FanMaster.FanMissionTable(CharaType.Chara13, FanType.Byakko))
		},
		new Item
		{
			MissionID = MissionID.Character_13_Fan04,
			Category = Category.Character,
			Kind = Kind.Fan,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara13,
			OtherArgs = new List<object> { FanType.Genbu },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara13",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int>(FanMaster.FanNumList(CharaType.Chara13)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FanMaster.FanMissionTable(CharaType.Chara13, FanType.Genbu))
		},
		new Item
		{
			MissionID = MissionID.Character_13_Debut,
			Category = Category.Character,
			Kind = Kind.Debut,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.MyRoom },
			Chara = CharaType.Chara13,
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_FightSpirit_Chara13",
			ItemNum = 10,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Character_14_Favo,
			Category = Category.Character,
			Kind = Kind.Favo,
			JudgeLocation = new List<JudgeLocation>
			{
				JudgeLocation.MyRoom,
				JudgeLocation.Result
			},
			Chara = CharaType.Chara14,
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_Favo14",
			ItemNum = 1,
			NonNumericalAchievementList = new List<int>(FavoMaster.FavoNumList(CharaType.Chara14)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FavoMaster.FavoMissionTable(CharaType.Chara14))
		},
		new Item
		{
			MissionID = MissionID.Character_14_Fan01,
			Category = Category.Character,
			Kind = Kind.Fan,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara14,
			OtherArgs = new List<object> { FanType.Seiryu },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara14",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int>(FanMaster.FanNumList(CharaType.Chara14)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FanMaster.FanMissionTable(CharaType.Chara14, FanType.Seiryu))
		},
		new Item
		{
			MissionID = MissionID.Character_14_Fan02,
			Category = Category.Character,
			Kind = Kind.Fan,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara14,
			OtherArgs = new List<object> { FanType.Suzaku },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara14",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int>(FanMaster.FanNumList(CharaType.Chara14)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FanMaster.FanMissionTable(CharaType.Chara14, FanType.Suzaku))
		},
		new Item
		{
			MissionID = MissionID.Character_14_Fan03,
			Category = Category.Character,
			Kind = Kind.Fan,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara14,
			OtherArgs = new List<object> { FanType.Byakko },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara14",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int>(FanMaster.FanNumList(CharaType.Chara14)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FanMaster.FanMissionTable(CharaType.Chara14, FanType.Byakko))
		},
		new Item
		{
			MissionID = MissionID.Character_14_Fan04,
			Category = Category.Character,
			Kind = Kind.Fan,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara14,
			OtherArgs = new List<object> { FanType.Genbu },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara14",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int>(FanMaster.FanNumList(CharaType.Chara14)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FanMaster.FanMissionTable(CharaType.Chara14, FanType.Genbu))
		},
		new Item
		{
			MissionID = MissionID.Character_14_Debut,
			Category = Category.Character,
			Kind = Kind.Debut,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.MyRoom },
			Chara = CharaType.Chara14,
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_FightSpirit_Chara14",
			ItemNum = 10,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Character_15_Favo,
			Category = Category.Character,
			Kind = Kind.Favo,
			JudgeLocation = new List<JudgeLocation>
			{
				JudgeLocation.MyRoom,
				JudgeLocation.Result
			},
			Chara = CharaType.Chara15,
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_Favo15",
			ItemNum = 1,
			NonNumericalAchievementList = new List<int>(FavoMaster.FavoNumList(CharaType.Chara15)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FavoMaster.FavoMissionTable(CharaType.Chara15))
		},
		new Item
		{
			MissionID = MissionID.Character_15_Fan01,
			Category = Category.Character,
			Kind = Kind.Fan,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara15,
			OtherArgs = new List<object> { FanType.Seiryu },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara15",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int>(FanMaster.FanNumList(CharaType.Chara15)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FanMaster.FanMissionTable(CharaType.Chara15, FanType.Seiryu))
		},
		new Item
		{
			MissionID = MissionID.Character_15_Fan02,
			Category = Category.Character,
			Kind = Kind.Fan,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara15,
			OtherArgs = new List<object> { FanType.Suzaku },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara15",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int>(FanMaster.FanNumList(CharaType.Chara15)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FanMaster.FanMissionTable(CharaType.Chara15, FanType.Suzaku))
		},
		new Item
		{
			MissionID = MissionID.Character_15_Fan03,
			Category = Category.Character,
			Kind = Kind.Fan,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara15,
			OtherArgs = new List<object> { FanType.Byakko },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara15",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int>(FanMaster.FanNumList(CharaType.Chara15)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FanMaster.FanMissionTable(CharaType.Chara15, FanType.Byakko))
		},
		new Item
		{
			MissionID = MissionID.Character_15_Fan04,
			Category = Category.Character,
			Kind = Kind.Fan,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara15,
			OtherArgs = new List<object> { FanType.Genbu },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara15",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int>(FanMaster.FanNumList(CharaType.Chara15)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FanMaster.FanMissionTable(CharaType.Chara15, FanType.Genbu))
		},
		new Item
		{
			MissionID = MissionID.Character_15_Debut,
			Category = Category.Character,
			Kind = Kind.Debut,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.MyRoom },
			Chara = CharaType.Chara15,
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_FightSpirit_Chara15",
			ItemNum = 10,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Character_16_Favo,
			Category = Category.Character,
			Kind = Kind.Favo,
			JudgeLocation = new List<JudgeLocation>
			{
				JudgeLocation.MyRoom,
				JudgeLocation.Result
			},
			Chara = CharaType.Chara16,
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_Favo16",
			ItemNum = 1,
			NonNumericalAchievementList = new List<int>(FavoMaster.FavoNumList(CharaType.Chara16)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FavoMaster.FavoMissionTable(CharaType.Chara16))
		},
		new Item
		{
			MissionID = MissionID.Character_16_Fan01,
			Category = Category.Character,
			Kind = Kind.Fan,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara16,
			OtherArgs = new List<object> { FanType.Seiryu },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara16",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int>(FanMaster.FanNumList(CharaType.Chara16)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FanMaster.FanMissionTable(CharaType.Chara16, FanType.Seiryu))
		},
		new Item
		{
			MissionID = MissionID.Character_16_Fan02,
			Category = Category.Character,
			Kind = Kind.Fan,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara16,
			OtherArgs = new List<object> { FanType.Suzaku },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara16",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int>(FanMaster.FanNumList(CharaType.Chara16)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FanMaster.FanMissionTable(CharaType.Chara16, FanType.Suzaku))
		},
		new Item
		{
			MissionID = MissionID.Character_16_Fan03,
			Category = Category.Character,
			Kind = Kind.Fan,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara16,
			OtherArgs = new List<object> { FanType.Byakko },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara16",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int>(FanMaster.FanNumList(CharaType.Chara16)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FanMaster.FanMissionTable(CharaType.Chara16, FanType.Byakko))
		},
		new Item
		{
			MissionID = MissionID.Character_16_Fan04,
			Category = Category.Character,
			Kind = Kind.Fan,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara16,
			OtherArgs = new List<object> { FanType.Genbu },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara16",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int>(FanMaster.FanNumList(CharaType.Chara16)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FanMaster.FanMissionTable(CharaType.Chara16, FanType.Genbu))
		},
		new Item
		{
			MissionID = MissionID.Character_16_Debut,
			Category = Category.Character,
			Kind = Kind.Debut,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.MyRoom },
			Chara = CharaType.Chara16,
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_FightSpirit_Chara16",
			ItemNum = 10,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Character_17_Favo,
			Category = Category.Character,
			Kind = Kind.Favo,
			JudgeLocation = new List<JudgeLocation>
			{
				JudgeLocation.MyRoom,
				JudgeLocation.Result
			},
			Chara = CharaType.Chara17,
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_Favo17",
			ItemNum = 1,
			NonNumericalAchievementList = new List<int>(FavoMaster.FavoNumList(CharaType.Chara17)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FavoMaster.FavoMissionTable(CharaType.Chara17))
		},
		new Item
		{
			MissionID = MissionID.Character_17_Fan01,
			Category = Category.Character,
			Kind = Kind.Fan,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara17,
			OtherArgs = new List<object> { FanType.Seiryu },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara17",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int>(FanMaster.FanNumList(CharaType.Chara17)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FanMaster.FanMissionTable(CharaType.Chara17, FanType.Seiryu))
		},
		new Item
		{
			MissionID = MissionID.Character_17_Fan02,
			Category = Category.Character,
			Kind = Kind.Fan,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara17,
			OtherArgs = new List<object> { FanType.Suzaku },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara17",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int>(FanMaster.FanNumList(CharaType.Chara17)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FanMaster.FanMissionTable(CharaType.Chara17, FanType.Suzaku))
		},
		new Item
		{
			MissionID = MissionID.Character_17_Fan03,
			Category = Category.Character,
			Kind = Kind.Fan,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara17,
			OtherArgs = new List<object> { FanType.Byakko },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara17",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int>(FanMaster.FanNumList(CharaType.Chara17)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FanMaster.FanMissionTable(CharaType.Chara17, FanType.Byakko))
		},
		new Item
		{
			MissionID = MissionID.Character_17_Fan04,
			Category = Category.Character,
			Kind = Kind.Fan,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara17,
			OtherArgs = new List<object> { FanType.Genbu },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara17",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int>(FanMaster.FanNumList(CharaType.Chara17)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FanMaster.FanMissionTable(CharaType.Chara17, FanType.Genbu))
		},
		new Item
		{
			MissionID = MissionID.Character_17_Debut,
			Category = Category.Character,
			Kind = Kind.Debut,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.MyRoom },
			Chara = CharaType.Chara17,
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_FightSpirit_Chara17",
			ItemNum = 10,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Character_18_Favo,
			Category = Category.Character,
			Kind = Kind.Favo,
			JudgeLocation = new List<JudgeLocation>
			{
				JudgeLocation.MyRoom,
				JudgeLocation.Result
			},
			Chara = CharaType.Chara18,
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_Favo18",
			ItemNum = 1,
			NonNumericalAchievementList = new List<int>(FavoMaster.FavoNumList(CharaType.Chara18)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FavoMaster.FavoMissionTable(CharaType.Chara18))
		},
		new Item
		{
			MissionID = MissionID.Character_18_Fan01,
			Category = Category.Character,
			Kind = Kind.Fan,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara18,
			OtherArgs = new List<object> { FanType.Seiryu },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara18",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int>(FanMaster.FanNumList(CharaType.Chara18)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FanMaster.FanMissionTable(CharaType.Chara18, FanType.Seiryu))
		},
		new Item
		{
			MissionID = MissionID.Character_18_Fan02,
			Category = Category.Character,
			Kind = Kind.Fan,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara18,
			OtherArgs = new List<object> { FanType.Suzaku },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara18",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int>(FanMaster.FanNumList(CharaType.Chara18)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FanMaster.FanMissionTable(CharaType.Chara18, FanType.Suzaku))
		},
		new Item
		{
			MissionID = MissionID.Character_18_Fan03,
			Category = Category.Character,
			Kind = Kind.Fan,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara18,
			OtherArgs = new List<object> { FanType.Byakko },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara18",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int>(FanMaster.FanNumList(CharaType.Chara18)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FanMaster.FanMissionTable(CharaType.Chara18, FanType.Byakko))
		},
		new Item
		{
			MissionID = MissionID.Character_18_Fan04,
			Category = Category.Character,
			Kind = Kind.Fan,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara18,
			OtherArgs = new List<object> { FanType.Genbu },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara18",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int>(FanMaster.FanNumList(CharaType.Chara18)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FanMaster.FanMissionTable(CharaType.Chara18, FanType.Genbu))
		},
		new Item
		{
			MissionID = MissionID.Character_18_Debut,
			Category = Category.Character,
			Kind = Kind.Debut,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.MyRoom },
			Chara = CharaType.Chara18,
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_FightSpirit_Chara18",
			ItemNum = 10,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Character_19_Favo,
			Category = Category.Character,
			Kind = Kind.Favo,
			JudgeLocation = new List<JudgeLocation>
			{
				JudgeLocation.MyRoom,
				JudgeLocation.Result
			},
			Chara = CharaType.Chara19,
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_Favo19",
			ItemNum = 1,
			NonNumericalAchievementList = new List<int>(FavoMaster.FavoNumList(CharaType.Chara19)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FavoMaster.FavoMissionTable(CharaType.Chara19))
		},
		new Item
		{
			MissionID = MissionID.Character_19_Fan01,
			Category = Category.Character,
			Kind = Kind.Fan,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara19,
			OtherArgs = new List<object> { FanType.Seiryu },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara19",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int>(FanMaster.FanNumList(CharaType.Chara19)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FanMaster.FanMissionTable(CharaType.Chara19, FanType.Seiryu))
		},
		new Item
		{
			MissionID = MissionID.Character_19_Fan02,
			Category = Category.Character,
			Kind = Kind.Fan,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara19,
			OtherArgs = new List<object> { FanType.Suzaku },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara19",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int>(FanMaster.FanNumList(CharaType.Chara19)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FanMaster.FanMissionTable(CharaType.Chara19, FanType.Suzaku))
		},
		new Item
		{
			MissionID = MissionID.Character_19_Fan03,
			Category = Category.Character,
			Kind = Kind.Fan,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara19,
			OtherArgs = new List<object> { FanType.Byakko },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara19",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int>(FanMaster.FanNumList(CharaType.Chara19)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FanMaster.FanMissionTable(CharaType.Chara19, FanType.Byakko))
		},
		new Item
		{
			MissionID = MissionID.Character_19_Fan04,
			Category = Category.Character,
			Kind = Kind.Fan,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara19,
			OtherArgs = new List<object> { FanType.Genbu },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara19",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int>(FanMaster.FanNumList(CharaType.Chara19)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FanMaster.FanMissionTable(CharaType.Chara19, FanType.Genbu))
		},
		new Item
		{
			MissionID = MissionID.Character_19_Debut,
			Category = Category.Character,
			Kind = Kind.Debut,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.MyRoom },
			Chara = CharaType.Chara19,
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_FightSpirit_Chara19",
			ItemNum = 10,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Character_20_Favo,
			Category = Category.Character,
			Kind = Kind.Favo,
			JudgeLocation = new List<JudgeLocation>
			{
				JudgeLocation.MyRoom,
				JudgeLocation.Result
			},
			Chara = CharaType.Chara20,
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_Favo20",
			ItemNum = 1,
			NonNumericalAchievementList = new List<int>(FavoMaster.FavoNumList(CharaType.Chara20)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FavoMaster.FavoMissionTable(CharaType.Chara20))
		},
		new Item
		{
			MissionID = MissionID.Character_20_Fan01,
			Category = Category.Character,
			Kind = Kind.Fan,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara20,
			OtherArgs = new List<object> { FanType.Seiryu },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara20",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int>(FanMaster.FanNumList(CharaType.Chara20)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FanMaster.FanMissionTable(CharaType.Chara20, FanType.Seiryu))
		},
		new Item
		{
			MissionID = MissionID.Character_20_Fan02,
			Category = Category.Character,
			Kind = Kind.Fan,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara20,
			OtherArgs = new List<object> { FanType.Suzaku },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara20",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int>(FanMaster.FanNumList(CharaType.Chara20)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FanMaster.FanMissionTable(CharaType.Chara20, FanType.Suzaku))
		},
		new Item
		{
			MissionID = MissionID.Character_20_Fan03,
			Category = Category.Character,
			Kind = Kind.Fan,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara20,
			OtherArgs = new List<object> { FanType.Byakko },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara20",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int>(FanMaster.FanNumList(CharaType.Chara20)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FanMaster.FanMissionTable(CharaType.Chara20, FanType.Byakko))
		},
		new Item
		{
			MissionID = MissionID.Character_20_Fan04,
			Category = Category.Character,
			Kind = Kind.Fan,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara20,
			OtherArgs = new List<object> { FanType.Genbu },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara20",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int>(FanMaster.FanNumList(CharaType.Chara20)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FanMaster.FanMissionTable(CharaType.Chara20, FanType.Genbu))
		},
		new Item
		{
			MissionID = MissionID.Character_20_Debut,
			Category = Category.Character,
			Kind = Kind.Debut,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.MyRoom },
			Chara = CharaType.Chara20,
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_FightSpirit_Chara20",
			ItemNum = 10,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Character_21_Favo,
			Category = Category.Character,
			Kind = Kind.Favo,
			JudgeLocation = new List<JudgeLocation>
			{
				JudgeLocation.MyRoom,
				JudgeLocation.Result
			},
			Chara = CharaType.Chara21,
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_Favo21",
			ItemNum = 1,
			NonNumericalAchievementList = new List<int>(FavoMaster.FavoNumList(CharaType.Chara21)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FavoMaster.FavoMissionTable(CharaType.Chara21))
		},
		new Item
		{
			MissionID = MissionID.Character_21_Fan01,
			Category = Category.Character,
			Kind = Kind.Fan,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara21,
			OtherArgs = new List<object> { FanType.Seiryu },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara21",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int>(FanMaster.FanNumList(CharaType.Chara21)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FanMaster.FanMissionTable(CharaType.Chara21, FanType.Seiryu))
		},
		new Item
		{
			MissionID = MissionID.Character_21_Fan02,
			Category = Category.Character,
			Kind = Kind.Fan,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara21,
			OtherArgs = new List<object> { FanType.Suzaku },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara21",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int>(FanMaster.FanNumList(CharaType.Chara21)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FanMaster.FanMissionTable(CharaType.Chara21, FanType.Suzaku))
		},
		new Item
		{
			MissionID = MissionID.Character_21_Fan03,
			Category = Category.Character,
			Kind = Kind.Fan,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara21,
			OtherArgs = new List<object> { FanType.Byakko },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara21",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int>(FanMaster.FanNumList(CharaType.Chara21)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FanMaster.FanMissionTable(CharaType.Chara21, FanType.Byakko))
		},
		new Item
		{
			MissionID = MissionID.Character_21_Fan04,
			Category = Category.Character,
			Kind = Kind.Fan,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara21,
			OtherArgs = new List<object> { FanType.Genbu },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara21",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int>(FanMaster.FanNumList(CharaType.Chara21)),
			ExtraRewardList = new Dictionary<int, (string, int)>(FanMaster.FanMissionTable(CharaType.Chara21, FanType.Genbu))
		},
		new Item
		{
			MissionID = MissionID.Character_21_Debut,
			Category = Category.Character,
			Kind = Kind.Debut,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.MyRoom },
			Chara = CharaType.Chara21,
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_FightSpirit_Chara21",
			ItemNum = 10,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Weekly_Login,
			Category = Category.Weekly,
			UpdateInterval = UpdateInterval.Weekly,
			Kind = Kind.Login,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_FightSpirit_Wildcard",
			ItemNum = 1,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Weekly_Vs,
			Category = Category.Weekly,
			UpdateInterval = UpdateInterval.Weekly,
			Kind = Kind.TaikyokuCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.All,
			ItemOID = "OID_FightSpirit_Wildcard",
			ItemNum = 1,
			NonNumericalAchievementList = new List<int> { 2, 5, 10 }
		},
		new Item
		{
			MissionID = MissionID.Weekly_FireReach_Vs,
			Category = Category.Weekly,
			GameEventType = GameEventType.ConstancyFireReachAppearance,
			UpdateInterval = UpdateInterval.Weekly,
			Kind = Kind.EventTaikyoku,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			EventType = EventType.FireReach,
			TakuTypes = new List<TakuType> { TakuType.Tonpu },
			IsConstancyEventTaku = true,
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Wildcard",
			ItemNum = 1,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Weekly_FireReach_Top,
			Category = Category.Weekly,
			GameEventType = GameEventType.ConstancyFireReachAppearance,
			UpdateInterval = UpdateInterval.Weekly,
			Kind = Kind.EventTop,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			EventType = EventType.FireReach,
			TakuTypes = new List<TakuType> { TakuType.Tonpu },
			IsConstancyEventTaku = true,
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Wildcard",
			ItemNum = 1,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Weekly_AccelDora_Vs,
			Category = Category.Weekly,
			GameEventType = GameEventType.ConstancyAccelDoraSchedule,
			UpdateInterval = UpdateInterval.Weekly,
			Kind = Kind.EventTaikyoku,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			EventType = EventType.AccelDora,
			TakuTypes = new List<TakuType> { TakuType.Tonpu },
			IsConstancyEventTaku = true,
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Wildcard",
			ItemNum = 1,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Weekly_AccelDora_Top,
			Category = Category.Weekly,
			GameEventType = GameEventType.ConstancyAccelDoraSchedule,
			UpdateInterval = UpdateInterval.Weekly,
			Kind = Kind.EventTop,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			EventType = EventType.AccelDora,
			TakuTypes = new List<TakuType> { TakuType.Tonpu },
			IsConstancyEventTaku = true,
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Wildcard",
			ItemNum = 1,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Weekly_Mentanpin_Vs,
			Category = Category.Weekly,
			GameEventType = GameEventType.ConstancyMentanpinSchedule,
			UpdateInterval = UpdateInterval.Weekly,
			Kind = Kind.EventTaikyoku,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			EventType = EventType.Mentanpin,
			TakuTypes = new List<TakuType> { TakuType.Tonpu },
			IsConstancyEventTaku = true,
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Wildcard",
			ItemNum = 1,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Weekly_Mentanpin_Top,
			Category = Category.Weekly,
			GameEventType = GameEventType.ConstancyMentanpinSchedule,
			UpdateInterval = UpdateInterval.Weekly,
			Kind = Kind.EventTop,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			EventType = EventType.Mentanpin,
			TakuTypes = new List<TakuType> { TakuType.Tonpu },
			IsConstancyEventTaku = true,
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Wildcard",
			ItemNum = 1,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Weekly_Chara_01_Top,
			Category = Category.Weekly,
			UpdateInterval = UpdateInterval.Weekly,
			Kind = Kind.OverScore,
			IsNimaEnable = false,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara01,
			OtherArgs = new List<object> { 10000, 15000, 0 },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara01",
			ItemNum = 1,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Weekly_Chara_02_Top,
			Category = Category.Weekly,
			UpdateInterval = UpdateInterval.Weekly,
			Kind = Kind.OverScore,
			IsNimaEnable = false,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara02,
			OtherArgs = new List<object> { 10000, 15000, 0 },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara02",
			ItemNum = 1,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Weekly_Chara_03_Top,
			Category = Category.Weekly,
			UpdateInterval = UpdateInterval.Weekly,
			Kind = Kind.OverScore,
			IsNimaEnable = false,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara03,
			OtherArgs = new List<object> { 10000, 15000, 0 },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara03",
			ItemNum = 1,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Weekly_Chara_04_Top,
			Category = Category.Weekly,
			UpdateInterval = UpdateInterval.Weekly,
			Kind = Kind.OverScore,
			IsNimaEnable = false,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara04,
			OtherArgs = new List<object> { 10000, 15000, 0 },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara04",
			ItemNum = 1,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Weekly_Chara_05_Top,
			Category = Category.Weekly,
			UpdateInterval = UpdateInterval.Weekly,
			Kind = Kind.OverScore,
			IsNimaEnable = false,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara05,
			OtherArgs = new List<object> { 10000, 15000, 0 },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara05",
			ItemNum = 1,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Weekly_Chara_06_Top,
			Category = Category.Weekly,
			UpdateInterval = UpdateInterval.Weekly,
			Kind = Kind.OverScore,
			IsNimaEnable = false,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara06,
			OtherArgs = new List<object> { 10000, 15000, 0 },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara06",
			ItemNum = 1,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Weekly_Chara_07_Top,
			Category = Category.Weekly,
			UpdateInterval = UpdateInterval.Weekly,
			Kind = Kind.OverScore,
			IsNimaEnable = false,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara07,
			OtherArgs = new List<object> { 10000, 15000, 0 },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara07",
			ItemNum = 1,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Weekly_Chara_08_Top,
			Category = Category.Weekly,
			UpdateInterval = UpdateInterval.Weekly,
			Kind = Kind.OverScore,
			IsNimaEnable = false,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara08,
			OtherArgs = new List<object> { 10000, 15000, 0 },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara08",
			ItemNum = 1,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Weekly_Chara_09_Top,
			Category = Category.Weekly,
			UpdateInterval = UpdateInterval.Weekly,
			Kind = Kind.OverScore,
			IsNimaEnable = false,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara09,
			OtherArgs = new List<object> { 10000, 15000, 0 },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara09",
			ItemNum = 1,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Weekly_Chara_10_Top,
			Category = Category.Weekly,
			UpdateInterval = UpdateInterval.Weekly,
			Kind = Kind.OverScore,
			IsNimaEnable = false,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara10,
			OtherArgs = new List<object> { 10000, 15000, 0 },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara10",
			ItemNum = 1,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Weekly_Chara_11_Top,
			Category = Category.Weekly,
			UpdateInterval = UpdateInterval.Weekly,
			Kind = Kind.OverScore,
			IsNimaEnable = false,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara11,
			OtherArgs = new List<object> { 10000, 15000, 0 },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara11",
			ItemNum = 1,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Weekly_Chara_12_Top,
			Category = Category.Weekly,
			UpdateInterval = UpdateInterval.Weekly,
			Kind = Kind.OverScore,
			IsNimaEnable = false,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara12,
			OtherArgs = new List<object> { 10000, 15000, 0 },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara12",
			ItemNum = 1,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Weekly_Chara_13_Top,
			Category = Category.Weekly,
			UpdateInterval = UpdateInterval.Weekly,
			Kind = Kind.OverScore,
			IsNimaEnable = false,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara13,
			OtherArgs = new List<object> { 10000, 15000, 0 },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara13",
			ItemNum = 1,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Weekly_Chara_14_Top,
			Category = Category.Weekly,
			UpdateInterval = UpdateInterval.Weekly,
			Kind = Kind.OverScore,
			IsNimaEnable = false,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara14,
			OtherArgs = new List<object> { 10000, 15000, 0 },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara14",
			ItemNum = 1,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Weekly_Chara_15_Top,
			Category = Category.Weekly,
			UpdateInterval = UpdateInterval.Weekly,
			Kind = Kind.OverScore,
			IsNimaEnable = false,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara15,
			OtherArgs = new List<object> { 10000, 15000, 0 },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara15",
			ItemNum = 1,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Weekly_Chara_16_Top,
			Category = Category.Weekly,
			UpdateInterval = UpdateInterval.Weekly,
			Kind = Kind.OverScore,
			IsNimaEnable = false,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara16,
			OtherArgs = new List<object> { 10000, 15000, 0 },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara16",
			ItemNum = 1,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Weekly_Chara_17_Top,
			Category = Category.Weekly,
			UpdateInterval = UpdateInterval.Weekly,
			Kind = Kind.OverScore,
			IsNimaEnable = false,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara17,
			OtherArgs = new List<object> { 10000, 15000, 0 },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara17",
			ItemNum = 1,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Weekly_Chara_18_Top,
			Category = Category.Weekly,
			UpdateInterval = UpdateInterval.Weekly,
			Kind = Kind.OverScore,
			IsNimaEnable = false,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara18,
			OtherArgs = new List<object> { 10000, 15000, 0 },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara18",
			ItemNum = 1,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Weekly_Chara_19_Top,
			Category = Category.Weekly,
			UpdateInterval = UpdateInterval.Weekly,
			Kind = Kind.OverScore,
			IsNimaEnable = false,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara19,
			OtherArgs = new List<object> { 10000, 15000, 0 },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara19",
			ItemNum = 1,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Weekly_Chara_20_Top,
			Category = Category.Weekly,
			UpdateInterval = UpdateInterval.Weekly,
			Kind = Kind.OverScore,
			IsNimaEnable = false,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara20,
			OtherArgs = new List<object> { 10000, 15000, 0 },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara20",
			ItemNum = 1,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Weekly_Chara_21_Top,
			Category = Category.Weekly,
			UpdateInterval = UpdateInterval.Weekly,
			Kind = Kind.OverScore,
			IsNimaEnable = false,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara21,
			OtherArgs = new List<object> { 10000, 15000, 0 },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Chara21",
			ItemNum = 1,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.StartDash_Limited,
			Category = Category.StartDash,
			Kind = Kind.PlayCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.All,
			ItemOID = "OID_FightSpirit_Wildcard",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int> { 1, 3, 5, 10, 30 },
			ExtraRewardList = new Dictionary<int, (string, int)>
			{
				{
					3,
					("OID_FightSpirit_Wildcard", 10)
				},
				{
					5,
					("OID_FightSpirit_Wildcard", 15)
				},
				{
					10,
					("OID_FightSpirit_Wildcard", 20)
				},
				{
					30,
					("OID_FightSpirit_Wildcard", 50)
				}
			}
		},
		new Item
		{
			MissionID = MissionID.StartDash_Endless,
			Category = Category.StartDash,
			Kind = Kind.PlayCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			LoopType = LoopType.Endless,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Wildcard",
			ItemNum = 1,
			FirstTerm = 40,
			CommonDifference = 10,
			RequiredMissionID = MissionID.StartDash_Limited
		},
		new Item
		{
			MissionID = MissionID.ScrambleMatch_School01,
			Category = Category.Scramble,
			Kind = Kind.ScrambleTop,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			OtherArgs = new List<object> { SchoolID.School001 },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.All,
			ItemOID = "OID_PlayerAccessory125",
			ItemNum = 1,
			NonNumericalAchievementList = new List<int> { 1, 10, 30 },
			ExtraRewardList = new Dictionary<int, (string, int)>
			{
				{
					10,
					("OID_PlayerAccessory126", 1)
				},
				{
					30,
					("OID_PlayerAccessory127", 1)
				}
			}
		},
		new Item
		{
			MissionID = MissionID.ScrambleMatch_School02,
			Category = Category.Scramble,
			Kind = Kind.ScrambleTop,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			OtherArgs = new List<object> { SchoolID.School002 },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.All,
			ItemOID = "OID_PlayerAccessory128",
			ItemNum = 1,
			NonNumericalAchievementList = new List<int> { 1, 10, 30 },
			ExtraRewardList = new Dictionary<int, (string, int)>
			{
				{
					10,
					("OID_PlayerAccessory129", 1)
				},
				{
					30,
					("OID_PlayerAccessory130", 1)
				}
			}
		},
		new Item
		{
			MissionID = MissionID.ScrambleMatch_School03,
			Category = Category.Scramble,
			Kind = Kind.ScrambleTop,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			OtherArgs = new List<object> { SchoolID.School003 },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.All,
			ItemOID = "OID_PlayerAccessory131",
			ItemNum = 1,
			NonNumericalAchievementList = new List<int> { 1, 10, 30 },
			ExtraRewardList = new Dictionary<int, (string, int)>
			{
				{
					10,
					("OID_PlayerAccessory132", 1)
				},
				{
					30,
					("OID_PlayerAccessory133", 1)
				}
			}
		},
		new Item
		{
			MissionID = MissionID.ScrambleMatch_School04,
			Category = Category.Scramble,
			GameEventType = GameEventType.CocoaAppearance,
			Kind = Kind.ScrambleTop,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			OtherArgs = new List<object> { SchoolID.School004 },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.All,
			ItemOID = "OID_PlayerAccessory134",
			ItemNum = 1,
			NonNumericalAchievementList = new List<int> { 1, 10, 30 },
			ExtraRewardList = new Dictionary<int, (string, int)>
			{
				{
					10,
					("OID_PlayerAccessory135", 1)
				},
				{
					30,
					("OID_PlayerAccessory136", 1)
				}
			}
		},
		new Item
		{
			MissionID = MissionID.ScrambleMatch_School05,
			Category = Category.Scramble,
			Kind = Kind.ScrambleTop,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			OtherArgs = new List<object> { SchoolID.School005 },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.All,
			ItemOID = "OID_PlayerAccessory137",
			ItemNum = 1,
			NonNumericalAchievementList = new List<int> { 1, 10, 30 },
			ExtraRewardList = new Dictionary<int, (string, int)>
			{
				{
					10,
					("OID_PlayerAccessory138", 1)
				},
				{
					30,
					("OID_PlayerAccessory139", 1)
				}
			}
		},
		new Item
		{
			MissionID = MissionID.ScrambleMatch_School06,
			Category = Category.Scramble,
			Kind = Kind.ScrambleTop,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			OtherArgs = new List<object> { SchoolID.School006 },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.All,
			ItemOID = "OID_PlayerAccessory140",
			ItemNum = 1,
			NonNumericalAchievementList = new List<int> { 1, 10, 30 },
			ExtraRewardList = new Dictionary<int, (string, int)>
			{
				{
					10,
					("OID_PlayerAccessory141", 1)
				},
				{
					30,
					("OID_PlayerAccessory142", 1)
				}
			}
		},
		new Item
		{
			MissionID = MissionID.ScrambleMatch_School07,
			Category = Category.Scramble,
			Kind = Kind.ScrambleTop,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			OtherArgs = new List<object> { SchoolID.School007 },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.All,
			ItemOID = "OID_PlayerAccessory143",
			ItemNum = 1,
			NonNumericalAchievementList = new List<int> { 1, 10, 30 },
			ExtraRewardList = new Dictionary<int, (string, int)>
			{
				{
					10,
					("OID_PlayerAccessory144", 1)
				},
				{
					30,
					("OID_PlayerAccessory145", 1)
				}
			}
		},
		new Item
		{
			MissionID = MissionID.ScrambleMatch_School08,
			Category = Category.Scramble,
			Kind = Kind.ScrambleTop,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			OtherArgs = new List<object> { SchoolID.School008 },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.All,
			ItemOID = "OID_PlayerAccessory146",
			ItemNum = 1,
			NonNumericalAchievementList = new List<int> { 1, 10, 30 },
			ExtraRewardList = new Dictionary<int, (string, int)>
			{
				{
					10,
					("OID_PlayerAccessory147", 1)
				},
				{
					30,
					("OID_PlayerAccessory148", 1)
				}
			}
		},
		new Item
		{
			MissionID = MissionID.ScrambleMatch_School09,
			Category = Category.Scramble,
			GameEventType = GameEventType.GrimAroeAppearance,
			Kind = Kind.ScrambleTop,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			OtherArgs = new List<object> { SchoolID.School009 },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.All,
			ItemOID = "OID_PlayerAccessory204",
			ItemNum = 1,
			NonNumericalAchievementList = new List<int> { 1, 10, 30 },
			ExtraRewardList = new Dictionary<int, (string, int)>
			{
				{
					10,
					("OID_PlayerAccessory205", 1)
				},
				{
					30,
					("OID_PlayerAccessory206", 1)
				}
			}
		},
		new Item
		{
			MissionID = MissionID.ScrambleMatch_School10,
			Category = Category.Scramble,
			GameEventType = GameEventType.ShiroeAppearance,
			Kind = Kind.ScrambleTop,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			OtherArgs = new List<object> { SchoolID.School010 },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.All,
			ItemOID = "OID_PlayerAccessory275",
			ItemNum = 1,
			NonNumericalAchievementList = new List<int> { 1, 10, 30 },
			ExtraRewardList = new Dictionary<int, (string, int)>
			{
				{
					10,
					("OID_PlayerAccessory276", 1)
				},
				{
					30,
					("OID_PlayerAccessory277", 1)
				}
			}
		},
		new Item
		{
			MissionID = MissionID.MahjongActor_01,
			Category = Category.MahjongActor,
			Kind = Kind.ProMatching,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara01,
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_Catchphrase_00_501",
			ItemNum = 1,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.MahjongActor_02,
			Category = Category.MahjongActor,
			Kind = Kind.ProMatching,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara02,
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_Catchphrase_00_502",
			ItemNum = 1,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.MahjongActor_03,
			Category = Category.MahjongActor,
			Kind = Kind.ProMatching,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara03,
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_Catchphrase_00_503",
			ItemNum = 1,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.MahjongActor_04,
			Category = Category.MahjongActor,
			Kind = Kind.ProMatching,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara04,
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_Catchphrase_00_504",
			ItemNum = 1,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.MahjongActor_05,
			Category = Category.MahjongActor,
			Kind = Kind.ProMatching,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara05,
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_Catchphrase_00_505",
			ItemNum = 1,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.MahjongActor_06,
			Category = Category.MahjongActor,
			Kind = Kind.ProMatching,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara06,
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_Catchphrase_00_506",
			ItemNum = 1,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.MahjongActor_07,
			Category = Category.MahjongActor,
			Kind = Kind.ProMatching,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara07,
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_Catchphrase_00_507",
			ItemNum = 1,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.MahjongActor_08,
			Category = Category.MahjongActor,
			Kind = Kind.ProMatching,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara08,
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_Catchphrase_00_508",
			ItemNum = 1,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.MahjongActor_11,
			Category = Category.MahjongActor,
			Kind = Kind.ProMatching,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara11,
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_Catchphrase_00_511",
			ItemNum = 1,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.MahjongActor_12,
			Category = Category.MahjongActor,
			Kind = Kind.ProMatching,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara12,
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_Catchphrase_00_512",
			ItemNum = 1,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Mentanpin2_Daily,
			Category = Category.Mentanpin2,
			UpdateInterval = UpdateInterval.Daily,
			Kind = Kind.EventTaikyoku,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			EventType = EventType.Mentanpin,
			TakuTypes = new List<TakuType> { TakuType.Sanma },
			LoopType = LoopType.Numerical,
			DisplayType = DisplayType.All,
			ItemOID = "Daikon",
			ItemNum = 3,
			FirstTerm = 1,
			CommonDifference = 4,
			LastTerm = 5,
			ExtraRewardList = new Dictionary<int, (string, int)> { 
			{
				5,
				("Daikon", 6)
			} }
		},
		new Item
		{
			MissionID = MissionID.Mentanpin2_Item,
			Category = Category.Mentanpin2,
			Kind = Kind.EventItem,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			EventType = EventType.Mentanpin,
			TakuTypes = new List<TakuType> { TakuType.Sanma },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.All,
			ItemOID = "OID_FightSpirit_Wildcard",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int> { 15, 30, 60, 100, 130, 160, 200 },
			ExtraRewardList = new Dictionary<int, (string, int)>
			{
				{
					30,
					("OID_PlayerAccessory194", 1)
				},
				{
					60,
					("OID_FightSpirit_Wildcard", 10)
				},
				{
					100,
					("OID_ReachStick132", 1)
				},
				{
					130,
					("OID_FightSpirit_Wildcard", 15)
				},
				{
					160,
					("OID_TableSticker199", 1)
				},
				{
					200,
					("OID_Catchphrase_00_027", 1)
				}
			}
		},
		new Item
		{
			MissionID = MissionID.Mentanpin2_Endless,
			Category = Category.Mentanpin2,
			Kind = Kind.EventItem,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			EventType = EventType.Mentanpin,
			TakuTypes = new List<TakuType> { TakuType.Sanma },
			LoopType = LoopType.Endless,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Wildcard",
			ItemNum = 1,
			FirstTerm = 230,
			CommonDifference = 30
		},
		new Item
		{
			MissionID = MissionID.HalfAnniv2024_Login,
			Category = Category.HalfAnniv2024,
			Kind = Kind.Login,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_ReachStick133",
			ItemNum = 1,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Halloween2024_Vs,
			Category = Category.Halloween2024,
			Kind = Kind.TaikyokuCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			LoopType = LoopType.Numerical,
			DisplayType = DisplayType.All,
			ItemOID = "OID_TableSticker209",
			ItemNum = 1,
			IsJandriProgressEnable = true,
			FirstTerm = 10,
			CommonDifference = 10,
			LastTerm = 50,
			ExtraRewardList = new Dictionary<int, (string, int)>
			{
				{
					20,
					("OID_ReachStick134", 1)
				},
				{
					30,
					("OID_PlayerAccessory195", 1)
				},
				{
					40,
					("OID_TileSurface008", 1)
				},
				{
					50,
					("OID_Catchphrase_00_036", 1)
				}
			}
		},
		new Item
		{
			MissionID = MissionID.GrimAroeTicket_Vs,
			Category = Category.GrimAroeTicket,
			Kind = Kind.PlayCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			LoopType = LoopType.Numerical,
			DisplayType = DisplayType.All,
			ItemOID = "OID_ExchangePoint_GrimAroe",
			ItemNum = 3,
			FirstTerm = 1,
			CommonDifference = 1,
			LastTerm = 10
		},
		new Item
		{
			MissionID = MissionID.Birthday2024_02_1,
			Category = Category.Birthday2024_02,
			Kind = Kind.Login,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_TableSticker218",
			ItemNum = 1,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Birthday2024_02_2,
			Category = Category.Birthday2024_02,
			Kind = Kind.TaikyokuCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_TableSticker126",
			ItemNum = 1,
			IsAutoClearItemPossession = true,
			FirstTerm = 5
		},
		new Item
		{
			MissionID = MissionID.MeldBonus_Daily,
			Category = Category.MeldBonus,
			UpdateInterval = UpdateInterval.Daily,
			Kind = Kind.EventTaikyoku,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			EventType = EventType.MeldBonus,
			TakuTypes = new List<TakuType> { TakuType.Tonpu },
			LoopType = LoopType.Numerical,
			DisplayType = DisplayType.All,
			ItemOID = "Kalbi",
			ItemNum = 5,
			FirstTerm = 1,
			CommonDifference = 4,
			LastTerm = 5,
			ExtraRewardList = new Dictionary<int, (string, int)> { 
			{
				5,
				("Kalbi", 10)
			} }
		},
		new Item
		{
			MissionID = MissionID.MeldBonus_Item,
			Category = Category.MeldBonus,
			Kind = Kind.EventItem,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			EventType = EventType.MeldBonus,
			TakuTypes = new List<TakuType> { TakuType.Tonpu },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.All,
			ItemOID = "OID_FightSpirit_Wildcard",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int> { 25, 50, 75, 100, 150, 200, 250 },
			ExtraRewardList = new Dictionary<int, (string, int)>
			{
				{
					50,
					("OID_PlayerAccessory207", 1)
				},
				{
					75,
					("OID_FightSpirit_Wildcard", 10)
				},
				{
					100,
					("OID_ReachStick143", 1)
				},
				{
					150,
					("OID_FightSpirit_Wildcard", 15)
				},
				{
					200,
					("OID_TableSticker219", 1)
				},
				{
					250,
					("OID_Catchphrase_00_037", 1)
				}
			}
		},
		new Item
		{
			MissionID = MissionID.MeldBonus_Endless,
			Category = Category.MeldBonus,
			Kind = Kind.EventItem,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			EventType = EventType.MeldBonus,
			TakuTypes = new List<TakuType> { TakuType.Tonpu },
			LoopType = LoopType.Endless,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Wildcard",
			ItemNum = 1,
			FirstTerm = 275,
			CommonDifference = 25
		},
		new Item
		{
			MissionID = MissionID.WelcomeComeback3_Login,
			Category = Category.WelcomeComeback3,
			Kind = Kind.Login,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			LoopType = LoopType.Numerical,
			DisplayType = DisplayType.All,
			ItemOID = "OID_FightSpirit_Wildcard",
			ItemNum = 10,
			FirstTerm = 1,
			CommonDifference = 1,
			LastTerm = 4,
			ExtraRewardList = new Dictionary<int, (string, int)>
			{
				{
					2,
					("OID_FightSpirit_Wildcard", 20)
				},
				{
					3,
					("OID_FightSpirit_Wildcard", 30)
				},
				{
					4,
					("OID_FightSpirit_Wildcard", 40)
				}
			}
		},
		new Item
		{
			MissionID = MissionID.Xmas2024_Daily,
			Category = Category.Xmas2024,
			UpdateInterval = UpdateInterval.Daily,
			Kind = Kind.Login,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "XmasCookie",
			ItemNum = 5,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Xmas2024_Login,
			Category = Category.Xmas2024,
			Kind = Kind.PlayCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			LoopType = LoopType.Endless,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "XmasCookie",
			ItemNum = 1,
			IsJandriRewardEnable = true,
			FirstTerm = 1,
			CommonDifference = 1
		},
		new Item
		{
			MissionID = MissionID.Xmas2024_First,
			Category = Category.Xmas2024,
			Kind = Kind.Login,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "XmasCookie",
			ItemNum = 5,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Competition6_Vs,
			Category = Category.Competition6,
			Kind = Kind.TaikyokuCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			LoopType = LoopType.Numerical,
			DisplayType = DisplayType.All,
			ItemOID = "OID_FightSpirit_Wildcard",
			ItemNum = 5,
			IsJandriProgressEnable = true,
			FirstTerm = 10,
			CommonDifference = 10,
			LastTerm = 50,
			ExtraRewardList = new Dictionary<int, (string, int)>
			{
				{
					20,
					("OID_ReachStick152", 1)
				},
				{
					30,
					("OID_FightSpirit_Wildcard", 10)
				},
				{
					40,
					("OID_TableSticker231", 1)
				},
				{
					50,
					("OID_TableSticker232", 1)
				}
			}
		},
		new Item
		{
			MissionID = MissionID.NewYear2025_Vs,
			Category = Category.NewYear2025,
			Kind = Kind.TaikyokuCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			LoopType = LoopType.Numerical,
			DisplayType = DisplayType.All,
			ItemOID = "OID_PlayerAccessory221",
			ItemNum = 1,
			IsJandriProgressEnable = true,
			FirstTerm = 10,
			CommonDifference = 10,
			LastTerm = 40,
			ExtraRewardList = new Dictionary<int, (string, int)>
			{
				{
					20,
					("OID_ReachStick153", 1)
				},
				{
					30,
					("OID_TableSticker229", 1)
				},
				{
					40,
					("OID_Catchphrase_00_040", 1)
				}
			},
			CardConnectInfo = (CardConnectMissionID.NewYear2025, new List<(CharaType, string)> { (CharaType.Chara13, "OID_TableSticker229") })
		},
		new Item
		{
			MissionID = MissionID.Birthday2025_13_1,
			Category = Category.Birthday2025_13,
			Kind = Kind.Login,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_TableSticker230",
			ItemNum = 1,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Valentine2025_Daily,
			Category = Category.Valentine2025,
			UpdateInterval = UpdateInterval.Daily,
			Kind = Kind.TaikyokuCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "ValentineLove",
			ItemNum = 5,
			FirstTerm = 1,
			AutoClearMissionIDs = new List<MissionID> { MissionID.Valentine2025_Main }
		},
		new Item
		{
			MissionID = MissionID.Valentine2025_Weekly,
			Category = Category.Valentine2025,
			UpdateInterval = UpdateInterval.Weekly,
			Kind = Kind.TaikyokuCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "ValentineLove",
			ItemNum = 5,
			FirstTerm = 1,
			AutoClearMissionIDs = new List<MissionID> { MissionID.Valentine2025_Main }
		},
		new Item
		{
			MissionID = MissionID.Valentine2025_Login,
			Category = Category.Valentine2025,
			Kind = Kind.TaikyokuCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			LoopType = LoopType.Endless,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "ValentineLove",
			ItemNum = 1,
			IsJandriRewardEnable = true,
			FirstTerm = 1,
			CommonDifference = 1,
			AutoClearMissionIDs = new List<MissionID> { MissionID.Valentine2025_Main }
		},
		new Item
		{
			MissionID = MissionID.Valentine2025_First,
			Category = Category.Valentine2025,
			Kind = Kind.Login,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_ReachStick154",
			ItemNum = 1,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Valentine2025_Main,
			Category = Category.Valentine2025,
			Kind = Kind.MissionEventItem,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.All,
			ItemOID = "ValentineReachStick",
			ItemNum = 1,
			NonNumericalAchievementList = new List<int>
			{
				10, 20, 40, 60, 80, 90, 100, 110, 120, 130,
				140, 150, 160, 170, 180, 190, 200, 210, 220
			},
			ExtraRewardList = new Dictionary<int, (string, int)>
			{
				{
					10,
					("OID_TableSticker233", 1)
				},
				{
					20,
					("OID_FightSpirit_Wildcard", 5)
				},
				{
					40,
					("OID_ReachStick155", 1)
				},
				{
					60,
					("OID_FightSpirit_Wildcard", 10)
				}
			}
		},
		new Item
		{
			MissionID = MissionID.Birthday2025_05_1,
			Category = Category.Birthday2025_05,
			Kind = Kind.Login,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_TableSticker234",
			ItemNum = 1,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Birthday2025_05_2,
			Category = Category.Birthday2025_05,
			Kind = Kind.TaikyokuCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_TableSticker158",
			ItemNum = 1,
			IsAutoClearItemPossession = true,
			FirstTerm = 5
		},
		new Item
		{
			MissionID = MissionID.Reversal_Daily,
			Category = Category.Reversal,
			UpdateInterval = UpdateInterval.Daily,
			Kind = Kind.EventTaikyoku,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			EventType = EventType.Reversal,
			TakuTypes = new List<TakuType> { TakuType.Sanma },
			LoopType = LoopType.Numerical,
			DisplayType = DisplayType.All,
			ItemOID = "Dumbbell",
			ItemNum = 5,
			FirstTerm = 1,
			CommonDifference = 4,
			LastTerm = 5,
			ExtraRewardList = new Dictionary<int, (string, int)> { 
			{
				5,
				("Dumbbell", 10)
			} }
		},
		new Item
		{
			MissionID = MissionID.Reversal_Item,
			Category = Category.Reversal,
			Kind = Kind.EventItem,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			EventType = EventType.Reversal,
			TakuTypes = new List<TakuType> { TakuType.Sanma },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.All,
			ItemOID = "OID_FightSpirit_Wildcard",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int> { 40, 80, 120, 160, 240, 320, 400 },
			ExtraRewardList = new Dictionary<int, (string, int)>
			{
				{
					80,
					("OID_TableSticker235", 1)
				},
				{
					120,
					("OID_FightSpirit_Wildcard", 10)
				},
				{
					160,
					("OID_ReachStick171", 1)
				},
				{
					240,
					("OID_FightSpirit_Wildcard", 15)
				},
				{
					320,
					("OID_PlayerAccessory222", 1)
				},
				{
					400,
					("OID_Catchphrase_00_041", 1)
				}
			}
		},
		new Item
		{
			MissionID = MissionID.Reversal_Endless,
			Category = Category.Reversal,
			Kind = Kind.EventItem,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			EventType = EventType.Reversal,
			TakuTypes = new List<TakuType> { TakuType.Sanma },
			LoopType = LoopType.Endless,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Wildcard",
			ItemNum = 1,
			FirstTerm = 440,
			CommonDifference = 40
		},
		new Item
		{
			MissionID = MissionID.Birthday2025_14_1,
			Category = Category.Birthday2025_14,
			Kind = Kind.Login,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_TableSticker236",
			ItemNum = 1,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.DiaTicket_Vs,
			Category = Category.DiaTicket,
			Kind = Kind.PlayCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			LoopType = LoopType.Numerical,
			DisplayType = DisplayType.All,
			ItemOID = "OID_ExchangePoint_Dia",
			ItemNum = 3,
			FirstTerm = 1,
			CommonDifference = 1,
			LastTerm = 10
		},
		new Item
		{
			MissionID = MissionID.Birthday2025_15_1,
			Category = Category.Birthday2025_15,
			Kind = Kind.Login,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_TableSticker237",
			ItemNum = 1,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Birthday2025_09_1,
			Category = Category.Birthday2025_09,
			Kind = Kind.Login,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_TableSticker246",
			ItemNum = 1,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Birthday2025_09_2,
			Category = Category.Birthday2025_09,
			Kind = Kind.TaikyokuCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_TableSticker160",
			ItemNum = 1,
			IsAutoClearItemPossession = true,
			FirstTerm = 5
		},
		new Item
		{
			MissionID = MissionID.Anniv2025_Login,
			Category = Category.Anniv2025,
			Kind = Kind.Login,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			LoopType = LoopType.Numerical,
			DisplayType = DisplayType.All,
			ItemOID = "OID_PlayerAccessory231",
			ItemNum = 1,
			FirstTerm = 1,
			CommonDifference = 1,
			LastTerm = 4,
			ExtraRewardList = new Dictionary<int, (string, int)>
			{
				{
					2,
					("OID_TableSticker247", 1)
				},
				{
					3,
					("OID_ReachStick180", 1)
				},
				{
					4,
					("OID_PlayerAccessory232", 1)
				}
			}
		},
		new Item
		{
			MissionID = MissionID.WelcomeComeback4_Login,
			Category = Category.WelcomeComeback4,
			Kind = Kind.Login,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			LoopType = LoopType.Numerical,
			DisplayType = DisplayType.All,
			ItemOID = "OID_FightSpirit_Wildcard",
			ItemNum = 10,
			FirstTerm = 1,
			CommonDifference = 1,
			LastTerm = 4,
			ExtraRewardList = new Dictionary<int, (string, int)>
			{
				{
					2,
					("OID_FightSpirit_Wildcard", 20)
				},
				{
					3,
					("OID_FightSpirit_Wildcard", 30)
				},
				{
					4,
					("OID_FightSpirit_Wildcard", 40)
				}
			}
		},
		new Item
		{
			MissionID = MissionID.Aotenjo2025_Vs,
			Category = Category.Aotenjo2025,
			Kind = Kind.EventTaikyoku,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			EventType = EventType.Aotenjo,
			TakuTypes = new List<TakuType> { TakuType.Sanma },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.All,
			ItemOID = "OID_PlayerAccessory233",
			ItemNum = 1,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Birthday2025_16_1,
			Category = Category.Birthday2025_16,
			Kind = Kind.Login,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_TableSticker248",
			ItemNum = 1,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Birthday2025_07_1,
			Category = Category.Birthday2025_07,
			Kind = Kind.Login,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_TableSticker249",
			ItemNum = 1,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Birthday2025_07_2,
			Category = Category.Birthday2025_07,
			Kind = Kind.TaikyokuCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_TableSticker164",
			ItemNum = 1,
			IsAutoClearItemPossession = true,
			FirstTerm = 5
		},
		new Item
		{
			MissionID = MissionID.Janpon2_Daily,
			Category = Category.Janpon2,
			UpdateInterval = UpdateInterval.Daily,
			Kind = Kind.TaikyokuCount,
			IsNimaEnable = false,
			IsKikenEnable = false,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "AnniversaryMedal",
			ItemNum = 20,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Janpon2_Vs,
			Category = Category.Janpon2,
			Kind = Kind.TaikyokuCount,
			IsNimaEnable = false,
			IsKikenEnable = false,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			LoopType = LoopType.Endless,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "AnniversaryMedal",
			ItemNum = 5,
			IsJandriRewardEnable = true,
			FirstTerm = 1,
			CommonDifference = 1
		},
		new Item
		{
			MissionID = MissionID.Bemani002_01,
			Category = Category.Bemani002,
			Kind = Kind.TaikyokuCount,
			IsResultAcquire = true,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara01,
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_ReachBgm116",
			ItemNum = 1,
			IsJandriProgressEnable = true,
			NonNumericalAchievementList = new List<int> { 2, 7, 10 },
			ExtraRewardList = new Dictionary<int, (string, int)>
			{
				{
					7,
					("OID_ReachBgm117", 1)
				},
				{
					10,
					("OID_MyRoomBgm048", 1)
				}
			}
		},
		new Item
		{
			MissionID = MissionID.Bemani002_02,
			Category = Category.Bemani002,
			Kind = Kind.TaikyokuCount,
			IsResultAcquire = true,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara02,
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_ReachBgm118",
			ItemNum = 1,
			IsJandriProgressEnable = true,
			NonNumericalAchievementList = new List<int> { 2, 7, 10 },
			ExtraRewardList = new Dictionary<int, (string, int)>
			{
				{
					7,
					("OID_ReachBgm119", 1)
				},
				{
					10,
					("OID_MyRoomBgm049", 1)
				}
			}
		},
		new Item
		{
			MissionID = MissionID.Bemani002_03,
			Category = Category.Bemani002,
			Kind = Kind.TaikyokuCount,
			IsResultAcquire = true,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara03,
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_ReachBgm120",
			ItemNum = 1,
			IsJandriProgressEnable = true,
			NonNumericalAchievementList = new List<int> { 2, 7, 10 },
			ExtraRewardList = new Dictionary<int, (string, int)>
			{
				{
					7,
					("OID_ReachBgm121", 1)
				},
				{
					10,
					("OID_MyRoomBgm050", 1)
				}
			}
		},
		new Item
		{
			MissionID = MissionID.Bemani002_04,
			Category = Category.Bemani002,
			Kind = Kind.TaikyokuCount,
			IsResultAcquire = true,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara04,
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_ReachBgm122",
			ItemNum = 1,
			IsJandriProgressEnable = true,
			NonNumericalAchievementList = new List<int> { 2, 7, 10 },
			ExtraRewardList = new Dictionary<int, (string, int)>
			{
				{
					7,
					("OID_ReachBgm123", 1)
				},
				{
					10,
					("OID_MyRoomBgm051", 1)
				}
			}
		},
		new Item
		{
			MissionID = MissionID.Bemani002_05,
			Category = Category.Bemani002,
			Kind = Kind.TaikyokuCount,
			IsResultAcquire = true,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara05,
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_ReachBgm124",
			ItemNum = 1,
			IsJandriProgressEnable = true,
			NonNumericalAchievementList = new List<int> { 2, 7, 10 },
			ExtraRewardList = new Dictionary<int, (string, int)>
			{
				{
					7,
					("OID_ReachBgm125", 1)
				},
				{
					10,
					("OID_MyRoomBgm052", 1)
				}
			}
		},
		new Item
		{
			MissionID = MissionID.Bemani002_06,
			Category = Category.Bemani002,
			Kind = Kind.TaikyokuCount,
			IsResultAcquire = true,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara06,
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_ReachBgm126",
			ItemNum = 1,
			IsJandriProgressEnable = true,
			NonNumericalAchievementList = new List<int> { 2, 7, 10 },
			ExtraRewardList = new Dictionary<int, (string, int)>
			{
				{
					7,
					("OID_ReachBgm127", 1)
				},
				{
					10,
					("OID_MyRoomBgm053", 1)
				}
			}
		},
		new Item
		{
			MissionID = MissionID.Bemani002_07,
			Category = Category.Bemani002,
			Kind = Kind.TaikyokuCount,
			IsResultAcquire = true,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara07,
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_ReachBgm128",
			ItemNum = 1,
			IsJandriProgressEnable = true,
			NonNumericalAchievementList = new List<int> { 2, 7, 10 },
			ExtraRewardList = new Dictionary<int, (string, int)>
			{
				{
					7,
					("OID_ReachBgm129", 1)
				},
				{
					10,
					("OID_MyRoomBgm054", 1)
				}
			}
		},
		new Item
		{
			MissionID = MissionID.Bemani002_08,
			Category = Category.Bemani002,
			Kind = Kind.TaikyokuCount,
			IsResultAcquire = true,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara08,
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_ReachBgm130",
			ItemNum = 1,
			IsJandriProgressEnable = true,
			NonNumericalAchievementList = new List<int> { 2, 7, 10 },
			ExtraRewardList = new Dictionary<int, (string, int)>
			{
				{
					7,
					("OID_ReachBgm131", 1)
				},
				{
					10,
					("OID_MyRoomBgm055", 1)
				}
			}
		},
		new Item
		{
			MissionID = MissionID.Bemani002_09,
			Category = Category.Bemani002,
			Kind = Kind.TaikyokuCount,
			IsResultAcquire = true,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara09,
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_ReachBgm132",
			ItemNum = 1,
			IsJandriProgressEnable = true,
			NonNumericalAchievementList = new List<int> { 2, 7, 10 },
			ExtraRewardList = new Dictionary<int, (string, int)>
			{
				{
					7,
					("OID_ReachBgm133", 1)
				},
				{
					10,
					("OID_MyRoomBgm056", 1)
				}
			}
		},
		new Item
		{
			MissionID = MissionID.Bemani002_10,
			Category = Category.Bemani002,
			Kind = Kind.TaikyokuCount,
			IsResultAcquire = true,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara10,
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_ReachBgm134",
			ItemNum = 1,
			IsJandriProgressEnable = true,
			NonNumericalAchievementList = new List<int> { 2, 7, 10 },
			ExtraRewardList = new Dictionary<int, (string, int)>
			{
				{
					7,
					("OID_ReachBgm135", 1)
				},
				{
					10,
					("OID_MyRoomBgm057", 1)
				}
			}
		},
		new Item
		{
			MissionID = MissionID.Bemani002_11,
			Category = Category.Bemani002,
			Kind = Kind.TaikyokuCount,
			IsResultAcquire = true,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara11,
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_ReachBgm136",
			ItemNum = 1,
			IsJandriProgressEnable = true,
			NonNumericalAchievementList = new List<int> { 2, 7, 10 },
			ExtraRewardList = new Dictionary<int, (string, int)>
			{
				{
					7,
					("OID_ReachBgm137", 1)
				},
				{
					10,
					("OID_MyRoomBgm058", 1)
				}
			}
		},
		new Item
		{
			MissionID = MissionID.Bemani002_12,
			Category = Category.Bemani002,
			Kind = Kind.TaikyokuCount,
			IsResultAcquire = true,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara12,
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_ReachBgm138",
			ItemNum = 1,
			IsJandriProgressEnable = true,
			NonNumericalAchievementList = new List<int> { 2, 7, 10 },
			ExtraRewardList = new Dictionary<int, (string, int)>
			{
				{
					7,
					("OID_ReachBgm139", 1)
				},
				{
					10,
					("OID_MyRoomBgm059", 1)
				}
			}
		},
		new Item
		{
			MissionID = MissionID.Bemani002_13,
			Category = Category.Bemani002,
			Kind = Kind.TaikyokuCount,
			IsResultAcquire = true,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara13,
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_ReachBgm140",
			ItemNum = 1,
			IsJandriProgressEnable = true,
			NonNumericalAchievementList = new List<int> { 2, 7, 10 },
			ExtraRewardList = new Dictionary<int, (string, int)>
			{
				{
					7,
					("OID_ReachBgm141", 1)
				},
				{
					10,
					("OID_MyRoomBgm060", 1)
				}
			}
		},
		new Item
		{
			MissionID = MissionID.Bemani002_14,
			Category = Category.Bemani002,
			Kind = Kind.TaikyokuCount,
			IsResultAcquire = true,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara14,
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_ReachBgm142",
			ItemNum = 1,
			IsJandriProgressEnable = true,
			NonNumericalAchievementList = new List<int> { 2, 7, 10 },
			ExtraRewardList = new Dictionary<int, (string, int)>
			{
				{
					7,
					("OID_ReachBgm143", 1)
				},
				{
					10,
					("OID_MyRoomBgm061", 1)
				}
			}
		},
		new Item
		{
			MissionID = MissionID.Bemani002_15,
			Category = Category.Bemani002,
			Kind = Kind.TaikyokuCount,
			IsResultAcquire = true,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara15,
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_ReachBgm144",
			ItemNum = 1,
			IsJandriProgressEnable = true,
			NonNumericalAchievementList = new List<int> { 2, 7, 10 },
			ExtraRewardList = new Dictionary<int, (string, int)>
			{
				{
					7,
					("OID_ReachBgm145", 1)
				},
				{
					10,
					("OID_MyRoomBgm062", 1)
				}
			}
		},
		new Item
		{
			MissionID = MissionID.Bemani002_16,
			Category = Category.Bemani002,
			Kind = Kind.TaikyokuCount,
			IsResultAcquire = true,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara16,
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_ReachBgm146",
			ItemNum = 1,
			IsJandriProgressEnable = true,
			NonNumericalAchievementList = new List<int> { 2, 7, 10 },
			ExtraRewardList = new Dictionary<int, (string, int)>
			{
				{
					7,
					("OID_ReachBgm147", 1)
				},
				{
					10,
					("OID_MyRoomBgm063", 1)
				}
			}
		},
		new Item
		{
			MissionID = MissionID.Blooming_Vs,
			Category = Category.Blooming,
			Kind = Kind.TaikyokuCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			LoopType = LoopType.Numerical,
			DisplayType = DisplayType.All,
			ItemOID = "OID_PlayerAccessory234",
			ItemNum = 1,
			IsJandriProgressEnable = true,
			FirstTerm = 10,
			CommonDifference = 10,
			LastTerm = 30,
			ExtraRewardList = new Dictionary<int, (string, int)>
			{
				{
					20,
					("OID_StageBg002", 1)
				},
				{
					30,
					("OID_Catchphrase_00_044", 1)
				}
			}
		},
		new Item
		{
			MissionID = MissionID.Competition7_Vs,
			Category = Category.Competition7,
			Kind = Kind.TaikyokuCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			LoopType = LoopType.Numerical,
			DisplayType = DisplayType.All,
			ItemOID = "OID_FightSpirit_Wildcard",
			ItemNum = 5,
			IsJandriProgressEnable = true,
			FirstTerm = 10,
			CommonDifference = 10,
			LastTerm = 40,
			ExtraRewardList = new Dictionary<int, (string, int)>
			{
				{
					20,
					("OID_ReachStick181", 1)
				},
				{
					30,
					("OID_FightSpirit_Wildcard", 10)
				},
				{
					40,
					("OID_TableSticker250", 1)
				}
			}
		},
		new Item
		{
			MissionID = MissionID.GW2025_Login,
			Category = Category.GW2025,
			Kind = Kind.Login,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			LoopType = LoopType.Numerical,
			DisplayType = DisplayType.All,
			ItemOID = "OID_FightSpirit_Wildcard",
			ItemNum = 5,
			FirstTerm = 1,
			CommonDifference = 1,
			LastTerm = 3,
			ExtraRewardList = new Dictionary<int, (string, int)>
			{
				{
					2,
					("OID_ReachStick182", 1)
				},
				{
					3,
					("OID_PlayerAccessory239", 1)
				}
			}
		},
		new Item
		{
			MissionID = MissionID.CharaPai001_Daily,
			Category = Category.CharaPai001,
			UpdateInterval = UpdateInterval.Daily,
			Kind = Kind.Login,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "CharaPaiMedal001",
			ItemNum = 10,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.CharaPai001_Login,
			Category = Category.CharaPai001,
			Kind = Kind.PlayCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			LoopType = LoopType.Endless,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "CharaPaiMedal001",
			ItemNum = 1,
			IsJandriRewardEnable = true,
			FirstTerm = 1,
			CommonDifference = 1
		},
		new Item
		{
			MissionID = MissionID.MeldBonus2_Daily,
			Category = Category.MeldBonus2,
			UpdateInterval = UpdateInterval.Daily,
			Kind = Kind.EventTaikyoku,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			EventType = EventType.MeldBonus,
			TakuTypes = new List<TakuType>
			{
				TakuType.Tonpu,
				TakuType.Sanma
			},
			LoopType = LoopType.Numerical,
			DisplayType = DisplayType.All,
			ItemOID = "Kalbi",
			ItemNum = 5,
			FirstTerm = 1,
			CommonDifference = 4,
			LastTerm = 5,
			ExtraRewardList = new Dictionary<int, (string, int)> { 
			{
				5,
				("Kalbi", 10)
			} }
		},
		new Item
		{
			MissionID = MissionID.MeldBonus2_Item,
			Category = Category.MeldBonus2,
			Kind = Kind.EventItem,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			EventType = EventType.MeldBonus,
			TakuTypes = new List<TakuType>
			{
				TakuType.Tonpu,
				TakuType.Sanma
			},
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.All,
			ItemOID = "OID_FightSpirit_Wildcard",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int> { 25, 50, 75, 100, 150, 200, 250 },
			ExtraRewardList = new Dictionary<int, (string, int)>
			{
				{
					50,
					("OID_TableSticker251", 1)
				},
				{
					75,
					("OID_FightSpirit_Wildcard", 10)
				},
				{
					100,
					("OID_ReachStick183", 1)
				},
				{
					150,
					("OID_FightSpirit_Wildcard", 15)
				},
				{
					200,
					("OID_PlayerAccessory240", 1)
				},
				{
					250,
					("OID_Catchphrase_00_045", 1)
				}
			}
		},
		new Item
		{
			MissionID = MissionID.MeldBonus2_Endless,
			Category = Category.MeldBonus2,
			Kind = Kind.EventItem,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			EventType = EventType.MeldBonus,
			TakuTypes = new List<TakuType>
			{
				TakuType.Tonpu,
				TakuType.Sanma
			},
			LoopType = LoopType.Endless,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Wildcard",
			ItemNum = 1,
			FirstTerm = 275,
			CommonDifference = 25
		},
		new Item
		{
			MissionID = MissionID.Birthday2025_11_1,
			Category = Category.Birthday2025_11_12,
			Kind = Kind.Login,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_TableSticker252",
			ItemNum = 1,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Birthday2025_11_2,
			Category = Category.Birthday2025_11_12,
			Kind = Kind.TaikyokuCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_TableSticker167",
			ItemNum = 1,
			IsAutoClearItemPossession = true,
			FirstTerm = 5
		},
		new Item
		{
			MissionID = MissionID.Birthday2025_12_1,
			Category = Category.Birthday2025_11_12,
			Kind = Kind.Login,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_TableSticker253",
			ItemNum = 1,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Birthday2025_12_2,
			Category = Category.Birthday2025_11_12,
			Kind = Kind.TaikyokuCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_TableSticker168",
			ItemNum = 1,
			IsAutoClearItemPossession = true,
			FirstTerm = 5
		},
		new Item
		{
			MissionID = MissionID.Birthday2025_10_1,
			Category = Category.Birthday2025_10,
			Kind = Kind.Login,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_TableSticker254",
			ItemNum = 1,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Birthday2025_10_2,
			Category = Category.Birthday2025_10,
			Kind = Kind.TaikyokuCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_TableSticker171",
			ItemNum = 1,
			IsAutoClearItemPossession = true,
			FirstTerm = 5
		},
		new Item
		{
			MissionID = MissionID.Birthday2025_08_1,
			Category = Category.Birthday2025_08,
			Kind = Kind.Login,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_TableSticker255",
			ItemNum = 1,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Birthday2025_08_2,
			Category = Category.Birthday2025_08,
			Kind = Kind.TaikyokuCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_TableSticker172",
			ItemNum = 1,
			IsAutoClearItemPossession = true,
			FirstTerm = 5
		},
		new Item
		{
			MissionID = MissionID.CharaPai002_Daily,
			Category = Category.CharaPai002,
			UpdateInterval = UpdateInterval.Daily,
			Kind = Kind.Login,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "CharaPaiMedal002",
			ItemNum = 10,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.CharaPai002_Login,
			Category = Category.CharaPai002,
			Kind = Kind.PlayCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			LoopType = LoopType.Endless,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "CharaPaiMedal002",
			ItemNum = 1,
			IsJandriRewardEnable = true,
			FirstTerm = 1,
			CommonDifference = 1
		},
		new Item
		{
			MissionID = MissionID.DoubrielTicket_Vs,
			Category = Category.DoubrielTicket,
			Kind = Kind.PlayCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			LoopType = LoopType.Numerical,
			DisplayType = DisplayType.All,
			ItemOID = "OID_ExchangePoint_Doubriel",
			ItemNum = 3,
			FirstTerm = 1,
			CommonDifference = 1,
			LastTerm = 10
		},
		new Item
		{
			MissionID = MissionID.Birthday2025_03_1,
			Category = Category.Birthday2025_03,
			Kind = Kind.Login,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_TableSticker266",
			ItemNum = 1,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Birthday2025_03_2,
			Category = Category.Birthday2025_03,
			Kind = Kind.TaikyokuCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_TableSticker173",
			ItemNum = 1,
			IsAutoClearItemPossession = true,
			FirstTerm = 5
		},
		new Item
		{
			MissionID = MissionID.Bomb_Daily,
			Category = Category.Bomb,
			UpdateInterval = UpdateInterval.Daily,
			Kind = Kind.EventTaikyoku,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			EventType = EventType.Bomb,
			TakuTypes = new List<TakuType>
			{
				TakuType.Tonpu,
				TakuType.Sanma
			},
			LoopType = LoopType.Numerical,
			DisplayType = DisplayType.All,
			ItemOID = "ConeIce",
			ItemNum = 5,
			FirstTerm = 1,
			CommonDifference = 4,
			LastTerm = 5,
			ExtraRewardList = new Dictionary<int, (string, int)> { 
			{
				5,
				("ConeIce", 10)
			} }
		},
		new Item
		{
			MissionID = MissionID.Bomb_Item,
			Category = Category.Bomb,
			Kind = Kind.EventItem,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			EventType = EventType.Bomb,
			TakuTypes = new List<TakuType>
			{
				TakuType.Tonpu,
				TakuType.Sanma
			},
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.All,
			ItemOID = "OID_FightSpirit_Wildcard",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int> { 25, 50, 100, 150, 200, 250, 300 },
			ExtraRewardList = new Dictionary<int, (string, int)>
			{
				{
					50,
					("OID_PlayerAccessory249", 1)
				},
				{
					100,
					("OID_FightSpirit_Wildcard", 10)
				},
				{
					150,
					("OID_ReachStick192", 1)
				},
				{
					200,
					("OID_FightSpirit_Wildcard", 15)
				},
				{
					250,
					("OID_TableSticker267", 1)
				},
				{
					300,
					("OID_Catchphrase_00_048", 1)
				}
			}
		},
		new Item
		{
			MissionID = MissionID.Bomb_Endless,
			Category = Category.Bomb,
			Kind = Kind.EventItem,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			EventType = EventType.Bomb,
			TakuTypes = new List<TakuType>
			{
				TakuType.Tonpu,
				TakuType.Sanma
			},
			LoopType = LoopType.Endless,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Wildcard",
			ItemNum = 1,
			FirstTerm = 325,
			CommonDifference = 25
		},
		new Item
		{
			MissionID = MissionID.Poolside_Vs,
			Category = Category.Poolside,
			Kind = Kind.TaikyokuCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			LoopType = LoopType.Numerical,
			DisplayType = DisplayType.All,
			ItemOID = "OID_PlayerAccessory250",
			ItemNum = 1,
			IsJandriProgressEnable = true,
			FirstTerm = 10,
			CommonDifference = 10,
			LastTerm = 30,
			ExtraRewardList = new Dictionary<int, (string, int)>
			{
				{
					20,
					("OID_StageBg003", 1)
				},
				{
					30,
					("OID_Catchphrase_00_051", 1)
				}
			}
		},
		new Item
		{
			MissionID = MissionID.Ponnomichi_Login,
			Category = Category.Ponnomichi,
			Kind = Kind.Login,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			LoopType = LoopType.Numerical,
			DisplayType = DisplayType.All,
			ItemOID = "OID_TableSticker269",
			ItemNum = 1,
			FirstTerm = 1,
			CommonDifference = 1,
			LastTerm = 7,
			ExtraRewardList = new Dictionary<int, (string, int)>
			{
				{
					2,
					("OID_TableSticker270", 1)
				},
				{
					3,
					("OID_TableSticker271", 1)
				},
				{
					4,
					("OID_TableSticker272", 1)
				},
				{
					5,
					("OID_TableSticker273", 1)
				},
				{
					6,
					("OID_PlayerAccessory251", 1)
				},
				{
					7,
					("OID_04AgariA03", 1)
				}
			}
		},
		new Item
		{
			MissionID = MissionID.Bonodori001_Daily,
			Category = Category.Bonodori001,
			UpdateInterval = UpdateInterval.Daily,
			Kind = Kind.Login,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "BonodoriMedal001",
			ItemNum = 10,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Bonodori001_Login,
			Category = Category.Bonodori001,
			Kind = Kind.PlayCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			LoopType = LoopType.Endless,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "BonodoriMedal001",
			ItemNum = 1,
			IsJandriRewardEnable = true,
			FirstTerm = 1,
			CommonDifference = 1
		},
		new Item
		{
			MissionID = MissionID.Birthday2025_06_1,
			Category = Category.Birthday2025_06,
			Kind = Kind.Login,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_TableSticker274",
			ItemNum = 1,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Birthday2025_06_2,
			Category = Category.Birthday2025_06,
			Kind = Kind.TaikyokuCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_TableSticker196",
			ItemNum = 1,
			IsAutoClearItemPossession = true,
			FirstTerm = 5
		},
		new Item
		{
			MissionID = MissionID.WelcomeComeback5_Login,
			Category = Category.WelcomeComeback5,
			Kind = Kind.Login,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			LoopType = LoopType.Numerical,
			DisplayType = DisplayType.All,
			ItemOID = "OID_FightSpirit_Wildcard",
			ItemNum = 10,
			FirstTerm = 1,
			CommonDifference = 1,
			LastTerm = 4,
			ExtraRewardList = new Dictionary<int, (string, int)>
			{
				{
					2,
					("OID_FightSpirit_Wildcard", 20)
				},
				{
					3,
					("OID_FightSpirit_Wildcard", 30)
				},
				{
					4,
					("OID_FightSpirit_Wildcard", 40)
				}
			}
		},
		new Item
		{
			MissionID = MissionID.Competition8_Vs,
			Category = Category.Competition8,
			Kind = Kind.TaikyokuCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			LoopType = LoopType.Numerical,
			DisplayType = DisplayType.All,
			ItemOID = "OID_FightSpirit_Wildcard",
			ItemNum = 5,
			IsJandriProgressEnable = true,
			FirstTerm = 10,
			CommonDifference = 10,
			LastTerm = 40,
			ExtraRewardList = new Dictionary<int, (string, int)>
			{
				{
					20,
					("OID_ReachStick193", 1)
				},
				{
					30,
					("OID_FightSpirit_Wildcard", 10)
				},
				{
					40,
					("OID_TableSticker275", 1)
				}
			}
		},
		new Item
		{
			MissionID = MissionID.Bonodori002_Daily,
			Category = Category.Bonodori002,
			UpdateInterval = UpdateInterval.Daily,
			Kind = Kind.Login,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "BonodoriMedal002",
			ItemNum = 10,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Bonodori002_Login,
			Category = Category.Bonodori002,
			Kind = Kind.PlayCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			LoopType = LoopType.Endless,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "BonodoriMedal002",
			ItemNum = 1,
			IsJandriRewardEnable = true,
			FirstTerm = 1,
			CommonDifference = 1
		},
		new Item
		{
			MissionID = MissionID.Birthday2025_01_1,
			Category = Category.Birthday2025_01,
			Kind = Kind.Login,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_TableSticker276",
			ItemNum = 1,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Birthday2025_01_2,
			Category = Category.Birthday2025_01,
			Kind = Kind.TaikyokuCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_TableSticker198",
			ItemNum = 1,
			IsAutoClearItemPossession = true,
			FirstTerm = 5
		},
		new Item
		{
			MissionID = MissionID.IppatsuTicket_Vs,
			Category = Category.IppatsuTicket,
			Kind = Kind.PlayCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			LoopType = LoopType.Numerical,
			DisplayType = DisplayType.All,
			ItemOID = "OID_ExchangePoint_Ippatsu",
			ItemNum = 3,
			FirstTerm = 1,
			CommonDifference = 1,
			LastTerm = 10
		},
		new Item
		{
			MissionID = MissionID.Birthday2025_04_1,
			Category = Category.Birthday2025_04,
			Kind = Kind.Login,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_TableSticker286",
			ItemNum = 1,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Birthday2025_04_2,
			Category = Category.Birthday2025_04,
			Kind = Kind.TaikyokuCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_TableSticker200",
			ItemNum = 1,
			IsAutoClearItemPossession = true,
			FirstTerm = 5
		},
		new Item
		{
			MissionID = MissionID.Bingo3_Item,
			Category = Category.Bingo3,
			Kind = Kind.Bingo,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.All,
			ItemOID = "OID_PlayerAccessory264",
			ItemNum = 1,
			NonNumericalAchievementList = new List<int> { 10, 20, 30, 40, 50 },
			ExtraRewardList = new Dictionary<int, (string, int)>
			{
				{
					20,
					("JanponMedal", 20)
				},
				{
					30,
					("OID_TableSticker287", 1)
				},
				{
					40,
					("JanponMedal", 30)
				},
				{
					50,
					("OID_TileSurface025", 1)
				}
			}
		},
		new Item
		{
			MissionID = MissionID.Bingo3_Endless,
			Category = Category.Bingo3,
			Kind = Kind.Bingo,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			LoopType = LoopType.Endless,
			DisplayType = DisplayType.Single,
			ItemOID = "JanponMedal",
			ItemNum = 10,
			FirstTerm = 60,
			CommonDifference = 10
		},
		new Item
		{
			MissionID = MissionID.HalfAnniv2025_Login,
			Category = Category.HalfAnniv2025,
			Kind = Kind.Login,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			LoopType = LoopType.Numerical,
			DisplayType = DisplayType.All,
			ItemOID = "OID_ReachStick202",
			ItemNum = 1,
			FirstTerm = 1,
			CommonDifference = 1,
			LastTerm = 5,
			ExtraRewardList = new Dictionary<int, (string, int)>
			{
				{
					2,
					("JanponMedal", 10)
				},
				{
					3,
					("JanponMedal", 20)
				},
				{
					4,
					("JanponMedal", 30)
				},
				{
					5,
					("JanponMedal", 40)
				}
			}
		},
		new Item
		{
			MissionID = MissionID.Janpon3_Daily,
			Category = Category.Janpon3,
			UpdateInterval = UpdateInterval.Daily,
			Kind = Kind.TaikyokuCount,
			IsNimaEnable = false,
			IsKikenEnable = false,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "JanponMedal",
			ItemNum = 20,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Janpon3_Vs,
			Category = Category.Janpon3,
			Kind = Kind.TaikyokuCount,
			IsNimaEnable = false,
			IsKikenEnable = false,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			LoopType = LoopType.Endless,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "JanponMedal",
			ItemNum = 5,
			IsJandriRewardEnable = true,
			FirstTerm = 1,
			CommonDifference = 1
		},
		new Item
		{
			MissionID = MissionID.AllGreen_Daily,
			Category = Category.AllGreen,
			UpdateInterval = UpdateInterval.Daily,
			Kind = Kind.EventTaikyoku,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			EventType = EventType.AllGreen,
			TakuTypes = new List<TakuType>
			{
				TakuType.Tonpu,
				TakuType.Sanma
			},
			LoopType = LoopType.Numerical,
			DisplayType = DisplayType.All,
			ItemOID = "Battery",
			ItemNum = 5,
			FirstTerm = 1,
			CommonDifference = 4,
			LastTerm = 5,
			ExtraRewardList = new Dictionary<int, (string, int)> { 
			{
				5,
				("Battery", 10)
			} }
		},
		new Item
		{
			MissionID = MissionID.AllGreen_Item,
			Category = Category.AllGreen,
			Kind = Kind.EventItem,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			EventType = EventType.AllGreen,
			TakuTypes = new List<TakuType>
			{
				TakuType.Tonpu,
				TakuType.Sanma
			},
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.All,
			ItemOID = "OID_FightSpirit_Wildcard",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int> { 25, 50, 100, 150, 200, 250, 300 },
			ExtraRewardList = new Dictionary<int, (string, int)>
			{
				{
					50,
					("OID_TableSticker288", 1)
				},
				{
					100,
					("OID_FightSpirit_Wildcard", 10)
				},
				{
					150,
					("OID_ReachStick203", 1)
				},
				{
					200,
					("OID_FightSpirit_Wildcard", 15)
				},
				{
					250,
					("OID_PlayerAccessory265", 1)
				},
				{
					300,
					("OID_Catchphrase_00_052", 1)
				}
			}
		},
		new Item
		{
			MissionID = MissionID.AllGreen_Endless,
			Category = Category.AllGreen,
			Kind = Kind.EventItem,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			EventType = EventType.AllGreen,
			TakuTypes = new List<TakuType>
			{
				TakuType.Tonpu,
				TakuType.Sanma
			},
			LoopType = LoopType.Endless,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Wildcard",
			ItemNum = 1,
			FirstTerm = 325,
			CommonDifference = 25
		},
		new Item
		{
			MissionID = MissionID.Birthday2025_18_1,
			Category = Category.Birthday2025_18,
			Kind = Kind.Login,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_TableSticker289",
			ItemNum = 1,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Birthday2025_02_1,
			Category = Category.Birthday2025_02,
			Kind = Kind.Login,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_TableSticker290",
			ItemNum = 1,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Birthday2025_02_2,
			Category = Category.Birthday2025_02,
			Kind = Kind.TaikyokuCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_TableSticker218",
			ItemNum = 1,
			IsAutoClearItemPossession = true,
			FirstTerm = 5
		},
		new Item
		{
			MissionID = MissionID.AutumnLeaves_Vs,
			Category = Category.AutumnLeaves,
			Kind = Kind.TaikyokuCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			LoopType = LoopType.Numerical,
			DisplayType = DisplayType.All,
			ItemOID = "OID_ReachStick204",
			ItemNum = 1,
			IsJandriProgressEnable = true,
			FirstTerm = 10,
			CommonDifference = 10,
			LastTerm = 30,
			ExtraRewardList = new Dictionary<int, (string, int)>
			{
				{
					20,
					("OID_StageBg004", 1)
				},
				{
					30,
					("OID_Catchphrase_00_055", 1)
				}
			}
		},
		new Item
		{
			MissionID = MissionID.BowBow2025_Login,
			Category = Category.BowBow2025,
			Kind = Kind.Login,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			LoopType = LoopType.Numerical,
			DisplayType = DisplayType.All,
			ItemOID = "OID_PlayerAccessory266",
			ItemNum = 1,
			FirstTerm = 1,
			CommonDifference = 1,
			LastTerm = 7,
			ExtraRewardList = new Dictionary<int, (string, int)>
			{
				{
					2,
					("OID_FightSpirit_Wildcard", 5)
				},
				{
					3,
					("OID_TableSticker291", 1)
				},
				{
					4,
					("OID_FightSpirit_Wildcard", 5)
				},
				{
					5,
					("OID_TableSticker292", 1)
				},
				{
					6,
					("OID_FightSpirit_Wildcard", 5)
				},
				{
					7,
					("OID_FightSpirit_Wildcard", 15)
				}
			}
		},
		new Item
		{
			MissionID = MissionID.Birthday2025_17_1,
			Category = Category.Birthday2025_17,
			Kind = Kind.Login,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_TableSticker293",
			ItemNum = 1,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.ShiroeTicket_Vs,
			Category = Category.ShiroeTicket,
			Kind = Kind.PlayCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			LoopType = LoopType.Numerical,
			DisplayType = DisplayType.All,
			ItemOID = "OID_ExchangePoint_Shiroe",
			ItemNum = 3,
			FirstTerm = 1,
			CommonDifference = 1,
			LastTerm = 10
		},
		new Item
		{
			MissionID = MissionID.DecorationSticker_Vs,
			Category = Category.DecorationSticker,
			Kind = Kind.TaikyokuCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.All,
			ItemOID = "OID_TableSticker295",
			ItemNum = 1,
			IsJandriProgressEnable = true,
			NonNumericalAchievementList = new List<int> { 1, 3, 5, 7, 9, 12, 15, 20 },
			ExtraRewardList = new Dictionary<int, (string, int)>
			{
				{
					3,
					("OID_TableSticker296", 1)
				},
				{
					5,
					("OID_TableSticker297", 1)
				},
				{
					7,
					("OID_TableSticker298", 1)
				},
				{
					9,
					("OID_TableSticker299", 1)
				},
				{
					12,
					("OID_TableSticker300", 1)
				},
				{
					15,
					("OID_TableSticker301", 1)
				},
				{
					20,
					("OID_TableSticker302", 1)
				}
			}
		},
		new Item
		{
			MissionID = MissionID.MotherWars_Win,
			Category = Category.MotherWars,
			Kind = Kind.MotherWars,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			LoopType = LoopType.Endless,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Wildcard",
			ItemNum = 2,
			FirstTerm = 1,
			CommonDifference = 1
		},
		new Item
		{
			MissionID = MissionID.Anniv1000days_Login,
			Category = Category.Anniv1000days,
			Kind = Kind.Login,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_TableSticker313",
			ItemNum = 1,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Omikuji2026_Login,
			Category = Category.Omikuji2026,
			Kind = Kind.Login,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_PlayerAccessory280",
			ItemNum = 1,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.ShopCompe001_1_Vs,
			Category = Category.ShopCompe001_1,
			Kind = Kind.TaikyokuCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			LoopType = LoopType.Numerical,
			DisplayType = DisplayType.All,
			ItemOID = "OID_FightSpirit_Wildcard",
			ItemNum = 10,
			IsJandriProgressEnable = true,
			FirstTerm = 10,
			CommonDifference = 10,
			LastTerm = 30,
			ExtraRewardList = new Dictionary<int, (string, int)>
			{
				{
					20,
					("OID_ReachStick213", 1)
				},
				{
					30,
					("OID_TableSticker314", 1)
				}
			}
		},
		new Item
		{
			MissionID = MissionID.ShopCompe001_2_Vs,
			Category = Category.ShopCompe001_2,
			Kind = Kind.TaikyokuCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			LoopType = LoopType.Numerical,
			DisplayType = DisplayType.All,
			ItemOID = "OID_FightSpirit_Wildcard",
			ItemNum = 10,
			IsJandriProgressEnable = true,
			FirstTerm = 10,
			CommonDifference = 10,
			LastTerm = 30,
			ExtraRewardList = new Dictionary<int, (string, int)>
			{
				{
					20,
					("OID_PlayerAccessory281", 1)
				},
				{
					30,
					("OID_TableSticker315", 1)
				}
			}
		},
		new Item
		{
			MissionID = MissionID.Birthday2026_13_1,
			Category = Category.Birthday2026_13,
			Kind = Kind.Login,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_TableSticker316",
			ItemNum = 1,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Birthday2026_13_2,
			Category = Category.Birthday2026_13,
			Kind = Kind.TaikyokuCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_TableSticker230",
			ItemNum = 1,
			IsAutoClearItemPossession = true,
			FirstTerm = 5
		},
		new Item
		{
			MissionID = MissionID.Valentine2026_Daily,
			Category = Category.Valentine2026,
			UpdateInterval = UpdateInterval.Daily,
			Kind = Kind.TaikyokuCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "ValentineLove2",
			ItemNum = 5,
			FirstTerm = 1,
			AutoClearMissionIDs = new List<MissionID> { MissionID.Valentine2026_Main }
		},
		new Item
		{
			MissionID = MissionID.Valentine2026_Weekly,
			Category = Category.Valentine2026,
			UpdateInterval = UpdateInterval.Weekly,
			Kind = Kind.TaikyokuCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "ValentineLove2",
			ItemNum = 5,
			FirstTerm = 1,
			AutoClearMissionIDs = new List<MissionID> { MissionID.Valentine2026_Main }
		},
		new Item
		{
			MissionID = MissionID.Valentine2026_Login,
			Category = Category.Valentine2026,
			Kind = Kind.TaikyokuCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			LoopType = LoopType.Endless,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "ValentineLove2",
			ItemNum = 1,
			IsJandriRewardEnable = true,
			FirstTerm = 1,
			CommonDifference = 1,
			AutoClearMissionIDs = new List<MissionID> { MissionID.Valentine2026_Main }
		},
		new Item
		{
			MissionID = MissionID.Valentine2026_First,
			Category = Category.Valentine2026,
			Kind = Kind.Login,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_ReachStick154",
			ItemNum = 1,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Valentine2026_Main,
			Category = Category.Valentine2026,
			Kind = Kind.MissionEventItem,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.All,
			ItemOID = "ValentineReachStick",
			ItemNum = 1,
			NonNumericalAchievementList = new List<int>
			{
				10, 20, 40, 60, 80, 100, 120, 140, 160, 180,
				200, 220, 240, 260, 280, 300, 320, 340, 360, 380,
				400, 420, 440
			},
			ExtraRewardList = new Dictionary<int, (string, int)>
			{
				{
					10,
					("OID_FightSpirit_Wildcard", 5)
				},
				{
					20,
					("OID_TableSticker319", 1)
				},
				{
					40,
					("OID_FightSpirit_Wildcard", 10)
				},
				{
					60,
					("OID_ReachStick155", 1)
				}
			}
		},
		new Item
		{
			MissionID = MissionID.Birthday2026_19_1,
			Category = Category.Birthday2026_19,
			Kind = Kind.Login,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_TableSticker317",
			ItemNum = 1,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Birthday2026_05_1,
			Category = Category.Birthday2026_05,
			Kind = Kind.Login,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_TableSticker318",
			ItemNum = 1,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Birthday2026_05_2,
			Category = Category.Birthday2026_05,
			Kind = Kind.TaikyokuCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_TableSticker234",
			ItemNum = 1,
			IsAutoClearItemPossession = true,
			FirstTerm = 5
		},
		new Item
		{
			MissionID = MissionID.Comeback2_Daily,
			Category = Category.Comeback2,
			UpdateInterval = UpdateInterval.Daily,
			Kind = Kind.EventTaikyoku,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			EventType = EventType.Comeback,
			TakuTypes = new List<TakuType>
			{
				TakuType.Tonpu,
				TakuType.Sanma
			},
			LoopType = LoopType.Numerical,
			DisplayType = DisplayType.All,
			ItemOID = "Crowbar",
			ItemNum = 5,
			FirstTerm = 1,
			CommonDifference = 4,
			LastTerm = 5,
			ExtraRewardList = new Dictionary<int, (string, int)> { 
			{
				5,
				("Crowbar", 10)
			} }
		},
		new Item
		{
			MissionID = MissionID.Comeback2_Item,
			Category = Category.Comeback2,
			Kind = Kind.EventItem,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			EventType = EventType.Comeback,
			TakuTypes = new List<TakuType>
			{
				TakuType.Tonpu,
				TakuType.Sanma
			},
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.All,
			ItemOID = "OID_FightSpirit_Wildcard",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int> { 25, 50, 100, 150, 200, 250, 300 },
			ExtraRewardList = new Dictionary<int, (string, int)>
			{
				{
					50,
					("OID_ReachStick218", 1)
				},
				{
					100,
					("OID_FightSpirit_Wildcard", 10)
				},
				{
					150,
					("OID_TableSticker320", 1)
				},
				{
					200,
					("OID_FightSpirit_Wildcard", 15)
				},
				{
					250,
					("OID_PlayerAccessory358", 1)
				},
				{
					300,
					("OID_Catchphrase_00_056", 1)
				}
			}
		},
		new Item
		{
			MissionID = MissionID.Comeback2_Endless,
			Category = Category.Comeback2,
			Kind = Kind.EventItem,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			EventType = EventType.Comeback,
			TakuTypes = new List<TakuType>
			{
				TakuType.Tonpu,
				TakuType.Sanma
			},
			LoopType = LoopType.Endless,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Wildcard",
			ItemNum = 1,
			FirstTerm = 325,
			CommonDifference = 25
		},
		new Item
		{
			MissionID = MissionID.Atomic_Daily,
			Category = Category.Atomic,
			UpdateInterval = UpdateInterval.Daily,
			Kind = Kind.EventTaikyoku,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			EventType = EventType.Atomic,
			TakuTypes = new List<TakuType>
			{
				TakuType.Tonpu,
				TakuType.Sanma
			},
			LoopType = LoopType.Numerical,
			DisplayType = DisplayType.All,
			ItemOID = "Hanko",
			ItemNum = 5,
			FirstTerm = 1,
			CommonDifference = 4,
			LastTerm = 5,
			ExtraRewardList = new Dictionary<int, (string, int)> { 
			{
				5,
				("Hanko", 10)
			} }
		},
		new Item
		{
			MissionID = MissionID.Atomic_Item,
			Category = Category.Atomic,
			Kind = Kind.EventItem,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			EventType = EventType.Atomic,
			TakuTypes = new List<TakuType>
			{
				TakuType.Tonpu,
				TakuType.Sanma
			},
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.All,
			ItemOID = "OID_FightSpirit_Wildcard",
			ItemNum = 5,
			NonNumericalAchievementList = new List<int> { 25, 50, 100, 150, 200, 250, 300 },
			ExtraRewardList = new Dictionary<int, (string, int)>
			{
				{
					50,
					("OID_TableSticker324", 1)
				},
				{
					100,
					("OID_FightSpirit_Wildcard", 10)
				},
				{
					150,
					("OID_ReachStick219", 1)
				},
				{
					200,
					("OID_FightSpirit_Wildcard", 15)
				},
				{
					250,
					("OID_PlayerAccessory359", 1)
				},
				{
					300,
					("OID_Catchphrase_00_059", 1)
				}
			}
		},
		new Item
		{
			MissionID = MissionID.Atomic_Endless,
			Category = Category.Atomic,
			Kind = Kind.EventItem,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			EventType = EventType.Atomic,
			TakuTypes = new List<TakuType>
			{
				TakuType.Tonpu,
				TakuType.Sanma
			},
			LoopType = LoopType.Endless,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_FightSpirit_Wildcard",
			ItemNum = 1,
			FirstTerm = 325,
			CommonDifference = 25
		},
		new Item
		{
			MissionID = MissionID.Birthday2026_14_1,
			Category = Category.Birthday2026_14,
			Kind = Kind.Login,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_TableSticker322",
			ItemNum = 1,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Birthday2026_14_2,
			Category = Category.Birthday2026_14,
			Kind = Kind.TaikyokuCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_TableSticker236",
			ItemNum = 1,
			IsAutoClearItemPossession = true,
			FirstTerm = 5
		},
		new Item
		{
			MissionID = MissionID.Birthday2026_15_1,
			Category = Category.Birthday2026_15,
			Kind = Kind.Login,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_TableSticker323",
			ItemNum = 1,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Birthday2026_15_2,
			Category = Category.Birthday2026_15,
			Kind = Kind.TaikyokuCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_TableSticker237",
			ItemNum = 1,
			IsAutoClearItemPossession = true,
			FirstTerm = 5
		},
		new Item
		{
			MissionID = MissionID.JunTicket_Vs,
			Category = Category.JunTicket,
			Kind = Kind.PlayCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			LoopType = LoopType.Numerical,
			DisplayType = DisplayType.All,
			ItemOID = "OID_ExchangePoint_Jun",
			ItemNum = 3,
			FirstTerm = 1,
			CommonDifference = 1,
			LastTerm = 10
		},
		new Item
		{
			MissionID = MissionID.Birthday2026_09_1,
			Category = Category.Birthday2026_09,
			Kind = Kind.Login,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_TableSticker333",
			ItemNum = 1,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Birthday2026_09_2,
			Category = Category.Birthday2026_09,
			Kind = Kind.TaikyokuCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_TableSticker246",
			ItemNum = 1,
			IsAutoClearItemPossession = true,
			FirstTerm = 5
		},
		new Item
		{
			MissionID = MissionID.CharaPai003_Daily,
			Category = Category.CharaPai003,
			UpdateInterval = UpdateInterval.Daily,
			Kind = Kind.Login,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "CharaPaiMedal003",
			ItemNum = 10,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.CharaPai003_Login,
			Category = Category.CharaPai003,
			Kind = Kind.PlayCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			LoopType = LoopType.Endless,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "CharaPaiMedal003",
			ItemNum = 1,
			IsJandriRewardEnable = true,
			FirstTerm = 1,
			CommonDifference = 1
		},
		new Item
		{
			MissionID = MissionID.CharaPai003_First,
			Category = Category.CharaPai003,
			Kind = Kind.Login,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "CharaPaiMedal003",
			ItemNum = 20,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Janpon4_Daily,
			Category = Category.Janpon4,
			UpdateInterval = UpdateInterval.Daily,
			Kind = Kind.TaikyokuCount,
			IsNimaEnable = false,
			IsKikenEnable = false,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "JanponMedal2",
			ItemNum = 20,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Janpon4_Vs,
			Category = Category.Janpon4,
			Kind = Kind.TaikyokuCount,
			IsNimaEnable = false,
			IsKikenEnable = false,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			LoopType = LoopType.Endless,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "JanponMedal2",
			ItemNum = 5,
			IsJandriRewardEnable = true,
			FirstTerm = 1,
			CommonDifference = 1
		},
		new Item
		{
			MissionID = MissionID.Anniv2026_Login,
			Category = Category.Anniv2026,
			Kind = Kind.Login,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			LoopType = LoopType.Numerical,
			DisplayType = DisplayType.All,
			ItemOID = "OID_ReachStick228",
			ItemNum = 1,
			FirstTerm = 1,
			CommonDifference = 1,
			LastTerm = 5,
			ExtraRewardList = new Dictionary<int, (string, int)>
			{
				{
					2,
					("JanponMedal2", 10)
				},
				{
					3,
					("JanponMedal2", 20)
				},
				{
					4,
					("JanponMedal2", 30)
				},
				{
					5,
					("JanponMedal2", 40)
				}
			}
		},
		new Item
		{
			MissionID = MissionID.Birthday2026_07_1,
			Category = Category.Birthday2026_07,
			Kind = Kind.Login,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_TableSticker334",
			ItemNum = 1,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Birthday2026_07_2,
			Category = Category.Birthday2026_07,
			Kind = Kind.TaikyokuCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_TableSticker249",
			ItemNum = 1,
			IsAutoClearItemPossession = true,
			FirstTerm = 5
		},
		new Item
		{
			MissionID = MissionID.Birthday2026_16_1,
			Category = Category.Birthday2026_16,
			Kind = Kind.Login,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_TableSticker335",
			ItemNum = 1,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Birthday2026_16_2,
			Category = Category.Birthday2026_16,
			Kind = Kind.TaikyokuCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_TableSticker248",
			ItemNum = 1,
			IsAutoClearItemPossession = true,
			FirstTerm = 5
		},
		new Item
		{
			MissionID = MissionID.Bemani003_01,
			Category = Category.Bemani003,
			Kind = Kind.TaikyokuCount,
			IsResultAcquire = true,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara01,
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_ReachBgm188",
			ItemNum = 1,
			IsJandriProgressEnable = true,
			NonNumericalAchievementList = new List<int> { 2, 7, 10 },
			ExtraRewardList = new Dictionary<int, (string, int)>
			{
				{
					7,
					("OID_ReachBgm189", 1)
				},
				{
					10,
					("OID_MyRoomBgm066", 1)
				}
			}
		},
		new Item
		{
			MissionID = MissionID.Bemani003_02,
			Category = Category.Bemani003,
			Kind = Kind.TaikyokuCount,
			IsResultAcquire = true,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara02,
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_ReachBgm190",
			ItemNum = 1,
			IsJandriProgressEnable = true,
			NonNumericalAchievementList = new List<int> { 2, 7, 10 },
			ExtraRewardList = new Dictionary<int, (string, int)>
			{
				{
					7,
					("OID_ReachBgm191", 1)
				},
				{
					10,
					("OID_MyRoomBgm067", 1)
				}
			}
		},
		new Item
		{
			MissionID = MissionID.Bemani003_03,
			Category = Category.Bemani003,
			Kind = Kind.TaikyokuCount,
			IsResultAcquire = true,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara03,
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_ReachBgm192",
			ItemNum = 1,
			IsJandriProgressEnable = true,
			NonNumericalAchievementList = new List<int> { 2, 7, 10 },
			ExtraRewardList = new Dictionary<int, (string, int)>
			{
				{
					7,
					("OID_ReachBgm193", 1)
				},
				{
					10,
					("OID_MyRoomBgm068", 1)
				}
			}
		},
		new Item
		{
			MissionID = MissionID.Bemani003_04,
			Category = Category.Bemani003,
			Kind = Kind.TaikyokuCount,
			IsResultAcquire = true,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara04,
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_ReachBgm194",
			ItemNum = 1,
			IsJandriProgressEnable = true,
			NonNumericalAchievementList = new List<int> { 2, 7, 10 },
			ExtraRewardList = new Dictionary<int, (string, int)>
			{
				{
					7,
					("OID_ReachBgm195", 1)
				},
				{
					10,
					("OID_MyRoomBgm069", 1)
				}
			}
		},
		new Item
		{
			MissionID = MissionID.Bemani003_05,
			Category = Category.Bemani003,
			Kind = Kind.TaikyokuCount,
			IsResultAcquire = true,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara05,
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_ReachBgm196",
			ItemNum = 1,
			IsJandriProgressEnable = true,
			NonNumericalAchievementList = new List<int> { 2, 7, 10 },
			ExtraRewardList = new Dictionary<int, (string, int)>
			{
				{
					7,
					("OID_ReachBgm197", 1)
				},
				{
					10,
					("OID_MyRoomBgm070", 1)
				}
			}
		},
		new Item
		{
			MissionID = MissionID.Bemani003_06,
			Category = Category.Bemani003,
			Kind = Kind.TaikyokuCount,
			IsResultAcquire = true,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara06,
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_ReachBgm198",
			ItemNum = 1,
			IsJandriProgressEnable = true,
			NonNumericalAchievementList = new List<int> { 2, 7, 10 },
			ExtraRewardList = new Dictionary<int, (string, int)>
			{
				{
					7,
					("OID_ReachBgm199", 1)
				},
				{
					10,
					("OID_MyRoomBgm071", 1)
				}
			}
		},
		new Item
		{
			MissionID = MissionID.Bemani003_07,
			Category = Category.Bemani003,
			Kind = Kind.TaikyokuCount,
			IsResultAcquire = true,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara07,
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_ReachBgm200",
			ItemNum = 1,
			IsJandriProgressEnable = true,
			NonNumericalAchievementList = new List<int> { 2, 7, 10 },
			ExtraRewardList = new Dictionary<int, (string, int)>
			{
				{
					7,
					("OID_ReachBgm201", 1)
				},
				{
					10,
					("OID_MyRoomBgm072", 1)
				}
			}
		},
		new Item
		{
			MissionID = MissionID.Bemani003_08,
			Category = Category.Bemani003,
			Kind = Kind.TaikyokuCount,
			IsResultAcquire = true,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara08,
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_ReachBgm202",
			ItemNum = 1,
			IsJandriProgressEnable = true,
			NonNumericalAchievementList = new List<int> { 2, 7, 10 },
			ExtraRewardList = new Dictionary<int, (string, int)>
			{
				{
					7,
					("OID_ReachBgm203", 1)
				},
				{
					10,
					("OID_MyRoomBgm073", 1)
				}
			}
		},
		new Item
		{
			MissionID = MissionID.Bemani003_09,
			Category = Category.Bemani003,
			Kind = Kind.TaikyokuCount,
			IsResultAcquire = true,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara09,
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_ReachBgm204",
			ItemNum = 1,
			IsJandriProgressEnable = true,
			NonNumericalAchievementList = new List<int> { 2, 7, 10 },
			ExtraRewardList = new Dictionary<int, (string, int)>
			{
				{
					7,
					("OID_ReachBgm205", 1)
				},
				{
					10,
					("OID_MyRoomBgm074", 1)
				}
			}
		},
		new Item
		{
			MissionID = MissionID.Bemani003_10,
			Category = Category.Bemani003,
			Kind = Kind.TaikyokuCount,
			IsResultAcquire = true,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara10,
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_ReachBgm206",
			ItemNum = 1,
			IsJandriProgressEnable = true,
			NonNumericalAchievementList = new List<int> { 2, 7, 10 },
			ExtraRewardList = new Dictionary<int, (string, int)>
			{
				{
					7,
					("OID_ReachBgm207", 1)
				},
				{
					10,
					("OID_MyRoomBgm075", 1)
				}
			}
		},
		new Item
		{
			MissionID = MissionID.Bemani003_11,
			Category = Category.Bemani003,
			Kind = Kind.TaikyokuCount,
			IsResultAcquire = true,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara11,
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_ReachBgm208",
			ItemNum = 1,
			IsJandriProgressEnable = true,
			NonNumericalAchievementList = new List<int> { 2, 7, 10 },
			ExtraRewardList = new Dictionary<int, (string, int)>
			{
				{
					7,
					("OID_ReachBgm209", 1)
				},
				{
					10,
					("OID_MyRoomBgm076", 1)
				}
			}
		},
		new Item
		{
			MissionID = MissionID.Bemani003_12,
			Category = Category.Bemani003,
			Kind = Kind.TaikyokuCount,
			IsResultAcquire = true,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara12,
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_ReachBgm210",
			ItemNum = 1,
			IsJandriProgressEnable = true,
			NonNumericalAchievementList = new List<int> { 2, 7, 10 },
			ExtraRewardList = new Dictionary<int, (string, int)>
			{
				{
					7,
					("OID_ReachBgm211", 1)
				},
				{
					10,
					("OID_MyRoomBgm077", 1)
				}
			}
		},
		new Item
		{
			MissionID = MissionID.Bemani003_13,
			Category = Category.Bemani003,
			Kind = Kind.TaikyokuCount,
			IsResultAcquire = true,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara13,
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_ReachBgm212",
			ItemNum = 1,
			IsJandriProgressEnable = true,
			NonNumericalAchievementList = new List<int> { 2, 7, 10 },
			ExtraRewardList = new Dictionary<int, (string, int)>
			{
				{
					7,
					("OID_ReachBgm213", 1)
				},
				{
					10,
					("OID_MyRoomBgm078", 1)
				}
			}
		},
		new Item
		{
			MissionID = MissionID.Bemani003_14,
			Category = Category.Bemani003,
			Kind = Kind.TaikyokuCount,
			IsResultAcquire = true,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara14,
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_ReachBgm214",
			ItemNum = 1,
			IsJandriProgressEnable = true,
			NonNumericalAchievementList = new List<int> { 2, 7, 10 },
			ExtraRewardList = new Dictionary<int, (string, int)>
			{
				{
					7,
					("OID_ReachBgm215", 1)
				},
				{
					10,
					("OID_MyRoomBgm079", 1)
				}
			}
		},
		new Item
		{
			MissionID = MissionID.Bemani003_15,
			Category = Category.Bemani003,
			Kind = Kind.TaikyokuCount,
			IsResultAcquire = true,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara15,
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_ReachBgm216",
			ItemNum = 1,
			IsJandriProgressEnable = true,
			NonNumericalAchievementList = new List<int> { 2, 7, 10 },
			ExtraRewardList = new Dictionary<int, (string, int)>
			{
				{
					7,
					("OID_ReachBgm217", 1)
				},
				{
					10,
					("OID_MyRoomBgm080", 1)
				}
			}
		},
		new Item
		{
			MissionID = MissionID.Bemani003_16,
			Category = Category.Bemani003,
			Kind = Kind.TaikyokuCount,
			IsResultAcquire = true,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara16,
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_ReachBgm218",
			ItemNum = 1,
			IsJandriProgressEnable = true,
			NonNumericalAchievementList = new List<int> { 2, 7, 10 },
			ExtraRewardList = new Dictionary<int, (string, int)>
			{
				{
					7,
					("OID_ReachBgm219", 1)
				},
				{
					10,
					("OID_MyRoomBgm081", 1)
				}
			}
		},
		new Item
		{
			MissionID = MissionID.Bemani003_17,
			Category = Category.Bemani003,
			Kind = Kind.TaikyokuCount,
			IsResultAcquire = true,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara17,
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_ReachBgm220",
			ItemNum = 1,
			IsJandriProgressEnable = true,
			NonNumericalAchievementList = new List<int> { 2, 7, 10 },
			ExtraRewardList = new Dictionary<int, (string, int)>
			{
				{
					7,
					("OID_ReachBgm221", 1)
				},
				{
					10,
					("OID_MyRoomBgm082", 1)
				}
			}
		},
		new Item
		{
			MissionID = MissionID.Bemani003_18,
			Category = Category.Bemani003,
			Kind = Kind.TaikyokuCount,
			IsResultAcquire = true,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara18,
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_ReachBgm222",
			ItemNum = 1,
			IsJandriProgressEnable = true,
			NonNumericalAchievementList = new List<int> { 2, 7, 10 },
			ExtraRewardList = new Dictionary<int, (string, int)>
			{
				{
					7,
					("OID_ReachBgm223", 1)
				},
				{
					10,
					("OID_MyRoomBgm083", 1)
				}
			}
		},
		new Item
		{
			MissionID = MissionID.Bemani003_19,
			Category = Category.Bemani003,
			Kind = Kind.TaikyokuCount,
			IsResultAcquire = true,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara19,
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_ReachBgm224",
			ItemNum = 1,
			IsJandriProgressEnable = true,
			NonNumericalAchievementList = new List<int> { 2, 7, 10 },
			ExtraRewardList = new Dictionary<int, (string, int)>
			{
				{
					7,
					("OID_ReachBgm225", 1)
				},
				{
					10,
					("OID_MyRoomBgm084", 1)
				}
			}
		},
		new Item
		{
			MissionID = MissionID.Bemani003_20,
			Category = Category.Bemani003,
			Kind = Kind.TaikyokuCount,
			IsResultAcquire = true,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			Chara = CharaType.Chara20,
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_ReachBgm226",
			ItemNum = 1,
			IsJandriProgressEnable = true,
			NonNumericalAchievementList = new List<int> { 2, 7, 10 },
			ExtraRewardList = new Dictionary<int, (string, int)>
			{
				{
					7,
					("OID_ReachBgm227", 1)
				},
				{
					10,
					("OID_MyRoomBgm085", 1)
				}
			}
		},
		new Item
		{
			MissionID = MissionID.Aotenjo2026_Vs,
			Category = Category.Aotenjo2026,
			Kind = Kind.EventTaikyoku,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			EventType = EventType.Aotenjo,
			TakuTypes = new List<TakuType> { TakuType.Sanma },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.All,
			ItemOID = "OID_PlayerAccessory368",
			ItemNum = 1,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Spring2026_Vs,
			Category = Category.Spring2026,
			Kind = Kind.TaikyokuCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.All,
			ItemOID = "OID_FightSpirit_Wildcard",
			ItemNum = 5,
			IsJandriProgressEnable = true,
			NonNumericalAchievementList = new List<int> { 5, 10, 15, 20, 30, 40, 50 },
			ExtraRewardList = new Dictionary<int, (string, int)>
			{
				{
					10,
					("OID_TableSticker336", 1)
				},
				{
					15,
					("OID_ReachStick229", 1)
				},
				{
					20,
					("OID_TableSticker337", 1)
				},
				{
					30,
					("OID_PlayerAccessory369", 1)
				},
				{
					40,
					("OID_TableSticker338", 1)
				},
				{
					50,
					("OID_Catchphrase_00_062", 1)
				}
			}
		},
		new Item
		{
			MissionID = MissionID.Birthday2026_11_1,
			Category = Category.Birthday2026_11_12,
			Kind = Kind.Login,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_TableSticker339",
			ItemNum = 1,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Birthday2026_11_2,
			Category = Category.Birthday2026_11_12,
			Kind = Kind.TaikyokuCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_TableSticker252",
			ItemNum = 1,
			IsAutoClearItemPossession = true,
			FirstTerm = 5
		},
		new Item
		{
			MissionID = MissionID.Birthday2026_12_1,
			Category = Category.Birthday2026_11_12,
			Kind = Kind.Login,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_TableSticker340",
			ItemNum = 1,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Birthday2026_12_2,
			Category = Category.Birthday2026_11_12,
			Kind = Kind.TaikyokuCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_TableSticker253",
			ItemNum = 1,
			IsAutoClearItemPossession = true,
			FirstTerm = 5
		},
		new Item
		{
			MissionID = MissionID.Birthday2026_10_1,
			Category = Category.Birthday2026_10,
			Kind = Kind.Login,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_TableSticker341",
			ItemNum = 1,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Birthday2026_10_2,
			Category = Category.Birthday2026_10,
			Kind = Kind.TaikyokuCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_TableSticker254",
			ItemNum = 1,
			IsAutoClearItemPossession = true,
			FirstTerm = 5
		},
		new Item
		{
			MissionID = MissionID.SportsDay_Vs,
			Category = Category.SportsDay,
			Kind = Kind.TaikyokuCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			LoopType = LoopType.NonNumerical,
			DisplayType = DisplayType.All,
			ItemOID = "OID_FightSpirit_Wildcard",
			ItemNum = 5,
			IsJandriProgressEnable = true,
			NonNumericalAchievementList = new List<int> { 5, 10, 15, 20, 25, 30, 40, 50 },
			ExtraRewardList = new Dictionary<int, (string, int)>
			{
				{
					10,
					("OID_PlayerAccessory374", 1)
				},
				{
					15,
					("OID_PlayerAccessory375", 1)
				},
				{
					20,
					("OID_TableSticker342", 1)
				},
				{
					25,
					("OID_ReachStick230", 1)
				},
				{
					30,
					("OID_TableSticker343", 1)
				},
				{
					40,
					("OID_TableSticker344", 1)
				},
				{
					50,
					("OID_Catchphrase_00_063", 1)
				}
			}
		},
		new Item
		{
			MissionID = MissionID.RainyDay001_Daily,
			Category = Category.RainyDay001,
			UpdateInterval = UpdateInterval.Daily,
			Kind = Kind.Login,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "RainyDayMedal001",
			ItemNum = 10,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.RainyDay001_Login,
			Category = Category.RainyDay001,
			Kind = Kind.PlayCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			LoopType = LoopType.Endless,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "RainyDayMedal001",
			ItemNum = 1,
			IsJandriRewardEnable = true,
			FirstTerm = 1,
			CommonDifference = 1
		},
		new Item
		{
			MissionID = MissionID.Competition9_Vs,
			Category = Category.Competition9,
			Kind = Kind.TaikyokuCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			LoopType = LoopType.Numerical,
			DisplayType = DisplayType.All,
			ItemOID = "OID_FightSpirit_Wildcard",
			ItemNum = 5,
			IsJandriProgressEnable = true,
			FirstTerm = 10,
			CommonDifference = 10,
			LastTerm = 50,
			ExtraRewardList = new Dictionary<int, (string, int)>
			{
				{
					20,
					("OID_ReachStick231", 1)
				},
				{
					30,
					("OID_FightSpirit_Wildcard", 10)
				},
				{
					40,
					("OID_TableSticker345", 1)
				},
				{
					50,
					("OID_TableSticker346", 1)
				}
			}
		},
		new Item
		{
			MissionID = MissionID.WelcomeComeback6_Login,
			Category = Category.WelcomeComeback6,
			Kind = Kind.Login,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			LoopType = LoopType.Numerical,
			DisplayType = DisplayType.All,
			ItemOID = "OID_FightSpirit_Wildcard",
			ItemNum = 10,
			FirstTerm = 1,
			CommonDifference = 1,
			LastTerm = 4,
			ExtraRewardList = new Dictionary<int, (string, int)>
			{
				{
					2,
					("OID_FightSpirit_Wildcard", 20)
				},
				{
					3,
					("OID_FightSpirit_Wildcard", 30)
				},
				{
					4,
					("OID_FightSpirit_Wildcard", 40)
				}
			}
		},
		new Item
		{
			MissionID = MissionID.Birthday2026_08_1,
			Category = Category.Birthday2026_08,
			Kind = Kind.Login,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_TableSticker348",
			ItemNum = 1,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Birthday2026_08_2,
			Category = Category.Birthday2026_08,
			Kind = Kind.TaikyokuCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_TableSticker255",
			ItemNum = 1,
			IsAutoClearItemPossession = true,
			FirstTerm = 5
		},
		new Item
		{
			MissionID = MissionID.ToukaTicket_Vs,
			Category = Category.ToukaTicket,
			Kind = Kind.PlayCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			LoopType = LoopType.Numerical,
			DisplayType = DisplayType.All,
			ItemOID = "OID_ExchangePoint_Touka",
			ItemNum = 3,
			FirstTerm = 1,
			CommonDifference = 1,
			LastTerm = 10
		},
		new Item
		{
			MissionID = MissionID.Birthday2026_20_1,
			Category = Category.Birthday2026_20,
			Kind = Kind.Login,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_TableSticker357",
			ItemNum = 1,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Birthday2026_03_1,
			Category = Category.Birthday2026_03,
			Kind = Kind.Login,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "OID_TableSticker358",
			ItemNum = 1,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.Birthday2026_03_2,
			Category = Category.Birthday2026_03,
			Kind = Kind.TaikyokuCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Result },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			ItemOID = "OID_TableSticker266",
			ItemNum = 1,
			IsAutoClearItemPossession = true,
			FirstTerm = 5
		},
		new Item
		{
			MissionID = MissionID.RainyDay002_Daily,
			Category = Category.RainyDay002,
			UpdateInterval = UpdateInterval.Daily,
			Kind = Kind.Login,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			LoopType = LoopType.Single,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "RainyDayMedal002",
			ItemNum = 10,
			FirstTerm = 1
		},
		new Item
		{
			MissionID = MissionID.RainyDay002_Login,
			Category = Category.RainyDay002,
			Kind = Kind.PlayCount,
			JudgeLocation = new List<JudgeLocation> { JudgeLocation.Home },
			LoopType = LoopType.Endless,
			DisplayType = DisplayType.Single,
			IsProgressDisplay = false,
			ItemOID = "RainyDayMedal002",
			ItemNum = 1,
			IsJandriRewardEnable = true,
			FirstTerm = 1,
			CommonDifference = 1
		}
	};

	public static string ToObjectID(MissionID missionID)
	{
		return $"OID_{missionID}";
	}

	public static MissionID ToMissionID(string objectID)
	{
		return _items.Find((Item x) => x.ObjectID == objectID)?.MissionID ?? MissionID.None;
	}

	public static List<Item> GetItems(Category category, CharaType chara = (CharaType)(-1))
	{
		IEnumerable<Item> source = _items.Where((Item x) => x.Category == category);
		if (category == Category.Birthday)
		{
			IEnumerable<Category> birthdayCategory = MissionCategoryMaster.ActiveBirthdayMission()?.Select((CategoryItem x) => x.Category);
			IEnumerable<Category> enumerable = birthdayCategory;
			if (enumerable != null && enumerable.Count() > 0)
			{
				source = _items.Where((Item x) => birthdayCategory.Contains(x.Category));
			}
		}
		if (chara >= CharaType.Chara01)
		{
			return (from x in source
				where x.Chara == chara
				orderby x.MissionID
				select x).ToList();
		}
		return source.OrderBy((Item x) => x.MissionID).ToList();
	}

	public static Item GetItem(MissionID missionID)
	{
		return _items.Find((Item x) => x.MissionID == missionID);
	}

	public static List<Item> ActiveItems()
	{
		return _items.Where((Item x) => x.IsEnable).ToList();
	}

	public static List<Item> ItemPossessionJudgeMissions()
	{
		return _items.Where((Item x) => x.IsAutoClearItemPossession).ToList();
	}
}
