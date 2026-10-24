using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MFG.GameData;
using MFG.GameRequest;
using MFG.Sytem.Utilities;
using MFG.Types;
using MFG.Types.Customize.Costume;
using MFG.Types.Customize.CustomizeItem;
using TestModeSkeleton;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SubMonitorFlow : BaseFlow
{
	public enum MonitorState
	{
		None,
		Close,
		Title,
		CharaIntroduction,
		Mirroring,
		MahjongPlayScore,
		CharaDisplay,
		SpiritGym
	}

	[SerializeField]
	private Camera subMainCam;

	[SerializeField]
	private UIAttachCam uIAttachCam;

	[SerializeField]
	private GameObject black;

	[SerializeField]
	private CanvasGroup titleCanvas;

	[SerializeField]
	private GameObject introductionScreen;

	[SerializeField]
	private GameObject mirroringScreen;

	[SerializeField]
	private SubMonitorScore scoreScreen;

	[SerializeField]
	private MahjongActorNotice maNotice;

	[SerializeField]
	private GameObject bannerParent;

	[SerializeField]
	private GameObject tipsParent;

	[SerializeField]
	private Image tipsImage;

	[SerializeField]
	private Animator transitionAnimation;

	[SerializeField]
	private Transform cameraParent;

	private static SubMonitorFlow m_instance = null;

	private SubMonitorCharaFlow m_charaSceneInstance;

	public static MFG.Types.Customize.Costume.ItemID DemoCharaCostume = MFG.Types.Customize.Costume.ItemID.Costume01_001;

	public static bool IsTransition = false;

	public static bool IsTransitionWithLoad = false;

	private static bool isForceState = true;

	public static bool IsLoaded = false;

	private CancellationToken token;

	private bool isTransitionAnimPlay;

	private MonitorState beforeState;

	private Light jichaLight;

	public static Dictionary<CabinetType, List<MonitorState>> MonitorStateDictInHome = new Dictionary<CabinetType, List<MonitorState>>
	{
		{
			CabinetType.UKS,
			new List<MonitorState>
			{
				MonitorState.Title,
				MonitorState.SpiritGym
			}
		},
		{
			CabinetType.TBS,
			new List<MonitorState>
			{
				MonitorState.CharaDisplay,
				MonitorState.SpiritGym,
				MonitorState.Title
			}
		},
		{
			CabinetType.UJK,
			new List<MonitorState>
			{
				MonitorState.CharaDisplay,
				MonitorState.SpiritGym,
				MonitorState.Title
			}
		}
	};

	public static Dictionary<CabinetType, List<MonitorState>> MonitorStateDictInMahjong = new Dictionary<CabinetType, List<MonitorState>>
	{
		{
			CabinetType.UKS,
			new List<MonitorState>
			{
				MonitorState.Title,
				MonitorState.Mirroring,
				MonitorState.MahjongPlayScore,
				MonitorState.SpiritGym
			}
		},
		{
			CabinetType.TBS,
			new List<MonitorState>
			{
				MonitorState.CharaDisplay,
				MonitorState.MahjongPlayScore,
				MonitorState.SpiritGym,
				MonitorState.Title
			}
		},
		{
			CabinetType.UJK,
			new List<MonitorState>
			{
				MonitorState.CharaDisplay,
				MonitorState.MahjongPlayScore,
				MonitorState.SpiritGym,
				MonitorState.Title
			}
		}
	};

	public static int SubMonitorCabinetIndex => Math.Max((int)(SystemConfigGameData.CabinetType - 6), 0);

	private SubMonitorFlow()
	{
		_enableDebugLogin = false;
	}

	public static SubMonitorFlow GetInstance()
	{
		if (!m_instance)
		{
			m_instance = GameObject.Find("SubMonitorFlow").GetComponent<SubMonitorFlow>();
		}
		return m_instance;
	}

	public async UniTask TransitionAnimOpen()
	{
		if (!isTransitionAnimPlay)
		{
			isTransitionAnimPlay = true;
			transitionAnimation.gameObject.SetActive(value: false);
			transitionAnimation.gameObject.SetActive(value: true);
			await UniTask.WaitUntil(() => transitionAnimation.GetCurrentAnimatorStateInfo(0).IsName("TransitionAnimationStay"));
		}
	}

	public void TransitionAnimClose()
	{
		if (isTransitionAnimPlay)
		{
			transitionAnimation.SetTrigger("Next");
			isTransitionAnimPlay = false;
		}
	}

	public static async UniTask ChangeStateInSceneLoad(string sceneName, bool isGameOption = false)
	{
		if (!IsTransitionWithLoad)
		{
			IsTransitionWithLoad = true;
			MonitorState currentOption = GetCurrentOption(isHome: true);
			MonitorState currentOption2 = GetCurrentOption(isHome: false);
			MonitorState monitorState;
			switch (sceneName)
			{
			case "MahjongPlay":
				monitorState = currentOption2;
				break;
			case "TitleDemo":
				monitorState = ((SystemConfigGameData.CabinetType == CabinetType.UKS) ? MonitorState.Mirroring : MonitorState.CharaDisplay);
				break;
			case "Logo":
				monitorState = ((SystemConfigGameData.CabinetType == CabinetType.UKS) ? MonitorState.Mirroring : MonitorState.CharaDisplay);
				break;
			case "Dojo":
				monitorState = ((currentOption == MonitorState.SpiritGym) ? MonitorState.Title : currentOption);
				break;
			case "Home":
			case "CharaSelect":
			case "Continue":
			case "GachaSelect":
			case "GachaPlay":
			case "Result":
				monitorState = currentOption;
				break;
			case "Matching":
				monitorState = ((SystemConfigGameData.CabinetType == CabinetType.UKS) ? MonitorState.Mirroring : MonitorState.Title);
				break;
			case "Title":
			case "FirstCharaSelect":
				monitorState = ((SystemConfigGameData.CabinetType == CabinetType.UKS) ? MonitorState.Title : MonitorState.CharaDisplay);
				break;
			case "StoryPlay":
			case "NewRegistration":
			case "EventStart":
			case "EventResult":
			case "Login":
			case "Guide":
				monitorState = MonitorState.Title;
				break;
			case "Gameover":
				monitorState = ((SystemConfigGameData.CabinetType != CabinetType.UKS) ? MonitorState.Close : MonitorState.Mirroring);
				break;
			default:
				monitorState = MonitorState.Close;
				break;
			}
			MonitorState state = monitorState;
			if (isGameOption)
			{
				await GetInstance().TransitionAnimOpen();
			}
			Debug.Log($"SceneName {sceneName} / ChangeState {state}");
			await GetInstance().SetState(state);
			if (isGameOption)
			{
				GetInstance().TransitionAnimClose();
			}
			IsTransitionWithLoad = false;
		}
	}

	public static async UniTask ChangeState(MonitorState state)
	{
		if (SystemConfigGameData.IsDualMonitor)
		{
			await GetInstance().SetState(state);
		}
	}

	protected override async UniTask OnInitialize(CancellationToken cancellationToken)
	{
		uIAttachCam.Attatch(subMainCam);
		uIAttachCam.transform.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
		ResidentFlow.GetInstance().SetMaNoticeParent(maNotice);
		ResidentFlow.GetInstance().SetUpperTips(tipsParent, tipsImage);
		if (SystemConfigGameData.IsSXGAMonitor)
		{
			uIAttachCam.transform.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1280f, 1024f);
			tipsImage.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(1280f, 720f);
		}
		Color color = ((SystemConfigGameData.CabinetType == CabinetType.UKS) ? Color.white : Color.black);
		for (int i = 1; i <= 7; i++)
		{
			Transform obj = transitionAnimation.transform.Find($"LineOrange{i}");
			obj.GetComponent<Image>().color = color;
			obj.Find($"Semicircle{i}").GetComponent<Image>().color = color;
		}
	}

	protected override async UniTask OnExecute(CancellationToken cancellationToken)
	{
	}

	protected override void OnUpdate()
	{
	}

	private void OnDestroy()
	{
		ResidentFlow.GetInstance().CameraContext_Restore();
	}

	public void CameraContextRestore(CameraContext context)
	{
		context.Restore(subMainCam);
	}

	private async UniTask SetState(MonitorState state)
	{
		black.SetActive(state == MonitorState.Close);
		if (IsTransition)
		{
			return;
		}
		IsTransition = true;
		bool isTitle = state == MonitorState.Title;
		await titleCanvas.DOFade(isTitle ? 1 : 0, 0.5f).ToAwaiter();
		MahjongActorNotice_SetUpperDisp(isTitle);
		bannerParent.SetActive(isTitle);
		introductionScreen.SetActive(state == MonitorState.CharaIntroduction);
		if (state == MonitorState.CharaDisplay)
		{
			for (int i = 0; i < SceneManager.sceneCount && !(SceneManager.GetSceneAt(i).name == "SubMonitorChara"); i++)
			{
				if (i == SceneManager.sceneCount - 1)
				{
					await SceneManager.LoadSceneAsync("SubMonitorChara", LoadSceneMode.Additive);
					subMainCam.enabled = false;
					m_charaSceneInstance = SubMonitorCharaFlow.GetInstance();
				}
			}
			try
			{
				if (jichaLight != null)
				{
					jichaLight.enabled = false;
				}
			}
			catch
			{
			}
		}
		else
		{
			for (int k = 0; k < SceneManager.sceneCount; k++)
			{
				if (SceneManager.GetSceneAt(k).name == "SubMonitorChara")
				{
					m_charaSceneInstance = SubMonitorCharaFlow.ResetInstance();
					subMainCam.enabled = true;
					try
					{
						await SceneManager.UnloadSceneAsync("SubMonitorChara");
					}
					catch (ArgumentNullException)
					{
						Debug.Log("SubMonitorFlow ArgumentNullException UnloadSceneAsync");
					}
					break;
				}
			}
			try
			{
				if (jichaLight != null)
				{
					jichaLight.enabled = true;
				}
			}
			catch
			{
			}
		}
		mirroringScreen.SetActive(state == MonitorState.Mirroring);
		if (state == MonitorState.SpiritGym)
		{
			for (int i = 0; i < SceneManager.sceneCount && !(SceneManager.GetSceneAt(i).name == "SubMonitorDojo"); i++)
			{
				if (i == SceneManager.sceneCount - 1)
				{
					await SceneManager.LoadSceneAsync("SubMonitorDojo", LoadSceneMode.Additive);
					subMainCam.enabled = false;
					DojoGetStatus.ResultData statusResult = await RequestApiBase<DojoGetStatus, DojoGetStatus.ResultData>.Send(enableErrorScreenTransition: false);
					await SubMonitorDojoFlow.GetInstance().Init(statusResult);
				}
			}
		}
		else
		{
			for (int l = 0; l < SceneManager.sceneCount; l++)
			{
				if (SceneManager.GetSceneAt(l).name == "SubMonitorDojo")
				{
					subMainCam.enabled = true;
					try
					{
						await SceneManager.UnloadSceneAsync("SubMonitorDojo");
					}
					catch (ArgumentNullException)
					{
						Debug.Log("SubMonitorFlow ArgumentNullException UnloadSceneAsync");
					}
					break;
				}
			}
		}
		scoreScreen.gameObject.SetActive(state == MonitorState.MahjongPlayScore);
		scoreScreen.AllCameraSetEnable(state == MonitorState.MahjongPlayScore);
		await CharaViewRefresh();
		IsTransition = false;
	}

	public async UniTask CharaViewRefresh()
	{
		if (!(m_charaSceneInstance != null))
		{
			return;
		}
		for (int i = 0; i < SceneManager.sceneCount; i++)
		{
			Scene sceneAt = SceneManager.GetSceneAt(i);
			if (sceneAt.name == "Home" || sceneAt.name == "CharaSelect" || sceneAt.name == "FirstCharaSelect" || sceneAt.name == "Continue")
			{
				await m_charaSceneInstance.ViewChange(SubMonitorCharaFlow.CharaState.Home);
				return;
			}
			if (sceneAt.name == "Matching")
			{
				await m_charaSceneInstance.ViewChange(SubMonitorCharaFlow.CharaState.Matching);
				return;
			}
			if (sceneAt.name == "MahjongPlay" || sceneAt.name == "TitleDemo")
			{
				await m_charaSceneInstance.ViewChange(SubMonitorCharaFlow.CharaState.MahjongPlay);
				return;
			}
			if (sceneAt.name == "GachaPlay")
			{
				return;
			}
		}
		MFG.Types.Customize.Costume.ItemID costume = DemoCharaCostume;
		if (!string.IsNullOrEmpty(GameDataManager.Account.DataID))
		{
			costume = GameDataManager.Player.GetCurrentChara().CostumeItemEquip;
		}
		await m_charaSceneInstance.ViewChange(SubMonitorCharaFlow.CharaState.LoadThisScene, costume);
	}

	public async UniTask AgariOpen()
	{
		beforeState = GetCurrentOption(isHome: false);
		if (SystemConfigGameData.CabinetType == CabinetType.UKS && beforeState == MonitorState.Title)
		{
			await SetState(MonitorState.Mirroring);
		}
	}

	public async UniTask AgariClose()
	{
		if (SystemConfigGameData.CabinetType == CabinetType.UKS && beforeState == MonitorState.Title)
		{
			await SetState(beforeState);
		}
		beforeState = MonitorState.None;
	}

	public async UniTask TaikyokuStart(List<Player> player, Light light)
	{
		jichaLight = light;
		jichaLight.enabled = GetCurrentOption(isHome: false) != MonitorState.CharaDisplay;
		scoreScreen.Init(player);
		scoreScreen.AllCameraSetEnable(GetCurrentOption(isHome: false) == MonitorState.MahjongPlayScore);
	}

	public async UniTask TaikyokuFinish()
	{
		if (SystemConfigGameData.CabinetType == CabinetType.UKS && GetCurrentOption(isHome: false) == MonitorState.Title)
		{
			await SetState(MonitorState.Mirroring);
		}
		beforeState = MonitorState.None;
	}

	private void MahjongActorNotice_SetUpperDisp(bool isUpperDisp)
	{
		ResidentFlow.GetInstance().GetMahjongActorNoticeManager().SetUpperDisp(isUpperDisp);
	}

	public async UniTask CreateCompeBanner()
	{
		if (!SystemConfigGameData.IsDualMonitor)
		{
			return;
		}
		foreach (Transform item in bannerParent.transform)
		{
			UnityEngine.Object.Destroy(item.gameObject);
		}
		await CompetitionUtility.TitleBanner_Instantiate(bannerParent);
	}

	public void ScoreRefresh(Cha cha, string scoreText, Color color)
	{
		scoreScreen.ScoreRefresh(cha, scoreText, color);
	}

	public void KazeRefresh(Cha cha, Kaze kaze)
	{
		scoreScreen.KazeRefresh(cha, kaze);
	}

	public void ChageAccessory(MFG.Types.Customize.CustomizeItem.ItemID itemID)
	{
		scoreScreen.AccessoryChange(itemID);
	}

	public void ReachAuraSetActive(Cha cha, bool isActive, Color auraPrefab)
	{
		scoreScreen.ReachAuraSetActive(cha, isActive, auraPrefab);
	}

	public static string IndexText(MonitorState state)
	{
		return state switch
		{
			MonitorState.CharaDisplay => "ディスプレイ", 
			MonitorState.MahjongPlayScore => "点数表示", 
			MonitorState.SpiritGym => "スピリットジム", 
			MonitorState.Title => "タイトル画面", 
			MonitorState.Mirroring => "対局画面", 
			_ => "", 
		};
	}

	public static string GetIndexText(GameOption.OptionType optionType)
	{
		return IndexText(GetCurrentOption(optionType == GameOption.OptionType.SubMonitorHome));
	}

	public static MonitorState GetCurrentOption(bool isHome)
	{
		if (!isHome)
		{
			return MonitorStateDictInMahjong[SystemConfigGameData.CabinetType][GameDataManager.PlayerConfig.SubMonitorInMahjong[SubMonitorCabinetIndex]];
		}
		return MonitorStateDictInHome[SystemConfigGameData.CabinetType][GameDataManager.PlayerConfig.SubMonitorInHome[SubMonitorCabinetIndex]];
	}
}
