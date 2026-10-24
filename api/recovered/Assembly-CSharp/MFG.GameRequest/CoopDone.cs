using System.Collections.Generic;
using System.Threading;
using System.Xml.Linq;
using Cysharp.Threading.Tasks;

namespace MFG.GameRequest;

public class CoopDone : RequestApiBase<CoopDone, CoopDone.ResultData, List<int>>
{
	public class ResultData : ResultBase
	{
	}

	protected override void OnCreateSendParameter()
	{
		string text = string.Join(",", base.SendParameter);
		m_sendUrl = base.DefaultFrontServerUrl + "/eashop_done";
		m_postStr = $"web_id={base.WebID}&area_id={base.AreaID}&pcuid={base.AppSessionID}&done_ids={text}";
	}

	protected override async UniTask OnParse(XElement baseElem, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		await base.OnParse(baseElem, cancellationToken);
	}
}
