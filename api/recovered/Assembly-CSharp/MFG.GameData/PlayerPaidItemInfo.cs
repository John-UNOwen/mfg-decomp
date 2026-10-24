using System;
using System.Collections.Generic;
using System.Linq;
using MFG.MasterGameData;
using MFG.Types.PlayerItem;
using MFG.Types.PlayerPaidItem;

namespace MFG.GameData;

[Serializable]
public class PlayerPaidItemInfo
{
	public MFG.Types.PlayerPaidItem.ItemID ItemID;

	public List<PaidItem> Details = new List<PaidItem>();

	public MFG.Types.PlayerPaidItem.Item Master => PlayerPaidItemMaster.GetItem(ItemID);

	public int Count()
	{
		Reflesh();
		try
		{
			return (from x in Details
				where x.Expire > DateTimeOffsetManager.GetJSTTime()
				select x.ItemCount).Sum();
		}
		catch
		{
		}
		return 0;
	}

	public bool Add(int count, string expire)
	{
		Reflesh();
		DateTimeOffset exp = DateTimeOffsetManager.GetJSTFromString(expire);
		if (count > 0 && exp > DateTimeOffsetManager.GetJSTTime())
		{
			if (Details.Exists((PaidItem x) => x.Expire == exp))
			{
				Details.Find((PaidItem x) => x.Expire == exp).ItemCount += count;
			}
			else
			{
				Details.Add(new PaidItem
				{
					ItemCount = count,
					ExpireDate = expire
				});
			}
			return true;
		}
		return false;
	}

	public bool Consume(int consumeCount)
	{
		Reflesh();
		try
		{
			if (Count() >= consumeCount)
			{
				foreach (PaidItem detail in new List<PaidItem>(Details.OrderBy((PaidItem x) => x.Expire).ToList()))
				{
					if (detail.ItemCount <= consumeCount)
					{
						consumeCount -= detail.ItemCount;
						Details.Remove(detail);
					}
					else
					{
						Details.Find((PaidItem x) => x == detail).ItemCount -= consumeCount;
						consumeCount = 0;
					}
					if (consumeCount <= 0)
					{
						return true;
					}
				}
			}
		}
		catch
		{
		}
		return false;
	}

	public void Reflesh()
	{
		try
		{
			foreach (PaidItem item in Details.Where((PaidItem x) => x.Expire <= DateTimeOffsetManager.GetJSTTime()))
			{
				if (Master.IsTicketConvertEnable)
				{
					GameDataManager.PlayerItem.Add(MFG.Types.PlayerItem.ItemID.GachaTicket, item.ItemCount);
				}
				Details.Remove(item);
			}
		}
		catch
		{
		}
	}

	public DateTimeOffset NearestExpire()
	{
		Reflesh();
		if (Details == null)
		{
			return default(DateTimeOffset);
		}
		return Details.OrderBy((PaidItem x) => x.Expire).First().Expire;
	}
}
