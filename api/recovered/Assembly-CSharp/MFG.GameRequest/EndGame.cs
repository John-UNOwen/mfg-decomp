using System.Threading;
using System.Xml.Linq;
using Cysharp.Threading.Tasks;
using MFG.GameData;
using MFG.Sytem.Utilities.Xml;

namespace MFG.GameRequest;

public class EndGame : RequestApiBase<EndGame, EndGame.ResultData, (int TakuID, int PlayerIndex, int PlayerID)>
{
	public class ResultData : ResultBase
	{
		public MahjongResultInfo MahjongResultInfo = new MahjongResultInfo();
	}

	protected override void OnCreateSendParameter()
	{
		int item = base.SendParameter.TakuID;
		int item2 = base.SendParameter.PlayerIndex;
		int item3 = base.SendParameter.PlayerID;
		m_sendUrl = base.DefaultFrontServerUrl + "/end_game";
		m_postStr = $"web_id={base.WebID}&area_id={base.AreaID}&pcuid={base.AppSessionID}&tid={item}&pindex={item2}&mid={item3}&version={base.AppVersion}";
		m_serverLog = $"{item}/{item2}/{item3}";
	}

	protected override async UniTask OnParse(XElement baseElem, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		await base.OnParse(baseElem, cancellationToken);
		XElement mgResultElem = baseElem.MustNeedElement("mgresult");
		base.Result.MahjongResultInfo.Parse(mgResultElem);
	}
}
