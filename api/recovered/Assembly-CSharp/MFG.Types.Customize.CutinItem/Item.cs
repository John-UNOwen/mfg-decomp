using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.Assets;
using MFG.MasterGameData;
using UnityEngine;

namespace MFG.Types.Customize.CutinItem;

public class Item
{
	public string ObjectID;

	public ItemID ItemID;

	public string ShortName;

	public Category Category;

	public int SubCategory;

	public CharaType CharaType;

	public Rarity Rarity;

	public bool IsComical;

	public int Priority;

	public bool Available = true;

	public bool IsMovie;

	private static LoadableAssetSet<Sprite> _iconGraphicAssetSet = new LoadableAssetSet<Sprite>();

	public string Name => MasterGameDataManager.Character.Properties(CharaType).Profile.ShortName + " / " + ReplacedShortName;

	public string ReplacedShortName => SaniTize.SanitizedString(ShortName);

	public bool IsDetachable => true;

	public CutinAssetType CutinAssetType => ToCutinAssetType(Category);

	public string IconGraphicPath => AssetPathResolver.Resolve(PathType.CutinIcon, CharaType, ItemID);

	public string CutInPrefabPath => AssetPathResolver.Resolve(PathType.CutinItem, CharaType, ToCutinAssetType(Category), this);

	public string MemorialPrintGraphicPath => AssetPathResolver.Resolve(PathType.CardConnectNotice, CharaType, ItemID);

	public async UniTask<Sprite> IconGraphic(CancellationToken cancellationToken = default(CancellationToken))
	{
		return await _iconGraphicAssetSet.Get(IconGraphicPath, cancellationToken);
	}

	public static CutinAssetType ToCutinAssetType(Category category)
	{
		switch (category)
		{
		case Category.AgariSmall:
		case Category.AgariBig:
			return CutinAssetType.AgariPrefab;
		case Category.Naki:
			return CutinAssetType.PonPrefab;
		case Category.Reach:
			return CutinAssetType.ReachPrefab;
		default:
			return CutinAssetType.PonPrefab;
		}
	}

	public static int GetRequiredExchangePoint(Rarity rarity)
	{
		return 100;
	}

	public static async UniTask ResidentLoad()
	{
		List<string> assetAddresses = (from x in Enum.GetValues(typeof(ItemID)).OfType<ItemID>()
			where x != ItemID.None
			select CutinItemMaster.GetItem(x).IconGraphicPath).ToList();
		await _iconGraphicAssetSet.Load(assetAddresses);
	}
}
