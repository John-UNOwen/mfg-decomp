using System.Collections.Generic;
using MFG.GameData.EventTaku;
using MFG.Scenes.Home;
using MFG.Types;
using MFG.Types.EventTakuMaster;
using MFG.Types.Mission;

namespace MFG.MasterGameData;

public class EventTakuMaster : MasterGameDataBase
{
	private static List<MFG.Types.EventTakuMaster.Item> _items = new List<MFG.Types.EventTakuMaster.Item>
	{
		new MFG.Types.EventTakuMaster.Item
		{
			EventName = "トイトイのぶっとびギャラクシー",
			EventType = EventType.ToitoiButtobiGalaxy,
			CurrentFlagType = GameEventType.BlowAwaySanma,
			CompetitionFlagType = GameEventType.None,
			ConstancyFlagType = GameEventType.None,
			IsDispConstancySchedule = () => false,
			GameDataType = typeof(BlowawayEventGameData),
			GameDataFieldName = "_blowaways",
			HomeButtonIconSpritePath = "ToitoiGalaxy/Textures/BtModeToitoiGalaxy.png",
			HomeWindowPrefabPath = "ToitoiGalaxy/Prefabs/GalaxySanmaHome.prefab",
			HomeModePanelType = HomeModePanelType.ButtobiGalaxySanma,
			HomeModePanelPrefabPath = "ToitoiGalaxy/Prefabs/ModeToitoiGalaxySanmaBunner.prefab",
			HomeModePanelTypeConstancy = HomeModePanelType.None,
			HomeModePanelPrefabPathConstancy = "ToitoiGalaxy/Prefabs/ModeToitoiGalaxySanmaBunner.prefab",
			ResultWindowPrefabPath = "ToitoiGalaxy/Prefabs/GalaxySanmaResult.prefab",
			TopTitlePrefabPath = "ToitoiGalaxy/Prefabs/TopTitleToitoiGalaxy.prefab",
			BgmLableName = new string[2][][]
			{
				new string[2][]
				{
					new string[4] { "BGM_4MJG3_1", "BGM_4MJG3_2", "BGM_4MJG3_3", "BGM_4MJG3_4" },
					new string[4] { "BGM_4MJG3_1", "BGM_4MJG3_2", "BGM_4MJG3_3", "BGM_4MJG3_4" }
				},
				new string[2][]
				{
					new string[4] { "BGM_4MJG3_1", "BGM_4MJG3_1", "BGM_4MJG3_2", "" },
					new string[4] { "BGM_4MJG3_2", "BGM_4MJG3_3", "BGM_4MJG3_4", "" }
				}
			},
			BgmName = "トイトイのぶっ飛びギャラクシー",
			HomeNoticeText = "トイトイのぶっとびギャラクシー\n開催中！",
			EventItemName = "ButtobiMedal",
			EventItemDisplayName = "ぶっとびメダル",
			EventItemUnit = "枚"
		},
		new MFG.Types.EventTakuMaster.Item
		{
			EventName = "気炎万丈 炎のリーチ道場",
			EventType = EventType.FireReach,
			CurrentFlagType = GameEventType.FireReach2,
			ConstancyFlagType = GameEventType.ConstancyFireReach,
			CompetitionFlagType = GameEventType.None,
			IsDispConstancySchedule = () => GameDataManager.System.GameControlInfo.EventStatus.IsActive(GameEventType.ConstancyFireReachAppearance),
			GameDataType = typeof(FireReachEventGameData),
			GameDataFieldName = "_fireReaches",
			HomeButtonIconSpritePath = "FireReach/Textures/BtModeFireReach2.png",
			HomeWindowPrefabPath = "FireReach/Prefabs/FireReachHome2.prefab",
			HomeModePanelType = HomeModePanelType.FireReach,
			HomeModePanelPrefabPath = "FireReach/Prefabs/ModeFireReachBunner.prefab",
			HomeModePanelTypeConstancy = HomeModePanelType.FireReach,
			HomeModePanelPrefabPathConstancy = "FireReach/Prefabs/ModeFireReachBunner.prefab",
			ResultWindowPrefabPath = "FireReach/Prefabs/FireReachResult2.prefab",
			TopTitlePrefabPath = "FireReach/Prefabs/TopTitleFireReach.prefab",
			BgmLableName = new string[2][][]
			{
				new string[2][]
				{
					new string[4] { "BGM_4MJG4", "BGM_4MJG4", "BGM_4MJG4", "BGM_4MJG4" },
					new string[4] { "BGM_4MJG4", "BGM_4MJG4", "BGM_4MJG4", "BGM_4MJG4" }
				},
				new string[2][]
				{
					new string[4] { "BGM_4MJG4", "BGM_4MJG4", "BGM_4MJG4", "" },
					new string[4] { "BGM_4MJG4", "BGM_4MJG4", "BGM_4MJG4", "" }
				}
			},
			BgmName = "気炎万丈 炎のリーチ道場",
			HomeNoticeText = "気炎万丈 炎のリーチ道場\n開催中！",
			EventItemName = "Kumatsuba",
			EventItemDisplayName = "熊鍔",
			EventItemUnit = "枚"
		},
		new MFG.Types.EventTakuMaster.Item
		{
			EventName = "冥き終焉の暗黒ドラ卓",
			EventType = EventType.AccelDora,
			EnableTakuType = new bool[3] { true, true, false },
			CurrentFlagType = GameEventType.Competition7,
			ConstancyFlagType = GameEventType.ConstancyAccelDora,
			CompetitionFlagType = GameEventType.Competition7,
			IsDispConstancySchedule = () => GameDataManager.System.GameControlInfo.EventStatus.IsActive(GameEventType.ConstancyAccelDoraSchedule),
			GameDataType = typeof(AccelDoraEventGameData),
			GameDataFieldName = "_accelDoras",
			HomeButtonIconSpritePath = "",
			HomeWindowPrefabPath = "",
			HomeModePanelType = HomeModePanelType.AccelDora,
			HomeModePanelPrefabPath = "AccelDora/Prefabs/ModeAccelDoraBunner.prefab",
			HomeModePanelTypeSanma = HomeModePanelType.AccelDoraSanma,
			HomeModePanelPrefabPathSanma = "AccelDora/Prefabs/ModeAccelDoraSanmaBunner.prefab",
			HomeModePanelTypeConstancy = HomeModePanelType.AccelDora,
			HomeModePanelPrefabPathConstancy = "AccelDora/Prefabs/ModeAccelDoraBunner.prefab",
			ResultWindowPrefabPath = "",
			TopTitlePrefabPath = "AccelDora/Prefabs/TopTitle.prefab",
			BgmLableName = new string[2][][]
			{
				new string[2][]
				{
					new string[4] { "BGM_4MJG5", "BGM_4MJG5", "BGM_4MJG5", "BGM_4MJG5" },
					new string[4] { "BGM_4MJG5", "BGM_4MJG5", "BGM_4MJG5", "BGM_4MJG5" }
				},
				new string[2][]
				{
					new string[4] { "BGM_4MJG5", "BGM_4MJG5", "BGM_4MJG5", "" },
					new string[4] { "BGM_4MJG5", "BGM_4MJG5", "BGM_4MJG5", "" }
				}
			},
			BgmName = "冥き終焉の暗黒ドラ卓",
			HomeNoticeText = "冥き終焉の暗黒ドラ卓\n開催中！",
			EventItemName = "DarknessSphere",
			EventItemDisplayName = "暗黒キューブ",
			EventItemUnit = "個"
		},
		new MFG.Types.EventTakuMaster.Item
		{
			EventName = "壱高トリオのメンタンピン教室",
			EventType = EventType.Mentanpin,
			EnableTakuType = new bool[3] { true, true, false },
			CurrentFlagType = GameEventType.Competition8,
			ConstancyFlagType = GameEventType.ConstancyMentanpin,
			CompetitionFlagType = GameEventType.Competition8,
			IsDispConstancySchedule = () => GameDataManager.System.GameControlInfo.EventStatus.IsActive(GameEventType.ConstancyMentanpinSchedule),
			GameDataType = typeof(MentanpinEventGameData),
			GameDataFieldName = "_mentanpins",
			HomeButtonIconSpritePath = "Mentanpin/Textures/BtMode2.png",
			HomeWindowPrefabPath = "Mentanpin/Prefabs/HomeWindow2.prefab",
			HomeModePanelType = HomeModePanelType.Mentanpin,
			HomeModePanelPrefabPath = "Mentanpin/Prefabs/ModeBunner.prefab",
			HomeModePanelTypeSanma = HomeModePanelType.MentanpinSanma,
			HomeModePanelPrefabPathSanma = "Mentanpin/Prefabs/ModeBunner2.prefab",
			HomeModePanelTypeConstancy = HomeModePanelType.Mentanpin,
			HomeModePanelPrefabPathConstancy = "Mentanpin/Prefabs/ModeBunner.prefab",
			ResultWindowPrefabPath = "Mentanpin/Prefabs/ResultWindow2.prefab",
			TopTitlePrefabPath = "Mentanpin/Prefabs/TopTitle.prefab",
			BgmLableName = new string[2][][]
			{
				new string[2][]
				{
					new string[4] { "BGM_4MJG6", "BGM_4MJG6", "BGM_4MJG6", "BGM_4MJG6" },
					new string[4] { "BGM_4MJG6", "BGM_4MJG6", "BGM_4MJG6", "BGM_4MJG6" }
				},
				new string[2][]
				{
					new string[4] { "BGM_4MJG6", "BGM_4MJG6", "BGM_4MJG6", "" },
					new string[4] { "BGM_4MJG6", "BGM_4MJG6", "BGM_4MJG6", "" }
				}
			},
			BgmName = "壱高トリオのメンタンピン教室",
			HomeNoticeText = "壱高トリオのメンタンピン教室\n開催中！",
			EventItemName = "Daikon",
			EventItemDisplayName = "ネリマ大根",
			EventItemUnit = "本"
		},
		new MFG.Types.EventTakuMaster.Item
		{
			EventName = "究極青天井！炎のらぶはむ暗黒メンタンPCKギャラクシー霧雨突撃爆砕カタルシスですヨ！",
			EventType = EventType.Aotenjo,
			CurrentFlagType = GameEventType.AotenjoEvent3,
			ConstancyFlagType = GameEventType.None,
			CompetitionFlagType = GameEventType.None,
			IsDispConstancySchedule = () => false,
			GameDataType = typeof(AotenjoEventGameData),
			GameDataFieldName = "_aotenjos",
			HomeButtonIconSpritePath = "Aotenjo/Textures/BtMode2.png",
			HomeWindowPrefabPath = "Aotenjo/Prefabs/HomeWindow3.prefab",
			HomeModePanelType = HomeModePanelType.Aotenjo,
			HomeModePanelPrefabPath = "Aotenjo/Prefabs/ModeBunner3.prefab",
			HomeModePanelTypeConstancy = HomeModePanelType.Aotenjo,
			HomeModePanelPrefabPathConstancy = "Aotenjo/Prefabs/ModeBunner3.prefab",
			ResultWindowPrefabPath = "",
			TopTitlePrefabPath = "Aotenjo/Prefabs/TopTitle3.prefab",
			BgmLableName = new string[2][][]
			{
				new string[2][]
				{
					new string[4] { "BGM_4MJG7", "BGM_4MJG7", "BGM_4MJG7", "BGM_4MJG7" },
					new string[4] { "BGM_4MJG7", "BGM_4MJG7", "BGM_4MJG7", "BGM_4MJG7" }
				},
				new string[2][]
				{
					new string[4] { "BGM_4MJG7", "BGM_4MJG7", "BGM_4MJG7", "" },
					new string[4] { "BGM_4MJG7", "BGM_4MJG7", "BGM_4MJG7", "" }
				}
			},
			BgmName = "ULTiMATE INFLATiON",
			HomeNoticeText = "究極青天井！\n炎のらぶはむ暗黒メンタンPCKギャラクシー\n霧雨突撃爆砕カタルシスですヨ！  開催中！",
			EventItemName = "",
			EventItemDisplayName = "",
			EventItemUnit = ""
		},
		new MFG.Types.EventTakuMaster.Item
		{
			EventName = "六魂清浄 革命のカタルシス",
			EventType = EventType.Comeback,
			EnableTakuType = new bool[3] { true, true, false },
			CurrentFlagType = GameEventType.ComebackTakuEvent2,
			ConstancyFlagType = GameEventType.None,
			CompetitionFlagType = GameEventType.None,
			IsDispConstancySchedule = () => false,
			GameDataType = typeof(ComebackTakuEventGameData),
			GameDataFieldName = "_comebacks",
			HomeButtonIconSpritePath = "Comeback/Textures/BtMode2.png",
			HomeWindowPrefabPath = "Comeback/Prefabs/HomeWindow2.prefab",
			HomeModePanelType = HomeModePanelType.Comeback,
			HomeModePanelPrefabPath = "Comeback/Prefabs/ModeBunner.prefab",
			HomeModePanelTypeSanma = HomeModePanelType.ComebackSanma,
			HomeModePanelPrefabPathSanma = "Comeback/Prefabs/ModeBunner2.prefab",
			HomeModePanelTypeConstancy = HomeModePanelType.Comeback,
			HomeModePanelPrefabPathConstancy = "Comeback/Prefabs/ModeBunner.prefab",
			ResultWindowPrefabPath = "Comeback/Prefabs/ResultWindow2.prefab",
			TopTitlePrefabPath = "Comeback/Prefabs/TopTitle.prefab",
			BgmLableName = new string[2][][]
			{
				new string[2][]
				{
					new string[4] { "BGM_MJG_ECSTASY1", "BGM_MJG_ECSTASY1", "BGM_MJG_ECSTASY1", "BGM_MJG_ECSTASY1" },
					new string[4] { "BGM_MJG_ECSTASY1", "BGM_MJG_ECSTASY1", "BGM_MJG_ECSTASY1", "BGM_MJG_ECSTASY1" }
				},
				new string[2][]
				{
					new string[4] { "BGM_MJG_ECSTASY1", "BGM_MJG_ECSTASY1", "BGM_MJG_ECSTASY1", "" },
					new string[4] { "BGM_MJG_ECSTASY1", "BGM_MJG_ECSTASY1", "BGM_MJG_ECSTASY1", "" }
				}
			},
			BgmName = "六魂清浄 革命のカタルシス",
			ExtraBgmLableName = new string[2][][]
			{
				new string[2][]
				{
					new string[4] { "BGM_MJG_ECSTASY2", "BGM_MJG_ECSTASY2", "BGM_MJG_ECSTASY2", "BGM_MJG_ECSTASY2" },
					new string[4] { "BGM_MJG_ECSTASY2", "BGM_MJG_ECSTASY2", "BGM_MJG_ECSTASY2", "BGM_MJG_ECSTASY2" }
				},
				new string[2][]
				{
					new string[4] { "BGM_MJG_ECSTASY2", "BGM_MJG_ECSTASY2", "BGM_MJG_ECSTASY2", "" },
					new string[4] { "BGM_MJG_ECSTASY2", "BGM_MJG_ECSTASY2", "BGM_MJG_ECSTASY2", "" }
				}
			},
			ExtraBgmName = "六魂清浄 革命のカタルシス(革命ver.)",
			HomeNoticeText = "六魂清浄 革命のカタルシス\n開催中！",
			EventItemName = "Crowbar",
			EventItemDisplayName = "バール",
			EventItemUnit = "本",
			MainMissionID = MissionID.Comeback2_Item
		},
		new MFG.Types.EventTakuMaster.Item
		{
			EventName = "霧雨魔法店",
			EventType = EventType.Kirisame,
			CurrentFlagType = GameEventType.KirisameTakuEvent,
			ConstancyFlagType = GameEventType.None,
			CompetitionFlagType = GameEventType.None,
			IsDispConstancySchedule = () => false,
			GameDataType = typeof(KirisameTakuEventGameData),
			GameDataFieldName = "_kirisames",
			HomeButtonIconSpritePath = "Kirisame/Textures/BtMode.png",
			HomeWindowPrefabPath = "Kirisame/Prefabs/HomeWindow.prefab",
			HomeModePanelType = HomeModePanelType.Kirisame,
			HomeModePanelPrefabPath = "Kirisame/Prefabs/ModeBunner.prefab",
			HomeModePanelTypeConstancy = HomeModePanelType.Kirisame,
			HomeModePanelPrefabPathConstancy = "Kirisame/Prefabs/ModeBunner.prefab",
			ResultWindowPrefabPath = "Kirisame/Prefabs/ResultWindow.prefab",
			TopTitlePrefabPath = "Kirisame/Prefabs/TopTitle.prefab",
			BgmLableName = new string[2][][]
			{
				new string[2][]
				{
					new string[4] { "BGM_MJG_TOHO1", "BGM_MJG_TOHO1", "BGM_MJG_TOHO1", "BGM_MJG_TOHO1" },
					new string[4] { "BGM_MJG_TOHO1", "BGM_MJG_TOHO1", "BGM_MJG_TOHO1", "BGM_MJG_TOHO1" }
				},
				new string[2][]
				{
					new string[4] { "BGM_MJG_TOHO1", "BGM_MJG_TOHO1", "BGM_MJG_TOHO1", "" },
					new string[4] { "BGM_MJG_TOHO1", "BGM_MJG_TOHO1", "BGM_MJG_TOHO1", "" }
				}
			},
			BgmName = "千年ノ理",
			HomeNoticeText = "霧雨魔法店\n開催中！",
			EventItemName = "Pitem",
			EventItemDisplayName = "Pアイテム",
			EventItemUnit = "個"
		},
		new MFG.Types.EventTakuMaster.Item
		{
			EventName = "オペレーションPCK",
			EventType = EventType.MeldBonus,
			EnableTakuType = new bool[3] { true, true, false },
			CurrentFlagType = GameEventType.MeldBonusTakuEvent2,
			ConstancyFlagType = GameEventType.None,
			CompetitionFlagType = GameEventType.None,
			IsDispConstancySchedule = () => false,
			GameDataType = typeof(MeldBonusTakuEventGameData),
			GameDataFieldName = "_meldbonuses",
			HomeButtonIconSpritePath = "MeldBonus/Textures/BtMode2.png",
			HomeWindowPrefabPath = "MeldBonus/Prefabs/HomeWindow2.prefab",
			HomeModePanelType = HomeModePanelType.MeldBonus,
			HomeModePanelPrefabPath = "MeldBonus/Prefabs/ModeBunner.prefab",
			HomeModePanelTypeSanma = HomeModePanelType.MeldBonusSanma,
			HomeModePanelPrefabPathSanma = "MeldBonus/Prefabs/ModeBunner2.prefab",
			HomeModePanelTypeConstancy = HomeModePanelType.MeldBonus,
			HomeModePanelPrefabPathConstancy = "MeldBonus/Prefabs/ModeBunner.prefab",
			ResultWindowPrefabPath = "MeldBonus/Prefabs/ResultWindow2.prefab",
			TopTitlePrefabPath = "MeldBonus/Prefabs/TopTitle.prefab",
			BgmLableName = new string[2][][]
			{
				new string[2][]
				{
					new string[4] { "BGM_MJG_OPERATION1", "BGM_MJG_OPERATION1", "BGM_MJG_OPERATION1", "BGM_MJG_OPERATION1" },
					new string[4] { "BGM_MJG_OPERATION1", "BGM_MJG_OPERATION1", "BGM_MJG_OPERATION1", "BGM_MJG_OPERATION1" }
				},
				new string[2][]
				{
					new string[4] { "BGM_MJG_OPERATION1", "BGM_MJG_OPERATION1", "BGM_MJG_OPERATION1", "" },
					new string[4] { "BGM_MJG_OPERATION1", "BGM_MJG_OPERATION1", "BGM_MJG_OPERATION1", "" }
				}
			},
			BgmName = "オペレーションPCK",
			HomeNoticeText = "オペレーションPCK\n開催中！",
			EventItemName = "Kalbi",
			EventItemDisplayName = "カルビ",
			EventItemUnit = "皿",
			MainMissionID = MissionID.MeldBonus2_Item
		},
		new MFG.Types.EventTakuMaster.Item
		{
			EventName = "炎のぶっとびギャラクシー",
			EventType = EventType.FireGalaxy,
			EnableTakuType = new bool[3] { true, true, false },
			CurrentFlagType = GameEventType.Competition6,
			CompetitionFlagType = GameEventType.Competition6,
			ConstancyFlagType = GameEventType.None,
			IsDispConstancySchedule = () => false,
			GameDataType = typeof(FireGalaxyEventGameData),
			GameDataFieldName = "_fireGalaxys",
			HomeButtonIconSpritePath = "",
			HomeWindowPrefabPath = "",
			HomeModePanelType = HomeModePanelType.FireGalaxy,
			HomeModePanelPrefabPath = "FireGalaxy/Prefabs/ModeBunner.prefab",
			HomeModePanelTypeSanma = HomeModePanelType.FireGalaxySanma,
			HomeModePanelPrefabPathSanma = "FireGalaxy/Prefabs/ModeBunner2.prefab",
			HomeModePanelTypeConstancy = HomeModePanelType.None,
			HomeModePanelPrefabPathConstancy = "",
			ResultWindowPrefabPath = "",
			TopTitlePrefabPath = "FireGalaxy/Prefabs/TopTitle.prefab",
			BgmLableName = new string[2][][]
			{
				new string[2][]
				{
					new string[4] { "BGM_4MJG3_1", "BGM_4MJG3_2", "BGM_4MJG3_3", "BGM_4MJG3_4" },
					new string[4] { "BGM_4MJG3_1", "BGM_4MJG3_2", "BGM_4MJG3_3", "BGM_4MJG3_4" }
				},
				new string[2][]
				{
					new string[4] { "BGM_4MJG3_1", "BGM_4MJG3_1", "BGM_4MJG3_2", "" },
					new string[4] { "BGM_4MJG3_2", "BGM_4MJG3_3", "BGM_4MJG3_4", "" }
				}
			},
			BgmName = "トイトイのぶっ飛びギャラクシー",
			ExtraBgmLableName = new string[2][][]
			{
				new string[2][]
				{
					new string[4] { "BGM_4MJG3_1", "BGM_4MJG3_1", "BGM_4MJG3_1", "BGM_4MJG3_1" },
					new string[4] { "BGM_4MJG3_1", "BGM_4MJG3_1", "BGM_4MJG3_1", "BGM_4MJG3_1" }
				},
				new string[2][]
				{
					new string[4] { "BGM_4MJG3_1", "BGM_4MJG3_1", "BGM_4MJG3_1", "" },
					new string[4] { "BGM_4MJG3_1", "BGM_4MJG3_1", "BGM_4MJG3_1", "" }
				}
			},
			ExtraBgmName = "トイトイのぶっとびギャラクシー",
			HomeNoticeText = "炎のぶっとびギャラクシー\n開催中！",
			EventItemName = "",
			EventItemDisplayName = "",
			EventItemUnit = ""
		},
		new MFG.Types.EventTakuMaster.Item
		{
			EventName = "恋もツモれば！らぶらぶ突撃麻雀",
			EventType = EventType.Reversal,
			EnableTakuType = new bool[3] { false, true, false },
			CurrentFlagType = GameEventType.ReversalTakuEvent,
			ConstancyFlagType = GameEventType.None,
			CompetitionFlagType = GameEventType.None,
			IsDispConstancySchedule = () => false,
			GameDataType = typeof(ReversalEventGameData),
			GameDataFieldName = "_reversals",
			HomeButtonIconSpritePath = "Reversal/Textures/BtMode.png",
			HomeWindowPrefabPath = "Reversal/Prefabs/HomeWindow.prefab",
			HomeModePanelType = HomeModePanelType.None,
			HomeModePanelPrefabPath = "",
			HomeModePanelTypeSanma = HomeModePanelType.ReversalSanma,
			HomeModePanelPrefabPathSanma = "Reversal/Prefabs/ModeBunner.prefab",
			HomeModePanelTypeConstancy = HomeModePanelType.None,
			HomeModePanelPrefabPathConstancy = "",
			ResultWindowPrefabPath = "Reversal/Prefabs/ResultWindow.prefab",
			TopTitlePrefabPath = "Reversal/Prefabs/TopTitle.prefab",
			BgmLableName = new string[2][][]
			{
				new string[2][]
				{
					new string[4] { "BGM_MJG_ASSAULT1", "BGM_MJG_ASSAULT1", "BGM_MJG_ASSAULT1", "BGM_MJG_ASSAULT1" },
					new string[4] { "BGM_MJG_ASSAULT1", "BGM_MJG_ASSAULT1", "BGM_MJG_ASSAULT1", "BGM_MJG_ASSAULT1" }
				},
				new string[2][]
				{
					new string[4] { "BGM_MJG_ASSAULT1", "BGM_MJG_ASSAULT1", "BGM_MJG_ASSAULT1", "" },
					new string[4] { "BGM_MJG_ASSAULT1", "BGM_MJG_ASSAULT1", "BGM_MJG_ASSAULT1", "" }
				}
			},
			BgmName = "恋もツモれば！らぶらぶ突撃麻雀",
			HomeNoticeText = "恋もツモれば！らぶらぶ突撃麻雀\n開催中！",
			EventItemName = "Dumbbell",
			EventItemDisplayName = "ダンベル",
			EventItemUnit = "個",
			MainMissionID = MissionID.Reversal_Item
		},
		new MFG.Types.EventTakuMaster.Item
		{
			EventName = "天才！爆砕！ボンバー卓",
			EventType = EventType.Bomb,
			EnableTakuType = new bool[3] { true, true, false },
			CurrentFlagType = GameEventType.BombTakuEvent,
			ConstancyFlagType = GameEventType.None,
			CompetitionFlagType = GameEventType.None,
			IsDispConstancySchedule = () => false,
			GameDataType = typeof(BombEventGameData),
			GameDataFieldName = "_bombs",
			HomeButtonIconSpritePath = "Bomb/Textures/BtMode.png",
			HomeWindowPrefabPath = "Bomb/Prefabs/HomeWindow.prefab",
			HomeModePanelType = HomeModePanelType.Bomb,
			HomeModePanelPrefabPath = "Bomb/Prefabs/ModeBunner.prefab",
			HomeModePanelTypeSanma = HomeModePanelType.BombSanma,
			HomeModePanelPrefabPathSanma = "Bomb/Prefabs/ModeBunner2.prefab",
			HomeModePanelTypeConstancy = HomeModePanelType.None,
			HomeModePanelPrefabPathConstancy = "",
			ResultWindowPrefabPath = "Bomb/Prefabs/ResultWindow.prefab",
			TopTitlePrefabPath = "Bomb/Prefabs/TopTitle.prefab",
			BgmLableName = new string[2][][]
			{
				new string[2][]
				{
					new string[4] { "BGM_MJG_BOMB4_1", "BGM_MJG_BOMB4_2", "BGM_MJG_BOMB4_3", "BGM_MJG_BOMB4_4" },
					new string[4] { "BGM_MJG_BOMB4_1", "BGM_MJG_BOMB4_2", "BGM_MJG_BOMB4_3", "BGM_MJG_BOMB4_4" }
				},
				new string[2][]
				{
					new string[4] { "BGM_MJG_BOMB3_1", "BGM_MJG_BOMB3_2", "BGM_MJG_BOMB3_3", "" },
					new string[4] { "BGM_MJG_BOMB3_4", "BGM_MJG_BOMB3_5", "BGM_MJG_BOMB3_6", "" }
				}
			},
			BgmName = "天才！爆砕！ボンバー卓",
			BgmNames = new string[2][][]
			{
				new string[2][]
				{
					new string[4] { "機械仕掛けのコロシアム", "アクア城ドラキュラ", "KARAKURI-ROCK", "ボンバーガールズ！！" },
					new string[4] { "機械仕掛けのコロシアム", "アクア城ドラキュラ", "KARAKURI-ROCK", "ボンバーガールズ！！" }
				},
				new string[2][]
				{
					new string[4] { "ボムタウンでは日常茶飯事", "海・太陽・サメ", "聖邪は夢をみる", "" },
					new string[4] { "雪踊る村のケルト", "ボムノヤマヴォルケイノ", "ボンバーガールズ！！レインボー", "" }
				}
			},
			HomeNoticeText = "天才！爆砕！ボンバー卓\n開催中！",
			EventItemName = "ConeIce",
			EventItemDisplayName = "コーンアイス",
			EventItemUnit = "個",
			MainMissionID = MissionID.Bomb_Item
		},
		new MFG.Types.EventTakuMaster.Item
		{
			EventName = "GET READYですヨ！全緑卓",
			EventType = EventType.AllGreen,
			EnableTakuType = new bool[3] { true, true, false },
			CurrentFlagType = GameEventType.AllGreenTaku,
			ConstancyFlagType = GameEventType.None,
			CompetitionFlagType = GameEventType.None,
			IsDispConstancySchedule = () => false,
			GameDataType = typeof(AllGreenEventGameData),
			GameDataFieldName = "_allgreens",
			HomeButtonIconSpritePath = "AllGreen/Textures/BtMode.png",
			HomeWindowPrefabPath = "AllGreen/Prefabs/HomeWindow.prefab",
			HomeModePanelType = HomeModePanelType.AllGreen,
			HomeModePanelPrefabPath = "AllGreen/Prefabs/ModeBunner.prefab",
			HomeModePanelTypeSanma = HomeModePanelType.AllGreenSanma,
			HomeModePanelPrefabPathSanma = "AllGreen/Prefabs/ModeBunner2.prefab",
			HomeModePanelTypeConstancy = HomeModePanelType.None,
			HomeModePanelPrefabPathConstancy = "",
			ResultWindowPrefabPath = "AllGreen/Prefabs/ResultWindow.prefab",
			TopTitlePrefabPath = "AllGreen/Prefabs/TopTitle.prefab",
			BgmLableName = new string[2][][]
			{
				new string[2][]
				{
					new string[4] { "BGM_MJG_GREEN1", "BGM_MJG_GREEN1", "BGM_MJG_GREEN1", "BGM_MJG_GREEN1" },
					new string[4] { "BGM_MJG_GREEN1", "BGM_MJG_GREEN1", "BGM_MJG_GREEN1", "BGM_MJG_GREEN1" }
				},
				new string[2][]
				{
					new string[4] { "BGM_MJG_GREEN1", "BGM_MJG_GREEN1", "BGM_MJG_GREEN1", "" },
					new string[4] { "BGM_MJG_GREEN1", "BGM_MJG_GREEN1", "BGM_MJG_GREEN1", "" }
				}
			},
			BgmName = "GET READYですヨ！全緑卓",
			HomeNoticeText = "GET READYですヨ！全緑卓\n開催中！",
			EventItemName = "Battery",
			EventItemDisplayName = "乾電池",
			EventItemUnit = "個",
			MainMissionID = MissionID.AllGreen_Item
		},
		new MFG.Types.EventTakuMaster.Item
		{
			EventName = "奥義継承！らぶはむアトミックリーチ卓",
			EventType = EventType.Atomic,
			EnableTakuType = new bool[3] { true, true, false },
			CurrentFlagType = GameEventType.AtomicTaku,
			ConstancyFlagType = GameEventType.None,
			CompetitionFlagType = GameEventType.None,
			IsDispConstancySchedule = () => false,
			GameDataType = typeof(AtomicEventGameData),
			GameDataFieldName = "_atomics",
			HomeButtonIconSpritePath = "Atomic/Textures/BtMode.png",
			HomeWindowPrefabPath = "Atomic/Prefabs/HomeWindow.prefab",
			HomeModePanelType = HomeModePanelType.Atomic,
			HomeModePanelPrefabPath = "Atomic/Prefabs/ModeBunner.prefab",
			HomeModePanelTypeSanma = HomeModePanelType.AtomicSanma,
			HomeModePanelPrefabPathSanma = "Atomic/Prefabs/ModeBunner2.prefab",
			HomeModePanelTypeConstancy = HomeModePanelType.None,
			HomeModePanelPrefabPathConstancy = "",
			ResultWindowPrefabPath = "Atomic/Prefabs/ResultWindow.prefab",
			TopTitlePrefabPath = "Atomic/Prefabs/TopTitle.prefab",
			BgmLableName = new string[2][][]
			{
				new string[2][]
				{
					new string[4] { "BGM_MJG_ATOMIC1", "BGM_MJG_ATOMIC1", "BGM_MJG_ATOMIC1", "BGM_MJG_ATOMIC1" },
					new string[4] { "BGM_MJG_ATOMIC1", "BGM_MJG_ATOMIC1", "BGM_MJG_ATOMIC1", "BGM_MJG_ATOMIC1" }
				},
				new string[2][]
				{
					new string[4] { "BGM_MJG_ATOMIC1", "BGM_MJG_ATOMIC1", "BGM_MJG_ATOMIC1", "" },
					new string[4] { "BGM_MJG_ATOMIC1", "BGM_MJG_ATOMIC1", "BGM_MJG_ATOMIC1", "" }
				}
			},
			BgmName = "奥義継承！らぶはむアトミックリーチ卓",
			HomeNoticeText = "奥義継承！らぶはむアトミックリーチ卓\n開催中！",
			EventItemName = "Hanko",
			EventItemDisplayName = "スタンプ",
			EventItemUnit = "個",
			MainMissionID = MissionID.Atomic_Item
		},
		new MFG.Types.EventTakuMaster.Item
		{
			EventName = "オペレーションぶっとびギャラクシー",
			EventType = EventType.PCKGalaxy,
			EnableTakuType = new bool[3] { true, true, false },
			CurrentFlagType = GameEventType.Competition9,
			CompetitionFlagType = GameEventType.Competition9,
			ConstancyFlagType = GameEventType.None,
			IsDispConstancySchedule = () => false,
			GameDataType = typeof(PckGalaxyEventGameData),
			GameDataFieldName = "_pckGalaxys",
			HomeButtonIconSpritePath = "",
			HomeWindowPrefabPath = "",
			HomeModePanelType = HomeModePanelType.PCKGalaxy,
			HomeModePanelPrefabPath = "PCKGalaxy/Prefabs/ModeBunner.prefab",
			HomeModePanelTypeSanma = HomeModePanelType.PCKGalaxySanma,
			HomeModePanelPrefabPathSanma = "PCKGalaxy/Prefabs/ModeBunner2.prefab",
			HomeModePanelTypeConstancy = HomeModePanelType.None,
			HomeModePanelPrefabPathConstancy = "",
			ResultWindowPrefabPath = "",
			TopTitlePrefabPath = "PCKGalaxy/Prefabs/TopTitle.prefab",
			BgmLableName = new string[2][][]
			{
				new string[2][]
				{
					new string[4] { "BGM_4MJG3_1", "BGM_4MJG3_2", "BGM_4MJG3_3", "BGM_4MJG3_4" },
					new string[4] { "BGM_4MJG3_1", "BGM_4MJG3_2", "BGM_4MJG3_3", "BGM_4MJG3_4" }
				},
				new string[2][]
				{
					new string[4] { "BGM_4MJG3_1", "BGM_4MJG3_1", "BGM_4MJG3_2", "" },
					new string[4] { "BGM_4MJG3_2", "BGM_4MJG3_3", "BGM_4MJG3_4", "" }
				}
			},
			BgmName = "トイトイのぶっとびギャラクシー",
			ExtraBgmLableName = new string[2][][]
			{
				new string[2][]
				{
					new string[4] { "BGM_4MJG3_1", "BGM_4MJG3_1", "BGM_4MJG3_1", "BGM_4MJG3_1" },
					new string[4] { "BGM_4MJG3_1", "BGM_4MJG3_1", "BGM_4MJG3_1", "BGM_4MJG3_1" }
				},
				new string[2][]
				{
					new string[4] { "BGM_4MJG3_1", "BGM_4MJG3_1", "BGM_4MJG3_1", "" },
					new string[4] { "BGM_4MJG3_1", "BGM_4MJG3_1", "BGM_4MJG3_1", "" }
				}
			},
			ExtraBgmName = "トイトイのぶっとびギャラクシー",
			HomeNoticeText = "オペレーションぶっとびギャラクシー\n開催中！",
			EventItemName = "",
			EventItemDisplayName = "",
			EventItemUnit = ""
		}
	};

	public static List<MFG.Types.EventTakuMaster.Item> Items => _items;

	public static MFG.Types.EventTakuMaster.Item GetItem(EventType eventType)
	{
		return _items.Find((MFG.Types.EventTakuMaster.Item item) => item.EventType == eventType);
	}
}
