using System;
using System.Collections.Generic;
using MFG.Scenes.Home;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayRuleWindowProperties", menuName = "PlayRuleWindowProperties")]
public class PlayRuleWindowProperties : ScriptableObject
{
	[Serializable]
	public class Item
	{
		public PlayRuleType PlayRuleType;

		public string Name;

		public Sprite Rule;

		public List<Sprite> Yakus;
	}

	public List<Item> Items;

	public Item GetItem(PlayRuleType playRuleType)
	{
		return Items.Find((Item x) => x.PlayRuleType == playRuleType);
	}
}
