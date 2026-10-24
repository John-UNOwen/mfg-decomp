using System;
using Cysharp.Threading.Tasks;
using MFG.Assets;
using MFG.MasterGameData;
using MFG.Types.Customize.CustomizeItem;
using UnityEngine;
using UnityEngine.UI;

public class StampIcon : MonoBehaviour, IDisposable
{
	[SerializeField]
	private Image _iconImage;

	private IDisposableAsset<Sprite> _iconSprite;

	private ItemID _itemID;

	private void OnDestroy()
	{
		Dispose();
	}

	public void Dispose()
	{
		if (_iconSprite != null)
		{
			_iconSprite.Dispose();
			_iconImage.enabled = false;
			_iconSprite = null;
		}
	}

	public async UniTask Reflesh(ItemID itemID)
	{
		await RefleshIconGraphic(itemID);
	}

	private async UniTask RefleshIconGraphic(ItemID itemID)
	{
		if (_itemID != itemID)
		{
			_iconSprite?.Dispose();
			Item item = CustomizeItemMaster.GetItem(itemID);
			Image iconImage = _iconImage;
			iconImage.sprite = await item.IconGraphic();
			_iconImage.enabled = true;
			_itemID = itemID;
		}
	}
}
