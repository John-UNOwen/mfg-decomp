using System.Collections.Generic;
using System.Linq;
using MFG.MasterGameData;

namespace MFG.Types.Customize.CustomizeItem;

public static class ItemIDExtentions
{
	private static List<ItemID> s_tileSeMaterialPath = new List<ItemID>
	{
		ItemID.TileSurface006,
		ItemID.TileSurface008,
		ItemID.TileSurface009,
		ItemID.TileSurface010,
		ItemID.TileSurface011,
		ItemID.TileSurface012,
		ItemID.TileSurface013,
		ItemID.TileSurface014,
		ItemID.TileSurface015,
		ItemID.TileSurface016,
		ItemID.TileSurface017,
		ItemID.TileSurface018,
		ItemID.TileSurface019,
		ItemID.TileSurface020,
		ItemID.TileSurface021,
		ItemID.TileSurface022,
		ItemID.TileSurface023,
		ItemID.TileSurface024,
		ItemID.TileSurface025,
		ItemID.TileSurface026,
		ItemID.TileSurface027,
		ItemID.TileSurface028,
		ItemID.TileSurface029
	};

	private static List<ItemID> s_tileCubeMaterialPath = new List<ItemID>
	{
		ItemID.TileSurface008,
		ItemID.TileSurface025
	};

	private static List<ItemID> s_tileKiraMaterialPath = new List<ItemID>
	{
		ItemID.TileSurface009,
		ItemID.TileSurface010,
		ItemID.TileSurface011,
		ItemID.TileSurface012,
		ItemID.TileSurface013,
		ItemID.TileSurface014,
		ItemID.TileSurface015,
		ItemID.TileSurface016,
		ItemID.TileSurface017,
		ItemID.TileSurface018,
		ItemID.TileSurface019,
		ItemID.TileSurface020,
		ItemID.TileSurface021,
		ItemID.TileSurface022,
		ItemID.TileSurface023,
		ItemID.TileSurface024,
		ItemID.TileSurface025,
		ItemID.TileSurface026,
		ItemID.TileSurface027,
		ItemID.TileSurface028,
		ItemID.TileSurface029
	};

	private static Dictionary<ItemID, (CharaType charaType, string bgmLabel)> s_labelDic = new Dictionary<ItemID, (CharaType, string)>
	{
		{
			ItemID.ReachBgm001,
			(CharaType.Chara01, "BGM_REACH1")
		},
		{
			ItemID.ReachBgm002,
			(CharaType.Chara01, "BGM_G01_REACH_1")
		},
		{
			ItemID.ReachBgm003,
			(CharaType.Chara01, "BGM_G01_REACH_3")
		},
		{
			ItemID.ReachBgm004,
			(CharaType.Chara01, "BGM_G01_REACH_2")
		},
		{
			ItemID.ReachBgm005,
			(CharaType.Chara01, "BGM_G01_REACH_4")
		},
		{
			ItemID.ReachBgm006,
			(CharaType.Chara02, "BGM_G02_REACH_1")
		},
		{
			ItemID.ReachBgm007,
			(CharaType.Chara02, "BGM_G02_REACH_3")
		},
		{
			ItemID.ReachBgm008,
			(CharaType.Chara02, "BGM_G02_REACH_2")
		},
		{
			ItemID.ReachBgm009,
			(CharaType.Chara02, "BGM_G02_REACH_4")
		},
		{
			ItemID.ReachBgm010,
			(CharaType.Chara03, "BGM_G03_REACH_1")
		},
		{
			ItemID.ReachBgm011,
			(CharaType.Chara03, "BGM_G03_REACH_3")
		},
		{
			ItemID.ReachBgm012,
			(CharaType.Chara03, "BGM_G03_REACH_2")
		},
		{
			ItemID.ReachBgm013,
			(CharaType.Chara03, "BGM_G03_REACH_4")
		},
		{
			ItemID.ReachBgm014,
			(CharaType.Chara04, "BGM_G04_REACH_1")
		},
		{
			ItemID.ReachBgm015,
			(CharaType.Chara04, "BGM_G04_REACH_3")
		},
		{
			ItemID.ReachBgm016,
			(CharaType.Chara04, "BGM_G04_REACH_2")
		},
		{
			ItemID.ReachBgm017,
			(CharaType.Chara04, "BGM_G04_REACH_4")
		},
		{
			ItemID.ReachBgm018,
			(CharaType.Chara05, "BGM_G05_REACH_1")
		},
		{
			ItemID.ReachBgm019,
			(CharaType.Chara05, "BGM_G05_REACH_3")
		},
		{
			ItemID.ReachBgm020,
			(CharaType.Chara05, "BGM_G05_REACH_2")
		},
		{
			ItemID.ReachBgm021,
			(CharaType.Chara05, "BGM_G05_REACH_4")
		},
		{
			ItemID.ReachBgm022,
			(CharaType.Chara06, "BGM_G06_REACH_1")
		},
		{
			ItemID.ReachBgm023,
			(CharaType.Chara06, "BGM_G06_REACH_3")
		},
		{
			ItemID.ReachBgm024,
			(CharaType.Chara06, "BGM_G06_REACH_2")
		},
		{
			ItemID.ReachBgm025,
			(CharaType.Chara06, "BGM_G06_REACH_4")
		},
		{
			ItemID.ReachBgm026,
			(CharaType.Chara07, "BGM_G07_REACH_1")
		},
		{
			ItemID.ReachBgm027,
			(CharaType.Chara07, "BGM_G07_REACH_3")
		},
		{
			ItemID.ReachBgm028,
			(CharaType.Chara07, "BGM_G07_REACH_2")
		},
		{
			ItemID.ReachBgm029,
			(CharaType.Chara07, "BGM_G07_REACH_4")
		},
		{
			ItemID.ReachBgm030,
			(CharaType.Chara08, "BGM_G08_REACH_1")
		},
		{
			ItemID.ReachBgm031,
			(CharaType.Chara08, "BGM_G08_REACH_3")
		},
		{
			ItemID.ReachBgm032,
			(CharaType.Chara08, "BGM_G08_REACH_2")
		},
		{
			ItemID.ReachBgm033,
			(CharaType.Chara08, "BGM_G08_REACH_4")
		},
		{
			ItemID.ReachBgm034,
			(CharaType.Chara09, "BGM_G09_REACH_1")
		},
		{
			ItemID.ReachBgm035,
			(CharaType.Chara09, "BGM_G09_REACH_3")
		},
		{
			ItemID.ReachBgm036,
			(CharaType.Chara09, "BGM_G09_REACH_2")
		},
		{
			ItemID.ReachBgm037,
			(CharaType.Chara09, "BGM_G09_REACH_4")
		},
		{
			ItemID.ReachBgm038,
			(CharaType.Chara10, "BGM_G10_REACH_1")
		},
		{
			ItemID.ReachBgm039,
			(CharaType.Chara10, "BGM_G10_REACH_3")
		},
		{
			ItemID.ReachBgm040,
			(CharaType.Chara10, "BGM_G10_REACH_2")
		},
		{
			ItemID.ReachBgm041,
			(CharaType.Chara10, "BGM_G10_REACH_4")
		},
		{
			ItemID.ReachBgm042,
			(CharaType.Chara11, "BGM_G11_REACH_1")
		},
		{
			ItemID.ReachBgm043,
			(CharaType.Chara11, "BGM_G11_REACH_3")
		},
		{
			ItemID.ReachBgm044,
			(CharaType.Chara11, "BGM_G11_REACH_2")
		},
		{
			ItemID.ReachBgm045,
			(CharaType.Chara11, "BGM_G11_REACH_4")
		},
		{
			ItemID.ReachBgm046,
			(CharaType.Chara12, "BGM_G12_REACH_1")
		},
		{
			ItemID.ReachBgm047,
			(CharaType.Chara12, "BGM_G12_REACH_3")
		},
		{
			ItemID.ReachBgm048,
			(CharaType.Chara12, "BGM_G12_REACH_2")
		},
		{
			ItemID.ReachBgm049,
			(CharaType.Chara12, "BGM_G12_REACH_4")
		},
		{
			ItemID.ReachBgm050,
			(CharaType.Chara01, "BGM_G01_REACH_BM1_1")
		},
		{
			ItemID.ReachBgm051,
			(CharaType.Chara01, "BGM_G01_REACH_BM1_2")
		},
		{
			ItemID.ReachBgm052,
			(CharaType.Chara02, "BGM_G02_REACH_BM1_1")
		},
		{
			ItemID.ReachBgm053,
			(CharaType.Chara02, "BGM_G02_REACH_BM1_2")
		},
		{
			ItemID.ReachBgm054,
			(CharaType.Chara03, "BGM_G03_REACH_BM1_1")
		},
		{
			ItemID.ReachBgm055,
			(CharaType.Chara03, "BGM_G03_REACH_BM1_2")
		},
		{
			ItemID.ReachBgm056,
			(CharaType.Chara04, "BGM_G04_REACH_BM1_1")
		},
		{
			ItemID.ReachBgm057,
			(CharaType.Chara04, "BGM_G04_REACH_BM1_2")
		},
		{
			ItemID.ReachBgm058,
			(CharaType.Chara05, "BGM_G05_REACH_BM1_1")
		},
		{
			ItemID.ReachBgm059,
			(CharaType.Chara05, "BGM_G05_REACH_BM1_2")
		},
		{
			ItemID.ReachBgm060,
			(CharaType.Chara06, "BGM_G06_REACH_BM1_1")
		},
		{
			ItemID.ReachBgm061,
			(CharaType.Chara06, "BGM_G06_REACH_BM1_2")
		},
		{
			ItemID.ReachBgm062,
			(CharaType.Chara07, "BGM_G07_REACH_BM1_1")
		},
		{
			ItemID.ReachBgm063,
			(CharaType.Chara07, "BGM_G07_REACH_BM1_2")
		},
		{
			ItemID.ReachBgm064,
			(CharaType.Chara08, "BGM_G08_REACH_BM1_1")
		},
		{
			ItemID.ReachBgm065,
			(CharaType.Chara08, "BGM_G08_REACH_BM1_2")
		},
		{
			ItemID.ReachBgm066,
			(CharaType.Chara09, "BGM_G09_REACH_BM1_1")
		},
		{
			ItemID.ReachBgm067,
			(CharaType.Chara09, "BGM_G09_REACH_BM1_2")
		},
		{
			ItemID.ReachBgm068,
			(CharaType.Chara10, "BGM_G10_REACH_BM1_1")
		},
		{
			ItemID.ReachBgm069,
			(CharaType.Chara10, "BGM_G10_REACH_BM1_2")
		},
		{
			ItemID.ReachBgm070,
			(CharaType.Chara11, "BGM_G11_REACH_BM1_1")
		},
		{
			ItemID.ReachBgm071,
			(CharaType.Chara11, "BGM_G11_REACH_BM1_2")
		},
		{
			ItemID.ReachBgm072,
			(CharaType.Chara12, "BGM_G12_REACH_BM1_1")
		},
		{
			ItemID.ReachBgm073,
			(CharaType.Chara12, "BGM_G12_REACH_BM1_2")
		},
		{
			ItemID.ReachBgm074,
			(CharaType.Chara13, "BGM_G13_REACH_1")
		},
		{
			ItemID.ReachBgm075,
			(CharaType.Chara13, "BGM_G13_REACH_3")
		},
		{
			ItemID.ReachBgm076,
			(CharaType.Chara13, "BGM_G13_REACH_2")
		},
		{
			ItemID.ReachBgm077,
			(CharaType.Chara13, "BGM_G13_REACH_4")
		},
		{
			ItemID.ReachBgm078,
			(CharaType.Chara01, "BGM_G01_REACH_TH1_1")
		},
		{
			ItemID.ReachBgm079,
			(CharaType.Chara01, "BGM_G01_REACH_TH1_2")
		},
		{
			ItemID.ReachBgm080,
			(CharaType.Chara02, "BGM_G02_REACH_TH1_1")
		},
		{
			ItemID.ReachBgm081,
			(CharaType.Chara02, "BGM_G02_REACH_TH1_2")
		},
		{
			ItemID.ReachBgm082,
			(CharaType.Chara03, "BGM_G03_REACH_TH1_1")
		},
		{
			ItemID.ReachBgm083,
			(CharaType.Chara03, "BGM_G03_REACH_TH1_2")
		},
		{
			ItemID.ReachBgm084,
			(CharaType.Chara04, "BGM_G04_REACH_TH1_1")
		},
		{
			ItemID.ReachBgm085,
			(CharaType.Chara04, "BGM_G04_REACH_TH1_2")
		},
		{
			ItemID.ReachBgm086,
			(CharaType.Chara05, "BGM_G05_REACH_TH1_1")
		},
		{
			ItemID.ReachBgm087,
			(CharaType.Chara05, "BGM_G05_REACH_TH1_2")
		},
		{
			ItemID.ReachBgm088,
			(CharaType.Chara06, "BGM_G06_REACH_TH1_1")
		},
		{
			ItemID.ReachBgm089,
			(CharaType.Chara06, "BGM_G06_REACH_TH1_2")
		},
		{
			ItemID.ReachBgm090,
			(CharaType.Chara07, "BGM_G07_REACH_TH1_1")
		},
		{
			ItemID.ReachBgm091,
			(CharaType.Chara07, "BGM_G07_REACH_TH1_2")
		},
		{
			ItemID.ReachBgm092,
			(CharaType.Chara08, "BGM_G08_REACH_TH1_1")
		},
		{
			ItemID.ReachBgm093,
			(CharaType.Chara08, "BGM_G08_REACH_TH1_2")
		},
		{
			ItemID.ReachBgm094,
			(CharaType.Chara09, "BGM_G09_REACH_TH1_1")
		},
		{
			ItemID.ReachBgm095,
			(CharaType.Chara09, "BGM_G09_REACH_TH1_2")
		},
		{
			ItemID.ReachBgm096,
			(CharaType.Chara10, "BGM_G10_REACH_TH1_1")
		},
		{
			ItemID.ReachBgm097,
			(CharaType.Chara10, "BGM_G10_REACH_TH1_2")
		},
		{
			ItemID.ReachBgm098,
			(CharaType.Chara11, "BGM_G11_REACH_TH1_1")
		},
		{
			ItemID.ReachBgm099,
			(CharaType.Chara11, "BGM_G11_REACH_TH1_2")
		},
		{
			ItemID.ReachBgm100,
			(CharaType.Chara12, "BGM_G12_REACH_TH1_1")
		},
		{
			ItemID.ReachBgm101,
			(CharaType.Chara12, "BGM_G12_REACH_TH1_2")
		},
		{
			ItemID.ReachBgm102,
			(CharaType.Chara13, "BGM_G13_REACH_TH1_1")
		},
		{
			ItemID.ReachBgm103,
			(CharaType.Chara13, "BGM_G13_REACH_TH1_2")
		},
		{
			ItemID.ReachBgm104,
			(CharaType.Chara14, "BGM_G14_REACH_1")
		},
		{
			ItemID.ReachBgm105,
			(CharaType.Chara14, "BGM_G14_REACH_3")
		},
		{
			ItemID.ReachBgm106,
			(CharaType.Chara14, "BGM_G14_REACH_2")
		},
		{
			ItemID.ReachBgm107,
			(CharaType.Chara14, "BGM_G14_REACH_4")
		},
		{
			ItemID.ReachBgm108,
			(CharaType.Chara15, "BGM_G15_REACH_1")
		},
		{
			ItemID.ReachBgm109,
			(CharaType.Chara15, "BGM_G15_REACH_3")
		},
		{
			ItemID.ReachBgm110,
			(CharaType.Chara15, "BGM_G15_REACH_2")
		},
		{
			ItemID.ReachBgm111,
			(CharaType.Chara15, "BGM_G15_REACH_4")
		},
		{
			ItemID.ReachBgm112,
			(CharaType.Chara16, "BGM_G16_REACH_1")
		},
		{
			ItemID.ReachBgm113,
			(CharaType.Chara16, "BGM_G16_REACH_3")
		},
		{
			ItemID.ReachBgm114,
			(CharaType.Chara16, "BGM_G16_REACH_2")
		},
		{
			ItemID.ReachBgm115,
			(CharaType.Chara16, "BGM_G16_REACH_4")
		},
		{
			ItemID.ReachBgm116,
			(CharaType.Chara01, "BGM_G01_REACH_BM2_1")
		},
		{
			ItemID.ReachBgm117,
			(CharaType.Chara01, "BGM_G01_REACH_BM2_2")
		},
		{
			ItemID.ReachBgm118,
			(CharaType.Chara02, "BGM_G02_REACH_BM2_1")
		},
		{
			ItemID.ReachBgm119,
			(CharaType.Chara02, "BGM_G02_REACH_BM2_2")
		},
		{
			ItemID.ReachBgm120,
			(CharaType.Chara03, "BGM_G03_REACH_BM2_1")
		},
		{
			ItemID.ReachBgm121,
			(CharaType.Chara03, "BGM_G03_REACH_BM2_2")
		},
		{
			ItemID.ReachBgm122,
			(CharaType.Chara04, "BGM_G04_REACH_BM2_1")
		},
		{
			ItemID.ReachBgm123,
			(CharaType.Chara04, "BGM_G04_REACH_BM2_2")
		},
		{
			ItemID.ReachBgm124,
			(CharaType.Chara05, "BGM_G05_REACH_BM2_1")
		},
		{
			ItemID.ReachBgm125,
			(CharaType.Chara05, "BGM_G05_REACH_BM2_2")
		},
		{
			ItemID.ReachBgm126,
			(CharaType.Chara06, "BGM_G06_REACH_BM2_1")
		},
		{
			ItemID.ReachBgm127,
			(CharaType.Chara06, "BGM_G06_REACH_BM2_2")
		},
		{
			ItemID.ReachBgm128,
			(CharaType.Chara07, "BGM_G07_REACH_BM2_1")
		},
		{
			ItemID.ReachBgm129,
			(CharaType.Chara07, "BGM_G07_REACH_BM2_2")
		},
		{
			ItemID.ReachBgm130,
			(CharaType.Chara08, "BGM_G08_REACH_BM2_1")
		},
		{
			ItemID.ReachBgm131,
			(CharaType.Chara08, "BGM_G08_REACH_BM2_2")
		},
		{
			ItemID.ReachBgm132,
			(CharaType.Chara09, "BGM_G09_REACH_BM2_1")
		},
		{
			ItemID.ReachBgm133,
			(CharaType.Chara09, "BGM_G09_REACH_BM2_2")
		},
		{
			ItemID.ReachBgm134,
			(CharaType.Chara10, "BGM_G10_REACH_BM2_1")
		},
		{
			ItemID.ReachBgm135,
			(CharaType.Chara10, "BGM_G10_REACH_BM2_2")
		},
		{
			ItemID.ReachBgm136,
			(CharaType.Chara11, "BGM_G11_REACH_BM2_1")
		},
		{
			ItemID.ReachBgm137,
			(CharaType.Chara11, "BGM_G11_REACH_BM2_2")
		},
		{
			ItemID.ReachBgm138,
			(CharaType.Chara12, "BGM_G12_REACH_BM2_1")
		},
		{
			ItemID.ReachBgm139,
			(CharaType.Chara12, "BGM_G12_REACH_BM2_2")
		},
		{
			ItemID.ReachBgm140,
			(CharaType.Chara13, "BGM_G13_REACH_BM2_1")
		},
		{
			ItemID.ReachBgm141,
			(CharaType.Chara13, "BGM_G13_REACH_BM2_2")
		},
		{
			ItemID.ReachBgm142,
			(CharaType.Chara14, "BGM_G14_REACH_BM2_1")
		},
		{
			ItemID.ReachBgm143,
			(CharaType.Chara14, "BGM_G14_REACH_BM2_2")
		},
		{
			ItemID.ReachBgm144,
			(CharaType.Chara15, "BGM_G15_REACH_BM2_1")
		},
		{
			ItemID.ReachBgm145,
			(CharaType.Chara15, "BGM_G15_REACH_BM2_2")
		},
		{
			ItemID.ReachBgm146,
			(CharaType.Chara16, "BGM_G16_REACH_BM2_1")
		},
		{
			ItemID.ReachBgm147,
			(CharaType.Chara16, "BGM_G16_REACH_BM2_2")
		},
		{
			ItemID.ReachBgm148,
			(CharaType.Chara01, "BGM_G01_REACH_6")
		},
		{
			ItemID.ReachBgm149,
			(CharaType.Chara01, "BGM_G01_REACH_8")
		},
		{
			ItemID.ReachBgm150,
			(CharaType.Chara01, "BGM_G01_REACH_7")
		},
		{
			ItemID.ReachBgm151,
			(CharaType.Chara01, "BGM_G01_REACH_9")
		},
		{
			ItemID.ReachBgm152,
			(CharaType.Chara02, "BGM_G02_REACH_6")
		},
		{
			ItemID.ReachBgm153,
			(CharaType.Chara02, "BGM_G02_REACH_8")
		},
		{
			ItemID.ReachBgm154,
			(CharaType.Chara02, "BGM_G02_REACH_7")
		},
		{
			ItemID.ReachBgm155,
			(CharaType.Chara02, "BGM_G02_REACH_9")
		},
		{
			ItemID.ReachBgm156,
			(CharaType.Chara17, "BGM_G17_REACH_1")
		},
		{
			ItemID.ReachBgm157,
			(CharaType.Chara17, "BGM_G17_REACH_3")
		},
		{
			ItemID.ReachBgm158,
			(CharaType.Chara17, "BGM_G17_REACH_2")
		},
		{
			ItemID.ReachBgm159,
			(CharaType.Chara17, "BGM_G17_REACH_4")
		},
		{
			ItemID.ReachBgm160,
			(CharaType.Chara05, "BGM_G05_REACH_6")
		},
		{
			ItemID.ReachBgm161,
			(CharaType.Chara05, "BGM_G05_REACH_8")
		},
		{
			ItemID.ReachBgm162,
			(CharaType.Chara05, "BGM_G05_REACH_7")
		},
		{
			ItemID.ReachBgm163,
			(CharaType.Chara05, "BGM_G05_REACH_9")
		},
		{
			ItemID.ReachBgm164,
			(CharaType.Chara04, "BGM_G04_REACH_6")
		},
		{
			ItemID.ReachBgm165,
			(CharaType.Chara04, "BGM_G04_REACH_8")
		},
		{
			ItemID.ReachBgm166,
			(CharaType.Chara04, "BGM_G04_REACH_7")
		},
		{
			ItemID.ReachBgm167,
			(CharaType.Chara04, "BGM_G04_REACH_9")
		},
		{
			ItemID.ReachBgm168,
			(CharaType.Chara18, "BGM_G18_REACH_1")
		},
		{
			ItemID.ReachBgm169,
			(CharaType.Chara18, "BGM_G18_REACH_3")
		},
		{
			ItemID.ReachBgm170,
			(CharaType.Chara18, "BGM_G18_REACH_2")
		},
		{
			ItemID.ReachBgm171,
			(CharaType.Chara18, "BGM_G18_REACH_4")
		},
		{
			ItemID.ReachBgm172,
			(CharaType.Chara08, "BGM_G08_REACH_6")
		},
		{
			ItemID.ReachBgm173,
			(CharaType.Chara08, "BGM_G08_REACH_8")
		},
		{
			ItemID.ReachBgm174,
			(CharaType.Chara08, "BGM_G08_REACH_7")
		},
		{
			ItemID.ReachBgm175,
			(CharaType.Chara08, "BGM_G08_REACH_9")
		},
		{
			ItemID.ReachBgm176,
			(CharaType.Chara19, "BGM_G19_REACH_1")
		},
		{
			ItemID.ReachBgm177,
			(CharaType.Chara19, "BGM_G19_REACH_3")
		},
		{
			ItemID.ReachBgm178,
			(CharaType.Chara19, "BGM_G19_REACH_2")
		},
		{
			ItemID.ReachBgm179,
			(CharaType.Chara19, "BGM_G19_REACH_4")
		},
		{
			ItemID.ReachBgm180,
			(CharaType.Chara06, "BGM_G06_REACH_6")
		},
		{
			ItemID.ReachBgm181,
			(CharaType.Chara06, "BGM_G06_REACH_8")
		},
		{
			ItemID.ReachBgm182,
			(CharaType.Chara06, "BGM_G06_REACH_7")
		},
		{
			ItemID.ReachBgm183,
			(CharaType.Chara06, "BGM_G06_REACH_9")
		},
		{
			ItemID.ReachBgm184,
			(CharaType.Chara20, "BGM_G20_REACH_1")
		},
		{
			ItemID.ReachBgm185,
			(CharaType.Chara20, "BGM_G20_REACH_3")
		},
		{
			ItemID.ReachBgm186,
			(CharaType.Chara20, "BGM_G20_REACH_2")
		},
		{
			ItemID.ReachBgm187,
			(CharaType.Chara20, "BGM_G20_REACH_4")
		},
		{
			ItemID.ReachBgm188,
			(CharaType.Chara01, "BGM_G01_REACH_BM3_1")
		},
		{
			ItemID.ReachBgm189,
			(CharaType.Chara01, "BGM_G01_REACH_BM3_2")
		},
		{
			ItemID.ReachBgm190,
			(CharaType.Chara02, "BGM_G02_REACH_BM3_1")
		},
		{
			ItemID.ReachBgm191,
			(CharaType.Chara02, "BGM_G02_REACH_BM3_2")
		},
		{
			ItemID.ReachBgm192,
			(CharaType.Chara03, "BGM_G03_REACH_BM3_1")
		},
		{
			ItemID.ReachBgm193,
			(CharaType.Chara03, "BGM_G03_REACH_BM3_2")
		},
		{
			ItemID.ReachBgm194,
			(CharaType.Chara04, "BGM_G04_REACH_BM3_1")
		},
		{
			ItemID.ReachBgm195,
			(CharaType.Chara04, "BGM_G04_REACH_BM3_2")
		},
		{
			ItemID.ReachBgm196,
			(CharaType.Chara05, "BGM_G05_REACH_BM3_1")
		},
		{
			ItemID.ReachBgm197,
			(CharaType.Chara05, "BGM_G05_REACH_BM3_2")
		},
		{
			ItemID.ReachBgm198,
			(CharaType.Chara06, "BGM_G06_REACH_BM3_1")
		},
		{
			ItemID.ReachBgm199,
			(CharaType.Chara06, "BGM_G06_REACH_BM3_2")
		},
		{
			ItemID.ReachBgm200,
			(CharaType.Chara07, "BGM_G07_REACH_BM3_1")
		},
		{
			ItemID.ReachBgm201,
			(CharaType.Chara07, "BGM_G07_REACH_BM3_2")
		},
		{
			ItemID.ReachBgm202,
			(CharaType.Chara08, "BGM_G08_REACH_BM3_1")
		},
		{
			ItemID.ReachBgm203,
			(CharaType.Chara08, "BGM_G08_REACH_BM3_2")
		},
		{
			ItemID.ReachBgm204,
			(CharaType.Chara09, "BGM_G09_REACH_BM3_1")
		},
		{
			ItemID.ReachBgm205,
			(CharaType.Chara09, "BGM_G09_REACH_BM3_2")
		},
		{
			ItemID.ReachBgm206,
			(CharaType.Chara10, "BGM_G10_REACH_BM3_1")
		},
		{
			ItemID.ReachBgm207,
			(CharaType.Chara10, "BGM_G10_REACH_BM3_2")
		},
		{
			ItemID.ReachBgm208,
			(CharaType.Chara11, "BGM_G11_REACH_BM3_1")
		},
		{
			ItemID.ReachBgm209,
			(CharaType.Chara11, "BGM_G11_REACH_BM3_2")
		},
		{
			ItemID.ReachBgm210,
			(CharaType.Chara12, "BGM_G12_REACH_BM3_1")
		},
		{
			ItemID.ReachBgm211,
			(CharaType.Chara12, "BGM_G12_REACH_BM3_2")
		},
		{
			ItemID.ReachBgm212,
			(CharaType.Chara13, "BGM_G13_REACH_BM3_1")
		},
		{
			ItemID.ReachBgm213,
			(CharaType.Chara13, "BGM_G13_REACH_BM3_2")
		},
		{
			ItemID.ReachBgm214,
			(CharaType.Chara14, "BGM_G14_REACH_BM3_1")
		},
		{
			ItemID.ReachBgm215,
			(CharaType.Chara14, "BGM_G14_REACH_BM3_2")
		},
		{
			ItemID.ReachBgm216,
			(CharaType.Chara15, "BGM_G15_REACH_BM3_1")
		},
		{
			ItemID.ReachBgm217,
			(CharaType.Chara15, "BGM_G15_REACH_BM3_2")
		},
		{
			ItemID.ReachBgm218,
			(CharaType.Chara16, "BGM_G16_REACH_BM3_1")
		},
		{
			ItemID.ReachBgm219,
			(CharaType.Chara16, "BGM_G16_REACH_BM3_2")
		},
		{
			ItemID.ReachBgm220,
			(CharaType.Chara17, "BGM_G17_REACH_BM3_1")
		},
		{
			ItemID.ReachBgm221,
			(CharaType.Chara17, "BGM_G17_REACH_BM3_2")
		},
		{
			ItemID.ReachBgm222,
			(CharaType.Chara18, "BGM_G18_REACH_BM3_1")
		},
		{
			ItemID.ReachBgm223,
			(CharaType.Chara18, "BGM_G18_REACH_BM3_2")
		},
		{
			ItemID.ReachBgm224,
			(CharaType.Chara19, "BGM_G19_REACH_BM3_1")
		},
		{
			ItemID.ReachBgm225,
			(CharaType.Chara19, "BGM_G19_REACH_BM3_2")
		},
		{
			ItemID.ReachBgm226,
			(CharaType.Chara20, "BGM_G20_REACH_BM3_1")
		},
		{
			ItemID.ReachBgm227,
			(CharaType.Chara20, "BGM_G20_REACH_BM3_2")
		},
		{
			ItemID.ReachBgm228,
			(CharaType.Chara07, "BGM_G07_REACH_6")
		},
		{
			ItemID.ReachBgm229,
			(CharaType.Chara07, "BGM_G07_REACH_8")
		},
		{
			ItemID.ReachBgm230,
			(CharaType.Chara07, "BGM_G07_REACH_7")
		},
		{
			ItemID.ReachBgm231,
			(CharaType.Chara07, "BGM_G07_REACH_9")
		},
		{
			ItemID.ReachBgm232,
			(CharaType.Chara21, "BGM_G21_REACH_1")
		},
		{
			ItemID.ReachBgm233,
			(CharaType.Chara21, "BGM_G21_REACH_3")
		},
		{
			ItemID.ReachBgm234,
			(CharaType.Chara21, "BGM_G21_REACH_2")
		},
		{
			ItemID.ReachBgm235,
			(CharaType.Chara21, "BGM_G21_REACH_4")
		}
	};

	private static Dictionary<ItemID, string> s_jingleDic = new Dictionary<ItemID, string>
	{
		{
			ItemID.ReachBgm050,
			"BGM_G01_REACH_BM1_3"
		},
		{
			ItemID.ReachBgm051,
			"BGM_G01_REACH_BM1_3"
		},
		{
			ItemID.ReachBgm052,
			"BGM_G02_REACH_BM1_3"
		},
		{
			ItemID.ReachBgm053,
			"BGM_G02_REACH_BM1_3"
		},
		{
			ItemID.ReachBgm054,
			"BGM_G03_REACH_BM1_3"
		},
		{
			ItemID.ReachBgm055,
			"BGM_G03_REACH_BM1_3"
		},
		{
			ItemID.ReachBgm056,
			"BGM_G04_REACH_BM1_3"
		},
		{
			ItemID.ReachBgm057,
			"BGM_G04_REACH_BM1_3"
		},
		{
			ItemID.ReachBgm058,
			"BGM_G05_REACH_BM1_3"
		},
		{
			ItemID.ReachBgm059,
			"BGM_G05_REACH_BM1_3"
		},
		{
			ItemID.ReachBgm060,
			"BGM_G06_REACH_BM1_3"
		},
		{
			ItemID.ReachBgm061,
			"BGM_G06_REACH_BM1_3"
		},
		{
			ItemID.ReachBgm062,
			"BGM_G07_REACH_BM1_3"
		},
		{
			ItemID.ReachBgm063,
			"BGM_G07_REACH_BM1_3"
		},
		{
			ItemID.ReachBgm064,
			"BGM_G08_REACH_BM1_3"
		},
		{
			ItemID.ReachBgm065,
			"BGM_G08_REACH_BM1_3"
		},
		{
			ItemID.ReachBgm066,
			"BGM_G09_REACH_BM1_3"
		},
		{
			ItemID.ReachBgm067,
			"BGM_G09_REACH_BM1_3"
		},
		{
			ItemID.ReachBgm068,
			"BGM_G10_REACH_BM1_3"
		},
		{
			ItemID.ReachBgm069,
			"BGM_G10_REACH_BM1_3"
		},
		{
			ItemID.ReachBgm070,
			"BGM_G11_REACH_BM1_3"
		},
		{
			ItemID.ReachBgm071,
			"BGM_G11_REACH_BM1_3"
		},
		{
			ItemID.ReachBgm072,
			"BGM_G12_REACH_BM1_3"
		},
		{
			ItemID.ReachBgm073,
			"BGM_G12_REACH_BM1_3"
		},
		{
			ItemID.ReachBgm078,
			"BGM_G01_REACH_TH1_3"
		},
		{
			ItemID.ReachBgm079,
			"BGM_G01_REACH_TH1_3"
		},
		{
			ItemID.ReachBgm080,
			"BGM_G02_REACH_TH1_3"
		},
		{
			ItemID.ReachBgm081,
			"BGM_G02_REACH_TH1_3"
		},
		{
			ItemID.ReachBgm082,
			"BGM_G03_REACH_TH1_3"
		},
		{
			ItemID.ReachBgm083,
			"BGM_G03_REACH_TH1_3"
		},
		{
			ItemID.ReachBgm084,
			"BGM_G04_REACH_TH1_3"
		},
		{
			ItemID.ReachBgm085,
			"BGM_G04_REACH_TH1_3"
		},
		{
			ItemID.ReachBgm086,
			"BGM_G05_REACH_TH1_3"
		},
		{
			ItemID.ReachBgm087,
			"BGM_G05_REACH_TH1_3"
		},
		{
			ItemID.ReachBgm088,
			"BGM_G06_REACH_TH1_3"
		},
		{
			ItemID.ReachBgm089,
			"BGM_G06_REACH_TH1_3"
		},
		{
			ItemID.ReachBgm090,
			"BGM_G07_REACH_TH1_3"
		},
		{
			ItemID.ReachBgm091,
			"BGM_G07_REACH_TH1_3"
		},
		{
			ItemID.ReachBgm092,
			"BGM_G08_REACH_TH1_3"
		},
		{
			ItemID.ReachBgm093,
			"BGM_G08_REACH_TH1_3"
		},
		{
			ItemID.ReachBgm094,
			"BGM_G09_REACH_TH1_3"
		},
		{
			ItemID.ReachBgm095,
			"BGM_G09_REACH_TH1_3"
		},
		{
			ItemID.ReachBgm096,
			"BGM_G10_REACH_TH1_3"
		},
		{
			ItemID.ReachBgm097,
			"BGM_G10_REACH_TH1_3"
		},
		{
			ItemID.ReachBgm098,
			"BGM_G11_REACH_TH1_3"
		},
		{
			ItemID.ReachBgm099,
			"BGM_G11_REACH_TH1_3"
		},
		{
			ItemID.ReachBgm100,
			"BGM_G12_REACH_TH1_3"
		},
		{
			ItemID.ReachBgm101,
			"BGM_G12_REACH_TH1_3"
		},
		{
			ItemID.ReachBgm102,
			"BGM_G13_REACH_TH1_3"
		},
		{
			ItemID.ReachBgm103,
			"BGM_G13_REACH_TH1_3"
		},
		{
			ItemID.ReachBgm116,
			"BGM_G01_REACH_BM2_3"
		},
		{
			ItemID.ReachBgm117,
			"BGM_G01_REACH_BM2_3"
		},
		{
			ItemID.ReachBgm118,
			"BGM_G02_REACH_BM2_3"
		},
		{
			ItemID.ReachBgm119,
			"BGM_G02_REACH_BM2_3"
		},
		{
			ItemID.ReachBgm120,
			"BGM_G03_REACH_BM2_3"
		},
		{
			ItemID.ReachBgm121,
			"BGM_G03_REACH_BM2_3"
		},
		{
			ItemID.ReachBgm122,
			"BGM_G04_REACH_BM2_3"
		},
		{
			ItemID.ReachBgm123,
			"BGM_G04_REACH_BM2_3"
		},
		{
			ItemID.ReachBgm124,
			"BGM_G05_REACH_BM2_3"
		},
		{
			ItemID.ReachBgm125,
			"BGM_G05_REACH_BM2_3"
		},
		{
			ItemID.ReachBgm126,
			"BGM_G06_REACH_BM2_3"
		},
		{
			ItemID.ReachBgm127,
			"BGM_G06_REACH_BM2_3"
		},
		{
			ItemID.ReachBgm128,
			"BGM_G07_REACH_BM2_3"
		},
		{
			ItemID.ReachBgm129,
			"BGM_G07_REACH_BM2_3"
		},
		{
			ItemID.ReachBgm130,
			"BGM_G08_REACH_BM2_3"
		},
		{
			ItemID.ReachBgm131,
			"BGM_G08_REACH_BM2_3"
		},
		{
			ItemID.ReachBgm132,
			"BGM_G09_REACH_BM2_3"
		},
		{
			ItemID.ReachBgm133,
			"BGM_G09_REACH_BM2_3"
		},
		{
			ItemID.ReachBgm134,
			"BGM_G10_REACH_BM2_3"
		},
		{
			ItemID.ReachBgm135,
			"BGM_G10_REACH_BM2_3"
		},
		{
			ItemID.ReachBgm136,
			"BGM_G11_REACH_BM2_3"
		},
		{
			ItemID.ReachBgm137,
			"BGM_G11_REACH_BM2_3"
		},
		{
			ItemID.ReachBgm138,
			"BGM_G12_REACH_BM2_3"
		},
		{
			ItemID.ReachBgm139,
			"BGM_G12_REACH_BM2_3"
		},
		{
			ItemID.ReachBgm140,
			"BGM_G13_REACH_BM2_3"
		},
		{
			ItemID.ReachBgm141,
			"BGM_G13_REACH_BM2_3"
		},
		{
			ItemID.ReachBgm142,
			"BGM_G14_REACH_BM2_3"
		},
		{
			ItemID.ReachBgm143,
			"BGM_G14_REACH_BM2_3"
		},
		{
			ItemID.ReachBgm144,
			"BGM_G15_REACH_BM2_3"
		},
		{
			ItemID.ReachBgm145,
			"BGM_G15_REACH_BM2_3"
		},
		{
			ItemID.ReachBgm146,
			"BGM_G16_REACH_BM2_3"
		},
		{
			ItemID.ReachBgm147,
			"BGM_G16_REACH_BM2_3"
		},
		{
			ItemID.ReachBgm148,
			"BGM_G01_REACH_10"
		},
		{
			ItemID.ReachBgm149,
			"BGM_G01_REACH_10"
		},
		{
			ItemID.ReachBgm150,
			"BGM_G01_REACH_10"
		},
		{
			ItemID.ReachBgm151,
			"BGM_G01_REACH_10"
		},
		{
			ItemID.ReachBgm152,
			"BGM_G02_REACH_10"
		},
		{
			ItemID.ReachBgm153,
			"BGM_G02_REACH_10"
		},
		{
			ItemID.ReachBgm154,
			"BGM_G02_REACH_10"
		},
		{
			ItemID.ReachBgm155,
			"BGM_G02_REACH_10"
		},
		{
			ItemID.ReachBgm160,
			"BGM_G05_REACH_10"
		},
		{
			ItemID.ReachBgm161,
			"BGM_G05_REACH_10"
		},
		{
			ItemID.ReachBgm162,
			"BGM_G05_REACH_10"
		},
		{
			ItemID.ReachBgm163,
			"BGM_G05_REACH_10"
		},
		{
			ItemID.ReachBgm164,
			"BGM_G04_REACH_10"
		},
		{
			ItemID.ReachBgm165,
			"BGM_G04_REACH_10"
		},
		{
			ItemID.ReachBgm166,
			"BGM_G04_REACH_10"
		},
		{
			ItemID.ReachBgm167,
			"BGM_G04_REACH_10"
		},
		{
			ItemID.ReachBgm172,
			"BGM_G08_REACH_10"
		},
		{
			ItemID.ReachBgm173,
			"BGM_G08_REACH_10"
		},
		{
			ItemID.ReachBgm174,
			"BGM_G08_REACH_10"
		},
		{
			ItemID.ReachBgm175,
			"BGM_G08_REACH_10"
		},
		{
			ItemID.ReachBgm180,
			"BGM_G06_REACH_10"
		},
		{
			ItemID.ReachBgm181,
			"BGM_G06_REACH_10"
		},
		{
			ItemID.ReachBgm182,
			"BGM_G06_REACH_10"
		},
		{
			ItemID.ReachBgm183,
			"BGM_G06_REACH_10"
		},
		{
			ItemID.ReachBgm188,
			"BGM_G01_REACH_BM3_3"
		},
		{
			ItemID.ReachBgm189,
			"BGM_G01_REACH_BM3_3"
		},
		{
			ItemID.ReachBgm190,
			"BGM_G02_REACH_BM3_3"
		},
		{
			ItemID.ReachBgm191,
			"BGM_G02_REACH_BM3_3"
		},
		{
			ItemID.ReachBgm192,
			"BGM_G03_REACH_BM3_3"
		},
		{
			ItemID.ReachBgm193,
			"BGM_G03_REACH_BM3_3"
		},
		{
			ItemID.ReachBgm194,
			"BGM_G04_REACH_BM3_3"
		},
		{
			ItemID.ReachBgm195,
			"BGM_G04_REACH_BM3_3"
		},
		{
			ItemID.ReachBgm196,
			"BGM_G05_REACH_BM3_3"
		},
		{
			ItemID.ReachBgm197,
			"BGM_G05_REACH_BM3_3"
		},
		{
			ItemID.ReachBgm198,
			"BGM_G06_REACH_BM3_3"
		},
		{
			ItemID.ReachBgm199,
			"BGM_G06_REACH_BM3_3"
		},
		{
			ItemID.ReachBgm200,
			"BGM_G07_REACH_BM3_3"
		},
		{
			ItemID.ReachBgm201,
			"BGM_G07_REACH_BM3_3"
		},
		{
			ItemID.ReachBgm202,
			"BGM_G08_REACH_BM3_3"
		},
		{
			ItemID.ReachBgm203,
			"BGM_G08_REACH_BM3_3"
		},
		{
			ItemID.ReachBgm204,
			"BGM_G09_REACH_BM3_3"
		},
		{
			ItemID.ReachBgm205,
			"BGM_G09_REACH_BM3_3"
		},
		{
			ItemID.ReachBgm206,
			"BGM_G10_REACH_BM3_3"
		},
		{
			ItemID.ReachBgm207,
			"BGM_G10_REACH_BM3_3"
		},
		{
			ItemID.ReachBgm208,
			"BGM_G11_REACH_BM3_3"
		},
		{
			ItemID.ReachBgm209,
			"BGM_G11_REACH_BM3_3"
		},
		{
			ItemID.ReachBgm210,
			"BGM_G12_REACH_BM3_3"
		},
		{
			ItemID.ReachBgm211,
			"BGM_G12_REACH_BM3_3"
		},
		{
			ItemID.ReachBgm212,
			"BGM_G13_REACH_BM3_3"
		},
		{
			ItemID.ReachBgm213,
			"BGM_G13_REACH_BM3_3"
		},
		{
			ItemID.ReachBgm214,
			"BGM_G14_REACH_BM3_3"
		},
		{
			ItemID.ReachBgm215,
			"BGM_G14_REACH_BM3_3"
		},
		{
			ItemID.ReachBgm216,
			"BGM_G15_REACH_BM3_3"
		},
		{
			ItemID.ReachBgm217,
			"BGM_G15_REACH_BM3_3"
		},
		{
			ItemID.ReachBgm218,
			"BGM_G16_REACH_BM3_3"
		},
		{
			ItemID.ReachBgm219,
			"BGM_G16_REACH_BM3_3"
		},
		{
			ItemID.ReachBgm220,
			"BGM_G17_REACH_BM3_3"
		},
		{
			ItemID.ReachBgm221,
			"BGM_G17_REACH_BM3_3"
		},
		{
			ItemID.ReachBgm222,
			"BGM_G18_REACH_BM3_3"
		},
		{
			ItemID.ReachBgm223,
			"BGM_G18_REACH_BM3_3"
		},
		{
			ItemID.ReachBgm224,
			"BGM_G19_REACH_BM3_3"
		},
		{
			ItemID.ReachBgm225,
			"BGM_G19_REACH_BM3_3"
		},
		{
			ItemID.ReachBgm226,
			"BGM_G20_REACH_BM3_3"
		},
		{
			ItemID.ReachBgm227,
			"BGM_G20_REACH_BM3_3"
		},
		{
			ItemID.ReachBgm228,
			"BGM_G07_REACH_10"
		},
		{
			ItemID.ReachBgm229,
			"BGM_G07_REACH_10"
		},
		{
			ItemID.ReachBgm230,
			"BGM_G07_REACH_10"
		},
		{
			ItemID.ReachBgm231,
			"BGM_G07_REACH_10"
		}
	};

	public static Dictionary<(int, CharaType), ItemID> s_reachBgmFavoIdx = new Dictionary<(int, CharaType), ItemID>
	{
		{
			(0, CharaType.Chara01),
			ItemID.ReachBgm002
		},
		{
			(1, CharaType.Chara01),
			ItemID.ReachBgm003
		},
		{
			(2, CharaType.Chara01),
			ItemID.ReachBgm004
		},
		{
			(3, CharaType.Chara01),
			ItemID.ReachBgm005
		},
		{
			(0, CharaType.Chara02),
			ItemID.ReachBgm006
		},
		{
			(1, CharaType.Chara02),
			ItemID.ReachBgm007
		},
		{
			(2, CharaType.Chara02),
			ItemID.ReachBgm008
		},
		{
			(3, CharaType.Chara02),
			ItemID.ReachBgm009
		},
		{
			(0, CharaType.Chara03),
			ItemID.ReachBgm010
		},
		{
			(1, CharaType.Chara03),
			ItemID.ReachBgm011
		},
		{
			(2, CharaType.Chara03),
			ItemID.ReachBgm012
		},
		{
			(3, CharaType.Chara03),
			ItemID.ReachBgm013
		},
		{
			(0, CharaType.Chara04),
			ItemID.ReachBgm014
		},
		{
			(1, CharaType.Chara04),
			ItemID.ReachBgm015
		},
		{
			(2, CharaType.Chara04),
			ItemID.ReachBgm016
		},
		{
			(3, CharaType.Chara04),
			ItemID.ReachBgm017
		},
		{
			(0, CharaType.Chara05),
			ItemID.ReachBgm018
		},
		{
			(1, CharaType.Chara05),
			ItemID.ReachBgm019
		},
		{
			(2, CharaType.Chara05),
			ItemID.ReachBgm020
		},
		{
			(3, CharaType.Chara05),
			ItemID.ReachBgm021
		},
		{
			(0, CharaType.Chara06),
			ItemID.ReachBgm022
		},
		{
			(1, CharaType.Chara06),
			ItemID.ReachBgm023
		},
		{
			(2, CharaType.Chara06),
			ItemID.ReachBgm024
		},
		{
			(3, CharaType.Chara06),
			ItemID.ReachBgm025
		},
		{
			(0, CharaType.Chara07),
			ItemID.ReachBgm026
		},
		{
			(1, CharaType.Chara07),
			ItemID.ReachBgm027
		},
		{
			(2, CharaType.Chara07),
			ItemID.ReachBgm028
		},
		{
			(3, CharaType.Chara07),
			ItemID.ReachBgm029
		},
		{
			(0, CharaType.Chara08),
			ItemID.ReachBgm030
		},
		{
			(1, CharaType.Chara08),
			ItemID.ReachBgm031
		},
		{
			(2, CharaType.Chara08),
			ItemID.ReachBgm032
		},
		{
			(3, CharaType.Chara08),
			ItemID.ReachBgm033
		},
		{
			(0, CharaType.Chara09),
			ItemID.ReachBgm034
		},
		{
			(1, CharaType.Chara09),
			ItemID.ReachBgm035
		},
		{
			(2, CharaType.Chara09),
			ItemID.ReachBgm036
		},
		{
			(3, CharaType.Chara09),
			ItemID.ReachBgm037
		},
		{
			(0, CharaType.Chara10),
			ItemID.ReachBgm038
		},
		{
			(1, CharaType.Chara10),
			ItemID.ReachBgm039
		},
		{
			(2, CharaType.Chara10),
			ItemID.ReachBgm040
		},
		{
			(3, CharaType.Chara10),
			ItemID.ReachBgm041
		},
		{
			(0, CharaType.Chara11),
			ItemID.ReachBgm042
		},
		{
			(1, CharaType.Chara11),
			ItemID.ReachBgm043
		},
		{
			(2, CharaType.Chara11),
			ItemID.ReachBgm044
		},
		{
			(3, CharaType.Chara11),
			ItemID.ReachBgm045
		},
		{
			(0, CharaType.Chara12),
			ItemID.ReachBgm046
		},
		{
			(1, CharaType.Chara12),
			ItemID.ReachBgm047
		},
		{
			(2, CharaType.Chara12),
			ItemID.ReachBgm048
		},
		{
			(3, CharaType.Chara12),
			ItemID.ReachBgm049
		},
		{
			(0, CharaType.Chara13),
			ItemID.ReachBgm074
		},
		{
			(1, CharaType.Chara13),
			ItemID.ReachBgm075
		},
		{
			(2, CharaType.Chara13),
			ItemID.ReachBgm076
		},
		{
			(3, CharaType.Chara13),
			ItemID.ReachBgm077
		},
		{
			(0, CharaType.Chara14),
			ItemID.ReachBgm104
		},
		{
			(1, CharaType.Chara14),
			ItemID.ReachBgm105
		},
		{
			(2, CharaType.Chara14),
			ItemID.ReachBgm106
		},
		{
			(3, CharaType.Chara14),
			ItemID.ReachBgm107
		},
		{
			(0, CharaType.Chara15),
			ItemID.ReachBgm108
		},
		{
			(1, CharaType.Chara15),
			ItemID.ReachBgm109
		},
		{
			(2, CharaType.Chara15),
			ItemID.ReachBgm110
		},
		{
			(3, CharaType.Chara15),
			ItemID.ReachBgm111
		},
		{
			(0, CharaType.Chara16),
			ItemID.ReachBgm112
		},
		{
			(1, CharaType.Chara16),
			ItemID.ReachBgm113
		},
		{
			(2, CharaType.Chara16),
			ItemID.ReachBgm114
		},
		{
			(3, CharaType.Chara16),
			ItemID.ReachBgm115
		},
		{
			(0, CharaType.Chara17),
			ItemID.ReachBgm156
		},
		{
			(1, CharaType.Chara17),
			ItemID.ReachBgm157
		},
		{
			(2, CharaType.Chara17),
			ItemID.ReachBgm158
		},
		{
			(3, CharaType.Chara17),
			ItemID.ReachBgm159
		},
		{
			(0, CharaType.Chara18),
			ItemID.ReachBgm168
		},
		{
			(1, CharaType.Chara18),
			ItemID.ReachBgm169
		},
		{
			(2, CharaType.Chara18),
			ItemID.ReachBgm170
		},
		{
			(3, CharaType.Chara18),
			ItemID.ReachBgm171
		},
		{
			(0, CharaType.Chara19),
			ItemID.ReachBgm176
		},
		{
			(1, CharaType.Chara19),
			ItemID.ReachBgm177
		},
		{
			(2, CharaType.Chara19),
			ItemID.ReachBgm178
		},
		{
			(3, CharaType.Chara19),
			ItemID.ReachBgm179
		},
		{
			(0, CharaType.Chara20),
			ItemID.ReachBgm184
		},
		{
			(1, CharaType.Chara20),
			ItemID.ReachBgm185
		},
		{
			(2, CharaType.Chara20),
			ItemID.ReachBgm186
		},
		{
			(3, CharaType.Chara20),
			ItemID.ReachBgm187
		},
		{
			(0, CharaType.Chara21),
			ItemID.ReachBgm232
		},
		{
			(1, CharaType.Chara21),
			ItemID.ReachBgm233
		},
		{
			(2, CharaType.Chara21),
			ItemID.ReachBgm234
		},
		{
			(3, CharaType.Chara21),
			ItemID.ReachBgm235
		}
	};

	private static Dictionary<ItemID, string> s_myRoomBgmLabelDic = new Dictionary<ItemID, string>
	{
		{
			ItemID.MyRoomBgm001,
			"BGM_HOME"
		},
		{
			ItemID.MyRoomBgm002,
			"BGM_STORY1"
		},
		{
			ItemID.MyRoomBgm003,
			"BGM_STORY2"
		},
		{
			ItemID.MyRoomBgm004,
			"BGM_HOME_SONG1"
		},
		{
			ItemID.MyRoomBgm005,
			"BGM_HOME_SONG2"
		},
		{
			ItemID.MyRoomBgm006,
			"BGM_HOME_KAC1"
		},
		{
			ItemID.MyRoomBgm007,
			"BGM_HOME_XMAS1"
		},
		{
			ItemID.MyRoomBgm008,
			"BGM_HOME_NEWYEAR1"
		},
		{
			ItemID.MyRoomBgm009,
			"BGM_STORY_G01"
		},
		{
			ItemID.MyRoomBgm010,
			"BGM_STORY_G02"
		},
		{
			ItemID.MyRoomBgm011,
			"BGM_STORY_G03"
		},
		{
			ItemID.MyRoomBgm012,
			"BGM_STORY_G04"
		},
		{
			ItemID.MyRoomBgm013,
			"BGM_STORY_G05"
		},
		{
			ItemID.MyRoomBgm014,
			"BGM_STORY_G06"
		},
		{
			ItemID.MyRoomBgm015,
			"BGM_STORY_G07"
		},
		{
			ItemID.MyRoomBgm016,
			"BGM_STORY_G08"
		},
		{
			ItemID.MyRoomBgm017,
			"BGM_STORY_G09"
		},
		{
			ItemID.MyRoomBgm018,
			"BGM_STORY_G10_3"
		},
		{
			ItemID.MyRoomBgm019,
			"BGM_STORY_G11"
		},
		{
			ItemID.MyRoomBgm020,
			"BGM_STORY_G12"
		},
		{
			ItemID.MyRoomBgm021,
			"BGM_HOME_BM1_G01"
		},
		{
			ItemID.MyRoomBgm022,
			"BGM_HOME_BM1_G02"
		},
		{
			ItemID.MyRoomBgm023,
			"BGM_HOME_BM1_G03"
		},
		{
			ItemID.MyRoomBgm024,
			"BGM_HOME_BM1_G04"
		},
		{
			ItemID.MyRoomBgm025,
			"BGM_HOME_BM1_G05"
		},
		{
			ItemID.MyRoomBgm026,
			"BGM_HOME_BM1_G06"
		},
		{
			ItemID.MyRoomBgm027,
			"BGM_HOME_BM1_G07"
		},
		{
			ItemID.MyRoomBgm028,
			"BGM_HOME_BM1_G08"
		},
		{
			ItemID.MyRoomBgm029,
			"BGM_HOME_BM1_G09"
		},
		{
			ItemID.MyRoomBgm030,
			"BGM_HOME_BM1_G10"
		},
		{
			ItemID.MyRoomBgm031,
			"BGM_HOME_BM1_G11"
		},
		{
			ItemID.MyRoomBgm032,
			"BGM_HOME_BM1_G12"
		},
		{
			ItemID.MyRoomBgm033,
			"BGM_HOME_TOHO2"
		},
		{
			ItemID.MyRoomBgm034,
			"BGM_HOME_TOHO1"
		},
		{
			ItemID.MyRoomBgm035,
			"BGM_HOME_TH1_G01"
		},
		{
			ItemID.MyRoomBgm036,
			"BGM_HOME_TH1_G02"
		},
		{
			ItemID.MyRoomBgm037,
			"BGM_HOME_TH1_G03"
		},
		{
			ItemID.MyRoomBgm038,
			"BGM_HOME_TH1_G04"
		},
		{
			ItemID.MyRoomBgm039,
			"BGM_HOME_TH1_G05"
		},
		{
			ItemID.MyRoomBgm040,
			"BGM_HOME_TH1_G06"
		},
		{
			ItemID.MyRoomBgm041,
			"BGM_HOME_TH1_G07"
		},
		{
			ItemID.MyRoomBgm042,
			"BGM_HOME_TH1_G08"
		},
		{
			ItemID.MyRoomBgm043,
			"BGM_HOME_TH1_G09"
		},
		{
			ItemID.MyRoomBgm044,
			"BGM_HOME_TH1_G10"
		},
		{
			ItemID.MyRoomBgm045,
			"BGM_HOME_TH1_G11"
		},
		{
			ItemID.MyRoomBgm046,
			"BGM_HOME_TH1_G12"
		},
		{
			ItemID.MyRoomBgm047,
			"BGM_HOME_TH1_G13"
		},
		{
			ItemID.MyRoomBgm048,
			"BGM_HOME_BM2_G01"
		},
		{
			ItemID.MyRoomBgm049,
			"BGM_HOME_BM2_G02"
		},
		{
			ItemID.MyRoomBgm050,
			"BGM_HOME_BM2_G03"
		},
		{
			ItemID.MyRoomBgm051,
			"BGM_HOME_BM2_G04"
		},
		{
			ItemID.MyRoomBgm052,
			"BGM_HOME_BM2_G05"
		},
		{
			ItemID.MyRoomBgm053,
			"BGM_HOME_BM2_G06"
		},
		{
			ItemID.MyRoomBgm054,
			"BGM_HOME_BM2_G07"
		},
		{
			ItemID.MyRoomBgm055,
			"BGM_HOME_BM2_G08"
		},
		{
			ItemID.MyRoomBgm056,
			"BGM_HOME_BM2_G09"
		},
		{
			ItemID.MyRoomBgm057,
			"BGM_HOME_BM2_G10"
		},
		{
			ItemID.MyRoomBgm058,
			"BGM_HOME_BM2_G11"
		},
		{
			ItemID.MyRoomBgm059,
			"BGM_HOME_BM2_G12"
		},
		{
			ItemID.MyRoomBgm060,
			"BGM_HOME_BM2_G13"
		},
		{
			ItemID.MyRoomBgm061,
			"BGM_HOME_BM2_G14"
		},
		{
			ItemID.MyRoomBgm062,
			"BGM_HOME_BM2_G15"
		},
		{
			ItemID.MyRoomBgm063,
			"BGM_HOME_BM2_G16"
		},
		{
			ItemID.MyRoomBgm064,
			"BGM_HOME_BGR1"
		},
		{
			ItemID.MyRoomBgm065,
			"BGM_HOME_QMA1"
		},
		{
			ItemID.MyRoomBgm066,
			"BGM_HOME_BM3_G01"
		},
		{
			ItemID.MyRoomBgm067,
			"BGM_HOME_BM3_G02"
		},
		{
			ItemID.MyRoomBgm068,
			"BGM_HOME_BM3_G03"
		},
		{
			ItemID.MyRoomBgm069,
			"BGM_HOME_BM3_G04"
		},
		{
			ItemID.MyRoomBgm070,
			"BGM_HOME_BM3_G05"
		},
		{
			ItemID.MyRoomBgm071,
			"BGM_HOME_BM3_G06"
		},
		{
			ItemID.MyRoomBgm072,
			"BGM_HOME_BM3_G07"
		},
		{
			ItemID.MyRoomBgm073,
			"BGM_HOME_BM3_G08"
		},
		{
			ItemID.MyRoomBgm074,
			"BGM_HOME_BM3_G09"
		},
		{
			ItemID.MyRoomBgm075,
			"BGM_HOME_BM3_G10"
		},
		{
			ItemID.MyRoomBgm076,
			"BGM_HOME_BM3_G11"
		},
		{
			ItemID.MyRoomBgm077,
			"BGM_HOME_BM3_G12"
		},
		{
			ItemID.MyRoomBgm078,
			"BGM_HOME_BM3_G13"
		},
		{
			ItemID.MyRoomBgm079,
			"BGM_HOME_BM3_G14"
		},
		{
			ItemID.MyRoomBgm080,
			"BGM_HOME_BM3_G15"
		},
		{
			ItemID.MyRoomBgm081,
			"BGM_HOME_BM3_G16"
		},
		{
			ItemID.MyRoomBgm082,
			"BGM_HOME_BM3_G17"
		},
		{
			ItemID.MyRoomBgm083,
			"BGM_HOME_BM3_G18"
		},
		{
			ItemID.MyRoomBgm084,
			"BGM_HOME_BM3_G19"
		},
		{
			ItemID.MyRoomBgm085,
			"BGM_HOME_BM3_G20"
		}
	};

	private static Dictionary<ItemID, (CharaType, int)> s_gachaNoDic = new Dictionary<ItemID, (CharaType, int)>
	{
		{
			ItemID.ReachBgm148,
			(CharaType.Chara01, 4)
		},
		{
			ItemID.ReachBgm149,
			(CharaType.Chara01, 3)
		},
		{
			ItemID.ReachBgm150,
			(CharaType.Chara01, 2)
		},
		{
			ItemID.ReachBgm151,
			(CharaType.Chara01, 1)
		},
		{
			ItemID.ReachBgm152,
			(CharaType.Chara02, 4)
		},
		{
			ItemID.ReachBgm153,
			(CharaType.Chara02, 3)
		},
		{
			ItemID.ReachBgm154,
			(CharaType.Chara02, 2)
		},
		{
			ItemID.ReachBgm155,
			(CharaType.Chara02, 1)
		},
		{
			ItemID.ReachBgm160,
			(CharaType.Chara05, 4)
		},
		{
			ItemID.ReachBgm161,
			(CharaType.Chara05, 3)
		},
		{
			ItemID.ReachBgm162,
			(CharaType.Chara05, 2)
		},
		{
			ItemID.ReachBgm163,
			(CharaType.Chara05, 1)
		},
		{
			ItemID.ReachBgm164,
			(CharaType.Chara04, 4)
		},
		{
			ItemID.ReachBgm165,
			(CharaType.Chara04, 3)
		},
		{
			ItemID.ReachBgm166,
			(CharaType.Chara04, 2)
		},
		{
			ItemID.ReachBgm167,
			(CharaType.Chara04, 1)
		},
		{
			ItemID.ReachBgm172,
			(CharaType.Chara08, 4)
		},
		{
			ItemID.ReachBgm173,
			(CharaType.Chara08, 3)
		},
		{
			ItemID.ReachBgm174,
			(CharaType.Chara08, 2)
		},
		{
			ItemID.ReachBgm175,
			(CharaType.Chara08, 1)
		},
		{
			ItemID.ReachBgm180,
			(CharaType.Chara06, 4)
		},
		{
			ItemID.ReachBgm181,
			(CharaType.Chara06, 3)
		},
		{
			ItemID.ReachBgm182,
			(CharaType.Chara06, 2)
		},
		{
			ItemID.ReachBgm183,
			(CharaType.Chara06, 1)
		},
		{
			ItemID.ReachBgm228,
			(CharaType.Chara07, 4)
		},
		{
			ItemID.ReachBgm229,
			(CharaType.Chara07, 3)
		},
		{
			ItemID.ReachBgm230,
			(CharaType.Chara07, 2)
		},
		{
			ItemID.ReachBgm231,
			(CharaType.Chara07, 1)
		}
	};

	public static string GetTileSeMaterialPath(ItemID id)
	{
		if (!s_tileSeMaterialPath.Contains(id))
		{
			return "";
		}
		return $"CustomizeItem/{id}/Se.mat";
	}

	public static string GetTileCubeMaterialPath(ItemID id)
	{
		if (!s_tileCubeMaterialPath.Contains(id))
		{
			return "";
		}
		return $"CustomizeItem/{id}/Cube.mat";
	}

	public static string GetTileKiraMaterialPath(ItemID id)
	{
		if (!s_tileKiraMaterialPath.Contains(id))
		{
			return "";
		}
		return $"CustomizeItem/{id}/Kira.mat";
	}

	public static string ReachBgmCustomJingle(ItemID id)
	{
		if (s_jingleDic.ContainsKey(id))
		{
			return s_jingleDic[id];
		}
		return "";
	}

	public static ItemID ReachBgmFromFavoOID(string oid, CharaType charaType)
	{
		return oid switch
		{
			"OID_Favo01_020" => s_reachBgmFavoIdx[(0, charaType)], 
			"OID_Favo01_060" => s_reachBgmFavoIdx[(1, charaType)], 
			"OID_Favo01_100" => s_reachBgmFavoIdx[(2, charaType)], 
			"OID_Favo01_142" => s_reachBgmFavoIdx[(3, charaType)], 
			_ => ItemID.ReachBgm001, 
		};
	}

	public static string ReachBgmToThumbBgmLabel(this ItemID itemID)
	{
		return s_labelDic[itemID].bgmLabel.Insert(3, "_THUMB");
	}

	public static string ReachBgmToBgmLabel(this ItemID itemID)
	{
		return s_labelDic[itemID].bgmLabel;
	}

	public static CharaType RreachBgmToCharaType(this ItemID itemID)
	{
		return s_labelDic[itemID].charaType;
	}

	public static string MyRoomToMyRoomBgmLabel(this ItemID itemID, bool isPreview = false)
	{
		if (isPreview)
		{
			switch (itemID)
			{
			case ItemID.MyRoomBgm002:
			case ItemID.MyRoomBgm003:
				return s_myRoomBgmLabelDic[itemID].Replace("BGM_", "BGM_THUMB_HOME_");
			case ItemID.MyRoomBgm018:
			{
				string text5 = s_myRoomBgmLabelDic[itemID];
				return text5.Remove(text5.Length - 2).Replace("BGM_", "BGM_THUMB_");
			}
			case ItemID.MyRoomBgm021:
			case ItemID.MyRoomBgm022:
			case ItemID.MyRoomBgm023:
			case ItemID.MyRoomBgm024:
			case ItemID.MyRoomBgm025:
			case ItemID.MyRoomBgm026:
			case ItemID.MyRoomBgm027:
			case ItemID.MyRoomBgm028:
			case ItemID.MyRoomBgm029:
			case ItemID.MyRoomBgm030:
			case ItemID.MyRoomBgm031:
			case ItemID.MyRoomBgm032:
			{
				string text4 = s_myRoomBgmLabelDic[itemID];
				int num = text4.Length;
				int length = num - 3;
				return "BGM_THUMB_" + text4.Substring(length, num - length) + "_REACH_BM1_2";
			}
			default:
				if (itemID >= ItemID.MyRoomBgm035 && itemID <= ItemID.MyRoomBgm047)
				{
					string text = s_myRoomBgmLabelDic[itemID];
					int length = text.Length;
					int num = length - 3;
					return "BGM_THUMB_" + text.Substring(num, length - num) + "_REACH_TH1_2";
				}
				if (itemID >= ItemID.MyRoomBgm048 && itemID <= ItemID.MyRoomBgm063)
				{
					string text2 = s_myRoomBgmLabelDic[itemID];
					int num = text2.Length;
					int length = num - 3;
					return "BGM_THUMB_" + text2.Substring(length, num - length) + "_REACH_BM2_2";
				}
				if (itemID >= ItemID.MyRoomBgm066 && itemID <= ItemID.MyRoomBgm085)
				{
					string text3 = s_myRoomBgmLabelDic[itemID];
					int length = text3.Length;
					int num = length - 3;
					return "BGM_THUMB_" + text3.Substring(num, length - num) + "_REACH_BM3_2";
				}
				return s_myRoomBgmLabelDic[itemID].Replace("BGM_", "BGM_THUMB_");
			}
		}
		return s_myRoomBgmLabelDic[itemID];
	}

	public static int MusicGachaItemNo(string oid)
	{
		ItemID key = CustomizeItemMaster.GetItem(oid)?.ItemID ?? ItemID.None;
		if (s_gachaNoDic.ContainsKey(key))
		{
			return s_gachaNoDic[key].Item2;
		}
		return 0;
	}

	public static List<ItemID> GetGachaReachSongs(CharaType chara)
	{
		return (from x in s_gachaNoDic
			where x.Value.Item1 == chara
			select x.Key).ToList();
	}
}
