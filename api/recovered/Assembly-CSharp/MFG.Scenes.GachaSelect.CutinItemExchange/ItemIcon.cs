using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MFG.Assets;
using MFG.MasterGameData;
using MFG.Types.Customize.CutinItem;
using UnityEngine;
using UnityEngine.UI;

namespace MFG.Scenes.GachaSelect.CutinItemExchange;

public class ItemIcon : MonoBehaviour, IDisposable
{
	[SerializeField]
	private Image m_iconImage;

	[SerializeField]
	private Image m_rarityImage;

	[SerializeField]
	private List<Sprite> m_rarirySprite = new List<Sprite>();

	private ItemID m_itemID;

	private IDisposableAsset<Sprite> m_iconSprite;

	public void Dispose()
	{
		if (m_iconSprite != null)
		{
			m_iconSprite.Dispose();
			m_iconSprite = null;
		}
	}

	private void OnDestroy()
	{
		Dispose();
	}

	public async UniTask Reflesh(ItemID itemID, bool isDispState = false)
	{
		Item item = CutinItemMaster.GetItem(itemID);
		await RefleshIconGraphic(itemID);
		if (m_rarirySprite[(int)item.Rarity] == null)
		{
			m_rarityImage.gameObject.SetActive(value: false);
			return;
		}
		m_rarityImage.gameObject.SetActive(value: true);
		m_rarityImage.sprite = m_rarirySprite[(int)item.Rarity];
	}

	private async UniTask RefleshIconGraphic(ItemID itemID)
	{
		if (m_itemID != itemID)
		{
			m_iconSprite?.Dispose();
			m_iconSprite = await AssetLoader.LoadAssetAsync<Sprite>(CutinItemMaster.GetItem(itemID).IconGraphicPath);
			m_iconImage.sprite = m_iconSprite.Asset;
			m_itemID = itemID;
		}
	}

	private void Start()
	{
	}
}
