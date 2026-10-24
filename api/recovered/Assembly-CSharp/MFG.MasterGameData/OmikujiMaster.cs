using System.Collections.Generic;
using MFG.GameData;
using MFG.Types;
using MFG.Types.Omikuji;

namespace MFG.MasterGameData;

public class OmikujiMaster : MasterGameDataBase
{
	private static List<Item> _items = new List<Item>
	{
		new Item
		{
			OmikujiType = OmikujiType.Omikuji2026,
			FlagType = GameEventType.Omikuji2026,
			CardConnectMissionID = CardConnectMissionID.Omikuji2026,
			MaxCharaType = CharaType.Chara19,
			ItemName = "おみくじ"
		}
	};

	public static List<Item> Items => _items;

	public static Item GetItem(GameEventType eventType)
	{
		return _items.Find((Item item) => item.FlagType == eventType);
	}

	public static Item GetItem(OmikujiType omikujiType)
	{
		return _items.Find((Item item) => item.OmikujiType == omikujiType);
	}
}
