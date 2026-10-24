using IO;
using KAMUNITY;
using Konami.Aio;
using MFG.Types;
using TCSIO;
using UnityEngine;

public abstract class Bi2xInput
{
	public enum INPUTBUTTONS
	{
		BUTTON_TEST,
		BUTTON_SERVICE,
		BUTTON_COIN_SW,
		BUTTON_Q_BUTTON_1,
		BUTTON_Q_BUTTON_2,
		BUTTON_Q_BUTTON_3,
		BUTTON_Q_BUTTON,
		BUTTON_JACK_DETECT,
		BUTTON_MIC_DETECT,
		BUTTON_MAX
	}

	public enum ECODE
	{
		CreateWriteFirmContext = 1,
		WriteFirmIsError,
		aioSciComm,
		NodeMgr,
		NodeCtl,
		NodeMgrError,
		NodeCtlError,
		DevCounterNotIncrements,
		CoinJumCounterNotIncrements
	}

	public enum LampID
	{
		Lamp1,
		Lamp2,
		Lamp3,
		Lamp4
	}

	public enum TAPELED
	{
		TAPELED_CTRL_PANEL,
		TAPELED_LEFT_PANEL,
		TAPELED_RIGHT_PANEL,
		TAPELED_TBS_TITLE_PANEL,
		TAPELED_TBS_SIDE_PANEL
	}

	public enum TAPELED_TBS
	{
		TAPELED_TITLE,
		TAPELED_SIDE
	}

	public struct InputState
	{
		public bool IsPress;

		public bool IsPrevPress;

		public float val;

		public void UpdatePushFlag(bool bPush)
		{
			IsPrevPress = IsPress;
			IsPress = bPush;
		}

		public void ResetVal()
		{
			val = 0f;
		}

		public bool isTrigger()
		{
			if (IsPress)
			{
				return !IsPrevPress;
			}
			return false;
		}

		public bool isRelease()
		{
			if (!IsPress)
			{
				return IsPrevPress;
			}
			return false;
		}

		public bool isPush()
		{
			if (!IsPress)
			{
				return IsPrevPress;
			}
			return true;
		}
	}

	private static Bi2xInput s_instance;

	public ICReader m_icCardReader;

	public static bool IsBi2xReady;

	protected const float COLOR_MAX = 254f;

	protected const ushort WATCH_DOG_COUNT = 6000;

	protected float BRIGHTNESS = 0.5f;

	[HideInInspector]
	public bool bValidGameButton = true;

	public static Bi2xInput Instance
	{
		get
		{
			if (s_instance == null)
			{
				if (SystemConfigGameData.CabinetType == CabinetType.UKS)
				{
					s_instance = new Bi2xInputUKS();
				}
				else if (SystemConfigGameData.CabinetType == CabinetType.TBS || SystemConfigGameData.CabinetType == CabinetType.UJK)
				{
					s_instance = new Bi2xInputTBS();
				}
				else
				{
					s_instance = new Bi2xInputVFG();
				}
			}
			return s_instance;
		}
	}

	public static bool IsError { get; protected set; }

	public static uint ErrorCode { get; protected set; }

	public abstract void Initialize(aioIobBi2aVFG.MECHTYPE mechType = aioIobBi2aVFG.MECHTYPE.MECHTYPE_UNKNOWN);

	public abstract void Update();

	public abstract void ForceComError();

	public abstract bool GetButtonStatus(INPUTBUTTONS button);

	public abstract int GetTotalCoinCount();

	public abstract bool IsHeadphoneJacking();

	public abstract aioIobBi2aVFG.MECHTYPE GetMechType();

	public abstract bool IsCardOn();

	public abstract AvsTypes.CardType GetCardType();

	public abstract string GetCardID();

	public abstract void ICReaderOn();

	public abstract void ICReaderOff();

	public abstract bool IsICReaderEnable();

	public abstract void ICReaderClear();

	public abstract void AdjustBRIGHTNESS(bool isUp);

	public abstract void SetWatchDogTimer(ushort count = 6000);

	public abstract string GetFirmVersion();

	public abstract void ControlCoinBlocker(bool is_open);

	public abstract void EnableUsbCharger(bool is_open);

	public abstract bool IsUsbChargerEnable();

	public abstract void AddCoinCounter(uint add);

	public abstract void SetLamp(LampID lampID, bool isActive);

	public abstract void SetPillarLED(Color32 color);

	public abstract void SetButtonLED(Color32 color, bool isForceDisp = false);

	public abstract void SetQButtonLED(byte color_r);

	public abstract void SetTapeLedColor(TAPELED no, int led_no, Color color);

	public abstract void SetTapeLedColor(TAPELED no, Color color);

	public abstract void SetTapeLED(TAPELED no, int led_no, Color32 color);

	public abstract void SetTapeLED(TAPELED no, Color32 color);

	public abstract Color GetLedColor(TAPELED no, int led_no);

	public abstract void SetCardLed(Color32 color);

	public abstract void ResetError();

	public abstract void ForceResetRequest();

	protected abstract bool IsTriggerPrivate(BUTTON type);

	protected abstract bool IsReleasePrivate(BUTTON type);

	protected abstract bool IsPressPrivate(BUTTON type);

	protected abstract float GetValPrivate(BUTTON type);

	public static bool IsTrigger(BUTTON type)
	{
		return Instance.IsTriggerPrivate(type);
	}

	public static bool IsRelease(BUTTON type)
	{
		return Instance.IsReleasePrivate(type);
	}

	public static bool IsPress(BUTTON type)
	{
		return Instance.IsPressPrivate(type);
	}

	public static float GetVal(BUTTON type)
	{
		return Instance.GetValPrivate(type);
	}

	public static bool GetKey(KeyCode keyCode)
	{
		if (SystemConfigGameData.IsCloudStreaming)
		{
			return CloudStreamingInputDevice.GetKey(keyCode);
		}
		return false;
	}

	public static bool GetKeyDown(KeyCode keyCode)
	{
		if (SystemConfigGameData.IsCloudStreaming)
		{
			return CloudStreamingInputDevice.GetKeyDown(keyCode);
		}
		return false;
	}

	public static bool GetKeyUp(KeyCode keyCode)
	{
		if (SystemConfigGameData.IsCloudStreaming)
		{
			return CloudStreamingInputDevice.GetKeyUp(keyCode);
		}
		return false;
	}

	public static bool MahjongGetKey(KeyCode keyCode)
	{
		if (keyCode == KeyCode.None)
		{
			return CloudStreamingInputDevice.GetScopeKey(94u);
		}
		return CloudStreamingInputDevice.GetKey(keyCode);
	}

	public static bool MahjongGetKeyDown(KeyCode keyCode)
	{
		if (keyCode == KeyCode.None)
		{
			return CloudStreamingInputDevice.GetScopeKeyDown(94u);
		}
		return CloudStreamingInputDevice.GetKeyDown(keyCode);
	}

	public static bool MahjongGetKeyUp(KeyCode keyCode)
	{
		if (keyCode == KeyCode.None)
		{
			return CloudStreamingInputDevice.GetScopeKeyUp(94u);
		}
		return CloudStreamingInputDevice.GetKeyUp(keyCode);
	}

	public abstract InputState GetInputState(BUTTON type);

	public abstract void SetHeadPhoneVolume(float rate);

	public abstract bool TouchInit();

	public abstract void TouchCalibrate(Vector2 screenUpperLeft, Vector2 screenLowerRight, Vector2 upperLeft, Vector2 lowerRight);

	public abstract void TouchCalibrate(float screen_l, float screen_u, float screen_r, float screen_d, float touch_l, float touch_u, float touch_r, float touch_d);

	public abstract bool IsTouchTrigger();

	public abstract bool IsTouchPress();

	public abstract bool IsTouchRelease();

	public abstract Vector2 GetTouchPosition();

	public abstract Vector2 GetNoAdjustTouchPosition();

	public abstract TouchPanelTelemetry GetTouchPanelTelemetry();

	public static bool GetMouseButton(int button)
	{
		if (SystemConfigGameData.IsCloudStreaming)
		{
			return CloudStreamingInputDevice.GetScopeMouseButton(button);
		}
		return Input.GetMouseButton(button);
	}

	public static bool GetMouseButtonDown(int button)
	{
		if (SystemConfigGameData.IsCloudStreaming)
		{
			return CloudStreamingInputDevice.GetScopeMouseButtonDown(button);
		}
		return Input.GetMouseButtonDown(button);
	}

	public static bool GetMouseButtonUp(int button)
	{
		if (SystemConfigGameData.IsCloudStreaming)
		{
			return CloudStreamingInputDevice.GetScopeMouseButtonUp(button);
		}
		return Input.GetMouseButtonUp(button);
	}

	public static Vector3 GetMousePosition()
	{
		if (SystemConfigGameData.IsCloudStreaming)
		{
			return CloudStreamingInputDevice.GetScopeMousePosition();
		}
		return Input.mousePosition;
	}
}
