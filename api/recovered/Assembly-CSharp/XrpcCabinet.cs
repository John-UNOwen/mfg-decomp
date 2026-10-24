using System;
using System.Text;
using System.Xml;
using KAMUNITY;

public class XrpcCabinet : XrpcMethodBase
{
	private static string GetFacilityString(AvsEa3.FACILITY_PATH_STRING type)
	{
		StringBuilder stringBuilder = new StringBuilder(256);
		if (AvsEa3.Ea3QueryFacilityPathString(type, stringBuilder, stringBuilder.Capacity) > 0)
		{
			return stringBuilder.ToString();
		}
		return "";
	}

	protected override void ParseHandler(XrpcBase xrpc, object param)
	{
	}

	public static XrpcBase RequestError(string message, string stackTrace, string type, Action<XrpcBase, object> callback, object cb_param)
	{
		XmlNode xmlNode = XrpcUtil.CreateRequestNode("vfglog", "error_log");
		XmlDocument ownerDocument = xmlNode.OwnerDocument;
		xmlNode.AppendChild(XmlPropUtil.CreateNodeString(ownerDocument, "type", "error"));
		xmlNode.AppendChild(XmlPropUtil.CreateNodeString(ownerDocument, "location_id", GetFacilityString(AvsEa3.FACILITY_PATH_STRING.LOCATION_ID)));
		XmlNode xmlNode2 = xmlNode.AppendChild(ownerDocument.CreateElement("data"));
		xmlNode2.AppendChild(XmlPropUtil.CreateNodeString(ownerDocument, "error_type", type));
		xmlNode2.AppendChild(XmlPropUtil.CreateNodeString(ownerDocument, "message", message));
		xmlNode2.AppendChild(XmlPropUtil.CreateNodeString(ownerDocument, "stack_trace", stackTrace));
		XrpcCabinet xrpcCabinet = new XrpcCabinet();
		xrpcCabinet.SetCallback(callback, cb_param);
		return xrpcCabinet.ExecRequest(xmlNode, null);
	}
}
