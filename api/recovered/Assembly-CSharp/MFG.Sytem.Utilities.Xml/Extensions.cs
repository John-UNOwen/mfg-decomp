using System;
using System.Linq;
using System.Xml.Linq;

namespace MFG.Sytem.Utilities.Xml;

internal static class Extensions
{
	public static bool IsExistElement(this XElement baseElement, string s)
	{
		if (baseElement.Element(s) == null)
		{
			return false;
		}
		return true;
	}

	public static XElement MustNeedElement(this XElement baseElement, string s)
	{
		return baseElement.Element(s) ?? throw new MustNeedNodeNotFoundException();
	}

	public static XElement NeedNotElement(this XElement baseElement, string s)
	{
		return baseElement.Element(s) ?? throw new NeedNotNodeNotFoundException();
	}

	public static int AttributeValueInt(this XElement baseElement, string s)
	{
		return int.Parse(baseElement.Attribute(s).Value);
	}

	public static long AttributeValueInt64(this XElement baseElement, string s)
	{
		return long.Parse(baseElement.Attribute(s).Value);
	}

	public static bool ValueBool(this XElement baseElement)
	{
		if (int.Parse(baseElement.Value) == 0)
		{
			return false;
		}
		return true;
	}

	public static int ValueInt(this XElement baseElement)
	{
		return int.Parse(baseElement.Value);
	}

	public static uint ValueUInt(this XElement baseElement)
	{
		return uint.Parse(baseElement.Value);
	}

	public static int[] ValueArrayInt(this XElement baseElement)
	{
		return baseElement.Value.Split(' ').Select(int.Parse).ToArray();
	}

	public static long ValueInt64(this XElement baseElement)
	{
		return long.Parse(baseElement.Value);
	}

	public static ulong ValueUInt64(this XElement baseElement)
	{
		return ulong.Parse(baseElement.Value);
	}

	public static DateTime ValueToDateTime(this XElement baseElement)
	{
		return DateTime.Parse(baseElement.Value);
	}

	public static DateTime AttributeValueToDateTime(this XElement baseElement, string attributeString)
	{
		return DateTime.Parse(baseElement.Attribute(attributeString).Value);
	}

	public static DateTime ValueUInt64ToDateTime(this XElement baseElement)
	{
		ulong num = ulong.Parse(baseElement.Value);
		return TimeZoneInfo.ConvertTimeFromUtc(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(num), TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time"));
	}

	public static DateTime AttributeValueJavaCurrentTimeMillsToDateTime(this XElement baseElement, string attributeString)
	{
		ulong num = ulong.Parse(baseElement.Attribute(attributeString).Value);
		return TimeZoneInfo.ConvertTimeFromUtc(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(num), TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time"));
	}
}
