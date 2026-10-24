using System;
using System.Collections.Generic;
using System.Linq;
using MFG.GameData;
using MFG.MasterGameData;
using MFG.Types.Customize.CustomizeItem;
using UnityEngine;
using UnityEngine.UI;

public class CustomizeItemTest : MonoBehaviour
{
	[SerializeField]
	private GameObject customizeItemDispPrefab;

	private Category _category;

	private List<CustomizeItemDisp> customizeItemDispScripts = new List<CustomizeItemDisp>(Enum.GetNames(typeof(ItemID)).Length);

	[SerializeField]
	private Button resetButton;

	[SerializeField]
	private Button allOnButton;

	[SerializeField]
	private Button allOffButton;

	[SerializeField]
	private ScrollView scrollView;

	private void OnDestroy()
	{
		new List<ItemID>();
		Debug.Log("保存");
		foreach (CustomizeItemDisp item in customizeItemDispScripts.Where((CustomizeItemDisp x) => x.possessing))
		{
			Debug.Log(item.itemID);
			GameDataManager.Customize.CustomizeItems.Add(new CustomizeItemInfo
			{
				ItemID = item.itemID
			});
		}
	}

	public void SetCategory(Category category)
	{
		_category = category;
	}

	public void CreateDisp()
	{
		foreach (Item item in CustomizeItemMaster.GetItems(delegate(Item x)
		{
			if (x.Category != _category)
			{
				return false;
			}
			Category category = x.Category;
			ItemID itemID = x.ItemID;
			switch (category)
			{
			case Category.PlayerAccessory:
				if (itemID == ItemID.PlayerAccessoryNone)
				{
					return false;
				}
				break;
			case Category.TableSticker:
				if (itemID == ItemID.TableStickerNone)
				{
					return false;
				}
				break;
			}
			return true;
		}))
		{
			GameObject gameObject = scrollView.Add(customizeItemDispPrefab);
			customizeItemDispScripts.Add(gameObject.GetComponent<CustomizeItemDisp>());
			customizeItemDispScripts.Last().SetItemID(item.ItemID);
		}
		resetButton.onClick.AddListener(delegate
		{
			ResetPossessing();
		});
		allOnButton.onClick.AddListener(delegate
		{
			ChangeAllPossessing(possessing: true);
		});
		allOffButton.onClick.AddListener(delegate
		{
			ChangeAllPossessing(possessing: false);
		});
	}

	private void ResetPossessing()
	{
		foreach (CustomizeItemDisp customizeItemDispScript in customizeItemDispScripts)
		{
			if (CustomizeItemMaster.GetDefaultItemIDs().Contains(customizeItemDispScript.itemID))
			{
				customizeItemDispScript.SetPossessing(pos: true);
			}
			else
			{
				customizeItemDispScript.SetPossessing(pos: false);
			}
		}
	}

	private void ChangeAllPossessing(bool possessing)
	{
		foreach (CustomizeItemDisp customizeItemDispScript in customizeItemDispScripts)
		{
			customizeItemDispScript.SetPossessing(possessing);
		}
	}
}
