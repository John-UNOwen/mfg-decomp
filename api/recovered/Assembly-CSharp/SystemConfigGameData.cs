using System;
using Konami.System.Net;
using MFG.GameData;
using MFG.Types;
using UnityEngine;

[Serializable]
public class SystemConfigGameData : GameDataBase
{
	private const int CurrentDataVersion = 1;

	public int DataVersion = 1;

	public GameControlInfo GameControlInfo = new GameControlInfo();

	private static string m_frontServerUrl = "";

	public string DefaultGameServerUrlFront = "https://stg-aog";

	public string DefaultGameServerUrlTail = "s.konaminet.jp/aog";

	public string MobileUrl;

	public string AppVersion = "2.0.2";

	public int AreaID = 1;

	public int WebID = 1;

	public static CabinetType CabinetType = CabinetType.None;

	public static AppBehaviorType AppBehaviorType = AppBehaviorType.AC;

	public static string FrontServerUrl
	{
		get
		{
			if (GameUtility.IsKamunityBootEnable())
			{
				return m_frontServerUrl;
			}
			return NetEnvironment.Instance.Connection.RequestUrl;
		}
		set
		{
			m_frontServerUrl = value;
		}
	}

	public static bool IsCloudStreaming
	{
		get
		{
			// Rebuild gate (docs/21 10): Konami's Konasta/Netcafe builds run as thin clients
			// hosted by cloudstrap.exe with Kamunity/eAC3 active. Standalone play with
			// -KONAMINET has no cloud host, so report plain local (non-cloud) mode there.
			if (!GameUtility.IsKamunityBootEnable())
			{
				return false;
			}
			if (AppBehaviorType != AppBehaviorType.Konasta)
			{
				return AppBehaviorType == AppBehaviorType.Netcafe;
			}
			return true;
		}
	}

	public static bool IsDualMonitor
	{
		get
		{
			if (CabinetType == CabinetType.UKS || CabinetType == CabinetType.TBS || CabinetType == CabinetType.UJK)
			{
				return Display.displays.Length > 1;
			}
			return false;
		}
	}

	public static bool IsSXGAMonitor
	{
		get
		{
			if (CabinetType == CabinetType.TBS || CabinetType == CabinetType.UJK)
			{
				return Display.displays.Length > 1;
			}
			return false;
		}
	}
}
