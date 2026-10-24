using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class JanponHomeWindow : MonoBehaviour
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
	private List<JanponObject> _janponObjects;

	[SerializeField]
	private Button _btChangeJanponKind;

	[SerializeField]
	private Button _btChangeJanponKindToNormal;

	[SerializeField]
	private ScrambleLotteryPlayWindow _scrambleLotteryPlayWindow;

	private bool _isOpen;

	private bool _isRateWindowOpen;

	private bool _isSuper;

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
		foreach (JanponObject objs in _janponObjects)
		{
			objs.BtNormalPlay.OnClickAsObservable().Subscribe(async delegate
			{
				await _scrambleLotteryPlayWindow.Open(isWithoutDuplicates: false, _detailWindow, null, isPlaySe: true, objs.Kind);
				Refresh();
			}).AddTo(this);
			objs.BtSuperPlay.OnClickAsObservable().Subscribe(async delegate
			{
				await _spiritChoiceWindowGameObject.Open(_scrambleLotteryPlayWindow, _detailWindow, objs.Kind);
				Refresh();
			}).AddTo(this);
			objs.ModeChange(_isSuper);
		}
		if (_btChangeJanponKind != null)
		{
			_btChangeJanponKind.OnClickAsObservable().Subscribe(async delegate
			{
				_btChangeJanponKind.gameObject.SetActive(value: false);
				_btChangeJanponKindToNormal.gameObject.SetActive(value: true);
				_isSuper = true;
				foreach (JanponObject janponObject in _janponObjects)
				{
					janponObject.ModeChange(_isSuper);
				}
				Refresh();
			}).AddTo(this);
		}
		if (!(_btChangeJanponKindToNormal != null))
		{
			return;
		}
		_btChangeJanponKindToNormal.OnClickAsObservable().Subscribe(async delegate
		{
			_btChangeJanponKind.gameObject.SetActive(value: true);
			_btChangeJanponKindToNormal.gameObject.SetActive(value: false);
			_isSuper = false;
			foreach (JanponObject janponObject2 in _janponObjects)
			{
				janponObject2.ModeChange(_isSuper);
			}
			Refresh();
		}).AddTo(this);
	}

	private void Update()
	{
		MainButtonInteractable(!_scrambleLotteryPlayWindow.IsOpen && !_spiritChoiceWindowGameObject.IsOpen);
	}

	public void MainButtonInteractable(bool isActive)
	{
		_detailOpenButton.interactable = isActive;
		_rateOpenButton.interactable = isActive;
		_closeButton.interactable = isActive;
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
			_isOpen = true;
			Refresh(IsFirstDisp);
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

	public void Refresh(bool isFirstDisp = false)
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
		foreach (JanponObject janponObject in _janponObjects)
		{
			var (numerator, denominator) = _detailWindow.GetItemCount(janponObject.Kind);
			janponObject.Refresh(numerator, denominator, isFirstDisp);
		}
		_btChangeJanponKind.interactable = !isFirstDisp;
		_btChangeJanponKindToNormal.interactable = !isFirstDisp;
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
