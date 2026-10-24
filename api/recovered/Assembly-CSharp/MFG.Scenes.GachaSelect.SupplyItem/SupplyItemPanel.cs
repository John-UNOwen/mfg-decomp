using System.Collections.Generic;
using MFG.MasterGameData;
using MFG.Types;
using MFG.Types.Customize.CutinItem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MFG.Scenes.GachaSelect.SupplyItem;

public class SupplyItemPanel : MonoBehaviour
{
	[SerializeField]
	private HorizontalLayoutGroup m_layoutGroup;

	[SerializeField]
	private GameObject m_rankImage;

	[SerializeField]
	private Image m_rarityImage;

	[SerializeField]
	private TextMeshProUGUI m_itemNoText;

	[SerializeField]
	private TextMeshProUGUI m_itemNameText;

	[SerializeField]
	private List<Sprite> m_rarirySprite = new List<Sprite>();

	private ItemID m_itemID;

	public void Reflesh(ItemID itemID)
	{
		m_itemNoText.gameObject.SetActive(value: false);
		m_rankImage.SetActive(value: true);
		m_itemID = itemID;
		Item item = CutinItemMaster.GetItem(m_itemID);
		m_itemNameText.text = item.Name ?? "";
		if (m_rarirySprite[(int)item.Rarity] == null)
		{
			m_rarityImage.gameObject.SetActive(value: false);
		}
		else
		{
			m_rarityImage.gameObject.SetActive(value: true);
			m_rarityImage.sprite = m_rarirySprite[(int)item.Rarity];
		}
		m_layoutGroup.SetLayoutHorizontal();
	}

	public void MusicReflesh(string charaName, string oid, int itemNo)
	{
		m_itemNoText.gameObject.SetActive(value: true);
		m_rankImage.SetActive(value: false);
		m_itemNameText.text = charaName + " / " + ItemUtility.ItemName(oid);
		m_itemNoText.text = $"No.{itemNo}";
		m_layoutGroup.SetLayoutHorizontal();
	}
}
