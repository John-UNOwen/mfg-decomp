using System.Threading;
using System.Xml.Linq;
using Cysharp.Threading.Tasks;
using MFG.Sytem.Utilities.Xml;

namespace MFG.GameRequest;

public class CutinGachaPlayApply : RequestApiBase<CutinGachaPlayApply, CutinGachaPlayApply.ResultData, ulong>
{
	public class ResultData : ResultBase
	{
		public bool IsSuccess;

		public ulong RequestID;
	}

	protected override void OnCreateSendParameter()
	{
		m_sendUrl = base.DefaultFrontServerUrl + "/cutin_gacha_play_apply";
		m_postStr = $"web_id={base.WebID}&area_id={base.AreaID}&pcuid={base.AppSessionID}&request_id={base.SendParameter}";
	}

	protected override async UniTask OnParse(XElement baseElem, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		await base.OnParse(baseElem, cancellationToken);
		try
		{
			XElement xElement = baseElem.Element("gacha_apply");
			if (xElement != null)
			{
				base.Result.IsSuccess = xElement.Element("is_success").ValueBool();
				base.Result.RequestID = xElement.Element("request_id").ValueUInt64();
			}
		}
		catch (NeedNotNodeNotFoundException)
		{
		}
	}
}
