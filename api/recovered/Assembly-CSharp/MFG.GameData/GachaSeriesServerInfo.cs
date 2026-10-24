using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MFG.Assets;
using MFG.MasterGameData;
using MFG.Scenes.CommonSystem;
using MFG.Types;
using MFG.Types.AD;
using MFG.Types.Customize.CustomizeItem;
using MFG.Types.Customize.CutinItem;
using MFG.Types.PlayerPaidItem;

namespace MFG.GameData;

public class GachaSeriesServerInfo : IAssetPath
{
	public int SeriesID;

	public GachaSeriesType SeriesType;

	public string Label = "";

	public int TicketNum;

	public bool IsActive = true;

	public List<MFG.Types.Customize.CutinItem.ItemID> Items = new List<MFG.Types.Customize.CutinItem.ItemID>();

	public List<MFG.Types.Customize.CutinItem.ItemID> PickupItems = new List<MFG.Types.Customize.CutinItem.ItemID>();

	public List<MFG.Types.Customize.CutinItem.ItemID> LimitedItems = new List<MFG.Types.Customize.CutinItem.ItemID>();

	public List<string> MusicItems = new List<string>();

	public List<CharaType> PickUpCharas = new List<CharaType>();

	public List<MFG.Types.Customize.CutinItem.ItemID> ExchangeItems = new List<MFG.Types.Customize.CutinItem.ItemID>();

	public string StartDate;

	public string EndDate;

	public bool IsForceActive;

	public string ForceEndDate;

	public List<Dictionary<Rarity, float>> ProbabilityTable;

	[NonSerialized]
	public static readonly List<GachaExtraMasterData> GachaExtraMasters = new List<GachaExtraMasterData>
	{
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.Normal,
			PickupKind = PickupKind.None
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.NormalTicket,
			PickupKind = PickupKind.None,
			EnableTickets = new List<MFG.Types.PlayerPaidItem.ItemID>()
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupPine,
			PickupKind = PickupKind.NewGirl,
			IsHomeAppear = true
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupMizugiSen,
			PickupKind = PickupKind.Mizugi,
			IsHomeAppear = false
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupMizugiMitsuba,
			PickupKind = PickupKind.Mizugi,
			IsHomeAppear = true
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupMizugiMusashi,
			PickupKind = PickupKind.Mizugi,
			IsHomeAppear = true,
			AdTipsIDs = new List<AdTipsID> { AdTipsID.Ad1 }
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupMizugiHiyori,
			PickupKind = PickupKind.Mizugi,
			IsHomeAppear = true,
			AdTipsIDs = new List<AdTipsID> { AdTipsID.Ad2 }
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupMizugiYao,
			PickupKind = PickupKind.Mizugi,
			IsHomeAppear = true,
			AdTipsIDs = new List<AdTipsID> { AdTipsID.Ad3 }
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupMizugiPain,
			PickupKind = PickupKind.Mizugi,
			IsHomeAppear = true,
			AdTipsIDs = new List<AdTipsID> { AdTipsID.Ad5 }
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupMizugiTenshi,
			PickupKind = PickupKind.Mizugi,
			IsHomeAppear = true,
			AdTipsIDs = new List<AdTipsID> { AdTipsID.Ad4 }
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupShiori,
			PickupKind = PickupKind.NewGirl,
			IsHomeAppear = true,
			AdTipsIDs = new List<AdTipsID> { AdTipsID.Ad6 }
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupMizugiTumire,
			PickupKind = PickupKind.Mizugi,
			IsHomeAppear = true,
			AdTipsIDs = new List<AdTipsID> { AdTipsID.Ad7 }
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupMizugiToitoi,
			PickupKind = PickupKind.Mizugi,
			IsHomeAppear = true,
			AdTipsIDs = new List<AdTipsID> { AdTipsID.Ad8 }
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupHalfAnniversary,
			PickupKind = PickupKind.HalfAnniversary,
			IsHomeAppear = true,
			AdTipsIDs = new List<AdTipsID> { AdTipsID.Ad9 }
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupMizugiIchiko,
			PickupKind = PickupKind.Mizugi,
			IsReturens = true,
			AdTipsIDs = new List<AdTipsID>
			{
				AdTipsID.Ad10,
				AdTipsID.Ad11,
				AdTipsID.Ad12
			}
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupMizugiTeiyaku,
			PickupKind = PickupKind.Mizugi,
			IsReturens = true,
			AdTipsIDs = new List<AdTipsID>
			{
				AdTipsID.Ad13,
				AdTipsID.Ad14,
				AdTipsID.Ad15
			}
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupHalloweenMusashi,
			PickupKind = PickupKind.Halloween,
			IsHomeAppear = true,
			AdTipsIDs = new List<AdTipsID> { AdTipsID.Ad16 }
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupMizugiNijo,
			PickupKind = PickupKind.Mizugi,
			IsReturens = true,
			AdTipsIDs = new List<AdTipsID>
			{
				AdTipsID.Ad17,
				AdTipsID.Ad18,
				AdTipsID.Ad19
			}
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupChaos,
			PickupKind = PickupKind.NewGirl,
			IsHomeAppear = true,
			AdTipsIDs = new List<AdTipsID> { AdTipsID.Ad20 }
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupCinderellaMitsuba,
			PickupKind = PickupKind.Cinderella,
			IsHomeAppear = true,
			AdTipsIDs = new List<AdTipsID> { AdTipsID.Ad21 }
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupXmasTenshi,
			PickupKind = PickupKind.Xmas,
			IsHomeAppear = true,
			AdTipsIDs = new List<AdTipsID> { AdTipsID.Ad22 }
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupNewYearTsumire,
			PickupKind = PickupKind.NewYear,
			IsHomeAppear = true,
			AdTipsIDs = new List<AdTipsID> { AdTipsID.Ad23 }
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupNewYearYao,
			PickupKind = PickupKind.NewYear,
			IsHomeAppear = true,
			AdTipsIDs = new List<AdTipsID> { AdTipsID.Ad24 }
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupValentineToytoy,
			PickupKind = PickupKind.Valentine,
			IsHomeAppear = true,
			AdTipsIDs = new List<AdTipsID> { AdTipsID.Ad25 }
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupClear,
			PickupKind = PickupKind.NewGirl,
			IsHomeAppear = true,
			AdTipsIDs = new List<AdTipsID>
			{
				AdTipsID.Ad26,
				AdTipsID.Ad27
			}
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.UnlockClear,
			PickupKind = PickupKind.Unlock,
			IsHomeAppear = false
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupSuccubusTenshi,
			PickupKind = PickupKind.Succubus,
			IsHomeAppear = true,
			AdTipsIDs = new List<AdTipsID>
			{
				AdTipsID.Ad28,
				AdTipsID.Ad29
			}
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupParodiusTsumire,
			PickupKind = PickupKind.Parodius,
			IsHomeAppear = true,
			AdTipsIDs = new List<AdTipsID>
			{
				AdTipsID.Ad30,
				AdTipsID.Ad31
			}
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupRaceQueenHiyori,
			PickupKind = PickupKind.RaceQueen,
			IsHomeAppear = true,
			AdTipsIDs = new List<AdTipsID> { AdTipsID.Ad32 }
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupShiori2,
			PickupKind = PickupKind.NewGirl,
			IsHomeAppear = false,
			IsReturens = true,
			AdTipsIDs = new List<AdTipsID> { AdTipsID.Ad6 }
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupMizugiChaos,
			PickupKind = PickupKind.Mizugi,
			IsHomeAppear = true,
			AdTipsIDs = new List<AdTipsID> { AdTipsID.Ad33 }
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupHalfAnniversary2,
			PickupKind = PickupKind.HalfAnniversary,
			IsHomeAppear = false,
			IsReturens = true,
			AdTipsIDs = new List<AdTipsID> { AdTipsID.Ad9 }
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupMizugiClear,
			PickupKind = PickupKind.Mizugi,
			IsHomeAppear = true,
			AdTipsIDs = new List<AdTipsID> { AdTipsID.Ad34 }
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupAliceMaidPine,
			PickupKind = PickupKind.AliceMaid,
			IsHomeAppear = true,
			AdTipsIDs = new List<AdTipsID>
			{
				AdTipsID.Ad35,
				AdTipsID.Ad36
			}
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupMizugiReturns1,
			PickupKind = PickupKind.Mizugi,
			IsHomeAppear = false,
			IsReturens = true,
			AdTipsIDs = new List<AdTipsID> { AdTipsID.Ad37 }
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupJerseyMitsuba,
			PickupKind = PickupKind.Jersey,
			IsHomeAppear = true,
			AdTipsIDs = new List<AdTipsID>
			{
				AdTipsID.Ad38,
				AdTipsID.Ad39
			}
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupUniformMusashi,
			PickupKind = PickupKind.Uniform,
			IsHomeAppear = true,
			AdTipsIDs = new List<AdTipsID> { AdTipsID.Ad40 }
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupUniformSen,
			PickupKind = PickupKind.Uniform,
			IsHomeAppear = true,
			AdTipsIDs = new List<AdTipsID> { AdTipsID.Ad41 }
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupChaos2,
			PickupKind = PickupKind.NewGirl,
			IsHomeAppear = false,
			IsReturens = true
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupUniformYao,
			PickupKind = PickupKind.Uniform,
			IsHomeAppear = true,
			AdTipsIDs = new List<AdTipsID>
			{
				AdTipsID.Ad43,
				AdTipsID.Ad44
			}
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupYukataToytoy,
			PickupKind = PickupKind.Yukata,
			IsHomeAppear = true,
			AdTipsIDs = new List<AdTipsID> { AdTipsID.Ad45 }
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupClear2,
			PickupKind = PickupKind.NewGirl,
			IsHomeAppear = false,
			IsReturens = true,
			AdTipsIDs = new List<AdTipsID> { AdTipsID.Ad46 }
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupShiori3,
			PickupKind = PickupKind.NewGirl,
			IsHomeAppear = false,
			IsReturens = true
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupIyo,
			PickupKind = PickupKind.NewGirl,
			IsHomeAppear = true,
			AdTipsIDs = new List<AdTipsID>
			{
				AdTipsID.Ad47,
				AdTipsID.Ad48
			}
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.UnlockIyo,
			PickupKind = PickupKind.Unlock,
			IsHomeAppear = false
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupTohoHiyori,
			PickupKind = PickupKind.Toho,
			IsHomeAppear = true,
			AdTipsIDs = new List<AdTipsID> { AdTipsID.Ad49 }
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupTohoTenshi,
			PickupKind = PickupKind.Toho,
			IsHomeAppear = true,
			AdTipsIDs = new List<AdTipsID> { AdTipsID.Ad50 }
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupTohoClear,
			PickupKind = PickupKind.Toho,
			IsHomeAppear = true,
			AdTipsIDs = new List<AdTipsID> { AdTipsID.Ad51 }
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupMizugiKomugi,
			PickupKind = PickupKind.Mizugi,
			IsHomeAppear = true,
			IsReturens = true,
			AdTipsIDs = new List<AdTipsID> { AdTipsID.Ad52 }
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupMizugiReturns2,
			PickupKind = PickupKind.Mizugi,
			IsHomeAppear = false,
			IsReturens = true,
			AdTipsIDs = new List<AdTipsID> { AdTipsID.Ad53 }
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupDateMizugiSen,
			PickupKind = PickupKind.DateMizugi,
			IsHomeAppear = true,
			AdTipsIDs = new List<AdTipsID> { AdTipsID.Ad54 }
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupSisterTsumire,
			PickupKind = PickupKind.Sister,
			IsHomeAppear = true,
			AdTipsIDs = new List<AdTipsID> { AdTipsID.Ad55 }
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupUniformSen2,
			PickupKind = PickupKind.Uniform2,
			IsHomeAppear = true,
			AdTipsIDs = new List<AdTipsID> { AdTipsID.Ad56 }
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupHalloweenMusashi2,
			PickupKind = PickupKind.Halloween2,
			IsHomeAppear = true,
			IsReturens = true,
			AdTipsIDs = new List<AdTipsID> { AdTipsID.Ad16 }
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupBomberPine,
			PickupKind = PickupKind.Bomber,
			IsHomeAppear = true,
			AdTipsIDs = new List<AdTipsID> { AdTipsID.Ad57 }
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupGrimAroe,
			PickupKind = PickupKind.NewGirl,
			IsHomeAppear = true,
			AdTipsIDs = new List<AdTipsID>
			{
				AdTipsID.Ad58,
				AdTipsID.Ad59
			}
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.UnlockGrimAroe,
			PickupKind = PickupKind.Unlock,
			IsHomeAppear = false
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupChaos3,
			PickupKind = PickupKind.NewGirl,
			RelationGachaSeriesName = GachaSeriesName.PickupMizugiChaos,
			IsHomeAppear = false,
			IsReturens = true
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupClear3,
			PickupKind = PickupKind.NewGirl,
			RelationGachaSeriesName = GachaSeriesName.PickupMizugiClear,
			IsHomeAppear = false,
			IsReturens = true
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupIyo2,
			PickupKind = PickupKind.NewGirl,
			RelationGachaSeriesName = GachaSeriesName.PickupIyo,
			IsHomeAppear = true,
			IsReturens = true,
			AdTipsIDs = new List<AdTipsID> { AdTipsID.Ad60 }
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupDarknessRobeMitsuba,
			PickupKind = PickupKind.DarknessRobe,
			IsHomeAppear = true,
			AdTipsIDs = new List<AdTipsID>
			{
				AdTipsID.Ad61,
				AdTipsID.Ad62
			}
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupNurseChaos,
			PickupKind = PickupKind.Nurse,
			IsHomeAppear = true,
			AdTipsIDs = new List<AdTipsID>
			{
				AdTipsID.Ad63,
				AdTipsID.Ad64
			}
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupCocoa,
			PickupKind = PickupKind.NewGirl,
			IsHomeAppear = true,
			AdTipsIDs = new List<AdTipsID>
			{
				AdTipsID.Ad65,
				AdTipsID.Ad66
			}
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.UnlockCocoa,
			PickupKind = PickupKind.Unlock,
			IsHomeAppear = false
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupClear4,
			PickupKind = PickupKind.NewGirl,
			RelationGachaSeriesName = GachaSeriesName.PickupMizugiClear,
			IsHomeAppear = false,
			IsReturens = true
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupGrimAroe2,
			PickupKind = PickupKind.NewGirl,
			RelationGachaSeriesName = GachaSeriesName.PickupGrimAroe,
			IsHomeAppear = true,
			IsReturens = true,
			AdTipsIDs = new List<AdTipsID> { AdTipsID.Ad67 }
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupXmasTenshi2,
			PickupKind = PickupKind.Xmas,
			IsHomeAppear = true,
			IsReturens = true,
			AdTipsIDs = new List<AdTipsID> { AdTipsID.Ad71 }
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupJiangshiIyo,
			PickupKind = PickupKind.Jiangshi,
			IsHomeAppear = true,
			AdTipsIDs = new List<AdTipsID>
			{
				AdTipsID.Ad68,
				AdTipsID.Ad69
			}
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupNewYearReturns1,
			PickupKind = PickupKind.NewYear,
			IsHomeAppear = true,
			IsReturens = true,
			AdTipsIDs = new List<AdTipsID> { AdTipsID.Ad70 }
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupFundoshiMusashi,
			PickupKind = PickupKind.Fundoshi,
			IsHomeAppear = true,
			AdTipsIDs = new List<AdTipsID>
			{
				AdTipsID.Ad72,
				AdTipsID.Ad73
			}
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupValentineToytoy2,
			PickupKind = PickupKind.Valentine,
			IsHomeAppear = true,
			IsReturens = true,
			AdTipsIDs = new List<AdTipsID>
			{
				AdTipsID.Ad25,
				AdTipsID.Ad76
			}
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupBellyDanceYao,
			PickupKind = PickupKind.BellyDance,
			IsHomeAppear = true,
			AdTipsIDs = new List<AdTipsID>
			{
				AdTipsID.Ad74,
				AdTipsID.Ad75
			}
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupAngelTenshi,
			PickupKind = PickupKind.Angel,
			IsHomeAppear = true,
			AdTipsIDs = new List<AdTipsID>
			{
				AdTipsID.Ad77,
				AdTipsID.Ad78
			}
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupDia,
			PickupKind = PickupKind.NewGirl,
			IsHomeAppear = true,
			AdTipsIDs = new List<AdTipsID>
			{
				AdTipsID.Ad79,
				AdTipsID.Ad80
			}
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.UnlockDia,
			PickupKind = PickupKind.Unlock,
			IsHomeAppear = false
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupIyo3,
			PickupKind = PickupKind.NewGirl,
			RelationGachaSeriesName = GachaSeriesName.PickupIyo,
			IsHomeAppear = false,
			IsReturens = true
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupGrimAroe3,
			PickupKind = PickupKind.NewGirl,
			RelationGachaSeriesName = GachaSeriesName.PickupGrimAroe,
			IsHomeAppear = false,
			IsReturens = true
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupCocoa2,
			PickupKind = PickupKind.NewGirl,
			RelationGachaSeriesName = GachaSeriesName.PickupCocoa,
			IsHomeAppear = true,
			IsReturens = true,
			AdTipsIDs = new List<AdTipsID> { AdTipsID.Ad81 }
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupLolitaToytoy,
			PickupKind = PickupKind.Lolita,
			IsHomeAppear = true,
			AdTipsIDs = new List<AdTipsID>
			{
				AdTipsID.Ad82,
				AdTipsID.Ad83
			}
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupGalHiyori,
			PickupKind = PickupKind.Gal,
			IsHomeAppear = true,
			AdTipsIDs = new List<AdTipsID>
			{
				AdTipsID.Ad84,
				AdTipsID.Ad85
			}
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupGalGrimAroe,
			PickupKind = PickupKind.Gal,
			IsHomeAppear = true,
			AdTipsIDs = new List<AdTipsID>
			{
				AdTipsID.Ad86,
				AdTipsID.Ad87
			}
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupGalClear,
			PickupKind = PickupKind.Gal,
			IsHomeAppear = true,
			AdTipsIDs = new List<AdTipsID>
			{
				AdTipsID.Ad88,
				AdTipsID.Ad89
			}
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupParodiusTsumire2,
			PickupKind = PickupKind.Parodius,
			IsHomeAppear = true,
			IsReturens = true,
			AdTipsIDs = new List<AdTipsID>
			{
				AdTipsID.Ad30,
				AdTipsID.Ad31
			}
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupAnimalMizugiCocoa,
			PickupKind = PickupKind.AnimalMizugi,
			IsHomeAppear = true,
			AdTipsIDs = new List<AdTipsID>
			{
				AdTipsID.Ad90,
				AdTipsID.Ad91
			}
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupMizugiReturns3,
			PickupKind = PickupKind.Mizugi,
			IsHomeAppear = true,
			IsReturens = true,
			AdTipsIDs = new List<AdTipsID> { AdTipsID.Ad92 }
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupParkaMusashi,
			PickupKind = PickupKind.Parka,
			IsHomeAppear = true,
			AdTipsIDs = new List<AdTipsID>
			{
				AdTipsID.Ad93,
				AdTipsID.Ad94
			}
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupAnimalMizugiDia,
			PickupKind = PickupKind.AnimalMizugi,
			IsHomeAppear = true,
			AdTipsIDs = new List<AdTipsID>
			{
				AdTipsID.Ad95,
				AdTipsID.Ad96
			}
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupSuccubusTenshi2,
			PickupKind = PickupKind.Succubus,
			IsHomeAppear = true,
			IsReturens = true,
			AdTipsIDs = new List<AdTipsID>
			{
				AdTipsID.Ad28,
				AdTipsID.Ad29
			}
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupDressChaos,
			PickupKind = PickupKind.Dress,
			IsHomeAppear = true,
			AdTipsIDs = new List<AdTipsID>
			{
				AdTipsID.Ad97,
				AdTipsID.Ad98
			}
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupRaceQueenHiyori2,
			PickupKind = PickupKind.RaceQueen,
			IsHomeAppear = true,
			IsReturens = true,
			AdTipsIDs = new List<AdTipsID>
			{
				AdTipsID.Ad32,
				AdTipsID.Ad99
			},
			IsExPickupOrderByDescending = true
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupMaidTsumire,
			PickupKind = PickupKind.Maid,
			IsHomeAppear = true,
			AdTipsIDs = new List<AdTipsID>
			{
				AdTipsID.Ad100,
				AdTipsID.Ad101
			}
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.MusicHiyori,
			PickupKind = PickupKind.Music,
			IsHomeAppear = true
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.MusicSen,
			PickupKind = PickupKind.Music,
			IsHomeAppear = true
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupShiori4,
			PickupKind = PickupKind.NewGirl,
			RelationGachaSeriesName = GachaSeriesName.PickupShiori,
			IsHomeAppear = false,
			IsReturens = true
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupChaos4,
			PickupKind = PickupKind.NewGirl,
			RelationGachaSeriesName = GachaSeriesName.PickupMizugiChaos,
			IsHomeAppear = false,
			IsReturens = true
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupClear5,
			PickupKind = PickupKind.NewGirl,
			RelationGachaSeriesName = GachaSeriesName.PickupMizugiClear,
			IsHomeAppear = false,
			IsReturens = true
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupIyo4,
			PickupKind = PickupKind.NewGirl,
			RelationGachaSeriesName = GachaSeriesName.PickupIyo,
			IsHomeAppear = false,
			IsReturens = true
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupGrimAroe4,
			PickupKind = PickupKind.NewGirl,
			RelationGachaSeriesName = GachaSeriesName.PickupGrimAroe,
			IsHomeAppear = false,
			IsReturens = true
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupCocoa3,
			PickupKind = PickupKind.NewGirl,
			RelationGachaSeriesName = GachaSeriesName.PickupCocoa,
			IsHomeAppear = false,
			IsReturens = true
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupDia2,
			PickupKind = PickupKind.NewGirl,
			RelationGachaSeriesName = GachaSeriesName.PickupDia,
			IsHomeAppear = true,
			IsReturens = true,
			AdTipsIDs = new List<AdTipsID> { AdTipsID.Ad102 }
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupDoubriel,
			PickupKind = PickupKind.NewGirl,
			IsHomeAppear = true,
			AdTipsIDs = new List<AdTipsID>
			{
				AdTipsID.Ad103,
				AdTipsID.Ad104
			}
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.UnlockDoubriel,
			PickupKind = PickupKind.Unlock,
			IsHomeAppear = false
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupPartTimerMitsuba,
			PickupKind = PickupKind.PartTimer,
			IsHomeAppear = true,
			AdTipsIDs = new List<AdTipsID>
			{
				AdTipsID.Ad105,
				AdTipsID.Ad106
			}
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupUniformYao2,
			PickupKind = PickupKind.Uniform,
			IsHomeAppear = true,
			IsReturens = true,
			AdTipsIDs = new List<AdTipsID>
			{
				AdTipsID.Ad43,
				AdTipsID.Ad44
			}
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupCinderellaMitsuba2,
			PickupKind = PickupKind.Cinderella,
			IsHomeAppear = true,
			IsReturens = true,
			AdTipsIDs = new List<AdTipsID> { AdTipsID.Ad21 }
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupAliceMaidPine2,
			PickupKind = PickupKind.AliceMaid,
			IsHomeAppear = true,
			IsReturens = true,
			AdTipsIDs = new List<AdTipsID>
			{
				AdTipsID.Ad35,
				AdTipsID.Ad36
			}
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupSchoolMizugiSen,
			PickupKind = PickupKind.SchoolMizugi,
			IsHomeAppear = true,
			AdTipsIDs = new List<AdTipsID>
			{
				AdTipsID.Ad107,
				AdTipsID.Ad108
			}
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.MusicYao,
			PickupKind = PickupKind.Music,
			IsHomeAppear = true
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupUniformMusashi2,
			PickupKind = PickupKind.Uniform,
			IsHomeAppear = true,
			IsReturens = true,
			AdTipsIDs = new List<AdTipsID> { AdTipsID.Ad40 }
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupSchoolMizugiIyo,
			PickupKind = PickupKind.SchoolMizugi,
			IsHomeAppear = true,
			AdTipsIDs = new List<AdTipsID>
			{
				AdTipsID.Ad109,
				AdTipsID.Ad110
			}
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupSchoolMizugiToytoy,
			PickupKind = PickupKind.SchoolMizugi,
			IsHomeAppear = true,
			AdTipsIDs = new List<AdTipsID>
			{
				AdTipsID.Ad111,
				AdTipsID.Ad112
			}
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupJerseyMitsuba2,
			PickupKind = PickupKind.Jersey,
			IsHomeAppear = true,
			IsReturens = true,
			AdTipsIDs = new List<AdTipsID>
			{
				AdTipsID.Ad38,
				AdTipsID.Ad39
			}
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupUniformSen3,
			PickupKind = PickupKind.Uniform2,
			IsHomeAppear = true,
			IsReturens = true,
			AdTipsIDs = new List<AdTipsID>
			{
				AdTipsID.Ad41,
				AdTipsID.Ad56
			}
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupBellyDancePine,
			PickupKind = PickupKind.BellyDance,
			IsHomeAppear = true,
			AdTipsIDs = new List<AdTipsID>
			{
				AdTipsID.Ad113,
				AdTipsID.Ad114
			}
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.MusicTenshi,
			PickupKind = PickupKind.Music,
			IsHomeAppear = true
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupYukataYao,
			PickupKind = PickupKind.Yukata,
			IsHomeAppear = true,
			AdTipsIDs = new List<AdTipsID>
			{
				AdTipsID.Ad115,
				AdTipsID.Ad116
			}
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupShiori5,
			PickupKind = PickupKind.NewGirl,
			RelationGachaSeriesName = GachaSeriesName.PickupShiori,
			IsHomeAppear = false,
			IsReturens = true
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupChaos5,
			PickupKind = PickupKind.NewGirl,
			RelationGachaSeriesName = GachaSeriesName.PickupMizugiChaos,
			IsHomeAppear = false,
			IsReturens = true
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupClear6,
			PickupKind = PickupKind.NewGirl,
			RelationGachaSeriesName = GachaSeriesName.PickupMizugiClear,
			IsHomeAppear = false,
			IsReturens = true
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupIyo5,
			PickupKind = PickupKind.NewGirl,
			RelationGachaSeriesName = GachaSeriesName.PickupIyo,
			IsHomeAppear = false,
			IsReturens = true
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupGrimAroe5,
			PickupKind = PickupKind.NewGirl,
			RelationGachaSeriesName = GachaSeriesName.PickupGrimAroe,
			IsHomeAppear = false,
			IsReturens = true
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupCocoa4,
			PickupKind = PickupKind.NewGirl,
			RelationGachaSeriesName = GachaSeriesName.PickupCocoa,
			IsHomeAppear = false,
			IsReturens = true
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupDia3,
			PickupKind = PickupKind.NewGirl,
			RelationGachaSeriesName = GachaSeriesName.PickupDia,
			IsHomeAppear = true,
			IsReturens = true
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupDoubriel2,
			PickupKind = PickupKind.NewGirl,
			RelationGachaSeriesName = GachaSeriesName.PickupDoubriel,
			IsHomeAppear = true,
			IsReturens = true,
			AdTipsIDs = new List<AdTipsID> { AdTipsID.Ad118 }
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupIppatsu,
			PickupKind = PickupKind.NewGirl,
			IsHomeAppear = true,
			AdTipsIDs = new List<AdTipsID>
			{
				AdTipsID.Ad119,
				AdTipsID.Ad120
			}
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.UnlockIppatsu,
			PickupKind = PickupKind.Unlock,
			IsHomeAppear = false
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupUniformTenshi,
			PickupKind = PickupKind.Uniform,
			IsHomeAppear = true,
			AdTipsIDs = new List<AdTipsID>
			{
				AdTipsID.Ad121,
				AdTipsID.Ad122
			}
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupMaidToytoyIyo,
			PickupKind = PickupKind.Maid,
			IsHomeAppear = true,
			AdTipsIDs = new List<AdTipsID>
			{
				AdTipsID.Ad123,
				AdTipsID.Ad124
			}
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupMizugiReturns4,
			PickupKind = PickupKind.Mizugi,
			IsHomeAppear = true,
			IsReturens = true
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupUniformDia,
			PickupKind = PickupKind.Uniform,
			IsHomeAppear = true,
			AdTipsIDs = new List<AdTipsID>
			{
				AdTipsID.Ad125,
				AdTipsID.Ad126
			}
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupYukataToytoy2,
			PickupKind = PickupKind.Yukata,
			IsHomeAppear = true,
			IsReturens = true,
			AdTipsIDs = new List<AdTipsID> { AdTipsID.Ad45 }
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupUniformDoubriel,
			PickupKind = PickupKind.Uniform,
			IsHomeAppear = true,
			AdTipsIDs = new List<AdTipsID>
			{
				AdTipsID.Ad127,
				AdTipsID.Ad128
			}
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.MusicMusashi,
			PickupKind = PickupKind.Music,
			IsHomeAppear = true
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.LimitedCharaReturns,
			PickupKind = PickupKind.LimitedReturns,
			IsHomeAppear = true,
			IsReturens = true,
			AdTipsIDs = new List<AdTipsID> { AdTipsID.Ad130 },
			LimitedCharaList = new List<CharaType>
			{
				CharaType.Chara10,
				CharaType.Chara11,
				CharaType.Chara12,
				CharaType.Chara13,
				CharaType.Chara14,
				CharaType.Chara15,
				CharaType.Chara16,
				CharaType.Chara17,
				CharaType.Chara18
			}
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupShiroe,
			PickupKind = PickupKind.NewGirl,
			IsHomeAppear = true,
			AdTipsIDs = new List<AdTipsID>
			{
				AdTipsID.Ad131,
				AdTipsID.Ad132
			}
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.UnlockShiroe,
			PickupKind = PickupKind.Unlock,
			IsHomeAppear = true
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupXmasGrimAroe,
			PickupKind = PickupKind.Xmas,
			IsHomeAppear = true,
			AdTipsIDs = new List<AdTipsID>
			{
				AdTipsID.Ad133,
				AdTipsID.Ad134
			}
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupMarchingIchiko,
			PickupKind = PickupKind.Marching,
			IsHomeAppear = true,
			AdTipsIDs = new List<AdTipsID>
			{
				AdTipsID.Ad135,
				AdTipsID.Ad136,
				AdTipsID.Ad137
			}
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupKimonoClear,
			PickupKind = PickupKind.Kimono,
			IsHomeAppear = true,
			AdTipsIDs = new List<AdTipsID>
			{
				AdTipsID.Ad138,
				AdTipsID.Ad139
			}
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupBomberPine2,
			PickupKind = PickupKind.Bomber,
			IsHomeAppear = true,
			IsReturens = true
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupLillyIppatsu,
			PickupKind = PickupKind.Lilly,
			IsHomeAppear = true,
			AdTipsIDs = new List<AdTipsID>
			{
				AdTipsID.Ad140,
				AdTipsID.Ad141
			}
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupSisterTsumire2,
			PickupKind = PickupKind.Sister,
			IsHomeAppear = true,
			IsReturens = true,
			AdTipsIDs = new List<AdTipsID> { AdTipsID.Ad55 }
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupAlotGrimAroe,
			PickupKind = PickupKind.Alot,
			IsHomeAppear = true,
			AdTipsIDs = new List<AdTipsID>
			{
				AdTipsID.Ad142,
				AdTipsID.Ad143
			}
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupDarknessRobeMitsuba2,
			PickupKind = PickupKind.DarknessRobe,
			IsHomeAppear = true,
			IsReturens = true,
			AdTipsIDs = new List<AdTipsID>
			{
				AdTipsID.Ad61,
				AdTipsID.Ad62
			}
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupValentineHiyori,
			PickupKind = PickupKind.Valentine,
			IsHomeAppear = true,
			AdTipsIDs = new List<AdTipsID>
			{
				AdTipsID.Ad144,
				AdTipsID.Ad145
			}
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupNurseChaos2,
			PickupKind = PickupKind.Nurse,
			IsHomeAppear = true,
			IsReturens = true,
			AdTipsIDs = new List<AdTipsID>
			{
				AdTipsID.Ad63,
				AdTipsID.Ad64
			}
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.MusicMitsuba,
			PickupKind = PickupKind.Music,
			IsHomeAppear = true
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupJun,
			PickupKind = PickupKind.NewGirl,
			IsHomeAppear = true,
			AdTipsIDs = new List<AdTipsID>
			{
				AdTipsID.Ad147,
				AdTipsID.Ad148
			}
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.UnlockJun,
			PickupKind = PickupKind.Unlock,
			IsHomeAppear = true
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.LimitedCharaReturns2,
			PickupKind = PickupKind.LimitedReturns,
			IsHomeAppear = true,
			IsReturens = true,
			AdTipsIDs = new List<AdTipsID> { AdTipsID.Ad149 },
			LimitedCharaList = new List<CharaType>
			{
				CharaType.Chara10,
				CharaType.Chara11,
				CharaType.Chara12,
				CharaType.Chara13,
				CharaType.Chara14,
				CharaType.Chara15,
				CharaType.Chara16,
				CharaType.Chara17,
				CharaType.Chara18,
				CharaType.Chara19
			}
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupGalSen,
			PickupKind = PickupKind.Gal,
			IsHomeAppear = true,
			AdTipsIDs = new List<AdTipsID> { AdTipsID.Ad150 }
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupGalCocoa,
			PickupKind = PickupKind.Gal,
			IsHomeAppear = true,
			AdTipsIDs = new List<AdTipsID> { AdTipsID.Ad151 }
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupGalDia,
			PickupKind = PickupKind.Gal,
			IsHomeAppear = true,
			AdTipsIDs = new List<AdTipsID> { AdTipsID.Ad152 }
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupGalReturns,
			PickupKind = PickupKind.Gal,
			IsHomeAppear = true,
			IsReturens = true,
			AdTipsIDs = new List<AdTipsID> { AdTipsID.Ad153 }
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupSuitChaos,
			PickupKind = PickupKind.Suit,
			IsHomeAppear = true,
			AdTipsIDs = new List<AdTipsID>
			{
				AdTipsID.Ad154,
				AdTipsID.Ad155
			}
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupJiangshiIyo2,
			PickupKind = PickupKind.Jiangshi,
			IsHomeAppear = true,
			AdTipsIDs = new List<AdTipsID>
			{
				AdTipsID.Ad68,
				AdTipsID.Ad69
			}
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupUniformShiroe,
			PickupKind = PickupKind.Uniform,
			IsHomeAppear = true,
			AdTipsIDs = new List<AdTipsID>
			{
				AdTipsID.Ad156,
				AdTipsID.Ad157
			}
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupMaidMusashi,
			PickupKind = PickupKind.Maid,
			IsHomeAppear = true,
			AdTipsIDs = new List<AdTipsID>
			{
				AdTipsID.Ad158,
				AdTipsID.Ad159
			}
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupSchoolMizugiTsumire,
			PickupKind = PickupKind.SchoolMizugi,
			IsHomeAppear = true,
			AdTipsIDs = new List<AdTipsID>
			{
				AdTipsID.Ad160,
				AdTipsID.Ad161
			}
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupFundoshiMusashi2,
			PickupKind = PickupKind.Fundoshi,
			IsHomeAppear = true,
			IsReturens = true,
			AdTipsIDs = new List<AdTipsID>
			{
				AdTipsID.Ad72,
				AdTipsID.Ad73
			}
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupBellyDanceYao2,
			PickupKind = PickupKind.BellyDance,
			IsHomeAppear = true,
			IsReturens = true,
			AdTipsIDs = new List<AdTipsID>
			{
				AdTipsID.Ad74,
				AdTipsID.Ad75
			}
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.MusicToytoy,
			PickupKind = PickupKind.Music,
			IsHomeAppear = true
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupTrainDia,
			PickupKind = PickupKind.Train,
			IsHomeAppear = true,
			AdTipsIDs = new List<AdTipsID>
			{
				AdTipsID.Ad162,
				AdTipsID.Ad163
			}
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupTouka,
			PickupKind = PickupKind.NewGirl,
			IsHomeAppear = true,
			AdTipsIDs = new List<AdTipsID>
			{
				AdTipsID.Ad165,
				AdTipsID.Ad166
			}
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.UnlockTouka,
			PickupKind = PickupKind.Unlock,
			IsHomeAppear = true
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.PickupMaidJun,
			PickupKind = PickupKind.Maid,
			IsHomeAppear = true,
			AdTipsIDs = new List<AdTipsID>
			{
				AdTipsID.Ad167,
				AdTipsID.Ad168
			}
		},
		new GachaExtraMasterData
		{
			GachaSeriesName = GachaSeriesName.LimitedCharaReturns3,
			PickupKind = PickupKind.LimitedReturns,
			IsHomeAppear = true,
			IsReturens = true,
			AdTipsIDs = new List<AdTipsID> { AdTipsID.Ad169 },
			LimitedCharaList = new List<CharaType>
			{
				CharaType.Chara10,
				CharaType.Chara11,
				CharaType.Chara12,
				CharaType.Chara13,
				CharaType.Chara14,
				CharaType.Chara15,
				CharaType.Chara16,
				CharaType.Chara17,
				CharaType.Chara18,
				CharaType.Chara19,
				CharaType.Chara20
			}
		}
	};

	public string AddressableAssetPath => AssetPathResolver.ResolveGachaBannerAssets(SeriesID);

	public string TitleAddressableAssetPath => AssetPathResolver.ResolveGachaTitleBannerAssets(SeriesID);

	public string IEndDateString
	{
		get
		{
			if (!(AddForceEndDate() != default(DateTimeOffset)))
			{
				return "";
			}
			return AddForceEndDate().ToString("yyyy年MM月dd日（ddd）HH：mmまで", new CultureInfo("ja-JP"));
		}
	}

	public GachaSeriesName GachaSeriesName => (GachaSeriesName)SeriesID;

	public GachaExtraMasterData ExtraMaster => GachaExtraMasters.Find((GachaExtraMasterData x) => x.GachaSeriesName == GachaSeriesName);

	public PickupKind PickupKind => ExtraMaster.PickupKind;

	public GachaSeriesInfo PlayerGachaInfo => GameDataManager.Gacha.GachaSeriesInfos.Find((GachaSeriesInfo a) => a.SeriesID == SeriesID);

	public bool IsTicketGacha()
	{
		return TicketNum > 0;
	}

	public string BgMonitorAddressableAssetPath()
	{
		return AssetPathResolver.ResolveGachaBgMonitorAssets(ExtraMaster.PickupKind, (PickUpCharas.Count > 0) ? PickUpCharas[0] : CharaType.Chara01);
	}

	private DateTimeOffset AddForceEndDate()
	{
		bool num = EndDate != null && !string.IsNullOrEmpty(EndDate);
		bool flag = ForceEndDate != null && !string.IsNullOrEmpty(ForceEndDate);
		if (num)
		{
			if (flag)
			{
				DateTimeOffset jSTFromString = DateTimeOffsetManager.GetJSTFromString(EndDate);
				DateTimeOffset jSTFromString2 = DateTimeOffsetManager.GetJSTFromString(ForceEndDate);
				if (!(jSTFromString2 >= jSTFromString))
				{
					return jSTFromString;
				}
				return jSTFromString2;
			}
			return DateTimeOffsetManager.GetJSTFromString(EndDate);
		}
		return default(DateTimeOffset);
	}

	public bool IsDisplay()
	{
		if (!IsActive)
		{
			return IsForceActive;
		}
		return true;
	}

	public bool IsGachaEnableFromChara()
	{
		return SeriesType switch
		{
			GachaSeriesType.Pickup => PickUpCharas.Exists((CharaType x) => GameDataManager.IsCharaSelectable(x)), 
			GachaSeriesType.Limited => ExtraMaster.LimitedCharaList.Exists((CharaType x) => GameDataManager.IsCharaSelectable(x)), 
			_ => true, 
		};
	}

	public bool IsGachaAllEnableFromChara()
	{
		return SeriesType switch
		{
			GachaSeriesType.Pickup => PickUpCharas.All((CharaType x) => GameDataManager.IsCharaSelectable(x)), 
			GachaSeriesType.Limited => ExtraMaster.LimitedCharaList.All((CharaType x) => GameDataManager.IsCharaSelectable(x)), 
			_ => true, 
		};
	}

	public string UnableReasonText()
	{
		try
		{
			if (SeriesType == GachaSeriesType.Limited)
			{
				return "ガチャ排出対象のファイトガールを\n1人以上解禁する必要があります";
			}
			List<CharaType> pickUpCharas = PickUpCharas;
			if (pickUpCharas != null && pickUpCharas.Contains(CharaType.Chara10))
			{
				return "e-amusement購買部での\n利用権の購入が必要です";
			}
			List<CharaType> list = PickUpCharas?.Where((CharaType x) => x >= CharaType.Chara11)?.ToList();
			if (list != null && list.Count > 0)
			{
				return MasterGameDataManager.Character.Properties(list.First()).Profile.Name + "を解禁する必要があります";
			}
		}
		catch
		{
		}
		return "";
	}

	public string GetExchangePointLabel(int i = 0)
	{
		return i switch
		{
			0 => "交換点(" + Label.Replace("ガチャ", "").Replace("ピックアップ", "") + ")", 
			1 => "所持中の交換点(" + Label.Replace("ガチャ", "").Replace("ピックアップ", "") + ")：", 
			_ => "交換点", 
		};
	}

	public int GetExchangePoint()
	{
		if (PlayerGachaInfo == null)
		{
			return 0;
		}
		return PlayerGachaInfo.ExchangePoint;
	}

	public void ConsumeExchangePoint(int consumeNum)
	{
		if (PlayerGachaInfo != null)
		{
			PlayerGachaInfo.ExchangePoint -= consumeNum;
		}
	}

	public void AddExchangePoint(int addNum)
	{
		if (PlayerGachaInfo != null)
		{
			PlayerGachaInfo.ExchangePoint += addNum;
			if (PlayerGachaInfo.ExchangePoint >= 9999)
			{
				PlayerGachaInfo.ExchangePoint = 9999;
			}
		}
	}

	public bool IsDisplayInfoNewGirl()
	{
		if (IsActive)
		{
			return ExtraMaster.IsHomeAppear;
		}
		return false;
	}

	public void SortItems()
	{
		Items = Items.SortItemID();
		ExchangeItems = ExchangeItems.SortItemIDUnlockGacha();
	}

	public bool IsPickitemsAllGet()
	{
		if (SeriesType != GachaSeriesType.Unlock && SeriesType != GachaSeriesType.Pickup)
		{
			return false;
		}
		bool flag = PickupItems.Count > 0;
		bool flag2 = LimitedItems.Count > 0;
		if (!flag && !flag2)
		{
			return false;
		}
		if (flag)
		{
			foreach (MFG.Types.Customize.CutinItem.ItemID item2 in PickupItems)
			{
				if (!GameDataManager.Customize.CutinItems.Exists((CutinItemInfo x) => x.ItemID == item2))
				{
					return false;
				}
			}
		}
		if (flag2)
		{
			foreach (MFG.Types.Customize.CutinItem.ItemID item in LimitedItems)
			{
				if (!GameDataManager.Customize.CutinItems.Exists((CutinItemInfo x) => x.ItemID == item))
				{
					return false;
				}
			}
		}
		return true;
	}

	public bool IsLimitedCutin(List<MFG.Types.Customize.CutinItem.ItemID> ids, bool isAll = false)
	{
		if (ids != null && ids.Count > 0)
		{
			if (isAll)
			{
				return ids.All((MFG.Types.Customize.CutinItem.ItemID x) => LimitedItems.Contains(x));
			}
			return ids.Exists((MFG.Types.Customize.CutinItem.ItemID x) => LimitedItems.Contains(x));
		}
		return false;
	}

	public List<(string oid, int no)> GetSortedMusicItems()
	{
		return (from x in MusicItems
			orderby ItemUtility.IsVoiceItem(x), ItemIDExtentions.MusicGachaItemNo(x), VoiceMaster.GetItem(x)?.GachaItemNo ?? 0
			select x).Select((string x, int i) => (x: x, i + 1)).ToList();
	}

	public List<string> GetGachaSelectMovies()
	{
		List<string> list = new List<string>();
		switch (SeriesType)
		{
		case GachaSeriesType.Pickup:
			if (PickupKind != PickupKind.NewGirl)
			{
				list.Add("CutinPlay");
			}
			try
			{
				foreach (CharaType pickUpChara in PickUpCharas)
				{
					list.Add(AssetPathResolver.ResolvePickUpGachaBgBase(pickUpChara));
					if (pickUpChara != CharaType.Chara11)
					{
						_ = 11;
					}
					if (pickUpChara <= CharaType.Chara12)
					{
						list.Add(AssetPathResolver.Resolve(PathType.CutinMovie, pickUpChara));
					}
				}
			}
			catch
			{
			}
			break;
		case GachaSeriesType.Unlock:
		{
			CharaType charaType = (((PickUpCharas?.Count ?? 0) == 1) ? PickUpCharas.First() : ((CharaType)(-1)));
			list.Add(AssetPathResolver.ResolvePickUpGachaBgMovie(PickupKind, charaType, ExtraMaster.GachaSeriesMovieSelector));
			break;
		}
		case GachaSeriesType.Limited:
			list.Add("CutinPlay");
			return list;
		}
		if (list == null || list.Count <= 0)
		{
			for (CharaType charaType2 = CharaType.Chara01; charaType2 <= CharaType.Chara09; charaType2++)
			{
				list.Add(AssetPathResolver.Resolve(PathType.CutinMovie, charaType2));
			}
		}
		return list;
	}
}
