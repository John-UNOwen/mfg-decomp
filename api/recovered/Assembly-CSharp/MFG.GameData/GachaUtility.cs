using System;
using System.Collections.Generic;
using System.Linq;
using MFG.MasterGameData;
using MFG.Types;
using MFG.Types.Customize.CutinItem;

namespace MFG.GameData;

public static class GachaUtility
{
	[NonSerialized]
	public static readonly Dictionary<CharaType, ItemID> CharaUnlockItemIDDict = new Dictionary<CharaType, ItemID>
	{
		{
			CharaType.Chara12,
			ItemID.Item12AgariSR01
		},
		{
			CharaType.Chara13,
			ItemID.Item13AgariSR01
		},
		{
			CharaType.Chara14,
			ItemID.Item14AgariSR01
		},
		{
			CharaType.Chara15,
			ItemID.Item15AgariSR01
		},
		{
			CharaType.Chara16,
			ItemID.Item16AgariSR01
		},
		{
			CharaType.Chara17,
			ItemID.Item17AgariSR01
		},
		{
			CharaType.Chara18,
			ItemID.Item18AgariSR01
		},
		{
			CharaType.Chara19,
			ItemID.Item19AgariSR01
		},
		{
			CharaType.Chara20,
			ItemID.Item20AgariSR01
		},
		{
			CharaType.Chara21,
			ItemID.Item21AgariSR01
		}
	};

	public static List<ItemID> SortItemID(this List<ItemID> info)
	{
		return (from itemID in info
			orderby CutinItemMaster.GetItem(itemID).Rarity descending, CutinItemMaster.GetItem(itemID).CharaType, CutinItemMaster.GetItem(itemID).Category descending, itemID
			select itemID).ToList();
	}

	public static List<ItemID> SortItemIDUnlockGacha(this List<ItemID> info)
	{
		return info.OrderByDescending((ItemID x) => CharaUnlockItemIDDict.Values.Contains(x)).ThenByDescending((ItemID itemID) => CutinItemMaster.GetItem(itemID).Rarity).ThenBy((ItemID itemID) => CutinItemMaster.GetItem(itemID).CharaType)
			.ThenByDescending((ItemID itemID) => CutinItemMaster.GetItem(itemID).Category)
			.ThenBy((ItemID itemID) => itemID)
			.ToList();
	}

	public static CharaType GetGachaUnlockCharaType(ItemID id)
	{
		return CharaUnlockItemIDDict.FirstOrDefault((KeyValuePair<CharaType, ItemID> x) => x.Value == id).Key;
	}
}
