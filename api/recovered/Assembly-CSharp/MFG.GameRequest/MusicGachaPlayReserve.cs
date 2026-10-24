using System.Threading;
using System.Xml.Linq;
using Cysharp.Threading.Tasks;
using MFG.Sytem.Utilities.Xml;

namespace MFG.GameRequest;

public class MusicGachaPlayReserve : RequestApiBase<MusicGachaPlayReserve, MusicGachaPlayReserve.ResultData, int>
{
	public class ResultData : ResultBase
	{
		public bool IsSuccess;

		public ulong RequestID;
	}

	protected override void OnCreateSendParameter()
	{
		m_sendUrl = base.DefaultFrontServerUrl + "/music_gacha_play_reserve";
		m_postStr = $"web_id={base.WebID}&area_id={base.AreaID}&pcuid={base.AppSessionID}&gacha_id={base.SendParameter}&api_ver=2";
	}

	protected override async UniTask OnParse(XElement baseElem, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		await base.OnParse(baseElem, cancellationToken);
		try
		{
			XElement xElement = baseElem.Element("gacha_reserve");
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
