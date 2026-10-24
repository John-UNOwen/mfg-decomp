using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.Assets;
using MFG.GameData;
using MFG.MasterGameData;
using MFG.Types.Customize.CutinItem;
using UnityEngine;

namespace MFG.Types.Customize.Costume;

public class Item
{
	public string ObjectID;

	public ItemID ItemID;

	public CharaType CharaType;

	public string Name;

	public bool Available = true;

	public int UnlockFightSpiritNum;

	public int MitigationUnlockFightSpiritNum;

	public GameEventType UnlockEventType;

	public ItemID needUnlockItemID;

	public List<MFG.Types.Customize.CutinItem.ItemID> mitigationCutinItemIDs;

	public SortID SortID;

	private static LoadableAssetSet<Sprite> _costumeIconAssetSet = new LoadableAssetSet<Sprite>();

	public bool IsDebutItem => ItemID == CostumeItemMaster.m_debutCostumeItems[CharaType];

	public string IconGraphicPath => AssetPathResolver.Resolve(PathType.CharaSelect, CharaType, CharaSelectAssetType.CostumeIcon, ItemID);

	public bool IsUnlocked()
	{
		if (needUnlockItemID != 0 && GameDataManager.Customize.CostumeItems.Find((CostumeItemInfo a) => a.ItemID == needUnlockItemID) == null)
		{
			return false;
		}
		if (UnlockEventType != 0)
		{
			return IsDispEnable();
		}
		return true;
	}

	public bool IsDispEnable()
	{
		if (UnlockEventType != 0 && !GameDataManager.System.GameControlInfo.EventStatus.IsActive(UnlockEventType))
		{
			return false;
		}
		return true;
	}

	public async UniTask<Sprite> IconGraphic(CancellationToken cancellationToken = default(CancellationToken))
	{
		return await _costumeIconAssetSet.Get(IconGraphicPath, cancellationToken);
	}

	public static async UniTask ResidentLoad()
	{
		List<string> assetAddresses = (from x in Enum.GetValues(typeof(ItemID)).OfType<ItemID>()
			where x != ItemID.None
			select CostumeItemMaster.GetItem(x).IconGraphicPath).ToList();
		await _costumeIconAssetSet.Load(assetAddresses);
	}
}
