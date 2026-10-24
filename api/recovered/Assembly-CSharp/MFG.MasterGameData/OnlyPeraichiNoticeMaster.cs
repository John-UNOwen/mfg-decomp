using System.Collections.Generic;
using System.Linq;
using MFG.Types.OnlyPeraichiNotice;

namespace MFG.MasterGameData;

public class OnlyPeraichiNoticeMaster : MasterGameDataBase
{
	private static List<Item> _items = new List<Item>
	{
		new Item
		{
			ID = NoticeID.MFCSpSerialCode,
			EventType = GameEventType.MFCSpSerialCode
		}
	};

	public static List<Item> Items => _items;

	public static List<Item> GetEnableItems()
	{
		return _items.Where((Item x) => x.IsEnable).ToList();
	}
}
