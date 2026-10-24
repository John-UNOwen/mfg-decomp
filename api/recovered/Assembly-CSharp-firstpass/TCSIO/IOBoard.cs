using System;
using TCSIO.Serial;

namespace TCSIO;

public class IOBoard : BoardBase
{
	private bool flag_init;

	private readonly int LED_BLOCK_NUM = 4;

	public byte[] Led = new byte[960];

	public bool Test { get; private set; }

	public bool Service { get; private set; }

	public bool Coin { get; private set; }

	public bool CoinJum { get; private set; }

	public byte[] IOStatus { get; private set; }

	public IOBoard()
	{
		base.BoardCode = new byte[4] { 66, 73, 50, 65 };
	}

	protected override bool OnConnection()
	{
		for (int i = 0; i < base.BoardCount; i++)
		{
			flag_init = false;
			base.Serial.SendPacket(new SerialPacket((byte)(i + 1), 1, 0, 0, new byte[1]));
			if (!WaitFor(ref flag_init))
			{
				return false;
			}
		}
		return true;
	}

	protected override void OnInitializePacketReceived(PacketReceivedEventArgs e)
	{
		if (e.ReceivedData.Type == 1 && e.ReceivedData.Command == 0 && e.ReceivedData.Data[0] == 0)
		{
			flag_init = true;
		}
	}

	protected override void OnPacketReceived(PacketReceivedEventArgs e)
	{
		if (e.ReceivedData.Type == 1 && e.ReceivedData.Command == 16)
		{
			IOStatus = e.ReceivedData.Data;
			Test = (((IOStatus[1] & 8u) != 0) ? true : false);
			Service = (((IOStatus[9] & 4u) != 0) ? true : false);
			Coin = (((IOStatus[9] & 2u) != 0) ? true : false);
			CoinJum = ((((uint)IOStatus[9] & (true ? 1u : 0u)) != 0) ? true : false);
		}
	}

	protected override void BoardTask()
	{
		base.Serial.SendPacket(new SerialPacket(1, 1, 16, 0));
		byte[] array = new byte[40];
		Array.Clear(array, 0, array.Length);
		array[11] = 128;
		array[12] = 128;
		array[13] = 128;
		base.Serial.SendPacket(new SerialPacket(1, 1, 18, 64, array));
		byte[] array2 = new byte[241];
		for (int i = 0; i < LED_BLOCK_NUM; i++)
		{
			Array.Clear(array2, 0, array2.Length);
			byte b = (byte)i;
			if (i == LED_BLOCK_NUM - 1)
			{
				b = (byte)(b | 0x80u);
			}
			array2[0] = b;
			Array.Copy(Led, i * 240, array2, 1, 240);
			base.Serial.SendPacket(new SerialPacket(1, 1, 65, 241, array2));
		}
	}
}
