using System.Collections.Generic;
using System.Threading;
using System.Xml.Linq;
using Cysharp.Threading.Tasks;
using MFG.GameData;
using MFG.Sytem.Utilities.Xml;

namespace MFG.GameRequest;

public class ReqGachaResult : RequestApiBase<ReqGachaResult, ReqGachaResult.ResultData, (string transactionID, int gachaTicketCount)>
{
	public class ResultData : ResultBase
	{
		public List<CutinItemInfo> CustomizeItems = new List<CutinItemInfo>();

		public int AcquiredGiftCount;

		public int PrevGiftCount;

		public int AfterGiftCount;
	}

	protected override void OnCreateSendParameter()
	{
		_ = GameDataManager.Account.PlayerID;
		string item = base.SendParameter.transactionID;
		int item2 = base.SendParameter.gachaTicketCount;
		bool flag = true;
		m_sendUrl = base.DefaultFrontServerUrl + "/get_gacha_result";
		m_postStr = $"web_id={base.WebID}&area_id={base.AreaID}&pcuid={base.AppSessionID}&transaction_id={item}&times={item2}&isUseTicket={flag}";
	}

	protected override async UniTask OnParse(XElement baseElem, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		await base.OnParse(baseElem, cancellationToken);
		try
		{
			XElement xElement = baseElem.Element("lottery_result");
			if (xElement == null)
			{
				return;
			}
			foreach (XElement item2 in xElement.Elements("data"))
			{
				_ = item2.Element("character_id").Value;
				_ = item2.Element("unique_id").Value;
				CutinItemInfo item = new CutinItemInfo();
				base.Result.CustomizeItems.Add(item);
			}
		}
		catch (NeedNotNodeNotFoundException)
		{
		}
	}
}
