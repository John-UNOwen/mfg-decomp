using System;
using System.Collections.Generic;
using System.Linq;
using MFG.GameData;
using MFG.MasterGameData;
using MFG.Types.Customize.Catchphrase;
using UnityEngine;
using UnityEngine.UI;

public class CatchphraseTest : MonoBehaviour
{
	[SerializeField]
	private GameObject catchphraseDispPrefab;

	private List<CatchphraseDisp> catchphraseDispScripts = new List<CatchphraseDisp>(Enum.GetNames(typeof(ItemID)).Length);

	[SerializeField]
	private Button resetButton;

	[SerializeField]
	private Button allOnButton;

	[SerializeField]
	private Button allOffButton;

	[SerializeField]
	private ScrollView scrollView;

	private void OnDestroy()
	{
		new List<ItemID>();
		Debug.Log("保存");
		foreach (CatchphraseDisp item in catchphraseDispScripts.Where((CatchphraseDisp x) => x.possessing))
		{
			Debug.Log(item.itemID);
			GameDataManager.Customize.CatchphraseItems.Add(new CatchphraseItemInfo
			{
				ItemID = item.itemID
			});
		}
	}

	public void CreateDisp()
	{
		foreach (Item item in CatchphraseItemMaster.GetItemAll())
		{
			GameObject gameObject = scrollView.Add(catchphraseDispPrefab);
			catchphraseDispScripts.Add(gameObject.GetComponent<CatchphraseDisp>());
			catchphraseDispScripts.Last().SetItemID(item.ItemID);
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
		foreach (CatchphraseDisp catchphraseDispScript in catchphraseDispScripts)
		{
			if (CatchphraseItemMaster.GetDefaultItemIDs().Contains(catchphraseDispScript.itemID))
			{
				catchphraseDispScript.SetPossessing(pos: true);
			}
			else
			{
				catchphraseDispScript.SetPossessing(pos: false);
			}
		}
	}

	private void ChangeAllPossessing(bool possessing)
	{
		foreach (CatchphraseDisp catchphraseDispScript in catchphraseDispScripts)
		{
			if (CatchphraseItemMaster.GetDefaultItemIDs().Contains(catchphraseDispScript.itemID))
			{
				catchphraseDispScript.SetPossessing(pos: true);
			}
			else
			{
				catchphraseDispScript.SetPossessing(possessing);
			}
		}
	}
}
