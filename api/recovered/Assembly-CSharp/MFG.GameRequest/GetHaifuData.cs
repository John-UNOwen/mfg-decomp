using System;
using System.Threading;
using System.Xml.Linq;
using Cysharp.Threading.Tasks;
using MFG.Taikyoku;

namespace MFG.GameRequest;

public class GetHaifuData : RequestApiBase<GetHaifuData, GetHaifuData.ResultData, int>
{
	[Serializable]
	public class ResultData : ResultBase
	{
		public HaifuData Haifu = new HaifuData();
	}

	protected override void OnCreateSendParameter()
	{
		int sendParameter = base.SendParameter;
		m_sendUrl = base.DefaultFrontServerUrl + "/get_haifu_data";
		m_postStr = $"version={base.AppVersion}&pcuid={base.AppSessionID}&web_id={base.WebID}&area_id={base.AreaID}&tid={sendParameter}";
	}

	protected override async UniTask OnParse(XElement baseElem, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		await base.OnParse(baseElem, cancellationToken);
		base.Result.Haifu.Parse(baseElem);
	}
}
