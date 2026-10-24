using System.Collections.Generic;
using System.Xml.Linq;
using MFG.Sytem.Utilities.Xml;

namespace TCSNET;

public class TcsXrpcModuleTypes
{
	public class GetServiceListResponse
	{
		public class ServiceInfo
		{
			private string Name { get; set; }

			private string Url { get; set; }

			private string Mode { get; set; }

			public void Parse(XElement elem)
			{
				Name = elem.Attribute("service").Value;
				Mode = elem.Attribute("mode").Value;
				Url = elem.Value;
			}
		}

		public string ServiceUrl = "";

		public List<ServiceInfo> ServiceInfos = new List<ServiceInfo>();

		public void Parse(XElement serviceListElem)
		{
			ServiceUrl = serviceListElem.Element("service_url").Value;
			foreach (XElement item in serviceListElem.Element("services").Elements())
			{
				ServiceInfo serviceInfo = new ServiceInfo();
				serviceInfo.Parse(item);
				ServiceInfos.Add(serviceInfo);
			}
		}
	}

	public class UpdateReferResponse
	{
		private int status;

		public void Parse(XElement updateReferElem)
		{
			status = updateReferElem.AttributeValueInt("status");
		}
	}

	public class PutMsgResponse
	{
		private int status;

		public void Parse(XElement putMsgElem)
		{
			status = putMsgElem.AttributeValueInt("status");
		}
	}

	public class GetExtCampaignResponse
	{
		public class ExtCampaignInfo
		{
			public int EventID { get; set; }

			public int EventValue { get; set; }

			public void Parse(XElement elem)
			{
				EventID = int.Parse(elem.Attribute("event_id").Value);
				EventValue = elem.Element("value").ValueInt();
			}
		}

		public List<ExtCampaignInfo> ExtCampaignInfos = new List<ExtCampaignInfo>();

		public void Parse(XElement elem)
		{
			IEnumerable<XElement> enumerable = elem.Elements("d");
			if (enumerable == null)
			{
				return;
			}
			foreach (XElement item in enumerable)
			{
				ExtCampaignInfo extCampaignInfo = new ExtCampaignInfo();
				extCampaignInfo.Parse(item);
				ExtCampaignInfos.Add(extCampaignInfo);
			}
		}
	}

	public class GetConsumeLogResponse
	{
		private int status;

		public void Parse(XElement putMsgElem)
		{
			status = putMsgElem.AttributeValueInt("status");
		}
	}
}
