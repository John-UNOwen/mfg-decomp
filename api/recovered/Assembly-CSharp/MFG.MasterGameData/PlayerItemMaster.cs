using System;
using System.Collections.Generic;
using System.Linq;
using MFG.Types;
using MFG.Types.PlayerItem;
using UnityEngine;

namespace MFG.MasterGameData;

public class PlayerItemMaster : MasterGameDataBase
{
	private static List<Item> m_items = new List<Item>
	{
		new Item
		{
			ObjectID = "OID_CharaExpUpPresent",
			ItemID = ItemID.Present01,
			Name = "ギフト",
			Description = "ガールにプレゼントすると好感度が上昇する。"
		},
		new Item
		{
			ObjectID = "OID_FightSpirit_Chara01",
			ItemID = ItemID.FightSpirit01,
			Category = Category.FightSpirit,
			Name = "ファイトスピリット（ヒヨリ）",
			Description = "ヒヨリのデビュー・衣装の交換に使用することができる。"
		},
		new Item
		{
			ObjectID = "OID_FightSpirit_Chara02",
			ItemID = ItemID.FightSpirit02,
			Category = Category.FightSpirit,
			Name = "ファイトスピリット（セン）",
			Description = "センのデビュー・衣装の交換に使用することができる。"
		},
		new Item
		{
			ObjectID = "OID_FightSpirit_Chara03",
			ItemID = ItemID.FightSpirit03,
			Category = Category.FightSpirit,
			Name = "ファイトスピリット（ツミレ）",
			Description = "ツミレのデビュー・衣装の交換に使用することができる。"
		},
		new Item
		{
			ObjectID = "OID_FightSpirit_Chara04",
			ItemID = ItemID.FightSpirit04,
			Category = Category.FightSpirit,
			Name = "ファイトスピリット（テンシ）",
			Description = "テンシのデビュー・衣装の交換に使用することができる。"
		},
		new Item
		{
			ObjectID = "OID_FightSpirit_Chara05",
			ItemID = ItemID.FightSpirit05,
			Category = Category.FightSpirit,
			Name = "ファイトスピリット（ヤオ）",
			Description = "ヤオのデビュー・衣装の交換に使用することができる。"
		},
		new Item
		{
			ObjectID = "OID_FightSpirit_Chara06",
			ItemID = ItemID.FightSpirit06,
			Category = Category.FightSpirit,
			Name = "ファイトスピリット（ミツバ）",
			Description = "ミツバのデビュー・衣装の交換に使用することができる。"
		},
		new Item
		{
			ObjectID = "OID_FightSpirit_Chara07",
			ItemID = ItemID.FightSpirit07,
			Category = Category.FightSpirit,
			Name = "ファイトスピリット（トイトイ）",
			Description = "トイトイのデビュー・衣装の交換に使用することができる。"
		},
		new Item
		{
			ObjectID = "OID_FightSpirit_Chara08",
			ItemID = ItemID.FightSpirit08,
			Category = Category.FightSpirit,
			Name = "ファイトスピリット（ムサシ）",
			Description = "ムサシのデビュー・衣装の交換に使用することができる。"
		},
		new Item
		{
			ObjectID = "OID_FightSpirit_Chara09",
			ItemID = ItemID.FightSpirit09,
			Category = Category.FightSpirit,
			Name = "ファイトスピリット（パイン）",
			Description = "パインのデビュー・衣装の交換に使用することができる。"
		},
		new Item
		{
			ObjectID = "OID_FightSpirit_Wildcard",
			ItemID = ItemID.FightSpiritW,
			Category = Category.FightSpirit,
			Name = "ファイトスピリット（だれでも）",
			Description = "任意のガールのデビュー・衣装の交換に使用することができる。"
		},
		new Item
		{
			ObjectID = "OID_GachaTicket",
			ItemID = ItemID.GachaTicket,
			Category = Category.GachaTicket,
			Name = "ガチャチケット",
			Description = "10枚で1回、チケットガチャを引くことができる。"
		},
		new Item
		{
			ObjectID = "OID_Voice",
			ItemID = ItemID.Voice,
			Name = "ボイス",
			Description = "対局中に聞くことができるガールのボイス。"
		},
		new Item
		{
			ObjectID = "OID_FightSpirit_Chara10",
			ItemID = ItemID.FightSpirit10,
			Category = Category.FightSpirit,
			Name = "ファイトスピリット（詩織）",
			Description = "詩織のデビュー・衣装の交換に使用することができる。"
		},
		new Item
		{
			ObjectID = "OID_FightSpirit_Chara11",
			ItemID = ItemID.FightSpirit11,
			Category = Category.FightSpirit,
			Name = "ファイトスピリット（カオス）",
			Description = "カオスのデビュー・衣装の交換に使用することができる。"
		},
		new Item
		{
			ObjectID = "OID_FightSpirit_Chara12",
			ItemID = ItemID.FightSpirit12,
			Category = Category.FightSpirit,
			Name = "ファイトスピリット（クリア）",
			Description = "クリアのデビュー・衣装の交換に使用することができる。"
		},
		new Item
		{
			ObjectID = "OID_FightSpirit_Chara13",
			ItemID = ItemID.FightSpirit13,
			Category = Category.FightSpirit,
			Name = "ファイトスピリット（イヨ）",
			Description = "イヨのデビュー・衣装の交換に使用することができる。"
		},
		new Item
		{
			ObjectID = "OID_ExchangePoint_GrimAroe",
			ItemID = ItemID.ExchangePointGrimAroe,
			Category = Category.ExchangePoint,
			Name = "マジカルキャンディ",
			Description = "後日開催予定の「『グリム・アロエ』ピックアップガチャ」の交換点に変換される。"
		},
		new Item
		{
			ObjectID = "OID_FightSpirit_Chara14",
			ItemID = ItemID.FightSpirit14,
			Category = Category.FightSpirit,
			Name = "ファイトスピリット（グリム・アロエ）",
			Description = "グリム・アロエのデビュー・衣装の交換に使用することができる。"
		},
		new Item
		{
			ObjectID = "OID_FightSpirit_Chara15",
			ItemID = ItemID.FightSpirit15,
			Category = Category.FightSpirit,
			Name = "ファイトスピリット（ココア）",
			Description = "ココアのデビュー・衣装の交換に使用することができる。"
		},
		new Item
		{
			ObjectID = "OID_ExchangePoint_Dia",
			ItemID = ItemID.ExchangePointDia,
			Category = Category.ExchangePoint,
			Name = "ダイアモンド",
			Description = "後日開催予定の「『ミツモト・ダイア』ピックアップガチャ」の交換点に変換される。"
		},
		new Item
		{
			ObjectID = "OID_FightSpirit_Chara16",
			ItemID = ItemID.FightSpirit16,
			Category = Category.FightSpirit,
			Name = "ファイトスピリット（ダイア）",
			Description = "ダイアのデビュー・衣装の交換に使用することができる。"
		},
		new Item
		{
			ObjectID = "OID_ExchangeItem_Wildcard",
			ItemID = ItemID.ExchangeItemW,
			Category = Category.ExchangePoint,
			Name = "交換点（どれでも）",
			Description = "ガチャの交換点として使用できる。",
			LimitedCount = 30
		},
		new Item
		{
			ObjectID = "OID_ExchangePoint_Doubriel",
			ItemID = ItemID.ExchangePointDoubriel,
			Category = Category.ExchangePoint,
			Name = "マヨネーズ",
			Description = "後日開催予定の「『ダブリエル』ピックアップガチャ」の交換点に変換される。"
		},
		new Item
		{
			ObjectID = "OID_FightSpirit_Chara17",
			ItemID = ItemID.FightSpirit17,
			Category = Category.FightSpirit,
			Name = "ファイトスピリット（ダブリエル）",
			Description = "ダブリエルのデビュー・衣装の交換に使用することができる。"
		},
		new Item
		{
			ObjectID = "OID_ExchangePoint_Ippatsu",
			ItemID = ItemID.ExchangePointIppatsu,
			Category = Category.ExchangePoint,
			Name = "ペン",
			Description = "後日開催予定の「『タテナオリ・イッパツ』ピックアップガチャ」の交換点に変換される。"
		},
		new Item
		{
			ObjectID = "OID_FightSpirit_Chara18",
			ItemID = ItemID.FightSpirit18,
			Category = Category.FightSpirit,
			Name = "ファイトスピリット（イッパツ）",
			Description = "イッパツのデビュー・衣装の交換に使用することができる。"
		},
		new Item
		{
			ObjectID = "OID_ExchangePoint_Shiroe",
			ItemID = ItemID.ExchangePointShiroe,
			Category = Category.ExchangePoint,
			Name = "肉じゃが",
			Description = "後日開催予定の「『シロヱ』ピックアップガチャ」の交換点に変換される。"
		},
		new Item
		{
			ObjectID = "OID_FightSpirit_Chara19",
			ItemID = ItemID.FightSpirit19,
			Category = Category.FightSpirit,
			Name = "ファイトスピリット（シロヱ）",
			Description = "シロヱのデビュー・衣装の交換に使用することができる。"
		},
		new Item
		{
			ObjectID = "OID_ExchangePoint_Jun",
			ItemID = ItemID.ExchangePointJun,
			Category = Category.ExchangePoint,
			Name = "金棒",
			Description = "後日開催予定の「『シゴロヌキ・ジュン』ピックアップガチャ」の交換点に変換される。"
		},
		new Item
		{
			ObjectID = "OID_FightSpirit_Chara20",
			ItemID = ItemID.FightSpirit20,
			Category = Category.FightSpirit,
			Name = "ファイトスピリット（ジュン）",
			Description = "ジュンのデビュー・衣装の交換に使用することができる。"
		},
		new Item
		{
			ObjectID = "OID_ExchangePoint_Touka",
			ItemID = ItemID.ExchangePointTouka,
			Category = Category.ExchangePoint,
			Name = "ちゅーちゅーパック",
			Description = "後日開催予定の「『クレンホウ・トウカ』ピックアップガチャ」の交換点に変換される。"
		},
		new Item
		{
			ObjectID = "OID_FightSpirit_Chara21",
			ItemID = ItemID.FightSpirit21,
			Category = Category.FightSpirit,
			Name = "ファイトスピリット（トウカ）",
			Description = "トウカのデビュー・衣装の交換に使用することができる。"
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

	public static ItemID ToFightSpiritItemID(CharaType charaType)
	{
		if (Enum.TryParse<ItemID>($"FightSpirit{(int)(charaType + 1):D2}", out var result))
		{
			return result;
		}
		Debug.LogWarning($"PlayerItemMaster.ToFightSpiritItemID: 変換できませんでした。CharaType={charaType}");
		return ItemID.None;
	}

	public static List<ItemID> GetFightSpiritList()
	{
		return (from x in m_items
			where x.Category == Category.FightSpirit
			orderby x.ItemID == ItemID.FightSpiritW
			select x.ItemID).ToList();
	}
}
