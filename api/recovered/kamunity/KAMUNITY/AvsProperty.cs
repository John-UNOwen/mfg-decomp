using System;
using System.Text;
using UnityEngine;

namespace KAMUNITY;

public class AvsProperty : IDisposable
{
	private IntPtr m_prop = IntPtr.Zero;

	private byte[] m_buffer;

	public AvsProperty(AvsTypes.PROPERTY_MODE mode, int sz_buf)
	{
		m_buffer = new byte[sz_buf];
		m_prop = AvsPropertyApi.PropertyCreate?.Invoke((uint)mode, m_buffer, (uint)m_buffer.Length) ?? IntPtr.Zero;
	}

	public AvsProperty(byte[] xmldata, int sz_xmldata, AvsTypes.PROPERTY_MODE mode, int additionalBufSize = 0)
	{
		uint nr_node;
		uint bin_size;
		int num = AvsPropertyApi.PropertyDataQueryMemSize?.Invoke(xmldata, xmldata.Length, out nr_node, out bin_size) ?? 0;
		if (num > 0)
		{
			m_buffer = new byte[num + additionalBufSize + 1024];
			m_prop = AvsPropertyApi.PropertyMemRead?.Invoke(xmldata, sz_xmldata, (int)mode, m_buffer, m_buffer.Length) ?? IntPtr.Zero;
		}
	}

	public void Dispose()
	{
		if (m_prop != IntPtr.Zero)
		{
			AvsPropertyApi.PropertyDestroy?.Invoke(m_prop);
			m_prop = IntPtr.Zero;
		}
		m_buffer = null;
	}

	public uint GetError()
	{
		return AvsPropertyApi.PropertyGetError?.Invoke(m_prop) ?? 0;
	}

	public AvsPropertyNode NodeCreate(string path)
	{
		return new AvsPropertyNode(AvsPropertyApi.PropertyNodeCreateVoid?.Invoke(m_prop, IntPtr.Zero, path) ?? IntPtr.Zero);
	}

	public int SetSetBool(string path, bool value)
	{
		return AvsPropertyApi.PropertySetBool?.Invoke(m_prop, IntPtr.Zero, path, value) ?? (-1);
	}

	public int SetS32(string path, int value)
	{
		return AvsPropertyApi.PropertySetS32?.Invoke(m_prop, IntPtr.Zero, path, value) ?? (-1);
	}

	public int Set2S32(string path, Vector2Int value)
	{
		int[] values = new int[2] { value.x, value.y };
		return AvsPropertyApi.PropertySet2S32?.Invoke(m_prop, IntPtr.Zero, path, values) ?? (-1);
	}

	public int SetU32(string path, uint value)
	{
		return AvsPropertyApi.PropertySetU32?.Invoke(m_prop, IntPtr.Zero, path, value) ?? (-1);
	}

	public int SetS64(string path, long value)
	{
		return AvsPropertyApi.PropertySetS64?.Invoke(m_prop, IntPtr.Zero, path, value) ?? (-1);
	}

	public int SetU64(string path, ulong value)
	{
		return AvsPropertyApi.PropertySetU64?.Invoke(m_prop, IntPtr.Zero, path, value) ?? (-1);
	}

	public int SetString(string path, string value)
	{
		return AvsPropertyApi.PropertySetString?.Invoke(m_prop, IntPtr.Zero, path, value) ?? (-1);
	}

	public int SetAttribute(string path, string value)
	{
		return AvsPropertyApi.PropertySetAttribute?.Invoke(m_prop, IntPtr.Zero, path, value) ?? (-1);
	}

	public int SetBinary(string path, byte[] value)
	{
		return AvsPropertyApi.PropertySetBinary?.Invoke(m_prop, IntPtr.Zero, path, value, value.Length) ?? (-1);
	}

	public int GetBoolOrDefault(string path, out bool value, bool defaultValue = false)
	{
		value = defaultValue;
		return AvsPropertyApi.PropertyGetBool?.Invoke(m_prop, IntPtr.Zero, path, out value) ?? (-1);
	}

	public int GetS32OrDefault(string path, out int value, int defaultValue = 0)
	{
		value = defaultValue;
		return AvsPropertyApi.PropertyGetS32?.Invoke(m_prop, IntPtr.Zero, path, out value) ?? (-1);
	}

	public int Get2S32(string path, out Vector2Int value)
	{
		int[] array = new int[2];
		int result = AvsPropertyApi.PropertyGet2S32?.Invoke(m_prop, IntPtr.Zero, path, array) ?? (-1);
		value = new Vector2Int(array[0], array[1]);
		return result;
	}

	public int GetU32OrDefault(string path, out uint value, uint defaultValue = 0u)
	{
		value = defaultValue;
		return AvsPropertyApi.PropertyGetU32?.Invoke(m_prop, IntPtr.Zero, path, out value) ?? (-1);
	}

	public int GetS64OrDefault(string path, out long value, long defaultValue = 0L)
	{
		value = defaultValue;
		return AvsPropertyApi.PropertyGetS64?.Invoke(m_prop, IntPtr.Zero, path, out value) ?? (-1);
	}

	public int GetU64OrDefault(string path, out ulong value, ulong defaultValue = 0uL)
	{
		value = defaultValue;
		return AvsPropertyApi.PropertyGetU64?.Invoke(m_prop, IntPtr.Zero, path, out value) ?? (-1);
	}

	public int GetString(string path, StringBuilder sb)
	{
		return AvsPropertyApi.PropertyGetString?.Invoke(m_prop, IntPtr.Zero, path, sb, sb.Capacity) ?? (-1);
	}

	public int GetAttribute(string path, StringBuilder sb)
	{
		return AvsPropertyApi.PropertyGetAttribute?.Invoke(m_prop, IntPtr.Zero, path, sb, sb.Capacity) ?? (-1);
	}

	public int GetBinary(string path, byte[] buf)
	{
		return AvsPropertyApi.PropertyGetBinary?.Invoke(m_prop, IntPtr.Zero, path, buf, buf.Length) ?? (-1);
	}

	private void SetModeXml()
	{
		AvsPropertyApi.PropertySetFlag(m_prop, 0u, 2056u);
	}

	private void SetModeBin()
	{
		AvsPropertyApi.PropertySetFlag(m_prop, 8u, 2056u);
	}

	public int QuerySize()
	{
		return (int)(AvsPropertyApi.PropertyQuerySize?.Invoke(m_prop) ?? 0);
	}

	public AvsPropertyNode Search(string path)
	{
		return new AvsPropertyNode(AvsPropertyApi.PropertySearch?.Invoke(m_prop, IntPtr.Zero, path) ?? IntPtr.Zero);
	}

	public byte[] ToBinaryXml()
	{
		SetModeBin();
		if (QuerySize() > 0)
		{
			byte[] array = new byte[QuerySize()];
			AvsPropertyApi.PropertyMemWrite?.Invoke(m_prop, array, array.Length);
			return array;
		}
		return null;
	}

	public string ToStringXml()
	{
		SetModeXml();
		int num = QuerySize();
		if (num > 0)
		{
			byte[] array = new byte[num];
			AvsPropertyApi.PropertyMemWrite?.Invoke(m_prop, array, array.Length);
			return Encoding.UTF8.GetString(array);
		}
		return null;
	}
}
