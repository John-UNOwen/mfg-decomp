using System;
using System.Reflection;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameContents;
using KAMUNITY;
using MFG.GameData;
using MFG.Sytem.Utilities;
using MFG.Types;
using TMPro;
using TestModeSkeleton;
using TestModeSkeleton.CabinetDef;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class TitleFlow : BaseTitleFlow
{
	[SerializeField]
	private TextMeshProUGUI m_softIDText;

	[SerializeField]
	private GameObject m_phoneIcon;

	[SerializeField]
	private GameObject m_paseliIcon;

	[SerializeField]
	private GameObject m_paseliNGIcon;

	[SerializeField]
	private Button m_licenseButton;

	[SerializeField]
	private Button m_licenseCloseButton;

	[SerializeField]
	private Button m_licenseClosePanel;

	[SerializeField]
	private GameObject m_licenseWindow;

	[SerializeField]
	private Button m_priceButton;

	[SerializeField]
	private GameObject m_priceTable;

	[SerializeField]
	private GameObject m_insertCoinText;

	[SerializeField]
	private GameObject m_proStayBanner;

	[SerializeField]
	private GameObject m_paseliPointCampaignBanner;

	[SerializeField]
	private GameObject m_bannerInstanceRoot;

	[SerializeField]
	private GameObject m_bgMovie;

	[SerializeField]
	private GameObject m_bgMovieNewChara;

	[SerializeField]
	private RawImage m_screen;

	[SerializeField]
	private Button m_btTitleWindow;

	[SerializeField]
	private GameObject m_qrLoginIcon;

	[SerializeField]
	private GameObject m_advertisement;

	private float _priceTableDispTime;

	private float _licenseWindowDispTime;

	private bool _isPriceTableOpen;

	private bool _isLicenseWindowOpen;

	private bool _loginRequest;

	private float _transitionToTitleDemoTimer;

	private bool _isBgMovieFinished;

	private bool _isNewCharaMovieFinished;

	private bool _isScreenTouched;

	private float _appTerminateWaitTimer;

	private bool IsAdvertisementStart;

	private float TitleDisplayTime = 60f;

	private float AddAdvertisementDisplayTime = 20f;

	private bool IsPriceButtonAvailable
	{
		get
		{
			if (!GameUtility.IsFreePlay())
			{
				return !GameUtility.IsKonastaOrNetcafeBehavior;
			}
			return false;
		}
	}

	private bool IsAddAdvertisementEnable
	{
		get
		{
			if (GameDataManager.IsToukaCpuForceEnable())
			{
				return !DebugFlag.IsActive(DebugFlag.Flag.SkipToIntroductionMovie);
			}
			return false;
		}
	}

	private bool BgMovie_IsFinished
	{
		get
		{
			if (!_isBgMovieFinished)
			{
				return DebugFlag.IsActive(DebugFlag.Flag.SkipDemoInterval);
			}
			return true;
		}
		set
		{
			_isBgMovieFinished = value;
		}
	}

	private bool NewCharaMovie_IsFinished
	{
		get
		{
			if (!_isNewCharaMovieFinished)
			{
				return DebugFlag.IsActive(DebugFlag.Flag.SkipToIntroductionMovie);
			}
			return true;
		}
		set
		{
			_isNewCharaMovieFinished = value;
		}
	}

	protected override async UniTask OnInitialize(CancellationToken cancellationToken)
	{
		TitleDemoUtility.GameEntryContext_ActivateIfGameEntryAcceptable();
		if (!GotoParticipationErrorIfNeed())
		{
			SoundManager.SetDemoVolume();
			m_licenseCloseButton.gameObject.SetActive(value: true);
			m_priceButton.gameObject.SetActive(IsPriceButtonAvailable);
			ResidentFlow.GetInstance().GetMahjongActorNoticeManager().SetLowerDisp(isLowerDisp: true);
			m_paseliPointCampaignBanner.SetActive(GameDataManager.System.GameControlInfo.EventStatus.IsActive(GameEventType.PaseliPointCampaign));
			if (SystemConfigGameData.CabinetType == CabinetType.UKS && SystemConfigGameData.IsDualMonitor)
			{
				await SubMonitorFlow.GetInstance().CreateCompeBanner();
			}
			await CompetitionUtility.TitleBanner_Instantiate(m_bannerInstanceRoot);
			m_licenseButton.OnClickAsObservable().Subscribe(delegate
			{
				LicenseWindow_Open();
			}).AddTo(this);
			m_licenseCloseButton.OnClickAsObservable().Subscribe(delegate
			{
				LicenseWindow_Close();
			}).AddTo(this);
			m_licenseClosePanel.OnClickAsObservable().Subscribe(delegate
			{
				LicenseWindow_Close();
			}).AddTo(this);
			m_priceButton.OnClickAsObservable().Subscribe(delegate
			{
				PriceTable_Open();
			}).AddTo(this);
			m_btTitleWindow.OnClickAsObservable().Subscribe(delegate
			{
				_isScreenTouched = true;
			}).AddTo(this);
			SoftIDText_Reflesh();
			BgMovie_Init();
			NewCharaMovie_Init();
			SingletonBehaviour<OnlineUpdateMessage>.Instance.SetDispType(OnlineUpdateMessage.DispType.Simple);
		}
	}

	protected override async UniTask OnExecute(CancellationToken cancellationToken)
	{
		try
		{
			SoundManager.PlayBgm("BGM_TITLE").Forget();
			await UniTask.Delay(TimeSpan.FromSeconds(2.0), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
			if (!_isNextSceneDecided)
			{
				VoiceManager.PlayVoiceTitle();
			}
		}
		catch (OperationCanceledException)
		{
			Debug.Log(GetType().Name + MethodBase.GetCurrentMethod().Name + ": OperationCanceledException Catched.");
		}
	}

	protected override void OnUpdate()
	{
		if (!UpdateCommonProcess())
		{
			return;
		}
		_transitionToTitleDemoTimer += Time.deltaTime;
		PaseliIcon_Update();
		if ((GameUtility.IsKonastaOrNetcafeBehavior && AppTerminateIfShopClose()) || GotoLoginIfPossible())
		{
			return;
		}
		if (!GameUtility.IsKonastaOrNetcafeBehavior)
		{
			if (GotoTitleDemoIfPossible())
			{
				return;
			}
		}
		else if (GotoLogoIfPossible())
		{
			return;
		}
		StartAddAdvertisementIfPossible().Forget();
		StartCharaIntroductionMovieIfPossible();
		PriceTable_Update();
		LicenseWindow_Update();
	}

	private bool AppTerminateIfShopClose()
	{
		if (!GameUtility.IsKonastaOrNetcafeBehavior)
		{
			return false;
		}
		GameSettings.ShopClosePhase shopClosePhase = ShopCloseUtility.GetShopClosePhase();
		if (shopClosePhase == GameSettings.ShopClosePhase.Enforce || shopClosePhase == GameSettings.ShopClosePhase.Final)
		{
			_appTerminateWaitTimer += Time.deltaTime;
			if (_appTerminateWaitTimer >= 60f || ResidentFlow.CustomInput.GetMouseButtonDown(0))
			{
				GotoNextScene("ScopeClose");
			}
			return true;
		}
		return false;
	}

	private void SoftIDText_Reflesh()
	{
		string softID = SystemUtility.Instance.SoftID;
		m_softIDText.text = softID;
	}

	private void PaseliIcon_Update()
	{
		if (GameUtility.IsKamunityBootEnable())
		{
			bool flag = AvsEa3Coin.Ea3EacoinGetStatus() == AvsTypes.EA3_EACOIN_STATUS.IGNORE;
			bool flag2 = PaseliUtility.IsEnable();
			bool active = GameUtility.IsNetworkActive();
			bool active2 = GameDataManager.IsQRLoginEnable() && QRLogin.GetStatus() >= QRLogin.STATUS.EAX_SPPASS_ERROR_CLOSE;
			m_paseliIcon.SetActive(!flag && flag2);
			m_paseliNGIcon.SetActive(!flag && !flag2);
			m_phoneIcon.SetActive(active);
			m_qrLoginIcon.SetActive(active2);
		}
	}

	private void LicenseWindow_Open()
	{
		_isLicenseWindowOpen = true;
		_licenseWindowDispTime = 0f;
		m_licenseWindow.SetActive(value: true);
		m_licenseButton.gameObject.SetActive(value: false);
		m_priceButton.gameObject.SetActive(value: false);
		ResidentFlow.GetInstance().GetMahjongActorNoticeManager().SetLowerDisp(isLowerDisp: false);
		SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
	}

	private void LicenseWindow_Close(bool enableCloseSE = true)
	{
		_isLicenseWindowOpen = false;
		m_licenseWindow.SetActive(value: false);
		m_licenseButton.gameObject.SetActive(value: true);
		m_licenseWindow.transform.Find("Scroll View").GetComponent<ScrollRect>().verticalNormalizedPosition = 1f;
		m_priceButton.gameObject.SetActive(IsPriceButtonAvailable);
		ResidentFlow.GetInstance().GetMahjongActorNoticeManager().SetLowerDisp(isLowerDisp: true);
		if (enableCloseSE)
		{
			SoundManager.PlaySe("SFX_SYS_CANCEL1").Forget();
		}
	}

	private void LicenseWindow_Update()
	{
		if (_isLicenseWindowOpen)
		{
			if (ResidentFlow.CustomInput.GetMouseButtonDown(0))
			{
				_licenseWindowDispTime = 0f;
			}
			_licenseWindowDispTime += Time.deltaTime;
			if (_licenseWindowDispTime > 10f)
			{
				LicenseWindow_Close(enableCloseSE: false);
			}
		}
	}

	private async void PriceTable_Open()
	{
		try
		{
			await PaseliUtility.RefleshPrice(Fraction.One, isFetchExternalBasicRateInfo: true);
			SetModePriceDisp();
			_isPriceTableOpen = true;
			m_priceButton.gameObject.SetActive(value: false);
			m_licenseButton.gameObject.SetActive(value: false);
			m_priceTable.SetActive(value: true);
			ResidentFlow.GetInstance().GetMahjongActorNoticeManager().SetLowerDisp(isLowerDisp: false);
			_priceTableDispTime = 0f;
			SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
		}
		catch
		{
			Debug.Log("価格情報の更新に失敗");
		}
	}

	private void PriceTable_Close(bool enableCloseSE = true)
	{
		if (enableCloseSE)
		{
			SoundManager.PlaySe("SFX_SYS_CANCEL1").Forget();
		}
		_isPriceTableOpen = false;
		m_priceTable.SetActive(value: false);
		m_priceButton.gameObject.SetActive(IsPriceButtonAvailable);
		m_licenseButton.gameObject.SetActive(value: true);
		ResidentFlow.GetInstance().GetMahjongActorNoticeManager().SetLowerDisp(isLowerDisp: true);
	}

	private void PriceTable_Update()
	{
		if (_isPriceTableOpen)
		{
			_priceTableDispTime += Time.deltaTime;
			if (_priceTableDispTime > 10f)
			{
				_isPriceTableOpen = false;
				m_priceButton.gameObject.SetActive(IsPriceButtonAvailable);
				m_licenseButton.gameObject.SetActive(value: true);
				m_priceTable.SetActive(value: false);
			}
		}
	}

	protected override bool GotoLoginIfPossible()
	{
		if (_isNextSceneDecided)
		{
			return true;
		}
		if (TitleDemoUtility.IsGameEntryAcceptable() && GameUtility.IsKonastaOrNetcafeBehavior && BgMovie_IsFinished)
		{
			SoundManager.StopBgmWithFadeout().Forget();
			SoundManager.StopVoiceAllWithFadeout().Forget();
			GotoNextScene("ScopeClose");
			return true;
		}
		if ((_loginRequest || (TitleDemoUtility.IsGameEntryAcceptable() && (_isScreenTouched || TitleDemoUtility.IsGameEntryActionDetectedExceptTouch(_isCoinInserted)))) && !_isNextSceneDecided)
		{
			_loginRequest = false;
			m_insertCoinText.SetActive(value: false);
			TitleDemoUtility.OnPreEnterLoginFlow();
			GotoNextScene("Login");
			return true;
		}
		return false;
	}

	private async UniTask StartAddAdvertisementIfPossible()
	{
		if (IsAddAdvertisementEnable)
		{
			float num = TitleDisplayTime;
			if (DebugFlag.IsActive(DebugFlag.Flag.SkipToAddAdvertisement))
			{
				num = 5f;
			}
			if (_transitionToTitleDemoTimer >= num && !_isNextSceneDecided && !NewCharaMovie_IsFinished)
			{
				NewCharaMovie_Play();
			}
		}
	}

	private void StartCharaIntroductionMovieIfPossible()
	{
		if (IsAddAdvertisementEnable)
		{
			if (NewCharaMovie_IsFinished && !_isNextSceneDecided && !BgMovie_IsFinished)
			{
				BgMovie_Play();
			}
			return;
		}
		float num = TitleDisplayTime;
		if (DebugFlag.IsActive(DebugFlag.Flag.SkipToIntroductionMovie))
		{
			num = 0f;
		}
		if (_transitionToTitleDemoTimer >= num && !_isNextSceneDecided && !BgMovie_IsFinished)
		{
			BgMovie_Play();
		}
	}

	private bool GotoTitleDemoIfPossible()
	{
		if (BgMovie_IsFinished && !_isNextSceneDecided)
		{
			SoundManager.StopBgmWithFadeout().Forget();
			SoundManager.StopVoiceAllWithFadeout().Forget();
			ResidentFlow.GetInstance().GetMahjongActorNoticeManager().SetLowerDisp(isLowerDisp: false);
			GotoNextScene("TitleDemo");
			return true;
		}
		return false;
	}

	private bool GotoLogoIfPossible()
	{
		if (BgMovie_IsFinished && !_isNextSceneDecided)
		{
			ResidentFlow.GetInstance().GetMahjongActorNoticeManager().SetLowerDisp(isLowerDisp: false);
			GotoNextScene("Logo");
			return true;
		}
		return false;
	}

	public void ScreenTouch()
	{
		if (_isNextSceneDecided || _isLicenseWindowOpen)
		{
			return;
		}
		if (_isPriceTableOpen)
		{
			PriceTable_Close();
		}
		else
		{
			if (!TitleDemoUtility.IsGameEntryAcceptable())
			{
				return;
			}
			if (GameUtility.IsKonastaOrNetcafeBehavior)
			{
				if (TitleDemoUtility.IsTouchSkipActionDetected())
				{
					_loginRequest = true;
				}
			}
			else if (TitleDemoUtility.IsGameEntryActionDetected(_isCoinInserted))
			{
				_loginRequest = true;
			}
		}
	}

	private void SetModePriceDisp()
	{
		bool flag = PaseliUtility.IsEnable();
		Transform transform = m_priceTable.transform.Find("Window").Find("Window").Find("Base")
			.Find("Contents")
			.Find("Price");
		transform.Find("Index/Prices/Paseli").gameObject.SetActive(flag);
		SetModePriceDisp(GAME_MODE.START, transform.Find("NormalPlay"));
		SetModePriceDisp(GAME_MODE.PREMIUMSTART, transform.Find("PremiumPlay"));
		SetModePriceDisp(GAME_MODE.GACHA, transform.Find("Gacha"));
		SetModePriceDisp(GAME_MODE.GACHA5, transform.Find("Gacha5"));
		SetModePriceDisp(GAME_MODE.MUSICGACHA, transform.Find("MusicGacha"));
		bool active = CabinetManager.Game.Data.ContinueMenu == GameOptionsDef.ContinueOnOff.On;
		m_priceTable.transform.Find("Window/Window/Base/Contents/Continue").gameObject.SetActive(active);
		Transform transform2 = m_priceTable.transform.Find("Window/Window/Base/Contents/Continue/Table");
		SetModeScore(TakuType.Tonpu, transform2.Find("4Play/Tensu/N3").GetComponent<TextMeshProUGUI>(), transform2.Find("4Play/Tensu/N2").GetComponent<TextMeshProUGUI>(), transform2.Find("4Play/Tensu/N1").GetComponent<TextMeshProUGUI>());
		SetModeScore(TakuType.Sanma, transform2.Find("3Play/Tensu/N3").GetComponent<TextMeshProUGUI>(), transform2.Find("3Play/Tensu/N2").GetComponent<TextMeshProUGUI>(), transform2.Find("3Play/Tensu/N1").GetComponent<TextMeshProUGUI>());
		transform2.Find("Frame/L2").gameObject.SetActive(flag);
		transform2.Find("Frame/R2").gameObject.SetActive(flag);
		transform2.Find("Frame/Text/4PASELI").gameObject.SetActive(flag);
		transform2.Find("Frame/Text/3PASELI").gameObject.SetActive(flag);
		transform2.Find("3Play/PASELI/N1").gameObject.SetActive(flag);
		transform2.Find("4Play/PASELI/N1").gameObject.SetActive(flag);
		SetX(transform2.Find("Frame/Text/4Credit"), -220 + ((!flag) ? 73 : 0));
		SetX(transform2.Find("Frame/Text/3Credit"), 230 + ((!flag) ? 73 : 0));
		SetX(transform2.Find("4Play/CREDIT/N1"), -220 + ((!flag) ? 73 : 0));
		SetX(transform2.Find("4Play/CREDIT/N2"), -220 + ((!flag) ? 73 : 0));
		SetX(transform2.Find("4Play/CREDIT/N3"), -220 + ((!flag) ? 73 : 0));
		SetX(transform2.Find("3Play/CREDIT/N1"), -220 + ((!flag) ? 73 : 0));
		SetX(transform2.Find("3Play/CREDIT/N2"), -220 + ((!flag) ? 73 : 0));
		SetX(transform2.Find("3Play/CREDIT/N3"), -220 + ((!flag) ? 73 : 0));
		SetContinuePriceDisp(flag, transform2.Find("4Play/CREDIT/N3").GetComponent<TextMeshProUGUI>(), transform2.Find("4Play/PASELI/N3").GetComponent<TextMeshProUGUI>(), transform2.Find("4Play/CREDIT/N2").GetComponent<TextMeshProUGUI>(), transform2.Find("4Play/PASELI/N2").GetComponent<TextMeshProUGUI>());
		SetContinuePriceDisp(flag, transform2.Find("3Play/CREDIT/N3").GetComponent<TextMeshProUGUI>(), transform2.Find("3Play/PASELI/N3").GetComponent<TextMeshProUGUI>(), transform2.Find("3Play/CREDIT/N2").GetComponent<TextMeshProUGUI>(), transform2.Find("3Play/PASELI/N2").GetComponent<TextMeshProUGUI>());
		static void SetX(Transform t, int x)
		{
			RectTransform obj = t as RectTransform;
			Vector2 anchoredPosition = obj.anchoredPosition;
			anchoredPosition.x = x;
			obj.anchoredPosition = anchoredPosition;
		}
	}

	private void SetModeScore(TakuType takuType, TextMeshProUGUI lowerScoreText, TextMeshProUGUI middleScoreText, TextMeshProUGUI upperScoreText)
	{
		(int lowerScore, int upperScore) boundaryScore = GameUtility.ContinuePriceUtility.GetBoundaryScore(takuType, MFG.Types.EventType.None);
		int item = boundaryScore.lowerScore;
		int item2 = boundaryScore.upperScore;
		lowerScoreText.text = $"～{item:#,0}点";
		middleScoreText.text = $"{item:#,0}点～";
		upperScoreText.text = $"{item2:#,0}点～";
	}

	private void SetModePriceDisp(GAME_MODE mode, Transform table)
	{
		if (mode == GAME_MODE.PREMIUMSTART)
		{
			table.gameObject.SetActive(GameDataManager.IsPremiumStartEnable());
		}
		if (mode == GAME_MODE.MUSICGACHA)
		{
			if (!GameDataManager.IsReachSongVoiceGachaEnable())
			{
				table.gameObject.SetActive(value: false);
				return;
			}
			table.gameObject.SetActive(value: true);
		}
		bool flag = PaseliUtility.IsEnable();
		Transform obj = table.Find("Prices/Paseli");
		TextMeshProUGUI component = table.Find("Prices/Credit/Text").GetComponent<TextMeshProUGUI>();
		TextMeshProUGUI component2 = table.Find("Prices/Paseli/Text").GetComponent<TextMeshProUGUI>();
		int price = GameUtility.GetPrice(mode, CONSUME_TYPE.CREDIT);
		if (price <= 0)
		{
			component.text = ((mode == GAME_MODE.MUSICGACHA) ? "―" : $"{price}");
		}
		else
		{
			string arg = ((price == 1) ? "CREDIT" : "CREDITS");
			component.text = $"{price}<size=0.75em>{arg}</size>";
		}
		int num = ((mode == GAME_MODE.MUSICGACHA) ? 300 : GameUtility.GetPrice(mode, CONSUME_TYPE.PASELI));
		obj.gameObject.SetActive(flag);
		if (!flag || num <= 0)
		{
			component2.text = "0<size=0.75em>円(税込)</size>";
		}
		else
		{
			component2.text = $"{num}<size=0.75em>円(税込)</size>";
		}
	}

	private void SetContinuePriceDisp(bool isPaseliEnable, TextMeshProUGUI lowerCreditText, TextMeshProUGUI lowerPaseliText, TextMeshProUGUI upperCreditText, TextMeshProUGUI upperPaseliText)
	{
		int continueLowerPrice = CabinetManager.VirtualCoin.CurrentPrice.ContinueLowerPrice;
		int continueUpperPrice = CabinetManager.VirtualCoin.CurrentPrice.ContinueUpperPrice;
		int price = GameUtility.GetPrice(CONSUME_MODE.CONTINUE_CREDIT);
		upperCreditText.text = $"{price}";
		lowerCreditText.text = $"{price}";
		if (!isPaseliEnable || continueLowerPrice <= 0)
		{
			upperPaseliText.text = "";
			lowerPaseliText.text = "";
		}
		else
		{
			upperPaseliText.text = $"{continueUpperPrice}円～";
			lowerPaseliText.text = $"{continueLowerPrice}円";
		}
	}

	private void BgMovie_Init()
	{
		VideoPlayer component = m_bgMovie.GetComponent<VideoPlayer>();
		component.targetTexture.Release();
		component.source = VideoSource.Url;
		component.url = MovieSelecter(isNewChara: false);
		component.loopPointReached += FinishPlayingVideo;
		BgMovie_IsFinished = false;
	}

	private void NewCharaMovie_Init()
	{
		VideoPlayer component = m_bgMovieNewChara.GetComponent<VideoPlayer>();
		component.targetTexture.Release();
		component.source = VideoSource.Url;
		component.url = MovieSelecter(isNewChara: true);
		component.loopPointReached += FinishPlayingNewCharaVideo;
		NewCharaMovie_IsFinished = false;
	}

	private void BgMovie_Play()
	{
		m_bgMovie.GetComponent<VideoPlayer>().Play();
		m_screen.gameObject.SetActive(value: true);
		SoundManager.PlayBgm(BGMSelecter(isNewChara: false)).Forget();
		if (SystemConfigGameData.CabinetType == CabinetType.UKS)
		{
			SubMonitorFlow.ChangeState(SubMonitorFlow.MonitorState.CharaIntroduction).Forget();
		}
		if (m_licenseWindow.activeSelf)
		{
			LicenseWindow_Close(enableCloseSE: false);
		}
		if (m_priceTable.activeSelf)
		{
			PriceTable_Close(enableCloseSE: false);
		}
		m_licenseButton.gameObject.SetActive(value: false);
		m_priceButton.gameObject.SetActive(value: false);
	}

	private void NewCharaMovie_Play()
	{
		m_bgMovieNewChara.GetComponent<VideoPlayer>().Play();
		m_screen.gameObject.SetActive(value: true);
		SoundManager.PlayBgm(BGMSelecter(isNewChara: true)).Forget();
		if (SystemConfigGameData.CabinetType == CabinetType.UKS)
		{
			SubMonitorFlow.ChangeState(SubMonitorFlow.MonitorState.CharaIntroduction).Forget();
		}
		if (m_licenseWindow.activeSelf)
		{
			LicenseWindow_Close(enableCloseSE: false);
		}
		if (m_priceTable.activeSelf)
		{
			PriceTable_Close(enableCloseSE: false);
		}
		m_licenseButton.gameObject.SetActive(value: false);
		m_priceButton.gameObject.SetActive(value: false);
	}

	public void FinishPlayingVideo(VideoPlayer player)
	{
		player.Stop();
		SoundManager.StopBgm().Forget();
		BgMovie_IsFinished = true;
	}

	public void FinishPlayingNewCharaVideo(VideoPlayer player)
	{
		player.Stop();
		SoundManager.StopBgm().Forget();
		NewCharaMovie_IsFinished = true;
	}

	private string MovieSelecter(bool isNewChara)
	{
		if (isNewChara)
		{
			return Application.streamingAssetsPath + "/Characters/Common/NewCharacterPV.webm";
		}
		if (GameDataManager.IsCharaEnable(CharaType.Chara12))
		{
			return Application.streamingAssetsPath + "/Characters/Common/CharacterPVA.webm";
		}
		return Application.streamingAssetsPath + "/Characters/Common/CharacterPVB.webm";
	}

	private string BGMSelecter(bool isNewChara)
	{
		if (isNewChara)
		{
			return "BGM_MOVIE_B";
		}
		if (GameDataManager.IsCharaEnable(CharaType.Chara12))
		{
			return "BGM_MOVIE_A";
		}
		return "BGM_MOVIE_B";
	}
}
