using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using IO;
using KAMUNITY;
using MFG.MasterGameData;
using MFG.Sytem.Utilities;
using MFG.Types;
using MFG.Types.Customize.Catchphrase;
using MFG.Types.Customize.Costume;
using MFG.Types.Customize.CustomizeItem;
using MFG.Types.Customize.CutinItem;
using TCSNET;
using TMPro;
using TestModeSkeleton;
using UniRx;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ResidentFlow : BaseFlow
{
	public enum TransitionEffectType
	{
		None,
		Fade,
		Anime,
		Tv
	}

	[SerializeField]
	private TextMeshProUGUI m_bootingText;

	[SerializeField]
	private GameObject m_camera;

	[SerializeField]
	private GameObject m_eventSystem;

	[SerializeField]
	public GameObject m_fadePanel;

	[SerializeField]
	private CountDownTimer m_timer;

	[SerializeField]
	private GameObject m_residentBG;

	[SerializeField]
	private LoadingProgressPanel m_loadingProgressPanel;

	[SerializeField]
	private AdvertiseTipsManager m_advertiseTipsManager;

	[SerializeField]
	private TouchEffect m_touchEffect;

	[SerializeField]
	private MouseCursor m_mouseCursor;

	[SerializeField]
	private MahjongActorNoticeManager _mahjongActorNoticeManager;

	public DemoErrorMessage DemoErrorMessage;

	public SerialIDDispCommand SerialIDDispCommand;

	[SerializeField]
	private EdgeTouchingWarning _edgeTouchingWarning;

	[SerializeField]
	private GameObject m_transitionAnimation;

	[SerializeField]
	private GameObject m_transitionTvOn;

	[SerializeField]
	private GameObject _insertCoinMessage;

	[SerializeField]
	private Header m_header;

	[SerializeField]
	private GameObject m_profiler;

	[SerializeField]
	private float m_fadeDuration = 1f;

	[SerializeField]
	private AudioMixer m_audioMixer;

	[SerializeField]
	private GameObject m_agingModeText;

	[SerializeField]
	private GameObject m_RandomAginModeText;

	[SerializeField]
	private GameObject m_tapTestPrefab;

	[NonSerialized]
	public GameObject m_tapTest;

	[SerializeField]
	private DebugCommandWindow _debugCommandWindow;

	[SerializeField]
	public DebugPlaySoundLabel m_debugPlaySoundLabel;

	[SerializeField]
	private GameObject m_ledTest;

	[SerializeField]
	private AppTerminateConfirmWindow m_appTerminateConfirmWindow;

	[SerializeField]
	private RenderTexture _lowerScreenRenderTexture;

	[SerializeField]
	private GameObject _lowerScreen;

	[SerializeField]
	private Canvas _lowerCanvas;

	[SerializeField]
	private Camera _streamingCamera;

	[SerializeField]
	private Camera _lowerCamera;

	[SerializeField]
	private SampleDriver _streamingDriver;

	public GameObject _touchpanelTelemetry;

	private CameraContext _cameraContextStack = new CameraContext();

	private bool m_isTestModeEffective;

	public bool IsForceTestModeEffective = true;

	public bool IsAppTerminateEffective;

	public bool IsAppTerminateConfirmRequest;

	public bool IsAppTerminated;

	private static ResidentFlow m_instance;

	public static bool IsAwaked;

	public static bool IsInitialized;

	private static bool IsInitializedBeforeStart;

	private bool m_sceneLoading;

	[Space(10f)]
	public bool m_isAgingMode;

	public static BaseInput CustomInput;

	private SceneInstance m_sceneInstance;

	public bool m_isTitleDemoMahjongPlayAlive;

	private string m_currentSceneName = "";

	private bool updateLog;

	private readonly string[] booting = new string[4] { "Booting", "Booting.", "Booting..", "Booting..." };

	private bool m_isStartGameMode;

	private TransitionEffectType m_transType;

	public DebugCommandWindow DebugCommandWindow => _debugCommandWindow;

	public bool IsTestModeEffective
	{
		get
		{
			return m_isTestModeEffective;
		}
		set
		{
			m_isTestModeEffective = value;
		}
	}

	public bool IsTestModeRequest { get; set; }

	public bool IsErrorModeEffective { get; set; }

	public bool IsErrorModeRequest { get; set; }

	public ERROR_LEVEL GotoErrorSceneLevel { get; set; }

	public bool IsAppTerminateConfirmWindowOpen => m_appTerminateConfirmWindow.IsOpen;

	public bool IsNetworkCofirmSkip { get; set; }

	public bool IsSceneLoading => m_sceneLoading;

	public GameObject InsertCoinMessage => _insertCoinMessage;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static async void InitializeOnBoot()
	{
		UnityEngine.Debug.unityLogger.logEnabled = false;
		UnityEngine.Debug.Log($"[{DateTime.Now}]ResidnetFlow:Initialize1");
		Cursor.visible = false;
		Input.multiTouchEnabled = false;
		DisableUnityAnalyticsCompletely();
		ApplyCommandLineArgs();
		ApplyEnvironmentVariables();
		_ = GameUtility.IsCloudStreaming;
		UnityEngine.Debug.Log($"[{DateTime.Now}]ResidnetFlow:Initialize2");
		if (GameUtility.IsKamunityBootEnable())
		{
			Kamunity.Startup();
			if (KamunityInstance.isEa3Boot)
			{
				TcsXrpcModule.AddModule();
			}
		}
		UnityEngine.Debug.Log($"[{DateTime.Now}]ResidnetFlow:Initialize3");
		TextManager.Initialize(TextManager.LANGUAGE.JAPANESE);
		UnityEngine.Debug.Log($"[{DateTime.Now}]ResidnetFlow:Initialize4");
		DeviceManager.Instance.Initialize();
		UnityEngine.Debug.Log($"[{DateTime.Now}]ResidnetFlow:Initialize5");
		UnityEngine.Debug.Log($"[{DateTime.Now}]ResidnetFlow:Initialize6");
		string sceneName = SceneManager.GetActiveScene().name;
		if (sceneName != "Resident")
		{
			AsyncOperation asyncOperation = SceneManager.LoadSceneAsync("Resident", LoadSceneMode.Additive);
			if (asyncOperation != null)
			{
				await asyncOperation;
			}
			SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
			GetInstance().SceneChangeDisable();
			GetInstance().m_currentSceneName = sceneName;
		}
		UnityEngine.Debug.Log($"[{DateTime.Now}]ResidnetFlow:Initialize7");
		CustomInput = GetInstance().gameObject.AddComponent<MFGCustomInput>();
		UnityEngine.Debug.Log($"[{DateTime.Now}]ResidnetFlow:Initialize8");
		IsInitializedBeforeStart = true;
	}

	private ResidentFlow()
	{
		_enableResidentFlowInitializeWait = false;
		_enableDebugLogin = false;
	}

	protected override void Awake()
	{
		UnityEngine.Debug.Log($"Screen at Awake: w:{Screen.width}, h:{Screen.height}, mode:{Screen.fullScreenMode}");
		UIAttachCam componentInChildren = _lowerCanvas.GetComponentInChildren<UIAttachCam>(includeInactive: true);
		if (componentInChildren != null)
		{
			UnityEngine.Debug.Log("ResidentFlow: Awake Camera Attatch");
			componentInChildren.Attatch(_lowerCamera);
		}
		IsAwaked = true;
	}

	public async void LoadScene(string sceneName, IReadOnlyDictionary<string, string> additionalAssets = null)
	{
		KAMUNITY.Logger.LogFormat("LoadScene-->{0}", sceneName);
		if (m_sceneLoading || m_currentSceneName == "ScopeClose")
		{
			KAMUNITY.Logger.LogWarning("LoadScene-->既にロード中です");
			return;
		}
		if (m_currentSceneName == "ScopeClose")
		{
			KAMUNITY.Logger.LogWarning("LoadScene-->ScopeClose後のロードは無効です");
			return;
		}
		m_sceneLoading = true;
		sceneName = GetForceInterruptScene(sceneName);
		m_currentSceneName = sceneName;
		List<UniTask> list = new List<UniTask>();
		if (IsSubMonitorTransitionPlay(sceneName))
		{
			list.Add(SubMonitorFlow.GetInstance().TransitionAnimOpen());
		}
		list.Add(TransitionEffectOpen(sceneName));
		await list;
		m_camera.SetActive(value: true);
		AudioListener audioListner = m_camera.GetComponent<AudioListener>();
		audioListner.gameObject.SetActive(value: false);
		SceneChangeDisable();
		var (isDisp, isClose, consumeProgress) = GetSceneLoadingPanelDispInfo(sceneName);
		_ = SceneManager.GetActiveScene().name;
		if (isDisp)
		{
			m_loadingProgressPanel.Open();
		}
		Stopwatch sw = new Stopwatch();
		sw.Restart();
		Scene activeScene = SceneManager.GetActiveScene();
		string unloadSceneName = activeScene.name;
		await SceneManager.UnloadSceneAsync(activeScene);
		if (isDisp)
		{
			m_loadingProgressPanel.SetProgress(0.2f * consumeProgress);
		}
		audioListner.gameObject.SetActive(value: true);
		sw.Stop();
		UnityEngine.Debug.Log($"UnLoadScene:{unloadSceneName}/{sw.ElapsedMilliseconds}ms");
		await UniTask.WaitWhile(() => m_isTitleDemoMahjongPlayAlive);
		sw.Restart();
		AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
		if (asyncOperation != null)
		{
			await asyncOperation;
		}
		if (isDisp)
		{
			m_loadingProgressPanel.SetProgress(consumeProgress);
		}
		if (isDisp && isClose)
		{
			m_loadingProgressPanel.Close();
		}
		sw.Stop();
		UnityEngine.Debug.Log($"LoadScene:{sceneName}/{sw.ElapsedMilliseconds}ms");
		audioListner.gameObject.SetActive(value: false);
		m_sceneLoading = false;
		SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
		m_camera.SetActive(value: false);
		if (SystemConfigGameData.IsDualMonitor)
		{
			await SubMonitorFlow.ChangeStateInSceneLoad(sceneName);
		}
		switch (sceneName)
		{
		case "StoryPlay":
			return;
		case "Home":
			return;
		case "GachaSelect":
			return;
		}
		if (IsSubMonitorTransitionPlay(sceneName))
		{
			SubMonitorFlow.GetInstance().TransitionAnimClose();
		}
		await TransitionEffectClose();
	}

	private (bool isDisp, bool isClose, float consumeProgress) GetSceneLoadingPanelDispInfo(string sceneName)
	{
		return sceneName switch
		{
			"MahjongPlay" => (isDisp: true, isClose: false, consumeProgress: 0.5f), 
			"StoryPlay" => (isDisp: true, isClose: false, consumeProgress: 0.3f), 
			"Home" => (isDisp: true, isClose: false, consumeProgress: 0.7f), 
			_ => (isDisp: false, isClose: false, consumeProgress: 0f), 
		};
	}

	public static ResidentFlow GetInstance()
	{
		if (!m_instance)
		{
			m_instance = GameObject.Find("ResidentFlow").GetComponent<ResidentFlow>();
		}
		return m_instance;
	}

	public void SetFadeSettings(Color color, float duration)
	{
		m_fadePanel.GetComponent<Image>().color = color;
		m_fadeDuration = duration;
	}

	public void SetLoadingProgress(float progress, float duration = 0.3f)
	{
		float item = GetSceneLoadingPanelDispInfo(m_currentSceneName).consumeProgress;
		float progress2 = ((item != 0f) ? (item + progress * (1f - item)) : progress);
		m_loadingProgressPanel.SetProgress(progress2, duration);
	}

	public async UniTask SetLoadingProgressComplete()
	{
		m_loadingProgressPanel.SetProgress(1f, 0f);
		await m_advertiseTipsManager.Close();
	}

	public async UniTask StartLoading()
	{
		float item = GetSceneLoadingPanelDispInfo(m_currentSceneName).consumeProgress;
		m_loadingProgressPanel.Open(item);
		await m_advertiseTipsManager.Open();
	}

	public void ExitLoading()
	{
		m_loadingProgressPanel.Close();
	}

	public void SetUpperTips(GameObject parent, Image image)
	{
		m_advertiseTipsManager.SetUpperTips(parent, image);
	}

	public async UniTask ActivateSecondDisplay(bool isDisp = true)
	{
		if (!SystemConfigGameData.IsDualMonitor)
		{
			return;
		}
		if (isDisp)
		{
			if (!SubMonitorFlow.IsLoaded)
			{
				SubMonitorFlow.IsLoaded = true;
				await SceneManager.LoadSceneAsync("SubMonitor", LoadSceneMode.Additive);
			}
		}
		else
		{
			await SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName("SubMonitor"));
		}
	}

	public Camera MainCamera(bool isForceLower = false)
	{
		if (!isForceLower)
		{
			if (!GameUtility.IsCloudStreaming)
			{
				return _lowerCamera;
			}
			return _streamingCamera;
		}
		return _lowerCamera;
	}

	public void CameraContext_Reserve(int displayNo = 0)
	{
		_cameraContextStack.Reserve(MainCamera());
	}

	public void CameraContext_Restore()
	{
		_cameraContextStack.Restore(MainCamera());
		if (SystemConfigGameData.IsDualMonitor)
		{
			try
			{
				SubMonitorFlow.GetInstance().CameraContextRestore(_cameraContextStack);
			}
			catch
			{
				UnityEngine.Debug.Log("SubMonitor unloaded");
			}
		}
	}

	public void CameraContext_Restore(CameraContext cameraContext)
	{
		cameraContext.Restore(MainCamera());
		if (SystemConfigGameData.IsDualMonitor)
		{
			try
			{
				SubMonitorFlow.GetInstance().CameraContextRestore(cameraContext);
			}
			catch
			{
				UnityEngine.Debug.Log("SubMonitor unloaded");
			}
		}
	}

	public CountDownTimer GetTimer()
	{
		return m_timer;
	}

	public TouchEffect GetTouchEffect()
	{
		return m_touchEffect;
	}

	public MouseCursor GetMouseCursor()
	{
		return m_mouseCursor;
	}

	public Header GetHeader()
	{
		return m_header;
	}

	public GameObject GetResidentBG()
	{
		return m_residentBG;
	}

	private void SceneChangeDisable()
	{
		m_timer.SetDisp(disp: false);
		m_timer.ResetTimer(disp: false);
		m_residentBG.SetActive(value: false);
		ExitLoading();
		m_header.SetDisp(disp: false);
	}

	private void ExecExeResidientFileLoad()
	{
		foreach (string item in new List<string> { "Characters/Chara1/SsrMangan.webm", "Characters/Chara2/SsrMangan.webm", "Characters/Chara3/SsrMangan.webm", "Characters/Chara4/SsrMangan.webm" })
		{
			EXECEXE.WaitForSingleObject(EXECEXE.CreateFileCache(Path.Combine(Application.streamingAssetsPath, item)), uint.MaxValue);
		}
	}

	protected override async UniTask OnInitialize(CancellationToken cancellationToken)
	{
		if (!IsInitializedBeforeStart)
		{
			await UniTask.WaitUntil(() => IsInitializedBeforeStart);
		}
		UnityEngine.Debug.Log($"Screen at OnInitialize: w:{Screen.width}, h:{Screen.height}, mode:{Screen.fullScreenMode}");
		ApplyDrawEnvironment();
		if (GameUtility.IsCloudStreaming)
		{
			int w = 1920;
			int h = 1080;
			UnityEngine.Debug.Log($"SetResolution at OnInitialize: w:{w}, h:{h}");
			Screen.SetResolution(w, h, FullScreenMode.Windowed);
			await UniTask.Yield();
			_streamingDriver.Initialize(w, h);
		}
		else
		{
			Screen.fullScreen = true;
		}
		await SoundManager.Initialize(m_audioMixer, GameUtility.IsCloudStreaming);
		await LoadResidentFiles();
		if (KamunityInstance.isActive)
		{
			CabinetManager.Startup();
			SystemUtility.Instance.Initialize();
		}
		ErrorData.Initialize();
		UnityEngine.Debug.Log($"[{DateTime.Now}]ResidnetFlow:Start3");
		if (GameUtility.IsKonastaOrNetcafeBehavior)
		{
			m_appTerminateConfirmWindow.OnDecided.Subscribe(delegate(bool isTerminateDecided)
			{
				if (isTerminateDecided && !GameUtility.IsAppTerminatDecided && !IsAppTerminated)
				{
					IsAppTerminateEffective = false;
					GameUtility.IsAppTerminatDecided = true;
					MahjongPlayFlow mahjongPlayFlow = null;
					GameObject gameObject = GameObject.Find("SceneFlow");
					if (gameObject != null)
					{
						mahjongPlayFlow = gameObject.GetComponent<MahjongPlayFlow>();
					}
					if (mahjongPlayFlow != null && !GameDataManager.Mahjong.IsHaifuPlay)
					{
						mahjongPlayFlow.KikenRequenst();
					}
					else if (m_currentSceneName == "Result")
					{
						IsAppTerminated = true;
					}
					else
					{
						IsAppTerminated = true;
						GetInstance().LoadScene("ScopeClose");
					}
				}
			}).AddTo(this);
			m_appTerminateConfirmWindow.OnClose.Subscribe(delegate
			{
				if (GameUtility.IsKonastaOrNetcafeBehavior && !GameUtility.IsAppTerminatDecided && !IsAppTerminated)
				{
					IsAppTerminateEffective = true;
				}
			}).AddTo(this);
			RefleshWindowHandle();
		}
		UnityEngine.Debug.Log($"[{DateTime.Now}]ResidnetFlow:Start4");
		SetFadeSettings(Color.white, 0.5f);
		IsInitialized = true;
		m_bootingText.gameObject.SetActive(value: false);
	}

	private void ApplyDrawEnvironment()
	{
		UnityEngine.Debug.Log("ResidentFlow: ApplyDrawEnvironment");
		if (GameUtility.IsCloudStreaming)
		{
			_streamingCamera.enabled = true;
			_lowerCamera.enabled = false;
			_lowerCanvas.renderMode = RenderMode.ScreenSpaceCamera;
			_lowerCanvas.worldCamera = _streamingCamera;
		}
		else if (SystemConfigGameData.IsDualMonitor)
		{
			_streamingCamera.enabled = false;
			_lowerCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
			_lowerCanvas.worldCamera = null;
			_lowerScreen.SetActive(value: true);
			_lowerCamera.enabled = true;
			_lowerCamera.targetTexture = _lowerScreenRenderTexture;
		}
		else
		{
			_streamingCamera.enabled = false;
			_lowerCanvas.renderMode = RenderMode.ScreenSpaceCamera;
			_lowerCanvas.worldCamera = _lowerCamera;
			_lowerScreen.SetActive(value: false);
			_lowerCamera.enabled = true;
			_lowerCamera.targetTexture = null;
		}
	}

	private async UniTask LoadResidentFiles()
	{
		ExecExeResidientFileLoad();
		UnityEngine.Debug.Log($"[{DateTime.Now}]ResidnetFlow:Start1");
		await MasterGameDataManager.Load();
		UnityEngine.Debug.Log($"[{DateTime.Now}]ResidnetFlow:Start2");
		UniTask uniTask = CharaSelectWindow.ResidentLoad();
		UniTask uniTask2 = MFG.Types.Customize.Costume.Item.ResidentLoad();
		UniTask uniTask3 = MFG.Types.Customize.CustomizeItem.Item.ResidentLoad();
		UniTask uniTask4 = MFG.Types.Customize.CutinItem.Item.ResidentLoad();
		UniTask uniTask5 = MFG.Types.Customize.Catchphrase.Item.ResidentLoad();
		UniTask uniTask6 = SchoolEmblemIcon.ResidentLoad();
		await UniTask.WhenAll(uniTask, uniTask2, uniTask3, uniTask4, uniTask5, uniTask6);
	}

	protected override void Update()
	{
		if (!IsInitialized)
		{
			int num = (int)Time.time % 4;
			m_bootingText.gameObject.SetActive(value: true);
			if (num < booting.Count())
			{
				m_bootingText.text = booting[num];
			}
		}
		base.Update();
	}

	protected override void OnUpdate()
	{
		if (updateLog)
		{
			UnityEngine.Debug.Log($"[{DateTime.Now}]ResidnetFlow:Update1");
		}
		Bi2xInput.Instance.Update();
		if (updateLog)
		{
			UnityEngine.Debug.Log($"[{DateTime.Now}]ResidnetFlow:Update2");
		}
		ModifyInputModuleOverride();
		if (updateLog)
		{
			UnityEngine.Debug.Log($"[{DateTime.Now}]ResidnetFlow:Update3");
		}
		if (updateLog)
		{
			UnityEngine.Debug.Log($"[{DateTime.Now}]ResidnetFlow:Update4");
		}
		if (BackupDataManager.Instance.Initialized)
		{
			BackupDataManager.Instance.Save();
		}
		if (updateLog)
		{
			UnityEngine.Debug.Log($"[{DateTime.Now}]ResidnetFlow:Update5");
		}
		EagCore.Instance.UpdateStatus();
		if (updateLog)
		{
			UnityEngine.Debug.Log($"[{DateTime.Now}]ResidnetFlow:Update6");
		}
		ModifyErrorModeRequest();
		ModifyTestModeRequest();
		if (updateLog)
		{
			UnityEngine.Debug.Log($"[{DateTime.Now}]ResidnetFlow:Update7");
		}
		if (!m_sceneLoading)
		{
			if (IsTestModeEffective && IsTestModeRequest)
			{
				IsTestModeEffective = false;
				IsTestModeRequest = false;
				LoadScene("TestModeMain");
			}
			else if (IsErrorModeEffective && IsErrorModeRequest)
			{
				IsErrorModeEffective = false;
				IsErrorModeRequest = false;
				LoadScene("ErrorMode");
			}
			else if (!IsSceneLoading && IsAppTerminateEffective && IsAppTerminateConfirmRequest && !m_appTerminateConfirmWindow.IsOpen)
			{
				IsAppTerminateEffective = false;
				IsAppTerminateConfirmRequest = false;
				m_appTerminateConfirmWindow.Open(isSeEnable: false);
			}
		}
		if (updateLog)
		{
			UnityEngine.Debug.Log($"[{DateTime.Now}]ResidnetFlow:Update8");
		}
	}

	private void ModifyInputModuleOverride()
	{
		if (SystemConfigGameData.CabinetType == CabinetType.B || SystemConfigGameData.CabinetType == CabinetType.C || GameUtility.IsCloudStreaming)
		{
			BaseInputModule baseInputModule = EventSystem.current?.currentInputModule;
			if (!(baseInputModule == null) && baseInputModule.inputOverride == null)
			{
				EventSystem.current.currentInputModule.inputOverride = CustomInput;
			}
		}
	}

	private string GetForceInterruptScene(string destSceneName)
	{
		if (!KamunityInstance.isActive)
		{
			return destSceneName;
		}
		if (destSceneName == "ScopeClose")
		{
			return destSceneName;
		}
		if (IsAppTerminated)
		{
			return "ScopeClose";
		}
		if (destSceneName != "Logo")
		{
			return destSceneName;
		}
		bool num = GameUtility.IsEAmuseActive();
		bool flag = EagUtility.IsNeedConfirm();
		ModifyErrorModeRequest();
		if (num && flag && !m_isStartGameMode)
		{
			return "NetworkConfirm";
		}
		if (IsErrorModeEffective && IsErrorModeRequest)
		{
			IsErrorModeRequest = false;
			return "ErrorMode";
		}
		if (ShopCloseUtility.GetShopClosePhase() == GameSettings.ShopClosePhase.Final)
		{
			return "ShopClose";
		}
		m_isStartGameMode = true;
		return destSceneName;
	}

	private void ModifyErrorModeRequest()
	{
		bool flag = false;
		bool flag2 = false;
		ErrorLogData lastError = ErrorControl.GetLastError();
		if (lastError != null)
		{
			flag = ErrorControl.IsEndRetryTime(lastError);
			flag2 = lastError.m_level >= GotoErrorSceneLevel;
		}
		IsErrorModeRequest = flag && flag2;
	}

	private void ModifyTestModeRequest()
	{
		IsTestModeRequest = IsTestModeEffective && Bi2xInput.IsTrigger(BUTTON.TEST);
	}

	private static void DisableUnityAnalyticsCompletely()
	{
		Analytics.enabled = false;
		Analytics.deviceStatsEnabled = false;
		Analytics.limitUserTracking = true;
		Analytics.initializeOnStartup = false;
		PerformanceReporting.enabled = false;
	}

	public static TransitionEffectType GetTransitionEffectType(string sceneName)
	{
		switch (sceneName)
		{
		case "Login":
		case "Home":
		case "ModeSelect":
		case "StorySelect":
		case "CharaSelect":
		case "StoryPlay":
		case "GachaSelect":
		case "Dojo":
			return TransitionEffectType.Anime;
		case "Matching":
			return TransitionEffectType.Tv;
		case "Consume":
			return TransitionEffectType.None;
		default:
			return TransitionEffectType.Fade;
		}
	}

	public static bool IsSubMonitorTransitionPlay(string sceneName)
	{
		if (SystemConfigGameData.IsDualMonitor)
		{
			switch (sceneName)
			{
			case "ScopeClose":
			case "TestModeMain":
			case "NetworkConfirm":
			case "QCMode":
			case "ErrorMode":
			case "ParticipationError":
			case "FadeTest":
			case "TitleDemo":
			case "GachaPlay":
				return false;
			default:
				return true;
			}
		}
		return false;
	}

	public async UniTask TransitionEffectOpen(string sceneName)
	{
		m_transType = GetTransitionEffectType(sceneName);
		switch (m_transType)
		{
		case TransitionEffectType.Fade:
			await m_fadePanel.GetComponent<CanvasGroup>().DOFade(1f, m_fadeDuration).ToAwaiter();
			break;
		case TransitionEffectType.Anime:
			m_transitionAnimation.SetActive(value: false);
			m_transitionAnimation.SetActive(value: true);
			await UniTask.WaitUntil(() => m_transitionAnimation.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("TransitionAnimationStay"));
			break;
		case TransitionEffectType.Tv:
			m_transitionTvOn.SetActive(value: false);
			m_transitionTvOn.SetActive(value: true);
			await UniTask.WaitUntil(() => m_transitionTvOn.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("TransitionTvOnStay"));
			break;
		case TransitionEffectType.None:
			break;
		}
	}

	public async UniTask TransitionEffectClose()
	{
		switch (m_transType)
		{
		case TransitionEffectType.Fade:
			await m_fadePanel.GetComponent<CanvasGroup>().DOFade(0f, m_fadeDuration).ToAwaiter();
			break;
		case TransitionEffectType.Anime:
			m_transitionAnimation.GetComponent<Animator>().SetTrigger("Next");
			break;
		case TransitionEffectType.Tv:
			m_transitionTvOn.GetComponent<Animator>().SetTrigger("Next");
			break;
		}
	}

	public void SetMaNoticeParent(MahjongActorNotice upper)
	{
		_mahjongActorNoticeManager.SetUpperParent(upper);
	}

	public MahjongActorNoticeManager GetMahjongActorNoticeManager()
	{
		return _mahjongActorNoticeManager;
	}

	public void SetInGameDisp(bool isDisp, bool edgeTouchingWarning = true, OnlineUpdateMessage.DispType onlineUpdateMessageDispType = OnlineUpdateMessage.DispType.None)
	{
		if (GameUtility.IsKonastaOrNetcafeBehavior)
		{
			edgeTouchingWarning = false;
		}
		DemoErrorMessage.gameObject.SetActive(isDisp);
		SerialIDDispCommand.gameObject.SetActive(isDisp);
		SingletonBehaviour<OnlineUpdateMessage>.Instance.SetDispType(onlineUpdateMessageDispType);
		SingletonBehaviour<ShopCloseMessage>.Instance.SetActive(isDisp);
		_edgeTouchingWarning.gameObject.SetActive(edgeTouchingWarning);
		if (!isDisp)
		{
			_mahjongActorNoticeManager.SetDisp(isLowerDisp: false, isUpperDisp: false);
		}
	}

	private static void ApplyEnvironmentVariables()
	{
		string environmentVariable = Environment.GetEnvironmentVariable("VFG_CABINET_TYPE");
		CabinetType cabinetType = CabinetType.None;
		CabinetType cabinetType2;
		switch (environmentVariable)
		{
		case "B":
			cabinetType2 = CabinetType.B;
			goto IL_007a;
		case "C":
			cabinetType2 = CabinetType.C;
			goto IL_007a;
		case "HG":
			cabinetType2 = CabinetType.HG;
			goto IL_007a;
		case "UKS":
			cabinetType2 = CabinetType.UKS;
			goto IL_007a;
		case "TBS":
			cabinetType2 = CabinetType.TBS;
			goto IL_007a;
		case "UJK":
			cabinetType2 = CabinetType.UJK;
			goto IL_007a;
		default:
			cabinetType2 = CabinetType.None;
			goto IL_007a;
		case null:
			break;
			IL_007a:
			cabinetType = cabinetType2;
			break;
		}
		SystemConfigGameData.CabinetType = cabinetType;
		KAMUNITY.Logger.Log($"CabinetType: {SystemConfigGameData.CabinetType}");
	}

	private static void ApplyCommandLineArgs()
	{
		CommandLineArgs commandLineArgs = new CommandLineArgs(Environment.GetCommandLineArgs());
		KAMUNITY.Logger.Log("args: " + commandLineArgs.Dump());
		GameUtility.IsKonamiNetConnection = commandLineArgs.GetSwitch("-KONAMINET", defaultValue: false);
		GameUtility.IsIOEnable = !commandLineArgs.GetSwitch("-NOIO", defaultValue: false);
		SystemConfigGameData.AppBehaviorType = commandLineArgs.GetOption("-appBehavior", "AC") switch
		{
			"AC" => AppBehaviorType.AC, 
			"KNST" => AppBehaviorType.Konasta, 
			"NETCAFE" => AppBehaviorType.Netcafe, 
			_ => AppBehaviorType.AC, 
		};
		KAMUNITY.Logger.Log($"IsKonamiNetConnection: {GameUtility.IsKonamiNetConnection}");
		KAMUNITY.Logger.Log($"IsIOEnable: {GameUtility.IsIOEnable}");
		KAMUNITY.Logger.Log($"AppBehavior: {SystemConfigGameData.AppBehaviorType}");
		KAMUNITY.Logger.Log($"IsCloudStreaming: {SystemConfigGameData.IsCloudStreaming}");
		string frontUrl = commandLineArgs.GetOption("-FRONTURL", "");
		if (!string.IsNullOrEmpty(frontUrl))
		{
			// rebuild-only: point the AOG front at any endpoint (local mock, webfront proxy, or the
			// public provider) with the proxy hop disabled; desktop reaches them directly, WebGL via
			// same-origin. NetEnvironment.SetEnv is shipped code left unused by Konami for exactly this.
			Konami.System.Net.NetEnvironment.Instance.SetEnv(new Konami.System.Net.NetEnvironmentConnection
			{
				Name = "rebuild",
				RequestUrl = frontUrl,
				Proxy = Konami.System.Net.NetEnvironmentProxy.Default
			});
			SystemConfigGameData.FrontServerUrl = frontUrl;
			KAMUNITY.Logger.Log($"FRONTURL: {frontUrl}");
		}
	}

	private void OnApplicationFocus(bool focus)
	{
		if (GameUtility.IsCloudStreaming && focus)
		{
			RefleshWindowHandle();
		}
	}

	private void RefleshWindowHandle()
	{
		if (GameUtility.IsCloudStreaming)
		{
			Win32Utility.MinimizeWindow(Win32Utility.FindWindow((string x) => x.Contains("UKS")));
		}
	}

	public static void AppTerminate()
	{
		Application.Quit();
	}
}
