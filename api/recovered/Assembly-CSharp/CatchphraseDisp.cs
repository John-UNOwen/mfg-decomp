using MFG.GameData;
using MFG.MasterGameData;
using MFG.Types.Customize.Catchphrase;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CatchphraseDisp : MonoBehaviour
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
		itemOnOffButton.transform.Find("Text").gameObject.GetComponent<TextMeshProUGUI>().text = CatchphraseItemMaster.GetItem(itemID).DispName + "：" + (possessing ? "オン" : "オフ");
	}

	private void ChangePossession()
	{
		if (!CatchphraseItemMaster.GetDefaultItemIDs().Contains(itemID))
		{
			possessing = !possessing;
			SetButtonColor();
		}
	}

	public async void SetItemID(ItemID id)
	{
		itemID = id;
		CatchphraseItemInfo catchphraseItemInfo = GameDataManager.Customize.CatchphraseItems.Find((CatchphraseItemInfo x) => x.ItemID == itemID);
		if (catchphraseItemInfo != null)
		{
			SetPossessing(pos: true);
			GameDataManager.Customize.CatchphraseItems.Remove(catchphraseItemInfo);
		}
		else
		{
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
