using System;
using System.Collections.Generic;
using System.Linq;
using MFG.GameData;
using MFG.MasterGameData;
using MFG.Types;
using MFG.Types.Customize.Voice;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GachaVoiceItemTest : MonoBehaviour
{
	[SerializeField]
	private GameObject voiceItemDispPrefab;

	private CharaType chara;

	[SerializeField]
	private TextMeshProUGUI charaName;

	private List<GachaVoiceItemDisp> voiceItemDispScripts = new List<GachaVoiceItemDisp>(Enum.GetNames(typeof(ItemID)).Length);

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
		new List<ItemID>();
		Debug.Log("保存");
		foreach (GachaVoiceItemDisp item in voiceItemDispScripts.Where((GachaVoiceItemDisp x) => x.possessing))
		{
			Debug.Log(item.itemID);
			GameDataManager.Player.CharaData[(int)chara].GachaVoiceItems.Add(new VoiceInfo
			{
				ItemID = item.itemID
			});
		}
	}

	public void SetChara(CharaType charaType)
	{
		chara = charaType;
		charaName.text = MasterGameDataManager.Character.Properties(charaType).Profile.ShortName;
	}

	public void CreateDisp()
	{
		IOrderedEnumerable<Item> orderedEnumerable = from x in VoiceMaster.GetItems((Item x) => x.ObtainType == ObtainType.Gacha)
			orderby x.GachaItemNo
			select x;
		int num = 0;
		foreach (Item item in orderedEnumerable)
		{
			GameObject gameObject = scrollView.Add(voiceItemDispPrefab);
			voiceItemDispScripts.Add(gameObject.GetComponent<GachaVoiceItemDisp>());
			voiceItemDispScripts.Last().SetItemID(chara, item.ItemID);
			num++;
		}
		allOnButton.onClick.AddListener(delegate
		{
			ChangeAllPossessing(possessing: true);
		});
		allOffButton.onClick.AddListener(delegate
		{
			ChangeAllPossessing(possessing: false);
		});
	}

	private void ChangeAllPossessing(bool possessing)
	{
		foreach (GachaVoiceItemDisp voiceItemDispScript in voiceItemDispScripts)
		{
			voiceItemDispScript.SetPossessing(possessing);
		}
	}
}
