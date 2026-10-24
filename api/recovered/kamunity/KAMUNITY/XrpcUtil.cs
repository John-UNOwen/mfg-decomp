using System;
using System.Xml;

namespace KAMUNITY;

public static class XrpcUtil
{
	public static XmlNode CreateRequestNode(string module_name, string method_name)
	{
		XmlDocument xmlDocument = new XmlDocument();
		XmlNode xmlNode = xmlDocument.CreateElement(module_name);
		xmlDocument.AppendChild(xmlNode);
		xmlNode.Attributes.Append(xmlDocument.CreateAttribute("method")).InnerText = method_name;
		return xmlNode;
	}

	private static string CreatetXmlText(XmlNode node)
	{
		return node.OwnerDocument.InnerXml;
	}

	public static XrpcBase ExecRequest(XmlNode nodeRoot, Action<XrpcBase, object> callback, object cb_param, int xrpc_size = 4096)
	{
		string request_xml = CreatetXmlText(nodeRoot);
		string name = nodeRoot.Name;
		string innerText = nodeRoot.SelectSingleNode("@method").InnerText;
		return XrpcService.Instance.Request(name, innerText, request_xml, callback, cb_param, xrpc_size);
	}

	public static T TryGetNodeValue<T>(XmlNode node, string attr_name, T val_default)
	{
		XmlNode xmlNode = node.SelectSingleNode(attr_name);
		if (xmlNode == null)
		{
			return val_default;
		}
		return (T)Convert.ChangeType(xmlNode.InnerText, typeof(T));
	}

	public static string TryGetNodeString(XmlNode node, string select_path, string val_default)
	{
		XmlNode xmlNode = node.SelectSingleNode(select_path);
		if (xmlNode == null)
		{
			return val_default;
		}
		return xmlNode.InnerText;
	}

	public static bool Int2Bool(int val)
	{
		if (val == 0)
		{
			return false;
		}
		return true;
	}
}
