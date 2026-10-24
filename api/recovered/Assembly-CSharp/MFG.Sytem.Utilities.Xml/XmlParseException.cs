using System;

namespace MFG.Sytem.Utilities.Xml;

public class XmlParseException : Exception
{
	public XmlParseException()
	{
	}

	public XmlParseException(string message)
		: base(message)
	{
	}

	public XmlParseException(string message, Exception inner)
		: base(message, inner)
	{
	}
}
