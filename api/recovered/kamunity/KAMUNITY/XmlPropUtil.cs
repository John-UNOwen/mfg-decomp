using System;
using System.Text;
using System.Xml;

namespace KAMUNITY;

public static class XmlPropUtil
{
	public static XmlNode CreateNodeBoolean(XmlDocument xml, string name, bool value)
	{
		XmlElement xmlElement = xml.CreateElement(name);
		xmlElement.Attributes.Append(xml.CreateAttribute("__type")).InnerText = "bool";
		xmlElement.InnerText = (value ? "1" : "0");
		return xmlElement;
	}

	public static XmlNode CreateNodeString(XmlDocument xml, string name, string value)
	{
		XmlElement xmlElement = xml.CreateElement(name);
		xmlElement.Attributes.Append(xml.CreateAttribute("__type")).InnerText = "str";
		xmlElement.InnerText = value;
		return xmlElement;
	}

	public static XmlNode CreateNodeS32(XmlDocument xml, string name, int value)
	{
		XmlElement xmlElement = xml.CreateElement(name);
		xmlElement.Attributes.Append(xml.CreateAttribute("__type")).InnerText = "s32";
		xmlElement.InnerText = value.ToString();
		return xmlElement;
	}

	public static XmlNode CreateNodeS64(XmlDocument xml, string name, long value)
	{
		XmlElement xmlElement = xml.CreateElement(name);
		xmlElement.Attributes.Append(xml.CreateAttribute("__type")).InnerText = "s64";
		xmlElement.InnerText = value.ToString();
		return xmlElement;
	}

	public static XmlNode CreateNodeU64(XmlDocument xml, string name, ulong value)
	{
		XmlElement xmlElement = xml.CreateElement(name);
		xmlElement.Attributes.Append(xml.CreateAttribute("__type")).InnerText = "u64";
		xmlElement.InnerText = value.ToString();
		return xmlElement;
	}

	public static XmlNode CreateNodeS32Array(XmlDocument xml, string name, int[] value)
	{
		XmlNode xmlNode = xml.CreateElement(name);
		xmlNode.Attributes.Append(xml.CreateAttribute("__type")).InnerText = "s32";
		xmlNode.Attributes.Append(xml.CreateAttribute("__count")).InnerText = value.Length.ToString();
		StringBuilder stringBuilder = new StringBuilder();
		if (value.Length >= 1)
		{
			stringBuilder.Append(value[0].ToString());
		}
		for (int i = 1; i < value.Length; i++)
		{
			stringBuilder.AppendFormat(" {0}", value[i].ToString());
		}
		xmlNode.InnerText = stringBuilder.ToString();
		return xmlNode;
	}

	public static XmlNode CreateNodeS64Array(XmlDocument xml, string name, long[] value)
	{
		XmlNode xmlNode = xml.CreateElement(name);
		xmlNode.Attributes.Append(xml.CreateAttribute("__type")).InnerText = "s64";
		xmlNode.Attributes.Append(xml.CreateAttribute("__count")).InnerText = value.Length.ToString();
		StringBuilder stringBuilder = new StringBuilder();
		if (value.Length >= 1)
		{
			stringBuilder.Append(value[0].ToString());
		}
		for (int i = 1; i < value.Length; i++)
		{
			stringBuilder.AppendFormat(" {0}", value[i].ToString());
		}
		xmlNode.InnerText = stringBuilder.ToString();
		return xmlNode;
	}

	public static bool ReadNodeBoolean(XmlNode node, string path)
	{
		bool dst = false;
		if (!TryReadNodeBoolean(node, path, ref dst))
		{
			throw new XmlPathNotFoundException();
		}
		return dst;
	}

	public static bool ReadNodeBooleanWithDefault(XmlNode node, string path, bool default_val)
	{
		bool dst = default_val;
		TryReadNodeBoolean(node, path, ref dst);
		return dst;
	}

	public static bool TryReadNodeBoolean(XmlNode node, string path, ref bool dst)
	{
		XmlNode xmlNode = node.SelectSingleNode(path);
		if (xmlNode != null)
		{
			dst = int.Parse(xmlNode.InnerText) != 0;
			return true;
		}
		return false;
	}

	public static string ReadNodeString(XmlNode node, string path)
	{
		string dst = null;
		if (!TryReadNodeString(node, path, ref dst))
		{
			throw new XmlPathNotFoundException();
		}
		return dst;
	}

	public static string ReadNodeStringWithDefault(XmlNode node, string path, string default_val)
	{
		string dst = default_val;
		TryReadNodeString(node, path, ref dst);
		return dst;
	}

	public static bool TryReadNodeString(XmlNode node, string path, ref string dst)
	{
		XmlNode xmlNode = node.SelectSingleNode(path);
		if (xmlNode != null)
		{
			dst = xmlNode.InnerText;
			return true;
		}
		return false;
	}

	public static int ReadNodeS32(XmlNode node, string path)
	{
		int dst = 0;
		if (!TryReadNodeS32(node, path, ref dst))
		{
			throw new XmlPathNotFoundException();
		}
		return dst;
	}

	public static int ReadNodeS32WithDefault(XmlNode node, string path, int default_val)
	{
		int dst = default_val;
		TryReadNodeS32(node, path, ref dst);
		return dst;
	}

	public static bool TryReadNodeS32(XmlNode node, string path, ref int dst)
	{
		XmlNode xmlNode = node.SelectSingleNode(path);
		if (xmlNode != null)
		{
			dst = int.Parse(xmlNode.InnerText);
			return true;
		}
		return false;
	}

	public static long ReadNodeS64(XmlNode node, string path)
	{
		long dst = 0L;
		if (!TryReadNodeS64(node, path, ref dst))
		{
			throw new XmlPathNotFoundException();
		}
		return dst;
	}

	public static long ReadNodeS64WithDefault(XmlNode node, string path, long default_val)
	{
		long dst = default_val;
		TryReadNodeS64(node, path, ref dst);
		return dst;
	}

	public static bool TryReadNodeS64(XmlNode node, string path, ref long dst)
	{
		XmlNode xmlNode = node.SelectSingleNode(path);
		if (xmlNode != null)
		{
			dst = long.Parse(xmlNode.InnerText);
			return true;
		}
		return false;
	}

	public static ulong ReadNodeU64(XmlNode node, string path)
	{
		ulong dst = 0uL;
		if (!TryReadNodeU64(node, path, ref dst))
		{
			throw new XmlPathNotFoundException();
		}
		return dst;
	}

	public static ulong ReadNodeU64WithDefault(XmlNode node, string path, ulong default_val)
	{
		ulong dst = default_val;
		TryReadNodeU64(node, path, ref dst);
		return dst;
	}

	public static bool TryReadNodeU64(XmlNode node, string path, ref ulong dst)
	{
		XmlNode xmlNode = node.SelectSingleNode(path);
		if (xmlNode != null)
		{
			dst = ulong.Parse(xmlNode.InnerText);
			return true;
		}
		return false;
	}

	public static DateTime ReadNodeDate(XmlNode node, string path)
	{
		DateTime dst = default(DateTime);
		if (!TryReadNodeDate(node, path, ref dst))
		{
			throw new XmlPathNotFoundException();
		}
		return dst;
	}

	public static DateTime ReadNodeDateWithDefault(XmlNode node, string path, DateTime default_val)
	{
		DateTime dst = default_val;
		TryReadNodeDate(node, path, ref dst);
		return dst;
	}

	public static bool TryReadNodeDate(XmlNode node, string path, ref DateTime dst)
	{
		string dst2 = null;
		if (TryReadNodeString(node, path, ref dst2))
		{
			if (dst2.CompareTo("infinity") == 0)
			{
				dst = DateTime.MaxValue;
			}
			else if (dst2.CompareTo("-infinity") == 0)
			{
				dst = DateTime.MinValue;
			}
			else
			{
				dst = DateTime.Parse(dst2);
			}
			return true;
		}
		return false;
	}

	public static DateTime ReadNodeUnixTimeWithDefault(XmlNode node, string path, DateTime default_val)
	{
		DateTime result = default_val;
		int dst = 0;
		if (TryReadNodeS32(node, path, ref dst))
		{
			result = DateTimeOffset.FromUnixTimeSeconds(dst).LocalDateTime;
		}
		return result;
	}
}
