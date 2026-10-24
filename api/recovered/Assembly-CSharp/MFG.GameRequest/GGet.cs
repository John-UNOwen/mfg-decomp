using System.Threading;
using System.Xml.Linq;
using Cysharp.Threading.Tasks;
using MFG.Sytem.Utilities.Xml;
using MFG.Taikyoku;

namespace MFG.GameRequest;

public class GGet : RequestApiBase<GGet, GGet.ResultData, (int TakuID, int PlayerIndex, int PlayerID, bool isReady, int nextSeqNo)>
{
	public class ResultData : ResultBase
	{
		public bool IsReadyToKyokuStartAll;

		public MatchingData Matching = new MatchingData();

		public TaikyokuData Taikyoku = new TaikyokuData();

		public StampData Stamp = new StampData();
	}

	protected override void OnCreateSendParameter()
	{
		int item = base.SendParameter.TakuID;
		int item2 = base.SendParameter.PlayerIndex;
		int item3 = base.SendParameter.PlayerID;
		int num = (base.SendParameter.isReady ? 1 : 0);
		int item4 = base.SendParameter.nextSeqNo;
		m_sendUrl = base.DefaultFrontServerUrl + "/gget";
		m_postStr = $"web_id={base.WebID}&area_id={base.AreaID}&pcuid={base.AppSessionID}&gserv_id={base.GameServiceID}&ready={num}&must={base.AppVersion}/{base.AppSessionID}/{item}/{item2}/{item3}/{item4}";
		m_serverLog = $"{item}/{item2}/{item3}/{item4}";
	}

	protected override async UniTask OnParse(XElement baseElem, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		await base.OnParse(baseElem, cancellationToken);
		XElement xElement = baseElem.Element("game");
		if (xElement != null)
		{
			try
			{
				XElement xElement2 = xElement.Element("all_ready");
				if (xElement2 != null)
				{
					base.Result.IsReadyToKyokuStartAll = xElement2.ValueInt() != 0;
				}
				else
				{
					base.Result.IsReadyToKyokuStartAll = true;
				}
				XElement xElement3 = xElement.NeedNotElement("mwait");
				if (xElement3 != null)
				{
					base.Result.Matching.Parse(xElement3);
				}
			}
			catch (NeedNotNodeNotFoundException)
			{
			}
			try
			{
				XElement xElement4 = xElement.NeedNotElement("taikyoku");
				if (xElement4 != null)
				{
					base.Result.Taikyoku.Parse(xElement4);
				}
			}
			catch (NeedNotNodeNotFoundException)
			{
			}
		}
		XElement xElement5 = baseElem.Element("chat");
		if (xElement5 != null)
		{
			base.Result.Stamp.Parse(xElement5);
		}
	}
}
