namespace TCSIO.Serial;

public class PacketReceivedEventArgs
{
	public SerialPacket ReceivedData { get; set; }

	public bool Return { get; set; }
}
