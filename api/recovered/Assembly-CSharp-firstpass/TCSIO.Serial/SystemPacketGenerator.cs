namespace TCSIO.Serial;

public static class SystemPacketGenerator
{
	public static SerialPacket GenerateSetIDCommandPacket()
	{
		return new SerialPacket(0, 0, 1, 0, new byte[1]);
	}

	public static SerialPacket GenerateGetVersionCommandPacket(byte id)
	{
		return new SerialPacket(id, 0, 2, 0);
	}

	public static SerialPacket GenerateIOExecutePermissionCommandPacket(byte id)
	{
		return new SerialPacket(id, 0, 3, 0);
	}

	public static SerialPacket GenerateUpdateExecutePermissionCommandPacket(byte id)
	{
		return new SerialPacket(id, 0, 4, 0);
	}

	public static SerialPacket GenerateSleepCommandPacket(byte id)
	{
		return new SerialPacket(id, 0, 8, 0);
	}

	public static SerialPacket GenerateGetErrorCommandPacket(byte id)
	{
		return new SerialPacket(id, 0, 65, 0);
	}

	public static SerialPacket GenerateCheckCommandPacket(byte id)
	{
		return new SerialPacket(id, 0, 128, 0);
	}
}
