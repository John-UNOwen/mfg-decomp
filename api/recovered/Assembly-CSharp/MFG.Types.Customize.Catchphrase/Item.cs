using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.Assets;
using MFG.MasterGameData;
using UnityEngine;

namespace MFG.Types.Customize.Catchphrase;

public class Item
{
	public ItemID ItemID;

	public CharaType CharaType;

	public BaseImageID BaseImageID;

	public string Name;

	public int RequiredFightSpiritNum;

	public GameEventType RequiredGameEventType;

	public ItemID RequiredItemID;

	public bool Available = true;

	public SortRules Sort;

	public static CharaType CharaTypeNone = (CharaType)9999;

	private static LoadableAssetSet<Sprite> _baseImageAssetSet = new LoadableAssetSet<Sprite>();

	public string ObjectID => CatchphraseItemMaster.ToObjectID(ItemID);

	public string DispName => SaniTize.SanitizedString(Name);

	public Category CategoryType
	{
		get
		{
			if (CharaType != CharaTypeNone)
			{
				return Category.Chara;
			}
			return Category.Common;
		}
	}

	public string UnlockConditionText
	{
		get
		{
			bool num = RequiredItemID != ItemID.None;
			bool flag = RequiredFightSpiritNum > 0;
			string text = (num ? CatchphraseItemMaster.GetItem(RequiredItemID).DispName : "");
			string text2 = (flag ? PlayerItemMaster.GetItem(PlayerItemMaster.ToFightSpiritItemID(CharaType)).Name : "");
			if (num)
			{
				if (flag)
				{
					return $"「{text}」を解禁後{text2}×{RequiredFightSpiritNum}で解禁";
				}
				return text + "」を解禁";
			}
			if (flag)
			{
				return $"「{text2}×{RequiredFightSpiritNum}」で解禁";
			}
			return "";
		}
	}

	public string BaseImagePath => AssetPathResolver.Catchphrase.ResolveBaseImage(BaseImageID);

	public async UniTask<Sprite> BaseImage(CancellationToken cancellationToken = default(CancellationToken))
	{
		return await _baseImageAssetSet.Get(BaseImagePath, cancellationToken);
	}

	public static async UniTask ResidentLoad()
	{
		List<string> assetAddresses = (from x in Enum.GetValues(typeof(ItemID)).OfType<ItemID>()
			where x != ItemID.None
			select CatchphraseItemMaster.GetItem(x).BaseImagePath).ToList();
		await _baseImageAssetSet.Load(assetAddresses);
	}
}
