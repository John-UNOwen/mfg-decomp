using System.Xml;

namespace KAMUNITY;

public abstract class XrpcMethodInternal
{
	public XrpcBase Xrpc { get; protected set; }

	protected XrpcBase ExecRequest(XmlNode nodeRoot, object param, int xrpc_size = 1048576)
	{
		Xrpc = XrpcUtil.ExecRequest(nodeRoot, ResponseHandler, param, xrpc_size);
		return Xrpc;
	}

	protected abstract void ResponseHandler(XrpcBase xrpc, object cb_param);
}
