using System.Collections.Generic;
using System.Linq;
using MFG.GameData;
using MFG.MasterGameData;
using MFG.Types.Customize.CutinItem;
using UnityEngine;

namespace MFG.Types;

public static class GachaPlayUtility
{
	public static int Count(GachaPlayMode mode)
	{
		if (mode == GachaPlayMode.Five || mode == GachaPlayMode.TicketFive)
		{
			return 5;
		}
		return 1;
	}

	public static bool IsConsumeGacha(GachaPlayMode mode)
	{
		if (mode != GachaPlayMode.Five)
		{
			return mode == GachaPlayMode.One;
		}
		return true;
	}

	public static bool IsTicketGacha(GachaPlayMode mode)
	{
		if (mode != GachaPlayMode.TicketOne)
		{
			return mode == GachaPlayMode.TicketFive;
		}
		return true;
	}

	public static string ToString(GachaPlayMode mode)
	{
		return mode switch
		{
			GachaPlayMode.One => "1回ガチャ", 
			GachaPlayMode.Five => "5回ガチャ", 
			GachaPlayMode.TicketOne => "チケットガチャ", 
			GachaPlayMode.TicketFive => "チケットガチャ", 
			_ => "ガチャ", 
		};
	}

	public static bool IsClearLottery(float range)
	{
		return Random.Range(0f, 1f) < range;
	}

	public static (Harbinger, bool) HarbingerLotate(GachaSeriesServerInfo info, List<GachaItemInfo> itemInfos)
	{
		GachaSeriesServerInfo gachaSeriesServerInfo = info;
		bool flag = gachaSeriesServerInfo != null && gachaSeriesServerInfo.LimitedItems?.Count((ItemID x) => CutinItemMaster.GetItem(x).Rarity >= Rarity.SR) > 0;
		bool flag2 = flag && itemInfos.Exists((GachaItemInfo x) => info.LimitedItems.Where((ItemID x) => CutinItemMaster.GetItem(x).Rarity >= Rarity.SR).Contains(x.ItemID));
		if ((ItemCount(Rarity.UR) > 0 && !flag) || flag2)
		{
			Debug.Log("前兆：最強");
			return (IsClearLottery(0.7f) ? Harbinger.Decision : (IsClearLottery(0.5f) ? Harbinger.High : Harbinger.None), true);
		}
		if ((ItemCount(Rarity.SR) >= 2 && !flag) || (ItemCount(Rarity.UR) >= 1 && flag))
		{
			Debug.Log("前兆：強");
			return (IsClearLottery(0.5f) ? Harbinger.High : (IsClearLottery(0.5f) ? Harbinger.Middle : Harbinger.None), true);
		}
		if (ItemCount(Rarity.SR) >= 1)
		{
			Debug.Log("前兆：中");
			return (IsClearLottery(0.5f) ? Harbinger.Middle : (IsClearLottery(0.5f) ? Harbinger.Low : Harbinger.None), false);
		}
		Debug.Log("前兆：弱");
		return (IsClearLottery(0.5f) ? Harbinger.Low : Harbinger.None, false);
		int ItemCount(Rarity rare)
		{
			return itemInfos?.Count((GachaItemInfo x) => CutinItemMaster.GetItem(x.ItemID).Rarity == rare) ?? 0;
		}
	}

	public static GachaItemInfo CreateItem(ItemID itemID)
	{
		GachaItemInfo obj = new GachaItemInfo
		{
			ItemID = itemID
		};
		Item item = CutinItemMaster.GetItem(obj.ItemID);
		GenerateRarity(obj, item.Rarity, ThumbnailChangePhase.Change2nd);
		return obj;
	}

	private static void GenerateRarity(GachaItemInfo argInfo, Rarity argRarity, ThumbnailChangePhase argPhase)
	{
		argInfo.ThumbnailChangeRaritys[argPhase] = argRarity;
		if (argPhase > ThumbnailChangePhase.Change1st)
		{
			float num = Random.Range(0f, 1f);
			GenerateRarity(argInfo, argRarity switch
			{
				Rarity.UR => (num >= 0.99f) ? Rarity.N : ((num >= 0.95f) ? Rarity.R : ((num >= 0.9f) ? Rarity.SR : Rarity.UR)), 
				Rarity.SR => (num >= 0.9f) ? Rarity.N : ((num >= 0.7f) ? Rarity.R : Rarity.SR), 
				Rarity.R => (num >= 0.6f) ? Rarity.N : Rarity.R, 
				Rarity.N => Rarity.N, 
				_ => argRarity, 
			}, argPhase - 1);
		}
	}

	public static string RarityColorName(Rarity argRarity)
	{
		return argRarity switch
		{
			Rarity.N => "", 
			Rarity.R => "Silver", 
			Rarity.SR => "Gold", 
			Rarity.UR => "Rainbow", 
			_ => "", 
		};
	}
}
