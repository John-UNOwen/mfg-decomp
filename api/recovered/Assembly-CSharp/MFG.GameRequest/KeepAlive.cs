using System.Threading;
using System.Xml.Linq;
using Cysharp.Threading.Tasks;

namespace MFG.GameRequest;

public class KeepAlive : RequestApiBase<KeepAlive, KeepAlive.ResultData>
{
	public class ResultData : ResultBase
	{
	}

	protected override void OnCreateSendParameter()
	{
		m_sendUrl = base.DefaultFrontServerUrl + "/keep_alive";
		m_postStr = $"web_id={base.WebID}&area_id={base.AreaID}&pcuid={base.AppSessionID}";
	}

	protected override async UniTask OnParse(XElement baseElem, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		await base.OnParse(baseElem, cancellationToken);
	}
}
