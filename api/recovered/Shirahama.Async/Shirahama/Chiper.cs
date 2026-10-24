using System.IO;
using System.Security.Cryptography;

namespace Shirahama;

public class Chiper
{
	private readonly byte[] _key = new byte[16]
	{
		121, 170, 132, 38, 163, 119, 221, 156, 72, 178,
		75, 20, 110, 183, 102, 98
	};

	private readonly byte[] _initial = new byte[16]
	{
		52, 110, 139, 64, 155, 254, 158, 79, 165, 161,
		238, 226, 114, 188, 114, 38
	};

	public byte[] Encrypt(string serialized)
	{
		if (string.IsNullOrEmpty(serialized))
		{
			return null;
		}
		using RijndaelManaged rijndaelManaged = new RijndaelManaged
		{
			BlockSize = _initial.Length * 8,
			IV = _initial,
			KeySize = _key.Length * 8,
			Key = _key,
			Mode = CipherMode.CBC,
			Padding = PaddingMode.PKCS7
		};
		using MemoryStream memoryStream = new MemoryStream();
		using CryptoStream stream = new CryptoStream(memoryStream, rijndaelManaged.CreateEncryptor(), CryptoStreamMode.Write);
		using (StreamWriter streamWriter = new StreamWriter(stream))
		{
			streamWriter.Write(serialized);
		}
		return memoryStream.ToArray();
	}

	public string Decrypt(byte[] data)
	{
		if (data == null)
		{
			return null;
		}
		if (data.Length == 0)
		{
			return null;
		}
		using RijndaelManaged rijndaelManaged = new RijndaelManaged
		{
			BlockSize = _initial.Length * 8,
			IV = _initial,
			KeySize = _key.Length * 8,
			Key = _key,
			Mode = CipherMode.CBC,
			Padding = PaddingMode.PKCS7
		};
		using MemoryStream stream = new MemoryStream(data);
		using CryptoStream stream2 = new CryptoStream(stream, rijndaelManaged.CreateDecryptor(), CryptoStreamMode.Read);
		using StreamReader streamReader = new StreamReader(stream2);
		return streamReader.ReadToEnd();
	}
}
