using System;
using System.IO;
using System.Text;

namespace TCSIO.Serial;

public class SerialPacket
{
	private byte _check_sum;

	public byte ID { get; set; }

	public byte Type { get; set; }

	public byte Command { get; set; }

	public byte Tag { get; set; }

	public byte CheckSum
	{
		get
		{
			GeneratePacketData();
			return _check_sum;
		}
	}

	public byte[] Data { get; private set; }

	public byte[] InternalData { get; private set; }

	private void initialize()
	{
		ID = 0;
		Type = 0;
		Command = 0;
		Tag = 0;
		Data = new byte[0];
		InternalData = new byte[0];
	}

	public SerialPacket()
	{
		initialize();
	}

	public SerialPacket(byte id, byte type, byte command, byte tag)
	{
		initialize();
		ID = id;
		Type = type;
		Command = command;
		Tag = tag;
	}

	public SerialPacket(byte id, byte type, byte command, byte tag, byte[] data)
	{
		initialize();
		ID = id;
		Type = type;
		Command = command;
		Tag = tag;
		SetData(data, 0, data.Length);
	}

	public void SetData(byte[] source, int index, int length)
	{
		byte[] array = new byte[length];
		Array.Copy(source, array, length);
		Data = array;
		GenInternalData();
	}

	private void GenInternalData()
	{
		int num = 0;
		for (int i = 0; i < Data.Length; i++)
		{
			if (Data[i] == 170)
			{
				num++;
			}
			else if (Data[i] == byte.MaxValue)
			{
				num++;
			}
			num++;
		}
		byte[] array = new byte[num];
		int num2 = 0;
		int num3 = 0;
		for (int j = 0; j < Data.Length; j++)
		{
			if (Data[j] == 170)
			{
				array[num2++] = byte.MaxValue;
				array[num2++] = 85;
				num3++;
			}
			else if (Data[j] == byte.MaxValue)
			{
				array[num2++] = byte.MaxValue;
				array[num2++] = 0;
				num3++;
			}
			else
			{
				array[num2++] = Data[j];
			}
		}
		InternalData = array;
	}

	public void SetInternalData(byte[] source, int index, int length)
	{
		byte[] array = new byte[length];
		Array.Copy(source, array, length);
		InternalData = array;
		GenBaseData();
	}

	private void GenBaseData()
	{
		int num = 0;
		for (int i = 0; i < InternalData.Length; i++)
		{
			if (InternalData[i] != byte.MaxValue)
			{
				num++;
			}
		}
		byte[] array = new byte[num];
		int num2 = 0;
		bool flag = false;
		for (int j = 0; j < num; j++)
		{
			if (InternalData[num2] == byte.MaxValue)
			{
				flag = true;
				num2++;
			}
			if (flag)
			{
				if (InternalData[num2] == 0)
				{
					array[j] = byte.MaxValue;
				}
				else if (InternalData[num2] == 85)
				{
					array[j] = 170;
				}
				flag = false;
				num2++;
			}
			else
			{
				array[j] = InternalData[num2];
				num2++;
			}
		}
		Data = array;
	}

	private void SetInternalData(byte[] baseData)
	{
		int num = 0;
		for (int i = 0; i < baseData.Length; i++)
		{
			num = baseData[i] switch
			{
				byte.MaxValue => num + 2, 
				170 => num + 2, 
				_ => num + 1, 
			};
		}
		byte[] array = new byte[num];
		int num2 = 0;
		for (int j = 0; j < baseData.Length; j++)
		{
			switch (baseData[j])
			{
			case byte.MaxValue:
				array[num2++] = byte.MaxValue;
				array[num2++] = 0;
				num2 += 2;
				break;
			case 170:
				array[num2++] = byte.MaxValue;
				array[num2++] = 85;
				break;
			default:
				array[num2++] = baseData[j];
				break;
			}
		}
		InternalData = array;
	}

	public byte[] GeneratePacketData()
	{
		MemoryStream memoryStream = new MemoryStream();
		byte check_sum = 0;
		memoryStream.WriteByte(170);
		ms_enc_write(memoryStream, ID, ref check_sum);
		ms_enc_write(memoryStream, Type, ref check_sum);
		ms_enc_write(memoryStream, Command, ref check_sum);
		ms_enc_write(memoryStream, Tag, ref check_sum);
		ms_enc_write(memoryStream, (byte)Data.Length, ref check_sum);
		memoryStream.Write(InternalData, 0, InternalData.Length);
		byte b = 0;
		for (int i = 0; i < Data.Length; i++)
		{
			b += Data[i];
		}
		check_sum = (_check_sum = (byte)(check_sum + b));
		ms_enc_write(memoryStream, check_sum, ref check_sum);
		return memoryStream.ToArray();
	}

	private void ms_enc_write(MemoryStream ms, byte data, ref byte check_sum)
	{
		check_sum += data;
		switch (data)
		{
		case byte.MaxValue:
			ms.WriteByte(byte.MaxValue);
			ms.WriteByte(0);
			break;
		case 170:
			ms.WriteByte(byte.MaxValue);
			ms.WriteByte(85);
			break;
		default:
			ms.WriteByte(data);
			break;
		}
	}

	public bool GetBit(int start_bit_index)
	{
		int num = start_bit_index / 8;
		start_bit_index &= 7;
		return ((Data[num] >> start_bit_index) & 1) == 1;
	}

	public string GetAsciiString(int start_index, int length)
	{
		return Encoding.ASCII.GetString(Data, start_index, length);
	}

	public uint GetUnsignedInteger(int start_bit_index, int bit_length)
	{
		int num = start_bit_index / 8;
		int num2 = (start_bit_index + bit_length) / 8;
		uint num3 = 0u;
		for (int i = num; i < num2; i++)
		{
			num3 <<= 8;
			num3 += Data[num + i];
		}
		int num4 = (start_bit_index + bit_length) & 7;
		num3 >>= 7 - num4;
		num3 <<= 32 - bit_length;
		return num3 >> 32 - bit_length;
	}
}
