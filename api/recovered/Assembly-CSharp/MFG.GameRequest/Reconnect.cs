using System.Threading;
using System.Xml.Linq;
using Cysharp.Threading.Tasks;
using MFG.Sytem.Utilities.Xml;
using MFG.Taikyoku;

namespace MFG.GameRequest;

public class Reconnect : RequestApiBase<Reconnect, Reconnect.ResultData>
{
	public class ResultData : ResultBase
	{
		public EntryTakuInfo TakuInfo = new EntryTakuInfo();
	}

	protected override void OnCreateSendParameter()
	{
		m_sendUrl = string.Format("{0}{1}.{2}{3}reconnect/{4}/{5}/{6}/", GameDataManager.System.DefaultGameServerUrlFront, base.GameServiceID.ToString("00"), GameDataManager.System.DefaultGameServerUrlTail, GameDataManager.Taikyoku.TakuInfo.GameServiceUrl, base.AppVersion, base.AppSessionID, base.WebID);
		m_postStr = "";
	}

	protected override async UniTask OnParse(XElement baseElem, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		await base.OnParse(baseElem, cancellationToken);
		XElement entryElement = baseElem.MustNeedElement("entry");
		base.Result.TakuInfo.Parse(entryElement);
	}
}
