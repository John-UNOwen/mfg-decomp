using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using IO;
using KAMUNITY;
using Konami.Acc;
using Konami.Aio;
using TCSIO;
using UnityEngine;

public class Bi2xInputVFG : Bi2xInput
{
	private enum INITSTEP
	{
		Init,
		ReadyWait,
		MechTypeConfirmWait,
		Recovery,
		ErrorIntervalWait,
		Idle,
		Failed
	}

	private delegate void de_getInput(BUTTON type, ref bool push, ref float val);

	private class InputElement
	{
		private InputState m_inputState;

		private de_getInput m_ioInputGetter;

		private de_getInput m_padInputGetter;

		private de_getInput m_devInputGetter;

		public BUTTON BtnType { get; private set; } = BUTTON.NUM;


		public InputState Input => m_inputState;

		public InputElement(BUTTON type, de_getInput ioInput, de_getInput padInput = null, de_getInput devInput = null)
		{
			BtnType = type;
			m_ioInputGetter = ioInput;
			m_padInputGetter = padInput;
			m_devInputGetter = devInput;
		}

		public void UpdateAll()
		{
			bool flag = false;
			float val = 0f;
			if (m_ioInputGetter != null)
			{
				bool push = false;
				float val2 = 0f;
				m_ioInputGetter(BtnType, ref push, ref val2);
				flag = push;
				val = val2;
			}
			if (m_padInputGetter != null)
			{
				bool push2 = false;
				float val3 = 0f;
				m_padInputGetter(BtnType, ref push2, ref val3);
				flag = flag || push2;
				if (val3 != 0f)
				{
					val = val3;
				}
			}
			if (m_devInputGetter != null)
			{
				bool push3 = false;
				float val4 = 0f;
				m_devInputGetter(BtnType, ref push3, ref val4);
				flag = flag || push3;
				if (val4 != 0f)
				{
					val = val4;
				}
			}
			m_inputState.val = val;
			m_inputState.UpdatePushFlag(flag);
		}

		public void UpdateIoKey()
		{
			bool flag = false;
			float val = 0f;
			if (m_ioInputGetter != null)
			{
				bool push = false;
				float val2 = 0f;
				m_ioInputGetter(BtnType, ref push, ref val2);
				flag = push;
				val = val2;
			}
			if (m_devInputGetter != null)
			{
				bool push2 = false;
				float val3 = 0f;
				m_devInputGetter(BtnType, ref push2, ref val3);
				flag = flag || push2;
				if (val3 != 0f)
				{
					val = val3;
				}
			}
			m_inputState.val = val;
			m_inputState.UpdatePushFlag(flag);
		}

		public void UpdateIo()
		{
			bool bPush = false;
			float val = 0f;
			if (m_ioInputGetter != null)
			{
				bool push = false;
				float val2 = 0f;
				m_ioInputGetter(BtnType, ref push, ref val2);
				bPush = push;
				val = val2;
			}
			m_inputState.val = val;
			m_inputState.UpdatePushFlag(bPush);
		}
	}

	private class BioContext
	{
		public aioIobBi2aVFG.MECHTYPE MechType;

		public float HeadPhoneVolumeRate;

		public bool IsBlockerOpen;

		public ushort WatchDogTimerCount;

		public bool UsbChargerEnable;

		public Color32 CardLedColor;

		public Color32 PillarLedColor;

		public bool[] LampEnables = new bool[Enum.GetValues(typeof(LampID)).Length];

		public uint CoinCounterIncrement;

		public int TotalCoinCount;

		public int m_prevCoinCount;

		public void ModifyTotalCoinCount(int currentCoinCount)
		{
			int num = currentCoinCount - m_prevCoinCount;
			if (num < 0)
			{
				num += 64;
				Debug.Log("CoinCounterが1周しました");
			}
			if (currentCoinCount != m_prevCoinCount)
			{
				Debug.Log($"[{DateTime.Now}]ModifyTotalCoinCount: current:{currentCoinCount} prev:{m_prevCoinCount} TotalCoinCount:{TotalCoinCount + num}");
			}
			TotalCoinCount += num;
			m_prevCoinCount = currentCoinCount;
		}

		public void DebugLog()
		{
			Debug.Log($" MechType:{MechType}");
			Debug.Log($" HeadPhoneVolumeRate:{HeadPhoneVolumeRate}");
			Debug.Log($" IsBlockerOpen:{IsBlockerOpen}");
			Debug.Log($" WatchDogTimerCount:{WatchDogTimerCount}");
			Debug.Log($" UsbChargerEnable:{UsbChargerEnable}");
			Debug.Log($" CardLedColor:{CardLedColor}");
			Debug.Log($" PillarLedColor:{PillarLedColor}");
			foreach (LampID value in Enum.GetValues(typeof(LampID)))
			{
				Debug.Log($" LampEnables: lampID:{value}, {LampEnables[(int)value]}");
			}
			Debug.Log($" CoinCounterIncrement:{CoinCounterIncrement}");
			Debug.Log($" TotalCoinCount:{TotalCoinCount}");
			Debug.Log($" m_prevCoinCount:{m_prevCoinCount}");
		}
	}

	private int CardType;

	private bool CardOnFlag;

	private string CardIDStrings = "";

	private float m_errorTime;

	private byte m_prevCoinJam;

	private INITSTEP m_InitStep;

	private Vector2 m_mouseScrollDelta = new Vector2(0f, 0f);

	private aioSci m_Sci;

	private aioNMgrIob m_NodeMgr;

	private static aioIobBi2aVFG m_NodeCtl;

	private static bool[] ButtonStatus = new bool[9];

	private static bool IsInputReady = false;

	private bool m_isInitializeCalled;

	private bool m_icReaderInitialized;

	private byte[] _ePassData = new byte[32];

	private float m_initStepTimer;

	private float m_bioInitErrorCounter;

	private uint m_prevCntSendErr;

	private uint m_prevCntRecvErr;

	private bool m_forceResetRequest;

	private bool m_forceError;

	private const int PILLAR_LED_COUNT = 90;

	private byte[] m_pillarLedColorData = new byte[90];

	private InputElement[] m_inputElements = new InputElement[9];

	private List<BUTTON> m_gameButtons = new List<BUTTON>
	{
		BUTTON.TEST,
		BUTTON.SERVICE,
		BUTTON.COIN
	};

	private TouchDevice m_touch = new TouchDevice();

	private BioContext m_bioContext = new BioContext();

	[DllImport("kernel32")]
	private static extern bool SetDllDirectoryA(string lpPathName);

	[DllImport("kernel32")]
	private static extern void OutputDebugStringA(string lpOutputString);

	public Bi2xInputVFG()
	{
		Debug.Log($"[{DateTime.Now}]Bi2xInput() begin");
		Input_Initialize();
		Debug.Log($"[{DateTime.Now}]Bi2xInput() end");
	}

	public override void Initialize(aioIobBi2aVFG.MECHTYPE mechType = aioIobBi2aVFG.MECHTYPE.MECHTYPE_UNKNOWN)
	{
		Debug.Log($"[{DateTime.Now}]Bi2xInput.Initialize begin");
		m_bioContext.MechType = mechType;
		SetDllDirectoryA(Application.dataPath + "/Plugins");
		Debug.Log($"[{DateTime.Now}]acSystem.SystemInitialize begin");
		acSystem.SystemInitialize();
		Debug.Log($"[{DateTime.Now}]acSystem.SystemInitialize end");
		Debug.Log($"[{DateTime.Now}]aioSystem.SystemInitialize begin");
		aioSystem.SystemInitialize();
		Debug.Log($"[{DateTime.Now}]aioSystem.SystemInitialize end");
		for (int i = 0; i < 9; i++)
		{
			ButtonStatus[i] = false;
		}
		if (!SystemUtility.Instance.IsFactoryBoot)
		{
			Debug.Log($"[{DateTime.Now}]ICReader_Create begin");
			ICReader_Create("COM3");
			Debug.Log($"[{DateTime.Now}]ICReader_Create end");
		}
		m_isInitializeCalled = true;
		Debug.Log($"[{DateTime.Now}]Bi2xInput.Initialize end");
	}

	private void ICReader_Create(string comPortName)
	{
		for (int i = 0; i < 4; i++)
		{
			m_icCardReader = new ICReader();
			if (m_icCardReader != null)
			{
				break;
			}
			Debug.Log($"[{DateTime.Now}]ICReader()に失敗しました。({i + 1}回目)");
			Thread.Sleep(1000);
		}
		if (!GameUtility.IsIOEnable)
		{
			return;
		}
		for (int j = 0; j < 4; j++)
		{
			if (Connect(comPortName))
			{
				Debug.Log($"[{DateTime.Now}]ICReader:{comPortName}への接続に成功しました。");
				m_icReaderInitialized = true;
				break;
			}
			Debug.Log($"[{DateTime.Now}]ICReader:{comPortName}への接続に失敗しました。({j + 1}回目)");
			Thread.Sleep(2000);
		}
		if (!m_icCardReader.IsConnected)
		{
			Debug.Log($"[{DateTime.Now}]ICReader:{comPortName}へ接続できませんでした。");
			ErrorData.GetInstance().ExtraMessage = comPortName + "へ接続できませんでした。";
		}
		bool Connect(string _comPortName)
		{
			bool result = false;
			Debug.Log($"[{DateTime.Now}]ICReader:Connect begin");
			try
			{
				if (m_icCardReader.Connect(_comPortName))
				{
					result = true;
				}
			}
			catch (Exception ex)
			{
				Debug.Log($"[{DateTime.Now}]ICReader:Exception {ex.Message}");
			}
			finally
			{
				Debug.Log($"[{DateTime.Now}]ICReader:Connect end");
			}
			return result;
		}
	}

	~Bi2xInputVFG()
	{
		BioNode_Destroy();
	}

	public override void Update()
	{
		ICCardReader_Update();
		if (GameUtility.IsIOEnable && m_isInitializeCalled && BioNode_UpdateInitProcess())
		{
			BioNode_Update();
		}
		Input_Update();
		if (Bi2xInput.IsError)
		{
			m_errorTime += Time.deltaTime;
			if (m_errorTime >= 3f && ErrorControl.GetLastError() == null)
			{
				Debug.Log($"[{DateTime.Now}]Bi2xinput.SetError ErrorCode:{Bi2xInput.ErrorCode}");
				ErrorControl.SetError(ERROR.IO, Bi2xInput.ErrorCode);
			}
		}
		else
		{
			m_errorTime = 0f;
		}
	}

	private void ICCardReader_Update()
	{
		CardIDStrings = "";
		CardOnFlag = false;
		if (m_icReaderInitialized && m_icCardReader.IsTransmissionEnable())
		{
			_ePassData = m_icCardReader.GetCardInfo(0);
			if (_ePassData != null && _ePassData[0] == 2)
			{
				CardType = _ePassData[1] & 1;
				CardIDStrings += _ePassData[2].ToString("X2");
				CardIDStrings += _ePassData[3].ToString("X2");
				CardIDStrings += _ePassData[4].ToString("X2");
				CardIDStrings += _ePassData[5].ToString("X2");
				CardIDStrings += _ePassData[6].ToString("X2");
				CardIDStrings += _ePassData[7].ToString("X2");
				CardIDStrings += _ePassData[8].ToString("X2");
				CardIDStrings += _ePassData[9].ToString("X2");
				CardOnFlag = true;
			}
		}
	}

	private bool BioNode_UpdateInitProcess()
	{
		try
		{
			switch (m_InitStep)
			{
			case INITSTEP.Init:
				BioNode_Create();
				m_InitStep = INITSTEP.ReadyWait;
				Debug.Log($"[{DateTime.Now}]Bio:ReadyWait");
				return false;
			case INITSTEP.ReadyWait:
				if (BioNode_IsReady())
				{
					m_NodeCtl.SetMechType(m_bioContext.MechType);
					Debug.Log($"[{DateTime.Now}]Bio:MechTypeConfirmWait");
					aioNodeCtl.UpdateDevicesStatus();
					m_NodeCtl.GetDeviceStatus();
					m_InitStep = INITSTEP.MechTypeConfirmWait;
				}
				return false;
			case INITSTEP.MechTypeConfirmWait:
				if (m_bioContext.MechType == (aioIobBi2aVFG.MECHTYPE)m_NodeCtl.m_DevStatus.MechType)
				{
					Debug.Log($"[{DateTime.Now}]Bio:Recovery");
					m_InitStep = INITSTEP.Recovery;
				}
				else
				{
					Debug.Log($"[{DateTime.Now}]Bio:MechTypeConfirmWait");
				}
				aioNodeCtl.UpdateDevicesStatus();
				m_NodeCtl.GetDeviceStatus();
				return false;
			case INITSTEP.Recovery:
				m_bioInitErrorCounter = 0f;
				m_Sci.GetCommStatus();
				m_prevCntSendErr = m_Sci.m_CommStatus.cntSendErr;
				m_prevCntRecvErr = m_Sci.m_CommStatus.cntRecvErr;
				Bi2xInput.IsBi2xReady = true;
				m_NodeCtl.GetDeviceStatus();
				m_bioContext.m_prevCoinCount = m_NodeCtl.m_DevStatus.Input.CoinCount;
				BioNode_RecoverContext();
				m_InitStep = INITSTEP.Idle;
				Debug.Log($"[{DateTime.Now}]Bio:Init完了: m_prevCntSendErr:{m_prevCntSendErr} m_prevCntRecvErr:{m_prevCntRecvErr}");
				return false;
			case INITSTEP.ErrorIntervalWait:
				if (Time.time - m_initStepTimer >= 1f)
				{
					BioNode_Destroy();
					m_bioInitErrorCounter += 1f;
					m_InitStep = INITSTEP.Init;
					Debug.Log($"[{DateTime.Now}]Bio:Init");
				}
				return false;
			case INITSTEP.Idle:
				m_bioContext.ModifyTotalCoinCount(m_NodeCtl.m_DevStatus.Input.CoinCount);
				if (BioNode_IsError())
				{
					Debug.Log($"[{DateTime.Now}]Bio:Errorを検知");
					m_NodeCtl.GetDeviceStatus();
					m_bioContext.m_prevCoinCount = m_NodeCtl.m_DevStatus.Input.CoinCount;
					m_bioContext.DebugLog();
					Bi2xInput.IsBi2xReady = false;
					BioNode_Destroy();
					m_InitStep = INITSTEP.Init;
					Debug.Log($"[{DateTime.Now}]Bio:Init");
					return false;
				}
				return true;
			case INITSTEP.Failed:
				Bi2xInput.IsError = true;
				return false;
			}
		}
		catch (Bi2xInputException ex)
		{
			Debug.Log($"[{DateTime.Now}]Bi2xInputException: ErrorCode:{ex.Code} State:{ex.State}");
			Bi2xInput.ErrorCode = (uint)Math.Abs(ex.State);
			if (m_bioInitErrorCounter > 3f)
			{
				m_InitStep = INITSTEP.Failed;
			}
			else
			{
				m_initStepTimer = Time.time;
				m_InitStep = INITSTEP.ErrorIntervalWait;
				Debug.Log($"[{DateTime.Now}]Bio:ErrorIntervalWait");
			}
		}
		return false;
	}

	[DllImport("libaio-iob_video")]
	public unsafe static extern AIO_SCI_COMM* aioIobBi2a_OpenSciUsbCdc(uint i_SerialNumber);

	private void BioNode_Create()
	{
		m_Sci = aioIobBi2a.OpenSciUsbCdc(0u) ?? throw new Bi2xInputException(ECODE.aioSciComm);
		m_NodeMgr = aioNMgrIob.Create(m_Sci, (aioNMgrIob.MODEFLAG)0) ?? throw new Bi2xInputException(ECODE.NodeMgr);
		m_NodeCtl = aioIobBi2aVFG.Create(m_NodeMgr, 1u) ?? throw new Bi2xInputException(ECODE.NodeCtl);
		m_NodeMgr.BeginManage();
	}

	private bool BioNode_IsReady()
	{
		int state = m_NodeMgr.GetState();
		if (m_NodeMgr.IsError(state))
		{
			throw new Bi2xInputException(ECODE.NodeMgrError, state);
		}
		if (!m_NodeMgr.IsReady(state))
		{
			return false;
		}
		int state2 = m_NodeCtl.GetState();
		if (m_NodeCtl.IsError(state2))
		{
			throw new Bi2xInputException(ECODE.NodeCtlError, state2);
		}
		if (!m_NodeCtl.IsReady(state2))
		{
			return false;
		}
		return true;
	}

	private void BioNode_Destroy()
	{
		if (m_NodeCtl != null)
		{
			m_NodeCtl.Destroy();
			m_NodeCtl = null;
		}
		if (m_NodeMgr != null)
		{
			m_NodeMgr.Destroy();
			m_NodeMgr = null;
		}
		if (m_Sci != null)
		{
			m_Sci.Destroy();
			m_Sci = null;
		}
	}

	private bool BioNode_IsError()
	{
		if (m_forceResetRequest)
		{
			Debug.Log($"[{DateTime.Now}]Bio_IsError:m_forceResetRequestを検知");
			m_forceResetRequest = false;
			return true;
		}
		m_Sci.GetCommStatus();
		if (m_Sci.m_CommStatus.cntSendErr <= m_prevCntSendErr && m_Sci.m_CommStatus.cntRecvErr <= m_prevCntRecvErr && !m_NodeMgr.IsError(m_NodeMgr.GetState()))
		{
			return m_NodeCtl.IsError(m_NodeCtl.GetState());
		}
		return true;
	}

	public override void ForceResetRequest()
	{
		m_forceResetRequest = true;
	}

	public override void ForceComError()
	{
		m_forceError = true;
	}

	private void BioNode_Update()
	{
		if (Bi2xInput.IsBi2xReady)
		{
			aioNodeCtl.UpdateDevicesStatus();
			m_NodeCtl.GetDeviceStatus();
			ButtonStatus[0] = m_NodeCtl.m_DevStatus.Input.bTest != 0;
			ButtonStatus[1] = m_NodeCtl.m_DevStatus.Input.bService != 0;
			ButtonStatus[2] = m_NodeCtl.m_DevStatus.Input.bCoinSw != 0;
			if (m_NodeCtl.m_DevStatus.Input.bCoinJam != 0 && m_prevCoinJam != 0)
			{
				ErrorControl.SetError(ERROR.COIN_JAM, 0u);
			}
			m_prevCoinJam = m_NodeCtl.m_DevStatus.Input.bCoinJam;
		}
	}

	public override bool GetButtonStatus(INPUTBUTTONS button)
	{
		if (!Bi2xInput.IsBi2xReady)
		{
			return false;
		}
		if (button < INPUTBUTTONS.BUTTON_MAX)
		{
			return ButtonStatus[(int)button];
		}
		return false;
	}

	public override int GetTotalCoinCount()
	{
		return m_bioContext.TotalCoinCount;
	}

	public override bool IsHeadphoneJacking()
	{
		if (!Bi2xInput.IsBi2xReady)
		{
			return false;
		}
		return m_NodeCtl.m_DevStatus.Input.bHPDetect != 0;
	}

	public override aioIobBi2aVFG.MECHTYPE GetMechType()
	{
		if (!Bi2xInput.IsBi2xReady)
		{
			return aioIobBi2aVFG.MECHTYPE.MECHTYPE_UNKNOWN;
		}
		return (aioIobBi2aVFG.MECHTYPE)m_NodeCtl.m_DevStatus.MechType;
	}

	public override bool IsCardOn()
	{
		return CardOnFlag;
	}

	public override AvsTypes.CardType GetCardType()
	{
		return (AvsTypes.CardType)(CardType + 1);
	}

	public override string GetCardID()
	{
		return CardIDStrings;
	}

	public override void ICReaderOn()
	{
		if (m_icCardReader == null)
		{
			return;
		}
		if (m_icCardReader.IsConnected)
		{
			if (!m_icCardReader.IsTransmissionEnable())
			{
				m_icCardReader.TransmissionOn();
				Bi2xInput.Instance.SetCardLed(new Color32(127, 30, 40, byte.MaxValue));
			}
		}
		else
		{
			Debug.Log("IC Reader is not connected.");
		}
	}

	public override void ICReaderOff()
	{
		if (m_icCardReader == null)
		{
			return;
		}
		if (m_icCardReader.IsConnected)
		{
			if (m_icCardReader.IsTransmissionEnable())
			{
				m_icCardReader.TransmissionOff();
				Bi2xInput.Instance.SetCardLed(Color.black);
			}
		}
		else
		{
			Debug.Log("IC Reader is not connected.");
		}
	}

	public override void ICReaderClear()
	{
		if (m_icCardReader != null)
		{
			if (m_icCardReader.IsConnected)
			{
				m_icCardReader.ClearCardInfo(0);
			}
			else
			{
				Debug.Log("IC Reader is not connected.");
			}
		}
	}

	public override bool IsICReaderEnable()
	{
		if (m_icCardReader == null)
		{
			return false;
		}
		return m_icCardReader.IsConnected;
	}

	public override void SetWatchDogTimer(ushort count)
	{
		Debug.Log($"SetWatchDogTimer: count{count}");
		m_bioContext.WatchDogTimerCount = count;
		if (Bi2xInput.IsBi2xReady)
		{
			m_NodeCtl.SetWatchDogTimer(count);
		}
	}

	public override string GetFirmVersion()
	{
		if (!Bi2xInput.IsBi2xReady)
		{
			return "-.-.-";
		}
		if (m_NodeCtl == null)
		{
			return "x.x.x";
		}
		m_NodeCtl.GetNodeInfo();
		byte major = m_NodeCtl.m_NodeInfo.FirmInfo.FirmVer.Major;
		byte minor = m_NodeCtl.m_NodeInfo.FirmInfo.FirmVer.Minor;
		byte revision = m_NodeCtl.m_NodeInfo.FirmInfo.FirmVer.Revision;
		return $"{major}.{minor}.{revision}";
	}

	public override void ControlCoinBlocker(bool is_open)
	{
		m_bioContext.IsBlockerOpen = is_open;
		if (Bi2xInput.IsBi2xReady)
		{
			m_NodeCtl.ControlCoinBlocker(aioIobBi2aVFG.SLOT.SLOT_1, is_open);
		}
	}

	public override void EnableUsbCharger(bool is_open)
	{
		m_bioContext.UsbChargerEnable = is_open;
		if (Bi2xInput.IsBi2xReady)
		{
			m_NodeCtl.EnableUsbCharger(is_open);
		}
	}

	public override bool IsUsbChargerEnable()
	{
		return m_bioContext.UsbChargerEnable;
	}

	public override void AddCoinCounter(uint add)
	{
		if (!Bi2xInput.IsBi2xReady)
		{
			m_bioContext.CoinCounterIncrement += add;
		}
		else
		{
			m_NodeCtl.AddCounter(aioIobBi2aVFG.COUNTER.COUNTER_COIN, add);
		}
	}

	public override void SetLamp(LampID lampID, bool isActive)
	{
		m_bioContext.LampEnables[(int)lampID] = isActive;
		if (Bi2xInput.IsBi2xReady)
		{
			m_NodeCtl.SetLamp((aioIobBi2aVFG.LAMP)lampID, (byte)(isActive ? 128 : 0));
		}
	}

	public override void SetPillarLED(Color32 color)
	{
		m_bioContext.PillarLedColor = color;
		if (Bi2xInput.IsBi2xReady)
		{
			for (int i = 0; i < 90; i += 3)
			{
				m_pillarLedColorData[i] = color.r;
				m_pillarLedColorData[i + 1] = color.g;
				m_pillarLedColorData[i + 2] = color.b;
			}
			m_NodeCtl.SetLedData(m_pillarLedColorData, 90u);
		}
	}

	public override void SetButtonLED(Color32 color, bool isForceDisp = false)
	{
	}

	public override void SetQButtonLED(byte color_r)
	{
	}

	public override void SetTapeLedColor(TAPELED no, int led_no, Color color)
	{
	}

	public override void SetTapeLedColor(TAPELED no, Color color)
	{
	}

	public override void SetTapeLED(TAPELED no, int led_no, Color32 color)
	{
	}

	public override void SetTapeLED(TAPELED no, Color32 color)
	{
	}

	public override Color GetLedColor(TAPELED no, int led_no)
	{
		return Color.clear;
	}

	public override void AdjustBRIGHTNESS(bool isUp)
	{
	}

	public override void SetCardLed(Color32 color)
	{
		m_bioContext.CardLedColor = color;
		if (Bi2xInput.IsBi2xReady)
		{
			uint r = color.r;
			uint g = color.g;
			uint b = color.b;
			m_NodeCtl.SetIccrLed((r << 16) | (g << 8) | b);
		}
	}

	public override void ResetError()
	{
		if (Bi2xInput.IsBi2xReady && m_NodeCtl.m_DevStatus.Input.bCoinJam != 0)
		{
			ForceResetRequest();
		}
		m_prevCoinJam = 0;
	}

	private void getBtnVal_io(BUTTON type, ref bool isPush, ref float val)
	{
		isPush = false;
		val = 0f;
		if (Bi2xInput.IsBi2xReady)
		{
			switch (type)
			{
			case BUTTON.TEST:
				isPush = GetButtonStatus(INPUTBUTTONS.BUTTON_TEST);
				val = (isPush ? 1 : 0);
				break;
			case BUTTON.SERVICE:
				isPush = GetButtonStatus(INPUTBUTTONS.BUTTON_SERVICE);
				val = (isPush ? 1 : 0);
				break;
			case BUTTON.COIN:
				isPush = GetButtonStatus(INPUTBUTTONS.BUTTON_COIN_SW);
				val = (isPush ? 1 : 0);
				break;
			}
		}
	}

	private void getBtnVal_pad(BUTTON type, ref bool isPush, ref float val)
	{
	}

	private void getBtnVal_key(BUTTON type, ref bool isPush, ref float val)
	{
		isPush = false;
		val = 0f;
	}

	protected override bool IsTriggerPrivate(BUTTON type)
	{
		if (!IsInputReady)
		{
			return false;
		}
		return Bi2xInput.Instance.GetInputState(type).isTrigger();
	}

	protected override bool IsReleasePrivate(BUTTON type)
	{
		if (!IsInputReady)
		{
			return false;
		}
		return Bi2xInput.Instance.GetInputState(type).isRelease();
	}

	protected override bool IsPressPrivate(BUTTON type)
	{
		if (!IsInputReady)
		{
			return false;
		}
		return Bi2xInput.Instance.GetInputState(type).IsPress;
	}

	protected override float GetValPrivate(BUTTON type)
	{
		if (!IsInputReady)
		{
			return 0f;
		}
		return Bi2xInput.Instance.GetInputState(type).val;
	}

	public override InputState GetInputState(BUTTON type)
	{
		if (!bValidGameButton && m_gameButtons.Contains(type))
		{
			return default(InputState);
		}
		if (type != BUTTON.NUM)
		{
			return m_inputElements[(int)type].Input;
		}
		return default(InputState);
	}

	private void Input_Initialize()
	{
		for (int i = 0; i < m_inputElements.Length; i++)
		{
			m_inputElements[i] = new InputElement((BUTTON)i, getBtnVal_io, getBtnVal_pad, getBtnVal_key);
		}
		IsInputReady = true;
	}

	private void Input_Update()
	{
		if (m_inputElements != null)
		{
			InputElement[] inputElements = m_inputElements;
			for (int i = 0; i < inputElements.Length; i++)
			{
				inputElements[i]?.UpdateAll();
			}
		}
		if (DeviceManager.Instance.Initialized)
		{
			if (Bi2xInput.IsTrigger(BUTTON.SERVICE))
			{
				DeviceManager.Instance.Coin().AddServiceCoin(1);
			}
			DeviceManager.Instance.Update();
		}
		m_touch.Update();
	}

	public TouchDevice Touch()
	{
		return m_touch;
	}

	public override bool TouchInit()
	{
		return m_touch.Init();
	}

	public override void TouchCalibrate(Vector2 screenUpperLeft, Vector2 screenLowerRight, Vector2 upperLeft, Vector2 lowerRight)
	{
		TouchCalibrate(screenUpperLeft.x, screenUpperLeft.y, screenLowerRight.x, screenLowerRight.y, upperLeft.x, upperLeft.y, lowerRight.x, lowerRight.y);
	}

	public override void TouchCalibrate(float screen_l, float screen_u, float screen_r, float screen_d, float touch_l, float touch_u, float touch_r, float touch_d)
	{
		m_touch.Calibrate(screen_l, screen_u, screen_r, screen_d, touch_l, touch_u, touch_r, touch_d);
	}

	public override bool IsTouchTrigger()
	{
		return m_touch.IsTrigger();
	}

	public override bool IsTouchPress()
	{
		return m_touch.IsPress();
	}

	public override bool IsTouchRelease()
	{
		return m_touch.IsRelease();
	}

	public override Vector2 GetTouchPosition()
	{
		return m_touch.GetPosition();
	}

	public override Vector2 GetNoAdjustTouchPosition()
	{
		return m_touch.GetPositionRaw();
	}

	public override TouchPanelTelemetry GetTouchPanelTelemetry()
	{
		return m_touch.TouchPanelTelemetry;
	}

	public override void SetHeadPhoneVolume(float rate)
	{
		m_bioContext.HeadPhoneVolumeRate = rate;
		if (Bi2xInput.IsBi2xReady)
		{
			float b = 0f;
			uint i_Volume = (uint)Mathf.Lerp(96f, b, rate);
			m_NodeCtl.SetAmpVolume(aioIobBi2aVFG.AMP.AMP_HP_L, i_Volume);
			m_NodeCtl.SetAmpVolume(aioIobBi2aVFG.AMP.AMP_HP_R, i_Volume);
		}
	}

	private void BioNode_RecoverContext()
	{
		m_bioContext.DebugLog();
		Debug.Log($"m_DevStatus.MechType:{m_NodeCtl.m_DevStatus.MechType}");
		SetWatchDogTimer(m_bioContext.WatchDogTimerCount);
		ControlCoinBlocker(m_bioContext.IsBlockerOpen);
		SetHeadPhoneVolume(m_bioContext.HeadPhoneVolumeRate);
		EnableUsbCharger(m_bioContext.UsbChargerEnable);
		SetCardLed(m_bioContext.CardLedColor);
		SetPillarLED(m_bioContext.PillarLedColor);
		foreach (LampID value in Enum.GetValues(typeof(LampID)))
		{
			SetLamp(value, m_bioContext.LampEnables[(int)value]);
		}
		if (m_bioContext.CoinCounterIncrement != 0)
		{
			AddCoinCounter(m_bioContext.CoinCounterIncrement);
			m_bioContext.CoinCounterIncrement = 0u;
		}
	}
}
