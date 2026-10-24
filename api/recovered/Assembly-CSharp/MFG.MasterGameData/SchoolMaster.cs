using System.Collections.Generic;
using MFG.Types;
using MFG.Types.SchoolMaster;

namespace MFG.MasterGameData;

public class SchoolMaster : MasterGameDataBase
{
	private static List<Item> _items = new List<Item>
	{
		new Item
		{
			SchoolID = SchoolID.School001,
			Name = "ネリマ壱役高等学校",
			ShortName = "壱高"
		},
		new Item
		{
			SchoolID = SchoolID.School002,
			Name = "アキバ弐役女学院",
			ShortName = "弐女"
		},
		new Item
		{
			SchoolID = SchoolID.School003,
			Name = "カワサキ参役塾高等学校",
			ShortName = "参塾"
		},
		new Item
		{
			SchoolID = SchoolID.School004,
			Name = "イケブクロ四役女学院",
			ShortName = "四女"
		},
		new Item
		{
			SchoolID = SchoolID.School005,
			Name = "私立セイジョウ六役学園",
			ShortName = "六学"
		},
		new Item
		{
			SchoolID = SchoolID.School006,
			Name = "帝国立ナガタチョウ役満高等学校",
			ShortName = "帝役"
		},
		new Item
		{
			SchoolID = SchoolID.School007,
			Name = "ドルプロ",
			ShortName = "ドルプロ"
		},
		new Item
		{
			SchoolID = SchoolID.School008,
			Name = "きらめき高校",
			ShortName = "きらめき高校"
		},
		new Item
		{
			SchoolID = SchoolID.School009,
			Name = "マジックアカデミー",
			ShortName = "マジックアカデミー"
		},
		new Item
		{
			SchoolID = SchoolID.School010,
			Name = "アカデミア",
			ShortName = "アカデミア"
		}
	};

	public static Item GetItem(SchoolID schoolID)
	{
		return _items.Find((Item item) => item.SchoolID == schoolID);
	}
}
