using System.Collections.Generic;
using System.Threading;
using System.Xml.Linq;
using Cysharp.Threading.Tasks;
using MFG.GameData;
using MFG.Sytem.Utilities.Xml;
using MFG.Types;

namespace MFG.GameRequest;

public class GetMenuData : RequestApiBase<GetMenuData, GetMenuData.ResultData>
{
	public class ResultData : ResultBase
	{
		public DanLevelType DanLevel;

		public PlayerGameData PlayerData = new PlayerGameData();

		public ReconnectData Reconnect = new ReconnectData();

		public List<GameModeInfo> GameModeInfos = new List<GameModeInfo>();

		public TaikyokuItemSetting TaikyokuItemSetting = new TaikyokuItemSetting();
	}

	public class ReconnectData
	{
		public bool IsActive;

		public int GameMode;

		public int TakuClass;

		public bool IsForceReconnect;

		public int ReconnectFrom;

		public string GameServiceUrl;

		public void Parse(XElement reconnectElem)
		{
			try
			{
				IsActive = true;
				GameMode = reconnectElem.NeedNotElement("gmode").ValueInt();
			}
			catch (NeedNotNodeNotFoundException)
			{
			}
		}
	}

	protected override void OnCreateSendParameter()
	{
		m_sendUrl = base.DefaultFrontServerUrl + "/get_menudata";
		m_postStr = $"web_id={base.WebID}&area_id={base.AreaID}&pcuid={base.AppSessionID}";
	}

	protected override async UniTask OnParse(XElement baseElem, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		await base.OnParse(baseElem, cancellationToken);
		try
		{
			XElement xElement = baseElem.MustNeedElement("menudata");
			XElement mpDataElem = xElement.NeedNotElement("mpdata");
			base.Result.PlayerData.Parse(mpDataElem);
			XElement xElement2 = baseElem.Element("recon");
			if (xElement2 != null)
			{
				base.Result.Reconnect.Parse(xElement2);
			}
			XElement xElement3 = xElement.Element("playmode_list");
			if (xElement3 != null)
			{
				foreach (XElement item in xElement3.Elements())
				{
					GameModeInfo gameModeInfo = new GameModeInfo();
					gameModeInfo.Parse(item);
					base.Result.GameModeInfos.Add(gameModeInfo);
				}
			}
			XElement xElement4 = xElement.Element("battle_item_settings");
			if (xElement4 != null)
			{
				base.Result.TaikyokuItemSetting.Parse(xElement4);
			}
		}
		catch (NeedNotNodeNotFoundException)
		{
		}
	}
}
