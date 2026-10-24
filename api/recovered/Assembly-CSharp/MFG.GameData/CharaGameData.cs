using System;
using System.Collections.Generic;
using System.Linq;
using MFG.Assets;
using MFG.MasterGameData;
using MFG.Types;
using MFG.Types.Customize.Catchphrase;
using MFG.Types.Customize.Costume;
using MFG.Types.Customize.CustomizeItem;
using MFG.Types.Customize.MatchingBackground;
using UnityEngine;

namespace MFG.GameData;

[Serializable]
public class CharaGameData : GameDataBase, ISerializationCallbackReceiver
{
	[Serializable]
	private class SerializableKeyValuePair<TKey, TValue>
	{
		public TKey _key;

		public TValue _value;

		public SerializableKeyValuePair(TKey key, TValue value)
		{
			_key = key;
			_value = value;
		}
	}

	[NonSerialized]
	public CharaType charaType;

	public int CharaVariantID;

	public Dictionary<MFG.Types.Customize.CustomizeItem.Category, List<MFG.Types.Customize.CustomizeItem.ItemID>> CustomizeItemEquips = new Dictionary<MFG.Types.Customize.CustomizeItem.Category, List<MFG.Types.Customize.CustomizeItem.ItemID>>();

	[SerializeField]
	[HideInInspector]
	private List<SerializableKeyValuePair<string, string>> _customizeItemEquips = new List<SerializableKeyValuePair<string, string>>();

	public List<string> CustomizeItemObjectIDEquipsList = new List<string>();

	public List<StickerEditInfo> StickerEditInfos = new List<StickerEditInfo>();

	public MFG.Types.Customize.Costume.ItemID CostumeItemEquip;

	[NonSerialized]
	public MFG.Types.Customize.Catchphrase.ItemID CatchphraseItemEquip;

	[SerializeField]
	[HideInInspector]
	private string _catchphraseItemEquipOID = "";

	public MFG.Types.Customize.MatchingBackground.ItemID MatchingBGEquip;

	[SerializeField]
	private int m_exp;

	[SerializeField]
	public List<int> m_fans = new List<int>();

	[SerializeField]
	private int m_level = 1;

	public int[] ScrambleMatchRecord = new int[4];

	public List<VoiceInfo> GachaVoiceItems = new List<VoiceInfo>();

	public CharacterProperties Properties => MasterGameDataManager.Character.Properties(charaType);

	public bool IsDebut => GameDataManager.Customize.CostumeItems.Find((CostumeItemInfo a) => a.Master.IsDebutItem && a.Master.CharaType == charaType) != null;

	public bool IsFakeClear => charaType.IsFakeClear(CharaVariantID);

	public int Exp
	{
		get
		{
			return m_exp;
		}
		set
		{
			m_exp = value;
			Level = Master.GetCurrentLevelAndExpBorder(m_exp).currentLevel;
			if (Level >= MaxLevel)
			{
				Level = MaxLevel;
				m_exp = Master.GetTotalExpRequired(MaxLevel);
			}
		}
	}

	public int TotalFan
	{
		get
		{
			int num = 0;
			foreach (int fan in m_fans)
			{
				num += fan;
			}
			return num;
		}
		private set
		{
		}
	}

	public int MaxFan
	{
		get
		{
			if (!IsDebut)
			{
				return 1200;
			}
			return 9999999;
		}
	}

	public int Level
	{
		get
		{
			return m_level;
		}
		private set
		{
			m_level = value;
		}
	}

	public int MaxLevel
	{
		get
		{
			if (!IsDebut)
			{
				return 20;
			}
			return 99999;
		}
	}

	public bool IsLevelMax => Level == MaxLevel;

	public CharacterLevel Master => MasterGameDataManager.Character.CharacterLevel;

	public (int expectedLevel, int expectedTotalExp) ExpectedLevelAndExp(int acquisitionExp)
	{
		(int, int, int) currentLevelAndExpBorder = Master.GetCurrentLevelAndExpBorder(Exp + acquisitionExp);
		(int, int) result = default((int, int));
		if (currentLevelAndExpBorder.Item1 < MaxLevel)
		{
			result.Item1 = currentLevelAndExpBorder.Item1;
			result.Item2 = Exp + acquisitionExp;
		}
		else
		{
			result.Item1 = MaxLevel;
			result.Item2 = Master.GetTotalExpRequired(MaxLevel);
		}
		return result;
	}

	public void OnBeforeSerialize()
	{
		if (CatchphraseItemEquip == MFG.Types.Customize.Catchphrase.ItemID.None)
		{
			CatchphraseItemEquip = CatchphraseItemMaster.GetDefaultEquipItemID();
		}
		_catchphraseItemEquipOID = CatchphraseItemMaster.ToObjectID(CatchphraseItemEquip);
		if (CustomizeItemEquips == null)
		{
			return;
		}
		foreach (MFG.Types.Customize.CustomizeItem.Category value in Enum.GetValues(typeof(MFG.Types.Customize.CustomizeItem.Category)))
		{
			if (!CustomizeItemMaster.IsMultiEquipEnableCategory(value) && !CustomizeItemMaster.IsUpperEquipEnableCategory(value) && CustomizeItemEquips.ContainsKey(value) && CustomizeItemEquips[value].Count() > 1)
			{
				CustomizeItemEquips[value] = new List<MFG.Types.Customize.CustomizeItem.ItemID> { CustomizeItemEquips[value][0] };
			}
		}
		_customizeItemEquips = CustomizeItemEquips.Select((KeyValuePair<MFG.Types.Customize.CustomizeItem.Category, List<MFG.Types.Customize.CustomizeItem.ItemID>> x) => new SerializableKeyValuePair<string, string>(x.Key.ToString(), string.Join(",", x.Value))).ToList();
	}

	public void OnAfterDeserialize()
	{
		CatchphraseItemEquip = CatchphraseItemMaster.ToItemID(_catchphraseItemEquipOID);
		if (CatchphraseItemEquip == MFG.Types.Customize.Catchphrase.ItemID.None)
		{
			Debug.LogWarning("キャッツフレーズ: デフォルトに変更します");
			CatchphraseItemEquip = CatchphraseItemMaster.GetDefaultEquipItemID();
		}
		if (CustomizeItemObjectIDEquipsList.Any())
		{
			CustomizeItemEquips = CustomizeItemEquip_Convert(CustomizeItemObjectIDEquipsList);
			CustomizeItemObjectIDEquipsList.Clear();
		}
		else
		{
			CustomizeItemEquips = _customizeItemEquips.ToDictionary((SerializableKeyValuePair<string, string> x) => Enum.Parse<MFG.Types.Customize.CustomizeItem.Category>(x._key), (SerializableKeyValuePair<string, string> x) => (!string.IsNullOrEmpty(x._value)) ? (from y in x._value.Split(',')
				select Enum.Parse<MFG.Types.Customize.CustomizeItem.ItemID>(y)).ToList() : new List<MFG.Types.Customize.CustomizeItem.ItemID>());
		}
		foreach (MFG.Types.Customize.CustomizeItem.Category value in Enum.GetValues(typeof(MFG.Types.Customize.CustomizeItem.Category)))
		{
			if (!CustomizeItemEquips.ContainsKey(value))
			{
				CustomizeItemEquips.Add(value, new List<MFG.Types.Customize.CustomizeItem.ItemID> { CustomizeItemMaster.GetDefaultEquipItemID(value) });
			}
		}
		if (CustomizeItemEquips[MFG.Types.Customize.CustomizeItem.Category.TableSticker].Contains(MFG.Types.Customize.CustomizeItem.ItemID.TableStickerNone))
		{
			CustomizeItemEquips[MFG.Types.Customize.CustomizeItem.Category.TableSticker].Remove(MFG.Types.Customize.CustomizeItem.ItemID.TableStickerNone);
		}
		if (CustomizeItemEquips[MFG.Types.Customize.CustomizeItem.Category.TableSticker].Count == 1 && StickerEditInfos.Count == 0)
		{
			MFG.Types.Customize.CustomizeItem.ItemID itemID = CustomizeItemEquips[MFG.Types.Customize.CustomizeItem.Category.TableSticker].First();
			StickerEditInfos.Add(new StickerEditInfo(itemID, 0, isDefault: true));
		}
		if (CostumeItemEquip != 0 && CostumeItemMaster.GetItem(CostumeItemEquip).CharaType != charaType)
		{
			CostumeItemEquip = CostumeItemMaster.GetDefaultEquipItemID(charaType);
		}
	}

	public CharaGameData(CharaType _charaType)
	{
		charaType = _charaType;
		m_fans.Clear();
		for (int i = 0; i < Enum.GetNames(typeof(FanType)).Length; i++)
		{
			m_fans.Add(0);
		}
		ResetAllEqipToDefault();
	}

	public void CustomizeItemEquip_ToggleEquip(MFG.Types.Customize.CustomizeItem.Category category, MFG.Types.Customize.CustomizeItem.ItemID itemID)
	{
		List<MFG.Types.Customize.CustomizeItem.ItemID> list = CustomizeItemEquips[category];
		if (list.Any((MFG.Types.Customize.CustomizeItem.ItemID x) => x == itemID))
		{
			list.Remove(itemID);
		}
		else
		{
			list.Add(itemID);
		}
	}

	public void CustomizeItemEquip_ToggleEquip_Sticker(MFG.Types.Customize.CustomizeItem.ItemID itemID)
	{
		int enableStickerCount = GameDataManager.Player.EnableStickerCount;
		List<MFG.Types.Customize.CustomizeItem.ItemID> list = CustomizeItemEquips[MFG.Types.Customize.CustomizeItem.Category.TableSticker];
		if (list.Any((MFG.Types.Customize.CustomizeItem.ItemID x) => x == itemID))
		{
			list.Remove(itemID);
			try
			{
				StickerEditInfo stickerEditInfo = StickerEditInfos.Find((StickerEditInfo x) => x.ItemID == itemID);
				int priority = stickerEditInfo.Priority;
				StickerEditInfos.Remove(stickerEditInfo);
				int i;
				for (i = priority + 1; i <= StickerEditInfos.Count; i++)
				{
					stickerEditInfo = StickerEditInfos.Find((StickerEditInfo x) => x.Priority == i);
					if (stickerEditInfo != null)
					{
						stickerEditInfo.Priority--;
					}
				}
			}
			catch
			{
			}
		}
		else if (list.Count < enableStickerCount)
		{
			list.Add(itemID);
			int count = StickerEditInfos.Count;
			StickerEditInfos.Add(new StickerEditInfo(itemID, count));
		}
		string text = "";
		for (int j = 0; j < StickerEditInfos.Count; j++)
		{
			text += $"{StickerEditInfos[j].ItemID}, {StickerEditInfos[j].Priority} / ";
		}
		Debug.Log(text);
	}

	public void CustomizeItemEquip_SetEquip(MFG.Types.Customize.CustomizeItem.Category category, MFG.Types.Customize.CustomizeItem.ItemID itemID, bool isEquip)
	{
		List<MFG.Types.Customize.CustomizeItem.ItemID> list = CustomizeItemEquips[category];
		if (isEquip)
		{
			if (!list.Exists((MFG.Types.Customize.CustomizeItem.ItemID x) => x == itemID))
			{
				list.Add(itemID);
			}
		}
		else
		{
			list.Remove(itemID);
		}
	}

	public void CustomizeItemEquip_SetEquipAll(MFG.Types.Customize.CustomizeItem.Category category, bool isEquip)
	{
		foreach (CustomizeItemInfo possessionItem in GameDataManager.Customize.CustomizeItems.GetPossessionItems(category, charaType))
		{
			CustomizeItemEquip_SetEquip(category, possessionItem.ItemID, isEquip);
		}
		if (category == MFG.Types.Customize.CustomizeItem.Category.TableSticker && !isEquip)
		{
			StickerEditInfos = new List<StickerEditInfo>();
		}
	}

	public bool CustomizeItemEquip_IsEquip(MFG.Types.Customize.CustomizeItem.Category category, MFG.Types.Customize.CustomizeItem.ItemID itemID)
	{
		return CustomizeItemEquips[category].Exists((MFG.Types.Customize.CustomizeItem.ItemID x) => x == itemID);
	}

	public MFG.Types.Customize.CustomizeItem.ItemID CustomizeItemEquip_GetItemAtRandom(MFG.Types.Customize.CustomizeItem.Category category)
	{
		List<MFG.Types.Customize.CustomizeItem.ItemID> list = CustomizeItemEquips[category];
		if (list.Count == 0)
		{
			List<CustomizeItemInfo> list2 = GameDataManager.Customize.CustomizeItems.Where((CustomizeItemInfo a) => a.Master.Category == category).ToList();
			return list2[UnityEngine.Random.Range(0, list2.Count)].ItemID;
		}
		return list[UnityEngine.Random.Range(0, list.Count)];
	}

	private Dictionary<MFG.Types.Customize.CustomizeItem.Category, List<MFG.Types.Customize.CustomizeItem.ItemID>> CustomizeItemEquip_Convert(List<string> customizeItemObjectIDEquipsList)
	{
		Dictionary<MFG.Types.Customize.CustomizeItem.Category, List<MFG.Types.Customize.CustomizeItem.ItemID>> dictionary = new Dictionary<MFG.Types.Customize.CustomizeItem.Category, List<MFG.Types.Customize.CustomizeItem.ItemID>>();
		if (customizeItemObjectIDEquipsList.Count >= Enum.GetNames(typeof(MFG.Types.Customize.CustomizeItem.Category)).Length - 1)
		{
			dictionary.Clear();
			foreach (MFG.Types.Customize.CustomizeItem.Category value in Enum.GetValues(typeof(MFG.Types.Customize.CustomizeItem.Category)))
			{
				MFG.Types.Customize.CustomizeItem.ItemID itemID = MFG.Types.Customize.CustomizeItem.ItemID.None;
				if ((int)value < CustomizeItemObjectIDEquipsList.Count)
				{
					itemID = CustomizeItemMaster.ToItemID(CustomizeItemObjectIDEquipsList[(int)value]);
				}
				if (value != MFG.Types.Customize.CustomizeItem.Category.TableSticker)
				{
					if (itemID == MFG.Types.Customize.CustomizeItem.ItemID.None)
					{
						itemID = CustomizeItemMaster.GetDefaultEquipItemID(value);
					}
					dictionary.Add(value, new List<MFG.Types.Customize.CustomizeItem.ItemID> { itemID });
				}
				else
				{
					dictionary.Add(value, new List<MFG.Types.Customize.CustomizeItem.ItemID>());
				}
			}
		}
		else
		{
			dictionary = CustomizeItemEquip_GetDefaultEqips();
		}
		return dictionary;
	}

	private Dictionary<MFG.Types.Customize.CustomizeItem.Category, List<MFG.Types.Customize.CustomizeItem.ItemID>> CustomizeItemEquip_GetDefaultEqips()
	{
		Dictionary<MFG.Types.Customize.CustomizeItem.Category, List<MFG.Types.Customize.CustomizeItem.ItemID>> dictionary = new Dictionary<MFG.Types.Customize.CustomizeItem.Category, List<MFG.Types.Customize.CustomizeItem.ItemID>>();
		foreach (MFG.Types.Customize.CustomizeItem.Category value in Enum.GetValues(typeof(MFG.Types.Customize.CustomizeItem.Category)))
		{
			if (value != MFG.Types.Customize.CustomizeItem.Category.TableSticker)
			{
				dictionary.Add(value, new List<MFG.Types.Customize.CustomizeItem.ItemID> { CustomizeItemMaster.GetDefaultEquipItemID(value) });
			}
			else
			{
				dictionary.Add(value, new List<MFG.Types.Customize.CustomizeItem.ItemID>());
			}
		}
		return dictionary;
	}

	public void ResetAllEqipToDefault()
	{
		ResetCustomizeEquipToDefault();
		CostumeItemEquip = CostumeItemMaster.GetDefaultEquipItemID(charaType);
		CatchphraseItemEquip = CatchphraseItemMaster.GetDefaultEquipItemID();
	}

	public void ResetCustomizeEquipToDefault()
	{
		CustomizeItemEquips = CustomizeItemEquip_GetDefaultEqips();
	}

	public CharaGameData Clone()
	{
		return (CharaGameData)MemberwiseClone();
	}
}
