using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MFG.Assets;
using MFG.Types;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class OmikujiHomeWindow : MonoBehaviour
{
	[SerializeField]
	private Animator _windowAnimator;

	[SerializeField]
	private Button _closeButton;

	[SerializeField]
	private Button _omikujiPlayButton;

	[SerializeField]
	private TextMeshProUGUI _dateText;

	[SerializeField]
	private OmikujiPlayWindow _lotteryPlayWindow;

	[SerializeField]
	private GameObject _todayLuck;

	[SerializeField]
	private Image _omikujiImage;

	[SerializeField]
	private GameObject _kaburiMessage;

	private bool _isOpen;

	private bool _isPlay;

	private bool _isFirstDisp;

	private bool _isRateWindowOpen;

	public virtual bool IsOpen => _isOpen;

	public virtual bool IsPlay => _isPlay;

	private async void Start()
	{
		if (_closeButton != null)
		{
			_closeButton.OnClickAsObservable().Subscribe(async delegate
			{
				await Close();
			}).AddTo(this);
		}
		if (_omikujiPlayButton != null)
		{
			_omikujiPlayButton.OnClickAsObservable().Subscribe(async delegate
			{
				_isPlay = true;
				await _lotteryPlayWindow.Open(_isFirstDisp);
				await Refresh();
				_isPlay = false;
			}).AddTo(this);
		}
	}

	private void Update()
	{
		if (Bi2xInput.GetKeyDown(KeyCode.R))
		{
			GameDataManager.OthersEvent.CurrentOmikujiInfo.ResetTimeStamp();
			Refresh().Forget();
		}
		else if (Bi2xInput.GetKeyDown(KeyCode.C))
		{
			GameDataManager.OthersEvent.CurrentOmikujiInfo.ResetDoneChara();
			Refresh().Forget();
		}
	}

	public async UniTask Open(bool isFirstDisp = false)
	{
		if (!_isOpen)
		{
			_isOpen = true;
			_isFirstDisp = isFirstDisp;
			await Refresh();
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
			if (_windowAnimator != null)
			{
				_windowAnimator.SetTrigger("Out");
			}
			_isOpen = false;
			await UniTask.Delay(TimeSpan.FromSeconds(1.0));
		}
	}

	public async UniTask Refresh()
	{
		if (_dateText != null)
		{
			_dateText.text = GameDataManager.OthersEvent.CurrentOmikujiInfo.Master.DispDate();
		}
		List<CharaType> lotteryCharaTypes = GameDataManager.OthersEvent.CurrentOmikujiInfo.GetLotteryCharaTypes();
		_kaburiMessage.SetActive(lotteryCharaTypes.Count > 0);
		if (GameDataManager.OthersEvent.CurrentOmikujiInfo.IsOmikujiPlayEnable())
		{
			_omikujiPlayButton.gameObject.SetActive(value: true);
			_todayLuck.SetActive(value: false);
			return;
		}
		_omikujiPlayButton.gameObject.SetActive(value: false);
		if (GameDataManager.OthersEvent.CurrentOmikujiInfo.TodayCharaTypes >= CharaType.Chara01)
		{
			IDisposableAsset<Sprite> disposableAsset = await AssetLoader.LoadAssetAsync<Sprite>(GameDataManager.OthersEvent.CurrentOmikujiInfo.TodayCharaImagePath);
			disposableAsset.AddTo(this);
			_omikujiImage.sprite = disposableAsset.Asset;
			_todayLuck.SetActive(value: true);
		}
		else
		{
			_todayLuck.SetActive(value: false);
		}
	}
}
