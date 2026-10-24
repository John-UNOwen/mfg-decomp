using MFG.GameData;
using MFG.MasterGameData;
using MFG.Types.Customize.CustomizeItem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CustomizeItemDisp : MonoBehaviour
{
	public ItemID itemID;

	public bool possessing;

	[SerializeField]
	private Button itemOnOffButton;

	private void Start()
	{
		itemOnOffButton.onClick.AddListener(delegate
		{
			ChangePossession();
		});
	}

	private void Update()
	{
		itemOnOffButton.transform.Find("Text").gameObject.GetComponent<TextMeshProUGUI>().text = CustomizeItemMaster.GetItem(itemID).DispName + "：" + (possessing ? "オン" : "オフ");
	}

	private void ChangePossession()
	{
		possessing = !possessing;
		SetButtonColor();
	}

	public async void SetItemID(ItemID id)
	{
		itemID = id;
		foreach (CustomizeItemInfo customizeItem in GameDataManager.Customize.CustomizeItems)
		{
			if (itemID == customizeItem.ItemID)
			{
				SetPossessing(pos: true);
				GameDataManager.Customize.CustomizeItems.Remove(customizeItem);
				break;
			}
			SetPossessing(pos: false);
		}
	}

	public void SetPossessing(bool pos)
	{
		possessing = pos;
		SetButtonColor();
	}

	private void SetButtonColor()
	{
		itemOnOffButton.GetComponent<Image>().color = (possessing ? new Color(1f, 1f, 1f, 1f) : new Color(0.7f, 0.7f, 0.7f, 1f));
	}
}
