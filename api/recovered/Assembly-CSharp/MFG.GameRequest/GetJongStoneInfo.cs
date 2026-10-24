using System.Threading;
using System.Xml.Linq;
using Cysharp.Threading.Tasks;
using MFG.Sytem.Utilities.Xml;

namespace MFG.GameRequest;

public class GetJongStoneInfo : RequestApiBase<GetJongStoneInfo, GetJongStoneInfo.ResultData>
{
	public class ResultData : ResultBase
	{
		public int JongStoneFree;

		public int JongStonePaid;
	}

	protected override void OnCreateSendParameter()
	{
		m_sendUrl = base.DefaultFrontServerUrl + "/get_jongstone_info";
		m_postStr = $"version={base.AppVersion}&pcuid={base.AppSessionID}&web_id={base.WebID}&area_id={base.AreaID}";
	}

	protected override async UniTask OnParse(XElement baseElem, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		await base.OnParse(baseElem, cancellationToken);
		XElement xElement = baseElem.Element("jongstone_info");
		base.Result.JongStoneFree = xElement.Element("free_point").ValueInt();
		base.Result.JongStonePaid = xElement.Element("record_point").ValueInt();
	}
}
