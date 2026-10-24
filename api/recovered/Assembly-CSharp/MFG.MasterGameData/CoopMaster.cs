using System;
using System.Collections.Generic;
using MFG.Types;
using MFG.Types.Coop;

namespace MFG.MasterGameData;

public class CoopMaster : MasterGameDataBase
{
	private static Dictionary<CoopID, List<string>> BoughtOIDMaster = new Dictionary<CoopID, List<string>> { 
	{
		CoopID.CoopCutinItem001,
		new List<string> { "OID_10AgariR01", "OID_10AgariSR01", "OID_10AgariSR03", "OID_10NakiN01", "OID_10NakiN02", "OID_10NakiN03", "OID_10ReachR01", "OID_10ReachR02" }
	} };

	private static List<Coop> _coops = new List<Coop>
	{
		new Coop
		{
			ObjectID = "OID_CoopNewCharacter001",
			CoopID = CoopID.CoopNewCharacter001,
			Name = "藤崎詩織利用権",
			Category = Category.NewCharacter,
			CharaType = CharaType.Chara10,
			UnlockEventType = GameEventType.ShioriAppearance
		},
		new Coop
		{
			ObjectID = "OID_CoopCutinCoop001",
			CoopID = CoopID.CoopCutinItem001,
			Name = "藤崎詩織カットインセット追加",
			Category = Category.CutinItem,
			CharaType = CharaType.Chara10,
			BoughtOIDs = BoughtOIDMaster[CoopID.CoopCutinItem001],
			UnlockEventType = GameEventType.ShioriAppearance
		},
		new Coop
		{
			ObjectID = "OID_CoopJanDri000",
			CoopID = CoopID.JanDri000,
			Name = "雀ドリ",
			Category = Category.UseItem
		}
	};

	public static bool IsCoopCharacter(CharaType charaType)
	{
		return new List<CharaType> { CharaType.Chara10 }.Contains(charaType);
	}

	public static Coop GetCoop(CoopID coopID)
	{
		return _coops.Find((Coop x) => x.CoopID == coopID);
	}

	public static List<Coop> GetCoops(Predicate<Coop> match)
	{
		return _coops.FindAll(match);
	}

	public static CoopID GetCoopID(string oid)
	{
		return _coops.Find((Coop x) => x.ObjectID == oid)?.CoopID ?? CoopID.None;
	}
}
