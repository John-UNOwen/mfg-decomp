using System;
using System.Xml;

namespace KAMUNITY;

public static class XrpcModuleCardmng
{
	public class XrpcMethodAuthpass : XrpcMethodBase
	{
		public static XrpcBase Request(string refid, string pass, Action<XrpcBase, object> callback, object cb_param)
		{
			XmlNode xmlNode = XrpcUtil.CreateRequestNode("cardmng", "authpass");
			XmlDocument ownerDocument = xmlNode.OwnerDocument;
			xmlNode.Attributes.Append(ownerDocument.CreateAttribute("refid")).InnerText = refid;
			xmlNode.Attributes.Append(ownerDocument.CreateAttribute("pass")).InnerText = pass;
			XrpcMethodAuthpass xrpcMethodAuthpass = new XrpcMethodAuthpass();
			xrpcMethodAuthpass.SetCallback(callback, cb_param);
			return xrpcMethodAuthpass.ExecRequest(xmlNode, null);
		}

		protected override void ParseHandler(XrpcBase xrpc, object param)
		{
		}
	}

	public class XrpcMethodBindmodel : XrpcMethodBase<XrpcModuleCardmngTypes.CardmngBindmodelResponse>
	{
		public static XrpcBase Request(string model, string refid, bool newflg, Action<XrpcBase, XrpcModuleCardmngTypes.CardmngBindmodelResponse, object> callback, object cb_param)
		{
			XmlNode xmlNode = XrpcUtil.CreateRequestNode("cardmng", "bindmodel");
			XmlDocument ownerDocument = xmlNode.OwnerDocument;
			if (!string.IsNullOrEmpty(model))
			{
				xmlNode.Attributes.Append(ownerDocument.CreateAttribute("model")).InnerText = model;
			}
			xmlNode.Attributes.Append(ownerDocument.CreateAttribute("refid")).InnerText = refid;
			xmlNode.Attributes.Append(ownerDocument.CreateAttribute("newflag")).InnerText = (newflg ? "1" : "0");
			XrpcMethodBindmodel xrpcMethodBindmodel = new XrpcMethodBindmodel();
			xrpcMethodBindmodel.SetCallback(callback, cb_param);
			return xrpcMethodBindmodel.ExecRequest(xmlNode, null);
		}

		protected override XrpcModuleCardmngTypes.CardmngBindmodelResponse ParseHandler(XrpcBase xrpc, object cb_param)
		{
			XrpcModuleCardmngTypes.CardmngBindmodelResponse cardmngBindmodelResponse = new XrpcModuleCardmngTypes.CardmngBindmodelResponse();
			XmlNode responseXmlNode = xrpc.GetResponseXmlNode();
			cardmngBindmodelResponse.dataid = responseXmlNode.SelectSingleNode("@dataid").InnerText;
			return cardmngBindmodelResponse;
		}
	}

	public class XrpcMethodGetdatalist : XrpcMethodBase<XrpcModuleCardmngTypes.CardmngGetdatalistResponse>
	{
		public static XrpcBase Request(string refid, Action<XrpcBase, XrpcModuleCardmngTypes.CardmngGetdatalistResponse, object> callback, object cb_param)
		{
			XmlNode xmlNode = XrpcUtil.CreateRequestNode("cardmng", "getdatalist");
			XmlDocument ownerDocument = xmlNode.OwnerDocument;
			xmlNode.Attributes.Append(ownerDocument.CreateAttribute("refid")).InnerText = refid;
			XrpcMethodGetdatalist xrpcMethodGetdatalist = new XrpcMethodGetdatalist();
			xrpcMethodGetdatalist.SetCallback(callback, cb_param);
			return xrpcMethodGetdatalist.ExecRequest(xmlNode, null);
		}

		protected override XrpcModuleCardmngTypes.CardmngGetdatalistResponse ParseHandler(XrpcBase xrpc, object cb_param)
		{
			XrpcModuleCardmngTypes.CardmngGetdatalistResponse result = new XrpcModuleCardmngTypes.CardmngGetdatalistResponse();
			xrpc.GetResponseXmlNode();
			return result;
		}
	}

	public class XrpcMethdoGetrefid : XrpcMethodBase<XrpcModuleCardmngTypes.CardmngGetrefidResponse>
	{
		public static XrpcBase Request(string model, AvsTypes.CardType type, string cardid, bool newflg, string passwd, Action<XrpcBase, XrpcModuleCardmngTypes.CardmngGetrefidResponse, object> callback, object cb_param)
		{
			XmlNode xmlNode = XrpcUtil.CreateRequestNode("cardmng", "getrefid");
			XmlDocument ownerDocument = xmlNode.OwnerDocument;
			if (!string.IsNullOrEmpty(model))
			{
				xmlNode.Attributes.Append(ownerDocument.CreateAttribute("model")).InnerText = model;
			}
			XmlAttribute xmlAttribute = xmlNode.Attributes.Append(ownerDocument.CreateAttribute("cardtype"));
			int num = (int)type;
			xmlAttribute.InnerText = num.ToString();
			xmlNode.Attributes.Append(ownerDocument.CreateAttribute("cardid")).InnerText = cardid;
			xmlNode.Attributes.Append(ownerDocument.CreateAttribute("newflag")).InnerText = (newflg ? "1" : "0");
			xmlNode.Attributes.Append(ownerDocument.CreateAttribute("passwd")).InnerText = passwd;
			XrpcMethdoGetrefid xrpcMethdoGetrefid = new XrpcMethdoGetrefid();
			xrpcMethdoGetrefid.SetCallback(callback, cb_param);
			return xrpcMethdoGetrefid.ExecRequest(xmlNode, null);
		}

		protected override XrpcModuleCardmngTypes.CardmngGetrefidResponse ParseHandler(XrpcBase xrpc, object param)
		{
			XrpcModuleCardmngTypes.CardmngGetrefidResponse cardmngGetrefidResponse = new XrpcModuleCardmngTypes.CardmngGetrefidResponse();
			XmlNode responseXmlNode = xrpc.GetResponseXmlNode();
			cardmngGetrefidResponse.refid = XrpcUtil.TryGetNodeString(responseXmlNode, "@refid", "");
			cardmngGetrefidResponse.dataid = XrpcUtil.TryGetNodeString(responseXmlNode, "@dataid", "");
			return cardmngGetrefidResponse;
		}
	}

	public class XrpcMethodInquire : XrpcMethodBase<XrpcModuleCardmngTypes.CardmngInquireResponse>
	{
		public static XrpcBase Request(string model, AvsTypes.CardType type, string cardid, bool update, Action<XrpcBase, XrpcModuleCardmngTypes.CardmngInquireResponse, object> callback, object cb_param)
		{
			XmlNode xmlNode = XrpcUtil.CreateRequestNode("cardmng", "inquire");
			XmlDocument ownerDocument = xmlNode.OwnerDocument;
			if (!string.IsNullOrEmpty(model))
			{
				xmlNode.Attributes.Append(ownerDocument.CreateAttribute("model")).InnerText = model;
			}
			XmlAttribute xmlAttribute = xmlNode.Attributes.Append(ownerDocument.CreateAttribute("cardtype"));
			int num = (int)type;
			xmlAttribute.InnerText = num.ToString();
			xmlNode.Attributes.Append(ownerDocument.CreateAttribute("cardid")).InnerText = cardid;
			xmlNode.Attributes.Append(ownerDocument.CreateAttribute("update")).InnerText = (update ? "1" : "0");
			XrpcMethodInquire xrpcMethodInquire = new XrpcMethodInquire();
			xrpcMethodInquire.SetCallback(callback, cb_param);
			return xrpcMethodInquire.ExecRequest(xmlNode, null);
		}

		public static XrpcBase Request(AvsTypes.CardType type, string cardid, bool update, Action<XrpcBase, XrpcModuleCardmngTypes.CardmngInquireResponse, object> callback, object cb_param)
		{
			return Request(null, type, cardid, update, callback, cb_param);
		}

		protected override XrpcModuleCardmngTypes.CardmngInquireResponse ParseHandler(XrpcBase xrpc, object cb_param)
		{
			XrpcModuleCardmngTypes.CardmngInquireResponse cardmngInquireResponse = new XrpcModuleCardmngTypes.CardmngInquireResponse();
			XmlNode responseXmlNode = xrpc.GetResponseXmlNode();
			cardmngInquireResponse.refid = XrpcUtil.TryGetNodeString(responseXmlNode, "@refid", "");
			cardmngInquireResponse.binded = XrpcUtil.Int2Bool(XrpcUtil.TryGetNodeValue(responseXmlNode, "@binded", 0));
			if (cardmngInquireResponse.binded)
			{
				cardmngInquireResponse.dataid = responseXmlNode.SelectSingleNode("@dataid").InnerText;
				cardmngInquireResponse.lastupdate = Utility.Epoch2DateTime(long.Parse(responseXmlNode.SelectSingleNode("@lastupdate").InnerText));
			}
			else
			{
				cardmngInquireResponse.dataid = "";
				cardmngInquireResponse.lastupdate = DateTime.MinValue;
			}
			cardmngInquireResponse.expired = XrpcUtil.Int2Bool(XrpcUtil.TryGetNodeValue(responseXmlNode, "@expired", 0));
			cardmngInquireResponse.ecflag = XrpcUtil.Int2Bool(XrpcUtil.TryGetNodeValue(responseXmlNode, "@ecflag", 0));
			cardmngInquireResponse.useridflag = XrpcUtil.Int2Bool(XrpcUtil.TryGetNodeValue(responseXmlNode, "@useridflag", 0));
			cardmngInquireResponse.extidflag = XrpcUtil.Int2Bool(XrpcUtil.TryGetNodeValue(responseXmlNode, "@extidflag", 0));
			cardmngInquireResponse.quickec = XrpcUtil.Int2Bool(XrpcUtil.TryGetNodeValue(responseXmlNode, "@quickec", 0));
			return cardmngInquireResponse;
		}
	}

	public static void AddModule()
	{
		AvsEa3Xrpc.Ea3XrpcAddModule("cardmng", "cardmng", new string[5] { "inquire", "getrefid", "bindmodel", "authpass", "getdatalist" });
	}

	[Obsolete("use XrpcMethodAuthpass.Request")]
	public static XrpcBase Authpass(string refid, string pass, Action<XrpcBase, object> callback, object cb_param)
	{
		return XrpcMethodAuthpass.Request(refid, pass, callback, cb_param);
	}

	[Obsolete("use XrpcMethodBindModel.Request")]
	public static XrpcBase Bindmodel(string model, string refid, bool newflg, Action<XrpcBase, XrpcModuleCardmngTypes.CardmngBindmodelResponse, object> callback, object cb_param)
	{
		return XrpcMethodBindmodel.Request(model, refid, newflg, callback, cb_param);
	}

	[Obsolete("use XrpcMethodGetdatalist.Request")]
	public static XrpcBase Getdatalist(string refid, Action<XrpcBase, XrpcModuleCardmngTypes.CardmngGetdatalistResponse, object> callback, object cb_param)
	{
		return XrpcMethodGetdatalist.Request(refid, callback, cb_param);
	}

	[Obsolete("use XrpcMethdoGetrefid.Request")]
	public static XrpcBase Getrefid(string model, AvsTypes.CardType type, string cardid, bool newflg, string passwd, Action<XrpcBase, XrpcModuleCardmngTypes.CardmngGetrefidResponse, object> callback, object cb_param)
	{
		return XrpcMethdoGetrefid.Request(model, type, cardid, newflg, passwd, callback, cb_param);
	}

	[Obsolete("use XrpcMethodInquire.Request")]
	public static XrpcBase Inquire(string model, AvsTypes.CardType type, string cardid, bool update, Action<XrpcBase, XrpcModuleCardmngTypes.CardmngInquireResponse, object> callback, object cb_param)
	{
		return XrpcMethodInquire.Request(model, type, cardid, update, callback, cb_param);
	}

	[Obsolete("use XrpcMethodInquire.Request")]
	public static XrpcBase Inquire(AvsTypes.CardType type, string cardid, bool update, Action<XrpcBase, XrpcModuleCardmngTypes.CardmngInquireResponse, object> callback, object cb_param)
	{
		return Inquire(null, type, cardid, update, callback, cb_param);
	}
}
