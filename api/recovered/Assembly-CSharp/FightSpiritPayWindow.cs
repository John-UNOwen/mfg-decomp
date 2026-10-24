using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.GameData;
using MFG.GameRequest;
using MFG.MasterGameData;
using MFG.Types;
using MFG.Types.Customize.Costume;
using MFG.Types.PlayerItem;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class FightSpiritPayWindow : MonoBehaviour
{
	[SerializeField]
	private Button payWindowConsumeButton;

	[SerializeField]
	private Button payWindowCancelButton;

	[SerializeField]
	private Image iconSpirit;

	[SerializeField]
	private TextMeshProUGUI spiritNum;

	[SerializeField]
	private TextMeshProUGUI spiritWNum;

	private MyRoomCostumeButton button;

	private bool isOpen;

	private CharaType chara => button.CharaType;

	private MFG.Types.Customize.Costume.ItemID selectingItemID => button.ItemID;

	public void PayWindowInit(MyRoomSpiritWindow spiritWindow, CancellationToken cancellationToken)
	{
		payWindowConsumeButton.OnClickAsObservable().Subscribe(async delegate
		{
			SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
			base.gameObject.SetActive(value: false);
			UnlockCostume();
			await spiritWindow.Debut(button, cancellationToken);
			isOpen = false;
		}).AddTo(this);
		payWindowCancelButton.OnClickAsObservable().Subscribe(delegate
		{
			SoundManager.PlaySe("SFX_SYS_CANCEL1").Forget();
			base.gameObject.SetActive(value: false);
			isOpen = false;
		}).AddTo(this);
	}

	public void WindowOpen(MyRoomCostumeButton bt, Sprite icon)
	{
		if (!isOpen)
		{
			isOpen = true;
			button = bt;
			int totalConsumeNum = CostumeItemMaster.NeedUnlockFightSpiritNum(selectingItemID);
			var (num, num2) = GetConsumeSpiritNum(totalConsumeNum);
			iconSpirit.sprite = icon;
			spiritNum.text = num.ToString("N0");
			spiritWNum.text = num2.ToString("N0");
			base.gameObject.SetActive(value: true);
		}
	}

	private void UnlockCostume()
	{
		MFG.Types.PlayerItem.ItemID itemID = PlayerItemMaster.ToFightSpiritItemID(chara);
		var (num, flag) = CostumeItemMaster.NeedUnlockSpiritIsMitigated(selectingItemID);
		var (num2, num3) = GetConsumeSpiritNum(num);
		if (num2 > 0)
		{
			GameDataManager.PlayerItem.Consume(itemID, num2, ItemConsumeType.ExchangeCostume).Forget();
		}
		if (num3 > 0)
		{
			GameDataManager.PlayerItem.Consume(MFG.Types.PlayerItem.ItemID.FightSpiritW, num3, ItemConsumeType.ExchangeCostume).Forget();
		}
		GameDataManager.Customize.CostumeItems.Add(new CostumeItemInfo
		{
			ItemID = selectingItemID
		});
		if (GameDataManager.IsItemGainLogEnable())
		{
			ItemGainType item = ((!flag) ? ItemGainType.FightSpiritConsume : ItemGainType.FightSpiritConsumeMitigation);
			RequestApiBase<ItemGainLog, ItemGainLog.ResultData, (string, ItemGainType, int)>.Send((CostumeItemMaster.GetItem(selectingItemID).ObjectID, item, num), enableErrorScreenTransition: false).Forget();
		}
		if (!CostumeItemMaster.m_sameTimeUnlockItems.ContainsKey(selectingItemID))
		{
			return;
		}
		foreach (MFG.Types.Customize.Costume.ItemID item2 in CostumeItemMaster.m_sameTimeUnlockItems[selectingItemID])
		{
			if (GameDataManager.System.GameControlInfo.EventStatus.IsActive(CostumeItemMaster.GetItem(item2).UnlockEventType))
			{
				GameDataManager.Customize.CostumeItems.Add(new CostumeItemInfo
				{
					ItemID = item2
				});
			}
		}
	}

	private (int charaSpiritNum, int wildSpiritNum) GetConsumeSpiritNum(int totalConsumeNum)
	{
		MFG.Types.PlayerItem.ItemID itemID = PlayerItemMaster.ToFightSpiritItemID(chara);
		int num = GameDataManager.PlayerItem.Count(itemID);
		int item = ((num >= totalConsumeNum) ? totalConsumeNum : num);
		int item2 = ((num < totalConsumeNum) ? (totalConsumeNum - num) : 0);
		return (charaSpiritNum: item, wildSpiritNum: item2);
	}
}
