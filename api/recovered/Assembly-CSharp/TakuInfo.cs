using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MFG.Assets;
using MFG.Taikyoku.Command.Receive;
using MFG.Types;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class TakuInfo : MonoBehaviour
{
	private enum EventWindowType
	{
		BingoWindow,
		ScrambleMatchHelpWindow,
		TutorialHelpWindow
	}

	[SerializeField]
	private MahjongPlayFlow m_mahjongPlayFlow;

	[SerializeField]
	private Button settingAreaButton;

	[SerializeField]
	private Animator settingAreaAnimator;

	[SerializeField]
	private GameObject nakinashiIconRoot;

	[SerializeField]
	private GameObject autoAgariIconRoot;

	[SerializeField]
	private GameObject supportIconRoot;

	[SerializeField]
	private Button autoAgariButton;

	[SerializeField]
	private Button supportButton;

	[SerializeField]
	private Button nakinashiButton;

	[SerializeField]
	private TextMeshProUGUI kyokuInfoText;

	[SerializeField]
	private TextMeshProUGUI tsumiNumText;

	[SerializeField]
	private TextMeshProUGUI reachNumText;

	[SerializeField]
	private TextMeshProUGUI restTileText;

	[SerializeField]
	public RestTile restTile;

	[SerializeField]
	private GameObject exitSelect;

	[SerializeField]
	private Button exitButton;

	[SerializeField]
	private Button exitButtonYes;

	[SerializeField]
	private Button exitButtonNo;

	[SerializeField]
	private Button exitButtonOutofRange;

	[SerializeField]
	private GameOption gameOption;

	[SerializeField]
	private Button gameOptionButton;

	[SerializeField]
	private GameObject eventButton;

	[SerializeField]
	private GameObject eventParent;

	private BingoWindow bingoWindow;

	[SerializeField]
	private Button bingoEventButton;

	[SerializeField]
	private Button _scrambleMatchEventButton;

	[SerializeField]
	private Button _tutorialButton;

	private ScrambleMatchHelpWindow _scrambleMatchHelpWindow;

	private TutorialHelpWindow _tutorialHelpWindow;

	private ExclusiveWindowController<int> _exclusiveWindowController = new ExclusiveWindowController<int>();

	public bool m_isNakinashi;

	private PlayerConfigGameData m_playerConfigGameData => GameDataManager.PlayerConfig;

	private void Start()
	{
		kyokuInfoText.text = "東一局";
		restTileText.text = "0";
		tsumiNumText.text = "0";
		reachNumText.text = "0";
		settingAreaButton.OnClickAsObservable().Subscribe(delegate
		{
			OnClick_SettingArea();
		}).AddTo(this);
		autoAgariButton.OnClickAsObservable().Subscribe(delegate
		{
			OnClick_AutoAgari();
		}).AddTo(this);
		supportButton.OnClickAsObservable().Subscribe(delegate
		{
			OnClick_Support();
		}).AddTo(this);
		nakinashiButton.OnClickAsObservable().Subscribe(delegate
		{
			OnClick_Nakinashi();
		}).AddTo(this);
		exitButton.OnClickAsObservable().Subscribe(delegate
		{
			OnClick_Exit();
		}).AddTo(this);
		exitButtonYes.OnClickAsObservable().Subscribe(delegate
		{
			OnClick_ExitYes();
		}).AddTo(this);
		exitButtonNo.OnClickAsObservable().Subscribe(delegate
		{
			OnClick_ExitNo();
		}).AddTo(this);
		exitButtonOutofRange.OnClickAsObservable().Subscribe(delegate
		{
			OnClick_ExitNo();
		}).AddTo(this);
		gameOptionButton.OnClickAsObservable().Subscribe(delegate
		{
			OnClick_GameOption();
		}).AddTo(this);
		EventWindowInit().Forget();
		nakinashiButton.transform.Find("BtOn").gameObject.SetActive(m_isNakinashi);
		nakinashiIconRoot?.transform.Find("IconOn").gameObject.SetActive(m_isNakinashi);
		autoAgariButton.transform.Find("BtOn").gameObject.SetActive(m_playerConfigGameData.IsAutoAgariEnable);
		autoAgariIconRoot?.transform.Find("IconOn").gameObject.SetActive(m_playerConfigGameData.IsAutoAgariEnable);
		supportButton.transform.Find("BtOn").gameObject.SetActive(m_playerConfigGameData.IsTaikyokuSupport);
		supportIconRoot?.transform.Find("IconOn").gameObject.SetActive(m_playerConfigGameData.IsTaikyokuSupport);
		exitSelect.transform.Find("WindowSub/Window/EndMessage3").gameObject.SetActive(GameDataManager.Mahjong.TakuType != TakuType.Nima && !GameDataManager.Mahjong.IsPrivateMatch);
	}

	private void Update()
	{
		if (GameDataManager.PlayerConfig.IsKeyboardPlay())
		{
			if (autoAgariButton.IsInteractable() && Bi2xInput.GetKeyDown(KeyCode.A))
			{
				OnClick_AutoAgari();
				MahjongUtility.ResetNoInputCount();
			}
			if (nakinashiButton.IsInteractable() && Bi2xInput.GetKeyDown(KeyCode.N))
			{
				OnClick_Nakinashi();
				MahjongUtility.ResetNoInputCount();
			}
		}
		try
		{
			autoAgariButton.transform.Find("KeyboardText").gameObject.SetActive(GameDataManager.PlayerConfig.IsKeyboardGuideDisplay());
			nakinashiButton.transform.Find("KeyboardText").gameObject.SetActive(GameDataManager.PlayerConfig.IsKeyboardGuideDisplay());
		}
		catch
		{
		}
	}

	public void OnClick_SettingArea()
	{
		settingAreaAnimator.ResetTrigger("InDisp");
		settingAreaAnimator.SetTrigger("InDisp");
	}

	public void OnClick_AutoAgari()
	{
		m_playerConfigGameData.IsAutoAgariEnable = !m_playerConfigGameData.IsAutoAgariEnable;
		autoAgariButton.transform.Find("BtOn").gameObject.SetActive(m_playerConfigGameData.IsAutoAgariEnable);
		autoAgariIconRoot?.transform.Find("IconOn").gameObject.SetActive(m_playerConfigGameData.IsAutoAgariEnable);
		if (m_playerConfigGameData.IsAutoAgariEnable)
		{
			SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
		}
		else
		{
			SoundManager.PlaySe("SFX_SYS_CANCEL2").Forget();
		}
	}

	public void OnClick_Nakinashi()
	{
		m_isNakinashi = !m_isNakinashi;
		nakinashiButton.transform.Find("BtOn").gameObject.SetActive(m_isNakinashi);
		nakinashiIconRoot?.transform.Find("IconOn").gameObject.SetActive(m_isNakinashi);
		if (m_isNakinashi)
		{
			SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
		}
		else
		{
			SoundManager.PlaySe("SFX_SYS_CANCEL2").Forget();
		}
	}

	public void OnClick_Support()
	{
		m_playerConfigGameData.IsTaikyokuSupport = !m_playerConfigGameData.IsTaikyokuSupport;
		supportButton?.transform.Find("BtOn").gameObject.SetActive(m_playerConfigGameData.IsTaikyokuSupport);
		supportIconRoot?.transform.Find("IconOn").gameObject.SetActive(m_playerConfigGameData.IsTaikyokuSupport);
		m_mahjongPlayFlow.SetTaikyokuSupport(m_playerConfigGameData.IsTaikyokuSupport);
		if (m_playerConfigGameData.IsTaikyokuSupport)
		{
			SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
		}
		else
		{
			SoundManager.PlaySe("SFX_SYS_CANCEL2").Forget();
		}
	}

	public void SetExitButtonInteractive(bool flag)
	{
		exitButton.interactable = flag;
		if (!flag)
		{
			exitSelect.SetActive(value: false);
		}
	}

	public void OnClick_Exit()
	{
		if (m_mahjongPlayFlow.m_isTaikyokuActive && m_mahjongPlayFlow.IsHaiSelectEnalbe())
		{
			SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
			exitSelect.SetActive(value: true);
		}
	}

	public void OnClick_ExitYes()
	{
		m_mahjongPlayFlow.KikenRequenst();
		SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
		exitSelect.SetActive(value: false);
	}

	public void OnClick_ExitNo()
	{
		SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
		exitSelect.gameObject.SetActive(value: false);
	}

	public void SetGameOptionButtonInteractive(bool flag)
	{
		gameOptionButton.interactable = flag;
		if (!flag)
		{
			gameOption.SetActive(active: false);
		}
	}

	public void OnClick_GameOption()
	{
		if (m_mahjongPlayFlow.m_isTaikyokuActive)
		{
			SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
			gameOption.SetActive(active: true);
		}
	}

	public void SetDaisouForceEnd()
	{
		if (m_mahjongPlayFlow.m_isDaiuchi)
		{
			gameOption.OnClick_Daiuchi();
		}
	}

	public void KyokuInit(KyokuStart cmd, int nokoriHaiNum)
	{
		if (cmd.IsAllLast)
		{
			kyokuInfoText.text = "オーラス";
		}
		else
		{
			string text = ((cmd.Ba == 0) ? "東" : "南");
			List<string> list = new List<string> { "一", "二", "三", "四" };
			if (cmd.Kyoku < list.Count)
			{
				kyokuInfoText.text = text + list[cmd.Kyoku] + "局";
			}
		}
		restTileText.text = $"{nokoriHaiNum}";
		tsumiNumText.text = $"{cmd.Honba}";
		reachNumText.text = $"{cmd.Kyotaku}";
	}

	public void KyokuInitAfterHaipai()
	{
		nakinashiButton.interactable = true;
		m_isNakinashi = false;
		nakinashiButton.transform.Find("BtOn").gameObject.SetActive(m_isNakinashi);
		nakinashiIconRoot?.transform.Find("IconOn").gameObject.SetActive(m_isNakinashi);
		autoAgariButton.interactable = true;
		autoAgariButton.transform.Find("BtOn").gameObject.SetActive(m_playerConfigGameData.IsAutoAgariEnable);
		autoAgariIconRoot?.transform.Find("IconOn").gameObject.SetActive(m_playerConfigGameData.IsAutoAgariEnable);
		restTile.gameObject.SetActive(value: true);
		restTile.Start();
		supportButton.interactable = true;
		supportButton.transform.Find("BtOn").gameObject.SetActive(m_playerConfigGameData.IsTaikyokuSupport);
		m_mahjongPlayFlow.SetTaikyokuSupport(m_playerConfigGameData.IsTaikyokuSupport);
		supportIconRoot?.transform.Find("IconOn").gameObject.SetActive(m_playerConfigGameData.IsTaikyokuSupport);
	}

	public void SetReachNum(int reachNum)
	{
		reachNumText.text = $"{reachNum}";
	}

	public void SetRestTileNum(int restTileNum)
	{
		restTileText.text = $"{restTileNum}";
	}

	public async void Destruct()
	{
		nakinashiButton.interactable = false;
		autoAgariButton.interactable = false;
		supportButton.interactable = false;
		restTile.gameObject.SetActive(value: false);
	}

	public void BtForceDisp()
	{
		bingoEventButton.gameObject.SetActive(value: true);
		_scrambleMatchEventButton.gameObject.SetActive(value: true);
		_tutorialButton.gameObject.SetActive(value: true);
	}

	private async UniTask EventWindowInit()
	{
		_exclusiveWindowController.Clear();
		if (GameDataManager.BingoEventRemake.IsEnable())
		{
			IDisposableAsset<GameObject> obj = await AssetLoader.LoadAssetAsync<GameObject>("Bingo/Prefabs/BingoMahjongPlay.prefab");
			obj.AddTo(this);
			GameObject gameObject = Object.Instantiate(obj.Asset, eventParent.transform, worldPositionStays: false);
			bingoWindow = gameObject.GetComponent<BingoWindow>();
			_exclusiveWindowController.Add(0, bingoWindow);
			bingoEventButton.gameObject.SetActive(value: true);
			bingoEventButton.OnClickAsObservable().Subscribe(delegate
			{
				if (m_mahjongPlayFlow.m_isTaikyokuActive && bingoWindow != null)
				{
					_exclusiveWindowController.ExclusiveOpen(0);
				}
			}).AddTo(this);
		}
		else
		{
			bingoEventButton.gameObject.SetActive(value: false);
		}
		_scrambleMatchEventButton.gameObject.SetActive(GameDataManager.Taikyoku.IsScrambleMatch);
		if (GameDataManager.Taikyoku.IsScrambleMatch)
		{
			IDisposableAsset<GameObject> obj2 = await AssetLoader.LoadAssetAsync<GameObject>("ScrambleMatch/Prefabs/ScrambleMatchHelpWindow.prefab");
			obj2.AddTo(this);
			GameObject gameObject2 = Object.Instantiate(obj2.Asset, eventParent.transform, worldPositionStays: false);
			_scrambleMatchHelpWindow = gameObject2.GetComponent<ScrambleMatchHelpWindow>();
			_exclusiveWindowController.Add(1, _scrambleMatchHelpWindow);
			_scrambleMatchEventButton.OnClickAsObservable().Subscribe(delegate
			{
				if (m_mahjongPlayFlow.m_isTaikyokuActive)
				{
					_exclusiveWindowController.ExclusiveOpen(1);
				}
			}).AddTo(this);
		}
		_tutorialButton.gameObject.SetActive(GameDataManager.Player.DanLevel <= DanLevelType.KYU1);
		if (GameDataManager.Player.DanLevel <= DanLevelType.KYU1)
		{
			IDisposableAsset<GameObject> obj3 = await AssetLoader.LoadAssetAsync<GameObject>("MahjongPlayExclusiveWindow/Prefabs/TutorialHelpWindow.prefab");
			obj3.AddTo(this);
			GameObject gameObject3 = Object.Instantiate(obj3.Asset, eventParent.transform, worldPositionStays: false);
			_tutorialHelpWindow = gameObject3.GetComponent<TutorialHelpWindow>();
			_exclusiveWindowController.Add(2, _tutorialHelpWindow);
			_tutorialButton.OnClickAsObservable().Subscribe(delegate
			{
				if (m_mahjongPlayFlow.m_isTaikyokuActive)
				{
					_exclusiveWindowController.ExclusiveOpen(2);
				}
			}).AddTo(this);
		}
		eventButton.SetActive(value: false);
	}

	public void SetEventButtonInteractive(bool isSelectable)
	{
		if (_exclusiveWindowController.Count() > 0)
		{
			eventButton.SetActive(isSelectable);
			if (!isSelectable)
			{
				_exclusiveWindowController.CloseAll();
			}
		}
	}
}
