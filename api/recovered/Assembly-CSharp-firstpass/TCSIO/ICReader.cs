using System;
using System.Threading;
using TCSIO.Serial;
using UnityEngine;

namespace TCSIO;

public class ICReader : BoardBase
{
	private bool flag_init;

	private bool flag_initic;

	private bool flag_icstop;

	private bool flag_EnableTransmission;

	private byte[][] cardinfo;

	private object _cardInfoLock = new object();

	public ICReader()
	{
		base.BoardCode = new byte[4] { 73, 67, 67, 66 };
	}

	public byte[] GetCardInfo(int index)
	{
		lock (_cardInfoLock)
		{
			if (cardinfo == null)
			{
				return null;
			}
			return cardinfo[index];
		}
	}

	public void ClearCardInfo(int index)
	{
		lock (_cardInfoLock)
		{
			if (cardinfo != null && cardinfo[index] != null)
			{
				Array.Clear(cardinfo[index], 0, cardinfo[index].Length);
			}
		}
	}

	public bool IsTransmissionEnable()
	{
		return flag_EnableTransmission;
	}

	public void TransmissionOn()
	{
		flag_EnableTransmission = true;
	}

	public void TransmissionOff()
	{
		flag_EnableTransmission = false;
	}

	protected override bool OnConnection()
	{
		Debug.Log($"[{DateTime.Now}]ICReader.OnConnection begin");
		Debug.Log($"[{DateTime.Now}]BoardCount:{base.BoardCount}");
		for (int i = 0; i < base.BoardCount; i++)
		{
			flag_init = false;
			base.Serial.SendPacket(new SerialPacket((byte)(i + 1), 1, 0, 0));
			if (!WaitFor(ref flag_init))
			{
				Debug.Log($"[{DateTime.Now}]ICReader.OnConnection end flag_init");
				return false;
			}
		}
		for (int j = 0; j < base.BoardCount; j++)
		{
			flag_initic = false;
			base.Serial.SendPacket(new SerialPacket((byte)(j + 1), 1, 48, 0, new byte[1]));
			if (!WaitFor(ref flag_initic))
			{
				Debug.Log($"[{DateTime.Now}]ICReader.OnConnection end flag_initic");
				return false;
			}
		}
		for (int k = 0; k < base.BoardCount; k++)
		{
			flag_initic = false;
			base.Serial.SendPacket(new SerialPacket((byte)(k + 1), 1, 58, 0, new byte[1] { 1 }));
			if (!WaitFor(ref flag_icstop))
			{
				Debug.Log($"[{DateTime.Now}]ICReader.OnConnection end flag_icstop");
				return false;
			}
			Thread.Sleep(120);
		}
		flag_EnableTransmission = true;
		lock (_cardInfoLock)
		{
			cardinfo = new byte[base.BoardCount][];
		}
		Debug.Log($"[{DateTime.Now}]ICReader.OnConnection end");
		return true;
	}

	protected override void OnInitializePacketReceived(PacketReceivedEventArgs e)
	{
		if (e.ReceivedData.Type != 1)
		{
			return;
		}
		switch (e.ReceivedData.Command)
		{
		case 0:
			if (e.ReceivedData.Data[0] == 0)
			{
				flag_init = true;
			}
			break;
		case 48:
			if (e.ReceivedData.Data[0] == 0)
			{
				flag_initic = true;
			}
			break;
		case 58:
			if (e.ReceivedData.Data[0] == 0)
			{
				flag_icstop = true;
			}
			break;
		}
	}

	protected override void OnPacketReceived(PacketReceivedEventArgs e)
	{
		if (e.ReceivedData.Type == 1 && e.ReceivedData.Command == 52 && e.ReceivedData.Data.Length != 0)
		{
			lock (_cardInfoLock)
			{
				cardinfo[(e.ReceivedData.ID & 0x7F) - 1] = e.ReceivedData.Data;
			}
		}
	}

	protected override void BoardTask()
	{
		for (int i = 0; i < base.BoardCount; i++)
		{
			if (flag_EnableTransmission)
			{
				base.Serial.SendPacket(new SerialPacket((byte)(i + 1), 1, 97, 0, new byte[4] { 0, 3, 255, 255 }));
				Thread.Sleep(150);
				base.Serial.SendPacket(new SerialPacket((byte)(i + 1), 1, 52, 0, new byte[1] { 23 }));
				Thread.Sleep(120);
				base.Serial.SendPacket(new SerialPacket((byte)(i + 1), 1, 58, 0, new byte[1] { 1 }));
				Thread.Sleep(120);
				base.Serial.SendPacket(new SerialPacket((byte)(i + 1), 1, 97, 0, new byte[4] { 0, 3, 255, 255 }));
				Thread.Sleep(150);
				base.Serial.SendPacket(new SerialPacket((byte)(i + 1), 1, 52, 0, new byte[1] { 23 }));
				Thread.Sleep(120);
			}
			else
			{
				base.Serial.SendPacket(new SerialPacket((byte)(i + 1), 1, 58, 0, new byte[1]));
				Thread.Sleep(120);
			}
		}
	}
}
