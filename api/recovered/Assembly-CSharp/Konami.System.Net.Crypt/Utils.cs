using System;
using System.Security.Cryptography;
using System.Text;

namespace Konami.System.Net.Crypt;

public static class Utils
{
	private static AesManaged MakeAesManaged(string pass, ref byte[] salt)
	{
		Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(pass, salt);
		byte[] key = rfc2898DeriveBytes.GetBytes(32);
		byte[] iv = rfc2898DeriveBytes.GetBytes(16);
		return MakeAesManagedKeyIv(ref key, ref iv);
	}

	private static AesManaged MakeAesManagedKeyIv(ref byte[] key, ref byte[] iv)
	{
		return new AesManaged
		{
			BlockSize = 128,
			KeySize = 256,
			Mode = CipherMode.CBC,
			Padding = PaddingMode.PKCS7,
			Key = key,
			IV = iv
		};
	}

	public static byte[] MakeSalt(string str)
	{
		if (str.Length == 0)
		{
			throw new Exception("length is 0");
		}
		string text = str;
		while (text.Length < 8)
		{
			text += str;
		}
		return Encoding.ASCII.GetBytes(text);
	}

	public static byte[] EncryptAes(byte[] src, string pass, byte[] salt)
	{
		using AesManaged aes = MakeAesManaged(pass, ref salt);
		return EncryptAes(src, aes);
	}

	public static byte[] EncryptAes(byte[] src, AesManaged aes)
	{
		using ICryptoTransform cryptoTransform = aes.CreateEncryptor(aes.Key, aes.IV);
		int num = 16;
		byte[] array = new byte[(src.Length + num - 1) / num * num + num];
		int num2 = src.Length;
		int num3 = 0;
		int num4 = 0;
		while (num2 != 0)
		{
			int num5 = ((num2 > num) ? num : num2);
			if (num5 < num || num5 == num2)
			{
				byte[] array2 = cryptoTransform.TransformFinalBlock(src, num3, num5);
				array2.CopyTo(array, num4);
				num4 += array2.Length;
			}
			else
			{
				num4 += cryptoTransform.TransformBlock(src, num3, num5, array, num4);
			}
			num2 -= num5;
			num3 += num5;
		}
		if (num4 == array.Length)
		{
			return array;
		}
		Array.Resize(ref array, num4);
		return array;
	}

	public static byte[] DecryptAes(byte[] src, int offset, string pass, byte[] salt)
	{
		using AesManaged aes = MakeAesManaged(pass, ref salt);
		return DecryptAes(src, offset, aes);
	}

	public static byte[] DecryptAes(byte[] src, int offset, AesManaged aes)
	{
		using ICryptoTransform cryptoTransform = aes.CreateDecryptor(aes.Key, aes.IV);
		int num = 16;
		byte[] array = new byte[src.Length - offset];
		int num2 = src.Length - offset;
		int num3 = offset;
		int num4 = 0;
		while (num2 != 0)
		{
			int num5 = ((num2 > num) ? num : num2);
			if (num5 < num || num5 == num2)
			{
				byte[] array2 = cryptoTransform.TransformFinalBlock(src, num3, num5);
				array2.CopyTo(array, num4);
				num4 += array2.Length;
			}
			else
			{
				num4 += cryptoTransform.TransformBlock(src, num3, num5, array, num4);
			}
			num2 -= num5;
			num3 += num5;
		}
		if (num4 == array.Length)
		{
			return array;
		}
		Array.Resize(ref array, num4);
		return array;
	}
}
