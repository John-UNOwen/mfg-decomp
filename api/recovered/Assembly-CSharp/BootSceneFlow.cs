using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using IO;
using KAMUNITY;
using Konami.Aio;
using MFG.Types;
using SYSTEM;
using TMPro;
using TestModeSkeleton;
using UnityEngine;

public class BootSceneFlow : BaseFlow
{
	private enum RESULT
	{
		CHECKING,
		OK,
		NG,
		PASS
	}

	private class ItemBase
	{
		protected BootSceneFlow m_parent;

		protected int m_step;

		public string Name { get; }

		public string Message { get; protected set; }

		public RESULT Result { get; protected set; }

		public ERROR ErrorCode { get; protected set; }

		public uint ErrorOption { get; protected set; }

		public ItemBase(string name, BootSceneFlow scene = null)
		{
			Name = name;
			m_parent = scene;
		}

		public virtual void Update()
		{
		}
	}

	private class IOItem : ItemBase
	{
		private enum STEP
		{
			IO_INIT,
			IO_INIT_WAIT,
			HEAD_PHONE_INIT,
			ERROR,
			END
		}

		private bool m_isTouchPanelOK;

		private bool m_isIOOK;

		private bool _touchInitialized;

		public IOItem()
			: base("I/O")
		{
		}

		public override void Update()
		{
			Debug.Log($"[{DateTime.Now}]IOItem:Update begin step:{m_step}");
			if (GameUtility.IsCloudStreaming)
			{
				base.Result = RESULT.OK;
				m_isTouchPanelOK = Bi2xInput.Instance.TouchInit();
				_touchInitialized = true;
				return;
			}
			if (!GameUtility.IsIOEnable)
			{
				base.Result = RESULT.PASS;
				Debug.Log($"[{DateTime.Now}]IOItem:Update end step:{m_step} RESULT.PASS");
				return;
			}
			switch ((STEP)m_step)
			{
			case STEP.IO_INIT:
			{
				Bi2xInput.Instance.Initialize((aioIobBi2aVFG.MECHTYPE)SystemConfigGameData.CabinetType);
				DeviceManager.Instance.Initialize();
				Bi2xInput instance = Bi2xInput.Instance;
				m_isTouchPanelOK = true;
				if (!_touchInitialized && (SystemConfigGameData.CabinetType == CabinetType.B || SystemConfigGameData.CabinetType == CabinetType.C))
				{
					if (!DeviceManager.Instance.Ikey().IsFactoryBoot)
					{
						m_isTouchPanelOK = instance.TouchInit();
					}
					Cursor.visible = false;
				}
				m_step = 1;
				break;
			}
			case STEP.IO_INIT_WAIT:
				if (Bi2xInput.IsError)
				{
					m_isIOOK = false;
					m_step = 3;
				}
				else if (Bi2xInput.IsBi2xReady && SystemConfigGameData.CabinetType == (CabinetType)Bi2xInput.Instance.GetMechType())
				{
					m_isIOOK = true;
					m_step = 2;
				}
				break;
			case STEP.HEAD_PHONE_INIT:
				if (SystemConfigGameData.CabinetType == CabinetType.B || SystemConfigGameData.CabinetType == CabinetType.C || SystemConfigGameData.CabinetType == CabinetType.HG || SystemConfigGameData.CabinetType == CabinetType.UKS || SystemConfigGameData.CabinetType == CabinetType.TBS || SystemConfigGameData.CabinetType == CabinetType.UJK)
				{
					Bi2xInput.Instance.SetHeadPhoneVolume(1f);
				}
				if (m_isIOOK && m_isTouchPanelOK)
				{
					base.Result = RESULT.OK;
					m_step = 4;
				}
				else
				{
					m_step = 3;
				}
				break;
			case STEP.ERROR:
				if (!m_isIOOK)
				{
					base.ErrorCode = ERROR.IO;
					base.ErrorOption = Bi2xInput.ErrorCode;
					base.Result = RESULT.NG;
				}
				else if (!m_isTouchPanelOK)
				{
					base.ErrorCode = ERROR.TOUCH_PANEL;
					base.ErrorOption = 0u;
					base.Result = RESULT.NG;
				}
				m_step = 4;
				break;
			}
			Debug.Log($"[{DateTime.Now}]IOItem:Update end step:{m_step}");
		}
	}

	private class CardReaderItem : ItemBase
	{
		public CardReaderItem()
			: base("IC CARD")
		{
		}

		public override void Update()
		{
			Debug.Log($"[{DateTime.Now}]CardReaderItem:Update begin");
			if (SystemUtility.Instance.IsFactoryBoot)
			{
				base.Result = RESULT.PASS;
				Debug.Log($"[{DateTime.Now}]CardReaderItem:Update end RESULT.PASS");
				return;
			}
			if (GameUtility.IsIOEnable)
			{
				base.Result = RESULT.OK;
				if (!Bi2xInput.Instance.m_icCardReader.IsConnected)
				{
					base.Result = RESULT.NG;
					base.ErrorCode = ERROR.IC_CARD;
					base.ErrorOption = 0u;
				}
			}
			else
			{
				base.Result = RESULT.PASS;
			}
			Debug.Log($"[{DateTime.Now}]CardReaderItem:Update end");
		}
	}

	private class IkeyItem : ItemBase
	{
		public IkeyItem(BootSceneFlow scene)
			: base("SECURITY", scene)
		{
		}

		public override void Update()
		{
			Debug.Log($"[{DateTime.Now}]IkeyItem:Update begin");
			if (!GameUtility.IsIOEnable || GameUtility.IsCloudStreaming)
			{
				base.Result = RESULT.PASS;
				Debug.Log($"[{DateTime.Now}]IkeyItem:Update end RESULT.PASS");
				return;
			}
			if (Bi2xInput.IsPress(BUTTON.SERVICE))
			{
				m_parent.DispFactorySystemId();
			}
			IkeyError ikeyError = DeviceManager.Instance.Ikey().CheckSecurityKeyOnBoot();
			if (GameUtility.IsKamunityBootEnable())
			{
				if (ikeyError == IkeyError.OK)
				{
					base.Result = RESULT.OK;
				}
				else
				{
					base.Result = RESULT.NG;
					base.ErrorCode = ERROR.SECURITY;
					base.ErrorOption = (uint)ikeyError;
				}
			}
			else
			{
				base.Result = RESULT.PASS;
			}
			Debug.Log($"[{DateTime.Now}]IkeyItem:Update end");
		}
	}

	private class GameDataItem : ItemBase
	{
		public GameDataItem()
			: base("GAME DATA")
		{
		}

		public override void Update()
		{
			Debug.Log($"[{DateTime.Now}]GameDataItem:Update begin");
			base.Result = RESULT.OK;
			Debug.Log($"[{DateTime.Now}]GameDataItem:Update end");
		}
	}

	private class BackupItem : ItemBase
	{
		private enum STATE
		{
			INIT,
			DELETE_WARNING_INIT,
			DELETE_WARNING_WAIT,
			DELETE,
			DELETE_END,
			TERM
		}

		private float m_backupDataTimer = 3f;

		private STATE m_state;

		public BackupItem(BootSceneFlow scene)
			: base("BACKUP", scene)
		{
		}

		public override void Update()
		{
			Debug.Log($"[{DateTime.Now}]GameDataItem:Update begin state:{m_state}");
			switch (m_state)
			{
			case STATE.INIT:
			{
				BackupDataManager instance = BackupDataManager.Instance;
				instance.Initialize();
				instance.Load();
				BackupDataManager.BACKUP_KIND bACKUP_KIND = instance.IsError();
				if (bACKUP_KIND < BackupDataManager.BACKUP_KIND.NUM)
				{
					base.ErrorCode = ERROR.BACKUP_DATA;
					base.ErrorOption = (uint)(bACKUP_KIND + 1);
				}
				m_state = (Bi2xInput.IsPress(BUTTON.TEST) ? STATE.DELETE_WARNING_INIT : STATE.TERM);
				break;
			}
			case STATE.DELETE_WARNING_INIT:
				m_parent.m_superFactoryText.transform.Find("Message").GetComponent<TextMeshProUGUI>().text = "PUSHING TEST BUTTON CONTINUOUS.\nSETTING DATA WILL BE CLEAR.";
				m_parent.m_superFactoryText.SetActive(value: true);
				m_state = STATE.DELETE_WARNING_WAIT;
				break;
			case STATE.DELETE_WARNING_WAIT:
				if (Bi2xInput.IsPress(BUTTON.TEST))
				{
					if (m_backupDataTimer <= 0f)
					{
						m_parent.m_superFactoryText.transform.Find("Message").GetComponent<TextMeshProUGUI>().text = "INITIALIZING THE BACKUP DATA COMPLETE.\nPLEASE TURN OFF THE SWITCH AND REBOOT.";
						m_state = STATE.DELETE;
					}
					m_backupDataTimer -= Time.deltaTime;
				}
				else
				{
					m_parent.m_superFactoryText.SetActive(value: false);
					m_state = STATE.TERM;
				}
				break;
			case STATE.DELETE:
			{
				CabinetManager.SuperAllFactoryAndSave();
				BackupDataManager.Instance.ServiceButtonLog().Clear();
				BackupDataManager.Instance.Recreate();
				FileDeleteUtility.DeleteDirectory(GetDir("dev/raw/ab"));
				FileDeleteUtility.DeleteDirectory(GetDir("dev/raw/image"));
				BackupDataManager.BACKUP_KIND bACKUP_KIND = BackupDataManager.Instance.IsError();
				if (bACKUP_KIND < BackupDataManager.BACKUP_KIND.NUM)
				{
					base.ErrorCode = ERROR.BACKUP_DATA;
					base.ErrorOption = (uint)(bACKUP_KIND + 1);
				}
				m_state = STATE.DELETE_END;
				break;
			}
			case STATE.TERM:
				base.Result = ((base.ErrorCode == ERROR.NONE) ? RESULT.OK : RESULT.NG);
				break;
			}
			Debug.Log($"[{DateTime.Now}]GameDataItem:Update end state:{m_state}");
		}

		private string GetDir(string path)
		{
			return path;
		}
	}

	private class NetworkItem : ItemBase
	{
		public NetworkItem()
			: base("NETWORK")
		{
		}

		public override void Update()
		{
			Debug.Log($"[{DateTime.Now}]NetworkItem:Update begin");
			if (!GameUtility.IsKamunityBootEnable())
			{
				base.Result = RESULT.PASS;
				Debug.Log($"[{DateTime.Now}]NetworkItem:Update end RESULT.PASS");
				return;
			}
			if (EagCore.Instance.Init() || EagCore.Instance.Code == 2012)
			{
				base.Result = RESULT.OK;
			}
			else
			{
				base.Result = RESULT.NG;
				base.ErrorCode = (ERROR)EagCore.Instance.Code;
				base.ErrorOption = (uint)EagCore.Instance.SubCode;
			}
			Debug.Log($"[{DateTime.Now}]NetworkItem:Update end");
		}
	}

	[SerializeField]
	private TextMeshProUGUI m_softID;

	[SerializeField]
	private TextMeshProUGUI m_text;

	[SerializeField]
	[Range(100f, 1000f)]
	private int m_animationSpeed = 200;

	[SerializeField]
	private GameObject m_superFactoryText;

	[SerializeField]
	private TextMeshProUGUI m_factorySystemId;

	private bool m_isOnExit;

	private string serial_id = "SOFTWARE ID <indent=42%>{0}</indent>\nSYSTEM ID <indent=42%>{1}</indent>\nHARDWARE ID <indent=42%>{2}</indent>";

	private readonly string[] checking = new string[3] { ".  ", ".. ", "..." };

	private readonly string[] resultString = new string[4] { "...", "<color=green>OK</color>", "<color=red>NG</color>", "<color=yellow>PASS</color>" };

	private List<ItemBase> items;

	private int itemIndex;

	private bool redraw;

	private uint animationTimer;

	private int animationIndex;

	private uint waitTimer;

	private const uint WAIT_TIME = 500u;

	private int _updateFrameCount;

	private Dictionary<AvsTypes.EA3_BOOT_CODE, ERROR_LEVEL> m_networkErrorTransitionInfos = new Dictionary<AvsTypes.EA3_BOOT_CODE, ERROR_LEVEL>
	{
		{
			AvsTypes.EA3_BOOT_CODE.DHCP_ERROR,
			ERROR_LEVEL.ERROR_STOP
		},
		{
			AvsTypes.EA3_BOOT_CODE.SERVICES_ERROR,
			ERROR_LEVEL.ERROR_STOP
		},
		{
			AvsTypes.EA3_BOOT_CODE.PCBTRACKER_ERROR,
			ERROR_LEVEL.ERROR_STOP
		},
		{
			AvsTypes.EA3_BOOT_CODE.EA3_BOOT_CODE_SERVICES_FORBIDDEN,
			ERROR_LEVEL.ERROR_STOP
		},
		{
			AvsTypes.EA3_BOOT_CODE.PCBTRACKER_FORBIDDEN,
			ERROR_LEVEL.ERROR_UNCANCELABLE
		},
		{
			AvsTypes.EA3_BOOT_CODE.PCBTRACKER_SERVEND,
			ERROR_LEVEL.ERROR_UNCANCELABLE
		},
		{
			AvsTypes.EA3_BOOT_CODE.MESSAGE_ERROR,
			ERROR_LEVEL.ERROR_STOP
		},
		{
			AvsTypes.EA3_BOOT_CODE.POSEVENT_ERROR,
			ERROR_LEVEL.ERROR_STOP
		},
		{
			AvsTypes.EA3_BOOT_CODE.FACILITY_ERROR,
			ERROR_LEVEL.ERROR_STOP
		},
		{
			AvsTypes.EA3_BOOT_CODE.PCBTRACKER_CONFIRM,
			ERROR_LEVEL.ERROR_STOP
		},
		{
			AvsTypes.EA3_BOOT_CODE.PCBTRACKER_REVISION,
			ERROR_LEVEL.ERROR_UNCANCELABLE
		},
		{
			AvsTypes.EA3_BOOT_CODE.PCBTRACKER_CONTACT,
			ERROR_LEVEL.ERROR_UNCANCELABLE
		},
		{
			AvsTypes.EA3_BOOT_CODE.APSMANAGER_ERROR,
			ERROR_LEVEL.ERROR_UNCANCELABLE
		},
		{
			AvsTypes.EA3_BOOT_CODE.APSMANAGER_CONTACT,
			ERROR_LEVEL.ERROR_UNCANCELABLE
		},
		{
			AvsTypes.EA3_BOOT_CODE.APSMANAGER_SUPPLY,
			ERROR_LEVEL.ERROR_UNCANCELABLE
		},
		{
			AvsTypes.EA3_BOOT_CODE.PCBEVENT_QUEUE,
			ERROR_LEVEL.ERROR_UNCANCELABLE
		},
		{
			AvsTypes.EA3_BOOT_CODE.APSMANAGER_MAINTE,
			ERROR_LEVEL.ERROR_UNCANCELABLE
		}
	};

	private BootSceneFlow()
	{
		_enableDebugLogin = false;
	}

	protected override void Awake()
	{
		AttatchCamera().Forget();
	}

	private async UniTask AttatchCamera()
	{
		await UniTask.WaitUntil(() => ResidentFlow.IsAwaked);
		UIAttachCam componentInChildren = GetComponentInChildren<UIAttachCam>(includeInactive: true);
		if (componentInChildren != null)
		{
			componentInChildren.Attatch(ResidentFlow.GetInstance().MainCamera(isForceLower: true));
		}
	}

	protected override async UniTask OnInitialize(CancellationToken cancellationToken = default(CancellationToken))
	{
		ResidentFlow.GetInstance().IsForceTestModeEffective = false;
		if (SystemConfigGameData.IsDualMonitor)
		{
			QualitySettings.vSyncCount = 1;
			await ResidentFlow.GetInstance().ActivateSecondDisplay();
			await UniTask.DelayFrame(1);
			Display.displays[1].Activate();
		}
		Screen.fullScreen = false;
		await UniTask.DelayFrame(1);
		if (GameUtility.IsCloudStreaming)
		{
			Debug.Log("BootScene SetResolution");
			Screen.SetResolution(1920, 1080, FullScreenMode.Windowed);
		}
		else
		{
			Screen.fullScreen = true;
			await UniTask.DelayFrame(1);
		}
		Debug.Log($"[{DateTime.Now}]BootSceneFlow:Start begin");
		SystemUtility.Instance.Initialize();
		m_softID.text = SystemUtility.Instance.SoftID;
		items = new List<ItemBase>();
		items.Add(new IOItem());
		if (!GameUtility.IsCloudStreaming)
		{
			items.Add(new IkeyItem(this));
		}
		items.Add(new BackupItem(this));
		items.Add(new GameDataItem());
		if (!GameUtility.IsCloudStreaming)
		{
			items.Add(new CardReaderItem());
		}
		items.Add(new NetworkItem());
		itemIndex = 0;
		redraw = true;
		Debug.Log($"[{DateTime.Now}]BootSceneFlow:Start end");
	}

	protected override void OnUpdate()
	{
		int num = _updateFrameCount++;
		Debug.Log($"[{DateTime.Now}]BootSceneFlow:Update begin frame:{num}");
		if (m_isOnExit)
		{
			Debug.Log($"[{DateTime.Now}]BootSceneFlow:Update end frame:{num} m_isOnExit:true");
			return;
		}
		uint num2 = (uint)(Time.deltaTime * 1000f);
		animationTimer += num2;
		int num3 = (int)animationTimer / m_animationSpeed % checking.Length;
		if (animationIndex != num3)
		{
			animationIndex = num3;
			redraw = true;
		}
		if (itemIndex < items.Count)
		{
			if (waitTimer < 500)
			{
				waitTimer += num2;
			}
			else
			{
				items[itemIndex].Update();
				if (items[itemIndex].Result != 0)
				{
					redraw = true;
					itemIndex++;
					waitTimer = 0u;
				}
			}
			if (redraw)
			{
				RenderMenu();
				// Rebuild-only observability: this headless harness cannot screenshot the boot
				// checklist (D3D swapchain captures black), so mirror the TMP text to the log.
				if (m_text != null)
				{
					Debug.Log("BOOTCHECKLIST\n" + m_text.text);
				}
				redraw = false;
			}
			Debug.Log($"[{DateTime.Now}]BootSceneFlow:Update end frame:{num}");
			return;
		}
		ApplyDeviceSetting();
		bool flag = false;
		for (int i = 0; i < items.Count; i++)
		{
			if (items[i].Result != RESULT.NG || (items[i].Name == "NETWORK" && SystemUtility.Instance.IsFactoryBoot))
			{
				continue;
			}
			if (items[i].Name == "NETWORK")
			{
				EagCore.Ea3GetBootStatus(out var boot_code, out var _);
				if (boot_code == AvsTypes.EA3_BOOT_CODE.PCBTRACKER_CONFIRM)
				{
					continue;
				}
			}
			if (items[i].Name == "NETWORK")
			{
				ERROR_LEVEL networkErrorTrasition = GetNetworkErrorTrasition((AvsTypes.EA3_BOOT_CODE)items[i].ErrorCode);
				ErrorControl.SetError(items[i].ErrorCode, items[i].ErrorOption, networkErrorTrasition);
			}
			else if (items[i].Name == "SECURITY")
			{
				ErrorControl.SetError(items[i].ErrorCode, items[i].ErrorOption, ERROR_LEVEL.HALT);
			}
			else
			{
				ErrorControl.SetError(items[i].ErrorCode, items[i].ErrorOption);
			}
			flag = true;
			break;
		}
		if (!flag)
		{
			if (ErrorControl.ResumeLastError())
			{
				ResidentFlow.GetInstance().IsErrorModeRequest = true;
				OnExit();
				Debug.Log($"[{DateTime.Now}]BootSceneFlow:Update end frame:{num} ResumeLastError:true");
			}
			else
			{
				OnExit();
				Debug.Log($"[{DateTime.Now}]BootSceneFlow:Update end frame:{num} ResumeLastError:false");
			}
		}
		else
		{
			ResidentFlow.GetInstance().IsErrorModeRequest = true;
			OnExit();
			Debug.Log($"[{DateTime.Now}]BootSceneFlow:Update end frame:{num} failed:true");
		}
	}

	private void OnExit()
	{
		Debug.Log("BootSceneFlow.OnExit()");
		string environmentVariable = Environment.GetEnvironmentVariable("VFG_WATCH_DOG");
		Debug.Log("VFG_WATCH_DOG:" + environmentVariable);
		if (environmentVariable != null && environmentVariable == "OFF")
		{
			Bi2xInput.Instance.SetWatchDogTimer(0);
		}
		else
		{
			Bi2xInput.Instance.SetWatchDogTimer(6000);
		}
		m_isOnExit = true;
		ResidentFlow instance = ResidentFlow.GetInstance();
		instance.IsTestModeRequest = false;
		instance.IsTestModeEffective = true;
		instance.IsErrorModeEffective = true;
		instance.IsAppTerminateEffective = GameUtility.IsKonastaOrNetcafeBehavior;
		instance.IsAppTerminateConfirmRequest = false;
		instance.IsForceTestModeEffective = true;
		if (GameUtility.IsKamunityBootEnable())
		{
			AvsEa3.KeepalivePing(AvsEa3.KeepaliveFindByName("router"));
			AvsEa3.KeepalivePing(AvsEa3.KeepaliveFindByName("center"));
			AvsEa3.KeepalivePing(AvsEa3.KeepaliveFindByName("server"));
		}
		if (SystemUtility.Instance.IsFactoryBoot)
		{
			ResidentFlow.GetInstance().LoadScene("QCMode");
		}
		else
		{
			ResidentFlow.GetInstance().LoadScene("Logo");
		}
	}

	private void RenderMenu()
	{
		if (m_text == null)
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		string value = "CHECKING";
		stringBuilder.Append("<indent=45%/>");
		stringBuilder.Append(value);
		stringBuilder.AppendLine();
		stringBuilder.AppendLine();
		stringBuilder.AppendLine();
		for (int i = 0; i < items.Count; i++)
		{
			value = items[i].Name;
			if (items[i].Result == RESULT.CHECKING)
			{
				stringBuilder.AppendFormat("<indent=30%/>{0}<indent=65%/>{1}", value, checking[animationIndex]);
			}
			else
			{
				stringBuilder.AppendFormat("<indent=30%/>{0}<indent=65%/>{1}", value, resultString[(int)items[i].Result]);
			}
			stringBuilder.AppendLine();
			stringBuilder.AppendLine();
		}
		if (itemIndex < items.Count && items[itemIndex].Message != null)
		{
			stringBuilder.AppendLine();
			stringBuilder.Append(items[itemIndex].Message);
		}
		m_text.text = stringBuilder.ToString();
	}

	private void ApplyDeviceSetting()
	{
	}

	private void DispFactorySystemId()
	{
		m_factorySystemId.text = string.Format(serial_id, SystemUtility.Instance.SoftID, SystemUtility.Instance.SystemID, SystemUtility.Instance.HardwareID);
	}

	private ERROR_LEVEL GetNetworkErrorTrasition(AvsTypes.EA3_BOOT_CODE code)
	{
		if (m_networkErrorTransitionInfos.ContainsKey(code))
		{
			return m_networkErrorTransitionInfos[code];
		}
		return ERROR_LEVEL.ERROR_STOP;
	}
}
