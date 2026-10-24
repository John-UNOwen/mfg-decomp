using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.GameData;
using MFG.GameRequest;
using MFG.MasterGameData;
using MFG.Sytem.Utilities;
using MFG.Taikyoku.Command;
using MFG.Types;
using MFG.Types.EventTakuMaster;
using UnityEngine;

[Serializable]
public static class GameDataManager
{
	public static ReceiveOnlyGameData ReceiveOnly = new ReceiveOnlyGameData();

	public static VersionGameData Version { get; private set; } = new VersionGameData();


	public static AccountGameData Account { get; private set; } = new AccountGameData();


	public static PlayerGameData Player { get; private set; } = new PlayerGameData();


	public static PlayerItemGameData PlayerItem { get; private set; } = new PlayerItemGameData();


	public static CustomizeGameData Customize { get; private set; } = new CustomizeGameData();


	public static StoryGameData Story { get; private set; } = new StoryGameData();


	public static MahjongGameData Mahjong { get; private set; } = new MahjongGameData();


	public static TaikyokuGameData Taikyoku { get; private set; } = new TaikyokuGameData();


	public static SystemConfigGameData System { get; private set; } = new SystemConfigGameData();


	public static CommandClient CommandClient { get; private set; } = new CommandClient();


	public static PlayerConfigGameData PlayerConfig { get; private set; } = new PlayerConfigGameData();


	public static MissionGameData Mission { get; private set; } = new MissionGameData();


	public static GachaInfoGameData GachaInfo { get; private set; } = new GachaInfoGameData();


	public static GachaGameData Gacha { get; private set; } = new GachaGameData();


	public static NoticeGameData Notice { get; private set; } = new NoticeGameData();


	public static PresentBoxGameData PresentBox { get; private set; } = new PresentBoxGameData();


	public static CardConnectGameData CardConnect { get; private set; } = new CardConnectGameData();


	public static MissionDateGameData MissionSchedule { get; private set; } = new MissionDateGameData();


	public static BingoEventGameData BingoEvent { get; private set; } = new BingoEventGameData();


	public static CompetitionGameData Competition { get; private set; } = new CompetitionGameData();


	public static PlayerRecordGameData PlayerRecord { get; private set; } = new PlayerRecordGameData();


	public static TenshiGowanEventGameData TenshiGowanEvent { get; private set; } = new TenshiGowanEventGameData();


	public static CoopGameData CoopData { get; private set; } = new CoopGameData();


	public static BingoEventRemakeGameData BingoEventRemake { get; private set; } = new BingoEventRemakeGameData();


	public static YellGameData YellCampaign { get; private set; } = new YellGameData();


	public static LogGameData Log { get; private set; } = new LogGameData();


	public static CharaStampEventGameData CharaStampEvent { get; private set; } = new CharaStampEventGameData();


	public static EventTakuGameData EventTaku { get; private set; } = new EventTakuGameData();


	public static VoteGameData Vote { get; private set; } = new VoteGameData();


	public static PlayerPaidItemGameData PlayerPaidItem { get; private set; } = new PlayerPaidItemGameData();


	public static ScrambleFestivalGameData ScrambleFestival { get; private set; } = new ScrambleFestivalGameData();


	public static FriendGameData Friend { get; private set; } = new FriendGameData();


	public static MusicGachaGameData MusicGacha { get; private set; } = new MusicGachaGameData();


	public static OthersEventGameData OthersEvent { get; private set; } = new OthersEventGameData();


	public static ShopCompetitionGameData ShopCompetition { get; private set; } = new ShopCompetitionGameData();


	public static async UniTask<(bool isLoadSuccess, TextManager.KEY errorMsgType)> ExecLoadAtCardEntry(CancellationToken cancellationToken = default(CancellationToken))
	{
		bool isLoadSuccess = true;
		TextManager.KEY errorMsgType = TextManager.KEY.ENTRY_NETWORK_ERROR;
		string dataID = Account.DataID;
		try
		{
			if (Account.IsGuestUser)
			{
				Account.IsFirstPlay = true;
				await RequestApiBase<CreatePlayer, CreatePlayer.ResultData, (string, string)>.Send(("PLAYER", dataID), enableErrorScreenTransition: false, enableSendLog: true, cancellationToken);
			}
			await System.GameControlInfo.Modify(isForceModify: true, suppressException: false);
			await PaseliUtility.RefleshPrice(Fraction.One, isFetchExternalBasicRateInfo: false);
			Login.ResultData resultData = await RequestApiBase<Login, Login.ResultData>.Send(enableErrorScreenTransition: false, enableSendLog: true, cancellationToken);
			Account.AppSessionID = resultData.SessionID;
			GetMenuData.ResultData resultData2 = await RequestApiBase<GetMenuData, GetMenuData.ResultData>.Send(enableErrorScreenTransition: false, enableSendLog: true, cancellationToken);
			Account.PlayerID = resultData2.PlayerData.PlayerID;
			Player.Apply(resultData2);
			if (!Account.IsGuestUser)
			{
				ClientStateRead.ResultData resultData3 = await RequestApiBase<ClientStateRead, ClientStateRead.ResultData, ClientStateRead.Param>.Send(new ClientStateRead.Param
				{
					OneKindType = "version_game"
				}, enableErrorScreenTransition: false, enableSendLog: true, cancellationToken);
				if (resultData3.Version != null && VersionGameData.IsNewVersion(resultData3.Version.SaveDataVersion))
				{
					return (false, TextManager.KEY.ENTRY_SAVEDATA_VERSION_ERROR);
				}
			}
			await MissionSchedule.ControlInfo.Modify(isForceModify: true, suppressException: false);
			GachaInfoRequest.ResultData resultData4 = await RequestApiBase<GachaInfoRequest, GachaInfoRequest.ResultData>.Send(enableErrorScreenTransition: true, enableSendLog: true, cancellationToken);
			GachaInfo.GachaSeriesInfosFromServer = resultData4.ResultGachaSeriesInfosFromServer;
			Apply(await RequestApiBase<ClientStateRead, ClientStateRead.ResultData>.Send(enableErrorScreenTransition: false, enableSendLog: true, cancellationToken));
			Gacha.Validate();
			GetRecord.ResultData resultData5 = await RequestApiBase<GetRecord, GetRecord.ResultData>.Send(enableErrorScreenTransition: false, enableSendLog: true, cancellationToken);
			PlayerRecord.Apply(resultData5);
			await CoopData.GrantItem();
			if (GameUtility.IsKonastaOrNetcafeBehavior)
			{
				Account.IsForceFreePlayUser |= Player.IsPro() || GameUtility.IsProCardId(Account.CardID);
			}
		}
		catch (Exception ex)
		{
			errorMsgType = ((!(ex is NullReferenceException)) ? TextManager.KEY.ENTRY_NETWORK_ERROR : TextManager.KEY.ENTRY_SAVEDATA_VERSION_ERROR);
			Debug.Log($"ExecLoadAtCardEntry exception:{ex}");
			isLoadSuccess = false;
		}
		return (isLoadSuccess, errorMsgType);
	}

	public static async UniTask ExecLoadAtContinueEvaluation(CancellationToken cancellationToken = default(CancellationToken))
	{
		await System.GameControlInfo.Modify(isForceModify: true, suppressException: false);
	}

	public static async UniTask ExecLoadAtContinue(CancellationToken cancellationToken = default(CancellationToken))
	{
		try
		{
			await MissionSchedule.ControlInfo.Modify(isForceModify: true, suppressException: false);
			Mission.MissionUpdate();
			Mission.ContinueProcess();
		}
		catch
		{
		}
		try
		{
			GachaInfoRequest.ResultData resultData = await RequestApiBase<GachaInfoRequest, GachaInfoRequest.ResultData>.Send(enableErrorScreenTransition: false, enableSendLog: true, cancellationToken);
			GachaInfo.GachaSeriesInfosFromServer = resultData.ResultGachaSeriesInfosFromServer;
			Gacha.Validate();
		}
		catch
		{
		}
		try
		{
			Apply(await RequestApiBase<ClientStateRead, ClientStateRead.ResultData, ClientStateRead.Param>.Send(new ClientStateRead.Param
			{
				OneKindType = "ea_shop"
			}, enableErrorScreenTransition: false));
		}
		catch
		{
		}
		try
		{
			Apply(await RequestApiBase<ClientStateRead, ClientStateRead.ResultData, ClientStateRead.Param>.Send(new ClientStateRead.Param
			{
				OneKindType = "competition_game"
			}, enableErrorScreenTransition: false));
		}
		catch
		{
		}
		try
		{
			Apply(await RequestApiBase<ClientStateRead, ClientStateRead.ResultData, ClientStateRead.Param>.Send(new ClientStateRead.Param
			{
				OneKindType = "present_box_game"
			}, enableErrorScreenTransition: false));
		}
		catch
		{
		}
		try
		{
			Apply(await RequestApiBase<ClientStateRead, ClientStateRead.ResultData, ClientStateRead.Param>.Send(new ClientStateRead.Param
			{
				OneKindType = "receive_only"
			}, enableErrorScreenTransition: false));
		}
		catch
		{
		}
		try
		{
			Apply(await RequestApiBase<ClientStateRead, ClientStateRead.ResultData, ClientStateRead.Param>.Send(new ClientStateRead.Param
			{
				OneKindType = "friend_game"
			}, enableErrorScreenTransition: false));
		}
		catch
		{
		}
		try
		{
			Apply(await RequestApiBase<ClientStateRead, ClientStateRead.ResultData, ClientStateRead.Param>.Send(new ClientStateRead.Param
			{
				OneKindType = "others_event"
			}, enableErrorScreenTransition: false));
		}
		catch
		{
		}
		try
		{
			Apply(await RequestApiBase<ClientStateRead, ClientStateRead.ResultData, ClientStateRead.Param>.Send(new ClientStateRead.Param
			{
				OneKindType = "shop_competition_game"
			}, enableErrorScreenTransition: false));
		}
		catch
		{
		}
		await ExecLoadAtGacha(cancellationToken);
	}

	public static async UniTask ExecLoadAtGacha(CancellationToken cancellationToken = default(CancellationToken))
	{
		try
		{
			Apply(await RequestApiBase<ClientStateRead, ClientStateRead.ResultData, ClientStateRead.Param>.Send(new ClientStateRead.Param
			{
				OneKindType = "music_gacha"
			}, enableErrorScreenTransition: false));
		}
		catch
		{
		}
	}

	public static async UniTask<(bool isLoadSuccess, TextManager.KEY errorMsgType)> PlayerRecordGet(CancellationToken cancellationToken = default(CancellationToken))
	{
		bool isLoadSuccess = true;
		TextManager.KEY errorMsgType = TextManager.KEY.ENTRY_NETWORK_ERROR;
		try
		{
			GetRecord.ResultData resultData = await RequestApiBase<GetRecord, GetRecord.ResultData>.Send(enableErrorScreenTransition: false, enableSendLog: true, cancellationToken);
			PlayerRecord.Apply(resultData);
		}
		catch (Exception ex)
		{
			errorMsgType = ((!(ex is NullReferenceException)) ? TextManager.KEY.ENTRY_NETWORK_ERROR : TextManager.KEY.ENTRY_SAVEDATA_VERSION_ERROR);
			Debug.Log($"ExecLoadAtCardEntry exception:{ex}");
			isLoadSuccess = false;
		}
		return (isLoadSuccess, errorMsgType);
	}

	public static void Apply(ClientStateRead.ResultData result)
	{
		if (result.Version != null)
		{
			Version = result.Version;
		}
		if (result.ReceiveOnly != null)
		{
			ReceiveOnly = result.ReceiveOnly;
		}
		if (result.PlayerConfig != null)
		{
			PlayerConfig = result.PlayerConfig;
		}
		if (result.Player != null)
		{
			Player = result.Player;
			Player.UpdateCostume();
		}
		if (result.Customize != null)
		{
			Customize = result.Customize;
			Customize.UpdateCostume();
			Customize.UpdateMatchingBG();
		}
		if (result.PlayerItem != null)
		{
			PlayerItem = result.PlayerItem;
		}
		if (result.PlayerPaidItem != null)
		{
			PlayerPaidItem = result.PlayerPaidItem;
		}
		if (result.Story != null)
		{
			Story = result.Story;
		}
		if (result.Notice != null)
		{
			Notice = result.Notice;
		}
		if (result.PresentBox != null)
		{
			PresentBox = result.PresentBox;
		}
		if (result.Mission != null)
		{
			Mission = result.Mission;
			Mission.MissionUpdate();
		}
		if (result.Gacha != null)
		{
			Gacha = result.Gacha;
			Gacha.PlayerConfigUpdate();
		}
		if (result.CardConnect != null)
		{
			CardConnect = result.CardConnect;
		}
		if (result.BingoEvent != null)
		{
			BingoEvent = result.BingoEvent;
		}
		if (result.Competition != null)
		{
			Competition = result.Competition;
			Player.RefleshCompetitionTrophy(Competition.GetLatestCompetitionInfo(), Competition.GetKAC2023CompetitionInfos());
		}
		if (result.CoopGameData != null)
		{
			CoopData = result.CoopGameData;
		}
		if (result.Log != null)
		{
			Log = result.Log;
		}
		if (result.CharaStampEvent != null)
		{
			CharaStampEvent = result.CharaStampEvent;
			CharaStampEvent.InfoUpdate();
		}
		if (result.EventTaku != null)
		{
			EventTaku = result.EventTaku;
		}
		if (result.Vote != null)
		{
			Vote = result.Vote;
			Vote.VoteUpdate();
		}
		if (result.ScrambleFestival != null)
		{
			ScrambleFestival = result.ScrambleFestival;
			ScrambleFestival.CatchphraseUpdate();
		}
		if (result.Friend != null)
		{
			Friend = result.Friend;
		}
		if (result.MusicGacha != null)
		{
			MusicGacha = result.MusicGacha;
		}
		if (result.BingoEventRemake != null)
		{
			BingoEventRemake = result.BingoEventRemake;
		}
		if (result.OthersEvent != null)
		{
			OthersEvent = result.OthersEvent;
		}
		if (result.ShopCompetition != null)
		{
			ShopCompetition = result.ShopCompetition;
		}
	}

	public static void Reset()
	{
		Debug.Log("GameData.Reset");
		Version = new VersionGameData();
		ReceiveOnly = new ReceiveOnlyGameData();
		Player = new PlayerGameData();
		PlayerItem = new PlayerItemGameData();
		PlayerPaidItem = new PlayerPaidItemGameData();
		Customize = new CustomizeGameData();
		Story = new StoryGameData();
		Account = new AccountGameData();
		Mahjong = new MahjongGameData();
		Taikyoku = new TaikyokuGameData();
		PlayerConfig = new PlayerConfigGameData();
		CommandClient = new CommandClient();
		Mission = new MissionGameData();
		Gacha = new GachaGameData();
		Notice = new NoticeGameData();
		PresentBox = new PresentBoxGameData();
		CardConnect = new CardConnectGameData();
		BingoEvent = new BingoEventGameData();
		Competition = new CompetitionGameData();
		TenshiGowanEvent = new TenshiGowanEventGameData();
		CoopData = new CoopGameData();
		BingoEventRemake = new BingoEventRemakeGameData();
		YellCampaign = new YellGameData();
		Log = new LogGameData();
		CharaStampEvent = new CharaStampEventGameData();
		EventTaku = new EventTakuGameData();
		Vote = new VoteGameData();
		ScrambleFestival = new ScrambleFestivalGameData();
		Friend = new FriendGameData();
		MusicGacha = new MusicGachaGameData();
		OthersEvent = new OthersEventGameData();
		ShopCompetition = new ShopCompetitionGameData();
	}

	public static void ResetForContinueStart()
	{
		Account.ResetForContinueStart();
		Mahjong.ResetForContinueStart();
		TaikyokuReset();
		PlayerConfig.ResetForContinueStart();
		EventTaku.ResetForContinueStart();
		Mahjong.AotenjoScore = 0;
		Story.SetupForceStory();
	}

	public static void ResetForDebugMenu()
	{
		Version = new VersionGameData();
		ReceiveOnly = new ReceiveOnlyGameData();
		PlayerGameData.LoadDataVersion = 14;
		Player = new PlayerGameData
		{
			DataVersion = 14
		};
		PlayerItem = new PlayerItemGameData();
		PlayerPaidItem = new PlayerPaidItemGameData();
		Customize = new CustomizeGameData();
		Story = new StoryGameData();
		PlayerConfig = new PlayerConfigGameData();
		Mission = new MissionGameData();
		Gacha = new GachaGameData();
		CardConnect = new CardConnectGameData();
		BingoEvent = new BingoEventGameData();
		Competition = new CompetitionGameData();
		TenshiGowanEvent = new TenshiGowanEventGameData();
		BingoEventRemake = new BingoEventRemakeGameData();
		YellCampaign = new YellGameData();
		Log = new LogGameData();
		CharaStampEvent = new CharaStampEventGameData();
		EventTaku = new EventTakuGameData();
		Vote = new VoteGameData();
		ScrambleFestival = new ScrambleFestivalGameData();
		MusicGacha = new MusicGachaGameData();
		OthersEvent = new OthersEventGameData();
		ShopCompetition = new ShopCompetitionGameData();
	}

	public static void TaikyokuReset()
	{
		Debug.Log("GameData.TaikyokuReset");
		Taikyoku = new TaikyokuGameData();
		CommandClient = new CommandClient();
	}

	public static bool IsProStay()
	{
		return System.GameControlInfo.ProStats.now_pro_stats;
	}

	public static bool IsProEventTakuStay()
	{
		return System.GameControlInfo.ProStats.AnyEventTakuStaying();
	}

	public static bool IsProNormalTakuStay()
	{
		return System.GameControlInfo.ProStats.AnyNormalTakuStaying();
	}

	public static bool IsProStay(TakuType takuType, MFG.Types.EventType eventType)
	{
		return System.GameControlInfo.ProStats.AnyTakuStaying(takuType, eventType);
	}

	public static bool IsBingoEventEnable()
	{
		return System.GameControlInfo.EventStatus.IsActive(GameEventType.YakuBingo);
	}

	public static DateTimeOffset GetBingoEventEndTime()
	{
		return System.GameControlInfo.EventStatus.GetGameEventData(GameEventType.YakuBingo)?.End ?? DateTimeOffset.MinValue;
	}

	public static bool IsTenshiGowanEventEnable()
	{
		return System.GameControlInfo.EventStatus.IsActive(GameEventType.TenshiGowan);
	}

	public static bool FanBonusEventEnable()
	{
		if (Account.IsPremiumMode())
		{
			return true;
		}
		return System.GameControlInfo.EventStatus.IsActive(GameEventType.FanBonusEvent);
	}

	public static float GetFanBonusRatio()
	{
		if (Account.IsPremiumMode())
		{
			return 2f;
		}
		return System.GameControlInfo.EventStatus.GetGameEventData(GameEventType.FanBonusEvent)?.ParamValue("FanRatio", 0f) ?? 1f;
	}

	public static bool FavoBonusEventEnable()
	{
		if (Account.IsPremiumMode())
		{
			return true;
		}
		return System.GameControlInfo.EventStatus.IsActive(GameEventType.FavoBonusEvent);
	}

	public static float GetFavoBonusRatio()
	{
		if (Account.IsPremiumMode())
		{
			return 2f;
		}
		return System.GameControlInfo.EventStatus.GetGameEventData(GameEventType.FavoBonusEvent)?.ParamValue("FavoRatio", 0f) ?? 1f;
	}

	public static bool IsPineCharaEnable()
	{
		return System.GameControlInfo.EventStatus.IsActive(GameEventType.PineAppearance);
	}

	public static bool IsShioriCharaEnable()
	{
		return System.GameControlInfo.EventStatus.IsActive(GameEventType.ShioriAppearance);
	}

	public static bool IsShioriStoryEnable()
	{
		if (IsShioriCharaEnable())
		{
			return System.GameControlInfo.EventStatus.IsActive(GameEventType.StoryAdditionShiori);
		}
		return false;
	}

	public static bool IsShioriKDECostumeEnable()
	{
		return System.GameControlInfo.EventStatus.IsActive(GameEventType.CostumeAddition12);
	}

	public static bool IsChaosCharaEnable()
	{
		return System.GameControlInfo.EventStatus.IsActive(GameEventType.ChaosAppearance);
	}

	public static bool IsClearProEventEnable()
	{
		return System.GameControlInfo.EventStatus.IsActive(GameEventType.ClearProCpuEvent);
	}

	public static bool IsClearCharaEnable()
	{
		return System.GameControlInfo.EventStatus.IsActive(GameEventType.ClearAppearance);
	}

	public static bool IsIyoProEventEnable()
	{
		return System.GameControlInfo.EventStatus.IsActive(GameEventType.IyoProCpuEvent);
	}

	public static bool IsIyoCharaEnable()
	{
		return System.GameControlInfo.EventStatus.IsActive(GameEventType.IyoAppearance);
	}

	public static bool IsGrimAroeCpuForceEnable()
	{
		return System.GameControlInfo.EventStatus.IsActive(GameEventType.GrimAroeEntryEvent);
	}

	public static bool IsGrimAroeCharaEnable()
	{
		return System.GameControlInfo.EventStatus.IsActive(GameEventType.GrimAroeAppearance);
	}

	public static bool IsCocoaCpuForceEnable()
	{
		return System.GameControlInfo.EventStatus.IsActive(GameEventType.CocoaEntryEvent);
	}

	public static bool IsCocoaCharaEnable()
	{
		return System.GameControlInfo.EventStatus.IsActive(GameEventType.CocoaAppearance);
	}

	public static bool IsDiaCpuForceEnable()
	{
		return System.GameControlInfo.EventStatus.IsActive(GameEventType.DiaEntryEvent);
	}

	public static bool IsDiaCharaEnable()
	{
		return System.GameControlInfo.EventStatus.IsActive(GameEventType.DiaAppearance);
	}

	public static bool IsDoubrielCpuForceEnable()
	{
		return System.GameControlInfo.EventStatus.IsActive(GameEventType.DoubrielEntryEvent);
	}

	public static bool IsDoubrielCharaEnable()
	{
		return System.GameControlInfo.EventStatus.IsActive(GameEventType.DoubrielAppearance);
	}

	public static bool IsIppatsuCpuForceEnable()
	{
		return System.GameControlInfo.EventStatus.IsActive(GameEventType.IppatsuEntryEvent);
	}

	public static bool IsIppatsuCharaEnable()
	{
		return System.GameControlInfo.EventStatus.IsActive(GameEventType.IppatsuAppearance);
	}

	public static bool IsShiroeCpuForceEnable()
	{
		return System.GameControlInfo.EventStatus.IsActive(GameEventType.ShiroeEntryEvent);
	}

	public static bool IsShiroeCharaEnable()
	{
		return System.GameControlInfo.EventStatus.IsActive(GameEventType.ShiroeAppearance);
	}

	public static bool IsJunCpuForceEnable()
	{
		return System.GameControlInfo.EventStatus.IsActive(GameEventType.JunEntryEvent);
	}

	public static bool IsJunCharaEnable()
	{
		return System.GameControlInfo.EventStatus.IsActive(GameEventType.JunAppearance);
	}

	public static bool IsToukaCpuForceEnable()
	{
		return System.GameControlInfo.EventStatus.IsActive(GameEventType.ToukaEntryEvent);
	}

	public static bool IsToukaCharaEnable()
	{
		return System.GameControlInfo.EventStatus.IsActive(GameEventType.ToukaAppearance);
	}

	public static bool IsReachSongToytoyNoticeEnable()
	{
		return System.GameControlInfo.EventStatus.IsActive(GameEventType.ReachSongGachaNotice6);
	}

	public static bool IsCharaEnable(CharaType charaType)
	{
		switch (charaType)
		{
		case CharaType.Chara01:
		case CharaType.Chara02:
		case CharaType.Chara03:
		case CharaType.Chara04:
		case CharaType.Chara05:
		case CharaType.Chara06:
		case CharaType.Chara07:
		case CharaType.Chara08:
			return true;
		case CharaType.Chara09:
			return IsPineCharaEnable();
		case CharaType.Chara10:
			return IsShioriCharaEnable();
		case CharaType.Chara11:
			return IsChaosCharaEnable();
		case CharaType.Chara12:
			return IsClearCharaEnable();
		case CharaType.Chara13:
			return IsIyoCharaEnable();
		case CharaType.Chara14:
			return IsGrimAroeCharaEnable();
		case CharaType.Chara15:
			return IsCocoaCharaEnable();
		case CharaType.Chara16:
			return IsDiaCharaEnable();
		case CharaType.Chara17:
			return IsDoubrielCharaEnable();
		case CharaType.Chara18:
			return IsIppatsuCharaEnable();
		case CharaType.Chara19:
			return IsShiroeCharaEnable();
		case CharaType.Chara20:
			return IsJunCharaEnable();
		case CharaType.Chara21:
			return IsToukaCharaEnable();
		default:
			return false;
		}
	}

	public static bool IsCharaSelectable(CharaType charaType)
	{
		if (!IsCharaEnable(charaType))
		{
			return false;
		}
		switch (charaType)
		{
		case CharaType.Chara01:
		case CharaType.Chara02:
		case CharaType.Chara03:
		case CharaType.Chara04:
		case CharaType.Chara05:
		case CharaType.Chara06:
		case CharaType.Chara07:
		case CharaType.Chara08:
		case CharaType.Chara09:
			return true;
		case CharaType.Chara10:
			return CoopData.IsCharacterSelectable(charaType);
		case CharaType.Chara11:
			return CharaStampEvent.IsCharacterEnable(charaType);
		case CharaType.Chara12:
		case CharaType.Chara13:
		case CharaType.Chara14:
		case CharaType.Chara15:
		case CharaType.Chara16:
		case CharaType.Chara17:
		case CharaType.Chara18:
		case CharaType.Chara19:
		case CharaType.Chara20:
		case CharaType.Chara21:
			return Player.GachaCharaUsableList.Contains(charaType);
		default:
			return false;
		}
	}

	public static List<CharaType> GetCharaSelectableList()
	{
		List<CharaType> list = new List<CharaType>();
		foreach (CharaType value in Enum.GetValues(typeof(CharaType)))
		{
			if (IsCharaSelectable(value))
			{
				list.Add(value);
			}
		}
		return list;
	}

	public static bool IsZoudaiAppear()
	{
		return System.GameControlInfo.EventStatus.IsActive(GameEventType.ZoudaiAppearance);
	}

	public static bool IsDaiuchiGuideAppear()
	{
		return System.GameControlInfo.EventStatus.IsActive(GameEventType.DaiuchiGuideAppearance);
	}

	public static bool IsAdJandriAppear()
	{
		return System.GameControlInfo.EventStatus.IsActive(GameEventType.AdJandriAppearance);
	}

	public static bool IsNewsClearInfoAppear()
	{
		return System.GameControlInfo.EventStatus.IsActive(GameEventType.NewsClearInfo);
	}

	public static bool IsAdCustomizeRandomConfigAppear()
	{
		return System.GameControlInfo.EventStatus.IsActive(GameEventType.AdCustomizeRandomConfig);
	}

	public static bool IsAdMatchingBGAppear()
	{
		return System.GameControlInfo.EventStatus.IsActive(GameEventType.AdMatchingBG);
	}

	public static bool IsAdMatchingBG2Appear()
	{
		return System.GameControlInfo.EventStatus.IsActive(GameEventType.AdMatchingBG2);
	}

	public static bool IsMAAddition1Appear()
	{
		return System.GameControlInfo.EventStatus.IsActive(GameEventType.MAAddition1);
	}

	public static bool IsLoadJandriAppear()
	{
		return System.GameControlInfo.EventStatus.IsActive(GameEventType.LoadJandriAppearance);
	}

	public static bool IsCinderellaMitsubaAppear()
	{
		return System.GameControlInfo.EventStatus.IsActive(GameEventType.CinderellaMitsubaAppearance);
	}

	public static bool IsCostumeHalfAnniversaryAppear()
	{
		return System.GameControlInfo.EventStatus.IsActive(GameEventType.CostumeHalfAnniversaryAppearance);
	}

	public static bool IsCostumeMizugiClearAppear()
	{
		return System.GameControlInfo.EventStatus.IsActive(GameEventType.CostumeMizugiClearAppearance);
	}

	public static bool IsScrambleMatchAppear()
	{
		return System.GameControlInfo.EventStatus.IsActive(GameEventType.AdScrambleMatchAppearance);
	}

	public static bool IsCutinPresent001Appear()
	{
		return System.GameControlInfo.EventStatus.IsActive(GameEventType.CutinPresent001);
	}

	public static bool IsCutinPresent002Appear()
	{
		return System.GameControlInfo.EventStatus.IsActive(GameEventType.CutinPresent002);
	}

	public static bool IsChaosForceActive()
	{
		return System.GameControlInfo.EventStatus.IsActive(GameEventType.ChaosUsable);
	}

	public static bool IsPremiumStartEnable()
	{
		return System.GameControlInfo.EventStatus.IsActive(GameEventType.PremiumStartEnable);
	}

	public static bool IsReachSongVoiceGachaEnable()
	{
		if (System.GameControlInfo.EventStatus.IsActive(GameEventType.ReachSongVoiceGacha))
		{
			return PaseliUtility.PPEX.IsEnablePPEX;
		}
		return false;
	}

	public static bool AnyConstancyEventTakuScheduleDisp()
	{
		return EventTakuMaster.Items.Any((Item x) => x.IsDispConstancySchedule());
	}

	public static bool SpiritGymBonusEventEnable()
	{
		if (CoopData.IsJanDriEnable(isRealTime: true))
		{
			return true;
		}
		return System.GameControlInfo.EventStatus.IsActive(GameEventType.SpiritGymBonusEvent);
	}

	public static int GetSpiritGymBonusRatio()
	{
		if (CoopData.IsJanDriEnable(isRealTime: true))
		{
			return 3;
		}
		string text = System.GameControlInfo.EventStatus.GetGameEventData(GameEventType.SpiritGymBonusEvent)?.ParamValue<string>("OID") ?? "";
		if (text == "OID_DOJO_BONUS_2X")
		{
			return 2;
		}
		if (text == "OID_DOJO_BONUS_3X")
		{
			return 3;
		}
		return 1;
	}

	public static bool IsPrivateMatchDisplay()
	{
		return System.GameControlInfo.EventStatus.IsActive(GameEventType.PrivateMatchingDisplay);
	}

	public static bool IsPrivateMatchEnable()
	{
		if (IsPrivateMatchDisplay())
		{
			return System.GameControlInfo.EventStatus.IsActive(GameEventType.PrivateMatchingEnable);
		}
		return false;
	}

	public static bool IsRevengeContinueEnable()
	{
		if (IsPrivateMatchDisplay())
		{
			return System.GameControlInfo.EventStatus.IsActive(GameEventType.RevengeContinueEnable);
		}
		return false;
	}

	public static bool IsOdekakeEnable()
	{
		return System.GameControlInfo.EventStatus.IsActive(GameEventType.EnableOdekake);
	}

	public static bool IsTenshiTenhoDemoEnable()
	{
		return System.GameControlInfo.EventStatus.IsActive(GameEventType.TenshiTenho);
	}

	public static bool IsItemGainLogEnable()
	{
		return System.GameControlInfo.EventStatus.IsActive(GameEventType.ItemGainLogEnable);
	}

	public static bool IsQRLoginEnable()
	{
		return System.GameControlInfo.EventStatus.IsActive(GameEventType.QRLogin);
	}

	public static bool IsPonnomichiCollabEnable()
	{
		if (!GameUtility.IsKonastaOrNetcafeBehavior)
		{
			return System.GameControlInfo.EventStatus.IsActive(GameEventType.Ponnomichi);
		}
		return false;
	}

	public static int PopularityVoteAdjustRankBonus(TakuType takuType, Rank rank)
	{
		try
		{
			if (System.GameControlInfo.EventStatus.IsActive(GameEventType.PopularityVoteAdjust))
			{
				GameEventData gameEventData = System.GameControlInfo.EventStatus.GetGameEventData(GameEventType.PopularityVoteAdjust);
				if (gameEventData != null)
				{
					string text = gameEventData.ParamValue(takuType.ToString(), "");
					if (!string.IsNullOrEmpty(text))
					{
						return int.Parse(text.Split('/')[(int)rank]);
					}
				}
			}
		}
		catch
		{
			Debug.Log("GetRankBonus PopularityVoteAdjust Empty");
		}
		return -1;
	}
}
