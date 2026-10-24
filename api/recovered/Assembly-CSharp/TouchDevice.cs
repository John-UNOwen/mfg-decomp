using MFG.Types;
using TestModeSkeleton;
using UnityEngine;

public class TouchDevice
{
	private SerialPortTouchDevice m_touchDevice = new SerialPortTouchDevice();

	private TouchPanelTelemetry _touchPanelTelemetry = new TouchPanelTelemetry();

	private bool m_isUseSerialPortTouchDevice;

	private bool _isUseCloudStreamingTouchDevice;

	private CalibrationData m_data = new CalibrationData();

	private float m_coeff_x = 1f;

	private float m_offset_x;

	private float m_coeff_y = 1f;

	private float m_offset_y;

	public TouchPanelTelemetry TouchPanelTelemetry
	{
		get
		{
			if (!m_isUseSerialPortTouchDevice)
			{
				return _touchPanelTelemetry;
			}
			return m_touchDevice.TouchPanelTelemetry;
		}
	}

	public bool Init()
	{
		CalibrationSettings.SaveData data = CabinetManager.Calibration.Data;
		m_data.screen_l = 200f;
		m_data.screen_u = 880f;
		m_data.screen_r = 1720f;
		m_data.screen_d = 200f;
		m_isUseSerialPortTouchDevice = SystemConfigGameData.CabinetType == CabinetType.B || SystemConfigGameData.CabinetType == CabinetType.C;
		_isUseCloudStreamingTouchDevice = SystemConfigGameData.IsCloudStreaming;
		if (_isUseCloudStreamingTouchDevice)
		{
			CloudStreamingInputDevice.Open();
		}
		else if (m_isUseSerialPortTouchDevice)
		{
			if (data.IsCalibrated)
			{
				Calibrate(m_data.screen_l, m_data.screen_u, m_data.screen_r, m_data.screen_d, data.UpperLeft.x, data.UpperLeft.y, data.LowerRight.x, data.LowerRight.y);
			}
			if (!m_touchDevice.Open())
			{
				return false;
			}
		}
		return true;
	}

	public bool IsTrigger()
	{
		if (_isUseCloudStreamingTouchDevice)
		{
			CloudStreamingInputDevice.GetScopeMouseButtonDown(0);
		}
		else if (m_isUseSerialPortTouchDevice)
		{
			return m_touchDevice.IsTrigger();
		}
		return Bi2xInput.GetMouseButtonDown(0);
	}

	public bool IsPress()
	{
		if (_isUseCloudStreamingTouchDevice)
		{
			CloudStreamingInputDevice.GetScopeMouseButton(0);
		}
		else if (m_isUseSerialPortTouchDevice)
		{
			return m_touchDevice.IsPress();
		}
		return Bi2xInput.GetMouseButton(0);
	}

	public bool IsRelease()
	{
		if (_isUseCloudStreamingTouchDevice)
		{
			CloudStreamingInputDevice.GetScopeMouseButtonUp(0);
		}
		else if (m_isUseSerialPortTouchDevice)
		{
			return m_touchDevice.IsRelease();
		}
		return Bi2xInput.GetMouseButtonUp(0);
	}

	public Vector2 GetPositionRaw()
	{
		if (_isUseCloudStreamingTouchDevice)
		{
			return CloudStreamingInputDevice.GetScopeMousePosition();
		}
		Vector3 vector;
		if (m_isUseSerialPortTouchDevice)
		{
			vector = m_touchDevice.GetPosition();
			vector.x = (1f - vector.x) * m_data.screen_r;
			vector.y *= m_data.screen_u;
			return vector;
		}
		vector = Bi2xInput.GetMousePosition();
		return vector;
	}

	public Vector2 GetPosition()
	{
		Vector2 positionRaw = GetPositionRaw();
		return AdjustPosition(positionRaw.x, positionRaw.y);
	}

	public void Calibrate(float screen_l, float screen_u, float screen_r, float screen_d, float touch_l, float touch_u, float touch_r, float touch_d)
	{
		if (touch_l - touch_r == 0f)
		{
			m_coeff_x = 1f;
			m_offset_x = 0f;
		}
		else
		{
			m_coeff_x = (screen_l - screen_r) / (touch_l - touch_r);
			m_offset_x = screen_l - touch_l * m_coeff_x;
		}
		if (touch_u - touch_d == 0f)
		{
			m_coeff_y = 1f;
			m_offset_y = 0f;
		}
		else
		{
			m_coeff_y = (screen_u - screen_d) / (touch_u - touch_d);
			m_offset_y = screen_u - touch_u * m_coeff_y;
		}
		m_data.screen_l = screen_l;
		m_data.screen_u = screen_u;
		m_data.screen_r = screen_r;
		m_data.screen_d = screen_d;
		m_data.touch_l = touch_l;
		m_data.touch_u = touch_u;
		m_data.touch_r = touch_r;
		m_data.touch_d = touch_d;
	}

	public Vector2 AdjustPosition(float pos_x, float pos_y)
	{
		if (m_isUseSerialPortTouchDevice)
		{
			pos_x = pos_x * m_coeff_x + m_offset_x;
			pos_y = pos_y * m_coeff_y + m_offset_y;
		}
		return new Vector2(pos_x, pos_y);
	}

	public void Update()
	{
		if (_isUseCloudStreamingTouchDevice)
		{
			CloudStreamingInputDevice.Update();
		}
		else if (m_isUseSerialPortTouchDevice)
		{
			m_touchDevice.Update();
		}
	}
}
