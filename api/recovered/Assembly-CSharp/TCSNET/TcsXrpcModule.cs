using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using GameContents;
using KAMUNITY;
using UnityEngine;

namespace TCSNET;

public static class TcsXrpcModule
{
	public class XrpcMethodGetConsumeLog : XrpcMethodBase<TcsXrpcModuleTypes.GetConsumeLogResponse>
	{
		public static XrpcBase Request(CONSUME_MODE mode, int price, Action<XrpcBase, TcsXrpcModuleTypes.GetConsumeLogResponse, object> callback, object cb_param)
		{
			int num = 100;
			int num2 = 1000;
			int num3 = 1000;
			int num4 = 1000;
			try
			{
				if (GameUtility.IsNetcafeBehavior && GameDataManager.System.GameControlInfo.EventStatus.IsActive(GameEventType.ExternalBasicRate))
				{
					GameEventData gameEventData = GameDataManager.System.GameControlInfo.EventStatus.GetGameEventData(GameEventType.ExternalBasicRate);
					num2 = gameEventData.ParamValue("gacha_rate", 1000);
					num3 = gameEventData.ParamValue("play_rate", 1000);
					num4 = gameEventData.ParamValue("music_gacha_rate", 1000);
				}
			}
			catch
			{
			}
			switch (mode)
			{
			case CONSUME_MODE.START_CREDIT:
			case CONSUME_MODE.START_PASELI:
				num = 100 * num3 / 1000;
				break;
			case CONSUME_MODE.PREMIUM_START_PASELI:
			case CONSUME_MODE.PREMIUM_START_CREDIT:
				num = 200 * num3 / 1000;
				break;
			case CONSUME_MODE.CONTINUE_CREDIT:
			case CONSUME_MODE.CONTINUE_PASELI:
			case CONSUME_MODE.PREMIUM_CONTINUE_PASELI:
			case CONSUME_MODE.PREMIUM_CONTINUE_CREDIT:
				num = price * num3 / 1000;
				break;
			case CONSUME_MODE.GACHA_CREDIT:
			case CONSUME_MODE.GACHA_PASELI:
				num = 100 * num2 / 1000;
				break;
			case CONSUME_MODE.GACHA5_CREDIT:
			case CONSUME_MODE.GACHA5_PASELI:
				num = 500 * num2 / 1000;
				break;
			case CONSUME_MODE.JANDRI_PASELI:
				num = price;
				break;
			case CONSUME_MODE.MUSIC_GACHA_PASELI:
				num = 300 * num4 / 1000;
				break;
			case CONSUME_MODE.NONE:
			case CONSUME_MODE.GACHA_TICKET_EX:
			case CONSUME_MODE.GACHA5_TICKET_EX:
			case CONSUME_MODE.CONTINUE_FREE:
			case CONSUME_MODE.MAX:
				num = 0;
				break;
			}
			XmlNode xmlNode = XrpcUtil.CreateRequestNode("vfgac", "send_paylog");
			XmlDocument ownerDocument = xmlNode.OwnerDocument;
			xmlNode.Attributes.Append(ownerDocument.CreateAttribute("ref_id")).InnerText = GameDataManager.Account.RefID;
			xmlNode.Attributes.Append(ownerDocument.CreateAttribute("mid")).InnerText = GameDataManager.Account.PlayerID.ToString();
			xmlNode.AppendChild(ownerDocument.CreateElement("loc_id")).InnerText = SystemUtility.Instance.LocationID;
			XmlNode xmlNode2 = xmlNode.AppendChild(ownerDocument.CreateElement("data"));
			xmlNode2.AppendChild(ownerDocument.CreateElement("label")).InnerText = mode.ToString();
			XmlNode xmlNode3 = xmlNode2.AppendChild(ownerDocument.CreateElement("pp_amount"));
			xmlNode3.InnerText = num.ToString();
			xmlNode3.Attributes.Append(ownerDocument.CreateAttribute("__type")).InnerText = "u32";
			Debug.Log("send_paylog xml : " + ownerDocument.DocumentElement.OuterXml);
			XrpcMethodGetConsumeLog xrpcMethodGetConsumeLog = new XrpcMethodGetConsumeLog();
			xrpcMethodGetConsumeLog.SetCallback(callback, cb_param);
			return xrpcMethodGetConsumeLog.ExecRequest(xmlNode, null);
		}

		protected override TcsXrpcModuleTypes.GetConsumeLogResponse ParseHandler(XrpcBase xrpc, object param)
		{
			TcsXrpcModuleTypes.GetConsumeLogResponse getConsumeLogResponse = new TcsXrpcModuleTypes.GetConsumeLogResponse();
			_ = xrpc.ResponseXmlText;
			XElement root = XDocument.Parse(xrpc.ResponseXmlText).Root;
			Debug.Log("GetConsumeLogResponse : " + root.ToString());
			getConsumeLogResponse.Parse(root);
			return getConsumeLogResponse;
		}
	}

	public class XrpcMethodGetExtCampaign : XrpcMethodBase<TcsXrpcModuleTypes.GetExtCampaignResponse>
	{
		public static XrpcBase Request(Action<XrpcBase, TcsXrpcModuleTypes.GetExtCampaignResponse, object> callback, object cb_param)
		{
			XmlNode xmlNode = XrpcUtil.CreateRequestNode("vfgac", "ext_campaign");
			XmlDocument ownerDocument = xmlNode.OwnerDocument;
			xmlNode.Attributes.Append(ownerDocument.CreateAttribute("ref_id")).InnerText = GameDataManager.Account.RefID;
			xmlNode.Attributes.Append(ownerDocument.CreateAttribute("loc_id")).InnerText = SystemUtility.Instance.LocationID;
			XrpcMethodGetExtCampaign xrpcMethodGetExtCampaign = new XrpcMethodGetExtCampaign();
			xrpcMethodGetExtCampaign.SetCallback(callback, cb_param);
			return xrpcMethodGetExtCampaign.ExecRequest(xmlNode, null);
		}

		protected override TcsXrpcModuleTypes.GetExtCampaignResponse ParseHandler(XrpcBase xrpc, object param)
		{
			TcsXrpcModuleTypes.GetExtCampaignResponse getExtCampaignResponse = new TcsXrpcModuleTypes.GetExtCampaignResponse();
			_ = xrpc.ResponseXmlText;
			XElement root = XDocument.Parse(xrpc.ResponseXmlText).Root;
			Debug.Log("GetExtCampaignResponse : " + root.ToString());
			getExtCampaignResponse.Parse(root);
			return getExtCampaignResponse;
		}
	}

	public class XrpcMethodPutMsg : XrpcMethodBase<TcsXrpcModuleTypes.PutMsgResponse>
	{
		public static XrpcBase Request(List<(string key, string value)> msgs, Action<XrpcBase, TcsXrpcModuleTypes.PutMsgResponse, object> callback, object cb_param)
		{
			XmlNode xmlNode = XrpcUtil.CreateRequestNode("vfglog", "put_msg");
			XmlDocument ownerDocument = xmlNode.OwnerDocument;
			xmlNode.AppendChild(XmlPropUtil.CreateNodeString(ownerDocument, "loc_id", GetLocationId()));
			foreach (var msg in msgs)
			{
				XmlNode xmlNode2 = ownerDocument.CreateElement("msg");
				xmlNode2.Attributes.Append(ownerDocument.CreateAttribute("__type")).InnerText = "str";
				xmlNode2.Attributes.Append(ownerDocument.CreateAttribute("label")).InnerText = msg.key;
				xmlNode2.InnerText = msg.value;
				xmlNode.AppendChild(xmlNode2);
			}
			XrpcMethodPutMsg xrpcMethodPutMsg = new XrpcMethodPutMsg();
			xrpcMethodPutMsg.SetCallback(callback, cb_param);
			return xrpcMethodPutMsg.ExecRequest(xmlNode, null);
		}

		protected override TcsXrpcModuleTypes.PutMsgResponse ParseHandler(XrpcBase xrpc, object param)
		{
			TcsXrpcModuleTypes.PutMsgResponse putMsgResponse = new TcsXrpcModuleTypes.PutMsgResponse();
			_ = xrpc.ResponseXmlText;
			XElement root = XDocument.Parse(xrpc.ResponseXmlText).Root;
			putMsgResponse.Parse(root);
			return putMsgResponse;
		}
	}

	public class XrpcMethodGetServiceList : XrpcMethodBase<TcsXrpcModuleTypes.GetServiceListResponse>
	{
		public static XrpcBase Request(Action<XrpcBase, TcsXrpcModuleTypes.GetServiceListResponse, object> callback, object cb_param)
		{
			XmlNode xmlNode = XrpcUtil.CreateRequestNode("vfgac", "service_list");
			_ = xmlNode.OwnerDocument;
			XrpcMethodGetServiceList xrpcMethodGetServiceList = new XrpcMethodGetServiceList();
			xrpcMethodGetServiceList.SetCallback(callback, cb_param);
			return xrpcMethodGetServiceList.ExecRequest(xmlNode, null);
		}

		protected override TcsXrpcModuleTypes.GetServiceListResponse ParseHandler(XrpcBase xrpc, object param)
		{
			TcsXrpcModuleTypes.GetServiceListResponse getServiceListResponse = new TcsXrpcModuleTypes.GetServiceListResponse();
			_ = xrpc.ResponseXmlText;
			XElement root = XDocument.Parse(xrpc.ResponseXmlText).Root;
			getServiceListResponse.Parse(root);
			return getServiceListResponse;
		}
	}

	public class XrpcMethodUpdateRefer : XrpcMethodBase<TcsXrpcModuleTypes.UpdateReferResponse>
	{
		public static XrpcBase Request(Action<XrpcBase, TcsXrpcModuleTypes.UpdateReferResponse, object> callback, object cb_param)
		{
			XmlNode xmlNode = XrpcUtil.CreateRequestNode("vfgac", "update_refer");
			XmlDocument ownerDocument = xmlNode.OwnerDocument;
			xmlNode.Attributes.Append(ownerDocument.CreateAttribute("ref_id")).InnerText = GameDataManager.Account.RefID;
			xmlNode.Attributes.Append(ownerDocument.CreateAttribute("data_id")).InnerText = GameDataManager.Account.DataID;
			XrpcMethodUpdateRefer xrpcMethodUpdateRefer = new XrpcMethodUpdateRefer();
			xrpcMethodUpdateRefer.SetCallback(callback, cb_param);
			return xrpcMethodUpdateRefer.ExecRequest(xmlNode, null);
		}

		protected override TcsXrpcModuleTypes.UpdateReferResponse ParseHandler(XrpcBase xrpc, object param)
		{
			TcsXrpcModuleTypes.UpdateReferResponse updateReferResponse = new TcsXrpcModuleTypes.UpdateReferResponse();
			_ = xrpc.ResponseXmlText;
			XElement root = XDocument.Parse(xrpc.ResponseXmlText).Root;
			updateReferResponse.Parse(root);
			return updateReferResponse;
		}
	}

	private static bool s_Called;

	private static bool s_isDevelop;

	public static void AddModule()
	{
		AvsEa3Xrpc.Ea3XrpcAddModule("vfgac", "local", new string[4] { "service_list", "update_refer", "ext_campaign", "send_paylog" });
		AvsEa3Xrpc.Ea3XrpcAddModule("vfglog", "local", new string[2] { "put_msg", "error_log" });
	}

	public static string GetLocationId()
	{
		StringBuilder stringBuilder = new StringBuilder(32);
		AvsEa3.Ea3QueryFacilityPathString(AvsEa3.FACILITY_PATH_STRING.LOCATION_ID, stringBuilder, stringBuilder.Capacity);
		return stringBuilder.ToString();
	}

	private static bool IsDevelopEnv()
	{
		if (s_Called)
		{
			return s_isDevelop;
		}
		if (AvsEa3.Ea3QueryServicesProductDomain(out var product_domain) >= 0)
		{
			s_isDevelop = product_domain == 0;
			s_Called = true;
		}
		return s_isDevelop;
	}

	public static int CompareSoftIdCode(string a, string b)
	{
		return a.CompareTo(b);
	}

	public static int CompareSoftIdCode(string b)
	{
		return CompareSoftIdCode(Avs.GetSoftIdCode(), b);
	}
}
