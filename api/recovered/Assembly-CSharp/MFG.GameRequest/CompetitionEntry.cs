using System.Threading;
using System.Xml.Linq;
using Cysharp.Threading.Tasks;
using MFG.Sytem.Utilities.Xml;
using MFG.Types.Competition;

namespace MFG.GameRequest;

public class CompetitionEntry : RequestApiBase<CompetitionEntry, CompetitionEntry.ResultData, CompetitionEntry.Param>
{
	public class Param
	{
		public CompetitionID CompetitionID;

		public bool IsEntryRequest;
	}

	public class ResultData : ResultBase
	{
		public bool IsSuccess;

		public string Reason = "";

		public bool IsEntried;

		public bool IsFinalResultDisplayed;
	}

	protected override void OnCreateSendParameter()
	{
		int playerID = GameDataManager.Account.PlayerID;
		int competitionID = (int)base.SendParameter.CompetitionID;
		string text = (base.SendParameter.IsEntryRequest ? "" : "&displayed_result=1");
		m_sendUrl = base.DefaultFrontServerUrl + "/competition_entry";
		m_postStr = $"web_id={base.WebID}&area_id={base.AreaID}&pcuid={base.AppSessionID}&mid={playerID}&compe_id={competitionID}{text}";
	}

	protected override async UniTask OnParse(XElement baseElem, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		await base.OnParse(baseElem, cancellationToken);
		try
		{
			XElement xElement = baseElem.Element("entry");
			if (xElement != null)
			{
				base.Result.IsSuccess = xElement.Element("result").ValueBool();
				base.Result.IsEntried = xElement.Element("is_entry").ValueBool();
				XElement xElement2 = xElement.Element("reason");
				if (xElement2 != null)
				{
					base.Result.Reason = (base.Result.Reason = xElement2.Value);
				}
			}
			XElement xElement3 = baseElem.Element("displayed_result");
			if (xElement3 != null)
			{
				base.Result.IsSuccess = xElement3.Element("result").ValueBool();
				base.Result.IsFinalResultDisplayed = xElement3.Element("is_check_result").ValueBool();
			}
		}
		catch (NeedNotNodeNotFoundException)
		{
		}
	}
}
