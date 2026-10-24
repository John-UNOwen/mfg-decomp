using System.Collections.Generic;
using MFG.MasterGameData;
using MFG.Types.Customize.CutinItem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SimulationItemPanel : MonoBehaviour
{
	[SerializeField]
	private Image m_rarityImage;

	[SerializeField]
	private TextMeshProUGUI m_itemNameText;

	[SerializeField]
	private List<Sprite> m_rarirySprite = new List<Sprite>();

	private ItemID m_itemID;

	public Rarity SetGachaSimulationItem(ItemID itemID, int count)
	{
		m_itemID = itemID;
		Item item = CutinItemMaster.GetItem(m_itemID);
		m_itemNameText.text = $"{item.Name}  /  {count}";
		if (m_rarirySprite[(int)item.Rarity] == null)
		{
			m_rarityImage.gameObject.SetActive(value: false);
		}
		else
		{
			m_rarityImage.gameObject.SetActive(value: true);
			m_rarityImage.sprite = m_rarirySprite[(int)item.Rarity];
		}
		return item.Rarity;
	}
}
