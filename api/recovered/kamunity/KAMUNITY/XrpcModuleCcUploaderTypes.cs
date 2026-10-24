using System;
using System.Xml;

namespace KAMUNITY;

public class XrpcModuleCcUploaderTypes
{
	public enum PayloadType
	{
		PyloadType_Default,
		PayloadType_CardConnect
	}

	public enum ServiceCategoryId
	{
		Machine,
		ToyosuCenter,
		GcpCenter,
		e_amusementSite
	}

	public class FindFilesResponse_Content : XrpcModuleUploaderTypes.FindFilesResponse_Content
	{
		public string payloadType;

		public int uploadStatus;

		public DateTime reserveTime;

		public string postKey;

		public string retentionKey;

		public new void Parse(XmlNode node)
		{
			base.Parse(node);
			payloadType = XmlPropUtil.ReadNodeStringWithDefault(node, "payloadType", "");
			uploadStatus = XmlPropUtil.ReadNodeS32WithDefault(node, "uploadStatus", 0);
			reserveTime = XmlPropUtil.ReadNodeUnixTimeWithDefault(node, "reserveTime", DateTime.MinValue);
			postKey = XmlPropUtil.ReadNodeStringWithDefault(node, "postKey", "");
			retentionKey = XmlPropUtil.ReadNodeStringWithDefault(node, "retentionKey", "");
		}
	}

	public static string PayloadTypeString(PayloadType type)
	{
		return type switch
		{
			PayloadType.PyloadType_Default => "default", 
			PayloadType.PayloadType_CardConnect => "cc", 
			_ => "", 
		};
	}

	public static string ServiceCategoryIdString(ServiceCategoryId id)
	{
		return id switch
		{
			ServiceCategoryId.Machine => "1", 
			ServiceCategoryId.ToyosuCenter => "2", 
			ServiceCategoryId.GcpCenter => "3", 
			ServiceCategoryId.e_amusementSite => "G", 
			_ => "", 
		};
	}
}
