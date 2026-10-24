using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.Assets;
using MFG.GameData;
using MFG.GameData.EventTaku;
using MFG.Types;
using MFG.Types.Competition;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class CompetitionScoreWindow : MonoBehaviour
{
	[SerializeField]
	private Animator _panelAnimator;

	[SerializeField]
	private GameObject _titleBannerParent;

	[SerializeField]
	private TextMeshProUGUI _periodText;

	[SerializeField]
	private GameObject _allBg;

	[SerializeField]
	private Image _bgImage;

	[SerializeField]
	private GameObject _aggregatingIcon;

	[SerializeField]
	private Button _detailButton;

	[SerializeField]
	private Button _missionButton;

	[SerializeField]
	private Button _closeButton;

	[SerializeField]
	private List<Image> _buttobiCharas;

	private CompetitionScoreBase _scorePlay;

	[SerializeField]
	private CompetitionHalfItScorePlay _halfItScorePlay;

	[SerializeField]
	private CompetitionOriginScorePlay _originScorePlay;

	[SerializeField]
	private CompetitionDoraBonusScorePlay _doraScorePlay;

	[SerializeField]
	private CompetitionMentanpinBonusScorePlay _mentanpinScorePlay;

	[HideInInspector]
	public MissionWindow _missionWindow;

	[HideInInspector]
	public CompetitionEntryWindow _competitionEntryWindow;

	private bool _isOpen;

	private TimeoutController _timeoutController = new TimeoutController();

	private CompetitionID _loadedTitleBannerCompetitionID;

	public bool IsOpen => _isOpen;

	public async UniTask OpenAsScoreView(CompetitionInfo competitionInfo, bool isDetailButtonActive = true, bool isFinalResult = false, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!_isOpen && competitionInfo != null)
		{
			_isOpen = true;
			_closeButton.gameObject.SetActive(!isFinalResult);
			await Init(competitionInfo, isButtobiCharaDisp: false, isDetailButtonActive);
			await _scorePlay.InitAtHome(competitionInfo);
			base.gameObject.SetActive(value: true);
			_panelAnimator.SetTrigger("In");
			_panelAnimator.Update(0f);
			await UniTask.WaitWhile(() => _panelAnimator.GetCurrentAnimatorStateInfo(0).IsName("CompeScoreAllIn") && _panelAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f, PlayerLoopTiming.Update, cancellationToken);
			AllButton_Enable(!isFinalResult);
		}
	}

	public async UniTask OpenAsResultView(CompetitionInfo beforeCompetitionInfo, CompetitionInfo afterCompetitionInfo, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (_isOpen || beforeCompetitionInfo == null || afterCompetitionInfo == null)
		{
			return;
		}
		_isOpen = true;
		CancellationToken token2 = _timeoutController.Timeout(TimeSpan.FromSeconds((GameUtility.IsAppTerminatDecided || Cloudstrap.IsDisconnect) ? 1 : 20));
		using CancellationTokenSource linkedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, token2);
		CancellationToken token = linkedCancellationTokenSource.Token;
		try
		{
			int num = 0;
			try
			{
				await Init(afterCompetitionInfo, afterCompetitionInfo.ScoreType == ScoreType.ButtobiHalfIt, isDetailButtonActive: false);
				await _scorePlay.InitAtResult(beforeCompetitionInfo, afterCompetitionInfo, token);
				base.gameObject.SetActive(value: true);
				_panelAnimator.SetTrigger("In");
				_panelAnimator.Update(0f);
				await UniTask.WhenAny(UniTask.WaitWhile(() => _panelAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f, PlayerLoopTiming.Update, token), UniTask.WaitUntil(() => ResultFlow.Skip()));
				await _scorePlay.ResultAnimationPlay(afterCompetitionInfo, token);
				AllButton_Enable(isEnable: true);
				await UniTask.WaitWhile(() => IsOpen, PlayerLoopTiming.Update, token);
			}
			catch (OperationCanceledException)
			{
				num = 1;
			}
			if (num == 1 && _timeoutController.IsTimeout())
			{
				await Close();
			}
		}
		finally
		{
			_timeoutController.Reset();
		}
	}

	public async UniTask Close(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!_isOpen)
		{
			return;
		}
		GetComponent<CanvasGroup>().blocksRaycasts = false;
		AllButton_Enable(isEnable: false);
		_panelAnimator.SetTrigger("Out");
		_panelAnimator.Update(0f);
		try
		{
			await UniTask.WaitWhile(() => _panelAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f, PlayerLoopTiming.Update, cancellationToken);
		}
		catch (OperationCanceledException)
		{
			Debug.Log("Canceled");
		}
		_scorePlay.Close();
		_isOpen = false;
	}

	private void Start()
	{
		CancellationToken token = this.GetCancellationTokenOnDestroy();
		_detailButton.OnClickAsObservable().Subscribe(async delegate
		{
			if (_competitionEntryWindow != null)
			{
				await SoundManager.PlaySe("SFX_SYS_INPUT1");
				_competitionEntryWindow.WindowOpen().Forget();
			}
		}).AddTo(this);
		_missionButton.gameObject.SetActive(value: false);
		_missionButton.OnClickAsObservable().Subscribe(async delegate
		{
			if (_missionWindow != null)
			{
				await SoundManager.PlaySe("SFX_SYS_INPUT1");
				await SoundManager.PlayMyCharaVoice("VOX_GXX_MENU5");
				await _missionWindow.Play();
			}
		}).AddTo(this);
		_closeButton.OnClickAsObservable().Subscribe(async delegate
		{
			if (_isOpen)
			{
				SoundManager.PlaySe("SFX_SYS_CANCEL1").Forget();
			}
			try
			{
				await Close(token);
			}
			catch (OperationCanceledException)
			{
			}
		}).AddTo(this);
	}

	public async UniTask Init(CompetitionInfo competitionInfo, bool isButtobiCharaDisp, bool isDetailButtonActive)
	{
		GetComponent<CanvasGroup>().blocksRaycasts = true;
		AllButton_Enable(isEnable: false);
		_scorePlay = competitionInfo.CompetitionID switch
		{
			CompetitionID.Competition006 => _halfItScorePlay, 
			CompetitionID.Competition007 => _doraScorePlay, 
			CompetitionID.Competition008 => _mentanpinScorePlay, 
			CompetitionID.Competition009 => _halfItScorePlay, 
			_ => _originScorePlay, 
		};
		if (_loadedTitleBannerCompetitionID != competitionInfo.CompetitionID)
		{
			foreach (Transform item in _titleBannerParent.transform)
			{
				UnityEngine.Object.Destroy(item.gameObject);
			}
			IDisposableAsset<GameObject> obj = await AssetLoader.LoadAssetAsync<GameObject>(AssetPathResolver.ResolveCompetitionAssets(competitionInfo.CompetitionID, CompetitionAssetType.ScoreWindowTitleBannerPrefab));
			obj.AddTo(this);
			UnityEngine.Object.Instantiate(obj.Asset, _titleBannerParent.transform, worldPositionStays: false);
			_loadedTitleBannerCompetitionID = competitionInfo.CompetitionID;
		}
		if (await AssetLoader.Exists(competitionInfo.BgPath))
		{
			_allBg.SetActive(value: true);
			IDisposableAsset<Sprite> disposableAsset = await AssetLoader.LoadAssetAsync<Sprite>(competitionInfo.BgPath);
			disposableAsset.AddTo(this);
			_bgImage.sprite = disposableAsset.Asset;
		}
		else
		{
			_allBg.SetActive(value: false);
		}
		bool active = competitionInfo.CompetitionStatus == CompetitionStatus.Aggregating || competitionInfo.CompetitionStatus == CompetitionStatus.None;
		_aggregatingIcon.SetActive(active);
		_detailButton.gameObject.SetActive(_competitionEntryWindow != null && competitionInfo.IsInProgress && isDetailButtonActive);
		_periodText.text = (string.IsNullOrEmpty(competitionInfo.Period) ? "" : ("大会期間： " + competitionInfo.Period));
		foreach (Image buttobiChara in _buttobiCharas)
		{
			buttobiChara.gameObject.SetActive(value: false);
		}
		if (!isButtobiCharaDisp)
		{
			return;
		}
		try
		{
			BlowAwayChaInfo blowAwayInfo = ((ResultInfo)GameDataManager.EventTaku.Current(competitionInfo.CompeEventType).ResultInfo).BlowAwayInfo;
			if (blowAwayInfo.ChaInfos[Cha.Jicha].BlowawayState != BlowawayState.Blowaway)
			{
				return;
			}
			List<CharaType> buttobiCharas = (from x in blowAwayInfo.ChaInfos.Values
				where x.BlowawayState == BlowawayState.Blowawayed
				select x.CharaType).ToList();
			if (buttobiCharas != null && buttobiCharas.Count > 0)
			{
				for (int i = 0; i < buttobiCharas.Count; i++)
				{
					IDisposableAsset<Sprite> disposableAsset2 = await AssetLoader.LoadAssetAsync<Sprite>($"CutinItem/{buttobiCharas[i]}/Houju.png");
					disposableAsset2.AddTo(this);
					_buttobiCharas[i].sprite = disposableAsset2.Asset;
					_buttobiCharas[i].gameObject.SetActive(value: true);
				}
			}
		}
		catch
		{
		}
	}

	private void AllButton_Enable(bool isEnable)
	{
		_detailButton.enabled = isEnable;
		_missionButton.enabled = isEnable;
		_closeButton.enabled = isEnable;
	}
}
