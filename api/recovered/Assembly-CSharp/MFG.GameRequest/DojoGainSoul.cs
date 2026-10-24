using System.Threading;
using System.Xml.Linq;
using Cysharp.Threading.Tasks;
using MFG.Sytem.Utilities.Xml;

namespace MFG.GameRequest;

public class DojoGainSoul : RequestApiBase<DojoGainSoul, DojoGainSoul.ResultData, int>
{
	public class ResultData : ResultBase
	{
		public int SlotID;

		public int AcquiredCount;

		public DojoGetStatus.SlotInfo SlotInfo = new DojoGetStatus.SlotInfo();
	}

	protected override void OnCreateSendParameter()
	{
		int sendParameter = base.SendParameter;
		m_sendUrl = base.DefaultFrontServerUrl + "/dojo_gain_soul";
		m_postStr = $"web_id={base.WebID}&area_id={base.AreaID}&pcuid={base.AppSessionID}&slot_id={sendParameter}";
	}

	protected override async UniTask OnParse(XElement baseElem, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		await base.OnParse(baseElem, cancellationToken);
		try
		{
			XElement xElement = baseElem.Element("dojo");
			if (xElement != null)
			{
				base.Result.SlotID = xElement.Element("slot_id").ValueInt();
				base.Result.AcquiredCount = xElement.Element("get_nr").ValueInt();
				XElement slotElem = xElement.Element("slot");
				base.Result.SlotInfo.Parse(slotElem);
			}
		}
		catch (NeedNotNodeNotFoundException)
		{
		}
	}
}
