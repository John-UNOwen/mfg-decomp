using MFG.GameData;
using MFG.MasterGameData;
using MFG.Types;
using MFG.Types.Customize.Voice;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GachaVoiceItemDisp : MonoBehaviour
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
		itemOnOffButton.transform.Find("Text").gameObject.GetComponent<TextMeshProUGUI>().text = "No." + VoiceMaster.GetItem(itemID).GachaItemNo + "  " + VoiceMaster.GetItem(itemID).Name + "：" + (possessing ? "オン" : "オフ");
	}

	private void ChangePossession()
	{
		possessing = !possessing;
		SetButtonColor();
	}

	public void SetItemID(CharaType chara, ItemID id)
	{
		itemID = id;
		foreach (VoiceInfo gachaVoiceItem in GameDataManager.Player.CharaData[(int)chara].GachaVoiceItems)
		{
			if (itemID == gachaVoiceItem.ItemID)
			{
				SetPossessing(pos: true);
				GameDataManager.Player.CharaData[(int)chara].GachaVoiceItems.Remove(gachaVoiceItem);
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
