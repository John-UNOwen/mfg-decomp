using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using EagDef;
using KAMUNITY;
using MFG.GameRequest;
using TestModeSkeleton;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

public class BreakTimeWindow : MonoBehaviour
{
	private enum EndType
	{
		None,
		Timeout,
		ShopClose,
		ServerMaintenance,
		QRUnable
	}

	[SerializeField]
	private CountDownTimer _countDownTimer;

	[SerializeField]
	private GameObject _tenkeyWindow;

	[SerializeField]
	private CardEntryManager _cardEntryManager;

	[SerializeField]
	private GameObject _authDialogRoot;

	[SerializeField]
	private LoadingBar _loadingBar;

	[SerializeField]
	private Image _bgImage;

	[SerializeField]
	private Sprite _acBg;

	[SerializeField]
	private Sprite _netcafeBg;

	[SerializeField]
	private Sprite _qrBg;

	[SerializeField]
	private Button _qrButton;

	[SerializeField]
	private Image _qrImage;

	[SerializeField]
	private GameObject _qrDisplay;

	public bool IsAuthed;

	private bool _isAuthDialogOpen;

	private bool _isDecided;

	private bool _isInitialized;

	private CompositeDisposable _disposables = new CompositeDisposable();

	private CancellationTokenSource _authDialogCancellationTokenSource = new CancellationTokenSource();

	private float _time;

	public bool IsOpen { get; private set; }

	private void Start()
	{
		_qrDisplay.SetActive(GameDataManager.Account.IsCardless);
		if (!GameDataManager.Account.IsCardless)
		{
			return;
		}
		_qrButton.OnClickAsObservable().Subscribe(async delegate
		{
			Sprite sprite = await QRLoginManager.QRBeginToDisplay();
			if (sprite != null)
			{
				_qrButton.gameObject.SetActive(value: false);
				_qrImage.sprite = sprite;
				_qrImage.gameObject.SetActive(value: true);
				_time = Time.time;
			}
			else if (!_isDecided)
			{
				_isDecided = true;
				_qrDisplay.SetActive(value: false);
				SingletonBehaviour<Loading>.Instance.Connecting("二次元バーコードの表示に失敗しました");
				await UniTask.Delay(TimeSpan.FromSeconds(3.0));
				SingletonBehaviour<Loading>.Instance.DisConnect();
				await SendLog(EndType.QRUnable);
				PreExitProcess();
				GotoNextScene();
			}
		}).AddTo(this);
	}

	private bool IsCardOn()
	{
		if (GameUtility.IsKonastaOrNetcafeBehavior)
		{
			return ResidentFlow.CustomInput.GetMouseButtonDown(0);
		}
		if (GameDataManager.Account.IsCardless)
		{
			(int, string) cardTypeID = QRLogin.GetCardTypeID();
			if (cardTypeID.Item1 != -1 && !string.IsNullOrEmpty(cardTypeID.Item2))
			{
				_qrImage.gameObject.SetActive(value: false);
				return true;
			}
			return false;
		}
		return Bi2xInput.Instance.IsCardOn();
	}

	public async UniTask Open(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (IsOpen)
		{
			return;
		}
		_bgImage.sprite = (GameUtility.IsKonastaOrNetcafeBehavior ? _netcafeBg : (GameDataManager.Account.IsCardless ? _qrBg : _acBg));
		_authDialogCancellationTokenSource = new CancellationTokenSource();
		_countDownTimer.ZeroPaddingDisable = true;
		if (!_isInitialized)
		{
			this.UpdateAsObservable().Subscribe(async delegate
			{
				if (!_countDownTimer.IsEnd() && !_isDecided && IsOpen)
				{
					if (_qrImage.gameObject.activeSelf && Time.time - _time >= 480f)
					{
						_qrImage.gameObject.SetActive(value: false);
						QRLogin.End();
						_qrButton.gameObject.SetActive(value: true);
					}
					var (flag, endType) = IsPossibleContinueBreakTime();
					if (!flag)
					{
						_countDownTimer.SetPause(pause: true);
						_authDialogCancellationTokenSource.Cancel();
						AuthDialog_Close();
						_isDecided = true;
						IsAuthed = false;
						await _loadingBar.Play(GetEndMessage(endType), TimeSpan.FromSeconds(3.0), cancellationToken);
						cancellationToken.ThrowIfCancellationRequested();
						await SendLog(endType);
						PreExitProcess();
						GotoNextScene();
					}
					else if (!_isAuthDialogOpen && IsCardOn())
					{
						using CancellationTokenSource linkedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _authDialogCancellationTokenSource.Token);
						CancellationToken token = linkedCancellationTokenSource.Token;
						string cardID;
						AvsTypes.CardType cardType;
						if (GameUtility.IsKonastaOrNetcafeBehavior)
						{
							cardID = GameDataManager.Account.CardID;
							cardType = GameDataManager.Account.CardType;
						}
						else if (GameDataManager.Account.IsCardless)
						{
							(int, string) cardTypeID = QRLogin.GetCardTypeID();
							cardID = cardTypeID.Item2;
							cardType = (AvsTypes.CardType)cardTypeID.Item1;
							QRLogin.End();
						}
						else
						{
							cardID = Bi2xInput.Instance.GetCardID();
							cardType = Bi2xInput.Instance.GetCardType();
						}
						try
						{
							if (!(await AuthDialog_Open(cardID, cardType, token)) || _isDecided)
							{
								_qrButton.gameObject.SetActive(value: true);
								return;
							}
						}
						catch (OperationCanceledException ex) when (ex.CancellationToken == token)
						{
							if (_authDialogCancellationTokenSource.IsCancellationRequested)
							{
								Debug.Log("BreakTimeWindow.AuthDialogCancel");
								_qrButton.gameObject.SetActive(value: true);
								return;
							}
							Debug.Log("BreakTimeWindow.Cancel");
							_qrButton.gameObject.SetActive(value: true);
							throw;
						}
						_isDecided = true;
						IsAuthed = true;
						await SendLog(EndType.None);
						cancellationToken.ThrowIfCancellationRequested();
						await Close();
					}
				}
			}).AddTo(this);
			_isInitialized = true;
		}
		(from _ in this.UpdateAsObservable()
			where _countDownTimer.IsEnd()
			select _).First().Subscribe(async delegate
		{
			if (!_isDecided)
			{
				_isDecided = true;
				await SendLog(EndType.Timeout);
				PreExitProcess();
				GotoNextScene();
			}
		}).AddTo(_disposables);
		Observable.Interval(TimeSpan.FromSeconds(60.0)).Subscribe(async delegate
		{
			try
			{
				await RequestApiBase<KeepAlive, KeepAlive.ResultData>.Send(enableErrorScreenTransition: false);
			}
			catch
			{
			}
		}).AddTo(_disposables);
		_countDownTimer.MaxTime = 999;
		_countDownTimer.SetTimer(CabinetManager.Game.Data.BreakTimeLength * 60, disp: true);
		_countDownTimer.StartTimer();
		if (!GameDataManager.Account.IsCardless)
		{
			Bi2xInput.Instance.ICReaderClear();
			Bi2xInput.Instance.ICReaderOn();
		}
		CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
		canvasGroup.alpha = 0f;
		base.gameObject.SetActive(value: true);
		await canvasGroup.DOFade(1f, 0.1f).OnComplete(delegate
		{
			canvasGroup.alpha = 1f;
		}).ToUniTask(TweenCancelBehaviour.Complete, cancellationToken);
		_isDecided = false;
		IsOpen = true;
	}

	public async UniTask Close(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (IsOpen)
		{
			CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
			canvasGroup.alpha = 1f;
			await canvasGroup.DOFade(0f, 0.1f).OnComplete(delegate
			{
				canvasGroup.alpha = 0f;
			}).ToUniTask(TweenCancelBehaviour.Complete, cancellationToken);
			AuthDialog_Close();
			base.gameObject.SetActive(value: false);
			PreExitProcess();
			IsOpen = false;
		}
	}

	private void PreExitProcess()
	{
		Bi2xInput.Instance.ICReaderOff();
		_disposables.Clear();
		_authDialogCancellationTokenSource.Dispose();
	}

	private void GotoNextScene()
	{
		QRLogin.End();
		_isDecided = true;
		ResidentFlow.GetInstance().LoadScene("Gameover");
	}

	private async UniTask SendLog(EndType endType)
	{
		int num = CabinetManager.Game.Data.BreakTimeLength * 60;
		await GameUtility.ServerLog.Send("breaktime", $"{(int)((float)num - _countDownTimer.GetLeftTime())},{num},{(int)endType}");
	}

	private async UniTask<bool> AuthDialog_Open(string cardID, AvsTypes.CardType cardType, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (_isAuthDialogOpen)
		{
			return false;
		}
		try
		{
			_isAuthDialogOpen = true;
			SoundManager.PlaySe("SFX_SYS_EPASS1").Forget();
			SoundManager.PlayVoice("VOX_G01_SYSTEM3").Forget();
			await _loadingBar.Play(TextManager.KEY.ENTRY_PROCESSING, TimeSpan.FromSeconds(1.0), cancellationToken);
			cancellationToken.ThrowIfCancellationRequested();
			if (!IsUsedCardInThisPlay(cardID, cardType))
			{
				await _loadingBar.Play(TextManager.KEY.BREAKTIME_DIFFERENT_CARD, TimeSpan.FromSeconds(3.0), cancellationToken);
				return false;
			}
			SoundManager.PlaySe("SFX_SYS_EPASS2").Forget();
			SoundManager.PlayVoice("VOX_G01_SYSTEM5").Forget();
			await _loadingBar.Play(TextManager.KEY.ENTRY_CONFIRM, TimeSpan.FromSeconds(1.0), cancellationToken);
			cancellationToken.ThrowIfCancellationRequested();
			await _cardEntryManager.CreateTenkey(_bBinded: true, null, _countDownTimer);
			cancellationToken.ThrowIfCancellationRequested();
			_authDialogRoot.SetActive(value: true);
			await _cardEntryManager.CoCardEntryRequest();
			cancellationToken.ThrowIfCancellationRequested();
			if (_cardEntryManager.IsEnd() && !_cardEntryManager.IsErrorStopState)
			{
				_countDownTimer.SetPause(pause: true);
				return true;
			}
			if (_cardEntryManager.IsErrorStopState && _cardEntryManager.m_errorStopStateType == CardEntryManager.ErrorStopStateType.NetworkError)
			{
				await _loadingBar.Play(TextManager.KEY.ENTRY_NETWORK_ERROR, TimeSpan.FromSeconds(3.0), cancellationToken);
			}
			return false;
		}
		catch (OperationCanceledException)
		{
			Debug.Log("AuthDialog_Open.Canceled");
			return false;
		}
		finally
		{
			AuthDialog_Close();
		}
	}

	private void AuthDialog_Close()
	{
		if ((bool)_cardEntryManager)
		{
			_cardEntryManager.Clear();
		}
		if ((bool)_authDialogRoot)
		{
			_authDialogRoot.SetActive(value: false);
		}
		_isAuthDialogOpen = false;
		if ((bool)_loadingBar)
		{
			_loadingBar.SetDisp(isDisp: false);
		}
	}

	private (bool isPossible, EndType endType) IsPossibleContinueBreakTime()
	{
		if (!ShopCloseUtility.IsEntryAcceptable())
		{
			return (isPossible: false, endType: EndType.ShopClose);
		}
		if (EagCore.Instance.Status == STATUS.MAINTE)
		{
			return (isPossible: false, endType: EndType.ServerMaintenance);
		}
		return (isPossible: true, endType: EndType.None);
	}

	private string GetEndMessage(EndType endType)
	{
		return endType switch
		{
			EndType.None => "", 
			EndType.Timeout => "", 
			EndType.ShopClose => TextManager.Get(TextManager.KEY.SHOP_CLOSE_ENFORCE), 
			EndType.ServerMaintenance => TextManager.Get(TextManager.KEY.GAMEOVER_REASON_MAINTENANCE), 
			_ => "", 
		};
	}

	private bool IsUsedCardInThisPlay(string cardID, AvsTypes.CardType cardType)
	{
		string cardID2 = GameDataManager.Account.CardID;
		AvsTypes.CardType cardType2 = GameDataManager.Account.CardType;
		if (cardID2 == cardID)
		{
			return cardType2 == cardType;
		}
		return false;
	}
}
