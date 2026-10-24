using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.GameData;
using MFG.MasterGameData;
using MFG.Types;
using MFG.Types.Customize.Costume;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class MyRoomCostumeButton : MonoBehaviour
{
	private enum MessageType
	{
		None,
		Shortage,
		AfterDebut,
		AfterUnlock
	}

	[SerializeField]
	private Button btDress;

	[SerializeField]
	private Image dressImage;

	[SerializeField]
	private Image dressNameFrame;

	[SerializeField]
	private TextMeshProUGUI dressNameText;

	[SerializeField]
	private GameObject needSpiritNumFrame;

	[SerializeField]
	private Image needSpiritIcon;

	[SerializeField]
	private TextMeshProUGUI needSpiritNum;

	[SerializeField]
	private GameObject debutGuide;

	[SerializeField]
	private Button btMitigation;

	[SerializeField]
	private Button btDebut;

	[SerializeField]
	private TextMeshProUGUI debutText;

	[SerializeField]
	private GameObject shortageMessage;

	[SerializeField]
	private GameObject afterDebutMessage;

	[SerializeField]
	private GameObject afterUnlockMessage;

	[SerializeField]
	private TextMeshProUGUI afterUnlockMessageText;

	private CharaGameData currentChara;

	[HideInInspector]
	public ItemID ItemID;

	private Animator btDressAnim => btDress.transform.GetComponent<Animator>();

	[HideInInspector]
	public CharaType CharaType => currentChara.charaType;

	public async UniTask Create(MyRoomSpiritWindow myRoomSpiritWindow, Item costume, Sprite spiritIcon, CancellationToken cancellationToken)
	{
		currentChara = GameDataManager.Player.GetCurrentChara();
		ItemID = costume.ItemID;
		bool isDebutItem = costume.IsDebutItem;
		BtSetAlpha(isActive: false);
		Image image = dressImage;
		image.sprite = await costume.IconGraphic(cancellationToken);
		dressNameText.text = costume.Name;
		BtSetAlpha(isActive: true);
		if (GameDataManager.Customize.CostumeItems.Find((CostumeItemInfo a) => a.ItemID == GameUtility.CostumeUtility.CostumeConversion(ItemID, getDefault: true)) != null)
		{
			btDress.interactable = true;
			btDress.OnClickAsObservable().Subscribe(async delegate
			{
				if (currentChara.CostumeItemEquip != ItemID)
				{
					await myRoomSpiritWindow.CharaModelChange(this, ItemID, cancellationToken, isSEPlay: true);
				}
			}).AddTo(this);
			if (GameUtility.CostumeUtility.CostumeConversion(currentChara.CostumeItemEquip, getDefault: true) == ItemID)
			{
				btDressAnim.SetTrigger("Selected");
			}
			else
			{
				btDressAnim.SetTrigger("Normal");
			}
			needSpiritNumFrame.SetActive(value: false);
			debutGuide.SetActive(value: false);
			btDebut.gameObject.SetActive(value: false);
			SetMessage(MessageType.None);
			return;
		}
		btDress.interactable = false;
		btDressAnim.SetTrigger("Disabled");
		needSpiritNumFrame.SetActive(value: true);
		needSpiritIcon.sprite = spiritIcon;
		(int, GachaSeriesName) tuple = CostumeItemMaster.NeedSpiritWithIsMitigation(ItemID);
		var (num, _) = tuple;
		if (tuple.Item2 >= GachaSeriesName.Normal || DebugFlag.IsActive(DebugFlag.Flag.CharaSelectMitigationDisp))
		{
			btMitigation.gameObject.SetActive(value: true);
			btMitigation.OnClickAsObservable().Subscribe(async delegate
			{
				await myRoomSpiritWindow.MitigationWindowOpen(ItemID, cancellationToken);
			}).AddTo(this);
		}
		else
		{
			btMitigation.gameObject.SetActive(value: false);
		}
		needSpiritNum.text = $"{num}";
		debutGuide.SetActive(isDebutItem);
		bool flag = false;
		if (GameDataManager.PlayerItem.GetUsableSpiritCount(CharaType) >= num)
		{
			if (isDebutItem)
			{
				flag = true;
			}
			else if (currentChara.IsDebut)
			{
				flag = costume.needUnlockItemID == ItemID.None || GameDataManager.Customize.CostumeItems.Exists((CostumeItemInfo a) => a.ItemID == costume.needUnlockItemID);
			}
		}
		if (flag)
		{
			btDebut.gameObject.SetActive(value: true);
			SetMessage(MessageType.None);
			debutText.text = (isDebutItem ? "デビューさせる！！" : "GETする！");
			btDebut.OnClickAsObservable().Subscribe(delegate
			{
				SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
				myRoomSpiritWindow.PayWindowOpen(this);
			}).AddTo(this);
			return;
		}
		btDebut.gameObject.SetActive(value: false);
		SetMessage(MessageType.Shortage);
		if (!currentChara.IsDebut && !isDebutItem)
		{
			SetMessage(MessageType.AfterDebut);
		}
		if (costume.needUnlockItemID != 0 && !GameDataManager.Customize.CostumeItems.Exists((CostumeItemInfo a) => a.ItemID == costume.needUnlockItemID))
		{
			SetMessage(MessageType.AfterUnlock, costume.needUnlockItemID);
		}
	}

	private void SetMessage(MessageType type, ItemID needUnlockItem = ItemID.None)
	{
		shortageMessage.SetActive(type == MessageType.Shortage);
		afterDebutMessage.SetActive(type == MessageType.AfterDebut);
		afterUnlockMessage.SetActive(type == MessageType.AfterUnlock);
		if (type == MessageType.AfterUnlock)
		{
			afterUnlockMessageText.text = CostumeItemMaster.GetItem(needUnlockItem).Name + "\n解禁後に解禁可能";
		}
	}

	private void BtSetAlpha(bool isActive)
	{
		float alpha = (isActive ? 1f : 0f);
		btDressAnim.enabled = isActive;
		dressImage.SetAlpha(alpha);
		dressNameFrame.SetAlpha(alpha);
		dressNameText.SetAlpha(alpha);
	}

	public void BtDressAnimatorSetTrigger(string trigger, bool isCheckInteractable = false)
	{
		if (!isCheckInteractable || btDress.interactable)
		{
			btDressAnim.SetTrigger(trigger);
		}
	}
}
