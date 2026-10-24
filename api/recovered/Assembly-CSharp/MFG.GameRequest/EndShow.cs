using System.Threading;
using System.Xml.Linq;
using Cysharp.Threading.Tasks;
using MFG.GameData;
using MFG.Sytem.Utilities.Xml;

namespace MFG.GameRequest;

public class EndShow : RequestApiBase<EndShow, EndShow.ResultData, (int takuID, int playerIndex, int playerID, int voltage, int contributePercent, int bonus)>
{
	public class ResultData : ResultBase
	{
		public ShowResultInfo ShowResultInfo = new ShowResultInfo();
	}

	protected override void OnCreateSendParameter()
	{
		int item = base.SendParameter.takuID;
		int item2 = base.SendParameter.playerIndex;
		int item3 = base.SendParameter.playerID;
		int item4 = base.SendParameter.voltage;
		int item5 = base.SendParameter.contributePercent;
		int item6 = base.SendParameter.bonus;
		m_sendUrl = base.DefaultFrontServerUrl + "/end_show";
		m_postStr = $"web_id={base.WebID}&area_id={base.AreaID}&pcuid={base.AppSessionID}&tid={item}&pindex={item2}&mid={item3}&version={base.AppVersion}&voltage={item4}&contribute_percent={item5}&bonus={item6}";
	}

	protected override async UniTask OnParse(XElement baseElem, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		await base.OnParse(baseElem, cancellationToken);
		XElement showResultElem = baseElem.MustNeedElement("showresult");
		base.Result.ShowResultInfo.Parse(showResultElem);
	}
}
