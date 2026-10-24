using System;
using System.Collections.Generic;
using System.Linq;
using MFG.GameData;
using MFG.MasterGameData;
using MFG.Types;
using MFG.Types.Customize.CutinItem;
using MFG.Types.Customize.MatchingBackground;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CutinItemTest : MonoBehaviour
{
	[SerializeField]
	private GameObject cutinItemDispPrefab;

	private CharaType chara;

	[SerializeField]
	private TextMeshProUGUI charaName;

	private List<CutinItemDisp> cutinItemDispScripts = new List<CutinItemDisp>(Enum.GetNames(typeof(MFG.Types.Customize.CutinItem.ItemID)).Length);

	[SerializeField]
	private Button resetButton;

	[SerializeField]
	private Button allOnButton;

	[SerializeField]
	private Button allOffButton;

	[SerializeField]
	private ScrollView scrollView;

	private void Start()
	{
	}

	private void Update()
	{
	}

	private void OnDestroy()
	{
		new List<MFG.Types.Customize.CutinItem.ItemID>();
		Debug.Log("保存");
		foreach (CutinItemDisp items in cutinItemDispScripts.Where((CutinItemDisp x) => x.possessing))
		{
			Debug.Log(items.itemID);
			GameDataManager.Customize.CutinItems.Add(new CutinItemInfo
			{
				ItemID = items.itemID
			});
			MFG.Types.Customize.MatchingBackground.Item matchinBGItem = MatchingBackgroundMaster.GetGachaCutinItems().Find((MFG.Types.Customize.MatchingBackground.Item x) => x.UnlockingCutinID == items.itemID);
			if (matchinBGItem != null && !GameDataManager.Customize.MatchingBGItems.Exists((MatchingBGInfo x) => x.ItemID == matchinBGItem.ItemID))
			{
				GameDataManager.Customize.MatchingBGItems.Add(new MatchingBGInfo
				{
					ItemID = matchinBGItem.ItemID
				});
			}
		}
	}

	public void SetChara(CharaType charaType)
	{
		chara = charaType;
		charaName.text = MasterGameDataManager.Character.Properties(charaType).Profile.ShortName;
	}

	public void CreateDisp()
	{
		List<MFG.Types.Customize.CutinItem.Item> items = CutinItemMaster.GetItems((MFG.Types.Customize.CutinItem.Item x) => x.CharaType == chara);
		int num = 0;
		foreach (MFG.Types.Customize.CutinItem.Item item in items)
		{
			GameObject gameObject = scrollView.Add(cutinItemDispPrefab);
			cutinItemDispScripts.Add(gameObject.GetComponent<CutinItemDisp>());
			cutinItemDispScripts.Last().SetItemID(item.ItemID);
			num++;
		}
		resetButton.onClick.AddListener(delegate
		{
			ResetPossessing();
		});
		allOnButton.onClick.AddListener(delegate
		{
			ChangeAllPossessing(possessing: true);
		});
		allOffButton.onClick.AddListener(delegate
		{
			ChangeAllPossessing(possessing: false);
		});
	}

	private void ResetPossessing()
	{
		foreach (CutinItemDisp cutinItemDispScript in cutinItemDispScripts)
		{
			if (CutinItemMaster.GetDefaultItemIDs().Contains(cutinItemDispScript.itemID))
			{
				cutinItemDispScript.SetPossessing(pos: true);
			}
			else
			{
				cutinItemDispScript.SetPossessing(pos: false);
			}
		}
	}

	private void ChangeAllPossessing(bool possessing)
	{
		foreach (CutinItemDisp cutinItemDispScript in cutinItemDispScripts)
		{
			cutinItemDispScript.SetPossessing(possessing);
		}
	}
}
