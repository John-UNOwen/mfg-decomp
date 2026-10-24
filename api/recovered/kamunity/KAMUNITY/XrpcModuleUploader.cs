using System;
using System.Collections.Generic;
using System.Xml;

namespace KAMUNITY;

public static class XrpcModuleUploader
{
	public class XrpcMethodCommitUpload : XrpcMethodBase
	{
		public static XrpcBase Request(string arrangeNum, Action<XrpcBase, object> callback, object cb_param)
		{
			XmlNode xmlNode = XrpcUtil.CreateRequestNode("uploader", "commitUpload");
			XmlDocument ownerDocument = xmlNode.OwnerDocument;
			xmlNode.AppendChild(XmlPropUtil.CreateNodeString(ownerDocument, "arrangeNum", arrangeNum));
			XrpcMethodCommitUpload xrpcMethodCommitUpload = new XrpcMethodCommitUpload();
			xrpcMethodCommitUpload.SetCallback(callback, cb_param);
			return xrpcMethodCommitUpload.ExecRequest(xmlNode, null);
		}

		protected override void ParseHandler(XrpcBase xrpc, object param)
		{
		}
	}

	public class XrpcMethodDeclareDiscard : XrpcMethodBase
	{
		public static XrpcBase Request(string arrangeNum, Action<XrpcBase, object> callback, object cb_param)
		{
			XmlNode xmlNode = XrpcUtil.CreateRequestNode("uploader", "declareDiscard");
			XmlDocument ownerDocument = xmlNode.OwnerDocument;
			xmlNode.AppendChild(XmlPropUtil.CreateNodeString(ownerDocument, "arrangeNum", arrangeNum));
			XrpcMethodDeclareDiscard xrpcMethodDeclareDiscard = new XrpcMethodDeclareDiscard();
			xrpcMethodDeclareDiscard.SetCallback(callback, cb_param);
			return xrpcMethodDeclareDiscard.ExecRequest(xmlNode, null);
		}

		protected override void ParseHandler(XrpcBase xrpc, object param)
		{
		}
	}

	public class XrpcMethodDeclareUpload : XrpcMethodBase<XrpcModuleUploaderTypes.DeclareUploadResponse>
	{
		public static XrpcBase Request(XrpcModuleUploaderTypes.ServiceCategoryId category_id, string bindedUser, List<XrpcModuleUploaderTypes.DeclareUploadRequest_Content> contentList, Action<XrpcBase, XrpcModuleUploaderTypes.DeclareUploadResponse, object> callback, object cb_param)
		{
			XmlNode xmlNode = XrpcUtil.CreateRequestNode("uploader", "declareUpload");
			XmlDocument ownerDocument = xmlNode.OwnerDocument;
			xmlNode.AppendChild(XmlPropUtil.CreateNodeString(ownerDocument, "svcCategory", XrpcModuleUploaderTypes.ServiceCategoryIdString(category_id)));
			xmlNode.AppendChild(XmlPropUtil.CreateNodeString(ownerDocument, "payloadType", "default"));
			xmlNode.AppendChild(XmlPropUtil.CreateNodeString(ownerDocument, "bindedUser", bindedUser));
			XmlNode xmlNode2 = xmlNode.AppendChild(ownerDocument.CreateElement("contentList"));
			for (int i = 0; i < contentList.Count; i++)
			{
				XmlNode xmlNode3 = xmlNode2.AppendChild(ownerDocument.CreateElement("content"));
				XrpcModuleUploaderTypes.DeclareUploadRequest_Content declareUploadRequest_Content = contentList[i];
				xmlNode3.AppendChild(XmlPropUtil.CreateNodeString(ownerDocument, "relativePath", declareUploadRequest_Content.relativePath));
				xmlNode3.AppendChild(XmlPropUtil.CreateNodeString(ownerDocument, "contentKey", declareUploadRequest_Content.contentKey));
				xmlNode3.AppendChild(XmlPropUtil.CreateNodeString(ownerDocument, "contentType", declareUploadRequest_Content.contentType));
				xmlNode3.AppendChild(XmlPropUtil.CreateNodeS32(ownerDocument, "contentSize", declareUploadRequest_Content.contentSize));
				xmlNode3.AppendChild(XmlPropUtil.CreateNodeString(ownerDocument, "contentHash", declareUploadRequest_Content.contentHash));
			}
			XrpcMethodDeclareUpload xrpcMethodDeclareUpload = new XrpcMethodDeclareUpload();
			xrpcMethodDeclareUpload.SetCallback(callback, cb_param);
			return xrpcMethodDeclareUpload.ExecRequest(xmlNode, null);
		}

		protected override XrpcModuleUploaderTypes.DeclareUploadResponse ParseHandler(XrpcBase xrpc, object cb_param)
		{
			XmlNode responseXmlNode = xrpc.GetResponseXmlNode();
			XrpcModuleUploaderTypes.DeclareUploadResponse declareUploadResponse = new XrpcModuleUploaderTypes.DeclareUploadResponse();
			declareUploadResponse.Parse(responseXmlNode);
			return declareUploadResponse;
		}
	}

	public class XrpcMethodFindFiles : XrpcMethodBase<List<XrpcModuleUploaderTypes.FindFilesResponse_Content>>
	{
		public static XrpcBase Request(XrpcModuleUploaderTypes.ServiceCategoryId callerCategory, XrpcModuleUploaderTypes.ServiceCategoryId targetCategory, string bindedUser, string targetModel, string[] contentKeyList, Action<XrpcBase, List<XrpcModuleUploaderTypes.FindFilesResponse_Content>, object> callback, object cb_param)
		{
			XmlNode xmlNode = XrpcUtil.CreateRequestNode("uploader", "findFiles");
			XmlDocument ownerDocument = xmlNode.OwnerDocument;
			xmlNode.AppendChild(XmlPropUtil.CreateNodeString(ownerDocument, "callerCategory", XrpcModuleUploaderTypes.ServiceCategoryIdString(callerCategory)));
			xmlNode.AppendChild(XmlPropUtil.CreateNodeString(ownerDocument, "targetCategory", XrpcModuleUploaderTypes.ServiceCategoryIdString(targetCategory)));
			xmlNode.AppendChild(XmlPropUtil.CreateNodeString(ownerDocument, "bindedUser", bindedUser));
			if (targetModel != null)
			{
				xmlNode.AppendChild(XmlPropUtil.CreateNodeString(ownerDocument, "targetModel", targetModel));
			}
			if (contentKeyList != null)
			{
				XmlNode xmlNode2 = xmlNode.AppendChild(ownerDocument.CreateElement("contentKeyList"));
				for (int i = 0; i < contentKeyList.Length; i++)
				{
					xmlNode2.AppendChild(XmlPropUtil.CreateNodeString(ownerDocument, "contentKey", contentKeyList[i]));
				}
			}
			XrpcMethodFindFiles xrpcMethodFindFiles = new XrpcMethodFindFiles();
			xrpcMethodFindFiles.SetCallback(callback, cb_param);
			return xrpcMethodFindFiles.ExecRequest(xmlNode, null);
		}

		public static XrpcBase Request(XrpcModuleUploaderTypes.ServiceCategoryId callerCategory, string bindedUser, string targetModel, string[] contentKeyList, Action<XrpcBase, List<XrpcModuleUploaderTypes.FindFilesResponse_Content>, object> callback, object cb_param)
		{
			XmlNode xmlNode = XrpcUtil.CreateRequestNode("uploader", "findFiles");
			XmlDocument ownerDocument = xmlNode.OwnerDocument;
			xmlNode.AppendChild(XmlPropUtil.CreateNodeString(ownerDocument, "callerCategory", XrpcModuleUploaderTypes.ServiceCategoryIdString(callerCategory)));
			xmlNode.AppendChild(XmlPropUtil.CreateNodeString(ownerDocument, "bindedUser", bindedUser));
			if (targetModel != null)
			{
				xmlNode.AppendChild(XmlPropUtil.CreateNodeString(ownerDocument, "targetModel", targetModel));
			}
			if (contentKeyList != null)
			{
				XmlNode xmlNode2 = xmlNode.AppendChild(ownerDocument.CreateElement("contentKeyList"));
				for (int i = 0; i < contentKeyList.Length; i++)
				{
					xmlNode2.AppendChild(XmlPropUtil.CreateNodeString(ownerDocument, "contentKey", contentKeyList[i]));
				}
			}
			XrpcMethodFindFiles xrpcMethodFindFiles = new XrpcMethodFindFiles();
			xrpcMethodFindFiles.SetCallback(callback, cb_param);
			return xrpcMethodFindFiles.ExecRequest(xmlNode, null);
		}

		public static XrpcBase Request(XrpcModuleUploaderTypes.ServiceCategoryId callerCategory, XrpcModuleUploaderTypes.ServiceCategoryId targetCategory, string targetModel, string[] contentKeyList, Action<XrpcBase, List<XrpcModuleUploaderTypes.FindFilesResponse_Content>, object> callback, object cb_param)
		{
			XmlNode xmlNode = XrpcUtil.CreateRequestNode("uploader", "findFiles");
			XmlDocument ownerDocument = xmlNode.OwnerDocument;
			xmlNode.AppendChild(XmlPropUtil.CreateNodeString(ownerDocument, "callerCategory", XrpcModuleUploaderTypes.ServiceCategoryIdString(callerCategory)));
			xmlNode.AppendChild(XmlPropUtil.CreateNodeString(ownerDocument, "targetCategory", XrpcModuleUploaderTypes.ServiceCategoryIdString(targetCategory)));
			if (targetModel != null)
			{
				xmlNode.AppendChild(XmlPropUtil.CreateNodeString(ownerDocument, "targetModel", targetModel));
			}
			if (contentKeyList != null)
			{
				XmlNode xmlNode2 = xmlNode.AppendChild(ownerDocument.CreateElement("contentKeyList"));
				for (int i = 0; i < contentKeyList.Length; i++)
				{
					xmlNode2.AppendChild(XmlPropUtil.CreateNodeString(ownerDocument, "contentKey", contentKeyList[i]));
				}
			}
			XrpcMethodFindFiles xrpcMethodFindFiles = new XrpcMethodFindFiles();
			xrpcMethodFindFiles.SetCallback(callback, cb_param);
			return xrpcMethodFindFiles.ExecRequest(xmlNode, null);
		}

		public static XrpcBase Request(XrpcModuleUploaderTypes.ServiceCategoryId callerCategory, string arrangeNum, Action<XrpcBase, List<XrpcModuleUploaderTypes.FindFilesResponse_Content>, object> callback, object cb_param)
		{
			XmlNode xmlNode = XrpcUtil.CreateRequestNode("uploader", "findFiles");
			XmlDocument ownerDocument = xmlNode.OwnerDocument;
			xmlNode.AppendChild(XmlPropUtil.CreateNodeString(ownerDocument, "callerCategory", XrpcModuleUploaderTypes.ServiceCategoryIdString(callerCategory)));
			xmlNode.AppendChild(XmlPropUtil.CreateNodeString(ownerDocument, "arrangeNum", arrangeNum));
			XrpcMethodFindFiles xrpcMethodFindFiles = new XrpcMethodFindFiles();
			xrpcMethodFindFiles.SetCallback(callback, cb_param);
			return xrpcMethodFindFiles.ExecRequest(xmlNode, null);
		}

		protected override List<XrpcModuleUploaderTypes.FindFilesResponse_Content> ParseHandler(XrpcBase xrpc, object cb_param)
		{
			List<XrpcModuleUploaderTypes.FindFilesResponse_Content> list = new List<XrpcModuleUploaderTypes.FindFilesResponse_Content>();
			XmlNodeList xmlNodeList = xrpc.GetResponseXmlNode().SelectNodes("content");
			for (int i = 0; i < xmlNodeList.Count; i++)
			{
				XmlNode node = xmlNodeList[i];
				XrpcModuleUploaderTypes.FindFilesResponse_Content findFilesResponse_Content = new XrpcModuleUploaderTypes.FindFilesResponse_Content();
				findFilesResponse_Content.Parse(node);
				findFilesResponse_Content.downloadUrl = findFilesResponse_Content.downloadUrl.Replace("#38;", "");
				list.Add(findFilesResponse_Content);
			}
			return list;
		}
	}

	public static readonly string module_name = "uploader";

	private const int XRPC_BUFFER_SIZE_FIND_FILES = 1048576;

	private static bool s_Called = false;

	private static bool s_isDevelop = false;

	public static void AddModule()
	{
		AvsEa3Xrpc.Ea3XrpcAddModule(module_name, "uploader", new string[4] { "declareUpload", "commitUpload", "findFiles", "declareDiscard" });
	}

	public static void RemoveModule()
	{
		AvsEa3Xrpc.Ea3XrpcRemoveModule(module_name);
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
}
