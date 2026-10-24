using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.GameData;
using MFG.MasterGameData;
using MFG.Types;
using MFG.Types.Customize.CustomizeItem;
using UnityEngine;
using UnityEngine.UI;

public static class MyRoomUtility
{
	private static Color BtOffTextColor = new Color(0.549f, 0.51f, 0.463f);

	private static Color BtOnTextColor = new Color(0.3f, 0.3f, 0.3f);

	public static Color BtTextColor(bool isActive)
	{
		if (!isActive)
		{
			return BtOffTextColor;
		}
		return BtOnTextColor;
	}

	public static string SanitizedNameString(string itemName)
	{
		return SaniTize.SanitizedString(itemName);
	}

	public static async UniTask<(string itemName, Sprite iconSprite)> GetItemNameAndIconSpriteAtCategory(Category category, CharaType charaType, List<ItemID> equipedItems, CancellationToken cancellationToken = default(CancellationToken))
	{
		int possessionItemCount = GameDataManager.Customize.CustomizeItems.GetPossessionItemCount(category, charaType);
		Sprite iconSprite = null;
		string itemName;
		if (equipedItems.Count == 1)
		{
			Item item = CustomizeItemMaster.GetItem(equipedItems[0]);
			itemName = item.DispName ?? "";
			iconSprite = await item.IconGraphic(cancellationToken);
		}
		else if (category == Category.TableSticker)
		{
			if (equipedItems.Count == 0)
			{
				itemName = "なし";
				Sprite sprite = await CustomizeItemMaster.GetItem(ItemID.TableStickerNone).IconGraphic(cancellationToken);
				if (sprite != null)
				{
					iconSprite = sprite;
				}
			}
			else
			{
				itemName = $"({equipedItems.Count}種)";
				Sprite sprite2 = await Item.RandomIconGraphic(category, cancellationToken);
				if (sprite2 != null)
				{
					iconSprite = sprite2;
				}
			}
		}
		else if (equipedItems.Count == possessionItemCount || equipedItems.Count == 0)
		{
			itemName = "ランダム(全種)";
			Sprite sprite3 = await Item.RandomIconGraphic(category, cancellationToken);
			if (sprite3 != null)
			{
				iconSprite = sprite3;
			}
		}
		else
		{
			itemName = $"ランダム({equipedItems.Count}種)";
			Sprite sprite4 = await Item.RandomIconGraphic(category, cancellationToken);
			if (sprite4 != null)
			{
				iconSprite = sprite4;
			}
		}
		return (SanitizedNameString(itemName), iconSprite);
	}

	public static void SetGauge(Transform gauge, int exp, int expBefore, int next)
	{
		if (next != 0)
		{
			gauge.Find("Mask").Find("Bar").transform.localPosition = new Vector3(-400f + 400f * (float)exp / (float)next, 0f, 0f);
			gauge.Find("Mask2").Find("Bar2").transform.localPosition = new Vector3(-400f + 400f * (float)expBefore / (float)next, 0f, 0f);
			gauge.Find("Mask3").gameObject.SetActive(value: false);
			gauge.Find("GaugeNum").GetComponent<Text>().text = exp + "/" + next;
		}
		else
		{
			gauge.Find("Mask").Find("Bar").transform.localPosition = new Vector3(-400f, 0f, 0f);
			gauge.Find("Mask2").Find("Bar2").transform.localPosition = new Vector3(-400f, 0f, 0f);
			gauge.Find("Mask3").gameObject.SetActive(value: false);
			gauge.Find("GaugeNum").GetComponent<Text>().text = "-/-";
		}
	}
}
