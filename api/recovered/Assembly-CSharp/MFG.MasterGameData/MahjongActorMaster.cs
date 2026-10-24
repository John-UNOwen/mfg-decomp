using System.Collections.Generic;
using MFG.Types;
using MFG.Types.MahjongActorMaster;

namespace MFG.MasterGameData;

public class MahjongActorMaster : MasterGameDataBase
{
	private static List<Item> _items = new List<Item>
	{
		new Item
		{
			MahjongActorID = MahjongActorID.Ma001,
			CharaType = CharaType.Chara01,
			LastName = "高宮",
			FirstName = "まり"
		},
		new Item
		{
			MahjongActorID = MahjongActorID.Ma002,
			CharaType = CharaType.Chara02,
			LastName = "東城",
			FirstName = "りお"
		},
		new Item
		{
			MahjongActorID = MahjongActorID.Ma003,
			CharaType = CharaType.Chara03,
			LastName = "岡田",
			FirstName = "紗佳"
		},
		new Item
		{
			MahjongActorID = MahjongActorID.Ma004,
			CharaType = CharaType.Chara04,
			LastName = "川上",
			FirstName = "レイ"
		},
		new Item
		{
			MahjongActorID = MahjongActorID.Ma005,
			CharaType = CharaType.Chara05,
			LastName = "木下",
			FirstName = "遥"
		},
		new Item
		{
			MahjongActorID = MahjongActorID.Ma006,
			CharaType = CharaType.Chara06,
			LastName = "赤木",
			FirstName = "クロ"
		},
		new Item
		{
			MahjongActorID = MahjongActorID.Ma007,
			CharaType = CharaType.Chara07,
			LastName = "武田",
			FirstName = "雛歩"
		},
		new Item
		{
			MahjongActorID = MahjongActorID.Ma008,
			CharaType = CharaType.Chara08,
			LastName = "伊達",
			FirstName = "朱里紗"
		},
		new Item
		{
			MahjongActorID = MahjongActorID.Ma011,
			CharaType = CharaType.Chara11,
			LastName = "御子柴",
			FirstName = "佑梨"
		},
		new Item
		{
			MahjongActorID = MahjongActorID.Ma012,
			CharaType = CharaType.Chara12,
			LastName = "香野",
			FirstName = "蘭"
		}
	};

	public static Item GetItem(MahjongActorID mahjongActorID)
	{
		return _items.Find((Item item) => item.MahjongActorID == mahjongActorID);
	}

	public static Item GetItem(CharaType charaType)
	{
		return _items.Find((Item item) => item.CharaType == charaType);
	}

	public static List<Item> GetItems()
	{
		return _items;
	}
}
