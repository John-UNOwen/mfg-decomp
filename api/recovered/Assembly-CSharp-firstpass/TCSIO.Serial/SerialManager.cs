using System;
using System.IO.Ports;
using System.Threading;
using UnityEngine;

namespace TCSIO.Serial;

public class SerialManager
{
	public delegate void PacketReceivedEventHandler(object sender, PacketReceivedEventArgs e);

	public delegate void ErrorPacketReceivedEventHandler(object sender, ErrorPacketReceivedEventArgs e);

	private SerialPort port;

	private string _portName;

	private int _baudRate;

	private SerialDataAnalyzer analyzer = new SerialDataAnalyzer();

	private Timer timer;

	private Timer reset_timer;

	private static Thread ComportTh;

	private bool _IsNegotiationMode;

	private bool _IsResetMode;

	private int nego_timeout;

	private byte[] stmdata = new byte[50];

	private int stmcount;

	public bool IsNegotiationMode
	{
		get
		{
			return _IsNegotiationMode;
		}
		private set
		{
			_IsNegotiationMode = value;
		}
	}

	public bool IsResetMode
	{
		get
		{
			return _IsResetMode;
		}
		private set
		{
			_IsResetMode = value;
		}
	}

	public string PortName
	{
		get
		{
			return port.PortName;
		}
		set
		{
			port.PortName = value;
		}
	}

	public int BaudRate
	{
		get
		{
			return port.BaudRate;
		}
		set
		{
			port.BaudRate = value;
		}
	}

	public bool IsOpen
	{
		get
		{
			if (!port.IsOpen)
			{
				return false;
			}
			return true;
		}
	}

	public event PacketReceivedEventHandler PacketReceived;

	public event ErrorPacketReceivedEventHandler ErrorPacketReceived;

	public event NegotiationCompletedEventHandler NegotiationCompleted;

	public event NegotiationFailedEventHandler NegotiationFailed;

	public event ResetCompletedEventHandler ResetCompleted;

	private void startNegotiate(int timeout)
	{
		for (int i = 0; i < stmdata.Length; i++)
		{
			stmdata[i] = 170;
		}
		IsNegotiationMode = true;
		nego_timeout = timeout / 100;
		timer.Change(0, 100);
	}

	private void stopNegotiate()
	{
		timer.Change(-1, -1);
	}

	private void sendNegotiationData(object obj)
	{
		port?.Write(stmdata, 0, stmdata.Length);
		nego_timeout--;
		if (nego_timeout <= 0)
		{
			stopNegotiate();
			IsNegotiationMode = false;
			OnNegotiationFailed(new NegotiationFailedEventArgs());
		}
	}

	public SerialManager()
	{
		init();
	}

	public SerialManager(string portName, int baudRate)
	{
		init();
		_portName = portName;
		_baudRate = baudRate;
		if (port != null)
		{
			port.PortName = _portName;
			port.BaudRate = _baudRate;
		}
	}

	private void init()
	{
		Debug.Log($"[{DateTime.Now}]SerialManager:init begin");
		port = new SerialPort();
		if (port == null)
		{
			Debug.Log($"[{DateTime.Now}]SerialManager:init port==null");
		}
		timer = new Timer(sendNegotiationData, null, -1, -1);
		reset_timer = new Timer(stopReset, null, -1, -1);
		if (ComportTh != null)
		{
			ComportTh.Abort();
			Thread.Sleep(1000);
			ComportTh = null;
		}
		ComportTh = new Thread(serialTask);
		ComportTh.Start();
		Debug.Log($"[{DateTime.Now}]SerialManager:init end");
	}

	private void serialTask()
	{
		while (true)
		{
			if (port != null && port.IsOpen && port.BytesToRead > 0)
			{
				port_DataReceived(port, null);
			}
			Thread.Sleep(1);
		}
	}

	public bool Open()
	{
		try
		{
			if (port == null)
			{
				return false;
			}
			port.Open();
			return true;
		}
		catch
		{
			return false;
		}
	}

	public bool Negotiate(TimeSpan negotiationTime)
	{
		if (!IsNegotiationMode)
		{
			startNegotiate((int)negotiationTime.TotalMilliseconds);
			return true;
		}
		return false;
	}

	public bool Negotiate()
	{
		return Negotiate(new TimeSpan(0, 0, 1));
	}

	public void Close()
	{
		port?.Close();
	}

	public bool Reset(TimeSpan resetTime)
	{
		if (IsResetMode)
		{
			return false;
		}
		IsResetMode = true;
		port.BreakState = true;
		reset_timer.Change((int)resetTime.TotalMilliseconds, -1);
		return true;
	}

	public bool Reset()
	{
		return Reset(new TimeSpan(0, 0, 0, 0, 250));
	}

	private void stopReset(object obj)
	{
		reset_timer.Change(-1, -1);
		port.BreakState = false;
		IsResetMode = false;
		OnReset(new ResetCompletedEventArgs());
	}

	public void SendPacket(SerialPacket packet)
	{
		byte[] array = packet.GeneratePacketData();
		port.Write(array, 0, array.Length);
	}

	public void SendCustomData(byte[] data)
	{
		port.Write(data, 0, data.Length);
	}

	private void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
	{
		if (IsNegotiationMode)
		{
			byte[] array = new byte[port.BytesToRead];
			port.Read(array, 0, array.Length);
			byte[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				if (array2[i] == 170)
				{
					stmcount++;
					if (stmcount >= 10)
					{
						stopNegotiate();
						IsNegotiationMode = false;
						stmcount = 0;
						OnNegotiationCompleted(new NegotiationCompletedEventArgs());
						break;
					}
				}
				else
				{
					stmcount = 0;
				}
			}
		}
		else if (!IsNegotiationMode)
		{
			byte[] array3 = new byte[port.BytesToRead];
			port.Read(array3, 0, array3.Length);
			analyzer.Write(array3);
			while (analyzer.ReceivedPackets.Count > 0)
			{
				PacketReceivedEventArgs packetReceivedEventArgs = new PacketReceivedEventArgs();
				packetReceivedEventArgs.ReceivedData = analyzer.ReceivedPackets.Dequeue();
				packetReceivedEventArgs.Return = false;
				OnPacketReceived(packetReceivedEventArgs);
			}
			while (analyzer.ReceivedErrorPackets.Count > 0)
			{
				ErrorPacketReceivedEventArgs errorPacketReceivedEventArgs = new ErrorPacketReceivedEventArgs();
				errorPacketReceivedEventArgs.ReceivedData = analyzer.ReceivedErrorPackets.Dequeue();
				errorPacketReceivedEventArgs.Return = false;
				OnErrorPacketReceived(errorPacketReceivedEventArgs);
			}
		}
	}

	protected virtual void OnPacketReceived(PacketReceivedEventArgs e)
	{
		if (this.PacketReceived != null)
		{
			this.PacketReceived(this, e);
		}
	}

	protected virtual void OnErrorPacketReceived(ErrorPacketReceivedEventArgs e)
	{
		if (this.ErrorPacketReceived != null)
		{
			this.ErrorPacketReceived(this, e);
		}
	}

	protected virtual void OnNegotiationCompleted(NegotiationCompletedEventArgs e)
	{
		if (this.NegotiationCompleted != null)
		{
			this.NegotiationCompleted(this, e);
		}
	}

	protected virtual void OnNegotiationFailed(NegotiationFailedEventArgs e)
	{
		if (this.NegotiationFailed != null)
		{
			this.NegotiationFailed(this, e);
		}
	}

	protected virtual void OnReset(ResetCompletedEventArgs e)
	{
		if (this.ResetCompleted != null)
		{
			this.ResetCompleted(this, e);
		}
	}
}
