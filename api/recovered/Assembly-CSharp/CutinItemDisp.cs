using MFG.GameData;
using MFG.MasterGameData;
using MFG.Types.Customize.CutinItem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CutinItemDisp : MonoBehaviour
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
		itemOnOffButton.transform.Find("Text").gameObject.GetComponent<TextMeshProUGUI>().text = CutinItemMaster.GetItem(itemID).ShortName + "：" + (possessing ? "オン" : "オフ");
	}

	private void ChangePossession()
	{
		possessing = !possessing;
		SetButtonColor();
	}

	public async void SetItemID(ItemID id)
	{
		itemID = id;
		foreach (CutinItemInfo cutinItem in GameDataManager.Customize.CutinItems)
		{
			if (itemID == cutinItem.ItemID)
			{
				SetPossessing(pos: true);
				GameDataManager.Customize.CutinItems.Remove(cutinItem);
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
