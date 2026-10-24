using System;
using UnityEngine;

namespace Konami.System.Net;

[Serializable]
public class NetEnvironmentProxy
{
	[SerializeField]
	private bool useProxy;

	[SerializeField]
	private string proxyHost = "";

	[SerializeField]
	private int proxyPort;

	public static readonly NetEnvironmentProxy Default = new NetEnvironmentProxy
	{
		useProxy = false,
		proxyHost = "",
		proxyPort = 0
	};

	public static readonly NetEnvironmentProxy DefaultKonamiNet = new NetEnvironmentProxy
	{
		useProxy = true,
		proxyHost = "proxy-jp100.konami",
		proxyPort = 8080
	};

	public static readonly NetEnvironmentProxy DefaultMobileNet = new NetEnvironmentProxy
	{
		useProxy = false,
		proxyHost = "",
		proxyPort = 0
	};

	public bool UseProxy
	{
		get
		{
			return useProxy;
		}
		set
		{
			useProxy = value;
		}
	}

	public string ProxyHost
	{
		get
		{
			if (!useProxy)
			{
				return "";
			}
			return proxyHost;
		}
	}

	public int ProxyPort
	{
		get
		{
			if (!useProxy)
			{
				return 0;
			}
			return proxyPort;
		}
	}
}
