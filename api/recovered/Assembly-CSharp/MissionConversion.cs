using System;
using System.Collections.Generic;
using MFG.GameData;
using MFG.Types.Mission;
using UnityEngine;

public static class MissionConversion
{
	private enum ConvertType
	{
		None,
		Progress,
		IsClear,
		Dan
	}

	private static Dictionary<MissionID, (string oidOID, ConvertType convertType)> ConvertDict = new Dictionary<MissionID, (string, ConvertType)>
	{
		{
			MissionID.Player_Vs4,
			("OID_Mission_Player_001", ConvertType.Progress)
		},
		{
			MissionID.Player_Vs4_Endless,
			("OID_Mission_Player_001", ConvertType.Progress)
		},
		{
			MissionID.Player_Vs3,
			("OID_Mission_Player_002", ConvertType.Progress)
		},
		{
			MissionID.Player_Vs3_Endless,
			("OID_Mission_Player_002", ConvertType.Progress)
		},
		{
			MissionID.Player_Vs2,
			("OID_Mission_Player_003", ConvertType.IsClear)
		},
		{
			MissionID.Player_Top4,
			("OID_Mission_Player_004", ConvertType.Progress)
		},
		{
			MissionID.Player_Top4_Endless,
			("OID_Mission_Player_004", ConvertType.Progress)
		},
		{
			MissionID.Player_Top3,
			("OID_Mission_Player_005", ConvertType.Progress)
		},
		{
			MissionID.Player_Top3_Endless,
			("OID_Mission_Player_005", ConvertType.Progress)
		},
		{
			MissionID.Player_UntilDan10,
			("", ConvertType.Dan)
		},
		{
			MissionID.Player_UpperJansei,
			("", ConvertType.Dan)
		},
		{
			MissionID.Player_Login,
			("OID_Mission_Player_032", ConvertType.IsClear)
		},
		{
			MissionID.Player_Gacha,
			("OID_Mission_Player_026", ConvertType.IsClear)
		},
		{
			MissionID.Player_Gacha5,
			("OID_Mission_Player_027", ConvertType.IsClear)
		},
		{
			MissionID.Player_SpiritGym,
			("OID_Mission_Player_028", ConvertType.IsClear)
		},
		{
			MissionID.Player_CharaChange,
			("OID_Mission_Player_029", ConvertType.IsClear)
		},
		{
			MissionID.Player_SetCutin,
			("OID_Mission_Player_030", ConvertType.IsClear)
		},
		{
			MissionID.Player_SetGoods,
			("OID_Mission_Player_031", ConvertType.IsClear)
		},
		{
			MissionID.Yaku_Mangan,
			("OID_Mission_Yaku_001", ConvertType.Progress)
		},
		{
			MissionID.Yaku_Mangan_Endless,
			("OID_Mission_Yaku_001", ConvertType.Progress)
		},
		{
			MissionID.Yaku_Haneman,
			("OID_Mission_Yaku_002", ConvertType.Progress)
		},
		{
			MissionID.Yaku_Haneman_Endless,
			("OID_Mission_Yaku_002", ConvertType.Progress)
		},
		{
			MissionID.Yaku_Baiman,
			("OID_Mission_Yaku_003", ConvertType.Progress)
		},
		{
			MissionID.Yaku_Baiman_Endless,
			("OID_Mission_Yaku_003", ConvertType.Progress)
		},
		{
			MissionID.Yaku_Sanbaiman,
			("OID_Mission_Yaku_004", ConvertType.Progress)
		},
		{
			MissionID.Yaku_Sanbaiman_Endless,
			("OID_Mission_Yaku_004", ConvertType.Progress)
		},
		{
			MissionID.Yaku_Yakuman,
			("OID_Mission_Yaku_005", ConvertType.Progress)
		},
		{
			MissionID.Yaku_Yakuman_Endless,
			("OID_Mission_Yaku_005", ConvertType.Progress)
		},
		{
			MissionID.Yaku_Yakuman2,
			("OID_Mission_Yaku_006", ConvertType.IsClear)
		},
		{
			MissionID.Yaku_Yakuman3,
			("OID_Mission_Yaku_007", ConvertType.IsClear)
		},
		{
			MissionID.Yaku_Tenho,
			("OID_Mission_Yaku_008", ConvertType.IsClear)
		},
		{
			MissionID.Yaku_Chiho,
			("OID_Mission_Yaku_009", ConvertType.IsClear)
		},
		{
			MissionID.Yaku_Tyuren9,
			("OID_Mission_Yaku_010", ConvertType.IsClear)
		},
		{
			MissionID.Yaku_TyurenTanki,
			("OID_Mission_Yaku_011", ConvertType.IsClear)
		},
		{
			MissionID.Yaku_Chinroto,
			("OID_Mission_Yaku_012", ConvertType.IsClear)
		},
		{
			MissionID.Yaku_Tsuiso,
			("OID_Mission_Yaku_013", ConvertType.IsClear)
		},
		{
			MissionID.Yaku_Ryuiso,
			("OID_Mission_Yaku_014", ConvertType.IsClear)
		},
		{
			MissionID.Yaku_Kokushi13,
			("OID_Mission_Yaku_015", ConvertType.IsClear)
		},
		{
			MissionID.Yaku_KokushiTanki,
			("OID_Mission_Yaku_016", ConvertType.IsClear)
		},
		{
			MissionID.Yaku_Sukantsu,
			("OID_Mission_Yaku_017", ConvertType.IsClear)
		},
		{
			MissionID.Yaku_Suanko,
			("OID_Mission_Yaku_018", ConvertType.IsClear)
		},
		{
			MissionID.Yaku_SuankoTanki,
			("OID_Mission_Yaku_019", ConvertType.IsClear)
		},
		{
			MissionID.Yaku_Daisushi,
			("OID_Mission_Yaku_020", ConvertType.IsClear)
		},
		{
			MissionID.Yaku_Syosushi,
			("OID_Mission_Yaku_021", ConvertType.IsClear)
		},
		{
			MissionID.Yaku_Daisangen,
			("OID_Mission_Yaku_022", ConvertType.IsClear)
		},
		{
			MissionID.Character_01_Favo,
			("OID_Mission_Chara01_001", ConvertType.Progress)
		},
		{
			MissionID.Character_01_Fan01,
			("OID_Mission_Chara01_002", ConvertType.Progress)
		},
		{
			MissionID.Character_01_Fan02,
			("OID_Mission_Chara01_003", ConvertType.Progress)
		},
		{
			MissionID.Character_01_Fan03,
			("OID_Mission_Chara01_004", ConvertType.Progress)
		},
		{
			MissionID.Character_01_Fan04,
			("OID_Mission_Chara01_005", ConvertType.Progress)
		},
		{
			MissionID.Character_01_Debut,
			("OID_Mission_Chara01_006", ConvertType.IsClear)
		},
		{
			MissionID.Character_02_Favo,
			("OID_Mission_Chara02_001", ConvertType.Progress)
		},
		{
			MissionID.Character_02_Fan01,
			("OID_Mission_Chara02_002", ConvertType.Progress)
		},
		{
			MissionID.Character_02_Fan02,
			("OID_Mission_Chara02_003", ConvertType.Progress)
		},
		{
			MissionID.Character_02_Fan03,
			("OID_Mission_Chara02_004", ConvertType.Progress)
		},
		{
			MissionID.Character_02_Fan04,
			("OID_Mission_Chara02_005", ConvertType.Progress)
		},
		{
			MissionID.Character_02_Debut,
			("OID_Mission_Chara02_006", ConvertType.IsClear)
		},
		{
			MissionID.Character_03_Favo,
			("OID_Mission_Chara03_001", ConvertType.Progress)
		},
		{
			MissionID.Character_03_Fan01,
			("OID_Mission_Chara03_002", ConvertType.Progress)
		},
		{
			MissionID.Character_03_Fan02,
			("OID_Mission_Chara03_003", ConvertType.Progress)
		},
		{
			MissionID.Character_03_Fan03,
			("OID_Mission_Chara03_004", ConvertType.Progress)
		},
		{
			MissionID.Character_03_Fan04,
			("OID_Mission_Chara03_005", ConvertType.Progress)
		},
		{
			MissionID.Character_03_Debut,
			("OID_Mission_Chara03_006", ConvertType.IsClear)
		},
		{
			MissionID.Character_04_Favo,
			("OID_Mission_Chara04_001", ConvertType.Progress)
		},
		{
			MissionID.Character_04_Fan01,
			("OID_Mission_Chara04_002", ConvertType.Progress)
		},
		{
			MissionID.Character_04_Fan02,
			("OID_Mission_Chara04_003", ConvertType.Progress)
		},
		{
			MissionID.Character_04_Fan03,
			("OID_Mission_Chara04_004", ConvertType.Progress)
		},
		{
			MissionID.Character_04_Fan04,
			("OID_Mission_Chara04_005", ConvertType.Progress)
		},
		{
			MissionID.Character_04_Debut,
			("OID_Mission_Chara04_006", ConvertType.IsClear)
		},
		{
			MissionID.Character_05_Favo,
			("OID_Mission_Chara05_001", ConvertType.Progress)
		},
		{
			MissionID.Character_05_Fan01,
			("OID_Mission_Chara05_002", ConvertType.Progress)
		},
		{
			MissionID.Character_05_Fan02,
			("OID_Mission_Chara05_003", ConvertType.Progress)
		},
		{
			MissionID.Character_05_Fan03,
			("OID_Mission_Chara05_004", ConvertType.Progress)
		},
		{
			MissionID.Character_05_Fan04,
			("OID_Mission_Chara05_005", ConvertType.Progress)
		},
		{
			MissionID.Character_05_Debut,
			("OID_Mission_Chara05_006", ConvertType.IsClear)
		},
		{
			MissionID.Character_06_Favo,
			("OID_Mission_Chara06_001", ConvertType.Progress)
		},
		{
			MissionID.Character_06_Fan01,
			("OID_Mission_Chara06_002", ConvertType.Progress)
		},
		{
			MissionID.Character_06_Fan02,
			("OID_Mission_Chara06_003", ConvertType.Progress)
		},
		{
			MissionID.Character_06_Fan03,
			("OID_Mission_Chara06_004", ConvertType.Progress)
		},
		{
			MissionID.Character_06_Fan04,
			("OID_Mission_Chara06_005", ConvertType.Progress)
		},
		{
			MissionID.Character_06_Debut,
			("OID_Mission_Chara06_006", ConvertType.IsClear)
		},
		{
			MissionID.Character_07_Favo,
			("OID_Mission_Chara07_001", ConvertType.Progress)
		},
		{
			MissionID.Character_07_Fan01,
			("OID_Mission_Chara07_002", ConvertType.Progress)
		},
		{
			MissionID.Character_07_Fan02,
			("OID_Mission_Chara07_003", ConvertType.Progress)
		},
		{
			MissionID.Character_07_Fan03,
			("OID_Mission_Chara07_004", ConvertType.Progress)
		},
		{
			MissionID.Character_07_Fan04,
			("OID_Mission_Chara07_005", ConvertType.Progress)
		},
		{
			MissionID.Character_07_Debut,
			("OID_Mission_Chara07_006", ConvertType.IsClear)
		},
		{
			MissionID.Character_08_Favo,
			("OID_Mission_Chara08_001", ConvertType.Progress)
		},
		{
			MissionID.Character_08_Fan01,
			("OID_Mission_Chara08_002", ConvertType.Progress)
		},
		{
			MissionID.Character_08_Fan02,
			("OID_Mission_Chara08_003", ConvertType.Progress)
		},
		{
			MissionID.Character_08_Fan03,
			("OID_Mission_Chara08_004", ConvertType.Progress)
		},
		{
			MissionID.Character_08_Fan04,
			("OID_Mission_Chara08_005", ConvertType.Progress)
		},
		{
			MissionID.Character_08_Debut,
			("OID_Mission_Chara08_006", ConvertType.IsClear)
		},
		{
			MissionID.Character_09_Favo,
			("OID_Mission_Chara09_001", ConvertType.Progress)
		},
		{
			MissionID.Character_09_Fan01,
			("OID_Mission_Chara09_002", ConvertType.Progress)
		},
		{
			MissionID.Character_09_Fan02,
			("OID_Mission_Chara09_003", ConvertType.Progress)
		},
		{
			MissionID.Character_09_Fan03,
			("OID_Mission_Chara09_004", ConvertType.Progress)
		},
		{
			MissionID.Character_09_Fan04,
			("OID_Mission_Chara09_005", ConvertType.Progress)
		},
		{
			MissionID.Character_09_Debut,
			("OID_Mission_Chara09_006", ConvertType.IsClear)
		},
		{
			MissionID.Character_10_Favo,
			("OID_Mission_Chara10_001", ConvertType.Progress)
		},
		{
			MissionID.Character_10_Fan01,
			("OID_Mission_Chara10_002", ConvertType.Progress)
		},
		{
			MissionID.Character_10_Fan02,
			("OID_Mission_Chara10_003", ConvertType.Progress)
		},
		{
			MissionID.Character_10_Fan03,
			("OID_Mission_Chara10_004", ConvertType.Progress)
		},
		{
			MissionID.Character_10_Fan04,
			("OID_Mission_Chara10_005", ConvertType.Progress)
		},
		{
			MissionID.Character_10_Debut,
			("OID_Mission_Chara10_006", ConvertType.IsClear)
		},
		{
			MissionID.Character_11_Favo,
			("OID_Mission_Chara11_001", ConvertType.Progress)
		},
		{
			MissionID.Character_11_Fan01,
			("OID_Mission_Chara11_002", ConvertType.Progress)
		},
		{
			MissionID.Character_11_Fan02,
			("OID_Mission_Chara11_003", ConvertType.Progress)
		},
		{
			MissionID.Character_11_Fan03,
			("OID_Mission_Chara11_004", ConvertType.Progress)
		},
		{
			MissionID.Character_11_Fan04,
			("OID_Mission_Chara11_005", ConvertType.Progress)
		},
		{
			MissionID.Character_11_Debut,
			("OID_Mission_Chara11_006", ConvertType.IsClear)
		},
		{
			MissionID.Character_12_Favo,
			("OID_Mission_Chara12_001", ConvertType.Progress)
		},
		{
			MissionID.Character_12_Fan01,
			("OID_Mission_Chara12_002", ConvertType.Progress)
		},
		{
			MissionID.Character_12_Fan02,
			("OID_Mission_Chara12_003", ConvertType.Progress)
		},
		{
			MissionID.Character_12_Fan03,
			("OID_Mission_Chara12_004", ConvertType.Progress)
		},
		{
			MissionID.Character_12_Fan04,
			("OID_Mission_Chara12_005", ConvertType.Progress)
		},
		{
			MissionID.Character_12_Debut,
			("OID_Mission_Chara12_006", ConvertType.IsClear)
		},
		{
			MissionID.Character_13_Favo,
			("OID_Mission_Chara13_001", ConvertType.Progress)
		},
		{
			MissionID.Character_13_Fan01,
			("OID_Mission_Chara13_002", ConvertType.Progress)
		},
		{
			MissionID.Character_13_Fan02,
			("OID_Mission_Chara13_003", ConvertType.Progress)
		},
		{
			MissionID.Character_13_Fan03,
			("OID_Mission_Chara13_004", ConvertType.Progress)
		},
		{
			MissionID.Character_13_Fan04,
			("OID_Mission_Chara13_005", ConvertType.Progress)
		},
		{
			MissionID.Character_13_Debut,
			("OID_Mission_Chara13_006", ConvertType.IsClear)
		},
		{
			MissionID.Weekly_Login,
			("OID_Mission_Weekly_001", ConvertType.IsClear)
		},
		{
			MissionID.Weekly_Vs,
			("OID_Mission_Weekly_004", ConvertType.Progress)
		},
		{
			MissionID.Weekly_FireReach_Vs,
			("OID_Mission_Weekly_005", ConvertType.Progress)
		},
		{
			MissionID.Weekly_FireReach_Top,
			("OID_Mission_Weekly_006", ConvertType.Progress)
		},
		{
			MissionID.Weekly_AccelDora_Vs,
			("OID_Mission_Weekly_007", ConvertType.Progress)
		},
		{
			MissionID.Weekly_AccelDora_Top,
			("OID_Mission_Weekly_008", ConvertType.Progress)
		},
		{
			MissionID.Weekly_Mentanpin_Vs,
			("OID_Mission_Weekly_009", ConvertType.Progress)
		},
		{
			MissionID.Weekly_Mentanpin_Top,
			("OID_Mission_Weekly_010", ConvertType.Progress)
		},
		{
			MissionID.StartDash_Limited,
			("OID_Mission_StartDash_005", ConvertType.Progress)
		},
		{
			MissionID.StartDash_Endless,
			("OID_Mission_StartDash_005", ConvertType.Progress)
		},
		{
			MissionID.ScrambleMatch_School01,
			("OID_Mission_Scramble_003", ConvertType.Progress)
		},
		{
			MissionID.ScrambleMatch_School02,
			("OID_Mission_Scramble_006", ConvertType.Progress)
		},
		{
			MissionID.ScrambleMatch_School03,
			("OID_Mission_Scramble_009", ConvertType.Progress)
		},
		{
			MissionID.ScrambleMatch_School05,
			("OID_Mission_Scramble_015", ConvertType.Progress)
		},
		{
			MissionID.ScrambleMatch_School06,
			("OID_Mission_Scramble_018", ConvertType.Progress)
		},
		{
			MissionID.ScrambleMatch_School07,
			("OID_Mission_Scramble_021", ConvertType.Progress)
		},
		{
			MissionID.ScrambleMatch_School08,
			("OID_Mission_Scramble_024", ConvertType.Progress)
		},
		{
			MissionID.MahjongActor_01,
			("OID_Mission_MahjongActor_001", ConvertType.IsClear)
		},
		{
			MissionID.MahjongActor_02,
			("OID_Mission_MahjongActor_002", ConvertType.IsClear)
		},
		{
			MissionID.MahjongActor_03,
			("OID_Mission_MahjongActor_003", ConvertType.IsClear)
		},
		{
			MissionID.MahjongActor_04,
			("OID_Mission_MahjongActor_004", ConvertType.IsClear)
		},
		{
			MissionID.MahjongActor_05,
			("OID_Mission_MahjongActor_005", ConvertType.IsClear)
		},
		{
			MissionID.MahjongActor_06,
			("OID_Mission_MahjongActor_006", ConvertType.IsClear)
		},
		{
			MissionID.MahjongActor_07,
			("OID_Mission_MahjongActor_007", ConvertType.IsClear)
		},
		{
			MissionID.MahjongActor_08,
			("OID_Mission_MahjongActor_008", ConvertType.IsClear)
		},
		{
			MissionID.MahjongActor_11,
			("OID_Mission_MahjongActor_011", ConvertType.IsClear)
		},
		{
			MissionID.MahjongActor_12,
			("OID_Mission_MahjongActor_012", ConvertType.IsClear)
		},
		{
			MissionID.Mentanpin2_Daily,
			("OID_Mission_Event_285", ConvertType.Progress)
		},
		{
			MissionID.Mentanpin2_Item,
			("OID_Mission_Event_293", ConvertType.Progress)
		},
		{
			MissionID.Mentanpin2_Endless,
			("OID_Mission_Event_293", ConvertType.Progress)
		},
		{
			MissionID.HalfAnniv2024_Login,
			("OID_Mission_Event_294", ConvertType.IsClear)
		}
	};

	public static List<MissionProgressInfo> Convert(List<MissionInfo> oldMissions)
	{
		List<MissionProgressInfo> list = new List<MissionProgressInfo>();
		foreach (MissionID value in Enum.GetValues(typeof(MissionID)))
		{
			Debug.Log($"{value}");
			if (value == MissionID.None)
			{
				continue;
			}
			try
			{
				MissionProgressInfo missionProgressInfo = new MissionProgressInfo(value);
				if (ConvertDict.ContainsKey(value))
				{
					(string, ConvertType) tuple = ConvertDict[value];
					string oidOID = tuple.Item1;
					ConvertType item = tuple.Item2;
					MissionInfo missionInfo = oldMissions.Find((MissionInfo x) => x.ObjectID == oidOID);
					switch (item)
					{
					case ConvertType.Progress:
						if (missionInfo != null)
						{
							missionProgressInfo.Convert(missionInfo?.Progress ?? 0);
						}
						break;
					case ConvertType.IsClear:
						if (missionInfo != null)
						{
							missionProgressInfo.Convert((missionInfo != null && missionInfo.IsClear) ? 1 : 0);
						}
						break;
					case ConvertType.Dan:
					{
						int i;
						for (i = 25; i >= 6; i--)
						{
							MissionInfo missionInfo2 = oldMissions.Find((MissionInfo x) => x.ObjectID == "OID_Mission_Player_0" + i.ToString("D2"));
							if (i == 25 && missionInfo2.LoopCount > 0)
							{
								missionProgressInfo.Convert(missionInfo2.LoopCount + 20);
								break;
							}
							if (i < 25 && missionInfo2.IsClear)
							{
								missionProgressInfo.Convert(i - 4);
								break;
							}
						}
						break;
					}
					}
				}
				list.Add(missionProgressInfo);
				Debug.Log($"{value} /");
			}
			catch
			{
				Debug.Log($"Convert Faild : {value}");
			}
		}
		return list;
	}
}
