using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using KAMUNITY;
using TestModeSkeleton.TestModeExtensions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TestModeSkeleton;

public class NetworkCheck : TestModeBase
{
	private enum Path
	{
		Router,
		Center,
		Server
	}

	private class ConnectionDisp
	{
		private readonly Text pathText;

		private readonly Text pointText;

		private readonly Text pingText;

		private readonly int keepalive;

		private readonly uint startSendCtr;

		private readonly uint startRecvCtr;

		public bool IsConnect { get; private set; }

		public ConnectionDisp(Text pathText, Text pointText, Text pingText, int keepalive)
		{
			this.pathText = pathText;
			this.pointText = pointText;
			this.pingText = pingText;
			this.keepalive = keepalive;
			AvsEa3.KeepaliveGetStatus(keepalive, out var status);
			startSendCtr = status.send_ctr;
			startRecvCtr = status.recv_ctr;
		}

		public void Update(string arrow)
		{
			if (keepalive > 0)
			{
				AvsEa3.KeepalivePing(keepalive);
				AvsEa3.KeepaliveGetStatus(keepalive, out var status);
				IsConnect = status.status != AvsTypes.AVS_KEEPALIVE_STATUS.DOWN;
				SetText(IsConnect, status.send_ctr - startSendCtr, status.recv_ctr - startRecvCtr, arrow);
			}
			else
			{
				IsConnect = false;
				SetText(IsConnect, 0u, 0u, arrow);
			}
		}

		private void SetText(bool isConnecting, uint pingSendCount, uint pingSuccessCount, string arrow)
		{
			if (isConnecting)
			{
				pathText.text = "<color=lime>" + arrow + "</color>";
				pointText.color = Color.green;
			}
			else
			{
				pathText.text = "<color=red>---</color>";
				pointText.color = Color.red;
			}
			float num = 1f * (float)pingSuccessCount / (float)((pingSendCount == 0) ? 1u : pingSendCount);
			if (float.IsNaN(num))
			{
				num = 0f;
			}
			pingText.text = $"{pingSuccessCount} / {pingSendCount} ({num:P0})";
		}
	}

	private class DomainName
	{
		private struct IP_ADDRESS_STRING
		{
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
			public string String;
		}

		private struct IP_MASK_STRING
		{
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
			public string String;
		}

		private struct IP_ADDR_STRING
		{
			public IntPtr Next;

			public IP_ADDRESS_STRING IpAddress;

			public IP_MASK_STRING IpMask;

			public uint Context;
		}

		private struct FIXED_INFO
		{
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 132)]
			public string HostName;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 132)]
			public string DomainName;

			public IntPtr CurrentDnsServer;

			public IP_ADDR_STRING DnsServerList;

			public uint NodeType;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
			public string ScopeId;

			public uint EnableRouting;

			public uint EnableProxy;

			public uint EnableDns;
		}

		private static readonly uint ERROR_SUCCESS;

		private const int MAX_HOSTNAME_LEN = 128;

		private const int MAX_DOMAIN_NAME_LEN = 128;

		private const int MAX_SCOPE_ID_LEN = 256;

		[DllImport("iphlpapi.dll", CharSet = CharSet.Ansi)]
		private static extern uint GetNetworkParams(IntPtr pFixedInfo, ref uint pBufOutLen);

		public static string Get()
		{
			string result = "-";
			uint pBufOutLen = 1024u;
			IntPtr intPtr = Marshal.AllocHGlobal((int)pBufOutLen);
			if (GetNetworkParams(intPtr, ref pBufOutLen) == ERROR_SUCCESS)
			{
				result = ((FIXED_INFO)Marshal.PtrToStructure(intPtr, typeof(FIXED_INFO))).DomainName;
			}
			Marshal.FreeHGlobal(intPtr);
			return result;
		}
	}

	[SerializeField]
	[Tooltip("接続パス")]
	private Text[] pathTexts;

	[SerializeField]
	[Tooltip("接続ポイント")]
	private Text[] pointTexts;

	[SerializeField]
	[Tooltip("ping状態")]
	private Text[] pingTexts;

	[SerializeField]
	[Tooltip("接続状態")]
	private Text statusText;

	[SerializeField]
	[Tooltip("IP ADDRESS")]
	private Text ipAddressText;

	[SerializeField]
	[Tooltip("SUBNET MASK")]
	private Text subNetMaskText;

	[SerializeField]
	[Tooltip("DEFAULT ROOTER")]
	private Text defaultRouterText;

	[SerializeField]
	[Tooltip("DOMAIN NAME")]
	private Text domainNameText;

	[SerializeField]
	[Tooltip("DNS SERVER")]
	private Text dnsServerText;

	[SerializeField]
	[Tooltip("DHCP SERVER")]
	private Text dhcpServerText;

	[SerializeField]
	[Tooltip("NTP SERVER")]
	private Text ntpServerText;

	[SerializeField]
	[Tooltip("MAC ADDRESS")]
	private Text macAddressText;

	[SerializeField]
	[Tooltip("パーティシペーションの状態")]
	private Text ppStatusText;

	protected override string TitleName => "<l>NETWORK CHECK</l>";

	private IEnumerator Start()
	{
		help.Set(TestModeHelp.HelpType.Exit);
		ArrowkeyGameObject.SetActive(value: true);
		ArrowkeyEnter.SetActive(value: true);
		ArrowkeyUp.SetActive(value: false);
		ArrowkeyDown.SetActive(value: false);
		ArrowkeyLeft.SetActive(value: false);
		ArrowkeyRight.SetActive(value: false);
		SingletonBehaviour<TestModeInput>.Instance.ResetArrowKey();
		StartCoroutine(CoCheck());
		yield return new WaitUntil(() => SingletonBehaviour<TestModeInput>.Instance.IsMenuExecute());
		Exit();
	}

	private IEnumerator CoCheck()
	{
		string arrow = "";
		StartCoroutine(CoSetArrow(0.33f, delegate(string s)
		{
			arrow = s;
		}));
		List<ConnectionDisp> states = new List<ConnectionDisp>
		{
			new ConnectionDisp(pathTexts[0], pointTexts[0], pingTexts[0], AvsEa3.KeepaliveFindByName("router")),
			new ConnectionDisp(pathTexts[1], pointTexts[1], pingTexts[1], AvsEa3.KeepaliveFindByName("center")),
			new ConnectionDisp(pathTexts[2], pointTexts[2], pingTexts[2], AvsEa3.KeepaliveFindByName("server"))
		};
		while (true)
		{
			states.ForEach(delegate(ConnectionDisp s)
			{
				s.Update(arrow);
			});
			statusText.SetLocalizedText(states[2].IsConnect ? "<color=lime><l>CONNECTED</l></color>" : "<color=red><l>NOT CONNECTED</l></color>");
			AvsEa3.NetCtrlFlush();
			ipAddressText.text = ToString(AvsEa3.NetCtrlAddress());
			subNetMaskText.text = ToString(AvsEa3.NetCtrlNetmask());
			defaultRouterText.text = ToString(AvsEa3.NetCtrlGateway());
			domainNameText.text = DomainName.Get();
			dnsServerText.text = ToString(AvsEa3.NetCtrlDns0()) + "\n" + ToString(AvsEa3.NetCtrlDns1());
			dhcpServerText.text = ToString(AvsEa3.NetCtrlDhcp());
			ntpServerText.text = ToString(GetNtpServer());
			macAddressText.text = ToString(AvsEa3.NetCtrlMac());
			switch (EagCore.Ea3GetPpStatus())
			{
			case AvsTypes.EA3_PP_STATUS.DISABLE:
				ppStatusText.SetLocalizedText("<color=red><l>NO/DISABLE</l></color>");
				break;
			case AvsTypes.EA3_PP_STATUS.NOT_AVAILABLE:
				ppStatusText.SetLocalizedText("<color=red><l>NO/NOT AVAILABLE</l></color>");
				break;
			case AvsTypes.EA3_PP_STATUS.AVAILABLE:
				ppStatusText.SetLocalizedText("<color=lime><l>NO/AVAILABLE</l></color>");
				break;
			default:
				ppStatusText.SetLocalizedText("<color=red>-</color>");
				break;
			}
			yield return null;
		}
	}

	private IEnumerator CoSetArrow(float seconds, UnityAction<string> action)
	{
		string[] arrows = new string[3] { ">--", "->-", "-->" };
		int phase = 0;
		while (true)
		{
			action(arrows[phase]);
			phase = (int)Mathf.Repeat(phase + 1, arrows.Length);
			yield return new WaitForSeconds(seconds);
		}
	}

	private AvsTypes.avs_net_in4_addr GetNtpServer()
	{
		Avs.SntpGetAddr(out var addr);
		return addr;
	}

	private string ToString(AvsTypes.avs_net_in4_addr addr)
	{
		if (!addr.addr8.All((byte v) => v == 0))
		{
			return string.Join(".", addr.addr8);
		}
		return "-";
	}

	private string ToString(AvsTypes.avs_net_in4_addr[] addrs)
	{
		if (addrs.Length != 0)
		{
			return ToString(addrs[0]);
		}
		return "-";
	}

	private string ToString(AvsTypes.avs_net_mac_addr[] addrs)
	{
		if (addrs.Length == 0)
		{
			return "--:--:--:--:--:--";
		}
		byte[] mac_addr = addrs[0].mac_addr;
		return string.Join(":", mac_addr.Select((byte b) => $"{b:X2}"));
	}
}
