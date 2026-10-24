using System;
using TCSIO.Serial;

namespace TCSIO;

public class TapeLED : BoardBase
{
	private readonly int BLOCK_NUM = 4;

	public TapeLED()
	{
		base.BoardCode = new byte[4] { 66, 73, 50, 65 };
	}

	protected override bool OnConnection()
	{
		return true;
	}

	protected override void OnInitializePacketReceived(PacketReceivedEventArgs e)
	{
	}

	protected override void BoardTask()
	{
		byte[] array = new byte[241];
		for (int i = 0; i < BLOCK_NUM; i++)
		{
			Array.Clear(array, 0, array.Length);
			byte b = (byte)i;
			if (i == BLOCK_NUM - 1)
			{
				b = (byte)(b | 0x80u);
			}
			array[0] = b;
			for (int j = 1; j < 241; j++)
			{
				array[j] = byte.MaxValue;
			}
			base.Serial.SendPacket(new SerialPacket(1, 1, 65, 241, array));
		}
	}
}
