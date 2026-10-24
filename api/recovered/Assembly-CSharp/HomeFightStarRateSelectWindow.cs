using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.GameData;
using MFG.Types;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class HomeFightStarRateSelectWindow : MonoBehaviour
{
	[SerializeField]
	private Button _superRiskyRateButton;

	[SerializeField]
	private Button _riskyRateButton;

	[SerializeField]
	private Button _steadilyRateButton;

	[SerializeField]
	private GameObject _explanationPanel;

	[SerializeField]
	private GameObject _superRiskyObj;

	[SerializeField]
	private GameObject _window;

	[SerializeField]
	private GameObject _explanationBlackScreen;

	[SerializeField]
	private GameObject _explanationBlackScreenWide;

	private bool _isOpen;

	private bool _isDecided;

	private Subject<GainLossType> m_onDecided = new Subject<GainLossType>();

	public IObservable<GainLossType> OnDecided => m_onDecided.AsObservable();

	public bool IsOpen => _isOpen;

	private void Start()
	{
		_superRiskyObj.SetActive(GameDataManager.CoopData.IsJanDriEnable() || GameDataManager.Account.IsPremiumMode());
		LayoutRebuilder.ForceRebuildLayoutImmediate(_window.GetComponent<RectTransform>());
		_superRiskyRateButton.OnClickAsObservable().Subscribe(async delegate
		{
			if (!_isDecided)
			{
				_isDecided = true;
				GameDataManager.Mahjong.GainLossType = GainLossType.SuperRisky;
				m_onDecided.OnNext(GameDataManager.Mahjong.GainLossType);
			}
		}).AddTo(this);
		_riskyRateButton.OnClickAsObservable().Subscribe(async delegate
		{
			if (!_isDecided)
			{
				_isDecided = true;
				GameDataManager.Mahjong.GainLossType = GainLossType.Risky;
				m_onDecided.OnNext(GameDataManager.Mahjong.GainLossType);
			}
		}).AddTo(this);
		_steadilyRateButton.OnClickAsObservable().Subscribe(async delegate
		{
			if (!_isDecided)
			{
				_isDecided = true;
				GameDataManager.Mahjong.GainLossType = GainLossType.Steadily;
				m_onDecided.OnNext(GameDataManager.Mahjong.GainLossType);
			}
		}).AddTo(this);
	}

	public async UniTask Open(TakuType takuType, CancellationToken cancellationToken)
	{
		if (_isOpen)
		{
			return;
		}
		_isOpen = true;
		for (Rank rank = Rank.Rank1; rank <= Rank.Rank4; rank++)
		{
			bool flag = RankElement_IsNeedActivate(takuType, rank);
			Transform transform = _window.transform.Find($"SuperHigh/SuperHighTable/Frame{(int)(rank + 1)}");
			Transform transform2 = _window.transform.Find($"High/HighTable/Frame{(int)(rank + 1)}");
			Transform transform3 = _window.transform.Find($"Low/LowTable/Frame{(int)(rank + 1)}");
			transform.gameObject.SetActive(flag);
			transform2.gameObject.SetActive(flag);
			transform3.gameObject.SetActive(flag);
			if (flag)
			{
				SetRateText(GainLossType.SuperRisky, rank, transform);
				SetRateText(GainLossType.Risky, rank, transform2);
				SetRateText(GainLossType.Steadily, rank, transform3);
			}
		}
		SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
		base.gameObject.SetActive(value: true);
		GetComponent<Animator>().SetTrigger("In");
		if (!GameDataManager.PlayerConfig.IsDispHelpFirstRateWindowDisp && !DebugFlag.IsActive(DebugFlag.Flag.SkipGuideDisp))
		{
			await FightStarRateGuideDisp(cancellationToken);
			GameDataManager.PlayerConfig.IsDispHelpFirstRateWindowDisp = true;
		}
		static bool RankElement_IsNeedActivate(TakuType argTakuType, Rank argRank)
		{
			switch (argTakuType)
			{
			case TakuType.Sanma:
				if (argRank == Rank.Rank4)
				{
					return false;
				}
				break;
			case TakuType.Nima:
				return false;
			}
			return true;
		}
	}

	private void SetRateText(GainLossType gainLossType, Rank rank, Transform frame)
	{
		int getAcquiredFightStar = DanLevelMaster.GetGetAcquiredFightStar(GameDataManager.Mahjong.TakuType, GameDataManager.Player.DanLevel, gainLossType, rank);
		frame.Find("NumText").GetComponent<TextMeshProUGUI>().text = Utility.GetSign(getAcquiredFightStar) + Math.Abs(getAcquiredFightStar);
		frame.Find("NumText").GetComponent<TextMeshProUGUI>().color = CustomColor.GetColor(getAcquiredFightStar);
	}

	private async UniTask FightStarRateGuideDisp(CancellationToken cancellationToken = default(CancellationToken))
	{
		float timeLimit = 20f;
		ResidentFlow.GetInstance().GetTimer().SetPause(pause: true);
		_explanationBlackScreen.SetActive(!GameDataManager.CoopData.IsJanDriEnable() && !GameDataManager.Account.IsPremiumMode());
		_explanationBlackScreenWide.SetActive(GameDataManager.CoopData.IsJanDriEnable() || GameDataManager.Account.IsPremiumMode());
		_explanationPanel.SetActive(value: true);
		_explanationPanel.GetComponent<Animator>().SetTrigger("Start");
		await UniTask.Delay(TimeSpan.FromSeconds(2.0), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
		await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(timeLimit), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken), UniTask.WaitUntil(() => ResidentFlow.CustomInput.GetMouseButtonDown(0), PlayerLoopTiming.Update, cancellationToken));
		_explanationPanel.GetComponent<Animator>().SetTrigger("Skip");
		await UniTask.Delay(TimeSpan.FromSeconds(1.0), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
		await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(timeLimit), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken), UniTask.WaitUntil(() => ResidentFlow.CustomInput.GetMouseButtonDown(0), PlayerLoopTiming.Update, cancellationToken));
		_explanationPanel.GetComponent<Animator>().SetTrigger("Skip");
		await UniTask.Delay(TimeSpan.FromSeconds(1.0), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
		await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(timeLimit), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken), UniTask.WaitUntil(() => ResidentFlow.CustomInput.GetMouseButtonDown(0), PlayerLoopTiming.Update, cancellationToken));
		_explanationPanel.SetActive(value: false);
		ResidentFlow.GetInstance().GetTimer().SetPause(pause: false);
	}

	public void TimeupForceDecide()
	{
		if (GameDataManager.Mahjong.GainLossType == GainLossType.Steadily)
		{
			_steadilyRateButton.onClick.Invoke();
			_steadilyRateButton.transition = Selectable.Transition.None;
			_steadilyRateButton.GetComponent<Animator>().SetTrigger("Selected");
		}
		else if (_superRiskyObj.activeSelf && GameDataManager.Mahjong.GainLossType == GainLossType.SuperRisky)
		{
			_superRiskyRateButton.onClick.Invoke();
			_superRiskyRateButton.transition = Selectable.Transition.None;
			_superRiskyRateButton.GetComponent<Animator>().SetTrigger("Selected");
		}
		else
		{
			_riskyRateButton.onClick.Invoke();
			_riskyRateButton.transition = Selectable.Transition.None;
			_riskyRateButton.GetComponent<Animator>().SetTrigger("Selected");
		}
	}
}
