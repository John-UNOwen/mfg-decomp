using System.Collections.Generic;
using System.Linq;
using MFG.GameData;
using MFG.MasterGameData;
using MFG.Types;
using MFG.Types.Customize.CustomizeItem;
using MFG.Types.Customize.CutinItem;
using UnityEngine;

namespace MFG.MahjongPlay.Cutin;

public class PlaybackContentDecider
{
	public RandomValueGenerator m_random = new RandomValueGenerator();

	public RandomValueGenerator m_randomForReach = new RandomValueGenerator();

	public CutinItemInfo DecideItem(CutinType cutinType, CharaType charaType, List<CutinItemInfo> cutinItems, bool isOverMangan)
	{
		MFG.Types.Customize.CutinItem.Category category = ToCategory(cutinType, isOverMangan);
		int num = cutinItems.Count((CutinItemInfo x) => x.Master.CharaType == charaType && x.Master.Category == MFG.Types.Customize.CutinItem.Category.AgariBig);
		int num2 = cutinItems.Count((CutinItemInfo x) => x.Master.CharaType == charaType && x.Master.Category == MFG.Types.Customize.CutinItem.Category.AgariSmall);
		if (category == MFG.Types.Customize.CutinItem.Category.AgariBig && num == 0)
		{
			category = MFG.Types.Customize.CutinItem.Category.AgariSmall;
		}
		else if (category == MFG.Types.Customize.CutinItem.Category.AgariSmall && num2 == 0)
		{
			category = MFG.Types.Customize.CutinItem.Category.AgariBig;
		}
		List<CutinItemInfo> list = (from x in cutinItems
			where x.IsOn && x.Master.CharaType == charaType && x.Master.Category == category
			orderby x.ItemID
			select x).ToList();
		if (list.Count() > 0)
		{
			return list[m_random.Range(0, list.Count)];
		}
		List<CutinItemInfo> list2 = (from x in cutinItems
			where !x.IsOn && x.Master.CharaType == charaType && x.Master.Category == category
			orderby x.ItemID
			select x).ToList();
		if (list2.Count() > 0)
		{
			return list2[m_random.Range(0, list2.Count)];
		}
		if (category == MFG.Types.Customize.CutinItem.Category.AgariBig)
		{
			category = MFG.Types.Customize.CutinItem.Category.AgariSmall;
		}
		MFG.Types.Customize.CutinItem.ItemID itemID = (from x in CutinItemMaster.GetItems((MFG.Types.Customize.CutinItem.Item x) => x.Rarity != 0 && !x.IsComical && x.Category == category && x.CharaType == charaType)
			orderby x.Rarity, x.ItemID
			select x.ItemID).FirstOrDefault();
		Debug.LogWarning($"PlaybackContentDecider: 条件に該当するカットインが見つかりませんでした。同カテゴリ内でレアリティが一番低い＆コミカルではない＆ItemIDが若いものを返します {cutinType} {charaType} {itemID}");
		return new CutinItemInfo
		{
			ItemID = itemID,
			IsOn = true
		};
	}

	public CustomizeItemInfo DecideThrownReachStick(Dictionary<MFG.Types.Customize.CustomizeItem.Category, List<MFG.Types.Customize.CustomizeItem.ItemID>> customizeItemEquips)
	{
		customizeItemEquips.TryGetValue(MFG.Types.Customize.CustomizeItem.Category.ReachStick, out var value);
		if (value == null || value.Count == 0)
		{
			return new CustomizeItemInfo
			{
				ItemID = CustomizeItemMaster.GetDefaultEquipItemID(MFG.Types.Customize.CustomizeItem.Category.ReachStick)
			};
		}
		return new CustomizeItemInfo
		{
			ItemID = value[m_randomForReach.Range(0, value.Count)]
		};
	}

	public CustomizeItemInfo DecideReachBgm(Dictionary<MFG.Types.Customize.CustomizeItem.Category, List<MFG.Types.Customize.CustomizeItem.ItemID>> customizeItemEquips)
	{
		customizeItemEquips.TryGetValue(MFG.Types.Customize.CustomizeItem.Category.ReachBgm, out var value);
		if (value == null || value.Count == 0)
		{
			return new CustomizeItemInfo
			{
				ItemID = CustomizeItemMaster.GetDefaultEquipItemID(MFG.Types.Customize.CustomizeItem.Category.ReachBgm)
			};
		}
		return new CustomizeItemInfo
		{
			ItemID = value[m_randomForReach.Range(0, value.Count)]
		};
	}

	private MFG.Types.Customize.CutinItem.Category ToCategory(CutinType cutinType, bool isOverMangan)
	{
		switch (cutinType)
		{
		case CutinType.Pon:
		case CutinType.Chi:
		case CutinType.Kan:
			return MFG.Types.Customize.CutinItem.Category.Naki;
		case CutinType.Reach:
			return MFG.Types.Customize.CutinItem.Category.Reach;
		case CutinType.Tsumo:
		case CutinType.Ron:
			if (isOverMangan)
			{
				return MFG.Types.Customize.CutinItem.Category.AgariBig;
			}
			return MFG.Types.Customize.CutinItem.Category.AgariSmall;
		default:
			return MFG.Types.Customize.CutinItem.Category.None;
		}
	}
}
