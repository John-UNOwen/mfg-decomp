using System;
using System.Reflection;

public static class EnumExtension
{
	public static string GetEnumName<T>(this T enumValue)
	{
		FieldInfo field = typeof(T).GetField(enumValue.ToString());
		Type typeFromHandle = typeof(EnumNameAttribute);
		return (Attribute.GetCustomAttribute(field, typeFromHandle) as EnumNameAttribute)?.Name;
	}
}
