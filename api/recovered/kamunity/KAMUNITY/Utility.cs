using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace KAMUNITY;

public static class Utility
{
	private static DateTime s_EpochBase = new DateTime(1970, 1, 1, 0, 0, 0, 0);

	public static bool IsSafeDelegate(Delegate func)
	{
		if ((object)func == null)
		{
			return false;
		}
		if (func.Target != null && func.Target.Equals(null))
		{
			return false;
		}
		return true;
	}

	public static string ByteArrayToHexString(byte[] ba)
	{
		StringBuilder stringBuilder = new StringBuilder(ba.Length * 2);
		foreach (byte b in ba)
		{
			stringBuilder.AppendFormat("{0:X2}", b);
		}
		return stringBuilder.ToString();
	}

	public static string MakeHashSHA256(byte[] data)
	{
		using SHA256CryptoServiceProvider sHA256CryptoServiceProvider = new SHA256CryptoServiceProvider();
		return ByteArrayToHexString(sHA256CryptoServiceProvider.ComputeHash(data));
	}

	public static string MakeHashSHA256(Stream inputStream)
	{
		using SHA256CryptoServiceProvider sHA256CryptoServiceProvider = new SHA256CryptoServiceProvider();
		return ByteArrayToHexString(sHA256CryptoServiceProvider.ComputeHash(inputStream));
	}

	public static string MakeHashSHA1(byte[] data)
	{
		using SHA1CryptoServiceProvider sHA1CryptoServiceProvider = new SHA1CryptoServiceProvider();
		return ByteArrayToHexString(sHA1CryptoServiceProvider.ComputeHash(data));
	}

	public static string MakeHashMD5(byte[] data)
	{
		using MD5CryptoServiceProvider mD5CryptoServiceProvider = new MD5CryptoServiceProvider();
		return ByteArrayToHexString(mD5CryptoServiceProvider.ComputeHash(data));
	}

	public static string MakeMultiPartBoundaryCode()
	{
		StringBuilder stringBuilder = new StringBuilder(70);
		Random random = new Random();
		string text = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
		for (int i = 0; i < 70; i++)
		{
			stringBuilder.Append(text[random.Next(0, text.Length - 1)]);
		}
		return stringBuilder.ToString();
	}

	public static void WriteAsciiStringToStream(Stream stream, string str)
	{
		byte[] bytes = Encoding.ASCII.GetBytes(str);
		stream.Write(bytes, 0, bytes.Length);
	}

	public static DateTime Epoch2DateTime(long epoch)
	{
		return s_EpochBase.AddSeconds(epoch);
	}

	public static DateTime Epoch2DateTime(ulong epoch)
	{
		return s_EpochBase.AddSeconds((long)epoch);
	}

	public static string GetRawPath()
	{
		return "dev/raw";
	}
}
