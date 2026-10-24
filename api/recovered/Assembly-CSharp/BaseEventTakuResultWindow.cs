using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MFG.GameData.EventTaku;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class BaseEventTakuResultWindow : MonoBehaviour
{
	[SerializeField]
	protected Animator _windowAnimator;

	[SerializeField]
	protected string WindowLoopStateName = "CompeScoreAllLoop";

	[SerializeField]
	protected Animator _scoreAnimator;

	[SerializeField]
	protected string ScoreLoopStateName = "GalaxyEventScoreLoop";

	[SerializeField]
	protected EventTakuRankBonusDisp _rankBonusDisp;

	[SerializeField]
	protected List<EventTakuOtherBonusDisp> _otherBbonusDisps = new List<EventTakuOtherBonusDisp>();

	[SerializeField]
	private GameObject _jandriBonusIcon;

	[SerializeField]
	protected Button _closeButton;

	[SerializeField]
	private TextMeshProUGUI _acquiredPointText;

	[SerializeField]
	private TextMeshProUGUI _totalAcquiredPointText;

	protected IResultInfo _resultInfo;

	protected bool _isOpen;

	protected TimeoutController _timeoutController = new TimeoutController();

	public virtual bool IsOpen => _isOpen;

	protected virtual void Start()
	{
		if (_closeButton != null)
		{
			_closeButton.OnClickAsObservable().Subscribe(delegate
			{
				Close();
			}).AddTo(this);
		}
	}

	public virtual async UniTask Open(IResultInfo resultInfo, Func<bool> skipFunc = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (_isOpen)
		{
			return;
		}
		_isOpen = true;
		_resultInfo = resultInfo;
		AllButton_Enable(isEnable: false);
		CancellationToken token2 = _timeoutController.Timeout(TimeSpan.FromSeconds(20.0));
		using CancellationTokenSource linkedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, token2);
		CancellationToken token = linkedCancellationTokenSource.Token;
		try
		{
			if (_closeButton != null)
			{
				_closeButton.gameObject.SetActive(value: false);
			}
			base.gameObject.SetActive(value: true);
			Reflesh(resultInfo);
			if (_windowAnimator != null)
			{
				_windowAnimator.GetComponent<Animator>().SetTrigger("In");
				await UniTask.WhenAny(UniTask.WaitUntil(() => _windowAnimator.GetCurrentAnimatorStateInfo(0).IsName(WindowLoopStateName), PlayerLoopTiming.Update, token), UniTask.WaitUntil(delegate
				{
					if (ResultFlow.Skip())
					{
						_windowAnimator.SetTrigger("Skip1");
						return true;
					}
					return false;
				}));
			}
			if (_scoreAnimator != null)
			{
				_scoreAnimator.SetTrigger("In");
				await UniTask.WhenAny(UniTask.WaitUntil(() => _scoreAnimator.GetCurrentAnimatorStateInfo(0).IsName(ScoreLoopStateName), PlayerLoopTiming.Update, token), UniTask.WaitUntil(delegate
				{
					if (ResultFlow.Skip())
					{
						_scoreAnimator.SetTrigger("Skip");
						return true;
					}
					return false;
				}));
			}
			await TotalAcquiredPoint_Reflesh(skipFunc, token);
			AllButton_Enable(isEnable: true);
			await UniTask.WhenAny(UniTask.WaitUntil(() => !IsOpen, PlayerLoopTiming.Update, token), UniTask.WaitUntil(delegate
			{
				if (ResultFlow.IsStreamingDisconnect())
				{
					Close();
					return true;
				}
				return false;
			}));
		}
		catch (OperationCanceledException)
		{
			if (_timeoutController.IsTimeout())
			{
				Close();
			}
		}
		finally
		{
			_timeoutController.Reset();
		}
	}

	public virtual void Close()
	{
		if (_isOpen)
		{
			SoundManager.StopSeAll().Forget();
			SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
			_windowAnimator.SetTrigger("Out");
			_isOpen = false;
		}
	}

	protected virtual void Reflesh(IResultInfo resultInfo)
	{
		if (_jandriBonusIcon != null)
		{
			_jandriBonusIcon.SetActive(resultInfo.IsJandriEffective);
		}
		_rankBonusDisp.Reflesh(resultInfo.RankBonus);
		foreach (EventTakuOtherBonusDisp otherBbonusDisp in _otherBbonusDisps)
		{
			otherBbonusDisp.Reflesh(resultInfo.OtherBonuses);
		}
		_acquiredPointText.text = $"{resultInfo.AcquiredPoint}";
		_totalAcquiredPointText.text = $"{resultInfo.BeforeTotalAcquiredPoint}";
	}

	protected virtual async UniTask TotalAcquiredPoint_Reflesh(Func<bool> skipFunc, CancellationToken cancellationToken)
	{
		float durationTime = 1f;
		bool isPlayEnd = false;
		int prevTotalAcquiredPoint = _resultInfo.AcquiredPoint;
		int afterTotalAcquiredPoint = _resultInfo.AcquiredPoint + _resultInfo.BeforeTotalAcquiredPoint;
		await UniTask.WhenAny(UniTask.WaitUntil(() => _scoreAnimator.GetCurrentAnimatorStateInfo(0).IsName("GalaxyMedalGetLoop"), PlayerLoopTiming.Update, cancellationToken), UniTask.WaitUntil(delegate
		{
			if (ResultFlow.Skip())
			{
				_scoreAnimator.SetTrigger("Skip");
				return true;
			}
			return false;
		}));
		await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(1.0), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken), UniTask.WaitUntil(() => ResultFlow.IsStreamingDisconnect()));
		Tweener tweener = DOVirtual.Float(_resultInfo.BeforeTotalAcquiredPoint, afterTotalAcquiredPoint, durationTime, delegate(float value)
		{
			int num = (int)value;
			_totalAcquiredPointText.text = $"{num}";
			int num2 = _resultInfo.AcquiredPoint - (num - _resultInfo.BeforeTotalAcquiredPoint);
			_acquiredPointText.text = $"{num2}";
			bool flag = _scoreAnimator.transform.Find("MedalNum") == null || _scoreAnimator.transform.Find("MedalNum").gameObject.activeSelf;
			if (prevTotalAcquiredPoint != num2 && flag)
			{
				SoundManager.PlaySe("SFX_SYS_BONUS1").Forget();
			}
			prevTotalAcquiredPoint = num2;
		}).SetEase(Ease.Linear).OnComplete(delegate
		{
			isPlayEnd = true;
		});
		await UniTask.WhenAny(UniTask.WaitUntil(() => isPlayEnd, PlayerLoopTiming.Update, cancellationToken), UniTask.WaitUntil(() => ResultFlow.Skip()));
		tweener.Complete();
		_acquiredPointText.text = $"{0}";
		_totalAcquiredPointText.text = $"{afterTotalAcquiredPoint}";
	}

	protected virtual void AllButton_Enable(bool isEnable)
	{
		if (_closeButton != null)
		{
			_closeButton.enabled = isEnable;
			_closeButton.gameObject.SetActive(isEnable);
		}
	}
}
