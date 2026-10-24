using System;
using Cysharp.Threading.Tasks;
using MFG.GameData.EventTaku;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class BaseEventTakuHomeWindow : MonoBehaviour
{
	[SerializeField]
	protected Animator _windowAnimator;

	[SerializeField]
	protected GameObject _rateWindowGameObject;

	[SerializeField]
	protected Button _closeButton;

	[SerializeField]
	protected Button _rateOpenButton;

	[SerializeField]
	protected Button _rateCloseButton;

	[SerializeField]
	protected TextMeshProUGUI _dateText;

	[SerializeField]
	protected TextMeshProUGUI _totalPointText;

	[SerializeField]
	protected GameObject _rankingWindowGameObject;

	[SerializeField]
	protected Button _rankingOpenButton;

	[SerializeField]
	protected Button _rankingCloseButton;

	protected bool _isOpen;

	protected bool _isRateWindowOpen;

	protected bool _isRankingWindowOpen;

	protected BaseEventGameData _baseEventGameData;

	public virtual bool IsOpen => _isOpen;

	protected virtual void Start()
	{
		if (_closeButton != null)
		{
			_closeButton.OnClickAsObservable().Subscribe(async delegate
			{
				await Close();
			}).AddTo(this);
		}
		if (_rateOpenButton != null)
		{
			_rateOpenButton.OnClickAsObservable().Subscribe(delegate
			{
				RateWindow_Open();
			}).AddTo(this);
		}
		if (_rateCloseButton != null)
		{
			_rateCloseButton.OnClickAsObservable().Subscribe(delegate
			{
				RateWindow_Close();
			}).AddTo(this);
		}
		if (_rankingOpenButton != null)
		{
			_rankingOpenButton.OnClickAsObservable().Subscribe(delegate
			{
				RankingWindow_Open();
			}).AddTo(this);
		}
		if (_rankingCloseButton != null)
		{
			_rankingCloseButton.OnClickAsObservable().Subscribe(delegate
			{
				RankingWindow_Close();
			}).AddTo(this);
		}
	}

	public virtual void Open(BaseEventGameData baseEventGameData)
	{
		if (!_isOpen)
		{
			_baseEventGameData = baseEventGameData;
			_isOpen = true;
			Reflesh();
			base.gameObject.SetActive(value: true);
			SoundManager.PlaySe("SFX_MJG_INFO1").Forget();
			if (_windowAnimator != null)
			{
				_windowAnimator.SetTrigger("In");
			}
		}
	}

	public virtual async UniTask Close()
	{
		if (_isOpen)
		{
			SoundManager.PlaySe("SFX_SYS_CANCEL2").Forget();
			RateWindow_Close(isPlaySe: false);
			RankingWindow_Close(isPlaySe: false);
			if (_windowAnimator != null)
			{
				_windowAnimator.SetTrigger("Out");
			}
			_isOpen = false;
			await UniTask.Delay(TimeSpan.FromSeconds(1.0));
		}
	}

	protected virtual void Reflesh()
	{
		if (_totalPointText != null)
		{
			_totalPointText.text = $"{_baseEventGameData.TotalPoint}";
		}
		if (_dateText != null)
		{
			_dateText.text = _baseEventGameData.GetEventDateText();
		}
	}

	private void RateWindow_Open(bool isPlaySe = true)
	{
		if (!_isRateWindowOpen)
		{
			_isRateWindowOpen = true;
			if (_rateWindowGameObject != null)
			{
				_rateWindowGameObject.SetActive(value: true);
			}
			if (isPlaySe)
			{
				SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
			}
			if (_rateWindowGameObject != null)
			{
				_rateWindowGameObject.GetComponent<Animator>().SetTrigger("In");
			}
		}
	}

	private void RateWindow_Close(bool isPlaySe = true)
	{
		if (_isRateWindowOpen)
		{
			if (isPlaySe)
			{
				SoundManager.PlaySe("SFX_SYS_CANCEL2").Forget();
			}
			if (_rateWindowGameObject != null)
			{
				_rateWindowGameObject.GetComponent<Animator>().SetTrigger("Out");
				_rateWindowGameObject.SetActive(value: false);
			}
			_isRateWindowOpen = false;
		}
	}

	private void RankingWindow_Open(bool isPlaySe = true)
	{
		if (!_isRankingWindowOpen)
		{
			_isRankingWindowOpen = true;
			if (isPlaySe)
			{
				SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
			}
			if (_rankingWindowGameObject != null)
			{
				_rankingWindowGameObject.SetActive(value: true);
				_rankingWindowGameObject.GetComponent<Animator>().SetTrigger("In");
			}
		}
	}

	private void RankingWindow_Close(bool isPlaySe = true)
	{
		if (_isRankingWindowOpen)
		{
			if (isPlaySe)
			{
				SoundManager.PlaySe("SFX_SYS_CANCEL2").Forget();
			}
			if (_rankingWindowGameObject != null)
			{
				_rankingWindowGameObject.GetComponent<Animator>().SetTrigger("Out");
				_rankingWindowGameObject.SetActive(value: false);
			}
			_isRankingWindowOpen = false;
		}
	}

	private void DebugPointChanger_Update()
	{
	}
}
