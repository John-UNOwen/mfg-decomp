using System;
using System.Text;
using UnityEngine;

namespace KAMUNITY;

public class AvsPropertyNode : IDisposable
{
	private IntPtr m_node = IntPtr.Zero;

	public IntPtr NativeNode => m_node;

	public bool IsNull => m_node == IntPtr.Zero;

	public AvsPropertyNode(IntPtr node)
	{
		m_node = node;
	}

	public void Dispose()
	{
		m_node = IntPtr.Zero;
	}

	public AvsPropertyNode NodeCreate(string path)
	{
		return new AvsPropertyNode(AvsPropertyApi.PropertyNodeCreateVoid?.Invoke(IntPtr.Zero, m_node, path) ?? IntPtr.Zero);
	}

	public int SetBool(string path, bool value)
	{
		return AvsPropertyApi.PropertySetBool?.Invoke(IntPtr.Zero, m_node, path, value) ?? (-1);
	}

	public int SetS32(string path, int value)
	{
		return AvsPropertyApi.PropertySetS32?.Invoke(IntPtr.Zero, m_node, path, value) ?? (-1);
	}

	public int Set2S32(string path, Vector2Int value)
	{
		int[] values = new int[2] { value.x, value.y };
		return AvsPropertyApi.PropertySet2S32?.Invoke(IntPtr.Zero, m_node, path, values) ?? (-1);
	}

	public int SetU32(string path, uint value)
	{
		return AvsPropertyApi.PropertySetU32?.Invoke(IntPtr.Zero, m_node, path, value) ?? (-1);
	}

	public int SetS64(string path, long value)
	{
		return AvsPropertyApi.PropertySetS64?.Invoke(IntPtr.Zero, m_node, path, value) ?? (-1);
	}

	public int SetU64(string path, ulong value)
	{
		return AvsPropertyApi.PropertySetU64?.Invoke(IntPtr.Zero, m_node, path, value) ?? (-1);
	}

	public int SetString(string path, string value)
	{
		return AvsPropertyApi.PropertySetString?.Invoke(IntPtr.Zero, m_node, path, value) ?? (-1);
	}

	public int SetAttribute(string path, string value)
	{
		return AvsPropertyApi.PropertySetAttribute?.Invoke(IntPtr.Zero, m_node, path, value) ?? (-1);
	}

	public int SetBinary(string path, byte[] value)
	{
		return AvsPropertyApi.PropertySetBinary?.Invoke(IntPtr.Zero, m_node, path, value, value.Length) ?? (-1);
	}

	public int GetBoolOrDefault(string path, out bool value, bool defaultValue = false)
	{
		value = defaultValue;
		return AvsPropertyApi.PropertyGetBool?.Invoke(IntPtr.Zero, m_node, path, out value) ?? (-1);
	}

	public int GetS32OrDefault(string path, out int value, int defaultValue = 0)
	{
		value = defaultValue;
		return AvsPropertyApi.PropertyGetS32?.Invoke(IntPtr.Zero, m_node, path, out value) ?? (-1);
	}

	public int Get2S32(string path, out Vector2Int value)
	{
		int[] array = new int[2];
		int result = AvsPropertyApi.PropertyGet2S32?.Invoke(IntPtr.Zero, m_node, path, array) ?? (-1);
		value = new Vector2Int(array[0], array[1]);
		return result;
	}

	public int GetU32OrDefault(string path, out uint value, uint defaultValue = 0u)
	{
		value = defaultValue;
		return AvsPropertyApi.PropertyGetU32?.Invoke(IntPtr.Zero, m_node, path, out value) ?? (-1);
	}

	public int GetS64OrDefault(string path, out long value, long defaultValue = 0L)
	{
		value = defaultValue;
		return AvsPropertyApi.PropertyGetS64?.Invoke(IntPtr.Zero, m_node, path, out value) ?? (-1);
	}

	public int GetU64OrDefault(string path, out ulong value, ulong defaultValue = 0uL)
	{
		value = defaultValue;
		return AvsPropertyApi.PropertyGetU64?.Invoke(IntPtr.Zero, m_node, path, out value) ?? (-1);
	}

	public int GetString(string path, StringBuilder sb)
	{
		return AvsPropertyApi.PropertyGetString?.Invoke(IntPtr.Zero, m_node, path, sb, sb.Capacity) ?? (-1);
	}

	public int GetAttribute(string path, StringBuilder sb)
	{
		return AvsPropertyApi.PropertyGetAttribute?.Invoke(IntPtr.Zero, m_node, path, sb, sb.Capacity) ?? (-1);
	}

	public int GetBinary(string path, byte[] buf)
	{
		return AvsPropertyApi.PropertyGetBinary?.Invoke(IntPtr.Zero, m_node, path, buf, buf.Length) ?? (-1);
	}

	public AvsPropertyNode Search(string path)
	{
		return new AvsPropertyNode(AvsPropertyApi.PropertySearch?.Invoke(IntPtr.Zero, m_node, path) ?? IntPtr.Zero);
	}

	public AvsPropertyNode Parent()
	{
		return new AvsPropertyNode(AvsPropertyApi.PropertyNodeTraversal?.Invoke(m_node, 0u) ?? IntPtr.Zero);
	}

	public AvsPropertyNode Child()
	{
		return new AvsPropertyNode(AvsPropertyApi.PropertyNodeTraversal?.Invoke(m_node, 1u) ?? IntPtr.Zero);
	}

	public AvsPropertyNode FirstNode()
	{
		return new AvsPropertyNode(AvsPropertyApi.PropertyNodeTraversal?.Invoke(m_node, 3u) ?? IntPtr.Zero);
	}

	public AvsPropertyNode Prev()
	{
		return new AvsPropertyNode(AvsPropertyApi.PropertyNodeTraversal?.Invoke(m_node, 5u) ?? IntPtr.Zero);
	}

	public AvsPropertyNode Next()
	{
		return new AvsPropertyNode(AvsPropertyApi.PropertyNodeTraversal?.Invoke(m_node, 4u) ?? IntPtr.Zero);
	}

	public AvsPropertyNode LastNode()
	{
		return new AvsPropertyNode(AvsPropertyApi.PropertyNodeTraversal?.Invoke(m_node, 6u) ?? IntPtr.Zero);
	}

	public AvsPropertyNode PrevSameName()
	{
		return new AvsPropertyNode(AvsPropertyApi.PropertyNodeTraversal?.Invoke(m_node, 8u) ?? IntPtr.Zero);
	}

	public AvsPropertyNode NextSameName()
	{
		return new AvsPropertyNode(AvsPropertyApi.PropertyNodeTraversal?.Invoke(m_node, 7u) ?? IntPtr.Zero);
	}

	public string DumpStringXml()
	{
		IntPtr prop = AvsPropertyApi.PropertyNodeGetDesc?.Invoke(m_node) ?? IntPtr.Zero;
		AvsPropertyApi.PropertySetFlag(prop, 0u, 2056u);
		int num = (int)(AvsPropertyApi.PropertyQuerySize?.Invoke(prop) ?? 0);
		if (num > 0)
		{
			byte[] array = new byte[num];
			AvsPropertyApi.PropertyMemWrite(prop, array, array.Length);
			return Encoding.UTF8.GetString(array);
		}
		return "";
	}
}
