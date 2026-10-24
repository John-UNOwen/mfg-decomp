using System.Threading;
using System.Xml.Linq;
using Cysharp.Threading.Tasks;
using MFG.Sytem.Utilities.Xml;

namespace MFG.GameRequest;

public class GetMg : RequestApiBase<GetMg, GetMg.ResultData>
{
	public class ResultData : ResultBase
	{
		public int Mg;

		public int AdditionalMg;
	}

	protected override void OnCreateSendParameter()
	{
	}

	protected override async UniTask OnParse(XElement baseElem, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		await base.OnParse(baseElem, cancellationToken);
		XElement xElement = baseElem.Element("mg_info");
		base.Result.Mg = xElement.Element("mg").ValueInt();
		base.Result.Mg = xElement.Element("additional_mg").ValueInt();
	}
}
