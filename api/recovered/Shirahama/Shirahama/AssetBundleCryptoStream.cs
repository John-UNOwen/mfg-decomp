using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Shirahama;

public class AssetBundleCryptoStream : Stream
{
	private Stream _internalStream;

	private AesManaged _cryptor;

	private ICryptoTransform _encryptor;

	public override bool CanRead => _internalStream.CanRead;

	public override bool CanSeek => _internalStream.CanSeek;

	public override bool CanWrite => _internalStream.CanWrite;

	public override long Length => _internalStream.Length;

	public override long Position
	{
		get
		{
			return _internalStream.Position;
		}
		set
		{
			_internalStream.Position = value;
		}
	}

	public AssetBundleCryptoStream(Stream baseStream, string password, byte[] salt)
	{
		CreateAesStream(baseStream, password, salt);
	}

	public AssetBundleCryptoStream(Stream baseStream, string password, string salt)
	{
		if (string.IsNullOrEmpty(salt))
		{
			throw new ArgumentException("length is 0");
		}
		string text = salt;
		while (text.Length < 8)
		{
			text += salt;
		}
		byte[] bytes = Encoding.UTF8.GetBytes(text);
		CreateAesStream(baseStream, password, bytes);
	}

	private void CreateAesStream(Stream baseStream, string password, byte[] salt)
	{
		_internalStream = baseStream;
		using PasswordDeriveBytes passwordDeriveBytes = new PasswordDeriveBytes(password, salt);
		_cryptor = new AesManaged();
		_cryptor.KeySize = 128;
		_cryptor.Mode = CipherMode.ECB;
		_cryptor.Padding = PaddingMode.None;
		_cryptor.Key = passwordDeriveBytes.GetBytes(_cryptor.KeySize / 8);
		_cryptor.IV = new byte[16];
		_encryptor = _cryptor.CreateEncryptor(_cryptor.Key, _cryptor.IV);
	}

	private void Cipher(byte[] buffer, int offset, int count, long streamPos)
	{
		int num = _cryptor.BlockSize / 8;
		long num2 = streamPos / num + 1;
		long num3 = streamPos % num;
		byte[] array = new byte[num];
		byte[] array2 = new byte[num];
		bool flag = false;
		for (int i = offset; i < count; i++)
		{
			if (!flag || num3 % num == 0L)
			{
				BitConverter.GetBytes(num2).CopyTo(array2, 0);
				_encryptor.TransformBlock(array2, 0, array2.Length, array, 0);
				if (flag)
				{
					num3 = 0L;
				}
				flag = true;
				num2++;
			}
			buffer[i] ^= array[num3];
			num3++;
		}
	}

	public override void Flush()
	{
		_internalStream.Flush();
	}

	public override void SetLength(long value)
	{
		_internalStream.SetLength(value);
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		return _internalStream.Seek(offset, origin);
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		long position = Position;
		int result = _internalStream.Read(buffer, offset, count);
		Cipher(buffer, offset, count, position);
		return result;
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		Cipher(buffer, offset, count, Position);
		_internalStream.Write(buffer, offset, count);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			_encryptor?.Dispose();
			_cryptor?.Dispose();
		}
		base.Dispose(disposing);
	}
}
