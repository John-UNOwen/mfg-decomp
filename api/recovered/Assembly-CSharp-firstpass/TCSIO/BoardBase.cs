using System;
using System.Threading;
using System.Threading.Tasks;
using TCSIO.Serial;
using UnityEngine;

namespace TCSIO;

public class BoardBase
{
	private Task task_board_check;

	public static bool flag_id;

	public static bool flag_ver;

	public static bool flag_execute;

	public static bool flag_taskdisable;

	protected SerialManager Serial { get; private set; }

	protected bool TaskKillOrder { get; private set; }

	public bool IsConnected { get; private set; }

	protected int BoardCount { get; private set; }

	protected byte[] BoardCode { get; set; }

	public bool Connect(string com)
	{
		Debug.Log($"[{DateTime.Now}]BoardBase.Connect begin");
		if (IsConnected)
		{
			Disconnect();
		}
		Debug.Log($"[{DateTime.Now}]new SerialManager begin");
		Serial = new SerialManager(com, 115200);
		Debug.Log($"[{DateTime.Now}] new SerialManager end");
		if (Serial == null)
		{
			Debug.Log($"[{DateTime.Now}]BoardBase.Connect end Serial == null");
			return false;
		}
		Debug.Log($"[{DateTime.Now}]Open begin");
		if (!Serial.Open())
		{
			Debug.Log($"[{DateTime.Now}]BoardBase.Connect end !Serial.Open()");
			return false;
		}
		Debug.Log($"[{DateTime.Now}]Open end");
		Debug.Log($"[{DateTime.Now}]Negotiate begin");
		if (!Serial.Negotiate(new TimeSpan(0, 0, 5)))
		{
			Serial.Close();
			Debug.Log($"[{DateTime.Now}]BoardBase.Connect end !Serial.Negotiate");
			return false;
		}
		Thread.Sleep(500);
		Serial.PacketReceived += Serial_PacketReceived_Nonconnection;
		flag_id = false;
		Serial.SendPacket(SystemPacketGenerator.GenerateSetIDCommandPacket());
		if (!WaitFor(ref flag_id))
		{
			Serial.PacketReceived -= Serial_PacketReceived_Nonconnection;
			Serial.Close();
			Debug.Log($"[{DateTime.Now}]BoardBase.Connect end !WaitFor(ref flag_id");
			return false;
		}
		Thread.Sleep(500);
		Debug.Log($"[{DateTime.Now}]version check begin");
		for (int i = 0; i < BoardCount; i++)
		{
			flag_ver = false;
			Serial.SendPacket(SystemPacketGenerator.GenerateGetVersionCommandPacket((byte)(i + 1)));
			if (!WaitFor(ref flag_ver))
			{
				Debug.Log("バージョンチェック -> 強制的に True にしました。");
				flag_ver = true;
			}
		}
		Debug.Log($"[{DateTime.Now}]version check end");
		Debug.Log($"[{DateTime.Now}]IOExecutePermission begin");
		for (int j = 0; j < BoardCount; j++)
		{
			flag_execute = false;
			Serial.SendPacket(SystemPacketGenerator.GenerateIOExecutePermissionCommandPacket((byte)(j + 1)));
			if (!WaitFor(ref flag_execute))
			{
				Serial.PacketReceived -= Serial_PacketReceived_Nonconnection;
				Serial.Close();
				Debug.Log($"[{DateTime.Now}]BoardBase.Connect end IOExecutePermission");
				return false;
			}
		}
		Debug.Log($"[{DateTime.Now}]IOExecutePermission end");
		Serial.PacketReceived -= Serial_PacketReceived_Nonconnection;
		Serial.PacketReceived += Serial_PacketReceived_UserNonconnection;
		bool num = OnConnection();
		Serial.PacketReceived -= Serial_PacketReceived_UserNonconnection;
		if (!num)
		{
			Serial.Close();
			Debug.Log($"[{DateTime.Now}]BoardBase.Connect end !result");
			return false;
		}
		IsConnected = true;
		flag_taskdisable = false;
		task_board_check = new Task(BoardTaskWrapper);
		task_board_check.Start();
		Serial.PacketReceived += Serial_PacketReceived_Connection;
		Debug.Log($"[{DateTime.Now}]BoardBase.Connect end");
		return true;
	}

	private void Serial_PacketReceived_Connection(object sender, PacketReceivedEventArgs e)
	{
		OnPacketReceived(e);
	}

	private void Serial_PacketReceived_UserNonconnection(object sender, PacketReceivedEventArgs e)
	{
		OnInitializePacketReceived(e);
	}

	protected virtual bool OnConnection()
	{
		return true;
	}

	private void Serial_PacketReceived_Nonconnection(object sender, PacketReceivedEventArgs e)
	{
		if (e.ReceivedData.Type != 0)
		{
			return;
		}
		switch (e.ReceivedData.Command)
		{
		case 1:
			BoardCount = e.ReceivedData.Data[0];
			Debug.Log($"[{DateTime.Now}]Serial_PacketReceived_Nonconnection BoardCount:{BoardCount}");
			flag_id = true;
			break;
		case 2:
			if (e.ReceivedData.Data[8] == BoardCode[0] && e.ReceivedData.Data[9] == BoardCode[1] && e.ReceivedData.Data[10] == BoardCode[2] && e.ReceivedData.Data[11] == BoardCode[3])
			{
				flag_ver = true;
			}
			break;
		case 3:
			if (e.ReceivedData.Data[0] == 0)
			{
				flag_execute = true;
			}
			break;
		}
	}

	public void Disconnect()
	{
		if (Serial != null)
		{
			Serial.PacketReceived -= Serial_PacketReceived_Connection;
			WaitForInfinite(ref flag_taskdisable);
			Serial.Close();
			Serial = null;
		}
		IsConnected = false;
	}

	protected virtual void OnInitializePacketReceived(PacketReceivedEventArgs e)
	{
	}

	protected virtual void OnPacketReceived(PacketReceivedEventArgs e)
	{
	}

	protected void WaitForInfinite(ref bool flag)
	{
		while (!flag)
		{
			Thread.Sleep(100);
		}
	}

	protected bool WaitFor(ref bool flag)
	{
		for (int i = 0; i < 100; i++)
		{
			if (flag)
			{
				return true;
			}
			Thread.Sleep(100);
		}
		return false;
	}

	private void BoardTaskWrapper()
	{
		while (IsConnected)
		{
			BoardTask();
		}
		flag_taskdisable = true;
	}

	protected virtual void BoardTask()
	{
	}
}
