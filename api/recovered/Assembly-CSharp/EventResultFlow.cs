using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Cysharp.Threading.Tasks;
using Konami.System.Net;
using MFG.Assets;
using MFG.GameData;
using MFG.GameRequest;
using MFG.Types;
using MFG.Types.Competition;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class EventResultFlow : BaseFlow
{
	[SerializeField]
	private Canvas _canvas;

	[SerializeField]
	private GameObject _compeScoreAll;

	[SerializeField]
	private GameObject _compeResultAll;

	[SerializeField]
	private CompetitionScoreWindow m_competitionScoreWindow;

	private CompetitionInfo _resultCompetitionInfo;

	[SerializeField]
	private Button _compeScoreAllNextButton;

	[SerializeField]
	private List<Sprite> _rankSprites = new List<Sprite>();

	protected override async UniTask OnInitialize(CancellationToken cancellationToken)
	{
		try
		{
			SoundManager.PlayBgm("BGM_RESULT5").Forget();
		}
		catch (OperationCanceledException)
		{
			Debug.Log(GetType().Name + MethodBase.GetCurrentMethod().Name + ": OperationCanceledException Catched.");
		}
	}

	protected override async UniTask OnExecute(CancellationToken cancellationToken)
	{
		_resultCompetitionInfo = GameDataManager.Competition.GetFinalResultCompetitionInfo();
		try
		{
			cancellationToken.ThrowIfCancellationRequested();
			await ScoreDisp(cancellationToken);
			await ResultEffect(cancellationToken);
			await WaitNext(cancellationToken);
			await GotoNextScene(cancellationToken);
		}
		catch (OperationCanceledException)
		{
			Debug.Log(GetType().Name + MethodBase.GetCurrentMethod().Name + ": OperationCanceledException Catched.");
		}
	}

	private async UniTask ScoreDisp(CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		_compeScoreAllNextButton.gameObject.SetActive(value: false);
		_compeScoreAll.SetActive(value: true);
		if (_resultCompetitionInfo != null)
		{
			m_competitionScoreWindow.OpenAsScoreView(_resultCompetitionInfo, isDetailButtonActive: true, isFinalResult: true).Forget();
			await UniTask.Delay(TimeSpan.FromSeconds(2.0), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
			await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(2.0), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken), UniTask.WaitUntil(() => ResultFlow.Skip()));
		}
	}

	private async UniTask ResultEffect(CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		if (_resultCompetitionInfo != null)
		{
			bool isGetTrophy = _resultCompetitionInfo.TrophyType != TrophyType.None;
			int rank = _resultCompetitionInfo.Rank;
			int rankRate = _resultCompetitionInfo.RankRate;
			TrophyType trophyType = _resultCompetitionInfo.TrophyType;
			_compeResultAll.transform.Find("Result").Find("Congra").gameObject.SetActive(isGetTrophy);
			CharacterDataManager.Emotion emotion = CharacterDataManager.Emotion.Default;
			if (isGetTrophy)
			{
				emotion = CharacterDataManager.Emotion.Laugh;
			}
			CharaType charaType = GameDataManager.Player.SelectChara;
			if (_resultCompetitionInfo.EntrySystemType == EntrySystemType.Character && _resultCompetitionInfo.EntryCharaType >= CharaType.Chara01)
			{
				charaType = _resultCompetitionInfo.EntryCharaType;
			}
			IDisposableAsset<Sprite> disposableAsset = await AssetLoader.LoadAssetAsync<Sprite>(AssetPathResolver.Resolve(PathType.StandingPose, charaType, emotion));
			disposableAsset.AddTo(this);
			_compeResultAll.transform.Find("Result").Find("CharaImage").GetComponent<Image>()
				.sprite = disposableAsset.Asset;
			if (isGetTrophy && trophyType != 0)
			{
				_compeResultAll.transform.Find("Result").Find("Trophy").gameObject.SetActive(value: true);
				IDisposableAsset<GameObject> obj = await AssetLoader.LoadAssetAsync<GameObject>(AssetPathResolver.ResolveCompetitionTrophyAssets(_resultCompetitionInfo.CompetitionID, trophyType));
				obj.AddTo(this);
				UnityEngine.Object.Instantiate(obj.Asset, _compeResultAll.transform.Find("Result").Find("Trophy").Find("TrophyParent"));
			}
			else
			{
				_compeResultAll.transform.Find("Result").Find("Trophy").gameObject.SetActive(value: false);
			}
			Transform transform = _compeResultAll.transform.Find("Result").Find("Rank");
			if (1 <= rank && rank <= 10)
			{
				transform.Find("CompeRankTop3").gameObject.SetActive(value: true);
				transform.Find("CompeRankOther").gameObject.SetActive(value: false);
				transform.Find("CompeRankTop3").Find("1st").GetComponent<Image>()
					.sprite = _rankSprites[rank];
			}
			else
			{
				transform.Find("CompeRankTop3").gameObject.SetActive(value: false);
				transform.Find("CompeRankOther").gameObject.SetActive(value: true);
				transform.Find("CompeRankOther").Find("Text").GetComponent<TextMeshProUGUI>()
					.text = CompetitionRankRateDisp.RankRateToString(rank, rankRate);
			}
			_compeResultAll.transform.Find("Result").Find("Info").Find("Text1")
				.gameObject.SetActive(isGetTrophy);
			_compeResultAll.SetActive(value: true);
			_compeResultAll.GetComponent<Animator>().SetTrigger("In");
			string label;
			switch (trophyType)
			{
			case TrophyType.Platinum:
				label = "SFX_MJG_RANK1";
				break;
			case TrophyType.Gold:
			case TrophyType.Silver:
				label = "SFX_MJG_RANK2";
				break;
			default:
				label = "SFX_MJG_RANK3";
				break;
			}
			SoundManager.PlaySe(label, 3f).Forget();
			await UniTask.Delay(TimeSpan.FromSeconds(1.0), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
			await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(20.0), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken), UniTask.WaitUntil(() => ResultFlow.Skip(), PlayerLoopTiming.Update, cancellationToken));
			_compeResultAll.GetComponent<Animator>().SetTrigger("Out");
		}
	}

	private async UniTask WaitNext(CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		_compeScoreAllNextButton.gameObject.SetActive(value: true);
		ResidentFlow.GetInstance().GetTimer().SetTimer(20f, disp: true);
		ResidentFlow.GetInstance().GetTimer().StartTimer();
		await base.OnTimerEnd.Merge(_compeScoreAllNextButton.OnClickAsObservable()).ToUniTask(useFirstValue: true, cancellationToken);
		if (!GetCountDownTimer().IsEnd())
		{
			SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
		}
	}

	public async UniTask GotoNextScene(CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		ResidentFlow.GetInstance().GetTimer().SetPause(pause: true);
		_canvas.GetComponent<GraphicRaycaster>().enabled = false;
		if (_resultCompetitionInfo != null)
		{
			try
			{
				await RequestApiBase<CompetitionEntry, CompetitionEntry.ResultData, CompetitionEntry.Param>.Send(new CompetitionEntry.Param
				{
					CompetitionID = _resultCompetitionInfo.CompetitionID,
					IsEntryRequest = false
				}, enableErrorScreenTransition: false);
			}
			catch (Exception)
			{
				Debug.LogWarning("CompetitionEntry: 結果閲覧済み通信に失敗しました。");
			}
			_resultCompetitionInfo.IsFinalResultDisplayed = true;
			GameDataManager.Player.RefleshCompetitionTrophy(GameDataManager.Competition.GetLatestCompetitionInfo(), GameDataManager.Competition.GetKAC2023CompetitionInfos());
		}
		try
		{
			await GameDataManager.Player.Save(cancellationToken);
		}
		catch (OperationCanceledException ex2)
		{
			Debug.Log("OperationCanceledException Catched. " + ex2.Message);
		}
		catch (NetworkResponseException ex3)
		{
			GameDataManager.PlayerConfig.IsDataSaveFailed = true;
			Debug.Log("NetworkResponseException Catched. " + ex3.Message);
		}
		GotoNextScene(GameDataManager.Competition.GetNextSceneWhenGotoHome());
	}
}
