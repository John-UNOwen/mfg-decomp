using System.Xml;
using System.Xml.Linq;

namespace KAMUNITY;

public static class XrpcBaseExtensions
{
	public static XDocument GetResponseXDocment(this XrpcBase xrpc)
	{
		using XmlNodeReader xmlNodeReader = new XmlNodeReader(xrpc.GetResponseXmlNode());
		xmlNodeReader.MoveToContent();
		return XDocument.Load(xmlNodeReader);
	}
}
