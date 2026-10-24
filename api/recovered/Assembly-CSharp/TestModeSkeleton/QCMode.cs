using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using IO;
using KAMUNITY;
using MFG.Types;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TestModeSkeleton;

public class QCMode : MonoBehaviour
{
	public delegate bool MenuOpenAndCloseFunc();

	private enum LampID
	{
		All,
		Auto,
		Left,
		Right,
		Front,
		Foot,
		UpRed,
		UpGreen,
		UpBlue,
		UpAll,
		ICCardRed,
		ICCardGreen,
		ICCardBlue,
		ICCardAll,
		Max
	}

	private enum Path
	{
		Router,
		Center,
		Server
	}

	private class ConnectionDisp
	{
		private readonly Text pathText;

		private readonly Text pointText;

		private readonly Text pingText;

		private readonly int keepalive;

		private readonly uint startSendCtr;

		private readonly uint startRecvCtr;

		public bool IsConnect { get; private set; }

		public ConnectionDisp(Text pathText, Text pointText, Text pingText, int keepalive)
		{
			this.pathText = pathText;
			this.pointText = pointText;
			this.pingText = pingText;
			this.keepalive = keepalive;
			AvsEa3.KeepaliveGetStatus(keepalive, out var status);
			startSendCtr = status.send_ctr;
			startRecvCtr = status.recv_ctr;
		}

		public void Update(string arrow)
		{
			if (keepalive > 0)
			{
				AvsEa3.KeepalivePing(keepalive);
				AvsEa3.KeepaliveGetStatus(keepalive, out var status);
				IsConnect = status.status != AvsTypes.AVS_KEEPALIVE_STATUS.DOWN;
				SetText(IsConnect, status.send_ctr - startSendCtr, status.recv_ctr - startRecvCtr, arrow);
			}
			else
			{
				IsConnect = false;
				SetText(IsConnect, 0u, 0u, arrow);
			}
		}

		private void SetText(bool isConnecting, uint pingSendCount, uint pingSuccessCount, string arrow)
		{
			if (isConnecting)
			{
				pathText.text = "<color=lime>" + arrow + "</color>";
				pointText.color = Color.green;
			}
			else
			{
				pathText.text = "<color=red>---</color>";
				pointText.color = Color.red;
			}
			float num = 1f * (float)pingSuccessCount / (float)((pingSendCount == 0) ? 1u : pingSendCount);
			if (float.IsNaN(num))
			{
				num = 0f;
			}
			pingText.text = $"{pingSuccessCount} / {pingSendCount} ({num:P0})";
		}
	}

	private const float VALID_INTERVAL_TIME = 0.3f;

	private const float VALID_LONG_PRESS_TIME = 0.8f;

	private const float INVALID_WAIT_TIME = 1f;

	private static readonly string On_ = "<color=red>ON</color>";

	private static readonly string Off = "<color=black>OFF</color>";

	[SerializeField]
	private GameObject root;

	[SerializeField]
	private GameObject exit;

	[SerializeField]
	[Header("バージョン")]
	private Text SoftIDText;

	[SerializeField]
	private Text QcModeVerText;

	[SerializeField]
	[Header("時間")]
	private Text timer;

	[SerializeField]
	private Text clockTimer;

	[SerializeField]
	[Header("セキュリティコード")]
	private Text m_licenceKeyCheck;

	[SerializeField]
	private Text m_accountKeyCheck;

	private bool m_isMahjongPlayLoading;

	[SerializeField]
	[Header("IO基板")]
	private Text m_ioStatus;

	private BUTTON[] iolist = new BUTTON[3]
	{
		BUTTON.TEST,
		BUTTON.SERVICE,
		BUTTON.COIN
	};

	[SerializeField]
	[Header("ボタン")]
	private Text[] m_buttonStatus;

	[SerializeField]
	[Header("コインブロッカー")]
	private Text m_CoinBlockerStatus;

	[SerializeField]
	private Button m_CoinBlockerButton;

	private bool IsCoinBlockerOpen = true;

	private bool IsCoinBlocker_ON;

	[SerializeField]
	[Header("コインカウンター")]
	private Text m_CoinCounter;

	[SerializeField]
	private Button m_CoinCounterResetButton;

	[SerializeField]
	private Button m_CoinCounterUpButton;

	private int m_CoinCounterNum;

	[SerializeField]
	[Header("USB Charger")]
	private Text m_usbChargerText;

	[SerializeField]
	private Button m_usbChargerButton;

	private bool m_usbChargerActive = true;

	[SerializeField]
	[Header("タッチパネル")]
	private Text[] m_touchpanel;

	[SerializeField]
	private RectTransform[] m_touchposition;

	private readonly Vector2 OutRange = new Vector2(-1000f, -1000f);

	[SerializeField]
	[Header("カードリーダー")]
	private Text m_cardreader;

	[SerializeField]
	private Text m_readerEnable;

	private bool m_cardReaderActive = true;

	[SerializeField]
	[Header("Lamp")]
	private Text[] m_lampTexts;

	private const int LAMP_MAX = 4;

	private bool[] m_lampActive = new bool[4] { true, true, true, true };

	private Color m_cardLedColor = Color.white;

	[SerializeField]
	private Text m_cardLedRText;

	[SerializeField]
	private Text m_cardLedGText;

	[SerializeField]
	private Text m_cardLedBText;

	private Color32 m_pillarLedColor = Color.white;

	[SerializeField]
	private Text m_pillarLedRText;

	[SerializeField]
	private Text m_pillarLedGText;

	[SerializeField]
	private Text m_pillarLedBText;

	[SerializeField]
	private InputField m_pillarRInput;

	[SerializeField]
	private InputField m_pillarGInput;

	[SerializeField]
	private InputField m_pillarBInput;

	[SerializeField]
	[Header("Lamp")]
	private Text[] m_autLampTexts;

	[SerializeField]
	[Tooltip("接続パス")]
	[Header("ネットワーク")]
	private Text[] pathTexts;

	[SerializeField]
	[Tooltip("接続ポイント")]
	private Text[] pointTexts;

	[SerializeField]
	[Tooltip("ping状態")]
	private Text[] pingTexts;

	[SerializeField]
	[Tooltip("接続状態")]
	private Text statusText;

	[SerializeField]
	[Header("サウンド")]
	private Text m_sound;

	[SerializeField]
	private Button m_soundVolumeDownButton;

	[SerializeField]
	private Button m_soundVolumeUpButton;

	[SerializeField]
	private Button m_soundPlayButton;

	[SerializeField]
	private Text m_soundVolumeText;

	[SerializeField]
	private Text m_soundButtonText;

	private bool IsSoundPlaying;

	private float sound_volume = 5f;

	private const float SoundMax = 30f;

	[SerializeField]
	[Header("ヘッドフォン")]
	private Text m_headphone;

	[SerializeField]
	private Button m_headphoneVolumeDownButton;

	[SerializeField]
	private Button m_headphoneVolumeUpButton;

	[SerializeField]
	private Text m_headphoneVolumeText;

	[SerializeField]
	private Text m_headphoneJackText;

	private int m_headphoneVolume = 5;

	private const int HEADPHONE_VOLUME_MAX = 30;

	private bool m_isExit;

	private MahjongPlayFlow m_mahjongPlayFlow;

	public MenuOpenAndCloseFunc OpenAndCloseFunc { get; set; } = () => (Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftControl)) && Input.GetKeyUp(KeyCode.D);


	private void TimerStart()
	{
		StartCoroutine(CoUpdateTimer());
	}

	private IEnumerator CoUpdateTimer()
	{
		DateTime startTime = DateTime.Now;
		while (true)
		{
			DateTime now = DateTime.Now;
			TimeSpan timeSpan = now - startTime;
			timer.text = timeSpan.ToString("dd\\.hh\\:mm\\:ss");
			clockTimer.text = $"{now:yyyy/MM/dd HH:mm:ss}";
			yield return null;
		}
	}

	private IEnumerator CoSecurityKeyCheck()
	{
		uint licFoundCount = 0u;
		uint licOkCount = 0u;
		uint accFoundCount = 0u;
		uint accOkCount = 0u;
		while (true)
		{
			yield return new WaitForSeconds(1f);
			DeviceManager instance = DeviceManager.Instance;
			if (instance == null)
			{
				break;
			}
			IkeyDevice ikeyDevice = instance.Ikey();
			if (ikeyDevice == null)
			{
				break;
			}
			Bootstrap.IKeyStatus keyStatus = ikeyDevice.keyStatus;
			if (keyStatus == null)
			{
				break;
			}
			Bootstrap.IKeyStatusItem license = keyStatus.license;
			if (license == null)
			{
				break;
			}
			Bootstrap.IKeyStatusItem account = keyStatus.account;
			if (account == null)
			{
				break;
			}
			licFoundCount++;
			if (license.available)
			{
				licOkCount++;
			}
			m_licenceKeyCheck.text = licOkCount * 100 / licFoundCount + "%(" + licOkCount + "/" + licFoundCount + ") Refind " + license.count_found;
			accFoundCount++;
			if (account.available)
			{
				accOkCount++;
			}
			m_accountKeyCheck.text = accOkCount * 100 / accFoundCount + "%(" + accOkCount + "/" + accFoundCount + ") Refind " + account.count_found;
		}
	}

	private void SecurityKeyCheckStart()
	{
		StartCoroutine(CoSecurityKeyCheck());
	}

	private void OnEnable()
	{
	}

	private void OnDisable()
	{
		Bi2xInput.Instance.ICReaderOff();
		SoundManager.SetGamePlayVolume();
	}

	private void UICamera_SetActive(bool isActive)
	{
		GameObject gameObject = GameObject.Find("UICamera");
		if ((bool)gameObject)
		{
			gameObject.GetComponent<Camera>().enabled = isActive;
			gameObject.GetComponent<AudioListener>().enabled = isActive;
		}
		GameObject gameObject2 = GameObject.Find("/TestModeMain/EventSystem");
		if ((bool)gameObject2)
		{
			gameObject2.GetComponent<EventSystem>().enabled = isActive;
		}
	}

	private async UniTask MahjongPlay_Load()
	{
		m_isMahjongPlayLoading = true;
		GameDataManager.Mahjong.IsHaifuPlay = true;
		AsyncOperation asyncOperation = SceneManager.LoadSceneAsync("MahjongPlay", LoadSceneMode.Additive);
		if (asyncOperation != null)
		{
			await asyncOperation;
		}
		GameObject gameObject = GameObject.Find("SceneFlow");
		m_mahjongPlayFlow = gameObject.GetComponent<MahjongPlayFlow>();
		m_isMahjongPlayLoading = false;
	}

	private async void Start()
	{
		ResidentFlow instance = ResidentFlow.GetInstance();
		instance.IsTestModeRequest = false;
		instance.IsTestModeEffective = false;
		instance.IsErrorModeEffective = false;
		LEDManager.Instance.Stop();
		LEDManager.Instance.AllowAnim(isAllow: false);
		if (SystemUtility.Instance.IsFactoryBoot)
		{
			exit.SetActive(value: false);
		}
		await MahjongPlay_Load();
		TimerStart();
		CoinBlockerStart();
		CoinCounterStart();
		USBChargerStart();
		LedStart();
		IcCardStart();
		NetworkStart();
		SecurityKeyCheckStart();
		SoundStart();
		HeadphoneStart();
		SoftIDText.text = SystemUtility.Instance.SoftID;
		QcModeVerText.text = "QC Mode Version 0.0.1";
		root.gameObject.SetActive(value: true);
	}

	private bool IsTimeOver(float lastTapTime)
	{
		return lastTapTime + 1f < Time.time;
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			Exit();
		}
		else if (!m_isExit && !m_isMahjongPlayLoading)
		{
			MahjongPlayFlow mahjongPlayFlow = m_mahjongPlayFlow;
			if ((object)mahjongPlayFlow != null && mahjongPlayFlow.m_taikyokuEnd)
			{
				SceneManager.UnloadSceneAsync("MahjongPlay");
				MahjongPlay_Load().Forget();
			}
		}
	}

	private void LateUpdate()
	{
		IoBoardUpdate();
		ButtonUpdate();
		HeadphoneUpadate();
		CoinBlockerUpdate();
		CoinCounterUpdate();
		USBChargerUpdate();
		TouchPanelUpdate();
	}

	private void IoBoardUpdate()
	{
		m_ioStatus.color = (Bi2xInput.IsError ? Color.red : Color.green);
		m_ioStatus.text = (Bi2xInput.IsError ? "NG" : "OK");
	}

	private void ButtonUpdate()
	{
		for (int i = 0; i < iolist.Length; i++)
		{
			if (Bi2xInput.IsPress(iolist[i]))
			{
				m_buttonStatus[i].text = On_;
			}
			else
			{
				m_buttonStatus[i].text = Off;
			}
		}
	}

	private void CoinBlockerStart()
	{
		Bi2xInput.Instance.ControlCoinBlocker(IsCoinBlockerOpen);
		m_CoinBlockerStatus.text = (IsCoinBlockerOpen ? "<color=red>OPEN</color>" : "<color=black>CLOSE</color>");
	}

	private void CoinBlockerUpdate()
	{
		if (IsCoinBlocker_ON)
		{
			if (IsCoinBlockerOpen)
			{
				Bi2xInput.Instance.ControlCoinBlocker(is_open: false);
				IsCoinBlockerOpen = false;
			}
			else
			{
				Bi2xInput.Instance.ControlCoinBlocker(is_open: true);
				IsCoinBlockerOpen = true;
			}
			m_CoinBlockerStatus.text = (IsCoinBlockerOpen ? "OPEN" : "CLOSE");
			IsCoinBlocker_ON = false;
		}
	}

	public void OnPushBlockerButton()
	{
		IsCoinBlocker_ON = !IsCoinBlocker_ON;
	}

	private void CoinCounterStart()
	{
		m_CoinCounterNum = 0;
		m_CoinCounter.text = m_CoinCounterNum.ToString();
	}

	private void CoinCounterUpdate()
	{
	}

	public void OnPushCoinCounterResetButton()
	{
		m_CoinCounterNum = 0;
		m_CoinCounter.text = m_CoinCounterNum.ToString();
	}

	public void OnPushCoinCounterUpButton()
	{
		m_CoinCounterNum++;
		m_CoinCounter.text = m_CoinCounterNum.ToString();
		Bi2xInput.Instance.AddCoinCounter(1u);
	}

	private void USBChargerStart()
	{
		m_usbChargerText.text = (m_usbChargerActive ? On_ : Off);
		Bi2xInput.Instance.EnableUsbCharger(m_usbChargerActive);
	}

	private void USBChargerUpdate()
	{
	}

	public void OnUSBChargerButton()
	{
		m_usbChargerActive = !m_usbChargerActive;
		m_usbChargerText.text = (m_usbChargerActive ? On_ : Off);
		Bi2xInput.Instance.EnableUsbCharger(m_usbChargerActive);
	}

	private void TouchPanelUpdate()
	{
		for (int i = 0; i < m_touchposition.Length; i++)
		{
			if (Bi2xInput.Instance.IsTouchPress())
			{
				Vector2 touchPosition = Bi2xInput.Instance.GetTouchPosition();
				m_touchpanel[i].text = "(" + touchPosition.x.ToString("0000") + "," + touchPosition.y.ToString("0000") + ")";
				m_touchposition[i].anchoredPosition = touchPosition;
			}
			else
			{
				m_touchpanel[i].text = "OFF";
				m_touchposition[i].anchoredPosition = OutRange;
			}
		}
	}

	private void IcCardStart()
	{
		StartCoroutine(CoIcCardCheck());
	}

	private IEnumerator CoIcCardCheck()
	{
		SetCardReader(m_cardReaderActive);
		while (true)
		{
			bool flag = false;
			string text = "";
			if (Bi2xInput.IsBi2xReady)
			{
				flag = Bi2xInput.Instance.IsCardOn();
				text = Bi2xInput.Instance.GetCardID();
			}
			m_cardreader.text = (flag ? ("<color=lime>OK[" + text + "]</color>") : "<color=black>NO CARD</color>");
			yield return null;
		}
	}

	private void SetCardReader(bool isActive)
	{
		m_cardReaderActive = isActive;
		m_readerEnable.text = (isActive ? On_ : Off);
		if (isActive)
		{
			Bi2xInput.Instance.ICReaderOn();
		}
		else
		{
			Bi2xInput.Instance.ICReaderOff();
		}
	}

	public void OnChangeIcCardReaderOnOff()
	{
		m_cardReaderActive = !m_cardReaderActive;
		SetCardReader(m_cardReaderActive);
	}

	private void LedStart()
	{
		SetLampAll(isActive: true);
		SetCardLed(m_cardLedColor);
		SetPillarLed(m_pillarLedColor);
		if (SystemUtility.Instance.IsFactoryBoot)
		{
			StartCoroutine(CoAutoLampCheck(0.8f));
		}
	}

	private IEnumerator CoAutoLampCheck(float seconds)
	{
		while (true)
		{
			foreach (LampID item in Lamps((SystemConfigGameData.CabinetType == CabinetType.None) ? CabinetType.HG : SystemConfigGameData.CabinetType))
			{
				SetLamp(item, isEnable: true);
				yield return new WaitForSeconds(seconds);
			}
		}
	}

	private IEnumerable<LampID> Lamps(CabinetType cabinetType)
	{
		return cabinetType switch
		{
			CabinetType.A => new LampID[2]
			{
				LampID.Left,
				LampID.Right
			}, 
			CabinetType.B => new LampID[4]
			{
				LampID.Left,
				LampID.Right,
				LampID.Front,
				LampID.Foot
			}, 
			CabinetType.C => new LampID[2]
			{
				LampID.Left,
				LampID.Right
			}, 
			CabinetType.HG => new LampID[8]
			{
				LampID.UpRed,
				LampID.UpGreen,
				LampID.UpBlue,
				LampID.UpAll,
				LampID.ICCardRed,
				LampID.ICCardGreen,
				LampID.ICCardBlue,
				LampID.ICCardAll
			}, 
			CabinetType.UKS => new LampID[12]
			{
				LampID.Left,
				LampID.Right,
				LampID.Front,
				LampID.Foot,
				LampID.UpRed,
				LampID.UpGreen,
				LampID.UpBlue,
				LampID.UpAll,
				LampID.ICCardRed,
				LampID.ICCardGreen,
				LampID.ICCardBlue,
				LampID.ICCardAll
			}, 
			_ => new LampID[12]
			{
				LampID.Left,
				LampID.Right,
				LampID.Front,
				LampID.Foot,
				LampID.UpRed,
				LampID.UpGreen,
				LampID.UpBlue,
				LampID.UpAll,
				LampID.ICCardRed,
				LampID.ICCardGreen,
				LampID.ICCardBlue,
				LampID.ICCardAll
			}, 
		};
	}

	private void SetLamp(LampID lampID, bool isEnable)
	{
		SetAllLamps(isEnable: false);
		SetLampText(lampID, isEnable);
		switch (lampID)
		{
		case LampID.Left:
			Bi2xInput.Instance.SetLamp(Bi2xInput.LampID.Lamp1, isEnable);
			Bi2xInput.Instance.SetTapeLED(Bi2xInput.TAPELED.TAPELED_LEFT_PANEL, isEnable ? Color.white : Color.black);
			break;
		case LampID.Right:
			Bi2xInput.Instance.SetLamp(Bi2xInput.LampID.Lamp2, isEnable);
			Bi2xInput.Instance.SetTapeLED(Bi2xInput.TAPELED.TAPELED_RIGHT_PANEL, isEnable ? Color.white : Color.black);
			break;
		case LampID.Front:
			Bi2xInput.Instance.SetLamp(Bi2xInput.LampID.Lamp3, isEnable);
			Bi2xInput.Instance.SetTapeLED(Bi2xInput.TAPELED.TAPELED_CTRL_PANEL, isEnable ? Color.white : Color.black);
			break;
		case LampID.Foot:
			Bi2xInput.Instance.SetButtonLED(isEnable ? Color.white : Color.black);
			break;
		case LampID.UpRed:
			Bi2xInput.Instance.SetPillarLED(isEnable ? Color.red : Color.black);
			Bi2xInput.Instance.SetTapeLED(Bi2xInput.TAPELED.TAPELED_CTRL_PANEL, isEnable ? Color.red : Color.black);
			Bi2xInput.Instance.SetTapeLED(Bi2xInput.TAPELED.TAPELED_LEFT_PANEL, isEnable ? Color.red : Color.black);
			Bi2xInput.Instance.SetTapeLED(Bi2xInput.TAPELED.TAPELED_RIGHT_PANEL, isEnable ? Color.red : Color.black);
			Bi2xInput.Instance.SetButtonLED(isEnable ? Color.red : Color.black);
			break;
		case LampID.UpGreen:
			Bi2xInput.Instance.SetPillarLED(isEnable ? Color.green : Color.black);
			Bi2xInput.Instance.SetTapeLED(Bi2xInput.TAPELED.TAPELED_CTRL_PANEL, isEnable ? Color.green : Color.black);
			Bi2xInput.Instance.SetTapeLED(Bi2xInput.TAPELED.TAPELED_LEFT_PANEL, isEnable ? Color.green : Color.black);
			Bi2xInput.Instance.SetTapeLED(Bi2xInput.TAPELED.TAPELED_RIGHT_PANEL, isEnable ? Color.green : Color.black);
			Bi2xInput.Instance.SetButtonLED(isEnable ? Color.green : Color.black);
			break;
		case LampID.UpBlue:
			Bi2xInput.Instance.SetPillarLED(isEnable ? Color.blue : Color.black);
			Bi2xInput.Instance.SetTapeLED(Bi2xInput.TAPELED.TAPELED_CTRL_PANEL, isEnable ? Color.blue : Color.black);
			Bi2xInput.Instance.SetTapeLED(Bi2xInput.TAPELED.TAPELED_LEFT_PANEL, isEnable ? Color.blue : Color.black);
			Bi2xInput.Instance.SetTapeLED(Bi2xInput.TAPELED.TAPELED_RIGHT_PANEL, isEnable ? Color.blue : Color.black);
			Bi2xInput.Instance.SetButtonLED(isEnable ? Color.blue : Color.black);
			break;
		case LampID.UpAll:
			Bi2xInput.Instance.SetPillarLED(isEnable ? Color.white : Color.black);
			Bi2xInput.Instance.SetTapeLED(Bi2xInput.TAPELED.TAPELED_CTRL_PANEL, isEnable ? Color.white : Color.black);
			Bi2xInput.Instance.SetTapeLED(Bi2xInput.TAPELED.TAPELED_LEFT_PANEL, isEnable ? Color.white : Color.black);
			Bi2xInput.Instance.SetTapeLED(Bi2xInput.TAPELED.TAPELED_RIGHT_PANEL, isEnable ? Color.white : Color.black);
			Bi2xInput.Instance.SetButtonLED(isEnable ? Color.white : Color.black);
			break;
		case LampID.ICCardRed:
			Bi2xInput.Instance.SetCardLed(isEnable ? Color.red : Color.black);
			break;
		case LampID.ICCardGreen:
			Bi2xInput.Instance.SetCardLed(isEnable ? Color.green : Color.black);
			break;
		case LampID.ICCardBlue:
			Bi2xInput.Instance.SetCardLed(isEnable ? Color.blue : Color.black);
			break;
		case LampID.ICCardAll:
			Bi2xInput.Instance.SetCardLed(isEnable ? Color.white : Color.black);
			break;
		case LampID.All:
		case LampID.Auto:
			break;
		}
	}

	private void SetAllLamps(bool isEnable)
	{
		for (int i = 2; i < 14; i++)
		{
			SetLampText((LampID)i, isEnable);
		}
		Bi2xInput.Instance.SetLamp(Bi2xInput.LampID.Lamp1, isEnable);
		Bi2xInput.Instance.SetLamp(Bi2xInput.LampID.Lamp2, isEnable);
		Bi2xInput.Instance.SetLamp(Bi2xInput.LampID.Lamp3, isEnable);
		Bi2xInput.Instance.SetLamp(Bi2xInput.LampID.Lamp4, isEnable);
		Bi2xInput.Instance.SetPillarLED(isEnable ? Color.white : Color.black);
		Bi2xInput.Instance.SetCardLed(isEnable ? Color.white : Color.black);
		Bi2xInput.Instance.SetTapeLED(Bi2xInput.TAPELED.TAPELED_CTRL_PANEL, isEnable ? Color.white : Color.black);
		Bi2xInput.Instance.SetTapeLED(Bi2xInput.TAPELED.TAPELED_LEFT_PANEL, isEnable ? Color.white : Color.black);
		Bi2xInput.Instance.SetTapeLED(Bi2xInput.TAPELED.TAPELED_RIGHT_PANEL, isEnable ? Color.white : Color.black);
		Bi2xInput.Instance.SetButtonLED(isEnable ? Color.white : Color.black);
	}

	private void SetLampText(LampID textID, bool isOn)
	{
		switch (textID)
		{
		case LampID.ICCardAll:
			m_autLampTexts[10].text = (isOn ? On_ : Off);
			m_autLampTexts[11].text = (isOn ? On_ : Off);
			m_autLampTexts[12].text = (isOn ? On_ : Off);
			break;
		case LampID.UpAll:
			m_autLampTexts[6].text = (isOn ? On_ : Off);
			m_autLampTexts[7].text = (isOn ? On_ : Off);
			m_autLampTexts[8].text = (isOn ? On_ : Off);
			break;
		default:
			if (m_autLampTexts[(int)textID] != null)
			{
				m_autLampTexts[(int)textID].text = (isOn ? On_ : Off);
			}
			break;
		}
	}

	private void SetLamp(int lampID, bool isActive)
	{
		m_lampActive[lampID] = isActive;
		m_lampTexts[lampID].text = (isActive ? On_ : Off);
		Bi2xInput.Instance.SetLamp((Bi2xInput.LampID)lampID, isActive);
	}

	private void ToggleLamp(int lampID)
	{
		m_lampActive[lampID] = !m_lampActive[lampID];
		SetLamp(lampID, m_lampActive[lampID]);
	}

	private void SetLampAll(bool isActive)
	{
		for (int i = 0; i < 4; i++)
		{
			SetLamp(i, isActive);
		}
	}

	private void SetCardLed(Color color)
	{
		m_cardLedColor = color;
		m_cardLedRText.text = ((color.r > 0f) ? On_ : Off);
		m_cardLedGText.text = ((color.g > 0f) ? On_ : Off);
		m_cardLedBText.text = ((color.b > 0f) ? On_ : Off);
		Bi2xInput.Instance.SetCardLed(m_cardLedColor);
	}

	private void ToggleCardLed(int colorIndex)
	{
		switch (colorIndex)
		{
		case 0:
			m_cardLedColor.r = ((!(m_cardLedColor.r > 0f)) ? 1 : 0);
			break;
		case 1:
			m_cardLedColor.g = ((!(m_cardLedColor.g > 0f)) ? 1 : 0);
			break;
		case 2:
			m_cardLedColor.b = ((!(m_cardLedColor.b > 0f)) ? 1 : 0);
			break;
		}
		SetCardLed(m_cardLedColor);
	}

	private void SetPillarLed(Color32 color)
	{
		m_pillarLedColor = color;
		m_pillarLedRText.text = color.r.ToString();
		m_pillarLedGText.text = color.g.ToString();
		m_pillarLedBText.text = color.b.ToString();
		Bi2xInput.Instance.SetPillarLED(m_pillarLedColor);
		Bi2xInput.Instance.SetTapeLED(Bi2xInput.TAPELED.TAPELED_CTRL_PANEL, m_pillarLedColor);
		Bi2xInput.Instance.SetTapeLED(Bi2xInput.TAPELED.TAPELED_LEFT_PANEL, m_pillarLedColor);
		Bi2xInput.Instance.SetTapeLED(Bi2xInput.TAPELED.TAPELED_RIGHT_PANEL, m_pillarLedColor);
	}

	private void TogglePillarLed(int colorIndex)
	{
		switch (colorIndex)
		{
		case 0:
			m_pillarLedColor.r = (byte)((m_pillarLedColor.r <= 0) ? 255u : 0u);
			break;
		case 1:
			m_pillarLedColor.g = (byte)((m_pillarLedColor.g <= 0) ? 255u : 0u);
			break;
		case 2:
			m_pillarLedColor.b = (byte)((m_pillarLedColor.b <= 0) ? 255u : 0u);
			break;
		}
		SetPillarLed(m_pillarLedColor);
	}

	public void OnInputEndPillarLED()
	{
		int num = int.Parse(m_pillarRInput.text);
		if (num > 255)
		{
			num = 255;
		}
		else if (num < 0)
		{
			num = 0;
		}
		int num2 = int.Parse(m_pillarGInput.text);
		if (num2 > 255)
		{
			num2 = 255;
		}
		else if (num2 < 0)
		{
			num2 = 0;
		}
		int num3 = int.Parse(m_pillarBInput.text);
		if (num3 > 255)
		{
			num3 = 255;
		}
		else if (num3 < 0)
		{
			num3 = 0;
		}
		m_pillarLedColor.r = (byte)num;
		m_pillarLedColor.g = (byte)num2;
		m_pillarLedColor.b = (byte)num3;
		SetPillarLed(m_pillarLedColor);
	}

	public void OnClickPillarR(int difference)
	{
		int num = m_pillarLedColor.r + difference;
		if (num > 255)
		{
			num = 255;
		}
		else if (num < 0)
		{
			num = 0;
		}
		m_pillarLedColor.r = (byte)num;
		SetPillarLed(m_pillarLedColor);
	}

	public void OnClickPillarG(int difference)
	{
		int num = m_pillarLedColor.g + difference;
		if (num > 255)
		{
			num = 255;
		}
		else if (num < 0)
		{
			num = 0;
		}
		m_pillarLedColor.g = (byte)num;
		SetPillarLed(m_pillarLedColor);
	}

	public void OnClickPillarB(int difference)
	{
		int num = m_pillarLedColor.b + difference;
		if (num > 255)
		{
			num = 255;
		}
		else if (num < 0)
		{
			num = 0;
		}
		m_pillarLedColor.b = (byte)num;
		SetPillarLed(m_pillarLedColor);
	}

	public void OnClickLamp(int buttonID)
	{
		ToggleLamp(buttonID);
	}

	public void OnClickLampAll()
	{
		if (m_lampActive.All((bool isactive) => isactive) && m_cardLedColor == Color.white && m_pillarLedColor == Color.white)
		{
			SetLampAll(isActive: false);
			SetCardLed(Color.black);
			SetPillarLed(Color.black);
		}
		else
		{
			SetLampAll(isActive: true);
			SetCardLed(Color.white);
			SetPillarLed(Color.white);
		}
	}

	public void OnClickCardLed(int colorIndex)
	{
		ToggleCardLed(colorIndex);
	}

	public void OnClickPillarLed(int colorIndex)
	{
		TogglePillarLed(colorIndex);
	}

	private void NetworkStart()
	{
		StartCoroutine(CoNetworkCheck());
	}

	private IEnumerator CoNetworkCheck()
	{
		string arrow = "";
		StartCoroutine(CoSetArrow(0.33f, delegate(string s)
		{
			arrow = s;
		}));
		statusText.text = "<color=red>NOT CONNECTED</color>";
		if (!GameUtility.IsKamunityBootEnable())
		{
			yield break;
		}
		List<ConnectionDisp> states = new List<ConnectionDisp>
		{
			new ConnectionDisp(pathTexts[0], pointTexts[0], pingTexts[0], AvsEa3.KeepaliveFindByName("router")),
			new ConnectionDisp(pathTexts[1], pointTexts[1], pingTexts[1], AvsEa3.KeepaliveFindByName("center")),
			new ConnectionDisp(pathTexts[2], pointTexts[2], pingTexts[2], AvsEa3.KeepaliveFindByName("server"))
		};
		while (true)
		{
			states.ForEach(delegate(ConnectionDisp s)
			{
				s.Update(arrow);
			});
			statusText.text = (states[2].IsConnect ? "<color=lime>CONNECTED</color>" : "<color=red>NOT CONNECTED</color>");
			yield return null;
		}
	}

	private IEnumerator CoSetArrow(float seconds, UnityAction<string> action)
	{
		string[] arrows = new string[3] { ">--", "->-", "-->" };
		int phase = 0;
		while (true)
		{
			action(arrows[phase]);
			phase = (int)Mathf.Repeat(phase + 1, arrows.Length);
			yield return new WaitForSeconds(seconds);
		}
	}

	private void ApplySpeakerVolume()
	{
		m_soundVolumeText.text = $"{sound_volume:00}/{30f}";
		SoundManager.SetSpeakerVolume(sound_volume / 30f);
		SoundManager.SetAttractVolume(1f);
	}

	private void SoundStart()
	{
		ApplySpeakerVolume();
		TogglePlaySound();
	}

	public void OnSoundVolumeUp()
	{
		if (sound_volume < 30f)
		{
			sound_volume += 1f;
		}
		else
		{
			sound_volume = 30f;
		}
		ApplySpeakerVolume();
	}

	public void OnSoundVolumeDown()
	{
		if (sound_volume > 0f)
		{
			sound_volume -= 1f;
		}
		else
		{
			sound_volume = 0f;
		}
		ApplySpeakerVolume();
	}

	public void TogglePlaySound()
	{
		if (!IsSoundPlaying)
		{
			m_soundButtonText.text = "■";
			IsSoundPlaying = true;
		}
		else
		{
			m_soundButtonText.text = "▶";
			IsSoundPlaying = false;
		}
	}

	private void ApplyHeadPhoneVolume()
	{
		m_headphoneVolumeText.text = $"{m_headphoneVolume:00}/{30}";
		SoundManager.SetHeadPhoneVolume((float)m_headphoneVolume / 30f);
	}

	private void HeadphoneStart()
	{
		ApplyHeadPhoneVolume();
		ModifyHeadphoneJack();
	}

	private void HeadphoneUpadate()
	{
		ModifyHeadphoneJack();
	}

	private void ModifyHeadphoneJack()
	{
		if (Bi2xInput.Instance.IsHeadphoneJacking())
		{
			m_headphoneJackText.text = On_;
		}
		else
		{
			m_headphoneJackText.text = Off;
		}
	}

	public void OnHeadphoneVolumeUp()
	{
		if (m_headphoneVolume < 30)
		{
			m_headphoneVolume++;
		}
		else
		{
			m_headphoneVolume = 30;
		}
		ApplyHeadPhoneVolume();
	}

	public void OnHeadphoneVolumeDown()
	{
		if (m_headphoneVolume > 0)
		{
			m_headphoneVolume--;
		}
		else
		{
			m_headphoneVolume = 0;
		}
		ApplyHeadPhoneVolume();
	}

	public void OnPushExit()
	{
		Exit();
	}

	public async void Exit()
	{
		if (!m_isExit)
		{
			m_isExit = true;
			Bi2xInput.Instance.ControlCoinBlocker(is_open: false);
			SetLampAll(isActive: false);
			SetCardLed(Color.black);
			SetPillarLed(Color.black);
			SoundManager.StopAll().Forget();
			await CoExit();
		}
	}

	private async UniTask CoExit()
	{
		KAMUNITY.Logger.Log("exit scene : " + base.gameObject.scene.name);
		m_mahjongPlayFlow.KikenRequenst();
		await UniTask.WaitUntil(() => m_mahjongPlayFlow.m_taikyokuEnd);
		await SceneManager.UnloadSceneAsync("MahjongPlay");
		await SceneManager.UnloadSceneAsync(base.gameObject.scene);
		LEDManager.Instance.AllowAnim(isAllow: true);
		LEDManager.Instance.Stop();
	}
}
