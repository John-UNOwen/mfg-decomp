using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Compress;
using UnityEngine;

namespace Konami.System.Net.Crypt;

public class Aes
{
	public const int FIND_KEY_LENGTH = 10;

	private readonly string FindKeyChars = "abcdefghkmnpqrstuvwxyz+ABCDEFGHJKLMNPQRSTUVWXYZ/012345679=";

	private readonly byte[] EncryptKey = new byte[32]
	{
		159, 148, 97, 149, 76, 61, 111, 93, 211, 27,
		115, 198, 220, 231, 69, 114, 89, 230, 45, 249,
		200, 48, 95, 79, 26, 178, 119, 137, 78, 138,
		181, 7
	};

	private readonly byte[] DecryptKey = new byte[32]
	{
		140, 18, 5, 202, 28, 183, 197, 70, 191, 73,
		187, 246, 30, 172, 22, 180, 249, 130, 246, 142,
		21, 254, 241, 131, 7, 76, 35, 186, 144, 135,
		171, 192
	};

	private readonly string[] EncryptKeys = new string[10] { "DKkn94RvOPjIp5iFQ0AmxyL3UsBb6uGt", "FrNUmAJ3i2gH14OYpwEWbCLR6hztylno", "Creu43yjF1t0GWSK6isxZaRPdp9NwHEq", "TSbYihlUKgszpwnetWvBdHo829MkjuG3", "XR9CnZ7G4Yw6AQFgyWdophj21BEHaMlq", "QHnbuSGcKvspiE8MxThq9PlA0zXfWD7F", "wFgVNupDO89CKTbUsB2rlkHoq7eGz4E5", "klOfaW59LzARjT7xngSUtP2eYXwpmCqE", "l0x72VEHgLwfY3PJdiSWbkOXMhue5q6F", "bcW6hXlj3IriAx7apFLMnEtZqN5P1wJd" };

	public string GetFindKey()
	{
		int num = 10;
		StringBuilder stringBuilder = new StringBuilder(num);
		for (int i = 0; i < num; i++)
		{
			int index = UnityEngine.Random.Range(0, FindKeyChars.Length);
			stringBuilder.Append(FindKeyChars[index]);
		}
		return stringBuilder.ToString();
	}

	private string getEncryptKey(string _str)
	{
		int num = 0;
		byte[] bytes = Encoding.ASCII.GetBytes(_str);
		for (int i = 0; i < bytes.Length; i++)
		{
			num += bytes[i];
		}
		int num2 = num % EncryptKeys.Length;
		return EncryptKeys[num2];
	}

	public byte[] Encrypt(string _postData)
	{
		return Encrypt(_postData, "");
	}

	public byte[] Encrypt(string _postData, string _findKey)
	{
		byte[] result = null;
		if (string.IsNullOrEmpty(_postData))
		{
			return result;
		}
		using RijndaelManaged rijndaelManaged = new RijndaelManaged();
		rijndaelManaged.Mode = CipherMode.CBC;
		rijndaelManaged.Padding = PaddingMode.PKCS7;
		rijndaelManaged.BlockSize = 128;
		if (_findKey.Length > 0)
		{
			rijndaelManaged.Key = Encoding.UTF8.GetBytes(getEncryptKey(_findKey));
		}
		else
		{
			rijndaelManaged.Key = EncryptKey;
		}
		ICryptoTransform transform = rijndaelManaged.CreateEncryptor();
		byte[] array = Compress.Utils.EncodeGZip(Encoding.UTF8.GetBytes(_postData));
		using MemoryStream memoryStream = new MemoryStream();
		using (CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write))
		{
			if (array != null)
			{
				cryptoStream.Write(array, 0, array.Length);
			}
			cryptoStream.FlushFinalBlock();
		}
		byte[] array2 = memoryStream.ToArray();
		byte[] iV = rijndaelManaged.IV;
		result = new byte[iV.Length + array2.Length];
		iV.CopyTo(result, 0);
		array2.CopyTo(result, iV.Length);
		return result;
	}

	public string Decrypt(ref byte[] _response)
	{
		return Decrypt(ref _response, "");
	}

	public string Decrypt(ref byte[] _response, string _findKey)
	{
		string result = "";
		if (_response == null || _response.Length == 0)
		{
			return result;
		}
		using (RijndaelManaged rijndaelManaged = new RijndaelManaged())
		{
			rijndaelManaged.Mode = CipherMode.CBC;
			rijndaelManaged.Padding = PaddingMode.PKCS7;
			rijndaelManaged.KeySize = 256;
			rijndaelManaged.BlockSize = 128;
			byte[] array = null;
			array = ((_findKey.Length <= 0) ? DecryptKey : Encoding.UTF8.GetBytes(getEncryptKey(_findKey)));
			int num = rijndaelManaged.IV.Length;
			byte[] array2 = new byte[num];
			Array.Copy(_response, 0, array2, 0, num);
			byte[] array3 = new byte[_response.Length - num];
			Array.Copy(_response, num, array3, 0, _response.Length - num);
			ICryptoTransform transform = rijndaelManaged.CreateDecryptor(array, array2);
			using MemoryStream stream = new MemoryStream(array3);
			using CryptoStream cryptoStream = new CryptoStream(stream, transform, CryptoStreamMode.Read);
			byte[] array4 = new byte[_response.Length - num];
			cryptoStream.Read(array4, 0, _response.Length - num);
			byte[] array5 = Compress.Utils.DecodeGZip(array4);
			if (array5 != null)
			{
				result = Encoding.UTF8.GetString(array5);
			}
		}
		return result;
	}
}
