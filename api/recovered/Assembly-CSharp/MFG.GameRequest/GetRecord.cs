using System.Collections.Generic;
using System.Threading;
using System.Xml.Linq;
using Cysharp.Threading.Tasks;
using MFG.GameData;
using MFG.Sytem.Utilities.Xml;

namespace MFG.GameRequest;

public class GetRecord : RequestApiBase<GetRecord, GetRecord.ResultData>
{
	public class ResultData : ResultBase
	{
		public PlayerTotalRecordInfo ResultPlayerTotalRecordInfo = new PlayerTotalRecordInfo();

		public List<PlayerRecordInfo> ResultPlayerRecordInfo = new List<PlayerRecordInfo>();
	}

	protected override void OnCreateSendParameter()
	{
		m_sendUrl = base.DefaultFrontServerUrl + "/player_record";
		m_postStr = $"web_id={base.WebID}&area_id={base.AreaID}&pcuid={base.AppSessionID}";
	}

	protected override async UniTask OnParse(XElement baseElem, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		await base.OnParse(baseElem, cancellationToken);
		try
		{
			XElement elem = baseElem.NeedNotElement("player_record");
			PlayerRecordGameData playerRecordGameData = new PlayerRecordGameData();
			playerRecordGameData.Parse(elem);
			base.Result.ResultPlayerTotalRecordInfo = playerRecordGameData.PlayerTotalRecordInfo;
			base.Result.ResultPlayerRecordInfo = playerRecordGameData.PlayerRecordInfos;
		}
		catch (NeedNotNodeNotFoundException)
		{
		}
	}
}
