using MFG.MasterGameData;
using MFG.Types.PlayerPaidItem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PaidItemDisp : MonoBehaviour
{
	private ItemID item;

	[SerializeField]
	private TextMeshProUGUI paidItemText;

	[SerializeField]
	private TextMeshProUGUI countText;

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

	[SerializeField]
	private Button addButton;

	[SerializeField]
	private Button add30Button;

	private int counter;

	private void Start()
	{
		up1Button.onClick.AddListener(delegate
		{
			ChangePaidItem(1);
		});
		down1Button.onClick.AddListener(delegate
		{
			ChangePaidItem(-1);
		});
		up10Button.onClick.AddListener(delegate
		{
			ChangePaidItem(10);
		});
		down10Button.onClick.AddListener(delegate
		{
			ChangePaidItem(-10);
		});
		up100Button.onClick.AddListener(delegate
		{
			ChangePaidItem(100);
		});
		down100Button.onClick.AddListener(delegate
		{
			ChangePaidItem(-100);
		});
		addButton.onClick.AddListener(delegate
		{
			AddItem(1);
		});
		add30Button.onClick.AddListener(delegate
		{
			AddItem(30);
		});
	}

	private void Update()
	{
	}

	private void ChangePaidItem(int change)
	{
		counter += change;
		counter = ((counter >= 0) ? counter : 0);
		countText.text = $"{counter}";
	}

	private void AddItem(int days)
	{
		if (counter > 0)
		{
			GameDataManager.PlayerPaidItem.Add(item, counter, DateTimeOffsetManager.GetJSTTime().AddDays(days).ToString());
			counter = 0;
			countText.text = $"{counter}";
		}
	}

	public void SetItemID(ItemID itemID)
	{
		item = itemID;
		paidItemText.text = PlayerPaidItemMaster.GetItem(item).Name;
		counter = 0;
		countText.text = $"{counter}";
	}
}
