using System;

namespace TCSIO.Serial;

public class ErrorPacketReceivedEventArgs : EventArgs
{
	public KDEErrorSerialPacket ReceivedData { get; set; }

	public byte ErrorCheckSum { get; set; }

	public bool Return { get; set; }
}
