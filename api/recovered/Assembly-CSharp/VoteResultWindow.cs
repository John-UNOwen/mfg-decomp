using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MFG.GameData;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class VoteResultWindow : MonoBehaviour
{
	[SerializeField]
	private Animator _windowAnimator;

	[SerializeField]
	private string WindowLoopStateName = "CompeScoreAllLoop";

	[SerializeField]
	private Animator _scoreAnimator;

	[SerializeField]
	private string ScoreLoopStateName = "GalaxyMedalGetLoop";

	[SerializeField]
	private TextMeshProUGUI _ranktText;

	[SerializeField]
	private TextMeshProUGUI _totalBonusPointText;

	[SerializeField]
	private TextMeshProUGUI _acquiredBonusPointText;

	[SerializeField]
	private GameObject _jandriBonusIcon;

	[SerializeField]
	private Button _closeButton;

	[SerializeField]
	private TextMeshProUGUI _acquiredPointText;

	[SerializeField]
	private TextMeshProUGUI _totalAcquiredPointText;

	private VoteResultInfo _resultInfo;

	private bool _isOpen;

	private TimeoutController _timeoutController = new TimeoutController();

	public bool IsOpen => _isOpen;

	private void Start()
	{
		if (_closeButton != null)
		{
			_closeButton.OnClickAsObservable().Subscribe(delegate
			{
				Close();
			}).AddTo(this);
		}
	}

	public async UniTask Open(VoteResultInfo resultInfo, Func<bool> skipFunc = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (resultInfo == null || _isOpen)
		{
			return;
		}
		_isOpen = true;
		_resultInfo = resultInfo;
		AllButton_Enable(isEnable: false);
		CancellationToken token2 = _timeoutController.Timeout(TimeSpan.FromSeconds((GameUtility.IsAppTerminatDecided || Cloudstrap.IsDisconnect) ? 5 : 20));
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
				await UniTask.WhenAny(UniTask.WaitUntil(() => _windowAnimator.GetCurrentAnimatorStateInfo(0).IsName(WindowLoopStateName), PlayerLoopTiming.Update, token), UniTask.WaitUntil(() => ResultFlow.Skip()));
			}
			if (_scoreAnimator != null)
			{
				_scoreAnimator.SetTrigger("In");
				await UniTask.WhenAny(UniTask.WaitUntil(() => _scoreAnimator.GetCurrentAnimatorStateInfo(0).IsName(ScoreLoopStateName), PlayerLoopTiming.Update, token), UniTask.WaitUntil(() => ResultFlow.Skip()));
			}
			await TotalAcquiredPoint_Reflesh(skipFunc, token);
			AllButton_Enable(isEnable: true);
			await UniTask.WhenAny(UniTask.WaitWhile(() => IsOpen, PlayerLoopTiming.Update, token), UniTask.WaitUntil(() => ResultFlow.Skip()));
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

	public void Close()
	{
		if (_isOpen)
		{
			SoundManager.StopSeAll().Forget();
			SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
			_windowAnimator.SetTrigger("Out");
			_isOpen = false;
		}
	}

	private void Reflesh(VoteResultInfo resultInfo)
	{
		if (_jandriBonusIcon != null)
		{
			_jandriBonusIcon.SetActive(resultInfo.IsJandriEffective);
		}
		_ranktText.text = $"{(int)(resultInfo.MyRank + 1)}";
		_totalBonusPointText.text = $"{resultInfo.RankBonus}";
		_acquiredBonusPointText.text = $"{resultInfo.ProBonus}";
		_acquiredPointText.text = $"{resultInfo.AcquiredPoint}";
		_totalAcquiredPointText.text = $"{resultInfo.BeforeTotalAcquiredPoint}";
	}

	private async UniTask TotalAcquiredPoint_Reflesh(Func<bool> skipFunc, CancellationToken cancellationToken)
	{
		float duration = 1f;
		bool isPlayEnd = false;
		int prevTotalAcquiredPoint = _resultInfo.AcquiredPoint;
		int afterTotalAcquiredPoint = _resultInfo.AcquiredPoint + _resultInfo.BeforeTotalAcquiredPoint;
		Tweener tweener = DOVirtual.Float(_resultInfo.BeforeTotalAcquiredPoint, afterTotalAcquiredPoint, duration, delegate(float value)
		{
			int num = (int)value;
			_totalAcquiredPointText.text = $"{num}";
			int num2 = _resultInfo.AcquiredPoint - (num - _resultInfo.BeforeTotalAcquiredPoint);
			_acquiredPointText.text = $"{num2}";
			if (prevTotalAcquiredPoint != num2)
			{
				SoundManager.PlaySe("SFX_SYS_BONUS1").Forget();
			}
			prevTotalAcquiredPoint = num2;
		}).SetEase(Ease.Linear).OnComplete(delegate
		{
			isPlayEnd = true;
		});
		try
		{
			await UniTask.WhenAny(UniTask.WaitUntil(() => isPlayEnd, PlayerLoopTiming.Update, cancellationToken), UniTask.WaitUntil(() => skipFunc != null && skipFunc(), PlayerLoopTiming.Update, cancellationToken));
		}
		catch (OperationCanceledException)
		{
		}
		tweener.Complete();
		_acquiredPointText.text = $"{0}";
		_totalAcquiredPointText.text = $"{afterTotalAcquiredPoint}";
	}

	private void AllButton_Enable(bool isEnable)
	{
		if (_closeButton != null)
		{
			_closeButton.enabled = isEnable;
			_closeButton.gameObject.SetActive(isEnable);
		}
	}
}
