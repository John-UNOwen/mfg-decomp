using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.GameRequest;
using MFG.MasterGameData;
using MFG.Types;
using MFG.Types.PlayerItem;
using UnityEngine;

namespace MFG.GameData;

[Serializable]
public class PlayerItemGameData
{
	private const int CurrentDataVersion = 2;

	public int DataVersion = 2;

	public const string ClientStateName = "player_item";

	public List<PlayerItemInfo> Items = new List<PlayerItemInfo>();

	public void Add(ItemID itemID, int count)
	{
		int num = Items.FindIndex((PlayerItemInfo x) => x.ItemID == itemID);
		if (num < 0)
		{
			Items.Add(new PlayerItemInfo
			{
				ItemID = itemID,
				Count = count
			});
		}
		else
		{
			Items[num].Add(count);
		}
	}

	public async UniTask Consume(ItemID itemID, int consumeCount, ItemConsumeType consumeType)
	{
		int num = Items.FindIndex((PlayerItemInfo x) => x.ItemID == itemID);
		int num2 = Count(itemID);
		if (num < 0 || !Items[num].Consume(consumeCount))
		{
			string message = $"Consumeできませんでした ItemID:{itemID} curCount:{num2} consumeCount:{consumeCount}";
			Debug.LogError(message);
			throw new InvalidOperationException(message);
		}
		if (consumeType != 0)
		{
			try
			{
				await RequestApiBase<ItemConsumeLog, ItemConsumeLog.ResultData, (string, int, int, ItemConsumeType)>.Send((PlayerItemMaster.ToOID(itemID), num2, Count(itemID), consumeType), enableErrorScreenTransition: false);
			}
			catch
			{
				Debug.Log("ItemConsumeLog Send Failed");
			}
		}
	}

	public int Count(ItemID itemID)
	{
		int num = Items.FindIndex((PlayerItemInfo x) => x.ItemID == itemID);
		if (num < 0)
		{
			return 0;
		}
		return Items[num].Count;
	}

	public void Set(ItemID itemID, int count)
	{
		int num = Items.FindIndex((PlayerItemInfo x) => x.ItemID == itemID);
		if (num < 0)
		{
			Items.Add(new PlayerItemInfo
			{
				ItemID = itemID,
				Count = count
			});
		}
		else
		{
			Items[num].Count = count;
		}
	}

	public int GetUsableSpiritCount(CharaType charaType)
	{
		ItemID itemID = PlayerItemMaster.ToFightSpiritItemID(charaType);
		int num = Count(itemID);
		int num2 = Count(ItemID.FightSpiritW);
		return num + num2;
	}

	public (int charaFightSpiritCount, int wildFightSpiritCount) GetFightSpiritCount(CharaType charaType)
	{
		ItemID itemID = PlayerItemMaster.ToFightSpiritItemID(charaType);
		return (charaFightSpiritCount: GameDataManager.PlayerItem.Count(itemID), wildFightSpiritCount: GameDataManager.PlayerItem.Count(ItemID.FightSpiritW));
	}

	public async UniTask Save(CancellationToken cancellationToken = default(CancellationToken))
	{
		string text = JsonUtility.ToJson(this);
		Debug.Log("PlayerItemGameData Save : " + text);
		await RequestApiBase<ClientStateWrite, ClientStateWrite.ResultData, (string, string)>.Send(("player_item", text), enableErrorScreenTransition: false, enableSendLog: true, cancellationToken);
	}

	public static PlayerItemGameData LoadFromJson(string jsonString)
	{
		Debug.Log("PlayerItemGameData Load : " + jsonString);
		PlayerItemGameData playerItemGameData = JsonUtility.FromJson<PlayerItemGameData>(jsonString);
		if (playerItemGameData == null)
		{
			playerItemGameData = new PlayerItemGameData();
		}
		playerItemGameData.Validate();
		return playerItemGameData;
	}

	private void Validate()
	{
		if (DataVersion != 1)
		{
			return;
		}
		if (Items.Find((PlayerItemInfo a) => a.ItemID == ItemID.GachaTicket) == null)
		{
			foreach (PlayerItemInfo item in Items)
			{
				if (item.ItemID >= ItemID.FightSpirit09)
				{
					item.ItemID++;
				}
			}
		}
		DataVersion = 2;
	}
}
