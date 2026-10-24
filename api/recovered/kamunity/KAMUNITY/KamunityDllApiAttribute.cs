using System;

namespace KAMUNITY;

[AttributeUsage(AttributeTargets.Property)]
public class KamunityDllApiAttribute : Attribute
{
	public string functionName { get; set; }

	public KamunityDllApiAttribute(string functionName)
	{
		this.functionName = functionName;
	}
}
