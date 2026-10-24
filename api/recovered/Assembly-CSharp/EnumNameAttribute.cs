using System;

[AttributeUsage(AttributeTargets.Field)]
public class EnumNameAttribute : Attribute
{
	public string Name { get; set; }

	public EnumNameAttribute(string name)
	{
		Name = name;
	}
}
