using System.Collections.Generic;
using MFG.Types.ImportantNotice;

namespace MFG.MasterGameData;

public class ImportantNoticeMaster : MasterGameDataBase
{
	private static List<Item> _items = new List<Item>
	{
		new Item
		{
			ID = NoticeID.Manner,
			Title = "対局中のマナーについて",
			IsCloseButtonDisp = true
		},
		new Item
		{
			ID = NoticeID.Nuisance,
			Title = "不適切行為について"
		}
	};

	public static List<Item> Items => _items;

	public static Item GetItem(NoticeID id)
	{
		return _items.Find((Item item) => item.ID == id);
	}
}
