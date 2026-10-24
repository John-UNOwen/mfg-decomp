using System.Collections.Generic;
using System.Threading;
using System.Xml.Linq;
using Cysharp.Threading.Tasks;
using MFG.GameData;
using MFG.Sytem.Utilities.Xml;

namespace MFG.GameRequest;

public class GachaInfoRequest : RequestApiBase<GachaInfoRequest, GachaInfoRequest.ResultData>
{
	public class ResultData : ResultBase
	{
		public List<GachaSeriesServerInfo> ResultGachaSeriesInfosFromServer = new List<GachaSeriesServerInfo>();
	}

	protected override void OnCreateSendParameter()
	{
		m_sendUrl = base.DefaultFrontServerUrl + "/gacha_info";
		m_postStr = $"web_id={base.WebID}&area_id={base.AreaID}&pcuid={base.AppSessionID}&api_ver=2";
	}

	protected override async UniTask OnParse(XElement baseElem, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		await base.OnParse(baseElem, cancellationToken);
		try
		{
			XElement elem = baseElem.NeedNotElement("gacha_schedule");
			GachaInfoGameData gachaInfoGameData = new GachaInfoGameData();
			gachaInfoGameData.Parse(elem);
			base.Result.ResultGachaSeriesInfosFromServer = gachaInfoGameData.GachaSeriesInfosFromServer;
		}
		catch (NeedNotNodeNotFoundException)
		{
		}
	}
}
