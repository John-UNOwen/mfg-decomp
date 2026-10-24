using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.GameRequest;
using MFG.MasterGameData;
using MFG.Types;
using MFG.Types.PlayerPaidItem;
using UnityEngine;

namespace MFG.GameData;

[Serializable]
public class PlayerPaidItemGameData
{
	private const int CurrentDataVersion = 1;

	public int DataVersion = 1;

	public const string ClientStateName = "player_paid_item";

	public List<PlayerPaidItemInfo> Items = new List<PlayerPaidItemInfo>();

	public void RefreshAll()
	{
		try
		{
			foreach (PlayerPaidItemInfo item in Items)
			{
				item.Reflesh();
			}
		}
		catch
		{
		}
	}

	public void Add(ItemID itemID, int count, string expire)
	{
		if (!Items.Exists((PlayerPaidItemInfo x) => x.ItemID == itemID))
		{
			Items.Add(new PlayerPaidItemInfo
			{
				ItemID = itemID
			});
		}
		Items.Find((PlayerPaidItemInfo x) => x.ItemID == itemID).Add(count, expire);
	}

	public async UniTask Consume(ItemID itemID, int consumeCount, ItemConsumeType consumeType)
	{
		int num = Items.FindIndex((PlayerPaidItemInfo x) => x.ItemID == itemID);
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
				await RequestApiBase<ItemConsumeLog, ItemConsumeLog.ResultData, (string, int, int, ItemConsumeType)>.Send((PlayerPaidItemMaster.ToOID(itemID), num2, Count(itemID), consumeType), enableErrorScreenTransition: false);
			}
			catch
			{
				Debug.Log("ItemConsumeLog Send Failed");
			}
		}
	}

	public int Count(ItemID itemID)
	{
		int num = Items.FindIndex((PlayerPaidItemInfo x) => x.ItemID == itemID);
		if (num < 0)
		{
			return 0;
		}
		return Items[num].Count();
	}

	public DateTimeOffset NearestExpire(ItemID itemID)
	{
		int num = Items.FindIndex((PlayerPaidItemInfo x) => x.ItemID == itemID);
		if (num < 0)
		{
			return default(DateTimeOffset);
		}
		return Items[num].NearestExpire();
	}

	public async UniTask Save(CancellationToken cancellationToken = default(CancellationToken))
	{
		string text = JsonUtility.ToJson(this);
		Debug.Log("PlayerPaidItemGameData Save : " + text);
		await RequestApiBase<ClientStateWrite, ClientStateWrite.ResultData, (string, string)>.Send(("player_paid_item", text), enableErrorScreenTransition: false, enableSendLog: true, cancellationToken);
	}

	public static PlayerPaidItemGameData LoadFromJson(string jsonString)
	{
		Debug.Log("PlayerPaidItemGameData Load : " + jsonString);
		PlayerPaidItemGameData playerPaidItemGameData = JsonUtility.FromJson<PlayerPaidItemGameData>(jsonString);
		if (playerPaidItemGameData == null)
		{
			playerPaidItemGameData = new PlayerPaidItemGameData();
		}
		playerPaidItemGameData.Validate();
		return playerPaidItemGameData;
	}

	private void Validate()
	{
	}
}
