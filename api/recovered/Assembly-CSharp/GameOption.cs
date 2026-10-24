using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using MFG.Types;
using MFG.Types.HaiConjecture;
using TMPro;
using TestModeSkeleton;
using TestModeSkeleton.CabinetDef;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class GameOption : MonoBehaviour
{
	public enum OptionType
	{
		None,
		View,
		Support,
		SafetyNavi,
		Daiuchi,
		RuleDisplay,
		Anonymous,
		Keyboard,
		UksLed,
		MouseCursor,
		USBCharge,
		HeadphoneVolume,
		SubMonitorHome,
		MaAnimation,
		SubMonitorMahjong,
		Sticker
	}

	public enum DisplayLocation
	{
		None,
		Home,
		MahjongPlay,
		All
	}

	public enum ButtonType
	{
		None,
		Others,
		Toggle,
		OnOff,
		ExtraOnOff,
		Arrow
	}

	[Serializable]
	public class OptionButtonInfo
	{
		public OptionType OptionType;

		public DisplayLocation DisplayLocation;

		public ButtonType ButtonType;

		public GameObject IndexGameObject;

		[Header("Toggleの場合は以下のボタンの順に入れるのが吉")]
		public List<GameObject> ButtonParents;

		[Header("オンボタンor左ボタン")]
		public Button Button1;

		public string Bt1MethodName;

		[Header("オフボタンor右ボタン")]
		public Button Button2;

		public string Bt2MethodName;

		[Header("ボタン3")]
		public Button Button3;

		public string Bt3MethodName;

		[Header("有効な筐体タイプ。[-, -, B, C, -, HG, UKS, TBS, UJK]")]
		public bool[] IsEnableCabinetType = new bool[9] { true, false, true, true, false, true, true, true, true };

		public bool IsArcadeOnly;

		public bool IsStreamingOnly;

		public bool IsGuestEnable = true;

		public bool IsOnlyPro;

		public int selectingIndex;

		public void ButtonActive(bool isActive)
		{
			if (isActive)
			{
				IndexGameObject.SetActive(value: true);
				foreach (GameObject buttonParent in ButtonParents)
				{
					buttonParent.SetActive(value: true);
				}
				ButtonRefresh();
				return;
			}
			IndexGameObject.SetActive(value: false);
			foreach (GameObject buttonParent2 in ButtonParents)
			{
				buttonParent2.SetActive(value: false);
			}
		}

		public bool ButtonInit(DisplayLocation location)
		{
			bool flag = (DisplayLocation != location && DisplayLocation != DisplayLocation.All) || (!IsGuestEnable && GameDataManager.Account.IsGuestUser) || (IsArcadeOnly && GameUtility.IsCloudStreaming) || (IsStreamingOnly && !GameUtility.IsCloudStreaming) || !IsEnableCabinetType[(int)SystemConfigGameData.CabinetType] || (IsOnlyPro && !GameDataManager.Player.IsPro());
			if (!flag)
			{
				switch (OptionType)
				{
				case OptionType.View:
					selectingIndex = ((MahjongUtility.GetViewType() != 0) ? 1 : 0);
					break;
				case OptionType.Support:
					selectingIndex = (GameDataManager.PlayerConfig.IsTaikyokuSupport ? 1 : 0);
					break;
				case OptionType.SafetyNavi:
					selectingIndex = (int)(GameDataManager.PlayerConfig.ConjectureType - 1);
					break;
				case OptionType.Anonymous:
					selectingIndex = ((!GameDataManager.Player.IsAnonymous) ? 1 : 0);
					break;
				case OptionType.Keyboard:
					selectingIndex = ((!GameDataManager.PlayerConfig.IsKeyboardPlayInStreaming) ? 1 : ((!GameDataManager.PlayerConfig.IsKeyboardGuideInStreaming) ? 2 : 0));
					break;
				case OptionType.UksLed:
				{
					GameSettings.SaveData data = CabinetManager.Game.Data;
					bool flag2;
					switch (SystemConfigGameData.CabinetType)
					{
					case CabinetType.UKS:
						flag2 = data.LEDCtrlOnOff == GameOptionsDef.LEDCtrlOnOff.On || data.LEDWingOnOff == GameOptionsDef.LEDWingOnOff.On || data.LEDBttnOnOff == GameOptionsDef.LEDBttnOnOff.On;
						break;
					case CabinetType.TBS:
					case CabinetType.UJK:
						flag2 = data.LEDHeadOnOff == GameOptionsDef.LEDHeadOnOff.On || data.LEDSideOnOff == GameOptionsDef.LEDSideOnOff.On;
						break;
					default:
						flag2 = false;
						break;
					}
					flag = !flag2;
					selectingIndex = ((!GameDataManager.PlayerConfig.IsUKSLEDAnimAllow) ? 1 : 0);
					break;
				}
				case OptionType.MouseCursor:
					selectingIndex = ((!GameDataManager.PlayerConfig.IsCursorAnimAllow) ? 1 : 0);
					break;
				case OptionType.SubMonitorHome:
					selectingIndex = GameDataManager.PlayerConfig.SubMonitorInHome[SubMonitorFlow.SubMonitorCabinetIndex];
					break;
				case OptionType.SubMonitorMahjong:
					selectingIndex = GameDataManager.PlayerConfig.SubMonitorInMahjong[SubMonitorFlow.SubMonitorCabinetIndex];
					break;
				case OptionType.MaAnimation:
					selectingIndex = ((!GameDataManager.PlayerConfig.IsProAnimation) ? 1 : 0);
					break;
				case OptionType.Sticker:
					selectingIndex = ((!GameDataManager.PlayerConfig.IsStickerAllDisp) ? 1 : 0);
					break;
				default:
					selectingIndex = 0;
					break;
				}
			}
			ButtonActive(!flag);
			return !flag;
		}

		public void ButtonRefresh()
		{
			if (OptionType == OptionType.USBCharge)
			{
				Button3.interactable = SystemConfigGameData.CabinetType == CabinetType.HG;
				Button3.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = (Bi2xInput.Instance.IsUsbChargerEnable() ? "充電可能" : "充電する");
				Button3.transform.Find("Text").GetComponent<TextMeshProUGUI>().color = (Bi2xInput.Instance.IsUsbChargerEnable() ? new Color(0f, 90f, 255f) : new Color(102f, 0f, 102f));
			}
			switch (ButtonType)
			{
			case ButtonType.Toggle:
				ButtonParents[0].SetActive(selectingIndex == 1);
				ButtonParents[1].SetActive(selectingIndex == 0);
				break;
			case ButtonType.OnOff:
				Button1.transform.GetComponent<Animator>().SetTrigger((selectingIndex == 0) ? "Selected" : "Normal");
				Button2.transform.GetComponent<Animator>().SetTrigger((selectingIndex == 1) ? "Selected" : "Normal");
				break;
			case ButtonType.ExtraOnOff:
				Button1.transform.GetComponent<Animator>().SetTrigger((selectingIndex == 0) ? "Selected" : "Normal");
				Button2.transform.GetComponent<Animator>().SetTrigger((selectingIndex == 1) ? "Selected" : "Normal");
				Button3.transform.GetComponent<Animator>().SetTrigger((selectingIndex == 2) ? "Selected" : "Normal");
				break;
			case ButtonType.Arrow:
				switch (OptionType)
				{
				case OptionType.SafetyNavi:
				{
					TextMeshProUGUI component = ButtonParents.First().transform.Find("BtOptionArrow/OptionFrame/Content").GetComponent<TextMeshProUGUI>();
					string text = ((GameDataManager.PlayerConfig.ConjectureType != ConjectureType.OnlyReach) ? $"リーチ<size=20> または </size>{(int)(GameDataManager.PlayerConfig.ConjectureType - 1)}鳴きに対する現物" : "リーチに対する現物");
					component.text = text;
					break;
				}
				case OptionType.SubMonitorHome:
				case OptionType.SubMonitorMahjong:
					ButtonParents.First().transform.Find("BtOptionArrow/OptionFrame/Content").GetComponent<TextMeshProUGUI>().text = SubMonitorFlow.GetIndexText(OptionType);
					break;
				}
				break;
			}
		}
	}

	[SerializeField]
	private MahjongPlayFlow m_mahjongPlayFlow;

	[SerializeField]
	private DisplayLocation location;

	[SerializeField]
	private Button closeButton;

	[SerializeField]
	private Button outOfRange;

	[SerializeField]
	private List<OptionButtonInfo> options;

	private Dictionary<int, List<OptionType>> pageList;

	[SerializeField]
	private GameObject pageParent;

	[SerializeField]
	private TextMeshProUGUI pageIndex;

	[SerializeField]
	private TextMeshProUGUI pageMaxIndex;

	[SerializeField]
	private Button pageLeftArrow;

	[SerializeField]
	private Button pageRightArrow;

	private int currentPage;

	private int pageMax;

	[SerializeField]
	private GameObject ruleWindow;

	[SerializeField]
	private Button ruleWindowOutofRange;

	[SerializeField]
	private Button ruleWindowOutofRange2;

	[SerializeField]
	private GameObject usbChargeWindow;

	[SerializeField]
	private GameObject soundIndex;

	[SerializeField]
	private GameObject soundSliderON;

	[SerializeField]
	private GameObject soundSliderOFF;

	[SerializeField]
	private Slider volumeChangeSlider;

	[SerializeField]
	private TextMeshProUGUI volumeChangeText;

	[SerializeField]
	private Image volumeChangeIcon;

	[SerializeField]
	private Sprite volumeChangeIconON;

	[SerializeField]
	private Sprite volumeChangeIconOFF;

	[SerializeField]
	private List<GameObject> dummyObjects;

	private bool isHeadPhoneJacking;

	private bool isInit;

	private int dispCountMax
	{
		get
		{
			if (location != DisplayLocation.Home)
			{
				return 4;
			}
			return 3;
		}
	}

	private void Start()
	{
		closeButton.OnClickAsObservable().Subscribe(delegate
		{
			Close();
		}).AddTo(this);
		outOfRange.OnClickAsObservable().Subscribe(delegate
		{
			Close();
		}).AddTo(this);
		if (ruleWindow != null)
		{
			ruleWindowOutofRange.OnClickAsObservable().Subscribe(delegate
			{
				ruleWindow.GetComponent<PlayRuleWindow>().Close().Forget();
				m_mahjongPlayFlow.m_isRuleDisplaying = false;
				Close();
			}).AddTo(this);
		}
		pageMax = 0;
		pageList = new Dictionary<int, List<OptionType>>();
		pageList.Add(pageMax, new List<OptionType>());
		isHeadPhoneJacking = false;
		soundIndex.SetActive(value: false);
		soundSliderON.SetActive(value: false);
		soundSliderOFF.SetActive(value: false);
		foreach (OptionButtonInfo option2 in options)
		{
			option2.ButtonActive(isActive: false);
		}
		foreach (OptionButtonInfo option in options)
		{
			if (!option.ButtonInit(location))
			{
				continue;
			}
			if (option.Button1 != null && !string.IsNullOrEmpty(option.Bt1MethodName))
			{
				option.Button1.OnClickAsObservable().Subscribe(delegate
				{
					if ((bool)GetType().GetMethod(option.Bt1MethodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.Invoke(this, null))
					{
						option.selectingIndex = 0;
						option.ButtonRefresh();
					}
				}).AddTo(this);
			}
			if (option.Button2 != null && !string.IsNullOrEmpty(option.Bt2MethodName))
			{
				option.Button2.OnClickAsObservable().Subscribe(delegate
				{
					if ((bool)GetType().GetMethod(option.Bt2MethodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.Invoke(this, null))
					{
						option.selectingIndex = 1;
						option.ButtonRefresh();
					}
				}).AddTo(this);
			}
			if (option.Button3 != null && !string.IsNullOrEmpty(option.Bt3MethodName))
			{
				option.Button3.OnClickAsObservable().Subscribe(delegate
				{
					if ((bool)GetType().GetMethod(option.Bt3MethodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.Invoke(this, null))
					{
						option.selectingIndex = 2;
						option.ButtonRefresh();
					}
				}).AddTo(this);
			}
			if (pageList[pageMax].Count >= dispCountMax)
			{
				pageMax++;
				pageList.Add(pageMax, new List<OptionType>());
			}
			pageList[pageMax].Add(option.OptionType);
			option.ButtonActive(pageList.Sum((KeyValuePair<int, List<OptionType>> x) => x.Value.Count()) <= dispCountMax);
		}
		if (!GameUtility.IsCloudStreaming)
		{
			volumeChangeSlider.value = GameDataManager.PlayerConfig.headPhoneVolume;
			volumeChangeSlider.OnValueChangedAsObservable().Subscribe(delegate(float volumeRate)
			{
				SoundManager.SetUserHeadPhoneVolume(volumeRate);
				GameDataManager.PlayerConfig.headPhoneVolume = volumeRate;
				int num = (int)(volumeRate * 99f);
				volumeChangeText.text = $"{num}";
				volumeChangeIcon.sprite = ((num == 0) ? volumeChangeIconOFF : volumeChangeIconON);
			}).AddTo(this);
			if (pageList[pageMax].Count >= dispCountMax)
			{
				pageMax++;
				pageList.Add(pageMax, new List<OptionType>());
			}
			pageList[pageMax].Add(OptionType.HeadphoneVolume);
			soundSliderOFF.SetActive(pageList.Sum((KeyValuePair<int, List<OptionType>> x) => x.Value.Count()) <= dispCountMax);
		}
		if (pageMax == 0)
		{
			pageLeftArrow.gameObject.SetActive(value: false);
			pageRightArrow.gameObject.SetActive(value: false);
		}
		else
		{
			pageLeftArrow.OnClickAsObservable().Subscribe(delegate
			{
				currentPage = (currentPage + pageMax) % (pageMax + 1);
				PageRefresh();
			}).AddTo(this);
			pageRightArrow.OnClickAsObservable().Subscribe(delegate
			{
				currentPage = (currentPage + 1) % (pageMax + 1);
				PageRefresh();
			}).AddTo(this);
		}
		isInit = true;
		PageRefresh();
	}

	private void PageRefresh(bool isOnlyHeadphone = false)
	{
		if (!isInit)
		{
			return;
		}
		pageIndex.text = $"{currentPage + 1}";
		pageMaxIndex.text = $"{pageMax + 1}";
		if (!isOnlyHeadphone)
		{
			foreach (OptionButtonInfo option in options)
			{
				option.ButtonActive(pageList[currentPage].Contains(option.OptionType));
			}
			int num = dispCountMax - pageList[currentPage].Count();
			for (int i = 0; i < 3; i++)
			{
				dummyObjects[i].SetActive(i < num);
			}
		}
		if (pageList[currentPage].Contains(OptionType.HeadphoneVolume))
		{
			if (isHeadPhoneJacking != Bi2xInput.Instance.IsHeadphoneJacking())
			{
				isHeadPhoneJacking = Bi2xInput.Instance.IsHeadphoneJacking();
			}
			soundIndex.SetActive(value: true);
			soundSliderON.SetActive(isHeadPhoneJacking);
			soundSliderOFF.SetActive(!isHeadPhoneJacking);
		}
		else
		{
			soundIndex.SetActive(value: false);
			soundSliderON.SetActive(value: false);
			soundSliderOFF.SetActive(value: false);
		}
	}

	private void Update()
	{
		PageRefresh(isOnlyHeadphone: true);
	}

	public void SetActive(bool active)
	{
		if (active)
		{
			base.gameObject.SetActive(value: true);
			PageRefresh();
			return;
		}
		base.gameObject.SetActive(value: false);
		if (ruleWindow != null)
		{
			ruleWindow.SetActive(value: false);
		}
		usbChargeWindow.SetActive(value: false);
		if (m_mahjongPlayFlow != null)
		{
			m_mahjongPlayFlow.m_isRuleDisplaying = false;
		}
	}

	public void Close()
	{
		if (m_mahjongPlayFlow != null)
		{
			m_mahjongPlayFlow.SetConjectureSupport(GameDataManager.PlayerConfig.IsTaikyokuSupport);
		}
		SoundManager.PlaySe("SFX_SYS_CANCEL1").Forget();
		SetActive(active: false);
	}

	private bool OnClickWaiwaiView()
	{
		if (MahjongUtility.GetViewType() != 0)
		{
			SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
			m_mahjongPlayFlow.ViewType_Change(ViewType.Type1);
		}
		return true;
	}

	private bool OnClickSimpleView()
	{
		if (MahjongUtility.GetViewType() != ViewType.Type3)
		{
			SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
			m_mahjongPlayFlow.ViewType_Change(ViewType.Type3);
		}
		return true;
	}

	private bool OnClickAnpaiLeftArrow()
	{
		SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
		GameDataManager.PlayerConfig.ConjectureType = (ConjectureType)((int)(GameDataManager.PlayerConfig.ConjectureType + 3) % 5 + 1);
		return true;
	}

	private bool OnClickAnpaiRightArrow()
	{
		SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
		GameDataManager.PlayerConfig.ConjectureType = (ConjectureType)((int)GameDataManager.PlayerConfig.ConjectureType % 5 + 1);
		return true;
	}

	private bool OnClickSupportOn()
	{
		SoundManager.PlaySe("SFX_SYS_CANCEL2").Forget();
		GameDataManager.PlayerConfig.IsTaikyokuSupport = false;
		m_mahjongPlayFlow.SetTaikyokuSupport(GameDataManager.PlayerConfig.IsTaikyokuSupport);
		return true;
	}

	private bool OnClickSupportOff()
	{
		SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
		GameDataManager.PlayerConfig.IsTaikyokuSupport = true;
		m_mahjongPlayFlow.SetTaikyokuSupport(GameDataManager.PlayerConfig.IsTaikyokuSupport);
		return true;
	}

	private bool OnClickStickerAll()
	{
		SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
		GameDataManager.PlayerConfig.IsStickerAllDisp = true;
		m_mahjongPlayFlow.SetStickerOption(GameDataManager.PlayerConfig.IsStickerAllDisp);
		return true;
	}

	private bool OnClickStickerOne()
	{
		SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
		GameDataManager.PlayerConfig.IsStickerAllDisp = false;
		m_mahjongPlayFlow.SetStickerOption(GameDataManager.PlayerConfig.IsStickerAllDisp);
		return true;
	}

	public bool OnClick_Daiuchi()
	{
		if (GameDataManager.Player.IsPro())
		{
			return false;
		}
		m_mahjongPlayFlow.m_isDaiuchi = !m_mahjongPlayFlow.m_isDaiuchi;
		m_mahjongPlayFlow.DaiuchiText.SetActive(m_mahjongPlayFlow.m_isDaiuchi);
		return true;
	}

	private bool OnClickRule()
	{
		SetActive(active: false);
		ruleWindow.GetComponent<PlayRuleWindow>().Open(GameDataManager.Mahjong.TakuType, GameDataManager.Mahjong.EventType, m_mahjongPlayFlow);
		m_mahjongPlayFlow.m_isRuleDisplaying = true;
		return true;
	}

	private bool OnClickAnonymousOn()
	{
		Debug.Log("OnClickAnonymousOn");
		SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
		GameDataManager.Player.IsAnonymous = true;
		return true;
	}

	private bool OnClickAnonymousOff()
	{
		Debug.Log("OnClickAnonymousOff");
		SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
		GameDataManager.Player.IsAnonymous = false;
		return true;
	}

	private bool OnClickUKSLEDOn()
	{
		SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
		GameDataManager.PlayerConfig.IsUKSLEDAnimAllow = true;
		LEDManager.Instance.Apply(GameDataManager.PlayerConfig);
		return true;
	}

	private bool OnClickUKSLEDOff()
	{
		SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
		GameDataManager.PlayerConfig.IsUKSLEDAnimAllow = false;
		LEDManager.Instance.Apply(GameDataManager.PlayerConfig);
		return true;
	}

	private bool OnClickSubMonitorHomeLeft()
	{
		if (SystemConfigGameData.IsDualMonitor && !SubMonitorFlow.IsTransition && !SubMonitorFlow.IsTransitionWithLoad)
		{
			SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
			int count = SubMonitorFlow.MonitorStateDictInHome[SystemConfigGameData.CabinetType].Count;
			int value = (count + GameDataManager.PlayerConfig.SubMonitorInHome[SubMonitorFlow.SubMonitorCabinetIndex] - 1) % count;
			GameDataManager.PlayerConfig.SubMonitorInHome[SubMonitorFlow.SubMonitorCabinetIndex] = value;
			SubMonitorFlow.ChangeStateInSceneLoad("Home", isGameOption: true).Forget();
			return true;
		}
		return false;
	}

	private bool OnClickSubMonitorHomeRight()
	{
		if (SystemConfigGameData.IsDualMonitor && !SubMonitorFlow.IsTransition && !SubMonitorFlow.IsTransitionWithLoad)
		{
			SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
			int count = SubMonitorFlow.MonitorStateDictInHome[SystemConfigGameData.CabinetType].Count;
			int value = (GameDataManager.PlayerConfig.SubMonitorInHome[SubMonitorFlow.SubMonitorCabinetIndex] + 1) % count;
			GameDataManager.PlayerConfig.SubMonitorInHome[SubMonitorFlow.SubMonitorCabinetIndex] = value;
			SubMonitorFlow.ChangeStateInSceneLoad("Home", isGameOption: true).Forget();
			return true;
		}
		return false;
	}

	private bool OnClickSubMonitorMahjongLeft()
	{
		if (SystemConfigGameData.IsDualMonitor && !SubMonitorFlow.IsTransition && !SubMonitorFlow.IsTransitionWithLoad)
		{
			SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
			int count = SubMonitorFlow.MonitorStateDictInMahjong[SystemConfigGameData.CabinetType].Count;
			int value = (count + GameDataManager.PlayerConfig.SubMonitorInMahjong[SubMonitorFlow.SubMonitorCabinetIndex] - 1) % count;
			GameDataManager.PlayerConfig.SubMonitorInMahjong[SubMonitorFlow.SubMonitorCabinetIndex] = value;
			SubMonitorFlow.ChangeStateInSceneLoad("MahjongPlay", isGameOption: true).Forget();
			return true;
		}
		return false;
	}

	private bool OnClickSubMonitorMahjongRight()
	{
		if (SystemConfigGameData.IsDualMonitor && !SubMonitorFlow.IsTransition && !SubMonitorFlow.IsTransitionWithLoad)
		{
			SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
			int count = SubMonitorFlow.MonitorStateDictInMahjong[SystemConfigGameData.CabinetType].Count;
			int value = (GameDataManager.PlayerConfig.SubMonitorInMahjong[SubMonitorFlow.SubMonitorCabinetIndex] + 1) % count;
			GameDataManager.PlayerConfig.SubMonitorInMahjong[SubMonitorFlow.SubMonitorCabinetIndex] = value;
			SubMonitorFlow.ChangeStateInSceneLoad("MahjongPlay", isGameOption: true).Forget();
			return true;
		}
		return false;
	}

	private bool OnClickMouseCursorOn()
	{
		SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
		GameDataManager.PlayerConfig.IsCursorAnimAllow = true;
		return true;
	}

	private bool OnClickMouseCursorOff()
	{
		SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
		GameDataManager.PlayerConfig.IsCursorAnimAllow = false;
		return true;
	}

	private bool OnClickMaAnimationOn()
	{
		SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
		GameDataManager.PlayerConfig.IsProAnimation = true;
		return true;
	}

	private bool OnClickMaAnimationOff()
	{
		SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
		GameDataManager.PlayerConfig.IsProAnimation = false;
		return true;
	}

	private bool OnClickKeyboardOn()
	{
		SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
		GameDataManager.PlayerConfig.IsKeyboardPlayInStreaming = true;
		GameDataManager.PlayerConfig.IsKeyboardGuideInStreaming = true;
		return true;
	}

	private bool OnClickKeyboardGuideOff()
	{
		SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
		GameDataManager.PlayerConfig.IsKeyboardPlayInStreaming = true;
		GameDataManager.PlayerConfig.IsKeyboardGuideInStreaming = false;
		return true;
	}

	private bool OnClickKeyboardOff()
	{
		SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
		GameDataManager.PlayerConfig.IsKeyboardPlayInStreaming = false;
		GameDataManager.PlayerConfig.IsKeyboardGuideInStreaming = false;
		return true;
	}

	private bool OnClickUSBCharge()
	{
		if (!Bi2xInput.Instance.IsUsbChargerEnable())
		{
			SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
			base.gameObject.SetActive(value: false);
			usbChargeWindow.SetActive(value: true);
			if (m_mahjongPlayFlow != null)
			{
				m_mahjongPlayFlow.m_isRuleDisplaying = true;
			}
		}
		return true;
	}

	private bool OnClickUSBChargeYes()
	{
		Bi2xInput.Instance.EnableUsbCharger(is_open: true);
		SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
		usbChargeWindow.SetActive(value: false);
		if (m_mahjongPlayFlow != null)
		{
			m_mahjongPlayFlow.m_isRuleDisplaying = false;
		}
		return true;
	}

	private bool OnClickUSBChargeNo()
	{
		Bi2xInput.Instance.EnableUsbCharger(is_open: false);
		SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
		usbChargeWindow.SetActive(value: false);
		if (m_mahjongPlayFlow != null)
		{
			m_mahjongPlayFlow.m_isRuleDisplaying = false;
		}
		return true;
	}
}
