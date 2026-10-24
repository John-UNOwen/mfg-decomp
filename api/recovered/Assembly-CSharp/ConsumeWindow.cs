using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using EagDef;
using GameContents;
using KAMUNITY;
using MFG.GameData;
using TMPro;
using TestModeSkeleton;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ConsumeWindow : PopupWindow
{
	public enum EndType
	{
		None,
		Success,
		NetworkError,
		Error,
		Cancel
	}

	[SerializeField]
	private CreditConsumeButton m_creditButton;

	[SerializeField]
	private CreditConsumeButton m_premiumCreditButton;

	[SerializeField]
	private PaseliConsumeButton m_paseliButton;

	[SerializeField]
	private PaseliConsumeButton m_premiumPaseliButton;

	[SerializeField]
	private FreeConsumeButton m_freePlayButton;

	[SerializeField]
	private ExtraGachaTicketConsumeButton m_extraGachaTicketButton;

	[SerializeField]
	private PaseliConsumeButton _jandriPurchaseButton;

	[SerializeField]
	private GameObject m_titleRoot;

	[SerializeField]
	private GameObject _jandriPurchaseMenuRoot;

	[SerializeField]
	private TextMeshProUGUI m_title;

	[SerializeField]
	private TextMeshProUGUI m_message;

	[SerializeField]
	private Button m_cancel;

	[SerializeField]
	private TextMeshProUGUI m_cancelText;

	[SerializeField]
	private CanvasGroup m_group;

	[SerializeField]
	private GraphicRaycaster Raycaster;

	[SerializeField]
	private GameObject m_jandriEnable;

	[SerializeField]
	private Button _jandriHelpButton;

	[SerializeField]
	private BaseMenuWindow _jandriHelpWindow;

	[SerializeField]
	private GameObject _entryAttention;

	[SerializeField]
	private Button _premiumDetailButton;

	[SerializeField]
	private BaseMenuWindow _premiumDetailWindow;

	[SerializeField]
	private GameObject _waitBlack;

	[SerializeField]
	private GameObject _payAttention;

	[SerializeField]
	private GameObject _gachaAttention;

	[SerializeField]
	private TextMeshProUGUI _gachaAttentionText;

	[SerializeField]
	private GameObject _modeSelectAttention;

	private UnityAction<bool, CONSUME_MODE, EndType> m_onEnd;

	private string m_contentsId = "";

	private CountDownTimer m_timer;

	private bool _enableJandriPurchaseMenu;

	private bool _isEntryAttentionOpen;

	private List<AvsEa3CoinTypes.ea3_eacoin_pricelist_article> _ppexItemInfos = new List<AvsEa3CoinTypes.ea3_eacoin_pricelist_article>();

	public async UniTask Set(string contents_id, GAME_TYPE gameType, PRICE_TYPE priceType, int num, GachaSeriesName gacha, string message, UnityAction<bool, CONSUME_MODE, EndType> OnEnd, GameObject tenkeyWindow, bool resetTimer = true, bool isRenewAccessDate = false, bool isJandriActiveIconDisp = false, bool enableJandriPurchaseMenu = false, bool isEntry = false, bool isFreePlayEnable = false)
	{
		if (_Set(contents_id, gameType, priceType, num, gacha, message, OnEnd, tenkeyWindow, resetTimer, isRenewAccessDate, isJandriActiveIconDisp, enableJandriPurchaseMenu, isEntry, isFreePlayEnable))
		{
			_waitBlack.SetActive(value: true);
			m_titleRoot.SetActive(value: false);
			await UniTask.Delay(TimeSpan.FromSeconds(0.5));
			_waitBlack.SetActive(value: false);
		}
	}

	private bool _Set(string contents_id, GAME_TYPE gameType, PRICE_TYPE priceType, int num, GachaSeriesName gacha, string message, UnityAction<bool, CONSUME_MODE, EndType> OnEnd, GameObject tenkeyWindow, bool resetTimer, bool isRenewAccessDate, bool isJandriActiveIconDisp, bool enableJandriPurchaseMenu, bool isEntry, bool isFreePlayEnable = false)
	{
		m_contentsId = contents_id;
		_enableJandriPurchaseMenu = enableJandriPurchaseMenu;
		_ppexItemInfos.Clear();
		_isEntryAttentionOpen = isEntry;
		_entryAttention.SetActive(_isEntryAttentionOpen);
		if (_enableJandriPurchaseMenu && !GameUtility.IsKamunityBootEnable())
		{
			_enableJandriPurchaseMenu = false;
		}
		if (_enableJandriPurchaseMenu && GameDataManager.CoopData.IsJanDriEnable())
		{
			_enableJandriPurchaseMenu = false;
		}
		if (_enableJandriPurchaseMenu && !PaseliUtility.PPEX.IsEnablePurchaseItem)
		{
			_enableJandriPurchaseMenu = false;
		}
		if (_enableJandriPurchaseMenu)
		{
			_ppexItemInfos = PaseliUtility.PPEX.RefleshPrice();
			string jandriArticleID = EaCoin.GetPathData(GAME_MODE.JANDRI).ppexArticleID;
			if (!_ppexItemInfos.Exists((AvsEa3CoinTypes.ea3_eacoin_pricelist_article x) => x.atid == jandriArticleID))
			{
				_enableJandriPurchaseMenu = false;
			}
		}
		_jandriPurchaseMenuRoot.SetActive(_enableJandriPurchaseMenu);
		var (flag, window) = IsEnableConsume(gameType);
		if (!flag && !isFreePlayEnable)
		{
			StartCoroutine(CoExit(window, OnEnd, EndType.Error));
			return false;
		}
		GAME_MODE gameMode = GameUtility.GetGameMode(gameType, priceType);
		m_title.text = GameUtility.GetGameModeStr(gameMode);
		_payAttention.SetActive(GameUtility.IsKonastaOrNetcafeBehavior || gameType == GAME_TYPE.MUSICGACHA);
		_modeSelectAttention.SetActive(GameUtility.IsKonastaOrNetcafeBehavior && (gameType == GAME_TYPE.START || gameType == GAME_TYPE.CONTINUE));
		_gachaAttention.SetActive((GameUtility.IsKonastaOrNetcafeBehavior && (gameType == GAME_TYPE.GACHA || gameType == GAME_TYPE.GACHA5)) || gameType == GAME_TYPE.MUSICGACHA);
		if (_gachaAttention.activeSelf)
		{
			try
			{
				GachaSeriesType seriesType = GameDataManager.GachaInfo.GachaSeriesInfosFromServer.Find((GachaSeriesServerInfo x) => x.GachaSeriesName == gacha).SeriesType;
				_gachaAttentionText.text = seriesType switch
				{
					GachaSeriesType.Unlock => "SPガチャ", 
					GachaSeriesType.Music => "リーチソング＆ボイスガチャ", 
					_ => "ガールズカットインガチャ", 
				} + "×" + ((gameType == GAME_TYPE.GACHA5) ? "5" : "1") + "を購入してガチャを引く";
			}
			catch
			{
				_gachaAttentionText.text = "";
			}
		}
		if (m_jandriEnable != null && isJandriActiveIconDisp)
		{
			m_jandriEnable.SetActive(GameDataManager.CoopData.IsJanDriEnable());
		}
		if (!isFreePlayEnable)
		{
			m_creditButton.Set(gameMode, num, delegate
			{
				SetEnableRaycaster(enable: false);
			}, delegate(bool isOk, EndType endType)
			{
				Close(isOk, gameMode, CONSUME_TYPE.CREDIT, num, endType);
			}, isRenewAccessDate);
			m_paseliButton.Set(gameMode, num, delegate
			{
				SetEnableRaycaster(enable: false);
			}, delegate(bool isOk, EndType endType)
			{
				Close(isOk, gameMode, CONSUME_TYPE.PASELI, num, endType);
			}, delegate
			{
				SetEnableRaycaster(enable: true);
			}, tenkeyWindow, isRenewAccessDate);
		}
		else
		{
			m_freePlayButton.Set(delegate
			{
				SetEnableRaycaster(enable: false);
			}, delegate(bool isOk, EndType endType)
			{
				Close(isOk, gameMode, CONSUME_TYPE.FREE, num, endType);
			});
		}
		GAME_MODE premiumGameMode = ((gameMode == GAME_MODE.CONTINUE) ? GAME_MODE.PREMIUMCONTINUE : ((gameMode == GAME_MODE.START) ? GAME_MODE.PREMIUMSTART : gameMode));
		bool flag2 = GameDataManager.IsPremiumStartEnable() && (premiumGameMode == GAME_MODE.PREMIUMSTART || premiumGameMode == GAME_MODE.PREMIUMCONTINUE);
		if (flag2)
		{
			m_premiumCreditButton.Set(premiumGameMode, num, delegate
			{
				SetEnableRaycaster(enable: false);
			}, delegate(bool isOk, EndType endType)
			{
				Close(isOk, premiumGameMode, CONSUME_TYPE.CREDIT, num, endType);
			}, isRenewAccessDate, isFreePlayEnable);
			m_premiumPaseliButton.Set(premiumGameMode, num, delegate
			{
				SetEnableRaycaster(enable: false);
			}, delegate(bool isOk, EndType endType)
			{
				Close(isOk, premiumGameMode, CONSUME_TYPE.PASELI, num, endType);
			}, delegate
			{
				SetEnableRaycaster(enable: true);
			}, tenkeyWindow, isRenewAccessDate);
			_premiumDetailButton.OnClickAsObservableSafety().Subscribe(delegate
			{
				_premiumDetailWindow.Open();
			}).AddTo(this);
		}
		_jandriPurchaseButton.Set(GAME_MODE.JANDRI, 1, delegate
		{
			SetEnableRaycaster(enable: false);
		}, delegate(bool isOk, EndType endType)
		{
			Close(isOk, GAME_MODE.JANDRI, CONSUME_TYPE.PASELI, 1, endType);
		}, delegate
		{
			SetEnableRaycaster(enable: true);
		}, tenkeyWindow, isRenewAccessDate);
		m_extraGachaTicketButton.Set(gameMode, gacha, delegate
		{
			SetEnableRaycaster(enable: false);
		}, delegate(bool isOk, EndType endType)
		{
			Close(isOk, gameMode, CONSUME_TYPE.TICKET, num, endType);
		}, isRenewAccessDate);
		_jandriHelpButton.OnClickAsObservableSafety().Subscribe(delegate
		{
			_jandriHelpWindow.Open();
		}).AddTo(this);
		m_message.text = message;
		m_cancel.onClick.AddListener(delegate
		{
			SoundManager.PlaySe("SFX_SYS_CANCEL1").Forget();
			Close(is_consume: false, GAME_MODE.NONE, CONSUME_TYPE.NONE, 0, EndType.Cancel);
		});
		m_cancelText.text = TextManager.Get(TextManager.KEY.CONSUME_CANCEL);
		m_onEnd = OnEnd;
		m_creditButton.gameObject.SetActive(!isFreePlayEnable && flag && gameMode != GAME_MODE.MUSICGACHA);
		m_freePlayButton.gameObject.SetActive(isFreePlayEnable);
		m_paseliButton.SetActive(PaseliUtility.IsEnablePaseli && !isFreePlayEnable && flag);
		m_premiumCreditButton.gameObject.SetActive(!GameDataManager.Account.IsGuestUser && flag2 && flag);
		m_premiumPaseliButton.SetActive(PaseliUtility.IsEnablePaseli && flag2 && flag);
		_premiumDetailButton.gameObject.SetActive(m_premiumCreditButton.gameObject.activeSelf || m_premiumPaseliButton.gameObject.activeSelf);
		if (GameUtility.IsKonastaOrNetcafeBehavior)
		{
			m_creditButton.gameObject.SetActive(value: false);
			m_premiumCreditButton.gameObject.SetActive(value: false);
		}
		m_timer = ResidentFlow.GetInstance().GetTimer();
		if (resetTimer)
		{
			m_timer.SetTimer(GameDef.CONSUME_TIME, disp: true);
			m_timer.StartTimer();
		}
		PaseliUtility.IsDispPaseliBalance = true;
		StartCoroutine(CoMain(isFreePlayEnable, gameMode, num));
		return true;
	}

	private IEnumerator CoMain(bool isFreePlayEnable, GAME_MODE gameMode, int num)
	{
		yield return new WaitUntil(() => m_timer.IsEnd());
		if (isFreePlayEnable)
		{
			Close(is_consume: true, gameMode, CONSUME_TYPE.FREE, num, EndType.Success);
		}
		else
		{
			Close(is_consume: false, GAME_MODE.NONE, CONSUME_TYPE.NONE, 0, EndType.Cancel);
		}
	}

	private void Close(bool is_consume, GAME_MODE mode, CONSUME_TYPE type, int num, EndType endType)
	{
		SetEnableRaycaster(enable: false);
		StartCoroutine(CoClose(is_consume, mode, type, num, endType));
	}

	private void SetEnableRaycaster(bool enable)
	{
		m_timer.SetPause(!enable);
		if (Raycaster != null)
		{
			Raycaster.enabled = enable;
		}
	}

	private IEnumerator CoClose(bool is_consume, GAME_MODE mode, CONSUME_TYPE type, int num, EndType endType)
	{
		if (!is_consume)
		{
			yield return null;
		}
		else
		{
			if (!EaCoin.GetPathData(mode).IsPPEXItem && type == CONSUME_TYPE.PASELI)
			{
				CabinetManager.VirtualCoin.ConsumeServiceRemain();
			}
			if (GameUtility.IsKamunityBootEnable())
			{
				BackupDataManager.Instance.TCSBookKeeping().AddCount(m_contentsId, GameUtility.GetGameType(mode), (uint)num);
			}
			yield return new WaitForSecondsRealtime(0.3f);
		}
		PaseliUtility.IsDispPaseliBalance = false;
		m_onEnd?.Invoke(is_consume, GameUtility.GetConsumeMode(mode, type), endType);
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private (bool isEnable, UnityEngine.Object errorDialogWindow) IsEnableConsume(GAME_TYPE gameType)
	{
		if (!GameUtility.IsKamunityBootEnable())
		{
			return (isEnable: true, errorDialogWindow: null);
		}
		bool flag = EagCore.Ea3GetPpStatus() == AvsTypes.EA3_PP_STATUS.AVAILABLE;
		bool flag2 = EagCore.Instance.Status == STATUS.MAINTE;
		if (!GameUtility.IsReservedGameOver)
		{
			if (GameUtility.IsKonastaOrNetcafeBehavior && !PaseliUtility.IsEnablePaseli)
			{
				GameUtility.IsReservedGameOver = true;
			}
			if (!flag)
			{
				GameUtility.IsReservedGameOver = true;
				GameUtility.ReasonGameOver = TextManager.Get(TextManager.KEY.GAMEOVER_REASON_PP_NOTAVAILABLE);
			}
			if (flag2)
			{
				GameUtility.IsReservedGameOver = true;
				GameUtility.ReasonGameOver = TextManager.Get(TextManager.KEY.GAMEOVER_REASON_MAINTENANCE);
			}
		}
		if (GameUtility.IsReservedGameOver)
		{
			if (PaseliUtility.IsCheckinProcessFinished && !PaseliUtility.IsSessionOpened())
			{
				PaseliWindow paseliWindow = DialogManager.Instance.CreatePaseliDialog(base.transform.parent);
				string message = string.Format(TextManager.Get(TextManager.KEY.REAUTH_DISABLE), PaseliUtility.GetLastNotch());
				paseliWindow.Open(PaseliWindow.COLOR.RED, message, 5f);
				return (isEnable: false, errorDialogWindow: paseliWindow);
			}
			DialogWindow dialogWindow = DialogManager.Instance.CreateNormalDialog(base.transform.parent);
			string message2 = ((GameUtility.IsKonastaOrNetcafeBehavior && !PaseliUtility.IsEnablePaseli) ? "<size=24>このe-amusement passはPASELIを利用できません。<br>PASELI利用設定をご確認ください。</size>" : string.Format(TextManager.Get(TextManager.KEY.RESERVE_GAMEOVER), GameUtility.ReasonGameOver));
			dialogWindow.SetMessage(message2);
			dialogWindow.Open(5f);
			return (isEnable: false, errorDialogWindow: dialogWindow);
		}
		return (isEnable: true, errorDialogWindow: null);
	}

	private IEnumerator CoExit(UnityEngine.Object window, UnityAction<bool, CONSUME_MODE, EndType> onEnd, EndType endType)
	{
		m_group.alpha = 0f;
		m_group.interactable = false;
		yield return new WaitWhile(() => window != null);
		onEnd?.Invoke(arg0: false, CONSUME_MODE.NONE, endType);
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void OnDestroy()
	{
		if (PaseliUtility.IsDispPaseliBalance)
		{
			PaseliUtility.IsDispPaseliBalance = false;
		}
	}
}
