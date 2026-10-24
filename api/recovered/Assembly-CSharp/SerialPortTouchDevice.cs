using System;
using System.IO.Ports;
using System.Threading;
using UnityEngine;

public class SerialPortTouchDevice
{
	[Flags]
	private enum TouchStateFlag
	{
		Touch = 1,
		ContinuousTouch = 2,
		DeTouch = 4,
		Reserve0 = 8,
		Error = 0x10,
		Reserve1 = 0x20,
		Reserve2 = 0x40,
		ZisActive = 0x80
	}

	public TouchPanelTelemetry TouchPanelTelemetry = new TouchPanelTelemetry();

	private TouchStateFlag m_touchStateFlag;

	private Vector2 m_touchPos;

	private SerialPort m_comPort;

	private const int BUILD_BUF_SIZE = 512;

	private const int COMMAND_PACKET_SIZE = 10;

	private int m_step;

	private int m_buildBuffPos;

	private byte[] m_buildBuff = new byte[512];

	private bool m_isTrigger;

	private bool m_isPress;

	private bool m_isDetouch;

	private Thread m_thread;

	public bool Open()
	{
		bool flag = false;
		if (m_comPort == null)
		{
			m_comPort = new SerialPort();
		}
		m_comPort.BaudRate = 9600;
		m_comPort.Parity = Parity.None;
		m_comPort.DataBits = 8;
		m_comPort.StopBits = StopBits.One;
		m_comPort.Handshake = Handshake.RequestToSend;
		m_comPort.PortName = "COM4";
		try
		{
			m_comPort.Open();
			m_comPort.DtrEnable = true;
			Debug.Log("serial port: opened");
			flag = true;
		}
		catch
		{
			flag = false;
		}
		m_step = 0;
		m_buildBuffPos = 0;
		m_touchStateFlag = (TouchStateFlag)0;
		return flag;
	}

	public void Close()
	{
		m_comPort.Close();
		m_comPort.Dispose();
	}

	public void Update()
	{
		m_isDetouch = false;
		m_isPress = false;
		m_isTrigger = false;
		DataReceivedHandler();
	}

	public bool IsTrigger()
	{
		return m_isTrigger;
	}

	public bool IsPress()
	{
		return m_isPress;
	}

	public bool IsRelease()
	{
		return m_isDetouch;
	}

	public Vector2 GetPosition()
	{
		return m_touchPos;
	}

	private void DataReceivedHandler()
	{
		if (m_comPort == null || !m_comPort.IsOpen || m_comPort.BytesToRead <= 0)
		{
			return;
		}
		byte[] array = new byte[m_comPort.BytesToRead];
		TouchPanelTelemetry.ReadCount += m_comPort.Read(array, 0, array.Length);
		byte[] array2 = array;
		foreach (byte b in array2)
		{
			switch (m_step)
			{
			case 0:
				if (b == 85)
				{
					m_buildBuffPos = 0;
					m_buildBuff[m_buildBuffPos] = b;
					m_buildBuffPos++;
					TouchPanelTelemetry.BeginCount++;
					m_step = 1;
				}
				break;
			case 1:
				m_buildBuff[m_buildBuffPos++] = b;
				if (m_buildBuffPos != 10)
				{
					break;
				}
				TouchPanelTelemetry.EndCount++;
				if (CheckSum(m_buildBuff))
				{
					m_touchStateFlag = (TouchStateFlag)m_buildBuff[2];
					m_touchPos.x = (float)((m_buildBuff[4] << 8) + m_buildBuff[3]) / 4095f;
					m_touchPos.y = (float)((m_buildBuff[6] << 8) + m_buildBuff[5]) / 4095f;
					if (m_touchStateFlag.HasFlag(TouchStateFlag.Touch))
					{
						m_isTrigger = true;
						m_isPress = true;
					}
					if (m_touchStateFlag.HasFlag(TouchStateFlag.ContinuousTouch))
					{
						m_isPress = true;
					}
					if (m_touchStateFlag.HasFlag(TouchStateFlag.DeTouch))
					{
						m_isDetouch = true;
					}
				}
				else
				{
					TouchPanelTelemetry.ErrorCount++;
				}
				m_step = 0;
				break;
			}
		}
	}

	private bool CheckSum(byte[] packet)
	{
		byte b = 170;
		for (int i = 0; i < 9; i++)
		{
			b += packet[i];
		}
		return b == packet[9];
	}
}
