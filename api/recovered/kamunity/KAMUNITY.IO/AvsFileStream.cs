using System;
using System.IO;

namespace KAMUNITY.IO;

public class AvsFileStream : Stream
{
	private string m_Path;

	private AvsTypes.FS_FLAG m_Flag;

	private AvsTypes.FS_MODE m_Mode;

	private int m_Fd;

	public override bool CanRead => (m_Flag & AvsTypes.FS_FLAG.RDONLY) != 0;

	public override bool CanSeek => true;

	public override bool CanWrite => (m_Flag & AvsTypes.FS_FLAG.WRONLY) != 0;

	public override long Length => Avs.FsFsize(m_Fd);

	public override long Position
	{
		get
		{
			int num = Avs.FsSeek(m_Fd, 0L, AvsTypes.FS_SEEK.CUR);
			if (num < 0)
			{
				throw new InvalidOperationException(num.ToString("X"));
			}
			return num;
		}
		set
		{
			int num = Avs.FsSeek(m_Fd, value, AvsTypes.FS_SEEK.SET);
			if (num < 0)
			{
				throw new IOException(num.ToString("X"));
			}
		}
	}

	public AvsFileStream(string path, AvsTypes.FS_FLAG flag, AvsTypes.FS_MODE mode)
	{
		m_Path = path;
		m_Flag = flag;
		m_Mode = mode;
		m_Fd = Avs.FsOpen(path, flag, mode);
		if (m_Fd <= 0)
		{
			if (m_Fd == -2147024894)
			{
				throw new FileNotFoundException(m_Fd.ToString("X"));
			}
			throw new IOException(m_Fd.ToString("X"));
		}
	}

	public override void Close()
	{
		if (m_Fd > 0)
		{
			Avs.FsClose(m_Fd);
			m_Fd = 0;
		}
		base.Close();
	}

	public override void Flush()
	{
		int num = Avs.FsFlush(m_Fd);
		if (num != 0)
		{
			throw new IOException(num.ToString("X"));
		}
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		if (offset != 0)
		{
			byte[] array = new byte[count];
			int num = Avs.FsRead(m_Fd, array, count);
			if (num < 0)
			{
				throw new IOException(num.ToString());
			}
			Array.Copy(array, 0, buffer, offset, count);
			return num;
		}
		int num2 = Avs.FsRead(m_Fd, buffer, count);
		if (num2 < 0)
		{
			throw new IOException(num2.ToString());
		}
		return num2;
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		int num = Avs.FsSeek(m_Fd, offset, origin switch
		{
			SeekOrigin.Begin => AvsTypes.FS_SEEK.SET, 
			SeekOrigin.Current => AvsTypes.FS_SEEK.CUR, 
			_ => AvsTypes.FS_SEEK.END, 
		});
		if (num < 0)
		{
			throw new IOException(num.ToString());
		}
		return num;
	}

	public override void SetLength(long value)
	{
		throw new NotImplementedException();
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		if (offset != 0)
		{
			byte[] array = new byte[count];
			Array.Copy(buffer, offset, array, 0, count);
			int num = Avs.FsWrite(m_Fd, array, count);
			if (num < 0)
			{
				throw new IOException(num.ToString());
			}
		}
		else
		{
			int num2 = Avs.FsWrite(m_Fd, buffer, count);
			if (num2 < 0)
			{
				throw new IOException(num2.ToString());
			}
		}
	}
}
