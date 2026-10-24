using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.GameRequest;
using MFG.MasterGameData;
using MFG.Types;
using MFG.Types.Customize.Costume;
using MFG.Types.Customize.MatchingBackground;
using UnityEngine;

namespace MFG.GameData;

[Serializable]
public class CustomizeGameData
{
	private const int CurrentDataVersion = 2;

	public int DataVersion = 2;

	public const string ClientStateName = "customize_item";

	public List<CustomizeItemInfo> CustomizeItems = new List<CustomizeItemInfo>();

	public List<CutinItemInfo> CutinItems = new List<CutinItemInfo>();

	public List<CostumeItemInfo> CostumeItems = new List<CostumeItemInfo>();

	public List<CatchphraseItemInfo> CatchphraseItems = new List<CatchphraseItemInfo>();

	public List<MatchingBGInfo> MatchingBGItems = new List<MatchingBGInfo>();

	public int TotalGacha1Count;

	public int TotalGacha1WithoutTicketCount;

	public int TotalGacha5Count;

	public int TotalGacha5WithoutTicketCount;

	public int TotalGacha5CountVer2;

	public int TotalGacha5WithoutTicketCountVer2;

	public int TotalMusicGachaCount;

	public bool IsGachaTried => TotalGacha1Count + TotalGacha5Count + TotalMusicGachaCount > 0;

	public void Reset()
	{
		CustomizeItems = CustomizeItemInfo.CreateDefaultItems();
		CutinItems = CutinItemInfo.CreateDefaultItems();
		CostumeItems = CostumeItemInfo.CreateDefaultItems();
		CatchphraseItems = CatchphraseItemInfo.CreateDefaultItems();
		MatchingBGItems = MatchingBGInfo.CreateDefaultItems();
	}

	public void AddNewCharacter()
	{
		foreach (CustomizeItemInfo item in CustomizeItemInfo.CreateDefaultItems())
		{
			if (CustomizeItems.Find((CustomizeItemInfo a) => a.ItemID == item.ItemID) == null)
			{
				CustomizeItems.Add(item);
			}
		}
		foreach (CutinItemInfo item2 in CutinItemInfo.CreateDefaultItems())
		{
			if (CutinItems.Find((CutinItemInfo a) => a.ItemID == item2.ItemID) == null)
			{
				CutinItems.Add(item2);
			}
		}
		foreach (CostumeItemInfo item3 in CostumeItemInfo.CreateDefaultItems())
		{
			if (CostumeItems.Find((CostumeItemInfo a) => a.ItemID == item3.ItemID) == null)
			{
				CostumeItems.Add(item3);
			}
		}
		foreach (CatchphraseItemInfo item4 in CatchphraseItemInfo.CreateDefaultItems())
		{
			if (CatchphraseItems.Find((CatchphraseItemInfo a) => a.ItemID == item4.ItemID) == null)
			{
				CatchphraseItems.Add(item4);
			}
		}
		foreach (MatchingBGInfo item5 in MatchingBGInfo.CreateDefaultItems())
		{
			if (MatchingBGItems.Find((MatchingBGInfo a) => a.ItemID == item5.ItemID) == null)
			{
				MatchingBGItems.Add(item5);
			}
		}
	}

	public CustomizeGameData()
	{
		Reset();
	}

	public async UniTask Save(CancellationToken cancellationToken = default(CancellationToken))
	{
		string text = JsonUtility.ToJson(this);
		UnityEngine.Debug.Log("CustomizeGameData Save : " + text);
		await RequestApiBase<ClientStateWrite, ClientStateWrite.ResultData, (string, string)>.Send(("customize_item", text), enableErrorScreenTransition: false, enableSendLog: true, cancellationToken);
	}

	public static CustomizeGameData LoadFromJson(string jsonString)
	{
		UnityEngine.Debug.Log("CustomizeGameData Load : " + jsonString);
		CustomizeGameData customizeGameData = JsonUtility.FromJson<CustomizeGameData>(jsonString);
		if (customizeGameData == null)
		{
			customizeGameData = new CustomizeGameData();
		}
		customizeGameData.Validate();
		return customizeGameData;
	}

	private void Validate()
	{
		int num = CustomizeItems.RemoveAll((CustomizeItemInfo x) => x.Master == null);
		if (num > 0)
		{
			UnityEngine.Debug.LogWarning($"CustomizeGameData: {num}個のアイテムを無効化しました。");
		}
		int num2 = CatchphraseItems.RemoveAll((CatchphraseItemInfo x) => x.Master == null);
		if (num2 > 0)
		{
			UnityEngine.Debug.LogWarning($"CatchphraseItem: {num2}個のアイテムを無効化しました。");
		}
		AddNewCharacter();
	}

	public void UpdateCostume()
	{
		if (GameDataManager.IsShioriKDECostumeEnable() && !CostumeItems.Exists((CostumeItemInfo x) => x.ItemID == MFG.Types.Customize.Costume.ItemID.Costume10_004) && CostumeItems.Exists((CostumeItemInfo x) => x.ItemID == MFG.Types.Customize.Costume.ItemID.Costume10_002))
		{
			CostumeItems.Add(new CostumeItemInfo
			{
				ItemID = MFG.Types.Customize.Costume.ItemID.Costume10_004
			});
		}
	}

	public void UpdateMatchingBG()
	{
		if (GameDataManager.Player.IsPro())
		{
			MatchingBGItems.Clear();
			MatchingBGItems.Add(new MatchingBGInfo
			{
				ItemID = MatchingBackgroundMaster.GetMaItemID(GameDataManager.Player.SelectChara)
			});
			return;
		}
		IEnumerable<MatchingBGInfo> enumerable = MatchingBGItems.Where((MatchingBGInfo x) => x.Master.Category == Category.Ma);
		if (enumerable != null)
		{
			foreach (MatchingBGInfo item2 in enumerable)
			{
				MatchingBGItems.Remove(item2);
			}
		}
		foreach (MFG.Types.Customize.MatchingBackground.Item item in MatchingBackgroundMaster.GetGachaCutinItems())
		{
			if (CutinItems.Find((CutinItemInfo x) => x.ItemID == item.UnlockingCutinID) != null && !MatchingBGItems.Exists((MatchingBGInfo x) => x.ItemID == item.ItemID))
			{
				MatchingBGItems.Add(new MatchingBGInfo
				{
					ItemID = item.ItemID
				});
			}
		}
	}

	public MFG.Types.Customize.MatchingBackground.ItemID GetRandomMatchingBG()
	{
		CharaType chara = GameDataManager.Player.GetCurrentChara().charaType;
		try
		{
			List<MatchingBGInfo> source = new List<MatchingBGInfo>(MatchingBGItems);
			if (GameUtility.IsKonastaOrNetcafeBehavior)
			{
				source = source.Where((MatchingBGInfo x) => !MatchingBackgroundMaster.IsPonnomichi(x.ItemID)).ToList();
			}
			List<MatchingBGInfo> list = source.Where((MatchingBGInfo x) => x.Master.CharaType == chara && x.IsOn).ToList();
			if (list.Count <= 0)
			{
				list = source.Where((MatchingBGInfo x) => x.Master.CharaType == chara).ToList();
			}
			return list[UnityEngine.Random.Range(0, list.Count)].ItemID;
		}
		catch
		{
			return MatchingBackgroundMaster.GetDefaultItemIDFirst(chara);
		}
	}

	[Conditional("DEBUG")]
	public void CutinItem_Log(Func<CutinItemInfo, bool> predicate)
	{
		int num = CutinItems.Count(predicate);
		UnityEngine.Debug.Log($"<color=cyan>CutinItem Count : {num}</color>");
		if (num <= 0)
		{
			return;
		}
		foreach (CutinItemInfo cutinItem in CutinItems)
		{
			if (predicate(cutinItem))
			{
				UnityEngine.Debug.Log($" <color=cyan>{cutinItem.ItemID}</color>");
			}
		}
	}
}
