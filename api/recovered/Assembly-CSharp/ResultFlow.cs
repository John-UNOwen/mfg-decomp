using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Cysharp.Threading.Tasks;
using Konami.System.Net;
using MFG.Assets;
using MFG.GameData;
using MFG.GameData.EventTaku;
using MFG.GameRequest;
using MFG.Sytem.Utilities;
using MFG.Types;
using MFG.Types.CharaStamp;
using MFG.Types.Mission;
using TestModeSkeleton;
using TestModeSkeleton.CabinetDef;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class ResultFlow : BaseFlow
{
	public class LevelCalc
	{
		public int m_level = 1;

		public int m_exp;

		public int m_maxLevel = 1;

		public int m_dispExp;

		public int m_dispExpBefore;

		private Func<int, int> _GetNextExp;

		public LevelCalc(Func<int, int> _func)
		{
			_GetNextExp = _func;
		}

		public int GetNextExp()
		{
			if (_GetNextExp == null)
			{
				return 1;
			}
			return _GetNextExp(m_level);
		}

		public void AddExp(int exp)
		{
			GetNextExp();
			m_exp += exp;
			if (m_exp >= GetNextExp())
			{
				m_exp -= GetNextExp();
				m_level++;
			}
			else if (m_exp < 0)
			{
				m_level--;
				m_exp += GetNextExp();
			}
		}

		public bool IsLevelUp(int addexp)
		{
			if (m_level >= m_maxLevel)
			{
				return false;
			}
			if (m_exp + addexp == 0)
			{
				return false;
			}
			if (m_exp + addexp >= GetNextExp())
			{
				return true;
			}
			return false;
		}

		public bool IsLevelDown(int addexp)
		{
			if (m_level == 1)
			{
				return false;
			}
			if (m_exp + addexp < 0)
			{
				return true;
			}
			return false;
		}
	}

	[SerializeField]
	private ResultFlowLikability m_resultFlowLikability;

	[SerializeField]
	private ResultFlowFan m_resultFlowFan;

	[SerializeField]
	private ResultFlowFightStar m_resultFlowFightStar;

	[SerializeField]
	private MissionWindowAcquisition m_missionWindowAcquisition;

	[SerializeField]
	private GameObject m_eventParent;

	private BingoResultWindow m_bingoResultWindow;

	private TenshiGowanResult m_tenshiGowanResultWindow;

	private CompetitionScoreWindow m_competitionScoreWindow;

	private YellResult m_yellResultWindow;

	private YellUnableResult m_yellUnableWindow;

	private CharaStampEventResult m_charaStampEventResult;

	private ShopCompetitionResultWindow m_shopCompetitionResultWindow;

	[SerializeField]
	private PlayCountResultWindow m_playCountResultWindow;

	[SerializeField]
	private GameObject m_charaAll;

	[SerializeField]
	private GameObject m_topInfo;

	[SerializeField]
	private CardConnectWindow m_cardConnectNotice;

	[SerializeField]
	private MissionEventAcquireWindow m_missionEventAcquireNotice;

	[SerializeField]
	private CharaPickupGachaNoticeWindow m_charaPickupGachaNoticeWindow;

	private Dictionary<GameEventType, GameObject> ProgressMapWindows = new Dictionary<GameEventType, GameObject>();

	protected override async UniTask OnInitialize(CancellationToken cancellationToken)
	{
		_ = 1;
		try
		{
			var (mode, num) = GameDataManager.Account.ParticipationBillingEvent.Consume();
			await GameUtility.SendConsumeLog(mode, num);
			await CharaInit(cancellationToken);
			ResidentFlow.GetInstance().GetHeader().SetDisp(disp: true);
			ResidentFlow.GetInstance().GetHeader().SetStatusDisp(disp: false);
			ResidentFlow.GetInstance().GetHeader().SetSceneName("リザルト");
			SoundManager.PlayBgm("BGM_RESULT3").Forget();
		}
		catch (OperationCanceledException)
		{
			Debug.Log(GetType().Name + MethodBase.GetCurrentMethod().Name + ": OperationCanceledException Catched.");
		}
	}

	protected override async UniTask OnExecute(CancellationToken cancellationToken)
	{
		_ = 12;
		try
		{
			if (!GameDataManager.PlayerConfig.IsDispHelpFirstPlayResultScene && !DebugFlag.IsActive(DebugFlag.Flag.SkipGuideDisp))
			{
				await GuideDisp(cancellationToken);
				GameDataManager.PlayerConfig.IsDispHelpFirstPlayResultScene = true;
			}
			await GameDataManager.PlayerRecordGet(cancellationToken);
			PlayCharaRankVoice();
			await m_resultFlowLikability.StartScene(cancellationToken);
			await m_resultFlowFan.StartScene(cancellationToken);
			if (GameDataManager.Mahjong.TakuType != TakuType.Nima && !GameDataManager.Mahjong.IsPrivateMatch)
			{
				await m_resultFlowFightStar.StartScene();
			}
			await EventWindow(cancellationToken);
			await VoteWindow(cancellationToken);
			await MissionResult(cancellationToken);
			await m_cardConnectNotice.DispMemorialPrintNotice();
			await m_cardConnectNotice.DispMissionMemorialPrintNotice();
			await m_missionEventAcquireNotice.Display();
			await Save(cancellationToken);
			if (GameUtility.IsKonastaOrNetcafeBehavior)
			{
				if (Cloudstrap.IsDisconnect)
				{
					GameUtility.IsAppTerminatDecided = true;
				}
				Cloudstrap.UserEvent.End(Cloudstrap.UserEventType.Play);
			}
			await GotoNextScene();
		}
		catch (OperationCanceledException)
		{
			Debug.Log(GetType().Name + MethodBase.GetCurrentMethod().Name + ": OperationCanceledException Catched.");
		}
	}

	private async UniTask MissionResult(CancellationToken cancellationToken)
	{
		if (!GameDataManager.Account.IsGuestUser)
		{
			m_missionWindowAcquisition.Judge(JudgeLocation.Result);
			await ProgressMapWindow(cancellationToken);
			await m_missionWindowAcquisition.Open(JudgeLocation.Result);
		}
	}

	private async UniTask Save(CancellationToken cancellationToken)
	{
		GameDataManager.Player.OdekakeInit();
		try
		{
			cancellationToken.ThrowIfCancellationRequested();
			await GameDataManager.Version.Save();
			await GameDataManager.Player.Save();
			await GameDataManager.PlayerConfig.Save();
			await GameDataManager.Gacha.Save();
			await GameDataManager.Story.Save();
			await GameDataManager.Vote.Save();
			await GameDataManager.ScrambleFestival.Save();
			await GameDataManager.BingoEventRemake.Save();
			await MissionFlow.Save(JudgeLocation.Result);
			await GameDataManager.Log.Save();
			await GameDataManager.CharaStampEvent.Save();
			await GameDataManager.OthersEvent.Save();
			await GameDataManager.EventTaku.Save();
		}
		catch (OperationCanceledException)
		{
			Debug.Log(GetType().Name + ".Save: OperationCanceledException Catched.");
		}
		catch (NetworkResponseException ex2)
		{
			GameDataManager.PlayerConfig.IsDataSaveFailed = true;
			Debug.Log("NetworkResponseException Catched. " + ex2.Message);
		}
	}

	private async UniTask GotoNextScene()
	{
		if (await IsContinueEnable())
		{
			GotoNextScene("Continue");
		}
		else if (GameUtility.IsAppTerminatDecided)
		{
			GotoNextScene("ScopeClose");
		}
		else
		{
			GotoNextScene("Gameover");
		}
	}

	private async UniTask<bool> IsContinueEnable()
	{
		try
		{
			await GameDataManager.ExecLoadAtContinueEvaluation();
			if (GameDataManager.Mahjong.EventType != 0)
			{
				GameEventType gameEventType = GameEventStatus.ToGameEventType(GameDataManager.Mahjong.EventType);
				if (gameEventType == GameEventType.None)
				{
					Debug.LogError($"ResultFlow.IsContinueEnable: 対応するGameEventTypeが取得できませんでした。eventType:{GameDataManager.Mahjong.EventType}");
					return false;
				}
				if (!GameDataManager.System.GameControlInfo.EventStatus.IsActive(gameEventType))
				{
					Debug.Log("ResultFlow.IsContinueEnable: イベント卓がアクティブではないので終了します。");
					return false;
				}
			}
		}
		catch
		{
			Debug.LogWarning("ResultFlow.IsContinueEnable: イベントフラグが取得できませんでした。");
			return false;
		}
		if (CabinetManager.Game.Data.ContinueMenu == GameOptionsDef.ContinueOnOff.Off || TitleDemoUtility.IsParticipationError() || !ShopCloseUtility.IsEntryAcceptable() || GameUtility.IsAppTerminatDecided || Cloudstrap.IsDisconnect)
		{
			return false;
		}
		if (GameDataManager.Account.IsForceFreePlayUser || GameUtility.IsFreePlay())
		{
			return true;
		}
		Fraction item = GameUtility.ContinuePriceUtility.GetPriceRate(GameDataManager.Mahjong.TakuType, GameDataManager.Mahjong.EventType, GameDataManager.Mahjong.IntScore()).priceRate;
		int num = Math.Sign(item.Numerator) * Math.Sign(item.Denominator);
		if (item.Numerator == 0 || num < 0)
		{
			return true;
		}
		return !GameUtility.IsReservedGameOver;
	}

	private async UniTask CharaInit(CancellationToken cancellationToken)
	{
		RankType rankType = MahjongUtility.GetRankType(GameDataManager.Mahjong.TakuType, (Rank)GameDataManager.Mahjong.Rank);
		CharaType selectChara = GameDataManager.Player.SelectChara;
		CharacterDataManager.Emotion[] array = new CharacterDataManager.Emotion[3]
		{
			CharacterDataManager.Emotion.Laugh,
			CharacterDataManager.Emotion.Default,
			CharacterDataManager.Emotion.Cry
		};
		IDisposableAsset<Sprite> disposableAsset = await AssetLoader.LoadAssetAsync<Sprite>(AssetPathResolver.Resolve(PathType.StandingPose, selectChara, array[(int)rankType]));
		disposableAsset.AddTo(this);
		m_charaAll.GetComponent<Image>().sprite = disposableAsset.Asset;
	}

	private void PlayCharaRankVoice()
	{
		if (MahjongUtility.GetRankType(GameDataManager.Mahjong.TakuType, (Rank)GameDataManager.Mahjong.Rank) == RankType.RankTypeWorst)
		{
			SoundManager.PlayMyCharaVoice("VOX_GXX_RESULT2").Forget();
		}
		else
		{
			SoundManager.PlayMyCharaVoice("VOX_GXX_RESULT1").Forget();
		}
	}

	public static void SetGauge(Transform gauge, LevelCalc calc, bool gaugeDown = false)
	{
		if (gaugeDown)
		{
			gauge.Find("Mask").gameObject.SetActive(value: false);
			gauge.Find("Mask3").gameObject.SetActive(value: true);
			if (calc.GetNextExp() != 0)
			{
				gauge.Find("Mask2").Find("Bar2").transform.localPosition = new Vector3(-400f + 400f * (float)calc.m_dispExp / (float)calc.GetNextExp(), 0f, 0f);
				gauge.Find("Mask3").Find("Bar3").transform.localPosition = new Vector3(-400f + 400f * (float)calc.m_dispExpBefore / (float)calc.GetNextExp(), 0f, 0f);
				int num = ((calc.m_dispExp >= 0) ? calc.m_dispExp : 0);
				gauge.Find("GaugeNum").GetComponent<Text>().text = num + "/" + calc.GetNextExp();
			}
			else
			{
				gauge.Find("Mask2").Find("Bar2").transform.localPosition = new Vector3(-400f, 0f, 0f);
				gauge.Find("Mask3").Find("Bar3").transform.localPosition = new Vector3(-400f, 0f, 0f);
				gauge.Find("GaugeNum").GetComponent<Text>().text = "-/-";
			}
		}
		else
		{
			gauge.Find("Mask").gameObject.SetActive(value: true);
			gauge.Find("Mask3").gameObject.SetActive(value: false);
			if (calc.GetNextExp() != 0)
			{
				gauge.Find("Mask").Find("Bar").transform.localPosition = new Vector3(-400f + 400f * (float)calc.m_dispExp / (float)calc.GetNextExp(), 0f, 0f);
				gauge.Find("Mask2").Find("Bar2").transform.localPosition = new Vector3(-400f + 400f * (float)calc.m_dispExpBefore / (float)calc.GetNextExp(), 0f, 0f);
				gauge.Find("GaugeNum").GetComponent<Text>().text = calc.m_dispExp + "/" + calc.GetNextExp();
			}
			else
			{
				gauge.Find("Mask").Find("Bar").transform.localPosition = new Vector3(-400f, 0f, 0f);
				gauge.Find("Mask2").Find("Bar2").transform.localPosition = new Vector3(-400f, 0f, 0f);
				gauge.Find("GaugeNum").GetComponent<Text>().text = "-/-";
			}
		}
	}

	public static bool Skip()
	{
		if (GameUtility.IsCloudStreaming && Bi2xInput.GetKeyDown(KeyCode.Return))
		{
			return true;
		}
		if (!ResidentFlow.CustomInput.GetMouseButtonDown(0))
		{
			return IsStreamingDisconnect();
		}
		return true;
	}

	public static bool IsStreamingDisconnect()
	{
		if (!GameUtility.IsAppTerminatDecided)
		{
			return Cloudstrap.IsDisconnect;
		}
		return true;
	}

	private async UniTask GuideDisp(CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		float timeLimit = ((GameUtility.IsAppTerminatDecided || Cloudstrap.IsDisconnect) ? 5 : 20);
		ResidentFlow.GetInstance().GetTimer().SetPause(pause: true);
		m_topInfo.SetActive(value: true);
		m_topInfo.GetComponent<Animator>().SetTrigger("Start");
		await UniTask.Delay(TimeSpan.FromSeconds(2.0), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
		await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(timeLimit), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken), UniTask.WaitUntil(() => ResidentFlow.CustomInput.GetMouseButtonDown(0), PlayerLoopTiming.Update, cancellationToken));
		m_topInfo.GetComponent<Animator>().SetTrigger("Skip");
		await UniTask.Delay(TimeSpan.FromSeconds(1.0), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
		await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(timeLimit), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken), UniTask.WaitUntil(() => ResidentFlow.CustomInput.GetMouseButtonDown(0), PlayerLoopTiming.Update, cancellationToken));
		m_topInfo.GetComponent<Animator>().SetTrigger("Skip");
		SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
		m_topInfo.SetActive(value: false);
		GetCountDownTimer().SetPause(pause: false);
	}

	private async UniTask EventWindow(CancellationToken cancellationToken = default(CancellationToken))
	{
		CharaStampInfo charaStampInfo = GameDataManager.CharaStampEvent.GetCurrentEventInfo();
		if (charaStampInfo != null && !charaStampInfo.IsUsable)
		{
			if (GameDataManager.IsChaosForceActive() && charaStampInfo.ID == CharaStampID.Chaos)
			{
				charaStampInfo.ForceActive();
			}
			else
			{
				IDisposableAsset<GameObject> obj = await AssetLoader.LoadAssetAsync<GameObject>($"CharaStampEvent/Prefabs/{charaStampInfo.ID}/CharaStampEventResult.prefab");
				obj.AddTo(this);
				GameObject gameObject = UnityEngine.Object.Instantiate(obj.Asset, m_eventParent.transform, worldPositionStays: false);
				m_charaStampEventResult = gameObject.GetComponent<CharaStampEventResult>();
				await m_charaStampEventResult.OpenWindow(cancellationToken);
				if (charaStampInfo.IsUsable)
				{
					CharaEnabledSend.ResultData resultData = await RequestApiBase<CharaEnabledSend, CharaEnabledSend.ResultData, CharaType>.Send(CharaType.Chara11);
					if (resultData.IsGachaEnable)
					{
						GameDataManager.Notice.PickupGachaNoticeCharaType = CharaType.Chara11;
						GameDataManager.Notice.PickupGachaNoticeDate = resultData.EndDateString;
						if (GameDataManager.Notice.PickupGachaNoticeCharaType == CharaType.Chara11)
						{
							m_charaPickupGachaNoticeWindow.OpenWindow(null);
							await UniTask.WhenAny(UniTask.WaitWhile(() => m_charaPickupGachaNoticeWindow.isOpen), UniTask.Delay(TimeSpan.FromSeconds(20.0)));
							await m_charaPickupGachaNoticeWindow.CloseWindow();
						}
					}
				}
			}
		}
		if (GameDataManager.Mahjong.IsPrivateMatch)
		{
			return;
		}
		if (GameDataManager.Taikyoku.IsValidCompetition || DebugFlag.IsActive(DebugFlag.Flag.CompeResultCheckEnable))
		{
			var (beforeInfo, afterInfo) = await GameDataManager.Competition.GetLatestResult();
			GameDataManager.Mission.CompetitionInfo = afterInfo;
			IDisposableAsset<GameObject> obj2 = await AssetLoader.LoadAssetAsync<GameObject>("Competition/Common/Prefabs/CompeScoreWindow.prefab");
			obj2.AddTo(this);
			GameObject gameObject2 = UnityEngine.Object.Instantiate(obj2.Asset, m_eventParent.transform, worldPositionStays: false);
			m_competitionScoreWindow = gameObject2.GetComponent<CompetitionScoreWindow>();
			m_competitionScoreWindow.gameObject.SetActive(value: true);
			await m_competitionScoreWindow.OpenAsResultView(beforeInfo, afterInfo);
			await UniTask.WaitWhile(() => m_competitionScoreWindow.IsOpen);
		}
		if (GameDataManager.Taikyoku.IsValidShopCompetition || DebugFlag.IsActive(DebugFlag.Flag.ShopCompe001ForceEnable))
		{
			(ShopCompetitionInfo, ShopCompetitionInfo) tuple2 = await GameDataManager.ShopCompetition.GetLatestResult();
			ShopCompetitionInfo beforeInfo2 = tuple2.Item1;
			ShopCompetitionInfo afterInfo2 = tuple2.Item2;
			IDisposableAsset<GameObject> obj3 = await AssetLoader.LoadAssetAsync<GameObject>($"ShopCompetition/{GameDataManager.ShopCompetition.CurrentGameEventType}/ResultWindow.prefab");
			obj3.AddTo(this);
			GameObject gameObject3 = UnityEngine.Object.Instantiate(obj3.Asset, m_eventParent.transform, worldPositionStays: false);
			m_shopCompetitionResultWindow = gameObject3.GetComponent<ShopCompetitionResultWindow>();
			await m_shopCompetitionResultWindow.Open(beforeInfo2, afterInfo2);
			await UniTask.WaitWhile(() => m_shopCompetitionResultWindow.IsOpen);
		}
		if (GameDataManager.BingoEventRemake.IsEnable())
		{
			IDisposableAsset<GameObject> obj4 = await AssetLoader.LoadAssetAsync<GameObject>("Bingo/Prefabs/BingCardResult.prefab");
			obj4.AddTo(this);
			GameObject gameObject4 = UnityEngine.Object.Instantiate(obj4.Asset, m_eventParent.transform, worldPositionStays: false);
			m_bingoResultWindow = gameObject4.GetComponent<BingoResultWindow>();
			await m_bingoResultWindow.OpenWindow(cancellationToken);
			await UniTask.WaitWhile(() => !m_bingoResultWindow.IsClose, PlayerLoopTiming.Update, cancellationToken);
		}
		if (GameDataManager.TenshiGowanEvent.IsEnable())
		{
			IDisposableAsset<GameObject> obj5 = await AssetLoader.LoadAssetAsync<GameObject>("TenshiGowan/Prefabs/GowanScore.prefab");
			obj5.AddTo(this);
			GameObject gameObject5 = UnityEngine.Object.Instantiate(obj5.Asset, m_eventParent.transform, worldPositionStays: false);
			m_tenshiGowanResultWindow = gameObject5.GetComponent<TenshiGowanResult>();
			await m_tenshiGowanResultWindow.OpenWindow(cancellationToken);
		}
		if (GameDataManager.YellCampaign.IsEnable())
		{
			if (GameDataManager.YellCampaign.IsCharacterEnable(GameDataManager.Player.SelectChara))
			{
				IDisposableAsset<GameObject> obj6 = await AssetLoader.LoadAssetAsync<GameObject>("YellCampaign/Prefabs/YellResultWindow.prefab");
				obj6.AddTo(this);
				GameObject gameObject6 = UnityEngine.Object.Instantiate(obj6.Asset, m_eventParent.transform, worldPositionStays: false);
				m_yellResultWindow = gameObject6.GetComponent<YellResult>();
				await m_yellResultWindow.OpenWindow(cancellationToken);
			}
			else
			{
				IDisposableAsset<GameObject> obj7 = await AssetLoader.LoadAssetAsync<GameObject>("YellCampaign/Prefabs/YellUnableResult.prefab");
				obj7.AddTo(this);
				GameObject gameObject7 = UnityEngine.Object.Instantiate(obj7.Asset, m_eventParent.transform, worldPositionStays: false);
				m_yellUnableWindow = gameObject7.GetComponent<YellUnableResult>();
				await m_yellUnableWindow.OpenWindow(cancellationToken);
			}
		}
		if (!GameDataManager.Mahjong.IsConstancyTaku && !GameDataManager.Mahjong.IsCompetitionTaku)
		{
			foreach (BaseEventGameData gameData in GameDataManager.EventTaku.ActiveAtTaikyokuGameDatas())
			{
				IResultInfo resultInfo2 = gameData.ResultInfo;
				if (!string.IsNullOrEmpty(gameData.Master.ResultWindowPrefabPath))
				{
					IDisposableAsset<GameObject> obj8 = await AssetLoader.LoadAssetAsync<GameObject>(gameData.Master.ResultWindowPrefabPath);
					obj8.AddTo(this);
					GameObject gameObject8 = UnityEngine.Object.Instantiate(obj8.Asset, m_eventParent.transform, worldPositionStays: false);
					BaseEventTakuResultWindow eventResultWindow = gameObject8.GetComponent<BaseEventTakuResultWindow>();
					await eventResultWindow.Open(resultInfo2, () => Skip(), cancellationToken);
					await UniTask.WaitWhile(() => eventResultWindow.IsOpen);
				}
				gameData.ApplyResult(resultInfo2);
			}
		}
		if (GameDataManager.Taikyoku.IsScrambleMatch)
		{
			(bool, SchoolID) tuple3 = GameDataManager.Mahjong.ResultInfo.IsScrambleMatchWin();
			if (GameDataManager.Mahjong.Rank == 0)
			{
				if (tuple3.Item1)
				{
					GameDataManager.Player.GetCurrentChara().ScrambleMatchRecord[GameDataManager.Mahjong.Rank]++;
				}
			}
			else
			{
				GameDataManager.Player.GetCurrentChara().ScrambleMatchRecord[GameDataManager.Mahjong.Rank]++;
			}
		}
		if (!GameDataManager.Vote.IsResultEnable)
		{
			return;
		}
		VoteResultInfo resultInfo = GameDataManager.Vote.ResultInfo;
		if (resultInfo != null && !resultInfo.IsKiken)
		{
			IDisposableAsset<GameObject> obj9 = await AssetLoader.LoadAssetAsync<GameObject>(GameDataManager.Vote.CurrentInfo.Master.ResultWindowPath);
			obj9.AddTo(this);
			GameObject gameObject9 = UnityEngine.Object.Instantiate(obj9.Asset, m_eventParent.transform, worldPositionStays: false);
			VoteResultWindow resultWindow = gameObject9.GetComponent<VoteResultWindow>();
			await resultWindow.Open(resultInfo, () => Skip(), cancellationToken);
			await UniTask.WaitWhile(() => resultWindow.IsOpen);
			GameDataManager.Vote.ApplyResult();
		}
	}

	private async UniTask VoteWindow(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!GameDataManager.Vote.IsEnable)
		{
			return;
		}
		VoteEventInfo currentInfo = GameDataManager.Vote.CurrentInfo;
		if (!currentInfo.IsComplete() && !currentInfo.Master.IsHomeVote)
		{
			IDisposableAsset<GameObject> obj = await AssetLoader.LoadAssetAsync<GameObject>(currentInfo.Master.VoteWindowPath);
			obj.AddTo(this);
			GameObject gameObject = UnityEngine.Object.Instantiate(obj.Asset, m_eventParent.transform, worldPositionStays: false);
			VoteWindow voteWindow = gameObject.GetComponent<VoteWindow>();
			voteWindow.OpenWindow();
			await UniTask.WaitUntil(() => !voteWindow.isOpen || (GameUtility.IsCloudStreaming && (GameUtility.IsAppTerminatDecided || Cloudstrap.IsDisconnect)), PlayerLoopTiming.Update, cancellationToken);
			if (voteWindow.isOpen)
			{
				voteWindow.CloseWindow(cancellationToken).Forget();
			}
		}
	}

	private async UniTask ProgressMapWindow(CancellationToken cancellationToken = default(CancellationToken))
	{
		Dictionary<GameEventType, EventProgressMap.ProgressInfo> progressMapDict = GameDataManager.Mission.ProgressMapDict;
		if (progressMapDict == null || progressMapDict.Count <= 0)
		{
			return;
		}
		foreach (GameEventType key in GameDataManager.Mission.ProgressMapDict.Keys)
		{
			if (!GameDataManager.Mission.ProgressMapDict[key].IsDisplay)
			{
				continue;
			}
			Debug.Log($"ProgressMapDict IsDisplay: {key}");
			GameDataManager.Mission.ProgressMapDict[key].SetAchieveIndex();
			if (GameDataManager.Mission.ProgressMapDict[key].MoveCount > 0)
			{
				Debug.Log($"ProgressMapDict MoveCount : {key}");
				await m_playCountResultWindow.Open(key, GameDataManager.Mission.ProgressMapDict[key], null, cancellationToken);
				await UniTask.WaitWhile(() => m_playCountResultWindow.IsOpen);
			}
		}
	}
}
