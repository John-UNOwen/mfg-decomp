using System.Threading;
using System.Xml.Linq;
using Cysharp.Threading.Tasks;

namespace MFG.GameRequest;

public class Logout : RequestApiBase<Logout, Logout.ResultData>
{
	public class ResultData : ResultBase
	{
	}

	protected override void OnCreateSendParameter()
	{
		m_sendUrl = base.DefaultFrontServerUrl + "/logout";
		m_postStr = $"web_id={base.WebID}&area_id={base.AreaID}&pcuid={base.AppSessionID}";
		m_serverLog = base.AppSessionID ?? "";
	}

	protected override async UniTask OnParse(XElement baseElem, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		await base.OnParse(baseElem, cancellationToken);
	}
}
