using System;
using System.Collections.Generic;

namespace Konami.System.Net;

[Serializable]
public class NetEnvironmentSettings : SerializableFile
{
	public bool useDevelopSetting = true;

	public int currentSettingIdx;

	public List<NetEnvironmentConnection> kinds = new List<NetEnvironmentConnection>();

	public NetEnvironmentConnection CurrentConnection
	{
		get
		{
			if (useDevelopSetting && 0 < kinds.Count && currentSettingIdx < kinds.Count)
			{
				return kinds[currentSettingIdx];
			}
			return NetEnvironmentConnection.Default;
		}
	}

	public NetEnvironmentSettings()
		: base(isCrypt: false, isCompress: false)
	{
	}
}
