using System.Collections.Generic;
using System.Linq;
using MFG.Types.PlayerPaidItem;

namespace MFG.MasterGameData;

public class PlayerPaidItemMaster : MasterGameDataBase
{
	private static List<Item> m_items = new List<Item>
	{
		new Item
		{
			ObjectID = "OID_ExtraGachaTicket",
			ItemID = ItemID.ExtraGachaTicket,
			Category = Category.GachaTicket,
			Name = "ガチャチケット(どれでも)",
			Description = "10枚で1回、ガチャを引くことができる。\n※対象のガチャに制限無し。"
		}
	};

	public static Item GetItem(ItemID itemID)
	{
		return m_items.Find((Item item) => item.ItemID == itemID);
	}

	public static Item GetItem(string oid)
	{
		return m_items.Find((Item a) => a.ObjectID == oid);
	}

	public static ItemID ToItemID(string oid)
	{
		return m_items.Find((Item _item) => _item.ObjectID == oid)?.ItemID ?? ItemID.None;
	}

	public static string ToOID(ItemID itemID)
	{
		Item item = m_items.Find((Item _item) => _item.ItemID == itemID);
		if (item == null)
		{
			return "";
		}
		return item.ObjectID;
	}

	public static List<ItemID> GetItems(Category category)
	{
		return m_items.Where((Item x) => x.Category == category)?.Select((Item x) => x.ItemID)?.ToList() ?? null;
	}
}
