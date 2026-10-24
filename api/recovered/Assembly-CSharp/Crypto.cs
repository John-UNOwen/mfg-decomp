using System;
using System.Security.Cryptography;
using System.Text;

public static class Crypto
{
	private const int BlockSize = 128;

	private const int KeySize = 256;

	private const int KeyByteLength = 32;

	private const int InitVectorLength = 16;

	public static byte[] Encrypt(string text, string pass, string salt)
	{
		return Encrypt(Encoding.UTF8.GetBytes(text), pass, salt);
	}

	public static byte[] Encrypt(byte[] binData, string pass, string salt)
	{
		RijndaelManaged obj = new RijndaelManaged
		{
			Padding = PaddingMode.PKCS7,
			Mode = CipherMode.CBC,
			KeySize = 256,
			BlockSize = 128
		};
		byte[] salt2 = MakeSalt(salt);
		Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(pass, salt2);
		byte[] bytes = rfc2898DeriveBytes.GetBytes(32);
		byte[] bytes2 = rfc2898DeriveBytes.GetBytes(16);
		ICryptoTransform cryptoTransform = obj.CreateEncryptor(bytes, bytes2);
		try
		{
			return cryptoTransform.TransformFinalBlock(binData, 0, binData.Length);
		}
		catch (Exception ex)
		{
			throw ex;
		}
		finally
		{
			cryptoTransform.Dispose();
		}
	}

	public static string DecryptToString(byte[] bytes, string pass, string salt)
	{
		byte[] array = Decrypt(bytes, pass, salt);
		return Encoding.UTF8.GetString(array, 3, array.Length - 3);
	}

	public static byte[] Decrypt(byte[] binData, string pass, string salt)
	{
		RijndaelManaged obj = new RijndaelManaged
		{
			Padding = PaddingMode.PKCS7,
			Mode = CipherMode.CBC,
			KeySize = 256,
			BlockSize = 128
		};
		byte[] salt2 = MakeSalt(salt);
		Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(pass, salt2);
		byte[] bytes = rfc2898DeriveBytes.GetBytes(32);
		byte[] bytes2 = rfc2898DeriveBytes.GetBytes(16);
		ICryptoTransform cryptoTransform = obj.CreateDecryptor(bytes, bytes2);
		try
		{
			byte[] result = cryptoTransform.TransformFinalBlock(binData, 0, binData.Length);
			cryptoTransform.Dispose();
			return result;
		}
		catch
		{
			return null;
		}
		finally
		{
			cryptoTransform.Dispose();
		}
	}

	private static byte[] MakeSalt(string str)
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
		return Encoding.UTF8.GetBytes(text);
	}
}
