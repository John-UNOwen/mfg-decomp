using System.Collections.Generic;
using System.Linq;
using MFG.Types.OneShotNotice;

namespace MFG.MasterGameData;

public class OneShotNoticeMaster : MasterGameDataBase
{
	private static List<Item> _items = new List<Item>
	{
		new Item
		{
			ID = NoticeID.GrimAroeExchangePoint,
			EventType = GameEventType.GrimAroeEntryEvent
		},
		new Item
		{
			ID = NoticeID.DiaExchangePoint,
			EventType = GameEventType.DiaEntryEvent
		},
		new Item
		{
			ID = NoticeID.DoubrielExchangePoint,
			EventType = GameEventType.DoubrielEntryEvent
		},
		new Item
		{
			ID = NoticeID.IppatsuExchangePoint,
			EventType = GameEventType.IppatsuEntryEvent
		},
		new Item
		{
			ID = NoticeID.ShiroeExchangePoint,
			EventType = GameEventType.ShiroeEntryEvent
		},
		new Item
		{
			ID = NoticeID.JunExchangePoint,
			EventType = GameEventType.JunEntryEvent
		},
		new Item
		{
			ID = NoticeID.ToukaExchangePoint,
			EventType = GameEventType.ToukaEntryEvent
		}
	};

	public static List<Item> Items => _items;

	public static List<Item> GetEnableItems()
	{
		return _items.Where((Item x) => x.IsEnable).ToList();
	}
}
