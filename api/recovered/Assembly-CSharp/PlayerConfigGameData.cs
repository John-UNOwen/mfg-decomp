using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.GameData;
using MFG.GameRequest;
using MFG.Types;
using MFG.Types.FightGirlNews;
using MFG.Types.HaiConjecture;
using MFG.Types.OneShotNotice;
using MFG.Types.OnlyPeraichiNotice;
using MFG.Types.ShopCompetition;
using UnityEngine;

[Serializable]
public class PlayerConfigGameData : GameDataBase
{
	private const int CurrentDataVersion = 1;

	public int DataVersion = 1;

	public const string ClientStateName = "player_config";

	public List<bool> QuestionDatas = new List<bool>();

	public bool IsNameDecided;

	public bool IsTaikyokuSupport = true;

	public ViewType ViewType;

	public bool IsAutoAgariEnable;

	public bool IsDispHelpOption;

	public bool IsDispHelpSyanten;

	public bool IsDispHelpAnpai;

	public bool IsDispHelpNavi;

	public bool IsDispHelpAfterGachaAtHomeScene;

	public bool IsDispHelpFirstPlayAtHomeScene;

	public bool IsDispHelpFirstPlayMyRoomScene;

	public bool IsDispHelpFirstPlayResultScene;

	public bool IsDispHelpFirstPlayResultFightStar;

	public bool IsDispHelpFirstRateWindowDisp;

	public bool IsDispHelpResultFanLimit;

	public bool IsDispHelpResultFavoLimit;

	public bool IsDispHelpFirstPlayDojo;

	public bool IsBlowAwayEventFirstExplanationDisplayed;

	public bool IsClearAppearanceEventFirstExplanationDisplayed;

	public List<NewsID> FightGirlNewsFinishIDs = new List<NewsID>();

	public List<MFG.Types.OneShotNotice.NoticeID> OneShotNoticeFinishIDs = new List<MFG.Types.OneShotNotice.NoticeID>();

	public List<MFG.Types.OnlyPeraichiNotice.NoticeID> OnlyPeraichiNoticeFinishIDs = new List<MFG.Types.OnlyPeraichiNotice.NoticeID>();

	public List<CompetitionID> ShopCompetitionCheckedIDs = new List<CompetitionID>();

	public bool IsZoudaiAppearFinish;

	public bool IsCinderellaMitsubaFinish;

	public bool IsCostumeHalfAnniversaryFinish;

	public bool IsCostumeMizugiClearFinish;

	public bool IsDaiuchiGuideFinish;

	public bool IsAdJandriFinish;

	public bool IsAdScrambleMatchFinish;

	public bool IsCutinPresent001Finish;

	public bool IsCutinPresent002Finish;

	public bool IsChara12GetNoticeFinish;

	public bool IsChara13GetNoticeFinish;

	public bool IsChara14GetNoticeFinish;

	public bool IsChara15GetNoticeFinish;

	public bool IsChara16GetNoticeFinish;

	public bool IsChara17GetNoticeFinish;

	public bool IsChara18GetNoticeFinish;

	public bool IsChara19GetNoticeFinish;

	public bool IsChara20GetNoticeFinish;

	public bool IsChara21GetNoticeFinish;

	public bool IsAdClearInfoFinish;

	public bool IsAdCustomizeRandomConfigFinish;

	public bool IsAdMatchingBGFinish;

	public bool IsAdMatchingBG2Finish;

	public bool IsAdMatchingBG3Finish;

	public bool IsAdMAAddition1Finish;

	public bool IsBingoEventFirst = true;

	public bool IsTenshiGowanEventFirst = true;

	public bool IsLongAutumnNightEventFirst = true;

	public bool IsHollyNightEventFirst = true;

	public bool IsNewYear2024EventFirst = true;

	public bool IsFirstStormOfSpringEventFirst = true;

	public bool IsAnniversaryBonus2024EventFirst = true;

	public bool IsBemani001EventFirst = true;

	public bool IsCompetition5EventFirst = true;

	public bool IsWelcomeComeback1EventFirst = true;

	public bool IsWelcomeComeback2EventFirst = true;

	public bool IsRainySeaconEventFirst = true;

	public bool IsSummerSunshineEventFirst = true;

	public bool IsToho001EventFirst = true;

	public bool IsToho001LoginEventFirst = true;

	public bool IsPlayVoiceRankUpDan;

	public bool IsPlayVoiceRankUpJansei;

	[NonSerialized]
	public bool IsCutinGachaSceneVisited;

	[NonSerialized]
	public bool IsSpGachaSceneVisited;

	[NonSerialized]
	public bool IsMusicGachaSceneVisited;

	[NonSerialized]
	public bool IsDojoSceneVisited;

	[NonSerialized]
	public bool IsMyRoomSceneVisited;

	[NonSerialized]
	public bool IsStoryRead;

	[NonSerialized]
	public bool IsContinued;

	[NonSerialized]
	public bool IsEventTakuSelected;

	[NonSerialized]
	public MFG.Types.EventType LatestSelectedEventType;

	[NonSerialized]
	public int RevengeContinueValue;

	[NonSerialized]
	public bool IsGachaSelectIn;

	[NonSerialized]
	public int GachaSelectRemainTime;

	public int HotenFightStar;

	[NonSerialized]
	public bool IsDataSaveFailed;

	public ConjectureType ConjectureType = ConjectureType.OnlyReach;

	public float headPhoneVolume = 101f / 198f;

	public bool IsUKSLEDAnimAllow = true;

	public List<int> SubMonitorInHome = new List<int> { 0, 0, 0 };

	public List<int> SubMonitorInMahjong = new List<int> { 0, 0, 0 };

	public bool IsProAnimation = true;

	public bool IsStickerAllDisp = true;

	public bool IsCursorAnimAllow = true;

	public bool IsMemorialPrintNotice = true;

	public bool IsKeyboardPlayInStreaming = true;

	public bool IsKeyboardGuideInStreaming = true;

	public string PrivateMatchPassCode = "";

	[NonSerialized]
	public bool IsPrivateMatch;

	public List<int> PrivateMatchNoticeFinishIndexes = new List<int>();

	public UnableCharaGachaSelectWindow.State UnableCharaGachaState;

	public bool IsTokushohoAgreed;

	public bool IsGachaSceneVisited
	{
		get
		{
			if (IsCutinGachaSceneVisited && IsSpGachaSceneVisited)
			{
				if (!IsMusicGachaSceneVisited)
				{
					if (PaseliUtility.PPEX.IsEnablePurchaseItem || GameUtility.IsKonastaOrNetcafeBehavior)
					{
						List<GachaSeriesServerInfo> list = GameDataManager.GachaInfo.ModeGachaInfo(GachaSeriesType.Music);
						return list == null || list.Count() <= 0;
					}
					return true;
				}
				return true;
			}
			return false;
		}
	}

	public bool IsKeyboardPlay()
	{
		if (DebugFlag.IsActive(DebugFlag.Flag.KeyboardEnable) || GameUtility.IsCloudStreaming)
		{
			return GameDataManager.PlayerConfig.IsKeyboardPlayInStreaming;
		}
		return false;
	}

	public bool IsKeyboardGuideDisplay()
	{
		if (DebugFlag.IsActive(DebugFlag.Flag.KeyboardEnable) || GameUtility.IsCloudStreaming)
		{
			return GameDataManager.PlayerConfig.IsKeyboardGuideInStreaming;
		}
		return false;
	}

	public async UniTask Save(CancellationToken cancellationToken = default(CancellationToken))
	{
		string text = JsonUtility.ToJson(this);
		Debug.Log("PlayerConfigGameData Save : " + text);
		await RequestApiBase<ClientStateWrite, ClientStateWrite.ResultData, (string, string)>.Send(("player_config", text), enableErrorScreenTransition: false, enableSendLog: true, cancellationToken);
	}

	public static PlayerConfigGameData LoadFromJson(string jsonString)
	{
		Debug.Log("PlayerConfigGameData Load : " + jsonString);
		PlayerConfigGameData playerConfigGameData = JsonUtility.FromJson<PlayerConfigGameData>(jsonString);
		if (playerConfigGameData == null)
		{
			playerConfigGameData = new PlayerConfigGameData();
		}
		playerConfigGameData.Validate();
		return playerConfigGameData;
	}

	private void Validate()
	{
		ViewType = (ViewType)Math.Clamp((int)ViewType, 0, Enum.GetValues(typeof(ViewType)).Length - 1);
	}

	public void ResetForContinueStart()
	{
		IsCutinGachaSceneVisited = false;
		IsSpGachaSceneVisited = false;
		IsMusicGachaSceneVisited = false;
		IsDojoSceneVisited = false;
		IsMyRoomSceneVisited = false;
		IsStoryRead = false;
		IsContinued = true;
	}
}
