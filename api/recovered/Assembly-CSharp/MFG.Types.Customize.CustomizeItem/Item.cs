using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.Assets;
using MFG.MasterGameData;
using UnityEngine;

namespace MFG.Types.Customize.CustomizeItem;

public class Item
{
	public string ObjectID;

	public ItemID ItemID;

	public string Name;

	public Category Category;

	public bool IsDetachable;

	public bool Available = true;

	public CharaType charaType = (CharaType)(-1);

	public SortType Sort;

	public bool IsBigSize;

	private static LoadableAssetSet<Sprite> _iconGraphicAssetSet = new LoadableAssetSet<Sprite>();

	public string DispName => SaniTize.SanitizedString(Name);

	public string IconGraphicPath => AssetPathResolver.ResolveCustomizeIcon(ItemID);

	public string MahjongPlayDataPath => AssetPathResolver.ResolveCustomizeItem(ItemID, Category);

	public async UniTask<Sprite> IconGraphic(CancellationToken cancellationToken = default(CancellationToken))
	{
		return await _iconGraphicAssetSet.Get(IconGraphicPath, cancellationToken);
	}

	public static string RandomIconGraphicPath(Category category)
	{
		return AssetPathResolver.ResolveCustomizeRandomIcon(category);
	}

	public static async UniTask<Sprite> RandomIconGraphic(Category category, CancellationToken cancellationToken = default(CancellationToken))
	{
		return await _iconGraphicAssetSet.Get(RandomIconGraphicPath(category), cancellationToken);
	}

	public static async UniTask ResidentLoad()
	{
		List<string> list = (from x in Enum.GetValues(typeof(ItemID)).OfType<ItemID>()
			where x != ItemID.None
			select CustomizeItemMaster.GetItem(x).IconGraphicPath).ToList();
		list.Add(AssetPathResolver.ResolveCustomizeRandomIcon(Category.ReachStick));
		list.Add(AssetPathResolver.ResolveCustomizeRandomIcon(Category.ReachBgm));
		await _iconGraphicAssetSet.Load(list);
	}
}
