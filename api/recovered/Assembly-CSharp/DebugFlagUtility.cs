using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public static class DebugFlagUtility
{
	public class FoldoutGroup
	{
		public string Name = "";

		public List<DebugFlag.Flag> Items = new List<DebugFlag.Flag>();

		public bool IsOpen;

		public bool UseFoldout;

		private Dictionary<DebugFlag.Flag, GameObject> _itemDic = new Dictionary<DebugFlag.Flag, GameObject>();

		private GameObject _folderObj;

		public void OnGUI()
		{
			if (Items.Count == 0)
			{
				return;
			}
			if (UseFoldout)
			{
				if (GUILayout.Button((IsOpen ? "▼ " : "▶ ") + Name, GUILayout.Width(300f)))
				{
					IsOpen = !IsOpen;
				}
				if (!IsOpen)
				{
					return;
				}
				using (new GUILayout.HorizontalScope())
				{
					GUILayout.Space(10f);
					using (new GUILayout.VerticalScope())
					{
						foreach (DebugFlag.Flag item in Items)
						{
							DebugFlag.SetActive(item, GUILayout.Toggle(DebugFlag.IsActive(item), item.GetEnumName().Split(new char[1] { '/' }, 2)[1]));
						}
						return;
					}
				}
			}
			foreach (DebugFlag.Flag item2 in Items)
			{
				DebugFlag.SetActive(item2, GUILayout.Toggle(DebugFlag.IsActive(item2), item2.GetEnumName()));
			}
		}

		public void CreateView(GameObject panelPrefab, GameObject folderPrefab, GameObject parent)
		{
			_itemDic.Clear();
			if (Items.Count == 0)
			{
				return;
			}
			if (UseFoldout)
			{
				GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate((UnityEngine.Object)folderPrefab, parent.transform, instantiateInWorldSpace: false);
				gameObject.GetComponent<Button>().OnClickAsObservable().Subscribe(delegate
				{
					IsOpen = !IsOpen;
					Reflesh();
				})
					.AddTo(gameObject);
				_folderObj = gameObject;
			}
			foreach (DebugFlag.Flag item in Items)
			{
				DebugFlag.Flag itemLocal = item;
				GameObject gameObject2 = (GameObject)UnityEngine.Object.Instantiate((UnityEngine.Object)panelPrefab, parent.transform, instantiateInWorldSpace: false);
				Toggle toggle = gameObject2.GetComponentInChildren<Toggle>(includeInactive: true);
				toggle.isOn = DebugFlag.IsActive(item);
				toggle.GetComponentInChildren<Text>(includeInactive: true).text = (UseFoldout ? item.GetEnumName().Split(new char[1] { '/' }, 2)[1] : item.GetEnumName());
				_itemDic.Add(item, gameObject2);
				toggle.OnValueChangedAsObservable().Subscribe(delegate(bool isOn)
				{
					DebugFlag.SetActive(itemLocal, isOn);
					toggle.isOn = DebugFlag.IsActive(itemLocal);
				}).AddTo(gameObject2);
			}
			Reflesh();
		}

		public void Reflesh()
		{
			if ((bool)_folderObj)
			{
				_folderObj.GetComponentInChildren<TextMeshProUGUI>(includeInactive: true).text = (IsOpen ? "▼ " : "■ ") + Name;
			}
			foreach (KeyValuePair<DebugFlag.Flag, GameObject> item in _itemDic)
			{
				item.Value.SetActive(IsOpen);
				item.Value.GetComponentInChildren<Toggle>(includeInactive: true).isOn = DebugFlag.IsActive(item.Key);
			}
		}
	}

	public static List<FoldoutGroup> GenerateFoldoutGroups(bool isAllOpen = true)
	{
		List<FoldoutGroup> list = new List<FoldoutGroup>();
		foreach (KeyValuePair<string, List<DebugFlag.Flag>> item in (from x in Enum.GetValues(typeof(DebugFlag.Flag)).OfType<DebugFlag.Flag>()
			group x by x.GetEnumName().Split(new char[1] { '/' }, 2)[0] into x
			select new
			{
				Name = x.Key,
				Items = x.Select((DebugFlag.Flag y) => y).ToList()
			}).ToDictionary(x => x.Name, x => x.Items))
		{
			bool useFoldout = item.Value.Count != 1 || item.Value[0].GetEnumName().IndexOf('/') >= 0;
			list.Add(new FoldoutGroup
			{
				UseFoldout = useFoldout,
				Name = item.Key,
				Items = item.Value,
				IsOpen = isAllOpen
			});
		}
		for (int i = 0; i < Enum.GetValues(typeof(DebugFlag.Flag)).Length; i++)
		{
			int key = i;
			DebugFlag.Flag flag = (DebugFlag.Flag)i;
			DebugFlag.SetActive((DebugFlag.Flag)key, PlayerPrefs.GetInt("VFG/DebugFlag/" + flag, 0) > 0);
		}
		return list;
	}
}
