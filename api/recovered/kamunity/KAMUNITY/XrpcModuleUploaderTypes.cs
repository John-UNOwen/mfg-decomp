using System;
using System.Xml;

namespace KAMUNITY;

public class XrpcModuleUploaderTypes
{
	public enum ServiceCategoryId
	{
		Machine,
		Center,
		e_amusementSite
	}

	public class DeclareUploadRequest_Content
	{
		public string relativePath;

		public string contentKey;

		public string contentType;

		public int contentSize;

		public string contentHash;
	}

	public class DeclareUploadResponse
	{
		public string arrangeNum;

		public string uploadUrl;

		public int urlValidSec;

		public string accessKey;

		public int bandWidth;

		public DateTime expireDate;

		public void Parse(XmlNode node)
		{
			arrangeNum = XmlPropUtil.ReadNodeStringWithDefault(node, "arrangeNum", "");
			uploadUrl = XmlPropUtil.ReadNodeStringWithDefault(node, "uploadUrl", "");
			urlValidSec = XmlPropUtil.ReadNodeS32WithDefault(node, "urlValidSec", -1);
			accessKey = XmlPropUtil.ReadNodeStringWithDefault(node, "accessKey", "");
			bandWidth = XmlPropUtil.ReadNodeS32WithDefault(node, "bandWidth", -1);
			expireDate = XmlPropUtil.ReadNodeDateWithDefault(node, "expireDate", DateTime.MinValue);
		}
	}

	public class FindFilesResponse_Content
	{
		public string arrangeNum;

		public string relativePath;

		public string contentKey;

		public string contentType;

		public int contentSize;

		public string contentHash;

		public string targetCategory;

		public string targetModel;

		public string bindedUser;

		public DateTime expireDate;

		public string downloadUrl;

		public int urlValidSec;

		public void Parse(XmlNode node)
		{
			arrangeNum = XmlPropUtil.ReadNodeStringWithDefault(node, "arrangeNum", "");
			relativePath = XmlPropUtil.ReadNodeStringWithDefault(node, "relativePath", "");
			contentKey = XmlPropUtil.ReadNodeStringWithDefault(node, "contentKey", "");
			contentType = XmlPropUtil.ReadNodeStringWithDefault(node, "contentType", "");
			contentSize = XmlPropUtil.ReadNodeS32WithDefault(node, "contentSize", 0);
			contentHash = XmlPropUtil.ReadNodeStringWithDefault(node, "contentHash", "");
			targetCategory = XmlPropUtil.ReadNodeStringWithDefault(node, "targetCategory", "");
			targetModel = XmlPropUtil.ReadNodeStringWithDefault(node, "targetModel", "");
			bindedUser = XmlPropUtil.ReadNodeStringWithDefault(node, "bindedUser", "");
			expireDate = XmlPropUtil.ReadNodeDateWithDefault(node, "expireDate", DateTime.MinValue);
			downloadUrl = XmlPropUtil.ReadNodeStringWithDefault(node, "downloadUrl", "");
			urlValidSec = XmlPropUtil.ReadNodeS32WithDefault(node, "urlValidSec", 0);
		}
	}

	public static string ServiceCategoryIdString(ServiceCategoryId id)
	{
		return id switch
		{
			ServiceCategoryId.Machine => "1", 
			ServiceCategoryId.Center => "2", 
			ServiceCategoryId.e_amusementSite => "G", 
			_ => "", 
		};
	}
}
