using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;

namespace KAMUNITY;

public class XrpcBase
{
	private string m_RequestName;

	private string m_RequestXmlText;

	private int m_RequestSize;

	private IntPtr m_RequestHandle = IntPtr.Zero;

	private AvsEa3XrpcTypes.XRPC_STATUS_DATA m_Status;

	private string m_ResponseXmlText;

	private XmlDocument m_ResponseXmlDocument;

	private XmlNode m_ResponseXmlNode;

	public string ModuleName { get; private set; }

	public string MethodName { get; private set; }

	public string RequestXmlText => m_RequestXmlText;

	public string ResponseXmlText => m_ResponseXmlText;

	public AvsEa3XrpcTypes.XRPC_STATUS_DATA Status
	{
		get
		{
			return m_Status;
		}
		set
		{
			m_Status = value;
		}
	}

	public XmlDocument GetResponseXmlDocment()
	{
		return m_ResponseXmlDocument;
	}

	public XmlNode GetResponseXmlNode()
	{
		return m_ResponseXmlNode;
	}

	public void SetRequest(string module_name, string method_name, string xml, int size)
	{
		ModuleName = module_name;
		MethodName = method_name;
		m_RequestName = module_name + "." + method_name;
		m_RequestXmlText = xml;
		m_RequestSize = size;
	}

	public void StartRequest()
	{
		Logger.LogFormat("<color=orange>xrpc_request</color> : {0}\n{1}", m_RequestName, m_RequestXmlText);
		m_Status.Clear();
		if (ModuleName.CompareTo("package") != 0 && AvsEa3.Ea3GetStatus(out var _, out var _) != AvsTypes.EA3_STATUS.ONLINE)
		{
			m_Status.status = -100;
			Logger.LogWarningFormat("<color=orange>xrpc_status(error)</color>:{0}", m_Status.ToString());
			return;
		}
		Logger.LogFormat("<color=orange>Ea3XrpcApply before</color>");
		m_RequestHandle = AvsEa3Xrpc.Ea3XrpcApply(m_RequestName, m_RequestXmlText, m_RequestSize);
		Logger.LogFormat("<color=orange>Ea3XrpcApply after</color>");
		if (m_RequestHandle == IntPtr.Zero)
		{
			m_Status.status = -99;
			Logger.LogWarningFormat("<color=orange>xrpc_status(error)</color>:{0}", m_Status.ToString());
		}
	}

	public bool UpdateRequest()
	{
		int num = 1;
		if (m_RequestHandle != IntPtr.Zero)
		{
			num = AvsEa3Xrpc.Ea3XrpcIsFinish(m_RequestHandle);
			if (num != 0)
			{
				AvsEa3Xrpc.Ea3XrpcGetStatus(m_RequestHandle, out m_Status);
				if (m_Status.HasError())
				{
					m_ResponseXmlText = "";
					m_ResponseXmlDocument = null;
					m_ResponseXmlNode = null;
				}
				else
				{
					IntPtr intPtr = AvsEa3Xrpc.Ea3XrpcGetResponseData(m_RequestHandle);
					if (intPtr != IntPtr.Zero)
					{
						int num2 = AvsEa3Xrpc.Ea3XrpcGetResponseSize(m_RequestHandle);
						byte[] array = new byte[num2];
						Marshal.Copy(intPtr, array, 0, num2);
						m_ResponseXmlText = Encoding.UTF8.GetString(array).TrimEnd('\0');
						m_ResponseXmlDocument = new XmlDocument();
						m_ResponseXmlDocument.LoadXml(m_ResponseXmlText);
						m_ResponseXmlNode = m_ResponseXmlDocument.SelectSingleNode(ModuleName);
					}
				}
				Logger.LogFormat("<color=orange>xrpc_response</color> : {0}\nstatus:{1}\n{2}", m_RequestName, m_Status.ToString(), m_ResponseXmlText);
				AvsEa3Xrpc.Ea3XrpcDestroy(m_RequestHandle);
				m_RequestHandle = IntPtr.Zero;
			}
		}
		return num == 0;
	}
}
