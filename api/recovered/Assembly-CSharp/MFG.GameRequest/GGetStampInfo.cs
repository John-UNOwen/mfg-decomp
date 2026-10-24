using System.Threading;
using System.Xml.Linq;
using Cysharp.Threading.Tasks;
using MFG.Sytem.Utilities.Xml;
using MFG.Taikyoku;
using MFG.Taikyoku.Command.Send;

namespace MFG.GameRequest;

public class GGetStampInfo : RequestApiBase<GGetStampInfo, GGetStampInfo.ResultData, (int TakuID, int PlayerIndex, int PlayerID, int nextSeqNo, StampCommand stamp)>
{
	public class ResultData : ResultBase
	{
		public StampData Stamp = new StampData();
	}

	protected override void OnCreateSendParameter()
	{
		int item = base.SendParameter.TakuID;
		int item2 = base.SendParameter.PlayerIndex;
		int item3 = base.SendParameter.PlayerID;
		int item4 = base.SendParameter.nextSeqNo;
		string text = "";
		StampCommand item5 = base.SendParameter.stamp;
		text = ((item5.m_commandType != 0) ? $"&stamp_info={item5.RoomID},{item5.SeqID},{item5.ObjectID},{item5.PlayerName}" : $"&stamp_info={item5.RoomID},{item5.SeqID}");
		m_sendUrl = base.DefaultFrontServerUrl + "/gget_stamp_info";
		m_postStr = $"web_id={base.WebID}&area_id={base.AreaID}&pcuid={base.AppSessionID}&gserv_id={base.GameServiceID}&must={base.AppVersion}/{base.AppSessionID}/{item}/{item2}/{item3}/{item4}{text}";
	}

	protected override async UniTask OnParse(XElement baseElem, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		await base.OnParse(baseElem, cancellationToken);
		try
		{
			XElement xElement = baseElem.NeedNotElement("stamp_info");
			if (xElement != null)
			{
				base.Result.Stamp.Parse(xElement);
			}
		}
		catch (NeedNotNodeNotFoundException)
		{
		}
	}
}
