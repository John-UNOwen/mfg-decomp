using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class ScrambleLotteryHomeWindow : MonoBehaviour
{
	[SerializeField]
	private Animator _windowAnimator;

	[SerializeField]
	private LotteryDetail _detailWindow;

	[SerializeField]
	private Button _closeButton;

	[SerializeField]
	private Button _detailOpenButton;

	[SerializeField]
	private Button _detailCloseButton;

	[SerializeField]
	private Button _rateOpenButton;

	[SerializeField]
	private Button _rateCloseButton;

	[SerializeField]
	private TextMeshProUGUI _dateText;

	[SerializeField]
	private TextMeshProUGUI _totalPointText;

	[SerializeField]
	private GameObject _rateWindowGameObject;

	[SerializeField]
	private ScrambleLotterySpiritWindow _spiritChoiceWindowGameObject;

	[SerializeField]
	private Button _spiritChoiceOpenButton;

	[SerializeField]
	private Button _lotteryStartButton;

	[SerializeField]
	private Button _spiritExchangeButton;

	[SerializeField]
	private ScrambleLotteryPlayWindow _scrambleLotteryPlayWindow;

	[SerializeField]
	private ScrambleLotteryExchangeWindow _scrambleLotteryExchangeWindow;

	[SerializeField]
	private TextMeshProUGUI _itemNumeratorCounter;

	[SerializeField]
	private TextMeshProUGUI _itemDenominatorCounter;

	[SerializeField]
	private GameObject _completeStamp;

	private bool _isOpen;

	private bool _isRateWindowOpen;

	public virtual bool IsOpen => _isOpen;

	private async void Start()
	{
		if (_closeButton != null)
		{
			_closeButton.OnClickAsObservable().Subscribe(async delegate
			{
				await Close();
			}).AddTo(this);
		}
		if (_detailOpenButton != null)
		{
			_detailOpenButton.OnClickAsObservable().Subscribe(delegate
			{
				_detailWindow.Open();
			}).AddTo(this);
		}
		if (_detailCloseButton != null)
		{
			_detailCloseButton.OnClickAsObservable().Subscribe(delegate
			{
				_detailWindow.Close();
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
		if (_spiritChoiceOpenButton != null)
		{
			_spiritChoiceOpenButton.OnClickAsObservable().Subscribe(async delegate
			{
				await _spiritChoiceWindowGameObject.Open(_scrambleLotteryPlayWindow, _detailWindow);
				Refresh();
			}).AddTo(this);
		}
		if (_lotteryStartButton != null)
		{
			_lotteryStartButton.OnClickAsObservable().Subscribe(async delegate
			{
				await _scrambleLotteryPlayWindow.Open(isWithoutDuplicates: false, _detailWindow);
				Refresh();
			}).AddTo(this);
		}
		if (_spiritExchangeButton != null)
		{
			_spiritExchangeButton.OnClickAsObservable().Subscribe(async delegate
			{
				await _scrambleLotteryExchangeWindow.Open();
				Refresh();
			}).AddTo(this);
		}
	}

	public async UniTask Init()
	{
		await _detailWindow.Init();
		await _spiritChoiceWindowGameObject.Init(_detailWindow);
	}

	public void Open(bool IsFirstDisp)
	{
		if (!_isOpen)
		{
			_spiritChoiceOpenButton.interactable = !IsFirstDisp;
			_lotteryStartButton.interactable = !IsFirstDisp;
			_spiritExchangeButton.interactable = !IsFirstDisp;
			_isOpen = true;
			Refresh();
			base.gameObject.SetActive(value: true);
			SoundManager.PlaySe("SFX_MJG_INFO1").Forget();
			if (_windowAnimator != null)
			{
				_windowAnimator.SetTrigger("In");
			}
		}
	}

	public async UniTask Close()
	{
		if (_isOpen)
		{
			SoundManager.PlaySe("SFX_SYS_CANCEL2").Forget();
			_detailWindow.Close(isPlaySe: false);
			_scrambleLotteryPlayWindow.Close(isPlaySe: false);
			_spiritChoiceWindowGameObject.Close(isPlaySe: false);
			if (_windowAnimator != null)
			{
				_windowAnimator.SetTrigger("Out");
			}
			_isOpen = false;
			await UniTask.Delay(TimeSpan.FromSeconds(1.0));
		}
	}

	public void Refresh()
	{
		_detailWindow.Refresh();
		if (_totalPointText != null)
		{
			_totalPointText.text = $"{GameDataManager.ScrambleFestival.CurrentInfo.Medal}";
		}
		if (_dateText != null)
		{
			_dateText.text = GameDataManager.ScrambleFestival.CurrentInfo.Master.DispDate();
		}
		(int, int) itemCount = _detailWindow.GetItemCount();
		int item = itemCount.Item1;
		int item2 = itemCount.Item2;
		bool flag = item == item2;
		if (_itemDenominatorCounter != null && _itemNumeratorCounter != null)
		{
			_itemNumeratorCounter.text = $"{item}";
			_itemDenominatorCounter.text = $"{item2}";
			_completeStamp.SetActive(flag);
		}
		_lotteryStartButton.interactable = !flag && GameDataManager.ScrambleFestival.CurrentInfo.IsLotteryEnable(isWithoutDuplicates: false);
		_spiritChoiceOpenButton.interactable = !flag && GameDataManager.ScrambleFestival.CurrentInfo.IsLotteryEnable(isWithoutDuplicates: true);
		_spiritExchangeButton.interactable = GameDataManager.ScrambleFestival.CurrentInfo.IsExchangeEnable();
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

	private void DebugPointChanger_Update()
	{
	}
}
