using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

public class CompetitionScoreDisp : MonoBehaviour
{
	private enum AnimationState
	{
		None,
		Wait,
		In,
		Loop,
		Out
	}

	[SerializeField]
	private TextMeshProUGUI _scoreText;

	[SerializeField]
	private Animator _scoreTextAnimator;

	[SerializeField]
	private bool _isSEPlay;

	private AnimationState _animationState;

	public void ButtobiMarkSEPlay()
	{
		SoundManager.PlaySe("SFX_MJG_INFO4").Forget();
	}

	public void Reflesh(int score, bool isEnableMinusValue = false)
	{
		_scoreText.text = ScoreToString(score, isbestScoreColor: false, isEnableMinusValue);
	}

	public void Reflesh(string text)
	{
		_scoreText.text = text;
	}

	public async UniTask Fade(float endValue, float durationSec, CancellationToken cancellationToken = default(CancellationToken))
	{
		await _scoreText.DOFade(endValue, durationSec).ToUniTask(TweenCancelBehaviour.Kill, cancellationToken);
	}

	public async UniTask RefleshWithDelay(int score, int delayMilliSec, bool bestScoreEffectEnable = false, CancellationToken cancellationToken = default(CancellationToken), Action fadeoutSE = null, Action bestScoreSE = null)
	{
		cancellationToken.ThrowIfCancellationRequested();
		try
		{
			if (_scoreTextAnimator != null)
			{
				fadeoutSE?.Invoke();
				_scoreTextAnimator.gameObject.SetActive(value: false);
				_scoreTextAnimator.gameObject.SetActive(value: true);
				_scoreTextAnimator.SetTrigger("In");
				_scoreTextAnimator.Update(0f);
				await UniTask.Delay(333, ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
			}
			_scoreText.text = ScoreToString(score, bestScoreEffectEnable);
			if (delayMilliSec > 0)
			{
				await UniTask.Delay(delayMilliSec, ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
			}
			if (bestScoreEffectEnable)
			{
				bestScoreSE?.Invoke();
			}
			if (_scoreTextAnimator != null)
			{
				_scoreTextAnimator.SetTrigger("Out");
				_scoreTextAnimator.Update(0f);
				await UniTask.Delay(333, ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
			}
		}
		catch (OperationCanceledException)
		{
			_scoreText.text = ScoreToString(score, bestScoreEffectEnable);
			_scoreTextAnimator.Play("CompeScoreBest1Out", 0, 1f);
		}
	}

	public async UniTask RefleshWithEnterAnimation(int score, CancellationToken cancellationToken = default(CancellationToken), Action fadeinSE = null)
	{
		await RefleshWithEnterAnimation(ScoreToString(score), cancellationToken, fadeinSE);
	}

	public async UniTask RefleshWithEnterAnimation(string str, CancellationToken cancellationToken = default(CancellationToken), Action fadeinSE = null)
	{
		cancellationToken.ThrowIfCancellationRequested();
		_scoreText.text = str;
		try
		{
			if (_scoreTextAnimator != null)
			{
				_scoreTextAnimator.gameObject.SetActive(value: false);
				_scoreTextAnimator.gameObject.SetActive(value: true);
				_scoreTextAnimator.SetTrigger("Out");
				_scoreTextAnimator.Update(0f);
				fadeinSE?.Invoke();
				await UniTask.WaitWhile(() => _scoreTextAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f, PlayerLoopTiming.Update, cancellationToken);
			}
		}
		catch (OperationCanceledException)
		{
			_scoreText.text = str;
			_scoreTextAnimator.Play("CompeScoreBest1Out", 0, 1f);
		}
	}

	private void Awake()
	{
		ObservableStateMachineTrigger observableStateMachineTrigger = null;
		Animator component = GetComponent<Animator>();
		if (component != null)
		{
			observableStateMachineTrigger = component.GetBehaviour<ObservableStateMachineTrigger>();
		}
		observableStateMachineTrigger?.OnStateEnterAsObservable().Subscribe(delegate(ObservableStateMachineTrigger.OnStateInfo onStateInfo)
		{
			if (onStateInfo.StateInfo.IsName("CompeScoreBest1Wait"))
			{
				_animationState = AnimationState.Wait;
			}
			else if (onStateInfo.StateInfo.IsName("CompeScoreBest1In"))
			{
				_animationState = AnimationState.In;
			}
			else if (onStateInfo.StateInfo.IsName("CompeScoreBest1Loop"))
			{
				_animationState = AnimationState.Loop;
			}
			else if (onStateInfo.StateInfo.IsName("CompeScoreBest1Out"))
			{
				_animationState = AnimationState.Out;
			}
		}).AddTo(this);
		observableStateMachineTrigger?.OnStateExitAsObservable().Subscribe(delegate
		{
		}).AddTo(this);
		observableStateMachineTrigger?.OnStateMachineEnterAsObservable().Subscribe(delegate
		{
		}).AddTo(this);
		observableStateMachineTrigger?.OnStateMachineExitAsObservable().Subscribe(delegate
		{
			_animationState = AnimationState.None;
		}).AddTo(this);
	}

	private void Start()
	{
	}

	private void Update()
	{
	}

	private string ScoreToString(int competitionScore, bool isbestScoreColor = false, bool isEnableMinusValue = false)
	{
		if (!isEnableMinusValue && competitionScore < 0)
		{
			return "―.―";
		}
		return string.Format("{0}{1}.{2}", isbestScoreColor ? "<color=#FF0000>" : "", competitionScore / 10, ((competitionScore < 0) ? (-competitionScore) : competitionScore) % 10);
	}
}
