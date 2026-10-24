using Cysharp.Threading.Tasks;
using MFG.MasterGameData;
using MFG.Types;
using MFG.Types.PlayerItem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FightSpiritDisp : MonoBehaviour
{
	private ItemID item;

	[SerializeField]
	private TextMeshProUGUI fightSpiritText;

	[SerializeField]
	private Button up1Button;

	[SerializeField]
	private Button down1Button;

	[SerializeField]
	private Button up10Button;

	[SerializeField]
	private Button down10Button;

	[SerializeField]
	private Button up100Button;

	[SerializeField]
	private Button down100Button;

	private void Start()
	{
		up1Button.onClick.AddListener(delegate
		{
			ChangeFightSpirit(1);
		});
		down1Button.onClick.AddListener(delegate
		{
			ChangeFightSpirit(-1);
		});
		up10Button.onClick.AddListener(delegate
		{
			ChangeFightSpirit(10);
		});
		down10Button.onClick.AddListener(delegate
		{
			ChangeFightSpirit(-10);
		});
		up100Button.onClick.AddListener(delegate
		{
			ChangeFightSpirit(100);
		});
		down100Button.onClick.AddListener(delegate
		{
			ChangeFightSpirit(-100);
		});
	}

	private void Update()
	{
	}

	private void ChangeFightSpirit(int change)
	{
		if (change > 0)
		{
			GameDataManager.PlayerItem.Add(item, change);
		}
		else if (change < 0)
		{
			GameDataManager.PlayerItem.Consume(item, change * -1, ItemConsumeType.None).Forget();
		}
		DispFightSpirit();
	}

	public async void SetItemID(ItemID itemID)
	{
		item = itemID;
		DispFightSpirit();
	}

	private void DispFightSpirit()
	{
		fightSpiritText.text = PlayerItemMaster.GetItem(item).Name + "：" + GameDataManager.PlayerItem.Count(item);
	}
}
