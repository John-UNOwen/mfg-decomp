namespace TCSIO.Serial;

public class KDEErrorSerialPacket
{
	public byte ErrorCheckSum { get; set; }

	public SerialPacket Packet { get; set; }
}
