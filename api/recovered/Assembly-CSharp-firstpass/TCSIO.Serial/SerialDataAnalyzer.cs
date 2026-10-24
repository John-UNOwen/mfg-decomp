using System.Collections.Generic;

namespace TCSIO.Serial;

internal class SerialDataAnalyzer
{
	private enum AnalyzeState
	{
		Standby,
		ID,
		Type,
		Command,
		Tag,
		DataLength,
		Data,
		CheckSum,
		CheckSum2
	}

	private SerialPacket workPacket;

	private AnalyzeState state;

	private byte[] packetBuffer = new byte[256];

	private int dataLength;

	private int dataPointer;

	private bool escaped;

	public Queue<SerialPacket> ReceivedPackets { get; private set; }

	public Queue<KDEErrorSerialPacket> ReceivedErrorPackets { get; private set; }

	public void Write(byte[] stream)
	{
		foreach (byte code in stream)
		{
			Analyze(code);
		}
	}

	public SerialDataAnalyzer()
	{
		ReceivedPackets = new Queue<SerialPacket>();
		ReceivedErrorPackets = new Queue<KDEErrorSerialPacket>();
		workPacket = new SerialPacket();
	}

	private void Analyze(byte code)
	{
		switch (code)
		{
		case 170:
			state = AnalyzeState.ID;
			return;
		case byte.MaxValue:
			escaped = true;
			return;
		}
		if (escaped)
		{
			code = (byte)(~code);
			escaped = false;
		}
		switch (state)
		{
		case AnalyzeState.ID:
			workPacket.ID = code;
			state = AnalyzeState.Type;
			break;
		case AnalyzeState.Type:
			workPacket.Type = code;
			state = AnalyzeState.Command;
			break;
		case AnalyzeState.Command:
			workPacket.Command = code;
			state = AnalyzeState.Tag;
			break;
		case AnalyzeState.Tag:
			workPacket.Tag = code;
			state = AnalyzeState.DataLength;
			break;
		case AnalyzeState.DataLength:
			dataLength = code;
			dataPointer = 0;
			if (dataLength > 0)
			{
				state = AnalyzeState.Data;
			}
			else
			{
				state = AnalyzeState.CheckSum;
			}
			break;
		case AnalyzeState.Data:
			packetBuffer[dataPointer] = code;
			dataPointer++;
			if (dataPointer == dataLength)
			{
				state = AnalyzeState.CheckSum;
			}
			break;
		case AnalyzeState.CheckSum:
			workPacket.SetData(packetBuffer, 0, dataLength);
			if (workPacket.CheckSum == code)
			{
				ReceivedPackets.Enqueue(workPacket);
				workPacket = new SerialPacket();
			}
			else
			{
				KDEErrorSerialPacket kDEErrorSerialPacket = new KDEErrorSerialPacket();
				kDEErrorSerialPacket.ErrorCheckSum = code;
				kDEErrorSerialPacket.Packet = workPacket;
				ReceivedErrorPackets.Enqueue(kDEErrorSerialPacket);
				workPacket = new SerialPacket();
			}
			state = AnalyzeState.Standby;
			break;
		}
	}
}
