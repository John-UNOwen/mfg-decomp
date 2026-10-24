using System;
using System.Collections.Generic;
using System.IO;

namespace Compress;

internal class GZipWriteMemStream : Stream
{
	private long m_WriteSize;

	private List<byte[]> m_Data = new List<byte[]>();

	private byte[] m_Buff;

	private int m_Cur;

	private int m_SizeLine;

	public override bool CanRead => false;

	public override bool CanSeek => false;

	public override bool CanWrite => true;

	public override long Length => m_WriteSize;

	public override long Position
	{
		get
		{
			return m_WriteSize;
		}
		set
		{
		}
	}

	public long writeSize => m_WriteSize;

	public GZipWriteMemStream(int szLine = 4096)
	{
		if (szLine < 4096)
		{
			m_SizeLine = 4096;
		}
		else
		{
			m_SizeLine = szLine;
		}
	}

	public override void Flush()
	{
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		return -1;
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		return 0L;
	}

	public override void SetLength(long value)
	{
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		m_WriteSize += count;
		int num = count;
		while (num != 0)
		{
			if (m_Buff == null)
			{
				m_Buff = new byte[m_SizeLine];
				m_Cur = 0;
				m_Data.Add(m_Buff);
			}
			int num2 = m_Buff.Length - m_Cur;
			int num3 = ((num2 > num) ? num : num2);
			Array.Copy(buffer, offset + (count - num), m_Buff, m_Cur, num3);
			m_Cur += num3;
			num -= num3;
			if (m_Cur == m_Buff.Length)
			{
				m_Buff = null;
				m_Cur = 0;
			}
		}
	}

	public byte[] GetBytes()
	{
		byte[] array = new byte[m_WriteSize];
		long num = 0L;
		for (int i = 0; i < m_Data.Count; i++)
		{
			if (i + 1 == m_Data.Count)
			{
				long num2 = m_WriteSize - num;
				Array.Copy(m_Data[i], 0L, array, num, num2);
				num += num2;
			}
			else
			{
				Array.Copy(m_Data[i], 0L, array, num, m_Data[i].Length);
				num += m_Data[i].Length;
			}
		}
		return array;
	}
}
