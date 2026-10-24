using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MFG.GameData;
using MFG.MasterGameData;
using MFG.Scenes.Home;
using MFG.Types;
using MFG.Types.AD;
using MFG.Types.Competition;
using MFG.Types.Customize.Catchphrase;
using MFG.Types.Customize.Costume;
using MFG.Types.Customize.CustomizeItem;
using MFG.Types.Customize.CutinItem;
using MFG.Types.EventTakuMaster;
using MFG.Types.PlayerItem;
using MFG.Types.Story;
using MFG.Types.YellCampaign;
using UnityEngine;

namespace MFG.Assets;

public static class AssetPathResolver
{
	public static class Catchphrase
	{
		public static string ResolveBaseImage(BaseImageID baseImageID)
		{
			return $"Catchphrase/{baseImageID}.png";
		}
	}

	public static class MatchingBG
	{
		public static string Resolve(CharaType charaType, string index)
		{
			return $"Characters/Chara{(int)(charaType + 1)}/MatchingBG/Icons/{index}.png";
		}

		public static string ResolveBG(CharaType charaType, string index)
		{
			return $"Characters/Chara{(int)(charaType + 1)}/MatchingBG/BGs/{index}.prefab";
		}
	}

	public static class SchoolEmblemIcon
	{
		public static string ResolveBaseImage(SchoolID schoolID)
		{
			return $"SchoolEmblem/{schoolID}.png";
		}
	}

	public static class ScrambleMatch
	{
		public static string GetScrambleTrophyPath(SchoolID school, ScrambleTrophyRank trophy)
		{
			if (trophy == ScrambleTrophyRank.None)
			{
				return "";
			}
			return $"ScrambleMatch/{school}/Textures/{trophy}.png";
		}
	}

	public static class MahjongActor
	{
		public static string ResolveActorIconPath(MahjongActorID mahjongActorID)
		{
			if (mahjongActorID == MahjongActorID.None)
			{
				return "";
			}
			return $"MahjongActor/Icon/{mahjongActorID}.png";
		}
	}

	public static class GachaSelect
	{
		public static string ResolveGetCharaImagePath(CharaType chara)
		{
			return $"Assets/MFG/Characters/Character{(int)(chara + 1)}/GachaSelect/Chara.png";
		}

		public static string ResolveGetCharaNamePath(CharaType chara)
		{
			return $"Assets/MFG/Characters/Character{(int)(chara + 1)}/GachaSelect/Name.png";
		}

		public static string ResolveGetCharaTextPath(CharaType chara)
		{
			return $"Assets/MFG/Characters/Character{(int)(chara + 1)}/GachaSelect/Text.png";
		}
	}

	private static Dictionary<TrophyType, CompetitionAssetType> _trophyIconAssetTypes = new Dictionary<TrophyType, CompetitionAssetType>
	{
		{
			TrophyType.Platinum,
			CompetitionAssetType.TrophyPlatinum
		},
		{
			TrophyType.Gold,
			CompetitionAssetType.TrophyGold
		},
		{
			TrophyType.Silver,
			CompetitionAssetType.TrophySilver
		},
		{
			TrophyType.Bronze,
			CompetitionAssetType.TrophyBronze
		}
	};

	private static Dictionary<TrophyType, CompetitionAssetType> _trophyIconPngAssetTypes = new Dictionary<TrophyType, CompetitionAssetType>
	{
		{
			TrophyType.Platinum,
			CompetitionAssetType.TrophyPlatinumIcon
		},
		{
			TrophyType.Gold,
			CompetitionAssetType.TrophyGoldIcon
		},
		{
			TrophyType.Silver,
			CompetitionAssetType.TrophySilverIcon
		},
		{
			TrophyType.Bronze,
			CompetitionAssetType.TrophyBronzeIcon
		}
	};

	private static Dictionary<TrophyType, CompetitionAssetType> _trophyIconMiniPngAssetTypes = new Dictionary<TrophyType, CompetitionAssetType>
	{
		{
			TrophyType.Platinum,
			CompetitionAssetType.TrophyPlatinumIconMini
		},
		{
			TrophyType.Gold,
			CompetitionAssetType.TrophyGoldIconMini
		},
		{
			TrophyType.Silver,
			CompetitionAssetType.TrophySilverIconMini
		},
		{
			TrophyType.Bronze,
			CompetitionAssetType.TrophyBronzeIconMini
		}
	};

	public static string Resolve(PathType pathType, CharaType charaType, params object[] args)
	{
		return pathType switch
		{
			PathType.CardConnectNotice => ResolveCardConnectNoticeAssets(charaType, (MFG.Types.Customize.CutinItem.ItemID)args[0]), 
			PathType.CharaSelect => ResolveCharaSelectAssets(charaType, (CharaSelectAssetType)args[0], (args.Length > 1) ? ((MFG.Types.Customize.Costume.ItemID)args[1]) : MFG.Types.Customize.Costume.ItemID.None), 
			PathType.Dojo => ResolveDojoAssets(charaType, (DojoFlow.AssetType)args[0]), 
			PathType.Result => ResolveResultAssets(charaType, (ResultAssetType)args[0]), 
			PathType.StandingPose => ResolveStandingPoseAssets(charaType, (CharacterDataManager.Emotion)args[0]), 
			PathType.CharaProperties => $"CharaProperties/{charaType}/Properties.asset", 
			PathType.BgScreen => $"BgScreen/{charaType}/{(string)args[0]}.png", 
			PathType.Awake => $"Awake/{charaType}Awake.prefab", 
			PathType.SdChara => $"SdChara/{charaType}/SdChara_Chara{(int)(charaType + 1)}.prefab", 
			PathType.CutinIcon => string.Format("CutinIcon/{0}/{1}.png", charaType, ((MFG.Types.Customize.CutinItem.ItemID)args[0]).ToString().Replace("Item", "ActIcon")), 
			PathType.CutinItem => ResolveCutinItemAssets(charaType, (CutinAssetType)args[0], (args.Length > 1) ? ((MFG.Types.Customize.CutinItem.Item)args[1]) : null), 
			PathType.Pro => ResolveProAssets(charaType, (ProAssetType)args[0]), 
			PathType.CutinMovie => ResolveCutinMovieAssets(charaType), 
			PathType.CharaArmAwayModel => ResolveCharaArmAwayModel(charaType, (Cha)args[0]), 
			PathType.CharaArmNakiModel => ResolveCharaArmNakiModel(charaType, (Cha)args[0]), 
			PathType.CharaVoice => ResolveCharaVoice(charaType, (string)args[0]), 
			_ => "", 
		};
	}

	private static string ResolveCardConnectNoticeAssets(CharaType charaType, MFG.Types.Customize.CutinItem.ItemID itemID)
	{
		Regex regex = new Regex("Item[0-9]+");
		return string.Format("CardConnectNotice/{0}/{1}.png", charaType, regex.Replace(itemID.ToString(), ""));
	}

	private static string ResolveCharaSelectAssets(CharaType charaType, CharaSelectAssetType assetType, MFG.Types.Customize.Costume.ItemID itemID)
	{
		return assetType switch
		{
			CharaSelectAssetType.CharaSelectButton => $"CharaSelect/{charaType}/BtCharaSelect{OneOriginNumber(charaType)}.png", 
			CharaSelectAssetType.DebutEffectCharaName => $"CharaSelect/{charaType}/DebutName.png", 
			CharaSelectAssetType.CostumeIcon => $"CharaSelect/{charaType}/Icon{itemID}.png", 
			_ => "", 
		};
	}

	private static string ResolveDojoAssets(CharaType charaType, DojoFlow.AssetType assetType)
	{
		return assetType switch
		{
			DojoFlow.AssetType.Lesson01 => $"Dojo/{charaType}/Lesson01.png", 
			DojoFlow.AssetType.Lesson02 => $"Dojo/{charaType}/Lesson02.png", 
			DojoFlow.AssetType.Rest01 => $"Dojo/{charaType}/Rest01.png", 
			DojoFlow.AssetType.Rest02 => $"Dojo/{charaType}/Rest02.png", 
			DojoFlow.AssetType.FightSpirit => $"Dojo/{charaType}/FightSpirit{OneOriginNumber(charaType)}.png", 
			_ => "", 
		};
	}

	private static string ResolveResultAssets(CharaType charaType, ResultAssetType assetType)
	{
		if (assetType == ResultAssetType.SdCharaTexture)
		{
			return $"Result/{charaType}/Chara{(int)(charaType + 1)}SdResultFan.png";
		}
		return "";
	}

	private static string ResolveStandingPoseAssets(CharaType charaType, CharacterDataManager.Emotion emotionType)
	{
		return emotionType switch
		{
			CharacterDataManager.Emotion.Default => $"StandingPose/{charaType}/Chara{(int)(charaType + 1)}_WholeBody.png", 
			CharacterDataManager.Emotion.Laugh => $"StandingPose/{charaType}/Chara{(int)(charaType + 1)}_WholeBodyLaugh.png", 
			CharacterDataManager.Emotion.Cry => $"StandingPose/{charaType}/Chara{(int)(charaType + 1)}_WholeBodyCry.png", 
			_ => "", 
		};
	}

	public static string StandingChara3DAssets(MFG.Types.Customize.Costume.ItemID itemID, StandingCharaAssetType standingCharaSceneType)
	{
		switch (standingCharaSceneType)
		{
		case StandingCharaAssetType.Home:
		case StandingCharaAssetType.CharaSelect:
		case StandingCharaAssetType.Story:
			return "Assets/MFG/Characters/Common/Motion3D/Prefabs/Standing3D.prefab";
		case StandingCharaAssetType.MahjongPlay:
			return $"Standing3D/{itemID}_Model.prefab";
		default:
			return "Standing3D/Costume01_001.prefab";
		}
	}

	public static string StandingChara2DAssets(MFG.Types.Customize.Costume.ItemID itemID, StandingCharaAssetType standingCharaSceneType)
	{
		if ((uint)standingCharaSceneType <= 2u)
		{
			return "Assets/MFG/Characters/Common/Motion3D/Prefabs/Standing2D.prefab";
		}
		return "Standing2D/Costume01_001_Home.prefab";
	}

	private static string ResolveCutinItemAssets(CharaType charaType, CutinAssetType assetType, MFG.Types.Customize.CutinItem.Item item)
	{
		Regex regex = new Regex("Item[0-9]+");
		switch (assetType)
		{
		case CutinAssetType.Houju:
			return $"CutinItem/{charaType}/Houju.png";
		case CutinAssetType.Houju2:
			return $"CutinItem/{charaType}/Houju2.png";
		case CutinAssetType.HoujuBlue:
			return $"CutinItem/{charaType}/HoujuBlue.png";
		case CutinAssetType.HoujuCutin:
			return $"CutinItem/{charaType}/HoujuCutIn.png";
		case CutinAssetType.PonPrefab:
		case CutinAssetType.ChiPrefab:
		case CutinAssetType.KanPrefab:
		case CutinAssetType.ReachPrefab:
		case CutinAssetType.AgariPrefab:
		{
			if (item == null)
			{
				Debug.LogError($"ResolveCutinItemAssets: item is null. {charaType},{assetType}");
				return "";
			}
			string text = string.Format("CutinItem/{0}/{1}.prefab", charaType, regex.Replace(item.ItemID.ToString(), ""));
			if (item.Category == MFG.Types.Customize.CutinItem.Category.Naki)
			{
				text = text.Replace("Naki", CutinAssetTypeToLabel(assetType));
			}
			return text;
		}
		default:
			return "";
		}
		static string CutinAssetTypeToLabel(CutinAssetType argCutinAssetType)
		{
			return argCutinAssetType switch
			{
				CutinAssetType.PonPrefab => "Pon", 
				CutinAssetType.ChiPrefab => "Chi", 
				CutinAssetType.KanPrefab => "Kan", 
				CutinAssetType.ReachPrefab => "Reach", 
				CutinAssetType.AgariPrefab => "Agari", 
				_ => "", 
			};
		}
	}

	private static string ResolveProAssets(CharaType charaType, ProAssetType assetType)
	{
		return assetType switch
		{
			ProAssetType.MatchingImage => $"Pro/{charaType}/Ma.png", 
			ProAssetType.MatchingName => $"Pro/{charaType}/MaName.png", 
			ProAssetType.ScoreCutinCry => $"Pro/{charaType}/MahjongPlayScoreCutInProCry.png", 
			ProAssetType.ScoreCutinJoy => $"Pro/{charaType}/MahjongPlayScoreCutInProJoy.png", 
			ProAssetType.ScoreCutinNormal => $"Pro/{charaType}/MahjongPlayScoreCutInProNormal.png", 
			ProAssetType.Stand => $"Pro/{charaType}/ProMahjongPlayStand.png", 
			ProAssetType.StandShimo => $"Pro/{charaType}/ProMahjongPlayStandShimo.png", 
			ProAssetType.StandToi => $"Pro/{charaType}/ProMahjongPlayStandToi.png", 
			ProAssetType.StandKami => $"Pro/{charaType}/ProMahjongPlayStandKami.png", 
			ProAssetType.StandResult01 => $"Pro/{charaType}/ProStand01.png", 
			ProAssetType.StandResult02 => $"Pro/{charaType}/ProStand02.png", 
			ProAssetType.StandResult03 => $"Pro/{charaType}/ProStand03.png", 
			ProAssetType.Agari => $"Pro/{charaType}/MaAgari.png", 
			_ => "", 
		};
	}

	public static string ResolveCustomizeItem(MFG.Types.Customize.CustomizeItem.ItemID itemID, MFG.Types.Customize.CustomizeItem.Category category)
	{
		return category switch
		{
			MFG.Types.Customize.CustomizeItem.Category.TableSticker => $"CustomizeItem/{itemID}.png", 
			MFG.Types.Customize.CustomizeItem.Category.TileSurface => $"CustomizeItem/{itemID}.png", 
			MFG.Types.Customize.CustomizeItem.Category.ReachStick => $"CustomizeItem/{itemID}.prefab", 
			MFG.Types.Customize.CustomizeItem.Category.PlayerAccessory => $"CustomizeItem/{itemID}.prefab", 
			MFG.Types.Customize.CustomizeItem.Category.StageBg => $"CustomizeItem/{itemID}.prefab", 
			_ => "", 
		};
	}

	public static string ResolveTileSurfaceSpriteAtlas(MFG.Types.Customize.CustomizeItem.ItemID itemID)
	{
		return $"CustomizeItem/{itemID}.spriteatlas";
	}

	public static string ResolveCustomizeIcon(MFG.Types.Customize.CustomizeItem.ItemID itemID)
	{
		return $"CustomizeIcon/Icon{itemID}.png";
	}

	public static string ResolveCustomizeRandomIcon(MFG.Types.Customize.CustomizeItem.Category category)
	{
		return category switch
		{
			MFG.Types.Customize.CustomizeItem.Category.ReachBgm => "CustomizeIcon/IconReachBgmRandom.png", 
			MFG.Types.Customize.CustomizeItem.Category.ReachStick => "CustomizeIcon/IconReachStickRandom.png", 
			MFG.Types.Customize.CustomizeItem.Category.TableSticker => "CustomizeIcon/IconTableStickerMulti.png", 
			_ => "", 
		};
	}

	public static string ResolvePlayersItemIcon(MFG.Types.PlayerItem.ItemID itemID)
	{
		return $"CustomizeIcon/Icon{itemID}.png";
	}

	public static string ResolveCompetitionAssets(CompetitionID competitionID, CompetitionAssetType assetType)
	{
		switch (assetType)
		{
		case CompetitionAssetType.BannerTitle:
			return $"Competition/{competitionID}/BannerTitle.png";
		case CompetitionAssetType.TrophyPlatinum:
		case CompetitionAssetType.TrophyGold:
		case CompetitionAssetType.TrophySilver:
		case CompetitionAssetType.TrophyBronze:
			return $"Competition/{competitionID}/{assetType}.prefab";
		case CompetitionAssetType.TrophyPlatinumIcon:
		case CompetitionAssetType.TrophyGoldIcon:
		case CompetitionAssetType.TrophySilverIcon:
		case CompetitionAssetType.TrophyBronzeIcon:
		case CompetitionAssetType.TrophyPlatinumIconMini:
		case CompetitionAssetType.TrophyGoldIconMini:
		case CompetitionAssetType.TrophySilverIconMini:
		case CompetitionAssetType.TrophyBronzeIconMini:
			return $"Competition/{competitionID}/{assetType}.png";
		case CompetitionAssetType.Title:
			return $"Competition/{competitionID}/Title.png";
		case CompetitionAssetType.ScoreWindowTitleBannerPrefab:
			return $"Competition/{competitionID}/TitleBanner.prefab";
		case CompetitionAssetType.TitleScreenBannerPrefab:
			return $"Competition/{competitionID}/BannerCompe.prefab";
		case CompetitionAssetType.MahjongPlayCaptionPrefab:
			return $"Competition/{competitionID}/TopTitleCompe.prefab";
		default:
			return "";
		}
	}

	public static string ResolveCompetitionTrophyAssets(CompetitionID competitionID, TrophyType trophyType)
	{
		return ResolveCompetitionAssets(competitionID, _trophyIconAssetTypes[trophyType]);
	}

	public static string ResolveCompetitionTrophyIconAssets(CompetitionID competitionID, TrophyType trophyType)
	{
		return ResolveCompetitionAssets(competitionID, _trophyIconPngAssetTypes[trophyType]);
	}

	public static string ResolveCompetitionTrophyIconMiniAssets(CompetitionID competitionID, TrophyType trophyType)
	{
		return ResolveCompetitionAssets(competitionID, _trophyIconMiniPngAssetTypes[trophyType]);
	}

	public static string ResolveCompetitionScoreWindowAssets(CharaType charaType, CompetitionScoreWindowAssetType competitionScoreWindowAssetType)
	{
		if (charaType < CharaType.Chara01)
		{
			charaType = CharaType.Chara01;
		}
		return competitionScoreWindowAssetType switch
		{
			CompetitionScoreWindowAssetType.BgTexture => $"Competition/{charaType}/EntryCharaBg.png", 
			CompetitionScoreWindowAssetType.EntryCharaNameTexture => $"Competition/{charaType}/EntryCharaName.png", 
			CompetitionScoreWindowAssetType.EntryCharaIconTexture => $"Competition/{charaType}/EntryCharaIcon.png", 
			_ => "", 
		};
	}

	private static string ResolveCharaArmAwayModel(CharaType charaType, Cha cha)
	{
		return "ArmMotion/Prefabs/" + ChaToPathName(cha) + "Hand.prefab";
		static string ChaToPathName(Cha _cha)
		{
			return _cha switch
			{
				Cha.Jicha => "Zi", 
				Cha.Kamicha => "Kami", 
				Cha.Toimen => "Toi", 
				Cha.Simocha => "Shimo", 
				_ => "", 
			};
		}
	}

	private static string ResolveCharaArmNakiModel(CharaType charaType, Cha cha)
	{
		return "ArmMotion/Prefabs/" + ChaToPathName(cha) + "HandNaki.prefab";
		static string ChaToPathName(Cha _cha)
		{
			return _cha switch
			{
				Cha.Jicha => "Zi", 
				Cha.Kamicha => "Kami", 
				Cha.Toimen => "Toi", 
				Cha.Simocha => "Shimo", 
				_ => "", 
			};
		}
	}

	private static string ResolveCharaVoice(CharaType charaType, string label)
	{
		return $"Assets/MFG/Characters/Character{(int)(charaType + 1)}/Sound/" + label + ".wav";
	}

	public static string getChaName(Cha _cha)
	{
		return _cha switch
		{
			Cha.Jicha => "Zi", 
			Cha.Kamicha => "Kami", 
			Cha.Toimen => "Toi", 
			Cha.Simocha => "Shimo", 
			_ => "", 
		};
	}

	private static GachaSeriesName GetNewGirlReturensSeriesName(int seriesID)
	{
		GachaSeriesName seriesName = (GachaSeriesName)seriesID;
		GachaExtraMasterData gachaExtraMasterData = GachaSeriesServerInfo.GachaExtraMasters.Find((GachaExtraMasterData x) => x.GachaSeriesName == seriesName);
		if (gachaExtraMasterData != null && gachaExtraMasterData.PickupKind == PickupKind.NewGirl && gachaExtraMasterData.IsReturens)
		{
			Match match = Regex.Match(seriesName.ToString(), "^(Pickup.*?)(\\d+)$");
			if (match.Success)
			{
				string value = match.Groups[1].Value;
				string value2 = (seriesName.ToString().Contains("Shiori") ? (value + "3") : (value + "2"));
				seriesName = (GachaSeriesName)Enum.Parse(typeof(GachaSeriesName), value2);
				seriesID = (int)seriesName;
			}
		}
		return seriesName;
	}

	public static string ResolveGachaBannerAssets(int seriesID)
	{
		if (Enum.GetValues(typeof(GachaSeriesName)).Length <= seriesID)
		{
			return "";
		}
		seriesID = (int)GetNewGirlReturensSeriesName(seriesID);
		return string.Format("Banner/BannerGacha{0}{1}.prefab", seriesID.ToString("D3"), (GachaSeriesName)seriesID);
	}

	public static string ResolveGachaTitleBannerAssets(int seriesID)
	{
		if (Enum.GetValues(typeof(GachaSeriesName)).Length <= seriesID)
		{
			return "";
		}
		GachaSeriesName newGirlReturensSeriesName = GetNewGirlReturensSeriesName(seriesID);
		seriesID = (int)newGirlReturensSeriesName;
		return string.Format("GachaPlayBG/Textures/GachaTitle{0}{1}.png", seriesID.ToString("D3"), newGirlReturensSeriesName);
	}

	public static string ResolveGachaBgMonitorAssets(PickupKind kind, CharaType chara = CharaType.Chara01)
	{
		return kind switch
		{
			PickupKind.NewGirl => chara switch
			{
				CharaType.Chara09 => "GachaPlayBG/Textures/BgMonitorPine.png", 
				CharaType.Chara10 => "GachaPlayBG/Textures/BgMonitorShiori.png", 
				CharaType.Chara11 => "GachaPlayBG/Textures/BgMonitorChaos.png", 
				CharaType.Chara12 => "GachaPlayBG/Textures/BgMonitorClear.png", 
				CharaType.Chara13 => "GachaPlayBG/Textures/BgMonitorIyo.png", 
				CharaType.Chara14 => "GachaPlayBG/Textures/BgMonitorGrimAroe.png", 
				CharaType.Chara15 => "GachaPlayBG/Textures/BgMonitorCocoa.png", 
				CharaType.Chara16 => "GachaPlayBG/Textures/BgMonitorDia.png", 
				CharaType.Chara17 => "GachaPlayBG/Textures/BgMonitorDoubriel.png", 
				CharaType.Chara18 => "GachaPlayBG/Textures/BgMonitorIppatsu.png", 
				CharaType.Chara19 => "GachaPlayBG/Textures/BgMonitorShiroe.png", 
				CharaType.Chara20 => "GachaPlayBG/Textures/BgMonitorJun.png", 
				CharaType.Chara21 => "GachaPlayBG/Textures/BgMonitorTouka.png", 
				_ => "GachaPlayBG/Textures/BgMonitor.png", 
			}, 
			PickupKind.Unlock => chara switch
			{
				CharaType.Chara12 => "GachaPlayBG/Textures/BgMonitorClear.png", 
				CharaType.Chara13 => "GachaPlayBG/Textures/BgMonitorIyo.png", 
				CharaType.Chara14 => "GachaPlayBG/Textures/BgMonitorGrimAroe.png", 
				CharaType.Chara15 => "GachaPlayBG/Textures/BgMonitorCocoa.png", 
				CharaType.Chara16 => "GachaPlayBG/Textures/BgMonitorDia.png", 
				CharaType.Chara17 => "GachaPlayBG/Textures/BgMonitorDoubriel.png", 
				CharaType.Chara18 => "GachaPlayBG/Textures/BgMonitorIppatsu.png", 
				CharaType.Chara19 => "GachaPlayBG/Textures/BgMonitorShiroe.png", 
				CharaType.Chara20 => "GachaPlayBG/Textures/BgMonitorJun.png", 
				CharaType.Chara21 => "GachaPlayBG/Textures/BgMonitorTouka.png", 
				_ => "GachaPlayBG/Textures/BgMonitor.png", 
			}, 
			PickupKind.Mizugi => "GachaPlayBG/Textures/BgMonitorBeach.png", 
			_ => "GachaPlayBG/Textures/BgMonitor.png", 
		};
	}

	public static string ResolveHomeBannerAssets(string prefabName)
	{
		if (string.IsNullOrEmpty(prefabName))
		{
			return "";
		}
		Dictionary<string, string> dictionary = new Dictionary<string, string>
		{
			{ "PaseliPointCampaignBanner", "BannerEventPaseliPointCampaign001" },
			{ "BtGachaSeriesPickup000", "BannerGachaPickup002Pine" },
			{ "BtEventBingo000", "BannerEventBingo000" },
			{ "BtGachaSeriesPickup001", "BannerGacha003PickupMizugiSen" },
			{ "MizugiMitubaPop", "BannerCostumeMizugiMitsubaPop" },
			{ "BtCompeBanner", "BannerCompetition001" },
			{ "PaseliPointCampaignBanner2", "BannerEventPaseliPointCampaign002" },
			{ "MizugiMusashiPop", "BannerCostumeMizugiMusashiPop" },
			{ "BtGowanBanner", "BannerEventTenshiGowan" }
		};
		if (dictionary.ContainsKey(prefabName))
		{
			return "Banner/" + dictionary[prefabName] + ".prefab";
		}
		return "Banner/" + prefabName + ".prefab";
	}

	public static string ResolveYakumanCutin(Yaku yaku)
	{
		switch (yaku)
		{
		case Yaku.Tenho:
			return "CutinItem/YakumanTenhou.prefab";
		case Yaku.Chiho:
			return "CutinItem/YakumanChihou.prefab";
		case Yaku.Tyuren9:
		case Yaku.TyurenTanki:
			return "CutinItem/YakumanChuren.prefab";
		case Yaku.Chinroto:
			return "CutinItem/YakumanChinro.prefab";
		case Yaku.Tsuiso:
			return "CutinItem/YakumanTuiso.prefab";
		case Yaku.Ryuiso:
			return "CutinItem/YakumanRyuiso.prefab";
		case Yaku.Kokushi13:
		case Yaku.KokushiTanki:
			return "CutinItem/YakumanKokushi.prefab";
		case Yaku.Sukantsu:
			return "CutinItem/YakumanSukantu.prefab";
		case Yaku.Suanko:
		case Yaku.SuankoTanki:
			return "CutinItem/YakumanSuan.prefab";
		case Yaku.Daisushi:
			return "CutinItem/YakumanDaisushi.prefab";
		case Yaku.Syosushi:
			return "CutinItem/YakumanShousushi.prefab";
		case Yaku.Daisangen:
			return "CutinItem/YakumanDaisangen.prefab";
		case Yaku.Kazoeyakuman:
			return "CutinItem/YakumanKazoe.prefab";
		default:
			Debug.LogError("想定外のケース");
			return "";
		}
	}

	public static string ResolveStoryImage(string imageName)
	{
		return "Assets/MFG/Story/Images/" + imageName + ".png";
	}

	public static string ResolveStoryScript(StoryType storyType, CharaType charaType, int part, int chapter)
	{
		string text = "";
		int num = (int)(charaType + 1);
		for (int i = 0; i < 3 - num.ToString().Length; i++)
		{
			text += "0";
		}
		text += num;
		string text2 = "";
		for (int j = 0; j < 2 - part.ToString().Length; j++)
		{
			text2 += "0";
		}
		text2 += part;
		string text3 = "";
		for (int k = 0; k < 2 - chapter.ToString().Length; k++)
		{
			text3 += "0";
		}
		text3 += chapter;
		switch (storyType)
		{
		case StoryType.EventStory:
			return "Assets/MFG/Story/StoryScripts/StoryScriptEvent" + text2 + text3 + ".csv";
		case StoryType.GirlsMainStory:
			return "Assets/MFG/Story/StoryScripts/StoryScriptChara" + text + "Main" + text2 + text3 + ".csv";
		case StoryType.GirlsLoveStory:
			return "Assets/MFG/Story/StoryScripts/StoryScriptChara" + text + "Love" + text2 + text3 + ".csv";
		default:
			Debug.LogError("想定外のケース");
			return "";
		}
	}

	public static string ResolveStoryThumbnail(StoryType storyType, CharaType charaType, int part, int chapter)
	{
		string text = "";
		int num = (int)(charaType + 1);
		for (int i = 0; i < 3 - num.ToString().Length; i++)
		{
			text += "0";
		}
		text += num;
		string text2 = "";
		for (int j = 0; j < 2 - part.ToString().Length; j++)
		{
			text2 += "0";
		}
		text2 += part;
		string text3 = "";
		for (int k = 0; k < 2 - chapter.ToString().Length; k++)
		{
			text3 += "0";
		}
		text3 += chapter;
		switch (storyType)
		{
		case StoryType.EventStory:
			return "Assets/MFG/Story/Thumbnails/StoryImageEvent" + text2 + text3 + ".png";
		case StoryType.GirlsMainStory:
			return "Assets/MFG/Story/Thumbnails/StoryImageChara" + text + "Main" + text2 + text3 + ".png";
		case StoryType.GirlsLoveStory:
			return "Assets/MFG/Story/Thumbnails/StoryImageChara" + text + "Love" + text2 + text3 + ".png";
		default:
			Debug.LogError("想定外のケース");
			return "";
		}
	}

	public static string ResolveTips(List<string> adTipsList)
	{
		if (adTipsList.Count > 0 && UnityEngine.Random.Range(0f, 1f) >= 0.5f)
		{
			string text = adTipsList[UnityEngine.Random.Range(0, adTipsList.Count)];
			return "Tips/" + text + ".png";
		}
		List<string> list = new List<string>();
		list.Add(TipsID.Tips1.ToString());
		list.Add(TipsID.Tips2.ToString());
		list.Add(TipsID.Tips4.ToString());
		list.Add(TipsID.Tips6.ToString());
		list.Add(TipsID.Tips7.ToString());
		if (GameDataManager.IsLoadJandriAppear())
		{
			list.Add(TipsID.Tips3.ToString());
		}
		if (GameDataManager.IsQRLoginEnable())
		{
			list.Add(TipsID.Tips5.ToString());
		}
		return "Tips/" + list[UnityEngine.Random.Range(0, list.Count)] + ".png";
	}

	private static string OneOriginNumber(CharaType charaType)
	{
		return ((int)(charaType + 1)).ToString("00");
	}

	public static string ResolveCutinMovieAssets(CharaType charaType)
	{
		return Application.streamingAssetsPath + "/" + $"Characters/Chara{(int)(charaType + 1)}/SsrMangan.webm";
	}

	public static string ResolvePickUpGachaBgMovie(PickupKind kind, CharaType charaType, GachaSeriesName gachaSeriesName)
	{
		if (kind != 0)
		{
			if (charaType == (CharaType)(-1))
			{
				return $"{Application.streamingAssetsPath}/Characters/Common/GachaSelect/{gachaSeriesName}.webm";
			}
			return $"{Application.streamingAssetsPath}/Characters/Chara{(int)(charaType + 1)}/GachaSelect/{gachaSeriesName}.webm";
		}
		return "";
	}

	public static string ResolvePickUpGachaBgBase(CharaType charaType)
	{
		if (charaType == CharaType.Chara10)
		{
			return Application.streamingAssetsPath + "/Characters/Chara10/GachaSelect/PickupShiori.webm";
		}
		return $"{Application.streamingAssetsPath}/Characters/Chara{(int)(charaType + 1)}/GachaSelect/Base.webm";
	}

	public static string ResolvePickUpGachaBgBase2(CharaType charaType)
	{
		return $"{Application.streamingAssetsPath}/Characters/Chara{(int)(charaType + 1)}/GachaSelect/Base2.webm";
	}

	public static string ResolveNoticeImage(string str)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>
		{
			{ "Banner01Start", "NoticeInfoStart01" },
			{ "Banner02Spirit50Present", "NoticeInfoStart02" },
			{ "PaseliPointCampaignBanner", "NoticeEventPaseliPointCampaign001" },
			{ "GachaTitlePickup000", "NoticeGacha002PickupPine" },
			{ "EventBingo", "NoticeEventBingo000" },
			{ "GachaTitlePainPickUp001", "NoticeGacha003PickupMizugiSen" },
			{ "MizugiMitubaPop", "NoticeCostumeMizugiMitsubaPop" },
			{ "Title", "NoticeCompetitionEmerald001" },
			{ "PaseliPointCampaignBanner2", "NoticeEventPaseliPointCampaign002" },
			{ "MizugiMusashiPop", "NoticeCostumeMizugiMusashiPop" },
			{ "TenshiGowanEvent", "NoticeEventTenshiGowan" }
		};
		if (dictionary.ContainsKey(str))
		{
			return "NoticeImage/" + dictionary[str] + ".png";
		}
		return "NoticeImage/" + str + ".png";
	}

	public static string ResolveHomeModeSelectPanel(HomeModePanelType homeModePanelType, TakuType takuType)
	{
		try
		{
			switch (takuType)
			{
			case TakuType.Tonpu:
				return EventTakuMaster.Items.Where((MFG.Types.EventTakuMaster.Item x) => x.HomeModePanelType == homeModePanelType).First().HomeModePanelPrefabPath;
			case TakuType.Sanma:
				return EventTakuMaster.Items.Where((MFG.Types.EventTakuMaster.Item x) => x.HomeModePanelTypeSanma == homeModePanelType).First().HomeModePanelPrefabPathSanma;
			}
		}
		catch
		{
		}
		try
		{
			return EventTakuMaster.Items.Where((MFG.Types.EventTakuMaster.Item x) => x.HomeModePanelTypeConstancy == homeModePanelType).First().HomeModePanelPrefabPathConstancy;
		}
		catch
		{
		}
		return homeModePanelType switch
		{
			HomeModePanelType.Yonma => "HomeModeBanner/Prefabs/Mode4Ton.prefab", 
			HomeModePanelType.Sanma => "HomeModeBanner/Prefabs/Mode3TonNan.prefab", 
			HomeModePanelType.Nima => "HomeModeBanner/Prefabs/Mode2TonNan.prefab", 
			HomeModePanelType.ButtobiGalaxy => "ToitoiGalaxy/Prefabs/ModeToitoiGalaxyBunner.prefab", 
			_ => "", 
		};
	}

	public static string ResolveYellRankImage(YellRank rank, bool isTaikyoku = false)
	{
		if (!isTaikyoku)
		{
			return $"YellCampaign/Textures/Yell{rank}.png";
		}
		return $"YellCampaign/Textures/YellRankText{rank}.png";
	}
}
