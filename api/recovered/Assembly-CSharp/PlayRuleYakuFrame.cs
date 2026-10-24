using System.Collections.Generic;
using System.Linq;
using MFG.Types;
using MFG.Types.PlayRuleMaster;
using TMPro;
using UnityEngine;

public class PlayRuleYakuFrame : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI indexText;

	[SerializeField]
	private TextMeshProUGUI yakuText;

	[SerializeField]
	private List<GameObject> contentSizes;

	public void Set(Han han, Item item, Item baseItem)
	{
		TextMeshProUGUI textMeshProUGUI = indexText;
		textMeshProUGUI.text = han switch
		{
			Han.Han1 => "1翻役", 
			Han.Han2 => "2翻役", 
			Han.Han3 => "3翻役", 
			Han.Haneman => "6翻役", 
			Han.Yakuman => "役満", 
			Han.Yakuman2 => "ダブル役満", 
			_ => "", 
		};
		IEnumerable<Yaku> baseYaku = from x in PlayRuleUtility.YakuNames
			where x.Key.han == han
			select x.Key.yaku;
		baseYaku = (from x in baseYaku
			where !baseItem.UnableYaku.Contains(x) || item.EnableYaku.Contains(x)
			where !item.UnableYaku.Contains(x)
			where !item.ChangeYaku.Select(((Yaku, Han) x) => x.Item1).Contains(x)
			select x).Concat(from x in item.ChangeYaku
			where x.Item2 == han
			select x.Item1);
		Dictionary<Yaku, string> dictionary = PlayRuleUtility.YakuNames.Where((KeyValuePair<(Han han, Yaku yaku), string> x) => baseYaku.Contains(x.Key.yaku)).ToDictionary((KeyValuePair<(Han han, Yaku yaku), string> x) => x.Key.yaku, (KeyValuePair<(Han han, Yaku yaku), string> x) => x.Value);
		yakuText.text = "";
		foreach (Yaku key in dictionary.Keys)
		{
			string text = dictionary[key];
			if (!PlayRuleUtility.YakuNames.ContainsKey((han, key)))
			{
				text = "<color=red>" + text + "</color>";
			}
			yakuText.text += text;
			if (dictionary.Keys.Last() != key)
			{
				yakuText.text += "\n";
			}
		}
		RefreshLayout();
	}

	public void RefreshLayout()
	{
		foreach (GameObject contentSize in contentSizes)
		{
			PlayRuleUtility.RefreshLayout(contentSize);
		}
	}
}
