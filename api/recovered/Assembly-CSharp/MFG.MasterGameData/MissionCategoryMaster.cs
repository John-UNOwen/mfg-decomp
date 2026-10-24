using System;
using System.Collections.Generic;
using System.Linq;
using MFG.Types.Mission;

namespace MFG.MasterGameData;

public class MissionCategoryMaster : MasterGameDataBase
{
	private static List<CategoryItem> _item = new List<CategoryItem>
	{
		new CategoryItem
		{
			Category = Category.JudgmentDaily,
			Name = "デイリー",
			IsJudgeCategory = true
		},
		new CategoryItem
		{
			Category = Category.JudgmentWeekly,
			Name = "ウィークリー",
			IsJudgeCategory = true
		},
		new CategoryItem
		{
			Category = Category.Player,
			Name = "プレーヤー"
		},
		new CategoryItem
		{
			Category = Category.Character,
			Name = "キャラクター"
		},
		new CategoryItem
		{
			Category = Category.Weekly,
			Name = "ウィークリー"
		},
		new CategoryItem
		{
			Category = Category.Yaku,
			Name = "役ミッション"
		},
		new CategoryItem
		{
			Category = Category.StartDash,
			Name = "スタートダッシュ"
		},
		new CategoryItem
		{
			Category = Category.Scramble,
			Name = "スクランブルマッチ"
		},
		new CategoryItem
		{
			Category = Category.MahjongActor,
			Name = "麻雀アクター"
		},
		new CategoryItem
		{
			Category = Category.Birthday,
			Name = "誕生日",
			IsDispPlayCountWindow = false
		},
		new CategoryItem
		{
			Category = Category.Mentanpin2,
			Name = "メンタンピン教室"
		},
		new CategoryItem
		{
			Category = Category.HalfAnniv2024,
			Name = "1.5周年"
		},
		new CategoryItem
		{
			Category = Category.Halloween2024,
			Name = "ハロウィン",
			MissionEventType = GameEventType.Halloween2024
		},
		new CategoryItem
		{
			Category = Category.GrimAroeTicket,
			Name = "マジカルキャンディ",
			MissionEventType = GameEventType.GrimAroeGachaTicket
		},
		new CategoryItem
		{
			Category = Category.Birthday2024_02,
			Name = "誕生日"
		},
		new CategoryItem
		{
			Category = Category.MeldBonus,
			Name = "PCK"
		},
		new CategoryItem
		{
			Category = Category.WelcomeComeback3,
			Name = "ウェルカムカムバック",
			MissionEventType = GameEventType.WelcomeComeback3,
			IsWelcomeComebackEvent = true
		},
		new CategoryItem
		{
			Category = Category.Xmas2024,
			Name = "クリスマス"
		},
		new CategoryItem
		{
			Category = Category.Competition6,
			Name = "大会",
			MissionEventType = GameEventType.Competition6,
			MainMissionID = MissionID.Competition6_Vs,
			MapBGType = BGType.Universe
		},
		new CategoryItem
		{
			Category = Category.NewYear2025,
			Name = "遊び初め",
			MissionEventType = GameEventType.NewYear2025,
			MainMissionID = MissionID.NewYear2025_Vs,
			MapBGType = BGType.Wagara
		},
		new CategoryItem
		{
			Category = Category.Birthday2025_13,
			Name = "誕生日"
		},
		new CategoryItem
		{
			Category = Category.Valentine2025,
			Name = "バレンタイン",
			MissionEventType = GameEventType.Valentine2025,
			MainMissionID = MissionID.Valentine2025_Main,
			EventItemOID = "ValentineLove",
			MapBGType = BGType.Universe
		},
		new CategoryItem
		{
			Category = Category.Birthday2025_05,
			Name = "誕生日"
		},
		new CategoryItem
		{
			Category = Category.Reversal,
			Name = "突撃麻雀",
			MainMissionID = MissionID.Reversal_Item,
			MapBGType = BGType.Love
		},
		new CategoryItem
		{
			Category = Category.Birthday2025_14,
			Name = "誕生日"
		},
		new CategoryItem
		{
			Category = Category.DiaTicket,
			Name = "ダイアお迎え",
			IsJandriEnable = false,
			IsDispPlayCountWindow = false,
			MissionEventType = GameEventType.DiaTicket,
			MainMissionID = MissionID.DiaTicket_Vs
		},
		new CategoryItem
		{
			Category = Category.Birthday2025_15,
			Name = "誕生日",
			IsDispPlayCountWindow = false
		},
		new CategoryItem
		{
			Category = Category.Birthday2025_09,
			Name = "誕生日",
			IsDispPlayCountWindow = false
		},
		new CategoryItem
		{
			Category = Category.Anniv2025,
			Name = "2周年",
			IsDispPlayCountWindow = false,
			IsJandriEnable = false,
			MissionEventType = GameEventType.Anniv2025,
			MainMissionID = MissionID.Anniv2025_Login
		},
		new CategoryItem
		{
			Category = Category.WelcomeComeback4,
			Name = "ウェルカムカムバック",
			IsDispPlayCountWindow = false,
			IsJandriEnable = false,
			MissionEventType = GameEventType.WelcomeComeback4,
			MainMissionID = MissionID.WelcomeComeback4_Login,
			IsWelcomeComebackEvent = true
		},
		new CategoryItem
		{
			Category = Category.Aotenjo2025,
			Name = "青天井",
			IsDispPlayCountWindow = false
		},
		new CategoryItem
		{
			Category = Category.Birthday2025_16,
			Name = "誕生日",
			IsDispPlayCountWindow = false
		},
		new CategoryItem
		{
			Category = Category.Birthday2025_07,
			Name = "誕生日",
			IsDispPlayCountWindow = false
		},
		new CategoryItem
		{
			Category = Category.Janpon2,
			Name = "雀ポン",
			IsDispPlayCountWindow = false
		},
		new CategoryItem
		{
			Category = Category.Bemani002,
			Name = "BEMANI",
			MissionEventType = GameEventType.Bemani002,
			IsCharaMainMission = true
		},
		new CategoryItem
		{
			Category = Category.Blooming,
			Name = "お花見",
			MissionEventType = GameEventType.Blooming,
			MainMissionID = MissionID.Blooming_Vs,
			MapBGType = BGType.Sakura
		},
		new CategoryItem
		{
			Category = Category.Competition7,
			Name = "暗黒ダービー",
			MissionEventType = GameEventType.Competition7,
			MainMissionID = MissionID.Competition7_Vs,
			MapBGType = BGType.Stadium
		},
		new CategoryItem
		{
			Category = Category.GW2025,
			Name = "GW",
			IsDispPlayCountWindow = false,
			IsJandriEnable = false,
			MissionEventType = GameEventType.GW2025,
			MainMissionID = MissionID.GW2025_Login
		},
		new CategoryItem
		{
			Category = Category.CharaPai001,
			Name = "痛牌",
			IsDispPlayCountWindow = false
		},
		new CategoryItem
		{
			Category = Category.MeldBonus2,
			Name = "PCK",
			MainMissionID = MissionID.MeldBonus2_Item,
			MapBGType = BGType.Bill
		},
		new CategoryItem
		{
			Category = Category.Birthday2025_11_12,
			Name = "誕生日",
			IsDispPlayCountWindow = false
		},
		new CategoryItem
		{
			Category = Category.Birthday2025_10,
			Name = "誕生日",
			IsDispPlayCountWindow = false
		},
		new CategoryItem
		{
			Category = Category.Birthday2025_08,
			Name = "誕生日",
			IsDispPlayCountWindow = false
		},
		new CategoryItem
		{
			Category = Category.CharaPai002,
			Name = "痛牌",
			IsDispPlayCountWindow = false
		},
		new CategoryItem
		{
			Category = Category.DoubrielTicket,
			Name = "ダブリエルお迎え",
			IsJandriEnable = false,
			IsDispPlayCountWindow = false,
			MissionEventType = GameEventType.DoubrielTicket,
			MainMissionID = MissionID.DoubrielTicket_Vs
		},
		new CategoryItem
		{
			Category = Category.Birthday2025_03,
			Name = "誕生日",
			IsDispPlayCountWindow = false
		},
		new CategoryItem
		{
			Category = Category.Bomb,
			Name = "ボンバー卓",
			MainMissionID = MissionID.Bomb_Item,
			MapBGType = BGType.Universe
		},
		new CategoryItem
		{
			Category = Category.Poolside,
			Name = "プールサイド",
			MissionEventType = GameEventType.Poolside,
			MainMissionID = MissionID.Poolside_Vs,
			MapBGType = BGType.Universe
		},
		new CategoryItem
		{
			Category = Category.Ponnomichi,
			Name = "ぽんのみち",
			IsDispPlayCountWindow = false,
			IsJandriEnable = false,
			MissionEventType = GameEventType.Ponnomichi,
			MainMissionID = MissionID.Ponnomichi_Login,
			IsKonastaEnable = false
		},
		new CategoryItem
		{
			Category = Category.Bonodori001,
			Name = "盆踊り",
			IsDispPlayCountWindow = false
		},
		new CategoryItem
		{
			Category = Category.Birthday2025_06,
			Name = "誕生日",
			IsDispPlayCountWindow = false
		},
		new CategoryItem
		{
			Category = Category.WelcomeComeback5,
			Name = "ウェルカムカムバック",
			IsDispPlayCountWindow = false,
			IsJandriEnable = false,
			MissionEventType = GameEventType.WelcomeComeback5,
			MainMissionID = MissionID.WelcomeComeback5_Login,
			IsWelcomeComebackEvent = true
		},
		new CategoryItem
		{
			Category = Category.Competition8,
			Name = "メンタンピン",
			MissionEventType = GameEventType.Competition8,
			MainMissionID = MissionID.Competition8_Vs,
			MapBGType = BGType.Sky
		},
		new CategoryItem
		{
			Category = Category.Bonodori002,
			Name = "盆踊り",
			IsDispPlayCountWindow = false
		},
		new CategoryItem
		{
			Category = Category.Birthday2025_01,
			Name = "誕生日",
			IsDispPlayCountWindow = false
		},
		new CategoryItem
		{
			Category = Category.IppatsuTicket,
			Name = "イッパツお迎え",
			IsJandriEnable = false,
			IsDispPlayCountWindow = false,
			MissionEventType = GameEventType.IppatsuTicket,
			MainMissionID = MissionID.IppatsuTicket_Vs
		},
		new CategoryItem
		{
			Category = Category.Birthday2025_04,
			Name = "誕生日",
			IsDispPlayCountWindow = false
		},
		new CategoryItem
		{
			Category = Category.Bingo3,
			Name = "ビンゴ大会",
			MissionEventType = GameEventType.YakuBingo3,
			IsBingo = true,
			MainMissionID = MissionID.Bingo3_Item,
			MapBGType = BGType.Sky
		},
		new CategoryItem
		{
			Category = Category.HalfAnniv2025,
			Name = "2.5周年",
			IsDispPlayCountWindow = false,
			IsJandriEnable = false,
			MissionEventType = GameEventType.HalfAnniv2025,
			MainMissionID = MissionID.HalfAnniv2025_Login
		},
		new CategoryItem
		{
			Category = Category.Janpon3,
			Name = "雀ポン",
			IsDispPlayCountWindow = false
		},
		new CategoryItem
		{
			Category = Category.AllGreen,
			Name = "全緑卓",
			MainMissionID = MissionID.AllGreen_Item,
			MapBGType = BGType.Universe
		},
		new CategoryItem
		{
			Category = Category.Birthday2025_18,
			Name = "誕生日",
			IsDispPlayCountWindow = false
		},
		new CategoryItem
		{
			Category = Category.Birthday2025_02,
			Name = "誕生日",
			IsDispPlayCountWindow = false
		},
		new CategoryItem
		{
			Category = Category.AutumnLeaves,
			Name = "紅葉の帳",
			MissionEventType = GameEventType.AutumnLeaves,
			MainMissionID = MissionID.AutumnLeaves_Vs,
			MapBGType = BGType.Wagara
		},
		new CategoryItem
		{
			Category = Category.BowBow2025,
			Name = "タテナオリ家",
			IsDispPlayCountWindow = false,
			IsJandriEnable = false,
			MissionEventType = GameEventType.BowBow2025,
			MainMissionID = MissionID.BowBow2025_Login
		},
		new CategoryItem
		{
			Category = Category.Birthday2025_17,
			Name = "誕生日",
			IsDispPlayCountWindow = false
		},
		new CategoryItem
		{
			Category = Category.ShiroeTicket,
			Name = "シロヱお迎え",
			IsJandriEnable = false,
			IsDispPlayCountWindow = false,
			MissionEventType = GameEventType.ShiroeTicket,
			MainMissionID = MissionID.ShiroeTicket_Vs
		},
		new CategoryItem
		{
			Category = Category.DecorationSticker,
			Name = "デコレーション",
			MissionEventType = GameEventType.DecorationSticker,
			MainMissionID = MissionID.DecorationSticker_Vs,
			MapBGType = BGType.Love
		},
		new CategoryItem
		{
			Category = Category.MotherWars,
			Name = "マザーウォーズ",
			IsJandriEnable = false,
			IsDispPlayCountWindow = false,
			MissionEventType = GameEventType.MotherWars,
			MainMissionID = MissionID.MotherWars_Win
		},
		new CategoryItem
		{
			Category = Category.Anniv1000days,
			Name = "稼働1000日",
			IsDispPlayCountWindow = false
		},
		new CategoryItem
		{
			Category = Category.Omikuji2026,
			Name = "おみくじ",
			IsDispPlayCountWindow = false
		},
		new CategoryItem
		{
			Category = Category.ShopCompe001_1,
			Name = "最強店舗前半戦",
			MissionEventType = GameEventType.ShopCompetition001Mission1,
			MainMissionID = MissionID.ShopCompe001_1_Vs,
			MapBGType = BGType.Stadium
		},
		new CategoryItem
		{
			Category = Category.ShopCompe001_2,
			Name = "最強店舗後半戦",
			MissionEventType = GameEventType.ShopCompetition001Mission2,
			MainMissionID = MissionID.ShopCompe001_2_Vs,
			MapBGType = BGType.Stadium
		},
		new CategoryItem
		{
			Category = Category.Birthday2026_13,
			Name = "誕生日",
			IsDispPlayCountWindow = false
		},
		new CategoryItem
		{
			Category = Category.Valentine2026,
			Name = "チョコレートリーチ",
			MissionEventType = GameEventType.Valentine2026,
			MainMissionID = MissionID.Valentine2026_Main,
			EventItemOID = "ValentineLove2",
			MapBGType = BGType.Universe
		},
		new CategoryItem
		{
			Category = Category.Birthday2026_19,
			Name = "誕生日",
			IsDispPlayCountWindow = false
		},
		new CategoryItem
		{
			Category = Category.Birthday2026_05,
			Name = "誕生日",
			IsDispPlayCountWindow = false
		},
		new CategoryItem
		{
			Category = Category.Comeback2,
			Name = "革命のカタルシス",
			MainMissionID = MissionID.Comeback2_Item,
			MapBGType = BGType.Universe
		},
		new CategoryItem
		{
			Category = Category.Atomic,
			Name = "アトミックリーチ",
			MainMissionID = MissionID.Atomic_Item,
			MapBGType = BGType.Universe
		},
		new CategoryItem
		{
			Category = Category.Birthday2026_14,
			Name = "誕生日",
			IsDispPlayCountWindow = false
		},
		new CategoryItem
		{
			Category = Category.Birthday2026_15,
			Name = "誕生日",
			IsDispPlayCountWindow = false
		},
		new CategoryItem
		{
			Category = Category.JunTicket,
			Name = "ジュンお迎え",
			IsJandriEnable = false,
			IsDispPlayCountWindow = false,
			MissionEventType = GameEventType.JunTicket,
			MainMissionID = MissionID.JunTicket_Vs
		},
		new CategoryItem
		{
			Category = Category.Birthday2026_09,
			Name = "誕生日",
			IsDispPlayCountWindow = false
		},
		new CategoryItem
		{
			Category = Category.CharaPai003,
			Name = "痛牌",
			IsDispPlayCountWindow = false
		},
		new CategoryItem
		{
			Category = Category.Janpon4,
			Name = "雀ポン",
			IsDispPlayCountWindow = false
		},
		new CategoryItem
		{
			Category = Category.Anniv2026,
			Name = "3周年",
			IsDispPlayCountWindow = false,
			IsJandriEnable = false,
			MissionEventType = GameEventType.Anniv2026,
			MainMissionID = MissionID.Anniv2026_Login
		},
		new CategoryItem
		{
			Category = Category.Birthday2026_07,
			Name = "誕生日",
			IsDispPlayCountWindow = false
		},
		new CategoryItem
		{
			Category = Category.Birthday2026_16,
			Name = "誕生日",
			IsDispPlayCountWindow = false
		},
		new CategoryItem
		{
			Category = Category.Bemani003,
			Name = "BEMANI",
			MissionEventType = GameEventType.Bemani003,
			IsCharaMainMission = true
		},
		new CategoryItem
		{
			Category = Category.Aotenjo2026,
			Name = "青天井",
			IsDispPlayCountWindow = false
		},
		new CategoryItem
		{
			Category = Category.Spring2026,
			Name = "春祭り",
			MissionEventType = GameEventType.Spring2026,
			MainMissionID = MissionID.Spring2026_Vs,
			MapBGType = BGType.Wagara
		},
		new CategoryItem
		{
			Category = Category.Birthday2026_11_12,
			Name = "誕生日",
			IsDispPlayCountWindow = false
		},
		new CategoryItem
		{
			Category = Category.Birthday2026_10,
			Name = "誕生日",
			IsDispPlayCountWindow = false
		},
		new CategoryItem
		{
			Category = Category.SportsDay,
			Name = "大運動会",
			MissionEventType = GameEventType.SportsDay,
			MainMissionID = MissionID.SportsDay_Vs,
			MapBGType = BGType.Sky
		},
		new CategoryItem
		{
			Category = Category.RainyDay001,
			Name = "雨の日",
			IsDispPlayCountWindow = false
		},
		new CategoryItem
		{
			Category = Category.Competition9,
			Name = "オペレーション",
			MissionEventType = GameEventType.Competition9,
			MainMissionID = MissionID.Competition9_Vs,
			MapBGType = BGType.Stadium
		},
		new CategoryItem
		{
			Category = Category.WelcomeComeback6,
			Name = "ウェルカムカムバック",
			IsDispPlayCountWindow = false,
			IsJandriEnable = false,
			MissionEventType = GameEventType.WelcomeComeback6,
			MainMissionID = MissionID.WelcomeComeback6_Login,
			IsWelcomeComebackEvent = true
		},
		new CategoryItem
		{
			Category = Category.Birthday2026_08,
			Name = "誕生日",
			IsDispPlayCountWindow = false
		},
		new CategoryItem
		{
			Category = Category.ToukaTicket,
			Name = "トウカお迎え",
			IsJandriEnable = false,
			IsDispPlayCountWindow = false,
			MissionEventType = GameEventType.ToukaTicket,
			MainMissionID = MissionID.ToukaTicket_Vs
		},
		new CategoryItem
		{
			Category = Category.Birthday2026_20,
			Name = "誕生日",
			IsDispPlayCountWindow = false
		},
		new CategoryItem
		{
			Category = Category.Birthday2026_03,
			Name = "誕生日",
			IsDispPlayCountWindow = false
		},
		new CategoryItem
		{
			Category = Category.RainyDay002,
			Name = "雨の日",
			IsDispPlayCountWindow = false
		}
	};

	public static List<CategoryItem> MissionEvents => _item.Where((CategoryItem x) => x.MissionEventType != 0 && !x.IsBingo).ToList();

	public static List<CategoryItem> BingoEvents => _item.Where((CategoryItem x) => x.MissionEventType != 0 && x.IsBingo).ToList();

	public static List<CategoryItem> GetActiveTab()
	{
		List<CategoryItem> list = ActiveBirthdayMission();
		List<CategoryItem> list2 = (from x in GetActiveCategories()
			where !x.Category.ToString().Contains("Birthday20")
			select x).ToList();
		if (list != null && list.Count > 0)
		{
			list2.Add(_item.Find((CategoryItem x) => x.Category == Category.Birthday));
		}
		return list2.OrderBy((CategoryItem x) => x.Category).ToList();
	}

	public static List<CategoryItem> GetActiveCategories()
	{
		return _item.Where((CategoryItem x) => x.IsEnable() && !x.IsJudgeCategory).ToList();
	}

	public static List<CategoryItem> ActiveBirthdayMission()
	{
		return _item.Where((CategoryItem x) => x.IsEnable() && !x.IsJudgeCategory && x.Category.ToString().Contains("Birthday20")).ToList();
	}

	public static string GetTimeStamp(UpdateInterval interval)
	{
		DateTimeOffset dateTimeOffset = interval switch
		{
			UpdateInterval.Daily => GetItem(Category.JudgmentDaily)?.BeginDateTime ?? DateTimeOffset.MinValue, 
			UpdateInterval.Weekly => GetItem(Category.JudgmentWeekly)?.BeginDateTime ?? DateTimeOffset.MinValue, 
			_ => DateTimeOffset.MinValue, 
		};
		if (!(dateTimeOffset == DateTimeOffset.MinValue))
		{
			return dateTimeOffset.ToString("yyyy/MM/dd HH:mm:ss");
		}
		return "";
	}

	public static CategoryItem GetItem(Category category)
	{
		return _item.Find((CategoryItem x) => x.Category == category);
	}

	public static CategoryItem GetItem(GameEventType eventType)
	{
		return _item.Find((CategoryItem x) => x.MissionEventType == eventType);
	}
}
