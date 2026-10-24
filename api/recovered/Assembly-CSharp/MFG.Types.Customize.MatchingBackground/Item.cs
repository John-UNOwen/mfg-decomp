using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.Assets;
using MFG.MasterGameData;
using MFG.Types.Customize.CutinItem;
using UnityEngine;

namespace MFG.Types.Customize.MatchingBackground;

public class Item
{
	public ItemID ItemID;

	public CharaType CharaType;

	public string Name;

	public bool Available = true;

	public Rarity Rarity = Rarity.Nothing;

	public UnlockType UnlockType;

	public Category Category;

	public bool IsDefaultEquip;

	public MFG.Types.Customize.CutinItem.ItemID UnlockingCutinID;

	private static LoadableAssetSet<Sprite> _iconGraphicAssetSet = new LoadableAssetSet<Sprite>();

	public string ObjectID => MatchingBackgroundMaster.ToObjectID(ItemID);

	public string IconGraphicPath
	{
		get
		{
			CharaType charaType = CharaType;
			string objectID = ObjectID;
			int length = objectID.Length;
			int num = length - 3;
			return AssetPathResolver.MatchingBG.Resolve(charaType, objectID.Substring(num, length - num));
		}
	}

	public string BGPath
	{
		get
		{
			CharaType charaType = CharaType;
			string objectID = ObjectID;
			int length = objectID.Length;
			int num = length - 3;
			return AssetPathResolver.MatchingBG.ResolveBG(charaType, objectID.Substring(num, length - num));
		}
	}

	public async UniTask<Sprite> IconGraphic(CancellationToken cancellationToken = default(CancellationToken))
	{
		return await _iconGraphicAssetSet.Get(IconGraphicPath, cancellationToken);
	}

	public static async UniTask ResidentLoad()
	{
		List<string> assetAddresses = (from x in Enum.GetValues(typeof(ItemID)).OfType<ItemID>()
			where x != ItemID.None
			select MatchingBackgroundMaster.GetItem(x).IconGraphicPath).ToList();
		await _iconGraphicAssetSet.Load(assetAddresses);
	}
}
