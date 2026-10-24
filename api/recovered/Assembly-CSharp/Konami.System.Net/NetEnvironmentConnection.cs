using System;

namespace Konami.System.Net;

[Serializable]
public class NetEnvironmentConnection
{
	public string Name = "";

	public float NetworkTimeout = 30f;

	public int NetworkRetry = 3;

	public float NetworkRetryWait = 1f;

	public string RequestUrl = "http://localhost/";

	public NetEnvironmentProxy Proxy = new NetEnvironmentProxy();

	public static readonly NetEnvironmentConnection Default = new NetEnvironmentConnection
	{
		Name = "",
		NetworkTimeout = 30f,
		NetworkRetry = 3,
		NetworkRetryWait = 1f,
		RequestUrl = "https://rproxy0400.ea.konami.net/aog",
		Proxy = NetEnvironmentProxy.Default
	};

	public static readonly NetEnvironmentConnection DefaultEamDev = new NetEnvironmentConnection
	{
		Name = "",
		NetworkTimeout = 30f,
		NetworkRetry = 3,
		NetworkRetryWait = 1f,
		RequestUrl = "http://rproxy0400.pvt.ea.konami.net/aog",
		Proxy = NetEnvironmentProxy.Default
	};

	public static readonly NetEnvironmentConnection DefaultKonamiNet = new NetEnvironmentConnection
	{
		Name = "",
		NetworkTimeout = 30f,
		NetworkRetry = 3,
		NetworkRetryWait = 1f,
		RequestUrl = "https://aog03-stg.pvt.mo.konami.net/aog",
		Proxy = NetEnvironmentProxy.DefaultKonamiNet
	};
}
