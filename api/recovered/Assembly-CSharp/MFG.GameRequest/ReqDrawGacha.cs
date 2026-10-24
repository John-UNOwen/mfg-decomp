using System.Threading;
using System.Xml.Linq;
using Cysharp.Threading.Tasks;
using MFG.Sytem.Utilities.Xml;

namespace MFG.GameRequest;

public class ReqDrawGacha : RequestApiBase<ReqDrawGacha, ReqDrawGacha.ResultData, (string gachaName, int gachaTicketCount)>
{
	public class ResultData : ResultBase
	{
		public string TransactionID;
	}

	protected override void OnCreateSendParameter()
	{
		_ = GameDataManager.Account.PlayerID;
		string item = base.SendParameter.gachaName;
		int item2 = base.SendParameter.gachaTicketCount;
		int num = 1;
		m_sendUrl = base.DefaultFrontServerUrl + "/req_draw_gacha";
		m_postStr = $"web_id={base.WebID}&area_id={base.AreaID}&pcuid={base.AppSessionID}&gacha_name={item}&times={item2}&jongstone={num}&use_ticket=false";
	}

	protected override async UniTask OnParse(XElement baseElem, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		await base.OnParse(baseElem, cancellationToken);
		try
		{
			XElement xElement = baseElem.Element("transaction_info");
			if (xElement != null)
			{
				base.Result.TransactionID = xElement.Element("transaction_id")?.Value;
			}
		}
		catch (NeedNotNodeNotFoundException)
		{
		}
	}
}
