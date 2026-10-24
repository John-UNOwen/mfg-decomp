using System;
using Cysharp.Threading.Tasks;
using MFG.GameData;
using MFG.MasterGameData;
using MFG.Types.Customize.CutinItem;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace MFG.Scenes.GachaSelect.CutinItemExchange;

public class ItemPanel : MonoBehaviour
{
	[NonSerialized]
	public ItemID m_itemID;

	[NonSerialized]
	public bool m_isReGet;

	[SerializeField]
	private ItemIcon m_itemIcon;

	[SerializeField]
	private TextMeshProUGUI m_itemNameText;

	[SerializeField]
	private TextMeshProUGUI m_requiredExchangePointText;

	[SerializeField]
	private TextMeshProUGUI m_bonusFightSpiritText;

	[SerializeField]
	private Button m_exchangeButton;

	[SerializeField]
	private GameObject m_alreadyAcquiredCautionGameObject;

	private GachaSeriesServerInfo m_gachaSeriesInfo;

	private bool m_isInitialized;

	public IObservable<Unit> OnClickExchangeButton => m_exchangeButton.OnClickAsObservable();

	public async UniTask Init(GachaSeriesServerInfo gachaSeriesInfo, ItemID itemID)
	{
		m_gachaSeriesInfo = gachaSeriesInfo;
		if (!m_isInitialized)
		{
			m_itemID = itemID;
			Item item = CutinItemMaster.GetItem(m_itemID);
			m_isReGet = GameDataManager.Customize.CutinItems.Exists((CutinItemInfo x) => x.ItemID == m_itemID) && (item.Rarity == Rarity.UR || item.Rarity == Rarity.SR);
			m_itemIcon.Reflesh(m_itemID).Forget();
			m_itemNameText.text = item.Name ?? "";
			m_requiredExchangePointText.text = $"{Item.GetRequiredExchangePoint(item.Rarity)}";
			m_isInitialized = true;
		}
		Reflesh();
	}

	public void Reflesh()
	{
		Item item = CutinItemMaster.GetItem(m_itemID);
		int requiredExchangePoint = Item.GetRequiredExchangePoint(item.Rarity);
		bool flag = GameDataManager.Customize.CutinItems.Exists((CutinItemInfo x) => x.ItemID == m_itemID);
		int exchangePoint = m_gachaSeriesInfo.GetExchangePoint();
		bool flag2 = item.Rarity == Rarity.UR || item.Rarity == Rarity.SR;
		bool flag3 = exchangePoint < requiredExchangePoint;
		m_exchangeButton.interactable = (flag2 ? (!flag3) : (!flag3 && !flag));
		m_alreadyAcquiredCautionGameObject.SetActive(flag);
	}
}
