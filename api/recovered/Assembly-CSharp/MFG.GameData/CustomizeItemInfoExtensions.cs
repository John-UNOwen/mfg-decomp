using System.Collections.Generic;
using System.Linq;
using MFG.Types;
using MFG.Types.Customize.CustomizeItem;

namespace MFG.GameData;

public static class CustomizeItemInfoExtensions
{
	public static IEnumerable<CustomizeItemInfo> GetPossessionItems(this List<CustomizeItemInfo> customizeItemInfos, Category category, CharaType charaType)
	{
		return customizeItemInfos.Where((CustomizeItemInfo a) => a.Master.Category == category && category switch
		{
			Category.ReachBgm => a.Master.ItemID == ItemID.ReachBgm001 || a.Master.charaType == charaType, 
			Category.TableSticker => a.Master.ItemID != ItemID.TableStickerNone, 
			_ => true, 
		});
	}

	public static int GetPossessionItemCount(this List<CustomizeItemInfo> customizeItemInfos, Category category, CharaType charaType)
	{
		return customizeItemInfos.GetPossessionItems(category, charaType).Count();
	}
}
